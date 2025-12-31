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
Imports Workspace.GBL.GBL_Assembly
Imports Microsoft.Data.SqlClient

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_PGM_String_Helper
	Public Class MainClass
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args

				Select Case args.FunctionName.ToLower()
					Case "getsortedappnlist"
						Return Me.GetSortedAppnList()
					Case "displaydashboard"
						Return Me.displaydashboard()
					Case "createlbltext"
						Return Me.createlbltext()
					Case "showhide"
						Return Me.ShowHide()
					Case "showhidemulti"
						Return Me.ShowHideMulti()
					Case "showhidelabel"
						Return Me.ShowHideLabel()
					Case "showhidelabelmulti"
						Return Me.ShowHideLabelMulti()	
					Case "procctrllblmsg"
						Return Me.ProcCtrlLblMsg()
					Case "procctrllblmsgmulti"
						Return Me.ProcCtrlLblMsgMulti()	
					Case "getfileguidance"
						Return Me.GetFileGuidance()		
					Case "reqretrievecache"
						Return Me.REQRetrieveCache()
					Case "getusercommandpgmexport"
						Return Me.GetUserCommandPGMExport()
					Case "getmassuploadstatus"
						Return Me.GetMassUploadStatus()	
					Case "cert_summary_report"
						Return Me.Cert_Summary_Report()		
					Case "org_totals_by_sag"
						Return Me.Org_Totals_by_SAG()	
					Case "exportreportmdepfilterlist"
						Return Me.ExportReportMDEPFilterList()
					Case "displayrateinfo"
						Return Me.DisplayRateInfo()	
					Case "retrievepbposition"
						Return Me.RetrievePBPosition()
					Case "getreqid"
						Return Me.GetREQID()
					Case "getdemotereqs"
						Return Me.GetDemotereqs()	
					Case "displaypegdashboard"
						Return Me.displaypegdashboard()
					Case "displaybaseinfodashboard"
						Return Me.displaybaseinfodashboard()
					Case "displaydynflow"
						Return Me.displaydynflow()
					
				End Select

'=================Still referenced but not configured to new standards====================================================================
#Region "Get Linked Report Read Only / Visibility"
	'Return U8#ReadOnlyAnnotation or Visibility for Linked Report 
				If args.FunctionName.XFEqualsIgnoreCase("GetLRVisibility") Then				
					Return Me.GetLRVisibility(si,globals,api,args)
				End If	
				
#End Region 'referenced
#Region  "Get Current Content Cbx by WF"
	'Return the content cbx prompt for the current WF step 
				If args.FunctionName.XFEqualsIgnoreCase("GetContentCbxByWF") Then				
					Return Me.GetContentCbxByWF(si,globals,api,args)
				End If	
				
#End Region 'referenced
#Region "REQControlsEntityFilter"
				If args.FunctionName.XFEqualsIgnoreCase("REQControlsEntityFilter") Then	
					Return Me.REQControlsEntityFilter(si, globals, api, args)

				End If	
#End Region 'referenced
#Region "REQReportSubConsolidateTime"
'Return 5 years Time for subconsolidate data management sequence for REQ Report using Selected Scenario
				If args.FunctionName.XFEqualsIgnoreCase("REQReportSubConsolidateTime") Then	
					Return Me.REQReportSubConsolidateTime(si, globals, api, args)

				End If	
#End Region 'referenced
#Region "MDEPStripTime"
				If args.FunctionName.XFEqualsIgnoreCase("MDEPStripTime") Then	
					Return Me.MDEPStripTime(si, globals, api, args)

				End If	
#End Region 'referenced
#Region "MemberFilterTotal"
				If args.FunctionName.XFEqualsIgnoreCase("MemberFilterTotal") Then	
					Return Me.MemberFilterTotal(si, globals, api, args)

				End If	
#End Region ' referenced
#Region "Display Upload Guidance Btn"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayUploadGuidanceBtn") Then
					Return Me.DisplayUploadGuidanceBtn(si, globals, api, args)
				End If
#End Region 'Used by btn_CMD_PGM_GuidanceFileUpload
#Region "Display CMD Rows" 
				If args.FunctionName.XFEqualsIgnoreCase("DisplayCmdRows") Then
					Return Me.DisplayCmdRows(si, globals, api, args)
				End If
#End Region 'Used by cv: CMD_PGM_YOYPercentLimit
#Region "YOYLimitColorCode"
				If args.FunctionName.XFEqualsIgnoreCase("YOYLimitColorCode") Then	
					Return Me.YOYLimitColorCode(si, globals, api, args)
				End If
#End Region 'Still referenced

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Get Linked Report Read Only / Visibility"
		'Return U8#ReadOnlyAnnotation or Visibility for Linked Report
		'Function works by comparing the workflow name and the TrueValue passed in as an arg from the cv or component. Return desired behavior based on the mode, and the result of the comparison
		'ex: btn_REQPRO_AAAAAA_0CaAa_SubmitREQ__Shared
		Public Function GetLRVisibility(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
'BRapi.ErrorLog.LogMessage(si, "Here")
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name	
			Dim sTrueVal As String = args.NameValuePairs.XFGetValue("TrueValue").Trim
			Dim sMode As String = args.NameValuePairs.XFGetValue("mode").Trim
			Dim oTrueVal As List(Of String) = sTrueVal.Split(",").ToList
			Dim bCheck As Boolean = False
			For Each TrueVal As String In oTrueVal
				If wfProfileName.XFContainsIgnoreCase(TrueVal) Then bCheck = True
			Next
'BRapi.ErrorLog.LogMessage(si, "Is True")
			If bCheck = True Then
					If sMode.XFContainsIgnoreCase("RO") Then
						Return ""
					Else If sMode.XFContainsIgnoreCase("isVisible") Then
						Return "True"
					End If
			Else
				If sMode.XFContainsIgnoreCase("RO") Then
'BRapi.ErrorLog.LogMessage(si, "Read Only")
					Return "U8#ReadOnlyAnnotation"
				Else If sMode.XFContainsIgnoreCase("isVisible") Then
					Return "False"
				End If				
			End If
			Return Nothing
		End Function
#End Region ' Reference by general comment components
#Region "Get Current Content Cbx by WF"
		'Return the content cbx prompt for the current WF step
		'This function evaluates the workflow name and return the correct cbx prompt for the dashboard dropdown. Used to get the value or the prompt, i.e. the dashboard name
		'Use case: btn_REQPRO_RefreshREQ - passed in as an arg to the show/hide BR in solutionhelper where the value will be cached. The cached dc value will then be used for other BR like the XFBR GetLRBoundParam above
		Public Function GetContentCbxByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
'BRapi.ErrorLog.LogMessage(si, "Here")
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name	
			Dim sPrompt As String = "|!prompt_REQPRO_REQXXX_0CaAb_cbx_ContentList!|" 
			Select Case True
			Case wfProfileName.XFContainsIgnoreCase("Formulate")
				sPrompt = "|!prompt_cbx_REQPRO_REQCRT_0CaAb_ContentList!|"
			Case wfProfileName.XFContainsIgnoreCase("Validate")
				sPrompt = sPrompt.Replace("XXX","VAL")
			Case wfProfileName.XFContainsIgnoreCase("Manage") 
				sPrompt = sPrompt.Replace("XXX","MGT")
			Case wfProfileName.XFContainsIgnoreCase("Approve") 
				sPrompt = sPrompt.Replace("XXX","PRT")
			Case wfProfileName.XFContainsIgnoreCase("Prioritize")
				sPrompt = sPrompt.Replace("XXX","PRT")
			Case wfProfileName.XFContainsIgnoreCase("Review")
				sPrompt = "|!prompt_cbx_REQPRO_REQREV_0CaAb_ContentList!|"
			End Select
			Return sPrompt
		End Function
#End Region ' Referenced by btn_REQREP_MDPBRF_0CaAa_RefreshCheckForBlank__Shared
#Region "REQControlsEntityFilter"
		Public Function REQControlsEntityFilter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")
'brapi.ErrorLog.LogMessage(si,"entity = " & sEntity)		
'brapi.ErrorLog.LogMessage(si,"sCube = " & sCube)	
				Dim sReturnString As String = ""
				If String.IsNullOrEmpty(sEntity) Then
					sReturnString = "E#" & sCube & ".base.where(Name DoesNotContain _FC)"
				Else
					sReturnString = "E#" & sEntity & ".base.where(Name DoesNotContain _FC)"
				End If
'brapi.ErrorLog.LogMessage(si,"sReturnString = " & sReturnString)					
				Return sReturnString
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region ' Referenced by cv REQ_Reporting_Controls
#Region "REQReportSubConsolidateTime"
		'Return 5 years Time for subconsolidate data management sequence for REQ Report
		Public Function REQReportSubConsolidateTime(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
				Dim sReturnString As String = ""
				Dim iScenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario)
				Dim iTime As Integer = BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioId)/1000000
				For i As Integer = 0 To 4 Step 1
					If i = 0 Then
						sReturnString = $"T#{iTime}"
					Else
						sReturnString = $"{sReturnString},T#{iTime + i}"
					End If
				Next
'BRApi.ErrorLog.LogMessage(si, $"sReturnString = {sReturnString}")
				Return sReturnString
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region ' Referenced data management step Report_SubConsolidate
#Region "MDEPStripTime"
		Public Function MDEPStripTime(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		
			Dim cvScenario As String = args.NameValuePairs.XFGetValue("cvScenario")
			Dim sTimeStrip As String = Mid(cvScenario, 6)
'BRapi.ErrorLog.LogMessage(si, sTimeStrip)

'			Dim sScenarioId As Integer = ScenarioDimHelper.GetIdFromName(si, cvScenario)
'			Dim iStartTime As Integer = BRApi.Finance.Scenario.GetWorkflowStartTime(si, sScenarioId)/1000000
'			Dim sStartTime As String = iStartTime
'BRapi.ErrorLog.LogMessage(si, "sStartTime: " & sStartTime)

			Return sTimeStrip
		End Function
		
#End Region ' Referenced by cv: REQ_MDEP_Briefing
#Region "MemberFilterTotal"
		Public Function MemberFilterTotal(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
				' Example: XFBR(z_MH_REQ_String_Helper, MemberFilterTotal, MemberFilter = [U1#Appropriation.Descendants.Where((Text3 Contains APPNLevel=L2) And (Name DoesNotContain _General)):U4#BASE], WhichDim = U1#, RemainingFilter = :U4#Base, DimName = U1_APPN_FUND)
				Dim cvMemberFilter As String = args.NameValuePairs.XFGetValue("MemberFilter")
				Dim WhichDim As String = args.NameValuePairs.XFGetValue("WhichDim")
				Dim RemainingFilter As String = args.NameValuePairs.XFGetValue("RemainingFilter") 'optional
				Dim DimName As String = args.NameValuePairs.XFGetValue("DimName")
'BRapi.ErrorLog.LogMessage(si, RemainingFilter)

    			Dim mMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, DimName, cvMemberFilter, True, Nothing, Nothing)
		
    			Dim str As String = ""
				
		   		For Each member As MemberInfo In mMemberList
		        	str += WhichDim & member.Member.Name & RemainingFilter & "+"
				
		    	Next 
				'remove last "+"
		   		str.Remove(str.Length - 1, 1) 
				'create member filter for row header
   				Dim finalFilter As String = "GetDataCell(" & str.Remove(str.Length - 1, 1) & ")"
'BRapi.ErrorLog.LogMessage(si, finalFilter)

   				Return finalFilter
	
			'End If
			
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region ' Referenced by cv: REQ_MDEP_Briefing
#Region "Display Upload Guidance Btn"
'Created 09/26/2024 by Fronz - this is called by the btn_REQPRO_AAAAAA_0Cb__Guidance_FileUpload__Shared IsVisible Display Format
'Purpose: If the WF is marked "Completed" then the btn hides from the user
	Public Function DisplayUploadGuidanceBtn(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
		If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
			Return "False"
		Else
			Return "True"
		End If			
		
	End Function
#End Region ' Come back to - Fronz
#Region "DisplayCmdRows"
	Public Function DisplayCmdRows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim currentProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
		Dim sWFProfile As String = currentProfile.Name
		If  sWFProfile.Contains("CMD") Then		
			Return "True"
		Else
			Return "False"
		End If
	End Function
#End Region ' Come back to - Fronz

#Region "YOYLimitColorCode"
	Public Function YOYLimitColorCode(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			
			Dim Account As String = args.NameValuePairs.XFGetValue("Account").Trim

			Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0)
			Dim sYOYPctLimitMbrScript As String  = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & iWFYear & ":C#USD:E#" & sACOM & "_General"
			sYOYPctLimitMbrScript &= ":V#Periodic:A#" & Account & ":O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			Dim PrctLimit As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sYOYPctLimitMbrScript).DataCellEx.DataCell.CellAmount
			PrctLimit = Math.Round(PrctLimit, 1)

			Return "If (CellAmount < 2) Then 
    NumberFormat = [0.0\%] 
End if
'If (CellAmount >= 2) AND (CellAmount < " & PrctLimit & ") Then 
'    BackgroundColor = LightYellow, NumberFormat = [0.0\%] 
'End if
If (CellAmount >= " & PrctLimit & ") Then 
    BackgroundColor = LightPink, NumberFormat = [0.0\%] 
End if"
		
	End Function
#End Region '  Come back to - Fronz
'=============================================================================================================================]
#Region "Constants"
Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
Public GBL_DataSet As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardDataSet.GBL_DB_DataSet.MainClass()
#End Region

#Region "Utilities: Get DataBuffer"
	Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object,ByRef args As DashboardStringFunctionArgs)
		'Dim filter = globals.GetStringValue("Filter","NA")
		'Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")				
		Dim Dictionary As New Dictionary(Of String, String)
		'BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,workspaceID, "workspace.GBL.GBL Objects.WSMU","Global_Buffers",Dictionary,customcalculatetimetype.CurrentPeriod)
		BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Global_Buffers","GetCVDataBuffer",Dictionary,customcalculatetimetype.CurrentPeriod)
	End Sub

#End Region

#Region "GetSortedAppnList"
		Public Function GetSortedAppnList() As String
			
			Dim u1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN")		
			Dim appn_List As New List(Of MemberInfo) 
			appn_List = BRApi.Finance.Members.GetMembersUsingFilter(si, u1DimPk, "U1#Appn.Base.Options(Cube=Army,ScenarioType=LongTerm,MergeMembersFromReferencedCubes=False)", True)
			If appn_List.Count > 0 Then
				Dim sortedMemberInfos As List(Of MemberInfo) = appn_List.OrderBy(Function(memberInfo) memberInfo.Member.Name).ToList()
				Dim prefixedMemberNames As List(Of String) = sortedMemberInfos.Select(Function(mi) "U1#" & mi.Member.Name).ToList()
				Return String.Join(",", prefixedMemberNames)
			Else
				Return "U1#Top:Name(No Appns returned)"
			End If
			
		End Function
		
#End Region

#Region "DisplayDashboard"
		Public Function displaydashboard() As String
			Dim reqTitle = args.NameValuePairs.XFGetValue("REQTitle")
			Dim sDashName = args.NameValuePairs.XFGetValue("DashName",String.Empty)
'brapi.ErrorLog.LogMessage(si, "TRV: REQ Title: " & reqTitle)			
			If String.IsNullOrEmpty(reqTitle)
				Dim wsID = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "10 CMD PGM")
				Dim cmd_PGM_Base_Info As New DashboardFileResource 
				cmd_PGM_Base_Info = BRApi.Dashboards.FileResources.GetFileResource(si,False, wsID, "CMD_PGM_Base_Info.xlsx")
'BRAPI.ErrorLog.LogMessage(si,"Hit: " & cmd_PGM_Base_Info.FileName)
				BRApi.Dashboards.FileResources.SaveFileResource(si, False, cmd_PGM_Base_Info)
				Return "CMD_PGM_0_Body_REQDetailsHide"
			End If
			
			Select Case sDashName
				Case "BaseInfoGeneralText"		
					Return "CMD_PGM_0b_Body_BaseInfoGeneralText"
				Case "FormulateMprREQ"		
					Return "CMD_PGM_3a1_Body_FormulateMprREQ"
				Case Else	
					Return "CMD_PGM_0_Body_REQDetailsMain"
			End Select
		End Function
#End Region

#Region "DisplayRateInfo"
		Public Function DisplayRateInfo() As String
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")
'BRApi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ": sEntitySelection=" & sEntitySelection) 				
			If 	sCube <> "" Then wfCube = sCube

			Dim sScenarioName As String = args.NameValuePairs.XFGetValue("Scenario")
			If sScenarioName <> "" Then 	wfScenarioName = sScenarioName

			Dim sTimeName As String = wfScenarioName.Substring(9,4)

			Dim iYear As Integer = CInt(sTimeName)
'BRApi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ": sPGMSelection=" & sPGMSelection & "  wfScenarioName=" & wfScenarioName & "  wfTimeName=" & wfTimeName ) 				
			Dim Account As String = args.NameValuePairs.XFGetValue("Account").Trim

			Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0)
			Dim sYOYPctLimitMbrScript As String  = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & iWFYear & ":C#USD:E#" & sACOM & "_General"
			sYOYPctLimitMbrScript &= ":V#Periodic:A#" & Account & ":O#Forms:I#None:F#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			Dim PrctLimit As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sYOYPctLimitMbrScript).DataCellEx.DataCell.CellAmount
			PrctLimit = Math.Round(PrctLimit, 1)
			
			Return PrctLimit & "%"
		End Function
#End Region

#Region "CreateLblText"
		Public Function CreateLblText() As String
	'brapi.ErrorLog.LogMessage(si, $"inside create lable")	
			Dim sLabelName As String = args.NameValuePairs.XFGetValue("LabelName")
			'============Used for confirm rollforward============================
			Dim sLabelType As String = args.NameValuePairs.XFGetValue("LabelType")
			'=====================================================================
			Dim sREQTitle As String = args.NameValuePairs.XFGetValue("REQTitle")
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim iWfYearPrior As Integer = iWFYear - 1
			Dim wFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	'brapi.ErrorLog.LogMessage(si, $"sREQTitle {sREQTitle}")		
	
	'brapi.ErrorLog.LogMessage(si, $"sLabelName {sLabelName}")		
			Select Case sLabelName
			Case "ConfirmRollForward"
				If sLabelType.XFContainsIgnoreCase("warning")
					Return $"!!!!! WARNING !!!!!{vbCrLf}{vbCrLf}{iWFYear} requirements already exist.{vbCrLf} {vbCrLf}If you continue, any {iWfYearPrior} requirements that were selected and already exist in {iWFYear} will be overwritten.{vbCrLf}"
				Else
					Return Nothing
				End If
			Case "DemoteREQ"	
				Return $"Are you sure you wish to DELETE: {sREQTitle}"
			End Select	
			
		End Function
#End Region
'****Component Showhide*****
#Region "ShowHide"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ShowHide() As String	
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")
'brapi.ErrorLog.LogMessage(si, $"sEntity {sEntity}")			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
	'brapi.ErrorLog.LogMessage(si, $"wfProfileName: {wfProfileName}")		
			Dim sComponentName As String = args.NameValuePairs.XFGetValue("ComponentName")
			'Manpower Specific
			Dim sREQ_ID As String = args.NameValuePairs.XFGetValue("REQ_ID")
	'brapi.ErrorLog.LogMessage(si, $"sComponentName: {sComponentName}")		
			#Region "Workflow complete and revert buttons"		
			Select Case sComponentName				
				Case "WorkflowAccess"
					If wfProfileFullName.XFContainsIgnoreCase("Manage CMD Requirements") Then 
						Return "True"
					Else
						Return "False"
					End If
			End Select	
			#End Region	
			'Grab workflow status (locked or unlocked)
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_PGM_Utilities.GetProcCtrlVal(si,sEntity)
			
			Select Case sComponentName							
				#Region "Manpower approve and revert buttons"
				Case "ManpowerApprove"
					Dim sql As String = String.empty
					SQL = $"Select distinct Flow
						FROM XFC_CMD_PGM_REQ_Details
						where Entity = '{sEntity}'
						and WFScenario_Name = '{sScenario}'
						and Unit_of_Measure = 'CIV_COST'"
	'brapi.ErrorLog.LogMessage(si, $"manpower approved: {SQL}")					
					Dim dtAll As New DataTable
					Dim sStatus As String = ""
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
					End Using
					For Each row As DataRow In dtAll.rows
						sStatus = row.Item(0)
					Next
					
	'brapi.ErrorLog.LogMessage(si, $"manpower approved: {sStatus}")				
					If sStatus.XFContainsIgnoreCase("Formulate") Then 
						Return "True"
					Else 
						Return "False"
					End If 
				
				Case "ManpowerRevert"
					Dim sql As String = String.empty
					SQL = $"Select distinct Flow
						FROM XFC_CMD_PGM_REQ_Details
						where Entity = '{sEntity}'
						and WFScenario_Name = '{sScenario}'
						and Unit_of_Measure = 'CIV_COST'"
	'brapi.ErrorLog.LogMessage(si, $"manpower Revert: {SQL}")											
					Dim dtAll As New DataTable
					Dim sStatus As String = ""
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
					End Using
					For Each row As DataRow In dtAll.rows
						sStatus = row.Item(0)
					Next
	'brapi.ErrorLog.LogMessage(si, $"manpower Revert: {sStatus}")				
					If sStatus.XFContainsIgnoreCase("Final") Then 
						Return "True"
					Else 
						Return "False"
					End If 						
				#End Region
				#Region "Sub CMD Approve & CMD Finalize components"
				Case "SubCMDApprove"
					If wfProfileFullName.XFContainsIgnoreCase("Approve CMD Requirements") Then 
						Return "False"
					End If
				Case "CMDFinalize"
					If wfProfileFullName.XFContainsIgnoreCase("Approve Requirements") Then
						Return "False"
					End If
				#End Region
				#Region "WeightPriortizationComponents"
				Case "WeightPriorLbl"
				
					Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
	                Dim WFScenario As String = "RMW_Cycle_Config_Annual"
	                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
	                Dim REQTime As String = WFYear
	                Dim priCatMembers As List(Of MemberInfo)
	                Dim priFilter As String = "A#GBL_Priority_Cat_Weight.Children"
	                Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
	                priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
	        
	                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sEntity & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	
	                Dim priCatNameList As New List(Of String)
	                Dim priCatName As String = ""
	                'Dim priCatWeight As Decimal
	                
	                'Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
	                For Each pricat As MemberInfo In priCatMembers
	              	  priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
						If Not String.IsNullOrEmpty(priCatName)		
							priCatNameList.Add(priCatName)
						End If
	                Next
				
					If Not priCatNameList.Count = 0  Then 
						Return "False"
					End If
				
				Case "CalculateWeightedScorebtn"
					Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
	                Dim WFScenario As String = "RMW_Cycle_Config_Annual"
	                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
	                Dim REQTime As String = WFYear
	                Dim priCatMembers As List(Of MemberInfo)
	                Dim priFilter As String = "A#GBL_Priority_Cat_Weight.Children"
	                Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
	                priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
	        
	                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sEntity & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	
	                Dim priCatNameList As New List(Of String)
	                Dim priCatName As String = ""
	                'Dim priCatWeight As Decimal
	                
	                'Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
	                For Each pricat As MemberInfo In priCatMembers
	              	  priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
'BRApi.ErrorLog.LogMessage(si, "catNameMemScript replaced: " & priCatName & " : " & catNameMemScript.Replace("UUUU",priCat.Member.Name))					  
						If Not String.IsNullOrEmpty(priCatName)		
							priCatNameList.Add(priCatName)
						End If
	                Next
				
					If priCatNameList.Count = 0  Then 
						Return "False"
					End If
				#End Region
				Case Else
					If sProcCtrlVal.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then
				
						Return "False"
					Else
			
						Return "True"
					End If
			End Select		
		End Function
#End Region
#Region "ShowHideMulti"
'Purpose: Any component, cv, object etc., add a case statment 	
		Public Function ShowHideMulti() As String	
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
	'brapi.ErrorLog.LogMessage(si, $"wfProfileName: {wfProfileName}")		
	
			'Grab workflow status (locked or unlocked)
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_PGM_Utilities.GetProcCtrlValMulti(si)
			
			If sProcCtrlVal.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then 
				Return "False"
			Else
				Return "True"
		
			End If	
		End Function
#End Region
'***************************

'**Process Control Label***
#Region "ShowHideLabel"
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ShowHideLabel() As String				
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")		
			
			'Grab workflow status (locked or unlocked)
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_PGM_Utilities.GetProcCtrlVal(si,sEntity)
			
			If sWFStatus.XFEqualsIgnoreCase("locked") Then			
				Return "True"
			Else If sProcCtrlVal.XFContainsIgnoreCase("no") Then 
				Return "True"
			Else	
				Return "False"
			End If
		End Function
#End Region
#Region "ShowHideLabelMulti"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ShowHideLabelMulti() As String				
			'Grab workflow status (locked or unlocked)
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlValMulti As String = CMD_PGM_Utilities.GetProcCtrlValMulti(si)
	'brapi.ErrorLog.LogMessage(si, $"sProcCtrlValMulti {sProcCtrlValMulti}")			
			If sWFStatus.XFEqualsIgnoreCase("locked") Then			
				Return "True"
			Else If sProcCtrlValMulti.XFContainsIgnoreCase("no") Then 
	'brapi.ErrorLog.LogMessage(si, $"inside no condition {sProcCtrlValMulti}")				
				Return "True"
			Else	
				Return "False"
			End If
		End Function
#End Region

#Region "ProcCtrlLblMsg"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ProcCtrlLblMsg() As String	
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)
					
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_PGM_Utilities.GetProcCtrlVal(si,sEntity)
			sEntity = If(sEntity.Contains("_"), sEntity.Substring(0, sEntity.IndexOf("_")), sEntity)
					
	'		If wfProfileName.XFContainsIgnoreCase("Rollover") Then
	'			Return $"Your manager locked the Rollover step for {sEntity}. {vbCrLf}Requirements cannot be rolled over at this time. {vbCrLf}Please contact your manager to allow access."
			
	''			Dim AuditTable As DataSet = brapi.Dashboards.Process.GetAdoDataSetForAdapter(si,False,"da_GetLockAudit","AuditTable",Nothing)	
	''			Dim AuditTable_dt As DataTable = AuditTable.Tables(0)
	''			Dim StatusText As String = AuditTable_dt.Rows(0).Item("StatusText")
	''			Dim LastExecutedStepTime As String = AuditTable_dt.Rows(0).Item("LastExecutedStepTime")
	'''brapi.ErrorLog.LogMessage(si, $"StatusText: {StatusText} | LastExecutedStepTime: {LastExecutedStepTime}")				
	''			If Not StatusText.XFContainsIgnoreCase("completed") Then
	'''brapi.ErrorLog.LogMessage(si, $"StatusText: {StatusText} | LastExecutedStepTime: {LastExecutedStepTime}")				
	''				Return $"StatusText: {StatusText} | LastExecutedStepTime: {LastExecutedStepTime}"
	''			End If
	'		End If
			
			If sWFStatus.XFEqualsIgnoreCase("locked") Then
				Return $"The CMD manager locked all workflow steps for {sCube}. {vbCrLf}No further work can be completed at this time."
			Else If sProcCtrlVal.XFContainsIgnoreCase("no") Then 
				
				Select Case wfProfileName
				Case "Formulate"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be formulated or modified at this time. {vbCrLf}Please contact your manager to allow access."
					
				Case "Prioritize"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be prioritized or modified at this time. {vbCrLf}Please contact your manager to allow access."
					
				Case "Validate"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be validated or modified at this time. {vbCrLf}Please contact your manager to allow access."
						
				Case "Rollover"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be rolled over at this time.. {vbCrLf}Please contact your manager to allow access."
					
				End Select
			End If
		End Function
#End Region 
#Region "ProcCtrlLblMsgMulti"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ProcCtrlLblMsgMulti() As String	
			Dim sEntity = String.Empty
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)
					
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_PGM_Utilities.GetProcCtrlValMulti(si)
			
			Dim userSecGroup As DataSet = brapi.Dashboards.Process.GetAdoDataSetForAdapter(si,False,"da_CMD_PGM_GetUserSec","FundsCenterByWF",Nothing)	
			Dim userSecGroup_dt As DataTable = userSecGroup.Tables(0)
	
	'brapi.ErrorLog.LogMessage(si, "777: " &  userSecGroup_dt.Rows.Count)
					
			If sWFStatus.XFEqualsIgnoreCase("locked") Then
				Return $"The CMD manager locked all workflow steps for {sCube}. {vbCrLf}No further work can be completed at this time."
			
			Else If sProcCtrlVal.XFContainsIgnoreCase("no") Then 
				'Build list of entities a user is assigned to and diaplay them in the label message.
				For Each row As DataRow In userSecGroup_dt.Rows
					Dim sUserSecEntity = row("Value").ToString
					sUserSecEntity = If(sUserSecEntity.Contains("_"), sUserSecEntity.Substring(0, sUserSecEntity.IndexOf("_")), sEntity)		
					If String.IsNullOrEmpty(sEntity) Then
						sEntity = sUserSecEntity 				
					Else
						sEntity = sEntity & ", " & sUserSecEntity
					End If				
				Next		
				
				Select Case wfProfileName
				Case "Formulate"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be formulated or modified at this time. {vbCrLf}Please contact your manager to allow access."
					
				Case "Import"
					Return $"Your manager locked requirement imports for {sEntity}. {vbCrLf}Please contact your manager to allow access."
	
				Case "Validate"
					Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be validated or modified at this time. {vbCrLf}Please contact your manager to allow access."
				End Select
			End If
		End Function
#End Region 
'***************************
#Region "Get File Guidance Content" 
		Public Function GetFileGuidance() As String	
			Dim sFileName As String = args.NameValuePairs.XFGetValue("FileName")
			Dim sFileNameLower As String = sFileName.ToLower			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sFilePath As String = "Documents/Public/CMD_Programming"
			
			'Check if the file was uploaded by the ARMY
			Dim isArmyFile As Boolean = False
			Dim sARMYFolder As String = $"Documents/Public/CMD_Programming/ARMY"
			Dim ARMYFileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sARMYFolder, XFFileType.All, False, False, False)
			For Each file As NameAndAccessLevel In ARMYFileNames
				If sFileName.XFContainsIgnoreCase(file.Name.Substring(file.Name.LastIndexOf("/") + 1)) Then
					isArmyFile = True
				End If
			Next			

			If String.IsNullOrEmpty(sFileName) Then
				Return Nothing					
			Else If isArmyFile Then					
				Return $"{sARMYFolder}/{sFileName}"
				'Return $"Documents/Public/CMD_Programming/ARMY/{sFileName}"
			Else					
				Return $"Documents/Public/CMD_Programming/{sCube}/{sFileName}"	
			End If
		End Function
#End Region

#Region "REQRetrieveCache"
		'Updated EH 8/29/24 RMW-1565 Retrieve title from REQ_Shared
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function REQRetrieveCache() As String
			Try

				Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim sREQToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
'brapi.ErrorLog.LogMessage(si,"sEntityto demote = " & sEntity)
'brapi.ErrorLog.LogMessage(si,"sREQto demote = " & sREQToDelete)
				
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim REQTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleMemberScript).DataCellEx.DataCellAnnotation
				
				Dim cachedREQ As String = sREQToDelete
		
				Return cachedREQ
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try
		End Function
		
#End Region

#Region "GetUserCommandPGMExport"
		'Return user's command for PGM REQ export
		Public Function GetUserCommandPGMExport()
			Try
				Dim sUserName As String = si.UserName
				Dim oUserInfo As UserInfo = BRApi.Security.Authorization.GetUser(si, sUserName)
				Dim oParentGroups As Dictionary(Of GUID, Group) = oUserInfo.ParentGroups
				Dim sCmd As String = ""
				Dim mbrScript As String = ""
				For Each kvp As KeyValuePair(Of GUID, Group) In oParentGroups
'BRApi.ErrorLog.LogMessage(si, kvp.Value.Name)	
				If kvp.Value.Name.XFContainsIgnoreCase("_PGM_") Then
						sCmd = kvp.Value.Name.Split("_")(2)
					End If
					If kvp.Value.Name = "Administrators" Then
						sCmd = "ARMY"
					End If
					If sCmd.XFContainsIgnoreCase("ARMY") Then Exit For
				Next
				
				If Not String.IsNullOrWhiteSpace(sCmd) Then
					If sCmd.XFContainsIgnoreCase("ARMY") Then
						mbrScript = "E#ACOM.Children,E#ARCYBER,,E#HQDA,E#INSCOM,E#USAREUR_AF,E#USARPAC,E#USASMDC,E#USMA"
					Else If sCmd.XFContainsIgnoreCase("USAREUR") Then 
						mbrScript = "E#USAREUR_AF"
					Else
						mbrScript = "E#" & sCmd
					End If
					Return mbrScript
				Else
					Throw New Exception("User not assigned security permissions. Please contact administrator.")
					Return Nothing
				End If
			Catch ex As Exception	
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Get Mass Upload Status"
		Public Function GetMassUploadStatus() As String
			Try
'BRApi.ErrorLog.LogMessage(si,"In XFBR")			
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus","")
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
			End Try
		End Function	
#End Region

#Region "Return Certify Summary Report"
		Public Function Cert_Summary_Report() As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim & "_General"
			Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
			Dim Scenario As String = args.NameValuePairs("Scenario")
			'Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
			Dim Time As String = args.NameValuePairs("Time")
			Dim lTime As List(Of String) = StringHelper.SplitString(Time,",")
			Dim Account As String = "REQ_Funding"
		'brapi.ErrorLog.LogMessage(si,$"APPN: {APPN}")
			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim FilterString As String
		'brapi.ErrorLog.LogMessage(si,$"Account: {Account}")
		'brapi.ErrorLog.LogMessage(si,$"Entity List: " & lEntity.Count)
			If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U2#None:U3#None"
		'brapi.ErrorLog.LogMessage(si,$"Entity Count: " & lEntity.Count)		
			For Each e As String In lEntity
				FilterString = String.Empty
		'brapi.ErrorLog.LogMessage(si,$"Entity Loop: {e} - {FilterString}")
				For Each tm As String In lTime
					'FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					If String.IsNullOrWhiteSpace(FilterString) Then
						FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					Else
						FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
					End If
				Next
		'brapi.ErrorLog.LogMessage(si,"filter=" & FilterString)
						globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")
		
				GetDataBuffer(si,globals,api,args)
		
				If Not globals.GetObject("Results") Is Nothing
		
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
		'brapi.ErrorLog.LogMessage(si,"KLCM results = " & results.Count)
			
				Dim objU2DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U2_MDEP")	
		
				Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
		
				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  e		   
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString
				   
				   Dim lsAncestorListUD2 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU2DimPK, "U2#" &  msb.UD2 & ".Ancestors.Where(MemberDim = U2_PEG)", True,,)
				   msb.UD2 = lsAncestorListUD2(0).Member.Name
				
				   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True,,)
				   msb.UD3 = lsAncestorList(0).Member.Name
		'brapi.ErrorLog.LogMessage(si,"UD3 = " & msb.UD3)		   
				   msb.UD4 = vbNullString
				   msb.UD5 = vbNullString
				   msb.UD6 = vbNullString
				   msb.UD7 = vbNullString
				   msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3}")
					End If
				Next
				End If
			Next
		
			Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
		
			For Each item In sorted
				output &= item.key & ","
			Next
		'brapi.ErrorLog.LogMessage(si,"output:" & output)
			
			If output = "" Then
			output = "U5#One"
			End If
			
			Return output
	
		End Function
#End Region

#Region "Return ORG Totals By SAG"
		Public Function Org_Totals_by_SAG() As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
			Dim Scenario As String = args.NameValuePairs("Scenario")
			'Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
			Dim Time As String = args.NameValuePairs("Time")
			Dim lTime As List(Of String) = StringHelper.SplitString(Time,",")
			Dim Account As String = "REQ_Funding"
		'brapi.ErrorLog.LogMessage(si,$"APPN: {APPN}")
			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim FilterString As String
		'brapi.ErrorLog.LogMessage(si,$"Account: {Account}")
		'brapi.ErrorLog.LogMessage(si,$"Entity List: " & lEntity.Count)
			If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
		'brapi.ErrorLog.LogMessage(si,$"Entity Count: " & lEntity.Count)		
			For Each e As String In lEntity
				FilterString = String.Empty
		'brapi.ErrorLog.LogMessage(si,$"Entity Loop: {e} - {FilterString}")
				For Each tm As String In lTime
					'FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					If String.IsNullOrWhiteSpace(FilterString) Then
						FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					Else
						FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
					End If
				Next
'brapi.ErrorLog.LogMessage(si,"filter=" & FilterString)
						globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")
		
				GetDataBuffer(si,globals,api,args)
		
				If Not globals.GetObject("Results") Is Nothing
		
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
		'brapi.ErrorLog.LogMessage(si,"results = " & results.Count)
			
				Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
		
				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  e		   
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString
				   msb.UD1 = vbNullString
				   msb.UD2 = vbNullString   
				
				   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True,,)
				   msb.UD3 = lsAncestorList(0).Member.Name
		'brapi.ErrorLog.LogMessage(si,"UD3 = " & msb.UD3)		   
				   msb.UD4 = vbNullString
				   msb.UD5 = vbNullString
				   msb.UD6 = vbNullString
				   msb.UD7 = vbNullString
				   msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U3#{msb.UD3}")
					End If
				Next
				End If
			Next
		
			Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
		
			For Each item In sorted
				output &= item.key & ","
			Next
		'brapi.ErrorLog.LogMessage(si,"output:" & output)
			
			If output = "" Then
			output = "U5#One"
			End If
			
			Return output

		End Function
#End Region

#Region "ExportReportMDEPFilterList"
		Public Function ExportReportMDEPFilterList() As String
			Try
				Dim sPEGFilteredList As String = args.NameValuePairs.XFGetValue("PEGFilteredList")
'BRApi.ErrorLog.LogMessage(si,"sPEGFilteredList: " & sPEGFilteredList)				
				If String.IsNullOrWhiteSpace(sPEGFilteredList)
					Return "U2#PEG.Base"
				End If
				

				Dim lPEGFilteredList As List(Of String) = sPEGFilteredList.Split(",").ToList
				Dim sMDEPFilter As String = ""
				
				For Each PEG As String In lPEGFilteredList 
'BRApi.ErrorLog.LogMessage(si,"PEG List Item: " & PEG)
					PEG = PEG.Trim()
					If String.IsNullOrWhiteSpace(sMDEPFilter) Then 
						sMDEPFilter = $"U2#{PEG}.Base"
					Else
						sMDEPFilter = $"{sMDEPFilter},U2#{PEG}.Base"
					End If		
				Next
				'EE, II
'BRApi.ErrorLog.LogMessage(si,"MDEP Filter: " & sMDEPFilter)				
				Return sMDEPFilter
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "RetrievePBPosition"
		Public Function RetrievePBPosition() As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName			
	  		Dim sAllowAccountMbrScript As String = $"Cb#Army:E#Army:C#Local:S#|WFScenario|:T#|WFTime|:V#Annotation:A#Var_Selected_Position:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sPBPosition As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAllowAccountMbrScript).DataCellEx.DataCellAnnotation		
			If String.IsNullOrEmpty(sPBPosition) Then
				Return "A PB Position has not be defined for |WFScenario|. Please contact your manager."
			Else 
				Return sPBPosition
			End IF
		End Function
#End Region

#Region "zShowHide"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
	Public Function zShowHide(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
		
		Dim sEntity = args.NameValuePairs.XFGetValue("entity")
		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'brapi.ErrorLog.LogMessage(si, $"wfProfileName: {wfProfileName}")		
		Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)
		Dim sComponentName As String = args.NameValuePairs.XFGetValue("ComponentName")
		'Manpower Specific
		Dim sREQ_ID As String = args.NameValuePairs.XFGetValue("REQ_ID")
	
		
#Region "Workflow complete and revert buttons"		
		Select Case sComponentName				
		Case "WorkflowAccess"
			If wfProfileFullName.XFContainsIgnoreCase("Manage CMD Requirements") Then 
				Return "True"
			Else
				Return "False"
			End If
		End Select	
#End Region		

'brapi.ErrorLog.LogMessage(si, $"Manual: {sCube} Sub CMD Review (CMD PGM)")
'brapi.ErrorLog.LogMessage(si, $"outside")
		'Process control & workflow lock/unlocked verification
		Dim WF_ProcCtrl_Account As New Dictionary(Of String, String) From {
			{"Formulate","CMD_PGM_Allow_Req_Creation"},
			{"Import","CMD_PGM_Allow_Req_Creation"},
			{"Validate","CMD_PGM_Allow_Req_Validation"},
			{"Prioritize","CMD_PGM_Allow_Req_Priority"},
			{"Rollover","CMD_PGM_Allow_Req_Rollover"}
			}	
		If WF_ProcCtrl_Account.ContainsKey(wfProfileName) Then
'brapi.ErrorLog.Logmessage(si, $"wfPrfoileName: {wfProfileName}")

			'Grab workflow status (locked or unlocked)
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)

			'If on the Import workflow step - need retrieve the user's security permissions to grab the entity to pass through the member script
			Dim dsArgs As New DashboardDataSetArgs
			dsArgs.FunctionType = DashboardDataSetFunctionType.GetDataSet
			dsargs.DataSetName = "GetUserFundsCenterByWF"
			Dim userSecurity_dt = GBL_DataSet.Main(si, globals, api, dsArgs)
		
			If wfProfileName.XFContainsIgnoreCase("import")
			#Region "Import workflow step condition"				
				For Each row As DataRow In userSecurity_dt.Rows
					Dim entity = row("Value").ToString		
			    	Dim sAllowAccount As String = WF_ProcCtrl_Account(wfProfileName)
			  		Dim sAllowAccountMbrScript As String = $"Cb#{sCube}:E#{entity}:C#Local:S#RMW_Cycle_Config_Annual:T#{sREQTime}:V#Annotation:A#{sAllowAccount}:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sAllowAccountValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAllowAccountMbrScript).DataCellEx.DataCellAnnotation			
					
					If sAllowAccountValue.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then 
						Return "False"
					End If
				Next 
			#End Region
			Else
				'Grab workflow process control status (blank, yes or no)
		    	Dim sAllowAccount As String = WF_ProcCtrl_Account(wfProfileName)
		  		Dim sAllowAccountMbrScript As String = $"Cb#{sCube}:E#{sEntity}:C#Local:S#RMW_Cycle_Config_Annual:T#{sREQTime}:V#Annotation:A#{sAllowAccount}:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sAllowAccountValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAllowAccountMbrScript).DataCellEx.DataCellAnnotation			
'brapi.ErrorLog.Logmessage(si, $"sAllowAccount: {sAllowAccount} | sAllowAccountValue: {sAllowAccountValue}")	
				If sAllowAccountValue.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then			
					Return "False"
				Else 		
					Select Case sComponentName							
						#Region "Manpower approve and revert buttons"
						Case "ManpowerApprove"
							Dim sql As String = String.empty
							SQL = $"Select distinct Flow
								FROM XFC_CMD_PGM_REQ_Details
								where Entity = '{sEntity}'
								and WFScenario_Name = '{sScenario}'
								and Unit_of_Measure = 'CIV_COST'"
							
							Dim dtAll As New DataTable
							Dim sStatus As String = ""
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
							End Using
							For Each row As DataRow In dtAll.rows
								sStatus = row.Item(0)
							Next
							If sStatus.XFContainsIgnoreCase("Formulate") Then 
								Return "True"
							Else 
								Return "False"
							End If 
						
						Case "ManpowerRevert"
							Dim sql As String = String.empty
							SQL = $"Select distinct Flow
								FROM XFC_CMD_PGM_REQ_Details
								where Entity = '{sEntity}'
								and WFScenario_Name = '{sScenario}'
								and Unit_of_Measure = 'CIV_COST'"
													
							Dim dtAll As New DataTable
							Dim sStatus As String = ""
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
							End Using
							For Each row As DataRow In dtAll.rows
								sStatus = row.Item(0)
							Next
							If sStatus.XFContainsIgnoreCase("Final") Then 
								Return "True"
							Else 
								Return "False"
							End If 						
						#End Region
						Case Else
							Return "True"
					End Select		
				End If
			End If
		'Used for the review step	
		Else If String.IsNullOrEmpty(wfProfileName) 
			Return "False"
		End If	
	End Function
#End Region 'Code is referenced for bits and parts

#Region "Get CMD_PGM_REQ_ID From title"
Public Function GetREQID() As Object	
			Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			Dim sREQ_ID_Val As String = String.Empty
			Dim REQ_ID_Val_guid As Guid = Guid.Empty
		'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ_CMT) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)		
			Dim ReqID As String = args.NameValuePairs.XFGetValue("REQ")
			
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
			sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa,dt,sql, sqlparams)
			
			
			
		If dt.Rows.Count > 0 Then
    		'sREQ_ID_Val = Convert.ToString(dt.Rows(0)("CMD_PGM_REQ_ID"))
    		REQ_ID_Val_guid = dt.Rows(0)("CMD_PGM_REQ_ID")
		Else
    
   	 		Return Nothing 
	End If	
	
End If 
		                End Using
		            End Using
'Brapi.ErrorLog.LogMessage(si, "REQ ID" & REQ_ID_Val_guid.ToString())
            Return REQ_ID_Val_guid
		End Function
		
#End Region

		
#Region "Get demote reqs"
Public Function GetDemotereqs() As String

    Try
      
        Dim REQIDs As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName, "REQPrompts", "REQ", ",")
        Dim cbxentity As String = args.NameValuePairs.XFGetValue("cbxentity")
        Dim Req_ID_List As List(Of String) = StringHelper.SplitString(REQIDs, ",").Distinct().ToList()
       
        Dim filteredReqList As New List (Of String)
		
        For Each req As String In Req_ID_List
            Dim sreq As String = String.Empty
			If req.Contains("xx") Then
                If req.Length >= 3 Then
                    sreq = req.Substring(0, 3)
                Else
                    sreq = req 
                End If
            Else
                If req.Length >= 5 Then
                    sreq = req.Substring(0, 5)
                Else
                    sreq = req 
                End If
            End If
   
            If String.Equals(sreq, cbxentity, StringComparison.OrdinalIgnoreCase) Then
                filteredReqList.Add(req)
            End If

        Next
		'Brapi.ErrorLog.LogMessage(si,"Demote REQS" & String.Join(",",filteredReqList))
        Return String.Join(",",filteredReqList)

    Catch ex As Exception
        
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try

End Function
#End Region

#Region "Display PEG Dashboard button"
		Public Function displaypegdashboard() As String
			Dim reqTitle = args.NameValuePairs.XFGetValue("REQTitle")
			Dim sPEG = args.NameValuePairs.XFGetValue("Peg")

			Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
		Dim MDEP As String = String.Empty
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ_CMT) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)		
			
			
				If String.IsNullOrWhiteSpace(reqTitle)
					Return Nothing
				Else 
				'Fill the DataTable 
				Dim sql As String = $"SELECT MDEP 
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
		New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = reqTitle}
   		}
			sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa,dt,sql, sqlparams)
			
		
			
		If dt.Rows.Count > 0 Then
			
    		MDEP = dt.Rows(0)("MDEP")
		End If
	End If 
	End Using
End Using 
	
		Dim iU2MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD2.Id, MDEP)
		Dim sParentPEG As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), iU2MbrID, False)(0).Name

		
				
			Select Case True
				Case (sParentPEG = "DD"	And sPEG = "DD")	
					Return True
				Case (sParentPEG = "SS"	And sPEG = "SS")		
					Return True
				Case Else	
					Return False
			End Select
		End Function
#End Region

#Region "Display Base Info Dashboards"
'Used to display cWork OOC, Peg detilas and general comments
		Public Function displaybaseinfodashboard() As String
			Dim sDashName As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"PGM_DB_Cache","Db","")
			Select Case sDashName
				Case "cWork"		
					Return "CMD_PGM_0b2a_Body_cWork"
				Case "SS PEG"		
					'Brapi.ErrorLog.LogMessage(si, "Here")
					Return "CMD_PGM_0b2b_Body_SSPEG"
				Case "DD PEG"		
					Return "CMD_PGM_0b2bc_Body_DDPEG"
				Case "Gen Cmt"		
					Return "CMD_PGM_0b2c_Body_GeneralCmt"
				Case Else	
					Return Nothing
			End Select
		End Function
#End Region

#Region "Display dynamic flow"
'Used to display dynamic flow in cubeviews
		Public Function displaydynflow() As String
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
                Dim x As Integer = InStr(wfProfileName, ".")
                Dim sProfileName As String = wfProfileName.Substring(x + 0)

			Select Case True
				Case sProfileName.XFContainsIgnoreCase("Formulate")
					Return "F#Dyn_CMD_PGM_Formulate"
				Case sProfileName.XFContainsIgnoreCase("Validate")	
					Return  "F#Dyn_CMD_PGM_Validate"
				Case sProfileName.XFContainsIgnoreCase("Prioritize")		
					Return  "F#Dyn_CMD_PGM_Prioritize"
				Case sProfileName.XFContainsIgnoreCase("Approve")	
					Return  "F#Dyn_CMD_PGM_Approve"
				Case Else	
					Return Nothing
			End Select
		End Function
#End Region

	End Class
End Namespace