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
			'if no REQs to roll over simply end
			If String.IsNullOrWhiteSpace(CMD_PGM_REQ_IDs) Then
				Return Nothing
			End If
						
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

					Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
	                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
					
					'Prepare Detail	
					Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
					Dim SqlREQDetail As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ_Details
										WHERE
										--WFScenario_Name = '{prevscName}'
										--And WFCMD_Name = '{cmd}'
										--AND WFTime_Name = '{prevtm}'
										 --Account = 'REQ_Requested_Amt'
					 					 CMD_PGM_REQ_ID IN ({CMD_PGM_REQ_IDs})"
		
					Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
	                sqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
					
					'create the new record
					Dim newREQRows As New List(Of DataRow)
					Dim newREQDetailRows As New List(Of DataRow)
					For Each row As DataRow In REQDT.Rows
						Dim newREQRow As datarow = CMD_PGM_Utilities.GetCopiedRow(si, row)

						Me.UpdateREQColumns(newREQRow)

						CMD_PGM_Utilities.UpdateAuditColumns(si, newREQRow)
						
						newREQRows.Add(newREQRow)
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
					
					'Call delete to delte rows
					Dim existingREQs As New List(Of String)
					existingREQs = Me.GetExistingDupREQs(REQ_IDs)
					
					If existingREQs.Count > 0 Then
						Dim deleter As New CMD_PGM_Helper.MainClass
						Args.NameValuePairs.Add("req_IDs", String.Join(",", existingREQs))
						Args.NameValuePairs.Add("Action", "Delete")
						deleter.main(si, globals, api, args)
					End If
					
'BRApi.ErrorLog.LogMessage(si, "rollover update: 3")					
					sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
					sqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)
'BRApi.ErrorLog.LogMessage(si, "rollover update: 4")					
					'Load to the cube
					Dim loader As New CMD_PGM_Helper.MainClass
					Args.NameValuePairs("req_IDs") =  String.Join(",", REQ_IDs)
					Args.NameValuePairs("Action") = "Insert"
					Args.NameValuePairs.Add("new_Status", "Formulate") 
					loader.main(si, globals, api, args)	
'BRApi.ErrorLog.LogMessage(si, "rollover update: 5")						
				End Using
			End Using

		End Function

#Region "GetNextREQID"

	Dim startingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
	Function GetNextREQID (fundCenter As String) As String
		Dim currentREQID As Integer
		Dim newREQ_ID As String
		If startingREQ_IDByFC.TryGetValue(fundCenter, currentREQID) Then
'BRApi.ErrorLog.LogMessage(si,"IF Fund Center: " & fundCenter & ", currentREQID: " & currentREQID )			
			currentREQID = currentREQID + 1
			Dim modifiedFC As String = fundCenter
			modifiedFC = modifiedFC.Replace("_General", "")
			If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
			newREQ_ID =  modifiedFC &"_" & currentREQID.ToString("D5")
			startingREQ_IDByFC(fundCenter) = currentREQID
		Else	
			newREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,fundCenter)
'BRApi.ErrorLog.LogMessage(si,"ELSE Fund Center: " & fundCenter & ", newREQ_ID: " & newREQ_ID.Split("_")(1) )				
			startingREQ_IDByFC.Add(fundCenter.Trim, newREQ_ID.Split("_")(1))
		End If 
			
		Return newREQ_ID
	End Function
#End Region	

		Public Sub UpdateREQColumns(ByRef newRow As DataRow) 
			'update the columns
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim trgtfundCenter = newRow("Entity").ToString
			newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
			newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			newRow("Entity") = trgtfundCenter
			newRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
			newRow("WFTime_Name") = wfInfoDetails("TimeName")
			
		End Sub
		
		Public Sub UpdateREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_PGM_REQ_ID As String) 
			'update the columns
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim trgtfundCenter = newRow("Entity").ToString
			
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
			
			Dim inflationRate As Decimal  = GetInflationRate(newRow("UD1"))
			newRow("FY_5") = currFY5Amt + (currFY5Amt * inflationRate)
			
		End Sub
		
#Region "Get Existing Row"
		
	Function GetExistingDupREQs(ByRef REQsToRollover As List(Of String)) As List(Of String)

		Dim sqa As New SqlDataAdapter()
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim scName As String = wfInfoDetails("ScenarioName")
		Dim cmd As String = wfInfoDetails("CMDName")
		Dim tm As String = wfInfoDetails("TimeName")
		
		Dim REQDT As  New DataTable()
		Dim existingREQs As New List(Of String)

        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                sqlConn.Open()
                Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
                Dim SqlREQ As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

				Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)

			End Using
		End Using				

		For Each row In REQDT.Rows
			Dim REQinDB As String = row("REQ_ID").ToString
			If REQsToRollover.Contains(REQinDB) Then
				existingREQs.Add(REQinDB)
			End If
		Next
		Return existingREQs
	End Function
	
#End Region

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