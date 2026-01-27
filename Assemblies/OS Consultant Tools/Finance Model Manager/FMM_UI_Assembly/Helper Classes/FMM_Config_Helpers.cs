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
		public class CalcConfig
		{
			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CalcRegistry
		{
			public static readonly Dictionary<CalcType, CalcConfig> Configs = new()
			{
				[CalcType.Table] = new CalcConfig
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
				[CalcType.Cube] = new CalcConfig
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
				[CalcType.BRTabletoCube] = new CalcConfig
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
				},
				[CalcType.ImportTabletoCube] = new CalcConfig
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
				},
				[CalcType.CubetoTable] = new CalcConfig
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
				},
				[CalcType.Consolidate] = new CalcConfig
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

		public class DestConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class DestRegistry
		{
			public static readonly Dictionary<CalcType, DestConfig> Configs = new()
			{
				[CalcType.Table] = new DestConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDim_Alloc" } } }
					}
				},
				[CalcType.Cube] = new DestConfig
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
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD8", "UD8" } } }
					}
				},
				[CalcType.BRTabletoCube] = new DestConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_DDM_Layout_DB_Name", "DB_Name" } } }
					}
				}
			};
		}

		public class SrcConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class SrcRegistry
		{
			public static readonly Dictionary<CalcType, SrcConfig> Configs = new()
			{
				[CalcType.Table] = new SrcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDim_Alloc" } } }
					}
				},
				[CalcType.Cube] = new SrcConfig
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
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_Dest_UD8", "UD8" } } }
					}
				},
				[CalcType.BRTabletoCube] = new SrcConfig
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

		public static CalcConfig Get_CalcConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (CalcRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public static DestConfig Get_DestConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (DestRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public static SrcConfig Get_SrcConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (SrcRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		/// <summary>
		/// Gets the list of database column names (property names) that are enabled for a given CalcType in SrcRegistry
		/// </summary>
		public static List<string> GetEnabledSrcProperties(int calctypeintValue)
		{
			var properties = new List<string>();
			var srcConfig = Get_SrcConfigType(calctypeintValue);

			if (srcConfig != null && srcConfig.ParameterMappings != null)
			{
				foreach (var mapping in srcConfig.ParameterMappings.Values)
				{
					foreach (var propertyName in mapping.Values)
					{
						if (!properties.Contains(propertyName) && !string.IsNullOrEmpty(propertyName))
						{
							properties.Add(propertyName);
						}
					}
				}
			}

			return properties;
		}

		/// <summary>
		/// Gets the list of database column names (property names) that are enabled for a given CalcType in DestRegistry
		/// </summary>
		public static List<string> GetEnabledDestProperties(int calctypeintValue)
		{
			var properties = new List<string>();
			var destConfig = Get_DestConfigType(calctypeintValue);

			if (destConfig != null && destConfig.ParameterMappings != null)
			{
				foreach (var mapping in destConfig.ParameterMappings.Values)
				{
					foreach (var propertyName in mapping.Values)
					{
						if (!properties.Contains(propertyName) && !string.IsNullOrEmpty(propertyName))
						{
							properties.Add(propertyName);
						}
					}
				}
			}

			return properties;
		}

		/// <summary>
		/// Gets the formatted column configuration for a given model type and column index
		/// </summary>
		public static string Get_Model_Col_Format(string curr_TED, string curr_ModelType, string col)
		{
			if (curr_TED.Equals("Calc_Config", StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(col, out int colIndex) && colIndex > 0)
				{
					var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

					if (ModelColumnFormatter.CalcConfigColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
					{
						return columns[colIndex - 1].ToString();
					}
				}
			}
			else if (curr_TED.Equals("Dest", StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(col, out int colIndex) && colIndex > 0)
				{
					var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

					if (ModelColumnFormatter.DestCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
					{
						return columns[colIndex - 1].ToString();
					}
				}
			}
			else if (curr_TED.Equals("Src", StringComparison.OrdinalIgnoreCase))
			{
				if (int.TryParse(col, out int colIndex) && colIndex > 0)
				{
					var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

					if (ModelColumnFormatter.SrcCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
					{
						return columns[colIndex - 1].ToString();
					}
				}
			}
			return null;
		}

		public List<FMM_Src_CellModel> BuildSrcCellModels(
			IDictionary<string, string> customSubstVarsWithUserSelectedValues,
			string componentId,
			string dynamicSuffix,
			Func<string, string, string> getDynamicParamValue,
			Func<string, Guid> getDynamicParamGuid)
		{
			try
			{
				var cellModels = new List<FMM_Src_CellModel>();

				if (customSubstVarsWithUserSelectedValues == null)
				{
					return cellModels;
				}

				var calcTypeInt = 0;
				if (customSubstVarsWithUserSelectedValues.TryGetValue("IV_FMM_Calc_Type", out var calcTypeValue))
				{
					int.TryParse(calcTypeValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out calcTypeInt);
				}

				var srcConfig = Get_SrcConfigType(calcTypeInt);
				if (srcConfig?.ParameterMappings == null)
				{
					return cellModels;
				}

				foreach (var mapping in srcConfig.ParameterMappings.Values)
				{
					var paramName = mapping.Keys.FirstOrDefault();
					if (string.IsNullOrWhiteSpace(paramName))
					{
						continue;
					}

					customSubstVarsWithUserSelectedValues.TryGetValue(paramName, out var defaultValue);

					var dynamicParamName = $"{paramName}{dynamicSuffix}";
					var model = new FMM_Src_CellModel();
					SetPropertyIfExists(model, new[] { "DynamicParamGuid", "DynamicParameterGuid", "ParamGuid" }, getDynamicParamGuid(dynamicParamName));
					SetPropertyIfExists(model, new[] { "DynamicParamName", "DynamicParameterName", "ParamName" }, dynamicParamName);
					SetPropertyIfExists(model, new[] { "Value", "ParamValue", "DynamicParameterValue" }, getDynamicParamValue(paramName, defaultValue ?? string.Empty));
					cellModels.Add(model);
				}

				return cellModels;
			}
			catch (Exception ex)
			{
				throw new XFException(ex.Message, ex);
			}
		}

		private static void SetPropertyIfExists(object target, IEnumerable<string> candidateNames, object value)
		{
			if (target == null)
			{
				return;
			}

			foreach (var candidate in candidateNames)
			{
				var prop = target.GetType().GetProperty(candidate);
				if (prop != null && prop.CanWrite)
				{
					prop.SetValue(target, value);
					break;
				}
			}
		}

		public enum CalcType
		{
			None = 0,
			Table = 1,
			Cube = 2,
			BRTabletoCube = 3,
			ImportTabletoCube = 4,
			CubetoTable = 5,
			Consolidate = 6
		}

		public class ColumnConfig
		{
			public string ColumnName { get; set; }
			public string Description { get; set; }
			public bool IsVisible { get; set; }
			public string Width { get; set; } = "Auto";
			public bool AllowUpdates { get; set; } = true;
			public string DefaultValue { get; set; }
			public string ParameterName { get; set; }

			public override string ToString()
			{
				var parts = new List<string>();
				if (!AllowUpdates) parts.Add("AllowUpdates = False");
				if (!string.IsNullOrEmpty(ColumnName)) parts.Add($"ColumnName = {ColumnName}");
				if (!string.IsNullOrEmpty(DefaultValue)) parts.Add($"DefaultValue = {DefaultValue}");
				if (!string.IsNullOrEmpty(Description)) parts.Add($"Description = {Description}");
				if (!string.IsNullOrEmpty(Width)) parts.Add($"Width = {Width}");
				if (!string.IsNullOrEmpty(ParameterName)) parts.Add($"ParameterName = {ParameterName}");
				if (IsVisible) parts.Add("IsVisible = True");

				return string.Join(", ", parts);
			}
		}

		public class ModelColumnFormatter
		{
			public static readonly Dictionary<string, ColumnConfig[]> CalcConfigColumns = new Dictionary<string, ColumnConfig[]>
			{
				{ "Table", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
						new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
						new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
						new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
						new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
						new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
						new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
						new ColumnConfig { ColumnName = "MultiDim_Alloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
						new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc]", IsVisible = true },
						new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
						new ColumnConfig { ColumnName = "Time_Phasing", Description = "[Time Phasing]", IsVisible = true, ParameterName = "DL_FMM_Calc_Time_Phasing" },
						new ColumnConfig { ColumnName = "Input_Frequency", Description = "[Input Freq]", IsVisible = true },
						new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
						new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
					}
				},
				{ "Cube", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
						new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
						new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
						new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
						new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
						new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
						new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
						new ColumnConfig { ColumnName = "MultiDim_Alloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
						new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
						new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
						new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
						new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
						new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
						new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
						new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
					}
				}
			};

			public static readonly Dictionary<string, ColumnConfig[]> DestCellColumns = new Dictionary<string, ColumnConfig[]>
			{
				{ "Table", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Dest_Cell_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Location", Description = "Target Location", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Calc_Plan_Units", Description = "[Plan Units]", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Time_Filter", Description = "Time Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Conditional_Filter", Description = "Conditional Filter", Width = "Auto", IsVisible = true },
						// Add more placeholders if needed
					}
				},
				{ "Cube", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Dest_Cell_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Time_Filter", Description = "Time Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Acct_Filter", Description = "Account Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Origin_Filter", Description = "Origin Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Flow_Filter", Description = "Flow Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD1_Filter", Description = "UD1 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD2_Filter", Description = "UD2 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD3_Filter", Description = "UD3 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD4_Filter", Description = "UD4 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD5_Filter", Description = "UD5 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD6_Filter", Description = "UD6 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD7_Filter", Description = "UD7 Filter", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD8_Filter", Description = "UD8 Filter", Width = "Auto", IsVisible = true },
						// Add more placeholders if needed
					}
				}
			};
			public static readonly Dictionary<string, ColumnConfig[]> SrcCellColumns = new Dictionary<string, ColumnConfig[]>
			{
				{ "Table", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Src_Cell_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
						new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
						new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
						new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Calc_ID!|" },
						new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
						new ColumnConfig { ColumnName = "Src_Type", Description = "[Source/Calc Type]", ParameterName = "DL_FMM_Table_Calc_Src", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Src_Item", Description = "[Source/Calc Item]", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Table_Calc_Expression", Description = "[Calc Expression]", Width = "200", IsVisible = true },
						new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Table_Join_Expression", Description = "[Join Expression]", Width = "200", IsVisible = true },
						new ColumnConfig { ColumnName = "Table_Filter_Expression", Description = "[Filter Expression]", Width = "200", IsVisible = true },
						new ColumnConfig { ColumnName = "Map_Type", Description = "[Map Type]", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Map_Source", Description = "[Map Source]", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Map_Logic", Description = "[Map Logic]", Width = "Auto", IsVisible = true },
						// Add more placeholders if needed
					}
				},
				{ "Cube", new ColumnConfig[]
					{
						new ColumnConfig { ColumnName = "Cell_ID", IsVisible = false },
						new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
						new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
						new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
						new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Calc_ID!|" },
						new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
						new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Entity", Description = "Entity", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Cons", Description = "Cons", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Scenario", Description = "Scenario", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Time", Description = "Time", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
						new ColumnConfig { ColumnName = "Src_Type", Description = "[Source Type]", ParameterName = "DL_FMM_Cube_Calc_Source", DefaultValue = "[Stored Cell]", Width = "Auto", IsVisible = true },
						// Add more placeholders if needed
					}
				}
			};
		}
	}
}