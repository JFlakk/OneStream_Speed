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
    public class SQA_DDM_Config_Hdr_Ctrls
    {
        private readonly SQA_GBL_Command_Builder _cmdBuilder;

        public SQA_DDM_Config_Hdr_Ctrls(SessionInfo si, SqlConnection connection)
        {
            _cmdBuilder = new SQA_GBL_Command_Builder(si, connection);
        }

        public void Fill_DDM_Config_Hdr_Ctrls_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
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

        public void Update_DDM_Config_Hdr_Ctrls(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, "DDM_Config_Hdr_Ctrls", dt);
                    builder.SetPrimaryKey("Hdr_Ctrl_ID");
                    builder.ExcludeFromUpdate("Hdr_Ctrl_ID", "DDM_Config_ID", "Create_Date", "Create_User");
                    builder.ConfigureAdapter(sqa, transaction);

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

        private void AddParameters(SqlCommand cmd)
        {
            // IDs
            cmd.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
            cmd.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
            cmd.Parameters.Add("@DDM_Hdr_Ctrl_ID", SqlDbType.Int).SourceColumn = "DDM_Hdr_Ctrl_ID";
            
            // Core Config
            cmd.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
            cmd.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
            cmd.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
            cmd.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
            cmd.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
            cmd.Parameters.Add("@Sort_Order", SqlDbType.Int).SourceColumn = "Sort_Order";
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).SourceColumn = "Name";
            cmd.Parameters.Add("@Option_Type", SqlDbType.Int).SourceColumn = "Option_Type";
            
            // Filters
            cmd.Parameters.Add("@Fltr_Dependency_Tier", SqlDbType.Int).SourceColumn = "Fltr_Dependency_Tier";
            cmd.Parameters.Add("@Fltr_Dim_Type", SqlDbType.NVarChar, 50).SourceColumn = "Fltr_Dim_Type";
            cmd.Parameters.Add("@Fltr_Dim_Name", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Dim_Name";
            cmd.Parameters.Add("@Fltr_MFB", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_MFB";
            cmd.Parameters.Add("@Fltr_Default", SqlDbType.NVarChar, -1).SourceColumn = "Fltr_Default";
            
            // Filter Controls (Bits)
            cmd.Parameters.Add("@Fltr_Btn", SqlDbType.Bit).SourceColumn = "Fltr_Btn";
            cmd.Parameters.Add("@Fltr_Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Btn_Lbl";
            cmd.Parameters.Add("@Fltr_Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Btn_ToolTip";
            cmd.Parameters.Add("@Fltr_Cbx", SqlDbType.Bit).SourceColumn = "Fltr_Cbx";
            cmd.Parameters.Add("@Fltr_Cbx_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Cbx_Lbl";
            cmd.Parameters.Add("@Fltr_Cbx_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Cbx_ToolTip";
            cmd.Parameters.Add("@Fltr_BtnCbx_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_BtnCbx_BoundParam";
            cmd.Parameters.Add("@Fltr_Txt", SqlDbType.Bit).SourceColumn = "Fltr_Txt";
            cmd.Parameters.Add("@Fltr_Txt_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_Lbl";
            cmd.Parameters.Add("@Fltr_Txt_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Fltr_Txt_ToolTip";
            cmd.Parameters.Add("@Fltr_Txt_BoundParam", SqlDbType.NVarChar, 100).SourceColumn = "Fltr_Txt_BoundParam";
            
            // Buttons
            cmd.Parameters.Add("@Btn_Type", SqlDbType.NVarChar, 50).SourceColumn = "Btn_Type";
            cmd.Parameters.Add("@Btn_Lbl", SqlDbType.NVarChar, 100).SourceColumn = "Btn_Lbl";
            cmd.Parameters.Add("@Btn_ToolTip", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ToolTip";
            cmd.Parameters.Add("@Btn_Image_URL", SqlDbType.NVarChar, 2048).SourceColumn = "Btn_Image_URL";
            cmd.Parameters.Add("@Btn_ServerTask", SqlDbType.NVarChar, 255).SourceColumn = "Btn_ServerTask";
            cmd.Parameters.Add("@Btn_DBOpen", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBOpen";
            cmd.Parameters.Add("@Btn_DBRefresh", SqlDbType.NVarChar, 255).SourceColumn = "Btn_DBRefresh";
            
            // Audit
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
            cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
            cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
        }
    }
}