﻿using System;
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
using Workspace.OSConsultantTools.FMM_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.FMM_RegPlan_DB_DataSets
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        #endregion

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
                        // var names = new List<string>();
                        // names.Add("MyDataSet");
                        // return names;
                        break;
                    case DashboardDataSetFunctionType.GetDataSet:
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_Gov_PlanCycle_Activity_List"))
                        {
                            return Get_Register_Plan_Activity_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Approval_Register_Options"))
                        {
                            var curr_Approval_ID = Convert.ToInt32(args.CustomSubstVars.XFGetValue("IV_FMM_Approval_ID", "-1"));
                            var curr_Approval_Step_ID = Convert.ToInt32(args.CustomSubstVars.XFGetValue("IV_FMM_Approval_Step_ID", "-1"));


                            if (curr_Approval_ID == -1 || curr_Approval_Step_ID == -1)
                            {
                                //find approval and step id through matching step config
                                find_SetApprovalIDs(ref curr_Approval_ID, ref curr_Approval_Step_ID);
                            }


                            return Get_FMM_Approval_Register_Options(curr_Approval_ID, curr_Approval_Step_ID);
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

        public DataTable Get_FMM_Approval_Register_Options(int Approval_ID, int Approval_Step_ID)
        {
            DataTable dt = new DataTable("FMM_Register_Options");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_Adapter_FMM_Approval_Step_Activity_Config = new SQL_Adapter_FMM_Approval_Step_Activity_Config(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();

                // Fill DataTable for Approval_Step_Activity_Config
                string selectQuery = @"
                    SELECT Description, Register_Config_ID
                    FROM FMM_Approval_Step_Activity_Config
                    WHERE Approval_ID = @Approval_ID
					AND Approval_Step_ID = @Approval_Step_ID";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Approval_ID", SqlDbType.Int) { Value = Approval_ID },
                    new SqlParameter("@Approval_Step_ID", SqlDbType.Int) { Value = Approval_Step_ID },

                };

                sql_Adapter_FMM_Approval_Step_Activity_Config.Fill_FMM_Approval_Step_Activity_Config_DataTable(si, sqlDataAdapter, dt, selectQuery, parameters);

            }

            return dt;
        }

        #region "Helper Queries"
        #region "Register Plan Activity List Data Sets"
        private DataTable Get_Register_Plan_Activity_List()
        {
            try
            {
                var FMM_Register_Plan_Activity_List_DT = new DataTable("FMM_Register_Plan_Activity_List_DT");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    //						// Create a new DataTable
                    //						var sqlDataAdapter = new SqlDataAdapter();
                    //						// Define the select query and parameters
                    //		                string selectQuery = @"
                    //				        	SELECT CubeID, Name
                    //				       		FROM Cube
                    //				       		WHERE IsTopLevelCube = 1";
                    //			            // Create an array of SqlParameter objects
                    //	                    var parameters = new SqlParameter[]
                    //	                    {
                    //	                    };
                    //	                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter,FMM_Register_Plan_Activity_List_DT, selectQuery, parameters);

                }

                return FMM_Register_Plan_Activity_List_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        private void find_SetApprovalIDs(ref int Approval_ID, ref int Approval_Step_ID)
        {
            var wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si);
            var wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo();

            //				var wfScenario = wfUnitInfo.ScenarioName;
            //				var wfName = wfUnitInfo.WorkflowName;
            var wfProfile = wfUnitInfo.ProfileName;
            //				var other = wfUnitInfo.WfUnitPk;
            //				var cubeinfo = BRApi.Workflow.Metadata.GetCubeRootInfo(si, wfUnitInfo.WfUnitPk.ProfileKey, false);
            //				var cubeName = cubeinfo.CubeRootProfile.CubeName;


            var FMM_Approval_Step_Config_DT = new DataTable();

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_Adapter_FMM_Approval_Step_Config = new SQL_Adapter_FMM_Approval_Step_Config(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();

                // Fill DataTable for Approval_Step_Config
                // Possibly... need to have cube information to determine if we're within the right cube
                string selectQuery = @"
	                    SELECT Approval_ID, Approval_Step_ID, WFProfile_Step 
	                    FROM FMM_Approval_Step_Config
	                    WHERE WFProfile_Step = @Profile
						OR @Profile like WFProfile_Step";

                var parameters = new SqlParameter[]
                {
                        new SqlParameter("@Profile", SqlDbType.NVarChar, 100) { Value = wfProfile }
                };

                sql_Adapter_FMM_Approval_Step_Config.Fill_FMM_Approval_Step_Config_DataTable(si, sqlDataAdapter, FMM_Approval_Step_Config_DT, selectQuery, parameters);

                if (FMM_Approval_Step_Config_DT.Rows.Count > 0)
                {
                    //Find exact matches first
                    var exactMatches = FMM_Approval_Step_Config_DT.Select($"WFProfile_Step = '{wfProfile}'");

                    //if no matched rows were found, get the top row of the data table as a fuzzy match
                    if (exactMatches.Length != 0)
                    {
                        Approval_ID = Convert.ToInt32(exactMatches[0]["Approval_ID"]);
                        Approval_Step_ID = Convert.ToInt32(exactMatches[0]["Approval_Step_ID"]);
                    }
                    else
                    {
                        var likeMatch = FMM_Approval_Step_Config_DT.Rows[0];

                        Approval_ID = Convert.ToInt32(likeMatch["Approval_ID"]);
                        Approval_Step_ID = Convert.ToInt32(likeMatch["Approval_Step_ID"]);
                    }
                }
                else
                {
                    BRApi.ErrorLog.LogMessage(si, "No matching approval steps found");
                }

            }


            //				Approval_ID = 2;
            //				Approval_Step_ID = 0;
        }

        #endregion
    }
}