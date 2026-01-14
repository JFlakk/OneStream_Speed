
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Config_Data
{
    public class MainClass
    {
        #region "Global Variables"
        public int gbl_Calc_ID { get; set; }
        public string gbl_Bal_Calc { get; set; } = string.Empty;
        public string gbl_Bal_Buffer_Calc { get; set; } = string.Empty;
        public string gbl_Unbal_Buffer_Calc { get; set; } = string.Empty;
        public string gbl_Unbal_Calc { get; set; } = string.Empty;
        public string gbl_Table_Calc_Logic { get; set; } = string.Empty;
        public int gbl_Table_SrcCell { get; set; }
        public int gbl_Src_Cell_Cnt { get; set; }
        public string gbl_Model_Type { get; set; } = "Cube";
        public Dictionary<int, string> gbl_SrcCell_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_Unbal_Calc_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_SrcCell_Drill_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, string> gbl_Act_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Calc_Unit_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Unit_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Acct_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Appr_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Appr_Step_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Register_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Col_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Calc_Dict { get; set; } = new Dictionary<string, string>();
        public bool gbl_Duplicate_Activity { get; set; } = false;
        public bool gbl_Duplicate_Calc_Unit_Config { get; set; } = false;
        public bool gbl_Duplicate_Unit_Config { get; set; } = false;
        public bool gbl_Duplicate_Acct_Config { get; set; } = false;
        public bool gbl_Duplicate_Approval { get; set; } = false;
        public bool gbl_Duplicate_Appr_Step { get; set; } = false;
        public bool gbl_Duplicate_Reg_Config { get; set; } = false;
        public bool gbl_Duplicate_Col_Config { get; set; } = false;
        public bool gbl_Duplicate_Calc_Config { get; set; } = false;
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        public int gbl_FMM_Act_ID { get; set; }
        public int gbl_Curr_FMM_Act_ID { get; set; }
        public int gbl_FMM_Unit_ID { get; set; }
        public int gbl_Curr_FMM_Unit_ID { get; set; }
        public int gbl_FMM_Acct_ID { get; set; }
        public int gbl_Curr_FMM_Acct_ID { get; set; }
        public int gbl_FMM_Appr_ID { get; set; }
        public int gbl_Curr_FMM_Appr_ID { get; set; }
        public int gbl_FMM_Appr_Step_ID { get; set; }
        public int gbl_Curr_FMM_Appr_Step_ID { get; set; }
        public int gbl_FMM_Reg_Config_ID { get; set; }
        public int gbl_Curr_FMM_Reg_Config_ID { get; set; }
        public int gbl_FMM_Col_ID { get; set; }
        public int gbl_Curr_FMM_Col_ID { get; set; }
        public int gbl_FMM_Models_ID { get; set; }
        public int gbl_Curr_FMM_Models_ID { get; set; }
        public int gbl_FMM_Calc_ID { get; set; }
        public int gbl_Curr_FMM_Calc_ID { get; set; }
        public int gbl_FMM_Dest_Cell_ID { get; set; }
        public int gbl_Curr_FMM_Dest_Cell_ID { get; set; }
        public int gbl_FMM_Src_Cell_ID { get; set; }
        public int gbl_Curr_FMM_Src_Cell_ID { get; set; }
        public int gbl_FMM_Model_Grps_ID { get; set; }
        public int gbl_Curr_FMM_Model_Grps_ID { get; set; }
        public int gbl_FMM_Model_Grp_Assign_ID { get; set; }
        public int gbl_Curr_FMM_Model_Grp_Assign_ID { get; set; }
        public int gbl_FMM_Calc_Unit_ID { get; set; }
        public int gbl_Curr_FMM_Calc_Unit_ID { get; set; }
        public int gbl_FMM_Calc_Unit_Assign_ID { get; set; }
        public int gbl_Curr_FMM_Calc_Unit_Assign_ID { get; set; }
        public Dictionary<string, string> gbl_Models_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Model_Grps_Dict { get; set; } = new Dictionary<string, string>();
        public bool gbl_Duplicate_Models { get; set; } = false;
        public bool gbl_Duplicate_Model_Grps { get; set; } = false;
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
                        var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                        switch (args.FunctionName)
                        {
                            case var fn when fn.XFEqualsIgnoreCase("Save_Act_Config"):
                                save_Result = Save_Act_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Calc_Unit_Config"):
                                save_Result = Save_Calc_Unit_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Unit_Config"):
                                save_Result = Save_Unit_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Acct_Config"):
                                save_Result = Save_Acct_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Appr_Config"):
                                save_Result = Save_Appr_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Appr_Step_Config"):
                                save_Result = Save_Appr_Step_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Reg_Config"):
                                save_Result = Save_Reg_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Col_Config"):
                                save_Result = Save_Col_Config();
                                return save_Result;
                        }
                        break;

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var changed_Result = new XFSelectionChangedTaskResult();
                        switch (args.FunctionName)
                        {
                            case var fn when fn.XFEqualsIgnoreCase("Save_Calc_Config_Rows"):
                                gbl_Model_Type = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type");
                                changed_Result = Save_Calc_Config_Rows();
                                if (gbl_Model_Type == "Cube")
                                {
                                    Evaluate_Calc_Config_Setup(gbl_Calc_ID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("save_Dest_Cell_Rows"):
                                gbl_Model_Type = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type");
                                changed_Result = Save_Dest_Cell_Rows();
                                if (gbl_Model_Type == "Cube")
                                {
                                    Evaluate_Calc_Config_Setup(gbl_Calc_ID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Src_Cell_Rows"):
                                gbl_Model_Type = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type");
                                changed_Result = Save_Src_Cell_Rows();
                                if (gbl_Model_Type == "Cube")
                                {
                                    Evaluate_Calc_Config_Setup(gbl_Calc_ID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Cube_Config"):
                                changed_Result = Save_Cube_Config("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Updated_Cube_Config"):
                                changed_Result = Save_Cube_Config("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model"):
                                changed_Result = Save_Model("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Model_Updates"):
                                changed_Result = Save_Model("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model_Group"):
                                changed_Result = Save_Model_Grp("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Updated_Model_Group"):
                                changed_Result = Save_Model_Grp("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model_Grp_Seq"):
                                changed_Result = Save_Model_Grp_Seq("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Updated_Model_Grp_Seq"):
                                changed_Result = Save_Model_Grp_Seq("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model_Grp_Model_Assignments"):
                                changed_Result = Save_Model_Grp_Assign("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Calc_Unit_Assign"):
                                changed_Result = Save_Calc_Unit_Assign("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Appr_Step"):
                                changed_Result = Save_Appr_Step("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Act_Appr_Step_Config"):
                                var runType = args.NameValuePairs.XFGetValue("runType");
                                changed_Result = Save_Act_Appr_Step_Config(runType);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Bulk_Calc_Unit"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Copy_Cube_Config"):
                                Process_Copy_Cube_Config(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Copy_Reg_Config"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Copy_Act_Config"):
                                Process_Copy_Act_Config(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Model_Copy"):
                                BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Calc_Copy"):
                                BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                                Process_Calc_Copy(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Copy_Model_Grp_Config"):
                                BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
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

        private XFSelectionChangedTaskResult Save_Calc_Config_Rows()
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var save_Task_Info = args.SelectionChangedTaskInfo;

                var createNewDestCell = false;
                var cube_ID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                var act_ID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                var model_ID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Model_ID", "0").XFConvertToInt();
                var calcListVal = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CalcList", "0").XFConvertToInt();
                var saveType = "New";
                var calc_ID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Calc_ID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();

                    var FMM_Calc_Config_DT = new DataTable();
                    var FMM_Dest_Cell_DT = new DataTable();
                    Duplicate_Calc_Config(cube_ID, act_ID, model_ID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_Calc_Config 
                                WHERE Cube_ID = @Cube_ID 
                                AND Act_ID = @Act_ID 
                                AND Model_ID = @Model_ID";

                    var sqlparams = new[]
                    {
                    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                    new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                    new SqlParameter("@Model_ID", SqlDbType.Int) { Value = model_ID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);
                    FMM_Calc_Config_DT.PrimaryKey = new DataColumn[] { FMM_Calc_Config_DT.Columns["Calc_ID"]! };

                    if (saveType == "New")
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);

                        calc_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "Calc_ID");
                        gbl_Calc_ID = calc_ID;

                        var new_config_Row = FMM_Calc_Config_DT.NewRow();

                        new_config_Row["Calc_ID"] = calc_ID;
                        new_config_Row["Status"] = "Build";
                        new_config_Row["Create_Date"] = DateTime.Now;
                        new_config_Row["Create_User"] = si.UserName;
                        new_config_Row["Update_Date"] = DateTime.Now;
                        new_config_Row["Update_User"] = si.UserName;

                        FMM_Calc_Config_DT.Rows.Add(new_config_Row);
                        Duplicate_Calc_Config(cube_ID, act_ID, model_ID, "Update Row", ref save_Result, "Insert");

                        var dest_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Dest_Cell", "Dest_Cell_ID");

                        sql = @"SELECT * 
                            FROM FMM_Dest_Cell 
                            WHERE Calc_ID = @Calc_ID 
                            AND Dest_Cell_ID = @Dest_Cell_ID";

                        sqlparams = new[]
                        {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = calc_ID },
                            new SqlParameter("@Dest_Cell_ID", SqlDbType.Int) { Value = dest_Cell_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Dest_Cell_DT, sql, sqlparams);

                        var new_Row = FMM_Dest_Cell_DT.NewRow();
                        new_Row["Cube_ID"] = cube_ID;
                        new_Row["Act_ID"] = act_ID;
                        new_Row["Model_ID"] = model_ID;
                        new_Row["Calc_ID"] = calc_ID;
                        new_Row["Dest_Cell_ID"] = dest_Cell_ID;

                        FMM_Dest_Cell_DT.Rows.Add(new_Row);
                    }
                    else
                    {
                        calc_ID = calc_ID == 0
                            ? save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CalcList", "0").XFConvertToInt()
                            : calc_ID;
                        var rowsToUpdate = FMM_Calc_Config_DT.Select($"Calc_ID = {calc_ID}");
                        if (rowsToUpdate.Length > 0)
                        {
                            var rowToUpdate = rowsToUpdate[0];
                            gbl_Calc_ID = rowToUpdate["Calc_ID"] != DBNull.Value ? Convert.ToInt32(rowToUpdate["Calc_ID"]) : 0;

                            rowToUpdate["Update_Date"] = DateTime.Now;
                            rowToUpdate["Update_User"] = si.UserName;
                            Duplicate_Calc_Config(cube_ID, act_ID, model_ID, "Update Row", ref save_Result, "Update");
                        }
                    }

                    var dup_Calc_Config = gbl_Calc_Dict
                    .GroupBy(x => x.Value)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                    gbl_Duplicate_Calc_Config = dup_Calc_Config.Count > 0;

                    if (gbl_Duplicate_Calc_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;

                        var calcTypeValue = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type", "0").XFConvertToInt();
                        var rowToMap = FMM_Calc_Config_DT.Rows.Find(calc_ID);
                        if (rowToMap != null)
                        {
                            var calcType = (FMM_Config_Helpers.CalcType)calcTypeValue;
                            if (FMM_Config_Helpers.CalcRegistry.Configs.TryGetValue(calcType, out var calcConfig))
                            {
                                foreach (var group in calcConfig.ParameterMappings.Values)
                                {
                                    foreach (var mapping in group)
                                    {
                                        var sourceSubstVar = mapping.Key;
                                        var targetCol = mapping.Value;
                                        rowToMap[targetCol] = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue(sourceSubstVar, string.Empty);
                                    }
                                }
                            }
                        }
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", FMM_Calc_Config_DT, sqa, "Calc_ID");

                        if (createNewDestCell)
                        {
                            cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                        }
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult Save_Dest_Cell_Rows()
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;

                gbl_Calc_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Calc_ID", "0").XFConvertToInt();
                var cube_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                var act_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                var model_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Model_ID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmDestCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_Dest_Cell 
                    WHERE Calc_ID = @Calc_ID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID }
                };

                    cmdBuilder.FillDataTable(si, sqa, fmmDestCellDt, sql, sqlparams);
                    fmmDestCellDt.PrimaryKey = new[] { fmmDestCellDt.Columns["Dest_Cell_ID"]! };

                    var destRow = fmmDestCellDt.Rows.Cast<DataRow>().FirstOrDefault();
                    if (destRow == null)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message = "Destination cell record not found for update.";
                        return saveResult;
                    }

                    destRow["Update_Date"] = DateTime.Now;
                    destRow["Update_User"] = si.UserName;

                    cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", fmmDestCellDt, sqa, "Dest_Cell_ID");
                }

                saveResult.IsOK = true;
                saveResult.ShowMessageBox = false;
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private XFSelectionChangedTaskResult Save_Src_Cell_Rows()
        {
            try
            {
                var saveResult = new XFSelectionChangedTaskResult();
                var saveTaskInfo = args.SelectionChangedTaskInfo;

                gbl_Calc_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Calc_ID", "0").XFConvertToInt();
                var cube_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                var act_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                var model_ID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Model_ID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmSrcCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_Src_Cell 
                    WHERE Calc_ID = @Calc_ID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID }
                };

                    cmdBuilder.FillDataTable(si, sqa, fmmSrcCellDt, sql, sqlparams);
                    fmmSrcCellDt.PrimaryKey = new[] { fmmSrcCellDt.Columns["Src_Cell_ID"]! };

                    if (fmmSrcCellDt.Rows.Count == 0)
                    {
                        saveResult.IsOK = false;
                        saveResult.ShowMessageBox = true;
                        saveResult.Message = "Source cell records not found for update.";
                        return saveResult;
                    }

                    foreach (DataRow row in fmmSrcCellDt.Rows)
                    {
                        row["Update_Date"] = DateTime.Now;
                        row["Update_User"] = si.UserName;
                    }

                    cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", fmmSrcCellDt, sqa, "Src_Cell_ID");
                }

                saveResult.IsOK = true;
                saveResult.ShowMessageBox = false;
                return saveResult;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Act_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var act_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Act_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    Duplicate_Act_Config(cube_ID, "Initiate", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    var sql = @"SELECT * 
                                FROM FMM_Act_Config
                                WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Act_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            {
                                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                                act_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Config", "Act_ID");

                                var new_DataRow = FMM_Act_Config_DT.NewRow();
                                new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                                new_DataRow["Act_ID"] = act_ID;
                                new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                                new_DataRow["Calc_Type"] = (string)xfRow.ModifiedDataRow.Items["Calc_Type"];
                                new_DataRow["Status"] = "Build";
                                new_DataRow["Create_Date"] = DateTime.Now;
                                new_DataRow["Create_User"] = si.UserName;
                                new_DataRow["Update_Date"] = DateTime.Now;
                                new_DataRow["Update_User"] = si.UserName;
                                FMM_Act_Config_DT.Rows.Add(new_DataRow);
                                Duplicate_Act_Config(cube_ID, "Update Row", ref save_Result, "Insert", xfRow);
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
                                Duplicate_Act_Config(cube_ID, "Update Row", ref save_Result, "Update", xfRow);
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
                                    Duplicate_Act_Config(cube_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Activity_Config = gbl_Act_Config_Dict
                                             .GroupBy(x => x.Value)
                                             .Where(g => g.Count() > 1)
                                             .Select(g => g.Key)
                                             .ToList();

                    gbl_Duplicate_Activity = dup_Activity_Config.Count > 0;

                    if (gbl_Duplicate_Activity)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Activity Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Act_Config", FMM_Act_Config_DT, sqa, "Act_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Calc_Unit_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var calc_Unit_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Calc_Unit_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    Duplicate_Calc_Unit_Config(cube_ID, "Initiate", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    var sql = @"SELECT * 
                                FROM FMM_Calc_Unit_Config
                                WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Unit_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            calc_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");

                            var new_DataRow = FMM_Calc_Unit_Config_DT.NewRow();
                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Calc_Unit_ID"] = calc_Unit_ID;
                            new_DataRow["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                            new_DataRow["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            FMM_Calc_Unit_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Calc_Unit_Config(cube_ID, "Update Row", ref save_Result, "Insert", xfRow);
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
                                Duplicate_Calc_Unit_Config(cube_ID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Calc_Unit_Config_DT.Select($"Calc_Unit_ID = {(int)xfRow.OriginalDataRow["Calc_Unit_ID"]}");

                            if (rowsToDelete.Length > 0)
                            {
                                BRApi.ErrorLog.LogMessage(si, $"Hit found Delete {FMM_Calc_Unit_Config_DT.Rows.Count}");
                                foreach (var row in rowsToDelete)
                                {
                                    Duplicate_Calc_Unit_Config(cube_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                    row.Delete();
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Calc_Unit_Config = gbl_Calc_Unit_Config_Dict
                                                 .GroupBy(x => x.Value)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

                    gbl_Duplicate_Calc_Unit_Config = dup_Calc_Unit_Config.Count > 0;

                    if (gbl_Duplicate_Calc_Unit_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate WF DU Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Config", FMM_Calc_Unit_Config_DT, sqa, "Calc_Unit_ID");
                    }
                }

                // Set return value
                //                save_Result.IsOK = true;
                //                save_Result.ShowMessageBox = false;
                //                save_Result.Message = String.Empty;
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Unit & Acct TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult Save_Unit_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var unit_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Unit_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var act_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    Duplicate_Unit_Config(cube_ID, act_ID, "Initiate", ref save_Result);
                    BRApi.ErrorLog.LogMessage(si, "Hit: " + cube_ID + " | " + act_ID);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    var sql = @"SELECT * 
                                FROM FMM_Unit_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Unit_Config_DT, sql, sqlparams);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + cube_ID + " | " + act_ID);
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Unit_Config", "Unit_ID");
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + cube_ID + " | " + unit_ID);
                            var new_DataRow = FMM_Unit_Config_DT.NewRow();
                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            new_DataRow["Unit_ID"] = unit_ID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Status"] = (string)xfRow.ModifiedDataRow.Items["Status"];
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            FMM_Unit_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Unit_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Insert", xfRow);
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
                                Duplicate_Unit_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Update", xfRow);
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
                                    Duplicate_Unit_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Unit_Config = gbl_Unit_Config_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Duplicate_Unit_Config = dup_Unit_Config.Count > 0;

                    if (gbl_Duplicate_Unit_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Unit Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", FMM_Unit_Config_DT, sqa, "Unit_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Acct_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();

                // Save the Calc Config data rows
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                var acct_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Acct_Config_DT = new DataTable();
                    var cube_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var act_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    var unit_ID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Unit_ID", "0").XFConvertToInt();
                    Duplicate_Acct_Config(cube_ID, act_ID, unit_ID, "Initiate", ref save_Result);
                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    var sql = @"SELECT * 
                                FROM FMM_Acct_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Acct_Config_DT, sql, sqlparams);


                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);

                            // Example: Get the max ID for the "FMM_Calc_Config" table
                            acct_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Acct_Config", "Acct_ID");

                            var new_DataRow = FMM_Acct_Config_DT.NewRow();
                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            new_DataRow["Unit_ID"] = (int)xfRow.ModifiedDataRow["Unit_ID"];
                            new_DataRow["Acct_ID"] = acct_ID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Map_Req"] = (bool)xfRow.ModifiedDataRow.Items["Map_Req"];
                            new_DataRow["Map_Type"] = (string)xfRow.ModifiedDataRow.Items["Map_Type"];
                            new_DataRow["Map_Loc"] = (string)xfRow.ModifiedDataRow.Items["Map_Loc"];
                            new_DataRow["Map_Logic"] = (string)xfRow.ModifiedDataRow.Items["Map_Logic"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            // Set other column values for the new row as needed
                            FMM_Acct_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Acct_Config(cube_ID, act_ID, unit_ID, "Update Row", ref save_Result, "Insert", xfRow);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {

                            // Find the row to update and modify its data
                            var rowsToUpdate = FMM_Acct_Config_DT.Select($"Acct_ID = {(int)xfRow.ModifiedDataRow["Acct_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Map_Req"] = (bool)xfRow.ModifiedDataRow["Map_Req"];
                                rowToUpdate["Map_Type"] = (string)xfRow.ModifiedDataRow.Items["Map_Type"];
                                rowToUpdate["Map_Loc"] = (string)xfRow.ModifiedDataRow.Items["Map_Loc"];
                                rowToUpdate["Map_Logic"] = (string)xfRow.ModifiedDataRow["Map_Logic"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Acct_Config(cube_ID, act_ID, unit_ID, "Update Row", ref save_Result, "Update", xfRow);

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
                                    Duplicate_Acct_Config(cube_ID, act_ID, unit_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }

                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Acct_Config = gbl_Acct_Config_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Duplicate_Acct_Config = dup_Acct_Config.Count > 0;

                    if (gbl_Duplicate_Acct_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Acct Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", FMM_Acct_Config_DT, sqa, "Acct_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Register Config TED Inputs"

        private XFSqlTableEditorSaveDataTaskResult Save_Reg_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var new_Reg_List = new List<int>();
                int reg_Config_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Reg_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var act_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    Duplicate_Reg_Config(cube_ID, act_ID, "Initiate", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    var sql = @"SELECT * 
                                FROM FMM_Reg_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Reg_Config_DT, sql, sqlparams);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + cube_ID + " | " + act_ID);
                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            reg_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Config", "Reg_Config_ID");
                            new_Reg_List.Add(reg_Config_ID);
                            var new_DataRow = FMM_Reg_Config_DT.NewRow();
                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Act_ID"] = (int)xfRow.ModifiedDataRow["Act_ID"];
                            new_DataRow["Reg_Config_ID"] = reg_Config_ID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Time_Phase"] = (string)xfRow.ModifiedDataRow.Items["Time_Phase"];
                            new_DataRow["Time_Phase_Driver"] = (string)xfRow.ModifiedDataRow.Items["Time_Phase_Driver"];
                            new_DataRow["Manual_Input_Plan_Units"] = (string)xfRow.ModifiedDataRow.Items["Manual_Input_Plan_Units"];
                            new_DataRow["Start_End_Dt_Src_Obj"] = (string)xfRow.ModifiedDataRow.Items["Start_End_Dt_Src_Obj"];
                            new_DataRow["Start_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["Start_Dt_Src"];
                            new_DataRow["End_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["End_Dt_Src"];
                            new_DataRow["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            FMM_Reg_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Reg_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Reg_Config_DT.Select($"Reg_Config_ID = {(int)xfRow.ModifiedDataRow["Reg_Config_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Time_Phase"] = (string)xfRow.ModifiedDataRow.Items["Time_Phase"];
                                rowToUpdate["Time_Phase_Driver"] = (string)xfRow.ModifiedDataRow.Items["Time_Phase_Driver"];
                                rowToUpdate["Manual_Input_Plan_Units"] = (string)xfRow.ModifiedDataRow.Items["Manual_Input_Plan_Units"];
                                rowToUpdate["Start_End_Dt_Src_Obj"] = (string)xfRow.ModifiedDataRow.Items["Start_End_Dt_Src_Obj"];
                                rowToUpdate["Start_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["Start_Dt_Src"];
                                rowToUpdate["End_Dt_Src"] = (string)xfRow.ModifiedDataRow.Items["End_Dt_Src"];
                                rowToUpdate["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Reg_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Update", xfRow);
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
                                    Duplicate_Reg_Config(cube_ID, act_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Reg_Config = gbl_Register_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Duplicate_Reg_Config = dup_Reg_Config.Count > 0;

                    if (gbl_Duplicate_Reg_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Register Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", FMM_Reg_Config_DT, sqa, "Reg_Config_ID");
                    }
                    foreach (var reg_config_ID in new_Reg_List)
                    {
                        // Call the function and pass the current integer
                        Insert_Col_Default_Rows(cube_ID, act_ID, reg_config_ID);
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_Col_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Col_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var act_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0").XFConvertToInt();
                    var reg_Config_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Reg_Config_ID", "0").XFConvertToInt();
                    Duplicate_Col_Config(cube_ID, act_ID, reg_Config_ID, "Initiate", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Act_Config
                    var sql = @"SELECT * 
                                FROM FMM_Col_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID
                                AND Reg_Config_ID = @Reg_Config_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                        new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = reg_Config_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, sql, sqlparams);

                    BRApi.ErrorLog.LogMessage(si, "Hit: " + cube_ID + " | " + act_ID);
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
                                Duplicate_Col_Config(cube_ID, act_ID, reg_Config_ID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                    }

                    // Check for duplicates in the dictionary
                    var dup_Col_Config = gbl_Col_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Duplicate_Col_Config = dup_Col_Config.Count > 0;

                    if (gbl_Duplicate_Col_Config)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Col Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Act_Config table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa, "Col_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Activity Approval Inputs"
        private XFSqlTableEditorSaveDataTaskResult Save_Appr_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var appr_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Appr_Config_DT = new DataTable();
                    var cube_ID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    BRApi.ErrorLog.LogMessage(si, "Hit" + cube_ID);
                    Duplicate_Config(cube_ID, "Initiate", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    string sql = @"SELECT * 
                                    FROM FMM_Appr_Config
                                    WHERE Cube_ID = @Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Appr_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        // Logic applied to new record inserts
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            appr_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Config", "Appr_ID");

                            var new_DataRow = FMM_Appr_Config_DT.NewRow();
                            BRApi.ErrorLog.LogMessage(si, "Hit" + (int)xfRow.ModifiedDataRow["Cube_ID"]);
                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Appr_ID"] = appr_ID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            FMM_Appr_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Config(cube_ID, "Update Row", ref save_Result, "Insert", xfRow);

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
                                Duplicate_Config(cube_ID, "Update Row", ref save_Result, "Update", xfRow);
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
                                    Duplicate_Config(cube_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Approvals = gbl_Appr_Dict
                                        .GroupBy(x => x.Value)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

                    gbl_Duplicate_Approval = dup_Approvals.Count > 0;

                    if (gbl_Duplicate_Approval)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Activity Approval entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Config", FMM_Appr_Config_DT, sqa, "Appr_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private XFSqlTableEditorSaveDataTaskResult Save_Appr_Step_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var customSubstVars = save_Task_Info.CustomSubstVars;
                var appr_Step_ID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Appr_Step_Config_DT = new DataTable();
                    var cube_ID = customSubstVars.XFGetValue("IV_FMM_Cube_ID", "0").XFConvertToInt();
                    var appr_ID = customSubstVars.XFGetValue("IV_FMM_Appr_ID", "0").XFConvertToInt();
                    Duplicate_Appr_Step_Config(cube_ID, appr_ID, "Update Row", ref save_Result);

                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    string sql = @"SELECT * 
                                    FROM FMM_Appr_Step_Config
                                    WHERE Cube_ID = @Cube_ID
                                    AND Appr_ID = @Appr_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);

                    // Loops through each row in the table editor that was added or updated prior to hitting save
                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            // Create a new row and populate its fields
                            var new_DataRow = FMM_Appr_Step_Config_DT.NewRow();

                            new_DataRow["Cube_ID"] = (int)xfRow.ModifiedDataRow["Cube_ID"];
                            new_DataRow["Appr_ID"] = (int)xfRow.ModifiedDataRow["Appr_ID"];
                            new_DataRow["Appr_Step_ID"] = (int)xfRow.ModifiedDataRow["Appr_Step_ID"];
                            new_DataRow["WFProfile_Step"] = (string)xfRow.ModifiedDataRow["WFProfile_Step"];
                            new_DataRow["Step_Num"] = (int)xfRow.ModifiedDataRow["Step_Num"];
                            new_DataRow["User_Group"] = (string)xfRow.ModifiedDataRow["User_Group"];
                            new_DataRow["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                            new_DataRow["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                            new_DataRow["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                            new_DataRow["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                            new_DataRow["Init_Status"] = (string)xfRow.ModifiedDataRow["Init_Status"];
                            new_DataRow["Appr_Status"] = (string)xfRow.ModifiedDataRow["Appr_Status"];
                            new_DataRow["Rej_Status"] = (string)xfRow.ModifiedDataRow["Rej_Status"];
                            new_DataRow["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;

                            // Add the row to the DataTable
                            FMM_Appr_Step_Config_DT.Rows.Add(new_DataRow);

                            // Check for duplicates before saving
                            Duplicate_Appr_Step_Config(cube_ID, appr_ID, "Insert Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Appr_Step_Config_DT.Select($"Appr_Step_ID = {(int)xfRow.ModifiedDataRow["Appr_Step_ID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Step_Num"] = (int)xfRow.ModifiedDataRow["Step_Num"];
                                rowToUpdate["User_Group"] = (string)xfRow.ModifiedDataRow["User_Group"];
                                rowToUpdate["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                                rowToUpdate["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                                rowToUpdate["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                                rowToUpdate["Appr_Config"] = (int)xfRow.ModifiedDataRow["Appr_Config"];
                                rowToUpdate["Init_Status"] = (string)xfRow.ModifiedDataRow["Init_Status"];
                                rowToUpdate["Appr_Status"] = (string)xfRow.ModifiedDataRow["Appr_Status"];
                                rowToUpdate["Rej_Status"] = (string)xfRow.ModifiedDataRow["Rej_Status"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                                Duplicate_Appr_Step_Config(cube_ID, appr_ID, "Update Row", ref save_Result, "Update", xfRow);
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
                                    Duplicate_Appr_Step_Config(cube_ID, appr_ID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
                    // Check for duplicates in the dictionary
                    var dup_Appr_Steps = gbl_Appr_Step_Dict
                                        .GroupBy(x => x.Value)
                                        .Where(g => g.Count() > 1)
                                        .Select(g => g.Key)
                                        .ToList();

                    gbl_Duplicate_Appr_Step = dup_Appr_Steps.Count > 0;

                    if (gbl_Duplicate_Appr_Step)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Activity Approval entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableComposite(si, "FMM_Appr_Step_Config", FMM_Appr_Step_Config_DT, sqa, "Cube_ID", "Appr_ID", "Appr_Step_ID");
                    }
                }

                // Set return value
                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
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
            var src_Attributes = new Dictionary<string, string>();
            var dest_Attributes = new Dictionary<string, string>();
            var dim_token = new Dictionary<string, List<string>>();
            string src_Cell_Drill_Down = string.Empty;
            int src_Cell_Drill_Down_Count = 0;
            int src_Cell_Count = 0;
            bool balanced_Src_Row = true;
            string unbal_Src_Cell_Buffer_Filter = string.Empty;
            int unbal_Src_Cell_Buffer_FilterCount = 0;

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();
                var FMM_Src_Cell_DT = new DataTable();

                // Fill the DataTable with the current data from FMM_Dest_Cell
                var sql = @"SELECT * 
                            FROM FMM_Src_Cell
                            WHERE Calc_ID = @Calc_ID
                            ORDER BY Src_Cell_ID";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID }
                };

                connection.Open();

                cmdBuilder.FillDataTable(si, sqlDataAdapter, FMM_Src_Cell_DT, sql, parameters);

                string[] all_Dim_Types = { "Entity", "Cons", "Scenario", "Time", "View", "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                #region "All Dim Types"
                foreach (var dim_Type in all_Dim_Types)
                {
                    switch (dim_Type)
                    {
                        case "Entity":
                            dim_token[dim_Type] = new List<string> { "E#" };
                            break;
                        case "Cons":
                            dim_token[dim_Type] = new List<string> { "C#" };
                            break;
                        case "Scenario":
                            dim_token[dim_Type] = new List<string> { "S#" };
                            break;
                        case "Time":
                            dim_token[dim_Type] = new List<string> { "T#" };
                            break;
                        case "View":
                            dim_token[dim_Type] = new List<string> { "V#" };
                            break;
                        case "Acct":
                            dim_token[dim_Type] = new List<string> { "A#" };
                            break;
                        case "IC":
                            dim_token[dim_Type] = new List<string> { "I#", "IC#" };
                            break;
                        case "Origin":
                            dim_token[dim_Type] = new List<string> { "O#" };
                            break;
                        case "Flow":
                            dim_token[dim_Type] = new List<string> { "F#" };
                            break;
                        case "UD1":
                            dim_token[dim_Type] = new List<string> { "UD1#", "U1#" };
                            break;
                        case "UD2":
                            dim_token[dim_Type] = new List<string> { "UD2#", "U2#" };
                            break;
                        case "UD3":
                            dim_token[dim_Type] = new List<string> { "UD3#", "U3#" };
                            break;
                        case "UD4":
                            dim_token[dim_Type] = new List<string> { "UD4#", "U4#" };
                            break;
                        case "UD5":
                            dim_token[dim_Type] = new List<string> { "UD5#", "U5#" };
                            break;
                        case "UD6":
                            dim_token[dim_Type] = new List<string> { "UD6#", "U6#" };
                            break;
                        case "UD7":
                            dim_token[dim_Type] = new List<string> { "UD7#", "U7#" };
                            break;
                        case "UD8":
                            dim_token[dim_Type] = new List<string> { "UD8#", "U8#" };
                            break;
                    }
                }
                #endregion
                string[] Supp_Dim_Types = { "Entity", "Cons", "Scenario", "Time" };
                string[] core_Dim_Types = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                foreach (var FMM_Src_Cell_DT_Row in FMM_Src_Cell_DT.Rows.Cast<DataRow>())
                {
                    src_Cell_Drill_Down = string.Empty;
                    src_Cell_Drill_Down_Count = 0;
                    //If Cube												
                    src_Cell_Count += 1;
                    foreach (var dim_Type in all_Dim_Types)
                    {
                        balances[dim_Type] = 0;
                        src_Attributes[dim_Type] = string.Empty;
                        dest_Attributes[dim_Type] = string.Empty;
                    }
                    foreach (var Supp_Dim_Type in Supp_Dim_Types)
                    {
                        var src_Field = "" + Supp_Dim_Type;
                        var dim_token_1 = string.Empty;
                        var dim_token_2 = string.Empty;
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
                        if (FMM_Src_Cell_DT_Row[src_Field] != DBNull.Value)
                            if (FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                src_Attributes[Supp_Dim_Type] = FMM_Src_Cell_DT_Row[src_Field].ToString();
                                // Assuming srcCellDrillDown and srcCellDrillDownCount are initialized and declared elsewhere												
                                src_Cell_Drill_Down += src_Cell_Drill_Down_Count == 0 ? src_Attributes[Supp_Dim_Type] : ":" + src_Attributes[Supp_Dim_Type];
                                src_Cell_Drill_Down_Count += 1;
                            }
                    }
                    foreach (var core_Dim_Type in core_Dim_Types)
                    {
                        var dest_Field = core_Dim_Type;
                        var src_Field = core_Dim_Type;
                        var filter_Field = $"{core_Dim_Type}_Filter";
                        var balanceKey = core_Dim_Type;
                        var dim_token_1 = string.Empty;
                        var dim_token_2 = string.Empty;
                        List<string> dim_tokens = dim_token[core_Dim_Type];
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
                        if (Data_Row[dest_Field] != DBNull.Value)
                            if (Data_Row[dest_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                Data_Row[dest_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                dest_Attributes[core_Dim_Type] = Data_Row[dest_Field].ToString();
                                balances[core_Dim_Type] = 1;  // Assume resetting or incrementing depends on the logic												
                            }
                            else if (Data_Row[filter_Field] != DBNull.Value)
                                if (Data_Row[filter_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    Data_Row[filter_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    dest_Attributes[core_Dim_Type] = Data_Row[filter_Field].ToString();
                                    balances[core_Dim_Type] = 0;  // Assume resetting or incrementing depends on the logic												
                                }
                        if (FMM_Src_Cell_DT_Row[src_Field] != DBNull.Value)
                            if (FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                src_Attributes[core_Dim_Type] = FMM_Src_Cell_DT_Row[src_Field].ToString();
                                // Assuming srcCellDrillDown and srcCellDrillDownCount are initialized and declared elsewhere												
                                src_Cell_Drill_Down += src_Cell_Drill_Down_Count == 0 ? src_Attributes[core_Dim_Type] : ":" + src_Attributes[core_Dim_Type];
                                src_Cell_Drill_Down_Count += 1;
                                balances[core_Dim_Type] += 1;
                            }
                        if (balances[core_Dim_Type] == 1)
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{core_Dim_Type}_Override"] = "Common";
                            if (src_Cell_Count == 1)
                            {
                                gbl_Bal_Calc = "UnbalAlloc";
                            }
                            else
                            {
                                gbl_Bal_Calc = "Unbalanced";
                            }
                            balanced_Src_Row = false;
                        }
                        else
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{core_Dim_Type}_Override"] = string.Empty;
                            if (Data_Row[$"OS_{core_Dim_Type}_Filter"] != DBNull.Value)
                            {
                                var core_Dim_Type_Filter = Data_Row[$"OS_{core_Dim_Type}_Filter"].ToString();
                                if (core_Dim_Type_Filter.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 || core_Dim_Type_Filter.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    unbal_Src_Cell_Buffer_Filter = "[" + core_Dim_Type_Filter + "]";
                                    unbal_Src_Cell_Buffer_FilterCount++;
                                }
                            }
                        }
                    }

                    if ((string)FMM_Src_Cell_DT_Row["Src_Type"] == "Dynamic Calc")
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit Dynamic");
                        gbl_Bal_Calc = "UnbalAlloc";
                    }
                    gbl_SrcCell_Dict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], (string)FMM_Src_Cell_DT_Row["Open_Parens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["Math_Operator"].ToString().Trim() + " " + src_Cell_Drill_Down + "|" + (string)FMM_Src_Cell_DT_Row["Close_Parens"].ToString().Trim());
                    gbl_SrcCell_Drill_Dict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], src_Cell_Drill_Down);
                    gbl_Unbal_Calc_Dict.Add((int)FMM_Src_Cell_DT_Row["Cell_ID"], (string)FMM_Src_Cell_DT_Row["Open_Parens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["Math_Operator"].ToString().Trim() + " -Calculation- " + "|" + (string)FMM_Src_Cell_DT_Row["Close_Parens"].ToString().Trim());
                    FMM_Src_Cell_DT_Row["Src_Order"] = src_Cell_Count;
                    FMM_Src_Cell_DT_Row["Dyn_Calc_Script"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer_Filter"] = unbal_Src_Cell_Buffer_Filter;
                    BRApi.ErrorLog.LogMessage(si, "dyn calc script: " + src_Cell_Drill_Down);
                }


                //						If Cubedt.Rows.Item(0).Item("ModelType").ToString.XFEqualsIgnoreCase("Table")"												
                //							GBL_BalancedCalc = "DB Model""												
                //						End If"												

                //						gbl_SrcCell_Dict.Add(src_DataRow.Item("OSCalcSrcCellID")"	src_DataRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & src_DataRow.Item("OSCalcMathOperator").ToString.Trim() & " " & SrcCellDrillDown & "|" & src_DataRow.Item("OSCalcCloseParens").ToString.Trim())											
                //						If Cubedt.Rows.Item(0).Item("ModelType").ToString.XFEqualsIgnoreCase("Table")"												
                //							If src_DataRow.Item("OSCalcSrcLocation").ToString.Trim().XFEqualsIgnoreCase("Cube")"												
                //								gbl_Unbal_Calc_Dict.Add(src_DataRow.Item("OSCalcSrcCellID")"	src_DataRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & src_DataRow.Item("OSCalcMathOperator").ToString.Trim() & src_DataRow.Item("OSCalcSrcLocation").ToString.Trim() &  "-Calculation-|" & src_DataRow.Item("OSCalcCloseParens").ToString.Trim())											
                //							Else"												
                //								gbl_Unbal_Calc_Dict.Add(src_DataRow.Item("OSCalcSrcCellID")"	src_DataRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & src_DataRow.Item("OSCalcMathOperator").ToString.Trim() & src_DataRow.Item("OSCalcSrcLocation").ToString.Trim() &  "|" & src_DataRow.Item("OSCalcCloseParens").ToString.Trim())											
                //							End If"												
                //						Else"												
                //							gbl_Unbal_Calc_Dict.Add(src_DataRow.Item("OSCalcSrcCellID")"	src_DataRow.Item("OSCalcOpenParens").ToString.Trim() & "|" & src_DataRow.Item("OSCalcMathOperator").ToString.Trim() & " -Calculation- " &  "|" & src_DataRow.Item("OSCalcCloseParens").ToString.Trim())											
                //						End If"																							

                //						'--------------------------------------------------------------------------------	"																							

                //						dml.AppendLine("UPDATE THG_ModelMgr_SrcCell ")"												
                //						dml.AppendLine("SET UnbalancedSrcCellBuffer = '"  & srccelldrilldown & "'"	unbal_Src_Cell_Buffer_Filter = '" & unbal_Src_Cell_Buffer_Filter & "' ")											
                //						dml.AppendLine("WHERE OSCalcSrcCellID = " & src_DataRow.Item("OSCalcSrcCellID") & " ")"												
                //						dml.AppendLine("AND OSCalcID = " & GBL_OSCalcID & ";")"												
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", FMM_Src_Cell_DT, sqlDataAdapter, "Src_Cell_ID");
            }
        }

        private void Update_Unbal_Src_Columns(DataRow dest_DataRow)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            string unbal_Src_Cell_Buffer_Filter = string.Empty;
            int unbal_Src_Cell_Buffer_Filter_Cnt = 0;
            string override_Dest_Val = string.Empty;
            int override_Dest_Cnt = 0;
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
                new DbParamInfo("@Calc_ID", gbl_Calc_ID)
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
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Src_Cell_DT = new DataTable();

                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    var sql = @"SELECT * 
                                FROM FMM_Src_Cell 
                                WHERE Calc_ID = @Calc_ID
                                ORDER BY Src_Order";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID },
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Src_Cell_DT, sql, sqlparams);

                    FMM_Src_Cell_DT.PrimaryKey = new DataColumn[] { FMM_Src_Cell_DT.Columns["Cell_ID"] };

                    foreach (DataRow src_DataRow in FMM_Src_Cell_DT.Rows)
                    {
                        unbal_Src_Cell_Buffer_Filter = string.Empty;
                        unbal_Src_Cell_Buffer_Filter_Cnt = 0;
                        override_Dest_Val = string.Empty;
                        override_Dest_Cnt = 0;

                        // Filter columns come from FMM_Dest_Cell (first arg)


                        Evaluate_Dim("Acct_Filter", "Acct", "A#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("Origin_Filter", "Origin", "O#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("Flow_Filter", "Flow", "F#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("IC_Filter", "IC", "I#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD1_Filter", "UD1", "U1#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD2_Filter", "UD2", "U2#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD3_Filter", "UD3", "U3#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD4_Filter", "UD4", "U4#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD5_Filter", "UD5", "U5#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD6_Filter", "UD6", "U6#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD7_Filter", "UD7", "U7#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        Evaluate_Dim("UD8_Filter", "UD8", "U8#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        src_DataRow["Unbal_Buffer_Filter"] = unbal_Src_Cell_Buffer_Filter;
                        src_DataRow["Override_Value"] = override_Dest_Val;
                    }
                    cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", FMM_Src_Cell_DT, sqa, "Src_Cell_ID");
                }
            }
        }

        private void Update_Cell_Columns()
        {
            try
            {
                var dim_token = new Dictionary<string, List<string>>();
                var curr_Cube_Buffer_Filter = String.Empty;
                var src_Buffer_Filter = String.Empty;
                int curr_Cube_Buffer_FilterCnt = 0;
                int src_Buffer_FilterCnt = 0;

                string[] core_Dim_Types = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                #region "Core Dim Types"												
                foreach (var dim_Type in core_Dim_Types)
                {
                    switch (dim_Type)
                    {
                        case "Acct":
                            dim_token[dim_Type] = new List<string> { "A#" };
                            break;
                        case "IC":
                            dim_token[dim_Type] = new List<string> { "I#", "IC#" };
                            break;
                        case "Origin":
                            dim_token[dim_Type] = new List<string> { "O#" };
                            break;
                        case "Flow":
                            dim_token[dim_Type] = new List<string> { "F#" };
                            break;
                        case "UD1":
                            dim_token[dim_Type] = new List<string> { "UD1#", "U1#" };
                            break;
                        case "UD2":
                            dim_token[dim_Type] = new List<string> { "UD2#", "U2#" };
                            break;
                        case "UD3":
                            dim_token[dim_Type] = new List<string> { "UD3#", "U3#" };
                            break;
                        case "UD4":
                            dim_token[dim_Type] = new List<string> { "UD4#", "U4#" };
                            break;
                        case "UD5":
                            dim_token[dim_Type] = new List<string> { "UD5#", "U5#" };
                            break;
                        case "UD6":
                            dim_token[dim_Type] = new List<string> { "UD6#", "U6#" };
                            break;
                        case "UD7":
                            dim_token[dim_Type] = new List<string> { "UD7#", "U7#" };
                            break;
                        case "UD8":
                            dim_token[dim_Type] = new List<string> { "UD8#", "U8#" };
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
                //	                                    Where Calc.Calc_ID = @gbl_Calc_ID";												

                if (gbl_Bal_Calc != "DB Model")
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var sqa = new SqlDataAdapter();
                        var FMM_Dest_Cell_DT = new DataTable();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        var sql = @"SELECT * 
                                    FROM FMM_Dest_Cell
                                    WHERE Calc_ID = @Calc_ID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID }
                        };

                        connection.Open();

                        cmdBuilder.FillDataTable(si, sqa, FMM_Dest_Cell_DT, sql, sqlparams);
                        foreach (DataRow FMM_Dest_Cell_DT_Row in FMM_Dest_Cell_DT.Rows)
                        {
                            foreach (var dim_Type in core_Dim_Types)
                            {
                                string dest_Field = "" + dim_Type;
                                string filter_Field = "OS_" + dim_Type + "_Filter";
                                string balanceKey = dim_Type;
                                string dim_token_1 = string.Empty;
                                string dim_token_2 = string.Empty;
                                List<string> dim_tokens = dim_token[dim_Type];
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

                                if (FMM_Dest_Cell_DT_Row[dest_Field] != DBNull.Value)
                                {
                                    string targetValue = FMM_Dest_Cell_DT_Row[dest_Field].ToString();
                                    if (targetValue.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        targetValue.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + targetValue + "]";
                                        curr_Cube_Buffer_FilterCnt++;
                                    }
                                }

                                if (FMM_Dest_Cell_DT_Row[filter_Field] != DBNull.Value)
                                {
                                    string filterValue = FMM_Dest_Cell_DT_Row[filter_Field].ToString();
                                    if (filterValue.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        filterValue.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {

                                        // Check if the filterValue contains the pattern |!MbrList_1_Filter.Name!|
                                        string patternStart = "|!MbrList_";
                                        string patternEnd = "_Filter.Name!|";

                                        if (filterValue.Contains(patternStart) && filterValue.Contains(patternEnd))
                                        {
                                            // Find the start and end indexes of the number
                                            int startIndex = filterValue.IndexOf(patternStart) + patternStart.Length;
                                            int endIndex = filterValue.IndexOf(patternEnd, startIndex);

                                            // Extract the number (substring between the pattern start and end)
                                            string number = filterValue.Substring(startIndex, endIndex - startIndex);
                                            // Call the function that selects from FMM_Calc_Config table and returns MbrList_x_Filter
                                            string memberListFilter = Get_Calc_Config_MbrList(number); // Assuming this is your function
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + memberListFilter + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + curr_Cube_Buffer_Filter);
                                        }
                                        else
                                        {
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + curr_Cube_Buffer_Filter);
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                            BRApi.ErrorLog.LogMessage(si, "Hit: " + curr_Cube_Buffer_Filter);
                                        }
                                    }
                                }
                            }
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + curr_Cube_Buffer_Filter);
                            FMM_Dest_Cell_DT_Row["Curr_Cube_Buffer_Filter"] = curr_Cube_Buffer_Filter;
                            FMM_Dest_Cell_DT_Row["Buffer_Filter"] = src_Buffer_Filter;
                        }
                        cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");

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

                    var sql = @"Select *
                                FROM FMM_Dest_Cell
                                WHERE Calc_ID = @Curr_Calc_ID";

                    using (var command = new SqlCommand(sql, connection))
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
                    gbl_Bal_Calc = "Balanced";
                    Update_Src_Cell_Columns(destRow);
                    Update_Cell_Columns();
                    Update_Globals();

                    if (gbl_Bal_Calc == "Unbalanced" || gbl_Bal_Calc == "UnbalAlloc" || gbl_Bal_Calc == "Ext_Unbalanced" || gbl_Bal_Calc == "Ext_UnbalAlloc")
                    {
                        Update_Unbal_Src_Columns(destRow);
                    }
                }

                Update_Calc_Config_Columns();
                gbl_SrcCell_Dict.Clear();

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
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Calc_Config_DT = new DataTable();

                    // Fill the DataTable with the current data from FMM_Dest_Cell												
                    var sql = @"SELECT * 												
                                FROM FMM_Calc_Config 												
                                WHERE Calc_ID = @Calc_ID";

                    // Create an array of SqlParameter objects												
                    var sqlparams = new SqlParameter[]
                    {
                            new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);

                    // Check if the DataTable has rows
                    if (FMM_Calc_Config_DT.Rows.Count > 0)
                    {
                        // Loop through each DataRow in DataTable												
                        foreach (DataRow row in FMM_Calc_Config_DT.Rows)
                        {
                            if (row["MultiDim_Alloc"] != DBNull.Value && Convert.ToBoolean(row["MultiDim_Alloc"]))
                            {
                                gbl_Bal_Calc = "MultiDim_Alloc";
                                row["Bal_Buffer"] = gbl_Bal_Calc;
                                row["Bal_Buffer_Calc"] = gbl_Unbal_Buffer_Calc;
                                row["Unbal_Calc"] = gbl_Unbal_Calc;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }
                            else if (row["MbrList_Calc"] != DBNull.Value && Convert.ToBoolean(row["MbrList_Calc"]))
                            {
                                if (gbl_Bal_Calc == "Unbalanced")
                                {
                                    gbl_Bal_Calc = "Ext_Unbalanced";
                                    row["Bal_Buffer"] = gbl_Bal_Calc;
                                    row["Bal_Buffer_Calc"] = gbl_Unbal_Buffer_Calc;
                                    row["Unbal_Calc"] = gbl_Unbal_Calc;
                                    row["Update_Date"] = DateTime.Now;
                                    row["Update_User"] = si.UserName;
                                }
                                else if (gbl_Bal_Calc == "UnbalAlloc")
                                {
                                    gbl_Bal_Calc = "Ext_UnbalAlloc";
                                    row["Bal_Buffer"] = gbl_Bal_Calc;
                                    row["Bal_Buffer_Calc"] = gbl_Unbal_Buffer_Calc;
                                    row["Unbal_Calc"] = gbl_Unbal_Calc;
                                    row["Update_Date"] = DateTime.Now;
                                    row["Update_User"] = si.UserName;
                                }
                            }
                            else if (row["BR_Calc"] != DBNull.Value && Convert.ToBoolean(row["BR_Calc"]))
                            {
                                gbl_Bal_Calc = "BR_Calc";
                            }
                            else if (gbl_Bal_Calc == "Balanced")
                            {
                                row["Bal_Buffer"] = "Balanced";
                                row["Bal_Buffer_calc"] = gbl_Bal_Buffer_Calc;
                                row["Unbal_Calc"] = String.Empty;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;

                            }
                            else if (gbl_Bal_Calc == "DB Model")
                            {
                                row["Bal_Buffer"] = gbl_Bal_Calc;
                                row["Table_Calc_Logic"] = gbl_Table_Calc_Logic;
                                row["Table_Src_Cell_Count"] = gbl_Table_SrcCell;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }
                            else
                            {
                                row["Bal_Buffer"] = gbl_Bal_Calc;
                                row["Bal_Buffer_calc"] = gbl_Unbal_Buffer_Calc;
                                row["Unbal_Calc"] = gbl_Unbal_Calc;
                                row["Update_Date"] = DateTime.Now;
                                row["Update_User"] = si.UserName;
                            }

                        }
                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable												
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", FMM_Calc_Config_DT, sqa, "Calc_ID");
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
                // Assuming gbl_SrcCell_Dict is a Dictionary<int	 string>											
                var sortedDict = gbl_SrcCell_Dict.OrderBy(entry => entry.Key);

                BRApi.ErrorLog.LogMessage(si, "Hit Globals 1" + gbl_Bal_Calc);

                int calcIterations = 1;
                foreach (KeyValuePair<int, string> calcSegment in sortedDict)
                {
                    if (gbl_Bal_Calc != "DB Model")
                    {
                        gbl_Bal_Buffer_Calc += StringHelper.ReplaceString(calcSegment.Value, "|", string.Empty, true);
                        if (calcIterations == 1 && (gbl_Bal_Calc == "Unbalanced" || gbl_Bal_Calc == "UnbalAlloc"))
                        {
                            gbl_Unbal_Buffer_Calc = gbl_SrcCell_Drill_Dict[calcSegment.Key];
                            gbl_Unbal_Calc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "-Calculation-", "BalancedBuffer", true), "|", "", true);
                        }
                        else if (gbl_Bal_Calc == "Unbalanced" || gbl_Bal_Calc == "UnbalAlloc")
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Globals Unbal");
                            gbl_Unbal_Calc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "-Calculation-", "SrcBufferValue" + calcIterations.ToString(), true), "|", "", true);
                        }
                    }
                    else
                    {
                        gbl_Unbal_Buffer_Calc = gbl_SrcCell_Drill_Dict[calcSegment.Key];
                        if (gbl_Unbal_Buffer_Calc.Contains("T#|DBModelYear|", StringComparison.OrdinalIgnoreCase))
                        {
                            gbl_Table_Calc_Logic += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "Cube-Calculation-", "Annual_Cube", true), "|", "", true);
                        }
                        else
                        {
                            gbl_Table_Calc_Logic += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "Cube-Calculation-", "Monthly_Cube", true), "|", "", true);
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

        private void Evaluate_Dim(string destFilterColumn, string src_Column, string filter_Prefix, DataTable srcDt, DataRow src_DataRow, ref string bufferFilter, ref int bufferFilterCnt, DataRow dest_DataRow, ref string override_Dest_Val, ref int override_Cnt)
        {
            if (gbl_Bal_Calc == "Unbalanced" || (gbl_Bal_Calc == "UnbalAlloc" && Convert.ToInt32(src_DataRow["Src_Order"]) == 1))
            {
                if (dest_DataRow[destFilterColumn] != DBNull.Value && XFContains_Ignore_Case(dest_DataRow[destFilterColumn].ToString(), filter_Prefix))
                {
                    // based on discussion w/ Jeff, if the Src_ col is empty then the unbal src buffer filter in src should contain the associated dim filter
                    if (src_DataRow["" + src_Column] == DBNull.Value && !XFContains_Ignore_Case(src_DataRow["" + src_Column].ToString(), filter_Prefix))
                    {
                        Add_to_Buffer_Filter(dest_DataRow[destFilterColumn].ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
            }
            else
            {
                if (src_DataRow["" + src_Column] == DBNull.Value || !XFContains_Ignore_Case(src_DataRow["" + src_Column].ToString(), filter_Prefix))
                {
                    // is this supposed to be checking 2 rows before? No Austin
                    // Check the FIlters in teh Dest Cell table and compare check the current cell for each of those dimensions.  If the dimension has explicit member defined for it, drop the Dest Cell Filter for that cell, otherwise keep it.
                    if (srcDt.Rows[Convert.ToInt32(src_DataRow["Src_Order"]) - 2]["" + src_Column] == DBNull.Value || !XFContains_Ignore_Case(srcDt.Rows[Convert.ToInt32(src_DataRow["Src_Order"]) - 2]["" + src_Column].ToString(), filter_Prefix))
                    {
                        //Add_to_Buffer_Filter($"{filter_Prefix}Replace", ref bufferFilter, ref bufferFilterCnt);
                    }
                    else if (dest_DataRow[destFilterColumn] != DBNull.Value && XFContains_Ignore_Case(dest_DataRow[destFilterColumn].ToString(), filter_Prefix))
                    {
                        Add_to_Buffer_Filter(dest_DataRow[destFilterColumn].ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
            }
            if (Convert.ToInt32(src_DataRow["Src_Order"]) != 1)
            {
                if (((!dest_DataRow["" + src_Column].ToString().XFContainsIgnoreCase(filter_Prefix) && !dest_DataRow["OS_" + src_Column + "_Filter"].ToString().XFContainsIgnoreCase(filter_Prefix)) &&
                     src_DataRow["Dyn_Calc_Script"].ToString().XFContainsIgnoreCase(filter_Prefix)) ||
                    (srcDt.Rows[Convert.ToInt32(src_DataRow["Src_Order"]) - 2]["" + src_Column].ToString().XFContainsIgnoreCase(filter_Prefix) &&
                     src_DataRow["" + src_Column] == DBNull.Value) &&
                    !src_DataRow["Unbal_" + src_Column + "_Override"].ToString().XFContainsIgnoreCase(filter_Prefix))
                {
                    if (override_Cnt > 0)
                    {
                        override_Dest_Val += "," + filter_Prefix;
                    }
                    else
                    {
                        override_Dest_Val = filter_Prefix;
                    }
                    override_Cnt++;
                }
            }
        }

        private void Add_to_Buffer_Filter(string value, ref string bufferFilter, ref int bufferFilterCnt)
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

        private bool XFContains_Ignore_Case(string source, string toCheck)
        {
            return source?.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public class MbrList_Dim_Checker
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

            public string Dim_Check(DataRow row, string columnName)
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
            public string Get_Src_Dest_Filter(DataRow row, string columnName)
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

        private string Get_Calc_Config_MbrList(string number)
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
                var sql = @$"SELECT MbrList_{number}_Filter
                            FROM FMM_Calc_Config
                            WHERE Calc_ID = @Calc_ID";
                // Create an array of SqlParameter objects
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@Calc_ID", SqlDbType.Int) { Value = gbl_Calc_ID}
                };

                // Attempt to fill the data table and check for any issues
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);

                foreach (DataRow Calc_Config_Row in FMM_Calc_Config_DT.Rows)
                {
                    return (string)Calc_Config_Row[$"MbrList_{number}_Filter"];
                }
            }


            // Return the result (this could be empty if no match was found)
            return string.Empty;
        }


        #endregion

        #region "Check Duplicates"

        #region "Duplicate Cube Config"
        private void Duplicate_Act_Config(int cube_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Act_Config_DataRow)
        {
            var act_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Act_Config
                                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Act_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Act_Config_Dict and handle errors
                            foreach (DataRow cube_Activity_Row in FMM_Act_Config_DT.Rows)
                            {
                                act_Key = cube_ID + "|" + (int)cube_Activity_Row["Act_ID"];
                                gbl_Act_Config_Dict.Add(act_Key, (string)cube_Activity_Row["Name"] + "|" + (string)cube_Activity_Row["Calc_Type"]);
                            }
                        }
                        break;

                    case "Update Row":

                        act_Key = cube_ID + "|" + (int)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Act_ID"];
                        var newact_Value = (string)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Name"] + "|" + (string)Modified_FMM_Act_Config_DataRow.ModifiedDataRow["Calc_Type"];


                        if (ddl_Process == "Insert")
                        {
                            gbl_Act_Config_Dict.Add(act_Key, newact_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origact_Value = (string)Modified_FMM_Act_Config_DataRow.OriginalDataRow["Name"] + "|" + (string)Modified_FMM_Act_Config_DataRow.OriginalDataRow["Calc_Type"];

                            if (origact_Value != newact_Value)
                            {
                                gbl_Act_Config_Dict.XFSetValue(act_Key, newact_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Act_Config_Dict.ContainsKey(act_Key))
                            {
                                gbl_Act_Config_Dict.Remove(act_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {act_Key} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Calc_Unit_Config(int cube_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Calc_Unit_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Calc_Unit_Config
                                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Unit_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Calc_Unit_Config_Dict and handle errors
                            foreach (DataRow Calc_Unit_Row in FMM_Calc_Unit_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + (int)Calc_Unit_Row["Calc_Unit_ID"];
                                var config_Value = (string)Calc_Unit_Row["Entity_MFB"] + "|" + (string)Calc_Unit_Row["WFChannel"];

                                gbl_Calc_Unit_Config_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        var newconfig_Value = string.Empty;

                        if (ddl_Process == "Insert")
                        {
                            config_Key = cube_ID + "|" + (int)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Calc_Unit_ID"];
                            newconfig_Value = (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["WFChannel"];

                            gbl_Calc_Unit_Config_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            config_Key = cube_ID + "|" + (int)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Calc_Unit_ID"];
                            newconfig_Value = (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_Calc_Unit_Config_DataRow.ModifiedDataRow["WFChannel"];
                            var origConfig_Value = (string)Modified_FMM_Calc_Unit_Config_DataRow.OriginalDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_Calc_Unit_Config_DataRow.OriginalDataRow["WFChannel"];

                            if (origConfig_Value != newconfig_Value)
                            {
                                gbl_Calc_Unit_Config_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            config_Key = cube_ID + "|" + (int)Modified_FMM_Calc_Unit_Config_DataRow.OriginalDataRow["Calc_Unit_ID"];
                            if (gbl_Calc_Unit_Config_Dict.ContainsKey(config_Key))
                            {
                                gbl_Calc_Unit_Config_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = "An unexpected error occurred.";
            }
        }

        #endregion

        #region "Duplicate Unit/Acct"
        private void Duplicate_Unit_Config(int cube_ID, int act_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Unit_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Unit_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Unit_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID} and Act_ID {act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Unit_Config_Dict and handle errors
                            foreach (DataRow cube_Unit_Row in FMM_Unit_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + act_ID + "|" + (int)cube_Unit_Row["Unit_ID"];
                                var config_Value = (string)cube_Unit_Row["Name"];

                                gbl_Unit_Config_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + act_ID + "|" + (int)Modified_FMM_Unit_Config_DataRow.ModifiedDataRow["Unit_ID"];
                        var newconfig_Value = (string)Modified_FMM_Unit_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Unit_Config_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origconfig_Value = (string)Modified_FMM_Unit_Config_DataRow.OriginalDataRow["Name"];

                            if (origconfig_Value != newconfig_Value)
                            {
                                gbl_Unit_Config_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Unit_Config_Dict.ContainsKey(config_Key))
                            {
                                gbl_Unit_Config_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = "An unexpected error occurred.";
            }
        }

        private void Duplicate_Acct_Config(int cube_ID, int act_ID, int unit_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Acct_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Acct_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID
                                        AND Unit_ID = @Unit_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                                new SqlParameter("@Unit_ID", SqlDbType.Int) { Value = unit_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Acct_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}, Act_ID {act_ID}, Unit_ID {unit_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Acct_Config_Dict and handle errors
                            foreach (DataRow cube_Acct_Row in FMM_Acct_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + act_ID + "|" + unit_ID + "|" + (int)cube_Acct_Row["Acct_ID"];
                                var config_Value = (string)cube_Acct_Row["Name"];

                                gbl_Acct_Config_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + act_ID + "|" + unit_ID + "|" + (int)Modified_FMM_Acct_Config_DataRow.ModifiedDataRow["Acct_ID"];
                        var newconfig_Value = (string)Modified_FMM_Acct_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Acct_Config_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origConfig_Value = (string)Modified_FMM_Acct_Config_DataRow.OriginalDataRow["Name"];

                            if (origConfig_Value != newconfig_Value)
                            {
                                gbl_Acct_Config_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Acct_Config_Dict.ContainsKey(config_Key))
                            {
                                gbl_Acct_Config_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion

        #region "Duplicate Approvals"
        private void Duplicate_Config(int cube_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Appr_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Appr_Config
                                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Appr_Dict and handle errors
                            foreach (DataRow Appr_Row in FMM_Appr_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + (int)Appr_Row["Appr_ID"];
                                var config_Value = (string)Appr_Row["Name"];

                                if (!gbl_Appr_Dict.ContainsKey(config_Key))
                                {
                                    gbl_Appr_Dict.Add(config_Key, config_Value);
                                }
                                else
                                {
                                    save_Result.IsOK = false;
                                    save_Result.ShowMessageBox = true;
                                    save_Result.Message += $"Duplicate found in initial fetch: {config_Key}";
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + (int)Modified_FMM_Appr_Config_DataRow.ModifiedDataRow["Appr_ID"];
                        string newconfig_Value = (string)Modified_FMM_Appr_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Appr_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            string origconfig_Value = (string)Modified_FMM_Appr_Config_DataRow.OriginalDataRow["Name"];

                            if (origconfig_Value != newconfig_Value)
                            {
                                gbl_Appr_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Appr_Dict.ContainsKey(config_Key))
                            {
                                gbl_Appr_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Appr_Step_Config(int cube_ID, int appr_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Appr_Step_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Appr_Step_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Appr_ID = @Appr_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Appr_Step_Dict and handle errors
                            foreach (DataRow Appr_Step_Row in FMM_Appr_Step_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + appr_ID + "|" + (int)Appr_Step_Row["Appr_Step_ID"];
                                var config_Value = (string)Appr_Step_Row["Name"];

                                if (!gbl_Appr_Step_Dict.ContainsKey(config_Key))
                                {
                                    gbl_Appr_Step_Dict.Add(config_Key, config_Value);
                                }
                                else
                                {
                                    save_Result.IsOK = false;
                                    save_Result.ShowMessageBox = true;
                                    save_Result.Message += $"Duplicate found in initial fetch: {config_Key}";
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + appr_ID + "|" + (int)Modified_FMM_Appr_Step_Config_DataRow.ModifiedDataRow["Appr_Step_ID"];
                        var newconfig_Value = (string)Modified_FMM_Appr_Step_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Appr_Step_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origconfig_Value = (string)Modified_FMM_Appr_Step_Config_DataRow.OriginalDataRow["Name"];

                            if (origconfig_Value != newconfig_Value)
                            {
                                gbl_Appr_Step_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Appr_Step_Dict.ContainsKey(config_Key))
                            {
                                gbl_Appr_Step_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }


        #endregion

        #region "Duplicate Register Config"
        private void Duplicate_Reg_Config(int cube_ID, int act_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Reg_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Reg_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Reg_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID} and Act_ID {act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Register_Dict and handle errors
                            foreach (DataRow Register_Row in FMM_Reg_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + act_ID + "|" + (int)Register_Row["Reg_Config_ID"];
                                var config_Value = (string)Register_Row["Name"];


                                gbl_Register_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + act_ID + "|" + (int)Modified_FMM_Reg_Config_DataRow.ModifiedDataRow["Reg_Config_ID"];
                        var newConfig_Value = (string)Modified_FMM_Reg_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Register_Dict.Add(config_Key, newConfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origConfig_Value = (string)Modified_FMM_Reg_Config_DataRow.OriginalDataRow["Name"];

                            if (origConfig_Value != newConfig_Value)
                            {
                                gbl_Register_Dict.XFSetValue(config_Key, newConfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Register_Dict.ContainsKey(config_Key))
                            {
                                gbl_Register_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Col_Config(int cube_ID, int act_ID, int reg_config_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Col_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Col_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID
                                        AND Reg_Config_ID = @Reg_Config_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                                new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = reg_config_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Col_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID} and Act_ID {act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Col_Dict and handle errors
                            foreach (DataRow Col_Row in FMM_Col_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + act_ID + "|" + reg_config_ID + "|" + (int)Col_Row["Col_ID"];
                                var config_Value = ((int)Col_Row["Order"]).XFToString();

                                if (config_Value != "99")
                                {
                                    gbl_Col_Dict.Add(config_Key, config_Value);
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + act_ID + "|" + reg_config_ID + "|" + (int)Modified_FMM_Col_Config_DataRow.ModifiedDataRow["Col_ID"];
                        var newconfig_Value = ((int)Modified_FMM_Col_Config_DataRow.ModifiedDataRow["Order"]).XFToString();

                        if (ddl_Process == "Update")
                        {
                            var origconfig_Value = ((int)Modified_FMM_Col_Config_DataRow.OriginalDataRow["Order"]).XFToString();

                            if (origconfig_Value != newconfig_Value)
                            {
                                gbl_Col_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion

        #region "Duplicate Calcs"
        private void Duplicate_Calc_Config(int cube_ID, int act_ID, int model_ID, string dup_Process_Step, ref XFSelectionChangedTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Calc_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Calc_Config
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID
                                        AND Model_ID = @Model_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                                new SqlParameter("@Model_ID", SqlDbType.Int) { Value = model_ID }
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID} and Act_ID {act_ID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate gbl_Calc_Dict and handle errors
                            foreach (DataRow Calc_Row in FMM_Calc_Config_DT.Rows)
                            {
                                config_Key = cube_ID + "|" + act_ID + "|" + model_ID + "|" + (int)Calc_Row["Calc_ID"];
                                var config_Value = (string)Calc_Row["Name"];


                                gbl_Calc_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = cube_ID + "|" + act_ID + "|" + model_ID + "|" + (int)Modified_FMM_Calc_Config_DataRow.ModifiedDataRow["Calc_ID"];
                        var newconfig_Value = (string)Modified_FMM_Calc_Config_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Calc_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origConfig_Value = (string)Modified_FMM_Calc_Config_DataRow.OriginalDataRow["Name"];

                            if (origConfig_Value != newconfig_Value)
                            {
                                gbl_Calc_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_Calc_Dict.ContainsKey(config_Key))
                            {
                                gbl_Calc_Dict.Remove(config_Key);
                            }
                            else
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message += $"Delete operation failed: entry {config_Key} not found.";
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        #endregion
        #endregion

        #region "Col Helpers"

        public void Insert_Col_Default_Rows(int cube_ID, int act_ID, int Reg_Config_ID)
        {
            BRApi.ErrorLog.LogMessage(si, "Hit Defaults: " + "|" + si.AuthToken.AuthSessionID.ToString());
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
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
                    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                    new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                    new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { Value = Reg_Config_ID }
                };
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, selectQuery_colSetup, parameters_colSetup);
                //os_Col_ID -= os_Col_ID;
                // Insert new rows into the DataTable using default values
                var columnConfigDefaults = new ColumnConfigDefaults();
                foreach (var columnConfig in columnConfigDefaults.DefaultColumns)
                {
                    var new_DataRow = FMM_Col_Config_DT.NewRow();
                    new_DataRow["Cube_ID"] = cube_ID;
                    new_DataRow["Act_ID"] = act_ID;
                    new_DataRow["Reg_Config_ID"] = Reg_Config_ID;
                    new_DataRow["Col_ID"] = ++os_Col_ID; // Ensure unique IDs
                    new_DataRow["Name"] = columnConfig.Col_Name;
                    new_DataRow["InUse"] = true; // Set default value for Col_InUse if needed
                    new_DataRow["Required"] = true;
                    new_DataRow["Updates"] = true;
                    new_DataRow["Alias"] = columnConfig.Col_Alias;
                    new_DataRow["Order"] = columnConfig.Col_Order;
                    new_DataRow["Default"] = DBNull.Value; // Set default value if needed
                    new_DataRow["Param"] = DBNull.Value; // Set default value if needed
                    new_DataRow["Format"] = string.Empty; // Set default value if needed
                    new_DataRow["Filter_Param"] = DBNull.Value; // Set default value if needed
                    new_DataRow["Create_Date"] = DateTime.Now;
                    new_DataRow["Create_User"] = si.UserName;
                    new_DataRow["Update_Date"] = DateTime.Now;
                    new_DataRow["Update_User"] = si.UserName;

                    FMM_Col_Config_DT.Rows.Add(new_DataRow);
                }
                BRApi.ErrorLog.LogMessage(si, "Hit Defaults 2");
                // Update the FMM_Col_Config table based on the changes made to the DataTable
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa, "Col_ID");
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
                    new ColumnConfig { Col_Name = "Level_ID", Col_Alias = "Approval Level ID", Col_Order = 2 },
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

        private XFSelectionChangedTaskResult Save_Cube_Config(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var Cube = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_All_Cube_Names", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube", string.Empty));
                var Scen_Type = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_Scen_Types", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Scen_Type", string.Empty));
                var agg_Consol = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Agg_Consol", string.Empty);
                var entity_Dim = String.Empty;
                var entity_MFB = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Entity_MFB", string.Empty);
                var cube_Description = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Descr", string.Empty);
                int new_Cube_ID = 0;

                BRApi.ErrorLog.LogMessage(si, "hit: " + Cube);
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    // Create a new DataTable to check for existing records
                    var sqa = new SqlDataAdapter();
                    var FMM_Cube_Config_Count_DT = new DataTable();

                    // Query to check if the cube config already exists
                    var sql = @"SELECT Count(*) as Count
                                FROM FMM_Cube_Config
                                WHERE Cube = @Cube
                                AND Scen_Type = @Scen_Type";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.NVarChar, 50) { Value = Cube },
                        new SqlParameter("@Scen_Type", SqlDbType.NVarChar, 20) { Value = Scen_Type }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Cube_Config_Count_DT, sql, sqlparams);

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_Cube_Config_DT = new DataTable();

                    // Insert new cube config record
                    sql = "SELECT * FROM FMM_Cube_Config WHERE Cube_ID = @Cube_ID";
                    // Define the variable to hold the parameters
                    sqlparams = new SqlParameter[]
                    {
                    };

                    // If no existing record and runType is "New", insert new record
                    if (runType == "New")
                    {
                        // Get the next Cube_ID for the new record
                        new_Cube_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Cube_Config", "Cube_ID");

                        // Assign a new array with the parameter to the variable
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = new_Cube_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Cube_Config_DT, sql, sqlparams);

                        var new_DataRow = FMM_Cube_Config_DT.NewRow();
                        new_DataRow["Cube_ID"] = new_Cube_ID;
                        new_DataRow["Cube"] = Cube;
                        new_DataRow["Scen_Type"] = Scen_Type;
                        new_DataRow["Descr"] = cube_Description;
                        new_DataRow["Agg_Consol"] = agg_Consol;
                        new_DataRow["Entity_Dim"] = entity_Dim;
                        new_DataRow["Entity_MFB"] = entity_MFB;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["Create_Date"] = DateTime.Now;
                        new_DataRow["Create_User"] = si.UserName;
                        new_DataRow["Update_Date"] = DateTime.Now;
                        new_DataRow["Update_User"] = si.UserName;

                        FMM_Cube_Config_DT.Rows.Add(new_DataRow);

                        // Save the changes to the database
                        cmdBuilder.UpdateTableSimple(si, "FMM_Cube_Config", FMM_Cube_Config_DT, sqa, "Cube_ID");

                        save_Result.IsOK = true;
                        save_Result.Message = "New Cube Config Saved.";
                        save_Result.ShowMessageBox = true;
                    }
                    // If record exists and runType is "Update", update the record
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && runType == "Update")
                    {
                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                        new_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0"));
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = new_Cube_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Cube_Config_DT, sql, sqlparams);

                        // Update the existing row
                        if (FMM_Cube_Config_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_Cube_Config_DT.Rows[0];
                            rowToUpdate["Descr"] = cube_Description;
                            rowToUpdate["Entity_MFB"] = entity_MFB;
                            rowToUpdate["Agg_Consol"] = agg_Consol;
                            rowToUpdate["Status"] = cubeStatus;
                            rowToUpdate["Update_Date"] = DateTime.Now;
                            rowToUpdate["Update_User"] = si.UserName;

                            // Update the database with the changes
                            cmdBuilder.UpdateTableSimple(si, "FMM_Cube_Config", FMM_Cube_Config_DT, sqa, "Cube_ID");

                            save_Result.IsOK = true;
                            save_Result.Message = "Cube Config Updates Saved.";
                            save_Result.ShowMessageBox = true;
                        }
                    }
                    // If a duplicate exists and runType is "New", show an error
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && runType == "New")
                    {
                        save_Result.IsOK = false;
                        save_Result.Message = "Duplicated Cube and Scenario Type, Cube Config not saved.";
                        save_Result.ShowMessageBox = true;
                    }
                }

                return save_Result;
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


        private XFSelectionChangedTaskResult Save_Model(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var customSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var act_ID = Convert.ToInt32(customSubstVars.XFGetValue("IV_FMM_Act_ID", "0"));
                var cube_ID = Convert.ToInt32(customSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_Model_Name = customSubstVars.XFGetValue("IV_FMM_Model_Name", string.Empty);
                int new_Model_ID = 0;
                if (new_Model_ID == 0)
                {
                    DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        if (runType == "New")
                        {
                            new_Model_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "Model_ID");
                        }
                        else
                        {
                            new_Model_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_ID", "0"));
                        }

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Models_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
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

                        cmdBuilder.FillDataTable(si, sqa, FMM_Models_DT, sql, sqlparams);
                        if (runType == "New")
                        {
                            var new_DataRow = FMM_Models_DT.NewRow();
                            new_DataRow["Cube_ID"] = cube_ID;
                            new_DataRow["Act_ID"] = act_ID;
                            new_DataRow["Model_ID"] = new_Model_ID;
                            new_DataRow["Name"] = new_Model_Name;
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;
                            // Set other column values for the new row as needed
                            FMM_Models_DT.Rows.Add(new_DataRow);
                        }
                        else if (runType == "Update")
                        {
                            if (FMM_Models_DT.Rows.Count > 0)
                            {
                                BRApi.ErrorLog.LogMessage(si, "Hit this");
                                var rowToUpdate = FMM_Models_DT.Rows[0];
                                rowToUpdate["Name"] = new_Model_Name;
                                rowToUpdate["Status"] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                                rowToUpdate["Update_Date"] = DateTime.Now;
                                rowToUpdate["Update_User"] = si.UserName;
                            }

                        }
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Models", FMM_Models_DT, sqa, "Model_ID");
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
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

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Model_Grps_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
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

                        cmdBuilder.FillDataTable(si, sqa, FMM_Model_Grps_DT, sql, sqlparams);

                        var new_DataRow = FMM_Model_Grps_DT.NewRow();
                        new_DataRow["Cube_ID"] = new_OS_Model_Grp_Cube_ID;
                        new_DataRow["Model_Grp_ID"] = new_Model_Grp_ID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["Create_Date"] = DateTime.Now;
                        new_DataRow["Create_User"] = si.UserName;
                        new_DataRow["Update_Date"] = DateTime.Now;
                        new_DataRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grps_DT.Rows.Add(new_DataRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grps", FMM_Model_Grps_DT, sqa, "Model_Grp_ID");
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp_Seq(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
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

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Model_Grp_Seqs_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
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

                        cmdBuilder.FillDataTable(si, sqa, FMM_Model_Grp_Seqs_DT, sql, sqlparams);

                        var new_DataRow = FMM_Model_Grp_Seqs_DT.NewRow();
                        new_DataRow["Cube_ID"] = new_Cube_ID;
                        new_DataRow["Model_Grp_Seq_ID"] = new_Model_Grp_Seq_ID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["Create_Date"] = DateTime.Now;
                        new_DataRow["Create_User"] = si.UserName;
                        new_DataRow["Update_Date"] = DateTime.Now;
                        new_DataRow["Update_User"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grp_Seqs_DT.Rows.Add(new_DataRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grp_Seqs", FMM_Model_Grp_Seqs_DT, sqa, "Model_Grp_Seq_ID");
                        BRApi.ErrorLog.LogMessage(si, "Hit 7: ");
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Model_Grp_Assign(string runType)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var cube_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_Grp_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Model_ID_Selection", "0");
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: " + custom_SubstVars.XFGetValue("IV_FMM_Model_ID_Selection", "0") + " - " + cube_ID);
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

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Model_Grp_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string sql = @"
                                        SELECT * 
                                        FROM FMM_Model_Grp_Assign
                                        WHERE Cube_ID = @Cube_ID
                                        AND Model_Grp_ID = @Model_Grp_ID";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = Model_Grp_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Model_Grp_Assign_DT, sql, sqlparams);

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
                            var new_DataRow = FMM_Model_Grp_Assign_DT.NewRow();
                            new_DataRow["Cube_ID"] = cube_ID;
                            new_DataRow["Model_Grp_ID"] = Model_Grp_ID;
                            new_DataRow["Model_ID"] = Convert.ToInt32(modelId.Trim());
                            new_DataRow["Model_Grp_Assign_ID"] = new_OS_Model_Grp_Assign_ID;
                            new_DataRow["Sequence"] = 0;
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;

                            FMM_Model_Grp_Assign_DT.Rows.Add(new_DataRow);
                        }

                        // Update the database with the new rows
                        cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grp_Assign", FMM_Model_Grp_Assign_DT, sqa, "Model_Grp_Assign_ID");
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Calc_Unit_Assign(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var cube_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Selection", "0");
                var Model_Grp_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID_Selection");
                var Model_Grp_Seq_ID = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_ID");
                BRApi.ErrorLog.LogMessage(si, "Hit Grup Model Assignment: " + custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID_Selection") + " - " + cube_ID);
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

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Calc_Unit_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        var sql = @"SELECT * 
                                    FROM FMM_Calc_Unit_Assign
                                    WHERE Cube_ID = @Cube_ID
                                    AND Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = Model_Grp_Seq_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Unit_Assign_DT, sql, sqlparams);

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
                                var new_DataRow = FMM_Calc_Unit_Assign_DT.NewRow();
                                new_DataRow["Cube_ID"] = cube_ID;
                                new_DataRow["Model_Grp_ID"] = Convert.ToInt32(modelGroupId.Trim());
                                new_DataRow["Calc_Unit_ID"] = Convert.ToInt32(WfDuId.Trim());
                                new_DataRow["Model_Grp_Seq_ID"] = Model_Grp_Seq_ID;
                                new_DataRow["Calc_Unit_Assign_ID"] = new_Calc_Unit_Assign_ID;
                                new_DataRow["Sequence"] = 0;
                                new_DataRow["Status"] = "Build";
                                new_DataRow["Create_Date"] = DateTime.Now;
                                new_DataRow["Create_User"] = si.UserName;
                                new_DataRow["Update_Date"] = DateTime.Now;
                                new_DataRow["Update_User"] = si.UserName;

                                FMM_Calc_Unit_Assign_DT.Rows.Add(new_DataRow);
                            }
                        }

                        // Update the database with the new rows
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Appr_Step(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var cube_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var appr_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var wfProfile_Step = custom_SubstVars.XFGetValue("IV_FMM_trv_Appr_Step_WFProfile", string.Empty);

                BRApi.ErrorLog.LogMessage(si, "Hit Approval Step: " + cube_ID + "|" + appr_ID + "|" + wfProfile_Step);
                var new_Appr_Step_ID = 0;

                if (cube_ID > 0 && appr_ID > 0 && wfProfile_Step != string.Empty && new_Appr_Step_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        // Get the max ID for the Appr_Step_Config table
                        new_Appr_Step_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "Appr_Step_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Appr_Step_Config_DT = new DataTable();
                        var FMM_Appr_Step_WFProfile_DT = new DataTable();
                        var FMM_Cube_Config_DT = new DataTable();
                        var FMM_Act_Appr_Step_Config_DT = new DataTable(); // DataTable for FMM_Reg_Dtl_Cube_Map
                        var sqa = new SqlDataAdapter();

                        // Fill DataTable for Appr_Step_Config
                        var sql = @"SELECT * 
                                    FROM FMM_Appr_Step_Config
                                    WHERE Cube_ID = @Cube_ID
                                    AND Appr_ID = @Appr_ID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);

                        // Fill DataTable for Appr_Step_Config
                        //                        sql = @"SELECT * 
                        //                                FROM FMM_Act_Appr_Step_Config
                        //                                WHERE Cube_ID = @Cube_ID
                        //                                AND Act_ID = @Act_ID
                        //                                AND Appr_ID = @Appr_ID
                        //                                AND Appr_Step_ID = @Appr_Step_ID";

                        //                        sqlparams = new SqlParameter[]
                        //                        {
                        //                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID },
                        //                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                        //                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID },
                        //                        };

                        //                        sqa_fmm_act_appr_step_config.Fill_FMM_Act_Appr_Step_Config_DT(si, sqa, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        // Load Cube_Config data
                        sql = @"SELECT * 
                                FROM FMM_Cube_Config
                                WHERE Cube_ID = @Cube_ID";

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Cube_Config_DT, sql, sqlparams);

                        var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, FMM_Cube_Config_DT.Rows[0].Field<string>("Cube"));
                        //var cubeScenarioType = ScenarioType.GetItem(FMM_Cube_Config_DT.Rows[0].Field<string>("Scen_Type")).Id;
                        var cubeScenarioType = ScenarioType.GetItem("Plan").Id;
                        var cubeScenarioTypeId = ScenarioTypeId.LongTerm;
                        var rootProfileName = topCubeInfo.GetTopLevelCubeWFPName(si, cubeScenarioType);
                        var wfProfileSuffixes = new Dictionary<ScenarioTypeId, string>();
                        wfProfileSuffixes = topCubeInfo.TopLevelCubeWFPSuffixes;
                        var wfProfileSuffix = wfProfileSuffixes.XFGetValue(cubeScenarioTypeId, string.Empty);

                        // Loop through each entry in the dictionary
                        foreach (var entry in wfProfileSuffixes)
                        {
                            // You can access the key and value from the 'entry' variable
                            ScenarioTypeId id = entry.Key;
                            string suffix = entry.Value;

                            // Now you can use them, for example, print them to the console
                            BRApi.ErrorLog.LogMessage(si, $"ID: {id} | Suffix: {suffix}");
                        }

                        BRApi.ErrorLog.LogMessage(si, $"Hit: {rootProfileName}_{wfProfileSuffix} - {cubeScenarioTypeId.ToString()}");
                        // Retrieve WFProfile information
                        sql = @"WITH RecursiveCTE AS (
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
                                    RecursiveCTE rcte ON prof.ParentProfileKey = rcte.ProfileKey)
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

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@rootprofilename", SqlDbType.NVarChar) { Value = rootProfileName }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Step_WFProfile_DT, sql, sqlparams);

                        BRApi.ErrorLog.LogMessage(si, $"Hit: {FMM_Appr_Step_WFProfile_DT.Rows.Count}");
                        // LINQ query to filter WFProfiles based on WFProfile_Step
                        var filteredWFProfiles = from profile in FMM_Appr_Step_WFProfile_DT.AsEnumerable()
                                                 where profile["ProfileName"].ToString().Contains(wfProfile_Step)
                                                 select profile;

                        // Loop through filtered WFProfiles and insert new Approval Steps and Cube Map
                        foreach (var profile in filteredWFProfiles)
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit");
                            var new_DataRow = FMM_Appr_Step_Config_DT.NewRow();
                            new_DataRow["Cube_ID"] = cube_ID;
                            new_DataRow["Appr_ID"] = appr_ID;
                            new_DataRow["WFProfile_Step"] = profile["ProfileKey"];
                            new_DataRow["Appr_Step_ID"] = new_Appr_Step_ID++;
                            new_DataRow["Step_Num"] = 0;
                            new_DataRow["User_Group"] = "Test";
                            new_DataRow["Logic"] = "Test";
                            new_DataRow["Item"] = "Test";
                            new_DataRow["Level"] = 0;
                            new_DataRow["Appr_Config"] = 0;
                            new_DataRow["Init_Status"] = "Testy";
                            new_DataRow["Appr_Status"] = "Testy";
                            new_DataRow["Rej_Status"] = "Testy";
                            new_DataRow["Status"] = "Build";
                            new_DataRow["Create_Date"] = DateTime.Now;
                            new_DataRow["Create_User"] = si.UserName;
                            new_DataRow["Update_Date"] = DateTime.Now;
                            new_DataRow["Update_User"] = si.UserName;

                            FMM_Appr_Step_Config_DT.Rows.Add(new_DataRow);

                            //                            // Insert into FMM_Reg_Dtl_Cube_Map
                            //                            var mapRow = FMM_Act_Appr_Step_Config_DT.NewRow();
                            //                            mapRow["Cube_ID"] = cube_ID;
                            //                            mapRow["Appr_ID"] = appr_ID;
                            //                            mapRow["Appr_Step_ID"] = new_DataRow["Appr_Step_ID"];
                            //                            mapRow["FMM_Act_Appr_Step_Config_ID"] = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Appr_Step_Config", "FMM_Act_Appr_Step_Config_ID");
                            //                            mapRow["Acct"] = "AcctValue"; // replace with actual values
                            //                            mapRow["Flow"] = "FlowValue";
                            //                            mapRow["UD1"] = "UD1Value";
                            //                            mapRow["UD2"] = "UD2Value";
                            //                            mapRow["UD3"] = "UD3Value";
                            //                            mapRow["UD4"] = "UD4Value";
                            //                            mapRow["UD5"] = "UD5Value";
                            //                            mapRow["UD6"] = "UD6Value";
                            //                            mapRow["UD7"] = "UD7Value";
                            //                            mapRow["UD8"] = "UD8Value";
                            //                            mapRow["Create_Date"] = DateTime.Now;
                            //                            mapRow["Create_User"] = si.UserName;
                            //                            mapRow["Update_Date"] = DateTime.Now;
                            //                            mapRow["Update_User"] = si.UserName;

                            //                            FMM_Act_Appr_Step_Config_DT.Rows.Add(mapRow);
                        }

                        // Save Approval Steps
                        cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", FMM_Appr_Step_Config_DT, sqa, "Appr_Step_ID");

                        // Save Cube Map entries
                        //SQA_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                        BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                    }
                }
                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Act_Appr_Step_Config(string runType)
        {
            try
            {

                BRApi.ErrorLog.LogMessage(si, "Save approval step activity runtype: " + runType);
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var act_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Act_ID", "0"));
                var appr_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var appr_Step_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_Step_ID", "-1")); // this can actually be 0
                var reg_Config_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Reg_Configs", "0"));

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_Act_Appr_Step_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    // Fill DataTable for Appr_Step_Activity_Config
                    var sql = @"SELECT * 
                                    FROM FMM_Act_Appr_Step_Config
                                    WHERE Act_ID = @Act_ID
                                    AND Appr_ID = @Appr_ID
                                    AND Appr_Step_ID = @Appr_Step_ID";

                    var sqlparams = new SqlParameter[] { };


                    BRApi.ErrorLog.LogMessage(si, "save data: " + act_ID + " " + appr_ID + " " + appr_Step_ID + " " + reg_Config_ID);

                    if (runType == "New" && act_ID > 0 && appr_ID > 0 && appr_Step_ID >= 0 && reg_Config_ID > 0) // new row
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        // Get the max ID for the Appr_Step_Config table
                        var new_Appr_Step_Act_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Appr_Step_Config", "Act_Appr_Step_ID");


                        sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID },
                        new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID },
                        new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = appr_Step_ID },

                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        string noneVal = "None";
                        FMM_Act_Appr_Step_Config_DT.Clear(); // clear data, because we only want the column information to create a new row.
                                                             // Insert into FMM_Appr_Step_Activity_Config_DT
                        var new_DataRow = FMM_Act_Appr_Step_Config_DT.NewRow();
                        new_DataRow["Act_ID"] = act_ID;
                        new_DataRow["Appr_ID"] = appr_ID;
                        new_DataRow["Appr_Step_ID"] = appr_Step_ID;
                        new_DataRow["Appr_Step_Act_ID"] = new_Appr_Step_Act_ID;
                        new_DataRow["Reg_Config_ID"] = reg_Config_ID;
                        new_DataRow["Description"] = "Desc";
                        new_DataRow["Acct"] = noneVal;
                        new_DataRow["Flow"] = noneVal;
                        new_DataRow["UD1"] = noneVal;
                        new_DataRow["UD2"] = noneVal;
                        new_DataRow["UD3"] = noneVal;
                        new_DataRow["UD4"] = noneVal;
                        new_DataRow["UD5"] = noneVal;
                        new_DataRow["UD6"] = noneVal;
                        new_DataRow["UD7"] = noneVal;
                        new_DataRow["UD8"] = noneVal;
                        new_DataRow["Create_Date"] = DateTime.Now;
                        new_DataRow["Create_User"] = si.UserName;
                        new_DataRow["Update_Date"] = DateTime.Now;
                        new_DataRow["Update_User"] = si.UserName;

                        FMM_Act_Appr_Step_Config_DT.Rows.Add(new_DataRow);

                        // Save Approval Step Activity Config
                        cmdBuilder.UpdateTableSimple(si, "FMM_Act_Appr_Step_Config", FMM_Act_Appr_Step_Config_DT, sqa, "Appr_Step_Act_ID");

                        // Save Cube Map entries
                        //SQA_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                        BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                    }
                    else if (runType == "Update" && act_ID > 0)
                    { // existing row

                        connection.Open();

                        // Fill DataTable for Appr_Step_Activity_Config
                        sql = @"SELECT * 
                            FROM FMM_Act_Appr_Step_Config
                            WHERE Appr_Step_Act_ID = @Appr_Step_Act_ID";

                        sqlparams = new SqlParameter[]
                        {
                        //kinda weird, but we're passing in Act_ID as the Approval Step Activity ID when in update runtype
                        new SqlParameter("@Appr_Step_Act_ID", SqlDbType.Int) { Value = act_ID }

                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        BRApi.ErrorLog.LogMessage(si, "mcm: " + FMM_Act_Appr_Step_Config_DT.Rows[0]["Reg_Config_ID"]);
                        // update existing row
                        FMM_Act_Appr_Step_Config_DT.Rows[0]["Reg_Config_ID"] = reg_Config_ID;
                        FMM_Act_Appr_Step_Config_DT.Rows[0]["Update_Date"] = DateTime.Now;
                        FMM_Act_Appr_Step_Config_DT.Rows[0]["Update_User"] = si.UserName;

                        // Save Approval Step Activity Config
                        cmdBuilder.UpdateTableSimple(si, "FMM_Act_Appr_Step_Config", FMM_Act_Appr_Step_Config_DT, sqa, "Appr_Step_Act_ID");
                    }
                }
                return save_Result;
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
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var cube_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_Entity_MFB = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Entity_MFB", "0");
                var WF_Channels = custom_SubstVars.XFGetValue("BL_FMM_WFChannels", "0");
                var new_Calc_Unit_ID = 0;
                var loop_times = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    connection.Open();

                    // Example: Get the max ID for the "FMM_Calc_Config" table
                    new_Calc_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_Calc_Unit_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    string sql = @"SELECT * 
                                   FROM FMM_Calc_Unit_Config
                                   WHERE Cube_ID = @Cube_ID";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Unit_Config_DT, sql, sqlparams);
                    var cubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, "Army_RMW_Consol");
                    var ent_DimID = cubeInfo.Cube.CubeDims.GetEntityDimId();
                    var cubeDim_Lookup = cubeInfo.Cube.CubeDims.CubeDimsDictionary.Values.ToLookup(dim => dim.DimId);
                    IEnumerable<CubeDim> ent_Dim = cubeDim_Lookup[ent_DimID];
                    var Calc_Unit_EntList = new List<MemberInfo>();
                    foreach (CubeDim dim in ent_Dim)
                    {
                        var dimTypeID = dim.CubeDimPk.DimTypeId;
                        var ent_DimPk = new DimPk(dimTypeID, ent_DimID);
                        //var EntDimPk = BRApi.Finance.Dim.GetDimPk(si, "DHP_ConsolEntities_Dim");
                        Calc_Unit_EntList = BRApi.Finance.Members.GetMembersUsingFilter(si, ent_DimPk, Calc_Unit_Entity_MFB, true);
                    }

                    // Loop through the Calc_Unit_EntList and add new rows to FMM_Calc_Unit_Config_DT
                    foreach (var entity in Calc_Unit_EntList)
                    {
                        var new_DataRow = FMM_Calc_Unit_Config_DT.NewRow();
                        if (loop_times == 0)
                        {
                            new_DataRow["Calc_Unit_ID"] = new_Calc_Unit_ID;
                        }
                        {
                            new_DataRow["Calc_Unit_ID"] = ++new_Calc_Unit_ID;
                        }
                        new_DataRow["Cube_ID"] = cube_ID;
                        new_DataRow["Entity_MFB"] = entity.Member.Name;
                        new_DataRow["WFChannel"] = WF_Channels;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["Create_Date"] = DateTime.Now;
                        new_DataRow["Create_User"] = si.UserName;
                        new_DataRow["Update_Date"] = DateTime.Now;
                        new_DataRow["Update_User"] = si.UserName;

                        FMM_Calc_Unit_Config_DT.Rows.Add(new_DataRow);
                        loop_times += 1;
                    }

                    cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Config", FMM_Calc_Unit_Config_DT, sqa, "Calc_Unit_ID");

                    return save_Result;
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
            var src_FMM_Dest_Cell_DT = new DataTable();
            var tgt_FMM_Dest_Cell_DT = new DataTable();
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
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                #endregion
                connection.Open();
                #region "Get FMM Data"
                // Call for get_FMM_Act_Config_Data
                get_FMM_Act_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Act_Config_DT, ref tgt_FMM_Act_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Step_Config_Data
                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Model_Grp_Assign_DT, ref tgt_FMM_Model_Grp_Assign_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Assign_Data
                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
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
                    foreach (DataRow ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Approvals(ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
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
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Cell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
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
                cmdBuilder.UpdateTableSimple(si, "FMM_Act_Config", tgt_FMM_Act_Config_DT, sqa, "Act_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", tgt_FMM_Unit_Config_DT, sqa, "Unit_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", tgt_FMM_Acct_Config_DT, sqa, "Acct_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", tgt_FMM_Reg_Config_DT, sqa, "Reg_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa, "Col_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Config", tgt_FMM_Appr_Config_DT, sqa, "Appr_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", tgt_FMM_Appr_Step_Config_DT, sqa, "Appr_Step_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Models", tgt_FMM_Models_DT, sqa, "Model_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", tgt_FMM_Calc_Config_DT, sqa, "Calc_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", tgt_FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", tgt_FMM_Src_Cell_DT, sqa, "Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grp_Assign", tgt_FMM_Model_Grp_Assign_DT, sqa, "Model_Grp_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Config", tgt_FMM_Calc_Unit_Config_DT, sqa, "Calc_Unit_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", tgt_FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grps", tgt_FMM_Model_Grps_DT, sqa, "Model_Grp_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Cube_Config", tgt_FMM_Cube_Config_DT, sqa, "Cube_ID");
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
            var src_FMM_Dest_Cell_DT = new DataTable();
            var tgt_FMM_Dest_Cell_DT = new DataTable();
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
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get FMM Data"
                // Call for get_FMM_Act_Config_Data
                get_FMM_Act_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Act_Config_DT, ref tgt_FMM_Act_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Appr_Step_Config_Data
                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Model_Grp_Assign_DT, ref tgt_FMM_Model_Grp_Assign_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Calc_Unit_Assign_Data
                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_gbl_get_datasets,
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
                    foreach (DataRow ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Approvals(ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
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
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                            {
                                Copy_Cell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
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

                cmdBuilder.UpdateTableSimple(si, "FMM_Act_Config", tgt_FMM_Act_Config_DT, sqa, "Act_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", tgt_FMM_Unit_Config_DT, sqa, "Unit_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", tgt_FMM_Acct_Config_DT, sqa, "Acct_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", tgt_FMM_Reg_Config_DT, sqa, "Reg_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa, "Col_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Config", tgt_FMM_Appr_Config_DT, sqa, "Appr_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", tgt_FMM_Appr_Step_Config_DT, sqa, "Appr_Step_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Models", tgt_FMM_Models_DT, sqa, "Model_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", tgt_FMM_Calc_Config_DT, sqa, "Calc_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", tgt_FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", tgt_FMM_Src_Cell_DT, sqa, "Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grp_Assign", tgt_FMM_Model_Grp_Assign_DT, sqa, "Model_Grp_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Config", tgt_FMM_Calc_Unit_Config_DT, sqa, "Calc_Unit_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", tgt_FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grps", tgt_FMM_Model_Grps_DT, sqa, "Model_Grp_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Cube_Config", tgt_FMM_Cube_Config_DT, sqa, "Cube_ID");
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
            gbl_Curr_FMM_Act_ID = tgt_Act_ID;
            gbl_Curr_FMM_Models_ID = tgt_Model_ID;
            BRApi.ErrorLog.LogMessage(si, "Hit Copy Calc: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Calc_Config_DT = new DataTable();
            var tgt_FMM_Calc_Config_DT = new DataTable();
            var src_FMM_Dest_Cell_DT = new DataTable();
            var tgt_FMM_Dest_Cell_DT = new DataTable();
            var src_FMM_Src_Cell_DT = new DataTable();
            var tgt_FMM_Src_Cell_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
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
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@Calc_ID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@Model_ID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
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
                    sql_gbl_get_datasets,
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
                    foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                    {
                        Copy_Cell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                    foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"Calc_ID = {curr_srcCalcID}"))
                    {
                        Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                }
                #endregion

                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", tgt_FMM_Calc_Config_DT, sqa, "Calc_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", tgt_FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", tgt_FMM_Src_Cell_DT, sqa, "Cell_ID");

            }
        }

        #region "Get Data Functions"
        private void get_FMM_Act_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Act_Config_DT, ref DataTable tgt_FMM_Act_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Act_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Act_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Act_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Act_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Act_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Config", "Act_ID");
        }

        private void get_FMM_Unit_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Unit_Config_DT, ref DataTable tgt_FMM_Unit_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Unit_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Unit_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Unit_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Unit_Config_DT, sql, tgt_sqlparams);
            gbl_FMM_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Unit_Config", "Unit_ID");
        }

        private void get_FMM_Acct_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Acct_Config_DT, ref DataTable tgt_FMM_Acct_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Acct_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Acct_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Acct_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Acct_Config_DT, sql, tgt_sqlparams);
            gbl_FMM_Acct_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Acct_Config", "Acct_ID");
        }

        private void get_FMM_Appr_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Appr_Config_DT, ref DataTable tgt_FMM_Appr_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Appr_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Appr_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Appr_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Appr_Config_DT, sql, tgt_sqlparams);
            gbl_FMM_Appr_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Config", "Appr_ID");
        }

        private void get_FMM_Appr_Step_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Appr_Step_Config_DT, ref DataTable tgt_FMM_Appr_Step_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Appr_Step_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Appr_Step_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Appr_Step_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Appr_Step_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Appr_Step_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "Appr_Step_ID");
        }

        private void get_FMM_Reg_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Reg_Config_DT, ref DataTable tgt_FMM_Reg_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Reg_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Reg_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Reg_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Reg_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Reg_Config_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Config", "Reg_Config_ID");
        }

        private void get_FMM_Col_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Col_Config_DT, ref DataTable tgt_FMM_Col_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Col_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Col_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Col_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Col_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Col_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
        }

        private void get_FMM_Models_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Models_DT, ref DataTable tgt_FMM_Models_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Models {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Models_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Models {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Models_DT, sql, tgt_sqlparams);

            gbl_FMM_Models_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "Model_ID");
        }

        private void get_FMM_Calc_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Calc_Config_DT, ref DataTable tgt_FMM_Calc_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Calc_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Calc_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Calc_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Calc_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Calc_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "Calc_ID");
        }

        private void get_FMM_Dest_Cell_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Dest_Cell_DT, ref DataTable tgt_FMM_Dest_Cell_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Dest_Cell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Dest_Cell_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Dest_Cell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Dest_Cell_DT, sql, tgt_sqlparams);

            gbl_FMM_Dest_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Dest_Cell", "Dest_Cell_ID");
        }

        private void get_FMM_Src_Cell_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Src_Cell_DT, ref DataTable tgt_FMM_Src_Cell_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Src_Cell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Src_Cell_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Src_Cell {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Src_Cell_DT, sql, tgt_sqlparams);

            gbl_FMM_Src_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Src_Cell", "Cell_ID");
        }

        private void get_FMM_Model_Grps_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Model_Grps_DT, ref DataTable tgt_FMM_Model_Grps_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Model_Grps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Model_Grps_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Model_Grps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Model_Grps_DT, sql, tgt_sqlparams);

            gbl_FMM_Model_Grps_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grps", "Model_Grp_ID");
        }

        private void get_FMM_Model_Grp_Assign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Model_Grp_Assign_DT, ref DataTable tgt_FMM_Model_Grp_Assign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Model_Grp_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Model_Grp_Assign_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Model_Grp_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Model_Grp_Assign_DT, sql, tgt_sqlparams);

            gbl_FMM_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grp_Assign", "Model_Grp_Assign_ID");
        }

        private void get_FMM_Calc_Unit_Config_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Calc_Unit_Config_DT, ref DataTable tgt_FMM_Calc_Unit_Config_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Calc_Unit_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Calc_Unit_Config_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Calc_Unit_Config {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Calc_Unit_Config_DT, sql, tgt_sqlparams);

            gbl_FMM_Calc_Unit_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");
        }

        private void get_FMM_Calc_Unit_Assign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_Calc_Unit_Assign_DT, ref DataTable tgt_FMM_Calc_Unit_Assign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_Calc_Unit_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_Calc_Unit_Assign_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_Calc_Unit_Assign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_Calc_Unit_Assign_DT, sql, tgt_sqlparams);

            gbl_FMM_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Assign", "Calc_Unit_Assign_ID");
        }
        #endregion
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
                gbl_FMM_Act_ID += 1;
                gbl_Curr_FMM_Act_ID = gbl_FMM_Act_ID;
                DataRow new_DestDataRow = tgt_FMM_Act_Config_DT.NewRow();

                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_FMM_Act_ID;
                new_DestDataRow["Name"] = src_FMM_Act_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Calc_Type"] = src_FMM_Act_Config_Row.Field<string>("Calc_Type") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as "Build"
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // Set the appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // Set the appropriate user context

                tgt_FMM_Act_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_FMM_Act_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Act_Config_Row["Name"].ToString() &&
                                           row["Calc_Type"].ToString() == src_FMM_Act_Config_Row["Calc_Type"].ToString());
                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Act_ID = existing_DataRow.Field<int>("Act_ID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // Set the appropriate user context
                }
            }
        }
        private void Copy_Units(DataRow src_FMM_Unit_Config_Row, ref DataTable tgt_FMM_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID units for duplicate Unit_Name
            bool isDuplicate = tgt_FMM_Unit_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Unit to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Unit_ID += 1;
                gbl_Curr_FMM_Unit_ID = gbl_FMM_Unit_ID;
                DataRow new_DestDataRow = tgt_FMM_Unit_Config_DT.NewRow();

                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Unit_ID"] = gbl_FMM_Unit_ID;
                new_DestDataRow["Name"] = src_FMM_Unit_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // Set appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_FMM_Unit_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_FMM_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Unit_ID = existing_DataRow.Field<int>("Unit_ID");
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Accts(DataRow src_FMM_Acct_Config_Row, ref DataTable tgt_FMM_Acct_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID accounts for duplicate Acct_Name
            bool isDuplicate = tgt_FMM_Acct_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Unit_ID"].ToString() == gbl_Curr_FMM_Unit_ID.ToString());

            // If not duplicate, add the new Account to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Acct_ID += 1;
                gbl_Curr_FMM_Acct_ID = gbl_FMM_Acct_ID;
                DataRow new_DestDataRow = tgt_FMM_Acct_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Unit_ID"] = gbl_Curr_FMM_Unit_ID;
                new_DestDataRow["Acct_ID"] = gbl_FMM_Acct_ID;
                new_DestDataRow["Name"] = src_FMM_Acct_Config_Row.Field<string>("Acct_Name") ?? string.Empty; // Handle nulls
                new_DestDataRow["Map_Logic"] = src_FMM_Acct_Config_Row.Field<string>("Map_Logic") ?? string.Empty; // Handle nulls
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Acct_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_FMM_Acct_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                           row["Unit_ID"].ToString() == gbl_Curr_FMM_Unit_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Acct_ID = existing_DataRow.Field<int>("Acct_ID");

                    // Update fields, handle nulls using Field<T>
                    existing_DataRow["Map_Logic"] = src_FMM_Acct_Config_Row.Field<string>("Map_Logic") ?? existing_DataRow.Field<string>("Map_Logic");
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Reg_Config(DataRow src_Reg_Config_Row, ref DataTable tgt_Reg_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID register config for duplicate Register_Name
            bool isDuplicate = tgt_Reg_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Register Config to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Reg_Config_ID += 1;
                gbl_Curr_FMM_Reg_Config_ID = gbl_FMM_Reg_Config_ID;
                DataRow new_DestDataRow = tgt_Reg_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Reg_Config_ID"] = gbl_FMM_Reg_Config_ID;
                new_DestDataRow["Name"] = src_Reg_Config_Row.Field<string>("Name") ?? string.Empty; // Handle nulls
                new_DestDataRow["Time_Phasing"] = src_Reg_Config_Row.Field<string>("Time_Phasing") ?? string.Empty; // Handle nulls
                new_DestDataRow["Start_Dt_Src"] = src_Reg_Config_Row.Field<string>("Start_Dt_Src") ?? string.Empty; // Handle nulls
                new_DestDataRow["End_Dt_Src"] = src_Reg_Config_Row.Field<string>("End_Dt_Src") ?? string.Empty; // Handle nulls
                new_DestDataRow["Appr_Config"] = src_Reg_Config_Row.Field<string>("Appr_Config") ?? string.Empty; // Handle nulls
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // Set appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_Reg_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Reg_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Reg_Config_ID = existing_DataRow.Field<int>("Reg_Config_ID");

                    // Update fields, handle nulls using Field<T>
                    existing_DataRow["Time_Phasing"] = src_Reg_Config_Row.Field<string>("Time_Phasing") ?? existing_DataRow.Field<string>("Time_Phasing");
                    existing_DataRow["Start_Dt_Src"] = src_Reg_Config_Row.Field<string>("Start_Dt_Src") ?? existing_DataRow.Field<string>("Start_Dt_Src");
                    existing_DataRow["End_Dt_Src"] = src_Reg_Config_Row.Field<string>("End_Dt_Src") ?? existing_DataRow.Field<string>("End_Dt_Src");
                    existing_DataRow["Appr_Config"] = src_Reg_Config_Row.Field<string>("Appr_Config") ?? existing_DataRow.Field<string>("Appr_Config");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Col_Config(DataRow src_Col_Config_Row, ref DataTable tgt_Col_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID column config for duplicate Col_Name
            bool isDuplicate = tgt_Col_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Reg_Config_ID"].ToString() == gbl_Curr_FMM_Reg_Config_ID.ToString());

            // If not duplicate, add the new Column Config to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Col_ID += 1;
                gbl_Curr_FMM_Col_ID = gbl_FMM_Col_ID;
                DataRow new_DestDataRow = tgt_Col_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Reg_Config_ID"] = gbl_Curr_FMM_Reg_Config_ID;
                new_DestDataRow["Col_ID"] = gbl_FMM_Col_ID;
                new_DestDataRow["Name"] = src_Col_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                new_DestDataRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                new_DestDataRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                new_DestDataRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                new_DestDataRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                new_DestDataRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                new_DestDataRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                new_DestDataRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Col_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Col_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                                    row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                    row["Reg_Config_ID"].ToString() == gbl_Curr_FMM_Reg_Config_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Col_ID = existing_DataRow.Field<int>("Col_ID");
                    existing_DataRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                    existing_DataRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                    existing_DataRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                    existing_DataRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                    existing_DataRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                    existing_DataRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                    existing_DataRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                    existing_DataRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Approval to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Appr_ID += 1;
                gbl_Curr_FMM_Appr_ID = gbl_FMM_Appr_ID;
                DataRow new_DestDataRow = tgt_FMM_Appr_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Appr_ID"] = gbl_FMM_Appr_ID;
                new_DestDataRow["Name"] = src_FMM_Appr_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Appr_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_FMM_Appr_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Appr_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Appr_ID = existing_DataRow.Field<int>("Appr_ID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Appr_ID"].ToString() == gbl_Curr_FMM_Appr_ID.ToString());

            // If not duplicate, add the new Approval Step to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Appr_Step_ID += 1;
                gbl_Curr_FMM_Appr_Step_ID = gbl_FMM_Appr_Step_ID;
                DataRow new_DestDataRow = tgt_FMM_Appr_Step_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_FMM_Act_ID;
                new_DestDataRow["Appr_ID"] = gbl_FMM_Appr_ID;
                new_DestDataRow["Appr_Step_ID"] = gbl_FMM_Appr_Step_ID;
                new_DestDataRow["Step_Num"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Step_Num") ?? 0;
                new_DestDataRow["WFProfile_Step"] = src_FMM_Appr_Step_Config_Row.Field<Guid?>("WFProfile_Step") ?? Guid.Empty;
                new_DestDataRow["User_Group"] = src_FMM_Appr_Step_Config_Row.Field<string>("User_Group") ?? string.Empty;
                new_DestDataRow["Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Logic") ?? string.Empty;
                new_DestDataRow["Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Item") ?? string.Empty;
                new_DestDataRow["Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Level") ?? 0;
                new_DestDataRow["Appr_Config"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Config") ?? 0;
                new_DestDataRow["Init_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Init_Status") ?? string.Empty;
                new_DestDataRow["Appr_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Status") ?? string.Empty;
                new_DestDataRow["Rej_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Rej_Status") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_FMM_Appr_Step_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_FMM_Appr_Step_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Step_Num"].ToString() == src_FMM_Appr_Step_Config_Row["Step_Num"].ToString() &&
                                           row["WFProfile_Step"].ToString() == src_FMM_Appr_Step_Config_Row["WFProfile_Step"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                           row["Appr_ID"].ToString() == gbl_Curr_FMM_Appr_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Appr_Step_ID = existing_DataRow.Field<int>("Appr_Step_ID");
                    existing_DataRow["User_Group"] = src_FMM_Appr_Step_Config_Row.Field<string>("User_Group") ?? string.Empty;
                    existing_DataRow["Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Logic") ?? string.Empty;
                    existing_DataRow["Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Item") ?? string.Empty;
                    existing_DataRow["Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Level") ?? 0;
                    existing_DataRow["Appr_Config"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Appr_Config") ?? 0;
                    existing_DataRow["Init_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Init_Status") ?? string.Empty;
                    existing_DataRow["Appr_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Appr_Status") ?? string.Empty;
                    existing_DataRow["Rej_Status"] = src_FMM_Appr_Step_Config_Row.Field<string>("Rej_Status") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Models(DataRow src_Model_Config_Row, ref DataTable tgt_Model_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target Cube_ID model config for duplicate Model_Name
            bool isDuplicate = tgt_Model_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Model Config to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Models_ID += 1;
                gbl_Curr_FMM_Models_ID = gbl_FMM_Models_ID;
                DataRow new_DestDataRow = tgt_Model_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Model_ID"] = gbl_FMM_Models_ID;
                new_DestDataRow["Name"] = src_Model_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Model_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Models_ID = existing_DataRow.Field<int>("Model_ID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calcs(DataRow src_Calc_Config_Row, ref DataTable tgt_Calc_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult, string runType)
        {
            var row_Status = "Build";
            // Check the target Cube_ID calculation config for duplicate Calc_Name and Calc_Type
            bool isDuplicate = tgt_Calc_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString());

            // If not duplicate, add the new Calculation Config to the target DataTable
            if (runType == "Model Calc Copy" || !isDuplicate)
            {
                gbl_FMM_Calc_ID += 1;
                gbl_Curr_FMM_Calc_ID = gbl_FMM_Calc_ID;

                DataRow new_DestDataRow = tgt_Calc_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Model_ID"] = gbl_Curr_FMM_Models_ID;
                new_DestDataRow["Calc_ID"] = gbl_FMM_Calc_ID;
                if (runType == "Model Calc Copy")
                {
                    // Get the name from the source row
                    string calcName = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;

                    // Append " - Copy" to the name
                    new_DestDataRow["Name"] = calcName + " - Copy";
                }
                else
                {
                    // If runType is not "Model Calc Copy", assign the name without modification
                    new_DestDataRow["Name"] = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;
                }
                new_DestDataRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                new_DestDataRow["Calc_Condition"] = src_Calc_Config_Row.Field<string>("Calc_Condition") ?? string.Empty;
                new_DestDataRow["Calc_Explanation"] = src_Calc_Config_Row.Field<string>("Calc_Explanation") ?? string.Empty;
                new_DestDataRow["Balanced_Buffer"] = src_Calc_Config_Row.Field<string>("Balanced_Buffer") ?? string.Empty;
                new_DestDataRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                new_DestDataRow["Unbal_Calc"] = src_Calc_Config_Row.Field<string>("Unbal_Calc") ?? string.Empty;
                new_DestDataRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                new_DestDataRow["Time_Phasing"] = src_Calc_Config_Row.Field<string>("Time_Phasing") ?? string.Empty;
                new_DestDataRow["Input_Frequency"] = src_Calc_Config_Row.Field<string>("Input_Frequency") ?? string.Empty;
                new_DestDataRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                new_DestDataRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                new_DestDataRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status;
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Calc_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                           row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Calc_ID = existing_DataRow.Field<int>("Calc_ID");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                    existing_DataRow["Calc_Condition"] = src_Calc_Config_Row.Field<string>("Calc_Condition") ?? string.Empty;
                    existing_DataRow["Calc_Explanation"] = src_Calc_Config_Row.Field<string>("Calc_Explanation") ?? string.Empty;
                    existing_DataRow["Balanced_Buffer"] = src_Calc_Config_Row.Field<string>("Balanced_Buffer") ?? string.Empty;
                    existing_DataRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                    existing_DataRow["Unbal_Calc"] = src_Calc_Config_Row.Field<string>("Unbal_Calc") ?? string.Empty;
                    existing_DataRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                    existing_DataRow["Time_Phasing"] = src_Calc_Config_Row.Field<string>("Time_Phasing") ?? string.Empty;
                    existing_DataRow["Input_Frequency"] = src_Calc_Config_Row.Field<string>("Input_Frequency") ?? string.Empty;
                    existing_DataRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                    existing_DataRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                    existing_DataRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Cell(DataRow src_Cell_Config_Row, ref DataTable tgt_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target Cube_ID destination cell config for duplicate Cell_Name and Cell_Type
            bool isDuplicate = tgt_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString() &&
                            row["Calc_ID"].ToString() == gbl_Curr_FMM_Calc_ID.ToString());

            // If not duplicate, add the new Destination Cell Config to the target DataTable
            if (!isDuplicate)
            {
                gbl_FMM_Dest_Cell_ID += 1;
                gbl_Curr_FMM_Dest_Cell_ID = gbl_FMM_Dest_Cell_ID;

                DataRow new_DestDataRow = tgt_Cell_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID; // Correct Act_ID assignment
                new_DestDataRow["Model_ID"] = gbl_Curr_FMM_Models_ID;
                new_DestDataRow["Calc_ID"] = gbl_Curr_FMM_Calc_ID;
                new_DestDataRow["Dest_Cell_ID"] = gbl_FMM_Dest_Cell_ID;

                // Handle nullable fields
                new_DestDataRow["Location"] = src_Cell_Config_Row.Field<string>("Location") ?? string.Empty;
                new_DestDataRow["Calc_Plan_Units"] = src_Cell_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                new_DestDataRow["Acct"] = src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                new_DestDataRow["View"] = src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                new_DestDataRow["Origin"] = src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                new_DestDataRow["IC"] = src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                new_DestDataRow["Flow"] = src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                new_DestDataRow["UD1"] = src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                new_DestDataRow["UD2"] = src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                new_DestDataRow["UD3"] = src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                new_DestDataRow["UD4"] = src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                new_DestDataRow["UD5"] = src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                new_DestDataRow["UD6"] = src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                new_DestDataRow["UD7"] = src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                new_DestDataRow["UD8"] = src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                new_DestDataRow["Time_Filter"] = src_Cell_Config_Row.Field<string>("Time_Filter") ?? string.Empty;
                new_DestDataRow["Acct_Filter"] = src_Cell_Config_Row.Field<string>("Acct_Filter") ?? string.Empty;
                new_DestDataRow["Origin_Filter"] = src_Cell_Config_Row.Field<string>("Origin_Filter") ?? string.Empty;
                new_DestDataRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                new_DestDataRow["Flow_Filter"] = src_Cell_Config_Row.Field<string>("Flow_Filter") ?? string.Empty;
                new_DestDataRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                new_DestDataRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                new_DestDataRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                new_DestDataRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                new_DestDataRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                new_DestDataRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                new_DestDataRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                new_DestDataRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                new_DestDataRow["Conditional_Filter"] = src_Cell_Config_Row.Field<string>("Conditional_Filter") ?? string.Empty;
                new_DestDataRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                new_DestDataRow["Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Buffer_Filter") ?? string.Empty;
                new_DestDataRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                new_DestDataRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;

                tgt_Cell_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                            row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString() &&
                                           row["Calc_ID"].ToString() == gbl_Curr_FMM_Calc_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Dest_Cell_ID = existing_DataRow.Field<int>("Dest_Cell_ID");
                    existing_DataRow["Location"] = src_Cell_Config_Row.Field<string>("Location") ?? string.Empty;
                    existing_DataRow["Calc_Plan_Units"] = src_Cell_Config_Row.Field<string>("Calc_Plan_Units") ?? string.Empty;
                    existing_DataRow["Acct"] = src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existing_DataRow["View"] = src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                    existing_DataRow["Origin"] = src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existing_DataRow["IC"] = src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                    existing_DataRow["Flow"] = src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existing_DataRow["UD1"] = src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existing_DataRow["UD2"] = src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existing_DataRow["UD3"] = src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existing_DataRow["UD4"] = src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existing_DataRow["UD5"] = src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existing_DataRow["UD6"] = src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existing_DataRow["UD7"] = src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existing_DataRow["UD8"] = src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existing_DataRow["Time_Filter"] = src_Cell_Config_Row.Field<string>("Time_Filter") ?? string.Empty;
                    existing_DataRow["Acct_Filter"] = src_Cell_Config_Row.Field<string>("Acct_Filter") ?? string.Empty;
                    existing_DataRow["Origin_Filter"] = src_Cell_Config_Row.Field<string>("Origin_Filter") ?? string.Empty;
                    existing_DataRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                    existing_DataRow["Flow_Filter"] = src_Cell_Config_Row.Field<string>("Flow_Filter") ?? string.Empty;
                    existing_DataRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                    existing_DataRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                    existing_DataRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                    existing_DataRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                    existing_DataRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                    existing_DataRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                    existing_DataRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                    existing_DataRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                    existing_DataRow["Conditional_Filter"] = src_Cell_Config_Row.Field<string>("Conditional_Filter") ?? string.Empty;
                    existing_DataRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                    existing_DataRow["Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Buffer_Filter") ?? string.Empty;
                    existing_DataRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                    existing_DataRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;
                }
            }
        }
        private void Copy_Src_Cell(DataRow src_Src_Cell_Config_Row, ref DataTable tgt_Src_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            gbl_FMM_Src_Cell_ID += 1;
            gbl_Curr_FMM_Src_Cell_ID = gbl_FMM_Src_Cell_ID;
            // Check the target Cube_ID source cell config for duplicate Src_Item and Src_Type
            bool isDuplicate = tgt_Src_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                            row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString() &&
                            row["Calc_ID"].ToString() == gbl_Curr_FMM_Calc_ID.ToString() &&
                            row["Cell_ID"].ToString() == gbl_Curr_FMM_Src_Cell_ID.ToString());

            // If not duplicate, add the new Source Cell Config to the target DataTable
            if (!isDuplicate)
            {
                DataRow new_DestDataRow = tgt_Src_Cell_Config_DT.NewRow();

                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = gbl_Curr_FMM_Act_ID;
                new_DestDataRow["Model_ID"] = gbl_Curr_FMM_Models_ID;
                new_DestDataRow["Calc_ID"] = gbl_Curr_FMM_Calc_ID;
                new_DestDataRow["Cell_ID"] = gbl_FMM_Src_Cell_ID;
                new_DestDataRow["Src_Order"] = src_Src_Cell_Config_Row.Field<int?>("Src_Order") ?? 0;
                new_DestDataRow["Src_Type"] = src_Src_Cell_Config_Row.Field<string>("Src_Type") ?? string.Empty;
                new_DestDataRow["Src_Item"] = src_Src_Cell_Config_Row.Field<string>("Src_Item") ?? string.Empty;
                new_DestDataRow["Open_Parens"] = src_Src_Cell_Config_Row.Field<string>("Open_Parens") ?? string.Empty;
                new_DestDataRow["Math_Operator"] = src_Src_Cell_Config_Row.Field<string>("Math_Operator") ?? string.Empty;
                new_DestDataRow["Entity"] = src_Src_Cell_Config_Row.Field<string>("Entity") ?? string.Empty;
                new_DestDataRow["Cons"] = src_Src_Cell_Config_Row.Field<string>("Cons") ?? string.Empty;
                new_DestDataRow["Scenario"] = src_Src_Cell_Config_Row.Field<string>("Scenario") ?? string.Empty;
                new_DestDataRow["Time"] = src_Src_Cell_Config_Row.Field<string>("Time") ?? string.Empty;
                new_DestDataRow["Origin"] = src_Src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                new_DestDataRow["IC"] = src_Src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                new_DestDataRow["View"] = src_Src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                new_DestDataRow["Src_Plan_Units"] = src_Src_Cell_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                new_DestDataRow["Acct"] = src_Src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                new_DestDataRow["Flow"] = src_Src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                new_DestDataRow["UD1"] = src_Src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                new_DestDataRow["UD2"] = src_Src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                new_DestDataRow["UD3"] = src_Src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                new_DestDataRow["UD4"] = src_Src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                new_DestDataRow["UD5"] = src_Src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                new_DestDataRow["UD6"] = src_Src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                new_DestDataRow["UD7"] = src_Src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                new_DestDataRow["UD8"] = src_Src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                new_DestDataRow["Close_Parens"] = src_Src_Cell_Config_Row.Field<string>("Close_Parens") ?? string.Empty;
                new_DestDataRow["Unbal_Src_Cell_Buffer"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer") ?? string.Empty;
                new_DestDataRow["Unbal_Origin_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                new_DestDataRow["Unbal_IC_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                new_DestDataRow["Unbal_Acct_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                new_DestDataRow["Unbal_Flow_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD1_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD2_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD3_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD4_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD5_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD6_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD7_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                new_DestDataRow["Unbal_UD8_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                new_DestDataRow["Unbal_Src_Cell_Buffer_Filter"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer_Filter") ?? string.Empty;
                new_DestDataRow["Dyn_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                new_DestDataRow["Override_Value"] = src_Src_Cell_Config_Row.Field<string>("Override_Value") ?? string.Empty;
                new_DestDataRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                new_DestDataRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                new_DestDataRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                new_DestDataRow["Map_Type"] = src_Src_Cell_Config_Row.Field<string>("Map_Type") ?? string.Empty;
                new_DestDataRow["Map_Source"] = src_Src_Cell_Config_Row.Field<string>("Map_Source") ?? string.Empty;
                new_DestDataRow["Map_Logic"] = src_Src_Cell_Config_Row.Field<string>("Map_Logic") ?? string.Empty;
                new_DestDataRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                new_DestDataRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                new_DestDataRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // Set appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // Set appropriate user context

                tgt_Src_Cell_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Src_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == gbl_Curr_FMM_Act_ID.ToString() &&
                                           row["Model_ID"].ToString() == gbl_Curr_FMM_Models_ID.ToString() &&
                                           row["Calc_ID"].ToString() == gbl_Curr_FMM_Calc_ID.ToString() &&
                                           row["Cell_ID"].ToString() == gbl_Curr_FMM_Src_Cell_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Src_Cell_ID = Convert.ToInt32(existing_DataRow["Cell_ID"].ToString());
                    // Update fields as necessary
                    existing_DataRow["Src_Order"] = src_Src_Cell_Config_Row.Field<int?>("Src_Order") ?? 0;
                    existing_DataRow["Src_Type"] = src_Src_Cell_Config_Row.Field<string>("Src_Type") ?? string.Empty;
                    existing_DataRow["Src_Item"] = src_Src_Cell_Config_Row.Field<string>("Src_Item") ?? string.Empty;
                    existing_DataRow["Open_Parens"] = src_Src_Cell_Config_Row.Field<string>("Open_Parens") ?? string.Empty;
                    existing_DataRow["Math_Operator"] = src_Src_Cell_Config_Row.Field<string>("Math_Operator") ?? string.Empty;
                    existing_DataRow["Entity"] = src_Src_Cell_Config_Row.Field<string>("Entity") ?? string.Empty;
                    existing_DataRow["Cons"] = src_Src_Cell_Config_Row.Field<string>("Cons") ?? string.Empty;
                    existing_DataRow["Scenario"] = src_Src_Cell_Config_Row.Field<string>("Scenario") ?? string.Empty;
                    existing_DataRow["Time"] = src_Src_Cell_Config_Row.Field<string>("Time") ?? string.Empty;
                    existing_DataRow["Origin"] = src_Src_Cell_Config_Row.Field<string>("Origin") ?? string.Empty;
                    existing_DataRow["IC"] = src_Src_Cell_Config_Row.Field<string>("IC") ?? string.Empty;
                    existing_DataRow["View"] = src_Src_Cell_Config_Row.Field<string>("View") ?? string.Empty;
                    existing_DataRow["Src_Plan_Units"] = src_Src_Cell_Config_Row.Field<string>("Src_Plan_Units") ?? string.Empty;
                    existing_DataRow["Acct"] = src_Src_Cell_Config_Row.Field<string>("Acct") ?? string.Empty;
                    existing_DataRow["Flow"] = src_Src_Cell_Config_Row.Field<string>("Flow") ?? string.Empty;
                    existing_DataRow["UD1"] = src_Src_Cell_Config_Row.Field<string>("UD1") ?? string.Empty;
                    existing_DataRow["UD2"] = src_Src_Cell_Config_Row.Field<string>("UD2") ?? string.Empty;
                    existing_DataRow["UD3"] = src_Src_Cell_Config_Row.Field<string>("UD3") ?? string.Empty;
                    existing_DataRow["UD4"] = src_Src_Cell_Config_Row.Field<string>("UD4") ?? string.Empty;
                    existing_DataRow["UD5"] = src_Src_Cell_Config_Row.Field<string>("UD5") ?? string.Empty;
                    existing_DataRow["UD6"] = src_Src_Cell_Config_Row.Field<string>("UD6") ?? string.Empty;
                    existing_DataRow["UD7"] = src_Src_Cell_Config_Row.Field<string>("UD7") ?? string.Empty;
                    existing_DataRow["UD8"] = src_Src_Cell_Config_Row.Field<string>("UD8") ?? string.Empty;
                    existing_DataRow["Close_Parens"] = src_Src_Cell_Config_Row.Field<string>("Close_Parens") ?? string.Empty;
                    existing_DataRow["Unbal_Src_Cell_Buffer"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer") ?? string.Empty;
                    existing_DataRow["Unbal_Origin_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Origin_Override") ?? string.Empty;
                    existing_DataRow["Unbal_IC_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_IC_Override") ?? string.Empty;
                    existing_DataRow["Unbal_Acct_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Acct_Override") ?? string.Empty;
                    existing_DataRow["Unbal_Flow_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Flow_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD1_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD1_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD2_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD2_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD3_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD3_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD4_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD4_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD5_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD5_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD6_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD6_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD7_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD7_Override") ?? string.Empty;
                    existing_DataRow["Unbal_UD8_Override"] = src_Src_Cell_Config_Row.Field<string>("Unbal_UD8_Override") ?? string.Empty;
                    existing_DataRow["Unbal_Src_Cell_Buffer_Filter"] = src_Src_Cell_Config_Row.Field<string>("Unbal_Src_Cell_Buffer_Filter") ?? string.Empty;
                    existing_DataRow["Dyn_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                    existing_DataRow["Override_Value"] = src_Src_Cell_Config_Row.Field<string>("Override_Value") ?? string.Empty;
                    existing_DataRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                    existing_DataRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                    existing_DataRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                    existing_DataRow["Map_Type"] = src_Src_Cell_Config_Row.Field<string>("Map_Type") ?? string.Empty;
                    existing_DataRow["Map_Source"] = src_Src_Cell_Config_Row.Field<string>("Map_Source") ?? string.Empty;
                    existing_DataRow["Map_Logic"] = src_Src_Cell_Config_Row.Field<string>("Map_Logic") ?? string.Empty;
                    existing_DataRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                    existing_DataRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                    existing_DataRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // Set appropriate user context
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
                gbl_FMM_Model_Grps_ID += 1;
                gbl_Curr_FMM_Model_Grps_ID = gbl_FMM_Model_Grps_ID;
                DataRow new_DestDataRow = tgt_Model_Grp_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = targetCubeID;
                new_DestDataRow["Model_Grp_Config_ID"] = gbl_FMM_Model_Grps_ID;
                new_DestDataRow["Group_Name"] = src_Model_Grp_Config_Row["Group_Name"].ToString();
                new_DestDataRow["Group_Type"] = src_Model_Grp_Config_Row["Group_Type"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Model_Grp_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Group_Name"].ToString() == src_Model_Grp_Config_Row["Group_Name"].ToString() &&
                                           row["Group_Type"].ToString() == src_Model_Grp_Config_Row["Group_Type"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Model_Grps_ID = Convert.ToInt32(existing_DataRow["Model_Grp_Config_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
                gbl_FMM_Model_Grp_Assign_ID += 1;
                gbl_Curr_FMM_Model_Grp_Assign_ID = gbl_FMM_Model_Grp_Assign_ID;
                DataRow new_DestDataRow = tgt_Model_Grp_Assign_Model_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = targetCubeID;
                new_DestDataRow["Model_Grp_Assign_ID"] = gbl_FMM_Model_Grp_Assign_ID;
                new_DestDataRow["Group_Name"] = src_Model_Grp_Assign_Model_Row["Group_Name"].ToString();
                new_DestDataRow["Model_Name"] = src_Model_Grp_Assign_Model_Row["Model_Name"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Assign_Model_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Group_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Group_Name"].ToString() &&
                                           row["Model_Name"].ToString() == src_Model_Grp_Assign_Model_Row["Model_Name"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Model_Grp_Assign_ID = Convert.ToInt32(existing_DataRow["Model_Grp_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Assign_Model_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
                gbl_FMM_Calc_Unit_ID += 1;
                gbl_Curr_FMM_Calc_Unit_ID = gbl_FMM_Calc_Unit_ID;
                DataRow new_DestDataRow = tgt_Calc_Unit_Config_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = targetCubeID;
                new_DestDataRow["Calc_Unit_ID"] = gbl_FMM_Calc_Unit_ID;
                new_DestDataRow["DU_Name"] = src_Calc_Unit_Config_Row["DU_Name"].ToString();
                new_DestDataRow["DU_Type"] = src_Calc_Unit_Config_Row["DU_Type"].ToString();
                new_DestDataRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Unit_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Calc_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Config_Row["DU_Name"].ToString() &&
                                           row["DU_Type"].ToString() == src_Calc_Unit_Config_Row["DU_Type"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Calc_Unit_ID = Convert.ToInt32(existing_DataRow["Calc_Unit_ID"].ToString());
                    existing_DataRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Calc_Unit_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
                gbl_FMM_Calc_Unit_Assign_ID += 1;
                gbl_Curr_FMM_Calc_Unit_Assign_ID = gbl_FMM_Calc_Unit_Assign_ID;
                DataRow new_DestDataRow = tgt_Calc_Unit_Assign_DT.NewRow();
                new_DestDataRow["Cube_ID"] = targetCubeID;
                new_DestDataRow["Act_ID"] = targetCubeID;
                new_DestDataRow["Calc_Unit_Assign_ID"] = gbl_FMM_Calc_Unit_Assign_ID;
                new_DestDataRow["DU_Name"] = src_Calc_Unit_Assign_Row["DU_Name"].ToString();
                new_DestDataRow["Model_Grp_Name"] = src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString();
                new_DestDataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["Create_Date"] = DateTime.Now;
                new_DestDataRow["Create_User"] = si.UserName; // or appropriate user context
                new_DestDataRow["Update_Date"] = DateTime.Now;
                new_DestDataRow["Update_User"] = si.UserName; // or appropriate user context

                tgt_Calc_Unit_Assign_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existing_DataRow = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                                           row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_FMM_Calc_Unit_Assign_ID = Convert.ToInt32(existing_DataRow["Calc_Unit_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Calc_Unit_Assign_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["Update_Date"] = DateTime.Now;
                    existing_DataRow["Update_User"] = si.UserName; // or appropriate user context
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
        private void Duplicate_Model_List(int cube_ID, int act_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result)
        {
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Models
                                        WHERE Cube_ID = @Cube_ID
                                        AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID},
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Models_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Model_List_Dict and handle errors
                            foreach (DataRow cube_Model_Row in FMM_Models_DT.Rows)
                            {
                                string ModelKey = cube_ID + "|" + act_ID + "|" + (int)cube_Model_Row["Model_ID"];
                                gbl_Models_Dict.Add(ModelKey, (string)cube_Model_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Model_Grp_List(int cube_ID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result)
        {
            try
            {
                switch (dup_Process_Step)
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
                            var sql = @"SELECT *
                                        FROM FMM_Model_Grps
                                        WHERE Cube_ID = @Cube_ID";
                            // Create an array of SqlParameter objects
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                SQL_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, FMM_Model_Grps_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for Cube_ID {cube_ID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate GBL_Model_Grp_List_Dict and handle errors
                            foreach (DataRow FMM_Model_Grp_Row in FMM_Model_Grps_DT.Rows)
                            {
                                var model_GroupKey = cube_ID + "|" + (int)FMM_Model_Grp_Row["Model_Grp_ID"];
                                gbl_Model_Grps_Dict.Add(model_GroupKey, (string)FMM_Model_Grp_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the save_Result
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }

        #endregion
        #region "Duplicate Approval Steps"

        #endregion
        #endregion
    }
}
