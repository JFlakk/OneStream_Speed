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
    public static class GBL_SQA_Helper
    {
        /// <summary>
        /// Gets the appropriate SqlDbType based on the DataColumn type
        /// </summary>
        private static SqlDbType GetSqlDbType(Type dataType, int maxLength)
        {
            if (dataType == typeof(int))
                return SqlDbType.Int;
            else if (dataType == typeof(long))
                return SqlDbType.BigInt;
            else if (dataType == typeof(short))
                return SqlDbType.SmallInt;
            else if (dataType == typeof(byte))
                return SqlDbType.TinyInt;
            else if (dataType == typeof(bool))
                return SqlDbType.Bit;
            else if (dataType == typeof(DateTime))
                return SqlDbType.DateTime;
            else if (dataType == typeof(decimal))
                return SqlDbType.Decimal;
            else if (dataType == typeof(double))
                return SqlDbType.Float;
            else if (dataType == typeof(float))
                return SqlDbType.Real;
            else if (dataType == typeof(Guid))
                return SqlDbType.UniqueIdentifier;
            else if (dataType == typeof(byte[]))
                return SqlDbType.VarBinary;
            else if (dataType == typeof(string))
            {
                if (maxLength > 4000 || maxLength == -1)
                    return SqlDbType.NVarChar;
                else
                    return SqlDbType.NVarChar;
            }
            else
                return SqlDbType.NVarChar;
        }

        /// <summary>
        /// Builds dynamic INSERT command based on DataTable columns
        /// </summary>
        public static void BuildInsertCommand(SqlDataAdapter sqa, SqlConnection connection, SqlTransaction transaction, DataTable dt, string tableName)
        {
            var columns = new List<string>();
            var parameters = new List<string>();

            foreach (DataColumn column in dt.Columns)
            {
                columns.Add($"[{column.ColumnName}]");
                parameters.Add($"@{column.ColumnName}");
            }

            string insertQuery = $@"
                INSERT INTO [{tableName}] (
                    {string.Join(", ", columns)}
                ) VALUES (
                    {string.Join(", ", parameters)}
                )";

            sqa.InsertCommand = new SqlCommand(insertQuery, connection, transaction);

            foreach (DataColumn column in dt.Columns)
            {
                var sqlDbType = GetSqlDbType(column.DataType, column.MaxLength);
                if (column.MaxLength > 0 && (sqlDbType == SqlDbType.NVarChar || sqlDbType == SqlDbType.VarChar))
                {
                    sqa.InsertCommand.Parameters.Add($"@{column.ColumnName}", sqlDbType, column.MaxLength).SourceColumn = column.ColumnName;
                }
                else
                {
                    sqa.InsertCommand.Parameters.Add($"@{column.ColumnName}", sqlDbType).SourceColumn = column.ColumnName;
                }
            }
        }

        /// <summary>
        /// Builds dynamic UPDATE command based on DataTable columns
        /// </summary>
        public static void BuildUpdateCommand(SqlDataAdapter sqa, SqlConnection connection, SqlTransaction transaction, DataTable dt, string tableName, string primaryKeyColumn)
        {
            var setStatements = new List<string>();

            foreach (DataColumn column in dt.Columns)
            {
                if (!column.ColumnName.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase))
                {
                    setStatements.Add($"[{column.ColumnName}] = @{column.ColumnName}");
                }
            }

            string updateQuery = $@"
                UPDATE [{tableName}] SET
                    {string.Join(", ", setStatements)}
                WHERE [{primaryKeyColumn}] = @{primaryKeyColumn}";

            sqa.UpdateCommand = new SqlCommand(updateQuery, connection, transaction);

            // Add primary key parameter with Original version
            var pkColumn = dt.Columns[primaryKeyColumn];
            if (pkColumn != null)
            {
                var pkSqlDbType = GetSqlDbType(pkColumn.DataType, pkColumn.MaxLength);
                sqa.UpdateCommand.Parameters.Add(new SqlParameter($"@{primaryKeyColumn}", pkSqlDbType)
                {
                    SourceColumn = primaryKeyColumn,
                    SourceVersion = DataRowVersion.Original
                });
            }

            // Add parameters for other columns
            foreach (DataColumn column in dt.Columns)
            {
                if (!column.ColumnName.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase))
                {
                    var sqlDbType = GetSqlDbType(column.DataType, column.MaxLength);
                    if (column.MaxLength > 0 && (sqlDbType == SqlDbType.NVarChar || sqlDbType == SqlDbType.VarChar))
                    {
                        sqa.UpdateCommand.Parameters.Add($"@{column.ColumnName}", sqlDbType, column.MaxLength).SourceColumn = column.ColumnName;
                    }
                    else
                    {
                        sqa.UpdateCommand.Parameters.Add($"@{column.ColumnName}", sqlDbType).SourceColumn = column.ColumnName;
                    }
                }
            }
        }

        /// <summary>
        /// Builds dynamic DELETE command based on primary key
        /// </summary>
        public static void BuildDeleteCommand(SqlDataAdapter sqa, SqlConnection connection, SqlTransaction transaction, DataTable dt, string tableName, string primaryKeyColumn)
        {
            string deleteQuery = $@"
                DELETE FROM [{tableName}] 
                WHERE [{primaryKeyColumn}] = @{primaryKeyColumn}";

            sqa.DeleteCommand = new SqlCommand(deleteQuery, connection, transaction);

            var pkColumn = dt.Columns[primaryKeyColumn];
            if (pkColumn != null)
            {
                var pkSqlDbType = GetSqlDbType(pkColumn.DataType, pkColumn.MaxLength);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter($"@{primaryKeyColumn}", pkSqlDbType)
                {
                    SourceColumn = primaryKeyColumn,
                    SourceVersion = DataRowVersion.Original
                });
            }
        }

        /// <summary>
        /// Performs a MERGE operation (upsert) based on DataTable columns
        /// </summary>
        public static void MergeData(SessionInfo si, SqlConnection connection, DataTable dt, string tableName, string primaryKeyColumn, bool deleteUnmatched = false, string deleteCondition = null)
        {
            if (dt == null || dt.Rows.Count == 0)
                return;

            using (SqlTransaction transaction = connection.BeginTransaction())
            {
                try
                {
                    var columns = new List<string>();
                    var sourceColumns = new List<string>();
                    var updateStatements = new List<string>();
                    var insertColumns = new List<string>();
                    var insertValues = new List<string>();

                    foreach (DataColumn column in dt.Columns)
                    {
                        columns.Add(column.ColumnName);
                        sourceColumns.Add($"@{column.ColumnName}");
                        
                        if (!column.ColumnName.Equals(primaryKeyColumn, StringComparison.OrdinalIgnoreCase))
                        {
                            updateStatements.Add($"[{column.ColumnName}] = source.[{column.ColumnName}]");
                        }
                        
                        insertColumns.Add($"[{column.ColumnName}]");
                        insertValues.Add($"source.[{column.ColumnName}]");
                    }

                    // Build the MERGE statement
                    var mergeBuilder = new StringBuilder();
                    mergeBuilder.AppendLine($"MERGE INTO [{tableName}] AS target");
                    mergeBuilder.AppendLine($"USING (SELECT {string.Join(", ", sourceColumns.Select((s, i) => $"{s} AS [{columns[i]}]"))}) AS source");
                    mergeBuilder.AppendLine($"ON target.[{primaryKeyColumn}] = source.[{primaryKeyColumn}]");
                    mergeBuilder.AppendLine("WHEN MATCHED THEN");
                    mergeBuilder.AppendLine($"    UPDATE SET {string.Join(", ", updateStatements)}");
                    mergeBuilder.AppendLine("WHEN NOT MATCHED BY TARGET THEN");
                    mergeBuilder.AppendLine($"    INSERT ({string.Join(", ", insertColumns)})");
                    mergeBuilder.AppendLine($"    VALUES ({string.Join(", ", insertValues)})");
                    
                    // Add conditional delete clause if requested
                    if (deleteUnmatched)
                    {
                        if (!string.IsNullOrWhiteSpace(deleteCondition))
                        {
                            mergeBuilder.AppendLine($"WHEN NOT MATCHED BY SOURCE AND {deleteCondition} THEN");
                        }
                        else
                        {
                            mergeBuilder.AppendLine("WHEN NOT MATCHED BY SOURCE THEN");
                        }
                        mergeBuilder.AppendLine("    DELETE");
                    }
                    
                    mergeBuilder.AppendLine(";");

                    string mergeQuery = mergeBuilder.ToString();

                    // Execute merge for each row
                    foreach (DataRow row in dt.Rows)
                    {
                        using (SqlCommand cmd = new SqlCommand(mergeQuery, connection, transaction))
                        {
                            foreach (DataColumn column in dt.Columns)
                            {
                                var sqlDbType = GetSqlDbType(column.DataType, column.MaxLength);
                                var paramValue = row[column.ColumnName];
                                
                                if (paramValue == null || paramValue == DBNull.Value)
                                {
                                    cmd.Parameters.Add($"@{column.ColumnName}", sqlDbType).Value = DBNull.Value;
                                }
                                else
                                {
                                    if (column.MaxLength > 0 && (sqlDbType == SqlDbType.NVarChar || sqlDbType == SqlDbType.VarChar))
                                    {
                                        cmd.Parameters.Add($"@{column.ColumnName}", sqlDbType, column.MaxLength).Value = paramValue;
                                    }
                                    else
                                    {
                                        cmd.Parameters.Add($"@{column.ColumnName}", sqlDbType).Value = paramValue;
                                    }
                                }
                            }
                            
                            cmd.ExecuteNonQuery();
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new XFException(si, ex);
                }
            }
        }

        /// <summary>
        /// Synchronizes data - ensures target table matches source DataTable exactly (full sync with delete of unmatched)
        /// </summary>
        public static void SyncData(SessionInfo si, SqlConnection connection, DataTable dt, string tableName, string primaryKeyColumn, string syncCondition = null)
        {
            // Sync is essentially a merge with deleteUnmatched = true
            MergeData(si, connection, dt, tableName, primaryKeyColumn, true, syncCondition);
        }
    }
}
