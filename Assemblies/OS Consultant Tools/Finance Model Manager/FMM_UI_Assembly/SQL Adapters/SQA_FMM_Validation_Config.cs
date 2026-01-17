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
    /// SQL Adapter for FMM Validation Framework
    /// Provides CRUD operations for validation configurations, runs, and results
    /// Supports both TABLE and CUBE validation contexts
    /// </summary>
    public class SQA_FMM_Validation_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Validation_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        #region "Validation Config Methods"

        public void Fill_FMM_Validation_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Updates validation configuration records in the database.
        /// Note: Caller must populate Update_Date and Update_User in the DataTable before calling this method.
        /// </summary>
        public void Update_FMM_Validation_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "FMM_Validation_Config", dt);
                    builder.SetPrimaryKey("Validation_Config_ID");
                    builder.ExcludeFromUpdate("Validation_Config_ID", "Create_Date", "Create_User");
                    builder.ConfigureAdapter(sqa, transaction);

                    sqa.Update(dt);
                    transaction.Commit();
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public DataTable Get_Validation_Config_By_ID(SessionInfo si, int validationConfigID)
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var sql = @"SELECT 
                                Validation_Config_ID,
                                Name,
                                Description,
                                Validation_Context,
                                Validation_Type,
                                Target_Object,
                                Process_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM FMM_Validation_Config
                            WHERE Validation_Config_ID = @Validation_Config_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Validation_Config_ID", SqlDbType.Int) { Value = validationConfigID }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Config_By_Context(SessionInfo si, string validationContext, bool activeOnly = true)
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var sql = @"SELECT 
                                Validation_Config_ID,
                                Name,
                                Description,
                                Validation_Context,
                                Validation_Type,
                                Target_Object,
                                Process_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM FMM_Validation_Config
                            WHERE Validation_Context = @Validation_Context";
                
                if (activeOnly)
                {
                    sql += " AND Is_Active = 1";
                }
                
                sql += " ORDER BY Process_Type, Name";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Validation_Context", SqlDbType.NVarChar, 20) { Value = validationContext }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Config_By_Process(SessionInfo si, string processType, bool activeOnly = true)
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var sql = @"SELECT 
                                Validation_Config_ID,
                                Name,
                                Description,
                                Validation_Context,
                                Validation_Type,
                                Target_Object,
                                Process_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM FMM_Validation_Config
                            WHERE Process_Type = @Process_Type";
                
                if (activeOnly)
                {
                    sql += " AND Is_Active = 1";
                }
                
                sql += " ORDER BY Validation_Context, Name";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Process_Type", SqlDbType.NVarChar, 100) { Value = processType }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Active_Validation_Configs(SessionInfo si)
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var sql = @"SELECT 
                                Validation_Config_ID,
                                Name,
                                Description,
                                Validation_Context,
                                Validation_Type,
                                Target_Object,
                                Process_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM FMM_Validation_Config
                            WHERE Is_Active = 1
                            ORDER BY Validation_Context, Process_Type, Name";
                
                var sqlparams = new SqlParameter[] { };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Validation Run Methods"

        public void Fill_FMM_Validation_Run_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Updates validation run records in the database.
        /// </summary>
        public void Update_FMM_Validation_Run(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "FMM_Validation_Run", dt);
                    builder.SetPrimaryKey("Run_ID");
                    builder.ExcludeFromUpdate("Run_ID", "Create_Date");
                    builder.ConfigureAdapter(sqa, transaction);

                    sqa.Update(dt);
                    transaction.Commit();
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public DataTable Get_Validation_Run_By_ID(SessionInfo si, int runID)
        {
            try
            {
                var dt = new DataTable("Validation_Run");
                var sql = @"SELECT 
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Validation_Context,
                                Process_Type,
                                Target_Object,
                                Total_Validations,
                                Total_Records_Checked,
                                Total_Failures,
                                Total_Warnings,
                                Total_Info,
                                Execution_Time_Ms,
                                Status,
                                Error_Message,
                                Notes,
                                Create_Date
                            FROM FMM_Validation_Run
                            WHERE Run_ID = @Run_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Run_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Recent_Validation_Runs(SessionInfo si, int topN = 50, string validationContext = null, string processType = null)
        {
            try
            {
                var dt = new DataTable("Validation_Runs");
                var sql = $@"SELECT TOP {topN}
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Validation_Context,
                                Process_Type,
                                Target_Object,
                                Total_Validations,
                                Total_Records_Checked,
                                Total_Failures,
                                Total_Warnings,
                                Total_Info,
                                Execution_Time_Ms,
                                Status,
                                Error_Message,
                                Notes,
                                Create_Date
                            FROM FMM_Validation_Run
                            WHERE 1=1";
                
                var paramList = new List<SqlParameter>();
                
                if (!string.IsNullOrEmpty(validationContext))
                {
                    sql += " AND Validation_Context = @Validation_Context";
                    paramList.Add(new SqlParameter("@Validation_Context", SqlDbType.NVarChar, 20) { Value = validationContext });
                }
                
                if (!string.IsNullOrEmpty(processType))
                {
                    sql += " AND Process_Type = @Process_Type";
                    paramList.Add(new SqlParameter("@Process_Type", SqlDbType.NVarChar, 100) { Value = processType });
                }
                
                sql += " ORDER BY Run_Date DESC";

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Run_DT(si, sqa, dt, sql, paramList.ToArray());
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Validation Result Methods"

        public void Fill_FMM_Validation_Result_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Inserts validation results in bulk for performance.
        /// </summary>
        public void Insert_Validation_Results(SessionInfo si, DataTable dt)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, transaction))
                    {
                        bulkCopy.DestinationTableName = "FMM_Validation_Result";
                        bulkCopy.BatchSize = 1000;
                        
                        // Map columns explicitly
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

        public DataTable Get_Validation_Results_By_Run_ID(SessionInfo si, int runID)
        {
            try
            {
                var dt = new DataTable("Validation_Results");
                var sql = @"SELECT 
                                Validation_Result_ID,
                                Validation_Config_ID,
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Validation_Context,
                                Table_Name,
                                Primary_Key_Value,
                                Column_Name,
                                Column_Value,
                                Cube_POV,
                                Dimension_Values,
                                Cell_Value,
                                Comparison_Value,
                                Validation_Status,
                                Error_Message,
                                Error_Details,
                                Create_Date
                            FROM FMM_Validation_Result
                            WHERE Run_ID = @Run_ID
                            ORDER BY Validation_Result_ID DESC";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Result_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Results_By_Config_ID(SessionInfo si, int validationConfigID, int? topN = null)
        {
            try
            {
                var dt = new DataTable("Validation_Results");
                var sql = topN.HasValue 
                    ? $"SELECT TOP {topN.Value} " 
                    : "SELECT ";
                
                sql += @"Validation_Result_ID,
                                Validation_Config_ID,
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Validation_Context,
                                Table_Name,
                                Primary_Key_Value,
                                Column_Name,
                                Column_Value,
                                Cube_POV,
                                Dimension_Values,
                                Cell_Value,
                                Comparison_Value,
                                Validation_Status,
                                Error_Message,
                                Error_Details,
                                Create_Date
                            FROM FMM_Validation_Result
                            WHERE Validation_Config_ID = @Validation_Config_ID
                            ORDER BY Run_Date DESC, Validation_Result_ID DESC";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Validation_Config_ID", SqlDbType.Int) { Value = validationConfigID }
                };

                var sqa = new SqlDataAdapter();
                Fill_FMM_Validation_Result_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public void Delete_Validation_Results_By_Run_ID(SessionInfo si, int runID)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("DELETE FROM FMM_Validation_Result WHERE Run_ID = @Run_ID", _connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID });
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public void Delete_Old_Validation_Results(SessionInfo si, int daysToKeep)
        {
            try
            {
                var sql = @"DELETE FROM FMM_Validation_Result 
                            WHERE Run_Date < DATEADD(day, @DaysToKeep, GETDATE())";
                
                using (SqlCommand cmd = new SqlCommand(sql, _connection))
                {
                    cmd.Parameters.Add(new SqlParameter("@DaysToKeep", SqlDbType.Int) { Value = -daysToKeep });
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Helper Methods"

        public int Get_Next_Run_ID(SessionInfo si)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(Run_ID), 0) + 1 FROM FMM_Validation_Run", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public int Get_Next_Validation_Config_ID(SessionInfo si)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT ISNULL(MAX(Validation_Config_ID), 0) + 1 FROM FMM_Validation_Config", _connection))
                {
                    var result = cmd.ExecuteScalar();
                    return Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion
    }
}
