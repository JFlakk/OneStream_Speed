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
    public class SQA_RegPlan
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_RegPlan(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_RegPlan_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_RegPlan(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                new ColumnDefinition("RegPlan_ID", SqlDbType.UniqueIdentifier),
                new ColumnDefinition("WFScenario_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("WFProfile_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("WFTime_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Act_ID", SqlDbType.Int),
                new ColumnDefinition("Entity", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Appr_Level_ID", SqlDbType.UniqueIdentifier),
                new ColumnDefinition("Reg_ID_1", SqlDbType.Int),
                new ColumnDefinition("Reg_ID_2", SqlDbType.Int),
                new ColumnDefinition("Reg_ID", SqlDbType.NVarChar, 100)
            };

            // Add Attr_1 through Attr_20
            for (int i = 1; i <= 20; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"Attr_{i}", SqlDbType.NVarChar, 100));
            }

            // Add Attr_Val_1 through Attr_Val_12
            for (int i = 1; i <= 12; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"Attr_Val_{i}", SqlDbType.Decimal));
            }

            // Add Date_Val_1 through Date_Val_5
            for (int i = 1; i <= 5; i++)
            {
                columnDefinitions.Add(new ColumnDefinition($"Date_Val_{i}", SqlDbType.DateTime));
            }

            // Add remaining columns
            columnDefinitions.AddRange(new List<ColumnDefinition>
            {
                new ColumnDefinition("Spread_Amount", SqlDbType.Decimal),
                new ColumnDefinition("Spread_Curve", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Status", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Invalid", SqlDbType.Bit),
                new ColumnDefinition("Create_Date", SqlDbType.DateTime),
                new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Update_Date", SqlDbType.DateTime),
                new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
            });

            var primaryKeyColumns = new List<string> { "RegPlan_ID" };

            _helper.UpdateDataTable(si, dt, sqa, "RegPlan", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
        }
    }
}