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

        public FMM_Src_CellModel() : this(null)
        {
        }

        public FMM_Src_CellModel(IEnumerable<string> enabledDimensions)
        {
            Type = OpenParens = MathOperator = CloseParens = string.Empty;
            Entity = Cons = Scenario = Time = View = Acct = IC = Origin = Flow = string.Empty;
            UD1 = UD2 = UD3 = UD4 = UD5 = UD6 = UD7 = UD8 = string.Empty;
            
            _dimensionValues = enabledDimensions?
                .ToDictionary(d => d, _ => string.Empty, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int CubeID { get; set; }
        public int ActID { get; set; }
        public int ModelID { get; set; }
        public int CalcID { get; set; }
        public int CellID { get; set; }
        public int Order { get; set; }

        public string Type { get; set; }
        public string ItemType { get; set; }
        public string OpenParens { get; set; }
        public string MathOperator { get; set; }
        public string CloseParens { get; set; }

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