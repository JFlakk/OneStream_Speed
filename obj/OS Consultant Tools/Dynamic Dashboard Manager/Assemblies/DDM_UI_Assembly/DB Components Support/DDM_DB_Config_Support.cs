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
    public class DDM_DB_Config_Support
    {
        //Params
        public const string Param_CubeName = "IV_DDM_App_Cube_Name";
        public const string Param_DashboardMenuOption = "BL_DDM_App_Menu";
		


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

        public static string getCubeName(SessionInfo si, int cubeId)
        {
            string cubeName = "";
            try
            {
                var cubes_DT = new DataTable("Cubes");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = @"
			        	SELECT Name
			       		FROM Cube
			       		WHERE CubeId = @OS_Cube_ID";
                    // Create an array of SqlParameter objects
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@OS_Cube_ID", SqlDbType.Int) { Value = cubeId }
                    };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqlDataAdapter, cubes_DT, selectQuery, parameters);

                }
                if (cubes_DT.Rows.Count > 0)
                {
                    cubeName = cubes_DT.Rows[0]["Name"].ToString();
                }

                return cubeName;

            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static Dictionary<string, string> getParamsToAdd(DataTable headerItems)
        {
            Dictionary<string, string> paramVals = new Dictionary<string, string>();

            foreach (DataRow item in headerItems.Rows)
            {

                string baseSearch = "DDM_Menu_Hdr_";

                string optType = item[baseSearch + "Option_Type"].ToString();

                if (optType == "Filter")
                {
                    baseSearch += "Fltr";

                    foreach (string colSuffix in DDM_Header.dashboardTypeResolver.Keys)
                    {
                        if ((bool)item[baseSearch + "_" + colSuffix] && colSuffix != "Txt")
                        {
                            string dimType = item[baseSearch + "_Dim_Type"].ToString();

                            // Dimension
                            if (paramVals.ContainsKey($"IV_DDM_App_{dimType}_Dim_Name"))
                            {
                                paramVals[$"IV_DDM_App_{dimType}_Dim_Name"] = item[baseSearch + "_Dim_Name"].ToString();
                            }
                            else
                            {
                                paramVals.Add($"IV_DDM_App_{dimType}_Dim_Name", item[baseSearch + "_Dim_Name"].ToString());
                            }

                            // Member filter
                            if (paramVals.ContainsKey($"IV_DDM_App_{dimType}_MFB"))
                            {
                                paramVals[$"IV_DDM_App_{dimType}_MFB"] = item[baseSearch + "_MFB"].ToString();
                            }
                            else
                            {
                                paramVals.Add($"IV_DDM_App_{dimType}_MFB", item[baseSearch + "_MFB"].ToString());
                            }

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

        public static int getCurrentProfileID(SessionInfo si, Guid profileKey)
        {
            var configProfileDT = new DataTable("configProfileDT");

            int profileID = -1;

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                var sqlDataAdapter = new SqlDataAdapter();

                string selectQuery = @"
										Select DDM_Profile_ID
										From DDM_Config
										Where DDM_Profile_Name = @OS_ProfileKey";

                var parameters = new SqlParameter[] {
                    new SqlParameter("@OS_ProfileKey", SqlDbType.UniqueIdentifier) { Value = profileKey }
                };

                if (profileKey != null)
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqlDataAdapter, configProfileDT, selectQuery, parameters);
                }
            }

            if (configProfileDT.Rows.Count > 0)
            {
                profileID = Convert.ToInt32(configProfileDT.Rows[0]["DDM_Profile_ID"]);
            }

            return profileID;
        }

        public static DataTable getConfigMenuOptions(SessionInfo si, int profileID, int SelectedMenuOption)
        {

            var configMenuOptionsDT = new DataTable("configMenuOptionsDT");
            if (profileID != -1 && SelectedMenuOption != -1)
            {
                var dbConnApp1 = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp1.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);

                    var sqlDataAdapter = new SqlDataAdapter();

                    string selectQuery = @"
											Select *
											From DDM_Config_Menu
											Where DDM_Menu_ID = @Menu_Option
											And DDM_Profile_ID = @ProfileID";

                    var parameters = new SqlParameter[] {
                        new SqlParameter("@Menu_Option", SqlDbType.Int) { Value = SelectedMenuOption },
                        new SqlParameter("@ProfileID", SqlDbType.Int) { Value = profileID }
                    };

                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqlDataAdapter, configMenuOptionsDT, selectQuery, parameters);
                }
            }

            return configMenuOptionsDT;

        }

        public static int getSelectedMenuOption(SessionInfo si, Dictionary<string, string> customSubstVars)
        {
            int menuOption = -1;

            string menuOptionStr = customSubstVars.XFGetValue(Param_DashboardMenuOption, "1");


            if (!String.IsNullOrEmpty(menuOptionStr))
            {
                menuOption = Convert.ToInt32(menuOptionStr);
            }

            return menuOption;
        }

        public static DataTable getHeaderItems(SessionInfo si, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {

            var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
            var ProfileKey = wfUnitPk.ProfileKey;
            int configProfileID = DDM_DB_Config_Support.getCurrentProfileID(si, ProfileKey);

            string menu_option = customSubstVarsAlreadyResolved.XFGetValue(Param_DashboardMenuOption, "1");

            var Menu_Hdr_Options_DT = new DataTable("Menu_Hdr_Options");

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                // Create a new DataTable

                var sqlDataAdapter = new SqlDataAdapter();
                // Define the select query and parameters
                string selectQuery = @"
							        	Select *
										FROM DDM_Config_Menu_Hdr
										WHERE DDM_Profile_ID = @DDM_Profile_ID
										AND DDM_Menu_ID = @DDM_Menu_ID
										ORDER BY DDM_Menu_Hdr_Order";

                // Create an array of SqlParameter objects
                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@DDM_Profile_ID", SqlDbType.Int) { Value = configProfileID},
                    new SqlParameter("@DDM_Menu_ID", SqlDbType.Int) { Value = menu_option}
                };

                if (!String.IsNullOrEmpty(menu_option) && configProfileID != -1)
                {
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqlDataAdapter, Menu_Hdr_Options_DT, selectQuery, parameters);
                }
            }

            return Menu_Hdr_Options_DT;
        }

    }
}