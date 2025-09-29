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
	Public Class CMD_TGT_Dist
		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)

		Public Sub New()
			Dim command As New GBL_ColumnMap("ACOM","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(command)
            Dim fiscalyear As New GBL_ColumnMap("FiscalYear","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(fiscalyear)
            Dim fundscenter As New GBL_ColumnMap("FundCenter","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(fundscenter)
            Dim mdep As New GBL_ColumnMap("MDEP","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(mdep)
            Dim ape9 As New GBL_ColumnMap("APE(9)","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(ape9)
            Dim amount As New GBL_ColumnMap("Amount","CMD",GetType(String),"WFCube",String.Empty,String.Empty,100)
			ColumnMaps.Add(amount)
		End Sub
	End Class
End Namespace