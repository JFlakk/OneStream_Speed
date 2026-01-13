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
using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_FMM_Calc_Unit_Assign
    {
        private readonly SQA_GBL_Command_Builder _cmdBuilder;

        public SQA_FMM_Calc_Unit_Assign(SessionInfo si, SqlConnection connection)
        {
            _cmdBuilder = new SQA_GBL_Command_Builder(si, connection);
        }

        public void Fill_FMM_Calc_Unit_Assign_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _cmdBuilder.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_FMM_Calc_Unit_Assign(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            _cmdBuilder.UpdateTableSimple(si, "FMM_Calc_Unit_Assign", dt, sqa, "Calc_Unit_Assign_ID");
        }
    }
}
