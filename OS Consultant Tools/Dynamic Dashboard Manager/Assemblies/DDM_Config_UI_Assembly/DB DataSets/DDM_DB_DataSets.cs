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
    /// Main entry class for Dynamic Dashboard Manager DataSets.
    /// </summary>
    public class MainClass
    {
        /// <summary>
        /// Main entry point for Dashboard DataSet rule.
        /// </summary>
        /// <param name="si">Session information.</param>
        /// <param name="globals">Business rule globals.</param>
        /// <param name="api">API object.</param>
        /// <param name="args">Dashboard dataset arguments.</param>
        /// <returns>Object result, typically a DataTable or DataSet.</returns>
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
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
                        // Return available dataset names
                        // var names = new List<string>();
                        // names.Add("MyDataSet");
                        // return names;
                        break;

                    case DashboardDataSetFunctionType.GetDataSet:
                        // Return WF Root Profiles
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_Root_WF_Profiles"))
                        {
                            return Get_Root_WF_Profiles();
                        }
                        // Return WF Profile Hierarchy for selected root profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WF_Profile_TreeView"))
                        {
                            return Get_WF_Profile_Hierarchy();
                        }
                        // Return WF Profile Menu Options for selected profile
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WF_Profile_Menu_Options"))
                        {
                            return Get_WF_Profile_Menu_Options(si, globals, api);
                        }
                        // Return WF Profile Name/ID List for lookup
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WF_Profile_Name_ID"))
                        {
                            return Get_WF_Profile_Name_ID(si, globals, api);
                        }
                        // Return WF Profile Header Items
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_WF_Profile_Header_Items"))
                        {
                            return Get_WF_Profile_Header_Items(si, globals, api);
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

        #region "Get WF Profile Data"

        /// <summary>
        /// Retrieves the root workflow profiles (HierarchyLevel = 1, not templates).
        /// </summary>
        private DataTable Get_Root_WF_Profiles()
        {
            try
            {
                var Select_sql = @"
                    SELECT ProfileName
                    FROM WorkflowProfileHierarchy
                    WHERE HierarchyLevel = 1
                        AND IsTemplate = 0 ";

                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, Select_sql, false);
                    dt.TableName = "Root_WF_Profiles";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves the workflow profile hierarchy as a tree structure for a given root profile.
        /// </summary>
        private DataSet Get_WF_Profile_Hierarchy()
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Here");
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                string rootProfileName = CustomSubstVars.XFGetValue("BL_DDM_Root_WF_Profiles");

                var dt = new DataTable();

                var select_sql = @"
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
                        WFM.DDM_Profile_ID
                    FROM 
                        RecursiveCTE rcte
                    LEFT JOIN 
                        WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
                    LEFT JOIN
                        DDM_Profile_Config WFM ON WFM.DDM_Profile_Name = rcte.ProfileKey
                    ORDER BY 
                        rcte.HierarchyLevel DESC, 
                        rcte.HierarchyIndex";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                    var sqlDataAdapter = new SqlDataAdapter();

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, dt, select_sql, parameters);
                }

                foreach (DataRow row in dt.Rows)
                {
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string profileName = row["ProfileName"].ToString();
                    string profileKey = row["ProfileKey"].ToString();
                    string parentprofileName = row["ParentProfileName"].ToString();
                    string parentprofileKey = row["ParentProfileKey"].ToString();
                    var Bold_WF_Profile = row["DDM_Profile_ID"] != DBNull.Value;
                    parent_child.Add(profileKey, parentprofileKey);

                    var childProfiles = parent_child.Where(pair => pair.Value == profileKey)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.UniqueName == childProfile);
                            hierarchy_children_mbrs.Add(childXFTreeItem);
                        }
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WF_Profile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(profileKey, profileName, string.Empty, Bold_WF_Profile, true, true, true, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.TrueValue);
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
        /// Retrieves menu options for the current workflow profile.
        /// </summary>
        private DataTable Get_WF_Profile_Menu_Options()
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var wf_Profile_Config_DT = new DataTable("wf_Profile_Config");

                var select_sql = @"
                    Select Menu.DDM_Profile_Menu_Option_ID, Menu.DDM_Menu_Option_Name
                    FROM DDM_Profile_Config Cnfg
                    JOIN DDM_Profile_Config_Menu_Options Menu
                        ON Cnfg.DDM_Profile_ID = Menu.DDM_Profile_ID
                    WHERE Cnfg.DDM_Profile_Name = @ProfileKey
                    ORDER BY DDM_Menu_Option_Sort_Order";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                    var sqlDataAdapter = new SqlDataAdapter();

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey }
                    };

                    sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, wf_Profile_Config_DT, select_sql, parameters);
                }
                return wf_Profile_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves a list of workflow profile names and IDs (not templates).
        /// </summary>
        private DataTable Get_WF_Profile_Name_ID(SessionInfo si, BRGlobals globals, object api)
        {
            try
            {
                var Select_sql = @"
                    SELECT ProfileName, ProfileKey
                    FROM WorkflowProfileHierarchy
                    WHERE IsTemplate = 0 ";

                using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnApp, Select_sql, false);
                    dt.TableName = "WF_Profile_Name_ID_List";
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Retrieves header configuration options for the current workflow profile and menu option.
        /// </summary>
        private DataTable Get_WF_Profile_Header_Items(SessionInfo si, BRGlobals globals, object api)
        {
            try
            {
                var wfUserPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                var wf_Profile_Header_Config_DT = new DataTable("wf_Profile_Header_Items");
                string optionID = null;

                var select_sql = @"
                    SELECT *
                    FROM DDM_Profile_Config_Menu_Header_Options
                    WHERE DDM_Profile_ID = @ProfileKey
                        AND DDM_Profile_Menu_Option_ID = @OptionID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                    var sqlDataAdapter = new SqlDataAdapter();

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wfUserPk.ProfileKey },
                        new SqlParameter("@OptionID", SqlDbType.UniqueIdentifier) { Value = optionID }
                    };

                    sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sqlDataAdapter, wf_Profile_Header_Config_DT, select_sql, parameters);
                }
                return wf_Profile_Header_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion
    }
}