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
    public class FMM_Global_Buffers : IWsasFinanceCustomCalculateV800
    {
        public void CustomCalculate(SessionInfo si, BRGlobals brGlobals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                if (args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("GetCVDataBuffer"))
                {
                    var filter = globals.GetStringValue("Filter");

                    DataBuffer dynDataBuffer = api.Data.GetDataBufferUsingFormula(filter);
                    var dynResults = new Dictionary<MemberScriptBuilder, DataBufferCell>();

                    foreach (var cell in dynDataBuffer.DataBufferCells.Values)
                    {
                        var msb = cell.DataBufferCellPk.CreateMemberScriptBuilder(api);
                        dynResults[msb] = cell;
                    }

                    globals.SetObject("Results", dynResults);
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}