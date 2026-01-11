# Dynamic Column Update Examples

This document provides practical examples of using the new `UpdateDataTableDynamic` method for efficient partial column updates.

## Problem Statement

**Before**: You needed to update just 1 column in a table, but had to define all 50+ columns:
```csharp
var columnDefinitions = new List<ColumnDefinition>
{
    new ColumnDefinition("ID", SqlDbType.Int),
    new ColumnDefinition("Col1", SqlDbType.NVarChar, 100),
    new ColumnDefinition("Col2", SqlDbType.NVarChar, 100),
    // ... 47 more columns ...
    new ColumnDefinition("Status", SqlDbType.NVarChar, 20)  // The one you want to update
};
```

**After**: Just pass a DataTable with only the columns you need:
```csharp
// DataTable only has: ID + Status
_helper.UpdateDataTableDynamic(si, dt, sqa, "MyTable", new List<string> { "ID" });
```

## Example 1: Update Single Column (Simple Primary Key)

### Scenario
Update the `Status` column for multiple products without touching other columns.

```csharp
public void UpdateProductStatus(SessionInfo si, SqlConnection connection, Dictionary<int, string> productStatuses)
{
    var helper = new GBL_SQA_Helper(connection);
    
    // Create DataTable with only columns we need
    DataTable dt = new DataTable();
    dt.Columns.Add("Product_ID", typeof(int));      // Primary key
    dt.Columns.Add("Status", typeof(string));        // Column to update
    
    // Populate with data
    foreach (var kvp in productStatuses)
    {
        dt.Rows.Add(kvp.Key, kvp.Value);
    }
    
    // Update - helper infers types automatically
    using (SqlDataAdapter sqa = new SqlDataAdapter())
    {
        var primaryKeyColumns = new List<string> { "Product_ID" };
        helper.UpdateDataTableDynamic(si, dt, sqa, "Products", primaryKeyColumns);
    }
}
```

**Generated SQL** (automatically):
```sql
UPDATE [Products] 
SET [Status] = @Status 
WHERE [Product_ID] = @Product_ID
```

## Example 2: Update Multiple Columns (Composite Primary Key)

### Scenario
Update monthly values in RegPlan_Details without defining all 50+ columns in the table.

```csharp
public void UpdateMonthlyValues(SessionInfo si, SqlConnection connection)
{
    var helper = new GBL_SQA_Helper(connection);
    
    // Create DataTable with only what we need
    DataTable dt = new DataTable();
    
    // Add composite primary key columns
    dt.Columns.Add("RegPlan_ID", typeof(Guid));
    dt.Columns.Add("Year", typeof(string));
    dt.Columns.Add("Plan_Units", typeof(string));
    dt.Columns.Add("Account", typeof(string));
    
    // Add columns to update
    dt.Columns.Add("Month1", typeof(decimal));
    dt.Columns.Add("Month2", typeof(decimal));
    
    // Add data
    var row = dt.NewRow();
    row["RegPlan_ID"] = Guid.NewGuid();
    row["Year"] = "2024";
    row["Plan_Units"] = "Units";
    row["Account"] = "5000";
    row["Month1"] = 1000.50m;
    row["Month2"] = 1500.75m;
    dt.Rows.Add(row);
    
    // Update - only updates Month1 and Month2, leaves other months untouched
    using (SqlDataAdapter sqa = new SqlDataAdapter())
    {
        var primaryKeyColumns = new List<string> { "RegPlan_ID", "Year", "Plan_Units", "Account" };
        helper.UpdateDataTableDynamic(si, dt, sqa, "RegPlan_Details", primaryKeyColumns);
    }
}
```

**Generated SQL** (automatically):
```sql
UPDATE [RegPlan_Details] 
SET [Month1] = @Month1, [Month2] = @Month2 
WHERE [RegPlan_ID] = @RegPlan_ID 
  AND [Year] = @Year 
  AND [Plan_Units] = @Plan_Units 
  AND [Account] = @Account
```

## Example 3: Bulk Status Update from Query Results

### Scenario
Read data from one table, calculate new values, update another table.

```csharp
public void UpdateCalculatedValues(SessionInfo si, SqlConnection connection)
{
    var helper = new GBL_SQA_Helper(connection);
    
    // Query data from source
    DataTable sourceData = new DataTable();
    using (SqlDataAdapter sqa = new SqlDataAdapter("SELECT ID, SomeValue FROM SourceTable", connection))
    {
        sqa.Fill(sourceData);
    }
    
    // Create target DataTable with calculated values
    DataTable targetData = new DataTable();
    targetData.Columns.Add("ID", typeof(int));
    targetData.Columns.Add("CalculatedValue", typeof(decimal));
    targetData.Columns.Add("LastCalculated", typeof(DateTime));
    
    foreach (DataRow row in sourceData.Rows)
    {
        int id = (int)row["ID"];
        decimal calculated = PerformCalculation(row["SomeValue"]);
        targetData.Rows.Add(id, calculated, DateTime.Now);
    }
    
    // Update target table
    using (SqlDataAdapter sqa = new SqlDataAdapter())
    {
        var primaryKeyColumns = new List<string> { "ID" };
        helper.UpdateDataTableDynamic(si, targetData, sqa, "TargetTable", primaryKeyColumns);
    }
}

private decimal PerformCalculation(object value)
{
    // Your calculation logic here
    return Convert.ToDecimal(value) * 1.1m;
}
```

## Example 4: Update with Auto-Timestamps

```csharp
public void UpdateWithTimestamps(SessionInfo si, SqlConnection connection)
{
    var helper = new GBL_SQA_Helper(connection);
    
    DataTable dt = new DataTable();
    dt.Columns.Add("ID", typeof(int));
    dt.Columns.Add("Status", typeof(string));
    dt.Columns.Add("Update_Date", typeof(DateTime));  // Will use GETDATE()
    dt.Columns.Add("Update_User", typeof(string));    // Will use si.UserName
    
    dt.Rows.Add(1, "Active", DBNull.Value, DBNull.Value);  // Date/User filled automatically
    
    using (SqlDataAdapter sqa = new SqlDataAdapter())
    {
        var primaryKeyColumns = new List<string> { "ID" };
        helper.UpdateDataTableDynamic(si, dt, sqa, "MyTable", primaryKeyColumns, 
            autoTimestamps: true);  // Automatically handles Update_Date and Update_User
    }
}
```

## Type Inference Rules

The helper automatically maps DataTable column types to SQL types:

| .NET Type | SQL Type | Notes |
|-----------|----------|-------|
| `string` | `NVarChar` | Uses `MaxLength` property, defaults to 255 if not set |
| `int` | `Int` | |
| `long` | `BigInt` | |
| `short` | `SmallInt` | |
| `byte` | `TinyInt` | |
| `bool` | `Bit` | |
| `DateTime` | `DateTime` | |
| `decimal` | `Decimal` | |
| `double` | `Float` | |
| `float` | `Real` | |
| `Guid` | `UniqueIdentifier` | |
| `byte[]` | `VarBinary` | Uses `MaxLength`, defaults to 8000 |

## Important Notes

1. **Primary Key Columns Are Required**: All columns in the primary key list must be present in the DataTable, or an exception will be thrown.

2. **Column Sizes**: For string columns, set the `MaxLength` property if you need a specific size:
   ```csharp
   DataColumn col = dt.Columns.Add("Name", typeof(string));
   col.MaxLength = 100;  // Will create NVarChar(100)
   ```

3. **Backward Compatibility**: The original `UpdateDataTable` method still works exactly as before. Use `UpdateDataTableDynamic` when you want automatic type inference.

4. **Performance**: No performance difference - both methods generate the same SQL commands.

5. **Validation**: The method validates that all primary key columns are present before attempting any updates.

## Comparison: Traditional vs Dynamic

### Traditional Approach
```csharp
// Must define every column in the table
var columnDefinitions = new List<ColumnDefinition>
{
    new ColumnDefinition("ID", SqlDbType.Int),
    new ColumnDefinition("Name", SqlDbType.NVarChar, 100),
    new ColumnDefinition("Status", SqlDbType.NVarChar, 20),
    // ... 47 more columns ...
};

_helper.UpdateDataTable(si, dt, sqa, "BigTable", columnDefinitions, primaryKeyColumns);
```

### Dynamic Approach
```csharp
// DataTable only has columns you're updating
// Types inferred automatically
_helper.UpdateDataTableDynamic(si, dt, sqa, "BigTable", primaryKeyColumns);
```

**Result**: Same SQL generated, 90% less code to write.
