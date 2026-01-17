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
    public class SQA_MDM_Validation_Config
    {
        private readonly SqlConnection _connection;

        public SQA_MDM_Validation_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        #region "Validation Config Methods"

        public void Fill_MDM_Validation_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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
        public void Update_MDM_Validation_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "MDM_Validation_Config", dt);
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
                                Dim_Type,
                                Dim_ID,
                                Validation_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM MDM_Validation_Config
                            WHERE Validation_Config_ID = @Validation_Config_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Validation_Config_ID", SqlDbType.Int) { Value = validationConfigID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Config_By_Dimension(SessionInfo si, string dimType, int dimID)
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var sql = @"SELECT 
                                Validation_Config_ID,
                                Name,
                                Description,
                                Dim_Type,
                                Dim_ID,
                                Validation_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM MDM_Validation_Config
                            WHERE Dim_Type = @Dim_Type AND Dim_ID = @Dim_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Dim_Type", SqlDbType.NVarChar, 50) { Value = dimType },
                    new SqlParameter("@Dim_ID", SqlDbType.Int) { Value = dimID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
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
                                Dim_Type,
                                Dim_ID,
                                Validation_Type,
                                Is_Active,
                                Severity,
                                Config_JSON,
                                Create_Date,
                                Create_User,
                                Update_Date,
                                Update_User
                            FROM MDM_Validation_Config
                            WHERE Is_Active = 1
                            ORDER BY Dim_Type, Dim_ID, Name";
                
                var sqlparams = new SqlParameter[] { };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Config_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Validation Result Methods"

        public void Fill_MDM_Validation_Result_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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
                        bulkCopy.DestinationTableName = "MDM_Validation_Result";
                        bulkCopy.BatchSize = 1000;
                        
                        // Map columns explicitly
                        bulkCopy.ColumnMappings.Add("Validation_Config_ID", "Validation_Config_ID");
                        bulkCopy.ColumnMappings.Add("Run_ID", "Run_ID");
                        bulkCopy.ColumnMappings.Add("Run_Date", "Run_Date");
                        bulkCopy.ColumnMappings.Add("Run_User", "Run_User");
                        bulkCopy.ColumnMappings.Add("Member_Name", "Member_Name");
                        bulkCopy.ColumnMappings.Add("Member_ID", "Member_ID");
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
                                vr.Validation_Result_ID,
                                vr.Validation_Config_ID,
                                vc.Name as Validation_Name,
                                vc.Validation_Type,
                                vc.Severity,
                                vr.Run_ID,
                                vr.Run_Date,
                                vr.Run_User,
                                vr.Member_Name,
                                vr.Member_ID,
                                vr.Validation_Status,
                                vr.Error_Message,
                                vr.Error_Details,
                                vr.Create_Date
                            FROM MDM_Validation_Result vr
                            INNER JOIN MDM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID
                            WHERE vr.Run_ID = @Run_ID
                            ORDER BY vc.Severity DESC, vr.Member_Name";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Result_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Results_By_Config_ID(SessionInfo si, int validationConfigID, int? runID = null)
        {
            try
            {
                var dt = new DataTable("Validation_Results");
                var sql = @"SELECT 
                                vr.Validation_Result_ID,
                                vr.Validation_Config_ID,
                                vc.Name as Validation_Name,
                                vc.Validation_Type,
                                vc.Severity,
                                vr.Run_ID,
                                vr.Run_Date,
                                vr.Run_User,
                                vr.Member_Name,
                                vr.Member_ID,
                                vr.Validation_Status,
                                vr.Error_Message,
                                vr.Error_Details,
                                vr.Create_Date
                            FROM MDM_Validation_Result vr
                            INNER JOIN MDM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID
                            WHERE vr.Validation_Config_ID = @Validation_Config_ID";
                
                List<SqlParameter> sqlparams = new List<SqlParameter>
                {
                    new SqlParameter("@Validation_Config_ID", SqlDbType.Int) { Value = validationConfigID }
                };

                if (runID.HasValue)
                {
                    sql += " AND vr.Run_ID = @Run_ID";
                    sqlparams.Add(new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID.Value });
                }

                sql += " ORDER BY vr.Run_Date DESC, vr.Member_Name";

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Result_DT(si, sqa, dt, sql, sqlparams.ToArray());
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public int Delete_Validation_Results_By_Run_ID(SessionInfo si, int runID)
        {
            try
            {
                var sql = "DELETE FROM MDM_Validation_Result WHERE Run_ID = @Run_ID";
                using (SqlCommand command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.Add(new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID });
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public int Delete_Old_Validation_Results(SessionInfo si, int daysToKeep)
        {
            try
            {
                var sql = "DELETE FROM MDM_Validation_Result WHERE Run_Date < DATEADD(day, @DaysToKeep, GETDATE())";
                using (SqlCommand command = new SqlCommand(sql, _connection))
                {
                    command.Parameters.Add(new SqlParameter("@DaysToKeep", SqlDbType.Int) { Value = -daysToKeep });
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Validation Run Methods"

        public void Fill_MDM_Validation_Run_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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
        public void Update_MDM_Validation_Run(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "MDM_Validation_Run", dt);
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
                                Dim_Type,
                                Dim_ID,
                                Total_Validations,
                                Total_Members_Checked,
                                Total_Failures,
                                Total_Warnings,
                                Execution_Time_Ms,
                                Status,
                                Notes,
                                Create_Date
                            FROM MDM_Validation_Run
                            WHERE Run_ID = @Run_ID";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Run_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Recent_Validation_Runs(SessionInfo si, int topN = 50)
        {
            try
            {
                var dt = new DataTable("Validation_Runs");
                var sql = @"SELECT TOP (@TopN)
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Dim_Type,
                                Dim_ID,
                                Total_Validations,
                                Total_Members_Checked,
                                Total_Failures,
                                Total_Warnings,
                                Execution_Time_Ms,
                                Status,
                                Notes,
                                Create_Date
                            FROM MDM_Validation_Run
                            ORDER BY Run_Date DESC";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@TopN", SqlDbType.Int) { Value = topN }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Run_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public DataTable Get_Validation_Runs_By_Dimension(SessionInfo si, string dimType, int dimID)
        {
            try
            {
                var dt = new DataTable("Validation_Runs");
                var sql = @"SELECT 
                                Run_ID,
                                Run_Date,
                                Run_User,
                                Dim_Type,
                                Dim_ID,
                                Total_Validations,
                                Total_Members_Checked,
                                Total_Failures,
                                Total_Warnings,
                                Execution_Time_Ms,
                                Status,
                                Notes,
                                Create_Date
                            FROM MDM_Validation_Run
                            WHERE Dim_Type = @Dim_Type AND Dim_ID = @Dim_ID
                            ORDER BY Run_Date DESC";
                
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Dim_Type", SqlDbType.NVarChar, 50) { Value = dimType },
                    new SqlParameter("@Dim_ID", SqlDbType.Int) { Value = dimID }
                };

                var sqa = new SqlDataAdapter();
                Fill_MDM_Validation_Run_DT(si, sqa, dt, sql, sqlparams);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion
    }
}
