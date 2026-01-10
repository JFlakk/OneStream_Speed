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
    public class SQA_DDM_Config_Menu
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }


        public void Fill_DDM_Config_Menu_DataTable(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
				command.Parameters.Clear();
				sqa.SelectCommand = null;
            }
        }

        public void Update_DDM_Config_Menu(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Build commands dynamically based on DataTable columns
                    GBL_SQA_Helper.BuildInsertCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu");
                    GBL_SQA_Helper.BuildUpdateCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu", "DDM_Menu_ID");
                    GBL_SQA_Helper.BuildDeleteCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu", "DDM_Menu_ID");

                    sqa.Update(dt);
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
        /// Performs a MERGE operation (upsert) on DDM_Config_Menu table
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to merge</param>
        /// <param name="deleteUnmatched">If true, deletes records not in the source DataTable</param>
        /// <param name="deleteCondition">Optional SQL condition for conditional deletes (e.g., "Status = 'Inactive'")</param>
        public void Merge_DDM_Config_Menu(SessionInfo si, DataTable dt, bool deleteUnmatched = false, string deleteCondition = null)
        {
            GBL_SQA_Helper.MergeData(si, _connection, dt, "DDM_Config_Menu", "DDM_Menu_ID", deleteUnmatched, deleteCondition);
        }

        /// <summary>
        /// Synchronizes DDM_Config_Menu table with the DataTable (full sync with delete of unmatched records)
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to sync</param>
        /// <param name="syncCondition">Optional SQL condition to limit which records can be deleted</param>
        public void Sync_DDM_Config_Menu(SessionInfo si, DataTable dt, string syncCondition = null)
        {
            GBL_SQA_Helper.SyncData(si, _connection, dt, "DDM_Config_Menu", "DDM_Menu_ID", syncCondition);
        }
    }
}