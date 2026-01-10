using System;
using System.Collections.Generic;
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
    /// Reference implementation showing how to integrate FMM Table Calculation Framework
    /// into existing CMD PGM and CMD SPLN implementations
    /// 
    /// Usage: Replace existing Load_Reqs_to_Cube and Consol_Aggregated methods with these implementations
    /// </summary>
    public class FMM_Table_Calc_Integration_Examples
    {
        #region CMD PGM Integration Example

        /// <summary>
        /// Replacement for CMD_PGM_FinCustCalc.Load_Reqs_to_Cube()
        /// Original: 237 lines of VB.NET code
        /// New: 20 lines of configurable C# code
        /// </summary>
        public static void CMD_PGM_Load_Reqs_to_Cube_New(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                // Parse dynamic filters from global variables
                // These are set by the UI/dashboard to control what data loads
                FMM_Table_Calc_Builder.ParseGlobalFilters(
                    globals,
                    api,
                    out List<string> statusFilters,
                    out List<string> appnFilters
                );

                // Build configuration using helper method
                var config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
                    prefix: "CMD_PGM",
                    timeCalculation: "Annual",
                    accounts: new List<string> { "Req_Funding" },
                    statusFilters: statusFilters,
                    appnFilters: appnFilters
                );

                // Execute the load
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Replacement for CMD_PGM_FinCustCalc.Consol_Aggregated()
        /// Original: 63 lines of VB.NET code
        /// New: 15 lines of configurable C# code
        /// </summary>
        public static void CMD_PGM_Consol_Aggregated_New(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                // Build aggregation configuration with level-specific flow filters
                var config = FMM_Table_Calc_Builder.BuildStandardAggregationConfig(
                    configName: "PGM_Consolidation",
                    accounts: new List<string> { "Req_Funding" },
                    levelFlowFilters: new Dictionary<string, List<string>>
                    {
                        {
                            "L2",
                            new List<string>
                            {
                                "L2_Formulate_PGM", "L3_Formulate_PGM", "L4_Formulate_PGM", "L5_Formulate_PGM",
                                "L3_Validate_PGM", "L4_Validate_PGM", "L5_Validate_PGM",
                                "L3_Approve_PGM", "L2_Validate_PGM", "L2_Approve_PGM", "L2_Final_PGM"
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
                    }
                );

                // Execute aggregation
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.AggregateData(config);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region CMD SPLN Integration Example

        /// <summary>
        /// Replacement for CMD_SPLN_FinCustCalc.Load_Reqs_to_Cube()
        /// Original: 249 lines of VB.NET code
        /// New: 20 lines of configurable C# code
        /// </summary>
        public static void CMD_SPLN_Load_Reqs_to_Cube_New(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                // Parse dynamic filters from global variables
                FMM_Table_Calc_Builder.ParseGlobalFilters(
                    globals,
                    api,
                    out List<string> statusFilters,
                    out List<string> appnFilters
                );

                // Build configuration for periodic (monthly) loading
                var config = FMM_Table_Calc_Builder.BuildPeriodicTableConfig(
                    prefix: "CMD_SPLN",
                    accounts: new List<string> { "Commitments", "Obligations", "WH_Commitments", "WH_Obligations" },
                    statusFilters: statusFilters,
                    dimensionFilters: appnFilters,
                    filterDimension: "UD3"  // SPLN uses UD3 for APPN filtering
                );

                // Execute the load
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Advanced Custom Example

        /// <summary>
        /// Example showing more advanced customization
        /// For scenarios that don't fit the standard patterns exactly
        /// </summary>
        public static void Custom_Table_Load_Advanced(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                // Create custom configuration
                var config = new FMM_Table_Calc_Config
                {
                    ConfigName = "CustomAdvancedLoad",
                    SourceTable = "XFC_Custom_REQ_Details",
                    TimeCalculation = "Annual",
                    HandleParentEntities = true,
                    ClearStaleData = true
                };

                // Set standard dimension mapping
                config.SetStandardDimensionMapping();

                // Custom grouping - exclude certain dimensions
                config.GroupByColumns = new List<string>
                {
                    "Entity", "Account", "Flow", "UD1", "UD2", "UD3"
                    // Note: Excluding UD4-UD8 for this scenario
                };

                // Complex filters
                config.FilterConditions = new List<string>
                {
                    "Account IN ('Req_Funding', 'Target')",
                    "(Flow LIKE '%Formulate%' OR Flow LIKE '%Validate%')",
                    "Amount > 0",
                    "Status = 'Active'"
                };

                // Custom origin handling
                config.TargetOrigin = "Import";
                config.OriginFilter = "O#Import,O#AdjInput";

                // Execute
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example showing configuration caching for performance
        /// Useful when the same calculation runs multiple times
        /// </summary>
        public static void Cached_Configuration_Example(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                // Try to get cached configuration
                string cacheKey = "CMD_PGM_LoadConfig";
                var config = globals.GetObject(cacheKey) as FMM_Table_Calc_Config;

                // If not cached, build and cache it
                if (config == null)
                {
                    FMM_Table_Calc_Builder.ParseGlobalFilters(
                        globals,
                        api,
                        out List<string> statusFilters,
                        out List<string> appnFilters
                    );

                    config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
                        "CMD_PGM",
                        "Annual",
                        new List<string> { "Req_Funding" },
                        statusFilters,
                        appnFilters
                    );

                    globals.SetObject(cacheKey, config);
                    BRApi.ErrorLog.LogMessage(si, "Configuration built and cached");
                }
                else
                {
                    BRApi.ErrorLog.LogMessage(si, "Using cached configuration");
                }

                // Execute with cached config
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Debugging Helpers

        /// <summary>
        /// Example with detailed logging for troubleshooting
        /// </summary>
        public static void Debug_Table_Load_Example(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Starting table load debug example");

                // Parse filters and log
                FMM_Table_Calc_Builder.ParseGlobalFilters(
                    globals,
                    api,
                    out List<string> statusFilters,
                    out List<string> appnFilters
                );

                BRApi.ErrorLog.LogMessage(si, $"Status Filters: {string.Join(", ", statusFilters)}");
                BRApi.ErrorLog.LogMessage(si, $"APPN Filters: {string.Join(", ", appnFilters)}");

                // Build configuration
                var config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
                    "CMD_PGM",
                    "Annual",
                    new List<string> { "Req_Funding" },
                    statusFilters,
                    appnFilters
                );

                BRApi.ErrorLog.LogMessage(si, $"Configuration: {config.ConfigName}");
                BRApi.ErrorLog.LogMessage(si, $"Source Table: {config.SourceTable}");
                BRApi.ErrorLog.LogMessage(si, $"Time Calculation: {config.TimeCalculation}");

                // Execute
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);

                BRApi.ErrorLog.LogMessage(si, "Table load completed successfully");
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error in table load: {ex.Message}");
                BRApi.ErrorLog.LogMessage(si, $"Stack trace: {ex.StackTrace}");
                throw new XFException(si, ex);
            }
        }

        #endregion
    }
}
