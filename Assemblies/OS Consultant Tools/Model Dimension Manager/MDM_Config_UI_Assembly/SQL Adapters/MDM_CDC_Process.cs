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
    /// Performance-optimized CDC processor for dimension member updates from external sources or files
    /// </summary>
    public class SQA_MDM_CDC_Process
    {
        private readonly SqlConnection _connection;
        private readonly SessionInfo _si;

        public SQA_MDM_CDC_Process(SessionInfo si, SqlConnection connection)
        {
            _si = si;
            _connection = connection;
        }

        /// <summary>
        /// Process CDC data from external source or file with column mapping support
        /// </summary>
        public void Process_CDC_Data(SessionInfo si, int cdcConfigId, DataTable sourceData, Dictionary<string, string> columnMappings, 
            Dictionary<string, string> textPropertyMappings, string scenarioType = null, string timePeriod = null)
        {
            try
            {
                using (SqlTransaction transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // Get CDC configuration
                        var config = Get_CDC_Config(si, cdcConfigId, transaction);
                        if (config == null)
                            throw new Exception($"CDC Configuration ID {cdcConfigId} not found.");

                        // Prepare dimension member update data
                        var memberUpdates = Map_Source_To_Member_Updates(sourceData, columnMappings, textPropertyMappings, 
                            config, scenarioType, timePeriod);

                        // Bulk process member updates for performance
                        Bulk_Process_Member_Updates(si, memberUpdates, config, transaction);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Read data from external source using configured connection and SQL
        /// </summary>
        public DataTable Read_External_Source_Data(SessionInfo si, string connectionString, string sqlQuery)
        {
            try
            {
                var dt = new DataTable();
                
                using (var extConn = new SqlConnection(connectionString))
                {
                    extConn.Open();
                    using (var cmd = new SqlCommand(sqlQuery, extConn))
                    {
                        cmd.CommandTimeout = 300; // 5 minutes timeout for large datasets
                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Read data from uploaded file (CSV format)
        /// </summary>
        public DataTable Read_File_Source_Data(SessionInfo si, string filePath)
        {
            try
            {
                var dt = new DataTable();
                
                if (!File.Exists(filePath))
                    throw new Exception($"File not found: {filePath}");

                var lines = File.ReadAllLines(filePath);
                if (lines.Length == 0)
                    throw new Exception("File is empty.");

                // Parse header
                var headers = ParseCsvLine(lines[0]);
                foreach (var header in headers)
                {
                    dt.Columns.Add(header.Trim());
                }

                // Parse data rows
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseCsvLine(lines[i]);
                    if (values.Length > 0 && !string.IsNullOrWhiteSpace(string.Join("", values)))
                    {
                        dt.Rows.Add(values);
                    }
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Parse CSV line handling quoted values
        /// </summary>
        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            
            result.Add(current.ToString());
            return result.ToArray();
        }

        private DataRow Get_CDC_Config(SessionInfo si, int cdcConfigId, SqlTransaction transaction)
        {
            string sql = @"SELECT * FROM MDM_CDC_Config WHERE CDC_Config_ID = @CDC_Config_ID";
            
            using (var cmd = new SqlCommand(sql, _connection, transaction))
            {
                cmd.Parameters.Add(new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { Value = cdcConfigId });
                
                var dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
                
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        private List<DimensionMemberUpdate> Map_Source_To_Member_Updates(DataTable sourceData, 
            Dictionary<string, string> columnMappings, Dictionary<string, string> textPropertyMappings,
            DataRow config, string scenarioType, string timePeriod)
        {
            var updates = new List<DimensionMemberUpdate>();
            
            // Get column mapping
            string memberNameColumn = columnMappings.ContainsKey("MemberName") ? columnMappings["MemberName"] : null;
            string memberDescColumn = columnMappings.ContainsKey("MemberDescription") ? columnMappings["MemberDescription"] : null;
            string parentColumn = columnMappings.ContainsKey("Parent") ? columnMappings["Parent"] : null;
            
            if (string.IsNullOrEmpty(memberNameColumn))
                throw new Exception("MemberName column mapping is required.");

            foreach (DataRow row in sourceData.Rows)
            {
                var update = new DimensionMemberUpdate
                {
                    MemberName = row[memberNameColumn].ToString(),
                    Description = !string.IsNullOrEmpty(memberDescColumn) && sourceData.Columns.Contains(memberDescColumn) 
                        ? row[memberDescColumn].ToString() : string.Empty,
                    Parent = !string.IsNullOrEmpty(parentColumn) && sourceData.Columns.Contains(parentColumn)
                        ? row[parentColumn].ToString() : string.Empty,
                    TextProperties = new Dictionary<string, string>(),
                    ScenarioType = scenarioType,
                    TimePeriod = timePeriod
                };

                // Map text properties
                foreach (var mapping in textPropertyMappings)
                {
                    if (sourceData.Columns.Contains(mapping.Value))
                    {
                        update.TextProperties[mapping.Key] = row[mapping.Value].ToString();
                    }
                }

                updates.Add(update);
            }
            
            return updates;
        }

        private void Bulk_Process_Member_Updates(SessionInfo si, List<DimensionMemberUpdate> updates, 
            DataRow config, SqlTransaction transaction)
        {
            // Performance optimization: Batch process updates
            int batchSize = 100;
            int dimId = Convert.ToInt32(config["Dim_ID"]);
            
            for (int i = 0; i < updates.Count; i += batchSize)
            {
                var batch = updates.Skip(i).Take(batchSize).ToList();
                Process_Update_Batch(si, batch, dimId, transaction);
            }
        }

        private void Process_Update_Batch(SessionInfo si, List<DimensionMemberUpdate> batch, 
            int dimId, SqlTransaction transaction)
        {
            // Create staging table for bulk operations
            string stagingTable = "#CDC_Staging_" + Guid.NewGuid().ToString("N");
            
            string createStagingTableSql = $@"
                CREATE TABLE {stagingTable} (
                    MemberName NVARCHAR(255),
                    Description NVARCHAR(500),
                    Parent NVARCHAR(255),
                    ScenarioType NVARCHAR(100),
                    TimePeriod NVARCHAR(100),
                    TextProperty1 NVARCHAR(MAX),
                    TextProperty2 NVARCHAR(MAX),
                    TextProperty3 NVARCHAR(MAX)
                )";
            
            using (var cmd = new SqlCommand(createStagingTableSql, _connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }

            // Bulk insert into staging table
            var stagingData = new DataTable();
            stagingData.Columns.Add("MemberName");
            stagingData.Columns.Add("Description");
            stagingData.Columns.Add("Parent");
            stagingData.Columns.Add("ScenarioType");
            stagingData.Columns.Add("TimePeriod");
            stagingData.Columns.Add("TextProperty1");
            stagingData.Columns.Add("TextProperty2");
            stagingData.Columns.Add("TextProperty3");

            foreach (var update in batch)
            {
                var row = stagingData.NewRow();
                row["MemberName"] = update.MemberName;
                row["Description"] = update.Description;
                row["Parent"] = update.Parent;
                row["ScenarioType"] = update.ScenarioType ?? DBNull.Value;
                row["TimePeriod"] = update.TimePeriod ?? DBNull.Value;
                row["TextProperty1"] = update.TextProperties.ContainsKey("Text1") ? update.TextProperties["Text1"] : DBNull.Value;
                row["TextProperty2"] = update.TextProperties.ContainsKey("Text2") ? update.TextProperties["Text2"] : DBNull.Value;
                row["TextProperty3"] = update.TextProperties.ContainsKey("Text3") ? update.TextProperties["Text3"] : DBNull.Value;
                stagingData.Rows.Add(row);
            }

            using (var bulkCopy = new SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.DestinationTableName = stagingTable;
                bulkCopy.BulkCopyTimeout = 300;
                bulkCopy.WriteToServer(stagingData);
            }

            // Process staging data - this would call OneStream dimension management APIs
            // For now, logging the operations
            string logSql = $@"
                INSERT INTO MDM_CDC_Log (CDC_Config_ID, MemberName, Operation, Processed_Date, Status)
                SELECT {dimId}, MemberName, 'UPDATE', GETDATE(), 'STAGED'
                FROM {stagingTable}";
            
            using (var cmd = new SqlCommand(logSql, _connection, transaction))
            {
                cmd.ExecuteNonQuery();
            }
        }

        private class DimensionMemberUpdate
        {
            public string MemberName { get; set; }
            public string Description { get; set; }
            public string Parent { get; set; }
            public Dictionary<string, string> TextProperties { get; set; }
            public string ScenarioType { get; set; }
            public string TimePeriod { get; set; }
        }
    }
}
