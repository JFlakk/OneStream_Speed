using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.IO;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DDM_Config_Migration
{
    public class MainClass
    {
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                string functionName = args.FunctionName;
                
                switch (functionName.XFEqualsIgnoreCase())
                {
                    case "EXPORT_CONFIG":
                        return ExportConfig(si);

                    case "IMPORT_CONFIG":
                        string fileGUID = args.NameValuePairs.XFGetValue("FileID");
                        return ImportConfig(si, fileGUID);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region Export Logic
        private XFSelectionChangedTaskResult ExportConfig(SessionInfo si)
        {
            // 1. Create a DataSet to hold our 3 tables
            DataSet ds = new DataSet("DDM_Export");
            DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);

            // 2. Fetch Data for Table 1: DDM_Config
            using (DataTable dt = BRApi.Database.ExecuteSql(dbConn, "SELECT * FROM DDM_Config", false))
            {
                dt.TableName = "DDM_Config";
                ds.Tables.Add(dt.Copy());
            }

            // 3. Fetch Data for Table 2: DDM_Config_Menu_Layout
            using (DataTable dt = BRApi.Database.ExecuteSql(dbConn, "SELECT * FROM DDM_Config_Menu_Layout", false))
            {
                dt.TableName = "DDM_Config_Menu_Layout";
                ds.Tables.Add(dt.Copy());
            }

            // 4. Fetch Data for Table 3: DDM_Config_Hdr_Ctrls
            using (DataTable dt = BRApi.Database.ExecuteSql(dbConn, "SELECT * FROM DDM_Config_Hdr_Ctrls", false))
            {
                dt.TableName = "DDM_Config_Hdr_Ctrls";
                ds.Tables.Add(dt.Copy());
            }

            // 5. Convert DataSet to XML String
            string xmlContent = ds.GetXml();

            // 6. Return File to User
            string fileName = $"DDM_Migration_{DateTime.Now:yyyyMMdd_HHmm}.xml";
            XFFileContent fileContent = new XFFileContent(fileName, Encoding.UTF8.GetBytes(xmlContent));

            XFSelectionChangedTaskResult result = new XFSelectionChangedTaskResult();
            result.TaskType = XFSelectionChangedTaskResultType.DownloadFile;
            result.FileContent = fileContent;
            result.Message = "Export generated successfully.";

            return result;
        }
        #endregion

        #region Import Logic
        private XFSelectionChangedTaskResult ImportConfig(SessionInfo si, string fileGUID)
        {
            if (string.IsNullOrEmpty(fileGUID)) 
                throw new XFException(si, "No file selected.");

            // 1. Read File
            XFFile file = BRApi.FileSystem.GetFile(si, Guid.Parse(fileGUID));
            string xmlData = Encoding.UTF8.GetString(file.FileBytes);

            // 2. Load XML back into DataSet
            DataSet ds = new DataSet();
            using (StringReader sr = new StringReader(xmlData))
            {
                ds.ReadXml(sr);
            }

            // 3. Process Import in Transaction
            DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
            
            using (DbConnection conn = BRApi.Database.CreateDbConnection(dbConn))
            {
                conn.Open();
                using (DbTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        if(ds.Tables.Contains("DDM_Config"))
                            ProcessTableImport(conn, trans, ds.Tables["DDM_Config"], "DDM_Config_ID");

                        if (ds.Tables.Contains("DDM_Config_Menu_Layout"))
                            ProcessTableImport(conn, trans, ds.Tables["DDM_Config_Menu_Layout"], "DDM_Menu_ID");

                        if (ds.Tables.Contains("DDM_Config_Hdr_Ctrls"))
                            ProcessTableImport(conn, trans, ds.Tables["DDM_Config_Hdr_Ctrls"], "DDM_Hdr_Ctrl_ID");

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }

            XFSelectionChangedTaskResult result = new XFSelectionChangedTaskResult();
            result.Message = "Configuration Imported Successfully.";
            result.IsOK = true;
            return result;
        }

        private void ProcessTableImport(DbConnection conn, DbTransaction trans, DataTable dt, string pkColumnName)
        {
            foreach (DataRow row in dt.Rows)
            {
                // 1. Check if record exists
                object pkValue = row[pkColumnName];
                
                string checkSql = $"SELECT COUNT(1) FROM [{dt.TableName}] WHERE [{pkColumnName}] = @PK";
                List<DbParam> checkParams = new List<DbParam> { new DbParam("@PK", pkValue) };

                object existsObj = BRApi.Database.ExecuteScalar(conn, trans, CommandType.Text, checkSql, checkParams);
                int count = Convert.ToInt32(existsObj);

                List<DbParam> upsertParams = new List<DbParam>();
                StringBuilder sql = new StringBuilder();

                if (count > 0)
                {
                    // === UPDATE ===
                    sql.Append($"UPDATE [{dt.TableName}] SET ");
                    
                    bool isFirst = true;
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.ColumnName.XFEqualsIgnoreCase(pkColumnName)) continue;

                        if (!isFirst) sql.Append(", ");
                        sql.Append($"[{col.ColumnName}] = @{col.ColumnName}");
                        
                        object val = row[col];
                        upsertParams.Add(new DbParam($"@{col.ColumnName}", 
                            (val == null || val == DBNull.Value) ? DBNull.Value : val));

                        isFirst = false;
                    }
                    
                    sql.Append($" WHERE [{pkColumnName}] = @PK_Where");
                    upsertParams.Add(new DbParam("@PK_Where", pkValue));
                }
                else
                {
                    // === INSERT ===
                    sql.Append($"INSERT INTO [{dt.TableName}] (");
                    sql.Append(string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]")));
                    sql.Append(") VALUES (");
                    sql.Append(string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => $"@{c.ColumnName}")));
                    sql.Append(")");

                    foreach (DataColumn col in dt.Columns)
                    {
                        object val = row[col];
                        upsertParams.Add(new DbParam($"@{col.ColumnName}", 
                            (val == null || val == DBNull.Value) ? DBNull.Value : val));
                    }
                }

                BRApi.Database.ExecuteNonQuery(conn, trans, CommandType.Text, sql.ToString(), upsertParams);
            }
        }
        #endregion
    }
}
