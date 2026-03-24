using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.FMM_Generate_DDL_From_Config
{
    /// <summary>
    /// XFBR to generate DDL files from FMM table configuration metadata.
    /// This reads the FMM_Table_Config tables and generates SQL DDL scripts.
    /// </summary>
    public class MainClass
    {
        public object Main(SessionInfo si, string processType, string outputFileName)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(processType))
                {
                    throw new XFException(si, "ProcessType parameter is required. Example: 'FMM', 'DDM', 'MDM'");
                }

                if (string.IsNullOrWhiteSpace(outputFileName))
                {
                    outputFileName = $"DDL_{processType}_Generated_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                }

                // Create helper instance
                var helper = new FMM_Table_Config_Helper(si);

                // Generate DDL
                BRApi.ErrorLog.LogMessage(si, $"Generating DDL for process type: {processType}");
                string ddl = helper.GenerateCompleteDDL(processType);

                if (string.IsNullOrEmpty(ddl))
                {
                    return $"No table configurations found for process type: {processType}";
                }

                // Save to file in user's documents folder
                var folderPath = $"Documents/Users/{si.UserName}/GeneratedDDL";
                var filePath = $"{folderPath}/{outputFileName}";

                // Ensure folder exists
                var folderInfo = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, folderPath, true);

                // Create file
                var fileBytes = Encoding.UTF8.GetBytes(ddl);
                var fileInfo = new XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath);
                var file = new XFFile(fileInfo, string.Empty, fileBytes);
                BRApi.FileSystem.InsertOrUpdateFile(si, file);

                BRApi.ErrorLog.LogMessage(si, $"DDL generated successfully: {filePath}");
                
                return $"DDL file generated successfully:\n{filePath}\n\nFile size: {fileBytes.Length} bytes\n\nPreview (first 500 characters):\n{ddl.Substring(0, Math.Min(500, ddl.Length))}...";
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }

    /// <summary>
    /// Dashboard Extender version for UI integration.
    /// Allows users to generate DDL files through a dashboard interface.
    /// </summary>
    public class DashboardExtenderClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        if (args.FunctionName.XFEqualsIgnoreCase("Generate_DDL_From_Config"))
                        {
                            return GenerateDDLFromConfig(si, args);
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult GenerateDDLFromConfig(SessionInfo si, DashboardExtenderArgs args)
        {
            try
            {
                // Get parameters from dashboard
                string processType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_ProcessType", "");
                string outputFileName = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_OutputFileName", "");

                if (string.IsNullOrWhiteSpace(processType))
                {
                    var result = new XFSelectionChangedTaskResult();
                    result.IsOK = false;
                    result.Message = "Please specify a Process Type (e.g., FMM, DDM, MDM)";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(outputFileName))
                {
                    outputFileName = $"DDL_{processType}_Generated_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                }

                // Create helper instance
                var helper = new FMM_Table_Config_Helper(si);

                // Generate DDL
                BRApi.ErrorLog.LogMessage(si, $"Generating DDL for process type: {processType}");
                string ddl = helper.GenerateCompleteDDL(processType);

                if (string.IsNullOrEmpty(ddl))
                {
                    var result = new XFSelectionChangedTaskResult();
                    result.IsOK = false;
                    result.Message = $"No table configurations found for process type: {processType}";
                    return result;
                }

                // Save to file in user's documents folder
                var folderPath = $"Documents/Users/{si.UserName}/GeneratedDDL";
                var filePath = $"{folderPath}/{outputFileName}";

                // Ensure folder exists
                var folderInfo = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, folderPath, true);

                // Create file
                var fileBytes = Encoding.UTF8.GetBytes(ddl);
                var fileInfo = new XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath);
                var file = new XFFile(fileInfo, string.Empty, fileBytes);
                BRApi.FileSystem.InsertOrUpdateFile(si, file);

                BRApi.ErrorLog.LogMessage(si, $"DDL generated successfully: {filePath}");

                var taskResult = new XFSelectionChangedTaskResult();
                taskResult.IsOK = true;
                taskResult.Message = $"DDL file generated successfully:\n{filePath}\n\nFile size: {fileBytes.Length:N0} bytes";
                
                return taskResult;
            }
            catch (Exception ex)
            {
                var result = new XFSelectionChangedTaskResult();
                result.IsOK = false;
                result.Message = $"Error generating DDL: {ex.Message}";
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }

    /// <summary>
    /// Helper class to generate DDL for individual tables.
    /// Can be used for more granular DDL generation.
    /// </summary>
    public class TableDDLGenerator
    {
        private SessionInfo si;
        private FMM_Table_Config_Helper helper;

        public TableDDLGenerator(SessionInfo si)
        {
            this.si = si;
            this.helper = new FMM_Table_Config_Helper(si);
        }

        /// <summary>
        /// Generates DDL for a specific table by TableConfigID.
        /// </summary>
        public string GenerateTableDDL(int tableConfigId, bool includeIndexes = true, bool includeForeignKeys = true)
        {
            var ddl = new StringBuilder();

            // Generate table
            ddl.AppendLine(helper.GenerateCreateTableDDL(tableConfigId));
            ddl.AppendLine();

            // Generate indexes
            if (includeIndexes)
            {
                var indexDDL = helper.GenerateCreateIndexesDDL(tableConfigId);
                if (!string.IsNullOrEmpty(indexDDL))
                {
                    ddl.AppendLine(indexDDL);
                    ddl.AppendLine();
                }
            }

            // Generate foreign keys
            if (includeForeignKeys)
            {
                var fkDDL = helper.GenerateForeignKeysDDL(tableConfigId);
                if (!string.IsNullOrEmpty(fkDDL))
                {
                    ddl.AppendLine(fkDDL);
                    ddl.AppendLine();
                }
            }

            return ddl.ToString();
        }

        /// <summary>
        /// Generates DDL for specific tables by name.
        /// </summary>
        public string GenerateTableDDLByName(string tableName, bool includeIndexes = true, bool includeForeignKeys = true)
        {
            try
            {
                // Find table config by name
                var sql = @"
                    SELECT TableConfigID 
                    FROM FMM_Table_Config 
                    WHERE TableName = @TableName 
                    AND IsActive = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@TableName", SqlDbType.NVarChar, 255) { Value = tableName }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    
                    if (dt.Rows.Count == 0)
                    {
                        throw new XFException(si, $"Table configuration not found for table: {tableName}");
                    }

                    int tableConfigId = dt.Rows[0].Field<int>("TableConfigID");
                    return GenerateTableDDL(tableConfigId, includeIndexes, includeForeignKeys);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Generates DDL for multiple tables.
        /// </summary>
        public string GenerateMultipleTablesDDL(List<string> tableNames, bool includeIndexes = true, bool includeForeignKeys = true)
        {
            var ddl = new StringBuilder();
            
            ddl.AppendLine("-- =============================================");
            ddl.AppendLine($"-- DDL for Multiple Tables");
            ddl.AppendLine($"-- Generated: {DateTime.Now}");
            ddl.AppendLine("-- =============================================");
            ddl.AppendLine();

            foreach (var tableName in tableNames)
            {
                try
                {
                    ddl.AppendLine(GenerateTableDDLByName(tableName, includeIndexes, includeForeignKeys));
                }
                catch (Exception ex)
                {
                    ddl.AppendLine($"-- Error generating DDL for {tableName}: {ex.Message}");
                    BRApi.ErrorLog.LogMessage(si, $"Error generating DDL for {tableName}: {ex.Message}");
                }
            }

            return ddl.ToString();
        }
    }
}
