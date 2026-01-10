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
    public class SQA_FMM_Acct_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Acct_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Acct_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_FMM_Acct_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            sqa.UpdateBatchSize = 0; // Set batch size for performance
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Build commands dynamically based on DataTable columns
                    GBL_SQA_Helper.BuildInsertCommand(sqa, _connection, transaction, dt, "FMM_Acct_Config");
                    GBL_SQA_Helper.BuildUpdateCommand(sqa, _connection, transaction, dt, "FMM_Acct_Config", "Acct_ID");
                    GBL_SQA_Helper.BuildDeleteCommand(sqa, _connection, transaction, dt, "FMM_Acct_Config", "Acct_ID");

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

        /// <summary>
        /// Performs a MERGE operation (upsert) on FMM_Acct_Config table
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to merge</param>
        /// <param name="deleteUnmatched">If true, deletes records not in the source DataTable</param>
        /// <param name="deleteCondition">Optional SQL condition for conditional deletes (e.g., "Status = 'Inactive'")</param>
        public void Merge_FMM_Acct_Config(SessionInfo si, DataTable dt, bool deleteUnmatched = false, string deleteCondition = null)
        {
            GBL_SQA_Helper.MergeData(si, _connection, dt, "FMM_Acct_Config", "Acct_ID", deleteUnmatched, deleteCondition);
        }

        /// <summary>
        /// Synchronizes FMM_Acct_Config table with the DataTable (full sync with delete of unmatched records)
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to sync</param>
        /// <param name="syncCondition">Optional SQL condition to limit which records can be deleted (e.g., "Cube_ID = 1")</param>
        public void Sync_FMM_Acct_Config(SessionInfo si, DataTable dt, string syncCondition = null)
        {
            GBL_SQA_Helper.SyncData(si, _connection, dt, "FMM_Acct_Config", "Acct_ID", syncCondition);
        }
    }
}