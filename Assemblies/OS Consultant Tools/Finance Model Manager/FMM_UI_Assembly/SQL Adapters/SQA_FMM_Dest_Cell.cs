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
    public class SQA_FMM_Dest_Cell
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_FMM_Dest_Cell(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_FMM_Dest_Cell_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_FMM_Dest_Cell(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                new ColumnDefinition("Cube_ID", SqlDbType.Int),
                new ColumnDefinition("Act_ID", SqlDbType.Int),
                new ColumnDefinition("Model_ID", SqlDbType.Int),
                new ColumnDefinition("Calc_ID", SqlDbType.Int),
                new ColumnDefinition("Dest_Cell_ID", SqlDbType.Int),
                new ColumnDefinition("Location", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Calc_Plan_Units", SqlDbType.NVarChar, 250),
                new ColumnDefinition("Acct", SqlDbType.NVarChar, 100),
                new ColumnDefinition("View", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Origin", SqlDbType.NVarChar, 100),
                new ColumnDefinition("IC", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Flow", SqlDbType.NVarChar, 100)
            };

            // Add UD1 through UD8
            for (int i = 1; i <= 8; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"UD{i}", SqlDbType.NVarChar, 100));
            }

            columnDefinitions.AddRange(new List<ColumnDefinition>
            {
                new ColumnDefinition("Time_Filter", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Acct_Filter", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Origin_Filter", SqlDbType.NVarChar, 200),
                new ColumnDefinition("IC_Filter", SqlDbType.NVarChar, 200),
                new ColumnDefinition("Flow_Filter", SqlDbType.NVarChar, 200)
            });

            // Add UD1_Filter through UD8_Filter
            for (int i = 1; i <= 8; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"UD{i}_Filter", SqlDbType.NVarChar, 200));
            }

            columnDefinitions.AddRange(new List<ColumnDefinition>
            {
                new ColumnDefinition("Conditional_Filter", SqlDbType.NVarChar, 1000),
                new ColumnDefinition("Curr_Cube_Buffer_Filter", SqlDbType.NVarChar, 1000),
                new ColumnDefinition("Buffer_Filter", SqlDbType.NVarChar, 2000),
                new ColumnDefinition("Dest_Cell_Logic", SqlDbType.NVarChar, 2000),
                new ColumnDefinition("SQL_Logic", SqlDbType.NVarChar, 2000),
                new ColumnDefinition("Create_Date", SqlDbType.DateTime),
                new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Update_Date", SqlDbType.DateTime),
                new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
            });

            var primaryKeyColumns = new List<string> { "Dest_Cell_ID" };

            _helper.UpdateDataTable(si, dt, sqa, "FMM_Dest_Cell", columnDefinitions, primaryKeyColumns, autoTimestamps: true);
        }
    }
}