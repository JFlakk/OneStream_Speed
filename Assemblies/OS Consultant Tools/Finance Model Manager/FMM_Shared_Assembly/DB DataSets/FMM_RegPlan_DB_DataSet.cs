using System;
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
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Approval_Register_Options"))
                        {
                            var curr_ApprID = Convert.ToInt32(args.CustomSubstVars.XFGetValue("IV_FMM_ApprID", "-1"));
                            var curr_Appr_Step_ID = Convert.ToInt32(args.CustomSubstVars.XFGetValue("IV_FMM_Appr_Step_ID", "-1"));


                            if (curr_ApprID == -1 || curr_Appr_Step_ID == -1)
                            {
                                //find approval and step id through matching step config
                                find_SetApprovalIDs(ref curr_ApprID, ref curr_Appr_Step_ID);
                            }


                            return get_FMM_Approval_Register_Options(curr_ApprID, curr_Appr_Step_ID);
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

        public DataTable get_FMM_Approval_Register_Options(int ApprID, int Appr_Step_ID)
        {
            DataTable dt = new DataTable("FMM_Register_Options");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_Adapter_FMM_Approval_Step_Activity_Config = new SQL_Adapter_FMM_Approval_Step_Activity_Config(si, connection);
                var sqlDataAdapter = new SqlDataAdapter();

                // Fill DataTable for Approval_Step_Activity_Config
                string selectQuery = @"
                    SELECT Description, RegConfigID
                    FROM FMM_Approval_Step_Activity_Config
                    WHERE ApprID = @ApprID
					AND Appr_Step_ID = @Appr_Step_ID";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID },
                    new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = Appr_Step_ID },

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
                    //	                    sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sqlDataAdapter,FMM_Register_Plan_Activity_List_DT, selectQuery, parameters);

                }

                return FMM_Register_Plan_Activity_List_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        private void find_SetApprovalIDs(ref int ApprID, ref int Appr_Step_ID)
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
	                    SELECT ApprID, Appr_Step_ID, WFProfile_Step 
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
                        ApprID = Convert.ToInt32(exactMatches[0]["ApprID"]);
                        Appr_Step_ID = Convert.ToInt32(exactMatches[0]["Appr_Step_ID"]);
                    }
                    else
                    {
                        var likeMatch = FMM_Approval_Step_Config_DT.Rows[0];

                        ApprID = Convert.ToInt32(likeMatch["ApprID"]);
                        Appr_Step_ID = Convert.ToInt32(likeMatch["Appr_Step_ID"]);
                    }
                }
                else
                {
                }

            }


            //				ApprID = 2;
            //				Appr_Step_ID = 0;
        }

        #endregion
    }
}