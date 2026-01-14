using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
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
        public SessionInfo SI { get; }

        // constructor
        public FMM_Src_CellDB(SessionInfo si)
        {
            this.SI = si;
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

            var helpers = new FMM_Config_Helpers();
            var srcConfig = helpers.Get_SrcConfigType(calcType);
            
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
                
                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
                    if (dt.Rows.Count < 1) return null;
                    
                    DataRow dr = dt.Rows[0];
                    return MapDataRowToModel(dr);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(SI, ex);
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
                
                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConn, sql, paramList, false);
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
                throw new XFException(SI, ex);
            }
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
                
                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
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
                throw new XFException(SI, ex);
            }
        }

        /// <summary>
        /// Maps a DataRow to FMM_Src_CellModel, safely handling null values
        /// </summary>
        private FMM_Src_CellModel MapDataRowToModel(DataRow dr)
        {
            return new FMM_Src_CellModel
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
                
                // Dimension fields - only present if column exists
                Entity = dr.Table.Columns.Contains("Entity") ? dr.Field<string>("Entity") : null,
                Cons = dr.Table.Columns.Contains("Cons") ? dr.Field<string>("Cons") : null,
                Scenario = dr.Table.Columns.Contains("Scenario") ? dr.Field<string>("Scenario") : null,
                Time = dr.Table.Columns.Contains("Time") ? dr.Field<string>("Time") : null,
                View = dr.Table.Columns.Contains("View") ? dr.Field<string>("View") : null,
                Acct = dr.Table.Columns.Contains("Acct") ? dr.Field<string>("Acct") : null,
                IC = dr.Table.Columns.Contains("IC") ? dr.Field<string>("IC") : null,
                Origin = dr.Table.Columns.Contains("Origin") ? dr.Field<string>("Origin") : null,
                Flow = dr.Table.Columns.Contains("Flow") ? dr.Field<string>("Flow") : null,
                UD1 = dr.Table.Columns.Contains("UD1") ? dr.Field<string>("UD1") : null,
                UD2 = dr.Table.Columns.Contains("UD2") ? dr.Field<string>("UD2") : null,
                UD3 = dr.Table.Columns.Contains("UD3") ? dr.Field<string>("UD3") : null,
                UD4 = dr.Table.Columns.Contains("UD4") ? dr.Field<string>("UD4") : null,
                UD5 = dr.Table.Columns.Contains("UD5") ? dr.Field<string>("UD5") : null,
                UD6 = dr.Table.Columns.Contains("UD6") ? dr.Field<string>("UD6") : null,
                UD7 = dr.Table.Columns.Contains("UD7") ? dr.Field<string>("UD7") : null,
                UD8 = dr.Table.Columns.Contains("UD8") ? dr.Field<string>("UD8") : null,
                
                // Additional fields
                DB_Name = dr.Table.Columns.Contains("DB_Name") ? dr.Field<string>("DB_Name") : null,
                Dyn_Calc_Script = dr.Table.Columns.Contains("Dyn_Calc_Script") ? dr.Field<string>("Dyn_Calc_Script") : null,
                Unbal_Src_Cell_Buffer = dr.Table.Columns.Contains("Unbal_Src_Cell_Buffer") ? dr.Field<string>("Unbal_Src_Cell_Buffer") : null,
                Unbal_Src_Cell_Buffer_Filter = dr.Table.Columns.Contains("Unbal_Src_Cell_Buffer_Filter") ? dr.Field<string>("Unbal_Src_Cell_Buffer_Filter") : null,
                Unbal_Buffer_Filter = dr.Table.Columns.Contains("Unbal_Buffer_Filter") ? dr.Field<string>("Unbal_Buffer_Filter") : null,
                Override_Value = dr.Table.Columns.Contains("Override_Value") ? dr.Field<string>("Override_Value") : null,
                
                // Unbalanced dimension overrides
                Unbal_Acct_Override = dr.Table.Columns.Contains("Unbal_Acct_Override") ? dr.Field<string>("Unbal_Acct_Override") : null,
                Unbal_Origin_Override = dr.Table.Columns.Contains("Unbal_Origin_Override") ? dr.Field<string>("Unbal_Origin_Override") : null,
                Unbal_Flow_Override = dr.Table.Columns.Contains("Unbal_Flow_Override") ? dr.Field<string>("Unbal_Flow_Override") : null,
                Unbal_IC_Override = dr.Table.Columns.Contains("Unbal_IC_Override") ? dr.Field<string>("Unbal_IC_Override") : null,
                Unbal_UD1_Override = dr.Table.Columns.Contains("Unbal_UD1_Override") ? dr.Field<string>("Unbal_UD1_Override") : null,
                Unbal_UD2_Override = dr.Table.Columns.Contains("Unbal_UD2_Override") ? dr.Field<string>("Unbal_UD2_Override") : null,
                Unbal_UD3_Override = dr.Table.Columns.Contains("Unbal_UD3_Override") ? dr.Field<string>("Unbal_UD3_Override") : null,
                Unbal_UD4_Override = dr.Table.Columns.Contains("Unbal_UD4_Override") ? dr.Field<string>("Unbal_UD4_Override") : null,
                Unbal_UD5_Override = dr.Table.Columns.Contains("Unbal_UD5_Override") ? dr.Field<string>("Unbal_UD5_Override") : null,
                Unbal_UD6_Override = dr.Table.Columns.Contains("Unbal_UD6_Override") ? dr.Field<string>("Unbal_UD6_Override") : null,
                Unbal_UD7_Override = dr.Table.Columns.Contains("Unbal_UD7_Override") ? dr.Field<string>("Unbal_UD7_Override") : null,
                Unbal_UD8_Override = dr.Table.Columns.Contains("Unbal_UD8_Override") ? dr.Field<string>("Unbal_UD8_Override") : null,
                
                // Audit fields
                Create_Date = dr.Field<DateTime?>("Create_Date"),
                Create_User = dr.Field<string>("Create_User"),
                Update_Date = dr.Field<DateTime?>("Update_Date"),
                Update_User = dr.Field<string>("Update_User")
            };
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
                
                using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(this.SI))
                {
                    BRApi.Database.ExecuteActionQuery(dbConn, sql, paramList, false, true);
                }
            }
            catch (Exception ex)
            {
                throw new XFException(SI, ex);
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
