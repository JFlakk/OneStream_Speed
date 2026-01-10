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
    public class SQA_FMM_Cube_Config
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_FMM_Cube_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_FMM_Cube_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_FMM_Cube_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var columnDefinitions = new List<ColumnDefinition>
            {
                new ColumnDefinition("Cube_ID", SqlDbType.Int),
                new ColumnDefinition("Cube", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Scen_Type", SqlDbType.NVarChar, 20),
                new ColumnDefinition("Descr", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Entity_Dim", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Entity_MFB", SqlDbType.NVarChar, 100),
                new ColumnDefinition("Agg_Consol", SqlDbType.NVarChar, 15),
                new ColumnDefinition("Status", SqlDbType.NVarChar, 10),
                new ColumnDefinition("Create_Date", SqlDbType.DateTime),
                new ColumnDefinition("Create_User", SqlDbType.NVarChar, 50),
                new ColumnDefinition("Update_Date", SqlDbType.DateTime),
                new ColumnDefinition("Update_User", SqlDbType.NVarChar, 50)
            };

            var primaryKeyColumns = new List<string> { "Cube_ID" };

            _helper.UpdateDataTable(si, dt, sqa, "FMM_Cube_Config", columnDefinitions, primaryKeyColumns, autoTimestamps: false);
        }
    }
}