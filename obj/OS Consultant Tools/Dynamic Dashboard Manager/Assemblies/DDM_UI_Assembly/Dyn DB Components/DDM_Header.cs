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
        private const string template_Mbr_List_cbxbtn_BoundParam = "Mbr_List_cbxbtn_BoundParam";
		private const string template_Mbr_List_Default = "Mbr_List_Default";
		private const string template_Mbr_List_Cube = "Mbr_List_Cube";
		private const string template_Mbr_List_Dim = "Mbr_List_Dim";
		private const string template_Mbr_List_Filter = "Mbr_List_Filter";
		private const string template_Mbr_List_btn_Visible = "btn_Visible";
		private const string template_Mbr_List_btn_Text = "btn_Text";
		private const string template_Mbr_List_btn_ToolTip = "btn_ToolTip";
		private const string template_Mbr_List_cbx_Visible = "cbx_Visible";
		private const string template_Mbr_List_cbx_Text = "cbx_Text";
		private const string template_Mbr_List_cbx_ToolTip = "cbx_ToolTip";
		private const string template_Mbr_List_txt_Visible = "txt_Visible";
		private const string template_Mbr_List_txt_Text = "txt_Text";
		private const string template_Mbr_List_txt_ToolTip = "txt_ToolTip";
		private const string template_Mbr_List_txt_BoundParam = "Mbr_List_txt_BoundParam";
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
			var repeatArg_List = new List<WsDynamicComponentRepeatArgs>();
			var dt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved,"Filter");
			foreach (DataRow row in dt.Rows)
            {
				var templateSubstVars = new Dictionary<string, string>();
				var dimType = row["Fltr_Dim_Type"].ToString();
                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbxbtn_BoundParam))
                {
                    templateSubstVars.Add(template_Mbr_List_cbxbtn_BoundParam, $"ML_DDM_App_{dimType}_Mbr_List");
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_Default))
                {
                    templateSubstVars.Add(template_Mbr_List_Default, row["Fltr_Default"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_Cube))
                {
                    templateSubstVars.Add(template_Mbr_List_Cube, "Army_RMW_Consol");
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_Dim))
                {
                    templateSubstVars.Add(template_Mbr_List_Dim, row["Fltr_Dim_Name"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_Filter))
                {
                    templateSubstVars.Add(template_Mbr_List_Filter, row["Fltr_MFB"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_btn_Visible))
                {
                    // Convert the bit (bool) value to "True"/"False" string
                    templateSubstVars.Add(template_Mbr_List_btn_Visible, Convert.ToBoolean(row["Fltr_Btn"]) ? "True" : "False");
                }		
                if (!templateSubstVars.ContainsKey(template_Mbr_List_btn_Text))
                {
                    templateSubstVars.Add(template_Mbr_List_btn_Text, row["Fltr_Btn_Lbl"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_Mbr_List_btn_ToolTip))
                {
                    templateSubstVars.Add(template_Mbr_List_btn_ToolTip, row["Fltr_Btn_ToolTip"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbx_Visible))
                {
                    templateSubstVars.Add(template_Mbr_List_cbx_Visible, Convert.ToBoolean(row["Fltr_Cbx"]) ? "True" : "False");
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbx_Text))
                {
                    templateSubstVars.Add(template_Mbr_List_cbx_Text, row["Fltr_Cbx_Lbl"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbx_ToolTip))
                {
                    templateSubstVars.Add(template_Mbr_List_cbx_ToolTip, row["Fltr_Cbx_ToolTip"].ToString());
                }	
                if (!templateSubstVars.ContainsKey(template_Mbr_List_txt_Visible))
                {
                    templateSubstVars.Add(template_Mbr_List_txt_Visible, Convert.ToBoolean(row["Fltr_Txt"]) ? "True" : "False");
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_txt_Text))
                {
                    templateSubstVars.Add(template_Mbr_List_txt_Text, row["Fltr_Txt_Lbl"].ToString());
                }
                if (!templateSubstVars.ContainsKey(template_Mbr_List_txt_ToolTip))
                {
                    templateSubstVars.Add(template_Mbr_List_txt_ToolTip, row["Fltr_Txt_ToolTip"].ToString());
                }			
                if (!templateSubstVars.ContainsKey(template_Mbr_List_txt_BoundParam))
                {
                    templateSubstVars.Add(template_Mbr_List_txt_BoundParam, $"IV_DDM_App_{dimType}_Mbr");
                }
				repeatArg_List.Add(new WsDynamicComponentRepeatArgs(dimType,templateSubstVars));
				BRApi.ErrorLog.LogMessage(si,$"Hit: {dimType} - {row["Fltr_Default"].ToString()} - {row["Fltr_MFB"].ToString()} - {row["Fltr_MFB"].ToString()}");
			}

        	var dynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty,null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

        	dynamicDashboardEx.DynamicDashboard.Tag = repeatArg_List;

        	api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, dynamicDashboardEx, WsDynamicItemStateType.MinimalWithTemplateParameters);

            return dynamicDashboardEx;
        }
		
        internal static WsDynamicComponentCollection get_DynamicHdrRepeatedComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {	
			var repeatArg_List = dynamicDashboardEx.DynamicDashboard.Tag as List<WsDynamicComponentRepeatArgs>;
			var dynCompRepeated_dynDashboard = new WsDynamicComponentCollection();
			dynCompRepeated_dynDashboard = api.GetDynamicComponentsRepeatedForDynamicDashboard(si,workspace,dynamicDashboardEx,repeatArg_List,TriStateBool.TrueValue,WsDynamicItemStateType.EntireObject);
            // Loop through the repeated components to find Dashboards
            foreach (var comp in dynCompRepeated_dynDashboard.Components)
            {
                if (comp.DynamicComponentEx.DynamicComponent != null && comp.DynamicComponentEx.DynamicComponent.Component != null)
                {
                    var componentType = comp.DynamicComponentEx.DynamicComponent.Component.DashboardComponentType.ToString();
					var dashboardName = comp.DynamicComponentEx.DynamicComponent.Component.Name;
					
					var DyynDashBoard = new WsDynamicDashboardEx();
					var DB = new WsDynamicDashboard(dynCompRepeated_dynDashboard.ParentDashboard.DynamicDashboard);
					//DB.Name = dashboardName;
					BRApi.ErrorLog.LogMessage(si, $"Hit cnt {comp.DynamicComponentEx.DynamicComponent.Component.EmbeddedDashboardName} - {comp.DynamicComponentEx.DynamicComponent.Component.TemplateParameterValues}");
                    if (DB.ComponentTemplateRepeatItems != null)
                    {
                        foreach (var storedCompTmplateRpt in DB.ComponentTemplateRepeatItems)
                        {
							BRApi.ErrorLog.LogMessage(si, $"Hit Template {storedCompTmplateRpt}");
                            if (storedCompTmplateRpt.TemplateParameterValues != null)
                            {
								 BRApi.ErrorLog.LogMessage(si, $"Hit Template Params {storedCompTmplateRpt.TemplateParameterValues.ToString()}");
                                 foreach (var paramValues in storedCompTmplateRpt.TemplateParameterValues)
                                 {
                                     BRApi.ErrorLog.LogMessage(si, $"TemplateParameter: {paramValues} =");
                                 }
                            }
                            else
                            {
                                BRApi.ErrorLog.LogMessage(si, "TemplateParameterValues is null.");
                            }
                        }
                    }
                    else
                    {
                        BRApi.ErrorLog.LogMessage(si, "ComponentTemplateRepeatItems is null.");
                    }
                    if (!string.IsNullOrEmpty(componentType))
                    {
//						api.GetStoredComponentsForDynamicDashboard
                        var tempComp_List = api.GetStoredComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx.DynamicDashboard) as List<DashboardDbrdCompMemberEx>;

                        BRApi.ErrorLog.LogMessage(si, $"Found Dashboard Component: {componentType} - {tempComp_List.Count}");

                        foreach (var storedComp in tempComp_List)
                        {
                            var compName = storedComp.Component?.Name ?? "null";
                            var compType = storedComp.Component?.DashboardComponentType.ToString() ?? "null";
                            BRApi.ErrorLog.LogMessage(si, $"Stored Component - Name: {compName}, Type: {compType}");
                        }
                        // You can add your logic here to process the dashboard as needed
                    }
                }
            }
			return dynCompRepeated_dynDashboard;
		}

        internal static WsDynamicComponentCollection get_DynamicHdrComponents(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            var componentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
			BRApi.ErrorLog.LogMessage(si,$"Dyn DB {dynamicDashboardEx.DynamicDashboard.Name}");
            // add header items
            var dt = DDM_Support.get_HeaderItems(si, customSubstVarsAlreadyResolved,"Filter");
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
            var tempXML = new XElement("XFButtonDefinition");
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
        
        private static WsDynamicComponentCollection addHeaderItems(ref DataTable headerItems, SessionInfo si, DashboardWorkspace ws, IWsasDynamicDashboardsApiV800 api, WsDynamicDashboardEx dynamicDashboardEx, DashboardMaintUnit maintUnit)
        {

            var wsDynCompMembers = new List<WsDynamicDbrdCompMemberEx>();
            var storedComponent = new DashboardDbrdCompMemberEx();
            var nameSuffix = string.Empty;

            var templateSubstVars = new Dictionary<string, string>();

            int iteration = 1;
            bool containsTxtBox = false;
            int rowCount = headerItems.Rows.Count;

            foreach (DataRow row in headerItems.Rows)
            {
				templateSubstVars.Clear();
                var tempCompMember = new WsDynamicDbrdCompMember();
                var compDefinition = new XElement("XFCompDefinition");
				var tempComp = new WsDynamicComponentEx();

                var baseSearch = string.Empty;

                var optType = row["Option_Type"].ToString();

                var storedCompName_servertaskbtn = "btn_DDM_App_Complete_WF";


                // based on item type, switch to define the header item
                switch (optType)
                {
                    case "Filter":
                        baseSearch += "Fltr";

                        foreach (string colSuffix in dashboardTypeResolver.Keys)
                        {
                            if ((bool)row[baseSearch + "_" + colSuffix])
                            {
								var dimType = row[baseSearch + "_Dim_Type"].ToString();
                                var stored_param = new DashboardParamDisplayInfo();
								stored_param = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, false, null, ws.WorkspaceID, $"{ws.Name}.ML_DDM_App_{dimType}_Mbr_List");
                                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbxbtn_BoundParam))
                                {
                                    templateSubstVars.Add(template_Mbr_List_cbxbtn_BoundParam, stored_param.Parameter.Name);
                                }
                                if (!templateSubstVars.ContainsKey(template_Mbr_List_Default))
                                {
                                    templateSubstVars.Add(template_Mbr_List_Default, row[baseSearch + "_Default"].ToString());
                                }
                                if (!templateSubstVars.ContainsKey(template_Mbr_List_Cube))
                                {
                                    templateSubstVars.Add(template_Mbr_List_Cube, "Army_RMW_Consol");
                                }
                                if (!templateSubstVars.ContainsKey(template_Mbr_List_Dim))
                                {
                                    templateSubstVars.Add(template_Mbr_List_Dim, row[baseSearch + "_Dim_Name"].ToString());
                                }
                                if (!templateSubstVars.ContainsKey(template_Mbr_List_Filter))
                                {
                                    templateSubstVars.Add(template_Mbr_List_Filter, row[baseSearch + "_MFB"].ToString());
                                }
								storedComponent = api.GetStoredComponentForDynamicDashboard(si, ws, dynamicDashboardEx.DynamicDashboard, colSuffix.ToLower() + "_DDM_App_Mbr_List");
                                if (colSuffix.ToLower() == "btn" || colSuffix.ToLower() == "cbx")
                                {
									var new_param = new WsDynamicParameter(true,stored_param.Parameter,stored_param.Parameter.UniqueID,stored_param.Parameter.Name,ws.Name);
									new_param.Parameter = new DashboardParameter();
									//new_param.Parameter.UniqueID = Guid.NewGuid();
									//new_param.Parameter.Name = row["Fltr_BtnCbx_BoundParam"].ToString();
									new_param.Parameter.Name = $"{stored_param.Parameter.Name}_dynamic_{dimType}";
									new_param.Parameter.ParameterType = DashboardParamType.MemberList;
									new_param.Parameter.DimTypeName = dimType;
                                    new_param.Parameter.CubeName = "Army_RMW_Consol";
                                    new_param.Parameter.MemberFilter = row[baseSearch + "_MFB"].ToString();
                                    new_param.Parameter.DimName = row[baseSearch + "_Dim_Name"].ToString();
									//tempComp.DynamicComponent.Component.BoundParameterName = new_param.Parameter.Name;
								}
                                else
                                {
									var new_param = new WsDynamicParameter(true,stored_param.Parameter,Guid.NewGuid(),row["Fltr_Txt_BoundParam"].ToString(),ws.Name);
                                    new_param.Parameter = new DashboardParameter();
                                    new_param.Parameter.Name = row["Fltr_Txt_BoundParam"].ToString();
                                    new_param.Parameter.ParameterType = DashboardParamType.InputValue;
									//tempComp.DynamicComponent.Component.BoundParameterName = new_param.Parameter.Name;
                                }
								tempComp = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, storedComponent.Component, string.Empty, null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                                tempComp.DynamicComponent.Component.ApplyParamValueToCurrentDbrd = true;
                                //								tempComp.DisplayFormat = "|!LV_Std_btn_Format!|";

                                tempComp.DynamicComponent.Component.DashboardComponentType = dashboardTypeResolver[colSuffix];
                                tempComp.DynamicComponent.Component.Text = row[baseSearch + "_" + colSuffix + "_Lbl"].ToString();
                                tempComp.DynamicComponent.Component.ToolTip = row[baseSearch + "_" + colSuffix + "_ToolTip"].ToString();
                                tempComp.DynamicComponent.Component.Name = storedComponent.Component.Name;

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

                                nameSuffix = "Dyn_1";

                                if (!templateSubstVars.ContainsKey(template_Mbr_List_cbxbtn_BoundParam))
                                {
                                    templateSubstVars.Add(template_Mbr_List_cbxbtn_BoundParam, stored_param.Parameter.Name);
                                }
                                else
                                {
                                    templateSubstVars[template_Mbr_List_cbxbtn_BoundParam] = stored_param.Parameter.Name;
                                }

                                WsDynamicComponentEx filterCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, dimType, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, filterCompEx));

                                 //add a secondary button component to handle text entry
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

                        WsDynamicComponentEx buttonCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempComp.DynamicComponent.Component, nameSuffix, templateSubstVars, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
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
                    WsDynamicComponentEx tempLineCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, tempLine, "line", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);

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

                WsDynamicComponentEx txtEntryCompEx = api.GetDynamicComponentForDynamicDashboard(si, ws, dynamicDashboardEx, txtEntryComp, "TextEntry", null, TriStateBool.TrueValue, WsDynamicItemStateType.EntireObject);
                wsDynCompMembers.Add(new WsDynamicDbrdCompMemberEx(tempCompMember, txtEntryCompEx));
            }


            return new WsDynamicComponentCollection(dynamicDashboardEx, wsDynCompMembers);
        }

#endregion
    }
}