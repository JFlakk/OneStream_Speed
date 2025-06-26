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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DDM_DB_DataSets
{
    /// <summary>
    /// MainClass provides data retrieval methods for Dynamic Dashboard Manager datasets.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        #endregion

        /// <summary>
        /// Main entry point for the Dashboard DataSet business rule.
        /// Handles requests for dataset names and data retrieval based on function type and dataset name.
        /// </summary>
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        // Return available dataset names if needed
                        // var names = new List<string> { "MyDataSet" };
                        // return names;
                        break;
                    case DashboardDataSetFunctionType.GetDataSet:
                        // Return WF Root Profiles
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_Root_WFProfiles"))
                        {
                            return Get_Root_WFProfiles();
                        }
                        // Return WF Profile Hierarchy for selected root profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_TreeView"))
                        {
                            return Get_WFProfile_TreeView();
                        }
                        // Return WF Profile Menu Options for selected profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_Menu_Options"))
                        {
                            return Get_WFProfile_Menu_Options();
                        }
                        // Return WF Profile Name/ID List for lookup in Table Editor
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_Name_ID"))
                        {
                            return Get_WFProfile_Name_ID();
                        }
                        // Return WF Profile Header Items for selected profile/menu option
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WFProfile_Header_Items"))
                        {
                            return Get_WFProfile_Header_Items();
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Get WF Profile Data"
        /// <summary>
        /// Retrieves the root workflow profiles (HierarchyLevel = 1, not templates).
        /// </summary>
        private DataTable Get_Root_WFProfiles()
        {
            try
            {
                // Define the SQL Statement
                var sql = @"
                    SELECT ProfileName
                    FROM WorkflowProfileHierarchy
                    WHERE HierarchyLevel = 1
                        AND IsTemplate = 0 ";

                // Return the DataTable
                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, sql, false);
                    dt.TableName = "Root_WFProfiles";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves the workflow profile hierarchy as a tree view for the selected root profile.
        /// </summary>
        private DataSet Get_WFProfile_TreeView()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                string rootProfileName = args.CustomSubstVars.XFGetValue("BL_DDM_Root_WFProfiles");

                var dt = new DataTable();

                // Define the SQL Statement
                var sql = @"
                    WITH RecursiveCTE AS (
                        SELECT 
                            prof.ProfileKey,
                            prof.ProfileName, 
                            CAST('00000000-0000-0000-0000-000000000000' AS uniqueidentifier) AS ParentProfileKey, 
                            prof.HierarchyLevel, 
                            prof.HierarchyIndex
                        FROM 
                            WorkflowProfileHierarchy prof
                        WHERE 
                            prof.HierarchyLevel = 1
                            AND prof.IsTemplate = 0
                            AND prof.ProfileName = @rootprofilename 
                        UNION ALL
                        SELECT 
                            prof.ProfileKey,
                            prof.ProfileName,
                            prof.ParentProfileKey, 
                            prof.HierarchyLevel, 
                            prof.HierarchyIndex
                        FROM 
                            WorkflowProfileHierarchy prof
                        INNER JOIN 
                            RecursiveCTE rcte ON prof.ParentProfileKey = rcte.ProfileKey
                    )
                    SELECT 
                        rcte.ProfileName, 
                        rcte.ProfileKey,
                        rcte.ParentProfileKey, 
                        parentProf.ProfileName as ParentProfileName, 
                        rcte.HierarchyLevel,
                        rcte.HierarchyIndex,
                        DDM.DDM_Profile_ID
                    FROM 
                        RecursiveCTE rcte
                    LEFT JOIN 
                        WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
                    LEFT JOIN
                        DDM_Config DDM ON DDM.DDM_Profile_Name = rcte.ProfileKey
                    ORDER BY 
                        rcte.HierarchyLevel DESC, 
                        rcte.HierarchyIndex";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                // Build the tree structure from the DataTable
                foreach (DataRow row in dt.Rows)
                {
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string profileName = row["ProfileName"].ToString();
                    string profileKey = row["ProfileKey"].ToString();
                    string parentprofileName = row["ParentProfileName"].ToString();
                    string parentprofileKey = row["ParentProfileKey"].ToString();
                    var Bold_WFProfile = true;
                    if (row["DDM_Profile_ID"] == DBNull.Value)
                    {
                        Bold_WFProfile = false;
                    }
                    parent_child.Add(profileKey, parentprofileKey);
                    var childProfiles = parent_child.Where(pair => pair.Value == profileKey)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {
                            // Create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.UniqueName == childProfile);
                            hierarchy_children_mbrs.Add(childXFTreeItem);
                        }
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WFProfile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                }

                var hierXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == rootProfileName);
                hierarchy.TreeItems.Add(hierXFTreeItem);
                return hierarchy.CreateDataSet(si);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves the menu options for a given workflow profile.
        /// </summary>
        private DataTable Get_WFProfile_Menu_Options()
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var WFProfile_Config_DT = new DataTable("WFProfile_Config");
                // Define the SQL Statement
                var sql = @"
                    Select Menu.DDM_Menu_ID, Menu.DDM_Menu_Name
                    FROM DDM_Config Cnfg
                    JOIN DDM_Config_Menu Menu
                    ON Cnfg.DDM_Profile_ID = Menu.DDM_Profile_ID
                    WHERE Cnfg.DDM_Profile_Name = @ProfileKey
                    ORDER BY DDM_Menu_Order";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, WFProfile_Config_DT, sql, sqlparams);
                }
                return WFProfile_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves a list of workflow profile names and IDs (not templates).
        /// </summary>
        private DataTable Get_WFProfile_Name_ID()
        {
            try
            {
                // Define the SQL Statement
                var sql = @"
                    SELECT ProfileName, ProfileKey
                    FROM WorkflowProfileHierarchy
                    WHERE IsTemplate = 0 ";

                // Return the DataTable
                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, sql, false);
                    dt.TableName = "WFProfile_Name_ID_List";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves header configuration options for a given workflow profile and menu option.
        /// </summary>
        private DataTable Get_WFProfile_Header_Items()
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var WFProfile_Header_Config_DT = new DataTable("WFProfile_Header_Items");
                string optionID = null;

                // Define the SQL Statement
                var sql = @"
                    SELECT *
                    FROM DDM_Profile_Config_Menu_Header_Options
                    WHERE DDM_Profile_ID = @ProfileKey
                        AND DDM_Profile_Menu_Option_ID = @OptionID";

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    // Add the parameter for ProfileKey and selected menu option 
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey },
                        new SqlParameter("@OptionID", SqlDbType.UniqueIdentifier) { Value = optionID }
                    };

                    sql_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, WFProfile_Header_Config_DT, sql, sqlparams);
                }
                return WFProfile_Header_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion
    }
}
