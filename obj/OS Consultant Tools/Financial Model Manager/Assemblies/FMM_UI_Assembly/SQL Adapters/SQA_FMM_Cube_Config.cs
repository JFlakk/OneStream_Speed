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
    public class SQA_FMM_Cube_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Cube_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Cube_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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


        public void Update_FMM_Cube_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
		            INSERT INTO FMM_Cube_Config (
		                Cube_ID, Cube, Scen_Type, Descr, Entity_Dim, Entity_MFB, Agg_Consol,  
						Status,Create_Date,Create_User,Update_Date,Update_User
		            ) VALUES (
		                @Cube_ID, @Cube, @Scen_Type, @Descr,@Entity_Dim, @Entity_MFB,@Agg_Consol,
						@Status,@Create_Date,@Create_User,@Update_Date,@Update_User
		            )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Cube", SqlDbType.NVarChar, 50).SourceColumn = "Cube";
                sqa.InsertCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.InsertCommand.Parameters.Add("@Descr", SqlDbType.NVarChar, 100).SourceColumn = "Descr";
                sqa.InsertCommand.Parameters.Add("@Entity_Dim", SqlDbType.NVarChar, 50).SourceColumn = "Entity_Dim";
                sqa.InsertCommand.Parameters.Add("@Entity_MFB", SqlDbType.NVarChar, 100).SourceColumn = "Entity_MFB";
                sqa.InsertCommand.Parameters.Add("@Agg_Consol", SqlDbType.NVarChar, 15).SourceColumn = "Agg_Consol";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
		            UPDATE FMM_Cube_Config SET
		                Cube = @Cube,
		                Scen_Type = @Scen_Type,
						Descr = @Descr,
		                Entity_Dim = @Entity_Dim,
						Entity_MFB = @Entity_MFB,
		                Agg_Consol = @Agg_Consol,
						Status = @Status,
						Update_Date = @Update_Date,
						Update_User = @Update_User
		            WHERE Cube_ID = @Cube_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Cube_ID", SqlDbType.Int) { SourceColumn = "Cube_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube", SqlDbType.NVarChar, 50).SourceColumn = "Cube";
                sqa.UpdateCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.UpdateCommand.Parameters.Add("@Descr", SqlDbType.NVarChar, 100).SourceColumn = "Descr";
                sqa.UpdateCommand.Parameters.Add("@Entity_Dim", SqlDbType.NVarChar, 50).SourceColumn = "Entity_Dim";
                sqa.UpdateCommand.Parameters.Add("@Entity_MFB", SqlDbType.NVarChar, 100).SourceColumn = "Entity_MFB";
                sqa.UpdateCommand.Parameters.Add("@Agg_Consol", SqlDbType.NVarChar, 15).SourceColumn = "Agg_Consol";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
		            DELETE FROM FMM_Cube_Config 
		            WHERE Cube_ID = @Cube_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Cube_ID", SqlDbType.Int) { SourceColumn = "Cube_ID", SourceVersion = DataRowVersion.Original });

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