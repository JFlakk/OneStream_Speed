# SQA_GBL_Command_Builder

## Overview
The `SQA_GBL_Command_Builder` is a SQL Adapter that wraps the `GBL_SQL_Command_Builder` class to provide a simplified interface for updating database tables. Instead of manually configuring the command builder, you can now pass DataTables directly and let the adapter handle all the SQL command generation automatically.

## Purpose
- **Simplified API**: Call a single method instead of multiple configuration steps
- **Consistent Pattern**: Follows the standard SQL Adapter pattern used across OS Consultant Tools
- **Accessible from Anywhere**: Can be instantiated from any assembly in OS Consultant Tools
- **Automatic Command Management**: Handles transaction management and cleanup automatically

## Key Benefits
1. **Less Boilerplate**: Reduce 10+ lines of code to a single method call
2. **Standard Conventions**: Built-in support for common patterns (single key, composite key)
3. **Error Handling**: Automatic transaction rollback on errors
4. **Cleanup**: Automatic disposal of commands after use

## Basic Usage

### Method 1: Simple Update (Single Primary Key)
This is the most common pattern - single primary key with standard audit columns.

```csharp
// In any assembly's SQL Adapter
public void Update_MyTable(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    // Create the command builder adapter
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    // Single method call to update the table
    gblBuilder.UpdateTableSimple(si, "MyTable_Name", dt, sqa, "MyTable_ID");
}
```

**What it does:**
- Uses "MyTable_ID" as the primary key
- Automatically excludes: "MyTable_ID", "Create_Date", "Create_User" from UPDATE
- Handles INSERT, UPDATE, DELETE based on row states
- Manages transactions automatically

### Method 2: Composite Primary Key
For tables with multiple primary keys:

```csharp
public void Update_MyTable(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    gblBuilder.UpdateTableComposite(si, "MyTable_Name", dt, sqa, 
        "Key1", "Key2", "Key3");
}
```

**What it does:**
- Uses all specified columns as composite primary key
- Automatically excludes: all keys + "Create_Date", "Create_User" from UPDATE

### Method 3: Custom Exclusions
For advanced scenarios where you need custom control:

```csharp
public void Update_MyTable(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    string[] primaryKeys = new[] { "MyTable_ID" };
    string[] excludeFromUpdate = new[] { "MyTable_ID", "Foreign_Key_ID", "Create_Date", "Create_User" };
    string[] excludeFromInsert = new[] { "MyTable_ID" }; // Identity column
    
    gblBuilder.UpdateTable(si, "MyTable_Name", dt, sqa, 
        primaryKeys, excludeFromUpdate, excludeFromInsert);
}
```

### Method 4: Advanced - Direct Builder Access
For complete control, get the underlying command builder:

```csharp
public void Update_MyTable(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    using (SqlTransaction transaction = _connection.BeginTransaction())
    {
        try
        {
            var builder = gblBuilder.GetCommandBuilder(si, "MyTable_Name", dt);
            builder.SetPrimaryKey("MyTable_ID");
            builder.ExcludeFromUpdate("MyTable_ID", "Create_Date", "Create_User");
            builder.ConfigureAdapter(sqa, transaction);
            
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

## Complete Example: FMM_Act_Config

### Before (Old Pattern)
```csharp
public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    using (SqlTransaction transaction = _connection.BeginTransaction())
    {
        try
        {
            // Use GBL_SQL_Command_Builder to dynamically generate commands
            var builder = new GBL_SQL_Command_Builder(_connection, "FMM_Act_Config", dt);
            builder.SetPrimaryKey("Act_ID");
            builder.ExcludeFromUpdate("Act_ID", "Cube_ID", "Create_Date", "Create_User");
            builder.ConfigureAdapter(sqa, transaction);

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

### After (New Pattern with SQL Adapter)
```csharp
public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    string[] primaryKeys = new[] { "Act_ID" };
    string[] excludeFromUpdate = new[] { "Act_ID", "Cube_ID", "Create_Date", "Create_User" };
    
    gblBuilder.UpdateTable(si, "FMM_Act_Config", dt, sqa, primaryKeys, excludeFromUpdate);
}
```

**Result:** 5 lines instead of 19 lines, with the same functionality!

## Migration Guide

### Step 1: Identify Your Pattern
Determine which update method fits your needs:
- **Single primary key + standard audit columns** → Use `UpdateTableSimple()`
- **Composite primary key + standard audit columns** → Use `UpdateTableComposite()`
- **Custom exclusions** → Use `UpdateTable()`
- **Need full control** → Use `GetCommandBuilder()`

### Step 2: Update Your SQL Adapter
Replace the transaction block with the appropriate method call.

### Step 3: Test
Run your existing update operations to verify functionality.

## Common Patterns

### Pattern 1: Simple Config Table
```csharp
// Table: FMM_Config
// Primary Key: Config_ID
// Audit: Create_Date, Create_User

public void Update_FMM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    gblBuilder.UpdateTableSimple(si, "FMM_Config", dt, sqa, "Config_ID");
}
```

### Pattern 2: Foreign Key Constraints
```csharp
// Table: FMM_Details
// Primary Key: Detail_ID
// Foreign Keys: Master_ID, Related_ID (immutable after insert)

public void Update_FMM_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    string[] primaryKeys = new[] { "Detail_ID" };
    string[] excludeFromUpdate = new[] { 
        "Detail_ID", "Master_ID", "Related_ID", "Create_Date", "Create_User" 
    };
    
    gblBuilder.UpdateTable(si, "FMM_Details", dt, sqa, primaryKeys, excludeFromUpdate);
}
```

### Pattern 3: Composite Key Mapping Table
```csharp
// Table: RegPlan_Details
// Composite Key: Reg_ID, Plan_ID, Detail_Seq

public void Update_RegPlan_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    gblBuilder.UpdateTableComposite(si, "RegPlan_Details", dt, sqa, 
        "Reg_ID", "Plan_ID", "Detail_Seq");
}
```

### Pattern 4: Identity Column
```csharp
// Table: Audit_Log
// Primary Key: Log_ID (Identity column - auto-generated)

public void Update_Audit_Log(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    
    string[] primaryKeys = new[] { "Log_ID" };
    string[] excludeFromUpdate = new[] { "Log_ID", "Create_Date" };
    string[] excludeFromInsert = new[] { "Log_ID" }; // Don't insert identity column
    
    gblBuilder.UpdateTable(si, "Audit_Log", dt, sqa, 
        primaryKeys, excludeFromUpdate, excludeFromInsert);
}
```

## Method Reference

### UpdateTableSimple()
**Use when:** Single primary key with standard audit columns  
**Parameters:**
- `tableName`: Database table name
- `dt`: DataTable with changes
- `sqa`: SqlDataAdapter
- `primaryKeyColumn`: Name of the primary key column

**Auto-excludes from UPDATE:** Primary key, Create_Date, Create_User

### UpdateTableComposite()
**Use when:** Multiple primary keys with standard audit columns  
**Parameters:**
- `tableName`: Database table name
- `dt`: DataTable with changes
- `sqa`: SqlDataAdapter
- `primaryKeyColumns`: Array of primary key column names

**Auto-excludes from UPDATE:** All primary keys, Create_Date, Create_User

### UpdateTable()
**Use when:** Need custom control over exclusions  
**Parameters:**
- `tableName`: Database table name
- `dt`: DataTable with changes
- `sqa`: SqlDataAdapter
- `primaryKeyColumns`: Array of primary key column names
- `excludeFromUpdate`: (Optional) Columns to exclude from UPDATE
- `excludeFromInsert`: (Optional) Columns to exclude from INSERT

### GetCommandBuilder()
**Use when:** Need direct access to GBL_SQL_Command_Builder  
**Parameters:**
- `tableName`: Database table name
- `dt`: DataTable with schema

**Returns:** Configured `GBL_SQL_Command_Builder` instance

## Best Practices

### 1. Always Use the Simplest Method That Works
```csharp
// Good - simple and clear
gblBuilder.UpdateTableSimple(si, "Config", dt, sqa, "Config_ID");

// Avoid - unnecessarily complex
string[] pk = new[] { "Config_ID" };
string[] exclude = new[] { "Config_ID", "Create_Date", "Create_User" };
gblBuilder.UpdateTable(si, "Config", dt, sqa, pk, exclude);
```

### 2. Be Explicit About Foreign Keys
```csharp
// Good - clearly shows immutable foreign keys
string[] excludeFromUpdate = new[] { 
    "Record_ID",        // Primary key
    "Parent_ID",        // Foreign key - immutable
    "Related_Type_ID",  // Foreign key - immutable
    "Create_Date", 
    "Create_User" 
};
```

### 3. Document Identity Columns
```csharp
// Good - comment explains why we exclude from insert
string[] excludeFromInsert = new[] { "ID" }; // Identity column
```

### 4. Use Consistent Naming
Follow the pattern: `Update_<TableName>()`

## Error Handling

The adapter automatically handles errors and rolls back transactions:

```csharp
// You don't need to write this anymore - it's built-in!
try
{
    // Update logic
}
catch (Exception)
{
    transaction.Rollback();
    throw;
}
```

If an error occurs during the update:
1. The transaction is automatically rolled back
2. All commands are cleaned up
3. The original exception is re-thrown

## Performance Considerations

- **Command Generation**: Occurs once per update operation
- **Transaction Management**: Automatic transaction scope per call
- **Batch Updates**: UpdateBatchSize is set automatically
- **Memory**: Minimal overhead - just wraps existing functionality

## Calling from Other Assemblies

The SQL Adapter can be called from any assembly in OS Consultant Tools:

```csharp
// From DDM, FMM, MDM, or any other assembly
using (var dbInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
{
    using (var connection = new SqlConnection(dbInfo.ConnectionString))
    {
        connection.Open();
        
        // Instantiate the GBL SQL Adapter
        var gblBuilder = new SQA_GBL_Command_Builder(si, connection);
        
        using (var sqa = new SqlDataAdapter())
        {
            // Use any of the update methods
            gblBuilder.UpdateTableSimple(si, "MyTable", dt, sqa, "MyTable_ID");
        }
    }
}
```

## Troubleshooting

### Error: "Primary key columns must be set before building UPDATE command"
**Solution**: Ensure you're passing the primary key column name(s) to the method.

### Error: Column not found in DataTable
**Solution**: Ensure your DataTable schema matches the database table schema.

### Transaction Issues
**Solution**: The adapter manages transactions automatically. Don't wrap the call in another transaction.

### Commands Not Executing
**Solution**: Ensure you're calling the update method with a valid DataTable that has modified rows.

## See Also

- [GBL_SQL_Command_Builder](../GBL%20Support%20Classes/README_GBL_SQL_Command_Builder.md) - Underlying command builder
- [SQA_GBL_Get_DataSets](SQL_GBL_Get_DataSets.cs) - For SELECT operations
- [SQA_GBL_Get_Max_ID](SQL_GBL_Get_Max_ID.cs) - For getting next ID values

## Version History

**Version 1.0** (2026-01-13)
- Initial release
- Support for simple, composite, and custom update patterns
- Automatic transaction management
- Built-in error handling

---
**Author**: OneStream Development Team  
**Last Updated**: 2026-01-13
