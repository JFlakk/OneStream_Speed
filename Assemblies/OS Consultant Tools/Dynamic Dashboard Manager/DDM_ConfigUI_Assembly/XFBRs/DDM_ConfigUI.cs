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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.DDM_ConfigUI
{
    public class MainClass
    {
        #region "Global Variables"
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;
        #endregion
        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
                if (args.FunctionName.XFEqualsIgnoreCase("Get_Clean_Username"))
                {
                    // Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
                    return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, true, false);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_ColFormat"))
                {
                    var curr_TED = args.NameValuePairs.XFGetValue("curr_TED");
                    var curr_DB = args.NameValuePairs.XFGetValue("curr_DB");
                    var col = args.NameValuePairs.XFGetValue("col");
                    return Get_DDM_ColFormat(curr_TED, curr_DB, col);
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_Config_Menu_Layout_DB"))
                {
                    return this.Get_DDM_Config_Menu_Layout_DB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_Config_Hdr_Ctrl_DB"))
                {
                    return this.Get_DDM_Config_Hdr_Ctrl_DB();
                }
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_DDM_Config_IsVisible"))
                {
                    BRApi.ErrorLog.LogMessage(si, "hit");
                    return this.Get_DDM_Config_IsVisible();
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static string Get_DDM_ColFormat(string curr_ted, string curr_DB, string col)
        {
            if (curr_ted.Equals("Config", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    if (DDMColFormatter.ConfigColumns.TryGetValue(curr_DB, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            else if (curr_ted.Equals("Config_Menu", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    if (DDMColFormatter.ConfigMenuColumns.TryGetValue(curr_DB, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            else if (curr_ted.Equals("Config_Menu_Hdr", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    if (DDMColFormatter.ConfigMenuHdrColumns.TryGetValue(curr_DB, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            return null;
        }

        public string Get_DDM_Config_Menu_Layout_DB()
        {
            var OptionType = args.CustomSubstVars.XFGetValue("DL_DDM_Layout_Type", "0").XFConvertToInt();
            var configHelpers = new DDM_ConfigHelpers();
            var layoutOptionConfig = configHelpers.Get_Layout_Option_Type_Config(OptionType);
            if (layoutOptionConfig != null)
            {
                return layoutOptionConfig.DashboardName;
            }
            else
            {
                return string.Empty;
            }
        }


        public string Get_DDM_Config_Hdr_Ctrl_DB()
        {
            var hdrCtrlType = args.CustomSubstVars.XFGetValue("DL_DDM_Hdr_Ctrl_Option_Type", "0").XFConvertToInt();
            var configHelpers = new DDM_ConfigHelpers();
            var hdrCtrlConfig = configHelpers.Get_Hdr_Ctrl_Option_Type_Config(hdrCtrlType);
            if (hdrCtrlConfig != null)
            {
                return hdrCtrlConfig.DashboardName;
            }
            else
            {
                return string.Empty;
            }
        }

        public string Get_DDM_Config_IsVisible()
        {
            var checkType = args.NameValuePairs.XFGetValue("checkType", "NA");

            switch (checkType)
            {
                case "paneContent":
                    {
                        var paneVal = args.NameValuePairs.XFGetValue("dbPaneContentType", "0").XFConvertToInt();
                        var txtBoxType = args.NameValuePairs.XFGetValue("txtBoxType", "NA");
                        var isVisible = (paneVal == (int)DDM_ConfigHelpers.DBPaneContents.CubeView && txtBoxType == "CV") ||
                                         (paneVal == (int)DDM_ConfigHelpers.DBPaneContents.Dashboard && txtBoxType == "DB");
                        BRApi.ErrorLog.LogMessage(si, $"Hit {isVisible}");
                        return isVisible.ToString();
                    }
                case "filterCheckbox":
                    {
                        return string.Empty;
                    }

                case "btnCbxBoundParam":
                    {
                        return string.Empty;
                    }
                default:
                    // If we don't recognize the type, return empty immediately
                    return string.Empty;
            }
        }
        public class ColumnConfig
        {
            public string ColumnName { get; set; }
            public string Description { get; set; }
            public bool IsVisible { get; set; }
            public string Width { get; set; } = "Auto";
            public bool AllowUpdates { get; set; } = true;
            public string DefaultValue { get; set; }
            public string ParameterName { get; set; }

            public override string ToString()
            {
                var parts = new List<string>();
                if (!AllowUpdates) parts.Add("AllowUpdates = False");
                if (!string.IsNullOrEmpty(ColumnName)) parts.Add($"ColumnName = {ColumnName}");
                if (!string.IsNullOrEmpty(DefaultValue)) parts.Add($"DefaultValue = {DefaultValue}");
                if (!string.IsNullOrEmpty(Description)) parts.Add($"Description = {Description}");
                if (!string.IsNullOrEmpty(Width)) parts.Add($"Width = {Width}");
                if (!string.IsNullOrEmpty(ParameterName)) parts.Add($"ParameterName = {ParameterName}");
                if (IsVisible) parts.Add("IsVisible = True");

                return string.Join(", ", parts);
            }
        }

        public class DDMColFormatter
        {
            public static readonly Dictionary<string, ColumnConfig[]> ConfigColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "DDM_WFP_Config", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "DDM_ConfigID", IsVisible = false, AllowUpdates = false},
                        new ColumnConfig { ColumnName = "ProfileKey", Description = "[Profile Name]", IsVisible = true, AllowUpdates = false, DefaultValue = "|!IV_DDM_trv_WFP!|", ParameterName = "BL_DDM_RootWFP" },
                        new ColumnConfig { ColumnName = "ProfileStepType", Description = "[Profile Step Type]", IsVisible = true, AllowUpdates = false, ParameterName = "DL_DDM_Profile_Step_Type" },
                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CreateDate", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "CreateUser", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "UpdateDate", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "UpdateUser", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
                    }
                },
                { "0b1_DDM_StandAlone_Config_Content", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
                        new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
                        new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MultiDim_Alloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_Calc", Description = "[Mbr List Calc?]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_Dim", Description = "[Mbr List 1 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_Filter", Description = "[Mbr List 1 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_1_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_1_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_2_Dim", Description = "[Mbr List 2 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_2_Filter", Description = "[Mbr List 2 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_2_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_2_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_3_Dim", Description = "[Mbr List 3 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_3_Filter", Description = "[Mbr List 3 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_3_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_3_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_4_Dim", Description = "[Mbr List 4 Dim]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_4_Filter", Description = "[Mbr List 4 Filter]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MbrList_4_DimType", IsVisible = false },
                        new ColumnConfig { ColumnName = "MbrList_4_Filter", IsVisible = false },
                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc?]", IsVisible = true },
                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
                    }
                }
            };

            public static readonly Dictionary<string, ColumnConfig[]> ConfigMenuColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "0b1_DDM_WFProfile_Config_Content", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "DDM_Config_ID", DefaultValue = "|!IV_DDM_Config_ID!|" },
                        new ColumnConfig { ColumnName = "DDM_Menu_ID"},
                        new ColumnConfig { ColumnName = "DDM_Type", DefaultValue = "1" },
                        new ColumnConfig { ColumnName = "Sort_Order", Description = "[Sort Order]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Name", Description = "[Option Name]" },
                        new ColumnConfig { ColumnName = "Option_Type", Description = "[Option Type]", IsVisible = true, ParameterName = "DL_DDM_Option_Type" },
                        new ColumnConfig { ColumnName = "Custom_DB_Header", Description = "[Custom Dashboard Header]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Custom_DB_Content", Description = "[Custom Dashboard Content]", IsVisible = true },
                        new ColumnConfig { ColumnName = "DB_Name", Description = "[Dashboard Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CV_Name", Description = "[Cube View Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "DB_Name_Top", Description = "[Dashboard Name Top]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CV_Name_Top", Description = "[Cube View Name Top]", IsVisible = true },
                        new ColumnConfig { ColumnName = "DB_Name_Bottom", Description = "[Dashboard Name Bottom]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CV_Name_Bottom", Description = "[Cube View Name Bottom]", IsVisible = true },
                        new ColumnConfig { ColumnName = "DB_Name_Left", Description = "[Dashboard Name Left]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CV_Name_Left", Description = "[Cube View Name Left]", IsVisible = true },
                        new ColumnConfig { ColumnName = "DB_Name_Right", Description = "[Dashboard Name Right]", IsVisible = true },
                        new ColumnConfig { ColumnName = "CV_Name_Right", Description = "[Cube View Name Right]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Status", IsVisible = true },
                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
			            // Add more placeholders if needed
			        }
                },
                { "0b1_DDM_StandAlone_Config_Content", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Dest_Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Time_Filter", Description = "Time Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Acct_Filter", Description = "Account Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin_Filter", Description = "Origin Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow_Filter", Description = "Flow Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1_Filter", Description = "UD1 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2_Filter", Description = "UD2 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3_Filter", Description = "UD3 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4_Filter", Description = "UD4 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5_Filter", Description = "UD5 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6_Filter", Description = "UD6 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7_Filter", Description = "UD7 Filter", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8_Filter", Description = "UD8 Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                }
            };
            public static readonly Dictionary<string, ColumnConfig[]> ConfigMenuHdrColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "0b1_DDM_WFProfile_Config_Content", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Src_Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
                        new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
                        new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
                        new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Calc_ID!|" },
                        new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Type", Description = "[Source/Calc Type]", ParameterName = "DL_FMM_Table_Calc_Src", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Item", Description = "[Source/Calc Item]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Calc_Expression", Description = "[Calc Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Join_Expression", Description = "[Join Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "Table_Filter_Expression", Description = "[Filter Expression]", Width = "200", IsVisible = true },
                        new ColumnConfig { ColumnName = "Map_Type", Description = "[Map Type]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Map_Source", Description = "[Map Source]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Map_Logic", Description = "[Map Logic]", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "0b1_DDM_StandAlone_Config_Content", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
                        new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
                        new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
                        new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false, DefaultValue = "|!IV_FMM_Calc_ID!|" },
                        new ColumnConfig { ColumnName = "Src_Order", Description = "Order", IsVisible = true },
                        new ColumnConfig { ColumnName = "Open_Parens", Description = "(", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Math_Operator", Description = "Op", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Entity", Description = "Entity", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Cons", Description = "Cons", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Scenario", Description = "Scenario", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Time", Description = "Time", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Origin", Description = "Origin", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "IC", Description = "IC", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Flow", Description = "Flow", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD1", Description = "UD1", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD2", Description = "UD2", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD3", Description = "UD3", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD4", Description = "UD4", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD5", Description = "UD5", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD6", Description = "UD6", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD7", Description = "UD7", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "UD8", Description = "UD8", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Close_Parens", Description = ")", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Src_Type", Description = "[Source Type]", ParameterName = "DL_FMM_Cube_Calc_Source", DefaultValue = "[Stored Cell]", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                }
            };
        }
    }
}