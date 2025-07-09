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
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_FMM_Act_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Act_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Act_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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


        public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
		            INSERT INTO FMM_Act_Config (
		                Cube_ID,Act_ID,Name,Calc_Type,
						Status,Create_Date,Create_User,Update_Date,Update_User )
		            VALUES
		                (@Cube_ID,@Act_ID,@Name,@Calc_Type,
						@Status,@Create_Date,@Create_User,@Update_Date,@Update_User )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.InsertCommand.Parameters.Add("@Calc_Type", SqlDbType.NVarChar, 20).SourceColumn = "Calc_Type";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
							            UPDATE FMM_Act_Config SET
							                Name = @Name,
							                Calc_Type = @Calc_Type,
							                Status = @Status,
											Update_Date = @Update_Date,
											Update_User = @Update_User
							            WHERE Act_ID = @Act_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Act_ID", SqlDbType.Int) { SourceColumn = "Act_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.UpdateCommand.Parameters.Add("@Calc_Type", SqlDbType.NVarChar, 20).SourceColumn = "Calc_Type";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
							            DELETE FROM FMM_Act_Config 
							            WHERE Act_ID = @Act_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Act_ID", SqlDbType.Int) { SourceColumn = "Act_ID", SourceVersion = DataRowVersion.Original });

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