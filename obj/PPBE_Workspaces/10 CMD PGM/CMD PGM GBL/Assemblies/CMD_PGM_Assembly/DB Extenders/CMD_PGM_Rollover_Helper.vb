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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_PGM_Rollover_Helper
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
			Dim prevtm As Integer
			Dim prevscName As String
			If Integer.TryParse(tm, prevtm) Then
				prevtm = prevtm - 1
				prevscName = "CMD_PGM_C" & prevtm
			End If
			
			
			Dim CMD_PGM_REQ_IDs As String = args.NameValuePairs("rolloverREQs")
'BRApi.ErrorLog.LogMessage(si, "rolloverREQs: " & CMD_PGM_REQ_IDs)				
			CMD_PGM_REQ_IDs = CMD_PGM_REQ_IDs.Replace(" ","")
			If CMD_PGM_REQ_IDs.Length > 0 Then
				CMD_PGM_REQ_IDs = "'" & CMD_PGM_REQ_IDs.Replace(",","','") & "'"
			End If
	
			Dim REQDT As DataTable = New DataTable()
			Dim REQDetailDT As DataTable = New DataTable()
	       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
	            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
	                sqlConn.Open()
	                Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
	                Dim SqlREQ As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ
										WHERE 
										--WFScenario_Name = '{prevscName}'
										--And WFCMD_Name = '{cmd}'
										--AND WFTime_Name = '{prevtm}'
										 CMD_PGM_REQ_ID in ({CMD_PGM_REQ_IDs})"
'BRApi.ErrorLog.LogMessage(si, "SqlREQ: " & SqlREQ.ToString)	
					Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
	                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
					
					'Prepare Detail	
					'Dim GUIDs As String = Me.getGUID(REQDT)
					Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
					Dim SqlREQDetail As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ_Details
										WHERE
										--WFScenario_Name = '{prevscName}'
										--And WFCMD_Name = '{cmd}'
										--AND WFTime_Name = '{prevtm}'
										 --Account = 'REQ_Requested_Amt'
					 					 CMD_PGM_REQ_ID IN ({CMD_PGM_REQ_IDs})"
'BRApi.ErrorLog.LogMessage(si, "SqlREQDetail: " & SqlREQDetail.ToString)		
					Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
	                sqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
					
					'create the new record
					Dim newREQRows As New List(Of DataRow)
					Dim newREQDetailRows As New List(Of DataRow)
					For Each row As DataRow In REQDT.Rows
						Dim newREQRow As datarow = CMD_PGM_Utilities.GetCopiedRow(si, row)
						Me.UpdateREQColumns(newREQRow)
						CMD_PGM_Utilities.UpdateAuditColumns(si, row)
						
						newREQRows.Add(newREQRow)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 1 " & row("CMD_PGM_REQ_ID").ToString)							
						'create details
						Dim foundDetailRows As DataRow() = REQDetailDT.Select(String.Format("CMD_PGM_REQ_ID = '{0}'", row("CMD_PGM_REQ_ID").ToString))
						For Each dtRow In foundDetailRows
							Dim newDetailREQRow As datarow = CMD_PGM_Utilities.GetCopiedRow(si, dtRow)
							Me.UpdateREQDetailColumns(newDetailREQRow, newREQRow("CMD_PGM_REQ_ID").ToString)
							CMD_PGM_Utilities.UpdateAuditColumns(si, newDetailREQRow)
							newREQDetailRows.Add(newDetailREQRow)
						Next
'BRApi.ErrorLog.LogMessage(si, "rollover update: 2")							
					Next
					'update SQL Adapter and update
					Dim REQ_IDs As New List(Of String)
					For Each row In newREQRows
						REQDT.Rows.Add(row)
						REQ_IDs.Add(row("REQ_ID").ToString)
					Next
					For Each row In newREQDetailRows
						REQDetailDT.Rows.Add(row)
					Next
'BRApi.ErrorLog.LogMessage(si, "rollover update: 3")					
					sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
					sqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 4")					
					'Load to the cube
					Dim loader As New CMD_PGM_Helper.MainClass
					Args.NameValuePairs.Add("req_IDs", String.Join(",", REQ_IDs))
					Args.NameValuePairs.Add("new_Status", "Formulate") '*** HARD CODE FOR TEST ***
					loader.main(si, globals, api, args)	
'BRApi.ErrorLog.LogMessage(si, "rollover update: 5")						
				End Using
			End Using

		End Function

		Public Sub UpdateREQColumns(ByRef newRow As DataRow) 
			'update the columns
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim trgtfundCenter = newRow("Entity").ToString
			Dim newREQ_ID As String= GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si, trgtfundCenter)
			newRow("REQ_ID") = newREQ_ID
			newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
			newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			newRow("Entity") = trgtfundCenter
			newRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
			newRow("WFTime_Name") = wfInfoDetails("TimeName")
			
		End Sub
		
		Public Sub UpdateREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_PGM_REQ_ID As String) 
			'update the columns
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim trgtfundCenter = newRow("Entity").ToString' BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
'BRApi.ErrorLog.LogMessage(si, "trgtfundCenter: " & trgtfundCenter)				
			newRow("CMD_PGM_REQ_ID") = newCMD_PGM_REQ_ID
			newRow("Flow") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			newRow("Entity") = trgtfundCenter
			newRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
			newRow("WFTime_Name") = wfInfoDetails("TimeName")
			
			newRow("FY_1") = newRow("FY_2").ToString
			newRow("FY_2") = newRow("FY_3").ToString
			newRow("FY_3") = newRow("FY_4").ToString
			newRow("FY_4") = newRow("FY_5").ToString
			
			Dim currFY5Amt As Decimal = Convert.ToDecimal(newRow("FY_5").ToString)
			newRow("FY_5") = currFY5Amt + (currFY5Amt * .03)
			
		End Sub
		
		Public Function getGUID(ByRef REQ As DataTable) As String
			
			Dim GUIDs As String = "'"
			For Each r As DataRow In REQ.Rows
				GUIDs = GUIDs & r("CMD_PGM_REQ_ID").ToString & "',"
			Next
			GUIDs = GUIDs.Substring(0,GUIDs.Length-1)
'BRApi.ErrorLog.LogMessage(si, "Guids: " & GUIDs)			
			Return GUIDs

		End Function
		
	End Class
End Namespace
