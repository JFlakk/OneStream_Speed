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
    /// <summary>
    /// SQL Adapter wrapper for GBL_SQL_Command_Builder that simplifies usage.
    /// This adapter allows you to pass DataTables and automatically build and execute
    /// INSERT/UPDATE/DELETE commands without hardcoding SQL statements.
    /// </summary>
    public class SQA_GBL_Command_Builder
    {
        private readonly SqlConnection _connection;

        public SQA_GBL_Command_Builder(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        /// <summary>
        /// Update a DataTable with automatic SQL command generation.
        /// This method will handle INSERT, UPDATE, and DELETE operations based on row states.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="tableName">Database table name</param>
        /// <param name="dt">DataTable with changes to apply</param>
        /// <param name="sqa">SqlDataAdapter to use for the update</param>
        /// <param name="primaryKeyColumns">Primary key column name(s)</param>
        /// <param name="excludeFromUpdate">Columns to exclude from UPDATE operations (e.g., audit fields)</param>
        /// <param name="excludeFromInsert">Columns to exclude from INSERT operations (e.g., identity columns)</param>
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
                    
                    // Cleanup
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
        /// Simplified update for tables with a single primary key.
        /// Automatically excludes the primary key and standard audit columns from updates.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="tableName">Database table name</param>
        /// <param name="dt">DataTable with changes to apply</param>
        /// <param name="sqa">SqlDataAdapter to use for the update</param>
        /// <param name="primaryKeyColumn">Primary key column name</param>
        public void UpdateTableSimple(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa, string primaryKeyColumn)
        {
            // Standard pattern: exclude primary key and common audit columns from updates
            string[] excludeFromUpdate = new[] { primaryKeyColumn, "Create_Date", "Create_User" };
            UpdateTable(si, tableName, dt, sqa, new[] { primaryKeyColumn }, excludeFromUpdate);
        }

        /// <summary>
        /// Update with composite primary key.
        /// Automatically excludes all primary keys and standard audit columns from updates.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="tableName">Database table name</param>
        /// <param name="dt">DataTable with changes to apply</param>
        /// <param name="sqa">SqlDataAdapter to use for the update</param>
        /// <param name="primaryKeyColumns">Primary key column names</param>
        public void UpdateTableComposite(SessionInfo si, string tableName, DataTable dt, SqlDataAdapter sqa, params string[] primaryKeyColumns)
        {
            // Build exclusion list: all primary keys plus standard audit columns
            var excludeList = new List<string>(primaryKeyColumns);
            excludeList.Add("Create_Date");
            excludeList.Add("Create_User");
            
            UpdateTable(si, tableName, dt, sqa, primaryKeyColumns, excludeList.ToArray());
        }

        /// <summary>
        /// Get a configured GBL_SQL_Command_Builder instance for advanced scenarios.
        /// Use this when you need fine-grained control over the command builder.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="tableName">Database table name</param>
        /// <param name="dt">DataTable with schema information</param>
        /// <returns>Configured GBL_SQL_Command_Builder instance</returns>
        public GBL_SQL_Command_Builder GetCommandBuilder(SessionInfo si, string tableName, DataTable dt)
        {
            return new GBL_SQL_Command_Builder(_connection, tableName, dt);
        }
    }
}
