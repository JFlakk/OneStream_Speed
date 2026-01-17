# Model Dimension Manager - Validation Configuration

## Overview

The Validation Configuration feature in Model Dimension Manager provides a comprehensive framework for configuring and executing dimension member validations through the Dashboard Config UI. This system allows administrators to define validation rules, execute them against dimension data, and review detailed results for each member that fails validation.

## Key Features

### Configurable Validation Types

The system supports nine built-in validation types covering common EPM dimension validation scenarios:

1. **MissingInHierarchy** - Ensures members appear in required hierarchies
2. **MissingTextProperty** - Checks if required text properties are populated
3. **InvalidTextPropertyValue** - Validates text property values against lists or databases
4. **DuplicateMembers** - Identifies duplicate member names or descriptions
5. **OrphanedMembers** - Finds members with no valid parent relationships
6. **CircularReferences** - Detects circular parent-child relationships
7. **InvalidParentChild** - Validates parent-child relationships against business rules
8. **MissingRequiredProperty** - Checks for missing required properties
9. **CustomSQL** - Executes custom SQL queries for specialized validations

### Severity Levels

Each validation can be assigned one of three severity levels:
- **Error** - Critical issues that must be resolved
- **Warning** - Issues that should be reviewed but may not block processing
- **Info** - Informational findings for review

### Execution Tracking

The system tracks validation execution with:
- Run metadata (date, user, dimension, execution time)
- Summary statistics (total validations, members checked, failures, warnings)
- Detailed results for each member that fails validation
- Historical tracking for trend analysis

## Database Schema

The validation framework uses three related tables:

### MDM_Validation_Config (Configuration Table)

Stores validation rule definitions and configuration.

**Key Fields:**
- `Validation_Config_ID` - Primary key (INT, NOT NULL, manually assigned)
- `Name` - Descriptive name for the validation (NVARCHAR(100), NOT NULL)
- `Description` - Detailed description of validation purpose (NVARCHAR(500))
- `Dim_Type` - The dimension type name (NVARCHAR(50), NOT NULL)
- `Dim_ID` - The specific dimension ID (INT, NOT NULL)
- `Validation_Type` - Type of validation (NVARCHAR(50), NOT NULL, constrained)
- `Is_Active` - Whether validation is enabled (BIT, NOT NULL)
- `Severity` - Severity level: Error, Warning, or Info (NVARCHAR(20), NOT NULL)
- `Config_JSON` - JSON configuration specific to validation type (NVARCHAR(MAX))
- Audit fields: `Create_Date`, `Create_User`, `Update_Date`, `Update_User`

**Constraints:**
- Foreign key to `Dim` table on `Dim_ID`
- Check constraint on `Validation_Type` for valid types
- Check constraint on `Severity` for valid levels

### MDM_Validation_Result (Results Table)

Stores detailed results for each member that fails validation.

**Key Fields:**
- `Validation_Result_ID` - Primary key (INT, IDENTITY)
- `Validation_Config_ID` - Foreign key to configuration (INT, NOT NULL)
- `Run_ID` - Links results to a specific validation run (INT, NOT NULL)
- `Run_Date` - When the validation was executed (DATETIME, NOT NULL)
- `Run_User` - User who executed the validation (NVARCHAR(50), NOT NULL)
- `Member_Name` - Name of the member that failed (NVARCHAR(255), NOT NULL)
- `Member_ID` - OneStream member ID (INT, nullable)
- `Validation_Status` - Result status: Failed, Passed, Warning, Error (NVARCHAR(20), NOT NULL)
- `Error_Message` - Summary error message (NVARCHAR(MAX))
- `Error_Details` - Detailed error information (NVARCHAR(MAX))
- `Create_Date` - Timestamp when result was recorded (DATETIME, NOT NULL)

**Constraints:**
- Foreign key to `MDM_Validation_Config` with CASCADE DELETE
- Check constraint on `Validation_Status`

**Indexes:**
- Config ID for filtering by validation
- Run ID for retrieving run results
- Run Date for temporal queries
- Member Name and Status for member-specific lookups

### MDM_Validation_Run (Run Metadata Table)

Stores summary information about validation executions.

**Key Fields:**
- `Run_ID` - Primary key (INT, NOT NULL, manually assigned)
- `Run_Date` - When validation run was executed (DATETIME, NOT NULL)
- `Run_User` - User who executed the run (NVARCHAR(50), NOT NULL)
- `Dim_Type` - Dimension type validated (NVARCHAR(50), nullable)
- `Dim_ID` - Specific dimension validated (INT, nullable)
- `Total_Validations` - Number of validations executed (INT, NOT NULL)
- `Total_Members_Checked` - Number of members checked (INT, NOT NULL)
- `Total_Failures` - Count of failed validations (INT, NOT NULL)
- `Total_Warnings` - Count of warnings (INT, NOT NULL)
- `Execution_Time_Ms` - Execution time in milliseconds (INT, nullable)
- `Status` - Run status: Completed, Failed, InProgress, Cancelled (NVARCHAR(20), NOT NULL)
- `Notes` - Additional notes about the run (NVARCHAR(MAX))
- `Create_Date` - Timestamp when run was recorded (DATETIME, NOT NULL)

**Constraints:**
- Check constraint on `Status`

**Indexes:**
- Run Date for historical queries
- Dimension Type and ID for dimension-specific lookups

See `/scripts/MDM_Validation_Table_Schema.sql` for the complete schema definition.

## Implementation Components

### 1. SQL Adapter (`SQA_MDM_Validation_Config.cs`)

Located in: `MDM_Config_UI_Assembly/SQL Adapters/`

#### Validation Config Methods:
- **Fill_MDM_Validation_Config_DT** - Fills DataTable with config data
- **Update_MDM_Validation_Config** - Saves configuration changes
- **Get_Validation_Config_By_ID** - Retrieves specific config by ID
- **Get_Validation_Config_By_Dimension** - Retrieves configs for a dimension
- **Get_Active_Validation_Configs** - Retrieves all active validations

#### Validation Result Methods:
- **Fill_MDM_Validation_Result_DT** - Fills DataTable with result data
- **Insert_Validation_Results** - Bulk inserts results (uses SqlBulkCopy)
- **Get_Validation_Results_By_Run_ID** - Retrieves results for a run
- **Get_Validation_Results_By_Config_ID** - Retrieves results for a config
- **Delete_Validation_Results_By_Run_ID** - Deletes results for a run
- **Delete_Old_Validation_Results** - Archives old results

#### Validation Run Methods:
- **Fill_MDM_Validation_Run_DT** - Fills DataTable with run data
- **Update_MDM_Validation_Run** - Saves run metadata
- **Get_Validation_Run_By_ID** - Retrieves specific run by ID
- **Get_Recent_Validation_Runs** - Retrieves N most recent runs
- **Get_Validation_Runs_By_Dimension** - Retrieves runs for a dimension

### 2. Dashboard DataSets (`MDM_DB_DataSets.cs`)

Located in: `MDM_Config_UI_Assembly/DB DataSets/`

#### Available DataSets:

**Get_Validation_Config**
- Returns all validation configurations
- Columns: Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, Validation_Type, Is_Active, Severity, Config_JSON, Create/Update audit fields

**Get_Validation_Types**
- Returns available validation types with descriptions
- Columns: TypeName, TypeDisplayName, Description
- Useful for populating validation type dropdowns

**Get_Validation_Severities**
- Returns severity levels
- Columns: SeverityName, SeverityDisplayName
- Values: Error, Warning, Info

**Get_Validation_Results**
- Returns validation results with optional filtering
- Optional Parameters: runID, validationConfigID
- Columns: Validation_Result_ID, Validation_Config_ID, Validation_Name, Validation_Type, Severity, Run_ID, Run_Date, Run_User, Member_Name, Member_ID, Validation_Status, Error_Message, Error_Details, Create_Date
- Joins to config table for validation details

**Get_Validation_Runs**
- Returns validation run history with optional filtering
- Optional Parameters: dimType, dimID, topN (default: 50)
- Columns: Run_ID, Run_Date, Run_User, Dim_Type, Dim_ID, Total_Validations, Total_Members_Checked, Total_Failures, Total_Warnings, Execution_Time_Ms, Status, Notes, Create_Date

## Validation Configuration Examples

### 1. Missing in Hierarchy Validation

Ensures all members appear in required hierarchies (e.g., Total Company, Legal Entity).

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1,
    'Account Hierarchy Coverage',
    'Ensures all accounts are represented in both Total Company and Legal Entity hierarchies',
    'Account',
    1,
    'MissingInHierarchy',
    1,
    'Error',
    '{"RequiredHierarchies": ["Total Company", "Legal Entity"], "CheckAllParents": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "RequiredHierarchies": ["Hierarchy1", "Hierarchy2"],
  "CheckAllParents": true
}
```

### 2. Missing Text Property Validation

Checks if required text properties are populated.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    2,
    'Account Category Required',
    'Validates that all accounts have Text1 (Category) and Text2 (SubCategory) populated',
    'Account',
    1,
    'MissingTextProperty',
    1,
    'Warning',
    '{"RequiredProperties": ["Text1", "Text2"], "AllowEmpty": false}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "RequiredProperties": ["Text1", "Text2", "Text3"],
  "AllowEmpty": false
}
```

### 3. Invalid Text Property Value - List Validation

Validates text property values against a predefined list.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    3,
    'Account Type Validation',
    'Ensures account type (Text1) contains only valid values',
    'Account',
    1,
    'InvalidTextPropertyValue',
    1,
    'Error',
    '{"PropertyName": "Text1", "ValidationType": "List", "ValidValues": ["Asset", "Liability", "Equity", "Revenue", "Expense"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema for List:**
```json
{
  "PropertyName": "Text1",
  "ValidationType": "List",
  "ValidValues": ["Value1", "Value2", "Value3"]
}
```

### 4. Invalid Text Property Value - Database Validation

Validates text property values against a database source.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    4,
    'Account Category DB Validation',
    'Validates account categories against source system master list',
    'Account',
    1,
    'InvalidTextPropertyValue',
    1,
    'Error',
    '{"PropertyName": "Text3", "ValidationType": "Database", "SourceConnection": "FinanceDB", "SourceQuery": "SELECT DISTINCT Category FROM ValidAccountCategories WHERE Active = 1"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema for Database:**
```json
{
  "PropertyName": "Text3",
  "ValidationType": "Database",
  "SourceConnection": "ConnectionName",
  "SourceQuery": "SELECT DISTINCT ValidValue FROM ReferenceTable WHERE Active = 1"
}
```

### 5. Duplicate Members Validation

Identifies duplicate member names or descriptions.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    5,
    'Duplicate Entity Check',
    'Identifies duplicate entity names',
    'Entity',
    2,
    'DuplicateMembers',
    1,
    'Error',
    '{"CheckFields": ["Name"], "CaseSensitive": false}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "CheckFields": ["Name", "Description"],
  "CaseSensitive": false
}
```

### 6. Orphaned Members Validation

Identifies members with no valid parent relationships.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    6,
    'Orphaned Account Check',
    'Identifies accounts with no parent relationships',
    'Account',
    1,
    'OrphanedMembers',
    1,
    'Warning',
    '{"ExcludeBaseMembers": true, "RequireAtLeastOneParent": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "ExcludeBaseMembers": true,
  "RequireAtLeastOneParent": true
}
```

### 7. Circular References Validation

Detects circular parent-child relationships in hierarchies.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    7,
    'Circular Reference Detection',
    'Detects circular parent-child relationships in hierarchies',
    'CostCenter',
    3,
    'CircularReferences',
    1,
    'Error',
    '{"MaxDepth": 50, "CheckAllHierarchies": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "MaxDepth": 50,
  "CheckAllHierarchies": true
}
```

### 8. Invalid Parent-Child Validation

Validates parent-child relationships against business rules.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    8,
    'Parent-Child Type Validation',
    'Ensures base members cannot have children',
    'Product',
    4,
    'InvalidParentChild',
    1,
    'Error',
    '{"Rules": [{"ParentType": "Base", "ChildType": "Any", "Allowed": false}]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "Rules": [
    {
      "ParentType": "Summary",
      "ChildType": "Base",
      "AllowedTypes": ["Base", "Summary"]
    },
    {
      "ParentType": "Base",
      "ChildType": "Any",
      "Allowed": false
    }
  ]
}
```

### 9. Custom SQL Validation

Executes custom SQL queries for specialized validation scenarios.

```sql
INSERT INTO MDM_Validation_Config (
    Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
    Validation_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    9,
    'Custom Name Length Check',
    'Validates member names do not exceed 50 characters',
    'Account',
    1,
    'CustomSQL',
    1,
    'Warning',
    '{"SQLQuery": "SELECT m.Name FROM Member m WHERE m.DimID = @DimID AND LEN(m.Name) > 50", "ErrorMessage": "Member name exceeds maximum length"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**Config_JSON Schema:**
```json
{
  "SQLQuery": "SELECT m.Name FROM Member m WHERE m.DimID = @DimID AND [condition]",
  "ErrorMessage": "Custom error message to display"
}
```

## Execution Workflow

### Creating a Validation Run

1. **Prepare Run Metadata:**
```sql
INSERT INTO MDM_Validation_Run (
    Run_ID, Run_Date, Run_User, Dim_Type, Dim_ID,
    Total_Validations, Total_Members_Checked, Total_Failures, Total_Warnings,
    Execution_Time_Ms, Status, Notes, Create_Date
)
VALUES (
    1, GETDATE(), 'admin', 'Account', 1,
    5, 0, 0, 0, 
    0, 'InProgress', 'Scheduled validation run', GETDATE()
);
```

2. **Execute Validations:**
   - Retrieve active validation configs for the dimension
   - For each config, execute the validation logic
   - Record results for each failed member

3. **Insert Results:**
```sql
INSERT INTO MDM_Validation_Result (
    Validation_Config_ID, Run_ID, Run_Date, Run_User,
    Member_Name, Member_ID, Validation_Status, 
    Error_Message, Error_Details, Create_Date
)
VALUES (
    1, 1, GETDATE(), 'admin',
    'A1000', 12345, 'Failed',
    'Member not found in required hierarchy: Total Company',
    'Member A1000 exists in Legal Entity hierarchy but is missing from Total Company hierarchy',
    GETDATE()
);
```

4. **Update Run Summary:**
```sql
UPDATE MDM_Validation_Run
SET Total_Members_Checked = 1000,
    Total_Failures = 15,
    Total_Warnings = 8,
    Execution_Time_Ms = 2500,
    Status = 'Completed'
WHERE Run_ID = 1;
```

## Dashboard Configuration UI Usage

### Configuring Validations

1. **Access Configuration Dashboard:**
   - Navigate to Model Dimension Manager
   - Select Validation Configuration section

2. **Create Validation Config:**
   - Click "New Validation"
   - Enter validation details:
     - Name (descriptive identifier)
     - Description (purpose and scope)
     - Select Dimension Type
     - Select Dimension
     - Choose Validation Type from dropdown
     - Set Severity (Error/Warning/Info)
     - Enable/disable with Is_Active checkbox
     - Configure validation-specific JSON settings

3. **Configure Validation-Specific Settings:**
   - Based on validation type selected
   - JSON editor for Config_JSON field
   - Use examples above as templates
   - Validate JSON syntax before saving

4. **Save and Activate:**
   - Save configuration
   - Set Is_Active = 1 to enable
   - Validation will be included in next run

### Running Validations

1. **Execute Validation Run:**
   - Select dimension (or all dimensions)
   - Click "Run Validations"
   - System creates Run_ID and begins execution
   - Progress displayed in UI

2. **View Results:**
   - Navigate to Validation Results section
   - Filter by Run_ID to see specific run
   - Filter by Validation_Config_ID to see specific validation
   - View member-level details with error messages

3. **Review Summary:**
   - Access Validation Runs section
   - View run history with summary statistics
   - Identify trends over time
   - Download results for reporting

## Data Retention and Maintenance

### Archiving Old Results

The system supports automatic cleanup of historical results:

```csharp
// Delete results older than 90 days
var adapter = new SQA_MDM_Validation_Config(si, connection);
int deletedCount = adapter.Delete_Old_Validation_Results(si, 90);
```

### Deleting Specific Run Results

```csharp
// Delete all results for a specific run
var adapter = new SQA_MDM_Validation_Config(si, connection);
int deletedCount = adapter.Delete_Validation_Results_By_Run_ID(si, runID);
```

## Error Handling

All operations include try-catch blocks with proper error logging using:
```csharp
ErrorHandler.LogWrite(si, new XFException(si, ex))
```

## Security Considerations

### Protecting Validation Configurations

1. **Access Control:**
   - Restrict direct database access to validation tables
   - Implement row-level security if multiple tenants share tables
   - Validate user permissions before allowing configuration changes
   - Use stored procedures for all validation operations

2. **Audit Trail:**
   - All configuration changes tracked via Create_User/Update_User
   - All validation runs tracked with Run_User
   - Implement additional audit logging for sensitive operations

3. **SQL Injection Prevention:**
   - Validate Config_JSON before saving
   - For CustomSQL validation type:
     - Validate SQL syntax
     - Restrict to SELECT statements only
     - Implement query timeout limits
     - Use parameterized queries where possible
     - Log all query executions

4. **Database Connection Security:**
   - Use named connections for database validations
   - Never store credentials in Config_JSON
   - Reference secure OneStream connections
   - Implement minimum required permissions

5. **Best Practices:**
   - Never log or display Config_JSON in plain text if it contains sensitive data
   - Use masked values in UI displays
   - Implement separate read/write permissions for sensitive fields
   - Review custom SQL queries for security vulnerabilities
   - Regularly audit validation configurations

## Performance Considerations

### Bulk Insert for Results

The system uses SqlBulkCopy for efficient result insertion:
- Batch size: 1000 rows
- Optimized for large validation runs
- Transactional integrity maintained

### Indexing Strategy

Multiple indexes optimize common queries:
- Validation Config: Dim_Type, Dim_ID, Is_Active
- Validation Result: Config_ID, Run_ID, Run_Date, Member_Name
- Validation Run: Run_Date, Dim_Type/Dim_ID

### Query Optimization

- Use TOP N for recent runs queries
- Filter early with WHERE clauses
- Join only necessary tables
- Use appropriate indexes for filtering

## Future Enhancements

Potential future additions:
- Scheduled automatic validation runs
- Email notifications for validation failures
- Validation result trending and analytics
- Configurable auto-remediation actions
- Integration with approval workflows
- Real-time validation during dimension maintenance
- Validation rule templates library
- Export validation results to Excel
- Validation dashboard with charts and KPIs
- Validation comparison between runs
- Batch validation configuration import/export

## Related Documentation

- See `README_CDC_Config.md` for Change Data Capture configuration
- See `/scripts/MDM_Validation_Table_Schema.sql` for complete schema
- See AI Coding Guide for OneStream development patterns
