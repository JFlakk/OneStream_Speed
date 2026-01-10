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
    public class SQA_DDM_Config_Menu_Layout
    {
        private readonly SqlConnection _connection;

        public SQA_DDM_Config_Menu_Layout(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_DDM_Config_Menu_Layout_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string selectQuery, params SqlParameter[] parameters)
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

        public void Update_DDM_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                try
                {
                    // Build commands dynamically based on DataTable columns
                    GBL_SQA_Helper.BuildInsertCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu_Layout");
                    GBL_SQA_Helper.BuildUpdateCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu_Layout", "DDM_Menu_ID");
                    GBL_SQA_Helper.BuildDeleteCommand(sqa, _connection, transaction, dt, "DDM_Config_Menu_Layout", "DDM_Menu_ID");

                    sqa.Update(dt);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    sqa.InsertCommand = null;
                    sqa.UpdateCommand = null;
                    sqa.DeleteCommand = null;
                }
            }
        }

        /// <summary>
        /// Performs a MERGE operation (upsert) on DDM_Config_Menu_Layout table
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to merge</param>
        /// <param name="deleteUnmatched">If true, deletes records not in the source DataTable</param>
        /// <param name="deleteCondition">Optional SQL condition for conditional deletes (e.g., "Status = 'Inactive'")</param>
        public void Merge_DDM_Config_Menu_Layout(SessionInfo si, DataTable dt, bool deleteUnmatched = false, string deleteCondition = null)
        {
            GBL_SQA_Helper.MergeData(si, _connection, dt, "DDM_Config_Menu_Layout", "DDM_Menu_ID", deleteUnmatched, deleteCondition);
        }

        /// <summary>
        /// Synchronizes DDM_Config_Menu_Layout table with the DataTable (full sync with delete of unmatched records)
        /// </summary>
        /// <param name="si">SessionInfo</param>
        /// <param name="dt">DataTable containing data to sync</param>
        /// <param name="syncCondition">Optional SQL condition to limit which records can be deleted</param>
        public void Sync_DDM_Config_Menu_Layout(SessionInfo si, DataTable dt, string syncCondition = null)
        {
            GBL_SQA_Helper.SyncData(si, _connection, dt, "DDM_Config_Menu_Layout", "DDM_Menu_ID", syncCondition);
        }

        private void AddParameters(SqlCommand cmd)
        {
            // Identity/Keys
            cmd.Parameters.Add("@DDM_Config_ID", SqlDbType.Int).SourceColumn = "DDM_Config_ID";
            cmd.Parameters.Add("@DDM_Menu_ID", SqlDbType.Int).SourceColumn = "DDM_Menu_ID";
            
            // Core Config
            cmd.Parameters.Add("@DDM_Type", SqlDbType.Int).SourceColumn = "DDM_Type";
            cmd.Parameters.Add("@Scen_Type", SqlDbType.NVarChar, 20).SourceColumn = "Scen_Type";
            cmd.Parameters.Add("@Profile_Key", SqlDbType.UniqueIdentifier).SourceColumn = "Profile_Key";
            cmd.Parameters.Add("@Workspace_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Workspace_ID";
            cmd.Parameters.Add("@MaintUnit_ID", SqlDbType.UniqueIdentifier).SourceColumn = "MaintUnit_ID";
            cmd.Parameters.Add("@Sort_Order", SqlDbType.Int).SourceColumn = "Sort_Order";
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255).SourceColumn = "Name";
            
            // Option Types (Updated to Int per DDL)
            cmd.Parameters.Add("@Option_Type", SqlDbType.Int).SourceColumn = "Option_Type";
            cmd.Parameters.Add("@Top_Option_Type", SqlDbType.Int).SourceColumn = "Top_Option_Type";
            cmd.Parameters.Add("@TopLeft_Option_Type", SqlDbType.Int).SourceColumn = "TopLeft_Option_Type";
            cmd.Parameters.Add("@TopRight_Option_Type", SqlDbType.Int).SourceColumn = "TopRight_Option_Type";
            cmd.Parameters.Add("@Bottom_Option_Type", SqlDbType.Int).SourceColumn = "Bottom_Option_Type";
            cmd.Parameters.Add("@BottomLeft_Option_Type", SqlDbType.Int).SourceColumn = "BottomLeft_Option_Type";
            cmd.Parameters.Add("@BottomRight_Option_Type", SqlDbType.Int).SourceColumn = "BottomRight_Option_Type";
            cmd.Parameters.Add("@Left_Option_Type", SqlDbType.Int).SourceColumn = "Left_Option_Type";
            cmd.Parameters.Add("@Right_Option_Type", SqlDbType.Int).SourceColumn = "Right_Option_Type";

            // Layout Dimensions
            cmd.Parameters.Add("@Top_Height", SqlDbType.NVarChar, 255).SourceColumn = "Top_Height";
            cmd.Parameters.Add("@Left_Width", SqlDbType.NVarChar, 255).SourceColumn = "Left_Width";

            // Content
            cmd.Parameters.Add("@Custom_DB_Header", SqlDbType.NVarChar, 255).SourceColumn = "Custom_DB_Header";
            cmd.Parameters.Add("@Custom_DB_Content", SqlDbType.NVarChar, -1).SourceColumn = "Custom_DB_Content";
            cmd.Parameters.Add("@DB_Name", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name";
            cmd.Parameters.Add("@CV_Name", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name";
            
            // Quadrant Columns
            cmd.Parameters.Add("@DB_Name_Top", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_Top";
            cmd.Parameters.Add("@CV_Name_Top", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_Top";
            cmd.Parameters.Add("@DB_Name_Bottom", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_Bottom";
            cmd.Parameters.Add("@CV_Name_Bottom", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_Bottom";
            cmd.Parameters.Add("@DB_Name_TopLeft", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_TopLeft";
            cmd.Parameters.Add("@CV_Name_TopLeft", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_TopLeft";
            cmd.Parameters.Add("@DB_Name_TopRight", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_TopRight";
            cmd.Parameters.Add("@CV_Name_TopRight", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_TopRight";
            cmd.Parameters.Add("@DB_Name_BottomLeft", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_BottomLeft";
            cmd.Parameters.Add("@CV_Name_BottomLeft", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_BottomLeft";
            cmd.Parameters.Add("@DB_Name_BottomRight", SqlDbType.NVarChar, 255).SourceColumn = "DB_Name_BottomRight";
            cmd.Parameters.Add("@CV_Name_BottomRight", SqlDbType.NVarChar, 255).SourceColumn = "CV_Name_BottomRight";

            // Metadata
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
            cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
            cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
        }
    }
}