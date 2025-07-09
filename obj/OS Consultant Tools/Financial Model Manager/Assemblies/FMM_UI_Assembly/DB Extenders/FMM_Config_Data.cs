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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Config_Data
{
    public class MainClass
    {
        #region "Global Variables"
        public int GBL_Calc_ID { get; set; }
        public string GBL_BalCalc { get; set; } = string.Empty;
        public string GBL_BalBufferCalc { get; set; } = string.Empty;
        public string GBL_UnbalBufferCalc { get; set; } = string.Empty;
        public string GBL_UnbalCalc { get; set; } = string.Empty;
        public string GBL_Table_CalcLogic { get; set; } = string.Empty;
        public int GBL_Table_SrcCell { get; set; }
        public int GBL_Src_Cell_Cnt { get; set; }
        public string GBL_Model_Type { get; set; } = "Cube";
        public Dictionary<int, string> GBL_SrcCellDict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> GBL_UnbalCalcDict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> GBL_SrcCellDrillDownDict { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, string> GBL_Activity_List_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Calc_Unit_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Unit_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Acct_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Appr_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Appr_Step_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Register_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Col_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Calc_Dict { get; set; } = new Dictionary<string, string>();
        public bool GBL_Duplicate_Activity { get; set; } = false;
        public bool GBL_Duplicate_Calc_Unit_Config { get; set; } = false;
        public bool GBL_Duplicate_Unit_Config { get; set; } = false;
        public bool GBL_Duplicate_Acct_Config { get; set; } = false;
        public bool GBL_Duplicate_Approval { get; set; } = false;
        public bool GBL_Duplicate_Appr_Step { get; set; } = false;
        public bool GBL_Duplicate_Reg_Config { get; set; } = false;
        public bool GBL_Duplicate_Col_Config { get; set; } = false;
        public bool GBL_Duplicate_Calc_Config { get; set; } = false;
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        public int GBL_FMM_Act_ID { get; set; }
        public int GBL_Curr_FMM_Act_ID { get; set; }
        public int GBL_FMM_Unit_ID { get; set; }
        public int GBL_Curr_FMM_Unit_ID { get; set; }
        public int GBL_FMM_Acct_ID { get; set; }
        public int GBL_Curr_FMM_Acct_ID { get; set; }
        public int GBL_FMM_Appr_ID { get; set; }
        public int GBL_Curr_FMM_Appr_ID { get; set; }
        public int GBL_FMM_Appr_Step_ID { get; set; }
        public int GBL_Curr_FMM_Appr_Step_ID { get; set; }
        public int GBL_FMM_Reg_Config_ID { get; set; }
        public int GBL_Curr_FMM_Reg_Config_ID { get; set; }
        public int GBL_FMM_Col_ID { get; set; }
        public int GBL_Curr_FMM_Col_ID { get; set; }
        public int GBL_FMM_Models_ID { get; set; }
        public int GBL_Curr_FMM_Models_ID { get; set; }
        public int GBL_FMM_Calc_ID { get; set; }
        public int GBL_Curr_FMM_Calc_ID { get; set; }
        public int GBL_FMM_Cell_ID { get; set; }
        public int GBL_Curr_FMM_Cell_ID { get; set; }
        public int GBL_FMM_Src_Cell_ID { get; set; }
        public int GBL_Curr_FMM_Src_Cell_ID { get; set; }
        public int GBL_FMM_Model_Grps_ID { get; set; }
        public int GBL_Curr_FMM_Model_Grps_ID { get; set; }
        public int GBL_FMM_Model_Grp_Assign_ID { get; set; }
        public int GBL_Curr_FMM_Model_Grp_Assign_ID { get; set; }
        public int GBL_FMM_Calc_Unit_ID { get; set; }
        public int GBL_Curr_FMM_Calc_Unit_ID { get; set; }
        public int GBL_FMM_Calc_Unit_Assign_ID { get; set; }
        public int GBL_Curr_FMM_Calc_Unit_Assign_ID { get; set; }
        public Dictionary<string, string> GBL_Models_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> GBL_Model_Grps_Dict { get; set; } = new Dictionary<string, string>();
        public bool GBL_Duplicate_Models { get; set; } = false;
        public bool GBL_Duplicate_Model_Grps { get; set; } = false;
        #endregion

        public MainClass()
        {
        }

        public MainClass(SessionInfo si, BRGlobals globals)
        {
            this.si = si;
            this.globals = globals;
        }
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
                        var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_Calc_Config_Rows"))
                        {
                            GBL_Model_Type = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Calc_Type");
                            save_Task_Result = Save_Calc_Config_Rows();
                            if (GBL_Model_Type == "Cube")
                            {
                                Evaluate_Calc_Config_Setup(GBL_Calc_ID);
                            }

                            return save_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Cell_Rows"))
                        {
                            GBL_Model_Type = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Calc_Type");
                            save_Task_Result = Save_Cell_Rows();
                            if (GBL_Model_Type == "Cube")
                            {
                                Evaluate_Calc_Config_Setup(GBL_Calc_ID);
                            }

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Src_Cell_Rows"))
                        {
                            GBL_Model_Type = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Calc_Type");
                            save_Task_Result = Save_Src_Cell_Rows();
                            if (GBL_Model_Type == "Cube")
                            {
                                Evaluate_Calc_Config_Setup(GBL_Calc_ID);
                            }

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Activity_List"))
                        {
                            save_Task_Result = Save_Activity_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Calc_Unit_List"))
                        {
                            save_Task_Result = Save_Calc_Unit_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Unit_List"))
                        {
                            save_Task_Result = Save_Unit_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Acct_List"))
                        {
                            save_Task_Result = Save_Acct_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Appr_List"))
                        {
                            save_Task_Result = Save_Appr_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Appr_Steps"))
                        {
                            save_Task_Result = Save_Appr_Steps();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_RegConfig_List"))
                        {
                            save_Task_Result = Save_RegConfig_List();

                            return save_Task_Result;

                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Col_Config_List"))
                        {
                            save_Task_Result = Save_Col_Config_List();

                            return save_Task_Result;

                        }
                        break;
					                    
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var changed_Task_Result = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Cube_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Cube_Config("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Cube_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Cube_Config("Update");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Model_Updates"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model("Update");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Group"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model_Grp("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Model_Group"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model_Grp("Update");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Grp_Seq"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model_Grp_Seq("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Model_Grp_Seq"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model_Grp_Seq("Update");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Grp_Model_Assignments"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Model_Grp_Assign("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Calc_Unit_Assign"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Calc_Unit_Assign("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Appr_Step"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Sve Approval step: ");
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Save_Appr_Step("New");
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Act_Appr_Step_Config"))
                        {

                            // Implement Dashboard Component Selection Changed logic here.
                            var runType = args.NameValuePairs.XFGetValue("RunType");

                            //							var runType = AddUpdateDBName == "0b3b2a2_FMM_Appr_Steps_Activities_Row2b_Header" ? "Update" : "New";

                            BRApi.ErrorLog.LogMessage(si, "Hit Sve Approval step activity: " + runType);

                            changed_Task_Result = Save_Act_Appr_Step_Config(runType);
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Bulk_Calc_Unit"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Process_Bulk_Calc_Unit();
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Copy_Cube_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Copy_Cube_Config(ref changed_Task_Result);
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Copy_Reg_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Process_Bulk_Calc_Unit();
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Copy_Act_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Copy_Act_Config(ref changed_Task_Result);
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Model_Copy"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Process_Bulk_Calc_Unit();
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Calc_Copy"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Calc_Copy(ref changed_Task_Result);
                            return changed_Task_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Copy_Model_Grp_Config"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Task_Result = Process_Bulk_Calc_Unit();
                            return changed_Task_Result;
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


        #region "Save TED Inputs"

        #region "Model TED Inputs"

        //Save Calc Config Rows Function - New Adds will also insert into FMM_Cell 
        private XFSqlTableEditorSaveDataTaskResult Save_Calc_Config_Rows()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                DimChecker checker = new DimChecker();
                var createNewDestCell = false;
                var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                var Act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                var Model_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_ID", "0").XFConvertToInt();
                var Calc_ID = 0;
                var OS_Cell_ID = 0;

                // Create SQL connection and adapters
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();

                    var sqa_fmm_calc_config = new SQA_FMM_Calc_Config(si, connection);
                    var FMM_Calc_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();
                    var sqa_fmm_cell = new SQA_FMM_Cell(si, connection);
                    var FMM_Cell_DT = new DataTable();
                    var sqa_Cell = new SqlDataAdapter();
                    Duplicate_Calc_Config(Cube_ID, Act_ID, Model_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the existing FMM_Calc_Config data
                    string sql = @"
		                SELECT * 
		                FROM FMM_Calc_Config 
		                WHERE Cube_ID = @Cube_ID AND Act_ID = @Act_ID AND Model_ID = @Model_ID";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                        new SqlParameter("@Model_ID", SqlDbType.Int) { Value = Model_ID }
                    };

                    sqa_fmm_calc_config.Fill_FMM_Calc_Config_DT(si, sqa,FMM_Calc_Config_DT, sql, sqlparams);
                    FMM_Calc_Config_DT.PrimaryKey = new DataColumn[] { FMM_Calc_Config_DT.Columns["Calc_ID"] };
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            createNewDestCell = true;
                            var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);

                            // Get the max ID for the "FMM_Calc_Config" table
                            Calc_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "Calc_ID");
                            GBL_Calc_ID = Calc_ID;

                            xfRow.ModifiedDataRow.SetValue("Calc_ID", Calc_ID, XFDataType.Int16);
                            xfRow.ModifiedDataRow.SetValue("Status", "Build", XFDataType.Text);
                            xfRow.ModifiedDataRow.SetValue("Create_Date", DateTime.Now, XFDataType.DateTime);
                            xfRow.ModifiedDataRow.SetValue("Create_User", si.UserName, XFDataType.Text);
                            xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                            xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);

                            // Add new row to DataTable
                            var new_config_Row = FMM_Calc_Config_DT.NewRow();
                            foreach (DataColumn dc in FMM_Calc_Config_DT.Columns)
                            {
                                if (xfRow.ModifiedDataRow.Items.ContainsKey(dc.ColumnName))
                                {
                                    if (dc.ColumnName.XFContainsIgnoreCase("_DimType"))
                                    {
                                        new_config_Row[dc.ColumnName] = checker.CheckForDim(new_config_Row, dc.ColumnName.Replace("_DimType", "_Filter"));
                                    }
                                    else if (dc.ColumnName.XFContainsIgnoreCase("_Filter"))
                                    {
                                        new_config_Row[dc.ColumnName] = checker.GetSrcDestFilter(new_config_Row, dc.ColumnName.Replace("_Filter", "_Filter"));
                                    }
                                    else
                                    {
                                        new_config_Row[dc.ColumnName] = xfRow.ModifiedDataRow.Items[dc.ColumnName];
                                    }
                                }
                                else
                                {
                                    new_config_Row[dc.ColumnName] = DBNull.Value;
                                }
                            }

                            FMM_Calc_Config_DT.Rows.Add(new_config_Row);
                            Duplicate_Calc_Config(Cube_ID, Act_ID, Model_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);


                            //begin FMM_cell updates logic
                            OS_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Cell", "OS_Cell_ID");

                            // Fill the DataTable with the current data from FMM_Cell
                            var sql_Cell = @"
												SELECT * 
												FROM FMM_Cell 
												WHERE Calc_ID = @Calc_ID 
												AND OS_Cell_ID = @OS_Cell_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams_Cell = new SqlParameter[]
                            {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = Calc_ID },
                            new SqlParameter("@OS_Cell_ID", SqlDbType.Int) { Value = OS_Cell_ID }
                            };

                            sqa_fmm_cell.Fill_FMM_Cell_DT(si, sqa_Cell, FMM_Cell_DT, sql_Cell, sqlparams_Cell);

                            BRApi.ErrorLog.LogMessage(si, "New row kickoff for OSCalc: " + Calc_ID + " OSCalcDestCellID: " + OS_Cell_ID);
                            var new_Row = FMM_Cell_DT.NewRow();
                            foreach (DataColumn column in FMM_Cell_DT.Columns)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit " + column.ColumnName + "|" + Cube_ID + "|" + Calc_ID + "|" + OS_Cell_ID);
                                if (column.ColumnName == "Cube_ID")
                                {
                                    new_Row["Cube_ID"] = Cube_ID;
                                }
                                else if (column.ColumnName == "Act_ID")
                                {
                                    new_Row["Act_ID"] = Act_ID;
                                }
                                else if (column.ColumnName == "Model_ID")
                                {
                                    new_Row["Model_ID"] = Model_ID;
                                }
                                else if (column.ColumnName == "Calc_ID")
                                {
                                    new_Row["Calc_ID"] = Calc_ID;
                                }
                                else if (column.ColumnName == "OS_Cell_ID")
                                {
                                    new_Row["OS_Cell_ID"] = OS_Cell_ID;
                                }
                                else
                                {
                                    new_Row[column.ColumnName] = DBNull.Value;
                                }
                            }
                            // Set other column values for the new row as needed
                            FMM_Cell_DT.Rows.Add(new_Row);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            // Find the row to update and modify its data
                            var rowsToUpdate = FMM_Calc_Config_DT.Select($"Calc_ID = {xfRow.ModifiedDataRow["Calc_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                GBL_Calc_ID = rowToUpdate["Calc_ID"] != DBNull.Value ? Convert.ToInt32(rowToUpdate["Calc_ID"]) : 0;
                                // Set the updated fields
                                xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                                xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                                foreach (DataColumn column in FMM_Calc_Config_DT.Columns)
                                {
                                    if (xfRow.ModifiedDataRow.Items.ContainsKey(column.ColumnName))
                                    {
                                        if (column.ColumnName.XFContainsIgnoreCase("_DimType"))
                                        {
                                            rowToUpdate[column.ColumnName] = checker.CheckForDim(rowToUpdate, column.ColumnName.Replace("_DimType", "_Filter"));
                                        }
                                        else if (column.ColumnName.XFContainsIgnoreCase("_Filter"))
                                        {
                                            rowToUpdate[column.ColumnName] = checker.GetSrcDestFilter(rowToUpdate, column.ColumnName.Replace("_Filter", "_Filter"));
                                        }
                                        else
                                        {
                                            rowToUpdate[column.ColumnName] = xfRow.ModifiedDataRow.Items[column.ColumnName];
                                        }
                                    }
                                }
                                Duplicate_Calc_Config(Cube_ID, Act_ID, Model_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            // Find the row
                            var Deleted_Row = FMM_Calc_Config_DT.Rows.Find(xfRow.OriginalDataRow["Calc_ID"]);

                            if (Deleted_Row != null)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit Delete SRC: ");
                                // Delete the row if found
                                Deleted_Row.Delete();
                                Duplicate_Calc_Config(Cube_ID, Act_ID, Model_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Calc_Config = GBL_Calc_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    GBL_Duplicate_Calc_Config = dup_Calc_Config.Count > 0;

                    if (GBL_Duplicate_Calc_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the database with the changes made to the FMM_Calc_Config DataTable
                        sqa_fmm_calc_config.Update_FMM_Calc_Config(si, FMM_Calc_Config_DT, sqa);

                        // Update the FMM_Cell table based on the changes made to the DataTable
                        if (createNewDestCell == true)
                        {
                            sqa_fmm_cell.Update_FMM_Cell(si, FMM_Cell_DT, sqa_Cell);
                        }
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true;

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Cell_Rows()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                GBL_Calc_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Calc_ID", "0").XFConvertToInt();


                // Create SQL connection and adapters
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();

                    var sqa_fmm_cell = new SQA_FMM_Cell(si, connection);
                    var FMM_Cell_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Cell
                    string sql = @"
										SELECT * 
										FROM FMM_Cell 
										WHERE Calc_ID = @Calc_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID }
                    };

                    sqa_fmm_cell.Fill_FMM_Cell_DT(si, sqa,FMM_Cell_DT, sql, sqlparams);
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Find the row to update and modify its data
                        var rowsToUpdate = FMM_Cell_DT.Select($"Calc_ID = {xfRow.ModifiedDataRow["Calc_ID"]}");
                        if (rowsToUpdate.Length > 0)
                        {
                            var rowToUpdate = rowsToUpdate[0];
                            // Set the updated fields
                            xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                            xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                            foreach (DataColumn column in FMM_Cell_DT.Columns)
                            {
                                if (xfRow.ModifiedDataRow.Items.ContainsKey(column.ColumnName))
                                {
                                    rowToUpdate[column.ColumnName] = xfRow.ModifiedDataRow.Items[column.ColumnName];
                                }
                            }
                        }
                    }

                    sqa_fmm_cell.Update_FMM_Cell(si, FMM_Cell_DT, sqa);
                }

                // Set return value
                save_Task_Result.IsOK = true;
                save_Task_Result.ShowMessageBox = false;
                save_Task_Result.Message = String.Empty;
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.


                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        private XFSqlTableEditorSaveDataTaskResult Save_Src_Cell_Rows()
        {
            try
            {
                var Calc_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Calc_ID");
                GBL_Calc_ID = Convert.ToInt32(Calc_ID);
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqlAdapterMcmSrcCell = new SQA_FMM_Src_Cell(si, connection);
                    var FMM_Src_Cell_DT = new DataTable();
                    var sqlSrcDataAdapter = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Cell
                    string src_selectQuery = @"
										SELECT * 
										FROM FMM_Src_Cell 
										WHERE Calc_ID = @Calc_ID";
                    // Create an array of SqlParameter objects
                    var src_parameters = new SqlParameter[]
                    {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID },
                    };

                    sqlAdapterMcmSrcCell.Fill_FMM_Src_Cell_DT(si, sqlSrcDataAdapter, FMM_Src_Cell_DT, src_selectQuery, src_parameters);

                    FMM_Src_Cell_DT.PrimaryKey = new DataColumn[] { FMM_Src_Cell_DT.Columns["Cell_ID"] };
                    // Save the Calc Config data rows
                    var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                    var Calc_Config_Dict = new Dictionary<int, string>();
                    int Cell_ID = 0;
                    string SaveRowType = "Update";

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        #region "Insert New Src Cell Rows"
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            SaveRowType = "Insert";


                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);

                            // Example: Get the max ID for the "FMM_Calc_Config" table
                            Cell_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Src_Cell", "Cell_ID");

                            BRApi.ErrorLog.LogMessage(si, "HitInsert SRC: " + Cell_ID + "-" + GBL_Calc_ID);
                            xfRow.ModifiedDataRow["Cell_ID"] = Cell_ID;

                            // Create a new row and populate it with data
                            var newRow = FMM_Src_Cell_DT.NewRow();

                            foreach (DataColumn column in FMM_Src_Cell_DT.Columns)
                            {
                                if (xfRow.ModifiedDataRow.Items.ContainsKey(column.ColumnName))
                                {
                                    // Check if the column's data type is string
                                    if (column.DataType == typeof(string))
                                    {
                                        // If it's a string, check if the value is not nullor empty string and replace single quotes
                                        var originalValue = xfRow.ModifiedDataRow[column.ColumnName];
                                        if (originalValue != null && !String.Empty.Equals(originalValue))
                                        {
                                            string stringValue = originalValue.ToString();
                                            // Replace single quotes with two single quotes for SQL string safety
                                            newRow[column.ColumnName] = stringValue.Replace("'", "''");
                                        }
                                        else
                                        {
                                            // If the original value is null or it is an empty string, assign it directly
                                            newRow[column.ColumnName] = DBNull.Value;
                                        }
                                    }
                                    else
                                    {
                                        // For non-string columns, just assign the value
                                        newRow[column.ColumnName] = xfRow.ModifiedDataRow[column.ColumnName];
                                    }
                                }
                            }

                            // set updated and created users for new row as they are not contained within the sql table editor
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            BRApi.ErrorLog.LogMessage(si, "HitEnd Insert SRC: " + Cell_ID);
                            FMM_Src_Cell_DT.Rows.Add(newRow);
                            BRApi.ErrorLog.LogMessage(si, "HitEnd 2Insert SRC: " + Cell_ID);
                        }
                        #endregion
                        #region "Update Src Cell Row"
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            // Find the row to update and modify its data
                            var rowsToUpdate = FMM_Src_Cell_DT.Select($"Cell_ID = {xfRow.ModifiedDataRow["Cell_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                foreach (DataColumn column in FMM_Src_Cell_DT.Columns)
                                {
                                    if (xfRow.ModifiedDataRow.Items.ContainsKey(column.ColumnName))
                                    {
                                        // Check if the column's data type is string
                                        if (column.DataType == typeof(string))
                                        {
                                            // If it's a string, check if the value is not null or empty string and replace single quotes
                                            var originalValue = xfRow.ModifiedDataRow[column.ColumnName];
                                            if (originalValue != null && !String.Empty.Equals(originalValue))
                                            {
                                                string stringValue = originalValue.ToString();
                                                // Replace single quotes with two single quotes for SQL string safety
                                                rowToUpdate[column.ColumnName] = stringValue.Replace("'", "''");
                                            }
                                            else
                                            {
                                                if (!column.ColumnName.Contains("User"))
                                                {
                                                    // If the original value is null and it is not the update user or create user columns, assign it directly
                                                    rowToUpdate[column.ColumnName] = DBNull.Value;
                                                }
                                                else
                                                {
                                                    xfRow.ModifiedDataRow.SetValue("Update_Date", DateTime.Now, XFDataType.DateTime);
                                                    xfRow.ModifiedDataRow.SetValue("Update_User", si.UserName, XFDataType.Text);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // For non-string columns, just assign the value
                                            rowToUpdate[column.ColumnName] = xfRow.ModifiedDataRow[column.ColumnName];
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            // Find the row
                            var Deleted_Row = FMM_Src_Cell_DT.Rows.Find(xfRow.OriginalDataRow["Cell_ID"]);

                            if (Deleted_Row != null)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit Delete SRC: ");
                                // Delete the row if found
                                Deleted_Row.Delete();
                            }
                        }

                        if (xfRow.InsertUpdateOrDelete != DbInsUpdateDelType.Delete)
                        {
                            //string CalcString = xfRow.ModifiedDataRow["OSCalcOpenParens"].ToString().Trim() + "|" + xfRow.ModifiedDataRow["OSCalcMathOperator"].ToString().Trim() + " " + xfRow.ModifiedDataRow["OSDynamicCalcScript"].ToString().Trim() + "|" + xfRow.ModifiedDataRow["OSCalcCloseParens"].ToString().Trim();
                        }
                    }

                    sqlAdapterMcmSrcCell.Update_FMM_Src_Cell(si, FMM_Src_Cell_DT, sqlSrcDataAdapter);


                    //                    if (GBL_BalCalc == "Unbalanced" || GBL_BalCalc == "UnbalAlloc")
                    //                    {
                    //                        foreach (DataRow Row in FMM_Cell_DT.Rows)
                    //                        {
                    //                            //UpdateUnbalSrcCellColumns(si,globals,api,args,Row);
                    //                        }
                    //                    }

                    // Additional logic continues...
                    // The full conversion continues in the same fashion as the snippet above.

                    // Set return value
                    save_Task_Result.IsOK = true;
                    save_Task_Result.ShowMessageBox = false;
                    save_Task_Result.Message = String.Empty;
                    save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                    return save_Task_Result;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Cube Config TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult Save_Activity_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                int Act_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqa_fmm_act_config = new SQA_FMM_Act_Config(si, connection);
                    var FMM_Act_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    Duplicate_Activity_List(Cube_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    string sql = @"
		                SELECT * 
		                FROM FMM_Act_Config
						WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                    };
                    sqa_fmm_act_config.Fill_FMM_Act_Config_DT(si, sqa,FMM_Act_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            {
                                var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                                Act_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Act_Config", "Act_ID");

                                var newRow = FMM_Act_Config_DT.NewRow();
                                newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                                newRow["Act_ID"] = Act_ID;
                                newRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                                newRow["Calc_Type"] = (string)xfRow.ModifiedDataRow.Items["Calc_Type"];
                                newRow["Status"] = "Build";
                                newRow["Create_Date"] = DateTime.Now;
                                newRow["Create_User"] = si.UserName;
                                newRow["Update_Date"] = DateTime.Now;
                                newRow["Update_User"] = si.UserName;
                                FMM_Act_Config_DT.Rows.Add(newRow);
                                Duplicate_Activity_List(Cube_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Act_Config_DT.Select($"Act_ID = {(int)xfRow.ModifiedDataRow["Act_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit");
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Activity_List(Cube_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Act_Config_DT.Select($"Act_ID = {(int)xfRow.OriginalDataRow["Act_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Activity_List(Cube_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Activity_Config = GBL_Activity_List_Dict
                                             .GroupBy(x => x.Value)
                                             .Where(g => g.Count() > 1)
                                             .Select(g => g.Key)
                                             .ToList();

                    GBL_Duplicate_Activity = dup_Activity_Config.Count > 0;

                    if (GBL_Duplicate_Activity)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Activity Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        sqa_fmm_act_config.Update_FMM_Act_Config(si, FMM_Act_Config_DT, sqa);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Calc_Unit_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                int Calc_Unit_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqa_fmm_calc_unit_config = new SQA_FMM_Calc_Unit_Config(si, connection);
                    var FMM_Calc_Unit_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    Duplicate_Calc_Unit_Config(Cube_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    string sql = @"
		                SELECT * 
		                FROM FMM_Calc_Unit_Config
						WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                    };
                    sqa_fmm_calc_unit_config.Fill_FMM_Calc_Unit_Config_DT(si, sqa,FMM_Calc_Unit_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                            Calc_Unit_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");

                            var newRow = FMM_Calc_Unit_Config_DT.NewRow();
                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Calc_Unit_ID"] = Calc_Unit_ID;
                            newRow["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                            newRow["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            FMM_Calc_Unit_Config_DT.Rows.Add(newRow);
                            Duplicate_Calc_Unit_Config(Cube_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Calc_Unit_Config_DT.Select($"Calc_Unit_ID = {(int)xfRow.ModifiedDataRow["Calc_Unit_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                                rowToUpdate["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Calc_Unit_Config(Cube_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Calc_Unit_Config_DT.Select($"Calc_Unit_ID = {(int)xfRow.OriginalDataRow["Calc_Unit_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Calc_Unit_Config(Cube_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Calc_Unit_Config = GBL_Calc_Unit_Config_Dict
                                                 .GroupBy(x => x.Value)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

                    GBL_Duplicate_Calc_Unit_Config = dup_Calc_Unit_Config.Count > 0;

                    if (GBL_Duplicate_Calc_Unit_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate WF DU Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        sqa_fmm_calc_unit_config.Update_FMM_Calc_Unit_Config(si, FMM_Calc_Unit_Config_DT, sqa);
                    }
                }

                // Set return value
                //                save_Task_Result.IsOK = true;
                //                save_Task_Result.ShowMessageBox = false;
                //                save_Task_Result.Message = String.Empty;
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Unit & Acct TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult Save_Unit_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                int Unit_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var SQA_FMM_Unit_Config = new SQA_FMM_Unit_Config(si, connection);
                    var FMM_Unit_Config_DT = new DataTable();
                    var sqlDataAdapter = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var Act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    Duplicate_Unit_Config(Cube_ID, Act_ID, "Initiate", ref save_Task_Result);
                    BRApi.ErrorLog.LogMessage(si, "Hit: " + Cube_ID + " | " + Act_ID);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    string selectQuery = @"
		                SELECT * 
		                FROM FMM_Unit_Config
						WHERE Cube_ID = @Cube_ID
						AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID }
                    };
                    SQA_FMM_Unit_Config.Fill_FMM_Unit_Config_DT(si, sqlDataAdapter, FMM_Unit_Config_DT, selectQuery, parameters);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + Cube_ID + " | " + Act_ID);
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                            Unit_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Unit_Config", "Unit_ID");
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + Cube_ID + " | " + Unit_ID);
                            var newRow = FMM_Unit_Config_DT.NewRow();
                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            newRow["Unit_ID"] = Unit_ID;
                            newRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            newRow["Status"] = (string)xfRow.ModifiedDataRow.Items["Status"];
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            FMM_Unit_Config_DT.Rows.Add(newRow);
                            Duplicate_Unit_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Unit_Config_DT.Select($"Unit_ID = {(int)xfRow.ModifiedDataRow["Unit_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Unit_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Unit_Config_DT.Select($"Unit_ID = {(int)xfRow.OriginalDataRow["Unit_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Unit_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Unit_Config = GBL_Unit_Config_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    GBL_Duplicate_Unit_Config = dup_Unit_Config.Count > 0;

                    if (GBL_Duplicate_Unit_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        SQA_FMM_Unit_Config.Update_FMM_Unit_Config(si, FMM_Unit_Config_DT, sqlDataAdapter);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Acct_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                int OS_Activity_Unit_Acct_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var SQA_FMM_Acct_Config = new SQA_FMM_Acct_Config(si, connection);
                    var FMM_Acct_Config_DT = new DataTable();
                    var sqlDataAdapter = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var Act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    var Unit_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Unit_ID", "0").XFConvertToInt();
                    Duplicate_Acct_Config(Cube_ID, Act_ID, Unit_ID, "Initiate", ref save_Task_Result);
                    // Fill the DataTable with the current data from FMM_Cell
                    string selectQuery = @"
										SELECT * 
										FROM FMM_Acct_Config
										WHERE Cube_ID = @Cube_ID
										AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID }
                    };

                    SQA_FMM_Acct_Config.Fill_FMM_Acct_Config_DT(si, sqlDataAdapter, FMM_Acct_Config_DT, selectQuery, parameters);


                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);

                            // Example: Get the max ID for the "FMM_Calc_Config" table
                            OS_Activity_Unit_Acct_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Acct_Config", "Acct_ID");

                            var newRow = FMM_Acct_Config_DT.NewRow();
                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            newRow["Unit_ID"] = (int)xfRow.ModifiedDataRow["Unit_ID"];
                            newRow["Acct_ID"] = OS_Activity_Unit_Acct_ID;
                            newRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            newRow["Acct_Map_Req"] = (bool)xfRow.ModifiedDataRow.Items["Acct_Map_Req"];
                            newRow["Acct_Map_Type"] = (string)xfRow.ModifiedDataRow.Items["Acct_Map_Type"];
                            newRow["Acct_Map_Loc"] = (string)xfRow.ModifiedDataRow.Items["Acct_Map_Loc"];
                            newRow["Acct_Map_Logic"] = (string)xfRow.ModifiedDataRow.Items["Acct_Map_Logic"];
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            // Set other column values for the new row as needed
                            FMM_Acct_Config_DT.Rows.Add(newRow);
                            Duplicate_Acct_Config(Cube_ID, Act_ID, Unit_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {

                            // Find the row to update and modify its data
                            var rowsToUpdate = FMM_Acct_Config_DT.Select($"Acct_ID = {(int)xfRow.ModifiedDataRow["Acct_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Acct_Map_Req"] = (bool)xfRow.ModifiedDataRow["Acct_Map_Req"];
                                rowToUpdate["Acct_Map_Type"] = (string)xfRow.ModifiedDataRow.Items["Acct_Map_Type"];
                                rowToUpdate["Acct_Map_Loc"] = (string)xfRow.ModifiedDataRow.Items["Acct_Map_Loc"];
                                rowToUpdate["Acct_Map_Logic"] = (string)xfRow.ModifiedDataRow["Acct_Map_Logic"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Acct_Config(Cube_ID, Act_ID, Unit_ID, "Update Row", ref save_Task_Result, "Update", xfRow);

                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Acct_Config_DT.Select($"Acct_ID = {(int)xfRow.OriginalDataRow["Acct_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Acct_Config(Cube_ID, Act_ID, Unit_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }

                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Acct_Config = GBL_Acct_Config_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    GBL_Duplicate_Acct_Config = dup_Acct_Config.Count > 0;

                    if (GBL_Duplicate_Acct_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Acct Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        SQA_FMM_Acct_Config.Update_FMM_Acct_Config(si, FMM_Acct_Config_DT, sqlDataAdapter);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Register Config TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult Save_RegConfig_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var New_Reg_List = new List<int>();
                int OS_Reg_Config_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var SQA_FMM_Reg_Config = new SQA_FMM_Reg_Config(si, connection);
                    var FMM_Reg_Config_DT = new DataTable();
                    var sqlDataAdapter = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var Act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    Duplicate_Reg_Config(Cube_ID, Act_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    string selectQuery = @"
		                SELECT * 
		                FROM FMM_Reg_Config
						WHERE Cube_ID = @Cube_ID
						AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID }
                    };
                    SQA_FMM_Reg_Config.Fill_FMM_Reg_Config_DT(si, sqlDataAdapter, FMM_Reg_Config_DT, selectQuery, parameters);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + Cube_ID + " | " + Act_ID);
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                            OS_Reg_Config_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Reg_Config", "Reg_Config_ID");
                            New_Reg_List.Add(OS_Reg_Config_ID);
                            var newRow = FMM_Reg_Config_DT.NewRow();
                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            newRow["Reg_Config_ID"] = OS_Reg_Config_ID;
                            newRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            newRow["Time_Phasing"] = (string)xfRow.ModifiedDataRow.Items["Time_Phasing"];
                            newRow["Start_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["Start_Dt_Src"];
                            newRow["End_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["End_Dt_Src"];
                            newRow["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            FMM_Reg_Config_DT.Rows.Add(newRow);
                            Duplicate_Reg_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Reg_Config_DT.Select($"Reg_Config_ID = {(int)xfRow.ModifiedDataRow["Reg_Config_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Time_Phasing"] = (string)xfRow.ModifiedDataRow["Time_Phasing"];
                                rowToUpdate["Start_Dt_Src"] = (string)xfRow.ModifiedDataRow["Start_Dt_Src"];
                                rowToUpdate["End_Dt_Src"] = (string)xfRow.ModifiedDataRow["End_Dt_Src"];
                                rowToUpdate["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Reg_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Reg_Config_DT.Select($"Reg_Config_ID = {(int)xfRow.OriginalDataRow["Reg_Config_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Reg_Config(Cube_ID, Act_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Reg_Config = GBL_Register_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    GBL_Duplicate_Reg_Config = dup_Reg_Config.Count > 0;

                    if (GBL_Duplicate_Reg_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Register Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        SQA_FMM_Reg_Config.Update_FMM_Reg_Config(si, FMM_Reg_Config_DT, sqlDataAdapter);
                    }
                    foreach (var registerID in New_Reg_List)
                    {
                        // Call the function and pass the current integer
                        Insert_Col_Default_Rows(Cube_ID, Act_ID, registerID);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Col_Config_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var SQA_FMM_Col_Config = new SQA_FMM_Col_Config(si, connection);
                    var FMM_Col_Config_DT = new DataTable();
                    var sqlDataAdapter = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var Act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    var Register_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Reg_Config_ID", "0").XFConvertToInt();
                    Duplicate_Col_Config(Cube_ID, Act_ID, Register_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    string selectQuery = @"
		                SELECT * 
		                FROM FMM_Col_Config
						WHERE Cube_ID = @Cube_ID
						AND Act_ID = @Act_ID
						AND Reg_Config_ID = @Reg_Config_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                        new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = Register_ID }
                    };
                    SQA_FMM_Col_Config.Fill_FMM_Col_Config_DT(si, sqlDataAdapter, FMM_Col_Config_DT, selectQuery, parameters);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + Cube_ID + " | " + Act_ID);
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record updates
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Col_Config_DT.Select($"Col_ID = {(int)xfRow.ModifiedDataRow["Col_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["InUse"] = (bool)xfRow.ModifiedDataRow["InUse"];
                                rowToUpdate["Required"] = (bool)xfRow.ModifiedDataRow["Required"];
                                rowToUpdate["Updates"] = (bool)xfRow.ModifiedDataRow["Updates"];
                                rowToUpdate["Alias"] = (string)xfRow.ModifiedDataRow["Alias"];
                                // If InUse is false, set the Order column to 99
                                if (!(bool)xfRow.ModifiedDataRow["InUse"])
                                {
                                    rowToUpdate["Order"] = 99;
                                }
                                else
                                {
                                    // Otherwise, set the Order column to the updated value from the ModifiedDataRow
                                    rowToUpdate["Order"] = (int)xfRow.ModifiedDataRow["Order"];
                                }
                                rowToUpdate["Default"] = (string)xfRow.ModifiedDataRow["Default"];
                                rowToUpdate["Param"] = (string)xfRow.ModifiedDataRow["Param"];
                                rowToUpdate["Format"] = (string)xfRow.ModifiedDataRow["Format"];
                                rowToUpdate["Filter_Param"] = (string)xfRow.ModifiedDataRow["Filter_Param"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Col_Config(Cube_ID, Act_ID, Register_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Col_Config = GBL_Col_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    GBL_Duplicate_Col_Config = dup_Col_Config.Count > 0;

                    if (GBL_Duplicate_Col_Config)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Col Config entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        SQA_FMM_Col_Config.Update_FMM_Col_Config(si, FMM_Col_Config_DT, sqlDataAdapter);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Activity Approval Inputs"
        private XFSqlTableEditorSaveDataTaskResult Save_Appr_List()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                int OS_Appr_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqa_fmm_appr_config = new SQA_FMM_Appr_Config(si, connection);
                    var FMM_Appr_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    BRApi.ErrorLog.LogMessage(si, "Hit" + Cube_ID);
                    Duplicate_Appr_Config(Cube_ID, "Initiate", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Cell
                    string sql = @"
						                SELECT * 
						                FROM FMM_Appr_Config
										WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                    };
                    sqa_fmm_appr_config.Fill_FMM_Appr_Config_DT(si, sqa,FMM_Appr_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                            OS_Appr_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Appr_Config", "Appr_ID");

                            var newRow = FMM_Appr_Config_DT.NewRow();
                            BRApi.ErrorLog.LogMessage(si, "Hit" + (int)xfRow.ModifiedDataRow["Cube_ID"]);
                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Appr_ID"] = OS_Appr_ID;
                            newRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            FMM_Appr_Config_DT.Rows.Add(newRow);
                            Duplicate_Appr_Config(Cube_ID, "Update Row", ref save_Task_Result, "Insert", xfRow);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Appr_Config_DT.Select($"Appr_ID = {(int)xfRow.ModifiedDataRow["Appr_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Appr_Config(Cube_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Appr_Config_DT.Select($"Appr_ID = {(int)xfRow.OriginalDataRow["Appr_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Appr_Config(Cube_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Approvals = GBL_Appr_Dict
                                        .GroupBy(x => x.Value)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

                    GBL_Duplicate_Approval = dup_Approvals.Count > 0;

                    if (GBL_Duplicate_Approval)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Activity Approval entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        sqa_fmm_appr_config.Update_FMM_Appr_Config(si, FMM_Appr_Config_DT, sqa);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private XFSqlTableEditorSaveDataTaskResult Save_Appr_Steps()
        {
            try
            {
                var save_Task_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                int OS_Appr_Step_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqa_fmm_appr_step_config = new SQA_FMM_Appr_Step_Config(si, connection);
                    var FMM_Appr_Step_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();
                    var Cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var Appr_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Appr_ID", "0").XFConvertToInt();
                    Duplicate_Appr_Step_Config(Cube_ID, Appr_ID, "Update Row", ref save_Task_Result);

                    // Fill the DataTable with the current data from FMM_Cell
                    string sql = @"
					                SELECT * 
					                FROM FMM_Appr_Step_Config
									WHERE Cube_ID = @Cube_ID
									AND Appr_ID = @Appr_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID }
                    };
                    sqa_fmm_appr_step_config.Fill_FMM_Appr_Step_Config_DT(si, sqa,FMM_Appr_Step_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            // Create a new row and populate its fields
                            var newRow = FMM_Appr_Step_Config_DT.NewRow();

                            newRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            newRow["Appr_ID"] = (int)xfRow.ModifiedDataRow["Appr_ID"];
                            newRow["Appr_Step_ID"] = (int)xfRow.ModifiedDataRow["Appr_Step_ID"];
                            newRow["WFProfile_Step"] = (string)xfRow.ModifiedDataRow["WFProfile_Step"];
                            newRow["Step_Num"] = (int)xfRow.ModifiedDataRow["Step_Num"];
                            newRow["Appr_User_Group"] = (string)xfRow.ModifiedDataRow["Appr_User_Group"];
                            newRow["Appr_Logic"] = (string)xfRow.ModifiedDataRow["Appr_Logic"];
                            newRow["Appr_Item"] = (string)xfRow.ModifiedDataRow["Appr_Item"];
                            newRow["Appr_Level"] = (int)xfRow.ModifiedDataRow["Appr_Level"];
                            newRow["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                            newRow["Init_Status"] = (string)xfRow.ModifiedDataRow["Init_Status"];
                            newRow["Appr_Status"] = (string)xfRow.ModifiedDataRow["Appr_Status"];
                            newRow["Rej_Status"] = (string)xfRow.ModifiedDataRow["Rej_Status"];
                            newRow["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;

                            // Add the row to the DataTable
                            FMM_Appr_Step_Config_DT.Rows.Add(newRow);

                            // Check for duplicates before saving
                            Duplicate_Appr_Step_Config(Cube_ID, Appr_ID, "Insert Row", ref save_Task_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Appr_Step_Config_DT.Select($"Appr_Step_ID = {(int)xfRow.ModifiedDataRow["Appr_Step_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Step_Num"] = (int)xfRow.ModifiedDataRow["Step_Num"];
                                rowToUpdate["Appr_User_Group"] = (string)xfRow.ModifiedDataRow["Appr_User_Group"];
                                rowToUpdate["Appr_Logic"] = (string)xfRow.ModifiedDataRow["Appr_Logic"];
                                rowToUpdate["Appr_Item"] = (string)xfRow.ModifiedDataRow["Appr_Item"];
                                rowToUpdate["Appr_Level"] = (int)xfRow.ModifiedDataRow["Appr_Level"];
                                rowToUpdate["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                                rowToUpdate["Init_Status"] = (string)xfRow.ModifiedDataRow["Init_Status"];
                                rowToUpdate["Appr_Status"] = (string)xfRow.ModifiedDataRow["Appr_Status"];
                                rowToUpdate["Rej_Status"] = (string)xfRow.ModifiedDataRow["Rej_Status"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Appr_Step_Config(Cube_ID, Appr_ID, "Update Row", ref save_Task_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Appr_Step_Config_DT.Select($"Appr_Step_ID = {(int)xfRow.OriginalDataRow["Appr_Step_ID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Appr_Step_Config(Cube_ID, Appr_ID, "Update Row", ref save_Task_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Appr_Steps = GBL_Appr_Step_Dict
                                        .GroupBy(x => x.Value)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

                    GBL_Duplicate_Appr_Step = dup_Appr_Steps.Count > 0;

                    if (GBL_Duplicate_Appr_Step)
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.ShowMessageBox = true;
                        save_Task_Result.Message += "Duplicate Activity Approval entries found during the operation.";
                    }
                    else
                    {
                        save_Task_Result.IsOK = true;
                        save_Task_Result.ShowMessageBox = false;
                        // Update the FMM_Cell table based on the changes made to the DataTable
                        sqa_fmm_appr_step_config.Update_FMM_Appr_Step_Config(si, FMM_Appr_Step_Config_DT, sqa);
                    }
                }

                // Set return value
                save_Task_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #endregion

        #region "Save Data Helpers"	

        private void Update_Src_Cell_Columns(DataRow Data_Row)
        {
            var balances = new Dictionary<string, int>();
            var srcAttributes = new Dictionary<string, string>();
            var targetAttributes = new Dictionary<string, string>();
            var dim_token = new Dictionary<string, List<string>>();
            string src_Cell_Drill_Down = string.Empty;
            int src_Cell_Drill_Down_Count = 0;
            int src_Cell_Count = 0;
            bool balanced_Src_Row = true;
            string UnbalancedSrcCellBufferFilter = string.Empty;
            int unbalancedSrcCellBufferFilterCount = 0;

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sqlAdapterMcmSrcCell = new SQA_FMM_Src_Cell(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();
                var FMM_Src_Cell_DT = new DataTable();

                // Fill the DataTable with the current data from FMM_Cell
                string selectQuery = @"
		            SELECT * 
		            FROM FMM_Src_Cell
		            WHERE Calc_ID = @Calc_ID
					ORDER BY Cell_ID";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID }
                };

                connection.Open();

                sqlAdapterMcmSrcCell.Fill_FMM_Src_Cell_DT(si, sqlDataAdapter, FMM_Src_Cell_DT, selectQuery, parameters);

                string[] All_Dim_Types = { "Entity", "Cons", "Scenario", "Time", "View", "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                #region "All Dim Types"
                foreach (var Dim_Type in All_Dim_Types)
                {
                    switch (Dim_Type)
                    {
                        case "Entity":
                            dim_token[Dim_Type] = new List<string> { "E#" };
                            break;
                        case "Cons":
                            dim_token[Dim_Type] = new List<string> { "C#" };
                            break;
                        case "Scenario":
                            dim_token[Dim_Type] = new List<string> { "S#" };
                            break;
                        case "Time":
                            dim_token[Dim_Type] = new List<string> { "T#" };
                            break;
                        case "View":
                            dim_token[Dim_Type] = new List<string> { "V#" };
                            break;
                        case "Acct":
                            dim_token[Dim_Type] = new List<string> { "A#" };
                            break;
                        case "IC":
                            dim_token[Dim_Type] = new List<string> { "I#", "IC#" };
                            break;
                        case "Origin":
                            dim_token[Dim_Type] = new List<string> { "O#" };
                            break;
                        case "Flow":
                            dim_token[Dim_Type] = new List<string> { "F#" };
                            break;
                        case "UD1":
                            dim_token[Dim_Type] = new List<string> { "UD1#", "U1#" };
                            break;
                        case "UD2":
                            dim_token[Dim_Type] = new List<string> { "UD2#", "U2#" };
                            break;
                        case "UD3":
                            dim_token[Dim_Type] = new List<string> { "UD3#", "U3#" };
                            break;
                        case "UD4":
                            dim_token[Dim_Type] = new List<string> { "UD4#", "U4#" };
                            break;
                        case "UD5":
                            dim_token[Dim_Type] = new List<string> { "UD5#", "U5#" };
                            break;
                        case "UD6":
                            dim_token[Dim_Type] = new List<string> { "UD6#", "U6#" };
                            break;
                        case "UD7":
                            dim_token[Dim_Type] = new List<string> { "UD7#", "U7#" };
                            break;
                        case "UD8":
                            dim_token[Dim_Type] = new List<string> { "UD8#", "U8#" };
                            break;
                    }
                }
                #endregion
                string[] Supp_Dim_Types = { "Entity", "Cons", "Scenario", "Time" };
                string[] Core_Dim_Types = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                foreach (var FMM_Src_Cell_DT_Row in FMM_Src_Cell_DT.Rows.Cast<DataRow>())
                {
                    src_Cell_Drill_Down = string.Empty;
                    src_Cell_Drill_Down_Count = 0;
                    //If Cube												
                    src_Cell_Count += 1;
                    foreach (var Dim_Type in All_Dim_Types)
                    {
                        balances[Dim_Type] = 0;
                        srcAttributes[Dim_Type] = string.Empty;
                        targetAttributes[Dim_Type] = string.Empty;
                    }
                    foreach (var Supp_Dim_Type in Supp_Dim_Types)
                    {
                        string srcField = "" + Supp_Dim_Type;
                        string dim_token_1 = string.Empty;
                        string dim_token_2 = string.Empty;
                        List<string> dim_tokens = dim_token[Supp_Dim_Type];
                        if (dim_tokens.Count == 1)
                        {
                            dim_token_1 = dim_tokens[0];
                            dim_token_2 = dim_tokens[0];
                        }
                        else if (dim_tokens.Count > 1)
                        {
                            dim_token_1 = dim_tokens[0];
                            dim_token_2 = dim_tokens[1];
                        }
                        if (FMM_Src_Cell_DT_Row[srcField] != DBNull.Value)
                            if (FMM_Src_Cell_DT_Row[srcField].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 FMM_Src_Cell_DT_Row[srcField].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                srcAttributes[Supp_Dim_Type] = FMM_Src_Cell_DT_Row[srcField].ToString();
                                // Assuming srcCellDrillDown and srcCellDrillDownCount are initialized and declared elsewhere												
                                src_Cell_Drill_Down += src_Cell_Drill_Down_Count == 0 ? srcAttributes[Supp_Dim_Type] : ":" + srcAttributes[Supp_Dim_Type];
                                src_Cell_Drill_Down_Count += 1;
                            }
                    }
                    foreach (var Core_Dim_Type in Core_Dim_Types)
                    {
                        string targetField = "" + Core_Dim_Type;
                        string srcField = "" + Core_Dim_Type;
                        string filterField = "OS_" + Core_Dim_Type + "_Filter";
                        string balanceKey = Core_Dim_Type;
                        string dim_token_1 = string.Empty;
                        string dim_token_2 = string.Empty;
                        List<string> dim_tokens = dim_token[Core_Dim_Type];
                        if (dim_tokens.Count == 1)
                        {
                            dim_token_1 = dim_tokens[0];
                            dim_token_2 = dim_tokens[0];
                        }
                        else if (dim_tokens.Count > 1)
                        {
                            dim_token_1 = dim_tokens[0];
                            dim_token_2 = dim_tokens[1];
                        }
                        if (Data_Row[targetField] != DBNull.Value)
                            if (Data_Row[targetField].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                Data_Row[targetField].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                targetAttributes[Core_Dim_Type] = Data_Row[targetField].ToString();
                                balances[Core_Dim_Type] = 1;  // Assume resetting or incrementing depends on the logic												
                            }
                            else if (Data_Row[filterField] != DBNull.Value)
                                if (Data_Row[filterField].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    Data_Row[filterField].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    targetAttributes[Core_Dim_Type] = Data_Row[filterField].ToString();
                                    balances[Core_Dim_Type] = 0;  // Assume resetting or incrementing depends on the logic												
                                }
                        if (FMM_Src_Cell_DT_Row[srcField] != DBNull.Value)
                            if (FMM_Src_Cell_DT_Row[srcField].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 FMM_Src_Cell_DT_Row[srcField].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                srcAttributes[Core_Dim_Type] = FMM_Src_Cell_DT_Row[srcField].ToString();
                                // Assuming srcCellDrillDown and srcCellDrillDownCount are initialized and declared elsewhere												
                                src_Cell_Drill_Down += src_Cell_Drill_Down_Count == 0 ? srcAttributes[Core_Dim_Type] : ":" + srcAttributes[Core_Dim_Type];
                                src_Cell_Drill_Down_Count += 1;
                                balances[Core_Dim_Type] += 1;
                            }
                        if (balances[Core_Dim_Type] == 1)
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{Core_Dim_Type}_Override"] = "Common";
                            if (src_Cell_Count == 1)
                            {
                                GBL_BalCalc = "UnbalAlloc";
                            }
                            else
                            {
                                GBL_BalCalc = "Unbalanced";
                            }
                            balanced_Src_Row = false;
                        }
                        else
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{Core_Dim_Type}_Override"] = string.Empty;
                            if (Data_Row[$"OS_{Core_Dim_Type}_Filter"] != DBNull.Value)
                            {
                                string os_core_dimtype_Filter = Data_Row[$"OS_{Core_Dim_Type}_Filter"].ToString();
                                if (os_core_dimtype_Filter.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 || os_core_dimtype_Filter.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    UnbalancedSrcCellBufferFilter = "[" + os_core_dimtype_Filter + "]";
                                    unbalancedSrcCellBufferFilterCount++;
                                }
                            }
                        }
                    }

                    if ((string)FMM_Src_Cell_DT_Row["Calc_Src_Type"] == "Dynamic Calc")
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit Dynamic");
                        GBL_BalCalc = "UnbalAlloc";
                    }
                    GBL_SrcCellDict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], (string)FMM_Src_Cell_DT_Row["Calc_Open_Parens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["Calc_Math_Operator"].ToString().Trim() + " " + src_Cell_Drill_Down + "|" + (string)FMM_Src_Cell_DT_Row["Calc_Close_Parens"].ToString().Trim());
                    GBL_SrcCellDrillDownDict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], src_Cell_Drill_Down);
                    GBL_UnbalCalcDict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], (string)FMM_Src_Cell_DT_Row["Calc_Open_Parens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["Calc_Math_Operator"].ToString().Trim() + " -Calculation- " + "|" + (string)FMM_Src_Cell_DT_Row["Calc_Close_Parens"].ToString().Trim());
                    FMM_Src_Cell_DT_Row["Calc_Src_ID_Order"] = src_Cell_Count;
                    FMM_Src_Cell_DT_Row["OS_Dynamic_Calc_Script"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer_Filter"] = UnbalancedSrcCellBufferFilter;
                    BRApi.ErrorLog.LogMessage(si, "dyn calc script: " + src_Cell_Drill_Down);
                }


                //						If Cubedt.Rows.Item(0).Item("ModelType").ToString.XFEqualsIgnoreCase("Table")"												
                //							GBL_BalancedCalc = "DB Model""												
                //						End If"												

                //						GBL_SrcCellDict.Add(SrcDtRow.Item("OSCalcSrcCellID")"	SrcDtRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & SrcDtRow.Item("OSCalcMathOperator").ToString.Trim() & " " & SrcCellDrillDown & "|" & SrcDtRow.Item("OSCalcCloseParens").ToString.Trim())											
                //						If Cubedt.Rows.Item(0).Item("ModelType").ToString.XFEqualsIgnoreCase("Table")"												
                //							If SRCDtRow.Item("OSCalcSrcLocation").ToString.Trim().XFEqualsIgnoreCase("Cube")"												
                //								GBL_UnbalCalcDict.Add(SRCDtRow.Item("OSCalcSrcCellID")"	SRCDtRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & SRCDtRow.Item("OSCalcMathOperator").ToString.Trim() & SRCDtRow.Item("OSCalcSrcLocation").ToString.Trim() &  "-Calculation-|" & SRCDtRow.Item("OSCalcCloseParens").ToString.Trim())											
                //							Else"												
                //								GBL_UnbalCalcDict.Add(SRCDtRow.Item("OSCalcSrcCellID")"	SRCDtRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & SRCDtRow.Item("OSCalcMathOperator").ToString.Trim() & SRCDtRow.Item("OSCalcSrcLocation").ToString.Trim() &  "|" & SRCDtRow.Item("OSCalcCloseParens").ToString.Trim())											
                //							End If"												
                //						Else"												
                //							GBL_UnbalCalcDict.Add(SRCDtRow.Item("OSCalcSrcCellID")"	SRCDtRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & SRCDtRow.Item("OSCalcMathOperator").ToString.Trim() & " -Calculation- " &  "|" & SRCDtRow.Item("OSCalcCloseParens").ToString.Trim())											
                //						End If"																							

                //						'--------------------------------------------------------------------------------	"																							

                //						dml.AppendLine("UPDATE THG_ModelMgr_SrcCell ")"												
                //						dml.AppendLine("SET UnbalancedSrcCellBuffer = '"  & srccelldrilldown & "'"	UnbalancedSrcCellBufferFilter = '" & UnbalancedSrcCellBufferFilter & "' ")											
                //						dml.AppendLine("WHERE OSCalcSrcCellID = " & SrcDtRow.Item("OSCalcSrcCellID") & " ")"												
                //						dml.AppendLine("AND OSCalcID = " & GBL_OSCalcID & ";")"												
                sqlAdapterMcmSrcCell.Update_FMM_Src_Cell(si, FMM_Src_Cell_DT, sqlDataAdapter);
            }
        }

        private void Update_Unbal_Src_Columns(DataRow destDataRow)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            string unbalancedSrcCellBufferFilter = string.Empty;
            int unbalancedSrcCellBufferFilterCnt = 0;
            string overrideDestValue = string.Empty;
            int overrideCnt = 0;
            int countSrcCells = 0;
            BRApi.ErrorLog.LogMessage(si, "hit");

            string countSql = @"
		        SELECT COUNT(*) as Count 
		        FROM FMM_Src_Cell 
		        WHERE Calc_ID = @Calc_ID";

            DataTable srcDt;
            DataTable countDt;

            var sql_paramList = new List<DbParamInfo>
            {
                new DbParamInfo("@Calc_ID", GBL_Calc_ID)
            };

            using (dbConnApp)
            {
                countDt = BRApi.Database.ExecuteSql(dbConnApp, countSql, sql_paramList, false);

                if (countDt.Rows[0]["Count"] != DBNull.Value)
                {
                    countSrcCells = Convert.ToInt32(countDt.Rows[0]["Count"]);
                }

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var sqlAdapterMcmSrcCell = new SQA_FMM_Src_Cell(si, connection);
                    var FMM_Src_Cell_DT = new DataTable();
                    var sqlSrcDataAdapter = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Cell
                    string src_selectQuery = @"
									SELECT * 
									FROM FMM_Src_Cell 
									WHERE Calc_ID = @Calc_ID
									ORDER BY Calc_Src_ID_Order";
                    // Create an array of SqlParameter objects
                    var src_parameters = new SqlParameter[]
                    {
                new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID },
                    };

                    sqlAdapterMcmSrcCell.Fill_FMM_Src_Cell_DT(si, sqlSrcDataAdapter, FMM_Src_Cell_DT, src_selectQuery, src_parameters);

                    FMM_Src_Cell_DT.PrimaryKey = new DataColumn[] { FMM_Src_Cell_DT.Columns["Cell_ID"] };

                    foreach (DataRow srcDtRow in FMM_Src_Cell_DT.Rows)
                    {
                        unbalancedSrcCellBufferFilter = string.Empty;
                        unbalancedSrcCellBufferFilterCnt = 0;
                        overrideDestValue = string.Empty;
                        overrideCnt = 0;

                        // Filter columns come from FMM_Cell (first arg)


                        EvaluateDimension("OS_Acct_Filter", "Acct", "A#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_Origin_Filter", "Origin", "O#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_Flow_Filter", "Flow", "F#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_IC_Filter", "IC", "I#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD1_Filter", "UD1", "U1#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD2_Filter", "UD2", "U2#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD3_Filter", "UD3", "U3#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD4_Filter", "UD4", "U4#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD5_Filter", "UD5", "U5#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD6_Filter", "UD6", "U6#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD7_Filter", "UD7", "U7#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        EvaluateDimension("OS_UD8_Filter", "UD8", "U8#", FMM_Src_Cell_DT, srcDtRow, ref unbalancedSrcCellBufferFilter, ref unbalancedSrcCellBufferFilterCnt, destDataRow, ref overrideDestValue, ref overrideCnt);
                        srcDtRow["Unbal_Src_Cell_Buffer_Filter"] = unbalancedSrcCellBufferFilter;
                        srcDtRow["Override_Value"] = overrideDestValue;
                    }
                    sqlAdapterMcmSrcCell.Update_FMM_Src_Cell(si, FMM_Src_Cell_DT, sqlSrcDataAdapter);
                }
            }
        }

        private void Update_Cell_Columns()
        {
            try
            {
                var dim_token = new Dictionary<string, List<string>>();
                string CurrCubeBufferFilter = String.Empty;
                string SrcBufferFilter = String.Empty;
                int CurrCubeBufferFilterCnt = 0;
                int SrcBufferFilterCnt = 0;

                string[] Core_Dim_Types = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                #region "Core Dim Types"												
                foreach (var Dim_Type in Core_Dim_Types)
                {
                    switch (Dim_Type)
                    {
                        case "Acct":
                            dim_token[Dim_Type] = new List<string> { "A#" };
                            break;
                        case "IC":
                            dim_token[Dim_Type] = new List<string> { "I#", "IC#" };
                            break;
                        case "Origin":
                            dim_token[Dim_Type] = new List<string> { "O#" };
                            break;
                        case "Flow":
                            dim_token[Dim_Type] = new List<string> { "F#" };
                            break;
                        case "UD1":
                            dim_token[Dim_Type] = new List<string> { "UD1#", "U1#" };
                            break;
                        case "UD2":
                            dim_token[Dim_Type] = new List<string> { "UD2#", "U2#" };
                            break;
                        case "UD3":
                            dim_token[Dim_Type] = new List<string> { "UD3#", "U3#" };
                            break;
                        case "UD4":
                            dim_token[Dim_Type] = new List<string> { "UD4#", "U4#" };
                            break;
                        case "UD5":
                            dim_token[Dim_Type] = new List<string> { "UD5#", "U5#" };
                            break;
                        case "UD6":
                            dim_token[Dim_Type] = new List<string> { "UD6#", "U6#" };
                            break;
                        case "UD7":
                            dim_token[Dim_Type] = new List<string> { "UD7#", "U7#" };
                            break;
                        case "UD8":
                            dim_token[Dim_Type] = new List<string> { "UD8#", "U8#" };
                            break;
                    }
                }
                #endregion

                //                string selectQuery = @"
                //									Select Cube.Cube,Cube.Scen_Type,Act.Calc_Type,Model.OS_Model_ID								
                //	                                    From FMM_Models Model											
                //	                                    Join FMM_Calc_Config Calc												
                //	                                    On Model.OS_Model_ID = Calc.OS_Model_ID
                //										JOIN FMM_Cube_Config Cube
                //										ON Cube.Cube_ID = Model.Cube_ID
                //										JOIN FMM_Act_Config Act
                //										ON Act.Act_ID = Model.Act_ID
                //	                                    Where Calc.Calc_ID = @GBL_Calc_ID";												

                if (GBL_BalCalc != "DB Model")
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var SQA_FMM_Cell = new SQA_FMM_Cell(si, connection);
                        var sqlDataAdapter = new SqlDataAdapter();
                        var FMM_Cell_DT = new DataTable();

                        // Fill the DataTable with the current data from FMM_Cell
                        string selectQuery = @"
				            SELECT * 
				            FROM FMM_Cell
				            WHERE Calc_ID = @Calc_ID";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID }
                        };

                        connection.Open();

                        SQA_FMM_Cell.Fill_FMM_Cell_DT(si, sqlDataAdapter, FMM_Cell_DT, selectQuery, parameters);
                        foreach (DataRow FMM_Cell_DT_Row in FMM_Cell_DT.Rows)
                        {
                            foreach (var Dim_Type in Core_Dim_Types)
                            {
                                string targetField = "" + Dim_Type;
                                string filterField = "OS_" + Dim_Type + "_Filter";
                                string balanceKey = Dim_Type;
                                string dim_token_1 = string.Empty;
                                string dim_token_2 = string.Empty;
                                List<string> dim_tokens = dim_token[Dim_Type];
                                if (dim_tokens.Count == 1)
                                {
                                    dim_token_1 = dim_tokens[0];
                                    dim_token_2 = dim_tokens[0];
                                }
                                else if (dim_tokens.Count > 1)
                                {
                                    dim_token_1 = dim_tokens[0];
                                    dim_token_2 = dim_tokens[1];
                                }

                                if (FMM_Cell_DT_Row[targetField] != DBNull.Value)
                                {
                                    string targetValue = FMM_Cell_DT_Row[targetField].ToString();
                                    if (targetValue.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        targetValue.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        CurrCubeBufferFilter += (CurrCubeBufferFilterCnt == 0 ? "[" : ",[") + targetValue + "]";
                                        CurrCubeBufferFilterCnt++;
                                    }
                                }

                                if (FMM_Cell_DT_Row[filterField] != DBNull.Value)
                                {
                                    string filterValue = FMM_Cell_DT_Row[filterField].ToString();
                                    if (filterValue.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        filterValue.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {

                                        // Check if the filterValue contains the pattern |!MemberList_1_Filter.Name!|
                                        string patternStart = "|!MemberList_";
                                        string patternEnd = "_Filter.Name!|";

                                        if (filterValue.Contains(patternStart) && filterValue.Contains(patternEnd))
                                        {
                                            // Find the start and end indexes of the number
                                            int startIndex = filterValue.IndexOf(patternStart) + patternStart.Length;
                                            int endIndex = filterValue.IndexOf(patternEnd, startIndex);

                                            // Extract the number (substring between the pattern start and end)
                                            string number = filterValue.Substring(startIndex, endIndex - startIndex);
                                            // Call the function that selects from FMM_Calc_Config table and returns MemberList_x_Filter
                                            string memberListFilter = GetMemberListFromCalcConfig(number); // Assuming this is your function
                                            CurrCubeBufferFilter += (CurrCubeBufferFilterCnt == 0 ? "[" : ",[") + memberListFilter + "]";
                                            SrcBufferFilter += (SrcBufferFilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            CurrCubeBufferFilterCnt++;
                                            SrcBufferFilterCnt++;
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + CurrCubeBufferFilter);
                                        }
                                        else
                                        {
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + CurrCubeBufferFilter);
                                            CurrCubeBufferFilter += (CurrCubeBufferFilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            SrcBufferFilter += (SrcBufferFilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            CurrCubeBufferFilterCnt++;
                                            SrcBufferFilterCnt++;
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + CurrCubeBufferFilter);
                                        }
                                    }
                                }
                            }
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + CurrCubeBufferFilter);
                            FMM_Cell_DT_Row["OS_Curr_Cube_Buffer_Filter"] = CurrCubeBufferFilter;
                            FMM_Cell_DT_Row["Buffer_Filter"] = SrcBufferFilter;
                        }
                        SQA_FMM_Cell.Update_FMM_Cell(si, FMM_Cell_DT, sqlDataAdapter);

                    }

                }

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }

        private void Evaluate_Calc_Config_Setup(int Curr_Calc_ID)
        {
            try
            {
                var DT = new DataTable();
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {

                    string destSql = @"
										Select *
										FROM FMM_Cell
										WHERE Calc_ID = @Curr_Calc_ID";

                    using (var command = new SqlCommand(destSql, connection))
                    {
                        command.Parameters.AddWithValue("@Curr_Calc_ID", Curr_Calc_ID);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            adapter.Fill(DT);
                        }
                    }
                }

                foreach (DataRow destRow in DT.Rows)
                {
                    GBL_BalCalc = "Balanced";
                    Update_Src_Cell_Columns(destRow);
                    Update_Cell_Columns();
                    Update_Globals();

                    if (GBL_BalCalc == "Unbalanced" || GBL_BalCalc == "UnbalAlloc" || GBL_BalCalc == "Ext_Unbalanced" || GBL_BalCalc == "Ext_UnbalAlloc")
                    {
                        Update_Unbal_Src_Columns(destRow);
                    }
                }

                Update_Calc_Config_Columns();
                GBL_SrcCellDict.Clear();

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void Update_Calc_Config_Columns()
        {
            try
            {
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var SQA_FMM_Calc_Config = new SQA_FMM_Calc_Config(si, connection);
                    var FMM_Calc_Config_DT = new DataTable();
                    var sql_FMM_Calc_Config_DataAdapter = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Cell												
                    string selectQuery = @"
										SELECT * 												
											FROM FMM_Calc_Config 												
											WHERE Calc_ID = @Calc_ID";

                    // Create an array of SqlParameter objects												
                    var FMM_Calc_Config_params = new SqlParameter[]
                    {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID }
                    };

                    SQA_FMM_Calc_Config.Fill_FMM_Calc_Config_DT(si, sql_FMM_Calc_Config_DataAdapter, FMM_Calc_Config_DT, selectQuery, FMM_Calc_Config_params);

                    // Check if the DataTable has rows
                    if (FMM_Calc_Config_DT.Rows.Count > 0)
                    {
                        // Loop through each DataRow in DataTable												
                        foreach (DataRow row in FMM_Calc_Config_DT.Rows)
                        {
                            if (row["MultiDim_Alloc"] != DBNull.Value && Convert.ToBoolean(row["MultiDim_Alloc"]))
                            {
                                GBL_BalCalc = "MultiDim_Alloc";
                                row["Balanced_Buffer"] = GBL_BalCalc;
                                row["bal_buffer_calc"] = GBL_UnbalBufferCalc;
                                row["Unbal_Calc"] = GBL_UnbalCalc;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }
                            else if (row["MemberList_Calc"] != DBNull.Value && Convert.ToBoolean(row["MemberList_Calc"]))
                            {
                                if (GBL_BalCalc == "Unbalanced")
                                {
                                    GBL_BalCalc = "Ext_Unbalanced";
                                    row["Balanced_Buffer"] = GBL_BalCalc;
                                    row["bal_buffer_calc"] = GBL_UnbalBufferCalc;
                                    row["Unbal_Calc"] = GBL_UnbalCalc;
                                    row["Update_Date"] = DateTime.Now;
                                    row["Update_User"] = si.UserName;
                                }
                                else if (GBL_BalCalc == "UnbalAlloc")
                                {
                                    GBL_BalCalc = "Ext_UnbalAlloc";
                                    row["Balanced_Buffer"] = GBL_BalCalc;
                                    row["bal_buffer_calc"] = GBL_UnbalBufferCalc;
                                    row["Unbal_Calc"] = GBL_UnbalCalc;
                                    row["Update_Date"] = DateTime.Now;
                                    row["Update_User"] = si.UserName;
                                }
                            }
                            else if (row["Business_Rule_Calc"] != DBNull.Value && Convert.ToBoolean(row["Business_Rule_Calc"]))
                            {
                                GBL_BalCalc = "Business_Rule_Calc";
                            }
                            else if (GBL_BalCalc == "Balanced")
                            {
                                row["Balanced_Buffer"] = "Balanced";
                                row["bal_buffer_calc"] = GBL_BalBufferCalc;
                                row["Unbal_Calc"] = String.Empty;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;

                            }
                            else if (GBL_BalCalc == "DB Model")
                            {
                                row["Balanced_Buffer"] = GBL_BalCalc;
                                row["Table_Calc_Logic"] = GBL_Table_CalcLogic;
                                row["Table_Src_Cell_Count"] = GBL_Table_SrcCell;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }
                            else
                            {
                                row["Balanced_Buffer"] = GBL_BalCalc;
                                row["bal_buffer_calc"] = GBL_UnbalBufferCalc;
                                row["Unbal_Calc"] = GBL_UnbalCalc;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }

                        }
                        //BRApi.ErrorLog.LogMessage(si,"HIt Herere 3: " +  FMM_Calc_Config_DT.Rows[0]["Update_Date"]);						
                        // Update the FMM_Cell table based on the changes made to the DataTable												
                        SQA_FMM_Calc_Config.Update_FMM_Calc_Config(si, FMM_Calc_Config_DT, sql_FMM_Calc_Config_DataAdapter);
                    }
                    else
                    {
                        // Log if there are no rows to process
                        BRApi.ErrorLog.LogMessage(si, "No rows found in DataTable");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void Update_Globals()
        {
            try
            {
                // Assuming GBL_SrcCellDict is a Dictionary<int	 string>											
                var sortedDict = GBL_SrcCellDict.OrderBy(entry => entry.Key);

                BRApi.ErrorLog.LogMessage(si, "Hit Globals 1" + GBL_BalCalc);

                int calcIterations = 1;
                foreach (KeyValuePair<int, string> calcSegment in sortedDict)
                {
                    if (GBL_BalCalc != "DB Model")
                    {
                        GBL_BalBufferCalc += StringHelper.ReplaceString(calcSegment.Value, "|", String.Empty, true);
                        if (calcIterations == 1 && (GBL_BalCalc == "Unbalanced" || GBL_BalCalc == "UnbalAlloc"))
                        {
                            GBL_UnbalBufferCalc = GBL_SrcCellDrillDownDict[calcSegment.Key];
                            GBL_UnbalCalc += StringHelper.ReplaceString(StringHelper.ReplaceString(GBL_UnbalCalcDict[calcSegment.Key], "-Calculation-", "BalancedBuffer", true), "|", "", true);
                        }
                        else if (GBL_BalCalc == "Unbalanced" || GBL_BalCalc == "UnbalAlloc")
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Globals Unbal");
                            GBL_UnbalCalc += StringHelper.ReplaceString(StringHelper.ReplaceString(GBL_UnbalCalcDict[calcSegment.Key], "-Calculation-", "SrcBufferValue" + calcIterations.ToString(), true), "|", "", true);
                        }
                    }
                    else
                    {
                        GBL_UnbalBufferCalc = GBL_SrcCellDrillDownDict[calcSegment.Key];
                        if (GBL_UnbalBufferCalc.Contains("T#|DBModelYear|", StringComparison.OrdinalIgnoreCase))
                        {
                            GBL_Table_CalcLogic += StringHelper.ReplaceString(StringHelper.ReplaceString(GBL_UnbalCalcDict[calcSegment.Key], "Cube-Calculation-", "Annual_Cube", true), "|", "", true);
                        }
                        else
                        {
                            GBL_Table_CalcLogic += StringHelper.ReplaceString(StringHelper.ReplaceString(GBL_UnbalCalcDict[calcSegment.Key], "Cube-Calculation-", "Monthly_Cube", true), "|", "", true);
                        }
                    }
                    calcIterations++;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void EvaluateDimension(string destFilterColumn, string srcColumn, string filterPrefix, DataTable srcDt, DataRow srcDtRow, ref string bufferFilter, ref int bufferFilterCnt, DataRow destDataRow, ref string overrideDestValue, ref int override_Cnt)
        {
            if (GBL_BalCalc == "Unbalanced" || (GBL_BalCalc == "UnbalAlloc" && Convert.ToInt32(srcDtRow["Calc_Src_ID_Order"]) == 1))
            {
                if (destDataRow[destFilterColumn] != DBNull.Value && XFContainsIgnoreCase(destDataRow[destFilterColumn].ToString(), filterPrefix))
                {
                    // based on discussion w/ Jeff, if the Src_ col is empty then the unbal src buffer filter in src should contain the associated dim filter
                    if (srcDtRow["" + srcColumn] == DBNull.Value && !XFContainsIgnoreCase(srcDtRow["" + srcColumn].ToString(), filterPrefix))
                    {
                        AddToBufferFilter(destDataRow[destFilterColumn].ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
            }
            else
            {
                if (srcDtRow["" + srcColumn] == DBNull.Value || !XFContainsIgnoreCase(srcDtRow["" + srcColumn].ToString(), filterPrefix))
                {
                    // is this supposed to be checking 2 rows before? No Austin
                    // Check the FIlters in teh Dest Cell table and compare check the current cell for each of those dimensions.  If the dimension has explicit member defined for it, drop the Dest Cell Filter for that cell, otherwise keep it.
                    if (srcDt.Rows[Convert.ToInt32(srcDtRow["Calc_Src_ID_Order"]) - 2]["" + srcColumn] == DBNull.Value || !XFContainsIgnoreCase(srcDt.Rows[Convert.ToInt32(srcDtRow["Calc_Src_ID_Order"]) - 2]["" + srcColumn].ToString(), filterPrefix))
                    {
                        //AddToBufferFilter($"{filterPrefix}Replace", ref bufferFilter, ref bufferFilterCnt);
                    }
                    else if (destDataRow[destFilterColumn] != DBNull.Value && XFContainsIgnoreCase(destDataRow[destFilterColumn].ToString(), filterPrefix))
                    {
                        AddToBufferFilter(destDataRow[destFilterColumn].ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
            }
            if (Convert.ToInt32(srcDtRow["Calc_Src_ID_Order"]) != 1)
            {
                if (((!destDataRow["" + srcColumn].ToString().XFContainsIgnoreCase(filterPrefix) && !destDataRow["OS_" + srcColumn + "_Filter"].ToString().XFContainsIgnoreCase(filterPrefix)) &&
                     srcDtRow["OS_Dynamic_Calc_Script"].ToString().XFContainsIgnoreCase(filterPrefix)) ||
                    (srcDt.Rows[Convert.ToInt32(srcDtRow["Calc_Src_ID_Order"]) - 2]["" + srcColumn].ToString().XFContainsIgnoreCase(filterPrefix) &&
                     srcDtRow["" + srcColumn] == DBNull.Value) &&
                    !srcDtRow["Unbal_" + srcColumn + "_Override"].ToString().XFContainsIgnoreCase(filterPrefix))
                {
                    if (override_Cnt > 0)
                    {
                        overrideDestValue += "," + filterPrefix;
                    }
                    else
                    {
                        overrideDestValue = filterPrefix;
                    }
                    override_Cnt++;
                }
            }
        }

        private void AddToBufferFilter(string value, ref string bufferFilter, ref int bufferFilterCnt)
        {
            if (bufferFilterCnt == 0)
            {
                bufferFilter = $"[{value}]";
            }
            else
            {
                bufferFilter += $",[{value}]";
            }
            bufferFilterCnt++;
        }

        private bool XFContainsIgnoreCase(string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public class DimChecker
        {
            // Define the dictionary as a static field, so it's created once and reused
            private static readonly Dictionary<string, string> patterns = new Dictionary<string, string>
            {
                { "A#", "Account" },
                { "F#", "Flow" },
                { "U1#", "UD1" },
                { "U2#", "UD2" },
                { "U3#", "UD3" },
                { "U4#", "UD4" },
                { "U5#", "UD5" },
                { "U6#", "UD6" },
                { "U7#", "UD7" },
                { "U8#", "UD8" },
                { "UD1#", "UD1" },
                { "UD2#", "UD2" },
                { "UD3#", "UD3" },
                { "UD4#", "UD4" },
                { "UD5#", "UD5" },
                { "UD6#", "UD6" },
                { "UD7#", "UD7" },
                { "UD8#", "UD8" }
            };

            public string CheckForDim(DataRow row, string columnName)
            {
                // Check if the column exists and is not null
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    // Convert the column value to string
                    string columnValue = row[columnName].ToString();

                    // Loop through each pattern and check if the columnValue contains the pattern
                    foreach (var pattern in patterns)
                    {
                        if (columnValue.StartsWith(pattern.Key) && columnValue.Length > 1)
                        {
                            return pattern.Value;
                        }
                    }
                }

                // If no match is found, return an empty string or other appropriate value
                return string.Empty;
            }
            public string GetSrcDestFilter(DataRow row, string columnName)
            {
                // Check if the column exists and is not null
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    // Convert the column value to string
                    string columnValue = row[columnName].ToString().Replace(".Children", ".Base");

                    return columnValue;
                }

                // If no match is found, return an empty string or other appropriate value
                return string.Empty;
            }
        }

        private string GetMemberListFromCalcConfig(string number)
        {

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                connection.Open();

                // Create a new DataTable
                var sqa = new SqlDataAdapter();
                var FMM_Calc_Config_DT = new DataTable();
                // Define the select query and parameters
                string sql = @$"
                    SELECT MemberList_{number}_Filter
                    FROM FMM_Calc_Config
                    WHERE Calc_ID = @Calc_ID";
                // Create an array of SqlParameter objects
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = GBL_Calc_ID}
                };

                // Attempt to fill the data table and check for any issues
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);

                foreach (DataRow Calc_Config_Row in FMM_Calc_Config_DT.Rows)
                {
                    return (string)Calc_Config_Row[$"MemberList_{number}_Filter"];
                }
            }


            // Return the result (this could be empty if no match was found)
            return string.Empty;
        }


        #endregion

        #region "Check Duplicates"

        #region "Duplicate Cube Config"
        private void Duplicate_Activity_List(int Cube_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Act_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Act_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Act_Config
		                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Act_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Activity_List_Dict and handle errors
                            foreach (DataRow cube_Activity_Row in FMM_Act_Config_DT.Rows)
                            {
                                string activityKey = Cube_ID + "|" + (int)cube_Activity_Row["Act_ID"];
                                GBL_Activity_List_Dict.Add(activityKey, (string)cube_Activity_Row["Name"] + "|" + (string)cube_Activity_Row["Calc_Type"]);
                            }
                        }
                        break;

                    case "Update Row":

                        string newActivityName = (string)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Name"] + "|" + (string)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Calc_Type"];
                        string ActivityKey = Cube_ID + "|" + (int)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Act_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Activity_List_Dict.Add(ActivityKey, newActivityName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origActivityName = (string)Modified_FMM_Act_Config_DataRow.OriginalDataRow["Name"] + "|" + (string)Modified_FMM_Act_Config_DataRow.OriginalDataRow["Calc_Type"];

                            if (origActivityName != newActivityName)
                            {
                                GBL_Activity_List_Dict.XFSetValue(ActivityKey, newActivityName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Activity_List_Dict.ContainsKey(ActivityKey))
                            {
                                GBL_Activity_List_Dict.Remove(ActivityKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {ActivityKey} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Calc_Unit_Config(int Cube_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Calc_Unit_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Calc_Unit_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Calc_Unit_Config
		                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Unit_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Calc_Unit_Config_Dict and handle errors
                            foreach (DataRow Calc_Unit_Row in FMM_Calc_Unit_Config_DT.Rows)
                            {
                                string configKey = Cube_ID + "|" + (int)Calc_Unit_Row["Calc_Unit_ID"];
                                string configValue = (string)Calc_Unit_Row["Entity_MFB"] + "|" + (string)Calc_Unit_Row["WFChannel"];

                                GBL_Calc_Unit_Config_Dict.Add(configKey, configValue);
                            }
                        }
                        break;

                    case "Update Row":

                        string ConfigKey = Cube_ID + "|" + (int)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Calc_Unit_ID"];
                        string newConfigValue = (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["WFChannel"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Calc_Unit_Config_Dict.Add(ConfigKey, newConfigValue);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origConfigValue = (string)Modified_FMM_Calc_Unit_Config_DataRow.OriginalDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_Calc_Unit_Config_DataRow.OriginalDataRow["WFChannel"];

                            if (origConfigValue != newConfigValue)
                            {
                                GBL_Calc_Unit_Config_Dict.XFSetValue(ConfigKey, newConfigValue);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Calc_Unit_Config_Dict.ContainsKey(ConfigKey))
                            {
                                GBL_Calc_Unit_Config_Dict.Remove(ConfigKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {ConfigKey} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = "An unexpected error occurred.";
            }
        }

        #endregion

        #region "Duplicate Unit/Acct"
        private void Duplicate_Unit_Config(int Cube_ID, int Act_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Unit_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Unit_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Unit_Config
		                        WHERE Cube_ID = @Cube_ID
		                          AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Unit_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID} and Act_ID {Act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Unit_Config_Dict and handle errors
                            foreach (DataRow cube_Unit_Row in FMM_Unit_Config_DT.Rows)
                            {
                                string unitKey = Cube_ID + "|" + Act_ID + "|" + (int)cube_Unit_Row["Unit_ID"];
                                string unitValue = (string)cube_Unit_Row["Name"];

                                GBL_Unit_Config_Dict.Add(unitKey, unitValue);
                            }
                        }
                        break;

                    case "Update Row":

                        string newUnitName = (string)Modified_FMM_Unit_Config_DataRow.ModifiedDataRow["Name"];
                        string UnitKey = Cube_ID + "|" + Act_ID + "|" + (int)Modified_FMM_Unit_Config_DataRow.ModifiedDataRow["Unit_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Unit_Config_Dict.Add(UnitKey, newUnitName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origUnitName = (string)Modified_FMM_Unit_Config_DataRow.OriginalDataRow["Name"];

                            if (origUnitName != newUnitName)
                            {
                                GBL_Unit_Config_Dict.XFSetValue(UnitKey, newUnitName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Unit_Config_Dict.ContainsKey(UnitKey))
                            {
                                GBL_Unit_Config_Dict.Remove(UnitKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {UnitKey} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = "An unexpected error occurred.";
            }
        }

        private void Duplicate_Acct_Config(int Cube_ID, int Act_ID, int Unit_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Acct_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Acct_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Acct_Config
		                        WHERE Cube_ID = @Cube_ID
		                          AND Act_ID = @Act_ID
		                          AND Unit_ID = @Unit_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                                new SqlParameter("@Unit_ID", SqlDbType.Int) { Value = Unit_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Acct_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}, Act_ID {Act_ID}, Unit_ID {Unit_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Acct_Config_Dict and handle errors
                            foreach (DataRow cube_Acct_Row in FMM_Acct_Config_DT.Rows)
                            {
                                string acctKey = Cube_ID + "|" + Act_ID + "|" + Unit_ID + "|" + (int)cube_Acct_Row["Acct_ID"];
                                string acctValue = (string)cube_Acct_Row["Name"];

                                GBL_Acct_Config_Dict.Add(acctKey, acctValue);
                            }
                        }
                        break;

                    case "Update Row":

                        string newAcctName = (string)Modified_FMM_Acct_Config_DataRow.ModifiedDataRow["Name"];
                        string AcctKey = Cube_ID + "|" + Act_ID + "|" + Unit_ID + "|" + (int)Modified_FMM_Acct_Config_DataRow.ModifiedDataRow["Acct_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Acct_Config_Dict.Add(AcctKey, newAcctName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origAcctName = (string)Modified_FMM_Acct_Config_DataRow.OriginalDataRow["Name"];

                            if (origAcctName != newAcctName)
                            {
                                GBL_Acct_Config_Dict.XFSetValue(AcctKey, newAcctName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Acct_Config_Dict.ContainsKey(AcctKey))
                            {
                                GBL_Acct_Config_Dict.Remove(AcctKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {AcctKey} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion

        #region "Duplicate Approvals"
        private void Duplicate_Appr_Config(int Cube_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Appr_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Appr_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Appr_Config
		                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Appr_Dict and handle errors
                            foreach (DataRow Appr_Row in FMM_Appr_Config_DT.Rows)
                            {
                                string approvalKey = Cube_ID + "|" + (int)Appr_Row["Appr_ID"];
                                string approvalValue = (string)Appr_Row["Name"];

                                if (!GBL_Appr_Dict.ContainsKey(approvalKey))
                                {
                                    GBL_Appr_Dict.Add(approvalKey, approvalValue);
                                }
                                else
                                {
                                    save_Task_Result.IsOK = false;
                                    save_Task_Result.ShowMessageBox = true;
                                    save_Task_Result.Message += $"Duplicate found in initial fetch: {approvalKey}";
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        string newApprovalName = (string)Modified_FMM_Appr_Config_DataRow.ModifiedDataRow["Name"];
                        string ApprovalKey = Cube_ID + "|" + (int)Modified_FMM_Appr_Config_DataRow.ModifiedDataRow["Appr_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Appr_Dict.Add(ApprovalKey, newApprovalName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origApprovalName = (string)Modified_FMM_Appr_Config_DataRow.OriginalDataRow["Name"];

                            if (origApprovalName != newApprovalName)
                            {
                                GBL_Appr_Dict.XFSetValue(ApprovalKey, newApprovalName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Appr_Dict.ContainsKey(ApprovalKey))
                            {
                                GBL_Appr_Dict.Remove(ApprovalKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {ApprovalKey} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Appr_Step_Config(int Cube_ID, int Appr_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Appr_Step_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Appr_Step_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Appr_Step_Config
		                        WHERE Cube_ID = @Cube_ID
							      AND Appr_ID = @Appr_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Appr_Step_Dict and handle errors
                            foreach (DataRow Appr_Step_Row in FMM_Appr_Step_Config_DT.Rows)
                            {
                                string approvalStepKey = Cube_ID + "|" + Appr_ID + "|" + (int)Appr_Step_Row["Appr_Step_ID"];
                                string approvalStepValue = (string)Appr_Step_Row["Name"];

                                if (!GBL_Appr_Step_Dict.ContainsKey(approvalStepKey))
                                {
                                    GBL_Appr_Step_Dict.Add(approvalStepKey, approvalStepValue);
                                }
                                else
                                {
                                    save_Task_Result.IsOK = false;
                                    save_Task_Result.ShowMessageBox = true;
                                    save_Task_Result.Message += $"Duplicate found in initial fetch: {approvalStepKey}";
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        string newApprovalStepName = (string)Modified_FMM_Appr_Step_Config_DataRow.ModifiedDataRow["Name"];
                        string ApprovalStepKey = Cube_ID + "|" + Appr_ID + "|" + (int)Modified_FMM_Appr_Step_Config_DataRow.ModifiedDataRow["Appr_Step_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Appr_Step_Dict.Add(ApprovalStepKey, newApprovalStepName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origApprovalStepName = (string)Modified_FMM_Appr_Step_Config_DataRow.OriginalDataRow["Name"];

                            if (origApprovalStepName != newApprovalStepName)
                            {
                                GBL_Appr_Step_Dict.XFSetValue(ApprovalStepKey, newApprovalStepName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Appr_Step_Dict.ContainsKey(ApprovalStepKey))
                            {
                                GBL_Appr_Step_Dict.Remove(ApprovalStepKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {ApprovalStepKey} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }


        #endregion

        #region "Duplicate Register Config"
        private void Duplicate_Reg_Config(int Cube_ID, int Act_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Reg_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Reg_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Reg_Config
		                        WHERE Cube_ID = @Cube_ID
		                          AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Reg_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID} and Act_ID {Act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Register_Dict and handle errors
                            foreach (DataRow Register_Row in FMM_Reg_Config_DT.Rows)
                            {
                                string registerKey = Cube_ID + "|" + Act_ID + "|" + (int)Register_Row["Reg_Config_ID"];
                                string registerValue = (string)Register_Row["Name"];


                                GBL_Register_Dict.Add(registerKey, registerValue);
                            }
                        }
                        break;

                    case "Update Row":

                        string newRegisterName = (string)Modified_FMM_Reg_Config_DataRow.ModifiedDataRow["Name"];
                        string RegisterKey = Cube_ID + "|" + Act_ID + "|" + (int)Modified_FMM_Reg_Config_DataRow.ModifiedDataRow["Reg_Config_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Register_Dict.Add(RegisterKey, newRegisterName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origRegisterName = (string)Modified_FMM_Reg_Config_DataRow.OriginalDataRow["Name"];

                            if (origRegisterName != newRegisterName)
                            {
                                GBL_Register_Dict.XFSetValue(RegisterKey, newRegisterName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Register_Dict.ContainsKey(RegisterKey))
                            {
                                GBL_Register_Dict.Remove(RegisterKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {RegisterKey} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Col_Config(int Cube_ID, int Act_ID, int Register_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Col_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Col_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Col_Config
		                        WHERE Cube_ID = @Cube_ID
		                          AND Act_ID = @Act_ID
								  AND Reg_Config_ID = @Reg_Config_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                                new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = Register_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Col_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID} and Act_ID {Act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Col_Dict and handle errors
                            foreach (DataRow Col_Row in FMM_Col_Config_DT.Rows)
                            {
                                string colKey = Cube_ID + "|" + Act_ID + "|" + Register_ID + "|" + (int)Col_Row["Col_ID"];
                                string colValue = ((int)Col_Row["Order"]).XFToString();

                                if (colValue != "99")
                                {
                                    GBL_Col_Dict.Add(colKey, colValue);
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        string newColName = ((int)Modified_FMM_Col_Config_DataRow.ModifiedDataRow["Order"]).XFToString();
                        string ColKey = Cube_ID + "|" + Act_ID + "|" + Register_ID + "|" + (int)Modified_FMM_Col_Config_DataRow.ModifiedDataRow["Col_ID"];

                        if (DDL_Process == "Update")
                        {
                            string origColName = ((int)Modified_FMM_Col_Config_DataRow.OriginalDataRow["Order"]).XFToString();

                            if (origColName != newColName)
                            {
                                GBL_Col_Dict.XFSetValue(ColKey, newColName);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion

        #region "Duplicate Calcs"
        private void Duplicate_Calc_Config(int Cube_ID, int Act_ID, int Model_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result, [Optional] String DDL_Process, [Optional] XFEditedDataRow Modified_FMM_Calc_Config_DataRow)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Calc_Config_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Calc_Config
		                        WHERE Cube_ID = @Cube_ID
		                          AND Act_ID = @Act_ID
		                          AND Model_ID = @Model_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                                new SqlParameter("@Model_ID", SqlDbType.Int) { Value = Model_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID} and Act_ID {Act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Calc_Dict and handle errors
                            foreach (DataRow Calc_Row in FMM_Calc_Config_DT.Rows)
                            {
                                string calcKey = Cube_ID + "|" + Act_ID + "|" + Model_ID + "|" + (int)Calc_Row["Calc_ID"];
                                string CalcValue = (string)Calc_Row["Name"];


                                GBL_Calc_Dict.Add(calcKey, CalcValue);
                            }
                        }
                        break;

                    case "Update Row":

                        string newCalcName = (string)Modified_FMM_Calc_Config_DataRow.ModifiedDataRow["Name"];
                        string CalcKey = Cube_ID + "|" + Act_ID + "|" + Model_ID + "|" + (int)Modified_FMM_Calc_Config_DataRow.ModifiedDataRow["Calc_ID"];

                        if (DDL_Process == "Insert")
                        {
                            GBL_Calc_Dict.Add(CalcKey, newCalcName);
                        }
                        else if (DDL_Process == "Update")
                        {
                            string origCalcName = (string)Modified_FMM_Calc_Config_DataRow.OriginalDataRow["Name"];

                            if (origCalcName != newCalcName)
                            {
                                GBL_Calc_Dict.XFSetValue(CalcKey, newCalcName);
                            }
                        }
                        else if (DDL_Process == "Delete")
                        {
                            if (GBL_Calc_Dict.ContainsKey(CalcKey))
                            {
                                GBL_Calc_Dict.Remove(CalcKey);
                            }
                            else
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message += $"Delete operation failed: entry {CalcKey} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion
        #endregion

        #region "Col Helpers"

        public void Insert_Col_Default_Rows(int Cube_ID, int Act_ID, int Reg_Config_ID)
        {
            BRApi.ErrorLog.LogMessage(si, "Hit Defaults: " + "|" + si.AuthToken.AuthSessionID.ToString());
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                var sqlAdapterMcmCubeActivityColConfig = new SQA_FMM_Col_Config(si, connection);
                var sqlDataAdapter_colSetup = new SqlDataAdapter();
                var FMM_Col_Config_DT = new DataTable();
                connection.Open();
                // Get the max ID for the "FMM_Col_Config" table
                int os_Col_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                // Fill the DataTable with the current data from FMM_Col_Config
                string selectQuery_colSetup = @"
				    SELECT * 
				    FROM FMM_Col_Config 
				    WHERE Cube_ID = @Cube_ID
					AND Act_ID = @Act_ID
					AND Reg_Config_ID = @Reg_Config_ID";

                // Create an array of SqlParameter objects
                var parameters_colSetup = new SqlParameter[]
                {
                    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                    new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                    new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = Reg_Config_ID }
                };
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                sqlAdapterMcmCubeActivityColConfig.Fill_FMM_Col_Config_DT(si, sqlDataAdapter_colSetup, FMM_Col_Config_DT, selectQuery_colSetup, parameters_colSetup);
                //os_Col_ID -= os_Col_ID;
                // Insert new rows into the DataTable using default values
                var columnConfigDefaults = new ColumnConfigDefaults();
                foreach (var columnConfig in columnConfigDefaults.DefaultColumns)
                {
                    var newRow = FMM_Col_Config_DT.NewRow();
                    newRow["Cube_ID"] = Cube_ID;
                    newRow["Act_ID"] = Act_ID;
                    newRow["Reg_Config_ID"] = Reg_Config_ID;
                    newRow["Col_ID"] = ++os_Col_ID; // Ensure unique IDs
                    newRow["Name"] = columnConfig.Col_Name;
                    newRow["InUse"] = true; // Set default value for Col_InUse if needed
                    newRow["Required"] = true;
                    newRow["Updates"] = true;
                    newRow["Alias"] = columnConfig.Col_Alias;
                    newRow["Order"] = columnConfig.Col_Order;
                    newRow["Default"] = DBNull.Value; // Set default value if needed
                    newRow["Param"] = DBNull.Value; // Set default value if needed
                    newRow["Format"] = string.Empty; // Set default value if needed
                    newRow["Filter_Param"] = DBNull.Value; // Set default value if needed
                    newRow["Create_Date"] = DateTime.Now;
                    newRow["Create_User"] = si.UserName;
                    newRow["Update_Date"] = DateTime.Now;
                    newRow["Update_User"] = si.UserName;

                    FMM_Col_Config_DT.Rows.Add(newRow);
                }
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                // Update the FMM_Col_Config table based on the changes made to the DataTable
                sqlAdapterMcmCubeActivityColConfig.Update_FMM_Col_Config(si, FMM_Col_Config_DT, sqlDataAdapter_colSetup);
            }
        }

        #endregion

        #region "Col Data Storage"
        public class ColumnConfigDefaults
        {
            public List<ColumnConfig> DefaultColumns { get; set; }

            public ColumnConfigDefaults()
            {
                DefaultColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { Col_Name = "Entity", Col_Alias = "Entity", Col_Order = 1 },
                    new ColumnConfig { Col_Name = "Appr_Level_ID", Col_Alias = "Approval Level ID", Col_Order = 2 },
                    new ColumnConfig { Col_Name = "Register_ID", Col_Alias = "Register ID", Col_Order = 3 },
                    new ColumnConfig { Col_Name = "Register_ID_1", Col_Alias = "Register ID 1", Col_Order = 4 },
                    new ColumnConfig { Col_Name = "Register_ID_2", Col_Alias = "Register ID 2", Col_Order = 5 },
                    new ColumnConfig { Col_Name = "Spread_Amount", Col_Alias = "Spread Amount", Col_Order = 43 },
                    new ColumnConfig { Col_Name = "Spread_Curve", Col_Alias = "Spread Curve", Col_Order = 44 }
                };

                for (int i = 1; i <= 20; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        Col_Name = $"Attribute_{i}",
                        Col_Alias = $"Attribute {i}",
                        Col_Order = i + 5 // Starting from 6
                    });
                }
                for (int i = 1; i <= 12; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        Col_Name = $"Attribute_Value_{i}",
                        Col_Alias = $"Attribute Value {i}",
                        Col_Order = i + 25 // Starting from 26
                    });
                }
                for (int i = 1; i <= 5; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        Col_Name = $"Date_Value_{i}",
                        Col_Alias = $"Date Value {i}",
                        Col_Order = i + 37 // Starting from 38
                    });
                }

            }

            public class ColumnConfig
            {
                public string Col_Name { get; set; }
                public string Col_Alias { get; set; }
                public int Col_Order { get; set; }
            }
        }
        #endregion


        // possible future helper to sanitize data rows vs doing them all inline
        // TODO: Figure out how to account for all types of data row
        private XFDataRow SanitizeRow(XFDataRow dataRow)
        {
            foreach (string key in dataRow.Items.Keys)
            {
                if (dataRow.Items[key] == typeof(string))
                {
                    if (dataRow.Items[key] == null || String.Empty.Equals(dataRow.Items[key]))
                    {
                        dataRow.Items[key] = DBNull.Value;
                    }
                }
            }

            return dataRow;
        }
		
        #region "Class Helper Functions"	
        #region "Solution Setup"
        private XFSelectionChangedTaskResult Create_Schema_New_Install(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
            return selectionChangedTaskResult;

        }

        private XFSelectionChangedTaskResult Load_App_Settings(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
            return selectionChangedTaskResult;
        }

        private XFSelectionChangedTaskResult Save_App_Settings(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
            return selectionChangedTaskResult;
        }
        #endregion
        #region "Migrate Model"

        #endregion
        #region "Model Maintenance"

        private XFSelectionChangedTaskResult Save_Cube_Config(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var Cube = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_All_Cubes", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Cube", String.Empty));
                var Scen_Type = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_Cube_Config_Scen_Types", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Scen_Type", String.Empty));
                var agg_Consol = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Cube_Config_Agg_Consol", String.Empty);
                var entity_Dim = String.Empty;
                var entity_MFB = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Entity_MFB", String.Empty);
                var cube_Description = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Description", String.Empty);
                int new_Cube_ID = 0;

                BRApi.ErrorLog.LogMessage(si, "hit: " + Cube);
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    // Create a new DataTable to check for existing records
                    var sqa = new SqlDataAdapter();
                    var FMM_Cube_Config_Count_DT = new DataTable();

                    // Query to check if the cube config already exists
                    var sql = @"
		                SELECT Count(*) as Count
		                FROM FMM_Cube_Config
		                WHERE Cube = @Cube
		                AND Scen_Type = @Scen_Type";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.NVarChar, 50) { Value = Cube },
                        new SqlParameter("@Scen_Type", SqlDbType.NVarChar, 20) { Value = Scen_Type }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Cube_Config_Count_DT, sql, sqlparams);
					
					var sqa_fmm_cube_config = new SQA_FMM_Cube_Config(si, connection);
                    var FMM_Cube_Config_DT = new DataTable();

                    // Insert new cube config record
                    sql = "SELECT * FROM FMM_Cube_Config WHERE Cube_ID = @Cube_ID";
					// Define the variable to hold the parameters
				    sqlparams = new SqlParameter[]
				    {
				    };

                    // If no existing record and RunType is "New", insert new record
                    if (RunType == "New")
                    {
                        // Get the next Cube_ID for the new record
                        new_Cube_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Cube_Config", "Cube_ID");

					    // Assign a new array with the parameter to the variable
					    sqlparams = new SqlParameter[]
					    {
					        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = new_Cube_ID }
					    };

                        sqa_fmm_cube_config.Fill_FMM_Cube_Config_DT(si, sqa,FMM_Cube_Config_DT, sql, sqlparams);

                        var newRow = FMM_Cube_Config_DT.NewRow();
                        newRow["Cube_ID"] = new_Cube_ID;
                        newRow["Cube"] = Cube;
                        newRow["Scen_Type"] = Scen_Type;
                        newRow["Desc"] = cube_Description;
                        newRow["Agg_Consol"] = agg_Consol;
                        newRow["Entity_Dim"] = entity_Dim;
                        newRow["entity_MFB"] = entity_MFB;
                        newRow["Status"] = "Build";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;

                        FMM_Cube_Config_DT.Rows.Add(newRow);

                        // Save the changes to the database
                        sqa_fmm_cube_config.Update_FMM_Cube_Config(si, FMM_Cube_Config_DT, sqa);

                        save_Task_Result.IsOK = true;
                        save_Task_Result.Message = "New Cube Config Saved.";
                        save_Task_Result.ShowMessageBox = true;
                    }
                    // If record exists and RunType is "Update", update the record
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && RunType == "Update")
                    {
                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", String.Empty);
                        new_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0"));
                        sqlparams = new SqlParameter[]
						{
						    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = new_Cube_ID }
						};

                        sqa_fmm_cube_config.Fill_FMM_Cube_Config_DT(si, sqa,FMM_Cube_Config_DT, sql, sqlparams);

                        // Update the existing row
                        if (FMM_Cube_Config_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_Cube_Config_DT.Rows[0];
                            rowToUpdate["Desc"] = cube_Description;
                            rowToUpdate["Entity_MFB"] = entity_MFB;
                            rowToUpdate["Agg_Consol"] = agg_Consol;
                            rowToUpdate["Status"] = cubeStatus;
                            rowToUpdate["Update_Date"] = DateTime.Now;
                            rowToUpdate["Update_User"] = si.UserName;

                            // Update the database with the changes
                            sqa_fmm_cube_config.Update_FMM_Cube_Config(si, FMM_Cube_Config_DT, sqa);

                            save_Task_Result.IsOK = true;
                            save_Task_Result.Message = "Cube Config Updates Saved.";
                            save_Task_Result.ShowMessageBox = true;
                        }
                    }
                    // If a duplicate exists and RunType is "New", show an error
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && RunType == "New")
                    {
                        save_Task_Result.IsOK = false;
                        save_Task_Result.Message = "Duplicated Cube and Scenario Type, Cube Config not saved.";
                        save_Task_Result.ShowMessageBox = true;
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                var errorResult = new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
                return errorResult;
            }
        }

        //		Select Dim.Name
        //FROM Cube
        //JOIN CubeDim
        //ON Cube.CubeID = CubeDim.CubeID
        //JOIN Dim
        //ON CubeDim.DimID = Dim.DimID
        //AND CubeDim.DimTypeID = Dim.DimTypeID
        //Where Cube.Name = 'DHA_Consol'
        //And CubeDim.DimTypeId = 0


        private XFSelectionChangedTaskResult Save_Model(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var Act_List_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0"));
                var Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_OS_Model_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Name", String.Empty);
                int new_Model_ID = 0;
                if (new_Model_ID == 0)
                {
                    DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        if (RunType == "New")
                        {
                            new_Model_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "Model_ID");
                        }
                        else
                        {
                            new_Model_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_ID", "0"));
                        }

                        var sqa_fmm_models = new SQA_FMM_Models(si, connection);
                        var FMM_Models_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Cell
                        string sql = @"
										SELECT * 
										FROM FMM_Models 
										WHERE Model_ID = @Model_ID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + sql + "-" + new_Model_ID);

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        	new SqlParameter("@Model_ID", SqlDbType.Int) { Value = new_Model_ID }
                        };

                        sqa_fmm_models.Fill_FMM_Models_DT(si, sqa,FMM_Models_DT, sql, sqlparams);
                        if (RunType == "New")
                        {
                            var newRow = FMM_Models_DT.NewRow();
                            newRow["Cube_ID"] = Cube_ID;
                            newRow["Act_ID"] = Act_List_ID;
                            newRow["Model_ID"] = new_Model_ID;
                            newRow["Name"] = new_OS_Model_Name;
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;
                            // Set other column values for the new row as needed
                            FMM_Models_DT.Rows.Add(newRow);
                        }
                        else if (RunType == "Update")
                        {
                            if (FMM_Models_DT.Rows.Count > 0)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit this");
                                var rowToUpdate = FMM_Models_DT.Rows[0];
                                rowToUpdate["Name"] = new_OS_Model_Name;
                                rowToUpdate["Status"] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                            }

                        }
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Cell table based on the changes made to the DataTable
                        sqa_fmm_models.Update_FMM_Models(si, FMM_Models_DT, sqa);
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var new_OS_Model_Grp_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Name", "0");
                int new_Model_Grp_ID = 0;
                if (new_Model_Grp_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Model_Grp_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grps", "Model_Grp_ID");

                        var sqa_fmm_model_grps = new SQA_FMM_Model_Grps(si, connection);
                        var FMM_Model_Grps_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Cell
                        string sql = @"
										SELECT * 
										FROM FMM_Model_Grps 
										WHERE Model_Grp_ID = @Model_Grp_ID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + sql);

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = new_Model_Grp_ID }
                        };

                        sqa_fmm_model_grps.Fill_FMM_Model_Grps_DT(si, sqa,FMM_Model_Grps_DT, sql, sqlparams);

                        var newRow = FMM_Model_Grps_DT.NewRow();
                        newRow["Cube_ID"] = new_OS_Model_Grp_Cube_ID;
                        newRow["Model_Grp_ID"] = new_Model_Grp_ID;
                        newRow["Name"] = new_Name;
                        newRow["Status"] = "Build";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grps_DT.Rows.Add(newRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Cell table based on the changes made to the DataTable
                        sqa_fmm_model_grps.Update_FMM_Model_Grps(si, FMM_Model_Grps_DT, sqa);
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp_Seq(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var new_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_Name", "0");
                int new_Model_Grp_Seq_ID = 0;
                if (new_Model_Grp_Seq_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Model_Grp_Seq_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grp_Seqs", "Model_Grp_Seq_ID");

                        var sqa_fmm_model_grp_seqs = new SQA_FMM_Model_Grp_Seqs(si, connection);
                        var FMM_Model_Grp_Seqs_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Cell
                        string sql = @"
										SELECT * 
										FROM FMM_Model_Grp_Seqs 
										WHERE Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + sql);

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = new_Model_Grp_Seq_ID }
                        };

                        sqa_fmm_model_grp_seqs.Fill_FMM_Model_Grp_Seqs_DT(si, sqa,FMM_Model_Grp_Seqs_DT, sql, sqlparams);

                        var newRow = FMM_Model_Grp_Seqs_DT.NewRow();
                        newRow["Cube_ID"] = new_Cube_ID;
                        newRow["Model_Grp_Seq_ID"] = new_Model_Grp_Seq_ID;
                        newRow["Name"] = new_Name;
                        newRow["Status"] = "Build";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grp_Seqs_DT.Rows.Add(newRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Cell table based on the changes made to the DataTable
                        sqa_fmm_model_grp_seqs.Update_FMM_Model_Grp_Seqs(si, FMM_Model_Grp_Seqs_DT, sqa);
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp_Assign(string RunType)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var custom_Subst_Vars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var Cube_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Cube_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_Grp_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Model_Grp_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_ID_List = custom_Subst_Vars.XFGetValue("IV_FMM_Model_ID_Selection", "0");
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: " + custom_Subst_Vars.XFGetValue("IV_FMM_Model_ID_Selection", "0") + " - " + Cube_ID);
                int new_OS_Model_Grp_Assign_ID = 0;
                if (Model_ID_List.Length > 0 && new_OS_Model_Grp_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_OS_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grp_Assign", "Model_Grp_Assign_ID");

                        var sqa_fmm_model_grp_assign = new SQA_FMM_Model_Grp_Assign(si, connection);
                        var FMM_Model_Grp_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Cell
                        string sql = @"
										SELECT * 
										FROM FMM_Model_Grp_Assign
										WHERE Cube_ID = @Cube_ID
								        AND Model_Grp_ID = @Model_Grp_ID";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = Model_Grp_ID }
                        };

                        sqa_fmm_model_grp_assign.Fill_FMM_Model_Grp_Assign_DT(si, sqa,FMM_Model_Grp_Assign_DT, sql, sqlparams);

                        // Split the Model_ID_List by comma and loop through each ID
                        var modelIds = Model_ID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var modelId in modelIds)
                        {
                            if (!isFirstIteration)
                            {
                                new_OS_Model_Grp_Assign_ID += 1;
                            }
                            isFirstIteration = false;
                            var newRow = FMM_Model_Grp_Assign_DT.NewRow();
                            newRow["Cube_ID"] = Cube_ID;
                            newRow["Model_Grp_ID"] = Model_Grp_ID;
                            newRow["Model_ID"] = Convert.ToInt32(modelId.Trim());
                            newRow["Model_Grp_Assign_ID"] = new_OS_Model_Grp_Assign_ID;
                            newRow["Sequence"] = 0;
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;

                            FMM_Model_Grp_Assign_DT.Rows.Add(newRow);
                        }

                        // Update the database with the new rows
                        sqa_fmm_model_grp_assign.Update_FMM_Model_Grp_Assign(si, FMM_Model_Grp_Assign_DT, sqa);
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Calc_Unit_Assign(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var custom_Subst_Vars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var Cube_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_ID_List = custom_Subst_Vars.XFGetValue("IV_FMM_Calc_Unit_Selection", "0");
                var Model_Grp_ID_List = custom_Subst_Vars.XFGetValue("IV_FMM_Model_Grp_ID_Selection");
                var Model_Grp_Seq_ID = custom_Subst_Vars.XFGetValue("IV_FMM_Model_Grp_Seq_ID");
                BRApi.ErrorLog.LogMessage(si, "Hit Grup Model Assignment: " + custom_Subst_Vars.XFGetValue("IV_FMM_Model_Grp_ID_Selection") + " - " + Cube_ID);
                int new_Calc_Unit_Assign_ID = 0;
                if (Model_Grp_ID_List.Length > 0 && new_Calc_Unit_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Assign", "Calc_Unit_Assign_ID");

                        var sqa_fmm_calc_unit_assign = new SQA_FMM_Calc_Unit_Assign(si, connection);
                        var FMM_Calc_Unit_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Cell
                        string sql = @"
										SELECT * 
										FROM FMM_Calc_Unit_Assign
										WHERE Cube_ID = @Cube_ID
								        AND Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                        new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = Model_Grp_Seq_ID }
                        };

                        sqa_fmm_calc_unit_assign.Fill_FMM_Calc_Unit_Assign_DT(si, sqa,FMM_Calc_Unit_Assign_DT, sql, sqlparams);

                        // Split the Model_ID_List by comma and loop through each ID
                        var modelGroupIds = Model_Grp_ID_List.Split(',');
                        var WfDuIds = Calc_Unit_ID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var modelGroupId in modelGroupIds)
                        {
                            foreach (var WfDuId in WfDuIds)
                            {
                                if (!isFirstIteration)
                                {
                                    new_Calc_Unit_Assign_ID += 1;
                                }
                                isFirstIteration = false;
                                var newRow = FMM_Calc_Unit_Assign_DT.NewRow();
                                newRow["Cube_ID"] = Cube_ID;
                                newRow["Model_Grp_ID"] = Convert.ToInt32(modelGroupId.Trim());
                                newRow["Calc_Unit_ID"] = Convert.ToInt32(WfDuId.Trim());
                                newRow["Model_Grp_Seq_ID"] = Model_Grp_Seq_ID;
                                newRow["Calc_Unit_Assign_ID"] = new_Calc_Unit_Assign_ID;
                                newRow["Sequence"] = 0;
                                newRow["Status"] = "Build";
                                newRow["Create_Date"] = DateTime.Now;
                                newRow["Create_User"] = si.UserName;
                                newRow["Update_Date"] = DateTime.Now;
                                newRow["Update_User"] = si.UserName;

                                FMM_Calc_Unit_Assign_DT.Rows.Add(newRow);
                            }
                        }

                        // Update the database with the new rows
                        sqa_fmm_calc_unit_assign.Update_FMM_Calc_Unit_Assign(si, FMM_Calc_Unit_Assign_DT, sqa);
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Appr_Step(string RunType)
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var custom_Subst_Vars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var Cube_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Act_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Act_ID", "0"));
                var Appr_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var WFProfile_Step = custom_Subst_Vars.XFGetValue("IV_FMM_trv_Appr_Step_WFProfile", string.Empty);
                var Reg_Config_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("BL_FMM_Register_Profiles", "0"));

                BRApi.ErrorLog.LogMessage(si, "Hit Approval Step: " + Cube_ID + "|" + Act_ID + "|" + Appr_ID + "|" + WFProfile_Step);
                int new_Appr_Step_ID = 0;

                if (Cube_ID > 0 && Act_ID > 0 && Appr_ID > 0 && Reg_Config_ID > 0 && WFProfile_Step != string.Empty && new_Appr_Step_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        // Get the max ID for the Appr_Step_Config table
                        new_Appr_Step_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "Appr_Step_ID");

                        var sqa_fmm_appr_step_config = new SQA_FMM_Appr_Step_Config(si, connection);
                        var FMM_Appr_Step_Config_DT = new DataTable();
                        var FMM_Appr_Step_WFProfile_DT = new DataTable();
                        var FMM_Cube_Config_DT = new DataTable();
                        var sqa_fmm_act_appr_step_config = new SQA_FMM_Act_Appr_Step_Config(si, connection);
                        var FMM_Act_Appr_Step_Config_DT = new DataTable(); // DataTable for FMM_Reg_Dtl_Cube_Map
                        var sqa = new SqlDataAdapter();
						var sqa_FMM_Act_Appr_Step_Config = new SqlDataAdapter();

                        // Fill DataTable for Appr_Step_Config
                        string sql = @"
		                    SELECT * 
		                    FROM FMM_Appr_Step_Config
		                    WHERE Cube_ID = @Cube_ID
		                    AND Act_ID = @Act_ID
		                    AND Appr_ID = @Appr_ID
		                    AND Register_Profile = @Register_Profile";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                            new SqlParameter("@Register_Profile", SqlDbType.Int) { Value = Reg_Config_ID }
                        };

                        sqa_fmm_appr_step_config.Fill_FMM_Appr_Step_Config_DT(si, sqa,FMM_Appr_Step_Config_DT, sql, sqlparams);

                        // Fill DataTable for Appr_Step_Config
                        sql = @"
		                    SELECT * 
		                    FROM FMM_Reg_Dtl_Cube_Map
		                    WHERE Cube_ID = @Cube_ID
		                    AND Act_ID = @Act_ID
		                    AND Appr_ID = @Appr_ID";

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID },
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                        };

                        sqa_fmm_act_appr_step_config.Fill_FMM_Act_Appr_Step_Config_DT(si, sqa_FMM_Act_Appr_Step_Config, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        // Load Cube_Config data
                        string selectCubeConfigQuery = @"
		                    SELECT * 
		                    FROM FMM_Cube_Config
		                    WHERE Cube_ID = @Cube_ID";

                        var cubeConfigParams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,FMM_Cube_Config_DT, selectCubeConfigQuery, cubeConfigParams);

                        var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, FMM_Cube_Config_DT.Rows[0].Field<string>("Cube"));
                        var cubeScenarioType = ScenarioType.GetItem(FMM_Cube_Config_DT.Rows[0].Field<string>("Scen_Type"));
                        var rootProfileName = topCubeInfo.GetTopLevelCubeWFPName(si, cubeScenarioType.Id);

                        // Retrieve WFProfile information
                        string selectWFProfileQuery = @"
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
		                        rcte.HierarchyIndex
		                    FROM 
		                        RecursiveCTE rcte
		                    LEFT JOIN 
		                        WorkflowProfileHierarchy parentProf ON rcte.ParentProfileKey = parentProf.ProfileKey
		                    ORDER BY 
		                        rcte.HierarchyLevel DESC, 
		                        rcte.HierarchyIndex";

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,FMM_Appr_Step_WFProfile_DT, selectWFProfileQuery, new SqlParameter[]
                        {
                            new SqlParameter("@rootprofilename", SqlDbType.NVarChar) { Value = rootProfileName }
                        });

                        // LINQ query to filter WFProfiles based on WFProfile_Step
                        var filteredWFProfiles = from profile in FMM_Appr_Step_WFProfile_DT.AsEnumerable()
                                                 where profile["ProfileName"].ToString().Contains(WFProfile_Step)
                                                 select profile;

                        // Loop through filtered WFProfiles and insert new Approval Steps and Cube Map
                        foreach (var profile in filteredWFProfiles)
                        {
                            var newRow = FMM_Appr_Step_Config_DT.NewRow();
                            newRow["Cube_ID"] = Cube_ID;
                            newRow["Act_ID"] = Act_ID;
                            newRow["Appr_ID"] = Appr_ID;
                            newRow["WFProfile_Step"] = profile["ProfileKey"];
                            newRow["Appr_Step_ID"] = new_Appr_Step_ID++;
                            newRow["Step_Num"] = 0;
                            newRow["Register_Profile"] = Reg_Config_ID;
                            newRow["Appr_User_Group"] = "Test";
                            newRow["Appr_Logic"] = "Test";
                            newRow["Appr_Item"] = "Test";
                            newRow["Appr_Level"] = 0;
                            newRow["Appr_Config"] = 0;
                            newRow["Init_Status"] = "Testy";
                            newRow["Appr_Status"] = "Testy";
                            newRow["Rej_Status"] = "Testy";
                            newRow["Status"] = "Build";
                            newRow["Create_Date"] = DateTime.Now;
                            newRow["Create_User"] = si.UserName;
                            newRow["Update_Date"] = DateTime.Now;
                            newRow["Update_User"] = si.UserName;

                            FMM_Appr_Step_Config_DT.Rows.Add(newRow);

                            // Insert into FMM_Reg_Dtl_Cube_Map
                            var mapRow = FMM_Act_Appr_Step_Config_DT.NewRow();
                            mapRow["Cube_ID"] = Cube_ID;
                            mapRow["Act_ID"] = Act_ID;
                            mapRow["Appr_ID"] = Appr_ID;
                            mapRow["Appr_Step_ID"] = newRow["Appr_Step_ID"];
                            mapRow["Reg_Dtl_Cube_Map_ID"] = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Dtl_Cube_Map", "Reg_Dtl_Cube_Map_ID");
                            mapRow["Acct"] = "AcctValue"; // replace with actual values
                            mapRow["Flow"] = "FlowValue";
                            mapRow["UD1"] = "UD1Value";
                            mapRow["UD2"] = "UD2Value";
                            mapRow["UD3"] = "UD3Value";
                            mapRow["UD4"] = "UD4Value";
                            mapRow["UD5"] = "UD5Value";
                            mapRow["UD6"] = "UD6Value";
                            mapRow["UD7"] = "UD7Value";
                            mapRow["UD8"] = "UD8Value";
                            mapRow["Create_Date"] = DateTime.Now;
                            mapRow["Create_User"] = si.UserName;
                            mapRow["Update_Date"] = DateTime.Now;
                            mapRow["Update_User"] = si.UserName;

                            FMM_Act_Appr_Step_Config_DT.Rows.Add(mapRow);
                        }

                        // Save Approval Steps
                        sqa_fmm_appr_step_config.Update_FMM_Appr_Step_Config(si, FMM_Appr_Step_Config_DT, sqa);

                        // Save Cube Map entries
                        //SQA_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                        BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                    }
                }
                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Act_Appr_Step_Config(string RunType)
        {
            try
            {

                BRApi.ErrorLog.LogMessage(si, "Save approval step activity runtype: " + RunType);
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var custom_Subst_Vars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var Act_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("BL_FMM_Appr_Act_ID", "0"));
                var Appr_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var Appr_Step_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Appr_Step_ID", "-1")); // this can actually be 0
                var Reg_Config_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("BL_FMM_Register_Profiles", "0"));
				
				var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
	                var sqa_fmm_act_appr_step_config = new SQA_FMM_Act_Appr_Step_Config(si, connection);
	                var FMM_Act_Appr_Step_Config_DT = new DataTable();
	                var sqa = new SqlDataAdapter();
	
	                // Fill DataTable for Appr_Step_Activity_Config
	                string sql = @"
	                    SELECT * 
	                    FROM FMM_Act_Appr_Step_Config
	                    WHERE Act_ID = @Act_ID
	                    AND Appr_ID = @Appr_ID
						AND Appr_Step_ID = @Appr_Step_ID";
					
					var sqlparams = new SqlParameter[]{};


                BRApi.ErrorLog.LogMessage(si, "save data: " + Act_ID + " " + Appr_ID + " " + Appr_Step_ID + " " + Reg_Config_ID);

                if (RunType == "New" && Act_ID > 0 && Appr_ID > 0 && Appr_Step_ID >= 0 && Reg_Config_ID > 0) // new row
                {
                    BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                    var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    // Get the max ID for the Appr_Step_Config table
                    int new_Appr_Step_Act_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Appr_Step_Config", "Appr_Step_Act_ID");


                    sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                        new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                        new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = Appr_Step_ID },

                    };

                    sqa_fmm_act_appr_step_config.Fill_FMM_Act_Appr_Step_Config_DT(si, sqa,FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                    string noneVal = "None";
                    FMM_Act_Appr_Step_Config_DT.Clear(); // clear data, because we only want the column information to create a new row.
                                                                  // Insert into FMM_Appr_Step_Activity_Config_DT
                    var newRow = FMM_Act_Appr_Step_Config_DT.NewRow();
                    newRow["Act_ID"] = Act_ID;
                    newRow["Appr_ID"] = Appr_ID;
                    newRow["Appr_Step_ID"] = Appr_Step_ID;
                    newRow["Appr_Step_Act_ID"] = new_Appr_Step_Act_ID;
                    newRow["Reg_Config_ID"] = Reg_Config_ID;
                    newRow["Description"] = "Desc";
                    newRow["Acct"] = noneVal;
                    newRow["Flow"] = noneVal;
                    newRow["UD1"] = noneVal;
                    newRow["UD2"] = noneVal;
                    newRow["UD3"] = noneVal;
                    newRow["UD4"] = noneVal;
                    newRow["UD5"] = noneVal;
                    newRow["UD6"] = noneVal;
                    newRow["UD7"] = noneVal;
                    newRow["UD8"] = noneVal;
                    newRow["Create_Date"] = DateTime.Now;
                    newRow["Create_User"] = si.UserName;
                    newRow["Update_Date"] = DateTime.Now;
                    newRow["Update_User"] = si.UserName;

                    FMM_Act_Appr_Step_Config_DT.Rows.Add(newRow);

                    // Save Approval Step Activity Config
                    sqa_fmm_act_appr_step_config.Update_FMM_Act_Appr_Step_Config(si, FMM_Act_Appr_Step_Config_DT, sqa);

                    // Save Cube Map entries
                    //SQA_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                    BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                }
                else if (RunType == "Update" && Act_ID > 0)
                { // existing row

                    connection.Open();

                    // Fill DataTable for Appr_Step_Activity_Config
                    sql = @"
	                    SELECT * 
	                    FROM FMM_Act_Appr_Step_Config
	                    WHERE Appr_Step_Act_ID = @Appr_Step_Act_ID";

                    sqlparams = new SqlParameter[]
                    {
                        //kinda weird, but we're passing in Act_ID as the Approval Step Activity ID when in update runtype
	                    new SqlParameter("@Appr_Step_Act_ID", SqlDbType.Int) { Value = Act_ID },

                    };

                    sqa_fmm_act_appr_step_config.Fill_FMM_Act_Appr_Step_Config_DT(si, sqa,FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                    BRApi.ErrorLog.LogMessage(si, "mcm: " + FMM_Act_Appr_Step_Config_DT.Rows[0]["Reg_Config_ID"]);
                    // update existing row
                    FMM_Act_Appr_Step_Config_DT.Rows[0]["Reg_Config_ID"] = Reg_Config_ID;
                    FMM_Act_Appr_Step_Config_DT.Rows[0]["Update_Date"] = DateTime.Now;
                    FMM_Act_Appr_Step_Config_DT.Rows[0]["Update_User"] = si.UserName;

                    // Save Approval Step Activity Config
                    sqa_fmm_act_appr_step_config.Update_FMM_Act_Appr_Step_Config(si, FMM_Act_Appr_Step_Config_DT, sqa);
                }
				}
                return save_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private XFSelectionChangedTaskResult Process_Bulk_Calc_Unit()
        {
            try
            {
                var save_Task_Result = new XFSelectionChangedTaskResult();
                var custom_Subst_Vars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var Cube_ID = Convert.ToInt32(custom_Subst_Vars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_Entity_MFB = custom_Subst_Vars.XFGetValue("IV_FMM_Calc_Unit_Entity_MFB", "0");
                var WF_Channels = custom_Subst_Vars.XFGetValue("BL_FMM_WFChannels", "0");
                var new_Calc_Unit_ID = 0;
                var loop_times = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    connection.Open();

                    // Example: Get the max ID for the "FMM_Calc_Config" table
                    new_Calc_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");

                    var sqa_fmm_calc_unit_config = new SQA_FMM_Calc_Unit_Config(si, connection);
                    var FMM_Calc_Unit_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Cell
                    string sql = @"
									SELECT * 
									FROM FMM_Calc_Unit_Config
									WHERE Cube_ID = @Cube_ID";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID }
                    };

                    sqa_fmm_calc_unit_config.Fill_FMM_Calc_Unit_Config_DT(si, sqa,FMM_Calc_Unit_Config_DT, sql, sqlparams);

                    var EntDimPk = BRApi.Finance.Dim.GetDimPk(si, "DHP_ConsolEntities_Dim");
                    var Calc_Unit_EntList = BRApi.Finance.Members.GetMembersUsingFilter(si, EntDimPk, Calc_Unit_Entity_MFB, true);

                    // Loop through the Calc_Unit_EntList and add new rows to FMM_Calc_Unit_Config_DT
                    foreach (var entity in Calc_Unit_EntList)
                    {
                        var newRow = FMM_Calc_Unit_Config_DT.NewRow();
                        if (loop_times == 0)
                        {
                            newRow["Calc_Unit_ID"] = new_Calc_Unit_ID;
                        }
                        {
                            newRow["Calc_Unit_ID"] = ++new_Calc_Unit_ID;
                        }
                        newRow["Cube_ID"] = Cube_ID;
                        newRow["Entity_MFB"] = entity.Member.Name;
                        newRow["WFChannel"] = WF_Channels;
                        newRow["Status"] = "Build";
                        newRow["Create_Date"] = DateTime.Now;
                        newRow["Create_User"] = si.UserName;
                        newRow["Update_Date"] = DateTime.Now;
                        newRow["Update_User"] = si.UserName;

                        FMM_Calc_Unit_Config_DT.Rows.Add(newRow);
                        loop_times += 1;
                    }

                    sqa_fmm_calc_unit_config.Update_FMM_Calc_Unit_Config(si, FMM_Calc_Unit_Config_DT, sqa);

                    return save_Task_Result;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }
        #endregion
        #region "Copy Models"
        private void Process_Copy_Cube_Config(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Cube_ID", "0"));
            var tgt_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Cube_ID", "0"));
            BRApi.ErrorLog.LogMessage(si, "Hit Copy: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Act_Config_DT = new DataTable();
            var tgt_FMM_Act_Config_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_Appr_Config_DT = new DataTable();
            var tgt_FMM_Appr_Config_DT = new DataTable();
            var src_FMM_Appr_Step_Config_DT = new DataTable();
            var tgt_FMM_Appr_Step_Config_DT = new DataTable();
            var src_FMM_Reg_Config_DT = new DataTable();
            var tgt_FMM_Reg_Config_DT = new DataTable();
            var src_FMM_Col_Config_DT = new DataTable();
            var tgt_FMM_Col_Config_DT = new DataTable();
            var src_FMM_Models_DT = new DataTable();
            var tgt_FMM_Models_DT = new DataTable();
            var src_FMM_Calc_Config_DT = new DataTable();
            var tgt_FMM_Calc_Config_DT = new DataTable();
            var src_FMM_Cell_DT = new DataTable();
            var tgt_FMM_Cell_DT = new DataTable();
            var src_FMM_Src_Cell_DT = new DataTable();
            var tgt_FMM_Src_Cell_DT = new DataTable();
            var src_FMM_Model_Grps_DT = new DataTable();
            var tgt_FMM_Model_Grps_DT = new DataTable();
            var src_FMM_Model_Grp_Assign_DT = new DataTable();
            var tgt_FMM_Model_Grp_Assign_DT = new DataTable();
            var src_FMM_Calc_Unit_Config_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Config_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_Cube_Config_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var sqa = new SqlDataAdapter();
                var sqa_fmm_acct_config = new SQA_FMM_Acct_Config(si, connection);
                var sqa_fmm_act_config = new SQA_FMM_Act_Config(si, connection);
                var sqa_fmm_appr_config = new SQA_FMM_Appr_Config(si, connection);
                var sqa_fmm_appr_step_config = new SQA_FMM_Appr_Step_Config(si, connection);
                var sqa_fmm_calc_config = new SQA_FMM_Calc_Config(si, connection);
                var sqa_fmm_col_config = new SQA_FMM_Col_Config(si, connection);
                var sqa_fmm_cube_config = new SQA_FMM_Cube_Config(si, connection);
                var sqa_fmm_cell = new SQA_FMM_Cell(si, connection);
                var sqa_fmm_model_grp_assign = new SQA_FMM_Model_Grp_Assign(si, connection);
                var sqa_fmm_model_grps = new SQA_FMM_Model_Grps(si, connection);
                var sqa_fmm_models = new SQA_FMM_Models(si, connection);
                var sqa_fmm_reg_config = new SQA_FMM_Reg_Config(si, connection);
                var sqa_fmm_src_cell = new SQA_FMM_Src_Cell(si, connection);
                var sqa_fmm_unit_config = new SQA_FMM_Unit_Config(si, connection);
                var sqa_fmm_calc_unit_assign = new SQA_FMM_Calc_Unit_Assign(si, connection);
                var sqa_fmm_calc_unit_config = new SQA_FMM_Calc_Unit_Config(si, connection);
                #endregion
                connection.Open();
                #region "Get FMM Data"
                // Call for get_FMM_Act_Config_Data
                get_FMM_Act_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_act_config,
                    ref src_FMM_Act_Config_DT, ref tgt_FMM_Act_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_unit_config,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_acct_config,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_appr_config,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Step_Config_Data
                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_appr_step_config,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_reg_config,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_col_config,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_models,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Cell_Data
                get_FMM_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_cell,
                    ref src_FMM_Cell_DT, ref tgt_FMM_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_src_cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_model_grps,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_model_grp_assign,
                    ref src_FMM_Model_Grp_Assign_DT, ref tgt_FMM_Model_Grp_Assign_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_unit_config,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Assign_Data
                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_unit_assign,
                    ref src_FMM_Calc_Unit_Assign_DT, ref tgt_FMM_Calc_Unit_Assign_DT, sql_gbl_get_max_id
                );
                #endregion

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_Act_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hti Copy");
                    Copy_Activities(activity_ConfigRow, ref tgt_FMM_Act_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    // Access Act_ID from the current activity_ConfigRow
                    var curr_srcActivityID = activity_ConfigRow["Act_ID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["Act_ID"]
                                            : -1; // Handle null Act_IDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["Unit_ID"] != DBNull.Value
                        ? (int)unit_ConfigRow["Unit_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Units(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"Unit_ID = {curr_srcUnitID}"))
                        {
                            Copy_Accts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["Reg_Config_ID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["Reg_Config_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"Reg_Config_ID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Appr_ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = Appr_ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)Appr_ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Approvals(Appr_ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Appr_Step_Config_DT.Select($"Appr_ID = {curr_srcApprovalID}"))
                        {
                            Copy_Appr_Steps(approvalstep_ConfigRow, ref tgt_FMM_Appr_Step_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["Model_ID"] != DBNull.Value
                        ? (int)models_ConfigRow["Model_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Models(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"Model_ID = {curr_srcModelID}"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                            var curr_srcCalcID = calc_ConfigRow["Calc_ID"] != DBNull.Value
                            ? (int)calc_ConfigRow["Calc_ID"]
                            : -1; // Handle null Unit_IDs as needed
                            Copy_Calcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Cell(ConfigRow, ref tgt_FMM_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                #endregion
                foreach (var Model_GroupRow in src_FMM_Model_Grps_DT.Rows)
                {

                }
                foreach (var Calc_Unit_ConfigRow in src_FMM_Calc_Unit_Config_DT.Rows)
                {

                }
                sqa_fmm_act_config.Update_FMM_Act_Config(si, tgt_FMM_Act_Config_DT, sqa);
                sqa_fmm_unit_config.Update_FMM_Unit_Config(si, tgt_FMM_Unit_Config_DT, sqa);
                sqa_fmm_acct_config.Update_FMM_Acct_Config(si, tgt_FMM_Acct_Config_DT, sqa);
                sqa_fmm_reg_config.Update_FMM_Reg_Config(si, tgt_FMM_Reg_Config_DT, sqa);
                sqa_fmm_col_config.Update_FMM_Col_Config(si, tgt_FMM_Col_Config_DT, sqa);
                sqa_fmm_appr_config.Update_FMM_Appr_Config(si, tgt_FMM_Appr_Config_DT, sqa);
                sqa_fmm_appr_step_config.Update_FMM_Appr_Step_Config(si, tgt_FMM_Appr_Step_Config_DT, sqa);
                sqa_fmm_models.Update_FMM_Models(si, tgt_FMM_Models_DT, sqa);
                sqa_fmm_calc_config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sqa);
                sqa_fmm_cell.Update_FMM_Cell(si, tgt_FMM_Cell_DT, sqa);
                sqa_fmm_src_cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sqa);
                sqa_fmm_model_grp_assign.Update_FMM_Model_Grp_Assign(si, tgt_FMM_Model_Grp_Assign_DT, sqa);
                sqa_fmm_calc_unit_config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sqa);
                sqa_fmm_calc_unit_assign.Update_FMM_Calc_Unit_Assign(si, tgt_FMM_Calc_Unit_Assign_DT, sqa);
                sqa_fmm_model_grps.Update_FMM_Model_Grps(si, tgt_FMM_Model_Grps_DT, sqa);
                sqa_fmm_cube_config.Update_FMM_Cube_Config(si, tgt_FMM_Cube_Config_DT, sqa);
            }
        }

        private void Process_Copy_Act_Config(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Cube_ID", "0"));
            var tgt_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Cube_ID", "0"));
            var src_Act_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Act_ID", "0"));
            BRApi.ErrorLog.LogMessage(si, "Hit Copy: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Act_Config_DT = new DataTable();
            var tgt_FMM_Act_Config_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_Appr_Config_DT = new DataTable();
            var tgt_FMM_Appr_Config_DT = new DataTable();
            var src_FMM_Appr_Step_Config_DT = new DataTable();
            var tgt_FMM_Appr_Step_Config_DT = new DataTable();
            var src_FMM_Reg_Config_DT = new DataTable();
            var tgt_FMM_Reg_Config_DT = new DataTable();
            var src_FMM_Col_Config_DT = new DataTable();
            var tgt_FMM_Col_Config_DT = new DataTable();
            var src_FMM_Models_DT = new DataTable();
            var tgt_FMM_Models_DT = new DataTable();
            var src_FMM_Calc_Config_DT = new DataTable();
            var tgt_FMM_Calc_Config_DT = new DataTable();
            var src_FMM_Cell_DT = new DataTable();
            var tgt_FMM_Cell_DT = new DataTable();
            var src_FMM_Src_Cell_DT = new DataTable();
            var tgt_FMM_Src_Cell_DT = new DataTable();
            var src_FMM_Model_Grps_DT = new DataTable();
            var tgt_FMM_Model_Grps_DT = new DataTable();
            var src_FMM_Model_Grp_Assign_DT = new DataTable();
            var tgt_FMM_Model_Grp_Assign_DT = new DataTable();
            var src_FMM_Calc_Unit_Config_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Config_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_DT = new DataTable();
            var src_FMM_Cube_Config_DT = new DataTable();
            var tgt_FMM_Cube_Config_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var sqa = new SqlDataAdapter();
                var sqa_fmm_acct_config = new SQA_FMM_Acct_Config(si, connection);
                var sqa_fmm_act_config = new SQA_FMM_Act_Config(si, connection);
                var sqa_fmm_appr_config = new SQA_FMM_Appr_Config(si, connection);
                var sqa_fmm_appr_step_config = new SQA_FMM_Appr_Step_Config(si, connection);
                var sqa_fmm_calc_config = new SQA_FMM_Calc_Config(si, connection);
                var sqa_fmm_col_config = new SQA_FMM_Col_Config(si, connection);
                var sqa_fmm_cube_config = new SQA_FMM_Cube_Config(si, connection);
                var sqa_fmm_cell = new SQA_FMM_Cell(si, connection);
                var sqa_fmm_model_grp_assign = new SQA_FMM_Model_Grp_Assign(si, connection);
                var sqa_fmm_model_grps = new SQA_FMM_Model_Grps(si, connection);
                var sqa_fmm_models = new SQA_FMM_Models(si, connection);
                var sqa_fmm_reg_config = new SQA_FMM_Reg_Config(si, connection);
                var sqa_fmm_src_cell = new SQA_FMM_Src_Cell(si, connection);
                var sqa_fmm_unit_config = new SQA_FMM_Unit_Config(si, connection);
                var sqa_fmm_calc_unit_assign = new SQA_FMM_Calc_Unit_Assign(si, connection);
                var sqa_fmm_calc_unit_config = new SQA_FMM_Calc_Unit_Config(si, connection);
                #endregion
                connection.Open();
                #region "Get FMM Data"
                // Call for get_FMM_Act_Config_Data
                get_FMM_Act_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_act_config,
                    ref src_FMM_Act_Config_DT, ref tgt_FMM_Act_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_unit_config,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_acct_config,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_appr_config,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Step_Config_Data
                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_appr_step_config,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_reg_config,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_col_config,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_models,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Cell_Data
                get_FMM_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_cell,
                    ref src_FMM_Cell_DT, ref tgt_FMM_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_src_cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_model_grps,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_model_grp_assign,
                    ref src_FMM_Model_Grp_Assign_DT, ref tgt_FMM_Model_Grp_Assign_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_unit_config,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Assign_Data
                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets, sqa_fmm_calc_unit_assign,
                    ref src_FMM_Calc_Unit_Assign_DT, ref tgt_FMM_Calc_Unit_Assign_DT, sql_gbl_get_max_id
                );
                #endregion

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_Act_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hti Copy");
                    Copy_Activities(activity_ConfigRow, ref tgt_FMM_Act_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    // Access Act_ID from the current activity_ConfigRow
                    var curr_srcActivityID = activity_ConfigRow["Act_ID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["Act_ID"]
                                            : -1; // Handle null Act_IDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["Unit_ID"] != DBNull.Value
                        ? (int)unit_ConfigRow["Unit_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Units(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"Unit_ID = {curr_srcUnitID}"))
                        {
                            Copy_Accts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["Reg_Config_ID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["Reg_Config_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"Reg_Config_ID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Appr_ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = Appr_ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)Appr_ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Approvals(Appr_ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Appr_Step_Config_DT.Select($"Appr_ID = {curr_srcApprovalID}"))
                        {
                            Copy_Appr_Steps(approvalstep_ConfigRow, ref tgt_FMM_Appr_Step_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["Model_ID"] != DBNull.Value
                        ? (int)models_ConfigRow["Model_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Models(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"Model_ID = {curr_srcModelID}"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                            var curr_srcCalcID = calc_ConfigRow["Calc_ID"] != DBNull.Value
                            ? (int)calc_ConfigRow["Calc_ID"]
                            : -1; // Handle null Unit_IDs as needed
                            Copy_Calcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Cell(ConfigRow, ref tgt_FMM_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                #endregion
                foreach (var Model_GroupRow in src_FMM_Model_Grps_DT.Rows)
                {

                }
                foreach (var Calc_Unit_ConfigRow in src_FMM_Calc_Unit_Config_DT.Rows)
                {

                }

                sqa_fmm_act_config.Update_FMM_Act_Config(si, tgt_FMM_Act_Config_DT, sqa);
                sqa_fmm_unit_config.Update_FMM_Unit_Config(si, tgt_FMM_Unit_Config_DT, sqa);
                sqa_fmm_acct_config.Update_FMM_Acct_Config(si, tgt_FMM_Acct_Config_DT, sqa);
                sqa_fmm_reg_config.Update_FMM_Reg_Config(si, tgt_FMM_Reg_Config_DT, sqa);
                sqa_fmm_col_config.Update_FMM_Col_Config(si, tgt_FMM_Col_Config_DT, sqa);
                sqa_fmm_appr_config.Update_FMM_Appr_Config(si, tgt_FMM_Appr_Config_DT, sqa);
                sqa_fmm_appr_step_config.Update_FMM_Appr_Step_Config(si, tgt_FMM_Appr_Step_Config_DT, sqa);
                sqa_fmm_models.Update_FMM_Models(si, tgt_FMM_Models_DT, sqa);
                sqa_fmm_calc_config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sqa);
                sqa_fmm_cell.Update_FMM_Cell(si, tgt_FMM_Cell_DT, sqa);
                sqa_fmm_src_cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sqa);
                sqa_fmm_model_grp_assign.Update_FMM_Model_Grp_Assign(si, tgt_FMM_Model_Grp_Assign_DT, sqa);
                sqa_fmm_calc_unit_config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sqa);
                sqa_fmm_calc_unit_assign.Update_FMM_Calc_Unit_Assign(si, tgt_FMM_Calc_Unit_Assign_DT, sqa);
                sqa_fmm_calc_unit_config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sqa);
                sqa_fmm_model_grps.Update_FMM_Model_Grps(si, tgt_FMM_Model_Grps_DT, sqa);
                sqa_fmm_cube_config.Update_FMM_Cube_Config(si, tgt_FMM_Cube_Config_DT, sqa);
            }
        }


        private XFSelectionChangedTaskResult Copy_Model_Config(XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var srcActivities_DT = new DataTable("srcActivities");
            var srcWF_DataUnits_DT = new DataTable("srcWF_DataUnits");
            var srcRegisters_DT = new DataTable("srcRegisters");
            var srcApprovals_DT = new DataTable("srcApprovals");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                connection.Open();
                var srcActivities_sqlDataAdapter = new SqlDataAdapter();
                // Define the select query and parameters
                string srcActivities_selectQuery = @"
		        	SELECT CubeID, Name
		       		FROM Cube
		       		WHERE IsTopLevelCube = 1";
                // Create an array of SqlParameter objects
                var srcActivities_parameters = new SqlParameter[]
                {
                };
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, srcActivities_sqlDataAdapter, srcActivities_DT, srcActivities_selectQuery, srcActivities_parameters);

            }
            return XFCopyTaskResult;
        }

        private void Process_Calc_Copy(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Cube_ID", "0"));
            var tgt_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Cube_ID", "0"));
            var src_Act_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Act_ID", "0"));
            var tgt_Act_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Act_ID", "0"));
            var src_Model_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Model_ID", "0"));
            var tgt_Model_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Model_ID", "0"));
            var src_Calc_IDs = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Calc_IDs", "0"));
            GBL_Curr_FMM_Act_ID = tgt_Act_ID;
            GBL_Curr_FMM_Models_ID = tgt_Model_ID;
            BRApi.ErrorLog.LogMessage(si, "Hit Copy Calc: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Calc_Config_DT = new DataTable();
            var tgt_FMM_Calc_Config_DT = new DataTable();
            var src_FMM_Cell_DT = new DataTable();
            var tgt_FMM_Cell_DT = new DataTable();
            var src_FMM_Src_Cell_DT = new DataTable();
            var tgt_FMM_Src_Cell_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                var sqa = new SqlDataAdapter();
                var SQA_FMM_Calc_Config = new SQA_FMM_Calc_Config(si, connection);
                var SQA_FMM_Cell = new SQA_FMM_Cell(si, connection);
                var SQA_FMM_Src_Cell = new SQA_FMM_Src_Cell(si, connection);
                #endregion
                connection.Open();
                #region "Get MCM Data"

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@Calc_ID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_gbl_get_datasets, SQA_FMM_Calc_Config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Cell_Data
                get_FMM_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@Calc_ID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_gbl_get_datasets, SQA_FMM_Cell,
                    ref src_FMM_Cell_DT, ref tgt_FMM_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@Calc_ID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_gbl_get_datasets, SQA_FMM_Src_Cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                #endregion

                #region "Copy Calc Data"
                foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                    var curr_srcCalcID = calc_ConfigRow["Calc_ID"] != DBNull.Value
                    ? (int)calc_ConfigRow["Calc_ID"]
                    : -1; // Handle null Unit_IDs as needed
                    Copy_Calcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Model Calc Copy");
                    foreach (DataRow ConfigRow in src_FMM_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                    {
                        Copy_Cell(ConfigRow, ref tgt_FMM_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                    foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                    {
                        Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                }
                #endregion

                SQA_FMM_Calc_Config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sqa);
                SQA_FMM_Cell.Update_FMM_Cell(si, tgt_FMM_Cell_DT, sqa);
                SQA_FMM_Src_Cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sqa);

            }
        }

        #region "Get Data Functions"
        private void get_FMM_Act_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Act_Config sqa_fmm_act_config, ref DataTable src_FMM_Act_Config_DT, ref DataTable tgt_FMM_Act_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Act_Config
				{Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Act_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Act_Config
				{Where_Clause}";
            sqa_fmm_act_config.Fill_FMM_Act_Config_DT(si, sqa,tgt_FMM_Act_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Act_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Config", "Act_ID");
        }
        private void get_FMM_Unit_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Unit_Config SQA_FMM_Unit_Config, ref DataTable src_FMM_Unit_Config_DT, ref DataTable tgt_FMM_Unit_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Unit_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Unit_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Unit_Config
		        {Where_Clause}";
            SQA_FMM_Unit_Config.Fill_FMM_Unit_Config_DT(si, sqa,tgt_FMM_Unit_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Unit_Config", "Unit_ID");
        }
        private void get_FMM_Acct_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Acct_Config SQA_FMM_Acct_Config, ref DataTable src_FMM_Acct_Config_DT, ref DataTable tgt_FMM_Acct_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Acct_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Acct_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Acct_Config
		        {Where_Clause}";
            SQA_FMM_Acct_Config.Fill_FMM_Acct_Config_DT(si, sqa, tgt_FMM_Acct_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Acct_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Acct_Config", "Acct_ID");
        }
        private void get_FMM_Appr_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Appr_Config SQA_FMM_Appr_Config, ref DataTable src_FMM_Appr_Config_DT, ref DataTable tgt_FMM_Appr_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Appr_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Appr_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Appr_Config
		        {Where_Clause}";
            SQA_FMM_Appr_Config.Fill_FMM_Appr_Config_DT(si, sqa, tgt_FMM_Appr_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Appr_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Config", "Appr_ID");
        }
        private void get_FMM_Appr_Step_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Appr_Step_Config sqa_fmm_appr_step_config, ref DataTable src_FMM_Appr_Step_Config_DT, ref DataTable tgt_FMM_Appr_Step_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Appr_Step_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Appr_Step_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Appr_Step_Config
		        {Where_Clause}";
            sqa_fmm_appr_step_config.Fill_FMM_Appr_Step_Config_DT(si, sqa, tgt_FMM_Appr_Step_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Appr_Step_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "Appr_Step_ID");
        }
        private void get_FMM_Reg_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Reg_Config SQA_FMM_Reg_Config, ref DataTable src_FMM_Reg_Config_DT, ref DataTable tgt_FMM_Reg_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Reg_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Reg_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Reg_Config
		        {Where_Clause}";
            SQA_FMM_Reg_Config.Fill_FMM_Reg_Config_DT(si, sqa, tgt_FMM_Reg_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Reg_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Config", "Reg_Config_ID");
        }
        private void get_FMM_Col_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Col_Config SQA_FMM_Col_Config, ref DataTable src_FMM_Col_Config_DT, ref DataTable tgt_FMM_Col_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Col_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Col_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Col_Config
		        {Where_Clause}";
            SQA_FMM_Col_Config.Fill_FMM_Col_Config_DT(si, sqa, tgt_FMM_Col_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Col_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
        }
        private void get_FMM_Models_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Models SQA_FMM_Models, ref DataTable src_FMM_Models_DT, ref DataTable tgt_FMM_Models_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Models
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Models_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Models
		        {Where_Clause}";
            SQA_FMM_Models.Fill_FMM_Models_DT(si, sqa, tgt_FMM_Models_DT, sql, tgt_sqlparams);

            GBL_FMM_Models_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "Model_ID");
        }
        private void get_FMM_Calc_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Calc_Config SQA_FMM_Calc_Config, ref DataTable src_FMM_Calc_Config_DT, ref DataTable tgt_FMM_Calc_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Calc_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Calc_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Calc_Config
		        {Where_Clause}";
            SQA_FMM_Calc_Config.Fill_FMM_Calc_Config_DT(si, sqa, tgt_FMM_Calc_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Calc_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "Calc_ID");
        }
        private void get_FMM_Cell_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Cell SQA_FMM_Cell, ref DataTable src_FMM_Cell_DT, ref DataTable tgt_FMM_Cell_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Cell
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Cell_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Cell
		        {Where_Clause}";
            SQA_FMM_Cell.Fill_FMM_Cell_DT(si, sqa, tgt_FMM_Cell_DT, sql, tgt_sqlparams);

            GBL_FMM_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Cell", "OS_Cell_ID");
        }
        private void get_FMM_Src_Cell_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Src_Cell SQA_FMM_Src_Cell, ref DataTable src_FMM_Src_Cell_DT, ref DataTable tgt_FMM_Src_Cell_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Src_Cell
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Src_Cell_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Src_Cell
		        {Where_Clause}";
            SQA_FMM_Src_Cell.Fill_FMM_Src_Cell_DT(si, sqa, tgt_FMM_Src_Cell_DT, sql, tgt_sqlparams);

            GBL_FMM_Src_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Src_Cell", "Cell_ID");
        }
        private void get_FMM_Model_Grps_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Model_Grps SQA_FMM_Model_Grps, ref DataTable src_FMM_Model_Grps_DT, ref DataTable tgt_FMM_Model_Grps_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Model_Grps
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Model_Grps_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Model_Grps
		        {Where_Clause}";
            SQA_FMM_Model_Grps.Fill_FMM_Model_Grps_DT(si, sqa,tgt_FMM_Model_Grps_DT, sql, tgt_sqlparams);

            GBL_FMM_Model_Grps_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grps", "Model_Grp_ID");
        }
        private void get_FMM_Model_Grp_Assign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Model_Grp_Assign SQA_FMM_Model_Grp_Assign, ref DataTable src_FMM_Model_Grp_Assign_DT, ref DataTable tgt_FMM_Model_Grp_Assign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Model_Grp_Assign
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Model_Grp_Assign_DT, sql, src_sqlparams);

            var tgt_sqa = new SqlDataAdapter();
            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Model_Grp_Assign
		        {Where_Clause}";
            SQA_FMM_Model_Grp_Assign.Fill_FMM_Model_Grp_Assign_DT(si, sqa,tgt_FMM_Model_Grp_Assign_DT, sql, tgt_sqlparams);

            GBL_FMM_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grp_Assign", "Model_Grp_Assign_ID");
        }
        private void get_FMM_Calc_Unit_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Calc_Unit_Config SQA_FMM_Calc_Unit_Config, ref DataTable src_FMM_Calc_Unit_Config_DT, ref DataTable tgt_FMM_Calc_Unit_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Config
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Calc_Unit_Config_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Config
		        {Where_Clause}";
            SQA_FMM_Calc_Unit_Config.Fill_FMM_Calc_Unit_Config_DT(si, sqa,tgt_FMM_Calc_Unit_Config_DT, sql, tgt_sqlparams);

            GBL_FMM_Calc_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");
        }
        private void get_FMM_Calc_Unit_Assign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, SQA_FMM_Calc_Unit_Assign sqa_fmm_calc_unit_assign, ref DataTable src_FMM_Calc_Unit_Assign_DT, ref DataTable tgt_FMM_Calc_Unit_Assign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            var Where_Clause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Assign
		        {Where_Clause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa,src_FMM_Calc_Unit_Assign_DT, sql, src_sqlparams);

            Where_Clause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Assign
		        {Where_Clause}";
            sqa_fmm_calc_unit_assign.Fill_FMM_Calc_Unit_Assign_DT(si, sqa,tgt_FMM_Calc_Unit_Assign_DT, sql, tgt_sqlparams);

            GBL_FMM_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Assign", "Calc_Unit_Assign_ID");
        }
        #endregion

        #region "Copy Model Data"
        private void Copy_Activities(DataRow src_FMM_Act_Config_Row, ref DataTable tgt_FMM_Act_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID activities for duplicate Name and Calc_Type
            bool isDuplicate = tgt_FMM_Act_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Act_Config_Row["Name"].ToString() &&
                            row["Calc_Type"].ToString() == src_FMM_Act_Config_Row["Calc_Type"].ToString());

            // If not duplicate, add the new Activity Name and Calc_Type to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Act_ID += 1;
                GBL_Curr_FMM_Act_ID = GBL_FMM_Act_ID;
                DataRow newTargetRow = tgt_FMM_Act_Config_DT.NewRow();

                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_FMM_Act_ID;
                newTargetRow["Name"] = src_FMM_Act_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Calc_Type"] = src_FMM_Act_Config_Row.Field<string>("Calc_Type") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as "Build"
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // Set the appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // Set the appropriate user context

                tgt_FMM_Act_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Act_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Act_Config_Row["Name"].ToString() &&
                                           row["Calc_Type"].ToString() == src_FMM_Act_Config_Row["Calc_Type"].ToString());
                if (existingRow != null)
                {
                    GBL_Curr_FMM_Act_ID = existingRow.Field<int>("Act_ID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // Set the appropriate user context
                }
            }
        }
        private void Copy_Units(DataRow src_FMM_Unit_Config_Row, ref DataTable tgt_FMM_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID units for duplicate Unit_Name
            bool isDuplicate = tgt_FMM_Unit_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Unit to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Unit_ID += 1;
                GBL_Curr_FMM_Unit_ID = GBL_FMM_Unit_ID;
                DataRow newTargetRow = tgt_FMM_Unit_Config_DT.NewRow();

                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Unit_ID"] = GBL_FMM_Unit_ID;
                newTargetRow["Name"] = src_FMM_Unit_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // Set appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_FMM_Unit_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Unit_ID = existingRow.Field<int>("Unit_ID");
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Accts(DataRow src_FMM_Acct_Config_Row, ref DataTable tgt_FMM_Acct_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID accounts for duplicate Acct_Name
            bool isDuplicate = tgt_FMM_Acct_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Unit_ID"].ToString() == GBL_Curr_FMM_Unit_ID.ToString());

            // If not duplicate, add the new Account to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Acct_ID += 1;
                GBL_Curr_FMM_Acct_ID = GBL_FMM_Acct_ID;
                DataRow newTargetRow = tgt_FMM_Acct_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Unit_ID"] = GBL_Curr_FMM_Unit_ID;
                newTargetRow["Acct_ID"] = GBL_FMM_Acct_ID;
                newTargetRow["Name"] = src_FMM_Acct_Config_Row.Field<string>("Acct_Name") ?? string.Empty; // Handle nulls
                newTargetRow["Acct_Map_Logic"] = src_FMM_Acct_Config_Row.Field<string>("Acct_Map_Logic") ?? string.Empty; // Handle nulls
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Acct_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Acct_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                           row["Unit_ID"].ToString() == GBL_Curr_FMM_Unit_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Acct_ID = existingRow.Field<int>("Acct_ID");

                    // Update fields, handle nulls using Field<T>
                    existingRow["Acct_Map_Logic"] = src_FMM_Acct_Config_Row.Field<string>("Acct_Map_Logic") ?? existingRow.Field<string>("Acct_Map_Logic");
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Reg_Config(DataRow src_Reg_Config_Row, ref DataTable tgt_Reg_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID register config for duplicate Register_Name
            bool isDuplicate = tgt_Reg_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Register Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Reg_Config_ID += 1;
                GBL_Curr_FMM_Reg_Config_ID = GBL_FMM_Reg_Config_ID;
                DataRow newTargetRow = tgt_Reg_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Reg_Config_ID"] = GBL_FMM_Reg_Config_ID;
                newTargetRow["Name"] = src_Reg_Config_Row.Field<string>("Name") ?? string.Empty; // Handle nulls
                newTargetRow["Time_Phasing"] = src_Reg_Config_Row.Field<string>("Time_Phasing") ?? string.Empty; // Handle nulls
                newTargetRow["Start_Dt_Src"] = src_Reg_Config_Row.Field<string>("Start_Dt_Src") ?? string.Empty; // Handle nulls
                newTargetRow["End_Dt_Src"] = src_Reg_Config_Row.Field<string>("End_Dt_Src") ?? string.Empty; // Handle nulls
                newTargetRow["Appr_Config"] = src_Reg_Config_Row.Field<string>("Appr_Config") ?? string.Empty; // Handle nulls
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // Set appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_Reg_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Reg_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Reg_Config_ID = existingRow.Field<int>("Reg_Config_ID");

                    // Update fields, handle nulls using Field<T>
                    existingRow["Time_Phasing"] = src_Reg_Config_Row.Field<string>("Time_Phasing") ?? existingRow.Field<string>("Time_Phasing");
                    existingRow["Start_Dt_Src"] = src_Reg_Config_Row.Field<string>("Start_Dt_Src") ?? existingRow.Field<string>("Start_Dt_Src");
                    existingRow["End_Dt_Src"] = src_Reg_Config_Row.Field<string>("End_Dt_Src") ?? existingRow.Field<string>("End_Dt_Src");
                    existingRow["Appr_Config"] = src_Reg_Config_Row.Field<string>("Appr_Config") ?? existingRow.Field<string>("Appr_Config");
                    existingRow["Status"] = row_Status;
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Col_Config(DataRow src_Col_Config_Row, ref DataTable tgt_Col_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID column config for duplicate Col_Name
            bool isDuplicate = tgt_Col_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Reg_Config_ID"].ToString() == GBL_Curr_FMM_Reg_Config_ID.ToString());

            // If not duplicate, add the new Column Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Col_ID += 1;
                GBL_Curr_FMM_Col_ID = GBL_FMM_Col_ID;
                DataRow newTargetRow = tgt_Col_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Reg_Config_ID"] = GBL_Curr_FMM_Reg_Config_ID;
                newTargetRow["Col_ID"] = GBL_FMM_Col_ID;
                newTargetRow["Name"] = src_Col_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                newTargetRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                newTargetRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                newTargetRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                newTargetRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                newTargetRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                newTargetRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                newTargetRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Col_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Col_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                                    row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                    row["Reg_Config_ID"].ToString() == GBL_Curr_FMM_Reg_Config_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Col_ID = existingRow.Field<int>("Col_ID");
                    existingRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                    existingRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                    existingRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                    existingRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                    existingRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                    existingRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                    existingRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                    existingRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                    existingRow["Status"] = row_Status; // Set initial status as appropriate
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Approvals(DataRow src_FMM_Appr_Config_Row, ref DataTable tgt_FMM_Appr_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            BRApi.ErrorLog.LogMessage(si, "Hit Approval");
            var row_Status = "Build";
            // Check the target Cube_ID approvals for duplicate Appr_Name
            bool isDuplicate = tgt_FMM_Appr_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Appr_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Approval to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Appr_ID += 1;
                GBL_Curr_FMM_Appr_ID = GBL_FMM_Appr_ID;
                DataRow newTargetRow = tgt_FMM_Appr_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Appr_ID"] = GBL_FMM_Appr_ID;
                newTargetRow["Name"] = src_FMM_Appr_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Appr_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Appr_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Appr_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Appr_ID = existingRow.Field<int>("Appr_ID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Appr_Steps(DataRow src_FMM_Appr_Step_Config_Row, ref DataTable tgt_FMM_Appr_Step_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID approval steps for duplicate Step_Name
            bool isDuplicate = tgt_FMM_Appr_Step_Config_DT.AsEnumerable()
                .Any(row => row["Step_Num"].ToString() == src_FMM_Appr_Step_Config_Row["Step_Num"].ToString() &&
                            row["WFProfile_Step"].ToString() == src_FMM_Appr_Step_Config_Row["WFProfile_Step"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Appr_ID"].ToString() == GBL_Curr_FMM_Appr_ID.ToString());

            // If not duplicate, add the new Approval Step to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Appr_Step_ID += 1;
                GBL_Curr_FMM_Appr_Step_ID = GBL_FMM_Appr_Step_ID;
                DataRow newTargetRow = tgt_FMM_Appr_Step_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_FMM_Act_ID;
                newTargetRow["Appr_ID"] = GBL_FMM_Appr_ID;
                newTargetRow["Appr_Step_ID"] = GBL_FMM_Appr_Step_ID;
                newTargetRow["Step_Num"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Step_Num") ?? 0;
                newTargetRow["WFProfile_Step"] = src_FMM_Appr_Step_Config_Row.Field<Guid?>("WFProfile_Step") ?? Guid.Empty;
                newTargetRow["Appr_User_Group"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_User_Group") ?? string.Empty;
                newTargetRow["Appr_Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Logic") ?? string.Empty;
                newTargetRow["Appr_Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Item") ?? string.Empty;
                newTargetRow["Appr_Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Level") ?? 0;
                newTargetRow["Appr_Config"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Config") ?? 0;
                newTargetRow["Init_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Init_Status") ?? string.Empty;
                newTargetRow["Appr_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Status") ?? string.Empty;
                newTargetRow["Rej_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Rej_Status") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Appr_Step_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Appr_Step_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Step_Num"].ToString() == src_FMM_Appr_Step_Config_Row["Step_Num"].ToString() &&
                                           row["WFProfile_Step"].ToString() == src_FMM_Appr_Step_Config_Row["WFProfile_Step"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                           row["Appr_ID"].ToString() == GBL_Curr_FMM_Appr_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Appr_Step_ID = existingRow.Field<int>("Appr_Step_ID");
                    existingRow["Appr_User_Group"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_User_Group") ?? string.Empty;
                    existingRow["Appr_Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Logic") ?? string.Empty;
                    existingRow["Appr_Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Item") ?? string.Empty;
                    existingRow["Appr_Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Level") ?? 0;
                    existingRow["Appr_Config"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Config") ?? 0;
                    existingRow["Init_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Init_Status") ?? string.Empty;
                    existingRow["Appr_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Status") ?? string.Empty;
                    existingRow["Rej_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Rej_Status") ?? string.Empty;
                    existingRow["Status"] = row_Status; // Set initial status as appropriate
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Models(DataRow src_Model_Config_Row, ref DataTable tgt_Model_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID model config for duplicate Model_Name
            bool isDuplicate = tgt_Model_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Model Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Models_ID += 1;
                GBL_Curr_FMM_Models_ID = GBL_FMM_Models_ID;
                DataRow newTargetRow = tgt_Model_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Model_ID"] = GBL_FMM_Models_ID;
                newTargetRow["Name"] = src_Model_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Models_ID = existingRow.Field<int>("Model_ID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calcs(DataRow src_Calc_Config_Row, ref DataTable tgt_Calc_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult, string RunType)
        {
            var row_Status = "Build";
            // Check the target Cube_ID calculation config for duplicate Calc_Name and Calc_Type
            bool isDuplicate = tgt_Calc_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString());

            // If not duplicate, add the new Calculation Config to the target DataTable
            if (RunType == "Model Calc Copy" || !isDuplicate)
            {
                GBL_FMM_Calc_ID += 1;
                GBL_Curr_FMM_Calc_ID = GBL_FMM_Calc_ID;

                DataRow newTargetRow = tgt_Calc_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Model_ID"] = GBL_Curr_FMM_Models_ID;
                newTargetRow["Calc_ID"] = GBL_FMM_Calc_ID;
                if (RunType == "Model Calc Copy")
                {
                    // Get the name from the source row
                    string calcName = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;

                    // Append " - Copy" to the name
                    newTargetRow["Name"] = calcName + " - Copy";
                }
                else
                {
                    // If RunType is not "Model Calc Copy", assign the name without modification
                    newTargetRow["Name"] = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;
                }
                newTargetRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                newTargetRow["Calc_Condition"] = src_Calc_Config_Row.Field<string>("Calc_Condition") ?? string.Empty;
                newTargetRow["Calc_Explanation"] = src_Calc_Config_Row.Field<string>("Calc_Explanation") ?? string.Empty;
                newTargetRow["Balanced_Buffer"] = src_Calc_Config_Row.Field<string>("Balanced_Buffer") ?? string.Empty;
                newTargetRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                newTargetRow["Unbal_Calc"] = src_Calc_Config_Row.Field<string>("Unbal_Calc") ?? string.Empty;
                newTargetRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                newTargetRow["Time_Phasing"] = src_Calc_Config_Row.Field<string>("Time_Phasing") ?? string.Empty;
                newTargetRow["Input_Frequency"] = src_Calc_Config_Row.Field<string>("Input_Frequency") ?? string.Empty;
                newTargetRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                newTargetRow["Business_Rule_Calc"] = src_Calc_Config_Row.Field<bool?>("Business_Rule_Calc") ?? false;
                newTargetRow["Business_Rule_Calc_Name"] = src_Calc_Config_Row.Field<string>("Business_Rule_Calc_Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status;
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Calc_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                           row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Calc_ID = existingRow.Field<int>("Calc_ID");
                    existingRow["Status"] = row_Status;
                    existingRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                    existingRow["Calc_Condition"] = src_Calc_Config_Row.Field<string>("Calc_Condition") ?? string.Empty;
                    existingRow["Calc_Explanation"] = src_Calc_Config_Row.Field<string>("Calc_Explanation") ?? string.Empty;
                    existingRow["Balanced_Buffer"] = src_Calc_Config_Row.Field<string>("Balanced_Buffer") ?? string.Empty;
                    existingRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                    existingRow["Unbal_Calc"] = src_Calc_Config_Row.Field<string>("Unbal_Calc") ?? string.Empty;
                    existingRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                    existingRow["Time_Phasing"] = src_Calc_Config_Row.Field<string>("Time_Phasing") ?? string.Empty;
                    existingRow["Input_Frequency"] = src_Calc_Config_Row.Field<string>("Input_Frequency") ?? string.Empty;
                    existingRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                    existingRow["Business_Rule_Calc"] = src_Calc_Config_Row.Field<bool?>("Business_Rule_Calc") ?? false;
                    existingRow["Business_Rule_Calc_Name"] = src_Calc_Config_Row.Field<string>("Business_Rule_Calc_Name") ?? string.Empty;
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Cell(DataRow src_Cell_Config_Row, ref DataTable tgt_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID destination cell config for duplicate Cell_Name and Cell_Type
            bool isDuplicate = tgt_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString() &&
                            row["Calc_ID"].ToString() == GBL_Curr_FMM_Calc_ID.ToString());

            // If not duplicate, add the new Destination Cell Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Cell_ID += 1;
                GBL_Curr_FMM_Cell_ID = GBL_FMM_Cell_ID;

                DataRow newTargetRow = tgt_Cell_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID; // Correct Act_ID assignment
                newTargetRow["Model_ID"] = GBL_Curr_FMM_Models_ID;
                newTargetRow["Calc_ID"] = GBL_Curr_FMM_Calc_ID;
                newTargetRow["OS_Cell_ID"] = GBL_FMM_Cell_ID;

                // Handle nullable fields
                newTargetRow["Location"] = src_Cell_Config_Row.Field<string>("Location") ?? string.Empty;
                newTargetRow["Calc_Plan_Units"] = src_Cell_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                newTargetRow["Acct"] = src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                newTargetRow["View"] = src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                newTargetRow["Origin"] = src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                newTargetRow["IC"] = src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                newTargetRow["Flow"] = src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                newTargetRow["UD1"] = src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                newTargetRow["UD2"] = src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                newTargetRow["UD3"] = src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                newTargetRow["UD4"] = src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                newTargetRow["UD5"] = src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                newTargetRow["UD6"] = src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                newTargetRow["UD7"] = src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                newTargetRow["UD8"] = src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                newTargetRow["OS_Time_Filter"] = src_Cell_Config_Row.Field<string>("OS_Time_Filter") ?? string.Empty;
                newTargetRow["OS_Acct_Filter"] = src_Cell_Config_Row.Field<string>("OS_Acct_Filter") ?? string.Empty;
                newTargetRow["OS_Origin_Filter"] = src_Cell_Config_Row.Field<string>("OS_Origin_Filter") ?? string.Empty;
                newTargetRow["OS_IC_Filter"] = src_Cell_Config_Row.Field<string>("OS_IC_Filter") ?? string.Empty;
                newTargetRow["OS_Flow_Filter"] = src_Cell_Config_Row.Field<string>("OS_Flow_Filter") ?? string.Empty;
                newTargetRow["OS_UD1_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD1_Filter") ?? string.Empty;
                newTargetRow["OS_UD2_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD2_Filter") ?? string.Empty;
                newTargetRow["OS_UD3_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD3_Filter") ?? string.Empty;
                newTargetRow["OS_UD4_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD4_Filter") ?? string.Empty;
                newTargetRow["OS_UD5_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD5_Filter") ?? string.Empty;
                newTargetRow["OS_UD6_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD6_Filter") ?? string.Empty;
                newTargetRow["OS_UD7_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD7_Filter") ?? string.Empty;
                newTargetRow["OS_UD8_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD8_Filter") ?? string.Empty;
                newTargetRow["OS_Conditional_Filter"] = src_Cell_Config_Row.Field<string>("OS_Conditional_Filter") ?? string.Empty;
                newTargetRow["OS_Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("OS_Curr_Cube_Buffer_Filter") ?? string.Empty;
                newTargetRow["Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Buffer_Filter") ?? string.Empty;
                newTargetRow["OS_Cell_Logic"] = src_Cell_Config_Row.Field<string>("OS_Cell_Logic") ?? string.Empty;
                newTargetRow["OS_SQL_Logic"] = src_Cell_Config_Row.Field<string>("OS_SQL_Logic") ?? string.Empty;

                tgt_Cell_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                            row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString() &&
                                           row["Calc_ID"].ToString() == GBL_Curr_FMM_Calc_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Cell_ID = existingRow.Field<int>("OS_Cell_ID");
                    existingRow["Location"] = src_Cell_Config_Row.Field<string>("Location") ?? string.Empty;
                    existingRow["Calc_Plan_Units"] = src_Cell_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                    existingRow["Acct"] = src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existingRow["View"] = src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                    existingRow["Origin"] = src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existingRow["IC"] = src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                    existingRow["Flow"] = src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existingRow["UD1"] = src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existingRow["UD2"] = src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existingRow["UD3"] = src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existingRow["UD4"] = src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existingRow["UD5"] = src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existingRow["UD6"] = src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existingRow["UD7"] = src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existingRow["UD8"] = src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existingRow["OS_Time_Filter"] = src_Cell_Config_Row.Field<string>("OS_Time_Filter") ?? string.Empty;
                    existingRow["OS_Acct_Filter"] = src_Cell_Config_Row.Field<string>("OS_Acct_Filter") ?? string.Empty;
                    existingRow["OS_Origin_Filter"] = src_Cell_Config_Row.Field<string>("OS_Origin_Filter") ?? string.Empty;
                    existingRow["OS_IC_Filter"] = src_Cell_Config_Row.Field<string>("OS_IC_Filter") ?? string.Empty;
                    existingRow["OS_Flow_Filter"] = src_Cell_Config_Row.Field<string>("OS_Flow_Filter") ?? string.Empty;
                    existingRow["OS_UD1_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD1_Filter") ?? string.Empty;
                    existingRow["OS_UD2_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD2_Filter") ?? string.Empty;
                    existingRow["OS_UD3_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD3_Filter") ?? string.Empty;
                    existingRow["OS_UD4_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD4_Filter") ?? string.Empty;
                    existingRow["OS_UD5_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD5_Filter") ?? string.Empty;
                    existingRow["OS_UD6_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD6_Filter") ?? string.Empty;
                    existingRow["OS_UD7_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD7_Filter") ?? string.Empty;
                    existingRow["OS_UD8_Filter"] = src_Cell_Config_Row.Field<string>("OS_UD8_Filter") ?? string.Empty;
                    existingRow["OS_Conditional_Filter"] = src_Cell_Config_Row.Field<string>("OS_Conditional_Filter") ?? string.Empty;
                    existingRow["OS_Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("OS_Curr_Cube_Buffer_Filter") ?? string.Empty;
                    existingRow["Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Buffer_Filter") ?? string.Empty;
                    existingRow["OS_Cell_Logic"] = src_Cell_Config_Row.Field<string>("OS_Cell_Logic") ?? string.Empty;
                    existingRow["OS_SQL_Logic"] = src_Cell_Config_Row.Field<string>("OS_SQL_Logic") ?? string.Empty;
                }
            }
        }
        private void Copy_Src_Cell(DataRow src_Src_Cell_Config_Row, ref DataTable tgt_Src_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            GBL_FMM_Src_Cell_ID += 1;
            GBL_Curr_FMM_Src_Cell_ID = GBL_FMM_Src_Cell_ID;
            // Check the target Cube_ID source cell config for duplicate Calc_Src_Item and Calc_Src_Type
            bool isDuplicate = tgt_Src_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString() &&
                            row["Calc_ID"].ToString() == GBL_Curr_FMM_Calc_ID.ToString() &&
                            row["Cell_ID"].ToString() == GBL_Curr_FMM_Src_Cell_ID.ToString());

            // If not duplicate, add the new Source Cell Config to the target DataTable
            if (!isDuplicate)
            {
                DataRow newTargetRow = tgt_Src_Cell_Config_DT.NewRow();

                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = GBL_Curr_FMM_Act_ID;
                newTargetRow["Model_ID"] = GBL_Curr_FMM_Models_ID;
                newTargetRow["Calc_ID"] = GBL_Curr_FMM_Calc_ID;
                newTargetRow["Cell_ID"] = GBL_FMM_Src_Cell_ID;
                newTargetRow["Calc_Src_ID_Order"] = src_Src_Cell_Config_Row.Field<int?>("Calc_Src_ID_Order") ?? 0;
                newTargetRow["Calc_Src_Type"] = src_Src_Cell_Config_Row.Field<string>("Calc_Src_Type") ?? string.Empty;
                newTargetRow["Calc_Src_Item"] = src_Src_Cell_Config_Row.Field<string>("Calc_Src_Item") ?? string.Empty;
                newTargetRow["Calc_Open_Parens"] = src_Src_Cell_Config_Row.Field<string>("Calc_Open_Parens") ?? string.Empty;
                newTargetRow["Calc_Math_Operator"] = src_Src_Cell_Config_Row.Field<string>("Calc_Math_Operator") ?? string.Empty;
                newTargetRow["Entity"] = src_Src_Cell_Config_Row.Field<string>("Entity") ?? string.Empty;
                newTargetRow["Cons"] = src_Src_Cell_Config_Row.Field<string>("Cons") ?? string.Empty;
                newTargetRow["Scenario"] = src_Src_Cell_Config_Row.Field<string>("Scenario") ?? string.Empty;
                newTargetRow["Time"] = src_Src_Cell_Config_Row.Field<string>("Time") ?? string.Empty;
                newTargetRow["Origin"] = src_Src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                newTargetRow["IC"] = src_Src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                newTargetRow["View"] = src_Src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                newTargetRow["Src_Plan_Units"] = src_Src_Cell_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                newTargetRow["Acct"] = src_Src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                newTargetRow["Flow"] = src_Src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                newTargetRow["UD1"] = src_Src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                newTargetRow["UD2"] = src_Src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                newTargetRow["UD3"] = src_Src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                newTargetRow["UD4"] = src_Src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                newTargetRow["UD5"] = src_Src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                newTargetRow["UD6"] = src_Src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                newTargetRow["UD7"] = src_Src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                newTargetRow["UD8"] = src_Src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                newTargetRow["Calc_Close_Parens"] = src_Src_Cell_Config_Row.Field<string>("Calc_Close_Parens") ?? string.Empty;
                newTargetRow["Unbal_Src_Cell_Buffer"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer") ?? string.Empty;
                newTargetRow["Unbal_Origin_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                newTargetRow["Unbal_IC_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                newTargetRow["Unbal_Acct_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                newTargetRow["Unbal_Flow_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                newTargetRow["Unbal_UD1_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                newTargetRow["Unbal_UD2_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                newTargetRow["Unbal_UD3_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                newTargetRow["Unbal_UD4_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                newTargetRow["Unbal_UD5_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                newTargetRow["Unbal_UD6_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                newTargetRow["Unbal_UD7_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                newTargetRow["Unbal_UD8_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                newTargetRow["Unbal_Src_Cell_Buffer_Filter"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer_Filter") ?? string.Empty;
                newTargetRow["OS_Dynamic_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("OS_Dynamic_Calc_Script") ?? string.Empty;
                newTargetRow["Override_Value"] = src_Src_Cell_Config_Row.Field<string>("Override_Value") ?? string.Empty;
                newTargetRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                newTargetRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                newTargetRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                newTargetRow["Map_Type"] = src_Src_Cell_Config_Row.Field<string>("Map_Type") ?? string.Empty;
                newTargetRow["Map_Source"] = src_Src_Cell_Config_Row.Field<string>("Map_Source") ?? string.Empty;
                newTargetRow["Map_Logic"] = src_Src_Cell_Config_Row.Field<string>("Map_Logic") ?? string.Empty;
                newTargetRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                newTargetRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                newTargetRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // Set appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_Src_Cell_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Src_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == GBL_Curr_FMM_Act_ID.ToString() &&
                                           row["Model_ID"].ToString() == GBL_Curr_FMM_Models_ID.ToString() &&
                                           row["Calc_ID"].ToString() == GBL_Curr_FMM_Calc_ID.ToString() &&
                                           row["Cell_ID"].ToString() == GBL_Curr_FMM_Src_Cell_ID.ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Src_Cell_ID = Convert.ToInt32(existingRow["Cell_ID"].ToString());
                    // Update fields as necessary
                    existingRow["Calc_Src_ID_Order"] = src_Src_Cell_Config_Row.Field<int?>("Calc_Src_ID_Order") ?? 0;
                    existingRow["Calc_Src_Type"] = src_Src_Cell_Config_Row.Field<string>("Calc_Src_Type") ?? string.Empty;
                    existingRow["Calc_Src_Item"] = src_Src_Cell_Config_Row.Field<string>("Calc_Src_Item") ?? string.Empty;
                    existingRow["Calc_Open_Parens"] = src_Src_Cell_Config_Row.Field<string>("Calc_Open_Parens") ?? string.Empty;
                    existingRow["Calc_Math_Operator"] = src_Src_Cell_Config_Row.Field<string>("Calc_Math_Operator") ?? string.Empty;
                    existingRow["Entity"] = src_Src_Cell_Config_Row.Field<string>("Entity") ?? string.Empty;
                    existingRow["Cons"] = src_Src_Cell_Config_Row.Field<string>("Cons") ?? string.Empty;
                    existingRow["Scenario"] = src_Src_Cell_Config_Row.Field<string>("Scenario") ?? string.Empty;
                    existingRow["Time"] = src_Src_Cell_Config_Row.Field<string>("Time") ?? string.Empty;
                    existingRow["Origin"] = src_Src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existingRow["IC"] = src_Src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                    existingRow["View"] = src_Src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                    existingRow["Src_Plan_Units"] = src_Src_Cell_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                    existingRow["Acct"] = src_Src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existingRow["Flow"] = src_Src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existingRow["UD1"] = src_Src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existingRow["UD2"] = src_Src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existingRow["UD3"] = src_Src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existingRow["UD4"] = src_Src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existingRow["UD5"] = src_Src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existingRow["UD6"] = src_Src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existingRow["UD7"] = src_Src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existingRow["UD8"] = src_Src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existingRow["Calc_Close_Parens"] = src_Src_Cell_Config_Row.Field<string>("Calc_Close_Parens") ?? string.Empty;
                    existingRow["Unbal_Src_Cell_Buffer"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer") ?? string.Empty;
                    existingRow["Unbal_Origin_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                    existingRow["Unbal_IC_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                    existingRow["Unbal_Acct_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                    existingRow["Unbal_Flow_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                    existingRow["Unbal_UD1_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                    existingRow["Unbal_UD2_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                    existingRow["Unbal_UD3_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                    existingRow["Unbal_UD4_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                    existingRow["Unbal_UD5_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                    existingRow["Unbal_UD6_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                    existingRow["Unbal_UD7_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                    existingRow["Unbal_UD8_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                    existingRow["Unbal_Src_Cell_Buffer_Filter"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer_Filter") ?? string.Empty;
                    existingRow["OS_Dynamic_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("OS_Dynamic_Calc_Script") ?? string.Empty;
                    existingRow["Override_Value"] = src_Src_Cell_Config_Row.Field<string>("Override_Value") ?? string.Empty;
                    existingRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                    existingRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                    existingRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                    existingRow["Map_Type"] = src_Src_Cell_Config_Row.Field<string>("Map_Type") ?? string.Empty;
                    existingRow["Map_Source"] = src_Src_Cell_Config_Row.Field<string>("Map_Source") ?? string.Empty;
                    existingRow["Map_Logic"] = src_Src_Cell_Config_Row.Field<string>("Map_Logic") ?? string.Empty;
                    existingRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                    existingRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                    existingRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Model_Grps(DataRow src_Model_Grp_Config_Row, ref DataTable tgt_Model_Grp_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID model group config for duplicate Group_Name
            bool isDuplicate = tgt_Model_Grp_Config_DT.AsEnumerable()
                .Any(row => row["Group_Name"].ToString() == src_Model_Grp_Config_Row["Group_Name"].ToString() &&
                            row["Group_Type"].ToString() == src_Model_Grp_Config_Row["Group_Type"].ToString());

            // If not duplicate, add the new Model Group Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Model_Grps_ID += 1;
                GBL_Curr_FMM_Model_Grps_ID = GBL_FMM_Model_Grps_ID;
                DataRow newTargetRow = tgt_Model_Grp_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Model_Grp_Config_ID"] = GBL_FMM_Model_Grps_ID;
                newTargetRow["Group_Name"] = src_Model_Grp_Config_Row["Group_Name"].ToString();
                newTargetRow["Group_Type"] = src_Model_Grp_Config_Row["Group_Type"].ToString();
                newTargetRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Grp_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Group_Name"].ToString() == src_Model_Grp_Config_Row["Group_Name"].ToString() &&
                                           row["Group_Type"].ToString() == src_Model_Grp_Config_Row["Group_Type"].ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Model_Grps_ID = Convert.ToInt32(existingRow["Model_Grp_Config_ID"].ToString());
                    existingRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Model_Grp_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Model_Grp_Assign_Model(DataRow src_Model_Grp_Assign_Model_Row, ref DataTable tgt_Model_Grp_Assign_Model_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID model group assignment config for duplicate Group_Name and Model_Name
            bool isDuplicate = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                .Any(row => row["Group_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Group_Name"].ToString() &&
                            row["Model_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Model_Name"].ToString());

            // If not duplicate, add the new Model Group Assign Model Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Model_Grp_Assign_ID += 1;
                GBL_Curr_FMM_Model_Grp_Assign_ID = GBL_FMM_Model_Grp_Assign_ID;
                DataRow newTargetRow = tgt_Model_Grp_Assign_Model_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Model_Grp_Assign_ID"] = GBL_FMM_Model_Grp_Assign_ID;
                newTargetRow["Group_Name"] = src_Model_Grp_Assign_Model_Row["Group_Name"].ToString();
                newTargetRow["Model_Name"] = src_Model_Grp_Assign_Model_Row["Model_Name"].ToString();
                newTargetRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Assign_Model_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Group_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Group_Name"].ToString() &&
                                           row["Model_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Model_Name"].ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Model_Grp_Assign_ID = Convert.ToInt32(existingRow["Model_Grp_Assign_ID"].ToString());
                    existingRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Model_Grp_Assign_Model_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calc_Unit_Config(DataRow src_Calc_Unit_Config_Row, ref DataTable tgt_Calc_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID workflow DU config for duplicate DU_Name
            bool isDuplicate = tgt_Calc_Unit_Config_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Config_Row["DU_Name"].ToString() &&
                            row["DU_Type"].ToString() == src_Calc_Unit_Config_Row["DU_Type"].ToString());

            // If not duplicate, add the new Workflow DU Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Calc_Unit_ID += 1;
                GBL_Curr_FMM_Calc_Unit_ID = GBL_FMM_Calc_Unit_ID;
                DataRow newTargetRow = tgt_Calc_Unit_Config_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Calc_Unit_ID"] = GBL_FMM_Calc_Unit_ID;
                newTargetRow["DU_Name"] = src_Calc_Unit_Config_Row["DU_Name"].ToString();
                newTargetRow["DU_Type"] = src_Calc_Unit_Config_Row["DU_Type"].ToString();
                newTargetRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Unit_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Calc_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Config_Row["DU_Name"].ToString() &&
                                           row["DU_Type"].ToString() == src_Calc_Unit_Config_Row["DU_Type"].ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Calc_Unit_ID = Convert.ToInt32(existingRow["Calc_Unit_ID"].ToString());
                    existingRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Calc_Unit_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calc_Unit_Assign(DataRow src_Calc_Unit_Assign_Row, ref DataTable tgt_Calc_Unit_Assign_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID for duplicate DU_Name and Model_Grp_Name in the assignment config
            bool isDuplicate = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                            row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

            // If not duplicate, add the new Workflow DU Assign Model Group Config to the target DataTable
            if (!isDuplicate)
            {
                GBL_FMM_Calc_Unit_Assign_ID += 1;
                GBL_Curr_FMM_Calc_Unit_Assign_ID = GBL_FMM_Calc_Unit_Assign_ID;
                DataRow newTargetRow = tgt_Calc_Unit_Assign_DT.NewRow();
                newTargetRow["Cube_ID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Calc_Unit_Assign_ID"] = GBL_FMM_Calc_Unit_Assign_ID;
                newTargetRow["DU_Name"] = src_Calc_Unit_Assign_Row["DU_Name"].ToString();
                newTargetRow["Model_Grp_Name"] = src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString();
                newTargetRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["Create_Date"] = DateTime.Now;
                newTargetRow["Create_User"] = si.UserName; // or appropriate user context
                newTargetRow["Update_Date"] = DateTime.Now;
                newTargetRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Unit_Assign_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                                           row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

                if (existingRow != null)
                {
                    GBL_Curr_FMM_Calc_Unit_Assign_ID = Convert.ToInt32(existingRow["Calc_Unit_Assign_ID"].ToString());
                    existingRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Calc_Unit_Assign_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["Update_Date"] = DateTime.Now;
                    existingRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        #endregion

        #region "Helper Functions"
        private string Construct_Where_Clause(SqlParameter[] sqlparams)
        {
            if (sqlparams == null || sqlparams.Length == 0)
            {
                return string.Empty;
            }

            var whereClause = new List<string>();

            foreach (var param in sqlparams)
            {
                // Clean parameter name by removing the leading '@' if present
                var columnName = param.ParameterName.TrimStart('@');

                if (param.Value == DBNull.Value || param.Value == null)
                {
                    // Handle NULL values in SQL with IS NULL
                    whereClause.Add($"{columnName} IS NULL");
                }
                else
                {
                    // Different handling for NVARCHAR to support IN clauses
                    if (param.SqlDbType == SqlDbType.NVarChar && param.Value is string stringValue)
                    {
                        if (stringValue.Contains(","))
                        {
                            // Handle IN clause by splitting the comma-separated values
                            var inClauseValues = string.Join(",", stringValue.Split(',')
                                                            .Select(value => $"'{value.Trim()}'"));
                            whereClause.Add($"{columnName} IN ({inClauseValues})");
                        }
                        else
                        {
                            whereClause.Add($"{columnName} = @{columnName}");
                        }
                    }
                    else
                    {
                        // Default equality clause for other types
                        whereClause.Add($"{columnName} = @{columnName}");
                    }
                }
            }

            return "WHERE " + string.Join(" AND ", whereClause);
        }

        #endregion
        #endregion
        #region "Check Duplicates"

        #region "Duplicate Models"
        private void Duplicate_Model_List(int Cube_ID, int Act_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Models_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Models
		                        WHERE Cube_ID = @Cube_ID
								AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID},
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Models_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Model_List_Dict and handle errors
                            foreach (DataRow cube_Model_Row in FMM_Models_DT.Rows)
                            {
                                string ModelKey = Cube_ID + "|" + Act_ID + "|" + (int)cube_Model_Row["Model_ID"];
                                GBL_Models_Dict.Add(ModelKey, (string)cube_Model_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Model_Grp_List(int Cube_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Task_Result)
        {
            try
            {
                switch (Dup_Process_Step)
                {
                    case "Initiate":
                        // Select rows from the table before any updates to rows are processed
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var SQL_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sqa = new SqlDataAdapter();
                            var FMM_Model_Grps_DT = new DataTable();
                            // Define the select query and parameters
                            string sql = @"
		                        SELECT *
		                        FROM FMM_Model_Grps
		                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                SQL_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa,FMM_Model_Grps_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Task_Result.IsOK = false;
                                save_Task_Result.ShowMessageBox = true;
                                save_Task_Result.Message = $"Error fetching data for Cube_ID {Cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Model_Grp_List_Dict and handle errors
                            foreach (DataRow FMM_Model_Grp_Row in FMM_Model_Grps_DT.Rows)
                            {
                                string Model_GroupKey = Cube_ID + "|" + (int)FMM_Model_Grp_Row["Model_Grp_ID"];
                                GBL_Model_Grps_Dict.Add(Model_GroupKey, (string)FMM_Model_Grp_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Task_Result
                save_Task_Result.IsOK = false;
                save_Task_Result.ShowMessageBox = true;
                save_Task_Result.Message = $"An unexpected error occurred.";
            }
        }

        #endregion
        #region "Duplicate Approval Steps"

        #endregion
        #endregion


        #endregion
    }
}