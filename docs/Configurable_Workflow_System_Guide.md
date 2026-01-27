# Configurable Workflow System - Implementation Guide

## Overview

This system provides a flexible, configurable workflow framework for OneStream XF that differentiates between **Cube** and **Table** data sources and determines approval levels dynamically using either:
- **Columns** in tables (for relational data)
- **Dimensions** in cubes (for OLAP data)

## Architecture

The workflow system consists of five main configuration tables:

### 1. WF_Config (Master Workflow Configuration)
- Defines the workflow name, type, and process
- Controls workflow-level settings (comments required, delegation, notifications)

### 2. WF_Data_Source_Config (Data Source Configuration)
- **Differentiates between Cube and Table sources**
- Supports: `Cube`, `Table`, `RelationalView`, `CustomQuery`
- Stores cube name for cube sources or table/schema for relational sources

### 3. WF_Step_Config (Workflow Step Configuration)
- Defines approval steps, their order, and type
- Configures approval requirements (single, all, majority, custom)
- Sets timeout and escalation rules

### 4. WF_Approval_Level_Column (Table-Based Approval Levels)
- **Used when data source is a Table**
- Maps table columns to approval levels
- Supports three column types:
  - **Direct**: Use column value as-is
  - **Derived**: Apply derivation logic (SQL expression or business rule)
  - **Lookup**: Join to another table to derive approval value

### 5. WF_Approval_Level_Dimension (Cube-Based Approval Levels)
- **Used when data source is a Cube**
- Maps dimensions to approval levels
- Supports hierarchy traversal and aggregation
- Can target specific levels or aggregate to parent levels

## Key Concepts

### Differentiating Cube vs Table

The system automatically detects the data source type via the `Data_Source_Type` column:

```csharp
// Check if data source is a cube
bool isCube = workflowHelper.IsCubeDataSource(dataSourceConfigId);

// Check if data source is a table
bool isTable = workflowHelper.IsTableDataSource(dataSourceConfigId);
```

### Approval Level Derivation

#### For Table Data Sources

Approval levels are derived from columns:

```csharp
// Get approval levels for a table-based step
var approvalLevels = workflowHelper.DeriveTableApprovalLevels(
    stepConfigId, 
    dataSourceConfigId, 
    dataRow);

// Result format:
// [
//   { LevelOrder: 1, ColumnName: "Department", ApprovalValue: "Finance", ColumnType: "Direct" },
//   { LevelOrder: 2, ColumnName: "Manager", ApprovalValue: "John Doe", ColumnType: "Lookup" }
// ]
```

**Column Types:**
1. **Direct** - Use the column value directly
   - Example: Department column → Finance department approval
   
2. **Derived** - Apply calculation or logic
   - Example: IF Amount > 10000 THEN "VP" ELSE "Manager"
   
3. **Lookup** - Join to another table
   - Example: Department_ID → Department_Name → Manager_Name

#### For Cube Data Sources

Approval levels are derived from dimensions:

```csharp
// Get approval levels for a cube-based step
var approvalLevels = workflowHelper.DeriveCubeApprovalLevels(
    stepConfigId, 
    dataSourceConfigId, 
    memberSelections);

// Result format:
// [
//   { LevelOrder: 1, DimensionName: "Entity", HierarchyName: "Default", 
//     SelectedMember: "US_East_Sales", ApprovalMember: "US_East", ApprovalLevel: "Level2" }
// ]
```

**Dimension Configuration:**
- **Dimension_Name**: Which dimension to use (e.g., Entity, Account, Scenario)
- **Hierarchy_Name**: Which hierarchy within the dimension (optional)
- **Level_Name**: Target level in the hierarchy (e.g., Level2 for region)
- **Aggregate_To_Level**: Aggregate to a specific parent level
- **Require_All_Descendants**: Approve all child members vs aggregate

## Usage Examples

### Example 1: Table-Based Budget Approval

**Scenario**: Budget entries in a table need approval at the department level.

```sql
-- Create workflow
INSERT INTO WF_Config (Workflow_Name, Workflow_Type, Process_Type, Description)
VALUES ('Budget Approval', 'Approval', 'Budget', 'Department-level budget approval');

DECLARE @WFID INT = SCOPE_IDENTITY();

-- Configure table data source
INSERT INTO WF_Data_Source_Config (WF_Config_ID, Data_Source_Type, Table_Name, Schema_Name)
VALUES (@WFID, 'Table', 'Budget_Transactions', 'dbo');

DECLARE @DSID INT = SCOPE_IDENTITY();

-- Add approval step
INSERT INTO WF_Step_Config (WF_Config_ID, Step_Name, Step_Order, Step_Type, Approval_Type)
VALUES (@WFID, 'Department Manager Approval', 1, 'Approval', 'Single');

DECLARE @StepID INT = SCOPE_IDENTITY();

-- Configure column-based approval (direct column)
INSERT INTO WF_Approval_Level_Column (
    Step_Config_ID, Data_Source_Config_ID, 
    Column_Name, Column_Type, Level_Order
)
VALUES (@StepID, @DSID, 'Department', 'Direct', 1);
```

**Result**: Each budget entry is routed to the appropriate department manager based on the Department column value.

### Example 2: Table with Derived Approval Level

**Scenario**: Approval level depends on transaction amount.

```sql
-- Configure derived column-based approval
INSERT INTO WF_Approval_Level_Column (
    Step_Config_ID, Data_Source_Config_ID, 
    Column_Name, Column_Type, Derivation_Logic, Level_Order
)
VALUES (
    @StepID, @DSID, 
    'Amount', 'Derived', 
    'CASE WHEN Amount > 10000 THEN ''VP'' WHEN Amount > 5000 THEN ''Director'' ELSE ''Manager'' END',
    1
);
```

**Result**: Transactions are routed based on amount thresholds.

### Example 3: Table with Lookup Approval Level

**Scenario**: Approval is based on a manager assigned to each department.

```sql
-- Configure lookup column-based approval
INSERT INTO WF_Approval_Level_Column (
    Step_Config_ID, Data_Source_Config_ID, 
    Column_Name, Column_Type, 
    Lookup_Table, Lookup_Column, Lookup_Key_Column,
    Level_Order
)
VALUES (
    @StepID, @DSID, 
    'Department_ID', 'Lookup',
    'Department_Master', 'Manager_Name', 'Department_ID',
    1
);
```

**Result**: System looks up the manager for each department and routes accordingly.

### Example 4: Cube-Based Forecast Approval

**Scenario**: Forecast data in a cube needs approval at the entity region level.

```sql
-- Create workflow
INSERT INTO WF_Config (Workflow_Name, Workflow_Type, Process_Type, Description)
VALUES ('Forecast Approval', 'Approval', 'Forecast', 'Regional forecast approval');

DECLARE @WFID INT = SCOPE_IDENTITY();

-- Configure cube data source
INSERT INTO WF_Data_Source_Config (WF_Config_ID, Data_Source_Type, Cube_Name)
VALUES (@WFID, 'Cube', 'Forecast');

DECLARE @DSID INT = SCOPE_IDENTITY();

-- Add approval step
INSERT INTO WF_Step_Config (WF_Config_ID, Step_Name, Step_Order, Step_Type, Approval_Type)
VALUES (@WFID, 'Regional Manager Approval', 1, 'Approval', 'Single');

DECLARE @StepID INT = SCOPE_IDENTITY();

-- Configure dimension-based approval (aggregate to Level2 = Region)
INSERT INTO WF_Approval_Level_Dimension (
    Step_Config_ID, Data_Source_Config_ID, 
    Dimension_Name, Hierarchy_Name, Aggregate_To_Level, Level_Order
)
VALUES (@StepID, @DSID, 'Entity', 'Default', 'Level2', 1);
```

**Result**: Forecast entries are aggregated and routed to the regional manager based on the entity hierarchy.

### Example 5: Multi-Dimensional Cube Approval

**Scenario**: Approval requires both entity and scenario dimensions.

```sql
-- Add first dimension (Entity at Level2 = Region)
INSERT INTO WF_Approval_Level_Dimension (
    Step_Config_ID, Data_Source_Config_ID, 
    Dimension_Name, Hierarchy_Name, Aggregate_To_Level, Level_Order
)
VALUES (@StepID, @DSID, 'Entity', 'Default', 'Level2', 1);

-- Add second dimension (Scenario)
INSERT INTO WF_Approval_Level_Dimension (
    Step_Config_ID, Data_Source_Config_ID, 
    Dimension_Name, Level_Order
)
VALUES (@StepID, @DSID, 'Scenario', 2);
```

**Result**: Approval is determined by the combination of region (from entity) and scenario.

## Helper Class Methods

### GBL_Workflow_Config_Helper

```csharp
// Initialize helper
var helper = new GBL_Workflow_Config_Helper(si);

// Get workflow configurations
var workflows = helper.GetWorkflowConfigurations("Budget");

// Get data sources for a workflow
var dataSources = helper.GetDataSourceConfigurations(workflowConfigId);

// Check data source type
bool isCube = helper.IsCubeDataSource(dataSourceConfigId);
bool isTable = helper.IsTableDataSource(dataSourceConfigId);

// Get workflow steps
var steps = helper.GetWorkflowSteps(workflowConfigId);

// Get approval levels (table-based)
var tableApprovals = helper.GetTableApprovalLevels(stepConfigId, dataSourceConfigId);

// Get approval levels (cube-based)
var cubeApprovals = helper.GetCubeApprovalLevels(stepConfigId, dataSourceConfigId);

// Derive approval levels from data
var approvalLevels = helper.DeriveTableApprovalLevels(
    stepConfigId, 
    dataSourceConfigId, 
    dataRow);

// Validate configuration
var errors = helper.ValidateWorkflowConfiguration(workflowConfigId);
```

## Views for Querying

The system provides views for easy querying:

### vw_WF_Complete_Config
Complete workflow configuration with data sources and steps.

```sql
SELECT * FROM vw_WF_Complete_Config
WHERE Process_Type = 'Budget' AND Workflow_Is_Active = 1;
```

### vw_WF_Table_Approval_Levels
Table-based approval level configuration.

```sql
SELECT * FROM vw_WF_Table_Approval_Levels
WHERE Workflow_Name = 'Budget Approval';
```

### vw_WF_Cube_Approval_Levels
Cube-based approval level configuration.

```sql
SELECT * FROM vw_WF_Cube_Approval_Levels
WHERE Workflow_Name = 'Forecast Approval';
```

## Best Practices

1. **Choose the Right Data Source Type**
   - Use **Cube** for dimensional data with hierarchies (forecasts, budgets, actuals)
   - Use **Table** for relational data without hierarchies (transactions, requests, forms)

2. **Design Approval Hierarchies**
   - Start with highest level approval first (Step_Order = 1)
   - Use Level_Order to determine granularity within a step
   - Consider parallel vs serial approval flows

3. **Test Configuration**
   - Use the `ValidateWorkflowConfiguration` method to check for errors
   - Test with sample data before deploying to production

4. **Performance Considerations**
   - For tables: Index columns used in approval level derivation
   - For cubes: Consider hierarchy depth when aggregating
   - Use member filters to limit scope of dimension processing

5. **Security**
   - Approval assignments should respect existing security models
   - Consider using roles instead of specific users for flexibility

## Next Steps

1. **Database Setup**: Run `Workflow_Config_Table_Schema.sql` to create tables
2. **Configuration**: Use SQL or create a UI to configure workflows
3. **Integration**: Use `GBL_Workflow_Config_Helper` in your business rules
4. **Testing**: Validate configurations and test with sample data
5. **Extension**: Add custom logic for complex scenarios

## Support

For questions or issues, refer to the inline documentation in:
- `GBL_Workflow_Config_Helper.cs` - Main helper class
- `SQA_GBL_Workflow_Config.cs` - SQL adapter
- `Workflow_Config_Table_Schema.sql` - Database schema
