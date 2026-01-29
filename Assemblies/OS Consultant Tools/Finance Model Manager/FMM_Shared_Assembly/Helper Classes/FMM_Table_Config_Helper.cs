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
                        TableConfigID,
                        ProcessType,
                        TableName,
                        TableType,
                        ParentTableConfigID,
                        Description,
                        IsActive,
                        EnableAudit,
                        AuditTableConfigID
                    FROM FMM_Table_Config
                    WHERE ProcessType = @ProcessType
                    AND IsActive = 1
                    ORDER BY 
                        CASE TableType
                            WHEN 'Master' THEN 1
                            WHEN 'Detail' THEN 2
                            WHEN 'Extension' THEN 3
                            WHEN 'Audit' THEN 4
                        END,
                        TableName";

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
                        TableConfigID,
                        ProcessType,
                        TableName,
                        TableType,
                        ParentTableConfigID,
                        Description,
                        IsActive,
                        EnableAudit,
                        AuditTableConfigID
                    FROM FMM_Table_Config
                    WHERE TableConfigID = @TableConfigId
                    AND IsActive = 1";

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
                        TableConfigID,
                        ColumnName,
                        DataType,
                        MaxLength,
                        Precision,
                        Scale,
                        IsNullable,
                        DefaultValue,
                        IsIdentity,
                        IdentitySeed,
                        IdentityIncrement,
                        ColumnOrder,
                        Description,
                        IsActive
                    FROM FMM_Table_Column_Config
                    WHERE TableConfigID = @TableConfigId
                    AND IsActive = 1
                    ORDER BY ColumnOrder";

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
                        ic.TableConfigID,
                        ic.IndexName,
                        ic.IndexType,
                        ic.IsClustered,
                        ic.IsUnique,
                        ic.FillFactor,
                        ic.Description,
                        icc.Index_Column_Config_ID,
                        icc.Column_Config_ID,
                        cc.ColumnName,
                        icc.KeyOrdinal,
                        icc.SortDirection,
                        icc.Is_Included_Column
                    FROM FMM_Table_Index_Config ic
                    INNER JOIN FMM_Table_Index_Column_Config icc ON ic.Index_Config_ID = icc.Index_Config_ID
                    INNER JOIN FMM_Table_Column_Config cc ON icc.Column_Config_ID = cc.Column_Config_ID
                    WHERE ic.TableConfigID = @TableConfigId
                    AND ic.IsActive = 1
                    ORDER BY ic.Index_Config_ID, icc.KeyOrdinal";

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
                        st.TableName AS SourceTable,
                        fk.Target_Table_Config_ID,
                        tt.TableName AS TargetTable,
                        fk.On_Delete_Action,
                        fk.On_Update_Action,
                        fkc.FK_Column_Config_ID,
                        fkc.Source_Column_Config_ID,
                        sc.ColumnName AS SourceColumn,
                        fkc.Target_Column_Config_ID,
                        tc.ColumnName AS TargetColumn,
                        fkc.ColumnOrdinal
                    FROM FMM_Table_FK_Config fk
                    INNER JOIN FMM_Table_Config st ON fk.Source_Table_Config_ID = st.TableConfigID
                    INNER JOIN FMM_Table_Config tt ON fk.Target_Table_Config_ID = tt.TableConfigID
                    INNER JOIN FMM_Table_FK_Column_Config fkc ON fk.FK_Config_ID = fkc.FK_Config_ID
                    INNER JOIN FMM_Table_Column_Config sc ON fkc.Source_Column_Config_ID = sc.Column_Config_ID
                    INNER JOIN FMM_Table_Column_Config tc ON fkc.Target_Column_Config_ID = tc.Column_Config_ID
                    WHERE fk.Source_Table_Config_ID = @TableConfigId
                    AND fk.IsActive = 1
                    ORDER BY fk.FK_Config_ID, fkc.ColumnOrdinal";

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

                string tableName = tableConfig.Field<string>("TableName");
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
            var columnName = columnConfig.Field<string>("ColumnName");
            var dataType = columnConfig.Field<string>("DataType");
            var isNullable = columnConfig.Field<bool>("IsNullable");
            var isIdentity = columnConfig.Field<bool>("IsIdentity");
            var defaultValue = columnConfig.Field<string>("DefaultValue");

            var ddl = new StringBuilder();
            ddl.Append($"    [{columnName}] {dataType}");

            // Add size/precision for appropriate types
            // Skip size for TEXT/NTEXT as they don't support size specification
            if ((dataType.XFEqualsIgnoreCase("NVARCHAR") || dataType.XFEqualsIgnoreCase("VARCHAR") || 
                 dataType.XFEqualsIgnoreCase("CHAR") || dataType.XFEqualsIgnoreCase("NCHAR")) &&
                !dataType.XFEqualsIgnoreCase("TEXT") && !dataType.XFEqualsIgnoreCase("NTEXT"))
            {
                var maxLength = columnConfig.Field<int?>("MaxLength");
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
                var seed = columnConfig.Field<int?>("IdentitySeed") ?? 1;
                var increment = columnConfig.Field<int?>("IdentityIncrement") ?? 1;
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

                string tableName = tableConfig.Field<string>("TableName");
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
                    var indexName = firstRow.Field<string>("IndexName");
                    var indexType = firstRow.Field<string>("IndexType");
                    var isClustered = firstRow.Field<bool>("IsClustered");
                    var isUnique = firstRow.Field<bool>("IsUnique");
                    var fillFactor = firstRow.Field<int?>("FillFactor");

                    // Build column list
                    var keyColumns = indexGroup
                        .Where(r => !r.Field<bool>("Is_Included_Column"))
                        .OrderBy(r => r.Field<int>("KeyOrdinal"))
                        .Select(r => $"[{r.Field<string>("ColumnName")}] {r.Field<string>("SortDirection")}");

                    var includedColumns = indexGroup
                        .Where(r => r.Field<bool>("Is_Included_Column"))
                        .Select(r => $"[{r.Field<string>("ColumnName")}]");

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

                string tableName = tableConfig.Field<string>("TableName");
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
                    var sourceTable = firstRow.Field<string>("SourceTable");
                    var targetTable = firstRow.Field<string>("TargetTable");
                    var onDeleteAction = firstRow.Field<string>("On_Delete_Action");
                    var onUpdateAction = firstRow.Field<string>("On_Update_Action");

                    var sourceColumns = fkGroup
                        .OrderBy(r => r.Field<int>("ColumnOrdinal"))
                        .Select(r => $"[{r.Field<string>("SourceColumn")}]");

                    var targetColumns = fkGroup
                        .OrderBy(r => r.Field<int>("ColumnOrdinal"))
                        .Select(r => $"[{r.Field<string>("TargetColumn")}]");

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
                    int tableConfigId = table.Field<int>("TableConfigID");
                    string tableType = table.Field<string>("TableType");
                    
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
                    int tableConfigId = table.Field<int>("TableConfigID");
                    string tableType = table.Field<string>("TableType");
                    
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
                    int tableConfigId = table.Field<int>("TableConfigID");
                    string tableType = table.Field<string>("TableType");
                    
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

                string tableName = tableConfig.Field<string>("TableName");

                // Validate columns exist
                var columns = GetTableColumns(tableConfigId);
                if (columns.Rows.Count == 0)
                {
                    errors.Add($"Table {tableName} has no columns defined");
                }

                // Validate at least one primary key or clustered index
                var indexes = GetTableIndexes(tableConfigId);
                var hasPK = indexes.AsEnumerable()
                    .Any(r => r.Field<string>("IndexType").XFEqualsIgnoreCase("PRIMARY_KEY"));
                
                var hasClustered = indexes.AsEnumerable()
                    .Any(r => r.Field<bool>("IsClustered"));

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
                        .First().Field<bool>("IsClustered"));

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
