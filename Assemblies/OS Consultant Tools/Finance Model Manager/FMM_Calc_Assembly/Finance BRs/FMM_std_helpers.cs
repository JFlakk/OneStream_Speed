using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SqlClient;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// FMM Standard Helpers - Comprehensive helper functions for common calculation patterns
    /// 
    /// This class provides configurable helper methods for:
    /// 1. Cube calculations (reading/writing cube data)
    /// 2. Table operations (reading/writing table data)
    /// 3. Table to Cube (loading table data into cube)
    /// 4. BRCubeToTable (extracting cube data to tables)
    /// 5. Consolidation (aggregating data across entity hierarchies)
    /// 
    /// Based on patterns from CMD SPLN and integrated with FMM Table Calc Framework
    /// </summary>
    public class FMM_std_helpers
    {
        #region Core Properties
        
        public SessionInfo si;
        public BRGlobals globals;
        public FinanceRulesApi api;
        public FinanceRulesArgs args;
        
        #endregion

        #region Constructor
        
        public FMM_std_helpers(SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            this.si = si;
            this.globals = globals;
            this.api = api;
            this.args = args;
        }
        
        #endregion

        #region 1. Cube Calculation Helpers

        /// <summary>
        /// Configuration for cube data operations
        /// </summary>
        public class CubeCalcConfig
        {
            public string SourceMemberScript { get; set; }
            public string TargetOrigin { get; set; } = "Import";
            public List<string> AccountFilter { get; set; } = new List<string>();
            public List<string> FlowFilter { get; set; } = new List<string>();
            public Dictionary<string, string> DimensionMappings { get; set; } = new Dictionary<string, string>();
            public bool ClearTargetData { get; set; } = true;
            public decimal MultiplierFactor { get; set; } = 1.0m;
        }

        /// <summary>
        /// Read data from cube using member script filter
        /// Example: Read all funding data for an entity with specific flow status
        /// </summary>
        public DataBuffer ReadCubeData(CubeCalcConfig config)
        {
            try
            {
                var dataBuffer = api.Data.GetDataBufferUsingFormula(config.SourceMemberScript);
                
                if (dataBuffer == null || dataBuffer.DataBufferCells.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, $"No data returned from cube for script: {config.SourceMemberScript}");
                    return new DataBuffer();
                }
                
                BRApi.ErrorLog.LogMessage(si, $"Retrieved {dataBuffer.DataBufferCells.Count} cells from cube");
                return dataBuffer;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Write data to cube with dimension transformations
        /// Example: Copy cube data from one account/flow to another
        /// </summary>
        public void WriteCubeData(DataBuffer sourceBuffer, CubeCalcConfig config)
        {
            try
            {
                if (sourceBuffer == null || sourceBuffer.DataBufferCells.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No source data to write");
                    return;
                }

                var targetBuffer = new DataBuffer();
                
                foreach (var cell in sourceBuffer.DataBufferCells.Values)
                {
                    var targetCell = new DataBufferCell(cell);
                    
                    // Apply dimension mappings
                    foreach (var mapping in config.DimensionMappings)
                    {
                        ApplyDimensionMapping(ref targetCell, mapping.Key, mapping.Value);
                    }
                    
                    // Set target origin
                    if (!string.IsNullOrEmpty(config.TargetOrigin))
                    {
                        targetCell.DataBufferCellPk.OriginId = api.Members.GetMemberId(DimType.Origin.Id, config.TargetOrigin);
                    }
                    
                    // Apply multiplier
                    targetCell.CellAmount = cell.CellAmount * config.MultiplierFactor;
                    
                    targetBuffer.SetCell(targetCell);
                }
                
                // Clear target data if configured
                if (config.ClearTargetData)
                {
                    api.Data.ClearData(targetBuffer);
                }
                
                // Write data to cube
                api.Data.SetDataBuffer(targetBuffer);
                
                BRApi.ErrorLog.LogMessage(si, $"Wrote {targetBuffer.DataBufferCells.Count} cells to cube");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Copy cube data with transformations (common pattern for spreading/allocating data)
        /// Example: Copy Target account to Commitments/Obligations with rate spreads
        /// </summary>
        public void CopyCubeDataWithTransform(CubeCalcConfig config, DataTable rateTable = null, string rateKeyColumn = null)
        {
            try
            {
                // Read source data
                var sourceBuffer = ReadCubeData(config);
                
                if (sourceBuffer.DataBufferCells.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No source data found for cube copy operation");
                    return;
                }
                
                // If rate table provided, apply rates per account
                if (rateTable != null && rateTable.Rows.Count > 0)
                {
                    ApplyRateSpreadToCube(sourceBuffer, config, rateTable, rateKeyColumn);
                }
                else
                {
                    // Direct copy with mappings
                    WriteCubeData(sourceBuffer, config);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Apply rate spreads from table to cube data (like CMD SPLN CivPay/Withhold patterns)
        /// </summary>
        private void ApplyRateSpreadToCube(DataBuffer sourceBuffer, CubeCalcConfig config, DataTable rateTable, string rateKeyColumn)
        {
            try
            {
                var targetBuffer = new DataBuffer();
                
                foreach (var cell in sourceBuffer.DataBufferCells.Values)
                {
                    string keyValue = GetCellDimensionValue(cell, rateKeyColumn);
                    
                    // Find matching rate rows
                    var rateRows = rateTable.Select($"{rateKeyColumn} = '{keyValue}'");
                    
                    if (rateRows.Length == 0)
                    {
                        BRApi.ErrorLog.LogMessage(si, $"No rate found for {rateKeyColumn} = {keyValue}");
                        continue;
                    }
                    
                    var rateRow = rateRows[0];
                    
                    // Apply to each target account
                    foreach (var targetAccount in config.AccountFilter)
                    {
                        // Monthly spread
                        for (int month = 1; month <= 12; month++)
                        {
                            string timeCol = $"Time{month}";
                            if (!rateRow.Table.Columns.Contains(timeCol)) continue;
                            
                            decimal rate = Convert.ToDecimal(rateRow[timeCol]);
                            if (rate == 0) continue;
                            
                            var targetCell = new DataBufferCell(cell);
                            
                            // Map to target account
                            targetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id, targetAccount);
                            
                            // Map to target time period
                            var targetTime = GetMonthPeriod(month);
                            targetCell.DataBufferCellPk.TimeId = api.Members.GetMemberId(DimType.Time.Id, targetTime);
                            
                            // Apply dimension mappings
                            foreach (var mapping in config.DimensionMappings)
                            {
                                ApplyDimensionMapping(ref targetCell, mapping.Key, mapping.Value);
                            }
                            
                            // Calculate amount with rate
                            targetCell.CellAmount = cell.CellAmount * (rate / 100m);
                            
                            targetBuffer.SetCell(targetCell);
                        }
                    }
                }
                
                // Write to cube
                if (config.ClearTargetData && targetBuffer.DataBufferCells.Count > 0)
                {
                    api.Data.ClearData(targetBuffer);
                }
                api.Data.SetDataBuffer(targetBuffer);
                
                BRApi.ErrorLog.LogMessage(si, $"Applied rate spread to {targetBuffer.DataBufferCells.Count} cells");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region 2. Table Operation Helpers

        /// <summary>
        /// Configuration for table operations
        /// </summary>
        public class TableOpConfig
        {
            public string TableName { get; set; }
            public List<string> SelectColumns { get; set; } = new List<string>();
            public Dictionary<string, object> FilterConditions { get; set; } = new Dictionary<string, object>();
            public string WhereClause { get; set; }
            public bool UseWorkflowFilters { get; set; } = true;
        }

        /// <summary>
        /// Read data from custom table with filters
        /// Example: Read requirements from XFC_CMD_PGM_REQ_Details table
        /// </summary>
        public DataTable ReadTableData(TableOpConfig config)
        {
            try
            {
                var resultTable = new DataTable();
                
                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                using (var sqlConn = new SqlConnection(dbConnApp.ConnectionString))
                {
                    sqlConn.Open();
                    
                    // Build SELECT clause
                    string selectClause = config.SelectColumns.Count > 0 
                        ? string.Join(", ", config.SelectColumns)
                        : "*";
                    
                    // Build WHERE clause
                    string whereClause = BuildWhereClause(config);
                    
                    string sql = $"SELECT {selectClause} FROM {config.TableName} WITH (NOLOCK)";
                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        sql += $" WHERE {whereClause}";
                    }
                    
                    BRApi.ErrorLog.LogMessage(si, $"Executing SQL: {sql}");
                    
                    using (var cmd = new SqlCommand(sql, sqlConn))
                    using (var adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(resultTable);
                    }
                    
                    BRApi.ErrorLog.LogMessage(si, $"Retrieved {resultTable.Rows.Count} rows from {config.TableName}");
                }
                
                return resultTable;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Write data to custom table with merge logic
        /// Example: Write updated requirements back to XFC_CMD_PGM_REQ_Details
        /// </summary>
        public void WriteTableData(DataTable sourceData, TableOpConfig config, List<string> keyColumns)
        {
            try
            {
                if (sourceData == null || sourceData.Rows.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No data to write to table");
                    return;
                }
                
                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                using (var sqlConn = new SqlConnection(dbConnApp.ConnectionString))
                {
                    sqlConn.Open();
                    
                    // Read existing data
                    var existingData = ReadTableData(config);
                    
                    // Merge logic: Update existing, Insert new
                    foreach (DataRow sourceRow in sourceData.Rows)
                    {
                        string keyFilter = BuildKeyFilter(sourceRow, keyColumns);
                        var existingRows = existingData.Select(keyFilter);
                        
                        if (existingRows.Length > 0)
                        {
                            // Update existing row
                            UpdateTableRow(sqlConn, config.TableName, sourceRow, keyFilter);
                        }
                        else
                        {
                            // Insert new row
                            InsertTableRow(sqlConn, config.TableName, sourceRow);
                        }
                    }
                    
                    BRApi.ErrorLog.LogMessage(si, $"Wrote {sourceData.Rows.Count} rows to {config.TableName}");
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Delete data from table based on filters
        /// Example: Clear stale requirements before reloading
        /// </summary>
        public void DeleteTableData(TableOpConfig config)
        {
            try
            {
                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                using (var sqlConn = new SqlConnection(dbConnApp.ConnectionString))
                {
                    sqlConn.Open();
                    
                    string whereClause = BuildWhereClause(config);
                    string sql = $"DELETE FROM {config.TableName}";
                    
                    if (!string.IsNullOrEmpty(whereClause))
                    {
                        sql += $" WHERE {whereClause}";
                    }
                    
                    BRApi.ErrorLog.LogMessage(si, $"Executing DELETE: {sql}");
                    
                    using (var cmd = new SqlCommand(sql, sqlConn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        BRApi.ErrorLog.LogMessage(si, $"Deleted {rowsAffected} rows from {config.TableName}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region 3. Table to Cube Helpers (Leveraging FMM Framework)

        /// <summary>
        /// Load table data to cube using FMM framework (recommended approach)
        /// Example: Load CMD_PGM requirements to cube
        /// </summary>
        public void LoadTableToCube_Framework(string tablePrefix, string timeCalculation, List<string> accounts, 
            List<string> statusFilters = null, List<string> dimensionFilters = null, string filterDimension = "UD1")
        {
            try
            {
                // Parse filters if not provided
                if (statusFilters == null || dimensionFilters == null)
                {
                    FMM_Table_Calc_Builder.ParseGlobalFilters(globals, api, 
                        out statusFilters, out dimensionFilters);
                }
                
                // Build configuration based on time calculation type
                FMM_Table_Calc_Config config;
                
                if (timeCalculation.XFEqualsIgnoreCase("Annual") || timeCalculation.XFEqualsIgnoreCase("Fiscal_Year"))
                {
                    config = FMM_Table_Calc_Builder.BuildRequirementsTableConfig(
                        prefix: tablePrefix,
                        timeCalculation: timeCalculation,
                        accounts: accounts,
                        statusFilters: statusFilters,
                        appnFilters: dimensionFilters
                    );
                }
                else if (timeCalculation.XFEqualsIgnoreCase("Period") || timeCalculation.XFEqualsIgnoreCase("Monthly"))
                {
                    config = FMM_Table_Calc_Builder.BuildPeriodicTableConfig(
                        prefix: tablePrefix,
                        accounts: accounts,
                        statusFilters: statusFilters,
                        dimensionFilters: dimensionFilters,
                        filterDimension: filterDimension
                    );
                }
                else
                {
                    throw new XFException(si, $"Unsupported time calculation: {timeCalculation}");
                }
                
                // Execute using FMM engine
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.LoadTableDataToCube(config);
                
                BRApi.ErrorLog.LogMessage(si, $"Completed table to cube load for {tablePrefix}");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Custom table to cube load for non-standard patterns
        /// Example: Load with custom dimension mappings or complex transformations
        /// </summary>
        public void LoadTableToCube_Custom(TableOpConfig tableConfig, CubeCalcConfig cubeConfig, 
            Dictionary<string, string> columnToDimensionMap)
        {
            try
            {
                // Read data from table
                var tableData = ReadTableData(tableConfig);
                
                if (tableData.Rows.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, $"No data found in table {tableConfig.TableName}");
                    return;
                }
                
                // Convert table rows to cube cells
                var targetBuffer = new DataBuffer();
                
                foreach (DataRow row in tableData.Rows)
                {
                    var cell = ConvertTableRowToCubeCell(row, columnToDimensionMap);
                    
                    if (cell != null)
                    {
                        // Apply cube config transformations
                        foreach (var mapping in cubeConfig.DimensionMappings)
                        {
                            ApplyDimensionMapping(ref cell, mapping.Key, mapping.Value);
                        }
                        
                        targetBuffer.SetCell(cell);
                    }
                }
                
                // Write to cube
                if (cubeConfig.ClearTargetData && targetBuffer.DataBufferCells.Count > 0)
                {
                    api.Data.ClearData(targetBuffer);
                }
                api.Data.SetDataBuffer(targetBuffer);
                
                BRApi.ErrorLog.LogMessage(si, $"Loaded {targetBuffer.DataBufferCells.Count} cells from table to cube");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region 4. BRCubeToTable Helpers

        /// <summary>
        /// Configuration for cube to table extraction
        /// </summary>
        public class CubeToTableConfig
        {
            public string SourceMemberScript { get; set; }
            public string TargetTableName { get; set; }
            public Dictionary<string, string> DimensionToColumnMap { get; set; } = new Dictionary<string, string>();
            public List<string> AdditionalColumns { get; set; } = new List<string>();
            public bool ClearTargetTable { get; set; } = true;
            public bool IncludeWorkflowInfo { get; set; } = true;
        }

        /// <summary>
        /// Extract cube data to custom table (reverse of table to cube)
        /// Example: Extract actuals/forecast data to a staging table for reporting
        /// </summary>
        public void ExtractCubeToTable(CubeToTableConfig config)
        {
            try
            {
                // Read cube data
                var sourceBuffer = api.Data.GetDataBufferUsingFormula(config.SourceMemberScript);
                
                if (sourceBuffer == null || sourceBuffer.DataBufferCells.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No cube data to extract to table");
                    return;
                }
                
                // Create DataTable structure
                var extractTable = BuildExtractTableStructure(config);
                
                // Convert cube cells to table rows
                foreach (var cell in sourceBuffer.DataBufferCells.Values)
                {
                    var row = extractTable.NewRow();
                    
                    // Map dimensions to columns
                    foreach (var dimMap in config.DimensionToColumnMap)
                    {
                        string dimValue = GetCellDimensionValue(cell, dimMap.Key);
                        row[dimMap.Value] = dimValue;
                    }
                    
                    // Add amount
                    row["Amount"] = cell.CellAmount;
                    
                    // Add workflow info if configured
                    if (config.IncludeWorkflowInfo)
                    {
                        AddWorkflowInfoToRow(row);
                    }
                    
                    extractTable.Rows.Add(row);
                }
                
                // Write to target table
                var tableConfig = new TableOpConfig
                {
                    TableName = config.TargetTableName
                };
                
                if (config.ClearTargetTable)
                {
                    DeleteTableData(tableConfig);
                }
                
                WriteTableData(extractTable, tableConfig, new List<string>());
                
                BRApi.ErrorLog.LogMessage(si, $"Extracted {extractTable.Rows.Count} cells from cube to {config.TargetTableName}");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region 5. Consolidation Helpers

        /// <summary>
        /// Consolidate/aggregate data across entity hierarchy using FMM framework
        /// Example: Roll up base entity data to parent entities
        /// </summary>
        public void ConsolidateData_Framework(List<string> accounts, 
            Dictionary<string, List<string>> levelFlowFilters = null)
        {
            try
            {
                // Build aggregation configuration
                var config = FMM_Table_Calc_Builder.BuildStandardAggregationConfig(
                    configName: "StandardConsolidation",
                    accounts: accounts,
                    levelFlowFilters: levelFlowFilters
                );
                
                // Execute using FMM engine
                var engine = new FMM_Table_Calc_Engine(si, globals, api, args);
                engine.AggregateData(config);
                
                BRApi.ErrorLog.LogMessage(si, "Completed consolidation using FMM framework");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Custom consolidation with specific business logic
        /// Example: Consolidate with filters and origin transformations
        /// </summary>
        public void ConsolidateData_Custom(CubeCalcConfig config, string entityDimName = "Entity")
        {
            try
            {
                var entDimPk = BRApi.Finance.Dim.GetDimPk(si, entityDimName);
                var currentEntityId = api.Members.GetMemberId(DimType.Entity.Id, api.Pov.Entity.Name);
                
                // Check if current entity has children
                bool hasChildren = BRApi.Finance.Members.HasChildren(si, entDimPk, currentEntityId);
                
                if (!hasChildren)
                {
                    BRApi.ErrorLog.LogMessage(si, $"Entity {api.Pov.Entity.Name} is a base entity, no consolidation needed");
                    return;
                }
                
                // Get all child base entities
                var childEntities = BRApi.Finance.Members.GetMembersUsingFilter(
                    si, entDimPk, $"E#{api.Pov.Entity.Name}.Descendants", true);
                
                var baseEntities = new List<string>();
                foreach (var ent in childEntities)
                {
                    if (!BRApi.Finance.Members.HasChildren(si, entDimPk, ent.Member.MemberId))
                    {
                        baseEntities.Add(ent.Member.Name);
                    }
                }
                
                if (baseEntities.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No base entities found for consolidation");
                    return;
                }
                
                // Build member script for base entities
                string entityFilter = string.Join(",", baseEntities.Select(e => $"E#{e}"));
                string consolidationScript = config.SourceMemberScript.Replace("E#POV", entityFilter);
                
                // Read data from base entities
                var sourceBuffer = api.Data.GetDataBufferUsingFormula(consolidationScript);
                
                if (sourceBuffer == null || sourceBuffer.DataBufferCells.Count == 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "No data found for consolidation");
                    return;
                }
                
                // Aggregate to parent
                var parentBuffer = new DataBuffer();
                var aggregateMap = new Dictionary<string, decimal>();
                
                foreach (var cell in sourceBuffer.DataBufferCells.Values)
                {
                    var parentCell = new DataBufferCell(cell);
                    parentCell.DataBufferCellPk.EntityId = currentEntityId;
                    
                    // Apply dimension mappings
                    foreach (var mapping in config.DimensionMappings)
                    {
                        ApplyDimensionMapping(ref parentCell, mapping.Key, mapping.Value);
                    }
                    
                    // Aggregate amounts with same key
                    string cellKey = parentCell.DataBufferCellPk.ToString();
                    if (aggregateMap.ContainsKey(cellKey))
                    {
                        aggregateMap[cellKey] += cell.CellAmount;
                    }
                    else
                    {
                        aggregateMap[cellKey] = cell.CellAmount;
                        parentBuffer.SetCell(parentCell);
                    }
                }
                
                // Update amounts in parent buffer
                foreach (var cell in parentBuffer.DataBufferCells.Values)
                {
                    string cellKey = cell.DataBufferCellPk.ToString();
                    cell.CellAmount = aggregateMap[cellKey];
                }
                
                // Write to cube
                if (config.ClearTargetData)
                {
                    api.Data.ClearData(parentBuffer);
                }
                api.Data.SetDataBuffer(parentBuffer);
                
                BRApi.ErrorLog.LogMessage(si, $"Consolidated {baseEntities.Count} base entities to {api.Pov.Entity.Name}");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region Helper Utility Methods

        private void ApplyDimensionMapping(ref DataBufferCell cell, string dimensionType, string targetValue)
        {
            try
            {
                int targetId = api.Members.GetMemberId(GetDimTypeId(dimensionType), targetValue);
                
                switch (dimensionType.ToUpper())
                {
                    case "ACCOUNT": cell.DataBufferCellPk.AccountId = targetId; break;
                    case "FLOW": cell.DataBufferCellPk.FlowId = targetId; break;
                    case "ORIGIN": cell.DataBufferCellPk.OriginId = targetId; break;
                    case "IC": cell.DataBufferCellPk.ICId = targetId; break;
                    case "UD1": cell.DataBufferCellPk.UD1Id = targetId; break;
                    case "UD2": cell.DataBufferCellPk.UD2Id = targetId; break;
                    case "UD3": cell.DataBufferCellPk.UD3Id = targetId; break;
                    case "UD4": cell.DataBufferCellPk.UD4Id = targetId; break;
                    case "UD5": cell.DataBufferCellPk.UD5Id = targetId; break;
                    case "UD6": cell.DataBufferCellPk.UD6Id = targetId; break;
                    case "UD7": cell.DataBufferCellPk.UD7Id = targetId; break;
                    case "UD8": cell.DataBufferCellPk.UD8Id = targetId; break;
                    case "ENTITY": cell.DataBufferCellPk.EntityId = targetId; break;
                    case "TIME": cell.DataBufferCellPk.TimeId = targetId; break;
                }
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error applying dimension mapping {dimensionType}={targetValue}: {ex.Message}");
            }
        }

        private string GetCellDimensionValue(DataBufferCell cell, string dimensionType)
        {
            switch (dimensionType.ToUpper())
            {
                case "ACCOUNT": return cell.DataBufferCellPk.GetAccountName(api);
                case "ENTITY": return cell.DataBufferCellPk.GetEntityName(api);
                case "FLOW": return cell.DataBufferCellPk.GetFlowName(api);
                case "ORIGIN": return cell.DataBufferCellPk.GetOriginName(api);
                case "IC": return cell.DataBufferCellPk.GetICName(api);
                case "UD1": return cell.DataBufferCellPk.GetUD1Name(api);
                case "UD2": return cell.DataBufferCellPk.GetUD2Name(api);
                case "UD3": return cell.DataBufferCellPk.GetUD3Name(api);
                case "UD4": return cell.DataBufferCellPk.GetUD4Name(api);
                case "UD5": return cell.DataBufferCellPk.GetUD5Name(api);
                case "UD6": return cell.DataBufferCellPk.GetUD6Name(api);
                case "UD7": return cell.DataBufferCellPk.GetUD7Name(api);
                case "UD8": return cell.DataBufferCellPk.GetUD8Name(api);
                case "TIME": return cell.DataBufferCellPk.GetTimeName(api);
                default: return string.Empty;
            }
        }

        private int GetDimTypeId(string dimensionType)
        {
            switch (dimensionType.ToUpper())
            {
                case "ACCOUNT": return DimType.Account.Id;
                case "ENTITY": return DimType.Entity.Id;
                case "FLOW": return DimType.Flow.Id;
                case "ORIGIN": return DimType.Origin.Id;
                case "IC": return DimType.IC.Id;
                case "UD1": return DimType.UD1.Id;
                case "UD2": return DimType.UD2.Id;
                case "UD3": return DimType.UD3.Id;
                case "UD4": return DimType.UD4.Id;
                case "UD5": return DimType.UD5.Id;
                case "UD6": return DimType.UD6.Id;
                case "UD7": return DimType.UD7.Id;
                case "UD8": return DimType.UD8.Id;
                case "TIME": return DimType.Time.Id;
                case "SCENARIO": return DimType.Scenario.Id;
                case "VIEW": return DimType.View.Id;
                default: return -1;
            }
        }

        private string GetMonthPeriod(int month)
        {
            var povYear = api.Time.GetYearFromId(api.Pov.Time.MemberId);
            return $"{povYear}M{month}";
        }

        private string BuildWhereClause(TableOpConfig config)
        {
            var clauses = new List<string>();
            
            // Add custom where clause
            if (!string.IsNullOrEmpty(config.WhereClause))
            {
                clauses.Add(config.WhereClause);
            }
            
            // Add filter conditions
            foreach (var filter in config.FilterConditions)
            {
                if (filter.Value is string)
                {
                    clauses.Add($"{filter.Key} = '{filter.Value}'");
                }
                else if (filter.Value is List<string>)
                {
                    var values = string.Join("','", (List<string>)filter.Value);
                    clauses.Add($"{filter.Key} IN ('{values}')");
                }
                else
                {
                    clauses.Add($"{filter.Key} = {filter.Value}");
                }
            }
            
            // Add workflow filters if configured
            if (config.UseWorkflowFilters)
            {
                clauses.Add($"WFScenario_Name = '{api.Pov.Scenario.Name}'");
                clauses.Add($"WFTime_Name = '{api.Pov.Time.Name}'");
                clauses.Add($"Entity = '{api.Pov.Entity.Name}'");
            }
            
            return clauses.Count > 0 ? string.Join(" AND ", clauses) : string.Empty;
        }

        private string BuildKeyFilter(DataRow row, List<string> keyColumns)
        {
            var filters = new List<string>();
            foreach (var col in keyColumns)
            {
                if (row.Table.Columns.Contains(col))
                {
                    filters.Add($"{col} = '{row[col]}'");
                }
            }
            return string.Join(" AND ", filters);
        }

        private void UpdateTableRow(SqlConnection conn, string tableName, DataRow row, string keyFilter)
        {
            var setClauses = new List<string>();
            foreach (DataColumn col in row.Table.Columns)
            {
                if (!col.ColumnName.EndsWith("_ID", StringComparison.OrdinalIgnoreCase))
                {
                    setClauses.Add($"{col.ColumnName} = @{col.ColumnName}");
                }
            }
            
            string sql = $"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {keyFilter}";
            
            using (var cmd = new SqlCommand(sql, conn))
            {
                foreach (DataColumn col in row.Table.Columns)
                {
                    cmd.Parameters.AddWithValue($"@{col.ColumnName}", row[col]);
                }
                cmd.ExecuteNonQuery();
            }
        }

        private void InsertTableRow(SqlConnection conn, string tableName, DataRow row)
        {
            var columns = new List<string>();
            var parameters = new List<string>();
            
            foreach (DataColumn col in row.Table.Columns)
            {
                columns.Add(col.ColumnName);
                parameters.Add($"@{col.ColumnName}");
            }
            
            string sql = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", parameters)})";
            
            using (var cmd = new SqlCommand(sql, conn))
            {
                foreach (DataColumn col in row.Table.Columns)
                {
                    cmd.Parameters.AddWithValue($"@{col.ColumnName}", row[col]);
                }
                cmd.ExecuteNonQuery();
            }
        }

        private DataBufferCell ConvertTableRowToCubeCell(DataRow row, Dictionary<string, string> columnToDimensionMap)
        {
            try
            {
                var cell = new DataBufferCell();
                cell.DataBufferCellPk.ScenarioId = api.Pov.Scenario.MemberId;
                cell.DataBufferCellPk.ViewId = api.Pov.View.MemberId;
                
                // Map columns to dimensions
                foreach (var mapping in columnToDimensionMap)
                {
                    string columnName = mapping.Key;
                    string dimensionType = mapping.Value;
                    
                    if (!row.Table.Columns.Contains(columnName)) continue;
                    
                    string memberName = row[columnName].ToString();
                    if (string.IsNullOrEmpty(memberName)) continue;
                    
                    int memberId = api.Members.GetMemberId(GetDimTypeId(dimensionType), memberName);
                    
                    switch (dimensionType.ToUpper())
                    {
                        case "ACCOUNT": cell.DataBufferCellPk.AccountId = memberId; break;
                        case "ENTITY": cell.DataBufferCellPk.EntityId = memberId; break;
                        case "FLOW": cell.DataBufferCellPk.FlowId = memberId; break;
                        case "ORIGIN": cell.DataBufferCellPk.OriginId = memberId; break;
                        case "IC": cell.DataBufferCellPk.ICId = memberId; break;
                        case "UD1": cell.DataBufferCellPk.UD1Id = memberId; break;
                        case "UD2": cell.DataBufferCellPk.UD2Id = memberId; break;
                        case "UD3": cell.DataBufferCellPk.UD3Id = memberId; break;
                        case "UD4": cell.DataBufferCellPk.UD4Id = memberId; break;
                        case "UD5": cell.DataBufferCellPk.UD5Id = memberId; break;
                        case "UD6": cell.DataBufferCellPk.UD6Id = memberId; break;
                        case "UD7": cell.DataBufferCellPk.UD7Id = memberId; break;
                        case "UD8": cell.DataBufferCellPk.UD8Id = memberId; break;
                        case "TIME": cell.DataBufferCellPk.TimeId = memberId; break;
                    }
                }
                
                // Set amount
                if (row.Table.Columns.Contains("Amount"))
                {
                    cell.CellAmount = Convert.ToDecimal(row["Amount"]);
                }
                
                return cell;
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error converting table row to cube cell: {ex.Message}");
                return null;
            }
        }

        private DataTable BuildExtractTableStructure(CubeToTableConfig config)
        {
            var table = new DataTable();
            
            // Add dimension columns
            foreach (var mapping in config.DimensionToColumnMap)
            {
                table.Columns.Add(mapping.Value, typeof(string));
            }
            
            // Add amount column
            table.Columns.Add("Amount", typeof(decimal));
            
            // Add workflow columns if configured
            if (config.IncludeWorkflowInfo)
            {
                table.Columns.Add("WFScenario_Name", typeof(string));
                table.Columns.Add("WFTime_Name", typeof(string));
                table.Columns.Add("WFCube_Name", typeof(string));
                table.Columns.Add("Create_Date", typeof(DateTime));
                table.Columns.Add("Create_User", typeof(string));
            }
            
            // Add additional columns
            foreach (var col in config.AdditionalColumns)
            {
                if (!table.Columns.Contains(col))
                {
                    table.Columns.Add(col, typeof(string));
                }
            }
            
            return table;
        }

        private void AddWorkflowInfoToRow(DataRow row)
        {
            if (row.Table.Columns.Contains("WFScenario_Name"))
                row["WFScenario_Name"] = api.Pov.Scenario.Name;
            
            if (row.Table.Columns.Contains("WFTime_Name"))
                row["WFTime_Name"] = api.Pov.Time.Name;
            
            if (row.Table.Columns.Contains("WFCube_Name"))
                row["WFCube_Name"] = api.Pov.Cube.Name;
            
            if (row.Table.Columns.Contains("Create_Date"))
                row["Create_Date"] = DateTime.Now;
            
            if (row.Table.Columns.Contains("Create_User"))
                row["Create_User"] = si.UserName;
        }

        #endregion
    }
}
