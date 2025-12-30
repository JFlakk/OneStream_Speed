using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
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
	public class FMM_DB_Svc : IWsasDashboardV800
	{
        public XFLoadDashboardTaskResult ProcessLoadDashboardTask(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args?.LoadDashboardTaskInfo != null))
                {
                    if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
                    {
                        // Implement Load Dashboard logic here.
                        if ((args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize) && (args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters))
                        {
                            var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
                            loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
                            loadDashboardTaskResult.ModifiedCustomSubstVars = null;
                            return loadDashboardTaskResult;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
	}
}