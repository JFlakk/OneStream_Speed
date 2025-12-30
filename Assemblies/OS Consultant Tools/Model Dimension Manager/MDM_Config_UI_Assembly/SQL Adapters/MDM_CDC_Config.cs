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
    public class SQA_MDM_CDC_Config
    {
        private readonly SqlConnection _connection;

        public SQA_MDM_CDC_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_MDM_CDC_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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

        public void Update_MDM_CDC_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query
                string insertQuery = @"
                    INSERT INTO MDM_CDC_Config
                           (CDC_Config_ID
                           ,Name
                           ,Dim_Type
                           ,Dim_ID
                           ,Src_Connection
                           ,Src_SQL_String
                           ,Dim_Mgmt_Process
                           ,Trx_Rule
                           ,Appr_ID
                           ,Mbr_PrefSuff
                           ,Mbr_PrefSuff_Txt
                           ,Create_Date
                           ,Create_User
                           ,Update_Date
                           ,Update_User)
                     VALUES
                           (@CDC_Config_ID
                           ,@Name
                           ,@Dim_Type
                           ,@Dim_ID
                           ,@Src_Connection
                           ,@Src_SQL_String
                           ,@Dim_Mgmt_Process
                           ,@Trx_Rule
                           ,@Appr_ID
                           ,@Mbr_PrefSuff
                           ,@Mbr_PrefSuff_Txt
                           ,@Create_Date
                           ,@Create_User
                           ,@Update_Date
                           ,@Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@CDC_Config_ID", SqlDbType.Int).SourceColumn = "CDC_Config_ID";
                sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.InsertCommand.Parameters.Add("@Dim_Type", SqlDbType.NVarChar, 50).SourceColumn = "Dim_Type";
                sqa.InsertCommand.Parameters.Add("@Dim_ID", SqlDbType.Int).SourceColumn = "Dim_ID";
                sqa.InsertCommand.Parameters.Add("@Src_Connection", SqlDbType.NVarChar, 200).SourceColumn = "Src_Connection";
                sqa.InsertCommand.Parameters.Add("@Src_SQL_String", SqlDbType.NVarChar, -1).SourceColumn = "Src_SQL_String";
                sqa.InsertCommand.Parameters.Add("@Dim_Mgmt_Process", SqlDbType.NVarChar, -1).SourceColumn = "Dim_Mgmt_Process";
                sqa.InsertCommand.Parameters.Add("@Trx_Rule", SqlDbType.NVarChar, -1).SourceColumn = "Trx_Rule";
                sqa.InsertCommand.Parameters.Add("@Appr_ID", SqlDbType.Int).SourceColumn = "Appr_ID";
                sqa.InsertCommand.Parameters.Add("@Mbr_PrefSuff", SqlDbType.NVarChar, 20).SourceColumn = "Mbr_PrefSuff";
                sqa.InsertCommand.Parameters.Add("@Mbr_PrefSuff_Txt", SqlDbType.NVarChar, -1).SourceColumn = "Mbr_PrefSuff_Txt";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query
                string updateQuery = @"
                    UPDATE MDM_CDC_Config
                       SET Name = @Name
                          ,Dim_Type = @Dim_Type
                          ,Dim_ID = @Dim_ID
                          ,Src_Connection = @Src_Connection
                          ,Src_SQL_String = @Src_SQL_String
                          ,Dim_Mgmt_Process = @Dim_Mgmt_Process
                          ,Trx_Rule = @Trx_Rule
                          ,Appr_ID = @Appr_ID
                          ,Mbr_PrefSuff = @Mbr_PrefSuff
                          ,Mbr_PrefSuff_Txt = @Mbr_PrefSuff_Txt
                          ,Update_Date = @Update_Date
                          ,Update_User = @Update_User
                     WHERE CDC_Config_ID = @CDC_Config_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { SourceColumn = "CDC_Config_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.UpdateCommand.Parameters.Add("@Dim_Type", SqlDbType.NVarChar, 50).SourceColumn = "Dim_Type";
                sqa.UpdateCommand.Parameters.Add("@Dim_ID", SqlDbType.Int).SourceColumn = "Dim_ID";
                sqa.UpdateCommand.Parameters.Add("@Src_Connection", SqlDbType.NVarChar, 200).SourceColumn = "Src_Connection";
                sqa.UpdateCommand.Parameters.Add("@Src_SQL_String", SqlDbType.NVarChar, -1).SourceColumn = "Src_SQL_String";
                sqa.UpdateCommand.Parameters.Add("@Dim_Mgmt_Process", SqlDbType.NVarChar, -1).SourceColumn = "Dim_Mgmt_Process";
                sqa.UpdateCommand.Parameters.Add("@Trx_Rule", SqlDbType.NVarChar, -1).SourceColumn = "Trx_Rule";
                sqa.UpdateCommand.Parameters.Add("@Appr_ID", SqlDbType.Int).SourceColumn = "Appr_ID";
                sqa.UpdateCommand.Parameters.Add("@Mbr_PrefSuff", SqlDbType.NVarChar, 20).SourceColumn = "Mbr_PrefSuff";
                sqa.UpdateCommand.Parameters.Add("@Mbr_PrefSuff_Txt", SqlDbType.NVarChar, -1).SourceColumn = "Mbr_PrefSuff_Txt";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query
                string deleteQuery = @"
                    DELETE FROM MDM_CDC_Config
                    WHERE CDC_Config_ID = @CDC_Config_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { SourceColumn = "CDC_Config_ID", SourceVersion = DataRowVersion.Original });

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