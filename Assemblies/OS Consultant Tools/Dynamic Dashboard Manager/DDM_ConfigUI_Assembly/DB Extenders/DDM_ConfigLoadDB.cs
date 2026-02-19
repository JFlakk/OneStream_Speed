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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_ConfigLoadDB
{
    public class MainClass
    {
        #region "Global Params"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        #endregion

        #region "Dictionary Setup"
        private string MainMenuParam = "DL_FMM_Setup_Options";
        private string menuLayoutParam = "DL_FMM_Cube_Config_Options";
        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            {"BL_FMM_Setup_Cube_ID", "IV_FMM_Cube_ID"},
            {"BL_FMM_Cube_ID", "IV_FMM_Cube_ID"},
            {"BL_FMM_Table_Cube_ID", "IV_FMM_Cube_ID"},
            {"BL_FMM_Act_ID", "IV_FMM_Act_ID"},
            {"BL_FMM_Table_Act_ID", "IV_FMM_Act_ID"},
            {"BL_FMM_Model_ID", "IV_FMM_Model_ID"},
            {"BL_FMM_Model_Grp_Seq_ID", "IV_FMM_Model_Grp_Seq_ID"},
            {"BL_FMM_Model_Grp_ID","IV_FMM_Model_Grp_ID"}
        };
        // key string is dialog name, string array is list of IVs associated to textboxes that should be set to empty strings
        private Dictionary<string, string[]> clearTextBoxDict = new Dictionary<string, string[]>() {
            {"0b1b2b2_DDM_Config_Content_New", new string[] {"IV_FMM_Model_Name"}},
            {"1_FMM_Cube_Config_Dialog_Add", new string[] {"IV_FMM_Cube_Descr"}},
            {"1_FMM_Model_Grp_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Name"}},
            {"1_FMM_Model_Grp_Seq_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Seq_Name"}},

        };
        private Dictionary<string, Dictionary<int, string[]>> HierarchyDict = new Dictionary<string, Dictionary<int, string[]>>();

        private Dictionary<int, string[]> AddWFProfileDB = new Dictionary<int, string[]>()
        {
            {0, new string[] {"DL_DDM_Type"}},
            {1, new string[] {"BL_DDM_Root_WFProfiles"}},
            {2, new string[] {"BL_DDM_Scen_Type"}},
            {3, new string[] {"IV_DDM_trv_WF_Profile"}},
            {4, new string[] {"IV_DDM_Config_ID"}},
            {5, new string[] {"IV_DDM_Config_ID"}},
            {6, new string[] {"IV_DDM_Config_ID"}}
        };
        private Dictionary<int, string[]> AddStandAloneDB = new Dictionary<int, string[]>()
        {
            {0, new string[] {"DL_DDM_Type"}},
            {1, new string[] {"BL_DDM_Root_WFProfiles"}},
            {2, new string[] {"BL_DDM_Scen_Type"}},
            {3, new string[] {"IV_DDM_trv_WF_Profile"}},
            {4, new string[] {"IV_DDM_Config_ID"}},
            {5, new string[] {"IV_DDM_Config_ID"}},
            {6, new string[] {"IV_DDM_Config_ID"}}
        };
        #endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_DDM_Admin_Dashboard"))
                        {
                            // Implement Load Dashboard logic here.
                            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
                            {
                                var load_Dashboard_Task_Result = Load_Admin_Dashboard("Initial", ref args);
                                return load_Dashboard_Task_Result;
                            }
                            else if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.ComponentSelectionChanged && (args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters || args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeSubsequentGetParameters))
                            {
                                var load_Dashboard_Task_Result = Load_Admin_Dashboard("Post-Initial", ref args);
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
        private XFLoadDashboardTaskResult Load_Admin_Dashboard(string RunType, ref DashboardExtenderArgs args)
        {
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
            clearParams(ref args, ref Load_Dashboard_Task_Result);
            setParams(ref args, ref Load_Dashboard_Task_Result);

            updateShowHide(ref args, ref Load_Dashboard_Task_Result);

            return Load_Dashboard_Task_Result;

        }
        #endregion

        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string showHideIVName = "IV_DDM_Show_Hide_Menu_Btn";
            string showBtnVisibleName = "IV_DDM_Display_Show_Menu_Btn";
            string hideBtnVisibleName = "IV_DDM_Display_Hide_Menu_Btn";
            string menuWidthIV = "IV_DDM_Menu_Width";


            var ARCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;

            string showHideIVVal = ARCustomSubst.XFGetValue(showHideIVName, "");

            if (showHideIVVal == "Hide")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "0");
            }
            else if (showHideIVVal == "Show")
            {
                UpdateCustomSubstVar(ref taskResult, showBtnVisibleName, "False");
                UpdateCustomSubstVar(ref taskResult, hideBtnVisibleName, "True");
                UpdateCustomSubstVar(ref taskResult, menuWidthIV, "Auto");
            }
        }

        private void clearParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {

            // get dialog name
            string DialogSelection = args.PrimaryDashboard.Name;

            // if dialog dashboard name is contained within the clearTextBoxDict, set all underlying textbox params to empty strings
            if (clearTextBoxDict.ContainsKey(DialogSelection))
            {
                foreach (string param in clearTextBoxDict[DialogSelection])
                {
                    UpdateCustomSubstVar(ref taskResult, param, "");
                }
            }
        }

        private void setParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {

            //TODO: check selectedDashboard based on different higher level menu other than DL_FMM_Cube_Config_Options
            string DialogSelection = args.PrimaryDashboard.Name;
            BRApi.ErrorLog.LogMessage(si, DialogSelection);

            string MainMenuSelection = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam) != string.Empty ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam) : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(MainMenuParam);

            string CubeSubMenuSelection = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(menuLayoutParam) != string.Empty ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(menuLayoutParam) : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(menuLayoutParam);

            // check if cube menu option (secondary layer) has a real selection. If so, use it.
            var selectedDashboard = MainMenuSelection; // == CubeConfigOpt ? CubeSubMenuSelection : MainMenuSelection;

            //check if there's a dialog that exists as part of the hierarchy dict, if so use that instead.
            selectedDashboard = HierarchyDict.ContainsKey(DialogSelection) ? DialogSelection : selectedDashboard;


            var ARCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            var PRCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun;

            //			BRApi.ErrorLog.LogMessage(si, "Key count: " + taskResult.ModifiedCustomSubstVars.Keys.Count);
            foreach (string param in taskResult.ModifiedCustomSubstVars.Keys)
            {
                BRApi.ErrorLog.LogMessage(si, "param: " + param + " val: " + taskResult.ModifiedCustomSubstVars[param]);
            }


            if (HierarchyDict.ContainsKey(selectedDashboard))
            {
                Dictionary<int, string[]> tempDependencyDict = HierarchyDict[selectedDashboard];
                bool priorDependencyChanged = false;
                foreach (int dependencyDepth in tempDependencyDict.Keys)
                {
                    foreach (string param in tempDependencyDict[dependencyDepth])
                    {
                        BRApi.ErrorLog.LogMessage(si, "searching for: " + param + selectedDashboard);
                        BRApi.ErrorLog.LogMessage(si, "AR: " + ARCustomSubst.XFGetValue(param) + " " + (ARCustomSubst.XFGetValue(param) == string.Empty).ToString());
                        BRApi.ErrorLog.LogMessage(si, "PR: " + PRCustomSubst.XFGetValue(param) + " " + (PRCustomSubst.XFGetValue(param) == string.Empty).ToString());

                        bool ARContainsKey = ARCustomSubst.ContainsKey(param);
                        bool PRContainsKey = PRCustomSubst.ContainsKey(param);
                        string ARVal = ARCustomSubst.XFGetValue(param);
                        string PRVal = PRCustomSubst.XFGetValue(param);

                        //						bool ARContainsBRReplaceVal = false;
                        //						string ARBLReplaceVal = "";

                        //						bool PRContainsBRReplaceVal = false;
                        //						string PRBLReplaceVal = "";

                        string mappedParam = paramMap.ContainsKey(param) ? paramMap[param] : "";



                        if (!priorDependencyChanged)
                        {
                            if (mappedParam != "")
                            {
                                //								BRApi.ErrorLog.LogMessage(si, "mapped Param exists for: " + param + " mapped: " + mappedParam + " for dashboard: " + selectedDashboard);
                                string ARMappedVal = ARCustomSubst.XFGetValue(mappedParam, "");
                                string PRMappedVal = PRCustomSubst.XFGetValue(mappedParam, "");

                                //								BRApi.ErrorLog.LogMessage(si, "ARval: " + ARVal + " PRval: " + PRVal + " MappedAR: " + ARMappedVal + " MappedPR: " + PRMappedVal);

                                //check AR and PR for values
                                if (PRContainsKey && isValidParamValue(PRVal) && isValidParamValue(PRMappedVal))
                                {
                                    if (PRVal != PRMappedVal)
                                    {
                                        //										BRApi.ErrorLog.LogMessage(si, "Prior dependency changed for: " + param);
                                        priorDependencyChanged = true;
                                    }
                                    //									BRApi.ErrorLog.LogMessage(si, "setting PRVal for: " + param + " and " + mappedParam);
                                    UpdateCustomSubstVar(ref taskResult, param, PRVal);
                                    UpdateCustomSubstVar(ref taskResult, mappedParam, PRVal);

                                }
                                else if (ARContainsKey && isValidParamValue(ARVal) && isValidParamValue(ARMappedVal))
                                {
                                    if (ARVal != ARMappedVal)
                                    {
                                        //										BRApi.ErrorLog.LogMessage(si, "Prior dependency changed for: " + param);
                                        priorDependencyChanged = true;
                                    }

                                    //									BRApi.ErrorLog.LogMessage(si, "setting ARVal for: " + param + " and " + mappedParam);
                                    UpdateCustomSubstVar(ref taskResult, param, ARVal);
                                    UpdateCustomSubstVar(ref taskResult, mappedParam, ARVal);


                                }
                                else if (ARContainsKey && isValidParamValue(ARVal))
                                {
                                    //									BRApi.ErrorLog.LogMessage(si, "Hit 2");
                                    UpdateCustomSubstVar(ref taskResult, param, ARVal);
                                    UpdateCustomSubstVar(ref taskResult, mappedParam, ARVal);
                                }
                                else if (PRContainsKey && isValidParamValue(PRVal))
                                {
                                    //									BRApi.ErrorLog.LogMessage(si, "Hit 3");
                                    UpdateCustomSubstVar(ref taskResult, param, PRVal);
                                    UpdateCustomSubstVar(ref taskResult, mappedParam, PRVal);
                                }
                                else
                                {
                                    //get default param value
                                    string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                                    //									BRApi.ErrorLog.LogMessage(si, "Hit 4");
                                    UpdateCustomSubstVar(ref taskResult, param, paramDefault);
                                    UpdateCustomSubstVar(ref taskResult, mappedParam, paramDefault);

                                    //set defaults
                                    if (mappedParam == "IV_FMM_Cube_ID" && selectedDashboard == "0_FMM_Cube_Config")
                                    {
                                        // Load_Cube_Settings(taskResult);
                                    }
                                    if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "0_FMM_Model")
                                    {
                                        //Get_Calc_Type(taskResult);
                                    }
                                    if (mappedParam == "IV_FMM_Act_ID" && selectedDashboard == "0_FMM_Appr_Config")
                                    {
                                        // Get_Calc_Type(taskResult);
                                    }

                                    if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "3_FMM_Model_Dialog_Update")
                                    {
                                        //setupUpdateModelDialog(ref taskResult);
                                    }

                                }

                                //if a prior dependency change was detected as part of this loop, update the corresponding defaults
                                if (mappedParam == "IV_FMM_Cube_ID" && selectedDashboard == "0_FMM_Cube_Config" && priorDependencyChanged)
                                {
                                    // Load_Cube_Settings(taskResult);
                                }
                                if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "0_FMM_Model" && priorDependencyChanged)
                                {
                                    // Get_Calc_Type(taskResult);
                                }
                                if (mappedParam == "IV_FMM_Act_ID" && selectedDashboard == "0_FMM_Appr_Config" && priorDependencyChanged)
                                {
                                    // Get_Calc_Type(taskResult);
                                }
                                if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "3_FMM_Model_Dialog_Update" && priorDependencyChanged)
                                {
                                    //setupUpdateModelDialog(ref taskResult);
                                }


                            }
                            else
                            {
                                if (ARContainsKey && isValidParamValue(ARVal))
                                {
                                    UpdateCustomSubstVar(ref taskResult, param, ARVal);
                                }
                                else if (PRContainsKey && isValidParamValue(PRVal))
                                {
                                    UpdateCustomSubstVar(ref taskResult, param, PRVal);
                                }
                                else
                                {
                                    //get default param value
                                    string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                                    UpdateCustomSubstVar(ref taskResult, param, paramDefault);
                                }


                            }
                        }
                        else
                        {
                            //							BRApi.ErrorLog.LogMessage(si,"Prior dependency changed, setting default for: " + param);
                            // get default param if prior dependency changed
                            string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                            UpdateCustomSubstVar(ref taskResult, param, paramDefault);
                            UpdateCustomSubstVar(ref taskResult, mappedParam, paramDefault);

                            // if a dependency changed and there's a mapped param, make sure the associated information is updated to defaults
                            if (mappedParam == "IV_FMM_Cube_ID" && selectedDashboard == "0_FMM_Cube_Config")
                            {
                                // Load_Cube_Settings(taskResult);
                            }
                            if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "0_FMM_Model")
                            {
                                //Get_Calc_Type(taskResult);
                            }
                            if (mappedParam == "IV_FMM_Act_ID" && selectedDashboard == "0_FMM_Appr_Config")
                            {
                                //Get_Calc_Type(taskResult);
                            }
                            if (mappedParam == "IV_FMM_Model_ID" && selectedDashboard == "3_FMM_Model_Dialog_Update")
                            {
                                ///setupUpdateModelDialog(ref taskResult);
                            }
                        }
                    }
                }
            }
        }

        private bool isValidParamValue(string value)
        {
            if (value == null || value == string.Empty)
            {
                return false;
            }
            try
            {
                return Int32.Parse(value) > 0;
            }
            catch
            {
                // if we can't convert the param to an integer, that likely means it's a string value
                // there is currently no logic to tell whether a string param is a valid value, so return true by default
                return true;
            }
        }
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
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"SELECT ProfileName 
        							FROM WorkflowProfileHierarchy
        							WHERE HierarchyLevel = 1
									AND IsTemplate = 0 ";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, root_Profile_DT, sql, sqlparams);
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
                    return db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_trv_WF_Profile", "NA");
                }
                else
                {
                    var wf_Profile_DT = new DataTable("wf_profile");

                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable

                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"SELECT prof.ProfileKey,prof.ProfileName
									FROM WorkflowProfileHierarchy prof
									WHERE prof.HierarchyLevel = 1
								    AND prof.IsTemplate = 0
									AND prof.ProfileName = @root_ProfileName";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@root_ProfileName", SqlDbType.NVarChar,100) { Value = root_ProfileName }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wf_Profile_DT, sql, sqlparams);
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

        private void UpdateCustomSubstVar(ref XFLoadDashboardTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
                globals.SetStringValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
                globals.SetStringValue(key, value);
            }
        }
        private string getDefaultParam(string param, Dictionary<string, string> customsubstvars)
        {

            // replace IV with associated BL
            // IV should only be passed in if it's a value coming from a SQL Table Editor
            if (param.Contains("IV_"))
            {
                param = param.Replace("IV_", "BL_");
            }


            DashboardParamDisplayInfo paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, customsubstvars, args.PrimaryDashboard.WorkspaceID, param);
            //			BRApi.ErrorLog.LogMessage(si, "testing stuff for " + param + ": " + paramInfo?.ComboBoxItemsForBoundList?.Count);
            //BRApi.ErrorLog.LogMessage(si, "testing stuff 2: " + param + " " + paramInfo?.ListBoxItemsForBoundList?.Count);
            if (paramInfo?.ComboBoxItemsForBoundList?.Count > 0)
            {
                //				BRApi.ErrorLog.LogMessage(si,"Hit " + paramInfo.ComboBoxItemsForBoundList.First().Value.ToString());
                return paramInfo.ComboBoxItemsForBoundList.First().Value.ToString();
            }

            return "0";

        }
        #endregion
        #endregion
    }
}