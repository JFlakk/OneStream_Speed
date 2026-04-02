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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// SQL adapter for the Data Validation Manager (DVM).
    /// Provides CRUD operations for DVM_Config, DVM_Rule, DVM_Run, and DVM_Result tables.
    /// </summary>
    public class SQA_DVM_Validation
    {
        private readonly SqlConnection _connection;

        public SQA_DVM_Validation(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        // =====================================================================
        #region "DVM_Config Methods"
        // =====================================================================

        /// <summary>
        /// Fills a DataTable from an arbitrary SELECT query against DVM_Config.
        /// </summary>
        public void Fill_DVM_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt,
            string selectQuery, params SqlParameter[] sqlparams)
        {
            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams?.Length > 0)
                    command.Parameters.AddRange(sqlparams);

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Persists changes (insert / update / delete) to DVM_Config.
        /// The caller must set Update_Date and Update_User before invoking this method.
        /// </summary>
        public void Update_DVM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    var builder = new GBL_SQL_Command_Builder(_connection, "DVM_Config", dt);
                    builder.SetPrimaryKey("DVM_Config_ID");
                    builder.ExcludeFromUpdate("DVM_Config_ID", "Create_Date", "Create_User");
                    builder.ConfigureAdapter(sqa, transaction);

                    sqa.Update(dt);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        /// <summary>
        /// Returns the full DVM_Config row for the given ID.
        /// </summary>
        public DataTable Get_DVM_Config_By_ID(SessionInfo si, int dvmConfigID)
        {
            var dt = new DataTable("DVM_Config");
            var sql = @"
                SELECT
                    DVM_Config_ID, Name, Description, Context_Type,
                    Table_Schema, Table_Name, Table_Filter,
                    Cube_View_Name, FDX_Base_Query,
                    Is_Active,
                    Create_Date, Create_User, Update_Date, Update_User
                FROM DVM_Config
                WHERE DVM_Config_ID = @DVM_Config_ID";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Config_ID", SqlDbType.Int) { Value = dvmConfigID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Returns all active DVM_Config rows, ordered by Name.
        /// </summary>
        public DataTable Get_Active_DVM_Configs(SessionInfo si)
        {
            var dt = new DataTable("DVM_Config");
            var sql = @"
                SELECT
                    DVM_Config_ID, Name, Description, Context_Type,
                    Table_Schema, Table_Name, Table_Filter,
                    Cube_View_Name, FDX_Base_Query,
                    Is_Active,
                    Create_Date, Create_User, Update_Date, Update_User
                FROM DVM_Config
                WHERE Is_Active = 1
                ORDER BY Name";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Returns all DVM_Config rows for the given Context_Type ('Table' or 'Cube').
        /// </summary>
        public DataTable Get_DVM_Configs_By_Context(SessionInfo si, string contextType)
        {
            var dt = new DataTable("DVM_Config");
            var sql = @"
                SELECT
                    DVM_Config_ID, Name, Description, Context_Type,
                    Table_Schema, Table_Name, Table_Filter,
                    Cube_View_Name, FDX_Base_Query,
                    Is_Active,
                    Create_Date, Create_User, Update_Date, Update_User
                FROM DVM_Config
                WHERE Context_Type = @Context_Type
                ORDER BY Name";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@Context_Type", SqlDbType.NVarChar, 10) { Value = contextType });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        #endregion

        // =====================================================================
        #region "DVM_Rule Methods"
        // =====================================================================

        /// <summary>
        /// Fills a DataTable from an arbitrary SELECT query against DVM_Rule.
        /// </summary>
        public void Fill_DVM_Rule_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt,
            string selectQuery, params SqlParameter[] sqlparams)
        {
            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams?.Length > 0)
                    command.Parameters.AddRange(sqlparams);

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Persists changes (insert / update / delete) to DVM_Rule.
        /// The caller must set Update_Date and Update_User before invoking this method.
        /// </summary>
        public void Update_DVM_Rule(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    var builder = new GBL_SQL_Command_Builder(_connection, "DVM_Rule", dt);
                    builder.SetPrimaryKey("DVM_Rule_ID");
                    builder.ExcludeFromUpdate("DVM_Rule_ID", "Create_Date", "Create_User");
                    builder.ConfigureAdapter(sqa, transaction);

                    sqa.Update(dt);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        /// <summary>
        /// Returns all active rules for the specified DVM_Config, ordered by Sort_Order.
        /// </summary>
        public DataTable Get_Rules_By_Config(SessionInfo si, int dvmConfigID)
        {
            var dt = new DataTable("DVM_Rule");
            var sql = @"
                SELECT
                    DVM_Rule_ID, DVM_Config_ID, Rule_Name, Description,
                    Rule_Type, Severity,
                    Src_Row_Filter, Src_Column,
                    Tgt_Row_Filter, Tgt_Column,
                    Src_FDX, Tgt_FDX,
                    Tolerance_Pct, Sort_Order, Is_Active,
                    Create_Date, Create_User, Update_Date, Update_User
                FROM DVM_Rule
                WHERE DVM_Config_ID = @DVM_Config_ID
                  AND Is_Active = 1
                ORDER BY Sort_Order";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Config_ID", SqlDbType.Int) { Value = dvmConfigID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Returns all rules (active and inactive) for the specified DVM_Config.
        /// </summary>
        public DataTable Get_All_Rules_By_Config(SessionInfo si, int dvmConfigID)
        {
            var dt = new DataTable("DVM_Rule");
            var sql = @"
                SELECT
                    DVM_Rule_ID, DVM_Config_ID, Rule_Name, Description,
                    Rule_Type, Severity,
                    Src_Row_Filter, Src_Column,
                    Tgt_Row_Filter, Tgt_Column,
                    Src_FDX, Tgt_FDX,
                    Tolerance_Pct, Sort_Order, Is_Active,
                    Create_Date, Create_User, Update_Date, Update_User
                FROM DVM_Rule
                WHERE DVM_Config_ID = @DVM_Config_ID
                ORDER BY Sort_Order";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Config_ID", SqlDbType.Int) { Value = dvmConfigID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        #endregion

        // =====================================================================
        #region "DVM_Run Methods"
        // =====================================================================

        /// <summary>
        /// Fills a DataTable from an arbitrary SELECT query against DVM_Run.
        /// </summary>
        public void Fill_DVM_Run_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt,
            string selectQuery, params SqlParameter[] sqlparams)
        {
            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams?.Length > 0)
                    command.Parameters.AddRange(sqlparams);

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Persists changes (insert / update / delete) to DVM_Run.
        /// </summary>
        public void Update_DVM_Run(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    var builder = new GBL_SQL_Command_Builder(_connection, "DVM_Run", dt);
                    builder.SetPrimaryKey("DVM_Run_ID");
                    builder.ExcludeFromUpdate("DVM_Run_ID", "Run_Date", "Run_User");
                    builder.ConfigureAdapter(sqa, transaction);

                    sqa.Update(dt);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        /// <summary>
        /// Returns the most recent N runs for all configs, ordered by Run_Date descending.
        /// </summary>
        public DataTable Get_Recent_DVM_Runs(SessionInfo si, int topN = 50)
        {
            var dt = new DataTable("DVM_Run");
            var sql = $@"
                SELECT TOP {topN}
                    r.DVM_Run_ID, r.DVM_Config_ID, c.Name AS Config_Name,
                    r.Run_Date, r.Run_User, r.Status,
                    r.Total_Rules, r.Total_Pass, r.Total_Fail, r.Total_Warning,
                    r.Execution_Time_Ms, r.Error_Message
                FROM DVM_Run r
                INNER JOIN DVM_Config c ON c.DVM_Config_ID = r.DVM_Config_ID
                ORDER BY r.Run_Date DESC";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Returns runs for the specified config, ordered by Run_Date descending.
        /// </summary>
        public DataTable Get_DVM_Runs_By_Config(SessionInfo si, int dvmConfigID, int topN = 50)
        {
            var dt = new DataTable("DVM_Run");
            var sql = $@"
                SELECT TOP {topN}
                    r.DVM_Run_ID, r.DVM_Config_ID, c.Name AS Config_Name,
                    r.Run_Date, r.Run_User, r.Status,
                    r.Total_Rules, r.Total_Pass, r.Total_Fail, r.Total_Warning,
                    r.Execution_Time_Ms, r.Error_Message
                FROM DVM_Run r
                INNER JOIN DVM_Config c ON c.DVM_Config_ID = r.DVM_Config_ID
                WHERE r.DVM_Config_ID = @DVM_Config_ID
                ORDER BY r.Run_Date DESC";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Config_ID", SqlDbType.Int) { Value = dvmConfigID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        #endregion

        // =====================================================================
        #region "DVM_Result Methods"
        // =====================================================================

        /// <summary>
        /// Fills a DataTable from an arbitrary SELECT query against DVM_Result.
        /// </summary>
        public void Fill_DVM_Result_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt,
            string selectQuery, params SqlParameter[] sqlparams)
        {
            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams?.Length > 0)
                    command.Parameters.AddRange(sqlparams);

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Bulk-inserts a batch of validation results using SqlBulkCopy for performance.
        /// </summary>
        public void Insert_DVM_Results(SessionInfo si, DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return;

            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    using (var bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = "DVM_Result";
                        bulkCopy.BatchSize = 1000;
                        bulkCopy.BulkCopyTimeout = 120;

                        bulkCopy.ColumnMappings.Add("DVM_Run_ID", "DVM_Run_ID");
                        bulkCopy.ColumnMappings.Add("DVM_Rule_ID", "DVM_Rule_ID");
                        bulkCopy.ColumnMappings.Add("Rule_Name", "Rule_Name");
                        bulkCopy.ColumnMappings.Add("Status", "Status");
                        bulkCopy.ColumnMappings.Add("Src_Value", "Src_Value");
                        bulkCopy.ColumnMappings.Add("Tgt_Value", "Tgt_Value");
                        bulkCopy.ColumnMappings.Add("Expected_Operator", "Expected_Operator");
                        bulkCopy.ColumnMappings.Add("Tolerance_Pct", "Tolerance_Pct");
                        bulkCopy.ColumnMappings.Add("Message", "Message");
                        bulkCopy.ColumnMappings.Add("Row_Context", "Row_Context");

                        bulkCopy.WriteToServer(dt);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns all results for the specified run, joined with rule information.
        /// </summary>
        public DataTable Get_DVM_Results_By_Run(SessionInfo si, int dvmRunID)
        {
            var dt = new DataTable("DVM_Result");
            var sql = @"
                SELECT
                    res.DVM_Result_ID, res.DVM_Run_ID, res.DVM_Rule_ID,
                    res.Rule_Name, res.Status,
                    res.Src_Value, res.Tgt_Value,
                    res.Expected_Operator, res.Tolerance_Pct,
                    res.Message, res.Row_Context
                FROM DVM_Result res
                WHERE res.DVM_Run_ID = @DVM_Run_ID
                ORDER BY res.DVM_Result_ID";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Run_ID", SqlDbType.Int) { Value = dvmRunID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Returns failed or warning results for the specified run.
        /// </summary>
        public DataTable Get_DVM_Failures_By_Run(SessionInfo si, int dvmRunID)
        {
            var dt = new DataTable("DVM_Result");
            var sql = @"
                SELECT
                    res.DVM_Result_ID, res.DVM_Run_ID, res.DVM_Rule_ID,
                    res.Rule_Name, res.Status,
                    res.Src_Value, res.Tgt_Value,
                    res.Expected_Operator, res.Tolerance_Pct,
                    res.Message, res.Row_Context
                FROM DVM_Result res
                WHERE res.DVM_Run_ID = @DVM_Run_ID
                  AND res.Status IN ('Fail', 'Warning', 'Error')
                ORDER BY res.Status, res.DVM_Result_ID";

            using (var sqa = new SqlDataAdapter())
            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Run_ID", SqlDbType.Int) { Value = dvmRunID });
                sqa.SelectCommand = command;
                sqa.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Deletes all result rows for the specified run.
        /// </summary>
        public void Delete_DVM_Results_By_Run(SessionInfo si, int dvmRunID)
        {
            using (var command = new SqlCommand("DELETE FROM DVM_Result WHERE DVM_Run_ID = @DVM_Run_ID", _connection))
            {
                command.Parameters.Add(new SqlParameter("@DVM_Run_ID", SqlDbType.Int) { Value = dvmRunID });
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Deletes result rows older than the specified number of days to control table growth.
        /// </summary>
        public void Delete_Old_DVM_Results(SessionInfo si, int retentionDays = 90)
        {
            var sql = @"
                DELETE res
                FROM DVM_Result res
                INNER JOIN DVM_Run r ON r.DVM_Run_ID = res.DVM_Run_ID
                WHERE r.Run_Date < DATEADD(DAY, -@RetentionDays, GETDATE())";

            using (var command = new SqlCommand(sql, _connection))
            {
                command.Parameters.Add(new SqlParameter("@RetentionDays", SqlDbType.Int) { Value = retentionDays });
                command.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
