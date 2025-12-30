using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.SQM_Load_Dashboard_Helper
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
                        if (args.FunctionName.XFEqualsIgnoreCase("Load_SQM_Dashboard"))
                        {
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
            var Load_Dashboard_Task_Result = new XFLoadDashboardTaskResult();
            if (RunType.XFEqualsIgnoreCase("Initial"))
            {
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_SQM_Show_Hide_Menu_Btn", "Show");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_SQM_Display_Show_Menu_Btn", "False");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_SQM_Display_Hide_Menu_Btn", "True");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_SQM_Menu_Width", "Auto");
                Load_Dashboard_Task_Result.ModifiedCustomSubstVars.Add("IV_txt_SQL_Search", String.Empty);
            }
            else if (RunType.XFEqualsIgnoreCase("Post-Initial"))
            {
                Load_Dashboard_Task_Result.ChangeCustomSubstVarsInDashboard = true;
                //Checks current value of Show Menu param against prior run
                BRApi.ErrorLog.LogMessage(si, "hit Post-Initial ");
                var show_hide_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.GetValueOrEmpty("IV_SQM_Show_Hide_Menu_Btn");
                var new_display_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.GetValueOrEmpty("IV_SQM_Show_Hide_Menu_Btn");
                var prior_run_show_val = db_args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.GetValueOrEmpty("IV_SQM_Display_Show_Menu_Btn");
                BRApi.ErrorLog.LogMessage(si, "hit Show: " + show_hide_val);
                if (!string.IsNullOrEmpty(show_hide_val))
                {
                    if (show_hide_val == "Hide")
                    {
                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Display_Show_Menu_Btn"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Display_Show_Menu_Btn", "True");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Display_Show_Menu_Btn", "True");
                        }

                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Display_Hide_Menu_Btn"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Display_Hide_Menu_Btn", "False");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Display_Hide_Menu_Btn", "False");
                        }
                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Menu_Width"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Menu_Width", "0");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Menu_Width", "0");
                        }
                    }
                    else if (show_hide_val == "Show")
                    {
                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Display_Show_Menu_Btn"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Display_Show_Menu_Btn", "False");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Display_Show_Menu_Btn", "False");
                        }

                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Display_Hide_Menu_Btn"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Display_Hide_Menu_Btn", "True");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Display_Hide_Menu_Btn", "True");
                        }
                        if (Load_Dashboard_Task_Result.ModifiedCustomSubstVars.ContainsKey("IV_SQM_Menu_Width"))
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.XFSetValue("IV_SQM_Menu_Width", "Auto");
                        }
                        else
                        {
                            Load_Dashboard_Task_Result.ModifiedCustomSubstVars.TryAdd("IV_SQM_Menu_Width", "Auto");
                        }
                    }
                }
            }

            return Load_Dashboard_Task_Result;
        }
        #endregion
        #endregion
    }
}