﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
using OpenXmlPowerTools;
using Workspace.OSConsultantTools.GBL_UI_Assembly;


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Data
{
    /// <summary>
    /// MainClass handles Dynamic Dashboard Manager configuration data operations.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        /// <summary>
        /// Stores the sort order for each menu header option by ID.
        /// </summary>
        public Dictionary<int, int> GBL_Menu_Hdr_Options_Sort_Order_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the sort order for each menu option by ID.
        /// </summary>
        public Dictionary<int, int> GBL_Menu_Order_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the name for each menu option by ID.
        /// </summary>
        public Dictionary<int, String> GBL_Menu_Name_Dict { get; set; } = new Dictionary<int, String>();

        /// <summary>
        /// Indicates if duplicate menu options exist.
        /// </summary>
        public bool Duplicate_Menu_Options { get; set; } = false;

        /// <summary>
        /// Indicates if duplicate menu option sort orders exist.
        /// </summary>
        public bool Duplicate_Menu_Options_Sort_Order { get; set; } = false;

        /// <summary>
        /// Indicates if duplicate menu header option sort orders exist.
        /// </summary>
        public bool Duplicate_Menu_Hdr_Options_Sort_Order { get; set; } = false;

        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        #endregion

        /// <summary>
        /// Main entry point for the Dashboard Extender business rule.
        /// Handles different function types such as saving data or handling component selection changes.
        /// </summary>
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.SqlTableEditorSaveData:
                        var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_Profile_Config_Rows"))
                        {
                            // Save the data rows for profile config.
                            var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
                            saveDataTaskResult.IsOK = true;
                            saveDataTaskResult.ShowMessageBox = false;
                            saveDataTaskResult.Message = "";
                            saveDataTaskResult.CancelDefaultSave = false;
                            return saveDataTaskResult;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Config_Menu_Rows"))
                        {
                            // Save menu rows for DDM config.
                            save_Data_Task_Result = Save_DDM_Config_Menu_Rows();
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Config_Menu_Hdr_Rows"))
                        {
                            // Save menu header rows for DDM config.
                            save_Data_Task_Result = Save_DDM_Config_Menu_Hdr_Rows();
                        }
                        return save_Data_Task_Result;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var selection_Changed_Task_Result = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Profile_Config"))
                        {
                            // Handle saving a new profile config when component selection changes.
                            selection_Changed_Task_Result = Save_New_Profile_Config();
                            return selection_Changed_Task_Result;
                        }
                        break;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Saves DDM Profile Config Menu Options Rows.
        /// Handles insert, update, and duplicate checks for menu options.
        /// </summary>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Config_Menu_Rows()
        {
            try
            {
                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                int os_DDM_Menu_ID = 0;
                int wfProfile_ID = Convert.ToInt32(args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Profile_ID", "0"));

                // Check for duplicates before processing rows.
                Duplicate_Menu_Check(wfProfile_ID, "Initiate");

                // Loop through each edited row and process accordingly.
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        // Handle insert logic for new menu option.
                        Duplicate_Menu_Check(wfProfile_ID, "Update Row", "Insert", xfRow);
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            connection.Open();
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            os_DDM_Menu_ID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Menu", "DDM_Menu_ID");
                        }
                        xfRow.ModifiedDataRow.SetValue("DDM_Menu_ID", os_DDM_Menu_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Status", "In Process", XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        // Handle update logic for existing menu option.
                        Duplicate_Menu_Check(wfProfile_ID, "Update Row", "Update", xfRow);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    // No action for delete in this method.
                }

                // Set result based on duplicate checks.
                save_Data_Task_Result.ShowMessageBox = true;
                if (Duplicate_Menu_Options == true || Duplicate_Menu_Options_Sort_Order == true)
                {
                    save_Data_Task_Result.IsOK = false;
                    save_Data_Task_Result.Message = "Duplicate Sort Order and/or Duplicate Menu Options.";
                    save_Data_Task_Result.CancelDefaultSave = false;
                }
                else
                {
                    save_Data_Task_Result.IsOK = true;
                    save_Data_Task_Result.Message = "Menu Options Successfully Saved.";
                    save_Data_Task_Result.CancelDefaultSave = false;
                }
                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Saves DDM Profile Config Menu Options Rows.
        /// This method specifically handles the saving of profile config rows.
        /// </summary>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Profile_Config_Rows()
        {
            try
            {
                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                int os_DDM_Menu_ID = 0;

                // Loop through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        // Handle insert logic for new profile config row.
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            connection.Open();
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            os_DDM_Menu_ID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Menu", "DDM_Menu_ID");
                        }
                        xfRow.ModifiedDataRow.SetValue("DDM_Menu_ID", os_DDM_Menu_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Status", "In Process", XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        // Handle update logic for existing profile config row.
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
                        // Handle delete logic for profile config row if necessary.
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                }

                // Set return value
                save_Data_Task_Result.IsOK = true;
                save_Data_Task_Result.ShowMessageBox = false;
                save_Data_Task_Result.Message = String.Empty;
                if (Duplicate_Menu_Options == true || Duplicate_Menu_Options_Sort_Order == true)
                {
                    save_Data_Task_Result.CancelDefaultSave = false;
                }
                else
                {
                    save_Data_Task_Result.CancelDefaultSave = true;
                }
                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Saves DDM Profile Config Menu Header Rows.
        /// This method handles the saving of menu header rows for DDM config.
        /// </summary>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Config_Menu_Hdr_Rows()
        {
            try
            {
                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                int os_DDM_Menu_Hdr_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    os_DDM_Menu_Hdr_ID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Menu_Hdr", "DDM_Menu_Hdr_ID");
                }

                // Loops through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        // Handle insert logic for new menu header row.
                        xfRow.ModifiedDataRow.SetValue("DDM_Menu_Hdr_ID", os_DDM_Menu_Hdr_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);

                        os_DDM_Menu_Hdr_ID++; // Increment for each new row
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        // Handle update logic for existing menu header row.
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
                        // Handle delete logic for menu header row if necessary.
                        //xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        //xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                }

                // Set return value
                save_Data_Task_Result.IsOK = true;
                save_Data_Task_Result.ShowMessageBox = false;
                save_Data_Task_Result.Message = String.Empty;
                save_Data_Task_Result.CancelDefaultSave = false;

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Duplicate Menu Check"
		/// <summary>
        /// Checks for duplicate menu options and sort orders.
        /// This method is used to identify duplicates during the save process.
        /// </summary>
        private void Duplicate_Menu_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Config_Menu_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var DDM_Config_Menu_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
						        					SELECT *
						       						FROM DDM_Config_Menu
						       						WHERE DDM_Profile_ID = @DDM_Profile_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Menu_DT, sql, sqlparams);

                            foreach (DataRow menu_Row in DDM_Config_Menu_DT.Rows)
                            {
                                int menu_ID = (int)menu_Row["DDM_Menu_ID"];
                                int sortOrder = (int)menu_Row["Order"];
                                string menu_Name = (string)menu_Row["Name"];

                                GBL_Menu_Order_Dict.Add(menu_ID, sortOrder);
                                GBL_Menu_Name_Dict.Add(menu_ID, menu_Name);
                            }
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_Menu_ID"];
                        int sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Order"];
                        string menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        GBL_Menu_Order_Dict.Add(menuOptionID, sortOrder);
                        GBL_Menu_Name_Dict.Add(menuOptionID, menu_Name);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_Menu_ID"];
                        int orig_sortOrder = (int)Config_Menu_DataRow.OriginalDataRow["Order"];
                        int new_sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Order"];
                        string orig_menu_Name = (string)Config_Menu_DataRow.OriginalDataRow["Name"];
                        string new_menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        if (orig_sortOrder != new_sortOrder)
                        {
                            GBL_Menu_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menu_Name != new_menu_Name)
                        {
                            GBL_Menu_Name_Dict.XFSetValue(menuOptionID, new_menu_Name);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_SortOrders = GBL_Menu_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in GBL_Menu_Order_Dict)
            {

            }
            if (dup_Menu_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_Sort_Order = true;
            }
            else
            {
                Duplicate_Menu_Options_Sort_Order = false;
            }
            var dup_Menu_Options = GBL_Menu_Name_Dict.GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            if (dup_Menu_Options.Count > 0)
            {
                Duplicate_Menu_Options = true;
            }
            else
            {
                Duplicate_Menu_Options = false;
            }
        }
		#endregion

        /// <summary>
        /// Checks for duplicate menu header options and sort orders.
        /// This method is used to identify duplicates for menu header options during the save process.
        /// </summary>
        private void Duplicate_Menu_Hdr_Options_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Config_Menu_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var DDM_Config_Menu_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
						        					SELECT *
						       						FROM DDM_Config_Menu
						       						WHERE DDM_Profile_ID = @DDM_Profile_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Menu_DT, sql, sqlparams);

                            foreach (DataRow menu_Row in DDM_Config_Menu_DT.Rows)
                            {
                                int menu_ID = (int)menu_Row["DDM_Menu_ID"];
                                int sortOrder = (int)menu_Row["Order"];
                                string menu_Name = (string)menu_Row["Name"];

                                GBL_Menu_Order_Dict.Add(menu_ID, sortOrder);
                                GBL_Menu_Name_Dict.Add(menu_ID, menu_Name);
                            }
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_Menu_ID"];
                        int sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["DDM_Menu_Order"];
                        string menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        GBL_Menu_Order_Dict.Add(menuOptionID, sortOrder);
                        GBL_Menu_Name_Dict.Add(menuOptionID, menu_Name);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_Menu_ID"];
                        int orig_sortOrder = (int)Config_Menu_DataRow.OriginalDataRow["Order"];
                        int new_sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Order"];
                        string orig_menu_Name = (string)Config_Menu_DataRow.OriginalDataRow["Name"];
                        string new_menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        if (orig_sortOrder != new_sortOrder)
                        {
                            GBL_Menu_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menu_Name != new_menu_Name)
                        {
                            GBL_Menu_Name_Dict.XFSetValue(menuOptionID, new_menu_Name);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_SortOrders = GBL_Menu_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in GBL_Menu_Order_Dict)
            {

            }

            if (dup_Menu_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_Sort_Order = true;
            }
            else
            {
                Duplicate_Menu_Options_Sort_Order = false;
            }
            var dup_Menu_Options = GBL_Menu_Name_Dict.GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            if (dup_Menu_Options.Count > 0)
            {
                Duplicate_Menu_Options = true;
            }
            else
            {
                Duplicate_Menu_Options = false;
            }
        }
		
        /// <summary>
        /// Saves a new profile configuration.
        /// This method is called when the component selection changes to save a new profile config.
        /// </summary>
        private XFSelectionChangedTaskResult Save_New_Profile_Config()
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
				
                var wf_Profile_ID = Guid.Parse(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_trv_WF_Profile", Guid.Empty.ToString()));

                int new_Profile_Config_ID = 0;
                if (new_Profile_Config_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        var DDM_Config_Count_DT = new DataTable();
                        // Define the select query and parameters
                        var sql = @"
                                            SELECT Count(*) as Count
                                            FROM DDM_Config
                                            WHERE ProfileKey = @wf_Profile_ID";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@wf_Profile_ID", SqlDbType.UniqueIdentifier) { Value = wf_Profile_ID }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Count_DT, sql, sqlparams);

                        if (Convert.ToInt32(DDM_Config_Count_DT.Rows[0]["Count"]) > 0)

                        // Example: Get the max ID for the "MCM_Calc_Config" table
                        new_Profile_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config", "DDM_Profile_ID");
						BRApi.ErrorLog.LogMessage(si,$"Hit {new_Profile_Config_ID}");
                        var sqa_ddm_config = new SQA_DDM_Config(si, connection);
                        var DDM_Config_DT = new DataTable();

                        // Fill the DataTable with the current data from MCM_Dest_Cell
                        sql = @"
									SELECT * 
									FROM DDM_Config 
									WHERE DDM_Profile_ID = @DDM_Profile_ID";

                        // Update the value of the existing sqlparams array
						sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = new_Profile_Config_ID }
                        };
						
                        sqa_ddm_config.Fill_DDM_Config_DT(si, sqa, DDM_Config_DT, sql, sqlparams);



                        // Query WorkflowProfileHierarchy for the given wf_Profile_ID
                        var wfProfile_Step_Type_DT = new DataTable();
                        sql = @"
                            SELECT *
                            FROM WorkflowProfileHierarchy
                            WHERE ProfileKey = @ProfileKey";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wf_Profile_ID }
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wfProfile_Step_Type_DT, sql, sqlparams);

                        DataRow newRow = DDM_Config_DT.NewRow();
                        newRow["DDM_Profile_ID"] = new_Profile_Config_ID;
                        newRow["ProfileKey"] = wf_Profile_ID;
                        // Determine Profile_Step_Type based on wfProfile_Step_Type_DT
                        if (wfProfile_Step_Type_DT.Rows.Count > 0)
                        {
                            newRow["Profile_Step_Type"] = wfProfile_Step_Type_DT.Rows[0]["ProfileType"];
                        }
                        newRow["Status"] = "In Process";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        DDM_Config_DT.Rows.Add(newRow);

                        //Update the MCM_Dest_Cell table based on the changes made to the DataTable
                        sqa_ddm_config.Update_DDM_Config(si, DDM_Config_DT, sqa);
                    }
                }

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}