using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// Consolidated helper class for SQL Data Adapter (SQA) operations.
    /// Provides generalized methods for CRUD operations with support for both single and composite primary keys.
    /// </summary>
    public class GBL_SQA_Helper
    {
        private readonly SqlConnection _connection;

        public GBL_SQA_Helper(SqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Fills a DataTable using a SQL query with optional parameters.
        /// </summary>
        public void FillDataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlParams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Updates a DataTable back to the database with INSERT, UPDATE, and DELETE commands.
        /// Supports both single and composite primary keys.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="dt">DataTable to update</param>
        /// <param name="sqa">SqlDataAdapter to use</param>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="columnDefinitions">List of column definitions (name, type, size)</param>
        /// <param name="primaryKeyColumns">List of primary key column names</param>
        /// <param name="excludeFromUpdate">Optional list of columns to exclude from UPDATE SET clause (e.g., primary keys, Create_Date, Create_User)</param>
        /// <param name="autoTimestamps">Automatically handle Create_Date/Update_Date with GETDATE() and Create_User/Update_User with si.UserName</param>
        public void UpdateDataTable(
            SessionInfo si,
            DataTable dt,
            SqlDataAdapter sqa,
            string tableName,
            List<ColumnDefinition> columnDefinitions,
            List<string> primaryKeyColumns,
            List<string> excludeFromUpdate = null,
            bool autoTimestamps = false)
        {
            sqa.UpdateBatchSize = 0;
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Build INSERT command
                    BuildInsertCommand(sqa, tableName, columnDefinitions, transaction, si, autoTimestamps);

                    // Build UPDATE command
                    BuildUpdateCommand(sqa, tableName, columnDefinitions, primaryKeyColumns, excludeFromUpdate, transaction, si, autoTimestamps);

                    // Build DELETE command
                    BuildDeleteCommand(sqa, tableName, primaryKeyColumns, transaction);

                    // Execute update
                    sqa.Update(dt);
                    transaction.Commit();

                    // Clean up
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

        /// <summary>
        /// Builds a dynamic WHERE clause for UPDATE and DELETE operations.
        /// Supports both single and composite primary keys.
        /// </summary>
        /// <param name="primaryKeyColumns">List of primary key column names</param>
        /// <returns>WHERE clause string (e.g., "WHERE Col1 = @Col1 AND Col2 = @Col2")</returns>
        private string BuildWhereClause(List<string> primaryKeyColumns)
        {
            if (primaryKeyColumns == null || primaryKeyColumns.Count == 0)
            {
                throw new ArgumentException("At least one primary key column must be specified.");
            }

            var whereConditions = primaryKeyColumns.Select(col => $"[{col}] = @{col}").ToList();
            return "WHERE " + string.Join(" AND ", whereConditions);
        }

        /// <summary>
        /// Builds the INSERT command for the SqlDataAdapter.
        /// </summary>
        private void BuildInsertCommand(
            SqlDataAdapter sqa,
            string tableName,
            List<ColumnDefinition> columnDefinitions,
            SqlTransaction transaction,
            SessionInfo si,
            bool autoTimestamps)
        {
            var columns = new List<string>();
            var values = new List<string>();

            foreach (var colDef in columnDefinitions)
            {
                // Skip auto-generated columns if using GETDATE()
                if (autoTimestamps && (colDef.ColumnName == "Create_Date" || colDef.ColumnName == "Update_Date"))
                {
                    columns.Add($"[{colDef.ColumnName}]");
                    values.Add("GETDATE()");
                }
                else if (autoTimestamps && (colDef.ColumnName == "Create_User" || colDef.ColumnName == "Update_User"))
                {
                    columns.Add($"[{colDef.ColumnName}]");
                    values.Add($"@{colDef.ColumnName}");
                }
                else
                {
                    columns.Add($"[{colDef.ColumnName}]");
                    values.Add($"@{colDef.ColumnName}");
                }
            }

            string insertQuery = $@"INSERT INTO [{tableName}] ({string.Join(", ", columns)}) 
                                   VALUES ({string.Join(", ", values)})";

            sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

            // Add parameters
            foreach (var colDef in columnDefinitions)
            {
                if (autoTimestamps && (colDef.ColumnName == "Create_Date" || colDef.ColumnName == "Update_Date"))
                {
                    // Skip - using GETDATE() in query
                    continue;
                }
                else if (autoTimestamps && (colDef.ColumnName == "Create_User" || colDef.ColumnName == "Update_User"))
                {
                    sqa.InsertCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size).Value = si.UserName;
                }
                else
                {
                    if (colDef.Size > 0)
                    {
                        sqa.InsertCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size).SourceColumn = colDef.ColumnName;
                    }
                    else
                    {
                        sqa.InsertCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType).SourceColumn = colDef.ColumnName;
                    }
                }
            }
        }

        /// <summary>
        /// Builds the UPDATE command for the SqlDataAdapter.
        /// </summary>
        private void BuildUpdateCommand(
            SqlDataAdapter sqa,
            string tableName,
            List<ColumnDefinition> columnDefinitions,
            List<string> primaryKeyColumns,
            List<string> excludeFromUpdate,
            SqlTransaction transaction,
            SessionInfo si,
            bool autoTimestamps)
        {
            var setStatements = new List<string>();
            var excludeList = new HashSet<string>(excludeFromUpdate ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

            // Add primary keys to exclude list (they shouldn't be in SET clause)
            foreach (var pk in primaryKeyColumns)
            {
                excludeList.Add(pk);
            }

            // Add Create_Date and Create_User to exclude list
            if (autoTimestamps)
            {
                excludeList.Add("Create_Date");
                excludeList.Add("Create_User");
            }

            foreach (var colDef in columnDefinitions)
            {
                if (excludeList.Contains(colDef.ColumnName))
                {
                    continue;
                }

                if (autoTimestamps && colDef.ColumnName == "Update_Date")
                {
                    setStatements.Add($"[{colDef.ColumnName}] = GETDATE()");
                }
                else
                {
                    setStatements.Add($"[{colDef.ColumnName}] = @{colDef.ColumnName}");
                }
            }

            string whereClause = BuildWhereClause(primaryKeyColumns);
            string updateQuery = $@"UPDATE [{tableName}] 
                                   SET {string.Join(", ", setStatements)} 
                                   {whereClause}";

            sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

            // Add parameters for SET clause
            foreach (var colDef in columnDefinitions)
            {
                if (excludeList.Contains(colDef.ColumnName))
                {
                    continue;
                }

                if (autoTimestamps && colDef.ColumnName == "Update_Date")
                {
                    // Skip - using GETDATE() in query
                    continue;
                }
                else if (autoTimestamps && colDef.ColumnName == "Update_User")
                {
                    sqa.UpdateCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size).Value = si.UserName;
                }
                else
                {
                    if (colDef.Size > 0)
                    {
                        sqa.UpdateCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size).SourceColumn = colDef.ColumnName;
                    }
                    else
                    {
                        sqa.UpdateCommand.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType).SourceColumn = colDef.ColumnName;
                    }
                }
            }

            // Add parameters for WHERE clause (using Original version for concurrency)
            foreach (var pkCol in primaryKeyColumns)
            {
                var colDef = columnDefinitions.FirstOrDefault(c => c.ColumnName.Equals(pkCol, StringComparison.OrdinalIgnoreCase));
                if (colDef != null)
                {
                    if (colDef.Size > 0)
                    {
                        sqa.UpdateCommand.Parameters.Add(new SqlParameter($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size)
                        {
                            SourceColumn = colDef.ColumnName,
                            SourceVersion = DataRowVersion.Original
                        });
                    }
                    else
                    {
                        sqa.UpdateCommand.Parameters.Add(new SqlParameter($"@{colDef.ColumnName}", colDef.SqlDbType)
                        {
                            SourceColumn = colDef.ColumnName,
                            SourceVersion = DataRowVersion.Original
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Builds the DELETE command for the SqlDataAdapter.
        /// </summary>
        private void BuildDeleteCommand(
            SqlDataAdapter sqa,
            string tableName,
            List<string> primaryKeyColumns,
            SqlTransaction transaction)
        {
            string whereClause = BuildWhereClause(primaryKeyColumns);
            string deleteQuery = $"DELETE FROM [{tableName}] {whereClause}";

            sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);

            // Add parameters for WHERE clause (using Original version)
            foreach (var pkCol in primaryKeyColumns)
            {
                sqa.DeleteCommand.Parameters.Add(new SqlParameter($"@{pkCol}", SqlDbType.NVarChar)
                {
                    SourceColumn = pkCol,
                    SourceVersion = DataRowVersion.Original
                });
            }
        }

        /// <summary>
        /// Performs a MERGE operation (conditional upsert with optional delete).
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="sourceTable">Source DataTable</param>
        /// <param name="targetTableName">Target database table name</param>
        /// <param name="columnDefinitions">Column definitions</param>
        /// <param name="primaryKeyColumns">Primary key columns for matching</param>
        /// <param name="conditionalDeleteWhere">Optional WHERE clause for conditional delete (e.g., "Status = 'Inactive'")</param>
        public void MergeDataTable(
            SessionInfo si,
            DataTable sourceTable,
            string targetTableName,
            List<ColumnDefinition> columnDefinitions,
            List<string> primaryKeyColumns,
            string conditionalDeleteWhere = null)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Build MERGE statement
                    var mergeColumns = columnDefinitions.Select(c => c.ColumnName).ToList();
                    var matchConditions = primaryKeyColumns.Select(pk => $"target.[{pk}] = source.[{pk}]").ToList();
                    var updateSet = columnDefinitions
                        .Where(c => !primaryKeyColumns.Contains(c.ColumnName, StringComparer.OrdinalIgnoreCase))
                        .Select(c => $"target.[{c.ColumnName}] = source.[{c.ColumnName}]")
                        .ToList();
                    var insertColumns = string.Join(", ", mergeColumns.Select(c => $"[{c}]"));
                    var insertValues = string.Join(", ", mergeColumns.Select(c => $"source.[{c}]"));

                    string mergeSql = $@"
                        MERGE [{targetTableName}] AS target
                        USING (SELECT {string.Join(", ", mergeColumns.Select(c => $"@{c} AS [{c}]"))}) AS source
                        ON ({string.Join(" AND ", matchConditions)})
                        WHEN MATCHED THEN
                            UPDATE SET {string.Join(", ", updateSet)}
                        WHEN NOT MATCHED THEN
                            INSERT ({insertColumns}) VALUES ({insertValues})";

                    if (!string.IsNullOrEmpty(conditionalDeleteWhere))
                    {
                        mergeSql += $@"
                        WHEN NOT MATCHED BY SOURCE AND {conditionalDeleteWhere} THEN
                            DELETE";
                    }

                    mergeSql += ";";

                    // Execute for each row
                    foreach (DataRow row in sourceTable.Rows)
                    {
                        using (SqlCommand cmd = new SqlCommand(mergeSql, _connection, transaction))
                        {
                            foreach (var colDef in columnDefinitions)
                            {
                                var value = row[colDef.ColumnName];
                                cmd.Parameters.Add($"@{colDef.ColumnName}", colDef.SqlDbType, colDef.Size).Value = value ?? DBNull.Value;
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        /// <summary>
        /// Performs a SYNCHRONIZE operation - updates target table to match source, optionally deleting unmatched rows.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="sourceTable">Source DataTable</param>
        /// <param name="targetTableName">Target database table name</param>
        /// <param name="columnDefinitions">Column definitions</param>
        /// <param name="primaryKeyColumns">Primary key columns</param>
        /// <param name="syncDeleteCondition">Optional condition for deleting unmatched rows (e.g., "Parent_ID = 123")</param>
        public void SynchronizeDataTable(
            SessionInfo si,
            DataTable sourceTable,
            string targetTableName,
            List<ColumnDefinition> columnDefinitions,
            List<string> primaryKeyColumns,
            string syncDeleteCondition = null)
        {
            // First, perform merge
            MergeDataTable(si, sourceTable, targetTableName, columnDefinitions, primaryKeyColumns, syncDeleteCondition);
        }

        /// <summary>
        /// Updates a DataTable back to the database by inferring column definitions from the DataTable schema.
        /// This allows updating only specific columns without defining all table columns.
        /// Supports both single and composite primary keys.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="dt">DataTable to update (only columns present in DataTable will be updated)</param>
        /// <param name="sqa">SqlDataAdapter to use</param>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="primaryKeyColumns">List of primary key column names (must be present in DataTable)</param>
        /// <param name="excludeFromUpdate">Optional list of columns to exclude from UPDATE SET clause</param>
        /// <param name="autoTimestamps">Automatically handle Create_Date/Update_Date with GETDATE() and Create_User/Update_User with si.UserName</param>
        public void UpdateDataTableDynamic(
            SessionInfo si,
            DataTable dt,
            SqlDataAdapter sqa,
            string tableName,
            List<string> primaryKeyColumns,
            List<string> excludeFromUpdate = null,
            bool autoTimestamps = false)
        {
            // Validate that all primary key columns are present in the DataTable
            foreach (var pkCol in primaryKeyColumns)
            {
                if (!dt.Columns.Contains(pkCol))
                {
                    throw new ArgumentException($"Primary key column '{pkCol}' is not present in the DataTable. All primary key columns must be included.");
                }
            }

            // Infer column definitions from DataTable schema
            var columnDefinitions = InferColumnDefinitionsFromDataTable(dt);

            // Use the standard UpdateDataTable method
            UpdateDataTable(si, dt, sqa, tableName, columnDefinitions, primaryKeyColumns, excludeFromUpdate, autoTimestamps);
        }

        /// <summary>
        /// Infers column definitions from a DataTable's schema.
        /// Maps .NET types to SQL types automatically.
        /// </summary>
        private List<ColumnDefinition> InferColumnDefinitionsFromDataTable(DataTable dt)
        {
            var columnDefinitions = new List<ColumnDefinition>();

            foreach (DataColumn column in dt.Columns)
            {
                var sqlDbType = MapDotNetTypeToSqlDbType(column.DataType);
                var size = GetColumnSize(column);
                
                columnDefinitions.Add(new ColumnDefinition(column.ColumnName, sqlDbType, size));
            }

            return columnDefinitions;
        }

        /// <summary>
        /// Maps a .NET Type to the corresponding SqlDbType.
        /// </summary>
        private SqlDbType MapDotNetTypeToSqlDbType(Type dotNetType)
        {
            if (dotNetType == typeof(string))
                return SqlDbType.NVarChar;
            if (dotNetType == typeof(int))
                return SqlDbType.Int;
            if (dotNetType == typeof(long))
                return SqlDbType.BigInt;
            if (dotNetType == typeof(short))
                return SqlDbType.SmallInt;
            if (dotNetType == typeof(byte))
                return SqlDbType.TinyInt;
            if (dotNetType == typeof(bool))
                return SqlDbType.Bit;
            if (dotNetType == typeof(DateTime))
                return SqlDbType.DateTime;
            if (dotNetType == typeof(decimal))
                return SqlDbType.Decimal;
            if (dotNetType == typeof(double))
                return SqlDbType.Float;
            if (dotNetType == typeof(float))
                return SqlDbType.Real;
            if (dotNetType == typeof(Guid))
                return SqlDbType.UniqueIdentifier;
            if (dotNetType == typeof(byte[]))
                return SqlDbType.VarBinary;

            // Default to NVarChar for unknown types
            return SqlDbType.NVarChar;
        }

        /// <summary>
        /// Gets the appropriate size for a column based on its DataType and MaxLength.
        /// </summary>
        private int GetColumnSize(DataColumn column)
        {
            // For string types, use MaxLength if specified
            if (column.DataType == typeof(string))
            {
                // If MaxLength is -1 or very large, use a reasonable default
                if (column.MaxLength <= 0 || column.MaxLength > 8000)
                {
                    return 255; // Default reasonable size
                }
                return column.MaxLength;
            }

            // For byte arrays (VarBinary), use MaxLength if specified
            if (column.DataType == typeof(byte[]))
            {
                if (column.MaxLength <= 0 || column.MaxLength > 8000)
                {
                    return 8000; // Max for non-MAX types
                }
                return column.MaxLength;
            }

            // For other types, size is not needed
            return 0;
        }
    }

    /// <summary>
    /// Represents a column definition for dynamic SQL generation.
    /// </summary>
    public class ColumnDefinition
    {
        public string ColumnName { get; set; }
        public SqlDbType SqlDbType { get; set; }
        public int Size { get; set; }

        public ColumnDefinition(string columnName, SqlDbType sqlDbType, int size = 0)
        {
            ColumnName = columnName;
            SqlDbType = sqlDbType;
            Size = size;
        }
    }
}
