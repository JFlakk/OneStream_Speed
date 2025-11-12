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
	Public Class GBL_Helpers

        Public Shared Function GetSrccPROBEScen(ByVal si As SessionInfo,ByVal Scenario As String) As String
            Try
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim sCube = "Army"
' Updated 11/3/25  - Updated to allow CMD_SPLN to look at Montlhy Scenario on cprobe admin dashboard 
				Dim memberScript As String = ""
				If Scenario.XFContainsIgnoreCase("CMD_SPLN") Then
					memberScript = $"Cb#ARMY:E#ARMY:C#Local:S#{Scenario}:T#{wfInfoDetails("TimeName")}M1:V#Annotation:A#Var_Selected_Position:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Else 
					memberScript = $"Cb#ARMY:E#ARMY:C#Local:S#{Scenario}:T#{wfInfoDetails("TimeName")}:V#Annotation:A#Var_Selected_Position:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				End If 
				Dim sCprobeScen As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, memberScript).DataCellEx.DataCellAnnotation
			
                Return sCprobeScen
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		Public Shared Function GetValidationApproach(si As SessionInfo,ByVal CMD As String,ByVal Year As String) As Dictionary(Of String, String)
            Try
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd_ValidationApproaches As New Dictionary(Of String, String)
				#Region "acctList"
				Dim acctList As New List(Of String)
				acctList.Add("CMD_Val_APPN_Approach")
				acctList.Add("CMD_Val_APE_Approach")
				acctList.Add("CMD_Val_MDEP_Approach")
				acctList.Add("CMD_Val_DollarType_Approach")
				acctList.Add("CMD_Val_Pay_NonPay_Approach")
				#End Region
				#Region "CMD_Val_Config"
				Dim cmd_Validations As New Dictionary(Of String, String)
				cmd_Validations.Add("CMD_Val_APPN_Approach|1","APPN")
				cmd_Validations.Add("CMD_Val_APPN_Approach|2","Fund Code")
				cmd_Validations.Add("CMD_Val_APE_Approach|1","ABO Level")
				cmd_Validations.Add("CMD_Val_APE_Approach|2","APE9")
				cmd_Validations.Add("CMD_Val_MDEP_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_MDEP_Approach|0","No")
				cmd_Validations.Add("CMD_Val_DollarType_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_DollarType_Approach|0","No")
				cmd_Validations.Add("CMD_Val_Pay_NonPay_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_Pay_NonPay_Approach|0","No")
				#End Region
				
				For Each acct As String In acctList
					Dim memberScript As String = $"cb#{wfInfoDetails("CMDName")}:E#{CMD}:C#Local:S#RMW_Cycle_Config_Annual:T#{Year}:V#Periodic:A#{acct}:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim Val_Approach As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfInfoDetails("CMDName"), memberScript).DataCellEx.DataCell.CellAmount
					BRAPI.ErrorLog.LogMessage(si,$"{CMD} - {acct}|{Val_Approach.XFToString}")
					If cmd_Validations.XFGetValue($"{acct}|{Val_Approach.XFToString}","NA") <> "NA"
						cmd_ValidationApproaches.Add(acct,cmd_Validations.XFGetValue($"{acct}|{Val_Approach.XFToString()}","NA"))
					End If
				Next
				
                Return cmd_ValidationApproaches
			
			Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
		End Function
		
        Public Shared Function GetWFInfoDetails(si As SessionInfo) As Dictionary(Of String, String)
            Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
            'Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetCubeRootInfo(si, wfUnitInfo.wfunitPk.ProfileKey,True)
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
 

            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
            'WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeRootProfile.CubeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
            Return WFInfoDetails
        End Function
		
		Public Shared Function Is_Step_Allowed(ByRef si As SessionInfo, ByRef args As DashboardExtenderArgs, ByVal wfStep As String, ByVal sFundCenter As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)

			Dim sUFRPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#REQ_Allow_Approval:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sUFRPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sUFRPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If
		
		End Function
		
		Public Shared Function Check_WF_Complete_Lock(ByRef si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult						
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
			'--------------------------------------------------------------- 
			'Verify Workflow is NOT Complete Or Locked
			'---------------------------------------------------------------
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Notice: No updates are allowed. Workflow was marked ""Complete"" by Command HQ."
			End If
			Return selectionChangedTaskResult
		Return Nothing
		End Function			
		
		Public Shared Function GetEntityLevel(ByRef si As SessionInfo, sEntity As String) As String
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
	
			Dim level As String  = String.Empty
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
				level = entityText3.Substring(entityText3.Length -2, 2)
			End If
			
			Return level
			
		End Function	
	
		Public Shared Function GetDistinctValues(ByVal sourceTable As DataTable, ByVal columnName As String) As List(Of Object)
		    
		    If Not sourceTable.Columns.Contains(columnName) Then
		        Throw New ArgumentException($"Column '{columnName}' not found in the DataTable.")
		    End If
		
		    ' Use LINQ to select the column values and then filter for distinct results.
		    Dim distinctValues As List(Of Object) = sourceTable.AsEnumerable() _
		        .Select(Function(row) row.Field(Of Object)(columnName)) _
		        .Distinct() _
		        .ToList()
		        
		    Return distinctValues
		    
		End Function

'Public Function Send_Status_Change_Email(Optional ByVal REQ As String =  "", Optional ByVal Entity As String =  "")
'		'Usage: {UFR_SolutionHelper}{SendStkhldrEmail}{FundCenter=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_UserFundCenters__Shared!|],UFR=[|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|],StakeHolderEmails=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_StakeholderEmailList__Shared!|]}
'		Try
'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			'Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("reqEntity", Entity)
'			Dim sREQid As String = args.NameValuePairs.XFGetValue("reqFlow", REQ)	
'			Dim userName As String = si.UserName

'			'Title Member Script
'			Dim REQEntityTitleMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim sREQTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityTitleMemberScript).DataCellEx.DataCellAnnotation
			
'			'Status Member Script
'			Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim sREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation
			
'			'Creator Name Member Script
'			Dim REQEntityCreatorNameMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creator_Name:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim sREQCreatorName As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityCreatorNameMemberScript).DataCellEx.DataCellAnnotation
			
'			'Creation Data Member Script
'			Dim REQEntityCreationDateMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim sREQCreationDate As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityCreationDateMemberScript).DataCellEx.DataCellAnnotation	

'			'Creation Data Member Script
'			Dim REQEmailNotificationMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Notification_Email_List:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim REQStatusEmailList As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEmailNotificationMemberScript).DataCellEx.DataCellAnnotation	
''Brapi.ErrorLog.LogMessage(si, "REQStatusEmailList: " & REQStatusEmailList)

'			'Variables to set up email functionality 
'			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
'			Dim BodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
'			Dim StatusChangeEmails  As New List(Of String) 
'			StatusChangeEmails.AddRange(REQStatusEmailList.Split(","))
'			'//Status Change Email\\
'			Dim statusChangeSubject As String = "Requirement Status Change"
'			'Build Body
'			Dim statusChangeBody As String = "A Requirement Request for Fund Center: " & sFundCenter & " with Requirement Title: "  & sREQid & " - " & sREQTitle &  " has changed status to '" & sREQStatus & "' " & vbCrLf & "Submitted by: " & userName & " - " & sREQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & BodyDisclaimerBody
'			'Send email			
'			If Not String.IsNullOrWhiteSpace(REQStatusEmailList) Then
''Brapi.ErrorLog.LogMessage(si, "hits the send: " & EmailConnectorStr)
'				BRApi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmails, statusChangeSubject, statusChangeBody, Nothing)	
'			End If
'			'Executes the "SendReviewRequestEmail" function and sends an email to the resepective CMD roles within it
'			If sREQStatus = "Ready for Financial Review" Or sREQStatus = "Ready for Validation"
'				Me.SendReviewRequestEmail(si, globals, api, sFundCenter, sREQid, sREQTitle, sREQCreatorName, sREQCreationDate)
'			End If			
'		Return Nothing
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try                       
'	End Function

'#End Region  'update here

'#Region "Send Review Request Email"
''------------------------------------------------------------------------------------------------------------
''Creator(04/08/2024): Kenny, Connor, Fronz
''
''Description: Sends a request-for-review email to the CMD role that is next in the Requirements life cycle
''
''Usage: SendReviewRequestEmail is called from the SendStatusChangeEmail function
'	   'SendReviewRequestEmail uses REQDataSet.GetAllUsers to return dt of user emails.			
'	Public Function SendReviewRequestEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal FundCenter As String, ByVal REQid As String, ByVal REQTitle As String, ByVal REQCreatorName As String, ByVal REQCreationDate As String ) As Object
'		Try
'			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity",FundCenter)
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
'			Dim bodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
'			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
'			Dim DataSetArgs As New DashboardDataSetArgs
'			Dim requestEmailSubject As String = ""
'			Dim requestEmailBody As String = ""
'			Dim validatorEmailsScript As String = ""
'			Dim validatorEmails As String = ""

'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
'			'Set args to return all user emails assigned to the resepective Fund Center & "Review Financials" security group 
'			'Create email subject and body
'			If wfProfileName = "Formulate Requirement" Then
'				DataSetArgs.NameValuePairs.XFSetValue("mode","FC_RF")
'				'requestEmailSubject = "Request for Requirement Financial Review"
'				'requestEmailBody = "A requirement has been submitted for financial review for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
'				requestEmailSubject = "Request for Requirement Validation"
'				requestEmailBody = "A requirement has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
				
'			End If
'			'Set args to return all user emails assigned to the resepective Fund Center & "Validate Requirements" security group  (Commented out for now RMW-1283) 
'			'Create email subject and body
'			If wfProfileName = "Review Financials" Then
'				validatorEmailsScript = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Validation_Email_List:F#" & REQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				validatorEmails  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, validatorEmailsScript).DataCellEx.DataCellAnnotation
'				requestEmailSubject = "Request for Requirement Validation"
'				requestEmailBody = "A requirement has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
'			End If
			
'			'Call the REQDataSet and return a datetable (dt) of users' emails
'			Dim dtReviewUserEmails  As DataTable =  BR_REQDataSet.GetAllUsers(si, globals ,api , DataSetArgs)
'			'Create new list of users' emails from datatable
'			Dim lReviewEmails As New List(Of String)
'			Dim vaReviewEmails As New List(Of String)
			
'			For Each row As DataRow In dtReviewUserEmails.Rows
'				lReviewEmails.Add(CStr(row("Value")))		
'			Next
			
'			'Build and send email
'			BRApi.Utilities.SendMail(si, EmailConnectorStr, lReviewEmails, requestEmailSubject, requestEmailBody, Nothing)	

'			If wfProfileName = "Review Financials" Then
'				vaReviewEmails = validatorEmails.Split(",").ToList()
'				BRApi.Utilities.SendMail(si, EmailConnectorStr, vaReviewEmails, requestEmailSubject, requestEmailBody, Nothing)	
'			End If
			
'			Return Nothing	
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try                       
'End Function	
		
'#End Region 'update here

'	Public Function GetEntityLevel(sEntity As String) As String
'		Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
'		Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'		Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

'		Dim level As String  = String.Empty
'		Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
'		If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
'			level = entityText3.Substring(entityText3.Length -2, 2)
'		End If
		
'		Return level
		
'	End Function

	End Class
End Namespace
