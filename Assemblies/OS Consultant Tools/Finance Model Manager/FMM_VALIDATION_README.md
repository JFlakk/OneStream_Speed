# FMM Validation Framework - Complete Documentation

## Overview

The FMM Validation Framework provides a comprehensive, flexible system for configuring and executing data validations in the Finance Model Manager (FMM) module. The framework supports two distinct validation contexts:

1. **TABLE Validations** - Simple validations on table data (column values, constraints, referential integrity)
2. **CUBE Validations** - Complex validations on cube data (scenario comparisons, dimensional analysis, balance checks)

This document provides complete guidance on configuring, executing, and reviewing validations.

## Architecture

### Database Schema

The validation framework consists of three core tables:

1. **FMM_Validation_Config** - Stores validation rule definitions
2. **FMM_Validation_Run** - Tracks validation execution metadata
3. **FMM_Validation_Result** - Stores detailed results for failed validations

### Supporting Components

- **SQL Adapter** (`SQA_FMM_Validation_Config.cs`) - CRUD operations for validation data
- **Dashboard Datasets** (`FMM_DB_DataSets.cs`) - Data access for UI components
- **Helper Views** - Simplified queries for common use cases

## Table Validations

Table validations check data integrity at the table/column level. They are ideal for simple rule checks and data quality validations.

### Table Validation Types

#### 1. ColumnValueList
Validates that a column value is in a predefined list of valid values.

**Use Cases:**
- Status fields must be one of: Draft, Submitted, Approved, Rejected
- Category fields must match a predefined list
- Type fields must be valid enum values

**Config_JSON Structure:**
```json
{
  "ColumnName": "Status",
  "ValidValues": ["Draft", "Submitted", "Approved", "Rejected", "Closed"]
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1,
    'Register Plan Status Validation',
    'Validates that Status column contains only valid status values',
    'TABLE',
    'ColumnValueList',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"ColumnName": "Status", "ValidValues": ["Draft", "Submitted", "Approved", "Rejected", "Closed"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 2. ColumnValueRange
Validates that numeric column values fall within a specified range.

**Use Cases:**
- Monthly amounts must be within reasonable bounds
- Percentages must be between 0 and 100
- Quantities must be positive

**Config_JSON Structure:**
```json
{
  "ColumnName": "Month1",
  "MinValue": -1000000000,
  "MaxValue": 1000000000,
  "IncludeNull": true
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    2,
    'Register Plan Monthly Amount Range',
    'Validates that monthly amounts are within reasonable bounds',
    'TABLE',
    'ColumnValueRange',
    'Register_Plan_Details',
    'Register_Plan',
    1,
    'Warning',
    '{"ColumnName": "Month1", "MinValue": -1000000000, "MaxValue": 1000000000, "IncludeNull": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 3. ColumnValuePattern
Validates that column values match a regular expression pattern.

**Use Cases:**
- Email addresses must be valid format
- Account codes must follow naming convention
- Phone numbers must match expected pattern

**Config_JSON Structure:**
```json
{
  "ColumnName": "Account_Code",
  "Pattern": "^[A-Z]{2}[0-9]{4}$",
  "ErrorMessage": "Account code must be 2 letters followed by 4 digits"
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    3,
    'Account Code Format Validation',
    'Validates that account codes follow the standard format',
    'TABLE',
    'ColumnValuePattern',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"ColumnName": "Account_Code", "Pattern": "^[A-Z]{2}[0-9]{4}$", "ErrorMessage": "Account code must be 2 letters followed by 4 digits"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 4. RequiredColumns
Validates that required columns are not NULL or empty.

**Use Cases:**
- Mandatory fields must be populated
- Key business fields cannot be blank
- Workflow fields are required for processing

**Config_JSON Structure:**
```json
{
  "RequiredColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity"]
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    4,
    'Register Plan Required Fields',
    'Validates that required workflow fields are populated',
    'TABLE',
    'RequiredColumns',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"RequiredColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 5. UniqueConstraint
Validates uniqueness of column combinations.

**Use Cases:**
- Prevent duplicate entries
- Ensure natural key uniqueness
- Validate business key constraints

**Config_JSON Structure:**
```json
{
  "UniqueColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity", "Account"]
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    5,
    'Register Plan Unique Key',
    'Validates that combination of workflow fields is unique',
    'TABLE',
    'UniqueConstraint',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"UniqueColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity", "Account"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 6. ReferentialIntegrity
Validates foreign key relationships between tables.

**Use Cases:**
- Detail records must have valid parent
- Lookup values must exist in reference table
- Cross-table data consistency

**Config_JSON Structure:**
```json
{
  "ForeignKeyColumn": "Register_Plan_ID",
  "ReferencedTable": "Register_Plan",
  "ReferencedColumn": "Register_Plan_ID",
  "AllowNull": false
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    6,
    'Register Plan Detail Foreign Key',
    'Validates that detail records reference valid plan records',
    'TABLE',
    'ReferentialIntegrity',
    'Register_Plan_Details',
    'Register_Plan',
    1,
    'Error',
    '{"ForeignKeyColumn": "Register_Plan_ID", "ReferencedTable": "Register_Plan", "ReferencedColumn": "Register_Plan_ID", "AllowNull": false}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 7. CustomTableSQL
Executes custom SQL query for complex table validations.

**Use Cases:**
- Complex business rules requiring custom logic
- Multi-table validation logic
- Advanced data quality checks

**Config_JSON Structure:**
```json
{
  "SQLQuery": "SELECT Register_Plan_ID, WF_Scenario_Name FROM Register_Plan WHERE Status = 'Approved' AND Approval_Date IS NULL",
  "ErrorMessage": "Approved plans must have an approval date"
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    7,
    'Approved Plan Date Check',
    'Validates that approved plans have approval dates',
    'TABLE',
    'CustomTableSQL',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"SQLQuery": "SELECT Register_Plan_ID, WF_Scenario_Name FROM Register_Plan WHERE Status = ''Approved'' AND Approval_Date IS NULL", "ErrorMessage": "Approved plans must have an approval date"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

## Cube Validations

Cube validations check data integrity across OneStream dimensions and scenarios. They are ideal for complex multi-dimensional business rules.

### Cube Validation Types

#### 1. ScenarioComparison
Compares values between two scenarios across specified dimensions.

**Use Cases:**
- Actual vs Budget variance checks
- Forecast vs Plan comparison
- Current year vs Prior year analysis

**Config_JSON Structure:**
```json
{
  "BaseScenario": "S#Budget",
  "CompareScenario": "S#Actual",
  "VarianceType": "Percentage",
  "Threshold": 10,
  "Dimensions": ["Entity", "Account", "Time"],
  "FilterPOV": {
    "Flow": "F#EndBalance",
    "View": "V#Periodic"
  }
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    10,
    'Actual vs Budget Variance Check',
    'Validates that Actual values do not exceed Budget by more than 10%',
    'CUBE',
    'ScenarioComparison',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Warning',
    '{"BaseScenario": "S#Budget", "CompareScenario": "S#Actual", "VarianceType": "Percentage", "Threshold": 10, "Dimensions": ["Entity", "Account", "Time"], "FilterPOV": {"Flow": "F#EndBalance", "View": "V#Periodic"}}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 2. DimensionalBalance
Validates that data balances across dimensional hierarchies.

**Use Cases:**
- Parent entity totals equal sum of children
- Account rollups match detail
- Time period aggregations are correct

**Config_JSON Structure:**
```json
{
  "BalanceDimension": "Entity",
  "AggregationMethod": "Sum",
  "Tolerance": 0.01,
  "CheckAllParents": true,
  "FixedPOV": {
    "Scenario": "S#Actual",
    "Flow": "F#EndBalance"
  }
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    11,
    'Entity Consolidation Balance',
    'Validates that parent entity totals equal sum of children',
    'CUBE',
    'DimensionalBalance',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"BalanceDimension": "Entity", "AggregationMethod": "Sum", "Tolerance": 0.01, "CheckAllParents": true, "FixedPOV": {"Scenario": "S#Actual", "Flow": "F#EndBalance"}}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 3. CrossDimensionalRule
Validates business rules that span multiple dimensions.

**Use Cases:**
- Revenue must be positive for certain entities and accounts
- Specific account types must have specific flow members
- Complex allocation rules

**Config_JSON Structure:**
```json
{
  "RuleExpression": "Revenue >= 0",
  "Dimensions": {
    "Entity": "E#[TotalCompany].Base",
    "Account": "A#[Revenue].Base",
    "Flow": "F#EndBalance"
  },
  "FixedPOV": {
    "Scenario": "S#Actual",
    "View": "V#Periodic"
  }
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    12,
    'Revenue Positive Check',
    'Validates that revenue accounts have positive values',
    'CUBE',
    'CrossDimensionalRule',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"RuleExpression": "Revenue >= 0", "Dimensions": {"Entity": "E#[TotalCompany].Base", "Account": "A#[Revenue].Base", "Flow": "F#EndBalance"}, "FixedPOV": {"Scenario": "S#Actual", "View": "V#Periodic"}}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 4. TemporalConsistency
Validates data consistency across time periods.

**Use Cases:**
- Monthly amounts must sum to quarterly totals
- Quarterly amounts must sum to yearly totals
- Period-over-period consistency checks

**Config_JSON Structure:**
```json
{
  "TimeDimension": "Time",
  "BaseLevel": "Month",
  "RollupLevel": "Quarter",
  "Tolerance": 0.01,
  "CheckYearly": true
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    13,
    'Month to Quarter Rollup Check',
    'Validates that quarterly amounts equal sum of monthly amounts',
    'CUBE',
    'TemporalConsistency',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"TimeDimension": "Time", "BaseLevel": "Month", "RollupLevel": "Quarter", "Tolerance": 0.01, "CheckYearly": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 5. AllocationValidation
Validates allocation totals and distributions.

**Use Cases:**
- Allocation totals must equal source amount
- Distribution percentages must sum to 100%
- Driver-based calculations are consistent

**Config_JSON Structure:**
```json
{
  "AllocationAccount": "A#AllocatedExpense",
  "SourceAccount": "A#TotalExpense",
  "AllocationDimension": "Entity",
  "Tolerance": 0.01,
  "CheckPercentages": true
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    14,
    'Expense Allocation Total Check',
    'Validates that allocated expenses equal source total',
    'CUBE',
    'AllocationValidation',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"AllocationAccount": "A#AllocatedExpense", "SourceAccount": "A#TotalExpense", "AllocationDimension": "Entity", "Tolerance": 0.01, "CheckPercentages": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 6. VarianceThreshold
Validates that variances between scenarios are within acceptable thresholds.

**Use Cases:**
- Budget variance limits
- Forecast accuracy checks
- Tolerance-based comparisons

**Config_JSON Structure:**
```json
{
  "BaseScenario": "S#Budget",
  "CompareScenario": "S#Forecast",
  "ThresholdType": "Absolute",
  "MinThreshold": -100000,
  "MaxThreshold": 100000,
  "AccountFilter": "A#[Revenue].Base"
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    15,
    'Budget Variance Threshold',
    'Validates that budget variances are within acceptable limits',
    'CUBE',
    'VarianceThreshold',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Warning',
    '{"BaseScenario": "S#Budget", "CompareScenario": "S#Forecast", "ThresholdType": "Absolute", "MinThreshold": -100000, "MaxThreshold": 100000, "AccountFilter": "A#[Revenue].Base"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

#### 7. CustomCubeSQL
Executes custom SQL query for complex cube validations.

**Use Cases:**
- Advanced multi-dimensional rules
- Complex scenario comparisons
- Custom business logic validation

**Config_JSON Structure:**
```json
{
  "SQLQuery": "SELECT EntityName, AccountName, SUM(Amount) as Total FROM vw_CubeData WHERE ScenarioName = 'Actual' AND TimeName = '2024M01' GROUP BY EntityName, AccountName HAVING SUM(Amount) < 0 AND AccountName LIKE 'Revenue%'",
  "ErrorMessage": "Negative revenue found in allocation"
}
```

**Example Configuration:**
```sql
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, 
    Validation_Context, Validation_Type, Target_Object, 
    Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    16,
    'Custom Allocation Check',
    'Custom SQL to validate allocation logic across entities',
    'CUBE',
    'CustomCubeSQL',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"SQLQuery": "SELECT EntityName, AccountName, SUM(Amount) as Total FROM vw_CubeData WHERE ScenarioName = ''Actual'' AND TimeName = ''2024M01'' GROUP BY EntityName, AccountName HAVING SUM(Amount) < 0 AND AccountName LIKE ''Revenue%''", "ErrorMessage": "Negative revenue found in allocation"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

## Working with the Framework

### Creating a Validation Run

```sql
-- Get next Run_ID
DECLARE @RunID INT = (SELECT ISNULL(MAX(Run_ID), 0) + 1 FROM FMM_Validation_Run);

-- Insert run record
INSERT INTO FMM_Validation_Run (
    Run_ID, Run_Date, Run_User, 
    Validation_Context, Process_Type, Target_Object,
    Total_Validations, Total_Records_Checked, Total_Failures, Total_Warnings, Total_Info,
    Execution_Time_Ms, Status, Notes, Create_Date
)
VALUES (
    @RunID, GETDATE(), 'admin',
    NULL, 'Register_Plan', NULL,  -- NULL context runs all validations
    0, 0, 0, 0, 0,
    NULL, 'InProgress', 'Scheduled validation run', GETDATE()
);
```

### Inserting Validation Results

```sql
-- Insert a failed validation result (table context)
INSERT INTO FMM_Validation_Result (
    Validation_Config_ID, Run_ID, Run_Date, Run_User,
    Validation_Context,
    Table_Name, Primary_Key_Value, Column_Name, Column_Value,
    Cube_POV, Dimension_Values, Cell_Value, Comparison_Value,
    Validation_Status, Error_Message, Error_Details, Create_Date
)
VALUES (
    1, @RunID, GETDATE(), 'admin',
    'TABLE',
    'Register_Plan', 'ABC123', 'Status', 'InvalidStatus',
    NULL, NULL, NULL, NULL,
    'Failed', 
    'Invalid status value: InvalidStatus', 
    'Status must be one of: Draft, Submitted, Approved, Rejected, Closed',
    GETDATE()
);

-- Insert a failed validation result (cube context)
INSERT INTO FMM_Validation_Result (
    Validation_Config_ID, Run_ID, Run_Date, Run_User,
    Validation_Context,
    Table_Name, Primary_Key_Value, Column_Name, Column_Value,
    Cube_POV, Dimension_Values, Cell_Value, Comparison_Value,
    Validation_Status, Error_Message, Error_Details, Create_Date
)
VALUES (
    10, @RunID, GETDATE(), 'admin',
    'CUBE',
    NULL, NULL, NULL, NULL,
    '{"Scenario":"S#Actual","Entity":"E001","Account":"A100","Time":"2024M01"}',
    'Scenario=Actual, Entity=East Region, Account=Revenue, Time=Jan 2024',
    1250000.00, 1000000.00,
    'Warning', 
    'Actual exceeds Budget by 25%', 
    'Actual: 1,250,000 vs Budget: 1,000,000 (Variance: 250,000, 25%)',
    GETDATE()
);
```

### Updating Run Summary

```sql
-- Update run with completion statistics
UPDATE FMM_Validation_Run
SET 
    Total_Validations = (SELECT COUNT(DISTINCT Validation_Config_ID) FROM FMM_Validation_Result WHERE Run_ID = @RunID),
    Total_Failures = (SELECT COUNT(*) FROM FMM_Validation_Result WHERE Run_ID = @RunID AND Validation_Status = 'Failed'),
    Total_Warnings = (SELECT COUNT(*) FROM FMM_Validation_Result WHERE Run_ID = @RunID AND Validation_Status = 'Warning'),
    Total_Info = (SELECT COUNT(*) FROM FMM_Validation_Result WHERE Run_ID = @RunID AND Validation_Status = 'Info'),
    Execution_Time_Ms = 5000,
    Status = 'Completed'
WHERE Run_ID = @RunID;
```

## C# Usage Examples

### Using the SQL Adapter

```csharp
using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si))
{
    var adapter = new SQA_FMM_Validation_Config(si, connection);
    
    // Get all active table validations
    DataTable dtTableValidations = adapter.Get_Validation_Config_By_Context(si, "TABLE", activeOnly: true);
    
    // Get all validations for a specific process
    DataTable dtProcessValidations = adapter.Get_Validation_Config_By_Process(si, "Register_Plan", activeOnly: true);
    
    // Create a new validation run
    int runID = adapter.Get_Next_Run_ID(si);
    
    DataTable dtRun = new DataTable();
    dtRun.Columns.Add("Run_ID", typeof(int));
    dtRun.Columns.Add("Run_Date", typeof(DateTime));
    dtRun.Columns.Add("Run_User", typeof(string));
    dtRun.Columns.Add("Status", typeof(string));
    // ... add other columns
    
    DataRow row = dtRun.NewRow();
    row["Run_ID"] = runID;
    row["Run_Date"] = DateTime.Now;
    row["Run_User"] = si.UserName;
    row["Status"] = "InProgress";
    row.SetAdded();
    dtRun.Rows.Add(row);
    
    adapter.Update_FMM_Validation_Run(si, dtRun);
    
    // Execute validations and collect results
    DataTable dtResults = new DataTable();
    // ... populate results
    
    // Bulk insert results
    adapter.Insert_Validation_Results(si, dtResults);
    
    // Update run as completed
    DataTable dtRunUpdate = adapter.Get_Validation_Run_By_ID(si, runID);
    DataRow runRow = dtRunUpdate.Rows[0];
    runRow["Status"] = "Completed";
    runRow["Total_Failures"] = 5;
    runRow["Total_Warnings"] = 3;
    runRow.SetModified();
    
    adapter.Update_FMM_Validation_Run(si, dtRunUpdate);
}
```

### Using Dashboard Datasets

```csharp
// In a dashboard component, datasets can be accessed through the FMM_DB_DataSets class
// These datasets are automatically available through the OneStream dashboard framework

// Example dataset calls that would be available in dashboard configuration:
// - get_FMM_Validation_Config: Returns all validation configurations
// - get_FMM_Validation_Contexts: Returns TABLE and CUBE context options
// - get_FMM_Table_Validation_Types: Returns all table validation types
// - get_FMM_Cube_Validation_Types: Returns all cube validation types
// - get_FMM_Validation_Severities: Returns Error, Warning, Info options
// - get_FMM_Validation_Results: Returns results with optional filtering
// - get_FMM_Validation_Runs: Returns run history with optional filtering
```

## Querying Validation Data

### View All Validations with Statistics

```sql
SELECT * FROM vw_FMM_Validation_Config_Summary
WHERE Is_Active = 1
ORDER BY Validation_Context, Process_Type, Name;
```

### View Recent Validation Results

```sql
SELECT * FROM vw_FMM_Validation_Results
WHERE Run_Date >= DATEADD(day, -7, GETDATE())
  AND Validation_Status IN ('Failed', 'Warning')
ORDER BY Run_Date DESC, Validation_Status DESC;
```

### View Validation Run History

```sql
SELECT * FROM vw_FMM_Validation_Run_Summary
WHERE Status = 'Completed'
  AND Run_Date >= DATEADD(day, -30, GETDATE())
ORDER BY Run_Date DESC;
```

### Find All Failed Validations for a Specific Table

```sql
SELECT 
    vc.Name AS Validation_Name,
    vr.Primary_Key_Value,
    vr.Column_Name,
    vr.Column_Value,
    vr.Error_Message,
    vr.Run_Date
FROM FMM_Validation_Result vr
INNER JOIN FMM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID
WHERE vr.Table_Name = 'Register_Plan'
  AND vr.Validation_Status = 'Failed'
  AND vr.Run_Date >= DATEADD(day, -7, GETDATE())
ORDER BY vr.Run_Date DESC;
```

### Find All Cube Validations with Dimensional POV

```sql
SELECT 
    vc.Name AS Validation_Name,
    vr.Dimension_Values,
    vr.Cell_Value,
    vr.Comparison_Value,
    vr.Error_Message,
    vr.Run_Date
FROM FMM_Validation_Result vr
INNER JOIN FMM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID
WHERE vr.Validation_Context = 'CUBE'
  AND vr.Validation_Status IN ('Failed', 'Warning')
  AND vr.Run_Date >= DATEADD(day, -7, GETDATE())
ORDER BY vr.Run_Date DESC, vc.Severity DESC;
```

## Data Maintenance

### Archive Old Results

```sql
-- Delete results older than 90 days
DELETE FROM FMM_Validation_Result
WHERE Run_Date < DATEADD(day, -90, GETDATE());
```

### Delete Specific Run Results

```sql
-- Delete all results from a specific run
DECLARE @RunIDToDelete INT = 123;

DELETE FROM FMM_Validation_Result
WHERE Run_ID = @RunIDToDelete;

DELETE FROM FMM_Validation_Run
WHERE Run_ID = @RunIDToDelete;
```

### Disable a Validation

```sql
-- Disable a validation without deleting it
UPDATE FMM_Validation_Config
SET Is_Active = 0,
    Update_Date = GETDATE(),
    Update_User = 'admin'
WHERE Validation_Config_ID = 5;
```

## Best Practices

### Configuration Best Practices

1. **Use Clear, Descriptive Names** - Validation names should clearly indicate what is being validated
2. **Add Detailed Descriptions** - Include business context and purpose in descriptions
3. **Set Appropriate Severity Levels** - Use Error for must-fix issues, Warning for should-review, Info for awareness
4. **Start with Table Validations** - Implement simpler table validations before complex cube validations
5. **Test Validations Incrementally** - Test each validation individually before running all together

### Execution Best Practices

1. **Schedule Regular Runs** - Run validations on a regular schedule (daily, weekly, etc.)
2. **Run After Data Loads** - Execute validations after data imports or major updates
3. **Monitor Execution Time** - Track execution time and optimize slow validations
4. **Review Results Promptly** - Address validation failures quickly to maintain data quality
5. **Archive Old Results** - Regularly clean up old validation results to maintain performance

### Performance Considerations

1. **Use Bulk Operations** - SqlBulkCopy is optimized for inserting large result sets
2. **Index Critical Fields** - Ensure validation target tables have appropriate indexes
3. **Limit Result Details** - Store only essential information in result records
4. **Partition Large Runs** - Break very large validation runs into smaller batches
5. **Use Views for Complex Queries** - Create database views for frequently-used validation queries

### Security Considerations

1. **Protect Sensitive Data** - Avoid storing sensitive values in validation results
2. **Control Access** - Limit who can create/modify validation configurations
3. **Audit Configuration Changes** - Track all changes to validation rules
4. **Parameterize Custom SQL** - Use parameters in CustomSQL validations to prevent SQL injection
5. **Review Custom Queries** - Have custom SQL queries reviewed before deployment

## Troubleshooting

### Common Issues

**Issue:** Validation fails with "Object not found" error
- **Solution:** Verify Target_Object exists in database and user has permissions

**Issue:** Bulk insert fails with column mapping error
- **Solution:** Ensure result DataTable has all required columns matching schema

**Issue:** Custom SQL validation returns no results when it should
- **Solution:** Test SQL query independently to verify it returns expected results

**Issue:** Cube validation POV format is incorrect
- **Solution:** Ensure Cube_POV JSON uses correct member syntax (e.g., "S#Actual" not "Actual")

**Issue:** Validation results table grows too large
- **Solution:** Implement regular archival process using Delete_Old_Validation_Results method

## Summary

The FMM Validation Framework provides a comprehensive solution for data quality validation in the Finance Model Manager module. Key benefits include:

- **Flexibility** - Supports both simple table validations and complex cube validations
- **Extensibility** - Easy to add new validation types through JSON configuration
- **Performance** - Optimized for large data volumes with bulk operations and indexes
- **Auditability** - Complete tracking of validation runs and results
- **Maintainability** - Clear separation of configuration, execution, and results

For questions or support, refer to the database schema documentation, SQL adapter code comments, and dashboard dataset implementations.
