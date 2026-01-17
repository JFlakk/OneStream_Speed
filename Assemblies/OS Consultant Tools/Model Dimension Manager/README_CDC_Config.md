# Model Dimension Manager - CDC Configuration

## Overview

The Change Data Capture (CDC) Configuration feature in Model Dimension Manager allows users to configure automated imports of dimension members from various data sources through the Dashboard Config UI.

## Features

### Supported Source Types
1. **SQL** - Query data from SQL databases
2. **API** - Retrieve data from REST APIs
3. **Flat File** - Import data from CSV or other flat files

### Configurable Elements
- **DimType** - The dimension type to import members into
- **Dimension** - The specific dimension within the dimension type
- **Source Type** - SQL, API, or Flat File
- **Source Configuration** - Connection details, query/endpoint, or file path
- **Column Mappings** - Map source data columns to member properties:
  - Name
  - Description
  - Text1 through Text8

## Database Schema

The CDC configuration is stored in the `MDM_CDC_Config` table. See `/scripts/MDM_CDC_Config_Table_Schema.sql` for the complete schema definition.

### Key Fields
- `CDC_Config_ID` - Primary key (auto-increment)
- `DimTypeID` - Foreign key to DimType table
- `DimID` - Foreign key to Dim table
- `SourceType` - Type of data source (SQL, API, Flat File)
- `SourceConfig` - JSON/XML configuration for the source
- `Map_Name` - Source column for member name
- `Map_Description` - Source column for member description
- `Map_Text1` through `Map_Text8` - Source columns for text properties
- Audit fields: Create_Date, Create_User, Modify_Date, Modify_User

## Implementation Components

### 1. Dashboard DataSets (`MDM_DB_DataSets.cs`)

Located in: `MDM_Config_UI_Assembly/DB DataSets/`

#### Available DataSets:
- **Get_DimTypeList** - Returns all dimension types
  - Columns: DimTypeID, Name
  
- **Get_DimList** - Returns dimensions for a specific dimension type
  - Parameters: dimTypeID
  - Columns: DimID, Name
  
- **Get_CDC_Source_Types** - Returns available source types
  - Columns: SourceTypeID, SourceTypeName
  - Values: (1, 'SQL'), (2, 'API'), (3, 'Flat File')
  
- **Get_CDC_Config** - Returns all CDC configurations
  - Columns: All fields from MDM_CDC_Config table
  
- **Get_Member_Properties** - Returns available member properties for mapping
  - Columns: PropertyName, PropertyDisplayName
  - Values: Name, Description, Text1-Text8

### 2. SQL Adapter (`MDM_CDC_Config.cs`)

Located in: `MDM_Config_UI_Assembly/SQL Adapters/`

#### Available Methods:
- **Fill_MDM_CDC_Config_DT** - Fills a DataTable with CDC config data
- **Update_MDM_CDC_Config** - Saves changes to CDC configuration
- **Get_CDC_Config_By_ID** - Retrieves a specific config by ID
- **Get_CDC_Config_By_Dimension** - Retrieves config for a specific dimension

## Usage Examples

### SQL Source Configuration
```json
{
  "connectionString": "Server=myserver;Database=mydb;",
  "query": "SELECT Code, Name, Description, Category FROM SourceTable"
}
```

### API Source Configuration
```json
{
  "endpoint": "https://api.example.com/dimensions",
  "method": "GET",
  "headers": {
    "Authorization": "Bearer token123"
  }
}
```

### Flat File Source Configuration
```json
{
  "filePath": "/data/dimensions.csv",
  "delimiter": ",",
  "hasHeader": true
}
```

## Dashboard Configuration UI

To configure CDC in the Dashboard Config UI:

1. Navigate to the Model Dimension Manager configuration dashboard
2. Select the Dimension Type from the dropdown (populated by Get_DimTypeList)
3. Select the Dimension (populated by Get_DimList based on selected DimType)
4. Choose the Source Type (SQL, API, or Flat File)
5. Configure the source details in the SourceConfig field
6. Map source columns to member properties:
   - Select source column name for Name mapping
   - Select source column name for Description mapping
   - Map additional Text1-Text8 properties as needed
7. Save the configuration

## Data Flow

1. **Configuration** - User configures CDC settings through Dashboard UI
2. **Storage** - Configuration saved to MDM_CDC_Config table
3. **Execution** - CDC process reads configuration and fetches data from source
4. **Mapping** - Source columns mapped to member properties based on configuration
5. **Import** - Members created/updated in the target dimension

## Error Handling

All operations include try-catch blocks with proper error logging using:
```csharp
ErrorHandler.LogWrite(si, new XFException(si, ex))
```

## Security Considerations

### Protecting Sensitive Data in SourceConfig

The `SourceConfig` field stores connection strings, API keys, and other sensitive information. To protect this data:

1. **Encryption at Rest**: 
   - Consider using SQL Server's Transparent Data Encryption (TDE) for the entire database
   - Alternatively, implement column-level encryption using SQL Server's Always Encrypted feature
   - Example using Always Encrypted:
   ```sql
   ALTER TABLE MDM_CDC_Config
   ALTER COLUMN SourceConfig ADD ENCRYPTED WITH (
       COLUMN_ENCRYPTION_KEY = MyCEK,
       ENCRYPTION_TYPE = DETERMINISTIC,
       ALGORITHM = 'AEAD_AES_256_CBC_HMAC_SHA_256'
   );
   ```

2. **Application-Level Encryption**:
   - Encrypt sensitive values before storing in SourceConfig
   - Use OneStream's built-in encryption APIs if available
   - Store encryption keys in a secure key vault (Azure Key Vault, AWS KMS, etc.)
   - Example pattern:
   ```csharp
   // Before saving
   string encryptedConfig = EncryptionHelper.Encrypt(sourceConfig, encryptionKey);
   
   // When retrieving
   string decryptedConfig = EncryptionHelper.Decrypt(encryptedConfig, encryptionKey);
   ```

3. **Secure Configuration Storage**:
   - For SQL sources: Store connection strings in OneStream database connections, reference by name
   - For APIs: Use token-based authentication with short-lived tokens when possible
   - For file paths: Use network shares with proper NTFS permissions

4. **Access Control**:
   - Restrict direct database access to MDM_CDC_Config table
   - Implement row-level security if multiple tenants/applications share the table
   - Use stored procedures for all CDC configuration operations
   - Validate user permissions before allowing CDC configuration changes

5. **Audit Trail**:
   - All configuration changes are tracked via Create_User/Modify_User fields
   - Consider implementing additional audit logging for sensitive operations
   - Log access to SourceConfig field separately for security monitoring

6. **Best Practices**:
   - Never log or display SourceConfig values in plain text
   - Use masked values in UI displays (show only last 4 characters, etc.)
   - Implement separate read/write permissions for sensitive fields
   - Regularly rotate API keys and credentials stored in SourceConfig
   - Use service accounts with minimum required permissions for CDC operations

## Future Enhancements

Potential future additions:
- Scheduled CDC execution
- Delta/incremental loads
- Data validation rules
- Transformation logic
- Error notifications
- Import history tracking
