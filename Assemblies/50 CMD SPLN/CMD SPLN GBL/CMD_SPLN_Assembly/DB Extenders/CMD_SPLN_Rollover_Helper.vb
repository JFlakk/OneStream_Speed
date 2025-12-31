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
		
#Region "Main RollFwdReq"	
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
						Dim newGuid As String = newREQRow("CMD_SPLN_REQ_ID").ToString
						newREQRows.Add(newREQRow)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 0: newGuid= " & newGuid)							
						'create details
						Dim foundDetailRows As DataRow() = PGMREQDetailsDT.Select(String.Format("CMD_PGM_REQ_ID = '{0}'", row("CMD_PGM_REQ_ID").ToString))
						For Each dtRow In foundDetailRows
'BRApi.ErrorLog.LogMessage(si, "rollover update: 1")								
							Dim newOblDetailREQRow As datarow = SPLNREQDetailsDT.NewRow()
							Me.MapPGMReqDetailToSPLN(newOblDetailREQRow, dtRow, "Obligations", newGuid,obligSpread)
							UpdateAuditColumns(newOblDetailREQRow)
							newREQDetailRows.Add(newOblDetailREQRow)
							
							Dim newCmtDetailREQRow As datarow = SPLNREQDetailsDT.NewRow()
							Me.MapPGMReqDetailToSPLN(newCmtDetailREQRow, dtRow, "Commitments", newGuid, commitSpread)
							UpdateAuditColumns(newCmtDetailREQRow)
							newREQDetailRows.Add(newCmtDetailREQRow)
							
						Next
'BRApi.ErrorLog.LogMessage(si, "rollover update: 2")							
					Next
					
					'update SQL Adapter and update
					Dim REQ_IDs As New List(Of String)
					For Each row In newREQRows
						SPLNREQsDT.Rows.Add(row)
						REQ_IDs.Add(row("REQ_ID").ToString)
					Next
					
					'Add the new rows to the original dataset
					For Each row In newREQDetailRows
'BRApi.ErrorLog.LogMessage(si, "new rows: " &row("CMD_SPLN_REQ_ID").ToString)						
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
					
'BRApi.ErrorLog.LogMessage(si, "rollover update: 3")
'BRApi.ErrorLog.LogMessage(si, "SPLNREQsDT count= " & SPLNREQsDT.Rows.Count & " details= " & SPLNREQDetailsDT.Rows.Count)
					sqaSPLNReqReader.Update_XFC_CMD_SPLN_REQ(SPLNREQsDT, sqa)
					sqaSPLNReqDetailReader.Update_XFC_CMD_SPLN_REQ_Details(SPLNREQDetailsDT, sqa)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 4")					
					'Load to the cube
					Dim loader As New CMD_SPLN_Helper.MainClass
					Args.NameValuePairs("req_IDs") =  String.Join(",", REQ_IDs)
					Args.NameValuePairs("Action") = "Insert"
					Args.NameValuePairs.Add("new_Status", "Formulate") '*** HARD CODE FOR TEST ***
'BRApi.ErrorLog.LogMessage(si, "rollover update: 4.5")						
					loader.main(si, globals, api, args)	
'BRApi.ErrorLog.LogMessage(si, "rollover update: 5")						
				End Using
			End Using

		End Function
#End Region

#Region "Helper Methods"

#Region "Update Audit Columns"
		Public Sub UpdateAuditColumns(ByRef newRow As DataRow) 
			
			'Update Audit columns
			newRow("Create_Date") = DateTime.Now
			newRow("Create_User") = si.UserName
			newRow("Update_Date") = DateTime.Now
			newRow("Update_User") = si.UserName
		End Sub

#End Region

#Region "Map PGM Req To SPLN"		

		Public Sub MapPGMReqToSPLN(ByRef SPLNRow As DataRow, ByRef PGMRow As DataRow) 

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
			'SPLNRow("Obj_Class") = PGMRow("Obj_Class")
			If PGMRow("Obj_Class").ToString.XFEqualsIgnoreCase("None") Then 
				SPLNRow("Obj_Class") = "None"
			Else
				SPLNRow("Obj_Class") = PGMRow("Obj_Class") & "_General"
			End If
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
			SPLNRow("FF_1") = PGMRow("FF_1")
			SPLNRow("FF_2") = PGMRow("FF_2")
			SPLNRow("FF_3") = PGMRow("FF_3")
			SPLNRow("FF_4") = PGMRow("FF_4")
			SPLNRow("FF_5") = PGMRow("FF_5")
			SPLNRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, PGMRow("Entity")) & "_Formulate_SPLN"
			SPLNRow("Review_Entity") = PGMRow("Review_Entity")
			SPLNRow("Demotion_Comment") = PGMRow("Demotion_Comment")
			SPLNRow("Related_REQs") = PGMRow("Validation_List_Emails")
			
			UpdateAuditColumns(SPLNRow)


		End Sub

#End Region

#Region "Map PGM Req Detail To SPLN"		
			
		Public Sub MapPGMReqDetailToSPLN(ByRef SPLNDetailRow As DataRow, ByRef PGMDetailRow As DataRow, ByRef acct As String ,ByRef newGuid As String, ByRef spread As Dictionary(Of Integer, Decimal)) 
'BRApi.ErrorLog.LogMessage(si, "PGM FY_1: " & PGMDetailRow("FY_1") & " , account = "	& acct)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			SPLNDetailRow("CMD_SPLN_REQ_ID") = newGuid
			SPLNDetailRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
			SPLNDetailRow("WFCMD_Name") = PGMDetailRow("WFCMD_Name")
			SPLNDetailRow("WFTime_Name") = PGMDetailRow("WFTime_Name")
			SPLNDetailRow("Unit_of_Measure") = PGMDetailRow("Unit_of_Measure")
			SPLNDetailRow("Entity") = PGMDetailRow("Entity")
			SPLNDetailRow("IC") = PGMDetailRow("IC")
			SPLNDetailRow("Account") = acct
			SPLNDetailRow("Flow") = "Formulate_SPLN"'would be updated in the update peorcess  in the helper
			SPLNDetailRow("UD1") = Me.GetFundCode(PGMDetailRow("UD1").ToString, PGMDetailRow("UD4").ToString)' "202014D26"'PGMDetailRow("UD1")
			SPLNDetailRow("UD2") = PGMDetailRow("UD2")
			SPLNDetailRow("UD3") = PGMDetailRow("UD3")
			SPLNDetailRow("UD4") = PGMDetailRow("UD4")
			SPLNDetailRow("UD5") = PGMDetailRow("UD5")
			If PGMDetailRow("UD6").ToString.XFEqualsIgnoreCase("None") Then 
				SPLNDetailRow("UD6") = "None"
			Else
				SPLNDetailRow("UD6") = PGMDetailRow("UD6") & "_General"
			End If
			SPLNDetailRow("UD7") = PGMDetailRow("UD7")
			SPLNDetailRow("UD8") = PGMDetailRow("UD8")
			SPLNDetailRow("Fiscal_Year") = PGMDetailRow("Start_Year")
			
			Dim APPN As String  = PGMDetailRow("UD1")
			If acct.XFEqualsIgnoreCase("Commitments") Then
				If spread.Count = 0 Then 
					GetMonthlySpread(APPN, "Commit_Spread_Pct", spread)
				End If
			Else 
				If acct.XFEqualsIgnoreCase("Obligations") Then
					If spread.Count = 0 Then 
						GetMonthlySpread(APPN, "Commit_Spread_Pct", spread)
					End If
				Else
					Throw New Exception("Invalid Spend Plan Account: " & acct)
				End If
			End If
			
			Dim currYearAmt As Decimal = PGMDetailRow("FY_1")
			SPLNDetailRow("Month1") = currYearAmt * spread(1)
			SPLNDetailRow("Month2") = currYearAmt * spread(2)
			SPLNDetailRow("Month3") = currYearAmt * spread(3)
			SPLNDetailRow("Month4") = currYearAmt * spread(4)
			SPLNDetailRow("Month5") = currYearAmt * spread(5)
			SPLNDetailRow("Month6") = currYearAmt * spread(6)
			SPLNDetailRow("Month7") = currYearAmt * spread(7)
			SPLNDetailRow("Month8") = currYearAmt * spread(8)
			SPLNDetailRow("Month9") = currYearAmt * spread(9)
			SPLNDetailRow("Month10") = currYearAmt * spread(10)
			SPLNDetailRow("Month11") = currYearAmt * spread(11)
			SPLNDetailRow("Month12") = currYearAmt * spread(12)

			SPLNDetailRow("AllowUpdate") = PGMDetailRow("AllowUpdate")
'BRApi.ErrorLog.LogMessage(si, "currYearAmt: " & currYearAmt & ", Month 2: Spread: " & spread(2) & ", amt: " & SPLNDetailRow("Month2").ToString & ", Month 6: Spread: " & spread(6) & ", amt: " & SPLNDetailRow("Month6").ToString  )			

			UpdateAuditColumns(SPLNDetailRow)
		End Sub

#End Region

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

#Region "Get Monthly Spread"		

		Public Function GetMonthlySpread(ByRef UD1 As String, ByRef acct As String ,ByRef spread As Dictionary(Of Integer, Decimal)) As Decimal
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim cubeName As String = wfInfoDetails("CMDName")
			Dim entity As String = GetCmdFundCenterFromCube()
			Dim scenario As String = "RMW_Cycle_Config_Monthly"
			Dim tm As String = wfInfoDetails("TimeName")
			

			For i As Integer = 1 To 12
				Dim spreadPct As Decimal = 0
				
				Dim spreadmbrScript = $"Cb#{cubeName}:E#{entity}:C#Local:S#{scenario}:T#{tm}M{i}:V#Periodic:A#{acct}:F#None:O#AdjInput:I#None:U1#{UD1}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				spreadPct = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cubeName, spreadmbrScript).DataCellEx.DataCell.CellAmount
				spreadPct = spreadPct/100
				spread.Add(i,spreadPct)
'BRApi.ErrorLog.LogMessage(si, "spreadmbrScript: "  & spreadmbrScript & ", Amount = " & spreadPct)			
				
			Next 
			
			Return Nothing
			
		End Function
		

#End Region

#Region "get GUID"		

		Public Function getGUID(ByRef REQ As DataTable) As String
			
			Dim GUIDs As String = "'"
			For Each r As DataRow In REQ.Rows
				GUIDs = GUIDs & r("CMD_PGM_REQ_ID").ToString & "',"
			Next
			GUIDs = GUIDs.Substring(0,GUIDs.Length-1)
			
			Return GUIDs

		End Function

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

#Region "Get Fund Code"
		Public Function GetFundCode(ByRef UD1 As String, ByRef UD4 As String) As String
			Dim fundCode As String = String.Empty
			Dim APPNMap As New Dictionary(Of (String, String), String)
'BRApi.ErrorLog.LogMessage(si, "UD1= " & UD1 & ", UD4= " & UD4)			
			If globals.GetObject($"AppropriationMapping") Is Nothing Then
			'If APPNMap.Count = 0 Then
				GetAPPNMapping()
				APPNMap = globals.GetObject($"AppropriationMapping")
			Else 
				APPNMap = globals.GetObject($"AppropriationMapping")
			End If
'BRApi.ErrorLog.LogMessage(si,"AppropriationMapping count = " & APPNMap.Count)	
			Dim key As (String, String) = (UD1, UD4)
			
			If APPNMap.TryGetValue(key, fundCode) Then
'BRApi.ErrorLog.LogMessage(si,"fundCode = " & fundCode)					
				'Validate member
				Dim validFCList As New List(Of String)
				If globals.GetObject($"UD1_FundCenter_MemberList") Is Nothing Then
					GetValidFundCodes()
					validFCList = globals.GetObject($"UD1_FundCenter_MemberList")
				Else
					validFCList = globals.GetObject($"UD1_FundCenter_MemberList")
				End If
'BRApi.ErrorLog.LogMessage(si,"validFCList count = " & validFCList.Count) 				
				If Not validFCList.Contains(fundCode) Then
					Dim errMessage As String = $"Invalid Fundcode {fundCode} identified for the combilation Appropriation {UD1} and Dollar Type {UD4}."
					'Throw ErrorHandler.LogWrite(si, New XFException(si, New Exception(errMessage)))
					fundCode = fundCode.Replace("27","26")
				End If
					
			Else
				Dim errMessage As String = $"No valid Fundcode for the combilation Appropriation {UD1} and Dollar Type {UD4}."
				'Throw ErrorHandler.LogWrite(si, New XFException(si, New Exception(errMessage)))
				fundCode = "201011D"
			End If
			
			Return fundCode
			
		End Function
		

		
		Public Function GetValidFundCodes()
			Dim fundCodeList As New List(Of String)
			Dim dimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_FundCode")
			Dim mbrList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, dimPk, $"U1#Top.Base" , True)
			For Each m In mbrList
				fundCodeList.Add(m.Member.Name)
			Next
'BRApi.ErrorLog.LogMessage(si,"fundCodeList = " & fundCodeList.Count)				
			globals.SetObject("UD1_FundCenter_MemberList", fundCodeList)
			
			Return Nothing
		End Function
		
		Public Function GetAPPNMapping()
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim tm As String = wfInfoDetails("TimeName")
			Dim year As String = tm.Substring(2,2)
			
			Dim APPNMap As New Dictionary(Of (String, String), String) 
			
			Dim SQL As String = $"SELECT * FROM XFC_APPN_Mapping"
			Dim dt As New DataTable
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 dt = BRApi.Database.ExecuteSql(dbConn,SQL,True)
			End Using
			
			For Each row In dt.Rows
				APPNMap.Add((row("Appropriation_CD"), row("Dollar_Type")),row("Partial_Fund_CD") & year)	
'BRApi.ErrorLog.LogMessage(si, row("Appropriation_CD").toString & " : " &  row("Dollar_Type").ToString &  " : " &  row("Partial_Fund_CD").ToString & year)				
			Next
			
			globals.SetObject("AppropriationMapping", APPNMap)
			
			Return Nothing
		End Function
		
#End Region

#End Region

	End Class
End Namespace