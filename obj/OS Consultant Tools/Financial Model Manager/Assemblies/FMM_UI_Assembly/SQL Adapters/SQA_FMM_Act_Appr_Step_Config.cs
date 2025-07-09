﻿using System;
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
    public class SQA_FMM_Act_Appr_Step_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Act_Appr_Step_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Act_Appr_Step_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
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

        public void Update_FMM_Act_Appr_Step_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                    INSERT INTO FMM_Act_Appr_Step_Config (
                        Appr_ID, Appr_Step_ID, Act_ID, Appr_Step_Act_ID, Reg_Config_ID, Descr, Acct, Flow, UD1, UD2, UD3, 
                        UD4, UD5, UD6, UD7, UD8, Create_Date, Create_User, Update_Date, Update_User
                    ) VALUES (
                        @Appr_ID, @Appr_Step_ID, @Act_ID, @Appr_Step_Act_ID, @Reg_Config_ID, @Descr, @Acct, @Flow, @UD1, @UD2, @UD3, 
                        @UD4, @UD5, @UD6, @UD7, @UD8, @Create_Date, @Create_User, @Update_Date, @Update_User
                    )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@Appr_ID", SqlDbType.Int).SourceColumn = "Appr_ID";
                sqa.InsertCommand.Parameters.Add("@Appr_Step_ID", SqlDbType.Int).SourceColumn = "Appr_Step_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Appr_Step_Act_ID", SqlDbType.Int).SourceColumn = "Appr_Step_Act_ID";
                sqa.InsertCommand.Parameters.Add("@Reg_Config_ID", SqlDbType.Int).SourceColumn = "Reg_Config_ID";
                sqa.InsertCommand.Parameters.Add("@Descr", SqlDbType.NVarChar, 100).SourceColumn = "Descr";
                sqa.InsertCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.InsertCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                sqa.InsertCommand.Parameters.Add("@UD1", SqlDbType.NVarChar, 100).SourceColumn = "UD1";
                sqa.InsertCommand.Parameters.Add("@UD2", SqlDbType.NVarChar, 100).SourceColumn = "UD2";
                sqa.InsertCommand.Parameters.Add("@UD3", SqlDbType.NVarChar, 100).SourceColumn = "UD3";
                sqa.InsertCommand.Parameters.Add("@UD4", SqlDbType.NVarChar, 100).SourceColumn = "UD4";
                sqa.InsertCommand.Parameters.Add("@UD5", SqlDbType.NVarChar, 100).SourceColumn = "UD5";
                sqa.InsertCommand.Parameters.Add("@UD6", SqlDbType.NVarChar, 100).SourceColumn = "UD6";
                sqa.InsertCommand.Parameters.Add("@UD7", SqlDbType.NVarChar, 100).SourceColumn = "UD7";
                sqa.InsertCommand.Parameters.Add("@UD8", SqlDbType.NVarChar, 100).SourceColumn = "UD8";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
                    UPDATE FMM_Act_Appr_Step_Config SET
						Act_ID = @Act_ID,
						Reg_Config_ID = @Reg_Config_ID,
						Descr = @Descr,
                        Acct = @Acct,
                        Flow = @Flow,
                        UD1 = @UD1,
                        UD2 = @UD2,
                        UD3 = @UD3,
                        UD4 = @UD4,
                        UD5 = @UD5,
                        UD6 = @UD6,
                        UD7 = @UD7,
                        UD8 = @UD8,
                        Update_Date = @Update_Date,
                        Update_User = @Update_User
                    WHERE Appr_Step_Act_ID = @Appr_Step_Act_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Appr_Step_Act_ID", SqlDbType.Int) { SourceColumn = "Appr_Step_Act_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Act_ID", SqlDbType.NVarChar, 100).SourceColumn = "Act_ID";
                sqa.UpdateCommand.Parameters.Add("@Reg_Config_ID", SqlDbType.Int).SourceColumn = "Reg_Config_ID";
                sqa.UpdateCommand.Parameters.Add("@Descr", SqlDbType.NVarChar, 100).SourceColumn = "Descr";
                sqa.UpdateCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.UpdateCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                sqa.UpdateCommand.Parameters.Add("@UD1", SqlDbType.NVarChar, 100).SourceColumn = "UD1";
                sqa.UpdateCommand.Parameters.Add("@UD2", SqlDbType.NVarChar, 100).SourceColumn = "UD2";
                sqa.UpdateCommand.Parameters.Add("@UD3", SqlDbType.NVarChar, 100).SourceColumn = "UD3";
                sqa.UpdateCommand.Parameters.Add("@UD4", SqlDbType.NVarChar, 100).SourceColumn = "UD4";
                sqa.UpdateCommand.Parameters.Add("@UD5", SqlDbType.NVarChar, 100).SourceColumn = "UD5";
                sqa.UpdateCommand.Parameters.Add("@UD6", SqlDbType.NVarChar, 100).SourceColumn = "UD6";
                sqa.UpdateCommand.Parameters.Add("@UD7", SqlDbType.NVarChar, 100).SourceColumn = "UD7";
                sqa.UpdateCommand.Parameters.Add("@UD8", SqlDbType.NVarChar, 100).SourceColumn = "UD8";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
                    DELETE FROM FMM_Act_Appr_Step_Config 
                    WHERE Appr_Step_Act_ID = @Appr_Step_Act_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Appr_Step_Act_ID", SqlDbType.Int) { SourceColumn = "FMM_Act_Appr_Step_Config_ID", SourceVersion = DataRowVersion.Original });

                try
                {
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
    }
}