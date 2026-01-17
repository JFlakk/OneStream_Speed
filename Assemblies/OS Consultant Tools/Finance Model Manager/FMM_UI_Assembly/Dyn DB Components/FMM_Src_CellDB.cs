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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class FMM_Src_CellDB
    {
        // Database table that will contain our objects
        public string TableName { get; } = "FMM_Src_Cell";

        // SessionInfo instance
        public SessionInfo si { get; }

        // constructor
        public FMM_Src_CellDB(SessionInfo si)
        {
            this.si = si;
        }

        /// <summary>
        /// Gets the list of column names that are necessary for a given CalcType
        /// by using the FMM_Config_Helpers.SrcRegistry
        /// </summary>
        /// <param name="calcType">The calculation type</param>
        /// <returns>List of database column names to include in queries</returns>
        public List<string> GetRequiredColumnsByCalcType(int calcType)
        {
            var columns = new List<string>();

            // Always include core identification and common fields
            columns.AddRange(new[] {
                "Cell_ID", "Cube_ID", "Act_ID", "Model_ID", "Calc_ID", "Src_Order",
                "Src_Type", "Src_Item", "Open_Parens", "Math_Operator", "Close_Parens",
                "Create_Date", "Create_User", "Update_Date", "Update_User"
            });

            var srcConfig = FMM_Config_Helpers.Get_SrcConfigType(calcType);

            if (srcConfig != null && srcConfig.ParameterMappings != null)
            {
                foreach (var mapping in srcConfig.ParameterMappings.Values)
                {
                    foreach (var dbColumn in mapping.Values)
                    {
                        // Map the logical column name to the actual database column name
                        if (!columns.Contains(dbColumn) && !string.IsNullOrEmpty(dbColumn))
                        {
                            columns.Add(dbColumn);
                        }
                    }
                }
            }

            return columns;
        }

        /// <summary>
        /// Builds a SELECT statement with only the necessary columns for the given CalcType
        /// </summary>
        /// <param name="calcType">The calculation type</param>
        /// <returns>Comma-separated list of columns for SELECT statement</returns>
        public string GetSelectColumnsForCalcType(int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            return string.Join(", ", columns);
        }



        /// <summary>
        /// Retrieve a single FMM_Src_CellModel by Cell_ID
        /// </summary>
        public FMM_Src_CellModel GetSrcCell(int cellId, int calcType)
        {
            try
            {
                var columns = GetSelectColumnsForCalcType(calcType);
                string sql = $"SELECT {columns} FROM {this.TableName} WHERE Cell_ID = @cellID";

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@cellID", cellId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    if (dt.Rows.Count < 1) return null;

                    DataRow dr = dt.Rows[0];
                    return MapDataRowToModel(dr, calcType);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Retrieve all FMM_Src_CellModel objects for a given Calc_ID
        /// </summary>
        public List<FMM_Src_CellModel> GetSrcCellsByCalcId(int calcId, int calcType)
        {
            try
            {
                var columns = GetSelectColumnsForCalcType(calcType);
                string sql = $"SELECT {columns} FROM {this.TableName} WHERE Calc_ID = @calcID ORDER BY Src_Order";

                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@calcID", calcId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    List<FMM_Src_CellModel> result = new List<FMM_Src_CellModel>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(MapDataRowToModel(dr, calcType));
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
        /// <summary>
        /// Merge a collection of source cells using the generic SQA process
        /// </summary>
        public void Merge(List<FMM_Src_CellModel> models, int calcType)
        {
            if (models == null || models.Count == 0) return;

            try
            {
                var mergeTable = BuildMergeTable(models, calcType);

                var calcId = models[0].Calc_ID;
                var selectColumns = GetSelectColumnsForCalcType(calcType);
                var currentSql = $"SELECT {selectColumns} FROM {this.TableName} WHERE Calc_ID = @calcID";

                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(this.si);

                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();
                    var cmdBuilder = new GBL_UI_Assembly.SQA_GBL_Command_Builder(this.si, connection);
                    var sqa = new SqlDataAdapter();
                    var currentTable = new DataTable();

                    var sqlparams = new[]
                    {
                        new SqlParameter("@calcID", SqlDbType.Int) { Value = calcId }
                    };

                    cmdBuilder.FillDataTable(this.si, sqa, currentTable, currentSql, sqlparams);

                    // Set primary key if Cell_ID column exists in the result set
                    if (currentTable.Columns.Contains("Cell_ID") && currentTable.Columns["Cell_ID"] != null)
                    {
                        currentTable.PrimaryKey = new[] { currentTable.Columns["Cell_ID"]! };
                    }

                    // Merge the new/updated records with existing data
                    // preserveChanges=false: Overwrite existing rows with new values (upsert behavior)
                    currentTable.Merge(mergeTable, false, MissingSchemaAction.Add);

                    cmdBuilder.UpdateTableSimple(this.si, this.TableName, currentTable, sqa, "Cell_ID");
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Merge a single source cell using the generic SQA process
        /// </summary>
        public void Merge(FMM_Src_CellModel model, int calcType)
        {
            this.Merge(new List<FMM_Src_CellModel> { model }, calcType);
        }

        private DataTable BuildMergeTable(IEnumerable<FMM_Src_CellModel> models, int calcType)
        {
            var columns = GetRequiredColumnsByCalcType(calcType);
            DataTable dt = new DataTable(this.TableName);

            foreach (var col in columns)
            {
                dt.Columns.Add(col, typeof(object));
            }

            foreach (var model in models)
            {
                DataRow row = dt.NewRow();

                SetColumnValue(row, "Cell_ID", model.Cell_ID);
                SetColumnValue(row, "Cube_ID", model.Cube_ID);
                SetColumnValue(row, "Act_ID", model.Act_ID);
                SetColumnValue(row, "Model_ID", model.Model_ID);
                SetColumnValue(row, "Calc_ID", model.Calc_ID);
                SetColumnValue(row, "Src_Order", model.Src_Order);
                SetColumnValue(row, "Src_Type", model.Src_Type);
                SetColumnValue(row, "Src_Item", model.Src_Item);
                SetColumnValue(row, "Open_Parens", model.Open_Parens);
                SetColumnValue(row, "Math_Operator", model.Math_Operator);
                SetColumnValue(row, "Close_Parens", model.Close_Parens);
                SetColumnValue(row, "Create_Date", model.Create_Date ?? DateTime.Now);
                SetColumnValue(row, "Create_User", model.Create_User ?? this.si?.UserName);
                SetColumnValue(row, "Update_Date", model.Update_Date ?? DateTime.Now);
                SetColumnValue(row, "Update_User", model.Update_User ?? this.si?.UserName);

                SetColumnValue(row, "Entity", model.Entity);
                SetColumnValue(row, "Cons", model.Cons);
                SetColumnValue(row, "Scenario", model.Scenario);
                SetColumnValue(row, "Time", model.Time);
                SetColumnValue(row, "View", model.View);
                SetColumnValue(row, "Acct", model.Acct);
                SetColumnValue(row, "IC", model.IC);
                SetColumnValue(row, "Origin", model.Origin);
                SetColumnValue(row, "Flow", model.Flow);
                SetColumnValue(row, "UD1", model.UD1);
                SetColumnValue(row, "UD2", model.UD2);
                SetColumnValue(row, "UD3", model.UD3);
                SetColumnValue(row, "UD4", model.UD4);
                SetColumnValue(row, "UD5", model.UD5);
                SetColumnValue(row, "UD6", model.UD6);
                SetColumnValue(row, "UD7", model.UD7);
                SetColumnValue(row, "UD8", model.UD8);

                SetColumnValue(row, "DB_Name", model.DB_Name);
                SetColumnValue(row, "Dyn_Calc_Script", model.Dyn_Calc_Script);
                SetColumnValue(row, "Unbal_Src_Cell_Buffer", model.Unbal_Src_Cell_Buffer);
                SetColumnValue(row, "Unbal_Src_Cell_Buffer_Filter", model.Unbal_Src_Cell_Buffer_Filter);
                SetColumnValue(row, "Unbal_Buffer_Filter", model.Unbal_Buffer_Filter);
                SetColumnValue(row, "Override_Value", model.Override_Value);

                SetColumnValue(row, "Unbal_Acct_Override", model.Unbal_Acct_Override);
                SetColumnValue(row, "Unbal_Origin_Override", model.Unbal_Origin_Override);
                SetColumnValue(row, "Unbal_Flow_Override", model.Unbal_Flow_Override);
                SetColumnValue(row, "Unbal_IC_Override", model.Unbal_IC_Override);
                SetColumnValue(row, "Unbal_UD1_Override", model.Unbal_UD1_Override);
                SetColumnValue(row, "Unbal_UD2_Override", model.Unbal_UD2_Override);
                SetColumnValue(row, "Unbal_UD3_Override", model.Unbal_UD3_Override);
                SetColumnValue(row, "Unbal_UD4_Override", model.Unbal_UD4_Override);
                SetColumnValue(row, "Unbal_UD5_Override", model.Unbal_UD5_Override);
                SetColumnValue(row, "Unbal_UD6_Override", model.Unbal_UD6_Override);
                SetColumnValue(row, "Unbal_UD7_Override", model.Unbal_UD7_Override);
                SetColumnValue(row, "Unbal_UD8_Override", model.Unbal_UD8_Override);

                dt.Rows.Add(row);
            }

            return dt;
        }

        private void SetColumnValue(DataRow row, string columnName, object value)
        {
            if (!row.Table.Columns.Contains(columnName)) return;
            row[columnName] = value ?? DBNull.Value;
        }
        /// <summary>
        /// Retrieve all FMM_Src_CellModel objects
        /// Note: Since CalcType may vary, this returns all columns
        /// </summary>
        public List<FMM_Src_CellModel> GetAllSrcCells()
        {
            try
            {
                string sql = $"SELECT * FROM {this.TableName} ORDER BY Calc_ID, Src_Order";

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, false);
                    List<FMM_Src_CellModel> result = new List<FMM_Src_CellModel>();

                    foreach (DataRow dr in dt.Rows)
                    {
                        result.Add(MapDataRowToModel(dr));
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Maps a DataRow to FMM_Src_CellModel, safely handling null values
        /// Overload without calcType - uses default parameterless constructor
        /// </summary>
        private FMM_Src_CellModel MapDataRowToModel(DataRow dr)
        {
            return MapDataRowToModel(dr, 0); // 0 = CalcType.None, will use empty enabled properties list
        }

        /// <summary>
        /// Maps a DataRow to FMM_Src_CellModel, safely handling null values
        /// </summary>
        private FMM_Src_CellModel MapDataRowToModel(DataRow dr, int calcType)
        {
            // Get enabled properties from configuration
            var helpers = new FMM_Config_Helpers();
            var enabledProperties = helpers.GetEnabledSrcProperties(calcType);

            var model = new FMM_Src_CellModel(enabledProperties)
            {
                Cell_ID = dr.Field<int>("Cell_ID"),
                Cube_ID = dr.Field<int>("Cube_ID"),
                Act_ID = dr.Field<int>("Act_ID"),
                Model_ID = dr.Field<int>("Model_ID"),
                Calc_ID = dr.Field<int>("Calc_ID"),
                Src_Order = dr.Field<int?>("Src_Order") ?? 0,
                Src_Type = dr.Field<string>("Src_Type") ?? string.Empty,
                Src_Item = dr.Field<string>("Src_Item") ?? string.Empty,
                Open_Parens = dr.Field<string>("Open_Parens") ?? string.Empty,
                Math_Operator = dr.Field<string>("Math_Operator") ?? string.Empty,
                Close_Parens = dr.Field<string>("Close_Parens") ?? string.Empty,

                // Audit fields
                Create_Date = dr.Field<DateTime?>("Create_Date"),
                Create_User = dr.Field<string>("Create_User"),
                Update_Date = dr.Field<DateTime?>("Update_Date"),
                Update_User = dr.Field<string>("Update_User")
            };

            // Set dimension fields only if they exist in the column set and are enabled
            if (enabledProperties.Contains("Entity") && dr.Table.Columns.Contains("Entity"))
                model.Entity = dr.Field<string>("Entity");
            if (enabledProperties.Contains("Cons") && dr.Table.Columns.Contains("Cons"))
                model.Cons = dr.Field<string>("Cons");
            if (enabledProperties.Contains("Scenario") && dr.Table.Columns.Contains("Scenario"))
                model.Scenario = dr.Field<string>("Scenario");
            if (enabledProperties.Contains("Time") && dr.Table.Columns.Contains("Time"))
                model.Time = dr.Field<string>("Time");
            if (enabledProperties.Contains("View") && dr.Table.Columns.Contains("View"))
                model.View = dr.Field<string>("View");
            if (enabledProperties.Contains("Acct") && dr.Table.Columns.Contains("Acct"))
                model.Acct = dr.Field<string>("Acct");
            if (enabledProperties.Contains("IC") && dr.Table.Columns.Contains("IC"))
                model.IC = dr.Field<string>("IC");
            if (enabledProperties.Contains("Origin") && dr.Table.Columns.Contains("Origin"))
                model.Origin = dr.Field<string>("Origin");
            if (enabledProperties.Contains("Flow") && dr.Table.Columns.Contains("Flow"))
                model.Flow = dr.Field<string>("Flow");
            if (enabledProperties.Contains("UD1") && dr.Table.Columns.Contains("UD1"))
                model.UD1 = dr.Field<string>("UD1");
            if (enabledProperties.Contains("UD2") && dr.Table.Columns.Contains("UD2"))
                model.UD2 = dr.Field<string>("UD2");
            if (enabledProperties.Contains("UD3") && dr.Table.Columns.Contains("UD3"))
                model.UD3 = dr.Field<string>("UD3");
            if (enabledProperties.Contains("UD4") && dr.Table.Columns.Contains("UD4"))
                model.UD4 = dr.Field<string>("UD4");
            if (enabledProperties.Contains("UD5") && dr.Table.Columns.Contains("UD5"))
                model.UD5 = dr.Field<string>("UD5");
            if (enabledProperties.Contains("UD6") && dr.Table.Columns.Contains("UD6"))
                model.UD6 = dr.Field<string>("UD6");
            if (enabledProperties.Contains("UD7") && dr.Table.Columns.Contains("UD7"))
                model.UD7 = dr.Field<string>("UD7");
            if (enabledProperties.Contains("UD8") && dr.Table.Columns.Contains("UD8"))
                model.UD8 = dr.Field<string>("UD8");

            // Additional fields - only set if enabled
            if (enabledProperties.Contains("DB_Name") && dr.Table.Columns.Contains("DB_Name"))
                model.DB_Name = dr.Field<string>("DB_Name");
            if (enabledProperties.Contains("Dyn_Calc_Script") && dr.Table.Columns.Contains("Dyn_Calc_Script"))
                model.Dyn_Calc_Script = dr.Field<string>("Dyn_Calc_Script");
            if (enabledProperties.Contains("Unbal_Src_Cell_Buffer") && dr.Table.Columns.Contains("Unbal_Src_Cell_Buffer"))
                model.Unbal_Src_Cell_Buffer = dr.Field<string>("Unbal_Src_Cell_Buffer");
            if (enabledProperties.Contains("Unbal_Src_Cell_Buffer_Filter") && dr.Table.Columns.Contains("Unbal_Src_Cell_Buffer_Filter"))
                model.Unbal_Src_Cell_Buffer_Filter = dr.Field<string>("Unbal_Src_Cell_Buffer_Filter");
            if (enabledProperties.Contains("Unbal_Buffer_Filter") && dr.Table.Columns.Contains("Unbal_Buffer_Filter"))
                model.Unbal_Buffer_Filter = dr.Field<string>("Unbal_Buffer_Filter");
            if (enabledProperties.Contains("Override_Value") && dr.Table.Columns.Contains("Override_Value"))
                model.Override_Value = dr.Field<string>("Override_Value");

            // Unbalanced dimension overrides - only set if enabled
            if (enabledProperties.Contains("Unbal_Acct_Override") && dr.Table.Columns.Contains("Unbal_Acct_Override"))
                model.Unbal_Acct_Override = dr.Field<string>("Unbal_Acct_Override");
            if (enabledProperties.Contains("Unbal_Origin_Override") && dr.Table.Columns.Contains("Unbal_Origin_Override"))
                model.Unbal_Origin_Override = dr.Field<string>("Unbal_Origin_Override");
            if (enabledProperties.Contains("Unbal_Flow_Override") && dr.Table.Columns.Contains("Unbal_Flow_Override"))
                model.Unbal_Flow_Override = dr.Field<string>("Unbal_Flow_Override");
            if (enabledProperties.Contains("Unbal_IC_Override") && dr.Table.Columns.Contains("Unbal_IC_Override"))
                model.Unbal_IC_Override = dr.Field<string>("Unbal_IC_Override");
            if (enabledProperties.Contains("Unbal_UD1_Override") && dr.Table.Columns.Contains("Unbal_UD1_Override"))
                model.Unbal_UD1_Override = dr.Field<string>("Unbal_UD1_Override");
            if (enabledProperties.Contains("Unbal_UD2_Override") && dr.Table.Columns.Contains("Unbal_UD2_Override"))
                model.Unbal_UD2_Override = dr.Field<string>("Unbal_UD2_Override");
            if (enabledProperties.Contains("Unbal_UD3_Override") && dr.Table.Columns.Contains("Unbal_UD3_Override"))
                model.Unbal_UD3_Override = dr.Field<string>("Unbal_UD3_Override");
            if (enabledProperties.Contains("Unbal_UD4_Override") && dr.Table.Columns.Contains("Unbal_UD4_Override"))
                model.Unbal_UD4_Override = dr.Field<string>("Unbal_UD4_Override");
            if (enabledProperties.Contains("Unbal_UD5_Override") && dr.Table.Columns.Contains("Unbal_UD5_Override"))
                model.Unbal_UD5_Override = dr.Field<string>("Unbal_UD5_Override");
            if (enabledProperties.Contains("Unbal_UD6_Override") && dr.Table.Columns.Contains("Unbal_UD6_Override"))
                model.Unbal_UD6_Override = dr.Field<string>("Unbal_UD6_Override");
            if (enabledProperties.Contains("Unbal_UD7_Override") && dr.Table.Columns.Contains("Unbal_UD7_Override"))
                model.Unbal_UD7_Override = dr.Field<string>("Unbal_UD7_Override");
            if (enabledProperties.Contains("Unbal_UD8_Override") && dr.Table.Columns.Contains("Unbal_UD8_Override"))
                model.Unbal_UD8_Override = dr.Field<string>("Unbal_UD8_Override");

            return model;
        }

        /// <summary>
        /// Delete a source cell record by Cell_ID
        /// </summary>
        public void Delete(int cellId)
        {
            try
            {
                string sql = $"DELETE FROM {this.TableName} WHERE Cell_ID = @cellID";
                List<DbParamInfo> paramList = new List<DbParamInfo> { new DbParamInfo("@cellID", cellId) };

                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.si))
                {
                    BRApi.Database.ExecuteActionQuery(dbConn, sql, paramList, false, true);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        /// <summary>
        /// Delete a source cell record by model instance
        /// </summary>
        public void Delete(FMM_Src_CellModel model)
        {
            this.Delete(model.Cell_ID);
        }
    }
}
