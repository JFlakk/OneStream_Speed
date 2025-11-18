Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Data.SqlClient
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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly
Imports System.IO.Compression



Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_SPLN_Helper
	Public Class MainClass
		Private si As SessionInfo
        Public globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
'Brapi.ErrorLog.LogMessage(si,"@In helper " &args.FunctionType.ToString & " : " & args.FunctionName.ToLower())				
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args	
					
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						Dim dbExt_LoadResult As New XFLoadDashboardTaskResult()
						Select Case args.FunctionName.ToLower()
								Case "load_req_detailsdashboard"
'bRApi.ErrorLog.LogMessage(si,$"Hit this {args.FunctionName.ToLower()}")
									dbExt_LoadResult = Me.load_req_detailsdashboard()
									Return dbExt_LoadResult	
'								Case "load_cmd_SPLN_header"
'									dbExt_LoadResult = Me.load_cmd_SPLN_header()
'									Return dbExt_LoadResult
						End Select		
						
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						'====Validate whether a wf is locked or not====
						Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
						Dim dargs As New DashboardExtenderArgs
						dargs.FunctionName = "Check_WF_Complete_Lock"
						Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)
'BRApi.ErrorLog.LogMessage(si,$"{sWFStatus}")	
							If Not sWFStatus.XFContainsIgnoreCase("unlock")
								dbExt_ChangedResult.IsOK = False
								dbExt_ChangedResult.ShowMessageBox = True
								dbExt_ChangedResult.Message = vbCRLF & "Current workflow step is locked. Please contact your requriements manager to open access." & vbCRLF	
								Return dbExt_ChangedResult
							End If	

'BRApi.ErrorLog.LogMessage(si,args.FunctionName.ToLower())								
						Select Case args.FunctionName.ToLower()					
							Case "check_wf_complete_lock"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								Return dbExt_ChangedResult
							Case args.FunctionName.XFEqualsIgnoreCase("CreateREQMain")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								Me.DbCache(si,args)
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
								'dbExt_ChangedResult = Me.Send_Status_Change_Email()
								Return dbExt_ChangedResult
							Case "attach_doc"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Attach_Doc()
								Return dbExt_ChangedResult
							Case "submit_reqs", "importreq", "rollfwdreq","manage_req_status"
'brapi.ErrorLog.LogMessage(si,"Hit 1 Helper CMKL")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "rollfwdreq"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								If args.NameValuePairs.XFGetValue("Action").XFEqualsIgnoreCase("Insert")
									dbExt_ChangedResult = Me.Update_Status()
								End If
								If args.NameValuePairs.XFGetValue("Action").XFEqualsIgnoreCase("Delete")
									dbExt_ChangedResult = Me.DeleteRequirementID()
								End If
								
								Return dbExt_ChangedResult									
							Case "validate_reqs"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "prioritize_reqs"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "approve_reqs"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "demote_reqs"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "create_civpay_req"
								dbExt_ChangedResult = Me.CreateCivPayREQ(globals)
								Return dbExt_ChangedResult
							Case "create_withhold_req"
								dbExt_ChangedResult = Me.CreateWithholdREQ(globals)
								Return dbExt_ChangedResult
							Case "setrelatedreqs"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.SetRelatedREQs()
								Return dbExt_ChangedResult
							Case "save_adjustfundingline"
								dbExt_ChangedResult = Me.Save_AdjustFundingLine()
								Return dbExt_ChangedResult
							Case "saveweightprioritization"			
								dbExt_ChangedResult = Me.saveweightprioritization()
								Return dbExt_ChangedResult		
							Case "setnotificationlist"		
								Dim notificationEmails As String = args.NameValuePairs.XFGetValue("Emails")
								Dim vNotificationEmails As String = args.NameValuePairs.XFGetValue("vEmails")
								If notificationEmails.Length = 0 And vNotificationEmails.Length = 0 Then
									dbExt_ChangedResult.IsOK = False
									dbExt_ChangedResult.ShowMessageBox = True
									dbExt_ChangedResult.Message = vbCRLF & "No stakeholder(s) or validator(s) were added to the email notification list. Please select users before confirming." & vbCRLF	
									Return dbExt_ChangedResult	
								End If
								dbExt_ChangedResult = Me.setnotificationlist()
								Return dbExt_ChangedResult	
							Case "deleterequirementids"		
								dbExt_ChangedResult = Me.DeleteRequirementID()
								Return dbExt_ChangedResult	
								
							Case "attachdocument"
								dbExt_ChangedResult = Me.AttachDocument()
								Return dbExt_ChangedResult
								
								Case "downloaddocument"
								dbExt_ChangedResult = Me.DownloadDocument()
								Return dbExt_ChangedResult
								
							Case "clearkey5param"
								dbExt_ChangedResult = Me.ClearKey5param()
								Return dbExt_ChangedResult
							
							Case "clearkey5params_nofcappn"
								dbExt_ChangedResult = Me.ClearKey5params_NoFCAPPN()
								Return dbExt_ChangedResult
							Case "packagesubmission"
								Dim Entity_APPN As String = args.NameValuePairs.XFGetValue("Entity_APPN")
								brapi.ErrorLog.LogMessage(si,"entity_appn: " & Entity_APPN)
								
								
						End Select					

#Region "Cache Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("CachePrompts") Then
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
							 Me.CachePrompts(si,globals,api,args)
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

#Region "Demote REQ"
						If args.FunctionName.XFEqualsIgnoreCase("DemoteREQ") Then	
							 'Me.DemoteREQ(si,globals,api,args)
						End If
#End Region

#Region "Clear Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("ClearPrompts") Then
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'BRApi.ErrorLog.LogMessage(si,"test KN")
'							 Me.ClearPrompts(si,globals,api,args)

						End If

#End Region

#Region "OBE Roll Fwd Req"
'						If args.FunctionName.XFEqualsIgnoreCase("RollFwdReq") Then	

'							'---------------------------------------------------------------------------------------------------
'							' PURPOSE: invoke data management to copy requirements from source S#T# to target S#T#
'							'
'							' LOGIC OVERVIEW:
'							'		- if Roll Forward flag = No then exit
'							'		- data management sequence = RollFwdReq 
'							'
'							' USAGE:
'							'		- From button:
'							'			Sever Task = Execute Dashbaord Extender Business Rule (General Server)
'							'			Task Arguments = {REQ_SolutionHelper}{RollFwdReq}{}
'							'			
'							' MODIFIED: 
'							' <date> 		<user id> 	<JIRA ticket> 	<change description>
'							' 2024-04-02 	AK 			RMW-1171		created	
'							'---------------------------------------------------------------------------------------------------
							
''Brapi.ErrorLog.LogMessage(si,"Sequence should have started")

'							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'					        Dim sCurrScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'							Dim sCurrTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)

							
'							'---------- get Entity from WF title ----------
'							Dim StringArgs As DashboardStringFunctionArgs = New DashboardStringFunctionArgs
'							StringArgs.FunctionName = "GetPrimaryFundCenter"
'							StringArgs.NameValuePairs.XFSetValue("Cube", sCube)
'							Dim sEntity As String = GEN_General_String_Helper.Main(si, globals, api, StringArgs)


'							'---------- if roll over flag is set to NO then exit ----------
'							Dim sRllFwdFlag As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, "E#" & sEntity & "_General:P#" & sEntity & ":C#Local:S#" & sCurrScenario & ":T#" & sCurrTime & "M12:V#Annotation:A#REQ_Allow_Rollover:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").DataCellEx.DataCellAnnotation
''Brapi.ErrorLog.LogMessage(si,"RollFwdReq: E#" & sEntity & "_General:P#" & sEntity & ":C#Local:S#" & sCurrScenario & ":T#" & sCurrTime & "M12:V#Annotation:A#REQ_Allow_Rollover:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
''Brapi.ErrorLog.LogMessage(si,"RollFwdReq: sRllFwdFlag=" & sRllFwdFlag)

'							If sRllFwdFlag.XFEqualsIgnoreCase("no") Then
'								Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
'									selectionChangedTaskResult.IsOK = True
'									selectionChangedTaskResult.ShowMessageBox = True
'									selectionChangedTaskResult.Message = "Roll Forward has been disallowed by requirements manager."
'								Return selectionChangedTaskResult									
								
'							End If	
								
'							'---------- run data mgmt sequence ----------
'							Dim dataMgmtSeq As String = "RollFwdReq"     
'							Dim params As New Dictionary(Of String, String) 
'							'params.Add("Scenario", sScenario)
'							'params.Add("Time", "T#" & sTime & ".Base")
'							'params.Add("Entity", "E#" & sEntity)
''brapi.ErrorLog.LogMessage(si,"here1")					
'							BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)

							
'							'---------- display done message ----------
'							Dim selectionChangedTaskResult2 As New XFSelectionChangedTaskResult()
'								selectionChangedTaskResult2.IsOK = True
'								selectionChangedTaskResult2.ShowMessageBox = True
'								selectionChangedTaskResult2.Message = "Check Task Activity to confirm when Roll Forward is done"
'							Return selectionChangedTaskResult2									
'						End If
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
	'Private BR_REQDataSet As New Workspace.CMD_SPLN.CMD_SPLN_Assembly.BusinessRule.DashboardDataSet.CMD_SPLN_DataSet.MainClass()
	Public GEN_General_String_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardStringFunction.GBL_String_Helper.MainClass
	Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
	
	
#End Region

#Region "Set Notification List"

	Public Function setnotificationlist()
		Try
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","") 'args.NameValuePairs.XFGetValue("UFREntity")
			Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)	
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim notificationEmails As String = args.NameValuePairs.XFGetValue("Emails")
			Dim vNotificationEmails As String = args.NameValuePairs.XFGetValue("vEmails")
			
			Dim stakeholderEmailList As String() = notificationEmails.split(",")
			Dim validatorEmailList As String() = vNotificationEmails.split(",")
			
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
				
           
					Dim sqa_xfc_cmd_SPLN_req = New SQA_XFC_CMD_SPLN_REQ(connection)
					Dim SQA_XFC_CMD_SPLN_REQ_DT = New DataTable()
					Dim sqa = New SqlDataAdapter()

				'Fill the DataTable With the current data From FMM_Dest_Cell
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
   		}
			sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DT, sql, sqlparams)
			
			
			Dim REQ_ID As String  = sREQ.Split(" "c)(1)
			Dim Mainreqrow As DataRow = SQA_XFC_CMD_SPLN_REQ_DT.Select($"REQ_ID ='{REQ_ID}'" ).FirstOrDefault()
			Dim Existingstakeholders As String = Mainreqrow("Notification_List_Emails").ToString()
			
			
			Dim stakeholderlist As New List(Of String)(existingStakeholders.Split(","c, StringSplitOptions.RemoveEmptyEntries).Select(Function(e) e.Trim())
                )
			For Each newEmail As String In stakeholderEmailList
                   
                    If Not stakeholderList.Any(Function(e) e.Equals(newEmail.Trim(), StringComparison.OrdinalIgnoreCase)) Then
                        stakeholderList.Add(newEmail.Trim()) 
                    End If
                Next
                
                Mainreqrow("Notification_List_Emails") = String.Join(",", stakeholderList)

                
                Dim existingValidators As String = Mainreqrow("Validation_List_Emails").ToString()
                Dim validatorList As New List(Of String)(
                    existingValidators.Split(","c, StringSplitOptions.RemoveEmptyEntries).Select(Function(e) e.Trim())
                )

                For Each newEmail As String In validatorEmailList
                    If Not validatorList.Any(Function(e) e.Equals(newEmail.Trim(), StringComparison.OrdinalIgnoreCase)) Then
                        validatorList.Add(newEmail.Trim())
                    End If
                Next
                Mainreqrow("Validation_List_Emails") = String.Join(",", validatorList)

		sqa_xfc_cmd_SPLN_req.Update_XFC_CMD_SPLN_REQ(SQA_XFC_CMD_SPLN_REQ_DT, sqa)

			Return Nothing
		End Using 
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try 
	End Function
#End Region

#Region "Status Updates"
		Public Function Update_Status() As xfselectionchangedTaskResult

			Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
			Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs","NA")
			Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
			Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfStepAllowed As Boolean = True
brapi.ErrorLog.LogMessage(si,"REQIDS: " & req_IDs & ", Status: " & new_Status)

			Try
				If req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate") And Not Dashboard.XFContainsIgnoreCase("Mpr") Then
					
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Validate") Then
					Me.Update_REQ_Status("Validate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Import") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Rollover") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve CMD Requirements") Then
					Me.Update_REQ_Status("Approve CMD")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate CMD Requirements") And Dashboard.XFContainsIgnoreCase("Mpr") Then
					Me.Update_REQ_Status("Formulate CMD")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve Requirements") Then
					Me.Update_REQ_Status("Approve")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Manage") Then
					Me.ManageREQStatusUpdated(si, globals, api, args, "")
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
'brapi.ErrorLog.LogMessage(si,"hit inside status update first func")
			Return dbExt_ChangedResult

		End Function



		''' <summary>
		''' Centralized helper to set a REQ workflow status, update history, send emails, and set last updated.
		''' </summary>
		Private Function Update_REQ_Status(ByVal curr_Status As String) As xfselectionchangedTaskResult
			Try

				Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
				Dim demote_comment As String = args.NameValuePairs.XFGetValue("demotecomment")
				Dim FCList As New List(Of String)
				Dim Status_manager As New Dictionary(Of String,String)
				Status_manager.Add("L5_Formulate_SPLN|Validate","L4_Validate_SPLN")
				Status_manager.Add("L4_Formulate_SPLN|Validate","L3_Validate_SPLN")
				Status_manager.Add("L3_Formulate_SPLN|Validate","L3_Validate_SPLN")
				Status_manager.Add("L2_Formulate_SPLN|Validate","L2_Validate_SPLN")
				Status_manager.Add("L4_Validate_SPLN|Approve","L3_Validate_SPLN")
				Status_manager.Add("L3_Validate_SPLN|Approve","L3_Approve_SPLN")
				Status_manager.Add("L2_Validate_SPLN|Approve","L2_Approve_SPLN")
				Status_manager.Add("L2_Approve_SPLN|Final","L2_Final_SPLN")
				Status_manager.Add("L3_Approve_SPLN|Validate","L2_Validate_SPLN")
				Status_manager.Add("L2_Formulate_SPLN|Final","L2_Final_SPLN")
				Status_manager.Add("L2_Final_SPLN|Formulate","L2_Formulate_SPLN")

				
				If args.NameValuePairs.XFGetValue("Demote") = "True" Then
#Region "Demote Statuses"
					'--------------------------Demote Statuses----------------------------------
					'Approve
					Status_manager.Add("L2_Approve_SPLN|L2_Validate_SPLN","L2_Validate_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L3_Approve_SPLN","L3_Approve_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L3_Validate_SPLN","L3_Validate_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L3_Formulate_SPLN","L3_Formulate_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L4_Approve_SPLN","L4_Approve_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L4_Validate_SPLN","L4_Validate_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L2_Approve_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")
			
					Status_manager.Add("L3_Approve_SPLN|L3_Validate_SPLN","L3_Validate_SPLN")
					Status_manager.Add("L3_Approve_SPLN|L3_Formulate_SPLN","L3_Formulate_SPLN")
					Status_manager.Add("L3_Approve_SPLN|L4_Approve_SPLN","L4_Approve_SPLN")

					Status_manager.Add("L3_Approve_SPLN|L4_Validate_SPLN","L4_Validate_SPLN")
					Status_manager.Add("L3_Approve_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L3_Approve_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")

					Status_manager.Add("L4_Approve_SPLN|L4_Validate_SPLN","L4_Validate_SPLN")
					Status_manager.Add("L4_Approve_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L4_Approve_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")
					
					'Validate
					Status_manager.Add("L2_Validate_SPLN|L3_Approve_SPLN","L3_Approve_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L3_Validate_SPLN","L3_Validate_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L3_Formulate_SPLN","L3_Formulate_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L4_Approve_SPLN","L4_Approve_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L4_Validate_SPLN","L4_Validate_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L2_Validate_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")
					
					Status_manager.Add("L3_Validate_SPLN|L3_Formulate_SPLN","L3_Formulate_SPLN")
					Status_manager.Add("L3_Validate_SPLN|L4_Approve_SPLN","L4_Approve_SPLN")
					Status_manager.Add("L3_Validate_SPLN|L4_Validate_SPLN","L4_Validate_SPLN")
					Status_manager.Add("L3_Validate_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L3_Validate_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")
					
					Status_manager.Add("L4_Validate_SPLN|L4_Formulate_SPLN","L4_Formulate_SPLN")
					Status_manager.Add("L4_Validate_SPLN|L5_Formulate_SPLN","L5_Formulate_SPLN")
					'--------------------------Demote Statuses----------------------------------
#End Region
				End If 
				
				Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
				Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs")
'Brapi.ErrorLog.LogMessage(si, "Here CM KL")				
				Dim Mode As String = args.NameValuePairs.XFGetValue("Mode")
			If Mode.XFEqualsIgnoreCase("Single") Then 

				req_IDs = req_IDs.Split(" ").Last()
			Else 
				req_IDs = req_IDs
				
			End If 
				
			Dim Req_ID_List As List (Of String) =  StringHelper.SplitString(req_IDs, ",")
'Brapi.ErrorLog.LogMessage(si, "Req_ID_List = " & Req_ID_List.Count)
			
			Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
			If String.IsNullOrWhiteSpace(new_Status) Then 

				Return dbExt_ChangedResult
			Else
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
					Dim sqa_xfc_cmd_SPLN_req = New SQA_XFC_CMD_SPLN_REQ(connection)
					Dim SQA_XFC_CMD_SPLN_REQ_DT = New DataTable()
					Dim sqa_xfc_cmd_SPLN_req_details = New SQA_XFC_CMD_SPLN_REQ_DETAILS(connection)
					Dim SQA_XFC_CMD_SPLN_REQ_DETAILS_DT = New DataTable()
					Dim sqa = New SqlDataAdapter()

				'Fill the DataTable With the current data From FMM_Dest_Cell
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name"
				
		
	    ' 2. Create a list to hold the parameters
	    Dim paramList As New List(Of SqlParameter) From {
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
   		}

    ' 3. Dynamically build the rest of the query and parameters
    If Req_ID_List.Count > 1 Then
        Dim paramNames As New List(Of String)
        For i As Integer = 0 To Req_ID_List.Count - 1
            Dim paramName As String = "@REQ_ID" & i
            paramNames.Add(paramName)
            paramList.Add(New SqlParameter(paramName, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
        Next
        sql &= $" AND REQ_ID IN ({String.Join(",", paramNames)})"
    ElseIf Req_ID_List.Count = 1 Then
        sql &= " AND REQ_ID = @REQ_ID"
        paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
    End If
'Brapi.ErrorLog.LogMessage(si,"SQL: " & sql)
    ' 4. Convert the list to the array your method expects
    Dim sqlparams As SqlParameter() = paramList.ToArray()
	
					sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DT, sql, sqlparams)
Brapi.ErrorLog.LogMessage(si, "Here CM KL after req fill")	
					' --- get list of parent IDs and select all detail rows in one query ---
					Dim parentIds As New List(Of String)()
					
					For Each parentRow As DataRow In SQA_XFC_CMD_SPLN_REQ_DT.Rows
						
						If Not IsDBNull(parentRow("CMD_SPLN_REQ_ID")) Then
							Dim columnValue As Object = parentRow("CMD_SPLN_REQ_ID")
            				Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
							
							parentIds.Add(reqIdAsGuid.ToString())
							
						End If
					Next

					If parentIds.Count > 0 Then
						' Build a comma separated list of ints and query details table with IN (...)
						' NOTE: parentIds are integers sourced from DB so this string concatenation is safe in this context.
						Dim idsCsv As String = String.Join(",", parentIds)
						sql = $"SELECT * 
								FROM XFC_CMD_SPLN_REQ_Details 
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_SPLN_REQ_ID IN  ({String.Join(",", parentIds.Select(Function(id, idx) $"@ID{idx}"))})"

					  Dim detailsParams As New List(Of SqlParameter) From {
							New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
							New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
							New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
						
						For i As Integer = 0 To parentIds.Count - 1
                        detailsParams.Add(New SqlParameter($"@ID{i}", SqlDbType.NVarChar) With {.Value = parentIds(i)})
                    	Next
'Brapi.ErrorLog.LogMessage(si, "Here CM KL before detail fill: " & sql)	
						sqa_xfc_cmd_SPLN_req_details.Fill_XFC_CMD_SPLN_REQ_DETAILS_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DETAILS_DT, sql, detailsParams.ToArray())
'Brapi.ErrorLog.LogMessage(si, "Here CM KL after detail fill")							
					End If
'Brapi.ErrorLog.LogMessage(si, "SQL: " & sql)
					' At this point detailsAllDT contains all matching XFC_CMD_SPLN_REQ_Details rows (if any).
					' Update all returned parent rows
					For Each row As DataRow In SQA_XFC_CMD_SPLN_REQ_DT.Rows
						Dim wfStepAllowed = Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Step_Allowed(si, args, curr_Status, row("Entity"))
						If wfStepAllowed = False Then
							dbExt_ChangedResult.ShowMessageBox = True
							If Not String.IsNullOrWhiteSpace(dbExt_ChangedResult.Message) Then
								dbExt_ChangedResult.Message &= Environment.NewLine
							End If
							dbExt_ChangedResult.Message &= $"Cannot change status of REQ_ID '{row("REQ_ID")}' at this time. Contact requirements manager."
						Else

							Dim existingStatus As String = ""
							If Not IsDBNull(row("Status")) Then existingStatus = row("Status").ToString().Trim()

							Dim lookupKey As String = existingStatus & "|" & new_Status
'brapi.ErrorLog.LogMessage(si,"lookupKey: " & lookupKey)
							Dim resolvedStatus As String
							If Status_manager.ContainsKey(lookupKey) Then
								resolvedStatus = Status_manager(lookupKey)
							Else
								resolvedStatus = existingStatus
								dbExt_ChangedResult.ShowMessageBox = True
								dbExt_ChangedResult.Message &= $"REQ_ID '{row("REQ_ID")}' has an incorrect status, can't be updated."
							End If
							If Not String.IsNullOrEmpty(demote_comment) Then
								row("Demotion_Comment") = demote_comment
							End If 
							row("Status") = resolvedStatus
							row("Update_User") = si.UserName
							row("Update_Date") = DateTime.Now
'Brapi.ErrorLog.LogMessage(si, "existingStatus | new_Status  |  resolvedStatus: " & existingStatus & "|" & new_Status &  " | " & resolvedStatus)
							' If we have details loaded, update the detail rows that belong to this parent now
							If SQA_XFC_CMD_SPLN_REQ_DETAILS_DT IsNot Nothing AndAlso SQA_XFC_CMD_SPLN_REQ_DETAILS_DT.Rows.Count > 0 AndAlso Not IsDBNull(row("CMD_SPLN_REQ_ID")) Then
								
								Dim pid As String = ""
								pid = row("CMD_SPLN_REQ_ID").ToString()
									Dim filterExpr As String = String.Format("CMD_SPLN_REQ_ID = '{0}'", pid)
									Dim matchingDetails() As DataRow = SQA_XFC_CMD_SPLN_REQ_DETAILS_DT.Select(filterExpr)
									
									For Each drow As DataRow In matchingDetails
						
										If Not FCList.Contains($"E#{drow("Entity")}")
											FCList.Add($"E#{drow("Entity")}")
										End If
										globals.SetStringValue($"FundsCenterStatusUpdates - {drow("Entity")}", $"{existingStatus}|{resolvedStatus}")

										drow("Flow") = resolvedStatus
										drow("Update_User") = si.UserName
										drow("Update_Date") = DateTime.Now
									Next
								'End If
							End If
							
'Brapi.ErrorLog.LogMessage(si, "Resolved Status" & resolvedStatus)

							'Me.UpdateStatusHistory(si, globals, api, args, newREQStatus, reqFlow, reqEntity)
							'Me.Send_Status_Change_Email(reqFlow, reqEntity)
						End If
					Next

					' Persist all changes back to the database
					
					sqa_xfc_cmd_SPLN_req.Update_XFC_CMD_SPLN_REQ(SQA_XFC_CMD_SPLN_REQ_DT, sqa)
					sqa_xfc_cmd_SPLN_req_details.Update_XFC_CMD_SPLN_REQ_DETAILS(SQA_XFC_CMD_SPLN_REQ_DETAILS_DT, sqa)
					End Using
				End If

				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
'Brapi.ErrorLog.LogMessage(si,"@HERE1" &String.Join(",",FCList))
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("EntList",String.Join(",",FCList))
				customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
				Dim currentYear As String = wfInfoDetails("TimeName")
				Dim nextyear As String = currentYear + 1
				customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
'Brapi.ErrorLog.LogMessage(si,"HERE2")
				Return dbExt_ChangedResult
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
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
							
'			'Call dataset BR to return a datatable that has been filtered by ufr status
'			Dim dt As DataTable = BR_REQDataset.Main(si, globals, api, dsArgs)
			
'			For Each row As DataRow In dt.Rows
'				Dim sEntity As String = row.Item("Value").Split(" ")(0)
'				sUFR = row.Item("Value").Split(" ")(1)				
									
'				Dim REQStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Status_History:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim REQCurrentStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
'				Dim StatusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQStatusHistoryMemberScript).DataCellEx.DataCellAnnotation
'				Dim CurrentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQCurrentStatusMemberScript).DataCellEx.DataCellAnnotation
'' BRApi.ErrorLog.LogMessage(si, $"REQCurrentStatusMemberScript= {REQCurrentStatusMemberScript} || CurrentStatus = {CurrentStatus}" )	
'				'Dim LastHistoricalStatus As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
'				Dim LastHistoricalEntry As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
'				Dim LastHistoricalStatus As String = LastHistoricalEntry.Substring(LastHistoricalEntry.LastIndexOf(":") + 1)
'				'If Not String.compare(sFundCenter & " " & CurrentStatus, LastHistoricalStatus) = 0 Then
'				If Not String.compare(CurrentStatus, LastHistoricalStatus) = 0 Then
'					'update Status History
'					Try
'						'Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, sUFR, sEntity, sFundCenter)
'					Catch ex As Exception
'					End Try
					
'					'Send email
'					Try
'	'BRApi.ErrorLog.LogMessage(si,"Here Manage UFR Statuses Updated ")					
'						'Me.Send_Status_Change_Email(sUFR, sFundCenter)
'					Catch ex As Exception
'					End Try
					
				
'				End If 
'			Next
			Return Nothing				
			
		End Function 

		
#End Region

#Region "Manage Requirements"
		
		Public Function Copy_REQ() As xfselectionchangedTaskResult
			Try
				'Get source scenrio and time from the passed in value
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				
				Dim srcSenario As String = args.NameValuePairs.XFGetValue("sourceScenario")
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim tm As String = wfInfoDetails("TimeName")
				Dim srcfundCenter = args.NameValuePairs.XFGetValue("sourceEntity")
				Dim SrcREQNameTilte = args.NameValuePairs.XFGetValue("SourceREQ")
				
				Dim srcREQName = String.Empty
				Dim srcREQ_ID As String = String.Empty
				If SrcREQNameTilte.XFContainsIgnoreCase("-") Then
					srcREQ_ID = SrcREQNameTilte.Split("-")(0).Trim
					srcREQName = SrcREQNameTilte.Split("-")(1).Trim
				Else
					Throw New Exception("The name and Id are not in the correct format: " & SrcREQNameTilte)	
				End If
				
				
				Dim REQDT As DataTable = New DataTable()
				Dim REQDetailDT As DataTable = New DataTable()
			
				'target REQ
				Dim trgtfundCenter = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim trgtScenario As String = wfInfoDetails("ScenarioName")
				
				Dim trgtREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim trgtFlow = "Test" 'newREQFlow
				Dim trgtEntity As String = GetEntity(si, args)
				
'BRApi.ErrorLog.LogMessage(si, "srcSenario: " & srcSenario & ", srcREQ_ID: " & srcREQ_ID & ", srcREQName: " & srcREQName & ", SourceREQNameTilte: " & SrcREQNameTilte & ", srcfundCenter: " & srcfundCenter & ", trgtfundCenter: " )	

				Dim sqa As New SqlDataAdapter()
				
				'select the current record into the sql adapter
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                sqlConn.Open()
		                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
		                Dim SqlREQ As String = $"SELECT * 
											FROM XFC_CMD_SPLN_REQ
											WHERE WFScenario_Name = '{srcSenario}'
											And WFCMD_Name = '{cmd}'
											AND WFTime_Name = '{tm}'
											AND REQ_ID = '{srcREQ_ID}'
											AND ENTITY = '{srcfundCenter}'"
		
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
		                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
'						REQDT.PrimaryKey = New DataColumn() {REQDT.Columns("CMD_SPLN_REQ_ID")}
						
						Dim origCMD_SPLN_REQ_ID As String
						If REQDT.Rows.Count = 1 Then
							origCMD_SPLN_REQ_ID  = REQDT.Rows(0)("CMD_SPLN_REQ_ID").ToString
						Else
							Throw New Exception("More than one record with the same ID " & srcREQ_ID & " returned.")
						End If
						
						Dim newREQRow As datarow = CMD_SPLN_Utilities.GetCopiedRow(si, REQDT.Rows(0))
						Me.UpdateCopyREQColumns(newREQRow)
						CMD_SPLN_Utilities.UpdateAuditColumns(si, newREQRow)

						Dim newCMD_SPLN_REQ_ID As String = newREQRow("CMD_SPLN_REQ_ID").ToString
						REQDT.Rows.Add(newREQRow)
						
						'Prepare Detail	
						 Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
						 Dim SqlREQDetail As String = $"SELECT * 
												FROM XFC_CMD_SPLN_REQ_Details
												WHERE WFScenario_Name = '{srcSenario}'
												And WFCMD_Name = '{cmd}'
												AND WFTime_Name = '{tm}'
						 						AND CMD_SPLN_REQ_ID = '{origCMD_SPLN_REQ_ID}'"
						 Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
		                sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						
						'create a new Data row for req and detail
						Dim copiedRows As New List(Of DataRow)
						For Each row As datarow In REQDetailDT.Rows
							Dim newREQDetailRow As datarow = CMD_SPLN_Utilities.GetCopiedRow(si, row)
							Me.UpdateCopyREQDetailColumns(newREQDetailRow, newCMD_SPLN_REQ_ID)
							CMD_SPLN_Utilities.UpdateAuditColumns(si, newREQDetailRow)
							
							copiedRows.Add(newREQDetailRow)
						Next
						
						For Each row As datarow In copiedRows
							REQDetailDT.Rows.Add(row)	
						Next
						
						'Copy the current req to the new ones
							
						
						sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
						sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)
			
						
					End Using	
				End Using
		
				Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function

		Public Sub UpdateCopyREQColumns(ByRef newRow As DataRow) 
			'update the columns
			Dim trgtfundCenter = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")

			Dim newREQ_ID As String= GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si, trgtfundCenter)
			newRow("REQ_ID") = newREQ_ID
'BRApi.ErrorLog.LogMessage(si,"Copied REQ_ID: " & newREQ_ID)			
			newRow("CMD_SPLN_REQ_ID") = Guid.NewGuid()
			newRow("Title") = args.NameValuePairs.XFGetValue("NewREQName")
			newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_SPLN"
			newRow("Entity") = trgtfundCenter
			
		End Sub
		
		Public Sub UpdateCopyREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_SPLN_REQ_ID As String) 
			'update the columns
			Dim trgtfundCenter = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			
			newRow("CMD_SPLN_REQ_ID") = newCMD_SPLN_REQ_ID
			newRow("Flow") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_SPLN"
			newRow("Entity") = trgtfundCenter
			
		End Sub

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
					'Me.UpdateStatusHistory(si, globals, api, args, newStatus)
				Catch ex As Exception
				End Try
				
				'Send email
				Try
					'Me.Send_Status_Change_Email()
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
				Throw New Exception("Please select a funds center.")
				Return Nothing
			End If
			If Not sMode.XFContainsIgnoreCase("copyREQ") And String.IsNullOrEmpty(sREQ) Then
				Throw New Exception("Please select a requirement.")
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
				
				Return selectionChangedTaskResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Helper Methods"	

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
		'Updated: EH RMW-1564 9/3/24 Updated to annual for SPLN_C20XX
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
			
			Dim sInfRateScript As String = "Cb#" & sCube & ":S#" & sScenario & ":T#" & iCurrScenarioYear.ToString & ":C#USD:E#"& sCube & "_General:V#Periodic:A#SPLN_Inflation_Rate_Amt:O#BeforeAdj:I#None:F#None" 
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

#Region "Set Related REQs"
Public Function SetRelatedREQs()
	Try
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("REQScenario")
			Dim sREQTime As String = args.NameValuePairs.XFGetValue("REQTime")

			Dim sRelatedREQs As String = args.NameValuePairs.XFGetValue("RelatedREQs")
			Dim REQList As New List(Of String)
			If Not String.IsNullOrWhiteSpace(sRelatedREQs) Then
    
    			REQList = sRelatedREQs.Split(","c).Select(Function(REQ) REQ.Trim()).ToList()
			End If

			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
				
           
					Dim sqa_xfc_cmd_SPLN_req = New SQA_XFC_CMD_SPLN_REQ(connection)
					Dim SQA_XFC_CMD_SPLN_REQ_DT = New DataTable()
					Dim sqa = New SqlDataAdapter()

				'Fill the DataTable With the current data From FMM_Dest_Cell
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
   		}

			
			
	
				sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DT, sql, sqlparams)
			
			
			Dim REQ_ID As String  = sREQ.Split(" "c)(1)
			Dim Mainreqrow As DataRow = SQA_XFC_CMD_SPLN_REQ_DT.Select($"REQ_ID ='{REQ_ID}'" ).FirstOrDefault()
			
		
			Dim mainREQ As String = ""
			
				mainREQ = $"{sREQ}"

			
				
			Dim currentREQs As String = String.Empty
			' Get the current list of related REQs before we overwrite it.
			Dim oldRelatedREQsString As String = Mainreqrow("Related_REQs").ToString()
			Dim oldList As List(Of String) = oldRelatedREQsString.Split(","c).Select(Function(r) r.Trim()).Where(Function(r) Not String.IsNullOrWhiteSpace(r)).ToList()
			Dim newList As New List(Of String)
			For Each REQ As String In REQList
				If REQ.XFContainsIgnoreCase(mainREQ) Then Continue For
				Dim isGeneral As Boolean = REQ.Split(" ")(0).Trim().XFContainsIgnoreCase("_General")
				'Brapi.ErrorLog.LogMessage()
				If isGeneral Then REQ = REQ.Replace(REQ.Split(" ")(0).Trim(),REQ.Split(" ")(0).Trim().Replace("_General",""))
				If (Not newList.Any(Function(r) r.Equals(REQ, StringComparison.OrdinalIgnoreCase))) Then
					newList.Add(REQ)
				End If
			Next

	'Set Related REQS
			
						Mainreqrow("Related_REQs") = String.Join(", ", newList)
						Mainreqrow("Update_User") = si.UserName
						Mainreqrow("Update_Date") = DateTime.Now

				
			
	'Update the other side - Set the related REQs From REQ list
			For Each relatedREQ In newList
				'Brapi.ErrorLog.LogMessage(si, "relatedREQ" & relatedREQ)
				If relatedREQ.XFContainsIgnoreCase(mainREQ) Then Continue For
				Dim REQ = relatedREQ.Split(" ")(1).Trim()
				sEntity = relatedREQ.Split(" ")(0).Trim()
				'Brapi.ErrorLog.LogMessage(si, "REQ" & REQ)
				'Brapi.ErrorLog.LogMessage(si, "Entity" & sEntity)
				Dim RelatedREQsList As DataRow = SQA_XFC_CMD_SPLN_REQ_DT.Select($"REQ_ID ='{REQ}'" ).FirstOrDefault()
				If RelatedREQsList Is Nothing Then Continue For ' Safety check
					currentREQs = RelatedREQsList("Related_REQs").ToString()
					
				If (Not currentREQs.XFContainsIgnoreCase(mainREQ)) Then
					If (String.IsNullOrWhiteSpace(currentREQs)) Then
						currentREQs = mainREQ
					Else
						currentREQs = currentREQs & ", " & mainREQ	
					End If
				End If
					'For Each drow As DataRow In RelatedREQsList
						RelatedREQsList("Related_REQs") = currentREQs
						RelatedREQsList("Update_User") = si.UserName
						RelatedREQsList("Update_Date") = DateTime.Now
				'Next
				
			Next
' Find REQs that were removed
			Dim ToRemove = oldList.Except(newList, StringComparer.OrdinalIgnoreCase).ToList()
			
			'Update the other side - REMOVE mainREQ from old partners
			For Each relatedREQtoRemove In ToRemove
				If relatedREQtoRemove.XFContainsIgnoreCase(mainREQ) Then Continue For
				Dim REQ = relatedREQtoRemove.Split(" ")(1).Trim()
				sEntity = relatedREQtoRemove.Split(" ")(0).Trim()
				
				Dim RelatedREQsList As DataRow = SQA_XFC_CMD_SPLN_REQ_DT.Select($"REQ_ID ='{REQ}'" ).FirstOrDefault()
				If RelatedREQsList Is Nothing Then Continue For ' Safety check
				
				currentREQs = RelatedREQsList("Related_REQs").ToString()
				
				' If the list contains the main REQ, remove it
				If (currentREQs.XFContainsIgnoreCase(mainREQ)) Then
					' Rebuild the list without mainREQ
					Dim CurrentList As List(Of String) = currentREQs.Split(","c).Select(Function(r) r.Trim()).Where(Function(r) Not String.IsNullOrWhiteSpace(r)).ToList()
					CurrentList.RemoveAll(Function(r) r.Equals(mainREQ, StringComparison.OrdinalIgnoreCase))
					
					' Join it back together
					RelatedREQsList("Related_REQs") = String.Join(", ",CurrentList)
					RelatedREQsList("Update_User") = si.UserName
					RelatedREQsList("Update_Date") = DateTime.Now
				End If
			Next

				
			
			sqa_xfc_cmd_SPLN_req.Update_XFC_CMD_SPLN_REQ(SQA_XFC_CMD_SPLN_REQ_DT,sqa)
		End Using
			
			Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

#End Region

#Region "Create CivPay REQ"

			Public Function CreateCivPayREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
'BRApi.ErrorLog.LogMessage(si,"In Test ..")				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
	
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim tm As String = wfInfoDetails("TimeName")
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
				Dim REQDT As DataTable = New DataTable()
				Dim REQDetailDT As DataTable = New DataTable()
				Dim sqa As New SqlDataAdapter()
				
				'Check if Fundcenter or APPN dropdown is empty
				Dim return_message As String = ""
				If String.IsNullOrWhiteSpace(sEntity) OrElse String.IsNullOrWhiteSpace(SAPPN) Then
					return_message = "Please select a FundCenter and Appropriation to create a requirement"
					Throw New XFUserMsgException(si, New Exception(return_message))
				End If
				
				'Establish SQL DB call
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                sqlConn.Open()
						'Main
		                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
		                Dim SqlREQ As String = $"SELECT * 
											FROM XFC_CMD_SPLN_REQ
											WHERE WFCMD_Name = '{cmd}'
											And WFTime_Name = '{tm}'
											AND WFScenario_Name = '{sScenario}'
											AND ENTITY = '{sEntity}'
											AND REQ_ID LIKE '%{sAPPN}%'
						 					AND REQ_ID_Type = 'CivPay'"
		
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
		                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ) 
BRApi.ErrorLog.LogMessage(si,"SQL: " & SqlREQ)

						Dim row As DataRow
						Dim req_ID_Val As Guid
						Dim REQ_ID As String = ""
						Dim isInsert As Boolean = True				
						
						If Not REQDT.Rows.Count = 0 Then
BRApi.ErrorLog.LogMessage(si,"in if id check")					
							Dim return_message_ID_Check As String = ""
							'return_message_ID_Check = "CivPay requirement has already been created for " + sEntity + " - " + sAPPN
							return_message_ID_Check = "The CivPay Requirement for Appropriation " + sAPPN +" and Funds Center " + sEntity + " has already been created"
							Throw New XFUserMsgException(si, New Exception(return_message_ID_Check))
'							req_ID_Val = reqdt.Rows(0).Item("CMD_SPLN_REQ_ID")
'							req_id = reqdt.Rows(0).Item("REQ_ID")
'		                    isInsert = False
'		                    row = reqdt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
							
						 Else 
BRApi.ErrorLog.LogMessage(si,"in else id check")		
							row = REQDT.NewRow()
							req_ID_Val = Guid.NewGuid
							Dim modifiedFC As String = sEntity
							modifiedFC = modifiedFC.Replace("_General", "")
							If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
							req_id = modifiedFC + "_CP_" + sAPPN + "_00001"
							'req_id = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,sEntity)
'brapi.ErrorLog.LogMessage(si,req_id) 
						End If 
						
						row("WFScenario_Name") = sScenario
						row("WFTime_Name") = tm
						row("Title") =  sAPPN + " CivPay Requirement"
						row("Entity") = sEntity
						row("CMD_SPLN_REQ_ID") = req_ID_Val
						row("WFCMD_Name") = sCube
						row("REQ_ID_Type") = "CivPay"
						row("REQ_ID") = req_id
						row("Description") = "CivPay Requirement"
						row("APPN") = "None"
						row("MDEP") = "None"
						row("APE9") = "None"
						row("Dollar_Type") = "None"
						row("Obj_Class") = "None"
						row("CType") = "None"
						row("UIC") = "None"
						row("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity) & "_Formulate_SPLN"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
						
						If isInsert Then
'brapi.ErrorLog.LogMessage(si,"Inside if isinsert ") 
							REQDT.Rows.Add(row)
						End If
						
						globals.SetObject("REQ_ID_VAL",req_ID_Val)
						sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
						
						'Writting to Cube
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
						Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
						params.Add("APPN",sAPPN)
						params.Add("REQ_ID_VAL",REQ_ID_VAL.ToString)
						params.Add("Entity",sEntity)
						'BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Manpower", Params)
						
						Dim customSubstVars As New Dictionary(Of String, String) 
						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_SPLN|L2_Formulate_SPLN")
						customSubstVars.Add("EntList","E#" & sEntity)					
						customSubstVars.Add("WFScen",sScenario)
						Dim currentYear As Integer = Convert.ToInt32(tm)
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
				
						End Using 
					End Using
					
				Return Nothing
			End Function		

#End Region

#Region "Create Withhold REQ"

			Public Function CreateWithholdREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
'BRApi.ErrorLog.LogMessage(si,"In Test ..")				
					Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
	
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim tm As String = wfInfoDetails("TimeName")
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
				Dim REQDT As DataTable = New DataTable()
				Dim REQDetailDT As DataTable = New DataTable()
				Dim sqa As New SqlDataAdapter()
				
				'Check if Fundcenter or APPN dropdown is empty
				Dim return_message As String = ""
				If String.IsNullOrWhiteSpace(sEntity) OrElse String.IsNullOrWhiteSpace(SAPPN) Then
					return_message = "Please select a FundCenter and Appropriation to create a requirement"
					Throw New XFUserMsgException(si, New Exception(return_message))
				End If
				
				'Establish SQL DB call
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                sqlConn.Open()
						'Main
		                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
		                Dim SqlREQ As String = $"SELECT * 
											FROM XFC_CMD_SPLN_REQ
											WHERE WFCMD_Name = '{cmd}'
											And WFTime_Name = '{tm}'
											AND WFScenario_Name = '{sScenario}'
											AND ENTITY = '{sEntity}'
											AND REQ_ID LIKE '%{sAPPN}%'
						 					AND REQ_ID_Type = 'Withhold'"
		
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
		                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ) 
BRApi.ErrorLog.LogMessage(si,"SQL: " & SqlREQ)

						Dim row As DataRow
						Dim req_ID_Val As Guid
						Dim REQ_ID As String = ""
						Dim isInsert As Boolean = True				
						
						If Not REQDT.Rows.Count = 0 Then
BRApi.ErrorLog.LogMessage(si,"in if id check")					
							Dim return_message_ID_Check As String = ""
							'return_message_ID_Check = "CivPay requirement has already been created for " + sEntity + " - " + sAPPN
							return_message_ID_Check = "The Withhold Requirement for Appropriation " + sAPPN +" and Funds Center " + sEntity + " has already been created"
							Throw New XFUserMsgException(si, New Exception(return_message_ID_Check))
'							req_ID_Val = reqdt.Rows(0).Item("CMD_SPLN_REQ_ID")
'							req_id = reqdt.Rows(0).Item("REQ_ID")
'		                    isInsert = False
'		                    row = reqdt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
							
						 Else 
BRApi.ErrorLog.LogMessage(si,"in else id check")		
							row = REQDT.NewRow()
							req_ID_Val = Guid.NewGuid
							Dim modifiedFC As String = sEntity
							modifiedFC = modifiedFC.Replace("_General", "")
							If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
							req_id = modifiedFC + "_CP_" + sAPPN + "_00001"
							'req_id = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,sEntity)
'brapi.ErrorLog.LogMessage(si,req_id) 
						End If 
						
						row("WFScenario_Name") = sScenario
						row("WFTime_Name") = tm
						row("Title") =  sAPPN + " Withhold Requirement"
						row("Entity") = sEntity
						row("CMD_SPLN_REQ_ID") = req_ID_Val
						row("WFCMD_Name") = sCube
						row("REQ_ID_Type") = "Withhold"
						row("REQ_ID") = req_id
						row("Description") = "Withhold Requirement"
						row("APPN") = "None"
						row("MDEP") = "None"
						row("APE9") = "None"
						row("Dollar_Type") = "None"
						row("Obj_Class") = "None"
						row("CType") = "None"
						row("UIC") = "None"
						row("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity) & "_Formulate_SPLN"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
						
						If isInsert Then
'brapi.ErrorLog.LogMessage(si,"Inside if isinsert ") 
							REQDT.Rows.Add(row)
						End If
						
						globals.SetObject("REQ_ID_VAL",req_ID_Val)
						sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
						
						'Writting to Cube
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
						Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
						params.Add("APPN",sAPPN)
						params.Add("REQ_ID_VAL",REQ_ID_VAL.ToString)
						params.Add("Entity",sEntity)
						'BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Manpower", Params)
						
						Dim customSubstVars As New Dictionary(Of String, String) 
						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_SPLN|L2_Formulate_SPLN")
						customSubstVars.Add("EntList","E#" & sEntity)					
						customSubstVars.Add("WFScen",sScenario)
						Dim currentYear As Integer = Convert.ToInt32(tm)
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
				
						
					End Using 
					End Using
					
				Return Nothing
			End Function		

#End Region

#Region "Save Adjust Funding Line "		
Public Function Save_AdjustFundingLine() As xfselectionchangedTaskResult
Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs")
	req_IDs = req_IDs.Split(" ").Last()
	'Brapi.ErrorLog.LogMessage(si, "HERE 2")
		Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
		'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_SPLN_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_SPLN_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND REQ_ID  = @REQ_ID"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
				New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = req_IDs}
						}
              sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
			' --- Details Table (XFC_CMD_SPLN_REQ_Details) ---
            Dim dt_Details As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
			Dim parentIds As New List(Of String)()

For Each parentRow As DataRow In dt.Rows
   
    If Not IsDBNull(parentRow("CMD_SPLN_REQ_ID")) Then
        ' Get the CMD_SPLN_REQ_ID value and parse it as Guid
        Dim columnValue As Object = parentRow("CMD_SPLN_REQ_ID")
        Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
      
        parentIds.Add(reqIdAsGuid.ToString())
    End If
Next	
    ' Select a single CMD_SPLN_REQ_ID (assuming the requirement is only one ID for the query)
    		Dim singleCMD_SPLN_REQ_ID As String = parentIds(0)
Dim sql As String = ""
    ' Build the SQL query with the single CMD_SPLN_REQ_ID
    	sql = $"SELECT *
           FROM XFC_CMD_SPLN_REQ_Details
           WHERE WFScenario_Name = @WFScenario_Name
             AND WFCMD_Name = @WFCMD_Name
             AND WFTime_Name = @WFTime_Name
             AND CMD_SPLN_REQ_ID = @SingleCMD_SPLN_REQ_ID"

    ' Create the list of SQL parameters
    Dim detailsParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
        New SqlParameter("@SingleCMD_SPLN_REQ_ID", SqlDbType.NVarChar) With {.Value = singleCMD_SPLN_REQ_ID}
    }

    ' Fill the details table with the query results
    sqaReaderdetail.Fill_XFC_CMD_SPLN_REQ_DETAILS_DT(sqa2, dt_Details, sql, detailsParams)
			  

            ' ************************************
            ' ************************************
	
    Dim targetRow As DataRow 											
	
	
			
			targetRow = dt.Select($"REQ_ID = '{req_IDs}'").FirstOrDefault()
			Dim APPN As String =  args.NameValuePairs.XFGetValue("APPN")
		
			'Brapi.ErrorLog.LogMessage(si, "APPN " & APPN)	
			
			Dim Entity As String =  args.NameValuePairs.XFGetValue("Entity")
			Dim MDEP As String =  args.NameValuePairs.XFGetValue("MDEP")
			 Dim APE As String =  args.NameValuePairs.XFGetValue("APEPT")
		     Dim DollarType As String =  args.NameValuePairs.XFGetValue("DollarType")
		 	 Dim cTypecol As String =  args.NameValuePairs.XFGetValue("CType")
			 Dim obj_class As String =  args.NameValuePairs.XFGetValue("Obj_Class")
			Dim Status As String  = targetRow("Status")
			Dim Create_User As String = targetRow("Create_User")
			
			 If Not String.IsNullOrWhiteSpace(APPN) Then
			 	targetRow("APPN") = APPN
			End If 
			If Not String.IsNullOrWhiteSpace(MDEP) Then
			    targetRow("MDEP") = MDEP
			End If
			If Not String.IsNullOrWhiteSpace(APE) Then
			    targetRow("APE9") = APE
			End If
			If Not String.IsNullOrWhiteSpace(DollarType) Then
			    targetRow("Dollar_Type") = DollarType
			End If
			If Not String.IsNullOrWhiteSpace(obj_class) Then
			    targetRow("Obj_Class") = obj_class
			End If
			If Not String.IsNullOrWhiteSpace(cTypecol) Then
			    targetRow("CType") = cTypecol
			End If
			
				targetRow("Update_Date") = DateTime.Now
				targetRow("Update_User") = si.UserName																																																																																				
Dim req_ID_Val As Guid
	req_ID_Val = targetRow("CMD_SPLN_REQ_ID") 
	
	Dim targetRowFYValues As DataRow
			targetRowFYValues = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
			Dim FY1 As Decimal =  targetRowFYValues("FY_1")		 
			Dim FY2 As Decimal = targetRowFYValues("FY_2")	
			Dim FY3 As Decimal = targetRowFYValues("FY_3")
		    Dim FY4 As Decimal = targetRowFYValues("FY_4")	
		 	Dim FY5 As Decimal = targetRowFYValues("FY_5")
				
	
							
						
	
		Dim targetRowFunding As DataRow
			targetRowFunding = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
			
			If targetRowFunding IsNot Nothing Then
				targetRowFunding.Delete
   				 
				
			End If
		
			Dim targetRowFundingNew As DataRow = dt_Details.NewRow()
				targetRowFundingNew("CMD_SPLN_REQ_ID") = req_ID_Val
							targetRowFundingNew("WFScenario_Name") = wfInfoDetails("ScenarioName")
							targetRowFundingNew("WFCMD_Name") = wfInfoDetails("CMDName")
							targetRowFundingNew("WFTime_Name") = wfInfoDetails("TimeName")
							targetRowFundingNew("Entity") = Entity
							targetRowFundingNew("IC") = "None"
							targetRowFundingNew("Account") = "Req_Funding"
							targetRowFundingNew("Unit_of_Measure") = "Funding"
							targetRowFundingNew("Flow") = targetRow("Status")
								
			If Not String.IsNullOrWhiteSpace(APPN) Then
			 	targetRowFundingNew("UD1") = APPN
			Else 
				targetRowFundingNew("UD1") = targetRow("APPN")
			End If 
			If Not String.IsNullOrWhiteSpace(MDEP) Then
			    targetRowFundingNew("UD2") = MDEP
			Else 
				 targetRowFundingNew("UD2") =  targetRow("MDEP")
			End If
			If Not String.IsNullOrWhiteSpace(APE) Then
			   targetRowFundingNew("UD3") = APE
		   Else 
			   targetRowFundingNew("UD3")  =  targetRow("APE9")
			End If
			If Not String.IsNullOrWhiteSpace(DollarType) Then
			    targetRowFundingNew("UD4") = DollarType
			Else 
				 targetRowFundingNew("UD4") =  targetRow("Dollar_Type")
			End If
			If Not String.IsNullOrWhiteSpace(obj_class) Then
			   targetRowFundingNew("UD6") = obj_class
		   Else 
			     targetRowFundingNew("UD6") = targetRow("Obj_Class")
			End If
			If Not String.IsNullOrWhiteSpace(cTypecol) Then
			   targetRowFundingNew("UD5") = cTypecol
		   Else 
			    targetRowFundingNew("UD5") = targetRow("CType")
			End If	
			
			targetRowFundingNew("UD7") = "None"
			targetRowFundingNew("UD8") = "None"
			targetRowFundingNew("Start_Year") = wfInfoDetails("TimeName")			
							targetRowFundingNew("FY_1") = FY1
							targetRowFundingNew("FY_2") = FY2
							targetRowFundingNew("FY_3") = FY3
							targetRowFundingNew("FY_4") = FY4
							targetRowFundingNew("FY_5") = FY5
						
							targetRowFundingNew("AllowUpdate") = "True"
							targetRowFundingNew("Create_Date") = targetRow("Create_Date")
	                        targetRowFundingNew("Create_User") = targetRow("Create_User") 
							targetRowFundingNew("Update_Date") = DateTime.Now
	                        targetRowFundingNew("Update_User") = si.UserName  
		                   dt_Details.Rows.Add(targetRowFundingNew)
						   
		              
	
		                ' Persist changes back to the DB using the configured adapter
		               
		               sqaReaderdetail.Update_XFC_CMD_SPLN_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
						
						
						
		                End Using
		            End Using
Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

	'Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo 
		'objXFSelectionChangedUIActionInfo.DashboardsToRedraw= ""
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


End Function

		
#End Region

#Region "saveweightprioritization"
Public Function saveweightprioritization() As Object
	
	Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
	Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
	Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
	Dim sEntity As String = args.NameValuePairs.XFGetValue("entity")
    Dim sWeightPrioritizationMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#RMW_Cycle_Config_Annual:T#" & sTime &":V#Periodic:A#GBL_Priority_Cat_Weight:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	Dim TotPct As Double = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sWeightPrioritizationMbrScript).DataCellEx.DataCell.CellAmount
'brapi.ErrorLog.Logmessage(si, $"entity: {sEntity} | TotPct: {TotPct} | mbrscript: {sWeightPrioritizationMbrScript}")	
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

#Region"load_req_detailsdashboard"
Public Function load_req_detailsdashboard() As XFLoadDashboardTaskResult
	Dim LoadDBTaskResult As New XFLoadDashboardTaskResult()
	LoadDBTaskResult.ChangeCustomSubstVarsInDashboard = True
	
	Dim reqTitle = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleList")
	
	If reqTitle <> String.Empty Or Not String.IsNullOrEmpty(reqTitle)
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_SPLN_REQDetailsShowHide","CMD_SPLN_0_Body_REQDetailsMain")
	Else	
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_SPLN_REQDetailsShowHide","CMD_SPLN_0_Body_REQDetailsHide")
	End If
	Return LoadDBTaskResult
End Function
#End Region

#Region "Delete Requirement ID"
		Public Function DeleteRequirementID() As XFSelectionChangedTaskResult
			Try
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				' Get list of REQ IDs from args and split into list
				Dim req_IDsRaw As String = args.NameValuePairs.XFGetValue("req_IDs", "")
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "")
				If String.IsNullOrEmpty(sEntity)
					sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				End If
				Dim tm As String = wfInfoDetails("TimeName")
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'brapi.ErrorLog.LogMessage(si,"sEntity = " & sEntity)
				' Build a cleaned list of REQ IDs (handle single value, comma separated, or space-delimited passed-in values)
				Dim Req_ID_List As List(Of String) = New List(Of String)()
				Dim raw As String = req_IDsRaw.Trim()

				If String.IsNullOrWhiteSpace(raw) Then
					Req_ID_List = New List(Of String)()
				ElseIf raw.Contains(",") Then
					Req_ID_List = StringHelper.SplitString(raw, ",").Select(Function(s) s.Trim()).Where(Function(s) Not String.IsNullOrWhiteSpace(s)).ToList()
				ElseIf raw.Contains(" "c) Then
					' If a single REQ was passed as part of a space-delimited string, take the last token (matches other usages)
					Req_ID_List.Add(raw.Split(" "c).Last().Trim())
				Else
					Req_ID_List.Add(raw)
				End If
'brapi.ErrorLog.LogMessage(si,"here1")
				' Remove duplicates if any
				Req_ID_List = Req_ID_List.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
				' Prepare datatables to be filled and fill them from DB
				Dim REQDT As New DataTable()
				Dim REQDetailDT As New DataTable()
				Dim Status As String = ""
				Dim StatusList As String = ""
				
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)

				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
					Dim sqa As New SqlDataAdapter()

					' Fill main REQ table
					Dim sqa_xfc_cmd_SPLN_req = New SQA_XFC_CMD_SPLN_REQ(connection)
					Dim sql As String = $"SELECT * 
										FROM XFC_CMD_SPLN_REQ 
										WHERE WFScenario_Name = @WFScenario_Name
										AND WFCMD_Name = @WFCMD_Name
										AND WFTime_Name = @WFTime_Name"

					Dim paramList As New List(Of SqlParameter) From {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
					}
'brapi.ErrorLog.LogMessage(si,"here2")
					Dim sqa_xfc_cmd_SPLN_req_details = New SQA_XFC_CMD_SPLN_REQ_Details(connection)

					' Build details SQL using CMD_SPLN_REQ_ID IN (...) so both queries filter by the same parent identifiers
					Dim detailSql As String = $"SELECT * 
												FROM XFC_CMD_SPLN_REQ_Details AS Req
       											LEFT JOIN XFC_CMD_SPLN_REQ AS Dtl
       											ON Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
												WHERE Req.WFScenario_Name = @WFScenario_Name
												AND Req.WFCMD_Name = @WFCMD_Name
												AND Req.WFTime_Name = @WFTime_Name"

					Dim detailParamList As New List(Of SqlParameter) From {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
					}
'brapi.ErrorLog.LogMessage(si,"here3")
					If Req_ID_List IsNot Nothing AndAlso Req_ID_List.Count > 0 Then
						If Req_ID_List.Count = 1 Then
							sql &= " AND REQ_ID = @REQ_ID"
							detailSql &= " AND Dtl.REQ_ID = @REQ_ID"
							paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
							detailParamList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
						Else
							Dim paramNames As New List(Of String)
							For i As Integer = 0 To Req_ID_List.Count - 1
								Dim pname As String = "@REQ_ID" & i
								paramNames.Add(pname)
								paramList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
								detailParamList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
							Next
							sql &= $" AND REQ_ID IN ({String.Join(",", paramNames)})"
							detailSql &= $" AND Dtl.REQ_ID IN ({String.Join(",", paramNames)})"
						End If
					End If
'brapi.ErrorLog.LogMessage(si,"here4")
					sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, sql, paramList.ToArray())
					sqa_xfc_cmd_SPLN_req_details.Fill_XFC_CMD_SPLN_REQ_DETAILS_DT(sqa, REQDetailDT, detailSql, detailParamList.ToArray())

					' Mark rows for deletion in both tables
					Dim entitiesFromReqList As New List(Of String)
					For Each reqId As String In Req_ID_List
						Dim rows As DataRow() = REQDT.Select($"REQ_ID = '{reqId}'")
						Dim GUIDROW As String = rows.FirstOrDefault().Item("CMD_SPLN_REQ_ID").tostring
						Dim ent As String = rows.FirstOrDefault().Item("Entity").ToString
						entitiesFromReqList.Add(ent)
						Status  = rows.FirstOrDefault().Item("Status").tostring
						If Status.XFEqualsIgnoreCase("") Then
							StatusList = Status
						Else
							If Not statuslist.XFContainsIgnoreCase(Status) Then
								statuslist += StatusList & "|" & Status
							End If
						End If 
						For Each row As DataRow In rows
							row.Delete()
						Next
'brapi.ErrorLog.LogMessage(si,"here5")
						Dim detailRows As DataRow() = REQDetailDT.Select($"CMD_SPLN_REQ_ID = '{GUIDROW}'")
						For Each drow As DataRow In detailRows
							drow.Delete()
						Next
					Next
					' Persist changes back to DB
					sqa_xfc_cmd_SPLN_req_details.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)
					sqa_xfc_cmd_SPLN_req.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
					
					'clear cube
'brapi.ErrorLog.LogMessage(si,"here6")					
					Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
					Dim customSubstVars As New Dictionary(Of String, String) 
					globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", statuslist)
					If Not String.IsNullOrWhiteSpace(sEntity) Then
						customSubstVars.Add("EntList","E#" & sEntity)
					Else
'BRApi.ErrorLog.LogMessage(si, String.Join(",", entitiesFromReqList.Select(Function (r) "E#" & r)) & ", statuslist = " & statuslist)						
						Dim entities As String
						entitiesFromReqList = entitiesFromReqList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
						entities = String.Join(",", entitiesFromReqList.Select(Function (r) "E#" & r))
						customSubstVars.Add("EntList", entities)
					End If
					customSubstVars.Add("WFScen", sScenario)
					Dim currentYear As Integer = Convert.ToInt32(tm)
					Dim nextyear As String = currentYear + 1
					customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
					

				End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

#End Region

#Region "UpdateCustomSubstVar"
		Private Sub UpdateCustomSubstVar(ByRef Result As XFLoadDashboardTaskResult,ByVal key As String,ByVal value As String)
			If Result.ModifiedCustomSubstVars.ContainsKey(key)
				Result.ModifiedCustomSubstVars.XFSetValue(key,value)
			Else
				Result.ModifiedCustomSubstVars.Add(key,value)
			End If
			
		End Sub
#End Region		

#Region "Attach Document"
Public Function AttachDocument()
		Try
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
				 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()
				 Dim dt As New DataTable()
				Dim sqa As New SqlDataAdapter()
				Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
				
				Dim ReqID As String = args.NameValuePairs.XFGetValue("REQ_ID","")
				If String.IsNullOrWhiteSpace(ReqID)
					Return Nothing
				Else 
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
					
	Brapi.ErrorLog.LogMessage(si,"REQ" & RequirementID)

				'Fill the DataTable 
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name
									AND REQ_ID  = @REQ_ID"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
		New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = RequirementID}
   		}
			sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa,dt, sql, sqlparams)
			
			Dim sREQ_ID_Val As String = String.Empty
			Dim REQ_ID_Val_guid As Guid = Guid.Empty
			
		If dt.Rows.Count > 0 Then
    sREQ_ID_Val = Convert.ToString(dt.Rows(0)("CMD_SPLN_REQ_ID"))
    REQ_ID_Val_guid = sREQ_ID_Val.XFConvertToGuid
Else
    
    Return Nothing 
End If
			
			
		  Dim dt_Attachment As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderAttachment As New SQA_XFC_CMD_SPLN_REQ_Attachment(sqlConn)
            Dim sqlAttach As String = $"SELECT * FROM XFC_CMD_SPLN_REQ_Attachment Where CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID"
		Dim sqlParamsAttach As SqlParameter() = New SqlParameter(){
			New SqlParameter("@CMD_SPLN_REQ_ID", SqlDbType.uniqueidentifier) With {.Value = REQ_ID_Val_guid}
			}
			  sqaReaderAttachment.Fill_XFC_CMD_SPLN_REQ_Attachment_DT(sqa2, dt_Attachment, sqlAttach, sqlParamsAttach)
			 
			  Dim Tatgetrow As DataRow
			  
			  'If dt_Attachment.rows.count.Equals(0) Then
		
			  Tatgetrow = dt_Attachment.NewRow()
			  dt_Attachment.Rows.Add(Tatgetrow)
'			  Else
			  
'			  Tatgetrow = dt_Attachment.Select($"CMD_SPLN_REQ_ID = '{REQ_ID_Val_guid}'").FirstOrDefault()
'			  End If
			  
		Tatgetrow("CMD_SPLN_REQ_ID") = REQ_ID_Val_guid
			
		Dim FilePath As String = args.NameValuePairs.XFGetValue("FilePath")
		Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
		
		Tatgetrow("Attach_File_Name") = fileInfo.XFFile.FileInfo.Name
		Tatgetrow("Attach_File_Bytes") = fileInfo.XFFile.ContentFileBytes
		
		
		
		sqaReaderAttachment.Update_XFC_CMD_SPLN_REQ_Attachment(dt_Attachment,sqa2)
		
		
		'Delete the Loaded File
BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, FilePath)
		
		
End If
	End Using
End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
#End Region

#Region "Download Document"
Public Function DownloadDocument()
Try


Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")		
Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(sREQ, " ")	
Dim RequirementID As String  = REQ_ID_Split(1)
Dim FileName As String  = args.NameValuePairs.XFGetValue("File")
Dim File_Name_List As List(Of String) = StringHelper.SplitString(FileName, ",")



	 
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()
			Dim dt As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderAttachment As New SQA_XFC_CMD_SPLN_REQ_Attachment(sqlConn)
            Dim sqlAttach As String = $"SELECT * From XFC_CMD_SPLN_REQ_Attachment as Att
	   						LEFT JOIN XFC_CMD_SPLN_REQ AS Req
							ON Req.CMD_SPLN_REQ_ID = Att.CMD_SPLN_REQ_ID
							WHERE 
	  					 	REQ_ID = @REQ_ID
							"
		
			
	Dim paramList As New List(Of SqlParameter)
        paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = RequirementID})
		
		If File_Name_List.Count > 1 Then
            Dim paramNames As New List(Of String)
            For i As Integer = 0 To File_Name_List.Count - 1
                Dim paramName As String = "@Attach_File_Name" & i
                paramNames.Add(paramName)
                paramList.Add(New SqlParameter(paramName, SqlDbType.NVarChar) With {.Value = File_Name_List(i)})
            Next
            sqlAttach &= $" AND Att.Attach_File_Name IN ({String.Join(",", paramNames)})"
        ElseIf File_Name_List.Count = 1 Then
            sqlAttach &= " AND Att.Attach_File_Name = @Attach_File_Name"
            paramList.Add(New SqlParameter("@Attach_File_Name", SqlDbType.NVarChar) With {.Value = File_Name_List(0)})
        End If
        
        Dim sqlParamsAttach As SqlParameter() = paramList.ToArray()
		sqaReaderAttachment.Fill_XFC_CMD_SPLN_REQ_Attachment_DT(sqa2, dt, sqlAttach, sqlParamsAttach)
						
		If dt.Rows.Count = 0 Then
            Return New XFSelectionChangedTaskResult() With {.IsOK = True, .ShowMessageBox = True, .Message = "No documents found for the selected Requirement."}
        End If		

		Dim sFolderPath As String = "Documents/Users/" & si.UserName
        Dim sFilePath As String
        Dim fileBytes As Byte()
        Dim sFileName As String
		
		
If dt.Rows.Count = 1 Then
            ' --- SINGLE FILE LOGIC ---
            Dim row As DataRow = dt.Rows(0)
            
           
            sFileName = row.Field(Of String)("Attach_File_Name")
            fileBytes = row.Field(Of Byte())("Attach_File_Bytes")
            
            sFilePath = $"{sFolderPath}/{sFileName}"

        Else	
				
            ' --- MULTIPLE FILE LOGIC (ZIP) ---
            sFileName = $"Attachments_{RequirementID}.zip"
            sFilePath = $"{sFolderPath}/{sFileName}"

            Using memStream As New MemoryStream()
                ' Create a zip archive in memory
                Using zip As New ZipArchive(memStream, ZipArchiveMode.Create, True)
                    For Each row As DataRow In dt.Rows
                        
                        Dim entryFileName As String = row.Field(Of String)("Attach_File_Name")
                        Dim entryFileBytes As Byte() = row.Field(Of Byte())("Attach_File_Bytes")

                        ' Add the file to the zip
                        Dim zipEntry = zip.CreateEntry(entryFileName)
                        Using entryStream As Stream = zipEntry.Open()
                            entryStream.Write(entryFileBytes, 0, entryFileBytes.Length)
                        End Using
                    Next
                End Using 
                
                ' Get the complete zip file as a byte array
                fileBytes = memStream.ToArray()
            End Using
        End If

      ' Save the (single or zip) file to the user's temp folder
        Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, sFilePath)
        Dim objXFFile As New XFFile(objXFFileInfo, String.Empty, fileBytes)
        BRApi.FileSystem.InsertOrUpdateFile(si, objXFFile)

        'Create and return the task to open/download the file
        Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
        selectionChangedTaskResult.IsOK = True
        selectionChangedTaskResult.ShowMessageBox = False 
        selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
        selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
        selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"

        Return selectionChangedTaskResult
End Using
End Using
    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function
#End Region	


#Region "Clear Key5 params"
	Public Function ClearKey5param()
		Try
			Dim paramsToClear As String = "ML_CMD_SPLN_FormulateAPEPT," & _
											"ML_CMD_SPLN_FormulateFundCode," & _
											"ML_CMD_SPLN_FormulateAPPN," & _
											"ML_CMD_SPLN_FormulateCType," & _
											"ML_CMD_SPLN_FormulateMDEP," & _
											"ML_CMD_SPLN_FormulateDollarType," & _
											"ML_CMD_SPLN_FormulateCommitItem," & _
											"ML_CMD_SPLN_FormulateSAG" 
								
			Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Me.ClearSelections(si, globals, api, args, paramsToClear)	
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			Return selectionChangedTaskResult
	
	    Catch ex As Exception
	        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	    End Try
	End Function
	
	Public Function ClearKey5params_NoFCAPPN()
		Try
			Dim paramsToClear As String = "ML_CMD_SPLN_FormulateAPEPT," & _
											"ML_CMD_SPLN_FormulateFundCode," & _
											"ML_CMD_SPLN_FormulateCType," & _
											"ML_CMD_SPLN_FormulateMDEP," & _
											"ML_CMD_SPLN_FormulateDollarType," & _
											"ML_CMD_SPLN_FormulateCommitItem," & _
											"ML_CMD_SPLN_FormulateSAG" 
		
			Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Me.ClearSelections(si, globals, api, args, paramsToClear)	
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			Return selectionChangedTaskResult
	
	    Catch ex As Exception
	        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	    End Try
	End Function
#End Region	

	End Class
End Namespace
