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

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.FMM_Config_UI
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
                else if (args.FunctionName.XFEqualsIgnoreCase("Get_Model_Col_Format"))
                {
                    var curr_TED = args.NameValuePairs.XFGetValue("curr_TED");
                    var curr_ModelType = args.NameValuePairs.XFGetValue("curr_ModelType");
                    var col = args.NameValuePairs.XFGetValue("col");
                    return Get_Model_Col_Format(curr_TED, curr_ModelType, col);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        public static string Get_Model_Col_Format(string curr_TED, string curr_ModelType, string col)
        {
            if (curr_TED.Equals("Calc_Config", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

                    if (ModelColumnFormatter.CalcConfigColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            else if (curr_TED.Equals("Dest", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

                    if (ModelColumnFormatter.DestCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            else if (curr_TED.Equals("Src", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(col, out int colIndex) && colIndex > 0)
                {
                    var modelTypeKey = curr_ModelType.Equals("Table", StringComparison.OrdinalIgnoreCase) ? "Table" : "Cube";

                    if (ModelColumnFormatter.SrcCellColumns.TryGetValue(modelTypeKey, out var columns) && colIndex <= columns.Length)
                    {
                        return columns[colIndex - 1].ToString();
                    }
                }
            }
            return null;
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

        public class ModelColumnFormatter
        {
            public static readonly Dictionary<string, ColumnConfig[]> CalcConfigColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "Table", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Cube_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Cube_ID!|" },
                        new ColumnConfig { ColumnName = "Act_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Act_ID!|" },
                        new ColumnConfig { ColumnName = "Model_ID", IsVisible = false, AllowUpdates = false, DefaultValue = "|!IV_FMM_Model_ID!|" },
                        new ColumnConfig { ColumnName = "Sequence", Description = "Seq", IsVisible = true },
                        new ColumnConfig { ColumnName = "Name", Description = "[Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Condition", Description = "[Conditional Logic]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Explanation", Description = "[Calc Explanation]", IsVisible = true },
                        new ColumnConfig { ColumnName = "MultiDim_Alloc", Description = "[Multi-Dim Alloc]", IsVisible = true },
                        new ColumnConfig { ColumnName = "BR_Calc", Description = "[BR Calc]", IsVisible = true },
                        new ColumnConfig { ColumnName = "BR_Calc_Name", Description = "[BR Calc Name]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Time_Phasing", Description = "[Time Phasing]", IsVisible = true, ParameterName = "DL_FMM_Calc_Time_Phasing" },
                        new ColumnConfig { ColumnName = "Input_Frequency", Description = "[Input Freq]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Status", Description = "[Status]", IsVisible = true },
                        new ColumnConfig { ColumnName = "Create_Date", Description = "[Create Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Create_User", Description = "[Create User]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_Date", Description = "[Update Date]", IsVisible = true, AllowUpdates = false },
                        new ColumnConfig { ColumnName = "Update_User", Description = "[Update User]", IsVisible = true, AllowUpdates = false }
                    }
                },
                { "Cube", new ColumnConfig[]
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

            public static readonly Dictionary<string, ColumnConfig[]> DestCellColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "Table", new ColumnConfig[]
                    {
                        new ColumnConfig { ColumnName = "Calc_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Dest_Cell_ID", IsVisible = false },
                        new ColumnConfig { ColumnName = "Location", Description = "Target Location", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Calc_Plan_Units", Description = "[Plan Units]", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "Acct", Description = "Account", Width = "Auto", IsVisible = true },
                        new ColumnConfig { ColumnName = "View", Description = "View", Width = "Auto", IsVisible = true },
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
                        new ColumnConfig { ColumnName = "Conditional_Filter", Description = "Conditional Filter", Width = "Auto", IsVisible = true },
			            // Add more placeholders if needed
			        }
                },
                { "Cube", new ColumnConfig[]
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
            public static readonly Dictionary<string, ColumnConfig[]> SrcCellColumns = new Dictionary<string, ColumnConfig[]>
            {
                { "Table", new ColumnConfig[]
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
                { "Cube", new ColumnConfig[]
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