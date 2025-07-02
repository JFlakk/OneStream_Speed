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
    public class SQA_DDM_Config_Menu_Header
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu_Header(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_DDM_Config_Menu_Header_DataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dataTable, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dataTable);
            }
        }

        public void Update_DDM_Config_Menu_Header(SessionInfo si, DataTable dataTable, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
									INSERT INTO DDM_Config_Menu_Header
									           (DDM_Profile_ID
									           ,DDM_Menu_ID
									           ,DDM_Menu_Header_ID
									           ,DDM_Menu_Option_Header_Sort_Order
									           ,DDM_Profile_Menu_Header_Option_Type
									           ,DDM_Menu_Header_Filter_Dim_Type
									           ,DDM_Menu_Header_Filter_Dim_Name
									           ,DDM_Menu_Header_Filter_MFB
									           ,DDM_Menu_Header_Filter_Default
									           ,DDM_Menu_Header_Filter_Btn
									           ,DDM_Menu_Header_Filter_Btn_Lbl
									           ,DDM_Menu_Header_Filter_Cbx
									           ,DDM_Menu_Header_Filter_Cbx_Lbl
									           ,DDM_Menu_Header_Filter_Txt
									           ,DDM_Menu_Header_Filter_Txt_Lbl
									           ,DDM_Menu_Header_Option_Btn_Type
									           ,DDM_Menu_Header_Btn_Lbl
									           ,DDM_Menu_Header_Btn_ToolTip
									           ,DDM_Menu_Header_Btn_Image_URL
									           ,DDM_Menu_Header_Btn_ServerTask
									           ,DDM_Menu_Header_Btn_DBOpen
									           ,DDM_Menu_Header_Btn_DBRefresh
									           ,Create_Date
									           ,Create_User
									           ,Update_Date
									           ,Update_User)
									     VALUES
									           (@DDM_Profile_ID
									           ,@DDM_Menu_ID
									           ,@DDM_Menu_Header_ID
									           ,@DDM_Menu_Option_Header_Sort_Order
									           ,@DDM_Menu_Header_Option_Type
									           ,@DDM_Menu_Header_Filter_Dim_Type
									           ,@DDM_Menu_Header_Filter_Dim_Name
									           ,@DDM_Menu_Header_Filter_MFB
									           ,@DDM_Menu_Header_Filter_Default
									           ,@DDM_Menu_Header_Filter_Btn
									           ,@DDM_Menu_Header_Filter_Btn_Lbl
									           ,@DDM_Menu_Header_Filter_Cbx
									           ,@DDM_Menu_Header_Filter_Cbx_Lbl
									           ,@DDM_Menu_Header_Filter_Txt
									           ,@DDM_Menu_Header_Filter_Txt_Lbl
									           ,@DDM_Menu_Header_Option_Btn_Type
									           ,@DDM_Menu_Header_Btn_Lbl
									           ,@DDM_Menu_Header_Btn_ToolTip
									           ,@DDM_Menu_Header_Btn_Image_URL
									           ,@DDM_Menu_Header_Btn_ServerTask
									           ,@DDM_Menu_Header_Btn_DBOpen
									           ,@DDM_Menu_Header_Btn_DBRefresh
									           ,@Create_Date
									           ,@Create_User
									           ,@Update_Date
									           ,@Update_User)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@DDM_Profile_ID", SqlDbType.Int).SourceColumn = "DDM_Profile_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_Header_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Order", SqlDbType.Int).SourceColumn = "DDM_Menu_Header_Order";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Option_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Option_Type";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Dim_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Dim_Type";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Dim_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Dim_Name";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_MFB", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_MFB";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Default", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Default";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Btn";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Btn_Lbl";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Btn_ToolTip";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Cbx";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Cbx_Lbl";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Cbx_ToolTip";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Txt";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Txt_Lbl";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Txt_ToolTip";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Option_Btn_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Option_Btn_Type";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_Lbl";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_ToolTip";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_Image_URL", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_Image_URL";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_ServerTask", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_ServerTask";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_DBOpen", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_DBOpen";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Header_Btn_DBRefresh", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_DBRefresh";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar).SourceColumn = "Update_User";

                // Define the update query and parameters (similar to the insert query, but with a WHERE clause)
                string updateQuery = @"
								UPDATE DDM_Config_Menu_Header
								   SET DDM_Menu_Option_Header_Sort_Order = @DDM_Menu_Option_Header_Sort_Order
								      ,DDM_Menu_Header_Option_Type = @DDM_Menu_Header_Option_Type
								      ,DDM_Menu_Header_Filter_Dim_Type = @DDM_Menu_Header_Filter_Dim_Type
								      ,DDM_Menu_Header_Filter_Dim_Name = @DDM_Menu_Header_Filter_Dim_Name
								      ,DDM_Menu_Header_Filter_MFB = @DDM_Menu_Header_Filter_MFB
								      ,DDM_Menu_Header_Filter_Default = @DDM_Menu_Header_Filter_Default
								      ,DDM_Menu_Header_Filter_Btn = @DDM_Menu_Header_Filter_Btn
								      ,DDM_Menu_Header_Filter_Btn_Lbl = @DDM_Menu_Header_Filter_Btn_Lbl
								      ,DDM_Menu_Header_Filter_Btn_ToolTip = @DDM_Menu_Header_Filter_Btn_ToolTip
								      ,DDM_Menu_Header_Filter_Cbx = @DDM_Menu_Header_Filter_Cbx
								      ,DDM_Menu_Header_Filter_Cbx_Lbl = @DDM_Menu_Header_Filter_Cbx_Lbl
								      ,DDM_Menu_Header_Filter_Cbx_ToolTip = @DDM_Menu_Header_Filter_Cbx_ToolTip
								      ,DDM_Menu_Header_Filter_Txt = @DDM_Menu_Header_Filter_Txt
								      ,DDM_Menu_Header_Filter_Txt_Lbl = @DDM_Menu_Header_Filter_Txt_Lbl
								      ,DDM_Menu_Header_Filter_Txt_ToolTip = @DDM_Menu_Header_Filter_Txt_ToolTip
								      ,DDM_Menu_Header_Option_Btn_Type = @DDM_Menu_Header_Option_Btn_Type
								      ,DDM_Menu_Header_Btn_Lbl = @DDM_Menu_Header_Btn_Lbl
								      ,DDM_Menu_Header_Btn_ToolTip = @DDM_Menu_Header_Btn_ToolTip
								      ,DDM_Menu_Header_Btn_Image_URL = @DDM_Menu_Header_Btn_Image_URL
								      ,DDM_Menu_Header_Btn_ServerTask = @DDM_Menu_Header_Btn_ServerTask
								      ,DDM_Menu_Header_Btn_DBOpen = @DDM_Menu_Header_Btn_DBOpen
								      ,DDM_Menu_Header_Btn_DBRefresh = @DDM_Menu_Header_Btn_DBRefresh
								      ,Update_Date = @Update_Date
								      ,Update_User = @Update_User
								 WHERE DDM_Menu_ID = @DDM_Menu_ID
								 AND DDM_Profile_ID = @DDM_Profile_ID
								 AND DDM_Menu_Header_ID = @DDM_Menu_Header_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { SourceColumn = "DDM_Profile_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Menu_Header_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_Header_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Order", SqlDbType.Int).SourceColumn = "DDM_Menu_Header_Order";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Option_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Option_Type";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Dim_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Dim_Type";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Dim_Name", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Dim_Name";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_MFB", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_MFB";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Default", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Default";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Btn";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Btn_Lbl";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Btn_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Btn_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Cbx";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Cbx_Lbl";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Cbx_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Cbx_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt", SqlDbType.Bit).SourceColumn = "DDM_Menu_Header_Filter_Txt";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Txt_Lbl";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Filter_Txt_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Filter_Txt_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Option_Btn_Type", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Option_Btn_Type";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_Lbl", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_Lbl";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_ToolTip", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_Image_URL", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_Image_URL";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_ServerTask", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_ServerTask";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_DBOpen", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_DBOpen";
                sqa.UpdateCommand.Parameters.Add("@DDM_Menu_Header_Btn_DBRefresh", SqlDbType.NVarChar).SourceColumn = "DDM_Menu_Header_Btn_DBRefresh";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
					             DELETE FROM DDM_Config_Menu_Header
					             WHERE DDM_Profile_ID = @DDM_Profile_ID 
								 AND DDM_Menu_ID = @DDM_Menu_ID
								 AND DDM_Menu_Header_ID = @DDM_Menu_Header_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { SourceColumn = "DDM_Profile_ID", SourceVersion = DataRowVersion.Original });
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_ID", SourceVersion = DataRowVersion.Original });
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Menu_Header_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_Header_ID", SourceVersion = DataRowVersion.Original });

                try
                {
                    sqa.Update(dataTable);
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