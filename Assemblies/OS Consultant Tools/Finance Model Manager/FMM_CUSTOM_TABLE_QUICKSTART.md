# FMM Custom Table Configuration System - Quick Start Guide

## Overview
This guide provides step-by-step instructions for implementing custom tables using the FMM Custom Table Configuration System.

## Prerequisites
1. Execute `FMM_Table_Config_DDL.sql` to create the configuration tables
2. Ensure you have appropriate database permissions
3. Have the `FMM_Table_Config_Helper.cs` class available in your project

## Step-by-Step: Creating a Custom Process

### Scenario: Asset Register
Let's create a custom "Asset Register" process with master and detail tables.

### Step 1: Design Your Tables

**Asset_Register_Master** (Master Table)
- Asset_ID (PK)
- Asset_Name
- Purchase_Date
- Purchase_Amount
- Depreciation_Method
- Useful_Life_Years

**Asset_Depreciation_Detail** (Detail Table)
- Asset_Depreciation_ID (PK)
- Asset_ID (FK to Master)
- Year
- Month1-Month12 (monthly depreciation)
- Yearly_Total

### Step 2: Create Master Table Configuration

```sql
-- Insert master table configuration
DECLARE @AssetMasterID INT;

INSERT INTO dbo.FMM_Table_Config (
    Process_Type, 
    Table_Name, 
    Table_Type, 
    Description,
    Enable_Audit,
    Create_User
)
VALUES (
    'Asset_Register',
    'Asset_Register_Master',
    'Master',
    'Master table for asset register',
    1, -- Enable audit
    SYSTEM_USER
);

SET @AssetMasterID = SCOPE_IDENTITY();
```

### Step 3: Define Columns for Master Table

```sql
-- Define columns
INSERT INTO dbo.FMM_Table_Column_Config (
    Table_Config_ID, Column_Name, Data_Type, Max_Length, Precision, Scale,
    Is_Nullable, Column_Order, Description, Create_User
)
VALUES
    -- Primary Key
    (@AssetMasterID, 'Asset_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 1, 'Asset unique identifier', SYSTEM_USER),
    
    -- Workflow columns (required for FMM integration)
    (@AssetMasterID, 'WF_Scenario_Name', 'NVARCHAR', 255, NULL, NULL, 0, 2, 'Workflow scenario', SYSTEM_USER),
    (@AssetMasterID, 'WF_Profile_Name', 'NVARCHAR', 255, NULL, NULL, 0, 3, 'Workflow profile', SYSTEM_USER),
    (@AssetMasterID, 'WF_Time_Name', 'NVARCHAR', 255, NULL, NULL, 0, 4, 'Workflow time', SYSTEM_USER),
    (@AssetMasterID, 'Activity_ID', 'INT', NULL, NULL, NULL, 0, 5, 'Activity ID', SYSTEM_USER),
    
    -- Asset specific columns
    (@AssetMasterID, 'Asset_Name', 'NVARCHAR', 255, NULL, NULL, 0, 6, 'Asset name', SYSTEM_USER),
    (@AssetMasterID, 'Purchase_Date', 'DATETIME', NULL, NULL, NULL, 0, 7, 'Purchase date', SYSTEM_USER),
    (@AssetMasterID, 'Purchase_Amount', 'DECIMAL', NULL, 18, 2, 0, 8, 'Purchase amount', SYSTEM_USER),
    (@AssetMasterID, 'Depreciation_Method', 'NVARCHAR', 50, NULL, NULL, 1, 9, 'Depreciation method', SYSTEM_USER),
    (@AssetMasterID, 'Useful_Life_Years', 'INT', NULL, NULL, NULL, 1, 10, 'Useful life in years', SYSTEM_USER),
    
    -- Standard audit columns
    (@AssetMasterID, 'Create_Date', 'DATETIME', NULL, NULL, NULL, 0, 11, 'Creation date', SYSTEM_USER),
    (@AssetMasterID, 'Create_User', 'NVARCHAR', 100, NULL, NULL, 0, 12, 'Creation user', SYSTEM_USER),
    (@AssetMasterID, 'Update_Date', 'DATETIME', NULL, NULL, NULL, 0, 13, 'Last update date', SYSTEM_USER),
    (@AssetMasterID, 'Update_User', 'NVARCHAR', 100, NULL, NULL, 0, 14, 'Last update user', SYSTEM_USER);
```

### Step 4: Define Primary Key (Clustered)

```sql
DECLARE @PKIndexID INT;
DECLARE @AssetIDColID INT;

-- Create primary key index
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, 
    Index_Name, 
    Index_Type, 
    Is_Clustered, 
    Is_Unique,
    Description,
    Create_User
)
VALUES (
    @AssetMasterID,
    'PK_Asset_Register_Master',
    'PRIMARY_KEY',
    1, -- Clustered
    1, -- Unique
    'Primary key on Asset_ID',
    SYSTEM_USER
);

SET @PKIndexID = SCOPE_IDENTITY();

-- Get Asset_ID column config ID
SELECT @AssetIDColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @AssetMasterID AND Column_Name = 'Asset_ID';

-- Map PK column
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction, Create_User
)
VALUES (@PKIndexID, @AssetIDColID, 1, 'ASC', SYSTEM_USER);
```

### Step 5: Define Non-Clustered Indexes

```sql
DECLARE @NameIndexID INT;
DECLARE @AssetNameColID INT;

-- Create index on Asset_Name for searches
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, 
    Index_Name, 
    Index_Type, 
    Is_Clustered, 
    Is_Unique,
    Description,
    Create_User
)
VALUES (
    @AssetMasterID,
    'IX_Asset_Name',
    'NONCLUSTERED',
    0, -- Not clustered
    0, -- Not unique
    'Index on Asset_Name for efficient searching',
    SYSTEM_USER
);

SET @NameIndexID = SCOPE_IDENTITY();

-- Get Asset_Name column config ID
SELECT @AssetNameColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @AssetMasterID AND Column_Name = 'Asset_Name';

-- Map index column
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction, Create_User
)
VALUES (@NameIndexID, @AssetNameColID, 1, 'ASC', SYSTEM_USER);
```

### Step 6: Create Detail Table Configuration

```sql
DECLARE @AssetDetailID INT;

INSERT INTO dbo.FMM_Table_Config (
    Process_Type, 
    Table_Name, 
    Table_Type,
    Parent_Table_Config_ID,
    Description,
    Enable_Audit,
    Create_User
)
VALUES (
    'Asset_Register',
    'Asset_Depreciation_Detail',
    'Detail',
    @AssetMasterID, -- Link to master
    'Detail table for asset depreciation schedule',
    1, -- Enable audit
    SYSTEM_USER
);

SET @AssetDetailID = SCOPE_IDENTITY();
```

### Step 7: Define Detail Table Columns

```sql
INSERT INTO dbo.FMM_Table_Column_Config (
    Table_Config_ID, Column_Name, Data_Type, Max_Length, Precision, Scale,
    Is_Nullable, Column_Order, Description, Create_User
)
VALUES
    -- Primary Key
    (@AssetDetailID, 'Asset_Depreciation_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 1, 'Detail record ID', SYSTEM_USER),
    
    -- Foreign Key to Master
    (@AssetDetailID, 'Asset_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 2, 'Foreign key to master', SYSTEM_USER),
    
    -- Workflow columns
    (@AssetDetailID, 'WF_Scenario_Name', 'NVARCHAR', 255, NULL, NULL, 0, 3, 'Workflow scenario', SYSTEM_USER),
    (@AssetDetailID, 'WF_Profile_Name', 'NVARCHAR', 255, NULL, NULL, 0, 4, 'Workflow profile', SYSTEM_USER),
    (@AssetDetailID, 'WF_Time_Name', 'NVARCHAR', 255, NULL, NULL, 0, 5, 'Workflow time', SYSTEM_USER),
    (@AssetDetailID, 'Activity_ID', 'INT', NULL, NULL, NULL, 0, 6, 'Activity ID', SYSTEM_USER),
    
    -- Year
    (@AssetDetailID, 'Year', 'NVARCHAR', 10, NULL, NULL, 0, 7, 'Depreciation year', SYSTEM_USER),
    
    -- Monthly depreciation values
    (@AssetDetailID, 'Month1', 'DECIMAL', NULL, 18, 2, 1, 8, 'January depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month2', 'DECIMAL', NULL, 18, 2, 1, 9, 'February depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month3', 'DECIMAL', NULL, 18, 2, 1, 10, 'March depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month4', 'DECIMAL', NULL, 18, 2, 1, 11, 'April depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month5', 'DECIMAL', NULL, 18, 2, 1, 12, 'May depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month6', 'DECIMAL', NULL, 18, 2, 1, 13, 'June depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month7', 'DECIMAL', NULL, 18, 2, 1, 14, 'July depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month8', 'DECIMAL', NULL, 18, 2, 1, 15, 'August depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month9', 'DECIMAL', NULL, 18, 2, 1, 16, 'September depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month10', 'DECIMAL', NULL, 18, 2, 1, 17, 'October depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month11', 'DECIMAL', NULL, 18, 2, 1, 18, 'November depreciation', SYSTEM_USER),
    (@AssetDetailID, 'Month12', 'DECIMAL', NULL, 18, 2, 1, 19, 'December depreciation', SYSTEM_USER),
    
    -- Yearly total
    (@AssetDetailID, 'Yearly_Total', 'DECIMAL', NULL, 18, 2, 1, 20, 'Total depreciation for year', SYSTEM_USER),
    
    -- Audit columns
    (@AssetDetailID, 'Create_Date', 'DATETIME', NULL, NULL, NULL, 0, 21, 'Creation date', SYSTEM_USER),
    (@AssetDetailID, 'Create_User', 'NVARCHAR', 100, NULL, NULL, 0, 22, 'Creation user', SYSTEM_USER),
    (@AssetDetailID, 'Update_Date', 'DATETIME', NULL, NULL, NULL, 0, 23, 'Last update date', SYSTEM_USER),
    (@AssetDetailID, 'Update_User', 'NVARCHAR', 100, NULL, NULL, 0, 24, 'Last update user', SYSTEM_USER);
```

### Step 8: Define Detail Table Primary Key

```sql
DECLARE @DetailPKIndexID INT;
DECLARE @DetailPKColID INT;

-- Create primary key
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, Index_Name, Index_Type, Is_Clustered, Is_Unique, Create_User
)
VALUES (@AssetDetailID, 'PK_Asset_Depreciation_Detail', 'PRIMARY_KEY', 1, 1, SYSTEM_USER);

SET @DetailPKIndexID = SCOPE_IDENTITY();

-- Get PK column ID
SELECT @DetailPKColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @AssetDetailID AND Column_Name = 'Asset_Depreciation_ID';

-- Map PK column
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction, Create_User
)
VALUES (@DetailPKIndexID, @DetailPKColID, 1, 'ASC', SYSTEM_USER);
```

### Step 9: Create Foreign Key Constraint

```sql
DECLARE @FKID INT;
DECLARE @DetailAssetIDColID INT;
DECLARE @MasterAssetIDColID INT;

-- Create FK configuration
INSERT INTO dbo.FMM_Table_FK_Config (
    FK_Name,
    Source_Table_Config_ID,
    Target_Table_Config_ID,
    On_Delete_Action,
    On_Update_Action,
    Description,
    Create_User
)
VALUES (
    'FK_AssetDepreciation_Asset',
    @AssetDetailID,
    @AssetMasterID,
    'CASCADE', -- Delete details when master is deleted
    'CASCADE', -- Update details when master is updated
    'Links depreciation details to asset master',
    SYSTEM_USER
);

SET @FKID = SCOPE_IDENTITY();

-- Get column IDs
SELECT @DetailAssetIDColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @AssetDetailID AND Column_Name = 'Asset_ID';

SELECT @MasterAssetIDColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @AssetMasterID AND Column_Name = 'Asset_ID';

-- Map FK columns
INSERT INTO dbo.FMM_Table_FK_Column_Config (
    FK_Config_ID, Source_Column_Config_ID, Target_Column_Config_ID, 
    Column_Ordinal, Create_User
)
VALUES (@FKID, @DetailAssetIDColID, @MasterAssetIDColID, 1, SYSTEM_USER);
```

### Step 10: Generate and Execute DDL

#### Using C# Helper Class:

```csharp
using (var si = /* your SessionInfo */)
{
    var helper = new FMM_Table_Config_Helper(si);
    
    // Generate DDL for entire process
    string ddl = helper.GenerateCompleteDDL("Asset_Register");
    
    // Log the DDL
    BRApi.ErrorLog.LogMessage(si, ddl);
    
    // Execute DDL to create physical tables
    using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
    {
        BRApi.Database.ExecuteSql(dbConn, ddl, false);
    }
    
    // Validate configuration
    var errors = helper.ValidateTableConfiguration(@AssetMasterID);
    if (errors.Any())
    {
        foreach (var error in errors)
        {
            BRApi.ErrorLog.LogMessage(si, $"Validation Error: {error}");
        }
    }
}
```

#### Or Using SQL:

```sql
-- Query the views to see generated configuration
SELECT * FROM dbo.vw_FMM_Table_Structure 
WHERE Process_Type = 'Asset_Register';

SELECT * FROM dbo.vw_FMM_Table_Indexes
WHERE Table_Name LIKE 'Asset_%';

SELECT * FROM dbo.vw_FMM_Table_ForeignKeys
WHERE Source_Table LIKE 'Asset_%';
```

## Common Patterns

### Pattern 1: Composite Primary Key

```sql
-- For tables that need multi-column primary keys
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction
)
VALUES 
    (@PKIndexID, @Column1ID, 1, 'ASC'),
    (@PKIndexID, @Column2ID, 2, 'ASC'),
    (@PKIndexID, @Column3ID, 3, 'ASC');
```

### Pattern 2: Covering Index with Included Columns

```sql
-- Create index with key and included columns
INSERT INTO dbo.FMM_Table_Index_Config (...)
VALUES (...);

SET @IndexID = SCOPE_IDENTITY();

-- Key columns
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Is_Included_Column
)
VALUES 
    (@IndexID, @KeyCol1, 1, 0),  -- Key column
    (@IndexID, @KeyCol2, 2, 0);  -- Key column

-- Included columns
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID, Column_Config_ID, Key_Ordinal, Is_Included_Column
)
VALUES 
    (@IndexID, @IncCol1, 3, 1),  -- Included column
    (@IndexID, @IncCol2, 4, 1);  -- Included column
```

### Pattern 3: Self-Referencing Tables (Hierarchies)

```sql
-- Table references itself (e.g., parent-child relationships)
INSERT INTO dbo.FMM_Table_FK_Config (
    FK_Name,
    Source_Table_Config_ID,
    Target_Table_Config_ID,  -- Same table
    On_Delete_Action,
    On_Update_Action
)
VALUES (
    'FK_Asset_Parent',
    @AssetMasterID,
    @AssetMasterID,  -- Self-reference
    'NO ACTION',
    'CASCADE'
);
```

## Best Practices

1. **Always include workflow columns** for FMM integration:
   - WF_Scenario_Name
   - WF_Profile_Name
   - WF_Time_Name
   - Activity_ID

2. **Always include audit columns**:
   - Create_Date, Create_User
   - Update_Date, Update_User

3. **Use meaningful naming conventions**:
   - Tables: `[Process]_[Entity]_[Type]` (e.g., Asset_Register_Master)
   - Indexes: `PK_[Table]` or `IX_[Table]_[Columns]`
   - Foreign Keys: `FK_[Source]_[Target]`

4. **One clustered index per table** - Usually the primary key

5. **Create indexes on**:
   - Foreign key columns
   - Frequently queried columns
   - Workflow columns (Scenario, Profile, Time, Activity)

6. **Use CASCADE carefully** - Only when you want automatic deletion/updates

7. **Validate before generating DDL**:
   ```csharp
   var errors = helper.ValidateTableConfiguration(tableConfigId);
   ```

8. **Test in a development environment first**

## Querying Configuration

```sql
-- Get all tables for a process
SELECT * FROM dbo.vw_FMM_Table_Structure
WHERE Process_Type = 'Asset_Register';

-- Get all indexes for a table
SELECT * FROM dbo.vw_FMM_Table_Indexes
WHERE Table_Name = 'Asset_Register_Master';

-- Get all foreign keys
SELECT * FROM dbo.vw_FMM_Table_ForeignKeys
WHERE Source_Table = 'Asset_Depreciation_Detail';
```

## Troubleshooting

### Problem: Configuration validation fails

**Solution**: Check validation errors:
```csharp
var errors = helper.ValidateTableConfiguration(tableConfigId);
foreach (var error in errors) {
    Console.WriteLine(error);
}
```

### Problem: Multiple clustered indexes error

**Solution**: Ensure only one index per table has `Is_Clustered = 1`

### Problem: Foreign key validation fails

**Solution**: Ensure:
1. Target table exists
2. Target columns match source column data types
3. Target columns have a unique index or primary key

### Problem: DDL generation fails

**Solution**: Check:
1. All required columns are defined
2. Column orders are sequential
3. Data types are valid
4. No duplicate column names

## Next Steps

After creating your custom tables:

1. **Create SQL Adapters** for CRUD operations (similar to existing FMM adapters)
2. **Build UI Components** (Dashboards, XFTVs) to interact with the tables
3. **Implement Business Logic** in Business Rule classes
4. **Configure Audit Triggers** if using database-level auditing
5. **Create Migration Scripts** for existing data if needed

## Support

For questions or issues:
1. Review the main proposal document: `FMM_CUSTOM_TABLE_PROPOSAL.md`
2. Check sample implementation: `FMM_Table_Config_Sample_Data.sql`
3. Refer to the Register_Plan configuration as a working example
