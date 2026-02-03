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
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ScenTypes"))
                        {
                            return get_FMM_ScenTypes();
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
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_ModelGrps"))
                        {
                            return get_FMM_ModelGrps("All");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_ModelGrps"))
                        {
                            return get_FMM_ModelGrps("By_Cube");
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
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_RegConfigs_By_Act"))
                        {
                            return get_FMM_RegConfigs("By_Act");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Src_RegConfigs"))
                        {
                            return get_FMM_RegConfigs("Source");
                        }
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_Tgt_RegConfigs"))
                        {
                            return get_FMM_RegConfigs("Target");
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
                        else if (args.DataSetName.XFEqualsIgnoreCase("get_FMM_All_WFP"))
                        {
                            return get_FMM_WFProfiles("All");
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

        private DataTable get_FMM_ScenTypes()
        {
            try
            {
                var Cube = args.NameValuePairs.XFGetValue("Cube", string.Empty);
                var dt = new DataTable("ScenType");
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
                                FROM FMM_CubeConfig Cube
                                WHERE Cube.ScenType = Scen.ScenType
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
                var sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube,Cube.CubeID
                            FROM FMM_CubeConfig Cube";
                var dt = new DataTable("CubeConfig");
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
                    sql = @"SELECT DISTINCT CONCAT(Cube,' - ',Descr) AS Cube,Cube.CubeID
                            FROM FMM_CubeConfig Cube
                            JOIN FMM_ActConfig Act
                            ON Cube.CubeID = Act.CubeID
                            WHERE Act.CalcType = 1";
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                CubeID = "1";
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
                        sql = @"SELECT CONCAT(Name, ' - ',CalcType) AS Act,ActID
                                FROM FMM_CubeConfig Con
                                JOIN FMM_ActConfig Act ON Con.CubeID = Act.CubeID
                                WHERE Con.CubeID = @CubeID
                                ORDER BY Cube,ScenType,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("Target"))
                    {
                        dt.TableName = "Tgt_Act_Config";
                        sql = @"SELECT CONCAT(Name, ' - ',CalcType) AS Act,ActID
                                FROM FMM_CubeConfig Con
                                JOIN FMM_ActConfig Act ON Con.CubeID = Act.CubeID
                                WHERE Con.CubeID = @CubeID
                                ORDER BY Cube,ScenType,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("By_Cube"))
                    {
                        dt.TableName = "Acts_By_Cube";
                        sql = @"SELECT CONCAT(Name, ' - ',CalcType) AS Act,ActID
                                FROM FMM_CubeConfig Con
                                JOIN FMM_ActConfig Act ON Con.CubeID = Act.CubeID
                                WHERE Con.CubeID = @CubeID
                                ORDER BY Cube,ScenType,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
                        };
                    }
                    else if (actType.XFEqualsIgnoreCase("Table"))
                    {
                        dt.TableName = "Table_Act_Config";
                        sql = @"SELECT CONCAT(Name, ' - ',CalcType) AS Act,ActID
                                FROM FMM_CubeConfig Con
                                JOIN FMM_ActConfig Act ON Con.CubeID = Act.CubeID
                                WHERE Con.CubeID = @CubeID
                                AND Act.CalcType = 1
                                ORDER BY Cube,ScenType,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
                        };
                    }
                    else
                    {
                        // Default: return all activities for the cube
                        sql = @"SELECT CONCAT(Name, ' - ',CalcType) AS Act,ActID
                                FROM FMM_CubeConfig Con
                                JOIN FMM_ActConfig Act ON Con.CubeID = Act.CubeID
                                WHERE Con.CubeID = @CubeID
                                ORDER BY Cube,ScenType,Name";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                var ApprID = args.NameValuePairs.XFGetValue("ApprID", "-1");
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
                        var sql = @"Declare @CubeActivityResults Table (Activity nvarchar(100), ActID int)

                                    insert into @CubeActivityResults
                                    SELECT CONCAT(Name, ' - ',CalcType) AS Activity,ActID
                                    FROM FMM_CubeConfig Con
                                    JOIN FMM_ActConfig Act
                                    ON Con.CubeID = Act.CubeID
                                    WHERE Con.CubeID = @CubeID
                                    AND Act.CalcType IN (1,2)
                                    ORDER BY Cube,ScenType,Name

                                    Select CAR.Activity,CAR.ActID From @CubeActivityResults CAR
                                    FULL JOIN FMM_Approval_Step_Activity_Config ASAC
                                    ON CAR.ActID = ASAC.ActID
                                    Where ASAC.Approval_Step_ActID is null";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt()}
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
                        var sql = @"SELECT CONCAT(Con.Name, ' - ',Con.CalcType) AS Activity,Approval_Step_ActID AS ActID
                                    FROM FMM_Approval_Step_Activity_Config ASAC
                                    FULL JOIN FMM_ActConfig Con
                                    ON Con.ActID = ASAC.ActID
                                    WHERE ASAC.ApprID = @ApprID
                                    AND ASAC.Appr_Step_ID = @Appr_Step_ID
                                    ORDER BY Approval_Step_ActID";

                        //						Description,Approval_Step_ActID
                        //				       		FROM FMM_Approval_Step_Activity_Config
                        //							WHERE ApprID = @ApprID
                        //							AND Appr_Step_ID = @Appr_Step_ID
                        //							ORDER BY Approval_Step_ActID

                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ApprID", SqlDbType.Int) { Value = ApprID.XFConvertToInt()},
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                CubeID = "1";
                var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                var dt = new DataTable("Models");
                var sql = @"SELECT Name,ModelID
                            FROM FMM_Models
                            ORDER BY Name";
                if (modelType.XFEqualsIgnoreCase("Source"))
                {
                    dt.TableName = "Src_Models";
                    sql = @"SELECT Name,ModelID
                            FROM FMM_Models
                            WHERE CubeID = @CubeID
                            AND ActID = @ActID
                            ORDER BY Name";
                }
                else if (modelType.XFEqualsIgnoreCase("Target"))
                {
                    dt.TableName = "Tgt_Models";
                    sql = @"SELECT Name,ModelID
                            FROM FMM_Models
                            WHERE CubeID = @CubeID
                            AND ActID = @ActID
                            ORDER BY Name";
                }
                else if (modelType.XFEqualsIgnoreCase("By_Act"))
                {
                    dt.TableName = "Models_By_Act";
                    sql = @"SELECT Name,ModelID
                            FROM FMM_Models
                            WHERE CubeID = @CubeID
                            AND ActID = @ActID
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
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = Convert.ToInt16(ActID) }
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                var ModelID = args.NameValuePairs.XFGetValue("ModelID", "-1");
                CubeID = string.IsNullOrWhiteSpace(CubeID) ? "0" : CubeID;
                ActID = string.IsNullOrWhiteSpace(ActID) ? "0" : ActID;
                ModelID = string.IsNullOrWhiteSpace(ModelID) ? "0" : ModelID;
                var dt = new DataTable("Calc_List");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name,CalcID
                                FROM FMM_CalcConfig
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID
                                AND ModelID = @ModelID
                                ORDER BY Name";

                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = Convert.ToInt16(ActID) },
                        new SqlParameter("@ModelID", SqlDbType.Int) { Value = Convert.ToInt16(ModelID) }
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                var ModelID = args.NameValuePairs.XFGetValue("ModelID", "-1");
                var dt = new DataTable("FMM_CalcList");
                CubeID = string.IsNullOrWhiteSpace(CubeID) ? "0" : CubeID;
                ActID = string.IsNullOrWhiteSpace(ActID) ? "0" : ActID;
                ModelID = string.IsNullOrWhiteSpace(ModelID) ? "0" : ModelID;
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT CONCAT(Sequence,' - ',Name) as Name,CalcID
                                FROM FMM_CalcConfig
                                WHERE CubeID = @CubeID
                                AND ActID = @ActID
                                AND ModelID = @ModelID
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) },
                        new SqlParameter("@ActID", SqlDbType.Int) { Value = Convert.ToInt16(ActID) },
                        new SqlParameter("@ModelID", SqlDbType.Int) { Value = Convert.ToInt16(ModelID) }
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

        private DataTable get_FMM_ModelGrps(string modelGrpType)
        {
            try
            {
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                var dt = new DataTable("ModelGrps");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name, ModelGrpID
                                FROM FMM_ModelGrps
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                var dt = new DataTable("ModelGrpSeqs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT Name, ModelGrpSeqID
                                FROM FMM_ModelGrpSeqs
                                WHERE CubeID = @CubeID
                                ORDER BY Name";

                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) }
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

        private DataTable get_FMM_RegConfigs(string registerType)
        {
            try
            {
                var dt = new DataTable("Register_Configs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                var sql = string.Empty;
                var sqlparams = new SqlParameter[] { };

                if (registerType.XFEqualsIgnoreCase("By_Act"))
                {
                    var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                    var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                    dt.TableName = "RegConfigs_By_Act";
                    sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                    FROM FMM_RegConfig
                    WHERE CubeID = @CubeID
                    AND ActID = @ActID
                    ORDER BY Name";
                    sqlparams = new SqlParameter[]
                    {
                new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) },
                new SqlParameter("@ActID", SqlDbType.Int) { Value = Convert.ToInt16(ActID) }
                    };
                }
                else if (registerType.XFEqualsIgnoreCase("Source"))
                {
                    var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                    dt.TableName = "RegConfigs_Source";
                    sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                    FROM FMM_RegConfig
                    WHERE CubeID = @CubeID
                    ORDER BY Name";
                    sqlparams = new SqlParameter[]
                    {
                new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) }
                    };
                }
                else if (registerType.XFEqualsIgnoreCase("Target"))
                {
                    var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                    dt.TableName = "RegConfigs_Target";
                    sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                    FROM FMM_RegConfig
                    WHERE CubeID = @CubeID
                    ORDER BY Name";
                    sqlparams = new SqlParameter[]
                    {
                new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) }
                    };
                }
                else // Default: All
                {
                    dt.TableName = "RegConfigs";
                    sql = @"SELECT Name as Name, RegConfigID as RegConfigID
                    FROM FMM_RegConfig
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
                var dt = new DataTable("FMM_CalcUnitConfig");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = string.Empty;
                    SqlParameter[] sqlparams;

                    if (calcUnitType.XFEqualsIgnoreCase("By_Cube"))
                    {
                        var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
                        dt.TableName = "Calc_Units_By_Cube";
                        sql = @"
                            SELECT CONCAT(EntityMFB, ' - ',WFChannel) as Calc_Unit_Desc,CalcUnitID
                            FROM FMM_CalcUnitConfig
                            WHERE CubeID = @CubeID";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CubeID", SqlDbType.Int) { Value = Convert.ToInt16(CubeID) }
                        };
                    }
                    else // "All" or default
                    {
                        dt.TableName = "FMM_CalcUnitConfig";
                        sql = @"
                            SELECT CONCAT(EntityMFB, ' - ',WFChannel) as Calc_Unit_Desc,CalcUnitID
                            FROM FMM_CalcUnitConfig";
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
                var CubeID = args.NameValuePairs.XFGetValue("CubeID", "-1");
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
						WHERE CubeID = @CubeID";
                    //AND ActID = @ActID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@CubeID", SqlDbType.Int) { Value = CubeID.XFConvertToInt() }
						//new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID.XFConvertToInt() }
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

        private DataTable get_FMM_RegConfig(string regConfigType)
        {

            try
            {
                var RegConfig_DT = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    // get all the register configs that don't already have an entry for this activity
                    var sql = @"
			        	SELECT Name,RegCon.RegConfigID FROM FMM_RegConfig RegCon";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, RegConfig_DT, sql, sqlparams);

                }

                return RegConfig_DT;
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
                var ActID = args.NameValuePairs.XFGetValue("ActID", "-1");
                //var runType = args.CustomSubstVars.XFGetValue("IV_FMM_Display_Add_Update_Approval_Step_Activity") == "0b3b2a3_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var AddUpdateDBName = args.NameValuePairs.XFGetValue("AddUpdateDBName");
                var runType = AddUpdateDBName == "0b3b2a2_FMM_Approval_Steps_Activities_Row2b_Header" ? "Update" : "Add";
                var dt = new DataTable("Register_Profiles");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                if (ActID != "-1" && runType == "Add")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get all the register configs that don't already have an entry for this activity
                        var sql = @"
				        	SELECT Name,RegCon.RegConfigID FROM FMM_RegConfig RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.ActID = ASAC.ActID AND RegCon.RegConfigID = ASAC.RegConfigID
							WHERE RegCon.ActID = @ActID AND ASAC.RegConfigID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID.XFConvertToInt() },
                        };
                        sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                    }
                }
                else if (ActID != "-1" && runType == "Update")
                {
                    using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                    {
                        var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                        // Create a new DataTable
                        var sqa = new SqlDataAdapter();
                        // Define the select query and sqlparams
                        // get the current register config, and all of those that don't have associated entries (update)
                        var sql = @"
				        	Declare @initialResults Table (Name nvarchar(100), RegConfigID int, ActID int)
							Declare @trueActID int;
							
							Insert into @initialResults 
							Select Name,RegCon.RegConfigID,RegCon.ActID FROM FMM_RegConfig RegCon
							Full JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.ActID = ASAC.ActID AND RegCon.RegConfigID = ASAC.RegConfigID
							WHERE ASAC.Approval_Step_ActID = @ActID

							SELECT @trueActID = ActID from @initialResults;
							
							Select CONCAT('*',Name) As Name, RegConfigID from @initialResults
							Union
							SELECT Name,RegCon.RegConfigID FROM FMM_RegConfig RegCon
							FULL JOIN FMM_Approval_Step_Activity_Config ASAC
							ON RegCon.ActID = ASAC.ActID AND RegCon.RegConfigID = ASAC.RegConfigID
							WHERE RegCon.ActID = @trueActID AND ASAC.RegConfigID is NULL";
                        // Create an array of SqlParameter objects
                        var sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@ActID", SqlDbType.Int) { Value = ActID.XFConvertToInt() }, // in the update case, ActID is actually the approval_step_ActID. It's weird
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

        #endregion

    }
}