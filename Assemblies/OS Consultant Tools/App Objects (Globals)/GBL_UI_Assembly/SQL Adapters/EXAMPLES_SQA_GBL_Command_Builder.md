# Using SQA_GBL_Command_Builder Across Assemblies

This document provides concrete examples of how to use the `SQA_GBL_Command_Builder` SQL Adapter from different assemblies in the OS Consultant Tools folder.

## Overview

The `SQA_GBL_Command_Builder` is part of the `GBL_UI_Assembly` (Global Assembly) and can be instantiated from any other assembly in the OS Consultant Tools folder. This enables you to:

1. Pass DataTables directly to update methods
2. Automatically generate INSERT/UPDATE/DELETE SQL commands
3. Reduce boilerplate code in your SQL Adapters
4. Maintain consistency across all assemblies

## Example 1: Using from Finance Model Manager (FMM)

### File: `FMM_UI_Assembly/SQL Adapters/SQA_FMM_Act_Config.cs`

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
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
            // Instantiate the GBL SQL Adapter from FMM assembly
            var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
            
            // Define configuration
            string[] primaryKeys = new[] { "Act_ID" };
            string[] excludeFromUpdate = new[] { "Act_ID", "Cube_ID", "Create_Date", "Create_User" };
            
            // Single method call to update the table
            gblBuilder.UpdateTable(si, "FMM_Act_Config", dt, sqa, primaryKeys, excludeFromUpdate);
        }
    }
}
```

## Example 2: Using from Dynamic Dashboard Manager (DDM)

### File: `DDM_Config_UI_Assembly/SQL Adapters/SQA_DDM_Config.cs`

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_DDM_Config
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_DDM_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_DDM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            // Use the simple method for single primary key with standard audit columns
            var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
            gblBuilder.UpdateTableSimple(si, "DDM_Config", dt, sqa, "DDM_Config_ID");
        }
    }
}
```

## Example 3: Using from Model Dimension Manager (MDM)

### File: `MDM_Config_UI_Assembly/SQL Adapters/SQA_MDM_CDC_Config.cs`

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_MDM_CDC_Config
    {
        private readonly SqlConnection _connection;

        public SQA_MDM_CDC_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_MDM_CDC_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
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

        public void Update_MDM_CDC_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            // Another simple single-key example
            var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
            gblBuilder.UpdateTableSimple(si, "MDM_CDC_Config", dt, sqa, "CDC_Config_ID");
        }
    }
}
```

## Example 4: Composite Keys - RegPlan_Details

### File: `FMM_Shared_Assembly/SQL Adapters/SQA_RegPlan_Details.cs`

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_RegPlan_Details
    {
        private readonly SqlConnection _connection;

        public SQA_RegPlan_Details(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_RegPlan_Details_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_RegPlan_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            // Use composite key method for multi-column primary keys
            var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
            gblBuilder.UpdateTableComposite(si, "RegPlan_Details", dt, sqa, 
                "Reg_ID", "Plan_ID", "Detail_Seq");
        }
    }
}
```

## Example 5: Identity Columns

### File: Custom assembly with identity column handling

```csharp
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_Custom_Audit_Log
    {
        private readonly SqlConnection _connection;

        public SQA_Custom_Audit_Log(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Update_Audit_Log(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            // Handle identity columns by excluding from INSERT
            var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
            
            string[] primaryKeys = new[] { "Log_ID" };
            string[] excludeFromUpdate = new[] { "Log_ID", "Create_Date" };
            string[] excludeFromInsert = new[] { "Log_ID" }; // Identity column
            
            gblBuilder.UpdateTable(si, "Audit_Log", dt, sqa, 
                primaryKeys, excludeFromUpdate, excludeFromInsert);
        }
    }
}
```

## How to Call from Dashboard or Business Rule

Here's how you would use these SQL Adapters from a Dashboard Extender or Business Rule:

```csharp
public void ProcessData(SessionInfo si)
{
    using (var dbInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
    {
        using (var connection = new SqlConnection(dbInfo.ConnectionString))
        {
            connection.Open();
            
            using (var sqa = new SqlDataAdapter())
            {
                var dt = new DataTable();
                
                // Create your SQL Adapter instance
                var sqlAdapter = new SQA_FMM_Act_Config(si, connection);
                
                // Fill the DataTable with existing data
                sqlAdapter.Fill_FMM_Act_Config_DT(si, sqa, dt, 
                    "SELECT * FROM FMM_Act_Config WHERE Cube_ID = @CubeID",
                    new SqlParameter("@CubeID", cubeId));
                
                // Modify data in the DataTable
                foreach (DataRow row in dt.Rows)
                {
                    row["Act_Name"] = "Updated Name";
                    row["Modify_Date"] = DateTime.Now;
                }
                
                // Add new rows
                DataRow newRow = dt.NewRow();
                newRow["Act_Name"] = "New Activity";
                newRow["Cube_ID"] = cubeId;
                newRow["Create_Date"] = DateTime.Now;
                newRow["Create_User"] = si.UserName;
                dt.Rows.Add(newRow);
                
                // Update using the GBL Command Builder (single method call!)
                sqlAdapter.Update_FMM_Act_Config(si, dt, sqa);
            }
        }
    }
}
```

## Key Benefits

### Before (Manual Command Building)
```csharp
public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    using (SqlTransaction transaction = _connection.BeginTransaction())
    {
        try
        {
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
**Lines of code:** 19 lines

### After (Using SQL Adapter)
```csharp
public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
{
    var gblBuilder = new SQA_GBL_Command_Builder(si, _connection);
    string[] primaryKeys = new[] { "Act_ID" };
    string[] excludeFromUpdate = new[] { "Act_ID", "Cube_ID", "Create_Date", "Create_User" };
    gblBuilder.UpdateTable(si, "FMM_Act_Config", dt, sqa, primaryKeys, excludeFromUpdate);
}
```
**Lines of code:** 5 lines

### Result
- **73% reduction** in code lines
- **Automatic transaction management**
- **Consistent error handling**
- **Automatic command cleanup**

## Common Patterns Summary

| Pattern | Method to Use | Example |
|---------|--------------|---------|
| Single PK + Standard Audit | `UpdateTableSimple()` | `gblBuilder.UpdateTableSimple(si, "Table", dt, sqa, "ID")` |
| Composite PK + Standard Audit | `UpdateTableComposite()` | `gblBuilder.UpdateTableComposite(si, "Table", dt, sqa, "ID1", "ID2")` |
| Custom Exclusions | `UpdateTable()` | `gblBuilder.UpdateTable(si, "Table", dt, sqa, pk[], exclude[])` |
| Identity Columns | `UpdateTable()` with excludeFromInsert | `gblBuilder.UpdateTable(si, "Table", dt, sqa, pk[], update[], insert[])` |

## Testing Your Implementation

1. **Create a simple test method:**
```csharp
public void Test_GBL_Builder(SessionInfo si)
{
    using (var dbInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
    {
        using (var connection = new SqlConnection(dbInfo.ConnectionString))
        {
            connection.Open();
            var gblBuilder = new SQA_GBL_Command_Builder(si, connection);
            // Test your update method here
        }
    }
}
```

2. **Run from OneStream** using a dashboard or business rule
3. **Verify data** in the database tables
4. **Check the System Log** for any errors

## Troubleshooting

### Cannot find SQA_GBL_Command_Builder
**Solution:** Ensure you've imported/exported the GBL_UI_Assembly with the Code Utility extension. The class is in the same namespace, so it should be accessible from all assemblies.

### Transaction timeout
**Solution:** The SQL Adapter manages transactions automatically with a reasonable timeout. If you need longer operations, consider batching your updates.

### Primary key errors
**Solution:** Ensure your DataTable has the primary key columns defined and matches the database schema.

## See Also

- [SQA_GBL_Command_Builder README](README_SQA_GBL_Command_Builder.md) - Full documentation
- [GBL_SQL_Command_Builder README](../GBL%20Support%20Classes/README_GBL_SQL_Command_Builder.md) - Underlying class details

---
**Version**: 1.0  
**Last Updated**: 2026-01-13  
**Author**: OneStream Development Team
