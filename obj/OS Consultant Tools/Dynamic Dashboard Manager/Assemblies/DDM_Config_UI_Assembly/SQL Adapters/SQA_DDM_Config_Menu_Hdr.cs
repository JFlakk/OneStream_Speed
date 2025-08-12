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
    public class SQA_DDM_Config_Menu_Hdr
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu_Hdr(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_DDM_Config_Menu_Hdr_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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

        public void Update_DDM_Config_Menu_Hdr(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and selectqueryparams
                string insertQuery = @"
                    INSERT INTO DDM_Config_Menu_Hdr
                           (DDM_Config_ID
                           ,DDM_Menu_ID
                           ,DDM_Menu_Hdr_ID
                           ,DDM_Type
                           ,Scen_Type
                           ,Profile_Key
                           ,Workspace_ID
                           ,MaintUnit_ID
                           ,Sort_Order
                           ,Option_Type
                           ,Dependency_Tier
                           ,Fltr_Dim_Type
                           ,Fltr_Dim_Name
                           ,Fltr_MFB
                           ,Fltr_Default
                           ,Fltr_Btn
                           ,Fltr_Btn_Lbl
                           ,Fltr_Btn_ToolTip
                           ,Fltr_Cbx
                           ,Fltr_Cbx_Lbl
                           ,Fltr_Cbx_ToolTip
                           ,Fltr_BtnCbx_BoundParam
                           ,Fltr_Txt
                           ,Fltr_Txt_Lbl
                           ,Fltr_Txt_ToolTip
                           ,Fltr_Txt_BoundParam
                           ,Btn_Type
                           ,Btn_Lbl
                           ,Btn_ToolTip
                           ,Btn_Image_URL
                           ,Btn_ServerTask
                           ,Btn_DBOpen
                           ,Btn_DBRefresh
                           ,Create_Date
                           ,Create_User
                           ,Update_Date
                           ,Update_User)
                     VALUES
                           (@DDM_Config_ID
                           ,@DDM_Menu_ID
                           ,@DDM_Menu_Hdr_ID
                           ,@DDM_Type
                           ,@Scen_Type
                           ,@Profile_Key
                           ,@Workspace_ID
                           ,@MaintUnit_ID
                           ,@Sort_Order
                           ,@Option_Type
                           ,@Dependency_Tier
                           ,@Fltr_Dim_Type
                           ,@Fltr_Dim_Name
                           ,@Fltr_MFB
                           ,@Fltr_Default
                           ,@Fltr_Btn
                           ,@Fltr_Btn_Lbl
                           ,@Fltr_Btn_ToolTip
                           ,@Fltr_Cbx
                           ,@Fltr_Cbx_Lbl
                           ,@Fltr_Cbx_ToolTip
                           ,@Fltr_BtnCbx_BoundParam
                           ,@Fltr_Txt
                           ,@Fltr_Txt_Lbl
                           ,@Fltr_Txt_ToolTip
                           ,@Fltr_Txt_BoundParam
                           ,@Btn_Type
                           ,@Btn_Lbl
                           ,@Btn_ToolTip
                           ,@Btn_Image_URL
                           ,@Btn_ServerTask
                           ,@Btn_DBOpen
                           ,@Btn_DBRefresh
                           ,@Create_Date
                           ,@Create_User
                           ,@Update_Date
                           ,@Update_User)";
                
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Menu_Hdr_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_Hdr_ID";
                sqa.InsertCommand.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
                sqa.InsertCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.InsertCommand.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
                sqa.InsertCommand.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
                sqa.InsertCommand.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
                sqa.InsertCommand.Parameters.Add("@Sort_Order", SqlDbType.Int).SourceColumn = "Sort_Order";
                sqa.InsertCommand.Parameters.Add("@Option_Type", SqlDbType.NVarChar, 50).SourceColumn = "Option_Type";
                sqa.InsertCommand.Parameters.Add("@Dependency_Tier", SqlDbType.Int).SourceColumn = "Dependency_Tier";
                sqa.InsertCommand.Parameters.Add("@Fltr_Dim_Type", SqlDbType.NVarChar, 50).SourceColumn = "Fltr_Dim_Type";
                sqa.InsertCommand.Parameters.Add("@Fltr_Dim_Name", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Dim_Name";
                sqa.InsertCommand.Parameters.Add("@Fltr_MFB", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_MFB";
                sqa.InsertCommand.Parameters.Add("@Fltr_Default", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_Default";
                sqa.InsertCommand.Parameters.Add("@Fltr_Btn", SqlDbType.Bit).SourceColumn = "Fltr_Btn";
                sqa.InsertCommand.Parameters.Add("@Fltr_Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Btn_Lbl";
                sqa.InsertCommand.Parameters.Add("@Fltr_Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Btn_ToolTip";
                sqa.InsertCommand.Parameters.Add("@Fltr_Cbx", SqlDbType.Bit).SourceColumn = "Fltr_Cbx";
                sqa.InsertCommand.Parameters.Add("@Fltr_Cbx_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Cbx_Lbl";
                sqa.InsertCommand.Parameters.Add("@Fltr_Cbx_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Cbx_ToolTip";
                sqa.InsertCommand.Parameters.Add("@Fltr_BtnCbx_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_BtnCbx_BoundParam";
                sqa.InsertCommand.Parameters.Add("@Fltr_Txt", SqlDbType.Bit).SourceColumn = "Fltr_Txt";
                sqa.InsertCommand.Parameters.Add("@Fltr_Txt_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_Lbl";
                sqa.InsertCommand.Parameters.Add("@Fltr_Txt_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Txt_ToolTip";
                sqa.InsertCommand.Parameters.Add("@Fltr_Txt_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_BoundParam";
                sqa.InsertCommand.Parameters.Add("@Btn_Type", SqlDbType.NVarChar, 50).SourceColumn = "Btn_Type";
                sqa.InsertCommand.Parameters.Add("@Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Btn_Lbl";
                sqa.InsertCommand.Parameters.Add("@Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ToolTip";
                sqa.InsertCommand.Parameters.Add("@Btn_Image_URL", SqlDbType.NVarChar, 2048).SourceColumn = "Btn_Image_URL";
                sqa.InsertCommand.Parameters.Add("@Btn_ServerTask", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ServerTask";
                sqa.InsertCommand.Parameters.Add("@Btn_DBOpen", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBOpen";
                sqa.InsertCommand.Parameters.Add("@Btn_DBRefresh", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBRefresh";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                string updateQuery = @"
                    UPDATE DDM_Config_Menu_Hdr
                       SET DDM_Config_ID = @DDM_Config_ID
                          ,DDM_Type = @DDM_Type
                          ,Scen_Type = @Scen_Type
                          ,Profile_Key = @Profile_Key
                          ,Workspace_ID = @Workspace_ID
                          ,MaintUnit_ID = @MaintUnit_ID
                          ,Sort_Order = @Sort_Order
                          ,Option_Type = @Option_Type
                          ,Dependency_Tier = @Dependency_Tier
                          ,Fltr_Dim_Type = @Fltr_Dim_Type
                          ,Fltr_Dim_Name = @Fltr_Dim_Name
                          ,Fltr_MFB = @Fltr_MFB
                          ,Fltr_Default = @Fltr_Default
                          ,Fltr_Btn = @Fltr_Btn
                          ,Fltr_Btn_Lbl = @Fltr_Btn_Lbl
                          ,Fltr_Btn_ToolTip = @Fltr_Btn_ToolTip
                          ,Fltr_Cbx = @Fltr_Cbx
                          ,Fltr_Cbx_Lbl = @Fltr_Cbx_Lbl
                          ,Fltr_Cbx_ToolTip = @Fltr_Cbx_ToolTip
                          ,Fltr_BtnCbx_BoundParam = @Fltr_BtnCbx_BoundParam
                          ,Fltr_Txt = @Fltr_Txt
                          ,Fltr_Txt_Lbl = @Fltr_Txt_Lbl
                          ,Fltr_Txt_ToolTip = @Fltr_Txt_ToolTip
                          ,Fltr_Txt_BoundParam = @Fltr_Txt_BoundParam
                          ,Btn_Type = @Btn_Type
                          ,Btn_Lbl = @Btn_Lbl
                          ,Btn_ToolTip = @Btn_ToolTip
                          ,Btn_Image_URL = @Btn_Image_URL
                          ,Btn_ServerTask = @Btn_ServerTask
                          ,Btn_DBOpen = @Btn_DBOpen
                          ,Btn_DBRefresh = @Btn_DBRefresh
                          ,Update_Date = @Update_Date
                          ,Update_User = @Update_User
                     WHERE DDM_Menu_Hdr_ID = @DDM_Menu_Hdr_ID";
                
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@DDM_Menu_Hdr_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_Hdr_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
                sqa.UpdateCommand.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
                sqa.UpdateCommand.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
                sqa.UpdateCommand.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
                sqa.UpdateCommand.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
                sqa.UpdateCommand.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
                sqa.UpdateCommand.Parameters.Add("@Sort_Order", SqlDbType.Int).SourceColumn = "Sort_Order";
                sqa.UpdateCommand.Parameters.Add("@Option_Type", SqlDbType.NVarChar, 50).SourceColumn = "Option_Type";
                sqa.UpdateCommand.Parameters.Add("@Dependency_Tier", SqlDbType.Int).SourceColumn = "Dependency_Tier";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Dim_Type", SqlDbType.NVarChar, 50).SourceColumn = "Fltr_Dim_Type";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Dim_Name", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Dim_Name";
                sqa.UpdateCommand.Parameters.Add("@Fltr_MFB", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_MFB";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Default", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_Default";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Btn", SqlDbType.Bit).SourceColumn = "Fltr_Btn";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Btn_Lbl";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Btn_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Cbx", SqlDbType.Bit).SourceColumn = "Fltr_Cbx";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Cbx_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Cbx_Lbl";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Cbx_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Cbx_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@Fltr_BtnCbx_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_BtnCbx_BoundParam";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Txt", SqlDbType.Bit).SourceColumn = "Fltr_Txt";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Txt_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_Lbl";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Txt_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Txt_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@Fltr_Txt_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_BoundParam";
                sqa.UpdateCommand.Parameters.Add("@Btn_Type", SqlDbType.NVarChar, 50).SourceColumn = "Btn_Type";
                sqa.UpdateCommand.Parameters.Add("@Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Btn_Lbl";
                sqa.UpdateCommand.Parameters.Add("@Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ToolTip";
                sqa.UpdateCommand.Parameters.Add("@Btn_Image_URL", SqlDbType.NVarChar, 2048).SourceColumn = "Btn_Image_URL";
                sqa.UpdateCommand.Parameters.Add("@Btn_ServerTask", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ServerTask";
                sqa.UpdateCommand.Parameters.Add("@Btn_DBOpen", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBOpen";
                sqa.UpdateCommand.Parameters.Add("@Btn_DBRefresh", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBRefresh";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and selectqueryparams
                string deleteQuery = @"
                                 DELETE FROM DDM_Config_Menu_Hdr
                                 WHERE DDM_Menu_Hdr_ID = @DDM_Menu_Hdr_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@DDM_Menu_Hdr_ID", SqlDbType.Int) { SourceColumn = "DDM_Menu_Hdr_ID", SourceVersion = DataRowVersion.Original });

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