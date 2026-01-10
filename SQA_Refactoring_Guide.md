# SQA Refactoring Completion Guide

## Quick Reference for Remaining Files

This guide provides templates and instructions for completing the refactoring of remaining SQA files.

## Template for Simple Single-Key Tables

For tables with a single primary key (most common case):

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_{TableName}
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_{TableName}(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_{TableName}_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_{TableName}(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                // Add all columns from the INSERT statement in the original file
                // Format: new ColumnDefinition("ColumnName", SqlDbType.Type, size)
                // Size is optional for types like Int, Bit, DateTime
                // Size is required for NVarChar, VarChar, etc.
            };

            var primaryKeyColumns = new List<string> { "Primary_Key_Column_Name" };

            _helper.UpdateDataTable(si, dt, sqa, "TableName", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
        }
    }
}
```

## Step-by-Step Refactoring Process

### 1. Identify the Original File Structure

Look at the original file and note:
- Table name (from INSERT statement)
- All columns and their types
- Primary key column(s) from WHERE clause in UPDATE/DELETE
- Whether it uses GETDATE() or si.UserName directly

### 2. Create Column Definitions

From the original INSERT command, extract all columns:

```sql
INSERT INTO FMM_Appr_Config (
    Cube_ID, Appr_ID, Name, Type, Item, Item_Level,
    Status, Create_Date, Create_User, Update_Date, Update_User)
VALUES
    (@Cube_ID, @Appr_ID, @Name, @Type, @Item, @Item_Level,
    @Status, @Create_Date, @Create_User, @Update_Date, @Update_User)
```

And from the parameter definitions:
```csharp
sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
sqa.InsertCommand.Parameters.Add("@Appr_ID", SqlDbType.Int).SourceColumn = "Appr_ID";
sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
// etc...
```

Convert to:
```csharp
var columnDefinitions = new List<ColumnDefinition>
{
    new ColumnDefinition("Cube_ID", SqlDbType.Int),
    new ColumnDefinition("Appr_ID", SqlDbType.Int),
    new ColumnDefinition("Name", SqlDbType.NVarChar, 100),
    // etc...
};
```

### 3. Identify Primary Key(s)

From the DELETE command:
```sql
DELETE FROM FMM_Appr_Config WHERE Appr_ID = @Appr_ID
```

Extract primary key:
```csharp
var primaryKeyColumns = new List<string> { "Appr_ID" };
```

For composite keys (multiple columns in WHERE clause):
```sql
DELETE FROM RegPlan_Details 
WHERE RegPlan_ID = @RegPlan_ID 
  AND Year = @Year 
  AND Plan_Units = @Plan_Units 
  AND Account = @Account
```

Extract as:
```csharp
var primaryKeyColumns = new List<string> { "RegPlan_ID", "Year", "Plan_Units", "Account" };
```

### 4. Determine autoTimestamps Setting

Check if the original file uses:
- `GETDATE()` in INSERT/UPDATE for Create_Date or Update_Date
- `si.UserName` directly for Create_User or Update_User

If YES, use `autoTimestamps: true`
If NO (uses DataTable columns), use `autoTimestamps: false`

## Remaining Files to Refactor

### FMM_UI_Assembly (11 files)

1. **SQA_FMM_Acct_Config.cs**
   - Table: FMM_Acct_Config
   - Primary Key: Acct_ID
   - Columns: ~14
   
2. **SQA_FMM_Act_Appr_Step_Config.cs**
   - Table: FMM_Act_Appr_Step_Config
   - Primary Key: Check WHERE clause
   - Columns: ~10-15

3. **SQA_FMM_Appr_Config.cs**
   - Table: FMM_Appr_Config
   - Primary Key: Appr_ID
   - Columns: ~11

4. **SQA_FMM_Appr_Step_Config.cs**
   - Table: FMM_Appr_Step_Config
   - Primary Key: Check WHERE clause
   - Columns: ~10-15

5. **SQA_FMM_Calc_Config.cs**
   - Table: FMM_Calc_Config
   - Primary Key: Calc_ID
   - Columns: Many (15-20+)

6. **SQA_FMM_Calc_Unit_Assign.cs**
   - Table: FMM_Calc_Unit_Assign
   - Primary Key: Check WHERE clause (possibly composite)
   - Columns: ~8-10

7. **SQA_FMM_Calc_Unit_Config.cs**
   - Table: FMM_Calc_Unit_Config
   - Primary Key: Calc_Unit_ID
   - Columns: ~8-10

8. **SQA_FMM_Col_Config.cs**
   - Table: FMM_Col_Config
   - Primary Key: Col_ID
   - Columns: ~12-15

9. **SQA_FMM_Model_Grp_Assign.cs**
   - Table: FMM_Model_Grp_Assign
   - Primary Key: Check WHERE clause (possibly composite)
   - Columns: ~8

10. **SQA_FMM_Model_Grp_Seqs.cs**
    - Table: FMM_Model_Grp_Seqs
    - Primary Key: Seq_ID or composite
    - Columns: ~8

11. **SQA_FMM_Model_Grps.cs**
    - Table: FMM_Model_Grps
    - Primary Key: Model_Grp_ID
    - Columns: ~8-10

12. **SQA_FMM_Reg_Config.cs**
    - Table: FMM_Reg_Config
    - Primary Key: Reg_ID
    - Columns: ~15-20

13. **SQA_FMM_Unit_Config.cs**
    - Table: FMM_Unit_Config
    - Primary Key: Unit_ID
    - Columns: ~9

### FMM_Shared_Assembly (1 file)

14. **SQA_RegPlan_Audit.cs**
    - Table: RegPlan_Audit
    - Primary Key: Composite (multiple columns)
    - Columns: Many (50+)
    - **Note**: This file has some issues in the original (typos, incorrect parameter names) that should be fixed during refactoring

### DDM_Config_UI_Assembly (estimated 3-5 files)

Files in `Assemblies/OS Consultant Tools/Dynamic Dashboard Manager/DDM_Config_UI_Assembly/SQL Adapters/`:
- SQA_DDM_Config.cs
- SQA_DDM_Config_Hdr_Ctrls.cs
- SQA_DDM_Config_Menu.cs
- SQA_DDM_Config_Menu_Hdr.cs
- SQA_DDM_Config_Menu_Layout.cs

### Other Assemblies

Check for additional SQA files in:
- MDM (Model Dimension Manager)
- SQM (SQL Query Manager)
- GBL assemblies
- Custom assemblies (CMD SPLN, CMD UFR, CMD PGM, etc.)

## Testing Strategy

### 1. Syntax Validation
After refactoring each file:
- Verify it compiles (if possible with omnisharp/language server)
- Check for any missing using statements
- Ensure GBL_SQA_Helper is accessible from the namespace

### 2. Manual Verification
For each refactored file, verify:
- Column count matches original
- Column types match original
- Column sizes match original (for NVarChar, etc.)
- Primary key columns match the WHERE clause in original DELETE command
- autoTimestamps setting is correct

### 3. Database Testing (when possible)
- Create test data
- Execute Fill operation
- Modify data in DataTable
- Execute Update operation
- Verify INSERT works for new rows
- Verify UPDATE works for modified rows
- Verify DELETE works for deleted rows

## Common Pitfalls

1. **Missing Size Parameter**: NVarChar, VarChar, Char types NEED a size parameter
   ```csharp
   // WRONG
   new ColumnDefinition("Name", SqlDbType.NVarChar)
   
   // CORRECT
   new ColumnDefinition("Name", SqlDbType.NVarChar, 100)
   ```

2. **Wrong Primary Key**: Must match the WHERE clause in DELETE, not just the first column
   ```sql
   -- If DELETE says:
   DELETE FROM Table WHERE Col2 = @Col2
   
   -- Then use:
   var primaryKeyColumns = new List<string> { "Col2" };
   ```

3. **Composite Key Order**: Order doesn't technically matter for SQL WHERE clause, but keep it consistent with original

4. **AutoTimestamps**: Check carefully if original uses GETDATE() or DataTable values
   - If INSERT has `GETDATE()` → use `autoTimestamps: true`
   - If INSERT has `@Create_Date` → use `autoTimestamps: false`

## Quick Checklist for Each File

- [ ] Extract all column names, types, and sizes
- [ ] Create ColumnDefinition list
- [ ] Identify primary key column(s) from DELETE WHERE clause
- [ ] Determine autoTimestamps setting
- [ ] Replace constructor to add GBL_SQA_Helper
- [ ] Replace Fill method to use helper
- [ ] Replace Update method with new pattern
- [ ] Verify column count matches original
- [ ] Verify primary keys match original
- [ ] Commit changes

## Estimated Time

- Simple single-key table: 5-10 minutes
- Composite-key table: 10-15 minutes
- Complex table (50+ columns): 15-20 minutes
- Total remaining time estimate: 3-4 hours for all files

## Priority Order

1. FMM_UI_Assembly files (most used, 11 files)
2. SQA_RegPlan_Audit (has issues to fix)
3. DDM files (3-5 files)
4. Other assemblies (as needed)
