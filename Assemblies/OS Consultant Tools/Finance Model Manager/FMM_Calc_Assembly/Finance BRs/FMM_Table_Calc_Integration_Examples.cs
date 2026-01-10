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

        #region Hours * Rate Example: Table Data with Cube Data

        /// <summary>
        /// Example showing how to calculate costs using hours from a details table
        /// and rates from the cube (or another table).
        /// Demonstrates the use case: hours * rate where rate comes from cube
        /// </summary>
        public static void Example_HoursTimesRate_TableWithCube(
            SessionInfo si,
            BRGlobals globals,
            FinanceRulesApi api,
            FinanceRulesArgs args)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Starting Hours * Rate calculation example");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                // ================================================================
                // OPTION 1: Pure SQL Approach (Fastest - use when both in tables)
                // ================================================================
                
                // If rates are also in a table, use SQL JOIN for best performance
                var sqlConfig = new FMM_Table_Calc_Config
                {
                    ConfigName = "ProjectCosts_HoursRate_SQL",
                    SourceTable = "XFC_Project_Hours",
                    TimeCalculation = "Annual",
                    HandleParentEntities = true,
                    ClearStaleData = true,
                    TargetView = "V#Periodic",
                    
                    // SQL performs hours * rate calculation at database level
                    CustomSQL = @"
                        SELECT 
                            ph.Entity,
                            ph.Account AS Account,
                            ph.Flow,
                            ph.UD1 AS Project,
                            SUM(ph.Hours * COALESCE(pr.Rate, 0)) AS Tot_Amount
                        FROM XFC_Project_Hours ph
                        LEFT JOIN XFC_Pay_Rates pr 
                            ON ph.Entity = pr.Entity
                            AND ph.Pay_Grade = pr.Pay_Grade
                            AND ph.Scenario = pr.Scenario
                            AND ph.Fiscal_Year = pr.Fiscal_Year
                        WHERE ph.Scenario = @Scenario
                          AND ph.Fiscal_Year = @FiscalYear
                        GROUP BY ph.Entity, ph.Account, ph.Flow, ph.UD1
                    ",
                    
                    Parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Scenario", System.Data.SqlDbType.NVarChar) 
                            { Value = api.Pov.Scenario.Name },
                        new SqlParameter("@FiscalYear", System.Data.SqlDbType.NVarChar) 
                            { Value = api.Pov.Time.Name }
                    }
                };
                
                sqlConfig.SetStandardDimensionMapping();
                sqlConfig.GroupByColumns = new List<string> { "Entity", "Account", "Flow", "Project" };
                
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(sqlConfig);
                
                stopwatch.Stop();
                BRApi.ErrorLog.LogMessage(si, $"SQL approach completed in {stopwatch.ElapsedMilliseconds}ms");
                
                // ================================================================
                // OPTION 2: Hybrid Approach (Use when rate is in cube)
                // ================================================================
                
                stopwatch.Restart();
                
                // Step 1: Load rate buffer from cube
                var rateInfo = api.Data.GetCalculationInfo("V#Periodic", "A#[Pay_Rate].Base");
                var rateBuffer = api.Data.GetDataBuffer(rateInfo);
                
                // Cache the rate buffer for reuse
                string rateCacheKey = "PayRateBuffer_" + api.Pov.Scenario.Name;
                globals.SetObject(rateCacheKey, rateBuffer);
                
                // Step 2: Get hours from detail table
                var hoursTable = GetProjectHoursFromTable(si, api.Pov.Scenario.Name, api.Pov.Time.Name);
                
                // Step 3: Initialize global functions for GetBCValue
                var globalFunctions = new FMM_Global_Functions(si, globals, api, args);
                
                // Step 4: Create destination buffer
                var destBuffer = new DataBuffer();
                
                // Step 5: Iterate through hours and multiply by cube rates
                foreach (System.Data.DataRow hourRow in hoursTable.Rows)
                {
                    string entity = hourRow["Entity"].ToString();
                    string account = hourRow["Account"].ToString();
                    string project = hourRow["Project"].ToString();
                    string payGrade = hourRow["Pay_Grade"].ToString();
                    decimal hours = Convert.ToDecimal(hourRow["Hours"]);
                    
                    // Create cell reference to look up rate in cube
                    var rateCell = new DataBufferCell();
                    rateCell.DataBufferCellPk.SetEntity(api, entity);
                    rateCell.DataBufferCellPk.SetAccount(api, "Pay_Rate");
                    rateCell.DataBufferCellPk.SetFlow(api, "Input");
                    rateCell.DataBufferCellPk.SetOrigin(api, "Import");
                    rateCell.DataBufferCellPk.SetUD1(api, payGrade);
                    
                    // Get rate from cube using GetBCValue
                    decimal rate = globalFunctions.GetBCValue(
                        ref rateCell, 
                        rateBuffer,
                        DriverDB_Acct: "Pay_Rate",
                        DriverDB_Flow: "Input",
                        DriverDB_Origin: "Import",
                        DriverDB_UD1: payGrade);
                    
                    // Calculate: Hours * Rate
                    decimal totalCost = hours * rate;
                    
                    // Write result to destination buffer
                    if (totalCost != 0)
                    {
                        var destCell = new DataBufferCell();
                        destCell.DataBufferCellPk.SetEntity(api, entity);
                        destCell.DataBufferCellPk.SetAccount(api, account);
                        destCell.DataBufferCellPk.SetFlow(api, "Input");
                        destCell.DataBufferCellPk.SetOrigin(api, "AdjInput");
                        destCell.DataBufferCellPk.SetUD1(api, project);
                        
                        destCell.CellAmount = totalCost;
                        destCell.CellStatus = new DataCellStatus(true);
                        
                        destBuffer.SetCell(si, destCell);
                    }
                }
                
                // Step 6: Write buffer to cube
                if (destBuffer.DataBufferCells.Count > 0)
                {
                    var destInfo = api.Data.GetExpressionDestinationInfo("V#Periodic");
                    api.Data.SetDataBuffer(destBuffer, destInfo);
                }
                
                stopwatch.Stop();
                BRApi.ErrorLog.LogMessage(si, 
                    $"Hybrid approach (table + cube) completed: {destBuffer.DataBufferCells.Count} cells written in {stopwatch.ElapsedMilliseconds}ms");
                
                // ================================================================
                // OPTION 3: Cube View to Table Approach (CMD PGM/SPLN pattern)
                // ================================================================
                
                // Load rates from cube view into temp table, then SQL JOIN
                // This is useful when you need complex cube calculations before the join
                
                stopwatch.Restart();
                
                var objDashboardWorkspace = BRApi.Dashboards.Workspaces.GetWorkspace(si, false, "Finance");
                var cubeViewData = new System.Data.DataTable();
                var nvbParams = new NameValueFormatBuilder();
                
                // Execute cube view to get rates
                cubeViewData = BRApi.Import.Data.FdxExecuteCubeView(
                    si, 
                    objDashboardWorkspace.WorkspaceID, 
                    "PayRates_CubeView",  // Cube view with rate data
                    api.Pov.Entity.Dimension.Name, 
                    $"E#{api.Pov.Entity.Name}", 
                    api.Pov.Scenario.Dimension.Name, 
                    $"S#{api.Pov.Scenario.Name}", 
                    api.Pov.Time.Name, 
                    nvbParams, 
                    false, false, string.Empty, 8, true);
                
                // Now cubeViewData contains rates from cube - could write to temp table
                // Then use SQL JOIN between hours table and temp rate table
                
                stopwatch.Stop();
                BRApi.ErrorLog.LogMessage(si, 
                    $"Cube view extraction completed in {stopwatch.ElapsedMilliseconds}ms - {cubeViewData.Rows.Count} rate rows extracted");
                
                BRApi.ErrorLog.LogMessage(si, "Hours * Rate calculation example completed successfully");
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error in Hours * Rate example: {ex.Message}");
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Helper method to retrieve project hours from details table
        /// </summary>
        private static System.Data.DataTable GetProjectHoursFromTable(
            SessionInfo si, 
            string scenario, 
            string fiscalYear)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            var dt = new System.Data.DataTable();
            
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();
                
                var sql = @"
                    SELECT 
                        Entity, 
                        Account, 
                        Project, 
                        Pay_Grade, 
                        Hours
                    FROM XFC_Project_Hours
                    WHERE Scenario = @Scenario
                      AND Fiscal_Year = @FiscalYear
                      AND Hours > 0";
                
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Scenario", scenario);
                    command.Parameters.AddWithValue("@FiscalYear", fiscalYear);
                    
                    using (var adapter = new SqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            
            return dt;
        }

        #endregion
    }
}
