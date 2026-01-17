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
							case "Get_DimTypeList":
							{
								return Get_DimTypeList();
							}
							case "Get_CDC_Source_Types":
							{
								return Get_CDC_Source_Types();
							}
							case "Get_CDC_Config":
							{
								return Get_CDC_Config();
							}
							case "Get_Member_Properties":
							{
								return Get_Member_Properties();
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

        private DataTable Get_DimTypeList()
        {
            try
            {
                var dt = new DataTable("DimType");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT DimTypeID, Name
                                FROM DimType
                                ORDER BY Name";
                    var sqlparams = new SqlParameter[] { };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_CDC_Source_Types()
        {
            try
            {
                var dt = new DataTable("CDC_Source_Types");
                dt.Columns.Add("SourceTypeID", typeof(int));
                dt.Columns.Add("SourceTypeName", typeof(string));
                
                dt.Rows.Add(1, "SQL");
                dt.Rows.Add(2, "API");
                dt.Rows.Add(3, "Flat File");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_CDC_Config()
        {
            try
            {
                var dt = new DataTable("CDC_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT 
                                    CDC_Config_ID,
                                    DimTypeID,
                                    DimID,
                                    SourceType,
                                    SourceConfig,
                                    Map_Name,
                                    Map_Description,
                                    Map_Text1,
                                    Map_Text2,
                                    Map_Text3,
                                    Map_Text4,
                                    Map_Text5,
                                    Map_Text6,
                                    Map_Text7,
                                    Map_Text8,
                                    Create_Date,
                                    Create_User,
                                    Modify_Date,
                                    Modify_User
                                FROM MDM_CDC_Config
                                ORDER BY DimTypeID, DimID";
                    var sqlparams = new SqlParameter[] { };
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams);
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Member_Properties()
        {
            try
            {
                var dt = new DataTable("Member_Properties");
                dt.Columns.Add("PropertyName", typeof(string));
                dt.Columns.Add("PropertyDisplayName", typeof(string));
                
                dt.Rows.Add("Name", "Name");
                dt.Rows.Add("Description", "Description");
                dt.Rows.Add("Text1", "Text1");
                dt.Rows.Add("Text2", "Text2");
                dt.Rows.Add("Text3", "Text3");
                dt.Rows.Add("Text4", "Text4");
                dt.Rows.Add("Text5", "Text5");
                dt.Rows.Add("Text6", "Text6");
                dt.Rows.Add("Text7", "Text7");
                dt.Rows.Add("Text8", "Text8");
                
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