# FMM Table Calculation Framework

## Overview

The Financial Model Manager (FMM) Table Calculation Framework provides a configurable, performance-optimized approach for loading data from custom tables to the OneStream cube and aggregating data across entity hierarchies. This framework eliminates the need for hardcoded calculation logic and enables non-technical users to configure table calculations through simple configuration objects.

## Key Components

### 1. FMM_Table_Calc_Config
Configuration class for defining how data should be loaded from custom tables to the cube.

**Key Properties:**
- `SourceTable`: Name of the custom table (e.g., "XFC_CMD_PGM_REQ_Details")
- `TimeCalculation`: How to calculate time periods ("Annual", "Period", or "Fiscal_Year")
- `TimeColumnMapping`: Mapping of time columns in the source table
- `GroupByColumns`: Columns to group by in the SQL aggregation
- `FilterConditions`: WHERE clause conditions for filtering data
- `ColumnToDimensionMap`: Mapping of table columns to cube dimensions
- `TargetOrigin`: Origin dimension for loaded data
- `HandleParentEntities`: Whether to automatically handle entity hierarchy
- `ClearStaleData`: Whether to clear cube data not present in the table

### 2. FMM_Table_Aggregation_Config
Configuration class for consolidating data across entity hierarchies.

**Key Properties:**
- `AccountFilter`: List of accounts to aggregate
- `OriginFilter`: List of origins to include
- `FlowFilter`: List of flows to include
- `EntityLevelFlowFilters`: Different flow filters by entity level (L2, L3, L4, etc.)
- `SourceOriginForParents`: Origin to look for when aggregating to parents
- `DestOriginForParents`: Origin to write aggregated data to

### 3. FMM_Table_Calc_Engine
The execution engine that performs the actual data loading and aggregation operations.

**Key Methods:**
- `LoadTableDataToCube(config)`: Loads data from table to cube based on configuration
- `AggregateData(config)`: Aggregates data across entity hierarchies

### 4. FMM_Table_Calc_Builder
Helper class with factory methods to quickly build configurations for common scenarios.

**Key Methods:**
- `BuildRequirementsTableConfig()`: For standard requirements tables
- `BuildPeriodicTableConfig()`: For monthly/periodic data
- `BuildStandardAggregationConfig()`: For entity consolidation
- `BuildCustomSQLConfig()`: For custom scenarios
- `ParseGlobalFilters()`: Parses filters from BRGlobals

## Usage Examples

### Example 1: Load Annual Requirements Data (CMD PGM Pattern)

```csharp
// In your Finance Business Rule or Custom Calculate
public void Load_Requirements_Annual()
{
    // Parse dynamic filters from global variables
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
```

### Example 2: Load Periodic Data (CMD SPLN Pattern)

```csharp
public void Load_Requirements_Monthly()
{
    // Parse filters
    FMM_Table_Calc_Builder.ParseGlobalFilters(
        globals,
        api,
        out List<string> statusFilters,
        out List<string> dimensionFilters
    );

    // Build configuration for monthly data
    var config = FMM_Table_Calc_Builder.BuildPeriodicTableConfig(
        prefix: "CMD_SPLN",
        accounts: new List<string> { "Commitments", "Obligations" },
        statusFilters: statusFilters,
        dimensionFilters: dimensionFilters,
        filterDimension: "UD3"
    );

    // Execute
    var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
    engine.LoadTableDataToCube(config);
}
```

### Example 3: Aggregate/Consolidate Data

```csharp
public void Consolidate_Entity_Data()
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
```

### Example 4: Custom Configuration

```csharp
public void Load_Custom_Table()
{
    // Manual configuration for custom scenarios
    var config = new FMM_Table_Calc_Config
    {
        ConfigName = "CustomLoad",
        SourceTable = "XFC_Custom_Table",
        TimeCalculation = "Annual",
        HandleParentEntities = true,
        ClearStaleData = true
    };

    // Set dimension mapping
    config.SetStandardDimensionMapping();

    // Custom grouping
    config.GroupByColumns = new List<string> 
    { 
        "Entity", "Account", "Flow", "UD1", "UD2" 
    };

    // Custom filters
    config.FilterConditions = new List<string>
    {
        "Status = 'Active'",
        "Amount > 0"
    };

    // Execute
    var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
    engine.LoadTableDataToCube(config);
}
```

## Performance Optimizations

### 1. Entity Hierarchy Caching
The framework automatically caches entity hierarchy information in `BRGlobals` to avoid repeated lookups:
- Base vs. parent entity determination
- Child entity lists
- Entity level information

### 2. Efficient SQL Generation
- Uses SQL `GROUP BY` aggregation at the database level
- Includes `UNION ALL` for parent entity consolidation in a single query
- Parameterized queries prevent SQL injection

### 3. Smart Data Buffer Management
- Only processes non-zero amounts
- Removes processed cells from current cube buffer for efficient stale data detection
- Batches all writes to minimize cube operations

### 4. Minimal Cube Reads
- Single cube read per calculation using `RemoveZeros()` and `FilterMembers()`
- Targeted dimension filters reduce data volume

## Configuration Patterns by Use Case

### Annual Requirements Loading (PGM Pattern)
- **Time Calculation**: "Annual"
- **Time Mapping**: FY_1, FY_2, FY_3, FY_4, FY_5
- **Entity Handling**: Automatic base/parent distinction
- **Typical Accounts**: Req_Funding, Target
- **Filters**: Status (Flow), APPN (UD1)

### Monthly Spend Plans (SPLN Pattern)
- **Time Calculation**: "Period"
- **Time Mapping**: Month1-Month12, Yearly
- **Entity Handling**: Automatic base/parent distinction
- **Typical Accounts**: Commitments, Obligations, WH_Commitments, WH_Obligations
- **Filters**: Status (Flow), APPN (UD3)

### Entity Consolidation
- **Operation**: AggregateData
- **Level-Specific Filters**: Different flow filters per entity level (L2, L3, L4)
- **Origin Transformation**: AdjInput → AdjConsolidated for parents
- **Auto-Clear**: Removes stale consolidated data

## Migration from Hardcoded Patterns

### CMD PGM Load_Reqs_to_Cube → Framework

**Before (Hardcoded):**
```vb
Public Sub Load_Reqs_to_Cube()
    ' 237 lines of hardcoded VB.NET logic
    ' Manual SQL building
    ' Manual buffer management
    ' Repeated entity hierarchy logic
End Sub
```

**After (Configured):**
```csharp
public void Load_Reqs_to_Cube()
{
    FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api, 
        out var statusFilters, out var appnFilters);
    
    var config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
        "CMD_PGM", "Annual", 
        new List<string> { "Req_Funding" },
        statusFilters, appnFilters);
    
    new FMM_Table_Calc_Engine(si, globals, api, args)
        .LoadTableDataToCube(config);
}
```

**Benefits:**
- 90% reduction in code lines
- Reusable across different implementations
- Easier to test and maintain
- Configuration can be externalized to database tables if needed

## Best Practices

### 1. Use Builder Methods
Prefer `FMM_Table_Calc_Builder` factory methods over manual configuration:
- Reduces errors
- Ensures consistent patterns
- Easier to read and maintain

### 2. Cache Configurations
For repeated executions, cache configuration objects in `BRGlobals`:
```csharp
string cacheKey = "MyCalcConfig";
var config = globals.GetObject(cacheKey) as FMM_Table_Calc_Config;
if (config == null)
{
    config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(...);
    globals.SetObject(cacheKey, config);
}
```

### 3. Use Global Variables for Dynamic Filters
Store status and dimension filters in `BRGlobals` to enable dynamic configuration:
- Allows users to control filters without code changes
- Enables per-entity customization
- Follows existing CMD patterns

### 4. Test with Small Data Sets First
- Start with a single entity
- Verify SQL generation with logging
- Check cube data before scaling up

### 5. Consider Time Phase
- Annual calculations: Use "Annual" with FY_N columns
- Monthly/Quarterly: Use "Period" with MonthN or QuarterN columns
- Simple fiscal year: Use "Fiscal_Year" with Yearly column

## Error Handling

The framework automatically wraps all operations in try/catch blocks and throws `XFException` for OneStream error logging:

```csharp
try
{
    var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
    engine.LoadTableDataToCube(config);
}
catch (Exception ex)
{
    throw new XFException(si, ex);
}
```

All SQL errors, dimension not found errors, and data type errors will be properly logged to the OneStream error log.

## Future Enhancements

### Potential Database-Driven Configuration
The current in-code configuration could be extended to read from database tables:

```sql
-- Example: FMM_Table_Calc_Configs table
CREATE TABLE FMM_Table_Calc_Configs (
    Config_ID INT PRIMARY KEY,
    Config_Name NVARCHAR(100),
    Source_Table NVARCHAR(100),
    Time_Calculation NVARCHAR(20),
    Handle_Parent_Entities BIT,
    Clear_Stale_Data BIT,
    ...
)
```

This would enable:
- UI-based configuration
- Version control of configurations
- Easier migration between environments
- Non-technical user configuration

## Support and Troubleshooting

### Common Issues

**Issue**: Data not loading to cube
- **Check**: SQL query execution in database (add logging)
- **Check**: Dimension members exist in cube
- **Check**: Origin/View security allows write

**Issue**: Stale data not clearing
- **Check**: `ClearStaleData = true` in configuration
- **Check**: Filter in `OriginFilter` matches actual data origin

**Issue**: Parent entity data incorrect
- **Check**: `HandleParentEntities = true`
- **Check**: Entity hierarchy properly defined in dimension
- **Check**: Base vs. parent entity detection logic

### Debugging Tips

1. Enable SQL logging:
```csharp
var sql = BuildSQLQuery(config, entityInfo);
BRApi.ErrorLog.LogMessage(si, $"SQL Query: {sql}");
```

2. Log buffer counts:
```csharp
BRApi.ErrorLog.LogMessage(si, $"Dest buffer cells: {destBuffer.DataBufferCells.Count}");
BRApi.ErrorLog.LogMessage(si, $"Clear buffer cells: {currCubeBuffer.DataBufferCells.Count}");
```

3. Use `.LogDataBuffer()` for detailed inspection:
```csharp
destBuffer.LogDataBuffer(api, "Destination Buffer", 100);
```

## Conclusion

The FMM Table Calculation Framework provides a modern, configurable approach to table-based calculations in OneStream. By replacing hardcoded patterns with configuration-driven logic, it:

- **Improves Performance**: Optimized SQL, caching, and buffer management
- **Reduces Complexity**: 90% less code for common scenarios
- **Enhances Maintainability**: Single framework vs. multiple hardcoded implementations
- **Enables Flexibility**: Easy to configure new scenarios without code changes
- **Supports Non-Technical Users**: Builder methods and clear configuration options

The framework is production-ready and can be adopted incrementally, allowing existing implementations to be migrated over time while new implementations can use it immediately.
