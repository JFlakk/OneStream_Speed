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
					Me.Copy_CivPay(globals)
				End If


            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

#Region "Load_Reqs_to_Cube"
		Public Sub Load_Reqs_to_Cube()
System.Threading.Thread.Sleep(100)			
'Brapi.ErrorLog.LogMessage(si,"@HERE1 Load_Reqs_to_Cube")
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			Dim POVPeriodNum As Integer = api.Time.GetPeriodNumFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim POVYear As Integer = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim wfStartTimeID As Integer = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId)
			Dim wfStartYear As String = api.Time.GetNameFromId(wfStartTimeID)
		    Dim table_Month As String = api.Pov.Time.Name.Replace(POVYear & "M","")
'brapi.ErrorLog.LogMessage(si,"POVYear  & wfStartYear: " & POVYear & ": " & wfStartYear)
			Dim sumcolumn As String = String.Empty
			If POVYear = wfStartYear.Substring(0,4) Then 
				sumcolumn = "Month" & table_Month
			Else 
				sumcolumn = "Yearly"
			End If 
			Dim inClause As String
			
			Dim tgt_Origin As String = "Import"
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")	
			Dim EntParent = BRApi.Finance.Members.HasChildren(si, entDimPk, api.Pov.Entity.MemberId)
			
			If EntParent Then
				tgt_Origin = "AdjInput"
			End If
			
brapi.ErrorLog.LogMessage(si,"tgt_Origin: " & tgt_Origin)				
			Dim StatusbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusUpdates - {api.pov.entity.name}",String.Empty)
			Dim appnbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusappnUpdates - {api.pov.entity.name}",String.Empty)
			Dim StatusList As New List(Of String) 
			If StatusbyFundsCenter.xfcontainsIgnoreCase("|")
				
				StatusList = StringHelper.SplitString(StatusbyFundsCenter,"|")
				Dim prefixedStatuses As List(Of String) = StatusList.Select(Function(status) $"F#{status}").ToList()
				inClause = String.Join(",", StatusList.Select(Function(status) $"'{status}'"))
	
				
				If StatusbyFundsCenter <> String.Empty
					StatusbyFundsCenter = $",[{String.Join(", ", prefixedStatuses)}]"
				End If
			Else
				inClause = $"'{StatusbyFundsCenter}'"
				StatusbyFundsCenter = $",[{StatusbyFundsCenter}]"
			End If
			
			'brapi.ErrorLog.LogMessage(si,$"zHit: {StatusbyFundsCenter}")
	
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[O#{tgt_Origin}]{StatusbyFundsCenter})")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
	
			'Define the SQL Statement 
			Dim sql = $"SELECT Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, SUM({sumcolumn}) AS Tot_Annual
						FROM XFC_CMD_SPLN_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						and fiscal_year = '{POVYear}'
						AND Entity = '{api.Pov.Entity.Name}'
						AND Account In ('Commitments','Obligations')
						AND Flow In ({inClause})
						Group By Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
'brapi.ErrorLog.LogMessage(si,"sql: " & sql)	
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
							ReqDataCell.DataBufferCellPk.SetOrigin(api, tgt_Origin)
							ReqDataCell.DataBufferCellPk.SetUD1(api, reader("UD1").ToString())
							ReqDataCell.DataBufferCellPk.SetUD2(api, reader("UD2").ToString())
							ReqDataCell.DataBufferCellPk.SetUD3(api, reader("UD3").ToString())
							ReqDataCell.DataBufferCellPk.SetUD4(api, reader("UD4").ToString())
							ReqDataCell.DataBufferCellPk.SetUD5(api, reader("UD5").ToString())
							ReqDataCell.DataBufferCellPk.SetUD6(api, reader("UD6").ToString())
							ReqDataCell.DataBufferCellPk.SetUD7(api, reader("UD7").ToString())
							ReqDataCell.DataBufferCellPk.SetUD8(api, reader("UD8").ToString())
	
							If Not IsDBNull(reader("Tot_Annual")) AndAlso reader("Tot_Annual") <> 0 Then
								ReqDataCell.CellAmount = Convert.ToDecimal(reader("Tot_Annual"))
								destBuffer.DataBufferCells.Add(ReqDataCell.DataBufferCellPk, ReqDataCell)
								CurrCubeBuffer.DataBufferCells.Remove(ReqDataCell.DataBufferCellPk)
							End If
						End While	
					End If
				End Using
			End Using

			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
'brapi.ErrorLog.LogMessage(si,"**Hit count: " & destBuffer.DataBufferCells.Count)
			If destBuffer.DataBufferCells.Count > 0 Then
	
destBuffer.LogDataBuffer(api,"@ DataBuffer: " & api.Pov.Entity.Name & " : "  & api.Pov.Time.Name ,1000)
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

#Region "Copy_CivPay"
'Updated 12/03/2024 by Fronz - changed BO5 to BOC & added the 'Clear Existing Data' lines of code
		Public Sub Copy_CivPay(ByVal globals As BRGlobals)
			Try		
				Dim sSourcePosition As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourcePB")
				Dim req_id_val As guid = args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim sTargetFirstYear As String = api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim sTargetYear As Integer = api.Pov.Time.Name.XFConvertToInt
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top,[A#BO6,A#BOC])"

				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

                Dim BufferFY1Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY1 = api.Data.GetDataBufferUsingFormula(BufferFY1Script)
			
						
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				 
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
					    'Details
					    Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
						Dim SqlREQDetail As String = $"SELECT * 
								     				   FROM XFC_CMD_SPLN_REQ_Details
											           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
											           And WFTime_Name = '{api.pov.time.Name}'
											           AND WFScenario_Name = '{api.pov.Scenario.Name}'
											           AND ENTITY = '{api.pov.Entity.Name}'
						 					           and CMD_SPLN_REQ_ID = '{req_id_val}'"
						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("CMD_SPLN_REQ_ID"),
												 REQDetailDT.Columns("Start_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD5")}
				
'buffer log								 
dbfundinglines.LogDataBuffer(api,"manpower test",1000)
				
				For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
					Dim Acct = "CIV_FTE"
					Dim UoM = "CIV_FTE"
					Dim cPROBE_Acct = "BO6"

					If 	FundingLineCell.DataBufferCellPk.GetAccountName(api) = "BOC" Then
							Acct = "Req_Funding"
							UoM = "CIV_COST"
							cPROBE_Acct = "BOC"
					End If 
						Dim isinsert As Boolean = False

						Dim row As DataRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = '{Acct}'
																	AND CMD_SPLN_REQ_ID = '{req_id_val}' 
																	AND WFCMD_Name = '{api.pov.cube.name}' 
																	AND Unit_of_Measure = '{UoM}' 
																	AND Entity = '{api.pov.entity.name}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
				
						If IsNothing(row) Then
							isinsert = True
							row = REQDetailDT.NewRow()
						End If 
	
						row("WFScenario_Name") = api.pov.Scenario.Name
						row("WFTime_Name") = api.Pov.Time.Name
						row("CMD_SPLN_REQ_ID") = req_id_val
						row("WFCMD_Name") = api.Pov.Cube.Name
						row("Unit_of_Measure") = UoM 
						Row("Entity") = api.Pov.Entity.Name
						Row("IC") = "None"
						Row("Account") = Acct
						Row("Flow") = "L2_Formulate_PGM"
						Row("Start_Year") = api.Pov.Time.Name
						Row("FY_1") = GetBCValue(FundingLineCell,BufferFY1,cPROBE_Acct)
						Row("FY_Total") = Row("FY_1")
						row("UD1") = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
						row("UD2") = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
						row("UD3") = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
						row("UD4") = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
						row("UD5") = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
						row("UD6") = "Pay_General"
						row("UD7") = "None"
						row("UD8") = "None"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
					
						If isinsert Then
							REQDetailDT.Rows.Add(row)								
						End If 
'					Next
				Next
	
					'create a new Data row for req and detail
					sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)	
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
				'Dim sSourcePosition As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourcePB")
				Dim sSourcePosition As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim req_id_val As guid = args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim sTargetFirstYear As String = api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim sTargetYear As Integer = api.Pov.Time.Name.XFConvertToInt
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top,[A#Withhold])"

				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

                Dim BufferFY1Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#Withhold]"
                Dim BufferFY1 = api.Data.GetDataBufferUsingFormula(BufferFY1Script)

						
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				 
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
					    'Details
					    Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
						Dim SqlREQDetail As String = $"SELECT * 
								     				   FROM XFC_CMD_SPLN_REQ_Details
											           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
											           And WFTime_Name = '{api.pov.time.Name}'
											           AND WFScenario_Name = '{api.pov.Scenario.Name}'
											           AND ENTITY = '{api.pov.Entity.Name}'
						 					           and CMD_SPLN_REQ_ID = '{req_id_val}'"
						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("CMD_SPLN_REQ_ID"),
												 REQDetailDT.Columns("Start_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD5")}
				
'buffer log								 
'dbfundinglines.LogDataBuffer(api,"Withhold test",1000)
				
				For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
					Dim Acct = "TGT_WH"
					Dim UoM = "WH"
					
						Dim isinsert As Boolean = False

						Dim row As DataRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = '{Acct}'
																	AND CMD_SPLN_REQ_ID = '{req_id_val}' 
																	AND WFCMD_Name = '{api.pov.cube.name}' 																	
																	AND Entity = '{api.pov.entity.name}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
				
						If IsNothing(row) Then
							isinsert = True
							row = REQDetailDT.NewRow()
						End If 
	
						row("WFScenario_Name") = api.pov.Scenario.Name
						row("WFTime_Name") = api.Pov.Time.Name
						row("CMD_SPLN_REQ_ID") = req_id_val
						row("WFCMD_Name") = api.Pov.Cube.Name
						row("Unit_of_Measure") = UoM 
						Row("Entity") = api.Pov.Entity.Name
						Row("IC") = "None"
						Row("Account") = Acct
						Row("Flow") = "L2_Formulate_SPLN"
						Row("Start_Year") = api.Pov.Time.Name
						Row("Month1") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month2") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month3") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month4") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month5") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month6") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month7") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month8") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month9") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month10") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month11") = GetBCValue(FundingLineCell,BufferFY1)
						Row("Month12") = GetBCValue(FundingLineCell,BufferFY1)
						row("UD1") = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
						row("UD2") = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
						row("UD3") = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
						row("UD4") = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
						row("UD5") = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
						row("UD6") = "Pay_General"
						row("UD7") = "None"
						row("UD8") = "None"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
					
						If isinsert Then
							REQDetailDT.Rows.Add(row)								
						End If 
'					Next
				Next
	
					'create a new Data row for req and detail
					sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)	
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

#Region "Get Inflation Rate"		
		
		Public Function GetInflationRate(ByRef UD1 As String) As Decimal
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim cubeName As String = wfInfoDetails("CMDName")
			Dim entity As String = GetCmdFundCenterFromCube()
			Dim scenario As String = wfInfoDetails("ScenarioName")
			Dim tm As String = wfInfoDetails("TimeName")
			Dim mbrScript = $"Cb#{cubeName}:E#{entity}:C#Local:S#{scenario}:T#{tm}:V#Periodic:A#Inflation_Rate:F#None:O#AdjInput:I#None:U1#{UD1}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim inflationRate As Decimal = 0
			inflationRate = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, mbrScript).DataCellEx.DataCell.CellAmount
			inflationRate = inflationRate/100
			Return inflationRate
			
		End Function
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
