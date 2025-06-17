using System;
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
// 

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Data
{
    /// <summary>
    /// Main class for handling Dynamic Dashboard Manager configuration data operations.
    /// Provides methods for saving, updating, and validating dashboard menu options and header options.
    /// </summary>
    public class MainClass
    {
        #region "Global Variables"
        /// <summary>
        /// Stores the sort order for each menu header option by ID.
        /// </summary>
        public Dictionary<int, int> Global_Menu_Header_Options_Sort_Order_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the sort order for each menu option by ID.
        /// </summary>
        public Dictionary<int, int> Global_Menu_Options_Sort_Order_Dict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// Stores the name for each menu option by ID.
        /// </summary>
        public Dictionary<int, String> Global_Menu_Option_Name_Dict { get; set; } = new Dictionary<int, String>();

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
        public bool Duplicate_Menu_Header_Options_Sort_Order { get; set; } = false;

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
        /// <param name="si">Session information</param>
        /// <param name="globals">Global variables</param>
        /// <param name="api">API object</param>
        /// <param name="args">Dashboard extender arguments</param>
        /// <returns>Result object depending on the function type</returns>
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
                            // Implement SQL Table Editor Save Data logic here.
                            // Save the data rows.
                            // XFSqlTableEditorSaveDataTaskInfo saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
                            // using (DbConnInfo dbConn = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName))
                            // {
                            // dbConn.BeginTrans();
                            // BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, true, false, false);
                            // dbConn.CommitTrans();
                            // }

                            var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
                            saveDataTaskResult.IsOK = true;
                            saveDataTaskResult.ShowMessageBox = false;
                            saveDataTaskResult.Message = "";
                            saveDataTaskResult.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
                            return saveDataTaskResult;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Profile_Config_Menu_Options_Rows"))
                        {
                            save_Data_Task_Result = Save_DDM_Profile_Config_Menu_Options_Rows();
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Profile_Config_Menu_Header_Option_Rows"))
                        {
                            save_Data_Task_Result = Save_DDM_Profile_Config_Menu_Header_Option_Rows();
                        }
                        return save_Data_Task_Result;
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var selection_Changed_Task_Result = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Profile_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            selection_Changed_Task_Result = Save_New_Profile_Config(si, globals, api, args);
                            return selection_Changed_Task_Result;
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

        /// <summary>
        /// Saves menu options rows for a DDM profile configuration.
        /// Handles insert, update, and delete operations, and checks for duplicates.
        /// </summary>
        /// <returns>Result of the save operation</returns>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Profile_Config_Menu_Options_Rows()
        {
            try
            {

                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;

                int os_DDM_Profile_Menu_Option_ID = 0;

                int wfProfile_ID = Convert.ToInt32(args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Profile_ID", "0"));
                //int wfProfile_ID = Convert.ToInt32(args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("IV_DDM_Profile_ID","0"));
                Duplicate_Menu_Options_Check(wfProfile_ID, "Initiate");

                // Loops through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    // Logic applied to new record inserts
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        Duplicate_Menu_Options_Check(wfProfile_ID, "Update Row", "Insert", xfRow);
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            connection.Open();
                            var sqlMaxIdGetter = new SQL_DDM_Get_Max_ID(si, connection);

                            // Example: Get the max ID for the "MCM_Calc_Config" table
                            os_DDM_Profile_Menu_Option_ID = sqlMaxIdGetter.Get_Max_ID(si, "DDM_Profile_Config_Menu_Options", "DDM_Profile_Menu_Option_ID");
                        }
                        xfRow.ModifiedDataRow.SetValue("DDM_Profile_Menu_Option_ID", os_DDM_Profile_Menu_Option_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Status", "In Process", XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        Duplicate_Menu_Options_Check(wfProfile_ID, "Update Row", "Update", xfRow);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
                        //xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        //xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                }

                // Set return value
                save_Data_Task_Result.ShowMessageBox = true;
                //save_Data_Task_Result.Message = String.Empty;
                BRApi.ErrorLog.LogMessage(si, "Hit: " + Duplicate_Menu_Options + "-" + Duplicate_Menu_Options_Sort_Order);
                if (Duplicate_Menu_Options == true || Duplicate_Menu_Options_Sort_Order == true)
                {
                    save_Data_Task_Result.IsOK = false;
                    save_Data_Task_Result.Message = "Duplicate Sort Order and/or Duplicate Menu Options.";
                    save_Data_Task_Result.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
                }
                else
                {
                    save_Data_Task_Result.IsOK = true;
                    save_Data_Task_Result.Message = "Menu Options Successfully Saved.";
                    save_Data_Task_Result.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
                }
                return save_Data_Task_Result;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Saves general DDM profile configuration rows.
        /// Handles insert, update, and delete operations.
        /// </summary>
        /// <returns>Result of the save operation</returns>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Profile_Config_Rows()
        {
            try
            {

                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;

                int os_DDM_Profile_Menu_Option_ID = 0;

                // Loops through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    // Logic applied to new record inserts
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            connection.Open();
                            var sqlMaxIdGetter = new SQL_DDM_Get_Max_ID(si, connection);

                            // Example: Get the max ID for the "MCM_Calc_Config" table
                            os_DDM_Profile_Menu_Option_ID = sqlMaxIdGetter.Get_Max_ID(si, "DDM_Profile_Config_Menu_Options", "DDM_Profile_Menu_Option_ID");
                        }
                        if ((bool)xfRow.ModifiedDataRow.Items["DDM_Menu_Option_Custom_Dashboard"])
                        {

                        }
                        xfRow.ModifiedDataRow.SetValue("DDM_Profile_Menu_Option_ID", os_DDM_Profile_Menu_Option_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Status", "In Process", XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
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
                    save_Data_Task_Result.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
                }
                else
                {
                    save_Data_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.
                }
                return save_Data_Task_Result;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Saves menu header option rows for a DDM profile configuration.
        /// Handles insert, update, and delete operations.
        /// </summary>
        /// <returns>Result of the save operation</returns>
        private XFSqlTableEditorSaveDataTaskResult Save_DDM_Profile_Config_Menu_Header_Option_Rows()
        {
            try
            {

                var save_Data_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;

                int os_DDM_Profile_Menu_Header_Option_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqlMaxIdGetter = new SQL_DDM_Get_Max_ID(si, connection);

                    // Example: Get the max ID for the "MCM_Calc_Config" table
                    os_DDM_Profile_Menu_Header_Option_ID = sqlMaxIdGetter.Get_Max_ID(si, "DDM_Profile_Config_Menu_Header_Options", "DDM_Profile_Menu_Header_Option_ID");
                }


                // Loops through each row in the table editor that was added or updated prior to hitting save
                foreach (XFEditedDataRow xfRow in saveDataTaskInfo.EditedDataRows)
                {
                    // Logic applied to new record inserts
                    if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                    {
                        xfRow.ModifiedDataRow.SetValue("DDM_Profile_Menu_Header_Option_ID", os_DDM_Profile_Menu_Header_Option_ID, XFDataType.Int16);
                        xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);

                        os_DDM_Profile_Menu_Header_Option_ID++; // need to increment for each new row
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                    {
                        xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                    else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                    {
                        // can't do anything to deleted rows unless we specifically handle the save logic (CancelDefaultSave must be set to true)
                        //xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                        //xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                    }
                }

                // Set return value
                save_Data_Task_Result.IsOK = true;
                save_Data_Task_Result.ShowMessageBox = false;
                save_Data_Task_Result.Message = String.Empty;
                save_Data_Task_Result.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Data_Task_Result;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Checks for duplicate menu options and sort orders.
        /// Updates global dictionaries and sets duplicate flags.
        /// </summary>
        /// <param name="wfProfile_ID">Profile ID</param>
        /// <param name="Dup_Process_Step">Process step (e.g., "Initiate", "Update Row")</param>
        /// <param name="DDL_Process">DDL process type (optional)</param>
        /// <param name="Modified_Menu_Options_DataRow">Modified data row (optional)</param>
        private void Duplicate_Menu_Options_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_Menu_Options_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sql_DataSet_DataAdapter = new SqlDataAdapter();
                            var DDM_profile_menu_options_DT = new DataTable();
                            // Define the select query and parameters
                            string select_DS_Query = @"
						        					SELECT *
						       						FROM DDM_Profile_Config_Menu_Options
						       						WHERE DDM_Profile_ID = @DDM_Profile_ID";
                            // Create an array of SqlParameter objects
                            var ds_parameters = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sql_DataSet_DataAdapter, DDM_profile_menu_options_DT, select_DS_Query, ds_parameters);

                            foreach (DataRow menu_option_Row in DDM_profile_menu_options_DT.Rows)
                            {
                                int menu_optionID = (int)menu_option_Row["DDM_Profile_Menu_Option_ID"];
                                int sortOrder = (int)menu_option_Row["DDM_Menu_Option_Sort_Order"];
                                string menuOptionName = (string)menu_option_Row["DDM_Menu_Option_Name"];

                                Global_Menu_Options_Sort_Order_Dict.Add(menu_optionID, sortOrder);
                                Global_Menu_Option_Name_Dict.Add(menu_optionID, menuOptionName);
                            }
                            BRApi.ErrorLog.LogMessage(si, "Count " + Global_Menu_Options_Sort_Order_Dict.Count);
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Profile_Menu_Option_ID"];
                        int sortOrder = (int)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Sort_Order"];
                        string menuOptionName = (string)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Name"];

                        Global_Menu_Options_Sort_Order_Dict.Add(menuOptionID, sortOrder);
                        Global_Menu_Option_Name_Dict.Add(menuOptionID, menuOptionName);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Profile_Menu_Option_ID"];
                        int orig_sortOrder = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Menu_Option_Sort_Order"];
                        int new_sortOrder = (int)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Sort_Order"];
                        string orig_menuOptionName = (string)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Menu_Option_Name"];
                        string new_menuOptionName = (string)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Name"];
                        BRApi.ErrorLog.LogMessage(si, "Hit: " + orig_sortOrder + "-" + new_sortOrder);

                        if (orig_sortOrder != new_sortOrder)
                        {
                            Global_Menu_Options_Sort_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menuOptionName != new_menuOptionName)
                        {
                            Global_Menu_Option_Name_Dict.XFSetValue(menuOptionID, new_menuOptionName);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_Option_SortOrders = Global_Menu_Options_Sort_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in Global_Menu_Options_Sort_Order_Dict)
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Dist List: " + kvp.Value);
            }
            BRApi.ErrorLog.LogMessage(si, "Hit: " + dup_Menu_Option_SortOrders.Count);
            if (dup_Menu_Option_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_Sort_Order = true;
            }
            else
            {
                Duplicate_Menu_Options_Sort_Order = false;
            }
            var dup_Menu_Options = Global_Menu_Option_Name_Dict.GroupBy(x => x.Value)
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
        /// Checks for duplicate menu header options and sort orders.
        /// Updates global dictionaries and sets duplicate flags.
        /// </summary>
        /// <param name="wfProfile_ID">Profile ID</param>
        /// <param name="Dup_Process_Step">Process step (e.g., "Initiate", "Update Row")</param>
        /// <param name="DDL_Process">DDL process type (optional)</param>
        /// <param name="Modified_Menu_Options_DataRow">Modified data row (optional)</param>
        private void Duplicate_Menu_Header_Options_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_Menu_Options_DataRow)
        {
            switch (Dup_Process_Step)
            {
                case "Initiate":
                    // Select rows from the table before any updates to rows are processed
                    {
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sql_DataSet_DataAdapter = new SqlDataAdapter();
                            var DDM_profile_menu_options_DT = new DataTable();
                            // Define the select query and parameters
                            string select_DS_Query = @"
						        					SELECT *
						       						FROM DDM_Profile_Config_Menu_Options
						       						WHERE DDM_Profile_ID = @DDM_Profile_ID";
                            // Create an array of SqlParameter objects
                            var ds_parameters = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sql_DataSet_DataAdapter, DDM_profile_menu_options_DT, select_DS_Query, ds_parameters);

                            foreach (DataRow menu_option_Row in DDM_profile_menu_options_DT.Rows)
                            {
                                int menu_optionID = (int)menu_option_Row["DDM_Profile_Menu_Option_ID"];
                                int sortOrder = (int)menu_option_Row["DDM_Menu_Option_Sort_Order"];
                                string menuOptionName = (string)menu_option_Row["DDM_Menu_Option_Name"];

                                Global_Menu_Options_Sort_Order_Dict.Add(menu_optionID, sortOrder);
                                Global_Menu_Option_Name_Dict.Add(menu_optionID, menuOptionName);
                            }
                            BRApi.ErrorLog.LogMessage(si, "Count " + Global_Menu_Options_Sort_Order_Dict.Count);
                        }
                    }
                    break;
                case "Update Row":
                    // Check for duplicate menu options when the process step is "Initiate"
                    if (DDL_Process == "Insert")
                    {
                        int menuOptionID = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Profile_Menu_Option_ID"];
                        int sortOrder = (int)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Sort_Order"];
                        string menuOptionName = (string)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Name"];

                        Global_Menu_Options_Sort_Order_Dict.Add(menuOptionID, sortOrder);
                        Global_Menu_Option_Name_Dict.Add(menuOptionID, menuOptionName);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Profile_Menu_Option_ID"];
                        int orig_sortOrder = (int)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Menu_Option_Sort_Order"];
                        int new_sortOrder = (int)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Sort_Order"];
                        string orig_menuOptionName = (string)Modified_Menu_Options_DataRow.OriginalDataRow["DDM_Menu_Option_Name"];
                        string new_menuOptionName = (string)Modified_Menu_Options_DataRow.ModifiedDataRow["DDM_Menu_Option_Name"];
                        BRApi.ErrorLog.LogMessage(si, "Hit: " + orig_sortOrder + "-" + new_sortOrder);

                        if (orig_sortOrder != new_sortOrder)
                        {
                            Global_Menu_Options_Sort_Order_Dict.XFSetValue(menuOptionID, new_sortOrder);
                        }
                        if (orig_menuOptionName != new_menuOptionName)
                        {
                            Global_Menu_Option_Name_Dict.XFSetValue(menuOptionID, new_menuOptionName);
                        }
                    }
                    else if (DDL_Process == "Delete")
                    {
                        // TODO: Implement logic to check for duplicate menu options based on DDL_Process value
                    }
                    break;
            }
            var dup_Menu_Option_SortOrders = Global_Menu_Options_Sort_Order_Dict
                                                         .GroupBy(x => x.Value)
                                                         .Where(g => g.Count() > 1)
                                                         .Select(g => g.Key)
                                                         .ToList();
            foreach (var kvp in Global_Menu_Options_Sort_Order_Dict)
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Dist List: " + kvp.Value);
            }
            BRApi.ErrorLog.LogMessage(si, "Hit: " + dup_Menu_Option_SortOrders.Count);
            if (dup_Menu_Option_SortOrders.Count > 0)
            {
                Duplicate_Menu_Options_Sort_Order = true;
            }
            else
            {
                Duplicate_Menu_Options_Sort_Order = false;
            }
            var dup_Menu_Options = Global_Menu_Option_Name_Dict.GroupBy(x => x.Value)
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
        /// Handles saving a new profile configuration when a dashboard component selection changes.
        /// Inserts a new row into the DDM_Profile_Config table if needed.
        /// </summary>
        /// <param name="si">Session information</param>
        /// <param name="globals">Global variables</param>
        /// <param name="api">API object</param>
        /// <param name="args">Dashboard extender arguments</param>
        /// <returns>Result of the selection change operation</returns>
        private XFSelectionChangedTaskResult Save_New_Profile_Config(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var wf_Profile_ID = Guid.Parse(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_trv_Root_WF_Profile_Detail", Guid.Empty.ToString()));
                int new_wf_Profile_Config_ID = 0;
                if (new_wf_Profile_Config_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sqlMaxIdGetter = new SQL_DDM_Get_Max_ID(si, connection);
                        var sql_DDM_Get_DataSets = new SQL_DDM_Get_DataSets(si, connection);
                        connection.Open();

                        // Create a new DataTable
                        var sql_DS_DataAdapter = new SqlDataAdapter();
                        var DDM_Profile_Config_Count_DT = new DataTable();
                        // Define the select query and parameters
                        string select_DS_Query = @"
				        					SELECT Count(*) as Count
				       						FROM DDM_Profile_Config
				       						WHERE DDM_Profile_Name = @wf_Profile_ID";

                        // Create an array of SqlParameter objects
                        var DDM_ds_parameters = new SqlParameter[]
                        {
                        new SqlParameter("@wf_Profile_ID", SqlDbType.UniqueIdentifier) { Value = wf_Profile_ID }
                        };

                        sql_DDM_Get_DataSets.Fill_Get_DDM_DataTable(si, sql_DS_DataAdapter, DDM_Profile_Config_Count_DT, select_DS_Query, DDM_ds_parameters);

                        if (Convert.ToInt32(DDM_Profile_Config_Count_DT.Rows[0]["Count"]) > 0)
                            BRApi.ErrorLog.LogMessage(si, "Hit Max: " + new_wf_Profile_Config_ID);
                        // Example: Get the max ID for the "MCM_Calc_Config" table
                        new_wf_Profile_Config_ID = sqlMaxIdGetter.Get_Max_ID(si, "DDM_Profile_Config", "DDM_Profile_ID");
                        BRApi.ErrorLog.LogMessage(si, "Hit Max: " + new_wf_Profile_Config_ID);
                        var sqlAdapterDDMModels = new SQL_Adapter_DDM_Profile_Config(si, connection);
                        var DDM_Profile_Config_DT = new DataTable();
                        var sql_DDM_DataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from MCM_Dest_Cell
                        string select_DDM_Query = @"
												SELECT * 
												FROM DDM_Profile_Config 
												WHERE DDM_Profile_ID = @DDM_Profile_ID";

                        // Create an array of SqlParameter objects
                        var DDM_parameters = new SqlParameter[]
                        {
                            new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = new_wf_Profile_Config_ID}
                        };

                        sqlAdapterDDMModels.Fill_DDM_Profile_Config_DataTable(si, sql_DDM_DataAdapter, DDM_Profile_Config_DT, select_DDM_Query, DDM_parameters);

                        DataRow newRow = DDM_Profile_Config_DT.NewRow();
                        newRow["DDM_Profile_ID"] = new_wf_Profile_Config_ID;
                        newRow["DDM_Profile_Name"] = wf_Profile_ID;
                        newRow["DDM_Profile_Step_Type"] = "Import";
                        newRow["Status"] = "In Process";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        DDM_Profile_Config_DT.Rows.Add(newRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        //Update the MCM_Dest_Cell table based on the changes made to the DataTable
                        sqlAdapterDDMModels.Update_DDM_Profile_Config(si, DDM_Profile_Config_DT, sql_DDM_DataAdapter);
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}