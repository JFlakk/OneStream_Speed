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
Imports Microsoft.Data.SqlClient

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_SPLN_Rollover_Helper
	Public Class MainClass
			
		Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
			
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
	
			Try
	            Me.si = si
	            Me.globals = globals
	            Me.api = api
	            Me.args = args		
				
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						If args.FunctionName.XFEqualsIgnoreCase("RollFwdReq") Then
							
							'Implement Dashboard Component Selection Changed logic here.
'BRApi.ErrorLog.LogMessage(si,"in Rollover -: " & args.NameValuePairs.XFGetValue("rolloverREQs"))		
							Me.RollFwdReq()
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							selectionChangedTaskResult.Message = ""
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							Return selectionChangedTaskResult
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function RollFwdReq()As Object

			Dim sqa As New SqlDataAdapter()
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim scName As String = wfInfoDetails("ScenarioName")
			Dim cmd As String = wfInfoDetails("CMDName")
			Dim tm As String = wfInfoDetails("TimeName")
			
			Dim PGM_SscName As String = "CMD_PGM_C" & tm
			Dim PGMRQSdt As New DataTable ("REQsFromPGM")
			Dim PGMfinalStatus As String = "L2_Final_PGM"
			Dim PGMAccount As String = "REQ_Funding"
			
			Dim PGMREQsDT As New DataTable ("PGMREQsDT")
			Dim PGMREQDetailsDT As New DataTable ("PGMREQDetailsDT")
			
			Dim SPLNREQsDT As New DataTable ("SPLNREQsDT")
			Dim SPLNREQDetailsDT As New DataTable ("SPLNREQDetailsDT")
			
'			Dim CMD_PGM_REQ_IDs As String = args.NameValuePairs("rolloverREQs")
'			'if no REQs to roll over simply end
'			If String.IsNullOrWhiteSpace(CMD_PGM_REQ_IDs) Then
'				Return Nothing
'			End If
			
''BRApi.ErrorLog.LogMessage(si, "rolloverREQs: " & CMD_PGM_REQ_IDs)				
'			CMD_PGM_REQ_IDs = CMD_PGM_REQ_IDs.Replace(" ","")
'			If CMD_PGM_REQ_IDs.Length > 0 Then
'				CMD_PGM_REQ_IDs = "'" & CMD_PGM_REQ_IDs.Replace(",","','") & "'"
'			End If
	
			'Dim REQDT As DataTable = New DataTable()
			'Dim SPLNREQDetailsDT As DataTable = New DataTable()
	       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
	            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
	                sqlConn.Open()
					
					'Get reqs to be rolled over from PGM
					Dim sqaPGMReqReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					Dim sqlPGMReq As String = $"Select *
										FROM XFC_CMD_PGM_REQ AS Req
										WHERE Req.WFScenario_Name = '{PGM_SscName}'
										AND Req.WFCMD_Name = '{cmd}'
										AND Req.WFTime_Name = '{tm}'
										AND Req.Status = '{PGMfinalStatus}'"
'BRApi.ErrorLog.LogMessage(si, "sqlPGMReq: " & sqlPGMReq.ToString)		
					Dim sqlparamsPGMReq As SqlParameter() = New SqlParameter() {}
	                sqaPGMReqReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, PGMREQsDT, sqlPGMReq, sqlparamsPGMReq)
				
					
					'Prepare Detail	
					'Dim GUIDs As String = Me.getGUID(REQDT)
					Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					Dim SqlPGMReqDetail As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ_Details
										WHERE WFScenario_Name = '{PGM_SscName}'
										And WFCMD_Name = '{cmd}'
										AND WFTime_Name = '{tm}'
										AND Flow = '{PGMfinalStatus}'
										AND Account = 'Req_Funding'"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & SqlPGMReqDetail.ToString)		
					Dim sqlparamsPGMReqDetails As SqlParameter() = New SqlParameter() {}
	                sqaReqDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, PGMREQDetailsDT, SqlPGMReqDetail, sqlparamsPGMReqDetails)
				
					
					'Get existing SPLN Reqs
	                Dim sqaSPLNReqReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
	                Dim SqlSPLNReq As String = $"SELECT * 
										FROM XFC_CMD_SPLN_REQ
										WHERE 
										WFScenario_Name = '{scName}'
										And WFCMD_Name = '{cmd}'
										AND WFTime_Name = '{tm}'"
'BRApi.ErrorLog.LogMessage(si, "SqlSPLNReq: " & SqlSPLNReq.ToString)	
					Dim sqlparamsSPLNReq As SqlParameter() = New SqlParameter() {}
	                sqaSPLNReqReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SPLNREQsDT, SqlSPLNReq, sqlparamsSPLNReq)
					
					Dim sqaSPLNReqDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
					Dim SqlSPLNReqDetail As String = $"SELECT * 
										FROM XFC_CMD_SPLN_REQ_Details
										WHERE
										WFScenario_Name = '{scName}'
										And WFCMD_Name = '{cmd}'
										AND WFTime_Name = '{tm}'
										AND Account = 'Req_Funding'"
'BRApi.ErrorLog.LogMessage(si, "SqlSPLNReqDetail: " & SqlSPLNReqDetail.ToString)		
					Dim sqlparamsSPLNReqDetails As SqlParameter() = New SqlParameter() {}
	                sqaSPLNReqDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, SPLNREQDetailsDT, SqlSPLNReqDetail, sqlparamsSPLNReqDetails)
					
					'create the new record
					Dim newREQRows As New List(Of DataRow)
					Dim newREQDetailRows As New List(Of DataRow)
					
					Dim commitSpread As New Dictionary(Of Integer, Decimal)
					Dim obligSpread As New Dictionary(Of Integer, Decimal)
					
					For Each row As DataRow In PGMREQsDT.Rows
						Dim newREQRow As datarow = SPLNREQsDT.NewRow()
						MapPGMReqToSPLN(newREQRow, row)
						
						newREQRows.Add(newREQRow)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 0")							
						'create details
						Dim foundDetailRows As DataRow() = PGMREQDetailsDT.Select(String.Format("CMD_PGM_REQ_ID = '{0}'", row("CMD_PGM_REQ_ID").ToString))
						For Each dtRow In foundDetailRows
BRApi.ErrorLog.LogMessage(si, "rollover update: 1")								
							Dim newOblDetailREQRow As datarow = SPLNREQDetailsDT.NewRow()
							Me.MapPGMReqDetailToSPLN(newOblDetailREQRow, dtRow, "Obligations", newREQRow("CMD_SPLN_REQ_ID").ToString,commitSpread)
							UpdateAuditColumns(newOblDetailREQRow)
							newREQDetailRows.Add(newOblDetailREQRow)
							
							Dim newCmtDetailREQRow As datarow = SPLNREQDetailsDT.NewRow()
							Me.MapPGMReqDetailToSPLN(newCmtDetailREQRow, dtRow, "Commitments", newREQRow("CMD_SPLN_REQ_ID").ToString, obligSpread)
							UpdateAuditColumns(newCmtDetailREQRow)
							newREQDetailRows.Add(newCmtDetailREQRow)
							
						Next
BRApi.ErrorLog.LogMessage(si, "rollover update: 2")							
					Next
					
					'update SQL Adapter and update
					Dim REQ_IDs As New List(Of String)
					For Each row In newREQRows
						SPLNREQsDT.Rows.Add(row)
						REQ_IDs.Add(row("REQ_ID").ToString)
					Next
					
					'Add the new rows to the original dataset
					For Each row In newREQDetailRows
						SPLNREQDetailsDT.Rows.Add(row)
					Next
					
					'Call delete To delte rows
					Dim existingREQs As New List(Of String)
					existingREQs = Me.GetExistingDupREQs(REQ_IDs)
					
					If existingREQs.Count > 0 Then
						Dim deleter As New CMD_SPLN_Helper.MainClass
						Args.NameValuePairs.Add("req_IDs", String.Join(",", existingREQs))
						Args.NameValuePairs.Add("Action", "Delete")
						deleter.main(si, globals, api, args)
					End If
					
BRApi.ErrorLog.LogMessage(si, "rollover update: 3")					
					sqaSPLNReqReader.Update_XFC_CMD_SPLN_REQ(SPLNREQsDT, sqa)
					sqaSPLNReqDetailReader.Update_XFC_CMD_SPLN_REQ_Details(SPLNREQDetailsDT, sqa)
BRApi.ErrorLog.LogMessage(si, "rollover update: 4")					
					'Load to the cube
					Dim loader As New CMD_SPLN_Helper.MainClass
					Args.NameValuePairs("req_IDs") =  String.Join(",", REQ_IDs)
					Args.NameValuePairs("Action") = "Insert"
					Args.NameValuePairs.Add("new_Status", "Formulate") '*** HARD CODE FOR TEST ***
BRApi.ErrorLog.LogMessage(si, "rollover update: 4.5")						
					loader.main(si, globals, api, args)	
BRApi.ErrorLog.LogMessage(si, "rollover update: 5")						
				End Using
			End Using

		End Function

		
		Public Sub UpdateAuditColumns(ByRef newRow As DataRow) 
			
			'Update Audit columns
			newRow("Create_Date") = DateTime.Now
			newRow("Create_User") = si.UserName
			newRow("Update_Date") = DateTime.Now
			newRow("Update_User") = si.UserName
		End Sub
	
		Public Sub MapPGMReqToSPLN(ByRef SPLNRow As DataRow, ByRef PGMRow As DataRow) 
BRApi.ErrorLog.LogMessage(si, "rollover mapping: " & SPLNRow.Table.Columns.Count)			
Dim columns As String			
For i As Integer = 0 To SPLNRow.Table.Columns.Count - 1
	columns = columns & " | " & SPLNRow.Table.Columns(i).ColumnName
Next
'BRApi.ErrorLog.LogMessage(si, "columns : " & columns )
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			SPLNRow("CMD_SPLN_REQ_ID") = Guid.NewGuid()
			SPLNRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
			SPLNRow("WFCMD_Name") = PGMRow("WFCMD_Name")
			SPLNRow("WFTime_Name") = wfInfoDetails("TimeName")
			SPLNRow("REQ_ID_Type") = PGMRow("REQ_ID_Type")
			SPLNRow("REQ_ID") = PGMRow("REQ_ID")
			SPLNRow("Title") = PGMRow("Title")
			SPLNRow("Description") = PGMRow("Description")
			SPLNRow("Justification") = PGMRow("Justification")
			SPLNRow("Entity") = PGMRow("Entity")
			SPLNRow("APPN") = PGMRow("APPN")
			SPLNRow("MDEP") = PGMRow("MDEP")
			SPLNRow("APE9") = PGMRow("APE9")
			SPLNRow("Dollar_Type") = PGMRow("Dollar_Type")
			SPLNRow("Obj_Class") = PGMRow("Obj_Class")
			SPLNRow("CType") = PGMRow("CType")
			SPLNRow("UIC") = PGMRow("UIC")
			SPLNRow("Cost_Methodology") = PGMRow("Cost_Methodology")
			SPLNRow("Impact_Not_Funded") = PGMRow("Impact_Not_Funded")
			SPLNRow("Risk_Not_Funded") = PGMRow("Risk_Not_Funded")
			SPLNRow("Cost_Growth_Justification") = PGMRow("Cost_Growth_Justification")
			SPLNRow("Must_Fund") = PGMRow("Must_Fund")
			SPLNRow("Funding_Src") = PGMRow("Funding_Src")
			SPLNRow("Army_Init_Dir") = PGMRow("Army_Init_Dir")
			SPLNRow("CMD_Init_Dir") = PGMRow("CMD_Init_Dir")
			SPLNRow("Activity_Exercise") = PGMRow("Activity_Exercise")
			SPLNRow("Directorate") = PGMRow("Directorate")
			SPLNRow("Div") = PGMRow("Div")
			SPLNRow("Branch") = PGMRow("Branch")
			SPLNRow("IT_Cyber_REQ") = PGMRow("IT_Cyber_REQ")
			SPLNRow("Emerging_REQ") = PGMRow("Emerging_REQ")
			SPLNRow("CPA_Topic") = PGMRow("CPA_Topic")
			SPLNRow("PBR_Submission") = PGMRow("PBR_Submission")
			SPLNRow("UPL_Submission") = PGMRow("UPL_Submission")
			SPLNRow("Contract_Num") = PGMRow("Contract_Num")
			SPLNRow("Task_Order_Num") = PGMRow("Task_Order_Num")
			SPLNRow("Award_Target_Date") = PGMRow("Award_Target_Date")
			SPLNRow("POP_Exp_Date") = PGMRow("POP_Exp_Date")
			SPLNRow("CME") = PGMRow("Contractor_ManYear_Equiv_CME")
			SPLNRow("COR_Email") = PGMRow("COR_Email")
			SPLNRow("POC_Email") = PGMRow("POC_Email")
			SPLNRow("Review_POC_Email") = PGMRow("Review_POC_Email")
			SPLNRow("MDEP_Functional_Email") = PGMRow("MDEP_Functional_Email")
			SPLNRow("Notification_List_Emails") = PGMRow("Notification_List_Emails")
'			SPLNRow() = PGMRow("Gen_Comments_Notes")
'			SPLNRow() = PGMRow("JUON")
'			SPLNRow() = PGMRow("ISR_Flag")
'			SPLNRow() = PGMRow("Cost_Model")
'			SPLNRow() = PGMRow("Combat_Loss")
'			SPLNRow() = PGMRow("Cost_Location")
'			SPLNRow() = PGMRow("Cat_A_Code")
'			SPLNRow() = PGMRow("CBS_Code")
'			SPLNRow() = PGMRow("MIP_Proj_Code")
'			SPLNRow() = PGMRow("SS_Priority")
'			SPLNRow() = PGMRow("Commit_Group")
'			SPLNRow() = PGMRow("SS_Cap")
'			SPLNRow() = PGMRow("Strategic_BIN")
'			SPLNRow() = PGMRow("LIN")
'			SPLNRow() = PGMRow("REQ_Type")
'			SPLNRow() = PGMRow("DD_Priority")
'			SPLNRow() = PGMRow("Portfolio")
'			SPLNRow() = PGMRow("DD_Cap")
'			SPLNRow() = PGMRow("JNT_Cap_Area")
'			SPLNRow() = PGMRow("TBM_Cost_Pool")
'			SPLNRow() = PGMRow("TBM_Tower")
'			SPLNRow() = PGMRow("APMS_AITR_Num")
'			SPLNRow() = PGMRow("Zero_Trust_Cap")
'			SPLNRow() = PGMRow("Assoc_Directives")
'			SPLNRow() = PGMRow("Cloud_IND")
'			SPLNRow() = PGMRow("Strat_Cyber_Sec_PGM")
'			SPLNRow() = PGMRow("Notes")
			SPLNRow("FF_1") = PGMRow("FF_1")
			SPLNRow("FF_2") = PGMRow("FF_2")
			SPLNRow("FF_3") = PGMRow("FF_3")
			SPLNRow("FF_4") = PGMRow("FF_4")
			SPLNRow("FF_5") = PGMRow("FF_5")
			SPLNRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, PGMRow("Entity")) & "_Formulate_SPLN"
			'SPLNRow("Invalid") = PGMRow("Invalid")
			'SPLNRow("Val_Error") = PGMRow("Val_Error")
'			SPLNRow("Create_Date") = PGMRow("Create_Date")
'			SPLNRow("Create_User") = PGMRow("Create_User")
'			SPLNRow("Update_Date") = PGMRow("Update_Date")
'			SPLNRow("Update_User") = PGMRow("Update_User")
			'SPLNRow() = PGMRow("Related_REQs")
			SPLNRow("Review_Entity") = PGMRow("Review_Entity")
			SPLNRow("Demotion_Comment") = PGMRow("Demotion_Comment")
			SPLNRow("Related_REQs") = PGMRow("Validation_List_Emails")
			
			UpdateAuditColumns(SPLNRow)


		End Sub
			
		Public Sub MapPGMReqDetailToSPLN(ByRef SPLNDetailRow As DataRow, ByRef PGMDetailRow As DataRow, ByRef acct As String ,ByRef newGuid As String, ByRef spread As Dictionary(Of Integer, Decimal)) 
BRApi.ErrorLog.LogMessage(si, "PGM FY_1: " & PGMDetailRow("FY_1") & " , account = "	& acct)
			SPLNDetailRow("CMD_SPLN_REQ_ID") = newGuid
			SPLNDetailRow("WFScenario_Name") = PGMDetailRow("WFScenario_Name")
			SPLNDetailRow("WFCMD_Name") = PGMDetailRow("WFCMD_Name")
			SPLNDetailRow("WFTime_Name") = PGMDetailRow("WFTime_Name")
			SPLNDetailRow("Unit_of_Measure") = PGMDetailRow("Unit_of_Measure")
			SPLNDetailRow("Entity") = PGMDetailRow("Entity")
			SPLNDetailRow("IC") = PGMDetailRow("IC")
			SPLNDetailRow("Account") = acct
			SPLNDetailRow("Flow") = "202014D26"'PGMDetailRow("Flow")
			SPLNDetailRow("UD1") = PGMDetailRow("UD1")
			SPLNDetailRow("UD2") = PGMDetailRow("UD2")
			SPLNDetailRow("UD3") = PGMDetailRow("UD3")
			SPLNDetailRow("UD4") = PGMDetailRow("UD4")
			SPLNDetailRow("UD5") = PGMDetailRow("UD5")
			SPLNDetailRow("UD6") = PGMDetailRow("UD6")
			SPLNDetailRow("UD7") = PGMDetailRow("UD7")
			SPLNDetailRow("UD8") = PGMDetailRow("UD8")
			SPLNDetailRow("Fiscal_Year") = PGMDetailRow("Start_Year")
			
			If acct.XFEqualsIgnoreCase("Commitments") Then
				If spread.Count = 0 Then 
					GetMonthlySpread(SPLNDetailRow("UD1").ToString, "Commit_Spread_Pct", spread)
				End If
			Else 
				If acct.XFEqualsIgnoreCase("Obligations") Then
					If spread.Count = 0 Then 
						GetMonthlySpread(SPLNDetailRow("UD1").ToString, "Commit_Spread_Pct", spread)
					End If
				Else
					Throw New Exception("Invalid Spend Plan Account: " & acct)
				End If
			End If
			
			'If commitSpread.Count >= 12 Then
				SPLNDetailRow("Month1") = PGMDetailRow("FY_1") * spread(1)
BRApi.ErrorLog.LogMessage(si, "PGM FY_1: " & PGMDetailRow("FY_1") & " , M1 spread = " & spread(1))				
				SPLNDetailRow("Month2") = PGMDetailRow("FY_1") * spread(2)
				SPLNDetailRow("Month3") = PGMDetailRow("FY_1") * spread(3)
				SPLNDetailRow("Month4") = PGMDetailRow("FY_1") * spread(4)
				SPLNDetailRow("Month5") = PGMDetailRow("FY_1") * spread(5)
				SPLNDetailRow("Month6") = PGMDetailRow("FY_1") * spread(6)
				SPLNDetailRow("Month7") = PGMDetailRow("FY_1") * spread(7)
				SPLNDetailRow("Month8") = PGMDetailRow("FY_1") * spread(8)
				SPLNDetailRow("Month9") = PGMDetailRow("FY_1") * spread(9)
				SPLNDetailRow("Month10") = PGMDetailRow("FY_1") * spread(10)
				SPLNDetailRow("Month11") = PGMDetailRow("FY_1") * spread(11)
				SPLNDetailRow("Month12") = PGMDetailRow("FY_1") * spread(12)
			'End If
			
'			Dim oblSpread As New Dictionary(Of Integer, Decimal)
'			GetMonthlySpread(SPLNDetailRow("UD1").ToString, "Obligation_Spread_Pct", oblSpread)
'			'If oblSpread.Count  >= 12 Then
'				SPLNDetailRow("Month1") = PGMDetailRow("FY_1") * oblSpread(1)
'				SPLNDetailRow("Month2") = PGMDetailRow("FY_1") * oblSpread(2)
'				SPLNDetailRow("Month3") = PGMDetailRow("FY_1") * oblSpread(3)
'				SPLNDetailRow("Month4") = PGMDetailRow("FY_1") * oblSpread(4)
'				SPLNDetailRow("Month5") = PGMDetailRow("FY_1") * oblSpread(5)
'				SPLNDetailRow("Month6") = PGMDetailRow("FY_1") * oblSpread(6)
'				SPLNDetailRow("Month7") = PGMDetailRow("FY_1") * oblSpread(7)
'				SPLNDetailRow("Month8") = PGMDetailRow("FY_1") * oblSpread(8)
'				SPLNDetailRow("Month9") = PGMDetailRow("FY_1") * oblSpread(9)
'				SPLNDetailRow("Month10") = PGMDetailRow("FY_1") * oblSpread(10)
'				SPLNDetailRow("Month11") = PGMDetailRow("FY_1") * oblSpread(11)
'				SPLNDetailRow("Month12") = PGMDetailRow("FY_1") * oblSpread(12)
'			'End If
			
'			SPLNDetailRow("Quarter1") = PGMDetailRow()
'			SPLNDetailRow("Quarter2") = PGMDetailRow()
'			SPLNDetailRow("Quarter3") = PGMDetailRow()
'			SPLNDetailRow("Quarter4") = PGMDetailRow()
'			SPLNDetailRow("Yearly") = PGMDetailRow()
			SPLNDetailRow("AllowUpdate") = PGMDetailRow("AllowUpdate")
'			SPLNDetailRow("Create_Date") = PGMDetailRow()
'			SPLNDetailRow("Create_User") = PGMDetailRow()
'			SPLNDetailRow("Update_Date") = PGMDetailRow()
'			SPLNDetailRow("Update_User") = PGMDetailRow()

			UpdateAuditColumns(SPLNDetailRow)
		End Sub
#Region "Get Existing Row"
		
	Function GetExistingDupREQs(ByRef REQsToRollover As List(Of String)) As List(Of String)

		Dim sqa As New SqlDataAdapter()
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim scName As String = wfInfoDetails("ScenarioName")
		Dim cmd As String = wfInfoDetails("CMDName")
		Dim tm As String = wfInfoDetails("TimeName")
		
		Dim SPLNREQsDT As  New DataTable()
		Dim existingREQs As New List(Of String)

       Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                sqlConn.Open()
                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
                Dim SqlREQ As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

				Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SPLNREQsDT, SqlREQ, sqlparamsREQ)

			End Using
		End Using				

		For Each row In SPLNREQsDT.Rows
			Dim REQinDB As String = row("REQ_ID").ToString
			If REQsToRollover.Contains(REQinDB) Then
				existingREQs.Add(REQinDB)
			End If
		Next
		Return existingREQs
	End Function
	
#End Region

		Public Function GetMonthlySpread(ByRef UD1 As String, ByRef acct As String ,ByRef spread As Dictionary(Of Integer, Decimal)) As Decimal
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim cubeName As String = wfInfoDetails("CMDName")
			Dim entity As String = GetCmdFundCenterFromCube()
			Dim scenario As String = "RMW_Cycle_Config_Monthly"
			Dim tm As String = wfInfoDetails("TimeName")
			Dim month As Integer = 1
			Dim spreadmbrScript = $"Cb#{cubeName}:E#{entity}:C#Local:S#{scenario}:T#{tm}M{month}:V#Periodic:A#{acct}t:F#None:O#AdjInput:I#None:U1#{UD1}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			For i As Integer = 1 To 12
				Dim spreadPct As Decimal = 0

				spreadPct = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, spreadmbrScript).DataCellEx.DataCell.CellAmount
				spreadPct = spreadPct/100
				spread.Add(i,spreadPct)
				
			Next 
			Return Nothing
			
		End Function
		

		Public Function getGUID(ByRef REQ As DataTable) As String
			
			Dim GUIDs As String = "'"
			For Each r As DataRow In REQ.Rows
				GUIDs = GUIDs & r("CMD_PGM_REQ_ID").ToString & "',"
			Next
			GUIDs = GUIDs.Substring(0,GUIDs.Length-1)
			
			Return GUIDs

		End Function
		
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
		
	End Class
End Namespace
