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
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args	
'brapi.ErrorLog.LogMessage(si,"CMD SPLN HELPER MAIN")					
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
							Case "send_status_change_email"
								'dbExt_ChangedResult = Me.Send_Status_Change_Email()
								Return dbExt_ChangedResult
							Case "submit_reqs", "importreq", "manage_req_status", "DemotePackage"
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
'BRApi.ErrorLog.LogMessage(si,"In withhold main call")								
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
								dbExt_ChangedResult = Me.packagesubmission()
								Return dbExt_ChangedResult
								
							Case "setdeafultappnparam"
								dbExt_ChangedResult = Me.SetDeafultAPPNParam()
								Return dbExt_ChangedResult
							Case "demotereqpackage"
								Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
								Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
								Dim sNewStatus As String = args.NameValuePairs.XFGetValue("new_Status")
								Dim sDemoteComment As String = args.NameValuePairs.XFGetValue("demoteComment")		
								If String.IsNullOrEmpty(sEntity) Or String.IsNullOrEmpty(sAPPN) Or String.IsNullOrEmpty(sNewStatus) Or String.IsNullOrEmpty(sDemoteComment)  Then
									dbExt_ChangedResult.IsOK = False
									dbExt_ChangedResult.ShowMessageBox = True
									dbExt_ChangedResult.Message = vbCRLF & "All fields must be selected before demoting a requirement package." & vbCRLF	
									Return dbExt_ChangedResult
								End If
								dbExt_ChangedResult = Me.Update_Status()
								Return dbExt_ChangedResult		
								
							Case "copycmdsplntohqspln"
								Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")		
								If String.IsNullOrEmpty(sAPPN) Then
									dbExt_ChangedResult.IsOK = False
									dbExt_ChangedResult.ShowMessageBox = True
									dbExt_ChangedResult.Message = vbCRLF & "Please select a package from the cube view before copying to HQ spend plan." & vbCRLF	
									Return dbExt_ChangedResult
								End If								
								dbExt_ChangedResult = Me.CopyCMDSPLNToHQSPLN()
								Return dbExt_ChangedResult	
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
						#Region "Clear Prompts"
						If args.FunctionName.XFEqualsIgnoreCase("ClearPrompts") Then
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'BRApi.ErrorLog.LogMessage(si,"test KN")
							'Me.ClearPrompts(si,globals,api,args)
						End If
						#End Region						
						#Region "REQ_UpdateFilterLists Rollover"
						If args.FunctionName.XFEqualsIgnoreCase("REQ_UpdateFilterListsRollOver") Then
							Dim tStart As DateTime = Date.Now()
							
							'----- get req list -----
							Dim origcbxEntity As String = args.NameValuePairs.XFGetValue("cbxEntity")
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Fundcenter")
							'---------- get Entity in proper format ----------
							Dim StringArgs As DashboardStringFunctionArgs = New DashboardStringFunctionArgs
							StringArgs.FunctionName = "Remove_General"
							StringArgs.NameValuePairs.XFSetValue("Entity", origcbxEntity)
							Dim scbxEntity As String = GEN_General_String_Helper.Main(si, globals, api, StringArgs)
							Dim ListOfREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=REQ_Member_Lists, MemberListName=GetAllReqListRolloverCBX, TimeOffset=Prior, Caller=REQ_SolutionHelperRollOver, test=testing,FlowMbr=Top, mode=CVResult, ReturnDim=E#F#U1#U2#U3#U4#, cbxEntity = " & scbxEntity & ".Base, Page= , EntityFilter=[" & sFundCenter & "] )", False)
							
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
					
					Dim REQ_ID As String = sREQ.Split(" "c)(1)
					Dim Mainreqrow As DataRow = SQA_XFC_CMD_SPLN_REQ_DT.Select($"REQ_ID ='{REQ_ID}'").FirstOrDefault()
					Dim Existingstakeholders As String = Mainreqrow("Notification_List_Emails").ToString()
					
					Dim stakeholderlist As New List(Of String)(existingStakeholders.Split(","c, StringSplitOptions.RemoveEmptyEntries).Select(Function(e) e.Trim()))
					For Each newEmail As String In stakeholderEmailList
						If Not stakeholderList.Any(Function(e) e.Equals(newEmail.Trim(), StringComparison.OrdinalIgnoreCase)) Then
							stakeholderList.Add(newEmail.Trim())
						End If
					Next
					
					Mainreqrow("Notification_List_Emails") = String.Join(",", stakeholderList)
					
					Dim existingValidators As String = Mainreqrow("Validation_List_Emails").ToString()
					Dim validatorList As New List(Of String)(existingValidators.Split(","c, StringSplitOptions.RemoveEmptyEntries).Select(Function(e) e.Trim()))
					
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
		
#Region "Package Submission"
Public Function PackageSubmission() As Object
			Try
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim wfProfileName As String = wfInfoDetails("ProfileName")
				Dim wfProfileNameAdj As String = wfProfileName.Split("."c)(1).Split(" "c)(0)
				
				Dim wfCube = wfInfoDetails("CMDName")
				Dim wfScenario As String = wfInfoDetails("ScenarioName")
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = wfInfoDetails("TimeName")
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim Entity_APPN = args.NameValuePairs.XFGetValue("Entity_APPN")

				Dim lEntity_APPN As List (Of String) =  StringHelper.SplitString(Entity_APPN, ",")

				Dim Entity As String 
				Dim APPN As String
				Dim Flow As String
				Dim Errors As String
				Dim packagerror = New Dictionary(Of String,String)
				Dim scenFilter As String = String.empty
				
				For Each EA As String In lEntity_APPN

					Dim NameValuePairs = New Dictionary(Of String,String)
					Dim newstatus As String = String.Empty
					Entity = EA.Split(":")(0)
					APPN = EA.Split(":")(1)
					Flow = EA.Split(":")(2)	
					
					Dim EobjDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_Army")
					Dim EDesc As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EobjDimPk, "E#" & Entity & ".descendantsinclusive", True)
					Dim EntList As New List(Of String)
					For Each mementity As MemberInfo In EDesc
						EntList.Add(mementity.Member.Name)					
					Next
					
					Dim inEntClause As String = $"'{String.Join("','",EntList)}'"

					'Make this conditional, so if it's onesy/twosy, the reqID list would be passed to us, otherwise, we should use funds center & the fund codes related to the APPN selected.
					Dim sql As String = $"select distinct req_id 
										from XFC_CMD_SPLN_REQ 
										where Entity In ({inEntClause}) 
										and (APE9 like '{APPN}%') 
										and WFScenario_Name = '{wfscenario}' 
										and status = '{flow}'"			
					Dim REQ_ID_DT As DataTable
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						 REQ_ID_DT = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
					End Using
					Dim req_id_list As New List (Of String)

					If REQ_ID_DT.Rows.Count = 0 Then Continue For 
						
					For Each row As DataRow In req_id_dt.rows
						req_id_list.add(row.Item("req_id"))
					Next
					newstatus = Flow

					'Not sure I fully follow what this is doing.
					If newstatus.XFContainsIgnoreCase("L4_Approve") Then newstatus = "Validate"
					If newstatus.XFContainsIgnoreCase("L3_Approve") Then newstatus = "Validate"
					If newstatus.XFContainsIgnoreCase("L2_Approve") Then newstatus = "Final"
					If newstatus.XFContainsIgnoreCase("Formulate") Then newstatus = "Validate"
					If newstatus.XFContainsIgnoreCase("L5_Validate") Then newstatus = "Validate"
					If newstatus.XFContainsIgnoreCase("L4_Validate") Then newstatus = "Validate"
					If newstatus.XFContainsIgnoreCase("L3_Validate") Then newstatus = "Approve"
					If newstatus.XFContainsIgnoreCase("L2_Validate") Then newstatus = "Approve"
					args.NameValuePairs.Clear
					Args.NameValuePairs.Add("req_IDs",String.Join(",",req_id_list))
					Args.NameValuePairs.Add("fc_List", inEntClause)
					Args.NameValuePairs.Add("new_Status", newstatus)
					Args.NameValuePairs.Add("appn", APPN)
					BRapi.ErrorLog.LogMessage(si,$"Hit End of Package Submission {Entity} - {Appn} - {Flow}")
					Me.Update_Status()

				Next
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	
#End Region

#Region "DemoteREQPackage"
Public Function DemoteREQPackage()
	Try	
		Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
		Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
		Dim sNewStatus As String = args.NameValuePairs.XFGetValue("newStatus")
		Dim sDemoteComment As String = args.NameValuePairs.XFGetValue("demoteComment")
	    Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim req_id_list As New List (Of String)
		Dim REQ_ID_DT As DataTable		
		Dim sql As String = $"select distinct req_id 
							from XFC_CMD_SPLN_REQ 
							where Entity = '{sEntity}' 
							and (APE9 like '{sAPPN}%') 
							and WFScenario_Name = '{wfscenario}'"
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 REQ_ID_DT = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
		End Using
		For Each row As DataRow In req_id_dt.rows
			req_id_list.add(row.Item("req_id"))
		Next	

		Dim NameValuePairs = New Dictionary(Of String,String)
		args.NameValuePairs.Clear
		Args.NameValuePairs.Add("req_IDs", String.Join(",",req_id_list))
		Args.NameValuePairs.Add("new_Status", sNewStatus)
		Args.NameValuePairs.Add("demotecomment", sDemoteComment)
		Args.NameValuePairs.Add("fc_List",$"'{sEntity}'")
		Args.NameValuePairs.Add("appn", sAPPN)
		Args.NameValuePairs.Add("Demote", "True")
		
		Me.Update_Status()		

	Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
	Return Nothing	
End Function
#End Region


#Region "Update Status"
		Public Function Update_Status() As xfselectionchangedTaskResult
			
			Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
			'I think we should pass in req_IDs for the onesy/twosy submissions/approvals, but use the FC/APPN.  I think that will help improve performance and we can add an index around that.
			Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs","NA")
			Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
			Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim wfStepAllowed As Boolean = True
			
			Try
				If wfInfoDetails("ProfileName").XFContainsIgnoreCase("Formulate") And Not Dashboard.XFContainsIgnoreCase("Mpr") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf  wfInfoDetails("ProfileName").XFContainsIgnoreCase("Validate") Then
					Me.Update_REQ_Status("Validate")
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Import") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Rollover") Then
					Me.Update_REQ_Status("Formulate")
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Approve CMD Requirements") Then
					Me.Update_REQ_Status("Approve CMD")
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Formulate CMD Requirements") And Dashboard.XFContainsIgnoreCase("Mpr") Then
					Me.Update_REQ_Status("Formulate CMD")
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Approve Requirements") Then
					Me.Update_REQ_Status("Approve")
					'New - Fronz - Accounts for the Certify Dashboard
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase($"{wfInfoDetails("CMDName")} Review") Then
					Me.Update_REQ_Status("Final")					
				ElseIf wfInfoDetails("ProfileName").XFContainsIgnoreCase("Manage") Then
					Me.ManageREQStatusUpdated(si, globals, api, args, "")
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
			Return dbExt_ChangedResult
		End Function		''' <summary>
		''' Centralized helper to set a REQ workflow status, update history, send emails, and set last updated.
		''' </summary>
		Private Function Update_REQ_Status(ByVal curr_Status As String) As xfselectionchangedTaskResult
			Try			
				Dim tStart1 As DateTime =  Date.Now()
				Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
				Dim demote_comment As String = args.NameValuePairs.XFGetValue("demotecomment")
				Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
				Dim PrevStatus As String = args.NameValuePairs.XFGetValue("PrevStatus")
				Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs","NA")
				Dim Entity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim flow As String = args.NameValuePairs.XFGetValue("Flow","NA")
				Dim Appn As String = args.NameValuePairs.XFGetValue("appn")
				Dim Entity_APPN_Flow = args.NameValuePairs.XFGetValue("Entity_APPN_Flow","NA")
				Dim lEntityAppnFlow As New List(Of String)
				Dim Mode As String = args.NameValuePairs.XFGetValue("Mode")
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{wfInfoDetails("CMDName")}")
				Dim FCList As New List(Of String)
				Dim Status_manager As New Dictionary(Of String,String)
				Dim PackageList As New List(Of String)		
				Dim packageFilter As String = String.Empty
				Dim packageDetailFilter As String = String.empty
				Dim packageAuditFilter As String = String.empty
				Dim statusList As New List(Of String)
				Dim APPNList As New List(Of String)
				Dim sql As String = String.empty
				Dim currDBsql As String = String.empty
				Dim Detailsql As String = String.empty
				Dim currDBDetailsql As String = String.empty
			
					If Entity_APPN_Flow = "NA" Then 
						Entity_APPN_Flow = $"{Entity}:{APPN}:{PrevStatus}"		
					End If			
					lEntityAppnFlow =  StringHelper.SplitString(Entity_APPN_Flow, ",")
					
					#Region "Promote Statuses"
					Status_manager.Add("L5_Formulate_SPLN|Base","L5_Validate_SPLN")
					Status_manager.Add("L4_Formulate_SPLN|Parent","L4_Validate_SPLN")
					Status_manager.Add("L4_Formulate_SPLN|Base","L4_Validate_SPLN")
					Status_manager.Add("L3_Formulate_SPLN|Parent","L3_Validate_SPLN")
					Status_manager.Add("L3_Formulate_SPLN|Base","L3_Validate_SPLN")
					Status_manager.Add("L2_Formulate_SPLN|Parent","L2_Validate_SPLN")
					Status_manager.Add("L2_Formulate_SPLN|Base","L2_Validate_SPLN")
					Status_manager.Add("L5_Validate_SPLN|Base","L4_Validate_SPLN")
					Status_manager.Add("L4_Validate_SPLN|Base","L3_Validate_SPLN")
					Status_manager.Add("L4_Validate_SPLN|Parent","L4_Approve_SPLN")
					Status_manager.Add("L3_Validate_SPLN|Parent","L3_Approve_SPLN")
					Status_manager.Add("L3_Validate_SPLN|Base","L3_Approve_SPLN")
					Status_manager.Add("L2_Validate_SPLN|Parent","L2_Approve_SPLN")
					Status_manager.Add("L2_Validate_SPLN|Base","L2_Approve_SPLN")
					Status_manager.Add("L2_Approve_SPLN|Parent","L2_Final_SPLN")
					Status_manager.Add("L2_Approve_SPLN|Base","L2_Final_SPLN")
					Status_manager.Add("L3_Approve_SPLN|Parent","L2_Validate_SPLN")
					Status_manager.Add("L3_Approve_SPLN|Base","L2_Validate_SPLN")
					#End Region
'brapi.ErrorLog.LogMessage(si,"Hit 2 CM: " & lEntityAppnFlow(0))
					For Each EAF As String In lEntityAppnFlow
						Dim E As String = EAF.Split(":")(0)
						Dim A As String = EAF.Split(":")(1)
						Dim F As String = EAF.Split(":")(2)	
						If Not statusList.Contains(F) Then
							statusList.Add(F)
						End If
						If Not APPNList.Contains(A) Then
							APPNList.Add(A)
						End If
						Dim EobjDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_Army")
						Dim U1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
						Dim EDesc As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EobjDimPk, "E#" & E & ".descendantsinclusive", True)
						Dim U1Base As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U1objDimPk, "U1#" & A & ".Base", True)
'brapi.ErrorLog.LogMessage(si,"Hit 3 CM")	
						Dim newReviewEntity As String
						For Each mementity As MemberInfo In EDesc.orderby(Function(m) m.Member.Name).ToList()
							PackageList.Clear()
							If mementity.Member.Name = E
								Dim ReviewEntList = BRApi.Finance.Members.GetParents(si,entDimPk,mementity.member.MemberId,False)
								If Not demote_comment = "" Then 
									Dim entityLevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, mementity.Member.Name)
									Dim DemoteLevel As String = new_status.Substring(0,2)
									If entityLevel = DemoteLevel Then 
										NewReviewEntity = mementity.Member.Name
									Else
										NewReviewEntity = BRApi.Finance.Members.GetMembersUsingFilter(si, EobjDimPk, "E#" & mementity.Member.Name & ".Ancestors.where(text3 contains " & DemoteLevel & ")", True)(0).Member.Name
									End If 
								Else
									newReviewEntity = ReviewEntList(0).Name
								End If 
							End If
							PackageList.Add($"'{mementity.member.name}|{A}|{F}'")	
							For Each memU1 As MemberInfo In U1Base
								PackageList.Add($"'{mementity.member.name}|{memU1.member.name}|{F}'")
							Next
							packageFilter = "AND CONCAT(Entity,'|',APPN,'|',Status) IN (" & String.Join(",", PackageList) & ")"
							packageDetailFilter = "AND CONCAT(Entity,'|',UD1,'|',Flow) IN (" & String.Join(",", PackageList) & ")"
							Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,mementity.member.MemberId)
							Dim entBaseParent As String
							If globals.GetStringValue($"{mementity.member.name}_Base","NA") = "NA"
								Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, mementity.Member.MemberId)
								If Ent_Base = True
									entBaseParent = "Base" 
								Else
									entBaseParent = "Parent"
								End If
								globals.SetStringValue($"{mementity.member.name}_Base",entBaseParent)
							Else
								entBaseParent = globals.GetStringValue($"{mementity.member.name}_Base","NA")
							End If

							If Not String.IsNullOrEmpty(new_status) Then 
								If Not statusList.Contains(new_status) Then
									statusList.Add(new_status)
								End If
							Else If	Status_manager.ContainsKey($"{F}|{entBaseParent}") Then
								new_status = Status_manager.XFGetValue($"{F}|{entBaseParent}")
								If Not statusList.Contains(new_status) Then
									statusList.Add(new_status)
								End If
							End If

						'	brapi.ErrorLog.LogMessage(si,$"Hit: {new_status} - {F}|{entBaseParent}")
							Dim reviewEntityString As String = "Review_Entity"
							If F.Substring(0,2) <> new_status.Substring(0,2)
									reviewEntityString = $"'{newReviewEntity}' as Review_Entity"
							End If
							If sql.Length > 0
								sql = $"{sql}
										UNION ALL
										SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, '{new_Status}' as Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, {reviewEntityString}, REQ_ID_Type, '{demote_comment}' as Demotion_Comment,
														    Related_REQs
														FROM dbo.XFC_CMD_SPLN_REQ
														WHERE WFScenario_Name = @WFScenario_Name
														AND WFCMD_Name = @WFCMD_Name
														AND WFTime_Name = @WFTime_Name
														{packageFilter}"
								currDBsql = $"{currDBsql}
										UNION ALL
										SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, Review_Entity, REQ_ID_Type, Demotion_Comment,
														    Related_REQs
														FROM dbo.XFC_CMD_SPLN_REQ
														WHERE WFScenario_Name = @WFScenario_Name
														AND WFCMD_Name = @WFCMD_Name
														AND WFTime_Name = @WFTime_Name
														{packageFilter}"
								Detailsql = $"{Detailsql}
											UNION ALL 
											SELECT
											CMD_SPLN_REQ_ID,WFScenario_Name,WFCMD_Name,WFTime_Name,Unit_of_Measure,
											Entity,IC,Account,'{new_Status}' as Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,UD8,
											Fiscal_Year,Month1,Month2,Month3,Month4,Month5,Month6,
											Month7,Month8,Month9,Month10,Month11,Month12,
											Quarter1,Quarter2,Quarter3,Quarter4,Yearly,
											AllowUpdate,Create_Date,Create_User,Update_Date,Update_User
											FROM dbo.XFC_CMD_SPLN_REQ_Details
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											{packageDetailFilter}"
								
								currDBDetailSQL = $"{currDBDetailSQL}
												UNION ALL SELECT
												CMD_SPLN_REQ_ID,WFScenario_Name,WFCMD_Name,WFTime_Name,Unit_of_Measure,
												Entity,IC,Account,Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,UD8,
												Fiscal_Year,Month1,Month2,Month3,Month4,Month5,Month6,
												Month7,Month8,Month9,Month10,Month11,Month12,
												Quarter1,Quarter2,Quarter3,Quarter4,Yearly,
												AllowUpdate,Create_Date,Create_User,Update_Date,Update_User
												FROM dbo.XFC_CMD_SPLN_REQ_Details
												WHERE WFScenario_Name = @WFScenario_Name
												AND WFCMD_Name = @WFCMD_Name
												AND WFTime_Name = @WFTime_Name
												{packageDetailFilter}"
							Else
								sql = $"SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, '{new_Status}' as Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, {reviewEntityString}, REQ_ID_Type, '{demote_comment}' as Demotion_Comment,
														    Related_REQs
														FROM dbo.XFC_CMD_SPLN_REQ
														WHERE WFScenario_Name = @WFScenario_Name
														AND WFCMD_Name = @WFCMD_Name
														AND WFTime_Name = @WFTime_Name
														{packageFilter}"
								currDBsql = $"SELECT CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID,
														    Title, Description, Justification, Entity, APPN, MDEP, APE9,
														    Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded,
														    Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src,
														    Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div,
														    Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission,
														    UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date,
														    POP_Exp_Date, CME, COR_Email, POC_Email, Review_POC_Email,
														    MDEP_Functional_Email, Notification_List_Emails, FF_1, FF_2, FF_3,
														    FF_4, FF_5, Status, Invalid, Val_Error, Create_Date, Create_User,
														    Update_Date, Update_User, Review_Entity, REQ_ID_Type, Demotion_Comment,
														    Related_REQs
														FROM dbo.XFC_CMD_SPLN_REQ
														WHERE WFScenario_Name = @WFScenario_Name
														AND WFCMD_Name = @WFCMD_Name
														AND WFTime_Name = @WFTime_Name
														{packageFilter}"
								Detailsql = $"SELECT
											CMD_SPLN_REQ_ID,WFScenario_Name,WFCMD_Name,WFTime_Name,Unit_of_Measure,
											Entity,IC,Account,'{new_Status}' as Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,UD8,
											Fiscal_Year,Month1,Month2,Month3,Month4,Month5,Month6,
											Month7,Month8,Month9,Month10,Month11,Month12,
											Quarter1,Quarter2,Quarter3,Quarter4,Yearly,
											AllowUpdate,Create_Date,Create_User,Update_Date,Update_User
											FROM dbo.XFC_CMD_SPLN_REQ_Details
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											{packageDetailFilter}"
								currDBDetailSQL = $"SELECT
												CMD_SPLN_REQ_ID,WFScenario_Name,WFCMD_Name,WFTime_Name,Unit_of_Measure,
												Entity,IC,Account,Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,UD8,
												Fiscal_Year,Month1,Month2,Month3,Month4,Month5,Month6,
												Month7,Month8,Month9,Month10,Month11,Month12,
												Quarter1,Quarter2,Quarter3,Quarter4,Yearly,
												AllowUpdate,Create_Date,Create_User,Update_Date,Update_User
												FROM dbo.XFC_CMD_SPLN_REQ_Details
												WHERE WFScenario_Name = @WFScenario_Name
												AND WFCMD_Name = @WFCMD_Name
												AND WFTime_Name = @WFTime_Name
												{packageDetailFilter}"
							End If
							'brapi.ErrorLog.LogMessage(si,"Hit 4 CM")
						Next
					Next
				
				Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()

					Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
					Using connection As New SqlConnection(dbConnApp.ConnectionString)
						connection.Open()
						Dim sqa_xfc_cmd_SPLN_req = New SQA_XFC_CMD_SPLN_REQ(connection)
						Dim SQA_XFC_CMD_SPLN_REQ_DT = New DataTable()
						Dim src_XFC_CMD_SPLN_REQ_DT = New DataTable()
						Dim sqa_xfc_cmd_SPLN_req_details = New SQA_XFC_CMD_SPLN_REQ_DETAILS(connection)
						Dim SQA_XFC_CMD_SPLN_REQ_DETAILS_DT = New DataTable()
						Dim src_XFC_CMD_SPLN_REQ_DETAILS_DT = New DataTable()
						Dim sqa_xfc_cmd_spln_req_details_audit = New SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT(connection)
						Dim SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT = New DataTable()
						Dim sqa = New SqlDataAdapter()
						Dim dtfilter As String = String.Empty
						Dim req_ID_Filter As String = String.Empty
						Dim paramNames As New List(Of String)
						
					    ' 2. Create a list to hold the parameters
					    Dim paramList As New List(Of SqlParameter) From {
				        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
				        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
				        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
				   		}
					    ' 3. Convert the list to the array your method expects
					    Dim sqlparams As SqlParameter() = paramList.ToArray()
		
						'IMPROVEMENT - Filter by fund center 
						sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DT, currDBsql, sqlparams)
						'details
						sqa_xfc_cmd_SPLN_req_Details.Fill_XFC_CMD_SPLN_REQ_DETAILS_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DETAILS_DT, currDBDetailSQL, sqlparams)
'						For Each r As DataRow In SQA_XFC_CMD_SPLN_REQ_DT.Rows()
'							r.Delete()
'						Next
						SQA_XFC_CMD_SPLN_REQ_DT.AcceptChanges()
						SQA_XFC_CMD_SPLN_REQ_DETAILS_DT.AcceptChanges()
						sqa.AcceptChangesDuringFill = False
						
						sqa_xfc_cmd_SPLN_req.Fill_XFC_CMD_SPLN_REQ_DT(sqa, src_XFC_CMD_SPLN_REQ_DT, sql, sqlparams)
						sqa_xfc_cmd_SPLN_req_Details.Fill_XFC_CMD_SPLN_REQ_DETAILS_DT(sqa, src_XFC_CMD_SPLN_REQ_DETAILS_DT, Detailsql, sqlparams)
						
						fclist = src_XFC_CMD_SPLN_REQ_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("Entity")).Distinct().ToList()
'						For Each ent As String In distinctEnt
'							fclist.Add(ent)	
'						Next
						'merge and sync
						sqa_xfc_cmd_SPLN_req.MergeandSync_XFC_CMD_SPLN_REQ_toDB(si,src_XFC_CMD_SPLN_REQ_DT,SQA_XFC_CMD_SPLN_REQ_DT)

						sqa_xfc_cmd_SPLN_req_Details.MergeandSync_XFC_CMD_SPLN_REQ_Details_toDB(si,src_XFC_CMD_SPLN_REQ_DETAILS_DT,SQA_XFC_CMD_SPLN_REQ_DETAILS_DT)

						
					End Using
		
				'End If	
'brapi.ErrorLog.LogMessage(si,$"DB Processes Complete {String.Join(",",FCList)}")
				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_SPLN")
				Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
				For Each JoinedEntity As String In joinedentitylist
					Dim GlobalAPPNs As String = String.Join("|",APPNList)
					Dim GlobalFlows As String = String.Join("|",StatusList)
					globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", GlobalFlows)	
					Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",GlobalAPPNs)
				Next
				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))			
				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("EntList",String.Join(",",BaseEntityList))
				customSubstVars.Add("ParentEntList",String.Join(",",ParentEntityList))
				customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
				Dim currentYear As String = wfInfoDetails("TimeName")
				Dim nextyear As String = currentYear + 1
				customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")

				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)				
				
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
							

			Return Nothing				
			
		End Function 

		
#End Region

#Region "Chunck REQ List"
		Public Function ChunckREQList(reqList As List(Of String), size As Integer) As List(Of List(Of String))
			Dim chuncks As New List(Of List(Of String))()
			For i As Integer = 0 To reqList.Count - 1 Step size
				chuncks.Add(reqList.Skip(i).Take(size).ToList())
			Next
			Return chuncks
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

#Region "Helper Methods"	

#Region "Set Default APPN Parameter"
Public Function SetDeafultAPPNParam() As Object

	Dim dKeyVal As New Dictionary(Of String, String)
							
	dKeyVal.Add("ML_CMD_SPLN_FormulateAPPN","OMA")
	
	If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_SPLN_RelatedREQList","NA") <> "NA" Then
		dKeyVal.Add("BL_CMD_SPLN_RelatedREQList",args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_SPLN_RelatedREQList","NA"))
	End If	
'Added 2 line to clear user cache before launching getcascadingfilters
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_SPLN_CascadingFilterCache",$"CMD_SPLN_rebuildparams_APPN","")
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_SPLN_CascadingFilterCache",$"CMD_SPLN_rebuildparams_Other","")	
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
'This called used in the btn_CMD_TGT_Copy_CivPay_to_SPLN from the CMD_TGT Module. The process is used to copy Civpay from TGT to the CMD SPLN module hence why its sitting in CMD_SPLN Assembly

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
		       	'Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            'Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                'qlConn.Open()
						'Main

'BRApi.ErrorLog.LogMessage(si, SqlREQ & "sEntity: " & sEntity & ", FCList:" & String.Join(", ", FCList))
						'Writting to Tables
						Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
						Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
						params.Add("Entity",sEntity)
						Dim sTargetScenario As String = "CMD_SPLN_C" & tm
						params.Add("WFScen",sTargetScenario)
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_CivPay", Params)
						
'		                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
'		                Dim SqlREQ As String = $"SELECT * 
'											FROM XFC_CMD_SPLN_REQ (NOLOCK)
'											WHERE WFCMD_Name = '{cmd}'
'											And WFTime_Name = '{tm}'
'											AND WFScenario_Name = '{sScenario}'
'											AND ENTITY = '{sEntity}'
'											--AND REQ_ID LIKE '%{sAPPN}%'
'						 					AND REQ_ID_Type = 'CivPay'"
		
'						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
'		                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ) 
'						Dim FCList As New List(Of String)
'						For Each drow As DataRow In REQDT.Rows
'							If Not FCList.contains($"{drow("Entity")}")
'									FCList.Add($"{drow("Entity")}")
'							End If 
'						Next
						BRApi.ErrorLog.LogMessage(si,"Hit here")
						Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_Army")
						Dim entityList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & sEntity & ".Children", True)
						Dim FCList As New List(Of String)
						FCList.Add(sEntity)
						For Each mbr As MemberInfo In entityList
							FCList.Add(mbr.Member.Name)
						Next
						
						'call the status update to write to cube
						Dim customSubstVars As New Dictionary(Of String, String) 
						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_SPLN|L3_Formulate_SPLN|L4_Formulate_SPLN|L5_Formulate_SPLN")
						
						Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList)
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
						customSubstVars.Add("EntList",BaseEntityList)
						customSubstVars.Add("WFScen",sTargetScenario)
						
						customSubstVars.Add("ParentEntList", ParentEntityList)
						Dim currentYear As Integer = Convert.ToInt32(tm)
						Dim curYear As String = $"T#{currentYear.ToString()}"
						Dim months As String = ""
						For i As Integer = 1 To 12
							months = months & curYear & "M" & i & ","
						Next
						dim nextyear as Integer = currentYear  + 1
						months = months & $"T#{nextyear}"
						'months = months.Substring(0, months.Length-1)
brapi.ErrorLog.LogMessage(si,"Hit" & months)
						customSubstVars.Add("WFTime",months)
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)

						'Run consolidation for entities in the correct order
						Dim consolEntityList As list (Of String) = EntityLists.Item3
						consolEntityList.Add(sEntity)
						Dim L4Entity,L3Entity,L2Entity,L1Entity As String 
						For Each consolentity As String In consolEntityList
							Dim Entitylevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, consolentity)
							If consolentity = sCube Then EntityLevel = "L1"
							Select Case Entitylevel
							Case "L4"
								L4Entity = $"{L4Entity},E#{ConsolEntity}"
							Case "L3"
								L3Entity = $"{L3Entity},E#{ConsolEntity}"
							Case "L2"
								L2Entity = $"{L2Entity},E#{ConsolEntity}"
							Case "L1"
								L1Entity = $"{L1Entity},E#{ConsolEntity}"
							End Select
						Next
						customSubstVars.Add("consolEntityList","Default")
						If Not String.IsNullOrEmpty(L4Entity) Then
							customSubstVars("consolEntityList") = L4Entity 
							BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
						End If 
						If Not String.IsNullOrEmpty(L3Entity) Then
							customSubstVars("consolEntityList") = L3Entity 
							BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
						End If 
						If Not String.IsNullOrEmpty(L2Entity) Then
							customSubstVars("consolEntityList") = L2Entity 
							BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
						End If 
						If Not String.IsNullOrEmpty(L1Entity) Then
							customSubstVars("consolEntityList") = L1Entity 
							BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
						End If 
						'End Using 
					'End Using
					
				Return Nothing
			End Function		

#End Region

#Region "Create Withhold REQ"
'This called used in the btn_CMD_TGT_Copy_WH_to_SPLN from the CMD_TGT Module. The process is used to copy withholds from TGT to the CMD SPLN module hence why its sitting in CMD_SPLN Assembly
			Public Function CreateWithholdREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
'BRApi.ErrorLog.LogMessage(si,"In Withhold BR")				
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
				Dim account As String = "TGT_WH"
				'Check if Fundcenter or APPN dropdown is empty
				Dim return_message As String = ""
				If String.IsNullOrWhiteSpace(sEntity) Then
					return_message = "Please select a FundCenter to copy Withholds to CMD SPLN"
					Throw New XFUserMsgException(si, New Exception(return_message))
				End If
				
				Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
				Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
				'params.Add("APPN",sAPPN)
				'params.Add("REQ_ID_VAL",REQ_ID_VAL.ToString)
				params.Add("Entity",sEntity)
				Dim sTargetScenario As String = "CMD_SPLN_C" & tm
				params.Add("WFScen",sTargetScenario)
'brapi.ErrorLog.LogMessage(si,"before execute DM Withhold: sTargetScenario= " & sTargetScenario & ", sEntity= " & sEntity) 						
				BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Withhold", Params)
				
				
				'call the status update to write to cube
				Dim customSubstVars As New Dictionary(Of String, String) 
				globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_SPLN|L3_Formulate_SPLN|L4_Formulate_SPLN|L5_Formulate_SPLN")
				
				Dim FCList As New List(Of String)
				FCList.Add(sEntity)
				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList)
				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
				customSubstVars.Add("EntList",BaseEntityList)
				customSubstVars.Add("WFScen",sTargetScenario)
				
				customSubstVars.Add("ParentEntList", ParentEntityList)
				Dim currentYear As Integer = Convert.ToInt32(tm)
				Dim curYear As String = $"T#{currentYear.ToString()}"
				Dim months As String = ""
				For i As Integer = 1 To 12
					months = months & curYear & "M" & i & ","
				Next
				Dim nextyear As Integer = currentYear  + 1
				months = months & $"T#{nextyear}"
				'months = months.Substring(0, months.Length-1)

				customSubstVars.Add("WFTime",months)
				BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)

				'Run consolidation for entities in the correct order
				Dim consolEntityList As list (Of String) = EntityLists.Item3
				consolEntityList.Add(sEntity)
				Dim L4Entity,L3Entity,L2Entity,L1Entity As String 
				For Each consolentity As String In consolEntityList
					Dim Entitylevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, consolentity)
					If consolentity = sCube Then EntityLevel = "L1"
					Select Case Entitylevel
					Case "L4"
						L4Entity = $"{L4Entity},E#{ConsolEntity}"
					Case "L3"
						L3Entity = $"{L3Entity},E#{ConsolEntity}"
					Case "L2"
						L2Entity = $"{L2Entity},E#{ConsolEntity}"
					Case "L1"
						L1Entity = $"{L1Entity},E#{ConsolEntity}"
					End Select
				Next
				customSubstVars.Add("consolEntityList","Default")
				If Not String.IsNullOrEmpty(L4Entity) Then
					customSubstVars("consolEntityList") = L4Entity 
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
				End If 
				If Not String.IsNullOrEmpty(L3Entity) Then
					customSubstVars("consolEntityList") = L3Entity 
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
				End If 
				If Not String.IsNullOrEmpty(L2Entity) Then
					customSubstVars("consolEntityList") = L2Entity 
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
				End If 
				If Not String.IsNullOrEmpty(L1Entity) Then
					customSubstVars("consolEntityList") = L1Entity 
					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
				End If 
				Return Nothing
			End Function		

#End Region

#Region "zCreate Withhold REQ"
'This called used in the btn_CMD_TGT_Copy_WH_to_SPLN from the CMD_TGT Module. The process is used to copy withholds from TGT to the CMD SPLN module hence why its sitting in CMD_SPLN Assembly
			Public Function zCreateWithholdREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
'BRApi.ErrorLog.LogMessage(si,"In Withhold BR")				
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
				Dim row As DataRow
				Dim req_ID_Val As Guid
				Dim REQ_ID As String = ""
				Dim isInsert As Boolean = True				
					
				'Check if Fundcenter or APPN dropdown is empty
				Dim return_message As String = ""
				If String.IsNullOrWhiteSpace(sEntity) Then
					return_message = "Please select a FundCenter to copy Withholds to CMD SPLN"
					Throw New XFUserMsgException(si, New Exception(return_message))
				End If
				
				Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "50 CMD SPLN")	
						Dim Params As Dictionary(Of String, String) = New Dictionary (Of String, String)
						'params.Add("APPN",sAPPN)
						params.Add("REQ_ID_VAL",REQ_ID_VAL.ToString)
						params.Add("Entity",sEntity)
'brapi.ErrorLog.LogMessage(si,"before execute DM Withhold") 						
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Withhold", Params)
						
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
						 					AND REQ_ID_Type = 'Withhold'"
		
						Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
		                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ) 
'BRApi.ErrorLog.LogMessage(si,"SQL: " & SqlREQ)

					
						If Not REQDT.Rows.Count = 0 Then
'BRApi.ErrorLog.LogMessage(si,"in if id check")					
							Dim return_message_ID_Check As String = ""
							return_message_ID_Check = "The Withhold Requirement for Funds Center " + sEntity + " has already been created"
							Throw New XFUserMsgException(si, New Exception(return_message_ID_Check))
'							req_ID_Val = reqdt.Rows(0).Item("CMD_SPLN_REQ_ID")
'							req_id = reqdt.Rows(0).Item("REQ_ID")
'		                    isInsert = False
'		                    row = reqdt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
							
						 Else 	
							row = REQDT.NewRow()
							req_ID_Val = Guid.NewGuid
							Dim modifiedFC As String = sEntity
							If sEntity.Length = 3 Then modifiedFC = modifiedFC & "xx"
							req_id = modifiedFC + "_WH_00001"
						End If 
						
						row("WFScenario_Name") = sScenario
						row("WFTime_Name") = tm
						row("Title") =  sEntity + " Withhold Requirement"
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
						
'						If isInsert Then
'							REQDT.Rows.Add(row)
'						End If
						
'						globals.SetObject("REQ_ID_VAL",req_ID_Val)
'						sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
						
						
'						Dim customSubstVars As New Dictionary(Of String, String) 
'						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_SPLN|L2_Formulate_SPLN")
'						customSubstVars.Add("EntList","E#" & sEntity)					
'						customSubstVars.Add("WFScen",sScenario)
'						Dim currentYear As Integer = Convert.ToInt32(tm)
'						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
'						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
				
						
					End Using 
					End Using
					
				Return Nothing
			End Function		

#End Region

#Region "Save Adjust Funding Line "		
Public Function Save_AdjustFundingLine() As xfselectionchangedTaskResult
Dim req_IDs As String = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_SPLN_REQTitleList","NA")
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
			Dim WFYear As Integer = wfInfoDetails("TimeName")
			Dim nextyear As Integer = wfInfoDetails("TimeName") + 1
			Dim fclist As New List (Of String)
			Dim APPNList As New List (Of String)
			Dim StatusList As New List (Of String)
			
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
'brapi.ErrorLog.LogMessage(si,"dtdetailscount = " & dt_Details.Rows.count & ": " & sql & ": reqIDS " & singleCMD_SPLN_REQ_ID)			  
Dim sqa_xfc_cmd_spln_req_details_audit = New SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT(sqlConn)
		Dim SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT = New DataTable()

		Dim SQL_Audit As String = $"SELECT * 
								FROM XFC_CMD_SPLN_REQ_Details_Audit
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_SPLN_REQ_ID = @SingleCMD_SPLN_REQ_ID"

sqa_xfc_cmd_spln_req_details_audit.Fill_XFC_CMD_SPLN_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT, SQL_Audit, detailsParams)
            ' ************************************
            ' ************************************
	
    Dim targetRow As DataRow 											
	
	
			
			targetRow = dt.Select($"REQ_ID = '{req_IDs}'").FirstOrDefault()
			Dim APPN As String =  args.NameValuePairs.XFGetValue("APPN")
			Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
			Dim lsAncestorListU1 As List(Of MemberInfo)
			lsAncestorListU1 = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & APPN & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
			APPNList.add(lsAncestorListU1(0).Member.Name)
'Brapi.ErrorLog.LogMessage(si, "APPN " & APPN)	
			
			Dim Entity As String =  args.NameValuePairs.XFGetValue("Entity")
			fclist.Add(entity)
			Dim MDEP As String =  args.NameValuePairs.XFGetValue("MDEP")
			 Dim APE As String =  args.NameValuePairs.XFGetValue("APEPT")
		     Dim DollarType As String =  args.NameValuePairs.XFGetValue("DollarType")
		 	 Dim cTypecol As String =  args.NameValuePairs.XFGetValue("CType")
			 Dim obj_class As String =  args.NameValuePairs.XFGetValue("Obj_Class")
			Dim Status As String  = targetRow("Status")
			statuslist.Add(status)
			Dim Create_User As String = targetRow("Create_User")
			
			Dim req_ID_Val As Guid
			req_ID_Val = targetRow("CMD_SPLN_REQ_ID") 
			
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
			
			
			
			
			
	
			Dim targetRowFunding As DataRow
			Dim targetRowFundingCarryover As DataRow
			'targetRowFunding = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()

		Dim accountList As New List(Of String) From {"Obligations", "Commitments","Carryover_Commitments","Carryover_Obligations"}

	For Each accountName As String In accountList
		If accountName = "Carryover_Commitments" Then 
			accountname = "Commitments"
			wfyear = nextyear
		Else If accountName = "Carryover_Obligations" Then 
			accountname = "Obligations"
			wfyear = nextyear
		End If
'brapi.ErrorLog.LogMessage(si,"beforeaudit = " & wfyear)
		targetRowFunding = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account  = '{accountName}' and Fiscal_year = '{wfyear}'").FirstOrDefault()
'If SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT.Rows.Count > 0 Then
		If SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account  = '{accountName}' and Fiscal_year = '{wfyear}'").Count > 0 Then

			Dim drow As DataRow

			drow = SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account  = '{accountName}' and Fiscal_year = '{wfyear}'").FirstOrDefault()
			drow("Orig_UD1") = targetRow("APPN")
			drow("Updated_UD1") = APPN
			drow("Orig_UD2") = targetRow("MDEP")
			drow("Updated_UD2") = MDEP
			drow("Orig_UD3") = targetRow("APE9")
			drow("Updated_UD3") = APE
			drow("Orig_UD4") = targetRow("Dollar_Type")
			drow("Updated_UD4") = DollarType
			drow("Orig_UD5") = targetRow("CType")
			drow("Updated_UD5") = cTypecol
			drow("Orig_UD6") = targetRow("Obj_Class")
			drow("Updated_UD6") = obj_class
			drow("Orig_UD7") = "None"
			drow("Updated_UD7") = "None"
			drow("Orig_UD8") = "None"
			drow("Updated_UD8") = "None"
				
		Else
			
				Dim newrow As datarow = SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT.NewRow()
				newrow("CMD_SPLN_REQ_ID") = targetRow("CMD_SPLN_REQ_ID")
				newrow("WFScenario_Name") = targetRow("WFScenario_Name")
				newrow("WFCMD_Name") = targetRow("WFCMD_Name")
				newrow("WFTime_Name") = targetRow("WFTime_Name")
				newrow("Entity") = targetRow("Entity")
				newrow("Account") = accountName
				newrow("Fiscal_Year") = wfyear
				newrow("Orig_IC") = "None"
				newrow("Updated_IC") = "None"
				newrow("Orig_Flow") =  targetRow("Status")
				newrow("Updated_Flow") = targetRow("Status")
				newrow("Orig_UD1") = targetRow("APPN")
				newrow("Updated_UD1") = APPN
				newrow("Orig_UD2") = targetRow("MDEP")
				newrow("Updated_UD2") = MDEP
				newrow("Orig_UD3") = targetRow("APE9")
				newrow("Updated_UD3") = APE
				newrow("Orig_UD4") = targetRow("Dollar_Type")
				newrow("Updated_UD4") = DollarType
				newrow("Orig_UD5") = targetRow("CType")
				newrow("Updated_UD5") = cTypecol
				newrow("Orig_UD6") = targetRow("Obj_Class")
				newrow("Updated_UD6") = obj_class
				newrow("Orig_UD7") = "None"
				newrow("Updated_UD7") = "None"
				newrow("Orig_UD8") = "None"
				newrow("Updated_UD8") = "None"
				newrow("Create_Date") = DateTime.Now
				newrow("Create_User") = si.UserName
											
				SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT.rows.add(newrow)
			
			
		End If
			
			Dim targetRowFYValues As DataRow
			targetRowFYValues = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = '{accountName}' and Fiscal_year = '{wfyear}'").FirstOrDefault()
				 Dim Yearly As Decimal
				
				 Dim Month1 As Decimal
				 Dim Month2 As Decimal
				 Dim Month3 As Decimal
				 Dim Month4 As Decimal
				 Dim Month5 As Decimal
				 Dim Month6 As Decimal
				 Dim Month7 As Decimal
				 Dim Month8 As Decimal
				 Dim Month9 As Decimal
				  Dim Month10 As Decimal 
				  Dim Month11 As Decimal 
				  Dim Month12 As Decimal 
				 Dim Quarter1 As Decimal
				 Dim Quarter2 As Decimal
				 Dim Quarter3 As Decimal
				 Dim Quarter4 As Decimal
				If Not targetRowFYValues("Yearly").ToString = "" Then Yearly =  targetRowFYValues("Yearly")	

				If Not targetRowFYValues("Month1").ToString = "" Then Month1  =  targetRowFYValues("Month1")
				If Not targetRowFYValues("Month2").ToString = "" Then Month2  = targetRowFYValues("Month2")
			    If Not targetRowFYValues("Month3").ToString = "" Then Month3  = targetRowFYValues("Month3")	
			 	If Not targetRowFYValues("Month4").ToString = "" Then Month4  = targetRowFYValues("Month4")
				If Not targetRowFYValues("Month5").ToString = "" Then Month5  =  targetRowFYValues("Month5")
				If Not targetRowFYValues("Month6").ToString = "" Then Month6  =  targetRowFYValues("Month6")	
				If Not targetRowFYValues("Month7").ToString = "" Then Month7  =  targetRowFYValues("Month7")	
				If Not targetRowFYValues("Month8").ToString = "" Then Month8  =  targetRowFYValues("Month8")	
				If Not targetRowFYValues("Month9").ToString = "" Then Month9  =  targetRowFYValues("Month9")	
				If Not targetRowFYValues("Month10").ToString = "" Then Month10  =  targetRowFYValues("Month10")	
				If Not targetRowFYValues("Month11").ToString = "" Then Month11  =  targetRowFYValues("Month11")	
				If Not targetRowFYValues("Month12").ToString = "" Then Month12  =  targetRowFYValues("Month12")	
				If Not targetRowFYValues("Quarter1").ToString = "" Then Quarter1 =  targetRowFYValues("Quarter1")	
				If Not targetRowFYValues("Quarter2").ToString = "" Then Quarter2 =  targetRowFYValues("Quarter2")	
				If Not targetRowFYValues("Quarter3").ToString = "" Then Quarter3 =  targetRowFYValues("Quarter3")	
				If Not targetRowFYValues("Quarter4").ToString = "" Then Quarter4 =  targetRowFYValues("Quarter4")

	
							
						
	
		
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
							targetRowFundingNew("Account") = accountName
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
			targetRowFundingNew("Fiscal_Year") = wfyear		
							targetRowFundingNew("Month1") = Month1
							targetRowFundingNew("Month2") = Month2
							targetRowFundingNew("Month3") = Month3
							targetRowFundingNew("Month4") = Month4
							targetRowFundingNew("Month5") = Month5
							targetRowFundingNew("Month6") = Month6
							targetRowFundingNew("Month7") = Month7
							targetRowFundingNew("Month8") = Month8
							targetRowFundingNew("Month9") = Month9
							targetRowFundingNew("Month10") = Month10
							targetRowFundingNew("Month11") = Month11
							targetRowFundingNew("Month12") = Month12
							targetRowFundingNew("Quarter1") = Quarter1
							targetRowFundingNew("Quarter2") = Quarter2
							targetRowFundingNew("Quarter3") = Quarter3
							targetRowFundingNew("Quarter4") = Quarter4
							targetRowFundingNew("Yearly") = Yearly
							
						
							targetRowFundingNew("AllowUpdate") = "True"
							
							targetRowFundingNew("Create_Date") = targetRow("Create_Date")
	                        targetRowFundingNew("Create_User") = targetRow("Create_User") 
							targetRowFundingNew("Update_Date") = DateTime.Now
	                        targetRowFundingNew("Update_User") = si.UserName  
		                   dt_Details.Rows.Add(targetRowFundingNew)
						   
		          Next    
		                ' Persist changes back to the DB using the configured adapter
		               
		               	sqaReaderdetail.Update_XFC_CMD_SPLN_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
						sqa_xfc_cmd_spln_req_details_audit.Update_XFC_CMD_SPLN_REQ_DETAILS_AUDIT(SQA_XFC_CMD_SPLN_REQ_DETAILS_AUDIT_DT, sqa)
						
						Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_SPLN")
						Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
						For Each JoinedEntity As String In joinedentitylist
							Dim GlobalAPPNs As String = String.Join("|",APPNList)
							Dim GlobalFlows As String = String.Join("|",StatusList)
							globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", GlobalFlows)	
							Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",GlobalAPPNs)
						Next
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))			
						Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
						Dim customSubstVars As New Dictionary(Of String, String) 
						customSubstVars.Add("EntList",String.Join(",",BaseEntityList))
						customSubstVars.Add("ParentEntList",String.Join(",",ParentEntityList))
						customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
						Dim currentYear As String = wfInfoDetails("TimeName")
						customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
		
						BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
						
						
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
'brapi.ErrorLog.LogMessage(si,"cm count = " & REQDT.Rows.count)
					' Mark rows for deletion in both tables

					Dim APPNList As New List (Of String)
					Dim StatusList As New List (Of String)
					Dim FCList As New List (Of String)
					For Each reqId As String In Req_ID_List
						Dim rows As DataRow() = REQDT.Select($"REQ_ID = '{reqId}'")
'brapi.ErrorLog.LogMessage(si,"reqdt status = " & rows.FirstOrDefault().Item("Status").tostring)					
						Dim GUIDROW As String = rows.FirstOrDefault().Item("CMD_SPLN_REQ_ID").tostring
						Dim ent As String = rows.FirstOrDefault().Item("Entity").ToString
						Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
						Dim lsAncestorListU1 As List(Of MemberInfo)
						lsAncestorListU1 = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & rows.FirstOrDefault().Item("APPN").tostring & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
						Dim APPN As String = lsAncestorListU1(0).Member.Name
						
						
						Status  = rows.FirstOrDefault().Item("Status").tostring
						
						If Not FCList.Contains(ent) Then
							FCList.Add(ent)
						End If
						If Not APPNList.Contains(APPN) Then
							APPNList.Add(APPN)
						End If
						If Not StatusList.Contains(status) Then
							StatusList.Add(status)
						End If

						For Each row As DataRow In rows
							row.Delete()
						Next
'brapi.ErrorLog.LogMessage(si,"here5")
						Dim detailRows As DataRow() = REQDetailDT.Select($"CMD_SPLN_REQ_ID = '{GUIDROW}'")
						For Each drow As DataRow In detailRows
							drow.Delete()
						Next
						globals.SetStringValue($"FundsCenterStatusUpdates - {ent}", $"{statuslist}")
					Next
					
					
					' Persist changes back to DB
					sqa_xfc_cmd_SPLN_req_details.Update_XFC_CMD_SPLN_REQ_Details(REQDetailDT, sqa)
					sqa_xfc_cmd_SPLN_req.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
					
					
				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_SPLN")
				Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
				For Each JoinedEntity As String In joinedentitylist

					globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", String.Join("|",StatusList))	
					Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",String.Join("|",APPNList))
				Next
				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))			
				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("EntList",String.Join(",",BaseEntityList))
				customSubstVars.Add("ParentEntList",String.Join(",",ParentEntityList))
				customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
				Dim currentYear As String = wfInfoDetails("TimeName")
				Dim nextyear As String = currentYear + 1
				customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")

				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
					

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
					
'Brapi.ErrorLog.LogMessage(si,"REQ" & RequirementID)

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

#Region "Copy CMD SPLN to HQ SPLN"
Public Function CopyCMDSPLNToHQSPLN() As xfselectionchangedTaskResult
	Try
		Dim sAPPN As String  = args.NameValuePairs.XFGetValue("APPN")		
'brapi.ErrorLog.LogMessage(si, $"APPN: {sAPPN}")
		Dim customSubstVars As New Dictionary(Of String, String)
		customSubstVars.Add("APPN", sAPPN)
		Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"60 HQ SPLN")
'brapi.ErrorLog.LogMessage(si, $"wsid: {wsID}")
		BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "HQ_SPLN_Copy_CMD_SPLN", customSubstVars)
	Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try	
End Function
#End Region




	End Class
End Namespace