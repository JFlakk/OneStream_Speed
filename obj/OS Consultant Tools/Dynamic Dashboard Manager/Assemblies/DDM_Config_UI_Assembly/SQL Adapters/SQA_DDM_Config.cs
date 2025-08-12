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
    public class SQA_DDM_Config
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_DDM_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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

        public void Update_DDM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert command and sqlparams
                string insertQuery = @"
                    INSERT INTO DDM_Config (
                         DDM_Config_ID, DDM_Type, Scen_Type, Profile_Key, Profile_Step_Type, Workspace_ID, MaintUnit_ID, Status, Create_Date, Create_User, Update_Date, Update_User
                    ) VALUES (
                        @DDM_Config_ID, @DDM_Type, @Scen_Type, @Profile_Key, @Profile_Step_Type, @Workspace_ID, @MaintUnit_ID, @Status, @Create_Date, @Create_User, @Update_Date, @Update_User
                    );";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
                sqa.InsertCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.InsertCommand.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
                sqa.InsertCommand.Parameters.Add("@Profile_Step_Type", SqlDbType.NVarChar, 20).SourceColumn = "Profile_Step_Type";
                sqa.InsertCommand.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
                sqa.InsertCommand.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update command and sqlparams
                string updateQuery = @"
                    UPDATE dbo.DDM_Config SET
                         DDM_Type = @DDM_Type,
                         Scen_Type = @Scen_Type,
                         Profile_Key = @Profile_Key,
                         Profile_Step_Type = @Profile_Step_Type,
                         Workspace_ID = @Workspace_ID,
                         MaintUnit_ID = @MaintUnit_ID,
                         Status = @Status,
                         Create_Date = @Create_Date,
                         Create_User = @Create_User,
                         Update_Date = @Update_Date,
                         Update_User = @Update_User
                    WHERE DDM_Config_ID = @DDM_Config_ID;";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { SourceColumn = "DDM_Config_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
                sqa.UpdateCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.UpdateCommand.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
                sqa.UpdateCommand.Parameters.Add("@Profile_Step_Type", SqlDbType.NVarChar, 20).SourceColumn = "Profile_Step_Type";
                sqa.UpdateCommand.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
                sqa.UpdateCommand.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.UpdateCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete command and sqlparams
                string deleteQuery = @"
                    DELETE FROM dbo.DDM_Config 
                    WHERE DDM_Config_ID = @DDM_Config_ID;";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { SourceColumn = "DDM_Config_ID", SourceVersion = DataRowVersion.Original });

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