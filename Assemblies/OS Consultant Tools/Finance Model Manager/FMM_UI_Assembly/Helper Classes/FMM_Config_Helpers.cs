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
	public class FMM_Config_Helpers
	{
		#region "Config Setup"
		public class CubeCalcConfig
		{
			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CubeCalcRegistry
		{
			public static readonly Dictionary<CubeCalcConfigType, CubeCalcConfig> Configs = new()
			{
				[CubeCalcConfigType.CalcConfig] = new CubeCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDim_Alloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_MbrListCalc", "MbrList_Calc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList1Calc", "MbrList_1_Calc" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList1DimType", "MbrList_1_DimType" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList1Dim", "MbrList_1_Dim" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList1Filter", "MbrList_1_Filter" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList2Calc", "MbrList_2_Calc" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList2DimType", "MbrList_2_DimType" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList2Dim", "MbrList_2_Dim" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList2Filter", "MbrList_2_Filter" } } },
						{ 14, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList3Calc", "MbrList_3_Calc" } } },
						{ 15, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList3DimType", "MbrList_3_DimType" } } },
						{ 16, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList3Dim", "MbrList_3_Dim" } } },
						{ 17, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList3Filter", "MbrList_3_Filter" } } },
						{ 18, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList4Calc", "MbrList_4_Calc" } } },
						{ 19, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList4DimType", "MbrList_4_DimType" } } },
						{ 20, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList4Dim", "MbrList_4_Dim" } } },
						{ 21, new Dictionary<string, string> { { "IV_FMM_Calc_MbrList4Filter", "MbrList_4_Filter" } } }
					}
				},
				[CubeCalcConfigType.DestConfig] = new CubeCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Cons", "Cons" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_View", "View" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Acct", "Acct" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Flow", "Flow" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_IC", "IC" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Origin", "Origin" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD1", "UD1" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD2", "UD2" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD3", "UD3" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD4", "UD4" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD5", "UD5" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD6", "UD6" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD7", "UD7" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD8", "UD8" } } },
						{ 14, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Time_Filter", "Time_Filter" } } },
						{ 15, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Acct_Filter", "Acct_Filter" } } },
						{ 16, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Flow_Filter", "Flow_Filter" } } },
						{ 17, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_IC_Filter", "IC_Filter" } } },
						{ 18, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_Origin_Filter", "Origin_Filter" } } },
						{ 19, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD1_Filter", "UD1_Filter" } } },
						{ 20, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD2_Filter", "UD2_Filter" } } },
						{ 21, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD3_Filter", "UD3_Filter" } } },
						{ 22, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD4_Filter", "UD4_Filter" } } },
						{ 23, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD5_Filter", "UD5_Filter" } } },
						{ 24, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD6_Filter", "UD6_Filter" } } },
						{ 25, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD7_Filter", "UD7_Filter" } } },
						{ 26, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD8_Filter", "UD8_Filter" } } }
					}
				},
				[CubeCalcConfigType.SrcConfig] = new CubeCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "DL_FMM_Calc_Src_CubeType", "DB_Name" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Src_OpenParen", "DB_Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Src_MathOperator", "DB_Name" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Entity", "DB_Name" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Cons", "DB_Name" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Scenario", "DB_Name" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Time", "DB_Name" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Origin", "DB_Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Src_IC", "DB_Name" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Src_View", "DB_Name" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_Src_Flow", "DB_Name" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD1", "DB_Name" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD2", "DB_Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD3", "DB_Name" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD4", "DB_Name" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD5", "DB_Name" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD6", "DB_Name" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD7", "DB_Name" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_Src_UD8", "DB_Name" } } }
					}
				}
			};
		}

		public class RegCalcConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class RegCalcRegistry
		{
			public static readonly Dictionary<RegCalcConfigType, RegCalcConfig> Configs = new()
			{
				[RegCalcConfigType.CalcConfig] = new RegCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
					}
				},
				[RegCalcConfigType.DestConfig] = new RegCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
					}
				},
				[RegCalcConfigType.SrcConfig] = new RegCalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
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

		public CubeCalcConfig Get_CubeCalcConfigType(int optionintValue)
		{
			var optionType = (CubeCalcConfigType)optionintValue;

			if (CubeCalcRegistry.Configs.TryGetValue(optionType, out var config))
			{
				return config;
			}
			return null;
		}

		public RegCalcConfig Get_RegCalcConfigType(int optionintValue)
		{
			var optionType = (RegCalcConfigType)optionintValue;

			if (RegCalcRegistry.Configs.TryGetValue(optionType, out var config))
			{
				return config;
			}
			return null;
		}

		public enum CubeCalcConfigType
		{
			None = 0,
			CalcConfig = 1,
			DestConfig = 2,
			SrcConfig = 3
		}

		public enum RegCalcConfigType
		{
			None = 0,
			CalcConfig = 1,
			DestConfig = 2,
			SrcConfig = 3
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