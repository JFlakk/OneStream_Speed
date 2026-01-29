
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Config_Load_DB
{
    public class MainClass
    {
        private string MainMenuParam = "DL_FMM_Setup_Options";
        private string CubeConfigSubMenuParam = "DL_FMM_Cube_Config_Options";

        private string CubeConfigOpt = "FMM_Cube_Settings";

        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            {"BL_FMM_Setup_CubeID", "IV_FMM_CubeID"},
            {"BL_FMM_CubeID", "IV_FMM_CubeID"},
            {"BL_FMM_Table_CubeID", "IV_FMM_CubeID"},
            {"BL_FMM_ActID", "IV_FMM_ActID"},
            {"BL_FMM_Table_ActID", "IV_FMM_ActID"},
            {"BL_FMM_ModelID", "IV_FMM_ModelID"},
            {"BL_FMM_ModelGrpSeqID", "IV_FMM_ModelGrpSeqID"},
            {"BL_FMM_ModelGrpID","IV_FMM_ModelGrpID"}
        };

        // key string is dialog name, string array is list of IVs associated to textboxes that should be set to empty strings
        private Dictionary<string, string[]> clearTextBoxDict = new Dictionary<string, string[]>() {
            {"1_FMM_Model_Dialog_Add", new string[] {"IV_FMM_Model_Name"}},
            {"1_FMM_Cube_Config_Dialog_Add", new string[] {"IV_FMM_Cube_Descr"}},
            {"1_FMM_Model_Grp_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Name"}},
            {"1_FMM_Model_Grp_Seq_Dialog_Add", new string[] {"IV_FMM_Model_Grp_Seq_Name"}},

        };

        private Dictionary<string, Dictionary<int, string[]>> HierarchyDict = new Dictionary<string, Dictionary<int, string[]>>();
        //Dictionary<int, string[]> DependencyDict = new Dictionary<int, string[]>();

        // dependency hierarchys per page
        private Dictionary<int, string[]> CubeConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_Setup_CubeID"}}
        };

        private Dictionary<int, string[]> ApprovalConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"IV_FMM_ApprID"}}
        };

        private Dictionary<int, string[]> UnitAcctConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_Table_CubeID"}},
            {1, new string[] {"BL_FMM_Table_ActID"}},
            {2, new string[] {"IV_FMM_UnitID"}}
        };

        private Dictionary<int, string[]> RegisterConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_Table_CubeID"}},
            {1, new string[] {"BL_FMM_Table_ActID"}}
        };

        private Dictionary<int, string[]> BuildModel = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"BL_FMM_ActID"}},
            {2, new string[] {"BL_FMM_ModelID"}},
            {3, new string[] {"BL_FMM_CalcID"}}
        };

        private Dictionary<int, string[]> BuildModelGroup = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"BL_FMM_ModelGrpID"}}
        };

        private Dictionary<int, string[]> BuildModelGroupSeq = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"BL_FMM_ModelGrpSeqID"}}
        };

        private Dictionary<int, string[]> AddCube = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_All_Cube_Names"}},
            {1, new string[] {"BL_FMM_ScenTypes"}}
        };

        private Dictionary<int, string[]> CopyModel = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_Src_CubeID"}},
            {1, new string[] {"BL_FMM_Src_ActID"}},
            {2, new string[] {"BL_FMM_Src_ModelID"}},
            {3, new string[] {"BL_FMM_Src_CalcIDs"}},
            {4, new string[] {"BL_FMM_Tgt_CubeID"}},
            {5, new string[] {"BL_FMM_Tgt_ActID"}},
            {6, new string[] {"BL_FMM_Tgt_ModelID"}}
        };

        private Dictionary<int, string[]> UpdateModel = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"BL_FMM_ActID"}},
            {2, new string[] {"BL_FMM_ModelID"}},
        };

        private Dictionary<int, string[]> UpdateModelGroup = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"BL_FMM_ModelID"}},
        };

        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        #endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            //setup HierarchyDict
            HierarchyDict.Add("FMM_Cube_Config", CubeConfig);
            HierarchyDict.Add("FMM_Unit_and_Acct_Config", UnitAcctConfig);
            HierarchyDict.Add("FMM_Appr_Config", ApprovalConfig);
            HierarchyDict.Add("FMM_Reg_Col_Config", RegisterConfig);
            HierarchyDict.Add("FMM_Model", BuildModel);
            HierarchyDict.Add("FMM_Model_Grp", BuildModelGroup);
            HierarchyDict.Add("FMM_ModelGrpSeq", BuildModelGroupSeq);

            // setup dialogs for hierarchy dict
            HierarchyDict.Add("FMM_Model_Dialog_Copy", CopyModel);
            HierarchyDict.Add("1_FMM_Cube_Config_Dialog_Add", AddCube);
            HierarchyDict.Add("3_FMM_Model_Dialog_Update", UpdateModel); // Need to make sure all items on the dialog are set to refresh the dashboard AND dialog to make sure information is reloaded appropriately
                                                                         //			HierarchyDict.Add("1_FMM_Register_Col_Config_Copy", CopyRegisterConfig); // TODO: Based on requirements for copy, add a new dictionary above
                                                                         //			HierarchyDict.Add("3_FMM_Model_Grp_Dialog_Update", UpdateModelGroup); // TODO: Dialog needs addition of Model Group selection to udpate model group name
                                                                         //			HierarchyDict.Add("2_FMM_Model_Grp_Dialog_Copy",CopyModelGroup); // TODO: 



            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        // Implement Load Dashboard logic here.
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_FMM_Dashboard"))
                        {
                            var load_Dashboard_Task_Result = Load_Dashboard("", ref args);
                            return load_Dashboard_Task_Result;
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
        private XFLoadDashboardTaskResult Load_Dashboard(string RunType, ref DashboardExtenderArgs args)
        {
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
            clearParams(ref args, ref Load_Dashboard_Task_Result);
            setParams(ref args, ref Load_Dashboard_Task_Result);

            updateShowHide(ref args, ref Load_Dashboard_Task_Result);

            return Load_Dashboard_Task_Result;

        }

        #region "Setup Helpers"
        private XFLoadDashboardTaskResult Load_Cube_Settings(XFLoadDashboardTaskResult Load_Dashboard_Task_Result)
        {
            var XF_Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            XF_Load_Dashboard_Task_Result = Load_Dashboard_Task_Result;

            var cube_Settings_DT = new DataTable("Cube_Settings");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);


            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable

                    var sqa = new SqlDataAdapter();
                    // Define the select query and parameters
                    var sql = @"
				       	SELECT *
				    	FROM FMM_Cube_Config
						WHERE CubeID = @CubeID";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt32(Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFGetValue("IV_FMM_CubeID"))}
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, cube_Settings_DT, sql, sqlparams);
                }
            }
            catch
            {

            }

            if (cube_Settings_DT.Rows.Count > 0)
            {
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Cube", cube_Settings_DT.Rows[0]["Cube"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Scen_Type", cube_Settings_DT.Rows[0]["Scen_Type"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Entity_MFB", cube_Settings_DT.Rows[0]["Entity_MFB"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Cube_Descr", cube_Settings_DT.Rows[0]["Descr"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Create_Date", cube_Settings_DT.Rows[0]["Create_Date"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Create_User", cube_Settings_DT.Rows[0]["Create_User"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Update_Date", cube_Settings_DT.Rows[0]["Update_Date"].ToString());
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Update_User", cube_Settings_DT.Rows[0]["Update_User"].ToString());
            }
            else
            {
                BRApi.ErrorLog.LogMessage(si, "No CubeID likely");
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Cube", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Scen_Type", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Entity_MFB", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Create_Date", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Create_User", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Update_Date", string.Empty);
                XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_FMM_Update_User", string.Empty);
            }

            return XF_Load_Dashboard_Task_Result;
        }

        private XFLoadDashboardTaskResult Get_Calc_Type(XFLoadDashboardTaskResult Load_Dashboard_Task_Result)
        {
            var XF_Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            XF_Load_Dashboard_Task_Result = Load_Dashboard_Task_Result;

            var calc_Type_DT = new DataTable("Calc_Type");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            try
            {

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable

                    var sqa = new SqlDataAdapter();
                    // Define the select query and parameters
                    var sql = @"
								        	SELECT Calc_Type
								       		FROM FMM_Cube_Config Con
											JOIN FMM_Act_Config Act
											ON Con.CubeID = Act.CubeID
											WHERE Con.CubeID = @CubeID
											AND Act.ActID = @ActID";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt32(XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFGetValue("IV_FMM_CubeID","0"))},
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = Convert.ToInt32(XF_Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActID","0"))}
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, calc_Type_DT, sql, sqlparams);
                }
            }
            catch
            {
                BRApi.ErrorLog.LogMessage(si, "No cube selected likely, no calc information available");
            }

            if (calc_Type_DT.Rows.Count > 0)
            {
                UpdateCustomSubstVar(ref XF_Load_Dashboard_Task_Result, "DL_FMM_Calc_Type", calc_Type_DT.Rows[0]["Calc_Type"].ToString());
            }
            else
            {
                UpdateCustomSubstVar(ref XF_Load_Dashboard_Task_Result, "DL_FMM_Calc_Type", "Table");
            }
            return XF_Load_Dashboard_Task_Result;
        }

        private void setupUpdateModelDialog(ref XFLoadDashboardTaskResult taskResult)
        {

            // Get model name
            string modelName = "";
            int cubeID = 0;
            int activityID = 0;
            int modelID = 0;

            var ModelDT = new DataTable("Models");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            //			DashboardParamDisplayInfo paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, taskResult.ModifiedCustomSubstVars, args.PrimaryDashboard.WorkspaceID, "BL_FMM_ModelID");
            //			string modelID = taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ModelID");
            //			BRApi.ErrorLog.LogMessage(si, "model ID: " + modelID);

            //			if (paramInfo?.ComboBoxItemsForBoundList?.Count > 0) {
            //				modelName = paramInfo.ComboBoxItemsForBoundList.First().Value.ToString();
            //				string tempModelName = paramInfo.ComboBoxItemsForBoundList.Find(x => {
            //					return x.Value == modelID;
            //				}).Name;
            //				BRApi.ErrorLog.LogMessage(si, "model name temp: " + tempModelName);
            //				BRApi.ErrorLog.LogMessage(si, "model name param: " + modelName);
            //			}

            try
            {
                cubeID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_CubeID"));
                activityID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActID"));
                modelID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ModelID"));
            }
            catch
            {
                BRApi.ErrorLog.LogMessage(si, "error converting");
            }


            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable

                    var sqa = new SqlDataAdapter();
                    // Define the select query and parameters
                    var sql = @"
				       	SELECT *
				    	FROM FMM_Models
						WHERE CubeID = @CubeID
						AND ActID = @ActID
						AND ModelID = @ModelID";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = cubeID},
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = activityID},
                        new SqlParameter("@ModelID", SqlDbType.Int) { Value = modelID},

                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, ModelDT, sql, sqlparams);
                }
            }
            catch (Exception e)
            {

            }


            if (ModelDT.Rows.Count > 0)
            {
                modelName = ModelDT.Rows[0]["Name"].ToString();
            }
            BRApi.ErrorLog.LogMessage(si, "setting model name: " + modelName);

            UpdateCustomSubstVar(ref taskResult, "IV_FMM_Model_Name", modelName);
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

        #endregion

        #endregion


        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string showHideIVName = "IV_FMM_Show_Hide_Menu_Btn";
            string showBtnVisibleName = "IV_FMM_Display_Show_Menu_Btn";
            string hideBtnVisibleName = "IV_FMM_Display_Hide_Menu_Btn";
            string menuWidthIV = "IV_FMM_Menu_Width";


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

            string CubeSubMenuSelection = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(CubeConfigSubMenuParam) != string.Empty ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(CubeConfigSubMenuParam) : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(CubeConfigSubMenuParam);

            // check if cube menu option (secondary layer) has a real selection. If so, use it.
            string selectedDashboard = MainMenuSelection == CubeConfigOpt ? CubeSubMenuSelection : MainMenuSelection;

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
                                    if (mappedParam == "IV_FMM_CubeID" && selectedDashboard == "FMM_Cube_Config")
                                    {
                                        Load_Cube_Settings(taskResult);
                                    }
                                    if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "FMM_Model")
                                    {
                                        Get_Calc_Type(taskResult);
                                    }
                                    if (mappedParam == "IV_FMM_ActID" && selectedDashboard == "FMM_Appr_Config")
                                    {
                                        Get_Calc_Type(taskResult);
                                    }

                                    if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "3_FMM_Model_Dialog_Update")
                                    {
                                        setupUpdateModelDialog(ref taskResult);
                                    }

                                }

                                //if a prior dependency change was detected as part of this loop, update the corresponding defaults
                                if (mappedParam == "IV_FMM_CubeID" && selectedDashboard == "FMM_Cube_Config" && priorDependencyChanged)
                                {
                                    Load_Cube_Settings(taskResult);
                                }
                                if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "FMM_Model" && priorDependencyChanged)
                                {
                                    Get_Calc_Type(taskResult);
                                }
                                if (mappedParam == "IV_FMM_ActID" && selectedDashboard == "FMM_Appr_Config" && priorDependencyChanged)
                                {
                                    Get_Calc_Type(taskResult);
                                }
                                if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "3_FMM_Model_Dialog_Update" && priorDependencyChanged)
                                {
                                    setupUpdateModelDialog(ref taskResult);
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
                            if (mappedParam == "IV_FMM_CubeID" && selectedDashboard == "FMM_Cube_Config")
                            {
                                Load_Cube_Settings(taskResult);
                            }
                            if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "FMM_Model")
                            {
                                Get_Calc_Type(taskResult);
                            }
                            if (mappedParam == "IV_FMM_ActID" && selectedDashboard == "FMM_Appr_Config")
                            {
                                Get_Calc_Type(taskResult);
                            }
                            if (mappedParam == "IV_FMM_ModelID" && selectedDashboard == "3_FMM_Model_Dialog_Update")
                            {
                                setupUpdateModelDialog(ref taskResult);
                            }
                        }
                    }
                }
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

        #endregion
    }
}