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
	public class FMM_Content
	{
        public object Test(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
		
		/// <summary>
		/// Build dynamic embedded dashboard to present Cube Calc vs Reg Calc
		/// </summary>
		public static WsDynamicDashboardEx Get_DynamicModelContentDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, 
			DashboardWorkspace workspace, DashboardMaintUnit maintUnit, WsDynamicComponentEx parentDynamicComponentEx, 
			Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
		{
			try
			{
				// Get the dynamic dashboard
				var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, 
					string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
				
				// Determine if we should show Cube Calc or Reg Calc based on custom substitution variables
				string modelType = customSubstVarsAlreadyResolved.XFGetValue("IV_FMM_Model_Type");
				
				// Save state
				api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, 
					WsDynamicItemStateType.MinimalWithTemplateParameters);
				
				return dynamicDashboardEx;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
	}
}