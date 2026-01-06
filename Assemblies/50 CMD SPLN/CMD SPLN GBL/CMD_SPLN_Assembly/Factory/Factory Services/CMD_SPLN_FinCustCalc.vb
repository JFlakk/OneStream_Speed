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
'destBuffer.LogDataBuffer(api,"destBuffer = " & api.Pov.Entity.name,500)
			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			If destBuffer.DataBufferCells.Count > 0 Then
				api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, True)
				destBuffer.DataBufferCells.Clear()
			End If
			
'currcubebuffer.LogDataBuffer(api,"currcube = " & api.Pov.Entity.name,500)
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
				
#Region "Copy_CivPay"
		Public Sub Copy_CivPay(ByVal globals As BRGlobals)
			Try	
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd As String = wfInfoDetails("CMDName")	
				Dim sSourcePosition As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As String = ""' args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim sTargetScenario As String = "CMD_SPLN_C" & wfInfoDetails("TimeName")
				Dim currentYear As Integer = Convert.ToInt32(wfInfoDetails("TimeName"))
				Dim nextyear As Integer = currentYear  + 1
				Dim sTargetYear As Integer = wfInfoDetails("TimeName").XFConvertToInt
				Dim sEntity As String = api.Pov.Entity.Name
				Dim levl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim statuslvl As String = levl & "_Formulate_SPLN"
				
				Dim cmd_Appn_Rates As New DataTable()
				If globals.GetObject($"CMD APPN Rates - {wfInfoDetails("CMDName")}") Is Nothing
					cmd_Appn_Rates = GetFDXCMDSPLNData("CMD_SPLN_APPN_SpreadPct_FDX_CV",wfInfoDetails("CMDName"))
					globals.SetObject($"CMD APPN Rates - {wfInfoDetails("CMDName")}",cmd_Appn_Rates)
				Else
					cmd_Appn_Rates = globals.GetObject($"CMD APPN Rates - {wfInfoDetails("CMDName")}")
				End If
				Dim APPN As String  = String.Empty
				Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#Aggregated:V#Periodic:F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])"
				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
				Dim accounts As New List(Of String) From {"Commit", "Obligation"}
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

				Dim currREQDetailDT As DataTable = New DataTable("REQDetailDT")
				Dim currREQDT As DataTable = New DataTable("REQDT")
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
						'Get header
						Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					    Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
						
						Dim SqlREQ As String = $"SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, Review_Entity, REQ_ID_Type, Demotion_Comment,
														    Related_REQs
								     				   FROM XFC_CMD_SPLN_REQ WITH (NOLOCK)
											           WHERE WFCMD_Name = '{wfInfoDetails("CMDName")}'
											           And WFTime_Name = '{wfInfoDetails("TimeName")}'
											           AND WFScenario_Name = '{sTargetScenario}'
											           AND ENTITY = '{api.pov.Entity.Name}'
													   and REQ_ID_Type = 'CivPay_Copied'"
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & 	SqlREQ)					
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
						sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, currREQDT, SqlREQ, sqlparamsREQ)
					
					    'Details
						Dim SqlREQDetail As String = $"SELECT d.CMD_SPLN_REQ_ID,d.WFScenario_Name,d.WFCMD_Name,d.WFTime_Name,d.Unit_of_Measure,
															d.Entity,d.IC,d.Account,d.Flow,d.UD1,d.UD2,d.UD3,d.UD4,d.UD5,d.UD6,d.UD7,d.UD8,
															d.Fiscal_Year,d.Month1,d.Month2,d.Month3,d.Month4,d.Month5,d.Month6,
															d.Month7,d.Month8,d.Month9,d.Month10,d.Month11,d.Month12,
															d.Quarter1,d.Quarter2,d.Quarter3,d.Quarter4,d.Yearly,
															d.AllowUpdate,d.Create_Date,d.Create_User,d.Update_Date,d.Update_User
														FROM XFC_CMD_SPLN_REQ_Details d WITH (NOLOCK)
														INNER JOIN XFC_CMD_SPLN_REQ r WITH (NOLOCK) 
														ON d.CMD_SPLN_REQ_ID = r.CMD_SPLN_REQ_ID
														WHERE r.WFCMD_Name = '{wfInfoDetails("CMDName")}'
														AND r.WFTime_Name = '{wfInfoDetails("TimeName")}'
														AND r.WFScenario_Name = '{sTargetScenario}'
														AND r.ENTITY = '{api.pov.Entity.Name}'
														AND r.REQ_ID_Type = 'CivPay_Copied'"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & 	SqlREQDetail)	

						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, currREQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						
						Dim srcREQDT As DataTable = currREQDT.Clone()
						Dim srcREQDetailDT As DataTable = currREQDetailDT.Clone()
						For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
							BRApi.ErrorLog.LogMessage(si, "fOR 1")
							Dim req_ID As New Guid()
							Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
							Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
							Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
							Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
							Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
							Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
							Dim UD7 As String = FundingLineCell.DataBufferCellPk.GetUD7Name(api)
							Dim parentRow As DataRow = Nothing
							Dim currparentRow As DataRow = Nothing

							' Check if parent row exists in REQDT using key fields
							If Not srcREQDT Is Nothing Then
								parentRow = srcREQDT.Select($"APPN = '{UD1}' AND MDEP = '{UD2}' AND APE9 = '{UD3}' AND Dollar_Type = '{UD4}' AND Obj_Class = 'Pay_General' AND Ctype = '{UD5}' AND UIC = 'None'").FirstOrDefault()
								If parentRow Is Nothing Then
									Dim row = srcREQDT.NewRow()
									row("WFScenario_Name") = sTargetScenario
									row("WFTime_Name") = wfInfoDetails("TimeName")
									row("WFCMD_Name") = api.Pov.Cube.Name
									row("Entity") = sEntity
									row("Status") = statuslvl
									row("Title") = $"CivPay - {sEntity} - {UD1}-{UD2}-{UD3}-{UD4}-{UD5}"
									row("APPN") = UD1
									row("MDEP") = UD2
									row("APE9") = UD3
									row("Dollar_Type") = UD4
									row("Ctype") = UD5
									row("Obj_Class") = "Pay_General"
									row("UIC") = "None"
									row("REQ_ID_Type") = "CivPay_Copied"
									row("REQ_ID") = GetNextREQID(sEntity)
									row("Create_Date") = DateTime.Now
									row("Create_User") = si.UserName
									row("Update_Date") = DateTime.Now
									row("Update_User") = si.UserName	
									currparentRow = currREQDT.Select($"APPN = '{UD1}' AND MDEP = '{UD2}' AND APE9 = '{UD3}' AND Dollar_Type = '{UD4}' AND Obj_Class = 'Pay_General' AND Ctype = '{UD5}' AND UIC = 'None'").FirstOrDefault()
									If currparentRow Is Nothing Then
										req_ID = Guid.NewGuid()
										row("CMD_SPLN_REQ_ID") = req_ID
									Else
										req_ID = currparentRow("CMD_SPLN_REQ_ID")
										row("CMD_SPLN_REQ_ID") = req_ID
									End If
									srcREQDT.Rows.Add(row)
								Else
									req_ID = parentRow("CMD_SPLN_REQ_ID")								
								End If

								Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
								APPN = lsAncestorListU1(0).Member.Name
								For Each account As String In accounts
									BRApi.ErrorLog.LogMessage(si, "fOR 2")
									Dim detailRow = srcREQDetailDT.NewRow()
									detailRow("CMD_SPLN_REQ_ID") = req_ID
									detailRow("WFScenario_Name") = api.pov.Scenario.Name
									detailRow("WFTime_Name") = wfInfoDetails("TimeName")
									detailRow("WFCMD_Name") = wfInfoDetails("CMDName")
									detailRow("Unit_of_Measure") = "Funding"
									detailRow("Entity") = sEntity
									detailRow("IC") = "None"
									If account.XFEqualsIgnoreCase("Commit")
										detailRow("Account") = $"{account}ments"
									Else
										detailRow("Account") = $"{account}s"
									End If
									detailRow("Flow") = statuslvl
									detailRow("UD1") = UD1
									detailRow("UD2") = UD2
									detailRow("UD3") = UD3
									detailRow("UD4") = UD4
									detailRow("UD5") = UD5
									detailRow("UD6") = "Pay_General"
									detailRow("UD7") = "None"
									detailRow("UD8") = "None"
									detailRow("Fiscal_Year") = wfInfoDetails("TimeName")
									detailRow("Create_Date") = DateTime.Now
									detailRow("Create_User") = si.UserName
									detailRow("Update_Date") = DateTime.Now
									detailRow("Update_User") = si.UserName
									
									Dim rows = From row In cmd_Appn_Rates.AsEnumerable()
											   		Where row.Field(Of String)("UD1") = APPN _
												    AndAlso row.Field(Of String)("Account").StartsWith(account)
						           			   Select row
									If rows.FirstOrDefault().Item("Time13") <> 0
										Dim detailCarryOverRow = srcREQDetailDT.NewRow()
										detailCarryOverRow = detailRow
										detailCarryOverRow("Fiscal_Year") = (wfInfoDetails("TimeName").xfConverttoInt() + 1).xftoString()
										detailCarryOverRow("Yearly") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time13")/100
										srcREQDetailDT.Rows.Add(detailCarryOverRow)
									End If
									detailRow("Month1") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time1")/100
									detailRow("Month2") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time2")/100
									detailRow("Month3") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time3")/100
									detailRow("Month4") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time4")/100
									detailRow("Month5") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time5")/100
									detailRow("Month6") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time6")/100
									detailRow("Month7") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time7")/100
									detailRow("Month8") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time8")/100
									detailRow("Month9") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time9")/100
									detailRow("Month10") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time10")/100
									detailRow("Month11") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time11")/100
									detailRow("Month12") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time12")/100
									BRApi.ErrorLog.LogMessage(si, "fOR 4")
									srcREQDetailDT.Rows.Add(detailRow)
								Next
							End If
								
							globals.SetStringValue($"FundsCenterStatusUpdates - {api.Pov.Entity.Name}", $"{statuslvl}")
						
						Next
						BRApi.ErrorLog.LogMessage(si, "aFTER")	

					'create a new Data row for req and detail
						sqaREQReader.MergeandSync_XFC_CMD_SPLN_REQ_toDB(si,srcREQDT,currREQDT)
						
						sqaREQDetailReader.MergeandSync_XFC_CMD_SPLN_REQ_Details_toDB(si,srcREQDetailDT,currREQDetailDT)			
				 	End Using
				 End Using
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
				Dim sSourcePosition As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As String = ""' args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim sTargetScenario As String = "CMD_SPLN_C" & wfInfoDetails("TimeName")
				Dim currentYear As Integer = Convert.ToInt32(wfInfoDetails("TimeName"))
				Dim nextyear As Integer = currentYear  + 1
				Dim sTargetYear As Integer = wfInfoDetails("TimeName").XFConvertToInt
				Dim sEntity As String = api.Pov.Entity.Name
				Dim levl As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim statuslvl As String = levl & "_Formulate_SPLN"
				
				Dim cmd_WH_Rates As New DataTable()
				If globals.GetObject($"CMD WH Rates - {wfInfoDetails("CMDName")}") Is Nothing
					cmd_WH_Rates = GetFDXCMDSPLNData("CMD_SPLN_WH_SpreadPct_FDX_CV",wfInfoDetails("CMDName"))
					globals.SetObject($"CMD WH Rates - {wfInfoDetails("CMDName")}",cmd_WH_Rates)
				Else
					cmd_WH_Rates = globals.GetObject($"CMD WH Rates - {wfInfoDetails("CMDName")}")
				End If
				Dim APPN As String  = String.Empty
				Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(Cb#{cmd}:E#{sEntity}:S#{sSourcePosition}:T#{sTargetYear}:C#Aggregated:V#Periodic:F#Tot_Dist_Final:O#Top:I#None:U7#Top:U8#None),[A#TGT_WH])"

				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
				Dim accounts As New List(Of String) From {"Commit", "Obligation"}
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

				Dim currREQDetailDT As DataTable = New DataTable("REQDetailDT")
				Dim currREQDT As DataTable = New DataTable("REQDT")
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
						'Get header
						Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					    Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
						
						Dim SqlREQ As String = $"SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, Review_Entity, REQ_ID_Type, Demotion_Comment,
														    Related_REQs
								     				   FROM XFC_CMD_SPLN_REQ WITH (NOLOCK)
											           WHERE WFCMD_Name = '{wfInfoDetails("CMDName")}'
											           And WFTime_Name = '{wfInfoDetails("TimeName")}'
											           AND WFScenario_Name = '{sTargetScenario}'
											           AND ENTITY = '{api.pov.Entity.Name}'
													   and REQ_ID_Type = 'Withhold' "
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & 	SqlREQ)					
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
						sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, currREQDT, SqlREQ, sqlparamsREQ)
					
					    'Details
						Dim SqlREQDetail As String = $"SELECT d.CMD_SPLN_REQ_ID,d.WFScenario_Name,d.WFCMD_Name,d.WFTime_Name,d.Unit_of_Measure,
															d.Entity,d.IC,d.Account,d.Flow,d.UD1,d.UD2,d.UD3,d.UD4,d.UD5,d.UD6,d.UD7,d.UD8,
															d.Fiscal_Year,d.Month1,d.Month2,d.Month3,d.Month4,d.Month5,d.Month6,
															d.Month7,d.Month8,d.Month9,d.Month10,d.Month11,d.Month12,
															d.Quarter1,d.Quarter2,d.Quarter3,d.Quarter4,d.Yearly,
															d.AllowUpdate,d.Create_Date,d.Create_User,d.Update_Date,d.Update_User
														FROM XFC_CMD_SPLN_REQ_Details d WITH (NOLOCK)
														INNER JOIN XFC_CMD_SPLN_REQ r WITH (NOLOCK) 
														ON d.CMD_SPLN_REQ_ID = r.CMD_SPLN_REQ_ID
														WHERE r.WFCMD_Name = '{wfInfoDetails("CMDName")}'
														AND r.WFTime_Name = '{wfInfoDetails("TimeName")}'
														AND r.WFScenario_Name = '{sTargetScenario}'
														AND r.ENTITY = '{api.pov.Entity.Name}'
														AND r.REQ_ID_Type = 'Withhold'"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & 	SqlREQDetail)	

						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, currREQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						
						Dim srcREQDT As DataTable = currREQDT.Clone()
						Dim srcREQDetailDT As DataTable = currREQDetailDT.Clone()
						For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
							BRApi.ErrorLog.LogMessage(si, "fOR 1")
							Dim req_ID As New Guid()
							Dim UD1 As String = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
							Dim UD2 As String = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
							Dim UD3 As String = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
							Dim UD4 As String = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
							Dim UD5 As String = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
							Dim UD6 As String = FundingLineCell.DataBufferCellPk.GetUD6Name(api)
							APPN = UD3.Split("_")(0)
							If  (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General"))  Then
								UD6= "Pay_General"
							Else
								UD6 = "NonPay_General"
							End If
							Dim UD7 As String = FundingLineCell.DataBufferCellPk.GetUD7Name(api)
							Dim parentRow As DataRow = Nothing
							Dim currparentRow As DataRow = Nothing

							' Check if parent row exists in REQDT using key fields
							If Not srcREQDT Is Nothing Then
								parentRow = srcREQDT.Select($"APPN = '{APPN}'").FirstOrDefault()
								If parentRow Is Nothing Then
									Dim row = srcREQDT.NewRow()
									row("WFScenario_Name") = sTargetScenario
									row("WFTime_Name") = wfInfoDetails("TimeName")
									row("WFCMD_Name") = api.Pov.Cube.Name
									row("Entity") = sEntity
									row("Status") = statuslvl
									row("Title") = $"Withhold - {sEntity} - {APPN}"
									row("APPN") = APPN
									row("MDEP") = "None"
									row("APE9") = "None"
									row("Dollar_Type") = "None"
									row("Ctype") =  "None"
									row("Obj_Class") =  "None"
									row("UIC") = "None"
									row("REQ_ID_Type") = "Withhold"
									row("REQ_ID") = GetNextREQID(sEntity)
									row("Create_Date") = DateTime.Now
									row("Create_User") = si.UserName
									row("Update_Date") = DateTime.Now
									row("Update_User") = si.UserName	
									currparentRow = currREQDT.Select($"APPN = '{APPN}'").FirstOrDefault()
									If currparentRow Is Nothing Then
										req_ID = Guid.NewGuid()
										row("CMD_SPLN_REQ_ID") = req_ID
									Else
										req_ID = currparentRow("CMD_SPLN_REQ_ID")
										row("CMD_SPLN_REQ_ID") = req_ID
									End If
									srcREQDT.Rows.Add(row)
								Else
									req_ID = parentRow("CMD_SPLN_REQ_ID")								
								End If

								Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
								APPN = lsAncestorListU1(0).Member.Name
								For Each account As String In accounts
									BRApi.ErrorLog.LogMessage(si, "fOR 2")
									Dim detailRow = srcREQDetailDT.NewRow()
									detailRow("CMD_SPLN_REQ_ID") = req_ID
									detailRow("WFScenario_Name") = api.pov.Scenario.Name
									detailRow("WFTime_Name") = wfInfoDetails("TimeName")
									detailRow("WFCMD_Name") = wfInfoDetails("CMDName")
									detailRow("Unit_of_Measure") = "Funding"
									detailRow("Entity") = sEntity
									detailRow("IC") = "None"
									If account.XFEqualsIgnoreCase("Commit")
										detailRow("Account") = $"WH_{account}ments"
									Else
										detailRow("Account") = $"WH_{account}s"
									End If
									detailRow("Flow") = statuslvl
									detailRow("UD1") = UD1
									detailRow("UD2") = UD2
									detailRow("UD3") = UD3
									detailRow("UD4") = UD4
									detailRow("UD5") = UD5
									detailRow("UD6") = UD6
									detailRow("UD7") = "None"
									detailRow("UD8") = "None"
									detailRow("Fiscal_Year") = wfInfoDetails("TimeName")
									detailRow("Create_Date") = DateTime.Now
									detailRow("Create_User") = si.UserName
									detailRow("Update_Date") = DateTime.Now
									detailRow("Update_User") = si.UserName
									
									Dim rows = From row In cmd_WH_Rates.AsEnumerable()
											   		Where row.Field(Of String)("Account").StartsWith(account)
						           			   Select row
									detailRow("Month1") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time1")/100
									detailRow("Month2") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time2")/100
									detailRow("Month3") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time3")/100
									detailRow("Month4") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time4")/100
									detailRow("Month5") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time5")/100
									detailRow("Month6") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time6")/100
									detailRow("Month7") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time7")/100
									detailRow("Month8") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time8")/100
									detailRow("Month9") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time9")/100
									detailRow("Month10") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time10")/100
									detailRow("Month11") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time11")/100
									detailRow("Month12") = FundingLineCell.CellAmount * rows.FirstOrDefault().Item("Time12")/100
									srcREQDetailDT.Rows.Add(detailRow)
								Next
							End If
								
							globals.SetStringValue($"FundsCenterStatusUpdates - {api.Pov.Entity.Name}", $"{statuslvl}")
						
						Next

					'create a new Data row for req and detail
						sqaREQReader.MergeandSync_XFC_CMD_SPLN_REQ_toDB(si,srcREQDT,currREQDT)
						
						sqaREQDetailReader.MergeandSync_XFC_CMD_SPLN_REQ_Details_toDB(si,srcREQDetailDT,currREQDetailDT)			
				 	End Using
				 End Using
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

		Private Function GetFDXCMDSPLNData(ByVal cvName As String,ByVal entFilter As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "50 CMD SPLN"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{wfInfoDetails("ScenarioName")}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)

			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 8, False)

			Return dt
		End Function
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