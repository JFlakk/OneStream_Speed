# SQA_GBL_Command_Builder

## Overview
The `SQA_GBL_Command_Builder` is a comprehensive SQL Adapter wrapper that provides unified methods for common database operations including UPDATE, FILL, and MERGE operations. It wraps the `GBL_SQL_Command_Builder` to provide dynamic command generation and adds additional functionality for data retrieval and merging.

## Purpose
- **Unified Interface**: Single class for all common SQL operations
- **Dynamic Command Generation**: Automatic INSERT/UPDATE/DELETE via `GBL_SQL_Command_Builder`
- **Data Retrieval**: Flexible Fill methods with parameterized queries
- **Data Merging**: Support for simple and composite key merges
- **Consistency**: Standard patterns across all SQL Adapter files

## Features

### 1. Update Operations
Dynamic UPDATE operations using `GBL_SQL_Command_Builder`:

#### UpdateTable - Full Control
```csharp
public void UpdateTable(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa,
    string[] primaryKeyColumns, string[] excludeFromUpdate = null, string[] excludeFromInsert = null)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);
builder.UpdateTable(si, "FMM_Models", dt, sqa, 
    new[] { "Model_ID" }, 
    new[] { "Model_ID", "Create_Date", "Create_User" });
```

#### UpdateTableSimple - Single Primary Key
```csharp
public void UpdateTableSimple(SessionInfo si, string tableName, DataTable dt, 
    SqlDataAdapter sqa, string primaryKeyColumn)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);
builder.UpdateTableSimple(si, "DDM_Config", dt, sqa, "Config_ID");
```

#### UpdateTableComposite - Composite Primary Keys
```csharp
public void UpdateTableComposite(SessionInfo si, string tableName, DataTable dt, 
    SqlDataAdapter sqa, params string[] primaryKeyColumns)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);
builder.UpdateTableComposite(si, "FMM_Calc_Unit_Assign", dt, sqa, 
    "Cube_ID", "Model_ID", "Calc_ID");
```

### 2. Fill Operations
Dynamic data retrieval with parameterized queries:

#### FillDataTable - Parameterized Query
```csharp
public void FillDataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dt, 
    string sql, params SqlParameter[] sqlparams)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);
var dt = new DataTable();
var sqa = new SqlDataAdapter();

string sql = @"
    SELECT * FROM FMM_Models 
    WHERE Cube_ID = @CubeID 
    AND Status = @Status";

var parameters = new[] {
    new SqlParameter("@CubeID", SqlDbType.Int) { Value = cubeId },
    new SqlParameter("@Status", SqlDbType.NVarChar) { Value = "Active" }
};

builder.FillDataTable(si, sqa, dt, sql, parameters);
```

### 3. Merge Operations
Combine DataTables with support for primary key handling:

#### MergeDataTable - Simple Merge
```csharp
public void MergeDataTable(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
    bool preserveChanges = true, MissingSchemaAction missingSchemaAction = MissingSchemaAction.Error)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);

// Merge source into target, preserving target changes
builder.MergeDataTable(si, targetDt, sourceDt, true);

// Merge source into target, overwriting target changes
builder.MergeDataTable(si, targetDt, sourceDt, false);
```

#### MergeDataTableWithKeys - Composite Key Merge
```csharp
public void MergeDataTableWithKeys(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
    string[] primaryKeyColumns, bool preserveChanges = false)
```

**Example:**
```csharp
var builder = new SQA_GBL_Command_Builder(si, connection);

// Merge with composite primary key
builder.MergeDataTableWithKeys(si, targetDt, sourceDt, 
    new[] { "Cube_ID", "Model_ID", "Calc_ID" }, 
    false);

// Merge with single primary key
builder.MergeDataTableWithKeys(si, targetDt, sourceDt, 
    new[] { "Config_ID" });
```

## Complete Usage Examples

### Example 1: Load, Modify, and Save Data
```csharp
public void ProcessModels(SessionInfo si, int cubeId)
{
    using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si).OpenConnection())
    {
        var builder = new SQA_GBL_Command_Builder(si, connection);
        var dt = new DataTable();
        var sqa = new SqlDataAdapter();
        
        try
        {
            // Load data
            string sql = "SELECT * FROM FMM_Models WHERE Cube_ID = @CubeID";
            var param = new SqlParameter("@CubeID", SqlDbType.Int) { Value = cubeId };
            builder.FillDataTable(si, sqa, dt, sql, param);
            
            // Modify data
            foreach (DataRow row in dt.Rows)
            {
                row["Update_Date"] = DateTime.Now;
                row["Update_User"] = si.UserName;
            }
            
            // Save changes
            builder.UpdateTableSimple(si, "FMM_Models", dt, sqa, "Model_ID");
        }
        catch (Exception ex)
        {
            throw new XFException(si, $"Error processing models: {ex.Message}", ex);
        }
    }
}
```

### Example 2: Merge Two DataSets
```csharp
public void MergeConfigurations(SessionInfo si, int cubeId)
{
    using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si).OpenConnection())
    {
        var builder = new SQA_GBL_Command_Builder(si, connection);
        var masterDt = new DataTable();
        var updateDt = new DataTable();
        var sqa = new SqlDataAdapter();
        
        try
        {
            // Load master configuration
            string masterSql = "SELECT * FROM DDM_Config WHERE Cube_ID = @CubeID";
            builder.FillDataTable(si, sqa, masterDt, masterSql, 
                new SqlParameter("@CubeID", cubeId));
            
            // Load updates from staging table
            string updateSql = "SELECT * FROM DDM_Config_Staging WHERE Cube_ID = @CubeID";
            builder.FillDataTable(si, sqa, updateDt, updateSql, 
                new SqlParameter("@CubeID", cubeId));
            
            // Merge updates into master
            builder.MergeDataTableWithKeys(si, masterDt, updateDt, 
                new[] { "Config_ID" }, false);
            
            // Save merged data
            builder.UpdateTableSimple(si, "DDM_Config", masterDt, sqa, "Config_ID");
        }
        catch (Exception ex)
        {
            throw new XFException(si, $"Error merging configurations: {ex.Message}", ex);
        }
    }
}
```

### Example 3: Complex Multi-Table Operation
```csharp
public void SyncModelData(SessionInfo si, int sourceModelId, int targetModelId)
{
    using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si).OpenConnection())
    {
        var builder = new SQA_GBL_Command_Builder(si, connection);
        
        // Load source model configuration
        var sourceDt = new DataTable();
        var sqa = new SqlDataAdapter();
        
        string sql = @"
            SELECT Model_ID, Cube_ID, Name, Calc_Type, Status
            FROM FMM_Models 
            WHERE Model_ID = @ModelID";
            
        builder.FillDataTable(si, sqa, sourceDt, sql, 
            new SqlParameter("@ModelID", sourceModelId));
        
        // Load target model configuration
        var targetDt = new DataTable();
        builder.FillDataTable(si, sqa, targetDt, sql, 
            new SqlParameter("@ModelID", targetModelId));
        
        // Update target model with source data (except primary key)
        if (sourceDt.Rows.Count > 0 && targetDt.Rows.Count > 0)
        {
            var sourceRow = sourceDt.Rows[0];
            var targetRow = targetDt.Rows[0];
            
            targetRow["Name"] = sourceRow["Name"];
            targetRow["Calc_Type"] = sourceRow["Calc_Type"];
            targetRow["Status"] = sourceRow["Status"];
            targetRow["Update_Date"] = DateTime.Now;
            targetRow["Update_User"] = si.UserName;
            
            // Save changes
            builder.UpdateTableSimple(si, "FMM_Models", targetDt, sqa, "Model_ID");
        }
    }
}
```

## Best Practices

### 1. Always Use Using Blocks
```csharp
using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si).OpenConnection())
{
    var builder = new SQA_GBL_Command_Builder(si, connection);
    // ... operations ...
}
```

### 2. Parameterize All Queries
```csharp
// Good - Parameterized
string sql = "SELECT * FROM Table WHERE ID = @ID";
builder.FillDataTable(si, sqa, dt, sql, new SqlParameter("@ID", id));

// Bad - String concatenation (SQL injection risk)
string sql = $"SELECT * FROM Table WHERE ID = {id}";
```

### 3. Handle Exceptions Properly
```csharp
try
{
    builder.UpdateTableSimple(si, tableName, dt, sqa, "ID");
}
catch (Exception ex)
{
    throw new XFException(si, $"Error updating {tableName}: {ex.Message}", ex);
}
```

### 4. Update Audit Columns
```csharp
foreach (DataRow row in dt.Rows)
{
    if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
    {
        row["Update_Date"] = DateTime.Now;
        row["Update_User"] = si.UserName;
    }
}
```

### 5. Choose the Right Merge Method
```csharp
// Simple merge without key setup
builder.MergeDataTable(si, targetDt, sourceDt);

// When you need to specify primary keys explicitly
builder.MergeDataTableWithKeys(si, targetDt, sourceDt, 
    new[] { "ID1", "ID2" });
```

## Common Patterns

### Pattern 1: Load-Modify-Save
```csharp
// Load
builder.FillDataTable(si, sqa, dt, sql, parameters);

// Modify
foreach (DataRow row in dt.Rows) { /* modify */ }

// Save
builder.UpdateTableSimple(si, tableName, dt, sqa, primaryKey);
```

### Pattern 2: Load-Merge-Save
```csharp
// Load both datasets
builder.FillDataTable(si, sqa, masterDt, sql1, params1);
builder.FillDataTable(si, sqa, updateDt, sql2, params2);

// Merge
builder.MergeDataTableWithKeys(si, masterDt, updateDt, keys);

// Save
builder.UpdateTableComposite(si, tableName, masterDt, sqa, keys);
```

### Pattern 3: Multi-Table Update
```csharp
// Load related data
builder.FillDataTable(si, sqa, dt1, sql1, params1);
builder.FillDataTable(si, sqa, dt2, sql2, params2);

// Update each table
builder.UpdateTableSimple(si, table1, dt1, sqa, key1);
builder.UpdateTableComposite(si, table2, dt2, sqa, key2a, key2b);
```

## Error Handling

### Fill Operation Errors
```csharp
try
{
    builder.FillDataTable(si, sqa, dt, sql, parameters);
}
catch (SqlException ex)
{
    throw new XFException(si, $"Database error: {ex.Message}", ex);
}
catch (Exception ex)
{
    throw new XFException(si, $"Error filling DataTable: {ex.Message}", ex);
}
```

### Merge Operation Errors
The merge methods automatically wrap exceptions in `XFException`:
```csharp
// Automatically handles exceptions
builder.MergeDataTable(si, targetDt, sourceDt);

// If you need custom error handling
try
{
    builder.MergeDataTable(si, targetDt, sourceDt);
}
catch (XFException ex)
{
    BRApi.ErrorLog.LogMessage(si, $"Merge failed: {ex.Message}");
    throw;
}
```

## Performance Considerations

### 1. Batch Size
Update operations automatically set `UpdateBatchSize = 0` for best performance.

### 2. Connection Management
Always use `using` blocks to ensure proper connection disposal.

### 3. DataTable Size
Be mindful of memory when loading large datasets. Consider pagination:
```csharp
string sql = @"
    SELECT TOP (@PageSize) * 
    FROM Table 
    WHERE ID > @LastID 
    ORDER BY ID";
```

### 4. Merge Performance
For large datasets, ensure primary keys are set before merging:
```csharp
targetDt.PrimaryKey = new[] { targetDt.Columns["ID"] };
sourceDt.PrimaryKey = new[] { sourceDt.Columns["ID"] };
builder.MergeDataTable(si, targetDt, sourceDt);
```

## Troubleshooting

### Error: "Primary key columns must be set before building UPDATE command"
**Solution**: Call `UpdateTableSimple` or `UpdateTableComposite` with valid primary key column names.

### Error: "Error merging DataTable: ..."
**Solution**: Ensure both DataTables have compatible schemas and valid primary keys.

### Fill returns no rows
**Solution**: 
1. Verify SQL query syntax
2. Check parameter values
3. Ensure database permissions
4. Verify data exists in the table

### Merge creates duplicate rows
**Solution**: Ensure primary keys are correctly set on both tables before merging.

## See Also

- **GBL_SQL_Command_Builder** - Underlying dynamic command builder
- **README_GBL_SQL_Command_Builder.md** - Detailed command builder documentation
- **SQL_GBL_Get_DataSets.cs** - Example SQL Adapter for reference
- **SQA_FMM_Act_Config.cs** - Example with Fill and Update methods

## Migration from Traditional SQL Adapters

### Before (Traditional Pattern)
```csharp
public void Fill_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, 
    string sql, params SqlParameter[] sqlparams)
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

// Then separate Update method with hardcoded SQL...
```

### After (Using SQA_GBL_Command_Builder)
```csharp
public void ProcessConfig(SessionInfo si, string sql, SqlParameter[] sqlparams)
{
    using (var connection = BRApi.Database.CreateApplicationDbConnInfo(si).OpenConnection())
    {
        var builder = new SQA_GBL_Command_Builder(si, connection);
        var dt = new DataTable();
        var sqa = new SqlDataAdapter();
        
        // Fill
        builder.FillDataTable(si, sqa, dt, sql, sqlparams);
        
        // Modify data as needed...
        
        // Update (no hardcoded SQL needed!)
        builder.UpdateTableSimple(si, "TableName", dt, sqa, "ID");
    }
}
```

## Support

For questions or issues:
1. Review the examples in this document
2. Check the `GBL_SQL_Command_Builder` documentation
3. Examine existing SQL Adapter implementations
4. Consult the OneStream XF documentation

---
**Version**: 1.0  
**Last Updated**: 2026-01-13  
**Author**: OneStream Development Team
