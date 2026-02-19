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
	public class DDM_ConfigHelpers
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
					DashboardName = "DDM_Config_AddUpdate_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
					}
				},
				[Layout_OptionType.CubeView] = new LayoutConfig
				{
					DashboardName = "DDM_Config_AddUpdate_CV",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_CV_Name", "CV_Name" } } }
					}
				},
				[Layout_OptionType.Dashboard_TopBottom] = new LayoutConfig
				{
					DashboardName = "DDM_Config_Content_AddUpdate_TB_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_T_Height", "T_Height" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_T_Content_Type", "T_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_T_Name", "T_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_B_ContentType", "B_ContentType" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_B_Name", "B_Name" } } }
					}
				},
				[Layout_OptionType.Dashboard_LeftRight] = new LayoutConfig
				{
					DashboardName = "DDM_Config_AddUpdate_LR_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_L_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_L_ContentType", "L_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_L_Name", "L_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_R_ContentType", "R_ContentType" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_R_Name", "R_Name" } } }
					}
				},
				[Layout_OptionType.Dashboard_2Top1Bottom] = new LayoutConfig
				{
					DashboardName = "DDM_Config_AddUpdate_2T1B_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_L_Width", "L_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_TL_ContentType", "TL_ContentType" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_TL_Name", "TL_Name" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
					}
				},
				[Layout_OptionType.Dashboard_1Top2Bottom] = new LayoutConfig
				{
					DashboardName = "DDM_Config_AddUpdate_1T2B_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Content_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
					}
				},
				[Layout_OptionType.Dashboard_2Left1Right] = new LayoutConfig
				{
					DashboardName = "DDM_Config_Content_AddUpdate_2L1R_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Content_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
					}
				},
				[Layout_OptionType.Dashboard_1Left2Right] = new LayoutConfig
				{
					DashboardName = "DDM_Config_Content_AddUpdate_1L2R_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Content_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
					}
				},
				[Layout_OptionType.Dashboard_2x2] = new LayoutConfig
				{
					DashboardName = "DDM_Config_Content_AddUpdate_2x2_DB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Content_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
						{ 5, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Right", "DB_Name_Right" } } },
						{ 6, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Right", "CV_Name_Right" } } }
					}
				},
				[Layout_OptionType.Dashboard_CustomDB] = new LayoutConfig
				{
					DashboardName = "DDM_Config_Content_AddUpdate_CustomDB",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_Left_Width", "Left_Width" } } },
						{ 1, new Dictionary<string, string> { { "DL_DDM_Layout_Left_Content_Type", "Left_Option_Type" } } },
						{ 2, new Dictionary<string, string> { { "DL_DDM_Layout_DB_Name_Left", "DB_Name_Left" } } },
						{ 3, new Dictionary<string, string> { { "DL_DDM_Layout_CV_Name_Left", "CV_Name_Left" } } },
						{ 4, new Dictionary<string, string> { { "DL_DDM_Layout_Right_Content_Type", "Right_Option_Type" } } },
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
					DashboardName = "DDM_Config_Header_AddUpdate_Filter",
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Hdr_Ctrl_Name", "Name" } } }
					}
				},
				[HdrCtrlType.Button] = new HdrCtrlConfig
				{
					DashboardName = "DDM_Config_Header_AddUpdate_Btn",
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

		public HdrCtrlConfig Get_Hdr_Ctrl_Option_Type_Config(int optionintValue)
		{
			var hdrctrlType = (HdrCtrlType)optionintValue;

			if (HdrCtrlRegistry.Configs.TryGetValue(hdrctrlType, out var config))
			{
				return config;
			}
			return null;
		}

		public enum DDMType
		{
			None = 0,
			WFProfile = 1,
			StandAlone = 2
		}

		public enum Layout_OptionType
		{
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
			Dashboard_CustomDB = 10
		}

		public enum HdrCtrlType
		{
			None = 0,
			Filter = 1,
			Button = 2
		}

		public enum DBPaneContents
		{
			None = 0,
			Dashboard = 1,
			CubeView = 2
		}

		public enum HdrCtrlFilterType
		{
			None = 0,
			Dim = 1,
			SQL_BoundList = 2
		}

		public enum HdrCtrlBtn_ActionPOVType
		{
			None = 0,
			NoAction = 1,
			ChangePOV = 2,
			ChangeWF = 3,
			ChangePOVandWF = 4
		}

		public enum HdrCtrlBtn_ActionSaveType
		{
			None = 0,
			NoAction = 1,
			SaveDataComps = 2,
			PromptSaveDataComps = 3,
			SaveDataAllComps = 4,
			PromptSaveDataAllComps = 5,
			SaveDataCompsSaveFiles = 6,
			PromptSaveDataCompsSaveFiles = 7,
			SaveDataAllCompsSaveFiles = 8,
			PromptSaveDataAllCompsSaveFiles = 9,
			SaveAllFiles = 10
		}

		public enum HdrCtrlBtn_ActionServerTaskType
		{
			None = 0,
			NoTask = 1,
			ExeDBExtBR_GenServer = 2,
			ExeDBExtBR_StgServer = 3,
			ExeDBExtBR_ConsServer = 4,
			ExeDBExtBR_DMServer = 5,
			ExeFinCustCalcBR = 6,
			ExeDMSeq = 7,
			Calc = 8,
			ForceCalc = 9,
			CalcWLogging = 10,
			ForceCalcWLogging = 11,
			Trans = 12,
			ForceTrans = 13,
			TransWLogging = 14,
			ForceTransWLogging = 15,
			Cons = 16,
			ForceCons = 17,
			ConsWLogging = 18,
			ForceConsWLogging = 19
		}

		public enum HdrCtrlBtn_ActionUIChangedType
		{
			None = 0,
			NoAction = 1,
			Redraw = 2,
			Refresh = 3,
			CloseDialog = 4,
			CloseDialogOK = 5,
			CloseDialogCancel = 6,
			CloseAllDialogs = 7,
			OpenDialog = 8,
			OpenDialogRedraw = 9,
			OpenDialogRefresh = 10,
			OpenDialogNoBtns = 11,
			OpenDialogNoBtnsApplyChgsRedraw = 12,
			OpenDialogNoBtnsApplyChgsRefresh = 13,
			OpenDialogNoBtnsRedraw = 14,
			OpenDialogNoBtnsRefresh = 15,
			OpenDialogApplyChgsRedraw = 16,
			OpenDialogApplyChgsRefresh = 17,
			OpenDialogApplyChgsRedrawOk = 18,
			OpenDialogApplyChgsRefreshOk = 19
		}

		public enum HdrCtrlBtn_ActionNavType
		{
			None = 0,
			NoAction = 1,
			OpenFile = 2,
			OpenPage = 3,
			OpenWebSite = 4
		}
	}
}
