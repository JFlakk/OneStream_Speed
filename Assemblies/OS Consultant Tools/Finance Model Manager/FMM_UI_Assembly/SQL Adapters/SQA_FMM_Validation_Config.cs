using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;

namespace __WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// SQL Adapter for FMM Validation Framework
    /// Provides CRUD operations for validation configurations, runs, and results
    /// Supports both TABLE and CUBE validation contexts
    /// </summary>
    public class SQA_FMM_Validation_Config
    {
        private SessionInfo _si;
        private DbConnInfo _connection;

        public SQA_FMM_Validation_Config(SessionInfo si, DbConnInfo connection)
        {
            _si = si;
            _connection = connection;
        }

        #region Validation Config Methods

        /// <summary>
        /// Base method to fill FMM_Validation_Config DataTable
        /// </summary>
        private DataTable Fill_FMM_Validation_Config_DT(SessionInfo si, string whereClause = "")
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("    Validation_Config_ID,");
                sql.AppendLine("    Name,");
                sql.AppendLine("    Description,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Validation_Type,");
                sql.AppendLine("    Target_Object,");
                sql.AppendLine("    Process_Type,");
                sql.AppendLine("    Is_Active,");
                sql.AppendLine("    Severity,");
                sql.AppendLine("    Config_JSON,");
                sql.AppendLine("    Create_Date,");
                sql.AppendLine("    Create_User,");
                sql.AppendLine("    Update_Date,");
                sql.AppendLine("    Update_User");
                sql.AppendLine("FROM FMM_Validation_Config");

                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql.AppendLine(whereClause);
                }

                sql.AppendLine("ORDER BY Validation_Context, Process_Type, Name");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Update (Insert/Update/Delete) validation config records using GBL_SQL_Command_Builder
        /// </summary>
        public int Update_FMM_Validation_Config(SessionInfo si, DataTable dtChanges)
        {
            try
            {
                if (dtChanges == null || dtChanges.Rows.Count == 0)
                    return 0;

                // Get base table structure
                DataTable dtBase = Fill_FMM_Validation_Config_DT(si, "WHERE 1=0");

                // Use GBL_SQL_Command_Builder to generate and execute commands
                var builder = new GBL_SQL_Command_Builder(si, _connection, "FMM_Validation_Config", dtBase);
                int rowsAffected = builder.Update(si, dtChanges);

                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation config by ID
        /// </summary>
        public DataTable Get_Validation_Config_By_ID(SessionInfo si, int validationConfigID)
        {
            try
            {
                string whereClause = $"WHERE Validation_Config_ID = {validationConfigID}";
                return Fill_FMM_Validation_Config_DT(si, whereClause);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation configs by context (TABLE or CUBE)
        /// </summary>
        public DataTable Get_Validation_Config_By_Context(SessionInfo si, string validationContext, bool activeOnly = true)
        {
            try
            {
                StringBuilder whereClause = new StringBuilder();
                whereClause.AppendLine($"WHERE Validation_Context = '{validationContext}'");
                
                if (activeOnly)
                {
                    whereClause.AppendLine("AND Is_Active = 1");
                }

                return Fill_FMM_Validation_Config_DT(si, whereClause.ToString());
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation configs by process type
        /// </summary>
        public DataTable Get_Validation_Config_By_Process(SessionInfo si, string processType, bool activeOnly = true)
        {
            try
            {
                StringBuilder whereClause = new StringBuilder();
                whereClause.AppendLine($"WHERE Process_Type = '{processType}'");
                
                if (activeOnly)
                {
                    whereClause.AppendLine("AND Is_Active = 1");
                }

                return Fill_FMM_Validation_Config_DT(si, whereClause.ToString());
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation configs by target object (table or cube name)
        /// </summary>
        public DataTable Get_Validation_Config_By_Target(SessionInfo si, string targetObject, bool activeOnly = true)
        {
            try
            {
                StringBuilder whereClause = new StringBuilder();
                whereClause.AppendLine($"WHERE Target_Object = '{targetObject}'");
                
                if (activeOnly)
                {
                    whereClause.AppendLine("AND Is_Active = 1");
                }

                return Fill_FMM_Validation_Config_DT(si, whereClause.ToString());
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get all active validation configs
        /// </summary>
        public DataTable Get_Active_Validation_Configs(SessionInfo si)
        {
            try
            {
                string whereClause = "WHERE Is_Active = 1";
                return Fill_FMM_Validation_Config_DT(si, whereClause);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region Validation Run Methods

        /// <summary>
        /// Base method to fill FMM_Validation_Run DataTable
        /// </summary>
        private DataTable Fill_FMM_Validation_Run_DT(SessionInfo si, string whereClause = "")
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("    Run_ID,");
                sql.AppendLine("    Run_Date,");
                sql.AppendLine("    Run_User,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Process_Type,");
                sql.AppendLine("    Target_Object,");
                sql.AppendLine("    Total_Validations,");
                sql.AppendLine("    Total_Records_Checked,");
                sql.AppendLine("    Total_Failures,");
                sql.AppendLine("    Total_Warnings,");
                sql.AppendLine("    Total_Info,");
                sql.AppendLine("    Execution_Time_Ms,");
                sql.AppendLine("    Status,");
                sql.AppendLine("    Error_Message,");
                sql.AppendLine("    Notes,");
                sql.AppendLine("    Create_Date");
                sql.AppendLine("FROM FMM_Validation_Run");

                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql.AppendLine(whereClause);
                }

                sql.AppendLine("ORDER BY Run_Date DESC");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Update (Insert/Update/Delete) validation run records using GBL_SQL_Command_Builder
        /// </summary>
        public int Update_FMM_Validation_Run(SessionInfo si, DataTable dtChanges)
        {
            try
            {
                if (dtChanges == null || dtChanges.Rows.Count == 0)
                    return 0;

                // Get base table structure
                DataTable dtBase = Fill_FMM_Validation_Run_DT(si, "WHERE 1=0");

                // Use GBL_SQL_Command_Builder to generate and execute commands
                var builder = new GBL_SQL_Command_Builder(si, _connection, "FMM_Validation_Run", dtBase);
                int rowsAffected = builder.Update(si, dtChanges);

                return rowsAffected;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation run by ID
        /// </summary>
        public DataTable Get_Validation_Run_By_ID(SessionInfo si, int runID)
        {
            try
            {
                string whereClause = $"WHERE Run_ID = {runID}";
                return Fill_FMM_Validation_Run_DT(si, whereClause);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get recent validation runs with optional filtering
        /// </summary>
        public DataTable Get_Recent_Validation_Runs(SessionInfo si, int topN = 50, string validationContext = null, string processType = null)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine($"SELECT TOP {topN}");
                sql.AppendLine("    Run_ID,");
                sql.AppendLine("    Run_Date,");
                sql.AppendLine("    Run_User,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Process_Type,");
                sql.AppendLine("    Target_Object,");
                sql.AppendLine("    Total_Validations,");
                sql.AppendLine("    Total_Records_Checked,");
                sql.AppendLine("    Total_Failures,");
                sql.AppendLine("    Total_Warnings,");
                sql.AppendLine("    Total_Info,");
                sql.AppendLine("    Execution_Time_Ms,");
                sql.AppendLine("    Status,");
                sql.AppendLine("    Error_Message,");
                sql.AppendLine("    Notes,");
                sql.AppendLine("    Create_Date");
                sql.AppendLine("FROM FMM_Validation_Run");
                
                bool hasWhere = false;
                
                if (!string.IsNullOrEmpty(validationContext))
                {
                    sql.AppendLine($"WHERE Validation_Context = '{validationContext}'");
                    hasWhere = true;
                }
                
                if (!string.IsNullOrEmpty(processType))
                {
                    sql.AppendLine(hasWhere ? "AND" : "WHERE");
                    sql.AppendLine($"Process_Type = '{processType}'");
                }
                
                sql.AppendLine("ORDER BY Run_Date DESC");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region Validation Result Methods

        /// <summary>
        /// Base method to fill FMM_Validation_Result DataTable
        /// </summary>
        private DataTable Fill_FMM_Validation_Result_DT(SessionInfo si, string whereClause = "")
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.AppendLine("SELECT ");
                sql.AppendLine("    Validation_Result_ID,");
                sql.AppendLine("    Validation_Config_ID,");
                sql.AppendLine("    Run_ID,");
                sql.AppendLine("    Run_Date,");
                sql.AppendLine("    Run_User,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Table_Name,");
                sql.AppendLine("    Primary_Key_Value,");
                sql.AppendLine("    Column_Name,");
                sql.AppendLine("    Column_Value,");
                sql.AppendLine("    Cube_POV,");
                sql.AppendLine("    Dimension_Values,");
                sql.AppendLine("    Cell_Value,");
                sql.AppendLine("    Comparison_Value,");
                sql.AppendLine("    Validation_Status,");
                sql.AppendLine("    Error_Message,");
                sql.AppendLine("    Error_Details,");
                sql.AppendLine("    Create_Date");
                sql.AppendLine("FROM FMM_Validation_Result");

                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql.AppendLine(whereClause);
                }

                sql.AppendLine("ORDER BY Validation_Result_ID DESC");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Insert validation results using SqlBulkCopy for performance
        /// Optimized for large result sets
        /// </summary>
        public int Insert_Validation_Results(SessionInfo si, DataTable dtResults)
        {
            try
            {
                if (dtResults == null || dtResults.Rows.Count == 0)
                    return 0;

                int rowsInserted = 0;

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conn))
                    {
                        bulkCopy.DestinationTableName = "FMM_Validation_Result";
                        bulkCopy.BatchSize = 1000;
                        bulkCopy.BulkCopyTimeout = 300; // 5 minutes

                        // Map columns
                        bulkCopy.ColumnMappings.Add("Validation_Config_ID", "Validation_Config_ID");
                        bulkCopy.ColumnMappings.Add("Run_ID", "Run_ID");
                        bulkCopy.ColumnMappings.Add("Run_Date", "Run_Date");
                        bulkCopy.ColumnMappings.Add("Run_User", "Run_User");
                        bulkCopy.ColumnMappings.Add("Validation_Context", "Validation_Context");
                        bulkCopy.ColumnMappings.Add("Table_Name", "Table_Name");
                        bulkCopy.ColumnMappings.Add("Primary_Key_Value", "Primary_Key_Value");
                        bulkCopy.ColumnMappings.Add("Column_Name", "Column_Name");
                        bulkCopy.ColumnMappings.Add("Column_Value", "Column_Value");
                        bulkCopy.ColumnMappings.Add("Cube_POV", "Cube_POV");
                        bulkCopy.ColumnMappings.Add("Dimension_Values", "Dimension_Values");
                        bulkCopy.ColumnMappings.Add("Cell_Value", "Cell_Value");
                        bulkCopy.ColumnMappings.Add("Comparison_Value", "Comparison_Value");
                        bulkCopy.ColumnMappings.Add("Validation_Status", "Validation_Status");
                        bulkCopy.ColumnMappings.Add("Error_Message", "Error_Message");
                        bulkCopy.ColumnMappings.Add("Error_Details", "Error_Details");
                        bulkCopy.ColumnMappings.Add("Create_Date", "Create_Date");

                        bulkCopy.WriteToServer(dtResults);
                        rowsInserted = dtResults.Rows.Count;
                    }
                }

                return rowsInserted;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation results by run ID
        /// </summary>
        public DataTable Get_Validation_Results_By_Run_ID(SessionInfo si, int runID)
        {
            try
            {
                string whereClause = $"WHERE Run_ID = {runID}";
                return Fill_FMM_Validation_Result_DT(si, whereClause);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation results by config ID
        /// </summary>
        public DataTable Get_Validation_Results_By_Config_ID(SessionInfo si, int validationConfigID, int? topN = null)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                
                if (topN.HasValue)
                {
                    sql.AppendLine($"SELECT TOP {topN.Value}");
                }
                else
                {
                    sql.AppendLine("SELECT");
                }
                
                sql.AppendLine("    Validation_Result_ID,");
                sql.AppendLine("    Validation_Config_ID,");
                sql.AppendLine("    Run_ID,");
                sql.AppendLine("    Run_Date,");
                sql.AppendLine("    Run_User,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Table_Name,");
                sql.AppendLine("    Primary_Key_Value,");
                sql.AppendLine("    Column_Name,");
                sql.AppendLine("    Column_Value,");
                sql.AppendLine("    Cube_POV,");
                sql.AppendLine("    Dimension_Values,");
                sql.AppendLine("    Cell_Value,");
                sql.AppendLine("    Comparison_Value,");
                sql.AppendLine("    Validation_Status,");
                sql.AppendLine("    Error_Message,");
                sql.AppendLine("    Error_Details,");
                sql.AppendLine("    Create_Date");
                sql.AppendLine("FROM FMM_Validation_Result");
                sql.AppendLine($"WHERE Validation_Config_ID = {validationConfigID}");
                sql.AppendLine("ORDER BY Run_Date DESC, Validation_Result_ID DESC");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get validation results by context (TABLE or CUBE)
        /// </summary>
        public DataTable Get_Validation_Results_By_Context(SessionInfo si, string validationContext, int? runID = null, int? topN = null)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                
                if (topN.HasValue)
                {
                    sql.AppendLine($"SELECT TOP {topN.Value}");
                }
                else
                {
                    sql.AppendLine("SELECT");
                }
                
                sql.AppendLine("    Validation_Result_ID,");
                sql.AppendLine("    Validation_Config_ID,");
                sql.AppendLine("    Run_ID,");
                sql.AppendLine("    Run_Date,");
                sql.AppendLine("    Run_User,");
                sql.AppendLine("    Validation_Context,");
                sql.AppendLine("    Table_Name,");
                sql.AppendLine("    Primary_Key_Value,");
                sql.AppendLine("    Column_Name,");
                sql.AppendLine("    Column_Value,");
                sql.AppendLine("    Cube_POV,");
                sql.AppendLine("    Dimension_Values,");
                sql.AppendLine("    Cell_Value,");
                sql.AppendLine("    Comparison_Value,");
                sql.AppendLine("    Validation_Status,");
                sql.AppendLine("    Error_Message,");
                sql.AppendLine("    Error_Details,");
                sql.AppendLine("    Create_Date");
                sql.AppendLine("FROM FMM_Validation_Result");
                sql.AppendLine($"WHERE Validation_Context = '{validationContext}'");
                
                if (runID.HasValue)
                {
                    sql.AppendLine($"AND Run_ID = {runID.Value}");
                }
                
                sql.AppendLine("ORDER BY Run_Date DESC, Validation_Result_ID DESC");

                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql.ToString(), true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Delete validation results by run ID
        /// Used for cleanup of specific runs
        /// </summary>
        public int Delete_Validation_Results_By_Run_ID(SessionInfo si, int runID)
        {
            try
            {
                string sql = $"DELETE FROM FMM_Validation_Result WHERE Run_ID = {runID}";
                return BRApi.Database.ExecuteSqlNonQuery(si, _connection, sql);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Delete old validation results by date
        /// Used for regular cleanup/archival
        /// </summary>
        public int Delete_Old_Validation_Results(SessionInfo si, int daysToKeep)
        {
            try
            {
                string sql = $@"
                    DELETE FROM FMM_Validation_Result 
                    WHERE Run_Date < DATEADD(day, -{daysToKeep}, GETDATE())";
                
                return BRApi.Database.ExecuteSqlNonQuery(si, _connection, sql);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get next available Run_ID
        /// </summary>
        public int Get_Next_Run_ID(SessionInfo si)
        {
            try
            {
                string sql = "SELECT ISNULL(MAX(Run_ID), 0) + 1 FROM FMM_Validation_Run";
                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql, true);
                
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]);
                }
                
                return 1;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Get next available Validation_Config_ID
        /// </summary>
        public int Get_Next_Validation_Config_ID(SessionInfo si)
        {
            try
            {
                string sql = "SELECT ISNULL(MAX(Validation_Config_ID), 0) + 1 FROM FMM_Validation_Config";
                DataTable dt = BRApi.Database.ExecuteSql(si, _connection, sql, true);
                
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt32(dt.Rows[0][0]);
                }
                
                return 1;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion
    }
}
