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
Imports OneStreamWorkspacesApi.V800
Imports System.Net.Mail

'WF Complete & Revert
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.GBL_BFRM_SolutionHelper
Public Class MainClass
	
#Region "Main Function"
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
					Dim WorkflowForStatus As WorkFlowInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si) 
					Dim WorkflowUnitForStatus As WorkflowUnitInfo = WorkflowForStatus.GetSelectedWorkflowUnitInfo()
					Dim WorkflowProfileNameForStatus As String = WorkflowUnitForStatus.ProfileName
					Dim ScenarioForStatus As String = WorkflowUnitForStatus.ScenarioName
					Dim WFTimeForStatus As String = WorkflowUnitForStatus.TimeName
					Dim WFClusterPkForStatus As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, WorkflowProfileNameForStatus, ScenarioForStatus, WFTimeForStatus)
					Dim WFInfoForStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, WFClusterPkForStatus)
'brapi.ErrorLog.LogMessage(si,"WF Status: "& WFInfoForStatus.GetOverallStatusText(False))
					
					If WFInfoForStatus.GetOverallStatusText(False) = ", Locked" Then
						Throw New Exception("Workflow Status: " & environment.NewLine & "Function not available while workflow is locked." & environment.NewLine)
					Else
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowComplete"))
'brapi.ErrorLog.LogMessage(si,"Workflow Complete")					
							Return Me.WorkflowCompleteOneOrAllMain(si, globals, api, args)							
							
						Else If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteEmail")) 
							If WFInfoForStatus.GetOverallStatusText(False) <> "Completed" Then
								Throw New Exception("Workflow Status: " & environment.NewLine & "Function not available while workflow is incomplete." & environment.NewLine)
							Else
								Dim SmtpServer As New SmtpClient()
								Dim mail As New MailMessage()
								SmtpServer.Host = "10.79.209.161"
								'Create the message	
								Dim ScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
								Dim ProfileName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk).toString
								Dim ProfileNameParse As List(Of String) = stringhelper.SplitString(ProfileName, "(")
								Dim WFProfileName As String = ProfileNameParse(0)
								'Dim ProfileNameParseRAP As List(Of String) = stringhelper.SplitString(ProfileName, " ")
								'Dim WFProfileNameRAP As String = ProfileNameParseRAP(1)
								'BRApi.ErrorLog.LogMessage(si, WFProfileNameOMBJ & ": " & ScenarioName)
								Dim objGroupInfo1 As GroupInfo = BRApi.Security.Admin.GetGroup(si, "DHS_Admin_All")
								Dim objGroupGUID1 As Guid = objGroupInfo1.Group.UniqueID
								Dim objGroupInfo2 As GroupInfo = BRApi.Security.Admin.GetGroup(si, "DHS_OCFO_PA&E_Integration")
								Dim objGroupGUID2 As Guid = objGroupInfo2.Group.UniqueID
								Dim objGroupInfo3 As GroupInfo = BRApi.Security.Admin.GetGroup(si, "DHS_Component_" & WFProfileName & "")
								Dim objGroupGUID3 As Guid = objGroupInfo3.Group.UniqueID
								
								Dim Text1Filter As String = "Email Notifications"
								Dim notificationUsers As List(Of UserInfo) = BRApi.Workflow.General.GetUsersInWorkflowGroupForParents (si, si.WorkflowClusterPk, SharedConstants.WorkflowProfileAttributeIndexes.WorkflowExecutionGroup, True, True, Text1Filter)
								For Each notificationUser As UserInfo In notificationUsers
									If (BRApi.Security.Authorization.IsUserInGroup(si, objGroupGUID1) Or BRApi.Security.Authorization.IsUserInGroup(si, objGroupGUID2) Or BRApi.Security.Authorization.IsUserInGroup(si, objGroupGUID3))
										Dim objUserInfo As UserInfo = BRApi.Security.Authorization.GetUser(si, notificationUser.User.ToString)
										Dim UserEmail As String = objUserInfo.User.Email
										mail = New MailMessage()
										mail.to.add(UserEmail)
										mail.From = New MailAddress("PPBEOneNumberNotifications@associates.hq.dhs.gov")
										mail.Subject = WFProfileName & ": " & ScenarioName & " Submission Complete"
										Dim Message As String = "Hello," & vbCrLf &
											            "The "& WFProfileName & " " & ScenarioName & " submission is complete in OneNumber. Navigate to the " & WFProfileName & " workflow and the " & ScenarioName & " scenario to review." & vbCrLf & vbCrLf &
														"*This is an automated message. Please DO NOT reply to this message/email address.*"
										mail.Body = Message
										'Send the message
										SmtpServer.Send(mail)
									End If
								Next
							End If
							'Return Me.WorkflowCompleteOneOrAllMain(si, globals, api, args)
							
						Else If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevert"))
							Return Me.WorkflowRevertOneOrAllMain(si, globals, api, args)
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("CompleteSelectedWorkflows"))
							Return Me.CompleteSelectedWorkflows(si, globals, api, args)	
						End If 
						
						If (args.FunctionName.XFEqualsIgnoreCase("RevertSelectedWorkflows"))
							Return Me.RevertSelectedWorkflows(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteOptionalPayroll"))
							Return Me.WorkflowCompleteOptionalPayroll(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("CompleteWithholdReview"))
							Return Me.CompleteWithholdReview(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteCommandProgramming"))
							Return Me.WorkflowCompleteCommandProgramming(si, globals, api, args)	
						End If
'New						
						If (args.FunctionName.XFEqualsIgnoreCase("zMFWorkflowCompleteCommandProgramming"))
							Return Me.zMFWorkflowCompleteCommandProgramming(si, globals, api, args)	
						End If
'New						
						If (args.FunctionName.XFEqualsIgnoreCase("zMFWorkflowRevertCommandProgramming"))
							Return Me.zMFWorkflowRevertCommandProgramming(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertCommandProgramming"))
							Return Me.WorkflowRevertCommandProgramming(si, globals, api, args)	
						End If						
					
						If (args.FunctionName.XFEqualsIgnoreCase("CompleteIntDisWorkflowPayCost"))
							Return Me.CompleteIntDisWorkflowPayCost(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("CompleteIntDisWorkflowPayCostAdministrative"))
							Return Me.CompleteIntDisWorkflowPayCostAdministrative(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("CompleteIntDisWorkflowNonPayCostAdministrative"))
							Return Me.CompleteIntDisWorkflowNonPayCostAdministrative(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertBUD"))
							Return Me.WorkflowRevertBUD(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteBUD"))
							Return Me.WorkflowCompleteBUD(si, globals, api, args)	
						End If
						
							
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertCMDTGTDST"))
							Return Me.WorkflowRevertCMDTGTDST(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteCMDTGTDST"))
							Return Me.WorkflowCompleteCMDTGTDST(si, globals, api, args)	
						End If
						
						If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertSPLN"))
							Return Me.WorkflowRevertSPLN(si, globals, api, args)	
						End If
						
					
					'Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						'If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							'Not used
						'End If
					End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Public Functions"

		Public Function WorkflowCompleteOneOrAllMain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'This logic is commented out for RMW
				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
'				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
'				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
'				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
'				If Not wfRegParentStatus.AllTasksCompleted Then															

					Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
					Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					'Added by EBurke 3-19 Get Primary Dashboard for refresh
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					
					'Update workflow to COMPLETED
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
'					Dim StepwfClusterPk As New WorkflowUnitClusterPk()
'					StepwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, curProfile.Name & ".Payroll Import", wfScenarioName, wfTimeName)
'					Dim StepwfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, StepwfClusterPk)
					
'					If curProfile.Name.XFEqualsIgnoreCase(wfCube & " Target (Distribution)") And Not wfCube.XFEqualsIgnoreCase("TRADOC") Then 
'						'complete WFs for Optional Payroll Import
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)					
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)					
'						BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
					
'						BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
						
'					End If 
					
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage(Me.m_MsgWorkflowCompleted, wfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = False	
					selectionChangedTaskResult.Message = "This workflow step has been marked complete."

'				Else
'					'Parent Certified, cannot update workflow
'					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
'					selectionChangedTaskResult.IsOK = True							
'					selectionChangedTaskResult.ShowMessageBox = True
'					selectionChangedTaskResult.Message = Me.m_MsgCannotCompleteWorkflow												
'				End If	
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
		
		Public Function WorkflowRevertOneOrAllMain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
				If (Not wfRegParentStatus.AllTasksCompleted) Then															
					'Added by EBurke 3-19 Get Primary Dashboard for refresh
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					
					'Update the workspace workflow to INPROCESS
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)

					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage(Me.m_MsgWorkflowReverted, wfClusterDesc), "", Me.m_MsgWorkflowRevertedReasonButton, Guid.Empty)
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = False
					'Added by Eburke to refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					
				Else
					'Parent Certified, cannot update workflow
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True							
					selectionChangedTaskResult.ShowMessageBox = True
					selectionChangedTaskResult.Message = Me.m_MsgCannotRevertWorkflow												
				End If	

				
				Return selectionChangedTaskResult								
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
		
		 
		Public Function CompleteSelectedWorkflows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Modified 7/30/24 ehart: Added functionality for parent wf's
			Try	
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim ParentwfClusterPk As New WorkflowUnitClusterPk()
					Dim TargetwfClusterPk As New WorkflowUnitClusterPk()
					Dim ApproverwfClusterPk As New WorkflowUnitClusterPk()
					Dim wfClusterPk As New WorkflowUnitClusterPk()
					Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
					Dim sTime As String = sScenario.Substring(sScenario.IndexOf("FY") + 2, 4)
					Dim noUpdateMsg As New Text.StringBuilder
					Dim noUpdateCount As Integer = 0
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					Dim sEntities As String = args.NameValuePairs.XFGetValue("EntityList")
					Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
					
				
				
					'Dim sWfPRofileList As String = ""
					Dim sFCList As String() = sEntities.Split(",")
					For Each sFC In sFCList
						
						If BRApi.Finance.Members.HasChildren(si, DimPk, BRApi.Finance.Members.GetMemberId(si, dimtypeid.Entity, sFC)) Then
							Dim swfTargetProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, sFC & " Analyst (SpendPlan)")
							Dim swfTargetBaseProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,swfTargetProfileInfo.Name,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
						
							For Each sWorkflowProfile In swfTargetBaseProfileInfo	
								
								Dim sProfileFC As String = sWorkflowProfile.Name.Substring(0,sWorkflowProfile.Name.IndexOf(" "))
								If Not sProfileFC.XFEqualsIgnoreCase(sFC) Then
									Continue For
								End If
								
								If sWorkflowProfile.Name.XFContainsIgnoreCase(sFC) Then
								

									wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile.Name, sScenario, sTime)
									Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)	
									'Update workflows to COMPLETED TRADOC
									If sFC.XFContainsIgnoreCase("A57") Then
										If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Target Import") Then
										Continue For
										Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Target Review") Then
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Administrative") Then
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										Else If sWorkflowProfile.Name.XFequalsIgnoreCase(sFC & " Target Distribution (SpendPlan)") Then
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)											
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
											'Certify Analyst Parent
											ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Analyst (SpendPlan)", sScenario, sTime)
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											'Certify Approver Parent
											ApproverwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										End If										
									End If
									
									'Update workflows to COMPLETED 
									If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Or sWorkflowProfile.Name.XFContainsIgnoreCase(sFC & " (SpendPlan).Monthly Withhold") Then
										Continue For
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Import") And Not sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										'certify Parent and Approver parent
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Import") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										
									Else If sWorkflowProfile.Name.XFEqualsIgnoreCase(sFC & " (SpendPlan)") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										'Certify Target Parent
										TargetwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Target Distribution (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
'										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										'Certify Analyst Parent
										ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Analyst (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										'Certify Approver Parent
										ApproverwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)											
										'Certify except Target Distribution Workspace (it doesn't exist)
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Administrative") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Target Review") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
'									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Target Distribution (SpendPlan).Monthly Withhold Review") Then
'										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
									
									Else If Not sWorkflowProfile.Name.XFContainsIgnoreCase("Target Distribution") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
										
									End If
								End If
							Next
							
						Else
						
							
							Dim swfParentProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, sFC & " Approver (SpendPlan)")
								
							Dim swfBaseProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,swfParentProfileInfo.Name,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							For Each sWorkflowProfile In swfBaseProfileInfo
											
								wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile.Name, sScenario, sTime)
																				
								Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
								
								'Update workflows to COMPLETED
								'If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Or sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Then
								If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Then
									Continue For
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Import") And Not sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
								'certify Parent and Approver parent
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Import") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Review") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.InputForms, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
								
								Else If sWorkflowProfile.Name.XFEqualsIgnoreCase(sFC & " (SpendPlan)") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
								'Certify Approver Parent
									ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
							
								Else 
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									
								End If 
								
							Next
						
						End If
					Next
					
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Workflows have been complete"
					Return selectionChangedTaskResult
					

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function RevertSelectedWorkflows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Modified 7/30/24 ehart: Added functionality for parent wf's
			Try	
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim ParentwfClusterPk As New WorkflowUnitClusterPk()
					Dim TargetwfClusterPk As New WorkflowUnitClusterPk()
					Dim ApproverwfClusterPk As New WorkflowUnitClusterPk()
					Dim wfClusterPk As New WorkflowUnitClusterPk()
					Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
					Dim sTime As String = sScenario.Substring(sScenario.IndexOf("FY") + 2, 4)
					Dim noUpdateMsg As New Text.StringBuilder
					Dim noUpdateCount As Integer = 0
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					Dim sEntities As String = args.NameValuePairs.XFGetValue("EntityList")	
					Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)

				
					'Dim sWfPRofileList As String = ""
					Dim sFCList As String() = sEntities.Split(",")
					For Each sFC In sFCList
						
						If BRApi.Finance.Members.HasChildren(si, DimPk, BRApi.Finance.Members.GetMemberId(si, dimtypeid.Entity, sFC)) Then
							Dim swfTargetProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, sFC & " Analyst (SpendPlan)")
							Dim swfTargetBaseProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,swfTargetProfileInfo.Name,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).ToList()
							
							For Each sWorkflowProfile In swfTargetBaseProfileInfo	
								Dim sProfileFC As String = sWorkflowProfile.Name.Substring(0,sWorkflowProfile.Name.IndexOf(" "))
								If Not sProfileFC.XFEqualsIgnoreCase(sFC) Then
									Continue For
								End If
								If sWorkflowProfile.Name.XFContainsIgnoreCase(sFC) Then
									wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile.Name, sScenario, sTime)
									Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)	
									If sFC.XFContainsIgnoreCase("A57") Then
										If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Target Import") Then
										Continue For
										Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Target Review") Then
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Administrative") Then
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										Else If sWorkflowProfile.Name.XFequalsIgnoreCase(sFC & " Target Distribution (SpendPlan)") Then
											'revert approver parent
											ApproverwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											'revert Analyst Parent
											ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Analyst (SpendPlan)", sScenario, sTime)
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
											BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
											'revert target distribution
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
											BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)											
											
											
										End If										
									End If
									
									'Update workflows to COMPLETED
									If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Or sWorkflowProfile.Name.XFContainsIgnoreCase(sFC & " (SpendPlan).Monthly Withhold") Then
										Continue For
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Import") And Not sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Then	
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										'certify Parent and Approver parent
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Import") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										
									Else If sWorkflowProfile.Name.XFEqualsIgnoreCase(sFC & " (SpendPlan)") Then
										'Certify Approver Parent
										ApproverwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ApproverwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										'Certify Analyst Parent
										ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Analyst (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										'Certify Target Parent
										TargetwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Target Distribution (SpendPlan)", sScenario, sTime)
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
'										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, TargetwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
										
										'Certify except Target Distribution Workspace (it doesn't exist)
									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Administrative") Or sWorkflowProfile.Name.XFContainsIgnoreCase("Target Review") Then
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
'									Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Target Distribution (SpendPlan).Monthly Withhold Review") Then
'										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
									
									Else If Not sWorkflowProfile.Name.XFContainsIgnoreCase("Target Distribution")
										BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)
									
										
									End If
								End If
							Next
						
						Else
							Dim swfParentProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, sFC & " Approver (SpendPlan)")			
							Dim swfBaseProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,swfParentProfileInfo.Name,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).ToList()
							
							
							For Each sWorkflowProfile In swfBaseProfileInfo
									
								
								wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile.Name, sScenario, sTime)
																				
								Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
								
								'Update workflows to COMPLETED
								If sWorkflowProfile.Name.XFContainsIgnoreCase(".Adj") Then
									Continue For
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Import") And Not sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Import") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.LoadCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
								Else If sWorkflowProfile.Name.XFContainsIgnoreCase("Withhold Review") Then
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.InputForms, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									
									'certify Parent and Approver parent
									
								Else If sWorkflowProfile.Name.XFEqualsIgnoreCase(sFC & " (SpendPlan)") Then
									'Revert Approver Parent
									ParentwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sFC & " Approver (SpendPlan)", sScenario, sTime)
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, ParentwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Certify, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Confirm, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)	
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)							
								
								Else 
									BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
								
								End If 
								
								Next
							End If
						Next
					
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Workflows have been reverted"
					Return selectionChangedTaskResult
					

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function WorkflowCompleteOptionalPayroll(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'This logic is commented out for RMW
				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
'				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
'				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
'				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
'				If Not wfRegParentStatus.AllTasksCompleted Then															

					Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
					Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					'Added by EBurke 3-19 Get Primary Dashboard for refresh
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					
					'Update workflow to COMPLETED
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					Dim StepwfClusterPk As New WorkflowUnitClusterPk()
					StepwfClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, curProfile.Name & ".Payroll Import", wfScenarioName, wfTimeName)
					Dim StepwfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, StepwfClusterPk)
					
					
					'complete WFs for Optional Payroll Import
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)					
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ValidateTransform, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ValidateIntersection, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.LoadCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)					
					BRApi.Workflow.Status.SetWorkflowStatus(si, StepwfClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", StepwfClusterDesc), "", "", Guid.Empty)	
					
					'Complete WFs for current workspace
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)						
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage(Me.m_MsgWorkflowCompleted, wfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = False	
					selectionChangedTaskResult.Message = "This workflow step has been marked complete."

'				Else
'					'Parent Certified, cannot update workflow
'					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
'					selectionChangedTaskResult.IsOK = True							
'					selectionChangedTaskResult.ShowMessageBox = True
'					selectionChangedTaskResult.Message = Me.m_MsgCannotCompleteWorkflow												
'				End If	
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
		
		Public Function CompleteWithholdReview(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			Try
				
					Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					Dim currDashboard As Dashboard = args.PrimaryDashboard
					Dim dashAction As String ="Refresh"
					Dim ParentwfClusterPk As New WorkflowUnitClusterPk()
					Dim TargetwfClusterPk As New WorkflowUnitClusterPk()
					Dim ApproverwfClusterPk As New WorkflowUnitClusterPk()
					Dim wfClusterPk As New WorkflowUnitClusterPk()
					Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
					Dim sTime As String = sScenario.Substring(sScenario.IndexOf("FY") + 2, 4)
					Dim noUpdateMsg As New Text.StringBuilder
					Dim noUpdateCount As Integer = 0
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					Dim sEntities As String = args.NameValuePairs.XFGetValue("EntityList")	
					Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)

				
					'Dim sWfPRofileList As String = ""
					Dim sFCList As String() = sEntities.Split(",")
					For Each sFC In sFCList
						Dim swfTargetProfileName As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, sFC & " (SpendPlan).Monthly Withhold Review")
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, swfTargetProfileName.name, sScenario, sTime)	
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.InputForms, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)			
						Next
					
					Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
					Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
					objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
					objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Workflow steps have been marked as complete"
					Return selectionChangedTaskResult
				
				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
		
		Public Function WorkflowCompleteCommandProgramming(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to lock non-L2 PGM wf steps
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Lock Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been locked."
				
				'Lock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been locked."							
				End If
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function

		Public Function zMFWorkflowCompleteCommandProgramming(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to lock non-L2 PGM wf steps
			Try
				
'brapi.ErrorLog.LogMessage(si,"inside zmfwfcompletecmdpgm")
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
'New - from forum - locks all workflow profiles including 2024 AFC PGM
'brapi.Workflow.Locking.LockWorkflowUnit(si, wfClusterPK, WorkflowProfileTypes.AllProfiles, guid.Empty)

					'Lock Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
'						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
brapi.Workflow.Locking.UnlockWorkflowUnit(si,wfClusterPK)
						'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
									
					
					
					
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been locked."
				
				'Lock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been locked."							
				End If
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function

		Public Function zMFWorkflowRevertCommandProgramming(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to unlock non-L2 PGM wf steps		
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Unlock Sub CMD WF steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
'brapi.ErrorLog.LogMessage(si, "curProfile.Name  " & curProfile.Name)
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
'brapi.ErrorLog.LogMessage(si, "sWorkflowProfile & time: " & sWorkflowProfile)
				
					'Unlock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
'New
'brapi.Workflow.Locking.UnlockWorkflowUnit(si,wfClusterPK)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					

					'Unlock Chidlren Sub CMD WF steps
					'List of wf steps not inclusive of parent
					'Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					
					'List of Wf steps with parent (New)
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()			
					
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
'						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
'brapi.Workflow.Locking.UnlockWorkflowUnit(si,wfClusterPK)
'						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been unlocked."
				
				'Unlock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
'					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Unlock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been unlocked."							
				End If
				
				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
			
		Public Function WorkflowRevertCommandProgramming(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to unlock non-L2 PGM wf steps		
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Unlock Sub CMD WF steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))

					'Unlock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)

					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					

					'Unlock Chidlren Sub CMD WF steps
					'List Of wf steps Not inclusive Of parent
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been unlocked."
				
				'Unlock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Unlock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(PGM).Adj") Or sWFTargetProfileName.EndsWith("(PGM).Import") Or sWFTargetProfileName.Contains("(PGM).Review Financials") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been unlocked."							
				End If
				
				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
	
		Public Function WorkflowCompleteBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to lock non-L2 PGM wf steps
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Lock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(BUD).Adj") Or sWFTargetProfileName.EndsWith("(BUD).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been locked."
				
				'Lock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(BUD).Adj") Or sWFTargetProfileName.EndsWith("(BUD).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been locked."							
				End If
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
		
		Public Function WorkflowRevertBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to unlock non-L2 PGM wf steps		
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Unlock Sub CMD WF steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Unlock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(BUD).Adj") Or sWFTargetProfileName.EndsWith("(BUD).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been unlocked."
				
				'Unlock CMD WF Steps	
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
'					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Unlock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("(BUD).Adj") Or sWFTargetProfileName.EndsWith("(BUD).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been unlocked."							
				End If
				
				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
	
#Region "CompleteIntDisWorkflowPayCost"
	'Complete WF button on Initial Distribution BE Step
Public Function CompleteIntDisWorkflowPayCost(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
	Try
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		Dim swfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim swfYear As String = timeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)
		Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		
		Dim wfPk2_FTE As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.FTE Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_FTE As WorkflowUnitClusterPk = wfPk2_FTE.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_PayRatesCalc As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Pay Rates Calc", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayRatesCalc As WorkflowUnitClusterPk = wfPk2_PayRatesCalc.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_PayCostImport As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Total Pay Cost Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayCostImport As WorkflowUnitClusterPk = wfPk2_PayCostImport.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_GFEBSImport As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Pay Rates GFEBS Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_GFEBSImport As WorkflowUnitClusterPk = wfPk2_GFEBSImport.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_PayCost As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Total Pay Cost Calc", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayCost As WorkflowUnitClusterPk = wfPk2_PayCost.CreateWorkflowUnitClusterPk()

		
		Dim currDashboard As Dashboard = args.PrimaryDashboard
		Dim dashAction As String ="Refresh"
		Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
		Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
		objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
		objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
		
		
		Dim selectionChangedTaskResult_Admin As New XFSelectionChangedTaskResult()
		
		'Auto-Complete FTE Step
		Dim objLoadTransformProcessInfo_FTE As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_FTE)
		Dim valTranProcessInfo_FTE As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_FTE, True)
		Dim valIntersectProcessInfo_FTE As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_FTE, True)
		Dim lcProcessInfo_FTE As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_FTE)
		BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_FTE, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
		'Auto-Complete Pay Rates GFEBS Import Step
		BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_GFEBSImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		Dim objLoadTransformProcessInfo_GFEBSImport As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_GFEBSImport)
		Dim valTranProcessInfo_GFEBSImport As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_GFEBSImport, True)
		Dim valIntersectProcessInfo_GFEBSImport As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_GFEBSImport, True)
		Dim lcProcessInfo_GFEBSImport As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_GFEBSImport)
		Dim pcProcessInfo_GFEBSImport As ProcessCubeProcessInfo = BRApi.DataQuality.Process.ExecuteProcessCube(si, wfUnitClusterPk_GFEBSImport, StepClassificationTypes.ProcessCube, False)
		
		
		'Auto-Complete Pay Rates Calc Step
		BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayRatesCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
		'Auto-Complete Total Pay Cost Calc Step
		BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCost, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
		
		selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
		selectionChangedTaskResult.IsOK = True		
		selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
		selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

		'Process functionalit
		Dim customSubstVars As New Dictionary(Of String, String)
		Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")
		BRApi.Utilities.ExecuteDataMgmtSequence(si, gValue,"wfConsolidate", customSubstVars )
		
		'Complete Total Pay Cost Import Step
		Dim objLoadTransformProcessInfo_PayCostImport As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_PayCostImport)
		Dim valTranProcessInfo_PayCostImport As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_PayCostImport, True)
		Dim valIntersectProcessInfo_PayCostImport As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_PayCostImport, True)
		Dim lcProcessInfo_PayCostImport As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_PayCostImport)

		BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCostImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)

		selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
		selectionChangedTaskResult.IsOK = True		
		selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
		selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

			
		Return SelectionChangedTaskResult

		
		Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))	
		End Try

	End Function
				
#End Region

'For Target Build: This function allows the user to select either Pay Import or Pay Calc. WHen selecting one, the wf steps for the others are autocompleted
Public Function CompleteIntDisWorkflowPayCostAdministrative(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
	Try
		Dim mode As String = args.NameValuePairs.XFGetValue("mode")
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		Dim swfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim swfYear As String = timeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)
		Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		Dim param As String = args.NameValuePairs.XFGetValue("param")
		Dim paramValue As String = ""
		If wfCube.XFContainsIgnoreCase("USAREUR") Then 
			wfCube = "USAREUR AF"
		End If
		
		Dim wfPk2_FTE As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.FTE Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_FTE As WorkflowUnitClusterPk = wfPk2_FTE.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_GFEBSImport As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Pay Rates GFEBS Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_GFEBSImport As WorkflowUnitClusterPk = wfPk2_GFEBSImport.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_PayRatesCalc As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Pay Rates Calc", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayRatesCalc As WorkflowUnitClusterPk = wfPk2_PayRatesCalc.CreateWorkflowUnitClusterPk()			
		
		Dim wfPk2_PayCostCalc As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Total Pay Cost Calc", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayCostCalc As WorkflowUnitClusterPk = wfPk2_PayCostCalc.CreateWorkflowUnitClusterPk()
		
		Dim wfPk2_PayCostImport As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.Total Pay Cost Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_PayCostImport As WorkflowUnitClusterPk = wfPk2_PayCostImport.CreateWorkflowUnitClusterPk()

		
		Dim currDashboard As Dashboard = args.PrimaryDashboard
		Dim dashAction As String ="Refresh"
		Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
		Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
		objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
		objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
		
		
		Dim selectionChangedTaskResult_Admin As New XFSelectionChangedTaskResult()
		
		If mode = "PayImport" Then
			paramValue = "Pay Cost Import"
			'Auto-Complete FTE Step

			Dim objLoadTransformProcessInfo_FTE As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_FTE)
			Dim valTranProcessInfo_FTE As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_FTE, True)
			Dim valIntersectProcessInfo_FTE As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_FTE, True)
			Dim lcProcessInfo_FTE As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_FTE)
			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_FTE, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
			
			'Auto-Complete Pay Rates GFEBS Import Step
			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_GFEBSImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
			Dim objLoadTransformProcessInfo_GFEBSImport As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_GFEBSImport)
			Dim valTranProcessInfo_GFEBSImport As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_GFEBSImport, True)
			Dim valIntersectProcessInfo_GFEBSImport As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_GFEBSImport, True)
			Dim lcProcessInfo_GFEBSImport As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_GFEBSImport)
			Dim pcProcessInfo_GFEBSImport As ProcessCubeProcessInfo = BRApi.DataQuality.Process.ExecuteProcessCube(si, wfUnitClusterPk_GFEBSImport, StepClassificationTypes.ProcessCube, False)	
			
			'Auto-Complete Pay Rates Calc Step
			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayRatesCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
			'Auto-Complete Total Pay Cost Calc Step
			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCostCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
			'Auto Uncomplete Total Pay Cost Import Step
			
			Dim wfStatus As String = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_PayCostImport).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed")
				BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCostImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			End If
			selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			selectionChangedTaskResult.IsOK = True		
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

			'Process functionalit
			Dim customSubstVars As New Dictionary(Of String, String)
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")
			BRApi.Utilities.ExecuteDataMgmtSequence(si,gValue, "wfConsolidate", customSubstVars )
		End If
			
		If mode = "PayCalc" Then	
			paramValue = "Pay Cost Calculation"
			'Auto-Complete Total Pay Cost Import Step
			Dim objLoadTransformProcessInfo_PayCostImport As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_PayCostImport)
			Dim valTranProcessInfo_PayCostImport As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_PayCostImport, True)
			Dim valIntersectProcessInfo_PayCostImport As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_PayCostImport, True)
			Dim lcProcessInfo_PayCostImport As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_PayCostImport)

			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCostImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
	
			
			'Revert Completion of PayImport steps
			Dim wfStatus As String = ""
			'Auto Uncomplete FTE Step
			wfStatus = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_FTE).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed") Then BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_FTE, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			'Auto Uncomplete Pay Rates GFEBS Import Step
			wfStatus = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_GFEBSImport).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed") Then BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_GFEBSImport, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			'Auto Uncomplete Pay Rates Calc Step
			wfStatus = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_PayRatesCalc).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed") Then BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayRatesCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			'Auto Uncomplete Total Pay Cost Calc Step
			wfStatus = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_PayCostCalc).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed") Then BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_PayCostCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			
			
			
			
			selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			selectionChangedTaskResult.IsOK = True		
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

		End If	
		
		Dim dKeyVal As New Dictionary(Of String, String)
		dKeyVal.Add(param,paramValue)
		
		Return Me.SetParameter(si, globals, api, dKeyVal, SelectionChangedTaskResult)
			
'		Return SelectionChangedTaskResult

		
	Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))	
				End Try

End Function	

'For Target Build: This function allows the user to select either NonPay Import or NonPay Calc. When selecting one, the wf steps for the others are autocompleted. This function also update a param component to display in a label which method was selected
'Usage: Target Build > Administrative > Administrative > NonPay Cost Import button and NonPay Cost Calculation button
'Note for Admins: When traversing across ACOMS, the selected method label will carry over from one ACOM to another, potentially creating confusion. This would not happen to users.
Public Function CompleteIntDisWorkflowNonPayCostAdministrative(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
	Try
		Dim mode As String = args.NameValuePairs.XFGetValue("mode")
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		Dim swfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim swfYear As String = timeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)
		Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		Dim param As String = args.NameValuePairs.XFGetValue("param")
		Dim paramValue As String = ""
		
		If wfCube.XFContainsIgnoreCase("USAREUR") Then 
			wfCube = "USAREUR AF"
		End If
		
		Dim wfPk2_NonPayCalc As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.NonPay Cost Modify", swfScenario, swfYear)
		Dim wfUnitClusterPk_NonPayCalc As WorkflowUnitClusterPk = wfPk2_NonPayCalc.CreateWorkflowUnitClusterPk()			
		
		Dim wfPk2_NonPayCostImport As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, wfCube & " Target Build.NonPay Cost Import", swfScenario, swfYear)
		Dim wfUnitClusterPk_NonPayCostImport As WorkflowUnitClusterPk = wfPk2_NonPayCostImport.CreateWorkflowUnitClusterPk()

		
		Dim currDashboard As Dashboard = args.PrimaryDashboard
		Dim dashAction As String ="Refresh"
		Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
		Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
		objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
		objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
		
		
		Dim selectionChangedTaskResult_Admin As New XFSelectionChangedTaskResult()
		
		If mode = "NonPayImport" Then
			paramValue = "NonPay Cost Import"
			'Auto-Complete NonPay Modify Step
			BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_NonPayCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Auto complete", "Error Auto Complete ", "Auto Complete", Guid.Empty)
		
			'Auto Uncomplete NonPay Cost Import Step
			
			Dim wfStatus As String = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_NonPayCostImport).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed")
				BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_NonPayCostImport, StepClassificationTypes.DataLoadRetransform, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
			End If
			selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			selectionChangedTaskResult.IsOK = True		
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

		End If
			
		If mode = "NonPayCalc" Then	
			paramValue = "NonPay Cost Calculation"
			'Auto-Complete NonPay Cost Import Step
			Dim objLoadTransformProcessInfo_NonPayCostImport As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteRetransform(si,wfUnitClusterPk_NonPayCostImport)
			Dim valTranProcessInfo_NonPayCostImport As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfUnitClusterPk_NonPayCostImport, True)
			Dim valIntersectProcessInfo_NonPayCostImport As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfUnitClusterPk_NonPayCostImport, True)
			Dim lcProcessInfo_NonPayCostImport As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfUnitClusterPk_NonPayCostImport)
			Dim pcProcessInfo_NonPayCostImport As ProcessCubeProcessInfo = BRApi.DataQuality.Process.ExecuteProcessCube(si, wfUnitClusterPk_NonPayCostImport, StepClassificationTypes.ProcessCube, False)	
			
			'Auto Uncomplete NonPay Cost Modify Step
			Dim wfStatus As String = BRApi.Workflow.Status.GetWorkflowStatus(si, wfUnitClusterPk_NonPayCalc).GetOverallStatus.ToString
			If wfStatus.XFContainsIgnoreCase("Completed") Then BRApi.Workflow.Status.SetWorkflowStatus(si, wfUnitClusterPk_NonPayCalc, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, "Auto Uncomplete", "Error Auto Uncomplete ", "Auto Uncomplete", Guid.Empty)
					
			selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			selectionChangedTaskResult.IsOK = True		
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo

		End If	
		
		'Update param for label display
		Dim dKeyVal As New Dictionary(Of String, String)
		dKeyVal.Add(param,paramValue)
		
		Return Me.SetParameter(si, globals, api, dKeyVal, SelectionChangedTaskResult)

		
	Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))	
				End Try

End Function	

				
#Region "Complete WF TGTDST CMD"
		Public Function WorkflowCompleteCMDTGTDST(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 09/09/2024 by Connor and Fronz - used on the Manage Requirements CMD.Administrative dashboard to lock non-L2 PGM wf steps
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Lock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("Target Distribution.Adj") Or sWFTargetProfileName.EndsWith("Target Distribution.Import") Or sWFTargetProfileName.EndsWith("Target Distribution.Forms") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been locked."
				
				'Lock CMD WF Steps	
					ElseIf sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("Target Distribution CMD.Adj") Or sWFTargetProfileName.EndsWith("Target Distribution CMD.Import") Or sWFTargetProfileName.EndsWith("Target Distribution CMD.Forms")Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been locked."							
				End If 
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
#End Region	

#Region "Revert Wf TGTDST CMD"
		Public Function WorkflowRevertCMDTGTDST(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
			'Created 04/02/25 by JM	
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim sWFLevel As String = args.NameValuePairs.XFGetValue("WFLevel")
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				'Unlock Sub CMD WF steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD","")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Unlock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("Target Distribution.Adj") Or sWFTargetProfileName.EndsWith("Target Distribution.Import") Or sWFTargetProfileName.EndsWith("Target Distribution.Forms") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD Workflows have been unlocked."
				
				'Unlock CMD WF Steps	
				 ElseIf sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
'					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Unlock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("Target Distribution CMD.Adj") Or sWFTargetProfileName.EndsWith("Target Distribution CMD.Import") Or sWFTargetProfileName.EndsWith("Target Distribution CMD.Forms") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD Workflows have been unlocked."							
				End If
				
				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function				
				
				
				
	#End Region	

	#Region "Revert WF SPLN"
		Public Function WorkflowRevertSPLN(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			
			Try
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				Dim DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
				
				
				Dim sAppn As String = args.NameValuePairs.XFGetValue("APPN")
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				Dim profilename As String = sEntity & " (Spend Plan)." & sAppn & " Validate"
					Dim Dashboardname As String = currDashboard.Name
					Dim sWorkflowProfile As String = curProfile.Name
'					BRApi.ErrorLog.LogMessage(si, "Profile" & sWorkflowProfile)
'					BRApi.ErrorLog.LogMessage(si, "Dashboard" & Dashboardname)
					
					'Lock WF
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, profilename, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
										
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = False	
											
		 
				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
#End Region	
	
#End Region
		
#Region "Constants and Enumerations"

		'Parallel Options
		Dim m_ParallelThreadsNoSQL As Integer = 16		
		Dim m_ParallelThreadsWithSQL As Integer = 8 	'Do not increase beyond 8 Threads, may cause deadlocks on insert
		
		'String Constants
		Public m_PlanTemplate As String = "PlanTemplate"
		Public m_PlanGlobal As String = "PlanGlobal"
		Public m_ScenarioTypeAll As String = "All"
		
		Public m_StatusAll As String = "All"
		Public m_StatusIdled As String = "Idle"		
		
		'String Messages				
		Private m_MsgWorkflowPOVNotInitialized As String = "Error: Workflow POV has not been intialized."
		Private m_MsgSettingsSaved As String = "Settings Saved"
		Private m_MsgSettingsNotSaved As String = "Settings Saved with warning: Review message and make corrections."
		Private m_MsgSettingsEndPlanPerGTStartPlanPer As String = "End Plan Period must be GREATER than Start Plan Period."
		Private m_MsgFileLoaded As String = "File [{0}] Loaded"
		Private m_MsgFileNotLoaded As String = "File could NOT be loaded, Workflows have been COMPLETED based on existing register/activity data."
		Private m_MsgCalculatePlanCompleted As String = "Plan Calculation Completed"
		Private m_MsgCalculatePlanStarted As String = "Plan Calculation Started" & VBCrLF & "[Check Task Activity To Monitor Progress]"
		Private m_MsgCalculatePlanErr As String = "Plan Calculation Error"		
		Private m_MsgCannotCalculatePlan As String = "Cannot Calculate Plan, Parent Workflow has been Completed."
		Private m_MsgDeletePlanCompleted As String = "Delete Plan Completed"
		Private m_MsgCannotDeletePlan As String = "Cannot Delete Plan, Parent Workflow has been Completed."
		Private m_MsgCannotDeleteRegister As String = "Cannot Delete Register, Workflow has been Completed."
		Private m_MsgWorkflowCompleted As String = "Plan Workflow Completed: {0}"
		Private m_MsgCannotCompleteWorkflow As String = "Workflow NOT Completed: Parent Workflow has been Completed."
		Private m_MsgCannotCompleteWorkflowValErr As String = "Workflow NOT Completed: Check Validation Errors."
		Private m_MsgCannotCompleteMultipleWorkflows As String = "Workflow NOT FULLY Completed: ({0}) planning workflow(s) are implicity locked because Parent Workflow has been Completed." & vbcrlf & "  Items Listed Below:"
		Private m_MsgWorkflowCompletedReasonButton As String = "User clicked [Complete Workflow]"
		Private m_MsgCannotRevertWorkflow As String = "Workflow NOT Reverted: Parent Workflow has been Completed."
		Private m_MsgCannotRevertMultipleWorkflows As String = "Workflow NOT FULLY Reverted: ({0}) planning workflow(s) are implicity locked because Parent Workflow has been Completed." & vbcrlf & "  Items Listed Below:"
		Private m_MsgWorkflowReverted As String = "Capital Plan Workflow Reverted: {0}"
		Private m_MsgWorkflowRevertedReasonButton As String = "User clicked [Revert Workflow]"
		Private m_MsgWorkflowRevertedReasonCalc As String = "Calculate Plan Executed [Revert Workflow]"
		Private m_MsgAutoLoadWFProfileDoesNotExist As String = "Autoload Workflow Profile Does Not Exist: {0}, {1}, {2}"
		
			
#End Region		

#Region "Private Functions"

'	Private Function GetRegisterWFClusterPks(ByVal si As SessionInfo, ByVal scenarioName As String, ByVal timeName As String) As List(Of WorkflowUnitClusterPk)
'		Try
'			Dim wfClusterPks As New List(Of WorkflowUnitClusterPk)
			
'			'Define the SQL Statement
'			Dim sql As New Text.StringBuilder					
'			sql.Append("Select Distinct WFProfileName ")

'			sql.Append("From ")           
'			sql.Append("XFW_CPP_Register ")
		
'			sql.Append("Where (")
'			sql.Append("WFScenarioName = '" & scenarioName & "' ")
'			sql.Append("And WFTimeName = '" &  timeName  & "'")
'			sql.Append(")")

'			sql.Append("Order By ")
'			sql.Append("WFProfileName ")
							
'			'Create the list of WorkflowUnitClusterPks
'			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
'				Using dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sql.ToString, False)	
'					For Each dr As DataRow In dt.rows
'						Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("WFProfileName"), scenarioName, timeName)
'						If Not wfClusterPk Is Nothing Then
'							wfClusterPks.Add(wfClusterPk)
'						End If
'					Next	
'				End Using
'			End Using
			
'			Return wfClusterPks
			
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try		
'	End Function
	
#End Region

#Region "Workflow Helpers"

	Public Function ValidateGuid(ByVal si As SessionInfo, ByVal guidString As String) As String
		Try
			Dim testGuid As Guid
			If Guid.TryParse(guidString, testGuid) Then
				Return guidString
			Else
				Return Guid.Empty.ToString	
			End If				
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function

	Public Function ValidateWorkflowPOVInitialization(ByVal si As SessionInfo, ByVal Optional throwError As Boolean = False) As Boolean
		Try
			Dim isInitialized As Boolean = True
			
			'Make sure that the user has initialize the workflow cluster, otherwise all other calls will fail
			If (si.WorkflowClusterPk.ProfileKey = Guid.Empty) Or (si.WorkflowClusterPk.ScenarioKey = SharedConstants.Unknown) Or (si.WorkflowClusterPk.TimeKey = SharedConstants.Unknown) Then
				isInitialized = False
				If throwError Then Throw New XFException(Me.m_MsgWorkflowPOVNotInitialized, Nothing)
			End If	
			
			Return isInitialized 
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
	Public Function IsCentralRegisterWFProfile(ByVal si As SessionInfo) As Boolean
		Try
			'Compare the Central loading workflow profile to the current workflow profile
			Dim gValue As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")
			Dim centralLoadingWFProfile As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, gValue,"StoredPlanRegisterProfile_CPPT")			
			Dim currentProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			If Not currentProfileInfo Is Nothing Then
				Return currentProfileInfo.Name.XFEqualsIgnoreCase(centralLoadingWFProfile)
			Else
				Return False	
			End If	
				
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function

	Public Function IsWorkspaceWFParentCertified(ByVal si As SessionInfo, ByVal wfClusterPk As WorkflowUnitClusterPk) As Boolean
		Try
			Dim IsCertified As Boolean = False
			
			'Check the certification status of the workflow parent
			Dim wfParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, wfClusterPk)
			If Not wfParent Is Nothing Then
				Dim wfParentPk As New WorkflowUnitClusterPk(wfParent.ProfileKey, wfClusterPk.ScenarioKey, wfClusterPk.TimeKey)				
				Dim wfParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfParentPk, False)	
				If Not wfParentStatus Is Nothing Then
					If wfParentStatus.IsCertified Then
						IsCertified = True
					End If
				End If
			End If	
			
			Return IsCertified
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
	Public Sub SetWorkspaceWFToInprocess(ByVal si As SessionInfo, ByVal wfClusterPk As WorkflowUnitClusterPk)
		Try				
			'Get the workflow Info object, retrieve and the Workspace Workflow task and set its status ti INPROCESS	
			Dim wfStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfClusterPk, True)
			Dim wfTask As TaskInfo =  wfStatus.GetTask(New Guid(SharedConstants.WorkflowKeys.Tasks.Workspace))
			If Not wfTask Is Nothing Then
				If wfTask.Status = WorkflowStatusTypes.Completed Then
					'Update the workspace workflow to INPROCESS
					Dim wfRegClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPk)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPk, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage(Me.m_MsgWorkflowReverted, wfRegClusterDesc), "", Me.m_MsgWorkflowRevertedReasonCalc, Guid.Empty)							
				End If
			End If
					
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try			
	End Sub

	Public Function AutoCompleteLoadAndProcessWF(ByVal si As SessionInfo, ByVal wfPlanClusterPk As WorkflowUnitClusterPk, ByVal wfImportChildSuffix As String) As Boolean
		Try		
			Dim completed As Boolean = False
			
			'If a suffix was not provided, just exit sub
			If Not String.IsNullOrWhiteSpace(wfImportChildSuffix) Then 							
				'Get the workflow unit PK and process the workflow	
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, wfPlanClusterPk.ProfileKey)
				Dim wfProfileParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, wfProfile.ParentProfileKey)
				Dim wfImportChildName As String = wfProfileParent.Name & "." & wfImportChildSuffix
				Dim scenarioName As String = ScenarioDimHelper.GetNameFromId(si, wfPlanClusterPk.ScenarioKey)
				Dim timeName As String =  TimeDimHelper.GetNameFromId(wfPlanClusterPk.TimeKey)											
				Dim wfChildClusterPk As WorkflowUnitClusterPk = BRAPi.Workflow.General.GetWorkflowUnitClusterPk(si, wfImportChildName, scenarioName, timeName)
			
				'Execute IMPORT-VALIDATE-LOAD-PROCESS workflow
				If Not wfChildClusterPk Is Nothing Then
					Dim impProcessInfo As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfChildClusterPk, "", Nothing, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, True)
					If impProcessInfo.Status = WorkflowStatusTypes.Completed Then
						'Validate Transformation (Mapping)
						Dim valTranProcessInfo As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfChildClusterPk, True)
						If valTranProcessInfo.Status = WorkflowStatusTypes.Completed Then
							'Validate Intersections
							Dim valIntersectProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfChildClusterPk, True)
							If valTranProcessInfo.Status = WorkflowStatusTypes.Completed Then
								'Load the cube
								Dim lcProcessInfo = BRApi.Import.Process.LoadCube(si, wfChildClusterPk)
								If lcProcessInfo.Status = WorkflowStatusTypes.Completed Then
									BRApi.DataQuality.Process.ExecuteProcessCube(si, wfChildClusterPk, StepClassificationTypes.ProcessCube, False)
									completed = True
								End If	
							End If
						End If									
					End If	
				Else
					'BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(Me.m_MsgAutoLoadWFProfileDoesNotExist, wfImportChildName, scenarioName, timeName))		
				End If			
			End If
			
			Return completed
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try			
	End Function
	
	Public Function AutoRevertLoadAndProcessWF(ByVal si As SessionInfo, ByVal wfPlanClusterPk As WorkflowUnitClusterPk, ByVal wfImportChildSuffix As String) As Boolean
		Try		
			Dim completed As Boolean = False
			
			'If a suffix was not provided, just exit sub
			If Not String.IsNullOrWhiteSpace(wfImportChildSuffix) Then 							
				'Get the workflow unit PK and process the workflow	
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, wfPlanClusterPk.ProfileKey)
				Dim wfProfileParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, wfProfile.ParentProfileKey)
				Dim wfImportChildName As String = wfProfileParent.Name & "." & wfImportChildSuffix
				Dim scenarioName As String = ScenarioDimHelper.GetNameFromId(si, wfPlanClusterPk.ScenarioKey)
				Dim timeName As String =  TimeDimHelper.GetNameFromId(wfPlanClusterPk.TimeKey)											
				Dim wfChildClusterPk As WorkflowUnitClusterPk = BRAPi.Workflow.General.GetWorkflowUnitClusterPk(si, wfImportChildName, scenarioName, timeName)
			
				'Execute IMPORT-VALIDATE-LOAD-PROCESS workflow
				If Not wfChildClusterPk Is Nothing Then
					Dim impProcessInfo As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfChildClusterPk, "", Nothing, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, True)
					If impProcessInfo.Status = WorkflowStatusTypes.Completed Then
						'Set the AutoLoad workflow back to INPROCESS for the Import Step
						Dim wfChildClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfChildClusterPk)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfChildClusterPk, StepClassificationTypes.DataLoadTransform, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage(Me.m_MsgWorkflowReverted, wfChildClusterDesc), "", Me.m_MsgWorkflowRevertedReasonButton, Guid.Empty)						
					End If	
					completed = True
				Else
					'BRApi.ErrorLog.LogMessage(si, StringHelper.FormatMessage(Me.m_MsgAutoLoadWFProfileDoesNotExist, wfImportChildName, scenarioName, timeName))			
				End If			
			End If
			
			Return completed
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try			
	End Function
	
	Public Function ValidateWFStatusBeforeRegisterImport(ByVal si As SessionInfo) As Boolean
		Try
			'Check the workflow status before allowing a register file upload
			Dim canLoadFile As Boolean = True	

			'Get Workflow Cluster Information
			Dim scenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
			Dim timeName As String = TimeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)

			'CHECK WORKFLOW STATUS OF WORKFLOWS
			'----------------------------------------------------------
			If canLoadFile Then
				Dim wfStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, False)												
				If (wfStatus.AllTasksCompleted) Then
					canLoadFile = False
				End If	
			End If
		
			Return canLoadFile	
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
	#Region "SetParameter: Set Parameter"
		'Set a parameter with passed in value using selectionChangedTaskResult
		Public Function SetParameter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal dKeyVal As Dictionary(Of String, String), Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing )As Object				
				If selectionChangedTaskResult Is Nothing Then
					selectionChangedTaskResult = New XFSelectionChangedTaskResult()
				End If
				
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True			
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
				
				For Each KeyVal As KeyValuePair(Of String, String) In dKeyVal
					selectionChangedTaskResult.ModifiedCustomSubstVars.Remove(KeyVal.Key)
					selectionChangedTaskResult.ModifiedCustomSubstVars.Add(KeyVal.Key, KeyVal.Value)
				
					selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add(KeyVal.Key, KeyVal.Value)			
				Next
				
				Return selectionChangedTaskResult
		End Function
#End Region	
	
#End Region	

End Class
	
End Namespace