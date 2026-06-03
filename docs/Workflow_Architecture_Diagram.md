# Configurable Workflow System - Architecture Diagram

## System Flow

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Workflow Configuration                          │
│                         (WF_Config)                                  │
│  - Workflow Name, Type, Process                                      │
│  - Settings (comments, delegation, notifications)                    │
└────────────────────────┬────────────────────────────────────────────┘
                         │
                         ├─────────────────┬──────────────────────────┐
                         ▼                 ▼                          ▼
        ┌────────────────────────┐  ┌──────────────┐      ┌──────────────────┐
        │   Data Source Config   │  │   Steps      │      │   Steps          │
        │ (WF_Data_Source_Config)│  │ (WF_Step_    │      │ (WF_Step_        │
        │                        │  │  Config)     │      │  Config)         │
        │ ┌────────┬──────────┐  │  │ Step 1       │      │ Step 2           │
        │ │ Cube?  │  Table?  │  │  │ Approval     │      │ Review           │
        │ └────┬───┴────┬─────┘  │  └──────────────┘      └──────────────────┘
        └──────┼────────┼────────┘
               │        │
       ┌───────┘        └────────┐
       ▼                         ▼
┌─────────────────┐     ┌─────────────────────┐
│  CUBE SOURCE    │     │   TABLE SOURCE      │
│                 │     │                     │
│ Uses:           │     │ Uses:               │
│ - Dimension     │     │ - Column            │
│   hierarchies   │     │   mappings          │
│ - Member levels │     │ - Derivation logic  │
│ - Aggregation   │     │ - Lookup tables     │
└────────┬────────┘     └──────────┬──────────┘
         │                         │
         ▼                         ▼
┌──────────────────────┐  ┌────────────────────────┐
│ WF_Approval_Level_   │  │ WF_Approval_Level_     │
│ Dimension            │  │ Column                 │
│                      │  │                        │
│ - Dimension_Name     │  │ - Column_Name          │
│ - Hierarchy_Name     │  │ - Column_Type:         │
│ - Level_Name         │  │   • Direct             │
│ - Aggregate_To_Level │  │   • Derived            │
│ - Member_Filter      │  │   • Lookup             │
└──────────────────────┘  └────────────────────────┘
```

## Approval Level Derivation Flow

### For TABLE Data Sources:

```
┌─────────────┐
│  Data Row   │ (e.g., Budget transaction with Department = "Finance")
└──────┬──────┘
       │
       ▼
┌────────────────────────────────────────────────┐
│   DeriveTableApprovalLevels()                  │
│                                                │
│   For each approval level column:              │
│   1. Check Column_Type                         │
│      ├─ Direct  → Use column value as-is       │
│      ├─ Derived → Execute derivation logic     │
│      └─ Lookup  → Query related table          │
│                                                │
│   2. Build approval level list                 │
└──────────────────┬─────────────────────────────┘
                   │
                   ▼
          ┌─────────────────┐
          │ Approval Levels │
          │ [               │
          │   {             │
          │     LevelOrder: 1,              │
          │     ColumnName: "Department",   │
          │     ApprovalValue: "Finance",   │
          │     ColumnType: "Direct"        │
          │   }             │
          │ ]               │
          └─────────────────┘
```

### For CUBE Data Sources:

```
┌──────────────────┐
│ Member Selection │ (e.g., Entity = "US_East_Sales")
└────────┬─────────┘
         │
         ▼
┌────────────────────────────────────────────────┐
│   DeriveCubeApprovalLevels()                   │
│                                                │
│   For each approval level dimension:           │
│   1. Get member from dimension                 │
│   2. Find parent at target level               │
│      (walk up hierarchy if needed)             │
│   3. Determine approval member                 │
│                                                │
│   Example: US_East_Sales → Level2 → US_East   │
└──────────────────┬─────────────────────────────┘
                   │
                   ▼
          ┌─────────────────────┐
          │  Approval Levels    │
          │  [                  │
          │    {                │
          │      LevelOrder: 1,              │
          │      DimensionName: "Entity",    │
          │      SelectedMember: "US_East_Sales", │
          │      ApprovalMember: "US_East",  │
          │      ApprovalLevel: "Level2"     │
          │    }                │
          │  ]                  │
          └─────────────────────┘
```

## Table Column Types in Detail

```
┌──────────────────────────────────────────────────────────────┐
│  Column Type: DIRECT                                         │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Budget_Transaction Table                              │  │
│  │  ┌──────┬────────────┬────────┬────────┐              │  │
│  │  │  ID  │ Department │ Amount │ Status │              │  │
│  │  ├──────┼────────────┼────────┼────────┤              │  │
│  │  │  1   │  Finance   │ 10000  │  New   │              │  │
│  │  └──────┴────────────┴────────┴────────┘              │  │
│  │         ↓                                              │  │
│  │   ApprovalValue = "Finance"                            │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  Column Type: DERIVED                                        │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Derivation_Logic:                                     │  │
│  │  "CASE WHEN Amount > 10000 THEN 'VP'                   │  │
│  │        WHEN Amount > 5000 THEN 'Director'              │  │
│  │        ELSE 'Manager' END"                             │  │
│  │                                                        │  │
│  │  Amount = 15000 → ApprovalValue = "VP"                │  │
│  │  Amount = 7000  → ApprovalValue = "Director"          │  │
│  │  Amount = 3000  → ApprovalValue = "Manager"           │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│  Column Type: LOOKUP                                         │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Budget_Transaction         Department_Master          │  │
│  │  ┌──────┬──────────────┐    ┌──────────────┬────────┐ │  │
│  │  │  ID  │ Department_ID│    │ Department_ID│Manager │ │  │
│  │  ├──────┼──────────────┤    ├──────────────┼────────┤ │  │
│  │  │  1   │    101       │───→│     101      │John Doe│ │  │
│  │  └──────┴──────────────┘    └──────────────┴────────┘ │  │
│  │                                      ↓                 │  │
│  │                       ApprovalValue = "John Doe"       │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘
```

## Cube Dimension Hierarchy Traversal

```
Entity Dimension Hierarchy:
┌────────────────────────────────────────┐
│  Level 0 (Total)                       │
│    └─ Corporate                        │
│       │                                │
│  Level 1 (Country)                     │
│       ├─ US                            │
│       │  │                             │
│  Level 2 (Region)                      │
│       │  ├─ US_East ◄───── Target     │
│       │  │  │                          │
│  Level 3 (District)                    │
│       │  │  ├─ US_East_Sales ◄─ Current│
│       │  │  └─ US_East_Support         │
│       │  │                             │
│       │  └─ US_West                    │
│       │                                │
│       └─ EU                            │
└────────────────────────────────────────┘

Configuration:
  Dimension_Name: "Entity"
  Aggregate_To_Level: "Level2"

Processing:
  1. Selected Member: "US_East_Sales" (Level 3)
  2. Walk up hierarchy: US_East_Sales → US_East
  3. Approval Member: "US_East" (Level 2)
```

## API Usage Example

```csharp
// Initialize helper
var helper = new GBL_Workflow_Config_Helper(si);

// 1. DIFFERENTIATE: Determine data source type
var dataSource = helper.GetDataSourceConfiguration(dataSourceConfigId);
string sourceType = dataSource.Field<string>("Data_Source_Type");

if (sourceType == "Cube")
{
    // 2a. CUBE PATH: Derive approval from dimensions
    var memberSelections = new Dictionary<string, string>
    {
        { "Entity", "US_East_Sales" },
        { "Scenario", "Forecast" }
    };
    
    var approvalLevels = helper.DeriveCubeApprovalLevels(
        stepConfigId, 
        dataSourceConfigId, 
        memberSelections
    );
    
    // Result: Approval at US_East (Level2) for Forecast scenario
}
else if (sourceType == "Table")
{
    // 2b. TABLE PATH: Derive approval from columns
    var dataRow = new Dictionary<string, object>
    {
        { "Department", "Finance" },
        { "Amount", 15000 }
    };
    
    var approvalLevels = helper.DeriveTableApprovalLevels(
        stepConfigId, 
        dataSourceConfigId, 
        dataRow
    );
    
    // Result: Approval for Finance department
}
```

## Key Decision Points

```
                   ┌─────────────────┐
                   │ Start Workflow  │
                   └────────┬────────┘
                            │
                   ┌────────▼────────┐
                   │  Get Data       │
                   │  Source Config  │
                   └────────┬────────┘
                            │
                   ┌────────▼─────────┐
                   │ Data_Source_Type?│
                   └────┬─────────┬───┘
                        │         │
              ┌─────────┘         └──────────┐
              │                              │
         ┌────▼────┐                    ┌────▼────┐
         │  Cube   │                    │  Table  │
         └────┬────┘                    └────┬────┘
              │                              │
    ┌─────────▼──────────┐        ┌──────────▼─────────┐
    │ Get Dimension      │        │ Get Column         │
    │ Approval Levels    │        │ Approval Levels    │
    └─────────┬──────────┘        └──────────┬─────────┘
              │                              │
    ┌─────────▼──────────┐        ┌──────────▼─────────┐
    │ Walk Hierarchy     │        │ Check Column_Type: │
    │ to Target Level    │        │ • Direct           │
    │                    │        │ • Derived          │
    │                    │        │ • Lookup           │
    └─────────┬──────────┘        └──────────┬─────────┘
              │                              │
              └────────────┬─────────────────┘
                           │
                  ┌────────▼────────┐
                  │ Return Approval │
                  │ Levels          │
                  └─────────────────┘
```

This architecture provides complete flexibility to handle both dimensional (Cube) 
and relational (Table) data sources with configurable approval level derivation.
