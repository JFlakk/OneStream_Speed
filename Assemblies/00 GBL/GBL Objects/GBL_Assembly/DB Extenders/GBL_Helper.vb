Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Data.SqlClient
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports System.Text.RegularExpressions
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports System.IO.Compression
Imports OneStream.Finance.Common
Imports Workspace.CMD_UFR.CMD_UFR_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.GBL_Helper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				
				'Updated 09/16
				If args.FunctionName.XFEqualsIgnoreCase("Check_WF_Complete_Lock") Then
					If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") _
						And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) _ 
						Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
						Return "Locked"
						'Throw New Exception("INFO:"  & vbCRLF & vbCRLF & "NOT allowed to execute step while Workflow is marked as Complete or Locked. Select Workflow Revert button or Unlock if you need to load data." & vbCRLF)
					Else 
						Return "Unlocked"
					End If
				End If	

#Region "WorkflowCompleteCMDTGT"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteCMDTGT")) 
					Return Me.WorkflowCompleteCMDTGT(si, globals, api, args)	
				End If
#End Region 'Updated 09/17		

#Region "WorkflowRevertCMDTGT"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertCMDTGT"))
					Return Me.WorkflowRevertCMDTGT(si, globals, api, args)	
				End If
#End Region	 'Updated 09/17	

#Region "WorkflowComplete"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowComplete"))				
					Return Me.WorkflowComplete(si, globals, api, args)	
				End If
#End Region		'Updated 09/23/2025

#Region "WorkflowRevert"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevert"))
					Return Me.WorkflowRevert(si, globals, api, args)	
				End If	
#End Region		'Updated 09/23/2025		

#Region "WorkflowCompleteSPLN"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowCompleteSPLN"))				
					Return Me.WorkflowCompleteSPLN(si, globals, api, args)	
				End If
#End Region		'Updated 09/23/2025

#Region "WorkflowRevertSPLN"
				If (args.FunctionName.XFEqualsIgnoreCase("WorkflowRevertSPLN"))				
					Return Me.WorkflowRevertSPLN(si, globals, api, args)	
				End If
#End Region		'Updated 09/23/2025

#Region "ARWUTL_AddRowInit"
				If args.FunctionName.XFEqualsIgnoreCase("ARWUTL_AddRowInit") Then
					Return Me.ARWUTL_AddRowInit(si, args)
				End If
#End Region		 'Updated 09/17			

#Region "ResetParameters"
				If args.FunctionName.XFEqualsIgnoreCase("ResetParameters") Then
					Return Me.ResetParameters(si, globals, api, args)
				End If
#End Region	

#Region "Send Email"
'				If args.FunctionName.XFEqualsIgnoreCase("SendStatusChangeEmail") Then
'					Return Me.SendStatusChangeEmail(si, globals, api, args)
'				End If
#End Region	


			Return Nothing	
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function			
		
#Region "WorkflowCompleteCMDTGT"
		Public Function WorkflowCompleteCMDTGT(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
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
#Region "Lock Sub CMD"				
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					'Lock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
				
					'Lock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					
					For Each WFTargetProfile In sWFTargetProfileInfo			

'Brapi.ErrorLog.LogMessage(si,"WFTargetProfile.Name: " & WFTargetProfile.Name)								
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						'If sWFTargetProfileName.EndsWith("Sub CMD Dist Review (CMD TGT).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been locked."
#End Region		
				'Lock CMD WF Steps	
				ElseIf sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
#Region "Lock CMD"					
					Dim sWorkflowProfile As String = curProfile.Name
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
'For Each WFTargetProfile In sWFTargetProfileInfo
'brapi.ErrorLog.LogMessage(si,"WFTargetProfile.Name: " & WFTargetProfile.Name)		
'Next
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("CMD Dist Review (CMD TGT).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been locked."	
#End Region
				End If 
			
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
#End Region	'Updated 09/17		

#Region "WorkflowRevertCMDTGT"
		Public Function WorkflowRevertCMDTGT(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
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
#Region "Unlock Sub CMD"					
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))
					
					'Unlock Parent Sub CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					
					
					'Unlock Chidlren Sub CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
'For Each WFTargetProfile In sWFTargetProfileInfo
'brapi.ErrorLog.LogMessage(si,"WFTargetProfile.Name: " & WFTargetProfile.Name)		
'Next							
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						'If sWFTargetProfileName.EndsWith("CMD Dist Review (CMD TGT).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been unlocked."
#End Region			
				'Unlock CMD WF Steps	
				 ElseIf sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
#Region "Unlock CMD"									 
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
						'If sWFTargetProfileName.EndsWith("CMD Dist Review (CMD TGT).Import") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been unlocked."			
#End Region							
				End If
				
				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function				
	#End Region	'Updated 09/17		
	
#Region "WorkflowCompletePGM"
		Public Function WorkflowComplete(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
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
'brapi.ErrorLog.LogMessage(si, "Curprofile: " & curProfile.Name & " | Replace:  " & curProfile.Name.Replace(" CMD",""))				


				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")
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
						If sWFTargetProfileName.EndsWith("PGM).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been locked."
#Region "Lock CMD"				
				'LOCK CMD WF STEPS
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					'Grab the top WFprofile
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))	
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("PGM).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been locked."							
				End If
#End Region				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
	
#End Region 'Updated 09/23/2025

#Region "WorkflowRevertPGM"
		Public Function WorkflowRevert(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult
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
					
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")					
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))		
					
					'Grab Parent WF pk
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)

					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					

					'Unlock Chidlren Sub CMD WF steps
					'List Of wf steps Not inclusive Of parent
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("PGM).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been unlocked."
#Region "Unlock CMD"			
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
						If sWFTargetProfileName.EndsWith("PGM).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been unlocked."							
				End If
#End Region

				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
#End Region	'Updated 09/23/2025

#Region "WorkflowCompleteSPLN"
		Public Function WorkflowCompleteSPLN(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult

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
'brapi.ErrorLog.LogMessage(si, "Curprofile: " & curProfile.Name & " | Replace:  " & curProfile.Name.Replace(" CMD",""))				


				'Lock Sub CMD WF Steps
				If sWFLevel.XFEqualsIgnoreCase("SubCMD") Then
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")
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
						If sWFTargetProfileName.EndsWith("SPLN).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been locked."
#Region "Lock CMD"				
				'LOCK CMD WF STEPS
				Else If sWFLevel.XFEqualsIgnoreCase("CMDHQ") Then
					Dim sWorkflowProfile As String = curProfile.Name
					'Grab the top WFprofile
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))	
					'Lock Parent CMD WF step
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
					'Lock Chidlren CMD WF steps
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("SPLN).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been locked."							
				End If
#End Region				
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
	
#End Region 'Updated 09/23/2025

#Region "WorkflowRevertSPLN"
		Public Function WorkflowRevertSPLN(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult

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
					Dim sWorkflowProfile As String = curProfile.Name.Replace(" CMD"," Sub CMD")
					sWorkflowProfile = sWorkflowProfile.substring(0, sWorkflowProfile.IndexOf("."))		
					
					'Grab Parent WF pk
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWorkflowProfile, sScenario, sTime)

					'Variable used to format message 
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)					

					'Unlock Chidlren Sub CMD WF steps
					'List Of wf steps Not inclusive Of parent
					Dim sWFTargetProfileInfo As List (Of workflowprofileinfo) = brapi.Workflow.Metadata.GetRelatives(si,brapi.Workflow.General.GetWorkflowUnitClusterPk(si,sWorkflowProfile,sScenario,sTime),WorkflowProfileRelativeTypes.Descendants,workflowprofiletypes.AllProfiles).OrderBy(Function(x)  x.Name).reverse.ToList()
					
					For Each WFTargetProfile In sWFTargetProfileInfo
						Dim sWFTargetProfileName As String	= WFTargetProfile.Name
						If sWFTargetProfileName.EndsWith("SPLN).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "Sub-CMD workflows have been unlocked."
#Region "Unlock CMD"			
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
						If sWFTargetProfileName.EndsWith("SPLN).Adj") Then Continue For
						wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sWFTargetProfileName, sScenario, sTime)
						BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.InProcess, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)										
					Next					
					
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = "CMD workflows have been unlocked."							
				End If
#End Region

				Return selectionChangedTaskResult				
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try				
		End Function
#End Region	'Updated 09/23/2025
	
#Region "ARWUTL_AddRowInit"

		Private Function ARWUTL_AddRowInit(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As Object
			Try
'BRAPI.ErrorLog.LogMessage(si,"ARWUTL_AddRowInit:")				
				'---------------------------------------------------------------------------------------------------
				' Created: 2022-Jun-2 - AK
				'
				' Purpose: setup the ADD ROW dialog box
				'
				' Invocation: MAIN() args.FunctionName.XFEqualsIgnoreCase("AddRow_Init")
				'
				' Usage Instructions from a button component:
				'	{Global_SolutionHelper}{AddRow_Init}{ShowCbx=U1#U2#U3#U4#, CbxLabels=U1:APPNxx ; U2:MDEPxx ; U3:APEx ; U4:DollarTypex  , Cube=|WFCube|, Entity=|!LR_AddRow_Entity!|, Scenario=|!LR_AddRow_Scenario!|, Time=|WFYear|M12, Cons=|!LR_AddRow_Cons!|, View=|!LR_AddRow_View!|, Account=|!LR_AddRow_Account!|, IC=|!LR_AddRow_IC!|, Flow=|!LR_AddRow_Flow!|, Origin=|!LR_AddRow_Origin!|, U1=|!LR_AddRow_U1!|, U2=|!LR_AddRow_U2!|, U3=|!LR_AddRow_U3!|, U4=|!LR_AddRow_U4!|, U5=|!LR_AddRow_U5!|, U6=|!LR_AddRow_U6!|, U7=|!LR_AddRow_U7!|, U8=|!LR_AddRow_U8!|}
				'      Syntax:
				'           CbxLabels = <dim name> : <label> ;
				'
				' Where:
				'     - the LR_AddRow_???? prompts are link reporting parameters defined in the cubeview that the ADD ROW button is coupled with
				'
				'Modifications:
				' 2023-Oct-15 AK: added code to support assigning cbx label via parameters
				' 2024-May-29 AK: added code to allow lists to be controled on INIT
				' 2024-May-29 AK: added code to allow parent members
				' 2024-Jul-15 AK: added code to ShowLbl
				' 2024-Jul-22 AK: added code to build reset filters based on ShowCbx

				'--------------------------------------------------------------- 
				'Verify Workflow is NOT Complete Or Locked
				'Updated 09/25/2024 by Fronz - added a condition to check the REQ WF profiles for being marked as "Completed" by Command HQ
				'---------------------------------------------------------------
				If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked) And BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.XFContainsIgnoreCase("Requirement") Then
					Throw New Exception("Notice: No updates are allowed. Workflow was marked ""Complete"" by Command HQ.")
					
				Else If BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then 
					Throw New Exception("INFO:"  & vbCRLF & vbCRLF & "NOT allowed to execute step while Workflow is marked as Complete or Locked. Select Workflow Revert button or Unlock if you need to load data." & vbCRLF)
				End If
				'----------------------------------------------------------------

				Dim objDictionary As New Dictionary(Of String, String)
				
'		**********************************************************************************************************************************************
#Region "		Show MSG"
'		**********************************************************************************************************************************************
'Added 02/27/2025 by Fronz and Kyson
'Note: the defualt message is specifically used in the PGM module for the "Adjust Funding Line" btn
				Dim sMessage As String = args.NameValuePairs.XFGetValue("LblMsg")
				If sMessage = "" Then
					sMessage = "User must first select a fund code before adjusting a funding line."
				End If
'BRAPI.ErrorLog.LogMessage(si,"sMessage " & sMessage)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_LblMsg", sMessage)

'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************
				

'		**********************************************************************************************************************************************
#Region "		Show BTN"
'		**********************************************************************************************************************************************
				Dim sShowHideBtn_Add As String = "False"
				Dim sShowHideBtn_Remove As String = "False"
				Dim sShowHideBtn_Reclass As String = "False"
				
				Dim sShowButton As String = args.NameValuePairs.XFGetValue("ShowButton")
								
				If sShowButton.XFContainsIgnoreCase("Add") Then sShowHideBtn_Add = "True"
				If sShowButton.XFContainsIgnoreCase("Remove") Then sShowHideBtn_Remove = "True"
				If sShowButton.XFContainsIgnoreCase("Reclass") Then sShowHideBtn_Reclass = "True"

'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sShowHideBtn_Add=" & sShowHideBtn_Add & "   sShowHideBtn_Remove=" & sShowHideBtn_Remove & "   sShowHideBtn_Reclass=" & sShowHideBtn_Reclass)
				
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ShowButtonAdd", sShowHideBtn_Add)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ShowButtonRemove", sShowHideBtn_Remove)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ShowButtonReclass", sShowHideBtn_Reclass)

'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************
				

'		**********************************************************************************************************************************************
#Region "		Reclass"
'		**********************************************************************************************************************************************
				Dim sReclassDims As String = ""
				If args.NameValuePairs.ContainsKey("ReclassDims") Then sReclassDims = args.NameValuePairs.XFGetValue("ReclassDims")
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sReclassDims=" & sReclassDims)
				
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ReclassDims", sReclassDims)

				Dim sReclassNumYears As String = ""
				If args.NameValuePairs.ContainsKey("ReclassNumYears") Then sReclassNumYears = args.NameValuePairs.XFGetValue("ReclassNumYears")
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sReclassNumYears=" & sReclassNumYears)
				
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ReclassNumYears", sReclassNumYears)

				Dim sReclassNumMonths As String = ""
				If args.NameValuePairs.ContainsKey("ReclassNumMonths") Then sReclassNumMonths = args.NameValuePairs.XFGetValue("ReclassNumMonths")
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sReclassNumMonths=" & sReclassNumMonths)
				
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ReclassNumMonths", sReclassNumMonths)
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************

				
'		**********************************************************************************************************************************************
#Region "		Show CBX"
'		**********************************************************************************************************************************************
				Dim sShowCbx As String = args.NameValuePairs.XFGetValue("ShowCbx")
'BRAPI.ErrorLog.LogMessage(si,"aaaGPX_ARWUTL_AddRow_Init: sShowCbx=" & sShowCbx)
				
				'----- set default -----
				Dim sShowHideCbx_Cubex As String = "False"

				Dim sShowHideCbx_Scenariox As String = "False"
				Dim sShowHideCbx_Timex As String = "False"
				Dim sShowHideCbx_Entityx As String = "False"
				Dim sShowHideCbx_Consx As String = "False"
				Dim sShowHideCbx_Viewx As String = "False"

				Dim sShowHideCbx_Accountx As String = "False"
				Dim sShowHideCbx_ICx As String = "False"
				Dim sShowHideCbx_Flowx As String = "False"
				Dim sShowHideCbx_Originx As String = "False"
				
				Dim sShowHideCbx_U1x As String = "False"
				Dim sShowHideCbx_U2x As String = "False"
				Dim sShowHideCbx_U3x As String = "False"
				Dim sShowHideCbx_U4x As String = "False"
				Dim sShowHideCbx_U5x As String = "False"
				Dim sShowHideCbx_U6x As String = "False"
				Dim sShowHideCbx_U7x As String = "False"
				Dim sShowHideCbx_U8x As String = "False"

				'----- show combo boxes defined in parameter -----
				If sShowCbx.XFContainsIgnoreCase("Cbx#") Then sShowHideCbx_Cubex = "True"
				
				If sShowCbx.XFContainsIgnoreCase("Sx#") Then sShowHideCbx_Scenariox = "True"
				If sShowCbx.XFContainsIgnoreCase("Tx#") Then sShowHideCbx_Timex = "True"
				If sShowCbx.XFContainsIgnoreCase("Ex#") Then sShowHideCbx_Entityx = "True"
				If sShowCbx.XFContainsIgnoreCase("Cx#") Then sShowHideCbx_Consx = "True"
				If sShowCbx.XFContainsIgnoreCase("Vx#") Then sShowHideCbx_Viewx = "True"

				If sShowCbx.XFContainsIgnoreCase("Ax#") Then sShowHideCbx_Accountx = "True"
				If sShowCbx.XFContainsIgnoreCase("Ix#") Then sShowHideCbx_ICx = "True"
				If sShowCbx.XFContainsIgnoreCase("Fx#") Then sShowHideCbx_Flowx = "True"
				If sShowCbx.XFContainsIgnoreCase("Ox#") Then sShowHideCbx_Originx = "True"					
				
				If sShowCbx.XFContainsIgnoreCase("U1x#") Then sShowHideCbx_U1x = "True"
				If sShowCbx.XFContainsIgnoreCase("U2x#") Then sShowHideCbx_U2x = "True"
				If sShowCbx.XFContainsIgnoreCase("U3x#") Then sShowHideCbx_U3x = "True"
				If sShowCbx.XFContainsIgnoreCase("U4x#") Then sShowHideCbx_U4x = "True"
				If sShowCbx.XFContainsIgnoreCase("U5x#") Then sShowHideCbx_U5x = "True"
				If sShowCbx.XFContainsIgnoreCase("U6x#") Then sShowHideCbx_U6x = "True"
				If sShowCbx.XFContainsIgnoreCase("U7x#") Then sShowHideCbx_U7x = "True"
				If sShowCbx.XFContainsIgnoreCase("U8x#") Then sShowHideCbx_U8x = "True"					
					
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_ShowCbx", sShowHideCbx_Cubex)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_ShowCbx", sShowHideCbx_Scenariox)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_ShowCbx", sShowHideCbx_Timex)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_ShowCbx", sShowHideCbx_Entityx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_ShowCbx", sShowHideCbx_Consx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_ShowCbx", sShowHideCbx_Viewx)


				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_ShowCbx", sShowHideCbx_Accountx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_ShowCbx", sShowHideCbx_ICx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_ShowCbx", sShowHideCbx_Flowx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_ShowCbx", sShowHideCbx_Originx)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_ShowCbx", sShowHideCbx_U1x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_ShowCbx", sShowHideCbx_U2x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_ShowCbx", sShowHideCbx_U3x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_ShowCbx", sShowHideCbx_U4x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_ShowCbx", sShowHideCbx_U5x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_ShowCbx", sShowHideCbx_U6x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_ShowCbx", sShowHideCbx_U7x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_ShowCbx", sShowHideCbx_U8x)



				'----- set default -----	
				Dim sShowHideCbx_Cube As String = "False"

				Dim sShowHideCbx_Scenario As String = "False"
				Dim sShowHideCbx_Time As String = "False"
				Dim sShowHideCbx_Entity As String = "False"
				Dim sShowHideCbx_Cons As String = "False"
				Dim sShowHideCbx_View As String = "False"

				Dim sShowHideCbx_Account As String = "False"
				Dim sShowHideCbx_IC As String = "False"
				Dim sShowHideCbx_Flow As String = "False"
				Dim sShowHideCbx_Origin As String = "False"
				
				Dim sShowHideCbx_U1 As String = "False"
				Dim sShowHideCbx_U2 As String = "False"
				Dim sShowHideCbx_U3 As String = "False"
				Dim sShowHideCbx_U4 As String = "False"
				Dim sShowHideCbx_U5 As String = "False"
				Dim sShowHideCbx_U6 As String = "False"
				Dim sShowHideCbx_U7 As String = "False"
				Dim sShowHideCbx_U8 As String = "False"					

				'----- show combo boxes defined in parameter -----
				If sShowCbx.XFContainsIgnoreCase("Cb#") Then sShowHideCbx_Cube = "True"
				
				If sShowCbx.XFContainsIgnoreCase("S#") Then sShowHideCbx_Scenario = "True"
				If sShowCbx.XFContainsIgnoreCase("T#") Then sShowHideCbx_Time = "True"
				If sShowCbx.XFContainsIgnoreCase("E#") Then sShowHideCbx_Entity = "True"
				If sShowCbx.XFContainsIgnoreCase("C#") Then sShowHideCbx_Cons = "True"
				If sShowCbx.XFContainsIgnoreCase("V#") Then sShowHideCbx_View = "True"

				If sShowCbx.XFContainsIgnoreCase("A#") Then sShowHideCbx_Account = "True"
				If sShowCbx.XFContainsIgnoreCase("I#") Then sShowHideCbx_IC = "True"
				If sShowCbx.XFContainsIgnoreCase("F#") Then sShowHideCbx_Flow = "True"
				If sShowCbx.XFContainsIgnoreCase("O#") Then sShowHideCbx_Origin = "True"

					
				If sShowCbx.XFContainsIgnoreCase("U1#") Then sShowHideCbx_U1 = "True"
				If sShowCbx.XFContainsIgnoreCase("U2#") Then sShowHideCbx_U2 = "True"
				If sShowCbx.XFContainsIgnoreCase("U3#") Then sShowHideCbx_U3 = "True"
				If sShowCbx.XFContainsIgnoreCase("U4#") Then sShowHideCbx_U4 = "True"
				If sShowCbx.XFContainsIgnoreCase("U5#") Then sShowHideCbx_U5 = "True"
				If sShowCbx.XFContainsIgnoreCase("U6#") Then sShowHideCbx_U6 = "True"
				If sShowCbx.XFContainsIgnoreCase("U7#") Then sShowHideCbx_U7 = "True"
				If sShowCbx.XFContainsIgnoreCase("U8#") Then sShowHideCbx_U8 = "True"


				'----- store state to parameters -----	
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_ShowCbx", sShowHideCbx_Cube)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_ShowCbx", sShowHideCbx_Scenario)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_ShowCbx", sShowHideCbx_Time)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_ShowCbx", sShowHideCbx_Entity)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_ShowCbx", sShowHideCbx_Cons)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_ShowCbx", sShowHideCbx_View)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_ShowCbx", sShowHideCbx_Account)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_ShowCbx", sShowHideCbx_IC)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_ShowCbx", sShowHideCbx_Flow)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_ShowCbx", sShowHideCbx_Origin)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_ShowCbx", sShowHideCbx_U1)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_ShowCbx", sShowHideCbx_U2)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_ShowCbx", sShowHideCbx_U3)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_ShowCbx", sShowHideCbx_U4)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_ShowCbx", sShowHideCbx_U5)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_ShowCbx", sShowHideCbx_U6)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_ShowCbx", sShowHideCbx_U7)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_ShowCbx", sShowHideCbx_U8)	
				
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


'		**********************************************************************************************************************************************
#Region "		Show LBL"
'		**********************************************************************************************************************************************
				Dim sShowLbl As String = args.NameValuePairs.XFGetValue("ShowLbl")
'BRAPI.ErrorLog.LogMessage(si,"aaaGPX_ARWUTL_AddRow_Init: sShowLbl=" & sShowLbl)
				
				'----- set default -----	
				Dim sShowHideLbl_Cube As String = "False"

				Dim sShowHideLbl_Scenario As String = "False"
				Dim sShowHideLbl_Time As String = "False"
				Dim sShowHideLbl_Entity As String = "False"
				Dim sShowHideLbl_Cons As String = "False"
				Dim sShowHideLbl_View As String = "False"

				Dim sShowHideLbl_Account As String = "False"
				Dim sShowHideLbl_IC As String = "False"
				Dim sShowHideLbl_Flow As String = "False"
				Dim sShowHideLbl_Origin As String = "False"
				
				Dim sShowHideLbl_U1 As String = "False"
				Dim sShowHideLbl_U2 As String = "False"
				Dim sShowHideLbl_U3 As String = "False"
				Dim sShowHideLbl_U4 As String = "False"
				Dim sShowHideLbl_U5 As String = "False"
				Dim sShowHideLbl_U6 As String = "False"
				Dim sShowHideLbl_U7 As String = "False"
				Dim sShowHideLbl_U8 As String = "False"					

				'----- show combo boxes defined in parameter -----
				If sShowLbl.XFContainsIgnoreCase("Cb#") Then sShowHideLbl_Cube = "True"
				
				If sShowLbl.XFContainsIgnoreCase("S#") Then sShowHideLbl_Scenario = "True"
				If sShowLbl.XFContainsIgnoreCase("T#") Then sShowHideLbl_Time = "True"
				If sShowLbl.XFContainsIgnoreCase("E#") Then sShowHideLbl_Entity = "True"
				If sShowLbl.XFContainsIgnoreCase("C#") Then sShowHideLbl_Cons = "True"
				If sShowLbl.XFContainsIgnoreCase("V#") Then sShowHideLbl_View = "True"

				If sShowLbl.XFContainsIgnoreCase("A#") Then sShowHideLbl_Account = "True"
				If sShowLbl.XFContainsIgnoreCase("I#") Then sShowHideLbl_IC = "True"
				If sShowLbl.XFContainsIgnoreCase("F#") Then sShowHideLbl_Flow = "True"
				If sShowLbl.XFContainsIgnoreCase("O#") Then sShowHideLbl_Origin = "True"

					
				If sShowLbl.XFContainsIgnoreCase("U1#") Then sShowHideLbl_U1 = "True"
				If sShowLbl.XFContainsIgnoreCase("U2#") Then sShowHideLbl_U2 = "True"
				If sShowLbl.XFContainsIgnoreCase("U3#") Then sShowHideLbl_U3 = "True"
				If sShowLbl.XFContainsIgnoreCase("U4#") Then sShowHideLbl_U4 = "True"
				If sShowLbl.XFContainsIgnoreCase("U5#") Then sShowHideLbl_U5 = "True"
				If sShowLbl.XFContainsIgnoreCase("U6#") Then sShowHideLbl_U6 = "True"
				If sShowLbl.XFContainsIgnoreCase("U7#") Then sShowHideLbl_U7 = "True"
				If sShowLbl.XFContainsIgnoreCase("U8#") Then sShowHideLbl_U8 = "True"
				'----- store state to parameters -----	
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_ShowLbl", sShowHideLbl_Cube)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_ShowLbl", sShowHideLbl_Scenario)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_ShowLbl", sShowHideLbl_Time)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_ShowLbl", sShowHideLbl_Entity)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_ShowLbl", sShowHideLbl_Cons)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_ShowLbl", sShowHideLbl_View)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_ShowLbl", sShowHideLbl_Account)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_ShowLbl", sShowHideLbl_IC)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_ShowLbl", sShowHideLbl_Flow)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_ShowLbl", sShowHideLbl_Origin)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_ShowLbl", sShowHideLbl_U1)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_ShowLbl", sShowHideLbl_U2)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_ShowLbl", sShowHideLbl_U3)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_ShowLbl", sShowHideLbl_U4)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_ShowLbl", sShowHideLbl_U5)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_ShowLbl", sShowHideLbl_U6)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_ShowLbl", sShowHideLbl_U7)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_ShowLbl", sShowHideLbl_U8)
				
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


'		**********************************************************************************************************************************************
#Region "		Headers"
'		**********************************************************************************************************************************************		
				'------ assign cbx labels via parameters -----
				Dim sLabelList As String = ""
				If args.NameValuePairs.ContainsKey("LabelList") Then sLabelList = args.NameValuePairs.XFGetValue("LabelList")
				Dim sListOFLabels() As String = sLabelList.Split(";")
'BRAPI.ErrorLog.LogMessage(si, "sLabelList=" & sLabelList)				
				If sShowHideCbx_Cubex.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Cubex:") Then						
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Cube.XFEqualsIgnoreCase("true") Or sShowHideLbl_Cube.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Cube:") Then						
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Scenariox.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Scenariox:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_Scenario.XFEqualsIgnoreCase("true") Or sShowHideLbl_Scenario.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Scenario:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_Timex.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Timex:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				If sShowHideCbx_Time.XFEqualsIgnoreCase("true") Or sShowHideLbl_Time.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Time:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_Entityx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Entityx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				If sShowHideCbx_Entity.XFEqualsIgnoreCase("true") Or sShowHideLbl_Entity.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Entity:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If		
				If sShowHideCbx_Consx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Consx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_Cons.XFEqualsIgnoreCase("true") Or sShowHideLbl_Cons.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Cons:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Viewx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Viewx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_View.XFEqualsIgnoreCase("true") Or sShowHideLbl_View.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("View:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If		
				If sShowHideCbx_Accountx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Accountx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Account.XFEqualsIgnoreCase("true") Or sShowHideLbl_Account.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Account:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_ICx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("ICx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_IC.XFEqualsIgnoreCase("true") Or sShowHideLbl_IC.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("IC:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Flowx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Flowx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Flow.XFEqualsIgnoreCase("true") Or sShowHideLbl_Flow.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Flow:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Originx.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Originx:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_Origin.XFEqualsIgnoreCase("true") Or sShowHideLbl_Origin.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("Origin:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_U1x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U1x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_U1.XFEqualsIgnoreCase("true") Or sShowHideLbl_U1.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U1:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U2x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U2x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U2.XFEqualsIgnoreCase("true") Or sShowHideLbl_U2.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U2:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U3x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U3x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U3.XFEqualsIgnoreCase("true") Or sShowHideLbl_U3.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U3:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U4x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U4x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U4.XFEqualsIgnoreCase("true") Or sShowHideLbl_U4.XFEqualsIgnoreCase("true") Then								
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U4:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_U5x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U5x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U5.XFEqualsIgnoreCase("true") Or sShowHideLbl_U5.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U5:") Then						
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
				If sShowHideCbx_U6x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U6x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U6.XFEqualsIgnoreCase("true") Or sShowHideLbl_U6.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U6:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U7x.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U7x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U7.XFEqualsIgnoreCase("true") Or sShowHideLbl_U7.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U7:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U8x Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U8x:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If	
				
				If sShowHideCbx_U8.XFEqualsIgnoreCase("true") Or sShowHideLbl_U8.XFEqualsIgnoreCase("true") Then
					For Each sLabel As String In sListOFLabels
						If sLabel.XFContainsIgnoreCase("U8:") Then
							objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_Label", sLabel.Split(":")(1))
							Exit For
						End If	
					Next	
				End If
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


'		**********************************************************************************************************************************************
#Region "		CBX value"
'		**********************************************************************************************************************************************
				Dim sCubex As String = args.NameValuePairs.XFGetValue("Cubex")
				Dim sScenariox As String = args.NameValuePairs.XFGetValue("Scenariox")
				Dim sTimex As String = args.NameValuePairs.XFGetValue("Timex")
				Dim sEntityx As String = args.NameValuePairs.XFGetValue("Entityx")
				Dim sConsx As String = args.NameValuePairs.XFGetValue("Consx")
				Dim sViewx As String = args.NameValuePairs.XFGetValue("Viewx")

				Dim sAccountx As String = args.NameValuePairs.XFGetValue("Accountx")
				Dim sICx As String = args.NameValuePairs.XFGetValue("ICx")							
				Dim sFlowx As String = args.NameValuePairs.XFGetValue("Flowx")
				Dim sOriginx As String = args.NameValuePairs.XFGetValue("Originx")	
				
				'Moved U1 up from line 3262 to be used to get parrent appn for cbx
				Dim sU1 As String = args.NameValuePairs.XFGetValue("U1")
				Dim sU1x As String =""
				
				
				Dim wfScenarioTypeNameU1 As String = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Name	
					
				If	wfScenarioTypeNameU1.XFEqualsIgnoreCase("Budget") 
					Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id, sU1)
					Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
					sU1x = sParentAppn
					
				Else 
					sU1x = args.NameValuePairs.XFGetValue("U1x")
			End If 
				
			
				Dim sU2x As String = args.NameValuePairs.XFGetValue("U2x")
				Dim sU3x As String = args.NameValuePairs.XFGetValue("U3x")
				Dim sU4x As String = args.NameValuePairs.XFGetValue("U4x")
				Dim sU5x As String = args.NameValuePairs.XFGetValue("U5x")
				Dim sU6x As String = args.NameValuePairs.XFGetValue("U6x")
				Dim sU7x As String = args.NameValuePairs.XFGetValue("U7x")
				Dim sU8x As String = args.NameValuePairs.XFGetValue("U8x")
				
	'Brapi.ErrorLog.LogMessage(si, "sU1x" & sU1x)	
'Dim sMbrScriptx As String = "Cbx#" & sCubex
'sMbrScriptx &= ":Sx#" & sScenariox
'sMbrScriptx &= ":Tx#" & sTimex
'sMbrScriptx &= ":Ex#" & sEntityx
'sMbrScriptx &= ":Cx#" & sConsx
'sMbrScriptx &= ":Vx#" & sViewx

'sMbrScriptx &= ":Ax#" & sAccountx
'sMbrScriptx &= ":Ix#" & sICx
'sMbrScriptx &= ":Fx#" & sFlowx
'sMbrScriptx &= ":Ox#" & sOriginx

'sMbrScriptx &= ":U1x#" & sU1x
'sMbrScriptx &= ":U2x#" & sU2x
'sMbrScriptx &= ":U3x#" & sU3x
'sMbrScriptx &= ":U4x#" & sU4x
'sMbrScriptx &= ":U5x#" & sU5x
'sMbrScriptx &= ":U6x#" & sU6x
'sMbrScriptx &= ":U7x#" & sU7x
'sMbrScriptx &= ":U8x#" & sU8x
'BRAPI.ErrorLog.LogMessage(si,"aaaGPX_ARWUTL_AddRow_Init:   sMbrScriptx=" & sMbrScriptx)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_LRmbr", sCubex)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_LRmbr", sScenariox)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_LRmbr", sTimex)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_LRmbr", sEntityx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_LRmbr", sConsx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_LRmbr", sViewx)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_LRmbr", sAccountx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_LRmbr", sICx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_LRmbr", sFlowx)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_LRmbr", sOriginx)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_LRmbr", sU1x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_LRmbr", sU2x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_LRmbr", sU3x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_LRmbr", sU4x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_LRmbr", sU5x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_LRmbr", sU6x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_LRmbr", sU7x)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_LRmbr", sU8x)
				

				If sShowHideCbx_Cubex.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Cubex", sCubex)
				
				If sShowHideCbx_Scenariox.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Scenariox", sScenariox)
				If sShowHideCbx_Timex.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Timex", sTimex)
				If sShowHideCbx_Entityx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Entityx", sEntityx)
				If sShowHideCbx_Consx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Consx", sConsx)
				If sShowHideCbx_Viewx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Viewx", sViewx)
				
				If sShowHideCbx_Accountx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Accountx", sAccountx)
				If sShowHideCbx_ICx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_ICx", sICx)
				If sShowHideCbx_Flowx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Flowx", sFlowx)
				If sShowHideCbx_Originx.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Originx", sOriginx)
				
				If sShowHideCbx_U1x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U1x", sU1x)
				If sShowHideCbx_U2x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U2x", sU2x)
				If sShowHideCbx_U3x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U3x", sU3x)
				If sShowHideCbx_U4x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U4x", sU4x)
				If sShowHideCbx_U5x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U5x", sU5x)
				If sShowHideCbx_U6x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U6x", sU6x)
				If sShowHideCbx_U7x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U7x", sU7x)
				If sShowHideCbx_U8x.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U8x", sU8x)

				
				
				Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")
				Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
				Dim sTime As String = args.NameValuePairs.XFGetValue("Time")
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sCons As String = args.NameValuePairs.XFGetValue("Cons")
				Dim sView As String = args.NameValuePairs.XFGetValue("View")

				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
				Dim sIC As String = args.NameValuePairs.XFGetValue("IC")							
				Dim sFlow As String = args.NameValuePairs.XFGetValue("Flow")
				Dim sOrigin As String = args.NameValuePairs.XFGetValue("Origin")							

			
				Dim sU2 As String = args.NameValuePairs.XFGetValue("U2")
				Dim sU3 As String = args.NameValuePairs.XFGetValue("U3")
				Dim sU4 As String = args.NameValuePairs.XFGetValue("U4")
				Dim sU5 As String = args.NameValuePairs.XFGetValue("U5")
				Dim sU6 As String = args.NameValuePairs.XFGetValue("U6")
				Dim sU7 As String = args.NameValuePairs.XFGetValue("U7")
				Dim sU8 As String = args.NameValuePairs.XFGetValue("U8")								
		
'Dim sMbrScript As String = "Cb#" & sCube
'sMbrScript &= ":S#" & sScenario
'sMbrScript &= ":T#" & sTime
'sMbrScript &= ":E#" & sEntity
'sMbrScript &= ":C#" & sCons
'sMbrScript &= ":V#" & sView

'sMbrScript &= ":A#" & sAccount
'sMbrScript &= ":I#" & sIC
'sMbrScript &= ":F#" & sFlow
'sMbrScript &= ":O#" & sOrigin

'sMbrScript &= ":U1#" & sU1
'sMbrScript &= ":U2#" & sU2
'sMbrScript &= ":U3#" & sU3
'sMbrScript &= ":U4#" & sU4
'sMbrScript &= ":U5#" & sU5
'sMbrScript &= ":U6#" & sU6
'sMbrScript &= ":U7#" & sU7
'sMbrScript &= ":U8#" & sU8
'BRAPI.ErrorLog.LogMessage(si,"aaaGPX_ARWUTL_AddRow_Init:   sMbrScript=" & sMbrScript)

				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_LRmbr", sCube)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_LRmbr", sScenario)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_LRmbr", sTime)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_LRmbr", sEntity)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_LRmbr", sCons)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_LRmbr", sView)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_LRmbr", sAccount)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_LRmbr", sIC)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_LRmbr", sFlow)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_LRmbr", sOrigin)
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_LRmbr", sU1)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_LRmbr", sU2)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_LRmbr", sU3)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_LRmbr", sU4)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_LRmbr", sU5)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_LRmbr", sU6)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_LRmbr", sU7)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_LRmbr", sU8)

				
				
				
				If sShowHideCbx_Cube.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Cube", sCube)
				
				If sShowHideCbx_Scenario.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Scenario", sScenario)
				If sShowHideCbx_Time.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Time", sTime)
				If sShowHideCbx_Entity.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Entity", sEntity)
				If sShowHideCbx_Cons.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Cons", sCons)
				If sShowHideCbx_View.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_View", sView)
				
				If sShowHideCbx_Account.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Account", sAccount)
				If sShowHideCbx_IC.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_IC", sIC)
				If sShowHideCbx_Flow.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Flow", sFlow)
				If sShowHideCbx_Origin.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_Origin", sOrigin)
				
				If sShowHideCbx_U1.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U1", sU1)
				If sShowHideCbx_U2.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U2", sU2)
				If sShowHideCbx_U3.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U3", sU3)
				If sShowHideCbx_U4.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U4", sU4)
				If sShowHideCbx_U5.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U5", sU5)
				If sShowHideCbx_U6.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U6", sU6)
				If sShowHideCbx_U7.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U7", sU7)
				If sShowHideCbx_U8.XFEqualsIgnoreCase("true") Then objDictionary.Add("prompt_cbx_aaaGPX_ARWUTL_U8", sU8)
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


'		**********************************************************************************************************************************************
#Region "		CBX list"
'		**********************************************************************************************************************************************		
				If sShowHideCbx_Cubex.XFEqualsIgnoreCase("true") Then
					Dim sCubex_Dim As String = args.NameValuePairs.XFGetValue("CubexDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_Dimension", sCubex_Dim.Trim)

					Dim sCubex_ListOriginal As String = args.NameValuePairs.XFGetValue("CubexList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_ListOriginal", sCubex_ListOriginal.Trim)
					
					Dim sCubex_List As String = sCubex_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cubex_List", sCubex_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sCubex_Dim=" & sCubex_Dim & "   sCubex_ListOriginal=" & sCubex_ListOriginal & "   sCubex_List=" & sCubex_List)
				End If

				If sShowHideCbx_Cube.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("Cb#") Then
					Dim sCube_Dim As String = args.NameValuePairs.XFGetValue("CubeDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_Dimension", sCube_Dim.Trim)

					Dim sCube_ListOriginal As String = args.NameValuePairs.XFGetValue("CubeList")
					If sShowHideCbx_Cube.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("Cb#") Then sCube_ListOriginal = sCube
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_ListOriginal", sCube_ListOriginal.Trim)
				
					Dim sCube_List As String = sCube_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Cube.XFEqualsIgnoreCase("False") Then sCube_List = sCube
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cube_List", sCube_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sCube_Dim=" & sCube_Dim & "   sCube_ListOriginal=" & sCube_ListOriginal & "   sCube_List=" & sCube_List)
				End If

				If sShowHideCbx_Scenariox.XFEqualsIgnoreCase("true") Then
					Dim sScenariox_Dim As String = args.NameValuePairs.XFGetValue("ScenarioxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_Dimension", sScenariox_Dim.Trim)

					Dim sScenariox_ListOriginal As String = args.NameValuePairs.XFGetValue("ScenarioxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_ListOriginal", sScenariox_ListOriginal.Trim)
					
					Dim sScenariox_List As String = sScenariox_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenariox_List", sScenariox_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sScenariox_Dim=" & sScenariox_Dim & "   sScenariox_ListOriginal=" & sScenariox_ListOriginal & "   sScenariox_List=" & sScenariox_List)
				End If

				If sShowHideCbx_Scenario.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("S#") Then
					Dim sScenario_Dim As String = args.NameValuePairs.XFGetValue("ScenarioDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_Dimension", sScenario_Dim.Trim)

					Dim sScenario_ListOriginal As String = args.NameValuePairs.XFGetValue("ScenarioList")
					If sShowHideCbx_Scenario.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("S#") Then sScenario_ListOriginal = sScenario
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_ListOriginal", sScenario_ListOriginal.Trim)
					
					Dim sScenario_List As String = sScenario_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Scenario.XFEqualsIgnoreCase("False") Then sScenario_List = sScenario
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Scenario_List", sScenario_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sScenario_Dim=" & sScenario_Dim & "   sScenario_ListOriginal=" & sScenario_ListOriginal & "   sScenario_List=" & sScenario_List)
				End If

				If sShowHideCbx_Timex Then
					Dim sTimex_Dim As String = args.NameValuePairs.XFGetValue("TimexDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_Dimension", sTimex_Dim.Trim)

					Dim sTimex_ListOriginal As String = args.NameValuePairs.XFGetValue("TimexList")
					If sShowHideCbx_Timex.XFEqualsIgnoreCase("False") Then sTimex_ListOriginal = sTime				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_ListOriginal", sTimex_ListOriginal.Trim)
					
					Dim sTimex_List As String = sTimex_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Timex.XFEqualsIgnoreCase("False") Then sTimex_List = sTime				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Timex_List", sTimex_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sTime_Dim=" & sTime_Dim & "   sTime_ListOriginal=" & sTime_ListOriginal & "   sTime_List=" & sTime_List)
				End If

				If sShowHideCbx_Time.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("T#") Then
					Dim sTime_Dim As String = args.NameValuePairs.XFGetValue("TimeDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_Dimension", sTime_Dim.Trim)

					Dim sTime_ListOriginal As String = args.NameValuePairs.XFGetValue("TimeList")
					If sShowHideCbx_Time.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("T#") Then sTime_ListOriginal = sTime				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_ListOriginal", sTime_ListOriginal.Trim)
					
					Dim sTime_List As String = sTime_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Time.XFEqualsIgnoreCase("False") Then sTime_List = sTime				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Time_List", sTime_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sTime_Dim=" & sTime_Dim & "   sTime_ListOriginal=" & sTime_ListOriginal & "   sTime_List=" & sTime_List)
				End If

				If sShowHideCbx_Entityx.XFEqualsIgnoreCase("true") Then
					Dim sEntityx_Dim As String = args.NameValuePairs.XFGetValue("EntityxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_Dimension", sEntityx_Dim.Trim)

					Dim sEntityx_ListOriginal As String = args.NameValuePairs.XFGetValue("EntityxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_ListOriginal", sEntityx_ListOriginal.Trim)
					
					Dim sEntityx_List As String = sEntityx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entityx_List", sEntityx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sEntityx_Dim=" & sEntityx_Dim & "   sEntityx_ListOriginal=" & sEntityx_ListOriginal & "   sEntityx_List=" & sEntityx_List)
				End If

				If sShowHideCbx_Entity.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("E#") Then
					Dim sEntity_Dim As String = args.NameValuePairs.XFGetValue("EntityDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_Dimension", sEntity_Dim.Trim)

					Dim sEntity_ListOriginal As String = args.NameValuePairs.XFGetValue("EntityList")
					If sShowHideCbx_Entity.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("E#") Then sEntity_ListOriginal = sCons
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_ListOriginal", sEntity_ListOriginal.Trim)
					
					Dim sEntity_List As String = sEntity_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Entity.XFEqualsIgnoreCase("False") Then sEntity_List = sCons				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Entity_List", sEntity_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sEntity_Dim=" & sEntity_Dim & "   sEntity_ListOriginal=" & sEntity_ListOriginal & "   sEntity_List=" & sEntity_List)
				End If

				If sShowHideCbx_Consx.XFEqualsIgnoreCase("true") Then
					Dim sConsx_Dim As String = args.NameValuePairs.XFGetValue("ConsxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_Dimension", sConsx_Dim.Trim)

					Dim sConsx_ListOriginal As String = args.NameValuePairs.XFGetValue("ConsxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_ListOriginal", sConsx_ListOriginal.Trim)
					
					Dim sConsx_List As String = sConsx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consx_List", sConsx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sConsx_Dim=" & sConsx_Dim & "   sConsx_ListOriginal=" & sConsx_ListOriginal & "   sConsx_List=" & sConsx_List)
				End If

				If sShowHideCbx_Cons.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("C#") Then
					Dim sCons_Dim As String = args.NameValuePairs.XFGetValue("ConsDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_Dimension", sCons_Dim.Trim)

					Dim sCons_ListOriginal As String = args.NameValuePairs.XFGetValue("ConsList")
					If sShowHideCbx_Cons.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("C#") Then sCons_ListOriginal = sCons				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_ListOriginal", sCons_ListOriginal.Trim)
					
					Dim sCons_List As String = sCons_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Cons.XFEqualsIgnoreCase("False") Then sCons_List = sCons				
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Cons_List", sCons_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sCons_Dim=" & sCons_Dim & "   sCons_ListOriginal=" & sCons_ListOriginal & "   sCons_List=" & sCons_List)
				End If

				If sShowHideCbx_Viewx.XFEqualsIgnoreCase("true") Then
					Dim sViewx_Dim As String = args.NameValuePairs.XFGetValue("ViewxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_Dimension", sViewx_Dim.Trim)

					Dim sViewx_ListOriginal As String = args.NameValuePairs.XFGetValue("ViewxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_ListOriginal", sViewx_ListOriginal.Trim)
					
					Dim sViewx_List As String = sViewx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Viewx_List", sViewx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sViewx_Dim=" & sViewx_Dim & "   sViewx_ListOriginal=" & sViewx_ListOriginal & "   sViewx_List=" & sViewx_List)
				End If

				If sShowHideCbx_View.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("V#") Then
					Dim sView_Dim As String = args.NameValuePairs.XFGetValue("ViewDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_Dimension", sView_Dim.Trim)

					Dim sView_ListOriginal As String = args.NameValuePairs.XFGetValue("ViewList")
					If sShowHideCbx_View.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("V#") Then sView_ListOriginal = sView	
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_ListOriginal", sView_ListOriginal.Trim)
					
					Dim sView_List As String = sView_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_View.XFEqualsIgnoreCase("False") Then sView_List = sView
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_View_List", sView_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sView_Dim=" & sView_Dim & "   sView_ListOriginal=" & sView_ListOriginal & "   sView_List=" & sView_List)
				End If

				If sShowHideCbx_Accountx.XFEqualsIgnoreCase("true") Then
					Dim sAccountx_Dim As String = args.NameValuePairs.XFGetValue("AccountxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_Dimension", sAccountx_Dim.Trim)

					Dim sAccountx_ListOriginal As String = args.NameValuePairs.XFGetValue("AccountxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_ListOriginal", sAccountx_ListOriginal.Trim)
					
					Dim sAccountx_List As String = sAccountx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Accountx_List", sAccountx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sAccountx_Dim=" & sAccountx_Dim & "   sAccountx_ListOriginal=" & sAccountx_ListOriginal & "   sAccountx_List=" & sAccountx_List)
				End If

				If sShowHideCbx_Account.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("A#") Then
					Dim sAccount_Dim As String = args.NameValuePairs.XFGetValue("AccountDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_Dimension", sAccount_Dim.Trim)

					Dim sAccount_ListOriginal As String = args.NameValuePairs.XFGetValue("AccountList")
					If sShowHideCbx_Account.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("A#")  Then sAccount_ListOriginal = sAccount
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_ListOriginal", sAccount_ListOriginal.Trim)
					
					Dim sAccount_List As String = sAccount_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Account.XFEqualsIgnoreCase("False") Then sAccount_List = sAccount
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Account_List", sAccount_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sAccount_Dim=" & sAccount_Dim & "   sAccount_ListOriginal=" & sAccount_ListOriginal & "   sAccount_List=" & sAccount_List)
				End If

				If sShowHideCbx_ICx.XFEqualsIgnoreCase("true") Then
					Dim sICx_Dim As String = args.NameValuePairs.XFGetValue("ICxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_Dimension", sICx_Dim.Trim)

					Dim sICx_ListOriginal As String = args.NameValuePairs.XFGetValue("ICxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_ListOriginal", sICx_ListOriginal.Trim)
					
					Dim sICx_List As String = sICx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ICx_List", sICx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sICx_Dim=" & sICx_Dim & "   sICx_ListOriginal=" & sICx_ListOriginal & "   sICx_List=" & sICx_List)
				End If

				If sShowHideCbx_IC.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("I#") Then
					Dim sIC_Dim As String = args.NameValuePairs.XFGetValue("ICDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_Dimension", sIC_Dim.Trim)

					Dim sIC_ListOriginal As String = args.NameValuePairs.XFGetValue("ICList")
					If sShowHideCbx_IC.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("I#")  Then sIC_ListOriginal = sIC
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_ListOriginal", sIC_ListOriginal.Trim)
					
					Dim sIC_List As String = sIC_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_IC.XFEqualsIgnoreCase("False") Then sIC_List = sIC
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_IC_List", sIC_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sIC_Dim=" & sIC_Dim & "   sIC_ListOriginal=" & sIC_ListOriginal & "   sIC_List=" & sIC_List)
				End If

				If sShowHideCbx_Flowx.XFEqualsIgnoreCase("true") Then
					Dim sFlowx_Dim As String = args.NameValuePairs.XFGetValue("FlowxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_Dimension", sFlowx_Dim.Trim)

					Dim sFlowx_ListOriginal As String = args.NameValuePairs.XFGetValue("FlowxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_ListOriginal", sFlowx_ListOriginal.Trim)
					
					Dim sFlowx_List As String = sFlowx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flowx_List", sFlowx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sFlowx_Dim=" & sFlowx_Dim & "   sFlowx_ListOriginal=" & sFlowx_ListOriginal & "   sFlowx_List=" & sFlowx_List)
				End If

				If sShowHideCbx_Flow.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("F#") Then
					Dim sFlow_Dim As String = args.NameValuePairs.XFGetValue("FlowDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_Dimension", sFlow_Dim.Trim)

					Dim sFlow_ListOriginal As String = args.NameValuePairs.XFGetValue("FlowList")
					If sShowHideCbx_Flow.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("F#")  Then sFlow_ListOriginal = sFlow
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_ListOriginal", sFlow_ListOriginal.Trim)
					
					Dim sFlow_List As String = sFlow_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Flow.XFEqualsIgnoreCase("False") Then sFlow_List = sFlow
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Flow_List", sFlow_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sFlow_Dim=" & sFlow_Dim & "   sFlow_ListOriginal=" & sFlow_ListOriginal & "   sFlow_List=" & sFlow_List)
				End If

				If sShowHideCbx_Originx.XFEqualsIgnoreCase("true") Then
					Dim sOriginx_Dim As String = args.NameValuePairs.XFGetValue("OriginxDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_Dimension", sOriginx_Dim.Trim)

					Dim sOriginx_ListOriginal As String = args.NameValuePairs.XFGetValue("OriginxList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_ListOriginal", sOriginx_ListOriginal.Trim)
					
					Dim sOriginx_List As String = sOriginx_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Originx_List", sOriginx_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sOriginx_Dim=" & sOriginx_Dim & "   sOriginx_ListOriginal=" & sOriginx_ListOriginal & "   sOriginx_List=" & sOriginx_List)
				End If

				If sShowHideCbx_Origin.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("F#") Then
					Dim sOrigin_Dim As String = args.NameValuePairs.XFGetValue("OriginDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_Dimension", sOrigin_Dim.Trim)

					Dim sOrigin_ListOriginal As String = args.NameValuePairs.XFGetValue("OriginList")
					If sShowHideCbx_Origin.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("O#")  Then sOrigin_ListOriginal = sOrigin
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_ListOriginal", sOrigin_ListOriginal.Trim)
					
					Dim sOrigin_List As String = sOrigin_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_Origin.XFEqualsIgnoreCase("False") Then sOrigin_List = sOrigin
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Origin_List", sOrigin_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sOrigin_Dim=" & sOrigin_Dim & "   sOrigin_ListOriginal=" & sOrigin_ListOriginal & "   sOrigin_List=" & sOrigin_List)
				End If

				If sShowHideCbx_U1x.XFEqualsIgnoreCase("true") Then
				
					Dim sU1x_Dim As String = args.NameValuePairs.XFGetValue("U1xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_Dimension", sU1x_Dim.Trim)

					Dim sU1x_ListOriginal As String = args.NameValuePairs.XFGetValue("U1xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_ListOrignal", sU1x_ListOriginal.Trim)
					
					Dim sU1x_List As String = sU1x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1x_List", sU1x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU1x_Dim=" & sU1x_Dim & "   sU1x_ListOriginal=" & sU1x_ListOriginal & "   sU1x_List=" & sU1x_List)
				End If

				If sShowHideCbx_U1.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U1#") Then
					Dim sU1_Dim As String = args.NameValuePairs.XFGetValue("U1Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_Dimension", sU1_Dim.Trim)

					Dim sU1_ListOriginal As String = args.NameValuePairs.XFGetValue("U1List")
					If sShowHideCbx_U1.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U1#")  Then sU1_ListOriginal = sU1
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_ListOriginal", sU1_ListOriginal.Trim)
					
					Dim sU1_List As String = sU1_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U1.XFEqualsIgnoreCase("False") Then sU1_List = sU1
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U1_List", sU1_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU1_Dim=" & sU1_Dim & "   sU1_ListOriginal=" & sU1_ListOriginal & "   sU1_List=" & sU1_List)
				End If

				If sShowHideCbx_U2x.XFEqualsIgnoreCase("true") Then
					Dim sU2x_Dim As String = args.NameValuePairs.XFGetValue("U2xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_Dimension", sU2x_Dim.Trim)

					Dim sU2x_ListOriginal As String = args.NameValuePairs.XFGetValue("U2xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_ListOriginal", sU2x_ListOriginal.Trim)
					
					Dim sU2x_List As String = sU2x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2x_List", sU2x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU2x_Dim=" & sU2x_Dim & "   sU2x_ListOriginal=" & sU2x_ListOriginal & "   sU2x_List=" & sU2x_List)
				End If

				If sShowHideCbx_U2.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U2#") Then
					Dim sU2_Dim As String = args.NameValuePairs.XFGetValue("U2Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_Dimension", sU2_Dim.Trim)

					Dim sU2_ListOriginal As String = args.NameValuePairs.XFGetValue("U2List")
					If sShowHideCbx_U2.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U2#")  Then sU2_ListOriginal = sU2
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_ListOriginal", sU2_ListOriginal.Trim)
					
					Dim sU2_List As String = sU2_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U2.XFEqualsIgnoreCase("False") Then sU2_List = sU2
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U2_List", sU2_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU2_Dim=" & sU2_Dim & "   sU2_ListOriginal=" & sU2_ListOriginal & "   sU2_List=" & sU2_List)
				End If

				If sShowHideCbx_U3x.XFEqualsIgnoreCase("true") Then
					Dim sU3x_Dim As String = args.NameValuePairs.XFGetValue("U3xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_Dimension", sU3x_Dim.Trim)

					Dim sU3x_ListOriginal As String = args.NameValuePairs.XFGetValue("U3xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_ListOriginal", sU3x_ListOriginal.Trim)
					
					Dim sU3x_List As String = sU3x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3x_List", sU3x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU3x_Dim=" & sU3x_Dim & "   sU3x_ListOriginal=" & sU3x_ListOriginal & "   sU3x_List=" & sU3x_List)
				End If

				If sShowHideCbx_U3.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U3#") Then
					Dim sU3_Dim As String = args.NameValuePairs.XFGetValue("U3Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_Dimension", sU3_Dim.Trim)

					Dim sU3_ListOriginal As String = args.NameValuePairs.XFGetValue("U3List")
					If sShowHideCbx_U3.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U3#")  Then sU3_ListOriginal = sU3
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_ListOriginal", sU3_ListOriginal.Trim)
					
					Dim sU3_List As String = sU3_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U3.XFEqualsIgnoreCase("False") Then sU3_List = sU3
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U3_List", sU3_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU3_Dim=" & sU3_Dim & "   sU3_ListOriginal=" & sU3_ListOriginal & "   sU3_List=" & sU3_List)
				End If

				If sShowHideCbx_U4x.XFEqualsIgnoreCase("true") Then
					Dim sU4x_Dim As String = args.NameValuePairs.XFGetValue("U4xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_Dimension", sU4x_Dim.Trim)

					Dim sU4x_ListOriginal As String = args.NameValuePairs.XFGetValue("U4xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_ListOriginal", sU4x_ListOriginal.Trim)
					
					Dim sU4x_List As String = sU4x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4x_List", sU4x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU4x_Dim=" & sU4x_Dim & "   sU4x_ListOriginal=" & sU4x_ListOriginal & "   sU4x_List=" & sU4x_List)
				End If

				If sShowHideCbx_U4.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U4#") Then
					Dim sU4_Dim As String = args.NameValuePairs.XFGetValue("U4Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_Dimension", sU4_Dim.Trim)

					Dim sU4_ListOriginal As String = args.NameValuePairs.XFGetValue("U4List")
					If sShowHideCbx_U4.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U4#")  Then sU4_ListOriginal = sU4
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_ListOriginal", sU4_ListOriginal.Trim)
					
					Dim sU4_List As String = sU4_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U4.XFEqualsIgnoreCase("False") Then sU4_List = sU4
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U4_List", sU4_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU4_Dim=" & sU4_Dim & "   sU4_ListOriginal=" & sU4_ListOriginal & "   sU4_List=" & sU4_List)
				End If

				If sShowHideCbx_U5x.XFEqualsIgnoreCase("true") Then
					Dim sU5x_Dim As String = args.NameValuePairs.XFGetValue("U5xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_Dimension", sU5x_Dim.Trim)

					Dim sU5x_ListOriginal As String = args.NameValuePairs.XFGetValue("U5xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_ListOriginal", sU5x_ListOriginal.Trim)
					
					Dim sU5x_List As String = sU5x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5x_List", sU5x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU5x_Dim=" & sU5x_Dim & "   sU5x_ListOriginal=" & sU5x_ListOriginal & "   sU5x_List=" & sU5x_List)
				End If

				If sShowHideCbx_U5.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U5#") Then
					Dim sU5_Dim As String = args.NameValuePairs.XFGetValue("U5Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_Dimension", sU5_Dim.Trim)

					Dim sU5_ListOriginal As String = args.NameValuePairs.XFGetValue("U5List")
					If sShowHideCbx_U5.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U5#") Then sU5_ListOriginal = sU5
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_ListOriginal", sU5_ListOriginal.Trim)
					
					Dim sU5_List As String = sU5_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U5.XFEqualsIgnoreCase("False") Then sU5_List = sU5
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U5_List", sU5_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU5_Dim=" & sU5_Dim & "   sU5_ListOriginal=" & sU5_ListOriginal & "   sU5_List=" & sU5_List)
				End If

				If sShowHideCbx_U6x.XFEqualsIgnoreCase("true") Then
					Dim sU6x_Dim As String = args.NameValuePairs.XFGetValue("U6xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_Dimension", sU6x_Dim.Trim)

					Dim sU6x_ListOriginal As String = args.NameValuePairs.XFGetValue("U6xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_ListOriginal", sU6x_ListOriginal.Trim)
					
					Dim sU6x_List As String = sU6x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6x_List", sU6x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU6x_Dim=" & sU6x_Dim & "   sU6x_ListOriginal=" & sU6x_ListOriginal & "   sU6x_List=" & sU6x_List)
				End If

				If sShowHideCbx_U6.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U6#") Then
					Dim sU6_Dim As String = args.NameValuePairs.XFGetValue("U6Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_Dimension", sU6_Dim.Trim)

					Dim sU6_ListOriginal As String = args.NameValuePairs.XFGetValue("U6List")
					If sShowHideCbx_U6.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U6#") Then sU6_ListOriginal = sU6
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_ListOriginal", sU6_ListOriginal.Trim)
					
					Dim sU6_List As String = sU6_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U6.XFEqualsIgnoreCase("False") Then sU6_List = sU6
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U6_List", sU6_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU6_Dim=" & sU6_Dim & "   sU6_ListOriginal=" & sU6_ListOriginal & "   sU6_List=" & sU6_List)
				End If

				If sShowHideCbx_U7x.XFEqualsIgnoreCase("true") Then
					Dim sU7x_Dim As String = args.NameValuePairs.XFGetValue("U7xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_Dimension", sU7x_Dim.Trim)

					Dim sU7x_ListOriginal As String = args.NameValuePairs.XFGetValue("U7xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_ListOriginal", sU7x_ListOriginal.Trim)
					
					Dim sU7x_List As String = sU7x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7x_List", sU7x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU7x_Dim=" & sU7x_Dim & "   sU7x_ListOriginal=" & sU7x_ListOriginal & "   sU7x_List=" & sU7x_List)
				End If

				If sShowHideCbx_U7.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U7#") Then
					Dim sU7_Dim As String = args.NameValuePairs.XFGetValue("U7Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_Dimension", sU7_Dim.Trim)

					Dim sU7_ListOriginal As String = args.NameValuePairs.XFGetValue("U7List")
					If sShowHideCbx_U7.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U7#")  Then sU7_ListOriginal = sU7
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_ListOriginal", sU7_ListOriginal.Trim)
					
					Dim sU7_List As String = sU7_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U7.XFEqualsIgnoreCase("False") Then sU7_List = sU7
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U7_List", sU7_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU7_Dim=" & sU7_Dim & "   sU7_ListOriginal=" & sU7_ListOriginal & "   sU7_List=" & sU7_List)
				End If

				If sShowHideCbx_U8x.XFEqualsIgnoreCase("true") Then
					Dim sU8x_Dim As String = args.NameValuePairs.XFGetValue("U8xDim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_Dimension", sU8x_Dim.Trim)

					Dim sU8x_ListOriginal As String = args.NameValuePairs.XFGetValue("U8xList")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_ListOriginal", sU8x_ListOriginal.Trim)
					
					Dim sU8x_List As String = sU8x_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8x_List", sU8x_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU8x_Dim=" & sU8x_Dim & "   sU8x_ListOriginal=" & sU8x_ListOriginal & "   sU8x_List=" & sU8x_List)
				End If

				If sShowHideCbx_U8.XFEqualsIgnoreCase("true") Or sReclassDims.XFContainsIgnoreCase("U8#") Then
					Dim sU8_Dim As String = args.NameValuePairs.XFGetValue("U8Dim")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_Dimension", sU8_Dim.Trim)

					Dim sU8_ListOriginal As String = args.NameValuePairs.XFGetValue("U8List")
					If sShowHideCbx_U8.XFEqualsIgnoreCase("False") And Not sReclassDims.XFContainsIgnoreCase("U8#")  Then sU8_ListOriginal = sU8
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_ListOriginal", sU8_ListOriginal.Trim)
					
					Dim sU8_List As String = sU8_ListOriginal.Replace("|~", "|!").Replace("~|", "!|")
					If sShowHideCbx_U8.XFEqualsIgnoreCase("False") Then sU8_List = sU8
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_U8_List", sU8_List.Trim)
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sU8_Dim=" & sU8_Dim & "   sU8_ListOriginal=" & sU8_ListOriginal & "   sU8_List=" & sU8_List)
				End If					

'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


'		**********************************************************************************************************************************************
#Region "		Set Filters"
'		**********************************************************************************************************************************************		
				
				'----- set default -----
				Dim sDisplay_ParentCube As String = "Parent Cube"
				Dim sDisplay_Cube As String = "Fund Cube"

				Dim sDisplay_ParentScenario As String = "Parent Scenario"
				Dim sDisplay_Scenario As String = "Scenario"
				Dim sDisplay_ParentTime As String = "Parent Time"
				Dim sDisplay_Time As String = "Time"				
				Dim sDisplay_ParentEntity As String = "Parent Fund Center"
				Dim sDisplay_Entity As String = "Fund Center"
				Dim sDisplay_ParentCons As String = "Parent Cons"
				Dim sDisplay_Cons As String = "Cons"
				Dim sDisplay_ParentView As String = "Parent View"
				Dim sDisplay_View As String = "View"

				Dim sDisplay_ParentAccount As String = "Parent Account"
				Dim sDisplay_Account As String = "Account"
				Dim sDisplay_ParentIC As String = "Parent IC"
				Dim sDisplay_IC As String = "IC"
				Dim sDisplay_ParentFlow As String = "Parent Flow"
				Dim sDisplay_Flow As String = "Flow"				
				Dim sDisplay_ParentOrigin As String = "Parent Origin"
				Dim sDisplay_Origin As String = "Origin"
				
				Dim sDisplay_ParentU1 As String = "Appropriation"
				Dim sDisplay_U1 As String = "Fund Code"
				Dim sDisplay_ParentU2 As String = "PEG"
				Dim sDisplay_U2 As String = "MDEP"
				Dim sDisplay_ParentU3 As String = "SAG"
				Dim sDisplay_U3 As String = "APE"
				Dim sDisplay_ParentU4 As String = "Parent Dollar Type"
				Dim sDisplay_U4 As String = "Dollar Type"
				Dim sDisplay_ParentU5 As String = "Resource Code"
				Dim sDisplay_U5 As String = "cType"
				Dim sDisplay_ParentU6 As String = "Parent Object Class"
				Dim sDisplay_U6 As String = "Object Class"
				Dim sDisplay_ParentU7 As String = "Parent UIC"
				Dim sDisplay_U7 As String = "UIC"
				Dim sDisplay_ParentU8 As String = "Parent REIMS"
				Dim sDisplay_U8 As String = "REIMS"

				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeName As String = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Name	
				'Updated 073025 by JM - 1731 & 1736 update Cost Category to ObjectClass
				Select Case True
					Case wfScenarioTypeName.XFEqualsIgnoreCase("ScenarioType2")
						sDisplay_U3 = "APE_PT"
						sDisplay_U6 = "ObjectClass"
						
					Case wfScenarioTypeName.XFEqualsIgnoreCase("Budget")
						sDisplay_U6 = "ObjectClass"
				End Select		
				
				
				Dim sFilter_Display As String = ""
				Dim sFilter_Value As String = ""
				
				If sShowHideCbx_Cubex.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentCube
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Cubex"
				End If
				If sShowHideCbx_Cube.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Cube
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Cube"
				End If

				If sShowHideCbx_Scenariox.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentScenario
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Scenariox"
				End If
				If sShowHideCbx_Scenario.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Scenario
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Scenario"
				End If
				
				If sShowHideCbx_Timex.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentTime
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Timex"
				End If
				If sShowHideCbx_Time.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Time
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Time"
				End If
				
				If sShowHideCbx_Entityx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentEntity
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Entityx"
				End If
				If sShowHideCbx_Entity.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Entity
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Entity"
				End If
				
				If sShowHideCbx_Consx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentCons
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Consx"
				End If
				If sShowHideCbx_Cons.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Cons
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Cons"
				End If
				
				If sShowHideCbx_Viewx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentView
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Viewx"
				End If
				If sShowHideCbx_View.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_View
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_View"
				End If
				
				If sShowHideCbx_Accountx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentAccount
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Accountx"
				End If
				If sShowHideCbx_Account.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Account
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Account"
				End If
				
				If sShowHideCbx_ICx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentIC
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_ICx"
				End If
				If sShowHideCbx_IC.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_IC
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_IC"
				End If
				
				If sShowHideCbx_Flowx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentFlow
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Flowx"
				End If
				If sShowHideCbx_Flow.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Flow
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Flow"
				End If
				
				If sShowHideCbx_Originx.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentOrigin
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Originx"
				End If
				If sShowHideCbx_Origin.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_Origin
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_Origin"
				End If
				
				If sShowHideCbx_U1x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU1
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U1x"
				End If
				If sShowHideCbx_U1.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U1
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U1"
				End If
				
				If sShowHideCbx_U2x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU2
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U2x"
				End If
				If sShowHideCbx_U2.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U2
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U2"
				End If
				
				If sShowHideCbx_U3x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU3
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U3x"
				End If
				If sShowHideCbx_U3.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U3
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U3"
				End If
				
				If sShowHideCbx_U4x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU4
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U4x"
				End If
				If sShowHideCbx_U4.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U4
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U4"
				End If
				
				If sShowHideCbx_U5x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU5
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U5x"
				End If
				If sShowHideCbx_U5.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U5
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U5"
				End If
				
				If sShowHideCbx_U6x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU6
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U6x"
				End If
				If sShowHideCbx_U6.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U6
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U6"
				End If
				
				If sShowHideCbx_U7x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU7
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U7x"
				End If
				If sShowHideCbx_U7.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U7
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U7"
				End If
				
				If sShowHideCbx_U8x.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_ParentU8
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U8x"
				End If
				If sShowHideCbx_U8.XFEqualsIgnoreCase("true") Then
					If sFilter_Display <> "" Then sFilter_Display &= ","
					sFilter_Display &= sDisplay_U8
					If sFilter_Value <> "" Then sFilter_Value &= ","
					sFilter_Value &= "prompt_cbx_aaaGPX_ARWUTL_U8"
				End If				
				
				If sFilter_Display <> "" Then
					sFilter_Display = "All, " & sFilter_Display
					sFilter_Value = "[" & sFilter_Value & "], " & sFilter_Value
				End If
				
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ResetFilters_Display", sFilter_Display)
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ResetFilters_Value", sFilter_Value)
				
				
				If sFilter_Value = "" Then
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ResetFilters_ShowCbx", "False")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ShowButtonResetFilters", "False")
				Else
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ResetFilters_ShowCbx", "True")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_ShowButtonResetFilters", "True")
				End If

'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************

'		**********************************************************************************************************************************************
#Region "		Options"
'		**********************************************************************************************************************************************

				Dim sValidation As String = ""
				If args.NameValuePairs.ContainsKey("Validation") Then sValidation = args.NameValuePairs.XFGetValue("Validation")
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sValidation=" & sValidation)
				
				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Validation", sValidation)

				
				Dim sConsolidate As String = args.NameValuePairs.XFGetValue("Consolidate")
				If args.NameValuePairs.ContainsKey("Consolidate") Then sConsolidate = args.NameValuePairs.XFGetValue("Consolidate")
'BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: sConsolidate=" & sConsolidate)			

				'----- store state to parameters -----					
				objDictionary.Add("var_tbx_aaaGPX_ARWUTL_Consolidate", sConsolidate)
				
				Select Case True
				Case sShowHideBtn_Add.XFContainsIgnoreCase("True")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_DashboardDescription", "Add a Funding Line")
				Case sShowHideBtn_Remove.XFContainsIgnoreCase("True")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_DashboardDescription", "Remove a Funding Line")
				Case sShowHideBtn_Reclass.XFContainsIgnoreCase("True")
					objDictionary.Add("var_tbx_aaaGPX_ARWUTL_DashboardDescription", "Update all Funding Lines")
				End Select
				
'		**********************************************************************************************************************************************				
#End Region
'		**********************************************************************************************************************************************


				For i As Integer = 0 To objDictionary.Count-1	
				'	BRAPI.ErrorLog.LogMessage(si,"AddRow_Init: Key=" & objDictionary.ElementAt(i).Key & "  Value=" & objDictionary.ElementAt(i).Value)
				Next	

				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult.IsOK = True
					selectionChangedTaskResult.ShowMessageBox = False
					selectionChangedTaskResult.Message = ""
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing 'objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
					selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
					selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
					selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
					selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
					selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary

				Return selectionChangedTaskResult
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Updated 09/17		

#Region "ResetParameters"
		'---------------------------------------------------------------------------------------------------
		' Purpose: This will call a DM task to clear up the fields and reset the selection from a combo box
		' Usage: WF > Target Distribution > Average Salary > Average Salary Approval > Reset Reccommendation Button (btn_Reset_Reccommendation)
		' Created: 2022-Apr-01 - YH
		' Modified: 
		'---------------------------------------------------------------------------------------------------
		Public Function ResetParameters(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
'BRApi.ErrorLog.LogMessage(si,"In Reset_SelectedComponents ")
				'If there are no user selected values then there is no need to do anything
				If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues Is Nothing Then
					Return Nothing
				End If
				
				Dim parameterName As String = "var_Comp_to_Reset"
				Dim parameterValue As String = ""
				   
				'Set literal parameter
				'If there is no parameter to be cleared, then there is nothing to reset. 
				'The button needs To specify what parameters it needs to clear out
				parameterValue = args.NameValuePairs.XFGetValue(parameterName)
'BRApi.ErrorLog.LogMessage(si,"parameterValue:" & parameterValue)					
				If String.IsNullOrEmpty(parameterValue) Then
					Return Nothing
				End If
				Dim compsToBeCleared = parameterValue.Split(",")

'							Dim currDashboard As Dashboard = args.PrimaryDashboard
'							Dim dashAction As String ="Redraw"
'							Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
'							Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
'							objXFSelectionChangedUIActionInfo.DashboardsToRedraw = Nothing 'currDashboard.Name
'							objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
'BRApi.ErrorLog.LogMessage(si,"parameterValue:" & parameterValue & objDictionary.count)			
				For i As Integer = 0 To objDictionary.Count -1
'BRApi.ErrorLog.LogMessage(si,"key:" & objDictionary.ElementAt(i).Key)
'								If String.IsNullOrEmpty(objDictionary.ElementAt(i).Value) Then
'BRApi.ErrorLog.LogMessage(si,  "Value:" & objDictionary.ElementAt(i).Value)
'								End If

					'Clear only prompts that are passed in as parameters
					Dim thisKey = objDictionary.ElementAt(i).Key
					Dim thisValue = objDictionary.ElementAt(i).Value
					If(parameterValue.XFContainsIgnoreCase(thisKey)) Then
						objDictionary.Remove(thisKey)
						objDictionary.Add(thisKey,"")
					End If 
'BRApi.ErrorLog.LogMessage(si, "Button Key Deletion:" & thisKey)
				Next
				
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = False
				selectionChangedTaskResult.Message = ""
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing 'objXFSelectionChangedUIActionInfo
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
				selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary
				Return selectionChangedTaskResult
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region 'Updated 09/26/2025



	End Class
End Namespace