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
    public class FMM_Src_CellModel
    {
        private readonly Dictionary<string, string> _dimensionValues;

        public FMM_Src_CellModel(IEnumerable<string> enabledDimensions)
        {
            Src_Type = Src_Item = Open_Parens = Math_Operator = Close_Parens = string.Empty;
            Entity = Cons = Scenario = Time = View = Acct = IC = Origin = Flow = string.Empty;
            UD1 = UD2 = UD3 = UD4 = UD5 = UD6 = UD7 = UD8 = string.Empty;
            DB_Name = Dyn_Calc_Script = Unbal_Src_Cell_Buffer = Unbal_Src_Cell_Buffer_Filter = Unbal_Buffer_Filter = Override_Value = string.Empty;
            Unbal_Acct_Override = Unbal_Origin_Override = Unbal_Flow_Override = Unbal_IC_Override = string.Empty;
            Unbal_UD1_Override = Unbal_UD2_Override = Unbal_UD3_Override = Unbal_UD4_Override = Unbal_UD5_Override = Unbal_UD6_Override = Unbal_UD7_Override = Unbal_UD8_Override = string.Empty;
            Create_User = Update_User = string.Empty;

            _dimensionValues = enabledDimensions?
                .ToDictionary(d => d, _ => string.Empty, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int Cell_ID { get; set; }
        public int Cube_ID { get; set; }
        public int Act_ID { get; set; }
        public int Model_ID { get; set; }
        public int Calc_ID { get; set; }
        public int Src_Order { get; set; }

        public string Src_Type { get; set; }
        public string Src_Item { get; set; }
        public string Open_Parens { get; set; }
        public string Math_Operator { get; set; }
        public string Close_Parens { get; set; }

        public string Entity { get; set; }
        public string Cons { get; set; }
        public string Scenario { get; set; }
        public string Time { get; set; }
        public string View { get; set; }
        public string Acct { get; set; }
        public string IC { get; set; }
        public string Origin { get; set; }
        public string Flow { get; set; }
        public string UD1 { get; set; }
        public string UD2 { get; set; }
        public string UD3 { get; set; }
        public string UD4 { get; set; }
        public string UD5 { get; set; }
        public string UD6 { get; set; }
        public string UD7 { get; set; }
        public string UD8 { get; set; }

        public string DB_Name { get; set; }
        public string Dyn_Calc_Script { get; set; }
        public string Unbal_Src_Cell_Buffer { get; set; }
        public string Unbal_Src_Cell_Buffer_Filter { get; set; }
        public string Unbal_Buffer_Filter { get; set; }
        public string Override_Value { get; set; }

        public string Unbal_Acct_Override { get; set; }
        public string Unbal_Origin_Override { get; set; }
        public string Unbal_Flow_Override { get; set; }
        public string Unbal_IC_Override { get; set; }
        public string Unbal_UD1_Override { get; set; }
        public string Unbal_UD2_Override { get; set; }
        public string Unbal_UD3_Override { get; set; }
        public string Unbal_UD4_Override { get; set; }
        public string Unbal_UD5_Override { get; set; }
        public string Unbal_UD6_Override { get; set; }
        public string Unbal_UD7_Override { get; set; }
        public string Unbal_UD8_Override { get; set; }

        public DateTime? Create_Date { get; set; }
        public string Create_User { get; set; }
        public DateTime? Update_Date { get; set; }
        public string Update_User { get; set; }

        // Dynamic accessor based on passed-in dimension list
        public string this[string dimension]
        {
            get => _dimensionValues.TryGetValue(dimension, out var value) ? value : string.Empty;
            set
            {
                if (_dimensionValues.ContainsKey(dimension))
                {
                    _dimensionValues[dimension] = value;
                }
            }
        }
    }
}
