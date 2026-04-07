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
    public class FMM_CompSvc : IWsasComponentV800
    {
        public XFSelectionChangedTaskResult ProcessComponentSelectionChanged(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args?.SelectionChangedTaskInfo != null))
                {
                    if (args.FunctionName.XFEqualsIgnoreCase("Proc_ModelGrps"))
                    {
                        // Implement Dashboard Component Selection Changed logic here.
                        var selectionChangedTaskResult = new XFSelectionChangedTaskResult();
                        selectionChangedTaskResult.IsOK = true;
                        selectionChangedTaskResult.ShowMessageBox = false;
                        selectionChangedTaskResult.Message = "";
                        selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = false;
                        selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = null;
                        selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = false;
                        selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = null;
                        selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = false;
                        selectionChangedTaskResult.ModifiedCustomSubstVars = null;
                        selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = false;
                        selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = null;
                        return selectionChangedTaskResult;
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
