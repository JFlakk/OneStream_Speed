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

        // show/hide template param name
        private const string TmpParam_ShowHideMenu = "ShowHide";

        // header temp params
        private const string TmpParam_BoundParameter = "DynamicComponentItem_BoundParameter";
        private const string TmpParam_HeaderItemAction = "HeaderItemAction";
        private const string TmpParam_HeaderItemIcon = "HeaderItemIcon";

        // Regular Parameter
        // show/hide input parameter
        private const string Param_ShowHideMenu = "IV_ShowHideMenu";

        private const string Param_MenuWidth = "IV_DDM_Menu_Width";
        private const string MenuWidth_Auto = "Auto";
        private const string MenuWidth_Zero = "0";

        private const string NavAction_Show = "Show";
        private const string NavAction_Hide = "Hide";


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

        internal static XFLoadDashboardTaskResult OnLoadDynamicNavMenu(SessionInfo si, BRGlobals brGlobals, DashboardExtenderArgs args)
        {
            var loadResult = new XFLoadDashboardTaskResult();
            if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
            {
                loadResult.ChangeCustomSubstVarsInDashboard = true;
                loadResult.ModifiedCustomSubstVars.Add(Param_ShowHideMenu, NavAction_Hide);
                loadResult.ModifiedCustomSubstVars.Add(Param_MenuWidth, MenuWidth_Auto);
                if (args.PrimaryDashboard.Name.Contains("Admin"))
                {
                    // Add and set cubeName IV
                    var wfUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si);
                    var ProfileKey = wfUnitPk.ProfileKey;
                    int configProfileID = DDM_DB_Config_Support.getCurrentProfileID(si, ProfileKey);

                    int menuOptionID = DDM_DB_Config_Support.getSelectedMenuOption(si, args.SelectionChangedTaskInfo.CustomSubstVars);

                    DataTable configMenuOptionsDT = DDM_DB_Config_Support.getConfigMenuOptions(si, configProfileID, menuOptionID);

                    // get cube name based on SI.
                    int cubeID = si.PovDataCellPk.CubeId;
                    string cubeName = DDM_DB_Config_Support.getCubeName(si, cubeID);

                    // add cubename IV
                    loadResult.ModifiedCustomSubstVars.Add(DDM_DB_Config_Support.Param_CubeName, cubeName);

                    Dictionary<string, string> ParamsToAdd = DDM_DB_Config_Support.getParamsToAdd(DDM_DB_Config_Support.getHeaderItems(si, args.SelectionChangedTaskInfo.CustomSubstVars));

                    foreach (string param in ParamsToAdd.Keys)
                    {
                        loadResult.ModifiedCustomSubstVars.Add(param, ParamsToAdd[param]);
                    }
                }

                // need to figure out how to add all header config params on dashboard load
                // get components
                // get their params
                // add params to modifiedcustomsubstvars
                //				BRApi.ErrorLog.LogMessage(si, "test vals: " + args.EmbeddedDashboard.Name);


                //loadResult.ModifiedCustomSubstVars.Add();
            }
            else
            {

                //				BRApi.ErrorLog.LogMessage(si, "Testing load functionality");
                //				BRApi.ErrorLog.LogMessage(si, "reason: " + args.LoadDashboardTaskInfo.Reason.ToString());
                //				BRApi.ErrorLog.LogMessage(si, "action: " + args.LoadDashboardTaskInfo.Action.ToString());
                //				BRApi.ErrorLog.LogMessage(si, "test vals2: ");

            }
            return loadResult;
        }

        // Component changes should handle the underlying customsubstvars changes, then refresh dashboard all components to update themselves
        internal static XFSelectionChangedTaskResult OnDynamicNavMenuShowHideChange(SessionInfo si, DashboardExtenderArgs args)
        {
            var taskResult = new XFSelectionChangedTaskResult() { ChangeCustomSubstVarsInDashboard = true };

            // get current showhide status of menu
            string ShowHideStatus = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue(Param_ShowHideMenu);

            // BRApi.ErrorLog.LogMessage(si, "ShowHideStatus, Comp call: " + ShowHideStatus);

            // set new showhide status to param (inverted of current status)
            string newShowHideStatus = ShowHideStatus == NavAction_Hide ? NavAction_Show : NavAction_Hide;
            taskResult.ModifiedCustomSubstVars.Add(Param_ShowHideMenu, newShowHideStatus);


            // set menu width property
            string MenuWidth = ShowHideStatus == NavAction_Hide ? MenuWidth_Zero : MenuWidth_Auto;
            taskResult.ModifiedCustomSubstVars.Add(Param_MenuWidth, MenuWidth);

            return taskResult;
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


        internal static WsDynamicDashboardEx Get_DynamicNavMenu_Actions(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {


            // This will always include the show/hide menu button
            // this will be further filled out with the header configuration items from the SQL table
            // each item will contain an icon and an action (I think)
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();





            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);


            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }

        // menu label
        internal static WsDynamicDashboardEx Get_DynamicNavMenuLabel_Actions(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();

            // get current showhide status of menu
            string ShowHideStatus = customSubstVarsAlreadyResolved.XFGetValue(Param_ShowHideMenu, NavAction_Hide);

            // add show/hide menu button to the items
            Dictionary<string, string> nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                [TmpParam_ShowHideMenu] = ShowHideStatus,
            };
            repeatArgsList.Add(new WsDynamicComponentRepeatArgs(ShowHideStatus, nextLevelTemplateSubstVarsToAdd));


            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }
		
        // menu label
        internal static WsDynamicDashboardEx Get_DynamicHeader(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();

            // get current showhide status of menu
            string ShowHideStatus = customSubstVarsAlreadyResolved.XFGetValue(Param_ShowHideMenu, NavAction_Hide);

            // add show/hide menu button to the items
            Dictionary<string, string> nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
            {
                [TmpParam_ShowHideMenu] = ShowHideStatus,
            };
            repeatArgsList.Add(new WsDynamicComponentRepeatArgs(ShowHideStatus, nextLevelTemplateSubstVarsToAdd));


            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }

        // config items
        internal static WsDynamicDashboardEx Get_DynamicNavConfigItems_Actions(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = new List<WsDynamicComponentRepeatArgs>();


            var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            dynamicDashboardEx.DynamicDashboard.Tag = repeatArgsList;

            api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }



        internal static WsDynamicComponentCollection Get_DynamicNavMenu_Actions_DynamicComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
            var componentCollection = api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);


            // TODO: Figure out how to dynamically decide which dashboard to redraw
            // If possible to grab parent dashboard name and then filter through the underlying children
            // that would be best I think
            //string dynamicDashboardsToRedraw = dynamicDashboardEx.TemplateSubstVars.

            return componentCollection;
        }


        internal static WsDynamicComponentCollection Get_DynamicNavMenuLabel_Actions_DynamicComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var repeatArgsList = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
            var componentCollection = api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            var showHideStatus = customSubstVarsAlreadyResolved.XFGetValue(Param_ShowHideMenu, string.Empty);


            // TODO: Figure out how to dynamically decide which dashboard to redraw
            // If possible to grab parent dashboard name and then filter through the underlying children
            // that would be best I think
            //string dynamicDashboardsToRedraw = dynamicDashboardEx.TemplateSubstVars.

            if (!String.IsNullOrWhiteSpace(showHideStatus))
            {
                var navBarComponent = componentCollection?.Components?.Find(x => x.DynamicComponentEx?.TemplateSubstVars?[TmpParam_ShowHideMenu].XFEqualsIgnoreCase(showHideStatus) ?? false);
                if (navBarComponent != null)
                {
                    var btnComp = navBarComponent.DynamicComponentEx.DynamicComponent.Component;

                    //btnComp.DashboardsToRedraw = dynamicDashboardsToRedraw;

                    var displayFormat = new ComponentDisplayFormatBuilder(btnComp.DisplayFormat);
                    displayFormat.SetValue(ComponentDisplayFormatBuilder.constBackgroundColor, XFColors.XFLightBlue.Name);
                    displayFormat.SetValue(ComponentDisplayFormatBuilder.constBorderThickness, "1");
                    btnComp.DisplayFormat = displayFormat.FormatString;

                    api.SaveDynamicComponentState(si, dynamicDashboardEx.DynamicDashboard, navBarComponent.DynamicComponentEx, WsDynamicItemStateType.MinimalWithTemplateParameters);
                }
            }

            return componentCollection;
        }

        internal static WsDynamicComponentCollection Get_Dynamic_Header_Components(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var componentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);

            //			string testStr = "";
            //			if (componentCollection.Components.Count > 0) {
            //				testStr = componentCollection.Components[0].DynamicComponentEx.DynamicComponent.Component.ParamValueForButtonClick;
            //			}
            //			BRApi.ErrorLog.LogMessage(si, "xml data: " + testStr);

			
            // add header items
            DataTable headerItems = DDM_DB_Config_Support.getHeaderItems(si, customSubstVarsAlreadyResolved);
            //BRApi.ErrorLog.LogMessage(si, "headerItems: " + headerItems.Rows.Count);
            var tempColl = addHeaderItems(ref headerItems, si, workspace, api, dynamicDashboardEx, maintUnit);

            foreach (var item in tempColl.Components)
            {
                componentCollection.Components.Add(item);
            }

            // TODO: update header to be a grid with items spaced evenly horizontally

            return componentCollection;
        }


        private static WsDynamicComponentCollection addHeaderItems(ref DataTable headerItems, SessionInfo si, DashboardWorkspace ws, IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit)
        {

            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();

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


                BRApi.ErrorLog.LogMessage(si,$"Hit {dyn_Param.ItemName.ToString()}");

                //WsDynamicDbrdCompMemberEx tempCompEx = new WsDynamicDbrdCompMemberEx();

                string baseSearch = "DDM_Menu_Hdr_";

                string optType = row[baseSearch + "Option_Type"].ToString();

                var nameSuffix = "test" + Random.Shared.NextInt64(0, 10); // TODO: Come up with a better name suffix. They might be able to be all the same?

                var templateSubVars = new Dictionary<string, string>
                    {
                        { TmpParam_BoundParameter, BP_CompReplacement }
                    };

                var tempComp = new DashboardDbrdCompMemberEx();
					
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

                                tempComp = api.GetStoredComponentForDynamicDashboard(si,ws,dynamicDashboardEx.DynamicDashboard, colSuffix.ToLower() + "_DDM_Generic");
                                tempComp.Component.ApplyParamValueToCurrentDbrd = true;
                                //								tempComp.DisplayFormat = "|!LV_Std_btn_Format!|";

                                tempComp.Component.DashboardComponentType = dashboardTypeResolver[colSuffix];
                                tempComp.Component.Text = row[baseSearch + "_" + colSuffix + "_Lbl"].ToString();
                                tempComp.Component.ToolTip = row[baseSearch + "_" + colSuffix + "_ToolTip"].ToString();
                                var dimType = row[baseSearch + "_Dim_Type"].ToString();


                                if (BP_CompReplacement == "")
                                {
                                    BP_CompReplacement = colSuffix == "Txt" ? "IV_" : "ML_";
                                    BP_CompReplacement += "DDM_App_" + dimType + "_Selection";

                                }
                                
                                new_param.Parameter = new DashboardParameter();
                                new_param.Parameter.DimTypeName = dimType;
                                new_param.Parameter.Name = $"ML_DDM_App_{dimType}_Selection";
                                new_param.Parameter.ParameterType = ParameterType.MemberList;
                                new_param.Parameter.Cube = 

                                BP_CompReplacement = new_param.Parameter.Name; //row[baseSearch + "_" + colSuffix + "_BoundParameter"].ToString();

                                //tempComp.BoundParameterName = "~!" + TmpParam_BoundParameter + "!~";

                                //								tempComp.BoundParameterName = BP_CompReplacement;

                                string defaultSelection = row[baseSearch + "_Default"].ToString();

                                //tempComp.TemplateParameterValues = $"{TmpParam_BoundParameter}={defaultSelection}"

                                if (colSuffix == "Btn")
                                {
                                    if (!String.IsNullOrEmpty(tempComp.Component.XmlData))
                                    {
                                        compDefinition = XElement.Parse(tempComp.Component.XmlData);
                                    }
                                    else
                                    {
                                        compDefinition = buildButtonXML(optType);
                                    }

                                    tempComp.Component.SelectionChangedUIActionType = XFSelectionChangedUIActionType.Refresh;
                                    tempComp.Component.DashboardsToRedraw = "DDM_App_Content,DDM_App_Header"; //TODO: Update to use row btn dashboard refresh if necessary

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
                                    tempComp.Component.XmlData = compDefinition.ToString();
                                }

                                nameSuffix = "test" + Random.Shared.NextInt64(0, 10);

                                templateSubVars = new Dictionary<string, string>
                                    {
                                        { TmpParam_BoundParameter, BP_CompReplacement }
                                    };

                                WsDynamicComponentEx filterCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.Component, nameSuffix, templateSubVars, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
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
                        tempComp = api.GetStoredComponentForDynamicDashboard(si,ws,dynamicDashboardEx.DynamicDashboard, "btn_DDM_Generic");


                        tempComp.Component.DashboardComponentType = DashboardComponentType.Button;

                        if (!String.IsNullOrEmpty(tempComp.Component.XmlData))
                        {
                            compDefinition = XElement.Parse(tempComp.Component.XmlData);
                        }
                        else
                        {
                            compDefinition = buildButtonXML(optType);
                        }

                        tempComp.Component.Text = row[baseSearch + "_Lbl"].ToString();
                        tempComp.Component.ToolTip = row[baseSearch + "_ToolTip"].ToString();
                        compDefinition.SetElementValue("ImageFileSourceType", "DashboardFile");
                        compDefinition.SetElementValue("ImageUrlOrFullFileName", row[baseSearch + "_Image_URL"].ToString());

                        tempComp.Component.SelectionChangedUIActionType = row["Btn_Type"].ToString() == "Complete_WF" ? XFSelectionChangedUIActionType.OpenDialogApplyChangesAndRefresh : XFSelectionChangedUIActionType.Refresh;

                        tempComp.Component.SelectionChangedTaskType = serverTaskTypeResolver.ContainsKey(row[baseSearch + "_ServerTaskType"].ToString()) ? serverTaskTypeResolver[row[baseSearch + "_ServerTaskType"].ToString()] : XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                        //tempComp.SelectionChangedTaskType = XFSelectionChangedTaskType.ExecuteDashboardExtenderBRConsServer;
                        tempComp.Component.SelectionChangedTaskArgs = row[baseSearch + "_ServerTask"].ToString();


                        tempComp.Component.DashboardsToRedraw = row[baseSearch + "_DBRefresh"].ToString();
                        tempComp.Component.DashboardForDialog = row[baseSearch + "_DBOpen"].ToString();

                        if (compDefinition != null)
                        {
                            tempComp.Component.XmlData = compDefinition.ToString();
                        }

                        WsDynamicComponentEx buttonCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.Component, nameSuffix, templateSubVars, TriStateBool.TrueValue, WsDynamicItemStateType.MinimalWithTemplateParameters);
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

        public static XFSelectionChangedTaskResult printHello(SessionInfo si, DashboardExtenderArgs args)
        {
            XFSelectionChangedTaskResult temp = new XFSelectionChangedTaskResult();
            temp.ChangeCustomSubstVarsInDashboard = true;
            temp.IsOK = true;
            temp.Message = "Hello";
            BRApi.ErrorLog.LogMessage(si, "Here in hello");

            temp.ShowMessageBox = true;
            return temp;
        }
    }
}