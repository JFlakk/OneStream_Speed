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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Load_DB
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
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_DDM_Admin_Dashboard"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.LoadDashboardTaskInfo.Reason + "-" + args.LoadDashboardTaskInfo.Action);
                            // Implement Load Dashboard logic here.
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
                            {
                                var load_Dashboard_Task_Result = Load_Admin_Dashboard(si, globals, api, args, "Initial");
                                return load_Dashboard_Task_Result;
                            }
                            else if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeSubsequentGetParameters)
                            {
                                var load_Dashboard_Task_Result = Load_Admin_Dashboard(si, globals, api, args, "Post-Initial");
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
        private XFLoadDashboardTaskResult Load_Admin_Dashboard(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args, string RunType)
        {
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
            if (RunType.XFEqualsIgnoreCase("Initial"))
            {
                BRApi.ErrorLog.LogMessage(si, "Jeffy");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Show_Hide_Menu_Btn", "Show");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Display_Show_Menu_Btn", "False");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Display_Hide_Menu_Btn", "True");
                BRApi.ErrorLog.LogMessage(si, "Hit Hide: " + Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFGetValue("IV_DDM_Display_Hide_Menu_Btn"));
                BRApi.ErrorLog.LogMessage(si, "Hit Show: " + Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFGetValue("IV_DDM_Display_Show_Menu_Btn"));
                var setup_Options = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("DL_DDM_Setup_Options", String.Empty);
                if (setup_Options == String.Empty)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hit Setup Options DDM: " + setup_Options);
                    setup_Options = "0_DDM_Profile_Config";
                }

                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("DL_DDM_Setup_Options", setup_Options);
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_Menu_Width", "Auto");

                var default_root_Profile = get_Default_Root_Profile(si, globals, api, db_args); //Get Default root profile
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("BL_DDM_Root_WF_Profiles", default_root_Profile);
                var prior_root_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("BL_DDM_Root_WF_Profiles", "NA");
                var new_root_Profile = true;
                if (default_root_Profile == prior_root_Profile)
                {
                    new_root_Profile = false;
                }
                var default_WF_Profile = get_Default_WF_Profile(si, globals, api, db_args, new_root_Profile, default_root_Profile);
                BRApi.ErrorLog.LogMessage(si, "Hit: " + default_WF_Profile);
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_trv_Root_WF_Profile_Detail", default_WF_Profile);
                var prior_WF_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_trv_Root_WF_Profile_Detail", "NA");
                var new_WF_Profile = true;
                if ((default_WF_Profile == prior_WF_Profile) && (default_WF_Profile != "NA"))
                {
                    new_WF_Profile = false;
                }
                //				//Get Default Scenario Type
                //				//Get Default Ent Mbr Filter
                //				//Get Default Agg vs Consol

            }
            else if (RunType.XFEqualsIgnoreCase("Post-Initial"))
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Post Init");
                //Checks current value of Show Menu param against prior run
                var show_hide_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_Show_Hide_Menu_Btn", "NA");
                var new_display_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_Show_Hide_Menu_Btn", "NA");
                var prior_run_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_Display_Show_Menu_Btn", "NA");

                if (!string.IsNullOrEmpty(show_hide_val))
                {
                    if (show_hide_val == "Hide")
                    {
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Display_Show_Menu_Btn", "True");
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Display_Hide_Menu_Btn", "False");
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Menu_Width", "0");
                    }
                    else if (show_hide_val == "Show")
                    {
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Display_Show_Menu_Btn", "False");
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Display_Hide_Menu_Btn", "True");
                        Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_DDM_Menu_Width", "Auto");
                    }
                }
                if (db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("DL_DDM_Setup_Options") == "0_DDM_Profile_Cofig")
                {
                    var default_root_Profile = get_Default_Root_Profile(si, globals, api, db_args); //Get Default root profile
                    Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("BL_DDM_Root_WF_Profiles", default_root_Profile);
                    var prior_root_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("BL_DDM_Root_WF_Profiles", "NA");
                    var new_root_Profile = true;
                    if (default_root_Profile == prior_root_Profile)
                    {
                        new_root_Profile = false;
                    }
                    var default_WF_Profile = get_Default_WF_Profile(si, globals, api, db_args, new_root_Profile, default_root_Profile);
                    BRApi.ErrorLog.LogMessage(si, "Hit: " + default_WF_Profile);
                    Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_DDM_trv_Root_WF_Profile_Detail", default_WF_Profile);
                    var prior_WF_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("IV_DDM_trv_Root_WF_Profile_Detail", "NA");
                    var new_WF_Profile = true;
                    if ((default_WF_Profile == prior_WF_Profile) && (default_WF_Profile != "NA"))
                    {
                        new_WF_Profile = false;
                    }
                }
                //				else if ((db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.GetValueOrEmpty("DL_MCM_Setup_Options") == "0_MCM_Build_Model" ||
                //					     db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.GetValueOrEmpty("DL_MCM_Setup_Options") == "0_MCM_Build_Model_Group") &&
                //					     db_args.PrimaryDashboard.Name == "MCM_App_Admin (OnePlace)")
                //				{
                //					BRApi.ErrorLog.LogMessage(si,"hit Initial 3: " + db_args.PrimaryDashboard.Name);
                //					//Get Default Cube/Scenario Type ID for first time user logs in
                //					//string default_Cube_ID = Get_Default_Cube()
                //					string Cube_ID = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_MCM_Cube_Settings","1");
                //					//Get Default Activity ID if user hasn't selected one for the Cube ID
                //					//string default_Activity_ID = Get_Default_Cube()
                //					string Activity_ID = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_MCM_Cube_Model_Activity_List","1");
                //					if (Activity_ID.Length == 0)
                //					{
                //						Activity_ID = "1";
                //						Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("BL_MCM_Cube_Model_Activity_List",Activity_ID);
                //					}

                //					//Get Default Activity ID if user hasn't selected one for the Cube ID
                //					//string default_Activity_ID = Get_Default_Cube()
                //					string Activity_Model = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_MCM_Models","1");
                //					BRApi.ErrorLog.LogMessage(si,"hit Activity: " + Activity_ID + "-" + Cube_ID);
                //					//Get Calc Type from Cube/Activity ID
                //					Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_MCM_OS_Cube_ID",Cube_ID);
                //					globals.SetStringValue("IV_MCM_OS_Cube_ID",Cube_ID);
                //					Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_MCM_Cube_Model_Activity_ID",Activity_ID);
                //					globals.SetStringValue("IV_MCM_Cube_Model_Activity_ID",Activity_ID);
                //					Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_MCM_Models",Activity_Model);
                //					Load_Dashboard_Task_Result = Get_Calc_Type(si,globals,api,db_args,Load_Dashboard_Task_Result);
                //					Load_Dashboard_Task_Result = Load_Cube_Settings(si,globals,api,db_args,Load_Dashboard_Task_Result);
                //					if (db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.GetValueOrEmpty("DL_MCM_Setup_Options") == "0_MCM_Build_Model_Group")
                //					{
                //						string Model_Group = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("BL_MCM_Model_Groups","0");
                //						globals.SetStringValue("IV_MCM_Model_Group",Model_Group);
                //						Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_MCM_Model_Group",Model_Group);
                //					}
                //				}
            }

            return Load_Dashboard_Task_Result;

        }
        #endregion

        #region "Setup Helpers"

        private String get_Default_Root_Profile(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args)
        {
            try
            {
                var curr_Root_Profile = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_DDM_Root_WF_Profiles", "NA");

                if (curr_Root_Profile == "NA")
                {
                    var root_Profile_DT = new DataTable("root_profile");

                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        string selectQuery = @"
				       						SELECT ProfileName 
		        							FROM WorkflowProfileHierarchy
		        							WHERE HierarchyLevel = 1
											AND IsTemplate = 0 ";

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        };

                        sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, root_Profile_DT, selectQuery, parameters);
                    }
                    // Check if the DataTable has any rows
                    if (root_Profile_DT.Rows.Count > 0)
                    {
                        // Retrieve the value from the desired column
                        return root_Profile_DT.Rows[0]["ProfileName"].ToString();
                    }
                    else
                    {
                        return "NA";
                    }
                }
                else
                {
                    return curr_Root_Profile;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        private String get_Default_WF_Profile(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs db_args, Boolean new_root_Profile, string root_ProfileName)
        {
            try
            {
                if (new_root_Profile == false)
                {
                    return db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_trv_Root_WF_Profile_Detail", "NA");
                }
                else
                {
                    var wf_Profile_DT = new DataTable("wf_profile");

                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        string selectQuery = @"
										SELECT prof.ProfileKey,prof.ProfileName
										FROM WorkflowProfileHierarchy prof
										WHERE prof.HierarchyLevel = 1
									    AND prof.IsTemplate = 0
										AND prof.ProfileName = @root_ProfileName";

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        new SqlParameter("@root_ProfileName", SqlDbType.NVarChar,100) { Value = root_ProfileName }
                        };

                        sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, wf_Profile_DT, selectQuery, parameters);
                    }
                    // Check if the DataTable has any rows
                    if (wf_Profile_DT.Rows.Count > 0)
                    {
                        // Retrieve the value from the desired column
                        return wf_Profile_DT.Rows[0]["ProfileKey"].ToString();
                    }
                    else
                    {
                        return "NA";
                    }
                }

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }


        }

        #endregion
        #endregion
    }
}