using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Engine;
using Workspace.OSConsTools.GBL_UI_Assembly;
using Workspace.__WsNamespacePrefix.__WsAssemblyName;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_ConfigLoadDB
{
    public class MainClass
    {
        private string MainMenuParam = "DL_FMM_SetupOptions";
        private string CubeConfigSubMenuParam = "DL_FMM_CubeConfig_Options";
        private string CubeConfigOpt = "FMM_CubeSettings";

        private Dictionary<string, string> paramMap = new Dictionary<string, string>()
        {
            {"BL_FMM_CubeID_Setup", "IV_FMM_CubeID"},
            {"BL_FMM_CubeID", "IV_FMM_CubeID"},
            {"BL_FMM_CubeID_Table", "IV_FMM_CubeID"},
            {"BL_FMM_ActID", "IV_FMM_ActID"},
            {"BL_FMM_ActID_Table", "IV_FMM_ActID"},
            {"BL_FMM_ModelID", "IV_FMM_ModelID"},
            {"BL_FMM_ModelGrpSeqID", "IV_FMM_ModelGrpSeqID"},
            {"BL_FMM_ModelGrpID", "IV_FMM_ModelGrpID"}
        };

        private Dictionary<string, Dictionary<int, string[]>> HierarchyDict = new Dictionary<string, Dictionary<int, string[]>>();

        private Dictionary<int, string[]> CubeConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"IV_FMM_CubeConfig_AddUpdate"}},
            {1, new string[] {"BL_FMM_CubeID"}}
        };

        private Dictionary<int, string[]> ApprovalConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID"}},
            {1, new string[] {"IV_FMM_ApprID"}}
        };

        private Dictionary<int, string[]> UnitAcctConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID_Table"}},
            {1, new string[] {"BL_FMM_ActID_Table"}},
            {2, new string[] {"IV_FMM_UnitID"}}
        };

        private Dictionary<int, string[]> RegisterConfig = new Dictionary<int, string[]>()
        {
            {0, new string[] {"BL_FMM_CubeID_Table"}},
            {1, new string[] {"BL_FMM_ActID_Table"}}
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

        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private readonly GBL_Helpers gblHelpers = new GBL_Helpers();

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            HierarchyDict.Add("FMM_CubeConfig", CubeConfig);
            HierarchyDict.Add("FMM_UnitAcctConfig", UnitAcctConfig);
            HierarchyDict.Add("FMM_CustTableDef", UnitAcctConfig);
            HierarchyDict.Add("FMM_CustTableAssign", UnitAcctConfig);
            HierarchyDict.Add("FMM_Appr", ApprovalConfig);
            HierarchyDict.Add("FMM_RegCol", RegisterConfig);
            HierarchyDict.Add("FMM_DatVal", UnitAcctConfig);
            HierarchyDict.Add("FMM_Model", BuildModel);
            HierarchyDict.Add("FMM_ModelGrp", BuildModelGroup);
            HierarchyDict.Add("FMM_ModelGrpSeq", BuildModelGroupSeq);
            HierarchyDict.Add("FMM_Model_Dialog_Copy", CopyModel);
            HierarchyDict.Add("3_FMM_Model_Dialog_Update", UpdateModel);

            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;

                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_FMM_DB"))
                        {
                            var loadDbTaskResult = Load_Dashboard(string.Empty, ref args);
                            return loadDbTaskResult;
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

        private XFLoadDashboardTaskResult Load_Dashboard(string runType, ref DashboardExtenderArgs args)
        {
            var loadDbTaskResult = new XFLoadDashboardTaskResult
            {
                ChangeCustomSubstVarsInDashboard = true
            };

            clearParams(ref args, ref loadDbTaskResult);
            setParams(ref args, ref loadDbTaskResult);
            updateShowHide(ref args, ref loadDbTaskResult);

            return loadDbTaskResult;
        }

        private void Load_CubeConfig(ref XFLoadDashboardTaskResult loadDbTaskResult)
        {
            FMM_ConfigHelpers.SetCubeConfigParams(si, loadDbTaskResult.ModifiedCustomSubstVars);
        }

        private XFLoadDashboardTaskResult Get_CalcType(XFLoadDashboardTaskResult loadDbTaskResult)
        {
            var xfLoadDbTaskResult = loadDbTaskResult;
            var calcTypeTable = new DataTable("CalcType");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sqlGblGetDatasets = new SQL_GBL_Get_DataSets(si, connection);
                    var adapter = new SqlDataAdapter();
                    var sql = @"SELECT CalcType
                                FROM FMM_CubeConfig Cb
                                JOIN FMM_ActConfig Act
                                    ON Cb.CubeID = Act.CubeID
                                WHERE Cb.CubeID = @CubeID
                                    AND Act.ActID = @ActID";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int)
                        {
                            Value = Convert.ToInt32(xfLoadDbTaskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_CubeID", "0"))
                        },
                        new SqlParameter("@ActID", SqlDbType.Int)
                        {
                            Value = Convert.ToInt32(xfLoadDbTaskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActID", "0"))
                        }
                    };

                    sqlGblGetDatasets.Fill_Get_GBL_DT(si, adapter, calcTypeTable, sql, parameters);
                }
            }
            catch
            {
            }

            if (calcTypeTable.Rows.Count > 0)
            {
                gblHelpers.UpdateCustomSubstVar(ref xfLoadDbTaskResult, globals, "DL_FMM_CalcType", calcTypeTable.Rows[0]["CalcType"].ToString());
            }
            else
            {
                gblHelpers.UpdateCustomSubstVar(ref xfLoadDbTaskResult, globals, "DL_FMM_CalcType", "Table");
            }

            return xfLoadDbTaskResult;
        }

        private void setupUpdateModelDialog(ref XFLoadDashboardTaskResult taskResult)
        {
            string modelName = string.Empty;
            int cubeID = 0;
            int activityID = 0;
            int modelID = 0;

            var modelTable = new DataTable("Models");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

            try
            {
                cubeID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_CubeID"));
                activityID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ActID"));
                modelID = Convert.ToInt32(taskResult.ModifiedCustomSubstVars.XFGetValue("IV_FMM_ModelID"));
            }
            catch
            {
            }

            try
            {
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sqlGblGetDatasets = new SQL_GBL_Get_DataSets(si, connection);
                    var adapter = new SqlDataAdapter();
                    var sql = @"SELECT *
                                FROM FMM_Models
                                WHERE CubeID = @CubeID
                                    AND ActID = @ActID
                                    AND ModelID = @ModelID";

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = cubeID },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = activityID },
                        new SqlParameter("@ModelID", SqlDbType.Int) { Value = modelID },
                    };

                    sqlGblGetDatasets.Fill_Get_GBL_DT(si, adapter, modelTable, sql, parameters);
                }
            }
            catch
            {
            }

            if (modelTable.Rows.Count > 0)
            {
                modelName = modelTable.Rows[0]["Name"].ToString();
            }

            gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, "IV_FMM_Model_Name", modelName);
        }

        private void updateShowHide(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string showHideIVName = "IV_FMM_Show_Hide_Menu_Btn";
            string showBtnVisibleName = "IV_FMM_Display_Show_Menu_Btn";
            string hideBtnVisibleName = "IV_FMM_Display_Hide_Menu_Btn";
            string menuWidthIV = "IV_FMM_Menu_Width";

            var arCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            string showHideIVVal = arCustomSubst.XFGetValue(showHideIVName, string.Empty);

            if (showHideIVVal == "Hide")
            {
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, showBtnVisibleName, "True");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, hideBtnVisibleName, "False");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, menuWidthIV, "0");
            }
            else if (showHideIVVal == "Show")
            {
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, showBtnVisibleName, "False");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, hideBtnVisibleName, "True");
                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, menuWidthIV, "Auto");
            }
        }

        private void clearParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
        }

        private void setParams(ref DashboardExtenderArgs args, ref XFLoadDashboardTaskResult taskResult)
        {
            string dialogSelection = args.PrimaryDashboard.Name;

            string mainMenuSelection = !string.IsNullOrEmpty(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam))
                ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(MainMenuParam)
                : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(MainMenuParam);

            string cubeSubMenuSelection = !string.IsNullOrEmpty(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(CubeConfigSubMenuParam))
                ? args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue(CubeConfigSubMenuParam)
                : args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue(CubeConfigSubMenuParam);

            string selectedDashboard = mainMenuSelection == CubeConfigOpt ? cubeSubMenuSelection : mainMenuSelection;
            selectedDashboard = HierarchyDict.ContainsKey(dialogSelection) ? dialogSelection : selectedDashboard;

            var arCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved;
            var prCustomSubst = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun;

            if (HierarchyDict.ContainsKey(selectedDashboard))
            {
                Dictionary<int, string[]> dependencyDict = HierarchyDict[selectedDashboard];
                bool priorDependencyChanged = false;

                foreach (int dependencyDepth in dependencyDict.Keys)
                {
                    foreach (string param in dependencyDict[dependencyDepth])
                    {
                        bool arContainsKey = arCustomSubst.ContainsKey(param);
                        bool prContainsKey = prCustomSubst.ContainsKey(param);
                        string arVal = arCustomSubst.XFGetValue(param);
                        string prVal = prCustomSubst.XFGetValue(param);
                        string mappedParam = paramMap.ContainsKey(param) ? paramMap[param] : string.Empty;

                        if (!priorDependencyChanged)
                        {
                            if (mappedParam != string.Empty)
                            {
                                string arMappedVal = arCustomSubst.XFGetValue(mappedParam, string.Empty);
                                string prMappedVal = prCustomSubst.XFGetValue(mappedParam, string.Empty);

                                if (prContainsKey && isValidParamValue(prVal) && isValidParamValue(prMappedVal))
                                {
                                    if (prVal != prMappedVal)
                                    {
                                        priorDependencyChanged = true;
                                    }

                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, prVal);
                                }
                                else if (arContainsKey && isValidParamValue(arVal) && isValidParamValue(arMappedVal))
                                {
                                    if (arVal != arMappedVal)
                                    {
                                        priorDependencyChanged = true;
                                    }

                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, arVal);
                                }
                                else if (arContainsKey && isValidParamValue(arVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, arVal);
                                }
                                else if (prContainsKey && isValidParamValue(prVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, prVal);
                                }
                                else
                                {
                                    string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, paramDefault);
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, paramDefault);
                                }
                            }
                            else
                            {
                                if (arContainsKey && isValidParamValue(arVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, arVal);
                                }
                                else if (prContainsKey && isValidParamValue(prVal))
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, prVal);
                                }
                                else
                                {
                                    gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, getDefaultParam(param, taskResult.ModifiedCustomSubstVars));
                                }
                            }
                        }
                        else
                        {
                            string paramDefault = getDefaultParam(param, taskResult.ModifiedCustomSubstVars);
                            gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, param, paramDefault);

                            if (mappedParam != string.Empty)
                            {
                                gblHelpers.UpdateCustomSubstVar(ref taskResult, globals, mappedParam, paramDefault);
                            }
                        }
                    }
                }
            }
        }

        private void ExecuteSpecificRefreshLogic(string dashboard, string mappedParam, ref XFLoadDashboardTaskResult taskResult)
        {
            if (mappedParam == "IV_FMM_CubeID" && dashboard == "FMM_CubeConfig")
            {
                Load_CubeConfig(ref taskResult);
            }

            if (mappedParam == "IV_FMM_ModelID" && dashboard == "FMM_Model")
            {
                Get_CalcType(taskResult);
            }

            if (mappedParam == "IV_FMM_ActID" && dashboard == "FMM_Appr_Config")
            {
                Get_CalcType(taskResult);
            }

            if (mappedParam == "IV_FMM_ModelID" && dashboard == "3_FMM_Model_Dialog_Update")
            {
                setupUpdateModelDialog(ref taskResult);
            }
        }

        private string getDefaultParam(string param, Dictionary<string, string> customSubstVars)
        {
            if (param.Contains("IV_"))
            {
                param = param.Replace("IV_", "BL_");
            }

            DashboardParamDisplayInfo paramInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, customSubstVars, args.PrimaryDashboard.WorkspaceID, param);
            if (paramInfo?.ComboBoxItemsForBoundList?.Count > 0)
            {
                return paramInfo.ComboBoxItemsForBoundList.First().Value.ToString();
            }

            return "0";
        }

        private bool isValidParamValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                return int.Parse(value) > 0;
            }
            catch
            {
                return true;
            }
        }
    }
}
