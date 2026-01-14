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

		public CalcConfig Get_CalcConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (CalcRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public DestConfig Get_DestConfigType(int calctypeintValue)
		{
			var calcType = (CalcType)calctypeintValue;

			if (DestRegistry.Configs.TryGetValue(calcType, out var config))
			{
				return config;
			}
			return null;
		}

		public SrcConfig Get_SrcConfigType(int calctypeintValue)
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
		public List<string> GetEnabledSrcProperties(int calctypeintValue)
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
		public List<string> GetEnabledDestProperties(int calctypeintValue)
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
	}
}