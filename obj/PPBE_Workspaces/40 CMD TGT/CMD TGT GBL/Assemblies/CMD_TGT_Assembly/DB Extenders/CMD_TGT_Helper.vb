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
Imports System.Text.RegularExpressions

'TGTDST_SolutionHelper

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_TGT_Helper
	Public Class MainClass
		
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
            ' Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args
			
			Try
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			
#Region "SaveLockStatus"
						If args.FunctionName.XFEqualsIgnoreCase("SaveLockStatus") Then
'brapi.ErrorLog.LogMessage(si,"Here Savelockstatus")				
							Me.SaveLockStatus(si,args)
						End If
#End Region 'Updated 09/16

#Region "EmailTargetDistributionUpdates"
						If args.FunctionName.XFEqualsIgnoreCase("EmailTargetDistributionUpdates") Then
							Return Me.EmailTargetDistributionUpdates(si, args)
						End If
#End Region 'Updated 09/16

#Region "PublishCPROBEToTarget" 
				If args.FunctionName.XFEqualsIgnoreCase("PublishCPROBEToTarget") Then
					selectionChangedTaskResult = Me.PublishCPROBEToTarget(si,globals,api,args)
					Return selectionChangedTaskResult
				End If
#End Region 'Updated 09/17
					
#Region "Validate Target"
						If args.FunctionName.XFEqualsIgnoreCase("ValidateTarget") Then
							Me.ValidateTarget(Si, globals, api,args)
						End If
#End Region	'Updated 09/17

#Region "Copy Target Build Withholds"
						If args.FunctionName.XFEqualsIgnoreCase("CopyTargetBuildWithholds") Then
							Me.CopyTargetBuildWithholds(si,globals,api,args)
							Return Nothing
						End If
#End Region

#Region "Get Fund Center Lock Status"
						If args.FunctionName.XFEqualsIgnoreCase("GetFCLockStatus") Then
							Me.GetFCLockStatus(si, args)
						End If
#End Region					
				
#Region "Get Fund Center Lock Status and Throw Message"
						If args.FunctionName.XFEqualsIgnoreCase("GetFCLockStatusAndThrowMsg") Then
							Me.GetFCLockStatusAndThrowMsg(si, args)
						End If
#End Region	

#Region "Target Build: Execute NonPay Calc"
'TARGET BUILD: Execute Calc_NonPay Data Management to calculate NonPay amounts
'USAGE: Target Build > NonPay Cost Modify > Calc NonPay Cost button
'NOTE: Work in conjunction with DST_MainCalc > Calc_NonPay method
						If args.FunctionName.XFEqualsIgnoreCase("ExecuteNonPayCalc") Then						
							Return Me.ExecuteNonPayCalc(si, args)
						End If
#End Region
								
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
#Region "Constants"
Private BR_TGTDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_TGT_DataSet.MainClass
Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass	

#End Region		

'------------------------methods----------------------------	
#Region "SaveLockStatus"
		Public Sub SaveLockStatus(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs)
		'{TGTDST_SolutionHelper}{SaveLockStatus}{}
		'{TGTDST_SolutionHelper}{SaveLockStatus}{subCommandEntity=|!prompt_cbx_TGTDST_TGTADM_0CaAa_D_FundCenter__ManageAccessSubCMD!|}
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenario = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTime = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfile As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim entity As String = ""
			
			' Set entity source based on command/subcommand workflow
			If wfProfile.XFContainsIgnoreCase("Target Distribution.Administrative") Then
				entity = args.NameValuePairs.XFGetValue("subCommandEntity")
			Else If wfProfile.XFContainsIgnoreCase("Target Distribution CMD.Administrative CMD") Then
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				entity = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk , $"E#{wfCube}.Descendants.Where(HasChildren = True).Where(Text3 Contains EntityLevel=L2)", True,,)(0).Member.Name
			Else
				Throw New Exception("Invalid path -- please contact support.")
			End If
			
			If String.IsNullOrWhiteSpace(entity) Then
				Throw New Exception("Please select a funds center.")
			End If
'Brapi.ErrorLog.LogMessage(si, "Entity" & Entity)
			' Get children for loop
			Dim dimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
			Dim memberId As Integer = BRApi.Finance.Members.GetMemberId(si, Dimtype.Entity.Id, entity)
			Dim children As List(Of Member) = BRApi.Finance.Members.GetChildren(si, dimPk, memberId)
			
			' Declare loop variables
			Dim entityMemberName As String = ""
			Dim annotationScript As String = ""
			Dim annotation As String = ""
			Dim lockScript As String = ""
			Dim locked As Decimal = 1
			Dim unlocked As Decimal = 0
			
			' Loop through children (that have parents)
			For Each entityMember In children
				' Remove non-base funds centers
				If Not BRApi.Finance.Members.HasChildren(si, dimPk, entityMember.memberId) Then
'Brapi.ErrorLog.LogMessage(si, "Entity" & entityMember.Name)
					Continue For
				End If
				
				' Assign variables for setting lock status
				entityMemberName = entityMember.Name
				annotationScript = "E#" & entityMemberName & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				annotation = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "ARMY", annotationScript).DataCellEx.DataCellAnnotation
				lockScript = "Cb#" & wfCube & ":E#" & entityMemberName & "_General:C#USD:S#" & wfScenario & ":T#" & wfTime & "M12:V#YTD:A#TGT_Target_Distribution_Complete_Ind:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
				' Set lock status
				Dim objListofScriptsAmount As New List(Of MemberScriptandValue)
				Dim objScriptValAmount As New MemberScriptAndValue
				objScriptValAmount.CubeName = wfCube
				objScriptValAmount.Script = lockScript
				If annotation = "Yes"
					objScriptValAmount.Amount = locked
				Else
					objScriptValAmount.Amount = unlocked
					Dim sBalanceStatusScriptchild As String = "Cb#" & wfCube & ":E#"& entityMemberName & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sBalanceStatusScript As String = "Cb#" & wfCube & ":E#"& entity & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'brapi.ErrorLog.LogMessage(si, "sBalanceStatusScript= " & sBalanceStatusScript)
					Dim sBalancevalue As String = ""
					
					Dim objListofScriptsBalance As New List(Of MemberScriptandValue)
	    			Dim objScriptBalanceList As New MemberScriptAndValue
					objScriptBalanceList.CubeName = wfCube
					objScriptBalanceList.Script = sBalanceStatusScript
					objScriptBalanceList.TextValue = sBalancevalue
					objScriptBalanceList.IsNoData = True
					objListofScriptsBalance.Add(objScriptBalanceList)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsBalance)
					
					Dim objScriptBalancechild As New MemberScriptAndValue
					objScriptBalancechild.CubeName = wfCube
					objScriptBalancechild.Script = sBalanceStatusScriptchild
					objScriptBalancechild.TextValue = sBalancevalue
					objScriptBalancechild.IsNoData = True
					objListofScriptsBalance.Add(objScriptBalancechild)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsBalance)

				End If
				objScriptValAmount.IsNoData = False
				objListofScriptsAmount.Add(objScriptValAmount)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAmount)
			Next
		End Sub
#End Region

#Region "EmailTargetDistributionUpdates"
		Private Function EmailTargetDistributionUpdates(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			' Initialize variables
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			
			' Define workflow entity as either L2 funds center (CMD) or parameter value (subCMD)
			Dim entityName As String
			If wfProfileName.XFContainsIgnoreCase("CMD") Then
				Dim memberScript As String = "E#" & wfCube & ".Descendants.Where(Text3 Contains EntityLevel=L2).Where(HasChildren = True)"
				Dim entityMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, memberScript, True)
				Dim entityMember As MemberInfo = entityMemberList(0)
				entityName = entityMember.Member.Name
			Else
				entityName = args.NameValuePairs.XFGetValue("entity")
			End If
			
			' Add workflow entity and its children to the list of entities
			Dim entityChildren As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, "E#" & entityName & ".Children.Where(Name DoesNotContain _)", True)
			Dim entityList As New List(Of String)
			entityList.Add(entityName)
			For Each entityMember In entityChildren
				entityList.Add(entityMember.Member.Name)
			Next
		
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
			' Connection to the mail server
			Dim emailConnectionName As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,gValue, "Var_Email_Connector_String")
			
			' Email content (Subject & Body)
			Dim subject As String = "RMW -- Targets have been updated"
			Dim messageBody As String = "Your reporting organization (" & entityName & ") has updated its target distribution"
			   
			' Get list of users
			Dim toEmail As New List(Of String)
			
			Dim allUsers As List(Of UserSummaryInfo) = BRApi.Security.Admin.GetUsers(si)
			For Each tempUser As UserSummaryInfo In allUsers
				Dim targetFCString As String = GetText2TargetFCString(tempUser)
				If String.IsNullOrWhiteSpace(targetFCString) Then
					Continue For
				End If
				
				AddEmail(si, toEmail, tempUser, targetFCString, entityList)
			Next
			
			For Each item In toEmail
				BRAPI.ErrorLog.LogMessage(si, item)
			Next
			' Send the message
			BRApi.Utilities.SendMail(si, emailConnectionName, toEmail, subject, messageBody, Nothing)
		
			' Return message to user
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = False
				selectionChangedTaskResult.Message = "Email Sent."
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
			Return selectionChangedTaskResult
		End Function
		
		Public Function GetText2TargetFCString(ByVal tempUser As UserSummaryInfo) As String
			Dim text2 As String = tempUser.Text2.Replace(" ","")
			If String.IsNullOrWhiteSpace(text2) OrElse Not text2.XFContainsIgnoreCase("TGTFundCenters") Then
				Return ""
			End If
			
			Dim subTitle As String = "TGTFundCenters=["
			Dim subStart As Integer = text2.IndexOf(subTitle) + Len(subTitle)
			Dim subEnd As Integer = text2.IndexOf("]",text2.IndexOf(subTitle)) - subStart
			Dim targetFCString As String = text2.Substring(subStart, subEnd)
			
			Return targetFCString
		End Function
		
		Public Sub AddEmail(ByVal si As SessionInfo, ByVal toEmail As List(Of String), ByVal tempUser As UserSummaryInfo, ByVal targetFCString As String, ByVal entityList As List(Of String))
			For Each child In entityList
				If targetFCString.XFContainsIgnoreCase(child) Then
					toEmail.Add(BRApi.Security.Admin.GetUser(si, tempUser.Name).User.Email)
					Exit Sub
				End If
			Next
		End Sub
#End Region

#Region "PublishCPROBEToTarget"
Public Function PublishCPROBEToTarget(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
	Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
	Dim sTargetSource As String = args.NameValuePairs.XFGetValue("TargetSource")	
	Dim dargs As New DashboardExtenderArgs
	dargs.FunctionName = "Check_WF_Complete_Lock"
	Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)

	If sWFStatus.XFContainsIgnoreCase("Unlocked") Then		
		Dim gWorkSpaceId As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
		Dim dmDictionary As Dictionary(Of String, String) = New Dictionary(Of String, String)
		dmDictionary.Add("TargetSource", sTargetSource)
		brapi.Utilities.ExecuteDataMgmtSequence(si,gWorkSpaceId,"CMD_TGT_PublishCPROBEToTarget",dmDictionary)
		Return selectionChangedTaskResult
	Else
		selectionChangedTaskResult.IsOK = False
		selectionChangedTaskResult.ShowMessageBox = True
		selectionChangedTaskResult.Message = vbCRLF & "NOT allowed to publish targets while the current workflow is locked. Nagvigate to the Manage Access dashboard and select the Revert Workflows button before publishing targets." & vbCRLF	
		Return selectionChangedTaskResult
	End If
End Function

#End Region
	 	
#Region "Validate Target"	
Public Function ValidateTarget(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
''==========================================================================================================================================
'' Updated 6/2/2025 - MH & KN
'' Changed TGT validation to account for APPN-specific validation rules per Fundcenter and Fundcode
'' Ticket 1274 Sprint 23
'' Used in: btn_TGTDST_TGTVAL_0CaAa_A_ValidateTarget 
'' With: {zMH_TGTDST_SolutionHelper}{ValidateTarget}{Entity = |!prompt_cbx_TGTDST_AAAAAA_0CaAa_FundCenter__Shared!|}
''==========================================================================================================================================

			'This code runs the validation cube view and looks at the variance column.
			'If there is a non zero valu, it throws an exception

			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTimeNameMonth As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sEntity As String =  args.NameValuePairs.XFGetValue("Entity")
			Dim sEntityParent As String = sEntity.Replace("_General","")

			Dim  dic As New Dictionary(Of String, String)
			Dim  cubeViewName As String = args.NameValuePairs.XFGetValue("CVName") ' "TGT_Review_In_Out_Variance_YH"
			Dim  entityDimName As String = "E_" & wfCube 
			Dim  entityMemFilter As String = "E#" & sEntity'"A29_General"
			Dim  scenarioDimName As String = "S_Main" 
			Dim  scenarioMemFilter As String = "" 
			Dim  timeMemFilter As String = "" 
			Dim  nvbParams As New NameValueFormatBuilder("",dic,False)
			Dim  includeCellTextCols As Boolean = False 
			Dim  useStandardFactTableFields As Boolean = True
			Dim  filter As String = "" 
			Dim  parallelQueryCount As Integer = 2 
			Dim  logStatistics As Boolean = False
			
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
			Dim dt As DataTable = BRApi.Import.Data.FdxExecuteCubeView(si, gValue, cubeViewName, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, nvbParams, includeCellTextCols, useStandardFactTableFields, filter, parallelQueryCount, logStatistics)
			
			If dt Is Nothing Then
			 	Throw New Exception( vbCrLf & "No Target and Distribution to validate.")
				Return Nothing
			Else
'BRApi.ErrorLog.LogMessage(si, "Count = " & dt.Rows.Count)
				Dim ud1 As String = ""
				Dim ud3 As String = ""
				Dim variance As Decimal
				For Each r As DataRow In dt.Rows
					Dim rowContent As String = ""
					ud1 = r("UD1").ToString
					ud3 = r("UD3").ToString
					variance = Math.Round(Convert.ToDecimal(r("VVariance")),2)
					rowContent =  "Entity : " & r("Entity").ToString &  "UD1 : " & r("UD1").ToString &  "UD3 : " & r("UD3").ToString &  "Target : " & r("VControl").ToString & ", Target : " & r("VTargetLines").ToString & ", VVariance : " & r("VVariance").ToString 
'brapi.ErrorLog.LogMessage(si, rowContent)
					If variance > 0	
						brapi.ErrorLog.LogMessage(si, "Balanced 1")
				    	Throw New Exception( vbCrLf & "Target Control is not fully distributed for " & sEntityParent & " on " & ud1 & " and " & ud3 & environment.NewLine & "The Variance is " &  variance & ".")
					Else If variance < 0
						brapi.ErrorLog.LogMessage(si, "Balanced 2")
				     	Throw New Exception(vbCrLf & "Total Distributed Target is more than the Target Control amount for " & sEntityParent & " on " & ud1 & " and " & ud3 & environment.NewLine & "The Variance is " &  variance & ".")
					End If
						
				Next
			End If
'brapi.ErrorLog.LogMessage(si, "Balanced")	
			'The validation passed. Set balanced flag
			Dim sDataBufferConfirmationFlag As String =  "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeNameMonth & ":C#Local:V#Annotation:E#" & sEntity & ":A#TGT_Target_Distribution_Complete_Ind:I#None:F#None:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim objListofScriptsSetflag As New List(Of MemberScriptandValue)
			Dim objScriptValSetflag As New MemberScriptAndValue
			objScriptValSetflag.CubeName = wfCube
			objScriptValSetflag.Script = sDataBufferConfirmationFlag
			objScriptValSetflag.TextValue = "Yes"
			objScriptValSetflag.IsNoData = False
			objListofScriptsSetflag.Add(objScriptValSetflag)
			
			
			Brapi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsSetflag)
			Dim sBalanceStatusScript As String = "Cb#" & wfCube & ":E#" & sEntity & ":C#Local:S#" & wfScenarioName & ":T#" & wfTimeName & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		'brapi.ErrorLog.LogMessage(si, "sBalanceStatusScript= " & sBalanceStatusScript)
			Me.SetAnnotationValue(si, wfCube, sBalanceStatusScript, "Y")			
			Dim Statusmsg As String = "The validation For " & sEntityParent  & " has been completed. The Funds Center Is now locked. "
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "ValidationStatusMsg", "ValidationStatusMsg",Statusmsg)
			'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prompt_lbl_TGTDST_TGTVAL_0CaAa_Aa__TargetValidateStatus", Statusmsg)

			Return Nothing
			
		End Function
		#End Region
		
#Region "Get User Fund Centers By WF"
	'Get data table for FC list using Data Set
	Public Function GetUserFundCentersByWF (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As DataTable
		'Args To pass Into dataset BR
		Dim dsArgs As New DashboardDataSetArgs 
		dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
		dsArgs.DataSetName = "GetUserFundCentersByWF"
		'Get datatable for FC list
		Dim dt As DataTable = BR_TGTDataset.GetUserFundCentersByWF(si, globals, api, dsArgs)
		Return dt
	End Function
#End Region
		
#Region "Get Fund Center Lock Status"
	'Get Lock Status for passed in FC
	Public Function GetFCLockStatus (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
		Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "")	
		Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim sTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"	
		'Check TGT_Target_Distribution_Complete_Ind
		Dim indTargetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTimeM12 & ":V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		Dim indSourceValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, indTargetMbrScript).DataCellEx.DataCellAnnotation
'BRApi.ErrorLog.LogMessage(si,$"indTargetMbrScript = {indTargetMbrScript} || indSourceValue = {indSourceValue}")		
		Return indSourceValue
			
	End Function
#End Region

#Region "Get Fund Center Lock Status and Throw Message"
	''Get Lock Status for passed in FC and throw message if locked
	Public Function GetFCLockStatusAndThrowMsg (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs)
		Dim indSourceValue As String = Me.GetFCLockStatus(si, args)
		If indSourceValue.XFEqualsIgnoreCase("Yes") Then 
			Throw New Exception("This Funds Center is locked. Cannot perform action until it is unlocked.")
		Else
			Return Nothing
		End If
			
	End Function
#End Region	

#Region "Target Build: Execute NonPay Calc"
	'TARGET BUILD: Execute Data Management Sequence for NonPay Calc and display Message Box for any negative (overbudgetted) row
	'USAGE: Target Build > NonPay Cost Modify > Calc NonPay Cost button
	'NOTE: Work in conjunction with DST_MainCalc > Calc_NonPay method
	Public Function ExecuteNonPayCalc (ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs)
		Try
			Dim dataMgmtSeq As String = "Calc_NonPay"     
			Dim params As New Dictionary(Of String, String)
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
			'Execute Data Managment Sequence
			BRApi.Utilities.ExecuteDataMgmtSequence(si,gValue, dataMgmtSeq, params)
			'Get cached session variable from DST_MainCalc > Calc_NonPay for negative rows
			Dim iError As Integer = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "TargetBuild_NonPayCalc", "iError", 0)
			Dim sError As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "TargetBuild_NonPayCalc", "sError", "")
			If iError > 0 Then
				Dim lErrors As List(Of String) = sError.Split("|").ToList
				Dim sMessage As String = $"WARNING: {iError} row(s) not created due to overbudgeting in Pay/Withhold amount for the following combination(s):"
				For Each msg As String In lErrors
					sMessage = sMessage & vbCrLf & msg
				Next
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = sMessage
				
				Return selectionChangedTaskResult
				
			End If
			
			Return Nothing
		
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
			
	End Function
#End Region

#Region "Copy Target Build Withholds"
		'DEV NOTE: All code regarding CarryOver is commented out. If the requirement doesn's come back for a while, it can be removed
		Public Function CopyTargetBuildWithholds(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal Entity As String = "") As Boolean

		    Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim objDimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_ARMY")
			Dim iEntityID As Integer = BRApi.Finance.Members.GetMemberId(si,objDimPK.DimTypeId,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si,iEntityID, 1, False, False) & "_General"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim sTimeint As Integer = (sTime.XFConvertToInt +1) 
			'Dim sTimeNY As String = sTimeint & "M12"
			Dim sTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'brapi.ErrorLog.LogMessage(si,"Time" &	sTimeNY )
'brapi.ErrorLog.LogMessage(si,"Source Entity: " & sSourceEntity & "target Entiy: " & sTargetEntity & "Source REQ: " & sSourceREQ & "Target REQ: " & sTargetREQ)			
		'Check TGT_Target_Distribution_Complete_Ind
			Dim indTargetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTimeM12 & ":V#YTD:A#TGT_Target_Distribution_Complete_Ind:F#Baseline:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim indSourceValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, indTargetMbrScript).DataCellEx.DataCellAnnotation
			
			If indSourceValue.XFEqualsIgnoreCase("Yes") Then 
				Throw New Exception("This Funds Center is locked. Target Build Withholds cannot be copied until it is unlocked.")
			End If
			
'		'Check TGT_Obligation_Withhold_Spread_Prct Next Year (NY)'
'			Dim obligSpreadPrctTargetMbrScriptNY As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTimeNY & ":V#Periodic:A#TGT_Obligation_Withhold_Spread_Prct:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Dim obligSpreadPrctSourceValueNY As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, obligSpreadPrctTargetMbrScriptNY).DataCellEx.DataCell.CellAmount
'			'Check TGT_Commitment_Withhold_Spread_Prct Next Year (NY)			
'			Dim commitSpreadPrctTargetMbrScriptNY As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTimeNY & ":V#Periodic:A#TGT_Commitment_Withhold_Spread_Prct:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Dim commitSpreadPrctSourceValueNY As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, commitSpreadPrctTargetMbrScriptNY).DataCellEx.DataCell.CellAmount
'Brapi.ErrorLog.LogMessage(si, "NYspread" & commitSpreadPrctSourceValueNY )

		'Check TGT_Obligation_Withhold_Spread_Prct
			Dim obligSpreadPrctTargetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Periodic:A#TGT_Obligation_Withhold_Spread_Prct:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim obligSpreadPrctSourceValue As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, obligSpreadPrctTargetMbrScript).DataCellEx.DataCell.CellAmount
		'Check TGT_Commitment_Withhold_Spread_Prct				
			Dim commitSpreadPrctTargetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Periodic:A#TGT_Commitment_Withhold_Spread_Prct:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim commitSpreadPrctSourceValue As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, commitSpreadPrctTargetMbrScript).DataCellEx.DataCell.CellAmount

			Dim TotalcommitSpreadPrctSourceValue As Decimal = commitSpreadPrctSourceValue ' + commitSpreadPrctSourceValueNY)
			Dim TotalobligSpreadPrctSourceValue As Decimal =  obligSpreadPrctSourceValue  '+ obligSpreadPrctSourceValueNY)
			
			If TotalobligSpreadPrctSourceValue <> 100.00  And TotalcommitSpreadPrctSourceValue <> 100.00 Then 
				Throw New Exception("The Commitment or the Obligation total spread withhold rates do not equal 100%. Please update rates.")
			End If
			
		'Consolidate Target Build Wtihholds
			Dim dataMgmtSeq As String = "Copy_TargetBuild_Withholds"     
			Dim params As New Dictionary(Of String, String) 
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
			BRApi.Utilities.ExecuteDataMgmtSequence(si,gValue, dataMgmtSeq, params)	
	
		'Add extra rounding to carryover year for phased commitment and phased obligation		

			'Commitment FY|WFTime| total Phased_Commitment_Withhold
				Dim myDataUnitPkCommitFY As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTime))
			' Buffer coordinates.
			' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPkCommitFY As New DataBufferCellPk( DimConstants.All )
				myDbCellPkCommitFY.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, "Phased_Commitment_Withhold")
				myDbCellPkCommitFY.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Baseline")
				myDbCellPkCommitFY.OriginId = DimConstants.BeforeAdj
				myDbCellPkCommitFY.UD4Id = DimConstants.None
				myDbCellPkCommitFY.UD5Id = DimConstants.None
				myDbCellPkCommitFY.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "92")
				myDbCellPkCommitFY.UD7Id = DimConstants.None
				myDbCellPkCommitFY.UD8Id = DimConstants.None	

				Dim totalComitWithholdsFY As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCommitFY, dimConstants.Periodic, myDbCellPkCommitFY, True, True)	

'			'Commitment Carryover total Phased_Commitment_Withhold
'				Dim myDataUnitPkCommitCarryover As New DataUnitPk( _
'				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
'				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
'				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
'				DimConstants.Local, _
'				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
'				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTimeNY))
'			' Buffer coordinates.
'			' Default to #All for everything, then set IDs where we need it.
'				Dim myDbCellPkCommitCarryover As New DataBufferCellPk( DimConstants.All )
'				myDbCellPkCommitCarryover.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, "Total_Phased_Obligation")
'				myDbCellPkCommitCarryover.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Baseline")
'				myDbCellPkCommitCarryover.OriginId = DimConstants.BeforeAdj
'				myDbCellPkCommitCarryover.UD4Id = DimConstants.None
'				myDbCellPkCommitCarryover.UD5Id = DimConstants.None
'				myDbCellPkCommitCarryover.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "92")
'				myDbCellPkCommitCarryover.UD7Id = DimConstants.None
'				myDbCellPkCommitCarryover.UD8Id = DimConstants.None	

'				Dim totalComitWithholdsCarryover As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCommitCarryover, dimConstants.Periodic, myDbCellPkCommitCarryover, True, True)	
			
			'Looping through UD1,UD2,UD3 against the Phased_Commitment total for FY|WFYear|
				For Each fyCell As DataCell In totalComitWithholdsFY	
					Dim fyU1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, fyCell.DataCellPk.UD1Id)
					Dim fyU2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, fyCell.DataCellPk.UD2Id)
					Dim fyU3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, fyCell.DataCellPk.UD3Id)
			'Looping through UD1,UD2,UD3 against the Phased_Commitment total for FY|NextWFYear|		
'					For Each carryoverCell As DataCell In totalComitWithholdsCarryover
'						Dim U1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, carryoverCell.DataCellPk.UD1Id)
'						Dim U2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, carryoverCell.DataCellPk.UD2Id)
'						Dim U3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, carryoverCell.DataCellPk.UD3Id)
			'Comparing the respective UD dimensions of the current year and next year 
			'If they match, we add their totals togehter
'						If U1.XFEqualsIgnoreCase(fyU1) And U2.XFEqualsIgnoreCase(fyU2) And U3.XFEqualsIgnoreCase(fyU3) Then
							Dim fyPhaseWithholdTotal As Decimal = fycell.CellAmount' + carryoverCell.CellAmount
			'Grab the Target Build withhold (TBW) amount				
							Dim sTotalUndistributedMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":S#" & sScenario & ":T#" & sTimeM12 & ":A#Total_Undistributed:C#Local:V#Periodic:F#Baseline:O#BeforeAdj:I#None:" & ":U1#" & fyU1 & ":U2#" & fyU2 & ":U3#" & fyU3 & ":U4#Top:U5#Top:U6#Top:U7#Top:U8#None"	
							Dim iTotalUndistributedAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si ,sCube,sTotalUndistributedMbrScript).DataCellEx.DataCell.CellAmount
			'Calculate the variance between the TBW amount and the phase commitment withhold total			
							Dim iVariance As Decimal = iTotalUndistributedAmt - fyPhaseWithholdTotal			
			'Add and save the variance to the phase commitment at FY|WFTime|M12										
							Dim sCommitFYM12MbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":S#" & sScenario & ":T#" & sTimeM12 & ":C#Local:V#Periodic:A#Phased_Commitment_Withhold:F#Baseline:O#BeforeAdj:I#None:U1#" & fyU1  & ":U2#" & fyU2 & ":U3#" & fyU3 & ":U4#None:U5#None:U6#92:U7#None:U8#None"			
							Dim iCommitFYM12Amt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si ,sCube,sCommitFYM12MbrScript).DataCellEx.DataCell.CellAmount

							iCommitFYM12Amt += iVariance
							
							Dim objListofCommitFYM12Scripts As New List(Of MemberScriptandValue)
							
							Dim objScriptCommitFYM12 As New MemberScriptAndValue
							objScriptCommitFYM12.CubeName = sCube
							objScriptCommitFYM12.Script = sCommitFYM12MbrScript
							objScriptCommitFYM12.Amount = iCommitFYM12Amt
							objScriptCommitFYM12.IsNoData = False
							objListofCommitFYM12Scripts.Add(objScriptCommitFYM12)
			
							BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofCommitFYM12Scripts)
							
'brapi.ErrorLog.LogMessage(si, fycell.CellAmountAsText & "||" & carryoverCell.CellAmountAsText & "||" & fyCarroverTotal.ToString)
'						End If
'					Next
				Next
			'Follows the same process as the phase commitment above			
			'Obligation FY|WFTime| total Phased_Obligation_Withhold
				Dim myDataUnitPkObligFY As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTime))
			' Buffer coordinates.
			' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPkObligFY As New DataBufferCellPk( DimConstants.All )
				myDbCellPkObligFY.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, "Phased_Obligation_Withhold")
				myDbCellPkObligFY.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Baseline")
				myDbCellPkObligFY.OriginId = DimConstants.BeforeAdj
				myDbCellPkObligFY.UD4Id = DimConstants.None
				myDbCellPkObligFY.UD5Id = DimConstants.None
				myDbCellPkObligFY.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "92")
				myDbCellPkObligFY.UD7Id = DimConstants.None
				myDbCellPkObligFY.UD8Id = DimConstants.None	

				Dim totalObligWithholdsFY As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkObligFY, dimConstants.Periodic, myDbCellPkObligFY, True, True)	
				
				For Each fyCell As DataCell In totalObligWithholdsFY	
					Dim fyU1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, fyCell.DataCellPk.UD1Id)
					Dim fyU2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, fyCell.DataCellPk.UD2Id)
					Dim fyU3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, fyCell.DataCellPk.UD3Id)
					
'					For Each carryoverCell As DataCell In totalObligWithholdsCarryover
'						Dim U1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, carryoverCell.DataCellPk.UD1Id)
'						Dim U2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, carryoverCell.DataCellPk.UD2Id)
'						Dim U3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, carryoverCell.DataCellPk.UD3Id)
						
'						If U1.XFEqualsIgnoreCase(fyU1) And U2.XFEqualsIgnoreCase(fyU2) And U3.XFEqualsIgnoreCase(fyU3) Then
							Dim fyPhaseWithholdTotal As Decimal = fycell.CellAmount '+ carryoverCell.CellAmount
							
							Dim sTotalUndistributedMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":S#" & sScenario & ":T#" & sTimeM12 & ":A#Total_Undistributed:C#Local:V#Periodic:F#Baseline:O#BeforeAdj:I#None:" & ":U1#" & fyU1 & ":U2#" & fyU2 & ":U3#" & fyU3 & ":U4#Top:U5#Top:U6#Top:U7#Top:U8#None"	
							Dim iTotalUndistributedAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si ,sCube,sTotalUndistributedMbrScript).DataCellEx.DataCell.CellAmount
							
							Dim iVariance As Decimal = iTotalUndistributedAmt - fyPhaseWithholdTotal			
													
							Dim sObligFYM12MbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":S#" & sScenario & ":T#" & sTimeM12 & ":C#Local:V#Periodic:A#Phased_Obligation_Withhold:F#Baseline:O#BeforeAdj:I#None:U1#" & fyU1  & ":U2#" & fyU2 & ":U3#" & fyU3 & ":U4#None:U5#None:U6#92:U7#None:U8#None"			
							Dim iObligFYM12Amt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si ,sCube,sObligFYM12MbrScript).DataCellEx.DataCell.CellAmount

							iObligFYM12Amt += iVariance
							
							Dim objListofObligFYM12Scripts As New List(Of MemberScriptandValue)
							
							Dim objScriptObligFYM12 As New MemberScriptAndValue
							objScriptObligFYM12.CubeName = sCube
							objScriptObligFYM12.Script = sObligFYM12MbrScript
							objScriptObligFYM12.Amount = iObligFYM12Amt
							objScriptObligFYM12.IsNoData = False
							objListofObligFYM12Scripts.Add(objScriptObligFYM12)
			
							BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofObligFYM12Scripts)
							
'brapi.ErrorLog.LogMessage(si, fycell.CellAmountAsText & "||" & carryoverCell.CellAmountAsText & "||" & fyCarroverTotal.ToString)
'						End If
'					Next
				Next
				
			Return Nothing
		End Function
			  
#End Region	

#Region "SetAnnotationValue"	
		' Purpose: To set the annotation value at a given intersection
		' Usage: Called from many places for an annotation set data cell
		Public Function SetAnnotationValue(ByVal si As SessionInfo, ByVal sCube As String, ByVal memberScriptUsed As String, ByVal annotationText As String)
			
			'Create the List of MemberScriptandValue and MemberScriptandValue, then performs a SetDataCell
			Dim objListofScripts As New List(Of MemberScriptandValue)
	    	Dim objScriptMergeList As New MemberScriptAndValue
			objScriptMergeList.CubeName = sCube
			objScriptMergeList.Script = memberScriptUsed
			objScriptMergeList.TextValue = annotationText
			objScriptMergeList.IsNoData = False
			objListofScripts.Add(objScriptMergeList)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
			
			Return Nothing
			
		End Function
#End Region

'===========================================================================
#Region "Target Import File Helper"	
'----------Created to Output log files for REQ Imports----------
		Public Function TargetsImportFileHelper(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal sErrorMsg As String, ByVal sTitle As String, Optional ByVal fName As String = "") 
		
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","").Replace(" ","_")
			Dim file As New Text.StringBuilder
			
			file.Append(sErrorMsg)
			
			Dim sfileName As String = sTitle & "_" & sUser & "_" & sTimeStamp & "_" & fName
			'Pass text to bytes
			Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(file.ToString)
							
			'Define folder to hold file
			Dim sUserFolderPath As String = "Documents/Users/" & sUser
			Dim sAdminFolderPath As String = "Documents/Public/Admin/Targets/Batch_Upload_Error_Log"
			
			Dim objXFFolderExUser As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sUserFolderPath)							
			Dim objXFFileInfoUser = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sUserFolderPath, "/", sFileName))
			Dim objXFFileUser As New XFFile(objXFFileInfoUser,String.Empty,fileBytes)
			
			Dim objXFFolderExAdmin As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sAdminFolderPath)							
			Dim objXFFileInfoAdmin = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sAdminFolderPath, "/", sFileName))
			Dim objXFFileAdmin As New XFFile(objXFFileInfoAdmin,String.Empty,fileBytes)

			'Load User file
			BRApi.FileSystem.InsertOrUpdateFile(si, objXFFileUser)
			BRApi.FileSystem.InsertOrUpdateFile(si, objXFFileAdmin)
			
			Return Nothing
			
		End Function
#End Region
'===========================================================================
#Region "TGTDST_TGTIMP_LoadFile"
		'---------------------------------------------------------------------------------------------------
		' Created: 2025-Feb-20 - AK
		'
		' Purpose: 
		'
		' Invocation: MAIN() args.FunctionName.XFEqualsIgnoreCase("TGTDST_TGTIMP_LoadFile")
		'
		' Usage Instructions from a component:
		'	{TGTDST_SolutionHelper}{TGTDST_TGTIMP_LoadFile}{Caller=TGTDST_Import, FilePath = |!FilePath!|}
		'
		'	Where:
		'		FilePath= is a string of the dimensions you want returned
		'
		'
		' Usage Instructions from a rule:
		'	Set reference assemblies = BR\TGTDST_SolutionHelper
		'	add code Private TGTDST_SolutionHelper As New OneStream.BusinessRule.DashboardExtender.TGTDST_SolutionHelper.MainClass
		'	add to rule: 
		'			Dim de_args as new DashboardExtenderArgs
		'			de_args.NameValuePairs.XFSetValue("FilePath", "xxxx")
		'			TGTDST_SolutionHelper.TGTDST_TGTIMP_LoadFile(si, globals, nothing, de_args)
		'
		'
		'Modifications:
		'
		'---------------------------------------------------------------------------------------------------

		Public Function	TGTDST_TGTIMP_LoadFile(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try

'========== debug vars ===================================
Dim tStart As DateTime =  Date.Now()	
Dim sDebugRuleName As String = "TGTDST_SolutionHelper"
Dim sDebugFuncName As String = "TGTDST_TGTIMP_LoadFile"
Dim sCaller As String = args.NameValuePairs.XFGetValue("Caller")
'=========================================================

'====================================================================================================================================
#Region "init module vars"			
				Dim timeStart As DateTime = System.DateTime.Now
			
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfYear As Integer = CInt(BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey))

				'----- ACOM transformations -----
				Dim sACOM As String  = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfCube).Member.name
				If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
					sACOM = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
				Else 
					sACOM = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
				End If
				
				Dim sACOM_FundCenter As String = BRApi.Finance.Entity.Text(si, BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sACOM), 1, 0, 0)	

				
				'---get user fund centers for target distribution--			
				Dim dt As DataTable = Me.GetUserFundCentersByWF(si, globals, api, args)
				Dim oUserFundCenters As New List(Of String)
				For Each row As DataRow In dt.Rows
					oUserFundCenters.Add(row("Value").Replace("_General",""))
					Dim objMemberInfos As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & wfCube,"E#" & row("Value").Replace("_General","") & ".Children.Where(Name DoesNotContain _FC)",True)
					For Each item As MemberInfo In objMemberInfos
						oUserFundCenters.Add(item.Member.Name)
					Next				
				Next
				
				Dim saUserFundCenters() As String = oUserFundCenters.ToArray()
				
'				Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
'				Dim sUserName As String = objSignedOnUser.User.Name
'				Dim sUserDesc As String = objSignedOnUser.User.Description
'				Dim text2 As String = objSignedOnUser.User.Text2
'				Dim sUserFundCenters As String = ""
'				If ((Not String.IsNullOrWhiteSpace(text2)) And text2.XFContainsIgnoreCase("TGTFundCenters") ) Then 
'					text2 = text2.Replace(" ", "")
'					Dim subStart As Integer = text2.IndexOf("TGTFundCenters=[") + 16
'					Dim subEnd As Integer = text2.IndexOf("]",text2.IndexOf("TGTFundCenters=[")) - subStart
'					sUserFundCenters = text2.Substring(subStart, subEnd)
'				End If	
'				Dim saUserFundCenters() As String = sUserFundCenters.Split(",")
	

				'---- adjust list per workflow ----
				Dim lsUserFundCenters As New List(Of String)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				If wfProfileName.XFContainsIgnoreCase("CMD") Then
					'----- check if user has access to ACOM -----
					For Each sFC In saUserFundCenters
						If sFC.XFEqualsIgnoreCase(sACOM_FundCenter) Then lsUserFundCenters.Add(sFC)
					Next	
					If lsUserFundCenters.Count = 0 Then
						Throw New System.Exception("WARNING: User does not have access to " & sACOM_FundCenter)
					End If
					
					'----- check if ACOM is locked -----
					Dim sIndTargetMbrScript As String = "Cb#" & wfCube & ":E#" & sACOM_FundCenter & "_General:C#Local:S#" & wfScenario & ":T#" & wfYear & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim dIndValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sIndTargetMbrScript).DataCellEx.DataCellAnnotation
'brapi.ErrorLog.LogMessage(si,"sACOM_FundCenter = " & sACOM_FundCenter)
					If dIndValue = "Yes" Then
						Throw New System.Exception("WARNING: " & sACOM_FundCenter & " is locked")
					End If
					
				Else
					
					'----- check if user has access to SubCmds -----
					For Each sUserFC As String In saUserFundCenters
						If Not sUserFC.XFEqualsIgnoreCase(sACOM_FundCenter) And sUserFC.XFContainsIgnoreCase(sACOM_FundCenter) Then lsUserFundCenters.Add(sUserFC)
					Next	
					If lsUserFundCenters.Count = 0 Then
						Throw New System.Exception("WARNING: User does not have access to any sub cmds")
					End If
					
				End If
				
			
				'----- get list of sub cmds user can load to -----
				Dim objDimPk As dimpk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				Dim objMemberInfos_InputMbrs As New List(Of MemberInfo)
				For Each userfundcenter In lsUserFundCenters
					
					Dim objMemberInfos As List(Of MemberInfo) = brapi.Finance.Members.GetMembersUsingFilter(si,objDimPk, "E#" & userfundcenter & ".children.where(name doesnotcontain _General)", True)		
	
					For Each MbrInfo As MemberInfo In objMemberInfos
	
						If brapi.finance.Members.HasChildren(si,objDimPk,MbrInfo.Member.MemberId) Then

						    If brapi.finance.Members.GetMembersUsingFilter(si,objDimPk, "E#" & MbrInfo.Member.Name & "_general", True).Count = 0 Then
								Throw New System.Exception("TGTDST_TGTIMP_LoadFile(): Parent Entity member=" & MbrInfo.Member.Name & " does not hava a _General member.  Please add a _General member before continuing." )
							End If
							Dim InputMbr As MemberInfo = brapi.finance.Members.GetMembersUsingFilter(si,objDimPk, "E#" & MbrInfo.Member.Name & "_General", True)(0)
							objMemberInfos_InputMbrs.Add(InputMbr)
							
						Else
							objMemberInfos_InputMbrs.Add(MbrInfo)
						End If	
						
					Next
				Next
			

				Dim objResult As XFResult

#End Region 
'====================================================================================================================================


'====================================================================================================================================
#Region "read file"	

				'---------- Confirm source file path exists ----------
				Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
				Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
				If objXFFileInfo Is Nothing
					Me.TargetsImportFileHelper(si, globals, api, args, "File " & objXFFileInfo.Name & " does NOT exist", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(String.Concat("File " & objXFFileInfo.Name & " does NOT exist")))
				End If
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & "-" & sCaller & ":   FolderPath: " & filePath & ", Name: " & objXFFileInfo.Name)


				'---------- Confirm source file is readable ----------
				Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)
				If objXFFileEx Is Nothing
					Me.TargetsImportFileHelper(si, globals, api, args, "Unable to open file " & objXFFileInfo.Name, "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(String.Concat( "Unable to open file " & objXFFileInfo.Name)))
				End If

				'---------- read file ----------
				'DVLP NOTE: If the file is too large check here for performance issue. Maybe a file streamer to read a line at a time may be better
				Dim sFileText As String = system.Text.Encoding.ASCII.GetString(objXFFileEX.XFFile.ContentFileBytes)
				
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & "-" & sCaller & ":   Size=" & sFileText.Length)				
				
#End Region 
'====================================================================================================================================


'====================================================================================================================================
#Region "remove invalid chars"	
			
				'----------Clean up CRLF from the file ----------

				Dim patt As String = "(" & vbCrLf & "|" & vbLf & ")(?!" & sACOM_FundCenter & ")"'(?=[a-zA-Z0-9,""@!#$%'()+=-_.:<>?~^]*)" '-- Include what is in the brackt
				Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")
				
				Dim patt2 As String = "[^a-zA-Z0-9,""@!#$%'()+=_.:<>?~\-\[\]\*\^\\\/" & vbcrlf & vbLf & "]"
				cleanedText = Regex.Replace(cleanedText,patt2," ")
				
				'this handles ZWSP's that get brought in as "???" and ignored by the second pass as question marks are allowed
				Dim patt3 As String = "???"
				cleanedText = cleanedText.Replace(patt3," ")	
				
				'Alternate method not being used
	'			'Looping through Ascii Table for character 
	'				'*2-4-25 requested By Paul Burke To replace special Char. Discovered during testing In Stage, By product owners
	'				For  i As Integer = 1 To 127
	'				Select Case i
	'				Case 48 <= i <= 57 '0-9
	'					'Do Nothing
	'				Case 65 <= i <= 95 'ABC
	'					'Do Nothing
	'				Case 97 <= i <= 126 'abc
	'					'Do Nothing	
	'				Case 32,34,35,36,37,39,40,41,42,43,44,45,46,47,58
	'					'Do Nothing	
	'				Case Else	
	'					line = line.Replace(chr(i)," ")
	'				End Select


				'***********Split will need to be replace with alternate solution to handle CRLF issue*******
				Dim lines As String() = cleanedText.Split(vbCRLF) 
				
				If lines.Count < 1 Then 
					Me.TargetsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is empty", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is empty"))
				End If
				
				'For performance we are capping the upload file to not more than 5000
				If lines.Count > 5001 Then 
					Me.TargetsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows"))
				End If

#End Region 
'====================================================================================================================================
			
			
'====================================================================================================================================
#Region "clear table"	

				'----------clear ACOM from table ----------
				Dim SQLClearRows As String = "Delete from XFC_TGTDST_Import Where ACOM = '" & sACOM_FundCenter & "' and Scenario = '" & wfScenario & "' And FiscalYear = '" & wfTime & "' And UserName = '" & si.UserName & "'"
				
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si,"SQLClearRows = " & SQLClearRows)

				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearRows.ToString, False, True)
				End Using
					
#End Region 
'====================================================================================================================================

'====================================================================================================================================
#Region "loop thru lines and validate"	
	
				Dim firstLine As Boolean = True
				'Dim currentUsedFlows As List(Of String) = New List(Of String)
				Dim lsSpendPlanRecords As List (Of SpendPlanRecord) = New List (Of SpendPlanRecord)
				Dim isFileValid As Boolean = True
				Dim iLineCount As Integer = 0
				Dim membList As List(Of memberinfo)
					
				
				'Loop through each line and process
				For Each line As String In lines
					iLineCount += 1

'------------------------------------------------------------------------------
#Region "Parse file"

					'Skip empty line
					If String.IsNullOrWhiteSpace(line) Then
						Continue For
					End If

					'If there are back to back (escaped) double quotes, they will be replaced with single quotes.
					'This is done becasue "s are used as column separator in csv files and "s inside would be represented as escaped quotes ("")
					line = line.Replace("""""", "'")

'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "Line : " & line)
					'Use reg expressions to split the csv.
					'The expression accounts for commas that are with in "" to treat them as data.
					Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
					Dim fields As String () = Regex.Split(line, pattern)
					
					'Check number of column and skip first line
					If firstLine Then
						'There needs to be 9 columns
						Dim sValidNumCols As Integer = 7
						If fields.Length <> sValidNumCols Then
							Me.TargetsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is " & sValidNumCols & ", number columns in file header is " & fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
							Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is " & sValidNumCols & ", number columns in file header is "& fields.Length & vbCrLf & line ))
						End If
						
						firstLine = False
						Continue For
					End If
				
					'----- The parsed fields are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well -----
					Dim CurrRow As SpendPlanRecord = New SpendPlanRecord
					'Trim any unprintable character and surrounding quotes

					CurrRow.ACOM = fields(0).Trim().Trim("""")
					'CurrRow.Scenario is derived 
					CurrRow.FiscalYear = fields(1).Trim().Trim("""")
					CurrRow.FundCenter = fields(2).Trim().Trim("""")
					CurrRow.FundCode = fields(3).Trim().Trim("""")
					CurrRow.MDEP = fields(4).Trim().Trim("""")
					CurrRow.APE_PT = fields(5).Trim().Trim("""")
					CurrRow.CY_Amt = fields(6).Trim().Trim("""")


					If CurrRow.CY_Amt = "" Then CurrRow.CY_Amt = "0"
						
					'----- confirm WF ACOM fundcenter matches ACOM fundcenter in file -----
					If Not currRow.ACOM.XFEqualsIgnoreCase(sACOM_FundCenter) Then
						Me.TargetsImportFileHelper(si, globals, api, args, "ACOM=" & currRow.ACOM & " in the file does not match the ACOM = " & sACOM_FundCenter & " of the workflow", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception("ACOM=" & currRow.ACOM & " in the file does not match the ACOM = " & sACOM_FundCenter & " of the workflow"))					
					End If	

					'----- If fundcenter is a parent, add _General -----
					objDimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sACOM)
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currRow.FundCenter & ".base", True)
					If membList.Count > 1 Then
						currRow.FundCenter = currRow.FundCenter & "_General"
					End If
					
					'----- assign WF scenario to REQ -----
					currRow.Scenario = wfScenario
					
					'----- confirm WF year matches year in file -----
					If currRow.FiscalYear <> wfTime Then
						Me.TargetsImportFileHelper(si, globals, api, args, "Fiscal year=" & currRow.FiscalYear & " in the file does not match the fiscal year = " & wfTime & " of the workflow", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception("Fiscal year=" & currRow.FiscalYear & " in the file does not match the fiscal year = " & wfTime & " of the workflow"))					
					End If	
					
					
#Region "U3 APPN grabber"					
					'====== Get APPN_FUND And PARENT APPN_L2 ======
					Dim U1Name As String = CurrRow.FundCode					
					Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
					Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 				
				

					'====== GET BA Parent for APEPT ======
					Dim U3Name As String = CurrRow.APE_PT
					Dim U3objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
					Dim U3memSearch As String = "U3#APPN.BASE.Where(Name Contains " & U3Name & " And  Name Contains " & U1ParentName & " )"
					Dim U3membList As List(Of Memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3objDimPk ,U3memSearch ,True)

					' check if 0 for  U3membList
					If U3membList.Count = 0 Then
							Throw New XFUserMsgException(si, New Exception("Error: Invalid APE value: " & CurrRow.APE_PT & " does Not exist"))
					End If
					
					Dim U3Member As String  = U3membList.Item(0).Member.Name
					
					CurrRow.APE_PT = U3Member
						
					
'================================================================================================						
#End Region					
				
#End Region
'------------------------------------------------------------------------------

'------------------------------------------------------------------------------
#Region "Validate members"
	
					CurrRow.valid = True
					Dim sValidationError As String = ""
					
					'----- Check if ACOM is valid -----
					objDimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & CurrRow.ACOM , True)
					If (membList.Count <> 1 ) Then 
						Throw New XFUserMsgException(si, New Exception("ERROR: invalid ACOM:" & CurrRow.ACOM & " does not exist"))
					End If
					
					'----- VCheck if fiscal year is valid -----
					objDimPk = DimPk.TimeDimPk
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "T#" & CurrRow.FiscalYear, True)
					If (membList.Count <> 1 ) Then 
						Throw New XFUserMsgException(si, New Exception("ERROR: invalid Current Year:" & CurrRow.FiscalYear & " does not exist"))
					End If

					'----- VCheck if fiscal year + 1 is valid -----
					objDimPk = DimPk.TimeDimPk
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "T#" & CurrRow.FiscalYear+1, True)
					If (membList.Count <> 1 ) Then 
						Throw New XFUserMsgException(si, New Exception("ERROR: invalid Next Year:" & CurrRow.FiscalYear+1 & " does not exist"))
					End If

					'-----Check if amount is numeric-----
					If Not isnumeric(CurrRow.CY_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY Amt contains nun-numeric. Amount="& CurrRow.CY_Amt
						CurrRow.CY_Amt = 0
					End If

					
					'-----Check if amount has decimal-----
'					For Each amount In LsAmountRows
'						If amount.XFContainsIgnoreCase(".") Then
'							CurrRow.valid = False
'							If sValidationError <> "" Then sValidationError &= vbCrlf 
'							sValidationError &= "ERROR: row contains decimals.  Amount = " & Amount
'							CurrRow.CY_Amt = 0
'							CurrRow.NY_Amt = 0
'						End If
'					Next 
					If CurrRow.CY_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY Amt contains a decimal. Amount="& CurrRow.CY_Amt
						CurrRow.CY_Amt = 0
					End If
					
					
					'----- Check if Fund Center being loaded is with in the user's security list -----
					Dim sFundCenter As String = CurrRow.FundCenter
					If Not objMemberInfos_InputMbrs.Exists(Function(x) x.Member.Name = sFundCenter) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: User does not have privileges to Funds Center: " & sFundCenter.Replace("_General","")
					End If
					
					
					'----- Check if fundcode being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & CurrRow.fundCode, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid FundCode: " & CurrRow.fundCode & " does not exist"
					End If
								
					'----- Check if mdep being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & CurrRow.MDEP, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid MDEP value: " & CurrRow.MDEP & " does not exist"
					End If
					
					'----- Check if ape being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#" & CurrRow.APE_PT, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid APE value: " & CurrRow.APE_PT & " does not exist"
					End If					
						
					CurrRow.validationError = sValidationError
'If Not CurrRow.valid Then BRApi.ErrorLog.LogMessage(si, "Error line : " & currRow.StringOutput())					
						
					If Not CurrRow.valid Then isFileValid = False

					'Add REQ to the REQ list
					lsSpendPlanRecords.Add(currRow)
			
#End Region	
'------------------------------------------------------------------------------

				Next	'loop lines

#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "loop thru rows and load table"	

				Dim lsEnitiesInTable As New List(Of String)

				'loop through all parsed requirements list
				For Each CurrRow As SpendPlanRecord In lsSpendPlanRecords

					'------ capture unique list of entities in table -----	
					If Not lsEnitiesInTable.Contains(CurrRow.FundCenter) Then lsEnitiesInTable.Add(CurrRow.FundCenter)
					
'------------------------------------------------------------------------------
#Region "load table"
'brapi.ErrorLog.LogMessage(si,"before SQL statement insert")				
					'insert into table
					Dim SQLInsert As String = "
					INSERT Into [dbo].[XFC_TGTDST_Import]
								([ACOM]
								,[Scenario]
								,[FiscalYear]
								,[FundCenter]
								,[FundCode]
								,[MDEP]
								,[APE_PT]
								,[Amount]
								,[Valid]
								,[UserName]
								,[ValidationError])
				    VALUES
		   				('" & 
							CurrRow.ACOM.Replace("'", "''") & "','" &
							CurrRow.Scenario.Replace("'", "''") & "','" &
							CurrRow.FiscalYear.Replace("'", "''") & "','" &
							CurrRow.FundCenter.Replace("'", "''") & "','" &
							CurrRow.FundCode.Replace("'", "''") & "','" &
							CurrRow.MDEP.Replace("'", "''") & "','" &
							CurrRow.APE_PT.Replace("'", "''") & "','" &
							CurrRow.CY_Amt & "','" &
							CurrRow.valid & "','" &
							si.UserName & "','" &
							CurrRow.validationError.Replace("'", "''") &
						"')"  				
 'BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
								
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
					End Using
'brapi.ErrorLog.LogMessage(si,"after SQL statement insert")	
#End Region
'------------------------------------------------------------------------------

				Next

#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "If import mode = partial then check if any entities in table is locked"

			

					Dim lsListOfParents As New List(Of String)
					For Each sFC As String In lsEnitiesInTable
						Dim iEntityID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sFC.Replace("_General",""))
						Dim sParentFC As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "E_ARMY"), iEntityID, False)(0).Name
						If Not 	lsListOfParents.Contains(sParentFC) Then lsListOfParents.Add(sParentFC)						
					Next	
					
					Dim lsListOfLockedSubCmds As New List(Of String)
	
					'----- is parent locked -----
					For Each sFC As String In lsListOfParents
						
						Dim sIndTargetMbrScript As String = "Cb#" & wfCube & ":E#" & sFC & "_General:C#Local:S#" & wfScenario & ":T#" & wfYear & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim dIndValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sIndTargetMbrScript).DataCellEx.DataCellAnnotation
						If dIndValue = "Yes" Then lsListOfLockedSubCmds.Add(sFC)
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRapi.ErrorLog.LogMessage(si, "sFC=" & sFC & "   dIndValue=" & dIndValue)
					Next
					
					'----- is child locked -----
					For Each sFC As String In lsEnitiesInTable
						
						Dim sIndTargetMbrScript As String = "Cb#" & wfCube & ":E#" & sFC & ":C#Local:S#" & wfScenario & ":T#" & wfYear & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim dIndValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sIndTargetMbrScript).DataCellEx.DataCellAnnotation
						If dIndValue = "Yes" Then lsListOfLockedSubCmds.Add(sFC)
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRapi.ErrorLog.LogMessage(si, "sFC=" & sFC & "   dIndValue=" & dIndValue)
					Next
					
					If lsListOfLockedSubCmds.Count > 0 Then
						Throw New System.Exception("WARNING: following funds center(s) in the load file are locked - " & String.Join(vbCrLF ,lsListOfLockedSubCmds) )
					End If
				
#End Region
'====================================================================================================================================


				If isFileValid Then

'====================================================================================================================================
#Region "loop thru table rows and sum amounts and append comments for same key4"	

					Dim SQL_GetRows As String = "SELECT * FROM XFC_TGTDST_Import WHERE ACOM='" & sACOM_FundCenter & "' and Scenario='" & wfScenario & "' And FiscalYear='" & wfTime & "' And UserName = '" & si.UserName & "' ORDER BY FundCenter, FundCode, MDEP, APE_PT;"
'brapi.ErrorLog.LogMessage(si,"sql=" & SQL_GetRows)
					Dim dictCY_Amt As New Dictionary(Of String, Long)

					Dim dtTableRows As DataTable
		
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 			dtTableRows = BRApi.Database.ExecuteSql(dbConn,SQL_GetRows,True)
					End Using
								
						For Each drAggRow As DataRow In dtTableRows.Rows
							Dim sRow_Scenario As String = drAggRow("Scenario")
							Dim sRow_FiscalYear As String = drAggRow("FiscalYear")
							Dim sRow_Fundcenter As String = drAggRow("Fundcenter")
							Dim sRow_FundCode As String = drAggRow("FundCode")
							Dim sRow_MDEP As String = drAggRow("MDEP")
							Dim sRow_APE_PT As String = drAggRow("APE_PT")
							Dim iRow_CY_Amt As Long = drAggRow("Amount")

							Dim sRow_Account As String = ""
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "sRow_Fundcenter=" & sRow_Fundcenter)					
							If sRow_Fundcenter.XFContainsIgnoreCase("_General") Then 
								sRow_Account = "Distr_Target_General"
							
								Dim sKey As String = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M12:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, sKey & "   CY_Amt=" & iRow_CY_Amt & "   CY_Com=" & sRow_CY_Com & "   NY_Amt=" & iRow_NY_Amt  & "   NY_Com=" & sRow_NY_Com)					
						
								If dictCY_Amt.ContainsKey(sKey) Then
									dictCY_Amt(sKey) += iRow_CY_Amt 
								Else
									dictCY_Amt.Add(sKey, iRow_CY_Amt)
								End If
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "Key=" & sKey & "   CY_Amt=" & dictCY_Amt(sKey4) & "   CY_Com=" & dictCY_Com(sKey4) & "   NY_Amt=" & dictNY_Amt(sKey4) & "   NY_Com=" & dictNY_Com(sKey4) )

								sKey = sKey.Replace("Distr_Target_General", "Ctrl_Target_In")
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, sKey & "   CY_Amt=" & iRow_CY_Amt & "   CY_Com=" & sRow_CY_Com & "   NY_Amt=" & iRow_NY_Amt  & "   NY_Com=" & sRow_NY_Com)					
						
								If dictCY_Amt.ContainsKey(sKey) Then
									dictCY_Amt(sKey) += iRow_CY_Amt 
								
								Else
									dictCY_Amt.Add(sKey, iRow_CY_Amt)
								
								End If
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "Key=" & sKey & "   CY_Amt=" & dictCY_Amt(sKey4) & "   CY_Com=" & dictCY_Com(sKey4) & "   NY_Amt=" & dictNY_Amt(sKey4) & "   NY_Com=" & dictNY_Com(sKey4) )
					
							Else
								
								sRow_Account = "Distr_Target"
							
								Dim sKey As String = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M12:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, sKey & "   CY_Amt=" & iRow_CY_Amt & "   CY_Com=" & sRow_CY_Com & "   NY_Amt=" & iRow_NY_Amt  & "   NY_Com=" & sRow_NY_Com)					
						
								If dictCY_Amt.ContainsKey(sKey) Then
									dictCY_Amt(sKey) += iRow_CY_Amt 
								Else
									dictCY_Amt.Add(sKey, iRow_CY_Amt)
								End If
								
								
							End If	
							
						Next	'loop table rows

				
#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "loop thru dictionaries and load cube"	

					Dim objListofLoadAmountScripts As New List(Of MemberScriptandValue)

					For Each kvp As KeyValuePair(Of String, Long) In dictCY_Amt
						Dim sKey As String = kvp.Key
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "sKey=" & sKey)

'------------------------------------------------------------------------------
#Region "load amounts to cube"

						Dim sDataBufferPOVScript_Amt As String = "Cb#" & sACOM & ":C#USD:V#Periodic:I#None:F#Baseline:O#Forms:U4#None:U5#None:U6#None:U7#None:U8#None:" & sKey

						
						Dim sCY_Amt_Script As String = sDataBufferPOVScript_Amt
						Dim iCY_Amt As Long = dictCY_Amt(sKey)
						
						Select Case True
							Case iCY_Amt = 0 
								'----- clear data -----
							    Dim msvCY_Amt_ScriptVal As New MemberScriptAndValue
								msvCY_Amt_ScriptVal.CubeName = wfcube
								msvCY_Amt_ScriptVal.Script = sCY_Amt_Script
								msvCY_Amt_ScriptVal.Amount = 0
								msvCY_Amt_ScriptVal.IsNoData = True
								objListofLoadAmountScripts.Add(msvCY_Amt_ScriptVal)

							Case Else
								'----- load amount -----
								
							    Dim msvCY_Amt_ScriptVal As New MemberScriptAndValue
								msvCY_Amt_ScriptVal.CubeName = wfcube
								msvCY_Amt_ScriptVal.Script = sCY_Amt_Script
								msvCY_Amt_ScriptVal.Amount = iCY_Amt
								msvCY_Amt_ScriptVal.IsNoData = False
								objListofLoadAmountScripts.Add(msvCY_Amt_ScriptVal)
						End Select
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then Brapi.ErrorLog.LogMessage(si, "sCY_Amt_Script=" & sCY_Amt_Script & "   iCY_Amt=" & iCY_Amt)
					
					
#End Region
'------------------------------------------------------------------------------

					Next	'loop dictionary	
'BRApi.ErrorLog.LogMessage(si, "Updated " & objListofLoadAmountScripts.count & " cells out of " & (dictCY_Amt.Count + dictCY_Com.Count + dictNY_Amt.Count + dictNY_Com.Count).ToString)
					
					objResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofLoadAmountScripts)
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then Brapi.ErrorLog.LogMessage(si, "objResult=" & objResult.Message)


'!!!!! DVLP NOTE: this code can replaces load cube above and is faster - needs more testing before implementing !!!!!
'					'---------- run data mgmt sequence ----------
'					Dim sEntityList2 As String = ""
'					For Each MbrInfo As MemberInfo In objMemberInfos_InputMbrs
'						If sEntityList2 = "" Then
'							sEntityList2 = "E#" & MbrInfo.Member.Name 
'						Else
'							sEntityList2 &= ", E#" & MbrInfo.Member.Name 
'						End If
'					Next

'					Dim params2 As New Dictionary(Of String, String) 
'					params2.Add("EntityList", sEntityList2)
'					'params2.Add("EntityList", "E#A97AB, E#A97AC")
					
'					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
'					Dim objTAI2 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Load_Targets_To_Cube", params2)

'					'system.Threading.Thread.Sleep(500)
'					Dim objTaskActivityItem2 As TaskActivityItem = BRApi.TaskActivity.GetTaskActivityItem(si, objTAI2.UniqueID)
'					If objTaskActivityItem2.HasError Then
'						Throw New System.Exception("ERROR: Data Management Job = Load_Targets_To_Cube failed.  Please check activity log for more details.")
'					End If

#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "Consolidate"	

					'---------- run data mgmt sequence ----------
					Dim sEntityList3 As String = ""
					Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")

					For Each sUserFC As String In lsUserFundCenters
						If sEntityList3 = "" Then
							sEntityList3 = "E#" & sUserFC
						Else
							sEntityList3 &= ", E#" & sUserFC
						End If
					Next
'brapi.ErrorLog.LogMessage(si,"Entitylist = " & sEntityList3)
					
					Dim params3 As New Dictionary(Of String, String) 
					params3.Add("EntityList", sEntityList3)


					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
					Dim objTAI3 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, gValue, "ConsolidateTargets", params3)

					'system.Threading.Thread.Sleep(500)
					Dim objTaskActivityItem3 As TaskActivityItem = BRApi.TaskActivity.GetTaskActivityItem(si, objTAI3.UniqueID)
					If objTaskActivityItem3.HasError Then
						Throw New System.Exception("ERROR: Data Management Job = ConsolidateTargets failed.  Please check activity log for more details.")
					End If					
					
			
#End Region 
'====================================================================================================================================


				End If	'isfilevalid
				
'====================================================================================================================================
#Region "staus messages"	

				'If the validation failed, write the error out.
				'If there are more than ten, we show only the first ten messages for the sake of redablity
				Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
				If Not isFileValid Then
					Dim sErrorLog As String = ""
					For Each CurrRow In lsSpendPlanRecords
						sErrorLog = sErrorLog & vbCrLf & CurrRow.StringOutput()
					Next
					'Throw New XFUserMsgException(si, New Exception("LOAD FAILED" & vbCrLf & filePath & " has invalid data." & vbCrLf & vbCrLf & "Please review stage table to view errors."))
					Dim sCompletionMessageFail As String = "IMPORT FAILED" & vbCrLf _
										& "File Name: " & objXFFileInfo.Name & vbCrLf _
										& "User Name: " & si.UserName & vbCrLf _ 
										& "Time Start: " & timeStart & vbCrLf _ 
										& "Time Ended: " & System.DateTime.Now & vbCrLf _ 
										& "Total seconds for import: " & sPasstimespent.TotalSeconds & "." & vbCrLf _
										& "Number of rows processed: " & lines.count & vbCrLf _
										& vbCrLf & "File Contents:" & vbCrLf _
										& sErrorLog

					Me.TargetsImportFileHelper(si, globals, api, args, sCompletionMessageFail, "FAIL", objXFFileInfo.Name)
					
					Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), scroll right to the column titled ""ValidationError""."
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
					Return Nothing
				End If
				
				'File load complete. Write file to explorer
				Dim timespent As System.TimeSpan = Now.Subtract(timestart)
	'			Dim sCompletionMessage As String = "IMPORT PASSED" & vbCrLf _
	'												& "seconds for import to complete: " & timespent.TotalSeconds & "." & vbCrLf _
	'												& "Number of rows minus header row: " & lines.count - 1 & vbCrLf _
	'												& "Records Loaded: " & iLineCount & vbcrlf 
				Dim sCompletionMessagePass As String = "IMPORT PASSED" & vbCrLf _
													& "File Name: " & objXFFileInfo.Name & vbCrLf _
													& "User Name: " & si.UserName & vbCrLf _ 
													& "Time Start: " & timeStart & vbCrLf _ 
													& "Time Ended: " & System.DateTime.Now & vbCrLf _ 
													& "Total seconds for import: " & sPasstimespent.TotalSeconds & "." & vbCrLf _
													& "Number of rows processed: " & lines.count & vbCrLf 
													'& vbCrLf & "File Contents:" & vbCrLf _
													'& sErrorLog

				Me.TargetsImportFileHelper(si, globals, api, args, sCompletionMessagePass, "PASS", objXFFileInfo.Name)
				'Throw New Exception("IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper)
				Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
				BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
		
#End Region 
'====================================================================================================================================

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try					
		End Function	
		
#End Region
'===========================================================================
#Region "Withhold Import File Helper"	
'----------Created to Output log files for REQ Imports----------
		Public Function WithholdsImportFileHelper(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal sErrorMsg As String, ByVal sTitle As String, Optional ByVal fName As String = "") 
		
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","").Replace(" ","_")
			Dim file As New Text.StringBuilder
			
			file.Append(sErrorMsg)
			
			Dim sfileName As String = sTitle & "_" & sUser & "_" & sTimeStamp & "_" & fName
			'Pass text to bytes
			Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(file.ToString)
							
			'Define folder to hold file
			Dim sUserFolderPath As String = "Documents/Users/" & sUser
			Dim sAdminFolderPath As String = "Documents/Public/Admin/Targets/Batch_Upload_Error_Log"
			
			Dim objXFFolderExUser As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sUserFolderPath)							
			Dim objXFFileInfoUser = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sUserFolderPath, "/", sFileName))
			Dim objXFFileUser As New XFFile(objXFFileInfoUser,String.Empty,fileBytes)
			
			Dim objXFFolderExAdmin As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sAdminFolderPath)							
			Dim objXFFileInfoAdmin = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sAdminFolderPath, "/", sFileName))
			Dim objXFFileAdmin As New XFFile(objXFFileInfoAdmin,String.Empty,fileBytes)

			'Load User file
			BRApi.FileSystem.InsertOrUpdateFile(si, objXFFileUser)
			BRApi.FileSystem.InsertOrUpdateFile(si, objXFFileAdmin)
			
			Return Nothing
			
		End Function
#End Region
'===========================================================================
#Region "TGTDST_WTHTIMP_LoadFile"
		'---------------------------------------------------------------------------------------------------
		' Created: 2025-Feb-20 - AK
		'
		' Purpose: 
		'
		' Invocation: MAIN() args.FunctionName.XFEqualsIgnoreCase("TGTDST_TGTIMP_LoadFile")
		'
		' Usage Instructions from a component:
		'	{TGTDST_SolutionHelper}{TGTDST_TGTIMP_LoadFile}{Caller=TGTDST_Import, FilePath = |!FilePath!|}
		'
		'	Where:
		'		FilePath= is a string of the dimensions you want returned
		'
		'
		' Usage Instructions from a rule:
		'	Set reference assemblies = BR\TGTDST_SolutionHelper
		'	add code Private TGTDST_SolutionHelper As New OneStream.BusinessRule.DashboardExtender.TGTDST_SolutionHelper.MainClass
		'	add to rule: 
		'			Dim de_args as new DashboardExtenderArgs
		'			de_args.NameValuePairs.XFSetValue("FilePath", "xxxx")
		'			TGTDST_SolutionHelper.TGTDST_TGTIMP_LoadFile(si, globals, nothing, de_args)
		'
		'
		'Modifications:
		'
		'---------------------------------------------------------------------------------------------------

		Public Function	TGTDST_WTHIMP_LoadFile(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try

'========== debug vars ===================================
Dim tStart As DateTime =  Date.Now()	
Dim sDebugRuleName As String = "TGTDST_SolutionHelper"
Dim sDebugFuncName As String = "TGTDST_WTHIMP_LoadFile"
Dim sCaller As String = args.NameValuePairs.XFGetValue("Caller")
'=========================================================


'====================================================================================================================================
#Region "init module vars"			
				Dim timeStart As DateTime = System.DateTime.Now
			
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfYear As Integer = CInt(BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey))
				
				'----- ACOM transformations -----
				Dim sACOM As String  = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfCube).Member.name
				If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
					sACOM = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
				Else 
					sACOM = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
				End If
				Dim sACOM_FundCenter As String = BRApi.Finance.Entity.Text(si, BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sACOM), 1, 0, 0)	

				'---get user fund centers for target distribution--
				Dim dt As DataTable = Me.GetUserFundCentersByWF(si, globals, api, args)
				Dim oUserFundCenters As New List(Of String)
				For Each row As DataRow In dt.Rows
					oUserFundCenters.Add(row("Value").Replace("_General",""))
					Dim objMemberInfos As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & wfCube,"E#" & row("Value").Replace("_General","") & ".Children.Where(Name DoesNotContain _FC)",True)
					For Each item As MemberInfo In objMemberInfos
						oUserFundCenters.Add(item.Member.Name)
					Next				
				Next
				
				Dim saUserFundCenters() As String = oUserFundCenters.ToArray()
				
'				Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
'				Dim sUserName As String = objSignedOnUser.User.Name
'				Dim sUserDesc As String = objSignedOnUser.User.Description
'				Dim text2 As String = objSignedOnUser.User.Text2
'				Dim sUserFundCenters As String = ""
'				If ((Not String.IsNullOrWhiteSpace(text2)) And text2.XFContainsIgnoreCase("TGTFundCenters") ) Then 
'					text2 = text2.Replace(" ", "")
'					Dim subStart As Integer = text2.IndexOf("TGTFundCenters=[") + 16
'					Dim subEnd As Integer = text2.IndexOf("]",text2.IndexOf("TGTFundCenters=[")) - subStart
'					sUserFundCenters = text2.Substring(subStart, subEnd)
'				End If
'				Dim saUserFundCenters() As String = sUserFundCenters.Split(",")
				
				'---- adjust list per workflow ----
				Dim lsUserFundCenters As New List(Of String)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				If wfProfileName.XFContainsIgnoreCase("CMD") Then
				
					'----- check if ACOM is locked -----
					Dim sIndTargetMbrScript As String = "Cb#" & wfCube & ":E#" & sACOM_FundCenter & "_General:C#Local:S#" & wfScenario & ":T#" & wfYear & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim dIndValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sIndTargetMbrScript).DataCellEx.DataCellAnnotation
					If dIndValue = "Yes" Then
						Throw New System.Exception("WARNING: " & sACOM_FundCenter & " is locked")
					End If
					lsUserFundCenters.Add(sACOM_FundCenter)
'For Each FC As String In lsUserFundCenters
'brapi.ErrorLog.LogMessage(si, fc)
'Next
				Else
					
					'----- check if user has access to SubCmds -----
					For Each sUserFC As String In saUserFundCenters
						If Not sUserFC.XFEqualsIgnoreCase(sACOM_FundCenter) And sUserFC.XFContainsIgnoreCase(sACOM_FundCenter) Then lsUserFundCenters.Add(sUserFC)
					Next	
					If lsUserFundCenters.Count = 0 Then
						Throw New System.Exception("WARNING: User does not hava access to any sub cmds")
					End If
					
				End If
				
				
				Dim objResult As XFResult

#End Region 
'====================================================================================================================================

'====================================================================================================================================
#Region "read file"	

				'---------- Confirm source file path exists ----------
				Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
				Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
				If objXFFileInfo Is Nothing
					Me.WithholdsImportFileHelper(si, globals, api, args, "File " & objXFFileInfo.Name & " does NOT exist", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(String.Concat("File " & objXFFileInfo.Name & " does NOT exist")))
				End If
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & "-" & sCaller & ":   FolderPath: " & filePath & ", Name: " & objXFFileInfo.Name)


				'---------- Confirm source file is readable ----------
				Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)
				If objXFFileEx Is Nothing
					Me.WithholdsImportFileHelper(si, globals, api, args, "Unable to open file " & objXFFileInfo.Name, "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(String.Concat( "Unable to open file " & objXFFileInfo.Name)))
				End If

				'---------- read file ----------
				'DVLP NOTE: If the file is too large check here for performance issue. Maybe a file streamer to read a line at a time may be better
				Dim sFileText As String = system.Text.Encoding.ASCII.GetString(objXFFileEX.XFFile.ContentFileBytes)
				
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & "-" & sCaller & ":   Size=" & sFileText.Length)				
				
#End Region 
'====================================================================================================================================

'====================================================================================================================================
#Region "remove invalid chars"	
			
				'----------Clean up CRLF from the file ----------

				Dim patt As String = "(" & vbCrLf & "|" & vbLf & ")(?!" & sACOM_FundCenter & ")"'(?=[a-zA-Z0-9,""@!#$%'()+=-_.:<>?~^]*)" '-- Include what is in the brackt
				Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")
				
				Dim patt2 As String = "[^a-zA-Z0-9,""@!#$%'()+=_.:<>?~\-\[\]\*\^\\\/" & vbcrlf & vbLf & "]"
				cleanedText = Regex.Replace(cleanedText,patt2," ")
				
				'this handles ZWSP's that get brought in as "???" and ignored by the second pass as question marks are allowed
				Dim patt3 As String = "???"
				cleanedText = cleanedText.Replace(patt3," ")	
				
				'Alternate method not being used
	'			'Looping through Ascii Table for character 
	'				'*2-4-25 requested By Paul Burke To replace special Char. Discovered during testing In Stage, By product owners
	'				For  i As Integer = 1 To 127
	'				Select Case i
	'				Case 48 <= i <= 57 '0-9
	'					'Do Nothing
	'				Case 65 <= i <= 95 'ABC
	'					'Do Nothing
	'				Case 97 <= i <= 126 'abc
	'					'Do Nothing	
	'				Case 32,34,35,36,37,39,40,41,42,43,44,45,46,47,58
	'					'Do Nothing	
	'				Case Else	
	'					line = line.Replace(chr(i)," ")
	'				End Select


				'***********Split will need to be replace with alternate solution to handle CRLF issue*******
				Dim lines As String() = cleanedText.Split(vbCRLF) 
				
				If lines.Count < 1 Then 
					Me.WithholdsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is empty", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is empty"))
				End If
				
				'For performance we are capping the upload file to not more than 5000
				If lines.Count > 5001 Then 
					Me.WithholdsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows"))
				End If

#End Region 
'====================================================================================================================================
		
'====================================================================================================================================
#Region "clear table"	

				'----------clear ACOM from table ----------
				Dim SQLClearRows As String = "Delete from XFC_WTHDST_Import Where ACOM = '" & sACOM_FundCenter & "' and Scenario = '" & wfScenario & "' And FiscalYear = '" & wfTime & "' And UserName = '" & si.UserName & "'"
				
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si,"SQLClearRows = " & SQLClearRows)

				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearRows.ToString, False, True)
				End Using
					
#End Region 
'====================================================================================================================================

'====================================================================================================================================
#Region "loop thru lines and validate"	
	
				Dim firstLine As Boolean = True
				'Dim currentUsedFlows As List(Of String) = New List(Of String)
				Dim lsWithholdRecords As List (Of WithholdRecord) = New List (Of WithholdRecord)
				Dim isFileValid As Boolean = True
				Dim iLineCount As Integer = 0
				
				'Loop through each line and process
				For Each line As String In lines
					iLineCount += 1
					
'------------------------------------------------------------------------------
#Region "Parse file"

					'Skip empty line
					If String.IsNullOrWhiteSpace(line) Then
						Continue For
					End If

					'If there are back to back (escaped) double quotes, they will be replaced with single quotes.
					'This is done becasue "s are used as column separator in csv files and "s inside would be represented as escaped quotes ("")
					line = line.Replace("""""", "'")

'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "Line : " & line)
					'Use reg expressions to split the csv.
					'The expression accounts for commas that are with in "" to treat them as data.
					Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
					Dim fields As String () = Regex.Split(line, pattern)
					
					'Check number of column and skip first line
					If firstLine Then
						'There needs to be 9 columns
						Dim sValidNumCols As Integer = 30
						If fields.Length <> sValidNumCols Then
							Me.WithholdsImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is " & sValidNumCols & ", number columns in file header is " & fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
							Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is " & sValidNumCols & ", number columns in file header is "& fields.Length & vbCrLf & line ))
						End If
						
						firstLine = False
						Continue For
					End If
				
					'----- The parsed fields are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well -----
					Dim CurrRow As WithholdRecord = New WithholdRecord
					'Trim any unprintable character and surrounding quotes

					CurrRow.ACOM = fields(0).Trim().Trim("""")
					'CurrRow.Scenario is derived 
					CurrRow.FiscalYear = fields(1).Trim().Trim("""")
					CurrRow.FundCenter = fields(2).Trim().Trim("""")
					CurrRow.FundCode = fields(3).Trim().Trim("""")
					CurrRow.MDEP = fields(4).Trim().Trim("""")
					CurrRow.APE_PT = fields(5).Trim().Trim("""")
					CurrRow.OBL_CY_M1_Amt = fields(6).Trim().Trim("""")
					CurrRow.OBL_CY_M2_Amt = fields(7).Trim().Trim("""")
					CurrRow.OBL_CY_M3_Amt = fields(8).Trim().Trim("""")
					CurrRow.OBL_CY_M4_Amt = fields(9).Trim().Trim("""")
					CurrRow.OBL_CY_M5_Amt = fields(10).Trim().Trim("""")
					CurrRow.OBL_CY_M6_Amt = fields(11).Trim().Trim("""")
					CurrRow.OBL_CY_M7_Amt = fields(12).Trim().Trim("""")
					CurrRow.OBL_CY_M8_Amt = fields(13).Trim().Trim("""")
					CurrRow.OBL_CY_M9_Amt = fields(14).Trim().Trim("""")
					CurrRow.OBL_CY_M10_Amt = fields(15).Trim().Trim("""")
					CurrRow.OBL_CY_M11_Amt = fields(16).Trim().Trim("""")
					CurrRow.OBL_CY_M12_Amt = fields(17).Trim().Trim("""")
					'CurrRow.OBL_NY_M12_Amt = fields(18).Trim().Trim("""")
					CurrRow.COM_CY_M1_Amt = fields(18).Trim().Trim("""")
					CurrRow.COM_CY_M2_Amt = fields(19).Trim().Trim("""")
					CurrRow.COM_CY_M3_Amt = fields(20).Trim().Trim("""")
					CurrRow.COM_CY_M4_Amt = fields(21).Trim().Trim("""")
					CurrRow.COM_CY_M5_Amt = fields(22).Trim().Trim("""")
					CurrRow.COM_CY_M6_Amt = fields(23).Trim().Trim("""")
					CurrRow.COM_CY_M7_Amt = fields(24).Trim().Trim("""")
					CurrRow.COM_CY_M8_Amt = fields(25).Trim().Trim("""")
					CurrRow.COM_CY_M9_Amt = fields(26).Trim().Trim("""")
					CurrRow.COM_CY_M10_Amt = fields(27).Trim().Trim("""")
					CurrRow.COM_CY_M11_Amt = fields(28).Trim().Trim("""")
					CurrRow.COM_CY_M12_Amt = fields(29).Trim().Trim("""")
					'CurrRow.COM_NY_M12_Amt = fields(30).Trim().Trim("""")					

					
					If CurrRow.OBL_CY_M1_Amt = "" Then CurrRow.OBL_CY_M1_Amt = "0"
					If CurrRow.OBL_CY_M2_Amt = "" Then CurrRow.OBL_CY_M2_Amt = "0"
					If CurrRow.OBL_CY_M3_Amt = "" Then CurrRow.OBL_CY_M3_Amt = "0"
					If CurrRow.OBL_CY_M4_Amt = "" Then CurrRow.OBL_CY_M4_Amt = "0"
					If CurrRow.OBL_CY_M5_Amt = "" Then CurrRow.OBL_CY_M5_Amt = "0"
					If CurrRow.OBL_CY_M6_Amt = "" Then CurrRow.OBL_CY_M6_Amt = "0"
					If CurrRow.OBL_CY_M7_Amt = "" Then CurrRow.OBL_CY_M7_Amt = "0"
					If CurrRow.OBL_CY_M8_Amt = "" Then CurrRow.OBL_CY_M8_Amt = "0"
					If CurrRow.OBL_CY_M9_Amt = "" Then CurrRow.OBL_CY_M9_Amt = "0"
					If CurrRow.OBL_CY_M10_Amt = "" Then CurrRow.OBL_CY_M10_Amt = "0"
					If CurrRow.OBL_CY_M11_Amt = "" Then CurrRow.OBL_CY_M11_Amt = "0"
					If CurrRow.OBL_CY_M12_Amt = "" Then CurrRow.OBL_CY_M12_Amt = "0"
					If CurrRow.COM_CY_M1_Amt = "" Then CurrRow.COM_CY_M1_Amt = "0"
					If CurrRow.COM_CY_M2_Amt = "" Then CurrRow.COM_CY_M2_Amt = "0"
					If CurrRow.COM_CY_M3_Amt = "" Then CurrRow.COM_CY_M3_Amt = "0"
					If CurrRow.COM_CY_M4_Amt = "" Then CurrRow.COM_CY_M4_Amt = "0"
					If CurrRow.COM_CY_M5_Amt = "" Then CurrRow.COM_CY_M5_Amt = "0"
					If CurrRow.COM_CY_M6_Amt = "" Then CurrRow.COM_CY_M6_Amt = "0"
					If CurrRow.COM_CY_M7_Amt = "" Then CurrRow.COM_CY_M7_Amt = "0"
					If CurrRow.COM_CY_M8_Amt = "" Then CurrRow.COM_CY_M8_Amt = "0"
					If CurrRow.COM_CY_M9_Amt = "" Then CurrRow.COM_CY_M9_Amt = "0"
					If CurrRow.COM_CY_M10_Amt = "" Then CurrRow.COM_CY_M10_Amt = "0"
					If CurrRow.COM_CY_M11_Amt = "" Then CurrRow.COM_CY_M11_Amt = "0"
					If CurrRow.COM_CY_M12_Amt = "" Then CurrRow.COM_CY_M12_Amt = "0"

						
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & "-" & sCaller & ":   Record=" & CurrRow.StringOutput)				

					'----- confirm WF ACOM fundcenter matches ACOM fundcenter in file -----
					If Not currRow.ACOM.XFEqualsIgnoreCase(sACOM_FundCenter) Then
						Me.WithholdsImportFileHelper(si, globals, api, args, "ACOM=" & currRow.ACOM & " in the file does not match the ACOM = " & sACOM_FundCenter & " of the workflow", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception("ACOM=" & currRow.ACOM & " in the file does not match the ACOM = " & sACOM_FundCenter & " of the workflow"))					
					End If	
					
					'----- If fundcenter is a parent, add _General -----
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sACOM)
					Dim membList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currRow.FundCenter & ".base", True)
					If membList.Count > 1 Then
						currRow.FundCenter = currRow.FundCenter & "_General"
					End If
					
					'----- assign WF scenario to REQ -----
					currRow.Scenario = wfScenario
					
					'----- confirm WF year matches year in file -----
					If currRow.FiscalYear <> wfTime Then
						Me.WithholdsImportFileHelper(si, globals, api, args, "Fiscal year=" & currRow.FiscalYear & " in the file does not match the fiscal year = " & wfTime & " of the workflow", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception("Fiscal year=" & currRow.FiscalYear & " in the file does not match the fiscal year = " & wfTime & " of the workflow"))					
					End If
					
#Region "U3 APPN  grabber "					
					'====== Get APPN_FUND And PARENT APPN_L2 ======
					Dim U1Name As String = CurrRow.FundCode					
					Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
					Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 				
				

					'====== GET BA Parent for APEPT ======
					Dim U3Name As String = CurrRow.APE_PT
					Dim U3objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
					Dim U3memSearch As String = "U3#APPN.BASE.Where(Name Contains " & U3Name & " And  Name Contains " & U1ParentName & " )"
					Dim U3membList As List(Of Memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3objDimPk ,U3memSearch ,True)

					' check if 0 for  U3membList
					If U3membList.Count = 0 Then
							Throw New XFUserMsgException(si, New Exception("Error: Invalid APE value: " & CurrRow.APE_PT & " does Not exist"))
					End If
					
					Dim U3Member As String  = U3membList.Item(0).Member.Name	
					CurrRow.APE_PT = U3Member
					
					
'================================================================================================						
#End Region						
				
#End Region
'------------------------------------------------------------------------------

'------------------------------------------------------------------------------
#Region "Validate members"
	
					CurrRow.valid = True
					Dim sValidationError As String = ""
					
					'----- Check if ACOM is valid -----
					objDimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & CurrRow.ACOM , True)
					If (membList.Count <> 1 ) Then 
						Throw New XFUserMsgException(si, New Exception("ERROR: invalid ACOM:" & CurrRow.ACOM & " does not exist"))
					End If
					
					'----- VCheck if fiscal year is valid -----
					objDimPk = DimPk.TimeDimPk
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "T#" & CurrRow.FiscalYear, True)
					If (membList.Count <> 1 ) Then 
						Throw New XFUserMsgException(si, New Exception("ERROR: invalid Currenct Year:" & CurrRow.FiscalYear & " does not exist"))
					End If

'--------------------------------------------------------------------------
#Region "Test if OBL amount is numeric "

					
					'-----Check if amount is numeric -----
					If Not isnumeric(CurrRow.OBL_CY_M1_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M1 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M1_Amt
						CurrRow.OBL_CY_M1_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M2_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M2 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M2_Amt
						CurrRow.OBL_CY_M2_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M3_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M3 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M3_Amt
						CurrRow.OBL_CY_M3_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M4_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M4 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M4_Amt
						CurrRow.OBL_CY_M4_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M5_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M5 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M5_Amt
						CurrRow.OBL_CY_M5_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M6_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M6 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M6_Amt
						CurrRow.OBL_CY_M6_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M7_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M7 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M7_Amt
						CurrRow.OBL_CY_M7_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M8_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M8 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M8_Amt
						CurrRow.OBL_CY_M8_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M9_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M9 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M9_Amt
						CurrRow.OBL_CY_M9_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M10_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M10 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M10_Amt
						CurrRow.OBL_CY_M10_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M11_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M11 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M11_Amt
						CurrRow.OBL_CY_M11_Amt = 0
					End If
					If Not isnumeric(CurrRow.OBL_CY_M12_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M12 amount contains nun-numeric. Amount="& CurrRow.OBL_CY_M12_Amt
						CurrRow.OBL_CY_M12_Amt = 0
					End If

					
#End Region
'--------------------------------------------------------------------------

'--------------------------------------------------------------------------
#Region "Test if COM amount is numeric"
				
					
					If Not isnumeric(CurrRow.COM_CY_M1_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M1 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M1_Amt
						CurrRow.COM_CY_M1_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M2_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M2 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M2_Amt
						CurrRow.COM_CY_M2_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M3_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M3 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M3_Amt
						CurrRow.COM_CY_M3_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M4_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M4 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M4_Amt
						CurrRow.COM_CY_M4_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M5_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M5 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M5_Amt
						CurrRow.COM_CY_M5_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M6_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M6 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M6_Amt
						CurrRow.COM_CY_M6_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M7_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M7 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M7_Amt
						CurrRow.COM_CY_M7_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M8_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M8 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M8_Amt
						CurrRow.COM_CY_M8_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M9_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M9 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M9_Amt
						CurrRow.COM_CY_M9_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M10_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M10 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M10_Amt
						CurrRow.COM_CY_M10_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M11_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M11 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M11_Amt
						CurrRow.COM_CY_M11_Amt = 0
					End If
					If Not isnumeric(CurrRow.COM_CY_M12_Amt) Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M12 amount contains nun-numeric. Amount="& CurrRow.COM_CY_M12_Amt
						CurrRow.COM_CY_M12_Amt = 0
					End If
					
#End Region
'--------------------------------------------------------------------------

'--------------------------------------------------------------------------
#Region "Test if OBL amount has decimal "
										
					'-----Check if amount has decimal-----
					If CurrRow.OBL_CY_M1_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M1 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M1_Amt
						CurrRow.OBL_CY_M1_Amt = 0
					End If
					If CurrRow.OBL_CY_M2_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M2 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M2_Amt
						CurrRow.OBL_CY_M2_Amt = 0
					End If					
					If CurrRow.OBL_CY_M3_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M3 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M3_Amt
						CurrRow.OBL_CY_M3_Amt = 0
					End If
					If CurrRow.OBL_CY_M4_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M4 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M4_Amt
						CurrRow.OBL_CY_M4_Amt = 0
					End If
					If CurrRow.OBL_CY_M5_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M5 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M5_Amt
						CurrRow.OBL_CY_M5_Amt = 0
					End If
					If CurrRow.OBL_CY_M6_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M6 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M6_Amt
						CurrRow.OBL_CY_M6_Amt = 0
					End If
					If CurrRow.OBL_CY_M7_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M7 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M7_Amt
						CurrRow.OBL_CY_M7_Amt = 0
					End If
					If CurrRow.OBL_CY_M8_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M8 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M8_Amt
						CurrRow.OBL_CY_M8_Amt = 0
					End If
					If CurrRow.OBL_CY_M9_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M9 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M9_Amt
						CurrRow.OBL_CY_M9_Amt = 0
					End If
					If CurrRow.OBL_CY_M10_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M10 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M10_Amt
						CurrRow.OBL_CY_M10_Amt = 0
					End If
					If CurrRow.OBL_CY_M11_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M11 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M11_Amt
						CurrRow.OBL_CY_M11_Amt = 0
					End If
					If CurrRow.OBL_CY_M12_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY OBL M12 amount contains decimals.  Amount = " & CurrRow.OBL_CY_M12_Amt
						CurrRow.OBL_CY_M12_Amt = 0
					End If
					
#End Region
'--------------------------------------------------------------------------

'--------------------------------------------------------------------------
#Region "Test if COM amount has decimal "
					
					If CurrRow.COM_CY_M1_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M1 amount contains decimals.  Amount = " & CurrRow.COM_CY_M1_Amt
						CurrRow.COM_CY_M1_Amt = 0
					End If
					If CurrRow.COM_CY_M2_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M2 amount contains decimals.  Amount = " & CurrRow.COM_CY_M2_Amt
						CurrRow.COM_CY_M2_Amt = 0
					End If					
					If CurrRow.COM_CY_M3_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M3 amount contains decimals.  Amount = " & CurrRow.COM_CY_M3_Amt
						CurrRow.COM_CY_M3_Amt = 0
					End If
					If CurrRow.COM_CY_M4_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M4 amount contains decimals.  Amount = " & CurrRow.COM_CY_M4_Amt
						CurrRow.COM_CY_M4_Amt = 0
					End If
					If CurrRow.COM_CY_M5_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M5 amount contains decimals.  Amount = " & CurrRow.COM_CY_M5_Amt
						CurrRow.COM_CY_M5_Amt = 0
					End If
					If CurrRow.COM_CY_M6_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M6 amount contains decimals.  Amount = " & CurrRow.COM_CY_M6_Amt
						CurrRow.COM_CY_M6_Amt = 0
					End If
					If CurrRow.COM_CY_M7_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M7 amount contains decimals.  Amount = " & CurrRow.COM_CY_M7_Amt
						CurrRow.COM_CY_M7_Amt = 0
					End If
					If CurrRow.COM_CY_M8_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M8 amount contains decimals.  Amount = " & CurrRow.COM_CY_M8_Amt
						CurrRow.COM_CY_M8_Amt = 0
					End If
					If CurrRow.COM_CY_M9_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M9 amount contains decimals.  Amount = " & CurrRow.COM_CY_M9_Amt
						CurrRow.COM_CY_M9_Amt = 0
					End If
					If CurrRow.COM_CY_M10_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M10 amount contains decimals.  Amount = " & CurrRow.COM_CY_M10_Amt
						CurrRow.COM_CY_M10_Amt = 0
					End If
					If CurrRow.COM_CY_M11_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M11 amount contains decimals.  Amount = " & CurrRow.COM_CY_M11_Amt
						CurrRow.COM_CY_M11_Amt = 0
					End If
					If CurrRow.COM_CY_M12_Amt.XFContainsIgnoreCase(".") Then
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "ERROR: FY COM M12 amount contains decimals.  Amount = " & CurrRow.COM_CY_M12_Amt
						CurrRow.COM_CY_M12_Amt = 0
					End If
					
#End Region
'--------------------------------------------------------------------------

					'----- truncate error message if too big			
					If sValidationError.Length > 256 Then sValidationError = Strings.Left(sValidationError, 256) 

					
					'----- Check if Fund Center being loaded is with in the user's security list ----
					Dim sFundCenter As String = CurrRow.FundCenter
					If Not lsuserfundcenters.Contains(sFundCenter.Replace("_General","")) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: User does not have privileges to Funds Center: " & sFundCenter.Replace("_General","")
					End If 	
					
					
					'----- Check if fundcode being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & CurrRow.fundCode, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid FundCode: " & CurrRow.fundCode & " does not exist"
					End If
								
					'----- Check if mdep being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & CurrRow.MDEP, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid MDEP value: " & CurrRow.MDEP & " does not exist"
					End If
					
					'----- Check if ape being loaded is valid -----
					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#" & CurrRow.APE_PT, True)
					If (membList.Count <> 1 ) Then 
						CurrRow.valid = False
						If sValidationError <> "" Then sValidationError &= vbCrlf 
						sValidationError &= "Error: Invalid APE value: " & CurrRow.APE_PT & " does not exist"
					End If
						
					CurrRow.validationError = sValidationError
'If Not CurrRow.valid Then BRApi.ErrorLog.LogMessage(si, "Error line : " & currRow.StringOutput())					
						
					If Not CurrRow.valid Then isFileValid = False

					'Add REQ to the REQ list
					lsWithholdRecords.Add(currRow)
			
#End Region	
'------------------------------------------------------------------------------

				Next	'loop lines

#End Region
'====================================================================================================================================

'====================================================================================================================================
#Region "loop thru rows and load table"	
		
				Dim lsEnitiesInTable As New List(Of String)

				'loop through all parsed requirements list
				For Each CurrRow As WithholdRecord In lsWithholdRecords

					'------ capture unique list of entities in table -----	
					If Not lsEnitiesInTable.Contains(CurrRow.FundCenter) Then lsEnitiesInTable.Add(CurrRow.FundCenter)
'------------------------------------------------------------------------------
#Region "load table"
				
					'insert into table
					Dim SQLInsert As String = "
					INSERT Into [dbo].[XFC_WTHDST_Import]
								([ACOM]
								,[Scenario]
								,[FiscalYear]
								,[FundCenter]
								,[FundCode]
								,[MDEP]
								,[APE_PT]
								,[OBL_M1]
								,[OBL_M2]
								,[OBL_M3]
								,[OBL_M4]
								,[OBL_M5]
								,[OBL_M6]
								,[OBL_M7]
								,[OBL_M8]
								,[OBL_M9]
								,[OBL_M10]
								,[OBL_M11]
								,[OBL_M12]
								,[COM_M1]
								,[COM_M2]
								,[COM_M3]
								,[COM_M4]
								,[COM_M5]
								,[COM_M6]
								,[COM_M7]
								,[COM_M8]
								,[COM_M9]
								,[COM_M10]
								,[COM_M11]
								,[COM_M12]
								,[Valid]
								,[UserName]
								,[ValidationError])
				    VALUES
		   				('" & 
							CurrRow.ACOM.Replace("'", "''") & "','" &
							CurrRow.Scenario.Replace("'", "''") & "','" &
							CurrRow.FiscalYear.Replace("'", "''") & "','" &
							CurrRow.FundCenter.Replace("'", "''") & "','" &
							CurrRow.FundCode.Replace("'", "''") & "','" &
							CurrRow.MDEP.Replace("'", "''") & "','" &
							CurrRow.APE_PT.Replace("'", "''") & "','" &
							CurrRow.OBL_CY_M1_Amt & "','" &
							CurrRow.OBL_CY_M2_Amt & "','" &
							CurrRow.OBL_CY_M3_Amt & "','" &
							CurrRow.OBL_CY_M4_Amt & "','" &
							CurrRow.OBL_CY_M5_Amt & "','" &
							CurrRow.OBL_CY_M6_Amt & "','" &
							CurrRow.OBL_CY_M7_Amt & "','" &
							CurrRow.OBL_CY_M8_Amt & "','" &
							CurrRow.OBL_CY_M9_Amt & "','" &
							CurrRow.OBL_CY_M10_Amt & "','" &
							CurrRow.OBL_CY_M11_Amt & "','" &
							CurrRow.OBL_CY_M12_Amt & "','" &
							CurrRow.COM_CY_M1_Amt & "','" &
							CurrRow.COM_CY_M2_Amt & "','" &
							CurrRow.COM_CY_M3_Amt & "','" &
							CurrRow.COM_CY_M4_Amt & "','" &
							CurrRow.COM_CY_M5_Amt & "','" &
							CurrRow.COM_CY_M6_Amt & "','" &
							CurrRow.COM_CY_M7_Amt & "','" &
							CurrRow.COM_CY_M8_Amt & "','" &
							CurrRow.COM_CY_M9_Amt & "','" &
							CurrRow.COM_CY_M10_Amt & "','" &
							CurrRow.COM_CY_M11_Amt & "','" &
							CurrRow.COM_CY_M12_Amt & "','" &
							CurrRow.valid & "','" &
							si.UserName & "','" &
							CurrRow.validationError.Replace("'", "''") &
						"')"  				
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
								
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
					End Using

#End Region
'------------------------------------------------------------------------------

				Next

#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "If import mode = partial then check if any entities in table is locked"

				

					Dim lsListOfLockedSubCmds As New List(Of String)
	
					'----- check if FC is locked   -----
					For Each sFC As String In lsEnitiesInTable
						
						Dim sIndTargetMbrScript As String = "Cb#" & wfCube & ":E#" & sFC & ":C#Local:S#" & wfScenario & ":T#" & wfYear & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim dIndValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sIndTargetMbrScript).DataCellEx.DataCellAnnotation
						If dIndValue = "Yes" Then lsListOfLockedSubCmds.Add(sFC)
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRapi.ErrorLog.LogMessage(si, "sFC=" & sFC & "   dIndValue=" & dIndValue)
					Next
					
					If lsListOfLockedSubCmds.Count > 0 Then
						Throw New System.Exception("WARNING: following funds center(s) in the load file are locked - " & String.Join(vbCrLF ,lsListOfLockedSubCmds) )
					End If
				
#End Region
'====================================================================================================================================


				If isFileValid Then
					
'====================================================================================================================================
#Region "loop thru table rows and sum amounts and append comments for same key4"	

					Dim SQL_GetRows As String = "SELECT * FROM XFC_WTHDST_Import WHERE ACOM='" & sACOM_FundCenter & "' and Scenario='" & wfScenario & "' And FiscalYear='" & wfTime & "' And UserName = '" & si.UserName & "' ORDER BY FundCenter, FundCode, MDEP, APE_PT;"

					Dim dict_Amt As New Dictionary(Of String, Long)

						Dim dtTableRows As DataTable
			
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 			dtTableRows = BRApi.Database.ExecuteSql(dbConn,SQL_GetRows,True)
						End Using								
						
						For Each drAggRow As DataRow In dtTableRows.Rows
							Dim sRow_Scenario As String = drAggRow("Scenario")
							Dim sRow_FiscalYear As String = drAggRow("FiscalYear")
							Dim sRow_Fundcenter As String = drAggRow("Fundcenter")
							Dim sRow_FundCode As String = drAggRow("FundCode")
							Dim sRow_MDEP As String = drAggRow("MDEP")
							Dim sRow_APE_PT As String = drAggRow("APE_PT")
							Dim iRow_OBL_CY_M1_Amt As Long = drAggRow("OBL_M1")
							Dim iRow_OBL_CY_M2_Amt As Long = drAggRow("OBL_M2")
							Dim iRow_OBL_CY_M3_Amt As Long = drAggRow("OBL_M3")
							Dim iRow_OBL_CY_M4_Amt As Long = drAggRow("OBL_M4")
							Dim iRow_OBL_CY_M5_Amt As Long = drAggRow("OBL_M5")
							Dim iRow_OBL_CY_M6_Amt As Long = drAggRow("OBL_M6")
							Dim iRow_OBL_CY_M7_Amt As Long = drAggRow("OBL_M7")
							Dim iRow_OBL_CY_M8_Amt As Long = drAggRow("OBL_M8")
							Dim iRow_OBL_CY_M9_Amt As Long = drAggRow("OBL_M9")
							Dim iRow_OBL_CY_M10_Amt As Long = drAggRow("OBL_M10")
							Dim iRow_OBL_CY_M11_Amt As Long = drAggRow("OBL_M11")
							Dim iRow_OBL_CY_M12_Amt As Long = drAggRow("OBL_M12")
							Dim iRow_COM_CY_M1_Amt As Long = drAggRow("COM_M1")
							Dim iRow_COM_CY_M2_Amt As Long = drAggRow("COM_M2")
							Dim iRow_COM_CY_M3_Amt As Long = drAggRow("COM_M3")
							Dim iRow_COM_CY_M4_Amt As Long = drAggRow("COM_M4")
							Dim iRow_COM_CY_M5_Amt As Long = drAggRow("COM_M5")
							Dim iRow_COM_CY_M6_Amt As Long = drAggRow("COM_M6")
							Dim iRow_COM_CY_M7_Amt As Long = drAggRow("COM_M7")
							Dim iRow_COM_CY_M8_Amt As Long = drAggRow("COM_M8")
							Dim iRow_COM_CY_M9_Amt As Long = drAggRow("COM_M9")
							Dim iRow_COM_CY_M10_Amt As Long = drAggRow("COM_M10")
							Dim iRow_COM_CY_M11_Amt As Long = drAggRow("COM_M11")
							Dim iRow_COM_CY_M12_Amt As Long = drAggRow("COM_M12")

							Dim sRow_Account As String = ""
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then BRApi.ErrorLog.LogMessage(si, "sRow_Fundcenter=" & sRow_Fundcenter)					
							sRow_Account = "Phased_Obligation_Withhold"

'------------------------------------------------------------------------------
#Region "load obligation to dictionary"

							Dim sKey As String = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M1:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M1_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M1_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M2:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M2_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M2_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M3:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M3_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M3_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M4:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M4_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M4_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M5:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M5_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M5_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M6:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M6_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M6_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M7:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M7_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M7_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M8:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M8_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M8_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M9:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M9_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M9_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M10:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M10_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M10_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M11:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M11_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M11_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M12:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_OBL_CY_M12_Amt 
							Else
								dict_Amt.Add(sKey, iRow_OBL_CY_M12_Amt)
							End If
							
#End Region
'------------------------------------------------------------------------------

							sRow_Account = "Phased_Commitment_Withhold"

'------------------------------------------------------------------------------
#Region "load commitment to dictionary"

							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M1:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M1_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M1_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M2:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M2_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M2_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M3:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M3_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M3_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M4:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M4_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M4_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M5:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M5_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M5_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M6:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M6_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M6_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M7:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M7_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M7_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M8:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M8_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M8_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M9:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M9_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M9_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M10:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M10_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M10_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M11:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M11_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M11_Amt)
							End If
							sKey = "S#" & sRow_Scenario & ":T#" & sRow_FiscalYear & "M12:E#" & sRow_Fundcenter & ":U1#" & sRow_FundCode & ":U2#" & sRow_MDEP & ":U3#" & sRow_APE_PT & ":A#" & sRow_Account
							If dict_Amt.ContainsKey(sKey) Then
								dict_Amt(sKey) += iRow_COM_CY_M12_Amt 
							Else
								dict_Amt.Add(sKey, iRow_COM_CY_M12_Amt)
							End If
						
#End Region
'------------------------------------------------------------------------------
					
						Next	'loop table rows
									
				
#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "loop thru dictionaries and load cube"	

					Dim objListofLoadAmountScripts As New List(Of MemberScriptandValue)

					For Each kvp As KeyValuePair(Of String, Long) In dict_Amt
						Dim sKey As String = kvp.Key
'BRApi.ErrorLog.LogMessage(si, "sKey=" & sKey & "   Value=" & kvp.Value)

'------------------------------------------------------------------------------
#Region "load amounts to cube"

						'!!!!! DVLP NOTE: withholds are stored are obj class = 92 !!!!!
						Dim sDataBufferPOVScript_Amt As String = "Cb#" & sACOM & ":C#USD:V#Periodic:I#None:F#Baseline:O#Forms:U4#None:U5#None:U6#92:U7#None:U8#None:" & sKey

						
						Dim sAmt_Script As String = sDataBufferPOVScript_Amt
						Dim iAmt As Long = dict_Amt(sKey)
						
						Select Case True
							Case iAmt = 0
								'----- clear data -----
							    Dim msvAmt_ScriptVal As New MemberScriptAndValue
								msvAmt_ScriptVal.CubeName = wfcube
								msvAmt_ScriptVal.Script = sAmt_Script
								msvAmt_ScriptVal.Amount = 0
								msvAmt_ScriptVal.IsNoData = True
								objListofLoadAmountScripts.Add(msvAmt_ScriptVal)

							Case Else
								'----- load amount -----
								
							    Dim msvAmt_ScriptVal As New MemberScriptAndValue
								msvAmt_ScriptVal.CubeName = wfcube
								msvAmt_ScriptVal.Script = sAmt_Script
								msvAmt_ScriptVal.Amount = iAmt
								msvAmt_ScriptVal.IsNoData = False
								objListofLoadAmountScripts.Add(msvAmt_ScriptVal)
						End Select
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then Brapi.ErrorLog.LogMessage(si, "sAmt_Script=" & sAmt_Script & "   iAmt=" & iAmt)
									
#End Region
'------------------------------------------------------------------------------

					Next	'loop dictionary	
'BRApi.ErrorLog.LogMessage(si, "Updated " & objListofLoadAmountScripts.count & " cells out of " & dict_Amt.Count )

								
					objResult = BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofLoadAmountScripts)
'If si.UserName.XFEqualsIgnoreCase(DEBUG_USERNAME) Then Brapi.ErrorLog.LogMessage(si, "objResult=" & objResult.Message)


'!!!!! DVLP NOTE: this code can replaces load cube above and is faster - needs more testing before implementing !!!!!
'					'---------- run data mgmt sequence ----------
'					Dim sEntityList2 As String = ""
'					For Each sUserFC As String In lsUserFundCenters
'						If sEntityList2 = "" Then
'							sEntityList2 = "E#" & sUserFC & "_General" 
'						Else
'							sEntityList2 &= ", E#" & sUserFC & "_General" 
'						End If
'					Next

'					Dim params2 As New Dictionary(Of String, String) 
'					params2.Add("EntityList", sEntityList2)
					'params2.Add("EntityList", "E#A97AA_General, E#A97CC_General")
					
'					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
'					Dim objTAI2 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Load_Withholds_To_Cube", params2)
'
'					Dim objTaskActivityItem2 As TaskActivityItem = BRApi.TaskActivity.GetTaskActivityItem(si, objTAI2.UniqueID)
'					If objTaskActivityItem2.HasError Then
'						Throw New System.Exception("ERROR: Data Management Job = Load_Withholds_To_Cube failed.  Please check activity log for more details.")
'					End If

#End Region
'====================================================================================================================================


'====================================================================================================================================
#Region "Consolidate"	

					'---------- run data mgmt sequence ----------
					Dim sEntityList3 As String = ""
					Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
					For Each sUserFC As String In lsUserFundCenters
						If sEntityList3 = "" Then
							sEntityList3 = "E#" & sUserFC
						Else
							sEntityList3 &= ", E#" & sUserFC
						End If
					Next
					
					Dim params3 As New Dictionary(Of String, String) 
					params3.Add("EntityList", sEntityList3)					

					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
					Dim objTAI3 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, gValue, "ConsolidateTargets", params3)

					'system.Threading.Thread.Sleep(500)
					Dim objTaskActivityItem3 As TaskActivityItem = BRApi.TaskActivity.GetTaskActivityItem(si, objTAI3.UniqueID)
					If objTaskActivityItem3.HasError Then
						Throw New System.Exception("ERROR: Data Management Job = ConsolidateTargets failed.  Please check activity log for more details.")
					End If
			
#End Region 
'====================================================================================================================================

				End If	'isfilevalid
				
'====================================================================================================================================
#Region "staus messages"	

				'If the validation failed, write the error out.
				'If there are more than ten, we show only the first ten messages for the sake of redablity
				Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
				If Not isFileValid Then
					Dim sErrorLog As String = ""
					For Each CurrRow In lsWithholdRecords
						sErrorLog = sErrorLog & vbCrLf & CurrRow.StringOutput()
					Next
					'Throw New XFUserMsgException(si, New Exception("LOAD FAILED" & vbCrLf & filePath & " has invalid data." & vbCrLf & vbCrLf & "Please review stage table to view errors."))
					Dim sCompletionMessageFail As String = "IMPORT FAILED" & vbCrLf _
										& "File Name: " & objXFFileInfo.Name & vbCrLf _
										& "User Name: " & si.UserName & vbCrLf _ 
										& "Time Start: " & timeStart & vbCrLf _ 
										& "Time Ended: " & System.DateTime.Now & vbCrLf _ 
										& "Total seconds for import: " & sPasstimespent.TotalSeconds & "." & vbCrLf _
										& "Number of rows processed: " & lines.count & vbCrLf _
										& vbCrLf & "File Contents:" & vbCrLf _
										& sErrorLog

					Me.WithholdsImportFileHelper(si, globals, api, args, sCompletionMessageFail, "FAIL", objXFFileInfo.Name)
					
					Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), scroll right to the column titled ""ValidationError""."
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
					Return Nothing
				End If
				
				'File load complete. Write file to explorer
				Dim timespent As System.TimeSpan = Now.Subtract(timestart)
	'			Dim sCompletionMessage As String = "IMPORT PASSED" & vbCrLf _
	'												& "seconds for import to complete: " & timespent.TotalSeconds & "." & vbCrLf _
	'												& "Number of rows minus header row: " & lines.count - 1 & vbCrLf _
	'												& "Records Loaded: " & iLineCount & vbcrlf 
				Dim sCompletionMessagePass As String = "IMPORT PASSED" & vbCrLf _
													& "File Name: " & objXFFileInfo.Name & vbCrLf _
													& "User Name: " & si.UserName & vbCrLf _ 
													& "Time Start: " & timeStart & vbCrLf _ 
													& "Time Ended: " & System.DateTime.Now & vbCrLf _ 
													& "Total seconds for import: " & sPasstimespent.TotalSeconds & "." & vbCrLf _
													& "Number of rows processed: " & lines.count & vbCrLf 
													'& vbCrLf & "File Contents:" & vbCrLf _
													'& sErrorLog

				Me.WithholdsImportFileHelper(si, globals, api, args, sCompletionMessagePass, "PASS", objXFFileInfo.Name)
				'Throw New Exception("IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper)
				Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
				BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
		
#End Region 
'====================================================================================================================================

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try					
		End Function	
		
#End Region
'===========================================================================

	End Class
	
#Region "Spendplan Record Class"
	Public Class SpendPlanRecord
		'Class to hold the requirement object
		'-----Mapping definitions----
		Public Dim ACOM As String = ""			
		Public Dim Scenario As String = ""			'derived from WF
		Public Dim FiscalYear As String = ""		
		Public Dim FundCenter As String = ""
		Public Dim FundCode As String = ""
		Public Dim MDEP As String = ""
		Public Dim APE_PT As String = ""
		Public Dim CY_Amt As String = ""


		Public Dim valid As Boolean = True
		Public Dim validationError As String = ""
		
		Public Function StringOutput() As String
			Dim output As String = Me.ACOM & "," & _	
			Me.Scenario & "," & _
			Me.FiscalYear & "," & _
			Me.FundCenter & "," & _
			Me.FundCode & "," & _
			Me.MDEP & "," & _
			Me.APE_PT & "," & _
			Me.CY_Amt & "," & _
			Me.valid & "," & _
			Me.validationError
			
			Return output
			
		End Function
	End Class
	
#End Region

#Region "Withhold Record Class"
	Public Class WithholdRecord
		'Class to hold the requirement object
		'-----Mapping definitions----
		Public Dim ACOM As String = ""			
		Public Dim Scenario As String = ""			'derived from WF
		Public Dim FiscalYear As String = ""		
		Public Dim FundCenter As String = ""
		Public Dim FundCode As String = ""
		Public Dim MDEP As String = ""
		Public Dim APE_PT As String = ""
		Public Dim OBL_CY_M1_Amt As String = ""
		Public Dim OBL_CY_M2_Amt As String = ""
		Public Dim OBL_CY_M3_Amt As String = ""
		Public Dim OBL_CY_M4_Amt As String = ""
		Public Dim OBL_CY_M5_Amt As String = ""
		Public Dim OBL_CY_M6_Amt As String = ""
		Public Dim OBL_CY_M7_Amt As String = ""
		Public Dim OBL_CY_M8_Amt As String = ""
		Public Dim OBL_CY_M9_Amt As String = ""
		Public Dim OBL_CY_M10_Amt As String = ""
		Public Dim OBL_CY_M11_Amt As String = ""
		Public Dim OBL_CY_M12_Amt As String = ""
		Public Dim COM_CY_M1_Amt As String = ""
		Public Dim COM_CY_M2_Amt As String = ""
		Public Dim COM_CY_M3_Amt As String = ""
		Public Dim COM_CY_M4_Amt As String = ""
		Public Dim COM_CY_M5_Amt As String = ""
		Public Dim COM_CY_M6_Amt As String = ""
		Public Dim COM_CY_M7_Amt As String = ""
		Public Dim COM_CY_M8_Amt As String = ""
		Public Dim COM_CY_M9_Amt As String = ""
		Public Dim COM_CY_M10_Amt As String = ""
		Public Dim COM_CY_M11_Amt As String = ""
		Public Dim COM_CY_M12_Amt As String = ""

		Public Dim valid As Boolean = True
		Public Dim validationError As String = ""
		
		Public Function StringOutput() As String
			Dim output As String = Me.ACOM & "," & _	
			Me.Scenario & "," & _
			Me.FiscalYear & "," & _
			Me.FundCenter & "," & _
			Me.FundCode & "," & _
			Me.MDEP & "," & _
			Me.APE_PT & "," & _
			Me.OBL_CY_M1_Amt & "," & _
			Me.OBL_CY_M2_Amt & "," & _
			Me.OBL_CY_M3_Amt & "," & _
			Me.OBL_CY_M4_Amt & "," & _
			Me.OBL_CY_M5_Amt & "," & _
			Me.OBL_CY_M6_Amt & "," & _
			Me.OBL_CY_M7_Amt & "," & _
			Me.OBL_CY_M8_Amt & "," & _
			Me.OBL_CY_M9_Amt & "," & _
			Me.OBL_CY_M10_Amt & "," & _
			Me.OBL_CY_M11_Amt & "," & _
			Me.OBL_CY_M12_Amt & "," & _
			Me.COM_CY_M1_Amt & "," & _
			Me.COM_CY_M2_Amt & "," & _
			Me.COM_CY_M3_Amt & "," & _
			Me.COM_CY_M4_Amt & "," & _
			Me.COM_CY_M5_Amt & "," & _
			Me.COM_CY_M6_Amt & "," & _
			Me.COM_CY_M7_Amt & "," & _
			Me.COM_CY_M8_Amt & "," & _
			Me.COM_CY_M9_Amt & "," & _
			Me.COM_CY_M10_Amt & "," & _
			Me.COM_CY_M11_Amt & "," & _
			Me.COM_CY_M12_Amt & "," & _
			Me.valid & "," & _
			Me.validationError
			
			Return output
			
		End Function
	End Class
	
#End Region

End Namespace