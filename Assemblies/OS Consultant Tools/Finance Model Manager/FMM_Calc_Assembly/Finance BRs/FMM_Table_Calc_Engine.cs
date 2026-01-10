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
    /// Generic table calculation engine for loading data from custom tables to cube
    /// Optimized for performance and configurability without requiring technical expertise
    /// </summary>
    public class FMM_Table_Calc_Engine
    {
        private SessionInfo si;
        private BRGlobals globals;
        private FinanceRulesApi api;
        private FinanceRulesArgs args;

        public FMM_Table_Calc_Engine(SessionInfo si, BRGlobals globals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            this.si = si;
            this.globals = globals;
            this.api = api;
            this.args = args;
        }

        /// <summary>
        /// Loads data from a custom table to the cube based on configuration
        /// </summary>
        public void LoadTableDataToCube(FMM_Table_Calc_Config config)
        {
            try
            {
                if (api.Pov.Entity.Name.XFContainsIgnoreCase("None")) return;

                // Determine entity hierarchy
                var entityInfo = GetEntityInfo();
                
                // Adjust origin based on entity type
                if (!entityInfo.IsBaseEntity && config.HandleParentEntities)
                {
                    config.TargetOrigin = "AdjInput";
                    config.OriginFilter = "O#AdjInput,O#AdjConsolidated,O#Import";
                }

                // Get current cube data for comparison
                DataBuffer currCubeBuffer = api.Data.GetDataBufferUsingFormula(
                    $"FilterMembers(REMOVEZeros(V#Periodic),[{config.OriginFilter}])"
                );

                // Build and execute SQL query
                string sql = BuildSQLQuery(config, entityInfo);
                
                DataBuffer destBuffer = new DataBuffer();
                DataBuffer clearBuffer = new DataBuffer();

                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    using (var reader = BRApi.Database.ExecuteSqlUsingReader(dbConnApp, sql, false).CreateDataReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                ProcessReaderRow(reader, config, currCubeBuffer, destBuffer);
                            }
                        }
                    }
                }

                // Write destination buffer to cube
                if (destBuffer.DataBufferCells.Count > 0)
                {
                    var destInfo = api.Data.GetExpressionDestinationInfo("V#Periodic");
                    api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, true);
                    destBuffer.DataBufferCells.Clear();
                }

                // Clear stale data if configured
                if (config.ClearStaleData && currCubeBuffer.DataBufferCells.Count > 0)
                {
                    ClearStaleData(currCubeBuffer);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Performs aggregation/consolidation based on configuration
        /// </summary>
        public void AggregateData(FMM_Table_Aggregation_Config config)
        {
            try
            {
                if (api.Pov.Entity.Name.XFContainsIgnoreCase("None")) return;

                var entityInfo = GetEntityInfo();
                if (entityInfo.IsBaseEntity) return; // Only run for parent entities

                // Determine flow filter based on entity level
                var flowFilter = GetFlowFilterForEntityLevel(config, entityInfo.EntityLevel);
                
                // Build filter strings
                string acctFilter = BuildFilterString(config.AccountFilter, "A#");
                string originFilter = BuildFilterString(config.OriginFilter, "O#");

                // Get current cube buffer and source data
                DataBuffer currCubeBuffer = api.Data.GetDataBufferUsingFormula(
                    $"FilterMembers(RemoveNoData(V#Periodic),[{originFilter}],[{flowFilter}],[{acctFilter}])"
                );

                // Build source data from children
                string srcDataScript = BuildChildrenAggregationScript(entityInfo, flowFilter, acctFilter);
                DataBuffer srcDataBuffer = api.Data.GetDataBufferUsingFormula(srcDataScript);

                // Process aggregation
                DataBuffer destBuffer = new DataBuffer();
                DataBuffer clearBuffer = new DataBuffer();

                foreach (var srcCell in srcDataBuffer.DataBufferCells.Values)
                {
                    string destOrigin = srcCell.GetOriginName(api);
                    if (srcCell.GetOriginName(api).XFEqualsIgnoreCase(config.SourceOriginForParents))
                    {
                        destOrigin = config.DestOriginForParents;
                    }

                    var destCell = new DataBufferCell(
                        UpdateCellOrigin(srcCell, destOrigin),
                        srcCell.CellAmount,
                        srcCell.CellStatus
                    );

                    destBuffer.SetCell(si, destCell);
                    currCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk);
                }

                // Write aggregated data
                if (destBuffer.DataBufferCells.Count > 0)
                {
                    var destInfo = api.Data.GetExpressionDestinationInfo("V#Periodic");
                    api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, true);
                }

                // Clear stale data
                foreach (var clearCell in currCubeBuffer.DataBufferCells.Values)
                {
                    var status = new DataCellStatus(false);
                    var clear = new DataBufferCell(clearCell.DataBufferCellPk, 0, status);
                    clearBuffer.SetCell(si, clear);
                }

                if (clearBuffer.DataBufferCells.Count > 0)
                {
                    var clearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic");
                    api.Data.SetDataBuffer(clearBuffer, clearInfo);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #region Helper Methods

        private EntityHierarchyInfo GetEntityInfo()
        {
            var info = new EntityHierarchyInfo
            {
                EntityName = api.Pov.Entity.Name
            };

            var entDimPk = BRApi.Finance.Dim.GetDimPk(si, api.Pov.EntityDim.Name);
            int entityID = api.Members.GetMemberId(DimType.Entity.Id, api.Pov.Entity.Name);
            info.IsBaseEntity = !BRApi.Finance.Members.HasChildren(si, entDimPk, entityID);

            // Get entity level if available via helper
            try
            {
                info.EntityLevel = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, api.Pov.Entity.Name);
            }
            catch
            {
                info.EntityLevel = info.IsBaseEntity ? "Base" : "Parent";
            }

            // Get child and parent entities if not base
            if (!info.IsBaseEntity)
            {
                string cacheKey = $"{api.Pov.Entity.Name}_EntityHierarchy";
                if (globals.GetObject(cacheKey) == null)
                {
                    var entList = BRApi.Finance.Members.GetMembersUsingFilter(
                        si, entDimPk, $"E#{api.Pov.Entity.Name}.Descendants", true
                    );

                    foreach (var ent in entList)
                    {
                        if (!BRApi.Finance.Members.HasChildren(si, entDimPk, ent.Member.MemberId))
                        {
                            info.BaseChildren.Add(ent.Member.Name);
                        }
                        else
                        {
                            info.ParentChildren.Add(ent.Member.Name);
                        }
                    }

                    globals.SetObject(cacheKey, info);
                }
                else
                {
                    info = (EntityHierarchyInfo)globals.GetObject(cacheKey);
                }
            }

            return info;
        }

        private string BuildSQLQuery(FMM_Table_Calc_Config config, EntityHierarchyInfo entityInfo)
        {
            // Calculate time mapping
            var timeInfo = GetTimeInfo(config);
            
            // Build base query
            var selectColumns = string.Join(", ", config.ColumnToDimensionMap.Keys);
            var groupByColumns = string.Join(", ", config.GroupByColumns);

            string sql = $@"
                SELECT {selectColumns}, '{config.TargetOrigin}' as Origin, 
                       SUM({timeInfo.AggregateColumn}) AS Tot_Amount
                FROM {config.SourceTable}
                WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
                  AND Entity = '{api.Pov.Entity.Name}'
                  {BuildFilterConditions(config.FilterConditions, timeInfo)}
                GROUP BY {groupByColumns}";

            // Add union for parent entities if configured
            if (!entityInfo.IsBaseEntity && config.HandleParentEntities)
            {
                if (entityInfo.ParentChildren.Count > 0)
                {
                    string parentFilter = $"('{string.Join("','", entityInfo.ParentChildren)}')";
                    sql += $@"
                        UNION ALL
                        SELECT '{api.Pov.Entity.Name}' as Entity, {selectColumns.Replace("Entity, ", "")}, 
                               'AdjConsolidated' as Origin, SUM({timeInfo.AggregateColumn}) AS Tot_Amount
                        FROM {config.SourceTable}
                        WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
                          AND Entity IN {parentFilter}
                          {BuildFilterConditions(config.FilterConditions, timeInfo)}
                        GROUP BY {groupByColumns.Replace("Entity, ", "")}";
                }

                if (entityInfo.BaseChildren.Count > 0)
                {
                    string baseFilter = $"('{string.Join("','", entityInfo.BaseChildren)}')";
                    sql += $@"
                        UNION ALL
                        SELECT '{api.Pov.Entity.Name}' as Entity, {selectColumns.Replace("Entity, ", "")}, 
                               'Import' as Origin, SUM({timeInfo.AggregateColumn}) AS Tot_Amount
                        FROM {config.SourceTable}
                        WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
                          AND Entity IN {baseFilter}
                          {BuildFilterConditions(config.FilterConditions, timeInfo)}
                        GROUP BY {groupByColumns.Replace("Entity, ", "")}";
                }
            }

            return sql;
        }

        private TimeCalcInfo GetTimeInfo(FMM_Table_Calc_Config config)
        {
            var info = new TimeCalcInfo();
            
            if (config.TimeCalculation == "Annual")
            {
                int wfStartTimeID = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId);
                string wfStartYear = api.Time.GetNameFromId(wfStartTimeID);
                int tableYear = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name));
                int fiscalYear = tableYear - int.Parse(wfStartYear) + 1;
                info.AggregateColumn = $"FY_{fiscalYear}";
            }
            else if (config.TimeCalculation == "Period")
            {
                int povYear = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name));
                int periodNum = api.Time.GetPeriodNumFromId(api.Time.GetIdFromName(api.Pov.Time.Name));
                string monthColumn = api.Pov.Time.Name.Replace(povYear + "M", "");
                
                int wfStartTimeID = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId);
                string wfStartYear = api.Time.GetNameFromId(wfStartTimeID).Substring(0, 4);
                
                info.AggregateColumn = povYear.ToString() == wfStartYear ? $"Month{monthColumn}" : "Yearly";
                info.FiscalYearValue = povYear.ToString();
            }
            else if (config.TimeCalculation == "Fiscal_Year")
            {
                info.AggregateColumn = "Yearly";
                info.FiscalYearValue = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name)).ToString();
            }
            
            return info;
        }

        private string BuildFilterConditions(List<string> conditions, TimeCalcInfo timeInfo)
        {
            if (conditions == null || conditions.Count == 0) return string.Empty;
            
            var result = string.Join(" AND ", conditions.Select(c => $"AND {c}"));
            
            // Add fiscal year filter if needed
            if (!string.IsNullOrEmpty(timeInfo.FiscalYearValue))
            {
                result += $" AND fiscal_year = '{timeInfo.FiscalYearValue}'";
            }
            
            return result;
        }

        private void ProcessReaderRow(
            IDataReader reader, 
            FMM_Table_Calc_Config config, 
            DataBuffer currCubeBuffer, 
            DataBuffer destBuffer)
        {
            var cellPk = new DataBufferCellPk();
            var status = new DataCellStatus(true);

            // Map database columns to dimensions
            foreach (var mapping in config.ColumnToDimensionMap)
            {
                string value = reader[mapping.Key]?.ToString() ?? "None";
                SetDimension(cellPk, mapping.Value, value);
            }

            // Get amount
            decimal amount = 0;
            if (!reader.IsDBNull(reader.GetOrdinal("Tot_Amount")))
            {
                amount = Convert.ToDecimal(reader["Tot_Amount"]);
            }

            if (amount != 0)
            {
                var cell = new DataBufferCell(cellPk, amount, status);
                destBuffer.SetCell(si, cell);
                currCubeBuffer.DataBufferCells.Remove(cellPk);
            }
        }

        private void SetDimension(DataBufferCellPk cellPk, string dimension, string value)
        {
            switch (dimension.ToUpper())
            {
                case "ACCOUNT": cellPk.SetAccount(api, value); break;
                case "FLOW": cellPk.SetFlow(api, value); break;
                case "IC": cellPk.SetIC(api, value); break;
                case "ORIGIN": cellPk.SetOrigin(api, value); break;
                case "UD1": cellPk.SetUD1(api, value); break;
                case "UD2": cellPk.SetUD2(api, value); break;
                case "UD3": cellPk.SetUD3(api, value); break;
                case "UD4": cellPk.SetUD4(api, value); break;
                case "UD5": cellPk.SetUD5(api, value); break;
                case "UD6": cellPk.SetUD6(api, value); break;
                case "UD7": cellPk.SetUD7(api, value); break;
                case "UD8": cellPk.SetUD8(api, value); break;
            }
        }

        private void ClearStaleData(DataBuffer currCubeBuffer)
        {
            var clearBuffer = new DataBuffer();
            foreach (var cell in currCubeBuffer.DataBufferCells.Values)
            {
                var status = new DataCellStatus(false);
                var clearCell = new DataBufferCell(cell.DataBufferCellPk, 0, status);
                clearBuffer.SetCell(si, clearCell);
            }

            if (clearBuffer.DataBufferCells.Count > 0)
            {
                var clearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic");
                api.Data.SetDataBuffer(clearBuffer, clearInfo);
            }
        }

        private string BuildFilterString(List<string> items, string prefix)
        {
            if (items == null || items.Count == 0) return string.Empty;
            return string.Join(",", items.Select(i => $"{prefix}{i}"));
        }

        private string GetFlowFilterForEntityLevel(FMM_Table_Aggregation_Config config, string entityLevel)
        {
            if (config.EntityLevelFlowFilters.ContainsKey(entityLevel))
            {
                return BuildFilterString(config.EntityLevelFlowFilters[entityLevel], "F#");
            }
            
            // Return default if no specific filter
            return BuildFilterString(new List<string> { "Top" }, "F#");
        }

        private string BuildChildrenAggregationScript(EntityHierarchyInfo entityInfo, string flowFilter, string acctFilter)
        {
            var entDimPk = BRApi.Finance.Dim.GetDimPk(si, api.Pov.EntityDim.Name);
            var entList = BRApi.Finance.Members.GetMembersUsingFilter(
                si, entDimPk, $"E#{api.Pov.Entity.Name}.Children", true
            );

            var scripts = new List<string>();
            foreach (var ent in entList)
            {
                scripts.Add($"cb#{api.Pov.Cube.Name}:E#{ent.Member.Name}:C#Aggregated:V#Periodic");
            }

            string srcData = string.Join(" + ", scripts);
            return $"FilterMembers(RemoveNoData({srcData}),[O#AdjConsolidated,O#AdjInput,O#Forms,O#Import],[{flowFilter}],[{acctFilter}])";
        }

        private DataBufferCellPk UpdateCellOrigin(DataBufferCell srcCell, string newOrigin)
        {
            var newPk = new DataBufferCellPk(srcCell.DataBufferCellPk);
            newPk.SetOrigin(api, newOrigin);
            return newPk;
        }

        #endregion

        #region Helper Classes

        private class EntityHierarchyInfo
        {
            public string EntityName { get; set; }
            public bool IsBaseEntity { get; set; }
            public string EntityLevel { get; set; }
            public List<string> BaseChildren { get; set; } = new List<string>();
            public List<string> ParentChildren { get; set; } = new List<string>();
        }

        private class TimeCalcInfo
        {
            public string AggregateColumn { get; set; }
            public string FiscalYearValue { get; set; }
        }

        #endregion
    }
}
