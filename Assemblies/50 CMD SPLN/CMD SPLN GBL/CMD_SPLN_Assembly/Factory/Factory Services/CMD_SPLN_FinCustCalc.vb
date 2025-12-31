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
	Public Class CMD_SPLN_FinCustCalc
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

				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Load_Reqs_to_Cube") Then				
					Me.Load_Reqs_to_Cube()
				End If
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Copy_CivPay") Then
'brapi.ErrorLog.LogMessage(si,"CivPay calc call main")
					Me.Copy_CivPay(globals)
				End If
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Copy_Withhold") Then
'brapi.ErrorLog.LogMessage(si,"Withhold Calc call main")
					Me.Copy_Withhold(globals)
				End If
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Consol_Aggregated") Then
'brapi.ErrorLog.LogMessage(si,"Withhold Calc call main")
					Me.Consol_Aggregated()
				End If


            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

#Region "Load_Reqs_to_Cube"
		Public Sub Load_Reqs_to_Cube()		
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			if api.Pov.Entity.Name.XFContainsIgnoreCase("None") then return
			Dim POVPeriodNum As Integer = api.Time.GetPeriodNumFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim POVYear As Integer = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim wfStartTimeID As Integer = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId)
			Dim wfStartYear As String = api.Time.GetNameFromId(wfStartTimeID)
		    Dim table_Month As String = api.Pov.Time.Name.Replace(POVYear & "M","")
			Dim parentFCList As New List(Of String)
			Dim baseFCList As New List(Of String)
'			If table_Month = 1 Then
'brapi.ErrorLog.LogMessage(si,"entity fincalc = " & api.Pov.Entity.Name)
'End If
			Dim sumcolumn As String = String.Empty
			If POVYear = wfStartYear.Substring(0,4) Then 
				sumcolumn = "Month" & table_Month
			Else 
				sumcolumn = "Yearly"
			End If 
			Dim statusinClause As String
		
			Dim tgt_Origin As String = "Import"
			Dim origin_Filter As String = "O#Import"
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")	
			Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
			Dim entityID As Integer = api.Members.GetMemberId(dimType.Entity.Id,api.Pov.Entity.Name)
			Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)
			
			If Not Ent_Base Then
				tgt_Origin = "AdjInput"
				origin_Filter = "O#AdjInput,O#AdjConsolidated,O#Import"
				If (globals.GetObject($"{api.pov.entity.name}_baseFCList") Is Nothing) Then
					Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{api.Pov.Entity.Name}.Descendants", True)
					For Each ent As MemberInfo In entList
						If Not BRApi.Finance.Members.HasChildren(si, entDimPk,ent.Member.MemberId)
							baseFCList.Add(ent.Member.Name)
						Else
							parentFCList.Add(ent.Member.Name)
						End If
					Next
					If parentFCList.Count > 0
						globals.SetObject($"{api.pov.entity.name}_parentFCList",parentFCList)
					End If
					If baseFCList.Count > 0
						globals.SetObject($"{api.pov.entity.name}_baseFCList",baseFCList)
					End If
				Else
					baseFCList = globals.GetObject($"{api.pov.entity.name}_baseFCList")
					If Not (globals.GetObject($"{api.pov.entity.name}_parentFCList") Is Nothing) Then
						parentFCList = globals.GetObject($"{api.pov.entity.name}_parentFCList")
					End If
				End If
			End If
						
			Dim StatusbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusUpdates - {api.pov.entity.name}",String.Empty)
			Dim appnbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusappnUpdates - {api.pov.entity.name}",String.Empty)
'If table_Month = 1 Then
'			brAPI.ERRORLOG.LogMessage(SI,"StatusbyFundsCenter:" & api.Pov.Entity.Name & ": "& StatusbyFundsCenter)
'			brAPI.ERRORLOG.LogMessage(SI,"appnbyFundsCenter:" & api.Pov.Entity.Name & ": "& appnbyFundsCenter)
'End If
			Dim StatusList As New List(Of String) 
			If StatusbyFundsCenter.xfcontainsIgnoreCase("|")
				
				StatusList = StringHelper.SplitString(StatusbyFundsCenter,"|")
				Dim prefixedStatuses As List(Of String) = StatusList.Select(Function(status) $"F#{status}").ToList()
				statusinClause = String.Join(",", StatusList.Select(Function(status) $"'{status}'"))
	
				
				If StatusbyFundsCenter <> String.Empty
					StatusbyFundsCenter = $",[{String.Join(", ", prefixedStatuses)}]"
				End If
			Else
				statusinClause = $"'{StatusbyFundsCenter}'"
				StatusbyFundsCenter = $",[{StatusbyFundsCenter}]"
			End If
			
			Dim finalUD3WhereClause As String
			Dim appnList As New List(Of String) 
			If appnbyFundsCenter.xfcontainsIgnoreCase("|")
				
				appnList = StringHelper.SplitString(appnbyFundsCenter,"|")
				Dim prefixedAppn As List(Of String) = appnList.Select(Function(appn) $"U3#{appn}.Base").ToList()
				Dim appnUD3whereList As New List(Of String)
				appnUD3whereList = appnList.Select(Function(appn) $"UD3 LIKE '{appn}%'").ToList()

				finalUD3WhereClause = $"({String.Join(" OR ", appnUD3whereList)})"
	
				If appnbyFundsCenter <> String.Empty
					appnbyFundsCenter = $",[{String.Join(", ", prefixedAppn)}]"
				End If
			Else
				finalUD3WhereClause = $"UD3 LIKE '{appnbyFundsCenter}%'"
				appnbyFundsCenter = $",[U3#{appnbyFundsCenter}.Base]"
			End If
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[{origin_Filter}]{StatusbyFundsCenter}{appnbyFundsCenter})")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
			'brapi.ErrorLog.LogMessage(Si,"filter: " & $"FilterMembers(REMOVEZeros(V#Periodic),[{origin_Filter}]{StatusbyFundsCenter}{appnbyFundsCenter})")
'currcubebuffer.LogDataBuffer(api,"currcube orig = " & api.Pov.Entity.name,500)		
			'Define the SQL Statement 
			Dim sql = $"SELECT Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, '{tgt_origin}' as Origin, SUM({sumcolumn}) AS Tot_Annual
						FROM XFC_CMD_SPLN_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						and fiscal_year = '{POVYear}'
						AND Entity = '{api.Pov.Entity.Name}'
						AND Account In ('Commitments','Obligations','WH_Commitments','WH_Obligations')
						AND Flow In ({statusinClause})
						AND ({finalUD3WhereClause})
						Group By Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			If parentFCList.Count > 0 Then
				Dim parentFCFilter = $"('{String.Join("','", parentFCList)}')"
				sql = $"{sql} UNION ALL
						SELECT '{api.Pov.Entity.Name}' as Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, 'AdjConsolidated' as Origin, SUM({sumcolumn}) AS Tot_Annual
						FROM XFC_CMD_SPLN_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						and fiscal_year = '{POVYear}'
						AND Entity In {parentFCFilter}
						AND Account In ('Commitments','Obligations','WH_Commitments','WH_Obligations')
						AND Flow In ({statusinClause})
						AND ({finalUD3WhereClause})
						Group By Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			End If
			If baseFCList.Count > 0 Then
				Dim baseFCFilter = $"('{String.Join("','", baseFCList)}')"
				sql = $"{sql} UNION ALL
						SELECT '{api.Pov.Entity.Name}' as Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, 'Import' as Origin, SUM({sumcolumn}) AS Tot_Annual
						FROM XFC_CMD_SPLN_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						and fiscal_year = '{POVYear}'
						AND Entity In {baseFCFilter}
						AND Account In ('Commitments','Obligations','WH_Commitments','WH_Obligations')
						AND Flow In ({statusinClause})
						AND ({finalUD3WhereClause})
						Group By Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			End If
'If table_Month = 1 Then
'			brAPI.ERRORLOG.LogMessage(SI,"hIT:" & SQL)
'End If

			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using reader As DataTableReader = BRApi.Database.ExecuteSqlUsingReader(dbConnApp, sql, False).CreateDataReader
					If reader.HasRows Then
						While reader.Read()
							Dim ReqDataCellpk As New DataBufferCellPk()
							Dim Status As New DataCellStatus(True)
							Dim ReqDataCell As New DataBufferCell(ReqDataCellpk, 0, Status)
							
							ReqDataCell.DataBufferCellPk.SetAccount(api, reader("Account").ToString())
							ReqDataCell.DataBufferCellPk.SetFlow(api, reader("Flow").ToString())
							ReqDataCell.DataBufferCellPk.SetIC(api, reader("IC").ToString())
							ReqDataCell.DataBufferCellPk.SetOrigin(api, reader("Origin").ToString())
							ReqDataCell.DataBufferCellPk.SetUD1(api, reader("UD1").ToString())
							ReqDataCell.DataBufferCellPk.SetUD2(api, reader("UD2").ToString())
							ReqDataCell.DataBufferCellPk.SetUD3(api, reader("UD3").ToString())
							ReqDataCell.DataBufferCellPk.SetUD4(api, reader("UD4").ToString())
							ReqDataCell.DataBufferCellPk.SetUD5(api, reader("UD5").ToString())
							ReqDataCell.DataBufferCellPk.SetUD6(api, reader("UD6").ToString())
							ReqDataCell.DataBufferCellPk.SetUD7(api, reader("UD7").ToString())
							ReqDataCell.DataBufferCellPk.SetUD8(api, reader("UD8").ToString())
	
							If Not IsDBNull(reader("Tot_Annual")) AndAlso reader("Tot_Annual") <> 0 Then
								UpdateValue(ReqDataCell, CurrCubeBuffer, destBuffer, Convert.ToDecimal(reader("Tot_Annual")))
								CurrCubeBuffer.DataBufferCells.Remove(ReqDataCell.DataBufferCellPk)
							End If
						End While	
					End If
				End Using
			End Using
destBuffer.LogDataBuffer(api,"destBuffer = " & api.Pov.Entity.name,500)
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			If destBuffer.DataBufferCells.Count > 0 Then
				api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, True)
				destBuffer.DataBufferCells.Clear()
			End If
			
currcubebuffer.LogDataBuffer(api,"currcube = " & api.Pov.Entity.name,500)
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


#Region "Copy Civ Pay and Withhold"
		Public Sub Copy_CivPay_Withhold(ByVal globals As BRGlobals, ByRef req_id_Type As String)
			Try	
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd As String = wfInfoDetails("CMDName")	
				Dim sSourcePosition As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As String = ""' args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				Dim sTargetScenario As String = "CMD_SPLN_C" & tm
				Dim currentYear As Integer = Convert.ToInt32(tm)
				Dim nextyear As Integer = currentYear  + 1
				'Dim sTargetFirstYear As String = api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim sTargetYear As Integer =tm.XFConvertToInt' api.Pov.Time.Name.XFConvertToInt
				Dim sEntity As String = api.Pov.Entity.Name
				Dim levl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim statuslvl As String = levl & "_Formulate_SPLN"
				
				If Not (req_id_Type.XFEqualsIgnoreCase("CivPay_Copied") Or req_id_Type.XFEqualsIgnoreCase("withhold")) Then
					
				End If
				
				Dim sSrcMbrScript = ""
				If req_id_Type.XFEqualsIgnoreCase("CivPay_Copied")
					sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])"
				ElseIf req_id_Type.XFEqualsIgnoreCase("withhold")
					sSrcMbrScript = $"FilterMembers(RemoveZeros(Cb#{cmd}:E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#USD:V#Periodic:F#Tot_Dist_Final:O#AdjInput:I#None:U7#Top:U8#None),[A#TGT_WH])"
				End If
				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				Dim REQDT As DataTable = New DataTable("REQDT")
				
				Dim sqa As New SqlDataAdapter()
				Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
						'Get header
						Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					    Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
						
						Dim SqlREQ As String = $"SELECT * 
								     				   FROM XFC_CMD_SPLN_REQ WITH (NOLOCK)
											           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
											           And WFTime_Name = '{tm}'
											           AND WFScenario_Name = '{sTargetScenario}'
											           AND ENTITY = '{api.pov.Entity.Name}'
													   and REQ_ID_Type = '{req_id_Type}' "
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & 	SqlREQ)					
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
						sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
						
						Dim cmd_req_ids As New List(Of String) 
						Dim whereClause As String = " 1 = 2"
						If Not REQDT Is Nothing And REQDT.Rows.Count > 0 Then
							For Each r As DataRow In REQDT.Rows
								cmd_req_ids.Add($"'{r("CMD_SPLN_REQ_ID").ToString}'")
							Next
							 whereClause = $"CMD_SPLN_REQ_ID In ({String.Join(",", cmd_req_ids)})"
						End If
					
					    'Details
						Dim SqlREQDetail As String = $"SELECT *  
								     				   FROM XFC_CMD_SPLN_REQ_Details WITH (NOLOCK)
											           WHERE {whereClause}"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & 	SqlREQDetail)	

						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						
						REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("Entity"),
												 REQDetailDT.Columns("Fiscal_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD5"),
												 REQDetailDT.Columns("Account"),
												 REQDetailDT.Columns("CMD_SPLN_REQ_ID")
												 }
												 
						For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
							Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
							Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
							Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
							Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
							Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
							Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
							Dim APPN As String = UD3.Split("_")(0)
							If  (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General"))  Then
								UD6= "Pay_General"
							Else
								UD6 = "NonPay_General"
							End If
							
							'Dim Acct = "Obligations"  'Target account?
		 					Dim UoM = "Funding"
							Dim parentRow As DataRow = Nothing
							Dim obligRow As DataRow = Nothing
							Dim comtRow As DataRow = Nothing
							Dim carryOverObligRow As DataRow = Nothing 
							Dim carryOverComtRow As DataRow = Nothing
							Dim insertCivPay, insertParent, insertoblig, insertComit, insertCarryover As Boolean 
							
							
							
							'--Handle CivPay
							'Each commt and oblig row is paired with a parent row.
							'Civpay copied is only created through this process
#Region "CivPay_Copied"							
							If req_id_Type.XFEqualsIgnoreCase("CivPay_Copied")
								
								'Get data from db for each funding line in the data buffer
								If Not REQDetailDT Is Nothing And REQDetailDT.Rows.Count > 0 Then
									'parentRow = GetParentRow(REQDetailDT, tm, "CivPay_Copied", FundingLineCell)
									obligRow = GetDetailRow(REQDetailDT, tm, "Obligations", FundingLineCell, "CivPay_Copied")
									comtRow = GetDetailRow(REQDetailDT, tm, "Commitments",FundingLineCell, "CivPay_Copied")
									
									carryOverObligRow = GetDetailRow(REQDetailDT, nextyear, "Obligations", FundingLineCell, "CivPay_Copied")
									carryOverComtRow = GetDetailRow(REQDetailDT, nextyear, "Commitments", FundingLineCell, "CivPay_Copied")
								
									
								End If
	
								If obligRow Is Nothing Then
									insertCivPay = True
									
									parentRow = REQDT.NewRow()
									Me.Populate_REQ(parentRow, FundingLineCell, insertCivPay)
									'REQDT.Rows.Add(parentRow)
									obligRow = REQDetailDT.NewRow()
									comtRow = REQDetailDT.NewRow()
									
								Else
									parentRow = REQDT.Select("CMD_SPLN_REQ_ID = " & "'" & obligRow("CMD_SPLN_REQ_ID").ToString & "'").FirstOrDefault()
								End If 
								
								'Dim isinsertCarryOver = False
								If carryOverObligRow Is Nothing Then
									insertCarryover = True
									carryOverObligRow = REQDetailDT.NewRow()
									carryOverComtRow = REQDetailDT.NewRow()
								End If
								
								'Me.Populate_REQ(parentRow, FundingLineCell, isinsert)'*Dead Lock
								req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
								Me.Populate_REQDetail(obligRow, FundingLineCell, req_id_val, "Obligations", insertCivPay, False)
								Me.Populate_REQDetail(comtRow, FundingLineCell, req_id_val, "Commitments", insertCivPay, False)
								'do carry over accounts
								Me.Populate_REQDetail(carryOverObligRow, FundingLineCell, req_id_val, "Obligations", insertCarryover, True)
								Me.Populate_REQDetail(carryOverComtRow, FundingLineCell, req_id_val, "Commitments", insertCarryover, True)
								
								If insertCivPay Then
									
									REQDT.Rows.Add(parentRow)
		
									Dim newObligRow As Object() = obligRow.ItemArray
									REQDetailDT.Rows.Add(newObligRow)
		
									Dim newCmtRow As Object() = comtRow.ItemArray
									REQDetailDT.Rows.Add(newCmtRow)
								End If 
								
								If insertCarryover Then
									Dim newcarryOverObligRow As Object() = carryOverObligRow.ItemArray
									REQDetailDT.Rows.Add(newcarryOverObligRow)
		
									Dim newcarryOverCmtRow As Object() = carryOverComtRow.ItemArray
									REQDetailDT.Rows.Add(newcarryOverCmtRow)
								End If
							End If
#End Region			

#Region "withhold"	
							'Handle withhold
							If req_id_Type.XFEqualsIgnoreCase("withhold")
								dim insertshell as Boolean
								'Get data from db for each funding line in the data buffer
								If Not REQDetailDT Is Nothing And REQDetailDT.Rows.Count > 0 Then
									If req_id_Type.XFEqualsIgnoreCase("withhold")
										obligRow = GetDetailRow(REQDetailDT, tm, "Obligations", FundingLineCell, "withhold")
										comtRow = GetDetailRow(REQDetailDT, tm, "Commitments", FundingLineCell, "withhold")
										
									End If
	
								End If 
								'there should only be one row per APPN and Entity
								If Not REQDT Is Nothing And REQDT.Rows.Count > 0 Then
									parentRow = REQDT.Select($"APPN = '{APPN}'").FirstOrDefault()
									If parentRow Is Nothing Then
										insertShell = True
										parentRow = REQDT.NewRow()
										Me.Populate_REQParent_WH(parentRow, FundingLineCell, insertShell)
										REQDT.Rows.Add(parentRow)
									Else
									End If
								Else
									insertShell = True
									parentRow = REQDT.NewRow()
									Me.Populate_REQParent_WH(parentRow, FundingLineCell, insertShell)
									REQDT.Rows.Add(parentRow)
									'req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
									'whereClause = $"CMD_SPLN_REQ_ID = '{req_id_val}'"
								End If
								
								req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
								
						
							    'Details
								
								
								REQDetailDT.PrimaryKey = New DataColumn() {
													 REQDetailDT.Columns("Entity"),
													 REQDetailDT.Columns("Fiscal_Year"),
											         REQDetailDT.Columns("Unit_of_Measure"),
													 REQDetailDT.Columns("UD1"),
													 REQDetailDT.Columns("UD2"),
													 REQDetailDT.Columns("UD3"),
													 REQDetailDT.Columns("UD4"),
													 REQDetailDT.Columns("UD6"),
													 REQDetailDT.Columns("Account"),
													 REQDetailDT.Columns("CMD_SPLN_REQ_ID")
													 }
										 
			 					
								Dim isDetailinsert As Boolean = False
								Dim detailObligRow As DataRow = Nothing 'REQDetailDT.NewRow()
								Dim detailCommitRow As DataRow = Nothing
								If Not REQDetailDT Is Nothing And REQDetailDT.Rows.Count > 0 Then
	
	'BRApi.ErrorLog.LogMessage(si,"Looking for Detail : " & api.Pov.Entity.Name  & " Acct = WH_Obligations  UD1 = " & UD1 & " UD2 = " & UD2 & " UD3 = " & UD3 & " UD4 = " & UD4 & " UD5 = " & UD5 &  " UD6 = " & UD6)	
	
									detailObligRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																			AND Account = 'WH_Obligations'
																			AND WFCMD_Name = '{api.pov.cube.name}' 																	
																			AND Entity = '{api.pov.entity.name}'
																			AND UD1 = '{UD1}'
																			AND UD2 = '{UD2}'
																			AND UD3 = '{UD3}'
																			AND UD4 = '{UD4}'
																			AND UD6 = '{UD6}'").FirstOrDefault()
									
									detailCommitRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																			AND Account = 'WH_Commitments'
																			AND WFCMD_Name = '{api.pov.cube.name}' 																	
																			AND Entity = '{api.pov.entity.name}'
																			AND UD1 = '{UD1}'
																			AND UD2 = '{UD2}'
																			AND UD3 = '{UD3}'
																			AND UD4 = '{UD4}'
																			AND UD6 = '{UD6}'").FirstOrDefault()
		
								End If
	
								If detailObligRow Is Nothing Then
									isDetailinsert = True
									detailObligRow = REQDetailDT.NewRow()
									'REQDetailDT.Rows.Add(detailObligRow)
								Else
								End If 
								If detailCommitRow Is Nothing Then
									isDetailinsert = True
									detailCommitRow = REQDetailDT.NewRow()
									'REQDetailDT.Rows.Add(detailCommitRow)
								End If 
								'Me.Populat_REQDetail(obligRow, FundingLineCell, req_id_val, "Commitments", isinsert)
								Me.Populate_REQDetail_WH(detailObligRow, FundingLineCell, req_id_val, "WH_Obligations", isDetailinsert)
								Me.Populate_REQDetail_WH(detailCommitRow, FundingLineCell, req_id_val, "WH_Commitments", isDetailinsert)
								If isDetailinsert Then
									REQDetailDT.Rows.Add(detailObligRow)
									REQDetailDT.Rows.Add(detailCommitRow)
								End If
								
							End If
#End Region								
						Next
						
				 	End Using
				 End Using				

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub
					
#End Region
		Public Function GetParentRow(ByVal REQDT As DataTable, ByRef year As String, ByRef req_id_type As String, ByRef FundingLineCell As DataBufferCell ) As DataRow
			Try	
				Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
				Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
				Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
				Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
				Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
				Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
				Dim UD7 As String = FundingLineCell.DataBufferCellPk.GetUD7Name(api)
				
				'Dim APPN As String = ""
				If req_id_type.XFEqualsIgnoreCase("civPay_Copied") Then
					UD6 = "Pay_General"
				End If
				
				If req_id_type.XFEqualsIgnoreCase("withhold") Then
					Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
					Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
					UD1 = lsAncestorListU1(0).Member.Name
				End If
				
				Dim detailRow As DataRow = Nothing
				detailRow = REQDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
											AND req_id_type = 'req_id_type'
											AND WFCMD_Name = '{api.pov.cube.name}' 																	
											AND Entity = '{api.pov.entity.name}'
											AND Fiscal_Year = '{year}'
											AND APPN = '{UD1}'
											AND MDEP = '{UD2}'
											AND APE9 = '{UD3}'
											AND Dollar_Type = '{UD4}'
											AND CType = '{UD5}'
											AND Obj_Class = '{UD6}'").FirstOrDefault()	
				Return detailRow
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Function

		Public Function GetDetailRow(ByVal REQDetailDT As DataTable, ByRef year As String, ByRef account As String, ByRef FundingLineCell As DataBufferCell,ByRef req_id_type As String ) As DataRow
			Try	
				Dim UD6 As String = ""
				If req_id_type.XFEqualsIgnoreCase("civPay_Copied") Then
					UD6 = "Pay_General"
				End If
				
				If req_id_type.XFEqualsIgnoreCase("withhold") Then
					If  (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General"))  Then
						UD6= "Pay_General"
					Else
						UD6 = "NonPay_General"
					End If
				End If
				
				Dim detailRow As DataRow = Nothing
				detailRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
											AND Account = 'account'
											AND WFCMD_Name = '{api.pov.cube.name}' 																	
											AND Entity = '{api.pov.entity.name}'
											AND Fiscal_Year = '{year}'
											AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
											AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
											AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
											AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
											AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()	
				Return detailRow
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Function
		
				
#Region "Copy_CivPay"
		Public Sub Copy_CivPay(ByVal globals As BRGlobals)
			Try	
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd As String = wfInfoDetails("CMDName")	
				Dim sSourcePosition As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As String = ""' args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				Dim sTargetScenario As String = "CMD_SPLN_C" & tm
				Dim currentYear As Integer = Convert.ToInt32(tm)
				Dim nextyear As Integer = currentYear  + 1
				'Dim sTargetFirstYear As String = api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim sTargetYear As Integer =tm.XFConvertToInt' api.Pov.Time.Name.XFConvertToInt
				Dim sEntity As String = api.Pov.Entity.Name
				Dim levl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim statuslvl As String = levl & "_Formulate_SPLN"

				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])"
				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
	'BRApi.ErrorLog.LogMessage(si, "dbFundingLines: " & sSrcMbrScript & "dbFundingLines count: " & dbFundingLines.DataBufferCells.Count)	
	'dbFundingLines.LogDataBuffer(api,"Buffer log " & api.Pov.Entity.Name, 1000)
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)
				
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				Dim REQDT As DataTable = New DataTable("REQDT")
				 
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
						'Get header
						Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					    Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
						
						Dim SqlREQ As String = $"SELECT * 
								     				   FROM XFC_CMD_SPLN_REQ WITH (NOLOCK)
											           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
											           And WFTime_Name = '{tm}'
											           AND WFScenario_Name = '{sTargetScenario}'
											           AND ENTITY = '{api.pov.Entity.Name}'
													   and REQ_ID_Type = 'CivPay_Copied' "
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & 	SqlREQ)					
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
						sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
						
						Dim cmd_req_ids As New List(Of String) 
						Dim whereClause As String = " 1 = 2"
						If Not REQDT Is Nothing And REQDT.Rows.Count > 0 Then
							For Each r As DataRow In REQDT.Rows
								cmd_req_ids.Add($"'{r("CMD_SPLN_REQ_ID").ToString}'")
							Next
							 whereClause = $"CMD_SPLN_REQ_ID In ({String.Join(",", cmd_req_ids)})"
						End If
					
					    'Details
						Dim SqlREQDetail As String = $"SELECT *  
								     				   FROM XFC_CMD_SPLN_REQ_Details WITH (NOLOCK)
											           WHERE {whereClause}"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & 	SqlREQDetail)	

						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						
						REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("Entity"),
												 REQDetailDT.Columns("Fiscal_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD5"),
												 REQDetailDT.Columns("Account"),
												 REQDetailDT.Columns("CMD_SPLN_REQ_ID")
												 }
									 
'BRApi.ErrorLog.LogMessage(si,"civpay dbFundingLines.DataBufferCells.values: " & dbFundingLines.DataBufferCells.values.Count)

					'Dim REQDetailDTToADD As DataTable = New DataTable("REQDetailDTToADD")
					For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
						'Dim Acct = "Obligations"  'Target account?
	 					Dim UoM = "Funding"
					
						Dim isinsert As Boolean = False
						Dim parentRow As DataRow = REQDT.NewRow()
						Dim obligRow As DataRow = Nothing 'REQDetailDT.NewRow()
						Dim comtRow As DataRow = Nothing
						Dim carryOverObligRow As DataRow = Nothing 
						Dim carryOverComtRow As DataRow = Nothing
						If Not REQDetailDT Is Nothing And REQDetailDT.Rows.Count > 0 Then
							obligRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = 'Obligations'
																	AND WFCMD_Name = '{api.pov.cube.name}' 																	
																	AND Entity = '{api.pov.entity.name}'
																	AND Fiscal_Year = '{currentYear}' 
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
							
							comtRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = 'Commitments'
																	AND WFCMD_Name = '{api.pov.cube.name}' 																	
																	AND Entity = '{api.pov.entity.name}'
																	AND Fiscal_Year = '{currentYear}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
							
							carryOverObligRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = 'Obligations'
																	AND WFCMD_Name = '{api.pov.cube.name}' 																	
																	AND Entity = '{api.pov.entity.name}'
																	AND Fiscal_Year = '{nextYear}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
							
							carryOverComtRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = 'Commitments'
																	AND WFCMD_Name = '{api.pov.cube.name}' 																	
																	AND Entity = '{api.pov.entity.name}'
																	AND Fiscal_Year = '{nextYear}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
						
						End If
						If obligRow Is Nothing Then
							isinsert = True
							
							parentRow = REQDT.NewRow()
							Me.Populate_REQ(parentRow, FundingLineCell, isinsert)
							'REQDT.Rows.Add(parentRow)
							obligRow = REQDetailDT.NewRow()
							comtRow = REQDetailDT.NewRow()
							
						Else
							parentRow = REQDT.Select("CMD_SPLN_REQ_ID = " & "'" & obligRow("CMD_SPLN_REQ_ID").ToString & "'").FirstOrDefault()
						End If 
						
						Dim isinsertCarryOver = False
						If carryOverObligRow Is Nothing Then
							isinsertCarryOver = True
							carryOverObligRow = REQDetailDT.NewRow()
							carryOverComtRow = REQDetailDT.NewRow()
						End If
						'Me.Populate_REQ(parentRow, FundingLineCell, isinsert)'*Dead Lock
						req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
						Me.Populate_REQDetail(obligRow, FundingLineCell, req_id_val, "Obligations", isinsert, False)
						Me.Populate_REQDetail(comtRow, FundingLineCell, req_id_val, "Commitments", isinsert, False)
						'do carry over accounts
						Me.Populate_REQDetail(carryOverObligRow, FundingLineCell, req_id_val, "Obligations", isinsertCarryOver, True)
						Me.Populate_REQDetail(carryOverComtRow, FundingLineCell, req_id_val, "Commitments", isinsertCarryOver, True)
						
						If isinsert Then
							
							REQDT.Rows.Add(parentRow)

							Dim newObligRow As Object() = obligRow.ItemArray
							REQDetailDT.Rows.Add(newObligRow)

							Dim newCmtRow As Object() = comtRow.ItemArray
							REQDetailDT.Rows.Add(newCmtRow)
						End If 
						
						If isinsertCarryOver Then
							Dim newcarryOverObligRow As Object() = carryOverObligRow.ItemArray
							REQDetailDT.Rows.Add(newcarryOverObligRow)

							Dim newcarryOverCmtRow As Object() = carryOverComtRow.ItemArray
							REQDetailDT.Rows.Add(newcarryOverCmtRow)
						End If
						
					Next
					'create a new Data row for req and detail
					sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)	
					sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)	
				 	End Using
				 End Using
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub
					
#End Region

#Region "civ pay helpers"
		Public Sub Populate_REQ(ByVal row As DataRow, ByVal FundingLineCell As DataBufferCell, ByRef isNew As Boolean )
			Try	
				Dim sTargetScenario As String = "CMD_SPLN_C" + api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim statuslvl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, api.Pov.Entity.Name) & "_Formulate_SPLN"
 				Dim UoM = "Funding"
				Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
				Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
				Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
				Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
				Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
				Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
				Dim UD7 As String = FundingLineCell.DataBufferCellPk.GetUD7Name(api)
				Dim entity As String = api.Pov.Entity.Name

				row("WFScenario_Name") = sTargetScenario
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				row("WFTime_Name") = tm
				
				row("WFCMD_Name") = api.Pov.Cube.Name
				Row("Entity") = entity
				Row("Status") = statuslvl
				
				
				Row("Title") = $"CivPay - {entity} - {UD1}-{UD2}-{UD3}-{UD4}-{UD5}"
				
				Row("APPN") = UD1
				Row("MDEP") = UD2
				Row("APE9") = UD3
				Row("Dollar_Type") = UD4
				Row("Ctype") = UD5
				Row("Obj_Class") = "Pay_General"
				Row("UIC") = "None"
				Row("REQ_ID_Type") = "CivPay_Copied"

				If isNew  Then
					row("CMD_SPLN_REQ_ID") = Guid.NewGuid()
					'row("REQ_ID") = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,api.Pov.Entity.Name)
					row("REQ_ID") = GetNextREQID(entity)
					row("Create_Date") = DateTime.Now
					row("Create_User") = si.UserName
				End If
				row("Update_Date") = DateTime.Now
				row("Update_User") = si.UserName
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub	
		
		Public Sub Populate_REQDetail(ByVal row As DataRow, ByVal FundingLineCell As DataBufferCell, ByRef req_id_val As String,ByRef act As String, ByRef isNew As Boolean, ByRef isCarryOver As Boolean)
			Try	
				Dim sTargetScenario As String = "CMD_SPLN_C" + api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim statuslvl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, api.Pov.Entity.Name) & "_Formulate_SPLN"
				Dim Acct = act 'Target account?
 				Dim UoM = "Funding"
				Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
				Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
				Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
				Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
				Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
				Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
				Dim Amt As Decimal = FundingLineCell.CellAmount
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				Dim currentYear As Integer = Convert.ToInt32(tm)
				Dim nextyear As Integer = currentYear  + 1

				Dim spread As Dictionary(Of Integer, Decimal)
				
				If isCarryOver Then
					Row("Fiscal_Year") = nextyear
				Else
					Row("Fiscal_Year") = tm
				End If

				row("WFScenario_Name") = api.pov.Scenario.Name
				
				row("WFTime_Name") = tm
				row("WFCMD_Name") = api.Pov.Cube.Name
				row("Unit_of_Measure") = UoM 
				Row("Entity") = api.Pov.Entity.Name
				Row("IC") = "None"
				Row("Account") = Acct
				Row("Flow") = statuslvl
				
				Dim APPN As String  = ""
				Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
				Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
				APPN = lsAncestorListU1(0).Member.Name
'Brapi.ErrorLog.LogMessage(si, "writing detail for: New rec= " & isNew & ", iscarryOver= " & isCarryOver & ", GUID= " & req_id_val & ", entity = " & api.Pov.Entity.Name  & ", scenario = " & api.pov.Scenario.Name & ", APPN: " & APPN & ", UD1: " & UD1 & ", UD3: " & UD3)								
				Dim spreadTitle As String = $"Monthly Spread - {APPN} - {acct}"
				spread = globals.GetObject(spreadTitle)
				If spread Is Nothing Then
					spread = New Dictionary(Of Integer, Decimal)
					If acct.XFEqualsIgnoreCase("Commitments") Then
							GetMonthlySpread(APPN, "Commit_Spread_Pct", spreadTitle, spread)
					Else 
						If acct.XFEqualsIgnoreCase("Obligations") Then
								GetMonthlySpread(APPN, "Obligation_Spread_Pct",spreadTitle, spread)
						Else
							Throw New Exception("Invalid Spend Plan Account: " & acct)
						End If
					End If
				End If
				
				If isCarryOver Then
					Row("Yearly") = Amt * spread(13)
				Else
					Row("Month1") = Amt * spread(1)
					Row("Month2") = Amt * spread(2)
					Row("Month3") = Amt * spread(3)
					Row("Month4") = Amt * spread(4)
					Row("Month5") = Amt * spread(5)
					Row("Month6") = Amt * spread(6)
					Row("Month7") = Amt * spread(7)
					Row("Month8") = Amt * spread(8)
					Row("Month9") = Amt * spread(9)
					Row("Month10") = Amt * spread(10)
					Row("Month11") = Amt * spread(11)
					Row("Month12") = Amt * spread(12)
				End If

				
				row("UD1") = UD1
				row("UD2") = UD2
				row("UD3") = UD3
				row("UD4") = UD4
				row("UD5") = UD5
				row("UD6") = "Pay_General"
				row("UD7") = "None"
				row("UD8") = "None"
				If isNew  Then
					row("CMD_SPLN_REQ_ID") = req_id_val
					row("Create_Date") = DateTime.Now
					row("Create_User") = si.UserName
				End If
				row("Update_Date") = DateTime.Now
				row("Update_User") = si.UserName
					
				globals.SetStringValue($"FundsCenterStatusUpdates - {api.Pov.Entity.Name}", $"{statuslvl}")
				
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub	
		

#End Region

#Region "Copy_Withhold"

		Public Sub Copy_Withhold(ByVal globals As BRGlobals)
			Try		
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim sSourcePosition As String = wfInfoDetails("ScenarioName")'ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As String = ""
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				Dim sTargetScenario As String = "CMD_SPLN_C" & tm
				Dim sTargetYear As Integer =tm.XFConvertToInt
				Dim sEntity As String = api.Pov.Entity.Name
				Dim levl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim statuslvl As String = levl & "_Formulate_SPLN"
				Dim UoM = "Funding"
				
				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(Cb#{cmd}:E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#USD:V#Periodic:F#Tot_Dist_Final:O#AdjInput:I#None:U7#Top:U8#None),[A#TGT_WH])"
				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
	'dbFundingLines.LogDataBuffer(api, "DataBuffer: " & api.pov.Entity.name & api.Pov.Scenario.Name & api.Pov.Time.Name & api.Pov.Cons.name	,1000)
			
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)
						
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				Dim REQDT As DataTable = New DataTable("REQDT")
				 
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
						'Get header
						Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					    Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
						Dim SqlREQ As String = $"SELECT * 
									     				   FROM XFC_CMD_SPLN_REQ
												           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
												           And WFTime_Name = '{tm}'
												           AND WFScenario_Name = '{sTargetScenario}'
												           AND ENTITY = '{api.pov.Entity.Name}'
														   and REQ_ID_Type = 'Withhold'"
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & 	SqlREQ)					
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
						sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
						
						Dim whereClause As String = " 1 = 22 "
						If Not REQDT Is Nothing And REQDT.Rows.Count > 0 Then
							whereClause = "CMD_SPLN_REQ_ID in ("
							For Each r In REQDT.Rows
								whereClause = whereClause & "'" & r("CMD_SPLN_REQ_ID").toString &"',"
							Next
							whereClause = whereClause.Substring(0, whereClause.Length -1)
							whereClause = whereClause & ")"
						End If
						Dim SqlREQDetail As String = $"SELECT * 
							     				   FROM XFC_CMD_SPLN_REQ_Details
										           WHERE {whereClause}"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & 	SqlREQDetail)	

						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
							
							
						For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
							
							Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
							Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
							Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
							Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
							Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
							Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
							Dim APPN As String = UD3.Split("_")(0)
							If  (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General"))  Then
								UD6= "Pay_General"
							Else
								UD6 = "NonPay_General"
							End If
							
							Dim Amt As Decimal = FundingLineCell.CellAmount
							
							Dim cmd_req_ids As New List(Of String) 
							
							Dim insertShell As Boolean = False
							Dim parentRow As DataRow = REQDT.NewRow()
							
							'there should only be one row per APPN and Entity
							If Not REQDT Is Nothing And REQDT.Rows.Count > 0 Then
								parentRow = REQDT.Select($"APPN = '{APPN}'").FirstOrDefault()
								If parentRow Is Nothing Then
									insertShell = True
									parentRow = REQDT.NewRow()
									Me.Populate_REQParent_WH(parentRow, FundingLineCell, insertShell)
									REQDT.Rows.Add(parentRow)
								Else
								End If
							Else
								insertShell = True
								parentRow = REQDT.NewRow()
								Me.Populate_REQParent_WH(parentRow, FundingLineCell, insertShell)
								REQDT.Rows.Add(parentRow)
								'req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
								'whereClause = $"CMD_SPLN_REQ_ID = '{req_id_val}'"
							End If
							
							req_id_val = parentRow("CMD_SPLN_REQ_ID").ToString
							
					
						    'Details
							
							
							REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("Entity"),
												 REQDetailDT.Columns("Fiscal_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD6"),
												 REQDetailDT.Columns("Account"),
												 REQDetailDT.Columns("CMD_SPLN_REQ_ID")
												 }
									 
		 					
							Dim isDetailinsert As Boolean = False
							Dim detailObligRow As DataRow = Nothing 'REQDetailDT.NewRow()
							Dim detailCommitRow As DataRow = Nothing
							If Not REQDetailDT Is Nothing And REQDetailDT.Rows.Count > 0 Then

'BRApi.ErrorLog.LogMessage(si,"Looking for Detail : " & api.Pov.Entity.Name  & " Acct = WH_Obligations  UD1 = " & UD1 & " UD2 = " & UD2 & " UD3 = " & UD3 & " UD4 = " & UD4 & " UD5 = " & UD5 &  " UD6 = " & UD6)	

								detailObligRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																		AND Account = 'WH_Obligations'
																		AND WFCMD_Name = '{api.pov.cube.name}' 																	
																		AND Entity = '{api.pov.entity.name}'
																		AND UD1 = '{UD1}'
																		AND UD2 = '{UD2}'
																		AND UD3 = '{UD3}'
																		AND UD4 = '{UD4}'
																		AND UD6 = '{UD6}'").FirstOrDefault()
								
								detailCommitRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																		AND Account = 'WH_Commitments'
																		AND WFCMD_Name = '{api.pov.cube.name}' 																	
																		AND Entity = '{api.pov.entity.name}'
																		AND UD1 = '{UD1}'
																		AND UD2 = '{UD2}'
																		AND UD3 = '{UD3}'
																		AND UD4 = '{UD4}'
																		AND UD6 = '{UD6}'").FirstOrDefault()
	
							End If

							If detailObligRow Is Nothing Then
								isDetailinsert = True
								detailObligRow = REQDetailDT.NewRow()
								'REQDetailDT.Rows.Add(detailObligRow)
							Else
							End If 
							If detailCommitRow Is Nothing Then
								isDetailinsert = True
								detailCommitRow = REQDetailDT.NewRow()
								'REQDetailDT.Rows.Add(detailCommitRow)
							End If 
							'Me.Populat_REQDetail(obligRow, FundingLineCell, req_id_val, "Commitments", isinsert)
							Me.Populate_REQDetail_WH(detailObligRow, FundingLineCell, req_id_val, "WH_Obligations", isDetailinsert)
							Me.Populate_REQDetail_WH(detailCommitRow, FundingLineCell, req_id_val, "WH_Commitments", isDetailinsert)
							If isDetailinsert Then
								REQDetailDT.Rows.Add(detailObligRow)
								REQDetailDT.Rows.Add(detailCommitRow)
							End If

						Next

						'create a new Data row for req and detail
						sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
						sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)	
				 	End Using
				 End Using
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				 
		End Sub
					
#End Region

#Region "Withhold helpers"
		Public Sub Populate_REQParent_WH(ByVal row As DataRow, ByVal FundingLineCell As DataBufferCell, ByRef isNew As Boolean )
			Try	
				Dim sTargetScenario As String = "CMD_SPLN_C" + api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim statuslvl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, api.Pov.Entity.Name) & "_Formulate_SPLN"
 				Dim UoM = "Funding"
				Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
				Dim APPN As String = UD3.Split("_")(0)
				Dim entity As String = api.Pov.Entity.Name

				row("WFScenario_Name") = sTargetScenario
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				row("WFTime_Name") = tm
				
				row("WFCMD_Name") = api.Pov.Cube.Name
				Row("Entity") = entity
				Row("Status") = statuslvl
				
				
				Row("Title") = $"Withhold - {entity} - {APPN}"
				
				Row("APPN") = APPN
				Row("MDEP") = "None"
				Row("APE9") = "None"
				Row("Dollar_Type") = "None"
				Row("Ctype") =  "None"
				Row("Obj_Class") =  "None"
				Row("UIC") = "None"
				Row("REQ_ID_Type") = "Withhold"
				If isNew  Then
					row("CMD_SPLN_REQ_ID") = Guid.NewGuid()
					'row("REQ_ID") = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,api.Pov.Entity.Name)
					row("REQ_ID") = GetNextREQID(api.Pov.Entity.Name)
					row("Create_Date") = DateTime.Now
					row("Create_User") = si.UserName
				End If
				row("Update_Date") = DateTime.Now
				row("Update_User") = si.UserName
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub	
		
		Public Sub Populate_REQDetail_WH(ByVal row As DataRow, ByVal FundingLineCell As DataBufferCell, ByRef req_id_val As String,ByRef act As String, ByRef isNew As Boolean)
			Try	
				Dim sTargetScenario As String = "CMD_SPLN_C" + api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim statuslvl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, api.Pov.Entity.Name) & "_Formulate_SPLN"
				Dim Acct = act
 				Dim UoM = "Funding"
				Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
				Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
				Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
				Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
				Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
				Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
				If (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General")) Then
					UD6 = "Pay_General"
				Else
					UD6 = "NonPay_General"
				End If
				Dim Amt As Decimal = FundingLineCell.CellAmount
				Dim tm As String = api.Pov.Time.Name
				tm = tm.Substring(0, tm.Length - 3)
				Dim spread As Dictionary(Of Integer, Decimal)
				
'BRApi.ErrorLog.LogMessage(si,"Detail is new: " & isNew & " " & req_id_val & " " & api.Pov.Entity.Name & " UD1 = " & UD1 & " UD2 = " & UD2 & " UD3 = " & UD3 & " UD4 = " & UD4 & " UD5 = " & UD5 &  " UD6 = " & UD6 & " Acct = " & Acct & ", Amt = " & Amt)	
				row("WFScenario_Name") = api.pov.Scenario.Name
				row("WFTime_Name") = tm
				
				row("WFCMD_Name") = api.Pov.Cube.Name
				row("Unit_of_Measure") = UoM 
				Row("Entity") = api.Pov.Entity.Name
				Row("IC") = "None"
				Row("Account") = Acct
				Row("Flow") = statuslvl
				Row("Fiscal_Year") = tm
				'Dim APPN As String = UD3.Split("_")(0)
				
				Dim APPN As String  = ""
				Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
				Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
				APPN = lsAncestorListU1(0).Member.Name

				Dim spreadTitle As String = "Monthly Withhold Spread"
				spread = globals.GetObject(spreadTitle)
				If spread Is Nothing Then
					spread = New Dictionary(Of Integer, Decimal)
					If acct.XFEqualsIgnoreCase("WH_Commitments") Then
							GetMonthlySpread("None", "Commit_WH_Spread_Pct", spreadTitle, spread)
					Else 
						If acct.XFEqualsIgnoreCase("WH_Obligations") Then
								GetMonthlySpread("None", "Commit_WH_Spread_Pct", spreadTitle, spread)
						Else
							Throw New Exception("Invalid Spend Plan Account: " & acct)
						End If
					End If
				End If
				Row("Month1") = Amt * spread(1)
				Row("Month2") = Amt * spread(2)
				Row("Month3") = Amt * spread(3)
				Row("Month4") = Amt * spread(4)
				Row("Month5") = Amt * spread(5)
				Row("Month6") = Amt * spread(6)
				Row("Month7") = Amt * spread(7)
				Row("Month8") = Amt * spread(8)
				Row("Month9") = Amt * spread(9)
				Row("Month10") = Amt * spread(10)
				Row("Month11") = Amt * spread(11)
				Row("Month12") = Amt * spread(12)
				row("UD1") = UD1
				row("UD2") = UD2
				row("UD3") = UD3
				row("UD4") = UD4
				row("UD5") = UD5
				row("UD6") = UD6
				
				row("UD7") = "None"
				row("UD8") = "None"
				
				If isNew  Then
					row("CMD_SPLN_REQ_ID") = req_id_val
					row("Create_Date") = DateTime.Now
					row("Create_User") = si.UserName
				End If
				row("Update_Date") = DateTime.Now
				row("Update_User") = si.UserName
				globals.SetStringValue($"FundsCenterStatusUpdates - {api.Pov.Entity.Name}", $"{statuslvl}")
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub	
#End Region

#Region "Get Cmd FundCenter From Cube"		
		
		Public Function GetCmdFundCenterFromCube() As String
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim profileName = wfInfoDetails("ScenarioName")
			Dim cubeName As String = wfInfoDetails("CMDName")
			Dim entityMem As Member =  BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, cubeName).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

			Dim fundCenter As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 1, wfScenarioTypeID, wfTimeId)
			
			Return fundCenter			
		End Function

#End Region

#Region "Get Monthly Spread"		

		Public Function GetMonthlySpread(ByRef APPN As String, ByRef acct As String ,ByRef title As String, ByRef spread As Dictionary(Of Integer, Decimal)) As Decimal
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim cubeName As String = wfInfoDetails("CMDName")
			Dim entity As String = GetCmdFundCenterFromCube()
			Dim scenario As String = "RMW_Cycle_Config_Monthly"
			Dim tm As String = wfInfoDetails("TimeName")
			
			Dim spreadPct As Decimal = 0
			For i As Integer = 1 To 12
				Dim spreadmbrScript = $"Cb#{cubeName}:E#{entity}:C#Local:S#{scenario}:T#{tm}M{i}:V#Periodic:A#{acct}:F#None:O#AdjInput:I#None:U1#{APPN}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				spreadPct = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, spreadmbrScript).DataCellEx.DataCell.CellAmount
				spreadPct = spreadPct/100
				spread.Add(i,spreadPct)
			Next 
			
			'Add carry over
			Dim tmInt As Integer = Convert.ToInt32(tm)
			Dim nextyear As Integer = tmInt  + 1
			Dim carryOverSpreadmbrScript = $"Cb#{cubeName}:E#{entity}:C#Local:S#{scenario}:T#{nextyear}M12:V#Periodic:A#{acct}:F#None:O#AdjInput:I#None:U1#{APPN}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			spreadPct = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, carryOverSpreadmbrScript).DataCellEx.DataCell.CellAmount
'BRApi.ErrorLog.LogMessage(si,"carryOverSpreadmbrScript: " & carryOverSpreadmbrScript.ToString & ", Amt: " & spreadPct)		
			spreadPct = spreadPct/100
			spread.Add(13,spreadPct)
				
			globals.SetObject($"{title}", spread)
			
			Return Nothing
			
		End Function
		

#End Region


#Region "GetNextREQID"
	Dim startingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
	Function GetNextREQID (fundCenter As String) As String
		Dim currentREQID As Integer
		Dim newREQ_ID As String
		If startingREQ_IDByFC.TryGetValue(fundCenter, currentREQID) Then
			currentREQID = currentREQID + 1
			Dim modifiedFC As String = fundCenter
			modifiedFC = modifiedFC.Replace("_General", "")
			If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
			newREQ_ID =  modifiedFC &"_" & currentREQID.ToString("D5")
			startingREQ_IDByFC(fundCenter) = currentREQID
		Else	
			newREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,fundCenter)
			startingREQ_IDByFC.Add(fundCenter.Trim, newREQ_ID.Split("_")(1))
		End If 
		Return newREQ_ID
	End Function
#End Region	

#Region "Consol Aggregated"
Private Sub Consol_Aggregated()
		If api.Pov.Entity.Name.XFContainsIgnoreCase("E#None") Then Return
		Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
		Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,api.Pov.Entity.Name)
		Dim entityID As Integer = api.Members.GetMemberId(dimType.Entity.Id,api.Pov.Entity.Name)
		Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)
'brapi.ErrorLog.LogMessage(si,"entity = " & api.Pov.Entity.Name)
		If Not Ent_Base
			Dim Src_Data = String.Empty
			Dim acctFilter = "A#Commitments,A#Obligations,A#WH_Obligations,A#WH_Commitments"
			Dim originFilter = "O#AdjConsolidated,O#Forms,O#Import"
			Dim flowFilter = "F#L2_Formulate,F#L3_Formulate_SPLN,F#L4_Formulate_SPLN,F#L5_Formulate_SPLN,F#L3_Validate_SPLN,F#L4_Validate_SPLN,F#L5_Validate_SPLN,F#L3_Approve_SPLN,F#L2_Validate_SPLN,F#L2_Approve_SPLN,F#L2_Final_SPLN"
			Select Case entityLevel
				Case "L3"
					flowFilter =  "F#L3_Formulate_SPLN,F#L4_Formulate_SPLN,F#L5_Formulate_SPLN,F#L3_Validate_SPLN,F#L4_Validate_SPLN,F#L5_Validate_SPLN,F#L3_Approve_SPLN,F#L2_Validate_SPLN,F#L2_Approve_SPLN,F#L2_Final_SPLN"
				Case "L4"
					flowFilter =  "F#L4_Formulate_SPLN,F#L5_Formulate_SPLN,F#L3_Validate_SPLN,F#L4_Validate_SPLN,F#L5_Validate_SPLN,F#L3_Approve_SPLN,F#L2_Validate_SPLN,F#L2_Approve_SPLN,F#L2_Final_SPLN"
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
			
			src_DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(RemoveNoData({Src_Data}),[O#AdjConsolidated,O#AdjInput,O#Forms,O#Import],[{flowFilter}],[{acctFilter}])")
'CurrCubeBuffer.LogDataBuffer(api,$"{api.pov.entity.name}_CurrBufferConsol",500)
'Src_DataBuffer.LogDataBuffer(api,$"{api.pov.entity.name}_SrcBufferConsol - C#{api.Pov.Cons.Name} - T#{api.Pov.Time.name}",500)  

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
#End Region
		Public Sub Copy_HQ_SPLN()
			
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