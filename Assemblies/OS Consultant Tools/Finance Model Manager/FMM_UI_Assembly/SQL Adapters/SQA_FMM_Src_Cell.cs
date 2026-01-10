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
    public class SQA_FMM_Src_Cell
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_FMM_Src_Cell(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_FMM_Src_Cell_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_FMM_Src_Cell(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                new ColumnDefinition("Cube_ID", SqlDbType.Int),
                new ColumnDefinition("Act_ID", SqlDbType.Int),
                new ColumnDefinition("Model_ID", SqlDbType.Int),
                new ColumnDefinition("Calc_ID", SqlDbType.Int),
                new ColumnDefinition("Src_Cell_ID", SqlDbType.Int),
                new ColumnDefinition("Src_Order", SqlDbType.Int),
                new ColumnDefinition("Src_Type", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Src_Item", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Open_Parens", SqlDbType.NVarChar, 10),
                new ColumnDefinition("Math_Operator", SqlDbType.NVarChar, 10),
                new ColumnDefinition("Entity", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Cons", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Scenario", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Time", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Origin", SqlDbType.NVarChar, 100),
                new ColumnDefinition("IC", SqlDbType.NVarChar, 100),
                new ColumnDefinition("View", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Src_Plan_Units", SqlDbType.NVarChar, 250),
                new ColumnDefinition("Acct", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Flow", SqlDbType.NVarChar, 100)
            };

            // Add UD1 through UD8
            for (int i = 1; i <= 8; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"UD{i}", SqlDbType.NVarChar, 100));
            }

            columnDefinitions.AddRange(new List<ColumnDefinition>
            {
                new ColumnDefinition("Close_Parens", SqlDbType.NVarChar, 10),
                new ColumnDefinition("Unbal_Buffer", SqlDbType.NVarChar, 500),
                new ColumnDefinition("Unbal_Origin_Override", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Unbal_IC_Override", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Unbal_Acct_Override", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Unbal_Flow_Override", SqlDbType.NVarChar, 200)
            });

            // Add Unbal_UD1_Override through Unbal_UD8_Override
            for (int i = 1; i <= 8; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"Unbal_UD{i}_Override", SqlDbType.NVarChar, 200));
            }

            columnDefinitions.AddRange(new List<ColumnDefinition>
            {
                new ColumnDefinition("Unbal_Buffer_Filter", SqlDbType.NVarChar, 500),
                new ColumnDefinition("Dyn_Calc_Script", SqlDbType.NVarChar, 500),
                new ColumnDefinition("Override_Value", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Table_Calc_Expression", SqlDbType.NVarChar, 1000),
                new ColumnDefinition("Table_Join_Expression", SqlDbType.NVarChar, 1000),
                new ColumnDefinition("Table_Filter_Expression", SqlDbType.NVarChar, 1000),
                new ColumnDefinition("Map_Type", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Map_Source", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Map_Logic", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Src_SQL_Stmt", SqlDbType.NVarChar, 2000),
                new ColumnDefinition("Use_Temp_Table", SqlDbType.Bit),
                new ColumnDefinition("Temp_Table_Name", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Create_Date", SqlDbType.DateTime),
                new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Update_Date", SqlDbType.DateTime),
                new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
            });

            var primaryKeyColumns = new List<string> { "Src_Cell_ID" };

            _helper.UpdateDataTable(si, dt, sqa, "FMM_Src_Cell", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
        }
    }
}