﻿﻿using System;
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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Support
    {
        //Params
        public const string Param_CubeName = "IV_DDM_App_Cube_Name";
        public const string Param_DashboardMenu = "BL_DDM_App_Menu";
		


        public object Test(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static string get_CubeName(SessionInfo si, int cubeId)
        {
            var cubeName = string.Empty;
            try
            {
                var dt = new DataTable("Cubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and parameters
                    var sql = @"SELECT Name
					       		FROM Cube
					       		WHERE CubeId = @OS_Cube_ID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
                        new SqlParameter("@OS_Cube_ID", SqlDbType.Int) { Value = cubeId }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);

                }
                if (dt.Rows.Count > 0)
                {
                    cubeName = dt.Rows[0]["Name"].ToString();
                }

                return cubeName;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static Dictionary<string, string> get_ParamsToAdd(DataTable headerItems)
        {
            var paramVals = new Dictionary<string, string>();

            foreach (DataRow item in headerItems.Rows)
            {

                var baseSearch = string.Empty;

                var optType = item["Option_Type"].ToString();

                if (optType == "Filter")
                {
                    baseSearch += "Fltr";

                    foreach (string colSuffix in DDM_Header.dashboardTypeResolver.Keys)
                    {
                        if ((bool)item[baseSearch + "_" + colSuffix] && colSuffix != "Txt")
                        {
                            string dimType = item[baseSearch + "_Dim_Type"].ToString();

                            // set the ML value here directly
                            if (paramVals.ContainsKey($"ML_DDM_App_{dimType}_Selection"))
                            {
                                paramVals[$"ML_DDM_App_{dimType}_Selection"] = item[baseSearch + "_Default"].ToString();
                            }
                            else
                            {
                                paramVals.Add($"ML_DDM_App_{dimType}_Selection", item[baseSearch + "_Default"].ToString());
                            }


                            //set default ML, this will default the ML to this everytime it is loaded
                            //paramVals.Add($"IV_DDM_App_{dimType}_ML_Default", item[baseSearch + "_Default"].ToString()); // default val for ML



                        }
                    }
                }
            }

            return paramVals;
        }

        public static int get_CurrProfileID(SessionInfo si, Guid profileKey)
        {
            var dt = new DataTable("configProfileDT");

            var profileID = -1;

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqa = new SqlDataAdapter();

                var sql = @"Select DDM_Profile_ID
                            From DDM_Config
                            Where ProfileKey = @OS_ProfileKey";

                var sqlparams = new SqlParameter[] {
                    new SqlParameter("@OS_ProfileKey", SqlDbType.UniqueIdentifier) { Value = profileKey }
                };

                if (profileKey != null)
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }

            if (dt.Rows.Count > 0)
            {
                profileID = Convert.ToInt32(dt.Rows[0]["DDM_Profile_ID"]);
            }

            return profileID;
        }

        public static DataTable get_ConfigMenu(SessionInfo si, int SelectedMenu)
        {

            var dt = new DataTable("configMenu_DT");
            if (SelectedMenu != -1)
            {
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                    var sqa = new SqlDataAdapter();

                    var sql = @"Select *
                                From DDM_Config_Menu
                                Where DDM_Menu_ID = @Menu_Option"; 

                    var sqlparams = new SqlParameter[] {
                        new SqlParameter("@Menu_Option", SqlDbType.Int) { Value = SelectedMenu }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }

            return dt;

        }

        public static int get_SelectedMenu(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            var menuOption = -1;

            var menuOptionStr = customSubstVars.XFGetValue(Param_DashboardMenu, "1");


            if (!String.IsNullOrEmpty(menuOptionStr))
            {
                menuOption = Convert.ToInt32(menuOptionStr);
            }

            return menuOption;
        }

        public static DataTable get_HeaderItems(SessionInfo si, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var menu_option = customSubstVarsAlreadyResolved.XFGetValue(Param_DashboardMenu, "1");

            var dt = new DataTable("Menu_Hdr_Options");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqa = new SqlDataAdapter();
                // Define the select query and parameters
                var sql = @"Select *
                            FROM DDM_Config_Menu_Hdr
                            WHERE DDM_Menu_ID = @DDM_Menu_ID
                            ORDER BY Sort_Order";

                // Create an array of SqlParameter objects
                var sqlparams = new SqlParameter[]
                {
                    new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = menu_option}
                };

                if (!String.IsNullOrEmpty(menu_option))
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
            }

            return dt;
        }

    }
}