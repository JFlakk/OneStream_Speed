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
    public class DDM_Svc_Factory : IWsAssemblyServiceFactory
    {
        public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, WsAssemblyServiceType wsAssemblyServiceType, string itemName)
        {
            try
            {
                return wsAssemblyServiceType switch
                {
                    WsAssemblyServiceType.DynamicDashboards => new DDM_DynDB_Svc(),
                    WsAssemblyServiceType.Dashboard => new DDM_DB_Svc(),

                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}