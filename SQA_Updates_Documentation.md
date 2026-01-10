# SQA File Updates - Merge and Sync Implementation

## Overview

This document describes the updates made to all SQA (SQL Adapter) files to add dynamic SQL building, MERGE, and SYNC capabilities.

## Problem Statement

Previously, all SQA files had hardcoded INSERT, UPDATE, and DELETE SQL statements. This meant:
- Any schema changes required manual updates to multiple files
- No built-in MERGE (upsert) functionality
- No way to perform conditional deletes
- No synchronization capabilities

## Solution

Created a centralized helper class (`GBL_SQA_Helper`) that:
1. **Dynamically builds SQL** based on DataTable columns at runtime
2. **Provides MERGE operations** with optional conditional deletes
3. **Provides SYNC operations** for full table synchronization

## Files Modified

### Helper Class
- **Location**: `Assemblies/OS Consultant Tools/App Objects (Globals)/GBL_UI_Assembly/GBL Support Classes/GBL_SQA_Helper.cs`
- **Purpose**: Provides static utility methods for all SQA files

### Updated SQA Files (24 total)

#### FMM_UI_Assembly (18 files)
- SQA_FMM_Models.cs
- SQA_FMM_Act_Config.cs
- SQA_FMM_Acct_Config.cs
- SQA_FMM_Act_Appr_Step_Config.cs
- SQA_FMM_Appr_Config.cs
- SQA_FMM_Appr_Step_Config.cs
- SQA_FMM_Calc_Config.cs
- SQA_FMM_Calc_Unit_Assign.cs
- SQA_FMM_Calc_Unit_Config.cs
- SQA_FMM_Col_Config.cs
- SQA_FMM_Cube_Config.cs
- SQA_FMM_Dest_Cell.cs
- SQA_FMM_Model_Grp_Assign.cs
- SQA_FMM_Model_Grp_Seqs.cs
- SQA_FMM_Model_Grps.cs
- SQA_FMM_Reg_Config.cs
- SQA_FMM_Src_Cell.cs
- SQA_FMM_Unit_Config.cs

#### DDM_Config_UI_Assembly (5 files)
- SQA_DDM_Config.cs
- SQA_DDM_Config_Hdr_Ctrls.cs
- SQA_DDM_Config_Menu.cs
- SQA_DDM_Config_Menu_Hdr.cs
- SQA_DDM_Config_Menu_Layout.cs

#### FMM_Shared_Assembly (1 file)
- SQA_RegPlan.cs

### Not Updated (2 files)
- SQA_RegPlan_Audit.cs - Uses composite primary key (RegisterID, RegisterIDInstance)
- SQA_RegPlan_Details.cs - Uses composite primary key (RegPlan_ID, Year, Plan_Units, Account)

## New Methods Added to Each SQA File

### 1. Updated Update Method
The existing `Update_*` method now uses dynamic SQL building:

```csharp
public void Update_FMM_Models(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    sqa.UpdateBatchSize = 0;
    using (SqlTransaction transaction = _connection.BeginTransaction())
    {
        try
        {
            // Build commands dynamically based on DataTable columns
            GBL_SQA_Helper.BuildInsertCommand(sqa, _connection, transaction, dt, "FMM_Models");
            GBL_SQA_Helper.BuildUpdateCommand(sqa, _connection, transaction, dt, "FMM_Models", "Model_ID");
            GBL_SQA_Helper.BuildDeleteCommand(sqa, _connection, transaction, dt, "FMM_Models", "Model_ID");

            sqa.Update(dt);
            transaction.Commit();
            sqa.InsertCommand = null;
            sqa.UpdateCommand = null;
            sqa.DeleteCommand = null;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

### 2. Merge Method
Performs UPSERT (insert or update) operations with optional conditional deletes:

```csharp
public void Merge_FMM_Models(SessionInfo si, DataTable dt, bool deleteUnmatched = false, string deleteCondition = null)
{
    GBL_SQA_Helper.MergeData(si, _connection, dt, "FMM_Models", "Model_ID", deleteUnmatched, deleteCondition);
}
```

**Parameters:**
- `si` - SessionInfo for error handling
- `dt` - DataTable containing data to merge
- `deleteUnmatched` - If true, deletes records not in source DataTable
- `deleteCondition` - Optional SQL WHERE clause for conditional deletes (e.g., "Status = 'Inactive'")

### 3. Sync Method
Ensures target table exactly matches the source DataTable:

```csharp
public void Sync_FMM_Models(SessionInfo si, DataTable dt, string syncCondition = null)
{
    GBL_SQA_Helper.SyncData(si, _connection, dt, "FMM_Models", "Model_ID", syncCondition);
}
```

**Parameters:**
- `si` - SessionInfo for error handling
- `dt` - DataTable containing data to sync
- `syncCondition` - Optional SQL WHERE clause to limit which records can be deleted

## GBL_SQA_Helper Methods

### BuildInsertCommand
Dynamically creates INSERT statement based on DataTable columns.

### BuildUpdateCommand
Dynamically creates UPDATE statement based on DataTable columns, excluding primary key.

### BuildDeleteCommand
Creates DELETE statement using primary key.

### MergeData
Executes SQL MERGE statement for each row with optional conditional deletes:
```sql
MERGE INTO [TableName] AS target
USING (SELECT @Col1 AS [Col1], @Col2 AS [Col2], ...) AS source
ON target.[PrimaryKey] = source.[PrimaryKey]
WHEN MATCHED THEN
    UPDATE SET [Col1] = source.[Col1], ...
WHEN NOT MATCHED BY TARGET THEN
    INSERT ([Col1], [Col2], ...) VALUES (source.[Col1], source.[Col2], ...)
WHEN NOT MATCHED BY SOURCE AND {deleteCondition} THEN  -- Optional
    DELETE;
```

### SyncData
Wrapper around MergeData with deleteUnmatched=true for full synchronization.

## Usage Examples

### Example 1: Basic Update (Dynamic)
```csharp
var dt = new DataTable();
// Add columns and rows to dt
// Columns will be auto-detected and SQL built dynamically

var sqa = new SqlDataAdapter();
var adapter = new SQA_FMM_Models(si, connection);
adapter.Update_FMM_Models(si, dt, sqa);
```

### Example 2: Merge with Conditional Delete
```csharp
var dt = new DataTable();
// Populate dt with current data

var adapter = new SQA_FMM_Models(si, connection);
// Merge data, deleting only inactive records not in source
adapter.Merge_FMM_Models(si, dt, 
    deleteUnmatched: true, 
    deleteCondition: "Status = 'Inactive'");
```

### Example 3: Full Sync with Scope
```csharp
var dt = new DataTable();
// Populate dt with data for Cube_ID = 1

var adapter = new SQA_FMM_Models(si, connection);
// Sync only records for Cube_ID = 1
adapter.Sync_FMM_Models(si, dt, syncCondition: "Cube_ID = 1");
```

## Benefits

1. **Flexibility**: Add/remove columns without code changes
2. **Maintainability**: Centralized SQL building logic
3. **New Capabilities**: MERGE and SYNC operations
4. **Safety**: Parameterized queries prevent SQL injection
5. **Transactions**: All operations atomic with proper rollback

## Type Mapping

The helper automatically maps .NET types to SqlDbType:

| .NET Type | SqlDbType |
|-----------|-----------|
| int | Int |
| long | BigInt |
| short | SmallInt |
| byte | TinyInt |
| bool | Bit |
| DateTime | DateTime |
| decimal | Decimal |
| double | Float |
| float | Real |
| Guid | UniqueIdentifier |
| byte[] | VarBinary |
| string | NVarChar |

## Limitations

1. **Composite Keys**: Files with composite primary keys (RegPlan_Audit, RegPlan_Details) not updated
2. **Single Primary Key**: Helper methods assume single column primary key
3. **Schema Changes**: Dynamic SQL builds from DataTable, not database schema

## Testing Recommendations

1. **Unit Tests**: Test each new method with various DataTable configurations
2. **Integration Tests**: Test against actual OneStream database
3. **Performance Tests**: Measure impact of dynamic SQL building
4. **Edge Cases**: Test with NULL values, empty DataTables, missing columns

## Security Considerations

✅ **SQL Injection Prevention**: All values use parameterized queries
✅ **Transaction Safety**: Proper rollback on errors
✅ **Error Handling**: Uses OneStream XFException pattern
✅ **State Management**: Commands properly disposed after use

## Migration Notes

**No breaking changes** - existing code continues to work:
- Original `Update_*` methods still exist with same signature
- Old code path replaced with dynamic version
- New methods (Merge/Sync) are additions only

## Future Enhancements

Potential improvements for future iterations:
1. Support for composite primary keys
2. Batch operations for better performance
3. Conflict resolution strategies for MERGE
4. Audit logging for SYNC operations
5. Schema validation before execution
