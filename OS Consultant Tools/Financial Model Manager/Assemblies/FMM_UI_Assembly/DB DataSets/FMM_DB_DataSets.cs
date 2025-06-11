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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.FMM_DB_DataSets
{
    public class MainClass
    {
        private string IV_Cube_Name = "IV_FMM_Cube_ID";
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardDataSetArgs args;
        private string defaultStringVal = "0"; // first item in the list
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
                        if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Cube_List"))
                        {
                            return Get_FMM_Cube_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Scen_Type_List"))
                        {
                            var cube = args.NameValuePairs.XFGetValue("Cube");
                            return Get_FMM_Scen_Type_List(cube);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Setup_Cube_Config_List"))
                        {
                            return Get_FMM_Setup_Cube_Config_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Cube_Config_List"))
                        {
                            return Get_FMM_Cube_Config_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Src_Cube_Config_List"))
                        {
                            return Get_FMM_Src_Cube_Config_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Tgt_Cube_Config_List"))
                        {
                            return Get_FMM_Tgt_Cube_Config_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Table_Cube_Config_List"))
                        {
                            return Get_FMM_Table_Cube_Config_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Src_Activity_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            return Get_FMM_Src_Activity_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Tgt_Activity_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            return Get_FMM_Tgt_Activity_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Activity_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            //							var cubeID = args.CustomSubstVars.XFGetValue(IV_Cube_Name);
                            return Get_FMM_Activity_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Table_Activity_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            return Get_FMM_Table_Activity_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Approval_Activity_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            var approvID = args.NameValuePairs.XFGetValue("Approval_ID");
                            var approvStepID = args.NameValuePairs.XFGetValue("Approval_Step_ID");
                            var AddUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName");

                            var runType = AddUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "update" : "add";

                            return Get_FMM_Approval_Activity_List(cubeID, approvID, approvStepID, runType);

                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_All_Approval_Activity_List"))
                        {

                            return Get_FMM_All_Approval_Activity_List();

                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_All_Model_List"))
                        {
                            return Get_FMM_All_Model_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Model_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("cubeID");
                            var activityID = args.NameValuePairs.XFGetValue("activityID");

                            try
                            {
                                Convert.ToInt16(cubeID);
                                Convert.ToInt16(activityID);

                            }
                            catch
                            {

                                return new DataTable();
                            }

                            return Get_FMM_Model_List(cubeID, activityID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Src_Model_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            var activityID = args.NameValuePairs.XFGetValue("Activity_ID");
                            try
                            {
                                Convert.ToInt16(cubeID);
                                Convert.ToInt16(activityID);

                            }
                            catch
                            {

                                return new DataTable();
                            }
                            return Get_FMM_Src_Model_List(cubeID, activityID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Tgt_Model_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            var activityID = args.NameValuePairs.XFGetValue("Activity_ID");

                            try
                            {
                                Convert.ToInt16(cubeID);
                                Convert.ToInt16(activityID);

                            }
                            catch
                            {

                                return new DataTable();
                            }

                            return Get_FMM_Tgt_Model_List(cubeID, activityID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Calc_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            var activityID = args.NameValuePairs.XFGetValue("Activity_ID");
                            var modelID = args.NameValuePairs.XFGetValue("Model_ID");
                            return Get_FMM_Calc_List(cubeID, activityID, modelID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_All_Model_Group_List"))
                        {
                            return Get_FMM_All_Model_Group_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Model_Group_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            return Get_FMM_Model_Group_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Model_Group_Seq_List"))
                        {
                            var cubeID = args.NameValuePairs.XFGetValue("Cube_ID");
                            return Get_FMM_Model_Group_Seq_List(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_WFChannels"))
                        {
                            return Get_FMM_WFChannels();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_WF_DU"))
                        {
                            return Get_FMM_WF_DU();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_All_WF_DUs"))
                        {
                            return Get_FMM_All_WF_DUs();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Src_Register_List"))
                        {
                            return Get_FMM_Src_Register_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Tgt_Register_List"))
                        {
                            return Get_FMM_Tgt_Register_List();
                        }
                        //Return WF Profile Hierarchy for WF Root Profile selected
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_WFProfile_TreeView"))
                        {
                            return Get_FMM_WFProfile_Hierarchy();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Approval_Config"))
                        {

                            string cubeID = args.NameValuePairs.XFGetValue("cubeID");
                            //string activityID = args.NameValuePairs.XFGetValue("activityID");
                            return Get_FMM_Approval_Config(cubeID);
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Register_Config"))
                        {
                            //BRApi.ErrorLog.LogMessage(si,"Hit " + globals.GetStringValue("IV_FMM_Cube_ID"));
                            return Get_FMM_Register_Config();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Register_Profiles_by_Activity"))
                        {
                            //BRApi.ErrorLog.LogMessage(si,"Hit " + globals.GetStringValue("IV_FMM_Cube_ID"));
                            return Get_FMM_Register_Profiles_by_Activity();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_Security_Group_List"))
                        {
                            return Get_FMM_Security_Group_List();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_FMM_All_WFProfiles"))
                        {
                            return Get_FMM_All_WFProfiles();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("Get_Fdx_Cube_View"))
                        {
                            return Get_Fdx_Cube_View();
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

        #region "Helper Queries"
        #region "Cube List Data Sets"
        private DataTable Get_FMM_Cube_List()
        {
            try
            {
                var cubes_DT = new DataTable("Cubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CubeID, Name
			       		FROM Cube
			       		WHERE IsTopLevelCube = 1";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cubes_DT, selectQuery, parameters);

                }

                return cubes_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Scenario Type Data Sets

        private DataTable Get_FMM_Scen_Type_List(string Cube)
        {
            try
            {
                var scen_Types_DT = new DataTable("Scen_Types");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
										WITH ScenType_List AS (
										    SELECT 'Actual' AS ScenType
										    UNION ALL
										    SELECT 'Administration' AS ScenType
										    UNION ALL
										    SELECT 'Budget' AS ScenType
										    UNION ALL
										    SELECT 'Control' AS ScenType
										    UNION ALL
										    SELECT 'Flash' AS ScenType
										    UNION ALL
										    SELECT 'Forecast' AS ScenType
										    UNION ALL
										    SELECT 'FXModel' AS ScenType
										    UNION ALL
										    SELECT 'History' AS ScenType
										    UNION ALL
										    SELECT 'LongTerm' AS ScenType
										    UNION ALL
										    SELECT 'Model' AS ScenType
										    UNION ALL
										    SELECT 'Operational' AS ScenType
										    UNION ALL
										    SELECT 'Plan' AS ScenType
										    UNION ALL
										    SELECT 'Sustainability' AS ScenType
										    UNION ALL
										    SELECT 'Target' AS ScenType -- fixed typo from 'Taget' to 'Target'
										    UNION ALL
										    SELECT 'Tax' AS ScenType
										    UNION ALL
										    SELECT 'Variance' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType1' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType2' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType3' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType4' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType5' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType6' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType7' AS ScenType
										    UNION ALL
										    SELECT 'ScenarioType8' AS ScenType
										)
										SELECT ScenType
										FROM ScenType_List Scen
										WHERE NOT EXISTS (
										    SELECT 1
										    FROM FMM_Cube_Config Cube
										    WHERE Cube.Scen_Type = Scen.ScenType -- use alias 'Scen' instead of 'ScenType_List'
										    AND Cube.Cube = @Cube
										)";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.VarChar) { Value = Cube}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, scen_Types_DT, selectQuery, parameters);

                }

                return scen_Types_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Cube Config Data Sets"
        private DataTable Get_FMM_Setup_Cube_Config_List()
        {
            try
            {
                var cube_Config_DT = new DataTable("Cube_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT DISTINCT CONCAT(Cube,' - ',Cube_Description) AS OS_Cube,Cube.Cube_ID
						FROM FMM_Cube_Config Cube";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Config_DT, selectQuery, parameters);

                }

                return cube_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Cube_Config_List()
        {
            try
            {
                var cube_Config_DT = new DataTable("Cube_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT DISTINCT CONCAT(Cube,' - ',Cube_Description) AS OS_Cube,Cube.Cube_ID
						FROM FMM_Cube_Config Cube
						JOIN FMM_Activity_Config Act
						ON Cube.Cube_ID = Act.Cube_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Config_DT, selectQuery, parameters);

                }

                return cube_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Src_Cube_Config_List()
        {
            try
            {
                var cube_Config_DT = new DataTable("Cube_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT DISTINCT CONCAT(Cube,' - ',Cube_Description) AS OS_Cube,Cube.Cube_ID
						FROM FMM_Cube_Config Cube
						JOIN FMM_Activity_Config Act
						ON Cube.Cube_ID = Act.Cube_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Config_DT, selectQuery, parameters);

                }

                return cube_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Tgt_Cube_Config_List()
        {
            try
            {
                var cube_Config_DT = new DataTable("Cube_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT DISTINCT CONCAT(Cube,' - ',Cube_Description) AS OS_Cube,Cube.Cube_ID
						FROM FMM_Cube_Config Cube";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Config_DT, selectQuery, parameters);

                }

                return cube_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Table_Cube_Config_List()
        {
            try
            {
                var cube_Config_DT = new DataTable("Cube_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
										SELECT DISTINCT CONCAT(Cube, ' - ', Cube_Description) AS OS_Cube, Cube.Cube_ID
										FROM FMM_Cube_Config Cube
										JOIN FMM_Activity_Config Act 
										ON Cube.Cube_ID = Act.Cube_ID 
										AND Act.Calc_Type = 'Table'";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Config_DT, selectQuery, parameters);

                }

                return cube_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        #endregion

        #region "Activity List"

        private DataTable Get_FMM_Activity_List(string Cube_ID)
        {
            try
            {
                var cube_Model_Activities_DT = new DataTable("Activity_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
			       		FROM FMM_Cube_Config Con
						JOIN FMM_Activity_Config Act
						ON Con.Cube_ID = Act.Cube_ID
						WHERE Con.Cube_ID = @Cube_ID
						ORDER BY Cube,Scen_Type,Name";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID.XFConvertToInt()}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                }

                return cube_Model_Activities_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private DataTable Get_FMM_Table_Activity_List(string Cube_ID)
        {
            try
            {

                //BRApi.ErrorLog.LogMessage(si,"Hit Check Cube ID: " + Cube_ID);
                var cube_Model_Activities_DT = new DataTable("Table_Activity_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
			       		FROM FMM_Cube_Config Con
						JOIN FMM_Activity_Config Act
						ON Con.Cube_ID = Act.Cube_ID
						WHERE Con.Cube_ID = @Cube_ID
						AND Act.Calc_Type = 'Table'
						ORDER BY Cube,Scen_Type,Name";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID.XFConvertToInt()}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                }

                return cube_Model_Activities_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Src_Activity_List(string Cube_ID)
        {
            try
            {
                var cube_Model_Activities_DT = new DataTable("Src_Activity_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
			       		FROM FMM_Cube_Config Con
						JOIN FMM_Activity_Config Act
						ON Con.Cube_ID = Act.Cube_ID
						WHERE Con.Cube_ID = @Cube_ID
						ORDER BY Activity_ID,Calc_Type,Name";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID.XFConvertToInt()}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                }

                // set default selection for returned activity list to top of the list
                //setLBDefaultSelection("BL_FMM_Cube_Model_Activity_List", ref cube_Model_Activities_DT);

                //BRApi.ErrorLog.LogMessage(si, "Hit Get Datea: " + Cube_ID);

                return cube_Model_Activities_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Tgt_Activity_List(string Cube_ID)
        {
            try
            {
                var cube_Model_Activities_DT = new DataTable("Tgt_Activity_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
			       		FROM FMM_Cube_Config Con
						JOIN FMM_Activity_Config Act
						ON Con.Cube_ID = Act.Cube_ID
						WHERE Con.Cube_ID = @Cube_ID
						ORDER BY Activity_ID,Calc_Type,Name";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID.XFConvertToInt()}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                }

                // set default selection for returned activity list to top of the list
                //setLBDefaultSelection("BL_FMM_Cube_Model_Activity_List", ref cube_Model_Activities_DT);

                //BRApi.ErrorLog.LogMessage(si, "Hit Get Datea: " + Cube_ID);

                return cube_Model_Activities_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Approval_Activity_List(string Cube_ID, string Approval_ID, string Approval_Step_ID, string runType)
        {
            try
            {

                var cube_Model_Activities_DT = new DataTable("Approval_Activity_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (runType == "add")
                { // add
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        string selectQuery = @"
				        	Declare @CubeActivityResults Table (Activity nvarchar(100), Activity_ID int)

							insert into @CubeActivityResults
							SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
				       		FROM FMM_Cube_Config Con
							JOIN FMM_Activity_Config Act
							ON Con.Cube_ID = Act.Cube_ID
							WHERE Con.Cube_ID = @Cube_ID
							AND Act.Calc_Type IN ('Table','Cube')
							ORDER BY Cube,Scen_Type,Name

							Select CAR.Activity,CAR.Activity_ID From @CubeActivityResults CAR
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON CAR.Activity_ID = ASAC.Activity_ID
							Where ASAC.Approval_Step_Activity_ID is null";
                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Cube_ID.XFConvertToInt()}
                        };
                        sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                    }
                }
                else if (runType == "update")
                { // update
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        string selectQuery = @"
				        	SELECT CONCAT(Con.Name, ' - ',Con.Calc_Type) AS Activity,Approval_Step_Activity_ID AS Activity_ID
				       		FROM FMM_Approval_Step_Activity_Config ASAC
							FULL JOIN FMM_Activity_Config Con
							ON Con.Activity_ID = ASAC.Activity_ID
							WHERE ASAC.Approval_ID = @Approval_ID
							AND ASAC.Approval_Step_ID = @Approval_Step_ID
							ORDER BY Approval_Step_Activity_ID";

                        //						Description,Approval_Step_Activity_ID
                        //				       		FROM FMM_Approval_Step_Activity_Config
                        //							WHERE Approval_ID = @Approval_ID
                        //							AND Approval_Step_ID = @Approval_Step_ID
                        //							ORDER BY Approval_Step_Activity_ID

                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Approval_ID", SqlDbType.Int) { Value = Approval_ID.XFConvertToInt()},
                            new SqlParameter("@Approval_Step_ID", SqlDbType.Int) { Value = Approval_Step_ID.XFConvertToInt()}

                        };
                        sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);
                    }
                }


                return cube_Model_Activities_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_All_Approval_Activity_List()
        {
            try
            {

                var cube_Model_Activities_DT = new DataTable("Approval_Activity_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Activity_ID
			       		FROM FMM_Cube_Config Con
						JOIN FMM_Activity_Config Act
						ON Con.Cube_ID = Act.Cube_ID
						WHERE Act.Calc_Type IN ('Table','Cube')
						ORDER BY Cube,Scen_Type,Name";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, cube_Model_Activities_DT, selectQuery, parameters);

                    return cube_Model_Activities_DT;
                }
            }

            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }



        #endregion

        #region "Model Lists"

        private DataTable Get_FMM_All_Model_List()
        {
            try
            {

                var Model_List_By_Activity_DT = new DataTable("Model_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name,Model_ID
			       		FROM FMM_Models
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_List_By_Activity_DT, selectQuery, parameters);

                }

                return Model_List_By_Activity_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private DataTable Get_FMM_Model_List(string Cube_ID, string Activity_ID)
        {
            try
            {
                //BRApi.ErrorLog.LogMessage(si,"Hit: " + Cube_ID + "-" + Activity_ID);
                var Model_List_By_Activity_DT = new DataTable("Model_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name,Model_ID
			       		FROM FMM_Models
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Convert.ToInt16(Activity_ID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_List_By_Activity_DT, selectQuery, parameters);

                }

                return Model_List_By_Activity_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Src_Model_List(string Cube_ID, string Activity_ID)
        {
            try
            {
                //BRApi.ErrorLog.LogMessage(si,"Hit Src Model: " + Cube_ID + "-" + Activity_ID);
                var src_Model_List_DT = new DataTable("Src_Model_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name,Model_ID
			       		FROM FMM_Models
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Convert.ToInt16(Activity_ID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, src_Model_List_DT, selectQuery, parameters);

                }

                return src_Model_List_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Tgt_Model_List(string Cube_ID, string Activity_ID)
        {
            try
            {
                //BRApi.ErrorLog.LogMessage(si,"Hit Tgt Model: " + Cube_ID + "-" + Activity_ID);
                var tgt_Model_List_DT = new DataTable("Tgt_Model_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters

                    // Handle empty or null Activity_ID, default to 0 if blank
                    //int parsedActivityID = string.IsNullOrEmpty(Activity_ID) ? 0 : Convert.ToInt16(Activity_ID);

                    string selectQuery = @"
			        	SELECT Name,Model_ID
			       		FROM FMM_Models
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Convert.ToInt16(Activity_ID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, tgt_Model_List_DT, selectQuery, parameters);

                }

                return tgt_Model_List_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Calc_List(string Cube_ID, string Activity_ID, string Model_ID)
        {
            try
            {
                //BRApi.ErrorLog.LogMessage(si,"Hit: " + Cube_ID + "-" + Activity_ID);
                var Calc_List_DT = new DataTable("Calc_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name,OS_Calc_ID
			       		FROM FMM_Calc_Config
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						AND Model_ID = @Model_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Convert.ToInt16(Activity_ID) },
                        new SqlParameter("@Model_ID", SqlDbType.Int) { Value = Convert.ToInt16(Model_ID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Calc_List_DT, selectQuery, parameters);

                }

                return Calc_List_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Model_List_By_Cube()
        {
            try
            {
                var Cube_ID = globals.GetStringValue("IV_FMM_Cube_ID", "1");

                var Model_List_By_Cube_DT = new DataTable("Model_List_By_Cube");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name,Model_ID
			       		FROM FMM_Models
						WHERE Cube_ID = @Cube_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Model Group Lists"

        private DataTable Get_FMM_All_Model_Group_List()
        {
            try
            {
                var Model_Group_List_By_Cube_DT = new DataTable("Model_Group_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name, Model_Group_ID
			       		FROM FMM_Model_Groups
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_Group_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_Group_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        private DataTable Get_FMM_Model_Group_List(string cubeID)
        {
            try
            {
                var Cube_ID = globals.GetStringValue("IV_FMM_Cube_ID", "0");
                //BRApi.ErrorLog.LogMessage(si,"Hit Cube ID: " + cubeID);
                var Model_Group_List_By_Cube_DT = new DataTable("Model_Group_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name, Model_Group_ID
			       		FROM FMM_Model_Groups
						WHERE Cube_ID = @Cube_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cubeID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_Group_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_Group_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "Model Group Sequences"
        private DataTable Get_FMM_Model_Group_Seq_List(string cubeID)
        {
            try
            {
                //BRApi.ErrorLog.LogMessage(si,"hit Model Group Seq: " + cubeID);
                var Model_Group_Seq_List_By_Cube_DT = new DataTable("Model_Group_Seq_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name, Model_Group_Seq_ID
			       		FROM FMM_Model_Group_Seqs
						WHERE Cube_ID = @Cube_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cubeID) }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_Group_Seq_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_Group_Seq_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Register List"

        private DataTable Get_FMM_Src_Register_List()
        {
            try
            {
                var Cube_ID = globals.GetStringValue("IV_FMM_Cube_ID", "0");
                var Activity_ID = globals.GetStringValue("IV_FMM_Activity_ID", "0");
                //BRApi.ErrorLog.LogMessage(si,"Hit Cube ID: " + Cube_ID);
                var Model_Group_List_By_Cube_DT = new DataTable("Register_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name as Name,Register_Config_ID as Reg_Config_ID
			       		FROM FMM_Register_Config
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Activity_ID}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_Group_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_Group_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Tgt_Register_List()
        {
            try
            {
                var Cube_ID = globals.GetStringValue("IV_FMM_Cube_ID", "0");
                var Activity_ID = globals.GetStringValue("IV_FMM_Activity_ID", "0");
                //BRApi.ErrorLog.LogMessage(si,"Hit Cube ID: " + Cube_ID);
                var Model_Group_List_By_Cube_DT = new DataTable("Register_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name as Name,Register_Config_ID as Reg_Config_ID
			       		FROM FMM_Register_Config
						WHERE Cube_ID = @Cube_ID
						AND Activity_ID = @Activity_ID
						ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(Cube_ID) },
                        new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = Activity_ID}
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, Model_Group_List_By_Cube_DT, selectQuery, parameters);

                }

                return Model_Group_List_By_Cube_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion


        #region "WF Channels"

        private DataTable Get_FMM_WFChannels()
        {
            try
            {
                var FMM_WF_Channels_DT = new DataTable("FMM_WF_Channels");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name as DisplayValue,Name as StoredValue
			       		FROM WorkflowChannel
						UNION
						SELECT 'Standard' as DisplayValue,'Standard' as StoredValue
						UNION
						SELECT 'NoDataLock' as DisplayValue,'NoDataLock' as StoredValue
						UNION
						SELECT 'AllChannelInput' as DisplayValue,'AllChannelInput' as StoredValue";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, FMM_WF_Channels_DT, selectQuery, parameters);

                }

                return FMM_WF_Channels_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        #region "WF Data Units"

        private DataTable Get_FMM_WF_DU()
        {
            try
            {
                var FMM_WF_DU_DT = new DataTable("FMM_WF_DU_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Entity_MFB, ' - ',WFChannel) as DisplayValue,WF_DU_ID as StoredValue
			       		FROM FMM_WF_DU_Config";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, FMM_WF_DU_DT, selectQuery, parameters);

                }

                return FMM_WF_DU_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion

        private DataTable Get_FMM_All_WF_DUs()
        {
            try
            {
                var FMM_WF_DU_DT = new DataTable("FMM_All_WF_DUs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT CONCAT(Entity_MFB, ' - ',WFChannel) as WF_DU_Desc,WF_DU_ID
			       		FROM FMM_WF_DU_Config";

                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, FMM_WF_DU_DT, selectQuery, parameters);

                }

                return FMM_WF_DU_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Fdx_Cube_View()
        {
            try
            {

                var objDashboardWorkspace = BRApi.Dashboards.Workspaces.GetWorkspace(si, false, "Gov_PlanCycle");
                var dt = new DataTable();
                var nvbParams = new NameValueFormatBuilder();
                nvbParams.NameValuePairs.XFSetValue("DL_PGM_Personnel_Type", "Dir");
                dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, objDashboardWorkspace.WorkspaceID, "01_Civ_Con_Pay_Rates_by_PE", "DHP_Consol_Entities_Dim", "E#HQ_ManpowerFactors", "Main_Scenario_Dim", "S#POM2630_v0", "", nvbParams, false, false, false, string.Empty, 8, true);
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataSet Get_FMM_WFProfile_Hierarchy()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                var rootProfileName = "DHA_Consol_PGM"; //args.CustomSubstVars.XFGetValue("BL_WFM_Root_WF_Profiles");

                var dt = new DataTable();

                // Define the SQL Statement
                var select_sql = @"
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

                // Return the DataTable
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    var sqlDataAdapter = new SqlDataAdapter();

                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, dt, select_sql, parameters);
                }

                foreach (DataRow row in dt.Rows)
                {
                    var hierarchy_children_mbrs = new List<XFTreeItem>();
                    string profileName = row["ProfileName"].ToString();
                    string profileKey = row["ProfileKey"].ToString();
                    string parentprofileName = row["ParentProfileName"].ToString();
                    string parentprofileKey = row["ParentProfileKey"].ToString();
                    parent_child.Add(profileName, parentprofileName);
                    var childProfiles = parent_child.Where(pair => pair.Value == profileName)
                                        .Select(pair => pair.Key)
                                        .ToList();
                    if (childProfiles.Count > 0)
                    {
                        foreach (var childProfile in childProfiles)
                        {

                            // Do something with each childProfile
                            // For example, create an XFTreeItem for each child profile
                            var childXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == childProfile);

                            hierarchy_children_mbrs.Add(childXFTreeItem);
                        }
                        var wfprofile_xftreeitem = new XFTreeItem(profileName, profileName, string.Empty, false, true, false, false, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.FalseValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                    else
                    {
                        var wfprofile_xftreeitem = new XFTreeItem(profileName, profileName, string.Empty, false, true, false, false, XFImageFileSourceType.Unknown, string.Empty, string.Empty, hierarchy_children_mbrs, TriStateBool.FalseValue);
                        hierarchy_mbrs.Add(wfprofile_xftreeitem);
                    }
                }

                var hierXFTreeItem = hierarchy_mbrs.Find(item => item.HeaderText == rootProfileName);
                hierarchy.TreeItems.Add(hierXFTreeItem);
                return hierarchy.CreateDataSet(si);
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Approval_Config(string cubeID)
        {

            //			BRApi.ErrorLog.LogMessage(si, "cube: " + cubeID + " activity: " + activityID);

            try
            {
                var approval_config_DT = new DataTable("Approval_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT * FROM FMM_Approval_Config
						WHERE Cube_ID = @Cube_ID";
                    //AND Activity_ID = @Activity_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cubeID.XFConvertToInt() }
						//new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = activityID.XFConvertToInt() }
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, approval_config_DT, selectQuery, parameters);

                }

                return approval_config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Register_Config()
        {

            try
            {
                var register_config_DT = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    // get all the register configs that don't already have an entry for this activity
                    string selectQuery = @"
			        	SELECT Name,RegCon.Register_Config_ID FROM FMM_Register_Config RegCon";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, register_config_DT, selectQuery, parameters);

                }

                return register_config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Register_Profiles_by_Activity()
        {

            try
            {
                var activityID = args.NameValuePairs.XFGetValue("Activity_ID", "0");
                //var runType = args.CustomSubstVars.XFGetValue("IV_FMM_Display_Add_Update_Approval_Step_Activity") == "0b3b2a3_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var AddUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName");
                var runType = AddUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                BRApi.ErrorLog.LogMessage(si, "get reg config actID: " + activityID + " runType: " + runType);
                var register_config_DT = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (activityID != "0" && runType == "Add")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        // get all the register configs that don't already have an entry for this activity
                        string selectQuery = @"
				        	SELECT Name,RegCon.Register_Config_ID FROM FMM_Register_Config RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Activity_ID = ASAC.Activity_ID AND RegCon.Register_Config_ID = ASAC.Register_Config_ID
							WHERE RegCon.Activity_ID = @Activity_ID AND ASAC.Register_Config_ID is NULL";
                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = activityID.XFConvertToInt() },
                        };
                        sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, register_config_DT, selectQuery, parameters);

                    }
                }
                else if (activityID != "0" && runType == "Update")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqlDataAdapter = new SqlDataAdapter();
                        // Define the select query and parameters
                        // get the current register config, and all of those that don't have associated entries (update)
                        string selectQuery = @"
				        	Declare @initialResults Table (Name nvarchar(100), Register_Config_ID int, Activity_ID int)
							Declare @trueActivityID int;
							
							Insert into @initialResults 
							Select Name,RegCon.Register_Config_ID,RegCon.Activity_ID FROM FMM_Register_Config RegCon
							Full JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Activity_ID = ASAC.Activity_ID AND RegCon.Register_Config_ID = ASAC.Register_Config_ID
							WHERE ASAC.Approval_Step_Activity_ID = @Activity_ID

							SELECT @trueActivityID = Activity_ID from @initialResults;
							
							Select CONCAT('*',Name) As Name, Register_Config_ID from @initialResults
							Union
							SELECT Name,RegCon.Register_Config_ID FROM FMM_Register_Config RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Activity_ID = ASAC.Activity_ID AND RegCon.Register_Config_ID = ASAC.Register_Config_ID
							WHERE RegCon.Activity_ID = @trueActivityID AND ASAC.Register_Config_ID is NULL";
                        // Create an array of SqlParameter objects
                        var parameters = new SqlParameter[]
                        {
                            new SqlParameter("@Activity_ID", SqlDbType.Int) { Value = activityID.XFConvertToInt() }, // in the update case, activityID is actually the approval_step_activity_ID. It's weird
	                    };
                        sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, register_config_DT, selectQuery, parameters);

                    }
                }


                return register_config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_Security_Group_List()
        {

            try
            {
                var security_Group_DT = new DataTable("Security_Groups");
                // Define the columns for the DataTable
                security_Group_DT.Columns.Add("GroupName", typeof(string)); // Column for group name
                security_Group_DT.Columns.Add("Description", typeof(string)); // Column for group description

                // Call the API to get the list of security groups
                var objList = BRApi.Security.Admin.GetGroups(si);

                // Loop through the groups and add them to the DataTable
                foreach (var group in objList)
                {
                    // Create a new DataRow
                    DataRow newRow = security_Group_DT.NewRow();

                    // Assuming the group object has properties `Name` and `Description`
                    newRow["GroupName"] = group.Name; // Set the group name
                    newRow["Description"] = group.Description ?? string.Empty; // Set the description or an empty string if null

                    // Add the row to the DataTable
                    security_Group_DT.Rows.Add(newRow);
                }

                return security_Group_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_FMM_All_WFProfiles()
        {

            try
            {
                var WFProfile_DT = new DataTable("WFProfiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_FMM_Get_DataSets = new SQL_FMM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
										Select ProfileName,ProfileKey
										FROM WorkflowProfileHierarchy
										Where CubeName <> '-1'";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                    };
                    sql_FMM_Get_DataSets.Fill_Get_FMM_DataTable(si, sqlDataAdapter, WFProfile_DT, selectQuery, parameters);

                }

                return WFProfile_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion


        #region "Helper Functions"

        private void setLBDefaultSelection(string boundListKey, ref DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                string itemID = dt.Rows[0].ItemArray[1].ToString(); // get ID of first item in the list
                globals.SetStringValue(boundListKey, itemID);
            }
            return;
        }

        #endregion

    }
}