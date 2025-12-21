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
Imports Microsoft.Data.SqlClient

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class HQ_SPLN_FinCustCalc
		Implements IWsasFinanceCustomCalculateV800
		
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As FinanceRulesApi
		Public args As FinanceRulesArgs
		Public Global_Functions As OneStream.BusinessRule.Finance.Global_Functions.MainClass

        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
#Region "General Initialization"
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Me.Global_Functions = New OneStream.BusinessRule.Finance.Global_Functions.MainClass(si,globals,api,args)
#End Region
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Copy_CMD_SPLN") Then
					Me.Copy_CMD_SPLN_OMA()
					Me.Copy_CMD_SPLN_OPA()
					Me.Copy_CMD_SPLN_RDTE()
				End If
				
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

		Public Sub Copy_CMD_SPLN_OMA()
			Dim wfYear As String = api.Time.GetYearFromId(api.Pov.Time.MemberId)
			Dim originFilter = "O#AdjInput"
			Dim srcacctFilter = "A#Funded_Commitments.Base,A#Funded_Obligations.Base"
			Dim srcFlowFilter = "F#L2_Finalized_SPLN"
			Dim srcu1Filter = "U1#OMA.Base.Options(Cube=Army,ScenarioType=Plan,MergeMembersFromReferencedCubes=False)"
			Dim srcu3Filter = "U3#OMA.Base.Options(Cube=Army,ScenarioType=Plan,MergeMembersFromReferencedCubes=False)"
			Dim destacctFilter = "A#Commitments,A#Obligations"
			Dim destFlowFilter = "F#Copy_Adj"
			Dim destu1Filter = "U1#OMA.Base.Options(Cube=Army,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)"
			Dim destu2Filter = "U2#None"
			Dim destu3Filter = "U3#OMA.Base.Options(Cube=Army,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)"			
			Dim destu4Filter = "U4#None"
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[{originFilter}],[{destFlowFilter}],[{destu1Filter}],[{destu2Filter}],[{destu4Filter}],[{destacctFilter}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			Dim srcCommDims As String = "V#Periodic:C#Aggregated:O#Top:S#CMD_SPLN_C{wfYear}:U2#Top:U4#Top:U5#Top:U7#Top"
			
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({srcCommDims}),[{srcFlowFilter}],[{srcacctFilter}])")
			Dim hqSPLN_DataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, api.Pov.Entity.Name, $"CMD_SPLN_C{wfYear}", src_DataBuffer)
			For Each hqSPLN_DataCell As DataBufferCell In hqSPLN_DataBuffer.DataBufferCells.Values
				Dim acctName = "Obligations"
				If hqSPLN_DataCell.GetAccountName(api).XFContainsIgnoreCase("Commitments")
					acctName = "Commitments"
				End If
				Dim destCell As New DataBufferCell(UpdateCellDefinition(hqSPLN_DataCell,acctName,"Copy_Adj","AdjInput","None",,,,,,,,))

				UpdateValue(destCell, CurrCubeBuffer, destBuffer, destCell.CellAmount)
				CurrCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk)				
			Next
			
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,,True)
			destBuffer.DataBufferCells.Clear()

			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next

			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
		End Sub
		
		Public Sub Copy_CMD_SPLN_OPA()
			Dim originFilter = "O#AdjInput"
			Dim destFlowFilter = "F#Copy_Adj"
			Dim srcFlowFilter = "F#L2_Finalized_SPLN"
			Dim destacctFilter = "A#Commitments,A#Obligations"
			Dim srcacctFilter = "A#Funded_Commitments.Base,A#Funded_Obligations.Base"
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[{originFilter}],[{destFlowFilter}],[{destacctFilter}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic:C#Aggregated:O#Top:S#CMD_SPLN_C{api.Time.GetYearFromId(api.Pov.Time.MemberId)}),[{srcFlowFilter}],[{srcacctFilter}])")
			Dim hqSPLN_DataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, api.Pov.Entity.Name, $"CMD_SPLN_C{api.Time.GetYearFromId(api.Pov.Time.MemberId)}", src_DataBuffer)
			For Each hqSPLN_DataCell As DataBufferCell In hqSPLN_DataBuffer.DataBufferCells.Values
				Dim acctName = "Obligations"
				If hqSPLN_DataCell.GetAccountName(api).XFContainsIgnoreCase("Commitments")
					acctName = "Commitments"
				End If
				Dim destCell As New DataBufferCell(UpdateCellDefinition(hqSPLN_DataCell,acctName,"Copy_Adj","AdjInput","None",,,,,,,,))

				UpdateValue(destCell, CurrCubeBuffer, destBuffer, destCell.CellAmount)
				CurrCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk)				
			Next
			
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,,True)
			destBuffer.DataBufferCells.Clear()

			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next

			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
		End Sub
		
		Public Sub Copy_CMD_SPLN_RDTE()
			Dim originFilter = "O#AdjInput"
			Dim destFlowFilter = "F#Copy_Adj"
			Dim srcFlowFilter = "F#L2_Finalized_SPLN"
			Dim destacctFilter = "A#Commitments,A#Obligations"
			Dim srcacctFilter = "A#Funded_Commitments.Base,A#Funded_Obligations.Base"
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[{originFilter}],[{destFlowFilter}],[{destacctFilter}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic:C#Aggregated:O#Top:S#CMD_SPLN_C{api.Time.GetYearFromId(api.Pov.Time.MemberId)}),[{srcFlowFilter}],[{srcacctFilter}])")
			Dim hqSPLN_DataBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, api.Pov.Entity.Name, $"CMD_SPLN_C{api.Time.GetYearFromId(api.Pov.Time.MemberId)}", src_DataBuffer)
			For Each hqSPLN_DataCell As DataBufferCell In hqSPLN_DataBuffer.DataBufferCells.Values
				Dim acctName = "Obligations"
				If hqSPLN_DataCell.GetAccountName(api).XFContainsIgnoreCase("Commitments")
					acctName = "Commitments"
				End If
				Dim destCell As New DataBufferCell(UpdateCellDefinition(hqSPLN_DataCell,acctName,"Copy_Adj","AdjInput","None",,,,,,,,))

				UpdateValue(destCell, CurrCubeBuffer, destBuffer, destCell.CellAmount)
				CurrCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk)				
			Next
			
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,,True)
			destBuffer.DataBufferCells.Clear()

			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next

			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
		End Sub
		
		Private Sub CopyLocal()
	'BRApi.ErrorLog.LogMessage(si, $"Hit CopyLocal - {api.Pov.Entity.Name}")
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
			Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[F#Init_Copy,F#Adjustment])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			Dim consMbr As String = "Local"
					
			Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, api.Pov.Entity.MemberId)
			If Ent_Base Then
				consMbr = "Aggregated"
			End If
			
			Dim Src_Data As String = $"E#{api.pov.Entity.name}:S#{api.pov.scenario.name}:C#{consMbr}:V#Periodic"
	
			Src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}),[F#Init_Copy,F#Adjustment])")
			For Each Src_DataCell As DataBufferCell In Src_DataBuffer.DataBufferCells.Values
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_DataCell))
				UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Destcell.CellAmount)
				CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
			Next
			
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,,True)
			destBuffer.DataBufferCells.Clear()
	
			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next
	
			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
		End Sub

#Region "Helper Functions"		
		''-------------------------------------------------------------------------		
		Public Function UpdateCellDefinition(ByRef destcell As DataBufferCell,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As DatabufferCell
		
			Dim BufferCell = Global_Functions.UpdateCellDefinition(destcell,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
			Return BufferCell
			
	    End Function
				
		''-------------------------------------------------------------------------
		Public Function GetBCValue(ByRef srccell As DataBufferCell,ByRef DriverDB As DataBuffer,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As Decimal
			
			Return Global_Functions.GetBCValue(srccell,DriverDB,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
			
		End Function
		
		''-------------------------------------------------------------------------
		Public Sub UpdateValue(ByRef srcCell As DataBufferCell, ByRef currCellDB As DataBuffer, ByRef destDB As DataBuffer, ByVal value As Decimal)
			
			Global_Functions.UpdateValue(srccell,currCellDB,destDB,value)

		End Sub
		
#End Region
			
				
	End Class

	
	
	
	
End Namespace
