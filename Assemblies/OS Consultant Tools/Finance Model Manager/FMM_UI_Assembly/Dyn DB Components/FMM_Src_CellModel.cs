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
        // Core identification fields - always present
        public int Cell_ID { get; set; }
        public int Cube_ID { get; set; }
        public int Act_ID { get; set; }
        public int Model_ID { get; set; }
        public int Calc_ID { get; set; }
        public int Src_Order { get; set; }
        
        // Common fields for source cells
        public string Src_Type { get; set; }
        public string Src_Item { get; set; }
        public string Open_Parens { get; set; }
        public string Math_Operator { get; set; }
        public string Close_Parens { get; set; }
        
        // Dimension fields - may or may not be present depending on CalcType
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
        
        // Additional fields for specific calc types
        public string DB_Name { get; set; }
        public string Dyn_Calc_Script { get; set; }
        public string Unbal_Src_Cell_Buffer { get; set; }
        public string Unbal_Src_Cell_Buffer_Filter { get; set; }
        public string Unbal_Buffer_Filter { get; set; }
        public string Override_Value { get; set; }
        
        // Unbalanced dimension overrides
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
        
        // Audit fields
        public DateTime? Create_Date { get; set; }
        public string Create_User { get; set; }
        public DateTime? Update_Date { get; set; }
        public string Update_User { get; set; }
    }
}
