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
    /// Configuration class for table-based calculations
    /// Provides simplified, configurable approach to loading data from custom tables to cube
    /// </summary>
    public class FMM_Table_Calc_Config
    {
        public string ConfigName { get; set; }
        public string SourceTable { get; set; }
        public string TimeColumnMapping { get; set; } // FY_1,FY_2 or Month1,Month2 etc
        public string TimeCalculation { get; set; } // "Annual" or "Period" or "Fiscal_Year"
        public List<string> GroupByColumns { get; set; } = new List<string>();
        public List<string> FilterConditions { get; set; } = new List<string>();
        public Dictionary<string, string> ColumnToDimensionMap { get; set; } = new Dictionary<string, string>();
        public string TargetOrigin { get; set; } = "Import";
        public string OriginFilter { get; set; } = "O#Import";
        public bool HandleParentEntities { get; set; } = true;
        public bool ClearStaleData { get; set; } = true;
        
        /// <summary>
        /// Standard dimension mappings for typical scenarios
        /// </summary>
        public void SetStandardDimensionMapping()
        {
            ColumnToDimensionMap = new Dictionary<string, string>
            {
                {"Entity", "Entity"},
                {"Account", "Account"},
                {"IC", "IC"},
                {"Flow", "Flow"},
                {"Origin", "Origin"},
                {"UD1", "UD1"},
                {"UD2", "UD2"},
                {"UD3", "UD3"},
                {"UD4", "UD4"},
                {"UD5", "UD5"},
                {"UD6", "UD6"},
                {"UD7", "UD7"},
                {"UD8", "UD8"}
            };
        }
    }
    
    /// <summary>
    /// Aggregation configuration for consolidating data
    /// </summary>
    public class FMM_Table_Aggregation_Config
    {
        public string ConfigName { get; set; }
        public List<string> AccountFilter { get; set; } = new List<string>();
        public List<string> OriginFilter { get; set; } = new List<string>();
        public List<string> FlowFilter { get; set; } = new List<string>();
        public Dictionary<string, List<string>> EntityLevelFlowFilters { get; set; } = new Dictionary<string, List<string>>();
        public string SourceOriginForParents { get; set; } = "AdjInput";
        public string DestOriginForParents { get; set; } = "AdjConsolidated";
        
        /// <summary>
        /// Set default aggregation filters for typical consolidation scenarios
        /// </summary>
        public void SetDefaultAggregationFilters()
        {
            OriginFilter = new List<string> { "AdjConsolidated", "AdjInput", "Forms", "Import" };
        }
    }
}
