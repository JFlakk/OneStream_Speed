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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DVM_Config_Data
{
    public class MainClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                switch (args.FunctionType)
                {
                    case DashboardExtenderFunctionType.LoadDashboard:
                        return HandleLoadDashboard(si, args);

                    case DashboardExtenderFunctionType.ComponentSelectionChanged:
                        return HandleSelectionChanged(si, args);

                    case DashboardExtenderFunctionType.SqlTableEditorSaveData:
                        return HandleSaveData(si, args);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        // =====================================================================
        #region "Load Dashboard"
        // =====================================================================

        private object HandleLoadDashboard(SessionInfo si, DashboardExtenderArgs args)
        {
            var result = new XFLoadDashboardTaskResult();
            result.ChangeCustomSubstVarsInDashboard = false;
            result.ModifiedCustomSubstVars = null;

            if (args.LoadDashboardTaskInfo?.Reason != LoadDashboardReasonType.Initialize)
                return result;

            // On first load, set default substitution variables
            if (args.FunctionName.XFEqualsIgnoreCase("DVM_Config_Load"))
            {
                result.ChangeCustomSubstVarsInDashboard = true;
                result.ModifiedCustomSubstVars.TryAdd("!DVM_Config_ID", "0");
                result.ModifiedCustomSubstVars.TryAdd("!DVM_Run_ID",    "0");
            }

            return result;
        }

        #endregion

        // =====================================================================
        #region "Component Selection Changed"
        // =====================================================================

        private object HandleSelectionChanged(SessionInfo si, DashboardExtenderArgs args)
        {
            var result = new XFSelectionChangedTaskResult();
            result.IsOK = true;
            result.ShowMessageBox = false;
            result.Message = string.Empty;
            result.ChangeCustomSubstVarsInDashboard = false;
            result.ChangeSelectionChangedUIActionInDashboard = false;
            result.ChangeSelectionChangedNavigationInDashboard = false;

            // When the user selects a config, propagate the config ID to other components
            if (args.FunctionName.XFEqualsIgnoreCase("DVM_Config_Selected"))
            {
                var selectedValue = args.SelectionChangedTaskInfo?.CustomSubstVarsWithUserSelectedValues
                    .XFGetValue("!DVM_Config_ID", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedValue))
                {
                    result.ChangeCustomSubstVarsInDashboard = true;
                    result.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;

                    var substVars = result.ModifiedCustomSubstVars;
                    // Reset run ID when config changes
                    if (substVars.ContainsKey("!DVM_Run_ID"))
                        substVars["!DVM_Run_ID"] = "0";
                }
            }

            // When the user selects a run, propagate the run ID for results display
            if (args.FunctionName.XFEqualsIgnoreCase("DVM_Run_Selected"))
            {
                var selectedValue = args.SelectionChangedTaskInfo?.CustomSubstVarsWithUserSelectedValues
                    .XFGetValue("!DVM_Run_ID", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedValue))
                {
                    result.ChangeCustomSubstVarsInDashboard = true;
                    result.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
                }
            }

            return result;
        }

        #endregion

        // =====================================================================
        #region "Save Data (SQL Table Editor)"
        // =====================================================================

        private object HandleSaveData(SessionInfo si, DashboardExtenderArgs args)
        {
            var saveResult = new XFSqlTableEditorSaveDataTaskResult();
            saveResult.IsOK = true;
            saveResult.ShowMessageBox = false;
            saveResult.Message = string.Empty;
            saveResult.CancelDefaultSave = true; // We handle the save ourselves

            var saveInfo = args.SqlTableEditorSaveDataTaskInfo;

            switch (args.FunctionName.ToUpperInvariant())
            {
                case "DVM_CONFIG_SAVE":
                    SaveDvmConfig(si, saveInfo);
                    break;

                case "DVM_RULE_SAVE":
                    SaveDvmRule(si, saveInfo);
                    break;

                case "DVM_RUN_VALIDATION":
                    RunValidation(si, args, saveResult);
                    break;

                default:
                    saveResult.CancelDefaultSave = false; // fall through to default save
                    break;
            }

            return saveResult;
        }

        // ------------------------------------------------------------------

        private void SaveDvmConfig(SessionInfo si, XFSqlTableEditorSaveDataTaskInfo saveInfo)
        {
            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                using (var sqa = new SqlDataAdapter())
                {
                    var dt = saveInfo.EditedDataRows.CopyToDataTable();

                    // Stamp audit columns on modified / added rows
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                        {
                            row["Update_Date"] = DateTime.Now;
                            row["Update_User"] = si.UserName;
                        }
                    }

                    var adapter = new SQA_DVM_Validation(si, conn);
                    adapter.Update_DVM_Config(si, dt, sqa);
                }
            }
        }

        private void SaveDvmRule(SessionInfo si, XFSqlTableEditorSaveDataTaskInfo saveInfo)
        {
            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();

                using (var sqa = new SqlDataAdapter())
                {
                    var dt = saveInfo.EditedDataRows.CopyToDataTable();

                    // Stamp audit columns
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified)
                        {
                            row["Update_Date"] = DateTime.Now;
                            row["Update_User"] = si.UserName;
                        }
                    }

                    var adapter = new SQA_DVM_Validation(si, conn);
                    adapter.Update_DVM_Rule(si, dt, sqa);
                }
            }
        }

        private void RunValidation(SessionInfo si, DashboardExtenderArgs args,
            XFSqlTableEditorSaveDataTaskResult saveResult)
        {
            var dvmConfigIDStr = args.SqlTableEditorSaveDataTaskInfo?.CustomSubstVars
                ?.XFGetValue("!DVM_Config_ID", "0");
            if (!int.TryParse(dvmConfigIDStr, out int dvmConfigID) || dvmConfigID <= 0)
            {
                saveResult.IsOK = false;
                saveResult.ShowMessageBox = true;
                saveResult.Message = "Please select a validation configuration before running.";
                return;
            }

            var contextType = GetContextType(si, dvmConfigID);

            var engine = new DVM_Validation_Engine();
            DataTable results;

            if (contextType.XFEqualsIgnoreCase("Cube"))
            {
                results = engine.RunCubeValidation(si, dvmConfigID, si.UserName);
            }
            else
            {
                results = engine.RunTableValidation(si, dvmConfigID, si.UserName);
            }

            var failCount = results.AsEnumerable()
                .Count(r => r.Field<string>("Status").XFEqualsIgnoreCase("Fail"));
            var warnCount = results.AsEnumerable()
                .Count(r => r.Field<string>("Status").XFEqualsIgnoreCase("Warning"));

            saveResult.ShowMessageBox = true;
            saveResult.Message = $"Validation complete. Rules evaluated: {results.Rows.Count}. " +
                                  $"Failures: {failCount}. Warnings: {warnCount}.";
        }

        // ------------------------------------------------------------------

        private string GetContextType(SessionInfo si, int dvmConfigID)
        {
            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa      = new SQA_DVM_Validation(si, conn);
                var configDt = sqa.Get_DVM_Config_By_ID(si, dvmConfigID);
                return configDt.Rows.Count > 0 ? configDt.Rows[0]["Context_Type"].ToString() : "Table";
            }
        }

        #endregion
    }
}
