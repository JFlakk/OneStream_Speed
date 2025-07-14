﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.FMM_RegPlan_XFTV
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private SpreadsheetArgs args;

        string nameColName = "Name";
        string invalidColName = "Invalid";
        string aliasColName = "Alias";
        string reg_TimePhasing = "Derived";

        string registerSelectParam = "BL_FMM_Register_Select";

        string filterIV = "IV_TVFilter";
        string searchIV = "IV_TVSearch";

        string profileDictKey = "profile";
        string scenarioDictKey = "scenario";
        string timeDictKey = "time";

        int registerColsInUse = 0;

        int g_modelID = -1;
        int g_activityID = -1;

        private Dictionary<string, string> filterParams = new Dictionary<string, string>();
        private List<YearTimePhasingConfig> yearConfigs = null;

        List<string> timephasing_years = new List<string>(); // Add actual years
        Dictionary<string, (string scenarioFrequency, List<int> Periods)> timephasing_timePeriods = new Dictionary<string, (string scenarioFrequency, List<int> Periods)>(); // Store periods & Frequency by year
        public Dictionary<string, string> columnParamDictionary { get; set; } = new Dictionary<string, string>();

        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, SpreadsheetArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                var watch = System.Diagnostics.Stopwatch.StartNew();

                //unsavedRows = JsonSerializer.Deserialize<object>(globals.GetObject(unsavedRowsGlobalsKey));

                switch (args.FunctionType)
                {
                    case SpreadsheetFunctionType.GetCustomSubstVarsInUse:

                        // Call function to get Custom SubstVars
                        var customSubstVarsInUse = new List<string>();
                        customSubstVarsInUse = TableViewGet_RegPlan_CustomSubstVarsInUse(args.TableViewName);

                        //timing end
                        watch.Stop();
                        BRApi.ErrorLog.LogMessage(si, args.FunctionType.ToString() + " time elapsed: " + watch.ElapsedMilliseconds);

                        return customSubstVarsInUse;

                    case SpreadsheetFunctionType.GetTableView:
                        var tableView = new TableView();
                        if (args.CustSubstVarsAlreadyResolved != null)
                        {
                            var json = JsonSerializer.Serialize(args.CustSubstVarsAlreadyResolved);
                            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                            BRApi.State.SetSessionState(si, false, ClientModuleType.Unknown, typeof(Dictionary<string, string>).Name, string.Empty, "SubstVars", si.UserName, string.Empty, bytes);
                        }
                        else
                        {
                            BRApi.State.SetSessionState(si, false, ClientModuleType.Unknown, typeof(Dictionary<string, string>).Name, string.Empty, "SubstVars", si.UserName, string.Empty, null);
                        }
                        tableView = Get_registerTV(args.CustSubstVarsAlreadyResolved, args.TableViewName);

                        //timing end
                        watch.Stop();
                        BRApi.ErrorLog.LogMessage(si, args.FunctionType.ToString() + " time elapsed: " + watch.ElapsedMilliseconds);

                        return tableView;

                    case SpreadsheetFunctionType.SaveTableView:
                        BRApi.ErrorLog.LogMessage(si, "Save TableView");
                        var uState = BRApi.State.GetSessionState(si, false, ClientModuleType.Unknown, typeof(Dictionary<string, string>).Name, string.Empty, "SubstVars", si.UserName);

                        if (uState != null)
                        {
                            var jsonString = System.Text.Encoding.UTF8.GetString(uState.BinaryValue);
                            args.CustSubstVarsAlreadyResolved = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                        }

                        var retVal = Save_registerTV(args.CustSubstVarsAlreadyResolved, args.TableView);

                        //timing end
                        //                        watch.Stop();
                        //                        BRApi.ErrorLog.LogMessage(si, args.FunctionType.ToString() + " time elapsed: " + watch.ElapsedMilliseconds);

                        return retVal;

                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private List<string> TableViewGet_RegPlan_CustomSubstVarsInUse(string tableViewName)
        {
            try
            {
                var wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si);
                var wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo();
                var substVarsList = new List<string>();

                return substVarsList;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region "Get TableView"
        private TableView Get_registerTV(Dictionary<string, string> substVars, string tvName)
        {
            string registerConfigID = substVars.XFGetValue(FMM_Register_Plan_Helpers.filterIV);

            // Fetch configurations and data
            var registerConfigDt = getRegisterConfig(registerConfigID);
            var columnsDt = getRegisterColumnProfile(registerConfigID, substVars);
            var regDt = getRegisterTable(columnsDt, registerConfigDt, substVars);

            var configProvider = new ColumnConfigProvider();

            // Initialize TableView
            var regTV = new TableView
            {
                HeaderFormat = { IsBold = true },
                CanModifyData = true,
                NumberOfEmptyRowsToAdd = 50
            };

            // Add header row
            var headerRow = CreateHeaderRow(regDt, columnsDt);
            regTV.Rows.Add(headerRow);

            // Populate TableView with data
            regTV.PopulateFromDataTable(regDt, false, true);

            // Configure columns
            ConfigureColumns(regTV, columnsDt, configProvider);

            return regTV;
        }

        #endregion

        #region Save Data
        private bool Save_registerTV(Dictionary<string, string> substVars, TableView tableView)
        {
            //			BRApi.ErrorLog.LogMessage(si,"Hit Reg");
            var wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si);
            var wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo();

            //			string approvLevelID = substVars.XFGetValue(FMM_Register_Plan_Helpers.searchIV);
            string registerConfigID = substVars.XFGetValue(FMM_Register_Plan_Helpers.filterIV);

            var uState = BRApi.State.GetSessionState(si, false, ClientModuleType.Unknown, typeof(Dictionary<string, string>).Name, string.Empty, "yearConfigs", si.UserName);

            if (uState != null)
            {
                var jsonString = System.Text.Encoding.UTF8.GetString(uState.BinaryValue);
                yearConfigs = JsonSerializer.Deserialize<List<YearTimePhasingConfig>>(jsonString);
            }

            var registerConfig_dt = getRegisterConfig(registerConfigID);

            string startTime = "";
            string endTime = "";
            if (registerConfig_dt.Rows.Count > 0)
            {
                startTime = registerConfig_dt.Rows[0]["Start_Dt_Src"].ToString();
                endTime = registerConfig_dt.Rows[0]["End_Dt_Src"].ToString();
                g_activityID = Convert.ToInt32(registerConfig_dt.Rows[0]["Act_ID"]);
                reg_TimePhasing = registerConfig_dt.Rows[0]["Time_Phasing"].ToString();
            }

            var profileName = wfUnitInfo.ProfileName;
            var scenarioName = wfUnitInfo.ScenarioName;
            var timeName = wfUnitInfo.TimeName;
            var modelID = g_modelID != -1 ? g_modelID : Convert.ToInt32(substVars.XFGetValue("Model_ID", "-1"));
            var activityID = g_activityID != -1 ? g_activityID : Convert.ToInt32(substVars.XFGetValue("Act_ID", "-1"));

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            var connection = new SqlConnection(dbConnApp.ConnectionString);

            var requiredColumns = new List<DataColumn>();

            fillReqColumnsList(ref requiredColumns, registerConfigID, substVars);

            #region "Using connection"			
            using (connection)
            {
                connection.Open();
                #region "Setup SQL Adapter"				
                var regPlanAdapter = new SqlDataAdapter();
                var regPlanHelpers = new SQL_Adapter_Register_Plan(si, connection);
                var regPlan_Sql = @"
				    SELECT *
				    FROM Register_Plan
				    WHERE WF_Scenario_Name = @WFScenarioName
				    AND WF_Profile_Name = @WFProfileName
				    AND WF_Time_Name = @WFTimeName
				    AND Act_ID = @ActivityID"; // Added condition for Act_ID

                var regPlan_parameters = new SqlParameter[]
                {
                    new SqlParameter("@WFScenarioName", SqlDbType.VarChar) { Value = scenarioName },
                    new SqlParameter("@WFProfileName", SqlDbType.VarChar) { Value = profileName },
                    new SqlParameter("@WFTimeName", SqlDbType.VarChar) { Value = timeName },
                    new SqlParameter("@ActivityID", SqlDbType.Int) { Value = activityID } // Added parameter for Act_ID
				};

                var sqlParam_Index = 4;

                foreach (var kvp in filterParams)
                {
                    string paramName = $"@{kvp.Key}"; // Use the key as the parameter name
                    regPlan_Sql += $" AND {kvp.Key} = {paramName}"; // Append to the SQL query
                    regPlan_parameters[sqlParam_Index].ParameterName = paramName;
                    regPlan_parameters[sqlParam_Index].Value = kvp.Value;
                    sqlParam_Index += 1;
                }
                var Register_Plan_DT = new DataTable();

                regPlanHelpers.Fill_Register_Plan_DataTable(si, regPlanAdapter, Register_Plan_DT, regPlan_Sql, regPlan_parameters);

                var regPlanDetailsAdapter = new SqlDataAdapter();
                var regPlanDetailsHelpers = new SQL_Adapter_Register_Plan_Details(si, connection);
                var regPlanDetails_Sql = @"
								SELECT *
								FROM Register_Plan_Details
								WHERE WF_Scenario_Name = @WFScenarioName
								AND WF_Profile_Name = @WFProfileName
								AND WF_Time_Name = @WFTimeName";
                var regPlanDetails_parameters = new SqlParameter[]
                {
                    new SqlParameter("@WFScenarioName", SqlDbType.VarChar) { Value = scenarioName },
                    new SqlParameter("@WFProfileName", SqlDbType.VarChar) { Value = profileName },
                    new SqlParameter("@WFTimeName", SqlDbType.VarChar) { Value = timeName },

                };
                var Register_Plan_Details_DT = new DataTable();

                regPlanDetailsHelpers.Fill_Register_Plan_Details_DataTable(si, regPlanDetailsAdapter, Register_Plan_Details_DT, regPlanDetails_Sql, regPlanDetails_parameters);
                #endregion
                foreach (var tvr in tableView.Rows)
                {
                    if (tvr.IsHeader)
                    {
                        continue;
                    }

                    //int numRequiredColsFilled = numReqColsFilled(requiredColumns.ToArray(), tvr);


                    var selectedRegisterPlanUpdateRows = Register_Plan_DT.Select($"Register_Plan_ID = '{tvr["Register_Plan_ID"].Value.ToString()}'");

                    if (selectedRegisterPlanUpdateRows.Length > 0)
                    {
                        //register_Plan_ID should be unique per table view row, meaning we should only ever need to get the first row returned
                        DataRow existing_Register_Plan_Row = selectedRegisterPlanUpdateRows.First();
                        existing_Register_Plan_Row["Entity"] = tvr.Items["Entity"].Value.ToString() ?? string.Empty;
                        existing_Register_Plan_Row["Invalid"] = isRowInvalid(requiredColumns.ToArray(), tvr);
                        for (int i = 1; i <= 20; i++)
                        {
                            checkAndSetColumn(ref existing_Register_Plan_Row, tvr, $"Attribute_{i}");
                        }

                        for (int i = 1; i <= 12; i++)
                        {
                            checkAndSetColumn(ref existing_Register_Plan_Row, tvr, $"Attribute_Value_{i}");
                        }

                        for (int i = 1; i <= 5; i++)
                        {
                            checkAndSetColumn(ref existing_Register_Plan_Row, tvr, $"Date_Value_{i}");
                        }

                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Register_ID");
                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Register_ID_1");
                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Register_ID_2");

                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Spread_Amount");
                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Spread_Curve");
                        checkAndSetColumn(ref existing_Register_Plan_Row, tvr, "Status");

                        existing_Register_Plan_Row["Update_Date"] = DateTime.Now;
                        existing_Register_Plan_Row["Update_User"] = si.UserName;
                    }
                    else
                    {
                        //						BRApi.ErrorLog.LogMessage(si,"Hit Reg Else");
                        var new_Register_Plan_Row = Register_Plan_DT.NewRow();
                        new_Register_Plan_Row["Register_Plan_ID"] = Guid.NewGuid();
                        new_Register_Plan_Row["WF_Scenario_Name"] = scenarioName;
                        new_Register_Plan_Row["WF_Profile_Name"] = profileName;
                        new_Register_Plan_Row["WF_Time_Name"] = timeName;
                        new_Register_Plan_Row["Act_ID"] = activityID;

                        bool invalid = isRowInvalid(requiredColumns.ToArray(), tvr);
                        new_Register_Plan_Row["Invalid"] = invalid;

                        bool saveRow = shouldSaveRow(requiredColumns.ToArray(), tvr);
                        if (!saveRow)
                        {
                            continue;
                        }

                        //						new_Register_Plan_Row["Approval_Level_ID"] = approvLevelID;
                        //new_Register_Plan_Row["Approval_Level_ID"] = tvr.Items["Approval_Level_ID"].Value.ToString() != string.Empty ? tvr.Items["Approval_Level_ID"].Value.ToString() : approvLevelID;
                        // Check if "Approval_Level_ID" key exists in filterParams
                        if (filterParams.ContainsKey("Approval_Level_ID"))
                        {
                            // Set the value from the dictionary
                            new_Register_Plan_Row["Approval_Level_ID"] = filterParams["Approval_Level_ID"];
                        }
                        else
                        {
                            // Set the value based on tvr.Items
                            new_Register_Plan_Row["Approval_Level_ID"] =
                                !string.IsNullOrEmpty(tvr.Items["Approval_Level_ID"].Value.ToString())
                                ? tvr.Items["Approval_Level_ID"].Value.ToString()
                                : string.Empty; // Optionally parse to GUID if necessary
                        }
                        if (filterParams.ContainsKey("Entity"))
                        {
                            // Set the value from the dictionary
                            new_Register_Plan_Row["Entity"] = filterParams["Entity"];
                        }
                        else
                        {
                            // Set the value based on tvr.Items
                            new_Register_Plan_Row["Entity"] =
                                !string.IsNullOrEmpty(tvr.Items["Entity"].Value.ToString())
                                ? tvr.Items["Entity"].Value.ToString()
                                : string.Empty;
                        }

                        for (int i = 1; i <= 20; i++)
                        {
                            checkAndSetColumn(ref new_Register_Plan_Row, tvr, $"Attribute_{i}");
                        }

                        for (int i = 1; i <= 12; i++)
                        {
                            checkAndSetColumn(ref new_Register_Plan_Row, tvr, $"Attribute_Value_{i}");
                        }

                        for (int i = 1; i <= 5; i++)
                        {
                            checkAndSetColumn(ref new_Register_Plan_Row, tvr, $"Date_Value_{i}");
                        }


                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Register_ID");
                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Register_ID_1");
                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Register_ID_2");

                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Spread_Amount");
                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Spread_Curve");
                        checkAndSetColumn(ref new_Register_Plan_Row, tvr, "Status");

                        new_Register_Plan_Row["Create_Date"] = DateTime.Now;
                        new_Register_Plan_Row["Create_User"] = si.UserName;
                        new_Register_Plan_Row["Update_Date"] = DateTime.Now;
                        new_Register_Plan_Row["Update_User"] = si.UserName;

                        //Add row to DataTable
                        Register_Plan_DT.Rows.Add(new_Register_Plan_Row);
                    }

                    #region "Time-Phase Columns"
                    if (reg_TimePhasing == "Input")
                    {
                        var planUnits = string.Empty;

                        //Timephasing information should be prepopulated before save call, see getTimePhasingColumns

                        // Iterate over years
                        foreach (var yearConfig in yearConfigs)
                        {
                            BRApi.ErrorLog.LogMessage(si, $"Hit Reg 4 {yearConfig}");
                            var selectedRegisterPlanDetailsUpdateRows = Register_Plan_Details_DT.Select($"Register_Plan_ID = '{tvr["Register_Plan_ID"].Value.ToString()}' AND " +
                            $"Plan_Units = '{planUnits}' AND Year = '{yearConfig}'");

                            if (selectedRegisterPlanDetailsUpdateRows.Length > 0)
                            #region "existing Reg_Plan_Detail"
                            {
                                foreach (var existing_Register_Plan_Details_Row in selectedRegisterPlanDetailsUpdateRows)
                                {
                                    // Update entity
                                    existing_Register_Plan_Details_Row["Entity"] = tvr.Items["Entity"]?.Value.ToString() ?? string.Empty;
                                    existing_Register_Plan_Details_Row["Plan_Units"] = planUnits; //planUnits;
                                                                                                  //									existing_Register_Plan_Details_Row["Account"] = tvr.Items["Account"]?.ToString() ?? string.Empty; //planUnits;
                                                                                                  //		                            existing_Register_Plan_Details_Row["Flow"] = tvr.Items["Flow"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD1"] = tvr.Items["UD1"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD2"] = tvr.Items["UD2"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD3"] = tvr.Items["UD3"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD4"] = tvr.Items["UD4"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD5"] = tvr.Items["UD5"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD6"] = tvr.Items["UD6"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD7"] = tvr.Items["UD7"]?.Value.ToString() ?? string.Empty;
                                                                                                  //									existing_Register_Plan_Details_Row["UD8"] = tvr.Items["UD8"]?.Value.ToString() ?? string.Empty;
                                    existing_Register_Plan_Details_Row["Year"] = yearConfig;
                                    //get scenario frequency
                                    //									var (scenarioFrequency, periods) = timephasing_timePeriods[yearConfig];

                                    // Populate time periods
                                    var yearlyTotal = 0.0m;
                                    var Quarter1 = 0.0m;
                                    var Quarter2 = 0.0m;
                                    var Quarter3 = 0.0m;
                                    var Quarter4 = 0.0m;
                                    #region "existing Monthly"	
                                    if (yearConfig.Frequency == "Monthly")
                                    {
                                        for (int month = 1; month <= 12; month++)
                                        {
                                            var monthValue = 0.0m;
                                            if (yearConfig.Periods.Contains(month.XFToString()))
                                            {
                                                // If the period exists in the list, fetch the value from `tvr.Items`
                                                monthValue = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig}_M{month}"].Value) ? "0" : tvr.Items[$"{yearConfig}_M{month}"].Value);
                                                existing_Register_Plan_Details_Row[$"Month{month}"] = monthValue;
                                            }
                                            else
                                            {
                                                // If period doesn't exist, attempt to retrieve from the row
                                                monthValue = existing_Register_Plan_Details_Row[$"Month{month}"] != DBNull.Value ? Convert.ToDecimal(existing_Register_Plan_Details_Row[$"Month{month}"]) : 0;
                                            }
                                            yearlyTotal += monthValue;

                                            if (month <= 3) Quarter1 += monthValue;
                                            else if (month <= 6) Quarter2 += monthValue;
                                            else if (month <= 9) Quarter3 += monthValue;
                                            else Quarter4 += monthValue;

                                        }
                                        existing_Register_Plan_Details_Row["Quarter1"] = Quarter1;
                                        existing_Register_Plan_Details_Row["Quarter2"] = Quarter2;
                                        existing_Register_Plan_Details_Row["Quarter3"] = Quarter3;
                                        existing_Register_Plan_Details_Row["Quarter4"] = Quarter4;
                                        existing_Register_Plan_Details_Row["Yearly"] = yearlyTotal;
                                    }
                                    #endregion
                                    #region "existing Quarterly"	
                                    else if (yearConfig.Frequency == "Quarterly")
                                    {
                                        for (int quarter = 1; quarter <= 4; quarter++)
                                        {
                                            var quarterValue = 0.0m;
                                            if (yearConfig.Periods.Contains(quarter.XFToString()))
                                            {
                                                quarterValue = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig}_Q{quarter}"].Value) ? "0" : tvr.Items[$"{yearConfig}_Q{quarter}"].Value);
                                                existing_Register_Plan_Details_Row[$"Quarter{quarter}"] = quarterValue;
                                            }
                                            else
                                            {
                                                quarterValue = existing_Register_Plan_Details_Row[$"Quarter{quarter}"] != DBNull.Value ? Convert.ToDecimal(existing_Register_Plan_Details_Row[$"Quarter{quarter}"]) : 0;
                                            }
                                            yearlyTotal += quarterValue;
                                        }
                                        existing_Register_Plan_Details_Row["Yearly"] = yearlyTotal;
                                    }
                                    #endregion
                                    #region "existing Yearly"
                                    else if (yearConfig.Frequency == "Yearly")
                                    {
                                        existing_Register_Plan_Details_Row["Yearly"] = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig.Year}"].Value) ? "0" : tvr.Items[$"{yearConfig.Year}"].Value);
                                    }
                                    #endregion

                                    // Update metadata
                                    existing_Register_Plan_Details_Row["Update_Date"] = DateTime.Now;
                                    existing_Register_Plan_Details_Row["Update_User"] = Environment.UserName;
                                }
                            }
                            #endregion
                            else
                            #region "new Reg_Plan_Detail"
                            {
                                foreach (var item in tvr.Items)
                                {
                                    BRApi.ErrorLog.LogMessage(si, $"Hit: {item.Key} - {item.Value}");
                                }
                                var new_Register_Plan_Details_Row = Register_Plan_Details_DT.NewRow();
                                new_Register_Plan_Details_Row["Register_Plan_ID"] = Guid.NewGuid();
                                new_Register_Plan_Details_Row["WF_Scenario_Name"] = scenarioName;
                                new_Register_Plan_Details_Row["WF_Profile_Name"] = profileName;
                                new_Register_Plan_Details_Row["WF_Time_Name"] = timeName;
                                new_Register_Plan_Details_Row["Act_ID"] = activityID;
                                new_Register_Plan_Details_Row["Model_ID"] = modelID;
                                new_Register_Plan_Details_Row["Entity"] = tvr.Items["Entity"]?.Value.ToString() ?? string.Empty;
                                //new_Register_Plan_Details_Row["Approval_Level_ID"] = tvr.Items["Approval_Level_ID"].Value.ToString() != string.Empty ? tvr.Items["Approval_Level_ID"].Value.ToString() : approvLevelID; // might need to do Guid.Parse()
                                new_Register_Plan_Details_Row["Approval_Level_ID"] = tvr.Items["Approval_Level_ID"].Value.ToString() != string.Empty ? tvr.Items["Approval_Level_ID"].Value.ToString() : string.Empty; // might need to do Guid.Parse()
                                new_Register_Plan_Details_Row["Plan_Units"] = planUnits;
                                //								new_Register_Plan_Details_Row["Account"] = tvr.Items["Account"]?.Value.ToString() ?? string.Empty; //planUnits;
                                //	                            new_Register_Plan_Details_Row["Flow"] = tvr.Items["Flow"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD1"] = tvr.Items["UD1"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD2"] = tvr.Items["UD2"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD3"] = tvr.Items["UD3"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD4"] = tvr.Items["UD4"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD5"] = tvr.Items["UD5"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD6"] = tvr.Items["UD6"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD7"] = tvr.Items["UD7"]?.Value.ToString() ?? string.Empty;
                                //								new_Register_Plan_Details_Row["UD8"] = tvr.Items["UD8"]?.Value.ToString() ?? string.Empty;
                                new_Register_Plan_Details_Row["Year"] = yearConfig;
                                //get scenario frequency
                                //var (scenarioFrequency, periods) = timephasing_timePeriods[yearConfig];

                                // Populate time periods
                                var yearlyTotal = 0.0m;
                                var Quarter1 = 0.0m;
                                var Quarter2 = 0.0m;
                                var Quarter3 = 0.0m;
                                var Quarter4 = 0.0m;
                                #region "new Monthly"			
                                if (yearConfig.Frequency == "Monthly")
                                {
                                    for (int month = 1; month <= 12; month++)
                                    {
                                        var monthValue = 0.0m;
                                        if (yearConfig.Periods.Contains(month.XFToString()))
                                        {
                                            // If the period exists in the list, fetch the value from `tvr.Items`
                                            monthValue = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig}_M{month}"].Value) ? "0" : tvr.Items[$"{yearConfig}_M{month}"].Value);
                                            new_Register_Plan_Details_Row[$"Month{month}"] = monthValue;
                                        }
                                        else
                                        {
                                            // If period doesn't exist, attempt to retrieve from the row
                                            monthValue = new_Register_Plan_Details_Row[$"Month{month}"] != DBNull.Value ? Convert.ToDecimal(new_Register_Plan_Details_Row[$"Month{month}"]) : 0;
                                        }
                                        yearlyTotal += monthValue;

                                        if (month <= 3) Quarter1 += monthValue;
                                        else if (month <= 6) Quarter2 += monthValue;
                                        else if (month <= 9) Quarter3 += monthValue;
                                        else Quarter4 += monthValue;

                                    }
                                    new_Register_Plan_Details_Row["Quarter1"] = Quarter1;
                                    new_Register_Plan_Details_Row["Quarter2"] = Quarter2;
                                    new_Register_Plan_Details_Row["Quarter3"] = Quarter3;
                                    new_Register_Plan_Details_Row["Quarter4"] = Quarter4;
                                    new_Register_Plan_Details_Row["Yearly"] = yearlyTotal;
                                }
                                #endregion
                                #region "new Quarterly"
                                else if (yearConfig.Frequency == "Quarterly")
                                {
                                    for (int quarter = 1; quarter <= 4; quarter++)
                                    {
                                        var quarterValue = 0.0m;
                                        if (yearConfig.Periods.Contains(quarter.XFToString()))
                                        {
                                            quarterValue = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig}_Q{quarter}"].Value) ? "0" : tvr.Items[$"{yearConfig}_Q{quarter}"].Value);
                                            new_Register_Plan_Details_Row[$"Quarter{quarter}"] = quarterValue;
                                        }
                                        else
                                        {
                                            quarterValue = new_Register_Plan_Details_Row[$"Quarter{quarter}"] != DBNull.Value ? Convert.ToDecimal(new_Register_Plan_Details_Row[$"Quarter{quarter}"]) : 0;
                                        }
                                        yearlyTotal += quarterValue;
                                    }
                                    new_Register_Plan_Details_Row["Yearly"] = yearlyTotal;
                                }
                                #endregion
                                #region "new Yearly"
                                else if (yearConfig.Frequency == "Yearly")
                                {
                                    new_Register_Plan_Details_Row["Yearly"] = Convert.ToDecimal(string.IsNullOrEmpty(tvr.Items[$"{yearConfig.Year}"].Value) ? "0" : tvr.Items[$"{yearConfig.Year}"].Value);
                                }
                                #endregion
                                Register_Plan_Details_DT.Rows.Add(new_Register_Plan_Details_Row);
                            }
                            #endregion
                        }
                    }
                    #endregion
                }
                regPlanHelpers.Update_Register_Plan(si, Register_Plan_DT, regPlanAdapter);
                regPlanDetailsHelpers.Update_Register_Plan_Details(si, Register_Plan_Details_DT, regPlanDetailsAdapter);
            }
            #endregion
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
        #endregion

        #region "Helper functions"
        #region "Get Data Helpers"
        private DataTable getRegisterTable(DataTable registerColumns, DataTable registerConfig, Dictionary<string, string> substVars)
        {
            var regDt = new DataTable();

            // Retrieve Time_Phasing and start/end times from RegisterConfig
            string timePhasing = registerConfig.Rows.Count > 0 ? registerConfig.Rows[0]["Time_Phasing"]?.ToString() : string.Empty;
            string startTime = registerConfig.Rows.Count > 0 ? registerConfig.Rows[0]["Start_Dt_Src"]?.ToString() : string.Empty;
            string endTime = registerConfig.Rows.Count > 0 ? registerConfig.Rows[0]["End_Dt_Src"]?.ToString() : string.Empty;
            g_activityID = Convert.ToInt32(registerConfig.Rows[0]["Act_ID"]);

            // Fetch Workflow Information
            var wfInfoDetails = getWFInfoDetails();

            // Fetch YearTimePhasingConfigs if Time_Phasing is 'Input'

            if (timePhasing == "Input")
            {
                //Get TimePhasing Start/End Date
                yearConfigs = GetYearTimePhasingConfigs(startTime, endTime);
            }

            // Create a dictionary to hold filter parameter values mapped to column names

            foreach (var kvp in substVars)
            {
                // Search the registerColumns DataTable for rows where Filter_Param equals kvp.Key
                var matchingRow = registerColumns.AsEnumerable()
                                                 .FirstOrDefault(row => row.Field<string>("Filter_Param") == kvp.Key);

                if (matchingRow != null)
                {
                    // Grab the column name from the "Name" column
                    string columnName = matchingRow.Field<string>("Name");

                    // Add the column name and value to the new dictionary
                    filterParams[columnName] = kvp.Value;
                }
            }

            // Build the SELECT clause
            string selectedColumns = string.Join(", ", registerColumns.Rows.Cast<DataRow>().Select(row => $"rp.{row["Name"]}"));
            selectedColumns += ", rp.Invalid";
            // Add additional columns based on yearConfigs if Time_Phasing is "Input"
            if (timePhasing == "Input" && yearConfigs != null)
            {
                var json = JsonSerializer.Serialize(yearConfigs);
                var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                BRApi.State.SetSessionState(si, false, ClientModuleType.Unknown, typeof(Dictionary<string, string>).Name, string.Empty, "yearConfigs", si.UserName, string.Empty, bytes);

                var timePhasingColumns = new List<string>();
                foreach (var config in yearConfigs)
                {
                    foreach (var period in config.Periods)
                    {
                        timePhasingColumns.Add(GetColumnNameAndAlias(config.Year, period));
                    }
                }
                // Append time-phased columns to the SELECT clause
                selectedColumns += ", " + string.Join(", ", timePhasingColumns);
            }
            string baseSelect = $"SELECT rp.Register_Plan_ID, rp.WF_Scenario_Name, rp.WF_Profile_Name, rp.WF_Time_Name, " +
                                $"rp.Act_ID, {selectedColumns}";

            var Plan_Units = string.Empty;
            Plan_Units = "'Test'";
            // Build the FROM and optional JOIN clause
            string fromClause = "FROM Register_Plan rp ";
            if (timePhasing == "Input" && yearConfigs != null)
            {
                var joinClauses = new List<string>();
                foreach (var config in yearConfigs)
                {
                    string alias = $"rpd_{config.Year}";
                    joinClauses.Add($"LEFT JOIN Register_Plan_Details {alias} " +
                                    $"ON rp.Register_Plan_ID = {alias}.Register_Plan_ID " +
                                    $"AND rp.Act_ID = {alias}.Act_ID " +
                                    $"AND rp.WF_Scenario_Name = {alias}.WF_Scenario_Name " +
                                    $"AND rp.Entity = {alias}.Entity " +
                                    $"AND rp.Approval_Level_ID = {alias}.Approval_Level_ID " +
                                    $"AND {alias}.Year = '{config.Year}' " +
                                    $"AND {alias}.Plan_Units In ({Plan_Units}) ");
                }

                fromClause += " " + string.Join(" ", joinClauses);
            }

            // Build the WHERE clause
            string whereClause = $"WHERE rp.WF_Scenario_Name = '{wfInfoDetails[scenarioDictKey]}' " +
                                 $"AND rp.WF_Profile_Name = '{wfInfoDetails[profileDictKey]}' " +
                                 $"AND rp.WF_Time_Name = {wfInfoDetails[timeDictKey]} " +
                                 $"AND rp.Act_ID = {g_activityID} ";

            var whereConditions = new List<string>();
            // Append filter parameter conditions dynamically
            foreach (var param in filterParams)
            {
                whereConditions.Add($"AND rp.{param.Key} = '{param.Value}'");
            }

            whereClause += string.Join(" ", whereConditions);

            // Combine the SQL parts
            string sql = $"{baseSelect} {fromClause} {whereClause}";

            // Log the SQL query
            BRApi.ErrorLog.LogMessage(si, $"Generated SQL: {sql}");

            // Execute the SQL query and fetch results
            using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                regDt = BRApi.Database.ExecuteSql(dbConnApp, sql, false);
            }

            //		    // Process the data if Time_Phasing is 'Input'
            //		    if (timePhasing == "Input" && yearConfigs != null)
            //		    {
            //		        foreach (var config in yearConfigs)
            //		        {
            //		            BRApi.ErrorLog.LogMessage(si, $"Processing year {config.Year} with frequency {config.Frequency} and periods: {string.Join(", ", config.Periods)}");
            //		            // Add additional processing logic if necessary
            //		        }
            //		    }

            return regDt;
        }

        private List<YearTimePhasingConfig> GetYearTimePhasingConfigs(string startTime, string endTime)
        {
            var configs = new List<YearTimePhasingConfig>();
            var wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si);
            var wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo();
            var scenarioKey = wfUnitInfo.WfUnitPk.ScenarioKey;

            // Check if the tracking frequency is a range
            WorkflowTrackingFrequency wfTrackingFreq = BRApi.Finance.Scenario.GetWorkflowTrackingFrequency(si, scenarioKey);
            bool isRangeTrackingFreq = wfTrackingFreq == WorkflowTrackingFrequency.Range;

            // Determine the start and end years
            int wfStartTime = DetermineYear(startTime, "|WFTime|", "|WFStartTime|", isRangeTrackingFreq, () => BRApi.Finance.Scenario.GetWorkflowTime(si, scenarioKey));
            int wfEndTime = DetermineYear(endTime, "|WFTime|", "|WFEndTime|", isRangeTrackingFreq, () => BRApi.Finance.Scenario.GetWorkflowEndTime(si, scenarioKey));

            BRApi.ErrorLog.LogMessage(si, $"wf start: {wfStartTime}, wf end: {wfEndTime}");

            // Populate YearTimePhasingConfig objects
            for (int year = wfStartTime; year <= wfEndTime; year++)
            {
                var periods = new List<string>();
                var inputFreq = BRApi.Finance.Scenario.GetInputFrequencyForYear(si, scenarioKey, year);

                switch (inputFreq.Name)
                {
                    case "Monthly":
                        periods.AddRange(Enumerable.Range(1, 12).Select(month => new DateTime(year, month, 1).ToString("MMM", CultureInfo.InvariantCulture)));
                        break;

                    case "Quarterly":
                        periods.AddRange(Enumerable.Range(1, 4).Select(quarter => $"Q{quarter}"));
                        break;

                    case "HalfYearly":
                        periods.Add("H1");
                        periods.Add("H2");
                        break;

                    case "Yearly":
                        periods.Add("Yearly");
                        break;
                }

                configs.Add(new YearTimePhasingConfig(year.ToString(), periods, inputFreq.Name));
            }

            return configs;
        }

        private int DetermineYear(string time, string wfTime, string wfStartOrEndTime, bool isRangeTrackingFreq, Func<int> getWorkflowTimeFunc)
        {
            if (time == wfTime)
            {
                return Convert.ToInt32(getWorkflowTimeFunc().ToString().Substring(0, 4));
            }
            else if (time == wfStartOrEndTime && isRangeTrackingFreq)
            {
                return Convert.ToInt32(getWorkflowTimeFunc().ToString().Substring(0, 4));
            }
            else if (int.TryParse(time, out int parsedYear))
            {
                return parsedYear;
            }
            return 0;
        }


        private DataTable getRegisterColumnProfile(string registerConfigID, Dictionary<string, string> substVars)
        {
            var dt = new DataTable();

            var sqlcolumns = string.Empty;
            sqlcolumns = @"
							SELECT [Order],Name,Alias,[Default],Param,Required,Filter_Param
							  FROM FMM_Col_Config
							  WHERE Reg_Config_ID = @Reg_Config_ID
							  AND InUse = 1
							  ORDER BY [Order]";

            sqlcolumns = sqlcolumns.Replace("@Reg_Config_ID", registerConfigID);

            using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                if (registerConfigID != string.Empty)
                {
                    dt = BRApi.Database.ExecuteSql(dbConnApp, sqlcolumns.ToString(), false);
                }
            }

            // Check if DataTable has data
            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    // Check if the Filter_Param column has a value
                    if (row["Filter_Param"] != DBNull.Value && !string.IsNullOrWhiteSpace(row["Filter_Param"].ToString()))
                    {
                        var columnName = row["Name"].ToString(); // Get the column name
                        var filterParam = row["Filter_Param"].ToString(); // Get the Filter_Param value
                        var paramValue = substVars.XFGetValue(filterParam, string.Empty);

                        // Check if the param exists in the provided dictionary
                        if (!string.IsNullOrEmpty(paramValue))
                        {
                            // Add to the result dictionary
                            columnParamDictionary[columnName] = paramValue;
                        }
                    }
                }
            }

            return dt;
        }

        private DataTable getRegisterConfig(string registerConfigID)
        {
            DataTable dt = new DataTable();

            var sqlcolumns = string.Empty;
            sqlcolumns = @"
							SELECT Name, Time_Phasing, Time_Phasing_Driver, Manual_Input_Plan_Units, Start_End_Dt_Src_Obj,Approval_Config,
							Start_Dt_Src, End_Dt_Src, Act_ID
							  FROM FMM_Reg_Config
							  WHERE Reg_Config_ID = @Reg_Config_ID";

            sqlcolumns = sqlcolumns.Replace("@Reg_Config_ID", registerConfigID);

            using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                if (registerConfigID != string.Empty)
                {
                    dt = BRApi.Database.ExecuteSql(dbConnApp, sqlcolumns.ToString(), false);
                }

            }

            return dt;
        }

        private TableViewRow CreateHeaderRow(DataTable regDt, DataTable columnsDt)
        {
            var headerRow = new TableViewRow { IsHeader = true };

            foreach (DataColumn col in regDt.Columns)
            {
                var headerColumn = new TableViewColumn
                {
                    Name = col.ColumnName,
                    DataType = MapDataType(col.DataType),
                    Value = GetHeaderValue(columnsDt, col)
                };

                if (headerColumn.Value.StartsWith("**"))
                {
                    headerColumn.IsHeader = true;
                    //headerColumn.ColumnFormat.BackgroundColor = XFColors.AliceBlue;
                    headerColumn.ColumnFormat.IsBold = true;
                }

                headerRow.Items.Add(headerColumn.Name, headerColumn);
            }

            return headerRow;
        }

        private XFDataType MapDataType(Type dataType) =>
            dataType == typeof(decimal) ? XFDataType.Decimal :
            dataType == typeof(string) ? XFDataType.Text :
            dataType == typeof(DateTime) ? XFDataType.DateTime :
            dataType == typeof(bool) ? XFDataType.Boolean :
            XFDataType.Text;

        private string GetHeaderValue(DataTable columnsDt, DataColumn col)
        {
            var foundRow = columnsDt.AsEnumerable().FirstOrDefault(row => row["Name"].ToString() == col.ColumnName);
            if (foundRow != null)
            {
                return foundRow["Required"] != DBNull.Value && Convert.ToBoolean(foundRow["Required"])
                    ? $"** {foundRow[aliasColName]}"
                    : foundRow[aliasColName].ToString();
            }
            return col.ColumnName;
        }

        private string GetColumnNameAndAlias(string year, string period)
        {
            // Generate the alias for the Register_Plan_Detail table based on year
            string alias = $"rpd_{year}";
            string columnName;

            // Apply conditional logic based on the content of the period
            if (period.Contains("Month", StringComparison.OrdinalIgnoreCase))
            {
                columnName = $"{alias}.[{year}_M{period.Replace("Month", "").Trim()}] AS [{year} M{period.Replace("Month", "").Trim()}]";
            }
            else if (period.Contains("Quarter", StringComparison.OrdinalIgnoreCase))
            {
                columnName = $"{alias}.[{year}_Q{period.Replace("Quarter", "").Trim()}] AS [{year} Q{period.Replace("Quarter", "").Trim()}]";
            }
            else if (period.Equals("Yearly", StringComparison.OrdinalIgnoreCase))
            {
                columnName = $"{alias}.[Yearly] AS [{year}]";
            }
            else
            {
                // Default case if period doesn't match known patterns
                columnName = $"{alias}.[{period}]";
            }

            return columnName;
        }

        private void ConfigureColumns(TableView regTV, DataTable columnsDt, ColumnConfigProvider configProvider)
        {
            foreach (var col in regTV.Columns)
            {
                var foundRow = columnsDt.AsEnumerable().FirstOrDefault(row => row["Name"].ToString() == col.Name);
                if (foundRow != null)
                {
                    var colConfig = configProvider.GetColumnConfig(foundRow);

                    if (colConfig.colParam)
                    {
                        col.WorkspaceName = colConfig.WorkspaceName;
                        col.ParameterName = colConfig.ParameterName;
                        col.UseDisplay = colConfig.UseDisplay;
                    }

                    col.ColumnFormat.ColumnWidth = 20;

                    if (foundRow["Required"] != DBNull.Value && Convert.ToBoolean(foundRow["Required"]))
                    {
                        col.ColumnFormat.BackgroundColor = XFColors.DeepSkyBlue;
                    }
                }
                else
                {
                    col.ColumnFormat.ColumnWidth = col.Name.Contains("Name") || col.Name.Contains("ID") ? 0.01 : 10;
                }
            }
        }

        public class ColumnConfigProvider
        {
            public ColumnConfig GetColumnConfig(DataRow colConfig)
            {
                var config = new ColumnConfig();

                if (colConfig == null)
                {
                    throw new ArgumentNullException(nameof(colConfig), "DataRow cannot be null.");
                }

                // Check if "Param" column exists and is not null
                if (colConfig.Table.Columns.Contains("Param") && colConfig["Param"] != DBNull.Value)
                {
                    //					BRApi.ErrorLog.LogMessage(si,"Hit: " + paramValue);
                    //					BRApi.ErrorLog.LogMessage(si,"Hit: " + paramValue + " " + colConfig["Name"].ToString());
                    var paramValue = colConfig["Param"].ToString();
                    var parts = new List<string>();
                    parts = StringHelper.SplitString(paramValue, ".", "");

                    //throw new ArgumentNullException(nameof(colConfig), paramValue + "DataRow cannot be null." + " " + colConfig["Name"].ToString());

                    if (parts.Count == 2)
                    {
                        config.colParam = true;
                        config.WorkspaceName = parts[0];
                        config.ParameterName = parts[1];
                        config.UseDisplay = true;
                    }
                    else
                    {
                        //						BRApi.ErrorLog.LogMessage(si,"Hit Config Helper Else: " + paramValue + " - " + colConfig["Name"].ToString());
                        config.colParam = false;
                        config.WorkspaceName = string.Empty;
                        config.ParameterName = string.Empty;
                        config.UseDisplay = false;

                    }
                }
                else
                {
                    //					BRApi.ErrorLog.LogMessage(si,"Hit Config Helper Outer Else:  - " + colConfig["Name"].ToString());
                    // Default values
                    config.colParam = false;
                    config.WorkspaceName = string.Empty;
                    config.ParameterName = string.Empty;
                    config.UseDisplay = false;
                }

                config.ColumnWidth = colConfig.Table.Columns.Contains("Format") && double.TryParse(colConfig["Format"].ToString(), out var columnWidth) ? columnWidth : 10;
                config.Required = colConfig.Table.Columns.Contains("Required") && bool.TryParse(colConfig["Required"].ToString(), out var required) ? required : false;

                //				BRApi.ErrorLog.LogMessage(si,"Hit Config Helper:  - " + colConfig["Name"].ToString() + "-" + config.colParam.ToString());
                return config;
            }
        }

        public class ColumnConfig
        {
            public bool colParam { get; set; }
            public string WorkspaceName { get; set; }
            public string ParameterName { get; set; }
            public bool UseDisplay { get; set; }
            public double ColumnWidth { get; set; }
            public bool Required { get; set; }
            public XFColor BackgroundColor { get; set; }
        }

        public class YearTimePhasingConfig
        {
            public string Year { get; set; }
            public List<string> Periods { get; set; }
            public string Frequency { get; set; }

            public YearTimePhasingConfig(string year, List<string> periods, string frequency)
            {
                Year = year;
                Periods = periods;
                Frequency = frequency;
            }
        }


        #endregion

        #region "save Data Helpers"
        private void fillReqColumnsList(ref List<DataColumn> reqCols, string regConfID, Dictionary<string, string> substVars)
        {
            DataTable colConfigDT = getRegisterColumnProfile(regConfID, substVars);

            DataRow[] foundReqColRows = colConfigDT.Select("Required = 1");

            foreach (DataRow dr in foundReqColRows)
            {
                reqCols.Add(new DataColumn(dr["Name"].ToString()));
                //				BRApi.ErrorLog.LogMessage(si, $"Adding {dr["Name"].ToString()} to req col list");
            }

        }

        // checks all the columns that are set to determine whether row should be saved and if row is invalid
        private bool shouldSaveRow(DataColumn[] reqCols, TableViewRow tvr)
        {
            return numReqColsFilled(reqCols, tvr) > 0;
        }

        private bool isRowInvalid(DataColumn[] reqCols, TableViewRow tvr)
        {
            return numReqColsFilled(reqCols, tvr) == reqCols.Length;
        }

        private int numReqColsFilled(DataColumn[] reqCols, TableViewRow tvr)
        {
            int filledCols = 0;
            List<string> emptyCols = new List<string>();

            foreach (DataColumn dc in reqCols)
            {
                string colName = dc.ColumnName;
                if (tvr.Items.ContainsKey(colName))
                {
                    if (tvr[colName].Value != string.Empty)
                    {
                        //						BRApi.ErrorLog.LogMessage(si, $"col {colName} filled with {tvr[colName].Value} val");
                        filledCols++;
                    }
                    else
                    {
                        emptyCols.Add(colName);
                    }
                }
            }

            return filledCols;
        }

        private bool reqColumnsFilled(DataColumn[] reqCols, TableViewRow tvr, DataTable colAliasResolutionDT)
        {
            var filledCols = 0;
            var emptyCols = new List<string>();

            colAliasResolutionDT.PrimaryKey = new DataColumn[] {
                    colAliasResolutionDT.Columns[nameColName]
                };

            foreach (var dc in reqCols)
            {
                string aliasedColName = colAliasResolutionDT.Rows.Find(dc.ColumnName)[aliasColName].ToString();
                if (tvr.Items.ContainsKey(aliasedColName))
                {
                    if (tvr.Items[aliasedColName].Value != null && tvr.Items[aliasedColName].Value != string.Empty)
                    {
                        filledCols++;
                    }
                    else
                    {
                        emptyCols.Add(aliasedColName);
                    }
                }
            }

            return filledCols == reqCols.Length;
        }

        //checks if the column exists in the table view row and set it, otherwise if the col is nullable, set null
        private void checkAndSetColumn(ref DataRow row, TableViewRow tvr, string colName)
        {
            colTypeConfig colConfig = new colTypeConfig(colName);

            if (tvr.Items.ContainsKey(colName))
            {
                string val = tvr[colName].Value;
                switch (colConfig.colType)
                {
                    case colTypeEnum.DateTime:
                        if (val != string.Empty)
                        {
                            row[colName] = Convert.ToDateTime(val);
                        }
                        else
                        {
                            setNullCol(ref row, colName, colConfig);
                        }
                        break;
                    case colTypeEnum.Decimal:
                        if (val != string.Empty)
                        {
                            row[colName] = Convert.ToDecimal(val);
                        }
                        else
                        {
                            setNullCol(ref row, colName, colConfig);
                        }
                        break;
                    case colTypeEnum.Int:
                        if (val != string.Empty)
                        {
                            row[colName] = Convert.ToInt32(val);
                        }
                        else
                        {
                            setNullCol(ref row, colName, colConfig);
                        }
                        break;
                    case colTypeEnum.NVarChar:
                        if (filterParams.ContainsKey(colName))
                        {
                            string paramValue = filterParams[colName];
                            if (!string.IsNullOrEmpty(paramValue))
                            {
                                row[colName] = paramValue;
                            }
                            else
                            {
                                row[colName] = string.Empty;
                            }
                        }
                        else
                        {
                            row[colName] = val;
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                setNullCol(ref row, colName, colConfig);
            }
        }

        private void setNullCol(ref DataRow row, string colName, colTypeConfig colConfig)
        {
            if (colConfig.isColNullable)
            {
                row[colName] = DBNull.Value;
            }
            else
            {
                switch (colConfig.colType)
                {
                    case colTypeEnum.DateTime:
                        row[colName] = new DateTime(0);
                        break;
                    case colTypeEnum.Decimal:
                        row[colName] = 0.0;
                        break;
                    case colTypeEnum.Int:
                        row[colName] = -1;
                        break;
                    default:
                        break;
                }
            }
        }

        public enum colTypeEnum { DateTime, Int, Decimal, NVarChar }

        public class colTypeConfig
        {
            public colTypeEnum colType;
            public bool isColNullable = false;

            public colTypeConfig(string colName)
            {
                if (colName.Contains("Attribue_Value") || colName == "Spread_Amount")
                {
                    colType = colTypeEnum.Decimal;
                    isColNullable = true;
                }
                else if (colName.Contains("Date_Value"))
                {
                    colType = colTypeEnum.DateTime;
                    isColNullable = true;
                }
                else if (colName.Contains("Register_ID_"))
                {
                    colType = colTypeEnum.Int;
                    isColNullable = false;
                }
                else
                {
                    colType = colTypeEnum.NVarChar;
                    isColNullable = true;
                }
            }

        }


        #endregion

        private void SetColWidths(ref Dictionary<string, int> ColChars, ref TableView TV)
        {
            foreach (var TVC in TV.Columns)
            {
                if (ColChars.ContainsKey(TVC.Name))
                {
                    TVC.ColumnFormat.ColumnWidth = ColChars[TVC.Name] < 255 ? ColChars[TVC.Name] : 255;
                }
            }
        }

        private void AddTotalRow(ref TableView TV, ref SortedDictionary<string, decimal> ColTots, SessionInfo si)
        {
            var totRow = new TableViewRow();

            //find the first non-header row to copy all keys
            var rowToCopy = TV.Rows.Find(
                delegate (TableViewRow TVR)
                {
                    return !TVR.IsHeader;
                }
            );

            // deep copy the row so we don't affect the original row
            totRow.Items = JsonSerializer.Deserialize<Dictionary<string, TableViewColumn>>(JsonSerializer.Serialize(rowToCopy.Items));

            // remove values for all columns and set totals for all decimal columns
            var i = 0;
            foreach (var key in totRow.Items.Keys)
            {
                var toSetValue = "";
                if (key == "RegisterID")
                {
                    toSetValue = "TOTAL";
                }

                //                if (Get_DataType(key) == XFDataType.Decimal)
                //                {
                if (ColTots.ContainsKey(key))
                {
                    toSetValue = ColTots[key].ToString("C", CultureInfo.CurrentCulture);
                }
                //                }

                totRow.Items[key].Value = toSetValue;
                i++;
            }

            TV.Rows.Insert(1, totRow);
        }


        private string stripFormatting(string inpStr)
        {
            return Regex.Replace(inpStr, "[^0-9.]", "");
        }


        private Dictionary<string, string> getWFInfoDetails()
        {
            Dictionary<string, string> WFInfoDetails = new Dictionary<string, string>();

            var wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si);
            var wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo();

            WFInfoDetails.Add(profileDictKey, wfUnitInfo.ProfileName);
            WFInfoDetails.Add(scenarioDictKey, wfUnitInfo.ScenarioName);
            WFInfoDetails.Add(timeDictKey, wfUnitInfo.TimeName);

            return WFInfoDetails;
        }


        private string formatValue(string colName, string val)
        {
            string formattedVal = val;

            //			switch(Get_DataType(colName)) {
            //				case XFDataType.Decimal:
            //					formattedVal = Convert.ToDecimal(formattedVal).ToString("C", CultureInfo.CurrentCulture);
            //					break;
            //				default:
            //					break;
            //			}

            return formattedVal;
        }

        //		private void updateDataTableForTimePhasing(ref DataTable dt, string timePhasing, string startTime, string endTime) {	

        //			DataColumn[] dataColsToAdd = getTimePhasingColumns(startTime, endTime);

        //			BRApi.ErrorLog.LogMessage(si, "timephasing " + timePhasing);


        //			switch (timePhasing) {
        //				case "Input":
        //					for(int i = 0; i < dataColsToAdd.Length; i++) {
        //						dt.Columns.Add(dataColsToAdd[i]);
        //					}
        //					break;
        //				case "Derived":
        //				default:
        //					break;
        //			}

        //		}




        #endregion
    }
}