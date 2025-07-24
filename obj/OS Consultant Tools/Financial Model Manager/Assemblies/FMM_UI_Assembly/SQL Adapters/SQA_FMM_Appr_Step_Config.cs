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
    public class SQA_FMM_Appr_Step_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Appr_Step_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Appr_Step_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_FMM_Appr_Step_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            sqa.UpdateBatchSize = 0; // Set batch size for performance
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                    INSERT INTO FMM_Appr_Step_Config (
                        Cube_ID, Appr_ID, Appr_Step_ID, Step_Num,
                        WFProfile_Step, User_Group, Logic, Item,
                        Level, Appr_Config, Init_Status, Appr_Status,
                        Rej_Status, Status, Create_Date, Create_User, Update_Date, Update_User)
                    VALUES
                        (@Cube_ID, @Appr_ID, @Appr_Step_ID, @Step_Num,
                        @WFProfile_Step, @User_Group, @Logic, @Item,
                        @Level, @Appr_Config, @Init_Status, @Appr_Status,
                        @Rej_Status, @Status, @Create_Date, @Create_User, @Update_Date, @Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Appr_ID", SqlDbType.Int).SourceColumn = "Appr_ID";
                sqa.InsertCommand.Parameters.Add("@Appr_Step_ID", SqlDbType.Int).SourceColumn = "Appr_Step_ID";
                sqa.InsertCommand.Parameters.Add("@Step_Num", SqlDbType.Int).SourceColumn = "Step_Num";
                sqa.InsertCommand.Parameters.Add("@WFProfile_Step", SqlDbType.NVarChar, 100).SourceColumn = "WFProfile_Step";
                sqa.InsertCommand.Parameters.Add("@User_Group", SqlDbType.NVarChar, 250).SourceColumn = "User_Group";
                sqa.InsertCommand.Parameters.Add("@Logic", SqlDbType.NVarChar, 100).SourceColumn = "Logic";
                sqa.InsertCommand.Parameters.Add("@Item", SqlDbType.NVarChar, 100).SourceColumn = "Item";
                sqa.InsertCommand.Parameters.Add("@Level", SqlDbType.Int).SourceColumn = "Level";
                sqa.InsertCommand.Parameters.Add("@Appr_Config", SqlDbType.Int).SourceColumn = "Appr_Config";
                sqa.InsertCommand.Parameters.Add("@Init_Status", SqlDbType.NVarChar, 100).SourceColumn = "Init_Status";
                sqa.InsertCommand.Parameters.Add("@Appr_Status", SqlDbType.NVarChar, 100).SourceColumn = "Appr_Status";
                sqa.InsertCommand.Parameters.Add("@Rej_Status", SqlDbType.NVarChar, 100).SourceColumn = "Rej_Status";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
                    UPDATE FMM_Appr_Step_Config SET
                        Step_Num = @Step_Num,
                        WFProfile_Step = @WFProfile_Step,
                        User_Group = @User_Group,
                        Logic = @Logic,
                        Item = @Item,
                        Level = @Level,
                        Appr_Config = @Appr_Config,
                        Init_Status = @Init_Status,
                        Appr_Status = @Appr_Status,
                        Rej_Status = @Rej_Status,
                        Status = @Status,
                        Update_Date = @Update_Date,
                        Update_User = @Update_User
                    WHERE Appr_Step_ID = @Appr_Step_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { SourceColumn = "Appr_Step_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Step_Num", SqlDbType.Int).SourceColumn = "Step_Num";
                sqa.UpdateCommand.Parameters.Add("@WFProfile_Step", SqlDbType.NVarChar, 100).SourceColumn = "WFProfile_Step";
                sqa.UpdateCommand.Parameters.Add("@User_Group", SqlDbType.NVarChar, 250).SourceColumn = "User_Group";
                sqa.UpdateCommand.Parameters.Add("@Logic", SqlDbType.NVarChar, 100).SourceColumn = "Logic";
                sqa.UpdateCommand.Parameters.Add("@Item", SqlDbType.NVarChar, 100).SourceColumn = "Item";
                sqa.UpdateCommand.Parameters.Add("@Level", SqlDbType.Int).SourceColumn = "Level";
                sqa.UpdateCommand.Parameters.Add("@Appr_Config", SqlDbType.Int).SourceColumn = "Appr_Config";
                sqa.UpdateCommand.Parameters.Add("@Init_Status", SqlDbType.NVarChar, 100).SourceColumn = "Init_Status";
                sqa.UpdateCommand.Parameters.Add("@Appr_Status", SqlDbType.NVarChar, 100).SourceColumn = "Appr_Status";
                sqa.UpdateCommand.Parameters.Add("@Rej_Status", SqlDbType.NVarChar, 100).SourceColumn = "Rej_Status";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
                    DELETE FROM FMM_Appr_Step_Config 
                    WHERE Appr_Step_ID = @Appr_Step_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { SourceColumn = "Appr_Step_ID", SourceVersion = DataRowVersion.Original });

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