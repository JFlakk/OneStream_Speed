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
using OneStreamWorkspacesApi.V800;


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Load_DB
{
    public class MainClass
    {

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_DDM_Dashboard"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit > 1 Row");
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeGetDashboardDisplayInfo)
                            {
                           }
                            // Implement Load Dashboard logic here.
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
                            {
                                var load_Dashboard_Task_Result = Load_Dashboard(si, globals, api, args, "Initial");
                                return load_Dashboard_Task_Result;
                            }
                            else if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeSubsequentGetParameters)
                            {
                                var load_Dashboard_Task_Result = Load_Dashboard(si, globals, api, args, "Post-Initial");
                                return load_Dashboard_Task_Result;
                            }
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        #region "Class Helper Functions"	
        #region "Load Dashboard"
        private XFLoadDashboardTaskResult Load_Dashboard(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args, string RunType)
        {
            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            if (RunType.XFEqualsIgnoreCase("Initial"))
            {
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
                //				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Show_Hide_Menu_Btn","Show");
                //				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Display_Show_Menu_Btn","False");
                //				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Display_Hide_Menu_Btn","True");
                //				Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Menu_Width","Auto");
                Load_Dashboard_Task_Result = Get_Default_Menu_Options(si, globals, api, db_args, Load_Dashboard_Task_Result, wfUnitPk.ProfileKey, "Initial");
                //Load_Dashboard_Task_Result = Get_Default_Menu_Option(si,globals,api,db_args,Load_Dashboard_Task_Result);
                //Get Default Cube Settings
                //Get Default Cube
                //Get Default Scenario Type
                //Get Default Ent Mbr Filter
                //Get Default Agg vs Consol

            }
            else if (RunType.XFEqualsIgnoreCase("Post-Initial"))
            {
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
                //				//Checks current value of Show Menu param against prior run
                //				var show_hide_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_Show_Hide_Menu_Btn");
                //				var new_display_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_Show_Hide_Menu_Btn");
                //				var prior_run_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_Display_Show_Menu_Btn");

                //				if (!string.IsNullOrEmpty(show_hide_val))
                //				{
                //				    if (show_hide_val == "Hide")
                //				    {
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Display_Show_Menu_Btn", "True");
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Display_Hide_Menu_Btn", "False");
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Menu_Width", "0");
                //				    }
                //				    else if (show_hide_val == "Show")
                //				    {
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Display_Show_Menu_Btn", "False");
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Display_Hide_Menu_Btn", "True");
                //				        UpdateCustomSubstVar(Load_Dashboard_Task_Result, "IV_DDM_Menu_Width", "Auto");
                //				    }
                //				}
                Load_Dashboard_Task_Result = Get_Default_Menu_Options(si, globals, api, db_args, Load_Dashboard_Task_Result, wfUnitPk.ProfileKey, "Post-Initial");
            }

            return Load_Dashboard_Task_Result;

        }

        #region "Setup Helpers"
        private XFLoadDashboardTaskResult Get_Default_Menu_Options(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args, XFLoadDashboardTaskResult Load_Dashboard_Task_Result, Guid ProfileKey, String RunType)
        {
            var XF_Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            XF_Load_Dashboard_Task_Result = Load_Dashboard_Task_Result;

            var wf_Profile_Config_DT = new DataTable("wf_Profile_Config");

            var menu_option = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("BL_DDM_App_Menu", String.Empty);

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                // Create a new DataTable

                var sqa = new SqlDataAdapter();
                // Define the select query and parameters
                var sql = @"
	       					Select Menu.DDM_Profile_ID,Menu.DDM_Menu_ID,Menu.Name,
							Menu.DDM_Option_Type, Menu.DDM_Custom_DB_Header,
							Menu.DDM_Custom_DB_Content,
							Menu.DDM_DB_Name,Menu.DDM_CVName
							FROM DDM_Config Cnfg
							JOIN DDM_Config_Menu Menu
							ON Cnfg.DDM_Profile_ID = Menu.DDM_Profile_ID
							WHERE Cnfg.DDM_Profile_Name = @OS_WFProfileKey";
                if (menu_option != String.Empty)
                {
                    sql += @"
									AND Menu.DDM_Menu_ID = @DDM_Menu_ID";
                }

                sql += @"
									ORDER BY DDM_Menu_Order";

                // Initialize the list of parameters
                var sqlparams = new List<SqlParameter>
                {
                    new SqlParameter("@OS_WFProfileKey", SqlDbType.UniqueIdentifier) { Value = ProfileKey }
                };

                // Add the second parameter conditionally
                if (menu_option != String.Empty)
                {
                    sqlparams.Add(new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = Convert.ToInt32(menu_option) });
                }


                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wf_Profile_Config_DT, sql, sqlparams.ToArray());
            }
            if (wf_Profile_Config_DT.Rows.Count > 0)
            {
                DataRow row = wf_Profile_Config_DT.Rows[0];

                UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, "BL_DDM_App_Menu", row["DDM_Menu_ID"].ToString());

//                SetDynamicDBParameter(XF_Load_Dashboard_Task_Result, row);
//                SetDynamicHeaderDBParams(si, globals, api, args, XF_Load_Dashboard_Task_Result, row);


//                var menu_Option_Type = row["DDM_Option_Type"].ToString();
//                var customDashboardHeader = row["DDM_Menu_Option_Custom_Dashboard_Header"].ToString();
//                var customDashboardContent = row["DDM_Menu_Option_Custom_Dashboard_Content"].ToString();
//                var dashboardName = row["DDM_Menu_Option_Dashboard_Name"].ToString();
//                var cubeViewName = row["DDM_Menu_Option_CubeView_Name"].ToString();
            }

            return XF_Load_Dashboard_Task_Result;
        }

        private void SetDynamicDBParameter(XFLoadDashboardTaskResult result, DataRow row)
        {
            if (row["DDM_Option_Type"].ToString().XFEqualsIgnoreCase("Custom Dashboard"))
            {
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_DB", "0_DDM_App_Dyn_Custom_DB");
            }
            else if (row["DDM_Option_Type"].ToString().XFEqualsIgnoreCase("Cube View"))
            {
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_DB", "0_DDM_App_Dyn_DB");
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_App_Content_DB", "0b2_DDM_App_Dyn_CV_Content_Col2");
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_Cube_View", row["DDM_CVName"].ToString());
            }
            else if (row["DDM_Option_Type"].ToString().XFEqualsIgnoreCase("Dashboard"))
            {
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_DB", "0_DDM_App_Dyn_DB");
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_App_Content_DB", "0b2_DDM_App_Dyn_CV_Content_Col2");
                UpdateCustomSubstVar(result, "IV_DDM_App_Dynamic_Dashboard", row["DDM_DB_Name"].ToString());
            }
        }


        private void UpdateCustomSubstVar(XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
            }
        }

//        private XFLoadDashboardTaskResult SetDynamicHeaderDBParams(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args, XFLoadDashboardTaskResult Load_Dashboard_Task_Result, DataRow menu_row)
//        {
//            var XF_Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
//            XF_Load_Dashboard_Task_Result = Load_Dashboard_Task_Result;

//            var Menu_Hdr_Options_DT = new DataTable("Menu_Hdr_Options");

//            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
//            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
//            {
//                var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
//                // Create a new DataTable

//                var sqlDataAdapter = new SqlDataAdapter();
//                // Define the select query and parameters
//                string selectQuery = @"
//							        	Select *
//										FROM DDM_Profile_Config_Menu_Hdr_Options
//										WHERE DDM_Profile_ID = @DDM_Profile_ID
//										AND DDM_Profile_Menu_Option_ID = @DDM_Profile_Menu_Option_ID
//										ORDER BY DDM_Menu_Option_Header_Sort_Order";

//                // Create an array of SqlParameter objects
//                var parameters = new SqlParameter[]
//                {
//                    new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = menu_row["DDM_Profile_ID"]},
//                    new SqlParameter("@DDM_Profile_Menu_Option_ID", SqlDbType.Int) { Value = menu_row["DDM_Profile_Menu_Option_ID"]}
//                };

//                sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, Menu_Hdr_Options_DT, selectQuery, parameters);
//            }
//            if (Menu_Hdr_Options_DT.Rows.Count > 0)
//            {
//                BRApi.ErrorLog.LogMessage(si, "Hit > 1 Row");
//                //XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("DL_MCM_Calc_Type",calc_Type_DT.Rows[0]["Calc_Type"].ToString());
//            }
//            else
//            {
//                //XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("DL_MCM_Calc_Type","Table");
//            }


//            int header_option_cnt = 0;
//            // Loop through each row in the DataTable
//            foreach (DataRow row in Menu_Hdr_Options_DT.Rows)
//            {
//                header_option_cnt += 1;
//                // Check specific conditions and set substitution variables accordingly
//                if (row["Option_Type"].ToString() == "Filter")
//                {
//                    UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_" + row["Fltr_Dim_Type"].ToString());
//                    UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Dim_Name", row["Fltr_Dim_Name"].ToString());
//                    UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_MFB", row["Fltr_MFB"].ToString());
//                    //UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result,$"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Default",row["Fltr_Default"].ToString());
//                    if (row["Fltr_Btn"].ToString() == "True")
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_Visible", "True");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_Label", row["Fltr_Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_ToolTip", row["Fltr_Btn_ToolTip"].ToString());
//                    }
//                    else
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_Visible", "False");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_Label", String.Empty);
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Btn_ToolTip", String.Empty);
//                    }

//                    if (row["Fltr_Cbx"].ToString() == "True")
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_Visible", "True");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_Label", row["Fltr_Cbx_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_ToolTip", row["Fltr_Cbx_ToolTip"].ToString());
//                    }
//                    else
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_Visible", "False");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_Label", String.Empty);
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Cbx_ToolTip", String.Empty);
//                    }
//                    if (row["Fltr_Txt"].ToString() == "True")
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_Visible", "True");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_Label", row["Fltr_Txt_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_ToolTip", row["Fltr_Txt_ToolTip"].ToString());
//                    }
//                    else
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_Visible", "False");
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_Label", String.Empty);
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["Fltr_Dim_Type"].ToString() + "_Txt_ToolTip", String.Empty);
//                    }
//                }
//                else if (row["Option_Type"].ToString() == "Button")
//                {
//                    bool Generic_DBExt = row["DDM_Menu_Hdr_Option_Btn_Type"].ToString().IndexOf("Generic_DBExt", StringComparison.OrdinalIgnoreCase) >= 0;

//                    if (Generic_DBExt)
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_Btn_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Text", row["Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ToolTip", row["Btn_ToolTip"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Image_URL", row["Btn_Image_URL"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ServerTask", row["Btn_ServerTask"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Open", row["Btn_DBOpen"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Refresh", row["Btn_DBRefresh"].ToString());
//                    }

//                    bool Open_Dialog = row["DDM_Menu_Hdr_Option_Btn_Type"].ToString().IndexOf("Open_Dialog", StringComparison.OrdinalIgnoreCase) >= 0;

//                    if (Open_Dialog)
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_Btn_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Text", row["Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ToolTip", row["Btn_ToolTip"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Image_URL", row["Btn_Image_URL"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ServerTask", row["Btn_ServerTask"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Open", row["Btn_DBOpen"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Refresh", row["Btn_DBRefresh"].ToString());
//                    }

//                    bool Save_DM_Seq = row["DDM_Menu_Hdr_Option_Btn_Type"].ToString().IndexOf("Save_DM_Seq", StringComparison.OrdinalIgnoreCase) >= 0;

//                    if (Save_DM_Seq)
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_Btn_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Text", row["Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ToolTip", row["Btn_ToolTip"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Image_URL", row["Btn_Image_URL"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ServerTask", row["Btn_ServerTask"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Open", row["Btn_DBOpen"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Refresh", row["Btn_DBRefresh"].ToString());
//                    }

//                    bool Complete_WF = row["DDM_Menu_Hdr_Option_Btn_Type"].ToString().IndexOf("Complete_WF", StringComparison.OrdinalIgnoreCase) >= 0;

//                    if (Complete_WF)
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_Btn_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Text", row["Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ToolTip", row["Btn_ToolTip"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Image_URL", row["Btn_Image_URL"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ServerTask", row["Btn_ServerTask"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Open", row["Btn_DBOpen"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Refresh", row["Btn_DBRefresh"].ToString());
//                    }

//                    bool Revert_WF = row["DDM_Menu_Hdr_Option_Btn_Type"].ToString().IndexOf("Revert_WF", StringComparison.OrdinalIgnoreCase) >= 0;

//                    if (Revert_WF)
//                    {
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + header_option_cnt.ToString(), $"0ax_DDM_App_Dyn_DB_Header_Btn_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Text", row["Btn_Lbl"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ToolTip", row["Btn_ToolTip"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_Image_URL", row["Btn_Image_URL"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_ServerTask", row["Btn_ServerTask"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Open", row["Btn_DBOpen"].ToString());
//                        UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_" + row["DDM_Menu_Hdr_Option_Btn_Type"].ToString() + "_DB_To_Refresh", row["Btn_DBRefresh"].ToString());
//                    }

//                }
//            }
//            for (int x = header_option_cnt + 1; x <= 23; x++)
//            {
//                UpdateCustomSubstVar(XF_Load_Dashboard_Task_Result, $"IV_DDM_App_Header_Col" + x.ToString(), String.Empty);
//            }

//            return XF_Load_Dashboard_Task_Result;
//        }

        #endregion

        #endregion


//        private XFSelectionChangedTaskResult updateDashboardOptions(SessionInfo si, DashboardExtenderArgs args)
//        {

//            var taskResult = new XFSelectionChangedTaskResult();

//            BRApi.ErrorLog.LogMessage(si, "inside updateDashboardOptions");


//            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
//            var profileKey = wfUnitPk.ProfileKey;

//            string menu_selection = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_DDM_App_Menu", String.Empty);
//            int profileID = -1;

//            var configProfileDT = new DataTable("configProfileDT");
//            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
//            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
//            {
//                var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);

//                var sqlDataAdapter = new SqlDataAdapter();

//                string selectQuery = @"
//										Select DDM_Profile_ID
//										From DDM_Profile_Config
//										Where DDM_Profile_Name = @OS_ProfileKey";

//                var parameters = new SqlParameter[] {
//                    new SqlParameter("@OS_ProfileKey", SqlDbType.UniqueIdentifier) { Value = profileKey }
//                };

//                if (profileKey != null)
//                {
//                    sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, configProfileDT, selectQuery, parameters);
//                }
//            }

//            if (configProfileDT.Rows.Count > 0)
//            {
//                profileID = Convert.ToInt32(configProfileDT.Rows[0]["DDM_Profile_ID"]);
//            }

//            var configMenuOptionsDT = new DataTable("configMenuOptionsDT");
//            if (profileID != -1 && !String.IsNullOrEmpty(menu_selection))
//            {
//                var dbConnApp1 = BRApi.Database.CreateApplicationDbConnInfo(si);
//                using (var connection = new SqlConnection(dbConnApp1.ConnectionString))
//                {
//                    var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);

//                    var sqlDataAdapter = new SqlDataAdapter();

//                    string selectQuery = @"
//											Select *
//											From DDM_Profile_Config_Menu_Options
//											Where DDM_Profile_Menu_Option_ID = @Menu_Option
//											And DDM_Profile_ID = @ProfileID";

//                    var parameters = new SqlParameter[] {
//                        new SqlParameter("@Menu_Option", SqlDbType.Int) { Value = Convert.ToInt32(menu_selection) },
//                        new SqlParameter("@ProfileID", SqlDbType.Int) { Value = profileID }
//                    };

//                    sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, configMenuOptionsDT, selectQuery, parameters);
//                }
//            }

//            if (configMenuOptionsDT.Rows.Count > 0)
//            {
//                taskResult.ModifiedCustomSubstVars.Add("IV_DDM_App_Dynamic_DB_Name", configMenuOptionsDT.Rows[0]["DDM_Menu_Option_Dashboard_Name"].ToString());
//                //args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFSetValue<string, string>("IV_DDM_App_Dynamic_DB_Name", configMenuOptionsDT.Rows[0]["DDM_Menu_Option_Dashboard_Name"].ToString());
//            }

//            return taskResult;
//        }

        public XFSelectionChangedTaskResult printHello(SessionInfo si, DashboardExtenderArgs args)
        {
            XFSelectionChangedTaskResult temp = new XFSelectionChangedTaskResult();
            temp.ChangeCustomSubstVarsInDashboard = true;
            temp.IsOK = true;
            temp.Message = "Hello";
            BRApi.ErrorLog.LogMessage(si, "Here in hello");

            temp.ShowMessageBox = true;
            return temp;
        }

        #endregion
    }
}