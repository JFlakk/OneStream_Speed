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
using Workspace.OSConsTools.GBL_UI_Assembly;


namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Data
{
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
						break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var selection_Changed_Task_Result = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Profile_Config"))
                        {
                            // Handle saving a new profile config when component selection changes.
                            selection_Changed_Task_Result = Save_New_Profile_Config();
                            return selection_Changed_Task_Result;
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_Add_DDM_Config_Menu_Layout"))
						{
                            return this.Select_Add_DDM_Config_Menu_Layout();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_Config_Menu_Layout"))
						{
                            return this.Select_DDM_Config_Menu_Layout();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Config_Menu_Layout"))
						{
                            return this.Save_DDM_Config_Menu_Layout();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_Layout_Option_Type"))
						{
                            return this.Select_DDM_Layout_Option_Type();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_Add_DDM_Config_Hdr_Ctrls"))
						{
                            return this.Select_Add_DDM_Config_Hdr_Ctrls();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Select_DDM_Config_Hdr_Ctrl"))
						{
                            return this.Select_DDM_Config_Hdr_Ctrl();
                        }
						else if (args.FunctionName.XFEqualsIgnoreCase("Save_DDM_Config_Hdr_Ctrls"))
						{
                            return this.Save_DDM_Config_Hdr_Ctrls();
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

        private XFSelectionChangedTaskResult Save_New_Profile_Config()
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
				
                var wf_ProfileKey = Guid.Parse(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_trv_WF_Profile", Guid.Empty.ToString()));

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
                        var sql = @"SELECT Count(*) as Count
                                    FROM DDM_Config
                                    WHERE Profile_Key = @wf_ProfileKey";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@wf_ProfileKey", SqlDbType.UniqueIdentifier) { Value = wf_ProfileKey }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Count_DT, sql, sqlparams);

                        if (Convert.ToInt32(DDM_Config_Count_DT.Rows[0]["Count"]) == 0)
						{

	                        // Example: Get the max ID for the "MCM_Calc_Config" table
	                        new_Profile_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config", "DDM_Config_ID");
							var sqa_ddm_config = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection, "DDM_Config", "DDM_Config_ID");
	                        var DDM_Config_DT = new DataTable();
	
	                        // Fill the DataTable with the current data from MCM_Dest_Cell
	                        sql = @"SELECT * 
									FROM DDM_Config 
									WHERE DDM_Config_ID = @DDM_Config_ID";
	
	                        // Update the value of the existing sqlparams array
							sqlparams = new SqlParameter[]
	                        {
	                        new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { Value = new_Profile_Config_ID }
	                        };
							
							cmdBuilder.FillDataTable(si, sqa, DDM_Config_DT, sql, sqlparams);
	
	
	
	                        // Query WorkflowProfileHierarchy for the given wf_ProfileKey
	                        var wfProfile_Step_Type_DT = new DataTable();
	                        sql = @"SELECT *
		                            FROM WorkflowProfileHierarchy
		                            WHERE ProfileKey = @ProfileKey";
	                        sqlparams = new SqlParameter[]
	                        {
	                            new SqlParameter("@ProfileKey", SqlDbType.UniqueIdentifier) { Value = wf_ProfileKey }
	                        };
	                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, wfProfile_Step_Type_DT, sql, sqlparams);
	
	                        DataRow newRow = DDM_Config_DT.NewRow();
	                        newRow["DDM_Config_ID"] = new_Profile_Config_ID;
							newRow["DDM_Type"] = 1;
	                        newRow["Profile_Key"] = wf_ProfileKey;
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
							cmdBuilder.UpdateTableSimple(si, "DDM_Config", DDM_Config_DT, sqa, "DDM_Config_ID");
						}
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

		private XFSelectionChangedTaskResult Select_Add_DDM_Config_Menu_Layout()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				var optionintValue = 2;
				UpdateCustomSubstVar(ref select_Result,"BL_DDM_Config_Menu",string.Empty);
	            DDM_Config_Helpers.Layout_OptionType optionType = (DDM_Config_Helpers.Layout_OptionType)optionintValue;
	
	            // 3. Lookup in your LayoutRegistry class
	            if (DDM_Config_Helpers.LayoutRegistry.Configs.TryGetValue(optionType, out var config))
	            {
					// 4. Update the UI Subst Var so the Dashboard flips to the correct UI
	                UpdateCustomSubstVar(ref select_Result, "IV_DDM_Layout_Option_Type", config.DashboardName);
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_Config_Menu_Layout()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				BRApi.ErrorLog.LogMessage(si,$"Hit Select Menu Layout");
				select_Result.ModifiedCustomSubstVars = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
				var existingMenuID = this.args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_DDM_Config_Menu", "0").XFConvertToInt();
				var ddm_Config_Menu_Layout_DT = new DataTable();
				UpdateCustomSubstVar(ref select_Result,"IV_DDM_Config_Menu_UI","0b1b2b2_DDM_Config_Content_NewUpdates");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
					var sqa = new SqlDataAdapter();
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();
	                var sql = @"SELECT *
	                        FROM DDM_Config_Menu_Layout
							WHERE DDM_Menu_ID = @DDM_Menu_ID";
	                var sqlparams = new SqlParameter[]
	                {
	                    new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = existingMenuID }
	                };
	                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, ddm_Config_Menu_Layout_DT, sql, sqlparams);
				}
				if (ddm_Config_Menu_Layout_DT.Rows.Count > 0)
		        {
		            DataRow row = ddm_Config_Menu_Layout_DT.Rows[0];
		
		            // 2. Extract Option_Type and Convert to Enum
		            DDM_Config_Helpers.Layout_OptionType optionType = (DDM_Config_Helpers.Layout_OptionType)row["Option_Type"];
					UpdateCustomSubstVar(ref select_Result, "DL_DDM_Layout_Type", row["Option_Type"].ToString());
		            // 3. Lookup in your LayoutRegistry class
		            if (DDM_Config_Helpers.LayoutRegistry.Configs.TryGetValue(optionType, out var config))
		            {
						foreach (var step in config.ParameterMappings)
						{
						    // The 'step.Value' is the inner Dictionary<string, string>
						    // It usually contains just one pair, but we loop to be safe
						    foreach (var map in step.Value)
						    {
						        string tgtParamName = map.Key;   // e.g. "IV_DDM_Layout_Top_Height"
						        string columnName = map.Value;  // e.g. "Top_Height"
								UpdateCustomSubstVar(ref select_Result, tgtParamName, row[columnName].ToString());
							}
						}
					}
					
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_Layout_Option_Type()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				var ddm_Config_Menu_Layout_DT = new DataTable();

				var optionintValue = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_DDM_Layout_Type", "0").XFConvertToInt();
				var histoptionintValue = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_Layout_Type", "0").XFConvertToInt();

				//UpdateCustomSubstVar(ref select_Result,"IV_DDM_Config_Menu_UI","0b1b2b2_DDM_Config_Content_NewUpdates");
BRApi.ErrorLog.LogMessage(si,$"Hit {optionintValue} - {histoptionintValue} - {args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Layout_Option_Type", "NA")} - {args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_DDM_Layout_Option_Type", "NA")}");
	            // 2. Extract Option_Type and Convert to Enum
	            DDM_Config_Helpers.Layout_OptionType optionType = (DDM_Config_Helpers.Layout_OptionType)optionintValue;
	
	            // 3. Lookup in your LayoutRegistry class
	            if (DDM_Config_Helpers.LayoutRegistry.Configs.TryGetValue(optionType, out var config))
	            {
BRApi.ErrorLog.LogMessage(si,$"Hit {config.DashboardName}");
					// 4. Update the UI Subst Var so the Dashboard flips to the correct UI
	                UpdateCustomSubstVar(ref select_Result, "IV_DDM_Layout_Option_Type", config.DashboardName);
				}
				foreach (KeyValuePair<string, string> kvp in select_Result.ModifiedCustomSubstVars)
    {
        	BRApi.ErrorLog.LogMessage(si,$"Key: {kvp.Key} | Value: {kvp.Value}");
    }
				var uiAction = new XFSelectionChangedUIActionInfo();
				uiAction.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
				uiAction.DashboardsToRedraw = "0b1b2b_DDM_Config_Content";
				select_Result.ModifiedSelectionChangedUIActionInfo = uiAction;
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}


		private XFSelectionChangedTaskResult Select_Add_DDM_Config_Hdr_Ctrls()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				UpdateCustomSubstVar(ref select_Result,"BL_DDM_Config_Hdr_Ctrls",string.Empty);
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
		private XFSelectionChangedTaskResult Select_DDM_Config_Hdr_Ctrl()
		{
		    try
		    {
		        var select_Result = new XFSelectionChangedTaskResult();
				var ddm_Config_Hdr_Ctrls_DT = new DataTable();
				var configHdrCtrlID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_Config_Hdr_Ctrls", "0").XFConvertToInt();
				select_Result.ChangeCustomSubstVarsInDashboard = true;
				UpdateCustomSubstVar(ref select_Result,"IV_DDM_Hdr_Ctrl_UI","0b1b2c2_DDM_Config_Content_NewUpdates");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
					var sqa = new SqlDataAdapter();
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();
	                var sql = @"SELECT *
	                        FROM DDM_Config_Hdr_Ctrls
							WHERE DDM_Hdr_Ctrl_ID = @DDM_Hdr_Ctrl_ID";
	                var sqlparams = new SqlParameter[]
	                {
	                    new SqlParameter("@DDM_Hdr_Ctrl_ID", SqlDbType.Int) { Value = configHdrCtrlID }
	                };
	                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, ddm_Config_Hdr_Ctrls_DT, sql, sqlparams);
				}
				if (ddm_Config_Hdr_Ctrls_DT.Rows.Count > 0)
		        {
		            DataRow row = ddm_Config_Hdr_Ctrls_DT.Rows[0];
		
		            // 2. Extract Option_Type and Convert to Enum
		            DDM_Config_Helpers.HdrCtrlType hdrctrlType = (DDM_Config_Helpers.HdrCtrlType)row["Option_Type"];
		
		            // 3. Lookup in your LayoutRegistry class
		            if (DDM_Config_Helpers.HdrCtrlRegistry.Configs.TryGetValue(hdrctrlType, out var config))
		            {
		                // Retrieve the DashboardName from the class
		                string targetDashboard = config.DashboardName;
		BRApi.ErrorLog.LogMessage(si,$"Hit: {targetDashboard}");
		                // 4. Update the UI Subst Var so the Dashboard flips to the correct UI
		                UpdateCustomSubstVar(ref select_Result, "IV_DDM_Hdr_Ctrl_Option_Type", targetDashboard);
					}
				}
				return select_Result;
			}
            catch (Exception ex)
            {
                // Log and rethrow exceptions for error handling.
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
		}
		
        #region "Menu Layout Save"
		private XFSelectionChangedTaskResult Save_DDM_Config_Menu_Layout()
		{
		    try
		    {
				var runType = "New";
				var existingMenuID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_Config_Menu", "0").XFConvertToInt();
				if (existingMenuID > 0)
				{
					runType = "Update";
				}
		        var save_Result = new XFSelectionChangedTaskResult();
		        var configID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Config_ID", "0").XFConvertToInt();
		        var menuName = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Menu_Name", string.Empty);
		        var sortOrder = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Menu_Sort_Order", "0").XFConvertToInt();
		        var optionType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_Layout_Type", "0").XFConvertToInt();
		
		        // 1. Run Duplicate Check before proceeding
		        // We 'Initiate' to fill GBL_Menu dictionaries from the DB
		        Duplicate_Menu_Check(configID, "Initiate");
		        
		        // Logic to check if current name/sort order exists in the dictionaries 
		        // (Assuming Duplicate_Menu_Options/Sort_Order are class-level booleans)
		        if (GBL_Menu_Name_Dict.Values.Contains(menuName) && runType == "New")
		            throw new Exception("A menu with this name already exists.");
		
		        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
		        using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
		        {
		            connection.Open();
		            var sqa = new SqlDataAdapter();
		            var DDM_Config_Menu_Layout_DT = new DataTable();
		            		            var sqlSelect = "SELECT * FROM DDM_Config_Menu_Layout WHERE DDM_Menu_ID = @Menu_ID";
		
		            DataRow row;
		            if (runType == "New")
		            {
						var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
		                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
		                var newID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Menu_Layout", "DDM_Menu_ID");
						var DDM_Config_DT = new DataTable();
		                
		                cmdBuilder.FillDataTable(si, sqa, DDM_Config_Menu_Layout_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@Menu_ID", -1) });
						sqlSelect = "SELECT * FROM DDM_Config WHERE DDM_Config_ID = @DDM_Config_ID";
						sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { Value = configID }});
		                row = DDM_Config_Menu_Layout_DT.NewRow();
						if (DDM_Config_DT.Rows.Count > 0)
					    {
					        var typeValue = DDM_Config_DT.Rows[0]["DDM_Type"];
					        DDM_Config_Helpers.DDMType configType = (DDM_Config_Helpers.DDMType)typeValue;
					        
					        switch (configType)
					        {
					            case DDM_Config_Helpers.DDMType.WFProfile:
					                row["DDM_Type"] = (int)DDM_Config_Helpers.DDMType.WFProfile;
					                row["Scen_Type"] = DDM_Config_DT.Rows[0]["Scen_Type"];
									row["Profile_Key"] = DDM_Config_DT.Rows[0]["Profile_Key"];
					                break;
					
					            case DDM_Config_Helpers.DDMType.StandAlone:
					                row["DDM_Type"] = (int)DDM_Config_Helpers.DDMType.StandAlone;
					                // Add specific logic/column defaults for StandAlone here
					                break;
					        }
					    }
		                row["DDM_Menu_ID"] = newID;
						row["Status"] = "In Process";
		                row["Create_Date"] = DateTime.Now;
		                row["Create_User"] = si.UserName;
		                row["Update_Date"] = DateTime.Now;
		                row["Update_User"] = si.UserName;
		                DDM_Config_Menu_Layout_DT.Rows.Add(row);
		            }
		            else
		            {
		                cmdBuilder.FillDataTable(si, sqa, DDM_Config_Menu_Layout_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@Menu_ID", existingMenuID) });
		                if (DDM_Config_Menu_Layout_DT.Rows.Count == 0) throw new Exception("Record not found.");
		                row = DDM_Config_Menu_Layout_DT.Rows[0];
		            }
		
		            // Map standard fields
		            row["DDM_Config_ID"] = configID;
		            row["Name"] = menuName;
		            row["Sort_Order"] = sortOrder;
		            row["Option_Type"] = optionType;
		            row["Update_Date"] = DateTime.Now;
		            row["Update_User"] = si.UserName;
		
					DDM_Config_Helpers.Layout_OptionType layoutType = (DDM_Config_Helpers.Layout_OptionType)optionType;
					if (DDM_Config_Helpers.LayoutRegistry.Configs.TryGetValue(layoutType, out var layoutConfig))
					{
					    // 2. Iterate through the mapping: Key = IV Name (Source), Value = DB Column (Target)
					    // ParameterMappings is Dictionary<int, Dictionary<string, string>>
					    foreach (var group in layoutConfig.ParameterMappings.Values)
					    {
					        foreach (var mapping in group)
					        {
					            string sourceSubstVar = mapping.Key;   // e.g., "IV_DDM_Layout_DB_Name"
					            string targetCol = mapping.Value; // e.g., "DB_Name"
					            
					            // Assign the value from CustomSubstVars to the DataRow
					            row[targetCol] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar, string.Empty);
					        }
					    }
					}
		
		            // 2. Perform Layout Validation and Column Clearing
		            var valResult = Val_DDM_Config_Menu_Layout(ref row);
		            if (!valResult.IsOK) return valResult;
		
		            // 3. Save to DB
		            cmdBuilder.UpdateTableSimple(si, "DDM_Config_Menu_Layout", DDM_Config_Menu_Layout_DT, sqa, "DDM_Menu_ID");
		            save_Result.Message = "Save Successful.";
		        }
		
		        save_Result.IsOK = true;
		        save_Result.ShowMessageBox = true;
		        return save_Result;
		    }
		    catch (Exception ex) { return new XFSelectionChangedTaskResult { IsOK = false, Message = ex.Message, ShowMessageBox = true }; }
		}
		
		#endregion
		
        #region "Header Controls Save"
	
		private XFSelectionChangedTaskResult Save_DDM_Config_Hdr_Ctrls()
		{
		    try
		    {
				var runType = "New";
				var existingHdrCtrlID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_Config_Hdr_Ctrls", "0").XFConvertToInt();
				
				if (existingHdrCtrlID > 0)
				{
					runType = "Update";
				}
		        var save_Result = new XFSelectionChangedTaskResult();
		        var configID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Config_ID", "0").XFConvertToInt();
		        var menuName = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Menu_Name", string.Empty);
		        var sortOrder = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_DDM_Menu_Sort_Order", "0").XFConvertToInt();
		        var optionType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_DDM_Hdr_Ctrl_Option_Type", "0").XFConvertToInt();
				var menuID = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_DDM_Config_Menu", "0").XFConvertToInt();
		
		        // 1. Run Duplicate Check before proceeding
		        // We 'Initiate' to fill GBL_Menu dictionaries from the DB
		        Duplicate_HdrCtrl_Check(menuID, "Initiate");
		        
		        // Logic to check if current name/sort order exists in the dictionaries 
		        // (Assuming Duplicate_Menu_Options/Sort_Order are class-level booleans)
		        if (GBL_Menu_Name_Dict.Values.Contains(menuName) && runType == "New")
		            throw new Exception("A menu with this name already exists.");
		
		        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
		        using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
		        {
		            connection.Open();
		            var sqa = new SqlDataAdapter();
		            var DDM_Config_Hdr_Ctrls_DT = new DataTable();
		            		            var sqlSelect = "SELECT * FROM DDM_Config_Hdr_Ctrls WHERE DDM_Hdr_Ctrl_ID = @DDM_Hdr_Ctrl_ID";
		
		            DataRow row;
		            if (runType == "New")
		            {
						var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
		                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
		                var newID = sql_gbl_get_max_id.Get_Max_ID(si, "DDM_Config_Hdr_Ctrls", "DDM_Hdr_Ctrl_ID");
						var DDM_Config_DT = new DataTable();
		                
		                cmdBuilder.FillDataTable(si, sqa, DDM_Config_Hdr_Ctrls_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_Hdr_Ctrl_ID", -1) });
						sqlSelect = "SELECT * FROM DDM_Config WHERE DDM_Config_ID = @DDM_Config_ID";
						sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { Value = configID }});
		                row = DDM_Config_Hdr_Ctrls_DT.NewRow();
						if (DDM_Config_DT.Rows.Count > 0)
					    {
					        var typeValue = DDM_Config_DT.Rows[0]["DDM_Type"];
					        DDM_Config_Helpers.DDMType configType = (DDM_Config_Helpers.DDMType)typeValue;
					        
					        switch (configType)
					        {
					            case DDM_Config_Helpers.DDMType.WFProfile:
					                row["DDM_Type"] = (int)DDM_Config_Helpers.DDMType.WFProfile;
					                row["Scen_Type"] = DDM_Config_DT.Rows[0]["Scen_Type"];
									row["Profile_Key"] = DDM_Config_DT.Rows[0]["Profile_Key"];
					                break;
					
					            case DDM_Config_Helpers.DDMType.StandAlone:
					                row["DDM_Type"] = (int)DDM_Config_Helpers.DDMType.StandAlone;
					                // Add specific logic/column defaults for StandAlone here
					                break;
					        }
					    }
		                row["DDM_Menu_ID"] = menuID;
						row["DDM_Config_ID"] = configID;
						row["DDM_Hdr_Ctrl_ID"] = newID;
						row["Status"] = "In Process";
		                DDM_Config_Hdr_Ctrls_DT.Rows.Add(row);
		            }
		            else
		            {
		                cmdBuilder.FillDataTable(si, sqa, DDM_Config_Hdr_Ctrls_DT, sqlSelect, new SqlParameter[] { new SqlParameter("@DDM_Hdr_Ctrl_ID", existingHdrCtrlID) });
		                if (DDM_Config_Hdr_Ctrls_DT.Rows.Count == 0) throw new Exception("Record not found.");
		                row = DDM_Config_Hdr_Ctrls_DT.Rows[0];
		            }
		
		            // Map standard fields
		            row["Name"] = menuName;
		            row["Sort_Order"] = sortOrder;
					row["Status"] = "In Process";
		            row["Option_Type"] = optionType;
		            row["Create_Date"] = DateTime.Now;
		            row["Create_User"] = si.UserName;
		            row["Update_Date"] = DateTime.Now;
		            row["Update_User"] = si.UserName;
		
 					DDM_Config_Helpers.HdrCtrlType hdrctrlType = (DDM_Config_Helpers.HdrCtrlType)row["Option_Type"];	
		        	if (DDM_Config_Helpers.HdrCtrlRegistry.Configs.TryGetValue(hdrctrlType, out var HdrCtrlConfig))
					{
					    // 2. Iterate through the mapping: Key = IV Name (Source), Value = DB Column (Target)
					    // ParameterMappings is Dictionary<int, Dictionary<string, string>>
					    foreach (var group in HdrCtrlConfig.ParameterMappings.Values)
					    {
					        foreach (var mapping in group)
					        {
					            string sourceSubstVar = mapping.Key;   // e.g., "IV_DDM_Layout_DB_Name"
					            string targetCol = mapping.Value; // e.g., "DB_Name"
					            
					            // Assign the value from CustomSubstVars to the DataRow
					            row[targetCol] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(sourceSubstVar, string.Empty);
					        }
					    }
					}
		
		            // 2. Perform Layout Validation and Column Clearing
		            var valResult = Val_DDM_Config_Hdr_Ctrls(ref row);
		            if (!valResult.IsOK) return valResult;
		
		            // 3. Save to DB
		            cmdBuilder.UpdateTableSimple(si, "DDM_Config_Hdr_Ctrls", DDM_Config_Hdr_Ctrls_DT, sqa, "Hdr_Ctrl_ID");
		            save_Result.Message = "Save Successful.";
		        }
		
		        save_Result.IsOK = true;
		        save_Result.ShowMessageBox = true;
		        return save_Result;
		    }
		    catch (Exception ex) { return new XFSelectionChangedTaskResult { IsOK = false, Message = ex.Message, ShowMessageBox = true }; }
		}
		
		#endregion
		
		#region "Data Validation"
		

        #region "Duplicate Checks"
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
                            var sql = @"SELECT *
						       			FROM DDM_Config_Menu_Layout
						       			WHERE DDM_Config_ID = @DDM_Config_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { Value = wfProfile_ID}
                            };

                            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, DDM_Config_Menu_DT, sql, sqlparams);

                            foreach (DataRow menu_Row in DDM_Config_Menu_DT.Rows)
                            {
                                int menu_ID = (int)menu_Row["DDM_Menu_ID"];
                                int sortOrder = (int)menu_Row["Sort_Order"];
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
                        int sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Sort_Order"];
                        string menu_Name = (string)Config_Menu_DataRow.ModifiedDataRow["Name"];

                        GBL_Menu_Order_Dict.Add(menuOptionID, sortOrder);
                        GBL_Menu_Name_Dict.Add(menuOptionID, menu_Name);

                    }
                    else if (DDL_Process == "Update")
                    {
                        int menuOptionID = (int)Config_Menu_DataRow.OriginalDataRow["DDM_Menu_ID"];
                        int orig_sortOrder = (int)Config_Menu_DataRow.OriginalDataRow["Sort_Order"];
                        int new_sortOrder = (int)Config_Menu_DataRow.ModifiedDataRow["Sort_Order"];
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
        /// Checks for duplicate menu header options and sort orders.
        /// This method is used to identify duplicates for menu header options during the save process.
        /// </summary>
        private void Duplicate_HdrCtrl_Check(int wfProfile_ID, String Dup_Process_Step, [Optional] String DDL_Process, [Optional] XFEditedDataRow Config_Menu_DataRow)
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
                            var sql = @"SELECT *
						       			FROM DDM_Config_Hdr_Ctrls
						       			WHERE DDM_Config_ID = @DDM_Config_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@DDM_Config_ID", SqlDbType.Int) { Value = wfProfile_ID}
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
		#endregion
		
		#region "Validate Columns"
		public XFSelectionChangedTaskResult Val_DDM_Config_Menu_Layout(ref DataRow row)
		{
		    var valResult = new XFSelectionChangedTaskResult { IsOK = true };
		
		    try
		    {
		        // Parse the Option Type from the row
		        DDM_Config_Helpers.Layout_OptionType optionType = (DDM_Config_Helpers.Layout_OptionType)row["Option_Type"];
		
		        if (DDM_Config_Helpers.LayoutRegistry.Configs.TryGetValue(optionType, out var config))
		        {
		            // 1. Identify "Whitelist" columns (Target DB columns)
		            var requiredColumns = config.ParameterMappings.Values
		                .SelectMany(d => d.Values)
		                .ToHashSet();
		
		            // 2. Define the pool of all potential layout columns to be cleaned if not in whitelist
		            // Add all your DB column names here that relate to specific layouts
		            var allLayoutColumns = new List<string> { 
		                "DB_Name", "CV_Name", "DB_Name_Top", "CV_Name_Top", "DB_Name_Bottom", "CV_Name_Bottom",
		                "DB_Name_Left", "CV_Name_Left", "DB_Name_Right", "CV_Name_Right" 
		            };
		
		            // 3. Part A: Validate Required
		            foreach (var col in requiredColumns)
		            {
		                if (row.IsNull(col) || string.IsNullOrWhiteSpace(row[col].ToString()))
		                {
		                    valResult.IsOK = false;
		                    valResult.Message = $"Validation Error: '{col}' is required for {optionType}.";
		                    return valResult;
		                }
		            }
		
		            // 4. Part B: Clear other layout columns
		            foreach (var col in allLayoutColumns)
		            {
		                if (!requiredColumns.Contains(col))
		                {
		                    row[col] = DBNull.Value;
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        valResult.IsOK = false;
		        valResult.Message = "Error during row validation: " + ex.Message;
		    }
		
		    return valResult;
		}

		public XFSelectionChangedTaskResult Val_DDM_Config_Hdr_Ctrls(ref DataRow row)
		{
		    var valResult = new XFSelectionChangedTaskResult { IsOK = true };
		
		    try
		    {
		        // Parse the Option Type from the row
		        DDM_Config_Helpers.HdrCtrlType optionType = (DDM_Config_Helpers.HdrCtrlType)row["Option_Type"];
		
		        if (DDM_Config_Helpers.HdrCtrlRegistry.Configs.TryGetValue(optionType, out var config))
		        {
		            // 1. Identify "Whitelist" columns (Target DB columns)
		            var requiredColumns = config.ParameterMappings.Values
		                .SelectMany(d => d.Values)
		                .ToHashSet();
		
		            // 2. Define the pool of all potential layout columns to be cleaned if not in whitelist
		            // Add all your DB column names here that relate to specific layouts
		            var allLayoutColumns = new List<string> { 
		                 "Name"
		            };
		
		            // 3. Part A: Validate Required
		            foreach (var col in requiredColumns)
		            {
		                if (row.IsNull(col) || string.IsNullOrWhiteSpace(row[col].ToString()))
		                {
		                    valResult.IsOK = false;
		                    valResult.Message = $"Validation Error: '{col}' is required for {optionType}.";
		                    return valResult;
		                }
		            }
		
		            // 4. Part B: Clear other layout columns
		            foreach (var col in allLayoutColumns)
		            {
		                if (!requiredColumns.Contains(col))
		                {
		                    row[col] = DBNull.Value;
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        valResult.IsOK = false;
		        valResult.Message = "Error during row validation: " + ex.Message;
		    }
		
		    return valResult;
		}

		public void Val_Col_Populated(DataRow row, string colName, ref XFSelectionChangedTaskResult result)
		{
		    if (row.IsNull(colName) || string.IsNullOrEmpty(row[colName].ToString()))
		    {
		        // Pseudo-code: add your specific framework's error message
		        result.IsOK = false;
		        result.Message = $"{colName} is required for this layout.";
		    }
		}
		
		#endregion
		#endregion
		
        private void UpdateCustomSubstVar(ref XFSelectionChangedTaskResult result, string key, string value)
        {
            if (result.ModifiedCustomSubstVars.ContainsKey(key))
            {
                result.ModifiedCustomSubstVars.XFSetValue(key, value);
                globals.SetStringValue(key, value);
            }
            else
            {
                result.ModifiedCustomSubstVars.Add(key, value);
                globals.SetStringValue(key, value);
            }
        }
    }
}