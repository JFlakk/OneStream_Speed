using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Core validation engine for the Data Validation Manager (DVM).
    ///
    /// Supports two validation contexts:
    ///   Table – reads rows from a SQL table or view and evaluates comparison
    ///           rules between named columns, e.g.:
    ///             row y column x  =   row a column y
    ///             row y column x  &lt;   row a column y  +  5%
    ///
    ///   Cube  – retrieves cell values via FDX queries and applies the same
    ///           comparison operators.
    /// </summary>
    public class DVM_Validation_Engine
    {
        // =====================================================================
        #region "Rule Type Constants"
        // =====================================================================

        public static class RuleType
        {
            public const string Equality             = "Equality";
            public const string NotEqual             = "NotEqual";
            public const string LessThan             = "LessThan";
            public const string LessThanOrEqual      = "LessThanOrEqual";
            public const string GreaterThan          = "GreaterThan";
            public const string GreaterThanOrEqual   = "GreaterThanOrEqual";
            /// <summary>|src – tgt| / |tgt|  &lt;=  Tolerance_Pct / 100</summary>
            public const string PercentVariance      = "PercentVariance";
            /// <summary>src  &lt;  tgt * (1 + Tolerance_Pct / 100)</summary>
            public const string LessThanWithPct      = "LessThanWithPct";
            /// <summary>src  &gt;  tgt * (1 – Tolerance_Pct / 100)</summary>
            public const string GreaterThanWithPct   = "GreaterThanWithPct";
        }

        public static class ResultStatus
        {
            public const string Pass    = "Pass";
            public const string Fail    = "Fail";
            public const string Warning = "Warning";
            public const string Error   = "Error";
        }

        public static class RunStatus
        {
            public const string InProgress = "InProgress";
            public const string Completed  = "Completed";
            public const string Failed     = "Failed";
            public const string Cancelled  = "Cancelled";
        }

        #endregion

        // =====================================================================
        #region "Run Validation (Table Context)"
        // =====================================================================

        /// <summary>
        /// Executes all active rules for a Table-context DVM_Config and persists
        /// the results.  Returns a summary DataTable with one row per rule.
        /// </summary>
        /// <param name="si">OneStream SessionInfo.</param>
        /// <param name="dvmConfigID">ID of the DVM_Config to run.</param>
        /// <param name="runUser">User name to record on the run.</param>
        /// <returns>DataTable containing the per-rule results for this run.</returns>
        public DataTable RunTableValidation(SessionInfo si, int dvmConfigID, string runUser)
        {
            var stopwatch = Stopwatch.StartNew();

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                var sqa = new SQA_DVM_Validation(si, conn);

                // Load config
                var configDt = sqa.Get_DVM_Config_By_ID(si, dvmConfigID);
                if (configDt.Rows.Count == 0)
                    throw new XFException(si, $"DVM_Config ID {dvmConfigID} not found.");

                var configRow  = configDt.Rows[0];
                var schemaName = configRow["Table_Schema"] == DBNull.Value ? "dbo" : configRow["Table_Schema"].ToString();
                var tableName  = configRow["Table_Name"].ToString();
                var tableFilter= configRow["Table_Filter"] == DBNull.Value ? null : configRow["Table_Filter"].ToString();

                if (string.IsNullOrWhiteSpace(tableName))
                    throw new XFException(si, $"DVM_Config ID {dvmConfigID} has no Table_Name configured.");

                // Insert an InProgress run record
                int runID = InsertRun(si, conn, dvmConfigID, runUser);

                var resultsDt  = BuildResultsDataTable();
                int totalPass  = 0;
                int totalFail  = 0;
                int totalWarn  = 0;

                try
                {
                    // Load the source table into memory once for efficiency
                    var sourceData = LoadSourceTable(si, conn, schemaName, tableName, tableFilter);

                    // Load active rules
                    var rulesDt = sqa.Get_Rules_By_Config(si, dvmConfigID);

                    foreach (DataRow ruleRow in rulesDt.Rows)
                    {
                        var result = EvaluateTableRule(si, sourceData, ruleRow, runID);
                        resultsDt.Rows.Add(result);

                        switch (result["Status"].ToString())
                        {
                            case ResultStatus.Pass:    totalPass++;    break;
                            case ResultStatus.Fail:    totalFail++;    break;
                            case ResultStatus.Warning: totalWarn++;    break;
                        }
                    }

                    // Bulk-insert results
                    if (resultsDt.Rows.Count > 0)
                        sqa.Insert_DVM_Results(si, resultsDt);

                    stopwatch.Stop();
                    FinaliseRun(si, conn, runID, RunStatus.Completed,
                                rulesDt.Rows.Count, totalPass, totalFail, totalWarn,
                                (int)stopwatch.ElapsedMilliseconds, null);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    FinaliseRun(si, conn, runID, RunStatus.Failed,
                                0, 0, 0, 0,
                                (int)stopwatch.ElapsedMilliseconds,
                                ex.Message);
                    throw ErrorHandler.LogWrite(si, new XFException(si, ex));
                }

                return resultsDt;
            }
        }

        // =====================================================================
        #endregion

        // =====================================================================
        #region "Run Validation (Cube Context)"
        // =====================================================================

        /// <summary>
        /// Executes all active rules for a Cube-context DVM_Config using FDX queries
        /// and persists the results.  Returns a summary DataTable with one row per rule.
        /// </summary>
        /// <param name="si">OneStream SessionInfo.</param>
        /// <param name="dvmConfigID">ID of the DVM_Config to run.</param>
        /// <param name="runUser">User name to record on the run.</param>
        /// <returns>DataTable containing the per-rule results for this run.</returns>
        public DataTable RunCubeValidation(SessionInfo si, int dvmConfigID, string runUser)
        {
            var stopwatch = Stopwatch.StartNew();

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                var sqa = new SQA_DVM_Validation(si, conn);

                // Load config
                var configDt = sqa.Get_DVM_Config_By_ID(si, dvmConfigID);
                if (configDt.Rows.Count == 0)
                    throw new XFException(si, $"DVM_Config ID {dvmConfigID} not found.");

                // Insert an InProgress run record
                int runID = InsertRun(si, conn, dvmConfigID, runUser);

                var resultsDt = BuildResultsDataTable();
                int totalPass = 0;
                int totalFail = 0;
                int totalWarn = 0;

                try
                {
                    // Load active rules
                    var rulesDt = sqa.Get_Rules_By_Config(si, dvmConfigID);

                    foreach (DataRow ruleRow in rulesDt.Rows)
                    {
                        var result = EvaluateCubeRule(si, ruleRow, runID);
                        resultsDt.Rows.Add(result);

                        switch (result["Status"].ToString())
                        {
                            case ResultStatus.Pass:    totalPass++;    break;
                            case ResultStatus.Fail:    totalFail++;    break;
                            case ResultStatus.Warning: totalWarn++;    break;
                        }
                    }

                    // Bulk-insert results
                    if (resultsDt.Rows.Count > 0)
                        sqa.Insert_DVM_Results(si, resultsDt);

                    stopwatch.Stop();
                    FinaliseRun(si, conn, runID, RunStatus.Completed,
                                rulesDt.Rows.Count, totalPass, totalFail, totalWarn,
                                (int)stopwatch.ElapsedMilliseconds, null);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    FinaliseRun(si, conn, runID, RunStatus.Failed,
                                0, 0, 0, 0,
                                (int)stopwatch.ElapsedMilliseconds,
                                ex.Message);
                    throw ErrorHandler.LogWrite(si, new XFException(si, ex));
                }

                return resultsDt;
            }
        }

        #endregion

        // =====================================================================
        #region "Table Rule Evaluation"
        // =====================================================================

        /// <summary>
        /// Evaluates a single Table-context rule against the pre-loaded source data.
        /// Source = "row y column x", Target = "row a column y" in problem-statement terms.
        /// </summary>
        private DataRow EvaluateTableRule(SessionInfo si, DataTable sourceData,
            DataRow ruleRow, int runID)
        {
            var ruleID      = Convert.ToInt32(ruleRow["DVM_Rule_ID"]);
            var ruleName    = ruleRow["Rule_Name"].ToString();
            var ruleType    = ruleRow["Rule_Type"].ToString();
            var severity    = ruleRow["Severity"].ToString();
            var srcFilter   = ruleRow["Src_Row_Filter"] == DBNull.Value ? null : ruleRow["Src_Row_Filter"].ToString();
            var srcColumn   = ruleRow["Src_Column"] == DBNull.Value ? null : ruleRow["Src_Column"].ToString();
            var tgtFilter   = ruleRow["Tgt_Row_Filter"] == DBNull.Value ? null : ruleRow["Tgt_Row_Filter"].ToString();
            var tgtColumn   = ruleRow["Tgt_Column"] == DBNull.Value ? null : ruleRow["Tgt_Column"].ToString();
            var tolerancePct= ruleRow["Tolerance_Pct"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(ruleRow["Tolerance_Pct"]);

            var resultsDt   = BuildResultsDataTable();
            var resultRow   = resultsDt.NewRow();
            resultRow["DVM_Run_ID"]  = runID;
            resultRow["DVM_Rule_ID"] = ruleID;
            resultRow["Rule_Name"]   = ruleName;
            resultRow["Expected_Operator"] = RuleTypeToOperatorSymbol(ruleType);
            if (tolerancePct.HasValue)
                resultRow["Tolerance_Pct"] = tolerancePct.Value;

            try
            {
                // Resolve source value
                decimal? srcValue = GetTableCellValue(si, sourceData, srcFilter, srcColumn);
                // Resolve target value
                decimal? tgtValue = GetTableCellValue(si, sourceData, tgtFilter, tgtColumn);

                resultRow["Src_Value"]   = srcValue.HasValue ? srcValue.Value.ToString(CultureInfo.InvariantCulture) : "(no match)";
                resultRow["Tgt_Value"]   = tgtValue.HasValue ? tgtValue.Value.ToString(CultureInfo.InvariantCulture) : "(no match)";
                resultRow["Row_Context"] = $"Src: [{srcFilter}] Col: [{srcColumn}] | Tgt: [{tgtFilter}] Col: [{tgtColumn}]";

                if (!srcValue.HasValue || !tgtValue.HasValue)
                {
                    resultRow["Status"]  = ResultStatus.Error;
                    resultRow["Message"] = "One or both row filters returned no matching rows.";
                    return resultRow;
                }

                bool passed = EvaluateComparison(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);

                if (passed)
                {
                    resultRow["Status"]  = ResultStatus.Pass;
                    resultRow["Message"] = BuildPassMessage(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);
                }
                else
                {
                    resultRow["Status"]  = severity.XFEqualsIgnoreCase("Warning") ? ResultStatus.Warning : ResultStatus.Fail;
                    resultRow["Message"] = BuildFailMessage(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);
                }
            }
            catch (Exception ex)
            {
                resultRow["Status"]  = ResultStatus.Error;
                resultRow["Message"] = ex.Message;
            }

            return resultRow;
        }

        /// <summary>
        /// Loads the configured table into a DataTable so all rules can share a single
        /// round-trip to the database.
        /// </summary>
        private DataTable LoadSourceTable(SessionInfo si, SqlConnection conn,
            string schemaName, string tableName, string tableFilter)
        {
            // Validate identifiers to prevent SQL injection
            if (!IsValidIdentifier(schemaName) || !IsValidIdentifier(tableName))
                throw new XFException(si, $"Invalid table identifier: [{schemaName}].[{tableName}]");

            var sql = $"SELECT * FROM [{schemaName}].[{tableName}]";
            if (!string.IsNullOrWhiteSpace(tableFilter))
                sql += $" WHERE {tableFilter}";

            var dt = new DataTable();
            using (var cmd = new SqlCommand(sql, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Resolves a single numeric cell value from a pre-loaded DataTable.
        /// The row is identified by applying the filter expression via DataTable.Select.
        /// Returns null when no matching row is found.
        /// </summary>
        private decimal? GetTableCellValue(SessionInfo si, DataTable dt,
            string rowFilter, string columnName)
        {
            if (string.IsNullOrWhiteSpace(rowFilter) || string.IsNullOrWhiteSpace(columnName))
                return null;

            var matchingRows = dt.Select(rowFilter);
            if (matchingRows.Length == 0)
                return null;

            // When multiple rows match, sum the column values (aggregate)
            decimal total = 0m;
            foreach (var row in matchingRows)
            {
                if (row[columnName] == DBNull.Value)
                    continue;
                total += Convert.ToDecimal(row[columnName]);
            }

            return total;
        }

        #endregion

        // =====================================================================
        #region "Cube Rule Evaluation"
        // =====================================================================

        /// <summary>
        /// Evaluates a single Cube-context rule by retrieving cell values via FDX queries.
        /// </summary>
        private DataRow EvaluateCubeRule(SessionInfo si, DataRow ruleRow, int runID)
        {
            var ruleID      = Convert.ToInt32(ruleRow["DVM_Rule_ID"]);
            var ruleName    = ruleRow["Rule_Name"].ToString();
            var ruleType    = ruleRow["Rule_Type"].ToString();
            var severity    = ruleRow["Severity"].ToString();
            var srcFdx      = ruleRow["Src_FDX"] == DBNull.Value ? null : ruleRow["Src_FDX"].ToString();
            var tgtFdx      = ruleRow["Tgt_FDX"] == DBNull.Value ? null : ruleRow["Tgt_FDX"].ToString();
            var tolerancePct= ruleRow["Tolerance_Pct"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(ruleRow["Tolerance_Pct"]);

            var resultsDt = BuildResultsDataTable();
            var resultRow = resultsDt.NewRow();
            resultRow["DVM_Run_ID"]  = runID;
            resultRow["DVM_Rule_ID"] = ruleID;
            resultRow["Rule_Name"]   = ruleName;
            resultRow["Expected_Operator"] = RuleTypeToOperatorSymbol(ruleType);
            if (tolerancePct.HasValue)
                resultRow["Tolerance_Pct"] = tolerancePct.Value;

            try
            {
                if (string.IsNullOrWhiteSpace(srcFdx) || string.IsNullOrWhiteSpace(tgtFdx))
                {
                    resultRow["Status"]  = ResultStatus.Error;
                    resultRow["Message"] = "Src_FDX and Tgt_FDX must both be supplied for Cube-context rules.";
                    return resultRow;
                }

                decimal? srcValue = GetCubeCellValue(si, srcFdx);
                decimal? tgtValue = GetCubeCellValue(si, tgtFdx);

                resultRow["Src_Value"]   = srcValue.HasValue ? srcValue.Value.ToString(CultureInfo.InvariantCulture) : "(null)";
                resultRow["Tgt_Value"]   = tgtValue.HasValue ? tgtValue.Value.ToString(CultureInfo.InvariantCulture) : "(null)";
                resultRow["Row_Context"] = $"Src FDX: {srcFdx} | Tgt FDX: {tgtFdx}";

                if (!srcValue.HasValue || !tgtValue.HasValue)
                {
                    resultRow["Status"]  = ResultStatus.Error;
                    resultRow["Message"] = "One or both FDX expressions returned no value.";
                    return resultRow;
                }

                bool passed = EvaluateComparison(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);

                if (passed)
                {
                    resultRow["Status"]  = ResultStatus.Pass;
                    resultRow["Message"] = BuildPassMessage(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);
                }
                else
                {
                    resultRow["Status"]  = severity.XFEqualsIgnoreCase("Warning") ? ResultStatus.Warning : ResultStatus.Fail;
                    resultRow["Message"] = BuildFailMessage(srcValue.Value, tgtValue.Value, ruleType, tolerancePct);
                }
            }
            catch (Exception ex)
            {
                resultRow["Status"]  = ResultStatus.Error;
                resultRow["Message"] = ex.Message;
            }

            return resultRow;
        }

        /// <summary>
        /// Retrieves a numeric cell value from the OneStream cube using an FDX query string.
        /// FDX format example: S#Actual:T#2024M1:E#TotalEntity:A#NetIncome
        /// </summary>
        private decimal? GetCubeCellValue(SessionInfo si, string fdxQuery)
        {
            if (string.IsNullOrWhiteSpace(fdxQuery))
                return null;

            // BRApi.Finance.Data.GetDataCell returns the cell value for the given FDX intersection.
            var cellData = BRApi.Finance.Data.GetDataCell(si, false, fdxQuery);
            if (cellData == null)
                return null;

            return Convert.ToDecimal(cellData.CellAmount);
        }

        #endregion

        // =====================================================================
        #region "Comparison Logic"
        // =====================================================================

        /// <summary>
        /// Applies the specified rule type comparison between source and target values.
        /// Returns true when the comparison passes (the rule is satisfied).
        /// </summary>
        private bool EvaluateComparison(decimal srcValue, decimal tgtValue,
            string ruleType, decimal? tolerancePct)
        {
            switch (ruleType)
            {
                case RuleType.Equality:
                    return srcValue == tgtValue;

                case RuleType.NotEqual:
                    return srcValue != tgtValue;

                case RuleType.LessThan:
                    return srcValue < tgtValue;

                case RuleType.LessThanOrEqual:
                    return srcValue <= tgtValue;

                case RuleType.GreaterThan:
                    return srcValue > tgtValue;

                case RuleType.GreaterThanOrEqual:
                    return srcValue >= tgtValue;

                case RuleType.PercentVariance:
                {
                    // |src – tgt| / |tgt|  <=  Tolerance_Pct / 100
                    if (tgtValue == 0m)
                        return srcValue == 0m;  // both zero = pass; src non-zero vs zero tgt = fail

                    var pct = tolerancePct.GetValueOrDefault(0m);
                    var variance = Math.Abs(srcValue - tgtValue) / Math.Abs(tgtValue) * 100m;
                    return variance <= pct;
                }

                case RuleType.LessThanWithPct:
                {
                    // src  <  tgt * (1 + Tolerance_Pct / 100)
                    // Covers: "row y column x  <  row a column y  +  x%"
                    var pct = tolerancePct.GetValueOrDefault(0m);
                    return srcValue < tgtValue * (1m + pct / 100m);
                }

                case RuleType.GreaterThanWithPct:
                {
                    // src  >  tgt * (1 – Tolerance_Pct / 100)
                    // Tolerance_Pct must be < 100; otherwise the multiplier is <= 0 and the
                    // comparison becomes meaningless (every src > a negative number).
                    var pct = tolerancePct.GetValueOrDefault(0m);
                    if (pct >= 100m)
                        throw new XFException(
                            $"Tolerance_Pct ({pct}) must be less than 100 for GreaterThanWithPct rules. " +
                            "A tolerance of 100% or more makes the comparison trivially true against any positive target.");
                    return srcValue > tgtValue * (1m - pct / 100m);
                }

                default:
                    throw new XFException($"Unknown rule type: {ruleType}");
            }
        }

        #endregion

        // =====================================================================
        #region "Run Management Helpers"
        // =====================================================================

        private int InsertRun(SessionInfo si, SqlConnection conn,
            int dvmConfigID, string runUser)
        {
            var sql = @"
                INSERT INTO DVM_Run (DVM_Config_ID, Run_Date, Run_User, Status)
                VALUES (@DVM_Config_ID, GETDATE(), @Run_User, 'InProgress');
                SELECT SCOPE_IDENTITY();";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@DVM_Config_ID", SqlDbType.Int)         { Value = dvmConfigID });
                cmd.Parameters.Add(new SqlParameter("@Run_User",       SqlDbType.NVarChar, 50){ Value = runUser ?? si.UserName });
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        private void FinaliseRun(SessionInfo si, SqlConnection conn, int runID,
            string status, int totalRules, int totalPass, int totalFail, int totalWarn,
            int elapsedMs, string errorMessage)
        {
            var sql = @"
                UPDATE DVM_Run
                SET Status            = @Status,
                    Total_Rules       = @Total_Rules,
                    Total_Pass        = @Total_Pass,
                    Total_Fail        = @Total_Fail,
                    Total_Warning     = @Total_Warning,
                    Execution_Time_Ms = @Execution_Time_Ms,
                    Error_Message     = @Error_Message
                WHERE DVM_Run_ID = @DVM_Run_ID";

            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add(new SqlParameter("@Status",            SqlDbType.NVarChar, 20) { Value = status });
                cmd.Parameters.Add(new SqlParameter("@Total_Rules",       SqlDbType.Int)          { Value = totalRules });
                cmd.Parameters.Add(new SqlParameter("@Total_Pass",        SqlDbType.Int)          { Value = totalPass });
                cmd.Parameters.Add(new SqlParameter("@Total_Fail",        SqlDbType.Int)          { Value = totalFail });
                cmd.Parameters.Add(new SqlParameter("@Total_Warning",     SqlDbType.Int)          { Value = totalWarn });
                cmd.Parameters.Add(new SqlParameter("@Execution_Time_Ms", SqlDbType.Int)          { Value = elapsedMs });
                cmd.Parameters.Add(new SqlParameter("@Error_Message",     SqlDbType.NVarChar)     { Value = (object)errorMessage ?? DBNull.Value });
                cmd.Parameters.Add(new SqlParameter("@DVM_Run_ID",        SqlDbType.Int)          { Value = runID });
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        // =====================================================================
        #region "Result DataTable Schema"
        // =====================================================================

        /// <summary>
        /// Creates an empty DataTable matching the DVM_Result schema (without the
        /// IDENTITY column so it can be used with SqlBulkCopy).
        /// </summary>
        private DataTable BuildResultsDataTable()
        {
            var dt = new DataTable("DVM_Result");
            dt.Columns.Add("DVM_Run_ID",         typeof(int));
            dt.Columns.Add("DVM_Rule_ID",         typeof(int));
            dt.Columns.Add("Rule_Name",           typeof(string));
            dt.Columns.Add("Status",              typeof(string));
            dt.Columns.Add("Src_Value",           typeof(string));
            dt.Columns.Add("Tgt_Value",           typeof(string));
            dt.Columns.Add("Expected_Operator",   typeof(string));
            dt.Columns.Add("Tolerance_Pct",       typeof(decimal));
            dt.Columns.Add("Message",             typeof(string));
            dt.Columns.Add("Row_Context",         typeof(string));
            return dt;
        }

        #endregion

        // =====================================================================
        #region "Message Helpers"
        // =====================================================================

        private string BuildPassMessage(decimal src, decimal tgt, string ruleType, decimal? tolerancePct)
        {
            var op = RuleTypeToOperatorSymbol(ruleType);
            if (ruleType == RuleType.PercentVariance || ruleType == RuleType.LessThanWithPct || ruleType == RuleType.GreaterThanWithPct)
                return $"Pass: {src} {op} {tgt} (tolerance {tolerancePct?.ToString("F2") ?? "0"}%)";

            return $"Pass: {src} {op} {tgt}";
        }

        private string BuildFailMessage(decimal src, decimal tgt, string ruleType, decimal? tolerancePct)
        {
            var op = RuleTypeToOperatorSymbol(ruleType);
            if (ruleType == RuleType.PercentVariance || ruleType == RuleType.LessThanWithPct || ruleType == RuleType.GreaterThanWithPct)
                return $"Fail: {src} {op} {tgt} (tolerance {tolerancePct?.ToString("F2") ?? "0"}%)";

            return $"Fail: {src} {op} {tgt}";
        }

        private string RuleTypeToOperatorSymbol(string ruleType)
        {
            switch (ruleType)
            {
                case RuleType.Equality:           return "=";
                case RuleType.NotEqual:           return "!=";
                case RuleType.LessThan:           return "<";
                case RuleType.LessThanOrEqual:    return "<=";
                case RuleType.GreaterThan:        return ">";
                case RuleType.GreaterThanOrEqual: return ">=";
                case RuleType.PercentVariance:    return "~%";
                case RuleType.LessThanWithPct:    return "<%";
                case RuleType.GreaterThanWithPct: return ">%";
                default:                          return ruleType;
            }
        }

        #endregion

        // =====================================================================
        #region "Security Helper"
        // =====================================================================

        /// <summary>
        /// Validates that an identifier (schema or table name) contains only safe characters
        /// to prevent SQL injection when used in bracket-quoted identifiers.
        /// </summary>
        private bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;

            // Allow letters, digits, underscore, space, and hash (#) — common for temp tables.
            // Reject closing bracket which would break the bracket-quoting.
            foreach (char c in identifier)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != ' ' && c != '#')
                    return false;
            }

            return true;
        }

        #endregion
    }
}
