# Model Dimension Manager - CDC Configuration

## Overview

The Change Data Capture (CDC) Configuration feature in Model Dimension Manager allows users to configure automated imports of dimension members from various data sources through the Dashboard Config UI. The configuration uses a master-detail table structure to support flexible column mappings including varying by scenario and time.

## Database Schema

The CDC configuration uses two related tables:

### MDM_CDC_Config (Master Table)
The main configuration table stores overall CDC settings for dimension imports.

**Key Fields:**
- `CDC_Config_ID` - Primary key (INT, NOT NULL)
- `Name` - Descriptive name for the configuration (NVARCHAR(100))
- `Dim_Type` - The dimension type name (NVARCHAR(50), NOT NULL)
- `Dim_ID` - The specific dimension ID (INT, NOT NULL)
- `Src_Connection` - Named connection or connection identifier (NVARCHAR(200))
- `Src_SQL_String` - SQL query to retrieve source data (NVARCHAR(MAX))
- `Dim_Mgmt_Process` - Dimension management process configuration (NVARCHAR(MAX))
- `Trx_Rule` - Transformation rule configuration (NVARCHAR(MAX))
- `Appr_ID` - Approval/Approver ID reference (INT)
- `Mbr_PrefSuff` - Member prefix/suffix identifier (NVARCHAR(20))
- `Mbr_PrefSuff_Txt` - Member prefix/suffix text template (NVARCHAR(MAX))
- Audit fields: `Create_Date`, `Create_User`, `Update_Date`, `Update_User`

### MDM_CDC_Config_Detail (Detail Table)
Stores column mapping details for each CDC configuration, supporting varying by scenario and time.

**Key Fields:**
- `CDC_Config_ID` - Foreign key to MDM_CDC_Config (INT, NOT NULL)
- `CDC_Config_Detail_ID` - Detail line identifier (INT, NOT NULL)
- `OS_Mbr_Column` - OneStream member property to populate (NVARCHAR(100))
- `OS_Mbr_Vary_Scen_Column` - OneStream scenario-varying property (NVARCHAR(100))
- `OS_Mbr_Vary_Time_Column` - OneStream time-varying property (NVARCHAR(100))
- `Src_Mbr_Column` - Source column for member property (NVARCHAR(100))
- `Src_Vary_Scen_Column` - Source column for scenario-varying data (NVARCHAR(100))
- `Src_Vary_Time_Column` - Source column for time-varying data (NVARCHAR(100))
- Audit fields: `Create_Date`, `Create_User`, `Update_Date`, `Update_User`

See `/scripts/MDM_CDC_Config_Table_Schema.sql` for the complete schema definition.

**Important Notes:**
- **CDC_Config_ID Management**: The CDC_Config_ID field is NOT an IDENTITY column. IDs must be manually assigned or managed through application logic. This allows for controlled ID assignment and potential synchronization across environments.
- **Audit Fields**: When inserting or updating records, the application must set Create_Date, Create_User, Update_Date, and Update_User fields appropriately. The Update_MDM_CDC_Config and Update_MDM_CDC_Config_Detail methods expect these fields to be populated in the DataTable before calling the update.

## Features

### Master Configuration
- **Configuration Naming** - Assign descriptive names to CDC configurations
- **Dimension Targeting** - Specify dimension type (as string) and dimension ID
- **Source Definition** - Define connection name and SQL query
- **Business Logic** - Configure dimension management process and transformation rules
- **Approval Integration** - Reference approval IDs for governance
- **Member Naming** - Configure prefix/suffix patterns for member names

### Detail Mappings (Multiple per Configuration)
- **Standard Mapping** - Map source columns to OneStream member properties
- **Scenario Variation** - Support for properties that vary by scenario
- **Time Variation** - Support for properties that vary by time
- **Flexible Structure** - Multiple detail rows per configuration for complex mappings

### Configurable Elements
- **Dim_Type** - The dimension type name (string-based)
- **Dimension** - The specific dimension within the dimension type
- **Source Connection** - Named connection reference for security
- **SQL Query** - SQL statement to retrieve source data
- **Column Mappings** - Detailed mappings in the detail table supporting:
  - Standard member properties (Name, Description, Text1-8)
  - Scenario-varying properties
  - Time-varying properties

## Implementation Components

### 1. Dashboard DataSets (`MDM_DB_DataSets.cs`)

Located in: `MDM_Config_UI_Assembly/DB DataSets/` and `MDM_UI_Assembly/DB DataSets/`

#### Available DataSets:
- **Get_DimTypeList** - Returns all dimension types
  - Columns: DimTypeID, Name
  
- **Get_DimList** - Returns dimensions for a specific dimension type
  - Parameters: dimTypeID
  - Columns: DimID, Name
  
- **Get_CDC_Config** - Returns all CDC configurations
  - Columns: CDC_Config_ID, Name, Dim_Type, Dim_ID, Src_Connection, Src_SQL_String, 
    Dim_Mgmt_Process, Trx_Rule, Appr_ID, Mbr_PrefSuff, Mbr_PrefSuff_Txt,
    Create_Date, Create_User, Update_Date, Update_User

- **Get_CDC_Config_Detail** - Returns detail mappings for CDC configurations
  - Optional Parameters: cdcConfigID (to filter by specific config)
  - Columns: CDC_Config_ID, CDC_Config_Detail_ID, OS_Mbr_Column, 
    OS_Mbr_Vary_Scen_Column, OS_Mbr_Vary_Time_Column, Src_Mbr_Column,
    Src_Vary_Scen_Column, Src_Vary_Time_Column, Create_Date, Create_User, 
    Update_Date, Update_User
  
- **Get_Member_Properties** - Returns available member properties for mapping
  - Columns: PropertyName, PropertyDisplayName
  - Values: Name, Description, Text1-Text8

### 2. SQL Adapter (`MDM_CDC_Config.cs`)

Located in: `MDM_Config_UI_Assembly/SQL Adapters/`

#### Available Methods:

**Master Table Methods:**
- **Fill_MDM_CDC_Config_DT** - Fills a DataTable with CDC config data
- **Update_MDM_CDC_Config** - Saves changes to CDC configuration (master table)
- **Get_CDC_Config_By_ID** - Retrieves a specific config by ID
- **Get_CDC_Config_By_Dimension** - Retrieves config for a specific dimension (by Dim_Type and Dim_ID)

**Detail Table Methods:**
- **Fill_MDM_CDC_Config_Detail_DT** - Fills a DataTable with CDC detail data
- **Update_MDM_CDC_Config_Detail** - Saves changes to CDC detail mappings
- **Get_CDC_Config_Detail_By_Config_ID** - Retrieves all detail rows for a specific config

### 3. Dashboard Extender Save Methods (`MDM_Config_Data.cs`)

Located in: `MDM_Svc_Factory_Assembly/DB Extenders/`

The Dashboard Extender provides save/update functionality for CDC configurations through the OneStream Dashboard UI. These methods handle insert, update, and delete operations with automatic ID generation and audit field management.

#### Available Methods:

**Save_CDC_Config**
- **Function Type**: `SqlTableEditorSaveData`
- **Function Name**: `Save_CDC_Config`
- **Purpose**: Saves CDC configuration master records to the MDM_CDC_Config table
- **Operations Supported**:
  - **Insert**: Creates new CDC configuration with auto-generated CDC_Config_ID
  - **Update**: Updates existing CDC configuration with audit trail
  - **Delete**: Removes CDC configuration (cascade deletes detail records)
- **Features**:
  - Automatic ID generation using `SQL_GBL_Get_Max_ID` for new records
  - Audit field population (Create_Date, Create_User, Update_Date, Update_User)
  - Transaction-based updates for data integrity
  - Returns XFSqlTableEditorSaveDataTaskResult with CancelDefaultSave = true

**Save_CDC_Config_Detail**
- **Function Type**: `SqlTableEditorSaveData`
- **Function Name**: `Save_CDC_Config_Detail`
- **Purpose**: Saves CDC configuration detail records to the MDM_CDC_Config_Detail table
- **Operations Supported**:
  - **Insert**: Creates new detail mapping with auto-generated CDC_Config_Detail_ID
  - **Update**: Updates existing detail mapping with audit trail
  - **Delete**: Removes detail mapping
- **Parameters**:
  - Requires `IV_MDM_CDC_Config_ID` custom substitution variable (parent config ID)
- **Features**:
  - Automatic detail ID generation for new records
  - Composite primary key support (CDC_Config_ID, CDC_Config_Detail_ID)
  - Parent-child relationship enforcement
  - Audit field population
  - Transaction-based updates for data integrity

#### Usage in Dashboard Configuration:

To use these save methods in a OneStream Dashboard:

1. **Master Configuration Table Editor:**
   ```
   SQL Table Editor Component:
   - Function Name: Save_CDC_Config
   - Table: MDM_CDC_Config
   - Columns: Name, Dim_Type, Dim_ID, Src_Connection, Src_SQL_String, 
             Dim_Mgmt_Process, Trx_Rule, Appr_ID, Mbr_PrefSuff, Mbr_PrefSuff_Txt
   ```

2. **Detail Configuration Table Editor:**
   ```
   SQL Table Editor Component:
   - Function Name: Save_CDC_Config_Detail
   - Table: MDM_CDC_Config_Detail
   - Custom Substitution Variables: IV_MDM_CDC_Config_ID (parent config ID)
   - Columns: OS_Mbr_Column, OS_Mbr_Vary_Scen_Column, OS_Mbr_Vary_Time_Column,
             Src_Mbr_Column, Src_Vary_Scen_Column, Src_Vary_Time_Column
   ```

#### Example Workflow:

1. User opens CDC Configuration Dashboard
2. User enters data in the master configuration table editor
3. User clicks Save button
4. `Save_CDC_Config` method is invoked:
   - For new records: Generates new CDC_Config_ID
   - Populates all fields from the table editor
   - Sets audit fields (Create/Update Date/User)
   - Saves to MDM_CDC_Config table
5. User selects a CDC configuration to add detail mappings
6. User enters column mapping data in the detail table editor
7. User clicks Save button
8. `Save_CDC_Config_Detail` method is invoked:
   - Retrieves parent CDC_Config_ID from substitution variable
   - For new records: Generates new CDC_Config_Detail_ID
   - Populates all mapping fields
   - Sets audit fields
   - Saves to MDM_CDC_Config_Detail table

#### Error Handling:

Both save methods include comprehensive error handling:
- Try-catch blocks wrap all operations
- Errors are logged using `ErrorHandler.LogWrite(si, new XFException(si, ex))`
- Transaction rollback on failure (within UpdateTableSimple)
- User-friendly error messages returned in XFSqlTableEditorSaveDataTaskResult

## Usage Examples

### Master Configuration Example

**Create a basic CDC configuration:**
```sql
INSERT INTO MDM_CDC_Config (
    CDC_Config_ID, Name, Dim_Type, Dim_ID, 
    Src_Connection, Src_SQL_String,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1, 
    'Account Dimension Import',
    'Account',
    1,
    'SourceSystemConnection',
    'SELECT AccountCode, AccountName, AccountDesc, Category FROM ChartOfAccounts WHERE Active = 1',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

### Detail Mapping Examples

**Map source columns to standard member properties:**
```sql
-- Map account code to Name
INSERT INTO MDM_CDC_Config_Detail (
    CDC_Config_ID, CDC_Config_Detail_ID,
    OS_Mbr_Column, Src_Mbr_Column,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (1, 1, 'Name', 'AccountCode', GETDATE(), 'admin', GETDATE(), 'admin');

-- Map account name to Description
INSERT INTO MDM_CDC_Config_Detail (
    CDC_Config_ID, CDC_Config_Detail_ID,
    OS_Mbr_Column, Src_Mbr_Column,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (1, 2, 'Description', 'AccountName', GETDATE(), 'admin', GETDATE(), 'admin');

-- Map category to Text1
INSERT INTO MDM_CDC_Config_Detail (
    CDC_Config_ID, CDC_Config_Detail_ID,
    OS_Mbr_Column, Src_Mbr_Column,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (1, 3, 'Text1', 'Category', GETDATE(), 'admin', GETDATE(), 'admin');
```

**Map with scenario and time variations:**
```sql
-- Map with scenario-varying property
INSERT INTO MDM_CDC_Config_Detail (
    CDC_Config_ID, CDC_Config_Detail_ID,
    OS_Mbr_Column, OS_Mbr_Vary_Scen_Column,
    Src_Mbr_Column, Src_Vary_Scen_Column,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1, 4,
    'Text2', 'Text3',
    'BaseValue', 'ScenarioValue',
    GETDATE(), 'admin', GETDATE(), 'admin'
);

-- Map with time-varying property
INSERT INTO MDM_CDC_Config_Detail (
    CDC_Config_ID, CDC_Config_Detail_ID,
    OS_Mbr_Column, OS_Mbr_Vary_Time_Column,
    Src_Mbr_Column, Src_Vary_Time_Column,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1, 5,
    'Text4', 'Text5',
    'BaseStatus', 'TimeStatus',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

### Advanced Configuration Examples

**Using transformation rules:**
```sql
INSERT INTO MDM_CDC_Config (
    CDC_Config_ID, Name, Dim_Type, Dim_ID, 
    Src_Connection, Src_SQL_String,
    Trx_Rule, Dim_Mgmt_Process,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    2, 
    'Product with Transformations',
    'Product',
    2,
    'ProductDB',
    'SELECT * FROM Products',
    'UPPERCASE(Name); TRIM(Description)',
    'CreateIfNotExists',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

**With approval and member prefix/suffix:**
```sql
INSERT INTO MDM_CDC_Config (
    CDC_Config_ID, Name, Dim_Type, Dim_ID, 
    Src_Connection, Src_SQL_String,
    Appr_ID, Mbr_PrefSuff, Mbr_PrefSuff_Txt,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    3, 
    'Cost Center with Approval',
    'CostCenter',
    3,
    'HRSystem',
    'SELECT CC_Code, CC_Name FROM CostCenters',
    100,
    'Prefix',
    'CC_',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
```

## Dashboard Configuration UI

To configure CDC in the Dashboard Config UI:

1. **Master Configuration:**
   - Navigate to the Model Dimension Manager configuration dashboard
   - Enter a descriptive Name for the configuration
   - Enter the Dimension Type name (string value)
   - Select the Dimension ID from available dimensions
   - Enter the source connection name
   - Define the SQL query to retrieve source data
   - Optionally configure:
     - Dimension management process
     - Transformation rules
     - Approval ID
     - Member prefix/suffix settings
   - Save the master configuration

2. **Detail Mappings:**
   - For each column mapping needed:
     - Select or enter the CDC Config ID
     - Assign a unique Detail ID
     - Choose the OneStream member property (OS_Mbr_Column)
     - Map to the corresponding source column (Src_Mbr_Column)
     - If the property varies by scenario:
       - Specify the scenario-varying property (OS_Mbr_Vary_Scen_Column)
       - Specify the source scenario column (Src_Vary_Scen_Column)
     - If the property varies by time:
       - Specify the time-varying property (OS_Mbr_Vary_Time_Column)
       - Specify the source time column (Src_Vary_Time_Column)
     - Save the detail mapping
   - Repeat for additional column mappings

## Data Flow

1. **Configuration** - User configures CDC settings through Dashboard UI
   - Master configuration defines source and dimension target
   - Detail records define column mappings
2. **Storage** - Configuration saved to MDM_CDC_Config and MDM_CDC_Config_Detail tables
3. **Execution** - CDC process reads configuration and fetches data from source using SQL query
4. **Mapping** - Source columns mapped to member properties based on detail configuration
5. **Transformation** - Apply any transformation rules defined in Trx_Rule
6. **Management** - Apply dimension management process (create, update, etc.)
7. **Approval** - Route through approval process if Appr_ID is specified
8. **Import** - Members created/updated in the target dimension

## Error Handling

All operations include try-catch blocks with proper error logging using:
```csharp
ErrorHandler.LogWrite(si, new XFException(si, ex))
```

## Security Considerations

### Protecting Sensitive Data

#### Source Connection Security
The `Src_Connection` field stores connection names that should reference secure, named connections rather than embedding credentials directly:

1. **Use Named Connections**:
   - Store connection strings in OneStream's connection management
   - Reference connections by name in Src_Connection field
   - Example: `Src_Connection = 'FinanceDB'` instead of connection string

2. **Database-Level Encryption**:
   - Consider using SQL Server's Transparent Data Encryption (TDE) for the entire database
   - Alternatively, implement column-level encryption using SQL Server's Always Encrypted feature for sensitive fields
   - Example using Always Encrypted:
   ```sql
   ALTER TABLE MDM_CDC_Config
   ALTER COLUMN Src_Connection ADD ENCRYPTED WITH (
       COLUMN_ENCRYPTION_KEY = MyCEK,
       ENCRYPTION_TYPE = DETERMINISTIC,
       ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
   );
   ```

3. **Access Control**:
   - Restrict direct database access to MDM_CDC_Config and MDM_CDC_Config_Detail tables
   - Implement row-level security if multiple tenants/applications share the tables
   - Use stored procedures for all CDC configuration operations
   - Validate user permissions before allowing CDC configuration changes

4. **Audit Trail**:
   - All configuration changes are tracked via Create_User/Update_User fields
   - Consider implementing additional audit logging for sensitive operations
   - Log access to configuration fields separately for security monitoring

5. **Best Practices**:
   - Never log or display Src_Connection or Src_SQL_String values in plain text
   - Use masked values in UI displays
   - Implement separate read/write permissions for sensitive fields
   - Regularly review and rotate credentials referenced in named connections
   - Use service accounts with minimum required permissions for CDC operations
   - Review SQL queries in Src_SQL_String for SQL injection vulnerabilities

### SQL Injection Prevention
Since Src_SQL_String stores user-defined SQL queries:
- Validate SQL syntax before saving
- Consider implementing SQL query templates
- Restrict SQL operations to SELECT statements only
- Use parameterized queries where possible in the execution layer
- Implement query timeout limits
- Log all query executions for audit purposes

## Future Enhancements

Potential future additions:
- Scheduled CDC execution with configurable intervals
- Delta/incremental loads based on last run timestamp
- Data validation rules at the detail level
- Additional transformation logic support
- Error notifications and alerts
- Import history tracking with rollback capability
- Support for multiple source queries (join scenarios)
- Real-time CDC monitoring dashboard
- Conflict resolution strategies for scenario/time variations
