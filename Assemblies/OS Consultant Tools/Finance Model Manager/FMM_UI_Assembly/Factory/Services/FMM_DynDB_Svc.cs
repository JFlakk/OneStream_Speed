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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class FMM_DynDB_Svc : IWsasDynamicDashboardsV800
    {
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    if (storedDashboard.Name.XFEqualsIgnoreCase("FMM_Model_Content_Cube_R1C1R2R3R2"))
                    {
                        // retrieve our items
                        var src_CellDB = new FMM_Src_CellDB(si);
                        var src_Cells = src_CellDB.GetSrcCellsByCalcId(1, 1);

                        // prepare a list of repeated components			
                        var repeatArgs = new List<WsDynamicComponentRepeatArgs>();

                        // Get configuration to determine which properties to use
                        var configHelper = new FMM_Config_Helpers();
                        var enabledProperties = configHelper.GetEnabledSrcProperties(1); // calcType = 1 (Table)

                        // loop through our items, populating dictionaries that will contain Parameter values for each "row"
                        foreach (FMM_Src_CellModel cellModel in src_Cells)
                        {
                            Dictionary<string, string> nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>();

                            // Always add Cell_ID as the primary identifier
                            nextLevelTemplateSubstVarsToAdd["Cell_ID"] = cellModel.Cell_ID.ToString();
                            nextLevelTemplateSubstVarsToAdd["Calc_ID"] = cellModel.Calc_ID.ToString();
                            nextLevelTemplateSubstVarsToAdd["Src_Order"] = cellModel.Src_Order.ToString();

                            // Add enabled properties from configuration
                            foreach (var propName in enabledProperties)
                            {
                                string propValue = string.Empty;
                                switch (propName)
                                {
                                    case "Sequence":
                                    case "Name":
                                    case "Explanation":
                                    case "Condition":
                                    case "MultiDim_Alloc":
                                        // These are mapped properties from the config but may not directly map to model properties
                                        // For now, we'll skip them or handle them specially
                                        break;
                                    case "Entity":
                                        propValue = cellModel.Entity ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Cons":
                                        propValue = cellModel.Cons ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Scenario":
                                        propValue = cellModel.Scenario ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Time":
                                        propValue = cellModel.Time ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "View":
                                        propValue = cellModel.View ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Acct":
                                        propValue = cellModel.Acct ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "IC":
                                        propValue = cellModel.IC ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Origin":
                                        propValue = cellModel.Origin ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "Flow":
                                        propValue = cellModel.Flow ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD1":
                                        propValue = cellModel.UD1 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD2":
                                        propValue = cellModel.UD2 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD3":
                                        propValue = cellModel.UD3 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD4":
                                        propValue = cellModel.UD4 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD5":
                                        propValue = cellModel.UD5 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD6":
                                        propValue = cellModel.UD6 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD7":
                                        propValue = cellModel.UD7 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "UD8":
                                        propValue = cellModel.UD8 ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                    case "DB_Name":
                                        propValue = cellModel.DB_Name ?? string.Empty;
                                        nextLevelTemplateSubstVarsToAdd[propName] = propValue;
                                        break;
                                }
                            }

                            repeatArgs.Add(new WsDynamicComponentRepeatArgs(
                                cellModel.Cell_ID.ToString(),
                                nextLevelTemplateSubstVarsToAdd));
                        }

                        WsDynamicDashboardEx contentDashboard = api.GetEmbeddedDynamicDashboard(si,
                            workspace, parentDynamicComponentEx, storedDashboard, string.Empty,
                            null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                        // attach our List of repeaters 
                        contentDashboard.DynamicDashboard.Tag = repeatArgs;

                        // save the state and return the dashboard
                        api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, contentDashboard, WsDynamicItemStateType.NotUsed);
                        if (contentDashboard.DynamicDashboard.Dashboard != null)
                            return contentDashboard;
                        return null;
                    }
                    else
                    {
                        return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicComponentCollection GetDynamicComponentsForDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicAdapterCollection GetDynamicAdaptersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicCubeViewEx GetDynamicCubeViewForDynamicAdapter(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicAdapterEx dynamicAdapterEx, CubeViewItem storedCubeViewItem, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx, storedCubeViewItem, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicParameterCollection GetDynamicParametersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.FalseValue, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}