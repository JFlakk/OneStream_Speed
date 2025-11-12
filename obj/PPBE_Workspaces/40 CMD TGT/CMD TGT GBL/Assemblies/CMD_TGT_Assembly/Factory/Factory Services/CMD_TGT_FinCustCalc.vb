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
BRApi.ErrorLog.LogMessage(SI, "args.CustomCalculateArgs.FunctionName.ToLower() " & args.CustomCalculateArgs.FunctionName.ToLower())
				Select Case args.CustomCalculateArgs.FunctionName.ToLower()
					Case "copycprobetotarget"
						Me.CopycPROBEtoTGT()
					Case "tgtdst_load_targets_to_cube"
						Me.Load_TGT_Data_to_Cube("TGT_Dist")
					Case "loadwhtocube"
						Me.Load_TGT_Data_to_Cube("WH")
					Case "procdistout"
						Me.Process_TGT_Dist_Out()
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
			brapi.ErrorLog.LogMessage(si,$"Hit {apiCube} - {api.pov.scenario.Name} - {api.pov.entity.name}")
		
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
			
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(V#Periodic),[A#Target],[O#AdjInput],[F#L2_Ctrl_Intermediate])")
			Dim destBuffer As DataBuffer = New DataBuffer(currCubeBuffer.CommonDataBufferCellPk)

            Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			Dim Src_Calc As String = $"E#{apiCube}:S#{scenario}:C#Aggregated:V#Periodic:O#Top:I#Top:A#BO1:F#Tot_Position:U5#Top:U6#Top:U7#Top:U8#Top"
			BRAPI.ErrorLog.LogMessage(SI, $"Hit: {Src_Calc}")
			Dim Src_CalcBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(" & Src_Calc & ")")
			Src_CalcBuffer.LogDataBuffer(api,$"SRC: {api.pov.entity.Name}",200)

			For Each Src_CalcCell As DataBufferCell In Src_CalcBuffer.DataBufferCells.Values
				Dim destU1 As String
				If dictU1Mapping.ContainsKey(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower) Then
					destU1 = dictU1Mapping(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower)
				Else
					destU1 = Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & "_General"
				End If
				
				If Val_Approach("CMD_Val_Pay_NonPay_Approach") = "Yes" Then
					
					Dim Pay_TGT As Decimal = GetBCValue(Src_CalcCell,Src_DataBuffer,"BO5")
					Dim NonPay_TGT = Src_CalcCell.CellAmount-Pay_TGT
					
					Dim DestNonPaycell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","Non_Pay","None","None"))
	
					UpdateValue(DestNonPaycell, CurrCubeBuffer, destBuffer, NonPay_TGT)
					CurrCubeBuffer.DataBufferCells.Remove(DestNonPaycell.DataBufferCellPk)
					
					Dim DestPaycell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","Pay_Benefits","None","None"))
	
					UpdateValue(DestPaycell, CurrCubeBuffer, destBuffer, Pay_TGT)
					CurrCubeBuffer.DataBufferCells.Remove(DestPaycell.DataBufferCellPk)
					BRAPI.ErrorLog.LogMessage(SI, $"Hit: {Src_CalcCell.CellAmount} - {Pay_TGT}")

				Else
					Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Target","L2_Ctrl_Intermediate","AdjInput","None",destU1,,,,"None","CostCat_General","None","None"))
	
					UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Src_CalcCell.CellAmount)
					CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
				End If

			Next

			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
destBuffer.LogDataBuffer(api,$"Dest: {api.pov.Cons.Name}",200)
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
			Dim tgt_Flow As String = "L3_Dist_Intermediate_In"
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
				tgt_Flow = "L4_Dist_Intermediate_In"
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
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(tgtDataCell,tgt_Acct,tgt_Flow,tgt_Origin,"None",row("FundsCode").ToString(),row("MDEP").ToString(),row("APE9").ToString(),"None","None","None","None"))
                'Determine amount: prefer period column FY_<POVPeriodNum>, fallback to common amount column	
				If Not IsDBNull(row("Amount")) AndAlso row("Amount") <> 0 Then
					Destcell.CellAmount = Convert.ToDecimal(row("Amount"))
					destBuffer.DataBufferCells.Add(Destcell.DataBufferCellPk,Destcell)
					CurrCubeBuffer.DataBufferCells.Remove(Destcell.DataBufferCellPk)
				End If
            Next
			'Delete from the DatatABLE
'destBuffer.LogDataBuffer(api,"@ DataBuffer: " & api.Pov.Entity.Name & " : "  & api.Pov.Time.Name ,1000)
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

	Private Sub Process_TGT_Dist_Out()
		
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
