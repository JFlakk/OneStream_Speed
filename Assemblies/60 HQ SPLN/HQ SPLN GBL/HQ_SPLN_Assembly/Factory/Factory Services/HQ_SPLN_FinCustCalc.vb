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
					Me.Copy_CMD_SPLN()
				ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyLocal") Then
					Me.CopyLocal()
				End If
				
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub


		Public Sub Copy_CMD_SPLN()				
			Dim sAPPN As String  = args.CustomCalculateArgs.NameValuePairs.XFGetValue("APPN")
			Dim wfYear As String = api.Time.GetYearFromId(api.Pov.Time.MemberId)
			Dim srcacctFilter = "A#Funded_Commitments.Base,A#Funded_Obligations.Base"
			If Not sAPPN.XFContainsIgnoreCase("OMA") Then
				srcacctFilter = "A#Funded_Obligations.Base"
			End If
			'Dim srcFlowFilter = "F#L2_Final_SPLN"
			Dim srcu1Filter = $"U1#{sAPPN}.Base.Options(Cube=Army,ScenarioType=Plan,MergeMembersFromReferencedCubes=False)"
			Dim srcu3Filter = $"U3#{sAPPN}.Base.Options(Cube=Army,ScenarioType=Plan,MergeMembersFromReferencedCubes=False)"
			Dim srcu6Filter = "U6#CostCat.Base.Options(Cube=Army,ScenarioType=Plan,MergeMembersFromReferencedCubes=False)"
			Dim destacctFilter = ",[A#Commitments,A#Obligations]"
			If Not sAPPN.XFContainsIgnoreCase("OMA") Then
				destacctFilter = ",[A#Obligations]"
			End If
			Dim destFlowFilter = ",[F#Copy_Adj]"
			Dim destu1Filter = $",[U1#{sAPPN}.Base.Options(Cube=Army,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
			Dim destu2Filter = ",[U2#None]"
			If Not sAPPN.XFContainsIgnoreCase("OMA") Then
				destu2Filter = String.Empty
			End If
			Dim destu3Filter = $",[U3#{sAPPN}.Base.Options(Cube=Army,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
			Dim destu4Filter = ",[U4#None]"			
			If Not sAPPN.XFContainsIgnoreCase("OMA") Then
				destu4Filter = String.Empty
			End If
			Dim destu5Filter = ",[U5#None]"
			Dim destu6Filter = ",[U6#CostCat.Base.Options(Cube=Army,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
			Dim destu7Filter = ",[U7#None]"
			Dim destu8Filter = ",[U8#None]"
			Dim src_DataBuffer As New DataBuffer()
			Dim hqSPLN_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As New DataBuffer() 
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()		
			'-------------------------------------------------------------------------------------------------------------------------------------------------	

			CurrCubeBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic){destacctFilter}{destFlowFilter}
																										 {destu1Filter}{destu2Filter}{destu3Filter}
																										 {destu4Filter}{destu5Filter}{destu6Filter}
																										 {destu7Filter}{destu8Filter})")

			Dim commDims As String = $"V#Periodic:C#Aggregated:O#Top:S#CMD_SPLN_C{wfYear}:F#L2_Final_SPLN:U2#Top:U4#Top:U5#Top:U7#Top"
			If Not sAPPN.XFContainsIgnoreCase("OMA") Then
				commDims = $"V#Periodic:C#Aggregated:O#Top:S#CMD_SPLN_C{wfYear}:F#L2_Final_SPLN:U5#Top:U7#Top"
			End If
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({commDims}),[{srcacctFilter}],[{srcu1Filter}],[{srcu3Filter}],[{srcu6Filter}])")
			hqSPLN_DataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, api.Pov.Entity.Name, $"CMD_SPLN_C{wfYear}", src_DataBuffer)
			For Each hqSPLN_DataCell As DataBufferCell In hqSPLN_DataBuffer.DataBufferCells.Values
				Dim acctName = "Obligations"
				Dim mdepName = "None"
				Dim dollarTypeName = "None"
				If hqSPLN_DataCell.GetAccountName(api).XFContainsIgnoreCase("Commitments")
					acctName = "Commitments"
				End If
				If Not sAPPN.XFContainsIgnoreCase("OMA") Then
					mdepName = hqSPLN_DataCell.GetUD2Name(api)
					dollarTypeName = hqSPLN_DataCell.GetUD4Name(api) 
				End If
				Dim destCell As New DataBufferCell(UpdateCellDefinition(hqSPLN_DataCell,acctName,"Copy_Adj","AdjInput","None",,mdepName,,dollarTypeName,"None",,"None",))

				UpdateValue(destCell, CurrCubeBuffer, destBuffer, destCell.CellAmount)
				CurrCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk)				
			Next


			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next
			CurrCubeBuffer.DataBufferCells.Clear()
		
			'-------------------------------------------------------------------------------------------------------------------------------------------------
			'Commit to Cube
			'-------------------------------------------------------------------------------------------------------------------------------------------------
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,,True)
			destBuffer.DataBufferCells.Clear()
			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			api.Data.SetDataBuffer(ClearCubeData, ClearInfo)

		End Sub
		
		Private Sub CopyLocal()
			Dim runType = args.CustomCalculateArgs.NameValuePairs.XFGetValue("runType","NA")
			Dim destFlow = "Init_Copy"
			If runType = "Adjust SPLN"
				destFlow = "Adjustment"
			End If
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[F#{destFlow}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			Dim Src_Data As String = $"C#Local:V#Periodic:F#Copy_Adj"
			If runType = "Adjust SPLN"
				Src_Data = $"C#Local:V#Periodic:F#Copy_Adj - C#Aggregated:V#Periodic:F#Init_Copy"
			End If
	
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}))")
			src_DataBuffer.LogDataBuffer(api,"src",200)
			For Each Src_DataCell As DataBufferCell In Src_DataBuffer.DataBufferCells.Values
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_DataCell,,destFlow))
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