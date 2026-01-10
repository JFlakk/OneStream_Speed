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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.FMM_Run_DM
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private ExtenderArgs args;
        private StringBuilder debugString;
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
        {
            try
            {
                // Retrieve the name/value pairs for Entity, Scenario, and Model_Grp_Seq
                var modelGroupSeqValue = args.NameValuePairs.XFGetValue("Model_Grp_Seq");
                var entityValue = args.NameValuePairs.XFGetValue("Entity", "NA");
                var scenarioValue = args.NameValuePairs.XFGetValue("Scenario", "NA");
                var accountValue = args.NameValuePairs.XFGetValue("Account", "NA");
                var flowValue = args.NameValuePairs.XFGetValue("Flow", "NA");

                BRApi.ErrorLog.LogMessage(si, "Hit: Entity: " + entityValue + " Scenario: " + scenarioValue + " Model_Grp_Seq: " + modelGroupSeqValue);

                // SQL query to fetch the required data based on the parameters passed
                var calc_Model_SQL = @"
		            SELECT Calc_Unit_Mod_Grp.Sequence Mod_Grp_Seq, Mod_Grps.Name Mod_Grp_Name, WF_DU.Entity_MFB Entity,
		                   WF_DU.WFChannel, Mod_Grp_Assgn.Sequence Mod_Seq, Modl.Name, Act.Calc_Type, Calc_Unit_Mod_Grp.Cube_ID, Modl.Model_ID, CubeCon.Cube CubeName
		            FROM FMM_Model_Grp_Seqs Mod_Grp_Seqs
		            JOIN FMM_Calc_Unit_Assign_Model_Group Calc_Unit_Mod_Grp
		            ON Mod_Grp_Seqs.Model_Grp_Seq_ID = Calc_Unit_Mod_Grp.Model_Grp_Seq_ID
		            AND Mod_Grp_Seqs.Cube_ID = Calc_Unit_Mod_Grp.Cube_ID
		            JOIN FMM_Model_Groups Mod_Grps
		            ON Calc_Unit_Mod_Grp.Cube_ID = Mod_Grps.Cube_ID
		            AND Calc_Unit_Mod_Grp.Model_Grp_ID = Mod_Grps.Model_Grp_ID
		            JOIN FMM_Calc_Unit_Config WF_DU
		            ON Calc_Unit_Mod_Grp.Cube_ID = WF_DU.Cube_ID
		            AND Calc_Unit_Mod_Grp.Calc_Unit_ID = WF_DU.Calc_Unit_ID
		            JOIN FMM_Model_Grp_Assign_Model Mod_Grp_Assgn
		            ON Mod_Grps.Cube_ID = Mod_Grp_Assgn.Cube_ID
		            AND Mod_Grps.Model_Grp_ID = Mod_Grp_Assgn.Model_Grp_ID
		            JOIN FMM_Models Modl
		            ON Modl.Cube_ID = Mod_Grp_Assgn.Cube_ID
		            AND Modl.Model_ID = Mod_Grp_Assgn.Model_ID
		            JOIN FMM_Activity_Config Act
		            ON Modl.Cube_ID = Act.Cube_ID
		            AND Modl.Activity_ID = Act.Activity_ID
					JOIN FMM_Cube_Config CubeCon
					ON Modl.Cube_ID = CubeCon.Cube_ID
		            WHERE Mod_Grp_Seqs.Name = @Mod_Grp_Seqs
		            AND Mod_Grp_Seqs.Status <> 'Archived'
		            AND Calc_Unit_Mod_Grp.Status <> 'Archived'
		            AND WF_DU.Status <> 'Archived'
					AND Mod_Grps.Status <> 'Archived'
				    AND Modl.Status <> 'Archived'
					AND Act.Status <> 'Archived'
					AND Mod_Grp_Assgn.Status <> 'Archived'";
                if (!string.IsNullOrEmpty(entityValue) && entityValue != "All")
                {
                    calc_Model_SQL += @" AND WF_DU.Entity_MFB = @entityValue
											 OR Act.Calc_Type = 'Consolidate'";
                }
                calc_Model_SQL += " ORDER BY Calc_Unit_Mod_Grp.Sequence, Mod_Grp_Assgn.Sequence";

                // Helper class
                var calcHelper = new DashboardExtender.FMM_Solution_Calc_Helper.MainClass();
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand(calc_Model_SQL, connection))
                    {
                        // Add parameters
                        command.Parameters.AddWithValue("@Mod_Grp_Seqs", modelGroupSeqValue);
                        if (!string.IsNullOrEmpty(entityValue) && entityValue != "All")
                        {
                            command.Parameters.AddWithValue("@entityValue", entityValue);
                        }

                        // Use SqlDataAdapter to fill DataTable
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        var primKeys = new DataColumn[] { dataTable.Columns["Model_ID"] };

                        dataTable.PrimaryKey = primKeys;

                        var ModelId_List = new List<int>();
                        var previousCalcType = null;

                        var ExecutionFlow = new Dictionary<int, Dictionary<string, List<int>>>();

                        var executionLevel = 0;

                        foreach (DataRow row in dataTable.Rows)
                        {
                            var rowCalcType = row["Calc_Type"].ToString();
                            var rowModelId = int.Parse(row["Model_ID"].ToString());

                            if (ExecutionFlow.ContainsKey(executionLevel))
                            {
                                var executionGroup = ExecutionFlow[executionLevel];
                                var executionCalcType = executionGroup.Keys.ElementAt(0);

                                if (executionCalcType == rowCalcType)
                                {
                                    // get current list of models and update dictionary with additional model ids
                                    var models = executionGroup[executionCalcType];
                                    models.Add(rowModelId);
                                    executionGroup[executionCalcType] = models;

                                }
                                else
                                {
                                    // create a new execution level since the new row's calc type does not match the current executionLevels calc type
                                    executionLevel += 1;
                                    var models = new List<int>() { rowModelId };
                                    var tempDict = new Dictionary<string, List<int>>();
                                    tempDict.Add(rowCalcType, models);
                                    ExecutionFlow.Add(executionLevel, tempDict);
                                }

                            }
                            else
                            { // if executionLevel is not contained within the dictionary, create a new one (this should only occur on first row)
                                var models = new List<int>() { rowModelId };
                                var tempDict = new Dictionary<string, List<int>>();
                                tempDict.Add(rowCalcType, models);
                                ExecutionFlow.Add(executionLevel, tempDict);
                            }

                        }

                        foreach (int execLvl in ExecutionFlow.Keys)
                        {
                            var calcType = ExecutionFlow[execLvl].Keys.ElementAt(0);
                            var models = ExecutionFlow[execLvl][calcType];

                            //BRApi.ErrorLog.LogMessage(si, "modelID list (converted) : " + string.Join(", ", models));



                            // TODO: Will each row have different cubeName, entity or scenario? Below is assuming that cube, entity, scenario, and time are constant across the models
                            // otherwise, might need to include several customSubstVars for each model being passed into the data management job


                            DataRow row = dataTable.Rows.Find(models[0]); // find the first model in the group so that we can get cube and entity information

                            if (calcType == "Table")
                            {
                                // Process table logic
                                calcHelper.Process_TableModel();
                            }
                            else if (calcType == "Cube")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Consultant Tools");

                                var customSubstVars_Dict = new Dictionary<string, string>
                                {
                                    { "FMM_Cube", row["CubeName"].ToString()},
                                    { "FMM_Entity", "E#[" + row["Entity"].ToString() +"]"},
                                    { "FMM_Consol", "C#[Aggregated]"},
                                    { "FMM_Scenario", scenarioValue},
                                    { "FMM_Time", "T#Pov"}, // TODO: Figure out time from associated scenario or custom table
									{ "FMM_Model_IDs" , string.Join(", ", models)}
                                };

                                //BRApi.ErrorLog.LogMessage(si, "cube name: " + row["CubeName"].ToString());

                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "Run_FMM_Custom_Cube_Calcs", customSubstVars_Dict);
                            }
                            else if (calcType == "Consolidate")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Build Toolkit");
                                var customSubstVars_Dict = new Dictionary<string, string>
                                {
                                    { "FMM_Cube", row["CubeName"].ToString()},
                                    { "FMM_Entity", "E#[" + row["Entity"].ToString() +"]"},
                                    { "FMM_Consol", "C#[Aggregated]"},  //Grab Consol approach from Cube Config
									{ "FMM_Scenario", "S#[" + scenarioValue + "]"},
                                    { "FMM_Time", "T#Pov"}, // TODO: Figure out time from associated scenario or custom table
									{ "FMM_Model_IDs" , string.Join(", ", models)}
                                };
                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "Run_FMM_Consolidation", customSubstVars_Dict);
                            }
                            else if (calcType == "BR_Table_to_Cube")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Build Toolkit");
                                var customSubstVars_Dict = new Dictionary<string, string>
                                {

                                };
                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "test", customSubstVars_Dict);
                            }
                            else if (calcType == "Import_Table_to_Cube")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Build Toolkit");
                                var customSubstVars_Dict = new Dictionary<string, string>
                                {

                                };
                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "test", customSubstVars_Dict);
                            }
                            else if (calcType == "Complete_WF")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Build Toolkit");
                                var customSubstVars_Dict = new Dictionary<string, string>
                                {

                                };
                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "test", customSubstVars_Dict);
                            }
                            else if (calcType == "Revert_WF")
                            {
                                var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, false, "OS Build Toolkit");
                                var customSubstVars_Dict = new Dictionary<string, string>
                                {

                                };
                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, workspace_ID, "test", customSubstVars_Dict);
                            }

                        }

                        //		                // Loop through the rows in the data table
                        //		                foreach (DataRow row in dataTable.Rows)
                        //		                {

                        //						 	if (calcType == "Table")
                        //                            {
                        //                                // Process table logic
                        //                                calcHelper.Process_TableModel();
                        //                            }
                        //                            else if (calcType == "Cube")
                        //                            {
                        //								var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");

                        //								var customSubstVars_Dict = new Dictionary<string, string>
                        //								{
                        //									{ "FMM_Cube", row["CubeName"].ToString()},
                        //									{ "FMM_Entity", "E#[" + row["Entity"].ToString() +"]"},
                        //									{ "FMM_Consol", "C#[Aggregated]"},
                        //									{ "FMM_Scenario", scenarioValue},
                        //									{ "FMM_Time", "T#Pov"} // TODO: Figure out time from associated scenario
                        //								};
                        //                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"Run_FMM_Custom_Cube_Calcs",customSubstVars_Dict);
                        //                            }
                        //                            else if (calcType == "Consolidate")
                        //                            {
                        //								var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");									
                        //								var customSubstVars_Dict = new Dictionary<string, string>
                        //								{
                        //									{ "FMM_Cube", "Cb#[" + row["CubeName"].ToString() + "]"},
                        //									{ "FMM_Entity", "E#[" + row["Entity"].ToString() +"]"},
                        //									{ "FMM_Consol", "C#[Aggregated]"},
                        //									{ "FMM_Scenario", "S#[" + scenarioValue + "]"},
                        //									{ "FMM_Time", ""}
                        //								};
                        //                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"Run_FMM_Consolidation",customSubstVars_Dict);
                        //                            }
                        //                            else if (calcType == "BR_Table_to_Cube")
                        //                            {
                        //								var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");									
                        //								var customSubstVars_Dict = new Dictionary<string, string>
                        //								{

                        //								};
                        //                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"test",customSubstVars_Dict);
                        //                            }



                        //		                    if (previousCalcType != null && calcType == previousCalcType)
                        //		                    {
                        //		                        // If the calc type matches the previous one, add the model ID to the list
                        //		                        ModelId_List.Add(modelId);
                        //		                    }
                        //		                    else if (previousCalcType != null && calcType != previousCalcType)
                        //		                    {
                        //	                            if (previousCalcType == "Table")
                        //	                            {
                        //	                                // Process table logic
                        //	                                calcHelper.Process_TableModel();
                        //	                            }
                        //	                            else if (previousCalcType == "Cube")
                        //	                            {
                        //									var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");

                        //									var customSubstVars_Dict = new Dictionary<string, string>
                        //									{

                        //									};
                        //	                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"Run_FMM_Custom_Cube_Calcs",customSubstVars_Dict);
                        //	                            }
                        //	                            else if (previousCalcType == "Consolidate")
                        //	                            {
                        //									var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");									
                        //									var customSubstVars_Dict = new Dictionary<string, string>
                        //									{
                        //										{ "FMM_Cube", ""},
                        //										{ "FMM_Entity", "E#[" + row["Entity"].ToString() +"]"},
                        //										{ "FMM_Consol", "C#[Aggregated]"},
                        //										{ "FMM_Scenario", "S#[" + scenarioValue + "]"},
                        //										{ "FMM_Time", ""}
                        //									};
                        //	                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"Run_FMM_Consolidation",customSubstVars_Dict);
                        //	                            }
                        //	                            else if (previousCalcType == "BR_Table_to_Cube")
                        //	                            {
                        //									var workspace_ID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,false, "OS Build Toolkit");									
                        //									var customSubstVars_Dict = new Dictionary<string, string>
                        //									{

                        //									};
                        //	                                var taskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si,workspace_ID,"test",customSubstVars_Dict);
                        //	                            }

                        //		                        // Start a new list for the new calc type
                        //		                        ModelId_List.Clear();
                        //		                        ModelId_List.Add(modelId);
                        //								previousCalcType = calcType;
                        //		                    }
                        //							else if (previousCalcType == null)
                        //							{
                        //		                        ModelId_List.Add(modelId);
                        //								previousCalcType = calcType;								
                        //							}
                        //}
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}