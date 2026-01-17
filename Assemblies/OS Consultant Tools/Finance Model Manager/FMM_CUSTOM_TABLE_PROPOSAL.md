# FMM Custom Table Configuration System - Proposed Design

## Overview
This document proposes a flexible table configuration system for the Finance Model Manager (FMM) module that allows custom tables to be configured for different processes. The system provides flexibility to define primary keys, clustered indexes, non-clustered indexes, foreign key constraints, and audit tables.

## Design Goals
1. Allow multiple custom table structures similar to Reg_Plan and Reg_Plan_Detail
2. Provide flexibility to define primary keys
3. Allow configuration of clustered and non-clustered indexes
4. Support foreign key constraints between tables
5. Enable definition of custom tables linked to core Reg_Plan tables
6. Automatically generate and manage audit tables for each custom table set

## Proposed Table Structure

### 1. FMM_Table_Config (Core Table Configuration)
This table stores metadata about each custom table in the system.

```sql
CREATE TABLE FMM_Table_Config (
    Table_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    Process_Type NVARCHAR(100) NOT NULL,              -- e.g., 'Register_Plan', 'Custom_Process_1'
    Table_Name NVARCHAR(255) NOT NULL,                -- Physical table name
    Table_Type NVARCHAR(50) NOT NULL,                 -- 'Master', 'Detail', 'Audit', 'Extension'
    Parent_Table_Config_ID INT NULL,                  -- References parent table (for Detail/Extension tables)
    Description NVARCHAR(500) NULL,
    Is_Active BIT DEFAULT 1,
    Enable_Audit BIT DEFAULT 1,                       -- Whether to generate audit records
    Audit_Table_Config_ID INT NULL,                   -- Reference to corresponding audit table
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    Update_Date DATETIME DEFAULT GETDATE(),
    Update_User NVARCHAR(100),
    
    CONSTRAINT FK_Table_Parent FOREIGN KEY (Parent_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_Table_Audit FOREIGN KEY (Audit_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_Table_Name UNIQUE (Table_Name)
)
```

**Key Features:**
- `Process_Type`: Groups related tables together (e.g., all tables for a specific register)
- `Table_Type`: Distinguishes between Master (like Reg_Plan), Detail (like Reg_Plan_Detail), Audit, and Extension tables
- `Parent_Table_Config_ID`: Links Detail/Extension tables to their Master table
- `Enable_Audit`: Controls whether changes are tracked in audit table

### 2. FMM_Table_Column_Config (Column Definitions)
Defines columns for each custom table.

```sql
CREATE TABLE FMM_Table_Column_Config (
    Column_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    Table_Config_ID INT NOT NULL,
    Column_Name NVARCHAR(255) NOT NULL,
    Data_Type NVARCHAR(50) NOT NULL,                  -- 'INT', 'NVARCHAR', 'DECIMAL', 'DATETIME', etc.
    Max_Length INT NULL,                               -- For VARCHAR types
    Precision INT NULL,                                -- For DECIMAL types
    Scale INT NULL,                                    -- For DECIMAL types
    Is_Nullable BIT DEFAULT 1,
    Default_Value NVARCHAR(500) NULL,
    Is_Identity BIT DEFAULT 0,
    Identity_Seed INT NULL,
    Identity_Increment INT NULL,
    Column_Order INT NOT NULL,                         -- Display/creation order
    Description NVARCHAR(500) NULL,
    Is_Active BIT DEFAULT 1,
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    Update_Date DATETIME DEFAULT GETDATE(),
    Update_User NVARCHAR(100),
    
    CONSTRAINT FK_Column_Table FOREIGN KEY (Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_Column_Name UNIQUE (Table_Config_ID, Column_Name)
)
```

**Key Features:**
- Supports all SQL Server data types with appropriate parameters
- `Is_Identity`: Allows defining identity columns
- `Column_Order`: Controls the order of columns in table creation
- Flexible to handle various data types with precision and scale

### 3. FMM_Table_Index_Config (Index Definitions)
Defines indexes including primary keys, clustered, and non-clustered indexes.

```sql
CREATE TABLE FMM_Table_Index_Config (
    Index_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    Table_Config_ID INT NOT NULL,
    Index_Name NVARCHAR(255) NOT NULL,
    Index_Type NVARCHAR(50) NOT NULL,                 -- 'PRIMARY_KEY', 'CLUSTERED', 'NONCLUSTERED', 'UNIQUE'
    Is_Clustered BIT DEFAULT 0,                       -- TRUE if clustered index
    Is_Unique BIT DEFAULT 0,
    Fill_Factor INT NULL,                              -- Index fill factor (0-100)
    Description NVARCHAR(500) NULL,
    Is_Active BIT DEFAULT 1,
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    Update_Date DATETIME DEFAULT GETDATE(),
    Update_User NVARCHAR(100),
    
    CONSTRAINT FK_Index_Table FOREIGN KEY (Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_Index_Name UNIQUE (Table_Config_ID, Index_Name),
    CONSTRAINT CHK_Clustered_Unique CHECK (
        -- Only one clustered index per table (enforced at application level)
        Is_Clustered = 0 OR Index_Type IN ('PRIMARY_KEY', 'CLUSTERED')
    )
)
```

### 4. FMM_Table_Index_Column_Config (Index Column Mappings)
Links columns to indexes and defines their order.

```sql
CREATE TABLE FMM_Table_Index_Column_Config (
    Index_Column_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    Index_Config_ID INT NOT NULL,
    Column_Config_ID INT NOT NULL,
    Key_Ordinal INT NOT NULL,                          -- Order of column in index (1, 2, 3, ...)
    Sort_Direction NVARCHAR(10) DEFAULT 'ASC',         -- 'ASC' or 'DESC'
    Is_Included_Column BIT DEFAULT 0,                  -- For included columns in covering indexes
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    
    CONSTRAINT FK_IndexCol_Index FOREIGN KEY (Index_Config_ID) 
        REFERENCES FMM_Table_Index_Config(Index_Config_ID),
    CONSTRAINT FK_IndexCol_Column FOREIGN KEY (Column_Config_ID) 
        REFERENCES FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT UQ_IndexCol_Ordinal UNIQUE (Index_Config_ID, Key_Ordinal)
)
```

**Key Features:**
- `Key_Ordinal`: Defines the order of columns in composite indexes
- `Sort_Direction`: Allows ASC or DESC ordering per column
- `Is_Included_Column`: Supports covering indexes with included columns

### 5. FMM_Table_FK_Config (Foreign Key Constraint Definitions)
Defines foreign key relationships between tables.

```sql
CREATE TABLE FMM_Table_FK_Config (
    FK_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    FK_Name NVARCHAR(255) NOT NULL,
    Source_Table_Config_ID INT NOT NULL,               -- Child table
    Target_Table_Config_ID INT NOT NULL,               -- Parent table
    On_Delete_Action NVARCHAR(50) DEFAULT 'NO ACTION', -- 'CASCADE', 'SET NULL', 'SET DEFAULT', 'NO ACTION'
    On_Update_Action NVARCHAR(50) DEFAULT 'NO ACTION',
    Is_Active BIT DEFAULT 1,
    Description NVARCHAR(500) NULL,
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    Update_Date DATETIME DEFAULT GETDATE(),
    Update_User NVARCHAR(100),
    
    CONSTRAINT FK_Source_Table FOREIGN KEY (Source_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_Target_Table FOREIGN KEY (Target_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_FK_Name UNIQUE (FK_Name)
)
```

### 6. FMM_Table_FK_Column_Config (Foreign Key Column Mappings)
Maps columns between source and target tables in foreign keys.

```sql
CREATE TABLE FMM_Table_FK_Column_Config (
    FK_Column_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    FK_Config_ID INT NOT NULL,
    Source_Column_Config_ID INT NOT NULL,              -- Column in child table
    Target_Column_Config_ID INT NOT NULL,              -- Column in parent table
    Column_Ordinal INT NOT NULL,                       -- Order for composite foreign keys
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    
    CONSTRAINT FK_FKCol_FK FOREIGN KEY (FK_Config_ID) 
        REFERENCES FMM_Table_FK_Config(FK_Config_ID),
    CONSTRAINT FK_FKCol_Source FOREIGN KEY (Source_Column_Config_ID) 
        REFERENCES FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT FK_FKCol_Target FOREIGN KEY (Target_Column_Config_ID) 
        REFERENCES FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT UQ_FKCol_Ordinal UNIQUE (FK_Config_ID, Column_Ordinal)
)
```

**Key Features:**
- Supports composite foreign keys (multi-column)
- `On_Delete_Action` and `On_Update_Action`: Configurable cascade behaviors
- Links columns from source to target table

### 7. FMM_Table_Audit_Config (Audit Configuration)
Configures audit tracking for tables.

```sql
CREATE TABLE FMM_Table_Audit_Config (
    Audit_Config_ID INT PRIMARY KEY IDENTITY(1,1),
    Source_Table_Config_ID INT NOT NULL,
    Audit_Table_Config_ID INT NOT NULL,
    Track_Inserts BIT DEFAULT 1,
    Track_Updates BIT DEFAULT 1,
    Track_Deletes BIT DEFAULT 1,
    Audit_User_Column NVARCHAR(255) DEFAULT 'Audit_User',
    Audit_Date_Column NVARCHAR(255) DEFAULT 'Audit_Date',
    Audit_Action_Column NVARCHAR(255) DEFAULT 'Audit_Action', -- 'INSERT', 'UPDATE', 'DELETE'
    Is_Active BIT DEFAULT 1,
    Create_Date DATETIME DEFAULT GETDATE(),
    Create_User NVARCHAR(100),
    Update_Date DATETIME DEFAULT GETDATE(),
    Update_User NVARCHAR(100),
    
    CONSTRAINT FK_AuditCfg_Source FOREIGN KEY (Source_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_AuditCfg_Audit FOREIGN KEY (Audit_Table_Config_ID) 
        REFERENCES FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_AuditCfg_Source UNIQUE (Source_Table_Config_ID)
)
```

**Key Features:**
- Configurable tracking of INSERT, UPDATE, DELETE operations
- Standard audit columns (user, date, action)
- Links source table to its audit table

## Example: Register_Plan Configuration

Here's how the existing Reg_Plan and Reg_Plan_Detail tables would be configured:

### Step 1: Register Master Table (Reg_Plan)
```sql
-- Create table config entry
INSERT INTO FMM_Table_Config (Process_Type, Table_Name, Table_Type, Enable_Audit)
VALUES ('Register_Plan', 'Register_Plan', 'Master', 1);

-- Create column configs
INSERT INTO FMM_Table_Column_Config (Table_Config_ID, Column_Name, Data_Type, Is_Nullable, Is_Identity, Column_Order)
VALUES 
    (1, 'Register_Plan_ID', 'UNIQUEIDENTIFIER', 0, 0, 1),
    (1, 'WF_Scenario_Name', 'NVARCHAR', 0, 0, 2),
    (1, 'WF_Profile_Name', 'NVARCHAR', 0, 0, 3),
    (1, 'Activity_ID', 'INT', 0, 0, 4),
    (1, 'Entity', 'NVARCHAR', 1, 0, 5),
    (1, 'Attribute_1', 'NVARCHAR', 1, 0, 6),
    -- ... more columns ...
    (1, 'Create_Date', 'DATETIME', 0, 0, 50),
    (1, 'Create_User', 'NVARCHAR', 0, 0, 51);

-- Create primary key index
INSERT INTO FMM_Table_Index_Config (Table_Config_ID, Index_Name, Index_Type, Is_Clustered, Is_Unique)
VALUES (1, 'PK_Register_Plan', 'PRIMARY_KEY', 1, 1);

-- Map primary key column
INSERT INTO FMM_Table_Index_Column_Config (Index_Config_ID, Column_Config_ID, Key_Ordinal)
VALUES (1, 1, 1); -- Register_Plan_ID

-- Create non-clustered index on workflow columns
INSERT INTO FMM_Table_Index_Config (Table_Config_ID, Index_Name, Index_Type, Is_Clustered, Is_Unique)
VALUES (1, 'IX_Register_Plan_WF', 'NONCLUSTERED', 0, 0);

-- Map index columns
INSERT INTO FMM_Table_Index_Column_Config (Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction)
VALUES 
    (2, 2, 1, 'ASC'),  -- WF_Scenario_Name
    (2, 3, 2, 'ASC'),  -- WF_Profile_Name
    (2, 4, 3, 'ASC');  -- Activity_ID
```

### Step 2: Register Detail Table (Reg_Plan_Detail)
```sql
-- Create detail table config
INSERT INTO FMM_Table_Config (Process_Type, Table_Name, Table_Type, Parent_Table_Config_ID, Enable_Audit)
VALUES ('Register_Plan', 'Register_Plan_Details', 'Detail', 1, 1);

-- Create columns for detail table
INSERT INTO FMM_Table_Column_Config (Table_Config_ID, Column_Name, Data_Type, Is_Nullable, Column_Order)
VALUES 
    (2, 'Register_Plan_Detail_ID', 'UNIQUEIDENTIFIER', 0, 1),
    (2, 'Register_Plan_ID', 'UNIQUEIDENTIFIER', 0, 2),
    (2, 'Year', 'NVARCHAR', 1, 3),
    (2, 'Month1', 'DECIMAL', 1, 4),
    -- ... month columns ...
    (2, 'Quarter1', 'DECIMAL', 1, 16),
    -- ... quarter columns ...
    (2, 'Yearly', 'DECIMAL', 1, 20);

-- Create primary key
INSERT INTO FMM_Table_Index_Config (Table_Config_ID, Index_Name, Index_Type, Is_Clustered, Is_Unique)
VALUES (2, 'PK_Register_Plan_Details', 'PRIMARY_KEY', 1, 1);

INSERT INTO FMM_Table_Index_Column_Config (Index_Config_ID, Column_Config_ID, Key_Ordinal)
VALUES (3, <detail_id_column_config_id>, 1);

-- Create foreign key to parent table
INSERT INTO FMM_Table_FK_Config (FK_Name, Source_Table_Config_ID, Target_Table_Config_ID, On_Delete_Action)
VALUES ('FK_RegPlanDetails_RegPlan', 2, 1, 'CASCADE');

-- Map FK columns
INSERT INTO FMM_Table_FK_Column_Config (FK_Config_ID, Source_Column_Config_ID, Target_Column_Config_ID, Column_Ordinal)
VALUES (1, <detail_register_plan_id>, <master_register_plan_id>, 1);
```

### Step 3: Audit Table
```sql
-- Create audit table config (automatically mirrors source table structure plus audit columns)
INSERT INTO FMM_Table_Config (Process_Type, Table_Name, Table_Type, Parent_Table_Config_ID, Enable_Audit)
VALUES ('Register_Plan', 'Register_Plan_Audit', 'Audit', 1, 0);

-- Create audit configuration
INSERT INTO FMM_Table_Audit_Config (Source_Table_Config_ID, Audit_Table_Config_ID, Track_Inserts, Track_Updates, Track_Deletes)
VALUES (1, 3, 1, 1, 1);

-- Update source table to reference audit table
UPDATE FMM_Table_Config 
SET Audit_Table_Config_ID = 3 
WHERE Table_Config_ID = 1;
```

## Key Benefits

### 1. **Flexibility**
- Define any number of custom table structures
- Configure primary keys, indexes, and constraints per table
- Easily extend with new processes without code changes

### 2. **Metadata-Driven**
- All table structures are defined in metadata
- Changes can be made through configuration rather than code
- Enables dynamic table generation

### 3. **Audit Trail**
- Automatic audit table generation
- Configurable audit tracking (inserts, updates, deletes)
- Consistent audit pattern across all custom tables

### 4. **Relationship Management**
- Clear parent-child relationships between tables
- Foreign key constraints ensure data integrity
- Support for complex table hierarchies

### 5. **Index Optimization**
- Control over clustered vs non-clustered indexes
- Support for composite indexes
- Covering indexes with included columns

## Implementation Considerations

### Phase 1: Core Infrastructure
1. Create all configuration tables
2. Implement SQL adapters for CRUD operations on configuration tables
3. Create helper classes for reading and validating configurations

### Phase 2: Dynamic Table Management
1. Create utility classes for dynamic DDL generation
2. Implement table creation/modification based on configuration
3. Add validation logic to ensure configuration integrity

### Phase 3: Audit System
1. Implement audit table auto-generation
2. Create triggers or application-level audit tracking
3. Add audit query/reporting capabilities

### Phase 4: UI Components
1. Dashboard components for table configuration management
2. Visual index configuration tools
3. Foreign key relationship diagrams

### Phase 5: Migration Tools
1. Create utilities to migrate existing tables to new configuration system
2. Import/export configuration across environments
3. Version control for table configurations

## Usage Example: Creating a Custom Process

```csharp
// Example: Creating a custom "Asset Register" process

// 1. Create master table configuration
var assetRegisterMasterConfig = new TableConfig {
    ProcessType = "Asset_Register",
    TableName = "Asset_Register_Master",
    TableType = "Master",
    EnableAudit = true
};

// 2. Define columns
assetRegisterMasterConfig.AddColumn("Asset_ID", "UNIQUEIDENTIFIER", nullable: false, identity: false);
assetRegisterMasterConfig.AddColumn("Asset_Name", "NVARCHAR", maxLength: 255);
assetRegisterMasterConfig.AddColumn("Purchase_Date", "DATETIME");
assetRegisterMasterConfig.AddColumn("Purchase_Amount", "DECIMAL", precision: 18, scale: 2);

// 3. Define primary key (clustered)
assetRegisterMasterConfig.AddPrimaryKey("PK_Asset_Register", new[] { "Asset_ID" }, clustered: true);

// 4. Add non-clustered index
assetRegisterMasterConfig.AddIndex("IX_Asset_Name", new[] { "Asset_Name" }, unique: false, clustered: false);

// 5. Create detail table
var assetDepreciationDetail = new TableConfig {
    ProcessType = "Asset_Register",
    TableName = "Asset_Depreciation_Detail",
    TableType = "Detail",
    ParentTableConfigID = assetRegisterMasterConfig.TableConfigID
};

// 6. Add foreign key
assetDepreciationDetail.AddForeignKey(
    name: "FK_AssetDep_Asset",
    sourceColumns: new[] { "Asset_ID" },
    targetTable: assetRegisterMasterConfig,
    targetColumns: new[] { "Asset_ID" },
    onDelete: "CASCADE"
);

// 7. Generate physical tables
TableGenerator.CreatePhysicalTable(assetRegisterMasterConfig);
TableGenerator.CreatePhysicalTable(assetDepreciationDetail);
TableGenerator.CreateAuditTable(assetRegisterMasterConfig);
```

## Database Diagram

```
┌─────────────────────────┐
│  FMM_Table_Config       │
│  - Table_Config_ID (PK) │
│  - Process_Type         │
│  - Table_Name           │
│  - Table_Type           │
│  - Parent_Table_Config_ID│
└─────────────────────────┘
           │
           │ 1
           │
           │ *
┌─────────────────────────────┐
│ FMM_Table_Column_Config     │
│ - Column_Config_ID (PK)     │
│ - Table_Config_ID (FK)      │
│ - Column_Name               │
│ - Data_Type                 │
│ - Is_Nullable               │
└─────────────────────────────┘
           │
           │ 1
           │
           │ *
┌──────────────────────────────┐
│ FMM_Table_Index_Column_Config│
│ - Index_Column_Config_ID(PK) │
│ - Index_Config_ID (FK)       │
│ - Column_Config_ID (FK)      │
│ - Key_Ordinal                │
└──────────────────────────────┘
           │
           │ *
           │
           │ 1
┌─────────────────────────┐
│ FMM_Table_Index_Config  │
│ - Index_Config_ID (PK)  │
│ - Table_Config_ID (FK)  │
│ - Index_Name            │
│ - Index_Type            │
│ - Is_Clustered          │
└─────────────────────────┘
```

## Next Steps

1. Review and approve table structure
2. Create SQL scripts for table creation
3. Implement C# classes and SQL adapters
4. Build configuration UI components
5. Create documentation and training materials
6. Migrate existing Reg_Plan tables to new configuration system
7. Test with pilot custom processes

## Conclusion

This proposed design provides a comprehensive, flexible, and maintainable system for managing custom table structures in the FMM module. It addresses all requirements from the problem statement while providing a solid foundation for future extensions.
