Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_TGT_WH
		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)

		Public Sub New()
			Dim command As New GBL_ColumnMap("ACOM","CMD",GetType(String),True,"WFCube",String.Empty,String.Empty,30)
			ColumnMaps.Add(command)
            Dim fiscalyear As New GBL_ColumnMap("FiscalYear","FiscalYear",GetType(String),True,"WFTime",String.Empty,String.Empty,4)
			ColumnMaps.Add(fiscalyear)
            Dim fundscenter As New GBL_ColumnMap("FundCenter","FundsCenter",GetType(String),True,"Ent_Member","WFCube","E#",100)
			ColumnMaps.Add(fundscenter)
            Dim fundcode As New GBL_ColumnMap("FundCode","FundsCode",GetType(String),True,"Member","U1_FundCode","U1#",100)
			ColumnMaps.Add(fundcode)
            Dim mdep As New GBL_ColumnMap("MDEP","MDEP",GetType(String),True,"Member","U2_MDEP","U2#",100)
			ColumnMaps.Add(mdep)
            Dim ape9 As New GBL_ColumnMap("APE(9)","APE9",GetType(String),True,"U3_Member","U3_All_APE","U3#",100,"FundCode")
			ColumnMaps.Add(ape9)
            Dim dollarType As New GBL_ColumnMap("DollarType","DollarType",GetType(String),True,"Member","U4_DollarType","U4#",100)
			ColumnMaps.Add(dollarType)
            Dim costCat As New GBL_ColumnMap("CostCat","CostCat",GetType(String),True,"Member","U6_CostCat","U6#",100)
			ColumnMaps.Add(costCat)
            Dim amount As New GBL_ColumnMap("Amount","Amount",GetType(Decimal),True,"Numeric",String.Empty,String.Empty,100)
			ColumnMaps.Add(amount)
		End Sub
	End Class
End Namespace