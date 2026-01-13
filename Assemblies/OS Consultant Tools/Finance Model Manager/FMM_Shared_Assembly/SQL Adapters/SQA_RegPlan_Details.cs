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
        private readonly SQA_GBL_Command_Builder _cmdBuilder;

        public SQA_RegPlan_Details(SessionInfo si, SqlConnection connection)
        {
            _cmdBuilder = new SQA_GBL_Command_Builder(si, connection);
        }

        public void Fill_RegPlan_Details_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null)
                {
                    command.Parameters.AddRange(sqlparams);
                }
                sqa.SelectCommand = command;
                sqa.Fill(dt);
				command.Parameters.Clear();
            }
        }

        public void Update_RegPlan_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            _cmdBuilder.UpdateTableSimple(si, "RegPlan_Details", dt, sqa, "RegPlan_ID");
        }
    }
}