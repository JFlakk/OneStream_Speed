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
				Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
				
								
				Select Case args.FunctionName
					Case "savelockstatus"
						Me.SaveLockStatus()
					Case "clearkey5param"
						dbExt_ChangedResult = Me.ClearKey5param()
						Return dbExt_ChangedResult
					Case "clearkey5params_nofcappn"
						dbExt_ChangedResult = Me.ClearKey5params_NoFCAPPN()
						Return dbExt_ChangedResult	
					Case "SetDefaultAPPNParam"
						dbExt_ChangedResult = Me.SetDefaultAPPNParam()
						Return dbExt_ChangedResult
					Case "ModifyDistWH"
						dbExt_ChangedResult = Me.ModifyDistWH()
						Return dbExt_ChangedResult
					Case "ValidateDistWH"
						dbExt_ChangedResult = Me.ValidateDistWH()
						Return dbExt_ChangedResult
				End Select	


#Region "EmailTargetDistributionUpdates"
						If args.FunctionName.XFEqualsIgnoreCase("EmailTargetDistributionUpdates") Then
							Return Me.EmailTargetDistributionUpdates(si, args)
						End If
#End Region 'Updated 09/16

#Region "PublishCPROBEToTarget" 
				If args.FunctionName.XFEqualsIgnoreCase("PublishCPROBEToTarget") Then
					dbExt_ChangedResult = Me.PublishCPROBEToTarget(si,globals,api,args)
					Return dbExt_ChangedResult
				End If
#End Region 'Updated 09/17
				
#Region "Get Fund Center Lock Status and Throw Message"
						If args.FunctionName.XFEqualsIgnoreCase("GetFCLockStatusAndThrowMsg") Then
							Me.GetFCLockStatusAndThrowMsg(si, args)
						End If
#End Region	
								
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
#Region "Constants"
Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass

#End Region		

'------------------------methods----------------------------	
#Region "SaveLockStatus"
		Public Sub SaveLockStatus()
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
	Dim sTargetSource As String = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("ML_CMD_TGT_POMPBSrc","NA")	
	Dim dargs As New DashboardExtenderArgs
	dargs.FunctionName = "Check_WF_Complete_Lock"
	Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)

	If sWFStatus.XFContainsIgnoreCase("Unlocked") Then		
		Dim gWorkSpaceId As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
		Dim dmDictionary As Dictionary(Of String, String) = New Dictionary(Of String, String)
		dmDictionary.Add("TargetSource", sTargetSource)
		brapi.Utilities.ExecuteDataMgmtSequence(si,gWorkSpaceId,"CMD_TGT_PublishCPROBEToTGT",dmDictionary)
		Return selectionChangedTaskResult
	Else
		selectionChangedTaskResult.IsOK = False
		selectionChangedTaskResult.ShowMessageBox = True
		selectionChangedTaskResult.Message = vbCRLF & "NOT allowed to publish targets while the current workflow is locked. Nagvigate to the Manage Access dashboard and select the Revert Workflows button before publishing targets." & vbCRLF	
		Return selectionChangedTaskResult
	End If
End Function

#End Region

#Region "ModifyDistWH"
Public Function ModifyDistWH() As XFSelectionChangedTaskResult
	Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
	Dim Entity As String = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_TGT_FundsCenter","NA")	
	Dim dargs As New DashboardExtenderArgs
	dargs.FunctionName = "Check_WF_Complete_Lock"
	Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)
    BRAPI.ErrorLog.LogMessage(si,"Hit Ent: " & Entity)
	If sWFStatus.XFContainsIgnoreCase("Unlocked") Then		
		Dim gWorkSpaceId As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "40 CMD TGT")
		Dim dmDictionary As Dictionary(Of String, String) = New Dictionary(Of String, String)
		dmDictionary.Add("Entity", Entity)
		brapi.Utilities.ExecuteDataMgmtSequence(si,gWorkSpaceId,"CMD_TGT_ModifyDistWH",dmDictionary)
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
Public Function ValidateDistWH() As XFSelectionChangedTaskResult
	Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
	Dim Entity As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_CMD_TGT_FundsCenter","NA")	
	Dim cvName = "CMD_TGT_FDX_ValidateDist_CV"
	Dim dt As New DataTable 
	dt = GetFDXvalidateTGTData(cvName,Entity)

	Dim filteredRows = dt.AsEnumerable().Where(Function(row) row.Field(Of Decimal)("ColumnName") <> 0)
	If filteredRows.Any() Then
		dt = filteredRows.CopyToDataTable()
	Else
		dt.Clear()
	End If
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
#Region "Clear Key5 params"
Public Function ClearKey5param()
	Try
		Dim paramsToClear As String = "ML_CMD_TGT_AddRow_APEPT,ML_CMD_TGT_AddRow_APPN,ML_CMD_TGT_AddRow_FundCode,ML_CMD_TGT_AddRow_MDEP,ML_CMD_TGT_AddRow_DollarType,ML_CMD_TGT_AddRow_CostCat,ML_CMD_TGT_AddRow_SAG"
				
		Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.ClearSelections(si, globals, api, args, paramsToClear)	
		selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
		selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
		Return selectionChangedTaskResult

    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function

Public Function ClearKey5params_NoFCAPPN()
	Try
		Dim paramsToClear As String = "ML_CMD_TGT_AddRow_APEPT,ML_CMD_TGT_AddRow_FundCode,ML_CMD_TGT_AddRow_MDEP,ML_CMD_TGT_AddRow_DollarType,ML_CMD_TGT_AddRow_CostCat,ML_CMD_TGT_AddRow_SAG"						
						
		Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.ClearSelections(si, globals, api, args, paramsToClear)	
		selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
		selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
		Return selectionChangedTaskResult

    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function
#End Region	

#Region "Set Default APPN Parameter"
Public Function SetDefaultAPPNParam() As Object

	Dim dKeyVal As New Dictionary(Of String, String)
							
	dKeyVal.Add("ML_CMD_TGT_AddRow_APPN","OMA")
	dKeyVal.Add("ML_CMD_TGT_AddRow_APEPT","OMA")
	dKeyVal.Add("ML_CMD_TGT_AddRow_FundCode",String.Empty)
	dKeyVal.Add("ML_CMD_TGT_AddRow_MDEP",String.Empty)
	dKeyVal.Add("ML_CMD_TGT_AddRow_DollarType",String.Empty)
	dKeyVal.Add("ML_CMD_TGT_AddRow_CostCat",String.Empty)
	dKeyVal.Add("ML_CMD_TGT_AddRow_SAG",String.Empty)					

'Added 2 line to clear user cache before launching getcascadingfilters
	BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_TGT_CascadingFilterCache",$"CMD_TGT_rebuildparams_APPN","")
	BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_TGT_CascadingFilterCache",$"CMD_TGT_rebuildparams_Other","")	
	
	Return Workspace.GBL.GBL_Assembly.GBL_Helpers.SetParameter(si, globals, api, dKeyVal)
		
End Function
#End Region

		Private Function GetFDXvalidateTGTData(ByVal cvName As String,ByVal entFilter As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "40 CMD TGT"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{wfInfoDetails("ScenarioName")}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			Brapi.ErrorLog.LogMessage(si,"Scenario" & scenFilter)
			Brapi.ErrorLog.LogMessage(si,"timeFilter" & timeFilter)
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 8, False)
If dt Is Nothing
			BRAPI.ErrorLog.LogMessage(si,$"Hit FDX {EntFilter} - {cvName}")
		End If
			Return dt
		End Function
	End Class

End Namespace