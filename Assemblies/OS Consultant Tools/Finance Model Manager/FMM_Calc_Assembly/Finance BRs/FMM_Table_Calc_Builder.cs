using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Helper class to build table calculation configurations easily
    /// Designed for non-technical users to configure table calculations via UI or simple parameters
    /// </summary>
    public class FMM_Table_Calc_Builder
    {
        /// <summary>
        /// Creates a standard table load configuration for Requirements tables
        /// Common pattern: XFC_{PREFIX}_REQ_Details tables
        /// </summary>
        public static FMM_Table_Calc_Config BuildRequirementsTableConfig(
            string prefix,
            string timeCalculation = "Annual", 
            List<string> accounts = null,
            List<string> statusFilters = null,
            List<string> appnFilters = null)
        {
            var config = new FMM_Table_Calc_Config
            {
                ConfigName = $"{prefix}_RequirementsLoad",
                SourceTable = $"XFC_{prefix}_REQ_Details",
                TimeCalculation = timeCalculation,
                HandleParentEntities = true,
                ClearStaleData = true
            };

            // Set standard dimension mapping
            config.SetStandardDimensionMapping();

            // Set standard grouping
            config.GroupByColumns = new List<string> 
            { 
                "Entity", "Account", "IC", "Flow", "UD1", "UD2", "UD3", "UD4", 
                "UD5", "UD6", "UD7", "UD8" 
            };

            // Build filters
            var filters = new List<string>();
            
            if (accounts != null && accounts.Count > 0)
            {
                string accountList = string.Join("','", accounts);
                filters.Add($"Account IN ('{accountList}')");
            }

            if (statusFilters != null && statusFilters.Count > 0)
            {
                string statusList = string.Join("','", statusFilters);
                filters.Add($"Flow IN ('{statusList}')");
            }

            if (appnFilters != null && appnFilters.Count > 0)
            {
                string appnConditions = string.Join(" OR ", 
                    appnFilters.Select(a => $"UD1 LIKE '{a}%'"));
                filters.Add($"({appnConditions})");
            }

            config.FilterConditions = filters;

            return config;
        }

        /// <summary>
        /// Creates aggregation configuration for consolidating entity hierarchies
        /// </summary>
        public static FMM_Table_Aggregation_Config BuildStandardAggregationConfig(
            string configName,
            List<string> accounts = null,
            Dictionary<string, List<string>> levelFlowFilters = null)
        {
            var config = new FMM_Table_Aggregation_Config
            {
                ConfigName = configName
            };

            config.SetDefaultAggregationFilters();

            if (accounts != null && accounts.Count > 0)
            {
                config.AccountFilter = accounts;
            }
            else
            {
                // Default account filter
                config.AccountFilter = new List<string> { "Req_Funding" };
            }

            if (levelFlowFilters != null && levelFlowFilters.Count > 0)
            {
                config.EntityLevelFlowFilters = levelFlowFilters;
            }
            else
            {
                // Set default level-based flow filters
                config.EntityLevelFlowFilters = new Dictionary<string, List<string>>
                {
                    {
                        "L2", 
                        new List<string> 
                        { 
                            "L2_Formulate_PGM", "L3_Formulate_PGM", "L4_Formulate_PGM", "L5_Formulate_PGM",
                            "L2_Validate_PGM", "L3_Validate_PGM", "L4_Validate_PGM", "L5_Validate_PGM",
                            "L2_Approve_PGM", "L3_Approve_PGM", "L2_Final_PGM"
                        }
                    },
                    {
                        "L3",
                        new List<string>
                        {
                            "L3_Formulate_PGM", "L4_Formulate_PGM", "L5_Formulate_PGM",
                            "L3_Validate_PGM", "L4_Validate_PGM", "L5_Validate_PGM",
                            "L3_Approve_PGM", "L2_Validate_PGM", "L2_Approve_PGM", "L2_Final_PGM"
                        }
                    },
                    {
                        "L4",
                        new List<string>
                        {
                            "L4_Formulate_PGM", "L5_Formulate_PGM",
                            "L3_Validate_PGM", "L4_Validate_PGM", "L5_Validate_PGM",
                            "L3_Approve_PGM", "L2_Validate_PGM", "L2_Approve_PGM", "L2_Final_PGM"
                        }
                    }
                };
            }

            return config;
        }

        /// <summary>
        /// Creates a periodic (monthly/quarterly) table load configuration
        /// Common for SPLN (spend plan) scenarios
        /// </summary>
        public static FMM_Table_Calc_Config BuildPeriodicTableConfig(
            string prefix,
            List<string> accounts = null,
            List<string> statusFilters = null,
            List<string> dimensionFilters = null,
            string filterDimension = "UD3")
        {
            var config = new FMM_Table_Calc_Config
            {
                ConfigName = $"{prefix}_PeriodicLoad",
                SourceTable = $"XFC_{prefix}_REQ_Details",
                TimeCalculation = "Period",
                HandleParentEntities = true,
                ClearStaleData = true
            };

            config.SetStandardDimensionMapping();

            config.GroupByColumns = new List<string>
            {
                "Entity", "Account", "IC", "Flow", "UD1", "UD2", "UD3", "UD4",
                "UD5", "UD6", "UD7", "UD8"
            };

            var filters = new List<string>();

            if (accounts != null && accounts.Count > 0)
            {
                string accountList = string.Join("','", accounts);
                filters.Add($"Account IN ('{accountList}')");
            }

            if (statusFilters != null && statusFilters.Count > 0)
            {
                string statusList = string.Join("','", statusFilters);
                filters.Add($"Flow IN ('{statusList}')");
            }

            if (dimensionFilters != null && dimensionFilters.Count > 0)
            {
                string dimConditions = string.Join(" OR ",
                    dimensionFilters.Select(d => $"{filterDimension} LIKE '{d}%'"));
                filters.Add($"({dimConditions})");
            }

            config.FilterConditions = filters;

            return config;
        }

        /// <summary>
        /// Parses status and APPN filters from global variables
        /// Common pattern in CMD implementations
        /// </summary>
        public static void ParseGlobalFilters(
            BRGlobals globals,
            FinanceRulesApi api,
            out List<string> statusFilters,
            out List<string> appnFilters)
        {
            statusFilters = new List<string>();
            appnFilters = new List<string>();

            string statusByFC = globals.GetStringValue(
                $"FundsCenterStatusUpdates - {api.Pov.Entity.Name}", 
                string.Empty
            );
            
            string appnByFC = globals.GetStringValue(
                $"FundsCenterStatusappnUpdates - {api.Pov.Entity.Name}", 
                string.Empty
            );

            // Parse status filters
            if (!string.IsNullOrEmpty(statusByFC))
            {
                statusFilters = statusByFC.Contains("|") 
                    ? StringHelper.SplitString(statusByFC, "|").ToList()
                    : new List<string> { statusByFC };
            }

            // Parse APPN filters
            if (!string.IsNullOrEmpty(appnByFC))
            {
                appnFilters = appnByFC.Contains("|")
                    ? StringHelper.SplitString(appnByFC, "|").ToList()
                    : new List<string> { appnByFC };
            }
        }

        /// <summary>
        /// Creates a simple custom SQL-based configuration
        /// For advanced scenarios where standard patterns don't fit
        /// </summary>
        public static FMM_Table_Calc_Config BuildCustomSQLConfig(
            string configName,
            string sourceTable,
            string customWhereClause,
            string timeCalculation = "Annual")
        {
            var config = new FMM_Table_Calc_Config
            {
                ConfigName = configName,
                SourceTable = sourceTable,
                TimeCalculation = timeCalculation,
                HandleParentEntities = true,
                ClearStaleData = true
            };

            config.SetStandardDimensionMapping();

            config.GroupByColumns = new List<string>
            {
                "Entity", "Account", "IC", "Flow", "UD1", "UD2", "UD3", "UD4",
                "UD5", "UD6", "UD7", "UD8"
            };

            if (!string.IsNullOrEmpty(customWhereClause))
            {
                config.FilterConditions = new List<string> { customWhereClause };
            }

            return config;
        }
    }

    /// <summary>
    /// Example usage and implementation templates for common scenarios
    /// </summary>
    public class FMM_Table_Calc_Examples
    {
        /// <summary>
        /// Example: Load CMD PGM Requirements data to cube
        /// Replaces the hardcoded CMD_PGM_FinCustCalc.Load_Reqs_to_Cube() method
        /// </summary>
        public static void Example_CMD_PGM_Load(
            SessionInfo si, 
            BRGlobals globals, 
            FinanceRulesApi api, 
            FinanceRulesArgs args)
        {
            // Parse filters from global variables
            FMM_Table_Calc_Builder.ParseGlobalFilters(
                globals, 
                api, 
                out List<string> statusFilters, 
                out List<string> appnFilters
            );

            // Build configuration
            var config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
                prefix: "CMD_PGM",
                timeCalculation: "Annual",
                accounts: new List<string> { "Req_Funding" },
                statusFilters: statusFilters,
                appnFilters: appnFilters
            );

            // Execute
            var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
            engine.LoadTableDataToCube(config);
        }

        /// <summary>
        /// Example: Load CMD SPLN Requirements data to cube
        /// Replaces the hardcoded CMD_SPLN_FinCustCalc.Load_Reqs_to_Cube() method
        /// </summary>
        public static void Example_CMD_SPLN_Load(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            // Parse filters from global variables
            FMM_Table_Calc_Builder.ParseGlobalFilters(
                globals,
                api,
                out List<string> statusFilters,
                out List<string> dimensionFilters
            );

            // Build configuration for periodic loading
            var config = FMM_Table_Calc_Builder.BuildPeriodicTableConfig(
                prefix: "CMD_SPLN",
                accounts: new List<string> { "Commitments", "Obligations", "WH_Commitments", "WH_Obligations" },
                statusFilters: statusFilters,
                dimensionFilters: dimensionFilters,
                filterDimension: "UD3"
            );

            // Execute
            var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
            engine.LoadTableDataToCube(config);
        }

        /// <summary>
        /// Example: Aggregate/Consolidate data across entity hierarchy
        /// Replaces the hardcoded Consol_Aggregated() methods
        /// </summary>
        public static void Example_Consolidate_Aggregated(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            // Build aggregation configuration
            var config = FMM_Table_Calc_Builder.BuildStandardAggregationConfig(
                configName: "PGM_Consolidation",
                accounts: new List<string> { "Req_Funding" }
            );

            // Execute aggregation
            var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
            engine.AggregateData(config);
        }
    }
}
