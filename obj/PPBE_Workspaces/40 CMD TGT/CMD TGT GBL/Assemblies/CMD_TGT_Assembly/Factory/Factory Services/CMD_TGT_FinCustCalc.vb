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

				Select Case args.CustomCalculateArgs.FunctionName.ToLower()
					Case "copycprobetotarget"
						Me.CopycPROBEtoTGT()
					Case "loaddisttocube"
						Me.Load_TGT_Data_to_Cube("Dist")
					Case "procdistout"
						Me.Process_TGT_Dist_Out()
					Case "loadwhtocube"
						Me.Load_TGT_Data_to_Cube("WH")
				End Select

            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub
		
#Region "CopycPROBEtoTGT"		
		Private Sub CopycPROBEtoTGT()
			Dim apiCube = api.Pov.Cube.Name

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
			
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(RemoveNoData(V#Periodic),[A#Ctrl_Target_In])")
			Dim destBuffer As DataBuffer = New DataBuffer(currCubeBuffer.CommonDataBufferCellPk)

            Dim ClearCubeData As DataBuffer = New DataBuffer()
			
			Dim Src_Calc As String = $"Cb#{apiCube}:E#{apiCube}:S#{scenario}:T#{apiYear}:A#BO1:O#BeforeAdj:I#Top:F#Total_Position:U5#Top:U6#Top:U7#Top"
			Dim Src_CalcBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(" & Src_Calc & ")")
			Src_CalcBuffer.LogDataBuffer(api,"SRC",200)

			For Each Src_CalcCell As DataBufferCell In Src_CalcBuffer.DataBufferCells.Values
				Dim destU1 As String
				If dictU1Mapping.ContainsKey(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower) Then
					destU1 = dictU1Mapping(Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & Src_CalcCell.DataBufferCellPk.GetUD4Name(api).ToLower)
				Else
					destU1 = Src_CalcCell.DataBufferCellPk.GetUD1Name(api).ToLower & "_General"
				End If
				
				Dim Destcell As New DataBufferCell(UpdateCellDefinition(Src_CalcCell,"Ctrl_Target_In","Baseline","Forms","None",destU1,,,,"None","None","None"))

				UpdateValue(Destcell, CurrCubeBuffer, destBuffer, Src_CalcCell.CellAmount)
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
#End Region

#Region "Load_TGT_Data_to_Cube"
		Public Sub Load_TGT_Data_to_Cube(ByVal loadType As String)
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			Dim POVPeriodNum As Integer = api.Time.GetPeriodNumFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim POVYear As Integer = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim StatusbyFundsCenter As String = globals.GetStringValue($"FundsCenterStatusUpdates - {api.pov.entity.name}",String.Empty)
			If StatusbyFundsCenter <> String.Empty
				StatusbyFundsCenter = $",[{StatusbyFundsCenter}]"
			End If
			
			Dim TGT_Dist_DT = Brapi.Utilities.GetSessionDataTable(si,si.UserName, "CMD_PGM_Import")
			
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[O#Import]{StatusbyFundsCenter})")
	
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			'Select form the DataTable
            'Filter rows by FundsCenter = current POV entity and loop them into a DataBuffer
            Dim filteredRows = TGT_Dist_DT.AsEnumerable().
                Where(Function(r) Not IsDBNull(r("FundsCenter")) AndAlso
                                  String.Equals(r("FundsCenter").ToString(), api.Pov.Entity.Name, StringComparison.OrdinalIgnoreCase)).
                ToList()

            For Each row As DataRow In filteredRows
                Dim cellPk As New DataBufferCellPk()
                Dim status As New DataCellStatus(True)
                Dim dbCell As New DataBufferCell(cellPk, 0D, status)

                'Set common dimension values if present in the datatable
                If TGT_Dist_DT.Columns.Contains("Account") AndAlso Not IsDBNull(row("Account")) Then
                    cellPk.SetAccount(api, row("Account").ToString())
                End If
                If TGT_Dist_DT.Columns.Contains("IC") AndAlso Not IsDBNull(row("IC")) Then
                    cellPk.SetIC(api, row("IC").ToString())
                End If
                If TGT_Dist_DT.Columns.Contains("Flow") AndAlso Not IsDBNull(row("Flow")) Then
                    cellPk.SetFlow(api, row("Flow").ToString())
                End If

                'Set origin to Import (matches prior behaviour)
                cellPk.SetOrigin(api, "Import")

                'Set UD1..UD8 if they exist
                For i As Integer = 1 To 8
                    Dim colName = $"UD{i}"
                    If TGT_Dist_DT.Columns.Contains(colName) AndAlso Not IsDBNull(row(colName)) Then
                        Select Case i
                            Case 1 : cellPk.SetUD1(api, row(colName).ToString())
                            Case 2 : cellPk.SetUD2(api, row(colName).ToString())
                            Case 3 : cellPk.SetUD3(api, row(colName).ToString())
                            Case 4 : cellPk.SetUD4(api, row(colName).ToString())
                            Case 5 : cellPk.SetUD5(api, row(colName).ToString())
                            Case 6 : cellPk.SetUD6(api, row(colName).ToString())
                            Case 7 : cellPk.SetUD7(api, row(colName).ToString())
                            Case 8 : cellPk.SetUD8(api, row(colName).ToString())
                        End Select
                    End If
                Next

                'Determine amount: prefer period column FY_<POVPeriodNum>, fallback to common amount columns
                Dim amount As Decimal = 0D
                Dim periodCol As String = $"FY_{POVPeriodNum}"
                If TGT_Dist_DT.Columns.Contains(periodCol) AndAlso Not IsDBNull(row(periodCol)) Then
                    Decimal.TryParse(row(periodCol).ToString(), amount)
                ElseIf TGT_Dist_DT.Columns.Contains("Amount") AndAlso Not IsDBNull(row("Amount")) Then
                    Decimal.TryParse(row("Amount").ToString(), amount)
                ElseIf TGT_Dist_DT.Columns.Contains("Tot_Annual") AndAlso Not IsDBNull(row("Tot_Annual")) Then
                    Decimal.TryParse(row("Tot_Annual").ToString(), amount)
                End If

                'Only add non-zero amounts
                If amount <> 0D Then
                    dbCell.CellAmount = amount
                    destBuffer.DataBufferCells.Add(dbCell.DataBufferCellPk, dbCell)

                    'Remove any matching cell from current cube buffer so it will be cleared later (keeps behaviour consistent with other code)
                    If CurrCubeBuffer.DataBufferCells.ContainsKey(dbCell.DataBufferCellPk) Then
                        CurrCubeBuffer.DataBufferCells.Remove(dbCell.DataBufferCellPk)
                    End If
                End If
            Next
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
