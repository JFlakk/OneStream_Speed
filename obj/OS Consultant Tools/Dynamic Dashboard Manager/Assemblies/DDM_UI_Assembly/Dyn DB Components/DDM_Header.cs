using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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
using Microsoft.Data.SqlClient;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class DDM_Header
    {

        //Template Parameters

        // header temp params
        private const string TmpParam_BoundParameter = "DynamicComponentItem_BoundParameter";
        private const string TmpParam_HeaderItemAction = "HeaderItemAction";
        private const string TmpParam_HeaderItemIcon = "HeaderItemIcon";

        // Regular Parameter

        // header regular params
        private const string Param_HeaderAction = "IV_DDM_SelectedHeaderAction";
        private const string Param_HeaderTest = "Test_Entity";

        public static Dictionary<string, DashboardComponentType> dashboardTypeResolver = new Dictionary<string, DashboardComponentType>() {
            {"Btn", DashboardComponentType.Button},
            {"Cbx", DashboardComponentType.ComboBox},
            {"Txt", DashboardComponentType.TextBox}
        };

        public static Dictionary<string, XFSelectionChangedTaskType> serverTaskTypeResolver = new Dictionary<string, XFSelectionChangedTaskType>() {
            {"General", XFSelectionChangedTaskType.ExecuteDashboardExtenderBusinessRule},
            {"Stage", XFSelectionChangedTaskType.ExecuteDashboardExtenderBRStageServer},
            {"Data Management Server", XFSelectionChangedTaskType.ExecuteDashboardExtenderBRDataMgmtServer},
            {"Finance Custom Calc BR", XFSelectionChangedTaskType.ExecuteFinanceCustomCalculateBR},
            {"Data Management Sequence", XFSelectionChangedTaskType.ExecuteDataManagementSequence},
            {"Calculate", XFSelectionChangedTaskType.Calculate},
            {"Force Calculate", XFSelectionChangedTaskType.ForceCalculate},
            {"Calculate w/ Logging", XFSelectionChangedTaskType.CalculateWithLogging},
            {"Force Calculate w/ Logging", XFSelectionChangedTaskType.ForceCalculateWithLogging},
            {"Translate", XFSelectionChangedTaskType.Translate},
            {"Force Translate", XFSelectionChangedTaskType.ForceTranslate},
            {"Translate w/ Logging", XFSelectionChangedTaskType.TranslateWithLogging},
            {"Force Translate w/ Logging", XFSelectionChangedTaskType.ForceTranslateWithLogging},
            {"Consolidate", XFSelectionChangedTaskType.Consolidate},
            {"Force Consolidate", XFSelectionChangedTaskType.ForceConsolidate},
            {"Consolidate w/ Logging", XFSelectionChangedTaskType.ConsolidateWithLogging},
            {"Force Consolidate w/ Logging", XFSelectionChangedTaskType.ForceConsolidateWithLogging},

        };

        public object Main(SessionInfo si)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }


        // Technically, this will update all IVs with associated MLs
        internal static XFSelectionChangedTaskResult OnTextEntered(SessionInfo si, DashboardExtenderArgs args)
        {
            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

            //update all text box IVs to their respective MLs
            Dictionary<string, string> IVs = args.SelectionChangedTaskInfo.CustomSubstVars.Where(x => x.Key.Contains("IV") && x.Key.Contains("Selection")).ToDictionary<string, string>();
            foreach (string IV in IVs.Keys)
            {
                taskResult.ModifiedCustomSubstVars.Add(IV.Replace("IV", "ML"), IVs[IV]);
            }

            return taskResult;
        }

        // menu label
        internal static WsDynamicDashboardEx get_DynamicHdr(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();

            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }

        internal static WsDynamicComponentCollection get_DynamicHdrComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var componentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);



            // add header items
            var dt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved);
            var tempColl = addHeaderItems(ref dt, si, workspace, api, dynamicDashboardEx, maintUnit);

            foreach (var item in tempColl.Components)
            {
                componentCollection.Components.Add(item);
            }

            // TODO: update header to be a grid with items spaced evenly horizontally

            return componentCollection;
        }

        #region "Dynamic DB Helper Functions"
        private static XElement buildButtonXML(string btnType)
        {
            XElement tempXML = new XElement("XFButtonDefinition");
            tempXML.Add(new XElement("ImageFileSourceType"));
            tempXML.Add(new XElement("ImageUrlOrFullFileName"));
            tempXML.Add(new XElement("PageNumber"));
            tempXML.Add(new XElement("ExcelSheet"));
            tempXML.Add(new XElement("ExcelNamedRange"));
            if (btnType == "Filter")
            {
                tempXML.SetAttributeValue("ButtonType", "SelectMemberDialog");
                tempXML.Add(new XElement("SelectMemberInfo"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("DimTypeName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("UseAllDimsForDimType", false));
                tempXML.Element("SelectMemberInfo").Add(new XElement("DimName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("CubeName"));
                tempXML.Element("SelectMemberInfo").Add(new XElement("MemberFilter"));
            }

            return tempXML;
        }

        private static WsDynamicDbrdCompMemberEx BuildDynamicComponentFromStoredComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicDashboardEx dynamicDashboardEx, string storedComponentName, string nextLevelNameSuffixToAdd, Dictionary<string, string> nextLevelTemplateSubstVarsToAdd, WsDynamicItemStateType dynamicItemStateType)
        {
            var storedComp = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, workspace.UniqueID, maintUnit.UniqueID, storedComponentName, false, true);
            if (storedComp != null)
            {
                var wsDynamicCompEx = api.GetDynamicComponentForDynamicDashboard(si, workspace, dynamicDashboardEx, storedComp, nextLevelNameSuffixToAdd, nextLevelTemplateSubstVarsToAdd, TriStateBool.TrueValue, dynamicItemStateType);
                if (wsDynamicCompEx != null)
                {
                    var wsDynCompMember = new WsDynamicDbrdCompMember();
                    return new WsDynamicDbrdCompMemberEx(wsDynCompMember, wsDynamicCompEx);
                }
            }

            return null;
        }
        
        private static WsDynamicComponentCollection addHeaderItems(ref DataTable headerItems, SessionInfo si, DashboardWorkspace ws, IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit)
        {

            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            var storedComponent = new DashboardDbrdCompMemberEx();

            int iteration = 1;
            bool containsTxtBox = false;
            int rowCount = headerItems.Rows.Count;

            foreach (DataRow row in headerItems.Rows)
            {

                //				string BP_CompReplacement = "ML_DDM_App_Entity_Selection";
                string BP_CompReplacement = "";
                WsDynamicDbrdCompMember tempCompMember = new WsDynamicDbrdCompMember();
                XElement compDefinition = null;

                var new_param = new WsDynamicParameter();
                var dyn_Param = new WsDynamicItemNameKey();
                dyn_Param = new_param.CreateDynamicItemNameKey();

                string baseSearch = "";

                string optType = row["Option_Type"].ToString();

                var nameSuffix = "test" + Random.Shared.NextInt64(0, 10); // TODO: Come up with a better name suffix. They might be able to be all the same?

                var templateSubVars = new Dictionary<string, string>
                    {
                        { TmpParam_BoundParameter, BP_CompReplacement }
                    };

                var tempComp = new WsDynamicComponentEx();

                string storedCompName_servertaskbtn = "btn_DDM_App_Complete_WF";


                // based on item type, switch to define the header item
                switch (optType)
                {
                    case "Filter":
                        baseSearch += "Fltr";



                        foreach (string colSuffix in dashboardTypeResolver.Keys)
                        {
                            if ((bool)row[baseSearch + "_" + colSuffix])
                            {
                                storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, colSuffix.ToLower() + "_DDM_Generic");
                                tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                                tempComp.DynamicComponent.Component.ApplyParamValueToCurrentDbrd = true;
                                //								tempComp.DisplayFormat = "|!LV_Std_btn_Format!|";

                                tempComp.DynamicComponent.Component.DashboardComponentType = dashboardTypeResolver[colSuffix];
                                tempComp.DynamicComponent.Component.Text = row[baseSearch + "_" + colSuffix + "_Lbl"].ToString();
                                tempComp.DynamicComponent.Component.ToolTip = row[baseSearch + "_" + colSuffix + "_ToolTip"].ToString();
                                var dimType = row[baseSearch + "_Dim_Type"].ToString();
                                tempComp.DynamicComponent.Component.Name = $"btn_DDM_App_{dimType}_Selection";


                                if (BP_CompReplacement == "")
                                {
                                    BP_CompReplacement = colSuffix == "Txt" ? "IV_" : "ML_";
                                    BP_CompReplacement += "DDM_App_" + dimType + "_Selection";

                                }

                                new_param.Parameter = new DashboardParameter();
                                new_param.Parameter.DimTypeName = dimType;
                                new_param.Parameter.Name = $"ML_DDM_App_{dimType}_Selection";
                                new_param.Parameter.ParameterType = DashboardParamType.MemberList;
                                new_param.Parameter.CubeName = "Army_RMW_Consol";
                                new_param.Parameter.MemberFilter = "E#Army.Base";
                                new_param.Parameter.DimName = "Consol_Army_Dim";

                                BP_CompReplacement = new_param.Parameter.Name; //row[baseSearch + "_" + colSuffix + "_BoundParameter"].ToString();

                                //tempComp.BoundParameterName = "~!" + TmpParam_BoundParameter + "!~";

                                //								tempComp.BoundParameterName = BP_CompReplacement;

                                string defaultSelection = row[baseSearch + "_Default"].ToString();

                                //tempComp.TemplateParameterValues = $"{TmpParam_BoundParameter}={defaultSelection}"

                                if (colSuffix == "Btn")
                                {
                                    if (!String.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                                    {
                                        compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                                    }
                                    else
                                    {
                                        compDefinition = buildButtonXML(optType);
                                    }

                                    tempComp.DynamicComponent.Component.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
                                    tempComp.DynamicComponent.Component.DashboardsToRedraw = "DDM Dynamic App Dashboard (Workflow)"; //TODO: Update to use row btn dashboard refresh if necessary

                                    compDefinition.SetAttributeValue("ButtonType", "SelectMemberDialog"); // TODO: Check if DashboardComponentType.MemberSelectDialog is the same thing as this. Would likely still need to update through XML though for underlying

                                    if (compDefinition.Element("SelectMemberInfo") == null)
                                    {
                                        compDefinition.Add(new XElement("SelectMemberInfo"));
                                    }
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("DimTypeName", row[baseSearch + "_Dim_Type"].ToString());
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("DimName", row[baseSearch + "_Dim_Name"].ToString());
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("CubeName", "|!IV_DDM_App_Cube_Name!|");
                                    compDefinition.Element("SelectMemberInfo").SetElementValue("MemberFilter", row[baseSearch + "_MFB"].ToString());
                                    compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                                    compDefinition.SetElementValue("ImageUrlOrFullFileName", "Std_DB_Search.png"); // TODO: Add a col for allowing image input
                                }

                                if (compDefinition != null)
                                {
                                    tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();
                                }

                                nameSuffix = "test" + Random.Shared.NextInt64(0, 10);

                                templateSubVars = new Dictionary<string, string>
                                    {
                                        { TmpParam_BoundParameter, BP_CompReplacement }
                                    };

                                WsDynamicComponentEx filterCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, nameSuffix, templateSubVars, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
                                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, filterCompEx));

                                // add a secondary button component to handle text entry
                                if (colSuffix == "Txt")
                                {
                                    containsTxtBox = true;
                                }
                            }
                        }


                        break;
                    case "Button":
                        baseSearch += "Btn";
                        storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, "btn_DDM_Generic");
                        tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

                        tempComp.DynamicComponent.Component.DashboardComponentType = DashboardComponentType.Button;

                        if (!String.IsNullOrEmpty(tempComp.DynamicComponent.Component.XmlData))
                        {
                            compDefinition = XElement.Parse(tempComp.DynamicComponent.Component.XmlData);
                        }
                        else
                        {
                            compDefinition = buildButtonXML(optType);
                        }

                        tempComp.DynamicComponent.Component.Text = row[baseSearch + "_Lbl"].ToString();
                        tempComp.DynamicComponent.Component.ToolTip = row[baseSearch + "_ToolTip"].ToString();
                        compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                        compDefinition.SetElementValue("ImageUrlOrFullFileName", row[baseSearch + "_Image_URL"].ToString());

                        tempComp.DynamicComponent.Component.SelectionChangedUIActionType = row["Btn_Type"].ToString() == "Complete_WF" ? XFSelectionChangedUIActionType.OpenDialogApplyChangesAndRefresh : XFSelectionChangedUIActionType.Refresh;

                        tempComp.DynamicComponent.Component.SelectionChangedTaskType = serverTaskTypeResolver.ContainsKey(row[baseSearch + "_ServerTaskType"].ToString()) ? serverTaskTypeResolver[row[baseSearch + "_ServerTaskType"].ToString()] : XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                        //tempComp.SelectionChangedTaskType = XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                        tempComp.DynamicComponent.Component.SelectionChangedTaskArgs = row[baseSearch + "_ServerTask"].ToString();


                        tempComp.DynamicComponent.Component.DashboardsToRedraw = row[baseSearch + "_DBRefresh"].ToString();
                        tempComp.DynamicComponent.Component.DashboardForDialog = row[baseSearch + "_DBOpen"].ToString();

                        if (compDefinition != null)
                        {
                            tempComp.DynamicComponent.Component.XmlData = compDefinition.ToString();
                        }

                        WsDynamicComponentEx buttonCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, nameSuffix, templateSubVars, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
                        wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, buttonCompEx));


                        break;
                    default:
                        continue;
                        break;

                }



                // if there are items after this item and the current item is not a button, add a line
                // line component is really just an image component that shows a line /shrug
                // TODO: Figure out how to skip the line if it's a btn comp
                if (iteration < rowCount && optType != "Button")
                {
                    // add line image component
                    string storedImageCompName = "img_Line";

                    DashboardComponent tempLine = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, storedImageCompName, false, true);
                    WsDynamicComponentEx tempLineCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempLine, "line", null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

                    WsDynamicDbrdCompMemberEx tempLineDynCompMemberEx = new WsDynamicDbrdCompMemberEx(tempCompMember, tempLineCompEx);
                    wsDynCompMembers.Add(tempLineDynCompMemberEx);

                }


                iteration++;

            }


            // if any items contained a text box add a refresh button on the end
            if (containsTxtBox)
            {
                string storedCompName_textEntered = "btn_DDM_EnterText";
                WsDynamicDbrdCompMember tempCompMember = new WsDynamicDbrdCompMember(); // can adjust dashboard info here

                DashboardComponent txtEntryComp = EngineDashboardComponents.GetComponent(api.DbConnAppOrFW, ws.UniqueID, maintUnit.UniqueID, storedCompName_textEntered, false, true);

                WsDynamicComponentEx txtEntryCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, txtEntryComp, "TextEntry", null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, txtEntryCompEx));
            }


            return new WsDynamicComponentCollection(dynamicDashboardEx, wsDynCompMembers);
        }

#endregion
    }
}