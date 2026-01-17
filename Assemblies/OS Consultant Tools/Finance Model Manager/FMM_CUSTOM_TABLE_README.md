# FMM Custom Table Configuration System

## Overview

The FMM Custom Table Configuration System provides a comprehensive, metadata-driven approach to defining and managing custom table structures within the Finance Model Manager (FMM) module. This system allows you to configure tables similar to the existing `Register_Plan` and `Register_Plan_Details` tables with full control over:

- Table structure and columns
- Primary keys (clustered or non-clustered)
- Custom indexes
- Foreign key constraints
- Audit table generation
- Extensible table hierarchies

## Problem Statement

The original requirement was:

> "For the FMM Module, can you please provide me with a proposed table structure that would allow custom tables to be configured for different processes. The tables should be similar to Reg_Plan and Reg_Plan_Detail and each set of these table structures would need to have its own Audit table. For the Reg_Plan and Reg_Plan_Detail tables, I would like to provide flexibility to define primary key, define which index is clustered and allow indexes for non primary key items to be defined. I would also like to allow FK constraints and the ability to define new custom tables that are linked to the core Reg_Plan and Reg_Plan Detail tables."

## Solution Components

### 1. Database Schema (`FMM_Table_Config_DDL.sql`)

Seven configuration tables that store metadata about custom tables:

- **FMM_Table_Config** - Core table definitions
- **FMM_Table_Column_Config** - Column definitions with data types
- **FMM_Table_Index_Config** - Index definitions (PK, clustered, non-clustered)
- **FMM_Table_Index_Column_Config** - Index column mappings
- **FMM_Table_FK_Config** - Foreign key definitions
- **FMM_Table_FK_Column_Config** - FK column mappings
- **FMM_Table_Audit_Config** - Audit configuration

Three helper views for easy querying:
- **vw_FMM_Table_Structure** - Complete table/column structure
- **vw_FMM_Table_Indexes** - All indexes with columns
- **vw_FMM_Table_ForeignKeys** - All foreign keys with relationships

### 2. Sample Implementation (`FMM_Table_Config_Sample_Data.sql`)

Complete example showing how to configure the existing Register_Plan tables using the new system:
- Master table (Register_Plan) with 55 columns
- Detail table (Register_Plan_Details) with time-phased data
- Primary keys (clustered)
- Non-clustered indexes for workflow columns
- Foreign key with CASCADE
- Audit table linkage

### 3. C# Helper Class (`FMM_Table_Config_Helper.cs`)

Provides programmatic access to table configurations with methods for:
- **Reading Configuration**: Get tables, columns, indexes, foreign keys
- **DDL Generation**: Generate CREATE TABLE, CREATE INDEX, ALTER TABLE statements
- **Validation**: Validate configuration integrity
- **Complete Process Generation**: Generate all DDL for a process type

### 4. Documentation

- **FMM_CUSTOM_TABLE_PROPOSAL.md** - Detailed design specification with examples
- **FMM_CUSTOM_TABLE_QUICKSTART.md** - Step-by-step implementation guide
- **FMM_CUSTOM_TABLE_README.md** (this file) - Overview and getting started

## Key Features

### ✅ Flexible Table Definition
- Define any number of custom tables per process
- Support for Master, Detail, Extension, and Audit table types
- Hierarchical table relationships (parent-child)

### ✅ Complete Index Control
- Define primary keys (clustered or non-clustered)
- Multiple non-clustered indexes
- Composite indexes (multi-column)
- Covering indexes with included columns
- Control over sort direction (ASC/DESC)
- Fill factor specification

### ✅ Foreign Key Management
- Single or composite foreign keys
- Configurable CASCADE behaviors (DELETE/UPDATE)
- Cross-table references
- Self-referencing tables (hierarchies)

### ✅ Audit Trail Support
- Automatic audit table configuration
- Track INSERT, UPDATE, DELETE operations
- Configurable audit columns
- Linked to source tables

### ✅ Metadata-Driven
- All structure defined in configuration tables
- No code changes required for new tables
- Dynamic DDL generation from metadata
- Version control friendly

### ✅ SQL Server Data Type Support
All standard SQL Server data types:
- Numeric: INT, BIGINT, DECIMAL, NUMERIC, MONEY, FLOAT, etc.
- String: NVARCHAR, VARCHAR, CHAR, TEXT
- Date/Time: DATETIME, DATETIME2, DATE, TIME
- Binary: BINARY, VARBINARY, IMAGE
- Special: UNIQUEIDENTIFIER, XML, BIT

## Getting Started

### Installation

1. **Execute the DDL script**:
   ```sql
   -- Creates all configuration tables and views
   -- File: FMM_Table_Config_DDL.sql
   ```

2. **Optionally run sample data**:
   ```sql
   -- Configures Register_Plan tables as example
   -- File: FMM_Table_Config_Sample_Data.sql
   ```

3. **Add helper class to your project**:
   - Copy `FMM_Table_Config_Helper.cs` to your FMM_Shared_Assembly

### Quick Example: Creating a Custom Table

```sql
-- 1. Create table configuration
INSERT INTO FMM_Table_Config (Process_Type, Table_Name, Table_Type, Enable_Audit)
VALUES ('My_Process', 'My_Custom_Table', 'Master', 1);

DECLARE @TableID INT = SCOPE_IDENTITY();

-- 2. Define columns
INSERT INTO FMM_Table_Column_Config (Table_Config_ID, Column_Name, Data_Type, Is_Nullable, Column_Order)
VALUES 
    (@TableID, 'ID', 'UNIQUEIDENTIFIER', 0, 1),
    (@TableID, 'Name', 'NVARCHAR', 1, 2),
    (@TableID, 'Amount', 'DECIMAL', 1, 3);

-- 3. Define primary key
INSERT INTO FMM_Table_Index_Config (Table_Config_ID, Index_Name, Index_Type, Is_Clustered, Is_Unique)
VALUES (@TableID, 'PK_My_Table', 'PRIMARY_KEY', 1, 1);

-- 4. Generate DDL using helper class
-- See FMM_CUSTOM_TABLE_QUICKSTART.md for details
```

## Usage Examples

### Using C# Helper Class

```csharp
using (var si = /* SessionInfo */)
{
    var helper = new FMM_Table_Config_Helper(si);
    
    // Get all tables for a process
    var tables = helper.GetTableConfigurations("Asset_Register");
    
    // Generate complete DDL
    string ddl = helper.GenerateCompleteDDL("Asset_Register");
    
    // Execute DDL to create physical tables
    using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
    {
        BRApi.Database.ExecuteSql(dbConn, ddl, false);
    }
    
    // Validate configuration
    var errors = helper.ValidateTableConfiguration(tableConfigId);
    if (errors.Any())
    {
        // Handle validation errors
        foreach (var error in errors)
        {
            BRApi.ErrorLog.LogMessage(si, $"Error: {error}");
        }
    }
}
```

### Query Configuration Using Views

```sql
-- See all tables in a process
SELECT * FROM vw_FMM_Table_Structure
WHERE Process_Type = 'Register_Plan';

-- See all indexes
SELECT * FROM vw_FMM_Table_Indexes
WHERE Table_Name = 'Register_Plan';

-- See all foreign keys
SELECT * FROM vw_FMM_Table_ForeignKeys;
```

## Configuration Workflow

```
┌─────────────────────────┐
│ 1. Design Table         │
│    Structure            │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ 2. Insert Config        │
│    - Table              │
│    - Columns            │
│    - Indexes            │
│    - Foreign Keys       │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ 3. Validate Config      │
│    (C# Helper)          │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ 4. Generate DDL         │
│    (C# Helper)          │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ 5. Execute DDL          │
│    Create Physical      │
│    Tables               │
└───────────┬─────────────┘
            │
            ▼
┌─────────────────────────┐
│ 6. Build UI/Logic       │
│    - SQL Adapters       │
│    - Dashboards         │
│    - Business Rules     │
└─────────────────────────┘
```

## Architecture

### Table Relationships

```
FMM_Table_Config (1) ──────< (*) FMM_Table_Column_Config
        │
        │ (1)
        │
        ├──────< (*) FMM_Table_Index_Config
        │               │
        │               │ (1)
        │               │
        │               └───< (*) FMM_Table_Index_Column_Config
        │
        ├──────< (*) FMM_Table_FK_Config (Source)
        │               │
        │               │ (1)
        │               │
        │               └───< (*) FMM_Table_FK_Column_Config
        │
        ├──────< (*) FMM_Table_FK_Config (Target)
        │
        └──────< (1) FMM_Table_Audit_Config
```

### Table Type Hierarchy

```
Process_Type: "Asset_Register"
    │
    ├── Master Table: "Asset_Register_Master"
    │   ├── Columns (defined)
    │   ├── Primary Key (clustered)
    │   ├── Indexes (non-clustered)
    │   └── Audit Table ──> "Asset_Register_Master_Audit"
    │
    ├── Detail Table: "Asset_Depreciation_Detail"
    │   ├── Columns (defined)
    │   ├── Primary Key (clustered)
    │   ├── Foreign Key ──> Master Table
    │   └── Audit Table ──> "Asset_Depreciation_Detail_Audit"
    │
    └── Extension Table: "Asset_Additional_Attributes"
        ├── Columns (defined)
        ├── Primary Key (clustered)
        ├── Foreign Key ──> Master Table
        └── Audit Table ──> "Asset_Additional_Attributes_Audit"
```

## Benefits

### For Developers
- **Rapid Development**: Create new table structures without writing DDL
- **Consistency**: All tables follow same configuration pattern
- **Version Control**: Configuration stored in database, easily exported
- **Validation**: Built-in validation prevents configuration errors
- **Documentation**: Configuration is self-documenting

### For Business Users
- **Flexibility**: New processes can be added without code deployment
- **Auditability**: All changes tracked through configuration tables
- **Transparency**: Easy to see table structure through views
- **Maintenance**: Easier to modify table structures

### For System Administrators
- **Standardization**: All custom tables follow same patterns
- **Migration**: Easy to migrate configurations between environments
- **Backup**: Configuration metadata is backed up with database
- **Monitoring**: Can query configuration to understand system structure

## Best Practices

1. **Always include workflow columns** for FMM integration:
   - WF_Scenario_Name, WF_Profile_Name, WF_Time_Name, Activity_ID

2. **Always include audit columns**:
   - Create_Date, Create_User, Update_Date, Update_User

3. **Use consistent naming**:
   - Tables: `[Process]_[Entity]_[Type]`
   - Indexes: `PK_[Table]` or `IX_[Table]_[Columns]`
   - Foreign Keys: `FK_[Source]_[Target]`

4. **One clustered index per table** (usually the primary key)

5. **Index strategically**:
   - Foreign key columns
   - Frequently queried columns
   - Workflow columns

6. **Validate before deploying**:
   ```csharp
   var errors = helper.ValidateTableConfiguration(tableConfigId);
   ```

7. **Test in development first**

8. **Document your processes**:
   - Use Description fields in configuration
   - Maintain process-specific documentation

## Common Use Cases

### Use Case 1: Multi-Year Planning Tables
Create tables similar to Register_Plan for different planning cycles with time-phased data.

### Use Case 2: Asset Management
Track assets with master data and depreciation schedules in detail tables.

### Use Case 3: Budget Allocation
Create hierarchical structures for budget allocation across dimensions.

### Use Case 4: Custom Registers
Define process-specific registers with custom attributes and validations.

### Use Case 5: Extension Tables
Add custom attributes to existing processes without modifying core tables.

## Troubleshooting

### Common Issues

**Issue**: Multiple clustered indexes error
**Solution**: Ensure only one index per table has `Is_Clustered = 1`

**Issue**: Foreign key validation fails
**Solution**: Verify target table/columns exist and have unique constraint

**Issue**: DDL generation fails
**Solution**: Check all required columns are defined with proper data types

**Issue**: Cannot create table
**Solution**: Verify no table with same name exists; check permissions

## Performance Considerations

1. **Clustered Index Choice**: Choose the clustered index based on query patterns
   - Usually primary key for OLTP
   - Consider date/time for range queries

2. **Index Coverage**: Use covering indexes for frequently executed queries

3. **Fill Factor**: Set appropriate fill factor for tables with frequent updates

4. **Foreign Key Cascade**: Use CASCADE carefully as it can impact performance

5. **Audit Tables**: Consider partitioning audit tables for large volumes

## Migration & Deployment

### Export Configuration
```sql
-- Export configuration for a process
SELECT * FROM FMM_Table_Config WHERE Process_Type = 'Asset_Register';
SELECT * FROM FMM_Table_Column_Config WHERE Table_Config_ID IN (...);
-- Export other related config tables
```

### Import to Another Environment
1. Export configuration as INSERT statements
2. Execute in target environment
3. Validate configuration
4. Generate and execute DDL

## Future Enhancements

Potential future improvements:
- UI for visual table designer
- Automatic SQL Adapter generation
- Table versioning support
- Impact analysis tools
- Configuration comparison across environments
- Automated migration scripts
- Template library for common patterns

## Support & Documentation

### Files in This Package
- `FMM_Table_Config_DDL.sql` - Database schema
- `FMM_Table_Config_Sample_Data.sql` - Example implementation
- `FMM_Table_Config_Helper.cs` - C# helper class
- `FMM_CUSTOM_TABLE_PROPOSAL.md` - Detailed design document
- `FMM_CUSTOM_TABLE_QUICKSTART.md` - Step-by-step guide
- `FMM_CUSTOM_TABLE_README.md` - This file

### Getting Help
1. Review the proposal document for design details
2. Follow the quick start guide for step-by-step instructions
3. Examine the sample data script for working examples
4. Check validation errors using the helper class

## License & Credits

Part of the OneStream_Speed FMM Module.
Created in response to custom table configuration requirements.

---

**Version**: 1.0
**Last Updated**: 2026-01-17
**Status**: Ready for Implementation
