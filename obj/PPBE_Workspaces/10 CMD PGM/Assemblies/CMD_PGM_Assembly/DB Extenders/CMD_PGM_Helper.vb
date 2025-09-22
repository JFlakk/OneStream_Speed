Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database
Imports System.Text.RegularExpressions
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_PGM_Helper
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

						Select Case args.FunctionName
							Case args.FunctionName.XFEqualsIgnoreCase("Check_WF_Complete_Lock")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
							Case args.FunctionName.XFEqualsIgnoreCase("CreateREQMain")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								Me.DbCache(si,args)
						End Select

#Region "Updated Status"
'Updated by Fronz 09/24/2024 - Added the Check_WF_Complete_Lock function
				If args.FunctionName.XFEqualsIgnoreCase("UpdatedStatus") Then
					Me.Check_WF_Complete_Lock(si, globals, api, args)
					Dim REQFlow As String = args.NameValuePairs.XFGetValue("REQFlow")
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				
					If REQFlow.xfContainsignorecase("REQ") And wfProfileName.XFContainsIgnoreCase("Formulate") Then
						Try
							Me.SubmitREQ(si, globals, api, args)
							Catch ex As Exception
						End Try
						
					Else If REQFlow.xfContainsignorecase("REQ") And wfProfileName.XFContainsIgnoreCase("Validate") Then
						Try
							If Not Me.IsREQValidationAllowed(si, args) Then
								Throw New Exception("Cannot validate requirement at this time. Contact requirements manager.")
							End If
							Me.ValidateRequirement(si, globals, api, args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
						End Try

					Else If (Not REQFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Prioritize") Then
						Me.SubmitForApproval(si, globals, api, args)				
						
					Else If (Not REQFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Approve Requirements CMD") Then
						Me.ApproveRequirement(si, globals, api, args)

					Else If (Not REQFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Approve Requirements") Then				
						Me.SubmitToCommand(si, globals, api, args)
						
						
					Else If (Not REQFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Manage") Then
'						Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Requirements_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()									
'							For Each ExistingUFR As MemberInfo In ExistingUFRs
'								Dim UFR As String = ExistingUFR.Member.Name
'								Try
									Me.ManageREQStatusUpdated(si, globals, api, args, "")
'									Catch ex As Exception
'								End Try
'							Next
					End If
				End If
#End Region						
						
#Region "Create New REQ"
						'---------------------------------------------------------------------------------------------------------------------------------------------------------						
						'Create New REQ		
						'---------------------------------------------------------------------------------------------------------------------------------------------------------												
						If args.FunctionName.XFEqualsIgnoreCase("CreateREQ") Then
							Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
							
							'User's permissions check
							If Not Me.IsREQCreationAllowed(si, sEntity) Then
								Throw  New Exception("REQ Title:" & environment.NewLine & "Can not create REQ at this time. Contact REQ manager." & environment.NewLine)
							End If
							
							'Validate Requested Amount Input
							Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")	
							If Not Integer.TryParse(sRequestedAmt, 0) Then	
								Throw New Exception("Requested Amount: " &  sRequestedAmt &  environment.NewLine & "Must enter a whole number.")					
							ElseIf sRequestedAmt <= 0
								Throw New Exception("Requested Amount: Must enter a value greater than zero.")
							End If	
							
							Dim sU1Input As String = args.NameValuePairs.XFGetValue("APPNFund")
							Dim sU2Input As String = args.NameValuePairs.XFGetValue("MDEP")
							Dim sU3Input As String = args.NameValuePairs.XFGetValue("APE")
							Dim sU4Input As String = args.NameValuePairs.XFGetValue("DollarType")
							Dim sU6Input As String = args.NameValuePairs.XFGetValue("CommitmentItem")
							
							If String.IsNullOrWhiteSpace(sU1Input) Then
								Throw New Exception("Must enter a valid Fund Code for the Requirement.")
							End If	
							If String.IsNullOrWhiteSpace(sU2Input) Then
								Throw New Exception("Must enter a valid MDEP for the Requirement.")
							End If	
							If String.IsNullOrWhiteSpace(sU3Input) Then
								Throw New Exception("Must enter a valid APE for the Requirement.")
							End If	
							If String.IsNullOrWhiteSpace(sU4Input) Then
								Throw New Exception("Must enter a valid Dollar Type for the Requirement.")
							End If	
							If String.IsNullOrWhiteSpace(sU6Input) Then
								Throw New Exception("Must enter a valid Cost Category for the Requirement.")
							End If	
						
							Dim REQFlow = Me.CreateEmptyREQ(si, globals, api, args, "Create",sEntity)
				
							Me.SaveInitialAmount(si, globals, api, args, REQFlow)
							
							Me.CalculateFullFYDP(si, globals, api, args, REQFlow)
	
							'Clear the selections
							Dim paramsToClear As String = "prompt_tbx_REQPRO_AAAAAA_0CaAa_REQTitle__Shared," & _
														"prompt_tbx_REQPRO_AAAAAA_0CaAa_ReqAmt__Shared," & _
														"prompt_cbx_REQPRO_AAAAAA_0CaAa_APPN__Shared," & _
														"prompt_cbx_REQPRO_AAAAAA_0CaAa_APPNFund__Shared," & _
														"ML_REQPRO_MDEP," & _
														"ML_REQPRO_SAG," & _
														"ML_REQPRO_SAGAPE," & _
														"prompt_cbx_REQPRO_AAAAAA_0CaAa_DollarType__Shared," & _
														"prompt_cbx_REQPRO_AAAAAA_0CaAa_CommitmentItem__Shared," & _
														"prompt_cbx_REQPRO_AAAAAA_0CaAa_RecurringCost__Shared"
														
							
						
							
							
							
						Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Me.ClearSelections(si, globals, api, args, paramsToClear)	
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
						selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
						selectionChangedTaskResult.ModifiedCustomSubstVars.Remove("LR_REQ_Entity")
						selectionChangedTaskResult.ModifiedCustomSubstVars.Remove("LR_REQ_Flow")
						selectionChangedTaskResult.ModifiedCustomSubstVars.Add("LR_REQ_Entity",sEntity)
						selectionChangedTaskResult.ModifiedCustomSubstVars.Add("LR_REQ_Flow",REQFlow)
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Remove("LR_REQ_Entity")
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Remove("LR_REQ_Flow")
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("LR_REQ_Entity",sEntity)
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("LR_REQ_Flow",REQFlow)
						Return selectionChangedTaskResult
						
							
							
						End If
#End Region '(Updated)

#Region "Copy REQ"
						If args.FunctionName.XFEqualsIgnoreCase("CopyREQ") Then	
							Dim REQFlow = Me.CreateEmptyREQ(si, globals, api, args)
'							Me.CopyREQDetails(si,globals,api,args, REQFlow)
'BRApi.ErrorLog.LogMessage(si, "REQFlow = " & REQFlow)							
							'Me.SaveInitialAmount(si,globals,api,args, REQFlow)
							Me.CopyAmount(si,globals,api,args, REQFlow)
'BRApi.ErrorLog.LogMessage(si, "SAVE INITIAL AMOUNT ")							
							'Clear selections
							Dim paramsToClear As String = "prompt_tbx_REQPRO_AAAAAA_0CaAa_REQTitle__Shared," & _
														"prompt_cbx_REQPRO_AAAAAA_REQListCachedEntity__Shared,"
							Return Me.ClearSelections(si, globals, api, args, paramsToClear)							
						End If
#End Region '(Updated)

#Region "Merge UFRs"
						If args.FunctionName.XFEqualsIgnoreCase("CreateMergedUFR") Then
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
							Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter","")
'brapi.ErrorLog.LogMessage(si,"SEntity on intit = " & sEntity)
							If Not Me.IsUFRApprovalAllowed(si, sEntity, args) Then
								If wfProfileName.XFContainsIgnoreCase("Approve") Then
									Throw New Exception("Cannot merge UFR at this time. Contact UFR manager." & environment.NewLine)
								End If 
							End If
							Return Me.CreateMergedUFR(si,globals,api,args)
						End If
#End Region

#Region "Merge UFR Helper"
						If args.FunctionName.XFEqualsIgnoreCase("MergeUFRHelper")
		
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")	
							Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard","")
							
							BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter",sFundCenter)	
							BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Dashboard",sDashboard)
					
						
							Dim dKeyVal As New Dictionary(Of String, String)
							dKeyVal.Add("prompt_rad_UFRPRO_AAAAAA_0CaAa_SelectToCopy__Shared","No")
							dKeyVal.Add("prompt_lbx_UFRPRO_AAAAAA_0CaAa_UFRsToMerge__Shared","")
							Return Me.SetParameter(si, globals, api, dKeyVal)
							
						End If
#End Region

#Region "Demote REQ Helper"
'						If args.FunctionName.XFEqualsIgnoreCase("DemoteREQHelper")		
'							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")	
'							Dim sREQ As String = args.NameValuePairs.XFGetValue("REQ")
'brapi.ErrorLog.LogMessage(si,"fundcenter = " & sFundCenter)
'brapi.ErrorLog.LogMessage(si,"REQ = " & sREQ)
'							BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","REQ",sREQ)	
'							BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Entity",sFundCenter)	
						
'						End If
#End Region

#Region "Delete REQ"
						If args.FunctionName.XFEqualsIgnoreCase("DeleteREQ") Then	
							 Me.DeleteREQ(si,globals,api,args)

						End If

#End Region '(Updated)

#Region "Release UFR"
						If args.FunctionName.XFEqualsIgnoreCase("ReleaseUFR") Then	
							 Me.ReleaseUFR(si,globals,api,args)
							 Dim paramsToClear As String = "prompt_cbx_UFRPRO_UFRAPR_0CaAa_ApproveUFRs__UpdateFundingStatus," & _
														"prompt_cbx_UFRPRO_AAAAAA_ApproverWFStatus__Shared,"
							 Return Me.ReplaceSelections(si, globals, api, args, paramsToClear)

						End If

#End Region

#Region "DeleteAllFundingRequestsByREQ"
						'Function to remove all funding request values and to provide a default funding request
						If args.FunctionName.XFEqualsIgnoreCase("DeleteAllFundingRequestsByREQ") Then	

							'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
							Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
							Dim sUFRToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")
'Brapi.ErrorLog.LogMessage(si,"sEntity = " & sEntity)	
'Brapi.ErrorLog.LogMessage(si,"sUFRToDelete = " & sUFRToDelete)								
							
							'Add cached parameters to args
							args.NameValuePairs.Add("REQEntity", sEntity)
							args.NameValuePairs.Add("REQ", sUFRToDelete)
							
							'Calls method to clear the rows
							Me.CallDataManagementDeleteUFR(si, globals, api, args)
							'Calls method to add a default funding request
'Brapi.ErrorLog.LogMessage(si,"made it out of delete")								
							Me.SaveInitialAmount(si, globals, api, args, sUFRToDelete)
'Brapi.ErrorLog.LogMessage(si,"made it out of save")								
							'Set updated name and date
							Me.LastUpdated(si, globals, api, args, sUFRToDelete, sEntity)
							 
						End If

#End Region '(Updated)

#Region "Set Related REQs"
						If args.FunctionName.XFEqualsIgnoreCase("SetRelatedREQs") Then	
							 Me.SetRelatedREQs(si,globals,api,args)
						End If
#End Region '(Updated)

#Region "Send Stakeholder Email"
						If args.FunctionName.XFEqualsIgnoreCase("SendStkhldrEmail") Then	
							 Me.SendStatusChangeEmail(si,globals,api,args)
						End If	

#End Region ' update here

#Region "Cache Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("CachePrompts") Then
						     Me.Check_WF_Complete_Lock(si, globals, api, args)	
							 Me.CachePrompts(si,globals,api,args)
						End If
#End Region

#Region "Attach Doc"
'Updated by Kenny & Fronz 09/06/2024 - changed the S# to REQ_Shared and T# to 1999; commented out the passed-in args ClickedScenario & ClickedTime
'Updated by Evan 9/18/2024 - reverting REQ_Shared changes for RMW-1732
						If args.FunctionName.XFEqualsIgnoreCase("AttachDocument") Then
							Me.Check_WF_Complete_Lock(si, globals, api, args)
							Dim CubeName As String = args.NameValuePairs("CubeName")
							Dim ClickedEntity As String = args.NameValuePairs("ClickedEntity")
							Dim ClickedScenario As String = args.NameValuePairs("ClickedScenario")
'brapi.Errorlog.LogMessage(si,"Clickedscenario -= "& ClickedScenario)
							Dim ClickedTime As String = args.NameValuePairs("ClickedTime")
'brapi.Errorlog.LogMessage(si,"ClickedTime -= "& ClickedTime)
							Dim ClickedAccount As String = args.NameValuePairs("ClickedAccount")
							Dim ClickedFlow As String = args.NameValuePairs("ClickedFlow")
							Dim FilePath As String = args.NameValuePairs("FilePath")
							Dim AttachmentTitle As String = args.NameValuePairs("AttachmentTitle")
							'Dim reqShared As String = "REQ_Shared"
							'Dim reqSharedTime As String = "1999"
							Dim canUpdate As Boolean = True
							
							Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
							If Not fileInfo Is Nothing Then                      
								Using dt As DataTable = GetSupportDocDataTableCV(si, True)
								Dim dr As DataRow = dt.NewRow   
								
								dr("UniqueID")  = Guid.NewGuid
								dr("Cube")		= CubeName
								dr("Entity")    = ClickedEntity
								dr("Parent")    = ""
								dr("Cons")		= "USD"
								dr("Scenario")  = ClickedScenario
								dr("Time")		= ClickedTime
								dr("Account")	= ClickedAccount
								dr("Flow")	    = ClickedFlow
								dr("Origin")    = "BeforeAdj"
								dr("IC")		= "None"
								dr("UD1")		= "None" 
								dr("UD2")		= "None"
								dr("UD3")		= "None"
								dr("UD4")		= "None"
								dr("UD5")		= "None"
								dr("UD6")		= "None"
								dr("UD7")		= "None"
								dr("UD8")		= "None"

								dr("Title")					= AttachmentTitle
								dr("AttachmentType")		= "200"
								dr("CreatedUserName")		= si.UserName
								dr("CreatedTimestamp")		= DateTime.UtcNow
								dr("LastEditedUserName")    = si.UserName
								dr("LastEditedTimestamp")   = DateTime.UtcNow
								dr("Text")					= "Yes" 'Sets the cell value for annotation
								dr("FileName")				= fileInfo.XFFile.FileInfo.Name
								
								'Add logic for compression
								dr("FileBytes")				= fileInfo.XFFile.ContentFileBytes
								dt.Rows.Add(dr)
								BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)
								
								'Delete the Loaded File
								BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, FilePath)
								'Set Updated Date and Name
									Try
											Me.LastUpdated(si, globals, api, args, ClickedFlow, ClickedEntity)
											Catch ex As Exception
									End Try	
								End Using
							End If
							
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
#End Region

#Region "Save General Accounts Annotation"

						If args.FunctionName.XFEqualsIgnoreCase("SaveGenAcctAnnotation") Then
							Dim sProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
								
							If sProfileName.Contains(".") Then
								sProfileName = sProfileName.Split(".")(1)
							Else
								Return Nothing
							End If
							
							Me.Check_WF_Complete_Lock(si, globals, api, args)	

							Me.SaveGenAcctAnnotation(si,globals,api,args)
							If sProfileName.XFContainsIgnoreCase("Formulate") Or sProfileName.XFContainsIgnoreCase("Validate") Or sProfileName.XFContainsIgnoreCase("Approve") Then 
								Me.BlankTitleCheck(si,globals,api,args)
							End If 
						End If
#End Region

#Region "Set Notification List"
						If args.FunctionName.XFEqualsIgnoreCase("SetNotificationList") Then	
							 Me.SetNotificationList(si,globals,api,args)
						End If
#End Region

#Region "Show And Hide Dashboards"
						If args.FunctionName.XFEqualsIgnoreCase("ShowAndHideDashboards") Then
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
							Dim ModifiedCustomSubstVars As XFSelectionChangedTaskResult = Nothing
							If wfProfileName.XFContainsIgnoreCase("Approve") Then
								Dim paramsToClear As String = "prompt_cbx_UFRPRO_UFRAPR_0CaAa_ApproveUFRs__UpdateFundingStatus," & _
													"prompt_cbx_UFRPRO_AAAAAA_ApproverWFStatus__Shared," & _
													"ML_REQPRO_FundCenter_Status," & _
												    "ML_REQPRO_APPN," & _ 
												    "ML_REQPRO_MDEP," & _
												    "ML_REQPRO_APE9," & _
												    "ML_REQPRO_SAG," & _
												    "ML_REQPRO_DollarType," & _ 
													"prompt_tbx_REQPRO_AAAAAA_0CaAa_SearchReq__Shared"
						
								ModifiedCustomSubstVars = Me.ShowAndHideDashboards(si,globals,api,args)
								Return Me.ReplaceSelections(si, globals, api, args, paramsToClear, ModifiedCustomSubstVars)
							Else
							Dim paramsToClear As String = "ML_REQPRO_FundCenter_Status," & _
														  "ML_REQPRO_APPN," & _ 
														  "ML_REQPRO_MDEP," & _
														  "ML_REQPRO_APE9," & _
														  "ML_REQPRO_SAG," & _
														  "ML_REQPRO_DollarType," & _ 
														  "prompt_tbx_REQPRO_AAAAAA_0CaAa_SearchReq__Shared"
								
							ModifiedCustomSubstVars = Me.ShowAndHideDashboards(si,globals,api,args)
							Return Me.ReplaceSelections(si, globals, api, args, paramsToClear, ModifiedCustomSubstVars)
								
							'Return Me.ShowAndHideDashboards(si,globals,api,args)
							End If 
						End If
#End Region

#Region "Sub Create Save UFR"
'						If args.FunctionName.XFEqualsIgnoreCase("SubCreateSaveUFR") Then
'							Return Me.SubCreateSaveUFR(si,globals,api,args)
'						End If
#End Region 'commented

#Region "Last Updated Name and Date"
						If args.FunctionName.XFEqualsIgnoreCase("LastUpdated") Then
							Return Me.LastUpdated(si,globals,api,args)
						End If
#End Region

#Region "DemotionCreateUFRValidation"
						If args.FunctionName.XFEqualsIgnoreCase("DemotionCreateUFRValidation") Then						
							 Me.DemotionCreateUFRValidation(si,globals,api,args)

						End If

#End Region

#Region "Demote REQ"
						If args.FunctionName.XFEqualsIgnoreCase("DemoteREQ") Then	
							 Me.DemoteREQ(si,globals,api,args)
						End If
#End Region

#Region "Get Fund Center for Demotion"
						If args.FunctionName.XFEqualsIgnoreCase("GetUFRFundCenterForDemote") Then	
							 Me.GetUFRFundCenterForDemote(si,globals,api,args)

						End If

#End Region

#Region "Recall UFRs for Re-Prioritization"
						'RMW-1012 Recall UFRs for Re-Prioritization 
						If args.FunctionName.XFEqualsIgnoreCase("RePrioritizeUFR") Then	
							 Me.PrioritizeRecall(si,globals,api,args)
						End If

#End Region

#Region "Recall UFRs for Re-Prioritization Helper"
						'RMW-1012 Recall UFRs for Re-Prioritization Helper Function - called by outer button to open PopUp window
						If args.FunctionName.XFEqualsIgnoreCase("RePrioritizeUFRHelper")		
							'set cache prompt for Entity to be used by UFR_DataSet to generate list of UFRs
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")							
							BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Entity",sFundCenter)
							
							'Clear previous selections
							Dim lParam As List(Of String) = args.NameValuePairs.XFGetValue("Param").Split(",").ToList()
							Dim dKeyVal As New Dictionary(Of String, String)
							For Each Param As String In lParam
								dKeyVal.Add(Param,"")
							Next
							
							Return Me.SetParameter(si, globals, api, dKeyVal)
						End If
#End Region

#Region "SelectAll"
						If args.FunctionName.XFEqualsIgnoreCase("SelectAll")						
							Return Me.SelectAll(si,globals,api,args)					
						End If
#End Region

#Region "Clear Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("ClearPrompts") Then
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
BRApi.ErrorLog.LogMessage(si,"test KN")
'							 Me.ClearPrompts(si,globals,api,args)

						End If

#End Region

#Region "Roll Fwd Req"
						If args.FunctionName.XFEqualsIgnoreCase("RollFwdReq") Then	

							'---------------------------------------------------------------------------------------------------
							' PURPOSE: invoke data management to copy requirements from source S#T# to target S#T#
							'
							' LOGIC OVERVIEW:
							'		- if Roll Forward flag = No then exit
							'		- data management sequence = RollFwdReq 
							'
							' USAGE:
							'		- From button:
							'			Sever Task = Execute Dashbaord Extender Business Rule (General Server)
							'			Task Arguments = {REQ_SolutionHelper}{RollFwdReq}{}
							'			
							' MODIFIED: 
							' <date> 		<user id> 	<JIRA ticket> 	<change description>
							' 2024-04-02 	AK 			RMW-1171		created	
							'---------------------------------------------------------------------------------------------------
							
'Brapi.ErrorLog.LogMessage(si,"Sequence should have started")

							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sCurrScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sCurrTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)

							
							'---------- get Entity from WF title ----------
							Dim StringArgs As DashboardStringFunctionArgs = New DashboardStringFunctionArgs
							StringArgs.FunctionName = "GetPrimaryFundCenter"
							StringArgs.NameValuePairs.XFSetValue("Cube", sCube)
							Dim sEntity As String = GEN_General_String_Helper.Main(si, globals, api, StringArgs)


							'---------- if roll over flag is set to NO then exit ----------
							Dim sRllFwdFlag As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, "E#" & sEntity & "_General:P#" & sEntity & ":C#Local:S#" & sCurrScenario & ":T#" & sCurrTime & "M12:V#Annotation:A#REQ_Allow_Rollover:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
'Brapi.ErrorLog.LogMessage(si,"RollFwdReq: E#" & sEntity & "_General:P#" & sEntity & ":C#Local:S#" & sCurrScenario & ":T#" & sCurrTime & "M12:V#Annotation:A#REQ_Allow_Rollover:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
'Brapi.ErrorLog.LogMessage(si,"RollFwdReq: sRllFwdFlag=" & sRllFwdFlag)

							If sRllFwdFlag.XFEqualsIgnoreCase("no") Then
								Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
									selectionChangedTaskResult.IsOK = True
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = "Roll Forward has been disallowed by requirements manager."
								Return selectionChangedTaskResult									
								
							End If	
								
							'---------- run data mgmt sequence ----------
							Dim dataMgmtSeq As String = "RollFwdReq"     
							Dim params As New Dictionary(Of String, String) 
							'params.Add("Scenario", sScenario)
							'params.Add("Time", "T#" & sTime & ".Base")
							'params.Add("Entity", "E#" & sEntity)
'brapi.ErrorLog.LogMessage(si,"here1")					
							BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)

							
							'---------- display done message ----------
							Dim selectionChangedTaskResult2 As New XFSelectionChangedTaskResult()
								selectionChangedTaskResult2.IsOK = True
								selectionChangedTaskResult2.ShowMessageBox = True
								selectionChangedTaskResult2.Message = "Check Task Activity to confirm when Roll Forward is done"
							Return selectionChangedTaskResult2									
						End If
#End Region

#Region "Save All Components Helper"
						If args.FunctionName.XFEqualsIgnoreCase("SaveAllHelper") Then
							Me.Check_WF_Complete_Lock(si, globals, api, args)
							Return Me.SaveAllHelper(si,globals,api,args)
						End If
#End Region

#Region "Validate REQs"
'RMW-1708: KN: Allow submission by filtered result
						If args.FunctionName.XFEqualsIgnoreCase("ValidateREQs") Then
							Try
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not Me.IsREQValidationAllowed(si, args) Then
									Throw New Exception("Cannot validate requirement at this time. Contact requirements manager.")
								End If
								Return Me.ValidateREQs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "SubmitAllToValidation"
'RMW-739: MH: Allow submission by filtered result
						If args.FunctionName.XFEqualsIgnoreCase("SubmitAllToValidation") Then
							Try
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								Return Me.SubmitAllToValidation(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Approve REQs from Validate "
'RMW-1708: KN: Allow submission by filtered result
						If args.FunctionName.XFEqualsIgnoreCase("ApprovalREQsVal") Then
							Try
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not Me.IsREQValidationAllowed(si, args) Then
									Throw New Exception("Cannot approve requirement at this time. Contact requirements manager.")
								End If
								Return Me.ApprovalREQsVal(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Add REQ Account Value"
'RMW-1224: KN: Add REQ Account Value for Dropdown Lists
						If args.FunctionName.XFEqualsIgnoreCase("AddREQAcctValue") Then
							Try
								Me.AddREQAcctValue(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Delete REQ Account Value"
'RMW-1224: KN: Delete REQ Account Value for Dropdown Lists
						If args.FunctionName.XFEqualsIgnoreCase("DeleteREQAcctValue") Then
							Try
								Me.DeleteREQAcctValue(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Import REQ"
'RMW-1224: KN: Delete REQ Account Value for Dropdown Lists
						If args.FunctionName.XFEqualsIgnoreCase("ImportREQ") Then
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
									Me.ImportREQ(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region 


#Region "FC Import REQ"
'RMW-1224: KN: Delete REQ Account Value for Dropdown Lists
'						If args.FunctionName.XFEqualsIgnoreCase("FCImportREQ") Then
'							Try
'								Me.Check_WF_Complete_Lock(si, globals, api, args)
'								Me.FCImportREQ(si,globals,api,args)
'								Catch ex As Exception
'								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
'							End Try
'						End If
#End Region 


#Region "Delete Data Attachement"

						If args.FunctionName.XFEqualsIgnoreCase("DeleteDataAttachement") Then
							Try
'								Me.Check_WF_Complete_Lock(si, globals, api, args)
								Me.DeleteDataAttachement(si,globals,api,args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region 

#Region "CreateManpowerREQ"
						If args.FunctionName.XFEqualsIgnoreCase("CreateManpowerREQ") Then
							Try
								Me.CreateManpowerREQ(si, globals, api, args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "UpdateManpowerREQStatus"
						If args.FunctionName.XFEqualsIgnoreCase("UpdateManpowerREQStatus") Then
							Try
								Me.UpdateManpowerREQStatus(si, globals, api, args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region


#Region "Export"
						'Export PGM Requirement Data to be used as import
						If args.FunctionName.XFEqualsIgnoreCase("ExportReport") Then
							Try								
								Return Me.ExportReport(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Set Default PEG"
						'Set Default PEG for Requirements Export
						If args.FunctionName.XFEqualsIgnoreCase("SetDefaultPEG") Then	
							 Return Me.SetDefaultPEG(si,globals,api,args)
						End If
#End Region
						
#Region "IsFilterSelected"
							If args.FunctionName.XFEqualsIgnoreCase("IsFilterSelected") Then
								Me.IsFilterSelected(si,args)
								Return Me.LoadPages(si,args)
							End If
#End Region

#Region "ResetTitleFilter"
							If args.FunctionName.XFEqualsIgnoreCase("ResetTitleFilter") Then
								'aaaUIX_SolutionHelper.ResetParameters(si, globals, api, args)
								'Return Me.LoadPages(si, args)
							End If
#End Region


#Region "Export All Requirements"
						'Export PGM Requirements to Excel
						If args.FunctionName.XFEqualsIgnoreCase("ExportAllREQs") Then
							Try								
								Return Me.ExportAllREQs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "REQ_UpdateFilterLists"
						If args.FunctionName.XFEqualsIgnoreCase("REQ_UpdateFilterLists") Then
							

							'----- get req list -----
							Dim scbxEntity As String = args.NameValuePairs.XFGetValue("cbxEntity")
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Fundcenter")
							brapi.ErrorLog.LogMessage(si,"Hit: " & scbxEntity & "-" & sFundCenter)
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Go get list")
							Dim ListOfREQs As list(Of MemberInfo) =  BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=REQ_Member_Lists, MemberListName=GetREQListByStatus, Caller=REQ_SolutionHelper, mode=CVResult, ReturnDim=E#F#U1#U2#U3#U4#, FlowFilter=[Command_Requirements.Base.Where(Name doesnotcontain REQ_00)], cbxEntity = " & scbxEntity & ", Page= , EntityFilter=[" & sFundCenter & "] )"  , False)
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   scbxEntity=" & scbxEntity & "   ListOfREQs count=" & ListOfREQs.Count)

Dim tStart As DateTime =  Date.Now()							
							
							Dim lsU1List As New List(Of String)
							Dim lsU2List As New List(Of String)
							Dim lsU3List As New List(Of String)
							Dim lsU4List As New List(Of String)
							Dim lsSAGList As New List(Of String)
							
							Dim objDictionary As Dictionary(Of String, String) = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
							Dim U3DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
							If sFundCenter.Trim <> "" Then	
 'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Load Lists")								
 								
								For Each ReqMbrInfo As MemberInfo In ListOfREQs
									
									Dim sU1 As String = "U1#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
									Dim sU2 As String = "U2#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD2.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD2Id).Member.Name
									Dim sU3 As String = "U3#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
									Dim sU4 As String = "U4#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD4.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD4Id).Member.Name
									Dim U3ParentNameL4 As String = BRApi.Finance.Members.GetParents(si,U3DimPk, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id, False,)(0).Name
									Dim U3ParentMemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD3.Id, U3ParentNameL4)
									Dim U3ParentNameL3 As String = BRApi.Finance.Members.GetParents(si, U3DimPk,U3ParentMemberID,False,)(0).Name
									
									Dim sSAG As String = "U3#" & U3ParentNameL3
									
									If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
									If Not lsU2List.Contains(sU2) Then lsU2List.Add(sU2)
									If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
									If Not lsU4List.Contains(sU4) Then lsU4List.Add(sU4)
									If Not lsSAGList.Contains(sSAG) Then lsSAGList.Add(sSAG)
									
								Next	
'brapi.ErrorLog.LogMessage(si, "ListOfU1s=" & String.Join(",", lsU1List))
'brapi.ErrorLog.LogMessage(si, "ListOfU2s=" & String.Join(",", lsU2List))
'brapi.ErrorLog.LogMessage(si, "ListOfU3s=" & String.Join(",", lsU3List))
'brapi.ErrorLog.LogMessage(si, "ListOfU4s=" & String.Join(",", lsU4List))
'brapi.ErrorLog.LogMessage(si, "lsSAGList=" & String.Join(",", lsSAGList))							

								objDictionary.item("var_cbx_U1_List") = String.Join(",", lsU1List)
								objDictionary.item("var_cbx_U2_List") = String.Join(",", lsU2List)
								objDictionary.item("var_cbx_U3_List") = String.Join(",", lsU3List)
								objDictionary.item("var_cbx_U4_List") = String.Join(",", lsU4List)
								objDictionary.item("var_cbx_SAG_List") = String.Join(",", lsSAGList)


							Else
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Clear Lists")								
								objDictionary.item("var_cbx_U1_List") = "None"
								objDictionary.item("var_cbx_U2_List") = "None"
								objDictionary.item("var_cbx_U3_List") = "None"
								objDictionary.item("var_cbx_U4_List") = "None"
								objDictionary.item("var_cbx_SAG_List") = "None"
								
							End If	
								
								
'							'---- load cbx page ----
'							Dim sRowsPerPage As String = args.NameValuePairs.XFGetValue("RowsPerPage")
'							Dim iRowsPerPage As Integer = 25
'							If sRowsPerPage <> "" Then iRowsPerPage = CInt(sRowsPerPage)
								
'							Dim iPages As Integer = 0	
'							If 	ListOfREQs.Count > iRowsPerPage Then iPages = Math.Truncate(ListOfREQs.Count / iRowsPerPage)
'							If 	(ListOfREQs.Count Mod iRowsPerPage) > 0 Then iPages += 1
''If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   iRowsPerPage=" & iRowsPerPage & "    ListOfREQs.Count=" & ListOfREQs.Count & "   iPages=" & iPages)									
								
'							Dim lsPageList As New List(Of String)
'							For iPage As Integer = 1 To iPages
'								lsPageList.Add(iPage.ToString)
'							Next
'							objDictionary.item("var_cbx_Paging_List") = String.Join(",", lsPageList)
							
							objDictionary.item("prompt_cbx_Paging") = "1"
							
							
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
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   " & Date.Now().ToString("hh:mm:ss:fff") & " REQ_UpdateFilterLists took: " & Date.Now().Subtract(tStart).TotalSeconds.ToString("0.0000"))									
							Return selectionChangedTaskResult							


						End If
#End Region


#Region "REQ_UpdateFilterLists Rollover"
						If args.FunctionName.XFEqualsIgnoreCase("REQ_UpdateFilterListsRollOver") Then
							
Dim tStart As DateTime =  Date.Now()							

							'----- get req list -----
							Dim origcbxEntity As String = args.NameValuePairs.XFGetValue("cbxEntity")
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Fundcenter")
							'---------- get Entity in proper format ----------
							Dim StringArgs As DashboardStringFunctionArgs = New DashboardStringFunctionArgs
							StringArgs.FunctionName = "Remove_General"
							StringArgs.NameValuePairs.XFSetValue("Entity", origcbxEntity)
							Dim scbxEntity As String = GEN_General_String_Helper.Main(si, globals, api, StringArgs)					
							Dim ListOfREQs As List(Of MemberInfo) =  BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=REQ_Member_Lists, MemberListName=GetAllReqListRolloverCBX, TimeOffset=Prior, Caller=REQ_SolutionHelperRollOver, test=testing,FlowMbr=Top, mode=CVResult, ReturnDim=E#F#U1#U2#U3#U4#, cbxEntity = " & scbxEntity & ".Base, Page= , EntityFilter=[" & sFundCenter & "] )"  , False)

							If ListOfREQs.Count = 0 Then
								Return Nothing
							End If

#Region "no longer used"						
'							Dim lsU1List As New List(Of String)
'							Dim lsU2List As New List(Of String)
'							Dim lsU3List As New List(Of String)
'							Dim lsU4List As New List(Of String)
'							Dim lsSAGList As New List(Of String)
'							Dim U3DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
'							Dim objDictionary As Dictionary(Of String, String) = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
							
				
						
'							If sFundCenter.Trim <> "" Then	
' 'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Load Lists   sFundCenter=" & sFundCenter)								
								
' 'BRAPI.ErrorLog.LogMessage(si, "ListOfREQs.count: " & ListOfREQs.Count & " | ListOfREQs items: " & ListOfREQs.Item(0).ToString & vbCrLf & ListOfREQs.Item(1).ToString & vbCrLf & ListOfREQs.Item(2).ToString & vbCrLf & ListOfREQs.Item(3).ToString)
'' BRAPI.ErrorLog.LogMessage(si, "ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id: " & ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id)
''  BRAPI.ErrorLog.LogMessage(si, "ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD2Id: " & ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD2Id)
'' BRAPI.ErrorLog.LogMessage(si, "ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id member name (BREAKS) " & BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, ListOfREQs.Item(1).RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name)
 
'								For Each ReqMbrInfo As MemberInfo In ListOfREQs		
'BRAPI.ErrorLog.LogMessage(si, "ReqMbrInfo.Member.Name: " & ReqMbrInfo.Member.Name)
'								Next
 			
 
'								For Each ReqMbrInfo As MemberInfo In ListOfREQs		
'									Dim sU1 As String = "U1#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
'									Dim sU2 As String = "U2#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD2.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD2Id).Member.Name
'									Dim sU3 As String = "U3#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
'									Dim sU4 As String = "U4#" & BRApi.Finance.Metadata.GetMember(si, dimType.UD4.Id, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD4Id).Member.Name
									
'								'	Dim sU1alt As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, ReqMbrInfo.).Member.Name
									

'									Dim U3ParentNameL4 As String = BRApi.Finance.Members.GetParents(si,U3DimPk, ReqMbrInfo.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id, False,)(0).Name
'									Dim U3ParentMemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD3.Id, U3ParentNameL4)
'									Dim U3ParentNameL3 As String = BRApi.Finance.Members.GetParents(si, U3DimPk,U3ParentMemberID,False,)(0).Name
									
'									Dim sSAG As String = "U3#" & U3ParentNameL3
									
													
									
'									If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
'									If Not lsU2List.Contains(sU2) Then lsU2List.Add(sU2)
'									If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
'									If Not lsU4List.Contains(sU4) Then lsU4List.Add(sU4)
'									If Not lsSAGList.Contains(sSAG) Then lsSAGList.Add(sSAG)
									
'								Next	
''brapi.ErrorLog.LogMessage(si, "ListOfU1s=" & String.Join(",", lsU1List))
''brapi.ErrorLog.LogMessage(si, "ListOfU2s=" & String.Join(",", lsU2List))
''brapi.ErrorLog.LogMessage(si, "ListOfU3s=" & String.Join(",", lsU3List))
''brapi.ErrorLog.LogMessage(si, "ListOfU4s=" & String.Join(",", lsU4List))
''brapi.ErrorLog.LogMessage(si, "lsSAGList=" & String.Join(",", lsSAGList))							

'								objDictionary.item("var_cbx_U1_List_RollOver") = String.Join(",", lsU1List)
'								objDictionary.item("var_cbx_U2_List_RollOver") = String.Join(",", lsU2List)
'								objDictionary.item("var_cbx_U3_List_RollOver") = String.Join(",", lsU3List)
'								objDictionary.item("var_cbx_U4_List_RollOver") = String.Join(",", lsU4List)
'								objDictionary.item("var_cbx_SAG_List_RollOver") = String.Join(",", lsSAGList)


'							Else
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Clear Lists   sFundCenter=" & sFundCenter)	
'BRAPI.ErrorLog.LogMessage(si, "REQ_SolutionHelper HIT 970")
'								objDictionary.item("var_cbx_U1_List_RollOver") = "None"
'								objDictionary.item("var_cbx_U2_List_RollOver") = "None"
'								objDictionary.item("var_cbx_U3_List_RollOver") = "None"
'								objDictionary.item("var_cbx_U4_List_RollOver") = "None"
'								objDictionary.item("var_cbx_SAG_List_RollOver") = "None"

'							End If	
															
''							'---- load cbx page ----
''							Dim sRowsPerPage As String = args.NameValuePairs.XFGetValue("RowsPerPage")
''							Dim iRowsPerPage As Integer = 25
''							If sRowsPerPage <> "" Then iRowsPerPage = CInt(sRowsPerPage)
								
''							Dim iPages As Integer = 0	
''							If 	ListOfREQs.Count > iRowsPerPage Then iPages = Math.Truncate(ListOfREQs.Count / iRowsPerPage)
''							If 	(ListOfREQs.Count Mod iRowsPerPage) > 0 Then iPages += 1
'''If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   iRowsPerPage=" & iRowsPerPage & "    ListOfREQs.Count=" & ListOfREQs.Count & "   iPages=" & iPages)									
								
''							Dim lsPageList As New List(Of String)
''							For iPage As Integer = 1 To iPages
''								lsPageList.Add(iPage.ToString)
''							Next
''							objDictionary.item("var_cbx_Paging_List") = String.Join(",", lsPageList)
							
'							objDictionary.item("prompt_cbx_Paging") = "1"
							
							
'							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
'								selectionChangedTaskResult.IsOK = True
'								selectionChangedTaskResult.ShowMessageBox = False
'								selectionChangedTaskResult.Message = ""
'								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
'								selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing 'objXFSelectionChangedUIActionInfo
'								selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
'								selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
'								selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
'								selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
'								selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
'								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary
''If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   " & Date.Now().ToString("hh:mm:ss:fff") & " REQ_UpdateFilterLists took: " & Date.Now().Subtract(tStart).TotalSeconds.ToString("0.0000"))									
'							Return selectionChangedTaskResult							
#End Region

						End If
#End Region

#Region "Export All Updated Requirements"
						'Export PGM Requirements to Excel
						If args.FunctionName.XFEqualsIgnoreCase("ExportAllUpdatedREQs") Then
							Try								
								Return Me.ExportAllUpdatedREQs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Import/Update REQ"
'RMW-1265 - Created by JM/KL for new update REQ Import
						If args.FunctionName.XFEqualsIgnoreCase("ImportUpdateREQ") Then
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
									Me.ImportREQForUpdate(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region 



				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

'END MAIN =================================================================================================

#Region "Constants"
	'Public UFR_String_Helper As New OneStream.BusinessRule.DashboardStringFunction.UFR_String_Helper.MainClass	
	Private BR_REQDataSet As New Workspace.CMD_PGM.CMD_PGM_Assembly.BusinessRule.DashboardDataSet.CMD_PGM_DataSet.MainClass()
	Public GEN_General_String_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardStringFunction.GBL_General_String_Helper.MainClass	
	'Public aaaUIX_SolutionHelper As New Workspace.CMD_PGM.CMD_PGM_Assembly.BusinessRule.DashboardExtender.aaaUIX_SolutionHelper.MainClass
	
	
	
#End Region

#Region "REQ Mass Import"
		'This rule reads the imported file chcks if it is readable then parses into the REQ class
		'It then validate if the title is blank and the validity of Fund Code, MDEP, APE and Dollar Type
		'If there are errors, it will spit out the first 10 errors. That is because there could be thousands of records
		'If it is all valid, it will create a REQ_Id and Flow
		'It clears the  XFC_REQ_Import table for the Command scenario combination and saves it in the table. 
		'After that it loads the cube with the FYDP and narrative accounts
		'The mapping is shown below.
#Region "file structure"
		'-----Mapping definitions----
'			Entity
'			APPN (Fundcode)
'			MDEP
'			APE9
'			DollarType
'			Cycle
'			FY1
'			FY2
'			FY3
'			FY4
'			FY5
'			Title
'			Description
'			RequestedFundSource
'			DirectivePolicy
'			AssessedRisk
'			RiskLevel
'			SeniorLeaderPriority
'			PEGStragegyPriority
'			StrategicInitiative
'			RequirementType
'			Readiness
'			AreaOfOperations
'			Office
'			NotificationEmailList
'			FlexField1
'			FlexField2
'			FlexField3
'			FlexField4
'			FlexField5
'			ITCyberReq
'			CFLValidated
'			Contingency
'			EmergingRequirement
'			CPACandidate
'			Directorate
'			AssociateDirectorate
'			Division
'			Branch
'			OwnerName
'			OwnerEmail
'			OwnerPhone
'			OwnerComment
'			MDEPFuncName
'			MDEPFuncEmail
'			MDEPFuncPhone
'			MDEPFuncComment
'			TypeOfContract
'			ContractNumber
'			TaskOrder
'			AwardTargetDate
'			POPExpirationDate
'			WorkYearsReq
'			ContractValue
'			COR
'			CME
'			COREmail
'			CORPhone
'			OperationalImpact
'			Justification
'			CostMethodologyCmt
'			UnfundedMitigation
'           UIC
'		    FunctionalPriority 
'		    CommitmentGroup 
'		    Capability 
'		    StrategicBIN 
'		    LIN 
			'flow 				'position None. Calculated
			'REQ_ID 			'position None. Calculated			
		'-----------------------------
		'***If the import file structure changes this rule has to be updated.
		'***Assumption is that thereare no commas and | in the file. The file is comma delimeted
		'
#End Region

#Region "Import REQ Function Process"

		Public Function	ImportREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							

			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				
'Dim tStart1 As DateTime =  Date.Now()			
#Region "Validate File structure"			
			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
			Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
'BRApi.ErrorLog.LogMessage(si, "FolderPath: " & filePath & ", Name: " & objXFFileInfo.Name)			
			If objXFFileInfo Is Nothing
				Me.REQMassImportFileHelper(si, globals, api, args, "File " & objXFFileInfo.Name & " does NOT exist", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(String.Concat("File " & objXFFileInfo.Name & " does NOT exist")))
			End If

			'Confirm source file is readable
			Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)			
			If objXFFileEx Is Nothing
				Me.REQMassImportFileHelper(si, globals, api, args, "Unable to open file " & objXFFileInfo.Name, "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(String.Concat( "Unable to open file " & objXFFileInfo.Name)))
			End If

			'Read the content of the file
			'TO LOOK INTO: this approach picks up the whole data once into memory. If the file is too large check here for performance issue.
			'May be a file streamer to read a line at a time may be better
			Dim sFileText As String = system.Text.Encoding.ASCII.GetString(objXFFileEX.XFFile.ContentFileBytes)                                                                                   
			
			'Clean up CRLF from the file
			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfCube).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
			'Dim patt As String = vbCrLf & "(?!" & comd & ")|[&']" '-- exclude what is in the brackt
			'Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")

			Dim patt As String = "(" & vbCrLf & "|" & vbLf & ")(?!" & comd & ")"'(?=[a-zA-Z0-9,""@!#$%'()+=-_.:<>?~^]*)" '-- Include what is in the brackt
			Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")
			
			'Dim patt2 As String = "[^a-zA-Z0-9,""@!#$%'()+=-_.:<>?~/\\\[" & vbcrlf & vbLf & "]"
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
				
'				Next

			'***********Split will need to be replace with alternate solution to handle CRLF issue*******
			'Dim lines As String() = sFileText.Split(vbCRLF) 
			Dim lines As String() = cleanedText.Split(vbCRLF)
			
			If lines.Count < 1 Then 
				Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is empty", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is empty"))
			End If
			
			'For performance we are capping the upload file to not more than 5000
			If lines.Count > 5001 Then 
				Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows"))
			End If

			Dim firstLine As Boolean = True
			'Dim currentUsedFlows As List(Of String) = New List(Of String)
			Dim REQs As List (Of REQ) = New List (Of REQ)
			Dim isFileValid As Boolean = True
			Dim iLineCount As Integer = 0
			
			'Loop through each line and process
			For Each line As String In lines
				iLineCount += 1

				'Skip empty line
				If String.IsNullOrWhiteSpace(line) Then
					Continue For
				End If

				'If there are back to back (escaped) double quotes, they will be replaced with single quotes.
				'This is done becasue "s are used as column separator in csv files and "s inside would be represented as escaped quotes ("")
				line = line.Replace("""""", "'")

'BRApi.ErrorLog.LogMessage(si, "Line : " & line)
				'Use reg expressions to split the csv.
				'The expression accounts for commas that are with in "" to treat them as data.
				Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
				Dim fields As String () = Regex.Split(line, pattern)
				
				'Check number of column and skip first line
				If firstLine Then
					'There needs to be 68 columns
					If fields.Length <> 92 Then
						Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 92, number columns in file header is "& fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 92, number columns in file header is "& fields.Length & vbCrLf & line ))
					End If
					
					firstLine = False
					Continue For
				End If
#End Region

#Region "Parse file"
				'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
				Dim currREQ As REQ = Me.ParseREQ(si, fields)
				'If EntityDescriptorType is a parent, add _General
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.Entity & ".base", True)
				If membList.Count > 1 Then
					currREQ.Entity = currREQ.Entity & "_General"
				End If
				
				'====== Get APPN_FUND And PARENT APPN_L2 ======
					Dim U1Name As String = currREQ.FundCode				
					Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
					Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 	
					Dim U3Concat As String = U1ParentName & "_" & currREQ.APE9
					currREQ.APE9 = U3Concat
				
				
				
#End Region


#Region "Validate members"

				'validate req. If there is one error, then the file will be considered invalid and would not be loaded intot he cube
				'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				sScenario = "PGM_C" & currREQ.Cycle
				If Not sScenario.XFEqualsIgnoreCase(wfScenario) Then 
					Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is not targeting the current workflow year", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is not targeting the current workflow year"))
				End If
			
			
				Dim currREQVlidationRes As Boolean = False
				currREQVlidationRes = Me.ValidateMembers(si,currREQ)
				If isFileValid Then 
					isFileValid = currREQVlidationRes
'BRApi.ErrorLog.LogMessage(si, "Error line : " & currREQ.StringOutput())					
				End If
								
				'Add REQ tot the REQ list
				REQs.Add(currREQ)
				
'BRApi.ErrorLog.LogMessage(si, "Added REQ =  " & currREQ.title & ", valid= " & currREQ.valid & ", msg= " & currREQ.validationError)				
			Next
#End Region
'brapi.ErrorLog.LogMessage(si,"VALIDATEandPARSEtimespent=" & Date.Now().subtract(tStart1).TotalSeconds.ToString("0.0000"))

'Dim tStart2 As DateTime =  Date.Now()
#Region "Populate table XFC_REQ_Import, Cube and Annotation"
			
			'Prior to starting to load data, clear pre-existing rows
			Dim SQLClearREQ As String = "Delete from XFC_REQ_Import  
											Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "'"
			'Dim SQLClearREQ As String = "Truncate table [dbo].[XFC_REQ_Import]"
			
'BRApi.ErrorLog.LogMessage(si,"SQLClearREQ = " & SQLClearREQ)

			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearREQ.ToString, False, True)
			End Using
		
			'Find the list of existing flows
			Dim ExistingREQs As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).Select(Function(n) n.Member.Name).ToList()
			ExistingREQs.OrderBy(Function(x) convert.ToInt32(x.split("_")(1))).ToList()
			Dim currentUsedFlows As List(Of String) = Me.GetUsedFlows(si, wfCube, wfScenario)
			
			'loop through all parsed requirements list
			For Each currREQ As REQ In REQs
				Me.PopulateStageTable(si, globals, api, currREQ, isFileValid, ExistingREQs, currentUsedFlows)
				
				If isFileValid Then
					Me.PopulateCube(si, currREQ)
					Me.PopulateAnnotation(si, currREQ)
				End If
			Next
#End Region
'brapi.ErrorLog.LogMessage(si,"populateTimeSpent=" & Date.Now().subtract(tStart2).TotalSeconds.ToString("0.0000"))


			'If the validation failed, write the error out.
			'If there are more than ten, we show only the first ten messages for the sake of redablity
			Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
			If Not isFileValid Then
				Dim sErrorLog As String = ""
				For Each req In REQs
					sErrorLog = sErrorLog & vbCrLf & req.StringOutput()
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

				Me.REQMassImportFileHelper(si, globals, api, args, sCompletionMessageFail, "FAIL", objXFFileInfo.Name)
				
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

			Me.REQMassImportFileHelper(si, globals, api, args, sCompletionMessagePass, "PASS", objXFFileInfo.Name)
			'Throw New Exception("IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper)
			Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)



		Return Nothing
		End Function			
#End Region 

#End Region

#Region "Import Helpers"

#Region "Parse REQ"
		'Parse a line into REQ object and return
		Public Function	ParseREQ(ByVal si As SessionInfo, ByVal fields As String())As Object
			'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
			Dim currREQ As REQ = New REQ
			'Trim any unprintable character and surrounding quotes
			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
			Else 
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
			End If 
			currREQ.Entity = fields(0).Trim().Trim("""")
			currREQ.FundCode = fields(1).Trim().Trim("""") & "_General"
			currREQ.MDEP = fields(2).Trim().Trim("""")
			currREQ.APE9 = fields(3).Trim().Trim("""")
			currREQ.DollarType = fields(4).Trim().Trim("""")
			currREQ.CommitmentItem = fields(5).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.CommitmentItem)
				currREQ.CommitmentItem = "None"
			End If 
			currREQ.sCtype = fields(6).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.sCType)
				currREQ.sCType = "None"
			End If 
			currREQ.Cycle = fields(7).Trim().Trim("""")
			currREQ.FY1 = fields(8).Trim().Trim("""")
			currREQ.FY2 = fields(9).Trim().Trim("""")
			currREQ.FY3 = fields(10).Trim().Trim("""")
			currREQ.FY4 = fields(11).Trim().Trim("""")
			currREQ.FY5 = fields(12).Trim().Trim("""")
			currREQ.Title = fields(13).Trim().Trim("""")
			currREQ.Description  = fields(14).Trim().Trim("""")
			currREQ.Justification  = fields(15).Trim().Trim("""")
			currREQ.CostMethodology = fields(16).Trim().Trim("""")
			currREQ.ImpactifnotFunded = fields(17).Trim().Trim("""")
			currREQ.RiskifnotFunded = fields(18).Trim().Trim("""")
			currREQ.CostGrowthJustification = fields(19).Trim().Trim("""")
			currREQ.MustFund = fields(20).Trim().Trim("""")
			currREQ.FundingSource  = fields(21).Trim().Trim("""")
			currREQ.ArmyInitiative_Directive = fields(22).Trim().Trim("""")
			currREQ.CommandInitiative_Directive = fields(23).Trim().Trim("""")
			currREQ.Activity_Exercise = fields(24).Trim().Trim("""")
			currREQ.IT_CyberRequirement  = fields(25).Trim().Trim("""")
			currREQ.UIC = fields(26).Trim().Trim("""")
			currREQ.FlexField1  = fields(27).Trim().Trim("""")
			currREQ.FlexField2  = fields(28).Trim().Trim("""")
			currREQ.FlexField3  = fields(29).Trim().Trim("""")
			currREQ.FlexField4  = fields(30).Trim().Trim("""")
			currREQ.FlexField5  = fields(31).Trim().Trim("""")
			currREQ.EmergingRequirement = fields(32).Trim().Trim("""")
			currREQ.CPATopic = fields(33).Trim().Trim("""")
			currREQ.PBRSubmission = fields(34).Trim().Trim("""")
			currREQ.UPLSubmission = fields(35).Trim().Trim("""")
			currREQ.ContractNumber  = fields(36).Trim().Trim("""")
			currREQ.TaskOrderNumber  = fields(37).Trim().Trim("""")
			currREQ.AwardTargetDate  = fields(38).Trim().Trim("""")
			currREQ.POPExpirationDate  = fields(39).Trim().Trim("""")
			currREQ.ContractorManYearEquiv_CME  = fields(40).Trim().Trim("""")
			currREQ.COREmail  = fields(41).Trim().Trim("""")
			currREQ.POCEmail = fields(42).Trim().Trim("""")
			currREQ.Directorate  = fields(43).Trim().Trim("""")
			currREQ.Division  = fields(44).Trim().Trim("""")
			currREQ.Branch = fields(45).Trim().Trim("""")
			currREQ.ReviewingPOCEmail = fields(46).Trim().Trim("""")
			currREQ.MDEPFunctionalEmail = fields(47).Trim().Trim("""")
			currREQ.NotificationListEmails = fields(48).Trim().Trim("""")
			currREQ.GeneralComments_Notes = fields(49).Trim().Trim("""")
			currREQ.JUON = fields(50).Trim().Trim("""")
			currREQ.ISR_Flag = fields(51).Trim().Trim("""")
			currREQ.Cost_Model = fields(52).Trim().Trim("""")
			currREQ.Combat_Loss = fields(53).Trim().Trim("""")
			currREQ.Cost_Location = fields(54).Trim().Trim("""")
			currREQ.Category_A_Code = fields(55).Trim().Trim("""")
			currREQ.CBS_Code = fields(56).Trim().Trim("""")
			currREQ.MIP_Proj_Code = fields(57).Trim().Trim("""")
			currREQ.SS_Priority = fields(58).Trim().Trim("""")
			currREQ.Commitment_Group = fields(59).Trim().Trim("""")
			currREQ.SS_Capability = fields(60).Trim().Trim("""")
			currREQ.Strategic_BIN = fields(61).Trim().Trim("""")
			currREQ.LIN = fields(62).Trim().Trim("""")
			currREQ.FY1_QTY = fields(63).Trim().Trim("""")
			currREQ.FY2_QTY = fields(64).Trim().Trim("""")
			currREQ.FY3_QTY = fields(65).Trim().Trim("""")
			currREQ.FY4_QTY = fields(66).Trim().Trim("""")
			currREQ.FY5_QTY = fields(67).Trim().Trim("""")
			currREQ.RequirementType = fields(68).Trim().Trim("""")
			currREQ.DD_Priority = fields(69).Trim().Trim("""")
			currREQ.Portfolio = fields(70).Trim().Trim("""")
			currREQ.DD_Capability = fields(71).Trim().Trim("""")
			currREQ.JNT_CAP_AREA = fields(72).Trim().Trim("""")
			currREQ.TBM_COST_POOL = fields(73).Trim().Trim("""")
			currREQ.TBM_TOWER = fields(74).Trim().Trim("""")
			currREQ.APMSAITRNum = fields(75).Trim().Trim("""")
			currREQ.ZERO_TRUST_CAPABILITY = fields(76).Trim().Trim("""")
			currREQ.ASSOCIATED_DIRECTIVES = fields(77).Trim().Trim("""")
			currREQ.CLOUD_INDICATOR = fields(78).Trim().Trim("""")
			currREQ.STRAT_CYBERSEC_PGRM = fields(79).Trim().Trim("""")
			currREQ.NOTES = fields(80).Trim().Trim("""")
			currREQ.UNIT_OF_MEASURE = fields(81).Trim().Trim("""")
			currREQ.FY1_ITEMS = fields(82).Trim().Trim("""")
			currREQ.FY1_UNIT_COST = fields(83).Trim().Trim("""")
			currREQ.FY2_ITEMS = fields(84).Trim().Trim("""")
			currREQ.FY2_UNIT_COST = fields(85).Trim().Trim("""")
			currREQ.FY3_ITEMS = fields(86).Trim().Trim("""")
			currREQ.FY3_UNIT_COST = fields(87).Trim().Trim("""")
			currREQ.FY4_ITEMS = fields(88).Trim().Trim("""")
			currREQ.FY4_UNIT_COST = fields(89).Trim().Trim("""")
			currREQ.FY5_ITEMS = fields(90).Trim().Trim("""")
			currREQ.FY5_UNIT_COST = fields(91).Trim().Trim("""")
			
			
			Return currREQ
			
		End Function
#End Region

#Region "Validate Members"		
		Public Function	ValidateMembers(ByVal si As SessionInfo, ByRef currREQ As REQ) As Object
			
			'validate fund code
			'This code leverages the way we validate in Data Source
			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
			
			Dim isFileValid As Boolean = True
			
			If String.IsNullOrWhiteSpace(currREQ.title) Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Blank Title value in record"
			End If
			
			'Validate that the Fund Center being loaded  s with in the command
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
			Dim currEntity As String = currREQ.Entity
			If Not membList.Exists(Function(x) x.Member.Name = currEntity) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.fundCode & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.fundCode
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & currREQ.MDEP & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#" & currREQ.APE9.Trim & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.APE9
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid APE value: " & SAG_APE))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U4#" &currREQ. DollarType & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Dollar Type value: " & currREQ.DollarType
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.sCType) Or currREQ.sCType.XFEqualsIgnoreCase("None") Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U5_CTYPE")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U5#" &currREQ. sCType & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid CType value: " & currREQ.sCType
					
				End If
	
			End If 
			If Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem) Or currREQ.CommitmentItem.XFEqualsIgnoreCase("None")Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
					
				End If
			End If 
			
			'Validate Numeric
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1)) And (Not IsNumeric(currREQ.FY1))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1 should be Numeric: " & currREQ.FY1
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2)) And (Not IsNumeric(currREQ.FY2))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2 should be Numeric: " & currREQ.FY2
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3)) And ( Not IsNumeric(currREQ.FY3))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3 should be Numeric: " & currREQ.FY3
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4)) And ( Not IsNumeric(currREQ.FY4))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4 should be Numeric: " & currREQ.FY4
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5)) And ( Not IsNumeric(currREQ.FY5))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5 should be Numeric: " & currREQ.FY5
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1_QTY)) And ( Not IsNumeric(currREQ.FY1_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1_QTY should be Numeric: " & currREQ.FY1_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2_QTY)) And ( Not IsNumeric(currREQ.FY2_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2_QTY should be Numeric: " & currREQ.FY2_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3_QTY)) And ( Not IsNumeric(currREQ.FY3_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3_QTY should be Numeric: " & currREQ.FY3_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4_QTY)) And ( Not IsNumeric(currREQ.FY4_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4_QTY should be Numeric: " & currREQ.FY4_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5_QTY)) And ( Not IsNumeric(currREQ.FY5_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5_QTY should be Numeric: " & currREQ.FY5_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.ContractorManYearEquiv_CME)) And ( Not IsNumeric(currREQ.ContractorManYearEquiv_CME))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: ContractorManYearEquiv_CME should be Numeric: " & currREQ.ContractorManYearEquiv_CME
				
			End If
			
			
			'We determine the scenario from the cycle
			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sScenario = "PGM_C" & currREQ.Cycle
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
			If (membList.Count <> 1) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.Cycle
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			Else
				currREQ.scenario = sScenario
			End If
			
			Return isFileValid
			
		End Function
#End Region

#Region "Validate Membershashversion"		
'		Public Function	ValidateMembers(ByVal si As SessionInfo, ByRef currREQ As REQ, ByVal globals As BRGlobals ) As Object
			
'			Dim isFileValid As Boolean = True

			
'			If String.IsNullOrWhiteSpace(currREQ.title) Then
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Blank Title value in record"
'			End If
			
			
'			'validate fund code
'			'This code leverages the way we validate in Data Source
'			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
'			'---------------
'			'Store memberlists in Globals HashSet. 
'			'We are storing In Hashset because for a simple lookup it is the most performant than dataTable and List of string
			
'			'Validate that the Fund Center being loaded  s with in the command
			
'			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
'			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
			
'			Dim entityhashSet As New HashSet(Of String)()
			
'			If Globals.GetObject("Entity_" & currREQ.command) Is Nothing Then 
'				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
'				entityhashSet.UnionWith(membList.Select(Function(e) e.Member.Name))
'				Globals.SetObject("Entity_" & currREQ.command, entityhashSet)
'			Else
'				entityhashSet = Globals.GetObject("Entity_" & currREQ.command)
'			End If
			
'			Dim currEntity As String = currREQ.Entity
'			If Not entityhashSet.Contains(currEntity) Then 
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
'			End If
			
'			'Validate U1_APPN_FUND 
			
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
'			Dim U1_APPN_FUNDhashSet As New HashSet(Of String)()
			
'			If Globals.GetObject("U1_APPN_FUND_" & currREQ.command) Is Nothing Then 
'				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#Top.base", True)
'				U1_APPN_FUNDhashSet.UnionWith(membList.Select(Function(e) e.Member.Name))
'				Globals.SetObject("U1_APPN_FUND_" & currREQ.command, U1_APPN_FUNDhashSet)
'			Else
'				U1_APPN_FUNDhashSet = Globals.GetObject("U1_APPN_FUND_" & currREQ.command)
'			End If
					
'			If  Not U1_APPN_FUNDhashSet.Contains(currREQ.fundCode.Trim) Then 
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.fundCode
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
'			End If
			
'			'Validate U2_MDEP
			
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			
'			Dim U2_MDEPhashSet As New HashSet(Of String)()
			
'			If Globals.GetObject("U2_MDEP_" & currREQ.command) Is Nothing Then 
'				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#Top.base", True)
'				U2_MDEPhashSet.UnionWith(membList.Select(Function(e) e.Member.Name.Trim))
'				Globals.SetObject("U2_MDEP_" & currREQ.command, U2_MDEPhashSet)
'			Else
'				U2_MDEPhashSet = Globals.GetObject("U2_MDEP_" & currREQ.command)
'			End If
			
'			If  Not U2_MDEPhashSet.Contains(currREQ.MDEP.Trim) Then
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
'			End If
			
			
'			'Vakidate U3_APE_PT
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
'			Dim U3_APE_PThashSet As New HashSet(Of String)()
			
'			If Globals.GetObject("U3_APE_PThashSet_" & currREQ.command) Is Nothing Then 
'				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#Top.base", True)
'				U3_APE_PThashSet.UnionWith(membList.Select(Function(e) e.Member.Name))
'				Globals.SetObject("U3_APE_PThashSet_" & currREQ.command, U3_APE_PThashSet)
'			Else
'				U3_APE_PThashSet = Globals.GetObject("U3_APE_PThashSet_" & currREQ.command)
'			End If
			
'			If  Not U3_APE_PThashSet.Contains(currREQ.APE9.Trim) Then
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.APE9
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid APE value: " & SAG_APE))
'			End If
			
			
'			'Vakidate U4_DollarType
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType")
'			Dim U4_DollarTypehashSet As New HashSet(Of String)()
			
'			If Globals.GetObject("U4_DollarTypehashSet_" & currREQ.command) Is Nothing Then 
'				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U4#Top.base", True)
'				U4_DollarTypehashSet.UnionWith(membList.Select(Function(e) e.Member.Name))
'				Globals.SetObject("U4_DollarTypehashSet_" & currREQ.command, U4_DollarTypehashSet)
'			Else
'				U4_DollarTypehashSet = Globals.GetObject("U4_DollarTypehashSet_" & currREQ.command)
'			End If
			
'			If  Not U4_DollarTypehashSet.Contains(currREQ. DollarType.Trim) Then
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid Dollar Type value: " & currREQ.DollarType
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
'			End If
			
'			'We determine the scenario from the cycle
'			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sScenario = "PGM_C" & currREQ.Cycle
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
'			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
'			If (membList.Count <> 1) Then 
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.Cycle
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
'			Else
'				currREQ.scenario = sScenario
'			End If
			
'			Return isFileValid
			
'		End Function
#End Region

#Region "Populate Stage Table"		
		Public Function	PopulateStageTable(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByRef currREQ As REQ, ByVal isFileVlaid As Boolean,  ByRef ExistingREQs As List(Of String), ByRef currentUsedFlows As List(Of String)) As Object

			'get fcalcualted fields if file is valid. Else no need to create Flow as the records won't be loaded into the cube
			If(isFileVlaid) Then
				currREQ.flow = Me.GetFlow(si, globals, api, currREQ.command, currREQ.entity, currREQ.scenario, currREQ.Cycle, currREQ.title, currentUsedFlows, ExistingREQs)
				currREQ.REQ_ID = Me.GetREQID(si, globals, api, currREQ.entity, currREQ.scenario, currREQ.flow)
							
				'Add current flow to the list of used flows. This is used during the GetFlow process
				currentUsedFlows.Add(currREQ.entity & ":" & currREQ.flow)
			End If
		
			'insert into table
			Dim SQLInsert As String = "
			INSERT Into [dbo].[XFC_REQ_Import]
						([Command]
						,[Entity]
						,[APPN]
						,[MDEP]
						,[APE9]
						,[DollarType]
						,[ObjectClass]
						,[CType]
						,[Cycle]
						,[FY1]
						,[FY2]
						,[FY3]
						,[FY4]
						,[FY5]
						,[Title]
						,[Description]
						,[Justification]
						,[CostMethodology]
						,[ImpactifnotFunded]
						,[RiskifnotFunded]
						,[CostGrowthJustification]
						,[MustFund]
						,[FundingSource]
						,[ArmyInitiative_Directive]
						,[CommandInitiative_Directive]
						,[Activity_Exercise]
						,[IT_CyberRequirement]
						,[UIC]
						,[FlexField1]
						,[FlexField2]
						,[FlexField3]
						,[FlexField4]
						,[FlexField5]
						,[EmergingRequirement]
						,[CPATopic]
						,[PBRSubmission]
						,[UPLSubmission]
						,[ContractNumber]
						,[TaskOrderNumber]
						,[AwardTargetDate]
						,[POPExpirationDate]
						,[ContractorManYearEquiv_CME]
						,[COREmail]
						,[POCEmail]
						,[Directorate]
						,[Division]
						,[Branch]
						,[ReviewingPOCEmail]
						,[MDEPFunctionalEmail]
						,[NotificationListEmails]
						,[GeneralComments_Notes]
						,[JUON]
						,[ISR_Flag]
						,[Cost_Model]
						,[Combat_Loss]
						,[Cost_Location]
						,[Category_A_Code]
						,[CBS_Code]
						,[MIP_Proj_Code]
						,[SS_Priority]
						,[Commitment_Group]
						,[SS_Capability]
						,[Strategic_BIN]
						,[LIN]
						,[FY1_QTY]
						,[FY2_QTY]
						,[FY3_QTY]
						,[FY4_QTY]
						,[FY5_QTY]
						,[RequirementType]
						,[DD_Priority]
						,[Portfolio]
						,[DD_Capability]
						,[JNT_CAP_AREA]
						,[TBM_COST_POOL]
						,[TBM_TOWER]
						,[APMS_AITR_NUM]
						,[ZERO_TRUST_CAPABILITY]
						,[ASSOCIATED_DIRECTIVES]
						,[CLOUD_INDICATOR]
						,[STRAT_CYBERSEC_PGRM]
						,[NOTES]
						,[UNIT_OF_MEASURE]
						,[FY1_ITEMS]
						,[FY1_UNIT_COST]
						,[FY2_ITEMS]
						,[FY2_UNIT_COST]
						,[FY3_ITEMS]
						,[FY3_UNIT_COST]
						,[FY4_ITEMS]
						,[FY4_UNIT_COST]
						,[FY5_ITEMS]
						,[FY5_UNIT_COST]
						,[REQ_ID]
						,[Flow]
						,[ValidationError]
						,[Scenario])
		    VALUES
   				('" & currREQ.command.Replace("'", "''") & "','" &
					currREQ.Entity.Replace("'", "''") & "','" &
					currREQ.FundCode.Replace("'", "''") & "','" &
					currREQ.MDEP.Replace("'", "''") & "','" &
					currREQ.APE9.Replace("'", "''") & "','" &
					currREQ.DollarType.Replace("'", "''") & "','" &
					currREQ.CommitmentItem.Replace("'", "''") & "','" &
					currREQ.sCType.Replace("'", "''") & "','" &
					currREQ.Cycle.Replace("'", "''") & "','" &
					currREQ.FY1.Replace("'", "''") & "','" &
					currREQ.FY2.Replace("'", "''") & "','" &
					currREQ.FY3.Replace("'", "''") & "','" &
					currREQ.FY4.Replace("'", "''") & "','" &
					currREQ.FY5.Replace("'", "''") & "','" &
					currREQ.Title.Replace("'", "''") & "','" &
					currREQ.Description.Replace("'", "''") & "','" &
					currREQ.Justification.Replace("'", "''") & "','" &
					currREQ.CostMethodology.Replace("'", "''") & "','" &
					currREQ.ImpactifnotFunded.Replace("'", "''") & "','" &
					currREQ.RiskifnotFunded.Replace("'", "''") & "','" &
					currREQ.CostGrowthJustification.Replace("'", "''") & "','" &
					currREQ.MustFund.Replace("'", "''") & "','" &
					currREQ.FundingSource.Replace("'", "''") & "','" &
					currREQ.ArmyInitiative_Directive.Replace("'", "''") & "','" &
					currREQ.CommandInitiative_Directive.Replace("'", "''") & "','" &
					currREQ.Activity_Exercise.Replace("'", "''") & "','" &
					currREQ.IT_CyberRequirement.Replace("'", "''") & "','" &
					currREQ.UIC.Replace("'", "''") & "','" &
					currREQ.FlexField1.Replace("'", "''") & "','" &
					currREQ.FlexField2.Replace("'", "''") & "','" &
					currREQ.FlexField3.Replace("'", "''") & "','" &
					currREQ.FlexField4.Replace("'", "''") & "','" &
					currREQ.FlexField5.Replace("'", "''") & "','" &
					currREQ.EmergingRequirement.Replace("'", "''") & "','" &
					currREQ.CPATopic.Replace("'", "''") & "','" &
					currREQ.PBRSubmission.Replace("'", "''") & "','" &
					currREQ.UPLSubmission.Replace("'", "''") & "','" &
					currREQ.ContractNumber.Replace("'", "''") & "','" &
					currREQ.TaskOrderNumber.Replace("'", "''") & "','" &
					currREQ.AwardTargetDate.Replace("'", "''") & "','" &
					currREQ.POPExpirationDate.Replace("'", "''") & "','" &
					currREQ.ContractorManYearEquiv_CME.Replace("'", "''") & "','" &
					currREQ.COREmail.Replace("'", "''") & "','" &
					currREQ.POCEmail.Replace("'", "''") & "','" &
					currREQ.Directorate.Replace("'", "''") & "','" &
					currREQ.Division.Replace("'", "''") & "','" &
					currREQ.Branch.Replace("'", "''") & "','" &
					currREQ.ReviewingPOCEmail.Replace("'", "''") & "','" &
					currREQ.MDEPFunctionalEmail.Replace("'", "''") & "','" &
					currREQ.NotificationListEmails.Replace("'", "''") & "','" &
					currREQ.GeneralComments_Notes.Replace("'", "''") & "','" &
					currREQ.JUON.Replace("'", "''") & "','" &
					currREQ.ISR_Flag.Replace("'", "''") & "','" &
					currREQ.Cost_Model.Replace("'", "''") & "','" &
					currREQ.Combat_Loss.Replace("'", "''") & "','" &
					currREQ.Cost_Location.Replace("'", "''") & "','" &
					currREQ.Category_A_Code.Replace("'", "''") & "','" &
					currREQ.CBS_Code.Replace("'", "''") & "','" &
					currREQ.MIP_Proj_Code.Replace("'", "''") & "','" &
					currREQ.SS_Priority.Replace("'", "''") & "','" &
					currREQ.Commitment_Group.Replace("'", "''") & "','" &
					currREQ.SS_Capability.Replace("'", "''") & "','" &
					currREQ.Strategic_BIN.Replace("'", "''") & "','" &
					currREQ.LIN.Replace("'", "''") & "','" &
					currREQ.FY1_QTY.Replace("'", "''") & "','" &
					currREQ.FY2_QTY.Replace("'", "''") & "','" &
					currREQ.FY3_QTY.Replace("'", "''") & "','" &
					currREQ.FY4_QTY.Replace("'", "''") & "','" &
					currREQ.FY5_QTY.Replace("'", "''") & "','" &	
					currREQ.RequirementType.Replace("'", "''") & "','" &
					currREQ.DD_Priority.Replace("'", "''") & "','" &
					currREQ.Portfolio.Replace("'", "''") & "','" &
					currREQ.DD_Capability.Replace("'", "''") & "','" &
					currREQ.JNT_CAP_AREA.Replace("'", "''") & "','" &
					currREQ.TBM_COST_POOL.Replace("'", "''") & "','" &
					currREQ.TBM_TOWER.Replace("'", "''") & "','" &
					currREQ.APMSAITRNum.Replace("'", "''") & "','" &
					currREQ.ZERO_TRUST_CAPABILITY.Replace("'", "''") & "','" &
					currREQ.ASSOCIATED_DIRECTIVES.Replace("'", "''") & "','" &
					currREQ.CLOUD_INDICATOR.Replace("'", "''") & "','" &
					currREQ.STRAT_CYBERSEC_PGRM.Replace("'", "''") & "','" &
					currREQ.NOTES.Replace("'", "''") & "','" &
					currREQ.UNIT_OF_MEASURE.Replace("'", "''") & "','" &
					currREQ.FY1_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY1_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY2_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY2_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY3_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY3_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY4_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY4_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY5_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY5_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.REQ_ID.Replace("'", "''") & "','" & 	
					currREQ.flow.Replace("'", "''") & "','" &
					currREQ.validationError.Replace("'", "''") & "','" &
					currREQ.scenario & "')"  
				
'BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
						
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
			End Using				
			Return Nothing
		End Function
#End Region

#Region "Populate Cube"		
		Public Function	PopulateCube(ByVal si As SessionInfo, ByVal currREQ As REQ) As Object
			
			Dim wfyear As Integer = Convert.ToInt32(currREQ.Cycle)
			Dim sDataBufferPOVScript_Amount As String = "Cb#" & currREQ.command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":V#Periodic:A#REQ_Requested_Amt:I#" & currREQ.entity & ":F#"& currREQ.flow &":O#BeforeAdj:U1#" & currREQ.fundCode & ":U2#" & currREQ.MDEP & ":U3#" & currREQ.APE9 & ":U4#" & currREQ.DollarType & ":U5#"& currREQ.sCtype & ":U6#"& currREQ.CommitmentItem & ":U7#None:U8#None"
			'FY scripts
			Dim FY1TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & wfyear
			Dim FY2TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 1).ToString
			Dim FY3TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 2).ToString
			Dim FY4TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 3).ToString
			Dim FY5TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 4).ToString
			
			Dim objListofAmountScripts As New List(Of MemberScriptandValue)
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY1) Then
			    Dim objScriptValFY1 As New MemberScriptAndValue
				objScriptValFY1.CubeName = currREQ.command
				objScriptValFY1.Script = FY1TimeScript
				objScriptValFY1.Amount = currREQ.FY1
				objScriptValFY1.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY1)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY2) Then
				Dim objScriptValFY2 As New MemberScriptAndValue
				objScriptValFY2.CubeName = currREQ.command
				objScriptValFY2.Script = FY2TimeScript
				objScriptValFY2.Amount = currREQ.FY2
				objScriptValFY2.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY2)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY3) Then
				Dim objScriptValFY3 As New MemberScriptAndValue
				objScriptValFY3.CubeName = currREQ.command
				objScriptValFY3.Script = FY3TimeScript
				objScriptValFY3.Amount = currREQ.FY3
				objScriptValFY3.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY3)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY4) Then
				Dim objScriptValFY4 As New MemberScriptAndValue
				objScriptValFY4.CubeName = currREQ.command
				objScriptValFY4.Script = FY4TimeScript
				objScriptValFY4.Amount = currREQ.FY4
				objScriptValFY4.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY4)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY5) Then
				Dim objScriptValFY5 As New MemberScriptAndValue
				objScriptValFY5.CubeName = currREQ.command
				objScriptValFY5.Script = FY5TimeScript
				objScriptValFY5.Amount = currREQ.FY5
				objScriptValFY5.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY5)
			End If
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofAmountScripts)
''BRApi.ErrorLog.LogMessage(si, "FY5TimeScript = " & FY5TimeScript )						
			Return Nothing
			
		End Function
#End Region

#Region "Populate Annotations"		
		Public Function	PopulateAnnotation(ByVal si As SessionInfo, ByVal currREQ As REQ)As Object
			Dim sDataBufferPOVScript_POVLoop As String = "Cb#" & currREQ.command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":T#" & currREQ.Cycle & ":V#Annotation:I#None:F#"& currREQ.flow &":O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sUD5Filter As String = sDataBufferPOVScript_POVLoop.Replace("U5#None","U5#REQ_Owner")
				Dim sUD5FilterFunc As String = sDataBufferPOVScript_POVLoop.Replace("U5#None","U5#REQ_Func_POC")
				Dim txtValue As String = ""
				
				Dim objListofScripts As New List(Of MemberScriptandValue)
				
				Dim wfScenarioTypeID As Integer
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,currREQ.Cycle)
				wfScenarioTypeID = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si, dimtypeid.Scenario, currREQ.scenario)).Id
				
				'Set REQ_ID
				Dim sFilterScriptREQ_ID As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ID"
'BRAPI.ErrorLog.LogMessage(si, "REQ_ID = " & REQ_ID & ", sFilterScriptREQ_ID = " & sFilterScriptREQ_ID)							
			    Dim objScriptValREQ_ID As New MemberScriptAndValue
				objScriptValREQ_ID.CubeName = currREQ.command
				objScriptValREQ_ID.Script = sFilterScriptREQ_ID
				objScriptValREQ_ID.TextValue = currREQ.REQ_ID
				objScriptValREQ_ID.IsNoData = False
				objListofScripts.Add(objScriptValREQ_ID)
				
				
			If Not String.IsNullOrWhiteSpace(currREQ.Title) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Title"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Title
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Description ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Description"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Description 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Justification ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Recurring_Justification"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Justification 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ImpactifnotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Impact_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ImpactifnotFunded
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.RiskifnotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Risk_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RiskifnotFunded
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CostMethodology) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Methodology_Cmt"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CostMethodology
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CostGrowthJustification) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Growth_Justification"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CostGrowthJustification
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MustFund) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Must_Fund"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MustFund
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FundingSource ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Requested_Fund_Source"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FundingSource 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ArmyInitiative_Directive) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Army_initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ArmyInitiative_Directive
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CommandInitiative_Directive) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Command_Initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CommandInitiative_Directive
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Activity_Exercise) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Activity_Exercise"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Activity_Exercise
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.IT_CyberRequirement ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_IT_Cyber_Rqmt_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.IT_CyberRequirement 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UIC) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UIC_Acct"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UIC
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField1 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_1"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField1 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField2 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_2"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField2 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField3 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_3"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField3 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField4 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_4"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField4 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField5 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_5"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField5 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.EmergingRequirement) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_New_Rqmt_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.EmergingRequirement
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CPATopic) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CPA_Topic"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CPATopic
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.PBRSubmission) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_PBR_Submission"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.PBRSubmission
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UPLSubmission) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UPL_Submission"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UPLSubmission
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ContractNumber ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Contract_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ContractNumber 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TaskOrderNumber ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Task_Order_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TaskOrderNumber 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.AwardTargetDate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Target_Date_Of_Award"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.AwardTargetDate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.POPExpirationDate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POP_Expiration_Date"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.POPExpirationDate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ContractorManYearEquiv_CME ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FTE_CME"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ContractorManYearEquiv_CME 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.COREmail ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_COR_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.COREmail 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.POCEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POC_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.POCEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Directorate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Directorate"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Directorate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Division ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Division"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Division 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Branch) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Branch"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Branch
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ReviewingPOCEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Rev_POC_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ReviewingPOCEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MDEPFunctionalEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_MDEP_Func_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MDEPFunctionalEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.NotificationListEmails) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Notification_Email_List"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.NotificationListEmails
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.GeneralComments_Notes) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Comments"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.GeneralComments_Notes
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.JUON) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_JUON"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.JUON
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ISR_Flag) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ISR_Flag"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ISR_Flag
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Cost_Model) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Model"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Cost_Model
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Combat_Loss) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Combat_Loss"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Combat_Loss
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Cost_Location) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Location"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Cost_Location
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Category_A_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Category_A_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Category_A_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CBS_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CBS_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CBS_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MIP_Proj_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_MIP_Proj_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MIP_Proj_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.RequirementType) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Type"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RequirementType
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.DD_Priority) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_DD_Priority"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.DD_Priority
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Portfolio) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Portfolio"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Portfolio
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.DD_Capability) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Capability_DD"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.DD_Capability
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.JNT_CAP_AREA) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_JNT_CAP_AREA"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.JNT_CAP_AREA
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TBM_COST_POOL) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_TBM_COST_POOL"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TBM_COST_POOL
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TBM_TOWER) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_TBM_TOWER"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TBM_TOWER
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.APMSAITRNum) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_APMS_AITR_Num"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.APMSAITRNum
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ZERO_TRUST_CAPABILITY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ZERO_TRUST_CAPABILITY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ZERO_TRUST_CAPABILITY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ASSOCIATED_DIRECTIVES) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Assoc_Directorate"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ASSOCIATED_DIRECTIVES
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CLOUD_INDICATOR) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CLOUD_INDICATOR"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CLOUD_INDICATOR
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.STRAT_CYBERSEC_PGRM) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_STRAT_CYBERSEC_PGRM"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.STRAT_CYBERSEC_PGRM
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.NOTES) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Notes"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.NOTES
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UNIT_OF_MEASURE) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UNIT_OF_MEASURE"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UNIT_OF_MEASURE
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY5_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY5_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.SS_Priority) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_SS_Priority"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.SS_Priority
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Commitment_Group) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Commitment_Group"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Commitment_Group
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.SS_Capability) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Capability_SS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.SS_Capability
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Strategic_BIN) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Strategic_BIN"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Strategic_BIN
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.LIN) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_LIN"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.LIN
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
                If Not String.IsNullOrWhiteSpace(currREQ.FY5_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
			
'--------------------------------------------------------------------------------------------------------------					
				'Set status to L?_Working. Get the level from the entity
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, currREQ.entity).Member
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				Dim REQStatus As String = entityText3.Substring(entityText3.Length -2, 2) & " Imported"
				Dim sFilterScriptWFStatus As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Rqmt_Status"
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = currREQ.command
				objScriptValWFStatus.Script = sFilterScriptWFStatus
				objScriptValWFStatus.TextValue = REQStatus
				objScriptValWFStatus.IsNoData = False
				objListofScripts.Add(objScriptValWFStatus)
				
				'Set Funding Status
				Dim sFilterScriptFundingStatus As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Funding_Status"
				txtValue = "Unfunded"
			    Dim objScriptValFundingStatus As New MemberScriptAndValue
				objScriptValFundingStatus.CubeName = currREQ.command
				objScriptValFundingStatus.Script = sFilterScriptFundingStatus
				objScriptValFundingStatus.TextValue = txtValue
				objScriptValFundingStatus.IsNoData = False
				objListofScripts.Add(objScriptValFundingStatus)
				
				
				'Set REQ Status History
				Dim sREQUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				Dim sCompleteRQWFStatus As String = sREQUser & " : " & CurDate & " : " & currREQ.entity & " : " & REQStatus
				Dim sFilterScriptWFStatusHistory As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Status_History"
			    Dim objScriptValWFStatusHistory As New MemberScriptAndValue
				objScriptValWFStatusHistory.CubeName = currREQ.command
				objScriptValWFStatusHistory.Script = sFilterScriptWFStatusHistory
				objScriptValWFStatusHistory.TextValue = sCompleteRQWFStatus
				objScriptValWFStatusHistory.IsNoData = False
				objListofScripts.Add(objScriptValWFStatusHistory)	

				'Set created by
				Dim sFilterScriptCreatedBy As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Creator_Name"
			    Dim objScriptValCreatedBy As New MemberScriptAndValue
				objScriptValCreatedBy.CubeName = currREQ.command
				objScriptValCreatedBy.Script = sFilterScriptCreatedBy
				objScriptValCreatedBy.TextValue = sREQUser
				objScriptValCreatedBy.IsNoData = False
				objListofScripts.Add(objScriptValCreatedBy)	
				
				'Set Creation date
				Dim sFilterScriptCreatedTime As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Creation_Date_Time"
			    Dim objScriptValCreatedTime As New MemberScriptAndValue
				objScriptValCreatedTime.CubeName = currREQ.command
				objScriptValCreatedTime.Script = sFilterScriptCreatedTime
				objScriptValCreatedTime.TextValue = System.DateTime.Now.ToString()
				objScriptValCreatedTime.IsNoData = False
				objListofScripts.Add(objScriptValCreatedTime)	
				
				
				'Set cell for all the fields
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				Return Nothing
		End Function
#End Region

#Region "Get Flow"

		'For a given entity and scenario, it checks if the current title is in the data attachement table. If it is it is considered an update and the existing Flow ID is returned
		'If it is a new one, the logic will create one. A similar logic to the one in create reqquirement.
		'*** DEV TO DO The logic in create and here to generate the flow could be broken out to a function and be used from both places.
		Public Function	GetFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal Cube As String, ByVal entity As String, ByVal Scenario As String, ByVal Time As String, ByVal title As String, ByVal usedFlows As List(Of String),  ByRef ExistingREQs As List(Of String)) As String	
			Try
				Dim sCube As String = Cube.Trim()
				Dim sEntity As String = entity.Trim()				
				Dim sScenario As String = Scenario.Trim()
				Dim sREQTime As String = Time.Trim()
				
#Region "Commented code"				
				'******************************************************************************************************************************************************************************************
				'*********** This part of the code is commented out because it was decided that we won't check for duplicates
				'*********** Kept it here incase we get back to it. It can be removed once the process is solidified
				'*********** Right now we create flow for all even if the REQ title exist
				'******************************************************************************************************************************************************************************************
				
'				'Check if the REQ exists per entity, scenario and title. If it does grab the flow.
'				Dim sqlCheckExist As String = "Select distinct Flow,text  From DataAttachment 
'									 WHERE Account = 'REQ_Title' and Entity =  '" & entity & "' And Scenario = '" & sScenario & "' and Text = '" & title.Replace("'", "''") & "'"
''BRApi.ErrorLog.LogMessage(si,"sqlCheckExist=" & sqlCheckExist)


'				Dim dtFetch As New DataTable
'				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'					 dtFetch = BRApi.Database.ExecuteSql(dbConn, sqlCheckExist, True)
'				End Using
'				If dtFetch.Rows.Count > 1  Then
'					Throw New Exception("Duplicate Records found in Data Attachment Table. Please contact the administrator")
'				End If
'				If dtFetch.Rows.Count = 1  Then
'					If  Not IsDBNull(dtFetch.Rows(0).Item("Flow")) Then
'						Return dtFetch.Rows(0).Item("Flow")
'					End If
'				End If

'				Dim REQCreated As Boolean = False
#End Region	

'		        'Check if there is a REQ flow member available. If not create one	
				Dim REQFlow As String = ""' ExistingREQs.FirstOrDefault(Function(x) Not usedFlows.Contains(x.Split(":")(1).Trim))
				
				For Each fl As String In ExistingREQs
					Dim key As String = sEntity & ":"  & fl
					
					If Not usedFlows.Contains(key)
						'BRApi.ErrorLog.LogMessage(si, "Used REQ  = " & fl)
						Return fl
					End If
				Next
				
'				Dim flowMemberExists As Boolean = False
'				For Each ExistingREQ As String In ExistingREQs
'					Dim REQTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & ExistingREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			
'				 	Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleScr).DataCellEx.DataCellAnnotation
''brapi.ErrorLog.LogMessage(si,"REQ = " & ExistingREQ.Member.Name & " Title=" & TitleValue)
'	            	If String.IsNullOrWhiteSpace(TitleValue) And Not usedFlows.Contains(entity &":" & ExistingREQ) Then				
'						flowMemberExists = True
'						REQFlow = ExistingREQ
'						Exit For
'					End If
'				Next

				If String.IsNullOrWhiteSpace(REQFlow) Then	
					REQFlow = Me.CreateFlow(si, globals, api, New DashboardExtenderArgs())
				End If
				
				Return REQFlow
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 

#Region "Get REQ_ID"
		'This logic is the same as the logic in Create Req
		'***DEV TO DO the other function could be broken out to use it in both places
		Public Function GetREQID(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal sEntity As String,  ByVal sScenario As String,  ByVal sFlow As String) As Object
			Try
				'Check if the REQ exists per entity, scenario and title. If it does grab the flow.
				'*******************************************************************************
				'Stopped checking logic for now. 
				'Per Chris, we import all rows. Do Not check For existing since there could be the same title REQs that are different
				'Left it In comment incase we go back To it.
'				Dim sqlCheckExist As String = "Select distinct Flow,text  From DataAttachment 
'									 WHERE Account = 'REQ_ID' and Entity =  '" & sEntity & "' And Scenario = '" & sScenario & "' and Flow = '" & sFlow & "'"
''BRApi.ErrorLog.LogMessage(si,"REQ_ID sqlCheckExist = " & sqlCheckExist)

'				Dim dtFetch As New DataTable
'				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'					 dtFetch = BRApi.Database.ExecuteSql(dbConn, sqlCheckExist, True)
'				End Using

'				If dtFetch.Rows.Count = 1  Then
'					If  Not IsDBNull(dtFetch.Rows(0).Item("text")) Then
'						Return dtFetch.Rows(0).Item("text")
'					End If
'				End If
				
				Dim SQL As String ="Select Max(Text) as ID From DataAttachment WHERE Account = 'REQ_ID' and Entity =  '" & sEntity & "'"
			
				Dim dtID As New DataTable()
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					dtID =BRApi.Database.ExecuteSql(dbConnApp,SQL,True)
				End Using
'BRApi.ErrorLog.LogMessage(si, "dtID count = " & dtID.Rows.Count)					
				Dim sREQID As String = ""

				If dtID.Rows.Count = 0  Or IsDBNull( dtID.Rows(0).Item("ID")) Then
					Dim sEntityRemoveGen = sEntity.Replace("_General","")
					Dim sStratingIDNum As String = "00001"
					sREQID = sEntityRemoveGen & "_" & sStratingIDNum
				Else

					Dim sDTREQIDs As String = dtID.Rows(0).Item("ID")
					Dim sREQIDfromDT As String = String.Join(",", sDTREQIDs.ToArray())
					Dim iREQIDNum As Integer = Convert.ToInt32(sREQIDfromDT.split("_")(1))
					Dim sNewIDNum As Integer = iREQIDNum + 1
					Dim sNewfullIDNum = sNewIDNum.ToString("D5")
			 		Dim sEntityRemoveGeneral = sEntity.Replace("_General","")
					sREQID = sEntityRemoveGeneral & "_" & sNewfullIDNum
				End If
				
'BRApi.ErrorLog.LogMessage(si, "returned sREQID = " & sREQID)					
				Return sREQID

				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try			 
			End Function
					
#End Region 

#Region "Get Used Flows"
			Public Function GetUsedFlows(ByVal si As SessionInfo, ByVal command As String, ByVal scenario As String) As Object
			
				Dim UsedFlowsSQL As String ="Select [Entity], Flow From DataAttachment WHERE [Cube] = '" & command &"' And Scenario =  '" & scenario & "'"
			
				Dim dtUsedFlows As New DataTable()
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					dtUsedFlows =BRApi.Database.ExecuteSql(dbConnApp,UsedFlowsSQL,True)
				End Using
				
				Dim usedFlowsPerScenarioAndFC As List(Of String) = New List(Of String)
			
				For Each r As DataRow In dtUsedFlows.Rows
					usedFlowsPerScenarioAndFC.Add(r.Item("Entity") & ":" &  r.Item("Flow"))		
				Next
				
				Return usedFlowsPerScenarioAndFC
			End Function

#End Region

#End Region

#Region "Delete Data Attachement"
		'This function runs a delete on the Data Attachement table fo the give entity and cycle
		Public Function	DeleteDataAttachement(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object	
		
			Dim scenario As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
			Dim cycle As String = scenario.Substring(5,4)
			Dim cube As String =BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).CubeName
			Dim entity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim REQIDsToDelete As String() = args.NameValuePairs.XFGetValue("REQIDsToDelete").Split(",")
'BRApi.ErrorLog.LogMessage(si, "REQ IDs: " & args.NameValuePairs.XFGetValue("REQIDsToDelete"))		
'BRApi.ErrorLog.LogMessage(si, "Cycle: " & cycle)	
'BRApi.ErrorLog.LogMessage(si, "Entity: " & args.NameValuePairs.XFGetValue("Entity"))			
			
			'Get the FYDP from the cycle
			Dim intYear = 0
			Integer.TryParse(cycle, intYear)
			
			Dim time As String = "T#" & cycle &  
								  ",T#" & intYear + 1 &
								  ",T#" & intYear + 2 &
								  ",T#" & intYear + 3 &
								  ",T#" & intYear + 4
								  
			For Each DeleteREQ As String In REQIDsToDelete
				'delete cube data
				Dim params As New Dictionary(Of String, String) 
				params.Add("Cube", cube)
				params.Add("Entity", "E#" & entity)
				params.Add("Scenario", scenario)
				params.Add("Time", time )
				params.Add("Flow", DeleteREQ.Trim )
'brapi.ErrorLog.LogMessage(si, "Cube = " & Cube & ", entity = " & entity & ", scenario = " & scenario & ", time = " & time & ", Flow = " & DeleteREQ)				
				Dim result As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Delete_REQ", params)
				
				'Delete annotation from data Attachement table
				Dim deleteDataAttchSQL As String = "Delete from [dbo].[DataAttachment] WHERE Entity = '" &entity & "' And Scenario = '" & scenario & "' And Flow = '" & DeleteREQ.Trim & "'"
'brapi.ErrorLog.LogMessage(si, "deleteDataAttchSQL = " & deleteDataAttchSQL)				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn, deleteDataAttchSQL, True)
				End Using
				
				'Delete tem table
				Dim deleteTempTblSQL As String = "Delete From [dbo].[XFC_REQ_Import] Where Entity = '" & entity & "' And Cycle = '" & cycle & "' And Flow = '" & DeleteREQ.Trim & "'"
'brapi.ErrorLog.LogMessage(si, "deleteTempTblSQL = " & deleteTempTblSQL)				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn, deleteTempTblSQL, True)
				End Using
			Next

			Return Nothing
		End Function			
#End Region 

#Region "Check_WF_Complete_Lock"
'Added 09/24/2024 by Fronz RMW-1704
		Public Function	Check_WF_Complete_Lock(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							
			'--------------------------------------------------------------- 
			'Verify Workflow is NOT Complete Or Locked
			'---------------------------------------------------------------
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
				Throw New Exception("Notice: No updates are allowed. Workflow was marked ""Complete"" by Command HQ.")
			End If
			Return Nothing
		End Function				
#End Region

#Region "Create Empty REQ"
		'Created: YH - 10/25/2023
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'The Create function gets the Entity and Title passed into it.
		'It will get audit info from the logged in user and populates the audit information
		'The new REQ name is passed into it through args.
		'Updated: EH 8/22/2024 - Ticket 1565 Updated Title, ID, Creation Name/Date
		'Data saved at REQ_Shared and updated time to yearly
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function CreateEmptyREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal flag As String = "", Optional ByVal TargetEntity As String =  "") As Object

			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("sourceEntity", TargetEntity)
'BRApi.ErrorLog.LogMessage(si, "sEntity " & sEntity)			
			If String.IsNullOrWhiteSpace(sEntity) Then
				sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			End If
'BRApi.ErrorLog.LogMessage(si, "sEntity " & sEntity)				
			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)

			
			Dim sRecurringCost As String = args.NameValuePairs.XFGetValue("RecurringCost")
			If (String.IsNullOrWhiteSpace(sRecurringCost)) Then sRecurringCost = "Yes"
			Dim sREQTitle As String = args.NameValuePairs.XFGetValue("REQName")	
			Dim REQtitleLength As Integer = sREQTitle.Length
			
			'throw error if REQ title Is empty
			If REQtitleLength = 0 Then 
				Throw New Exception("REQ Title:" & environment.NewLine & "Must enter a title for the REQ." & environment.NewLine)
			End If
			
			Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")
			
			'throw error if REQ Amount Is empty
			If flag.XFEqualsIgnoreCase("create") Then
				If sRequestedAmt.Length = 0 Then 
					Throw New Exception("REQ Title:" & environment.NewLine & "Must enter an amount for the REQ." & environment.NewLine)
				End If
			End If
			Dim sREQUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim sREQFundingStatus As String = "Unfunded"
'BRApi.ErrorLog.LogMessage(si, "Here1 = " & sEntity)				
			'-----------Grab text3 from base entity member-------------------
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'BRApi.ErrorLog.LogMessage(si, "Here2")
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
			
'BRApi.ErrorLog.LogMessage(si, "Here3")
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			entityText3 = entityText3.Substring(entityText3.Length -2, 2)
'BRApi.ErrorLog.LogMessage(si, "Here4")			
			'-----------set new REQ workflow status--------------------------
			Dim sREQWFStatus As String = entityText3 & " Working"
				
			Dim REQCreated As Boolean = False
	        'Check if there is a REQ flow member available. If not create one	
			Dim REQFlow As String = ""
			Dim ExistingREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).OrderBy(Function(x) convert.ToInt32(x.Member.name.split("_")(1))).ToList()
			
			Dim flowMemberExists As Boolean = False
			For Each ExistingREQ As MemberInfo In ExistingREQs
				Dim REQTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & ExistingREQ.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
'BRApi.ErrorLog.LogMessage(si, "REq title scr : " &	REQTitleScr)				
			 	Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleScr).DataCellEx.DataCellAnnotation
            	If String.IsNullOrWhiteSpace(TitleValue) Then				
					flowMemberExists = True
					REQFlow = ExistingREQ.Member.Name
					Exit For
				End If
			Next

			If Not flowMemberExists Then	
				REQFlow = Me.CreateFlow(si, globals, api, args)
			End If
'BRApi.ErrorLog.LogMessage(si, "From Create REQ : " &	REQFlow & ", sEntity: " & sEntity & ", sREQTitle: " & sREQTitle)	

			'Create  REQ_ID 
			Dim REQIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_ID:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			'Dim REQIDSharedMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#REQ_Shared:T#1999:V#Annotation:A#REQ_ID:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

			Dim SQL As New Text.StringBuilder
			SQL.Append($"Select Text as ID
						From DataAttachment
						WHERE text = (Select Max(Text) From DataAttachment WHERE Account = 'REQ_ID'
						and Entity =  '{sEntity}') And
						Account = 'REQ_ID'
						and Entity =  '{sEntity}'
			Group By TEXT")
		
			Dim dtID As New DataTable
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
							dtID =BRApi.Database.ExecuteSql(dbConnApp,SQL.ToString(),True)
								End Using
			Dim sDTREQIDs As New List(Of String)
			
			For Each dataRow As DataRow In dtID.Rows
				sDTREQIDs.Add(dataRow.Item("ID"))
			Next		
				
			'	'For Each dataRow As DataRow In dtID.Rows		
			'	Dim record As String =  dataRow.ToString
			'	record = dataRow.Item("ID")
			'	brapi.ErrorLog.LogMessage(si, record)
			'	'Next

'BRApi.ErrorLog.LogMessage(si, "Before Rows = 0")								
			If dtID.Rows.Count = 0 Then
				Dim sEntityRemoveGen = sEntity.Replace("_General","")
				Dim sStratingIDNum As String = "00001"
				Dim sStrartingREQID As String = sEntityRemoveGen & "_" & sStratingIDNum
'BRApi.ErrorLog.LogMessage(si, "After Rows = 0")
			
				'Update REQ ID for WF Scenario
				Dim objListofScriptsREQID As New List(Of MemberScriptandValue)
			    Dim objScriptValREQID As New MemberScriptAndValue
				objScriptValREQID.CubeName = sCube
				objScriptValREQID.Script = REQIDMemberScript
				objScriptValREQID.TextValue = sStrartingREQID
				objScriptValREQID.IsNoData = False
				objListofScriptsREQID.Add(objScriptValREQID)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsREQID)
				'Update REQ ID for Shared Scenario
'				Dim objListofScriptsREQIDShared As New List(Of MemberScriptandValue)
'			    Dim objScriptValREQIDShared As New MemberScriptAndValue
'				objScriptValREQIDShared.CubeName = sCube
'				objScriptValREQIDShared.Script = REQIDSharedMemberScript
'				objScriptValREQIDShared.TextValue = sStrartingREQID
'				objScriptValREQIDShared.IsNoData = False
'				objListofScriptsREQIDShared.Add(objScriptValREQIDShared)
'				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsREQIDShared)	
				
			Else
'BRApi.ErrorLog.LogMessage(si, "Inside else ")	
				Dim sREQIDfromDT As String = String.Join(",", sDTREQIDs.ToArray())
				Dim iREQIDNum As Integer = Convert.ToInt32(sREQIDfromDT.split("_")(1))
				Dim sNewIDNum As Integer = iREQIDNum + 1
				Dim sNewfullIDNum = sNewIDNum.ToString("D5")
		 		Dim sEntityRemoveGeneral = sEntity.Replace("_General","")
				Dim sNewREQID As String = sEntityRemoveGeneral & "_" & sNewfullIDNum

				'Update REQ ID for WF Scenario
				Dim objListofScriptsREQID As New List(Of MemberScriptandValue)
			    Dim objScriptValREQID As New MemberScriptAndValue
				objScriptValREQID.CubeName = sCube
				objScriptValREQID.Script = REQIDMemberScript
				objScriptValREQID.TextValue = sNewREQID
				objScriptValREQID.IsNoData = False
				objListofScriptsREQID.Add(objScriptValREQID)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsREQID)
				'Update REQ ID for WF Scenario
'				Dim objListofScriptsREQIDShared As New List(Of MemberScriptandValue)
'			    Dim objScriptValREQIDShared As New MemberScriptAndValue
'				objScriptValREQIDShared.CubeName = sCube
'				objScriptValREQIDShared.Script = REQIDSharedMemberScript
'				objScriptValREQIDShared.TextValue = sNewREQID
'				objScriptValREQIDShared.IsNoData = False
'				objListofScriptsREQIDShared.Add(objScriptValREQIDShared)
'				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsREQIDShared)
			
			End If
			
'			Old intersection for Title Account
			Dim REQTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			'Dim REQTitleSharedMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#REQ_Shared:T#1999:V#Annotation:A#REQ_Title:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			
			Dim REQUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creator_Name:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim REQDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim REQFundingStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Funding_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim REQWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim REQRecurringCostMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Recurring_Cost_Ind:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"   				
			Dim REQWFREQStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Status_History:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
           'Update REQ Title For WF Scenario
			Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
		    Dim objScriptValTitle As New MemberScriptAndValue
			objScriptValTitle.CubeName = sCube
			objScriptValTitle.Script = REQTitleMemberScript
			objScriptValTitle.TextValue = sREQTitle
			objScriptValTitle.IsNoData = False
			objListofScriptsTitle.Add(objScriptValTitle)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
			
			'Update REQ TitleShared for Shared Scenario
'			Dim objListofScriptsTitleShared As New List(Of MemberScriptandValue)
'		    Dim objScriptValTitleShared As New MemberScriptAndValue
'			objScriptValTitleShared.CubeName = sCube
'			objScriptValTitleShared.Script = REQTitleSharedMemberScript
'			objScriptValTitleShared.TextValue = sREQTitle
'			objScriptValTitleShared.IsNoData = False
'			objListofScriptsTitleShared.Add(objScriptValTitleShared)
			
'			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitleShared)
			
			'Update REQ Creator
			Dim objListofScriptsUser As New List(Of MemberScriptandValue)
		    Dim objScriptValUser As New MemberScriptAndValue
			objScriptValUser.CubeName = sCube
			objScriptValUser.Script = REQUserMemberScript
			objScriptValUser.TextValue = sREQUser
			objScriptValUser.IsNoData = False
			objListofScriptsUser.Add(objScriptValUser)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsUser)
			
			'Update REQ Created Date
			Dim objListofScriptsDate As New List(Of MemberScriptandValue)
		    Dim objScriptValDate As New MemberScriptAndValue
			objScriptValDate.CubeName = sCube
			objScriptValDate.Script = REQDateMemberScript
			objScriptValDate.TextValue = CurDate
			objScriptValDate.IsNoData = False
			objListofScriptsDate.Add(objScriptValDate)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDate)
			
			'Update REQ Funding Status 
			Dim objListofScriptsFundingStatus As New List(Of MemberScriptandValue)
		    Dim objScriptValFundingStatus As New MemberScriptAndValue
			objScriptValFundingStatus.CubeName = sCube
			objScriptValFundingStatus.Script = REQFundingStatusMemberScript
			objScriptValFundingStatus.TextValue = sREQFundingStatus
			objScriptValFundingStatus.IsNoData = False
			objListofScriptsFundingStatus.Add(objScriptValFundingStatus)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsFundingStatus)
			
			'Update REQ Workflow Status
			Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
		    Dim objScriptValWFStatus As New MemberScriptAndValue
			objScriptValWFStatus.CubeName = sCube
			objScriptValWFStatus.Script = REQWFStatusMemberScript
			objScriptValWFStatus.TextValue = sREQWFStatus
			objScriptValWFStatus.IsNoData = False
			objListofScriptsWFStatus.Add(objScriptValWFStatus)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
			
			'Update REQ Recurring Cost
			Dim objListofScriptsRecurringCost As New List(Of MemberScriptandValue)
		    Dim objScriptValRecurringCost As New MemberScriptAndValue
			objScriptValRecurringCost.CubeName = sCube
			objScriptValRecurringCost.Script = REQRecurringCostMemberScript
			objScriptValRecurringCost.TextValue = sRecurringCost
			objScriptValRecurringCost.IsNoData = False
			objListofScriptsRecurringCost.Add(objScriptValRecurringCost)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsRecurringCost)		
			
			'Update REQ Status History
			Dim sCompleteRQWFStatus As String = sREQUser & " : " & CurDate & " : " & sEntity & " : " & sREQWFStatus
			Dim objListofScriptsStatusHistory As New List(Of MemberScriptandValue)
		    Dim objScriptValStatusHistory As New MemberScriptAndValue
			objScriptValStatusHistory.CubeName = sCube
			objScriptValStatusHistory.Script = REQWFREQStatusHistoryMemberScript
			objScriptValStatusHistory.TextValue = sCompleteRQWFStatus
			objScriptValStatusHistory.IsNoData = False
			objListofScriptsStatusHistory.Add(objScriptValStatusHistory)		
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsStatusHistory)	
			
#Region "Old update status history code"			
'			'Update REQ Status History(need to revise) causes other status except for working to record
'				Try
'					Me.UpdateStatusHistory(si, globals, api, args, sREQWFStatus, REQFlow, sEntity )
'				Catch ex As Exception
'				End Try
#End Region

				'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, REQFlow, sEntity)
				Catch ex As Exception
			End Try
			
			Return REQFlow
		End Function
	
#End Region '(Here update)

#Region "Save Initial Amount"	
		'The Save initial amount function gets the sfunding line, amount and destination REQ Flow passed into it.
		'If there is no funding line and amount it will create a "none" funding line and amount 1.
		'Updated: EH RMW-1564 8/22/24 Updated to save amount at the year level for PGM_C20XX 
		
		Public Function SaveInitialAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal REQFlow As String) 
						
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sEntity As String = GetEntity(si, args)			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'BRApi.ErrorLog.LogMessage(si, "ScENARIO " & sScenario)			
			'Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'BRApi.ErrorLog.LogMessage(si, "TIME " & sREQTime)				
			
			'if RequestedAttributeType amount is blank we default to 1. This is needed for copy.
			
			'During create there is an exception trown for blank amount and wont get here
			Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")
'BRApi.ErrorLog.LogMessage(si, "SRequestedAMT before " & sRequestedAmt)
			If String.IsNullOrWhiteSpace(sRequestedAmt) Then sRequestedAmt = "1"
'BRApi.ErrorLog.LogMessage(si, "SRequestedAMT after " & sRequestedAmt)

			Dim sAPPNFund As String = args.NameValuePairs.XFGetValue("APPNFund")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")		
			Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
			Dim sDollarType As String = args.NameValuePairs.XFGetValue("DollarType")
			Dim sCommimentItem As String = args.NameValuePairs.XFGetValue("CommitmentItem")
			
			If String.IsNullOrWhiteSpace(sAPPNFund) Then sAPPNFund = "None"
			If String.IsNullOrWhiteSpace(sMDEP) Then sMDEP = "None"
			If String.IsNullOrWhiteSpace(sAPE) Then sAPE = "None"
			If String.IsNullOrWhiteSpace(sDollarType) Then sDollarType = "None"
			If String.IsNullOrWhiteSpace(sCommimentItem) Then sCommimentItem = "None"
				
			'On the initial creation of REQs, the IC will be the creating entity
			Dim IC As String = sEntity

			Dim REQRequestedAmttMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#REQ_Requested_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommimentItem & ":U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"sAPPNFund = {sAPPNFund} || sMDEP = {sMDEP} || sAPE = {sAPE}  || sDollarType {sDollarType}")
'BRApi.ErrorLog.LogMessage(si, "Amount = " & sRequestedAmt & ", Script = " & REQRequestedAmttMemberScript)

			'Update REQ Requested Amount
			Dim objListofScriptsAmount As New List(Of MemberScriptandValue)
		    Dim objScriptValAmount As New MemberScriptAndValue
			objScriptValAmount.CubeName = sCube
			objScriptValAmount.Script = REQRequestedAmttMemberScript
			objScriptValAmount.Amount = sRequestedAmt
			objScriptValAmount.IsNoData = False
			objListofScriptsAmount.Add(objScriptValAmount)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAmount)	

			Return Nothing
			
		End Function
	
#End Region '(Updated)

#Region "CallDataManagementDeleteREQ"
		'The CallDataManagementDeleteUFR removes all funding request values for the entity/ufr combination
		Public Function CallDataManagementDeleteUFR(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Try						
					
				'Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim iREQAmount As Integer = Nothing
				Dim sEntity As String =  args.NameValuePairs.XFGetValue("REQEntity")
				Dim sREQ As String =  args.NameValuePairs.XFGetValue("REQ")
				
				'invoke a DM task to clear added row data
				Dim sREQTimeParam As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim dataMgmtSeq As String = "Delete_REQ"     
				Dim params As New Dictionary(Of String, String) 
				params.Add("Entity", "E#" & sEntity)
				params.Add("Scenario", sScenario)
				'params.Add("Time", "T#" & sREQTimeParam & ".Base")
				'params.Add("Time", "T#" & sREQTimeParam)
				params.Add("Flow", sREQ)
'Brapi.ErrorLog.LogMessage(si,"sREQTimeParam = " & sREQTimeParam)						
				'Get the FYDP from the cycle
				Dim intYear = 0
				Integer.TryParse(sREQTimeParam, intYear)
				
				'DEV NOTE: It ignores the first year for some reason, so it is added at the end.
				Dim time As String = ""
				time = "T#" & intYear & ",T#" & intYear - 1 &  ",T#" & intYear + 1 &  ",T#" & intYear + 2 &  ",T#" & intYear + 3 &	  ",T#" & intYear + 4 & ",T#" & intYear
								   									   
				params.Add("Time", "T#" & time)
'Brapi.ErrorLog.LogMessage(si,"time = " & time)					
				BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
'Brapi.ErrorLog.LogMessage(si,"Sequence should have finished")
				
				Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function
			  
#End Region '(Updated)

#Region "Copy Amount"
		
		Public Function CopyAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal newREQFlow As String) As Boolean
			Try
				Dim cube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				
				'Get source scenrio and time from the passed in value
				Dim srcEntity = args.NameValuePairs.XFGetValue("sourceEntity")
				Dim srcFlow As String = args.NameValuePairs.XFGetValue("SourceREQ")
				Dim srcSenario As String = args.NameValuePairs.XFGetValue("sourceScenario")
				Dim srcScenarioMbrId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, srcSenario)
				Dim srcTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si, srcScenarioMbrId)
				srcTime  =  srcTime.Substring(0,4)
				
				'target REQ
				Dim trgtScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim trgtREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim trgtFlow = newREQFlow
				Dim trgtEntity As String = GetEntity(si, args)
			
				Dim params As New Dictionary(Of String, String) 
				'params.Add("Scenario", sScenario)
				'params.Add("Time", "T#" & sTime & ".Base")
				params.Add("Entity", srcEntity)
				params.Add("srcEntity", srcEntity)
				params.Add("srcFlow", srcFlow)
				params.Add("srcSenario", srcSenario)
				params.Add("srcTime", srcTime)
				params.Add("trgtFlow", trgtFlow)
		
'BRApi.ErrorLog.LogMessage(si, "srcEntity = " & srcEntity & ", srcFlowREQ = " & srcFlow & ", srcSenario = " & srcSenario & ", trgtFlow = " & trgtFlow & ", trgtEntity = " & trgtEntity)
				
				BRApi.Utilities.ExecuteDataMgmtSequence(si, "PGM_Copy_Amount", params)

				Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function
			  
#End Region 

#Region "Copy REQ Details"
		'Created: YH - 10/20/2023
		'Updated: Fronz - 03/19/2024 replaced hard coded accounts assigned to the CopyAccounts list with REQ_Copy_List.Children
		'Updated: Fronz - 04/17/2024 commented out the function to copy an existing attachment and an existing "General Comment" from a previously created requirement
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'How it works:
				'The copy function gets the source REQ passed into it through args and gets the destination from session variable.
				'The new REQ name is passed into it through args.	
				'Source entity can be passed in as well. If it is not the target entity will be the default.
		'USAGE: Called on_Click btn_REQPRO_AAAAAA_0CaAa_SaveCopiedREQ__Shared
		'Updated: EH - 08/26/24 - RMW 1565 Updated Time to yearly for PGM_C20XX
		'Updated: AK 09/10/2024 - RMW-1573 modified to REQ_Shared-->1999
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		
		Public Function CopyREQDetails(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal newREQFlow As String, Optional ByVal srcEntity As String = "") As Boolean
			Dim sWFProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			
			'obtains the Fund, Name, Entity, and IT selection from the Create REQ Dashboard
			Dim sSourceREQ As String = args.NameValuePairs.XFGetValue("SourceREQ")
'BRApi.ErrorLog.LogMessage(si, "Source: " & sSourceREQ) 
			
			'For Merge the source format is "FC - Flow : Title but for copy it is just "Flow". These accounts for both
			If (sSourceREQ.XFContainsIgnoreCase("-") And sSourceREQ.XFContainsIgnoreCase(":")) Then
				sSourceREQ = sSourceREQ.Split(":")(0).Split("-")(1).Trim
			End If
			
			Dim sREQTitle As String = args.NameValuePairs.XFGetValue("REQName")		
			
			'Get source scenrio and time from the passed in value
			Dim sourceSenario As String = args.NameValuePairs.XFGetValue("sourceScenario")
			Dim sourceScenarioMbrId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sourceSenario)
			Dim sourceTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si, sourceScenarioMbrId)
			sourceTime  =  sourceTime.Substring(0,4)
			
'BRApi.ErrorLog.LogMessage(si, "title: " & sREQTitle)						
			Dim sEntity As String = GetEntity(si, args)
			
			If Not Me.IsREQCreationAllowed(si, sEntity) Then			
				Throw  New Exception("REQ Title:" & environment.NewLine & "Can not copy Requirement at this time. Contact Requirement manager." & environment.NewLine)			
			End If
			'For parent level merges the source and target entries may be different. In that case srcEntity is passed in.
			
			If String.IsNullOrWhiteSpace(srcEntity) Then
				'srcEntity = sEntity
				srcEntity = args.NameValuePairs.XFGetValue("sourceEntity")
			End If
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sREQUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim sREQFundingStatus As String = "Unfunded"
			
			'-----------Grab text3 from base entity member-------------------
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			entityText3 = entityText3.Substring(entityText3.Length -2, 2)
			'-----------set new REQ workflow status--------------------
			Dim sREQWFStatus As String = entityText3 & " Working"
			
			'Validate that REQ Title is not empty
			Dim REQCreated As Boolean = False
'	        'Create UFR	
			'Does this varaible serve a purpose? does not seem to be writing to an intersection after being declared Fronz 03/19/2024
			'Dim REQAmountMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#UFR_Requested_Amt:F#" & newREQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			'Copy all children of A#REQ_Copy_Info members when creating a new Requirement
			Dim CopyAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Copy_List.Children.Remove(REQ_Title,REQ_ID,REQ_Status_History,REQ_Rqmt_Status,REQ_Creator_Name,REQ_Creation_Date_Time)", True)

			For Each REQAccount As MemberInfo In CopyAccounts
				Dim sourceMbrScript As String = "Cb#" & sCube & ":E#" & srcEntity & ":C#Local:S#" & sourceSenario & ":T#" & sourceTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sSourceREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim targetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & newREQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim SourceValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sourceMbrScript).DataCellEx.DataCellAnnotation
'BRApi.ErrorLog.LogMessage(si, "Copy sourceMbrScript : " & sourceMbrScript & " ** SourceValue = " & SourceValue)	
'BRApi.ErrorLog.LogMessage(si, "Copy targetMbrScript : " & targetMbrScript)
'Return Nothing
				'Update UFR Target Account
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = targetMbrScript
				objScriptValAccount.TextValue = SourceValue
				objScriptValAccount.IsNoData = False
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)
				
			Next
			
'			'Copy Attachment
'			Dim sCopyAttachments As String = Me.CopyAttachments(si, sCube, srcEntity, sEntity, sScenario, sREQTime, sSourceREQ, newREQFlow)
			
'			'Copy General Comments
'			Me.CopyGeneralComments(si,globals,api,args, sSourceREQ, srcEntity, newREQFlow, sEntity)
				
			REQCreated = True

			Return REQCreated
			 
		End Function
			  
#End Region '(Updated)
		
#Region "Create Merged UFR"
		'Created: YH - 10/30/2023
		'Create Merge gets a list Of UFRs To be be merged With one Primary UFR. 
		'It Is called From the "Merge" button On Approval and parent level Analyst
		'It creates a new UFR, Copys the detail anotation from the primary UFR and copies the funding line from all UFRs being merged
		'---------------------------------------------------------------------------------------------------------------------------------
		'Updated: KN - 12/13/2023
		'Updated to accommodate new IC structure
		'	- no longer creating a new UFR to be merged into
		'	- source UFRs will be merged into Primary UFR
		' 	- if copy funding line is selected, copy funding & requested amount from sources to primary UFR
		'		- funding lines will have Entity of Primary UFR and IC of source UFRs
		'	- update merge status for all UFRs to 'merged' but only close non-primary UFRs
		'	- no longer calling data management to execute Finance BR for the funding line copy, this is now done through BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule
		
		Public Function CreateMergedUFR(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
		
			Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter","")
			Dim CopySelection As String  = args.NameValuePairs.XFGetValue("CopySelection") 
			Dim sPrimaryUFR As String  = args.NameValuePairs.XFGetValue("SourceUFR") 
			Dim UFRsToMerge As List(Of String) = args.NameValuePairs.XFGetValue("UFRsToMerge").Split(",").ToList
			Dim PrimaryEntity As String = ""
			Dim PrimaryUFR As String = ""
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRYr As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim bRecurring As Boolean = False
		
			'PrimaryUFR = "A601A - UFR_01 : YH Test 1"
			If (Not String.IsNullOrWhiteSpace(sPrimaryUFR)) Then
				PrimaryUFR = sPrimaryUFR.Split(":")(0).Split("-")(1).Trim
				PrimaryEntity  = sPrimaryUFR.Split(":")(0).Split("-")(0).Trim
			End If
			
			'Check if Primary UFR is selected		
			If String.IsNullOrWhiteSpace(PrimaryUFR) Then 
				Throw New Exception ("There was no Primary UFR selected. Please select a Primary UFR.")
			End If
			
			'Check if at least 2 UFRs are selected		
			If UFRsToMerge.Count < 2 Then 
				Throw New Exception ("Please select at least 2 UFRs to merge.")
			End If
			
'BRApi.ErrorLog.LogMessage(si,$"sFundCenter = {sFundCenter}, CopySelection = {CopySelection}, sPrimaryUFR = {sPrimaryUFR}, sCube = {sCube}, sScenario = {sScenario}, sUFRTime = {sUFRTime}")
			'When Funding line Copy is set, call funding lines copy Data Management to merge the funding lines. DM = "Merge_UFR"
			'If Funding line is not selected the Initial amount need to be called to create an finding line with None and $1
			If CopySelection.XFContainsIgnoreCase("Yes") Then

				'Build thelist of POV from the selected UFRs. The list is passed in a comma delimited format "FC - Flow : Title"
				Dim sUFREnt As String = ""
				Dim sUFRFlow As String = ""
				Dim sUFRPov As String = ""
				Dim sPOVFilter As String = ""
				For Each mergedUFR As String In UFRsToMerge						
					sUFREnt = mergedUFR.Split(":")(0).Split("-")(0).Trim
					sUFRFlow = mergedUFR.Split(":")(0).Split("-")(1).Trim
						'Check if the UFR is Recurring Cost
					If Not bRecurring Then
						Dim UFRRecurringCostMemberScript As String = "Cb#" & sCube & ":E#" & sUFREnt & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Recurring_Cost_Ind:F#" & sUFRFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim sRecurring As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRRecurringCostMemberScript).DataCellEx.DataCellAnnotation 
						If sRecurring = "Yes" Then bRecurring = True
					End If
					
					If mergedUFR.Contains(sPrimaryUFR) Then Continue For
					If (String.IsNullOrWhiteSpace(sUFRPov)) Then
						sUFRPov = sUFRPov & "E#" & sUFREnt & ":F#" & sUFRFlow
					Else
						sUFRPov = sUFRPov & ",E#" & sUFREnt & ":F#" & sUFRFlow
					End If

				Next
			
				Dim customSubstVars As Dictionary(Of String, String) = New Dictionary(Of String, String)
				customSubstVars.Add("Cube",sCube)
				customSubstVars.Add("Scenario",sScenario)			
				customSubstVars.Add("Entity",PrimaryEntity)
				customSubstVars.Add("Time",sUFRTime)
				customSubstVars.Add("Consolidation","Local")
				customSubstVars.Add("View","Periodic")
				customSubstVars.Add("Flow",PrimaryUFR)
				customSubstVars.Add("IC",sUFREnt)
'				customSubstVars.Add("UD1","ERA")
'				customSubstVars.Add("UD2","")
'				customSubstVars.Add("UD3","")
'				customSubstVars.Add("UD4","")
				customSubstVars.Add("SourcePOVList",sUFRPov)
				customSubstVars.Add("POVFilter","A#UFR_Requested_Amt, A#UFR_Funded_Amt")
				customSubstVars.Add("ClearTarget","False")
				customSubstVars.Add("IsReclass","True")
				If bRecurring Then

					'Loop through 12 Months
					'Merge UFRs
					For i As Integer = 1 To 12
						customSubstVars("Time") = sUFRYr & "M" & i
'BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Main_Copy","Copy_Reclass",customSubstVars,customcalculatetimetype.MemberFilter)
						
						BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"UFR_Main_Calc","MergeUFR",customSubstVars,customcalculatetimetype.MemberFilter)
					Next
				Else				
					'Write only to YYYYM12
					'Merge UFRs
					customSubstVars("Time") = sUFRTime
'BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Main_Copy","Copy_Reclass",customSubstVars,customcalculatetimetype.MemberFilter)
	
					BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"UFR_Main_Calc","MergeUFR",customSubstVars,customcalculatetimetype.MemberFilter)
				End If
					
					
'				Dim objTaskActivityItem As TaskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, "Merge_UFR", customSubstVars)
				
				'Update UFR Recurring Cost
				If bRecurring Then
					Dim UFRRecurringCostMemberScript As String = "Cb#" & sCube & ":E#" & PrimaryEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Recurring_Cost_Ind:F#" & PrimaryUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim objListofScriptsRecurringCost As New List(Of MemberScriptandValue)
		  		  	Dim objScriptValRecurringCost As New MemberScriptAndValue
					objScriptValRecurringCost.CubeName = sCube
					objScriptValRecurringCost.Script = UFRRecurringCostMemberScript
					objScriptValRecurringCost.TextValue = "Yes"
					objScriptValRecurringCost.IsNoData = False
					objListofScriptsRecurringCost.Add(objScriptValRecurringCost)
			
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsRecurringCost)	
				End If
			
			End If
			
			'RMW-1039 Update Merged UFR's Funding Status
			Dim sUFRFundingStatusMemberScript As String = "Cb#" & sCube & ":E#" & PrimaryEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Funding_Status:F#" & PrimaryUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sUFRiFundedAmtMemberScript As String = "Cb#" & sCube & ":E#" & PrimaryEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRYr & ":V#Periodic:A#UFR_Funded_Amt:F#" & PrimaryUFR & ":O#BeforeAdj:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			Dim sUFRiRequestedAmtMemberScript As String = "Cb#" & sCube & ":E#" & PrimaryEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRYr & ":V#Periodic:A#UFR_Requested_Amt:F#" & PrimaryUFR & ":O#BeforeAdj:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			Dim iFundedAmt As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRiFundedAmtMemberScript).DataCellEx.DataCell.CellAmount
			Dim iRequestedAmt As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRiRequestedAmtMemberScript).DataCellEx.DataCell.CellAmount
			Dim sNewFundingStatus As String = ""

			Select Case iFundedAmt
			Case 0
				sNewFundingStatus = "Unfunded"
			Case iRequestedAmt
				sNewFundingStatus = "Internally Funded"
			Case Is > 0, Is < iRequestedAmt
				sNewFundingStatus = "Partially Funded"		
			End Select
			
			Dim objListofScriptsFundingStatus As New List(Of MemberScriptandValue)
		    Dim objScriptsFundingStatus As New MemberScriptAndValue
			objScriptsFundingStatus.CubeName = sCube
			objScriptsFundingStatus.Script = sUFRFundingStatusMemberScript
			objScriptsFundingStatus.TextValue = sNewFundingStatus
			objScriptsFundingStatus.IsNoData = False
			objListofScriptsFundingStatus.Add(objScriptsFundingStatus)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsFundingStatus)

			'Update the status of Merged UFRs to 'Merged' and Closed
			'RMW-1063 adding UFR_Merged_List annotation in the same loop
						
			Dim mergedMbrScript As String = "Cb#" & sCube & ":E#EEEE" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Merge_Status" & ":F#FFFF"  & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim WFStatusMbrScript As String = "Cb#" & sCube & ":E#EEEE" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status" & ":F#FFFF"  & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim ufrMergedListMbrScript As String = "Cb#" & sCube & ":E#EEEE" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Merged_List" & ":F#FFFF"  & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

			Dim mergedStatus As String = "Merged"
			Dim WFStatus As String = " Closed"
			
			Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
'BRApi.ErrorLog.LogMessage(si,"wfScenario: " & wfScenario & ", wfScenarioTypeID: " & wfScenarioTypeID & " , wfTime: " & wfTime & ", wfTimeId: " & wfTimeId)
'BRApi.ErrorLog.LogMessage(si,"sScenario: " & sScenario & ", sUFRYr: " & sUFRYr)	

			For Each mergedUFR As String In UFRsToMerge
				Dim mergedUFRFlow = mergedUFR.Split(":")(0).Split("-")(1).Trim
				Dim mergedUFREntity = mergedUFR.Split(":")(0).Split("-")(0).Trim
				'Grab the entity Level from the Text 3 property. It is used in the Closed status
				Dim entityMbrInfo As Memberinfo = BRApi.Finance.Metadata.GetMember(si, dimType.Entity.Id, mergedUFREntity)
				'Dim sWfLevel As String = Me.GetWorkflowLevel(si)
				
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = mergedMbrScript.Replace("FFFF", mergedUFRFlow).Replace("EEEE", entityMbrInfo.Member.Name)
'BRApi.ErrorLog.LogMessage(si,"entityMbrInfo.Member.Name: " & entityMbrInfo.Member.Name)				
				objScriptValAccount.TextValue = mergedStatus
				objScriptValAccount.IsNoData = False
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)
				
				'Begin UFR_Merge_List
				Dim objListofScripts As New List(Of MemberScriptandValue)
		    	Dim objScriptMergeList As New MemberScriptAndValue
				objScriptMergeList.CubeName = sCube
				objScriptMergeList.Script = ufrMergedListMbrScript.Replace("FFFF", mergedUFRFlow).Replace("EEEE", entityMbrInfo.Member.Name)
				objScriptMergeList.TextValue = ""
				objScriptMergeList.IsNoData = False
				objListofScripts.Add(objScriptMergeList)
				'Reference object to set the text value depending on primary ufr condition
				Dim refObjScriptMergeList As MemberScriptAndValue = objListofScripts(0)

				If mergedUFR.Contains(sPrimaryUFR) Then
					Try
						Me.LastUpdated(si, globals, api, args, mergedUFRFlow, entityMbrInfo.Member.Name)
						
					'If primary, set data cell for UFR_Merged_List text
						'Remove the primary ufr from the list
						Dim mergeListTitle As New List(Of String)
						mergeListTitle.AddRange(UFRsToMerge.ToArray)
						mergeListTitle.Remove(mergedUFR)
						'Rebuild the list to remove the ufr title
						Dim mergeListNoTitle As New List(Of String)
						For Each ufr As String In mergeListTitle
							Dim entity As String = ufr.Split(":")(0).Split("-")(0).Trim
							Dim flow As String = ufr.Split(":")(0).Split("-")(1).Trim
							Dim noTitle As String = entity & " - " & flow
							mergeListNoTitle.Add(noTitle)
						Next
						
						'List to string and set text
						Dim mergeListString As String = String.Join(",", mergeListNoTitle.ToArray)
						refObjScriptMergeList.TextValue = mergeListString
						
						BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
						
						Catch ex As Exception
					End Try			
					Continue For
				Else
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
		    		Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = WFStatusMbrScript.Replace("FFFF", mergedUFRFlow).Replace("EEEE", entityMbrInfo.Member.Name)
					'objScriptValWFStatus.TextValue = sWfLevel & WFStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				'update status history on New UFR
					Try
						Me.UpdateStatusHistory(si, globals, api, args, mergedUFRFlow, entityMbrInfo.Member.Name, sFundCenter) 'sWfLevel & WFStatus,
					Catch ex As Exception
					End Try
					
				'If not primary change the UFR_Merged_List text to only include the primary
					Dim primaryUFREntity As String = sPrimaryUFR.Split(":")(0).Split("-")(0).Trim
					Dim primaryUFRFlow As String = sPrimaryUFR.Split(":")(0).Split("-")(1).Trim
					Dim primaryUFRNoTitle As String = primaryUFREntity & " - " & primaryUFRFlow
					Dim primaryString As String = "Merged Into: " & primaryUFRNoTitle
					refObjScriptMergeList.TextValue = primaryString
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				End If
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, mergedUFRFlow, entityMbrInfo.Member.Name)
				Catch ex As Exception
				End Try			
			Next	
		
			Return Nothing
			
		End Function
		
#End Region 'updated

#Region "Create Flow"
		Public Function CreateFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			Dim ExistingREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).OrderBy(Function(x) convert.ToInt32(x.Member.name.split("_")(1))).ToList()
			Dim mbrSourceREQ As MemberInfo = ExistingREQS.Last()
            Dim iREQNum As Integer = Convert.ToInt32(mbrSourceREQ.Member.Name.split("_")(1))
			Dim sParentMemberName As String = "Command_Requirements"
	        Dim sMemberName As String = "REQ_" & iREQNum + 1
			Dim sMemberDesc As String = ""
										
	        'Dim mbrSourceREQ As String
			Dim sourceMbrProperties As VaryingMemberProperties = BRApi.Finance.Members.ReadMemberPropertiesNoCache(si, dimtype.Flow.Id, mbrSourceREQ.member.Name)
	        'Create new Scenario member in the F_REQ_Main Dimension	

			Dim objDim As OneStream.Shared.Wcf.Dim = BRApi.Finance.Dim.GetDim(si, "F_REQ_Main")							
			Dim objMemberPk As New MemberPk(DimType.Flow.Id, DimConstants.Unknown)				
			
			'Create New Member
            Dim objMember As New Member(objMemberPk, sMemberName, sMemberDesc, objDim.DimPk.DimId)
			Dim objProperties As New VaryingMemberProperties(sourceMbrProperties) 
			objProperties.ChangeMemberId(objMember.MemberId)
			objProperties.ChangeParentId(DimConstants.Unknown)
							
			Dim objMemberInfo As New MemberInfo(objMember, objProperties, Nothing, objDim,DimConstants.Unknown)
	   		Dim isNew As TriStateBool = TriStateBool.TrueValue
		
			'Create new Flow REQ 
			BRApi.Finance.MemberAdmin.SaveMemberInfo(si, objMemberInfo, True, True, False, isNew)	 'Create new Scenario member

	   		Dim ParentID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, sParentMemberName)	
			'Relationship Assignment
			Dim objMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Flow.Id, sMemberName)
			Dim relPk As New RelationshipPk(DimType.Flow.Id, ParentID, objMemberId)
			Dim rel As New Relationship(relPk, objDim.DimPk.DimId, RelationshipMovementType.InsertAsLastSibling, 1)
			Dim relInfo As New RelationshipInfo(rel, Nothing)
			Dim relPostionOpt As New RelationshipPositionOptions()

			'Save the member Relationship and its properties.
			BRApi.Finance.MemberAdmin.SaveRelationshipInfo(si, relInfo, relPostionOpt)	
	
			Return sMemberName
		End Function
#End Region '(updated here)

#Region "Delete REQ"
		'Updated EH 08/27/24 RMW-1565 to delete at REQ_Shared level
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function DeleteREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
									
			'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			Dim sREQToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim iREQAmount As Integer = Nothing
			Dim sREQFlow As String = args.NameValuePairs.XFGetValue("reqFlow", sREQToDelete)

			'Add entity and REQ flow to args
			args.NameValuePairs.Add("REQEntity", sEntity)
			args.NameValuePairs.Add("REQ", sREQToDelete)
			'Call method to clear Requirement Funding Request Amount values
			Me.CallDataManagementDeleteUFR(si, globals, api, args)
				
			'Delete annotation from data Attachement table
			Dim deleteDataAttchSQL As String = "Delete from DataAttachment WHERE Cube = '" &  sCube & "' And Entity = '" & sEntity & "' And Scenario = '" & sScenario & "' And Flow = '" & sREQToDelete & "'"
'brapi.ErrorLog.LogMessage(si, "deleteDataAttchSQL = " & deleteDataAttchSQL)				
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 BRApi.Database.ExecuteSql(dbConn, deleteDataAttchSQL, True)
			End Using
				
			'Addresses annotation accounts
			Dim DeleteAnnotationAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Requirement_Info.Base", True)
			'Addresses periodic accounts not included in the Data Management sequence
			Dim DeletePeriodicAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Funding_Required_Date", True)
			Dim DeletePOCMbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U5_Main"), "U5#REQ_POCs.Base", True)
			Dim ResetValue As String = ""
			
			'Loops through accounts in an annotation member script
			For Each REQAccount As MemberInfo In DeleteAnnotationAccounts
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						

				'Sets member script and assigns the ResetValue
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = DeleteMbrScript
				objScriptValAccount.TextValue = ResetValue
				objScriptValAccount.IsNoData = False
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)					
			Next
			
			
			'Loops through accounts in an annotation member script at REQ_Shared scenario level
'			For Each REQAccount As MemberInfo In DeleteAnnotationAccounts
'				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#REQ_Shared:T#1999:V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						

'				'Sets member script and assigns the ResetValue
'				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
'		    	Dim objScriptValAccount As New MemberScriptAndValue
'				objScriptValAccount.CubeName = sCube
'				objScriptValAccount.Script = DeleteMbrScript
'				objScriptValAccount.TextValue = ResetValue
'				objScriptValAccount.IsNoData = False
'				objListofScriptsAccount.Add(objScriptValAccount)
'				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)					
'			Next
			
			'Loops through POC members in an annotation member script
			For Each POCMbr As MemberInfo In DeletePOCMbrs
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_POC_Name:F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & POCMbr.Member.Name & ":U6#None:U7#None:U8#None"										
				
				'Sets member script and assigns the ResetValue
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = DeleteMbrScript
				objScriptValAccount.TextValue = ResetValue
				objScriptValAccount.IsNoData = False
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)					
			Next
			
			'Loops through accounts in a periodic member script
			For Each REQAccount As MemberInfo In DeletePeriodicAccounts
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#" & REQAccount.Member.Name & ":F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									
				'Sets member script and assigns the ResetValue
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = DeleteMbrScript
				objScriptValAccount.TextValue = ResetValue
				objScriptValAccount.IsNoData = True
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)					
			Next	
					 			
			Return Nothing
		End Function
			  
#End Region '(updated here)

#Region "Submit REQ"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484	
'Updated: EH - 08/28/2024 - Ticket 1565. WFYear set to annual
		Public Function SubmitREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim reqTime As String = WFYear & "M12"
				Dim reqTime As String = WFYear
				Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
				Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow")
			
				'-------Get current REQ workflow status-------
				Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim currREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
				Dim newREQStatus As String =""
'Brapi.ErrorLog.LogMessage(si,"mem Script : " & REQMemberScript)
				Dim isBlank As Boolean = String.IsNullOrWhiteSpace(currREQStatus)
				Dim isStatusFormulate As Boolean = currREQStatus.Contains("Working") Or currREQStatus.Contains("Copied") Or currREQStatus.Contains("Imported")

				If isBlank Or Not isStatusFormulate Then
					Return Nothing
				End If
			'Added If Statement To account for creating on the parent level
				Dim currREQStatusLevel = currREQStatus.Substring(1,1)
				Dim icurrREQStatus As Integer = currREQStatusLevel
				If reqEntity.XFContainsIgnoreCase("_General") Or icurrREQStatus = 3 Or  icurrREQStatus = 2 Then 	
					Dim valLevel As String = icurrREQStatus
					newREQStatus = "L" & valLevel & " Ready for Validation"
				Else
				'-------Set new REQ workflow status-------
					Dim inewREQStatus = icurrREQStatus - 1
					Dim valLevel As String = inewREQStatus
					newREQStatus  = "L" & valLevel & " Ready for Validation"
				End If
			
                'Update REQ workflow Status
				Dim objListofScriptStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValStatus As New MemberScriptAndValue
				objScriptValStatus.CubeName = wfCube
				objScriptValStatus.Script = REQMemberScript
				objScriptValStatus.TextValue = newREQStatus
				objScriptValStatus.IsNoData = False
				objListofScriptStatus.Add(objScriptValStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)					
			
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, newREQStatus)
				Catch ex As Exception
				End Try
		
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
				Catch ex As Exception
				End Try
				
				'Hide db
'				Try
'					args.NameValuePairs.XFSetValue("allTimeValue","True")
'					args.NameValuePairs.XFSetValue("trueValue","REQPRO_BLNKDB_BlankDashboard")
'					Me.ShowAndHideDashboards(si, globals, api, args)
'				Catch ex As Exception
'				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region ' Updated

#Region "Confirm Review"
		
		Public Function ConfirmReview(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim reqTime As String = WFYear & "M12"

				Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
				Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow")
		
				'Get current status of UFR
				Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

				Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
				If(String.IsNullOrWhiteSpace(currentStatus)) Then
					Return Nothing
				End If
				
				Dim newStatus As String = "Ready for Validation"
					
                'Update UFR Status
				Dim objListofScriptStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValStatus As New MemberScriptAndValue	
				objScriptValStatus.CubeName = wfCube
				objScriptValStatus.Script = REQMemberScript
				objScriptValStatus.TextValue = newStatus
				objScriptValStatus.IsNoData = False
				objListofScriptStatus.Add(objScriptValStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
				
				
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, newStatus)
				Catch ex As Exception
				End Try
				
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
				Catch ex As Exception
				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region ' New

#Region "Validate REQ"
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484	
		'Updated 08292024 EH RMW-1565 reqTime updated to annual for PGM_C20XX scenario, IC for A#REQ_Validated_Amt updated 
		Public Function ValidateRequirement(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sProfileSubString As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim reqTime As String = WFYear & "M12"
				Dim reqTime As String = WFYear

				Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
				Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow")
					
				'------------Get current REQ workflow status-------------
				Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim currREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
				If(String.IsNullOrWhiteSpace(currREQStatus)) Then
					Return Nothing
				End If
				
'				Dim REQValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
'				Dim REQApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
				Dim newREQStatus As String = ""
			
				If sProfileSubString = "Validate Requirements" Then
'					If sValidatedAmount = 0 Then 
'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
'					End If
					currREQStatus = currREQStatus.Substring(0,2)
					newREQStatus = currREQStatus & " Ready for Prioritization"
					
				Else If sProfileSubString = "Validate Requirements CMD"
'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
'					End If
					currREQStatus = currREQStatus.Substring(0,2)
					newREQStatus = currREQStatus & " Ready for Prioritization"
				End If
				
                    'Update UFR Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = REQMemberScript
					objScriptValStatus.TextValue = newREQStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
				
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, newREQStatus)
				Catch ex As Exception
				End Try
				
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
				Catch ex As Exception
				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region ' New

#Region "Validate REQs"
		'Created: KN - 09/25/2024 - Ticket 1708	
		'Created to submit multiple REQs for prioritization 
		Public Function ValidateREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sProfileSubString As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim reqTime As String = WFYear & "M12"
				Dim reqTime As String = WFYear
			
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")
				For Each row As DataRow In oReqList.Rows
					Dim reqEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
					Dim reqFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
'BRApi.ErrorLog.LogMessage(si, $"EntityFlow = {row("EntityFlow")} || reqEntity = {reqEntity} || reqFlow = {reqFlow}")					
				
					'------------Get current REQ workflow status-------------
					Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"REQMemberScript = {REQMemberScript}")
					Dim currREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
					If(String.IsNullOrWhiteSpace(currREQStatus)) Then
						Return Nothing
					End If
					
					Dim REQValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim REQApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim newREQStatus As String = ""
				
					If sProfileSubString = "Validate Requirements" Then
	'					If sValidatedAmount = 0 Then 
	'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
	'					End If
						currREQStatus = currREQStatus.Substring(0,2)
						newREQStatus = currREQStatus & " Ready for Prioritization"
						
					Else If sProfileSubString = "Validate Requirements CMD"
	'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
	'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
	'					End If
						currREQStatus = currREQStatus.Substring(0,2)
						newREQStatus = currREQStatus & " Ready for Prioritization"
					End If
					
                    'Update REQ Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = REQMemberScript
					objScriptValStatus.TextValue = newREQStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, reqFlow, reqEntity)
						'Me.UpdateStatusHistory(si, globals, api, args, newREQStatus)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
					Catch ex As Exception
					End Try
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region ' New

#Region "SubmitAllToValidation"
		'Created: MH/CM - 2/10/2025 - Ticket 739	
		'Created to submit multiple REQs for Validation
		Public Function SubmitAllToValidation(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
'BRApi.ErrorLog.LogMessage(si, "START SubmitAllREQs in REQ_SolutiomHelper")
			Try
				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sProfileSubString As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim reqTime As String = WFYear & "M12"
				Dim reqTime As String = WFYear
			
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")
				For Each row As DataRow In oReqList.Rows
					Dim reqEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
					Dim reqFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
'BRApi.ErrorLog.LogMessage(si, $"EntityFlow = {row("EntityFlow")} || reqEntity = {reqEntity} || reqFlow = {reqFlow}")					
				
					'------------Get current REQ workflow status-------------
					Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"REQMemberScript = {REQMemberScript}")
					Dim currREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
					If(String.IsNullOrWhiteSpace(currREQStatus)) Then
						Return Nothing
					End If
					
					Dim newREQStatus As String = ""
				
					If reqEntity.XFContainsIgnoreCase("_General") Then
						newREQStatus = currREQStatus.Substring(0,2) & " Ready for Validation"
					Else 
						Dim NewLevel As Integer = currREQStatus.Substring(1,1) - 1
						newREQStatus = "L" & NewLevel & " Ready for Validation"
					End If 
					
					
                    'Update REQ Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = REQMemberScript
					objScriptValStatus.TextValue = newREQStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, reqFlow, reqEntity)
						'Me.UpdateStatusHistory(si, globals, api, args, newREQStatus)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
					Catch ex As Exception
					End Try
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
'BRApi.ErrorLog.LogMessage(si, "END SubmitAllREQs in REQ_SolutiomHelper")
			Return Nothing
		End Function	

#End Region

#Region "Approve REQs from Validate"
		'Created: KN - 09/25/2024 - Ticket 1708	
		'Created to submit multiple REQs for prioritization 
		Public Function ApprovalREQsVal(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sProfileSubString As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim reqTime As String = WFYear & "M12"
				Dim reqTime As String = WFYear
			
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")
				For Each row As DataRow In oReqList.Rows
					Dim reqEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
					Dim reqFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
'BRApi.ErrorLog.LogMessage(si, $"EntityFlow = {row("EntityFlow")} || reqEntity = {reqEntity} || reqFlow = {reqFlow}")					
				
					'------------Get current REQ workflow status-------------
					Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"REQMemberScript = {REQMemberScript}")
					Dim currREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
					If(String.IsNullOrWhiteSpace(currREQStatus)) Then
						Return Nothing
					End If
					
					Dim REQValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim REQApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & reqEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim newREQStatus As String = ""
				
					If sProfileSubString = "Validate Requirements" Then
	'					If sValidatedAmount = 0 Then 
	'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
	'					End If
						currREQStatus = currREQStatus.Substring(0,2)
						newREQStatus = currREQStatus & " Ready for Approval"
						
					Else If sProfileSubString = "Validate Requirements CMD"
	'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
	'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
	'					End If
						currREQStatus = currREQStatus.Substring(0,2)
						newREQStatus = currREQStatus & " Ready for Approval"
					End If
					
                    'Update REQ Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = REQMemberScript
					objScriptValStatus.TextValue = newREQStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, reqFlow, reqEntity)
						'Me.UpdateStatusHistory(si, globals, api, args, newREQStatus)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, reqFlow, reqEntity)
					Catch ex As Exception
					End Try
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region ' New

#Region "Update Status History"
		
		'Updates status history account with the passed in status in a comma delimited string
		'Updated EH 08292024 RMW-1565 Updated to annual for PGM_C20XX scenario
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function UpdateStatusHistory(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal status As String =  "", Optional ByVal REQ As String =  "", Optional ByVal Entity As String =  "", Optional ByVal FundCenter As String =  "") As String
			Try			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				'Dim REQTime As String = WFYear & "M12"
				Dim REQTime As String = WFYear
		
				'Original code
				Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity", Entity)
				Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow", REQ)
				Dim reqFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter", FundCenter)
'brapi.ErrorLog.LogMessage(si,"reqFundCenter " & reqFundCenter)
				Dim reqstatus As String = args.NameValuePairs.XFGetValue("status", status)			
				'reqstatus = reqFundCenter & " " & reqstatus
'brapi.ErrorLog.LogMessage(si,"reqstatus " & reqstatus)
				Dim updatedBy As String = si.AuthToken.UserName
				Dim UpdateDate As Date = DateTime.Now
				Dim completeReqStatus As String = updatedBy & " : " & UpdateDate & " : " & reqFundCenter & " : " & reqstatus
				
				If(Not String.IsNullOrWhiteSpace(reqstatus)) Then 				
					'Get current status of REQ
					Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & REQTime & ":V#Annotation:A#REQ_Status_History:F#" & reqFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim statusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation						

					If (String.IsNullOrWhiteSpace(statusHistory)) Then
						statusHistory = completeReqStatus
					
					Else
						statusHistory = statusHistory & ", " & completeReqStatus						
					End If
                    
					'Update REQ Status History
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = REQMemberScript
					objScriptValStatus.TextValue = statusHistory
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region 'updated

#Region "Cache Prompts"
		Public Function CachePrompts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sREQ As String = args.NameValuePairs.XFGetValue("REQ")
			Dim sMode As String = args.NameValuePairs.XFGetValue("mode","")
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			
			If sMode.XFContainsIgnoreCase("copyREQ") And String.IsNullOrWhiteSpace(sEntity) Then
				Throw New Exception("Please select a Fund Center")
				Return Nothing
			End If
			
			If Not String.IsNullOrWhiteSpace(sEntity) Then
					BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity",sEntity)
			End If
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ",sREQ)
			
			If Not String.IsNullOrWhiteSpace(sDashboard) Then
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Dashboard",sDashboard)
			End If

			Return Nothing
		End Function

#End Region '(updated here)

#Region "Set Related REQs"
'Updated by Fronz 09/06/2024 - changed the S# to REQ_Shared and T# to 1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function SetRelatedREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","") 'args.NameValuePairs.XFGetValue("UFREntity")
			Dim sUFR As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("REQScenario")
			Dim sUFRTime As String = args.NameValuePairs.XFGetValue("REQTime")
'			Dim sScenario As String = "REQ_Shared"
'			Dim sUFRTime As String = "1999"
			Dim sRelatedUFRs As String = args.NameValuePairs.XFGetValue("RelatedREQs")
			Dim RelatedUFRLength As Integer = sRelatedUFRs.Length
			Dim UFRList As String() = sRelatedUFRs.Split(","c).Select(Function(UFR) UFR.Trim()).ToArray()
'brapi.ErrorLog.LogMessage(si,"UfrList = " & RelatedUFRLength)
			If RelatedUFRLength = 0 Then 
				Return Nothing
			End If
'brapi.ErrorLog.LogMessage(si,"UFR = " & sUFR)			
			Dim UFRtitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Title:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRtitleMemberScript).DataCellEx.DataCellAnnotation
			Dim mainUFR As String = ""
			If sEntity.XFContainsIgnoreCase("_General") Then 
				mainUFR = $"{sEntity.Replace("_General","")} - {sUFR} - {TitleValue}"		
			Else
				mainUFR = $"{sEntity} - {sUFR} - {TitleValue}"
			End If
			
			Dim RelatedUFRMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Related_Request:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'brapi.ErrorLog.LogMessage(si,"RelatedUFRMemberScript = " & RelatedUFRMemberScript)			
			Dim currentUFRs As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, RelatedUFRMemberScript).DataCellEx.DataCellAnnotation

			For Each UFR As String In UFRList
				If UFR.XFContainsIgnoreCase(mainUFR) Then Continue For
				Dim isGeneral As Boolean = UFR.Split("-")(0).Trim().XFContainsIgnoreCase("_General")
				If isGeneral Then UFR = UFR.Replace(UFR.Split("-")(0).Trim(),UFR.Split("-")(0).Trim().Replace("_General",""))
				If (Not currentUFRs.XFContainsIgnoreCase(UFR)) Then
					If (String.IsNullOrWhiteSpace(currentUFRs)) Then				
						currentUFRs = UFR
					Else
						currentUFRs = currentUFRs & ", " & UFR	
					End If
				End If
			
			Next
'brapi.ErrorLog.LogMessage(si,"currentUFRs = " & currentUFRs)						
           'Update related UFR List
			Dim objListofScripts As New List(Of MemberScriptandValue)
		    Dim objScriptVal As New MemberScriptAndValue
			
			objScriptVal.CubeName = sCube
			objScriptVal.Script = RelatedUFRMemberScript
			objScriptVal.TextValue = currentUFRs
			objScriptVal.IsNoData = False
			objListofScripts.Add(objScriptVal)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
			
			'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, sUFR, sEntity)
			Catch ex As Exception
			End Try
			
			'Update the other side - set the relate UFRs
			
			For Each relatedUFR As String In UFRList
				If relatedUFR.XFContainsIgnoreCase(mainUFR) Then Continue For
				Dim UFR = relatedUFR.Split("-")(1).Trim()
				sEntity = relatedUFR.Split("-")(0).Trim()
				RelatedUFRMemberScript = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Related_Request:F#" & UFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				currentUFRs = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, RelatedUFRMemberScript).DataCellEx.DataCellAnnotation
				If (Not currentUFRs.XFContainsIgnoreCase(mainUFR)) Then
					If (String.IsNullOrWhiteSpace(currentUFRs)) Then
						currentUFRs = mainUFR
					Else
						currentUFRs = currentUFRs & ", " & mainUFR	
					End If
				End If

				objScriptVal.CubeName = sCube
				objScriptVal.Script = RelatedUFRMemberScript
				objScriptVal.TextValue = currentUFRs
				objScriptVal.IsNoData = False
				objListofScripts.Add(objScriptVal)
			
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, UFR, sEntity)
				Catch ex As Exception
				End Try
			Next
			Return Nothing
		End Function

#End Region '(updated here)

#Region "Approve UFR"
		Public Function ApproveUFR(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
		
			Dim sUFR As String = args.NameValuePairs.XFGetValue("ufrFlow")	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("ufrEntity")
			'Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
'Brapi.ErrorLog.LogMessage(si,"FundCenter" & sFundCenter & ", ufrEntity : " & sEntity & ", ufrflow: " & sUFR )			
			'Validate for Approval Allowance Status	
			If Not Me.IsUFRApprovalAllowed(si, sEntity, args) Then
				Throw New Exception("Cannot approve UFR at this time. Contact UFR manager." & environment.NewLine)	
			End If
			'Done Validation
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRTimeYr As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sUFRWFStatus As String = args.NameValuePairs.XFGetValue("UFRStatus")
			Dim sUFRFundingStatus As String = args.NameValuePairs.XFGetValue("FundingStatus")
			Dim sUFRFundingStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Funding_Status:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sUFRWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim UFRUpdated As Boolean = False
			Dim sHistoricalWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRWFStatusMemberScript).DataCellEx.DataCellAnnotation		
'			Dim sUFRFundingStatusMemberScriptValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRFundingStatusMemberScript).DataCellEx.DataCellAnnotation
			
			Dim sWFProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			Dim sWFLevel As String = sWFProfileName.Substring(0,2)
'			Dim iFundingAmtAccount = "UFR_Funded_Amt_" & sWFLevel
		    Dim sUFRiFundedAmtMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTimeYr & ":V#Periodic:A#UFR_Funded_Amt:F#" & sUFR & ":O#BeforeAdj:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			Dim sUFRiRequestedAmtMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTimeYr & ":V#Periodic:A#UFR_Requested_Amt:F#" & sUFR & ":O#BeforeAdj:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			Dim iFundedAmt As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRiFundedAmtMemberScript).DataCellEx.DataCell.CellAmount
			Dim iRequestedAmt As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRiRequestedAmtMemberScript).DataCellEx.DataCell.CellAmount

'Brapi.ErrorLog.LogMessage(si, "UFRFundingStatusValue " & sUFRFundingStatusMemberScriptValue)		
			'Validate that UFR Title Is Not empty
			If Not String.IsNullOrWhiteSpace(sUFR) Then
				'Funding Status validation check - Fronz 11.28.2023
				If (iFundedAmt > 0) And (iFundedAmt < iRequestedAmt) And (sUFRFundingStatus <> "Partially Funded") Then
					Throw New Exception("Warning: Update Funding Status." & environment.NewLine & "Funding Status should be set to Partially Funded because Total Funded Amount is greater than zero and less than the Requested Amount." & environment.NewLine)
				ElseIf  iFundedAmt <> 0 And iFundedAmt >= iRequestedAmt And sUFRFundingStatus <> "Internally Funded"
				    Throw New Exception("Warning: Update Funding Status." & environment.NewLine & "Funding Status should be set to Internally Funded because Total Funded Amount is greater than or equal to the Requested Amount." & environment.NewLine)
				ElseIf  (iFundedAmt = 0) And (sUFRFundingStatus <> "Unfunded" And sUFRFundingStatus <> "Non-Concur/Obsolete")
				    Throw New Exception("Warning: Update Funding Status." & environment.NewLine & "Funding Status should be set to Unfunded or Non-Concur/Obsolete because Total Funded Amount equals 0.")	
				
				End If

				'Update UFR Funding Status 
			    If Not String.IsNullOrWhiteSpace(sUFRFundingStatus) Then
					Dim objListofScriptsFundingStatus As New List(Of MemberScriptandValue)
					Dim objScriptValFundingStatus As New MemberScriptAndValue
					objScriptValFundingStatus.CubeName = sCube
					objScriptValFundingStatus.Script = sUFRFundingStatusMemberScript
					objScriptValFundingStatus.TextValue = sUFRFundingStatus
					objScriptValFundingStatus.IsNoData = False
					objListofScriptsFundingStatus.Add(objScriptValFundingStatus)
					
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsFundingStatus)
					UFRUpdated = True
				End If	
				
				'Update UFR Workflow Status
				If Not String.IsNullOrWhiteSpace(sUFRWFStatus) Then
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = sUFRWFStatusMemberScript
					objScriptValWFStatus.TextValue = sUFRWFStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
					
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
					UFRUpdated = True
				End If
			End If
				
				
			If UFRUpdated Then
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, sUFRWFStatus)
				Catch ex As Exception
				End Try
			
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, sUFR, sEntity)
				Catch ex As Exception
				End Try
			End If
			
			Return UFRUpdated
		End Function
#End Region 'updated

#Region "Release UFR"
		Public Function ReleaseUFR(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
		
			Dim sUFR As String = args.NameValuePairs.XFGetValue("ufrFlow")	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("ufrEntity")
			
			'Validate for Approval Allowance Status	
			If Not Me.IsUFRApprovalAllowed(si, sEntity, args) Then
				Throw New Exception("Cannot approve UFR at this time. Contact UFR manager." & environment.NewLine)	
			End If
'			If Not Me.IsParentUFRApprovalAllowed(si, sEntity) Then 
'				Throw New Exception("Cannot approve UFR at this time. Upper level is currently not allowing UFR submission" & environment.NewLine)	
'			End If
			'Done Validation
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRTimeYr As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim sWfLv As String = Me.GetWorkflowLevel(si)
			'Dim sNextWfLv As String = Me.GetNextWorkflowLevel(si)
			Dim sUFRWFStatus As String =  ""
'			If sWfLv = "L2" Then 
'				sUFRWFStatus = $"{sWfLv} Release Draft to HQ"
'			Else
'				sUFRWFStatus = $"{sWfLv} Release Draft to {sNextWfLv}"
'			End If

			Dim UFRWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim ufrUpdated As Boolean = False
			Dim HistoricalWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRWFStatusMemberScript).DataCellEx.DataCellAnnotation		
		
				'Validate that UFR Title is not empty
			If Not String.IsNullOrWhiteSpace(sUFR) Then

				'Update UFR Workflow Status - Release Draft
				If Not String.IsNullOrWhiteSpace(sUFRWFStatus) Then
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = UFRWFStatusMemberScript
					objScriptValWFStatus.TextValue = sUFRWFStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
					
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
					ufrUpdated = True
				End If
			End If
			
			
			If ufrUpdated Then
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, sUFRWFStatus)
				Catch ex As Exception
				End Try
			
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, sUFR, sEntity)
				Catch ex As Exception
				End Try
			End If
			
			Return ufrUpdated
		End Function
#End Region 'updated

#Region "Demote REQ"
		'Updated 07252024 Mikayla RMW-1477
		'Updated 08292024 EH RMW-1565 sUFRTime updated to annual for PGM_C20XX scenario
		'Updated 07282025 JM RMW-1207 Changed demotion process to demote requirements to last status in status history
		Public Function DemoteREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim sComment As String = args.NameValuePairs.XFGetValue("demotionComment")
			'Demotion comment can't be blank
			If sComment = "" Then
				Throw New Exception ("You must enter a comment for demotion.")
			End If
			
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")							
'brapi.ErrorLog.LogMessage(si,"sREQ = " & sREQ & ": sEntity = " & sEntity)
			'Set variables to be used in member script
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Create member script string to be used in the member script object
			Dim REQWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Status_History:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
			Dim REQdemoteMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Return_Cmt:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
			Dim REQNewStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
	
			
Dim Statushiststring As String  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQWFStatusMemberScript).DataCellEx.DataCellAnnotation				
Dim laststatusstring As String = ""
Dim lastcommaindex() As String = Statushiststring.Split(","c)

If lastcommaindex.Length >=2 Then 
laststatusstring = lastcommaindex(lastcommaindex.Length - 2)
End If 

'brapi.ErrorLog.LogMessage(si,"status hist = " & laststatusstring)


Dim Statushistvlaue As String  = laststatusstring
Dim laststatusvalue As String = ""
Dim lastvalueindex As Integer = Statushistvlaue.LastIndexOf(":"c)

If lastvalueindex >=0 Then 
	laststatusvalue = (Statushistvlaue.Substring(lastvalueindex + 1)).Trim()
	Else
	laststatusvalue = Statushistvlaue.Trim()
End If 

'brapi.ErrorLog.LogMessage(si,"sNewREQWFStatus = " & laststatusvalue)

		

'Validate that REQ Title is not empty
			If Not String.IsNullOrWhiteSpace(sREQ) Then
						
				'Update new REQ workflow status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = REQNewStatusMemberScript
				objScriptValWFStatus.TextValue = laststatusvalue
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				'Update the demotion comment
				Dim objListofScriptsDemotion As New List(Of MemberScriptandValue)
			    Dim objScriptValDemotion As New MemberScriptAndValue
				objScriptValDemotion.CubeName = sCube
				objScriptValDemotion.Script = REQdemoteMemberScript
				objScriptValDemotion.TextValue = sComment
				objScriptValDemotion.IsNoData = False
				objListofScriptsDemotion.Add(objScriptValDemotion)					
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDemotion)
				
			End If	
			
			'Update REQ workflow status history
			Try						
				Me.UpdateStatusHistory(si, globals, api, args, laststatusvalue,sREQ,sEntity,sEntity)						
			Catch ex As Exception
			End Try
		
			'Send email
			Try
				Me.SendStatusChangeEmail(si, globals, api, args)
			Catch ex As Exception
			End Try
			
			'Update date and name
			Try
				Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
			Catch ex As Exception
			End Try
			
			Return Nothing
		End Function
#End Region

#Region "DemotionCreateUFRValidation"
		Public Function DemotionCreateUFRValidation(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sFundCenterSelected As String = args.NameValuePairs.XFGetValue("SelectedFundCenter")
			Dim sUFR As String = args.NameValuePairs.XFGetValue("UFR")
			
			If sEntity = sFundCenterSelected Then
				Throw New Exception ("You can only demote subordinate fund center UFRs.")
			Else
				CachePrompts(si, globals, api, args)
			End If	

		Return Nothing
End Function


#End Region

#Region "Recall UFR for Re-Prioritization"
		'RMW-1012 Recall UFRs for Re-Prioritization - set WF Status from 'Ready For Disposition' to 'Ready For Prioritization'
		Public Function PrioritizeRecall(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			'Set variables to be used in member script
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRTimeYr As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim sWfLv As String = Me.GetWorkflowLevel(si)
			'If nothing is selected, escape
			If String.IsNullOrWhiteSpace(args.NameValuePairs.XFGetValue("UFRsToRecall")) Then Return Nothing
				
			Dim UFRsToRecall As List(Of String) = args.NameValuePairs.XFGetValue("UFRsToRecall").Split(",").ToList
			'Set status variables
			Dim sUFRWFStatus As String =  ""
			'sUFRWFStatus = $"{sWfLv} Ready For Prioritization"
			Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Entity","")
	
			For Each UFR As String In UFRsToRecall						
				Dim sUFREnt As String = UFR.Split(":")(0).Trim()
				Dim sUFRFlow As String = UFR.Split(":")(1).Split("-")(0).Trim()

				' Create member script string to be used in the member script object
				Dim UFRWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sUFREnt & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & sUFRFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"				
			
				' Set WF status to '{WfLevel} Ready For Prioritization'
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
				Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = UFRWFStatusMemberScript
				objScriptValWFStatus.TextValue = sUFRWFStatus
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
							
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, sUFRWFStatus, sUFRFlow, sUFREnt, sFundCenter)
				Catch ex As Exception
				End Try
				
				'Send status change email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args, sUFRFlow, sUFREnt)
				Catch ex As Exception
				End Try
					
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, sUFRFlow, sUFREnt)
				Catch ex As Exception
				End Try		
							
			Next
			
			'Send email to Approver(s) to notify the change
			Try
				Me.PrioritizeRecallEmail(si, globals, api, args, UFRsToRecall)
			Catch ex As Exception
			End Try	
			
			Return Nothing
		End Function
#End Region 'updated

#Region "Send UFR Recall Email"
		'RMW-1076 Recall UFRs for Re-Prioritization - send email to Approver when UFR(s) get recalled to Prioritize'
		Public Function PrioritizeRecallEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal lUFRs As List(Of String))
			'Args To pass Into dataset BR
			Dim dsArgs As New DashboardDataSetArgs 
			dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
			dsArgs.DataSetName = "GetAllUsers"
			dsArgs.NameValuePairs.XFSetValue("mode", "AP")	
			'Get datatable for emails list
			Dim dt As DataTable = BR_REQDataset.GetAllUsers(si, globals, api, dsArgs)
			'Extract email list
			Dim lApproverEmails As New List(Of String)
		
			For Each row As DataRow In dt.Rows
				lApproverEmails.Add(CStr(row("Value")))		
			Next
			'Set Email infos & Send
			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")	
			Dim bodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
			Dim emailSubject As String = "UFR Recall Notification"
			Dim userEmail As String = BRApi.Security.Admin.GetUser(si, si.UserName).User.Email
			Dim emailBody As String = $"The following UFR(s) have been recalled for re-prioritization by {si.UserName} ({userEmail}):" 
			For Each UFR As String In lUFRs
				emailBody = emailBody & vbCrLf & "- " & UFR.Trim()
			Next
			
			emailBody = emailBody & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
			BRApi.Utilities.SendMail(si, EmailConnectorStr, lApproverEmails, emailSubject, emailBody, Nothing)	
		
		Return Nothing
	End Function
#End Region

#Region "Send Status Change Email "
	'Updated EH 08292024 RMW-1565 Updated sREQTime to annual for PGM_C20XX and Centralized Text (REQ_Shared, 1999)
	'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
	Public Function SendStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal REQ As String =  "", Optional ByVal Entity As String =  "")
		'Usage: {UFR_SolutionHelper}{SendStkhldrEmail}{FundCenter=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_UserFundCenters__Shared!|],UFR=[|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|],StakeHolderEmails=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_StakeholderEmailList__Shared!|]}
		Try
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("reqEntity", Entity)
			Dim sREQid As String = args.NameValuePairs.XFGetValue("reqFlow", REQ)	
			Dim userName As String = si.UserName

			'Title Member Script
			Dim REQEntityTitleMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sREQTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityTitleMemberScript).DataCellEx.DataCellAnnotation
			
			'Status Member Script
			Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sREQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation
			
			'Creator Name Member Script
			Dim REQEntityCreatorNameMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creator_Name:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sREQCreatorName As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityCreatorNameMemberScript).DataCellEx.DataCellAnnotation
			
			'Creation Data Member Script
			Dim REQEntityCreationDateMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sREQCreationDate As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityCreationDateMemberScript).DataCellEx.DataCellAnnotation	

			'Creation Data Member Script
			Dim REQEmailNotificationMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Notification_Email_List:F#" & sREQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim REQStatusEmailList As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEmailNotificationMemberScript).DataCellEx.DataCellAnnotation	
'Brapi.ErrorLog.LogMessage(si, "REQStatusEmailList: " & REQStatusEmailList)

			'Variables to set up email functionality 
			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
			Dim BodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
			Dim StatusChangeEmails  As New List(Of String) 
			StatusChangeEmails.AddRange(REQStatusEmailList.Split(","))
			'//Status Change Email\\
			Dim statusChangeSubject As String = "Requirement Status Change"
			'Build Body
			Dim statusChangeBody As String = "A Requirement Request for Fund Center: " & sFundCenter & " with Requirement Title: "  & sREQid & " - " & sREQTitle &  " has changed status to '" & sREQStatus & "' " & vbCrLf & "Submitted by: " & userName & " - " & sREQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & BodyDisclaimerBody
			'Send email			
			If Not String.IsNullOrWhiteSpace(REQStatusEmailList) Then
'Brapi.ErrorLog.LogMessage(si, "hits the send: " & EmailConnectorStr)
				BRApi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmails, statusChangeSubject, statusChangeBody, Nothing)	
			End If
			'Executes the "SendReviewRequestEmail" function and sends an email to the resepective CMD roles within it
			If sREQStatus = "Ready for Financial Review" Or sREQStatus = "Ready for Validation"
				Me.SendReviewRequestEmail(si, globals, api, sFundCenter, sREQid, sREQTitle, sREQCreatorName, sREQCreationDate)
			End If			
		Return Nothing
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
	End Function

#End Region  'update here

#Region "Send Review Request Email"
'------------------------------------------------------------------------------------------------------------
'Creator(04/08/2024): Kenny, Connor, Fronz
'
'Description: Sends a request-for-review email to the CMD role that is next in the Requirements life cycle
'
'Usage: SendReviewRequestEmail is called from the SendStatusChangeEmail function
	   'SendReviewRequestEmail uses REQDataSet.GetAllUsers to return dt of user emails.			
	Public Function SendReviewRequestEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal FundCenter As String, ByVal REQid As String, ByVal REQTitle As String, ByVal REQCreatorName As String, ByVal REQCreationDate As String ) As Object
		Try
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity",FundCenter)
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(".")(1)
			Dim bodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
			Dim DataSetArgs As New DashboardDataSetArgs
			Dim requestEmailSubject As String = ""
			Dim requestEmailBody As String = ""
			Dim validatorEmailsScript As String = ""
			Dim validatorEmails As String = ""

			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
			'Set args to return all user emails assigned to the resepective Fund Center & "Review Financials" security group 
			'Create email subject and body
			If wfProfileName = "Formulate Requirement" Then
				DataSetArgs.NameValuePairs.XFSetValue("mode","FC_RF")
				'requestEmailSubject = "Request for Requirement Financial Review"
				'requestEmailBody = "A requirement has been submitted for financial review for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
				requestEmailSubject = "Request for Requirement Validation"
				requestEmailBody = "A requirement has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
				
			End If
			'Set args to return all user emails assigned to the resepective Fund Center & "Validate Requirements" security group  (Commented out for now RMW-1283) 
			'Create email subject and body
			If wfProfileName = "Review Financials" Then
				validatorEmailsScript = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Validation_Email_List:F#" & REQid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				validatorEmails  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, validatorEmailsScript).DataCellEx.DataCellAnnotation
				requestEmailSubject = "Request for Requirement Validation"
				requestEmailBody = "A requirement has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Requirement: " & REQid & " - " & REQTitle & vbCrLf & "Created: " & REQCreatorName & " - " & REQCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
			End If
			
			'Call the REQDataSet and return a datetable (dt) of users' emails
			Dim dtReviewUserEmails  As DataTable =  BR_REQDataSet.GetAllUsers(si, globals ,api , DataSetArgs)
			'Create new list of users' emails from datatable
			Dim lReviewEmails As New List(Of String)
			Dim vaReviewEmails As New List(Of String)
			
			For Each row As DataRow In dtReviewUserEmails.Rows
				lReviewEmails.Add(CStr(row("Value")))		
			Next
'BRapi.ErrorLog.LogMessage(si,$"wfProfileName = {wfProfileName} || validatorEmailsScript = {validatorEmailsScript} || validatorEmails = {validatorEmails}")		
			
			'Build and send email
			BRApi.Utilities.SendMail(si, EmailConnectorStr, lReviewEmails, requestEmailSubject, requestEmailBody, Nothing)	

			If wfProfileName = "Review Financials" Then
				vaReviewEmails = validatorEmails.Split(",").ToList()
				BRApi.Utilities.SendMail(si, EmailConnectorStr, vaReviewEmails, requestEmailSubject, requestEmailBody, Nothing)	
			End If
			
			Return Nothing	
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
End Function	
		
#End Region 'update here

#Region "Attach Doc"
		Public Function GetSupportDocDataTableCV(ByVal si As SessionInfo, ByVal includeFileBytes As Boolean) As DataTable
			Try
				Dim sql As New Text.StringBuilder                                                  
				If includeFileBytes Then
					sql.Append("Select * ")
				Else     
					sql.Append("Select ")
					sql.Append("UniqueID, ")
					sql.Append("Cube, ")
					sql.Append("Entity, ")
					sql.Append("Parent, ")
					sql.Append("Cons, ")
					sql.Append("Scenario, ")
					sql.Append("Time, ")
					sql.Append("Account, ")
					sql.Append("Flow, ")
					sql.Append("Origin, ")
					sql.Append("IC, ")
					sql.Append("UD1, ")
					sql.Append("UD2, ")
					sql.Append("UD3, ")
					sql.Append("UD4, ")
					sql.Append("UD5, ")
					sql.Append("UD6, ")
					sql.Append("UD7, ")
					sql.Append("UD8, ")
					sql.Append("Title, ")
					sql.Append("AttachmentType, ")
					sql.Append("CreatedUserName, ")
					sql.Append("CreatedTimestamp, ")
					sql.Append("LastEditedUserName, ")
					sql.Append("LastEditedTimestamp, ")
					sql.Append("Text, ")
					sql.Append("FileName, ")                  
				End If
				sql.Append("From dbo.DataAttachment With (NOLOCK) ")

				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					Return BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, False)
				End Using                               

				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try                       
		End Function
#End Region

#Region "Save General Accounts Annotation Function"	
		'This function is used to save annotations for UFR inputs
		'RMW-1083 - updated to incorporate new structure for UD5 stakeholder members 
		'RMW-1565 EH 08/22/24 - Updated comment accounts to S#REQ_Shared, T#1999
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function SaveGenAcctAnnotation(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
					
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Passed In Parameters. Required to pass in null value if Account is not used.
			Dim sFlow As String = args.NameValuePairs.XFGetValue("REQFlow").Trim
			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity").Trim
			Dim sComment As String = args.NameValuePairs.XFGetValue("Comment")
			Dim sPOCComment As String = args.NameValuePairs.XFGetValue("POCComment")
			Dim sREQAccount As String = args.NameValuePairs.XFGetValue("REQAccount").Trim
			Dim sU5 As String = args.NameValuePairs.XFGetValue("U5").Trim
			Dim sPOCAcct As String = args.NameValuePairs.XFGetValue("POCAccount").Trim

		
			Dim sInfoMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#" & sREQAccount & ":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sPOCMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#" & sPOCAcct & ":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#None"
		
'			Added by Fronz 11/08/2024
			'Validate the fundling contains one value greater than zero under its FY requested amount columns
'			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")

			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)

			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))	

			Dim iSum As Long = 0

			For i As Integer = 0 To 4 Step 1 
				Dim myDataUnitPk As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

				' Buffer coordinates.
				' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, "REQ_Requested_Amt")
				myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, sFlow)
				myDbCellPk.OriginId = DimConstants.BeforeAdj
				myDbCellPk.UD7Id = DimConstants.None
				myDbCellPk.UD8Id = DimConstants.None	


				Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)	
		
				For Each cell As DataCell In myCells	
					iSum += cell.CellAmount

				Next
			Next
			If iSum = 0 Then Throw New Exception("Please input a value larger than zero." & environment.NewLine)
		
	'===========================Delete=========================================
			
'			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
''BRApi.ErrorLog.LogMessage(si, "TIME " & sREQTime)				
			
'			'if RequestedAttributeType amount is blank we default to 1. This is needed for copy.
			
'			'During create there is an exception trown for blank amount and wont get here
'			Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")
'BRApi.ErrorLog.LogMessage(si, "SRequestedAMT before " & sRequestedAmt)
'			If String.IsNullOrWhiteSpace(sRequestedAmt) Then sRequestedAmt = "1"
''BRApi.ErrorLog.LogMessage(si, "SRequestedAMT after " & sRequestedAmt)

'			Dim sAPPNFund As String = args.NameValuePairs.XFGetValue("APPNFund")
'			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")		
'			Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
'			Dim sDollarType As String = args.NameValuePairs.XFGetValue("DollarType")
'			Dim sCommimentItem As String = args.NameValuePairs.XFGetValue("CommitmentItem")
			
'			If String.IsNullOrWhiteSpace(sAPPNFund) Then sAPPNFund = "None"
'			If String.IsNullOrWhiteSpace(sMDEP) Then sMDEP = "None"
'			If String.IsNullOrWhiteSpace(sAPE) Then sAPE = "None"
'			If String.IsNullOrWhiteSpace(sDollarType) Then sDollarType = "None"
'			If String.IsNullOrWhiteSpace(sCommimentItem) Then sCommimentItem = "None"
				
'			'On the initial creation of REQs, the IC will be the creating entity
'			Dim IC As String = sEntity

'			'Dim REQRequestedAmttMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#REQ_Requested_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommimentItem & ":U7#None:U8#None"

	'============================delete^^^^^+===========================		
			

			
			'set cell for general info comment
			Dim objListofScriptsComment As New List(Of MemberScriptandValue)
		    Dim objScriptValComment As New MemberScriptAndValue
			objScriptValComment.CubeName = sCube
			objScriptValComment.Script = sInfoMemberScript
			objScriptValComment.TextValue = sComment
			objScriptValComment.IsNoData = False
			objListofScriptsComment.Add(objScriptValComment)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsComment)
			
			'set cell for POC Comment	
			Dim objListofScriptsPOC As New List(Of MemberScriptandValue)
		    Dim objScriptValPOC As New MemberScriptAndValue
			objScriptValPOC.CubeName = sCube
			objScriptValPOC.Script = sPOCMemberScript
			objScriptValPOC.TextValue = sPOCComment
			objScriptValPOC.IsNoData = False
			objListofScriptsPOC.Add(objScriptValPOC)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsPOC)
			
			'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, sFlow, sEntity)
			Catch ex As Exception
			End Try
				
			Return Nothing
		End Function
#End Region	'(Update)

#Region "Set Notification List"

	Public Function SetNotificationList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
	'Added a section to show only validators on the list and seperated from all users - 5-30-24
	
		Try
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","") 'args.NameValuePairs.XFGetValue("UFREntity")
			Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")			
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("REQScenario")
			Dim sREQTime As String = args.NameValuePairs.XFGetValue("REQTime")
			Dim notificationEmails As String = args.NameValuePairs.XFGetValue("Emails")
			Dim vNotificationEmails As String = args.NameValuePairs.XFGetValue("vEmails")
				
			If notificationEmails.Length = 0 And vNotificationEmails.Length = 0 Then 
				Return Nothing
			End If
			
			Dim emailList As String() = notificationEmails.split(",")
			Dim vemailList As String() = vNotificationEmails.split(",")

			Dim notificationEmailsScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Notification_Email_List:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim notificationValidatorEmailsScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Validation_Email_List:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		
			Dim currentEmails As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, notificationEmailsScript).DataCellEx.DataCellAnnotation
			Dim validatorEmails As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, notificationValidatorEmailsScript).DataCellEx.DataCellAnnotation
			
			'loop through all users
			For Each email As String In emailList
				If (Not currentEmails.XFContainsIgnoreCase(email)) Then
					If (String.IsNullOrWhiteSpace(currentEmails)) Then
						currentEmails = email
					Else
						currentEmails = currentEmails & "," & email	
					End If
				End If
				
			Next
			
			'loop through validators
			For Each vemail As String In vemailList
				If (Not validatorEmails.XFContainsIgnoreCase(vemail)) Then
					If (String.IsNullOrWhiteSpace(validatorEmails)) Then
						validatorEmails = vemail
					Else
						validatorEmails = validatorEmails & "," & vemail	
					End If
				End If
			Next
			
			'Update related REQ List
			Dim objListofScripts As New List(Of MemberScriptandValue)
		   
			'for all user
			Dim objScriptVal As New MemberScriptAndValue
			
			' for Validator
			Dim objScriptVal2 As New MemberScriptAndValue
			
			'Setting for all users
			objScriptVal.CubeName = sCube
			objScriptVal.Script = notificationEmailsScript
			objScriptVal.TextValue = currentEmails
			objScriptVal.IsNoData = False
			objListofScripts.Add(objScriptVal)
			
			'setting for validators
			objScriptVal2.CubeName = sCube
			objScriptVal2.Script = notificationValidatorEmailsScript
			objScriptVal2.TextValue = validatorEmails
			objScriptVal2.IsNoData = False
			objListofScripts.Add(objScriptVal2)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
			
			'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
			Catch ex As Exception
			End Try
				
			Return Nothing

		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try 
	End Function

#End Region

#Region "Show and Hide"

		Public Function ShowAndHideDashboards(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Dim checkForBlank As String = args.NameValuePairs.XFGetValue("checkForBlank")
				'blank dashboard set to trueVale
				Dim trueVale As String = args.NameValuePairs.XFGetValue("trueValue")
				'dashboard content to show (eventually) set to falseValue
				Dim falseValue As String = args.NameValuePairs.XFGetValue("falseValue")
				Dim allTimeValue As String = args.NameValuePairs.XFGetValue("allTimeValue")
				'cache prompts for Linked Report
				Dim sDb As String = args.NameValuePairs.XFGetValue("Db","")
				If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"LR_REQ_Prompts","Db",sDb)
				Dim sFC As String = args.NameValuePairs.XFGetValue("FundCenter","")
				If Not String.IsNullOrWhiteSpace(sFC) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"LR_REQ_Prompts","FC",sFC)
'BRApi.ErrorLog.LogMessage(si, "sDb: " & sDb)				
				Dim toShow As String = ""
				Dim toHide As String = ""
'BRApi.ErrorLog.LogMessage(si, "checkForBlank: " & checkForBlank)
'BRApi.ErrorLog.LogMessage(si, "truevale: " & trueVale)
'BRApi.ErrorLog.LogMessage(si, "falsevalue: " & falseValue)
'BRApi.ErrorLog.LogMessage(si, "allTimeValue: " & allTimeValue)
				If (String.IsNullOrWhiteSpace(allTimeValue)) Then
					If (String.IsNullOrWhiteSpace(checkForBlank)) Then
'BRApi.ErrorLog.LogMessage(si, "show blnkDB IF statement; no titlebox selection:")
						toShow =  trueVale
						toHide =  falseValue
					Else
'BRApi.ErrorLog.LogMessage(si, "show contentDB Else statement; does not contain IsNullOrWhiteSpace:")
						toShow =  falseValue
						toHide =  trueVale
'BRApi.ErrorLog.LogMessage(si,"toShow: " & toShow & ", toHide: " & toHide )
					End If
				Else ' If all time is set, the check is bypassed
'BRApi.ErrorLog.LogMessage(si, "Last Else statement - 1146")
					toShow =  trueVale
					toHide =  falseValue
				End If
'BRApi.ErrorLog.LogMessage(si, "allTimeValue: " & allTimeValue & " checkForBlank: " & checkForBlank & ", trueValue: " & trueVale & " , falseValue: " & falseValue)
				
'BRApi.ErrorLog.LogMessage(si,"toShow: " & toShow & ", toHide: " & toHide )

				Dim objXFSelectionChangedUIActionInfo As XFSelectionChangedUIActionInfo = args.SelectionChangedTaskInfo.SelectionChangedUIActionInfo
'BRApi.ErrorLog.LogMessage(si, $"objXFSelectionChangedUIActionInfo.DashboardsToHide: {objXFSelectionChangedUIActionInfo.DashboardsToHide}")				
'BRApi.ErrorLog.LogMessage(si, $"objXFSelectionChangedUIActionInfo.DashboardsToShow: {objXFSelectionChangedUIActionInfo.DashboardsToShow}")								
				If String.IsNullOrWhiteSpace(objXFSelectionChangedUIActionInfo.DashboardsToHide) Then
					objXFSelectionChangedUIActionInfo.DashboardsToHide = toHide
				Else
					objXFSelectionChangedUIActionInfo.DashboardsToHide = objXFSelectionChangedUIActionInfo.DashboardsToHide & "," & toHide
				End If
				If String.IsNullOrWhiteSpace(objXFSelectionChangedUIActionInfo.DashboardsToShow) Then
					objXFSelectionChangedUIActionInfo.DashboardsToShow = toShow
				Else
					objXFSelectionChangedUIActionInfo.DashboardsToShow = objXFSelectionChangedUIActionInfo.DashboardsToShow & "," & toShow
				End If
				
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
								
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = False
				selectionChangedTaskResult.Message = ""
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
				selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
				
'				'Set Visibility Param to False
'				'Used when controlling whe to load the CV component on dashboards such as Craete, Review, Manage, etc...)
'				Dim sParam As String = "param_REQPRO_AAAAAA_Visibility"
'				Dim dKeyVal As New Dictionary(Of String, String)
'				dKeyVal.Add(sParam,"IsVisible=False")			
'				selectionChangedTaskResult = Me.SetParameter(si, globals, api, dKeyVal,selectionChangedTaskResult)
				
				Return selectionChangedTaskResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Submit for Approval"	
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
'Updated: EH RMW-1564 9/3/24 Changed to annual for PGM_C20XX and Title member script to S#REQ_Shared:T#1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function SubmitForApproval(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim REQList As String = ""
			Dim sEntity As String = ""
			Dim sREQ As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Validate for Prioritization Allowance Status	
			If Not Me.IsREQPrioritizationAllowed(si, sFundCenter) Then
				Throw  New Exception("Cannot prioritize REQ at this time. Contact REQ manager." & environment.NewLine)
			End If
			'Done Validation
			
'			'Args To pass Into dataset BR
'			Dim dsArgs As New DashboardDataSetArgs 
'			dsArgs.FunctionType = DashboardDataSetFunctionType.GetDataSet
'			dsArgs.DataSetName = "REQListByEntity"
'			dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
'			dsArgs.NameValuePairs.XFSetValue("CubeView", "")
							
'			'Call dataset BR to return a datatable that has been filtered by Requirement status
'			Dim dt As DataTable = BR_REQDataset.Main(si, globals, api, dsArgs)

'			'Loops Through all Requirements by fund center to check if their status is ready for prioritization and if their ranked override value is empty 
'			For Each row As DataRow In dt.Rows
'				sEntity = row.Item("Value").Split(" ")(0)
'				sREQ = row.Item("Value").Split(" ")(1)		
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")

			'Loops and update status
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sREQ = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")	
			    Dim REQRankOverrideMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#REQ_Priority_Override_Rank:F#" & sREQ & ":O#BeforeAdj:I#" & sFundCenter & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleMemberScript).DataCellEx.DataCellAnnotation
				Dim RankOverride As DataCell = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQRankOverrideMemberScript).DataCellEx.DataCell
				Dim REQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation
					
						
				If REQStatus.XFcontainsIgnoreCase("Prioritized") Then
					'Or REQStatus.XFEqualsIgnoreCase("CMD Prioritized")
					If RankOverride.Cellstatus.IsNoData Then
						'brapi.ErrorLog.LogMessage(si, RankOverride.Cellstatus)
						REQList = sREQ & " - " & TitleValue & environment.NewLine & REQList									
					End If 																		
				End If 					
			Next	
			
			If REQList.xfcontainsignorecase("REQ") Then
				Throw New Exception("Must enter a Ranked Override Value for: " & environment.NewLine & REQList)
				Return Nothing
			End If
			
			'Loops again and update status
			Dim newREQStatus As String = ""
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sREQ = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")		
			
			    Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation
				Dim currREQLevel As String = REQStatus.Substring(0,2)
				If REQStatus.XFcontainsIgnoreCase("Prioritized") Then
						
					newREQStatus = currREQLevel & " Ready for Approval"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = REQStatusMemberScript
					objScriptValWFStatus.TextValue = newREQStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, sREQ, sEntity, sFundCenter)
						Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args, sREQ)
						Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
						Catch ex As Exception
					End Try
				Else If REQStatus.XFEqualsIgnoreCase("L2 Prioritized") Then	
					newREQStatus = "L2 Ready for Approval"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = REQStatusMemberScript
					objScriptValWFStatus.TextValue = newREQStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, sREQ, sEntity, sFundCenter)
						Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args, sREQ)
						Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
						Catch ex As Exception
					End Try
				
				End If 
					
			Next 
#Region "Old Code"			
'			Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Requirements_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()
'				'Loops Through all UFRs by fund center to check if their status is ready for prioritization and if their ranked override value is empty 									
'				For Each ExistingUFR As MemberInfo In ExistingUFRs
'			            Dim UFRRankOverrideMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Periodic:A#REQ_Priority_Override_Rank:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#" & sFundCenter & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'						Dim UFRStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'						Dim UFRTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Title:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							

'						Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRTitleMemberScript).DataCellEx.DataCellAnnotation
'						Dim RankOverride As DataCell = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRRankOverrideMemberScript).DataCellEx.DataCell
'						Dim UFRStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRStatusMemberScript).DataCellEx.DataCellAnnotation
					
						
'						If UFRStatus.XFEqualsIgnoreCase(wfLevel & " Ready For Prioritization") Then
'							If RankOverride.Cellstatus.IsNoData Then
'								UFRList = ExistingUFR.Member.Name & " - " & TitleValue & environment.NewLine & UFRList									
'							End If 																		

'						End If 

'					Next	

'					If UFRList.xfcontainsignorecase("UFR") Then
'						Throw New Exception("Must enter a Ranked Override Value for: " & environment.NewLine & UFRList)
'					End If
					
'				For Each ExistingUFR As MemberInfo In ExistingUFRs
'					Dim UFRStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					     	
'					Dim UFRStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRStatusMemberScript).DataCellEx.DataCellAnnotation
				
'					If UFRStatus.XFEqualsIgnoreCase(wfLevel & " Ready For Prioritization") Then
						
'						Dim NewStatus As String = wfLevel & " Ready For Disposition"
 
'						Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
'			    		Dim objScriptValWFStatus As New MemberScriptAndValue
'						objScriptValWFStatus.CubeName = sCube
'						objScriptValWFStatus.Script = UFRStatusMemberScript
'						objScriptValWFStatus.TextValue = NewStatus
'						objScriptValWFStatus.IsNoData = False
'						objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
'						BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
'						'update Status History
'						Try
'							Me.UpdateStatusHistory(si, globals, api, args, NewStatus, existingufr.Member.name)
'							Catch ex As Exception
'						End Try
						
'						'Send email
'						Try
'							Me.SendStatusChangeEmail(si, globals, api, args, existingufr.Member.name)
'							Catch ex As Exception
'						End Try
						
'						'Set Updated Date and Name
'						Try
'							Me.LastUpdated(si, globals, api, args, ExistingUFR.Member.Name, sEntity)
'							Catch ex As Exception
'						End Try
				
'					End If 
					
'					Next 
#End Region					
					
			Return Nothing 
			
		End Function 
		
#End Region '(Updated) 

#Region "Submit to Command"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
'Updated: EH RMW-1564 9/3/24 Changed sREQTime annual for PGM_C20XX and Title member script to S#REQ_Shared:T#1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
'Updated: KN RMW-1717 9/30/24 Updated to allow submission by filters
		Public Function SubmitToCommand(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim REQList As String = ""
			Dim sEntity As String = ""
			Dim sREQ As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
						
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")

			'Loops and update status
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sREQ = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")				
			    Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation
				'----------set new REQ workflow status---------
				Dim currREQLevel As String = REQStatus.Substring(1,1)
				Dim icurrREQLevel As Integer = currREQLevel
				Dim snewREQlevel As String = "L" & icurrREQLevel - 1
				
				If REQStatus.XFContainsIgnoreCase("Ready for Approval") Then					
					Dim newREQStatus As String = snewREQlevel & " Ready for Validation"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = REQStatusMemberScript
					objScriptValWFStatus.TextValue = newREQStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, sREQ, sEntity, sFundCenter)
						Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args, sREQ)
						Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
						Catch ex As Exception
					End Try
			
				End If 
				
			Next
'BRApi.ErrorLog.LogMessage(si,"Debug C")					
			Return Nothing
		End Function
#End Region '(New)

#Region "Approve Requirement"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for PGM_C20XX
		'Updated: KN RMW-1717 9/30/24 Updated to allow submission by filters
		Public Function ApproveRequirement(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim REQList As String = ""
			Dim sEntity As String = ""
			Dim sREQ As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
'			'Args To pass Into dataset BR
'			Dim dsArgs As New DashboardDataSetArgs 
'			dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
'			dsArgs.DataSetName = "REQListByEntity"
'			dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
'			dsargs.NameValuePairs.XFSetValue("CubeView", "")

			
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")
			
			'Loops and update status
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sREQ = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
			
			    Dim REQStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusMemberScript).DataCellEx.DataCellAnnotation

				If REQStatus.XFEqualsIgnoreCase("L2 Ready for Approval") Then
				
				Dim NewStatus As String = "L2 Approved"
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
		    	Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = REQStatusMemberScript
				objScriptValWFStatus.TextValue = NewStatus
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
					
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, NewStatus, sREQ, sEntity, sFundCenter)
						Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args, sREQ)
						Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
						Catch ex As Exception
					End Try
			
				End If 					
			Next
			
			Return Nothing
		End Function
	
#End Region '(New)

#Region "Manage REQ Statuses Updated"		
		Public Function ManageREQStatusUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "") As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim sUFR As String = args.NameValuePairs.XFGetValue("UFR", UFR)
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Args To pass Into dataset BR
			Dim dsArgs As New DashboardDataSetArgs 
				dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
				dsArgs.DataSetName = "REQListByEntity"
				dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
				dsargs.NameValuePairs.XFSetValue("CubeView", "")
							
			'Call dataset BR to return a datatable that has been filtered by ufr status
			Dim dt As DataTable = BR_REQDataset.Main(si, globals, api, dsArgs)
			
			For Each row As DataRow In dt.Rows
				Dim sEntity As String = row.Item("Value").Split(" ")(0)
				sUFR = row.Item("Value").Split(" ")(1)				
									
				Dim REQStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Status_History:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQCurrentStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
				Dim StatusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusHistoryMemberScript).DataCellEx.DataCellAnnotation
				Dim CurrentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQCurrentStatusMemberScript).DataCellEx.DataCellAnnotation
' BRApi.ErrorLog.LogMessage(si, $"REQCurrentStatusMemberScript= {REQCurrentStatusMemberScript} || CurrentStatus = {CurrentStatus}" )	
				'Dim LastHistoricalStatus As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
				Dim LastHistoricalEntry As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
				Dim LastHistoricalStatus As String = LastHistoricalEntry.Substring(LastHistoricalEntry.LastIndexOf(":") + 1)
				'If Not String.compare(sFundCenter & " " & CurrentStatus, LastHistoricalStatus) = 0 Then
				If Not String.compare(CurrentStatus, LastHistoricalStatus) = 0 Then
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, sUFR, sEntity, sFundCenter)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
	'BRApi.ErrorLog.LogMessage(si,"Here Manage UFR Statuses Updated ")					
						Me.SendStatusChangeEmail(si, globals, api, args, sUFR, sFundCenter)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, sUFR, sFundCenter)
					Catch ex As Exception
					End Try
				
				End If 
			Next
			Return Nothing				
			
		End Function 
		
#End Region 'updated

#Region "Copy All REQ Details (Merge)"
		'The copy function gets the source REQ passed into it through args and gets the destination  from session variable.
		'The new REQ name is passed into it through args.
		Public Function CopyAllREQDetails(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal SourceREQ As String = "", Optional ByVal SourceEntity As String = "", Optional ByVal TargetREQ As String = "", Optional ByVal TargetEntity As String = "") As Boolean
									
			'obtains the Fund, Name, Entity, and IT selection from the Create REQ Dashboard
			Dim sSourceREQ As String = args.NameValuePairs.XFGetValue("SourceREQ", SourceREQ)
			Dim sSourceEntity As String = args.NameValuePairs.XFGetValue("REQEntity", SourceEntity)	
			Dim sTargetEntity As String = args.NameValuePairs.XFGetValue("TargetEntity", TargetEntity)
			Dim sTargetREQ As String = args.NameValuePairs.XFGetValue("TargetREQ", TargetREQ)
'brapi.ErrorLog.LogMessage(si,"Source Entity: " & sSourceEntity & "target Entiy: " & sTargetEntity & "Source REQ: " & sSourceREQ & "Target REQ: " & sTargetREQ)			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			'Validate that REQ Title is not empty
			Dim REQCreated As Boolean = False
'	        'Create REQ	
			Dim CopyAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#Unfunded_Requirements_Accounts.Base.Remove(REQ_Attachments_Ind,UFR_Stkhldr_Intel_Name,UFR_Stkhldr_Intel_Email,UFR_Stkhldr_Intel_Concur_Ind,UFR_Stkhldr_Intel_Cmt,UFR_Stkhldr_G8_RM_Name,UFR_Stkhldr_G8_RM_Email,UFR_Stkhldr_G8_RM_Concur_Ind,UFR_Stkhldr_G8_RM_Cmt,UFR_Stkhldr_G3_Name,UFR_Stkhldr_G3_Email,UFR_Stkhldr_G3_Concur_Ind,UFR_Stkhldr_G3_Cmt,REQ_Stkhldr_1_Name,UFR_Stkhldr_1_Email,UFR_Stkhldr_1_Concur_Ind,UFR_Stkhldr_1_Cmt,UFR_Stkhldr_2_Name,UFR_Stkhldr_2_Email,UFR_Stkhldr_2_Concur_Ind,UFR_Stkhldr_2_Cmt,UFR_Stkhldr_3_Name,UFR_Stkhldr_3_Email,UFR_Stkhldr_3_Concur_Ind,UFR_Stkhldr_3_Cmt)",True)
			
			For Each REQAccount As MemberInfo In CopyAccounts
				Dim sourceMbrScript As String = "Cb#" & sCube & ":E#" & sSourceEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sSourceREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim targetMbrScript As String = "Cb#" & sCube & ":E#" & sTargetEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sTargetREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim SourceValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sourceMbrScript).DataCellEx.DataCellAnnotation

				'Update REQ Target Account
				Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
		    	Dim objScriptValAccount As New MemberScriptAndValue
				objScriptValAccount.CubeName = sCube
				objScriptValAccount.Script = targetMbrScript
				objScriptValAccount.TextValue = SourceValue
				objScriptValAccount.IsNoData = False
				objListofScriptsAccount.Add(objScriptValAccount)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)
				
			Next
			
			'Copy Attachment
			Dim sCopyAttachments As String = Me.CopyAttachments(si, sCube, sSourceEntity, sTargetEntity, sScenario, sREQTime, sSourceREQ, sTargetREQ)
			'Copy General Comments
			Me.CopyGeneralComments(si,globals,api,args, sSourceREQ, sSourceEntity, sTargetREQ, sTargetEntity)
			
			REQCreated = True
			Return REQCreated
			 
		End Function
			  
#End Region '(updated here)

#Region "Copy Attachments"
	Public Function CopyAttachments(ByVal si As SessionInfo, sCube As String, sSourceEntity As String, sTargetEntity As String, sScenario As String, sREQTime As String, sSourceREQ As String, sTargetREQ As String) As String
		Dim sAttachmentMbrScript As String = "Cb#" & sCube & ":E#" & sSourceEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Attachments_Ind" & ":F#" & sSourceREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		Dim oDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAttachmentMbrScript)
		Dim cellPK As DataCellPk = oDataCellInfoUsingMemberScript.DataCellEx.DataCell.DataCellPk
		Dim oDataAttachmentList As DataAttachmentList = BRApi.Finance.Data.GetDataAttachments(si, cellPK, True)
'BRApi.ErrorLog.LogMessage(si,"Count: " & oDataAttachmentList.Items.Count)
		If oDataAttachmentList.Items.Count > 0 Then  
			Using dt As DataTable = Me.GetSupportDocDataTableCV(si, True)
				For i As Integer = 0 To oDataAttachmentList.Items.Count - 1
					Dim dr As DataRow = dt.NewRow   								
						dr("UniqueID")  = Guid.NewGuid
						dr("Cube")		= sCube
						dr("Entity")    = sTargetEntity
						dr("Parent")    = ""
						dr("Cons")		= "USD"
						dr("Scenario")  = sScenario
						dr("Time")		= sREQTime
						dr("Account")	= "REQ_Attachments_Ind"
						dr("Flow")	    = sTargetREQ
						dr("Origin")    = "BeforeAdj"
						dr("IC")		= "None"
						dr("UD1")		= "None" 
						dr("UD2")		= "None"
						dr("UD3")		= "None"
						dr("UD4")		= "None"
						dr("UD5")		= "None"
						dr("UD6")		= "None"
						dr("UD7")		= "None"
						dr("UD8")		= "None"

						dr("Title")					= oDataAttachmentList.Items(i).Text
						dr("AttachmentType")		= "200"
						dr("CreatedUserName")		= si.UserName
						dr("CreatedTimestamp")		= DateTime.UtcNow
						dr("LastEditedUserName")    = si.UserName
						dr("LastEditedTimestamp")   = DateTime.UtcNow
						dr("Text")					= "Yes" 'Sets the cell value for annotation
						dr("FileName")				= oDataAttachmentList.Items(i).FileName
						dr("FileBytes")				= oDataAttachmentList.Items(i).FileBytes
						dt.Rows.Add(dr)
						BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)	
				Next
			End Using
			Return $"Copied {oDataAttachmentList.Items.Count} Attachments"
		Else
			Return "No Attachment to Copy"	
		End If		
				
	End Function
#End Region '(updated here)

#Region "Copy Gen Comments"
			Public Function CopyGeneralComments(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal SourceREQ As String = "", Optional ByVal SourceEntity As String = "", Optional ByVal TargetREQ As String = "", Optional ByVal TargetEntity As String = "")
	
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfLevel As String = wfProfileName.Substring(0,2).ToLower
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim UFRTime As String = WFYear & "M12"
				Dim sSourceUFR As String = args.NameValuePairs.XFGetValue("InitialREQ", SourceREQ) 
				Dim sSourceEntity As String = args.NameValuePairs.XFGetValue("reqEntity", SourceEntity)	
				Dim sTargetEntity As String = args.NameValuePairs.XFGetValue("TargetEntity", TargetEntity)
				Dim sTargetUFR As String = args.NameValuePairs.XFGetValue("TargetREQ", TargetREQ)
				'Get number of data attachments at our source intersection
				Dim sAttachmentMbrScript As String = "Cb#" & wfCube & ":E#" & sSourceEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Comments" & ":F#" & sSourceUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim oDataCellInfoUsingMemberScript As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sAttachmentMbrScript)
				Dim cellPK As DataCellPk = oDataCellInfoUsingMemberScript.DataCellEx.DataCell.DataCellPk
				Dim oDataAttachmentList As DataAttachmentList = BRApi.Finance.Data.GetDataAttachments(si, cellPK, True)

				If oDataAttachmentList.Items.Count > 0 Then 
					
					'Get Annotaion detail. Attachement Type 300
					Dim sql = "SELECT Text, CreatedUserName, CreatedTimestamp, LastEditedUserName, LastEditedTimestamp, title
								  From DataAttachment with (NOLOCK)
								  WHERE 
									Cube = '" & wfCube & "'" &
									" and Entity =  '" & sSourceEntity & "'" &
									" and Scenario =  '" & wfScenario & "'" &
									" and Flow =  '" & sSourceUFR & "'" &
									" and Time =  '" & UFRTime & "'" &
									" and Account =  'REQ_Comments'
									  and Origin =  'BeforeAdj'
									  and IC =  'None'
									  and UD1 = 'None'
									  and UD2 = 'None'
									  and UD3 = 'None'
									  and UD4 = 'None'
									  and UD5 = 'None'
									  and UD6 = 'None'
									  and UD7 = 'None'
									  and UD8 = 'None'
									  and AttachmentType = '300'
									  ORDER BY CreatedTimestamp"
					
					'execute SQL				
					Dim dtComments As DataTable = New DataTable("comments")
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						dtComments = BRApi.Database.ExecuteSql(dbConnApp, sql, False)
					End Using 
					
					'Loop through each attachment row and get values
					For Each rw As DataRow In dtComments.Rows
						Dim comments As String = rw.Item("Text")
						Dim createdUserName As String = rw.Item("CreatedUserName")
						Dim createdTimeStamp As String = rw.Item("CreatedTimestamp")
						Dim lastEditedUserName As String = rw.Item("LastEditedUserName")
						Dim lastEditedTimeStamp As String = rw.Item("LastEditedTimestamp")
						Dim title As String = rw.Item("Title")
				        Dim filebytes As DBNull = Nothing
						
						'set values obtained at new UFR and Entity
						Using dt As DataTable = Me.GetSupportDocDataTableCV(si, True)
							Dim dr As DataRow = dt.NewRow   								
								dr("UniqueID")  = Guid.NewGuid
								dr("Cube")		= wfCube
								dr("Entity")    = sTargetEntity
								dr("Parent")    = ""
								dr("Cons")		= "USD"
								dr("Scenario")  = wfScenario
								dr("Time")		= UFRTime
								dr("Account")	= "REQ_Comments"
								dr("Flow")	    = sTargetUFR
								dr("Origin")    = "BeforeAdj"
								dr("IC")		= "None"
								dr("UD1")		= "None" 
								dr("UD2")		= "None"
								dr("UD3")		= "None"
								dr("UD4")		= "None"
								dr("UD5")		= "None"
								dr("UD6")		= "None"
								dr("UD7")		= "None"
								dr("UD8")		= "None"

								dr("Title")					= title
								dr("AttachmentType")		= "300"
								dr("CreatedUserName")		= createdUserName
								dr("CreatedTimestamp")		= createdTimeStamp
								dr("LastEditedUserName")    = lastEditedUserName
								dr("LastEditedTimestamp")   = lastEditedTimeStamp
								dr("Text")					= comments
								dr("FileName")				= ""
								dr("FileBytes")				= filebytes
								dt.Rows.Add(dr)
								'write to dataAttachment table
								BRApi.Database.SaveCustomDataTable(si, "App", "dbo.DataAttachment", dt, False)	
							
						End Using
					Next
				End If 
				Return $"Copied {oDataAttachmentList.Items.Count} Attachments"

			End Function
#End Region '(updated here)

#Region "Sub Create Save UFR"
'		Public Function SubCreateSaveUFR(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
'			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
									
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			'Dim wfLevel As String = wfProfileName.Substring(0,2)
''brapi.ErrorLog.LogMessage(si,"WFLEVEL= " & wfLevel)
'			Dim PrevWFLevelNum As Integer = wfProfileName.Substring(1,1) + 1
'			Dim PrevWFLevel As String = wfProfileName.Substring(0,1) & PrevWFLevelNum
'			'obtains the Fund, Name, Entity, from the Create UFR Dashboard
'			Dim sParentEntityGeneral As String = args.NameValuePairs.XFGetValue("ufrParentEntity")
'			Dim sParentEntity As String = sParentEntityGeneral.Replace("_General","")
'			Dim ExceptionFCs As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si,False,"param_UFRPRO_AAAAAA_FiveLevelFCs__Shared")

'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'			Dim ChildEntities As New List(Of MemberInfo)
'			Dim sEntityFilter As String = ""
'			Select Case True
'				Case wfProfileName.XFContainsIgnoreCase("L2")
'					'ChildEntities = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & sCube,"E#" & sParentEntity & ".member.children.Children",True)
'					sEntityFilter = sParentEntity & ".member.children.Children"
'				Case wfProfileName.XFContainsIgnoreCase("L3") And ExceptionFCs.XFContainsIgnoreCase(sCube)
'					'ChildEntities = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & sCube,"E#" & sParentEntity & ".member.children.Children",True)
'					sEntityFilter = sParentEntity & ".member.children.Children"
'				Case wfProfileName.XFContainsIgnoreCase("L3")
'					'ChildEntities = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & sCube,"E#" & sParentEntity & ".member.children",True)
'					sEntityFilter = sParentEntity & ".member.children"
'				Case wfProfileName.XFContainsIgnoreCase("L4") And ExceptionFCs.XFContainsIgnoreCase(sCube)
'					'ChildEntities = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_" & sCube,"E#" & sParentEntity & ".member.children",True)
'					sEntityFilter = sParentEntity & ".member.children"
'			End Select
			
'			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'			Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)	
'			Dim sDimDef As String = "E#F#"
'			Dim sDataBufferPOVScript="Cb#" & sCube & ":S#" & sScenario & ":T#" & iWFYear & ":C#Local:V#Periodic:A#UFR_Requested_Amt:I#Top:O#BeforeAdj:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
'			Dim sFlow As String = "Unfunded_Requirements_Flows.Base"
'			ChildEntities = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=General_Member_Lists, MemberListName=GetFilteredDataRows, DimDef=" & sDimDef & ", DataBufferPOVScript=" & sDataBufferPOVScript  & ",Entity="  & sEntityFilter & ")", False)
				
'			For Each ChildEntity As MemberInfo In ChildEntities
				
'				Dim ExistingUFR As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimType.Flow.Id, ChildEntity.RowOrColDataCellPkAndCalcScript.DataCellPk.FlowId)
'			'Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Requirements_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()
'			'For Each ExistingUFR As MemberInfo In ExistingUFRs
				
'				Dim UFRStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & ChildEntity.Member.name & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingUFR.Member.name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim UFRCurrentStatusMemberScript As String = "Cb#" & sCube & ":E#" & ChildEntity.Member.name & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ExistingUFR.Member.name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
'				Dim StatusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRStatusHistoryMemberScript).DataCellEx.DataCellAnnotation
'				Dim CurrentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRCurrentStatusMemberScript).DataCellEx.DataCellAnnotation
				
'				Dim LastHistoricalStatus As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)

'				If Not LastHistoricalStatus.EndsWith(CurrentStatus) Then
'					If CurrentStatus.XFContainsIgnoreCase("Closing") Then
'					'update status history on closed UFR
'					Try
'						Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, ExistingUFR.Member.name, ChildEntity.Member.name, sParentEntityGeneral)
'					Catch ex As Exception
'					End Try
'					'Send email for closed UFR
'					Try
'						Me.SendStatusChangeEmail(si, globals, api, args, ExistingUFR.Member.name, ChildEntity.Member.name)
'					Catch ex As Exception
'					End Try
'					'Set Updated Date and Name
'					Try
'						Me.LastUpdated(si, globals, api, args, ExistingUFR.Member.Name, ChildEntity.Member.name)
'					Catch ex As Exception
'					End Try
'					End If
						
'					If CurrentStatus.XFEqualsIgnoreCase(wflevel & " Working") Then

						
						
'										'''''''''''''''' Old code has been commented out, should keep it through testing - CS''''''''''''''''''''''''''''''''
						
''					'Copy All Text Accounts
''					Dim sTargetUFR As String = Me.CreateEmptyUFR(si, globals, api, args,, sParentEntityGeneral)

''					Me.CopyAllUFRDetails(si, globals, api, args, ExistingUFR.Member.name, ChildEntity.Member.name, sTargetUFR, sParentEntityGeneral)
		
''					'Copy All Funding Amounts
''					Dim customSubstVars As Dictionary(Of String, String) = New Dictionary(Of String, String)

''					customSubstVars.Add("UFREntity","E#" & sParentEntityGeneral)
''					customSubstVars.Add("UFRFlow",sTargetUFR)
''					customSubstVars.Add("UFRParameterName","SourcePOVList=[E#" & ChildEntity.Member.name & ":F#" & ExistingUFR.Member.name & "],POVFilter=[A#REQ_Funding_Required_Date, A#UFR_Requested_Amt, A#UFR_Funded_Amt_L5, A#UFR_Funded_Amt_L4, A#UFR_Funded_Amt_L3, A#UFR_Funded_Amt_L2, A#UFR_Funded_Amt_L1], IsReclass=False")
''					Dim objTaskActivityItem As TaskActivityItem = BRApi.Utilities.ExecuteDataMgmtSequence(si, "Merge_UFR", customSubstVars)
					
''					'update status history on New UFR
''					Try
''						Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, sTargetUFR, sParentEntityGeneral)
''					Catch ex As Exception
''					End Try
					
''					'Send email for new UFR
''					Try
''						Me.SendStatusChangeEmail(si, globals, api, args, sTargetUFR, sParentEntityGeneral)
''					Catch ex As Exception
''					End Try
''					'Set Updated Date and Name
''					Try
''						Me.LastUpdated(si, globals, api, args, sTargetUFR, sParentEntityGeneral)
''					Catch ex As Exception
''					End Try
''					'Set Status on L4 FC UFR To L4 Draft Released to L3
''					Dim sReleasedStatus As String = PrevWFLevel & " Draft Reviewed by " & wfLevel

''					'Update UFR Requested Amount
''					Dim objListofScriptsStatus As New List(Of MemberScriptandValue)
''					Dim objScriptValStatus As New MemberScriptAndValue
''					objScriptValStatus.CubeName = sCube
''					objScriptValStatus.Script = UFRCurrentStatusMemberScript
''					objScriptValStatus.TextValue = sReleasedStatus
''					objScriptValStatus.IsNoData = False
''					objListofScriptsStatus.Add(objScriptValStatus)

''					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsStatus)

'													'''''''''''''''''''''''''''' End of old code '''''''''''''''''''''''''''''''''''
					
'					'Update status history on UFR
'					Try
'						Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, ExistingUFR.Member.name, ChildEntity.Member.name, sParentEntityGeneral)
'					Catch ex As Exception
'					End Try
'					'Send email for UFR
'					Try
'						Me.SendStatusChangeEmail(si, globals, api, args, ExistingUFR.Member.name, ChildEntity.Member.name)
'					Catch ex As Exception
'					End Try
					
'					'Set Updated Date and Name
'					Try
'						Me.LastUpdated(si, globals, api, args, ExistingUFR.Member.Name, ChildEntity.Member.name)
'					Catch ex As Exception
'					End Try
				
'				End If 
'				End If 
				
'			Next
'			Return Nothing
'		End Function

#End Region 'Commented out

#Region "Last Updated"
		
		'Updates status history account with the passed in status in a comma delimited string
		'Updated: EH RMW-1565 9/4/24 Updated to annual for PGM_C20XX and REQ_Shared
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function LastUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "", Optional ByVal Entity As String =  "")
			Try
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim UFRTime As String = WFYear
				Dim sUFRUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				Dim ufrEntity As String =  args.NameValuePairs.XFGetValue("updatedREQEntity", Entity)
				Dim ufrFlow As String = args.NameValuePairs.XFGetValue("updatedREQFlow", UFR)				

				'Get current status of UFR
				Dim UFRUpdatedDate As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#REQ_Last_Updated_Date:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim UFRUpdatedName As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & wfTime & ":V#Annotation:A#REQ_Last_Updated_Name:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
        
				'Update UFR Updated Date
				Dim objListofScriptUpdatedDate As New List(Of MemberScriptandValue)
			    Dim objScriptValUpdatedDate As New MemberScriptAndValue
				
				objScriptValUpdatedDate.CubeName = wfCube
				objScriptValUpdatedDate.Script = UFRUpdatedDate
				objScriptValUpdatedDate.TextValue = CurDate
				objScriptValUpdatedDate.IsNoData = False
				objListofScriptUpdatedDate.Add(objScriptValUpdatedDate)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptUpdatedDate)
				
				'Update UFR Updated Name
				Dim objListofScriptUpdatedName As New List(Of MemberScriptandValue)
			    Dim objScriptValUpdatedName As New MemberScriptAndValue
				
				objScriptValUpdatedName.CubeName = wfCube
				objScriptValUpdatedName.Script = UFRUpdatedName
				objScriptValUpdatedName.TextValue = sUFRUser
				objScriptValUpdatedName.IsNoData = False
				objListofScriptUpdatedName.Add(objScriptValUpdatedName)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptUpdatedName)
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region '(Updated)

#Region "Add REQ Account Value"
		
		'RMW-1224: KN: Add REQ Account Value for Dropdown Lists
		Public Function AddREQAcctValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Try
				Dim sCmd As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
				Dim sValue As String = args.NameValuePairs.XFGetValue("Value")
				Dim SQL As New Text.StringBuilder
				SQL.Append($"INSERT INTO XFC_REQ_Name_Value_List VALUES ('{sCmd}', '{sAccount}', '{sValue}', '');") 
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "Delete REQ Account Value"
		
		'RMW-1224: KN: Delete REQ Account Value for Dropdown Lists
		Public Function DeleteREQAcctValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Try
				Dim sCmd As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
				Dim sValue As String = args.NameValuePairs.XFGetValue("Value")
				Dim SQL As New Text.StringBuilder
				SQL.Append($"DELETE FROM XFC_REQ_Name_Value_List WHERE REQ_ORGANIZATION = '{sCmd}' AND REQ_MEMBERNAME = '{sAccount}' AND REQ_VALUE = '{sValue}';") 
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "Get UFR by Fund Center for demotion"
		'Function returns the UFR Title combination for UFRs that are valid to be demoted. Currently used in Manage WF > Demote popup > Combobox > prompt_cbx_UFRPRO_AAAAAA_DemoteUFR_Shared
		Public Function GetUFRFundCenterForDemote(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
									
			'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter","")
			Dim sDashboard As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Dashboard","")
						
			If String.IsNullOrWhiteSpace(sFundCenter) Then
				Throw New Exception("Please Select a Fund Center")
			End If
			
			Dim sStatus As String = ""
			'Remove _General to get the parent Entity
			sFundCenter = sFundCenter.Replace("_General","")
			
			'Get List of FCs
			'Dim FundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", $"E#{sFundCenter}.ChildrenInclusive",True).OrderBy(Function(x) x.Member.name).ToList()
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)	
			Dim sDimDef As String = "E#"
			Dim sDataBufferPOVScript="Cb#" & sCube & ":S#" & sScenario & ":T#" & iWFYear & "M12:C#Local:V#Periodic:A#UFR_Requested_Amt:I#Top:O#BeforeAdj:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
			
			Dim sFlow As String = "Unfunded_Requirements_Flows.Base"
			Dim FundCenters As List(Of memberinfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=General_Member_Lists, MemberListName=GetFilteredDataRows, DimDef=" & sDimDef & ", DataBufferPOVScript=" & sDataBufferPOVScript  & ", Entity="  & sFundCenter & ".member.base" & ", Flow=" & sFlow & ")", True)
			If FundCenters.count = 0 Then Return Nothing
			
			Dim dsArgs As New DashboardDataSetArgs 
									dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
									dsArgs.DataSetName = "UFRListByEntity"
									'dsArgs.NameValuePairs.XFSetValue("Entity", MbrInfo.Member.Name)
									dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
									dsargs.NameValuePairs.XFSetValue("CubeView", sCube)
									
			Dim ds As New DashboardDataSetArgs 
									dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
									dsArgs.DataSetName = "UFRListByEntity"
									'dsArgs.NameValuePairs.XFSetValue("Entity", MbrInfo.Member.Name)
									dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
									dsargs.NameValuePairs.XFSetValue("CubeView", sCube)
									
			'Call dataset BR to return a datatable that has been filtered by ufr status
			Dim objDT As DataTable = BR_REQDataSet.Main(si, globals, api, dsArgs)								
			
			Return objDT

		End Function
			  
#End Region

#Region "CreateManpowerREQ"

			Public Function CreateManpowerREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
				Dim sREQTitle As String = "Manpower Requirement"
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sREQUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				
				Dim REQIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_ID:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim REQWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creator_Name:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
				Dim objListofScriptsREQID As New List(Of MemberScriptandValue)
			    Dim objScriptValREQID As New MemberScriptAndValue
				objScriptValREQID.CubeName = sCube
				objScriptValREQID.Script = REQIDMemberScript
				objScriptValREQID.TextValue = sEntity.Replace("_General","_") & "00000" 
				objScriptValREQID.IsNoData = False
				objListofScriptsREQID.Add(objScriptValREQID)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsREQID)
				
				' Update REQ Title For WF Scenario
				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = REQTitleScr
				objScriptValTitle.TextValue = sREQTitle
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)

				'Update REQ Creator
				Dim objListofScriptsUser As New List(Of MemberScriptandValue)
			    Dim objScriptValUser As New MemberScriptAndValue
				objScriptValUser.CubeName = sCube
				objScriptValUser.Script = REQUserMemberScript
				objScriptValUser.TextValue = sREQUser
				objScriptValUser.IsNoData = False
				objListofScriptsUser.Add(objScriptValUser)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsUser)
				
				'Update REQ Created Date
				Dim objListofScriptsDate As New List(Of MemberScriptandValue)
			    Dim objScriptValDate As New MemberScriptAndValue
				objScriptValDate.CubeName = sCube
				objScriptValDate.Script = REQDateMemberScript
				objScriptValDate.TextValue = CurDate
				objScriptValDate.IsNoData = False
				objListofScriptsDate.Add(objScriptValDate)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDate)

				'Update REQ Workflow Status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = REQWFStatusMemberScript
				objScriptValWFStatus.TextValue = "L2 Working"
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				Try
					Me.UpdateStatusHistory(si, globals, api, args, "L2 Working","REQ_00")
				Catch ex As Exception
				End Try
				Try
					Me.LastUpdated(si, globals, api, args, "REQ_00", sEntity)
				Catch ex As Exception
				End Try
				
				Return Nothing
			End Function				
#End Region

#Region "UpdateManpowerREQStatus"

			Public Function UpdateManpowerREQStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
				Dim sREQTitle As String = "Manpower Requirement"
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sREQUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				Dim sREQStatus As String = ""
				
				Dim REQIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_ID:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim REQWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creator_Name:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim REQDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
				Dim sCurrTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQWFStatusMemberScript).DataCellEx.DataCellAnnotation
				If sCurrTitle.XFEqualsIgnoreCase("L2 Working") Then 
					sREQStatus = "L2 Approved"
				End If 
				If sCurrTitle.XFEqualsIgnoreCase("L2 Approved") Then 
					sREQStatus = "L2 Working"
				End If

				'Update REQ Workflow Status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = REQWFStatusMemberScript
				objScriptValWFStatus.TextValue = sREQStatus
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				
				Try
					Me.UpdateStatusHistory(si, globals, api, args, sREQStatus,"REQ_00")
				Catch ex As Exception
				End Try
				Try
					Me.LastUpdated(si, globals, api, args, "REQ_00", sEntity)
				Catch ex As Exception
				End Try
				
				Return Nothing
			End Function				
#End Region

#Region "Helper Methods"

#Region "Get Workflow Level"
'		Public Function GetWorkflowLevel(ByVal si As SessionInfo) As String
			
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			Dim wfLevel As String = wfProfileName.Substring(0,2)
			
'			Return wfLevel
			
'		End Function
#End Region 'commented out

#Region "Get Next Workflow Level"
'		Public Function GetNextWorkflowLevel(ByVal si As SessionInfo) As String		
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			Dim wfLevel As String = wfProfileName.Substring(0,2)
'			Dim NextWFLevelNum As Integer = wfProfileName.Substring(1,1) - 1
'			Dim NextWFLevel As String = wfProfileName.Substring(0,1) & NextWFLevelNum
						
'			Return NextWFLevel
			
'		End Function
#End Region 'commented out

#Region "Get Previous Workflow Level"
'		Public Function GetPreviousWorkflowLevel(ByVal si As SessionInfo) As String		
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			Dim wfLevel As String = wfProfileName.Substring(0,2)
'			Dim PreWFLevelNum As Integer = wfProfileName.Substring(1,1) + 1
'			Dim PreWFLevel As String = wfProfileName.Substring(0,1) & PreWFLevelNum
						
'			Return PreWFLevel
			
'		End Function
#End Region 'commented out

#Region "Is REQ Creation Allowed"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for PGM_C20XX
		Public Function IsREQCreationAllowed(ByVal si As SessionInfo, ByVal sEntity As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sREQAllowCreationMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Allow_Creation:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sREQAllowCreation As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQAllowCreationMbrScript).DataCellEx.DataCellAnnotation	
			If sREQAllowCreation.XFEqualsIgnoreCase("No") Then
				Return False
			Else
				Return True
			End If
		End Function
#End Region		

#Region "Is REQ Prioritization Allowed" 
		'Updated: EH RMW-1564 9/3/24 Updated to annual for PGM_C20XX
		Public Function IsREQPrioritizationAllowed(ByVal si As SessionInfo, ByVal sEntity As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sREQPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Allow_Prioritization:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sREQPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sREQPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If
		End Function
#End Region	'(Updated)

#Region "Is REQ Validation Allowed"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for PGM_C20XX
		Public Function IsREQValidationAllowed(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, Optional ByVal sEntity As String = "") As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter", sEntity)
			Dim sREQPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Allow_Validation:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sREQPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sREQPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If	
		
		End Function
#End Region	

#Region "Is UFR Approval Allowed"
		Public Function IsUFRApprovalAllowed(ByVal si As SessionInfo, ByVal sEntity As String, ByVal args As DashboardExtenderArgs) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter", sEntity)
'brapi.ErrorLog.LogMessage(si,"Sfundcenter inside Approval Function = " & sFundCenter)
			Dim sUFRPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sUFRPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sUFRPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If

'Brapi.ErrorLog.LogMessage(si,"Entity" &sEntity)		
			
'Brapi.ErrorLog.LogMessage(si,"FundCenter" & sFundCenter)	
			
'Brapi.ErrorLog.LogMessage(si, "SParent" &sParentEntity)	
'Brapi.ErrorLog.LogMessage(si, "SGenparent" &sGeneralParent)	
		
		End Function
#End Region	

#Region "Is Parent UFR Approval Allowed"
'		Public Function IsParentUFRApprovalAllowed(ByVal si As SessionInfo, ByVal sEntity As String) As Boolean
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			'Dim wfLevel As String = wfProfileName.Substring(0,2)
'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'			Dim iEntityID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sEntity)
'			Dim sParentEntity As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "E_ARMY"), iEntityID, False)(0).Name
'			Dim iParentID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sParentEntity)
'			Dim sGeneralParent As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "E_ARMY"), iParentID, False)(0).Name
'			Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & sCube)
'			Dim bHasChildren As Boolean = brapi.Finance.Members.HasChildren(si,DimPk,brapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id,sEntity))			
			
''Brapi.ErrorLog.LogMessage(si, "SParent" &sParentEntity)	
''Brapi.ErrorLog.LogMessage(si, "SGenparent" &sGeneralParent)	
'			Dim sUFRPermissionMbrScript As String = ""
'			If wfLevel.XFEqualsIgnoreCase("L5") Then 
'				sUFRPermissionMbrScript = "Cb#" & sCube & ":E#" & sParentEntity & "_General" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval_L4:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Else If wfLevel.XFEqualsIgnoreCase("L4") And bHasChildren Then
'				sUFRPermissionMbrScript = "Cb#" & sCube & ":E#" & sGeneralParent & "_General" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval_L3:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Else If wfLevel.XFEqualsIgnoreCase("L4") And Not bHasChildren Then
'				sUFRPermissionMbrScript = "Cb#" & sCube & ":E#" & sParentEntity & "_General" & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval_L3:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	
'			Else If wfLevel.XFEqualsIgnoreCase("L3") Then
'				sUFRPermissionMbrScript = "Cb#" & sCube & ":E#" & sGeneralParent & "_General"  & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval_L2:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Else If wfLevel.XFEqualsIgnoreCase("L2") Then
'				sUFRPermissionMbrScript = "Cb#" & sCube & ":E#" & "ARMY_General"  & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval_L1:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			End If 
'			Dim sUFRPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRPermissionMbrScript).DataCellEx.DataCellAnnotation	
'			If sUFRPermission.XFEqualsIgnoreCase("no") Then
'				Return False
'			Else
'				Return True
'			End If
'		End Function
#End Region 'commented out

#Region "Is REQ Title Blank"
		'Updated: EH 8/28/2024 - Ticket 1565 Title member script updated to REQ_Shared scenario
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function BlankTitleCheck(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sFlow As String = args.NameValuePairs.XFGetValue("REQFlow").Trim
			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity").Trim
			Dim sREQTitleMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sREQTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQTitleMbrScript).DataCellEx.DataCellAnnotation	

			If sREQTitle = "" Then

				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = sREQTitleMbrScript
				objScriptValTitle.TextValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
				
				Throw New Exception("Requirement title cannot be blank." & environment.NewLine &  " Please enter a title and click save button.")
			End If	
			Return Nothing
		
		End Function
#End Region

#Region "Get Entity"
		'Entity can be passed in directly in the Args or through session when it comes through a pop up
		'This code handles that
		Public Function GetEntity(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
			If String.IsNullOrWhiteSpace(sEntity) Then
				sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			End If
			Return sEntity
		End Function
#End Region	

#Region "Set Parameter"
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

#Region "Clear Selections"
		Public Function ClearSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String)As Object
'BRApi.ErrorLog.LogMessage(si,$"ParamsToClear = {ParamsToClear}")
			Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
			For i As Integer = 0 To objDictionary.Count -1
				'Clear only prompts that are passed in as parameters
				Dim thisKey = objDictionary.ElementAt(i).Key
				Dim thisValue = objDictionary.ElementAt(i).Value
				If(ParamsToClear.XFContainsIgnoreCase(thisKey)) Then
					objDictionary.Remove(thisKey)
					objDictionary.Add(thisKey,"")
					
				End If 
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
		End Function
#End Region	

#Region "Replace Selections"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for PGM_C20XX
		Public Function ReplaceSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String, Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing)As Object
			'set showhide boolean
			Dim ShowHideCheck As Boolean = True
			If selectionChangedTaskResult Is Nothing Then 
				'if nothing is passed in then set boolean to false and taskresult as new
				ShowHidecheck = False
				selectionChangedTaskResult = New XFSelectionChangedTaskResult()
			End If 
			Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues	
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2).ToLower
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
			Dim UFRTime As String = WFYear
			Dim ufrEntity As String = args.NameValuePairs.XFGetValue("ufrEntity")
			Dim ufrFlow As String = args.NameValuePairs.XFGetValue("ufrFlow")
			'member scripts
			Dim UFRWFMemberScript As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim UFRFundingMemberScript As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Funding_Status:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            'get wf and funding satus
			Dim currentWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRWFMemberScript).DataCellEx.DataCellAnnotation
			Dim currentFundingStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRFundingMemberScript).DataCellEx.DataCellAnnotation
			For i As Integer = 0 To objDictionary.Count -1
		
				'Clear only prompts that are passed in as parameters
				Dim thisKey = objDictionary.ElementAt(i).Key
				Dim thisValue = objDictionary.ElementAt(i).Value
				If(ParamsToClear.XFContainsIgnoreCase(thisKey)) Then
					'Set parameter To actual Funding status
					If thisKey.XFContainsIgnoreCase("FundingStatus") And currentWFStatus.XFContainsIgnoreCase("Disposition")
						objDictionary.Remove(thisKey)
						objDictionary.Add(thisKey,currentFundingStatus)
					'set parameter to actual wf status
					Else If thisKey.XFContainsIgnoreCase("WFStatus") And currentWFStatus.XFContainsIgnoreCase("Disposition")
						objDictionary.Remove(thisKey)
						objDictionary.Add(thisKey,currentWFStatus)
					'clear params
					Else 
						objDictionary.Remove(thisKey)
						objDictionary.Add(thisKey,"")
					End If 
				
				End If 
			Next

			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = False
			selectionChangedTaskResult.Message = ""
			'if nothing is passed then set action in dash to false 
			If ShowHideCheck = False Then 
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
			Else 
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
			End If
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
			selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary
		
			Return selectionChangedTaskResult
		End Function
#End Region 'need to revise

#Region "Select All"
		'Use to set Parameter with value for all selection options of a parameter - Select/Deselect All
		Public Function SelectAll(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			'Get targeted Param
			Dim sParam As String = args.NameValuePairs.XFGetValue("Param").Trim()
			If String.IsNullOrEmpty(sParam) Then Return Nothing

			'Get User's selection & val
			Dim sSelection As String = args.NameValuePairs.XFGetValue("Selection").Trim()
			If String.IsNullOrWhiteSpace(sSelection) Then sSelection = "Unselect"			
			Dim sSelectAllVal As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","UFRSelectAll","")
			
			'Select All / Unsellect All
			Dim dKeyVal As New Dictionary(Of String, String)
	
			Select Case sSelection
			Case "SelectAll"
				dKeyVal.Add(sParam,sSelectAllVal)
			Case "Unselect"
				dKeyVal.Add(sParam,"")
			End Select
			Return Me.SetParameter(si, globals, api, dKeyVal)
		End Function
#End Region

#End Region

#Region "Save All Components Helper"
		
		'Triggered by Save All Components button to do various processes that need to also occur during the save
		'Updated EH 090424 RMW-1565 Updated to annual for PGM_C20XX
		Public Function SaveAllHelper(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "", Optional ByVal Entity As String =  "")
			
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			
			' Only do if workflow step name is Manage Requirements
			If Not wfProfileName.XFContainsIgnoreCase("Manage Requirements") Then
				Return Nothing
			End If
			
			Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
			Dim sDashboard As String = objDictionary("prompt_REQPRO_REQMGT_0CaAb_cbx_ContentList")
			
			' Only do if dashboard name is Administrative
			If Not sDashboard.Equals("_B__Administrative") Then
				Return Nothing
			End If
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sEntity As String = GetEntity(si, args)
			Dim sREQAdminPercentAmtMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime &":V#Periodic:A#REQ_Priority_Cat_Weight:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#UFR_Priority_Category:U6#None:U7#None:U8#None"
			Dim TotPct As Double = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQAdminPercentAmtMemberScript).DataCellEx.DataCell.CellAmount
			
			' Only do if total pct is 100%
			If TotPct = 100 Then 
				Return Nothing
			End If
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Prioritization categories' total weight does not equal 100%."
				
			Return selectionChangedTaskResult
			
		End Function
		
#End Region

#Region "Copy CMD Approved Amount"	
		'Copy Amt from A#REQ_Requested_Amt to A#REQ_CMD_App_Req_Amt_ADM once the REQ is approved by CMD and status is set to L2 Approved
		Public Function CopyCMDApprovedAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal sEntity As String, ByVal sFlow As String)		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)		
			Dim sSrcAcct As String = "REQ_Requested_Amt"
			Dim sTgtAcct As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim iTime As Integer = Convert.ToInt32(sREQTime)
			

			' Loop through 5 yrs and grab databuffer
			' DataUnit coordinates: cubeId, entityId, parentId (use empty string if not needed), consId, scenarioId, timeId
			For i As Integer = 0 To 4 Step 1
				Dim myDataUnitPk As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

				' Buffer coordinates.
				' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sSrcAcct)
				myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, sFlow)
				myDbCellPk.OriginId = DimConstants.BeforeAdj	
				myDbCellPk.UD7Id = DimConstants.None
				myDbCellPk.UD8Id = DimConstants.None					
			
			
				' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
				Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
				If myCells.Count > 0 Then 
					Dim objListofAmountScripts As New List(Of MemberScriptandValue)
					For Each oDataCell As DataCell In myCells
						If oDataCell.CellAmount > 0 Then 
							Dim U1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, oDataCell.DataCellPk.UD1Id)
							Dim U2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, oDataCell.DataCellPk.UD2Id)
							Dim U3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, oDataCell.DataCellPk.UD3Id)
							Dim U4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, oDataCell.DataCellPk.UD4Id)
							Dim U5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, oDataCell.DataCellPk.UD5Id)
							Dim U6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, oDataCell.DataCellPk.UD6Id)
							Dim IC As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.IC, oDataCell.DataCellPk.ICId)
							
							Dim sTgtScript = $"Cb#{sCube}:E#{sEntity}:C#Local:S#{sScenario}:T#{iTime + i}:V#Periodic:A#{sTgtAcct}:F#{sFlow}:O#BeforeAdj:I#{IC}:U1#{U1}:U2#{U2}:U3#{U3}:U4#{U4}:U5#{U5}:U6#{U6}:U7#None:U8#None"
							Dim objScriptVal As New MemberScriptAndValue
							objScriptVal.CubeName = sCube
							objScriptVal.Script = sTgtScript
							objScriptVal.Amount = oDataCell.CellAmount
							objScriptVal.IsNoData = False
							objListofAmountScripts.Add(objScriptVal)
	
							BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofAmountScripts)
						End If
					Next
				End If
			Next
			Return Nothing
		End Function
	
#End Region

#Region "Property"
	Private Property sFilePath As String = ""
#End Region

#Region "ExportReport"	
		'Export PGM Requirement Data to be used as import 
		Public Function ExportReport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)		
			
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Select Case sTemplate.ToUpper
			Case "CWORK KEY5"
				Return Me.ExportReport_cWorkKey5(si,globals,api,args)
			Case "CWORK KEY15 OOC"
				Return Me.ExportReport_cWorkKey15(si,globals,api,args)
			Case "CSUSTAIN NO LINS"
				Return Me.ExportReport_cSustain(si,globals,api,args)
			Case "CDIGITAL"
				Return Me.ExportReport_cDIGITAL(si,globals,api,args)
				Case "CSUSTAIN DMOPS"
				Return Me.ExportReport_DMOPS(si,globals,api,args)
			Case "ALL REQUIREMENTS"
				Return Me.ExportReport_General(si,globals,api,args)
			End Select
			Return Nothing
	
		End Function
	
#End Region

#Region "ExportReport_cWork Key5"	
		'Export PGM Requirement Data to be used as import using cWork template 
		Public Function ExportReport_cWorkKey5(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))	
			Dim iScenariofstyr As Integer = iTime -4
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			Dim iTime0 As Integer = iScenariofstyr + 0
			Dim iTime1 As Integer = iScenariofstyr + 1
			Dim iTime2 As Integer = iScenariofstyr + 2
			Dim iTime3 As Integer = iScenariofstyr + 3
			Dim iTime4 As Integer = iScenariofstyr + 4
			Dim iTime_1 As Integer = iScenariofstyr + 5
			Dim iTime_2 As Integer = iScenariofstyr + 6
			Dim iTime_3 As Integer = iScenariofstyr + 7 
			Dim iTime_4 As Integer = iScenariofstyr + 8 
			sEntity = sEntity.Replace("""","")		

			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			processColumns.AddRange(New String(){"SCENARIO","NAME","REMARKS","JUSTIFICATION","ISSUE","MDEP","APPN","APE","ROC","DOLLAR_TYPE","BO","RC","CTYPE","UIC","REIMS","REIMC","FSC",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",$"FY{iTime_1}",$"FY{iTime_2}",$"FY{iTime_3}",$"FY{iTime_4}"})
			sFileHeader = $"SCENARIO,NAME,REMARKS,JUSTIFICATION,ISSUE,MDEP,APPN,APE,ROC,DOLLAR_TYPE,BO,RC,CTYPE,UIC,REIMS,REIMC,FSC,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},FY{iTime_1},FY{iTime_2},FY{iTime_3},FY{iTime_4}"
				
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

			'Loop through list of base entity members, and loop through 12 months per entity member to get the databuffer data cells then write to fetchDataTable
'			For Each eMbrInfo As MemberInfo In E_List
'			For Each entity As String In E_List
'				entity = entity.Trim()
					' DataUnit coordinates: cubeId, entityId, parentId (use empty string if not needed), consId, scenarioId, timeId
'					Dim lowerbound As Integer = -4
'					Dim arraylength As Integer = 9 
'					Dim length As Integer() = {arraylength}
'					Dim newlowerbound As Integer() = {lowerbound}
'					Dim newarray As System.Array = Array.CreateInstance(GetType(Double), length,newlowerbound)
					
'			For i As Integer =	newarray.GetLowerBound(0) To newarray.GetUpperBound(0)	
	For i As Integer = 0 To 8
				Dim myDataUnitPk As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iScenariofstyr + i).ToString))

				' Buffer coordinates.
				' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
				myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
				myDbCellPk.OriginId = DimConstants.BeforeAdj
				myDbCellPk.ICId = DimConstants.Top
				myDbCellPk.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "Top")		
				myDbCellPk.UD7Id = DimConstants.None
				myDbCellPk.UD8Id = DimConstants.None					
				' Get & Loop through different U1 APPN members and write
				'No Selected PEG & MDEP = Get all MDEPs
				If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
					Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
					For Each oU1 As MemberInfo In oU1List
						myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
						' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
						Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
						If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
					Next			
				
				'No Selected PEG & Select MDEP = Get selected MDEPs
				'Selected PEG & Selected MDEP = Get selected MDEPs
				Else If Not String.IsNullOrWhiteSpace(sMDEP) Then			
					Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
					For Each oU2 As MemberInfo In oU2List
						myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
						Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
						For Each oU1 As MemberInfo In oU1List
							myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
						Next						
					Next				
				
				'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
				Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then	
					Dim arrPEG As String() = sPEG.Split(",")
					For Each PEG As String In arrPEG
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
							For Each oU1 As MemberInfo In oU1List
								myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
							Next	
						Next	
					Next
				End If
			Next	
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("firstYr",iScenariofstyr.ToString)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("Scenario",sScenario)
			dArgs.Add("Cube",sCube)
			
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport_cWorkKey15/OOC"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cWorkKey15(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	
				processColumns.AddRange(New String(){"SCENARIO","ISSUECODE","BO","RQMT TITLE","RQMT DESCRIPTION","REMARKS","MDEP","APPN","APE","ROC","DOLLAR TYPE","JUON","ISR FLAG","COST MODEL","COMBAT LOSS","COST LOCATION","CATEGORY A CODE","CBS CODE","MIP PROJ CODE",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,ISSUECODE,BO,REQUIREMENT_TITLE,REQUIREMENT_DESCRIPTION,REMARKS,MDEP,APPN,APE,ROC,DOLLAR_TYPE,JUON,ISR_FLAG,COST_MODEL,COMBAT_LOSS,COST_LOCATION,CATEGORY_A_CODE,CBS_CODE,MIP_PROJ_CODE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
							
				For Each Entity As Member In lsEntity
					'For i As Integer = 0 To 4 Step 1 
					'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
					For i As Integer =  0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						Dim oU4List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType"), $"U4#Dollar_Type.Base.Where(Name DoesNotContain 'BASE')", True,,)		
								
						For Each oU4 As MemberInfo In oU4List
									myDbCellPk.UD4Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oU4.Member.Name)
						
						
						'No Selected PEG & MDEP = Get all MDEPs
						If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)

						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						Else If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String() = sPEG.Split(",")
							For Each PEG As String In arrPEG
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							Next
						
						End If	
						Next
					Next
				Next
			
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport_cSustain NO LINS"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cSustain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			If Not sPEG.XFEqualsIgnoreCase("SS")
				Throw New Exception("Please select SS PEG for cSustain No LINS Export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

			If sReportType.XFContainsIgnoreCase("Summary") Then	
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
				For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
					
					'No Selected PEG & Select MDEP = Get selected MDEPs
					'Selected PEG & Selected MDEP = Get selected MDEPs
					 If Not String.IsNullOrWhiteSpace(sMDEP) Then
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
						Next				
					
					'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
					Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
						Dim arrPEG As String = "SS"
						
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
							Next	
						
					End If			
				Next
				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			Else If sReportType.XFContainsIgnoreCase("Detail") Then
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						
						'
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							
						End If			
					Next
				Next
			End If
			

			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport_DMOPS"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_DMOPS(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
				If Not sPEG.XFEqualsIgnoreCase("SS")
				Throw New Exception("Please select SS PEG for cSustain DMOPS Export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

'			If sReportType.XFContainsIgnoreCase("Summary") Then	
'				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","PRIORITY","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
'				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,PRIORITY,MDEP,APPN,APE,ROC,DOLLAR_TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
'				For i As Integer = 0 To 4 Step 1 
'					Dim myDataUnitPk As New DataUnitPk( _
'					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
'					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
'					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
'					DimConstants.Local, _
'					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
'					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

'					' Buffer coordinates.
'					' Default to #All for everything, then set IDs where we need it.
'					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
'					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
'					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
'					myDbCellPk.OriginId = DimConstants.BeforeAdj
'					myDbCellPk.ICId = DimConstants.Top
'					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
'					myDbCellPk.UD7Id = DimConstants.None
'					myDbCellPk.UD8Id = DimConstants.None
					
'					'No Selected PEG & MDEP = Get all MDEPs
'					If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
'						Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'						If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
'					'No Selected PEG & Select MDEP = Get selected MDEPs
'					'Selected PEG & Selected MDEP = Get selected MDEPs
'					Else If Not String.IsNullOrWhiteSpace(sMDEP) Then
'						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
'						For Each oU2 As MemberInfo In oU2List
'							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
'							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
''BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
'							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
'						Next				
					
'					'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
'					Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
'						Dim arrPEG As String() = sPEG.Split(",")
'						For Each PEG As String In arrPEG
'							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
'							For Each oU2 As MemberInfo In oU2List
'								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
'								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
'							Next	
'						Next
'					End If			
'				Next
				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN","LIN",$"FY{iTime0}",$"FY1_QTY",$"FY{iTime1}",$"FY2_QTY",$"FY{iTime2}",$"FY3_QTY",$"FY{iTime3}",$"FY4_QTY",$"FY{iTime4}",$"FY5_QTY"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,LIN,FY{iTime0}(D),FY{iTime0}(Q),FY{iTime1}(D),FY{iTime1}(Q),FY{iTime2}(D),FY{iTime2}(Q),FY{iTime3}(D),FY{iTime3}(Q),FY{iTime4}(D),FY{iTime4}(Q)"
				
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							
						End If			
					Next
				Next
			
			

			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport_cDIGITAL"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cDIGITAL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			If Not sPEG.XFEqualsIgnoreCase("DD")
				Throw New Exception("Please select DD PEG for cDigital Export")
			End If	
			
			
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
#Region "CDIGITAL Summary"
			If sReportType.XFContainsIgnoreCase("Summary") Then	
				processColumns.AddRange(New String(){"SCENARIO","RQMT TYPE","RQMT SHORT TITLE","RQMT DESCRIPTION","BO","MDEP","APPN","APE","ROC","SUBCMD","DOLLAR TYPE","CTYPE","EMERGING RQMT?","APMS AITR #","PRIORITY","PORTFOLIO","CAPABILITY","JNT CAP AREA,","TBM COST POOL","TBM TOWER","ZERO TRUST CAPABILITY","ASSOCIATED DIRECTIVES","CLOUD INDICATOR","STRAT CYBERSEC PGRM","NOTES","BOR ID","BO7 ID","BO1 ID","UNIT OF MEASURE",$"FY{iTime0} # ITEMS",$"FY{iTime0} # UNIT COST",$"FY{iTime1} # ITEMS",$"FY{iTime1} # UNIT COST",$"FY{iTime2} # ITEMS",$"FY{iTime2} # UNIT COST",$"FY{iTime3} # ITEMS",$"FY{iTime3} # UNIT COST",$"FY{iTime4} # ITEMS",$"FY{iTime4} # UNIT COST"})
				sFileHeader = $"SCENARIO,RQMT TYPE,RQMT SHORT TITLE,RQMT DESCRIPTION,BO,MDEP,APPN,APE,ROC,SUBCMD,DOLLAR TYPE,CTYPE,EMERGING RQMT?,APMS AITR #,PRIORITY,PORTFOLIO,CAPABILITY,JNT CAP AREA,,TBM COST POOL,TBM TOWER,ZERO TRUST CAPABILITY,ASSOCIATED DIRECTIVES,CLOUD INDICATOR,STRAT CYBERSEC PGRM,NOTES,BOR ID,BO7 ID,BO1 ID,UNIT OF MEASURE,FY{iTime0} # ITEMS,FY{iTime0} # UNIT COST,FY{iTime1} # ITEMS,FY{iTime1} # UNIT COST,FY{iTime2} # ITEMS,FY{iTime2} # UNIT COST,FY{iTime3} # ITEMS,FY{iTime3} # UNIT COST,FY{iTime4} # ITEMS,FY{iTime4} # UNIT COST"
				'For i As Integer = 0 To 4 Step 1 
				'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
				For  i As Integer = 0 To 0 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					'myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
				
					'No selected PEG & selected MDEP = Get selected MDEPs
					'Selected PEG & selected MDEP = Get selected MDEPs
					 If Not String.IsNullOrWhiteSpace(sMDEP) Then
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
						Next				
					
					'Selected PEG & No selected MDEP = Get all MDEPs under selected PEG
					Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
						Dim arrPEG As String = "DD"
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
							Next	
						
					End If			
				Next
#End Region				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			Else If sReportType.XFContainsIgnoreCase("Detail") Then
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	
				processColumns.AddRange(New String(){"SCENARIO","RQMT TYPE","RQMT SHORT TITLE","RQMT DESCRIPTION","BO","MDEP","APPN","APE","ROC","SUBCMD","DOLLAR TYPE","CTYPE","EMERGING RQMT?","APMS AITR #","PRIORITY","PORTFOLIO","CAPABILITY","JNT CAP AREA","TBM COST POOL","TBM TOWER","ZERO TRUST CAPABILITY","ASSOCIATED DIRECTIVES","CLOUD INDICATOR","STRAT CYBERSEC PGRM","NOTES","UNIT OF MEASURE",$"FY1 # ITEMS",$"FY1 # UNIT COST",$"FY2 # ITEMS",$"FY2 # UNIT COST",$"FY3 # ITEMS",$"FY3 # UNIT COST",$"FY4 # ITEMS",$"FY4 # UNIT COST",$"FY5 # ITEMS",$"FY5 # UNIT COST"})
				sFileHeader = $"SCENARIO,RQMT TYPE,RQMT SHORT TITLE,RQMT DESCRIPTION,BO,MDEP,APPN,APE,ROC,SUBCMD,DOLLAR TYPE,CTYPE,EMERGING RQMT?,APMS AITR #,PRIORITY,PORTFOLIO,CAPABILITY,JNT CAP AREA,TBM COST POOL,TBM TOWER,ZERO TRUST CAPABILITY,ASSOCIATED DIRECTIVES,CLOUD INDICATOR,STRAT CYBERSEC PGRM,NOTES,UNIT OF MEASURE,FY{iTime0} # ITEMS,FY{iTime0} # UNIT COST,FY{iTime1} # ITEMS,FY{iTime1} # UNIT COST,FY{iTime2} # ITEMS,FY{iTime2} # UNIT COST,FY{iTime3} # ITEMS,FY{iTime3} # UNIT COST,FY{iTime4} # ITEMS,FY{iTime4} # UNIT COST"
							
				For Each Entity As Member In lsEntity
					'For i As Integer = 0 To 4 Step 1 
					'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
					For i As Integer = 0 To 0 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String ="DD"
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
						
						End If			
					Next
				Next
			End If
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport_General(All Requirements PEG export dashboard) "	
		'Export PGM Requirement Data for all fields 
		Public Function ExportReport_General(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))	
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_Requested_Amt"

			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
'Dim tStart2 As DateTime =  Date.Now()
			
			
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR TYPE","COST CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"REQ_Return_Cmt",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST"})


			
sFileHeader = $"SCENARIO,Entity,FLOW,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,Requirement_Return_Comment,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"
			
			
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						'No Selected PEG & MDEP = Get all MDEPs
						If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						Else If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String() = sPEG.Split(",")
							For Each PEG As String In arrPEG
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							Next
						End If			
					Next
				Next
			
'Dim tStop2 As DateTime =  Date.Now()
'Dim tDuration2 As TimeSpan = tStop2.Subtract(tStart2)
'BRapi.ErrorLog.LogMessage(si, "Time to read & write DataBuffer" & tDuration2.TotalSeconds.ToString("0.0000"))
				
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport Helper"

		'----------------------------------------------------------------------------------
		'     Create data tables to be used for fetching and processing fetched data
		'----------------------------------------------------------------------------------
		Private Function CreateReportDataTable(ByVal si As SessionInfo, ByVal dataTableName As String, ByVal columns As List(Of String), Optional ByVal bAllowDBNull As Boolean = False) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				For Each column As String In columns	
					Dim objCol = New DataColumn
		            objCol.ColumnName = column
					If column.XFContainsIgnoreCase("amount") Then
						objCol.DataType = GetType(Long)
						objCol.DefaultValue = 0
					Else
		           		objCol.DataType = GetType(String)
						objCol.DefaultValue = ""
					End If
		            
		            objCol.AllowDBNull = bAllowDBNull
		            dt.Columns.Add(objCol)
				Next
								
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		'----------------------------------------------------------------------------------
		'     Write data from databuffer's datacells into datatable
		'----------------------------------------------------------------------------------
		Private Sub WriteFetchTable(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String, ByVal oDataCells As List(Of Datacell))
			Try
				For Each oDataCell As DataCell In oDataCells
	            'Create a new row and append it to the table
					Dim row As DataRow = dt.NewRow()
					Dim account As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Account, oDataCell.DataCellPk.AccountId)
					Dim flow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, oDataCell.DataCellPk.FlowId)
					Dim u1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, oDataCell.DataCellPk.UD1Id)
					Dim u2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, oDataCell.DataCellPk.UD2Id)
					Dim u3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, oDataCell.DataCellPk.UD3Id)
					Dim u4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, oDataCell.DataCellPk.UD4Id)
					Dim u5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, oDataCell.DataCellPk.UD5Id)
					Dim u6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, oDataCell.DataCellPk.UD6Id)
					Dim u7 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD7, oDataCell.DataCellPk.UD7Id)
					Dim u8 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD7, oDataCell.DataCellPk.UD8Id)
					
					'Get List of Column Names
					Dim columnNames As List(Of String) = dt.Columns.Cast(Of DataColumn)().Select(Function(col) col.ColumnName).ToList()
					'Assign values to row by column names
					If columnNames.Contains("ENTITY") Then row("ENTITY") = Entity
					If columnNames.Contains("SCENARIO") Then row("SCENARIO") = Scenario
					If columnNames.Contains("TIME") Then row("TIME") = Time
					If columnNames.Contains("FLOW") Then row("FLOW") = flow
					If columnNames.Contains("ACCOUNT") Then row("ACCOUNT") = account
					If columnNames.Contains("U1") Then row("U1") = u1
					If columnNames.Contains("U2") Then row("U2") = u2
					If columnNames.Contains("U3") Then row("U3") = u3.Split("_")(1)
					If columnNames.Contains("U4") Then row("U4") = u4
					If columnNames.Contains("U5") Then row("U5") = u5
					If columnNames.Contains("U6") Then row("U6") = u6
					If columnNames.Contains("U7") Then row("U7") = u7
					If columnNames.Contains("U8") Then row("U8") = u8
					If columnNames.Contains("AMOUNT") Then row("AMOUNT") = oDataCell.CellAmount
						
'					row("ENTITY") = Entity
'					row("U1") = u1
'					row("U2") = u2
'					row("U3") = u3
'					row("U4") = u4				
'					row("TIME") = Time
'					row("AMOUNT") = oDataCell.CellAmount
				
'					Select Case True
'					Case dt.TableName.XFContainsIgnoreCase("cWork") 					
'						row("U5") = u5
'						row("SCENARIO") = Scenario
'					Case dt.TableName.XFContainsIgnoreCase("General") Or dt.TableName.XFContainsIgnoreCase("SS PEG")
'						row("FLOW") = flow
'						row("U6") = u6
'					Case dt.TableName.XFContainsIgnoreCase("DD PEG")
'						row("SCENARIO") = Scenario
'						row("FLOW") = flow
'						row("U5") = u5
						
'					End Select
'Brapi.ErrorLog.LogMessage(si, $"Name = {Name} | Value = {Value}")	

                	dt.Rows.Add(row)
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		
		'--------------------------------------------------------------------------------------------------------------------------
		'     Process data from fetch datatable into another datatable with amount combined according to keys (E#F#U1#.....)
		'--------------------------------------------------------------------------------------------------------------------------
		
		Private Sub ProcessTableForReport(ByVal si As SessionInfo, ByVal FetchDt As DataTable, processDT As DataTable, Optional ByVal dArgs As Dictionary(Of String, String) = Nothing) 
			Try				
				#Region "cWork Key5"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cWork Key5") Then
					Dim Cube As String = dArgs("Cube")
					Dim firstYr As Integer = COnvert.ToInt32(dArgs("firstYr"))
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim ROC As String = dArgs("ROC")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
				
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						'Dim Entity As String = Row("ENTITY")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")							
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$" | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | ")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String) = Tuple.Create(U1,U2,U3,U4)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(8){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - firstYr	
						Brapi.ErrorLog.LogMessage(si, "iPOS" & iPos )
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
					
					
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						'Dim Entity As String = kvp.Key.Item1
						Dim U1 As String = kvp.Key.Item1
						Dim U2 As String = kvp.Key.Item2
						Dim U3 As String = kvp.Key.Item3
						Dim U4 As String = kvp.Key.Item4
					
					
						Dim RC As String = ""

						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						'Get Issuecode
						Dim Issuecode As String = ROC.Substring(0,2) & "RS1000"
						'Write to processed DataTable
						
						Dim newRow As DataRow = processDT.Rows.Add()
						'"BUDGET CYCLE","BUDGET VERSION","MAIN ACCOUNT","SPENDING PLAN VERSION","FUNDS CENTER","FUND","FUNDED PROGRAM","COMMITMENT ITEM GROUP","COST COLLECTOR TYPE","COST COLLECTOR","FUNCTIONAL AREA","SOURCE SYSTEM","BUDGET CYCLE YEAR","AREA OF RESP","ATTRIBUTE 1","ATTRIBUTE 2","ATTRIBUTE 3","ATTRIBUTE 4","Period 1-AMT","Period 2-AMT","Period 3-AMT","Period 4-AMT","Period 5-AMT","Period 6-AMT","Period 7-AMT","Period 8-AMT","Period 9-AMT","Period 10-AMT","Period 11-AMT","Period 12-AMT","Period 1-QTY","Period 2-QTY","Period 3-QTY","Period 4-QTY","Period 5-QTY","Period 6-QTY","Period 7-QTY","Period 8-QTY","Period 9-QTY","Period 10-QTY","Period 11-QTY","Period 12-QTY"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("ROC")= ROC
						newRow("DOLLAR_TYPE")= U4
						newRow("BO")= "R"
						newRow("RC")= U1
						newRow("Name")= "RMW_" & Issuecode
						newRow("Justification")= "RMW snchronization"
						newRow("Issue")= Issuecode
						newRow("Remarks")= "Command input for BO R data call"
						'Write 8-Up amounts
'					Dim lowerbound As Integer = -4
'					Dim arraylength As Integer = 9 
'					Dim length As Integer() = {arraylength}
'					Dim newlowerbound As Integer() = {lowerbound}
				
'				Dim newarray As System.Array = Array.CreateInstance(GetType(Double), length,newlowerbound)
'						For i As Integer =	newarray.GetLowerBound(0) To newarray.GetUpperBound(0)

				For i As Integer = 0 To 8
					'Get cPROBE Data
						Dim cProbePos As String =  "Cb#ARMY:E#GlobalVar:C#Local:S#" & Scenario & ":T#" & startYr & ":V#Annotation:A#Var_Selected_Position:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim cProbeScenario As String = 	 BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, cProbePos).DataCellEx.DataCellAnnotation
						Dim U3Concat As String = U1 & "_" & U3
					Dim sSrcMbrScript As String = "Cb#ARMY:S#" & cProbeScenario & ":T#" & firstYr + i & ":C#Local:E#" & ROC & ":V#YTD:A#BOR:F#Baseline:I#Top:O#BeforeAdj:U1#" & U1 & ":U2#" & U2 & ":U3#" & U3Concat & ":U4#" & U4 & ":U5#Top:U6#None:U7#Top:U8#Top"
					Dim cProbeAMT As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, sSrcMbrScript).DataCellEx.DataCell.CellAmount
						'Brapi.ErrorLog.LogMessage(si, "cPROBE script" & sSrcMbrScript)
					'Brapi.ErrorLog.LogMessage(si, "REQAMT" & Amount(i))
					Dim cWorkDelta As Decimal =Math.Round(((Amount(i) - cProbeAMT)/1000),0)
					'Dim cWorkDeltaRound As Decimal  =  Math.Round((cWorkDelta/1000),0)
							newRow($"FY{firstYr + i}")= cWorkDelta
						Next
						
					Next			
				End If
				#End Region
				
				#Region "cWork Key15"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cWork Key15") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","ISSUECODE","BO","RQMT TITLE","RQMT DESCRIPTION","REMARKS","MDEP","APPN","APE","ROC","DOLLAR TYPE","JUON","ISR FLAG","COST MODEL","COMBAT LOSS","COST LOCATION","CATEGORY A CODE","CBS CODE","MIP PROJ CODE"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get Text accounts From DataAttachment Using SQL - Do it For the entire cube

						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_DESCRIPTION],
											[REQ_JUON],
											[REQ_ISR_Flag],
											[REQ_Cost_Model],
											[REQ_Combat_Loss],
											[REQ_Cost_Location],
											[REQ_Category_A_Code],
											[REQ_CBS_Code],
											[REQ_MIP_Proj_Code])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'BRApi.ErrorLog.LogMessage(si, $"dt num rows: {dt.Rows.Count}")
						End Using
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
					    	
						'Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | Flow = {Flow} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(4){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						'Get Issuecode
						Dim Issuecode As String = ROC.Substring(0,2) & "RS1000"
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4 
						newRow("BO")= "R"
						newRow("ROC")= ROC
						newRow("ISSUECODE")= Issuecode
						newRow("Remarks")= "Command input for OOC key15 data call"
						
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
						Next

							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								
								newRow("RQMT TITLE")= """"&resultRow("REQ_TITLE") & """"
								newRow("RQMT DESCRIPTION")= """"&resultRow("REQ_Description") & """"
								newRow("JUON")= """"&resultRow("REQ_Juon") & """"
								newRow("ISR FLAG")= """"&resultRow("REQ_ISR_Flag") & """"
								newRow("COST MODEL")= """"&resultRow("REQ_Cost_Model") & """"
								newRow("COMBAT LOSS")= """"&resultRow("REQ_Combat_Loss") & """"
								newRow("COST LOCATION")= """"&resultRow("REQ_Cost_Location") & """"
								newRow("CATEGORY A CODE")= """"&resultRow("REQ_Category_A_Code") & """"
								newRow("CBS CODE")= """"&resultRow("REQ_CBS_Code") & """"
								newRow("MIP PROJ CODE")= """"&resultRow("REQ_MIP_Proj_Code") & """"
								
								
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "cSustain"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cSustain No LINS") Then
					
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","BO","REQ_RQMT_STATUS","REQ_TITLE","REQ_COMMENTS","REQ_RECURRING_JUSTIFICATION","REQ_SS_PRIORITY","REQ_COMMITMENT_GROUP","REQ_CAPABILITY_SS","REQ_STRATEGIC_BIN"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					If ReportType.XFContainsIgnoreCase("Detail") Then
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_SS_PRIORITY],[REQ_COMMITMENT_GROUP],[REQ_CAPABILITY_SS],[REQ_STRATEGIC_BIN])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
					End If	
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("ROC")= ROC	
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
						Next

							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
														
								newRow("NAME")= """"&resultRow("REQ_TITLE") & """"
								newRow("REMARKS")= """"&resultRow("REQ_COMMENTS") & """"
								newRow("JUSTIFICATION")= """"&resultRow("REQ_RECURRING_JUSTIFICATION") & """"
								newRow("FUNCTIONAL PRIORITY")= """"&resultRow("REQ_SS_PRIORITY") & """"
								newRow("CAPABILITY")= """"&resultRow("REQ_CAPABILITY_SS") & """"
								newRow("STRATEGIC BIN")= """"&resultRow("REQ_STRATEGIC_BIN") & """"
								newRow("COMMITMENT GROUP")= """"&resultRow("REQ_COMMITMENT_GROUP") & """"
							End If
						
					Next			
				End If
				#End Region
				
				#Region "cDIGITAL"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cDigital") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"REQ_RQMT_STATUS","REQ_Type","REQ_TITLE","REQ_DESCRIPTION","REQ_New_Rqmt_Ind","REQ_APMS_AITR_Num","REQ_DD_Priority","REQ_Portfolio","REQ_Capability_DD","REQ_JNT_CAP_AREA","REQ_TBM_COST_POOL","REQ_TBM_TOWER","REQ_ZERO_TRUST_CAPABILITY","REQ_Assoc_Directorate","REQ_CLOUD_INDICATOR","REQ_STRAT_CYBERSEC_PGRM","REQ_Notes","REQ_UNIT_OF_MEASURE","REQ_FY1_ITEMS","REQ_FY1_UNIT_COST","REQ_FY2_ITEMS","REQ_FY2_UNIT_COST","REQ_FY3_ITEMS","REQ_FY3_UNIT_COST","REQ_FY4_ITEMS","REQ_FY4_UNIT_COST","REQ_FY5_ITEMS","REQ_FY5_UNIT_COST"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					If ReportType.XFContainsIgnoreCase("Detail") Then
						
						'Get Text accounts From DataAttachment Using SQL - Do it For the entire cube

						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_Type],[REQ_TITLE],[REQ_DESCRIPTION],
											[REQ_New_Rqmt_Ind],
											[REQ_APMS_AITR_Num],
											[REQ_DD_Priority],
											[REQ_Portfolio],
											[REQ_Capability_DD],
											[REQ_JNT_CAP_AREA],
											[REQ_TBM_COST_POOL],
											[REQ_TBM_TOWER],
											[REQ_ZERO_TRUST_CAPABILITY],
											[REQ_Assoc_Directorate],
											[REQ_CLOUD_INDICATOR],
											[REQ_STRAT_CYBERSEC_PGRM],
											[REQ_Notes],
											[REQ_UNIT_OF_MEASURE],
											[REQ_FY1_ITEMS],
											[REQ_FY1_UNIT_COST],
											[REQ_FY2_ITEMS],
											[REQ_FY2_UNIT_COST],
											[REQ_FY3_ITEMS],
											[REQ_FY3_UNIT_COST],
											[REQ_FY4_ITEMS],
											[REQ_FY4_UNIT_COST],
											[REQ_FY5_ITEMS],
											[REQ_FY5_UNIT_COST])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'BRApi.ErrorLog.LogMessage(si, $"dt num rows: {dt.Rows.Count}")
						End Using
					End If	
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
					    Dim U5 As String = Row("U5")	
						'Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | Flow = {Flow} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U5)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U5 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("CTYPE")= U5
						newRow("ROC")= ROC
						NewRow("SUBCMD")= Entity
						'Write 5-Up amounts
						'For i As Integer = 0 To 4 Step 1
'						For i As Integer = 0 To 0 
'							newRow($"FY{startYr + i}")= Amount(i)
'						Next

							
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								newRow("RQMT TYPE")= """"&resultRow("REQ_TYPE") & """"
								newRow("RQMT SHORT TITLE")= """"&resultRow("REQ_TITLE") & """"
								newRow("RQMT DESCRIPTION")= """"&resultRow("REQ_Description") & """"
								newRow("EMERGING RQMT?")= """"&resultRow("REQ_NEW_RQMT_IND") & """"
								newRow("APMS AITR #")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
								newRow("Priority")=  """"&resultRow("REQ_DD_Priority") & """"
								newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
								newRow("Capability")=  """"&resultRow("REQ_Capability_DD") & """"
								newRow("JNT CAP AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
								newRow("TBM COST POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
								newRow("TBM TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
								newRow("ZERO TRUST CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
								newRow("Associated Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
								newRow("CLOUD INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
								newRow("STRAT CYBERSEC PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
								newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
								newRow("UNIT OF MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
								newRow("FY1 # ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
								newRow("FY1 # UNIT COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
								newRow("FY2 # ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
								newRow("FY2 # UNIT COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
								newRow("FY3 # ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
								newRow("FY3 # UNIT COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
								newRow("FY4 # ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
								newRow("FY4 # UNIT COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
								newRow("FY5 # ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
								newRow("FY5 # UNIT COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "General(All Reqs)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("All Requirements") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					
					Dim Scenario As String = dArgs("Scenario")
					
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & " REQ"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){
					"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Recurring_Justification",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_Return_Cmt",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($" (Select ENTITY, FLOW, TEXT,
								Case
									When ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									When ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									When ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									When ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									When ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									When ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									When ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									When ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								Else
									ACCOUNT
								End As AccountCAT
					From DataAttachment Where  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
					[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_Return_Cmt],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]
					)) AS PIVOTTABLE ") 
						 
						
						
	'	BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
				
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U5 As String = Row("U5")
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
					Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COST CATEGORY","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						
						newRow("COST CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next

						
							'Get "REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT"
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								'NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN
								'[REQ_Title],[REQ_ID],[REQ_DESCRIPTION],[REQ_APPROVAL_COMMENT],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_COMMITMENT_GROUP]
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("REQ_Return_Cmt")=  """"&resultRow("REQ_Return_Cmt") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"

							End If 
						
					Next			
				End If
				#End Region	
				
				#Region "DMOPS"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cSustain DMOPS") Then
					
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","BO","REQ_RQMT_STATUS","REQ_TITLE","REQ_COMMENTS","REQ_RECURRING_JUSTIFICATION","REQ_SS_PRIORITY","REQ_COMMITMENT_GROUP","REQ_CAPABILITY_SS","REQ_STRATEGIC_BIN"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_SS_PRIORITY],[REQ_COMMITMENT_GROUP],[REQ_CAPABILITY_SS],[REQ_STRATEGIC_BIN],[REQ_LIN],[REQ_FY1_QTY],[REQ_FY2_QTY],[REQ_FY3_QTY],[REQ_FY4_QTY],[REQ_FY5_QTY])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("ROC")= ROC	
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
							
						Next

							
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
														
								newRow("NAME")= """"&resultRow("REQ_TITLE") & """"
								newRow("REMARKS")= """"&resultRow("REQ_COMMENTS") & """"
								newRow("JUSTIFICATION")= """"&resultRow("REQ_RECURRING_JUSTIFICATION") & """"
								newRow("FUNCTIONAL PRIORITY")= """"&resultRow("REQ_SS_PRIORITY") & """"
								newRow("CAPABILITY")= """"&resultRow("REQ_CAPABILITY_SS") & """"
								newRow("STRATEGIC BIN")= """"&resultRow("REQ_STRATEGIC_BIN") & """"
								newRow("COMMITMENT GROUP")= """"&resultRow("REQ_COMMITMENT_GROUP") & """"
								newRow("LIN")= """"&resultRow("REQ_LIN") & """"
								
								If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY1_QTY").ToString)
									
								newRow("FY1_QTY") = "0"
							Else 
								newRow("FY1_QTY") = """"&resultRow("REQ_FY1_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY2_QTY").ToString)
									
								newRow("FY2_QTY") = "0"
							Else 
								newRow("FY2_QTY") = """"&resultRow("REQ_FY2_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY3_QTY").ToString)
									
								newRow("FY3_QTY") = "0"
							Else 
								newRow("FY3_QTY") = """"&resultRow("REQ_FY3_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY4_QTY").ToString)
									
								newRow("FY4_QTY") = "0"
							Else 
								newRow("FY4_QTY") = """"&resultRow("REQ_FY4_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY5_QTY").ToString)
									
								newRow("FY5_QTY") = "0"
							Else 
								newRow("FY5_QTY") = """"&resultRow("REQ_FY5_QTY") & """"
							End If
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "Export All REQs(Review dashboard)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("ExportAllREQs") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & " REQ"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)({
					"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Recurring_Justification",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_Return_Cmt",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY", 
																"Command"})
					dt = Me.CreateReportDataTable(si,"ExportAllREQs",detailColumns,True)	
				
						
						
					Dim SQL As New Text.StringBuilder
					SQL.Append($"SELECT * FROM ") 
					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
								CASE
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								ELSE
									ACCOUNT
								End as AccountCAT
					FROM DataAttachment WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
					[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_Return_Cmt],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]
					)) AS PIVOTTABLE ") 
						
						
					
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
					'Dim dtFetch As New DataTable
						
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
					End Using
'BRApi.ErrorLog.LogMessage(si, "SQL is done")
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U6 As String = Row("U6")
						Dim U5 As String = Row("U5")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
					
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
					
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
					
						newRow("SCENARIO")= exportScenario
						newRow("Command")= Cube
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR_TYPE")= U4	
						newRow("COST_CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next
						
						
							
						'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
						Dim resultRow As DataRow = dt.AsEnumerable() _
							.singleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
													AndAlso row.Field(Of String)("FLOW") = Flow)
													
						'Assign values
						'Assign values
						If resultRow IsNot Nothing Then
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("REQ_Return_Cmt")=  """"&resultRow("REQ_Return_Cmt") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"



						End If
					
					Next			
				End If
				#End Region	
				
				#Region "Export All Updated REQs(Import Dashboard)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("ExportAllUpdatedREQs") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = Convert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					Dim StatusAccount As String = "REQ_Rqmt_Status"
					
					Dim exportScenario As String = "PGM_C" & startYr.ToString.Substring(startYr.ToString.Length - 4) 
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Recurring_Justification",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY", 
																"Command"})
					dt = Me.CreateReportDataTable(si,"ExportAllUpdatedREQs",detailColumns,True)	
				
						
						
					Dim SQL As New Text.StringBuilder
					SQL.Append($"SELECT * FROM ") 
					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
								CASE
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								ELSE
									ACCOUNT
								End as AccountCAT
					FROM DataAttachment WHERE CUBE = '{Cube}' AND SCENARIO = '{Scenario}' ) as src ")
					'WHERE (ACCOUNT = '{StatusAccount}' and Text Not like '{ApprovedStatus}')
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]

					)) AS PIVOTTABLE ") 
						
						
					
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
					'Dim dtFetch As New DataTable
						
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
					End Using
'BRApi.ErrorLog.LogMessage(si, "SQL is done")
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U6 As String = Row("U6")
						Dim U5 As String = Row("U5")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
					
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
					
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
					
						newRow("SCENARIO")= exportScenario
						newRow("Command")= Cube
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR_TYPE")= U4	
						newRow("COST_CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next
						
							
						'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
						Dim resultRow As DataRow = dt.AsEnumerable() _
							.singleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
													AndAlso row.Field(Of String)("FLOW") = Flow)
													
						'Assign values
						If resultRow IsNot Nothing Then
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"



						End If
					
					Next			
				End If
				#End Region				
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	

		'----------------------------------------------------------------------------------
		'     Create data tables to be used for fetching and processing fetched data
		'----------------------------------------------------------------------------------
		Private Function GenerateReportFile(ByVal si As SessionInfo, ByVal processDT As DataTable, ByVal sFileHeader As String, ByVal sCube As String, ByVal iTime As Integer, ByVal sTemplate As String, ByVal sFvParam As String)
			Try
				'Initialize file 
				Dim file As New Text.StringBuilder
				file.Append(sFileHeader)	

				'Populate file
				For Each row As DataRow In processDT.Rows
					Dim rowInfo As String = ""
					For Each column As DataColumn In processDT.Columns
						rowInfo = rowInfo & "," & row(Column)				
					Next
					rowInfo = rowInfo.Remove(0,1)
					rowInfo = rowInfo.Replace("None","")
					file.Append(vbCrLf + rowInfo)
				Next
				Dim sUser As String = si.UserName
				Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","")
				If datetime.Now.Month < 10 Then sTimeStamp = "0" & sTimeStamp			
			    Dim fileName As String = $"{sCube}_{iTime}_{sTemplate}_{sUser}_{sTimeStamp}.csv"
			
				Me.BuildFile(si, file.ToString, fileName, sCube)
				Dim dKeyVal As New Dictionary(Of String,String) From {{sFvParam,sFilePath}}
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"
				
				Return selectionChangedTaskResult
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "BuildFile"
		
		'----------------------------------------------------------------------------------
		'    Build export file
		'----------------------------------------------------------------------------------
		Private Sub BuildFile(ByVal si As SessionInfo, ByVal sFileContent As String, ByVal sFileName As String, ByVal sCommandName As String)
			Try
				'Pass text to bytes
				Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(sFileContent)
				
				'Define folder to hold file
				Dim sFolderPath As String = "Documents/Users/" & si.UserName
				Dim objXFFolderEx As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sFolderPath)

				'Check if folder doesn't exist
				'This should never happen because we created the folder manually
				'If objXFFolderEx Is Nothing Then
				'	Throw New XFUserMsgException(si, New Exception("Users/" & si.UserName.Replace(" ",String.Empty) & " folder does NOT exist"))
				'End If
				
				Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sFolderPath, "/", sFileName))
				Dim objXFFile As New XFFile(objXFFileInfo,String.Empty,fileBytes)

				'Load file
				BRApi.FileSystem.InsertOrUpdateFile(si, objXFFile)
				sFilePath = $"{sFolderPath}/{sFileName}"
				
				'Delete file
				'BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, String.Concat(sFolderPath, "/", sfileName))
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		#End Region
		
#End Region

#Region "ExportReport - Set Default PEG"
			'Set Default PEG for Requirements Export
			Public Function	SetDefaultPEG(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
				Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template")
				Dim sParam As String = args.NameValuePairs.XFGetValue("Param")
				Dim sPEG As String = ""
				If sTemplate.XFContainsIgnoreCase("cSustain") Or sTemplate.XFContainsIgnoreCase("DMOPS")
					sPEG = "SS"
				Else If sTemplate.XFContainsIgnoreCase("cDigital")
					sPEG = "DD"
				End If
				Dim dKeyVal As New Dictionary(Of String, String)				
				dKeyVal.Add(sParam,sPEG)		
				Return Me.SetParameter(si, globals, api, dKeyVal)				
		
			End Function
#End Region

#Region "Calculate Full FYDP"	
' Created 10/30/24 by JM for JIRA-114. Used to calculate FYDP 2-5 using infaltion rate and req amt during creation

		Public Function CalculateFullFYDP(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal REQFlow As String) 
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sEntity As String = GetEntity(si, args)			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			Dim sAPPNFund As String = args.NameValuePairs.XFGetValue("APPNFund")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")		
			Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
			Dim sDollarType As String = args.NameValuePairs.XFGetValue("DollarType")
			Dim sCommitmentItem As String = args.NameValuePairs.XFGetValue("CommitmentItem")
			Dim IC As String = sEntity

			Dim REQRequestedAmt As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#REQ_Requested_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommitmentItem & ":U7#None:U8#None"
			Dim iCurrScenarioYear As Integer = Strings.Right(sScenario,4).XFConvertToInt
			Dim iAPPNID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id, sAPPNFund)
			Dim sAPPN As String = ""
			
			If sAPPNFund.XFEqualsIgnoreCase("none") Then
				sAPPN = "None"
			Else	
				sAPPN = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iAPPNID, False)(0).Name  & "_General"
			End If
			
			Dim sInfRateScript As String = "Cb#" & sCube & ":S#" & sScenario & ":T#" & iCurrScenarioYear.ToString & ":C#USD:E#"& sCube & "_General:V#Periodic:A#PGM_Inflation_Rate_Amt:O#BeforeAdj:I#None:F#None" 
			sInfRateScript &= ":U1#" & sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
			Dim dInflationRate As Decimal = 0
				dInflationRate = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sInfRateScript).DataCellEx.DataCell.CellAmount
				
			Dim REQAMT As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQRequestedAmt).DataCellEx.DataCell.CellAmount
			
			Dim dAmt2 As Decimal = Math.Round( REQAMT * (1 + (dInflationRate/100)))
			Dim dAmt3 As Decimal = Math.Round( dAmt2 * (1 + (dInflationRate/100)))
			Dim dAmt4 As Decimal =  Math.Round(dAmt3 * (1 + (dInflationRate/100)))
			Dim dAmt5 As Decimal =  Math.Round(dAmt4 * (1 + (dInflationRate/100)))
			
			Dim REQRequestedAmttMemberScriptFYDP As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":V#Periodic:A#REQ_Requested_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommitmentItem & ":U7#None:U8#None"
			
			Dim FY2TimeScript As String = REQRequestedAmttMemberScriptFYDP & ":T#" & (iCurrScenarioYear + 1).ToString
			Dim FY3TimeScript As String = REQRequestedAmttMemberScriptFYDP & ":T#" & (iCurrScenarioYear + 2).ToString
			Dim FY4TimeScript As String = REQRequestedAmttMemberScriptFYDP & ":T#" & (iCurrScenarioYear + 3).ToString
			Dim FY5TimeScript As String = REQRequestedAmttMemberScriptFYDP & ":T#" & (iCurrScenarioYear + 4).ToString
			
			Dim objListofAmountScripts As New List(Of MemberScriptandValue)
			
			
			Dim objScriptValFY2 As New MemberScriptAndValue
			objScriptValFY2.CubeName = sCube
			objScriptValFY2.Script = FY2TimeScript
			objScriptValFY2.Amount = dAmt2
			objScriptValFY2.IsNoData = False
			objListofAmountScripts.Add(objScriptValFY2)
			
			Dim objScriptValFY3 As New MemberScriptAndValue
			objScriptValFY3.CubeName = sCube
			objScriptValFY3.Script = FY3TimeScript
			objScriptValFY3.Amount = dAmt3
			objScriptValFY3.IsNoData = False
			objListofAmountScripts.Add(objScriptValFY3)
			
			Dim objScriptValFY4 As New MemberScriptAndValue
			objScriptValFY4.CubeName = sCube
			objScriptValFY4.Script = FY4TimeScript
			objScriptValFY4.Amount = dAmt4
			objScriptValFY4.IsNoData = False
			objListofAmountScripts.Add(objScriptValFY4)
			
			Dim objScriptValFY5 As New MemberScriptAndValue
			objScriptValFY5.CubeName = sCube
			objScriptValFY5.Script = FY5TimeScript
			objScriptValFY5.Amount = dAmt5
			objScriptValFY5.IsNoData = False
			objListofAmountScripts.Add(objScriptValFY5)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofAmountScripts)
			
			Return Nothing
			
		End Function
#End Region

#Region "REQ Mass Import File Helper"	
'----------Created to Output log files for REQ Imports----------
		Public Function REQMassImportFileHelper(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal sErrorMsg As String, ByVal sTitle As String, Optional ByVal fName As String = "") 
		
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","").Replace(" ","_")
			Dim file As New Text.StringBuilder
			
			file.Append(sErrorMsg)
			
			Dim sfileName As String = sTitle & "_" & sUser & "_" & sTimeStamp & "_" & fName
			'Pass text to bytes
			Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(file.ToString)
							
			'Define folder to hold file
			Dim sUserFolderPath As String = "Documents/Users/" & sUser
			Dim sAdminFolderPath As String = "Documents/Public/Admin/Requirements/Batch_Upload_Error_Log"
			
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
		
#Region "IsFilterSelected"
		Private Sub IsFilterSelected(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs)
			Try
				Dim filterList As String = args.NameValuePairs.XFGetValue("FilterList")
				If String.IsNullOrWhiteSpace(filterList) Then
					Throw New XFUserMsgException(si, New Exception("Please select a filter."))
				End If
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
#End Region

#Region "Cache Dashboard"
		Private Sub DbCache(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs)
			Try
				'cache in db name to be used for linked dashboard
				Dim sDb As String = args.NameValuePairs.XFGetValue("Db","")
				If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"LR_REQ_Prompts","Db",sDb)	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
#End Region

#Region "ExportAllREQs(Review Daashboard)"	
		'Export PGM Requirement Data
		Public Function ExportAllREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	 
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")	
			If sEntity.XFContainsIgnoreCase("_General") Then
				sEntity = sEntity.Replace("_General","")
			Else 
				sEntity = sEntity
			End If
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sEntity)
			Dim SAccount As String = "REQ_Requested_Amt"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
				columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","REQUIREMENT STATUS","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	
			Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
			Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR_TYPE","COST_CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"REQ_Return_Cmt",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST",
 "Command"})


			
sFileHeader = $"SCENARIO,Entity,FLOW,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,Requirement_Return_Comment,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST,Command"
			
			
			For Each Entity As Member In lsEntity
				For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
					Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
					If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
				Next
			Next
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region

#Region "LoadPages"
		Private Function LoadPages(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As Object
			Try
				'========== debug vars ============================================================================================================ 
				Dim sDebugRuleName As String = "REQ_SolutionHelper"
				Dim sDebugFuncName As String = "LoadPages()"	

Dim tStart As DateTime =  Date.Now()							

				'----- get req list -----
				Dim scbxEntity As String = args.NameValuePairs.XFGetValue("cbxEntity")
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Fundcenter")
				Dim sTitleSearch As String = args.NameValuePairs.XFGetValue("TitleSearch")
				Dim sAppn As String = args.NameValuePairs.XFGetValue("APPN")
				Dim sFundCode As String = args.NameValuePairs.XFGetValue("FundCode")
				Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")
				Dim sSAG As String = args.NameValuePairs.XFGetValue("SAG")
				Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
				Dim sDollarType As String = args.NameValuePairs.XFGetValue("DollarType")
				
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   Go get list")
				Dim ListOfREQs As list(Of MemberInfo) =  BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=REQ_Member_Lists, MemberListName=GetREQListByStatus, Caller=REQ_SolutionHelper, mode=CVResult, ReturnDim=E#F#U1#U2#U3#U4#, FlowFilter=[Command_Requirements.Base.Where(Name doesnotcontain REQ_00)], cbxEntity = " & scbxEntity & ", Page= , EntityFilter=[" & sFundCenter & "], TitleSearch=[" & sTitleSearch & "], U1ParentFilter=[" & sAPPN & "], U1Filter=[" & sFundCode & "], U2Filter=[" & sMDEP & "], U3ParentFilter=[" & sSAG & "], U3Filter=[ " & sAPE & "], U4Filter=[" & sDollarType & "] )"  , False)
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then brapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   ListOfREQs count=" & ListOfREQs.Count & "    cbxEntity=" & scbxEntity & "    sFundCenter=" & sFundCenter & "    sTitleSearch=" & sTitleSearch & "    sAppn=" & sAppn & "    sFundCode=" & sFundCode & "    sMDEP=" & sMDEP & "    sSAG=" & sSAG & "    sAPE=" & sAPE & "    sDollarType=" & sDollarType )

				'---- load cbx page ----
				Dim sRowsPerPage As String = args.NameValuePairs.XFGetValue("RowsPerPage")
				Dim iRowsPerPage As Integer = 25
				If sRowsPerPage <> "" Then iRowsPerPage = CInt(sRowsPerPage)
					
				Dim iPages As Integer = 0	
				If 	ListOfREQs.Count > iRowsPerPage Then iPages = Math.Truncate(ListOfREQs.Count / iRowsPerPage)
				If 	(ListOfREQs.Count Mod iRowsPerPage) > 0 Then iPages += 1
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   iRowsPerPage=" & iRowsPerPage & "    ListOfREQs.Count=" & ListOfREQs.Count & "   iPages=" & iPages)									
					
				Dim lsPageList As New List(Of String)
				For iPage As Integer = 1 To iPages
					lsPageList.Add(iPage.ToString)
				Next

				Dim objDictionary As Dictionary(Of String, String) = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues

				objDictionary.item("var_cbx_Paging_List") = String.Join(",", lsPageList)
				objDictionary.item("prompt_cbx_Paging") = "1"
				
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
'If si.UserName.XFEqualsIgnoreCase("akalwa") Then BRapi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ":   " & Date.Now().ToString("hh:mm:ss:fff") & " REQ_UpdateFilterLists took: " & Date.Now().Subtract(tStart).TotalSeconds.ToString("0.0000"))									
				Return selectionChangedTaskResult							

				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region


#Region "ExportAllUpdatedREQs(Import Dashboard)"	
		'Export PGM Requirement Data
		Public Function ExportAllUpdatedREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
			Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			
			
			Dim SAccount As String = "REQ_Requested_Amt"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","REQUIREMENT STATUS","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	
			
			processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR_TYPE","COST_CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST",
 "Command"})


			
sFileHeader = $"SCENARIO:Do Not Update,Entity:Do Not Update,FLOW:Do Not Update,REQUIREMENT STATUS:Do Not Update,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST,Command:Do Not Update"
			
'Get variable Ids and flow list to remove Manpower reqs
			Dim cubeid As Integer = BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId
			Dim scenarioid As Integer =BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario)
			Dim flowlist As List(Of MemberInfo) =  BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "F_REQ_Main"), $"F#Command_Requirements.Base.Where(Name DoesNotContain 'REQ_00')", True,,)
		
			
			For Each Entity As Member In lsEntity
				Dim entityid As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name)
				
					For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					 cubeid, _
					entityid, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					scenarioid, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
		'Get full data set for all flows then filter down to flow Ids we want			
					Dim allcellsubset As List(Of DataCell) = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
	
					 If allcellsubset.Count  > 0 Then
						 'Get flow Ids we need
						 Dim DesiredFlowIds As New HashSet(Of Integer)(flowlist.Select(Function(f) f.Member.MemberId))
						' Filter our subset cells vs the flows we want 
						 Dim Filteredcells As List(Of DataCell) = allcellsubset.Where(Function(cell) DesiredFlowIds.Contains(cell.DataCellPk.FlowId)).ToList()
					If Filteredcells.Count > 0 Then
						'write filter data to table 
						Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,Filteredcells)
					End If 
				End If 

					
				Next
			Next
		
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTableUpdate",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sCube)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region


#Region "REQ Mass Import/Update "
		'This rule reads the imported file chcks if it is readable then parses into the REQ class
		'It then validate if the title is blank and the validity of Fund Code, MDEP, APE and Dollar Type
		'If there are errors, it will spit out the first 10 errors. That is because there could be thousands of records
		'If it is all valid, it will create a REQ_Id and Flow
		'It clears the  XFC_REQ_Import table for the Command scenario combination and saves it in the table. 
		'After that it loads the cube with the FYDP and narrative accounts
		'The mapping is shown below.
#Region "file structure"
		'-----Mapping definitions----
'			Entity
'			APPN (Fundcode)
'			MDEP
'			SAG_APE
'			DollarType
'			Cycle
'			FY1
'			FY2
'			FY3
'			FY4
'			FY5
'			Title
'			Description
'			RequestedFundSource
'			DirectivePolicy
'			AssessedRisk
'			RiskLevel
'			SeniorLeaderPriority
'			PEGStragegyPriority
'			StrategicInitiative
'			RequirementType
'			Readiness
'			AreaOfOperations
'			Office
'			NotificationEmailList
'			FlexField1
'			FlexField2
'			FlexField3
'			FlexField4
'			FlexField5
'			ITCyberReq
'			CFLValidated
'			Contingency
'			EmergingRequirement
'			CPACandidate
'			Directorate
'			AssociateDirectorate
'			Division
'			Branch
'			OwnerName
'			OwnerEmail
'			OwnerPhone
'			OwnerComment
'			MDEPFuncName
'			MDEPFuncEmail
'			MDEPFuncPhone
'			MDEPFuncComment
'			TypeOfContract
'			ContractNumber
'			TaskOrder
'			AwardTargetDate
'			POPExpirationDate
'			WorkYearsReq
'			ContractValue
'			COR
'			CME
'			COREmail
'			CORPhone
'			OperationalImpact
'			Justification
'			CostMethodologyCmt
'			UnfundedMitigation
'           UIC
'		    FunctionalPriority 
'		    CommitmentGroup 
'		    Capability 
'		    StrategicBIN 
'		    LIN 
				
		'-----------------------------
		'***If the import file structure changes this rule has to be updated.
		'***Assumption is that thereare no commas and | in the file. The file is comma delimeted
		'
#End Region

#Region "Import REQ Function Process For Update"

		Public Function	ImportREQForUpdate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							

			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)

#Region "Validate File structure For Update"			
			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
			Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
'BRApi.ErrorLog.LogMessage(si, "FolderPath: " & filePath & ", Name: " & objXFFileInfo.Name)			
			If objXFFileInfo Is Nothing
				Me.REQMassImportFileHelper(si, globals, api, args, "File " & objXFFileInfo.Name & " does NOT exist", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(String.Concat("File " & objXFFileInfo.Name & " does NOT exist")))
			End If

			'Confirm source file is readable
			Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)			
			If objXFFileEx Is Nothing
				Me.REQMassImportFileHelper(si, globals, api, args, "Unable to open file " & objXFFileInfo.Name, "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(String.Concat( "Unable to open file " & objXFFileInfo.Name)))
			End If

			'Read the content of the file
			'TO LOOK INTO: this approach picks up the whole data once into memory. If the file is too large check here for performance issue.
			'May be a file streamer to read a line at a time may be better
			Dim sFileText As String = system.Text.Encoding.ASCII.GetString(objXFFileEX.XFFile.ContentFileBytes)                                                                                   
			
			'Clean up CRLF from the file
			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfCube).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
			'Dim patt As String = vbCrLf & "(?!" & comd & ")|[&']" '-- exclude what is in the brackt
			'Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")

			Dim patt As String = "(" & vbCrLf & "|" & vbLf & ")(?!" & wfScenario & ")"'(?=[a-zA-Z0-9,""@!#$%'()+=-_.:<>?~^]*)" '-- Include what is in the brackt
			Dim cleanedText As String = Regex.Replace(sFileText,patt,"  ")
			
			'Dim patt2 As String = "[^a-zA-Z0-9,""@!#$%'()+=-_.:<>?~/\\\[" & vbcrlf & vbLf & "]"
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
				
'				Next

			'***********Split will need to be replace with alternate solution to handle CRLF issue*******
			'Dim lines As String() = sFileText.Split(vbCRLF) 
			Dim lines As String() = cleanedText.Split(vbCRLF) 
			
			If lines.Count < 1 Then 
				Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is empty", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is empty"))
			End If
			
			'For performance we are capping the upload file to not more than 5000
			If lines.Count > 5001 Then 
				Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is too large. Please upload a file not more than 5000 rows"))
			End If

			Dim firstLine As Boolean = True
			'Dim currentUsedFlows As List(Of String) = New List(Of String)
			Dim REQs As List (Of REQ) = New List (Of REQ)
			Dim isFileValid As Boolean = True
			Dim iLineCount As Integer = 0
			
			'Loop through each line and process
			For Each line As String In lines
				iLineCount += 1

				'Skip empty line
				If String.IsNullOrWhiteSpace(line) Then
					Continue For
				End If

				'If there are back to back (escaped) double quotes, they will be replaced with single quotes.
				'This is done becasue "s are used as column separator in csv files and "s inside would be represented as escaped quotes ("")
				line = line.Replace("""""", "'")

'BRApi.ErrorLog.LogMessage(si, "Line : " & line)
				'Use reg expressions to split the csv.
				'The expression accounts for commas that are with in "" to treat them as data.
				Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
				Dim fields As String () = Regex.Split(line, pattern)
				
				'Check number of column and skip first line
				If firstLine Then
					'There needs to be 68 columns
					If fields.Length <> 95 Then
						Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 95, number columns in file header is "& fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 95, number columns in file header is "& fields.Length & vbCrLf & line ))
					End If
					
					firstLine = False
					Continue For
				End If
#End Region
				
#Region "Parse file For Update"
				'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
				Dim currREQ As REQ = Me.ParseREQForUpdate(si, fields)
				'If EntityDescriptorType is a parent, add _General
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.Entity & ".base", True)
				If membList.Count > 1 Then
					currREQ.Entity = currREQ.Entity & "_General"
				End If
				
			Dim currentUsedFlows As List(Of String) = Me.GetUsedFlows(si, wfCube, wfScenario)
			
			Dim sEntityREQ As String = currREQ.Entity
			Dim sFlowREQ As String = currREQ.flow
				Dim Key As String = sEntityREQ & ":"  & sFlowREQ
					
					If Not currentUsedFlows.Contains(Key)
						
				Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & "Invalid Flow member. Flow Member currently does not exist", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " Invalid Flow member. Flow Member currently does not exist"))
					End If
			
								
		'====== Get APPN_FUND And PARENT APPN_L2 ======
					Dim U1Name As String = currREQ.FundCode				
					Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
					Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 	
					Dim U3Concat As String = U1ParentName & "_" & currREQ.APE9
					currREQ.APE9 = U3Concat
				
				
				
				
				
				
				
#End Region

#Region "Validate members For Update"

				'validate req. If there is one error, then the file will be considered invalid and would not be loaded intot he cube
				'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				sScenario = currREQ.Scenario
				If Not sScenario.XFEqualsIgnoreCase(wfScenario) Then 
					Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is not targeting the current workflow year", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is not targeting the current workflow year"))
				End If
			
			
				Dim currREQVlidationRes As Boolean = False
				currREQVlidationRes = Me.ValidateMembersForUpdate(si,currREQ)
				If isFileValid Then 
					isFileValid = currREQVlidationRes
'BRApi.ErrorLog.LogMessage(si, "Error line : " & currREQ.StringOutput())					
				End If
								
				'Add REQ tot the REQ list
				REQs.Add(currREQ)
				
'BRApi.ErrorLog.LogMessage(si, "Added REQ =  " & currREQ.title & ", valid= " & currREQ.valid & ", msg= " & currREQ.validationError)				
			Next
#End Region
		
#Region "Populate table XFC_REQ_Import, Cube and Annotation For Update"
			
			'Prior to starting to load data, clear pre-existing rows
			Dim SQLClearREQ As String = "Delete from XFC_REQ_Import_Update  
											Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "'"
			'Dim SQLClearREQ As String = "Truncate table [dbo].[XFC_REQ_Import]"
			
'BRApi.ErrorLog.LogMessage(si,"SQLClearREQ = " & SQLClearREQ)

			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearREQ.ToString, False, True)
			End Using
		
'			'Find the list of existing flows
'			Dim ExistingREQs As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).Select(Function(n) n.Member.Name).ToList()
'			ExistingREQs.OrderBy(Function(x) convert.ToInt32(x.split("_")(1))).ToList()
'			Dim currentUsedFlows As List(Of String) = Me.GetUsedFlows(si, wfCube, wfScenario)
			
			'loop through all parsed requirements list
			For Each currREQ As REQ In REQs
				Me.PopulateStageTableForUpdate(si, globals, api, currREQ, isFileValid)
				
				If isFileValid Then
					Me.PopulateCubeUpdate(si, globals, api, args, currREQ)
					Me.PopulateAnnotationUpdate(si, currREQ)
				End If
			Next
#End Region

			'If the validation failed, write the error out.
			'If there are more than ten, we show only the first ten messages for the sake of redablity
			Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
			If Not isFileValid Then
				Dim sErrorLog As String = ""
				For Each req In REQs
					sErrorLog = sErrorLog & vbCrLf & req.StringOutput()
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

				Me.REQMassImportFileHelper(si, globals, api, args, sCompletionMessageFail, "FAIL", objXFFileInfo.Name)
				
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

			Me.REQMassImportFileHelper(si, globals, api, args, sCompletionMessagePass, "PASS", objXFFileInfo.Name)
			'Throw New Exception("IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper)
			Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)



		Return Nothing
		End Function			
#End Region 

#End Region

#Region "Import Helpers"

#Region "Parse REQ For Update "
		'Parse a line into REQ object and return
		Public Function	ParseREQForUpdate(ByVal si As SessionInfo, ByVal fields As String())As Object
			'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
			Dim currREQ As REQ = New REQ
			'Trim any unprintable character and surrounding quotes
'			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
'				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
'			Else 
'				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
'			End If 
			currREQ.Scenario = fields(0).Trim().Trim("""")
			currREQ.Entity = fields(1).Trim().Trim("""")
			currREQ.Flow = fields(2).Trim().Trim("""")
			currREQ.Status = fields(3).Trim().Trim("""")
			currREQ.FundCode = fields(4).Trim().Trim("""") 
			currREQ.MDEP = fields(5).Trim().Trim("""")
			currREQ.APE9 = fields(6).Trim().Trim("""")
			currREQ.DollarType = fields(7).Trim().Trim("""")
			currREQ.CommitmentItem = fields(8).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.CommitmentItem)
				currREQ.CommitmentItem = "None"
			End If 
			currREQ.sCtype = fields(9).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.sCType)
				currREQ.sCType = "None"
			End If 
			currREQ.FY1 = fields(10).Trim().Trim("""")
			currREQ.FY2 = fields(11).Trim().Trim("""")
			currREQ.FY3 = fields(12).Trim().Trim("""")
			currREQ.FY4 = fields(13).Trim().Trim("""")
			currREQ.FY5 = fields(14).Trim().Trim("""")
			currREQ.Title = fields(15).Trim().Trim("""")
			currREQ.Description  = fields(16).Trim().Trim("""")
			currREQ.Justification  = fields(17).Trim().Trim("""")
			currREQ.CostMethodology = fields(18).Trim().Trim("""")
			currREQ.ImpactifnotFunded = fields(19).Trim().Trim("""")
			currREQ.RiskifnotFunded = fields(20).Trim().Trim("""")
			currREQ.CostGrowthJustification = fields(21).Trim().Trim("""")
			currREQ.MustFund = fields(22).Trim().Trim("""")
			currREQ.FundingSource  = fields(23).Trim().Trim("""")
			currREQ.ArmyInitiative_Directive = fields(24).Trim().Trim("""")
			currREQ.CommandInitiative_Directive = fields(25).Trim().Trim("""")
			currREQ.Activity_Exercise = fields(26).Trim().Trim("""")
			currREQ.IT_CyberRequirement  = fields(27).Trim().Trim("""")
			currREQ.UIC = fields(28).Trim().Trim("""")
			currREQ.FlexField1  = fields(29).Trim().Trim("""")
			currREQ.FlexField2  = fields(30).Trim().Trim("""")
			currREQ.FlexField3  = fields(31).Trim().Trim("""")
			currREQ.FlexField4  = fields(32).Trim().Trim("""")
			currREQ.FlexField5  = fields(33).Trim().Trim("""")
			currREQ.EmergingRequirement = fields(34).Trim().Trim("""")
			currREQ.CPATopic = fields(35).Trim().Trim("""")
			currREQ.PBRSubmission = fields(36).Trim().Trim("""")
			currREQ.UPLSubmission = fields(37).Trim().Trim("""")
			currREQ.ContractNumber  = fields(38).Trim().Trim("""")
			currREQ.TaskOrderNumber  = fields(39).Trim().Trim("""")
			currREQ.AwardTargetDate  = fields(40).Trim().Trim("""")
			currREQ.POPExpirationDate  = fields(41).Trim().Trim("""")
			currREQ.ContractorManYearEquiv_CME  = fields(42).Trim().Trim("""")
			currREQ.COREmail  = fields(43).Trim().Trim("""")
			currREQ.POCEmail = fields(44).Trim().Trim("""")
			currREQ.Directorate  = fields(45).Trim().Trim("""")
			currREQ.Division  = fields(46).Trim().Trim("""")
			currREQ.Branch = fields(47).Trim().Trim("""")
			currREQ.ReviewingPOCEmail = fields(48).Trim().Trim("""")
			currREQ.MDEPFunctionalEmail = fields(49).Trim().Trim("""")
			currREQ.NotificationListEmails = fields(50).Trim().Trim("""")
			currREQ.GeneralComments_Notes = fields(51).Trim().Trim("""")
			currREQ.JUON = fields(52).Trim().Trim("""")
			currREQ.ISR_Flag = fields(53).Trim().Trim("""")
			currREQ.Cost_Model = fields(54).Trim().Trim("""")
			currREQ.Combat_Loss = fields(55).Trim().Trim("""")
			currREQ.Cost_Location = fields(56).Trim().Trim("""")
			currREQ.Category_A_Code = fields(57).Trim().Trim("""")
			currREQ.CBS_Code = fields(58).Trim().Trim("""")
			currREQ.MIP_Proj_Code = fields(59).Trim().Trim("""")
			currREQ.SS_Priority = fields(60).Trim().Trim("""")
			currREQ.Commitment_Group = fields(61).Trim().Trim("""")
			currREQ.SS_Capability = fields(62).Trim().Trim("""")
			currREQ.Strategic_BIN = fields(63).Trim().Trim("""")
			currREQ.LIN = fields(64).Trim().Trim("""")
			currREQ.FY1_QTY = fields(65).Trim().Trim("""")
			currREQ.FY2_QTY = fields(66).Trim().Trim("""")
			currREQ.FY3_QTY = fields(67).Trim().Trim("""")
			currREQ.FY4_QTY = fields(68).Trim().Trim("""")
			currREQ.FY5_QTY = fields(69).Trim().Trim("""")
			currREQ.RequirementType = fields(70).Trim().Trim("""")
			currREQ.DD_Priority = fields(71).Trim().Trim("""")
			currREQ.Portfolio = fields(72).Trim().Trim("""")
			currREQ.DD_Capability = fields(73).Trim().Trim("""")
			currREQ.JNT_CAP_AREA = fields(74).Trim().Trim("""")
			currREQ.TBM_COST_POOL = fields(75).Trim().Trim("""")
			currREQ.TBM_TOWER = fields(76).Trim().Trim("""")
			currREQ.APMSAITRNum = fields(77).Trim().Trim("""")
			currREQ.ZERO_TRUST_CAPABILITY = fields(78).Trim().Trim("""")
			currREQ.ASSOCIATED_DIRECTIVES = fields(79).Trim().Trim("""")
			currREQ.CLOUD_INDICATOR = fields(80).Trim().Trim("""")
			currREQ.STRAT_CYBERSEC_PGRM = fields(81).Trim().Trim("""")
			currREQ.NOTES = fields(82).Trim().Trim("""")
			currREQ.UNIT_OF_MEASURE = fields(83).Trim().Trim("""")
			currREQ.FY1_ITEMS = fields(84).Trim().Trim("""")
			currREQ.FY1_UNIT_COST = fields(85).Trim().Trim("""")
			currREQ.FY2_ITEMS = fields(86).Trim().Trim("""")
			currREQ.FY2_UNIT_COST = fields(87).Trim().Trim("""")
			currREQ.FY3_ITEMS = fields(88).Trim().Trim("""")
			currREQ.FY3_UNIT_COST = fields(89).Trim().Trim("""")
			currREQ.FY4_ITEMS = fields(90).Trim().Trim("""")
			currREQ.FY4_UNIT_COST = fields(91).Trim().Trim("""")
			currREQ.FY5_ITEMS = fields(92).Trim().Trim("""")
			currREQ.FY5_UNIT_COST = fields(93).Trim().Trim("""")
			
			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
			Else 
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
			End If 
			
			Return currREQ
			
		End Function
#End Region

#Region "Validate Members For Update"		
		Public Function	ValidateMembersForUpdate(ByVal si As SessionInfo, ByRef currREQ As REQ) As Object
			
			'validate fund code
			'This code leverages the way we validate in Data Source
			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
			
			Dim isFileValid As Boolean = True
			
			If String.IsNullOrWhiteSpace(currREQ.title) Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Blank Title value in record"
			End If
			
			'Add flow validation 6/13/25  - Per original requirement manpower should not be editable 
			If currREQ.flow = "REQ_00"
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Flow member. Manpower Requirements cannot be uploaded"
					
			End If
			'Validate that the Fund Center being loaded  s with in the command
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
			Dim currEntity As String = currREQ.Entity
			If Not membList.Exists(Function(x) x.Member.Name = currEntity) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.fundCode & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.fundCode
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & currREQ.MDEP & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#" & currREQ.APE9.Trim & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.APE9
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid APE value: " & SAG_APE))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U4#" &currREQ. DollarType & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Dollar Type value: " & currREQ.DollarType
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			End If
			If Not String.IsNullOrWhiteSpace(currREQ.sCType) Or currREQ.sCType.XFEqualsIgnoreCase("None") Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U5_CTYPE")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U5#" &currREQ. sCType & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid CType value: " & currREQ.sCType
					
				End If
		
			End If 
			If Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem) Or currREQ.CommitmentItem.XFEqualsIgnoreCase("None")Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
					
				End If
			End If 
			
			'Validate Numeric
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1)) And (Not IsNumeric(currREQ.FY1))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1 should be Numeric: " & currREQ.FY1
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2)) And (Not IsNumeric(currREQ.FY2))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2 should be Numeric: " & currREQ.FY2
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3)) And ( Not IsNumeric(currREQ.FY3))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3 should be Numeric: " & currREQ.FY3
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4)) And ( Not IsNumeric(currREQ.FY4))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4 should be Numeric: " & currREQ.FY4
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5)) And ( Not IsNumeric(currREQ.FY5))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5 should be Numeric: " & currREQ.FY5
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1_QTY)) And ( Not IsNumeric(currREQ.FY1_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1_QTY should be Numeric: " & currREQ.FY1_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2_QTY)) And ( Not IsNumeric(currREQ.FY2_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2_QTY should be Numeric: " & currREQ.FY2_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3_QTY)) And ( Not IsNumeric(currREQ.FY3_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3_QTY should be Numeric: " & currREQ.FY3_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4_QTY)) And ( Not IsNumeric(currREQ.FY4_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4_QTY should be Numeric: " & currREQ.FY4_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5_QTY)) And ( Not IsNumeric(currREQ.FY5_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5_QTY should be Numeric: " & currREQ.FY5_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.ContractorManYearEquiv_CME)) And ( Not IsNumeric(currREQ.ContractorManYearEquiv_CME))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: ContractorManYearEquiv_CME should be Numeric: " & currREQ.ContractorManYearEquiv_CME
				
			End If
			


			
'			'We determine the scenario from the cycle
'			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sScenario = currREQ.scenario
'			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
'			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
'			If (membList.Count <> 1) Then 
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.scenario
'				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
'			Else
'				currREQ.scenario = sScenario
'			End If
			
			Return isFileValid
			
		End Function
#End Region

#Region "Populate Stage Table For Update"		
		Public Function	PopulateStageTableForUpdate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByRef currREQ As REQ, ByVal isFileVlaid As Boolean) As Object

'			'get fcalcualted fields if file is valid. Else no need to create Flow as the records won't be loaded into the cube
'			If(isFileVlaid) Then
'				currREQ.flow = 
'				currREQ.REQ_ID = 
							
'				'Add current flow to the list of used flows. This is used during the GetFlow process

'			End If

		
			'insert into table
			Dim SQLInsert As String = "
			INSERT Into [dbo].[XFC_REQ_Import_Update]
						([Scenario]
						,[Entity]
						,[Flow]
						,[Status]
						,[APPN]
						,[MDEP]
						,[APE9]
						,[DollarType]
						,[ObjectClass]
						,[CType]
						,[FY1]
						,[FY2]
						,[FY3]
						,[FY4]
						,[FY5]
						,[Title]
						,[Description]
						,[Justification]
						,[CostMethodology]
						,[ImpactifnotFunded]
						,[RiskifnotFunded]
						,[CostGrowthJustification]
						,[MustFund]
						,[FundingSource]
						,[ArmyInitiative_Directive]
						,[CommandInitiative_Directive]
						,[Activity_Exercise]
						,[IT_CyberRequirement]
						,[UIC]
						,[FlexField1]
						,[FlexField2]
						,[FlexField3]
						,[FlexField4]
						,[FlexField5]
						,[EmergingRequirement]
						,[CPATopic]
						,[PBRSubmission]
						,[UPLSubmission]
						,[ContractNumber]
						,[TaskOrderNumber]
						,[AwardTargetDate]
						,[POPExpirationDate]
						,[ContractorManYearEquiv_CME]
						,[COREmail]
						,[POCEmail]
						,[Directorate]
						,[Division]
						,[Branch]
						,[ReviewingPOCEmail]
						,[MDEPFunctionalEmail]
						,[NotificationListEmails]
						,[GeneralComments_Notes]
						,[JUON]
						,[ISR_Flag]
						,[Cost_Model]
						,[Combat_Loss]
						,[Cost_Location]
						,[Category_A_Code]
						,[CBS_Code]
						,[MIP_Proj_Code]
						,[SS_Priority]
						,[Commitment_Group]
						,[SS_Capability]
						,[Strategic_BIN]
						,[LIN]
						,[FY1_QTY]
						,[FY2_QTY]
						,[FY3_QTY]
						,[FY4_QTY]
						,[FY5_QTY]
						,[RequirementType]
						,[DD_Priority]
						,[Portfolio]
						,[DD_Capability]
						,[JNT_CAP_AREA]
						,[TBM_COST_POOL]
						,[TBM_TOWER]
						,[APMS_AITR_NUM]
						,[ZERO_TRUST_CAPABILITY]
						,[ASSOCIATED_DIRECTIVES]
						,[CLOUD_INDICATOR]
						,[STRAT_CYBERSEC_PGRM]
						,[NOTES]
						,[UNIT_OF_MEASURE]
						,[FY1_ITEMS]
						,[FY1_UNIT_COST]
						,[FY2_ITEMS]
						,[FY2_UNIT_COST]
						,[FY3_ITEMS]
						,[FY3_UNIT_COST]
						,[FY4_ITEMS]
						,[FY4_UNIT_COST]
						,[FY5_ITEMS]
						,[FY5_UNIT_COST]
						,[Command]
						,[ValidationError])
		    VALUES
   				('" & currREQ.scenario & "','" &
					currREQ.Entity.Replace("'", "''") & "','" &
					currREQ.Flow.Replace("'", "''") & "','" &
					currREQ.Status.Replace("'", "''") & "','" &
					currREQ.FundCode.Replace("'", "''") & "','" &
					currREQ.MDEP.Replace("'", "''") & "','" &
					currREQ.APE9.Replace("'", "''") & "','" &
					currREQ.DollarType.Replace("'", "''") & "','" &
					currREQ.CommitmentItem.Replace("'", "''") & "','" &
					currREQ.sCType.Replace("'", "''") & "','" &
					currREQ.FY1.Replace("'", "''") & "','" &
					currREQ.FY2.Replace("'", "''") & "','" &
					currREQ.FY3.Replace("'", "''") & "','" &
					currREQ.FY4.Replace("'", "''") & "','" &
					currREQ.FY5.Replace("'", "''") & "','" &
					currREQ.Title.Replace("'", "''") & "','" &
					currREQ.Description.Replace("'", "''") & "','" &
					currREQ.Justification.Replace("'", "''") & "','" &
					currREQ.CostMethodology.Replace("'", "''") & "','" &
					currREQ.ImpactifnotFunded.Replace("'", "''") & "','" &
					currREQ.RiskifnotFunded.Replace("'", "''") & "','" &
					currREQ.CostGrowthJustification.Replace("'", "''") & "','" &
					currREQ.MustFund.Replace("'", "''") & "','" &
					currREQ.FundingSource.Replace("'", "''") & "','" &
					currREQ.ArmyInitiative_Directive.Replace("'", "''") & "','" &
					currREQ.CommandInitiative_Directive.Replace("'", "''") & "','" &
					currREQ.Activity_Exercise.Replace("'", "''") & "','" &
					currREQ.IT_CyberRequirement.Replace("'", "''") & "','" &
					currREQ.UIC.Replace("'", "''") & "','" &
					currREQ.FlexField1.Replace("'", "''") & "','" &
					currREQ.FlexField2.Replace("'", "''") & "','" &
					currREQ.FlexField3.Replace("'", "''") & "','" &
					currREQ.FlexField4.Replace("'", "''") & "','" &
					currREQ.FlexField5.Replace("'", "''") & "','" &
					currREQ.EmergingRequirement.Replace("'", "''") & "','" &
					currREQ.CPATopic.Replace("'", "''") & "','" &
					currREQ.PBRSubmission.Replace("'", "''") & "','" &
					currREQ.UPLSubmission.Replace("'", "''") & "','" &
					currREQ.ContractNumber.Replace("'", "''") & "','" &
					currREQ.TaskOrderNumber.Replace("'", "''") & "','" &
					currREQ.AwardTargetDate.Replace("'", "''") & "','" &
					currREQ.POPExpirationDate.Replace("'", "''") & "','" &
					currREQ.ContractorManYearEquiv_CME.Replace("'", "''") & "','" &
					currREQ.COREmail.Replace("'", "''") & "','" &
					currREQ.POCEmail.Replace("'", "''") & "','" &
					currREQ.Directorate.Replace("'", "''") & "','" &
					currREQ.Division.Replace("'", "''") & "','" &
					currREQ.Branch.Replace("'", "''") & "','" &
					currREQ.ReviewingPOCEmail.Replace("'", "''") & "','" &
					currREQ.MDEPFunctionalEmail.Replace("'", "''") & "','" &
					currREQ.NotificationListEmails.Replace("'", "''") & "','" &
					currREQ.GeneralComments_Notes.Replace("'", "''") & "','" &
					currREQ.JUON.Replace("'", "''") & "','" &
					currREQ.ISR_Flag.Replace("'", "''") & "','" &
					currREQ.Cost_Model.Replace("'", "''") & "','" &
					currREQ.Combat_Loss.Replace("'", "''") & "','" &
					currREQ.Cost_Location.Replace("'", "''") & "','" &
					currREQ.Category_A_Code.Replace("'", "''") & "','" &
					currREQ.CBS_Code.Replace("'", "''") & "','" &
					currREQ.MIP_Proj_Code.Replace("'", "''") & "','" &
					currREQ.SS_Priority.Replace("'", "''") & "','" &
					currREQ.Commitment_Group.Replace("'", "''") & "','" &
					currREQ.SS_Capability.Replace("'", "''") & "','" &
					currREQ.Strategic_BIN.Replace("'", "''") & "','" &
					currREQ.LIN.Replace("'", "''") & "','" &
					currREQ.FY1_QTY.Replace("'", "''") & "','" &
					currREQ.FY2_QTY.Replace("'", "''") & "','" &
					currREQ.FY3_QTY.Replace("'", "''") & "','" &
					currREQ.FY4_QTY.Replace("'", "''") & "','" &
					currREQ.FY5_QTY.Replace("'", "''") & "','" &
					currREQ.RequirementType.Replace("'", "''") & "','" &
					currREQ.DD_Priority.Replace("'", "''") & "','" &
					currREQ.Portfolio.Replace("'", "''") & "','" &
					currREQ.DD_Capability.Replace("'", "''") & "','" &
					currREQ.JNT_CAP_AREA.Replace("'", "''") & "','" &
					currREQ.TBM_COST_POOL.Replace("'", "''") & "','" &
					currREQ.TBM_TOWER.Replace("'", "''") & "','" &
					currREQ.APMSAITRNum.Replace("'", "''") & "','" &
					currREQ.ZERO_TRUST_CAPABILITY.Replace("'", "''") & "','" &
					currREQ.ASSOCIATED_DIRECTIVES.Replace("'", "''") & "','" &
					currREQ.CLOUD_INDICATOR.Replace("'", "''") & "','" &
					currREQ.STRAT_CYBERSEC_PGRM.Replace("'", "''") & "','" &
					currREQ.NOTES.Replace("'", "''") & "','" &
					currREQ.UNIT_OF_MEASURE.Replace("'", "''") & "','" &
					currREQ.FY1_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY1_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY2_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY2_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY3_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY3_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY4_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY4_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.FY5_ITEMS.Replace("'", "''") & "','" &
					currREQ.FY5_UNIT_COST.Replace("'", "''") & "','" &
					currREQ.Command.Replace("'", "''") & "','" &
					currREQ.validationError.Replace("'", "''") & "')"  
			
'BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
						
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
			
			End Using		
						
			Return Nothing
		End Function
#End Region

#Region "Populate Cube For Update"		
		Public Function	PopulateCubeUpdate (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal currREQ As REQ) As Object
			
			Dim Year As String  = currREQ.scenario.Substring(5)
			Dim wfyear As Integer = Convert.ToInt32(Year)
		
			'invoke a DM task to clear added row data
				Dim dataMgmtSeq As String = "Delete_REQ"     
				Dim params As New Dictionary(Of String, String) 
				params.Add("Entity", "E#" & currREQ.entity)
				params.Add("Scenario", currREQ.scenario)
				params.Add("Flow", currREQ.Flow)

				'DEV NOTE: It ignores the first year for some reason, so it is added at the end.
				Dim time As String = ""
				time = "T#" & wfyear & ",T#" & wfyear - 1 &  ",T#" & wfyear + 1 &  ",T#" & wfyear + 2 &  ",T#" & wfyear + 3 &	  ",T#" & wfyear + 4 & ",T#" & wfyear
								   									   
				params.Add("Time", "T#" & time)
'Brapi.ErrorLog.LogMessage(si,"time = " & time)					
				BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
		
			
			
			'Brapi.ErrorLog.LogMessage(si, "Year" & wfyear )
			Dim sDataBufferPOVScript_Amount As String = "Cb#" & currREQ.Command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":V#Periodic:A#REQ_Requested_Amt:I#" & currREQ.entity & ":F#"& currREQ.flow &":O#BeforeAdj:U1#" & currREQ.fundCode & ":U2#" & currREQ.MDEP & ":U3#" & currREQ.APE9 & ":U4#" & currREQ.DollarType & ":U5#"& currREQ.sCtype & ":U6#"& currREQ.CommitmentItem & ":U7#None:U8#None"
			'FY scripts
			Dim FY1TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & wfyear
			Dim FY2TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 1).ToString
			Dim FY3TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 2).ToString
			Dim FY4TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 3).ToString
			Dim FY5TimeScript As String = sDataBufferPOVScript_Amount & ":T#" & (wfyear + 4).ToString
			'Brapi.ErrorLog.LogMessage(si, "FY1AMT" & FY1TimeScript)
			Dim objListofAmountScripts As New List(Of MemberScriptandValue)
			
		    If Not String.IsNullOrWhiteSpace(currREQ.FY1) Then
				Dim objScriptValFY1 As New MemberScriptAndValue
				objScriptValFY1.CubeName = currREQ.command
				objScriptValFY1.Script = FY1TimeScript
				objScriptValFY1.Amount = currREQ.FY1
				objScriptValFY1.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY1)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY2) Then
				Dim objScriptValFY2 As New MemberScriptAndValue
				objScriptValFY2.CubeName = currREQ.command
				objScriptValFY2.Script = FY2TimeScript
				objScriptValFY2.Amount = currREQ.FY2
				objScriptValFY2.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY2)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY3) Then
				Dim objScriptValFY3 As New MemberScriptAndValue
				objScriptValFY3.CubeName = currREQ.command
				objScriptValFY3.Script = FY3TimeScript
				objScriptValFY3.Amount = currREQ.FY3
				objScriptValFY3.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY3)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY4) Then
				Dim objScriptValFY4 As New MemberScriptAndValue
				objScriptValFY4.CubeName = currREQ.command
				objScriptValFY4.Script = FY4TimeScript
				objScriptValFY4.Amount = currREQ.FY4
				objScriptValFY4.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY4)
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.FY5) Then
				Dim objScriptValFY5 As New MemberScriptAndValue
				objScriptValFY5.CubeName = currREQ.command
				objScriptValFY5.Script = FY5TimeScript
				objScriptValFY5.Amount = currREQ.FY5
				objScriptValFY5.IsNoData = False
				objListofAmountScripts.Add(objScriptValFY5)
			End If
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofAmountScripts)
''BRApi.ErrorLog.LogMessage(si, "FY5TimeScript = " & FY5TimeScript )						
			Return Nothing
			
		End Function
#End Region

#Region "Populate Annotations For Update"		
		Public Function	PopulateAnnotationUpdate(ByVal si As SessionInfo, ByVal currREQ As REQ)As Object
		
			Dim Year As String  = currREQ.scenario.Substring(5)
			Dim wfyear As Integer = Convert.ToInt32(Year)
			Dim sDataBufferPOVScript_POVLoop As String = "Cb#" & currREQ.command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":T#" & Year & ":V#Annotation:I#None:F#"& currREQ.flow &":O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sUD5Filter As String = sDataBufferPOVScript_POVLoop.Replace("U5#None","U5#REQ_Owner")
				Dim sUD5FilterFunc As String = sDataBufferPOVScript_POVLoop.Replace("U5#None","U5#REQ_Func_POC")
				Dim txtValue As String = ""
				
				Dim objListofScripts As New List(Of MemberScriptandValue)
				
				Dim wfScenarioTypeID As Integer
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,Year)
				wfScenarioTypeID = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si, dimtypeid.Scenario, currREQ.scenario)).Id
				
'				'Set REQ_ID
'				Dim sFilterScriptREQ_ID As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ID"
''BRAPI.ErrorLog.LogMessage(si, "REQ_ID = " & REQ_ID & ", sFilterScriptREQ_ID = " & sFilterScriptREQ_ID)							
'			    Dim objScriptValREQ_ID As New MemberScriptAndValue
'				objScriptValREQ_ID.CubeName = currREQ.command
'				objScriptValREQ_ID.Script = sFilterScriptREQ_ID
'				objScriptValREQ_ID.TextValue = currREQ.REQ_ID
'				objScriptValREQ_ID.IsNoData = False
'				objListofScripts.Add(objScriptValREQ_ID)
				
				
			If Not String.IsNullOrWhiteSpace(currREQ.Title) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Title"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Title
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Description ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Description"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Description 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Justification ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Recurring_Justification"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Justification 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ImpactifnotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Impact_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ImpactifnotFunded
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.RiskifnotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Risk_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RiskifnotFunded
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CostMethodology) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Methodology_Cmt"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CostMethodology
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CostGrowthJustification) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Growth_Justification"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CostGrowthJustification
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MustFund) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Must_Fund"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MustFund
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FundingSource ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Requested_Fund_Source"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FundingSource 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ArmyInitiative_Directive) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Army_initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ArmyInitiative_Directive
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CommandInitiative_Directive) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Command_Initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CommandInitiative_Directive
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Activity_Exercise) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Activity_Exercise"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Activity_Exercise
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.IT_CyberRequirement ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_IT_Cyber_Rqmt_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.IT_CyberRequirement 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UIC) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UIC_Acct"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UIC
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField1 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_1"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField1 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField2 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_2"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField2 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField3 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_3"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField3 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField4 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_4"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField4 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FlexField5 ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_5"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField5 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.EmergingRequirement) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_New_Rqmt_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.EmergingRequirement
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CPATopic) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CPA_Topic"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CPATopic
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.PBRSubmission) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_PBR_Submission"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.PBRSubmission
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UPLSubmission) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UPL_Submission"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UPLSubmission
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ContractNumber ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Contract_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ContractNumber 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TaskOrderNumber ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Task_Order_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TaskOrderNumber 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.AwardTargetDate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Target_Date_Of_Award"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.AwardTargetDate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.POPExpirationDate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POP_Expiration_Date"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.POPExpirationDate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ContractorManYearEquiv_CME ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FTE_CME"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ContractorManYearEquiv_CME 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.COREmail ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_COR_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.COREmail 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.POCEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POC_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.POCEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Directorate ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Directorate"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Directorate 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Division ) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Division"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Division 
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Branch) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Branch"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Branch
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ReviewingPOCEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Rev_POC_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ReviewingPOCEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MDEPFunctionalEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_MDEP_Func_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MDEPFunctionalEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.NotificationListEmails) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Notification_Email_List"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.NotificationListEmails
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.GeneralComments_Notes) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Comments"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.GeneralComments_Notes
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.JUON) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_JUON"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.JUON
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ISR_Flag) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ISR_Flag"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ISR_Flag
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Cost_Model) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Model"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Cost_Model
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Combat_Loss) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Combat_Loss"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Combat_Loss
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Cost_Location) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Location"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Cost_Location
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Category_A_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Category_A_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Category_A_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CBS_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CBS_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CBS_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.MIP_Proj_Code) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_MIP_Proj_Code"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MIP_Proj_Code
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.RequirementType) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Type"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RequirementType
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.DD_Priority) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_DD_Priority"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.DD_Priority
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Portfolio) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Portfolio"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Portfolio
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.DD_Capability) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Capability_DD"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.DD_Capability
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.JNT_CAP_AREA) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_JNT_CAP_AREA"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.JNT_CAP_AREA
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TBM_COST_POOL) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_TBM_COST_POOL"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TBM_COST_POOL
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.TBM_TOWER) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_TBM_TOWER"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TBM_TOWER
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.APMSAITRNum) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_APMS_AITR_Num"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.APMSAITRNum
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ZERO_TRUST_CAPABILITY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_ZERO_TRUST_CAPABILITY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ZERO_TRUST_CAPABILITY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.ASSOCIATED_DIRECTIVES) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Assoc_Directorate"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ASSOCIATED_DIRECTIVES
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.CLOUD_INDICATOR) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CLOUD_INDICATOR"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CLOUD_INDICATOR
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.STRAT_CYBERSEC_PGRM) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_STRAT_CYBERSEC_PGRM"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.STRAT_CYBERSEC_PGRM
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.NOTES) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Notes"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.NOTES
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.UNIT_OF_MEASURE) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_UNIT_OF_MEASURE"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.UNIT_OF_MEASURE
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY5_ITEMS) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_ITEMS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_ITEMS
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY5_UNIT_COST) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_UNIT_COST"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_UNIT_COST
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.SS_Priority) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_SS_Priority"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.SS_Priority
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Commitment_Group) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Commitment_Group"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Commitment_Group
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.SS_Capability) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Capability_SS"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.SS_Capability
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.Strategic_BIN) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Strategic_BIN"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Strategic_BIN
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.LIN) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_LIN"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.LIN
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY1_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY1_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY1_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY2_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY2_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY2_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY3_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY3_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY3_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
If Not String.IsNullOrWhiteSpace(currREQ.FY4_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY4_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY4_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
                If Not String.IsNullOrWhiteSpace(currREQ.FY5_QTY) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FY5_QTY"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FY5_QTY
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
			
'--------------------------------------------------------------------------------------------------------------					
'				'Set status to L?_Working. Get the level from the entity
'				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, currREQ.entity).Member
'				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
'				Dim REQStatus As String = entityText3.Substring(entityText3.Length -2, 2) & " Imported"
'				Dim sFilterScriptWFStatus As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Rqmt_Status"
'			    Dim objScriptValWFStatus As New MemberScriptAndValue
'				objScriptValWFStatus.CubeName = currREQ.command
'				objScriptValWFStatus.Script = sFilterScriptWFStatus
'				objScriptValWFStatus.TextValue = REQStatus
'				objScriptValWFStatus.IsNoData = False
'				objListofScripts.Add(objScriptValWFStatus)
				
				'Set Funding Status
				Dim sFilterScriptFundingStatus As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Funding_Status"
				txtValue = "Unfunded"
			    Dim objScriptValFundingStatus As New MemberScriptAndValue
				objScriptValFundingStatus.CubeName = currREQ.command
				objScriptValFundingStatus.Script = sFilterScriptFundingStatus
				objScriptValFundingStatus.TextValue = txtValue
				objScriptValFundingStatus.IsNoData = False
				objListofScripts.Add(objScriptValFundingStatus)
				
				Dim sREQUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
							
				'Update UFR Updated Date
				Dim sFilterScriptLastUpdateddate As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Last_Updated_Date"
				Dim objListofScriptUpdatedDate As New List(Of MemberScriptandValue)
			    Dim objScriptValUpdatedDate As New MemberScriptAndValue
				objScriptValUpdatedDate.CubeName = currREQ.command
				objScriptValUpdatedDate.Script = sFilterScriptLastUpdateddate
				objScriptValUpdatedDate.TextValue = CurDate
				objScriptValUpdatedDate.IsNoData = False
				objListofScriptUpdatedDate.Add(objScriptValUpdatedDate)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptUpdatedDate)
				
				'Update UFR Updated Name
				Dim sFilterScriptLastUpdatedName As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Last_Updated_Name"
				Dim objListofScriptUpdatedName As New List(Of MemberScriptandValue)
			    Dim objScriptValUpdatedName As New MemberScriptAndValue
				
				objScriptValUpdatedName.CubeName = currREQ.command
				objScriptValUpdatedName.Script = sFilterScriptLastUpdatedName
				objScriptValUpdatedName.TextValue = sREQUser
				objScriptValUpdatedName.IsNoData = False
				objListofScriptUpdatedName.Add(objScriptValUpdatedName)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptUpdatedName)
				
				
				
				
'				'Set REQ Status History
'				Dim sREQUser As String = si.UserName
'				Dim CurDate As Date = Datetime.Now
'				Dim sCompleteRQWFStatus As String = sREQUser & " : " & CurDate & " : " & currREQ.entity & " : " & REQStatus
'				Dim sFilterScriptWFStatusHistory As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Status_History"
'			    Dim objScriptValWFStatusHistory As New MemberScriptAndValue
'				objScriptValWFStatusHistory.CubeName = currREQ.command
'				objScriptValWFStatusHistory.Script = sFilterScriptWFStatusHistory
'				objScriptValWFStatusHistory.TextValue = sCompleteRQWFStatus
'				objScriptValWFStatusHistory.IsNoData = False
'				objListofScripts.Add(objScriptValWFStatusHistory)	

'				'Set created by
'				Dim sFilterScriptCreatedBy As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Creator_Name"
'			    Dim objScriptValCreatedBy As New MemberScriptAndValue
'				objScriptValCreatedBy.CubeName = currREQ.command
'				objScriptValCreatedBy.Script = sFilterScriptCreatedBy
'				objScriptValCreatedBy.TextValue = sREQUser
'				objScriptValCreatedBy.IsNoData = False
'				objListofScripts.Add(objScriptValCreatedBy)	
				
'				'Set Creation date
'				Dim sFilterScriptCreatedTime As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Creation_Date_Time"
'			    Dim objScriptValCreatedTime As New MemberScriptAndValue
'				objScriptValCreatedTime.CubeName = currREQ.command
'				objScriptValCreatedTime.Script = sFilterScriptCreatedTime
'				objScriptValCreatedTime.TextValue = System.DateTime.Now.ToString()
'				objScriptValCreatedTime.IsNoData = False
'				objListofScripts.Add(objScriptValCreatedTime)	
				
				
				'Set cell for all the fields
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				Return Nothing
		End Function
#End Region

#Region "Get Used Flows"
'			Public Function GetUsedFlows(ByVal si As SessionInfo, ByVal command As String, ByVal scenario As String) As Object
			
'				Dim UsedFlowsSQL As String ="Select [Entity], Flow From DataAttachment WHERE [Cube] = '" & command &"' And Scenario =  '" & scenario & "'"
			
'				Dim dtUsedFlows As New DataTable()
'				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
'					dtUsedFlows =BRApi.Database.ExecuteSql(dbConnApp,UsedFlowsSQL,True)
'				End Using
				
'				Dim usedFlowsPerScenarioAndFC As List(Of String) = New List(Of String)
			
'				For Each r As DataRow In dtUsedFlows.Rows
'					usedFlowsPerScenarioAndFC.Add(r.Item("Entity") & ":" &  r.Item("Flow"))		
'				Next
				
'				Return usedFlowsPerScenarioAndFC
'			End Function

#End Region
#End Region




	End Class

#Region "REQ Class"
	Public Class REQ
		'Class to hold the requirement object
		'-----Mapping definitions----
		Public Dim Entity As String = ""
		Public Dim FundCode As String = ""
		Public Dim MDEP As String = ""
		Public Dim APE9 As String = ""
		Public Dim DollarType As String = ""
		Public Dim Cycle As String = ""
		Public Dim FY1 As String = ""
		Public Dim FY2 As String = ""
		Public Dim FY3 As String = ""
		Public Dim FY4 As String = ""
		Public Dim FY5 As String = ""
		Public Dim Title As String = ""
		Public Dim Description As String = ""
		Public Dim Justification  As String = ""
		Public Dim ImpactifnotFunded As String = ""
		Public Dim RiskifnotFunded As String = ""
		Public Dim CostMethodology As String = ""
		Public Dim CostGrowthJustification As String = ""
		Public Dim MustFund As String = ""
		Public Dim FundingSource  As String = ""
		Public Dim ArmyInitiative_Directive As String = ""
		Public Dim CommandInitiative_Directive As String = ""
		Public Dim Activity_Exercise As String = ""
		Public Dim IT_CyberRequirement  As String = ""
		Public Dim UIC As String = ""
		Public Dim FlexField1  As String = ""
		Public Dim FlexField2  As String = ""
		Public Dim FlexField3  As String = ""
		Public Dim FlexField4  As String = ""
		Public Dim FlexField5  As String = ""
		Public Dim EmergingRequirement As String = ""
		Public Dim CPATopic As String = ""
		Public Dim PBRSubmission As String = ""
		Public Dim UPLSubmission As String = ""
		Public Dim ContractNumber  As String = ""
		Public Dim TaskOrderNumber  As String = ""
		Public Dim AwardTargetDate  As String = ""
		Public Dim POPExpirationDate  As String = ""
		Public Dim ContractorManYearEquiv_CME  As String = ""
		Public Dim COREmail  As String = ""
		Public Dim POCEmail As String = ""
		Public Dim Directorate  As String = ""
		Public Dim Division  As String = ""
		Public Dim Branch As String = ""
		Public Dim ReviewingPOCEmail As String = ""
		Public Dim MDEPFunctionalEmail As String = ""
		Public Dim NotificationListEmails As String = ""
		Public Dim GeneralComments_Notes As String = ""
		Public Dim JUON As String = ""
		Public Dim ISR_Flag As String = ""
		Public Dim Cost_Model As String = ""
		Public Dim Combat_Loss As String = ""
		Public Dim Cost_Location As String = ""
		Public Dim Category_A_Code As String = ""
		Public Dim CBS_Code As String = ""
		Public Dim MIP_Proj_Code As String = ""
		Public Dim RequirementType As String = ""
		Public Dim DD_Priority As String = ""
		Public Dim Portfolio As String = ""
		Public Dim DD_Capability As String = ""
		Public Dim JNT_CAP_AREA As String = ""
		Public Dim TBM_COST_POOL As String = ""
		Public Dim TBM_TOWER As String = ""
		Public Dim APMSAITRNum As String = ""
		Public Dim ZERO_TRUST_CAPABILITY As String = ""
		Public Dim ASSOCIATED_DIRECTIVES As String = ""
		Public Dim CLOUD_INDICATOR As String = ""
		Public Dim STRAT_CYBERSEC_PGRM As String = ""
		Public Dim NOTES As String = ""
		Public Dim UNIT_OF_MEASURE As String = ""
		Public Dim FY1_ITEMS As String = ""
		Public Dim FY1_UNIT_COST As String = ""
		Public Dim FY2_ITEMS As String = ""
		Public Dim FY2_UNIT_COST As String = ""
		Public Dim FY3_ITEMS As String = ""
		Public Dim FY3_UNIT_COST As String = ""
		Public Dim FY4_ITEMS As String = ""
		Public Dim FY4_UNIT_COST As String = ""
		Public Dim FY5_ITEMS As String = ""
		Public Dim FY5_UNIT_COST As String = ""
		Public Dim SS_Priority As String = ""
		Public Dim Commitment_Group As String = ""
		Public Dim SS_Capability As String = ""
		Public Dim Strategic_BIN As String = ""
		Public Dim LIN As String = ""
		Public Dim FY1_QTY As String = ""
		Public Dim FY2_QTY As String = ""
		Public Dim FY3_QTY As String = ""
		Public Dim FY4_QTY As String = ""
		Public Dim FY5_QTY As String = ""
		Public Dim CommitmentItem As String = ""
		Public Dim sCType As String = ""
		Public Dim Status As String = ""
		Public Dim flow As String = ""				'Calculated
		Public Dim REQ_ID As String = ""			'Calculated
		Public Dim command As String = ""			'Calculated
		Public Dim scenario As String = ""			'Calculated
		
		Public Dim valid As Boolean = True
		Public Dim validationError As String = ""
		
'		Public Sub SetValidationError(ByVal valError As String)
'			If String.IsNullOrWhiteSpace(validationError) Then
'				validationError = "REQ " & title & " has errors. " &  valError
'			Else
'				validationError = validationError  & ", "  & valError
'			End If
'		End Sub
		
		Public Function StringOutput() As String
			Dim output As String = Me.command & "," & _	
			Me.Entity & "," & _
			Me.FundCode & "," & _
			Me.MDEP & "," & _
			Me.APE9 & "," & _
			Me.DollarType & "," & _
			Me.Cycle & "," & _
			Me.FY1 & "," & _
			Me.FY2 & "," & _
			Me.FY3 & "," & _
			Me.FY4 & "," & _
			Me.FY5 & "," & _
			Me.Title & "," & _
			Me.Description  & "," & _
			Me.Justification  & "," & _
			Me.ImpactifnotFunded & "," & _
			Me.RiskifnotFunded & "," & _
			Me.CostMethodology & "," & _
			Me.CostGrowthJustification & "," & _
			Me.MustFund & "," & _
			Me.FundingSource  & "," & _
			Me.ArmyInitiative_Directive & "," & _
			Me.CommandInitiative_Directive & "," & _
			Me.Activity_Exercise & "," & _
			Me.IT_CyberRequirement  & "," & _
			Me.UIC & "," & _
			Me.FlexField1  & "," & _
			Me.FlexField2  & "," & _
			Me.FlexField3  & "," & _
			Me.FlexField4  & "," & _
			Me.FlexField5  & "," & _
			Me.EmergingRequirement & "," & _
			Me.CPATopic & "," & _
			Me.PBRSubmission & "," & _
			Me.UPLSubmission & "," & _
			Me.ContractNumber  & "," & _
			Me.TaskOrderNumber  & "," & _
			Me.AwardTargetDate  & "," & _
			Me.POPExpirationDate  & "," & _
			Me.ContractorManYearEquiv_CME  & "," & _
			Me.COREmail  & "," & _
			Me.POCEmail & "," & _
			Me.Directorate  & "," & _
			Me.Division  & "," & _
			Me.Branch & "," & _
			Me.ReviewingPOCEmail & "," & _
			Me.MDEPFunctionalEmail & "," & _
			Me.NotificationListEmails & "," & _
			Me.GeneralComments_Notes & "," & _
			Me.JUON & "," & _
			Me.ISR_Flag & "," & _
			Me.Cost_Model & "," & _
			Me.Combat_Loss & "," & _
			Me.Cost_Location & "," & _
			Me.Category_A_Code & "," & _
			Me.CBS_Code & "," & _
			Me.MIP_Proj_Code & "," & _
			Me.RequirementType & "," & _
			Me.DD_Priority & "," & _
			Me.Portfolio & "," & _
			Me.DD_Capability & "," & _
			Me.JNT_CAP_AREA & "," & _
			Me.TBM_COST_POOL & "," & _
			Me.TBM_TOWER & "," & _
			Me.APMSAITRNum & "," & _
			Me.ZERO_TRUST_CAPABILITY & "," & _
			Me.ASSOCIATED_DIRECTIVES & "," & _
			Me.CLOUD_INDICATOR & "," & _
			Me.STRAT_CYBERSEC_PGRM & "," & _
			Me.NOTES & "," & _
			Me.UNIT_OF_MEASURE & "," & _
			Me.FY1_ITEMS & "," & _
			Me.FY1_UNIT_COST & "," & _
			Me.FY2_ITEMS & "," & _
			Me.FY2_UNIT_COST & "," & _
			Me.FY3_ITEMS & "," & _
			Me.FY3_UNIT_COST & "," & _
			Me.FY4_ITEMS & "," & _
			Me.FY4_UNIT_COST & "," & _
			Me.FY5_ITEMS & "," & _
			Me.FY5_UNIT_COST & "," & _
			Me.SS_Priority & "," & _
			Me.Commitment_Group & "," & _
			Me.SS_Capability & "," & _
			Me.Strategic_BIN & "," & _
			Me.LIN & "," & _
			Me.FY1_QTY & "," & _
			Me.FY2_QTY & "," & _
			Me.FY3_QTY & "," & _
			Me.FY4_QTY & "," & _
			Me.FY5_QTY & "," & _
			Me.flow & "," & _				
			Me.REQ_ID & "," & _			
			Me.scenario & "," & _	
			Me.validationError
			
			Return output
		End Function
	End Class
#End Region

End Namespace
