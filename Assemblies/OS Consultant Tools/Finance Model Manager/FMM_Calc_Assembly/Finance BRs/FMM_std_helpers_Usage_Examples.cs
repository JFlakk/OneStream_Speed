using System;
using System.Collections.Generic;
using System.Data;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Engine;
using OneStreamWorkspacesApi;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Comprehensive usage examples for FMM_std_helpers
    /// Demonstrates all 5 main use cases with real-world scenarios
    /// </summary>
    public class FMM_std_helpers_Usage_Examples
    {
        #region Example 1: Cube Calculations

        /// <summary>
        /// Example: Copy data from one account to another with rate spreads (like CMD SPLN CivPay)
        /// Use Case: Spread annual target funding to monthly commitments/obligations based on rates
        /// </summary>
        public static void Example_Cube_CopyWithRateSpread(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Step 1: Get rate table from cube or database
                var rateTable = GetRateTableFromCube(si, globals, api, "CMD_SPLN_APPN_SpreadPct_FDX_CV");
                
                // Step 2: Configure cube calculation
                var config = new FMM_std_helpers.CubeCalcConfig
                {
                    // Source: Get target funding from cube
                    SourceMemberScript = "FilterMembers(RemoveZeros(" +
                        "E#{POV}:S#{POV}:T#{POV}:C#Aggregated:V#Periodic:" +
                        "F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])",
                    
                    // Target accounts to spread to
                    AccountFilter = new List<string> { "Commitments", "Obligations" },
                    
                    // Dimension mappings for target
                    DimensionMappings = new Dictionary<string, string>
                    {
                        { "UD6", "Pay_General" },  // Change UD6 to Pay_General
                        { "Origin", "Import" }      // Set origin to Import
                    },
                    
                    TargetOrigin = "Import",
                    ClearTargetData = true
                };
                
                // Step 3: Execute with rate spread
                helper.CopyCubeDataWithTransform(config, rateTable, "UD1");
                
                BRApi.ErrorLog.LogMessage(si, "Completed cube copy with rate spread");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Simple cube-to-cube copy with dimension transformation
        /// Use Case: Copy actuals to forecast scenario with different flow
        /// </summary>
        public static void Example_Cube_SimpleCopy(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.CubeCalcConfig
                {
                    SourceMemberScript = "E#POV:S#Actual:T#POV:V#Periodic:A#Revenue:F#Ending:O#Import",
                    
                    DimensionMappings = new Dictionary<string, string>
                    {
                        { "Scenario", "Forecast" },
                        { "Flow", "Target" }
                    },
                    
                    MultiplierFactor = 1.05m,  // 5% increase
                    TargetOrigin = "Import",
                    ClearTargetData = true
                };
                
                // Read, transform, write
                var sourceData = helper.ReadCubeData(config);
                helper.WriteCubeData(sourceData, config);
                
                BRApi.ErrorLog.LogMessage(si, "Completed simple cube copy");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Example 2: Table Operations

        /// <summary>
        /// Example: Read requirements from custom table with filters
        /// Use Case: Get all requirements for current entity and workflow
        /// </summary>
        public static void Example_Table_ReadWithFilters(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.TableOpConfig
                {
                    TableName = "XFC_CMD_PGM_REQ_Details",
                    SelectColumns = new List<string> 
                    { 
                        "Entity", "Account", "Flow", "UD1", "UD2", "Yearly", "Create_Date" 
                    },
                    FilterConditions = new Dictionary<string, object>
                    {
                        { "Account", "Req_Funding" },
                        { "Flow", new List<string> { "L2_Formulate_PGM", "L3_Formulate_PGM" } }
                    },
                    UseWorkflowFilters = true  // Adds scenario, time, entity filters
                };
                
                var data = helper.ReadTableData(config);
                
                BRApi.ErrorLog.LogMessage(si, $"Read {data.Rows.Count} requirement records");
                
                // Process the data
                foreach (DataRow row in data.Rows)
                {
                    string entity = row["Entity"].ToString();
                    decimal amount = Convert.ToDecimal(row["Yearly"]);
                    BRApi.ErrorLog.LogMessage(si, $"Entity: {entity}, Amount: {amount}");
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Write/update data to custom table
        /// Use Case: Update requirement status after approval
        /// </summary>
        public static void Example_Table_WriteWithMerge(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Create update data
                var updateData = new DataTable();
                updateData.Columns.Add("CMD_PGM_REQ_ID", typeof(Guid));
                updateData.Columns.Add("Status", typeof(string));
                updateData.Columns.Add("Update_Date", typeof(DateTime));
                updateData.Columns.Add("Update_User", typeof(string));
                
                var row = updateData.NewRow();
                row["CMD_PGM_REQ_ID"] = Guid.NewGuid();
                row["Status"] = "Approved";
                row["Update_Date"] = DateTime.Now;
                row["Update_User"] = si.UserName;
                updateData.Rows.Add(row);
                
                // Configure write operation
                var config = new FMM_std_helpers.TableOpConfig
                {
                    TableName = "XFC_CMD_PGM_REQ"
                };
                
                // Write with merge (update if exists, insert if new)
                var keyColumns = new List<string> { "CMD_PGM_REQ_ID" };
                helper.WriteTableData(updateData, config, keyColumns);
                
                BRApi.ErrorLog.LogMessage(si, "Updated requirement status");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Delete stale data from table
        /// Use Case: Clear old requirements before reloading
        /// </summary>
        public static void Example_Table_DeleteFiltered(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.TableOpConfig
                {
                    TableName = "XFC_CMD_PGM_REQ_Details",
                    FilterConditions = new Dictionary<string, object>
                    {
                        { "Flow", new List<string> { "L2_Formulate_PGM" } },
                        { "Status", "Draft" }
                    },
                    UseWorkflowFilters = true
                };
                
                helper.DeleteTableData(config);
                
                BRApi.ErrorLog.LogMessage(si, "Deleted draft requirements");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Example 3: Table to Cube

        /// <summary>
        /// Example: Load requirements table to cube using FMM framework (RECOMMENDED)
        /// Use Case: Load CMD_PGM requirements with annual data
        /// </summary>
        public static void Example_TableToCube_Framework_Annual(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Parse filters from global variables (set by UI)
                FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api, 
                    out var statusFilters, out var appnFilters);
                
                // Load using framework (easiest and fastest approach)
                helper.LoadTableToCube_Framework(
                    tablePrefix: "CMD_PGM",
                    timeCalculation: "Annual",
                    accounts: new List<string> { "Req_Funding", "Target" },
                    statusFilters: statusFilters,
                    dimensionFilters: appnFilters
                );
                
                BRApi.ErrorLog.LogMessage(si, "Loaded CMD_PGM requirements to cube");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Load spend plan table to cube with monthly data
        /// Use Case: Load CMD_SPLN with Month1-Month12 columns
        /// </summary>
        public static void Example_TableToCube_Framework_Monthly(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api,
                    out var statusFilters, out var appnFilters);
                
                helper.LoadTableToCube_Framework(
                    tablePrefix: "CMD_SPLN",
                    timeCalculation: "Period",  // For monthly data
                    accounts: new List<string> { "Commitments", "Obligations" },
                    statusFilters: statusFilters,
                    dimensionFilters: appnFilters,
                    filterDimension: "UD3"  // SPLN uses UD3 for APPN filter
                );
                
                BRApi.ErrorLog.LogMessage(si, "Loaded CMD_SPLN spend plan to cube");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Custom table to cube load with complex mappings
        /// Use Case: Load from non-standard table structure
        /// </summary>
        public static void Example_TableToCube_Custom(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Table configuration
                var tableConfig = new FMM_std_helpers.TableOpConfig
                {
                    TableName = "XFC_Custom_Budget_Data",
                    WhereClause = "Active = 1 AND Budget_Year = 2024",
                    UseWorkflowFilters = false  // Custom table structure
                };
                
                // Cube configuration
                var cubeConfig = new FMM_std_helpers.CubeCalcConfig
                {
                    TargetOrigin = "Import",
                    ClearTargetData = true
                };
                
                // Map table columns to cube dimensions
                var columnMapping = new Dictionary<string, string>
                {
                    { "Org_Code", "Entity" },
                    { "GL_Account", "Account" },
                    { "Fund_Code", "UD1" },
                    { "Dept_Code", "UD2" },
                    { "Project_Code", "UD3" },
                    { "Period", "Time" }
                };
                
                helper.LoadTableToCube_Custom(tableConfig, cubeConfig, columnMapping);
                
                BRApi.ErrorLog.LogMessage(si, "Loaded custom budget data to cube");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Example 4: BRCubeToTable (Cube to Table)

        /// <summary>
        /// Example: Extract cube data to staging table for reporting
        /// Use Case: Extract actuals to table for external reporting or analysis
        /// </summary>
        public static void Example_CubeToTable_Extract(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.CubeToTableConfig
                {
                    // Extract actuals data
                    SourceMemberScript = 
                        "E#POV.Base.Descendants:S#Actual:T#2024M1:2024M12:" +
                        "V#Periodic:A#Revenue,A#Expenses:F#Ending:O#Import",
                    
                    // Target staging table
                    TargetTableName = "XFC_Actuals_Extract",
                    
                    // Map dimensions to table columns
                    DimensionToColumnMap = new Dictionary<string, string>
                    {
                        { "Entity", "Entity_Code" },
                        { "Account", "Account_Code" },
                        { "Time", "Period" },
                        { "UD1", "Fund_Code" },
                        { "UD2", "Dept_Code" },
                        { "UD3", "Project_Code" }
                    },
                    
                    // Additional metadata columns
                    AdditionalColumns = new List<string> { "Extract_Date", "Extract_User" },
                    
                    ClearTargetTable = true,
                    IncludeWorkflowInfo = true
                };
                
                helper.ExtractCubeToTable(config);
                
                BRApi.ErrorLog.LogMessage(si, "Extracted cube data to staging table");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Extract forecast data for external integration
        /// Use Case: Export forecast to interface table for ERP system
        /// </summary>
        public static void Example_CubeToTable_ForecastExport(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.CubeToTableConfig
                {
                    SourceMemberScript =
                        "E#POV:S#Forecast:T#2024M1:2024M12:" +
                        "V#Periodic:A#Budget_Accounts.Base:" +
                        "F#Target:O#Import:I#None",
                    
                    TargetTableName = "XFC_ERP_Interface_Forecast",
                    
                    DimensionToColumnMap = new Dictionary<string, string>
                    {
                        { "Entity", "CostCenter" },
                        { "Account", "GLAccount" },
                        { "Time", "FiscalPeriod" },
                        { "UD1", "FundSource" }
                    },
                    
                    ClearTargetTable = true,
                    IncludeWorkflowInfo = false  // ERP doesn't need workflow info
                };
                
                helper.ExtractCubeToTable(config);
                
                BRApi.ErrorLog.LogMessage(si, "Exported forecast to ERP interface table");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Example 5: Consolidation

        /// <summary>
        /// Example: Consolidate data using FMM framework (RECOMMENDED)
        /// Use Case: Roll up base entity data to parent with flow filters
        /// </summary>
        public static void Example_Consolidation_Framework(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Define flow filters by entity level
                var levelFlowFilters = new Dictionary<string, List<string>>
                {
                    {
                        "L2",
                        new List<string>
                        {
                            "L2_Formulate_PGM", "L3_Formulate_PGM", "L4_Formulate_PGM",
                            "L2_Validate_PGM", "L3_Validate_PGM", "L4_Validate_PGM",
                            "L2_Approve_PGM", "L2_Final_PGM"
                        }
                    },
                    {
                        "L3",
                        new List<string>
                        {
                            "L3_Formulate_PGM", "L4_Formulate_PGM",
                            "L3_Validate_PGM", "L4_Validate_PGM",
                            "L3_Approve_PGM", "L2_Approve_PGM"
                        }
                    },
                    {
                        "L4",
                        new List<string>
                        {
                            "L4_Formulate_PGM",
                            "L4_Validate_PGM",
                            "L3_Approve_PGM"
                        }
                    }
                };
                
                helper.ConsolidateData_Framework(
                    accounts: new List<string> { "Req_Funding", "Target" },
                    levelFlowFilters: levelFlowFilters
                );
                
                BRApi.ErrorLog.LogMessage(si, "Consolidated data using framework");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Custom consolidation with origin transformation
        /// Use Case: Aggregate Import origin from base entities to AdjConsolidated at parent
        /// </summary>
        public static void Example_Consolidation_Custom(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                var config = new FMM_std_helpers.CubeCalcConfig
                {
                    SourceMemberScript = 
                        "E#POV.Base.Descendants:S#POV:T#POV:V#Periodic:" +
                        "A#Req_Funding:F#L3_Formulate_PGM,F#L4_Formulate_PGM:" +
                        "O#Import:I#None",
                    
                    // Transform origin for parent entity
                    DimensionMappings = new Dictionary<string, string>
                    {
                        { "Origin", "AdjConsolidated" }
                    },
                    
                    ClearTargetData = true
                };
                
                helper.ConsolidateData_Custom(config);
                
                BRApi.ErrorLog.LogMessage(si, "Consolidated with custom origin transformation");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Example: Selective consolidation with account filters
        /// Use Case: Consolidate only specific accounts
        /// </summary>
        public static void Example_Consolidation_Selective(
            SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                var helper = new FMM_std_helpers(si, globals, api, args);
                
                // Consolidate only revenue accounts
                helper.ConsolidateData_Framework(
                    accounts: new List<string> { "Revenue", "Sales", "Service_Revenue" }
                );
                
                BRApi.ErrorLog.LogMessage(si, "Consolidated revenue accounts");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Helper Methods for Examples

        private static DataTable GetRateTableFromCube(SessionInfo si, BRGlobals globals, 
            FinanceRulesApi api, string cubeViewName)
        {
            // Check if cached in globals
            string cacheKey = $"RateTable_{cubeViewName}";
            var cachedTable = globals.GetObject(cacheKey) as DataTable;
            
            if (cachedTable != null)
            {
                return cachedTable;
            }
            
            // In real implementation, read from cube view or database
            var rateTable = new DataTable();
            rateTable.Columns.Add("UD1", typeof(string));
            for (int i = 1; i <= 13; i++)
            {
                rateTable.Columns.Add($"Time{i}", typeof(decimal));
            }
            
            // Cache and return
            globals.SetObject(cacheKey, rateTable);
            return rateTable;
        }

        #endregion
    }
}
