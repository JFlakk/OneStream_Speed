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
Imports OneStream.Shared.Engine.BusinessRules
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
                        Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
                        Dim dargs As New DashboardExtenderArgs
                        dargs.FunctionName = "Check_WF_Complete_Lock"
                        Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)

                        If Not sWFStatus.XFContainsIgnoreCase("unlock") Then
                            dbExt_ChangedResult.IsOK = False
                            dbExt_ChangedResult.ShowMessageBox = True
                            dbExt_ChangedResult.Message = vbCRLF & "Current workflow step is locked. Please contact your requriements manager to open access." & vbCRLF
                            Return dbExt_ChangedResult
                        End If

                        Select Case args.FunctionName.ToLower()
                            Case "check_wf_complete_lock"
                                dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
                                Return dbExt_ChangedResult
                            Case args.FunctionName.XFEqualsIgnoreCase("CreateREQMain")
                                dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
                                Me.DbCache(si, args)
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
                            Case "importreq", "rollfwdreq"
                                dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
                                If dbExt_ChangedResult.ShowMessageBox = True Then
                                    Return dbExt_ChangedResult
                                End If
                                If args.NameValuePairs.XFGetValue("Action").XFEqualsIgnoreCase("Insert") Then
                                    dbExt_ChangedResult = Me.Update_Status()
                                End If
                                If args.NameValuePairs.XFGetValue("Action").XFEqualsIgnoreCase("Delete") Then
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
                        End Select

                        #Region "Cache Prompts"
                        If args.FunctionName.XFEqualsIgnoreCase("CachePrompts") Then
                            dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
                            If dbExt_ChangedResult.ShowMessageBox = True Then
                                Return dbExt_ChangedResult
                            End If
                            Me.CachePrompts(si, globals, api, args)
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

                                ModifiedCustomSubstVars = Me.ShowAndHideDashboards(si, globals, api, args)
                                Return Me.ReplaceSelections(si, globals, api, args, paramsToClear, ModifiedCustomSubstVars)
                            Else
                                Dim paramsToClear As String = "ML_REQPRO_FundCenter_Status," & _
                                    "ML_REQPRO_APPN," & _
                                    "ML_REQPRO_MDEP," & _
                                    "ML_REQPRO_APE9," & _
                                    "ML_REQPRO_SAG," & _
                                    "ML_REQPRO_DollarType," & _
                                    "prompt_tbx_REQPRO_AAAAAA_0CaAa_SearchReq__Shared"

                                ModifiedCustomSubstVars = Me.ShowAndHideDashboards(si, globals, api, args)
                                Return Me.ReplaceSelections(si, globals, api, args, paramsToClear, ModifiedCustomSubstVars)
                            End If
                        End If
                        #End Region
                End Select
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        #Region "Constants"
        Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
        #End Region

        #Region "Set Notification List"
        Public Function setnotificationlist()
            Try
                Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
                Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
                Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
                Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
                Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
                Dim notificationEmails As String = args.NameValuePairs.XFGetValue("Emails")
                Dim vNotificationEmails As String = args.NameValuePairs.XFGetValue("vEmails")

                Dim stakeholderEmailList As String() = notificationEmails.Split(",")
                Dim validatorEmailList As String() = vNotificationEmails.Split(",")

                Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
                Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)

                Using connection As New SqlConnection(dbConnApp.ConnectionString)
                    connection.Open()
                    Dim sqa_xfc_cmd_pgm_req = New SQA_XFC_CMD_PGM_REQ(connection)
                    Dim SQA_XFC_CMD_PGM_REQ_DT = New DataTable()
                    Dim sqa = New SqlDataAdapter()

                    Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"

                    Dim sqlParams As SqlParameter() = New SqlParameter() {
                        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                    }
                    sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlParams)

                    Dim REQ_ID As String = sREQ.Split(" "c)(1)
                    Dim Mainreqrow As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ_ID}'").FirstOrDefault()
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
            Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs", "NA")
            Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
            Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
            Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name

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
                ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Manage") Then
                    Me.ManageREQStatusUpdated(si, globals, api, args, "")
                End If
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
            Return dbExt_ChangedResult
        End Function

        Private Function Update_REQ_Status(ByVal curr_Status As String) As xfselectionchangedTaskResult
            Try
                Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
                Dim demote_comment As String = args.NameValuePairs.XFGetValue("demotecomment")
                Dim FCList As New List(Of String)
                Dim Status_manager As New Dictionary(Of String, String)
                Status_manager.Add("L5_Formulate_PGM|Validate", "L4_Validate_PGM")
                Status_manager.Add("L4_Formulate_PGM|Validate", "L3_Validate_PGM")
                Status_manager.Add("L3_Formulate_PGM|Validate", "L3_Validate_PGM")
                Status_manager.Add("L2_Formulate_PGM|Validate", "L2_Validate_PGM")
                Status_manager.Add("L4_Validate_PGM|Prioritize", "L4_Prioritize_PGM")
                Status_manager.Add("L3_Validate_PGM|Prioritize", "L3_Prioritize_PGM")
                Status_manager.Add("L2_Validate_PGM|Prioritize", "L2_Prioritize_PGM")
                Status_manager.Add("L4_Validate_PGM|Approve", "L4_Approve_PGM")
                Status_manager.Add("L3_Validate_PGM|Approve", "L3_Approve_PGM")
                Status_manager.Add("L2_Validate_PGM|Approve", "L2_Approve_PGM")
                Status_manager.Add("L4_Prioritize_PGM|Approve", "L4_Approve_PGM")
                Status_manager.Add("L3_Prioritize_PGM|Approve", "L3_Approve_PGM")
                Status_manager.Add("L2_Prioritize_PGM|Approve", "L2_Approve_PGM")
                Status_manager.Add("L2_Approve_PGM|Final", "L2_Final_PGM")
                Status_manager.Add("L3_Approve_PGM|Validate", "L2_Validate_PGM")
                Status_manager.Add("L4_Approve_PGM|Validate", "L3_Validate_PGM")
                Status_manager.Add("L2_Formulate_PGM|Final", "L2_Final_PGM")
                Status_manager.Add("L2_Final_PGM|Formulate", "L2_Formulate_PGM")

                If args.NameValuePairs.XFGetValue("Demote") = "True" Then
                    #Region "Demote Statuses"
                    Status_manager.Add("L2_Approve_PGM|L2_Prioritize_PGM", "L2_Prioritize_PGM")
                    Status_manager.Add("L2_Approve_PGM|L2_Validate_PGM", "L2_Validate_PGM")
                    Status_manager.Add("L2_Approve_PGM|L3_Approve_PGM", "L3_Approve_PGM")
                    Status_manager.Add("L2_Approve_PGM|L3_Prioritize_PGM", "L3_Prioritize_PGM")
                    Status_manager.Add("L2_Approve_PGM|L3_Validate_PGM", "L3_Validate_PGM")
                    Status_manager.Add("L2_Approve_PGM|L3_Formulate_PGM", "L3_Formulate_PGM")
                    Status_manager.Add("L2_Approve_PGM|L4_Approve_PGM", "L4_Approve_PGM")
                    Status_manager.Add("L2_Approve_PGM|L4_Prioritize_PGM", "L4_Prioritize_PGM")
                    Status_manager.Add("L2_Approve_PGM|L4_Validate_PGM", "L4_Validate_PGM")
                    Status_manager.Add("L2_Approve_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L2_Approve_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")

                    Status_manager.Add("L3_Approve_PGM|L3_Prioritize_PGM", "L3_Prioritize_PGM")
                    Status_manager.Add("L3_Approve_PGM|L3_Validate_PGM", "L3_Validate_PGM")
                    Status_manager.Add("L3_Approve_PGM|L3_Formulate_PGM", "L3_Formulate_PGM")
                    Status_manager.Add("L3_Approve_PGM|L4_Approve_PGM", "L4_Approve_PGM")
                    Status_manager.Add("L3_Approve_PGM|L4_Prioritize_PGM", "L4_Prioritize_PGM")
                    Status_manager.Add("L3_Approve_PGM|L4_Validate_PGM", "L4_Validate_PGM")
                    Status_manager.Add("L3_Approve_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L3_Approve_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")

                    Status_manager.Add("L4_Approve_PGM|L4_Prioritize_PGM", "L4_Prioritize_PGM")
                    Status_manager.Add("L4_Approve_PGM|L4_Validate_PGM", "L4_Validate_PGM")
                    Status_manager.Add("L4_Approve_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L4_Approve_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")

                    Status_manager.Add("L2_Validate_PGM|L2_Formulate_PGM", "L2_Formulate_PGM")
                    Status_manager.Add("L2_Validate_PGM|L3_Approve_PGM", "L3_Approve_PGM")
                    Status_manager.Add("L2_Validate_PGM|L3_Prioritize_PGM", "L3_Prioritize_PGM")
                    Status_manager.Add("L2_Validate_PGM|L3_Validate_PGM", "L3_Validate_PGM")
                    Status_manager.Add("L2_Validate_PGM|L3_Formulate_PGM", "L3_Formulate_PGM")
                    Status_manager.Add("L2_Validate_PGM|L4_Approve_PGM", "L4_Approve_PGM")
                    Status_manager.Add("L2_Validate_PGM|L4_Prioritize_PGM", "L4_Prioritize_PGM")
                    Status_manager.Add("L2_Validate_PGM|L4_Validate_PGM", "L4_Validate_PGM")
                    Status_manager.Add("L2_Validate_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L2_Validate_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")

                    Status_manager.Add("L3_Validate_PGM|L3_Formulate_PGM", "L3_Formulate_PGM")
                    Status_manager.Add("L3_Validate_PGM|L4_Approve_PGM", "L4_Approve_PGM")
                    Status_manager.Add("L3_Validate_PGM|L4_Prioritize_PGM", "L4_Prioritize_PGM")
                    Status_manager.Add("L3_Validate_PGM|L4_Validate_PGM", "L4_Validate_PGM")
                    Status_manager.Add("L3_Validate_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L3_Validate_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")

                    Status_manager.Add("L4_Validate_PGM|L4_Formulate_PGM", "L4_Formulate_PGM")
                    Status_manager.Add("L4_Validate_PGM|L5_Formulate_PGM", "L5_Formulate_PGM")
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
                    Dim full_Req_ID_List As List(Of String) = StringHelper.SplitString(req_IDs, ",")
                    Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")

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
                                paramNames.Add(paramName)
                                dtfilter = String.Join(",", paramNames)
                            End If

                            Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND REQ_ID in ({DTFilter})"

                            Dim paramList As New List(Of SqlParameter) From {
                                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                            }
                            Dim sqlparams As SqlParameter() = paramList.ToArray()
                            sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlparams)

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
                                Dim DetailparamNames As New List(Of String)
                                For i As Integer = 0 To parentIds.Count - 1
                                    Dim DetailparamName As String = "'" & parentIds(i) & "'"
                                    DetailparamNames.Add(DetailparamName)
                                Next
                                DetaildtFilter = String.Join(",", DetailparamNames)
                                Dim SQL_Audit As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details_Audit WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID in ({DetaildtFilter})"
                                sql = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID in ({DetaildtFilter})"

                                Dim detailsParams As New List(Of SqlParameter) From {
                                    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                                    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                                    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                                }
                                sqa_xfc_cmd_pgm_req_details.Fill_XFC_CMD_PGM_REQ_DETAILS_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_DT, sql, detailsParams.ToArray())
                                sqa_xfc_cmd_pgm_req_details_audit.Fill_XFC_CMD_PGM_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, SQL_Audit, detailsParams.ToArray())
                            End If

                            For Each row As DataRow In SQA_XFC_CMD_PGM_REQ_DT.Rows
                                Dim wfStepAllowed = Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Step_Allowed(si, args, curr_Status, row("Entity"))
                                If wfStepAllowed = False Then
                                    dbExt_ChangedResult.ShowMessageBox = True
                                    If Not String.IsNullOrWhiteSpace(dbExt_ChangedResult.Message) Then
                                        dbExt_ChangedResult.Message &= Environment.NewLine
                                    End If
                                    Throw New Exception($"Cannot change status of Requirement '{row("REQ_ID")}' at this time. Contact requirements manager.")
                                Else
                                    Dim existingStatus As String = ""
                                    If Not IsDBNull(row("Status")) Then existingStatus = row("Status").ToString().Trim()
                                    Dim lookupKey As String = existingStatus & "|" & new_Status
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

                                    Dim pid As String = row("CMD_PGM_REQ_ID").ToString()
                                    Dim filterExpr As String = String.Format("CMD_PGM_REQ_ID = '{0}'", pid)

                                    If SQA_XFC_CMD_PGM_REQ_DETAILS_DT IsNot Nothing AndAlso SQA_XFC_CMD_PGM_REQ_DETAILS_DT.Rows.Count > 0 AndAlso Not IsDBNull(row("CMD_PGM_REQ_ID")) Then
                                        Dim matchingDetails() As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_DT.Select(filterExpr)
                                        For Each drow As DataRow In matchingDetails
                                            If Not FCList.Contains($"E#{drow("Entity")}") Then
                                                FCList.Add($"E#{drow("Entity")}")
                                            End If
                                            globals.SetStringValue($"FundsCenterStatusUpdates - {drow("Entity")}", $"{existingStatus}|{resolvedStatus}")
                                            drow("Flow") = resolvedStatus
                                            drow("Update_User") = si.UserName
                                            drow("Update_Date") = DateTime.Now
                                        Next
                                    End If

                                    Dim matchingAuditDetails() As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Select(filterExpr)
                                    If matchingAuditDetails.Length > 0 Then
                                        For Each drow As DataRow In matchingAuditDetails
                                            Dim currentHistory As String = If(drow("Orig_Flow") Is DBNull.Value, String.Empty, drow("Orig_Flow").ToString())
                                            If String.IsNullOrEmpty(currentHistory) Then
                                                drow("Orig_Flow") = existingStatus
                                            Else
                                                drow("Orig_Flow") = currentHistory + ", " + existingStatus
                                            End If
                                            drow("Updated_Flow") = resolvedStatus
                                        Next
                                    Else
                                        Dim newrow As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.NewRow()
                                        newrow("CMD_PGM_REQ_ID") = row("CMD_PGM_REQ_ID")
                                        newrow("WFScenario_Name") = row("WFScenario_Name")
                                        newrow("WFCMD_Name") = row("WFCMD_Name")
                                        newrow("WFTime_Name") = row("WFTime_Name")
                                        newrow("Entity") = row("Entity")
                                        newrow("Account") = "Req_Funding"
                                        newrow("Start_Year") = row("WFTime_Name")
                                        newrow("Orig_IC") = "None"
                                        newrow("Updated_IC") = "None"
                                        newrow("Orig_Flow") = existingStatus
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
                                        SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Rows.Add(newrow)
                                    End If

                                    If Not wfProfileName.XFContainsIgnoreCase("Import") Then
                                        Workspace.GBL.GBL_Assembly.GBL_Helpers.SendStatusChangeEmail(si, globals, api, args, resolvedStatus, row("Req_ID"))
                                    End If
                                End If
                            Next

                            sqa_xfc_cmd_pgm_req.Update_XFC_CMD_PGM_REQ(SQA_XFC_CMD_PGM_REQ_DT, sqa)
                            sqa_xfc_cmd_pgm_req_details.Update_XFC_CMD_PGM_REQ_DETAILS(SQA_XFC_CMD_PGM_REQ_DETAILS_DT, sqa)
                            sqa_xfc_cmd_pgm_req_details_audit.Update_XFC_CMD_PGM_REQ_DETAILS_AUDIT(SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, sqa)
                        End Using
                    End If

                    Dim wsID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")
                    Dim customSubstVars As New Dictionary(Of String, String)
                    customSubstVars.Add("EntList", String.Join(",", FCList))
                    customSubstVars.Add("WFScen", wfInfoDetails("ScenarioName"))
                    Dim currentYear As Integer = Convert.ToInt32(wfInfoDetails("TimeName"))
                    customSubstVars.Add("WFTime", $"T#{currentYear.ToString()},T#{(currentYear + 1).ToString()},T#{(currentYear + 2).ToString()},T#{(currentYear + 3).ToString()},T#{(currentYear + 4).ToString()}")
                    BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
                    Return dbExt_ChangedResult
                End If
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Public Function ManageREQStatusUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String = "") As Boolean
            Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
            Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
            Dim sUFR As String = args.NameValuePairs.XFGetValue("UFR", UFR)
            Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)

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
                Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
                Dim srcSenario As String = wfInfoDetails("ScenarioName")
                Dim cmd As String = wfInfoDetails("CMDName")
                Dim tm As String = wfInfoDetails("TimeName")
                Dim srcfundCenter = args.NameValuePairs.XFGetValue("sourceEntity", "")
                Dim SrcREQNameTilte = args.NameValuePairs.XFGetValue("SourceREQ", "")
                Dim REQ_IDs As String = args.NameValuePairs("req_IDs")

                If String.IsNullOrWhiteSpace(REQ_IDs) Then
                    Return Nothing
                End If

                REQ_IDs = REQ_IDs.Replace(" ", "")
                Dim REQIDs_list As New List(Of String)
                REQIDs_list = REQ_IDs.Split(",").ToList
                REQ_IDs = "'" & REQ_IDs.Replace(",", "','") & "'"

                Dim REQDT As DataTable = New DataTable()
                Dim REQDetailDT As DataTable = New DataTable()
                Dim trgtfundCenter = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
                Dim trgtScenario As String = wfInfoDetails("ScenarioName")
                Dim trgtREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
                Dim trgtFlow = "Temp"
                Dim sqa As New SqlDataAdapter()

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
                        Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
                        Dim SqlREQ As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = '{srcSenario}' And WFCMD_Name = '{cmd}' AND WFTime_Name = '{tm}' AND REQ_ID in ({REQ_IDs})"

                        Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                        sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)

                        Dim GUIDs As String = "'"
                        For Each r As DataRow In REQDT.Rows
                            GUIDs = GUIDs & r("CMD_PGM_REQ_ID").ToString & "','"
                        Next
                        GUIDs = GUIDs.Substring(0, GUIDs.Length - 2)

                        Dim SqlREQDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = '{srcSenario}' And WFCMD_Name = '{cmd}' AND WFTime_Name = '{tm}' AND CMD_PGM_REQ_ID in ({GUIDs})"
                        Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
                        sqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)

                        Dim copiedRows As New List(Of DataRow)
                        Dim copiedDetailRows As New List(Of DataRow)
                        For Each row As DataRow In REQDT.Rows
                            Dim newREQRow As DataRow = CMD_PGM_Utilities.GetCopiedRow(si, row)
                            Me.UpdateCopyREQColumns(newREQRow)
                            CMD_PGM_Utilities.UpdateAuditColumns(si, newREQRow)
                            Dim newCMD_PGM_REQ_ID As String = newREQRow("CMD_PGM_REQ_ID").ToString
                            copiedRows.Add(newREQRow)

                            Dim foundDetailRows As DataRow() = REQDetailDT.Select(String.Format("CMD_PGM_REQ_ID = '{0}'", row("CMD_PGM_REQ_ID").ToString()))
                            For Each dtRow As DataRow In foundDetailRows
                                Dim newREQDetailRow As DataRow = CMD_PGM_Utilities.GetCopiedRow(si, dtRow)
                                Me.UpdateCopyREQDetailColumns(newREQDetailRow, newCMD_PGM_REQ_ID)
                                CMD_PGM_Utilities.UpdateAuditColumns(si, newREQDetailRow)
                                copiedDetailRows.Add(newREQDetailRow)
                            Next
                        Next

                        For Each r As DataRow In copiedRows
                            REQDT.Rows.Add(r)
                        Next
                        For Each r As DataRow In copiedDetailRows
                            REQDetailDT.Rows.Add(r)
                        Next

                        sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
                        sqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)
                        Args.NameValuePairs.Add("new_Status", "Formulate")
                    End Using
                End Using
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        #Region "GetNextREQID"
        Dim startingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
        Function GetNextREQID(fundCenter As String) As String
            Dim currentREQID As Integer
            Dim newREQ_ID As String
            If startingREQ_IDByFC.TryGetValue(fundCenter, currentREQID) Then
                currentREQID = currentREQID + 1
                Dim modifiedFC As String = fundCenter
                modifiedFC = modifiedFC.Replace("_General", "")
                If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
                newREQ_ID = modifiedFC & "_" & currentREQID.ToString("D5")
                startingREQ_IDByFC(fundCenter) = currentREQID
            Else
                newREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si, fundCenter)
                startingREQ_IDByFC.Add(fundCenter.Trim, newREQ_ID.Split("_")(1))
            End If
            Return newREQ_ID
        End Function
        #End Region

        Public Sub UpdateCopyREQColumns(ByRef newRow As DataRow)
            Dim trgtfundCenter = newRow("Entity")
            Dim newREQ_ID As String = GetNextREQID(trgtfundCenter)
            newRow("REQ_ID") = newREQ_ID
            newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
            newRow("Title") = newRow("Title") & " - Copy"
            newRow("Status") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
        End Sub

        Public Sub UpdateCopyREQDetailColumns(ByRef newRow As DataRow, ByRef newCMD_PGM_REQ_ID As String)
            Dim trgtfundCenter = newRow("Entity")
            newRow("CMD_PGM_REQ_ID") = newCMD_PGM_REQ_ID
            newRow("Flow") = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, trgtfundCenter) & "_Formulate_PGM"
        End Sub

        Public Function Delete_REQ() As xfselectionchangedTaskResult
            Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
            Dim sREQToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
            Dim iREQAmount As Integer = Nothing
            Dim sREQFlow As String = args.NameValuePairs.XFGetValue("reqFlow", sREQToDelete)

            args.NameValuePairs.Add("REQEntity", sEntity)
            args.NameValuePairs.Add("REQ", sREQToDelete)

            Dim deleteDataAttchSQL As String = "Delete from DataAttachment WHERE Cube = '" & sCube & "' And Entity = '" & sEntity & "' And Scenario = '" & sScenario & "' And Flow = '" & sREQToDelete & "'"
            Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                BRApi.Database.ExecuteSql(dbConn, deleteDataAttchSQL, True)
            End Using

            Dim DeleteAnnotationAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Requirement_Info.Base", True)
            Dim DeletePeriodicAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Funding_Required_Date", True)
            Dim DeletePOCMbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U5_Main"), "U5#REQ_POCs.Base", True)
            Dim ResetValue As String = ""

            For Each REQAccount As MemberInfo In DeleteAnnotationAccounts
                Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#" & REQAccount.Member.Name & ":F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

                Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
                Dim objScriptValAccount As New MemberScriptAndValue
                objScriptValAccount.CubeName = sCube
                objScriptValAccount.Script = DeleteMbrScript
                objScriptValAccount.TextValue = ResetValue
                objScriptValAccount.IsNoData = False
                objListofScriptsAccount.Add(objScriptValAccount)
                BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)
            Next

            For Each POCMbr As MemberInfo In DeletePOCMbrs
                Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_POC_Name:F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & POCMbr.Member.Name & ":U6#None:U7#None:U8#None"

                Dim objListofScriptsAccount As New List(Of MemberScriptandValue)
                Dim objScriptValAccount As New MemberScriptAndValue
                objScriptValAccount.CubeName = sCube
                objScriptValAccount.Script = DeleteMbrScript
                objScriptValAccount.TextValue = ResetValue
                objScriptValAccount.IsNoData = False
                objListofScriptsAccount.Add(objScriptValAccount)
                BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAccount)
            Next

            For Each REQAccount As MemberInfo In DeletePeriodicAccounts
                Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#" & REQAccount.Member.Name & ":F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

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
            Return Nothing
        End Function

        Public Function Set_Related_REQs() As xfselectionchangedTaskResult
            Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
            Dim sUFR As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
            Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")
            Dim sScenario As String = args.NameValuePairs.XFGetValue("REQScenario")
            Dim sUFRTime As String = args.NameValuePairs.XFGetValue("REQTime")
            Dim sRelatedUFRs As String = args.NameValuePairs.XFGetValue("RelatedREQs")
            Dim RelatedUFRLength As Integer = sRelatedUFRs.Length
            Dim UFRList As String() = sRelatedUFRs.Split(","c).Select(Function(UFR) UFR.Trim()).ToArray()

            If RelatedUFRLength = 0 Then
                Return Nothing
            End If

            Dim UFRtitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Title:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRtitleMemberScript).DataCellEx.DataCellAnnotation
            Dim mainUFR As String = ""

            If sEntity.XFContainsIgnoreCase("_General") Then
                mainUFR = $"{sEntity.Replace("_General", "")} - {sUFR} - {TitleValue}"
            Else
                mainUFR = $"{sEntity} - {sUFR} - {TitleValue}"
            End If

            Dim RelatedUFRMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Related_Request:F#" & sUFR & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim currentUFRs As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, RelatedUFRMemberScript).DataCellEx.DataCellAnnotation

            For Each UFR As String In UFRList
                If UFR.XFContainsIgnoreCase(mainUFR) Then Continue For
                Dim isGeneral As Boolean = UFR.Split("-")(0).Trim().XFContainsIgnoreCase("_General")
                If isGeneral Then UFR = UFR.Replace(UFR.Split("-")(0).Trim(), UFR.Split("-")(0).Trim().Replace("_General", ""))
                If (Not currentUFRs.XFContainsIgnoreCase(UFR)) Then
                    If (String.IsNullOrWhiteSpace(currentUFRs)) Then
                        currentUFRs = UFR
                    Else
                        currentUFRs = currentUFRs & ", " & UFR
                    End If
                End If
            Next

            Dim objListofScripts As New List(Of MemberScriptandValue)
            Dim objScriptVal As New MemberScriptAndValue
            objScriptVal.CubeName = sCube
            objScriptVal.Script = RelatedUFRMemberScript
            objScriptVal.TextValue = currentUFRs
            objScriptVal.IsNoData = False
            objListofScripts.Add(objScriptVal)
            BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)

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
                Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
                Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
                Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
                Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
                Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
                Dim reqTime As String = WFYear & "M12"
                Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
                Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow")

                Dim REQMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & reqTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & REQFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, REQMemberScript).DataCellEx.DataCellAnnotation
                If (String.IsNullOrWhiteSpace(currentStatus)) Then
                    Return Nothing
                End If

                Dim newStatus As String = "Ready for Validation"
                Dim objListofScriptStatus As New List(Of MemberScriptandValue)
                Dim objScriptValStatus As New MemberScriptAndValue
                objScriptValStatus.CubeName = wfCube
                objScriptValStatus.Script = REQMemberScript
                objScriptValStatus.TextValue = newStatus
                objScriptValStatus.IsNoData = False
                objListofScriptStatus.Add(objScriptValStatus)
                BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
            Return Nothing
        End Function
        #End Region

        #Region "Cache Prompts"
        Public Function CachePrompts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
            Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
            Dim sREQ As String = args.NameValuePairs.XFGetValue("REQ")
            Dim sMode As String = args.NameValuePairs.XFGetValue("mode", "")
            Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")

            If sMode.XFContainsIgnoreCase("copyREQ") And String.IsNullOrWhiteSpace(sEntity) Then
                Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
                Dim currDashboard As Dashboard = args.PrimaryDashboard
                Dim dashAction As String = "Refresh"
                Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType), dashAction)
                Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
                objXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
                objXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = objXFSelectionChangedUIActionType

                selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
                selectionChangedTaskResult.IsOK = True
                selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
                selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
                selectionChangedTaskResult.ShowMessageBox = True
                selectionChangedTaskResult.Message = "Please select a funds center."
                Return selectionChangedTaskResult
            End If

            If Not sMode.XFContainsIgnoreCase("copyREQ") And String.IsNullOrEmpty(sREQ) Then
                Throw New Exception("Please select a requirement.")
                Return Nothing
            End If

            If Not String.IsNullOrWhiteSpace(sEntity) Then
                BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", sEntity)
            End If
            BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", sREQ)

            If Not String.IsNullOrWhiteSpace(sDashboard) Then
                BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Dashboard", sDashboard)
            End If
            Return Nothing
        End Function
        #End Region

        #Region "Show and Hide"
        Public Function ShowAndHideDashboards(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
            Try
                Dim checkForBlank As String = args.NameValuePairs.XFGetValue("checkForBlank")
                Dim trueVale As String = args.NameValuePairs.XFGetValue("trueValue")
                Dim falseValue As String = args.NameValuePairs.XFGetValue("falseValue")
                Dim allTimeValue As String = args.NameValuePairs.XFGetValue("allTimeValue")
                Dim sDb As String = args.NameValuePairs.XFGetValue("Db", "")
                If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "LR_REQ_Prompts", "Db", sDb)
                Dim sFC As String = args.NameValuePairs.XFGetValue("FundCenter", "")
                If Not String.IsNullOrWhiteSpace(sFC) Then BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "LR_REQ_Prompts", "FC", sFC)

                Dim toShow As String = ""
                Dim toHide As String = ""

                If (String.IsNullOrWhiteSpace(allTimeValue)) Then
                    If (String.IsNullOrWhiteSpace(checkForBlank)) Then
                        toShow = trueVale
                        toHide = falseValue
                    Else
                        toShow = falseValue
                        toHide = trueVale
                    End If
                Else
                    toShow = trueVale
                    toHide = falseValue
                End If

                Dim objXFSelectionChangedUIActionInfo As XFSelectionChangedUIActionInfo = args.SelectionChangedTaskInfo.SelectionChangedUIActionInfo
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
            dKeyVal.Add("BL_CMD_PGM_FundsCenter", Entity)
            Return Me.SetParameter(si, globals, api, dKeyVal)
        End Function
        #End Region

        #Region "Set Default APPN Parameter"
        Public Function SetDeafultAPPNParam() As Object
            Dim dKeyVal As New Dictionary(Of String, String)
            dKeyVal.Add("ML_CMD_PGM_FormulateAPPN", "OMA")

            If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_RelatedREQList", "NA") <> "NA" Then
                dKeyVal.Add("BL_CMD_PGM_RelatedREQList", args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_RelatedREQList", "NA"))
            End If
            BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, $"CMD_PGM_CascadingFilterCache", $"CMD_PGM_rebuildparams_APPN", "")
            BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, $"CMD_PGM_CascadingFilterCache", $"CMD_PGM_rebuildparams_Other", "")
            Return Me.SetParameter(si, globals, api, dKeyVal)
        End Function
        #End Region

        #Region "Is REQ Title Blank"
        Public Function BlankTitleCheck(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
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
                Throw New Exception("Requirement title cannot be blank." & environment.NewLine & " Please enter a title and click save button.")
            End If
            Return Nothing
        End Function
        #End Region

        #Region "Get Entity"
        Public Function GetEntity(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
            Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
            If String.IsNullOrWhiteSpace(sEntity) Then
                sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
            End If
            Return sEntity
        End Function
        #End Region

        #Region "Set Parameter"
        Public Function SetParameter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal dKeyVal As Dictionary(Of String, String), Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing) As Object
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
        Public Function ClearSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String) As Object
            Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
            For i As Integer = 0 To objDictionary.Count - 1
                Dim thisKey = objDictionary.ElementAt(i).Key
                If (ParamsToClear.XFContainsIgnoreCase(thisKey)) Then
                    objDictionary.Remove(thisKey)
                    objDictionary.Add(thisKey, "")
                End If
            Next

            Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
            selectionChangedTaskResult.IsOK = True
            selectionChangedTaskResult.ShowMessageBox = False
            selectionChangedTaskResult.Message = ""
            selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
            selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
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
        Public Function ReplaceSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String, Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing) As Object
            Dim ShowHideCheck As Boolean = True
            If selectionChangedTaskResult Is Nothing Then
                ShowHidecheck = False
                selectionChangedTaskResult = New XFSelectionChangedTaskResult()
            End If
            Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
            Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
            Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
            Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
            Dim UFRTime As String = WFYear
            Dim ufrEntity As String = args.NameValuePairs.XFGetValue("ufrEntity")
            Dim ufrFlow As String = args.NameValuePairs.XFGetValue("ufrFlow")

            Dim UFRWFMemberScript As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim UFRFundingMemberScript As String = "Cb#" & wfCube & ":E#" & ufrEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Funding_Status:F#" & ufrFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

            Dim currentWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRWFMemberScript).DataCellEx.DataCellAnnotation
            Dim currentFundingStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRFundingMemberScript).DataCellEx.DataCellAnnotation
            For i As Integer = 0 To objDictionary.Count - 1
                Dim thisKey = objDictionary.ElementAt(i).Key
                If (ParamsToClear.XFContainsIgnoreCase(thisKey)) Then
                    If thisKey.XFContainsIgnoreCase("FundingStatus") And currentWFStatus.XFContainsIgnoreCase("Disposition") Then
                        objDictionary.Remove(thisKey)
                        objDictionary.Add(thisKey, currentFundingStatus)
                    ElseIf thisKey.XFContainsIgnoreCase("WFStatus") And currentWFStatus.XFContainsIgnoreCase("Disposition") Then
                        objDictionary.Remove(thisKey)
                        objDictionary.Add(thisKey, currentWFStatus)
                    Else
                        objDictionary.Remove(thisKey)
                        objDictionary.Add(thisKey, "")
                    End If
                End If
            Next

            selectionChangedTaskResult.IsOK = True
            selectionChangedTaskResult.ShowMessageBox = False
            selectionChangedTaskResult.Message = ""
            If ShowHideCheck = False Then
                selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
            Else
                selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
            End If
            selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
            selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
            selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
            selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary
            Return selectionChangedTaskResult
        End Function
        #End Region
        #End Region

        #Region "Calculate Full FYDP"
        Public Function CalculateFullFYDP(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal REQFlow As String)
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim sEntity As String = GetEntity(si, args)
            Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
            Dim sAPPNFund As String = args.NameValuePairs.XFGetValue("APPNFund")
            Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")
            Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
            Dim sDollarType As String = args.NameValuePairs.XFGetValue("DollarType")
            Dim sCommitmentItem As String = args.NameValuePairs.XFGetValue("CommitmentItem")
            Dim IC As String = sEntity

            Dim REQRequestedAmt As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Periodic:A#REQ_Requested_Amt:F#" & REQFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommitmentItem & ":U7#None:U8#None"
            Dim iCurrScenarioYear As Integer = Strings.Right(sScenario, 4).XFConvertToInt
            Dim iAPPNID As String = BRapi.Finance.Members.GetMemberId(si, dimtype.UD1.Id, sAPPNFund)
            Dim sAPPN As String = ""

            If sAPPNFund.XFEqualsIgnoreCase("none") Then
                sAPPN = "None"
            Else
                sAPPN = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iAPPNID, False)(0).Name & "_General"
            End If

            Dim sInfRateScript As String = "Cb#" & sCube & ":S#" & sScenario & ":T#" & iCurrScenarioYear.ToString & ":C#USD:E#" & sCube & "_General:V#Periodic:A#PGM_Inflation_Rate_Amt:O#BeforeAdj:I#None:F#None"
            sInfRateScript &= ":U1#" & sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim dInflationRate As Decimal = 0
            dInflationRate = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sInfRateScript).DataCellEx.DataCell.CellAmount

            Dim REQAMT As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQRequestedAmt).DataCellEx.DataCell.CellAmount
            Dim dAmt2 As Decimal = Math.Round(REQAMT * (1 + (dInflationRate / 100)))
            Dim dAmt3 As Decimal = Math.Round(dAmt2 * (1 + (dInflationRate / 100)))
            Dim dAmt4 As Decimal = Math.Round(dAmt3 * (1 + (dInflationRate / 100)))
            Dim dAmt5 As Decimal = Math.Round(dAmt4 * (1 + (dInflationRate / 100)))

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
                Dim sDb As String = args.NameValuePairs.XFGetValue("Db", "")
                If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "LR_REQ_Prompts", "Db", sDb)
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Sub
        #End Region

        #Region "Set Related REQs"
        Public Function SetRelatedREQs()
            Try
                Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
                Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
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

                    Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"

                    Dim sqlParams As SqlParameter() = New SqlParameter() {
                        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                    }

                    sqa_xfc_cmd_pgm_req.Fill_XFC_CMD_PGM_REQ_DT(sqa, SQA_XFC_CMD_PGM_REQ_DT, sql, sqlParams)
                    Dim REQ_ID As String = sREQ.Split(" "c)(1)
                    Dim Mainreqrow As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ_ID}'").FirstOrDefault()
                    Dim mainREQ As String = $"{sREQ}"

                    Dim currentREQs As String = String.Empty
                    Dim oldRelatedREQsString As String = Mainreqrow("Related_REQs").ToString()
                    Dim oldList As List(Of String) = oldRelatedREQsString.Split(","c).Select(Function(r) r.Trim()).Where(Function(r) Not String.IsNullOrWhiteSpace(r)).ToList()
                    Dim newList As New List(Of String)
                    For Each REQ As String In REQList
                        If REQ.XFContainsIgnoreCase(mainREQ) Then Continue For
                        Dim isGeneral As Boolean = REQ.Split(" ")(0).Trim().XFContainsIgnoreCase("_General")
                        If isGeneral Then REQ = REQ.Replace(REQ.Split(" ")(0).Trim(), REQ.Split(" ")(0).Trim().Replace("_General", ""))
                        If (Not newList.Any(Function(r) r.Equals(REQ, StringComparison.OrdinalIgnoreCase))) Then
                            newList.Add(REQ)
                        End If
                    Next

                    Mainreqrow("Related_REQs") = String.Join(", ", newList)
                    Mainreqrow("Update_User") = si.UserName
                    Mainreqrow("Update_Date") = DateTime.Now

                    For Each relatedREQ In newList
                        If relatedREQ.XFContainsIgnoreCase(mainREQ) Then Continue For
                        Dim REQ = relatedREQ.Split(" ")(1).Trim()
                        sEntity = relatedREQ.Split(" ")(0).Trim()
                        Dim RelatedREQsList As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ}'").FirstOrDefault()
                        If RelatedREQsList Is Nothing Then Continue For
                        currentREQs = RelatedREQsList("Related_REQs").ToString()

                        If (Not currentREQs.XFContainsIgnoreCase(mainREQ)) Then
                            If (String.IsNullOrWhiteSpace(currentREQs)) Then
                                currentREQs = mainREQ
                            Else
                                currentREQs = currentREQs & ", " & mainREQ
                            End If
                        End If
                        RelatedREQsList("Related_REQs") = currentREQs
                        RelatedREQsList("Update_User") = si.UserName
                        RelatedREQsList("Update_Date") = DateTime.Now
                    Next

                    Dim ToRemove = oldList.Except(newList, StringComparer.OrdinalIgnoreCase).ToList()
                    For Each relatedREQtoRemove In ToRemove
                        If relatedREQtoRemove.XFContainsIgnoreCase(mainREQ) Then Continue For
                        Dim REQ = relatedREQtoRemove.Split(" ")(1).Trim()
                        sEntity = relatedREQtoRemove.Split(" ")(0).Trim()
                        Dim RelatedREQsList As DataRow = SQA_XFC_CMD_PGM_REQ_DT.Select($"REQ_ID ='{REQ}'").FirstOrDefault()
                        If RelatedREQsList Is Nothing Then Continue For
                        currentREQs = RelatedREQsList("Related_REQs").ToString()

                        If (currentREQs.XFContainsIgnoreCase(mainREQ)) Then
                            Dim CurrentList As List(Of String) = currentREQs.Split(","c).Select(Function(r) r.Trim()).Where(Function(r) Not String.IsNullOrWhiteSpace(r)).ToList()
                            CurrentList.RemoveAll(Function(r) r.Equals(mainREQ, StringComparison.OrdinalIgnoreCase))
                            RelatedREQsList("Related_REQs") = String.Join(", ", CurrentList)
                            RelatedREQsList("Update_User") = si.UserName
                            RelatedREQsList("Update_Date") = DateTime.Now
                        End If
                    Next
                    sqa_xfc_cmd_pgm_req.Update_XFC_CMD_PGM_REQ(SQA_XFC_CMD_PGM_REQ_DT, sqa)
                End Using
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Create Manpower REQ"
        Public Function CreateManpowerREQ(ByVal globals As brglobals) As XFSelectionChangedTaskResult
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
                    Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
                    Dim SqlREQ As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFCMD_Name = '{cmd}' And WFTime_Name = '{tm}' AND WFScenario_Name = '{sScenario}' AND ENTITY = '{sEntity}' AND REQ_ID_Type = 'Manpower'"

                    Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                    sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)

                    Dim row As DataRow
                    Dim req_ID_Val As Guid
                    Dim REQ_ID As String = ""
                    Dim isInsert As Boolean = True

                    If Not REQDT.Rows.Count = 0 Then
                        req_ID_Val = reqdt.Rows(0).Item("CMD_PGM_REQ_ID")
                        req_id = reqdt.Rows(0).Item("REQ_ID")
                        isInsert = False
                        row = reqdt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
                    Else
                        row = REQDT.NewRow()
                        req_ID_Val = Guid.NewGuid
                        req_id = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si, sEntity)
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
                    row("UIC") = "None"
                    row("Status") = "L2_Formulate_PGM"
                    row("Create_Date") = DateTime.Now
                    row("Create_User") = si.UserName
                    row("Update_Date") = DateTime.Now
                    row("Update_User") = si.UserName

                    If isInsert Then
                        REQDT.Rows.Add(row)
                    End If

                    globals.SetObject("REQ_ID_VAL", req_ID_Val)
                    sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)

                    Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")
                    Dim Params As Dictionary(Of String, String) = New Dictionary(Of String, String)
                    Params.Add("CProbe", sSourcePosition)
                    params.Add("REQ_ID_VAL", REQ_ID_VAL.ToString)
                    params.Add("Entity", sEntity)
                    BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "Copy_Manpower", Params)

                    Dim customSubstVars As New Dictionary(Of String, String)
                    globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", $"L2_Formulate_PGM|L2_Formulate_PGM")
                    customSubstVars.Add("EntList", "E#" & sEntity)
                    customSubstVars.Add("WFScen", sScenario)
                    Dim currentYear As Integer = Convert.ToInt32(tm)
                    customSubstVars.Add("WFTime", $"T#{currentYear.ToString()},T#{(currentYear + 1).ToString()},T#{(currentYear + 2).ToString()},T#{(currentYear + 3).ToString()},T#{(currentYear + 4).ToString()}")
                    BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
                End Using
            End Using
            Return Nothing
        End Function
        #End Region

        #Region "Save Adjust Funding Line "
        Public Function Save_AdjustFundingLine() As xfselectionchangedTaskResult
            Dim req_IDs As String = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_PGM_REQTitleList", "NA")
            req_IDs = req_IDs.Split(" ").Last()

            Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
            Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si, wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
            WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)

            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                    sqlConn.Open()
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

                    Dim dt_Details As New DataTable()
                    Dim sqa2 As New SqlDataAdapter()
                    Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
                    Dim parentIds As New List(Of String)()
                    For Each parentRow As DataRow In dt.Rows
                        If Not IsDBNull(parentRow("CMD_PGM_REQ_ID")) Then
                            Dim columnValue As Object = parentRow("CMD_PGM_REQ_ID")
                            Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
                            parentIds.Add(reqIdAsGuid.ToString())
                        End If
                    Next
                    Dim singleCMD_PGM_REQ_ID As String = parentIds(0)
                    Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID = @SingleCMD_PGM_REQ_ID"

                    Dim detailsParams As SqlParameter() = New SqlParameter() {
                        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
                        New SqlParameter("@SingleCMD_PGM_REQ_ID", SqlDbType.NVarChar) With {.Value = singleCMD_PGM_REQ_ID}
                    }
                    sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_DETAILS_DT(sqa2, dt_Details, sql, detailsParams)

                    Dim sqa_xfc_cmd_pgm_req_details_audit = New SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT(sqlConn)
                    Dim SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT = New DataTable()
                    Dim SQL_Audit As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details_Audit WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID = @SingleCMD_PGM_REQ_ID"
                    sqa_xfc_cmd_pgm_req_details_audit.Fill_XFC_CMD_PGM_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, SQL_Audit, detailsParams)

                    Dim targetRow As DataRow = dt.Select($"REQ_ID = '{req_IDs}'").FirstOrDefault()
                    Dim APPN As String = args.NameValuePairs.XFGetValue("APPN")
                    Dim Entity As String = args.NameValuePairs.XFGetValue("Entity")
                    Dim MDEP As String = args.NameValuePairs.XFGetValue("MDEP")
                    Dim APE As String = args.NameValuePairs.XFGetValue("APEPT")
                    Dim DollarType As String = args.NameValuePairs.XFGetValue("DollarType")
                    Dim cTypecol As String = args.NameValuePairs.XFGetValue("CType")
                    Dim obj_class As String = args.NameValuePairs.XFGetValue("Obj_Class")
                    Dim req_ID_Val As Guid = targetRow("CMD_PGM_REQ_ID")

                    Dim targetRowFunding As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()

                    If SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Rows.Count > 0 Then
                        Dim drow As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
                        drow("Updated_UD1") = APPN
                        drow("Updated_UD2") = MDEP
                        drow("Updated_UD3") = APE
                        drow("Updated_UD4") = DollarType
                        drow("Updated_UD5") = cTypecol
                        drow("Updated_UD6") = obj_class
                    Else
                        Dim newrow As DataRow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.NewRow()
                        newrow("CMD_PGM_REQ_ID") = targetRowFunding("CMD_PGM_REQ_ID")
                        newrow("WFScenario_Name") = targetRowFunding("WFScenario_Name")
                        newrow("WFCMD_Name") = targetRowFunding("WFCMD_Name")
                        newrow("WFTime_Name") = targetRowFunding("WFTime_Name")
                        newrow("Entity") = targetRowFunding("Entity")
                        newrow("Account") = "Req_Funding"
                        newrow("Start_Year") = targetRowFunding("WFTime_Name")
                        newrow("Orig_UD4") = targetRowFunding("UD4")
                        newrow("Updated_UD4") = DollarType
                        newrow("Updated_UD5") = cTypecol
                        newrow("Updated_UD6") = obj_class
                        newrow("Create_Date") = DateTime.Now
                        newrow("Create_User") = si.UserName
                        SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Rows.Add(newrow)
                    End If

                    If Not String.IsNullOrWhiteSpace(APPN) Then targetRow("APPN") = APPN
                    If Not String.IsNullOrWhiteSpace(MDEP) Then targetRow("MDEP") = MDEP
                    If Not String.IsNullOrWhiteSpace(APE) Then targetRow("APE9") = APE
                    If Not String.IsNullOrWhiteSpace(DollarType) Then targetRow("Dollar_Type") = DollarType
                    If Not String.IsNullOrWhiteSpace(obj_class) Then targetRow("Obj_Class") = obj_class
                    If Not String.IsNullOrWhiteSpace(cTypecol) Then targetRow("CType") = cTypecol

                    targetRow("Update_Date") = DateTime.Now
                    targetRow("Update_User") = si.UserName

                    Dim targetRowFYValues As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
                    Dim FY1 As Decimal = targetRowFYValues("FY_1")
                    Dim FY2 As Decimal = targetRowFYValues("FY_2")
                    Dim FY3 As Decimal = targetRowFYValues("FY_3")
                    Dim FY4 As Decimal = targetRowFYValues("FY_4")
                    Dim FY5 As Decimal = targetRowFYValues("FY_5")

                    If targetRowFunding IsNot Nothing Then targetRowFunding.Delete()

                    Dim targetRowFundingNew As DataRow = dt_Details.NewRow()
                    targetRowFundingNew("CMD_PGM_REQ_ID") = req_ID_Val
                    targetRowFundingNew("WFScenario_Name") = wfInfoDetails("ScenarioName")
                    targetRowFundingNew("WFCMD_Name") = wfInfoDetails("CMDName")
                    targetRowFundingNew("WFTime_Name") = wfInfoDetails("TimeName")
                    targetRowFundingNew("Entity") = Entity
                    targetRowFundingNew("Account") = "Req_Funding"
                    targetRowFundingNew("Unit_of_Measure") = "Funding"
                    targetRowFundingNew("Flow") = targetRow("Status")

                    If Not String.IsNullOrWhiteSpace(APPN) Then targetRowFundingNew("UD1") = APPN Else targetRowFundingNew("UD1") = targetRow("APPN")
                    If Not String.IsNullOrWhiteSpace(MDEP) Then targetRowFundingNew("UD2") = MDEP Else targetRowFundingNew("UD2") = targetRow("MDEP")
                    If Not String.IsNullOrWhiteSpace(APE) Then targetRowFundingNew("UD3") = APE Else targetRowFundingNew("UD3") = targetRow("APE9")
                    If Not String.IsNullOrWhiteSpace(DollarType) Then targetRowFundingNew("UD4") = DollarType Else targetRowFundingNew("UD4") = targetRow("Dollar_Type")
                    If Not String.IsNullOrWhiteSpace(obj_class) Then targetRowFundingNew("UD6") = obj_class Else targetRowFundingNew("UD6") = targetRow("Obj_Class")
                    If Not String.IsNullOrWhiteSpace(cTypecol) Then targetRowFundingNew("UD5") = cTypecol Else targetRowFundingNew("UD5") = targetRow("CType")

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

                    sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details, sqa2)
                    sqaReader.Update_XFC_CMD_PGM_REQ(dt, sqa)
                    sqa_xfc_cmd_pgm_req_details_audit.Update_XFC_CMD_PGM_REQ_DETAILS_AUDIT(SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, sqa)
                End Using
            End Using

            Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
            selectionChangedTaskResult.IsOK = True
            Return selectionChangedTaskResult
        End Function
        #End Region

        #Region "saveweightprioritization"
        Public Function saveweightprioritization() As Object
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
            Dim sEntity As String = args.NameValuePairs.XFGetValue("entity")
            Dim sWeightPrioritizationMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#RMW_Cycle_Config_Annual:T#" & sTime & ":V#Periodic:A#GBL_Priority_Cat_Weight:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim TotPct As Double = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sWeightPrioritizationMbrScript).DataCellEx.DataCell.CellAmount

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

        #Region "load_req_detailsdashboard"
        Public Function load_req_detailsdashboard() As XFLoadDashboardTaskResult
            Dim LoadDBTaskResult As New XFLoadDashboardTaskResult()
            LoadDBTaskResult.ChangeCustomSubstVarsInDashboard = True
            Dim reqTitle = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")

            If reqTitle <> String.Empty Or Not String.IsNullOrEmpty(reqTitle) Then
                UpdateCustomSubstVar(LoadDBTaskResult, "IV_CMD_PGM_REQDetailsShowHide", "CMD_PGM_0_Body_REQDetailsMain")
            Else
                UpdateCustomSubstVar(LoadDBTaskResult, "IV_CMD_PGM_REQDetailsShowHide", "CMD_PGM_0_Body_REQDetailsHide")
            End If
            Return LoadDBTaskResult
        End Function
        #End Region

        #Region "Delete Requirement ID"
        Public Function DeleteRequirementID() As XFSelectionChangedTaskResult
            Try
                Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
                Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
                Dim req_IDsRaw As String = args.NameValuePairs.XFGetValue("req_IDs", "")
                Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "")
                Dim tm As String = wfInfoDetails("TimeName")
                Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)

                Dim Req_ID_List As List(Of String) = New List(Of String)()
                Dim raw As String = req_IDsRaw.Trim()

                If String.IsNullOrWhiteSpace(raw) Then
                    Req_ID_List = New List(Of String)()
                ElseIf raw.Contains(",") Then
                    Req_ID_List = StringHelper.SplitString(raw, ",").Select(Function(s) s.Trim()).Where(Function(s) Not String.IsNullOrWhiteSpace(s)).ToList()
                ElseIf raw.Contains(" "c) Then
                    Req_ID_List.Add(raw.Split(" "c).Last().Trim())
                Else
                    Req_ID_List.Add(raw)
                End If

                Req_ID_List = Req_ID_List.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                Dim REQDT As New DataTable()
                Dim REQDetailDT As New DataTable()
                Dim REQCMTDT As New DataTable()
                Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)

                Using connection As New SqlConnection(dbConnApp.ConnectionString)
                    connection.Open()
                    Dim sqa = New SqlDataAdapter()
                    Dim sqa_xfc_CMD_PGM_req = New SQA_XFC_CMD_PGM_REQ(connection)
                    Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
                    Dim paramList As New List(Of SqlParameter) From {
                        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                    }

                    Dim sqa_xfc_CMD_PGM_req_details = New SQA_XFC_CMD_PGM_REQ_Details(connection)
                    Dim detailSql As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details AS Req LEFT JOIN XFC_CMD_PGM_REQ AS Dtl ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID WHERE Req.WFScenario_Name = @WFScenario_Name AND Req.WFCMD_Name = @WFCMD_Name AND Req.WFTime_Name = @WFTime_Name"
                    Dim detailParamList As New List(Of SqlParameter) From {
                        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
                    }

                    Dim sqa_xfc_CMD_PGM_req_cmt = New SQA_XFC_CMD_PGM_REQ_CMT(connection)
                    Dim sql_cmt As String = $"SELECT * From XFC_CMD_PGM_REQ_Cmt as Cmt LEFT JOIN XFC_CMD_PGM_REQ AS Req ON Req.CMD_PGM_REQ_ID = Cmt.CMD_PGM_REQ_ID WHERE 1=1"
                    Dim paramListcmt As New List(Of SqlParameter) From {}

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

                    Dim entitiesFromReqList As New List(Of String)
                    For Each reqId As String In Req_ID_List
                        Dim rows As DataRow() = REQDT.Select($"REQ_ID = '{reqId}'")
                        Dim GUIDROW As String = rows.FirstOrDefault().Item("CMD_PGM_REQ_ID").ToString
                        Dim ent As String = rows.FirstOrDefault().Item("Entity").ToString
                        entitiesFromReqList.Add(ent)

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
                    Next

                    sqa_xfc_CMD_PGM_req_details.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)
                    sqa_xfc_CMD_PGM_req.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
                    sqa_xfc_CMD_PGM_req_cmt.Update_XFC_CMD_PGM_REQ_CMT(REQCMTDT, sqa)

                    Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")
                    Dim customSubstVars As New Dictionary(Of String, String)
                    If Not String.IsNullOrWhiteSpace(sEntity) Then
                        customSubstVars.Add("EntList", "E#" & sEntity)
                    Else
                        Dim entities As String
                        entitiesFromReqList = entitiesFromReqList.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                        entities = String.Join(",", entitiesFromReqList.Select(Function(r) "E#" & r))
                        customSubstVars.Add("EntList", entities)
                    End If
                    customSubstVars.Add("WFScen", sScenario)
                    Dim currentYear As Integer = Convert.ToInt32(tm)
                    customSubstVars.Add("WFTime", $"T#{currentYear.ToString()},T#{(currentYear + 1).ToString()},T#{(currentYear + 2).ToString()},T#{(currentYear + 3).ToString()},T#{(currentYear + 4).ToString()}")
                    BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)
                End Using
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "UpdateCustomSubstVar"
        Private Sub UpdateCustomSubstVar(ByRef Result As XFLoadDashboardTaskResult, ByVal key As String, ByVal value As String)
            If Result.ModifiedCustomSubstVars.ContainsKey(key) Then
                Result.ModifiedCustomSubstVars.XFSetValue(key, value)
            Else
                Result.ModifiedCustomSubstVars.Add(key, value)
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
                        Dim sqa = New SqlDataAdapter()
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
                        Dim ReqID As String = args.NameValuePairs.XFGetValue("REQ_ID", "")

                        If String.IsNullOrWhiteSpace(ReqID) Then
                            Return Nothing
                        Else
                            Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
                            Dim Entity As String = REQ_ID_Split(0)
                            Dim RequirementID As String = REQ_ID_Split(1)
                            Dim sql As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND REQ_ID  = @REQ_ID"

                            Dim sqlParams As SqlParameter() = New SqlParameter() {
                                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
                                New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = RequirementID}
                            }
                            sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sql, sqlparams)

                            Dim REQ_ID_Val_guid As Guid = Guid.Empty
                            If dt.Rows.Count > 0 Then
                                REQ_ID_Val_guid = Convert.ToString(dt.Rows(0)("CMD_PGM_REQ_ID")).XFConvertToGuid
                            Else
                                Return Nothing
                            End If

                            Dim dt_Attachment As New DataTable()
                            Dim sqa2 As New SqlDataAdapter()
                            Dim sqaReaderAttachment As New SQA_XFC_CMD_PGM_REQ_Attachment(sqlConn)
                            Dim sqlAttach As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Attachment Where CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID"
                            Dim sqlParamsAttach As SqlParameter() = New SqlParameter() {
                                New SqlParameter("@CMD_PGM_REQ_ID", SqlDbType.uniqueidentifier) With {.Value = REQ_ID_Val_guid}
                            }
                            sqaReaderAttachment.Fill_XFC_CMD_PGM_REQ_Attachment_DT(sqa2, dt_Attachment, sqlAttach, sqlParamsAttach)

                            Dim Tatgetrow As DataRow = dt_Attachment.NewRow()
                            dt_Attachment.Rows.Add(Tatgetrow)
                            Tatgetrow("CMD_PGM_REQ_ID") = REQ_ID_Val_guid

                            Dim FilePath As String = args.NameValuePairs.XFGetValue("FilePath")
                            Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
                            Tatgetrow("Attach_File_Name") = fileInfo.XFFile.FileInfo.Name
                            Tatgetrow("Attach_File_Bytes") = fileInfo.XFFile.ContentFileBytes

                            sqaReaderAttachment.Update_XFC_CMD_PGM_REQ_Attachment(dt_Attachment, sqa2)
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
                Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
                Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(sREQ, " ")
                Dim RequirementID As String = REQ_ID_Split(1)
                Dim FileName As String = args.NameValuePairs.XFGetValue("File")
                Dim File_Name_List As List(Of String) = StringHelper.SplitString(FileName, ",")

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim dt As New DataTable()
                        Dim sqa2 = New SqlDataAdapter()
                        Dim sqaReaderAttachment As New SQA_XFC_CMD_PGM_REQ_Attachment(sqlConn)
                        Dim sqlAttach As String = $"SELECT * From XFC_CMD_PGM_REQ_Attachment as Att LEFT JOIN XFC_CMD_PGM_REQ AS Req ON Req.CMD_PGM_REQ_ID = Att.CMD_PGM_REQ_ID WHERE REQ_ID = @REQ_ID"

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

                        sqaReaderAttachment.Fill_XFC_CMD_PGM_REQ_Attachment_DT(sqa2, dt, sqlAttach, paramList.ToArray())

                        If dt.Rows.Count = 0 Then
                            Return New XFSelectionChangedTaskResult() With {.IsOK = True, .ShowMessageBox = True, .Message = "No documents found for the selected Requirement."}
                        End If

                        Dim sFolderPath As String = "Documents/Users/" & si.UserName
                        Dim sFilePath As String
                        Dim fileBytes As Byte()
                        Dim sFileName As String

                        If dt.Rows.Count = 1 Then
                            Dim row As DataRow = dt.Rows(0)
                            sFileName = row.Field(Of String)("Attach_File_Name")
                            fileBytes = row.Field(Of Byte())("Attach_File_Bytes")
                            sFilePath = $"{sFolderPath}/{sFileName}"
                        Else
                            sFileName = $"Attachments_{RequirementID}.zip"
                            sFilePath = $"{sFolderPath}/{sFileName}"

                            Using memStream As New MemoryStream()
                                Using zip As New ZipArchive(memStream, ZipArchiveMode.Create, True)
                                    For Each row As DataRow In dt.Rows
                                        Dim entryFileName As String = row.Field(Of String)("Attach_File_Name")
                                        Dim entryFileBytes As Byte() = row.Field(Of Byte())("Attach_File_Bytes")
                                        Dim zipEntry = zip.CreateEntry(entryFileName)
                                        Using entryStream As Stream = zipEntry.Open()
                                            entryStream.Write(entryFileBytes, 0, entryFileBytes.Length)
                                        End Using
                                    Next
                                End Using
                                fileBytes = memStream.ToArray()
                            End Using
                        End If

                        Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, sFilePath)
                        Dim objXFFile As New XFFile(objXFFileInfo, String.Empty, fileBytes)
                        BRApi.FileSystem.InsertOrUpdateFile(si, objXFFile)

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
        Public Function SendStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal Status As String = "", Optional ByVal ReqID As String = "")
            Try
                Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
                Dim UserName As String = si.UserName
                Dim DataEmail As New DataTable()

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim SqlString As String = "SELECT Title, Status, Create_User, Create_Date, Notification_List_Emails, Entity From XFC_CMD_PGM_REQ Where WFScenario_Name = @WFScenario_Name And WFCMD_Name = @WFCMD_Name And WFTime_Name = @WFTime_Name AND REQ_ID = @REQ_ID"

                        Dim paramlist As New List(Of SqlParameter)()
                        paramlist.Add(New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")})
                        paramlist.Add(New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")})
                        paramlist.Add(New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")})
                        paramlist.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = ReqID})

                        Using sqlCommand As New SqlCommand(SqlString, sqlConn)
                            sqlCommand.Parameters.AddRange(paramlist.ToArray())
                            Using sqa As New SqlDataAdapter(sqlCommand)
                                sqa.Fill(DataEmail)
                            End Using
                        End Using
                    End Using
                End Using

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
                        Dim potentialDelimiters As Char() = {","c, ";"c, "|"c, " "c}
                        Dim emailDelimiter As Char = DetectDelimiter(EmailIs, potentialDelimiters)
                        Dim emailArray As String() = EmailIs.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries)

                        For Each singleEmail As String In emailArray
                            Dim trimmedEmail As String = singleEmail.Trim()
                            If Not String.IsNullOrWhiteSpace(trimmedEmail) Then
                                StatusChangeEmailIs.Add(trimmedEmail)
                            End If
                        Next
                    End If

                    Dim StatusChangeSubject As String = "Requirement Status Change"
                    Dim StatusChangeBody As String = "A Requirement Request for Funds Center: " & FundCenter & " with Requirement Title: " & ReqTitle & " has changed status to **" & ReqStatus & "**."
                    StatusChangeBody &= " Submitted by: " & UserName & " - " & CreateDate & vbCrLf & BodyDisclaimerBody

                    If StatusChangeEmailIs.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(EmailConnectorStr) Then
                    End If
                Next
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Helper Functions"
        Private Function DetectDelimiter(ByVal inputString As String, ByVal potentialDelimiters As Char()) As Char
            Dim defaultDelimiter As Char = If(potentialDelimiters IsNot Nothing AndAlso potentialDelimiters.Length > 0, potentialDelimiters(0), ","c)
            If String.IsNullOrWhiteSpace(inputString) Then Return defaultDelimiter

            For Each delimiter As Char In potentialDelimiters
                If inputString.Contains(delimiter) Then
                    If inputString.Split(delimiter).Length > 1 Then Return delimiter
                End If
            Next
            Return defaultDelimiter
        End Function
        #End Region
    End Class
End Namespace