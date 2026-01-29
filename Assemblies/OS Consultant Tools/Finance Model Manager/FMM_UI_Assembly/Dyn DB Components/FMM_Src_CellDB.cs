using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    public class FMM_Src_CellDB
    {
        // Database table that will contain our objects
        public string TableName { get; } = "FMM_SrcCell";

        // SessionInfo instance
        public SessionInfo si { get; }

        // constructor
        public FMM_Src_CellDB(SessionInfo si)
        {
            this.si = si;
        }

        /// <summary>
        /// Gets the list of column names that are necessary for a given CalcType
        /// by using the FMM_Config_Helpers.SrcRegistry
        /// </summary>
        /// <param name="calcType">The calculation type</param>
        /// <returns>List of database column names to include in queries</returns>
        public List<string> GetRequiredColumnsByCalcType(int calcType)
        {
            var columns = new List<string>();

            // Always include core identification and common fields
            columns.AddRange(new[] {
                "CellID", "CubeID", "ActID", "ModelID", "CalcID",
                "Type"
            });

            var srcConfig = FMM_Config_Helpers.Get_SrcConfigType(calcType);

            if (srcConfig != null && srcConfig.ParameterMappings != null)
            {
                foreach (var mapping in srcConfig.ParameterMappings.Values)
                {
                    foreach (var dbColumn in mapping.Values)
                    {
                        // Map the logical column name to the actual database column name
                        if (!columns.Contains(dbColumn) && !string.IsNullOrEmpty(dbColumn))
                        {
                            columns.Add(dbColumn);
                        }
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// Builds a SELECT statement with only the necessary columns for the given CalcType
        /// </summary>
        /// <param name="calcType">The calculation type</param>
        /// <returns>Comma-separated list of columns for SELECT statement</returns>
        public string GetSelectColumnsForCalcType(int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            return string.Join(", ", columns);
        }



        /// <summary>
        /// Retrieve a single FMM_Src_CellModel by CellID
        /// </summary>
        public FMM_Src_CellModel GetSrcCell(int cellId, int calcType)
        {
            try
            {
                var columns = GetSelectColumnsForCalcType(calcType);
                string sql = $"SELECT {columns} FROM {this.TableName} WHERE CellID = @cellID";

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@cellID", cellId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    if (dt.Rows.Count < 1) return null;

                    DataRow dr = dt.Rows[0];
                    return MapDataRowToModel(dr, calcType);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieve all FMM_Src_CellModel objects for a given CalcID
        /// </summary>
        public List<FMM_Src_CellModel> GetSrcCellsByCalcId(int calcId, int calcType)
        {
            try
            {
				calcId = string.IsNullOrWhiteSpace(calcId.XFToString()) ? 0 : calcId;
                var columns = GetSelectColumnsForCalcType(calcType);
                string sql = $"SELECT {columns} FROM {this.TableName} WHERE CalcID = @calcID";
				BRApi.ErrorLog.LogMessage(si,$"Hit {sql}");

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@calcID", calcId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    List<FMM_Src_CellModel> result = new List<FMM_Src_CellModel>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(MapDataRowToModel(dr, calcType));
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
        /// <summary>
        /// Merge a collection of source cells using the generic SQA process
        /// </summary>
        public void Merge(List<FMM_Src_CellModel> models, int calcType)
        {
            if (models == null || models.Count == 0) return;

            try
            {
                var mergeTable = BuildMergeTable(models, calcType);

                var calcId = models[0].CalcID;
                var selectColumns = GetSelectColumnsForCalcType(calcType);
                var currentSql = $"SELECT {selectColumns} FROM {this.TableName} WHERE CalcID = @calcID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(this.si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(this.si, connection);
                    var sqa = new SqlDataAdapter();
                    var currentTable = new DataTable();

                    var sqlparams = new[]
                    {
                        new SqlParameter("@calcID", SqlDbType.Int) { Value = calcId }
                    };

                    cmdBuilder.FillDataTable(this.si, sqa, currentTable, currentSql, sqlparams);

                    // Set primary key if CellID column exists in the result set
                    if (currentTable.Columns.Contains("CellID") && currentTable.Columns["CellID"] != null)
                    {
                        currentTable.PrimaryKey = new[] { currentTable.Columns["CellID"]! };
                    }

                    // Merge the new/updated records with existing data
                    // preserveChanges=false: Overwrite existing rows with new values (upsert behavior)
                    currentTable.Merge(mergeTable, false, MissingSchemaAction.Add);

                    cmdBuilder.UpdateTableSimple(this.si, this.TableName, currentTable, sqa, "CellID");
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Merge a single source cell using the generic SQA process
        /// </summary>
        public void Merge(FMM_Src_CellModel model, int calcType)
        {
            this.Merge(new List<FMM_Src_CellModel> { model }, calcType);
        }

        private DataTable BuildMergeTable(IEnumerable<FMM_Src_CellModel> models, int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            DataTable dt = new DataTable(this.TableName);

            foreach (var col in columns)
            {
                dt.Columns.Add(col, typeof(object));
            }

            foreach (var model in models)
            {
                DataRow row = dt.NewRow();

                foreach (var columnName in columns)
                {
                    var prop =
                        model.GetType().GetProperty(columnName,
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.IgnoreCase)
                        ?? model.GetType().GetProperty(columnName.Replace("_", string.Empty),
                            System.Reflection.BindingFlags.Instance |
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.IgnoreCase);

                    if (prop == null) continue;

                    var value = prop.GetValue(model);
                    SetColumnValue(row, columnName, value);
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private void SetColumnValue(DataRow row, string columnName, object value)
        {
            if (!row.Table.Columns.Contains(columnName)) return;
            row[columnName] = value ?? DBNull.Value;
        }
        /// <summary>
        /// Retrieve all FMM_Src_CellModel objects
        /// Note: Since CalcType may vary, this returns all columns
        /// </summary>
        public List<FMM_Src_CellModel> GetAllSrcCells()
        {
            try
            {
                string sql = $"SELECT * FROM {this.TableName} ORDER BY CalcID, SrcOrder";

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, false);
                    List<FMM_Src_CellModel> result = new List<FMM_Src_CellModel>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(MapDataRowToModel(dr));
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Maps a DataRow to FMM_Src_CellModel, safely handling null values
        /// Overload without calcType - uses default parameterless constructor
        /// </summary>
        private FMM_Src_CellModel MapDataRowToModel(DataRow dr)
        {
            return MapDataRowToModel(dr, 0); // 0 = CalcType.None, will use empty enabled properties list
        }

        /// <summary>
        /// Maps a DataRow to FMM_Src_CellModel, safely handling null values
        /// </summary>
        private FMM_Src_CellModel MapDataRowToModel(DataRow dr, int calcType)
        {
            // Get enabled properties from configuration
            var enabledProperties = FMM_Config_Helpers.GetEnabledSrcProperties(calcType);

            var model = new FMM_Src_CellModel(enabledProperties)
            {
                CubeID = dr.Field<int>("CubeID"),
                ActID = dr.Field<int>("ActID"),
                ModelID = dr.Field<int>("ModelID"),
                CalcID = dr.Field<int>("CalcID"),
                CellID = dr.Field<int>("CellID"),
                Order = dr.Field<int?>("Order") ?? 0,
                Type = dr.Field<string>("Type") ?? string.Empty,
                ItemType = dr.Field<string>("ItemType") ?? string.Empty
            };

            // Set dimension fields only if they exist in the column set and are enabled
            if (enabledProperties.Contains("Entity") && dr.Table.Columns.Contains("Entity"))
                model.Entity = dr.Field<string>("Entity");
            if (enabledProperties.Contains("Cons") && dr.Table.Columns.Contains("Cons"))
                model.Cons = dr.Field<string>("Cons");
            if (enabledProperties.Contains("Scenario") && dr.Table.Columns.Contains("Scenario"))
                model.Scenario = dr.Field<string>("Scenario");
            if (enabledProperties.Contains("Time") && dr.Table.Columns.Contains("Time"))
                model.Time = dr.Field<string>("Time");
            if (enabledProperties.Contains("View") && dr.Table.Columns.Contains("View"))
                model.View = dr.Field<string>("View");
            if (enabledProperties.Contains("Acct") && dr.Table.Columns.Contains("Acct"))
                model.Acct = dr.Field<string>("Acct");
            if (enabledProperties.Contains("IC") && dr.Table.Columns.Contains("IC"))
                model.IC = dr.Field<string>("IC");
            if (enabledProperties.Contains("Origin") && dr.Table.Columns.Contains("Origin"))
                model.Origin = dr.Field<string>("Origin");
            if (enabledProperties.Contains("Flow") && dr.Table.Columns.Contains("Flow"))
                model.Flow = dr.Field<string>("Flow");
            if (enabledProperties.Contains("UD1") && dr.Table.Columns.Contains("UD1"))
                model.UD1 = dr.Field<string>("UD1");
            if (enabledProperties.Contains("UD2") && dr.Table.Columns.Contains("UD2"))
                model.UD2 = dr.Field<string>("UD2");
            if (enabledProperties.Contains("UD3") && dr.Table.Columns.Contains("UD3"))
                model.UD3 = dr.Field<string>("UD3");
            if (enabledProperties.Contains("UD4") && dr.Table.Columns.Contains("UD4"))
                model.UD4 = dr.Field<string>("UD4");
            if (enabledProperties.Contains("UD5") && dr.Table.Columns.Contains("UD5"))
                model.UD5 = dr.Field<string>("UD5");
            if (enabledProperties.Contains("UD6") && dr.Table.Columns.Contains("UD6"))
                model.UD6 = dr.Field<string>("UD6");
            if (enabledProperties.Contains("UD7") && dr.Table.Columns.Contains("UD7"))
                model.UD7 = dr.Field<string>("UD7");
            if (enabledProperties.Contains("UD8") && dr.Table.Columns.Contains("UD8"))
                model.UD8 = dr.Field<string>("UD8");


            return model;
        }

        /// <summary>
        /// Delete a source cell record by CellID
        /// </summary>
        public void Delete(int cellId)
        {
            try
            {
                string sql = $"DELETE FROM {this.TableName} WHERE CellID = @cellID";
                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@cellID", cellId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    BRApi.Database.ExecuteActionQuery(dbConn, sql, paramList, false, true);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Delete a source cell record by CellID (string representation)
        /// </summary>
        public void Delete(string cellIdStr)
        {
            if (int.TryParse(cellIdStr, out int cellId))
            {
                this.Delete(cellId);
            }
            else
            {
                throw new XFException(si, new ArgumentException($"Invalid CellID format: {cellIdStr}"));
            }
        }

        /// <summary>
        /// Delete a source cell record by model instance
        /// </summary>
        public void Delete(FMM_Src_CellModel model)
        {
            this.Delete(model.CellID);
        }

        /// <summary>
        /// Create a default configuration for a workspace
        /// </summary>
        public void CreateDefaultConfig(Guid workspaceId, Dictionary<string, string> customSubstVars)
        {
            // Build a minimal starter row into a DataTable and push it through the GBL SQA helpers.
            // CalcType 1 = Table; adjust if other defaults are required.
            try
            {
                _ = workspaceId; // reserved for future workspace-specific defaults
                var CubeID = customSubstVars.XFGetValue("IV_FMM_CubeID", "0");
                var ActID = customSubstVars.XFGetValue("IV_FMM_ActID", "0");
                var ModelID = customSubstVars.XFGetValue("IV_FMM_ModelID", "0");
                var CalcID = customSubstVars.XFGetValue("BL_FMM_CalcList", "0");

                const int defaultCalcType = 1;

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();

                    // Compute next key
                    var maxIdHelper = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(this.si, connection);
                    var nextCellId = maxIdHelper.Get_Max_ID(this.si, this.TableName, "Src_Cell_ID");

                    // Pull current set (schema) using SQA builder
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(this.si, connection);
                    var sqa = new SqlDataAdapter();
                    var currentTable = new DataTable();

                    var selectColumns = GetSelectColumnsForCalcType(defaultCalcType);
                    if (!selectColumns.Split(',').Select(c => c.Trim()).Contains("Src_Cell_ID", StringComparer.OrdinalIgnoreCase))
                    {
                        selectColumns = $"CellID, {selectColumns}";
                    }

                    var currentSql = $"SELECT {selectColumns} FROM {this.TableName} WHERE CalcID = @calcID";
                    var sqlparams = new[] { new SqlParameter("@calcID", SqlDbType.Int) { Value = CalcID } };
                    cmdBuilder.FillDataTable(this.si, sqa, currentTable, currentSql, sqlparams);

                    if (currentTable.Columns.Contains("CellID"))
                    {
                        currentTable.PrimaryKey = new[] { currentTable.Columns["CellID"]! };
                    }

                    var now = DateTime.Now;
                    var user = this.si?.UserName ?? string.Empty;

                    // Create the starter row with minimal required fields
                    var row = currentTable.NewRow();
                    SetColumnValue(row, "CellID", nextCellId);
                    SetColumnValue(row, "CubeID", CubeID);
                    SetColumnValue(row, "ActID", ActID);
                    SetColumnValue(row, "ModelID", ModelID);
                    SetColumnValue(row, "CalcID", CalcID);
                    SetColumnValue(row, "Order", 1);
                    SetColumnValue(row, "Type", string.Empty);
                    SetColumnValue(row, "ItemType", string.Empty);

                    currentTable.Rows.Add(row);

                    cmdBuilder.UpdateTableSimple(this.si, this.TableName, currentTable, sqa, "CellID");
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}