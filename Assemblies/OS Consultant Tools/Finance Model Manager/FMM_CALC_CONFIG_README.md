# FMM Calculation Configuration Tables

## Overview

This document describes the FMM (Finance Model Manager) calculation configuration system consisting of three core tables: **FMM_Calc_Config**, **FMM_Dest_Cell**, and **FMM_Src_Cell**. These tables support a flexible calculation engine that handles multiple calculation types including cube calculations, table calculations, and cross-system data movements.

## Supported Calculation Types

The system supports six distinct calculation types as defined in `FMM_Config_Helpers.cs`:

| Calc_Type | Name | Description |
|-----------|------|-------------|
| 1 | **Table** | Table-based calculations operating on relational data |
| 2 | **Cube** | Cube-based calculations with dimensional operations |
| 3 | **BRTabletoCube** | Business Rule driven table to cube transformations |
| 4 | **ImportTabletoCube** | Import operations from tables to cubes |
| 5 | **CubetoTable** | Export operations from cubes to tables |
| 6 | **Consolidate** | Consolidation calculations across hierarchical dimensions |

## Table Structure

### 1. FMM_Calc_Config (Core Calculation Configuration)

**Purpose**: Stores the core calculation metadata and configuration for all calculation types.

**Key Fields**:
- `Calc_ID` (PK): Unique identifier for the calculation
- `Cube_ID`, `Act_ID`, `Model_ID`: Context identifiers linking to cube, activity, and model
- `Name`: Human-readable calculation name
- `Sequence`: Execution order for the calculation
- `Calc_Type`: Calculation type (1-6, see table above)
- `Calc_Condition`: Optional condition determining when the calculation runs
- `Calc_Explanation`: Documentation and explanation of the calculation logic

**Calculation-Type Specific Fields**:

**Table Calculations (Calc_Type = 1)**:
- `Table_Calc_SQL_Logic`: SQL logic for table-based operations
- `MbrList_*` fields: Member list configuration for dimensional filtering across up to 4 dimensions

**Cube Calculations (Calc_Type = 2)**:
- `Balanced_Buffer`, `bal_buffer_calc`: Buffer management for balanced calculations
- `Unbal_Calc`: Logic for unbalanced calculations
- `MultiDim_Alloc`: Enable multi-dimensional allocation

**All Types**:
- `BR_Calc`, `BR_Calc_Name`: Business Rule integration options
- `Time_Phasing`, `Input_Frequency`: Time management settings
- `Status`: Build, Active, Inactive, Archived

**Indexes**:
- `IX_FMM_Calc_Config_Cube_Act_Model`: Efficient lookup by context and sequence
- `IX_FMM_Calc_Config_Type`: Lookup by calculation type

### 2. FMM_Dest_Cell (Destination Cell Configuration)

**Purpose**: Defines where calculation results are written - either to cube cells (with dimensional coordinates) or table columns.

**Key Fields**:
- `Dest_Cell_ID` (PK): Unique identifier
- `Calc_ID` (FK): Links to parent calculation
- `Location`: Destination location identifier
- `Calc_Plan_Units`: Planning units specification

**Cube Destination Fields** (for Calc_Type = 2, 3, 4, 6):
- Dimension fields: `Cons`, `View`, `Acct`, `Flow`, `IC`, `Origin`, `UD1-UD8`
- Dimension filters: `Time_Filter`, `Acct_Filter`, `Origin_Filter`, `IC_Filter`, `Flow_Filter`, `UD1_Filter-UD8_Filter`
- Additional filters: `Conditional_Filter`, `Curr_Cube_Buffer_Filter`, `Buffer_Filter`
- `Dest_Cell_Logic`: Custom destination logic

**Table Destination Fields** (for Calc_Type = 1, 5):
- `SQL_Logic`: SQL statement or expression for table destinations

**Indexes**:
- `IX_FMM_Dest_Cell_Calc`: Fast lookup by calculation
- `IX_FMM_Dest_Cell_Cube_Act_Model`: Context-based lookup

### 3. FMM_Src_Cell (Source Cell Configuration)

**Purpose**: Defines source data for calculations. Multiple source cells can be configured per calculation, each representing a data source in a mathematical expression.

**Key Fields**:
- `Cell_ID` (PK): Unique identifier
- `Calc_ID` (FK): Links to parent calculation
- `Src_Order`: Order in the calculation expression (1, 2, 3, ...)
- `Src_Type`: Type of source (Cube, Table, Constant, etc.)
- `Src_Item`: Source item identifier

**Mathematical Expression**:
- `Open_Parens`, `Close_Parens`: Grouping for complex expressions
- `Math_Operator`: Mathematical operator (+, -, *, /)

**Cube Source Fields** (for Calc_Type = 2, 3, 4, 6):
- Dimension fields: `Entity`, `Cons`, `Scenario`, `Time`, `View`, `Acct`, `Flow`, `IC`, `Origin`, `UD1-UD8`
- `Src_Plan_Units`: Source planning units
- `DB_Name`: Database name for cross-database queries

**Table Source Fields** (for Calc_Type = 1, 5):
- `Table_Calc_Expression`: Table calculation expression
- `Table_Join_Expression`: JOIN clauses
- `Table_Filter_Expression`: WHERE clause filters
- `Src_SQL_Stmt`: Complete SQL statement
- `Use_Temp_Table`, `Temp_Table_Name`: Temporary table support

**Unbalanced Calculation Overrides**:
- `Unbal_Src_Cell_Buffer`: Buffer for unbalanced source
- Dimension overrides: `Unbal_Origin_Override`, `Unbal_IC_Override`, `Unbal_Acct_Override`, `Unbal_Flow_Override`, `Unbal_UD1_Override-UD8_Override`
- `Unbal_Src_Cell_Buffer_Filter`: Filter for unbalanced buffer

**Mapping Configuration** (for Calc_Type = 3, 4):
- `Map_Type`: Mapping type (Direct, Transform, Lookup)
- `Map_Source`: Source for mapping
- `Map_Logic`: Mapping transformation logic

**Advanced Options**:
- `Dyn_Calc_Script`: Dynamic calculation script
- `Override_Value`: Override value for calculations

**Indexes**:
- `IX_FMM_Src_Cell_Calc`: Fast lookup by calculation and order
- `IX_FMM_Src_Cell_Cube_Act_Model`: Context-based lookup

## Helper Views

Four views are provided for easier data querying:

### vw_FMM_Calc_Complete
Complete calculation overview with destination and source summary.

```sql
SELECT * FROM vw_FMM_Calc_Complete
WHERE Status = 'Active'
ORDER BY Sequence;
```

### vw_FMM_Src_Cell_Details
Source cell details with calculation context.

```sql
SELECT * FROM vw_FMM_Src_Cell_Details
WHERE Calc_Name = 'Revenue Allocation'
ORDER BY Src_Order;
```

### vw_FMM_Dest_Cell_Details
Destination cell details with calculation context.

```sql
SELECT * FROM vw_FMM_Dest_Cell_Details
WHERE Calc_Type = 2;
```

### vw_FMM_Calc_Summary_By_Type
Calculation statistics grouped by type and status.

```sql
SELECT * FROM vw_FMM_Calc_Summary_By_Type;
```

## Usage Examples

### Example 1: Cube Calculation (Revenue Allocation)

This example shows a cube-based calculation that allocates revenue across entities based on headcount.

**Configuration**:
- **Calc_Type**: 2 (Cube)
- **Destination**: Revenue account with consolidated view
- **Sources**: 
  1. Total revenue at parent entity
  2. Headcount at child entities (driver)
  3. Total headcount (for percentage calculation)

**Formula**: `(Total Revenue) * (Entity Headcount) / (Total Headcount)`

### Example 2: Table Calculation (Budget Totals)

This example shows a table-based calculation that sums monthly budget columns.

**Configuration**:
- **Calc_Type**: 1 (Table)
- **Destination**: Budget_Plan.Total column
- **Sources**: Month1 through Month12 columns
- **Logic**: SQL UPDATE statement summing monthly values

### Example 3: Table to Cube Import

This example imports budget data from a relational table into the OneStream cube.

**Configuration**:
- **Calc_Type**: 3 (BRTabletoCube) or 4 (ImportTabletoCube)
- **Destination**: Cube with specific dimensions and filters
- **Source**: Budget_Plan table with mapping configuration
- **Mapping**: Maps table columns to cube dimensions

### Example 4: Cube to Table Export

This example exports actual data from the cube to a reporting table.

**Configuration**:
- **Calc_Type**: 5 (CubetoTable)
- **Destination**: Actuals_Report table
- **Source**: Cube data with dimensional filters
- **Logic**: INSERT statement to populate table

### Example 5: Consolidation

This example consolidates child entities to parent entities.

**Configuration**:
- **Calc_Type**: 6 (Consolidate)
- **Destination**: Parent entity cells
- **Source**: Child entity cells using dimensional formulas
- **Logic**: Sum of children flows to parent

### Example 6: Multi-Dimensional Allocation

This example shows a complex allocation across multiple dimensions (Entity and Department).

**Configuration**:
- **Calc_Type**: 2 (Cube)
- **MultiDim_Alloc**: Enabled
- **Destination**: Overhead_Allocated account across entities and departments
- **Sources**: 
  1. Total overhead pool
  2. Department-specific headcount (driver)
  3. Total headcount (denominator)

## Integration with FMM_Config_Helpers.cs

The `FMM_Config_Helpers.cs` class provides helper functions for parameter mapping between UI components and database columns:

### CalcRegistry
Maps calculation type parameters to FMM_Calc_Config columns:
- `CalcRegistry.Configs[CalcType.Table]`: Table calculation mappings
- `CalcRegistry.Configs[CalcType.Cube]`: Cube calculation mappings
- etc.

### DestRegistry
Maps destination parameters to FMM_Dest_Cell columns.

### SrcRegistry
Maps source parameters to FMM_Src_Cell columns.

### Helper Methods
- `Get_CalcConfigType(int)`: Retrieves configuration for a calc type
- `Get_DestConfigType(int)`: Retrieves destination configuration
- `Get_SrcConfigType(int)`: Retrieves source configuration
- `GetEnabledSrcProperties(int)`: Returns enabled source properties for a calc type
- `GetEnabledDestProperties(int)`: Returns enabled destination properties for a calc type

## Data Flow

1. **Configuration Phase**:
   - User configures calculation in FMM_Calc_Config
   - Destination cell defined in FMM_Dest_Cell
   - One or more source cells configured in FMM_Src_Cell

2. **Execution Phase**:
   - System reads FMM_Calc_Config to determine calculation type and sequence
   - For each calculation:
     - Read destination from FMM_Dest_Cell
     - Read sources from FMM_Src_Cell (ordered by Src_Order)
     - Execute calculation based on Calc_Type
     - Write results to destination

3. **Calculation Types Execution**:
   - **Table (1)**: Execute SQL logic against relational tables
   - **Cube (2)**: Perform dimensional calculations in cube
   - **BRTabletoCube (3)**: Execute BR to transform table → cube
   - **ImportTabletoCube (4)**: Import table data → cube with mapping
   - **CubetoTable (5)**: Export cube data → table
   - **Consolidate (6)**: Roll up child entities to parents

## Best Practices

1. **Calculation Sequencing**: Use `Sequence` field to control execution order. Dependencies should be ordered sequentially.

2. **Status Management**: Use status field to control which calculations are active:
   - `Build`: Under development
   - `Active`: Enabled for execution
   - `Inactive`: Temporarily disabled
   - `Archived`: Historical record

3. **Filters and Conditions**: Use `Calc_Condition` to control when calculations run (e.g., "Entity <> None").

4. **Multi-Source Calculations**: For complex formulas, use multiple source cells with appropriate `Math_Operator` values:
   - Source 1: (no operator) - first operand
   - Source 2: `+` - add second operand
   - Source 3: `/` - divide by third operand

5. **Dimensional Wildcards**: Use POV (Point of View) references:
   - `E#POV`: Current entity
   - `T#POV`: Current time period
   - `A#POV`: Current account
   - `E#[All]`: All entities
   - `E#[ParentOf(E#POV)]`: Children of current entity

6. **Performance Optimization**:
   - Use filters (`Time_Filter`, `Acct_Filter`, etc.) to limit calculation scope
   - For table operations, leverage indexes and consider temp tables for complex queries
   - Use `MbrList_Filter` to restrict dimensional calculations

## Schema Files

- **FMM_Calc_Config_Schema.sql**: Complete table definitions, indexes, and views
- **FMM_Calc_Config_Sample_Data.sql**: Example data for all calculation types

## Testing

After deployment, verify the schema with these queries:

```sql
-- Check table structure
SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME IN ('FMM_Calc_Config', 'FMM_Dest_Cell', 'FMM_Src_Cell')
ORDER BY TABLE_NAME, ORDINAL_POSITION;

-- Check indexes
SELECT 
    i.name AS IndexName,
    t.name AS TableName,
    STRING_AGG(c.name, ', ') AS Columns
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
WHERE t.name IN ('FMM_Calc_Config', 'FMM_Dest_Cell', 'FMM_Src_Cell')
GROUP BY i.name, t.name
ORDER BY t.name, i.name;

-- Check views
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_NAME LIKE 'vw_FMM_%';
```

## Migration from Existing Systems

If migrating from an existing FMM implementation:

1. Back up existing calculation configurations
2. Map existing calculation definitions to new table structure
3. Identify calculation type for each existing calculation
4. Populate FMM_Calc_Config with metadata
5. Populate FMM_Dest_Cell with destination specifications
6. Populate FMM_Src_Cell with source specifications (may require splitting complex sources)
7. Test each calculation type independently
8. Validate calculation results against expected values

## Troubleshooting

### Issue: Calculation not executing
- Check `Status` field is 'Active'
- Verify `Calc_Condition` evaluates to true
- Check `Sequence` for execution order conflicts

### Issue: Incorrect calculation results
- Verify source cell `Src_Order` is correct
- Check `Math_Operator` values
- Review dimension filters on destination and sources
- Validate POV references resolve correctly

### Issue: Performance problems
- Add indexes on frequently filtered columns
- Use temp tables for complex table calculations
- Restrict calculation scope with filters
- Review `Calc_Condition` to avoid unnecessary executions

## Future Enhancements

Potential future enhancements to consider:

1. **Versioning**: Add version control for calculation configurations
2. **Dependencies**: Track calculation dependencies explicitly
3. **Audit Trail**: Enhanced audit logging for calculation executions
4. **Error Handling**: Dedicated error logging table for failed calculations
5. **Calculation Groups**: Group related calculations for batch execution
6. **Parameter Sets**: Reusable parameter configurations
7. **Testing Framework**: Built-in testing and validation capabilities

## References

- `FMM_Config_Helpers.cs`: Helper class with CalcType enum and parameter mappings
- `FMM_Config_Data.cs`: Dashboard extender with save operations
- `FMM_Table_Config_DDL.sql`: Related FMM table configuration system
- `FMM_Validation_Table_Schema.sql`: Validation framework for FMM
