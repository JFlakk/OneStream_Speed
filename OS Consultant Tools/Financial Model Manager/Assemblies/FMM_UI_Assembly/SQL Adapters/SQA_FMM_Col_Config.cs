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
    public class SQA_FMM_Col_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Col_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Col_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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
            }
        }

        public void Update_FMM_Col_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                    INSERT INTO FMM_Col_Config (
                        Cube_ID, Act_ID, Reg_Config_ID, Col_ID, Name, InUse, Required, Updates, 
                        Alias, [Order], [Default], Param, Format, Filter_Param, Create_Date, Create_User, Update_Date, Update_User)
                    VALUES (
                        @Cube_ID, @Act_ID, @Reg_Config_ID, @Col_ID, @Name, @InUse, @Required, @Updates, 
                        @Alias, @Order, @Default, @Param, @Format, @Filter_Param, @Create_Date, @Create_User, @Update_Date, @Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Reg_Config_ID", SqlDbType.Int).SourceColumn = "Reg_Config_ID";
                sqa.InsertCommand.Parameters.Add("@Col_ID", SqlDbType.Int).SourceColumn = "Col_ID";
                sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.InsertCommand.Parameters.Add("@InUse", SqlDbType.Bit).SourceColumn = "InUse";
                sqa.InsertCommand.Parameters.Add("@Required", SqlDbType.Bit).SourceColumn = "Required";
                sqa.InsertCommand.Parameters.Add("@Updates", SqlDbType.Bit).SourceColumn = "Updates";
                sqa.InsertCommand.Parameters.Add("@Alias", SqlDbType.NVarChar, 100).SourceColumn = "Alias";
                sqa.InsertCommand.Parameters.Add("@Order", SqlDbType.Int).SourceColumn = "Order";
                sqa.InsertCommand.Parameters.Add("@Default", SqlDbType.NVarChar, 250).SourceColumn = "Default";
                sqa.InsertCommand.Parameters.Add("@Param", SqlDbType.NVarChar, 250).SourceColumn = "Param";
                sqa.InsertCommand.Parameters.Add("@Format", SqlDbType.NVarChar, 100).SourceColumn = "Format";
                sqa.InsertCommand.Parameters.Add("@Filter_Param", SqlDbType.NVarChar, 250).SourceColumn = "Filter_Param";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
                    UPDATE FMM_Col_Config SET
                        Name = @Name,
                        InUse = @InUse,
                        Required = @Required,
                        Updates = @Updates,
                        Alias = @Alias,
                        [Order] = @Order,
                        [Default] = @Default,
                        Param = @Param,
                        Format = @Format,
						Filter_Param = @Filter_Param,
                        Update_Date = @Update_Date,
                        Update_User = @Update_User
                    WHERE Col_ID = @Col_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Col_ID", SqlDbType.Int) { SourceColumn = "Col_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.UpdateCommand.Parameters.Add("@InUse", SqlDbType.Bit).SourceColumn = "InUse";
                sqa.UpdateCommand.Parameters.Add("@Required", SqlDbType.Bit).SourceColumn = "Required";
                sqa.UpdateCommand.Parameters.Add("@Updates", SqlDbType.Bit).SourceColumn = "Updates";
                sqa.UpdateCommand.Parameters.Add("@Alias", SqlDbType.NVarChar, 100).SourceColumn = "Alias";
                sqa.UpdateCommand.Parameters.Add("@Order", SqlDbType.Int).SourceColumn = "Order";
                sqa.UpdateCommand.Parameters.Add("@Default", SqlDbType.NVarChar, 250).SourceColumn = "Default";
                sqa.UpdateCommand.Parameters.Add("@Param", SqlDbType.NVarChar, 250).SourceColumn = "Param";
                sqa.UpdateCommand.Parameters.Add("@Format", SqlDbType.NVarChar, 100).SourceColumn = "Format";
                sqa.UpdateCommand.Parameters.Add("@Filter_Param", SqlDbType.NVarChar, 250).SourceColumn = "Filter_Param";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
                    DELETE FROM FMM_Col_Config 
                    WHERE Col_ID = @Col_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Col_ID", SqlDbType.Int) { SourceColumn = "Col_ID", SourceVersion = DataRowVersion.Original });

                try
                {
                    sqa.Update(dt);
                    transaction.Commit();
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