using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Helper class for managing FMM custom table configurations.
    /// Provides methods to create, read, update and generate DDL for custom tables.
    /// </summary>
    public class FMM_Table_Config_Helper
    {
        private SessionInfo si;

        public FMM_Table_Config_Helper(SessionInfo si)
        {
            this.si = si;
        }

        #region "Table Configuration Management"

        /// <summary>
        /// Retrieves all table configurations for a specific process type.
        /// </summary>
        public DataTable GetTableConfigurations(string processType)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Table_Config_ID,
                        Process_Type,
                        Table_Name,
                        Table_Type,
                        Parent_Table_Config_ID,
                        Description,
                        Is_Active,
                        Enable_Audit,
                        Audit_Table_Config_ID
                    FROM FMM_Table_Config
                    WHERE Process_Type = @ProcessType
                    AND Is_Active = 1
                    ORDER BY 
                        CASE Table_Type
                            WHEN 'Master' THEN 1
                            WHEN 'Detail' THEN 2
                            WHEN 'Extension' THEN 3
                            WHEN 'Audit' THEN 4
                        END,
                        Table_Name";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ProcessType", SqlDbType.NVarChar, 100) { Value = processType }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves a single table configuration by ID.
        /// </summary>
        public DataRow GetTableConfiguration(int tableConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Table_Config_ID,
                        Process_Type,
                        Table_Name,
                        Table_Type,
                        Parent_Table_Config_ID,
                        Description,
                        Is_Active,
                        Enable_Audit,
                        Audit_Table_Config_ID
                    FROM FMM_Table_Config
                    WHERE Table_Config_ID = @TableConfigId
                    AND Is_Active = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TableConfigId", SqlDbType.Int) { Value = tableConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves column configuration for a specific table.
        /// </summary>
        public DataTable GetTableColumns(int tableConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Column_Config_ID,
                        Table_Config_ID,
                        Column_Name,
                        Data_Type,
                        Max_Length,
                        Precision,
                        Scale,
                        Is_Nullable,
                        Default_Value,
                        Is_Identity,
                        Identity_Seed,
                        Identity_Increment,
                        Column_Order,
                        Description,
                        Is_Active
                    FROM FMM_Table_Column_Config
                    WHERE Table_Config_ID = @TableConfigId
                    AND Is_Active = 1
                    ORDER BY Column_Order";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TableConfigId", SqlDbType.Int) { Value = tableConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves index configuration for a specific table.
        /// </summary>
        public DataTable GetTableIndexes(int tableConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        ic.Index_Config_ID,
                        ic.Table_Config_ID,
                        ic.Index_Name,
                        ic.Index_Type,
                        ic.Is_Clustered,
                        ic.Is_Unique,
                        ic.Fill_Factor,
                        ic.Description,
                        icc.Index_Column_Config_ID,
                        icc.Column_Config_ID,
                        cc.Column_Name,
                        icc.Key_Ordinal,
                        icc.Sort_Direction,
                        icc.Is_Included_Column
                    FROM FMM_Table_Index_Config ic
                    INNER JOIN FMM_Table_Index_Column_Config icc ON ic.Index_Config_ID = icc.Index_Config_ID
                    INNER JOIN FMM_Table_Column_Config cc ON icc.Column_Config_ID = cc.Column_Config_ID
                    WHERE ic.Table_Config_ID = @TableConfigId
                    AND ic.Is_Active = 1
                    ORDER BY ic.Index_Config_ID, icc.Key_Ordinal";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TableConfigId", SqlDbType.Int) { Value = tableConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves foreign key configuration for a specific table.
        /// </summary>
        public DataTable GetTableForeignKeys(int tableConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        fk.FK_Config_ID,
                        fk.FK_Name,
                        fk.Source_Table_Config_ID,
                        st.Table_Name AS Source_Table,
                        fk.Target_Table_Config_ID,
                        tt.Table_Name AS Target_Table,
                        fk.On_Delete_Action,
                        fk.On_Update_Action,
                        fkc.FK_Column_Config_ID,
                        fkc.Source_Column_Config_ID,
                        sc.Column_Name AS Source_Column,
                        fkc.Target_Column_Config_ID,
                        tc.Column_Name AS Target_Column,
                        fkc.Column_Ordinal
                    FROM FMM_Table_FK_Config fk
                    INNER JOIN FMM_Table_Config st ON fk.Source_Table_Config_ID = st.Table_Config_ID
                    INNER JOIN FMM_Table_Config tt ON fk.Target_Table_Config_ID = tt.Table_Config_ID
                    INNER JOIN FMM_Table_FK_Column_Config fkc ON fk.FK_Config_ID = fkc.FK_Config_ID
                    INNER JOIN FMM_Table_Column_Config sc ON fkc.Source_Column_Config_ID = sc.Column_Config_ID
                    INNER JOIN FMM_Table_Column_Config tc ON fkc.Target_Column_Config_ID = tc.Column_Config_ID
                    WHERE fk.Source_Table_Config_ID = @TableConfigId
                    AND fk.Is_Active = 1
                    ORDER BY fk.FK_Config_ID, fkc.Column_Ordinal";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TableConfigId", SqlDbType.Int) { Value = tableConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region "DDL Generation"

        /// <summary>
        /// Generates CREATE TABLE DDL statement from configuration.
        /// </summary>
        public string GenerateCreateTableDDL(int tableConfigId)
        {
            try
            {
                var tableConfig = GetTableConfiguration(tableConfigId);

                if (tableConfig == null)
                {
                    throw new XFException(si, $"Table configuration not found: {tableConfigId}");
                }

                string tableName = tableConfig.Field<string>("Table_Name");
                var columns = GetTableColumns(tableConfigId);
                var indexes = GetTableIndexes(tableConfigId);

                var ddl = new StringBuilder();
                ddl.AppendLine($"-- Table: {tableName}");
                ddl.AppendLine($"CREATE TABLE dbo.[{tableName}] (");

                // Generate column definitions
                var columnDefs = new List<string>();
                foreach (DataRow col in columns.Rows)
                {
                    columnDefs.Add(GenerateColumnDDL(col));
                }

                ddl.AppendLine(string.Join(",\n", columnDefs));
                ddl.AppendLine(");");
                ddl.AppendLine("GO");

                return ddl.ToString();
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Generates column DDL from column configuration.
        /// </summary>
        private string GenerateColumnDDL(DataRow columnConfig)
        {
            var columnName = columnConfig.Field<string>("Column_Name");
            var dataType = columnConfig.Field<string>("Data_Type");
            var isNullable = columnConfig.Field<bool>("Is_Nullable");
            var isIdentity = columnConfig.Field<bool>("Is_Identity");
            var defaultValue = columnConfig.Field<string>("Default_Value");

            var ddl = new StringBuilder();
            ddl.Append($"    [{columnName}] {dataType}");

            // Add size/precision for appropriate types
            // Skip size for TEXT/NTEXT as they don't support size specification
            if ((dataType.XFEqualsIgnoreCase("NVARCHAR") || dataType.XFEqualsIgnoreCase("VARCHAR") || 
                 dataType.XFEqualsIgnoreCase("CHAR") || dataType.XFEqualsIgnoreCase("NCHAR")) &&
                !dataType.XFEqualsIgnoreCase("TEXT") && !dataType.XFEqualsIgnoreCase("NTEXT"))
            {
                var maxLength = columnConfig.Field<int?>("Max_Length");
                ddl.Append($"({(maxLength.HasValue ? maxLength.Value.ToString() : "MAX")})");
            }
            else if (dataType.XFEqualsIgnoreCase("DECIMAL") || dataType.XFEqualsIgnoreCase("NUMERIC"))
            {
                var precision = columnConfig.Field<int?>("Precision") ?? 18;
                var scale = columnConfig.Field<int?>("Scale") ?? 2;
                ddl.Append($"({precision}, {scale})");
            }

            // Identity specification
            if (isIdentity)
            {
                var seed = columnConfig.Field<int?>("Identity_Seed") ?? 1;
                var increment = columnConfig.Field<int?>("Identity_Increment") ?? 1;
                ddl.Append($" IDENTITY({seed}, {increment})");
            }

            // Nullability
            ddl.Append(isNullable ? " NULL" : " NOT NULL");

            // Default value
            if (!string.IsNullOrEmpty(defaultValue))
            {
                ddl.Append($" DEFAULT {defaultValue}");
            }

            return ddl.ToString();
        }

        /// <summary>
        /// Generates CREATE INDEX statements from configuration.
        /// </summary>
        public string GenerateCreateIndexesDDL(int tableConfigId)
        {
            try
            {
                var tableConfig = GetTableConfiguration(tableConfigId);

                if (tableConfig == null)
                {
                    throw new XFException(si, $"Table configuration not found: {tableConfigId}");
                }

                string tableName = tableConfig.Field<string>("Table_Name");
                var indexes = GetTableIndexes(tableConfigId);

                if (indexes.Rows.Count == 0)
                {
                    return string.Empty;
                }

                var ddl = new StringBuilder();
                ddl.AppendLine($"-- Indexes for table: {tableName}");

                // Group by Index_Config_ID
                var indexGroups = indexes.AsEnumerable()
                    .GroupBy(r => r.Field<int>("Index_Config_ID"));

                foreach (var indexGroup in indexGroups)
                {
                    var firstRow = indexGroup.First();
                    var indexName = firstRow.Field<string>("Index_Name");
                    var indexType = firstRow.Field<string>("Index_Type");
                    var isClustered = firstRow.Field<bool>("Is_Clustered");
                    var isUnique = firstRow.Field<bool>("Is_Unique");
                    var fillFactor = firstRow.Field<int?>("Fill_Factor");

                    // Build column list
                    var keyColumns = indexGroup
                        .Where(r => !r.Field<bool>("Is_Included_Column"))
                        .OrderBy(r => r.Field<int>("Key_Ordinal"))
                        .Select(r => $"[{r.Field<string>("Column_Name")}] {r.Field<string>("Sort_Direction")}");

                    var includedColumns = indexGroup
                        .Where(r => r.Field<bool>("Is_Included_Column"))
                        .Select(r => $"[{r.Field<string>("Column_Name")}]");

                    if (indexType.XFEqualsIgnoreCase("PRIMARY_KEY"))
                    {
                        ddl.AppendLine($"ALTER TABLE dbo.[{tableName}]");
                        ddl.Append($"    ADD CONSTRAINT [{indexName}] PRIMARY KEY ");
                        ddl.Append(isClustered ? "CLUSTERED " : "NONCLUSTERED ");
                        ddl.AppendLine($"({string.Join(", ", keyColumns)});");
                    }
                    else
                    {
                        ddl.Append($"CREATE ");
                        if (isUnique) ddl.Append("UNIQUE ");
                        ddl.Append(isClustered ? "CLUSTERED " : "NONCLUSTERED ");
                        ddl.AppendLine($"INDEX [{indexName}]");
                        ddl.AppendLine($"    ON dbo.[{tableName}] ({string.Join(", ", keyColumns)})");
                        
                        if (includedColumns.Any())
                        {
                            ddl.AppendLine($"    INCLUDE ({string.Join(", ", includedColumns)})");
                        }
                        
                        if (fillFactor.HasValue)
                        {
                            ddl.AppendLine($"    WITH (FILLFACTOR = {fillFactor.Value});");
                        }
                        else
                        {
                            ddl.AppendLine(";");
                        }
                    }
                    ddl.AppendLine("GO");
                    ddl.AppendLine();
                }

                return ddl.ToString();
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Generates ALTER TABLE statements for foreign keys.
        /// </summary>
        public string GenerateForeignKeysDDL(int tableConfigId)
        {
            try
            {
                var tableConfig = GetTableConfiguration(tableConfigId);

                if (tableConfig == null)
                {
                    throw new XFException(si, $"Table configuration not found: {tableConfigId}");
                }

                string tableName = tableConfig.Field<string>("Table_Name");
                var foreignKeys = GetTableForeignKeys(tableConfigId);

                if (foreignKeys.Rows.Count == 0)
                {
                    return string.Empty;
                }

                var ddl = new StringBuilder();
                ddl.AppendLine($"-- Foreign keys for table: {tableName}");

                // Group by FK_Config_ID
                var fkGroups = foreignKeys.AsEnumerable()
                    .GroupBy(r => r.Field<int>("FK_Config_ID"));

                foreach (var fkGroup in fkGroups)
                {
                    var firstRow = fkGroup.First();
                    var fkName = firstRow.Field<string>("FK_Name");
                    var sourceTable = firstRow.Field<string>("Source_Table");
                    var targetTable = firstRow.Field<string>("Target_Table");
                    var onDeleteAction = firstRow.Field<string>("On_Delete_Action");
                    var onUpdateAction = firstRow.Field<string>("On_Update_Action");

                    var sourceColumns = fkGroup
                        .OrderBy(r => r.Field<int>("Column_Ordinal"))
                        .Select(r => $"[{r.Field<string>("Source_Column")}]");

                    var targetColumns = fkGroup
                        .OrderBy(r => r.Field<int>("Column_Ordinal"))
                        .Select(r => $"[{r.Field<string>("Target_Column")}]");

                    ddl.AppendLine($"ALTER TABLE dbo.[{sourceTable}]");
                    ddl.AppendLine($"    ADD CONSTRAINT [{fkName}]");
                    ddl.AppendLine($"    FOREIGN KEY ({string.Join(", ", sourceColumns)})");
                    ddl.AppendLine($"    REFERENCES dbo.[{targetTable}] ({string.Join(", ", targetColumns)})");
                    
                    if (!onDeleteAction.XFEqualsIgnoreCase("NO ACTION"))
                    {
                        ddl.AppendLine($"    ON DELETE {onDeleteAction}");
                    }
                    
                    if (!onUpdateAction.XFEqualsIgnoreCase("NO ACTION"))
                    {
                        ddl.AppendLine($"    ON UPDATE {onUpdateAction}");
                    }
                    
                    ddl.AppendLine(";");
                    ddl.AppendLine("GO");
                    ddl.AppendLine();
                }

                return ddl.ToString();
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Generates complete DDL for all tables in a process.
        /// </summary>
        public string GenerateCompleteDDL(string processType)
        {
            try
            {
                var tables = GetTableConfigurations(processType);
                var ddl = new StringBuilder();

                ddl.AppendLine("-- =============================================");
                ddl.AppendLine($"-- DDL for Process: {processType}");
                ddl.AppendLine($"-- Generated: {DateTime.Now}");
                ddl.AppendLine("-- =============================================");
                ddl.AppendLine();

                // Generate CREATE TABLE statements
                foreach (DataRow table in tables.Rows)
                {
                    int tableConfigId = table.Field<int>("Table_Config_ID");
                    string tableType = table.Field<string>("Table_Type");
                    
                    // Skip audit tables for now
                    if (tableType.XFEqualsIgnoreCase("Audit"))
                    {
                        continue;
                    }

                    ddl.AppendLine(GenerateCreateTableDDL(tableConfigId));
                    ddl.AppendLine();
                }

                // Generate indexes
                foreach (DataRow table in tables.Rows)
                {
                    int tableConfigId = table.Field<int>("Table_Config_ID");
                    string tableType = table.Field<string>("Table_Type");
                    
                    if (tableType.XFEqualsIgnoreCase("Audit"))
                    {
                        continue;
                    }

                    var indexDDL = GenerateCreateIndexesDDL(tableConfigId);
                    if (!string.IsNullOrEmpty(indexDDL))
                    {
                        ddl.AppendLine(indexDDL);
                        ddl.AppendLine();
                    }
                }

                // Generate foreign keys
                foreach (DataRow table in tables.Rows)
                {
                    int tableConfigId = table.Field<int>("Table_Config_ID");
                    string tableType = table.Field<string>("Table_Type");
                    
                    if (tableType.XFEqualsIgnoreCase("Audit"))
                    {
                        continue;
                    }

                    var fkDDL = GenerateForeignKeysDDL(tableConfigId);
                    if (!string.IsNullOrEmpty(fkDDL))
                    {
                        ddl.AppendLine(fkDDL);
                        ddl.AppendLine();
                    }
                }

                return ddl.ToString();
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region "Validation"

        /// <summary>
        /// Validates table configuration integrity.
        /// </summary>
        public List<string> ValidateTableConfiguration(int tableConfigId)
        {
            var errors = new List<string>();

            try
            {
                // Get table configuration using efficient lookup
                var tableConfig = GetTableConfiguration(tableConfigId);

                if (tableConfig == null)
                {
                    errors.Add($"Table configuration not found: {tableConfigId}");
                    return errors;
                }

                string tableName = tableConfig.Field<string>("Table_Name");

                // Validate columns exist
                var columns = GetTableColumns(tableConfigId);
                if (columns.Rows.Count == 0)
                {
                    errors.Add($"Table {tableName} has no columns defined");
                }

                // Validate at least one primary key or clustered index
                var indexes = GetTableIndexes(tableConfigId);
                var hasPK = indexes.AsEnumerable()
                    .Any(r => r.Field<string>("Index_Type").XFEqualsIgnoreCase("PRIMARY_KEY"));
                
                var hasClustered = indexes.AsEnumerable()
                    .Any(r => r.Field<bool>("Is_Clustered"));

                if (!hasPK)
                {
                    errors.Add($"Table {tableName} has no primary key defined");
                }

                if (!hasClustered)
                {
                    errors.Add($"Table {tableName} has no clustered index defined");
                }

                // Validate only one clustered index
                var clusteredCount = indexes.AsEnumerable()
                    .Select(r => r.Field<int>("Index_Config_ID"))
                    .Distinct()
                    .Count(indexId => indexes.AsEnumerable()
                        .Where(r => r.Field<int>("Index_Config_ID") == indexId)
                        .First().Field<bool>("Is_Clustered"));

                if (clusteredCount > 1)
                {
                    errors.Add($"Table {tableName} has multiple clustered indexes defined (only one allowed)");
                }

                // Validate foreign key references exist - using efficient single lookups
                var foreignKeys = GetTableForeignKeys(tableConfigId);
                foreach (var fkGroup in foreignKeys.AsEnumerable().GroupBy(r => r.Field<int>("FK_Config_ID")))
                {
                    var targetTableId = fkGroup.First().Field<int>("Target_Table_Config_ID");
                    var targetTable = GetTableConfiguration(targetTableId);

                    if (targetTable == null)
                    {
                        errors.Add($"Foreign key references non-existent table config ID: {targetTableId}");
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                return errors;
            }
        }

        #endregion
    }
}
