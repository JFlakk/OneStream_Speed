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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_TGT_FinCustCalc
		Implements IWsasFinanceCustomCalculateV800
		
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As FinanceRulesApi
		Public args As FinanceRulesArgs
		Public Global_Functions As OneStream.BusinessRule.Finance.Global_Functions.MainClass

        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
#Region "General Initialization"
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Me.Global_Functions = New OneStream.BusinessRule.Finance.Global_Functions.MainClass(si,globals,api,args)
#End Region
'BRApi.ErrorLog.LogMessage(SI, $"args.CustomCalculateArgs.FunctionName: {args.CustomCalculateArgs.FunctionName} - {api.pov.entity.name}")
				Select Case args.CustomCalculateArgs.FunctionName
					Case "CopycPROBEtoTGT"
						Me.CopycPROBEtoTGT()
					Case "Load_TGT_Dist_to_Cube"
						Me.Load_TGT_Data_to_Cube("TGT_DIST")
					Case "Load_WH_to_Cube"
						Me.Load_TGT_Data_to_Cube("TGT_WH")
					Case "ProcessDistOut"
						Me.ProcessDistOut()
					Case "CopyLocal"
						Me.CopyLocal()
					Case "Consol_Aggregated"
						Me.Consol_Aggregated()
				End Select

            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub
		
#Region "CopycPROBEtoTGT"		
		Private Sub CopycPROBEtoTGT()
			Dim apiCube = api.Pov.Cube.Name
			Dim src_DataBuffer As New DataBuffer()

			Dim apiYear As Integer = TimeDimHelper.GetYearFromId(api.Pov.Time.MemberId)	
			Dim apiYear2Char As String = Strings.Right(apiYear, 2) 
			Dim scenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("cPROBEscen")
		
			'Build SQL to return list of requirements that fit the user's criterias
			Dim SQL As String
			SQL = $"SELECT Appropriation_CD, Dollar_Type, Partial_Fund_CD  
					FROM XFC_APPN_Mapping"

			'Fetch mapping table 
			Dim dtU1Mappings As New DataTable
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 dtU1Mappings = BRApi.Database.ExecuteSql(dbConn,SQL,True)
			End Using

			'Loop through the fetched datatable and load to dictionary
			Dim dictU1Mapping As Dictionary(Of String, String) = New Dictionary(Of String, String)
			For Each dataRow As DataRow In dtU1Mappings.Rows		
				Dim sAppn As String =  dataRow.Item("Appropriation_CD").tolower	
				Dim sDlrTyp As String =  dataRow.Item("Dollar_Type").tolower		
				Dim sPartFund As String =  dataRow.Item("Partial_Fund_CD")	
			
				If Not dictU1Mapping.ContainsKey(sAppn & sDlrTyp) Then 
					dictU1Mapping.Add(sAppn & sDlrTyp, sPartFund & apiYear2Char )
				End If
			Next
			
			Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,api.Pov.Entity.Name,api.Pov.Time.Name)
			If Val_Approach("CMD_Val_Pay_NonPay_Approach") = "Yes" Then
				Dim Src_Data As String = $"E#{apiCube}:S#{scenario}:C#Aggregated:V#Periodic:O#Top:I#Top:F#Tot_Position:U5#Top:U6#Top:U7#Top:U8#Top"
				Src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}),[A#BO5])")
			End If
'BRAPI.ErrorLog.LogMessage(si,$"hit {api.pov.entity.name} - {Val_Approach("CMD_Val_Pay_NonPay_Approach")}")
			
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(V#Periodic),[A#Target],[O#AdjInput],[F#L2_Ctrl_Intermediate])")

			Dim destBuffer As DataBuffer = New DataBuffer(currCubeBuffer.CommonDataBufferCellPk)

            Dim ClearCubeData As DataBuffer = New DataBuffer()
		
			Dim Src_Calc As String = $"E#{apiCube}:S#{scenario}:C#Aggregated:V#Periodic:O#Top:I#Top:A#BO1:F#Tot_Position:U5#Top:U6#Top:U7#Top:U8#Top"
'brapi.ErrorLog.LogMessage(si,"srcCalc: " & Src_Calc)
			Dim Src_CalcBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(" & Src_Calc & ")")
'Src_CalcBuffer.LogDataBuffer(api,$"srccalcbuffer_",500)
			For Each Src_CalcCell As DataBufferCell In Src_CalcBuffer.DataBufferCells.Values
				Dim destU1 As String
				If dictU1Mapping.ContainsKey(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower) Then
					destU1 = dictU1Mapping(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower)
				Else
					destU1 = Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & "_General"
				End If
'brapi.ErrorLog.LogMessage(si,"DestU1: " & destU1)				
				If Val_Approach("CMD_Val_Pay_NonPay_Approach") = "Yes" Then
					
					Dim Pay_TGT As Decimal = GetBCValue(Src_CalcCell,Src_DataBuffer,"BO5")
					Dim NonPay_TGT = Src_CalcCell.CellAmount-Pay_TGT
					
					Dim DestNonPaycell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","Non_Pay","None","None"))
	
					UpdateValue(DestNonPaycell, CurrCubeBuffer, destBuffer, NonPay_TGT)
					CurrCubeBuffer.DataBufferCells.Remove(DestNonPaycell.DataBufferCellPk)
					
					Dim DestPaycell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","Pay_Benefits","None","None"))
	
					UpdateValue(DestPaycell, CurrCubeBuffer, destBuffer, Pay_TGT)
					CurrCubeBuffer.DataBufferCells.Remove(DestPaycell.DataBufferCellPk)

				Else
					Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","CostCat_General","None","None"))
	
					UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Src_CalcCell.CellAmount)
					CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
				End If

			Next

			'DestBuffer.LogDataBuffer(api,"Dest",500)
			'CurrCubeBuffer.LogDataBuffer(api,"Curr",500)
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
#End Region

#Region "Load_TGT_Data_to_Cube"
		Public Sub Load_TGT_Data_to_Cube(ByVal loadType As String)			
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			Dim tgt_Acct As String = "Target"
			Dim tgt_Flow As String = "L3_Ctrl_Intermediate"
			Dim tgt_Origin As String = "AdjInput"
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")	
			Dim EntBase = Not BRApi.Finance.Members.HasChildren(si, entDimPk, api.Pov.Entity.MemberId)
			Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
			If EntityLevel.XFContainsIgnoreCase("L3") And EntBase = True And loadType = "TGT_DIST"
				tgt_Flow = "L3_Dist_Final"
				tgt_Origin = "Import"
			Else If EntityLevel.XFContainsIgnoreCase("L4") And EntBase = True And loadType = "TGT_DIST"
				tgt_Flow = "L4_Dist_Final"
				tgt_Origin = "Import"
			Else If EntityLevel.XFContainsIgnoreCase("L4") And EntBase = False And loadType = "TGT_DIST"
				tgt_Flow = "L4_Ctrl_Intermediate"
			Else If EntityLevel.XFContainsIgnoreCase("L5") And EntBase = True And loadType = "TGT_DIST"
				tgt_Flow = "L5_Dist_Final"
				tgt_Origin = "Import"
			Else If EntityLevel.XFContainsIgnoreCase("L2") And EntBase = False And loadType = "TGT_WH"
				tgt_Flow = "L2_Dist_Final"
				tgt_Acct = "TGT_WH"
			Else If EntityLevel.XFContainsIgnoreCase("L3") And EntBase = False And loadType = "TGT_WH"
				tgt_Flow = "L3_Dist_Final"
				tgt_Acct = "TGT_WH"
			Else If EntityLevel.XFContainsIgnoreCase("L4") And EntBase = False And loadType = "TGT_WH"
				tgt_Flow = "L4_Dist_Final"
				tgt_Acct = "TGT_WH"
			End If
			Dim TGT_Import_DT As DataTable = Brapi.Utilities.GetSessionDataTable(si,si.UserName, $"CMD_TGT_Import_{loadType}_{api.pov.scenario.Name}")	
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[A#{tgt_Acct}],[O#{tgt_Origin}],[F#{tgt_Flow}])")
'currcubebuffer.LogDataBuffer(api,"currcubebuffer - ", 500)
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			'Select form the DataTable
            'Filter rows by FundsCenter = current POV entity and loop them into a DataBuffer
            Dim filteredRows = TGT_Import_DT.AsEnumerable().
                Where(Function(r) Not IsDBNull(r("FundsCenter")) AndAlso
                                  String.Equals(r("FundsCenter").ToString(), api.Pov.Entity.Name, StringComparison.OrdinalIgnoreCase)).ToList()
            For Each row As DataRow In filteredRows
                Dim tgtcellPk As New DataBufferCellPk()
                Dim status As New DataCellStatus(True)
                Dim tgtDataCell As New DataBufferCell(tgtcellPk, 0, status)				
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(tgtDataCell,tgt_Acct,tgt_Flow,tgt_Origin,"None",row("FundsCode").ToString(),row("MDEP").ToString(),row("APE9").ToString(),row("DollarType").ToString,"None",row("CostCat").ToString,"None","None"))
'brapi.ErrorLog.LogMessage(si,"hit 1: " & row("APE9").ToString())
                'Determine amount: prefer period column FY_<POVPeriodNum>, fallback to common amount column	
				If Not IsDBNull(row("Amount")) AndAlso row("Amount") <> 0 Then
					Destcell.CellAmount = Convert.ToDecimal(row("Amount"))
					UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Destcell.CellAmount)
					CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
				End If
            Next
			'destBuffer.LogDataBuffer(api,"Dest",600)
			'Delete from the DatatABLE
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			If destBuffer.DataBufferCells.Count > 0 Then
				api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, True)
				destBuffer.DataBufferCells.Clear()
			End If

			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next

			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			If ClearCubeData.DataBufferCells.Count > 0 Then
				api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
			End If
		
		End Sub
#End Region

	Private Sub ProcessDistOut()
		Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")	
		Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, api.Pov.Entity.MemberId)
		If Not Ent_Base Then
'brapi.ErrorLog.LogMessage(si,"Entity tgt: " & api.Pov.Entity.Name)
			Dim src_DataBuffer As New DataBuffer()
			Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
			Dim srcflowMbr As String = "F#L3_Ctrl_Intermediate,F#L2_Dist_Final,F#L3_Dist_Final"
			Dim destflowMbr As String = "L2_Dist_Intermediate_Out"
			Dim originFilter = "O#AdjInput,O#AdjConsolidated,O#Forms,O#Import"
			Dim acctFilter = "A#TGT_Target_WH.Base"
			Select Case entityLevel
				Case "L3"
					srcflowMbr = "F#L4_Ctrl_Intermediate,F#L3_Dist_Final,F#L4_Dist_Final"
					destflowMbr = "L3_Dist_Intermediate_Out"
				Case "L4"
					srcflowMbr = "F#L4_Dist_Final,F#L5_Dist_Final"
					destflowMbr = "L4_Dist_Intermediate_Out"
			End Select
'brapi.ErrorLog.LogMessage(si,"srcflowMbr" & srcflowMbr)
'brapi.ErrorLog.LogMessage(si,"destflowMbr" & destflowMbr)
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(cb#{api.pov.Cube.Name}:E#{api.Pov.Entity.Name}:V#Periodic:C#Aggregated),[{originFilter}],[F#{destflowMbr}],[{acctFilter}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
	
			
			Dim Src_Data As String = $"V#Periodic:C#Aggregated"

			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(cb#{api.pov.Cube.Name}:E#{api.Pov.Entity.Name}:V#Periodic:C#Aggregated),[{originFilter}],[{srcflowMbr}],[{acctFilter}])")
			'CurrCubeBuffer.LogDataBuffer(api,$"{api.pov.entity.name}_CurrBuffer",500)
'src_DataBuffer.LogDataBuffer(api,$"{api.pov.entity.name}_SrcBuffer",500)
			For Each Src_DataCell As DataBufferCell In src_DataBuffer.DataBufferCells.Values
				If (Src_DataCell.DataBufferCellPk.GetAccountName(api).XFEqualsIgnoreCase("TGT_WH") And Src_DataCell.DataBufferCellPk.GetFlowName(api).XFEqualsIgnoreCase($"{entityLevel}_Dist_Final")) Or
					Src_DataCell.DataBufferCellPk.GetAccountName(api).XFEqualsIgnoreCase("Target") Then
					Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_DataCell,,destflowMbr,"AdjInput"))
					UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Destcell.CellAmount)
					CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
				End If
			Next
			
'destBuffer.LogDataBuffer(api,$"{api.pov.entity.name}_destBuffer",500)
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
		End If
		
	End Sub
	
	Private Sub CopyLocal()
		Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
		Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
		Dim src_DataBuffer As New DataBuffer()
		Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[O#AdjInput],[F#{entityLevel}_Ctrl_Intermediate,F#{entityLevel}_Dist_Final])")
		Dim destBuffer As DataBuffer = New DataBuffer()
		Dim ClearCubeData As DataBuffer = New DataBuffer()
		Dim consMbr As String = "Local"
				
		Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, api.Pov.Entity.MemberId)
		If Not Ent_Base Then
		
			Dim Src_Data As String = $"C#{consMbr}:V#Periodic"
	
			Src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}),[O#AdjInput],[F#{entityLevel}_Ctrl_Intermediate,F#{entityLevel}_Dist_Final])")
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
			
		End If
	End Sub
	
	Private Sub Consol_Aggregated()
		Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
		Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
		Dim entityID As Integer = api.Members.GetMemberId(dimType.Entity.Id,api.Pov.Entity.Name)
		Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)
		If Not Ent_Base
			Dim Src_Data = String.Empty
			Dim acctFilter = "A#Target,A#TGT_WH"
			Dim originFilter = "O#AdjConsolidated,O#Forms,O#Import"
			Dim flowFilter = "F#L3_Dist_Final,F#L4_Dist_Final,F#L5_Dist_Final,F#L3_Dist_Intermediate_Out,F#L4_Dist_Intermediate_Out,F#L3_Ctrl_Intermediate,F#L4_Ctrl_Intermediate"
			Select Case entityLevel
				Case "L3"
					flowFilter = "F#L4_Dist_Final,F#L5_Dist_Final,F#L4_Dist_Intermediate_Out,F#L4_Ctrl_Intermediate"
				Case "L4"
					flowFilter = "F#L5_Dist_Final"
			End Select
			Dim src_DataBuffer As New DataBuffer()
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData(V#Periodic),[{originFilter}],[{flowFilter}],[{acctFilter}])")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{api.Pov.Entity.Name}.Children", True)
			For Each Ent As MemberInfo In entList
				If Src_Data.Length > 0
					Src_Data = $"{Src_Data} + cb#{api.Pov.Cube.Name}:E#{Ent.Member.Name}:C#Aggregated:V#Periodic"
				Else
					Src_Data = $"cb#{api.Pov.Cube.Name}:E#{Ent.Member.Name}:C#Aggregated:V#Periodic"
				End If
			Next
			
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}),[O#AdjInput,O#Forms,O#Import],[{flowFilter}],[{acctFilter}])")
			
			For Each Src_DataCell As DataBufferCell In Src_DataBuffer.DataBufferCells.Values
				Dim destOrigin = Src_DataCell.GetOriginName(api)
				If Src_DataCell.GetOriginName(api).XFEqualsIgnoreCase("AdjInput")
					destOrigin = "AdjConsolidated"
				End If
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_DataCell,,,destOrigin))
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
		End If
		
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