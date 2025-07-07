﻿using System;
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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using OneStreamWorkspacesApi.V820;
using Microsoft.Data.SqlClient;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Content
    {

        //Params
        private const string Param_CubeViewName = "IV_DDM_App_Cube_View_Name_Test";

        private const string Param_DashboardName = "IV_DDM_App_Dynamic_DB_Name";

        private const string CV_DashboardName = "DDM_Dyn_Dashboard_CV";

        public object Main(SessionInfo si)
        {
            try
            {
                // Orchestrate dashboard logic here
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        internal static XFSelectionChangedTaskResult OnMenuSelectionChanged(SessionInfo si, DashboardExtenderArgs args)
        {

            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var ProfileKey = wfUnitPk.ProfileKey;
            int configProfileID = DDM_Support.getCurrentProfileID(si, ProfileKey);

            int menuOptionID = DDM_Support.getSelectedMenuOption(si, args.SelectionChangedTaskInfo.CustomSubstVars);


            Dictionary<string, string> ParamsToAdd = DDM_Support.getParamsToAdd(DDM_Support.getHeaderItems(si, args.SelectionChangedTaskInfo.CustomSubstVars));

            // get cube name based on SI.
            int cubeID = si.PovDataCellPk.CubeId;
            var cubeName = DDM_Support.getCubeName(si, cubeID);

            // add cubename IV
            taskResult.ModifiedCustomSubstVars.Add(DDM_Support.Param_CubeName, cubeName);


            foreach (string param in ParamsToAdd.Keys)
            {
                taskResult.ModifiedCustomSubstVars.Add(param, ParamsToAdd[param]);
            }
            return taskResult;
        }

        internal static WsDynamicDashboardEx Get_DynamicContent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {

            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var ProfileKey = wfUnitPk.ProfileKey;
            int curr_Profile_ID = DDM_Support.getCurrentProfileID(si, ProfileKey);

            int menuOptionID = DDM_Support.getSelectedMenuOption(si, customSubstVarsAlreadyResolved);

            var config_Menu_DT = DDM_Support.getConfigMenu(si, curr_Profile_ID, menuOptionID);
            var dashboardName = "Default";
            var cubeViewName = "Default";

            if (config_Menu_DT.Rows.Count > 0)
            {
                var optType = config_Menu_DT.Rows[0]["Option_Type"].ToString();

                if (optType == "Dashboard")
                {
                    dashboardName = config_Menu_DT.Rows[0]["DB_Name"].ToString();
                }
                else if (optType == "Cube View")
                {
                    dashboardName = CV_DashboardName;
                    cubeViewName = config_Menu_DT.Rows[0]["CV_Name"].ToString();
                }
            }

            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();


            var nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>
            {
                [Param_DashboardName] = dashboardName,
                [Param_CubeViewName] = cubeViewName,

            };
            repeatArgsList.Add(new WsDynamicComponentRepeatArgs(dashboardName, nextLevelTemplateSubstVarsToAdd));

            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }
		
        internal static WsDynamicComponentCollection Get_DynamicContent_DynamicComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
            var componentCollection = api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return componentCollection;
        }

    }
}