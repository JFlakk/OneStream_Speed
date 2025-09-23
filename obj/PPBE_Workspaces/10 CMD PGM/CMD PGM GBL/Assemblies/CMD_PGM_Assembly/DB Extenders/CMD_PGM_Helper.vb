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
						Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
						Select Case args.FunctionName.ToLower()
							Case "check_wf_complete_lock"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								Return dbExt_ChangedResult
							Case args.FunctionName.XFEqualsIgnoreCase("CreateREQMain")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								Me.DbCache(si,args)
							Case "update_status"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "copy_req"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Copy_REQ()
								Return dbExt_ChangedResult
							Case "delete_req"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Delete_REQ()
								Return dbExt_ChangedResult
							Case "set_related_reqs"
								dbExt_ChangedResult = Me.Set_Related_REQs()
								Return dbExt_ChangedResult
							Case "send_status_change_email"
								dbExt_ChangedResult = Me.Send_Status_Change_Email()
								Return dbExt_ChangedResult
							Case "attach_doc"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Attach_Doc()
								Return dbExt_ChangedResult
						End Select					

#Region "Cache Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("CachePrompts") Then
						     Me.Check_WF_Complete_Lock(si, globals, api, args)	
							 Me.CachePrompts(si,globals,api,args)
						End If
#End Region


#Region "Set Notification List"
						If args.FunctionName.XFEqualsIgnoreCase("SetNotificationList") Then	
							 Me.SetNotificationList()
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
	Private BR_REQDataSet As New Workspace.CMD_PGM.CMD_PGM_Assembly.BusinessRule.DashboardDataSet.CMD_PGM_DataSet.MainClass()
	Public GEN_General_String_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardStringFunction.GBL_General_String_Helper.MainClass	
	
	
#End Region

#Region "Status Updates"
		Public Function Update_Status() As xfselectionchangedTaskResult
			Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()			
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
			
			Return dbExt_ChangedResult
			
		End Function
		
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
					Me.Send_Status_Change_Email(si, globals, api, args)
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
					Me.Send_Status_Change_Email(si, globals, api, args)
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
						Me.Send_Status_Change_Email(si, globals, api, args)
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
						Me.Send_Status_Change_Email(si, globals, api, args)
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
						Me.Send_Status_Change_Email(si, globals, api, args)
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
				Me.Send_Status_Change_Email(si, globals, api, args)
			Catch ex As Exception
			End Try
			
			'Update date and name
			Try
				Me.LastUpdated(si, globals, api, args, sREQ, sEntity)
			Catch ex As Exception
			End Try
			
			Return Nothing
		End Function
		
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
						Me.Send_Status_Change_Email(si, globals, api, args, sREQ)
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
						Me.Send_Status_Change_Email(si, globals, api, args, sREQ)
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
						Me.Send_Status_Change_Email(si, globals, api, args, sUFR, sFundCenter)
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
						Me.Send_Status_Change_Email(si, globals, api, args, sREQ)
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
						Me.Send_Status_Change_Email(si, globals, api, args, sREQ)
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
		
#End Region

#Region "Manage Requirements"
		
		Public Function Copy_REQ() As xfselectionchangedTaskResult
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

		Public Function Delete_REQ() As xfselectionchangedTaskResult
									
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
			  
		Public Function Attach_Doc() As xfselectionchangedTaskResult
		End Function
			
		Public Function Set_Related_REQs() As xfselectionchangedTaskResult
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

#End Region

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
					Me.Send_Status_Change_Email(si, globals, api, args)
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

#Region "Send Status Change Email "
	'Updated EH 08292024 RMW-1565 Updated sREQTime to annual for PGM_C20XX and Centralized Text (REQ_Shared, 1999)
	'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
	Public Function Send_Status_Change_Email(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal REQ As String =  "", Optional ByVal Entity As String =  "")
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

#Region "Set Notification List"

	Public Function SetNotificationList()
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

#Region "Manage Manpower REQs"

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
		
		End Function
#End Region	

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

#Region "Property"
	Private Property sFilePath As String = ""
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

	End Class

End Namespace
