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
using Workspace.OSConsTools.MDM_Config_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.MDM_Config_Data
{
	public class MainClass
	{
		private SessionInfo si;
		private BRGlobals globals;
		private object api;
		private DashboardExtenderArgs args;

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
					case DashboardExtenderFunctionType.LoadDashboard:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Load Dashboard logic here.
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
								loadDashboardTaskResult.ModifiedCustomSubstVars = null;
								return loadDashboardTaskResult;
							}
						}
						break;
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Dashboard Component Selection Changed logic here.
							var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
							selectionChangedTaskResult.IsOK = true;
							selectionChangedTaskResult.ShowMessageBox = false;
							selectionChangedTaskResult.Message = "";
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = null;
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = false;
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVars = null;
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = false;
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = null;
							return selectionChangedTaskResult;
						}
						break;
					case DashboardExtenderFunctionType.SqlTableEditorSaveData:
						var save_Result = new XFSqlTableEditorSaveDataTaskResult();
						
						if (args.FunctionName.XFEqualsIgnoreCase("Save_CDC_Config"))
						{
							save_Result = Save_CDC_Config();
							return save_Result;
						}
						else if (args.FunctionName.XFEqualsIgnoreCase("Save_CDC_Config_Detail"))
						{
							save_Result = Save_CDC_Config_Detail();
							return save_Result;
						}
						else if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
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
		/// Saves CDC configuration master records to the MDM_CDC_Config table.
		/// Handles both insert and update operations.
		/// </summary>
		private XFSqlTableEditorSaveDataTaskResult Save_CDC_Config()
		{
			try
			{
				var save_Result = new XFSqlTableEditorSaveDataTaskResult();
				var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
				var CDC_Config_ID = 0;

				var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
				using (var connection = new SqlConnection(dbConnApp.ConnectionString))
				{
					connection.Open();
					var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
					var sqa = new SqlDataAdapter();
					var MDM_CDC_Config_DT = new DataTable();

					// Fill the DataTable with the current data from MDM_CDC_Config
					var sql = @"SELECT * FROM MDM_CDC_Config";
					
					cmdBuilder.FillDataTable(si, sqa, MDM_CDC_Config_DT, sql);
					MDM_CDC_Config_DT.PrimaryKey = new DataColumn[] { MDM_CDC_Config_DT.Columns["CDC_Config_ID"]! };

					// Loop through each row in the table editor that was added or updated prior to hitting save
					foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
					{
						if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
						{
							// Get the next ID for new records
							var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
							CDC_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "MDM_CDC_Config", "CDC_Config_ID");

							var new_config_Row = MDM_CDC_Config_DT.NewRow();
							new_config_Row["CDC_Config_ID"] = CDC_Config_ID;
							new_config_Row["Name"] = xfRow.ModifiedDataRow.XFGetValue("Name", "");
							new_config_Row["Dim_Type"] = xfRow.ModifiedDataRow.XFGetValue("Dim_Type", "");
							new_config_Row["Dim_ID"] = xfRow.ModifiedDataRow.XFGetValue("Dim_ID", 0);
							new_config_Row["Src_Connection"] = xfRow.ModifiedDataRow.XFGetValue("Src_Connection", DBNull.Value);
							new_config_Row["Src_SQL_String"] = xfRow.ModifiedDataRow.XFGetValue("Src_SQL_String", DBNull.Value);
							new_config_Row["Dim_Mgmt_Process"] = xfRow.ModifiedDataRow.XFGetValue("Dim_Mgmt_Process", DBNull.Value);
							new_config_Row["Trx_Rule"] = xfRow.ModifiedDataRow.XFGetValue("Trx_Rule", DBNull.Value);
							new_config_Row["Appr_ID"] = xfRow.ModifiedDataRow.XFGetValue("Appr_ID", DBNull.Value);
							new_config_Row["Mbr_PrefSuff"] = xfRow.ModifiedDataRow.XFGetValue("Mbr_PrefSuff", DBNull.Value);
							new_config_Row["Mbr_PrefSuff_Txt"] = xfRow.ModifiedDataRow.XFGetValue("Mbr_PrefSuff_Txt", DBNull.Value);
							new_config_Row["Create_Date"] = DateTime.Now;
							new_config_Row["Create_User"] = si.UserName;
							new_config_Row["Update_Date"] = DateTime.Now;
							new_config_Row["Update_User"] = si.UserName;

							MDM_CDC_Config_DT.Rows.Add(new_config_Row);
						}
						else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
						{
							// Update existing record
							CDC_Config_ID = xfRow.ModifiedDataRow.XFGetValue("CDC_Config_ID", 0);
							var existing_Row = MDM_CDC_Config_DT.Rows.Find(CDC_Config_ID);
							if (existing_Row != null)
							{
								existing_Row["Name"] = xfRow.ModifiedDataRow.XFGetValue("Name", "");
								existing_Row["Dim_Type"] = xfRow.ModifiedDataRow.XFGetValue("Dim_Type", "");
								existing_Row["Dim_ID"] = xfRow.ModifiedDataRow.XFGetValue("Dim_ID", 0);
								existing_Row["Src_Connection"] = xfRow.ModifiedDataRow.XFGetValue("Src_Connection", DBNull.Value);
								existing_Row["Src_SQL_String"] = xfRow.ModifiedDataRow.XFGetValue("Src_SQL_String", DBNull.Value);
								existing_Row["Dim_Mgmt_Process"] = xfRow.ModifiedDataRow.XFGetValue("Dim_Mgmt_Process", DBNull.Value);
								existing_Row["Trx_Rule"] = xfRow.ModifiedDataRow.XFGetValue("Trx_Rule", DBNull.Value);
								existing_Row["Appr_ID"] = xfRow.ModifiedDataRow.XFGetValue("Appr_ID", DBNull.Value);
								existing_Row["Mbr_PrefSuff"] = xfRow.ModifiedDataRow.XFGetValue("Mbr_PrefSuff", DBNull.Value);
								existing_Row["Mbr_PrefSuff_Txt"] = xfRow.ModifiedDataRow.XFGetValue("Mbr_PrefSuff_Txt", DBNull.Value);
								existing_Row["Update_Date"] = DateTime.Now;
								existing_Row["Update_User"] = si.UserName;
							}
						}
						else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
						{
							// Delete record
							CDC_Config_ID = xfRow.ModifiedDataRow.XFGetValue("CDC_Config_ID", 0);
							var delete_Row = MDM_CDC_Config_DT.Rows.Find(CDC_Config_ID);
							if (delete_Row != null)
							{
								delete_Row.Delete();
							}
						}
					}

					// Update the database
					cmdBuilder.UpdateTableSimple(si, "MDM_CDC_Config", MDM_CDC_Config_DT, sqa, "CDC_Config_ID");
				}

				save_Result.IsOK = true;
				save_Result.ShowMessageBox = false;
				save_Result.Message = "";
				save_Result.CancelDefaultSave = true; // We handled the save ourselves
				return save_Result;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}

		/// <summary>
		/// Saves CDC configuration detail records to the MDM_CDC_Config_Detail table.
		/// Handles both insert and update operations.
		/// </summary>
		private XFSqlTableEditorSaveDataTaskResult Save_CDC_Config_Detail()
		{
			try
			{
				var save_Result = new XFSqlTableEditorSaveDataTaskResult();
				var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
				var CDC_Config_Detail_ID = 0;

				var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
				using (var connection = new SqlConnection(dbConnApp.ConnectionString))
				{
					connection.Open();
					var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
					var sqa = new SqlDataAdapter();
					var MDM_CDC_Config_Detail_DT = new DataTable();

					// Get CDC_Config_ID from custom substitution variables
					var CDC_Config_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_MDM_CDC_Config_ID", "0").XFConvertToInt();

					// Fill the DataTable with the current data from MDM_CDC_Config_Detail
					var sql = @"SELECT * 
								FROM MDM_CDC_Config_Detail
								WHERE CDC_Config_ID = @CDC_Config_ID
								ORDER BY CDC_Config_Detail_ID";
					
					var sqlparams = new SqlParameter[]
					{
						new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { Value = CDC_Config_ID }
					};

					cmdBuilder.FillDataTable(si, sqa, MDM_CDC_Config_Detail_DT, sql, sqlparams);
					MDM_CDC_Config_Detail_DT.PrimaryKey = new DataColumn[] 
					{ 
						MDM_CDC_Config_Detail_DT.Columns["CDC_Config_ID"]!, 
						MDM_CDC_Config_Detail_DT.Columns["CDC_Config_Detail_ID"]! 
					};

					// Loop through each row in the table editor that was added or updated prior to hitting save
					foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
					{
						if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
						{
							// Get the next detail ID for new records
							var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
							CDC_Config_Detail_ID = sql_gbl_get_max_id.Get_Max_ID(si, "MDM_CDC_Config_Detail", "CDC_Config_Detail_ID");

							var new_detail_Row = MDM_CDC_Config_Detail_DT.NewRow();
							new_detail_Row["CDC_Config_ID"] = CDC_Config_ID;
							new_detail_Row["CDC_Config_Detail_ID"] = CDC_Config_Detail_ID;
							new_detail_Row["OS_Mbr_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Column", DBNull.Value);
							new_detail_Row["OS_Mbr_Vary_Scen_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Vary_Scen_Column", DBNull.Value);
							new_detail_Row["OS_Mbr_Vary_Time_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Vary_Time_Column", DBNull.Value);
							new_detail_Row["Src_Mbr_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Mbr_Column", DBNull.Value);
							new_detail_Row["Src_Vary_Scen_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Vary_Scen_Column", DBNull.Value);
							new_detail_Row["Src_Vary_Time_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Vary_Time_Column", DBNull.Value);
							new_detail_Row["Create_Date"] = DateTime.Now;
							new_detail_Row["Create_User"] = si.UserName;
							new_detail_Row["Update_Date"] = DateTime.Now;
							new_detail_Row["Update_User"] = si.UserName;

							MDM_CDC_Config_Detail_DT.Rows.Add(new_detail_Row);
						}
						else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
						{
							// Update existing detail record
							CDC_Config_Detail_ID = xfRow.ModifiedDataRow.XFGetValue("CDC_Config_Detail_ID", 0);
							var existing_Row = MDM_CDC_Config_Detail_DT.Rows.Find(new object[] { CDC_Config_ID, CDC_Config_Detail_ID });
							if (existing_Row != null)
							{
								existing_Row["OS_Mbr_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Column", DBNull.Value);
								existing_Row["OS_Mbr_Vary_Scen_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Vary_Scen_Column", DBNull.Value);
								existing_Row["OS_Mbr_Vary_Time_Column"] = xfRow.ModifiedDataRow.XFGetValue("OS_Mbr_Vary_Time_Column", DBNull.Value);
								existing_Row["Src_Mbr_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Mbr_Column", DBNull.Value);
								existing_Row["Src_Vary_Scen_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Vary_Scen_Column", DBNull.Value);
								existing_Row["Src_Vary_Time_Column"] = xfRow.ModifiedDataRow.XFGetValue("Src_Vary_Time_Column", DBNull.Value);
								existing_Row["Update_Date"] = DateTime.Now;
								existing_Row["Update_User"] = si.UserName;
							}
						}
						else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
						{
							// Delete detail record
							CDC_Config_Detail_ID = xfRow.ModifiedDataRow.XFGetValue("CDC_Config_Detail_ID", 0);
							var delete_Row = MDM_CDC_Config_Detail_DT.Rows.Find(new object[] { CDC_Config_ID, CDC_Config_Detail_ID });
							if (delete_Row != null)
							{
								delete_Row.Delete();
							}
						}
					}

					// Update the database
					cmdBuilder.UpdateTableComposite(si, "MDM_CDC_Config_Detail", MDM_CDC_Config_Detail_DT, sqa, "CDC_Config_ID", "CDC_Config_Detail_ID");
				}

				save_Result.IsOK = true;
				save_Result.ShowMessageBox = false;
				save_Result.Message = "";
				save_Result.CancelDefaultSave = true; // We handled the save ourselves
				return save_Result;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
	}
}