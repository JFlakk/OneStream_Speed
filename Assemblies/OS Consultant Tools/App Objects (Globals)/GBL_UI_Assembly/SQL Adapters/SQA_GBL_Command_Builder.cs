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
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_GBL_Command_Builder
    {
        private readonly SqlConnection _connection;

        public SQA_GBL_Command_Builder(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void UpdateTable(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa,
            string[] primaryKeyColumns, string[] excludeFromUpdate = null, string[] excludeFromInsert = null)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Use GBL_SQL_Command_Builder to dynamically generate commands
                    var builder = new GBL_SQL_Command_Builder(_connection, tableName, dt);

                    // Set primary key(s)
                    builder.SetPrimaryKey(primaryKeyColumns);

                    // Set columns to exclude from UPDATE if provided
                    if (excludeFromUpdate != null && excludeFromUpdate.Length > 0)
                    {
                        builder.ExcludeFromUpdate(excludeFromUpdate);
                    }

                    // Set columns to exclude from INSERT if provided
                    if (excludeFromInsert != null && excludeFromInsert.Length > 0)
                    {
                        builder.ExcludeFromInsert(excludeFromInsert);
                    }

                    // Configure the adapter with generated commands
                    builder.ConfigureAdapter(sqa, transaction);

                    // Execute the update
                    sqa.Update(dt);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Cleanup commands to prevent connection leaks
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        public void UpdateTableSimple(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa, string primaryKeyColumn)
        {
            // Standard pattern: exclude primary key and common audit columns from updates
            string[] excludeFromUpdate = new[] { primaryKeyColumn, "Create_Date", "Create_User" };
            UpdateTable(si, tableName, dt, sqa, new[] { primaryKeyColumn }, excludeFromUpdate);
        }

        public void UpdateTableComposite(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa, params string[] primaryKeyColumns)
        {
            // Build exclusion list: all primary keys plus standard audit columns
            var excludeList = new List<string>(primaryKeyColumns);
            excludeList.Add("Create_Date");
            excludeList.Add("Create_User");

            UpdateTable(si, tableName, dt, sqa, primaryKeyColumns, excludeList.ToArray());
        }

        public GBL_SQL_Command_Builder GetCommandBuilder(SessionInfo si, string tableName, DataTable dt)
        {
            return new GBL_SQL_Command_Builder(_connection, tableName, dt);
        }

        /// <summary>
        /// Dynamically fill a DataTable from a SQL query with optional parameters
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="sqa">SqlDataAdapter to use for the Fill operation</param>
        /// <param name="dt">DataTable to fill with results</param>
        /// <param name="sql">SQL SELECT query to execute</param>
        /// <param name="sqlparams">Optional SQL parameters for the query</param>
        public void FillDataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null && sqlparams.Length > 0)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
                command.Parameters.Clear();
                sqa.SelectCommand = null;
            }
        }

        /// <summary>
        /// Merge source DataTable into target DataTable with primary key preservation
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="targetDt">Target DataTable to merge into</param>
        /// <param name="sourceDt">Source DataTable to merge from</param>
        /// <param name="preserveChanges">True to preserve changes in the target; false to overwrite</param>
        /// <param name="missingSchemaAction">Action to take when schema doesn't match</param>
        public void MergeDataTable(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
            bool preserveChanges = true, MissingSchemaAction missingSchemaAction = MissingSchemaAction.Error)
        {
            try
            {
                targetDt.Merge(sourceDt, preserveChanges, missingSchemaAction);
            }
            catch (Exception ex)
            {
                throw new XFException(si, $"Error merging DataTable: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Merge source DataTable into target DataTable with composite primary key handling
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="targetDt">Target DataTable to merge into</param>
        /// <param name="sourceDt">Source DataTable to merge from</param>
        /// <param name="primaryKeyColumns">Array of column names that form the composite primary key</param>
        /// <param name="preserveChanges">True to preserve changes in the target; false to overwrite</param>
        public void MergeDataTableWithKeys(SessionInfo si, DataTable targetDt, DataTable sourceDt, 
            string[] primaryKeyColumns, bool preserveChanges = false)
        {
            try
            {
                // Set primary keys on both tables if not already set
                if (targetDt.PrimaryKey == null || targetDt.PrimaryKey.Length == 0)
                {
                    var targetKeys = primaryKeyColumns.Select(col => targetDt.Columns[col]).ToArray();
                    targetDt.PrimaryKey = targetKeys;
                }

                if (sourceDt.PrimaryKey == null || sourceDt.PrimaryKey.Length == 0)
                {
                    var sourceKeys = primaryKeyColumns.Select(col => sourceDt.Columns[col]).ToArray();
                    sourceDt.PrimaryKey = sourceKeys;
                }

                // Perform the merge
                targetDt.Merge(sourceDt, preserveChanges, MissingSchemaAction.Add);
            }
            catch (Exception ex)
            {
                throw new XFException(si, $"Error merging DataTable with keys: {ex.Message}", ex);
            }
        }
    }
}