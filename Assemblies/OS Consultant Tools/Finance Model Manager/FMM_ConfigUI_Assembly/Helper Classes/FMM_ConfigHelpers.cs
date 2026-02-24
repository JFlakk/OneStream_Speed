using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
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
	public class FMM_ConfigHelpers
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
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.Cube] = new CalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.BRTabletoCube] = new CalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.CubetoTable] = new CalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateUser", "UpdateUser" } } }
					}
				},
				[CalcType.Consolidate] = new CalcConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Sequence", "Sequence" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Calc_Name", "Name" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Calc_Explanation", "Explanation" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Calc_Condition", "Condition" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Calc_MultiDimAlloc", "MultiDimAlloc" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalc", "BRCalc" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Calc_BRCalcName", "BRCalcName" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Calc_TimePhase", "TimePhase" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Calc_InputFreq", "InputFreq" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Calc_Status", "Status" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Calc_CreateDate", "CreateDate" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Calc_CreateUser", "CreateUser" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateDate", "UpdateDate" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Calc_UpdateUser", "UpdateUser" } } }
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
						{ 0, new Dictionary<string, string> { { "IV_FMM_Calc_Location", "Sequence" } } },
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
						{ 0, new Dictionary<string, string> { { "IV_FMM_Dest_Cons", "Cons" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_Dest_View", "View" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_Dest_Acct", "Acct" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_Dest_Flow", "Flow" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_Dest_IC", "IC" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_Dest_Origin", "Origin" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_Dest_UD1", "UD1" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_Dest_UD2", "UD2" } } },
						{ 8, new Dictionary<string, string> { { "IV_FMM_Dest_UD3", "UD3" } } },
						{ 9, new Dictionary<string, string> { { "IV_FMM_Dest_UD4", "UD4" } } },
						{ 10, new Dictionary<string, string> { { "IV_FMM_Dest_UD5", "UD5" } } },
						{ 11, new Dictionary<string, string> { { "IV_FMM_Dest_UD6", "UD6" } } },
						{ 12, new Dictionary<string, string> { { "IV_FMM_Dest_UD7", "UD7" } } },
						{ 13, new Dictionary<string, string> { { "IV_FMM_Dest_UD8", "UD8" } } }
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

					}
				},
				[CalcType.Cube] = new SrcConfig
				{
					ParameterMappings = new()
					{

					}
				},
				[CalcType.BRTabletoCube] = new SrcConfig
				{
					ParameterMappings = new()
					{

					}
				}
			};
		}

		public class CubeConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class CubeConfigRegistry
		{
			public static readonly Dictionary<SaveType, CubeConfig> Configs = new()
			{
				[SaveType.Add] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_CubeConfig_Cubes", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				},
				[SaveType.View] = new CubeConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Name", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_EntityMFB", "EntityMFB" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 4, new Dictionary<string, string> { { "IV_FMM_CreateDate", "CreateDate" } } },
						{ 5, new Dictionary<string, string> { { "IV_FMM_CreateUser", "CreateUser" } } },
						{ 6, new Dictionary<string, string> { { "IV_FMM_UpdateDate", "UpdateDate" } } },
						{ 7, new Dictionary<string, string> { { "IV_FMM_UpdateUser", "UpdateUser" } } }
					}
				}
			};
		}
		public class AcctConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class AcctConfigRegistry
		{
			public static readonly Dictionary<SaveType, AcctConfig> Configs = new()
			{
				[SaveType.Add] = new AcctConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new AcctConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}

		public class RegConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class RegConfigRegistry
		{
			public static readonly Dictionary<SaveType, RegConfig> Configs = new()
			{
				[SaveType.Add] = new RegConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new RegConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}
		public class ApprConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class ApprConfigRegistry
		{
			public static readonly Dictionary<SaveType, ApprConfig> Configs = new()
			{
				[SaveType.Add] = new ApprConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new ApprConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
					}
				}
			};
		}

		public class ApprStepConfig
		{

			public Dictionary<int, Dictionary<string, string>> ParameterMappings { get; init; }
		}

		public static class ApprStepConfigRegistry
		{
			public static readonly Dictionary<SaveType, ApprStepConfig> Configs = new()
			{
				[SaveType.Add] = new ApprStepConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "BL_FMM_All_Cube_Names", "Cube" } } },
						{ 1, new Dictionary<string, string> { { "BL_FMM_CubeConfig_ScenType", "ScenType" } } },
						{ 2, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 3, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 4, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } }
					}
				},
				[SaveType.Update] = new ApprStepConfig
				{
					ParameterMappings = new()
					{
						{ 0, new Dictionary<string, string> { { "IV_FMM_CubeConfig_Descr", "Descr" } } },
						{ 1, new Dictionary<string, string> { { "IV_FMM_CubeConfig_EntityMFB", "EntityMFB" } } },
						{ 2, new Dictionary<string, string> { { "DL_FMM_CubeConfig_AggConsol", "AggConsol" } } },
						{ 3, new Dictionary<string, string> { { "DL_FMM_Status", "Status" } } }
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

		public static CubeConfig Get_CubeSaveType(int savetypeintValue)
		{
			var saveType = (SaveType)savetypeintValue;

			if (CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
			{
				return config;
			}
			return null;
		}

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

		public static List<FMM_SrcCellModel> BuildSrcCellModels(
			IDictionary<string, string> customSubstVarsWithUserSelectedValues,
			string componentId,
			string dynamicSuffix,
			Func<string, string, string> getDynamicParamValue,
			Func<string, Guid> getDynamicParamGuid)
		{
			try
			{
				var cellModels = new List<FMM_SrcCellModel>();

				if (customSubstVarsWithUserSelectedValues == null)
				{
					return cellModels;
				}

				var calcTypeInt = 0;
				if (customSubstVarsWithUserSelectedValues.TryGetValue("IV_FMM_CalcType", out var calcTypeValue))
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
					var model = new FMM_SrcCellModel();
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
		public static void SetCubeConfigParams(SessionInfo si, Dictionary<string, string> substVars)
		{
			var cubeIDStr = substVars.ContainsKey("IV_FMM_CubeID") ? substVars["IV_FMM_CubeID"] : "0";
			var cubeID = 0;
			Int32.TryParse(cubeIDStr, out cubeID);

			var cubeConfig_DT = new DataTable("FMM_CubeConfig");

			try
			{
				using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
				{
					using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
					{
						var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
						var sqa = new SqlDataAdapter();
						var sql = "SELECT * FROM FMM_CubeConfig WHERE CubeID = @CubeID";
						var sqlparams = new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = cubeID } };

						sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, cubeConfig_DT, sql, sqlparams);
					}
				}
			}
			catch (Exception ex) { }

			Action<string, string> setVar = (key, val) =>
			{
				if (substVars.ContainsKey(key)) { substVars[key] = val; }
				else { substVars.Add(key, val); }
			};

			if (cubeConfig_DT.Rows.Count > 0)
			{
				DataRow row = cubeConfig_DT.Rows[0];
				if (CubeConfigRegistry.Configs.TryGetValue(SaveType.View, out var config))
				{
					foreach (var step in config.ParameterMappings)
					{
						foreach (var map in step.Value)
						{
							setVar(map.Key, row[map.Value].ToString());
						}
					}
				}
			}
			else
			{
				string[] keys = { "IV_FMM_Cube", "IV_FMM_ScenType", "IV_FMM_EntityMFB", "IV_FMM_CreateDate", "IV_FMM_CreateUser", "IV_FMM_UpdateDate", "IV_FMM_UpdateUser" };
				foreach (var key in keys) { setVar(key, string.Empty); }
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
			CubetoTable = 4,
			Consolidate = 5
		}

		public enum SaveType
		{
			None = 0,
			Add = 1,
			Update = 2,
			View = 3
		}
	}
}