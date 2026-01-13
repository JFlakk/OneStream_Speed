# GBL_SQL_Command_Builder

## Overview
The `GBL_SQL_Command_Builder` is a utility class that dynamically generates INSERT, UPDATE, and DELETE SQL commands based on DataTable schema. This eliminates the need for hardcoded SQL statements and provides automatic schema synchronization.

**Available in Both Languages:**
- C# version: `GBL_SQL_Command_Builder.cs`
- VB.NET version: `GBL_SQL_Command_Builder.vb`

Both versions provide identical functionality and work with **any** `SqlConnection`, including:
- Application Database connections (`CreateApplicationDbConnInfo`)
- Merge Database connections (`CreateMergeDbConnInfo`)
- Any other database connection type

## Purpose
- **Eliminate Hardcoded SQL**: No more manually writing INSERT/UPDATE/DELETE statements
- **Automatic Schema Sync**: When database schema changes, the builder automatically adapts
- **Reduce Maintenance**: Centralized logic reduces code duplication
- **Improve Consistency**: All SQA files use the same pattern
- **Multi-Database Support**: Works seamlessly with Application DB and Merge DB

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

## Complete Example (C#)

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

## Complete Example (VB.NET)

```vb
Public Sub Update_FMM_Models(si As SessionInfo, dt As DataTable, sqa As SqlDataAdapter)
    Using transaction As SqlTransaction = _connection.BeginTransaction()
        Try
            ' Create and configure the builder
            Dim builder As New GBL_SQL_Command_Builder(_connection, "FMM_Models", dt)
            builder.SetPrimaryKey("Model_ID")
            builder.ExcludeFromUpdate("Model_ID", "Cube_ID", "Act_ID", "Create_Date", "Create_User")
            builder.ConfigureAdapter(sqa, transaction)

            ' Execute the update
            sqa.Update(dt)
            transaction.Commit()
            
            ' Cleanup
            sqa.InsertCommand = Nothing
            sqa.UpdateCommand = Nothing
            sqa.DeleteCommand = Nothing
        Catch
            transaction.Rollback()
            Throw
        End Try
    End Using
End Sub
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

## Database Connection Support

The command builder works with **any** `SqlConnection`, allowing you to use it with different database types:

### Application Database (C#)
```csharp
var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
using (var connection = new SqlConnection(dbConnApp.ConnectionString))
{
    connection.Open();
    var sqa = new SQA_FMM_Models(si, connection);
    var dt = new DataTable();
    var adapter = new SqlDataAdapter();
    
    // ... fill data ...
    
    // The builder works with Application DB
    sqa.Update_FMM_Models(si, dt, adapter);
}
```

### Merge Database (C#)
```csharp
var dbConnMerge = BRApi.Database.CreateMergeDbConnInfo(si);
using (var connection = new SqlConnection(dbConnMerge.ConnectionString))
{
    connection.Open();
    var sqa = new SQA_FMM_Models(si, connection);
    var dt = new DataTable();
    var adapter = new SqlDataAdapter();
    
    // ... fill data ...
    
    // The builder works with Merge DB too!
    sqa.Update_FMM_Models(si, dt, adapter);
}
```

### Application Database (VB.NET)
```vb
Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
Using connection As New SqlConnection(dbConnApp.ConnectionString)
    connection.Open()
    Dim sqa As New SQA_XFC_CMD_PGM_REQ(connection)
    Dim dt As New DataTable()
    Dim adapter As New SqlDataAdapter()
    
    ' ... fill data ...
    
    ' The builder works with Application DB
    sqa.Update_XFC_CMD_PGM_REQ(dt, adapter)
End Using
```

### Merge Database (VB.NET)
```vb
Dim dbConnMerge = BRApi.Database.CreateMergeDbConnInfo(si)
Using connection As New SqlConnection(dbConnMerge.ConnectionString)
    connection.Open()
    Dim sqa As New SQA_XFC_CMD_PGM_REQ(connection)
    Dim dt As New DataTable()
    Dim adapter As New SqlDataAdapter()
    
    ' ... fill data ...
    
    ' The builder works with Merge DB too!
    sqa.Update_XFC_CMD_PGM_REQ(dt, adapter)
End Using
```

## Common Patterns

### Single Primary Key (C#)
```csharp
builder.SetPrimaryKey("ID");
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
```

### Composite Primary Key (C#)
```csharp
builder.SetPrimaryKey("ID1", "ID2");
builder.ExcludeFromUpdate("ID1", "ID2", "Create_Date", "Create_User");
```

### Many Columns with Foreign Keys (C#)
```csharp
builder.SetPrimaryKey("Record_ID");
builder.ExcludeFromUpdate("Record_ID", "Parent_ID", "Related_ID", "Create_Date", "Create_User");
```

### Single Primary Key (VB.NET)
```vb
builder.SetPrimaryKey("ID")
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User")
```

### Composite Primary Key (VB.NET)
```vb
builder.SetPrimaryKey("ID1", "ID2")
builder.ExcludeFromUpdate("ID1", "ID2", "Create_Date", "Create_User")
```

## Troubleshooting

### Error: "Primary key columns must be set before building UPDATE command"
**Solution**: Call `SetPrimaryKey()` before `ConfigureAdapter()`

### Error: Column not found in DataTable
**Solution**: Ensure the DataTable has the same schema as the database table

### Commands not executing
**Solution**: Ensure you're calling `sqa.Update(dt)` after configuring the adapter

## Migration Guide

### C# - Before (Hardcoded)
```csharp
string insertQuery = @"INSERT INTO Table (...) VALUES (...)";
sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
sqa.InsertCommand.Parameters.Add("@Col1", SqlDbType.Int).SourceColumn = "Col1";
// ... 50+ lines of parameter definitions ...
```

### C# - After (Dynamic)
```csharp
var builder = new GBL_SQL_Command_Builder(_connection, "Table", dt);
builder.SetPrimaryKey("ID");
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User");
builder.ConfigureAdapter(sqa, transaction);
```

### VB.NET - Before (Hardcoded)
```vb
Dim insertQuery As String = "INSERT INTO Table (...) VALUES (...)"
sqa.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
sqa.InsertCommand.Parameters.Add("@Col1", SqlDbType.Int).SourceColumn = "Col1"
' ... 50+ lines of parameter definitions ...
```

### VB.NET - After (Dynamic)
```vb
Dim builder As New GBL_SQL_Command_Builder(_connection, "Table", dt)
builder.SetPrimaryKey("ID")
builder.ExcludeFromUpdate("ID", "Create_Date", "Create_User")
builder.ConfigureAdapter(sqa, transaction)
```

## Performance Considerations

- **Command Generation**: Commands are generated once per call, minimal overhead
- **Batch Size**: Set via `adapter.UpdateBatchSize` (done automatically)
- **Transaction Scope**: Always use transactions for consistency
- **Memory**: DataTable schema reflection is lightweight

## See Also

- **C# Examples:**
  - SQA_FMM_Models.cs - Simple single-key example
  - SQA_RegPlan_Audit.cs - Complex composite-key example
  - SQA_DDM_Config_Menu_Layout.cs - Many-column example

- **VB.NET Examples:**
  - After migration: SQA_XFC_CMD_PGM_REQ.vb
  - After migration: SQA_XFC_CMD_SPLN_REQ.vb
  - After migration: SQA_XFC_CMD_UFR.vb

## Language Support

The command builder is available in both languages:

| Language | File | Status |
|----------|------|--------|
| C# | GBL_SQL_Command_Builder.cs | ✓ In use by all C# SQA files |
| VB.NET | GBL_SQL_Command_Builder.vb | ✓ Available for VB.NET SQA files |

**Current Implementation Status:**
- **26 C# SQA files**: Already using GBL_SQL_Command_Builder ✓
- **18 VB.NET SQA files**: Can now migrate to use GBL_SQL_Command_Builder

## Support

For questions or issues:
1. Review existing SQA files for examples
2. Check the GBL_SQL_Command_Builder source code
3. Consult the OneStream XF documentation

---
**Version**: 1.0  
**Last Updated**: 2026-01-12  
**Author**: OneStream Development Team
