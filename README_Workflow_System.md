# Configurable Workflow System

## Problem Statement

The requirement was to build a configurable workflow system that can:
1. **Differentiate between Cube and Table data sources**
2. **Determine approval levels** based on:
   - **Columns in tables** (for relational data)
   - **Dimensions in cubes** (for OLAP data)
3. Support **derived approval levels** using:
   - Direct column values
   - Calculated/derived values
   - Lookup values from related tables
   - Dimension hierarchy levels

## Solution Overview

This implementation provides a complete, production-ready configurable workflow system with the following components:

### 1. Database Schema (`scripts/Workflow_Config_Table_Schema.sql`)

Five main configuration tables that support both Cube and Table workflows:

- **WF_Config**: Master workflow configuration
- **WF_Data_Source_Config**: Differentiates between Cube and Table sources
- **WF_Step_Config**: Workflow steps and approval requirements
- **WF_Approval_Level_Column**: Table-based approval levels (Direct, Derived, Lookup)
- **WF_Approval_Level_Dimension**: Cube-based approval levels (with hierarchy support)

Includes:
- Sample data for both Cube and Table workflows
- Views for easy querying
- Comprehensive constraints and validation

### 2. Helper Class (`GBL_Workflow_Config_Helper.cs`)

Core business logic for workflow management:

```csharp
// Differentiate between Cube and Table
bool isCube = helper.IsCubeDataSource(dataSourceConfigId);
bool isTable = helper.IsTableDataSource(dataSourceConfigId);

// Derive approval levels for tables
var tableApprovals = helper.DeriveTableApprovalLevels(stepConfigId, dataSourceConfigId, dataRow);

// Derive approval levels for cubes
var cubeApprovals = helper.DeriveCubeApprovalLevels(stepConfigId, dataSourceConfigId, memberSelections);
```

Key features:
- Automatic Cube vs Table detection
- Support for Direct, Derived, and Lookup columns
- Dimension hierarchy traversal
- Parent level aggregation for cubes
- Configuration validation

### 3. SQL Adapter (`SQA_GBL_Workflow_Config.cs`)

Data access layer following OneStream patterns:
- Direct database access using SqlConnection
- Fill methods for all workflow tables
- Parameter mapping for insert/update operations
- Consistent with existing SQL adapters in the repository

### 4. Documentation (`docs/Configurable_Workflow_System_Guide.md`)

Comprehensive guide including:
- Architecture overview
- Detailed usage examples for both Cube and Table workflows
- API reference for all helper methods
- Best practices and performance considerations

## Key Capabilities

### Table-Based Workflows

Supports three column types for approval level derivation:

1. **Direct**: Use column value as-is
   ```sql
   Column_Type = 'Direct', Column_Name = 'Department'
   → Routes to department manager
   ```

2. **Derived**: Apply business logic or calculations
   ```sql
   Column_Type = 'Derived', 
   Derivation_Logic = 'CASE WHEN Amount > 10000 THEN "VP" ELSE "Manager" END'
   → Routes based on amount threshold
   ```

3. **Lookup**: Join to related tables
   ```sql
   Column_Type = 'Lookup',
   Lookup_Table = 'Department_Master',
   Lookup_Column = 'Manager_Name',
   Lookup_Key_Column = 'Department_ID'
   → Looks up manager for each department
   ```

### Cube-Based Workflows

Supports dimension hierarchy navigation:

```sql
Dimension_Name = 'Entity',
Hierarchy_Name = 'Default',
Aggregate_To_Level = 'Level2'
→ Aggregates to regional level in entity hierarchy
```

Features:
- Hierarchy traversal to find parent at target level
- Support for multiple dimensions per step
- Member filtering
- Property-based approval assignment

## Usage Examples

### Example 1: Table with Direct Column

```sql
-- Budget approval at department level
INSERT INTO WF_Approval_Level_Column (
    Step_Config_ID, Data_Source_Config_ID,
    Column_Name, Column_Type, Level_Order
)
VALUES (@StepID, @DSID, 'Department', 'Direct', 1);
```

### Example 2: Cube with Dimension Hierarchy

```sql
-- Forecast approval at regional level
INSERT INTO WF_Approval_Level_Dimension (
    Step_Config_ID, Data_Source_Config_ID,
    Dimension_Name, Aggregate_To_Level, Level_Order
)
VALUES (@StepID, @DSID, 'Entity', 'Level2', 1);
```

## Integration with OneStream

This solution follows OneStream XF patterns:

- ✅ Uses `SessionInfo` and `BRApi` throughout
- ✅ Follows naming conventions (GBL_, SQA_ prefixes)
- ✅ Uses XFException for error handling
- ✅ Leverages BRApi.Database for data access
- ✅ Uses BRApi.Finance for dimension/member access
- ✅ Maintains placeholder namespaces
- ✅ Consistent with existing SQL adapters

## Files Created

1. **scripts/Workflow_Config_Table_Schema.sql** (456 lines)
   - Complete database schema with constraints
   - Sample data for demonstration
   - Views for querying

2. **Assemblies/.../GBL_Workflow_Config_Helper.cs** (675 lines)
   - Core workflow helper class
   - Cube/Table differentiation
   - Approval level derivation logic

3. **Assemblies/.../SQA_GBL_Workflow_Config.cs** (423 lines)
   - SQL adapter for direct DB access
   - Fill methods for all tables
   - Parameter mappings

4. **docs/Configurable_Workflow_System_Guide.md** (360 lines)
   - Comprehensive documentation
   - Usage examples
   - Best practices

## Testing

The system includes:
- Sample workflow configurations in the SQL schema
- Configuration validation method
- Built-in error handling
- Inline documentation

To test:
1. Run `Workflow_Config_Table_Schema.sql` in OneStream database
2. Use sample data or create custom workflows
3. Call helper methods from business rules
4. Validate configurations before deployment

## Next Steps

1. **Deploy Database**: Import SQL schema into OneStream application database
2. **Import Code**: Use OneStream VS Code extension to import assemblies
3. **Configure Workflows**: Set up table or cube-based workflows
4. **Test**: Validate with sample data
5. **Integrate**: Call from dashboards or business rules

## Benefits

✅ **Flexible**: Supports both Cube and Table data sources
✅ **Configurable**: No code changes needed to add new workflows
✅ **Extensible**: Easy to add new column types or dimension logic
✅ **Validated**: Built-in configuration validation
✅ **Documented**: Comprehensive documentation and examples
✅ **Production-Ready**: Follows OneStream patterns and best practices

## Support

For detailed documentation, see:
- [Configurable Workflow System Guide](docs/Configurable_Workflow_System_Guide.md)
- Inline code documentation in C# files
- SQL schema comments
