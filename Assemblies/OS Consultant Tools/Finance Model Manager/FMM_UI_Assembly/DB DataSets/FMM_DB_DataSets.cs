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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.FMM_DB_DataSets
{
    public class MainClass
    {
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
                        if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cube_Names"))
                        {
                            return get_FMM_Cube_Names();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Scen_Types"))
                        {
                            return get_FMM_Scen_Types();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Cubes"))
                        {
                            return get_FMM_Cubes("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cubes_With_Table_Act"))
                        {
                            return get_FMM_Cubes("With_Table_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_Cubes"))
                        {
                            return get_FMM_Cubes("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_Cubes"))
                        {
                            return get_FMM_Cubes("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_Acts"))
                        {
                            return get_FMM_Acts("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_Acts"))
                        {
                            return get_FMM_Acts("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Acts_By_Cube"))
                        {
                            return get_FMM_Acts("By_Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Acts_Table"))
                        {
                            return get_FMM_Acts("Table");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Approval_Acts_Config"))
                        {

                            return get_FMM_Approval_Acts();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Acts_Config"))
                        {
                            return get_FMM_Acts("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Models"))
                        {
                            return get_FMM_Models("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Models_by_Act"))
                        {
                            return get_FMM_Models("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_Models"))
                        {
                            return get_FMM_Models("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_Models"))
                        {
                            return get_FMM_Models("Target");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Calcs"))
                        {
                            return get_FMM_Calcs("By_Model");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_CalcList"))
                        {
                            return get_FMM_CalcList("By_Model");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Model_Grps"))
                        {
                            return get_FMM_Model_Grps("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Model_Grps"))
                        {
                            return get_FMM_Model_Grps("By_Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Model_Grp_Seqs_By_Cube"))
                        {
                            return get_FMM_Model_Grp_Seqs("By_Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_WFChannels"))
                        {
                            return get_FMM_WFChannels();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Calc_Units"))
                        {
                            return get_FMM_Calc_Units("By_Cube");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_Calc_Units"))
                        {
                            return get_FMM_Calc_Units("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Reg_Configs_By_Act"))
                        {
                            return get_FMM_Reg_Configs("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_Reg_Configs"))
                        {
                            return get_FMM_Reg_Configs("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_Reg_Configs"))
                        {
                            return get_FMM_Reg_Configs("Target");
                        }
                        //Return WF Profile Hierarchy for WF Root Profile selected
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_WFProfile_TreeView"))
                        {
                            return get_FMM_WFProfile_Hierarchy();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Appr_Config"))
                        {
                            return get_FMM_Apprs("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Security_Grps"))
                        {
                            return get_FMM_Security_Grps();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_WFProfiles"))
                        {
                            return get_FMM_WFProfiles("All");
                        }
                        // Validation Framework Datasets
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Validation_Config"))
                        {
                            return get_FMM_Validation_Config();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Validation_Contexts"))
                        {
                            return get_FMM_Validation_Contexts();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Table_Validation_Types"))
                        {
                            return get_FMM_Validation_Types("TABLE");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Cube_Validation_Types"))
                        {
                            return get_FMM_Validation_Types("CUBE");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Validation_Severities"))
                        {
                            return get_FMM_Validation_Severities();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Validation_Results"))
                        {
                            return get_FMM_Validation_Results();
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Validation_Runs"))
                        {
                            return get_FMM_Validation_Runs();
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
        private DataTable get_FMM_Cube_Names()
        {
            try
            {
                var dt = new DataTable("Cubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT CubeID, Name
                                FROM Cube
                                WHERE IsTopLevelCube = 1";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {};
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Scen_Types()
        {
            try
            {
                var Cube = args.NameValuePairs.XFGetValue("Cube", string.Empty);
                var dt = new DataTable("Scen_Types");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"WITH ScenType_List AS (
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
                                SELECT 'Target' AS ScenType
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
                                WHERE Cube.Scen_Type = Scen.ScenType
                                AND Cube.Cube = @Cube)";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube", SqlDbType.VarChar) {Value = Cube}
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Cubes(string cubeType)
        {
            try
            {
                var sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube,Cube.Cube_ID
                            FROM FMM_Cube_Config Cube";
                var dt = new DataTable("Cube_Config");
                if (cubeType.XFEqualsIgnoreCase("Source"))
                {
                    dt.TableName = "Src_Cube_Config";
                }
                else if (cubeType.XFEqualsIgnoreCase("Target"))
                {
                    dt.TableName = "Tgt_Cube_Config";
                }
                else if (cubeType.XFEqualsIgnoreCase("With_Table_Act"))
                {
                    dt.TableName = "Cubes_With_Table_Act";
                    // This query is used to get the Cube Config for Table Activities
                    sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube,Cube.Cube_ID
                            FROM FMM_Cube_Config Cube
                            JOIN FMM_Act_Config Act
                            ON Cube.Cube_ID = Act.Cube_ID
                            WHERE Act.Calc_Type = 'Table'";
                }
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Acts(string actType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var dt = new DataTable("Act_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    string sql = "";
                    SqlParameter[] sqlparams;

                    // Determine SQL and parameters based on actType
                    if (actType.XFEqualsIgnoreCase("Source"))
                    {
                        dt.TableName = "Src_Act_Config";
                        sql = @"SELECT CONCAT(Name, ' - ',Calc_Type) AS Act,Act_ID
                                FROM FMM_Cube_Config Con
                                JOIN FMM_Act_Config Act ON Con.Cube_ID = Act.Cube_ID
                                WHERE Con.Cube_ID = @Cube_ID
                                AND Act.Calc_Type = 'Source'
                                ORDER BY Cube,Scen_Type,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("Target"))
                    {
                        dt.TableName = "Tgt_Act_Config";
                        sql = @"SELECT CONCAT(Name, ' - ',Calc_Type) AS Act,Act_ID
                                FROM FMM_Cube_Config Con
                                JOIN FMM_Act_Config Act ON Con.Cube_ID = Act.Cube_ID
                                WHERE Con.Cube_ID = @Cube_ID
                                AND Act.Calc_Type = 'Target'
                                ORDER BY Cube,Scen_Type,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("By_Cube"))
                    {
                        dt.TableName = "Acts_By_Cube";
                        sql = @"SELECT CONCAT(Name, ' - ',Calc_Type) AS Act,Act_ID
                                FROM FMM_Cube_Config Con
                                JOIN FMM_Act_Config Act ON Con.Cube_ID = Act.Cube_ID
                                WHERE Con.Cube_ID = @Cube_ID
                                ORDER BY Cube,Scen_Type,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("Table"))
                    {
                        dt.TableName = "Table_Act_Config";
                        sql = @"SELECT CONCAT(Name, ' - ',Calc_Type) AS Act,Act_ID
                                FROM FMM_Cube_Config Con
                                JOIN FMM_Act_Config Act ON Con.Cube_ID = Act.Cube_ID
                                WHERE Con.Cube_ID = @Cube_ID
                                AND Act.Calc_Type = 'Table'
                                ORDER BY Cube,Scen_Type,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
                        };
                    }
                    else
                    {
                        // Default: return all activities for the cube
                        sql = @"SELECT CONCAT(Name, ' - ',Calc_Type) AS Act,Act_ID
                                FROM FMM_Cube_Config Con
                                JOIN FMM_Act_Config Act ON Con.Cube_ID = Act.Cube_ID
                                WHERE Con.Cube_ID = @Cube_ID
                                ORDER BY Cube,Scen_Type,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
                        };
                    }

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Approval_Acts()
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var appr_ID = args.NameValuePairs.XFGetValue("Appr_ID", "-1");
                var appr_Step_ID = args.NameValuePairs.XFGetValue("Appr_Step_ID", "-1");
                var addUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName", string.Empty);
                var runType = addUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "update" : "add";

                var dt = new DataTable("Approval_Act_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (runType == "add")
                { // add
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"Declare @CubeActivityResults Table (Activity nvarchar(100), Act_ID int)

                                    insert into @CubeActivityResults
                                    SELECT CONCAT(Name, ' - ',Calc_Type) AS Activity,Act_ID
                                    FROM FMM_Cube_Config Con
                                    JOIN FMM_Act_Config Act
                                    ON Con.Cube_ID = Act.Cube_ID
                                    WHERE Con.Cube_ID = @Cube_ID
                                    AND Act.Calc_Type IN ('Table','Cube')
                                    ORDER BY Cube,Scen_Type,Name

                                    Select CAR.Activity,CAR.Act_ID From @CubeActivityResults CAR
                                    FULL JOIN FMM_Approval_Step_Activity_Config ASAC
                                    ON CAR.Act_ID = ASAC.Act_ID
                                    Where ASAC.Approval_Step_Act_ID is null";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt()}
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                    }
                }
                else if (runType == "update")
                { // update
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        var sql = @"SELECT CONCAT(Con.Name, ' - ',Con.Calc_Type) AS Activity,Approval_Step_Act_ID AS Act_ID
                                    FROM FMM_Approval_Step_Activity_Config ASAC
                                    FULL JOIN FMM_Act_Config Con
                                    ON Con.Act_ID = ASAC.Act_ID
                                    WHERE ASAC.Appr_ID = @Appr_ID
                                    AND ASAC.Appr_Step_ID = @Appr_Step_ID
                                    ORDER BY Approval_Step_Act_ID";

                        //						Description,Approval_Step_Act_ID
                        //				       		FROM FMM_Approval_Step_Activity_Config
                        //							WHERE Appr_ID = @Appr_ID
                        //							AND Appr_Step_ID = @Appr_Step_ID
                        //							ORDER BY Approval_Step_Act_ID

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Appr_ID", SqlDbType.Int) { Value = appr_ID.XFConvertToInt()},
                            new SqlParameter("@Appr_Step_ID", SqlDbType.Int) { Value = appr_Step_ID.XFConvertToInt()}

                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                    }
                }


                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Models(string modelType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                var dt = new DataTable("Models");
                var sql = @"SELECT Name,Model_ID
                            FROM FMM_Models
                            ORDER BY Name";
                if (modelType.XFEqualsIgnoreCase("Source"))
                {
                    dt.TableName = "Src_Models";
                    sql = @"SELECT Name,Model_ID
                            FROM FMM_Models
                            WHERE Cube_ID = @Cube_ID
                            AND Act_ID = @Act_ID
                            ORDER BY Name";
                }
                else if (modelType.XFEqualsIgnoreCase("Target"))
                {
                    dt.TableName = "Tgt_Models";
                    sql = @"SELECT Name,Model_ID
                            FROM FMM_Models
                            WHERE Cube_ID = @Cube_ID
                            AND Act_ID = @Act_ID
                            ORDER BY Name";
                }
                else if (modelType.XFEqualsIgnoreCase("By_Act"))
                {
                    dt.TableName = "Models_By_Act";
                    sql = @"SELECT Name,Model_ID
                            FROM FMM_Models
                            WHERE Cube_ID = @Cube_ID
                            AND Act_ID = @Act_ID
                            ORDER BY Name";
                }
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Convert.ToInt16(act_ID) }
                 };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Calcs(string calcType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                var model_ID = args.NameValuePairs.XFGetValue("Model_ID", "-1");
                var dt = new DataTable("Calc_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name,Calc_ID
                                FROM FMM_Calc_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID
                                AND Model_ID = @Model_ID
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Convert.ToInt16(act_ID) },
                        new SqlParameter("@Model_ID", SqlDbType.Int) { Value = Convert.ToInt16(model_ID) }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        private DataTable get_FMM_CalcList(string calcType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                var model_ID = args.NameValuePairs.XFGetValue("Model_ID", "-1");
                var dt = new DataTable("FMM_CalcList");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT CONCAT(Sequence,' - ',Name) as Name,Calc_ID
                                FROM FMM_Calc_Config
                                WHERE Cube_ID = @Cube_ID
                                AND Act_ID = @Act_ID
                                AND Model_ID = @Model_ID
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) },
                        new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Convert.ToInt16(act_ID) },
                        new SqlParameter("@Model_ID", SqlDbType.Int) { Value = Convert.ToInt16(model_ID) }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Model_Grps(string modelGrpType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var dt = new DataTable("Model_Grps");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name, Model_Grp_ID
                                FROM FMM_Model_Grps
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Model_Grp_Seqs(string modelGrpSeqType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                var dt = new DataTable("Model_Grp_Seqs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name, Model_Grp_Seq_ID
                                FROM FMM_Model_Grp_Seqs
                                WHERE Cube_ID = @Cube_ID
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Reg_Configs(string registerType)
        {
            try
            {
            var dt = new DataTable("Register_Configs");
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            var sql = string.Empty;
            var sqlparams = new SqlParameter[] { };

            if (registerType.XFEqualsIgnoreCase("By_Act"))
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                dt.TableName = "Reg_Configs_By_Act";
                sql = @"SELECT Name as Name, Reg_Config_ID as Reg_Config_ID
                    FROM FMM_Reg_Config
                    WHERE Cube_ID = @Cube_ID
                    AND Act_ID = @Act_ID
                    ORDER BY Name";
                sqlparams = new SqlParameter[]
                {
                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) },
                new SqlParameter("@Act_ID", SqlDbType.Int) { Value = Convert.ToInt16(act_ID) }
                };
            }
            else if (registerType.XFEqualsIgnoreCase("Source"))
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                dt.TableName = "Reg_Configs_Source";
                sql = @"SELECT Name as Name, Reg_Config_ID as Reg_Config_ID
                    FROM FMM_Reg_Config
                    WHERE Cube_ID = @Cube_ID
                    ORDER BY Name";
                sqlparams = new SqlParameter[]
                {
                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) }
                };
            }
            else if (registerType.XFEqualsIgnoreCase("Target"))
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                dt.TableName = "Reg_Configs_Target";
                sql = @"SELECT Name as Name, Reg_Config_ID as Reg_Config_ID
                    FROM FMM_Reg_Config
                    WHERE Cube_ID = @Cube_ID
                    ORDER BY Name";
                sqlparams = new SqlParameter[]
                {
                new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) }
                };
            }
            else // Default: All
            {
                dt.TableName = "Reg_Configs";
                sql = @"SELECT Name as Name, Reg_Config_ID as Reg_Config_ID
                    FROM FMM_Reg_Config
                    ORDER BY Name";
                sqlparams = new SqlParameter[] { };
            }

            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                var sqa = new SqlDataAdapter();
                sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
            }

            return dt;
            }
            catch (Exception ex)
            {
            throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_WFChannels()
        {
            try
            {
                var dt = new DataTable("FMM_WF_Channels");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"
			        	SELECT Name as DisplayValue,Name as StoredValue
			       		FROM WorkflowChannel
						UNION
						SELECT 'Standard' as DisplayValue,'Standard' as StoredValue
						UNION
						SELECT 'NoDataLock' as DisplayValue,'NoDataLock' as StoredValue
						UNION
						SELECT 'AllChannelInput' as DisplayValue,'AllChannelInput' as StoredValue";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Calc_Units(string calcUnitType)
        {
            try
            {
                var dt = new DataTable("FMM_Calc_Unit_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = string.Empty;
                    SqlParameter[] sqlparams;

                    if (calcUnitType.XFEqualsIgnoreCase("By_Cube"))
                    {
                        var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                        dt.TableName = "Calc_Units_By_Cube";
                        sql = @"
                            SELECT CONCAT(Entity_MFB, ' - ',WFChannel) as Calc_Unit_Desc,Calc_Unit_ID
                            FROM FMM_Calc_Unit_Config
                            WHERE Cube_ID = @Cube_ID";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = Convert.ToInt16(cube_ID) }
                        };
                    }
                    else // "All" or default
                    {
                        dt.TableName = "FMM_Calc_Unit_Config";
                        sql = @"
                            SELECT CONCAT(Entity_MFB, ' - ',WFChannel) as Calc_Unit_Desc,Calc_Unit_ID
                            FROM FMM_Calc_Unit_Config";
                        sqlparams = new SqlParameter[] { };
                    }

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }


                return dt;
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

        private DataSet get_FMM_WFProfile_Hierarchy()
        {
            try
            {
                var hierarchy = new XFTreeItemCollection();
                var hierarchy_mbrs = new List<XFTreeItem>();
                var parent_child = new Dictionary<string, string>();
                var rootProfileName = "Army_RMW_Consol_CMD_PGM"; //args.CustomSubstVars.XFGetValue("BL_WFM_Root_WF_Profiles");

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
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@rootprofilename", SqlDbType.NVarChar,100) { Value = rootProfileName }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, select_sql, sqlparams);
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

        private DataTable get_FMM_Apprs(string apprType)
        {
            try
            {
                var cube_ID = args.NameValuePairs.XFGetValue("Cube_ID", "-1");
                var dt = new DataTable("Approval_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"
			        	SELECT * FROM FMM_Appr_Config
						WHERE Cube_ID = @Cube_ID";
                    //AND Act_ID = @Act_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@Cube_ID", SqlDbType.Int) { Value = cube_ID.XFConvertToInt() }
						//new SqlParameter("@Act_ID", SqlDbType.Int) { Value = ActID.XFConvertToInt() }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Reg_Config(string regConfigType)
        {

            try
            {
                var Reg_Config_DT = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    // get all the register configs that don't already have an entry for this activity
                    var sql = @"
			        	SELECT Name,RegCon.Reg_Config_ID FROM FMM_Reg_Config RegCon";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, Reg_Config_DT, sql, sqlparams);

                }

                return Reg_Config_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Reg_Profiles(string regProfilesType)
        {
            try
            {
                var act_ID = args.NameValuePairs.XFGetValue("Act_ID", "-1");
                //var runType = args.CustomSubstVars.XFGetValue("IV_FMM_Display_Add_Update_Approval_Step_Activity") == "0b3b2a3_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var AddUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName");
                var runType = AddUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                BRApi.ErrorLog.LogMessage(si, "get reg config actID: " + act_ID + " runType: " + runType);
                var dt = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (act_ID != "-1" && runType == "Add")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get all the register configs that don't already have an entry for this activity
                        var sql = @"
				        	SELECT Name,RegCon.Reg_Config_ID FROM FMM_Reg_Config RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Act_ID = ASAC.Act_ID AND RegCon.Reg_Config_ID = ASAC.Reg_Config_ID
							WHERE RegCon.Act_ID = @Act_ID AND ASAC.Reg_Config_ID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID.XFConvertToInt() },
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                    }
                }
                else if (act_ID != "-1" && runType == "Update")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get the current register config, and all of those that don't have associated entries (update)
                        var sql = @"
				        	Declare @initialResults Table (Name nvarchar(100), Reg_Config_ID int, Act_ID int)
							Declare @trueActID int;
							
							Insert into @initialResults 
							Select Name,RegCon.Reg_Config_ID,RegCon.Act_ID FROM FMM_Reg_Config RegCon
							Full JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Act_ID = ASAC.Act_ID AND RegCon.Reg_Config_ID = ASAC.Reg_Config_ID
							WHERE ASAC.Approval_Step_Act_ID = @Act_ID

							SELECT @trueActID = Act_ID from @initialResults;
							
							Select CONCAT('*',Name) As Name, Reg_Config_ID from @initialResults
							Union
							SELECT Name,RegCon.Reg_Config_ID FROM FMM_Reg_Config RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.Act_ID = ASAC.Act_ID AND RegCon.Reg_Config_ID = ASAC.Reg_Config_ID
							WHERE RegCon.Act_ID = @trueActID AND ASAC.Reg_Config_ID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@Act_ID", SqlDbType.Int) { Value = act_ID.XFConvertToInt() }, // in the update case, ActID is actually the approval_step_Act_ID. It's weird
	                    };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable get_FMM_Security_Grps()
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

        private DataTable get_FMM_WFProfiles(string wfProfileType)
        {
            try
            {
                var dt = new DataTable("WFProfiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"Select ProfileName,ProfileKey
										FROM WorkflowProfileHierarchy
										Where CubeName <> '-1'";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        #region "Validation Framework Helper Queries"
        
        /// <summary>
        /// Get all validation configurations
        /// </summary>
        private DataTable get_FMM_Validation_Config()
        {
            try
            {
                var dt = new DataTable("ValidationConfig");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    
                    var sql = @"
                        SELECT 
                            Validation_Config_ID,
                            Name,
                            Description,
                            Validation_Context,
                            Validation_Type,
                            Target_Object,
                            Process_Type,
                            Is_Active,
                            Severity,
                            Config_JSON,
                            Create_Date,
                            Create_User,
                            Update_Date,
                            Update_User
                        FROM FMM_Validation_Config
                        ORDER BY Validation_Context, Process_Type, Name";
                    
                    var sqlparams = new SqlParameter[] {};
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        /// <summary>
        /// Get validation contexts (TABLE, CUBE)
        /// </summary>
        private DataTable get_FMM_Validation_Contexts()
        {
            try
            {
                var dt = new DataTable("ValidationContexts");
                dt.Columns.Add("ContextName", typeof(string));
                dt.Columns.Add("ContextDescription", typeof(string));
                
                dt.Rows.Add("TABLE", "Table Validations - Column values, constraints, referential integrity");
                dt.Rows.Add("CUBE", "Cube Validations - Scenario comparisons, dimensional analysis");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        /// <summary>
        /// Get validation types by context
        /// </summary>
        private DataTable get_FMM_Validation_Types(string context)
        {
            try
            {
                var dt = new DataTable("ValidationTypes");
                dt.Columns.Add("TypeName", typeof(string));
                dt.Columns.Add("TypeDisplayName", typeof(string));
                dt.Columns.Add("TypeDescription", typeof(string));
                
                if (context == "TABLE")
                {
                    dt.Rows.Add("ColumnValueList", "Column Value List", "Check column values against a predefined list");
                    dt.Rows.Add("ColumnValueRange", "Column Value Range", "Check numeric column values within a range");
                    dt.Rows.Add("ColumnValuePattern", "Column Value Pattern", "Check column values match a regex pattern");
                    dt.Rows.Add("RequiredColumns", "Required Columns", "Ensure required columns are populated (not NULL/empty)");
                    dt.Rows.Add("UniqueConstraint", "Unique Constraint", "Check for duplicate values in specified columns");
                    dt.Rows.Add("ReferentialIntegrity", "Referential Integrity", "Validate foreign key relationships");
                    dt.Rows.Add("CustomTableSQL", "Custom Table SQL", "Execute custom SQL query against table data");
                }
                else if (context == "CUBE")
                {
                    dt.Rows.Add("ScenarioComparison", "Scenario Comparison", "Compare values between two scenarios");
                    dt.Rows.Add("DimensionalBalance", "Dimensional Balance", "Check balances across dimensional slices");
                    dt.Rows.Add("CrossDimensionalRule", "Cross-Dimensional Rule", "Validate business rules across multiple dimensions");
                    dt.Rows.Add("TemporalConsistency", "Temporal Consistency", "Check data consistency across time periods");
                    dt.Rows.Add("AllocationValidation", "Allocation Validation", "Validate allocation totals and distributions");
                    dt.Rows.Add("VarianceThreshold", "Variance Threshold", "Check variances against thresholds");
                    dt.Rows.Add("CustomCubeSQL", "Custom Cube SQL", "Execute custom SQL against cube view data");
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        /// <summary>
        /// Get validation severities
        /// </summary>
        private DataTable get_FMM_Validation_Severities()
        {
            try
            {
                var dt = new DataTable("ValidationSeverities");
                dt.Columns.Add("SeverityName", typeof(string));
                dt.Columns.Add("SeverityDescription", typeof(string));
                
                dt.Rows.Add("Error", "Critical validation failure - must be corrected");
                dt.Rows.Add("Warning", "Potential issue - should be reviewed");
                dt.Rows.Add("Info", "Informational - for awareness only");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        /// <summary>
        /// Get validation results with configuration details
        /// </summary>
        private DataTable get_FMM_Validation_Results()
        {
            try
            {
                var runID = args.NameValuePairs.XFGetValue("RunID", string.Empty);
                var validationConfigID = args.NameValuePairs.XFGetValue("ValidationConfigID", string.Empty);
                
                var dt = new DataTable("ValidationResults");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    
                    var sql = @"
                        SELECT 
                            vr.Validation_Result_ID,
                            vr.Run_ID,
                            vr.Run_Date,
                            vr.Run_User,
                            vr.Validation_Context,
                            vc.Name AS Validation_Name,
                            vc.Validation_Type,
                            vc.Target_Object,
                            vc.Process_Type,
                            vc.Severity,
                            vr.Table_Name,
                            vr.Primary_Key_Value,
                            vr.Column_Name,
                            vr.Column_Value,
                            vr.Cube_POV,
                            vr.Dimension_Values,
                            vr.Cell_Value,
                            vr.Comparison_Value,
                            vr.Validation_Status,
                            vr.Error_Message,
                            vr.Error_Details,
                            vr.Create_Date
                        FROM FMM_Validation_Result vr
                        INNER JOIN FMM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID
                        WHERE 1=1";
                    
                    var paramList = new List<SqlParameter>();
                    
                    if (!string.IsNullOrEmpty(runID))
                    {
                        sql += " AND vr.Run_ID = @RunID";
                        paramList.Add(new SqlParameter("@RunID", int.Parse(runID)));
                    }
                    
                    if (!string.IsNullOrEmpty(validationConfigID))
                    {
                        sql += " AND vr.Validation_Config_ID = @ValidationConfigID";
                        paramList.Add(new SqlParameter("@ValidationConfigID", int.Parse(validationConfigID)));
                    }
                    
                    sql += " ORDER BY vr.Validation_Status DESC, vr.Run_Date DESC";
                    
                    var sqlparams = paramList.ToArray();
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        /// <summary>
        /// Get validation run history
        /// </summary>
        private DataTable get_FMM_Validation_Runs()
        {
            try
            {
                var validationContext = args.NameValuePairs.XFGetValue("ValidationContext", string.Empty);
                var processType = args.NameValuePairs.XFGetValue("ProcessType", string.Empty);
                var topN = args.NameValuePairs.XFGetValue("TopN", "50");
                
                var dt = new DataTable("ValidationRuns");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    
                    var sql = $@"
                        SELECT TOP {topN}
                            Run_ID,
                            Run_Date,
                            Run_User,
                            Validation_Context,
                            Process_Type,
                            Target_Object,
                            Total_Validations,
                            Total_Records_Checked,
                            Total_Failures,
                            Total_Warnings,
                            Total_Info,
                            Execution_Time_Ms,
                            Status,
                            Error_Message,
                            Notes,
                            Create_Date
                        FROM FMM_Validation_Run
                        WHERE 1=1";
                    
                    var paramList = new List<SqlParameter>();
                    
                    if (!string.IsNullOrEmpty(validationContext))
                    {
                        sql += " AND Validation_Context = @ValidationContext";
                        paramList.Add(new SqlParameter("@ValidationContext", validationContext));
                    }
                    
                    if (!string.IsNullOrEmpty(processType))
                    {
                        sql += " AND Process_Type = @ProcessType";
                        paramList.Add(new SqlParameter("@ProcessType", processType));
                    }
                    
                    sql += " ORDER BY Run_Date DESC";
                    
                    var sqlparams = paramList.ToArray();
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        
        #endregion

    }
}