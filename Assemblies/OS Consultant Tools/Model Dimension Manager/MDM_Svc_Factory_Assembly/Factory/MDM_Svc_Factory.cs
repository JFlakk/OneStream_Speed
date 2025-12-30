using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Wcf;
using OneStreamWorkspacesApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class MDM_Svc_Factory : IWsAssemblyServiceFactory
    {
        public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, WsAssemblyServiceType wsAssemblyServiceType, string itemName)
        {
            try
            {
                return wsAssemblyServiceType switch
                {
                    WsAssemblyServiceType.DynamicDashboards => new MDM_DynDB_Svc(),
                    WsAssemblyServiceType.Dashboard => new MDM_DB_Svc(),

                    _ => throw new NotImplementedException(),
                };
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}