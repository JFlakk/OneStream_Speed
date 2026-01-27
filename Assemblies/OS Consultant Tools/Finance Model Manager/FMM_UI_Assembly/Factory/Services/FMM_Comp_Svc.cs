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
    public class FMM_Comp_Svc : IWsasComponentV800
    {
        public XFSelectionChangedTaskResult ProcessComponentSelectionChanged(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args != null) && (args.SelectionChangedTaskInfo != null))
                {

                    // create our data-handling object
                    var src_CellDb = new FMM_Src_CellDB(si);

                    // Save any change to Config records
                    if (args.FunctionName.XFEqualsIgnoreCase("Save"))
                    {
                        // get values from parameters into an object
                        var componentId = this.getIdFromComponentName(args.ComponentInfo.Component.Name);
                        var dynamicSuffix = this.getDynamicSuffix(args.ComponentInfo.Component.Name);

                        var srcCellModels = FMM_Config_Helpers.BuildSrcCellModels(
                            args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues,
                            componentId,
                            dynamicSuffix,
                            (paramName, defaultValue) => this.getDynamicParamValue(args, paramName, defaultValue),
                            paramName => this.getDynamicParamGuid(args, paramName));

                        src_CellDb.Merge(srcCellModels, 1);

                        return new XFSelectionChangedTaskResult
                        {
                            IsOK = true,
                            ShowMessageBox = false
                        };
                    }
                    else if (args.FunctionName.XFEqualsIgnoreCase("Delete"))
                    {
                        src_CellDb.Delete(this.getIdFromComponentName(args.ComponentInfo.Component.Name));
                        // tell the dashboard if we need to do anything
                        XFSelectionChangedTaskResult selectionChangedTaskResult = new XFSelectionChangedTaskResult();
                        selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = true;
                        selectionChangedTaskResult.IsOK = true;
                        selectionChangedTaskResult.ShowMessageBox = false;
                        return selectionChangedTaskResult;
                    }
                    else if (args.FunctionName.XFEqualsIgnoreCase("NewConfig"))
                    {
                        var customSubstVars = args.SelectionChangedTaskInfo?.CustomSubstVarsWithUserSelectedValues ?? new Dictionary<string, string>();
                        src_CellDb.CreateDefaultConfig(args.EmbeddedDashboard.WorkspaceID, customSubstVars);
                        return new XFSelectionChangedTaskResult
                        {
                            ChangeCustomSubstVarsInDashboard = true,
                            IsOK = true,
                            ShowMessageBox = false
                        };
                    }
                }

                return null/* TODO Change to default(_) if this is not a reference type */;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        // Utility functions to retrieve parameter values from components.
        // Basically we need to figure out the dynamically-generated name of the parameter
        // before we can retrieve it from CustomSubstVarsWithUserSelectedValues.
        // Alternatively, you could use template variables everywhere, i.e. {wsmu}{myFunc}{id=[~!myID!~]}
        public string getDynamicParamValue(DashboardExtenderArgs args, string paramName, string defaultValue = null)
        {
            string paramValue = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues[this.buildDynamicParamName(args, paramName)];
            if (!string.IsNullOrEmpty(paramValue))
                return paramValue;
            return defaultValue;
        }

        public string getDynamicParamDisplayValue(DashboardExtenderArgs args, string paramName)
        {
            return args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues[$"!{this.buildDynamicParamName(args, paramName)}!"];
        }

        public string buildDynamicParamName(DashboardExtenderArgs args, string paramName)
        {
            return $"{paramName}_dynamic_{this.getDynamicSuffix(args.ComponentInfo.Component.Name)}";
        }

        // dynamic bound parameters are always of the form <paramname>_dynamic_<suffix>
        public string getDynamicSuffix(string name)
        {
            if (!name.Contains(("_dynamic_")))
                return "";
            return name.Split("_dynamic_")[1];
        }

        // guids need extra care to avoid returning Nothing
        public Guid getDynamicParamGuid(DashboardExtenderArgs args, string paramName)
        {
            string paramValue = this.getDynamicParamValue(args, paramName, Guid.Empty.ToString());
            if (string.IsNullOrEmpty(paramValue))
                return Guid.Empty;
            return Guid.Parse(paramValue);
        }


        public string getIdFromComponentName(string componentName)
        {
            return this.getDynamicSuffix(componentName);
        }
    }
}
