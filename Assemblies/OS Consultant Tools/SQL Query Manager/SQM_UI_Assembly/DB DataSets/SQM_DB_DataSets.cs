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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.SQM_DB_DataSets
{
    public class MainClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                switch (args.FunctionType)
                {
                    case DashboardDataSetFunctionType.GetDataSetNames:
                        var names = new List<string>();
                        names.Add("Run_SQL_Command");
                        return names;
                        break;
                    case DashboardDataSetFunctionType.GetDataSet:
                        if (args.DataSetName.XFEqualsIgnoreCase("Run_SQL_Command"))
                        {
                            return Run_SQL_Command(si, globals, api, args);
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
        private DataTable Run_SQL_Command(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
        {
            try
            {
                var txt_SQL_Search = args.NameValuePairs["txt_SQL_Search"];
                var Data_Source = args.NameValuePairs["Data_Source"];
                var SQL_DT = new DataTable("SQL_Results");
                if (txt_SQL_Search.Length == 0)
                    return SQL_DT;
                //If statement to get appropriate DBConnection Info
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_SQM_Get_DataSets = new SQL_SQM_Get_DataSets(si, connection);
                    // Create a new DataTable
                    SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
                    // Define the select query and parameters
                    string selectQuery = txt_SQL_Search;
                    // Create an array of SqlParameter objects
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                    };
                    sql_SQM_Get_DataSets.Fill_Get_SQM_DataTable(si, sqlDataAdapter, SQL_DT, selectQuery, parameters);

                }

                return SQL_DT;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #endregion
    }
}