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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_SPLN_String_Helper
	Public Class MainClass
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As Object
		Public args As DashboardStringFunctionArgs
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				'brapi.ErrorLog.LogMessage(si,"inside string helper")
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args

				Select Case args.FunctionName.ToLower()
					Case "displayapprovereqbtn"
						Return Me.DisplayApproveREQBtn(si, globals, api, args)
					Case "getsortedappnlist"
						Return Me.GetSortedAppnList()
					Case "displaydashboard"
						Return Me.displaydashboard()
					Case "DisplayRollFwdREQBtn"
					Case "CreateLblText"
					Case "GetFileGuidance"
					Case "dynflowmember"
						Return Me.DynFlowMember(si, globals, api, args)
					Case "dynaccountvalidations"
						Return Me.DynAccountValidations(si, globals, api, args)
					Case "packagesubmit"
						Return Me.PackageSubmit(si, globals, api, args)
					Case "getfcappnforsubmission"
						Return Me.GetFCAPPNforSubmission(si, globals, api, args)
					Case "getpaynonpaymembers"
						Return Me.GetPayNonPayMembers(si, globals, api, args)
				End Select
				

#Region "DisplayRollFwdREQBtn"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayRollFwdREQBtn") Then		
					Return Me.DisplayRollFwdREQBtn(si, globals, api, args)
				End If				
#End Region	'Updated 09/25/2025		

#Region "RollFwdLblText"
				If args.FunctionName.XFEqualsIgnoreCase("CreateLblText") Then		
					Return Me.CreateLblText(si, globals, api, args)
				End If				
#End Region	'Updated 09/25/2025		

#Region "ShowHide"
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide") Then		
					Return Me.ShowHide()
				End If				
#End Region	'Updated 09/25/2025	

#Region "ShowHideLabel"
				If args.FunctionName.XFEqualsIgnoreCase("ShowHideLabel") Then		
					Return Me.ShowHideLabel(si, globals, api, args)
				End If				
#End Region	'Updated 10/22/2025		

#Region "ProcCtrlLblMsg"
				If args.FunctionName.XFEqualsIgnoreCase("ProcCtrlLblMsg") Then			
					Return Me.ProcCtrlLblMsg(si, globals, api, args)
				End If				
#End Region	'Updated 10/22/2025	

#Region "Get File For CMD PGM Guidance"
	'Get File For CMD PGM Guidance
				If args.FunctionName.XFEqualsIgnoreCase("GetFileGuidance") Then				
					Return Me.GetFileGuidance(si,globals,api,args)
				End If	
#End Region 'Updated 09/25/2025

'=====================================================================================
				
#Region "Get Workflow Status"
				If args.FunctionName.XFEqualsIgnoreCase("GetWorkflowStatus") Then
					'Get current status of UFR
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
					Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
					Dim UFRTime As String = WFYear & "M12"
					
					Dim reqEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
					Dim reqFlow As String = args.NameValuePairs.XFGetValue("REQFlow")
'brapi.ErrorLog.LogMessage(si, "E#, F# " & ufrEntity & "," & ufrFlow)					
					Dim UFRMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & reqFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'brapi.ErrorLog.LogMessage(si, UFRMemberScript)
					Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRMemberScript).DataCellEx.DataCellAnnotation
'brapi.ErrorLog.LogMessage(si, currentStatus)									
					Return currentStatus
				End If
#End Region

#Region "Get Funding Status"
				If args.FunctionName.XFEqualsIgnoreCase("GetFundingStatus") Then
					'Get current status of UFR
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
					Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
					Dim UFRTime As String = WFYear & "M12"
					
					Dim reqEntity As String = args.NameValuePairs.XFGetValue("reqEntity")
					Dim reqFlow As String = args.NameValuePairs.XFGetValue("reqFlow")
					
					
					Dim UFRMemberScript As String = "Cb#" & wfCube & ":E#" & reqEntity & ":C#Local:S#" & wfScenario & ":T#" & UFRTime & ":V#Annotation:A#REQ_Funding_Status:F#" & reqFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

					Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, UFRMemberScript).DataCellEx.DataCellAnnotation
									
					Return currentStatus
				End If
#End Region

#Region "Category Member List"
				If args.FunctionName.XFEqualsIgnoreCase("CategoryMemberList") Then
					Dim sREQEntity As String = args.NameValuePairs.XFGetValue("sREQEntity")   'Passed in Parameter
					Return Me.CategoryMemberList(si, globals, api, args, sREQEntity)
				End If				
#End Region
	
#Region "Get Stakeholders Email"
				If args.FunctionName.XFEqualsIgnoreCase("GetStakeholdersEmail") Then
					Return Me.GetStakeholdersEmail(si, globals, api, args)

				End If
#End Region

#Region "REQDashboardsToRedraw"
				If args.FunctionName.XFEqualsIgnoreCase("REQDashboardsToRedraw") Then	
					Return Me.REQDashboardsToRedraw(si, globals, api, args)

				End If	
#End Region

#Region "REQRetrieveCache"
				If args.FunctionName.XFEqualsIgnoreCase("REQRetrieveCache") Then	
					Return Me.REQRetrieveCache(si, globals, api, args)

				End If	
#End Region

#Region "Get REQ FC"
				If args.FunctionName.XFEqualsIgnoreCase("getREQFC") Then 
					Dim sFC As String = args.NameValuePairs.XFGetValue("FC")	
					Dim sMode As String = args.NameValuePairs.XFGetValue("mode")
					Dim sExpansion As String = args.NameValuePairs.XFGetValue("expansion","")

					Select Case sMode
						Case "NonGeneral"
							Return sFC.Replace("_General","")
						Case "IncludeExpansion"
							If sFC.XFContainsIgnoreCase("_General") Then
								Return sFC.Replace("_General","") & sExpansion
							Else
								Return sFC
							End If
					End Select
				End If
#End Region

#Region "Parse UFR Title"
				If args.FunctionName.XFEqualsIgnoreCase("TitleParseEntity") Then
					
					Dim sEntity As String = Me.ParseREQEntity(si, globals, api, args)					
					Return sEntity
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("TitleParseFlow") Then
					Dim sFlow As String = Me.ParseREQFlow(si, globals, api, args)	
					Return sFlow
				End If
#End Region

#Region "Show and Hide Return Comment"
				If args.FunctionName.XFEqualsIgnoreCase("ShowHideReturnComment") Then	
					Dim sFlow As String = args.NameValuePairs.XFGetValue("UFRFlow")
					Dim sEntity As String = args.NameValuePairs.XFGetValue("UFREntity")
					Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
					Dim sScenario As String = args.NameValuePairs.XFGetValue("UFRScenario")
					Dim sTime As String = args.NameValuePairs.XFGetValue("UFRTime")	
					Return Me.ShowHideReturnComment(si, globals, api, args, sFlow, sEntity, sCube, sScenario, sTime)

				End If				
#End Region

#Region "Display Alt Rows"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayAltRows") Then		
					Return Me.DisplayAltRows(si, globals, api, args)
				End If				
#End Region

#Region "Display CMD Rows"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayCmdRows") Then
					Return Me.DisplayCmdRows(si, globals, api, args)
				End If
#End Region

#Region "Display Lock Unlock WF Components CMD/FC"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayWFComponentsCMDPGM") Then
					Return Me.DisplayWFComponentsCMDPGM(si, globals, api, args)
				End If
#End Region

#Region "Display Rate Info"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayRateInfo") Then	
					Return Me.DisplayRateInfo(si, globals, api, args)
				End If
#End Region

#Region "YOYLimitColorCode"
				If args.FunctionName.XFEqualsIgnoreCase("YOYLimitColorCode") Then	
					Return Me.YOYLimitColorCode(si, globals, api, args)
				End If
#End Region

#Region "Display Upload Guidance Btn"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayUploadGuidanceBtn") Then
					Return Me.DisplayUploadGuidanceBtn(si, globals, api, args)
				End If
#End Region

#Region "Dynamic U8"
	'======================================================================================================================================================================
	' Return U8# according to passed in Cube View name and wf profile name
	'======================================================================================================================================================================
	' Usage: 
	'	- CV: REQ_Funding_Month_Amt
	'	- Monthly Or Year Requested Amount, Row Overrides
	'	- In this instance, the method is used to return U8#Top to make the funding amount cells read-only for the Review Requirements workflow instead of having to create a separate CV
	'======================================================================================================================================================================
				If args.FunctionName.XFEqualsIgnoreCase("DynamicU8") Then		
					Return Me.DynamicU8(si, globals, api, args)
				End If				
#End Region

#Region "Get FC for Base Info CBX"
	'get first 3 FC characters for base info CV comboboxes
				If args.FunctionName.XFEqualsIgnoreCase("GetFC") Then						
					Return Me.GetFC(si,globals,api,args)
				End If	
#End Region

#Region "Get Linked Report Read Only / Visibility"
	'Return U8#ReadOnlyAnnotation or Visibility for Linked Report 
				If args.FunctionName.XFEqualsIgnoreCase("GetLRVisibility") Then				
					Return Me.GetLRVisibility(si,globals,api,args)
				End If	
				
#End Region

#Region  "Get Current Content Cbx by WF"
	'Return the content cbx prompt for the current WF step 
				If args.FunctionName.XFEqualsIgnoreCase("GetContentCbxByWF") Then				
					Return Me.GetContentCbxByWF(si,globals,api,args)
				End If	
				
#End Region

#Region "REQControlsEntityFilter"
				If args.FunctionName.XFEqualsIgnoreCase("REQControlsEntityFilter") Then	
					Return Me.REQControlsEntityFilter(si, globals, api, args)

				End If	
#End Region

#Region "REQReportSubConsolidateTime"
'Return 5 years Time for subconsolidate data management sequence for REQ Report using Selected Scenario
				If args.FunctionName.XFEqualsIgnoreCase("REQReportSubConsolidateTime") Then	
					Return Me.REQReportSubConsolidateTime(si, globals, api, args)

				End If	
#End Region

#Region "GetUserCommandPGMExport"
'Return User's Command for PGM REQ Export
				If args.FunctionName.XFEqualsIgnoreCase("GetUserCommandPGMExport") Then	
					Return Me.GetUserCommandPGMExport(si, globals, api, args)

				End If	
#End Region

#Region "MDEPStripTime"
				If args.FunctionName.XFEqualsIgnoreCase("MDEPStripTime") Then	
					Return Me.MDEPStripTime(si, globals, api, args)

				End If	
#End Region

#Region "MemberFilterTotal"
				If args.FunctionName.XFEqualsIgnoreCase("MemberFilterTotal") Then	
					Return Me.MemberFilterTotal(si, globals, api, args)

				End If	
#End Region

#Region "ExportReportMDEPFilterList"
				If args.FunctionName.XFEqualsIgnoreCase("ExportReportMDEPFilterList") Then				
					Return Me.ExportReportMDEPFilterList(si,globals,api,args)
				End If				
#End Region

#Region "Get Mass Upload Status"
				If args.FunctionName.XFEqualsIgnoreCase("GetMassUploadStatus") Then				
					Return Me.GetMassUploadStatus(si,globals,api,args)
				End If				
#End Region

#Region "Get Demote Button Visibility"
	'Return IsVisible = True/False for Demote button 
				If args.FunctionName.XFEqualsIgnoreCase("GetDemoteBtnVisibility") Then				
					Return Me.GetDemoteBtnVisibility(si,globals,api,args)
				End If	
				
#End Region

#Region "Adjust Funding Line comboboxes Visibility"
	'Return IsVisible = True/False for adjust Funding Line combo boxes 
				If args.FunctionName.XFEqualsIgnoreCase("AdjustFundingLine") Then				
					Return Me.AdjustFundingLine(si,globals,api,args)
				End If	
				
#End Region


				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Constants"
		Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
		Public GBL_DataSet As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardDataSet.GBL_DB_DataSet.MainClass()
		#End Region	'Updated 09/24/202525

		#Region "DisplayDashboard"
		Public Function displaydashboard() As String
			Dim reqTitle = args.NameValuePairs.XFGetValue("REQTitle")
			If reqTitle <> String.Empty Or Not String.IsNullOrEmpty(reqTitle) Then
				Return "CMD_SPLN_0_Body_REQDetailsMain"
			Else
				Return "CMD_SPLN_0_Body_REQDetailsHide"
			End If
		End Function
		#End Region

		#Region "ShowHide"
		'Purpose: Any component, cv, object etc., add a case statment
		Public Function ShowHide() As String
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")
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
		Dim sProcCtrlVal As String = CMD_SPLN_Utilities.GetProcCtrlVal(si,sEntity)
		
		If sProcCtrlVal.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then 
			Return "False"
		Else
			Select Case sComponentName							
				#Region "Manpower approve and revert buttons"
				Case "ManpowerApprove"
					Dim sql As String = String.empty
					SQL = $"Select distinct Flow
						FROM XFC_CMD_SPLN_REQ_Details
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
						FROM XFC_CMD_SPLN_REQ_Details
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
		End Function
		#End Region 'Updated 10/07/2025

		#Region "ShowHideLabel"
		'Purpose: Any component, cv, object etc., add a case statment
		'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")
		Public Function ShowHideLabel(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")		'Grab workflow status (locked or unlocked)
		Dim deArgs As New DashboardExtenderArgs
		deArgs.FunctionName = "Check_WF_Complete_Lock"
		Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
		
		'Grab process control value for an entity + workflow profile
		Dim sProcCtrlVal As String = CMD_SPLN_Utilities.GetProcCtrlVal(si,sEntity)
		
		If sWFStatus.XFEqualsIgnoreCase("locked") Then			
			Return "True"
		Else If sProcCtrlVal.XFContainsIgnoreCase("no") Then 
			Return "True"
		Else	
			Return "False"
		End If
	End Function
#End Region 

#Region "ProcCtrlLblMsg"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
	Public Function ProcCtrlLblMsg(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
		Dim sEntity = args.NameValuePairs.XFGetValue("entity")
		Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)
				
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim deArgs As New DashboardExtenderArgs
			deArgs.FunctionName = "Check_WF_Complete_Lock"
			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			'Grab process control value for an entity + workflow profile
			Dim sProcCtrlVal As String = CMD_SPLN_Utilities.GetProcCtrlVal(si,sEntity)
			sEntity = If(sEntity.Contains("_"), sEntity.Substring(0, sEntity.IndexOf("_")), sEntity)
			
			If sWFStatus.XFEqualsIgnoreCase("locked") Then
				Return $"The CMD manager locked all workflow steps for {sCube}. {vbCrLf}No further work can be completed at this time."
			ElseIf sProcCtrlVal.XFContainsIgnoreCase("no") Then
				Select Case wfProfileName
					Case "Formulate"
						Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be formulated or modified at this time. {vbCrLf}Please contact your manager to allow access."
					'Case "Import"
					'	Return $"Your manager locked requirement uploads for {sEntity}. {vbCrLf}Please contact your manager to allow access."
					Case "Prioritize"
						Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be prioritized or modified at this time. {vbCrLf}Please contact your manager to allow access."
					Case "Validate"
						Return $"Your manager locked the {wfProfileName} step for {sEntity}. {vbCrLf}Requirements cannot be validated or modified at this time. {vbCrLf}Please contact your manager to allow access."
					Case "Rollover"
						Return $""
				End Select
			End If
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
				{"Formulate","CMD_SPLN_Allow_Req_Creation"},
				{"Import","CMD_SPLN_Allow_Req_Creation"},
				{"Validate","CMD_SPLN_Allow_Req_Validation"},
				{"Prioritize","CMD_SPLN_Allow_Req_Priority"},
				{"Rollover","CMD_SPLN_Allow_Req_Rollover"}
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
						Dim Entity = row("Value").ToString		
						Dim sAllowAccount As String = WF_ProcCtrl_Account(wfProfileName)
						Dim sAllowAccountMbrScript As String = $"Cb#{sCube}:E#{sEntity}:C#Local:S#RMW_Cycle_Config_Annual:T#{sREQTime}:V#Annotation:A#{sAllowAccount}:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
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
									FROM XFC_CMD_SPLN_REQ_Details
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
									FROM XFC_CMD_SPLN_REQ_Details
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
			ElseIf String.IsNullOrEmpty(wfProfileName) Then
				Return "False"
			End If	
		End Function
		#End Region 'Updated 10/07/2025
		
		#Region "Get File For CMD PGM Guidance" 
		Public Function GetFileGuidance(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String	
			Dim sFileName As String = args.NameValuePairs.XFGetValue("FileName")	
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sFileNameLower As String = sFileName.ToLower
			Dim sFilePath As String = "Documents/Public/CMD_Programming"
			
			'returns files under its CMD
			If sFileNameLower.StartsWith(sCube.ToLower) Then		
				sFilePath = $"{sFilePath}/{sCube}/{sFileName}"
				Return sFilePath
			'returns files under its CMD	
			ElseIf sFileNameLower.StartsWith("ARMY".ToLower) Then				
				sFilePath = $"{sFilePath}/ARMY/{sFileName}"
				Return sFilePath
			'returns files if cbx is empty
			ElseIf sFileNameLower Is Nothing Then
				Return Nothing	
			End If
		End Function
		#End Region 'Updated 09/25/2025
		
		#Region "Mbr Lists"
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
		
#End Region 'Update 10/16/2025

#Region "DisplayApproveREQBtn"
	Public Function DisplayApproveREQBtn(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim CMDLevel As String = args.NameValuePairs.XFGetValue("Level")
		If wfProfileName.XFContainsIgnoreCase("Approve CMD Requirements") And CMDLevel.XFContainsIgnoreCase("CMDFinalize") Then 
			Return "True"
		Else If wfProfileName.XFContainsIgnoreCase("Approve Requirements") And CMDLevel.XFContainsIgnoreCase("SubCMDApprove") Then 
			Return "True"
		Else
			Return "False"
		End If 
		Return Nothing
		End Function
		#End Region 'Updated 09/24/2025
		
		#Region "CreateLblText"
		Public Function CreateLblText(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
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
					ElseIf sLabelType.XFContainsIgnoreCase("info")
						'this should be a gv where they can select or unselect requirement they want to roll foward
						Return  $"All requirements for the ENTIRE command will be rolled over except for the ones marked as ""No"" - not just the displayed requirements from the left screen.{vbCrLf}"
					Else
						Return Nothing
					End If
				Case "DemoteREQ"	
					Return $"Are you sure you wish to DELETE: {sREQTitle}"
			End Select	
		End Function
		#End Region 'Updated 10/2/2025#Region "ExportReportMDEPFilterList"
		Public Function ExportReportMDEPFilterList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
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

'====================i'M SOMEWHAT CONFIDENT EVERYTHING BELOW CAN BE DELETE - FRONZ===============================================================================

#Region "DisplayRollFwdREQBtn"
	Public Function DisplayRollFwdREQBtn(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim dargs As New DashboardExtenderArgs
		dargs.FunctionName = "Check_WF_Complete_Lock"
		Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)
		If sWFStatus.XFContainsIgnoreCase("unlocked")  Then 
			Return "True"
		Else
			Return "False"
		End If 
		Return Nothing
	End Function
#End Region 'Updated 09/25/2025 ' DELETE

#Region "Get Reviewer Email"
		Public Function GetStakeholdersEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim emails As String = ""
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim reqEntity As String = args.NameValuePairs.XFGetValue("reqEntity")
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				
				'logic to account for _General at the parent lvl
				If reqEntity.XFContainsIgnoreCase("General") Then
					reqEntity = reqEntity.Replace("_General","")
				End If 
				Dim stakeholderSecGroupName As String = ""
				
				stakeholderSecGroupName = "g_UFR_" & wfCube & "_ST_" '& wfLevel
'BRApi.ErrorLog.LogMessage(si, "stakeholderSecGroupName: " & stakeholderSecGroupName)				
				'Get all members of the sec group
				Dim stakeholderSecGroup As GroupInfo = BRApi.Security.Admin.GetGroup(si, stakeholderSecGroupName)
'BRApi.ErrorLog.LogMessage(si, "stakeholderSecGroup: " & GroupInfo)				
				If (stakeholderSecGroup Is Nothing) Then
'BRApi.ErrorLog.LogMessage(si, "Security group " & stakeholderSecGroupName & "was not found.")
					Return Nothing
				End If
				
				Dim users As List(Of Principal) = stakeholderSecGroup.ChildPrincipals
				
				For Each user As Principal In users
					If (user.PrincipalType = PrincipalType.User) Then
						Dim objUserInfo As UserInfo = BRApi.Security.Authorization.GetUser(si, user.Name)
						Dim text2 = objUserInfo.User.Text2		
						Dim sFundCenters As String = ""
						If ((Not String.IsNullOrWhiteSpace(text2)) And text2.XFContainsIgnoreCase("PGMFundCenters") ) Then 
							text2 = text2.Replace(" ", "")
							Dim subStart As Integer = text2.IndexOf("PGMFundCenters=[") + 13
							Dim subEnd As Integer = text2.IndexOf("]",text2.IndexOf("PGMFundCenters=[")) - subStart
							sFundCenters = text2.Substring(subStart, subEnd)
						End If
						'filter the people who have the FC in their text 2
						If (sFundCenters.XFContainsIgnoreCase(reqEntity)) Then
							emails = emails & objUserInfo.User.Email.Trim & ","
'BRApi.ErrorLog.LogMessage(si, "email: " & emails)							
						End If
'Brapi.ErrorLog.LogMessage(si, user.)
					End If
				Next				
				Return emails
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region ' DELETE?

#Region "Get Account Translated Value Function"
		Public Function GetAccountTranslatedValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs, _
									              ByVal sAccount As String, ByVal sTranslationCd As String, ByVal sFlow As String, ByVal sEntity As String) As String
			Try
				'Purpose: Get the Annotation Value of an Account for a given UFR and either return that value or return a translated value depding on sTranslationCd.
				'Usage Ex 1 Return the Annotation value - XFBR(UFR_String_Helper,GetAccountTranslatedValue,UFREntity=|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_FundCenter__Shared!|, UFRFlow=|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|,UFRAccount=REQ_Notification_Email_List,TranslationCd=)
				'Usage Ex 2 Return M12   or .Base - XFBR(UFR_String_Helper,GetAccountTranslatedValue,UFREntity=|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_FundCenter__Shared!|, UFRFlow=|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|,UFRAccount=REQ_Recurring_Cost_Ind,TranslationCd=AllMnthsOrSep)
				'Usage Ex 3 Return :None or :U8#ReadOnlyAnnotation - XFBR(UFR_String_Helper,GetAccountTranslatedValue,UFREntity=|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_FundCenter__Shared!|, UFRFlow=|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|,UFRAccount=REQ_Request_Manpower_Ind,TranslationCd=WriteOrReadText)
				
				Dim sCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim sTime As String = sYear & "M12"
				Dim sReturnValue As String = ""

				'Get Annotation Value of Account member   
				Dim sMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#" & sAccount &":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sAccountValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sMemberScript).DataCellEx.DataCellAnnotation
'BRApi.ErrorLog.LogMessage(si, "sTranslationCd: --" & sTranslationCd & "--sMemberScript: --" & sMemberScript & "--" & "sAccountValue: --" & sAccountValue & "--")	
				'Determine whether to do a transformation or just return the 
				If(Not String.IsNullOrWhiteSpace(sTranslationCd)) Then 
					'Perform a Translation of the REQ_Recurring_Cost_Ind Annotation Value. 'Valid Values: WriteOrReadText, WriteOrReadNumber, AllMnthsOrSep, <More will be added in future.
					If (sAccount = "REQ_Recurring_Cost_Ind" And sTranslationCd = "AllMnthsOrSep") Then	
						'Translate the No/Yes value of REQ_Recurring_Cost_Ind: If Yes, return ".Base"  Otherwise, return "M12"
						sReturnValue = "M12" 'Default to single month M12
						If sAccountValue = "Yes"
							sReturnValue = ".Base" 
						End If
					End If
					If (sTranslationCd = "WriteOrReadText" Or sTranslationCd = "WriteOrReadNumber") Then
						'Get REQ_Workflow_Status and the Workflow type (Create, Review...), Get User's Group (Stakeholder, Analyst, Manger)
						'RH 9/1/2023 Add logic below
						'If REQ_Workflow_Status <> “Working” AND the signed on User does NOT belong to a Manager (g_UFR_*_MG_*) security group, sReturnValue = ":U8#ReadOnlyAnnotation" , Else sReturnValue = ":U8#None" 
'						If sAccountValue = "Yes"
'							'Enable Write Access to Account Member
'							sReturnValue = "U8#None" 
'						End If

						'Set Default to be "Read Only". Only the UFR Manager can edit the Cube View if Account REQ_Workflow_Status = "Working"
						'RH 9/1/2023 Test using hardcoded: sReturnValue = "U8#ReadOnlyAnnotation" 
						If (sTranslationCd = "WriteOrReadText") Then
							sReturnValue = "U8#ReadOnlyAnnotation" 
						Else If (sTranslationCd = "WriteOrReadNumber") Then
							sReturnValue = "O#Top" 
						Else
							sReturnValue = "U8#None" 
						End If
					End If
				Else
					'sTranslationCd is blank so return the Account Annotation Value
					sReturnValue = sAccountValue
				End If

'BRApi.ErrorLog.LogMessage(si, "sAccount: --" & sAccount & "--" & "sReturnValue: --" & sReturnValue & "--"   )
				Return sReturnValue
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		
#End Region ' DELETE?
		
#Region "REQDashboardsToRedraw"
		Public Function REQDashboardsToRedraw(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sLRDashboard As String = "REQPRO_REQLDB_0__ReviewREQDetails"
				'Dim ufr As String = args.NameValuePairs.XFGetValue("UFR_Id")
				If sProfileName = "Review Financials" Or sProfileName = "Validate Requirements" Or sProfileName = "Validate Requirements CMD" Then 
					Return "REQPRO_REQVAL_0__Main" & "," & sLRDashboard
				Else If sProfileName = "Prioritize Requirements" Or sProfileName = "Prioritize Requirements CMD" Then 
					Return "REQPRO_REQPRT_0__Main" & "," & sLRDashboard
				Else If sProfileName = "Formulate Requirement" Then 
					Return "REQPRO_REQCRT_0__Main" & "," & sLRDashboard
				Else If wfProfileName.XFContainsIgnoreCase("Manage") Then 
					Return "REQPRO_REQMGT_0__Main" & "," & sLRDashboard
				Else If sProfileName = "Review Requirements" Then 
					Return "REQPRO_REQREV_0__Main" & "," & sLRDashboard
				End If 
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try
			Return Nothing
		End Function
		
#End Region ' DELETE

#Region "REQRetrieveCache"
		'Updated EH 8/29/24 RMW-1565 Retrieve title from REQ_Shared
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function REQRetrieveCache(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim sREQToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
'brapi.ErrorLog.LogMessage(si,"sEntityto demote = " & sEntity)
'brapi.ErrorLog.LogMessage(si,"sREQto demote = " & sREQToDelete)
				Dim sREQ As String = sREQToDelete.Split(" ")(1)
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim REQTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sREQToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleMemberScript).DataCellEx.DataCellAnnotation
				
				Dim cachedREQ As String = sREQ
		
				Return cachedREQ
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region ' DELETE?

#Region "Category Member List Function"
		Public Function CategoryMemberList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs, ByVal sREQEntity As String) As String
			Try
				'Usage: 'XFBR(UFR_String_Helper,CategoryMemberList,sUFREntity=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_FundCenter__Shared!|])
				'Purpose: Generate cube view column member list containing UFR Category Names for U5#UFR_Priority_Cat_01 to  U5#UFR_Priority_Cat_15
				'Created: 10/12/2023 by Ralph Hudack
				'Updated: mm/dd/yyyy by <name> Modification: 
					'Updated EH 090424 RMW-1565 Changed to annual for PGM_C20XX
				If args.FunctionName.XFEqualsIgnoreCase("CategoryMemberList") Then
					Dim sWFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
					Dim sWFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim sWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
					Dim sUFRTime As String = sWFYear
					Dim sIC As String = sREQEntity
					Dim sCatNum As String ="" ' "01"  'Range is 01 to 15
					Dim sCatNameMemberScript As String =""   'Generated string for BRApi Get Data Cell for REQ_Priority_Cat_Name
					Dim sCatName As String =""        'Category Name obtained from BRApi Get Data Cell for REQ_Priority_Cat_Name
					Dim sCategoryMemberList As String =""  'Returned Value: Example A#REQ_Priority_Cat_Name:V#Periodic:U5#UFR_Priority_Cat_01:Name(1st Category), 
					'Loop 15 times for UFR_Priority_Cat_01 to UFR_Priority_Cat_15
					For iCounter As Integer = 1 To 15 
						'Get Category Name and generate Category Member List
						If iCounter < 10 Then
							sCatNum =  "0" & iCounter.ToString  'Add leading zero's for single digit integers. Ex 01 to 09
						Else
							sCatNum =  iCounter.ToString 
						End If
						sCatNameMemberScript = "Cb#" & sWFCube & ":E#" & sREQEntity & ":C#Local:S#" & sWFScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Priority_Cat_Name:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#UFR_Priority_Cat_" & sCatNum & ":U6#None:U7#None:U8#None"
						sCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sWFCube, sCatNameMemberScript).DataCellEx.DataCellAnnotation
''brapi.ErrorLog.LogMessage(si, "sCatName = " & sCatName)
						If Len(sCatName) > 0 Then 
							'Concatenate Category Member List
'							sCategoryMemberList = sCategoryMemberList & ",A#REQ_Priority_Cat_Name:V#Periodic:I#" & sIC & ":U5#UFR_Priority_Cat_" & sCatNum & ":Name(" & sCatName & ")"
							sCategoryMemberList = sCategoryMemberList & ",A#REQ_Priority_Cat_Name:V#Periodic:U5#UFR_Priority_Cat_" & sCatNum & ":I#" & sIC &":Name(" & sCatName & ")"
						End If
						If iCounter = 1 And Len(sCatName) < 1 Then 
							'Missing UFR Weights in REQ_Priority_Cat_Weight so send back dummy value with embedded error message.
							Dim sErrorMessage As String = "A#REQ_Priority_Cat_Name:V#Periodic:U5#UFR_Priority_Category:Name(Missing PGM Weights. Contact PGM Manager.)"
							Return sErrorMessage
						End If
					Next
''brapi.ErrorLog.LogMessage(si,"PriorityCategoryMemberList: ||sCatNameMemberScript=" & sCatNameMemberScript & " ||sCategoryMemberList=" & sCategoryMemberList & "||")	
					Return sCategoryMemberList
				End If

		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
		Return Nothing
	End Function
#End Region  ' DELETE?

#Region "ParseREQEntity"
		Public Function ParseREQEntity(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sTitle As String = args.NameValuePairs.XFGetValue("title")
					'If first time user OR haven't used dashboards since title was switched from "UFR" to "Entity UFR" for parsing
					If String.IsNullOrEmpty(sTitle) Or sTitle = " " Or Not sTitle.Contains(" ") Then
						Dim sEDefault As String = "ARMY"					
						Return sEDefault
					End If
					Dim sEntity As String = sTitle.Split(" ")(0)
''brapi.ErrorLog.LogMessage(si,"sEntity=" & sEntity)					
					Return sEntity

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region ' DELETE?

#Region "ParseREQFlow"
		Public Function ParseREQFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sTitle As String = args.NameValuePairs.XFGetValue("title")
''brapi.ErrorLog.LogMessage(si,"title=" & sTitle)					
					'If first time user OR haven't used dashboards since title was switched from "UFR" to "Entity UFR" for parsing
					If String.IsNullOrEmpty(sTitle) Or sTitle = " " Or Not sTitle.Contains(" ") Then
						Dim sFDefault As String = "Top"						
						Return sFDefault
					End If
					Dim sFlow As String = sTitle.Split(" ")(1)
''brapi.ErrorLog.LogMessage(si,"flow=" & sFlow)
					Return sFlow
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region ' DELETE?

#Region "Show and Hide Return Comment"
		Public Function ShowHideReturnComment(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs,ByVal sFlow As String, ByVal sEntity As String, ByVal sCube As String, ByVal sScenario As String, ByVal sTime As String) As String
			Try
				
			Dim sUFRHideRowScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#REQ_Return_Cmt:F#" & sFlow &":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
			Dim sUFRHideRow As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRHideRowScript).DataCellEx.DataCellAnnotation
			
			
'BRApi.ErrorLog.LogMessage(si,"sHiderow=" & sUFRHideRowScript)

'BRApi.ErrorLog.LogMessage(si,"sHiderowVal=" & sUFRHideRow)
			If String.IsNullOrWhiteSpace(sUFRHideRow) Then 
				Return "False"
			Else 
				Return "True"
			
			End If 
			Return Nothing
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
		End Function
#End Region' DELETE?	

#Region "DisplayAltRows"
	Public Function DisplayAltRows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim x As Integer = InStr(wfProfileName, ".")
		Dim sProfileSubString As String = wfProfileName.Substring(x + 0)
		Dim RowNum As String = args.NameValuePairs.XFGetValue("rowNum")
		If sProfileSubString = "Review Financials" And RowNum.XFContainsIgnoreCase("Row1") Then 
			Return "True"
		Else If sProfileSubString = "Validate Requirements" And RowNum.XFContainsIgnoreCase("Row2") Then 
			Return "True"
		Else If sProfileSubString = "Validate Requirements CMD" And RowNum.XFContainsIgnoreCase("Row3") Then 
			Return "true"
		Else
			Return "False"
		End If 

	End Function
#End Region ' DELETE

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
#End Region ' DELETE

#Region "DisplayRateInfo"
	Public Function DisplayRateInfo(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	
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
		#End Region ' I think we keep 
		
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
		#End Region ' I think we keep 
		
		#Region "Display Complete or Revert PGM WF Steps Components"
		Public Function DisplayWFComponentsCMDPGM(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			Dim sWFProfile As String = curProfile.Name
			If sWFProfile.Contains("CMD") Then		
				Return "True"
				'brapi.ErrorLog.LogMessage(si, "True")
			Else
				Return "False"
			End If
		End Function
		#End Region ' DELETE
		
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
		#End Region ' DELETE?
		
		#Region "DynamicU8"
		'======================================================================================================================================================================
		' Return U8# according to passed in Cube View name and wf profile name
		'======================================================================================================================================================================
		' Usage: 
		'	- CV: REQ_Funding_Month_Amt
		'	- Monthly Or Year Requested Amount, Row Overrides
		'	- In this instance, the method is used to return U8#Top to make the cell read-only for the Review Requirements workflow instead of having to create a separate CV
		'======================================================================================================================================================================
		Public Function DynamicU8 (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sCV As String = args.NameValuePairs.XFGetValue("CV")
			Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
			
			Select Case sCV
				Case "REQ_Funding_Month_Amt"
					If wfProfileName.XFContainsIgnoreCase("Review Requirements") 
						Return "U8#Top"
					End If
					If (wfProfileName.XFContainsIgnoreCase("Formulate Req") And sAccount = "REQ_Validated_Amt")
						Return "U8#Top"
					End If
					If wfProfileName.XFContainsIgnoreCase("Validate Req") And sAccount = "REQ_Validated_Amt" Then Return Nothing
					If Not (wfProfileName.XFContainsIgnoreCase("Formulate") Or wfProfileName.XFContainsIgnoreCase("Approve") Or wfProfileName.XFContainsIgnoreCase("Validate"))Then Return "U8#Top"
				Case "REQ_Base_Info_RO_ALT"
					If wfProfileName.XFContainsIgnoreCase("Review Financials") 
				Return "None"
			Else
				Return "ReadOnlyAnnotation"
			End If
		End Select
		Return Nothing
	End Function
#End Region ' DELETE?

#Region "Get FC for Base Info CBX"
	Public Function GetFC (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	
	'get first 3 FC characters for base info CV comboboxes
		Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
		Dim FC As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk , $"E#{wfCube}.Children.Where((Name DoesNotContain ROC) And (Name DoesNotContain General))", True,,)								
'BRapi.ErrorLog.LogMessage(si, $"sFC = {FC.Item(0).Member.Name}")
		Return "'" & FC.Item(0).Member.Name & "'"
		
'	Dim sFC As String = args.NameValuePairs.XFGetValue("FC")
''BRapi.ErrorLog.LogMessage(si, $"sFC = {sFC}")		
'		Return "'"&sFC.Substring(0,3)&"'"
	End Function	
#End Region  ' DELETE

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
#End Region ' DELETE?

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
#End Region ' DELETE

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
		
#End Region ' DELETE

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
		
#End Region ' DELETE?

#Region "GetUserCommandPGMExport"
		'Return user's command for PGM REQ export
		Public Function GetUserCommandPGMExport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As DashboardStringFunctionArgs)
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
#End Region ' DELETE?

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
		
#End Region ' DELETE

#Region "ShowHideManpowerCreate"
		Public Function ShowHideManpowerCreate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sButtonName As String = args.NameValuePairs.XFGetValue("ButtonName")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			Dim REQTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			'
			'			Select *
			'From XFC_CMD_SPLN_REQ
			'Where Entity = 'A97'
			'And WFCMD_Name = 'AFC'
			'And WFScenario_Name = 'CMD_SPLN_C2026'
			'And WFTime_Name = '2026'
			'And REQ_ID_Type = 'Manpower Req'
			Dim ManpowerREQTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQTitleScr).DataCellEx.DataCellAnnotation
'brapi.ErrorLog.LogMessage(si,"Hit CM")
			If sButtonName.XFEqualsIgnoreCase("Create") Then
				If String.IsNullOrEmpty(ManpowerREQTitle) Then
					sIsVisible = "IsVisible = True"
				Else
					sIsVisible = "IsVisible = False"
				End If
			Else If sButtonName.XFEqualsIgnoreCase("Import") Then 
				If String.IsNullOrEmpty(ManpowerREQTitle) Then
					sIsVisible = "IsVisible = False"
				Else
					sIsVisible = "IsVisible = True"
				End If
			End If 
			
			Return sIsVisible
		End Function
		
#End Region 'Updated 09/23/2025 DELETE

#Region "ShowHideManpowerSave"
		Public Function ShowHideManpowerSave(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sREQ_ID As String = args.NameValuePairs.XFGetValue("REQ_ID")
			Dim sButton As String = args.NameValuePairs.XFGetValue("Button")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			
			Dim sql As String = String.empty
			SQL = $"Select distinct Flow
				FROM XFC_CMD_SPLN_REQ_Details
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
			
			If sButton.XFContainsIgnoreCase("Approve") Then 			
				If sStatus.XFContainsIgnoreCase("Formulate") Then 
					sIsVisible = "IsVisible = True"
				Else 
					sIsVisible = "IsVisible = False"
				End If 
			ElseIf sButton.XFContainsIgnoreCase("Revert") Then
				If sStatus.XFContainsIgnoreCase("Final") Then 
					sIsVisible = "IsVisible = True"
				Else 
					sIsVisible = "IsVisible = False"
				End If 
			End If 
					
			Return sIsVisible
		End Function
		
#End Region 'Updated 09/23/2025


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
		
#End Region ' DELETE?

#Region "Get Mass Upload Status"
		Public Function GetMassUploadStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
'BRApi.ErrorLog.LogMessage(si,"In XFBR")			
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus","")
			
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region ' DELETE?

#Region "Get Demote Button Visibility"
		'Return IsVisible = True/False for Demote button - this is applied so that L3 Base Validator cannot see/demote their requirements to a non-existent status for their FC
		Public Function GetDemoteBtnVisibility(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String			
			Dim fc As String = args.NameValuePairs.XFGetValue("FC")
			If String.IsNullOrWhiteSpace(fc) Then Return "False"
			If fc.XFContainsIgnoreCase("_General")
				Return "True"
			Else
				Return "False"
			End If
		End Function
#End Region ' DELETE

#Region "Adjust Funding Line Comboboxes"
		'Return IsVisible = True/False for Adjust Funding Line Comboboxes
		Public Function AdjustFundingLine(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String			
		Dim sVisible As String = args.NameValuePairs.XFGetValue("Adjust")
			If sVisible.XFEqualsIgnoreCase("Adjust") Then 
				Return "True"
			Else
				Return "False"
			End If
		End Function
#End Region ' DELETE

#Region "Dynamic Flow Member"
		'XFBR(Workspace.CMD_SPLN.CMD_SPLN_Assembly.CMD_SPLN_String_Helper, DynFlowMember, wfYear=[], FlowMbr=[])
		Public Function DynFlowMember(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim wfYear As String = args.NameValuePairs("WFYear")
				Dim sFlowMbr As String = args.NameValuePairs("FlowMbr")
				Dim sFlowMbrAdj As String = sFlowMbr.Split("_"c)(0) & "_Dist_Final"
				Dim OverrideRow As String = "S#CMD_TGT_C" & wfYear & ":F#" & sFlowMbrAdj
'				Brapi.Errorlog.LogMessage(si, "OverrideRow: " & OverrideRow)
				Return OverrideRow
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region		


#Region "Dynamic Account Validations"
		'XFBR(Workspace.CMD_SPLN.CMD_SPLN_Assembly.CMD_SPLN_String_Helper, DynAccountValidations, ValidationSelected=[], Field=[])
		Public Function DynAccountValidations(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim sField As String = args.NameValuePairs("Field")
				Dim ValidationSelected As String = args.NameValuePairs("ValidationSelected")
				Dim Result As String = String.Empty
				Select Case ValidationSelected
					Case "Target"
						If sField = "TGT_WH"
							Result = "T#|WFYear|:A#Target:Name(Target |WFYear|)"
						Else If sField = "Commitments" Then
							Result = "GetDataCell(T#|WFYear|:A#Commitments + T#|WFYearNext|:A#Commitments):Name(Commitments |WFYear|)"
						Else If sField = "Obligations" Then
							Result = "GetDataCell(T#|WFYear|:A#Obligations + T#|WFYearNext|:A#Obligations):Name(Obligations |WFYear|)"
						End If
					Case "Withholds"
						If sField = "TGT_WH"
							Result = "T#2026:A#TGT_WH:Name(Withholds |WFYear|)"
						Else If sField = "Commitments" Then
							Result = "GetDataCell(T#|WFYear|:A#WH_Commitments + T#|WFYearNext|:A#WH_Commitments):Name(WH Commitments |WFYear|)"
						Else If sField = "Obligations" Then
							Result = "GetDataCell(T#|WFYear|:A#WH_Obligations + T#|WFYearNext|:A#WH_Obligations):Name(WH Obligations |WFYear|)"
						End If
					Case "Total"
						If sField = "TGT_WH"
							Result = "GetDataCell(T#|WFYear|:A#Target + T#|WFYear|:A#TGT_WH):Name(Total |WFYear|)"
						Else If sField = "Commitments" Then
							Result = "GetDataCell(T#|WFYear|:A#Commitments + T#|WFYearNext|:A#Commitments + T#|WFYear|:A#WH_Commitments + T#|WFYearNext|:A#WH_Commitments):Name(Tot Commitments |WFYear|)"
						Else If sField = "Obligations" Then
							Result = "GetDataCell(T#|WFYear|:A#Obligations + T#|WFYearNext|:A#Obligations + T#|WFYear|:A#WH_Obligations + T#|WFYearNext|:A#WH_Obligations):Name(Tot Obligations |WFYear|)"
						End If		
				End Select
				
				Return Result
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region		


#Region "Get User Funds Center By Workflow"
'New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundsCenterByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim output As String = String.Empty
				Dim userSecGroup As DataSet = brapi.Dashboards.Process.GetAdoDataSetForAdapter(si,False,"da_GetUserSec","FundsCenterByWF",Nothing)	
				Dim userSecGroup_dt As DataTable = userSecGroup.Tables(0)
				For Each FC As datarow In userSecGroup_dt.rows
'brapi.ErrorLog.LogMessage(si,"FC CM KL = " & FC.Item("Value"))
					If String.IsNullOrEmpty(output)
						output = FC.Item("Value")
					Else 
						output = output & "," & FC.Item("Value")
					End If 
					
				Next
		
			Return output
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

Public Function GetFCAPPNforSubmission(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As DataTable
	brapi.ErrorLog.LogMessage(si,"inside getfcappn")
'------------------------------Prior Code-------------------------------------
			Dim dt As New DataTable()
			dt.TableName = "PackageForSubmission"
			Dim scenFilter As String = String.empty
			Dim cvname As String = "CMD_SPLN_Package_Summary_FDX"
			Dim wsName As String = "50 CMD SPLN"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim timeFilter = "T#2026" '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)
			Dim entityMemFilter As String = args.NameValuePairs.XFGetValue("entityMemFilter")
			dt = BRApi.Import.Data.FdxExecuteCubeView(si, wsID, cvName, String.Empty, String.Empty,String.Empty,String.Empty, timeFilter, nvbParams, False, True, String.Empty, 8, False)
			'dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, String.Empty, String.Empty,String.Empty,String.Empty, timeFilter, nvbParams, False, True, String.Empty, 8, False)
'---------------------------------- Prior Code End---------------------------------------------------
	For Each column As DataColumn In dt.Columns
		If column.Namespace Then
			
		end if 
	Next




Return dt
		End Function
		
#Region "Package Submit"
	Public Function PackageSubmit(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
brapi.errorlog.Logmessage(si,"inside Submit")
		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim wfProfileNameAdj As String = wfProfileName.Split("."c)(1).Split(" "c)(0)
		Dim lEntity As List(Of String) = stringhelper.splitstring(Me.GetUserFundsCenterByWF(si,globals,api,args),",")
		Dim Scenario As String = args.NameValuePairs("Scenario")
		Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
		Dim Time As String = args.NameValuePairs("Time")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = ""
		Dim FilterString As String
		Dim sLevel As String = "EntityLevel=" & args.NameValuePairs("Level")
		For Each e As String In lEntity			
			FilterString = String.Empty
	
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, e).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,Time)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			If Not sLevel =  entityText3 Then Continue For
			FilterString = $"Cb#{sCube}:C#Local:S#CMD_SPLN_C{Time}:T#{Time}:E#[{e}]:V#Periodic:O#Top:I#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"					

			globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({FilterString}))")
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
	
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
	
			Dim objU1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U1_FundCode")
	
			For Each msb In results.Keys
			   msb.Scenario = vbNullString
			   msb.Entity =  e	   
			   msb.Account = vbNullString
			   msb.Origin = vbNullString
			   msb.IC = vbNullString
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU1DimPK, "U1#" &  msb.UD1 & ".Ancestors.Where(MemberDim = U1_APPN)", True,,)
			   msb.UD1 = lsAncestorList(0).Member.Name
			   msb.UD2 = vbNullString   
			   msb.UD3 = vbNullString
			   msb.UD4 = vbNullString
			   msb.UD5 = vbNullString
			   msb.UD6 = vbNullString
			   msb.UD7 = vbNullString
			   msb.UD8 = vbNullString	 
			   
						
						If Not toSort.ContainsKey(msb.GetMemberScript) Then 
							toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}F#{msb.flow}")
						End If
					
			Next
			End If
			Next
		
		Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
		For Each item In sorted
			output &= item.key & ","
		Next
	'brapi.ErrorLog.LogMessage(si, "PackageSubmit ouput: " & output)	
		If output = "" Then
		output = "U8#None"
		End If
		
		Return output
	End Function
#End Region

#Region "Get Pay and Non Pay Members"
	'XFBR(Workspace.CMD_SPLN.CMD_SPLN_Assembly.CMD_SPLN_String_Helper,getpaynonpaymembers,Entity=[|!LR_Entity_CMD_SPLN_Package_Summary!|],Time=[|WFTime|])
	Public Function GetPayNonPayMembers(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
		Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
		Dim Time As String = args.NameValuePairs("Time")
		Dim Result As String = String.Empty
		If Entity = String.Empty Or Entity = "NA" Or Entity = vbNullString Or Entity = "" Or Time = String.Empty Or Time = "NA" Or Time = vbNullString Then
			Return "U1#Top:U2#Top:U3#Top:U4#Top:U6#Top"
		End If
		Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,Entity.Substring(0,3),Time)
		For Each pair In Val_Approach
			If pair.Key.xfcontainsIgnoreCase("Pay_NonPay") Then
				If pair.Value ="Yes" Then
					Result = "U6#Pay_Benefits, U6#Non_Pay"
				Else If pair.value = "No" Then
					Result = "U6#CostCat"
				End If
			End If
		Next
		Return Result
	End Function
#End Region		

	End Class
End Namespace


