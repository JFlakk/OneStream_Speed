using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
        private const string Param_CubeViewName = "IV_DDM_App_CV_Name";

        private const string Param_DashboardName = "IV_1a_DDM_App_Content_DB";

        private const string CV_DashboardName = "1a_DDM_App_Content_CV";

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


//        internal static XFSelectionChangedTaskResult OnMenuSelectionChanged(SessionInfo si, DashboardExtenderArgs args)
//        {

//            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

//            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
//            var ProfileKey = wfUnitPk.ProfileKey;
//            int configProfileID = DDM_Support.get_CurrProfileID(si, ProfileKey);

//            int menuOptionID = DDM_Support.get_SelectedMenu(si, args.SelectionChangedTaskInfo.CustomSubstVars);


//            Dictionary<string, string> ParamsToAdd = DDM_Support.get_ParamsToAdd(DDM_Support.get_HeaderItems(si, args.SelectionChangedTaskInfo.CustomSubstVars));

//            // get cube name based on SI.
//            int cubeID = si.PovDataCellPk.CubeId;
//            var cubeName = DDM_Support.get_CubeName(si, cubeID);

//            // add cubename IV
//            taskResult.ModifiedCustomSubstVars.Add(DDM_Support.Param_CubeName, cubeName);


//            foreach (string param in ParamsToAdd.Keys)
//            {
//                taskResult.ModifiedCustomSubstVars.Add(param, ParamsToAdd[param]);
//            }
//            return taskResult;
//        }

        internal static WsDynamicComponentCollection get_DynamicComponentContent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
		{
            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var ProfileKey = wfUnitPk.ProfileKey;
            int curr_Profile_ID = DDM_Support.get_CurrProfileID(si, ProfileKey);
			

            int menuOptionID = DDM_Support.get_SelectedMenu(si, customSubstVarsAlreadyResolved);

            var config_Menu_DT = DDM_Support.get_ConfigMenu(si, menuOptionID);
            var dashboardName = "Default";
            var cubeViewName = "Default";
			
			// Get the dynamic components collection
			var dynComponents = new WsDynamicComponentCollection();
			dynComponents = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);

            if (config_Menu_DT.Rows.Count > 0)
            {
				DataRow layoutRow = config_Menu_DT.Rows[0];
                var optionTypeValue = layoutRow["Option_Type"];
				int optionType = 0;
				
				// Handle both string and int option types
				if (optionTypeValue != null && optionTypeValue != DBNull.Value)
				{
					if (optionTypeValue is string)
					{
						// Legacy string-based option types
						string optTypeStr = optionTypeValue.ToString();
						if (optTypeStr == "Dashboard")
						{
							optionType = 1; // Dashboard
						}
						else if (optTypeStr == "Cube View")
						{
							optionType = 2; // CubeView
						}
					}
					else
					{
						// New int-based option types
						optionType = Convert.ToInt32(optionTypeValue);
					}
				}

				// Process based on layout option type
				switch (optionType)
				{
					case 1: // Dashboard - Single dashboard panel
						ProcessSingleDashboard(dynComponents, layoutRow, "emb_Dynamic_1a_DDM_App_Content_DB");
						break;
						
					case 2: // CubeView - Single cube view panel
						ProcessSingleCubeView(dynComponents, layoutRow, "cv_DDM_Dynamic");
						break;
						
					case 3: // Dashboard_TopBottom - Top and bottom panels
						ProcessTopBottomLayout(dynComponents, layoutRow, dynamicDashboardEx.DynamicDashboard.Name);
						break;
						
					case 4: // Dashboard_LeftRight - Left and right panels
						ProcessLeftRightLayout(dynComponents, layoutRow, dynamicDashboardEx.DynamicDashboard.Name);
						break;
						
					case 5: // Dashboard_2Top1Bottom - Two top panels, one bottom
					case 6: // Dashboard_1Top2Bottom - One top, two bottom panels
					case 7: // Dashboard_2Left1Right - Two left panels, one right
					case 8: // Dashboard_1Left2Right - One left, two right panels
						ProcessComplexLayout(dynComponents, layoutRow, optionType, dynamicDashboardEx.DynamicDashboard.Name);
						break;
						
					case 9: // Dashboard_2x2 - Four quadrant layout
						ProcessQuadrantLayout(dynComponents, layoutRow, dynamicDashboardEx.DynamicDashboard.Name);
						break;
						
					case 10: // Custom_Dashboard - Custom dashboard
						ProcessCustomDashboard(dynComponents, layoutRow, dynamicDashboardEx.DynamicDashboard.Name);
						break;
						
					default:
						// Fallback to simple dashboard
						ProcessSingleDashboard(dynComponents, layoutRow, "emb_Dynamic_1a_DDM_App_Content_DB");
						break;
				}
            }

            return dynComponents;
        }
		
		// Process single dashboard panel
		private static void ProcessSingleDashboard(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string componentName)
		{
			var dynDBComponent = dynComponents.GetComponentUsingBasedOnName(componentName);
			if (dynDBComponent != null)
			{
				string dashboardName = layoutRow["DB_Name"]?.ToString() ?? "Default";
				dynDBComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
			}
		}
		
		// Process single cube view panel
		private static void ProcessSingleCubeView(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string componentName)
		{
			var dynDBComponent = dynComponents.GetComponentUsingBasedOnName(componentName);
			if (dynDBComponent != null)
			{
				string cubeViewName = layoutRow["CV_Name"]?.ToString() ?? "Default";
				var CV_XmlData = new XElement(componentName);
				CV_XmlData = XElement.Parse(dynDBComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
				CV_XmlData.SetElementValue("CubeViewName", cubeViewName);
				dynDBComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = CV_XmlData.ToString();
			}
		}
		
		// Process top/bottom layout
		private static void ProcessTopBottomLayout(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string dashboardName)
		{
			// Process top panel
			var topOptionType = Convert.ToInt32(layoutRow["Top_Option_Type"] ?? 0);
			if (topOptionType == 1) // Dashboard
			{
				string topDashboard = layoutRow["DB_Name_Top"]?.ToString();
				if (!string.IsNullOrEmpty(topDashboard))
				{
					var topComponent = dynComponents.GetComponentUsingBasedOnName("emb_Top_Panel");
					if (topComponent != null)
					{
						topComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = topDashboard;
					}
				}
			}
			else if (topOptionType == 2) // CubeView
			{
				string topCubeView = layoutRow["CV_Name_Top"]?.ToString();
				if (!string.IsNullOrEmpty(topCubeView))
				{
					var topComponent = dynComponents.GetComponentUsingBasedOnName("cv_Top_Panel");
					if (topComponent != null)
					{
						var xmlData = XElement.Parse(topComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
						xmlData.SetElementValue("CubeViewName", topCubeView);
						topComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
					}
				}
			}
			
			// Process bottom panel
			var bottomOptionType = Convert.ToInt32(layoutRow["Bottom_Option_Type"] ?? 0);
			if (bottomOptionType == 1) // Dashboard
			{
				string bottomDashboard = layoutRow["DB_Name_Bottom"]?.ToString();
				if (!string.IsNullOrEmpty(bottomDashboard))
				{
					var bottomComponent = dynComponents.GetComponentUsingBasedOnName("emb_Bottom_Panel");
					if (bottomComponent != null)
					{
						bottomComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = bottomDashboard;
					}
				}
			}
			else if (bottomOptionType == 2) // CubeView
			{
				string bottomCubeView = layoutRow["CV_Name_Bottom"]?.ToString();
				if (!string.IsNullOrEmpty(bottomCubeView))
				{
					var bottomComponent = dynComponents.GetComponentUsingBasedOnName("cv_Bottom_Panel");
					if (bottomComponent != null)
					{
						var xmlData = XElement.Parse(bottomComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
						xmlData.SetElementValue("CubeViewName", bottomCubeView);
						bottomComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
					}
				}
			}
		}
		
		// Process left/right layout
		private static void ProcessLeftRightLayout(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string dashboardName)
		{
			// Process left panel
			var leftOptionType = Convert.ToInt32(layoutRow["Left_Option_Type"] ?? 0);
			if (leftOptionType == 1) // Dashboard
			{
				string leftDashboard = layoutRow["DB_Name_Left"]?.ToString();
				if (!string.IsNullOrEmpty(leftDashboard))
				{
					var leftComponent = dynComponents.GetComponentUsingBasedOnName("emb_Left_Panel");
					if (leftComponent != null)
					{
						leftComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = leftDashboard;
					}
				}
			}
			else if (leftOptionType == 2) // CubeView
			{
				string leftCubeView = layoutRow["CV_Name_Left"]?.ToString();
				if (!string.IsNullOrEmpty(leftCubeView))
				{
					var leftComponent = dynComponents.GetComponentUsingBasedOnName("cv_Left_Panel");
					if (leftComponent != null)
					{
						var xmlData = XElement.Parse(leftComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
						xmlData.SetElementValue("CubeViewName", leftCubeView);
						leftComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
					}
				}
			}
			
			// Process right panel
			var rightOptionType = Convert.ToInt32(layoutRow["Right_Option_Type"] ?? 0);
			if (rightOptionType == 1) // Dashboard
			{
				string rightDashboard = layoutRow["DB_Name_Right"]?.ToString();
				if (!string.IsNullOrEmpty(rightDashboard))
				{
					var rightComponent = dynComponents.GetComponentUsingBasedOnName("emb_Right_Panel");
					if (rightComponent != null)
					{
						rightComponent.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = rightDashboard;
					}
				}
			}
			else if (rightOptionType == 2) // CubeView
			{
				string rightCubeView = layoutRow["CV_Name_Right"]?.ToString();
				if (!string.IsNullOrEmpty(rightCubeView))
				{
					var rightComponent = dynComponents.GetComponentUsingBasedOnName("cv_Right_Panel");
					if (rightComponent != null)
					{
						var xmlData = XElement.Parse(rightComponent.DynamicComponentEx.DynamicComponent.Component.XmlData);
						xmlData.SetElementValue("CubeViewName", rightCubeView);
						rightComponent.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
					}
				}
			}
		}
		
		// Process complex layouts (2top1bottom, 1top2bottom, etc.)
		private static void ProcessComplexLayout(WsDynamicComponentCollection dynComponents, DataRow layoutRow, int layoutType, string dashboardName)
		{
			// These layouts combine top/bottom or left/right with additional split panels
			// The actual panel names would need to match the dashboard component names in the XML
			
			switch (layoutType)
			{
				case 5: // 2Top1Bottom
				case 6: // 1Top2Bottom
					ProcessTopBottomLayout(dynComponents, layoutRow, dashboardName);
					break;
					
				case 7: // 2Left1Right
				case 8: // 1Left2Right
					ProcessLeftRightLayout(dynComponents, layoutRow, dashboardName);
					break;
			}
		}
		
		// Process quadrant layout (2x2)
		private static void ProcessQuadrantLayout(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string dashboardName)
		{
			// Process all four quadrants: TopLeft, TopRight, BottomLeft, BottomRight
			
			// Top Left
			var topLeftType = Convert.ToInt32(layoutRow["TopLeft_Option_Type"] ?? 0);
			ProcessQuadrantPanel(dynComponents, layoutRow, topLeftType, "TopLeft", "emb_TopLeft_Panel", "cv_TopLeft_Panel");
			
			// Top Right
			var topRightType = Convert.ToInt32(layoutRow["TopRight_Option_Type"] ?? 0);
			ProcessQuadrantPanel(dynComponents, layoutRow, topRightType, "TopRight", "emb_TopRight_Panel", "cv_TopRight_Panel");
			
			// Bottom Left
			var bottomLeftType = Convert.ToInt32(layoutRow["BottomLeft_Option_Type"] ?? 0);
			ProcessQuadrantPanel(dynComponents, layoutRow, bottomLeftType, "BottomLeft", "emb_BottomLeft_Panel", "cv_BottomLeft_Panel");
			
			// Bottom Right
			var bottomRightType = Convert.ToInt32(layoutRow["BottomRight_Option_Type"] ?? 0);
			ProcessQuadrantPanel(dynComponents, layoutRow, bottomRightType, "BottomRight", "emb_BottomRight_Panel", "cv_BottomRight_Panel");
		}
		
		// Helper method to process individual quadrant panel
		private static void ProcessQuadrantPanel(WsDynamicComponentCollection dynComponents, DataRow layoutRow, int panelType, string position, string dashboardComponentName, string cubeViewComponentName)
		{
			if (panelType == 1) // Dashboard
			{
				string dashboardName = layoutRow[$"DB_Name_{position}"]?.ToString();
				if (!string.IsNullOrEmpty(dashboardName))
				{
					var component = dynComponents.GetComponentUsingBasedOnName(dashboardComponentName);
					if (component != null)
					{
						component.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = dashboardName;
					}
				}
			}
			else if (panelType == 2) // CubeView
			{
				string cubeViewName = layoutRow[$"CV_Name_{position}"]?.ToString();
				if (!string.IsNullOrEmpty(cubeViewName))
				{
					var component = dynComponents.GetComponentUsingBasedOnName(cubeViewComponentName);
					if (component != null)
					{
						var xmlData = XElement.Parse(component.DynamicComponentEx.DynamicComponent.Component.XmlData);
						xmlData.SetElementValue("CubeViewName", cubeViewName);
						component.DynamicComponentEx.DynamicComponent.Component.XmlData = xmlData.ToString();
					}
				}
			}
		}
		
		// Process custom dashboard
		private static void ProcessCustomDashboard(WsDynamicComponentCollection dynComponents, DataRow layoutRow, string dashboardName)
		{
			// For custom dashboards, use the Custom_DB_Content field if available
			string customDashboard = layoutRow["Custom_DB_Content"]?.ToString();
			if (!string.IsNullOrEmpty(customDashboard))
			{
				var component = dynComponents.GetComponentUsingBasedOnName("emb_Custom_Panel");
				if (component != null)
				{
					component.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = customDashboard;
				}
			}
			else
			{
				// Fallback to using Custom_DB_Header if Custom_DB_Content is not set
				string customHeader = layoutRow["Custom_DB_Header"]?.ToString();
				if (!string.IsNullOrEmpty(customHeader))
				{
					var component = dynComponents.GetComponentUsingBasedOnName("emb_Custom_Panel");
					if (component != null)
					{
						component.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName = customHeader;
					}
				}
			}
		}
		
        // menu label
        internal static WsDynamicDashboardEx get_DynamicContent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();

            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }

    }
}