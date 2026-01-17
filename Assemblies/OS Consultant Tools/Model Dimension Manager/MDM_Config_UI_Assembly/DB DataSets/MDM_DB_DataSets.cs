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
							case "Get_CDC_Config":
							{
								return Get_CDC_Config();
							}
							case "Get_CDC_Config_Detail":
							{
								return Get_CDC_Config_Detail();
							}
							case "Get_Member_Properties":
							{
								return Get_Member_Properties();
							}
							case "Get_Validation_Config":
							{
								return Get_Validation_Config();
							}
							case "Get_Validation_Types":
							{
								return Get_Validation_Types();
							}
							case "Get_Validation_Severities":
							{
								return Get_Validation_Severities();
							}
							case "Get_Validation_Results":
							{
								return Get_Validation_Results();
							}
							case "Get_Validation_Runs":
							{
								return Get_Validation_Runs();
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
                                    Name,
                                    Dim_Type,
                                    Dim_ID,
                                    Src_Connection,
                                    Src_SQL_String,
                                    Dim_Mgmt_Process,
                                    Trx_Rule,
                                    Appr_ID,
                                    Mbr_PrefSuff,
                                    Mbr_PrefSuff_Txt,
                                    Create_Date,
                                    Create_User,
                                    Update_Date,
                                    Update_User
                                FROM MDM_CDC_Config
                                ORDER BY Dim_Type, Dim_ID";
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

        private DataTable Get_CDC_Config_Detail()
        {
            try
            {
                var cdcConfigID = args.NameValuePairs.XFGetValue("cdcConfigID", "NA");
                var dt = new DataTable("CDC_Config_Detail");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT 
                                    CDC_Config_ID,
                                    CDC_Config_Detail_ID,
                                    OS_Mbr_Column,
                                    OS_Mbr_Vary_Scen_Column,
                                    OS_Mbr_Vary_Time_Column,
                                    Src_Mbr_Column,
                                    Src_Vary_Scen_Column,
                                    Src_Vary_Time_Column,
                                    Create_Date,
                                    Create_User,
                                    Update_Date,
                                    Update_User
                                FROM MDM_CDC_Config_Detail";
                    
                    SqlParameter[] sqlparams;
                    if (cdcConfigID != "NA")
                    {
                        sql += " WHERE CDC_Config_ID = @CDC_Config_ID";
                        sqlparams = new SqlParameter[]
                        {
                            new SqlParameter("@CDC_Config_ID", SqlDbType.Int) { Value = cdcConfigID.XFConvertToInt() }
                        };
                    }
                    else
                    {
                        sqlparams = new SqlParameter[] { };
                    }
                    
                    sql += " ORDER BY CDC_Config_ID, CDC_Config_Detail_ID";
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
                
                // Standard member properties that can be mapped
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

        private DataTable Get_Validation_Config()
        {
            try
            {
                var dt = new DataTable("Validation_Config");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT 
                                    Validation_Config_ID,
                                    Name,
                                    Description,
                                    Dim_Type,
                                    Dim_ID,
                                    Validation_Type,
                                    Is_Active,
                                    Severity,
                                    Config_JSON,
                                    Create_Date,
                                    Create_User,
                                    Update_Date,
                                    Update_User
                                FROM MDM_Validation_Config
                                ORDER BY Dim_Type, Dim_ID, Name";
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

        private DataTable Get_Validation_Types()
        {
            try
            {
                var dt = new DataTable("Validation_Types");
                dt.Columns.Add("TypeName", typeof(string));
                dt.Columns.Add("TypeDisplayName", typeof(string));
                dt.Columns.Add("Description", typeof(string));
                
                // Validation types from schema
                dt.Rows.Add("MissingInHierarchy", "Missing in Hierarchy", "Ensures all members appear in specified hierarchies");
                dt.Rows.Add("MissingTextProperty", "Missing Text Property", "Checks if required text properties are populated");
                dt.Rows.Add("InvalidTextPropertyValue", "Invalid Text Property Value", "Validates text property values against a data source or list");
                dt.Rows.Add("DuplicateMembers", "Duplicate Members", "Identifies duplicate member names or descriptions");
                dt.Rows.Add("OrphanedMembers", "Orphaned Members", "Identifies members with no valid parent relationships");
                dt.Rows.Add("CircularReferences", "Circular References", "Detects circular parent-child relationships");
                dt.Rows.Add("InvalidParentChild", "Invalid Parent-Child", "Validates parent-child relationships against business rules");
                dt.Rows.Add("MissingRequiredProperty", "Missing Required Property", "Checks for missing required properties");
                dt.Rows.Add("CustomSQL", "Custom SQL", "Executes custom SQL to validate members");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Validation_Severities()
        {
            try
            {
                var dt = new DataTable("Validation_Severities");
                dt.Columns.Add("SeverityName", typeof(string));
                dt.Columns.Add("SeverityDisplayName", typeof(string));
                
                // Severity levels from schema
                dt.Rows.Add("Error", "Error");
                dt.Rows.Add("Warning", "Warning");
                dt.Rows.Add("Info", "Info");
                
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Validation_Results()
        {
            try
            {
                var runID = args.NameValuePairs.XFGetValue("runID", "NA");
                var validationConfigID = args.NameValuePairs.XFGetValue("validationConfigID", "NA");
                
                var dt = new DataTable("Validation_Results");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT 
                                    vr.Validation_Result_ID,
                                    vr.Validation_Config_ID,
                                    vc.Name as Validation_Name,
                                    vc.Validation_Type,
                                    vc.Severity,
                                    vr.Run_ID,
                                    vr.Run_Date,
                                    vr.Run_User,
                                    vr.Member_Name,
                                    vr.Member_ID,
                                    vr.Validation_Status,
                                    vr.Error_Message,
                                    vr.Error_Details,
                                    vr.Create_Date
                                FROM MDM_Validation_Result vr
                                INNER JOIN MDM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID";
                    
                    List<SqlParameter> sqlparams = new List<SqlParameter>();
                    List<string> whereClauses = new List<string>();
                    
                    if (runID != "NA")
                    {
                        whereClauses.Add("vr.Run_ID = @Run_ID");
                        sqlparams.Add(new SqlParameter("@Run_ID", SqlDbType.Int) { Value = runID.XFConvertToInt() });
                    }
                    
                    if (validationConfigID != "NA")
                    {
                        whereClauses.Add("vr.Validation_Config_ID = @Validation_Config_ID");
                        sqlparams.Add(new SqlParameter("@Validation_Config_ID", SqlDbType.Int) { Value = validationConfigID.XFConvertToInt() });
                    }
                    
                    if (whereClauses.Count > 0)
                    {
                        sql += " WHERE " + string.Join(" AND ", whereClauses);
                    }
                    
                    sql += " ORDER BY vc.Severity DESC, vr.Member_Name";
                    
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams.ToArray());
                }
                return dt;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        private DataTable Get_Validation_Runs()
        {
            try
            {
                var dimType = args.NameValuePairs.XFGetValue("dimType", "NA");
                var dimID = args.NameValuePairs.XFGetValue("dimID", "NA");
                var topN = args.NameValuePairs.XFGetValue("topN", "50");
                
                var dt = new DataTable("Validation_Runs");
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    var sql_gbl_get_datasets = new GBL_UI_Assembly.SQL_GBL_Get_DataSets(si, connection);
                    var sqa = new SqlDataAdapter();
                    var sql = @"SELECT TOP (@TopN)
                                    Run_ID,
                                    Run_Date,
                                    Run_User,
                                    Dim_Type,
                                    Dim_ID,
                                    Total_Validations,
                                    Total_Members_Checked,
                                    Total_Failures,
                                    Total_Warnings,
                                    Execution_Time_Ms,
                                    Status,
                                    Notes,
                                    Create_Date
                                FROM MDM_Validation_Run";
                    
                    List<SqlParameter> sqlparams = new List<SqlParameter>();
                    sqlparams.Add(new SqlParameter("@TopN", SqlDbType.Int) { Value = topN.XFConvertToInt() });
                    
                    List<string> whereClauses = new List<string>();
                    
                    if (dimType != "NA")
                    {
                        whereClauses.Add("Dim_Type = @Dim_Type");
                        sqlparams.Add(new SqlParameter("@Dim_Type", SqlDbType.NVarChar, 50) { Value = dimType });
                    }
                    
                    if (dimID != "NA")
                    {
                        whereClauses.Add("Dim_ID = @Dim_ID");
                        sqlparams.Add(new SqlParameter("@Dim_ID", SqlDbType.Int) { Value = dimID.XFConvertToInt() });
                    }
                    
                    if (whereClauses.Count > 0)
                    {
                        sql += " WHERE " + string.Join(" AND ", whereClauses);
                    }
                    
                    sql += " ORDER BY Run_Date DESC";
                    
                    sql_gbl_get_datasets.Fill_Get_GBL_DT(si, sqa, dt, sql, sqlparams.ToArray());
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