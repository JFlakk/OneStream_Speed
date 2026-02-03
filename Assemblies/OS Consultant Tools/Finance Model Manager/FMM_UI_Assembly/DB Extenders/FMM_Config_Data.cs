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
        public string gbl_BalCalc { get; set; } = string.Empty;
        public string gbl_Bal_Buffer_Calc { get; set; } = string.Empty;
        public string gbl_UnbalBuffer_Calc { get; set; } = string.Empty;
        public string gbl_Unbal_Calc { get; set; } = string.Empty;
        public string gbl_Table_Calc_Logic { get; set; } = string.Empty;
        public int gbl_Table_SrcCell { get; set; }
        public int gbl_Src_Cell_Cnt { get; set; }
        public string gbl_Model_Type { get; set; } = "Cube";
        public Dictionary<int, string> gbl_SrcCell_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_Unbal_Calc_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, string> gbl_SrcCell_Drill_Dict { get; set; } = new Dictionary<int, string>();
        public Dictionary<string, string> gbl_ActConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_CalcUnitConfig_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Unit_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Acct_Config_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Appr_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Appr_Step_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Register_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Col_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_Calc_Dict { get; set; } = new Dictionary<string, string>();
        public bool gbl_Duplicate_ActConfig { get; set; } = false;
        public bool gbl_Duplicate_CalcUnitConfig { get; set; } = false;
        public bool gbl_Duplicate_Unit_Config { get; set; } = false;
        public bool gbl_Duplicate_AcctConfig { get; set; } = false;
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
        public int gbl_ActID { get; set; }
        public int gbl_Curr_ActID { get; set; }
        public int gbl_UnitID { get; set; }
        public int gbl_Curr_UnitID { get; set; }
        public int gbl_AcctID { get; set; }
        public int gbl_Curr_AcctID { get; set; }
        public int gbl_ApprID { get; set; }
        public int gbl_Curr_ApprID { get; set; }
        public int gbl_ApprStepID { get; set; }
        public int gbl_Curr_ApprStepID { get; set; }
        public int gbl_RegConfigID { get; set; }
        public int gbl_Curr_RegConfigID { get; set; }
        public int gbl_Col_ID { get; set; }
        public int gbl_Curr_Col_ID { get; set; }
        public int gbl_ModelID { get; set; }
        public int gbl_Curr_ModelID { get; set; }
        public int gbl_CalcID { get; set; }
        public int gbl_Curr_CalcID { get; set; }
        public int gbl_Dest_Cell_ID { get; set; }
        public int gbl_Curr_Dest_Cell_ID { get; set; }
        public int gbl_Src_Cell_ID { get; set; }
        public int gbl_Curr_Src_Cell_ID { get; set; }
        public int gbl_ModelGrps_ID { get; set; }
        public int gbl_Curr_ModelGrps_ID { get; set; }
        public int gbl_Model_Grp_Assign_ID { get; set; }
        public int gbl_Curr_Model_Grp_Assign_ID { get; set; }
        public int gbl_CalcUnitID { get; set; }
        public int gbl_Curr_CalcUnitID { get; set; }
        public int gbl_Calc_Unit_Assign_ID { get; set; }
        public int gbl_Curr_Calc_Unit_Assign_ID { get; set; }
        public Dictionary<string, string> gbl_Models_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> gbl_ModelGrps_Dict { get; set; } = new Dictionary<string, string>();
        public bool gbl_Duplicate_Models { get; set; } = false;
        public bool gbl_Duplicate_ModelGrps { get; set; } = false;

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
                            case var fn when fn.XFEqualsIgnoreCase("Save_ActConfig"):
                                save_Result = Save_ActConfig();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_CalcUnitConfig"):
                                save_Result = Save_CalcUnitConfig();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_UnitConfig"):
                                save_Result = Save_UnitConfig();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_Acct_Config"):
                                save_Result = Save_Acct_Config();
                                return save_Result;

                            case var fn when fn.XFEqualsIgnoreCase("Save_ApprConfig"):
                                save_Result = Save_ApprConfig();
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
                                    Evaluate_Calc_Config_Setup(gbl_CalcID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("save_Dest_Cell_Rows"):
                                gbl_Model_Type = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type");
                                changed_Result = Save_Dest_Cell_Rows();
                                if (gbl_Model_Type == "Cube")
                                {
                                    Evaluate_Calc_Config_Setup(gbl_CalcID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Src_Cell_Rows"):
                                gbl_Model_Type = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("DL_FMM_Calc_Type");
                                changed_Result = Save_Src_Cell_Rows();
                                if (gbl_Model_Type == "Cube")
                                {
                                    Evaluate_Calc_Config_Setup(gbl_CalcID);
                                }
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_SaveAdd"):
                                changed_Result = Save_CubeConfig("Add");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("CubeConfig_SaveUpdate"):
                                changed_Result = Save_CubeConfig("Update");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_New_Model"):
                                changed_Result = SaveModel("New");
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Save_Model_Updates"):
                                changed_Result = SaveModel("Update");
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
                            case var fn when fn.XFEqualsIgnoreCase("Process_Copy_ActConfig"):
                                Process_Copy_ActConfig(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Model_Copy"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Process_Calc_Copy"):
                                Process_Calc_Copy(ref changed_Result);
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Copy_Model_Grp_Config"):
                                changed_Result = Process_Bulk_Calc_Unit();
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Select_Add_FMM_CalcID"):
                                return changed_Result;
                            case var fn when fn.XFEqualsIgnoreCase("Select_FMM_CalcID"):
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




        private XFSelectionChangedTaskResult Save_Calc_Config_Rows()
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var save_Task_Info = args.SelectionChangedTaskInfo;

                var createNewDestCell = false;
                var CubeID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                var ActID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                var ModelID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ModelID", "0").XFConvertToInt();
                var calcListVal = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CalcList", "0").XFConvertToInt();
                var saveType = "New";
                var CalcID = save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();

                    var FMM_Calc_Config_DT = new DataTable();
                    var FMM_Dest_Cell_DT = new DataTable();
                    Duplicate_Calc_Config(CubeID, ActID, ModelID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_Calc_Config 
                                WHERE CubeID = @CubeID 
                                AND ActID = @ActID 
                                AND ModelID = @ModelID";

                    var sqlparams = new[]
                    {
                    new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                    new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                    new SqlParameter("@ModelID", SqlDbType.Int) { Value = ModelID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);
                    FMM_Calc_Config_DT.PrimaryKey = new DataColumn[] { FMM_Calc_Config_DT.Columns["CalcID"]! };

                    if (saveType == "New")
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);

                        CalcID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "CalcID");
                        gbl_CalcID = CalcID;

                        var new_config_Row = FMM_Calc_Config_DT.NewRow();

                        new_config_Row["CalcID"] = CalcID;
                        new_config_Row["Status"] = "Build";
                        new_config_Row["CreateDate"] = DateTime.Now;
                        new_config_Row["CreateUser"] = si.UserName;
                        new_config_Row["UpdateDate"] = DateTime.Now;
                        new_config_Row["UpdateUser"] = si.UserName;

                        FMM_Calc_Config_DT.Rows.Add(new_config_Row);
                        Duplicate_Calc_Config(CubeID, ActID, ModelID, "Update Row", ref save_Result, "Insert");

                        var dest_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Dest_Cell", "Dest_Cell_ID");

                        sql = @"SELECT * 
                            FROM FMM_Dest_Cell 
                            WHERE CalcID = @CalcID 
                            AND Dest_Cell_ID = @Dest_Cell_ID";

                        sqlparams = new[]
                        {
                            new SqlParameter("@CalcID", SqlDbType.Int) { Value = CalcID },
                            new SqlParameter("@Dest_Cell_ID", SqlDbType.Int) { Value = dest_Cell_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Dest_Cell_DT, sql, sqlparams);

                        var new_Row = FMM_Dest_Cell_DT.NewRow();
                        new_Row["CubeID"] = CubeID;
                        new_Row["ActID"] = ActID;
                        new_Row["ModelID"] = ModelID;
                        new_Row["CalcID"] = CalcID;
                        new_Row["Dest_Cell_ID"] = dest_Cell_ID;

                        FMM_Dest_Cell_DT.Rows.Add(new_Row);
                    }
                    else
                    {
                        CalcID = CalcID == 0
                            ? save_Task_Info.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_CalcList", "0").XFConvertToInt()
                            : CalcID;
                        var rowsToUpdate = FMM_Calc_Config_DT.Select($"CalcID = {CalcID}");
                        if (rowsToUpdate.Length > 0)
                        {
                            var rowToUpdate = rowsToUpdate[0];
                            gbl_CalcID = rowToUpdate["CalcID"] != DBNull.Value ? Convert.ToInt32(rowToUpdate["CalcID"]) : 0;

                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;
                            Duplicate_Calc_Config(CubeID, ActID, ModelID, "Update Row", ref save_Result, "Update");
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
                        var rowToMap = FMM_Calc_Config_DT.Rows.Find(CalcID);
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
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", FMM_Calc_Config_DT, sqa, "CalcID");

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

                gbl_CalcID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcID", "0").XFConvertToInt();
                var CubeID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                var ActID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                var ModelID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ModelID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmDestCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_Dest_Cell 
                    WHERE CalcID = @CalcID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID }
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

                    destRow["UpdateDate"] = DateTime.Now;
                    destRow["UpdateUser"] = si.UserName;

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

                gbl_CalcID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CalcID", "0").XFConvertToInt();
                var CubeID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                var ActID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                var ModelID = saveTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_ModelID", "0").XFConvertToInt();

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var fmmSrcCellDt = new DataTable();

                    var sql = @"SELECT * 
                    FROM FMM_Src_Cell 
                    WHERE CalcID = @CalcID";
                    var sqlparams = new[]
                    {
                new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID }
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
                        row["UpdateDate"] = DateTime.Now;
                        row["UpdateUser"] = si.UserName;
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

        private XFSqlTableEditorSaveDataTaskResult Save_ActConfig()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var ActID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ActConfig_DT = new DataTable();
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    Duplicate_ActConfig(CubeID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_ActConfig
                                WHERE CubeID = @CubeID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_ActConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            {
                                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                                ActID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ActConfig", "ActID");

                                var new_DataRow = FMM_ActConfig_DT.NewRow();
                                new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                                new_DataRow["ActID"] = ActID;
                                new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                                new_DataRow["CalcType"] = (string)xfRow.ModifiedDataRow.Items["CalcType"];
                                new_DataRow["Status"] = "Build";
                                new_DataRow["CreateDate"] = DateTime.Now;
                                new_DataRow["CreateUser"] = si.UserName;
                                new_DataRow["UpdateDate"] = DateTime.Now;
                                new_DataRow["UpdateUser"] = si.UserName;
                                FMM_ActConfig_DT.Rows.Add(new_DataRow);
                                Duplicate_ActConfig(CubeID, "Update Row", ref save_Result, "Insert", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_ActConfig_DT.Select($"ActID = {(int)xfRow.ModifiedDataRow["ActID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_ActConfig(CubeID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_ActConfig_DT.Select($"ActID = {(int)xfRow.OriginalDataRow["ActID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_ActConfig(CubeID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

                    var dup_Activity_Config = gbl_ActConfig_Dict
                                             .GroupBy(x => x.Value)
                                             .Where(g => g.Count() > 1)
                                             .Select(g => g.Key)
                                             .ToList();

                    gbl_Duplicate_ActConfig = dup_Activity_Config.Count > 0;

                    if (gbl_Duplicate_ActConfig)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Activity Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        cmdBuilder.UpdateTableSimple(si, "FMM_ActConfig", FMM_ActConfig_DT, sqa, "ActID");
                    }
                }

                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_CalcUnitConfig()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var CalcUnitID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_CalcUnitConfig_DT = new DataTable();
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    Duplicate_CalcUnitConfig(CubeID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_CalcUnitConfig
                                WHERE CubeID = @CubeID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcUnitConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            CalcUnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcUnitConfig", "CalcUnitID");

                            var new_DataRow = FMM_CalcUnitConfig_DT.NewRow();
                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["CalcUnitID"] = CalcUnitID;
                            new_DataRow["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                            new_DataRow["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_CalcUnitConfig_DT.Rows.Add(new_DataRow);
                            Duplicate_CalcUnitConfig(CubeID, "Update Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_CalcUnitConfig_DT.Select($"CalcUnitID = {(int)xfRow.ModifiedDataRow["CalcUnitID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Entity_MFB"] = (string)xfRow.ModifiedDataRow.Items["Entity_MFB"];
                                rowToUpdate["WFChannel"] = (string)xfRow.ModifiedDataRow.Items["WFChannel"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_CalcUnitConfig(CubeID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_CalcUnitConfig_DT.Select($"CalcUnitID = {(int)xfRow.OriginalDataRow["CalcUnitID"]}");

                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    Duplicate_CalcUnitConfig(CubeID, "Update Row", ref save_Result, "Delete", xfRow);
                                    row.Delete();
                                }
                            }
                        }
                    }
                    var dup_CalcUnitConfig = gbl_CalcUnitConfig_Dict
                                                 .GroupBy(x => x.Value)
                                                 .Where(g => g.Count() > 1)
                                                 .Select(g => g.Key)
                                                 .ToList();

                    gbl_Duplicate_CalcUnitConfig = dup_CalcUnitConfig.Count > 0;

                    if (gbl_Duplicate_CalcUnitConfig)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate WF DU Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        cmdBuilder.UpdateTableSimple(si, "FMM_CalcUnitConfig", FMM_CalcUnitConfig_DT, sqa, "CalcUnitID");
                    }
                }

                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }



        private XFSqlTableEditorSaveDataTaskResult Save_UnitConfig()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var UnitID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Unit_Config_DT = new DataTable();
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    var ActID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                    Duplicate_Unit_Config(CubeID, ActID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_Unit_Config
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Unit_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            UnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Unit_Config", "UnitID");
                            var new_DataRow = FMM_Unit_Config_DT.NewRow();
                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["ActID"] = (int)xfRow.ModifiedDataRow["ActID"];
                            new_DataRow["UnitID"] = UnitID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Status"] = (string)xfRow.ModifiedDataRow.Items["Status"];
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_Unit_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Unit_Config(CubeID, ActID, "Update Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Unit_Config_DT.Select($"UnitID = {(int)xfRow.ModifiedDataRow["UnitID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_Unit_Config(CubeID, ActID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Unit_Config_DT.Select($"UnitID = {(int)xfRow.OriginalDataRow["UnitID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Unit_Config(CubeID, ActID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
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
                        cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", FMM_Unit_Config_DT, sqa, "UnitID");
                    }
                }

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

                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;

                var AcctID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Acct_Config_DT = new DataTable();
                    var CubeID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    var ActID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                    var UnitID = args.SqlTableEditorSaveDataTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_UnitID", "0").XFConvertToInt();
                    Duplicate_Acct_Config(CubeID, ActID, UnitID, "Initiate", ref save_Result);
                    var sql = @"SELECT * 
                                FROM FMM_Acct_Config
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Acct_Config_DT, sql, sqlparams);


                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);

                            AcctID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Acct_Config", "AcctID");

                            var new_DataRow = FMM_Acct_Config_DT.NewRow();
                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["ActID"] = (int)xfRow.ModifiedDataRow["ActID"];
                            new_DataRow["UnitID"] = (int)xfRow.ModifiedDataRow["UnitID"];
                            new_DataRow["AcctID"] = AcctID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["MapReq"] = (bool)xfRow.ModifiedDataRow.Items["MapReq"];
                            new_DataRow["MapType"] = (string)xfRow.ModifiedDataRow.Items["MapType"];
                            new_DataRow["MapLoc"] = (string)xfRow.ModifiedDataRow.Items["MapLoc"];
                            new_DataRow["MapLogic"] = (string)xfRow.ModifiedDataRow.Items["MapLogic"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_Acct_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Acct_Config(CubeID, ActID, UnitID, "Update Row", ref save_Result, "Insert", xfRow);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {

                            var rowsToUpdate = FMM_Acct_Config_DT.Select($"AcctID = {(int)xfRow.ModifiedDataRow["AcctID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["MapReq"] = (bool)xfRow.ModifiedDataRow["MapReq"];
                                rowToUpdate["MapType"] = (string)xfRow.ModifiedDataRow.Items["MapType"];
                                rowToUpdate["MapLoc"] = (string)xfRow.ModifiedDataRow.Items["MapLoc"];
                                rowToUpdate["MapLogic"] = (string)xfRow.ModifiedDataRow["MapLogic"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_Acct_Config(CubeID, ActID, UnitID, "Update Row", ref save_Result, "Update", xfRow);

                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Acct_Config_DT.Select($"AcctID = {(int)xfRow.OriginalDataRow["AcctID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Acct_Config(CubeID, ActID, UnitID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }

                        }
                    }

                    var dup_Acct_Config = gbl_Acct_Config_Dict
                                         .GroupBy(x => x.Value)
                                         .Where(g => g.Count() > 1)
                                         .Select(g => g.Key)
                                         .ToList();

                    gbl_Duplicate_AcctConfig = dup_Acct_Config.Count > 0;

                    if (gbl_Duplicate_AcctConfig)
                    {
                        save_Result.IsOK = false;
                        save_Result.ShowMessageBox = true;
                        save_Result.Message += "Duplicate Acct Config entries found during the operation.";
                    }
                    else
                    {
                        save_Result.IsOK = true;
                        save_Result.ShowMessageBox = false;
                        cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", FMM_Acct_Config_DT, sqa, "AcctID");
                    }
                }

                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }



        private XFSqlTableEditorSaveDataTaskResult Save_Reg_Config()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var new_Reg_List = new List<int>();
                var RegConfigID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Reg_Config_DT = new DataTable();
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    var ActID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                    Duplicate_Reg_Config(CubeID, ActID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_Reg_Config
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Reg_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            RegConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Config", "RegConfigID");
                            new_Reg_List.Add(RegConfigID);
                            var new_DataRow = FMM_Reg_Config_DT.NewRow();
                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["ActID"] = (int)xfRow.ModifiedDataRow["ActID"];
                            new_DataRow["RegConfigID"] = RegConfigID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["TimePhase"] = (string)xfRow.ModifiedDataRow.Items["TimePhase"];
                            new_DataRow["TimePhaseDriver"] = (string)xfRow.ModifiedDataRow.Items["TimePhaseDriver"];
                            new_DataRow["ManualInputPlanUnits"] = (string)xfRow.ModifiedDataRow.Items["ManualInputPlanUnits"];
                            new_DataRow["StartEndDtSrcObj"] = (string)xfRow.ModifiedDataRow.Items["StartEndDtSrcObj"];
                            new_DataRow["StartDtSrc"] = (string)xfRow.ModifiedDataRow.Items["StartDtSrc"];
                            new_DataRow["EndDtSrc"] = (string)xfRow.ModifiedDataRow.Items["EndDtSrc"];
                            new_DataRow["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_Reg_Config_DT.Rows.Add(new_DataRow);
                            Duplicate_Reg_Config(CubeID, ActID, "Update Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Reg_Config_DT.Select($"RegConfigID = {(int)xfRow.ModifiedDataRow["RegConfigID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["TimePhase"] = (string)xfRow.ModifiedDataRow.Items["TimePhase"];
                                rowToUpdate["TimePhaseDriver"] = (string)xfRow.ModifiedDataRow.Items["TimePhaseDriver"];
                                rowToUpdate["ManualInputPlanUnits"] = (string)xfRow.ModifiedDataRow.Items["ManualInputPlanUnits"];
                                rowToUpdate["StartEndDtSrcObj"] = (string)xfRow.ModifiedDataRow.Items["StartEndDtSrcObj"];
                                rowToUpdate["StartDtSrc"] = (string)xfRow.ModifiedDataRow.Items["StartDtSrc"];
                                rowToUpdate["EndDtSrc"] = (string)xfRow.ModifiedDataRow.Items["EndDtSrc"];
                                rowToUpdate["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_Reg_Config(CubeID, ActID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Reg_Config_DT.Select($"RegConfigID = {(int)xfRow.OriginalDataRow["RegConfigID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Reg_Config(CubeID, ActID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }

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
                        cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", FMM_Reg_Config_DT, sqa, "RegConfigID");
                    }
                    foreach (var newRegConfigID in new_Reg_List)
                    {
                        Insert_Col_Default_Rows(CubeID, ActID, newRegConfigID);
                    }
                }

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
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    var ActID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_ActID", "0").XFConvertToInt();
                    var RegConfigID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_RegConfigID", "0").XFConvertToInt();
                    Duplicate_Col_Config(CubeID, ActID, RegConfigID, "Initiate", ref save_Result);

                    var sql = @"SELECT * 
                                FROM FMM_Col_Config
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID
                                AND RegConfigID = @RegConfigID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                        new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = RegConfigID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
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
                                if (!(bool)xfRow.ModifiedDataRow["InUse"])
                                {
                                    rowToUpdate["Order"] = 99;
                                }
                                else
                                {
                                    rowToUpdate["Order"] = (int)xfRow.ModifiedDataRow["Order"];
                                }
                                rowToUpdate["Default"] = (string)xfRow.ModifiedDataRow["Default"];
                                rowToUpdate["Param"] = (string)xfRow.ModifiedDataRow["Param"];
                                rowToUpdate["Format"] = (string)xfRow.ModifiedDataRow["Format"];
                                rowToUpdate["FilterParam"] = (string)xfRow.ModifiedDataRow["FilterParam"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_Col_Config(CubeID, ActID, RegConfigID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                    }

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
                        cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa, "Col_ID");
                    }
                }

                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSqlTableEditorSaveDataTaskResult Save_ApprConfig()
        {
            try
            {
                var save_Result = new XFSqlTableEditorSaveDataTaskResult();
                var save_Task_Info = args.SqlTableEditorSaveDataTaskInfo;
                var ApprID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_ApprConfig_DT = new DataTable();
                    var CubeID = save_Task_Info.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    DuplicateConfig(CubeID, "Initiate", ref save_Result);

                    string sql = @"SELECT * 
                                    FROM FMM_ApprConfig
                                    WHERE CubeID = @CubeID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_ApprConfig_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                            ApprID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ApprConfig", "ApprID");

                            var new_DataRow = FMM_ApprConfig_DT.NewRow();
                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["ApprID"] = ApprID;
                            new_DataRow["Name"] = (string)xfRow.ModifiedDataRow.Items["Name"];
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_ApprConfig_DT.Rows.Add(new_DataRow);
                            DuplicateConfig(CubeID, "Update Row", ref save_Result, "Insert", xfRow);

                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_ApprConfig_DT.Select($"ApprID = {(int)xfRow.ModifiedDataRow["ApprID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["Name"] = (string)xfRow.ModifiedDataRow["Name"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                DuplicateConfig(CubeID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_ApprConfig_DT.Select($"ApprID = {(int)xfRow.OriginalDataRow["ApprID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    DuplicateConfig(CubeID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
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
                        cmdBuilder.UpdateTableSimple(si, "FMM_ApprConfig", FMM_ApprConfig_DT, sqa, "ApprID");
                    }
                }

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
                var ApprStepID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var sqa = new SqlDataAdapter();
                    var FMM_Appr_Step_Config_DT = new DataTable();
                    var CubeID = customSubstVars.XFGetValue("IV_FMM_CubeID", "0").XFConvertToInt();
                    var ApprID = customSubstVars.XFGetValue("IV_FMM_ApprID", "0").XFConvertToInt();
                    Duplicate_Appr_Step_Config(CubeID, ApprID, "Update Row", ref save_Result);

                    string sql = @"SELECT * 
                                    FROM FMM_Appr_Step_Config
                                    WHERE CubeID = @CubeID
                                    AND ApprID = @ApprID";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID }
                    };
                    cmdBuilder.FillDataTable(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);

                    foreach (XFEditedDataRow xfRow in save_Task_Info.EditedDataRows)
                    {
                        if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Insert)
                        {
                            var new_DataRow = FMM_Appr_Step_Config_DT.NewRow();

                            new_DataRow["CubeID"] = (int)xfRow.ModifiedDataRow["CubeID"];
                            new_DataRow["ApprID"] = (int)xfRow.ModifiedDataRow["ApprID"];
                            new_DataRow["ApprStepID"] = (int)xfRow.ModifiedDataRow["ApprStepID"];
                            new_DataRow["WFProfile_Step"] = (string)xfRow.ModifiedDataRow["WFProfile_Step"];
                            new_DataRow["StepNum"] = (int)xfRow.ModifiedDataRow["StepNum"];
                            new_DataRow["UserGroup"] = (string)xfRow.ModifiedDataRow["UserGroup"];
                            new_DataRow["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                            new_DataRow["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                            new_DataRow["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                            new_DataRow["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                            new_DataRow["InitStatus"] = (string)xfRow.ModifiedDataRow["InitStatus"];
                            new_DataRow["ApprStatus"] = (string)xfRow.ModifiedDataRow["ApprStatus"];
                            new_DataRow["RejStatus"] = (string)xfRow.ModifiedDataRow["RejStatus"];
                            new_DataRow["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;

                            FMM_Appr_Step_Config_DT.Rows.Add(new_DataRow);

                            Duplicate_Appr_Step_Config(CubeID, ApprID, "Insert Row", ref save_Result, "Insert", xfRow);
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Update)
                        {
                            var rowsToUpdate = FMM_Appr_Step_Config_DT.Select($"ApprStepID = {(int)xfRow.ModifiedDataRow["ApprStepID"]}");
                            if (rowsToUpdate.Length > 0)
                            {
                                var rowToUpdate = rowsToUpdate[0];
                                rowToUpdate["StepNum"] = (int)xfRow.ModifiedDataRow["StepNum"];
                                rowToUpdate["UserGroup"] = (string)xfRow.ModifiedDataRow["UserGroup"];
                                rowToUpdate["Logic"] = (string)xfRow.ModifiedDataRow["Logic"];
                                rowToUpdate["Item"] = (string)xfRow.ModifiedDataRow["Item"];
                                rowToUpdate["Level"] = (int)xfRow.ModifiedDataRow["Level"];
                                rowToUpdate["ApprConfig"] = (int)xfRow.ModifiedDataRow["ApprConfig"];
                                rowToUpdate["InitStatus"] = (string)xfRow.ModifiedDataRow["InitStatus"];
                                rowToUpdate["ApprStatus"] = (string)xfRow.ModifiedDataRow["ApprStatus"];
                                rowToUpdate["RejStatus"] = (string)xfRow.ModifiedDataRow["RejStatus"];
                                rowToUpdate["Status"] = (string)xfRow.ModifiedDataRow["Status"];
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                                Duplicate_Appr_Step_Config(CubeID, ApprID, "Update Row", ref save_Result, "Update", xfRow);
                            }
                        }
                        else if (xfRow.InsertUpdateOrDelete == DbInsUpdateDelType.Delete)
                        {
                            var rowsToDelete = FMM_Appr_Step_Config_DT.Select($"ApprStepID = {(int)xfRow.OriginalDataRow["ApprStepID"]}");
                            if (rowsToDelete.Length > 0)
                            {
                                foreach (var row in rowsToDelete)
                                {
                                    row.Delete();
                                    Duplicate_Appr_Step_Config(CubeID, ApprID, "Update Row", ref save_Result, "Delete", xfRow);
                                }
                            }
                        }
                    }
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
                        cmdBuilder.UpdateTableComposite(si, "FMM_Appr_Step_Config", FMM_Appr_Step_Config_DT, sqa, "CubeID", "ApprID", "ApprStepID");
                    }
                }

                save_Result.CancelDefaultSave = true; // Note: Use True if we already saved the data rows in this Business Rule.

                return save_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }




        private void Update_Src_Cell_Columns(DataRow DataRow)
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

                var sql = @"SELECT * 
                            FROM FMM_Src_Cell
                            WHERE CalcID = @CalcID
                            ORDER BY Src_Cell_ID";
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID }
                };

                connection.Open();

                cmdBuilder.FillDataTable(si, sqlDataAdapter, FMM_Src_Cell_DT, sql, parameters);

                string[] all_Dim_Types = { "Entity", "Cons", "Scenario", "Time", "View", "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
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
                string[] Supp_Dim_Types = { "Entity", "Cons", "Scenario", "Time" };
                string[] core_Dim_Types = { "Acct", "IC", "Origin", "Flow", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD7", "UD8" };
                foreach (var FMM_Src_Cell_DT_Row in FMM_Src_Cell_DT.Rows.Cast<DataRow>())
                {
                    src_Cell_Drill_Down = string.Empty;
                    src_Cell_Drill_Down_Count = 0;
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
                        if (DataRow[dest_Field] != DBNull.Value)
                            if (DataRow[dest_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                DataRow[dest_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                dest_Attributes[core_Dim_Type] = DataRow[dest_Field].ToString();
                                balances[core_Dim_Type] = 1;  // Assume resetting or incrementing depends on the logic												
                            }
                            else if (DataRow[filter_Field] != DBNull.Value)
                                if (DataRow[filter_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    DataRow[filter_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    dest_Attributes[core_Dim_Type] = DataRow[filter_Field].ToString();
                                    balances[core_Dim_Type] = 0;  // Assume resetting or incrementing depends on the logic												
                                }
                        if (FMM_Src_Cell_DT_Row[src_Field] != DBNull.Value)
                            if (FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 FMM_Src_Cell_DT_Row[src_Field].ToString().IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                src_Attributes[core_Dim_Type] = FMM_Src_Cell_DT_Row[src_Field].ToString();
                                src_Cell_Drill_Down += src_Cell_Drill_Down_Count == 0 ? src_Attributes[core_Dim_Type] : ":" + src_Attributes[core_Dim_Type];
                                src_Cell_Drill_Down_Count += 1;
                                balances[core_Dim_Type] += 1;
                            }
                        if (balances[core_Dim_Type] == 1)
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{core_Dim_Type}_Override"] = "Common";
                            if (src_Cell_Count == 1)
                            {
                                gbl_BalCalc = "UnbalAlloc";
                            }
                            else
                            {
                                gbl_BalCalc = "Unbalanced";
                            }
                            balanced_Src_Row = false;
                        }
                        else
                        {
                            FMM_Src_Cell_DT_Row[$"Unbal_{core_Dim_Type}_Override"] = string.Empty;
                            if (DataRow[$"OS_{core_Dim_Type}_Filter"] != DBNull.Value)
                            {
                                var core_Dim_Type_Filter = DataRow[$"OS_{core_Dim_Type}_Filter"].ToString();
                                if (core_Dim_Type_Filter.IndexOf(dim_token_1, StringComparison.OrdinalIgnoreCase) >= 0 || core_Dim_Type_Filter.IndexOf(dim_token_2, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    unbal_Src_Cell_Buffer_Filter = "[" + core_Dim_Type_Filter + "]";
                                    unbal_Src_Cell_Buffer_FilterCount++;
                                }
                            }
                        }
                    }

                    if ((string)FMM_Src_Cell_DT_Row["SrcType"] == "Dynamic Calc")
                    {
                        gbl_BalCalc = "UnbalAlloc";
                    }
                    gbl_SrcCell_Dict.Add((int)FMM_Src_Cell_DT_Row["CellID"], (string)FMM_Src_Cell_DT_Row["OpenParens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["MathOperator"].ToString().Trim() + " " + src_Cell_Drill_Down + "|" + (string)FMM_Src_Cell_DT_Row["CloseParens"].ToString().Trim());
                    gbl_SrcCell_Drill_Dict.Add((int)FMM_Src_Cell_DT_Row["CellID"], src_Cell_Drill_Down);
                    gbl_Unbal_Calc_Dict.Add((int)FMM_Src_Cell_DT_Row["CellID"], (string)FMM_Src_Cell_DT_Row["OpenParens"].ToString().Trim() + "|" + (string)FMM_Src_Cell_DT_Row["MathOperator"].ToString().Trim() + " -Calculation- " + "|" + (string)FMM_Src_Cell_DT_Row["CloseParens"].ToString().Trim());
                    FMM_Src_Cell_DT_Row["SrcOrder"] = src_Cell_Count;
                    FMM_Src_Cell_DT_Row["Dyn_Calc_Script"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer"] = src_Cell_Drill_Down;
                    FMM_Src_Cell_DT_Row["Unbal_Src_Cell_Buffer_Filter"] = unbal_Src_Cell_Buffer_Filter;
                }





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

            string countSql = @"
                SELECT COUNT(*) as Count 
                FROM FMM_Src_Cell 
                WHERE CalcID = @CalcID";

            DataTable srcDt;
            DataTable countDt;

            var sql_paramList = new List<DbParamInfo>
            {
                new DbParamInfo("@CalcID", gbl_CalcID)
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

                    var sql = @"SELECT * 
                                FROM FMM_Src_Cell 
                                WHERE CalcID = @CalcID
                                ORDER BY SrcOrder";
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID },
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Src_Cell_DT, sql, sqlparams);

                    FMM_Src_Cell_DT.PrimaryKey = new DataColumn[] { FMM_Src_Cell_DT.Columns["CellID"] };

                    foreach (DataRow src_DataRow in FMM_Src_Cell_DT.Rows)
                    {
                        unbal_Src_Cell_Buffer_Filter = string.Empty;
                        unbal_Src_Cell_Buffer_Filter_Cnt = 0;
                        override_Dest_Val = string.Empty;
                        override_Dest_Cnt = 0;



                        EvaluateDim("AcctFilter", "Acct", "A#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("OriginFilter", "Origin", "O#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("FlowFilter", "Flow", "F#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("IC_Filter", "IC", "I#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD1_Filter", "UD1", "U1#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD2_Filter", "UD2", "U2#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD3_Filter", "UD3", "U3#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD4_Filter", "UD4", "U4#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD5_Filter", "UD5", "U5#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD6_Filter", "UD6", "U6#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD7_Filter", "UD7", "U7#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        EvaluateDim("UD8_Filter", "UD8", "U8#", FMM_Src_Cell_DT, src_DataRow, ref unbal_Src_Cell_Buffer_Filter, ref unbal_Src_Cell_Buffer_Filter_Cnt, dest_DataRow, ref override_Dest_Val, ref override_Dest_Cnt);
                        src_DataRow["UnbalBuffer_Filter"] = unbal_Src_Cell_Buffer_Filter;
                        src_DataRow["OverrideValue"] = override_Dest_Val;
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


                if (gbl_BalCalc != "DB Model")
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var sqa = new SqlDataAdapter();
                        var FMM_Dest_Cell_DT = new DataTable();

                        var sql = @"SELECT * 
                                    FROM FMM_Dest_Cell
                                    WHERE CalcID = @CalcID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID }
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

                                        string patternStart = "|!MbrList_";
                                        string patternEnd = "_Filter.Name!|";

                                        if (filterValue.Contains(patternStart) && filterValue.Contains(patternEnd))
                                        {
                                            int startIndex = filterValue.IndexOf(patternStart) + patternStart.Length;
                                            int endIndex = filterValue.IndexOf(patternEnd, startIndex);

                                            string number = filterValue.Substring(startIndex, endIndex - startIndex);
                                            string memberListFilter = Get_Calc_Config_MbrList(number); // Assuming this is your function
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + memberListFilter + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                        }
                                        else
                                        {
                                            curr_Cube_Buffer_Filter += (curr_Cube_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            src_Buffer_Filter += (src_Buffer_FilterCnt == 0 ? "[" : ",[") + filterValue + "]";
                                            curr_Cube_Buffer_FilterCnt++;
                                            src_Buffer_FilterCnt++;
                                        }
                                    }
                                }
                            }
                            FMM_Dest_Cell_DT_Row["Curr_Cube_Buffer_Filter"] = curr_Cube_Buffer_Filter;
                            FMM_Dest_Cell_DT_Row["BufferFilter"] = src_Buffer_Filter;
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

        private void Evaluate_Calc_Config_Setup(int Curr_CalcID)
        {
            try
            {
                var DT = new DataTable();
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {

                    var sql = @"Select *
                                FROM FMM_Dest_Cell
                                WHERE CalcID = @Curr_CalcID";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Curr_CalcID", Curr_CalcID);
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            connection.Open();
                            adapter.Fill(DT);
                        }
                    }
                }

                foreach (DataRow destRow in DT.Rows)
                {
                    gbl_BalCalc = "Balanced";
                    Update_Src_Cell_Columns(destRow);
                    Update_Cell_Columns();
                    UpdateGlobals();

                    if (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc" || gbl_BalCalc == "ExtUnbalanced" || gbl_BalCalc == "Ext_UnbalAlloc")
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

                    var sql = @"SELECT * 												
                                FROM FMM_Calc_Config 												
                                WHERE CalcID = @CalcID";

                    var sqlparams = new SqlParameter[]
                    {
                            new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);

                    if (FMM_Calc_Config_DT.Rows.Count > 0)
                    {
                        foreach (DataRow row in FMM_Calc_Config_DT.Rows)
                        {
                            if (row["MultiDim_Alloc"] != DBNull.Value && Convert.ToBoolean(row["MultiDim_Alloc"]))
                            {
                                gbl_BalCalc = "MultiDim_Alloc";
                                row["BalBuffer"] = gbl_BalCalc;
                                row["Bal_Buffer_Calc"] = gbl_UnbalBuffer_Calc;
                                row["UnbalCalc"] = gbl_Unbal_Calc;
                                row["UpdateDate"] = DateTime.Now;
                                row["UpdateUser"] = si.UserName;
                            }
                            else if (row["MbrList_Calc"] != DBNull.Value && Convert.ToBoolean(row["MbrList_Calc"]))
                            {
                                if (gbl_BalCalc == "Unbalanced")
                                {
                                    gbl_BalCalc = "ExtUnbalanced";
                                    row["BalBuffer"] = gbl_BalCalc;
                                    row["Bal_Buffer_Calc"] = gbl_UnbalBuffer_Calc;
                                    row["UnbalCalc"] = gbl_Unbal_Calc;
                                    row["UpdateDate"] = DateTime.Now;
                                    row["UpdateUser"] = si.UserName;
                                }
                                else if (gbl_BalCalc == "UnbalAlloc")
                                {
                                    gbl_BalCalc = "Ext_UnbalAlloc";
                                    row["BalBuffer"] = gbl_BalCalc;
                                    row["Bal_Buffer_Calc"] = gbl_UnbalBuffer_Calc;
                                    row["UnbalCalc"] = gbl_Unbal_Calc;
                                    row["UpdateDate"] = DateTime.Now;
                                    row["UpdateUser"] = si.UserName;
                                }
                            }
                            else if (row["BR_Calc"] != DBNull.Value && Convert.ToBoolean(row["BR_Calc"]))
                            {
                                gbl_BalCalc = "BR_Calc";
                            }
                            else if (gbl_BalCalc == "Balanced")
                            {
                                row["BalBuffer"] = "Balanced";
                                row["Bal_Buffer_calc"] = gbl_Bal_Buffer_Calc;
                                row["UnbalCalc"] = String.Empty;
                                row["UpdateDate"] = DateTime.Now;
                                row["UpdateUser"] = si.UserName;

                            }
                            else if (gbl_BalCalc == "DB Model")
                            {
                                row["BalBuffer"] = gbl_BalCalc;
                                row["Table_Calc_Logic"] = gbl_Table_Calc_Logic;
                                row["Table_Src_Cell_Count"] = gbl_Table_SrcCell;
                                row["UpdateDate"] = DateTime.Now;
                                row["UpdateUser"] = si.UserName;
                            }
                            else
                            {
                                row["BalBuffer"] = gbl_BalCalc;
                                row["Bal_Buffer_calc"] = gbl_UnbalBuffer_Calc;
                                row["UnbalCalc"] = gbl_Unbal_Calc;
                                row["UpdateDate"] = DateTime.Now;
                                row["UpdateUser"] = si.UserName;
                            }

                        }
                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", FMM_Calc_Config_DT, sqa, "CalcID");
                    }
                    else
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private void UpdateGlobals()
        {
            try
            {
                var sortedDict = gbl_SrcCell_Dict.OrderBy(entry => entry.Key);


                int calcIterations = 1;
                foreach (KeyValuePair<int, string> calcSegment in sortedDict)
                {
                    if (gbl_BalCalc != "DB Model")
                    {
                        gbl_Bal_Buffer_Calc += StringHelper.ReplaceString(calcSegment.Value, "|", string.Empty, true);
                        if (calcIterations == 1 && (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc"))
                        {
                            gbl_UnbalBuffer_Calc = gbl_SrcCell_Drill_Dict[calcSegment.Key];
                            gbl_Unbal_Calc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "-Calculation-", "BalancedBuffer", true), "|", "", true);
                        }
                        else if (gbl_BalCalc == "Unbalanced" || gbl_BalCalc == "UnbalAlloc")
                        {
                            gbl_Unbal_Calc += StringHelper.ReplaceString(StringHelper.ReplaceString(gbl_Unbal_Calc_Dict[calcSegment.Key], "-Calculation-", "SrcBufferValue" + calcIterations.ToString(), true), "|", "", true);
                        }
                    }
                    else
                    {
                        gbl_UnbalBuffer_Calc = gbl_SrcCell_Drill_Dict[calcSegment.Key];
                        if (gbl_UnbalBuffer_Calc.Contains("T#|DBModelYear|", StringComparison.OrdinalIgnoreCase))
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

        private void EvaluateDim(string destFilterColumn, string src_Column, string filter_Prefix, DataTable srcDt, DataRow src_DataRow, ref string bufferFilter, ref int bufferFilterCnt, DataRow dest_DataRow, ref string override_Dest_Val, ref int override_Cnt)
        {
            if (gbl_BalCalc == "Unbalanced" || (gbl_BalCalc == "UnbalAlloc" && Convert.ToInt32(src_DataRow["SrcOrder"]) == 1))
            {
                if (dest_DataRow[destFilterColumn] != DBNull.Value && XFContains_Ignore_Case(dest_DataRow[destFilterColumn].ToString(), filter_Prefix))
                {
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
                    if (srcDt.Rows[Convert.ToInt32(src_DataRow["SrcOrder"]) - 2]["" + src_Column] == DBNull.Value || !XFContains_Ignore_Case(srcDt.Rows[Convert.ToInt32(src_DataRow["SrcOrder"]) - 2]["" + src_Column].ToString(), filter_Prefix))
                    {
                    }
                    else if (dest_DataRow[destFilterColumn] != DBNull.Value && XFContains_Ignore_Case(dest_DataRow[destFilterColumn].ToString(), filter_Prefix))
                    {
                        Add_to_Buffer_Filter(dest_DataRow[destFilterColumn].ToString(), ref bufferFilter, ref bufferFilterCnt);
                    }
                }
            }
            if (Convert.ToInt32(src_DataRow["SrcOrder"]) != 1)
            {
                if (((!dest_DataRow["" + src_Column].ToString().XFContainsIgnoreCase(filter_Prefix) && !dest_DataRow["OS_" + src_Column + "_Filter"].ToString().XFContainsIgnoreCase(filter_Prefix)) &&
                     src_DataRow["Dyn_Calc_Script"].ToString().XFContainsIgnoreCase(filter_Prefix)) ||
                    (srcDt.Rows[Convert.ToInt32(src_DataRow["SrcOrder"]) - 2]["" + src_Column].ToString().XFContainsIgnoreCase(filter_Prefix) &&
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
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string columnValue = row[columnName].ToString();

                    foreach (var pattern in patterns)
                    {
                        if (columnValue.StartsWith(pattern.Key) && columnValue.Length > 1)
                        {
                            return pattern.Value;
                        }
                    }
                }

                return string.Empty;
            }
            public string Get_Src_Dest_Filter(DataRow row, string columnName)
            {
                if (row.Table.Columns.Contains(columnName) && row[columnName] != DBNull.Value)
                {
                    string columnValue = row[columnName].ToString().Replace(".Children", ".Base");

                    return columnValue;
                }

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

                var sqa = new SqlDataAdapter();
                var FMM_Calc_Config_DT = new DataTable();
                var sql = @$"SELECT MbrList_{number}_Filter
                            FROM FMM_Calc_Config
                            WHERE CalcID = @CalcID";
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@CalcID", SqlDbType.Int) { Value = gbl_CalcID}
                };

                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);

                foreach (DataRow Calc_Config_Row in FMM_Calc_Config_DT.Rows)
                {
                    return (string)Calc_Config_Row[$"MbrList_{number}_Filter"];
                }
            }


            return string.Empty;
        }




        private void Duplicate_ActConfig(int CubeID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_ActConfig_DataRow)
        {
            var act_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_ActConfig_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_ActConfig
                                        WHERE CubeID = @CubeID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID}
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_ActConfig_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow cube_Activity_Row in FMM_ActConfig_DT.Rows)
                            {
                                act_Key = CubeID + "|" + (int)cube_Activity_Row["ActID"];
                                gbl_ActConfig_Dict.Add(act_Key, (string)cube_Activity_Row["Name"] + "|" + (string)cube_Activity_Row["CalcType"]);
                            }
                        }
                        break;

                    case "Update Row":

                        act_Key = CubeID + "|" + (int)Modified_FMM_ActConfig_DataRow.ModifiedDataRow["ActID"];
                        var newact_Value = (string)Modified_FMM_ActConfig_DataRow.ModifiedDataRow["Name"] + "|" + (string)Modified_FMM_ActConfig_DataRow.ModifiedDataRow["CalcType"];


                        if (ddl_Process == "Insert")
                        {
                            gbl_ActConfig_Dict.Add(act_Key, newact_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            var origact_Value = (string)Modified_FMM_ActConfig_DataRow.OriginalDataRow["Name"] + "|" + (string)Modified_FMM_ActConfig_DataRow.OriginalDataRow["CalcType"];

                            if (origact_Value != newact_Value)
                            {
                                gbl_ActConfig_Dict.XFSetValue(act_Key, newact_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            if (gbl_ActConfig_Dict.ContainsKey(act_Key))
                            {
                                gbl_ActConfig_Dict.Remove(act_Key);
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_CalcUnitConfig(int CubeID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_CalcUnitConfig_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_CalcUnitConfig_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_CalcUnitConfig
                                        WHERE CubeID = @CubeID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID}
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CalcUnitConfig_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow Calc_Unit_Row in FMM_CalcUnitConfig_DT.Rows)
                            {
                                config_Key = CubeID + "|" + (int)Calc_Unit_Row["CalcUnitID"];
                                var config_Value = (string)Calc_Unit_Row["Entity_MFB"] + "|" + (string)Calc_Unit_Row["WFChannel"];

                                gbl_CalcUnitConfig_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        var newconfig_Value = string.Empty;

                        if (ddl_Process == "Insert")
                        {
                            config_Key = CubeID + "|" + (int)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["CalcUnitID"];
                            newconfig_Value = (string)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["WFChannel"];

                            gbl_CalcUnitConfig_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            config_Key = CubeID + "|" + (int)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["CalcUnitID"];
                            newconfig_Value = (string)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_CalcUnitConfig_DataRow.ModifiedDataRow["WFChannel"];
                            var origConfig_Value = (string)Modified_FMM_CalcUnitConfig_DataRow.OriginalDataRow["Entity_MFB"] + "|" + (string)Modified_FMM_CalcUnitConfig_DataRow.OriginalDataRow["WFChannel"];

                            if (origConfig_Value != newconfig_Value)
                            {
                                gbl_CalcUnitConfig_Dict.XFSetValue(config_Key, newconfig_Value);
                            }
                        }
                        else if (ddl_Process == "Delete")
                        {
                            config_Key = CubeID + "|" + (int)Modified_FMM_CalcUnitConfig_DataRow.OriginalDataRow["CalcUnitID"];
                            if (gbl_CalcUnitConfig_Dict.ContainsKey(config_Key))
                            {
                                gbl_CalcUnitConfig_Dict.Remove(config_Key);
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = "An unexpected error occurred.";
            }
        }


        private void Duplicate_Unit_Config(int CubeID, int ActID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Unit_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Unit_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Unit_Config
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Unit_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID} and ActID {ActID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow cube_Unit_Row in FMM_Unit_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ActID + "|" + (int)cube_Unit_Row["UnitID"];
                                var config_Value = (string)cube_Unit_Row["Name"];

                                gbl_Unit_Config_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = CubeID + "|" + ActID + "|" + (int)Modified_FMM_Unit_Config_DataRow.ModifiedDataRow["UnitID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = "An unexpected error occurred.";
            }
        }

        private void Duplicate_Acct_Config(int CubeID, int ActID, int UnitID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Acct_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Acct_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Acct_Config
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID
                                        AND UnitID = @UnitID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                                new SqlParameter("@UnitID", SqlDbType.Int) { Value = UnitID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Acct_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}, ActID {ActID}, UnitID {UnitID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow cube_Acct_Row in FMM_Acct_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ActID + "|" + UnitID + "|" + (int)cube_Acct_Row["AcctID"];
                                var config_Value = (string)cube_Acct_Row["Name"];

                                gbl_Acct_Config_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = CubeID + "|" + ActID + "|" + UnitID + "|" + (int)Modified_FMM_Acct_Config_DataRow.ModifiedDataRow["AcctID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }


        private void DuplicateConfig(int CubeID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_ApprConfig_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_ApprConfig_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_ApprConfig
                                        WHERE CubeID = @CubeID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_ApprConfig_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow ApprRow in FMM_ApprConfig_DT.Rows)
                            {
                                config_Key = CubeID + "|" + (int)ApprRow["ApprID"];
                                var config_Value = (string)ApprRow["Name"];

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

                        config_Key = CubeID + "|" + (int)Modified_FMM_ApprConfig_DataRow.ModifiedDataRow["ApprID"];
                        string newconfig_Value = (string)Modified_FMM_ApprConfig_DataRow.ModifiedDataRow["Name"];

                        if (ddl_Process == "Insert")
                        {
                            gbl_Appr_Dict.Add(config_Key, newconfig_Value);
                        }
                        else if (ddl_Process == "Update")
                        {
                            string origconfig_Value = (string)Modified_FMM_ApprConfig_DataRow.OriginalDataRow["Name"];

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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Appr_Step_Config(int CubeID, int ApprID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Appr_Step_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Appr_Step_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Appr_Step_Config
                                        WHERE CubeID = @CubeID
                                        AND ApprID = @ApprID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow Appr_Step_Row in FMM_Appr_Step_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ApprID + "|" + (int)Appr_Step_Row["ApprStepID"];
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

                        config_Key = CubeID + "|" + ApprID + "|" + (int)Modified_FMM_Appr_Step_Config_DataRow.ModifiedDataRow["ApprStepID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }



        private void Duplicate_Reg_Config(int CubeID, int ActID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Reg_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Reg_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Reg_Config
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Reg_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID} and ActID {ActID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow RegisterRow in FMM_Reg_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ActID + "|" + (int)RegisterRow["RegConfigID"];
                                var config_Value = (string)RegisterRow["Name"];


                                gbl_Register_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = CubeID + "|" + ActID + "|" + (int)Modified_FMM_Reg_Config_DataRow.ModifiedDataRow["RegConfigID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }

        private void Duplicate_Col_Config(int CubeID, int ActID, int RegConfigID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Col_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Col_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Col_Config
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID
                                        AND RegConfigID = @RegConfigID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                                new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = RegConfigID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Col_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID} and ActID {ActID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow ColRow in FMM_Col_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ActID + "|" + RegConfigID + "|" + (int)ColRow["Col_ID"];
                                var config_Value = ((int)ColRow["Order"]).XFToString();

                                if (config_Value != "99")
                                {
                                    gbl_Col_Dict.Add(config_Key, config_Value);
                                }
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = CubeID + "|" + ActID + "|" + RegConfigID + "|" + (int)Modified_FMM_Col_Config_DataRow.ModifiedDataRow["Col_ID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }


        private void Duplicate_Calc_Config(int CubeID, int ActID, int ModelID, string dup_Process_Step, ref XFSelectionChangedTaskResult save_Result, [Optional] string ddl_Process, [Optional] XFEditedDataRow Modified_FMM_Calc_Config_DataRow)
        {
            var config_Key = string.Empty;
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Calc_Config_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Calc_Config
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID
                                        AND ModelID = @ModelID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                                new SqlParameter("@ModelID", SqlDbType.Int) { Value = ModelID }
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Calc_Config_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID} and ActID {ActID}: {ex.Message}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow CalcRow in FMM_Calc_Config_DT.Rows)
                            {
                                config_Key = CubeID + "|" + ActID + "|" + ModelID + "|" + (int)CalcRow["CalcID"];
                                var config_Value = (string)CalcRow["Name"];


                                gbl_Calc_Dict.Add(config_Key, config_Value);
                            }
                        }
                        break;

                    case "Update Row":

                        config_Key = CubeID + "|" + ActID + "|" + ModelID + "|" + (int)Modified_FMM_Calc_Config_DataRow.ModifiedDataRow["CalcID"];
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
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred: {ex.Message}";
            }
        }



        public void Insert_Col_Default_Rows(int CubeID, int ActID, int RegConfigID)
        {
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sqlMaxIdGetter = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                var FMM_Col_Config_DT = new DataTable();
                connection.Open();
                int os_Col_ID = sqlMaxIdGetter.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
                string selectQuery_colSetup = @"
                    SELECT * 
                    FROM FMM_Col_Config 
                    WHERE CubeID = @CubeID
                    AND ActID = @ActID
                    AND RegConfigID = @RegConfigID";

                var parameters_colSetup = new SqlParameter[]
                {
                    new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                    new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                    new SqlParameter("@RegConfigID", SqlDbType.Int) { Value = RegConfigID }
                };
                cmdBuilder.FillDataTable(si, sqa, FMM_Col_Config_DT, selectQuery_colSetup, parameters_colSetup);
                var columnConfigDefaults = new ColumnConfigDefaults();
                foreach (var columnConfig in columnConfigDefaults.DefaultColumns)
                {
                    var new_DataRow = FMM_Col_Config_DT.NewRow();
                    new_DataRow["CubeID"] = CubeID;
                    new_DataRow["ActID"] = ActID;
                    new_DataRow["RegConfigID"] = RegConfigID;
                    new_DataRow["Col_ID"] = ++os_Col_ID; // Ensure unique IDs
                    new_DataRow["Name"] = columnConfig.ColName;
                    new_DataRow["InUse"] = true; // Set default value for Col_InUse if needed
                    new_DataRow["Required"] = true;
                    new_DataRow["Updates"] = true;
                    new_DataRow["Alias"] = columnConfig.ColAlias;
                    new_DataRow["Order"] = columnConfig.ColOrder;
                    new_DataRow["Default"] = DBNull.Value; // Set default value if needed
                    new_DataRow["Param"] = DBNull.Value; // Set default value if needed
                    new_DataRow["Format"] = string.Empty; // Set default value if needed
                    new_DataRow["FilterParam"] = DBNull.Value; // Set default value if needed
                    new_DataRow["CreateDate"] = DateTime.Now;
                    new_DataRow["CreateUser"] = si.UserName;
                    new_DataRow["UpdateDate"] = DateTime.Now;
                    new_DataRow["UpdateUser"] = si.UserName;

                    FMM_Col_Config_DT.Rows.Add(new_DataRow);
                }
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", FMM_Col_Config_DT, sqa, "Col_ID");
            }
        }


        public class ColumnConfigDefaults
        {
            public List<ColumnConfig> DefaultColumns { get; set; }

            public ColumnConfigDefaults()
            {
                DefaultColumns = new List<ColumnConfig>
                {
                    new ColumnConfig { ColName = "Entity", ColAlias = "Entity", ColOrder = 1 },
                    new ColumnConfig { ColName = "Level_ID", ColAlias = "Approval Level ID", ColOrder = 2 },
                    new ColumnConfig { ColName = "RegisterID", ColAlias = "Register ID", ColOrder = 3 },
                    new ColumnConfig { ColName = "RegisterID1", ColAlias = "Register ID 1", ColOrder = 4 },
                    new ColumnConfig { ColName = "RegisterID2", ColAlias = "Register ID 2", ColOrder = 5 },
                    new ColumnConfig { ColName = "SpreadAmount", ColAlias = "Spread Amount", ColOrder = 43 },
                    new ColumnConfig { ColName = "SpreadCurve", ColAlias = "Spread Curve", ColOrder = 44 }
                };

                for (int i = 1; i <= 20; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        ColName = $"Attribute{i}",
                        ColAlias = $"Attribute {i}",
                        ColOrder = i + 5 // Starting from 6
                    });
                }
                for (int i = 1; i <= 12; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        ColName = $"AttributeValue{i}",
                        ColAlias = $"Attribute Value {i}",
                        ColOrder = i + 25 // Starting from 26
                    });
                }
                for (int i = 1; i <= 5; i++)
                {
                    DefaultColumns.Add(new ColumnConfig
                    {
                        ColName = $"DateValue{i}",
                        ColAlias = $"Date Value {i}",
                        ColOrder = i + 37 // Starting from 38
                    });
                }

            }

            public class ColumnConfig
            {
                public string ColName { get; set; }
                public string ColAlias { get; set; }
                public int ColOrder { get; set; }
            }
        }

        private XFSelectionChangedTaskResult Select_Add_FMM_CalcID()
        {
            try
            {
                var select_Result = new XFSelectionChangedTaskResult();
                select_Result.ChangeCustomSubstVarsInDashboard = true;
                var optionintValue = 2;
                var gbl_helpers = new GBL_UI_Assembly.GBL_Helpers();
                gbl_helpers.UpdateCustomSubstVar(ref select_Result, "BL_DDM_Config_Menu", string.Empty);

                return select_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }









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


        private XFSelectionChangedTaskResult Save_CubeConfig(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var Cube = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_FMM_All_Cube_Names", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube", string.Empty));
                var ScenType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_ScenTypes", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Scen_Type", string.Empty));
                var agg_Consol = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Agg_Consol", string.Empty);
                var entity_Dim = String.Empty;
                var entity_MFB = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_EntityMFB", string.Empty);
                var cube_Description = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeConfigDescr", string.Empty);
                int new_CubeID = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    connection.Open();

                    var sqa = new SqlDataAdapter();
                    var FMM_CubeConfig_Count_DT = new DataTable();

                    var sql = @"SELECT Count(*) as Count
                                FROM FMM_CubeConfig
                                WHERE Cube = @Cube
                                AND ScenType = @ScenType";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.NVarChar, 50) { Value = Cube },
                        new SqlParameter("@ScenType", SqlDbType.NVarChar, 20) { Value = ScenType }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CubeConfig_Count_DT, sql, sqlparams);

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_CubeConfig_DT = new DataTable();

                    sql = "SELECT * FROM FMM_CubeConfig WHERE CubeID = @CubeID";
                    sqlparams = new SqlParameter[]
                    {
                    };

                    if (runType == "Add")
                    {
                        new_CubeID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CubeConfig", "CubeID");

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = new_CubeID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        var new_DataRow = FMM_CubeConfig_DT.NewRow();
                        var SaveTypeintValue = 1;
                        FMM_Config_Helpers.SaveType saveType = (FMM_Config_Helpers.SaveType)SaveTypeintValue;
                        if (FMM_Config_Helpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
                        {
                            foreach (var step in config.ParameterMappings)
                            {
                                foreach (var map in step.Value)
                                {
                                    new_DataRow[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Value, string.Empty);
                                }
                            }
                        }
                        new_DataRow["CubeID"] = new_CubeID;
                        new_DataRow["Status"] = 1;
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_CubeConfig_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTableSimple(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa, "CubeID");

                        save_Result.IsOK = true;
                        save_Result.Message = "New Cube Config Saved.";
                        save_Result.ShowMessageBox = true;
                    }
                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "Update")
                    {
                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                        new_CubeID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_CubeID", "0"));
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = new_CubeID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        if (FMM_CubeConfig_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_CubeConfig_DT.Rows[0];
                            var SaveTypeintValue = 2;
                            FMM_Config_Helpers.SaveType saveType = (FMM_Config_Helpers.SaveType)SaveTypeintValue;
                            if (FMM_Config_Helpers.CubeConfigRegistry.Configs.TryGetValue(saveType, out var config))
                            {
                                foreach (var step in config.ParameterMappings)
                                {
                                    foreach (var map in step.Value)
                                    {
                                        rowToUpdate[map.Value] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(map.Value, string.Empty);
                                    }
                                }
                            }
                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;

                            cmdBuilder.UpdateTableSimple(si, "FMM_CubeConfig", FMM_CubeConfig_DT, sqa, "CubeID");

                            save_Result.IsOK = true;
                            save_Result.Message = "Cube Config Updates Saved.";
                            save_Result.ShowMessageBox = true;
                        }
                    }
                    else if (Convert.ToInt32(FMM_CubeConfig_Count_DT.Rows[0]["Count"]) > 0 && runType == "New")
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
                var errorResult = new XFSelectionChangedTaskResult
                {
                    IsOK = false,
                    Message = $"An error occurred: {ex.Message}",
                    ShowMessageBox = true
                };
                return errorResult;
            }
        }



        private XFSelectionChangedTaskResult SaveModel(string runType)
        {
            try
            {
                var save_Result = new XFSelectionChangedTaskResult();
                var customSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var ActID = Convert.ToInt32(customSubstVars.XFGetValue("IV_FMM_ActID", "0"));
                var CubeID = Convert.ToInt32(customSubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var new_Model_Name = customSubstVars.XFGetValue("IV_FMM_Model_Name", string.Empty);
                int new_ModelID = 0;
                if (new_ModelID == 0)
                {
                    DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        if (runType == "New")
                        {
                            new_ModelID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "ModelID");
                        }
                        else
                        {
                            new_ModelID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_ModelID", "0"));
                        }

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Models_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_Models 
                                        WHERE ModelID = @ModelID";


                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ModelID", SqlDbType.Int) { Value = new_ModelID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Models_DT, sql, sqlparams);
                        if (runType == "New")
                        {
                            var new_DataRow = FMM_Models_DT.NewRow();
                            new_DataRow["CubeID"] = CubeID;
                            new_DataRow["ActID"] = ActID;
                            new_DataRow["ModelID"] = new_ModelID;
                            new_DataRow["Name"] = new_Model_Name;
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;
                            FMM_Models_DT.Rows.Add(new_DataRow);
                        }
                        else if (runType == "Update")
                        {
                            if (FMM_Models_DT.Rows.Count > 0)
                            {
                                var rowToUpdate = FMM_Models_DT.Rows[0];
                                rowToUpdate["Name"] = new_Model_Name;
                                rowToUpdate["Status"] = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", string.Empty);
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                            }

                        }

                        cmdBuilder.UpdateTableSimple(si, "FMM_Models", FMM_Models_DT, sqa, "ModelID");
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
                var new_OS_Model_Grp_CubeID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Name", "0");
                int new_Model_Grp_ID = 0;
                if (new_Model_Grp_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_Model_Grp_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrps", "Model_Grp_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ModelGrps_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_ModelGrps 
                                        WHERE Model_Grp_ID = @Model_Grp_ID";


                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = new_Model_Grp_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ModelGrps_DT, sql, sqlparams);

                        var new_DataRow = FMM_ModelGrps_DT.NewRow();
                        new_DataRow["CubeID"] = new_OS_Model_Grp_CubeID;
                        new_DataRow["Model_Grp_ID"] = new_Model_Grp_ID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;
                        FMM_ModelGrps_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrps", FMM_ModelGrps_DT, sqa, "Model_Grp_ID");
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
                var new_CubeID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_Name", "0");
                int new_Model_Grp_Seq_ID = 0;
                if (new_Model_Grp_Seq_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_Model_Grp_Seq_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Model_Grp_Seqs", "Model_Grp_Seq_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Model_Grp_Seqs_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_Model_Grp_Seqs 
                                        WHERE Model_Grp_Seq_ID = @Model_Grp_Seq_ID";


                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = new_Model_Grp_Seq_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Model_Grp_Seqs_DT, sql, sqlparams);

                        var new_DataRow = FMM_Model_Grp_Seqs_DT.NewRow();
                        new_DataRow["CubeID"] = new_CubeID;
                        new_DataRow["Model_Grp_Seq_ID"] = new_Model_Grp_Seq_ID;
                        new_DataRow["Name"] = new_Name;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;
                        FMM_Model_Grp_Seqs_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTableSimple(si, "FMM_Model_Grp_Seqs", FMM_Model_Grp_Seqs_DT, sqa, "Model_Grp_Seq_ID");
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
                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var Model_Grp_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID", "0"));
                var ModelID_List = custom_SubstVars.XFGetValue("IV_FMM_ModelID_Selection", "0");
                int new_OS_Model_Grp_Assign_ID = 0;
                if (ModelID_List.Length > 0 && new_OS_Model_Grp_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_OS_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrpAssign", "Model_Grp_Assign_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_ModelGrpAssign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        string sql = @"
                                        SELECT * 
                                        FROM FMM_ModelGrpAssign
                                        WHERE CubeID = @CubeID
                                        AND Model_Grp_ID = @Model_Grp_ID";

                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = Model_Grp_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_ModelGrpAssign_DT, sql, sqlparams);

                        var modelIds = ModelID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var modelId in modelIds)
                        {
                            if (!isFirstIteration)
                            {
                                new_OS_Model_Grp_Assign_ID += 1;
                            }
                            isFirstIteration = false;
                            var new_DataRow = FMM_ModelGrpAssign_DT.NewRow();
                            new_DataRow["CubeID"] = CubeID;
                            new_DataRow["Model_Grp_ID"] = Model_Grp_ID;
                            new_DataRow["ModelID"] = Convert.ToInt32(modelId.Trim());
                            new_DataRow["Model_Grp_Assign_ID"] = new_OS_Model_Grp_Assign_ID;
                            new_DataRow["Sequence"] = 0;
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;

                            FMM_ModelGrpAssign_DT.Rows.Add(new_DataRow);
                        }

                        cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrpAssign", FMM_ModelGrpAssign_DT, sqa, "Model_Grp_Assign_ID");
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
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var CalcUnitID_List = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Selection", "0");
                var Model_Grp_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID_Selection");
                var Model_Grp_Seq_ID = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_ID");
                int new_Calc_Unit_Assign_ID = 0;
                if (Model_Grp_ID_List.Length > 0 && new_Calc_Unit_Assign_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        connection.Open();

                        new_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Assign", "Calc_Unit_Assign_ID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Calc_Unit_Assign_DT = new DataTable();
                        var sqa = new SqlDataAdapter();

                        var sql = @"SELECT * 
                                    FROM FMM_Calc_Unit_Assign
                                    WHERE CubeID = @CubeID
                                    AND Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        var sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = Model_Grp_Seq_ID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Calc_Unit_Assign_DT, sql, sqlparams);

                        var modelGroupIds = Model_Grp_ID_List.Split(',');
                        var WfDuIds = CalcUnitID_List.Split(',');
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
                                new_DataRow["CubeID"] = CubeID;
                                new_DataRow["Model_Grp_ID"] = Convert.ToInt32(modelGroupId.Trim());
                                new_DataRow["CalcUnitID"] = Convert.ToInt32(WfDuId.Trim());
                                new_DataRow["Model_Grp_Seq_ID"] = Model_Grp_Seq_ID;
                                new_DataRow["Calc_Unit_Assign_ID"] = new_Calc_Unit_Assign_ID;
                                new_DataRow["Sequence"] = 0;
                                new_DataRow["Status"] = "Build";
                                new_DataRow["CreateDate"] = DateTime.Now;
                                new_DataRow["CreateUser"] = si.UserName;
                                new_DataRow["UpdateDate"] = DateTime.Now;
                                new_DataRow["UpdateUser"] = si.UserName;

                                FMM_Calc_Unit_Assign_DT.Rows.Add(new_DataRow);
                            }
                        }

                        cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
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
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var ApprID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_ApprID", "0"));
                var wfProfile_Step = custom_SubstVars.XFGetValue("IV_FMM_trv_Appr_Step_WFProfile", string.Empty);

                var new_ApprStepID = 0;

                if (CubeID > 0 && ApprID > 0 && wfProfile_Step != string.Empty && new_ApprStepID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        new_ApprStepID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "ApprStepID");

                        var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                        var FMM_Appr_Step_Config_DT = new DataTable();
                        var FMM_Appr_Step_WFProfile_DT = new DataTable();
                        var FMM_CubeConfig_DT = new DataTable();
                        var FMM_Act_Appr_Step_Config_DT = new DataTable(); // DataTable for FMM_Reg_Dtl_Cube_Map
                        var sqa = new SqlDataAdapter();

                        var sql = @"SELECT * 
                                    FROM FMM_Appr_Step_Config
                                    WHERE CubeID = @CubeID
                                    AND ApprID = @ApprID";

                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                            new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID }
                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Appr_Step_Config_DT, sql, sqlparams);




                        sql = @"SELECT * 
                                FROM FMM_CubeConfig
                                WHERE CubeID = @CubeID";

                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                        };

                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_CubeConfig_DT, sql, sqlparams);

                        var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, FMM_CubeConfig_DT.Rows[0].Field<string>("Cube"));
                        var cubeScenarioType = ScenarioType.GetItem("Plan").Id;
                        var cubeScenarioTypeId = ScenarioTypeId.LongTerm;
                        var rootProfileName = topCubeInfo.GetTopLevelCubeWFPName(si, cubeScenarioType);
                        var wfProfileSuffixes = new Dictionary<ScenarioTypeId, string>();
                        wfProfileSuffixes = topCubeInfo.TopLevelCubeWFPSuffixes;
                        var wfProfileSuffix = wfProfileSuffixes.XFGetValue(cubeScenarioTypeId, string.Empty);

                        foreach (var entry in wfProfileSuffixes)
                        {
                            ScenarioTypeId id = entry.Key;
                            string suffix = entry.Value;

                        }

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

                        var filteredWFProfiles = from profile in FMM_Appr_Step_WFProfile_DT.AsEnumerable()
                                                 where profile["ProfileName"].ToString().Contains(wfProfile_Step)
                                                 select profile;

                        foreach (var profile in filteredWFProfiles)
                        {
                            var new_DataRow = FMM_Appr_Step_Config_DT.NewRow();
                            new_DataRow["CubeID"] = CubeID;
                            new_DataRow["ApprID"] = ApprID;
                            new_DataRow["WFProfile_Step"] = profile["ProfileKey"];
                            new_DataRow["ApprStepID"] = new_ApprStepID++;
                            new_DataRow["StepNum"] = 0;
                            new_DataRow["UserGroup"] = "Test";
                            new_DataRow["Logic"] = "Test";
                            new_DataRow["Item"] = "Test";
                            new_DataRow["Level"] = 0;
                            new_DataRow["ApprConfig"] = 0;
                            new_DataRow["InitStatus"] = "Testy";
                            new_DataRow["ApprStatus"] = "Testy";
                            new_DataRow["RejStatus"] = "Testy";
                            new_DataRow["Status"] = "Build";
                            new_DataRow["CreateDate"] = DateTime.Now;
                            new_DataRow["CreateUser"] = si.UserName;
                            new_DataRow["UpdateDate"] = DateTime.Now;
                            new_DataRow["UpdateUser"] = si.UserName;

                            FMM_Appr_Step_Config_DT.Rows.Add(new_DataRow);


                        }

                        cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", FMM_Appr_Step_Config_DT, sqa, "ApprStepID");


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

                var save_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var ActID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_ActID", "0"));
                var ApprID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_ApprID", "0"));
                var ApprStepID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_ApprStepID", "-1")); // this can actually be 0
                var RegConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Reg_Configs", "0"));

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_Act_Appr_Step_Config_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    var sql = @"SELECT * 
                                    FROM FMM_Act_Appr_Step_Config
                                    WHERE ActID = @ActID
                                    AND ApprID = @ApprID
                                    AND ApprStepID = @ApprStepID";

                    var sqlparams = new SqlParameter[] { };



                    if (runType == "New" && ActID > 0 && ApprID > 0 && ApprStepID >= 0 && RegConfigID > 0) // new row
                    {
                        var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        connection.Open();

                        var new_Appr_Step_ActID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Act_Appr_Step_Config", "Act_ApprStepID");


                        sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID },
                        new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID },
                        new SqlParameter("@ApprStepID", SqlDbType.Int) { Value = ApprStepID },

                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        string noneVal = "None";
                        FMM_Act_Appr_Step_Config_DT.Clear(); // clear data, because we only want the column information to create a new row.
                        var new_DataRow = FMM_Act_Appr_Step_Config_DT.NewRow();
                        new_DataRow["ActID"] = ActID;
                        new_DataRow["ApprID"] = ApprID;
                        new_DataRow["ApprStepID"] = ApprStepID;
                        new_DataRow["Appr_Step_ActID"] = new_Appr_Step_ActID;
                        new_DataRow["RegConfigID"] = RegConfigID;
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
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_Act_Appr_Step_Config_DT.Rows.Add(new_DataRow);

                        cmdBuilder.UpdateTableSimple(si, "FMM_Act_Appr_Step_Config", FMM_Act_Appr_Step_Config_DT, sqa, "Appr_Step_ActID");


                    }
                    else if (runType == "Update" && ActID > 0)
                    { // existing row

                        connection.Open();

                        sql = @"SELECT * 
                            FROM FMM_Act_Appr_Step_Config
                            WHERE Appr_Step_ActID = @Appr_Step_ActID";

                        sqlparams = new SqlParameter[]
                        {
                        new SqlParameter("@Appr_Step_ActID", SqlDbType.Int) { Value = ActID }

                        };

                        cmdBuilder.FillDataTable(si, sqa, FMM_Act_Appr_Step_Config_DT, sql, sqlparams);

                        FMM_Act_Appr_Step_Config_DT.Rows[0]["RegConfigID"] = RegConfigID;
                        FMM_Act_Appr_Step_Config_DT.Rows[0]["UpdateDate"] = DateTime.Now;
                        FMM_Act_Appr_Step_Config_DT.Rows[0]["UpdateUser"] = si.UserName;

                        cmdBuilder.UpdateTableSimple(si, "FMM_Act_Appr_Step_Config", FMM_Act_Appr_Step_Config_DT, sqa, "Appr_Step_ActID");
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
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_CubeID", "0"));
                var Calc_Unit_Entity_MFB = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Entity_MFB", "0");
                var WF_Channels = custom_SubstVars.XFGetValue("BL_FMM_WFChannels", "0");
                var new_CalcUnitID = 0;
                var loop_times = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                    connection.Open();

                    new_CalcUnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcUnitConfig", "CalcUnitID");

                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                    var FMM_CalcUnitConfig_DT = new DataTable();
                    var sqa = new SqlDataAdapter();

                    string sql = @"SELECT * 
                                   FROM FMM_CalcUnitConfig
                                   WHERE CubeID = @CubeID";

                    var sqlparams = new SqlParameter[]
                    {
                    new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                    };

                    cmdBuilder.FillDataTable(si, sqa, FMM_CalcUnitConfig_DT, sql, sqlparams);
                    var cubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, "Army_RMW_Consol");
                    var ent_DimID = cubeInfo.Cube.CubeDims.GetEntityDimId();
                    var cubeDim_Lookup = cubeInfo.Cube.CubeDims.CubeDimsDictionary.Values.ToLookup(dim => dim.DimId);
                    IEnumerable<CubeDim> ent_Dim = cubeDim_Lookup[ent_DimID];
                    var Calc_Unit_EntList = new List<MemberInfo>();
                    foreach (CubeDim dim in ent_Dim)
                    {
                        var dimTypeID = dim.CubeDimPk.DimTypeId;
                        var ent_DimPk = new DimPk(dimTypeID, ent_DimID);
                        Calc_Unit_EntList = BRApi.Finance.Members.GetMembersUsingFilter(si, ent_DimPk, Calc_Unit_Entity_MFB, true);
                    }

                    foreach (var entity in Calc_Unit_EntList)
                    {
                        var new_DataRow = FMM_CalcUnitConfig_DT.NewRow();
                        if (loop_times == 0)
                        {
                            new_DataRow["CalcUnitID"] = new_CalcUnitID;
                        }
                        {
                            new_DataRow["CalcUnitID"] = ++new_CalcUnitID;
                        }
                        new_DataRow["CubeID"] = CubeID;
                        new_DataRow["Entity_MFB"] = entity.Member.Name;
                        new_DataRow["WFChannel"] = WF_Channels;
                        new_DataRow["Status"] = "Build";
                        new_DataRow["CreateDate"] = DateTime.Now;
                        new_DataRow["CreateUser"] = si.UserName;
                        new_DataRow["UpdateDate"] = DateTime.Now;
                        new_DataRow["UpdateUser"] = si.UserName;

                        FMM_CalcUnitConfig_DT.Rows.Add(new_DataRow);
                        loop_times += 1;
                    }

                    cmdBuilder.UpdateTableSimple(si, "FMM_CalcUnitConfig", FMM_CalcUnitConfig_DT, sqa, "CalcUnitID");

                    return save_Result;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }
        private void Process_Copy_Cube_Config(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeID", "0"));
            var tgt_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeID", "0"));
            var src_FMM_ActConfig_DT = new DataTable();
            var tgt_FMM_ActConfig_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_ApprConfig_DT = new DataTable();
            var tgt_FMM_ApprConfig_DT = new DataTable();
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
            var src_FMM_ModelGrps_DT = new DataTable();
            var tgt_FMM_ModelGrps_DT = new DataTable();
            var src_FMM_ModelGrpAssign_DT = new DataTable();
            var tgt_FMM_ModelGrpAssign_DT = new DataTable();
            var src_FMM_CalcUnitConfig_DT = new DataTable();
            var tgt_FMM_CalcUnitConfig_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_CubeConfig_DT = new DataTable();
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var sqa = new SqlDataAdapter();
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                connection.Open();
                get_FMM_ActConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ActConfig_DT, ref tgt_FMM_ActConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprConfig_DT, ref tgt_FMM_ApprConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
                );

                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrps_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrps_DT, ref tgt_FMM_ModelGrps_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpAssign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrpAssign_DT, ref tgt_FMM_ModelGrpAssign_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcUnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcUnitConfig_DT, ref tgt_FMM_CalcUnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Unit_Assign_DT, ref tgt_FMM_Calc_Unit_Assign_DT, sql_gbl_get_max_id
                );

                foreach (DataRow activity_ConfigRow in src_FMM_ActConfig_DT.Rows)
                {
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_ActConfig_DT, tgt_CubeID, ref XFCopyTaskResult);
                    var curr_srcActivityID = activity_ConfigRow["ActID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["ActID"]
                                            : -1; // Handle null ActIDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["UnitID"] != DBNull.Value
                        ? (int)unit_ConfigRow["UnitID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyUnits(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"UnitID = {curr_srcUnitID}"))
                        {
                            CopyAccts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["RegConfigID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["RegConfigID"]
                        : -1; // Handle null UnitIDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"RegConfigID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow ConfigRow in src_FMM_ApprConfig_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = ConfigRow["ApprID"] != DBNull.Value
                        ? (int)ConfigRow["ApprID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyApprovals(ConfigRow, ref tgt_FMM_ApprConfig_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Appr_Step_Config_DT.Select($"ApprID = {curr_srcApprovalID}"))
                        {
                            Copy_Appr_Steps(approvalstep_ConfigRow, ref tgt_FMM_Appr_Step_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["ModelID"] != DBNull.Value
                        ? (int)models_ConfigRow["ModelID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyModels(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"ModelID = {curr_srcModelID}"))
                        {
                            var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                            ? (int)calc_ConfigRow["CalcID"]
                            : -1; // Handle null UnitIDs as needed
                            CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_CubeID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                foreach (var Model_GroupRow in src_FMM_ModelGrps_DT.Rows)
                {

                }
                foreach (var CalcUnitConfigRow in src_FMM_CalcUnitConfig_DT.Rows)
                {

                }
                cmdBuilder.UpdateTableSimple(si, "FMM_ActConfig", tgt_FMM_ActConfig_DT, sqa, "ActID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", tgt_FMM_Unit_Config_DT, sqa, "UnitID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", tgt_FMM_Acct_Config_DT, sqa, "AcctID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", tgt_FMM_Reg_Config_DT, sqa, "RegConfigID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa, "Col_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ApprConfig", tgt_FMM_ApprConfig_DT, sqa, "ApprID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", tgt_FMM_Appr_Step_Config_DT, sqa, "ApprStepID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Models", tgt_FMM_Models_DT, sqa, "ModelID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", tgt_FMM_Calc_Config_DT, sqa, "CalcID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", tgt_FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", tgt_FMM_Src_Cell_DT, sqa, "CellID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrpAssign", tgt_FMM_ModelGrpAssign_DT, sqa, "Model_Grp_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_CalcUnitConfig", tgt_FMM_CalcUnitConfig_DT, sqa, "CalcUnitID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", tgt_FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrps", tgt_FMM_ModelGrps_DT, sqa, "Model_Grp_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_CubeConfig", tgt_FMM_CubeConfig_DT, sqa, "CubeID");
            }
        }

        private void Process_Copy_ActConfig(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeID", "0"));
            var tgt_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeID", "0"));
            var src_ActID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ActID", "0"));
            var src_FMM_ActConfig_DT = new DataTable();
            var tgt_FMM_ActConfig_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_ApprConfig_DT = new DataTable();
            var tgt_FMM_ApprConfig_DT = new DataTable();
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
            var src_FMM_ModelGrps_DT = new DataTable();
            var tgt_FMM_ModelGrps_DT = new DataTable();
            var src_FMM_ModelGrpAssign_DT = new DataTable();
            var tgt_FMM_ModelGrpAssign_DT = new DataTable();
            var src_FMM_CalcUnitConfig_DT = new DataTable();
            var tgt_FMM_CalcUnitConfig_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_DT = new DataTable();
            var src_FMM_CubeConfig_DT = new DataTable();
            var tgt_FMM_CubeConfig_DT = new DataTable();
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new GBL_UI_Assembly.SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                connection.Open();
                get_FMM_ActConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ActConfig_DT, ref tgt_FMM_ActConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_ApprConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ApprConfig_DT, ref tgt_FMM_ApprConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Appr_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Appr_Step_Config_DT, ref tgt_FMM_Appr_Step_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_gbl_get_max_id
                );

                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
                );

                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrps_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrps_DT, ref tgt_FMM_ModelGrps_DT, sql_gbl_get_max_id
                );

                get_FMM_ModelGrpAssign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_ModelGrpAssign_DT, ref tgt_FMM_ModelGrpAssign_DT, sql_gbl_get_max_id
                );

                get_FMM_CalcUnitConfig_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_CalcUnitConfig_DT, ref tgt_FMM_CalcUnitConfig_DT, sql_gbl_get_max_id
                );

                get_FMM_Calc_Unit_Assign_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID } },
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Unit_Assign_DT, ref tgt_FMM_Calc_Unit_Assign_DT, sql_gbl_get_max_id
                );

                foreach (DataRow activity_ConfigRow in src_FMM_ActConfig_DT.Rows)
                {
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_ActConfig_DT, tgt_CubeID, ref XFCopyTaskResult);
                    var curr_srcActivityID = activity_ConfigRow["ActID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["ActID"]
                                            : -1; // Handle null ActIDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["UnitID"] != DBNull.Value
                        ? (int)unit_ConfigRow["UnitID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyUnits(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"UnitID = {curr_srcUnitID}"))
                        {
                            CopyAccts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["RegConfigID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["RegConfigID"]
                        : -1; // Handle null UnitIDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"RegConfigID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow ConfigRow in src_FMM_ApprConfig_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = ConfigRow["ApprID"] != DBNull.Value
                        ? (int)ConfigRow["ApprID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyApprovals(ConfigRow, ref tgt_FMM_ApprConfig_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Appr_Step_Config_DT.Select($"ApprID = {curr_srcApprovalID}"))
                        {
                            Copy_Appr_Steps(approvalstep_ConfigRow, ref tgt_FMM_Appr_Step_Config_DT, tgt_CubeID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"ActID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["ModelID"] != DBNull.Value
                        ? (int)models_ConfigRow["ModelID"]
                        : -1; // Handle null UnitIDs as needed
                        CopyModels(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_CubeID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"ModelID = {curr_srcModelID}"))
                        {
                            var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                            ? (int)calc_ConfigRow["CalcID"]
                            : -1; // Handle null UnitIDs as needed
                            CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_CubeID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                            }
                        }
                    }
                }
                foreach (var Model_GroupRow in src_FMM_ModelGrps_DT.Rows)
                {

                }
                foreach (var CalcUnitConfigRow in src_FMM_CalcUnitConfig_DT.Rows)
                {

                }

                cmdBuilder.UpdateTableSimple(si, "FMM_ActConfig", tgt_FMM_ActConfig_DT, sqa, "ActID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Unit_Config", tgt_FMM_Unit_Config_DT, sqa, "UnitID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Acct_Config", tgt_FMM_Acct_Config_DT, sqa, "AcctID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Reg_Config", tgt_FMM_Reg_Config_DT, sqa, "RegConfigID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Col_Config", tgt_FMM_Col_Config_DT, sqa, "Col_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ApprConfig", tgt_FMM_ApprConfig_DT, sqa, "ApprID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Appr_Step_Config", tgt_FMM_Appr_Step_Config_DT, sqa, "ApprStepID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Models", tgt_FMM_Models_DT, sqa, "ModelID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Config", tgt_FMM_Calc_Config_DT, sqa, "CalcID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Dest_Cell", tgt_FMM_Dest_Cell_DT, sqa, "Dest_Cell_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Src_Cell", tgt_FMM_Src_Cell_DT, sqa, "CellID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrpAssign", tgt_FMM_ModelGrpAssign_DT, sqa, "Model_Grp_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_CalcUnitConfig", tgt_FMM_CalcUnitConfig_DT, sqa, "CalcUnitID");
                cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", tgt_FMM_Calc_Unit_Assign_DT, sqa, "Calc_Unit_Assign_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_ModelGrps", tgt_FMM_ModelGrps_DT, sqa, "Model_Grp_Config_ID");
                cmdBuilder.UpdateTableSimple(si, "FMM_CubeConfig", tgt_FMM_CubeConfig_DT, sqa, "CubeID");
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
                string srcActivities_selectQuery = @"
                    SELECT CubeID, Name
                    FROM Cube
                    WHERE IsTopLevelCube = 1";
                var srcActivities_parameters = new SqlParameter[]
                {
                };
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, srcActivities_sqlDataAdapter, srcActivities_DT, srcActivities_selectQuery, srcActivities_parameters);

            }
            return XFCopyTaskResult;
        }

        private void Process_Calc_Copy(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CubeID", "0"));
            var tgt_CubeID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_CubeID", "0"));
            var src_ActID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ActID", "0"));
            var tgt_ActID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_ActID", "0"));
            var src_ModelID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_ModelID", "0"));
            var tgt_ModelID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_ModelID", "0"));
            var src_CalcIDs = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_CalcIDs", "0"));
            gbl_Curr_ActID = tgt_ActID;
            gbl_Curr_ModelID = tgt_ModelID;
            var src_FMM_Calc_Config_DT = new DataTable();
            var tgt_FMM_Calc_Config_DT = new DataTable();
            var src_FMM_Dest_Cell_DT = new DataTable();
            var tgt_FMM_Dest_Cell_DT = new DataTable();
            var src_FMM_Src_Cell_DT = new DataTable();
            var tgt_FMM_Src_Cell_DT = new DataTable();
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sql_gbl_get_max_id = new SQL_GBL_Get_Max_ID(si, connection);
                var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(si, connection);
                var sqa = new SqlDataAdapter();
                connection.Open();

                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = src_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_ModelID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_CalcIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = tgt_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_ModelID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_gbl_get_max_id
                );

                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = src_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_ModelID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_CalcIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = tgt_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_ModelID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_gbl_get_max_id
                );

                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = src_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_ModelID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_CalcIDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_CubeID },
                                         new SqlParameter("@ActID", SqlDbType.Int) { Value = tgt_ActID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_ModelID }},
                    sql_gbl_get_datasets,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_gbl_get_max_id
                );


                foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Rows)
                {
                    var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                    ? (int)calc_ConfigRow["CalcID"]
                    : -1; // Handle null UnitIDs as needed
                    CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_CubeID, ref XFCopyTaskResult, "Model Calc Copy");
                    foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                    {
                        CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                    }
                    foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                    {
                        Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_CubeID, ref XFCopyTaskResult);
                    }
                }

                cmdBuilder.UpdateTableSimple(si, "FMM_CalcConfig", tgt_FMM_Calc_Config_DT, sqa, "CalcID");
                cmdBuilder.UpdateTableSimple(si, "FMM_DestCell", tgt_FMM_Dest_Cell_DT, sqa, "CellID");
                cmdBuilder.UpdateTableSimple(si, "FMM_SrcCell", tgt_FMM_Src_Cell_DT, sqa, "CellID");

            }
        }

        private void get_FMM_ActConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ActConfig_DT, ref DataTable tgt_FMM_ActConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ActConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ActConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ActConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ActConfig_DT, sql, tgt_sqlparams);

            gbl_ActID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ActConfig", "ActID");
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
            gbl_UnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Unit_Config", "UnitID");
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
            gbl_AcctID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Acct_Config", "AcctID");
        }

        private void get_FMM_ApprConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ApprConfig_DT, ref DataTable tgt_FMM_ApprConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ApprConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ApprConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ApprConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ApprConfig_DT, sql, tgt_sqlparams);
            gbl_ApprID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ApprConfig", "ApprID");
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

            gbl_ApprStepID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Appr_Step_Config", "ApprStepID");
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

            gbl_RegConfigID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Reg_Config", "RegConfigID");
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

            gbl_Col_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
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

            gbl_ModelID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Models", "ModelID");
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

            gbl_CalcID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Config", "CalcID");
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

            gbl_Dest_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Dest_Cell", "Dest_Cell_ID");
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

            gbl_Src_Cell_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Src_Cell", "CellID");
        }

        private void get_FMM_ModelGrps_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ModelGrps_DT, ref DataTable tgt_FMM_ModelGrps_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ModelGrps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ModelGrps_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ModelGrps {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ModelGrps_DT, sql, tgt_sqlparams);

            gbl_ModelGrps_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrps", "Model_Grp_ID");
        }

        private void get_FMM_ModelGrpAssign_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_ModelGrpAssign_DT, ref DataTable tgt_FMM_ModelGrpAssign_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_ModelGrpAssign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_ModelGrpAssign_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_ModelGrpAssign {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_ModelGrpAssign_DT, sql, tgt_sqlparams);

            gbl_Model_Grp_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_ModelGrpAssign", "Model_Grp_Assign_ID");
        }

        private void get_FMM_CalcUnitConfig_Data(SqlParameter[] src_sqlparams, SqlParameter[] tgt_sqlparams, SQL_GBL_Get_DataSets sql_gbl_get_datasets, ref DataTable src_FMM_CalcUnitConfig_DT, ref DataTable tgt_FMM_CalcUnitConfig_DT, SQL_GBL_Get_Max_ID sql_gbl_get_max_id)
        {
            var sqa = new SqlDataAdapter();
            var whereClause = Construct_Where_Clause(src_sqlparams);
            var sql = $@"SELECT * FROM FMM_CalcUnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, src_FMM_CalcUnitConfig_DT, sql, src_sqlparams);

            whereClause = Construct_Where_Clause(tgt_sqlparams);
            sql = $@"SELECT * FROM FMM_CalcUnitConfig {whereClause}";
            sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, tgt_FMM_CalcUnitConfig_DT, sql, tgt_sqlparams);

            gbl_CalcUnitID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_CalcUnitConfig", "CalcUnitID");
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

            gbl_Calc_Unit_Assign_ID = sql_gbl_get_max_id.Get_Max_ID(si, "FMM_Calc_Unit_Assign", "Calc_Unit_Assign_ID");
        }

        private void CopyActivities(DataRow src_FMM_ActConfig_Row, ref DataTable tgt_FMM_ActConfig_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_ActConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_ActConfig_Row["Name"].ToString() &&
                            row["CalcType"].ToString() == src_FMM_ActConfig_Row["CalcType"].ToString());

            if (!isDuplicate)
            {
                gbl_ActID += 1;
                gbl_Curr_ActID = gbl_ActID;
                DataRow new_DestDataRow = tgt_FMM_ActConfig_DT.NewRow();

                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_ActID;
                new_DestDataRow["Name"] = src_FMM_ActConfig_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["CalcType"] = src_FMM_ActConfig_Row.Field<string>("CalcType") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as "Build"
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set the appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set the appropriate user context

                tgt_FMM_ActConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_ActConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_ActConfig_Row["Name"].ToString() &&
                                           row["CalcType"].ToString() == src_FMM_ActConfig_Row["CalcType"].ToString());
                if (existing_DataRow != null)
                {
                    gbl_Curr_ActID = existing_DataRow.Field<int>("ActID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set the appropriate user context
                }
            }
        }
        private void CopyUnits(DataRow src_FMM_Unit_Config_Row, ref DataTable tgt_FMM_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_FMM_Unit_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString());

            if (!isDuplicate)
            {
                gbl_UnitID += 1;
                gbl_Curr_UnitID = gbl_UnitID;
                DataRow new_DestDataRow = tgt_FMM_Unit_Config_DT.NewRow();

                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["UnitID"] = gbl_UnitID;
                new_DestDataRow["Name"] = src_FMM_Unit_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_FMM_Unit_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_UnitID = existing_DataRow.Field<int>("UnitID");
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void CopyAccts(DataRow src_FMM_Acct_Config_Row, ref DataTable tgt_FMM_Acct_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_FMM_Acct_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["UnitID"].ToString() == gbl_Curr_UnitID.ToString());

            if (!isDuplicate)
            {
                gbl_AcctID += 1;
                gbl_Curr_AcctID = gbl_AcctID;
                DataRow new_DestDataRow = tgt_FMM_Acct_Config_DT.NewRow();

                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["UnitID"] = gbl_Curr_UnitID;
                new_DestDataRow["AcctID"] = gbl_AcctID;
                new_DestDataRow["Name"] = src_FMM_Acct_Config_Row.Field<string>("AcctName") ?? string.Empty; // Handle nulls
                new_DestDataRow["MapLogic"] = src_FMM_Acct_Config_Row.Field<string>("MapLogic") ?? string.Empty; // Handle nulls
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_Acct_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_Acct_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                           row["UnitID"].ToString() == gbl_Curr_UnitID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_AcctID = existing_DataRow.Field<int>("AcctID");

                    existing_DataRow["MapLogic"] = src_FMM_Acct_Config_Row.Field<string>("MapLogic") ?? existing_DataRow.Field<string>("MapLogic");
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Reg_Config(DataRow src_Reg_Config_Row, ref DataTable tgt_Reg_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Reg_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString());

            if (!isDuplicate)
            {
                gbl_RegConfigID += 1;
                gbl_Curr_RegConfigID = gbl_RegConfigID;
                DataRow new_DestDataRow = tgt_Reg_Config_DT.NewRow();

                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["RegConfigID"] = gbl_RegConfigID;
                new_DestDataRow["Name"] = src_Reg_Config_Row.Field<string>("Name") ?? string.Empty; // Handle nulls
                new_DestDataRow["TimePhasing"] = src_Reg_Config_Row.Field<string>("TimePhasing") ?? string.Empty; // Handle nulls
                new_DestDataRow["StartDtSrc"] = src_Reg_Config_Row.Field<string>("StartDtSrc") ?? string.Empty; // Handle nulls
                new_DestDataRow["EndDtSrc"] = src_Reg_Config_Row.Field<string>("EndDtSrc") ?? string.Empty; // Handle nulls
                new_DestDataRow["ApprConfig"] = src_Reg_Config_Row.Field<string>("ApprConfig") ?? string.Empty; // Handle nulls
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_Reg_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Reg_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_RegConfigID = existing_DataRow.Field<int>("RegConfigID");

                    existing_DataRow["TimePhasing"] = src_Reg_Config_Row.Field<string>("TimePhasing") ?? existing_DataRow.Field<string>("TimePhasing");
                    existing_DataRow["StartDtSrc"] = src_Reg_Config_Row.Field<string>("StartDtSrc") ?? existing_DataRow.Field<string>("StartDtSrc");
                    existing_DataRow["EndDtSrc"] = src_Reg_Config_Row.Field<string>("EndDtSrc") ?? existing_DataRow.Field<string>("EndDtSrc");
                    existing_DataRow["ApprConfig"] = src_Reg_Config_Row.Field<string>("ApprConfig") ?? existing_DataRow.Field<string>("ApprConfig");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Col_Config(DataRow src_Col_Config_Row, ref DataTable tgt_Col_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Col_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["RegConfigID"].ToString() == gbl_Curr_RegConfigID.ToString());

            if (!isDuplicate)
            {
                gbl_Col_ID += 1;
                gbl_Curr_Col_ID = gbl_Col_ID;
                DataRow new_DestDataRow = tgt_Col_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["RegConfigID"] = gbl_Curr_RegConfigID;
                new_DestDataRow["Col_ID"] = gbl_Col_ID;
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
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Col_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Col_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                                    row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                    row["RegConfigID"].ToString() == gbl_Curr_RegConfigID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_Col_ID = existing_DataRow.Field<int>("Col_ID");
                    existing_DataRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                    existing_DataRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                    existing_DataRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                    existing_DataRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                    existing_DataRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                    existing_DataRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                    existing_DataRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                    existing_DataRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyApprovals(DataRow src_FMM_ApprConfig_Row, ref DataTable tgt_FMM_ApprConfig_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_ApprConfig_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_ApprConfig_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString());

            if (!isDuplicate)
            {
                gbl_ApprID += 1;
                gbl_Curr_ApprID = gbl_ApprID;
                DataRow new_DestDataRow = tgt_FMM_ApprConfig_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["ApprID"] = gbl_ApprID;
                new_DestDataRow["Name"] = src_FMM_ApprConfig_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_ApprConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_ApprConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_ApprConfig_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_ApprID = existing_DataRow.Field<int>("ApprID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Appr_Steps(DataRow src_FMM_Appr_Step_Config_Row, ref DataTable tgt_FMM_Appr_Step_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_FMM_Appr_Step_Config_DT.AsEnumerable()
                .Any(row => row["StepNum"].ToString() == src_FMM_Appr_Step_Config_Row["StepNum"].ToString() &&
                            row["WFProfile_Step"].ToString() == src_FMM_Appr_Step_Config_Row["WFProfile_Step"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["ApprID"].ToString() == gbl_Curr_ApprID.ToString());

            if (!isDuplicate)
            {
                gbl_ApprStepID += 1;
                gbl_Curr_ApprStepID = gbl_ApprStepID;
                DataRow new_DestDataRow = tgt_FMM_Appr_Step_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_ActID;
                new_DestDataRow["ApprID"] = gbl_ApprID;
                new_DestDataRow["ApprStepID"] = gbl_ApprStepID;
                new_DestDataRow["StepNum"] = src_FMM_Appr_Step_Config_Row.Field<int?>("StepNum") ?? 0;
                new_DestDataRow["WFProfile_Step"] = src_FMM_Appr_Step_Config_Row.Field<Guid?>("WFProfile_Step") ?? Guid.Empty;
                new_DestDataRow["UserGroup"] = src_FMM_Appr_Step_Config_Row.Field<string>("UserGroup") ?? string.Empty;
                new_DestDataRow["Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Logic") ?? string.Empty;
                new_DestDataRow["Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Item") ?? string.Empty;
                new_DestDataRow["Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Level") ?? 0;
                new_DestDataRow["ApprConfig"] = src_FMM_Appr_Step_Config_Row.Field<int?>("ApprConfig") ?? 0;
                new_DestDataRow["InitStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                new_DestDataRow["ApprStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("ApprStatus") ?? string.Empty;
                new_DestDataRow["RejStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("RejStatus") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_FMM_Appr_Step_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_FMM_Appr_Step_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["StepNum"].ToString() == src_FMM_Appr_Step_Config_Row["StepNum"].ToString() &&
                                           row["WFProfile_Step"].ToString() == src_FMM_Appr_Step_Config_Row["WFProfile_Step"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                           row["ApprID"].ToString() == gbl_Curr_ApprID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_ApprStepID = existing_DataRow.Field<int>("ApprStepID");
                    existing_DataRow["UserGroup"] = src_FMM_Appr_Step_Config_Row.Field<string>("UserGroup") ?? string.Empty;
                    existing_DataRow["Logic"] = src_FMM_Appr_Step_Config_Row.Field<string>("Logic") ?? string.Empty;
                    existing_DataRow["Item"] = src_FMM_Appr_Step_Config_Row.Field<string>("Item") ?? string.Empty;
                    existing_DataRow["Level"] = src_FMM_Appr_Step_Config_Row.Field<int?>("Level") ?? 0;
                    existing_DataRow["ApprConfig"] = src_FMM_Appr_Step_Config_Row.Field<int?>("ApprConfig") ?? 0;
                    existing_DataRow["InitStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                    existing_DataRow["ApprStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("ApprStatus") ?? string.Empty;
                    existing_DataRow["RejStatus"] = src_FMM_Appr_Step_Config_Row.Field<string>("RejStatus") ?? string.Empty;
                    existing_DataRow["Status"] = row_Status; // Set initial status as appropriate
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyModels(DataRow src_Model_Config_Row, ref DataTable tgt_Model_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Model_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString());

            if (!isDuplicate)
            {
                gbl_ModelID += 1;
                gbl_Curr_ModelID = gbl_ModelID;
                DataRow new_DestDataRow = tgt_Model_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["ModelID"] = gbl_ModelID;
                new_DestDataRow["Name"] = src_Model_Config_Row.Field<string>("Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_ModelID = existing_DataRow.Field<int>("ModelID");
                    existing_DataRow["Status"] = row_Status; // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyCalcs(DataRow src_Calc_Config_Row, ref DataTable tgt_Calc_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult, string runType)
        {
            var row_Status = "Build";
            bool isDuplicate = tgt_Calc_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                            row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["ModelID"].ToString() == gbl_Curr_ModelID.ToString());

            if (runType == "Model Calc Copy" || !isDuplicate)
            {
                gbl_CalcID += 1;
                gbl_Curr_CalcID = gbl_CalcID;

                DataRow new_DestDataRow = tgt_Calc_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["ModelID"] = gbl_Curr_ModelID;
                new_DestDataRow["CalcID"] = gbl_CalcID;
                if (runType == "Model Calc Copy")
                {
                    string calcName = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;

                    new_DestDataRow["Name"] = calcName + " - Copy";
                }
                else
                {
                    new_DestDataRow["Name"] = src_Calc_Config_Row.Field<string>("Name") ?? string.Empty;
                }
                new_DestDataRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                new_DestDataRow["CalcCondition"] = src_Calc_Config_Row.Field<string>("CalcCondition") ?? string.Empty;
                new_DestDataRow["CalcExplanation"] = src_Calc_Config_Row.Field<string>("CalcExplanation") ?? string.Empty;
                new_DestDataRow["BalancedBuffer"] = src_Calc_Config_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                new_DestDataRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                new_DestDataRow["UnbalCalc"] = src_Calc_Config_Row.Field<string>("UnbalCalc") ?? string.Empty;
                new_DestDataRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                new_DestDataRow["TimePhasing"] = src_Calc_Config_Row.Field<string>("TimePhasing") ?? string.Empty;
                new_DestDataRow["InputFrequency"] = src_Calc_Config_Row.Field<string>("InputFrequency") ?? string.Empty;
                new_DestDataRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                new_DestDataRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                new_DestDataRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                new_DestDataRow["Status"] = row_Status;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Calc_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Calc_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                                           row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                           row["ModelID"].ToString() == gbl_Curr_ModelID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_CalcID = existing_DataRow.Field<int>("CalcID");
                    existing_DataRow["Status"] = row_Status;
                    existing_DataRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                    existing_DataRow["CalcCondition"] = src_Calc_Config_Row.Field<string>("CalcCondition") ?? string.Empty;
                    existing_DataRow["CalcExplanation"] = src_Calc_Config_Row.Field<string>("CalcExplanation") ?? string.Empty;
                    existing_DataRow["BalancedBuffer"] = src_Calc_Config_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                    existing_DataRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                    existing_DataRow["UnbalCalc"] = src_Calc_Config_Row.Field<string>("UnbalCalc") ?? string.Empty;
                    existing_DataRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                    existing_DataRow["TimePhasing"] = src_Calc_Config_Row.Field<string>("TimePhasing") ?? string.Empty;
                    existing_DataRow["InputFrequency"] = src_Calc_Config_Row.Field<string>("InputFrequency") ?? string.Empty;
                    existing_DataRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                    existing_DataRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                    existing_DataRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void CopyCell(DataRow src_Cell_Config_Row, ref DataTable tgt_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Cell_Config_DT.AsEnumerable()
                .Any(row => row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["ModelID"].ToString() == gbl_Curr_ModelID.ToString() &&
                            row["CalcID"].ToString() == gbl_Curr_CalcID.ToString());

            if (!isDuplicate)
            {
                gbl_Dest_Cell_ID += 1;
                gbl_Curr_Dest_Cell_ID = gbl_Dest_Cell_ID;

                DataRow new_DestDataRow = tgt_Cell_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["ModelID"] = gbl_Curr_ModelID;
                new_DestDataRow["CalcID"] = gbl_Curr_CalcID;
                new_DestDataRow["Dest_Cell_ID"] = gbl_Dest_Cell_ID;

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
                new_DestDataRow["TimeFilter"] = src_Cell_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                new_DestDataRow["AcctFilter"] = src_Cell_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                new_DestDataRow["OriginFilter"] = src_Cell_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                new_DestDataRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                new_DestDataRow["FlowFilter"] = src_Cell_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                new_DestDataRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                new_DestDataRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                new_DestDataRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                new_DestDataRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                new_DestDataRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                new_DestDataRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                new_DestDataRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                new_DestDataRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                new_DestDataRow["ConditionalFilter"] = src_Cell_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                new_DestDataRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                new_DestDataRow["BufferFilter"] = src_Cell_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                new_DestDataRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                new_DestDataRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;

                tgt_Cell_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                           row["ModelID"].ToString() == gbl_Curr_ModelID.ToString() &&
                                           row["CalcID"].ToString() == gbl_Curr_CalcID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_Dest_Cell_ID = existing_DataRow.Field<int>("Dest_Cell_ID");
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
                    existing_DataRow["TimeFilter"] = src_Cell_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                    existing_DataRow["AcctFilter"] = src_Cell_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                    existing_DataRow["OriginFilter"] = src_Cell_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                    existing_DataRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                    existing_DataRow["FlowFilter"] = src_Cell_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                    existing_DataRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                    existing_DataRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                    existing_DataRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                    existing_DataRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                    existing_DataRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                    existing_DataRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                    existing_DataRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                    existing_DataRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                    existing_DataRow["ConditionalFilter"] = src_Cell_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                    existing_DataRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                    existing_DataRow["BufferFilter"] = src_Cell_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                    existing_DataRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                    existing_DataRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;
                }
            }
        }
        private void Copy_Src_Cell(DataRow src_Src_Cell_Config_Row, ref DataTable tgt_Src_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            gbl_Src_Cell_ID += 1;
            gbl_Curr_Src_Cell_ID = gbl_Src_Cell_ID;
            bool isDuplicate = tgt_Src_Cell_Config_DT.AsEnumerable()
                .Any(row => row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                            row["ModelID"].ToString() == gbl_Curr_ModelID.ToString() &&
                            row["CalcID"].ToString() == gbl_Curr_CalcID.ToString() &&
                            row["CellID"].ToString() == gbl_Curr_Src_Cell_ID.ToString());

            if (!isDuplicate)
            {
                DataRow new_DestDataRow = tgt_Src_Cell_Config_DT.NewRow();

                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = gbl_Curr_ActID;
                new_DestDataRow["ModelID"] = gbl_Curr_ModelID;
                new_DestDataRow["CalcID"] = gbl_Curr_CalcID;
                new_DestDataRow["CellID"] = gbl_Src_Cell_ID;
                new_DestDataRow["SrcOrder"] = src_Src_Cell_Config_Row.Field<int?>("SrcOrder") ?? 0;
                new_DestDataRow["SrcType"] = src_Src_Cell_Config_Row.Field<string>("SrcType") ?? string.Empty;
                new_DestDataRow["SrcItem"] = src_Src_Cell_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                new_DestDataRow["OpenParens"] = src_Src_Cell_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                new_DestDataRow["MathOperator"] = src_Src_Cell_Config_Row.Field<string>("MathOperator") ?? string.Empty;
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
                new_DestDataRow["CloseParens"] = src_Src_Cell_Config_Row.Field<string>("CloseParens") ?? string.Empty;
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
                new_DestDataRow["OverrideValue"] = src_Src_Cell_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                new_DestDataRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                new_DestDataRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                new_DestDataRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                new_DestDataRow["MapType"] = src_Src_Cell_Config_Row.Field<string>("MapType") ?? string.Empty;
                new_DestDataRow["MapSource"] = src_Src_Cell_Config_Row.Field<string>("MapSource") ?? string.Empty;
                new_DestDataRow["MapLogic"] = src_Src_Cell_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                new_DestDataRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                new_DestDataRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                new_DestDataRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName; // Set appropriate user context
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_Src_Cell_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Src_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["ActID"].ToString() == gbl_Curr_ActID.ToString() &&
                                           row["ModelID"].ToString() == gbl_Curr_ModelID.ToString() &&
                                           row["CalcID"].ToString() == gbl_Curr_CalcID.ToString() &&
                                           row["CellID"].ToString() == gbl_Curr_Src_Cell_ID.ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_Src_Cell_ID = Convert.ToInt32(existing_DataRow["CellID"].ToString());
                    existing_DataRow["SrcOrder"] = src_Src_Cell_Config_Row.Field<int?>("SrcOrder") ?? 0;
                    existing_DataRow["SrcType"] = src_Src_Cell_Config_Row.Field<string>("SrcType") ?? string.Empty;
                    existing_DataRow["SrcItem"] = src_Src_Cell_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                    existing_DataRow["OpenParens"] = src_Src_Cell_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                    existing_DataRow["MathOperator"] = src_Src_Cell_Config_Row.Field<string>("MathOperator") ?? string.Empty;
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
                    existing_DataRow["CloseParens"] = src_Src_Cell_Config_Row.Field<string>("CloseParens") ?? string.Empty;
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
                    existing_DataRow["OverrideValue"] = src_Src_Cell_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                    existing_DataRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                    existing_DataRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                    existing_DataRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                    existing_DataRow["MapType"] = src_Src_Cell_Config_Row.Field<string>("MapType") ?? string.Empty;
                    existing_DataRow["MapSource"] = src_Src_Cell_Config_Row.Field<string>("MapSource") ?? string.Empty;
                    existing_DataRow["MapLogic"] = src_Src_Cell_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                    existing_DataRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                    existing_DataRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                    existing_DataRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_ModelGrps(DataRow src_Model_Grp_Config_Row, ref DataTable tgt_Model_Grp_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Model_Grp_Config_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                            row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

            if (!isDuplicate)
            {
                gbl_ModelGrps_ID += 1;
                gbl_Curr_ModelGrps_ID = gbl_ModelGrps_ID;
                DataRow new_DestDataRow = tgt_Model_Grp_Config_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = targetCubeID;
                new_DestDataRow["Model_Grp_Config_ID"] = gbl_ModelGrps_ID;
                new_DestDataRow["GroupName"] = src_Model_Grp_Config_Row["GroupName"].ToString();
                new_DestDataRow["GroupType"] = src_Model_Grp_Config_Row["GroupType"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Grp_Config_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Grp_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                                           row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_ModelGrps_ID = Convert.ToInt32(existing_DataRow["Model_Grp_Config_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Model_Grp_Assign_Model(DataRow src_Model_Grp_Assign_Model_Row, ref DataTable tgt_Model_Grp_Assign_Model_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                            row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

            if (!isDuplicate)
            {
                gbl_Model_Grp_Assign_ID += 1;
                gbl_Curr_Model_Grp_Assign_ID = gbl_Model_Grp_Assign_ID;
                DataRow new_DestDataRow = tgt_Model_Grp_Assign_Model_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = targetCubeID;
                new_DestDataRow["Model_Grp_Assign_ID"] = gbl_Model_Grp_Assign_ID;
                new_DestDataRow["GroupName"] = src_Model_Grp_Assign_Model_Row["GroupName"].ToString();
                new_DestDataRow["ModelName"] = src_Model_Grp_Assign_Model_Row["ModelName"].ToString();
                new_DestDataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Model_Grp_Assign_Model_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                                           row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_Model_Grp_Assign_ID = Convert.ToInt32(existing_DataRow["Model_Grp_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Model_Grp_Assign_Model_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_CalcUnitConfig(DataRow src_CalcUnitConfig_Row, ref DataTable tgt_CalcUnitConfig_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_CalcUnitConfig_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_CalcUnitConfig_Row["DU_Name"].ToString() &&
                            row["DU_Type"].ToString() == src_CalcUnitConfig_Row["DU_Type"].ToString());

            if (!isDuplicate)
            {
                gbl_CalcUnitID += 1;
                gbl_Curr_CalcUnitID = gbl_CalcUnitID;
                DataRow new_DestDataRow = tgt_CalcUnitConfig_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = targetCubeID;
                new_DestDataRow["CalcUnitID"] = gbl_CalcUnitID;
                new_DestDataRow["DU_Name"] = src_CalcUnitConfig_Row["DU_Name"].ToString();
                new_DestDataRow["DU_Type"] = src_CalcUnitConfig_Row["DU_Type"].ToString();
                new_DestDataRow["Description"] = src_CalcUnitConfig_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build";
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_CalcUnitConfig_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_CalcUnitConfig_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_CalcUnitConfig_Row["DU_Name"].ToString() &&
                                           row["DU_Type"].ToString() == src_CalcUnitConfig_Row["DU_Type"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_CalcUnitID = Convert.ToInt32(existing_DataRow["CalcUnitID"].ToString());
                    existing_DataRow["Description"] = src_CalcUnitConfig_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_CalcUnitConfig_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }
        private void Copy_Calc_Unit_Assign(DataRow src_Calc_Unit_Assign_Row, ref DataTable tgt_Calc_Unit_Assign_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            bool isDuplicate = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                            row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

            if (!isDuplicate)
            {
                gbl_Calc_Unit_Assign_ID += 1;
                gbl_Curr_Calc_Unit_Assign_ID = gbl_Calc_Unit_Assign_ID;
                DataRow new_DestDataRow = tgt_Calc_Unit_Assign_DT.NewRow();
                new_DestDataRow["CubeID"] = targetCubeID;
                new_DestDataRow["ActID"] = targetCubeID;
                new_DestDataRow["Calc_Unit_Assign_ID"] = gbl_Calc_Unit_Assign_ID;
                new_DestDataRow["DU_Name"] = src_Calc_Unit_Assign_Row["DU_Name"].ToString();
                new_DestDataRow["Model_Grp_Name"] = src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString();
                new_DestDataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString();
                new_DestDataRow["Status"] = "Build"; // Set initial status as appropriate
                new_DestDataRow["CreateDate"] = DateTime.Now;
                new_DestDataRow["CreateUser"] = si.UserName;
                new_DestDataRow["UpdateDate"] = DateTime.Now;
                new_DestDataRow["UpdateUser"] = si.UserName;
                tgt_Calc_Unit_Assign_DT.Rows.Add(new_DestDataRow);
            }
            else
            {
                DataRow existing_DataRow = tgt_Calc_Unit_Assign_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Row["DU_Name"].ToString() &&
                                           row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Row["Model_Grp_Name"].ToString());

                if (existing_DataRow != null)
                {
                    gbl_Curr_Calc_Unit_Assign_ID = Convert.ToInt32(existing_DataRow["Calc_Unit_Assign_ID"].ToString());
                    existing_DataRow["Description"] = src_Calc_Unit_Assign_Row["Description"].ToString(); // Update description or other fields as needed
                    existing_DataRow["Status"] = src_Calc_Unit_Assign_Row["Status"].ToString(); // Update status or other fields as needed
                    existing_DataRow["UpdateDate"] = DateTime.Now;
                    existing_DataRow["UpdateUser"] = si.UserName;
                }
            }
        }

        private string Construct_Where_Clause(SqlParameter[] sqlparams)
        {
            if (sqlparams == null || sqlparams.Length == 0)
            {
                return string.Empty;
            }

            var whereClause = new List<string>();

            foreach (var param in sqlparams)
            {
                var columnName = param.ParameterName.TrimStart('@');

                if (param.Value == DBNull.Value || param.Value == null)
                {
                    whereClause.Add($"{columnName} IS NULL");
                }
                else
                {
                    if (param.SqlDbType == SqlDbType.NVarChar && param.Value is string stringValue)
                    {
                        if (stringValue.Contains(","))
                        {
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
                        whereClause.Add($"{columnName} = @{columnName}");
                    }
                }
            }

            return "WHERE " + string.Join(" AND ", whereClause);
        }


        private void Duplicate_Model_List(int CubeID, int ActID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result)
        {
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_Models_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_Models
                                        WHERE CubeID = @CubeID
                                        AND ActID = @ActID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID},
                                new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID}
                            };

                            try
                            {
                                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, FMM_Models_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}";
                                return;
                            }

                            foreach (DataRow cube_Model_Row in FMM_Models_DT.Rows)
                            {
                                string ModelKey = CubeID + "|" + ActID + "|" + (int)cube_Model_Row["ModelID"];
                                gbl_Models_Dict.Add(ModelKey, (string)cube_Model_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Model_Grp_List(int CubeID, string dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult save_Result)
        {
            try
            {
                switch (dup_Process_Step)
                {
                    case "Initiate":
                        var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                        using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                        {
                            var SQL_GBL_Get_DataSets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                            connection.Open();

                            var sqa = new SqlDataAdapter();
                            var FMM_ModelGrps_DT = new DataTable();
                            var sql = @"SELECT *
                                        FROM FMM_ModelGrps
                                        WHERE CubeID = @CubeID";
                            var sqlparams = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID}
                            };

                            try
                            {
                                SQL_GBL_Get_DataSets.Fill_Get_GBL_DT(si, sqa, FMM_ModelGrps_DT, sql, sqlparams);
                            }
                            catch (Exception ex)
                            {
                                save_Result.IsOK = false;
                                save_Result.ShowMessageBox = true;
                                save_Result.Message = $"Error fetching data for CubeID {CubeID}";
                                return; // Exit the process if data retrieval fails
                            }

                            foreach (DataRow FMM_Model_Grp_Row in FMM_ModelGrps_DT.Rows)
                            {
                                var model_GroupKey = CubeID + "|" + (int)FMM_Model_Grp_Row["Model_Grp_ID"];
                                gbl_ModelGrps_Dict.Add(model_GroupKey, (string)FMM_Model_Grp_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                save_Result.IsOK = false;
                save_Result.ShowMessageBox = true;
                save_Result.Message = $"An unexpected error occurred.";
            }
        }


    }
}