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
    public class SQA_RegPlan_Audit
    {
        private readonly SQA_GBL_Command_Builder _cmdBuilder;

        public SQA_RegPlan_Audit(SessionInfo si, SqlConnection connection)
        {
            _cmdBuilder = new SQA_GBL_Command_Builder(si, connection);
        }

        public void Fill_RegPlan_Audit_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            _cmdBuilder.FillDataTable(si, sqa, dt, sql, sqlparams);
        }

        public void Update_RegPlan_Audit(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            // RegPlan_Audit has composite key - use UpdateTableComposite
            _cmdBuilder.UpdateTableComposite(si, "RegPlan_Audit", dt, sqa, "Register_Plan_ID", "WF_Scenario_Name", "Project_ID", "Entity");
        }
    }
}
