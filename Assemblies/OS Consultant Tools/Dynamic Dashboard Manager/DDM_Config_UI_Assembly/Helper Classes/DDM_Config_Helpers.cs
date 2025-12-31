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
	public class DDM_Config_Helpers
	{
		#region "Config Setup"
		public class LayoutConfig 
		{
		    public string DashboardName { get; init; }
		
		    public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class LayoutRegistry
		{
		    public static readonly Dictionary<Layout_OptionType, LayoutConfig> Configs = new()
		    {
		        [Layout_OptionType.Dashboard] = new LayoutConfig 
		        {
		            DashboardName = "0b1b2b2a2a_DDM_Config_Content_NewUpdates_DB",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
		            }
		        },
		        [Layout_OptionType.CubeView] = new LayoutConfig 
		        {
		            DashboardName = "0b1b2b2a2b_DDM_Config_Content_NewUpdates_CV",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Layout_CV_Name", "CV_Name" } } }
		            }
		        },
		        [Layout_OptionType.Dashboard_TopBottom] = new LayoutConfig 
		        {
		            DashboardName = "0b1b2b2a2c_DDM_Config_Content_Updates_DB_TopBottom",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Layout_Top_Height", "Top_Height" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Top_Option_Type", "Top_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Top", "DB_Name_Top" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Top", "CV_Name_Top" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Bottom_Option_Type", "Bottom_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Bottom", "DB_Name_Bottom" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Bottom", "CV_Name_Bottom" } } }
		            }
		        },
		        [Layout_OptionType.Dashboard_LeftRight] = new LayoutConfig 
		        {
		            DashboardName = "0b1b2b2a2d_DDM_Config_Content_Updates_DB_LeftRight",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Option_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Option_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		        [Layout_OptionType.Dashboard_2Top1Bottom] = new LayoutConfig 
		        {
		            DashboardName = "0b1b2b2a2d_DDM_Config_Content_Updates_DB_LeftRight",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Option_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Option_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
		            }
		        },
		    };
		}
		
		public class HdrCtrlConfig 
		{
		    public string DashboardName { get; init; }
		
		    public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class HdrCtrlRegistry
		{
		    public static readonly Dictionary<HdrCtrlType, HdrCtrlConfig> Configs = new()
		    {
		        [HdrCtrlType.Filter] = new HdrCtrlConfig
		        {
		            DashboardName = "0b1b2c2a2a_DDM_Config_Content_NewUpdates_Filter",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Hdr_Ctrl_Name", "Name" } } }
		            }
		        },
		        [HdrCtrlType.Button] = new HdrCtrlConfig 
		        {
		            DashboardName = "0b1b2b2a2b_DDM_Config_Content_NewUpdates_CV",
		            ParameterMappings = new() 
		            { 
		                { 0, new Dictionary<string, string> { { "IV_DDM_Hdr_Ctrl_Name", "Name" } } }
		            }
		        }
		    };
		}
		#endregion
		
        public object Test(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
		
		public LayoutConfig Get_Layout_Option_Type_Config(int optionintValue)
		{
			var optionType = (Layout_OptionType)optionintValue;

            if (LayoutRegistry.Configs.TryGetValue(optionType, out var config))
            {
				return config;
			}
			return null;
		}
		
		public enum DDMType {
			None = 0,
		    WFProfile = 1,
		    StandAlone = 2
		}
		
		public enum Layout_OptionType {
			None = 0,
		    Dashboard = 1,
		    CubeView = 2,
		    Dashboard_TopBottom = 3,
		    Dashboard_LeftRight = 4,
			Dashboard_2Top1Bottom = 5,
			Dashboard_1Top2Bottom = 6,
		    Dashboard_2Left1Right = 7,
		    Dashboard_1Left2Right = 8,
		    Dashboard_2x2 = 9,
			Custom_Dashboard = 10
		}
		
		public enum HdrCtrlType {
			None = 0,
		    Filter = 1,
		    Button = 2
		}
		
		public enum DBPaneContents {
			None = 0,
		    Dashboard = 1,
		    CubeView = 2
		}
	}
}