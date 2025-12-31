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



Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_PGM_Helper
	Public Class MainClass
		Private si As SessionInfo
        Public globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try			
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args	
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						Dim dbExt_LoadResult As New XFLoadDashboardTaskResult()
						Select Case args.FunctionName.ToLower()
								Case "load_req_detailsdashboard"
									dbExt_LoadResult = Me.load_req_detailsdashboard()
									Return dbExt_LoadResult	
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
								Me.DbCache()
							Case "copyrolloverreq"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.CopyRolloverREQ()
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
							Case "submit_reqs", "manage_req_status"
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult
							Case "importreq" , "rollfwdreq"
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
							Case "create_manpower_req"
								dbExt_ChangedResult = Me.CreateManpowerREQ(globals)
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
								
							Case "setdeafultappnparam"
								dbExt_ChangedResult = Me.SetDeafultAPPNParam()
								Return dbExt_ChangedResult
								
							Case "applyfundscenter"
								dbExt_ChangedResult = Me.ApplyFundsCenter()
								Return dbExt_ChangedResult
								
							Case "cachedashboard"
								dbExt_ChangedResult = Me.DbCache()
								Return dbExt_ChangedResult
								
							Case "setreqidfordetails"
								dbExt_ChangedResult = Me.Setreqidfordetails()
								Return dbExt_ChangedResult
								
								
						End Select					
'@Jeff Martin - Can this be removed
#Region "Cache Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("CachePrompts") Then
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
							 Me.CachePrompts(si,globals,api,args)
'							 dbExt_ChangedResult = Me.CachePrompts(si,globals,api,args)
'								If dbExt_ChangedResult.ShowMessageBox = True Then
'									Return dbExt_ChangedResult
'								End If
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

			End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

'END MAIN =================================================================================================

#Region "Constants"
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
			'Brapi.ErrorLog.LogMessage(si,"Valid" & validatorEmailList.count)
			'Brapi.ErrorLog.LogMessage(si,"All" & stakeholderEmailList.count)
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
				
           
					Dim sqa_xfc_cmd_pgm_req = New SQA_XFC_CMD_PGM_REQ(connection)
					Dim SQA_XFC_CMD_PGM_REQ_DT = New DataTable()
					Dim sqa = New SqlDataAdapter()

				'Fill the DataTable With the current data From FMM_Dest_Cell
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
   		}
			sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlparams)
			
			
			
			Dim Mainreqrow As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{sREQ}'" ).FirstOrDefault()
			
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
			
		sqa_xfc_cmd_pgm_req.Update_XFC_CMD_PGM_REQ(SQA_XFC_CMD_PGM_REQ_DT, sqa)
		
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
'brapi.ErrorLog.LogMessage(si,$"REQIDS: {req_IDs}, Status: {new_Status}, wfProfileName: {wfProfileName}, Dashboard: {Dashboard}")

			Try
				If req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate") And Not Dashboard.XFContainsIgnoreCase("Mpr") Then
					
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Validate") Then
					Me.Update_REQ_Status("Validate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Prioritize") Then
					Me.Update_REQ_Status("Prioritize")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Import") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Rollover") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve CMD Requirements") Then
					Me.Update_REQ_Status("Approve")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate CMD Requirements") And Dashboard.XFContainsIgnoreCase("Mpr") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve Requirements") Then
					Me.Update_REQ_Status("Approve")
				
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try

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
				'@Jeff Martin - Think we should add these on else side of 427/Demote
				Status_manager.Add("L5_Formulate_PGM|Validate","L4_Validate_PGM")
				Status_manager.Add("L4_Formulate_PGM|Validate","L3_Validate_PGM")
				Status_manager.Add("L3_Formulate_PGM|Validate","L3_Validate_PGM")
				Status_manager.Add("L2_Formulate_PGM|Validate","L2_Validate_PGM")
				Status_manager.Add("L4_Validate_PGM|Prioritize","L4_Prioritize_PGM")
				Status_manager.Add("L3_Validate_PGM|Prioritize","L3_Prioritize_PGM")
				Status_manager.Add("L2_Validate_PGM|Prioritize","L2_Prioritize_PGM")
				Status_manager.Add("L4_Validate_PGM|Approve","L4_Approve_PGM")
				Status_manager.Add("L3_Validate_PGM|Approve","L3_Approve_PGM")
				Status_manager.Add("L2_Validate_PGM|Approve","L2_Approve_PGM")
				Status_manager.Add("L4_Prioritize_PGM|Approve","L4_Approve_PGM")
				Status_manager.Add("L3_Prioritize_PGM|Approve","L3_Approve_PGM")
				Status_manager.Add("L2_Prioritize_PGM|Approve","L2_Approve_PGM")
				Status_manager.Add("L2_Approve_PGM|Final","L2_Final_PGM")
				Status_manager.Add("L3_Approve_PGM|Validate","L2_Validate_PGM")
				Status_manager.Add("L4_Approve_PGM|Validate","L3_Validate_PGM")
				Status_manager.Add("L2_Formulate_PGM|Final","L2_Final_PGM")
				Status_manager.Add("L2_Final_PGM|Formulate","L2_Formulate_PGM")

				
				If args.NameValuePairs.XFGetValue("Demote") = "True" Then
#Region "Demote Statuses"
					'--------------------------Demote Statuses----------------------------------
					'Approve
					Status_manager.Add("L2_Approve_PGM|L2_Prioritize_PGM","L2_Prioritize_PGM")
					Status_manager.Add("L2_Approve_PGM|L2_Validate_PGM","L2_Validate_PGM")
					Status_manager.Add("L2_Approve_PGM|L3_Approve_PGM","L3_Approve_PGM")
					Status_manager.Add("L2_Approve_PGM|L3_Prioritize_PGM","L3_Prioritize_PGM")
					Status_manager.Add("L2_Approve_PGM|L3_Validate_PGM","L3_Validate_PGM")
					Status_manager.Add("L2_Approve_PGM|L3_Formulate_PGM","L3_Formulate_PGM")
					Status_manager.Add("L2_Approve_PGM|L4_Approve_PGM","L4_Approve_PGM")
					Status_manager.Add("L2_Approve_PGM|L4_Prioritize_PGM","L4_Prioritize_PGM")
					Status_manager.Add("L2_Approve_PGM|L4_Validate_PGM","L4_Validate_PGM")
					Status_manager.Add("L2_Approve_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L2_Approve_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					
					Status_manager.Add("L3_Approve_PGM|L3_Prioritize_PGM","L3_Prioritize_PGM")
					Status_manager.Add("L3_Approve_PGM|L3_Validate_PGM","L3_Validate_PGM")
					Status_manager.Add("L3_Approve_PGM|L3_Formulate_PGM","L3_Formulate_PGM")
					Status_manager.Add("L3_Approve_PGM|L4_Approve_PGM","L4_Approve_PGM")
					Status_manager.Add("L3_Approve_PGM|L4_Prioritize_PGM","L4_Prioritize_PGM")
					Status_manager.Add("L3_Approve_PGM|L4_Validate_PGM","L4_Validate_PGM")
					Status_manager.Add("L3_Approve_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L3_Approve_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					
					Status_manager.Add("L4_Approve_PGM|L4_Prioritize_PGM","L4_Prioritize_PGM")
					Status_manager.Add("L4_Approve_PGM|L4_Validate_PGM","L4_Validate_PGM")
					Status_manager.Add("L4_Approve_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L4_Approve_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					
					'Validate
					Status_manager.Add("L2_Validate_PGM|L2_Formulate_PGM","L2_Formulate_PGM")
					Status_manager.Add("L2_Validate_PGM|L3_Approve_PGM","L3_Approve_PGM")
					Status_manager.Add("L2_Validate_PGM|L3_Prioritize_PGM","L3_Prioritize_PGM")
					Status_manager.Add("L2_Validate_PGM|L3_Validate_PGM","L3_Validate_PGM")
					Status_manager.Add("L2_Validate_PGM|L3_Formulate_PGM","L3_Formulate_PGM")
					Status_manager.Add("L2_Validate_PGM|L4_Approve_PGM","L4_Approve_PGM")
					Status_manager.Add("L2_Validate_PGM|L4_Prioritize_PGM","L4_Prioritize_PGM")
					Status_manager.Add("L2_Validate_PGM|L4_Validate_PGM","L4_Validate_PGM")
					Status_manager.Add("L2_Validate_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L2_Validate_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					
					Status_manager.Add("L3_Validate_PGM|L3_Formulate_PGM","L3_Formulate_PGM")
					Status_manager.Add("L3_Validate_PGM|L4_Approve_PGM","L4_Approve_PGM")
					Status_manager.Add("L3_Validate_PGM|L4_Prioritize_PGM","L4_Prioritize_PGM")
					Status_manager.Add("L3_Validate_PGM|L4_Validate_PGM","L4_Validate_PGM")
					Status_manager.Add("L3_Validate_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L3_Validate_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					
					Status_manager.Add("L4_Validate_PGM|L4_Formulate_PGM","L4_Formulate_PGM")
					Status_manager.Add("L4_Validate_PGM|L5_Formulate_PGM","L5_Formulate_PGM")
					'--------------------------Demote Statuses----------------------------------
#End Region
				End If 
				
				Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
				Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs")
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim Mode As String = args.NameValuePairs.XFGetValue("Mode")
				If Mode.XFEqualsIgnoreCase("Single") Then 

					req_IDs = req_IDs.Split(" ").Last()
				Else 
					req_IDs = req_IDs
					
				End If 
				
				If String.IsNullOrWhiteSpace(req_IDs) Then
						Return Nothing
				Else 

					Dim full_Req_ID_List As List (Of String) =  StringHelper.SplitString(req_IDs, ",")
'Brapi.ErrorLog.LogMessage(si, "full_Req_ID_List = " & full_Req_ID_List.Count & ", req_IDs; " & req_IDs)

					'SQL has a parameter limit of 2100. To work around that if REQ list is more than 200 we chunk it
'					Dim size As Integer = 2000
'					Dim reqListChuncks = Me.ChunckREQList(full_Req_ID_List, size)
'					For Each Req_ID_List As List(Of String) In reqListChuncks
'@Jeff Martin - DOes this code get called after the Manage DB is saved?  I assume not, need to see that code.
					Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
						
'Brapi.ErrorLog.LogMessage(si, "@HERE 2")						
						If String.IsNullOrWhiteSpace(new_Status) Then 
	
							Return dbExt_ChangedResult
						Else
							Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
							Using connection As New SqlConnection(dbConnApp.ConnectionString)
								connection.Open()
								Dim sqa_xfc_cmd_pgm_req = New SQA_XFC_CMD_PGM_REQ(connection)
								Dim SQA_XFC_CMD_PGM_REQ_DT = New DataTable()
								Dim sqa_xfc_cmd_pgm_req_details = New SQA_XFC_CMD_PGM_REQ_DETAILS(connection)
								Dim SQA_XFC_CMD_PGM_REQ_DETAILS_DT = New DataTable()
								Dim sqa_xfc_cmd_pgm_req_details_audit = New SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT(connection)
								Dim SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT = New DataTable()
								
								Dim sqa = New SqlDataAdapter()
								Dim dtfilter As String = String.Empty
								Dim paramNames As New List(Of String)
							    If full_Req_ID_List.Count > 1 Then
							        
							        For i As Integer = 0 To full_Req_ID_List.Count - 1
							            Dim paramName As String = "'" & full_Req_ID_List(i) & "'"
							            paramNames.Add(paramName)
							        Next
									dtfilter = String.Join(",", paramNames)
							    ElseIf full_Req_ID_List.Count = 1 Then
									Dim paramName As String = "'" & full_Req_ID_List(0) & "'"
									dtfilter = paramName 
							    End If
							'Fill the DataTable With the current data From FMM_Dest_Cell
							'@Jeff Martin - Need to discuss DB Table indexes and it might be more performant to include entity in that
								Dim sql As String = $"SELECT * 
														FROM XFC_CMD_PGM_REQ 
														WHERE WFScenario_Name = @WFScenario_Name
														AND WFCMD_Name = @WFCMD_Name
														AND WFTime_Name = @WFTime_Name
														AND REQ_ID in ({DTFilter})"
																
								' 2. Create a list to hold the parameters
								Dim paramList As New List(Of SqlParameter) From {
								New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
								New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
								New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
								}
	
								
							
								' 4. Convert the list to the array your method expects
								Dim sqlparams As SqlParameter() = paramList.ToArray()
		
								sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlparams)
	
								' --- get list of parent IDs and select all detail rows in one query ---
								Dim parentIds As New List(Of String)()
								
								For Each parentRow As DataRow In SQA_XFC_CMD_PGM_REQ_DT.Rows
									
									If Not IsDBNull(parentRow("CMD_PGM_REQ_ID")) Then
										Dim columnValue As Object = parentRow("CMD_PGM_REQ_ID")
										Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
										
										parentIds.Add(reqIdAsGuid.ToString())
										
									End If
								Next
								Dim DetaildtFilter As String 
								If parentIds.Count > 0 Then
									' Build a comma separated list of ints and query details table with IN (...)
						' NOTE: parentIds are integers sourced from DB so this string concatenation is safe in this context.
						Dim DetailparamNames As New List(Of String)
						For i As Integer = 0 To parentIds.Count - 1
	                        'detailsParams.Add(New SqlParameter($"@ID{i}", SqlDbType.NVarChar) With {.Value = parentIds(i)})
							'Dim DetailparamName As String = "'{" & parentIds(i) & "}'"
							'Dim Guiddetailparamname As String = $"CONVERT({DetailparamName},'System.Guid')"
							Dim DetailparamName As String = "'" & parentIds(i) & "'"
							
	            			'DetailparamNames.Add(Guiddetailparamname)
							DetailparamNames.Add(DetailparamName)
							
                    	Next
						'DetaildtFilter = $"CMD_PGM_REQ_ID IN ({String.Join(",", DetailparamNames)})"
						DetaildtFilter = String.Join(",", DetailparamNames)
									Dim idsCsv As String = String.Join(",", parentIds)
									'@Jeff Martin - On the Audit Table(s), we need to add another Audit Table for the Narrative fields per the SDA, but in either case, we will only be doing inserts into Audit Table, so might not need SQA and just build function to Insert.
									Dim SQL_Audit As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ_Details_Audit
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											AND CMD_PGM_REQ_ID in ({DetaildtFilter})"
									
									
									sql = $"SELECT * 
											FROM XFC_CMD_PGM_REQ_Details 
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											AND CMD_PGM_REQ_ID in ({DetaildtFilter})"
	
								  Dim detailsParams As New List(Of SqlParameter) From {
										New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
										New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
										New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
									}
									
								
									sqa_xfc_cmd_pgm_req_details.Fill_XFC_CMD_PGM_REQ_DETAILS_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_DT, sql, detailsParams.ToArray())
									sqa_xfc_cmd_pgm_req_details_audit.Fill_XFC_CMD_PGM_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, SQL_Audit, detailsParams.ToArray())
								End If
'Brapi.ErrorLog.LogMessage(si, "SQL: " & sql)
								' At this point detailsAllDT contains all matching XFC_CMD_PGM_REQ_Details rows (if any).
								' Update all returned parent rows
								For Each row As DataRow In SQA_XFC_CMD_PGM_REQ_DT.Rows
									'@Jeff Martin, is this something that we would ever run into due to hiding buttons, just for my own knowledge.  
									Dim wfStepAllowed = Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Step_Allowed(si, args, curr_Status, row("Entity"))
									If wfStepAllowed = False Then
										dbExt_ChangedResult.ShowMessageBox = True
										If Not String.IsNullOrWhiteSpace(dbExt_ChangedResult.Message) Then
											dbExt_ChangedResult.Message &= Environment.NewLine
										End If
										'@Jeff Martin - Should not throw an exception here, it will appear to user like there is a defect in the system.  We need to remove this, but keep the db extender message
										Throw New Exception($"Cannot change status of Requirement '{row("REQ_ID")}' at this time. Contact requirements manager.")
										dbExt_ChangedResult.Message &= $"Cannot change status of REQ_ID '{row("REQ_ID")}' at this time. Contact requirements manager."

									Else

										Dim existingStatus As String = ""
										If Not IsDBNull(row("Status")) Then existingStatus = row("Status").ToString().Trim()
	'@Jeff Martin - I am unsure that this will work if not a demote, we need to run through that on Thursday.
										Dim lookupKey As String = existingStatus & "|" & new_Status
'brapi.ErrorLog.LogMessage(si,"lookupKey: " & lookupKey)
										Dim resolvedStatus As String
										If Status_manager.ContainsKey(lookupKey) Then
											resolvedStatus = Status_manager(lookupKey)
										Else
											'@Jeff Martin, is this level of error handling overkill and something we should identify in testing?  Seems like we are saying 
											resolvedStatus = existingStatus
											dbExt_ChangedResult.ShowMessageBox = True
											dbExt_ChangedResult.Message &= $"REQ_ID '{row("REQ_ID")}' has an incorrect status, can't be updated."
										End If
										If Not String.IsNullOrEmpty(demote_comment) Then
											row("Demotion_Comment") = demote_comment
										End If 
										row("Status") = resolvedStatus
										'@Jeff Martin - Add Review Entity here
										row("Update_User") = si.UserName
										row("Update_Date") = DateTime.Now
'Brapi.ErrorLog.LogMessage(si, "existingStatus | new_Status  |  resolvedStatus: " & existingStatus & "|" & new_Status &  " | " & resolvedStatus)
										' If we have details loaded, update the detail rows that belong to this parent now
										
										Dim pid As String = ""
											pid = row("CMD_PGM_REQ_ID").ToString()
										Dim filterExpr As String = String.Format("CMD_PGM_REQ_ID = '{0}'", pid)

										If SQA_XFC_CMD_PGM_REQ_DETAILS_DT IsNot Nothing AndAlso SQA_XFC_CMD_PGM_REQ_DETAILS_DT.Rows.Count > 0 AndAlso Not IsDBNull(row("CMD_PGM_REQ_ID")) Then
											
											
											Dim matchingDetails() As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_DT.Select(filterExpr)
											
											For Each drow As DataRow In matchingDetails
								
												If Not FCList.Contains($"{drow("Entity")}")
													FCList.Add($"{drow("Entity")}")
												End If
												globals.SetStringValue($"FundsCenterStatusUpdates - {drow("Entity")}", $"{existingStatus}|{resolvedStatus}")

												drow("Flow") = resolvedStatus
												drow("Update_User") = si.UserName
												drow("Update_Date") = DateTime.Now
											Next
											'End If
										End If
										
										
										'@Jeff Martin - This should only ever be new inserted audit rows	
										Dim matchingAuditDetails() As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Select(filterExpr)
												
										If matchingAuditDetails.Length > 0 Then
											For Each drow As DataRow In matchingAuditDetails
												Dim currentHistory As String = If(drow("Orig_Flow") Is DBNull.Value, _
												 String.Empty, _
												 drow("Orig_Flow").ToString())
													If String.IsNullOrEmpty(currentHistory) Then
														drow("Orig_Flow") = existingStatus
													Else
														drow("Orig_Flow") = currentHistory + ", " + existingStatus
													End If
													drow("Updated_Flow") = resolvedStatus
											Next
										Else
											Dim newrow As datarow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.NewRow()
											newrow("CMD_PGM_REQ_ID") = row("CMD_PGM_REQ_ID")
											newrow("WFScenario_Name") = row("WFScenario_Name")
											newrow("WFCMD_Name") = row("WFCMD_Name")
											newrow("WFTime_Name") = row("WFTime_Name")
											newrow("Entity") = row("Entity")
											newrow("Account") = "Req_Funding"
											newrow("Start_Year") = row("WFTime_Name")
											newrow("Orig_IC") = "None"
											newrow("Updated_IC") = "None"
											newrow("Orig_Flow") =  existingStatus
											newrow("Updated_Flow") = resolvedStatus
											newrow("Orig_UD1") = row("APPN")
											newrow("Updated_UD1") = row("APPN")
											newrow("Orig_UD2") = row("MDEP")
											newrow("Updated_UD2") = row("MDEP")
											newrow("Orig_UD3") = row("APE9")
											newrow("Updated_UD3") = row("APE9")
											newrow("Orig_UD4") = row("Dollar_Type")
											newrow("Updated_UD4") = row("Dollar_Type")
											newrow("Orig_UD5") = "None"
											newrow("Updated_UD5") = "None"
											newrow("Orig_UD6") = row("Obj_Class")
											newrow("Updated_UD6") = row("Obj_Class")
											newrow("Orig_UD7") = "None"
											newrow("Updated_UD7") = "None"
											newrow("Orig_UD8") = "None"
											newrow("Updated_UD8") = "None"
											newrow("Create_Date") = DateTime.Now
											newrow("Create_User") = si.UserName
										
										
										
											SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.rows.add(newrow)
									
									
										End If
										
								'@Jeff Martin - This is how I think this piece should work
									'Audits Only on Updates.  If we import or Rollover and it's a new insert, the tables have time stamps and user info, so that should be sufficient from audit stand point, but the updates to the REQ tables will need to be audited using the table.
									'Emails - If Import results in no email, why wouldn't Rollover behave same way?  Are emails only necesary when reqs move beyond formulate status?
								If  Not wfProfileName.XFContainsIgnoreCase("Import")
								 Workspace.GBL.GBL_Assembly.GBL_Helpers.SendStatusChangeEmail(si, globals,api,args,resolvedStatus, row("Req_ID"))
								End If 
									End If
								Next
	
								
						
								
					' Persist all changes back to the database
					
								sqa_xfc_cmd_pgm_req.Update_XFC_CMD_PGM_REQ(SQA_XFC_CMD_PGM_REQ_DT, sqa)
								sqa_xfc_cmd_pgm_req_details.Update_XFC_CMD_PGM_REQ_DETAILS(SQA_XFC_CMD_PGM_REQ_DETAILS_DT, sqa)
								sqa_xfc_cmd_pgm_req_details_audit.Update_XFC_CMD_PGM_REQ_DETAILS_AUDIT(SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, sqa)
								
							End Using
						End If
					
					'@Yosef & Jeff Martin - I think this piece should be it's own function/sub, so that we don't have to update status when not necessary
					Dim EntityLists = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_PGM")
					Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
					Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
					Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")
'Brapi.ErrorLog.LogMessage(si,"@@HERE1" & String.Join(",",FCList))
					Dim customSubstVars As New Dictionary(Of String, String) 
					customSubstVars.Add("EntList",BaseEntityList)
					customSubstVars.Add("ParentEntList",ParentEntityList)
					customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
					Dim currentYear As Integer = Convert.ToInt32(wfInfoDetails("TimeName"))
					customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
					BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
'Brapi.ErrorLog.LogMessage(si,"HERE2")
					Return dbExt_ChangedResult
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function


#End Region
#Region "Chunck REQ List"
		'@Jeff Martin & Yosef - We should kill this
		Public Function ChunckREQList(reqList As List(Of String), size As Integer) As List(Of List(Of String))
			Dim chuncks As New List(Of List(Of String))()
			For i As Integer = 0 To reqList.Count - 1 Step size
				chuncks.Add(reqList.Skip(i).Take(size).ToList())
			Next
			Return chuncks
		End Function
#End Region

#Region "Manage Requirements"
		
		Public Function CopyRolloverREQ() As xfselectionchangedTaskResult
			Try
Dim startTime As DateTime = DateTime.Now	

				Dim action As String = args.NameValuePairs.XFGetValue("action")
				'Get source scenrio and time from the passed in value
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				
				Dim senario As String =  wfInfoDetails("ScenarioName")
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim tm As String = wfInfoDetails("TimeName")
				Dim srcfundCenter = args.NameValuePairs.XFGetValue("sourceEntity","")
				Dim SrcREQNameTilte = args.NameValuePairs.XFGetValue("SourceREQ","")
				
				Dim prevtm As Integer
				Dim prevscName As String
				If Integer.TryParse(tm, prevtm) Then
					prevtm = prevtm - 1
					prevscName = "CMD_PGM_C" & prevtm
				End If					
				Dim REQ_IDs As String = args.NameValuePairs("req_IDs")

				'if no REQs to roll over simply end
				If String.IsNullOrWhiteSpace(REQ_IDs) Then
					Return Nothing
				End If
				
				REQ_IDs = REQ_IDs.Replace(" ","")
				Dim REQIDs_list As New List(Of String)
				REQIDs_list = REQ_IDs.Split(",").ToList
				
				'Prep it for a SQL clause
				REQ_IDs = "'" & REQ_IDs.Replace(",","','") & "'"
				
				Dim REQDT As DataTable = New DataTable()
				Dim REQDetailDT As DataTable = New DataTable()

				Dim sqa As New SqlDataAdapter()
				
				'select the current record into the sql adapter
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
			           sqlConn.Open()
					   Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
					   Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
					   Dim whereClause As String = ""
					   Dim newREQ_ID As String = ""
					   Dim newTitle As String = ""
					   '@Yosef -  I also think these should use the GBL generic sqa since these are just the source queries.
					   '***I am not sure which one, lets discuss where that is..
					   
					   'Get target datatablssses
						Dim GUIDs As String = ""
						Dim targetREQDT As DataTable = New DataTable()
						Dim targetSqlREQ As String = ""
						Dim targetsqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
						If action.ToLower = "copy" Then
							targetSqlREQ = $"SELECT * 
								FROM XFC_CMD_PGM_REQ
								WHERE WFScenario_Name = '{senario}'
								And WFCMD_Name = '{cmd}'
								AND WFTime_Name = '{tm}'
								AND REQ_ID in ({REQ_IDs})"
						End If
						If action.ToLower = "rollover" Then
							targetSqlREQ  = $"SELECT *
								FROM XFC_CMD_PGM_REQ
								WHERE WFScenario_Name = '{senario}'
								And WFCMD_Name = '{cmd}'
								AND WFTime_Name = '{tm}'
								AND REQ_ID in ({REQ_IDs})"
						End If
'BRApi.ErrorLog.LogMessage(si, "REQ Targer: " & SqlREQ)	
						targetREQDT.AcceptChanges()
						sqa.AcceptChangesDuringFill = False
						targetsqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, targetREQDT, targetSqlREQ, Nothing)
						
						'Get Target Details
						GUIDs = ""
						For Each r As DataRow In targetREQDT.Rows
							GUIDs =GUIDs &  "'" & r("CMD_PGM_REQ_ID").ToString & "',"
						Next
						
						Dim SqlREQDetail As String = $"SELECT * 
								FROM XFC_CMD_PGM_REQ_Details
								WHERE 1=0"
						If Not String.IsNullOrWhiteSpace(GUIDs) Then
							GUIDs = GUIDs.Substring(0,GUIDs.Length -1)
							SqlREQDetail = $"SELECT * 
								FROM XFC_CMD_PGM_REQ_Details
								WHERE CMD_PGM_REQ_ID in ({GUIDs})"
						End If
	'BRApi.ErrorLog.LogMessage(si, "target detail: " & SqlREQDetail)					
						Dim targetREQDetailDT As DataTable = New DataTable()
						targetREQDetailDT.AcceptChanges()
						sqa.AcceptChangesDuringFill = False
					    Dim targetsqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
						targetsqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, targetREQDetailDT, SqlREQDetail, Nothing )
	'BRApi.ErrorLog.LogMessage(si, "After FILL")		
	
					   'Get source data
					   If action.ToLower = "copy" Then
						   whereClause = $" WFScenario_Name = '{senario}'
											And WFCMD_Name = '{cmd}'
											AND WFTime_Name = '{tm}'
											AND REQ_ID in ({REQ_IDs})"
						   newTitle = "Title + ' - Copy'"
					   End If
					   If action.ToLower = "rollover" Then
						   'whereClause = $" CMD_PGM_REQ_ID IN ({CMD_PGM_REQ_IDs})"
						   'REQs from prev years and the current year to be overwritten if exist
						   whereClause = $" WFScenario_Name = '{prevscName}'
											And WFCMD_Name = '{cmd}'
											AND WFTime_Name = '{prevtm}'
											AND REQ_ID in ({REQ_IDs})"
						   newTitle = "Title"
					   End If
					   
              		   Dim SqlREQ As String = $"SELECT 
												CASE 
											        WHEN EXISTS (
											            SELECT 1 
											            FROM XFC_CMD_PGM_REQ as InnerREQ 
											            WHERE InnerREQ.REQ_ID = OuterREQ.REQ_ID 
											              AND InnerREQ.WFScenario_Name = '{senario}'
											        ) 
											        THEN (
											            SELECT CMD_PGM_REQ_ID 
											            FROM XFC_CMD_PGM_REQ as InnerREQ 
											            WHERE InnerREQ.REQ_ID = OuterREQ.REQ_ID 
											              AND InnerREQ.WFScenario_Name = '{senario}'
											        )
											        ELSE NEWID()
											    END AS CMD_PGM_REQ_ID, CMD_PGM_REQ_ID orig_CMD_PGM_REQ_ID,
			   									'{senario}' WFScenario_Name,WFCMD_Name,'{tm}' WFTime_Name,REQ_ID_Type,REQ_ID,{newTitle} Title,Description,
											   Justification,Entity,APPN,MDEP,APE9,Dollar_Type,Obj_Class,CType,UIC,Cost_Methodology,Impact_Not_Funded,Risk_Not_Funded,Cost_Growth_Justification,Must_Fund,
											   Funding_Src,Army_Init_Dir,CMD_Init_Dir,Activity_Exercise,Directorate,Div,Branch,IT_Cyber_REQ,Emerging_REQ,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Num,
											   Task_Order_Num,Award_Target_Date,POP_Exp_Date,Contractor_ManYear_Equiv_CME,COR_Email,POC_Email,Review_POC_Email,MDEP_Functional_Email,Notification_List_Emails,
											   Gen_Comments_Notes,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Cat_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commit_Group,SS_Cap,Strategic_BIN,LIN,REQ_Type,
											   DD_Priority,Portfolio,DD_Cap,JNT_Cap_Area,TBM_Cost_Pool,TBM_Tower,APMS_AITR_Num,Zero_Trust_Cap,Assoc_Directives,Cloud_IND,Strat_Cyber_Sec_PGM,
											   Notes,FF_1,FF_2,FF_3,FF_4,FF_5,Status,Invalid,Val_Error,Create_Date,Create_User,Update_Date,Update_User,Related_REQs,Review_Entity,Demotion_Comment,Validation_List_Emails
									   FROM XFC_CMD_PGM_REQ AS OuterREQ
									   WHERE{whereClause}"
'BRApi.ErrorLog.LogMessage(si, "REQ Source: " & SqlREQ)	
					    REQDT.AcceptChanges()
						sqa.AcceptChangesDuringFill = False
		                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, Nothing)
						
						'capture new and old GUIDs
'BRApi.ErrorLog.LogMessage(si, "After REQ fill")						
						 Dim GUID_Mapping As New DataTable()
					        GUID_Mapping.Columns.Add("CMD_PGM_REQ_ID", GetType(String))
					        GUID_Mapping.Columns.Add("Orig_CMD_PGM_REQ_ID", GetType(String))
					        GUID_Mapping.Columns.Add("REQ_ID", GetType(String))
							GUID_Mapping.Columns.Add("Orig_REQ_ID", GetType(String))
							
						'Update REQ_IDs
						For Each r In REQDT.Rows
'BRApi.ErrorLog.LogMessage(si, "In loop")							
							Dim orig_guid As String = r("CMD_PGM_REQ_ID").toString '= Guid.NewGuid()
							Dim orig_id As String = r("REQ_ID")
							If action.ToLower = "copy" Then
								r("REQ_ID") = Me.GetNextREQID(r("Entity"))
								r("CMD_PGM_REQ_ID") = Guid.NewGuid()
							End If
							r("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, r("Entity") ) & "_Formulate_PGM"
							
							GUID_Mapping.Rows.Add( r("CMD_PGM_REQ_ID").toString, orig_guid, orig_id, r("REQ_ID"))
						Next
						REQDT.Columns.Remove("orig_CMD_PGM_REQ_ID")
						
'For Each r As DataRow In GUID_Mapping.Rows
'	BRApi.ErrorLog.LogMessage(si,"GUID_Mapping: "&  String.Join(", ", r.ItemArray) )
'Next						
						
						'Merge REQ dtatables
						Try	
							targetsqaREQReader.MergeandSync_XFC_CMD_PGM_REQ_toDB(si, REQDT, targetREQDT)
							
						Catch ex As ConstraintException
				            Brapi.ErrorLog.LogMessage(si, "Constraint violation occurred: " & ex.Message)
				
				            ' Inspect rows in error
				            For Each row As DataRow In targetREQDT.GetErrors()
				                Brapi.ErrorLog.LogMessage(si, "Row in error: " & String.Join(", ", row.ItemArray))
				                Brapi.ErrorLog.LogMessage(si, "Error description: " & row.RowError)
				            Next
						End Try
						
						'Get the source for REQ details
						Dim tempTable As DataTable = targetREQDetailDT.Clone() 'New DataTable()
						If action.ToLower = "rollover" Then 
							Dim  tempTableName As String = "#Temp_XFC_CMD_PGM_REQ_" & si.AuthToken.AuthSessionID.ToString	
							tempTableName = tempTableName.Replace("-", "_")
							Dim tempSQL As String = $" select 
															tmp.CMD_PGM_REQ_ID, tmp.WFScenario_Name,tmp.WFCMD_Name,tmp.WFTime_Name,dtl.Unit_of_Measure,tmp.Entity,dtl.IC,dtl.Account,dtl.Flow,dtl.UD1,
															dtl.UD2,dtl.UD3,dtl.UD4,dtl.UD5,dtl.UD6,dtl.UD7,dtl.UD8,dtl.Start_Year,dtl.FY_1,dtl.FY_2,dtl.FY_3,dtl.FY_4,dtl.FY_5,dtl.FY_Total,
															dtl.AllowUpdate,dtl.Create_Date,dtl.Create_User,dtl.Update_Date,dtl.Update_User
														From  {tempTableName} tmp
														Join XFC_CMD_PGM_REQ req On req.REQ_ID = tmp.REQ_ID And req.WFScenario_Name = '{prevscName}'
														Join XFC_CMD_PGM_REQ_Details dtl On dtl.CMD_PGM_REQ_ID = req.CMD_PGM_REQ_ID"
							
		BRApi.ErrorLog.LogMessage(si, "tempSQL: " & tempSQL)
							
							tempTable.AcceptChanges()
							sqa.AcceptChangesDuringFill = False
							Dim sqaTmpTblReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
							sqaTmpTblReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, tempTable, tempSQL, Nothing )
						End If

					
						If action.ToLower = "copy" Then 
'BRApi.ErrorLog.LogMessage(si, "in copy: targetREQDetailDT count: " & targetREQDetailDT.Rows.Count)							
							Dim query = From REQDetail In targetREQDetailDT.AsEnumerable()
		                    Join map In GUID_Mapping.AsEnumerable()
		                    On If(REQDetail.Field(Of Object)("CMD_PGM_REQ_ID"), "").ToString().Trim().ToLower() _
               					Equals If(map.Field(Of Object)("orig_CMD_PGM_REQ_ID"), "").ToString().Trim().ToLower()
		                    Select result = New With {
		                        .CMD_PGM_REQ_ID = map.Field(Of String)("CMD_PGM_REQ_ID"),
		                        .WFScenario_Name = REQDetail.Field(Of String)("WFScenario_Name"),
		                        .WFCMD_Name = REQDetail.Field(Of String)("WFCMD_Name"),
								.WFTime_Name = REQDetail.Field(Of String)("WFTime_Name"),
								.Unit_of_Measure = REQDetail.Field(Of String)("Unit_of_Measure"),
								.Entity = REQDetail.Field(Of String)("Entity"),
								.IC = REQDetail.Field(Of String)("IC"),
								.Account = REQDetail.Field(Of String)("Account"),
								.Flow = REQDetail.Field(Of String)("Flow"),
								.UD1 = REQDetail.Field(Of String)("UD1"),
								.UD2 = REQDetail.Field(Of String)("UD2"),
								.UD3 = REQDetail.Field(Of String)("UD3"),
								.UD4 = REQDetail.Field(Of String)("UD4"),
								.UD5 = REQDetail.Field(Of String)("UD5"),
								.UD6 = REQDetail.Field(Of String)("UD6"),
								.UD7 = REQDetail.Field(Of String)("UD7"),
								.UD8 = REQDetail.Field(Of String)("UD8"),
								.Start_Year = REQDetail.Field(Of String)("Start_Year"),
								.FY_1 = REQDetail.Field(Of Decimal?)("FY_1"),
								.FY_2 = REQDetail.Field(Of Decimal?)("FY_2"),
								.FY_3 = REQDetail.Field(Of Decimal?)("FY_3"),
								.FY_4 = REQDetail.Field(Of Decimal?)("FY_4"),
								.FY_5 = REQDetail.Field(Of Decimal?)("FY_5"),
								.FY_Total = REQDetail.Field(Of Decimal?)("FY_Total"),
								.AllowUpdate = REQDetail.Field(Of Boolean?)("AllowUpdate"),
								.Create_Date = REQDetail.Field(Of DateTime?)("Create_Date"),
								.Create_User = REQDetail.Field(Of String)("Create_User"),
								.Update_Date = REQDetail.Field(Of DateTime?)("Update_Date"),
								.Update_User = REQDetail.Field(Of String)("Update_User")
		                    }
							' Fill the  DataTable
'BRApi.ErrorLog.LogMessage(si, "query count:  " & query.Count() )							
					        For Each row In query
'BRApi.ErrorLog.LogMessage(si, "in query loop:  " )								
					            tempTable.Rows.Add(row.CMD_PGM_REQ_ID,row.WFScenario_Name,row.WFCMD_Name,row.WFTime_Name,row.Unit_of_Measure,row.Entity,row.IC,row.Account,row.Flow,row.UD1,row.UD2,row.UD3,row.UD4,row.UD5,row.UD6,row.UD7,row.UD8,row.Start_Year,row.FY_1,row.FY_2,row.FY_3,row.FY_4,row.FY_5,row.FY_Total,row.AllowUpdate,row.Create_Date,row.Create_User,row.Update_Date,row.Update_User)
					        Next
						
						End If
'For Each r As DataRow In targetREQDetailDT.Rows
'	BRApi.ErrorLog.LogMessage(si, " before join targetREQDetailDT: " & r("CMD_PGM_REQ_ID").ToString & " |   row: " & String.Join(",",r.ItemArray) )
'Next						
'For Each r As DataRow In tempTable.Rows
'	BRApi.ErrorLog.LogMessage(si, " before join src tempTable: " & r("CMD_PGM_REQ_ID").ToString  & " |   row: " & String.Join(",",r.ItemArray ))
'Next			
						'Merge REQ Details
						Try	
							targetsqaREQDetailReader.MergeandSync_XFC_CMD_PGM_REQ_Details_toDB(si, tempTable, targetREQDetailDT, sqa)
						Catch ex As ConstraintException
				            Brapi.ErrorLog.LogMessage(si, "Constraint violation occurred: " & ex.Message)
				
				            ' Inspect rows in error
				            For Each row As DataRow In targetREQDT.GetErrors()
				                Brapi.ErrorLog.LogMessage(si, "Row in error: " & String.Join(", ", row.ItemArray))
				                Brapi.ErrorLog.LogMessage(si, "Error description: " & row.RowError)
				            Next
						End Try
'For Each r As DataRow In tempTable.Rows
'	BRApi.ErrorLog.LogMessage(si, " After join targetREQDetailDT: " & r("CMD_PGM_REQ_ID").ToString & " |   row: " & String.Join(",",r.ItemArray) )
'Next							
						'targetsqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(targetREQDetailDT, sqa)
				
						Dim newREQRows As New List(Of DataRow)
						Dim newREQDetailRows As New List(Of DataRow)
						'Dim newREQRow As datarow = Nothing
						Dim FCList As New List(Of String)
						Dim EntList As New List(Of String)
						For Each row As DataRow In REQDT.Rows	
							'Set up entity/status for the cube writing rule
							If Not FCList.Contains($"E#{row("Entity")}")
								FCList.Add($"E#{row("Entity")}")
								Globals.SetStringValue($"FundsCenterStatusUpdates - {row("Entity")}",$"{row("Status")}")
								EntList.Add(row("Entity").ToString)
							End If
						Next

						'Write to the cube
						'@Jeff Martin - Ignore my comment above about Non-Demote status updates
						Args.NameValuePairs.Add("new_Status", "Formulate")
						'Me.Update_Status()
					
						FCList = FCList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
						'Globals.SetStringValue($"FundsCenterStatusUpdates - {api.pov.entity.name}",String.Empty)
						'-------------------------------------
						Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")
		'Brapi.ErrorLog.LogMessage(si,"@@FCList, " & String.Join(",",FCList))
						Dim EntityLists = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,EntList,"CMD_PGM")
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						
						Dim customSubstVars As New Dictionary(Of String, String) 
						customSubstVars.Add("ParentEntList",ParentEntityList)
						customSubstVars.Add("EntList",String.Join(",",FCList))
		'Brapi.ErrorLog.LogMessage(si,"@@ ParentEntityList: " & ParentEntityList)				
						customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
						Dim currentYear As Integer = Convert.ToInt32(wfInfoDetails("TimeName"))
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
								
					
					End Using	
				End Using
Dim endTime As DateTime = DateTime.Now
Dim elapsed As TimeSpan = endTime - startTime
Brapi.ErrorLog.LogMessage(si,$"**The method took {elapsed.TotalMilliseconds} milliseconds to execute.")
				Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function

		'@Jeff Martin & Yosef - This needs to be GBL function, I think this is 3rd place I have seen this exact same thing.
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

		'@Yosef - Looking through Import, Rollover and Copy, It feels like we could & should merge these with parameters
			'My justification for this is we have a pretty good idea that these relational tables will be relatively fluid and it will be painful to sustain this
			'Here are my thoughts....  No doubt Import will have to run the split table function and that would be Imports unique piece
			'I see it like this for all of these - 'Stage Data' so to speak -> Fill DT -> Map source to target -> Determine if Insert or Update -> Write to DB
			

		Public Sub UpdateNewREQColumns(ByRef newRow As DataRow, ByRef action As String) 
			'update the columns
			newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
			Dim trgtfundCenter = newRow("Entity")
			newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			
			If action.XFEqualsIgnoreCase("copy") Then
				Dim newREQ_ID As String= GetNextREQID(trgtfundCenter)
				newRow("REQ_ID") = newREQ_ID
				newRow("Title") = newRow("Title") & " - Copy" 'args.NameValuePairs.XFGetValue("NewREQName")
				
			End If
			
			If action.XFEqualsIgnoreCase("rollover") Then
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				newRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
				newRow("WFTime_Name") = wfInfoDetails("TimeName")				
			End If
			
		End Sub
		

		Public Sub UpdateCopyREQColumns(ByRef newRow As DataRow) 
			'update the columns
			Dim trgtfundCenter = newRow("Entity")'BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")

			Dim newREQ_ID As String= GetNextREQID(trgtfundCenter)
			newRow("REQ_ID") = newREQ_ID
'BRApi.ErrorLog.LogMessage(si,"Copied REQ_ID: " & newREQ_ID)			
			newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
			newRow("Title") = newRow("Title") & " - Copy" 'args.NameValuePairs.XFGetValue("NewREQName")
			newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			'newRow("Entity") = trgtfundCenter
			
		End Sub
		
		Public Sub UpdateCopyREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_PGM_REQ_ID As String) 
			'update the columns
			Dim trgtfundCenter = newRow("Entity")'BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
			
			newRow("CMD_PGM_REQ_ID") = newCMD_PGM_REQ_ID
			newRow("Flow") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			'newRow("Entity") = trgtfundCenter
			
		End Sub

		Public Sub UpdateNewREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_PGM_REQ_ID As String, ByRef action As String) 
			'update the columns
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim trgtfundCenter = newRow("Entity")
			newRow("Flow") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
			newRow("CMD_PGM_REQ_ID") = newCMD_PGM_REQ_ID
			
			If action.XFEqualsIgnoreCase("rollover") Then
				newRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
				newRow("WFTime_Name") = wfInfoDetails("TimeName")
				
				newRow("FY_1") = newRow("FY_2").ToString
				newRow("FY_2") = newRow("FY_3").ToString
				newRow("FY_3") = newRow("FY_4").ToString
				newRow("FY_4") = newRow("FY_5").ToString
				
				Dim currFY5Amt As Decimal = Convert.ToDecimal(newRow("FY_5").ToString)
				
				Dim inflationRate As Decimal  = GetInflationRate(newRow("UD1"))
				newRow("FY_5") = currFY5Amt + (currFY5Amt * inflationRate)
			End If
		End Sub
		
		'@Jeff Martin - Is this deprecated?  If so, lets delete
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
			  
		'@Jeff Martin - Is this just an oversight?  I thought this was wrapped up
		Public Function Attach_Doc() As xfselectionchangedTaskResult
		End Function
			
		'@jeff Martin - Deprecated? Delete if so
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

#Region "Cache Prompts"
		Public Function CachePrompts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)As Object
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sREQ As String = args.NameValuePairs.XFGetValue("REQ")
			Dim sMode As String = args.NameValuePairs.XFGetValue("mode","")
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			
			If sMode.XFContainsIgnoreCase("copyREQ") And String.IsNullOrWhiteSpace(sEntity) Then
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				Dim dashAction As String ="Refresh"
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType
				
				selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
				selectionChangedTaskResult.IsOK = True		
				'Added/Updated by Eburke to show message box and refresh dashboard 
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
				selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
				selectionChangedTaskResult.ShowMessageBox = True	
				selectionChangedTaskResult.Message = "Please select a funds center."
				'Throw New Exception("Please select a funds center.")
				Return selectionChangedTaskResult
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

#Region "Apply Funds Center"
Public Function ApplyFundsCenter() As Object
Dim Entity As String = args.NameValuePairs.XFGetValue("Entity")
	Dim dKeyVal As New Dictionary(Of String, String)
		dKeyVal.Add("BL_CMD_PGM_FundsCenter",Entity)
		
		'Brapi.ErrorLog.LogMessage(si, "Entity" & Entity)
	'If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_FundsCenter","NA") <> "NA" Then
		'dKeyVal.Add("BL_CMD_PGM_FundsCenter",args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_FundsCenter","NA"))
	'End If	
'
	Return Me.SetParameter(si, globals, api, dKeyVal)
		
End Function
#End Region

#Region "Set Default APPN Parameter"
Public Function SetDeafultAPPNParam() As Object

	Dim dKeyVal As New Dictionary(Of String, String)
							
	dKeyVal.Add("ML_CMD_PGM_FormulateAPPN","OMA")
	
	If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_RelatedREQList","NA") <> "NA" Then
		dKeyVal.Add("BL_CMD_PGM_RelatedREQList",args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_RelatedREQList","NA"))
	End If	
'Added 2 line to clear user cache before launching getcascadingfilters
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_PGM_CascadingFilterCache",$"CMD_PGM_rebuildparams_APPN","")
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_PGM_CascadingFilterCache",$"CMD_PGM_rebuildparams_Other","")	
	Return Me.SetParameter(si, globals, api, dKeyVal)
		
End Function
#End Region

#Region "Set reqid for details popup"
Public Function Setreqidfordetails() As Object
Dim reqid As String = args.NameValuePairs.XFGetValue("REQ")
	Dim dKeyVal As New Dictionary(Of String, String)					
	dKeyVal.Add("IV_CMD_TGT_REQTitleList",reqid)	
	Return Me.SetParameter(si, globals, api, dKeyVal)
		
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

#End Region

#Region "Cache Dashboard"
Public Function DbCache()
			Try
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"PGM_DB_Cache","Db","")	
				'cache in db name to be used for detials dashboards
				Dim sDb As String = args.NameValuePairs.XFGetValue("Db","")
				If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"PGM_DB_Cache","Db",sDb)	
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
#End Region

#Region "Set Related REQs"
Public Function SetRelatedREQs()
	Try
		'@Jeff Martin - Probably low hanging fruit, but I think we can grab the parameter that is bound to the grid view and get both the pre and post update values and use those to drive this function
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
				
           
					Dim sqa_xfc_cmd_pgm_req = New SQA_XFC_CMD_PGM_REQ(connection)
					Dim SQA_XFC_CMD_PGM_REQ_DT = New DataTable()
					Dim sqa = New SqlDataAdapter()
				
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name
									"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
   		}
	
				sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlparams)
			
	
			
			Dim Mainreqrow As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID = '{sREQ}'").FirstOrDefault()
'			
			Dim mainREQ As String = ""
'			
				mainREQ = $"{sREQ}"
		
			
				
			Dim currentREQs As String = String.Empty
			' Get the current list of related REQs before we overwrite it.
			Dim oldRelatedREQsString As String = Mainreqrow("Related_REQs").ToString()
			Dim oldList As List(Of String) = oldRelatedREQsString.Split(","c).Select(Function(r) r.Trim()).Where(Function(r) Not String.IsNullOrWhiteSpace(r)).ToList()
			Dim newList As New List(Of String)
			For Each REQ As String In REQList
				If REQ.XFContainsIgnoreCase(mainREQ) Then Continue For
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
				Brapi.ErrorLog.LogMessage(si, "relatedREQ" & relatedREQ)
				If relatedREQ.XFContainsIgnoreCase(mainREQ) Then Continue For
				Dim REQ = relatedREQ.Split(" ")(1).Trim()
				sEntity = relatedREQ.Split(" ")(0).Trim()
				'Brapi.ErrorLog.LogMessage(si, "REQ" & REQ)
				'Brapi.ErrorLog.LogMessage(si, "Entity" & sEntity)
				Dim RelatedREQsList As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ}'" ).FirstOrDefault()
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
				
				Dim RelatedREQsList As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ}'" ).FirstOrDefault()
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
			sqa_xfc_cmd_pgm_req.Update_XFC_CMD_PGM_REQ(SQA_XFC_CMD_PGM_REQ_DT,sqa)
		End Using
			
			Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

#End Region

#Region "Create Manpower REQ"

			Public Function CreateManpowerREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
'BRApi.ErrorLog.LogMessage(si,"In Test ..")				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
	
				Dim cmd As String = wfInfoDetails("CMDName")
				Dim tm As String = wfInfoDetails("TimeName")
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sSourcePosition As String = args.NameValuePairs.XFGetValue("Cprobe")
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)

				 Dim REQDT As DataTable = New DataTable()
				 Dim REQDetailDT As DataTable = New DataTable()
				 Dim sqa As New SqlDataAdapter()
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                sqlConn.Open()
						'Main
		                Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
		                Dim SqlREQ As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ
											WHERE WFCMD_Name = '{cmd}'
											And WFTime_Name = '{tm}'
											AND WFScenario_Name = '{sScenario}'
											AND ENTITY = '{sEntity}'
						 					AND REQ_ID_Type = 'Manpower'"
		
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
		                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ) 
							
						Dim row As DataRow
						Dim req_ID_Val As Guid
						Dim REQ_ID As String = ""
						Dim isInsert As Boolean = True
						
						If Not REQDT.Rows.Count = 0 Then
'brapi.ErrorLog.LogMessage(si,"Inside if not" & reqdt.Rows(0).Item("CMD_PGM_REQ_ID").ToString)
							req_ID_Val = reqdt.Rows(0).Item("CMD_PGM_REQ_ID")
							req_id = reqdt.Rows(0).Item("REQ_ID")
		                    isInsert = False
		                    row = reqdt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
							
						 Else 
							row = REQDT.NewRow()
							req_ID_Val = Guid.NewGuid
							req_id = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,sEntity)
'brapi.ErrorLog.LogMessage(si,"Inside else ") 
						End If 
						
						row("WFScenario_Name") = sScenario
						row("WFTime_Name") = tm
						row("Title") = "Manpower Requirement"
						row("Entity") = sEntity
						row("CMD_PGM_REQ_ID") = req_ID_Val
						row("WFCMD_Name") = sCube
						row("REQ_ID_Type") = "Manpower"
						row("REQ_ID") = req_id
						row("Description") = "Manpower Requirement"
						row("APPN") = "None"
						row("MDEP") = "None"
						row("APE9") = "None"
						row("Dollar_Type") = "None"
						row("Obj_Class") = "None"
						row("CType") = "None"
						row("UIC") = "None"
						row("Status") = "L2_Formulate_PGM"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
						
						If isInsert Then
'brapi.ErrorLog.LogMessage(si,"Inside if isinsert ") 
							REQDT.Rows.Add(row)
						End If
						
						'@Connor - Do we need to set this in Globals if we are passing into the DM Sequence also?
						globals.SetObject("REQ_ID_VAL",req_ID_Val)
						sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
						
						'@Connor - I think we should be able to kick off 1 DM Seq here and not call the Status Updates piece, just the piece that loads to cube and Consolidates
						'@Connor - Now for the PITA question....   What happens if they try to run the MPR Req Creation process more than once?  What is supposed to happen?
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")	
						Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
						Params.Add("CProbe",sSourcePosition)
						params.Add("REQ_ID_VAL",REQ_ID_VAL.ToString)
						params.Add("Entity",sEntity)
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Manpower", Params)
						
						Dim customSubstVars As New Dictionary(Of String, String) 
						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_PGM|L2_Formulate_PGM")
						customSubstVars.Add("EntList","E#" & sEntity)
						customSubstVars.Add("WFScen",sScenario)
						Dim currentYear As Integer = Convert.ToInt32(tm)
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
						
					End Using 
					End Using
					
				Return Nothing
			End Function		

#End Region

#Region "Save Adjust Funding Line "		
Public Function Save_AdjustFundingLine() As xfselectionchangedTaskResult
		
		Dim req_IDs As String = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_REQTitleList","NA")
		req_IDs = req_IDs.Split(" ").Last()

		'@Jeff Martin - Use GBLs for this entire block through 1992
		Dim WFInfoDetails As New Dictionary(Of String, String)()
	    Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
	    Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
		Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
	    WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
	    WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
	    WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
		WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
		
		Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

        ' ************************************
        ' *** Fetch Data for BOTH tables *****
        ' ************************************
        ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
        Dim dt As New DataTable()
        Dim sqa As New SqlDataAdapter()
        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
        Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND REQ_ID  = @REQ_ID"
        Dim sqlParams As SqlParameter() = New SqlParameter() {
            New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
            New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
            New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
			New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = req_IDs}
		}
          sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)
		  
		' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
        Dim dt_Details As New DataTable()
		Dim sqa2 As New SqlDataAdapter()
        Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
		Dim parentIds As New List(Of String)()
'Brapi.ErrorLog.LogMessage(si, "HERE 3:" & dt.Rows.Count)	
		For Each parentRow As DataRow In dt.Rows
'Brapi.ErrorLog.LogMessage(si, "HERE 4")	
		    If Not IsDBNull(parentRow("CMD_PGM_REQ_ID")) Then
		        ' Get the CMD_PGM_REQ_ID value and parse it as Guid
'Brapi.ErrorLog.LogMessage(si, "HERE 5")	
		        Dim columnValue As Object = parentRow("CMD_PGM_REQ_ID")
'Brapi.ErrorLog.LogMessage(si, "HERE 6")	
		        Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
		     
		        parentIds.Add(reqIdAsGuid.ToString())
		    End If
		Next	
    ' Select a single CMD_PGM_REQ_ID (assuming the requirement is only one ID for the query)
    	Dim singleCMD_PGM_REQ_ID As String = parentIds(0)
		Dim sql As String = ""
    ' Build the SQL query with the single CMD_PGM_REQ_ID
    	sql = $"SELECT *
           FROM XFC_CMD_PGM_REQ_Details
           WHERE WFScenario_Name = @WFScenario_Name
             AND WFCMD_Name = @WFCMD_Name
             AND WFTime_Name = @WFTime_Name
             AND CMD_PGM_REQ_ID = @SingleCMD_PGM_REQ_ID"

  	  ' Create the list of SQL parameters
	    Dim detailsParams As SqlParameter() = New SqlParameter(){
	        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
	        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
	        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
	        New SqlParameter("@SingleCMD_PGM_REQ_ID", SqlDbType.NVarChar) With {.Value = singleCMD_PGM_REQ_ID}
	    }
	
   		' Fill the details table with the query results
	    sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_DETAILS_DT(sqa2, dt_Details, sql, detailsParams)
	
	
		Dim sqa_xfc_cmd_pgm_req_details_audit = New SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT(sqlConn)
		Dim SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT = New DataTable()

		Dim SQL_Audit As String = $"SELECT * 
								FROM XFC_CMD_PGM_REQ_Details_Audit
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_PGM_REQ_ID = @SingleCMD_PGM_REQ_ID"

sqa_xfc_cmd_pgm_req_details_audit.Fill_XFC_CMD_PGM_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, SQL_Audit, detailsParams)
            ' ************************************
            ' ************************************
	
   		Dim targetRow As DataRow 											
		targetRow = dt.Select($"REQ_ID = '{req_IDs}'").FirstOrDefault()
		Dim APPN As String = args.NameValuePairs.XFGetValue("APPN")	
		Dim Entity As String = args.NameValuePairs.XFGetValue("Entity")
		Dim MDEP As String = args.NameValuePairs.XFGetValue("MDEP")
		Dim APE As String = args.NameValuePairs.XFGetValue("APEPT")
	    Dim DollarType As String = args.NameValuePairs.XFGetValue("DollarType")
	 	Dim cTypecol As String = args.NameValuePairs.XFGetValue("CType")
		Dim obj_class As String = args.NameValuePairs.XFGetValue("Obj_Class")
		Dim Status As String  = targetRow("Status")
		Dim Create_User As String = targetRow("Create_User")
		
		Dim req_ID_Val As Guid
		req_ID_Val = targetRow("CMD_PGM_REQ_ID") 	
		
		Dim targetRowFunding As DataRow
		targetRowFunding = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
												   
		
'----------------------------
'Audit Table Updates  -Update here before removing original vlaues 
'----------------------------
		If SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Rows.Count > 0 Then
			Dim drow As DataRow
			drow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
			drow("Orig_UD1") = targetRowFunding("UD1")
			drow("Updated_UD1") = APPN
			drow("Orig_UD2") = targetRowFunding("UD2")
			drow("Updated_UD2") = MDEP
			drow("Orig_UD3") = targetRowFunding("UD3")
			drow("Updated_UD3") = APE
			drow("Orig_UD4") = targetRowFunding("UD4")
			drow("Updated_UD4") = DollarType
			drow("Orig_UD5") = targetRowFunding("UD5")
			drow("Updated_UD5") = cTypecol
			drow("Orig_UD6") = targetRowFunding("UD6")
			drow("Updated_UD6") = obj_class
			drow("Orig_UD7") = "None"
			drow("Updated_UD7") = "None"
			drow("Orig_UD8") = "None"
			drow("Updated_UD8") = "None"
				
		Else
			Dim newrow As datarow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.NewRow()
			newrow("CMD_PGM_REQ_ID") = targetRowFunding("CMD_PGM_REQ_ID")
			newrow("WFScenario_Name") = targetRowFunding("WFScenario_Name")
			newrow("WFCMD_Name") = targetRowFunding("WFCMD_Name")
			newrow("WFTime_Name") = targetRowFunding("WFTime_Name")
			newrow("Entity") = targetRowFunding("Entity")
			newrow("Account") = "Req_Funding"
			newrow("Start_Year") = targetRowFunding("WFTime_Name")
			newrow("Orig_IC") = "None"
			newrow("Updated_IC") = "None"
			newrow("Orig_Flow") =  targetRowFunding("Flow")
			newrow("Updated_Flow") = targetRowFunding("Flow")
			newrow("Orig_UD1") = targetRowFunding("UD1")
			newrow("Updated_UD1") = APPN
			newrow("Orig_UD2") = targetRowFunding("UD2")
			newrow("Updated_UD2") = MDEP
			newrow("Orig_UD3") = targetRowFunding("UD3")
			newrow("Updated_UD3") = APE
			newrow("Orig_UD4") = targetRowFunding("UD4")
			newrow("Updated_UD4") = DollarType
			newrow("Orig_UD5") = targetRowFunding("UD5")
			newrow("Updated_UD5") = cTypecol
			newrow("Orig_UD6") = targetRowFunding("UD6")
			newrow("Updated_UD6") = obj_class
			newrow("Orig_UD7") = "None"
			newrow("Updated_UD7") = "None"
			newrow("Orig_UD8") = "None"
			newrow("Updated_UD8") = "None"
			newrow("Create_Date") = DateTime.Now
			newrow("Create_User") = si.UserName
		
			SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.rows.add(newrow)

			End If

			
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
		
	
		Dim targetRowFYValues As DataRow
		targetRowFYValues = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
		Dim FY1 As Decimal =  targetRowFYValues("FY_1")		 
		Dim FY2 As Decimal = targetRowFYValues("FY_2")	
		Dim FY3 As Decimal = targetRowFYValues("FY_3")
	    Dim FY4 As Decimal = targetRowFYValues("FY_4")	
	 	Dim FY5 As Decimal = targetRowFYValues("FY_5")
				

			If targetRowFunding IsNot Nothing Then
				targetRowFunding.Delete
			End If
		
			Dim targetRowFundingNew As DataRow = dt_Details.NewRow()
			targetRowFundingNew("CMD_PGM_REQ_ID") = req_ID_Val
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
		               
          	sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
            sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
			sqa_xfc_cmd_pgm_req_details_audit.Update_XFC_CMD_PGM_REQ_DETAILS_AUDIT(SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, sqa)
			
						Dim customSubstVars As New Dictionary(Of String, String) 
					    Dim workspaceid As guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")

						Dim FCList As New List (Of String)
						FCList.Add(Entity)
								
						Dim EntityLists  = GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_PGM")
						Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
						For Each JoinedEntity As String In joinedentitylist
							globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", Status)	
							Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",APPN)
						Next
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
						customSubstVars.Add("EntList",BaseEntityList)
						customSubstVars.Add("ParentEntList",ParentEntityList)
						customSubstVars.Add("WFScen",WFInfoDetails("ScenarioName"))
						Dim currentYear As Integer = Convert.ToInt32(WFInfoDetails("TimeName"))
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
						
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
    Dim sWeightPrioritizationMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#RMW_Cycle_Config_Annual:T#" & sTime &":V#Periodic:A#GBL_Priority_Cat_Weight:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
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
	
	Dim reqTitle = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
	
	If reqTitle <> String.Empty Or Not String.IsNullOrEmpty(reqTitle)
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_PGM_REQDetailsShowHide","CMD_PGM_0_Body_REQDetailsMain")
	Else	
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_PGM_REQDetailsShowHide","CMD_PGM_0_Body_REQDetailsHide")
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
				Dim REQCMTDT As New DataTable()
				Dim Status As String = ""
				Dim StatusList As String = ""
				
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)

				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
					Dim sqa As New SqlDataAdapter()

					' Fill main REQ table
					Dim sqa_xfc_CMD_PGM_req = New SQA_XFC_CMD_PGM_REQ(connection)
					Dim sql As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ 
										WHERE WFScenario_Name = @WFScenario_Name
										AND WFCMD_Name = @WFCMD_Name
										AND WFTime_Name = @WFTime_Name"

					Dim paramList As New List(Of SqlParameter) From {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
					}
'brapi.ErrorLog.LogMessage(si,"here2")
					Dim sqa_xfc_CMD_PGM_req_details = New SQA_XFC_CMD_PGM_REQ_Details(connection)

					' Build details SQL using CMD_PGM_REQ_ID IN (...) so both queries filter by the same parent identifiers
					Dim detailSql As String = $"SELECT * 
												FROM XFC_CMD_PGM_REQ_Details AS Req
       											LEFT JOIN XFC_CMD_PGM_REQ AS Dtl
       											ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
												WHERE Req.WFScenario_Name = @WFScenario_Name
												AND Req.WFCMD_Name = @WFCMD_Name
												AND Req.WFTime_Name = @WFTime_Name"

					Dim detailParamList As New List(Of SqlParameter) From {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
					}
'brapi.ErrorLog.LogMessage(si,"here3")
					Dim sqa_xfc_CMD_PGM_req_cmt = New SQA_XFC_CMD_PGM_REQ_CMT(connection)
					Dim sql_cmt As String = $"SELECT * 
										From XFC_CMD_PGM_REQ_Cmt as Cmt
	   									LEFT JOIN XFC_CMD_PGM_REQ AS Req
										ON Req.CMD_PGM_REQ_ID = Cmt.CMD_PGM_REQ_ID
										WHERE 1=1"

					Dim paramListcmt As New List(Of SqlParameter) From {	
					}

					If Req_ID_List IsNot Nothing AndAlso Req_ID_List.Count > 0 Then
						If Req_ID_List.Count = 1 Then
							sql &= " AND REQ_ID = @REQ_ID"
							detailSql &= " AND Dtl.REQ_ID = @REQ_ID"
							sql_cmt &= " AND Req.REQ_ID = @REQ_ID"
							paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
							detailParamList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
							paramListcmt.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
						Else
							Dim paramNames As New List(Of String)
							For i As Integer = 0 To Req_ID_List.Count - 1
								Dim pname As String = "@REQ_ID" & i
								paramNames.Add(pname)
								paramList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
								detailParamList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
								paramListcmt.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
							Next
							sql &= $" AND REQ_ID IN ({String.Join(",", paramNames)})"
							detailSql &= $" AND Dtl.REQ_ID IN ({String.Join(",", paramNames)})"
							sql_cmt &= $" AND Req.REQ_ID IN ({String.Join(",", paramNames)})"
						End If
					End If
					sqa_xfc_CMD_PGM_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, sql, paramList.ToArray())
					sqa_xfc_CMD_PGM_req_details.Fill_XFC_CMD_PGM_REQ_DETAILS_DT(sqa, REQDetailDT, detailSql, detailParamList.ToArray())
					sqa_xfc_CMD_PGM_req_cmt.Fill_XFC_CMD_PGM_REQ_CMT_DT(sqa, REQCMTDT, sql_cmt, paramList.ToArray())
					' Mark rows for deletion in both tables
					Dim entitiesFromReqList As New List(Of String)
					For Each reqId As String In Req_ID_List
						Dim rows As DataRow() = REQDT.Select($"REQ_ID = '{reqId}'")
						Dim GUIDROW As String = rows.FirstOrDefault().Item("CMD_PGM_REQ_ID").ToString
					
						Dim ent As String = rows.FirstOrDefault().Item("Entity").ToString
						entitiesFromReqList.Add(ent)
						Status  = rows.FirstOrDefault().Item("Status").ToString
						If StatusList.XFEqualsIgnoreCase("") Then
							StatusList = Status
						Else
							If Not statuslist.XFContainsIgnoreCase(Status) Then
								statuslist += StatusList & "|" & Status
							End If
						End If 
						For Each row As DataRow In rows
							row.Delete()
						Next
						Dim detailRows As DataRow() = REQDetailDT.Select($"CMD_PGM_REQ_ID = '{GUIDROW}'")
						For Each drow As DataRow In detailRows
							drow.Delete()
						Next
						Dim cmtRows As DataRow() = REQCMTDT.Select($"CMD_PGM_REQ_ID = '{GUIDROW}'")
						For Each srow As DataRow In cmtRows
							srow.Delete()
						Next
						globals.SetStringValue($"FundsCenterStatusUpdates - {ent}", statuslist)
					Next
					' Persist changes back to DB
					sqa_xfc_CMD_PGM_req_details.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)
					sqa_xfc_CMD_PGM_req.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
					sqa_xfc_CMD_PGM_req_cmt.Update_XFC_CMD_PGM_REQ_CMT(REQCMTDT, sqa)
					Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")	
					Dim customSubstVars As New Dictionary(Of String, String)
					Dim FCList As New List (Of String)
					If Not String.IsNullOrWhiteSpace(sEntity) Then
						FCList.Add(sEntity)
					Else
						Dim entities As String
						entitiesFromReqList = entitiesFromReqList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
						entities = String.Join(",", entitiesFromReqList.Select(Function (r) "E#" & r))
						FCList.Add(entities)
					End If
					Dim EntityLists = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList)
					Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
					Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
					customSubstVars.Add("EntList",BaseEntityList)
					customSubstVars.Add("ParentEntList",ParentEntityList)
					customSubstVars.Add("WFScen", sScenario)
					Dim currentYear As Integer = Convert.ToInt32(tm)
					customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
					

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
				Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
				
				Dim ReqID As String = args.NameValuePairs.XFGetValue("REQ_ID","")
				If String.IsNullOrWhiteSpace(ReqID)
					Return Nothing
				Else 
			
					
	

				'Fill the DataTable 
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ 
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name
									AND REQ_ID  = @REQ_ID"
				
		
	    ' 2. Create a list to hold the parameters
	   Dim sqlParams As SqlParameter() = New SqlParameter(){
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
		New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = ReqID}
   		}
			sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa,dt, sql, sqlparams)
			
			Dim sREQ_ID_Val As String = String.Empty
			Dim REQ_ID_Val_guid As Guid = Guid.Empty
			
		If dt.Rows.Count > 0 Then
    sREQ_ID_Val = Convert.ToString(dt.Rows(0)("CMD_PGM_REQ_ID"))
    REQ_ID_Val_guid = sREQ_ID_Val.XFConvertToGuid
Else
    
    Return Nothing 
End If
			
			
		  Dim dt_Attachment As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderAttachment As New SQA_XFC_CMD_PGM_REQ_Attachment(sqlConn)
            Dim sqlAttach As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Attachment Where CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID"
		Dim sqlParamsAttach As SqlParameter() = New SqlParameter(){
			New SqlParameter("@CMD_PGM_REQ_ID", SqlDbType.uniqueidentifier) With {.Value = REQ_ID_Val_guid}
			}
			  sqaReaderAttachment.Fill_XFC_CMD_PGM_REQ_Attachment_DT(sqa2, dt_Attachment, sqlAttach, sqlParamsAttach)
			 
			  Dim Tatgetrow As DataRow
			  
			  'If dt_Attachment.rows.count.Equals(0) Then
		
			  Tatgetrow = dt_Attachment.NewRow()
			  dt_Attachment.Rows.Add(Tatgetrow)
'			  Else
			  
'			  Tatgetrow = dt_Attachment.Select($"CMD_PGM_REQ_ID = '{REQ_ID_Val_guid}'").FirstOrDefault()
'			  End If
			  
		Tatgetrow("CMD_PGM_REQ_ID") = REQ_ID_Val_guid
			
		Dim FilePath As String = args.NameValuePairs.XFGetValue("FilePath")
		Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
		
		Tatgetrow("Attach_File_Name") = fileInfo.XFFile.FileInfo.Name
		Tatgetrow("Attach_File_Bytes") = fileInfo.XFFile.ContentFileBytes
		
		
		
		sqaReaderAttachment.Update_XFC_CMD_PGM_REQ_Attachment(dt_Attachment,sqa2)
		
		
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
Dim FileName As String  = args.NameValuePairs.XFGetValue("File")
Dim File_Name_List As List(Of String) = StringHelper.SplitString(FileName, ",")



	 
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()
			Dim dt As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderAttachment As New SQA_XFC_CMD_PGM_REQ_Attachment(sqlConn)
            Dim sqlAttach As String = $"SELECT * From XFC_CMD_PGM_REQ_Attachment as Att
	   						LEFT JOIN XFC_CMD_PGM_REQ AS Req
							ON Req.CMD_PGM_REQ_ID = Att.CMD_PGM_REQ_ID
							WHERE 
	  					 	REQ_ID = @REQ_ID
							"
		
			
	Dim paramList As New List(Of SqlParameter)
        paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = sREQ})
		
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
		sqaReaderAttachment.Fill_XFC_CMD_PGM_REQ_Attachment_DT(sqa2, dt, sqlAttach, sqlParamsAttach)
						
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
            sFileName = $"Attachments_{sREQ}.zip"
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
Dim paramsToClear As String = "ML_CMD_PGM_FormulateAPEPT," & _
														"ML_CMD_PGM_FormulateAPPN," & _
														"ML_CMD_PGM_FormulateCType," & _
														"ML_CMD_PGM_FormulateMDEP," & _
														"ML_CMD_PGM_FormulateDollarType," & _
														"ML_CMD_PGM_FormulateObjectClass," & _
														"ML_CMD_PGM_FormulateSAG" 
							
						
							
							
							
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
Dim paramsToClear As String = "ML_CMD_PGM_FormulateAPEPT," & _
														"ML_CMD_PGM_FormulateCType," & _
														"ML_CMD_PGM_FormulateMDEP," & _
														"ML_CMD_PGM_FormulateDollarType," & _
														"ML_CMD_PGM_FormulateObjectClass," & _
														"ML_CMD_PGM_FormulateSAG" 
							

						Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Me.ClearSelections(si, globals, api, args, paramsToClear)	
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
						selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
						Return selectionChangedTaskResult

    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function
#End Region	

#Region "Send Email"
Public Function SendStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs,  Optional ByVal Status As String = "",Optional ByVal ReqID As String = "")
    Try
        ' Get Workflow Details for the SQL Query
        Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
        Dim UserName As String = si.UserName
        ' Initialize DataTable to hold results
        Dim DataEmail As New DataTable()
        
        'Brapi.ErrorLog.LogMessage(si,"Status" & Status)
		' Brapi.ErrorLog.LogMessage(si,"ID" & ReqID)
        ' --- 2. DATABASE CONNECTION & QUERY EXECUTION ---
        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                sqlConn.Open()
                
                Dim SqlString As String = "SELECT Title, Status, Create_User, Create_Date, Notification_List_Emails, Entity 
                                          From XFC_CMD_PGM_REQ 
                                          Where WFScenario_Name = @WFScenario_Name 
                                          And WFCMD_Name = @WFCMD_Name 
                                          And WFTime_Name = @WFTime_Name
										  AND REQ_ID = @REQ_ID"
                
               Dim paramlist As New List(Of SqlParameter)()
                
                paramlist.Add(New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")})
                paramlist.Add(New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")})
                paramlist.Add(New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")})
				paramlist.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = ReqID})
                
               
                
                ' Execute the Query and fill the DataTable
                Using sqlCommand As New SqlCommand(SqlString, sqlConn)
                    sqlCommand.Parameters.AddRange(paramlist.ToArray())
                    
                    Using sqa As New SqlDataAdapter(sqlCommand)
                        sqa.Fill(DataEmail)
                    End Using
                End Using
            End Using 
        End Using 
        
        
        ' --- 3. PROCESS DATA & SEND EMAIL ---
        
        ' Get Email Parameters
        Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
        Dim BodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
        
        For Each drow As DataRow In DataEmail.Rows 
            
            Dim ReqTitle As String = drow("Title").ToString()
            Dim ReqStatus As String = Status
            Dim CreateDate As String = drow("Create_Date").ToString()
            Dim FundCenter As String = drow("Entity").ToString()
            Dim StatusChangeEmailIs As New List(Of String)
            Dim EmailIs As String = drow("Notification_List_Emails").ToString()
			
			    If Not String.IsNullOrWhiteSpace(EmailIs) Then

			        ' Define the potential delimiters to check 
			        Dim potentialDelimiters As Char() = {","c, ";"c, "|"c, " "c}
			       
			        Dim emailDelimiter As Char = DetectDelimiter(EmailIs, potentialDelimiters)
			        
			        ' Split the string using the detected delimiter
			        Dim emailArray As String() = EmailIs.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries)
			        
					' Trim whitespace from each email address and add it to the List(Of String)
			        For Each singleEmail As String In emailArray
			            Dim trimmedEmail As String = singleEmail.Trim()
			            If Not String.IsNullOrWhiteSpace(trimmedEmail) Then
			                StatusChangeEmailIs.Add(trimmedEmail)
			            End If
			        Next
			    End If
            
            Dim StatusChangeSubject As String = "Requirement Status Change"
            
            ' Build Body
            Dim StatusChangeBody As String = "A Requirement Request for Funds Center: " & FundCenter & _
                                             " with Requirement Title: " & ReqTitle & _
                                             " has changed status to **" & ReqStatus & "**." & _
                                             " Submitted by: " & UserName & " - " & CreateDate & vbCrLf & BodyDisclaimerBody
            
            ' Send email
            If StatusChangeEmailIs.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(EmailConnectorStr) Then
               ' Brapi.Errorlog.LogMessage(si, "Attempting to send status change email to: " & EmailIs)
                
                ' Call the utility function to send the email
               ' Brapi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmailIs, StatusChangeSubject, StatusChangeBody, Nothing)
                
               'Brapi.Errorlog.LogMessage(si, "Successfully triggered email for Request Title: " & ReqTitle)
            End If
            
        Next
        
        Return Nothing 
        
   Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try            
    
End Function
#End Region

#Region "Helper Fnctions"
' Helper function to find the delimiter used in a string
Private Function DetectDelimiter(ByVal inputString As String, ByVal potentialDelimiters As Char()) As Char
    ' Default to the first potential delimiter if none are found, or a comma if the list is empty
    Dim defaultDelimiter As Char = If(potentialDelimiters IsNot Nothing AndAlso potentialDelimiters.Length > 0, potentialDelimiters(0), ","c)

    If String.IsNullOrWhiteSpace(inputString) Then
        Return defaultDelimiter
    End If

    For Each delimiter As Char In potentialDelimiters
        ' Check if the string contains the delimiter
        If inputString.Contains(delimiter) Then
            ' Check if splitting yields more than one non-empty entry
            If inputString.Split(delimiter).Length > 1 Then
                Return delimiter
            End If
        End If
    Next

    Return defaultDelimiter
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
		
#End Region		
		
		
	End Class

End Namespace