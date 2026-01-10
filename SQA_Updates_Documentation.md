# SQA Updates Documentation

## Overview

This document describes the consolidated SQL Adapter (SQA) implementation that replaces the previous duplicated code pattern across all SQA files in the repository.

## Problem Statement

Previously, the codebase contained 26+ SQA files with duplicated code for handling database operations. Each file manually implemented:
- INSERT command with manual parameter binding
- UPDATE command with manual parameter binding  
- DELETE command with manual parameter binding
- Transaction management
- Error handling

The main variation was in handling **single primary keys** vs **composite primary keys** (e.g., tables like `SQA_RegPlan_Details` required special handling for composite keys).

## Solution

A consolidated helper class `GBL_SQA_Helper` was created in the Global Utilities Assembly that:
1. **Eliminates code duplication** - All SQA operations now use a single, parameterized helper function
2. **Supports both single and composite primary keys** - Dynamic WHERE clause generation accommodates any number of primary key columns
3. **Simplifies SQA files** - Each SQA file is now ~30-40 lines instead of 100-200 lines
4. **Provides MERGE and SYNC operations** - Advanced operations for conditional upserts and synchronization

## Architecture

### GBL_SQA_Helper Class

**Location**: `Assemblies/OS Consultant Tools/App Objects (Globals)/GBL_UI_Assembly/GBL Support Classes/GBL_SQA_Helper.cs`

### Key Components

#### 1. FillDataTable Method
Handles SELECT operations with parameterized queries.

**Signature**:
```csharp
public void FillDataTable(
    SessionInfo si, 
    SqlDataAdapter sqa, 
    DataTable dt, 
    string sql, 
    params SqlParameter[] sqlParams)
```

#### 2. UpdateDataTable Method
Handles INSERT, UPDATE, and DELETE operations with dynamic SQL generation.

**Signature**:
```csharp
public void UpdateDataTable(
    SessionInfo si,
    DataTable dt,
    SqlDataAdapter sqa,
    string tableName,
    List<ColumnDefinition> columnDefinitions,
    List<string> primaryKeyColumns,
    List<string> excludeFromUpdate = null,
    bool autoTimestamps = false)
```

**Parameters**:
- `si` - SessionInfo for user context
- `dt` - DataTable containing changes to persist
- `sqa` - SqlDataAdapter to use for database operations
- `tableName` - Name of the target database table
- `columnDefinitions` - List of column definitions (name, SQL type, size)
- `primaryKeyColumns` - List of primary key column names (supports single or composite keys)
- `excludeFromUpdate` - Optional columns to exclude from UPDATE SET clause
- `autoTimestamps` - Automatically handle Create_Date/Update_Date with GETDATE() and Create_User/Update_User with si.UserName

#### 3. BuildWhereClause Method
Dynamically constructs WHERE clauses for UPDATE and DELETE operations.

**Logic**:
- Single key: `WHERE [ID] = @ID`
- Composite key: `WHERE [Key1] = @Key1 AND [Key2] = @Key2 AND [Key3] = @Key3`

#### 4. MergeDataTable Method
Performs MERGE operations (upsert) with optional conditional delete.

**Signature**:
```csharp
public void MergeDataTable(
    SessionInfo si,
    DataTable sourceTable,
    string targetTableName,
    List<ColumnDefinition> columnDefinitions,
    List<string> primaryKeyColumns,
    string conditionalDeleteWhere = null)
```

**Use Case**: Synchronize source data with target table, optionally deleting unmatched rows based on a condition.

#### 5. SynchronizeDataTable Method
Performs full table synchronization.

**Signature**:
```csharp
public void SynchronizeDataTable(
    SessionInfo si,
    DataTable sourceTable,
    string targetTableName,
    List<ColumnDefinition> columnDefinitions,
    List<string> primaryKeyColumns,
    string syncDeleteCondition = null)
```

**Use Case**: Ensure target table exactly matches source data, with optional conditional delete for unmatched rows.

### ColumnDefinition Class

Helper class for defining table columns:

```csharp
public class ColumnDefinition
{
    public string ColumnName { get; set; }
    public SqlDbType SqlDbType { get; set; }
    public int Size { get; set; }
    
    public ColumnDefinition(string columnName, SqlDbType sqlDbType, int size = 0)
}
```

## Migration Guide

### Before (Old Pattern)

```csharp
public class SQA_FMM_Act_Config
{
    private readonly SqlConnection _connection;

    public SQA_FMM_Act_Config(SessionInfo si, SqlConnection connection)
    {
        _connection = connection;
    }

    public void Fill_FMM_Act_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
    {
        using (SqlCommand command = new SqlCommand(sql, _connection))
        {
            command.CommandType = CommandType.Text;
            if (sqlparams != null)
            {
                command.Parameters.AddRange(sqlparams);
            }
            sqa.SelectCommand = command;
            sqa.Fill(dt);
            command.Parameters.Clear();
            sqa.SelectCommand = null;
        }
    }

    public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
    {
        sqa.UpdateBatchSize = 0;
        using (SqlTransaction transaction = _connection.BeginTransaction())
        {
            // 50+ lines of INSERT command building
            // 50+ lines of UPDATE command building
            // 20+ lines of DELETE command building
            // Try/catch with transaction handling
        }
    }
}
```

### After (New Pattern)

```csharp
public class SQA_FMM_Act_Config
{
    private readonly SqlConnection _connection;
    private readonly GBL_SQA_Helper _helper;

    public SQA_FMM_Act_Config(SessionInfo si, SqlConnection connection)
    {
        _connection = connection;
        _helper = new GBL_SQA_Helper(connection);
    }

    public void Fill_FMM_Act_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
    {
        _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
    }

    public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
    {
        var columnDefinitions = new List<ColumnDefinition>
        {
            new ColumnDefinition("Cube_ID", SqlDbType.Int),
            new ColumnDefinition("Act_ID", SqlDbType.Int),
            new ColumnDefinition("Name", SqlDbType.NVarChar, 100),
            new ColumnDefinition("Calc_Type", SqlDbType.NVarChar, 20),
            new ColumnDefinition("Status", SqlDbType.NVarChar, 10),
            new ColumnDefinition("Create_Date", SqlDbType.DateTime),
            new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
            new ColumnDefinition("Update_Date", SqlDbType.DateTime),
            new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
        };

        var primaryKeyColumns = new List<string> { "Act_ID" };

        _helper.UpdateDataTable(si, dt, sqa, "FMM_Act_Config", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
    }
}
```

## Usage Examples

### Single Primary Key Table

```csharp
var columnDefinitions = new List<ColumnDefinition>
{
    new ColumnDefinition("ID", SqlDbType.Int),
    new ColumnDefinition("Name", SqlDbType.NVarChar, 100),
    new ColumnDefinition("Status", SqlDbType.NVarChar, 10)
};

var primaryKeyColumns = new List<string> { "ID" };

_helper.UpdateDataTable(si, dt, sqa, "MyTable", columnDefinitions, primaryKeyColumns);
```

### Composite Primary Key Table

```csharp
var columnDefinitions = new List<ColumnDefinition>
{
    new ColumnDefinition("RegPlan_ID", SqlDbType.UniqueIdentifier),
    new ColumnDefinition("Year", SqlDbType.NVarChar, 4),
    new ColumnDefinition("Account", SqlDbType.NVarChar, 20),
    new ColumnDefinition("Amount", SqlDbType.Decimal)
};

var primaryKeyColumns = new List<string> { "RegPlan_ID", "Year", "Account" };

_helper.UpdateDataTable(si, dt, sqa, "RegPlan_Details", columnDefinitions, primaryKeyColumns);
```

### Auto-Timestamp Management

```csharp
_helper.UpdateDataTable(
    si, dt, sqa, "MyTable", columnDefinitions, primaryKeyColumns, 
    autoTimestamps: true  // Automatically uses GETDATE() for dates and si.UserName for users
);
```

### Dynamic Column Generation

```csharp
var columnDefinitions = new List<ColumnDefinition>();

// Add base columns
columnDefinitions.Add(new ColumnDefinition("Base_ID", SqlDbType.Int));

// Dynamically add Attr_1 through Attr_20
for (int i = 1; i <= 20; i++)
{
    columnDefinitions.Add(new ColumnDefinition($"Attr_{i}", SqlDbType.NVarChar, 100));
}
```

## Benefits

1. **Code Reduction**: ~70% reduction in code per SQA file (from 100-200 lines to 30-40 lines)
2. **Maintainability**: Single source of truth for SQL generation logic
3. **Flexibility**: Handles both simple and complex table structures
4. **Safety**: Consistent transaction management and error handling
5. **Extensibility**: Easy to add new features (e.g., MERGE, SYNC operations)

## Migrated Files

### FMM_Shared_Assembly
- ✅ SQA_RegPlan
- ✅ SQA_RegPlan_Details (composite key example)
- ⏳ SQA_RegPlan_Audit

### FMM_UI_Assembly
- ✅ SQA_FMM_Act_Config
- ✅ SQA_FMM_Models
- ✅ SQA_FMM_Cube_Config
- ✅ SQA_FMM_Dest_Cell
- ✅ SQA_FMM_Src_Cell
- ⏳ SQA_FMM_Acct_Config
- ⏳ SQA_FMM_Act_Appr_Step_Config
- ⏳ SQA_FMM_Appr_Config
- ⏳ SQA_FMM_Appr_Step_Config
- ⏳ SQA_FMM_Calc_Config
- ⏳ SQA_FMM_Calc_Unit_Assign
- ⏳ SQA_FMM_Calc_Unit_Config
- ⏳ SQA_FMM_Col_Config
- ⏳ SQA_FMM_Model_Grp_Assign
- ⏳ SQA_FMM_Model_Grp_Seqs
- ⏳ SQA_FMM_Model_Grps
- ⏳ SQA_FMM_Reg_Config
- ⏳ SQA_FMM_Unit_Config

### Other Assemblies
- ⏳ DDM_Config_UI_Assembly SQA files
- ⏳ Additional assemblies as needed

## Testing Recommendations

1. **Unit Tests**: Test GBL_SQA_Helper methods independently
2. **Integration Tests**: Test each migrated SQA file with actual database operations
3. **Regression Tests**: Ensure migrated files produce identical SQL to original implementation
4. **Performance Tests**: Verify no performance degradation from consolidated approach

## Known Issues / Limitations

None identified at this time. The consolidated approach handles all known use cases including:
- Single and composite primary keys
- Auto-generated timestamps and user tracking
- Complex tables with 50+ columns
- Dynamic column generation (e.g., Attr_1 through Attr_20)

## Future Enhancements

1. **Auto-detection of primary keys** from database schema
2. **Automatic ColumnDefinition generation** from DataTable schema
3. **Bulk operations support** for high-volume data processing
4. **Stored procedure wrapper** for tables that require custom logic
5. **Audit trail support** for tracking all changes

## Contact

For questions or issues with the consolidated SQA implementation, please refer to this documentation or contact the development team.
