using System;
using System.Data;
using Microsoft.Data.SqlClient;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    /// <summary>
    /// SQL Adapter for Workflow Configuration data access.
    /// Provides direct database access methods for workflow configuration tables.
    /// </summary>
    public class SQA_GBL_Workflow_Config
    {
        private readonly SqlConnection _connection;

        public SQA_GBL_Workflow_Config(SqlConnection connection)
        {
            _connection = connection;
        }

        #region "WF_Config Operations"

        /// <summary>
        /// Fills DataTable with workflow configurations.
        /// </summary>
        public void Fill_WF_Config(SqlDataAdapter adapter, DataTable dt, string whereClause = null, params SqlParameter[] sqlParams)
        {
            var selectQuery = @"
                SELECT 
                    WF_Config_ID,
                    Workflow_Name,
                    Workflow_Type,
                    Process_Type,
                    Description,
                    Is_Active,
                    Require_Comments,
                    Enable_Delegation,
                    Enable_Notifications,
                    Create_Date,
                    Create_User,
                    Update_Date,
                    Update_User
                FROM WF_Config";

            if (!string.IsNullOrEmpty(whereClause))
            {
                selectQuery += " WHERE " + whereClause;
            }

            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                command.Parameters.Clear();
                adapter.SelectCommand = null;
            }
        }

        /// <summary>
        /// Adds parameters for WF_Config insert/update operations.
        /// </summary>
        private void AddWFConfigParameters(SqlCommand cmd, bool isUpdate = false)
        {
            if (isUpdate)
            {
                cmd.Parameters.Add("@WF_Config_ID", SqlDbType.Int).SourceColumn = "WF_Config_ID";
            }
            cmd.Parameters.Add("@Workflow_Name", SqlDbType.NVarChar, 255).SourceColumn = "Workflow_Name";
            cmd.Parameters.Add("@Workflow_Type", SqlDbType.NVarChar, 50).SourceColumn = "Workflow_Type";
            cmd.Parameters.Add("@Process_Type", SqlDbType.NVarChar, 100).SourceColumn = "Process_Type";
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).SourceColumn = "Description";
            cmd.Parameters.Add("@Is_Active", SqlDbType.Bit).SourceColumn = "Is_Active";
            cmd.Parameters.Add("@Require_Comments", SqlDbType.Bit).SourceColumn = "Require_Comments";
            cmd.Parameters.Add("@Enable_Delegation", SqlDbType.Bit).SourceColumn = "Enable_Delegation";
            cmd.Parameters.Add("@Enable_Notifications", SqlDbType.Bit).SourceColumn = "Enable_Notifications";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User";
            
            if (!isUpdate)
            {
                cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 100).SourceColumn = "Create_User";
            }
        }

        #endregion

        #region "WF_Data_Source_Config Operations"

        /// <summary>
        /// Fills DataTable with data source configurations.
        /// </summary>
        public void Fill_WF_Data_Source_Config(SqlDataAdapter adapter, DataTable dt, string whereClause = null, params SqlParameter[] sqlParams)
        {
            var selectQuery = @"
                SELECT 
                    Data_Source_Config_ID,
                    WF_Config_ID,
                    Data_Source_Type,
                    Cube_Name,
                    Table_Name,
                    Schema_Name,
                    Custom_Query,
                    Description,
                    Is_Active,
                    Create_Date,
                    Create_User,
                    Update_Date,
                    Update_User
                FROM WF_Data_Source_Config";

            if (!string.IsNullOrEmpty(whereClause))
            {
                selectQuery += " WHERE " + whereClause;
            }

            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                command.Parameters.Clear();
                adapter.SelectCommand = null;
            }
        }

        /// <summary>
        /// Adds parameters for WF_Data_Source_Config insert/update operations.
        /// </summary>
        private void AddDataSourceConfigParameters(SqlCommand cmd, bool isUpdate = false)
        {
            if (isUpdate)
            {
                cmd.Parameters.Add("@Data_Source_Config_ID", SqlDbType.Int).SourceColumn = "Data_Source_Config_ID";
            }
            cmd.Parameters.Add("@WF_Config_ID", SqlDbType.Int).SourceColumn = "WF_Config_ID";
            cmd.Parameters.Add("@Data_Source_Type", SqlDbType.NVarChar, 50).SourceColumn = "Data_Source_Type";
            cmd.Parameters.Add("@Cube_Name", SqlDbType.NVarChar, 255).SourceColumn = "Cube_Name";
            cmd.Parameters.Add("@Table_Name", SqlDbType.NVarChar, 255).SourceColumn = "Table_Name";
            cmd.Parameters.Add("@Schema_Name", SqlDbType.NVarChar, 50).SourceColumn = "Schema_Name";
            cmd.Parameters.Add("@Custom_Query", SqlDbType.NVarChar, -1).SourceColumn = "Custom_Query";
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).SourceColumn = "Description";
            cmd.Parameters.Add("@Is_Active", SqlDbType.Bit).SourceColumn = "Is_Active";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User";
            
            if (!isUpdate)
            {
                cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 100).SourceColumn = "Create_User";
            }
        }

        #endregion

        #region "WF_Step_Config Operations"

        /// <summary>
        /// Fills DataTable with workflow step configurations.
        /// </summary>
        public void Fill_WF_Step_Config(SqlDataAdapter adapter, DataTable dt, string whereClause = null, params SqlParameter[] sqlParams)
        {
            var selectQuery = @"
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
                    Description,
                    Create_Date,
                    Create_User,
                    Update_Date,
                    Update_User
                FROM WF_Step_Config";

            if (!string.IsNullOrEmpty(whereClause))
            {
                selectQuery += " WHERE " + whereClause;
            }

            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                command.Parameters.Clear();
                adapter.SelectCommand = null;
            }
        }

        /// <summary>
        /// Adds parameters for WF_Step_Config insert/update operations.
        /// </summary>
        private void AddStepConfigParameters(SqlCommand cmd, bool isUpdate = false)
        {
            if (isUpdate)
            {
                cmd.Parameters.Add("@Step_Config_ID", SqlDbType.Int).SourceColumn = "Step_Config_ID";
            }
            cmd.Parameters.Add("@WF_Config_ID", SqlDbType.Int).SourceColumn = "WF_Config_ID";
            cmd.Parameters.Add("@Step_Name", SqlDbType.NVarChar, 255).SourceColumn = "Step_Name";
            cmd.Parameters.Add("@Step_Order", SqlDbType.Int).SourceColumn = "Step_Order";
            cmd.Parameters.Add("@Step_Type", SqlDbType.NVarChar, 50).SourceColumn = "Step_Type";
            cmd.Parameters.Add("@Approval_Type", SqlDbType.NVarChar, 50).SourceColumn = "Approval_Type";
            cmd.Parameters.Add("@Min_Approvals", SqlDbType.Int).SourceColumn = "Min_Approvals";
            cmd.Parameters.Add("@Timeout_Hours", SqlDbType.Int).SourceColumn = "Timeout_Hours";
            cmd.Parameters.Add("@Escalation_Step_Config_ID", SqlDbType.Int).SourceColumn = "Escalation_Step_Config_ID";
            cmd.Parameters.Add("@Assigned_Role", SqlDbType.NVarChar, 255).SourceColumn = "Assigned_Role";
            cmd.Parameters.Add("@Assigned_User", SqlDbType.NVarChar, 255).SourceColumn = "Assigned_User";
            cmd.Parameters.Add("@Assignment_Rule", SqlDbType.NVarChar, -1).SourceColumn = "Assignment_Rule";
            cmd.Parameters.Add("@Next_Step_Config_ID", SqlDbType.Int).SourceColumn = "Next_Step_Config_ID";
            cmd.Parameters.Add("@Rejection_Step_Config_ID", SqlDbType.Int).SourceColumn = "Rejection_Step_Config_ID";
            cmd.Parameters.Add("@Is_Active", SqlDbType.Bit).SourceColumn = "Is_Active";
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).SourceColumn = "Description";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User";
            
            if (!isUpdate)
            {
                cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 100).SourceColumn = "Create_User";
            }
        }

        #endregion

        #region "WF_Approval_Level_Column Operations"

        /// <summary>
        /// Fills DataTable with column-based approval level configurations.
        /// </summary>
        public void Fill_WF_Approval_Level_Column(SqlDataAdapter adapter, DataTable dt, string whereClause = null, params SqlParameter[] sqlParams)
        {
            var selectQuery = @"
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
                    Description,
                    Create_Date,
                    Create_User,
                    Update_Date,
                    Update_User
                FROM WF_Approval_Level_Column";

            if (!string.IsNullOrEmpty(whereClause))
            {
                selectQuery += " WHERE " + whereClause;
            }

            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                command.Parameters.Clear();
                adapter.SelectCommand = null;
            }
        }

        /// <summary>
        /// Adds parameters for WF_Approval_Level_Column insert/update operations.
        /// </summary>
        private void AddApprovalLevelColumnParameters(SqlCommand cmd, bool isUpdate = false)
        {
            if (isUpdate)
            {
                cmd.Parameters.Add("@Approval_Level_Column_ID", SqlDbType.Int).SourceColumn = "Approval_Level_Column_ID";
            }
            cmd.Parameters.Add("@Step_Config_ID", SqlDbType.Int).SourceColumn = "Step_Config_ID";
            cmd.Parameters.Add("@Data_Source_Config_ID", SqlDbType.Int).SourceColumn = "Data_Source_Config_ID";
            cmd.Parameters.Add("@Column_Name", SqlDbType.NVarChar, 255).SourceColumn = "Column_Name";
            cmd.Parameters.Add("@Column_Type", SqlDbType.NVarChar, 50).SourceColumn = "Column_Type";
            cmd.Parameters.Add("@Derivation_Logic", SqlDbType.NVarChar, -1).SourceColumn = "Derivation_Logic";
            cmd.Parameters.Add("@Lookup_Table", SqlDbType.NVarChar, 255).SourceColumn = "Lookup_Table";
            cmd.Parameters.Add("@Lookup_Column", SqlDbType.NVarChar, 255).SourceColumn = "Lookup_Column";
            cmd.Parameters.Add("@Lookup_Key_Column", SqlDbType.NVarChar, 255).SourceColumn = "Lookup_Key_Column";
            cmd.Parameters.Add("@Level_Order", SqlDbType.Int).SourceColumn = "Level_Order";
            cmd.Parameters.Add("@Require_All_Values", SqlDbType.Bit).SourceColumn = "Require_All_Values";
            cmd.Parameters.Add("@Is_Active", SqlDbType.Bit).SourceColumn = "Is_Active";
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).SourceColumn = "Description";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User";
            
            if (!isUpdate)
            {
                cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 100).SourceColumn = "Create_User";
            }
        }

        #endregion

        #region "WF_Approval_Level_Dimension Operations"

        /// <summary>
        /// Fills DataTable with dimension-based approval level configurations.
        /// </summary>
        public void Fill_WF_Approval_Level_Dimension(SqlDataAdapter adapter, DataTable dt, string whereClause = null, params SqlParameter[] sqlParams)
        {
            var selectQuery = @"
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
                    Description,
                    Create_Date,
                    Create_User,
                    Update_Date,
                    Update_User
                FROM WF_Approval_Level_Dimension";

            if (!string.IsNullOrEmpty(whereClause))
            {
                selectQuery += " WHERE " + whereClause;
            }

            using (var command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlParams != null)
                {
                    command.Parameters.AddRange(sqlParams);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dt);
                command.Parameters.Clear();
                adapter.SelectCommand = null;
            }
        }

        /// <summary>
        /// Adds parameters for WF_Approval_Level_Dimension insert/update operations.
        /// </summary>
        private void AddApprovalLevelDimensionParameters(SqlCommand cmd, bool isUpdate = false)
        {
            if (isUpdate)
            {
                cmd.Parameters.Add("@Approval_Level_Dimension_ID", SqlDbType.Int).SourceColumn = "Approval_Level_Dimension_ID";
            }
            cmd.Parameters.Add("@Step_Config_ID", SqlDbType.Int).SourceColumn = "Step_Config_ID";
            cmd.Parameters.Add("@Data_Source_Config_ID", SqlDbType.Int).SourceColumn = "Data_Source_Config_ID";
            cmd.Parameters.Add("@Dimension_Name", SqlDbType.NVarChar, 255).SourceColumn = "Dimension_Name";
            cmd.Parameters.Add("@Hierarchy_Name", SqlDbType.NVarChar, 255).SourceColumn = "Hierarchy_Name";
            cmd.Parameters.Add("@Level_Name", SqlDbType.NVarChar, 255).SourceColumn = "Level_Name";
            cmd.Parameters.Add("@Member_Filter", SqlDbType.NVarChar, -1).SourceColumn = "Member_Filter";
            cmd.Parameters.Add("@Property_Name", SqlDbType.NVarChar, 255).SourceColumn = "Property_Name";
            cmd.Parameters.Add("@Level_Order", SqlDbType.Int).SourceColumn = "Level_Order";
            cmd.Parameters.Add("@Aggregate_To_Level", SqlDbType.NVarChar, 255).SourceColumn = "Aggregate_To_Level";
            cmd.Parameters.Add("@Require_All_Descendants", SqlDbType.Bit).SourceColumn = "Require_All_Descendants";
            cmd.Parameters.Add("@Is_Active", SqlDbType.Bit).SourceColumn = "Is_Active";
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).SourceColumn = "Description";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User";
            
            if (!isUpdate)
            {
                cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 100).SourceColumn = "Create_User";
            }
        }

        #endregion
    }
}
