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
using OneStreamWorkspacesApi.V820;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_FMM_Act_Config
    {
        private readonly SqlConnection _connection;
        private readonly GBL_SQA_Helper _helper;

        public SQA_FMM_Act_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
            _helper = new GBL_SQA_Helper(connection);
        }

        public void Fill_FMM_Act_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _helper.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_FMM_Act_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            var primaryKeyColumns = new List<string> { "Act_ID" };

            _helper.UpdateDataTableDynamic(si, dt, sqa, "FMM_Act_Config", primaryKeyColumns, autoTimestamps: false);
        }
    }
}