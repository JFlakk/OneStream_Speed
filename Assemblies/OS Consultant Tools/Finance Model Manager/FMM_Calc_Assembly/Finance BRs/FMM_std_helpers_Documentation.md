# FMM Standard Helpers - Comprehensive Documentation

## Overview

The **FMM_std_helpers** class provides a comprehensive, configurable framework for common OneStream calculation patterns. It simplifies complex operations by providing reusable helper methods for five major use cases:

1. **Cube Calculations** - Reading, writing, and transforming cube data
2. **Table Operations** - CRUD operations on custom tables
3. **Table to Cube** - Loading table data into the cube
4. **BRCubeToTable** - Extracting cube data to tables
5. **Consolidation** - Aggregating data across entity hierarchies

## Why Use FMM Standard Helpers?

### Benefits

- **Reusability**: Write once, use across all calculations
- **Consistency**: Standard patterns across your application
- **Maintainability**: Central location for common operations
- **Performance**: Optimized data handling and caching
- **Integration**: Works seamlessly with FMM Table Calc Framework
- **Flexibility**: Configurable for various scenarios

### Comparison

| Approach | Lines of Code | Development Time | Maintenance | Performance |
|----------|--------------|------------------|-------------|-------------|
| **Manual Implementation** | 200-300 lines | 4-8 hours | High | Variable |
| **FMM std_helpers** | 15-30 lines | 30-60 minutes | Low | Optimized |

## Quick Start Guide

### Installation

The helpers are part of the FMM_Calc_Assembly. Simply reference them in your finance rules:

```csharp
using Workspace.OSConsTools.FMM_Calc_Assembly;

// In your calculation method
var helper = new FMM_std_helpers(si, globals, api, args);
```

### Basic Pattern

All helper methods follow this pattern:

1. Create helper instance
2. Build configuration object
3. Call helper method
4. Handle results

## Use Case 1: Cube Calculations

### Overview

Cube calculations involve reading data from the cube, transforming it, and writing it back. Common scenarios include:

- Copying data between accounts
- Spreading annual amounts to monthly periods
- Applying rates or factors to data
- Transforming dimensions (account, flow, origin, etc.)

### Configuration

```csharp
var config = new FMM_std_helpers.CubeCalcConfig
{
    SourceMemberScript = "E#POV:S#POV:T#POV:...",  // Member script
    TargetOrigin = "Import",                        // Target origin
    AccountFilter = new List<string> { "..." },     // Target accounts
    FlowFilter = new List<string> { "..." },        // Flow filters
    DimensionMappings = new Dictionary<string, string>  // Dimension transformations
    {
        { "Account", "NewAccount" },
        { "Flow", "NewFlow" }
    },
    ClearTargetData = true,                         // Clear before write
    MultiplierFactor = 1.0m                         // Apply factor
};
```

### Methods

#### ReadCubeData

Read data from cube using member script:

```csharp
var sourceData = helper.ReadCubeData(config);
// Returns: DataBuffer with matching cells
```

#### WriteCubeData

Write transformed data to cube:

```csharp
helper.WriteCubeData(sourceData, config);
// Applies: Dimension mappings, multiplier, clears target
```

#### CopyCubeDataWithTransform

Complete copy operation with optional rate spreads:

```csharp
helper.CopyCubeDataWithTransform(config, rateTable, "UD1");
// Reads source, applies rates/mappings, writes to cube
```

### Real-World Examples

#### Example 1: Simple Account Copy

```csharp
// Copy Target to Commitments with 5% increase
var config = new FMM_std_helpers.CubeCalcConfig
{
    SourceMemberScript = "E#POV:S#POV:T#POV:A#Target:F#Ending:O#Import",
    DimensionMappings = new Dictionary<string, string>
    {
        { "Account", "Commitments" }
    },
    MultiplierFactor = 1.05m,
    TargetOrigin = "Import",
    ClearTargetData = true
};

var sourceData = helper.ReadCubeData(config);
helper.WriteCubeData(sourceData, config);
```

#### Example 2: Rate-Based Spread (CMD SPLN Pattern)

```csharp
// Spread annual target to monthly commitments/obligations
var rateTable = GetRateTableFromDatabase("APPN_Rates");

var config = new FMM_std_helpers.CubeCalcConfig
{
    SourceMemberScript = "E#POV:S#POV:T#2024:A#Target:F#Ending:O#Top",
    AccountFilter = new List<string> { "Commitments", "Obligations" },
    DimensionMappings = new Dictionary<string, string>
    {
        { "UD6", "Pay_General" },
        { "Origin", "Import" }
    },
    TargetOrigin = "Import",
    ClearTargetData = true
};

helper.CopyCubeDataWithTransform(config, rateTable, "UD1");
```

## Use Case 2: Table Operations

### Overview

Table operations provide CRUD functionality for custom tables, including:

- Reading requirements, details, or configuration data
- Writing/updating records with merge logic
- Deleting filtered data
- Workflow-aware filtering

### Configuration

```csharp
var config = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_YourTable",
    SelectColumns = new List<string> { "Col1", "Col2" },  // Optional
    FilterConditions = new Dictionary<string, object>
    {
        { "Status", "Active" },
        { "Account", new List<string> { "A1", "A2" } }
    },
    WhereClause = "Amount > 0",  // Additional custom SQL
    UseWorkflowFilters = true    // Auto-add workflow filters
};
```

### Methods

#### ReadTableData

Read data from custom table:

```csharp
DataTable data = helper.ReadTableData(config);
// Returns: DataTable with matching rows
```

#### WriteTableData

Write/merge data to table:

```csharp
helper.WriteTableData(sourceData, config, keyColumns);
// Updates existing rows, inserts new rows
```

#### DeleteTableData

Delete filtered data:

```csharp
helper.DeleteTableData(config);
// Deletes rows matching filters
```

### Real-World Examples

#### Example 1: Read Requirements

```csharp
var config = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_CMD_PGM_REQ_Details",
    FilterConditions = new Dictionary<string, object>
    {
        { "Account", "Req_Funding" },
        { "Flow", new List<string> { "L2_Formulate_PGM", "L3_Formulate_PGM" } }
    },
    UseWorkflowFilters = true  // Adds Entity, Scenario, Time filters
};

var requirements = helper.ReadTableData(config);
foreach (DataRow req in requirements.Rows)
{
    BRApi.ErrorLog.LogMessage(si, $"REQ: {req["Entity"]} - {req["Yearly"]}");
}
```

#### Example 2: Update Status

```csharp
// Read existing requirements
var readConfig = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_CMD_PGM_REQ",
    FilterConditions = new Dictionary<string, object>
    {
        { "Status", "Pending" }
    }
};

var pendingReqs = helper.ReadTableData(readConfig);

// Update status to Approved
foreach (DataRow req in pendingReqs.Rows)
{
    req["Status"] = "Approved";
    req["Update_Date"] = DateTime.Now;
    req["Update_User"] = si.UserName;
}

// Write back with merge
var writeConfig = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_CMD_PGM_REQ"
};

helper.WriteTableData(pendingReqs, writeConfig, new List<string> { "CMD_PGM_REQ_ID" });
```

## Use Case 3: Table to Cube

### Overview

Load data from custom tables into the OneStream cube. Two approaches available:

1. **Framework Method (Recommended)**: Uses FMM Table Calc Framework for standard patterns
2. **Custom Method**: For non-standard table structures or complex mappings

### Framework Method (Recommended)

#### Configuration

No complex config needed - just specify:

- Table prefix (e.g., "CMD_PGM")
- Time calculation type ("Annual", "Period", "Fiscal_Year")
- Account list
- Filters (optional)

#### Method

```csharp
helper.LoadTableToCube_Framework(
    tablePrefix: "CMD_PGM",
    timeCalculation: "Annual",
    accounts: new List<string> { "Req_Funding" },
    statusFilters: statusFilters,  // From globals
    dimensionFilters: appnFilters   // From globals
);
```

### Custom Method

For non-standard patterns:

```csharp
var tableConfig = new FMM_std_helpers.TableOpConfig { ... };
var cubeConfig = new FMM_std_helpers.CubeCalcConfig { ... };
var columnMapping = new Dictionary<string, string>
{
    { "TableColumn", "CubeDimension" }
};

helper.LoadTableToCube_Custom(tableConfig, cubeConfig, columnMapping);
```

### Real-World Examples

#### Example 1: Annual Requirements (CMD_PGM Pattern)

```csharp
// Parse filters from UI/globals
FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api,
    out var statusFilters, out var appnFilters);

// Load requirements to cube
helper.LoadTableToCube_Framework(
    tablePrefix: "CMD_PGM",
    timeCalculation: "Annual",
    accounts: new List<string> { "Req_Funding", "Target" },
    statusFilters: statusFilters,
    dimensionFilters: appnFilters
);

// That's it! Framework handles:
// - SQL query generation
// - Entity hierarchy (base + parents)
// - Time mapping (Yearly column to FY)
// - Data clearing
// - Cube write
```

#### Example 2: Monthly Spend Plan (CMD_SPLN Pattern)

```csharp
FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api,
    out var statusFilters, out var appnFilters);

helper.LoadTableToCube_Framework(
    tablePrefix: "CMD_SPLN",
    timeCalculation: "Period",  // Monthly data
    accounts: new List<string> { "Commitments", "Obligations" },
    statusFilters: statusFilters,
    dimensionFilters: appnFilters,
    filterDimension: "UD3"  // SPLN uses UD3 for filtering
);

// Framework handles Month1-Month12 columns
```

#### Example 3: Custom Table Structure

```csharp
// Non-standard table structure
var tableConfig = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_Custom_Budget_Data",
    WhereClause = "Active = 1 AND Budget_Year = 2024"
};

var cubeConfig = new FMM_std_helpers.CubeCalcConfig
{
    TargetOrigin = "Import",
    ClearTargetData = true
};

// Map custom columns to dimensions
var columnMapping = new Dictionary<string, string>
{
    { "Org_Code", "Entity" },
    { "GL_Account", "Account" },
    { "Fund_Code", "UD1" },
    { "Dept_Code", "UD2" },
    { "Period", "Time" }
    // Amount column auto-detected
};

helper.LoadTableToCube_Custom(tableConfig, cubeConfig, columnMapping);
```

## Use Case 4: BRCubeToTable (Cube to Table)

### Overview

Extract cube data to custom tables for:

- Reporting and analytics
- External system integration
- Data archival
- Staging for further processing

### Configuration

```csharp
var config = new FMM_std_helpers.CubeToTableConfig
{
    SourceMemberScript = "E#POV:S#Actual:T#2024M1:2024M12:...",
    TargetTableName = "XFC_Extract_Table",
    DimensionToColumnMap = new Dictionary<string, string>
    {
        { "Entity", "Entity_Code" },
        { "Account", "Account_Code" },
        { "Time", "Period" }
    },
    AdditionalColumns = new List<string> { "Extract_Date" },
    ClearTargetTable = true,
    IncludeWorkflowInfo = true  // Adds Scenario, Time, Cube, User, Date
};
```

### Method

```csharp
helper.ExtractCubeToTable(config);
```

### Real-World Examples

#### Example 1: Extract Actuals for Reporting

```csharp
var config = new FMM_std_helpers.CubeToTableConfig
{
    SourceMemberScript = 
        "E#POV.Base.Descendants:S#Actual:T#2024M1:2024M12:" +
        "V#Periodic:A#Revenue,A#Expenses:F#Ending:O#Import",
    
    TargetTableName = "XFC_Actuals_Extract",
    
    DimensionToColumnMap = new Dictionary<string, string>
    {
        { "Entity", "Entity_Code" },
        { "Account", "Account_Code" },
        { "Time", "Period" },
        { "UD1", "Fund_Code" },
        { "UD2", "Dept_Code" }
    },
    
    ClearTargetTable = true,
    IncludeWorkflowInfo = true
};

helper.ExtractCubeToTable(config);

// Result table structure:
// Entity_Code, Account_Code, Period, Fund_Code, Dept_Code, Amount,
// WFScenario_Name, WFTime_Name, WFCube_Name, Create_Date, Create_User
```

#### Example 2: ERP Interface Export

```csharp
var config = new FMM_std_helpers.CubeToTableConfig
{
    SourceMemberScript =
        "E#POV:S#Forecast:T#2024M1:2024M12:" +
        "V#Periodic:A#Budget_Accounts.Base:F#Target:O#Import",
    
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

// Lean table for ERP: CostCenter, GLAccount, FiscalPeriod, FundSource, Amount
```

## Use Case 5: Consolidation

### Overview

Aggregate data from base entities to parent entities across the hierarchy. Two approaches:

1. **Framework Method (Recommended)**: Standard consolidation with flow filters
2. **Custom Method**: Complex business logic or transformations

### Framework Method (Recommended)

#### Configuration

```csharp
// Define flow filters by entity level
var levelFlowFilters = new Dictionary<string, List<string>>
{
    { "L2", new List<string> { "L2_Flow", "L3_Flow" } },
    { "L3", new List<string> { "L3_Flow", "L4_Flow" } }
};
```

#### Method

```csharp
helper.ConsolidateData_Framework(
    accounts: new List<string> { "Req_Funding" },
    levelFlowFilters: levelFlowFilters
);
```

### Custom Method

For complex transformations:

```csharp
var config = new FMM_std_helpers.CubeCalcConfig
{
    SourceMemberScript = "E#POV.Base.Descendants:...",
    DimensionMappings = new Dictionary<string, string>
    {
        { "Origin", "AdjConsolidated" }  // Transform origin at parent
    },
    ClearTargetData = true
};

helper.ConsolidateData_Custom(config);
```

### Real-World Examples

#### Example 1: Requirements Consolidation (CMD_PGM Pattern)

```csharp
// Define which flows are visible at each level
var levelFlowFilters = new Dictionary<string, List<string>>
{
    {
        "L2",  // L2 entities see L2, L3, L4 flows
        new List<string>
        {
            "L2_Formulate_PGM", "L3_Formulate_PGM", "L4_Formulate_PGM",
            "L2_Validate_PGM", "L3_Validate_PGM", "L4_Validate_PGM",
            "L2_Approve_PGM", "L2_Final_PGM"
        }
    },
    {
        "L3",  // L3 entities see L3, L4 flows
        new List<string>
        {
            "L3_Formulate_PGM", "L4_Formulate_PGM",
            "L3_Validate_PGM", "L4_Validate_PGM",
            "L3_Approve_PGM", "L2_Approve_PGM"
        }
    },
    {
        "L4",  // L4 entities see L4 flows only
        new List<string>
        {
            "L4_Formulate_PGM",
            "L4_Validate_PGM"
        }
    }
};

helper.ConsolidateData_Framework(
    accounts: new List<string> { "Req_Funding", "Target" },
    levelFlowFilters: levelFlowFilters
);
```

#### Example 2: Origin Transformation

```csharp
// Aggregate Import from base entities to AdjConsolidated at parent
var config = new FMM_std_helpers.CubeCalcConfig
{
    SourceMemberScript = 
        "E#POV.Base.Descendants:S#POV:T#POV:V#Periodic:" +
        "A#Req_Funding:F#Formulate_Flows:O#Import",
    
    DimensionMappings = new Dictionary<string, string>
    {
        { "Origin", "AdjConsolidated" }
    },
    
    ClearTargetData = true
};

helper.ConsolidateData_Custom(config);
```

## Best Practices

### 1. Use Framework Methods When Possible

Framework methods are optimized and battle-tested:

```csharp
// ✅ GOOD: Use framework method
helper.LoadTableToCube_Framework("CMD_PGM", "Annual", accounts);

// ❌ AVOID: Manual implementation unless necessary
```

### 2. Cache Configuration Objects

Reuse configurations in loops:

```csharp
// ✅ GOOD: Build once, use multiple times
var config = new FMM_std_helpers.CubeCalcConfig { ... };
globals.SetObject("MyConfig", config);

foreach (var entity in entities)
{
    var cachedConfig = globals.GetObject("MyConfig") as FMM_std_helpers.CubeCalcConfig;
    helper.WriteCubeData(data, cachedConfig);
}
```

### 3. Use Workflow Filters

Let the framework handle workflow filtering:

```csharp
// ✅ GOOD: Auto-adds Scenario, Time, Entity filters
var config = new FMM_std_helpers.TableOpConfig
{
    TableName = "XFC_Table",
    UseWorkflowFilters = true
};
```

### 4. Clear Before Writing

Prevent data accumulation:

```csharp
// ✅ GOOD: Clear target data
config.ClearTargetData = true;
```

### 5. Log Key Operations

Track execution for troubleshooting:

```csharp
BRApi.ErrorLog.LogMessage(si, $"Starting consolidation for {api.Pov.Entity.Name}");
helper.ConsolidateData_Framework(accounts);
BRApi.ErrorLog.LogMessage(si, "Consolidation completed");
```

## Performance Tips

### 1. Batch Operations

Process multiple entities/periods together:

```csharp
// ✅ GOOD: Single member script for multiple entities
SourceMemberScript = "E#Ent1,E#Ent2,E#Ent3:..."
```

### 2. Use SQL Aggregation

Let database aggregate when possible:

```csharp
// ✅ GOOD: Framework uses SUM in SQL
helper.LoadTableToCube_Framework(...)

// ❌ SLOW: Manual row-by-row aggregation
```

### 3. Minimize Cube Reads

Read once, process in memory:

```csharp
// ✅ GOOD: Single read
var data = helper.ReadCubeData(config);
// Process data...
helper.WriteCubeData(data, config);
```

### 4. Cache Rate Tables

Avoid repeated database reads:

```csharp
// ✅ GOOD: Cache in globals
if (globals.GetObject("RateTable") == null)
{
    var rates = ReadRatesFromDB();
    globals.SetObject("RateTable", rates);
}
```

## Troubleshooting

### Issue: No Data Loaded

**Check:**
1. Member script syntax
2. Filter conditions
3. Dimension members exist
4. Security/origin access

**Debug:**
```csharp
BRApi.ErrorLog.LogMessage(si, $"Source script: {config.SourceMemberScript}");
var data = helper.ReadCubeData(config);
BRApi.ErrorLog.LogMessage(si, $"Cells read: {data.DataBufferCells.Count}");
```

### Issue: Incorrect Results

**Check:**
1. Dimension mappings correct
2. Time calculation type
3. Multiplier factor
4. Clear data setting

### Issue: Performance Problems

**Solutions:**
1. Use framework methods
2. Enable caching
3. Batch operations
4. Check database indexes

## Migration from Legacy Code

### Before (Manual Implementation)

```csharp
// 200+ lines of complex VB/C# code
// Reading table, building SQL, iterating rows
// Manual dimension lookups, member IDs
// Manual cube writes, error handling
```

### After (Using std_helpers)

```csharp
// 15-20 lines of simple configuration
var helper = new FMM_std_helpers(si, globals, api, args);
helper.LoadTableToCube_Framework("CMD_PGM", "Annual", accounts);
```

### Migration Steps

1. **Identify Pattern**: Determine which of the 5 use cases applies
2. **Build Config**: Create appropriate configuration object
3. **Test**: Validate results match legacy implementation
4. **Replace**: Swap old code with helper method
5. **Verify**: Run in production with monitoring

## Support and Feedback

### Documentation Files

1. **FMM_std_helpers_Documentation.md** (this file) - Complete guide
2. **FMM_std_helpers_Usage_Examples.cs** - Code examples
3. **FMM_std_helpers.cs** - Core implementation

### Related Documentation

- **FMM_Table_Calc_Framework_Documentation.md** - Framework details
- **README_FMM_Table_Calc_Framework.md** - Quick start guide

## Appendix: Configuration Reference

### CubeCalcConfig

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| SourceMemberScript | string | Member script to read cube data | Required |
| TargetOrigin | string | Origin for written data | "Import" |
| AccountFilter | List<string> | Target accounts for operations | [] |
| FlowFilter | List<string> | Flow dimension filters | [] |
| DimensionMappings | Dictionary | Dimension transformations | {} |
| ClearTargetData | bool | Clear before writing | true |
| MultiplierFactor | decimal | Apply multiplier to amounts | 1.0 |

### TableOpConfig

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| TableName | string | Custom table name | Required |
| SelectColumns | List<string> | Columns to retrieve (empty = all) | [] |
| FilterConditions | Dictionary | WHERE conditions | {} |
| WhereClause | string | Additional SQL WHERE | "" |
| UseWorkflowFilters | bool | Add workflow filters | true |

### CubeToTableConfig

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| SourceMemberScript | string | Member script to read cube data | Required |
| TargetTableName | string | Destination table | Required |
| DimensionToColumnMap | Dictionary | Map dimensions to columns | Required |
| AdditionalColumns | List<string> | Extra columns to create | [] |
| ClearTargetTable | bool | Clear before writing | true |
| IncludeWorkflowInfo | bool | Add workflow metadata | true |

## Version History

- **v1.0** (2024-04) - Initial release with 5 core use cases
- Based on CMD SPLN patterns and FMM Table Calc Framework
