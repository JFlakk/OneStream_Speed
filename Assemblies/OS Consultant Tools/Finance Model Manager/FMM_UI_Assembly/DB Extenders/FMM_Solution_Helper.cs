using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Solution_Helper
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        public int Global_FMM_Act_ID { get; set; }
        public int Global_Curr_FMM_Act_ID { get; set; }
        public int Global_FMM_Unit_ID { get; set; }
        public int Global_Curr_FMM_Unit_ID { get; set; }
        public int Global_FMM_Acct_ID { get; set; }
        public int Global_Curr_FMM_Acct_ID { get; set; }
        public int Global_FMM_Appr_ID { get; set; }
        public int Global_Curr_FMM_Appr_ID { get; set; }
        public int Global_FMM_Appr_Step_ID { get; set; }
        public int Global_Curr_FMM_Appr_Step_ID { get; set; }
        public int Global_FMM_Reg_Config_ID { get; set; }
        public int Global_Curr_FMM_Reg_Config_ID { get; set; }
        public int Global_FMM_Col_ID { get; set; }
        public int Global_Curr_FMM_Col_ID { get; set; }
        public int Global_FMM_Models_ID { get; set; }
        public int Global_Curr_FMM_Models_ID { get; set; }
        public int Global_FMM_Calc_ID { get; set; }
        public int Global_Curr_FMM_Calc_ID { get; set; }
        public int Global_FMM_Dest_Cell_ID { get; set; }
        public int Global_Curr_FMM_Dest_Cell_ID { get; set; }
        public int Global_FMM_Src_Cell_ID { get; set; }
        public int Global_Curr_FMM_Src_Cell_ID { get; set; }
        public int Global_FMM_Model_Grps_ID { get; set; }
        public int Global_Curr_FMM_Model_Grps_ID { get; set; }
        public int Global_FMM_Model_Grp_Assign_Model_ID { get; set; }
        public int Global_Curr_FMM_Model_Grp_Assign_Model_ID { get; set; }
        public int Global_FMM_Calc_Unit_Config_ID { get; set; }
        public int Global_Curr_FMM_Calc_Unit_Config_ID { get; set; }
        public int Global_FMM_Calc_Unit_Assign_Model_Grp_ID { get; set; }
        public int Global_Curr_FMM_Calc_Unit_Assign_Model_Grp_ID { get; set; }
        public Dictionary<string, string> Global_Models_Dict { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> Global_Model_Groups_Dict { get; set; } = new Dictionary<string, string>();
        public bool Global_Duplicate_Models { get; set; } = false;
        public bool Global_Duplicate_Model_Groups { get; set; } = false;


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
                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        var changed_Result = new XFSelectionChangedTaskResult();
                        if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Cube_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Cube_Config("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Cube_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Cube_Config("Update");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = SaveModel("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Model_Updates"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = SaveModel("Update");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Group"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Model_Group("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Model_Group"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Model_Group("Update");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Grp_Seq"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Model_Grp_Seq("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_Updated_Model_Grp_Seq"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Model_Grp_Seq("Update");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Model_Grp_Model_Assignments"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Model_Grp_Model_Assignments("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Calc_Unit_Model_Grp_Assignments"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Calc_Unit_Model_Grp_Assignments("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Approval_Step"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Sve Approval step: ");
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Save_Approval_Step("New");
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Save_New_Approval_Step_Activity_Config"))
                        {

                            // Implement Dashboard Component Selection Changed logic here.
                            var runType = args.NameValuePairs.XFGetValue("RunType");

                            //							var runType = AddUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "New";

                            BRApi.ErrorLog.LogMessage(si, "Hit Sve Approval step activity: " + runType);

                            changed_Result = Save_Approval_Step_Activity_Config(runType);
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Bulk_WF_DU"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Process_Bulk_WF_DU();
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Cube_Config_Copy"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Cube_Config_Copy(ref changed_Result);
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Copy_Reg_Config"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Process_Bulk_WF_DU();
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Activity_Config_Copy"))
                        {
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Activity_Config_Copy(ref changed_Result);
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Model_Copy"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Process_Bulk_WF_DU();
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Process_Calc_Copy"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            Process_Calc_Copy(ref changed_Result);
                            return changed_Result;
                        }
                        else if (args.FunctionName.XFEqualsIgnoreCase("Copy_Model_Grp_Config"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit: " + args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Appr_ID", "0"));
                            // Implement Dashboard Component Selection Changed logic here.
                            changed_Result = Process_Bulk_WF_DU();
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
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var Cube = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_All_Cubes", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Cube", String.Empty));
                var ScenType = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_FMM_Cube_Config_Scen_Types", args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Scen_Type", String.Empty));
                var agg_Consol = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Cube_Config_Agg_Consol", String.Empty);
                var entity_Dim = String.Empty;
                var entity_MFB = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Entity_MFB", String.Empty);
                var cube_Description = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_Config_Description", String.Empty);
                int new_Cube_ID = 0;

                BRApi.ErrorLog.LogMessage(si, "hit: " + Cube);
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    connection.Open();

                    // Create a new DataTable to check for existing records
                    var sql_DataSet_DataAdapter = new SqlDataAdapter();
                    var FMM_Cube_Config_Count_DT = new DataTable();

                    // Query to check if the cube config already exists
                    string select_DS_Query = @"
		                SELECT Count(*) as Count
		                FROM FMM_Cube_Config
		                WHERE Cube = @Cube
		                AND ScenType = @ScenType";

                    SqlParameter[] ds_parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.NVarChar, 50) { Value = Cube },
                        new SqlParameter("@ScenType", SqlDbType.NVarChar, 20) { Value = ScenType }
                    };

                    sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sql_DataSet_DataAdapter, FMM_Cube_Config_Count_DT, select_DS_Query, ds_parameters);

                    // If no existing record and RunType is "New", insert new record
                    if (RunType == "New")
                    {
                        // Get the next CubeID for the new record
                        new_Cube_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Cube_Config", "CubeID");

                        var sqa_fmm_cube_config = new SQA_FMM_Cube_Config(si, connection);
                        var FMM_Cube_Config_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Insert new cube config record
                        string selectQuery = "SELECT * FROM FMM_Cube_Config WHERE CubeID = @CubeID";
                        SqlParameter[] parameters = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = new_Cube_ID }
                        };

                        sqa_fmm_cube_config.Fill_FMM_Cube_Config_DataTable(si, sqlDataAdapter, FMM_Cube_Config_DT, selectQuery, parameters);

                        var newRow = FMM_Cube_Config_DT.NewRow();
                        newRow["CubeID"] = new_Cube_ID;
                        newRow["Cube"] = Cube;
                        newRow["ScenType"] = ScenType;
                        newRow["CubeDescription"] = cube_Description;
                        newRow["AggConsol"] = agg_Consol;
                        newRow["EntityDim"] = entity_Dim;
                        newRow["entity_MFB"] = entity_MFB;
                        newRow["Status"] = "Build";
                        newRow["CreateDate"] = DateTime.Now;
                        newRow["CreateUser"] = si.UserName;
                        newRow["UpdateDate"] = DateTime.Now;
                        newRow["UpdateUser"] = si.UserName;

                        FMM_Cube_Config_DT.Rows.Add(newRow);

                        // Save the changes to the database
                        sqa_fmm_cube_config.Update_FMM_Cube_Config(si, FMM_Cube_Config_DT, sqlDataAdapter);

                        save_Data_Task_Result.IsOK = true;
                        save_Data_Task_Result.Message = "New Cube Config Saved.";
                        save_Data_Task_Result.ShowMessageBox = true;
                    }
                    // If record exists and RunType is "Update", update the record
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && RunType == "Update")
                    {
                        var cubeStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("DL_FMM_Status", String.Empty);
                        new_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("IV_FMM_Cube_ID", "0"));
                        var sqa_fmm_cube_config = new SQL_Adapter_FMM_Cube_Config(si, connection);
                        var FMM_Cube_Config_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        string selectQuery = "SELECT * FROM FMM_Cube_Config WHERE CubeID = @CubeID";
                        SqlParameter[] parameters = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = new_Cube_ID }
                        };

                        sqa_fmm_cube_config.Fill_FMM_Cube_Config_DataTable(si, sqlDataAdapter, FMM_Cube_Config_DT, selectQuery, parameters);

                        // Update the existing row
                        if (FMM_Cube_Config_DT.Rows.Count > 0)
                        {
                            var rowToUpdate = FMM_Cube_Config_DT.Rows[0];
                            rowToUpdate["CubeDescription"] = cube_Description;
                            rowToUpdate["Entity_MFB"] = entity_MFB;
                            rowToUpdate["AggConsol"] = agg_Consol;
                            rowToUpdate["Status"] = cubeStatus;
                            rowToUpdate["UpdateDate"] = DateTime.Now;
                            rowToUpdate["UpdateUser"] = si.UserName;

                            // Update the database with the changes
                            sqa_fmm_cube_config.Update_FMM_Cube_Config(si, FMM_Cube_Config_DT, sqlDataAdapter);

                            save_Data_Task_Result.IsOK = true;
                            save_Data_Task_Result.Message = "Cube Config Updates Saved.";
                            save_Data_Task_Result.ShowMessageBox = true;
                        }
                    }
                    // If a duplicate exists and RunType is "New", show an error
                    else if (Convert.ToInt32(FMM_Cube_Config_Count_DT.Rows[0]["Count"]) > 0 && RunType == "New")
                    {
                        save_Data_Task_Result.IsOK = false;
                        save_Data_Task_Result.Message = "Duplicated Cube and Scenario Type, Cube Config not saved.";
                        save_Data_Task_Result.ShowMessageBox = true;
                    }
                }

                return save_Data_Task_Result;
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


        private XFSelectionChangedTaskResult SaveModel(string RunType)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var Act_Config_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID", "0"));
                var CubeID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_OS_Model_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Name", String.Empty);
                int new_Model_ID = 0;
                if (new_Model_ID == 0)
                {
                    DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (SqlConnection connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        if (RunType == "New")
                        {
                            new_Model_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Models", "ModelID");
                        }
                        else
                        {
                            new_Model_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_ID", "0"));
                        }

                        var sql_Adapter_FMM_Models = new SQL_Adapter_FMM_Models(si, connection);
                        var FMM_Models_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string selectQuery = @"
										SELECT * 
										FROM FMM_Models 
										WHERE ModelID = @ModelID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + selectQuery + "-" + new_Model_ID);

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        new SqlParameter("@ModelID", SqlDbType.Int) { Value = new_Model_ID }
                        };

                        sql_Adapter_FMM_Models.Fill_FMM_Models_DataTable(si, sqlDataAdapter, FMM_Models_DT, selectQuery, parameters);
                        if (RunType == "New")
                        {
                            var newRow = FMM_Models_DT.NewRow();
                            newRow["CubeID"] = CubeID;
                            newRow["Act_ID"] = Act_Config_ID;
                            newRow["ModelID"] = new_Model_ID;
                            newRow["Name"] = new_OS_Model_Name;
                            newRow["Status"] = "Build";
                            newRow["CreateDate"] = DateTime.Now;
                            newRow["CreateUser"] = si.UserName;
                            newRow["UpdateDate"] = DateTime.Now;
                            newRow["UpdateUser"] = si.UserName;
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
                                rowToUpdate["UpdateDate"] = DateTime.Now;
                                rowToUpdate["UpdateUser"] = si.UserName;
                            }

                        }
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        sql_Adapter_FMM_Models.Update_FMM_Models(si, FMM_Models_DT, sqlDataAdapter);
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

        private XFSelectionChangedTaskResult Save_Model_Group(string RunType)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var new_OS_Model_Grp_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Name", "0");
                int new_Model_Grp_ID = 0;
                if (new_Model_Grp_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Model_Grp_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Model_Grps", "Model_Grp_ID");

                        var sqlAdapterMcmModelGroups = new SQL_Adapter_FMM_Model_Grps(si, connection);
                        var FMM_Model_Grps_DT = new DataTable();
                        var FMM_Model_Grp_sqlDataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string selectQuery = @"
										SELECT * 
										FROM FMM_Model_Grps 
										WHERE Model_Grp_ID = @Model_Grp_ID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + selectQuery);

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = new_Model_Grp_ID }
                        };

                        sqlAdapterMcmModelGroups.Fill_FMM_Model_Grps_DataTable(si, FMM_Model_Grp_sqlDataAdapter, FMM_Model_Grps_DT, selectQuery, parameters);

                        var newRow = FMM_Model_Grps_DT.NewRow();
                        newRow["CubeID"] = new_OS_Model_Grp_Cube_ID;
                        newRow["Model_Grp_ID"] = new_Model_Grp_ID;
                        newRow["Name"] = new_Name;
                        newRow["Status"] = "Build";
                        newRow["CreateDate"] = DateTime.Now;
                        newRow["CreateUser"] = si.UserName;
                        newRow["UpdateDate"] = DateTime.Now;
                        newRow["UpdateUser"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grps_DT.Rows.Add(newRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        sqlAdapterMcmModelGroups.Update_FMM_Model_Grps(si, FMM_Model_Grps_DT, FMM_Model_Grp_sqlDataAdapter);
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

        private XFSelectionChangedTaskResult Save_Model_Grp_Seq(string RunType)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var new_Cube_ID = Convert.ToInt32(args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var new_Name = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_Name", "0");
                int new_Model_Grp_Seq_ID = 0;
                if (new_Model_Grp_Seq_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Model_Grp_Seq_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Model_Grp_Seqs", "Model_Grp_Seq_ID");

                        var sql_Adapter_FMM_Model_Grp_Seqs = new SQL_Adapter_FMM_Model_Grp_Seqs(si, connection);
                        var FMM_Model_Grp_Seqs_DT = new DataTable();
                        var FMM_Model_Grp_Seqs_sqlDataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string selectQuery = @"
										SELECT * 
										FROM FMM_Model_Grp_Seqs 
										WHERE Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        BRApi.ErrorLog.LogMessage(si, "Hit: " + selectQuery);

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = new_Model_Grp_Seq_ID }
                        };

                        sql_Adapter_FMM_Model_Grp_Seqs.Fill_FMM_Model_Grp_Seqs_DataTable(si, FMM_Model_Grp_Seqs_sqlDataAdapter, FMM_Model_Grp_Seqs_DT, selectQuery, parameters);

                        var newRow = FMM_Model_Grp_Seqs_DT.NewRow();
                        newRow["CubeID"] = new_Cube_ID;
                        newRow["Model_Grp_Seq_ID"] = new_Model_Grp_Seq_ID;
                        newRow["Name"] = new_Name;
                        newRow["Status"] = "Build";
                        newRow["CreateDate"] = DateTime.Now;
                        newRow["CreateUser"] = si.UserName;
                        newRow["UpdateDate"] = DateTime.Now;
                        newRow["UpdateUser"] = si.UserName;
                        // Set other column values for the new row as needed
                        FMM_Model_Grp_Seqs_DT.Rows.Add(newRow);
                        BRApi.ErrorLog.LogMessage(si, "Hit 6: ");

                        // Update the FMM_Dest_Cell table based on the changes made to the DataTable
                        sql_Adapter_FMM_Model_Grp_Seqs.Update_FMM_Model_Grp_Seqs(si, FMM_Model_Grp_Seqs_DT, FMM_Model_Grp_Seqs_sqlDataAdapter);
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

        private XFSelectionChangedTaskResult Save_Model_Grp_Model_Assignments(string RunType)
        {
            try
            {
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_Grp_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID", "0"));
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: ");
                var Model_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Model_ID_Selection", "0");
                BRApi.ErrorLog.LogMessage(si, "Hit Model Assignment: " + custom_SubstVars.XFGetValue("IV_FMM_Model_ID_Selection", "0") + " - " + CubeID);
                int new_OS_Model_Grp_Assign_Model_ID = 0;
                if (Model_ID_List.Length > 0 && new_OS_Model_Grp_Assign_Model_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_OS_Model_Grp_Assign_Model_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Model_Grp_Assign_Model", "Model_Grp_Assign_Model_ID");

                        var sqlAdapterMcmModelGroupAssignModel = new SQL_Adapter_FMM_Model_Grp_Assign_Model(si, connection);
                        var FMM_Model_Grp_Assign_Model_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string selectQuery = @"
										SELECT * 
										FROM FMM_Model_Grp_Assign_Model
										WHERE CubeID = @CubeID
								        AND Model_Grp_ID = @Model_Grp_ID";

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@Model_Grp_ID", SqlDbType.Int) { Value = Model_Grp_ID }
                        };

                        sqlAdapterMcmModelGroupAssignModel.Fill_FMM_Model_Grp_Assign_Model_DataTable(si, sqlDataAdapter, FMM_Model_Grp_Assign_Model_DT, selectQuery, parameters);

                        // Split the Model_ID_List by comma and loop through each ID
                        var modelIds = Model_ID_List.Split(',');
                        bool isFirstIteration = true;
                        foreach (var modelId in modelIds)
                        {
                            if (!isFirstIteration)
                            {
                                new_OS_Model_Grp_Assign_Model_ID += 1;
                            }
                            isFirstIteration = false;
                            var newRow = FMM_Model_Grp_Assign_Model_DT.NewRow();
                            newRow["CubeID"] = CubeID;
                            newRow["Model_Grp_ID"] = Model_Grp_ID;
                            newRow["ModelID"] = Convert.ToInt32(modelId.Trim());
                            newRow["Model_Grp_Assign_Model_ID"] = new_OS_Model_Grp_Assign_Model_ID;
                            newRow["Sequence"] = 0;
                            newRow["Status"] = "Build";
                            newRow["CreateDate"] = DateTime.Now;
                            newRow["CreateUser"] = si.UserName;
                            newRow["UpdateDate"] = DateTime.Now;
                            newRow["UpdateUser"] = si.UserName;

                            FMM_Model_Grp_Assign_Model_DT.Rows.Add(newRow);
                        }

                        // Update the database with the new rows
                        sqlAdapterMcmModelGroupAssignModel.Update_FMM_Model_Grp_Assign_Model(si, FMM_Model_Grp_Assign_Model_DT, sqlDataAdapter);
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Calc_Unit_Model_Grp_Assignments(string RunType)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVars;
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Selection", "0");
                var Model_Grp_ID_List = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID_Selection");
                var Model_Grp_Seq_ID = custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_Seq_ID");
                BRApi.ErrorLog.LogMessage(si, "Hit Grup Model Assignment: " + custom_SubstVars.XFGetValue("IV_FMM_Model_Grp_ID_Selection") + " - " + CubeID);
                int new_Calc_Unit_Assign_Model_Grp_ID = 0;
                if (Model_Grp_ID_List.Length > 0 && new_Calc_Unit_Assign_Model_Grp_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        connection.Open();

                        // Example: Get the max ID for the "FMM_Calc_Config" table
                        new_Calc_Unit_Assign_Model_Grp_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Calc_Unit_Assign_Model_Group", "Calc_Unit_Assign_Model_Grp_ID");

                        var sql_Adapter_FMM_Calc_Unit_Assign_Model_Group = new SQL_Adapter_FMM_Calc_Unit_Assign_Model_Group(si, connection);
                        var FMM_Calc_Unit_Assign_Model_Grp_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill the DataTable with the current data from FMM_Dest_Cell
                        string selectQuery = @"
										SELECT * 
										FROM FMM_Calc_Unit_Assign_Model_Group
										WHERE CubeID = @CubeID
								        AND Model_Grp_Seq_ID = @Model_Grp_Seq_ID";

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                        new SqlParameter("@Model_Grp_Seq_ID", SqlDbType.Int) { Value = Model_Grp_Seq_ID }
                        };

                        sql_Adapter_FMM_Calc_Unit_Assign_Model_Group.Fill_FMM_Calc_Unit_Assign_Model_Grp_DataTable(si, sqlDataAdapter, FMM_Calc_Unit_Assign_Model_Grp_DT, selectQuery, parameters);

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
                                    new_Calc_Unit_Assign_Model_Grp_ID += 1;
                                }
                                isFirstIteration = false;
                                var newRow = FMM_Calc_Unit_Assign_Model_Grp_DT.NewRow();
                                newRow["CubeID"] = CubeID;
                                newRow["Model_Grp_ID"] = Convert.ToInt32(modelGroupId.Trim());
                                newRow["Calc_Unit_ID"] = Convert.ToInt32(WfDuId.Trim());
                                newRow["Model_Grp_Seq_ID"] = Model_Grp_Seq_ID;
                                newRow["Calc_Unit_Assign_Model_Grp_ID"] = new_Calc_Unit_Assign_Model_Grp_ID;
                                newRow["Sequence"] = 0;
                                newRow["Status"] = "Build";
                                newRow["CreateDate"] = DateTime.Now;
                                newRow["CreateUser"] = si.UserName;
                                newRow["UpdateDate"] = DateTime.Now;
                                newRow["UpdateUser"] = si.UserName;

                                FMM_Calc_Unit_Assign_Model_Grp_DT.Rows.Add(newRow);
                            }
                        }

                        // Update the database with the new rows
                        sql_Adapter_FMM_Calc_Unit_Assign_Model_Group.Update_FMM_Calc_Unit_Assign_Model_Group(si, FMM_Calc_Unit_Assign_Model_Grp_DT, sqlDataAdapter);
                        BRApi.ErrorLog.LogMessage(si, "Model Group Assignments saved.");
                    }
                }

                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Approval_Step(string RunType)
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Act_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Act_ID", "0"));
                var Appr_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var WFProfile_Step = custom_SubstVars.XFGetValue("IV_FMM_trv_Approval_Step_WFProfile", string.Empty);
                var RegConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Register_Profiles", "0"));

                BRApi.ErrorLog.LogMessage(si, "Hit Approval Step: " + CubeID + "|" + Act_ID + "|" + Appr_ID + "|" + WFProfile_Step);
                int new_Appr_Step_ID = 0;

                if (CubeID > 0 && Act_ID > 0 && Appr_ID > 0 && RegConfigID > 0 && WFProfile_Step != string.Empty && new_Appr_Step_ID == 0)
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        connection.Open();

                        // Get the max ID for the Approval_Step_Config table
                        new_Appr_Step_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Approval_Step_Config", "Appr_Step_ID");

                        var sql_Adapter_FMM_Approval_Step_Config = new SQL_Adapter_FMM_Approval_Step_Config(si, connection);
                        var FMM_Approval_Step_Config_DT = new DataTable();
                        var FMM_Approval_Step_WFProfile_DT = new DataTable();
                        var FMM_Cube_Config_DT = new DataTable();
                        //var sql_Adapter_FMM_Reg_Dtl_Cube_Map = ""; //new SQL_Adapter_FMM_Reg_Dtl_Cube_Map(si, connection);
                        var FMM_Reg_Dtl_Cube_Map_DT = new DataTable(); // DataTable for FMM_Reg_Dtl_Cube_Map
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill DataTable for Approval_Step_Config
                        string selectQuery = @"
		                    SELECT * 
		                    FROM FMM_Approval_Step_Config
		                    WHERE CubeID = @CubeID
		                    AND Act_ID = @Act_ID
		                    AND Appr_ID = @Appr_ID
		                    AND RegisterProfile = @RegisterProfile";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                            new SqlParameter("@RegisterProfile", SqlDbType.Int) { Value = RegConfigID }
                        };

                        sql_Adapter_FMM_Approval_Step_Config.Fill_FMM_Approval_Step_Config_DataTable(si, sqlDataAdapter, FMM_Approval_Step_Config_DT, selectQuery, parameters);

                        // Fill DataTable for Approval_Step_Config
                        selectQuery = @"
		                    SELECT * 
		                    FROM FMM_Reg_Dtl_Cube_Map
		                    WHERE CubeID = @CubeID
		                    AND Act_ID = @Act_ID
		                    AND Appr_ID = @Appr_ID";

                        parameters = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID },
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                        };

                        //sql_Adapter_FMM_Reg_Dtl_Cube_Map.Fill_FMM_Reg_Dtl_Cube_Map_DataTable(si, sqlDataAdapter, FMM_Reg_Dtl_Cube_Map_DT, selectQuery,parameters);

                        // Load CubeConfig data
                        string selectCubeConfigQuery = @"
		                    SELECT * 
		                    FROM FMM_Cube_Config
		                    WHERE CubeID = @CubeID";

                        var cubeConfigParams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                        };

                        sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sqlDataAdapter, FMM_Cube_Config_DT, selectCubeConfigQuery, cubeConfigParams);

                        var topCubeInfo = BRApi.Finance.Cubes.GetCubeInfo(si, FMM_Cube_Config_DT.Rows[0].Field<string>("Cube"));
                        var cubeScenarioType = ScenarioType.GetItem(FMM_Cube_Config_DT.Rows[0].Field<string>("ScenType"));
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

                        sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sqlDataAdapter, FMM_Approval_Step_WFProfile_DT, selectWFProfileQuery, new SqlParameter[]
                        {
                            new SqlParameter("@rootprofilename", SqlDbType.NVarChar) { Value = rootProfileName }
                        });

                        // LINQ query to filter WFProfiles based on WFProfile_Step
                        var filteredWFProfiles = from profile in FMM_Approval_Step_WFProfile_DT.AsEnumerable()
                                                 where profile["ProfileName"].ToString().Contains(WFProfile_Step)
                                                 select profile;

                        // Loop through filtered WFProfiles and insert new Approval Steps and Cube Map
                        foreach (var profile in filteredWFProfiles)
                        {
                            var newRow = FMM_Approval_Step_Config_DT.NewRow();
                            newRow["CubeID"] = CubeID;
                            newRow["Act_ID"] = Act_ID;
                            newRow["Appr_ID"] = Appr_ID;
                            newRow["WFProfile_Step"] = profile["ProfileKey"];
                            newRow["Appr_Step_ID"] = new_Appr_Step_ID++;
                            newRow["StepNum"] = 0;
                            newRow["RegisterProfile"] = RegConfigID;
                            newRow["Approval_User_Group"] = "Test";
                            newRow["ApprovalLogic"] = "Test";
                            newRow["ApprovalItem"] = "Test";
                            newRow["ApprovalLevel"] = 0;
                            newRow["ApprovalConfig"] = 0;
                            newRow["InitStatus"] = "Testy";
                            newRow["ApprovedStatus"] = "Testy";
                            newRow["RejectionStatus"] = "Testy";
                            newRow["Status"] = "Build";
                            newRow["CreateDate"] = DateTime.Now;
                            newRow["CreateUser"] = si.UserName;
                            newRow["UpdateDate"] = DateTime.Now;
                            newRow["UpdateUser"] = si.UserName;

                            FMM_Approval_Step_Config_DT.Rows.Add(newRow);

                            // Insert into FMM_Reg_Dtl_Cube_Map
                            var mapRow = FMM_Reg_Dtl_Cube_Map_DT.NewRow();
                            mapRow["CubeID"] = CubeID;
                            mapRow["Act_ID"] = Act_ID;
                            mapRow["Appr_ID"] = Appr_ID;
                            mapRow["Appr_Step_ID"] = newRow["Appr_Step_ID"];
                            mapRow["Reg_Dtl_Cube_Map_ID"] = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Reg_Dtl_Cube_Map", "Reg_Dtl_Cube_Map_ID");
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
                            mapRow["CreateDate"] = DateTime.Now;
                            mapRow["CreateUser"] = si.UserName;
                            mapRow["UpdateDate"] = DateTime.Now;
                            mapRow["UpdateUser"] = si.UserName;

                            FMM_Reg_Dtl_Cube_Map_DT.Rows.Add(mapRow);
                        }

                        // Save Approval Steps
                        sql_Adapter_FMM_Approval_Step_Config.Update_FMM_Approval_Step_Config(si, FMM_Approval_Step_Config_DT, sqlDataAdapter);

                        // Save Cube Map entries
                        //sql_Adapter_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                        BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                    }
                }
                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private XFSelectionChangedTaskResult Save_Approval_Step_Activity_Config(string RunType)
        {
            try
            {

                BRApi.ErrorLog.LogMessage(si, "Save approval step activity runtype: " + RunType);
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var Act_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Approval_Act_ID", "0"));
                var Appr_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_ID", "0"));
                var Appr_Step_ID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Appr_Step_ID", "-1")); // this can actually be 0
                var RegConfigID = Convert.ToInt32(custom_SubstVars.XFGetValue("BL_FMM_Register_Profiles", "0"));


                BRApi.ErrorLog.LogMessage(si, "save data: " + Act_ID + " " + Appr_ID + " " + Appr_Step_ID + " " + RegConfigID);

                if (RunType == "New" && Act_ID > 0 && Appr_ID > 0 && Appr_Step_ID >= 0 && RegConfigID > 0) // new row
                {
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        BRApi.ErrorLog.LogMessage(si, "Hit: MaxID");
                        var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        connection.Open();

                        // Get the max ID for the Approval_Step_Config table
                        int new_Approval_Step_Act_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Approval_Step_Activity_Config", "Approval_Step_Act_ID");

                        var sql_Adapter_FMM_Approval_Step_Activity_Config = new SQL_Adapter_FMM_Approval_Step_Activity_Config(si, connection);
                        var FMM_Approval_Step_Activity_Config_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill DataTable for Approval_Step_Activity_Config
                        string selectQuery = @"
		                    SELECT * 
		                    FROM FMM_Approval_Step_Activity_Config
		                    WHERE Act_ID = @Act_ID
		                    AND Appr_ID = @Appr_ID
							AND Appr_Step_ID = @Appr_Step_ID";

                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID },
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = Appr_ID },
                            new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = Appr_Step_ID },

                        };

                        sql_Adapter_FMM_Approval_Step_Activity_Config.Fill_FMM_Approval_Step_Activity_Config_DataTable(si, sqlDataAdapter, FMM_Approval_Step_Activity_Config_DT, selectQuery, parameters);

                        string noneVal = "None";
                        FMM_Approval_Step_Activity_Config_DT.Clear(); // clear data, because we only want the column information to create a new row.
                                                                      // Insert into FMM_Approval_Step_Activity_Config_DT
                        var newRow = FMM_Approval_Step_Activity_Config_DT.NewRow();
                        newRow["Act_ID"] = Act_ID;
                        newRow["Appr_ID"] = Appr_ID;
                        newRow["Appr_Step_ID"] = Appr_Step_ID;
                        newRow["Approval_Step_Act_ID"] = new_Approval_Step_Act_ID;
                        newRow["RegConfigID"] = RegConfigID;
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
                        newRow["CreateDate"] = DateTime.Now;
                        newRow["CreateUser"] = si.UserName;
                        newRow["UpdateDate"] = DateTime.Now;
                        newRow["UpdateUser"] = si.UserName;

                        FMM_Approval_Step_Activity_Config_DT.Rows.Add(newRow);

                        // Save Approval Step Activity Config
                        sql_Adapter_FMM_Approval_Step_Activity_Config.Update_FMM_Approval_Step_Activity_Config(si, FMM_Approval_Step_Activity_Config_DT, sqlDataAdapter);
                    }


                    // Save Cube Map entries
                    //sql_Adapter_FMM_Reg_Dtl_Cube_Map.Update_FMM_Reg_Dtl_Cube_Map(si, FMM_Reg_Dtl_Cube_Map_DT, sqlDataAdapter);

                    BRApi.ErrorLog.LogMessage(si, "Approval Step Assignments and Cube Map saved.");
                }
                else if (RunType == "Update" && Act_ID > 0)
                { // existing row
                    var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        connection.Open();

                        var sql_Adapter_FMM_Approval_Step_Activity_Config = new SQL_Adapter_FMM_Approval_Step_Activity_Config(si, connection);
                        var FMM_Approval_Step_Activity_Config_DT = new DataTable();
                        var sqlDataAdapter = new SqlDataAdapter();

                        // Fill DataTable for Approval_Step_Activity_Config
                        string selectQuery = @"
		                    SELECT * 
		                    FROM FMM_Approval_Step_Activity_Config
		                    WHERE Approval_Step_Act_ID = @Approval_Step_Act_ID";

                        var parameters = new SqlParameter[]
                        {
							//kinda weird, but we're passing in Act_ID as the Approval Step Activity ID when in update runtype
		                    new SqlParameter("@Approval_Step_Act_ID", SqlDbType.Int) { Value = Act_ID },

                        };

                        sql_Adapter_FMM_Approval_Step_Activity_Config.Fill_FMM_Approval_Step_Activity_Config_DataTable(si, sqlDataAdapter, FMM_Approval_Step_Activity_Config_DT, selectQuery, parameters);

                        BRApi.ErrorLog.LogMessage(si, "mcm: " + FMM_Approval_Step_Activity_Config_DT.Rows[0]["RegConfigID"]);
                        // update existing row
                        FMM_Approval_Step_Activity_Config_DT.Rows[0]["RegConfigID"] = RegConfigID;
                        FMM_Approval_Step_Activity_Config_DT.Rows[0]["UpdateDate"] = DateTime.Now;
                        FMM_Approval_Step_Activity_Config_DT.Rows[0]["UpdateUser"] = si.UserName;

                        // Save Approval Step Activity Config
                        sql_Adapter_FMM_Approval_Step_Activity_Config.Update_FMM_Approval_Step_Activity_Config(si, FMM_Approval_Step_Activity_Config_DT, sqlDataAdapter);
                    }
                }
                return save_Data_Task_Result;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private XFSelectionChangedTaskResult Process_Bulk_WF_DU()
        {
            try
            {
                var save_Data_Task_Result = new XFSelectionChangedTaskResult();
                var custom_SubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                var CubeID = Convert.ToInt32(custom_SubstVars.XFGetValue("IV_FMM_Cube_ID", "0"));
                var Calc_Unit_Entity_MFB = custom_SubstVars.XFGetValue("IV_FMM_Calc_Unit_Entity_MFB", "0");
                var WF_Channels = custom_SubstVars.XFGetValue("BL_FMM_WFChannels", "0");
                var new_Calc_Unit_ID = 0;
                var loop_times = 0;

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                    connection.Open();

                    // Example: Get the max ID for the "FMM_Calc_Config" table
                    new_Calc_Unit_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");

                    var sql_Adapter_FMM_Calc_Unit_Config = new SQL_Adapter_FMM_Calc_Unit_Config(si, connection);
                    var FMM_Calc_Unit_Config_DT = new DataTable();
                    var sqlDataAdapter = new SqlDataAdapter();

                    // Fill the DataTable with the current data from FMM_Dest_Cell
                    string selectQuery = @"
									SELECT * 
									FROM FMM_Calc_Unit_Config
									WHERE CubeID = @CubeID";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID }
                    };

                    sql_Adapter_FMM_Calc_Unit_Config.Fill_FMM_Calc_Unit_Config_DataTable(si, sqlDataAdapter, FMM_Calc_Unit_Config_DT, selectQuery, parameters);

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
                        newRow["CubeID"] = CubeID;
                        newRow["Entity_MFB"] = entity.Member.Name;
                        newRow["WFChannel"] = WF_Channels;
                        newRow["Status"] = "Build";
                        newRow["CreateDate"] = DateTime.Now;
                        newRow["CreateUser"] = si.UserName;
                        newRow["UpdateDate"] = DateTime.Now;
                        newRow["UpdateUser"] = si.UserName;

                        FMM_Calc_Unit_Config_DT.Rows.Add(newRow);
                        loop_times += 1;
                    }

                    sql_Adapter_FMM_Calc_Unit_Config.Update_FMM_Calc_Unit_Config(si, FMM_Calc_Unit_Config_DT, sqlDataAdapter);

                    return save_Data_Task_Result;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }

        }
        #endregion
        #region "Copy Models"
        private void Process_Cube_Config_Copy(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Cube_ID", "0"));
            var tgt_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Cube_ID", "0"));
            BRApi.ErrorLog.LogMessage(si, "Hit Copy: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Activity_Config_DT = new DataTable();
            var tgt_FMM_Activity_Config_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_Appr_Config_DT = new DataTable();
            var tgt_FMM_Appr_Config_DT = new DataTable();
            var src_FMM_Approval_Step_Config_DT = new DataTable();
            var tgt_FMM_Approval_Step_Config_DT = new DataTable();
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
            var src_FMM_Model_Grp_Assign_Model_DT = new DataTable();
            var tgt_FMM_Model_Grp_Assign_Model_DT = new DataTable();
            var src_FMM_Calc_Unit_Config_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Config_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_Model_Grp_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_Model_Grp_DT = new DataTable();
            var tgt_FMM_Cube_Config_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                var sql_Adapter_FMM_Acct_Config = new SQL_Adapter_FMM_Acct_Config(si, connection);
                var sql_Data_Adapter_FMM_Acct_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Activity_Config = new SQL_Adapter_FMM_Activity_Config(si, connection);
                var sql_Data_Adapter_FMM_Activity_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Appr_Config = new SQL_Adapter_FMM_Appr_Config(si, connection);
                var sql_Data_Adapter_FMM_Appr_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Approval_Step_Config = new SQL_Adapter_FMM_Approval_Step_Config(si, connection);
                var sql_Data_Adapter_FMM_Approval_Step_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Config = new SQL_Adapter_FMM_Calc_Config(si, connection);
                var sql_Data_Adapter_FMM_Calc_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Col_Config = new SQL_Adapter_FMM_Col_Config(si, connection);
                var sql_Data_Adapter_FMM_Col_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Cube_Config = new SQL_Adapter_FMM_Cube_Config(si, connection);
                var sql_Data_Adapter_FMM_Cube_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Dest_Cell = new SQL_Adapter_FMM_Dest_Cell(si, connection);
                var sql_Data_Adapter_FMM_Dest_Cell = new SqlDataAdapter();
                var sql_Adapter_FMM_Model_Grp_Assign_Model = new SQL_Adapter_FMM_Model_Grp_Assign_Model(si, connection);
                var sql_Data_Adapter_FMM_Model_Grp_Assign_Model = new SqlDataAdapter();
                var sql_Adapter_FMM_Model_Grps = new SQL_Adapter_FMM_Model_Grps(si, connection);
                var sql_Data_Adapter_FMM_Model_Grps = new SqlDataAdapter();
                var sql_Adapter_FMM_Models = new SQL_Adapter_FMM_Models(si, connection);
                var sql_Data_Adapter_FMM_Models = new SqlDataAdapter();
                var sql_Adapter_FMM_Reg_Config = new SQL_Adapter_FMM_Reg_Config(si, connection);
                var sql_Data_Adapter_FMM_Reg_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Src_Cell = new SQL_Adapter_FMM_Src_Cell(si, connection);
                var sql_Data_Adapter_FMM_Src_Cell = new SqlDataAdapter();
                var sql_Adapter_FMM_Unit_Config = new SQL_Adapter_FMM_Unit_Config(si, connection);
                var sql_Data_Adapter_FMM_Unit_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Unit_Assign_Model_Group = new SQL_Adapter_FMM_Calc_Unit_Assign_Model_Group(si, connection);
                var sql_Data_Adapter_FMM_Calc_Unit_Assign_Model_Group = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Unit_Config = new SQL_Adapter_FMM_Calc_Unit_Config(si, connection);
                var sql_Data_Adapter_FMM_Calc_Unit_Config = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get MCM Data"
                // Call for get_FMM_Activity_Config_Data
                get_FMM_Activity_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Activity_Config,
                    ref src_FMM_Activity_Config_DT, ref tgt_FMM_Activity_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Unit_Config,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Acct_Config,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Appr_Config,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Approval_Step_Config_Data
                get_FMM_Approval_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Approval_Step_Config,
                    ref src_FMM_Approval_Step_Config_DT, ref tgt_FMM_Approval_Step_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Reg_Config,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Col_Config,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Models,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Dest_Cell,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Src_Cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Model_Grps,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Model_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Model_Grp_Assign_Model,
                    ref src_FMM_Model_Grp_Assign_Model_DT, ref tgt_FMM_Model_Grp_Assign_Model_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Unit_Config,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Unit_Assign_Model_Grp_Data
                get_FMM_Calc_Unit_Assign_Model_Grp_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Unit_Assign_Model_Group,
                    ref src_FMM_Calc_Unit_Assign_Model_Grp_DT, ref tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, sql_FMM_Get_Max_ID
                );
                #endregion

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_Activity_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hti Copy");
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_Activity_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    // Access Act_ID from the current activity_ConfigRow
                    var curr_srcActivityID = activity_ConfigRow["Act_ID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["Act_ID"]
                                            : -1; // Handle null Act_IDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["UnitID"] != DBNull.Value
                        ? (int)unit_ConfigRow["UnitID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyUnits(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"UnitID = {curr_srcUnitID}"))
                        {
                            CopyAccts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["RegConfigID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["RegConfigID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"RegConfigID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow approval_ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = approval_ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)approval_ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyApprovals(approval_ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Approval_Step_Config_DT.Select($"Appr_ID = {curr_srcApprovalID}"))
                        {
                            Copy_Approval_Steps(approvalstep_ConfigRow, ref tgt_FMM_Approval_Step_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["ModelID"] != DBNull.Value
                        ? (int)models_ConfigRow["ModelID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyModels(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"ModelID = {curr_srcModelID}"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                            var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                            ? (int)calc_ConfigRow["CalcID"]
                            : -1; // Handle null Unit_IDs as needed
                            CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
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
                sql_Adapter_FMM_Activity_Config.Update_FMM_Activity_Config(si, tgt_FMM_Activity_Config_DT, sql_Data_Adapter_FMM_Activity_Config);
                sql_Adapter_FMM_Unit_Config.Update_FMM_Unit_Config(si, tgt_FMM_Unit_Config_DT, sql_Data_Adapter_FMM_Unit_Config);
                sql_Adapter_FMM_Acct_Config.Update_FMM_Acct_Config(si, tgt_FMM_Acct_Config_DT, sql_Data_Adapter_FMM_Acct_Config);
                sql_Adapter_FMM_Reg_Config.Update_FMM_Reg_Config(si, tgt_FMM_Reg_Config_DT, sql_Data_Adapter_FMM_Reg_Config);
                sql_Adapter_FMM_Col_Config.Update_FMM_Col_Config(si, tgt_FMM_Col_Config_DT, sql_Data_Adapter_FMM_Col_Config);
                sql_Adapter_FMM_Appr_Config.Update_FMM_Appr_Config(si, tgt_FMM_Appr_Config_DT, sql_Data_Adapter_FMM_Appr_Config);
                sql_Adapter_FMM_Approval_Step_Config.Update_FMM_Approval_Step_Config(si, tgt_FMM_Approval_Step_Config_DT, sql_Data_Adapter_FMM_Approval_Step_Config);
                sql_Adapter_FMM_Models.Update_FMM_Models(si, tgt_FMM_Models_DT, sql_Data_Adapter_FMM_Models);
                sql_Adapter_FMM_Calc_Config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sql_Data_Adapter_FMM_Calc_Config);
                sql_Adapter_FMM_Dest_Cell.Update_FMM_Dest_Cell(si, tgt_FMM_Dest_Cell_DT, sql_Data_Adapter_FMM_Dest_Cell);
                sql_Adapter_FMM_Src_Cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sql_Data_Adapter_FMM_Src_Cell);
                sql_Adapter_FMM_Model_Grp_Assign_Model.Update_FMM_Model_Grp_Assign_Model(si, tgt_FMM_Model_Grp_Assign_Model_DT, sql_Data_Adapter_FMM_Model_Grp_Assign_Model);
                sql_Adapter_FMM_Calc_Unit_Config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sql_Data_Adapter_FMM_Calc_Unit_Config);
                sql_Adapter_FMM_Calc_Unit_Assign_Model_Group.Update_FMM_Calc_Unit_Assign_Model_Group(si, tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, sql_Data_Adapter_FMM_Calc_Unit_Assign_Model_Group);
                sql_Adapter_FMM_Calc_Unit_Config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sql_Data_Adapter_FMM_Calc_Unit_Config);
                sql_Adapter_FMM_Model_Grps.Update_FMM_Model_Grps(si, tgt_FMM_Model_Grps_DT, sql_Data_Adapter_FMM_Model_Grps);
                sql_Adapter_FMM_Cube_Config.Update_FMM_Cube_Config(si, tgt_FMM_Cube_Config_DT, sql_Data_Adapter_FMM_Cube_Config);
            }
        }

        private void Process_Activity_Config_Copy(ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var src_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Cube_ID", "0"));
            var tgt_Cube_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("tgt_Cube_ID", "0"));
            var src_Act_ID = Convert.ToInt32(args.NameValuePairs.XFGetValue("src_Act_ID", "0"));
            BRApi.ErrorLog.LogMessage(si, "Hit Copy: " + src_Cube_ID + "-" + tgt_Cube_ID);
            #region "Define Data Tables"
            var src_FMM_Activity_Config_DT = new DataTable();
            var tgt_FMM_Activity_Config_DT = new DataTable();
            var src_FMM_Unit_Config_DT = new DataTable();
            var tgt_FMM_Unit_Config_DT = new DataTable();
            var src_FMM_Acct_Config_DT = new DataTable();
            var tgt_FMM_Acct_Config_DT = new DataTable();
            var src_FMM_Appr_Config_DT = new DataTable();
            var tgt_FMM_Appr_Config_DT = new DataTable();
            var src_FMM_Approval_Step_Config_DT = new DataTable();
            var tgt_FMM_Approval_Step_Config_DT = new DataTable();
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
            var src_FMM_Model_Grp_Assign_Model_DT = new DataTable();
            var tgt_FMM_Model_Grp_Assign_Model_DT = new DataTable();
            var src_FMM_Calc_Unit_Config_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Config_DT = new DataTable();
            var src_FMM_Calc_Unit_Assign_Model_Grp_DT = new DataTable();
            var tgt_FMM_Calc_Unit_Assign_Model_Grp_DT = new DataTable();
            var tgt_FMM_Cube_Config_DT = new DataTable();
            #endregion
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                #region "Define SQL Adapter Classes"
                var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                var sql_Adapter_FMM_Acct_Config = new SQL_Adapter_FMM_Acct_Config(si, connection);
                var sql_Data_Adapter_FMM_Acct_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Activity_Config = new SQL_Adapter_FMM_Activity_Config(si, connection);
                var sql_Data_Adapter_FMM_Activity_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Appr_Config = new SQL_Adapter_FMM_Appr_Config(si, connection);
                var sql_Data_Adapter_FMM_Appr_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Approval_Step_Config = new SQL_Adapter_FMM_Approval_Step_Config(si, connection);
                var sql_Data_Adapter_FMM_Approval_Step_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Config = new SQL_Adapter_FMM_Calc_Config(si, connection);
                var sql_Data_Adapter_FMM_Calc_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Col_Config = new SQL_Adapter_FMM_Col_Config(si, connection);
                var sql_Data_Adapter_FMM_Col_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Cube_Config = new SQL_Adapter_FMM_Cube_Config(si, connection);
                var sql_Data_Adapter_FMM_Cube_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Dest_Cell = new SQL_Adapter_FMM_Dest_Cell(si, connection);
                var sql_Data_Adapter_FMM_Dest_Cell = new SqlDataAdapter();
                var sql_Adapter_FMM_Model_Grp_Assign_Model = new SQL_Adapter_FMM_Model_Grp_Assign_Model(si, connection);
                var sql_Data_Adapter_FMM_Model_Grp_Assign_Model = new SqlDataAdapter();
                var sql_Adapter_FMM_Model_Grps = new SQL_Adapter_FMM_Model_Grps(si, connection);
                var sql_Data_Adapter_FMM_Model_Grps = new SqlDataAdapter();
                var sql_Adapter_FMM_Models = new SQL_Adapter_FMM_Models(si, connection);
                var sql_Data_Adapter_FMM_Models = new SqlDataAdapter();
                var sql_Adapter_FMM_Reg_Config = new SQL_Adapter_FMM_Reg_Config(si, connection);
                var sql_Data_Adapter_FMM_Reg_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Src_Cell = new SQL_Adapter_FMM_Src_Cell(si, connection);
                var sql_Data_Adapter_FMM_Src_Cell = new SqlDataAdapter();
                var sql_Adapter_FMM_Unit_Config = new SQL_Adapter_FMM_Unit_Config(si, connection);
                var sql_Data_Adapter_FMM_Unit_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Unit_Assign_Model_Group = new SQL_Adapter_FMM_Calc_Unit_Assign_Model_Group(si, connection);
                var sql_Data_Adapter_FMM_Calc_Unit_Assign_Model_Group = new SqlDataAdapter();
                var sql_Adapter_FMM_Calc_Unit_Config = new SQL_Adapter_FMM_Calc_Unit_Config(si, connection);
                var sql_Data_Adapter_FMM_Calc_Unit_Config = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get MCM Data"
                // Call for get_FMM_Activity_Config_Data
                get_FMM_Activity_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Activity_Config,
                    ref src_FMM_Activity_Config_DT, ref tgt_FMM_Activity_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Unit_Config_Data
                get_FMM_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Unit_Config,
                    ref src_FMM_Unit_Config_DT, ref tgt_FMM_Unit_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Acct_Config_Data
                get_FMM_Acct_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Acct_Config,
                    ref src_FMM_Acct_Config_DT, ref tgt_FMM_Acct_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Appr_Config_Data
                get_FMM_Appr_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Appr_Config,
                    ref src_FMM_Appr_Config_DT, ref tgt_FMM_Appr_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Approval_Step_Config_Data
                get_FMM_Approval_Step_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Approval_Step_Config,
                    ref src_FMM_Approval_Step_Config_DT, ref tgt_FMM_Approval_Step_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Reg_Config_Data
                get_FMM_Reg_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Reg_Config,
                    ref src_FMM_Reg_Config_DT, ref tgt_FMM_Reg_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Col_Config_Data
                get_FMM_Col_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Col_Config,
                    ref src_FMM_Col_Config_DT, ref tgt_FMM_Col_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Models_Data
                get_FMM_Models_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Models,
                    ref src_FMM_Models_DT, ref tgt_FMM_Models_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Dest_Cell,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Src_Cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Model_Grps_Data
                get_FMM_Model_Grps_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Model_Grps,
                    ref src_FMM_Model_Grps_DT, ref tgt_FMM_Model_Grps_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Model_Grp_Assign_Model_Data
                get_FMM_Model_Grp_Assign_Model_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Model_Grp_Assign_Model,
                    ref src_FMM_Model_Grp_Assign_Model_DT, ref tgt_FMM_Model_Grp_Assign_Model_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Unit_Config_Data
                get_FMM_Calc_Unit_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Unit_Config,
                    ref src_FMM_Calc_Unit_Config_DT, ref tgt_FMM_Calc_Unit_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Calc_Unit_Assign_Model_Grp_Data
                get_FMM_Calc_Unit_Assign_Model_Grp_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID } },
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID } },
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Unit_Assign_Model_Group,
                    ref src_FMM_Calc_Unit_Assign_Model_Grp_DT, ref tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, sql_FMM_Get_Max_ID
                );
                #endregion

                #region "Copy Activity Data"
                foreach (DataRow activity_ConfigRow in src_FMM_Activity_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hti Copy");
                    CopyActivities(activity_ConfigRow, ref tgt_FMM_Activity_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    // Access Act_ID from the current activity_ConfigRow
                    var curr_srcActivityID = activity_ConfigRow["Act_ID"] != DBNull.Value
                                            ? (int)activity_ConfigRow["Act_ID"]
                                            : -1; // Handle null Act_IDs as needed
                    foreach (DataRow unit_ConfigRow in src_FMM_Unit_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcUnitID = unit_ConfigRow["UnitID"] != DBNull.Value
                        ? (int)unit_ConfigRow["UnitID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyUnits(unit_ConfigRow, ref tgt_FMM_Unit_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow acct_ConfigRow in src_FMM_Acct_Config_DT.Select($"UnitID = {curr_srcUnitID}"))
                        {
                            CopyAccts(acct_ConfigRow, ref tgt_FMM_Acct_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow Reg_ConfigRow in src_FMM_Reg_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcRegisterConfigID = Reg_ConfigRow["RegConfigID"] != DBNull.Value
                        ? (int)Reg_ConfigRow["RegConfigID"]
                        : -1; // Handle null Unit_IDs as needed
                        Copy_Reg_Config(Reg_ConfigRow, ref tgt_FMM_Reg_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow col_ConfigRow in src_FMM_Reg_Config_DT.Select($"RegConfigID = {curr_srcRegisterConfigID}"))
                        {
                            Copy_Col_Config(col_ConfigRow, ref tgt_FMM_Col_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow approval_ConfigRow in src_FMM_Appr_Config_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcApprovalID = approval_ConfigRow["Appr_ID"] != DBNull.Value
                        ? (int)approval_ConfigRow["Appr_ID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyApprovals(approval_ConfigRow, ref tgt_FMM_Appr_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow approvalstep_ConfigRow in src_FMM_Approval_Step_Config_DT.Select($"Appr_ID = {curr_srcApprovalID}"))
                        {
                            Copy_Approval_Steps(approvalstep_ConfigRow, ref tgt_FMM_Approval_Step_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        }
                    }
                    foreach (DataRow models_ConfigRow in src_FMM_Models_DT.Select($"Act_ID = {curr_srcActivityID}"))
                    {
                        var curr_srcModelID = models_ConfigRow["ModelID"] != DBNull.Value
                        ? (int)models_ConfigRow["ModelID"]
                        : -1; // Handle null Unit_IDs as needed
                        CopyModels(models_ConfigRow, ref tgt_FMM_Models_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                        foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Select($"ModelID = {curr_srcModelID}"))
                        {
                            BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                            var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                            ? (int)calc_ConfigRow["CalcID"]
                            : -1; // Handle null Unit_IDs as needed
                            CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Calc Copy");
                            foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                            {
                                CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                            }
                            foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
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
                sql_Adapter_FMM_Activity_Config.Update_FMM_Activity_Config(si, tgt_FMM_Activity_Config_DT, sql_Data_Adapter_FMM_Activity_Config);
                sql_Adapter_FMM_Unit_Config.Update_FMM_Unit_Config(si, tgt_FMM_Unit_Config_DT, sql_Data_Adapter_FMM_Unit_Config);
                sql_Adapter_FMM_Acct_Config.Update_FMM_Acct_Config(si, tgt_FMM_Acct_Config_DT, sql_Data_Adapter_FMM_Acct_Config);
                sql_Adapter_FMM_Reg_Config.Update_FMM_Reg_Config(si, tgt_FMM_Reg_Config_DT, sql_Data_Adapter_FMM_Reg_Config);
                sql_Adapter_FMM_Col_Config.Update_FMM_Col_Config(si, tgt_FMM_Col_Config_DT, sql_Data_Adapter_FMM_Col_Config);
                sql_Adapter_FMM_Appr_Config.Update_FMM_Appr_Config(si, tgt_FMM_Appr_Config_DT, sql_Data_Adapter_FMM_Appr_Config);
                sql_Adapter_FMM_Approval_Step_Config.Update_FMM_Approval_Step_Config(si, tgt_FMM_Approval_Step_Config_DT, sql_Data_Adapter_FMM_Approval_Step_Config);
                sql_Adapter_FMM_Models.Update_FMM_Models(si, tgt_FMM_Models_DT, sql_Data_Adapter_FMM_Models);
                sql_Adapter_FMM_Calc_Config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sql_Data_Adapter_FMM_Calc_Config);
                sql_Adapter_FMM_Dest_Cell.Update_FMM_Dest_Cell(si, tgt_FMM_Dest_Cell_DT, sql_Data_Adapter_FMM_Dest_Cell);
                sql_Adapter_FMM_Src_Cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sql_Data_Adapter_FMM_Src_Cell);
                sql_Adapter_FMM_Model_Grp_Assign_Model.Update_FMM_Model_Grp_Assign_Model(si, tgt_FMM_Model_Grp_Assign_Model_DT, sql_Data_Adapter_FMM_Model_Grp_Assign_Model);
                sql_Adapter_FMM_Calc_Unit_Config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sql_Data_Adapter_FMM_Calc_Unit_Config);
                sql_Adapter_FMM_Calc_Unit_Assign_Model_Group.Update_FMM_Calc_Unit_Assign_Model_Group(si, tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, sql_Data_Adapter_FMM_Calc_Unit_Assign_Model_Group);
                sql_Adapter_FMM_Calc_Unit_Config.Update_FMM_Calc_Unit_Config(si, tgt_FMM_Calc_Unit_Config_DT, sql_Data_Adapter_FMM_Calc_Unit_Config);
                sql_Adapter_FMM_Model_Grps.Update_FMM_Model_Grps(si, tgt_FMM_Model_Grps_DT, sql_Data_Adapter_FMM_Model_Grps);
                sql_Adapter_FMM_Cube_Config.Update_FMM_Cube_Config(si, tgt_FMM_Cube_Config_DT, sql_Data_Adapter_FMM_Cube_Config);
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
                var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
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
                sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, srcActivities_sqlDataAdapter, srcActivities_DT, srcActivities_selectQuery, srcActivities_parameters);

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
            Global_Curr_FMM_Act_ID = tgt_Act_ID;
            Global_Curr_FMM_Models_ID = tgt_Model_ID;
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
                var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                var sql_FMM_Get_Max_ID = new SQL_FMM_Get_Max_ID(si, connection);
                var sql_Adapter_FMM_Calc_Config = new SQL_Adapter_FMM_Calc_Config(si, connection);
                var sql_Data_Adapter_FMM_Calc_Config = new SqlDataAdapter();
                var sql_Adapter_FMM_Dest_Cell = new SQL_Adapter_FMM_Dest_Cell(si, connection);
                var sql_Data_Adapter_FMM_Dest_Cell = new SqlDataAdapter();
                var sql_Adapter_FMM_Src_Cell = new SQL_Adapter_FMM_Src_Cell(si, connection);
                var sql_Data_Adapter_FMM_Src_Cell = new SqlDataAdapter();
                #endregion
                connection.Open();
                #region "Get MCM Data"

                // Call for get_FMM_Calc_Config_Data
                get_FMM_Calc_Config_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Calc_Config,
                    ref src_FMM_Calc_Config_DT, ref tgt_FMM_Calc_Config_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Dest_Cell_Data
                get_FMM_Dest_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Dest_Cell,
                    ref src_FMM_Dest_Cell_DT, ref tgt_FMM_Dest_Cell_DT, sql_FMM_Get_Max_ID
                );

                // Call for get_FMM_Src_Cell_Data
                get_FMM_Src_Cell_Data(
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = src_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = src_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = src_Model_ID },
                                         new SqlParameter("@CalcID", SqlDbType.NVarChar) {Value = src_Calc_IDs }},
                    new SqlParameter[] { new SqlParameter("@CubeID", SqlDbType.Int) { Value = tgt_Cube_ID },
                                         new SqlParameter("@Act_ID", SqlDbType.Int) { Value = tgt_Act_ID },
                                         new SqlParameter("@ModelID", SqlDbType.Int) { Value = tgt_Model_ID }},
                    sql_FMM_Get_DataSets, sql_Adapter_FMM_Src_Cell,
                    ref src_FMM_Src_Cell_DT, ref tgt_FMM_Src_Cell_DT, sql_FMM_Get_Max_ID
                );

                #endregion

                #region "Copy Calc Data"
                foreach (DataRow calc_ConfigRow in src_FMM_Calc_Config_DT.Rows)
                {
                    BRApi.ErrorLog.LogMessage(si, "Hit Calc");
                    var curr_srcCalcID = calc_ConfigRow["CalcID"] != DBNull.Value
                    ? (int)calc_ConfigRow["CalcID"]
                    : -1; // Handle null Unit_IDs as needed
                    CopyCalcs(calc_ConfigRow, ref tgt_FMM_Calc_Config_DT, tgt_Cube_ID, ref XFCopyTaskResult, "Model Calc Copy");
                    foreach (DataRow ConfigRow in src_FMM_Dest_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                    {
                        CopyCell(ConfigRow, ref tgt_FMM_Dest_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                    foreach (DataRow src_ConfigRow in src_FMM_Src_Cell_DT.Select($"CalcID = {curr_srcCalcID}"))
                    {
                        Copy_Src_Cell(src_ConfigRow, ref tgt_FMM_Src_Cell_DT, tgt_Cube_ID, ref XFCopyTaskResult);
                    }
                }
                #endregion

                sql_Adapter_FMM_Calc_Config.Update_FMM_Calc_Config(si, tgt_FMM_Calc_Config_DT, sql_Data_Adapter_FMM_Calc_Config);
                sql_Adapter_FMM_Dest_Cell.Update_FMM_Dest_Cell(si, tgt_FMM_Dest_Cell_DT, sql_Data_Adapter_FMM_Dest_Cell);
                sql_Adapter_FMM_Src_Cell.Update_FMM_Src_Cell(si, tgt_FMM_Src_Cell_DT, sql_Data_Adapter_FMM_Src_Cell);
            }
        }

        #region "Get Data Functions"
        private void get_FMM_Activity_Config_Data(SqlParameter[] src_FMM_Activity_Config_parameters, SqlParameter[] tgt_FMM_Activity_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Activity_Config sql_Adapter_FMM_Activity_Config, ref DataTable src_FMM_Activity_Config_DT, ref DataTable tgt_FMM_Activity_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Activity_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Activity_Config_parameters);
            string src_FMM_Activity_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Activity_Config
				{src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Activity_Config_sqlDataAdapter, src_FMM_Activity_Config_DT, src_FMM_Activity_Config_selectQuery, src_FMM_Activity_Config_parameters);

            var tgt_FMM_Activity_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Activity_Config_parameters);
            string tgt_FMM_Activity_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Activity_Config
				{tgt_Where_Clause}";
            sql_Adapter_FMM_Activity_Config.Fill_FMM_Activity_Config_DataTable(si, tgt_FMM_Activity_Config_sqlDataAdapter, tgt_FMM_Activity_Config_DT, tgt_FMM_Activity_Config_selectQuery, tgt_FMM_Activity_Config_parameters);

            Global_FMM_Act_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Activity_Config", "Act_ID");
        }
        private void get_FMM_Unit_Config_Data(SqlParameter[] src_FMM_Unit_Config_parameters, SqlParameter[] tgt_FMM_Unit_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Unit_Config sql_Adapter_FMM_Unit_Config, ref DataTable src_FMM_Unit_Config_DT, ref DataTable tgt_FMM_Unit_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Unit_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Unit_Config_parameters);
            string src_FMM_Unit_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Unit_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Unit_Config_sqlDataAdapter, src_FMM_Unit_Config_DT, src_FMM_Unit_Config_selectQuery, src_FMM_Unit_Config_parameters);

            var tgt_FMM_Unit_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Unit_Config_parameters);
            string tgt_FMM_Unit_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Unit_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Unit_Config.Fill_FMM_Unit_Config_DataTable(si, tgt_FMM_Unit_Config_sqlDataAdapter, tgt_FMM_Unit_Config_DT, tgt_FMM_Unit_Config_selectQuery, tgt_FMM_Unit_Config_parameters);

            Global_FMM_Unit_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Unit_Config", "UnitID");
        }
        private void get_FMM_Acct_Config_Data(SqlParameter[] src_FMM_Acct_Config_parameters, SqlParameter[] tgt_FMM_Acct_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Acct_Config sql_Adapter_FMM_Acct_Config, ref DataTable src_FMM_Acct_Config_DT, ref DataTable tgt_FMM_Acct_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Acct_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Acct_Config_parameters);
            string src_FMM_Acct_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Acct_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Acct_Config_sqlDataAdapter, src_FMM_Acct_Config_DT, src_FMM_Acct_Config_selectQuery, src_FMM_Acct_Config_parameters);

            var tgt_FMM_Acct_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Acct_Config_parameters);
            string tgt_FMM_Acct_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Acct_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Acct_Config.Fill_FMM_Acct_Config_DataTable(si, tgt_FMM_Acct_Config_sqlDataAdapter, tgt_FMM_Acct_Config_DT, tgt_FMM_Acct_Config_selectQuery, tgt_FMM_Acct_Config_parameters);

            Global_FMM_Acct_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Acct_Config", "AcctID");
        }
        private void get_FMM_Appr_Config_Data(SqlParameter[] src_FMM_Appr_Config_parameters, SqlParameter[] tgt_FMM_Appr_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Appr_Config sql_Adapter_FMM_Appr_Config, ref DataTable src_FMM_Appr_Config_DT, ref DataTable tgt_FMM_Appr_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Appr_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Appr_Config_parameters);
            string src_FMM_Appr_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Appr_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Appr_Config_sqlDataAdapter, src_FMM_Appr_Config_DT, src_FMM_Appr_Config_selectQuery, src_FMM_Appr_Config_parameters);

            var tgt_FMM_Appr_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Appr_Config_parameters);
            string tgt_FMM_Appr_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Appr_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Appr_Config.Fill_FMM_Appr_Config_DataTable(si, tgt_FMM_Appr_Config_sqlDataAdapter, tgt_FMM_Appr_Config_DT, tgt_FMM_Appr_Config_selectQuery, tgt_FMM_Appr_Config_parameters);

            Global_FMM_Appr_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Appr_Config", "Appr_ID");
        }
        private void get_FMM_Approval_Step_Config_Data(SqlParameter[] src_FMM_Approval_Step_Config_parameters, SqlParameter[] tgt_FMM_Approval_Step_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Approval_Step_Config sql_Adapter_FMM_Approval_Step_Config, ref DataTable src_FMM_Approval_Step_Config_DT, ref DataTable tgt_FMM_Approval_Step_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Approval_Step_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Approval_Step_Config_parameters);
            string src_FMM_Approval_Step_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Approval_Step_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Approval_Step_Config_sqlDataAdapter, src_FMM_Approval_Step_Config_DT, src_FMM_Approval_Step_Config_selectQuery, src_FMM_Approval_Step_Config_parameters);

            var tgt_FMM_Approval_Step_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Approval_Step_Config_parameters);
            string tgt_FMM_Approval_Step_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Approval_Step_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Approval_Step_Config.Fill_FMM_Approval_Step_Config_DataTable(si, tgt_FMM_Approval_Step_Config_sqlDataAdapter, tgt_FMM_Approval_Step_Config_DT, tgt_FMM_Approval_Step_Config_selectQuery, tgt_FMM_Approval_Step_Config_parameters);

            Global_FMM_Appr_Step_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Approval_Step_Config", "Appr_Step_ID");
        }
        private void get_FMM_Reg_Config_Data(SqlParameter[] src_FMM_Reg_Config_parameters, SqlParameter[] tgt_FMM_Reg_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Reg_Config sql_Adapter_FMM_Reg_Config, ref DataTable src_FMM_Reg_Config_DT, ref DataTable tgt_FMM_Reg_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Reg_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Reg_Config_parameters);
            string src_FMM_Reg_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Reg_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Reg_Config_sqlDataAdapter, src_FMM_Reg_Config_DT, src_FMM_Reg_Config_selectQuery, src_FMM_Reg_Config_parameters);

            var tgt_FMM_Reg_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Reg_Config_parameters);
            string tgt_FMM_Reg_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Reg_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Reg_Config.Fill_FMM_Reg_Config_DataTable(si, tgt_FMM_Reg_Config_sqlDataAdapter, tgt_FMM_Reg_Config_DT, tgt_FMM_Reg_Config_selectQuery, tgt_FMM_Reg_Config_parameters);

            Global_FMM_Reg_Config_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Reg_Config", "RegConfigID");
        }
        private void get_FMM_Col_Config_Data(SqlParameter[] src_FMM_Col_Config_parameters, SqlParameter[] tgt_FMM_Col_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Col_Config sql_Adapter_FMM_Col_Config, ref DataTable src_FMM_Col_Config_DT, ref DataTable tgt_FMM_Col_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Col_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Col_Config_parameters);
            string src_FMM_Col_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Col_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Col_Config_sqlDataAdapter, src_FMM_Col_Config_DT, src_FMM_Col_Config_selectQuery, src_FMM_Col_Config_parameters);

            var tgt_FMM_Col_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Col_Config_parameters);
            string tgt_FMM_Col_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Col_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Col_Config.Fill_FMM_Col_Config_DataTable(si, tgt_FMM_Col_Config_sqlDataAdapter, tgt_FMM_Col_Config_DT, tgt_FMM_Col_Config_selectQuery, tgt_FMM_Col_Config_parameters);

            Global_FMM_Col_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Col_Config", "Col_ID");
        }
        private void get_FMM_Models_Data(SqlParameter[] src_FMM_Models_parameters, SqlParameter[] tgt_FMM_Models_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Models sql_Adapter_FMM_Models, ref DataTable src_FMM_Models_DT, ref DataTable tgt_FMM_Models_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Models_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Models_parameters);
            string src_FMM_Models_selectQuery = $@"
		        SELECT *
		        FROM FMM_Models
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Models_sqlDataAdapter, src_FMM_Models_DT, src_FMM_Models_selectQuery, src_FMM_Models_parameters);

            var tgt_FMM_Models_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Models_parameters);
            string tgt_FMM_Models_selectQuery = $@"
		        SELECT *
		        FROM FMM_Models
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Models.Fill_FMM_Models_DataTable(si, tgt_FMM_Models_sqlDataAdapter, tgt_FMM_Models_DT, tgt_FMM_Models_selectQuery, tgt_FMM_Models_parameters);

            Global_FMM_Models_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Models", "ModelID");
        }
        private void get_FMM_Calc_Config_Data(SqlParameter[] src_FMM_Calc_Config_parameters, SqlParameter[] tgt_FMM_Calc_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Calc_Config sql_Adapter_FMM_Calc_Config, ref DataTable src_FMM_Calc_Config_DT, ref DataTable tgt_FMM_Calc_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Calc_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Calc_Config_parameters);
            string src_FMM_Calc_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Calc_Config_sqlDataAdapter, src_FMM_Calc_Config_DT, src_FMM_Calc_Config_selectQuery, src_FMM_Calc_Config_parameters);

            var tgt_FMM_Calc_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Calc_Config_parameters);
            string tgt_FMM_Calc_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Calc_Config.Fill_FMM_Calc_Config_DataTable(si, tgt_FMM_Calc_Config_sqlDataAdapter, tgt_FMM_Calc_Config_DT, tgt_FMM_Calc_Config_selectQuery, tgt_FMM_Calc_Config_parameters);

            Global_FMM_Calc_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Calc_Config", "CalcID");
        }
        private void get_FMM_Dest_Cell_Data(SqlParameter[] src_FMM_Dest_Cell_parameters, SqlParameter[] tgt_FMM_Dest_Cell_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Dest_Cell sql_Adapter_FMM_Dest_Cell, ref DataTable src_FMM_Dest_Cell_DT, ref DataTable tgt_FMM_Dest_Cell_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Dest_Cell_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Dest_Cell_parameters);
            string src_FMM_Dest_Cell_selectQuery = $@"
		        SELECT *
		        FROM FMM_Dest_Cell
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Dest_Cell_sqlDataAdapter, src_FMM_Dest_Cell_DT, src_FMM_Dest_Cell_selectQuery, src_FMM_Dest_Cell_parameters);

            var tgt_FMM_Dest_Cell_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Dest_Cell_parameters);
            string tgt_FMM_Dest_Cell_selectQuery = $@"
		        SELECT *
		        FROM FMM_Dest_Cell
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Dest_Cell.Fill_FMM_Dest_Cell_DataTable(si, tgt_FMM_Dest_Cell_sqlDataAdapter, tgt_FMM_Dest_Cell_DT, tgt_FMM_Dest_Cell_selectQuery, tgt_FMM_Dest_Cell_parameters);

            Global_FMM_Dest_Cell_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Dest_Cell", "Dest_Cell_ID");
        }
        private void get_FMM_Src_Cell_Data(SqlParameter[] src_FMM_Src_Cell_parameters, SqlParameter[] tgt_FMM_Src_Cell_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Src_Cell sql_Adapter_FMM_Src_Cell, ref DataTable src_FMM_Src_Cell_DT, ref DataTable tgt_FMM_Src_Cell_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Src_Cell_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Src_Cell_parameters);
            string src_FMM_Src_Cell_selectQuery = $@"
		        SELECT *
		        FROM FMM_Src_Cell
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Src_Cell_sqlDataAdapter, src_FMM_Src_Cell_DT, src_FMM_Src_Cell_selectQuery, src_FMM_Src_Cell_parameters);

            var tgt_FMM_Src_Cell_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Src_Cell_parameters);
            string tgt_FMM_Src_Cell_selectQuery = $@"
		        SELECT *
		        FROM FMM_Src_Cell
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Src_Cell.Fill_FMM_Src_Cell_DataTable(si, tgt_FMM_Src_Cell_sqlDataAdapter, tgt_FMM_Src_Cell_DT, tgt_FMM_Src_Cell_selectQuery, tgt_FMM_Src_Cell_parameters);

            Global_FMM_Src_Cell_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Src_Cell", "CellID");
        }
        private void get_FMM_Model_Grps_Data(SqlParameter[] src_FMM_Model_Grps_parameters, SqlParameter[] tgt_FMM_Model_Grps_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Model_Grps sql_Adapter_FMM_Model_Grps, ref DataTable src_FMM_Model_Grps_DT, ref DataTable tgt_FMM_Model_Grps_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Model_Grps_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Model_Grps_parameters);
            string src_FMM_Model_Grps_selectQuery = $@"
		        SELECT *
		        FROM FMM_Model_Grps
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Model_Grps_sqlDataAdapter, src_FMM_Model_Grps_DT, src_FMM_Model_Grps_selectQuery, src_FMM_Model_Grps_parameters);

            var tgt_FMM_Model_Grps_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Model_Grps_parameters);
            string tgt_FMM_Model_Grps_selectQuery = $@"
		        SELECT *
		        FROM FMM_Model_Grps
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Model_Grps.Fill_FMM_Model_Grps_DataTable(si, tgt_FMM_Model_Grps_sqlDataAdapter, tgt_FMM_Model_Grps_DT, tgt_FMM_Model_Grps_selectQuery, tgt_FMM_Model_Grps_parameters);

            Global_FMM_Model_Grps_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Model_Grps", "Model_Grp_ID");
        }
        private void get_FMM_Model_Grp_Assign_Model_Data(SqlParameter[] src_FMM_Model_Grp_Assign_Model_parameters, SqlParameter[] tgt_FMM_Model_Grp_Assign_Model_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Model_Grp_Assign_Model sql_Adapter_FMM_Model_Grp_Assign_Model, ref DataTable src_FMM_Model_Grp_Assign_Model_DT, ref DataTable tgt_FMM_Model_Grp_Assign_Model_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Model_Grp_Assign_Model_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Model_Grp_Assign_Model_parameters);
            string src_FMM_Model_Grp_Assign_Model_selectQuery = $@"
		        SELECT *
		        FROM FMM_Model_Grp_Assign_Model
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Model_Grp_Assign_Model_sqlDataAdapter, src_FMM_Model_Grp_Assign_Model_DT, src_FMM_Model_Grp_Assign_Model_selectQuery, src_FMM_Model_Grp_Assign_Model_parameters);

            var tgt_FMM_Model_Grp_Assign_Model_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Model_Grp_Assign_Model_parameters);
            string tgt_FMM_Model_Grp_Assign_Model_selectQuery = $@"
		        SELECT *
		        FROM FMM_Model_Grp_Assign_Model
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Model_Grp_Assign_Model.Fill_FMM_Model_Grp_Assign_Model_DataTable(si, tgt_FMM_Model_Grp_Assign_Model_sqlDataAdapter, tgt_FMM_Model_Grp_Assign_Model_DT, tgt_FMM_Model_Grp_Assign_Model_selectQuery, tgt_FMM_Model_Grp_Assign_Model_parameters);

            Global_FMM_Model_Grp_Assign_Model_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Model_Grp_Assign_Model", "Model_Grp_Assign_Model_ID");
        }
        private void get_FMM_Calc_Unit_Config_Data(SqlParameter[] src_FMM_Calc_Unit_Config_parameters, SqlParameter[] tgt_FMM_Calc_Unit_Config_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Calc_Unit_Config sql_Adapter_FMM_Calc_Unit_Config, ref DataTable src_FMM_Calc_Unit_Config_DT, ref DataTable tgt_FMM_Calc_Unit_Config_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Calc_Unit_Config_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Calc_Unit_Config_parameters);
            string src_FMM_Calc_Unit_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Config
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Calc_Unit_Config_sqlDataAdapter, src_FMM_Calc_Unit_Config_DT, src_FMM_Calc_Unit_Config_selectQuery, src_FMM_Calc_Unit_Config_parameters);

            var tgt_FMM_Calc_Unit_Config_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Calc_Unit_Config_parameters);
            string tgt_FMM_Calc_Unit_Config_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Config
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Calc_Unit_Config.Fill_FMM_Calc_Unit_Config_DataTable(si, tgt_FMM_Calc_Unit_Config_sqlDataAdapter, tgt_FMM_Calc_Unit_Config_DT, tgt_FMM_Calc_Unit_Config_selectQuery, tgt_FMM_Calc_Unit_Config_parameters);

            Global_FMM_Calc_Unit_Config_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Calc_Unit_Config", "Calc_Unit_ID");
        }
        private void get_FMM_Calc_Unit_Assign_Model_Grp_Data(SqlParameter[] src_FMM_Calc_Unit_Assign_Model_Grp_parameters, SqlParameter[] tgt_FMM_Calc_Unit_Assign_Model_Grp_parameters, SQL_FMM_Get_DataSets sql_FMM_Get_DataSets, SQL_Adapter_FMM_Calc_Unit_Assign_Model_Group sql_Adapter_FMM_Calc_Unit_Assign_Model_Group, ref DataTable src_FMM_Calc_Unit_Assign_Model_Grp_DT, ref DataTable tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, SQL_FMM_Get_Max_ID sql_FMM_Get_Max_ID)
        {
            var src_FMM_Calc_Unit_Assign_Model_Grp_sqlDataAdapter = new SqlDataAdapter();
            // Construct WHERE clause for the source query
            string src_Where_Clause = Construct_Where_Clause(src_FMM_Calc_Unit_Assign_Model_Grp_parameters);
            string src_FMM_Calc_Unit_Assign_Model_Grp_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Assign_Model_Group
		        {src_Where_Clause}";
            sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, src_FMM_Calc_Unit_Assign_Model_Grp_sqlDataAdapter, src_FMM_Calc_Unit_Assign_Model_Grp_DT, src_FMM_Calc_Unit_Assign_Model_Grp_selectQuery, src_FMM_Calc_Unit_Assign_Model_Grp_parameters);

            var tgt_FMM_Calc_Unit_Assign_Model_Grp_sqlDataAdapter = new SqlDataAdapter();
            string tgt_Where_Clause = Construct_Where_Clause(tgt_FMM_Calc_Unit_Assign_Model_Grp_parameters);
            string tgt_FMM_Calc_Unit_Assign_Model_Grp_selectQuery = $@"
		        SELECT *
		        FROM FMM_Calc_Unit_Assign_Model_Group
		        {tgt_Where_Clause}";
            sql_Adapter_FMM_Calc_Unit_Assign_Model_Group.Fill_FMM_Calc_Unit_Assign_Model_Grp_DataTable(si, tgt_FMM_Calc_Unit_Assign_Model_Grp_sqlDataAdapter, tgt_FMM_Calc_Unit_Assign_Model_Grp_DT, tgt_FMM_Calc_Unit_Assign_Model_Grp_selectQuery, tgt_FMM_Calc_Unit_Assign_Model_Grp_parameters);

            Global_FMM_Calc_Unit_Assign_Model_Grp_ID = sql_FMM_Get_Max_ID.Get_Max_ID(si, "FMM_Calc_Unit_Assign_Model_Group", "Calc_Unit_Assign_Model_Grp_ID");
        }
        #endregion

        #region "Copy Model Data"
        private void CopyActivities(DataRow src_FMM_Activity_Config_Row, ref DataTable tgt_FMM_Activity_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target CubeID activities for duplicate Name and CalcType
            bool isDuplicate = tgt_FMM_Activity_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Activity_Config_Row["Name"].ToString() &&
                            row["CalcType"].ToString() == src_FMM_Activity_Config_Row["CalcType"].ToString());

            // If not duplicate, add the new Activity Name and CalcType to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Act_ID += 1;
                Global_Curr_FMM_Act_ID = Global_FMM_Act_ID;
                DataRow newTargetRow = tgt_FMM_Activity_Config_DT.NewRow();

                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_FMM_Act_ID;
                newTargetRow["Name"] = src_FMM_Activity_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["CalcType"] = src_FMM_Activity_Config_Row.Field<string>("CalcType") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as "Build"
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // Set the appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // Set the appropriate user context

                tgt_FMM_Activity_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Activity_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Activity_Config_Row["Name"].ToString() &&
                                           row["CalcType"].ToString() == src_FMM_Activity_Config_Row["CalcType"].ToString());
                if (existingRow != null)
                {
                    Global_Curr_FMM_Act_ID = existingRow.Field<int>("Act_ID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // Set the appropriate user context
                }
            }
        }
        private void CopyUnits(DataRow src_FMM_Unit_Config_Row, ref DataTable tgt_FMM_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID units for duplicate UnitName
            bool isDuplicate = tgt_FMM_Unit_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Unit to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Unit_ID += 1;
                Global_Curr_FMM_Unit_ID = Global_FMM_Unit_ID;
                DataRow newTargetRow = tgt_FMM_Unit_Config_DT.NewRow();

                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["UnitID"] = Global_FMM_Unit_ID;
                newTargetRow["Name"] = src_FMM_Unit_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // Set appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_FMM_Unit_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Unit_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Unit_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Unit_ID = existingRow.Field<int>("UnitID");
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void CopyAccts(DataRow src_FMM_Acct_Config_Row, ref DataTable tgt_FMM_Acct_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID accounts for duplicate AcctName
            bool isDuplicate = tgt_FMM_Acct_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["UnitID"].ToString() == Global_Curr_FMM_Unit_ID.ToString());

            // If not duplicate, add the new Account to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Acct_ID += 1;
                Global_Curr_FMM_Acct_ID = Global_FMM_Acct_ID;
                DataRow newTargetRow = tgt_FMM_Acct_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["UnitID"] = Global_Curr_FMM_Unit_ID;
                newTargetRow["AcctID"] = Global_FMM_Acct_ID;
                newTargetRow["Name"] = src_FMM_Acct_Config_Row.Field<string>("AcctName") ?? string.Empty; // Handle nulls
                newTargetRow["MapLogic"] = src_FMM_Acct_Config_Row.Field<string>("MapLogic") ?? string.Empty; // Handle nulls
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_FMM_Acct_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Acct_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Acct_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                           row["UnitID"].ToString() == Global_Curr_FMM_Unit_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Acct_ID = existingRow.Field<int>("AcctID");

                    // Update fields, handle nulls using Field<T>
                    existingRow["MapLogic"] = src_FMM_Acct_Config_Row.Field<string>("MapLogic") ?? existingRow.Field<string>("MapLogic");
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Reg_Config(DataRow src_Reg_Config_Row, ref DataTable tgt_Reg_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target CubeID register config for duplicate RegisterName
            bool isDuplicate = tgt_Reg_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Register Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Reg_Config_ID += 1;
                Global_Curr_FMM_Reg_Config_ID = Global_FMM_Reg_Config_ID;
                DataRow newTargetRow = tgt_Reg_Config_DT.NewRow();

                // Insert values using Field<T> and handling nulls
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["RegConfigID"] = Global_FMM_Reg_Config_ID;
                newTargetRow["Name"] = src_Reg_Config_Row.Field<string>("Name") ?? string.Empty; // Handle nulls
                newTargetRow["TimePhasing"] = src_Reg_Config_Row.Field<string>("TimePhasing") ?? string.Empty; // Handle nulls
                newTargetRow["StartDtSrc"] = src_Reg_Config_Row.Field<string>("StartDtSrc") ?? string.Empty; // Handle nulls
                newTargetRow["EndDtSrc"] = src_Reg_Config_Row.Field<string>("EndDtSrc") ?? string.Empty; // Handle nulls
                newTargetRow["ApprovalConfig"] = src_Reg_Config_Row.Field<string>("ApprovalConfig") ?? string.Empty; // Handle nulls
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // Set appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_Reg_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Reg_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Reg_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Reg_Config_ID = existingRow.Field<int>("RegConfigID");

                    // Update fields, handle nulls using Field<T>
                    existingRow["TimePhasing"] = src_Reg_Config_Row.Field<string>("TimePhasing") ?? existingRow.Field<string>("TimePhasing");
                    existingRow["StartDtSrc"] = src_Reg_Config_Row.Field<string>("StartDtSrc") ?? existingRow.Field<string>("StartDtSrc");
                    existingRow["EndDtSrc"] = src_Reg_Config_Row.Field<string>("EndDtSrc") ?? existingRow.Field<string>("EndDtSrc");
                    existingRow["ApprovalConfig"] = src_Reg_Config_Row.Field<string>("ApprovalConfig") ?? existingRow.Field<string>("ApprovalConfig");
                    existingRow["Status"] = row_Status;
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Col_Config(DataRow src_Col_Config_Row, ref DataTable tgt_Col_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target CubeID column config for duplicate ColName
            bool isDuplicate = tgt_Col_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["RegConfigID"].ToString() == Global_Curr_FMM_Reg_Config_ID.ToString());

            // If not duplicate, add the new Column Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Col_ID += 1;
                Global_Curr_FMM_Col_ID = Global_FMM_Col_ID;
                DataRow newTargetRow = tgt_Col_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["RegConfigID"] = Global_Curr_FMM_Reg_Config_ID;
                newTargetRow["Col_ID"] = Global_FMM_Col_ID;
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
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Col_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Col_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Col_Config_Row["Name"].ToString() &&
                                    row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                    row["RegConfigID"].ToString() == Global_Curr_FMM_Reg_Config_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Col_ID = existingRow.Field<int>("Col_ID");
                    existingRow["InUse"] = src_Col_Config_Row.Field<bool?>("InUse") ?? false;
                    existingRow["Required"] = src_Col_Config_Row.Field<bool?>("Required") ?? false;
                    existingRow["Updates"] = src_Col_Config_Row.Field<bool?>("Updates") ?? false;
                    existingRow["Alias"] = src_Col_Config_Row.Field<string>("Alias") ?? string.Empty;
                    existingRow["Order"] = src_Col_Config_Row.Field<int?>("Order") ?? 0;
                    existingRow["Default"] = src_Col_Config_Row.Field<string>("Default") ?? string.Empty;
                    existingRow["Param"] = src_Col_Config_Row.Field<string>("Param") ?? string.Empty;
                    existingRow["Format"] = src_Col_Config_Row.Field<string>("Format") ?? string.Empty;
                    existingRow["Status"] = row_Status; // Set initial status as appropriate
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void CopyApprovals(DataRow src_FMM_Appr_Config_Row, ref DataTable tgt_FMM_Appr_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            BRApi.ErrorLog.LogMessage(si, "Hit Approval");
            var row_Status = "Build";
            // Check the target CubeID approvals for duplicate Approval_Name
            bool isDuplicate = tgt_FMM_Appr_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_FMM_Appr_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Approval to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Appr_ID += 1;
                Global_Curr_FMM_Appr_ID = Global_FMM_Appr_ID;
                DataRow newTargetRow = tgt_FMM_Appr_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["Appr_ID"] = Global_FMM_Appr_ID;
                newTargetRow["Name"] = src_FMM_Appr_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_FMM_Appr_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Appr_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_FMM_Appr_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Appr_ID = existingRow.Field<int>("Appr_ID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Approval_Steps(DataRow src_FMM_Approval_Step_Config_Row, ref DataTable tgt_FMM_Approval_Step_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target CubeID approval steps for duplicate StepName
            bool isDuplicate = tgt_FMM_Approval_Step_Config_DT.AsEnumerable()
                .Any(row => row["StepNum"].ToString() == src_FMM_Approval_Step_Config_Row["StepNum"].ToString() &&
                            row["WFProfile_Step"].ToString() == src_FMM_Approval_Step_Config_Row["WFProfile_Step"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["Appr_ID"].ToString() == Global_Curr_FMM_Appr_ID.ToString());

            // If not duplicate, add the new Approval Step to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Appr_Step_ID += 1;
                Global_Curr_FMM_Appr_Step_ID = Global_FMM_Appr_Step_ID;
                DataRow newTargetRow = tgt_FMM_Approval_Step_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_FMM_Act_ID;
                newTargetRow["Appr_ID"] = Global_FMM_Appr_ID;
                newTargetRow["Appr_Step_ID"] = Global_FMM_Appr_Step_ID;
                newTargetRow["StepNum"] = src_FMM_Approval_Step_Config_Row.Field<int?>("StepNum") ?? 0;
                newTargetRow["WFProfile_Step"] = src_FMM_Approval_Step_Config_Row.Field<Guid?>("WFProfile_Step") ?? Guid.Empty;
                newTargetRow["Approval_User_Group"] = src_FMM_Approval_Step_Config_Row.Field<string>("Approval_User_Group") ?? string.Empty;
                newTargetRow["ApprovalLogic"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovalLogic") ?? string.Empty;
                newTargetRow["ApprovalItem"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovalItem") ?? string.Empty;
                newTargetRow["ApprovalLevel"] = src_FMM_Approval_Step_Config_Row.Field<int?>("ApprovalLevel") ?? 0;
                newTargetRow["ApprovalConfig"] = src_FMM_Approval_Step_Config_Row.Field<int?>("ApprovalConfig") ?? 0;
                newTargetRow["InitStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                newTargetRow["ApprovedStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovedStatus") ?? string.Empty;
                newTargetRow["RejectionStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("RejectionStatus") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_FMM_Approval_Step_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_FMM_Approval_Step_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["StepNum"].ToString() == src_FMM_Approval_Step_Config_Row["StepNum"].ToString() &&
                                           row["WFProfile_Step"].ToString() == src_FMM_Approval_Step_Config_Row["WFProfile_Step"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                           row["Appr_ID"].ToString() == Global_Curr_FMM_Appr_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Appr_Step_ID = existingRow.Field<int>("Appr_Step_ID");
                    existingRow["Approval_User_Group"] = src_FMM_Approval_Step_Config_Row.Field<string>("Approval_User_Group") ?? string.Empty;
                    existingRow["ApprovalLogic"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovalLogic") ?? string.Empty;
                    existingRow["ApprovalItem"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovalItem") ?? string.Empty;
                    existingRow["ApprovalLevel"] = src_FMM_Approval_Step_Config_Row.Field<int?>("ApprovalLevel") ?? 0;
                    existingRow["ApprovalConfig"] = src_FMM_Approval_Step_Config_Row.Field<int?>("ApprovalConfig") ?? 0;
                    existingRow["InitStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("InitStatus") ?? string.Empty;
                    existingRow["ApprovedStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("ApprovedStatus") ?? string.Empty;
                    existingRow["RejectionStatus"] = src_FMM_Approval_Step_Config_Row.Field<string>("RejectionStatus") ?? string.Empty;
                    existingRow["Status"] = row_Status; // Set initial status as appropriate
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void CopyModels(DataRow src_Model_Config_Row, ref DataTable tgt_Model_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            var row_Status = "Build";
            // Check the target CubeID model config for duplicate ModelName
            bool isDuplicate = tgt_Model_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

            // If not duplicate, add the new Model Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Models_ID += 1;
                Global_Curr_FMM_Models_ID = Global_FMM_Models_ID;
                DataRow newTargetRow = tgt_Model_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["ModelID"] = Global_FMM_Models_ID;
                newTargetRow["Name"] = src_Model_Config_Row.Field<string>("Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Model_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Model_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Models_ID = existingRow.Field<int>("ModelID");
                    existingRow["Status"] = row_Status; // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void CopyCalcs(DataRow src_Calc_Config_Row, ref DataTable tgt_Calc_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult, string RunType)
        {
            var row_Status = "Build";
            // Check the target CubeID calculation config for duplicate CalcName and CalcType
            bool isDuplicate = tgt_Calc_Config_DT.AsEnumerable()
                .Any(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                            row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString());

            // If not duplicate, add the new Calculation Config to the target DataTable
            if (RunType == "Model Calc Copy" || !isDuplicate)
            {
                Global_FMM_Calc_ID += 1;
                Global_Curr_FMM_Calc_ID = Global_FMM_Calc_ID;

                DataRow newTargetRow = tgt_Calc_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["ModelID"] = Global_Curr_FMM_Models_ID;
                newTargetRow["CalcID"] = Global_FMM_Calc_ID;
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
                newTargetRow["CalcCondition"] = src_Calc_Config_Row.Field<string>("CalcCondition") ?? string.Empty;
                newTargetRow["CalcExplanation"] = src_Calc_Config_Row.Field<string>("CalcExplanation") ?? string.Empty;
                newTargetRow["BalancedBuffer"] = src_Calc_Config_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                newTargetRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                newTargetRow["UnbalCalc"] = src_Calc_Config_Row.Field<string>("UnbalCalc") ?? string.Empty;
                newTargetRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                newTargetRow["TimePhasing"] = src_Calc_Config_Row.Field<string>("TimePhasing") ?? string.Empty;
                newTargetRow["InputFrequency"] = src_Calc_Config_Row.Field<string>("InputFrequency") ?? string.Empty;
                newTargetRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                newTargetRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                newTargetRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                newTargetRow["Status"] = row_Status;
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Calc_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Calc_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Name"].ToString() == src_Calc_Config_Row["Name"].ToString() &&
                                           row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                           row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Calc_ID = existingRow.Field<int>("CalcID");
                    existingRow["Status"] = row_Status;
                    existingRow["Sequence"] = src_Calc_Config_Row.Field<int?>("Sequence") ?? 0;
                    existingRow["CalcCondition"] = src_Calc_Config_Row.Field<string>("CalcCondition") ?? string.Empty;
                    existingRow["CalcExplanation"] = src_Calc_Config_Row.Field<string>("CalcExplanation") ?? string.Empty;
                    existingRow["BalancedBuffer"] = src_Calc_Config_Row.Field<string>("BalancedBuffer") ?? string.Empty;
                    existingRow["bal_buffer_calc"] = src_Calc_Config_Row.Field<string>("bal_buffer_calc") ?? string.Empty;
                    existingRow["UnbalCalc"] = src_Calc_Config_Row.Field<string>("UnbalCalc") ?? string.Empty;
                    existingRow["Table_Calc_SQL_Logic"] = src_Calc_Config_Row.Field<string>("Table_Calc_SQL_Logic") ?? string.Empty;
                    existingRow["TimePhasing"] = src_Calc_Config_Row.Field<string>("TimePhasing") ?? string.Empty;
                    existingRow["InputFrequency"] = src_Calc_Config_Row.Field<string>("InputFrequency") ?? string.Empty;
                    existingRow["MultiDim_Alloc"] = src_Calc_Config_Row.Field<bool?>("MultiDim_Alloc") ?? false;
                    existingRow["BR_Calc"] = src_Calc_Config_Row.Field<bool?>("BR_Calc") ?? false;
                    existingRow["BR_Calc_Name"] = src_Calc_Config_Row.Field<string>("BR_Calc_Name") ?? string.Empty;
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void CopyCell(DataRow src_Cell_Config_Row, ref DataTable tgt_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID destination cell config for duplicate CellName and CellType
            bool isDuplicate = tgt_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString() &&
                            row["CalcID"].ToString() == Global_Curr_FMM_Calc_ID.ToString());

            // If not duplicate, add the new Destination Cell Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Dest_Cell_ID += 1;
                Global_Curr_FMM_Dest_Cell_ID = Global_FMM_Dest_Cell_ID;

                DataRow newTargetRow = tgt_Cell_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID; // Correct Act_ID assignment
                newTargetRow["ModelID"] = Global_Curr_FMM_Models_ID;
                newTargetRow["CalcID"] = Global_Curr_FMM_Calc_ID;
                newTargetRow["Dest_Cell_ID"] = Global_FMM_Dest_Cell_ID;

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
                newTargetRow["TimeFilter"] = src_Cell_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                newTargetRow["AcctFilter"] = src_Cell_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                newTargetRow["OriginFilter"] = src_Cell_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                newTargetRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                newTargetRow["FlowFilter"] = src_Cell_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                newTargetRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                newTargetRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                newTargetRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                newTargetRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                newTargetRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                newTargetRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                newTargetRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                newTargetRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                newTargetRow["ConditionalFilter"] = src_Cell_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                newTargetRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                newTargetRow["BufferFilter"] = src_Cell_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                newTargetRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                newTargetRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;

                tgt_Cell_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                            row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString() &&
                                           row["CalcID"].ToString() == Global_Curr_FMM_Calc_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Dest_Cell_ID = existingRow.Field<int>("Dest_Cell_ID");
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
                    existingRow["TimeFilter"] = src_Cell_Config_Row.Field<string>("TimeFilter") ?? string.Empty;
                    existingRow["AcctFilter"] = src_Cell_Config_Row.Field<string>("AcctFilter") ?? string.Empty;
                    existingRow["OriginFilter"] = src_Cell_Config_Row.Field<string>("OriginFilter") ?? string.Empty;
                    existingRow["IC_Filter"] = src_Cell_Config_Row.Field<string>("IC_Filter") ?? string.Empty;
                    existingRow["FlowFilter"] = src_Cell_Config_Row.Field<string>("FlowFilter") ?? string.Empty;
                    existingRow["UD1_Filter"] = src_Cell_Config_Row.Field<string>("UD1_Filter") ?? string.Empty;
                    existingRow["UD2_Filter"] = src_Cell_Config_Row.Field<string>("UD2_Filter") ?? string.Empty;
                    existingRow["UD3_Filter"] = src_Cell_Config_Row.Field<string>("UD3_Filter") ?? string.Empty;
                    existingRow["UD4_Filter"] = src_Cell_Config_Row.Field<string>("UD4_Filter") ?? string.Empty;
                    existingRow["UD5_Filter"] = src_Cell_Config_Row.Field<string>("UD5_Filter") ?? string.Empty;
                    existingRow["UD6_Filter"] = src_Cell_Config_Row.Field<string>("UD6_Filter") ?? string.Empty;
                    existingRow["UD7_Filter"] = src_Cell_Config_Row.Field<string>("UD7_Filter") ?? string.Empty;
                    existingRow["UD8_Filter"] = src_Cell_Config_Row.Field<string>("UD8_Filter") ?? string.Empty;
                    existingRow["ConditionalFilter"] = src_Cell_Config_Row.Field<string>("ConditionalFilter") ?? string.Empty;
                    existingRow["Curr_Cube_Buffer_Filter"] = src_Cell_Config_Row.Field<string>("Curr_Cube_Buffer_Filter") ?? string.Empty;
                    existingRow["BufferFilter"] = src_Cell_Config_Row.Field<string>("BufferFilter") ?? string.Empty;
                    existingRow["Dest_Cell_Logic"] = src_Cell_Config_Row.Field<string>("Dest_Cell_Logic") ?? string.Empty;
                    existingRow["SQL_Logic"] = src_Cell_Config_Row.Field<string>("SQL_Logic") ?? string.Empty;
                }
            }
        }
        private void Copy_Src_Cell(DataRow src_Src_Cell_Config_Row, ref DataTable tgt_Src_Cell_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            Global_FMM_Src_Cell_ID += 1;
            Global_Curr_FMM_Src_Cell_ID = Global_FMM_Src_Cell_ID;
            // Check the target CubeID source cell config for duplicate SrcItem and SrcType
            bool isDuplicate = tgt_Src_Cell_Config_DT.AsEnumerable()
                .Any(row => row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                            row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString() &&
                            row["CalcID"].ToString() == Global_Curr_FMM_Calc_ID.ToString() &&
                            row["CellID"].ToString() == Global_Curr_FMM_Src_Cell_ID.ToString());

            // If not duplicate, add the new Source Cell Config to the target DataTable
            if (!isDuplicate)
            {
                DataRow newTargetRow = tgt_Src_Cell_Config_DT.NewRow();

                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = Global_Curr_FMM_Act_ID;
                newTargetRow["ModelID"] = Global_Curr_FMM_Models_ID;
                newTargetRow["CalcID"] = Global_Curr_FMM_Calc_ID;
                newTargetRow["CellID"] = Global_FMM_Src_Cell_ID;
                newTargetRow["SrcOrder"] = src_Src_Cell_Config_Row.Field<int?>("SrcOrder") ?? 0;
                newTargetRow["SrcType"] = src_Src_Cell_Config_Row.Field<string>("SrcType") ?? string.Empty;
                newTargetRow["SrcItem"] = src_Src_Cell_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                newTargetRow["OpenParens"] = src_Src_Cell_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                newTargetRow["MathOperator"] = src_Src_Cell_Config_Row.Field<string>("MathOperator") ?? string.Empty;
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
                newTargetRow["CloseParens"] = src_Src_Cell_Config_Row.Field<string>("CloseParens") ?? string.Empty;
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
                newTargetRow["Dyn_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                newTargetRow["OverrideValue"] = src_Src_Cell_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                newTargetRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                newTargetRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                newTargetRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                newTargetRow["MapType"] = src_Src_Cell_Config_Row.Field<string>("MapType") ?? string.Empty;
                newTargetRow["MapSource"] = src_Src_Cell_Config_Row.Field<string>("MapSource") ?? string.Empty;
                newTargetRow["MapLogic"] = src_Src_Cell_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                newTargetRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                newTargetRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                newTargetRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // Set appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // Set appropriate user context

                tgt_Src_Cell_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Src_Cell_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["Act_ID"].ToString() == Global_Curr_FMM_Act_ID.ToString() &&
                                           row["ModelID"].ToString() == Global_Curr_FMM_Models_ID.ToString() &&
                                           row["CalcID"].ToString() == Global_Curr_FMM_Calc_ID.ToString() &&
                                           row["CellID"].ToString() == Global_Curr_FMM_Src_Cell_ID.ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Src_Cell_ID = Convert.ToInt32(existingRow["CellID"].ToString());
                    // Update fields as necessary
                    existingRow["SrcOrder"] = src_Src_Cell_Config_Row.Field<int?>("SrcOrder") ?? 0;
                    existingRow["SrcType"] = src_Src_Cell_Config_Row.Field<string>("SrcType") ?? string.Empty;
                    existingRow["SrcItem"] = src_Src_Cell_Config_Row.Field<string>("SrcItem") ?? string.Empty;
                    existingRow["OpenParens"] = src_Src_Cell_Config_Row.Field<string>("OpenParens") ?? string.Empty;
                    existingRow["MathOperator"] = src_Src_Cell_Config_Row.Field<string>("MathOperator") ?? string.Empty;
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
                    existingRow["CloseParens"] = src_Src_Cell_Config_Row.Field<string>("CloseParens") ?? string.Empty;
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
                    existingRow["Dyn_Calc_Script"] = src_Src_Cell_Config_Row.Field<string>("Dyn_Calc_Script") ?? string.Empty;
                    existingRow["OverrideValue"] = src_Src_Cell_Config_Row.Field<string>("OverrideValue") ?? string.Empty;
                    existingRow["Table_Calc_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Calc_Expression") ?? string.Empty;
                    existingRow["Table_Join_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Join_Expression") ?? string.Empty;
                    existingRow["Table_Filter_Expression"] = src_Src_Cell_Config_Row.Field<string>("Table_Filter_Expression") ?? string.Empty;
                    existingRow["MapType"] = src_Src_Cell_Config_Row.Field<string>("MapType") ?? string.Empty;
                    existingRow["MapSource"] = src_Src_Cell_Config_Row.Field<string>("MapSource") ?? string.Empty;
                    existingRow["MapLogic"] = src_Src_Cell_Config_Row.Field<string>("MapLogic") ?? string.Empty;
                    existingRow["Src_SQL_Stmt"] = src_Src_Cell_Config_Row.Field<string>("Src_SQL_Stmt") ?? string.Empty;
                    existingRow["Use_Temp_Table"] = src_Src_Cell_Config_Row.Field<bool>("Use_Temp_Table");
                    existingRow["Temp_Table_Name"] = src_Src_Cell_Config_Row.Field<string>("Temp_Table_Name") ?? string.Empty;
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // Set appropriate user context
                }
            }
        }
        private void Copy_Model_Groups(DataRow src_Model_Grp_Config_Row, ref DataTable tgt_Model_Grp_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID model group config for duplicate GroupName
            bool isDuplicate = tgt_Model_Grp_Config_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                            row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

            // If not duplicate, add the new Model Group Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Model_Grps_ID += 1;
                Global_Curr_FMM_Model_Grps_ID = Global_FMM_Model_Grps_ID;
                DataRow newTargetRow = tgt_Model_Grp_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Model_Grp_Config_ID"] = Global_FMM_Model_Grps_ID;
                newTargetRow["GroupName"] = src_Model_Grp_Config_Row["GroupName"].ToString();
                newTargetRow["GroupType"] = src_Model_Grp_Config_Row["GroupType"].ToString();
                newTargetRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Config_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Grp_Config_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Config_Row["GroupName"].ToString() &&
                                           row["GroupType"].ToString() == src_Model_Grp_Config_Row["GroupType"].ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Model_Grps_ID = Convert.ToInt32(existingRow["Model_Grp_Config_ID"].ToString());
                    existingRow["Description"] = src_Model_Grp_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Model_Grp_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Model_Grp_Assign_Model(DataRow src_Model_Grp_Assign_Model_Row, ref DataTable tgt_Model_Grp_Assign_Model_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID model group assignment config for duplicate GroupName and ModelName
            bool isDuplicate = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                .Any(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                            row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

            // If not duplicate, add the new Model Group Assign Model Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Model_Grp_Assign_Model_ID += 1;
                Global_Curr_FMM_Model_Grp_Assign_Model_ID = Global_FMM_Model_Grp_Assign_Model_ID;
                DataRow newTargetRow = tgt_Model_Grp_Assign_Model_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Model_Grp_Assign_Model_ID"] = Global_FMM_Model_Grp_Assign_Model_ID;
                newTargetRow["GroupName"] = src_Model_Grp_Assign_Model_Row["GroupName"].ToString();
                newTargetRow["ModelName"] = src_Model_Grp_Assign_Model_Row["ModelName"].ToString();
                newTargetRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Model_Grp_Assign_Model_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Model_Grp_Assign_Model_DT.AsEnumerable()
                    .FirstOrDefault(row => row["GroupName"].ToString() == src_Model_Grp_Assign_Model_Row["GroupName"].ToString() &&
                                           row["ModelName"].ToString() == src_Model_Grp_Assign_Model_Row["ModelName"].ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Model_Grp_Assign_Model_ID = Convert.ToInt32(existingRow["Model_Grp_Assign_Model_ID"].ToString());
                    existingRow["Description"] = src_Model_Grp_Assign_Model_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Model_Grp_Assign_Model_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calc_Unit_Config(DataRow src_Calc_Unit_Config_Row, ref DataTable tgt_Calc_Unit_Config_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID workflow DU config for duplicate DU_Name
            bool isDuplicate = tgt_Calc_Unit_Config_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Config_Row["DU_Name"].ToString() &&
                            row["DU_Type"].ToString() == src_Calc_Unit_Config_Row["DU_Type"].ToString());

            // If not duplicate, add the new Workflow DU Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Calc_Unit_Config_ID += 1;
                Global_Curr_FMM_Calc_Unit_Config_ID = Global_FMM_Calc_Unit_Config_ID;
                DataRow newTargetRow = tgt_Calc_Unit_Config_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Calc_Unit_Config_ID"] = Global_FMM_Calc_Unit_Config_ID;
                newTargetRow["DU_Name"] = src_Calc_Unit_Config_Row["DU_Name"].ToString();
                newTargetRow["DU_Type"] = src_Calc_Unit_Config_Row["DU_Type"].ToString();
                newTargetRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

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
                    Global_Curr_FMM_Calc_Unit_Config_ID = Convert.ToInt32(existingRow["Calc_Unit_Config_ID"].ToString());
                    existingRow["Description"] = src_Calc_Unit_Config_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Calc_Unit_Config_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        private void Copy_Calc_Unit_Assign_Model_Group(DataRow src_Calc_Unit_Assign_Model_Grp_Row, ref DataTable tgt_Calc_Unit_Assign_Model_Grp_DT, int targetCubeID, ref XFSelectionChangedTaskResult XFCopyTaskResult)
        {
            // Check the target CubeID for duplicate DU_Name and Model_Grp_Name in the assignment config
            bool isDuplicate = tgt_Calc_Unit_Assign_Model_Grp_DT.AsEnumerable()
                .Any(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Model_Grp_Row["DU_Name"].ToString() &&
                            row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Model_Grp_Row["Model_Grp_Name"].ToString());

            // If not duplicate, add the new Workflow DU Assign Model Group Config to the target DataTable
            if (!isDuplicate)
            {
                Global_FMM_Calc_Unit_Assign_Model_Grp_ID += 1;
                Global_Curr_FMM_Calc_Unit_Assign_Model_Grp_ID = Global_FMM_Calc_Unit_Assign_Model_Grp_ID;
                DataRow newTargetRow = tgt_Calc_Unit_Assign_Model_Grp_DT.NewRow();
                newTargetRow["CubeID"] = targetCubeID;
                newTargetRow["Act_ID"] = targetCubeID;
                newTargetRow["Calc_Unit_Assign_Model_Grp_ID"] = Global_FMM_Calc_Unit_Assign_Model_Grp_ID;
                newTargetRow["DU_Name"] = src_Calc_Unit_Assign_Model_Grp_Row["DU_Name"].ToString();
                newTargetRow["Model_Grp_Name"] = src_Calc_Unit_Assign_Model_Grp_Row["Model_Grp_Name"].ToString();
                newTargetRow["Description"] = src_Calc_Unit_Assign_Model_Grp_Row["Description"].ToString();
                newTargetRow["Status"] = "Build"; // Set initial status as appropriate
                newTargetRow["CreateDate"] = DateTime.Now;
                newTargetRow["CreateUser"] = si.UserName; // or appropriate user context
                newTargetRow["UpdateDate"] = DateTime.Now;
                newTargetRow["UpdateUser"] = si.UserName; // or appropriate user context

                tgt_Calc_Unit_Assign_Model_Grp_DT.Rows.Add(newTargetRow);
            }
            else
            {
                // Find the matching row and update it
                DataRow existingRow = tgt_Calc_Unit_Assign_Model_Grp_DT.AsEnumerable()
                    .FirstOrDefault(row => row["DU_Name"].ToString() == src_Calc_Unit_Assign_Model_Grp_Row["DU_Name"].ToString() &&
                                           row["Model_Grp_Name"].ToString() == src_Calc_Unit_Assign_Model_Grp_Row["Model_Grp_Name"].ToString());

                if (existingRow != null)
                {
                    Global_Curr_FMM_Calc_Unit_Assign_Model_Grp_ID = Convert.ToInt32(existingRow["Calc_Unit_Assign_Model_Grp_ID"].ToString());
                    existingRow["Description"] = src_Calc_Unit_Assign_Model_Grp_Row["Description"].ToString(); // Update description or other fields as needed
                    existingRow["Status"] = src_Calc_Unit_Assign_Model_Grp_Row["Status"].ToString(); // Update status or other fields as needed
                    existingRow["UpdateDate"] = DateTime.Now;
                    existingRow["UpdateUser"] = si.UserName; // or appropriate user context
                }
            }
        }
        #endregion

        #region "Helper Functions"
        private string Construct_Where_Clause(SqlParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                return string.Empty;
            }

            var whereClause = new List<string>();

            foreach (var param in parameters)
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
        private void Duplicate_Model_List(int CubeID, int Act_ID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult Save_Data_Task_Result)
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
                            var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sql_DataSet_DataAdapter = new SqlDataAdapter();
                            var FMM_Models_DT = new DataTable();
                            // Define the select query and parameters
                            string select_DS_Query = @"
		                        SELECT *
		                        FROM FMM_Models
		                        WHERE CubeID = @CubeID
								AND Act_ID = @Act_ID";
                            // Create an array of SqlParameter objects
                            var ds_parameters = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID},
                                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Act_ID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sql_DataSet_DataAdapter, FMM_Models_DT, select_DS_Query, ds_parameters);
                            }
                            catch (Exception ex)
                            {
                                Save_Data_Task_Result.IsOK = false;
                                Save_Data_Task_Result.ShowMessageBox = true;
                                Save_Data_Task_Result.Message = $"Error fetching data for CubeID {CubeID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate Global_Model_List_Dict and handle errors
                            foreach (DataRow cube_Model_Row in FMM_Models_DT.Rows)
                            {
                                string ModelKey = CubeID + "|" + Act_ID + "|" + (int)cube_Model_Row["ModelID"];
                                Global_Models_Dict.Add(ModelKey, (string)cube_Model_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the Save_Data_Task_Result
                Save_Data_Task_Result.IsOK = false;
                Save_Data_Task_Result.ShowMessageBox = true;
                Save_Data_Task_Result.Message = $"An unexpected error occurred.";
            }
        }

        private void Duplicate_Model_Grp_List(int CubeID, String Dup_Process_Step, ref XFSqlTableEditorSaveDataTaskResult Save_Data_Task_Result)
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
                            var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                            connection.Open();

                            // Create a new DataTable
                            var sql_DataSet_DataAdapter = new SqlDataAdapter();
                            var FMM_Model_Grps_DT = new DataTable();
                            // Define the select query and parameters
                            string select_DS_Query = @"
		                        SELECT *
		                        FROM FMM_Model_Grps
		                        WHERE CubeID = @CubeID";
                            // Create an array of SqlParameter objects
                            var ds_parameters = new SqlParameter[]
                            {
                                new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID}
                            };

                            // Attempt to fill the data table and check for any issues
                            try
                            {
                                sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sql_DataSet_DataAdapter, FMM_Model_Grps_DT, select_DS_Query, ds_parameters);
                            }
                            catch (Exception ex)
                            {
                                Save_Data_Task_Result.IsOK = false;
                                Save_Data_Task_Result.ShowMessageBox = true;
                                Save_Data_Task_Result.Message = $"Error fetching data for CubeID {CubeID}";
                                return; // Exit the process if data retrieval fails
                            }

                            // Populate Global_Model_Grp_List_Dict and handle errors
                            foreach (DataRow cube_Model_Grp_Row in FMM_Model_Grps_DT.Rows)
                            {
                                string Model_GroupKey = CubeID + "|" + (int)cube_Model_Grp_Row["Model_Grp_ID"];
                                Global_Model_Groups_Dict.Add(Model_GroupKey, (string)cube_Model_Grp_Row["Name"]);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                // Catch any unhandled exceptions and log to the Save_Data_Task_Result
                Save_Data_Task_Result.IsOK = false;
                Save_Data_Task_Result.ShowMessageBox = true;
                Save_Data_Task_Result.Message = $"An unexpected error occurred.";
            }
        }

        #endregion
        #region "Duplicate Approval Steps"

        #endregion
        #endregion


        #endregion
    }
}