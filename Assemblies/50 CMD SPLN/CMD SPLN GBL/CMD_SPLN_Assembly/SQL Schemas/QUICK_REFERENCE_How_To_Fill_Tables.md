# Quick Reference: How to Fill Configuration Tables

This is a **practical guide** for filling out the cube-to-table calculation configuration tables. Use this as a cheat sheet when creating new configurations.

## Table Filling Order

Always fill tables in this order:
1. `XFC_CMD_SPLN_Calc_Config` (header)
2. `XFC_CMD_SPLN_Calc_Src_Cell` (source)
3. `XFC_CMD_SPLN_Calc_Dest_Cell` (destination - header first, then detail)

---

## Table 1: XFC_CMD_SPLN_Calc_Config (Calculation Header)

**Purpose:** Define the overall calculation

### Common Field Values

| Field | CivPay Example | Withhold Example | Notes |
|-------|---------------|------------------|-------|
| `Calc_Name` | `'Copy_CivPay'` | `'Copy_Withhold'` | Unique name, matches function name |
| `Calc_Type` | `'CubeToTable'` | `'CubeToTable'` | Type of calculation |
| `Description` | `'Copy CivPay funding lines...'` | `'Copy Withhold funding lines...'` | What it does |
| `Source_Type` | `'Cube'` | `'Cube'` | Where data comes from |
| `Source_Formula_Template` | See template below | See template below | Cube formula with {placeholders} |
| `Dest_Table_Header` | `'XFC_CMD_SPLN_REQ'` | `'XFC_CMD_SPLN_REQ'` | Header table name |
| `Dest_Table_Detail` | `'XFC_CMD_SPLN_REQ_Details'` | `'XFC_CMD_SPLN_REQ_Details'` | Detail table name |
| `Use_Spread_Rates` | `1` (yes) | `1` (yes) | Apply spread rates? |
| `Spread_Rate_FDX_Name` | `'CMD_SPLN_APPN_SpreadPct_FDX_CV'` | `'CMD_SPLN_WH_SpreadPct_FDX_CV'` | FDX table name |
| `Spread_Rate_Match_Columns` | `'UD1,Account'` | `'Account'` | How to match rates |
| `REQ_ID_Type` | `'CivPay_Copied'` | `'Withhold'` | For filtering REQs |
| `Generate_REQ_ID` | `1` (yes) | `1` (yes) | Auto-generate IDs? |
| `Status_Level_Template` | `'{EntityLevel}_Formulate_SPLN'` | `'{EntityLevel}_Formulate_SPLN'` | Status template |
| `Execution_Order` | `10` | `20` | Run order |
| `Is_Active` | `1` | `1` | Active flag |

### Source Formula Template Examples

**CivPay:**
```
FilterMembers(RemoveZeros(
  E#{Entity}:S#{SourcePosition}:T#{TargetYear}:C#Aggregated:V#Periodic:
  F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top
),[A#Target])
```

**Withhold:**
```
FilterMembers(RemoveZeros(
  Cb#{CMDName}:E#{Entity}:S#{SourcePosition}:T#{TargetYear}:C#Aggregated:V#Periodic:
  F#Tot_Dist_Final:O#Top:I#None:U7#Top:U8#None
),[A#TGT_WH])
```

---

## Table 2: XFC_CMD_SPLN_Calc_Src_Cell (Source Cell)

**Purpose:** Define which cube cells to read

### Typical Configuration (One Row)

| Field | CivPay | Withhold | Notes |
|-------|--------|----------|-------|
| `Calc_Config_ID` | `@CalcID` | `@CalcID` | FK to parent |
| `Src_Order` | `1` | `1` | Order (usually 1) |
| `Src_Type` | `'CubeCell'` | `'CubeCell'` | Reading from cube |
| `Src_Item` | `'CivPay_FundingLines'` | `'Withhold_FundingLines'` | Description |
| `Entity` | `'{POV}'` | `'{POV}'` | Use current entity |
| `Scenario` | `'{SourcePosition}'` | `'{SourcePosition}'` | Runtime param |
| `Time` | `'{TargetYear}'` | `'{TargetYear}'` | Runtime param |
| `View` | `'Periodic'` | `'Periodic'` | View member |
| `Account` | `'Target'` | `'TGT_WH'` | Filter to this account |
| `IC` | `'Top'` | `'None'` | IC member |
| `Origin` | `'Top'` | `'Top'` | Origin member |
| `Flow` | `'Tot_Dist_Final'` | `'Tot_Dist_Final'` | Flow member |
| `UD1` | `NULL` | `NULL` | Iterate all (NULL = any) |
| `UD2` | `NULL` | `NULL` | Iterate all |
| `UD3` | `NULL` | `NULL` | Iterate all |
| `UD4` | `NULL` | `NULL` | Iterate all |
| `UD5` | `NULL` | `NULL` | Iterate all |
| `UD6` | `'Pay_Benefits'` | `NULL` | Filter or iterate |
| `UD7` | `'Top'` | `'Top'` | Specific value |
| `UD8` | `'Top'` | `'None'` | Specific value |
| `Additional_Filter` | `'RemoveZeros'` | `'RemoveZeros'` | Extra filters |
| `Group_By_Dimensions` | `'UD1,UD2,UD3,UD4,UD5'` | `'UD1,UD2,UD3,UD4,UD5,UD6'` | Group by these |
| `Aggregation_Method` | `'SUM'` | `'SUM'` | How to aggregate |

### Dimension Filter Rules

- `NULL` = Iterate all values (group by this dimension)
- `'Top'` / `'None'` / `'Value'` = Filter to specific value
- `'{POV}'` = Use current POV value
- `'{SourcePosition}'` = Use runtime parameter

---

## Table 3: XFC_CMD_SPLN_Calc_Dest_Cell (Destination Mappings)

**Purpose:** Map source data to destination table columns

### 3A. Header Table Mappings

Create one row per header column. Common patterns:

#### Workflow Fields (Constants)
```sql
-- All use runtime parameters
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ', 'Header', 'WFScenario_Name', 'Constant', '{TargetScenario}'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'WFTime_Name', 'Constant', '{TimeName}'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'WFCMD_Name', 'Constant', '{CMDName}');
```

#### Dimension Mappings
```sql
-- Map dimensions directly
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ', 'Header', 'Entity', 'Dimension', 'Entity'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'APPN', 'Dimension', 'UD1'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'MDEP', 'Dimension', 'UD2'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'APE9', 'Dimension', 'UD3');
```

#### Constants
```sql
-- Fixed values
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ', 'Header', 'Obj_Class', 'Constant', 'Pay_General'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'UIC', 'Constant', 'None');
```

#### Formulas
```sql
-- Combine values
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ', 'Header', 'Title', 'Formula', 'CivPay - {Entity} - {UD1}-{UD2}-{UD3}-{UD4}-{UD5}');
```

#### Generated Values
```sql
-- Auto-generated
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ', 'Header', 'CMD_SPLN_REQ_ID', 'Generated', 'GUID'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'REQ_ID', 'Generated', 'GetNextREQID'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'Create_Date', 'Generated', 'DateTime'),
  ('XFC_CMD_SPLN_REQ', 'Header', 'Create_User', 'Generated', 'UserName');
```

### 3B. Detail Table Mappings

#### Simple Mappings
```sql
-- Most fields same as header
INSERT INTO Dest_Cell (Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value)
VALUES 
  ('XFC_CMD_SPLN_REQ_Details', 'Detail', 'CMD_SPLN_REQ_ID', 'Generated', 'ParentGUID'),
  ('XFC_CMD_SPLN_REQ_Details', 'Detail', 'Entity', 'Dimension', 'Entity'),
  ('XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD1', 'Dimension', 'UD1'),
  ('XFC_CMD_SPLN_REQ_Details', 'Detail', 'IC', 'Constant', 'None');
```

#### Conditional Account (CivPay)
```sql
-- Account changes based on iteration (Commitments vs Obligations)
INSERT INTO Dest_Cell (
  Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
  Source_Type, Source_Value, Transform_Type, Transform_Rule,
  Condition_Expression, Condition_True_Value, Condition_False_Value
) VALUES (
  'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Account',
  'Formula', '{AccountType}', 'Append', 'ments|s',
  'AccountType = ''Commit''', 'Commitments', 'Obligations'
);
```

#### Conditional Account with Prefix (Withhold)
```sql
-- Same but with WH_ prefix
INSERT INTO Dest_Cell (
  Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
  Source_Type, Source_Value, Transform_Type, Transform_Rule,
  Condition_Expression, Condition_True_Value, Condition_False_Value
) VALUES (
  'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Account',
  'Formula', 'WH_{AccountType}', 'Append', 'ments|s',
  'AccountType = ''Commit''', 'WH_Commitments', 'WH_Obligations'
);
```

#### UD6 Transformation (Withhold)
```sql
-- Transform Pay_Benefits/Pay_General → Pay_General, else NonPay_General
INSERT INTO Dest_Cell (
  Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
  Source_Type, Source_Value, Transform_Type, Transform_Rule,
  Condition_Expression, Condition_True_Value, Condition_False_Value
) VALUES (
  'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD6',
  'Dimension', 'UD6', 'Replace', 'Pay_Benefits,Pay_General→Pay_General',
  'UD6 IN (''Pay_Benefits'', ''Pay_General'')', 'Pay_General', 'NonPay_General'
);
```

#### Monthly Spread (Month1-12)
```sql
-- Special notation: Month{1-12} expands to 12 rows
INSERT INTO Dest_Cell (
  Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
  Source_Type, Source_Value,
  Apply_Time_Spread, Time_Column_Pattern
) VALUES (
  'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Month{1-12}',
  'SpreadRate', '{CellAmount} * {SpreadRate_Time{N}} / 100',
  1, 'Month{1-12}'
);
```

#### Carryover Year (CivPay Only)
```sql
-- Next year if Time13 spread rate exists
INSERT INTO Dest_Cell (
  Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
  Source_Type, Source_Value,
  Condition_Expression, Condition_True_Value, Condition_False_Value,
  Apply_Time_Spread, Time_Column_Pattern
) VALUES (
  'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Yearly',
  'SpreadRate', '{CellAmount} * {SpreadRate_Time13} / 100',
  'SpreadRate_Time13 <> 0', '{Formula}', '0',
  1, 'Yearly'
);
```

---

## Quick Fill Template

Use this template for a new calculation:

```sql
-- 1. HEADER
DECLARE @CalcID INT;

INSERT INTO XFC_CMD_SPLN_Calc_Config (
    Calc_Name, Calc_Type, Description,
    Source_Formula_Template, 
    Dest_Table_Header, Dest_Table_Detail,
    Use_Spread_Rates, Spread_Rate_FDX_Name, Spread_Rate_Match_Columns,
    REQ_ID_Type, Generate_REQ_ID, Status_Level_Template,
    Execution_Order, Is_Active
) VALUES (
    'My_Calc', 'CubeToTable', 'Description...',
    'FilterMembers(RemoveZeros(...)),[filters]',
    'XFC_CMD_SPLN_REQ', 'XFC_CMD_SPLN_REQ_Details',
    1, 'My_FDX', 'UD1,Account',
    'MyType', 1, '{EntityLevel}_Formulate_SPLN',
    100, 1
);
SET @CalcID = SCOPE_IDENTITY();

-- 2. SOURCE
INSERT INTO XFC_CMD_SPLN_Calc_Src_Cell (
    Calc_Config_ID, Src_Order, Src_Type, Src_Item,
    Entity, Scenario, Time, View, Account, IC, Origin, Flow,
    UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,
    Additional_Filter, Group_By_Dimensions, Aggregation_Method, Is_Active
) VALUES (
    @CalcID, 1, 'CubeCell', 'MySource',
    '{POV}', '{SourcePosition}', '{TargetYear}', 'Periodic', 'MyAccount', 'Top', 'Top', 'MyFlow',
    NULL, NULL, NULL, NULL, NULL, NULL, 'Top', 'Top',
    'RemoveZeros', 'UD1,UD2,UD3', 'SUM', 1
);

-- 3. DEST - HEADER (one per column)
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active)
VALUES 
    (@CalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFScenario_Name', 'Constant', '{TargetScenario}', 'NVARCHAR', 1, 1),
    (@CalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Entity', 'Dimension', 'Entity', 'NVARCHAR', 1, 1),
    (@CalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'APPN', 'Dimension', 'UD1', 'NVARCHAR', 1, 1);
    -- ... more columns ...

-- 4. DEST - DETAIL (one per column)
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active)
VALUES 
    (@CalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'CMD_SPLN_REQ_ID', 'Generated', 'ParentGUID', 'UNIQUEIDENTIFIER', 1, 1),
    (@CalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD1', 'Dimension', 'UD1', 'NVARCHAR', 1, 1);
    -- ... more columns ...

-- 5. DEST - TIME SPREAD
INSERT INTO XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Apply_Time_Spread, Time_Column_Pattern, Data_Type, Is_Active
) VALUES (
    @CalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Month{1-12}',
    'SpreadRate', '{CellAmount} * {SpreadRate_Time{N}} / 100', 1, 'Month{1-12}', 'DECIMAL', 1
);
```

---

## Common Patterns Summary

| What | Source_Type | Source_Value | Transform_Type | Notes |
|------|-------------|--------------|----------------|-------|
| Copy dimension | `Dimension` | `UD1` | `None` | Direct copy |
| Fixed value | `Constant` | `'None'` | `None` | Always this value |
| Runtime param | `Constant` | `'{TimeName}'` | `None` | Replaced at runtime |
| Build string | `Formula` | `'Title - {Entity} - {UD1}'` | `None` | Combines values |
| Append text | `Formula` | `'{AccountType}'` | `Append` | Add suffix (ments/s) |
| Replace text | `Dimension` | `UD6` | `Replace` | Transform values |
| If/Then/Else | Any | Any | Any | Use Condition_Expression |
| Spread rates | `SpreadRate` | `'{CellAmount} * {Rate} / 100'` | `Calculate` | With Apply_Time_Spread=1 |
| GUID | `Generated` | `'GUID'` | `None` | New GUID |
| Timestamp | `Generated` | `'DateTime'` | `None` | Current datetime |
| Username | `Generated` | `'UserName'` | `None` | Current user |
| REQ ID | `Generated` | `'GetNextREQID'` | `None` | Sequential ID |
| Parent link | `Generated` | `'ParentGUID'` | `None` | Link to header |

---

## Validation Checklist

Before running a new configuration:

- [ ] Calc_Config has unique Calc_Name
- [ ] Source_Formula_Template has all needed placeholders
- [ ] Src_Cell has correct dimension filters (NULL vs specific)
- [ ] Group_By_Dimensions includes all iterated dimensions
- [ ] All required header columns are mapped
- [ ] All required detail columns are mapped
- [ ] Account conditional logic is correct
- [ ] Time spread columns use correct pattern
- [ ] Spread rate FDX name matches existing table
- [ ] REQ_ID_Type is unique for this calc
- [ ] Execution_Order doesn't conflict with other calcs

---

For complete examples, see:
- `XFC_CMD_SPLN_Calc_Config_Sample_CivPay.sql`
- `XFC_CMD_SPLN_Calc_Config_Sample_Withhold.sql`
- `README_Calc_Config_System.md`
