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
    }
}