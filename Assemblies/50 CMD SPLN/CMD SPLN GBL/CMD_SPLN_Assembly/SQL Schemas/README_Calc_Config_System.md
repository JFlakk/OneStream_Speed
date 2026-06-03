# CMD SPLN Cube-to-Table Calculation Configuration System

## Overview

This document describes a **table-driven configuration system** for making cube-to-table calculations configurable in the CMD SPLN application. The design is inspired by the Finance Model Manager (FMM) `Src_Cell` and `Dest_Cell` structures and models the existing `Copy_CivPay` and `Copy_Withhold` calculations found in `CMD_SPLN_FinCustCalc.vb`.

## Problem Statement

Currently, calculations like `Copy_CivPay` and `Copy_Withhold` are **hardcoded** in the `CMD_SPLN_FinCustCalc` business rule. Each calculation:
- Reads cube data using specific filters
- Applies spread rates from FDX tables
- Maps dimension values to table columns
- Writes to REQ and REQ_Details tables

This approach requires **code changes** every time a new calculation variant is needed. The proposed solution makes these calculations **table-driven and configurable**.

## Solution Architecture

### Three-Table Structure

The solution uses three interconnected configuration tables:

1. **`XFC_CMD_SPLN_Calc_Config`** (Calculation Header)
   - Defines the overall calculation
   - Source cube/formula template
   - Destination tables
   - Spread rate configuration
   - REQ ID generation rules

2. **`XFC_CMD_SPLN_Calc_Src_Cell`** (Source Cell Mapping)
   - Defines which cube cells to read
   - Dimension filters (Entity, Account, UD1-UD8, etc.)
   - Math operations and aggregations
   - Inspired by FMM's `FMM_Src_Cell`

3. **`XFC_CMD_SPLN_Calc_Dest_Cell`** (Destination Cell/Table Mapping)
   - Defines how to map source data to destination columns
   - Transformation rules
   - Conditional logic
   - Time spread distribution
   - Inspired by destination mapping patterns

### Key Concepts

#### Placeholders in Templates

Configuration uses **runtime placeholders** that get replaced during execution:

- `{Entity}` - Current POV entity
- `{Scenario}` - Current scenario
- `{TimeName}` - Current workflow time
- `{CMDName}` - CMD name from cube
- `{SourcePosition}` - Source scenario for reading cube data
- `{TargetScenario}` - Target scenario for writing data
- `{TargetYear}` - Target fiscal year
- `{EntityLevel}` - Entity hierarchy level (e.g., "CMD", "MSC", "BDE")
- `{UD1}`, `{UD2}`, etc. - Current dimension values from iteration

#### Source Types

**Src_Type** in `XFC_CMD_SPLN_Calc_Src_Cell`:
- `CubeCell` - Read from cube using dimension filters
- `TableRow` - Read from custom table
- `Constant` - Use a constant value
- `Formula` - Use a formula expression

#### Destination Source Types

**Source_Type** in `XFC_CMD_SPLN_Calc_Dest_Cell`:
- `Dimension` - Map from a source dimension (e.g., "UD1", "Account")
- `Constant` - Use a constant value (e.g., "None", "CivPay_Copied")
- `Formula` - Use a formula with placeholders (e.g., "CivPay - {Entity} - {UD1}")
- `SpreadRate` - Apply spread rate calculation with time distribution
- `Generated` - Auto-generate value (GUID, DateTime, UserName, REQ_ID)

#### Transform Types

**Transform_Type** in `XFC_CMD_SPLN_Calc_Dest_Cell`:
- `None` - No transformation
- `Append` - Append text to source (e.g., "Commit" → "Commitments")
- `Replace` - Replace text patterns (e.g., "Pay_Benefits,Pay_General→Pay_General")
- `Lookup` - Look up value in another table
- `Calculate` - Perform calculation

## Table Schemas

See `XFC_CMD_SPLN_Calc_Config_DDL.sql` for complete DDL.

### XFC_CMD_SPLN_Calc_Config

**Key Columns:**
- `Calc_Name` - Unique calculation name (e.g., "Copy_CivPay")
- `Calc_Type` - Type of calculation ("CubeToTable", "TableToCube", "Transform")
- `Source_Formula_Template` - Template for building cube formulas
- `Dest_Table_Header` - Header table name (e.g., "XFC_CMD_SPLN_REQ")
- `Dest_Table_Detail` - Detail table name (e.g., "XFC_CMD_SPLN_REQ_Details")
- `Use_Spread_Rates` - Whether to apply spread rates
- `Spread_Rate_FDX_Name` - Name of FDX table with spread rates
- `REQ_ID_Type` - Type for REQ_ID_Type column (e.g., "CivPay_Copied", "Withhold")
- `Status_Level_Template` - Template for status (e.g., "{EntityLevel}_Formulate_SPLN")

### XFC_CMD_SPLN_Calc_Src_Cell

**Key Columns:**
- `Calc_Config_ID` - Foreign key to parent config
- `Src_Order` - Order of source cells
- `Src_Type` - Type of source ("CubeCell", "TableRow", etc.)
- `Entity`, `Scenario`, `Time`, `Account`, `UD1`-`UD8` - Dimension filters
  - `NULL` means "any value" (will iterate)
  - Specific value means "filter to this"
  - `{POV}` means "use current POV value"
- `Additional_Filter` - Advanced filter (e.g., "RemoveZeros")
- `Group_By_Dimensions` - Dimensions to group by (e.g., "UD1,UD2,UD3,UD4,UD5")
- `Aggregation_Method` - How to aggregate ("SUM", "AVG", etc.)

### XFC_CMD_SPLN_Calc_Dest_Cell

**Key Columns:**
- `Calc_Config_ID` - Foreign key to parent config
- `Dest_Table_Name` - Target table ("XFC_CMD_SPLN_REQ" or "XFC_CMD_SPLN_REQ_Details")
- `Dest_Table_Type` - "Header" or "Detail"
- `Dest_Column_Name` - Target column name
- `Source_Type` - Where value comes from ("Dimension", "Constant", "Formula", etc.)
- `Source_Value` - The value/expression
- `Transform_Type` - Transformation to apply
- `Transform_Rule` - Transformation details
- `Apply_Time_Spread` - Whether to apply monthly/quarterly spread
- `Time_Column_Pattern` - Pattern for time columns (e.g., "Month{1-12}")

## Configuration Examples

### Example 1: Copy_CivPay

See `XFC_CMD_SPLN_Calc_Config_Sample_CivPay.sql` for complete example.

**What it does:**
1. Reads cube data: Account#Target, U6#Pay_Benefits
2. Groups by UD1 (FundCode), UD2 (MDEP), UD3 (APE9), UD4 (Dollar_Type), UD5 (Ctype)
3. Creates REQ header per unique combination
4. Creates detail records for Commitments and Obligations
5. Applies APPN-specific spread rates from `CMD_SPLN_APPN_SpreadPct_FDX_CV`
6. Distributes amounts across months (and optionally next year if Time13 > 0)

**Key Configuration:**
- **Source Formula Template:**
  ```
  FilterMembers(RemoveZeros(
    E#{Entity}:S#{SourcePosition}:T#{TargetYear}:C#Aggregated:V#Periodic:
    F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top
  ),[A#Target])
  ```

- **Header Mappings:**
  - APPN ← UD1 (dimension)
  - MDEP ← UD2 (dimension)
  - APE9 ← UD3 (dimension)
  - Dollar_Type ← UD4 (dimension)
  - Ctype ← UD5 (dimension)
  - Obj_Class ← "Pay_General" (constant)
  - Title ← "CivPay - {Entity} - {UD1}-{UD2}-{UD3}-{UD4}-{UD5}" (formula)

- **Detail Mappings:**
  - Account ← Conditional: "Commitments" if Commit, "Obligations" if Obligation
  - UD6 ← "Pay_General" (constant)
  - Month1-12 ← `{CellAmount} * {SpreadRate_Time{N}} / 100` (spread rate)
  - Yearly ← Carryover calculation if Time13 > 0

### Example 2: Copy_Withhold

See `XFC_CMD_SPLN_Calc_Config_Sample_Withhold.sql` for complete example.

**What it does:**
1. Reads cube data: Account#TGT_WH (withhold accounts)
2. Extracts APPN from UD3 (splits on underscore)
3. Creates simplified REQ header (APPN only, other fields = "None")
4. Creates detail records for WH_Commitments and WH_Obligations
5. Applies generic withhold spread rates from `CMD_SPLN_WH_SpreadPct_FDX_CV`
6. Transforms UD6: Pay_Benefits/Pay_General → Pay_General, others → NonPay_General
7. Distributes amounts across months (no carryover year)

**Key Differences from CivPay:**
- Simpler header (only APPN populated, rest = "None")
- Account prefix: WH_Commitments / WH_Obligations
- UD6 transformation logic
- No Time13 carryover
- Different source formula (Cb#{CMDName}, I#None, U8#None)

## How to Configure New Calculations

### Step 1: Understand the Calculation Logic

1. What cube data needs to be read?
   - Which dimensions to filter?
   - Which dimensions to iterate?
   - Any special filters (RemoveZeros, FilterMembers)?

2. What tables to write to?
   - Header table? Detail table?
   - What uniquely identifies a header record?

3. Are spread rates needed?
   - From which FDX table?
   - How to match spread rates to source data?

4. What transformations are needed?
   - Dimension mappings
   - Constant values
   - Conditional logic
   - Time distributions

### Step 2: Create Calc_Config Record

```sql
INSERT INTO XFC_CMD_SPLN_Calc_Config (
    Calc_Name,
    Calc_Type,
    Description,
    Source_Formula_Template,
    Dest_Table_Header,
    Dest_Table_Detail,
    Use_Spread_Rates,
    Spread_Rate_FDX_Name,
    REQ_ID_Type,
    Status_Level_Template,
    Is_Active
) VALUES (
    'My_New_Calc',
    'CubeToTable',
    'Description of what this calc does',
    'FilterMembers(RemoveZeros(...)),[filters]',
    'XFC_CMD_SPLN_REQ',
    'XFC_CMD_SPLN_REQ_Details',
    1,
    'My_SpreadRate_FDX',
    'MyCalcType',
    '{EntityLevel}_Formulate_SPLN',
    1
);
```

### Step 3: Create Src_Cell Record(s)

```sql
INSERT INTO XFC_CMD_SPLN_Calc_Src_Cell (
    Calc_Config_ID,
    Src_Order,
    Src_Type,
    Src_Item,
    Account,
    UD6,
    Group_By_Dimensions,
    Aggregation_Method,
    Is_Active
) VALUES (
    @CalcConfigID,
    1,
    'CubeCell',
    'MySourceData',
    'MyAccountFilter',
    'MyUD6Filter',
    'UD1,UD2,UD3',
    'SUM',
    1
);
```

**Tips:**
- Set dimension to `NULL` if you want to iterate all values
- Set dimension to `{POV}` to use current POV value
- Set dimension to specific value to filter
- Use `Additional_Filter` for complex filters

### Step 4: Create Dest_Cell Records for Header

Map each column in the header table:

```sql
-- Example: Constant value
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Data_Type, Is_Required, Is_Active
) VALUES (
    @CalcConfigID, 'XFC_CMD_SPLN_REQ', 'Header', 'Obj_Class',
    'Constant', 'MyObjClass', 'NVARCHAR', 1, 1
);

-- Example: Dimension mapping
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Data_Type, Is_Required, Is_Active
) VALUES (
    @CalcConfigID, 'XFC_CMD_SPLN_REQ', 'Header', 'APPN',
    'Dimension', 'UD1', 'NVARCHAR', 1, 1
);

-- Example: Formula
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Data_Type, Is_Required, Is_Active
) VALUES (
    @CalcConfigID, 'XFC_CMD_SPLN_REQ', 'Header', 'Title',
    'Formula', 'MyCalc - {Entity} - {UD1}', 'NVARCHAR', 1, 1
);
```

### Step 5: Create Dest_Cell Records for Detail

Map each column in the detail table:

```sql
-- Example: Monthly spread
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, 
    Apply_Time_Spread, Time_Column_Pattern,
    Data_Type, Is_Active
) VALUES (
    @CalcConfigID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Month{1-12}',
    'SpreadRate', '{CellAmount} * {SpreadRate_Time{N}} / 100',
    1, 'Month{1-12}',
    'DECIMAL', 1
);

-- Example: Conditional Account
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Condition_Expression, Condition_True_Value, Condition_False_Value,
    Data_Type, Is_Active
) VALUES (
    @CalcConfigID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Account',
    'Formula', '{AccountType}', 'Append', 'ments|s',
    'AccountType = ''Commit''', 'Commitments', 'Obligations',
    'NVARCHAR', 1
);
```

## Implementation Notes

### Runtime Execution Flow

1. **Load Configuration**
   - Read `XFC_CMD_SPLN_Calc_Config` for the requested calc name
   - Load associated `Src_Cell` and `Dest_Cell` records

2. **Build Source Formula**
   - Replace placeholders in `Source_Formula_Template`
   - Execute `GetDataBufferUsingFormula`

3. **Iterate Source Cells**
   - For each cell in the data buffer:
     - Extract dimension values
     - Group by configured dimensions
     - Load spread rates if configured

4. **Generate Header Record**
   - Check if header already exists (based on unique key dimensions)
   - If not exists, create new header record
   - Map dimensions/values per Dest_Cell configuration

5. **Generate Detail Records**
   - For each account type (Commit/Obligation):
     - Create detail record
     - Apply spread rates if configured
     - Distribute across time columns

6. **Write to Database**
   - Use SQL adapters (SQA_XFC_CMD_SPLN_REQ, SQA_XFC_CMD_SPLN_REQ_Details)
   - Merge with existing data

### Spread Rate Handling

Spread rates are stored in FDX tables with columns:
- `UD1` (or other match column) - Key to match
- `Account` - Account prefix to match
- `Time1` through `Time13` - Percentage for each time period

**Matching Logic:**
1. For CivPay: Match on APPN (ancestor of UD1) and Account prefix
2. For Withhold: Match on Account prefix only
3. Look up spread rate row
4. Apply percentage to cell amount: `CellAmount * SpreadRate / 100`

### Special Column Patterns

**`Month{1-12}` Pattern:**
Expands to 12 individual mappings (Month1, Month2, ..., Month12), each with Time{N} substitution.

**`ParentGUID` Source:**
Links detail record to its parent header record via `CMD_SPLN_REQ_ID`.

**`GetNextREQID` Generator:**
Calls helper function to generate unique REQ_ID per entity.

## Benefits of This Approach

1. **No Code Changes** - Add new calculations via table configuration
2. **Reusable Patterns** - Common patterns (spread rates, time distribution) are standardized
3. **Maintainable** - Configuration is easier to understand than hardcoded VB logic
4. **Auditable** - All configuration changes tracked in database
5. **Flexible** - Supports complex transformations and conditional logic
6. **Testable** - Can test configurations independently

## Migration Path

### Phase 1: Implement Configuration Tables
- Create DDL schema
- Create SQL adapters for config tables
- Create helper classes to read configuration

### Phase 2: Build Configuration Engine
- Create `CMD_SPLN_Calc_Engine` class
- Implement configuration loader
- Implement source cell reader
- Implement destination cell mapper
- Implement spread rate handler

### Phase 3: Configure Existing Calculations
- Create CivPay configuration
- Create Withhold configuration
- Test side-by-side with existing code

### Phase 4: Migrate FinCustCalc
- Replace hardcoded CivPay with config-driven version
- Replace hardcoded Withhold with config-driven version
- Remove old code

### Phase 5: Enable New Calculations
- Add new calculations via configuration
- No code changes required

## Related Files

- **DDL Schema:** `XFC_CMD_SPLN_Calc_Config_DDL.sql`
- **CivPay Example:** `XFC_CMD_SPLN_Calc_Config_Sample_CivPay.sql`
- **Withhold Example:** `XFC_CMD_SPLN_Calc_Config_Sample_Withhold.sql`
- **Source Business Rule:** `CMD_SPLN_FinCustCalc.vb` (lines 253-670)
- **Inspiration:** FMM `FMM_Src_CellModel.cs`, `FMM_Src_CellDB.cs`

## Future Enhancements

1. **Configuration UI** - Dashboard for managing configurations
2. **Validation Rules** - Validate configuration before execution
3. **Dry Run Mode** - Preview what would be created without writing
4. **Versioning** - Track configuration versions over time
5. **Import/Export** - Move configurations between environments
6. **Extended Transforms** - Add more transformation types as needed
7. **Performance Optimization** - Batch processing for large data sets
8. **Error Handling** - Detailed error logging per configuration step

## Questions?

For questions or clarifications about this configuration system, contact the development team or refer to the implementation code in the CMD SPLN assembly.
