# GBL_SQL_Command_Builder

## Overview
The `GBL_SQL_Command_Builder` is a utility class that dynamically generates INSERT, UPDATE, and DELETE SQL commands based on DataTable schema. This eliminates the need for hardcoded SQL statements and provides automatic schema synchronization.

## Purpose
- **Eliminate Hardcoded SQL**: No more manually writing INSERT/UPDATE/DELETE statements
- **Automatic Schema Sync**: When database schema changes, the builder automatically adapts
- **Reduce Maintenance**: Centralized logic reduces code duplication
- **Improve Consistency**: All SQA files use the same pattern

## Basic Usage

### 1. Initialize the Builder
```csharp
var builder = new GBL_SQL_Command_Builder(connection, "TableName", dataTable);
```

### 2. Configure Primary Key
```csharp
// Single primary key
builder.SetPrimaryKey("ID");

// Composite primary key
builder.SetPrimaryKey("ID1", "ID2", "ID3");
```

### 3. Exclude Columns from Update
```csharp
// Typically exclude primary keys and audit columns
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
```

### 4. Configure SqlDataAdapter
```csharp
builder.ConfigureAdapter(sqlDataAdapter, transaction);
```

## Complete Example

```csharp
public void Update_FMM_Models(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    using (SqlTransaction transaction = _connection.BeginTransaction())
    {
        try
        {
            // Create and configure the builder
            var builder = new GBL_SQL_Command_Builder(_connection, "FMM_Models", dt);
            builder.SetPrimaryKey("Model_ID");
            builder.ExcludeFromUpdate("Model_ID", "Cube_ID", "Act_ID", "Create_Date", "Create_User");
            builder.ConfigureAdapter(sqa, transaction);

            // Execute the update
            sqa.Update(dt);
            transaction.Commit();
            
            // Cleanup
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

## Advanced Features

### Excluding Columns from Insert
```csharp
// Exclude identity columns or computed columns
builder.ExcludeFromInsert("ID", "ComputedColumn");
```

### Type Mapping
The builder automatically maps .NET types to SQL Server types:
- `int` → `SqlDbType.Int`
- `string` → `SqlDbType.NVarChar`
- `DateTime` → `SqlDbType.DateTime`
- `decimal` → `SqlDbType.Decimal`
- `Guid` → `SqlDbType.UniqueIdentifier`
- `bool` → `SqlDbType.Bit`
- And more...

### Generated Commands
The builder creates three commands:

#### INSERT Command
```sql
INSERT INTO TableName (
    Column1, Column2, Column3
) VALUES (
    @Column1, @Column2, @Column3
)
```

#### UPDATE Command
```sql
UPDATE TableName SET
    Column2 = @Column2,
    Column3 = @Column3
WHERE Column1 = @Column1
```

#### DELETE Command
```sql
DELETE FROM TableName
WHERE Column1 = @Column1
```

## Best Practices

### 1. Always Use Transactions
```csharp
using (SqlTransaction transaction = _connection.BeginTransaction())
{
    // ... builder code ...
}
```

### 2. Exclude Audit Columns from Updates
```csharp
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
```

### 3. Clean Up Commands
```csharp
sqa.InsertCommand = null;
sqa.UpdateCommand = null;
sqa.DeleteCommand = null;
```

### 4. Handle Exceptions Properly
```csharp
try
{
    sqa.Update(dt);
    transaction.Commit();
}
catch (Exception)
{
    transaction.Rollback();
    throw;
}
```

## Common Patterns

### Single Primary Key
```csharp
builder.SetPrimaryKey("ID");
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
```

### Composite Primary Key
```csharp
builder.SetPrimaryKey("ID1", "ID2");
builder.ExcludeFromUpdate("ID1", "ID2", "Create_Date", "Create_User");
```

### Many Columns with Foreign Keys
```csharp
builder.SetPrimaryKey("Record_ID");
builder.ExcludeFromUpdate("Record_ID", "Parent_ID", "Related_ID", "Create_Date", "Create_User");
```

## Troubleshooting

### Error: "Primary key columns must be set before building UPDATE command"
**Solution**: Call `SetPrimaryKey()` before `ConfigureAdapter()`

### Error: Column not found in DataTable
**Solution**: Ensure the DataTable has the same schema as the database table

### Commands not executing
**Solution**: Ensure you're calling `sqa.Update(dt)` after configuring the adapter

## Migration Guide

### Before (Hardcoded)
```csharp
string insertQuery = @"INSERT INTO Table (...) VALUES (...)";
sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
sqa.InsertCommand.Parameters.Add("@Col1", SqlDbType.Int).SourceColumn = "Col1";
// ... 50+ lines of parameter definitions ...
```

### After (Dynamic)
```csharp
var builder = new GBL_SQL_Command_Builder(_connection, "Table", dt);
builder.SetPrimaryKey("ID");
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
builder.ConfigureAdapter(sqa, transaction);
```

## Performance Considerations

- **Command Generation**: Commands are generated once per call, minimal overhead
- **Batch Size**: Set via `adapter.UpdateBatchSize` (done automatically)
- **Transaction Scope**: Always use transactions for consistency
- **Memory**: DataTable schema reflection is lightweight

## See Also

- SQA_FMM_Models.cs - Simple single-key example
- SQA_RegPlan_Audit.cs - Complex composite-key example
- SQA_DDM_Config_Menu_Layout.cs - Many-column example

## Support

For questions or issues:
1. Review existing SQA files for examples
2. Check the GBL_SQL_Command_Builder source code
3. Consult the OneStream XF documentation

---
**Version**: 1.0  
**Last Updated**: 2026-01-12  
**Author**: OneStream Development Team
