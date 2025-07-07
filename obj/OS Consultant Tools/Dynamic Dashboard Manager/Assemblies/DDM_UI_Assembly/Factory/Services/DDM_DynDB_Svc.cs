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
	public class DDM_DynDB_Svc : IWsasDynamicDashboardsV800
	{
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
		            return storedDashboard.Name switch
		            {
		                // Dynamic Navigation Menu
		                "0a_DDM_Profile_Config_Header" => DDM_Header.Get_DynamicNavMenuLabel_Actions(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		                // General header
		                "0_DDM_App_Header" => DDM_Header.Get_DynamicNavMenu_Actions(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		                // Label & Menu hide/show
		                "0a_DDM_App_Header_MenuLabel" => DDM_Header.Get_DynamicNavMenuLabel_Actions(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		                // configurable header items
		                "0b_DDM_App_Header_ConfigItems" => DDM_Header.Get_DynamicHeader(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		
		
		
		                // Dynamic Content
		                "1a_DDM_App_Content_DB" => DDM_Content.Get_DynamicContent(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		
		                //                // Color Dynamic
		                //                "2000_ColorDynamic_Content" => ColorDynamicDashboard.Get_2000_ColorDynamic_Content_DynamicDashboard(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		
		                //                // Dynamic Navigation Menu
		                //                "0210_DynamicNavMenu_Actions" => DynamicNavMenuDashboard.Get_0210_DynamicNavMenu_Actions_DynamicDashboard(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		
		                //                // Dynamic Adv Filters
		                //                "0220_AdvFilters_Content" => DynamicAdvFiltersDashboard.Get_0220_AdvFilters_Content_DynamicDashboard(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		                //                "0200_SectionContent" => DynamicAdvFiltersDashboard.Get_0200_SectionContent_DynamicDashboard(si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved),
		
		                _ => api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject)
		            };
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
		            //BRApi.ErrorLog.LogMessage(si, "dashboard name: " + dynamicDashboardEx.DynamicDashboard.BasedOnName);
		            return dynamicDashboardEx.DynamicDashboard.BasedOnName switch
		            {
		
		                // Dynamic Navigation Menu
		                "0a_DDM_Profile_Config_Header" => DDM_Header.Get_DynamicNavMenuLabel_Actions_DynamicComponents(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                // Label & Menu hide/show
		                //"DDM_App_Header_MenuLabel" => DDM_Header.Get_Dynamic_Menu_Label(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		                // configurable header items
		                "0b_DDM_App_Header_ConfigItems" => DDM_Header.Get_Dynamic_Header_Components(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		
		
		                // Dynamic Content
		                "DDM_App_Content" => DDM_Content.Get_DynamicContent_DynamicComponents(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		
		
		                // Dynamic Form
		                // "0000_GeneratedForm" => DDM_Load_Dashboard_Helper.Get_0000_GeneratedForm_DynamicComponents(si, api, workspace, maintUnit, dynamicDashboardEx, customSubstVarsAlreadyResolved),
		
		                _ => api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters)
		            };
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
                    return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx,string.Empty,dynamicComponentEx.TemplateSubstVars, TriStateBool.FalseValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
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
