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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.MDM_DB_DataSets
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
						switch (args.DataSetName)
						{
							case "Get_DimList":
							{
								return Get_DimList();
							}
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
        private DataTable Get_DimList()
        {
            try
            {
				var dimTypeID = args.NameValuePairs.XFGetValue("dimTypeID", "NA");
                var dt = new DataTable("Dimension");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    // Create a new DataTable
                    var sqa = new SqlDataAdapter();
                    // Define the select query and sqlparams
                    var sql = @"SELECT DimID, Name
                                FROM Dim
                                WHERE DimTypeId = @DimTypeID";
                    // Create an array of SqlParameter objects
                    var sqlparams = new SqlParameter[]
                    {
						new SqlParameter("@DimTypeID", SqlDbType.Int) { Value = dimTypeID.XFConvertToInt() },
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