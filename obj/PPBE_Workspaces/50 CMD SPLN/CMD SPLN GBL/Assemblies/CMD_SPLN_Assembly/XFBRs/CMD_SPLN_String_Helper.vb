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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_SPLN_StringHelper
	Public Class MainClass
		Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Me.si = si
			Me.globals = globals
			Me.api = api
			Me.args = args
			
			Try
				Select Case args.FunctionName
				Case args.FunctionName.XFEqualsIgnoreCase("GetCascadingMbrFilter")	
					Return Me.GetCascadingMbrFilter()
				Case args.FunctionName.XFEqualsIgnoreCase("GetFDXFilteredRows")
					Return Me.GetFDXFilteredRows
				Case args.FunctionName.XFEqualsIgnoreCase("BUDControlsEntityFilter")	
					Return Me.BUDControlsEntityFilter(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("BUDDashboardsToRedraw")
					Return Me.BUDDashboardsToRedraw(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("BUDReportSubConsolidateTime")
					Return Me.BUDReportSubConsolidateTime(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("BUDRetrieveCache")
					Return Me.BUDRetrieveCache(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("DisplayAltRows")	
					Return Me.DisplayAltRows(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("DisplayCmdRows")
					Return Me.DisplayCmdRows(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("DisplayRateInfo")
					Return Me.DisplayRateInfo(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("DisplayWFComponentsCMDPGM")
					Return Me.DisplayWFComponentsCMDPGM(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("DynamicU8")
					Return Me.DynamicU8(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetAccountTranslatedValue")
					Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
					Dim sFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
					Dim sAccount As String = args.NameValuePairs.XFGetValue("BUDAccount")
					Dim sTranslationCd As String = args.NameValuePairs.XFGetValue("TranslationCd") 
'BRApi.ErrorLog.LogMessage(si, " GetAccountTranslatedValue: --sEntity: --" & sEntity & "--sFlow: --" & sFlow & "--" & "sAccount: --" & sAccount & "--" & "sTranslationCd: --" & sTranslationCd & "--")	
					If(String.IsNullOrWhiteSpace(sAccount)) Then 
						'BUDAccount is missing so throw an error
						Throw New Exception("ERROR GetAccountTranslatedValue: Account member name is required. Contact support." & environment.NewLine)
					End If
					Return Me.GetAccountTranslatedValue(si,globals,api,args, sAccount, sTranslationCd, sFlow, sEntity)
				Case args.FunctionName.XFEqualsIgnoreCase("GetAnnotationDetail")
					Return Me.GetAnnotationDetail(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("getBUDAnnotation")
'BRApi.ErrorLog.LogMessage(si,"here Acc = " & sAccount & " , flw = " & sFlow)						
					Return Me.getBUDAnnotation(si, globals, api, args, sAccount, sFlow, sEntity, sCube, sScenario, sTime, sIC, sU5, sU8)
				Case args.FunctionName.XFEqualsIgnoreCase("getBUDFC")
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
				Case args.FunctionName.XFEqualsIgnoreCase("GetFC")						
					Return Me.GetFC(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetFC_RO")			
					Return Me.GetFC_RO(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetFileGuidance")				
					Return Me.GetFileGuidance(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetFundingStatus")
					'Get current status of BUD
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)& "M12"
					Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
					Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
					Dim BUDTime As String = WFYear & "M12"
					
					Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
					Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
					
					
					Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Funding_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

					Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
									
					Return currentStatus
				Case args.FunctionName.XFEqualsIgnoreCase("GetLRVisibility")				
					Return Me.GetLRVisibility(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetStakeholdersEmail")
					Return Me.GetStakeholdersEmail(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetWorkflowStatus")
					'Get current status of BUD
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
					Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
					Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
					Dim BUDTime As String = WFYear & "M12"
					
					Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
					Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
					Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
'brapi.ErrorLog.LogMessage(si, currentStatus)									
					Return currentStatus
				Case args.FunctionName.XFEqualsIgnoreCase("IsBUDApprovalAllowed")	
					Dim BUDEntity As String = args.NameValuePairs.XFGetValue("UFREntity")
					Return Me.IsBUDApprovalAllowed(si, globals, api, args, BUDEntity)
				Case args.FunctionName.XFEqualsIgnoreCase("MDEPStripTime")
					Return Me.MDEPStripTime(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("MultiYrVisibility")			
					Return Me.MultiYrVisibility(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("MultiYrVisibilityU1")				
					Return Me.MultiYrVisibilityU1()
				Case args.FunctionName.XFEqualsIgnoreCase("ReadWriteManpowerCVs")			
					Return Me.ReadWriteManpowerCVs(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("RemoveGeneral")			
					Return Me.RemoveGeneral(si,globals,api,args)
'---Manpower Calls---
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideManpowerCreate")				
					Return Me.ShowHideManpowerCreate(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideManpowerRevert")				
					Return Me.ShowHideManpowerRevert(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideManpowerSave")			
					Return Me.ShowHideManpowerSave(si,globals,api,args)
'---Withhold Calls---
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideWithholdCreate")				
					Return Me.ShowHideWithholdCreate(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideWithholdSave")				
					Return Me.ShowHideWithholdSave(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("ShowHideReturnComment")
					Dim sFlow As String = args.NameValuePairs.XFGetValue("UFRFlow")
					Dim sEntity As String = args.NameValuePairs.XFGetValue("UFREntity")
					Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
					Dim sScenario As String = args.NameValuePairs.XFGetValue("UFRScenario")
					Dim sTime As String = args.NameValuePairs.XFGetValue("UFRTime")	
					Return Me.ShowHideReturnComment(si, globals, api, args, sFlow, sEntity, sCube, sScenario, sTime)
				Case args.FunctionName.XFEqualsIgnoreCase("TitleParseEntity")
					Return Me.ParseBUDEntity(si, globals, api, args)					
				Case args.FunctionName.XFEqualsIgnoreCase("TitleParseFlow")
					Return Me.ParseBUDFlow(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("YOYLimitColorCode")	
					Return Me.YOYLimitColorCode(si, globals, api, args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetMassUploadStatus")			
					Return Me.GetMassUploadStatus(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("GetGVSql")
					Return Me.GetGVSql(si,globals,api,args)
				Case args.FunctionName.XFEqualsIgnoreCase("getBUDAnnotationFormulateReq")
					Dim sU8 As String = args.NameValuePairs.XFGetValue("U8")
					Dim sMode As String = args.NameValuePairs.XFGetValue("sMode")
					Return Me.getBUDAnnotationFormulateReq(si, globals, api, args, sU8, sMode)

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetCascadingMbrFilter() As String
			Try
				' Read incoming args
				Dim fundsCenter As String = args.NameValuePairs.XFGetValue("fundsCenter", "NA")
				Dim appn As String = args.NameValuePairs.XFGetValue("appn", "NA")
				Dim mdep As String = args.NameValuePairs.XFGetValue("mdep", "NA")
				Dim sag As String = args.NameValuePairs.XFGetValue("sag", "NA")
				Dim ape As String = args.NameValuePairs.XFGetValue("ape", "NA")
				Dim dollarType As String = args.NameValuePairs.XFGetValue("dollarType", "NA")
				Dim status As String = args.NameValuePairs.XFGetValue("status", "NA")
				Dim returnType As String = args.NameValuePairs.XFGetValue("returnType", "NA")

				' Build a deterministic signature of the inputs
				Dim currentSignature As String = String.Concat(fundsCenter, "|", appn, "|", mdep, "|", sag, "|", ape, "|", dollarType, "|", status, "|", returnType)

				' Use workspace session settings to persist last seen signature per user/workspace
				Dim cacheCategory As String = "CascadingFilterCache"
				Dim signatureKey As String = "SignatureKey"
				Dim prevSignature As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCategory, signatureKey, "")

				Dim needsRebuild As Boolean = Not String.Equals(prevSignature, currentSignature, StringComparison.Ordinal)

				If needsRebuild Then
					' mark new signature in the session so subsequent calls know inputs haven't changed
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, cacheCategory, signatureKey, currentSignature)
					' Caller can decide to rebuild the datatable when this function returns "True"
				End If

				' Optionally fetch the stored datatable (if one already exists) - caller can choose to use or rebuild it
				Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName, "CascadingFilter")

				' Return a simple string flag indicating whether a rebuild is required.
				' If you need to return the actual member-filter string, replace this return with your original serialization logic using 'dt'.
				Return needsRebuild.ToString()
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "BUDControlsEntityFilter: BUDControlsEntityFilter"
		Public Function BUDControlsEntityFilter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
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
		
#End Region

#Region "BUDDashboardsToRedraw: BUDDashboardsToRedraw"
		Public Function BUDDashboardsToRedraw(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
'BRAPI.ErrorLog.LogMessage(si, sProfileName)
				Dim sLRDashboard As String = "BUDPRO_BUDLDB_0__ReviewBUDDetails"
				'Dim ufr As String = args.NameValuePairs.XFGetValue("UFR_Id")
				
				If sProfileName = "Review Financials" Or sProfileName = "Validate Requirements" Or sProfileName = "Validate Requirements CMD" Then 
					Return "BUDPRO_BUDVAL_0__Main" & "," & sLRDashboard
					
				Else If sProfileName = "Formulate Requirements" Then 
					Return "BUDPRO_BUDCRT_0__Main" & "," & sLRDashboard
					
				Else If wfProfileName.XFContainsIgnoreCase("Manage") Then 
					Return "BUDPRO_BUDMGT_0__Main" & "," & sLRDashboard
					
				Else If sProfileName = "Review Requirements" Then 
					Return "BUDPRO_BUDREV_0__Main" & "," & sLRDashboard
					
				Else If sProfileName = "Rollover Requirements" Then 
					Return "BUDPRO_BUDREV_0__Main" & "," & sLRDashboard
					
				Else If wfProfileName.XFContainsIgnoreCase("Approve") Then 
					Return "BUDPRO_BUDAPR_0__Main" & "," & sLRDashboard
					
				End If 
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try
			Return Nothing
		End Function
		
#End Region

#Region "BUDReportSubConsolidateTime: BUDReportSubConsolidateTime"
		'Return 5 years Time for subconsolidate data management sequence for BUD Report
		Public Function BUDReportSubConsolidateTime(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
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
		
#End Region

#Region "BUDRetrieveCache: BUDRetrieveCache"
		'Updated EH 8/29/24 RMW-1565 Retrieve title from REQ_Shared
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function BUDRetrieveCache(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
				Dim sBUDToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")	
'brapi.ErrorLog.LogMessage(si,"sEntityto demote = " & sEntity)
'brapi.ErrorLog.LogMessage(si,"sBUDto demote = " & sBUDToDelete)
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim BUDTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & sBUDToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleMemberScript).DataCellEx.DataCellAnnotation
				
				Dim cachedBUD As String = sBUDToDelete & " - " & TitleValue
		
				Return cachedBUD
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region

#Region "DisplayAltRows: DisplayAltRows"
	Public Function DisplayAltRows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim x As Integer = InStr(wfProfileName, ".")
		Dim sProfileSubString As String = wfProfileName.Substring(x + 0)
		Dim RowNum As String = args.NameValuePairs.XFGetValue("rowNum")
		If sProfileSubString = "Review Financials" And RowNum.XFContainsIgnoreCase("Row1") Then 
			Return "True"
		Else If sProfileSubString = "Validate Budgets" And RowNum.XFContainsIgnoreCase("Row2") Then 
			Return "True"
		Else If sProfileSubString = "Validate Budgets CMD" And RowNum.XFContainsIgnoreCase("Row3") Then 
			Return "true"
		Else
			Return "False"
		End If 

	End Function
#End Region

#Region "DisplayCmdRows: DisplayCmdRows"
	Public Function DisplayCmdRows(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim currentProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
		Dim sWFProfile As String = currentProfile.Name
		If  sWFProfile.Contains("CMD") Then		
			Return "True"
		Else
			Return "False"
		End If
	End Function
#End Region

#Region "DisplayRateInfo: DisplayRateInfo"
	Public Function DisplayRateInfo(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")
'BRApi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ": sEntitySelection=" & sEntitySelection) 				
			If 	sCube <> "" Then wfCube = sCube

			Dim sScenarioName As String = args.NameValuePairs.XFGetValue("Scenario")
			If sScenarioName <> "" Then 	wfScenarioName = sScenarioName

			Dim sTimeName As String = wfScenarioName.Substring(5,4)
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

#Region "DisplayWFComponentsCMDPGM: Display Complete or Revert PGM WF Steps Components"
	Public Function DisplayWFComponentsCMDPGM(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
		Dim sWFProfile As String = curProfile.Name
		If  sWFProfile.Contains("CMD") Then		
			Return "True"
'brapi.ErrorLog.LogMessage(si, "True")
		Else
			Return "False"
		End If
	End Function
#End Region

#Region "DynamicU8: DynamicU8"
	'======================================================================================================================================================================
	' Return U8# according to passed in Cube View name and wf profile name
	'======================================================================================================================================================================
	' Usage: 
	'	- CV: BUD_Funding_Month_Amt
	'	- Monthly Or Year Requested Amount, Row Overrides
	'	- In this instance, the method is used to return U8#Top to make the cell read-only for the Review Budgets workflow instead of having to create a separate CV
	'======================================================================================================================================================================
	
	Public Function DynamicU8(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim sCV As String = args.NameValuePairs.XFGetValue("CV")
		Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
		
		Select Case sCV
		Case "BUD_Funding_Month_Amt"
			If wfProfileName.XFContainsIgnoreCase("Review Budgets") 
				Return "U8#Top"
			End If
			If (wfProfileName.XFContainsIgnoreCase("Formulate Bud") And sAccount = "REQ_Validated_Amt")
				Return "U8#Top"
			End If
			If wfProfileName.XFContainsIgnoreCase("Validate Bud") And sAccount = "REQ_Validated_Amt" Then Return Nothing
			If Not (wfProfileName.XFContainsIgnoreCase("Formulate") Or wfProfileName.XFContainsIgnoreCase("Approve") Or wfProfileName.XFContainsIgnoreCase("Validate"))Then Return "U8#Top"
			
		Case "BUD_Base_Info_RO_ALT"
			If wfProfileName.XFContainsIgnoreCase("Review Financials") 
				Return "None"
			Else
				Return "ReadOnlyAnnotation"
			End If
		End Select
		Return Nothing
	End Function
#End Region

#Region "GetAccountTranslatedValue: Get Account Translated Value Function"
		Public Function GetAccountTranslatedValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs, _
									              ByVal sAccount As String, ByVal sTranslationCd As String, ByVal sFlow As String, ByVal sEntity As String) As String
			Try
				'Purpose: Get the Annotation Value of an Account for a given BUD and either return that value or return a translated value depding on sTranslationCd.
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

		
#End Region

#Region "GetAnnotationDetail: Get Annotation Detail"
		'Updated EH 8/29/24 RMW-1565 Updated wfTime to Annual
		Public Function GetAnnotationDetail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim BUDTime As String = WFYear & "M12"
				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim BUDFlow As String =  args.NameValuePairs.XFGetValue("Flow")
				
				If String.IsNullOrEmpty(BUDEntity) Or String.IsNullOrEmpty(BUDFlow) Then			
					BUDEntity = ParseBUDEntity(si, globals, api, args)
					BUDFlow = ParseBUDFlow(si, globals, api, args)	
				End If
				
				Dim comments As String = ""

				
'BRApi.ErrorLog.LogMessage(si, "entitiy " & BUDEntity)
'BRApi.ErrorLog.LogMessage(si, "flow  " & BUDFlow)
				'Get Annotaion detail. Attachement Type 300
				Dim sql = "SELECT Text, CreatedUserName, CreatedTimestamp, LastEditedUserName, LastEditedTimestamp
							  From DataAttachment with (NOLOCK)
							  WHERE 
								Cube = '" & wfCube & "'" &
								" and Entity =  '" & BUDEntity & "'" &
								" and Scenario =  '" & wfScenario & "'" &
								" and Flow =  '" & BUDFlow & "'" &
								" and Time =  '" & BUDTime & "'" &
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
'BRApi.ErrorLog.LogMessage(si, "sql: " & sql)									
				Dim dtComments As DataTable = New DataTable("comments")
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					dtComments = BRApi.Database.ExecuteSql(dbConnApp, sql, False)
				End Using 
				
				
				For Each rw As DataRow In dtComments.Rows
					comments = vbCrLf & "Created by: " & rw.Item("CreatedUserName") & " (" & rw.Item("CreatedTimestamp") & ")" _
					           & " - " & "Updated by: " & rw.Item("LastEditedUserName") & " (" & rw.Item("LastEditedTimestamp") & ")" _
							   & vbCrLf & rw.Item("Text") _
							   & vbCrLf & comments
				Next
'BRApi.ErrorLog.LogMessage(si, "c: " & comments)
				Return comments
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "getBUDAnnotation: Get BUD Annotation Function"
	'Get BUD Annotation using Dim infos
	Public Function getBUDAnnotation() As String
		Try
			Dim sAccount As String = args.NameValuePairs.XFGetValue("BUDAccount")
			Dim sFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("BUDScenario")
			Dim sTime As String = args.NameValuePairs.XFGetValue("BUDTime")
			Dim sIC As String = args.NameValuePairs.XFGetValue("IC")
			Dim sU5 As String = args.NameValuePairs.XFGetValue("U5","None")
			Dim sU8 As String = args.NameValuePairs.XFGetValue("U8","None")
			Dim sMemberScript As String = ""
			If sAccount.XFContainsIgnoreCase("Stkhldr")	Then
				sMemberScript = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#" & sAccount & ":F#" & sFlow & ":O#BeforeAdj:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#" & sU8 
		
			Else
			'Get annotations to return
				sMemberScript = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#" & sAccount & ":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#" & sU8 
			End If
			Dim BUDAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sMemberScript)
			Dim BUDAnnotation As String = BUDAnnotationCell.DataCellEx.DataCellAnnotation
	
'BRApi.ErrorLog.LogMessage(si,$"sAccount = {sAccount} || sFlow = {sFlow} || sEntity = {sEntity} || sCube = {sCube} || sScenario = {sScenario} || sTime = {sTime} || sIC = {sIC} || sU5 = {sU5} || sU8 = {sU8} ||BUDAnnotation = {BUDAnnotation}")			
		
			Return BUDAnnotation
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
#End Region

#Region "GetFC: Get FC for Base Info CBX"
	Public Function GetFC(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	
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
#End Region

#Region "GetFC_RO: Get FC for Allow Budget Rollover"
	Public Function GetFC_RO(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
	'get WFP name for hiding "allow req rollover" column
	Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
	Dim x As Integer = InStr(wfProfileName, ".")
	Dim sProfileName As String = wfProfileName.Substring(x + 0)
	Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
	
'Brapi.ErrorLog.LogMessage(si, sProfileName)
	If sProfileName = "Manage Requirements CMD" Then
		Return "True"
	Else
		Return "False"
	End If
	End Function
#End Region	

#Region "GetFileGuidance: Get File For CMD PGM Guidance"
		Public Function GetFileGuidance(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sFileName As String = args.NameValuePairs.XFGetValue("FileName")	
			If sFileName = "" Then Return Nothing
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sFileNameLower As String = sFileName.ToLower
			Dim sFilePath As String = "Documents/Public/CMD_Budget"
			If sFileNameLower.StartsWith(sCube.ToLower) And Not sFileNameLower.EndsWith("pdf") Then
				sFilePath = $"{sFilePath}/{sCube}/{sFileName}"
			Else If sFileNameLower.StartsWith("ARMY".ToLower) And Not sFileNameLower.EndsWith("pdf") Then
				sFilePath = $"{sFilePath}/ARMY/{sFileName}"
			End If
'BRApi.ErrorLog.LogMessage(si,sFilePath)
			Return sFilePath
			
		End Function
#End Region

#Region "GetLRVisibility: Get Linked Report Read Only / Visibility"
		'Return U8#ReadOnlyAnnotation or Visibility for Linked Report
		'Function works by comparing the workflow name and the TrueValue passed in as an arg from the cv or component. Return desired behavior based on the mode, and the result of the comparison
		'ex: btn_CMD_SPLN_SubmitREQ
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
#End Region

#Region "GetStakeholdersEmail: Get Reviewer Email"
		Public Function GetStakeholdersEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim emails As String = ""
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				
				'logic to account for _General at the parent lvl
				If BUDEntity.XFContainsIgnoreCase("General") Then
					BUDEntity = BUDEntity.Replace("_General","")
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
						If (sFundCenters.XFContainsIgnoreCase(BUDEntity)) Then
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
#End Region 'need to revisit security logic in general

#Region "IsBUDApprovalAllowed: Is BUD Approval Allowed"
		Public Function IsBUDApprovalAllowed(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs, ByVal sEntity As String) As String
			Dim annotationFlag As String = args.NameValuePairs.XFGetValue("annotationFlag")	
			Dim formatFlag As String = args.NameValuePairs.XFGetValue("formatFlag")
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim iEntityID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sEntity)
			Dim sParentEntity As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "E_ARMY"), iEntityID, False)(0).Name
			Dim iParentID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sParentEntity)
			Dim sGeneralParent As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "E_ARMY"), iParentID, False)(0).Name
			Dim sUFRPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Allow_Approval:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
			Dim sUFRPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sUFRPermissionMbrScript).DataCellEx.DataCellAnnotation
			
			If sUFRPermission.XFEqualsIgnoreCase("No") And formatFlag.XFEqualsIgnoreCase("No") And annotationflag.XFEqualsIgnoreCase("Yes") Then 
				Return "U8#ReadOnlyAnnotation"
			Else If sUFRPermission.XFEqualsIgnoreCase("No") And formatFlag.XFEqualsIgnoreCase("No") And annotationflag.XFEqualsIgnoreCase("") Then 
				Return "U8#ReadOnlyData" 
			Else If sUFRPermission.XFEqualsIgnoreCase("No") And formatFlag.XFEqualsIgnoreCase("Yes")
				Return "BackgroundColor = XFLightBlue3,|!CubeViewCellStyleWholeNumber!|"
			Else If (sUFRPermission.XFEqualsIgnoreCase("Yes") And formatFlag.XFEqualsIgnoreCase("No")) Or (sUFRPermission.XFEqualsIgnoreCase("") And formatFlag.XFEqualsIgnoreCase("No"))
				Return "U8#None"
			Else If (sUFRPermission.XFEqualsIgnoreCase("Yes") And formatFlag.XFEqualsIgnoreCase("Yes")) Or (sUFRPermission.XFEqualsIgnoreCase("") And formatFlag.XFEqualsIgnoreCase("Yes"))
				Return "|!CubeViewCellStyleWholeNumber!|"
			End If 

			Return Nothing
		End Function
#End Region	

#Region "MDEPStripTime: MDEPStripTime"
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
		
#End Region

#Region "MultiYrVisibility: Multi Year Visibility"
		'Uses Enity and Flow to figure out UD1 of the funding line and determine whether if it's a multi year APPN. Return true or false for Visiility formatting
		Public Function MultiYrVisibility(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
				' Example: XFBR(z_BUD_String_Helper, MultiYrVisibility, Entity = XFBR(BUD_String_Helper, TitleParseEntity, title = |!prompt_cbx_BUDPRO_AAAAAA_BUDListByEntity__Shared_UpdateReq!|), Flow = XFBR(BUD_String_Helper, TitleParseFlow, title = |!prompt_cbx_BUDPRO_AAAAAA_BUDListByEntity__Shared_UpdateReq!|))
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTitle As String = args.NameValuePairs.XFGetValue("Title")
				Dim sFlow As String = Me.ParseBUDFlow(si, globals, api, args)	
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName				
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
				Dim sAccount As String = "Phased_Commitment"
				Dim sText2 As String = ""
				Dim U1ID As New Integer
'BRapi.ErrorLog.LogMessage(si, $"Entity = {sEntity}, Flow = {sFlow}, sCube = {sCube}, sScenario= {sScenario}, sBUDTime = {sBUDTime}")		
				
				Dim myDataUnitPk As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sBUDTime))

				' Buffer coordinates.
				' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
				myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, sFlow)
				myDbCellPk.OriginId = DimConstants.BeforeAdj	
				myDbCellPk.UD7Id = DimConstants.None
				myDbCellPk.UD8Id = DimConstants.None
				
				Dim myCells As List(Of DataCell) = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
				
				If myCells.Count = 0 Then Return "U8#ReadOnlyAnnotation"	
				
					For Each cell As DataCell In myCells
						U1Id = cell.DataCellPk.UD1Id
					Next
					Dim U1dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
					Dim oParentsList As List(Of Member) = BRApi.Finance.Members.GetParents(si, U1dimPK, U1Id, False,)					
					Dim sU1Parent As String = oParentsList.Item(0).Name
					Dim sU1ParentGen As String = sU1Parent & "_General"
					Dim sU1ParentGenID As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,sU1ParentGen)
		
					sText2 = Brapi.Finance.UD.Text(si,dimtype.UD1.Id,sU1ParentGenID,2,Nothing, Nothing)
					
'Brapi.ErrorLog.LogMessage(si, "Text2" & sText2 & "appn" & sU1ParentGen)
		
					If sText2.XFContainsIgnoreCase("Yes") Then
						Return "U8#None"
					Else
						Return "U8#ReadOnlyData"			
				End If
				
'				Dim U1dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
'				Dim oParentsList As List(Of Member) = BRApi.Finance.Members.GetParents(si, U1dimPK, U1Id, False,)					
'				Dim sU1Parent As String = oParentsList.Item(0).Name
'				Dim sU1ParentGen As String = sU1Parent & "_General"
'				'Dim Genmultistatus As String = BRApi.Finance.UD.Text()
'				Dim memberScript As String = $"Cb#{sCube}:E#{sCube}_General:C#Local:S#{sScenario}:T#{sBUDTime}M12:V#Annotation:A#REQ_Multi_Year_APPN_Ind:F#None:O#BeforeAdj:I#None:U1#{sU1Parent}_General:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim multiYrStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, memberScript).DataCellEx.DataCellAnnotation
				
				
'				If multiYrStatus = "Yes" Then
'					Return True
'				Else
'   					Return False
'				End If
			
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region

#Region "MultiYrVisibility: Multi Year Visibility Using U1"
		'Uses passed in UD1 of the funding line and determine whether if it's a multi year APPN. Return true or false for Visiility formatting
		Public Function MultiYrVisibilityU1() As String
		Try
			' Example: XFBR(BUD_String_Helper, MultiYrVisibilityU1, UD1=|!prompt_cbx_BUDPRO_BUDCRT_0CaAa_U1Filter__ReviseREQsByTarget!|)
			Dim sUD1 As String = args.NameValuePairs.XFGetValue("UD1")
			Dim U1ID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, sUD1)
'BRapi.ErrorLog.LogMessage(si, $"sUD1 = {sUD1}")		
		
			Dim U1dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			Dim oParentsList As List(Of Member) = BRApi.Finance.Members.GetParents(si, U1dimPK, U1Id, False,)					
			Dim sU1Parent As String = oParentsList.Item(0).Name
			Dim sU1ParentGen As String = sU1Parent & "_General"
			Dim sU1ParentGenID As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,sU1ParentGen)
			Dim sText2 = Brapi.Finance.UD.Text(si,dimtype.UD1.Id,sU1ParentGenID,2,Nothing, Nothing)
				
'Brapi.ErrorLog.LogMessage(si, "Text2" & sText2 & "appn" & sU1ParentGen)	
			If sText2.XFContainsIgnoreCase("Yes") Then
				Return "U8#None"
			Else
				Return "U8#ReadOnlyData"			
			End If
			
		Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))			
		End Try
	End Function
		
#End Region

#Region "ParseBUDEntity: ParseBUDEntity"
		Public Function ParseBUDEntity(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sTitle As String = args.NameValuePairs.XFGetValue("title")
					'If first time user OR haven't used dashboards since title was switched from "UFR" to "Entity UFR" for parsing
					If String.IsNullOrEmpty(sTitle) Or sTitle = " " Or Not sTitle.Contains(" ") Then
						Dim sEDefault As String = "ARMY"					
						Return sEDefault
					End If
					Dim sEntity As String = sTitle.Split(" ")(0)
'BRApi.ErrorLog.LogMessage(si,"sEntity=" & sEntity)					
					Return sEntity

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "ParseBUDFlow: ParseBUDFlow"
		Public Function ParseBUDFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sTitle As String = args.NameValuePairs.XFGetValue("title")
'BRApi.ErrorLog.LogMessage(si,"title=" & sTitle)					
					'If first time user OR haven't used dashboards since title was switched from "UFR" to "Entity UFR" for parsing
					If String.IsNullOrEmpty(sTitle) Or sTitle = " " Or Not sTitle.Contains(" ") Then
						Dim sFDefault As String = "Top"						
						Return sFDefault
					End If
					Dim sFlow As String = sTitle.Split(" ")(1)
'BRApi.ErrorLog.LogMessage(si,"flow=" & sFlow)
					Return sFlow
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "RemoveGeneral: Remove General"
	'======================================================================================================================================================================
	' Strip _General from passed in value
	'======================================================================================================================================================================
	' Usage: 
	'	- DL_CMD_SPLN_FundCenterFilter__Shared
	'======================================================================================================================================================================
	Public Function RemoveGeneral(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Dim sVal As String = args.NameValuePairs.XFGetValue("Entity")
		sVal = sVal.Replace("_General","")
'BRAPi.ErrorLog.LogMessage(si, "BUD_String_Helper.RemoveGeneral: sVal=" & sVal)		
		Return sVal
	End Function				
#End Region

#Region "ShowHideReturnComment: Show and Hide Return Comment"
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
#End Region

'---Manpower---
#Region "ShowHideManpowerCreate: ShowHideManpowerCreate"
		Public Function ShowHideManpowerCreate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sButtonName As String = args.NameValuePairs.XFGetValue("ButtonName")
			Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sFlow As String = "REQ_00_CP_" & sAPPN
			Dim sIsVisible As String = ""
			Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim ManpowerBUDTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleScr).DataCellEx.DataCellAnnotation
			Dim BUDStatusScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Rqmt_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim ManpowerBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScr).DataCellEx.DataCellAnnotation
			If sButtonName.XFEqualsIgnoreCase("Create") Then
				If String.IsNullOrEmpty(ManpowerBUDTitle) Then
					sIsVisible = "IsVisible = True"
				ElseIf Not String.IsNullOrEmpty(ManpowerBUDTitle) And ManpowerBUDStatus.XFContainsIgnoreCase("Working") Then
					sIsVisible = "IsVisible = False"
				ElseIf Not String.IsNullOrEmpty(ManpowerBUDTitle) And Not ManpowerBUDStatus.XFContainsIgnoreCase("Working") Then
					sIsVisible = "IsVisible = False"
				End If
			Else If sButtonName.XFEqualsIgnoreCase("Import") Then 
				If String.IsNullOrEmpty(ManpowerBUDTitle) Then
					sIsVisible = "IsVisible = False"
				ElseIf Not String.IsNullOrEmpty(ManpowerBUDTitle) And ManpowerBUDStatus.XFContainsIgnoreCase("Working") Then 
					sIsVisible = "IsVisible = True"
				ElseIf Not String.IsNullOrEmpty(ManpowerBUDTitle) And Not ManpowerBUDStatus.XFContainsIgnoreCase("Working")
					sIsVisible = "IsVisible = False"
				End If
			End If
			
			Return sIsVisible
		End Function
		
#End Region

#Region "ReadWriteManpowerCVs: ReadWriteManpowerCVs"
		Public Function ReadWriteManpowerCVs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sCubeView As String = args.NameValuePairs.XFGetValue("CubeView")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim ManpowerBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScript).DataCellEx.DataCellAnnotation
			If sCubeView.XFEqualsIgnoreCase("Text")
				If ManpowerBUDStatus.XFContainsIgnoreCase("Approved") Then
					sIsVisible = "ReadOnlyAnnotation"
				Else
					sIsVisible = "None"
				End If
			End If
			If sCubeView.XFEqualsIgnoreCase("Data")
				If ManpowerBUDStatus.XFContainsIgnoreCase("Approved") Then
					sIsVisible = "ReadOnlyData"
				Else
					sIsVisible = "None"
				End If
			End If
			
			Return sIsVisible
		End Function
		
#End Region

#Region "ShowHideManpowerRevert: ShowHideManpowerRevert"
		Public Function ShowHideManpowerRevert(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim ManpowerBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScript).DataCellEx.DataCellAnnotation
			If ManpowerBUDStatus.XFContainsIgnoreCase("Approved") Then
					sIsVisible = "IsVisible = True"
				Else
					sIsVisible = "IsVisible = False"
				End If
			
			Return sIsVisible
		End Function
		
#End Region

#Region "ShowHideManpowerSave: ShowHideManpowerSave"
		Public Function ShowHideManpowerSave(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim ManpowerBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScript).DataCellEx.DataCellAnnotation
			If ManpowerBUDStatus.XFContainsIgnoreCase("Approved") Then
					sIsVisible = "IsVisible = False"
				Else
					sIsVisible = "IsVisible = True"
				End If
			
			Return sIsVisible
		End Function
		
#End Region

'---Withhold---
#Region "ShowHideWithholdCreate: ShowHideWithholdCreate"
		Public Function ShowHideWithholdCreate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sButtonName As String = args.NameValuePairs.XFGetValue("ButtonName")
			Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sFlow As String = "REQ_00_WH_" & sAPPN
			Dim sIsVisible As String = ""
			Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim WithholdBUDTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleScr).DataCellEx.DataCellAnnotation
			Dim BUDStatusScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Rqmt_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim WithholdBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScr).DataCellEx.DataCellAnnotation
			If sButtonName.XFEqualsIgnoreCase("Create") Then
				If String.IsNullOrEmpty(WithholdBUDTitle) Then
					sIsVisible = "IsVisible = True"
				ElseIf Not String.IsNullOrEmpty(WithholdBUDTitle) And WithholdBUDStatus.XFContainsIgnoreCase("Working") Then
					sIsVisible = "IsVisible = False"
				ElseIf Not String.IsNullOrEmpty(WithholdBUDTitle) And Not WithholdBUDStatus.XFContainsIgnoreCase("Working") Then
					sIsVisible = "IsVisible = False"
				End If
			Else If sButtonName.XFEqualsIgnoreCase("Import") Then 
				If String.IsNullOrEmpty(WithholdBUDTitle) Then
					sIsVisible = "IsVisible = False"
				ElseIf Not String.IsNullOrEmpty(WithholdBUDTitle) And WithholdBUDStatus.XFContainsIgnoreCase("Working") Then 
					sIsVisible = "IsVisible = True"
				ElseIf Not String.IsNullOrEmpty(WithholdBUDTitle) And Not WithholdBUDStatus.XFContainsIgnoreCase("Working")
					sIsVisible = "IsVisible = False"
				End If
			End If
			
			Return sIsVisible
		End Function
		
#End Region

#Region "ShowHideWithholdSave: ShowHideWithholdSave"
		Public Function ShowHideWithholdSave(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)	
			Dim sIsVisible As String = ""
			Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"									
			Dim WithholdBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusScript).DataCellEx.DataCellAnnotation
			If WithholdBUDStatus.XFContainsIgnoreCase("Approved") Then
					sIsVisible = "IsVisible = False"
				Else
					sIsVisible = "IsVisible = True"
				End If
			
			Return sIsVisible
		End Function
		
#End Region


#Region "YOYLimitColorCode: YOYLimitColorCode"
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
If (CellAmount >= 2) AND (CellAmount < " & PrctLimit & ") Then 
    BackgroundColor = LightYellow, NumberFormat = [0.0\%] 
End if
If (CellAmount >= " & PrctLimit & ") Then 
    BackgroundColor = LightPink, NumberFormat = [0.0\%] 
End if"
		
	End Function
#End Region

#Region "GetMassUploadStatus: GetMassUploadStatus"
		Public Function GetMassUploadStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus","")
			
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region

#Region "Get Grid View SQL"
		Public Function GetGVSql(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim wfName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
   				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
				Dim sFundCenter As String = sEntity
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim Year1 As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim Year2 As Integer = Year1 + 1
'				Dim Year3 As Integer = Year2 + 1
'				Dim Year4 As Integer = Year3 + 1
'				Dim Year5 As Integer = Year4 + 1
				Dim account1 As String = "Phased_Obligation_Base"
				Dim account2 As String = "Phased_Commitment"
				Dim account3 As String = "Phased_Obligation_UFR"
				Dim account4 As String = "Phased_Commitment_UFR"
				Dim accountId1 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account1)
				Dim accountId2 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account2)
				Dim accountId3 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account3)
				Dim accountId4 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account4)
'				accountId3 = accountId1
'				accountId4 = accountId1
				If String.IsNullOrWhiteSpace(sEntity) Then Return " "
					
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = entityText3.Substring(1,1)
				Dim iCurrentLevel As Integer = CInt(currentStatus)
				Dim iNextLevel As Integer = iCurrentLevel - 1
				Dim currentWFLevel As String = "L" & iCurrentLevel
				Dim newWFLevel As String = "L" & iNextLevel
				
				'--------- derive FC List ----------
				sFundCenter = sFundCenter.ToLower.Replace("_general","")
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, wfCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				Dim entitySQL As String = ""				
				If isBase = True Then 
'					entitySQL = $"And ENTITY = '{sFundCenter}' "
					entitySQL = $"('{sFundCenter}')"				
				Else
					Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, "E#"& sFundCenter &".Base",True)
					Dim isFirst As Boolean = True
'					entitySQL = $"And ENTITY IN ("
					For Each FC As MemberInfo In LFundCenters						
						If isFirst Then
'							entitySQL &= $"'{FC.Member.Name}'"
							entitySQL &= $"('{FC.Member.Name}')"
						Else
'							entitySQL &= $",'{FC.Member.Name}'"
							entitySQL &= $", ('{FC.Member.Name}')"
						End If
						isFirst = False
					Next
'					entitySQL &= ")"
				End If
				
				'--------- derive status ----------
				Dim statusSQL As String = ""
				
				Select Case True
				Case wfName.XFContainsIgnoreCase("Formulate")
'					statusSQL = $"Where pd.Status IN ('{currentWFLevel} Imported','{currentWFLevel} Working','{currentWFLevel} Copied','{newWFLevel} Return to Formulator')"
					statusSQL = $"('{currentWFLevel} Imported'), ('{currentWFLevel} Working'), ('{currentWFLevel} Copied'), ('{newWFLevel} Return to Formulator')"
				Case wfName.XFContainsIgnoreCase("Review")
				End Select
				
				Dim SQL As New Text.StringBuilder
				SQL.AppendLine($"With ")
				SQL.AppendLine($"PivotedDetails As ( " )
				SQL.AppendLine($"	Select " )
				SQL.AppendLine($"	 	ENTITY," )
				SQL.AppendLine($"	 	FLOW," )
				SQL.AppendLine($"	 	SCENARIO," )
				SQL.AppendLine($"	 	MAX(Case When ACCOUNT = 'REQ_TITLE' THEN TEXT END) AS 'Requirement Title'," )
				SQL.AppendLine($"	 	MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) AS 'Status'" )
				SQL.AppendLine($"	From " )
				SQL.AppendLine($"	 	DATAATTACHMENT " )
				SQL.AppendLine($"	Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON DATAATTACHMENT.ENTITY = ENTITY_LIST.VALUE " )			
				SQL.AppendLine($"	Join (VALUES {statusSQL}) AS STATUS_LIST(VALUE) ON DATAATTACHMENT.ACCOUNT = 'REQ_RQMT_STATUS' and DATAATTACHMENT.TEXT = STATUS_LIST.VALUE " )
				SQL.AppendLine($"	Where " )
				SQL.AppendLine($"	 	CUBE = '{wfCube}' " )
'				SQL.AppendLine($"	 	And ENTITY = '{sEntity}' " )
'				If Not String.IsNullOrWhiteSpace(entitySQL) Then
'					SQL.AppendLine($"	 	{entitySQL} ")
'				End If 
				SQL.AppendLine($"	 	And SCENARIO = '{wfScenario}' " )
				SQL.AppendLine($"	Group By " )
				SQL.AppendLine($"		 ENTITY, FLOW, SCENARIO " )
				SQL.AppendLine($"), " ) 
'				SQL.AppendLine($"PivotedDetailsFiltered As ( ")
'				SQL.AppendLine($"	Select " )
'				SQL.AppendLine($"	 	ENTITY," )
'				SQL.AppendLine($"	 	FLOW," )
'				SQL.AppendLine($"	 	SCENARIO," )
'				SQL.AppendLine($"	 	'Requirement Title' AS 'Requirement Title'," )
'				SQL.AppendLine($"	 	Status" )
'				SQL.AppendLine($"	From " )
'				SQL.AppendLine($"	 	PivotedDetails " )
'				SQL.AppendLine($"	Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON PivotedDetails.ENTITY = ENTITY_LIST.VALUE " )
'				SQL.AppendLine($"	Join (VALUES {statusSQL}) AS STATUS_LIST(VALUE) ON PivotedDetails.Status = STATUS_LIST.VALUE " )
'				SQL.AppendLine($"), " )
				SQL.AppendLine($"ScenarioID As ( ")
				SQL.AppendLine($"    Select MEMBERID ")
				SQL.AppendLine($"    From MEMBER ")
				SQL.AppendLine($"    Where NAME = '{wfScenario}' AND DIMTYPEID = 2 ")
				SQL.AppendLine($"), ")
				SQL.AppendLine($"FilteredDataRecords As ( ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account1}_{Year1}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year1} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId1} ") 
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account2}_{Year1}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year1} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId2} ")
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account3}_{Year1}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year1} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId3} ")
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account4}_{Year1}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year1} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId4} ")
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account1}_{Year2}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year2} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId1} ") 
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account2}_{Year2}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year2} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId2} ")
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account3}_{Year2}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year2} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId3} ")
				SQL.AppendLine($"    UNION ALL ")
				SQL.AppendLine($"    Select ENTITYID, FLOWID, UD1ID, UD2ID, UD3ID, UD5ID, UD6ID, M12VALUE, '{account4}_{Year2}' AS Type, SCENARIOID ")
				SQL.AppendLine($"    From DATARECORD{Year2} ")
				SQL.AppendLine($"    Where SCENARIOID = (Select MEMBERID From ScenarioID) and ACCOUNTID = {accountId4} ")
				SQL.AppendLine($") ")
				SQL.AppendLine($"Select ")
				SQL.AppendLine($"    CONCAT(pd.ENTITY, ':', pd.FLOW) AS ENTITYFLOW, ")
				SQL.AppendLine($"    pd.ENTITY, ")
				SQL.AppendLine($"    pd.FLOW, ")
'				SQL.AppendLine($"    pd.SCENARIO, ")
				SQL.AppendLine($"    m_ud1.NAME As APPN, ")
				SQL.AppendLine($"    m_ud2.NAME As MDEP, ")
				SQL.AppendLine($"    m_ud3.NAME As 'APE(9)', ")
				SQL.AppendLine($"    m_ud5.NAME As CType, ")
				SQL.AppendLine($"    m_ud6.NAME As 'Commitment Item', ")
				SQL.AppendLine($"    pd.[Requirement Title], ")
				SQL.AppendLine($"    pd.Status, ")
				SQL.AppendLine($"    MAX(Case When Type = '{account1}_{Year1}' THEN M12VALUE END) AS '{Year1}_{account1}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account2}_{Year1}' THEN M12VALUE END) AS '{Year1}_{account2}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account3}_{Year1}' THEN M12VALUE END) AS '{Year1}_{account3}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account4}_{Year1}' THEN M12VALUE END) AS '{Year1}_{account4}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account1}_{Year2}' THEN M12VALUE END) AS 'CarryOver_{account1}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account2}_{Year2}' THEN M12VALUE END) AS 'CarryOver_{account2}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account3}_{Year2}' THEN M12VALUE END) AS 'CarryOver_{account3}', ")
				SQL.AppendLine($"    MAX(Case When Type = '{account4}_{Year2}' THEN M12VALUE END) AS 'CarryOver_{account4}' ")
				SQL.AppendLine($"From ")
				SQL.AppendLine($"    PivotedDetails pd ")
				SQL.AppendLine($"LEFT Join MEMBER m_Entity On pd.ENTITY= m_ENTITY.NAME ")
				SQL.AppendLine($"LEFT Join MEMBER m_Scenario On pd.SCENARIO = m_Scenario.NAME ")
				SQL.AppendLine($"LEFT Join MEMBER m_Flow On pd.FLOW= m_FLOW.NAME ")
				SQL.AppendLine($"LEFT Join FilteredDataRecords A On m_ENTITY.memberID= A.ENTITYID And m_FLOW.memberID= A.FLOWID And A.SCENARIOID = m_Scenario.memberID ")
				SQL.AppendLine($"LEFT Join MEMBER m_ud1 On A.UD1ID = m_ud1.memberId ")
				SQL.AppendLine($"LEFT Join MEMBER m_ud2 On A.UD2ID = m_ud2.memberId ")
				SQL.AppendLine($"LEFT Join MEMBER m_ud3 On A.UD3ID = m_ud3.memberId ")
				SQL.AppendLine($"LEFT Join MEMBER m_ud5 On A.UD5ID = m_ud5.memberId ")
				SQL.AppendLine($"LEFT Join MEMBER m_ud6 On A.UD6ID = m_ud6.memberId ")				
				
'				If Not String.IsNullOrWhiteSpace(statusSQL) Then
'					SQL.AppendLine($"	 	{statusSQL} " )
'				End If
				
				SQL.AppendLine($"Group By ")
				SQL.AppendLine($"    pd.ENTITY, ")
				SQL.AppendLine($"    pd.FLOW, ")
				SQL.AppendLine($"    m_Flow.memberId, ")
'				SQL.AppendLine($"    pd.SCENARIO, ")
				SQL.AppendLine($"    m_ud1.NAME, ")
				SQL.AppendLine($"    m_ud2.NAME, ")
				SQL.AppendLine($"    m_ud3.NAME, ")
				SQL.AppendLine($"    m_ud5.NAME, ")
				SQL.AppendLine($"    m_ud6.NAME, ")
				SQL.AppendLine($"    pd.[Requirement Title], ")
				SQL.AppendLine($"    pd.Status  ")
				SQL.AppendLine($"Order By ")
				SQL.AppendLine($"    m_Flow.memberId ")
'				SQL.AppendLine($") ")
				
'BRApi.ErrorLog.LogMessage(si,$"SQL: {sql}")					
				Return sql
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region

#Region "GetTGTScenario: Get TGT Scenario"
		Public Function GetTGTScenario(ByVal si As SessionInfo, args As DashboardStringFunctionArgs) As String
		Try
   				Return Nothing
			
			Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			
		End Try
	End Function
		
#End Region

#Region "getBUDAnnotationFormulateReq: Get BUD Annotation Function for formulate Req"
	'Get BUD Annotation tbx or lbl based on U8
	Public Function getBUDAnnotationFormulateReq(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs, ByVal sU8 As String, ByVal sMode As String) As String
		Try
			Dim sIsVisible As String = ""
			If sU8.XFEqualsIgnoreCase("ReadOnlyAnnotation") And sMode = "Lbl"
			sIsvisible = "True"
		Else If sU8.XFEqualsIgnoreCase("ReadOnlyAnnotation") And sMode = "Tbx"
			sIsvisible = "False"
		End If
			 If Not sU8.XFEqualsIgnoreCase("ReadOnlyAnnotation") And sMode = "Tbx"
			sIsvisible = "True"
			Else If Not sU8.XFEqualsIgnoreCase("ReadOnlyAnnotation") And sMode = "Lbl"
			sIsvisible = "False"
		End If 
		'Brapi.ErrorLog.LogMessage(si, "U8" & sU8 & "Mode" & sMode)
		Return sIsVisible
		
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
#End Region

#Region "Helper Functions"

	Private Function GetFDXCascadingMbrFilter() As String
		Dim workspaceName As String = "50 CMD SPLN"
		Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,"50 CMD SPLN")
		Dim cubeViewName As String = "CMD_PGM_cPROBE_FDX_CV"
		Dim entityDimName = "E_Army"
		Dim scenarioDimName = "S_Army"
		Dim timeMemFilter = "T_Army"
		Dim dt As DataTable = BRApi.Import.Data.FdxExecuteCubeView(si, workspaceID, cubeViewName, entityDimName, entityMemFilter, scenarioDimName, scenarioMemFilter, timeMemFilter, nvbParams, includeCellTextCols, useStandardFactTableFields, filter, parallelQueryCount, logStatistics)
		
	End Function
	
	Public Function GetFDXFilteredRows() As String
		Dim FilterString As String
		If String.IsNullOrWhiteSpace(FilterString) Then
			FilterString = $"Cb#ARMY:C#Local:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		Else
			FilterString = $"{FilterString} + Cb#ARMY:C#Aggregated:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"		
		End If
		
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U6#None,U6#Pay_Benefits.Children,U6#Non_Pay.Children,U6#Obj_Class.Children])")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

			'Add Total row
			toSort.Add($"E#{sEntity}:U1#{U1}:U3#{U3}:U6#Top:Name(Total)",$"E#{sEntity}:U6#Top")

			For Each msb In results.Keys
			   msb.Scenario = vbNullString
			   msb.Entity =  sEntity
			   msb.Account = vbNullString
			   msb.Origin = vbNullString
			   msb.IC = vbNullString
			   msb.Flow = vbNullString		
			   msb.UD2 = vbNullString
			   msb.UD4 = vbNullString
			   msb.UD5 = vbNullString
			   msb.UD7 = vbNullString   
				If Not toSort.ContainsKey(msb.GetMemberScript)
					toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}")
				End If
			Next
		End If	
		
	End Function

#End Region


	End Class
End Namespace
