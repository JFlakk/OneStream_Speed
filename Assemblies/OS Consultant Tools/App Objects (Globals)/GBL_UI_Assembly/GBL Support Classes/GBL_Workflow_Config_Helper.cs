using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// Helper class for managing configurable workflow system.
    /// Provides methods to differentiate between Cube and Table data sources
    /// and derive approval levels using columns or dimensions.
    /// </summary>
    public class GBL_Workflow_Config_Helper
    {
        private SessionInfo si;

        public GBL_Workflow_Config_Helper(SessionInfo si)
        {
            this.si = si;
        }

        #region "Workflow Configuration Management"

        /// <summary>
        /// Retrieves all workflow configurations for a specific process type.
        /// </summary>
        public DataTable GetWorkflowConfigurations(string processType)
        {
            try
            {
                var sql = @"
                    SELECT 
                        WF_Config_ID,
                        Workflow_Name,
                        Workflow_Type,
                        Process_Type,
                        Description,
                        Is_Active,
                        Require_Comments,
                        Enable_Delegation,
                        Enable_Notifications
                    FROM WF_Config
                    WHERE Process_Type = @ProcessType
                    AND Is_Active = 1
                    ORDER BY Workflow_Name";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ProcessType", SqlDbType.NVarChar, 100) { Value = processType }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves a single workflow configuration by ID.
        /// </summary>
        public DataRow GetWorkflowConfiguration(int workflowConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        WF_Config_ID,
                        Workflow_Name,
                        Workflow_Type,
                        Process_Type,
                        Description,
                        Is_Active,
                        Require_Comments,
                        Enable_Delegation,
                        Enable_Notifications
                    FROM WF_Config
                    WHERE WF_Config_ID = @WorkflowConfigId
                    AND Is_Active = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@WorkflowConfigId", SqlDbType.Int) { Value = workflowConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region "Data Source Management - Cube vs Table Differentiation"

        /// <summary>
        /// Retrieves data source configuration for a workflow.
        /// Differentiates between Cube and Table data sources.
        /// </summary>
        public DataTable GetDataSourceConfigurations(int workflowConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Data_Source_Config_ID,
                        WF_Config_ID,
                        Data_Source_Type,
                        Cube_Name,
                        Table_Name,
                        Schema_Name,
                        Custom_Query,
                        Description,
                        Is_Active
                    FROM WF_Data_Source_Config
                    WHERE WF_Config_ID = @WorkflowConfigId
                    AND Is_Active = 1
                    ORDER BY Data_Source_Type";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@WorkflowConfigId", SqlDbType.Int) { Value = workflowConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves a single data source configuration by ID.
        /// </summary>
        public DataRow GetDataSourceConfiguration(int dataSourceConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Data_Source_Config_ID,
                        WF_Config_ID,
                        Data_Source_Type,
                        Cube_Name,
                        Table_Name,
                        Schema_Name,
                        Custom_Query,
                        Description,
                        Is_Active
                    FROM WF_Data_Source_Config
                    WHERE Data_Source_Config_ID = @DataSourceConfigId
                    AND Is_Active = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@DataSourceConfigId", SqlDbType.Int) { Value = dataSourceConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Determines if a data source is a Cube or Table.
        /// </summary>
        public string GetDataSourceType(int dataSourceConfigId)
        {
            try
            {
                var dataSource = GetDataSourceConfiguration(dataSourceConfigId);
                if (dataSource == null)
                {
                    throw new XFException(si, $"Data source configuration not found: {dataSourceConfigId}");
                }

                return dataSource.Field<string>("Data_Source_Type");
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Checks if a data source is a Cube.
        /// </summary>
        public bool IsCubeDataSource(int dataSourceConfigId)
        {
            var dataSourceType = GetDataSourceType(dataSourceConfigId);
            return dataSourceType.XFEqualsIgnoreCase("Cube");
        }

        /// <summary>
        /// Checks if a data source is a Table.
        /// </summary>
        public bool IsTableDataSource(int dataSourceConfigId)
        {
            var dataSourceType = GetDataSourceType(dataSourceConfigId);
            return dataSourceType.XFEqualsIgnoreCase("Table") || 
                   dataSourceType.XFEqualsIgnoreCase("RelationalView");
        }

        #endregion

        #region "Workflow Step Management"

        /// <summary>
        /// Retrieves workflow steps for a workflow configuration.
        /// </summary>
        public DataTable GetWorkflowSteps(int workflowConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Step_Config_ID,
                        WF_Config_ID,
                        Step_Name,
                        Step_Order,
                        Step_Type,
                        Approval_Type,
                        Min_Approvals,
                        Timeout_Hours,
                        Escalation_Step_Config_ID,
                        Assigned_Role,
                        Assigned_User,
                        Assignment_Rule,
                        Next_Step_Config_ID,
                        Rejection_Step_Config_ID,
                        Is_Active,
                        Description
                    FROM WF_Step_Config
                    WHERE WF_Config_ID = @WorkflowConfigId
                    AND Is_Active = 1
                    ORDER BY Step_Order";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@WorkflowConfigId", SqlDbType.Int) { Value = workflowConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieves a single workflow step by ID.
        /// </summary>
        public DataRow GetWorkflowStep(int stepConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Step_Config_ID,
                        WF_Config_ID,
                        Step_Name,
                        Step_Order,
                        Step_Type,
                        Approval_Type,
                        Min_Approvals,
                        Timeout_Hours,
                        Escalation_Step_Config_ID,
                        Assigned_Role,
                        Assigned_User,
                        Assignment_Rule,
                        Next_Step_Config_ID,
                        Rejection_Step_Config_ID,
                        Is_Active,
                        Description
                    FROM WF_Step_Config
                    WHERE Step_Config_ID = @StepConfigId
                    AND Is_Active = 1";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@StepConfigId", SqlDbType.Int) { Value = stepConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    return dt.Rows.Count > 0 ? dt.Rows[0] : null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region "Approval Level Management - Table Columns"

        /// <summary>
        /// Retrieves column-based approval levels for a workflow step.
        /// Used when the data source is a Table.
        /// </summary>
        public DataTable GetTableApprovalLevels(int stepConfigId, int dataSourceConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Approval_Level_Column_ID,
                        Step_Config_ID,
                        Data_Source_Config_ID,
                        Column_Name,
                        Column_Type,
                        Derivation_Logic,
                        Lookup_Table,
                        Lookup_Column,
                        Lookup_Key_Column,
                        Level_Order,
                        Require_All_Values,
                        Is_Active,
                        Description
                    FROM WF_Approval_Level_Column
                    WHERE Step_Config_ID = @StepConfigId
                    AND Data_Source_Config_ID = @DataSourceConfigId
                    AND Is_Active = 1
                    ORDER BY Level_Order";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@StepConfigId", SqlDbType.Int) { Value = stepConfigId },
                    new SqlParameter("@DataSourceConfigId", SqlDbType.Int) { Value = dataSourceConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Derives approval level from table columns.
        /// Supports direct columns, derived columns, and lookup columns.
        /// </summary>
        public List<Dictionary<string, object>> DeriveTableApprovalLevels(
            int stepConfigId, 
            int dataSourceConfigId, 
            Dictionary<string, object> dataRow)
        {
            try
            {
                var approvalLevels = new List<Dictionary<string, object>>();
                var columnConfigs = GetTableApprovalLevels(stepConfigId, dataSourceConfigId);

                foreach (DataRow colConfig in columnConfigs.Rows)
                {
                    var columnName = colConfig.Field<string>("Column_Name");
                    var columnType = colConfig.Field<string>("Column_Type");
                    var levelOrder = colConfig.Field<int>("Level_Order");
                    
                    object approvalValue = null;

                    if (columnType.XFEqualsIgnoreCase("Direct"))
                    {
                        // Direct column value
                        if (dataRow.ContainsKey(columnName))
                        {
                            approvalValue = dataRow[columnName];
                        }
                    }
                    else if (columnType.XFEqualsIgnoreCase("Derived"))
                    {
                        // Execute derivation logic
                        var derivationLogic = colConfig.Field<string>("Derivation_Logic");
                        approvalValue = ExecuteDerivationLogic(derivationLogic, dataRow);
                    }
                    else if (columnType.XFEqualsIgnoreCase("Lookup"))
                    {
                        // Lookup value from another table
                        var lookupTable = colConfig.Field<string>("Lookup_Table");
                        var lookupColumn = colConfig.Field<string>("Lookup_Column");
                        var lookupKeyColumn = colConfig.Field<string>("Lookup_Key_Column");
                        
                        if (dataRow.ContainsKey(columnName))
                        {
                            var keyValue = dataRow[columnName];
                            approvalValue = ExecuteLookup(lookupTable, lookupColumn, lookupKeyColumn, keyValue);
                        }
                    }

                    if (approvalValue != null)
                    {
                        approvalLevels.Add(new Dictionary<string, object>
                        {
                            { "LevelOrder", levelOrder },
                            { "ColumnName", columnName },
                            { "ApprovalValue", approvalValue },
                            { "ColumnType", columnType }
                        });
                    }
                }

                return approvalLevels;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Executes derivation logic for derived columns.
        /// </summary>
        private object ExecuteDerivationLogic(string derivationLogic, Dictionary<string, object> dataRow)
        {
            try
            {
                // Simple placeholder implementation
                // In production, this could invoke a business rule or execute SQL
                // For now, we'll check if derivation logic is a column name with an operator
                
                if (string.IsNullOrEmpty(derivationLogic))
                {
                    return null;
                }

                // Example: "Column1 + Column2" or "UPPER(Column1)"
                // This is a simplified version; production would need proper parsing
                
                // For now, just return the logic itself as placeholder
                // Real implementation would use BRApi or custom expression evaluator
                return derivationLogic;
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error executing derivation logic: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Executes a lookup query to retrieve approval value.
        /// </summary>
        private object ExecuteLookup(string lookupTable, string lookupColumn, string lookupKeyColumn, object keyValue)
        {
            try
            {
                var sql = $@"
                    SELECT TOP 1 [{lookupColumn}]
                    FROM [{lookupTable}]
                    WHERE [{lookupKeyColumn}] = @KeyValue";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@KeyValue", SqlDbType.NVarChar) { Value = keyValue ?? DBNull.Value }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    var dt = BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                    if (dt.Rows.Count > 0)
                    {
                        return dt.Rows[0][lookupColumn];
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error executing lookup: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region "Approval Level Management - Cube Dimensions"

        /// <summary>
        /// Retrieves dimension-based approval levels for a workflow step.
        /// Used when the data source is a Cube.
        /// </summary>
        public DataTable GetCubeApprovalLevels(int stepConfigId, int dataSourceConfigId)
        {
            try
            {
                var sql = @"
                    SELECT 
                        Approval_Level_Dimension_ID,
                        Step_Config_ID,
                        Data_Source_Config_ID,
                        Dimension_Name,
                        Hierarchy_Name,
                        Level_Name,
                        Member_Filter,
                        Property_Name,
                        Level_Order,
                        Aggregate_To_Level,
                        Require_All_Descendants,
                        Is_Active,
                        Description
                    FROM WF_Approval_Level_Dimension
                    WHERE Step_Config_ID = @StepConfigId
                    AND Data_Source_Config_ID = @DataSourceConfigId
                    AND Is_Active = 1
                    ORDER BY Level_Order";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@StepConfigId", SqlDbType.Int) { Value = stepConfigId },
                    new SqlParameter("@DataSourceConfigId", SqlDbType.Int) { Value = dataSourceConfigId }
                };

                using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    return BRApi.Database.ExecuteSql(dbConnApp, sql, parameters, false);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Derives approval level from cube dimensions.
        /// Uses dimension hierarchy to determine appropriate approval level.
        /// </summary>
        public List<Dictionary<string, object>> DeriveCubeApprovalLevels(
            int stepConfigId, 
            int dataSourceConfigId, 
            Dictionary<string, string> memberSelections)
        {
            try
            {
                var approvalLevels = new List<Dictionary<string, object>>();
                var dimensionConfigs = GetCubeApprovalLevels(stepConfigId, dataSourceConfigId);

                foreach (DataRow dimConfig in dimensionConfigs.Rows)
                {
                    var dimensionName = dimConfig.Field<string>("Dimension_Name");
                    var hierarchyName = dimConfig.Field<string>("Hierarchy_Name");
                    var levelName = dimConfig.Field<string>("Level_Name");
                    var levelOrder = dimConfig.Field<int>("Level_Order");
                    var aggregateToLevel = dimConfig.Field<string>("Aggregate_To_Level");
                    
                    // Get member selection for this dimension
                    string memberName = null;
                    if (memberSelections.ContainsKey(dimensionName))
                    {
                        memberName = memberSelections[dimensionName];
                    }

                    if (!string.IsNullOrEmpty(memberName))
                    {
                        // Determine approval level based on member hierarchy
                        var approvalMember = DetermineApprovalMember(
                            dimensionName, 
                            hierarchyName, 
                            memberName, 
                            levelName, 
                            aggregateToLevel);

                        approvalLevels.Add(new Dictionary<string, object>
                        {
                            { "LevelOrder", levelOrder },
                            { "DimensionName", dimensionName },
                            { "HierarchyName", hierarchyName ?? "Default" },
                            { "SelectedMember", memberName },
                            { "ApprovalMember", approvalMember },
                            { "ApprovalLevel", levelName ?? aggregateToLevel ?? "Base" }
                        });
                    }
                }

                return approvalLevels;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Determines the appropriate approval member based on hierarchy level.
        /// </summary>
        private string DetermineApprovalMember(
            string dimensionName, 
            string hierarchyName, 
            string memberName, 
            string levelName, 
            string aggregateToLevel)
        {
            try
            {
                // Get dimension info from OneStream API
                var dimInfo = BRApi.Finance.Dimensions.GetDimension(si, dimensionName);
                if (dimInfo == null)
                {
                    return memberName;
                }

                // Get member info
                var memberInfo = BRApi.Finance.Members.GetMember(si, dimInfo.DimPk.DimId, memberName);
                if (memberInfo == null)
                {
                    return memberName;
                }

                // If specific level is specified, find parent at that level
                if (!string.IsNullOrEmpty(levelName) || !string.IsNullOrEmpty(aggregateToLevel))
                {
                    var targetLevel = levelName ?? aggregateToLevel;
                    var levelNumber = GetLevelNumber(dimInfo, targetLevel);
                    
                    if (levelNumber >= 0)
                    {
                        // Find parent at target level
                        var parentAtLevel = FindParentAtLevel(si, dimInfo, memberInfo, levelNumber);
                        if (parentAtLevel != null)
                        {
                            return parentAtLevel.Name;
                        }
                    }
                }

                // Default to the member itself
                return memberName;
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error determining approval member: {ex.Message}");
                return memberName;
            }
        }

        /// <summary>
        /// Gets the numeric level for a level name.
        /// </summary>
        private int GetLevelNumber(DimInfo dimInfo, string levelName)
        {
            // Simplified implementation
            // In production, would parse level from dimension hierarchy
            // For now, use basic level mapping
            
            if (levelName.XFEqualsIgnoreCase("Level0") || levelName.XFEqualsIgnoreCase("Base"))
                return 0;
            if (levelName.XFEqualsIgnoreCase("Level1"))
                return 1;
            if (levelName.XFEqualsIgnoreCase("Level2"))
                return 2;
            if (levelName.XFEqualsIgnoreCase("Level3"))
                return 3;
            if (levelName.XFEqualsIgnoreCase("Level4"))
                return 4;
            
            return -1;
        }

        /// <summary>
        /// Finds a parent member at a specific level in the hierarchy.
        /// </summary>
        private MemberInfo FindParentAtLevel(SessionInfo si, DimInfo dimInfo, MemberInfo memberInfo, int targetLevel)
        {
            try
            {
                // Start with the current member
                var currentMember = memberInfo;
                
                // Walk up the hierarchy until we reach the target level
                while (currentMember != null && currentMember.MemberLevel > targetLevel)
                {
                    // Get parent member
                    if (currentMember.ParentMemberId.HasValue && currentMember.ParentMemberId.Value > 0)
                    {
                        currentMember = BRApi.Finance.Members.GetMember(si, dimInfo.DimPk.DimId, currentMember.ParentMemberId.Value);
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (currentMember != null && currentMember.MemberLevel == targetLevel)
                {
                    return currentMember;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                BRApi.ErrorLog.LogMessage(si, $"Error finding parent at level: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region "Validation"

        /// <summary>
        /// Validates workflow configuration integrity.
        /// </summary>
        public List<string> ValidateWorkflowConfiguration(int workflowConfigId)
        {
            var errors = new List<string>();

            try
            {
                // Get workflow configuration
                var workflowConfig = GetWorkflowConfiguration(workflowConfigId);
                if (workflowConfig == null)
                {
                    errors.Add($"Workflow configuration not found: {workflowConfigId}");
                    return errors;
                }

                string workflowName = workflowConfig.Field<string>("Workflow_Name");

                // Validate data sources exist
                var dataSources = GetDataSourceConfigurations(workflowConfigId);
                if (dataSources.Rows.Count == 0)
                {
                    errors.Add($"Workflow {workflowName} has no data sources defined");
                }

                // Validate steps exist
                var steps = GetWorkflowSteps(workflowConfigId);
                if (steps.Rows.Count == 0)
                {
                    errors.Add($"Workflow {workflowName} has no steps defined");
                }

                // Validate each data source has approval levels configured
                foreach (DataRow ds in dataSources.Rows)
                {
                    var dataSourceConfigId = ds.Field<int>("Data_Source_Config_ID");
                    var dataSourceType = ds.Field<string>("Data_Source_Type");
                    
                    foreach (DataRow step in steps.Rows)
                    {
                        var stepConfigId = step.Field<int>("Step_Config_ID");
                        
                        if (dataSourceType.XFEqualsIgnoreCase("Cube"))
                        {
                            var cubeApprovalLevels = GetCubeApprovalLevels(stepConfigId, dataSourceConfigId);
                            if (cubeApprovalLevels.Rows.Count == 0)
                            {
                                errors.Add($"Step {step.Field<string>("Step_Name")} has no cube approval levels configured");
                            }
                        }
                        else if (dataSourceType.XFEqualsIgnoreCase("Table") || 
                                 dataSourceType.XFEqualsIgnoreCase("RelationalView"))
                        {
                            var tableApprovalLevels = GetTableApprovalLevels(stepConfigId, dataSourceConfigId);
                            if (tableApprovalLevels.Rows.Count == 0)
                            {
                                errors.Add($"Step {step.Field<string>("Step_Name")} has no table approval levels configured");
                            }
                        }
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
                return errors;
            }
        }

        #endregion
    }
}
