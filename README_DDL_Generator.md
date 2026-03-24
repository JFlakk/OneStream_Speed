# DDL Generation from Custom Table Configurations

This document explains how to use the FMM Table Configuration system to generate DDL scripts dynamically from database metadata.

## Overview

The Finance Model Manager includes a table configuration system that stores table metadata in the following tables:
- **FMM_Table_Config**: Master table configuration (table names, types, descriptions)
- **FMM_Table_Column_Config**: Column definitions (names, data types, constraints)
- **FMM_Table_Index_Config**: Index configurations
- **FMM_Table_Index_Column_Config**: Index column mappings
- **FMM_Table_FK_Config**: Foreign key configurations
- **FMM_Table_FK_Column_Config**: Foreign key column mappings

## New DDL Generation Code

### File: FMM_Generate_DDL_From_Config.cs

This file contains three classes for generating DDL from table configurations:

#### 1. MainClass (XFBR Function)
Call directly from business rules or other code:

```csharp
var generator = new MainClass();
string result = generator.Main(si, "FMM", "MyCustomTables.sql");
```

**Parameters:**
- `processType` (string): The process type to filter tables (e.g., "FMM", "DDM", "MDM")
- `outputFileName` (string, optional): Output file name (defaults to auto-generated name)

**Returns:** Status message with file location

**Output:** Saves DDL file to `Documents/Users/{username}/GeneratedDDL/`

#### 2. DashboardExtenderClass (Dashboard Integration)
Integrate into a dashboard for user-friendly DDL generation:

**Function Name:** `Generate_DDL_From_Config`

**Dashboard Parameters:**
- `IV_ProcessType`: Text input for process type
- `IV_OutputFileName`: Text input for output filename (optional)

**Example Dashboard Setup:**
1. Create a dashboard with text inputs for ProcessType and OutputFileName
2. Add a button that calls the Dashboard Extender function `Generate_DDL_From_Config`
3. Users can select process type and generate DDL files on demand

#### 3. TableDDLGenerator (Granular Control)
Generate DDL for specific tables programmatically:

```csharp
var generator = new TableDDLGenerator(si);

// Generate DDL for a single table by name
string ddl = generator.GenerateTableDDLByName("RegPlan", 
    includeIndexes: true, 
    includeForeignKeys: true);

// Generate DDL for multiple tables
var tables = new List<string> { "RegPlan", "RegPlan_Details" };
string multiDDL = generator.GenerateMultipleTablesDDL(tables);

// Generate DDL for a table by ConfigID
string ddl = generator.GenerateTableDDL(tableConfigId: 1);
```

## Usage Examples

### Example 1: Generate DDL for All FMM Tables

```csharp
// In an XFBR or Business Rule
var generator = new Workspace.__WsNamespacePrefix.__WsAssemblyName
    .BusinessRule.Extender.FMM_Generate_DDL_From_Config.MainClass();
    
string result = generator.Main(si, "FMM", null);
// Result: DDL file saved to Documents/Users/{username}/GeneratedDDL/DDL_FMM_Generated_20260324_223000.sql
```

### Example 2: Generate DDL for Specific Tables

```csharp
var generator = new TableDDLGenerator(si);

// Just the RegPlan table
string regPlanDDL = generator.GenerateTableDDLByName("RegPlan");

// Multiple related tables
var fmmTables = new List<string> { 
    "FMM_Calc_Config", 
    "FMM_Dest_Cell", 
    "FMM_Src_Cell" 
};
string fmmDDL = generator.GenerateMultipleTablesDDL(fmmTables);
```

### Example 3: Generate DDL Without Foreign Keys

```csharp
var generator = new TableDDLGenerator(si);

// Table and indexes only, no foreign keys
string ddl = generator.GenerateTableDDLByName("RegPlan", 
    includeIndexes: true, 
    includeForeignKeys: false);
```

## Generated DDL Format

The generated DDL includes:

### 1. CREATE TABLE Statements
```sql
-- Table: RegPlan
CREATE TABLE dbo.[RegPlan] (
    [RegPlanID] UNIQUEIDENTIFIER NOT NULL,
    [WFScenarioName] NVARCHAR(100) NULL,
    -- ... more columns
);
GO
```

### 2. Index Definitions
```sql
-- Indexes for table: RegPlan
ALTER TABLE dbo.[RegPlan]
    ADD CONSTRAINT [PK_RegPlan] PRIMARY KEY CLUSTERED ([RegPlanID]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Status]
    ON dbo.[RegPlan] ([Status]);
GO
```

### 3. Foreign Key Constraints
```sql
-- Foreign keys for table: RegPlan_Details
ALTER TABLE dbo.[RegPlan_Details]
    ADD CONSTRAINT [FK_RegPlan_Details_RegPlan]
    FOREIGN KEY ([RegPlanID])
    REFERENCES dbo.[RegPlan] ([RegPlanID])
    ON DELETE CASCADE;
GO
```

## Populating Table Configurations

To use this DDL generation system, you need to populate the FMM_Table_Config tables with your table metadata:

### Example: Adding a New Table Configuration

```sql
-- 1. Add table configuration
INSERT INTO FMM_Table_Config (
    ProcessType, TableName, TableType, Description, IsActive, EnableAudit
) VALUES (
    'FMM', 'MyCustomTable', 'Master', 'My custom table description', 1, 0
);

DECLARE @TableConfigID INT = SCOPE_IDENTITY();

-- 2. Add column configurations
INSERT INTO FMM_Table_Column_Config (
    Table_Config_ID, ColumnName, DataType, MaxLength, 
    IsNullable, IsIdentity, IsPrimaryKey
) VALUES 
    (@TableConfigID, 'MyTableID', 'INT', NULL, 0, 1, 1),
    (@TableConfigID, 'Name', 'NVARCHAR', 255, 0, 0, 0),
    (@TableConfigID, 'Description', 'NVARCHAR', 1000, 1, 0, 0),
    (@TableConfigID, 'CreateDate', 'DATETIME', NULL, 0, 0, 0);

-- 3. Add index configuration (primary key)
INSERT INTO FMM_Table_Index_Config (
    Table_Config_ID, IndexName, IndexType, IsClustered, IsUnique
) VALUES (
    @TableConfigID, 'PK_MyCustomTable', 'PRIMARY_KEY', 1, 1
);

DECLARE @IndexConfigID INT = SCOPE_IDENTITY();

-- 4. Link column to index
INSERT INTO FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, KeyOrdinal, SortDirection
) 
SELECT @IndexConfigID, Column_Config_ID, 1, 'ASC'
FROM FMM_Table_Column_Config
WHERE Table_Config_ID = @TableConfigID AND ColumnName = 'MyTableID';
```

## Benefits

1. **Metadata-Driven**: Table structures defined once in configuration tables
2. **Version Control**: DDL generation from metadata ensures consistency
3. **Documentation**: Auto-generated DDL serves as documentation
4. **Flexibility**: Generate DDL for entire processes or individual tables
5. **Audit Trail**: Track table structure changes through configuration versioning

## Integration with Existing DDL Files

The manually created DDL files (DDL_FMM_Tables.sql, DDL_DDM_Tables.sql) serve as templates. To integrate with the configuration system:

1. Populate FMM_Table_Config tables based on existing DDL
2. Use the DDL generator to create consistent, metadata-driven scripts
3. Compare generated DDL with manual DDL to ensure accuracy

## Best Practices

1. **Process Type Naming**: Use consistent process type names (FMM, DDM, MDM, etc.)
2. **Table Type Classification**: Mark tables as Master, Detail, Extension, or Audit
3. **Index Strategy**: Define primary keys and commonly-used indexes in configuration
4. **Foreign Key Naming**: Use consistent naming conventions (FK_{TableName}_{ReferencedTable})
5. **Documentation**: Fill in Description fields for tables and columns

## Troubleshooting

### No tables found for process type
- Verify ProcessType value matches FMM_Table_Config.ProcessType
- Check IsActive = 1 on table configurations

### Missing columns in generated DDL
- Verify FMM_Table_Column_Config entries exist for the table
- Check Table_Config_ID foreign key references

### Indexes not generated
- Verify FMM_Table_Index_Config and FMM_Table_Index_Column_Config entries exist
- Ensure proper foreign key relationships between index and column configs

## See Also

- **FMM_Table_Config_Helper.cs**: Core helper class with DDL generation methods
- **DDL_FMM_Tables.sql**: Example manually-created FMM DDL
- **DDL_DDM_Tables.sql**: Example manually-created DDM DDL
- **README_DDL.md**: Documentation for manually-created DDL files
