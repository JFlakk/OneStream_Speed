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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.MDM_CDC_StringFunctions
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
        #endregion

        public string Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;

                switch (args.FunctionName)
                {
                    case "ProcessCDCData":
                        return ProcessCDCData();
                    case "UploadFile":
                        return UploadFile();
                    case "ValidateColumnMapping":
                        return ValidateColumnMapping();
                    case "GetSourceDataPreview":
                        return GetSourceDataPreview();
                    default:
                        return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Helper Functions"

        private string ProcessCDCData()
        {
            try
            {
                int cdcConfigId = args.NameValuePairs.XFGetValue("CDC_Config_ID", "-1").XFConvertToInt();
                if (cdcConfigId <= 0)
                    return "Error: Invalid CDC Configuration ID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    
                    var cdcProcessor = new SQA_MDM_CDC_Process(si, connection);
                    
                    // Get configuration
                    var config = GetCDCConfiguration(connection, cdcConfigId);
                    if (config == null)
                        return "Error: CDC Configuration not found";

                    string srcType = config["Src_Type"]?.ToString() ?? "External";
                    DataTable sourceData = null;

                    // Load source data based on type
                    if (srcType.XFEqualsIgnoreCase("File"))
                    {
                        string filePath = config["File_Path"]?.ToString();
                        if (string.IsNullOrEmpty(filePath))
                            return "Error: File path not specified";

                        sourceData = cdcProcessor.Read_File_Source_Data(si, filePath);
                    }
                    else // External
                    {
                        string connString = config["Src_Connection"]?.ToString();
                        string sqlQuery = config["Src_SQL_String"]?.ToString();
                        
                        if (string.IsNullOrEmpty(connString) || string.IsNullOrEmpty(sqlQuery))
                            return "Error: External source connection or SQL not specified";

                        sourceData = cdcProcessor.Read_External_Source_Data(si, connString, sqlQuery);
                    }

                    if (sourceData == null || sourceData.Rows.Count == 0)
                        return "Warning: No source data found";

                    // Parse column mappings
                    var columnMappings = ParseJsonMapping(config["Column_Mappings"]?.ToString());
                    var textPropertyMappings = ParseJsonMapping(config["Text_Property_Mappings"]?.ToString());
                    
                    string scenarioTypeField = config["ScenarioType_Field"]?.ToString();
                    string timeField = config["Time_Field"]?.ToString();

                    // Process the data
                    cdcProcessor.Process_CDC_Data(si, cdcConfigId, sourceData, columnMappings, 
                        textPropertyMappings, scenarioTypeField, timeField);

                    return $"Success: Processed {sourceData.Rows.Count} records";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string UploadFile()
        {
            try
            {
                string fileName = args.NameValuePairs.XFGetValue("FileName", string.Empty);
                string fileContent = args.NameValuePairs.XFGetValue("FileContent", string.Empty);
                
                if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(fileContent))
                    return "Error: File name or content is empty";

                // Use OneStream file system API to save file
                string filePath = BRApi.FileSystem.GetApplicationFolderForDatabase(si, false, "CDC_Uploads");
                string fullPath = Path.Combine(filePath, fileName);

                // Decode base64 content if needed
                byte[] fileBytes = Convert.FromBase64String(fileContent);
                BRApi.FileSystem.WriteFileBytes(si, false, fullPath, fileBytes);

                return $"Success:{fullPath}";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string ValidateColumnMapping()
        {
            try
            {
                string mapping = args.NameValuePairs.XFGetValue("Mapping", string.Empty);
                var parsed = ParseJsonMapping(mapping);
                
                if (parsed == null || parsed.Count == 0)
                    return "Error: Invalid or empty mapping";

                if (!parsed.ContainsKey("MemberName"))
                    return "Error: MemberName mapping is required";

                return $"Success: Valid mapping with {parsed.Count} fields";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private string GetSourceDataPreview()
        {
            try
            {
                int cdcConfigId = args.NameValuePairs.XFGetValue("CDC_Config_ID", "-1").XFConvertToInt();
                int maxRows = args.NameValuePairs.XFGetValue("MaxRows", "10").XFConvertToInt();
                
                if (cdcConfigId <= 0)
                    return "Error: Invalid CDC Configuration ID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    
                    var cdcProcessor = new SQA_MDM_CDC_Process(si, connection);
                    var config = GetCDCConfiguration(connection, cdcConfigId);
                    
                    if (config == null)
                        return "Error: CDC Configuration not found";

                    string srcType = config["Src_Type"]?.ToString() ?? "External";
                    DataTable sourceData = null;

                    if (srcType.XFEqualsIgnoreCase("File"))
                    {
                        string filePath = config["File_Path"]?.ToString();
                        if (string.IsNullOrEmpty(filePath))
                            return "Error: File path not specified";

                        sourceData = cdcProcessor.Read_File_Source_Data(si, filePath);
                    }
                    else
                    {
                        string connString = config["Src_Connection"]?.ToString();
                        string sqlQuery = config["Src_SQL_String"]?.ToString();
                        
                        if (string.IsNullOrEmpty(connString) || string.IsNullOrEmpty(sqlQuery))
                            return "Error: External source connection or SQL not specified";

                        sourceData = cdcProcessor.Read_External_Source_Data(si, connString, sqlQuery);
                    }

                    if (sourceData == null || sourceData.Rows.Count == 0)
                        return "No data available";

                    // Return first N rows as formatted text
                    var sb = new StringBuilder();
                    sb.AppendLine("Columns: " + string.Join(", ", sourceData.Columns.Cast<DataColumn>().Select(c => c.ColumnName)));
                    sb.AppendLine($"Total Rows: {sourceData.Rows.Count}");
                    sb.AppendLine("Preview:");
                    
                    for (int i = 0; i < Math.Min(maxRows, sourceData.Rows.Count); i++)
                    {
                        var row = sourceData.Rows[i];
                        sb.AppendLine($"Row {i + 1}: " + string.Join(" | ", row.ItemArray));
                    }

                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private DataRow GetCDCConfiguration(SqlConnection connection, int cdcConfigId)
        {
            string sql = "SELECT * FROM MDM_CDC_Config WHERE CDC_Config_ID = @CDC_Config_ID";
            using (var cmd = new SqlCommand(sql, connection))
            {
                cmd.Parameters.AddWithValue("@CDC_Config_ID", cdcConfigId);
                var dt = new DataTable();
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
                return dt.Rows.Count > 0 ? dt.Rows[0] : null;
            }
        }

        private Dictionary<string, string> ParseJsonMapping(string json)
        {
            var result = new Dictionary<string, string>();
            
            if (string.IsNullOrEmpty(json))
                return result;

            try
            {
                // Simple JSON parsing for key-value pairs
                // Format: {"Key1":"Value1","Key2":"Value2"}
                json = json.Trim().Trim('{', '}');
                var pairs = json.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var pair in pairs)
                {
                    var parts = pair.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim().Trim('"');
                        string value = parts[1].Trim().Trim('"');
                        result[key] = value;
                    }
                }
            }
            catch
            {
                // Return empty dictionary on parse error
            }

            return result;
        }

        #endregion
    }
}
