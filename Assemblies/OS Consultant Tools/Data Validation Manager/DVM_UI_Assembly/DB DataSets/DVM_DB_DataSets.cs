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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DVM_DB_DataSets
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
                this.si      = si;
                this.globals = globals;
                this.api     = api;
                this.args    = args;

                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        break;

                    case DashboardDataSetFunctionType.GetDataSet:
                        switch (args.DataSetName)
                        {
                            case "Get_DVM_Configs":
                                return Get_DVM_Configs();

                            case "Get_DVM_Config_By_ID":
                                return Get_DVM_Config_By_ID();

                            case "Get_DVM_Rules":
                                return Get_DVM_Rules();

                            case "Get_DVM_Rule_Types":
                                return Get_DVM_Rule_Types();

                            case "Get_DVM_Severities":
                                return Get_DVM_Severities();

                            case "Get_DVM_Context_Types":
                                return Get_DVM_Context_Types();

                            case "Get_DVM_Recent_Runs":
                                return Get_DVM_Recent_Runs();

                            case "Get_DVM_Runs_By_Config":
                                return Get_DVM_Runs_By_Config();

                            case "Get_DVM_Results":
                                return Get_DVM_Results();

                            case "Get_DVM_Failures":
                                return Get_DVM_Failures();
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

        // ------------------------------------------------------------------
        // DVM_Config queries
        // ------------------------------------------------------------------

        private DataTable Get_DVM_Configs()
        {
            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);

                // Optional filter by Context_Type substitution variable
                var contextType = args.NameValuePairs.XFGetValue("Context_Type", string.Empty);
                if (!string.IsNullOrWhiteSpace(contextType))
                    return sqa.Get_DVM_Configs_By_Context(si, contextType);

                return sqa.Get_Active_DVM_Configs(si);
            }
        }

        private DataTable Get_DVM_Config_By_ID()
        {
            var dvmConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("DVM_Config_ID", "0"));

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return sqa.Get_DVM_Config_By_ID(si, dvmConfigID);
            }
        }

        // ------------------------------------------------------------------
        // DVM_Rule queries
        // ------------------------------------------------------------------

        private DataTable Get_DVM_Rules()
        {
            var dvmConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("DVM_Config_ID", "0"));
            var activeOnly  = !string.Equals(args.NameValuePairs.XFGetValue("All", "0"), "1", StringComparison.Ordinal);

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return activeOnly
                    ? sqa.Get_Rules_By_Config(si, dvmConfigID)
                    : sqa.Get_All_Rules_By_Config(si, dvmConfigID);
            }
        }

        // ------------------------------------------------------------------
        // Reference / lookup tables
        // ------------------------------------------------------------------

        private DataTable Get_DVM_Rule_Types()
        {
            var dt = new DataTable("DVM_Rule_Types");
            dt.Columns.Add("Rule_Type",   typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("Operator",    typeof(string));

            dt.Rows.Add("Equality",           "Source equals Target",                                    "=");
            dt.Rows.Add("NotEqual",           "Source does not equal Target",                            "!=");
            dt.Rows.Add("LessThan",           "Source is less than Target",                              "<");
            dt.Rows.Add("LessThanOrEqual",    "Source is less than or equal to Target",                  "<=");
            dt.Rows.Add("GreaterThan",        "Source is greater than Target",                           ">");
            dt.Rows.Add("GreaterThanOrEqual", "Source is greater than or equal to Target",               ">=");
            dt.Rows.Add("PercentVariance",    "|Source – Target| / |Target| <= Tolerance %",             "~%");
            dt.Rows.Add("LessThanWithPct",    "Source < Target * (1 + Tolerance %) — e.g. row x < row y + 5%", "<%");
            dt.Rows.Add("GreaterThanWithPct", "Source > Target * (1 – Tolerance %) — e.g. row x > row y – 5%", ">%");

            return dt;
        }

        private DataTable Get_DVM_Severities()
        {
            var dt = new DataTable("DVM_Severities");
            dt.Columns.Add("Severity",    typeof(string));
            dt.Columns.Add("Description", typeof(string));

            dt.Rows.Add("Error",   "Treat failures as errors (blocks further processing)");
            dt.Rows.Add("Warning", "Treat failures as warnings (non-blocking)");
            dt.Rows.Add("Info",    "Informational only");

            return dt;
        }

        private DataTable Get_DVM_Context_Types()
        {
            var dt = new DataTable("DVM_Context_Types");
            dt.Columns.Add("Context_Type", typeof(string));
            dt.Columns.Add("Description",  typeof(string));

            dt.Rows.Add("Table", "Validate data within a SQL table or view");
            dt.Rows.Add("Cube",  "Validate data using OneStream cube FDX queries");

            return dt;
        }

        // ------------------------------------------------------------------
        // DVM_Run queries
        // ------------------------------------------------------------------

        private DataTable Get_DVM_Recent_Runs()
        {
            var topNStr = args.NameValuePairs.XFGetValue("Top_N", "50");
            var topN    = string.IsNullOrWhiteSpace(topNStr) ? 50 : Convert.ToInt32(topNStr);

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return sqa.Get_Recent_DVM_Runs(si, topN);
            }
        }

        private DataTable Get_DVM_Runs_By_Config()
        {
            var dvmConfigID = Convert.ToInt32(args.NameValuePairs.XFGetValue("DVM_Config_ID", "0"));
            var topNStr     = args.NameValuePairs.XFGetValue("Top_N", "50");
            var topN        = string.IsNullOrWhiteSpace(topNStr) ? 50 : Convert.ToInt32(topNStr);

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return sqa.Get_DVM_Runs_By_Config(si, dvmConfigID, topN);
            }
        }

        // ------------------------------------------------------------------
        // DVM_Result queries
        // ------------------------------------------------------------------

        private DataTable Get_DVM_Results()
        {
            var dvmRunID = Convert.ToInt32(args.NameValuePairs.XFGetValue("DVM_Run_ID", "0"));

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return sqa.Get_DVM_Results_By_Run(si, dvmRunID);
            }
        }

        private DataTable Get_DVM_Failures()
        {
            var dvmRunID = Convert.ToInt32(args.NameValuePairs.XFGetValue("DVM_Run_ID", "0"));

            using (var dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            using (var conn   = new SqlConnection(dbConn.ConnectionString))
            {
                conn.Open();
                var sqa = new SQA_DVM_Validation(si, conn);
                return sqa.Get_DVM_Failures_By_Run(si, dvmRunID);
            }
        }

        #endregion
    }
}
