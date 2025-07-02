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
    public class SQA_DDM_Config_Menu
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_DDM_Config_Menu_DataTable(SessionInfo si, SqlDataAdapter adapter, DataTable dataTable, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                adapter.SelectCommand = command;
                adapter.Fill(dataTable);
            }
        }

        public void Update_DDM_Config_Menu(SessionInfo si, DataTable dataTable, SqlDataAdapter adapter)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
	            INSERT INTO DDM_Config_Menu
			           (DDM_Profile_ID
			           ,DDM_Menu_ID
			           ,DDM_Menu_Order
			           ,DDM_Menu_Name
			           ,DDM_Menu_Custom_DB
			           ,DDM_Menu_Custom_DB_Header
			           ,DDM_Menu_Custom_DB_Content
			           ,DDM_Menu_DB
			           ,DDM_Menu_DB_Name
			           ,DDM_Menu_CV
			           ,DDM_Menu_CV_Name
			           ,Status
			           ,Create_Date
			           ,Create_User
			           ,Update_Date
			           ,Update_User)
					VALUES
		                (@DDM_Profile_ID
			           ,DDM_Menu_ID
			           ,DDM_Menu_Order
			           ,DDM_Menu_Name
			           ,DDM_Menu_Custom_DB
			           ,DDM_Menu_Custom_DB_Header
			           ,DDM_Menu_Custom_DB_Content
			           ,DDM_Menu_DB
			           ,DDM_Menu_DB_Name
			           ,DDM_Menu_CV
			           ,DDM_Menu_CV_Name
			           ,Status
			           ,Create_Date
			           ,Create_User
			           ,Update_Date
			           ,Update_User)";

                adapter.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                adapter.InsertCommand.Parameters.Add("@DDM_Profile_ID", SqlDbType.Int).SourceColumn = "DDM_Profile_ID";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_Order", SqlDbType.Int).SourceColumn = "DDM_Menu_Order";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Name";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_Custom_DB", SqlDbType.Bit).SourceColumn = "DDM_Menu_Custom_DB";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_Custom_DB_Header", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Custom_DB_Header";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_Custom_DB_Content", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Custom_DB_Content";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_DB", SqlDbType.Bit).SourceColumn = "DDM_Menu_DB";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_DB_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_DB_Name";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_CV", SqlDbType.Bit).SourceColumn = "DDM_Menu_CV";
                adapter.InsertCommand.Parameters.Add("@DDM_Menu_CV_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_CV_Name";
                adapter.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar).SourceColumn = "Status";
                adapter.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                adapter.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar).SourceColumn = "Create_User";
                adapter.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                adapter.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar).SourceColumn = "Update_User";

                // Define the update query and parameters (similar to the insert query, but with a WHERE clause)
                string updateQuery = @"
								   UPDATE DDM_Config_Menu
								   SET DDM_Menu_Order = @DDM_Menu_Order
								      ,DDM_Menu_Name = @DDM_Menu_Name
								      ,DDM_Menu_Custom_DB = @DDM_Menu_Custom_DB
								      ,DDM_Menu_Custom_DB_Header = @DDM_Menu_Custom_DB_Header
								      ,DDM_Menu_Custom_DB_Content = @DDM_Menu_Custom_DB_Content
								      ,DDM_Menu_DB = @DDM_Menu_DB
								      ,DDM_Menu_DB_Name = @DDM_Menu_DB_Name
								      ,DDM_Menu_CV = @DDM_Menu_CV
								      ,DDM_Menu_CV_Name = @DDM_Menu_CV_Name
								      ,Status = @Status
								      ,Update_Date = @Update_Date
								      ,Update_User = @Update_User
								 WHERE DDM_Menu_ID = @DDM_Menu_ID
								 AND DDM_Profile_ID = @DDM_Profile_ID";

                adapter.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                adapter.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { SourceColumn = "DDM_Profile_ID", SourceVersion = DataRowVersion.Original });
                adapter.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_ID", SourceVersion = DataRowVersion.Original });
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_Order", SqlDbType.Int).SourceColumn = "DDM_Menu_Order";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Name";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_Custom_DB", SqlDbType.Bit).SourceColumn = "DDM_Menu_Custom_DB";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_Custom_DB_Header", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Custom_DB_Header";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_Custom_DB_Content", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Custom_DB_Content";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_DB", SqlDbType.Bit).SourceColumn = "DDM_Menu_DB";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_DB_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_DB_Name";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_CV", SqlDbType.Bit).SourceColumn = "DDM_Menu_CV";
                adapter.UpdateCommand.Parameters.Add("@DDM_Menu_CV_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_CV_Name";
                adapter.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar).SourceColumn = "Status";
                adapter.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                adapter.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
		            DELETE FROM [dbo].[DDM_Config_Menu]
		            WHERE [DDM_Profile_ID] = @DDM_Profile_ID AND [DDM_Menu_ID] = @DDM_Menu_ID";

                adapter.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { SourceColumn = "DDM_Profile_ID", SourceVersion = DataRowVersion.Original });
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_ID", SourceVersion = DataRowVersion.Original });

                try
                {
                    adapter.Update(dataTable);
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