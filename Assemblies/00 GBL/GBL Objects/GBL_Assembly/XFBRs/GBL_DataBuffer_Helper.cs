using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.GBL_DataBuffer_Helper
{
    /// <summary>
    /// Parameterized DataBuffer Helper - C# Implementation
    /// 
    /// Provides a highly configurable interface for working with databuffers in OneStream.
    /// This class consolidates common patterns found across multiple assembly-specific databuffer XFBRs
    /// into a single, reusable, and easily configurable function.
    /// 
    /// Key Features:
    /// - Flexible filter expression building
    /// - Dynamic dimension clearing and setting
    /// - Member transformation capabilities (ancestor lookups, etc.)
    /// - Flexible sorting and output formatting
    /// - Reduces code duplication across assemblies
    /// </summary>
    public class MainClass
    {
        private SessionInfo si;
        private BRGlobals globals;
        private object api;
        private DashboardStringFunctionArgs args;

        public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;

                switch (args.FunctionName)
                {
                    case "GetDataBufferMembers":
                        return GetDataBufferMembers();
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        #region GetDataBufferMembers - Parameterized DataBuffer Function

        /// <summary>
        /// Parameterized function to retrieve data buffer members with flexible configuration.
        /// 
        /// Parameters (passed via args.NameValuePairs):
        /// 
        /// Core Filter Parameters:
        /// - FilterExpression: The data buffer filter expression (required)
        /// - Entity: Entity member(s), comma-separated for multiple (optional, can be part of FilterExpression)
        /// - Scenario: Scenario member (optional, can be part of FilterExpression)
        /// - Time: Time member(s), comma-separated for multiple (optional, can be part of FilterExpression)
        /// - Account: Account member (optional, can be part of FilterExpression)
        /// - Cube: Cube name (optional, defaults to workflow cube)
        /// 
        /// Dimension Clear Parameters (set to "True" to clear):
        /// - ClearScenario: Clear scenario dimension from results
        /// - ClearEntity: Clear entity dimension from results
        /// - ClearAccount: Clear account dimension from results
        /// - ClearFlow: Clear flow dimension from results
        /// - ClearOrigin: Clear origin dimension from results
        /// - ClearIC: Clear IC dimension from results
        /// - ClearUD1 through ClearUD8: Clear UD1-UD8 dimensions from results
        /// 
        /// Dimension Set Parameters (set dimension to specific value):
        /// - SetEntity: Set entity to specific value
        /// - SetUD1 through SetUD8: Set UD1-UD8 to specific values
        /// 
        /// Member Transformation Parameters:
        /// - TransformUD1: Ancestor transformation for UD1 (e.g., "U1_APPN" to get APPN ancestor)
        /// - TransformUD2: Ancestor transformation for UD2 (e.g., "U2_PEG" to get PEG ancestor)
        /// - TransformUD3: Ancestor transformation for UD3 (e.g., "U3_SAG" to get SAG ancestor)
        /// - TransformUD1Filter: Member filter for UD1 (optional custom filter)
        /// - TransformUD2Filter: Member filter for UD2 (optional custom filter)
        /// - TransformUD3Filter: Member filter for UD3 (optional custom filter)
        /// 
        /// Output Parameters:
        /// - SortBy: Sort expression (e.g., "E#{entity},U3#{UD3},U1#{UD1}")
        /// - SortOrder: "Ascending" or "Descending" (default: Descending)
        /// - DefaultOutput: Default output when no results (default: "U5#One")
        /// - OutputFormat: "MemberScript" (default) or "Custom"
        /// - CustomOutputFormat: Custom format string (e.g., "U1#{UD1}:U3#{UD3}")
        /// 
        /// Returns: Comma-separated list of member scripts or custom formatted output
        /// </summary>
        public string GetDataBufferMembers()
        {
            try
            {
                // Get filter expression (required)
                string filterExpr = args.NameValuePairs.XFGetValue("FilterExpression", "");
                if (string.IsNullOrWhiteSpace(filterExpr))
                {
                    return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One");
                }

                // Optional: Build filter from components if not provided directly
                if (filterExpr.XFEqualsIgnoreCase("BUILD"))
                {
                    filterExpr = BuildFilterFromComponents();
                }

                // Set filter and execute GetCVDataBuffer
                globals.SetStringValue("Filter", filterExpr);
                GetDataBuffer(si, globals, api, args);

                // Check if results exist
                if (globals.GetObject("Results") == null)
                {
                    return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One");
                }

                Dictionary<MemberScriptBuilder, DataBufferCell> results = 
                    (Dictionary<MemberScriptBuilder, DataBufferCell>)globals.GetObject("Results");
                
                if (results.Count == 0)
                {
                    return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One");
                }

                // Process results
                Dictionary<string, string> toSort = new Dictionary<string, string>();

                foreach (MemberScriptBuilder msb in results.Keys)
                {
                    // Clear dimensions as specified
                    ApplyDimensionClearing(msb);

                    // Set dimensions to specific values
                    ApplyDimensionSetting(msb);

                    // Apply member transformations
                    ApplyMemberTransformations(msb);

                    // Build sort key
                    string sortKey = BuildSortKey(msb);

                    // Add to dictionary if not duplicate
                    if (!toSort.ContainsKey(msb.GetMemberScript))
                    {
                        toSort.Add(msb.GetMemberScript, sortKey);
                    }
                }

                // Sort results
                Dictionary<string, string> sorted;
                string sortOrder = args.NameValuePairs.XFGetValue("SortOrder", "Descending");

                if (sortOrder.XFEqualsIgnoreCase("Ascending"))
                {
                    sorted = toSort.OrderBy(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                }
                else
                {
                    sorted = toSort.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, y => y.Value);
                }

                // Build output
                StringBuilder output = new StringBuilder();
                string outputFormat = args.NameValuePairs.XFGetValue("OutputFormat", "MemberScript");

                foreach (KeyValuePair<string, string> item in sorted)
                {
                    if (outputFormat.XFEqualsIgnoreCase("MemberScript"))
                    {
                        output.Append(item.Key).Append(",");
                    }
                    else
                    {
                        // Custom format - will be implemented based on needs
                        output.Append(item.Key).Append(",");
                    }
                }

                // Return results or default
                if (output.Length == 0)
                {
                    return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One");
                }

                return output.ToString();
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }

        /// <summary>
        /// Builds a filter expression from component parameters
        /// </summary>
        private string BuildFilterFromComponents()
        {
            string sCube = args.NameValuePairs.XFGetValue("Cube", "");
            if (string.IsNullOrWhiteSpace(sCube))
            {
                sCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName;
            }

            string entities = args.NameValuePairs.XFGetValue("Entity", "");
            string scenario = args.NameValuePairs.XFGetValue("Scenario", "");
            string times = args.NameValuePairs.XFGetValue("Time", "");
            string account = args.NameValuePairs.XFGetValue("Account", "");
            string baseFilter = args.NameValuePairs.XFGetValue("BaseFilter", "");

            List<string> filterList = new List<string>();
            List<string> lEntity = StringHelper.SplitString(entities, ",");
            List<string> lTime = StringHelper.SplitString(times, ",");

            // Build filter for each entity/time combination
            foreach (string e in lEntity)
            {
                foreach (string tm in lTime)
                {
                    string filterPart = baseFilter;
                    if (string.IsNullOrWhiteSpace(filterPart))
                    {
                        filterPart = $"Cb#{sCube}:C#Aggregated:S#{scenario}:T#{tm}:E#[{e}]:A#{account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None";
                    }
                    else
                    {
                        // Replace placeholders
                        filterPart = filterPart.Replace("{Cube}", sCube);
                        filterPart = filterPart.Replace("{Entity}", e);
                        filterPart = filterPart.Replace("{Scenario}", scenario);
                        filterPart = filterPart.Replace("{Time}", tm);
                        filterPart = filterPart.Replace("{Account}", account);
                    }
                    filterList.Add(filterPart);
                }
            }

            // Combine with REMOVENODATA
            string combinedFilter = string.Join(" + ", filterList);
            string additionalFilters = args.NameValuePairs.XFGetValue("AdditionalFilters", "");

            if (!string.IsNullOrWhiteSpace(additionalFilters))
            {
                return $"FilterMembers(REMOVENODATA({combinedFilter}),{additionalFilters})";
            }
            else
            {
                return $"REMOVENODATA({combinedFilter})";
            }
        }

        /// <summary>
        /// Clears dimensions based on parameters
        /// </summary>
        private void ApplyDimensionClearing(MemberScriptBuilder msb)
        {
            if (args.NameValuePairs.XFGetValue("ClearScenario", "").XFEqualsIgnoreCase("True"))
            {
                msb.Scenario = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearEntity", "").XFEqualsIgnoreCase("True"))
            {
                msb.Entity = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearAccount", "").XFEqualsIgnoreCase("True"))
            {
                msb.Account = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearFlow", "").XFEqualsIgnoreCase("True"))
            {
                msb.Flow = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearOrigin", "").XFEqualsIgnoreCase("True"))
            {
                msb.Origin = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearIC", "").XFEqualsIgnoreCase("True"))
            {
                msb.IC = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD1", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD1 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD2", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD2 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD3", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD3 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD4", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD4 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD5", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD5 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD6", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD6 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD7", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD7 = string.Empty;
            }
            if (args.NameValuePairs.XFGetValue("ClearUD8", "").XFEqualsIgnoreCase("True"))
            {
                msb.UD8 = string.Empty;
            }
        }

        /// <summary>
        /// Sets dimensions to specific values based on parameters
        /// </summary>
        private void ApplyDimensionSetting(MemberScriptBuilder msb)
        {
            string setValue;

            setValue = args.NameValuePairs.XFGetValue("SetEntity", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.Entity = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD1", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD1 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD2", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD2 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD3", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD3 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD4", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD4 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD5", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD5 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD6", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD6 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD7", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD7 = setValue;
            }

            setValue = args.NameValuePairs.XFGetValue("SetUD8", "");
            if (!string.IsNullOrWhiteSpace(setValue))
            {
                msb.UD8 = setValue;
            }
        }

        /// <summary>
        /// Applies member transformations (ancestor lookups, etc.)
        /// </summary>
        private void ApplyMemberTransformations(MemberScriptBuilder msb)
        {
            // Transform UD1
            string transformUD1 = args.NameValuePairs.XFGetValue("TransformUD1", "");
            if (!string.IsNullOrWhiteSpace(transformUD1) && !string.IsNullOrWhiteSpace(msb.UD1))
            {
                string ud1Filter = args.NameValuePairs.XFGetValue("TransformUD1Filter", "");
                if (string.IsNullOrWhiteSpace(ud1Filter))
                {
                    ud1Filter = $".Ancestors.Where(MemberDim = {transformUD1})";
                }

                DimPK objUD1DimPK = BRApi.Finance.Dim.GetDimPk(si, transformUD1);
                List<MemberInfo> lsAncestorList = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD1DimPK, $"U1#{msb.UD1}{ud1Filter}", true);
                if (lsAncestorList != null && lsAncestorList.Count > 0)
                {
                    msb.UD1 = lsAncestorList[0].Member.Name;
                }
            }

            // Transform UD2
            string transformUD2 = args.NameValuePairs.XFGetValue("TransformUD2", "");
            if (!string.IsNullOrWhiteSpace(transformUD2) && !string.IsNullOrWhiteSpace(msb.UD2))
            {
                string ud2Filter = args.NameValuePairs.XFGetValue("TransformUD2Filter", "");
                if (string.IsNullOrWhiteSpace(ud2Filter))
                {
                    ud2Filter = $".Ancestors.Where(MemberDim = {transformUD2})";
                }

                DimPK objUD2DimPK = BRApi.Finance.Dim.GetDimPk(si, transformUD2);
                List<MemberInfo> lsAncestorList = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD2DimPK, $"U2#{msb.UD2}{ud2Filter}", true);
                if (lsAncestorList != null && lsAncestorList.Count > 0)
                {
                    msb.UD2 = lsAncestorList[0].Member.Name;
                }
            }

            // Transform UD3
            string transformUD3 = args.NameValuePairs.XFGetValue("TransformUD3", "");
            if (!string.IsNullOrWhiteSpace(transformUD3) && !string.IsNullOrWhiteSpace(msb.UD3))
            {
                string ud3Filter = args.NameValuePairs.XFGetValue("TransformUD3Filter", "");
                if (string.IsNullOrWhiteSpace(ud3Filter))
                {
                    ud3Filter = $".Ancestors.Where(MemberDim = {transformUD3})";
                }

                DimPK objUD3DimPK = BRApi.Finance.Dim.GetDimPk(si, transformUD3);
                List<MemberInfo> lsAncestorList = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD3DimPK, $"U3#{msb.UD3}{ud3Filter}", true);
                if (lsAncestorList != null && lsAncestorList.Count > 0)
                {
                    msb.UD3 = lsAncestorList[0].Member.Name;
                }
            }
        }

        /// <summary>
        /// Builds sort key for ordering results
        /// </summary>
        private string BuildSortKey(MemberScriptBuilder msb)
        {
            string sortBy = args.NameValuePairs.XFGetValue("SortBy", "");

            if (string.IsNullOrWhiteSpace(sortBy))
            {
                // Default sort key
                return $"E#{msb.Entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3}";
            }

            // Replace placeholders in sort expression
            string sortKey = sortBy;
            sortKey = sortKey.Replace("{entity}", msb.Entity);
            sortKey = sortKey.Replace("{scenario}", msb.Scenario);
            sortKey = sortKey.Replace("{account}", msb.Account);
            sortKey = sortKey.Replace("{flow}", msb.Flow);
            sortKey = sortKey.Replace("{origin}", msb.Origin);
            sortKey = sortKey.Replace("{ic}", msb.IC);
            sortKey = sortKey.Replace("{UD1}", msb.UD1);
            sortKey = sortKey.Replace("{UD2}", msb.UD2);
            sortKey = sortKey.Replace("{UD3}", msb.UD3);
            sortKey = sortKey.Replace("{UD4}", msb.UD4);
            sortKey = sortKey.Replace("{UD5}", msb.UD5);
            sortKey = sortKey.Replace("{UD6}", msb.UD6);
            sortKey = sortKey.Replace("{UD7}", msb.UD7);
            sortKey = sortKey.Replace("{UD8}", msb.UD8);

            return sortKey;
        }

        #endregion

        #region Utilities: Get DataBuffer

        /// <summary>
        /// Executes the GetCVDataBuffer business rule to populate the databuffer results
        /// </summary>
        public void GetDataBuffer(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "Global_Buffers", "GetCVDataBuffer", dictionary, CustomCalculateTimeType.CurrentPeriod);
        }

        #endregion
    }
}
