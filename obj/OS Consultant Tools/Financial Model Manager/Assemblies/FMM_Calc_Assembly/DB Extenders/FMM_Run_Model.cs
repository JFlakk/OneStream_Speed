﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
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
using OpenXmlPowerTools;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.FMM_Process_Calc
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardExtenderArgs args;
        private StringBuilder debugString;
        private Dictionary<string, DataTable> outputTables = new Dictionary<string, DataTable>();
        #endregion

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                return "Test";
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public object Process_TableModel()
        {
            var new_Model_ID = 0;
            var sql_DataSet_DataAdapter = new SqlDataAdapter();
            var FMM_calc_Config_DT = new DataTable();
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();
                var sql_FMM_Get_DataSets = new FMM_UI_Assembly.SQL_FMM_Get_DataSets(si, connection);
                Seed_Temp_Tables_for_Calcs();

                var Proc_FinModel_Sql = @"
                    Select Sequence,Src_Type,Src_Item,Table_Calc_Expression,
					Table_Join_Expression,Table_Filter_Expression,Calc_Plan_Units,Acct,View,IC,
					Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,
					UD8,OS_Time_Filter,OS_Conditional_Filter
                    FROM FMM_Calc_Config calc
                    JOIN FMM_Src_Cell Src
                    ON calc.Calc_ID = Src.Calc_ID
					AND calc.Model_ID = Src.Model_ID
					JOIN FMM_Dest_Cell Dest
                    ON calc.Calc_ID = Dest.Calc_ID
					AND calc.Model_ID = Dest.Model_ID
                    WHERE Model_ID = @Model_ID
					AND Src.Calc_Src_ID_Order = 0
					AND Src.Src_Type IN ('Proportional Time','Allocation')
					ORDER By Sequence";

                var parameters = new SqlParameter[]
                {
                    new SqlParameter("@Model_ID", SqlDbType.Int) { Value = new_Model_ID }
                };

                // Fetch rows into a DataTable
                sql_FMM_Get_DataSets.Fill_get_FMM_DataTable(si, sql_DataSet_DataAdapter, FMM_calc_Config_DT, Proc_FinModel_Sql, parameters);

                // Process rows dynamically based on Src_Type
                foreach (DataRow row in FMM_calc_Config_DT.Rows)
                {
                    string calcSrcType = row["Src_Type"].ToString();
                    ProcessCalculationType(calcSrcType, row);
                }
                // Loop through results
            }
            return "Calculations Completed";
        }

        private void Seed_Temp_Tables_for_Calcs()
        {
            var new_OS_Model_ID = 0;
            var Seed_Temp_Table_Sql = @"
                Select Src_Type Src_Type, Src_Item Src_Item, Temp_Table_Name 
                FROM FMM_Calc_Config calc
                JOIN FMM_Src_Cell Src
                ON calc.Calc_ID = Src.Calc_ID
                WHERE OS_Model_ID = @OS_Model_ID
                AND Use_Temp_Table = 1";

            var parameters = new SqlParameter[]
            {
                new SqlParameter("@OS_Model_ID", SqlDbType.Int) { Value = new_OS_Model_ID }
            };

            var objDashboardWorkspace = BRApi.Dashboards.Workspaces.GetWorkspace(si, false, "Gov_PlanCycle");
            var dt = new DataTable();
            var nvbParams = new NameValueFormatBuilder();
            dt = BRApi.Import.Data.FdxExecuteCubeView(si, objDashboardWorkspace.WorkspaceID, "01_Civ_Con_Pay_Rates_by_PE", "DHP_Consol_Entities_Dim", "E#HQ_ManpowerFactors", "Main_Scenario_Dim", "S#POM2630_v0", "T#2024,T#2025", nvbParams, false, false, string.Empty, 8, true);

            // Loop through results
            // If Cube View run FDX into Temp Table
            // If Map -> Query Transformation Rule and load to Temp
            // If Dim Info -> Dim SQL and load to Temp
        }

        // Process calculation type dynamically
        private void ProcessCalculationType(string calcSrcType, DataRow row)
        {
            switch (calcSrcType)
            {
                case "Proportional Time":
                    HandleProportionalTime(row);
                    break;

                case "Allocation":
                    HandleAllocation(row);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown calculation type: {calcSrcType}");
            }
        }

        private void HandleProportionalTime(DataRow row)
        {
            try
            {
                // Step 1: Extract configuration fields from the DataRow
                string sequence = row["Sequence"].ToString();
                string calcSrcType = row["Src_Type"].ToString();
                string tableCalcExpression = row["Table_Calc_Expression"].ToString();
                string tableJoinExpression = row["Table_Join_Expression"].ToString();
                string tableFilterExpression = row["Table_Filter_Expression"].ToString();

                // Step 2: Build the dynamic SELECT statement
                string selectStatement = $@"
		            SELECT {tableCalcExpression} AS CalculatedValue
		            FROM YourSourceTable
		            {tableJoinExpression}
		            WHERE {tableFilterExpression}";

                Console.WriteLine($"Generated SQL for Sequence {sequence}:");
                Console.WriteLine(selectStatement);

                // Step 3: Execute the SQL statement and fetch results
                var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
                using (var connection = new SqlConnection(dbConnApp.ConnectionString))
                {
                    connection.Open();

                    DataTable resultTable = new DataTable();
                    using (var command = new SqlCommand(selectStatement, connection))
                    {
                        using (var adapter = new SqlDataAdapter(command))
                        {
                            adapter.Fill(resultTable);
                        }
                    }

                    // Step 4: Process results
                    foreach (DataRow resultRow in resultTable.Rows)
                    {
                        // Example: Output the calculated value
                        Console.WriteLine($"Sequence {sequence} - Calculated Value: {resultRow["CalculatedValue"]}");
                    }
                }

                Console.WriteLine($"Completed processing Proportional Time for Sequence {sequence}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing Proportional Time for Sequence {row["Sequence"]}: {ex.Message}");
            }
        }


        // Handle Allocation logic
        private void HandleAllocation(DataRow row)
        {
            // Logic specific to Allocation
            Console.WriteLine($"Processing Allocation for row: {row["Sequence"]}");
        }

        private DataTable joinMultipleDataTablesTest(List<DataTable> tables, DataTable configTable)
        {
            DataTable dt = new DataTable();

            IEnumerable<DataRow> EnumerableDT = dt.AsEnumerable();

            Dictionary<int, List<string>> joinKeyDict = new Dictionary<int, List<string>>();
            Dictionary<int, List<string>> targetKeyDict = new Dictionary<int, List<string>>();

            var joinConfigs = configTable.AsEnumerable().ToList();

            int rowNum = 1;
            foreach (DataRow dr in configTable.Rows)
            {
                joinKeyDict.Add(rowNum, dr["BaseJoinKeys"].ToString().Split(',').Select(x => x.Trim()).ToList());
                targetKeyDict.Add(rowNum, dr["TargetJoinKeys"].ToString().Split(',').Select(x => x.Trim()).ToList());
            }

            switch (tables.Count)
            {
                case 1:
                    break;
                case 2:
                    EnumerableDT = from row1 in tables[0].AsEnumerable()
                                   join row2 in tables[1].AsEnumerable()
                                   on GenerateCompositeKey(row1, joinKeyDict[1])
                                   equals GenerateCompositeKey(row2, targetKeyDict[1])
                                   select CombineRows(row1, row2);
                    break;
                case 3:
                    EnumerableDT = from row1 in tables[0].AsEnumerable()
                                   join row2 in tables[1].AsEnumerable()
                                   on GenerateCompositeKey(row1, joinKeyDict[1])
                                   equals GenerateCompositeKey(row2, targetKeyDict[1])
                                   join row3 in tables[2].AsEnumerable()
                                   on GenerateCompositeKeyFromRowPref(new DataRow[] { row1, row2 }, joinKeyDict[2])
                                   equals GenerateCompositeKey(row3, targetKeyDict[2])
                                   select CombineMultipleRows(new DataRow[] { row1, row2, row3 });
                    break;
            }

            dt = EnumerableDT.CopyToDataTable();

            return dt;
        }

        //		private DataTable JoinMultipleDataTables(List<DataTable> tables, DataTable configTable)
        //		{
        //		    // Start with the first DataTable as the base table
        //		    var baseTable = tables.First();
        //		    var remainingTables = tables.Skip(1);

        //		    // Parse configuration
        //		    var joinConfigs = configTable.AsEnumerable().ToList();

        //		    // Perform iterative joins
        //		    var joinedData = baseTable.AsEnumerable();
        //		    foreach (var table in remainingTables)
        //		    {
        //		        var joinConfig = joinConfigs.FirstOrDefault(); // Get the join config for this pair
        //		        if (joinConfig == null) continue;

        //		        // Extract join keys
        //		        var baseJoinKeys = joinConfig["BaseJoinKeys"].ToString().Split(',').Select(x => x.Trim()).ToList();
        //		        var targetJoinKeys = joinConfig["TargetJoinKeys"].ToString().Split(',').Select(x => x.Trim()).ToList();

        //		        // Perform the join
        //		        joinedData = from baseRow in joinedData
        //		                     join targetRow in table.AsEnumerable()
        //		                     on GenerateCompositeKey(baseRow, baseJoinKeys)
        //		                     equals GenerateCompositeKey(targetRow, targetJoinKeys)
        //		                     select CombineRows(baseRow, targetRow, joinConfig);
        //		    }

        //		    // Convert the joined data back to a DataTable
        //		    var resultTable = new DataTable();
        //		    foreach (var column in configTable.AsEnumerable().Select(row => row["OutputColumnName"].ToString()))
        //		    {
        //		        resultTable.Columns.Add(column, typeof(string)); // Adjust types as needed
        //		    }

        //		    foreach (var row in joinedData)
        //		    {
        //		        resultTable.Rows.Add(row.ItemArray);
        //		    }

        //		    return resultTable;
        //		}

        private object[] GenerateCompositeKey(DataRow row, List<string> keys)
        {
            return keys.Select(key => row[key.Split('.')[1]]).ToArray();
        }

        private object[] GenerateCompositeKeyFromRowPref(DataRow[] rows, List<string> keys)
        {
            return keys.Select(key => rows[Convert.ToInt32(key.Remove(0, 3))][key.Split('.')[1]]).ToArray();
        }

        private DataRow CombineMultipleRows(DataRow[] drs)
        {
            DataTable dt = new DataTable();

            //create a new datatable that contains all the columns of the passed in data rows
            foreach (DataRow dr in drs)
            {
                foreach (object colName in dr.ItemArray)
                {
                    dt.Columns.Add(new DataColumn(colName.ToString()));
                }
            }

            DataRow resultRow = dt.NewRow();

            foreach (DataRow dr in drs)
            {
                foreach (object colName in dr.ItemArray)
                {
                    resultRow[colName.ToString()] = dr[colName.ToString()];
                }
            }

            return resultRow;
        }

        private DataRow CombineRows(DataRow baseRow, DataRow targetRow)
        {
            var resultRow = baseRow.Table.NewRow();

            // Combine base and target rows
            foreach (DataColumn column in baseRow.Table.Columns)
            {
                resultRow[column.ColumnName] = baseRow[column.ColumnName];
            }

            foreach (DataColumn column in targetRow.Table.Columns)
            {
                resultRow[column.ColumnName] = targetRow[column.ColumnName];
            }

            return resultRow;
        }
    }
}