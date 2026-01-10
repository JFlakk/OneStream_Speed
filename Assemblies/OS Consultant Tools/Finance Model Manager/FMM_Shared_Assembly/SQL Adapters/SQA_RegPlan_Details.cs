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
    public class SQA_RegPlan_Details
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_RegPlan_Details(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_RegPlan_Details_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_RegPlan_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                new ColumnDefinition("RegPlan_ID", SqlDbType.UniqueIdentifier),
                new ColumnDefinition("WFScenario_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("WFProfile_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("WFTime_Name", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Act_ID", SqlDbType.Int),
                new ColumnDefinition("Model_ID", SqlDbType.Int),
                new ColumnDefinition("Entity", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Appr_Level_ID", SqlDbType.UniqueIdentifier),
                new ColumnDefinition("Plan_Units", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Account", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Flow", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD1", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD2", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD3", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD4", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD5", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD6", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD7", SqlDbType.NVarChar, 100),
                new ColumnDefinition("UD8", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Year", SqlDbType.NVarChar, 4),
                new ColumnDefinition("Month1", SqlDbType.Decimal),
                new ColumnDefinition("Month2", SqlDbType.Decimal),
                new ColumnDefinition("Month3", SqlDbType.Decimal),
                new ColumnDefinition("Month4", SqlDbType.Decimal),
                new ColumnDefinition("Month5", SqlDbType.Decimal),
                new ColumnDefinition("Month6", SqlDbType.Decimal),
                new ColumnDefinition("Month7", SqlDbType.Decimal),
                new ColumnDefinition("Month8", SqlDbType.Decimal),
                new ColumnDefinition("Month9", SqlDbType.Decimal),
                new ColumnDefinition("Month10", SqlDbType.Decimal),
                new ColumnDefinition("Month11", SqlDbType.Decimal),
                new ColumnDefinition("Month12", SqlDbType.Decimal),
                new ColumnDefinition("Quarter1", SqlDbType.Decimal),
                new ColumnDefinition("Quarter2", SqlDbType.Decimal),
                new ColumnDefinition("Quarter3", SqlDbType.Decimal),
                new ColumnDefinition("Quarter4", SqlDbType.Decimal),
                new ColumnDefinition("Yearly", SqlDbType.Decimal),
                new ColumnDefinition("AllowUpdate", SqlDbType.Bit),
                new ColumnDefinition("Create_Date", SqlDbType.DateTime),
                new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Update_Date", SqlDbType.DateTime),
                new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
            };

            var primaryKeyColumns = new List<string> { "RegPlan_ID", "Year", "Plan_Units", "Account" };

            _helper.UpdateDataTable(si, dt, sqa, "RegPlan_Details", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
        }
    }
}