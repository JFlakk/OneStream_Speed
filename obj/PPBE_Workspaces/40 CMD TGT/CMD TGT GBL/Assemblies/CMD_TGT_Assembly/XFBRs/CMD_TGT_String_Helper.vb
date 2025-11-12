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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

'TGT_String_Helper
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_TGT_String_Helper
	Public Class MainClass
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try			
	            Me.si = si
	            Me.globals = globals
	            Me.api = api
	            Me.args = args
'This will define the wf profile's command level i.e., CMD vs SubCMD from which the code is executed
'This will be used primarily for case statements
				Dim sWFProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCMDLevel As String = String.Empty
				If sWFProfileName.XFContainsIgnoreCase("CMD")
					wfCMDLevel = "CMD"
				Else
					wfCMDLevel = "SubCMD"
				End If
				
				Select Case args.FunctionName.ToLower()
					Case "gettgtreviewdist"
						Return Me.GetTGTReviewDist()
					Case "returnentity"
						Return Me.ReturnEntityList()
					Case "returnmodifyflow"
						Return Me.ReturnModifyFlow()
					Case "returnmodifyorigin"
						Return Me.ReturnModifyOrigin()
					Case "returnentctrlacct"
						Return Me.ReturnEntCtrlAcct()
					Case "getcascadingmbrfilter"
						Return Me.GetCascadingMbrFilter()
					Case "getpaynonpay"
						Return Me.GetPayNonPay()
					Case "getentflow"
						Return Me.GetEntFlow()
				End Select				

#Region "GetProfileEntity"
'Called By CMD_TGT_SelectTopLineTargetsInitDistrVsSpendPlan
				If args.FunctionName.XFEqualsIgnoreCase("GetProfileEntity") Then
					Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					
					Dim sEntity As String = wfProfileName.Split(" ")(0).Trim
					If wfCube.XFContainsIgnoreCase("USAREUR") Then 
						sEntity = sEntity & "_AF"
					End If 
'BRapi.ErrorLog.LogMessage(si, "GetProfileEntity:  sEntity=" & sEntity)				
					Return sEntity
						
				End If

#End Region

#Region "CopySourceU1"
'Called By CMD_TGT_SelectTopLineTargetsInitDistrVsSpendPlan
				If args.FunctionName.XFEqualsIgnoreCase("CopySourceU1") Then
					Dim sU1 As String = args.NameValuePairs.XFGetValue("U1")
					Dim sScenario As String = args.NameValuePairs.XFGetValue("SourcePosition")
					
					If sScenario.XFContainsIgnoreCase("cProbe")
						If sU1.XFEqualsIgnoreCase("Appropriation") Then Return "[Appropriation.Descendants.Where(Text3 Contains L2).Where(Name DoesNotContain _General)]"
						Return $"{sU1}"
					Else
						Return $"{sU1}.Base"
					End If
				End If

#End Region

#Region "CopySourceAccount"
'Called By CMD_TGT_SelectTopLineTargetsInitDistrVsSpendPlan
				If args.FunctionName.XFEqualsIgnoreCase("CopySourceAccount") Then
					Dim sSource As String = args.NameValuePairs.XFGetValue("SourcePosition")
		
					If sSource.XFContainsIgnoreCase("cProbe") Then
						Return "BO1"
					Else
						Return "Total_Appr_Distr_Budget"
					End If
				End If
#End Region

#Region "CopySourceScenario"
'Called By CMD_TGT_SelectTopLineTargetsInitDistrVsSpendPlan
				If args.FunctionName.XFEqualsIgnoreCase("CopySourceScenario") Then
					Dim sWFTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim sSource As String = args.NameValuePairs.XFGetValue("SourcePosition")
					Dim sPBScenario As String = args.NameValuePairs.XFGetValue("PBScenario")
		
					If sSource.XFContainsIgnoreCase("cProbe") Then
						If String.IsNullOrWhiteSpace(sPBScenario) Then
							Return "None"
						End If	
							Return sPBScenario
					Else
						Return "Initial_Distribution_FY" & sWFTime
					End If
				End If
#End Region

#Region "CopySourceAccountScenario"
'Called By CMD_TGT_SelectTopLineTargetsInitDistrVsSpendPlan
				If args.FunctionName.XFEqualsIgnoreCase("CopySourceAccountScenario") Then
						Dim sWFTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
						Dim sSource As String = args.NameValuePairs.XFGetValue("SourcePosition")
						Dim sPBScenario As String = args.NameValuePairs.XFGetValue("PBScenario")
		
						If sSource.XFContainsIgnoreCase("cProbe") Then
							If String.IsNullOrWhiteSpace(sPBScenario) Then
								Return "S#None" & ":A#BO1:Name(Copy From cPROBE)"
							End If	
							Return "S#" & sPBScenario & ":A#BO1:Name(Copy From cPROBE)"
						Else
							Return "S#Initial_Distribution_FY" & sWFTime & ":A#Total_Appr_Distr_Budget:Name(Copy From Initial Distribution)"
						End If
				End If
#End Region

#Region "TargetLockCheck"
				If args.FunctionName.XFEqualsIgnoreCase("TargetLockCheck") Then
					Return Me.TargetLockCheck(si,api,args)
				End If
#End Region





'++++++++++
				
#Region "Get Target Amount Account"		
				'Return correct distribution target account for parent vs base FC 
				If args.FunctionName.XFEqualsIgnoreCase("GetTgtAmtAcct") Then
					Dim sFC As String = args.NameValuePairs.XFGetValue("FC","")
					Dim sAccount As String = ""
					If String.IsNullOrWhiteSpace(sFC) Then Return Nothing
					If sFC.XFContainsIgnoreCase("_general") Then
						sAccount = "Distr_Target_General"
					Else
						sAccount =  "Distr_Target"
					End If
'BRApi.ErrorLog.LogMessage(si, $"{BRName}.{args.FunctionName} - sAccount = {sAccount}")
					Return sAccount
				End If
#End Region 

#Region "Remove General from FC"		
				'Return correct distribution target account for parent vs base FC 
				If args.FunctionName.XFEqualsIgnoreCase("RemoveGeneralFromFC") Then
					Dim sFC As String = args.NameValuePairs.XFGetValue("FC","").ToLower.Replace("_general","").Trim()
					Dim sExpansion As String = args.NameValuePairs.XFGetValue("Expansion","")
					If String.IsNullOrWhiteSpace(sFC) Then Return Nothing
					sFC = sFC & sExpansion
'BRApi.ErrorLog.LogMessage(si, $"{BRName}.{args.FunctionName} - sFC = {sFC}")
					Return sFC
				End If
#End Region 

#Region "Display CMD Rows or Columns"
' Example: IsColumnVisible = XFBR(zMH_TGT_String_Helper,DisplayCmdRows,IsCMD=True)

				If args.FunctionName.XFEqualsIgnoreCase("DisplayCmdRows") Then
					Dim currentProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
					Dim sWFProfile As String = currentProfile.Name
					Dim sIsCMD As String = args.NameValuePairs.XFGetValue("IsCMD")
'Brapi.ErrorLog.LogMessage(si, "sIsCMD = " & sIsCMD)
					
						If (sWFProfile.Contains("CMD")) And (sIsCMD = "True") Then
								Return "True"
									
							Else If (Not sWFProfile.Contains("CMD")) And (sIsCMD = "False")Then
								Return "True"
							Else
								Return "False"
						End If
						
				End If
#End Region

#Region "Get Mass Upload Status"
			
			If args.FunctionName.XFEqualsIgnoreCase("GetImportStatus") Then
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus","")
			End If
			
#End Region

#Region "ShowHideScenario"
		If args.FunctionName.XFEqualsIgnoreCase("ShowHideScenario") Then
				Dim sourceType As String = args.NameValuePairs.XFGetValue("sourceType").ToUpper
				Dim visible As String = ""
					If sourceType.XFContainsIgnoreCase("cPROBE") Then
						visible = "True"
					Else
						visible = "False"
					End If	
'BRapi.ErrorLog.LogMessage(si, "sourceType=" & visible)						
				Return visible
			End If
#End Region

#Region "ShowHidePublishSPLButtonMain"
		If args.FunctionName.XFEqualsIgnoreCase("ShowHidePublishSPLButtonMain") Then
		        Dim btnType As String = args.NameValuePairs.XFGetValue("btnType").ToUpper
				Dim visibleBTN As String = ""
					If btnType.XFContainsIgnoreCase("Initial") Then
						visibleBTN = "True"
					Else
						visibleBTN = "False"
					End If	
'BRapi.ErrorLog.LogMessage(si, "btuoone=" & visible)						
				Return visibleBTN
			End If
#End Region

#Region "Show Hide Component By FC Lock Status"
		If args.FunctionName.XFEqualsIgnoreCase("ShowHideComponentByFCLockStatus") Then
		        Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "")	
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"	
				'Check TGT_Target_Distribution_Complete_Ind
				Dim indTargetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTimeM12 & ":V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim indSourceValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, indTargetMbrScript).DataCellEx.DataCellAnnotation
				Dim isVisible As Boolean = True
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode", "")
				Select Case sMode
				Case "ShowWhenLocked"
					If indSourceValue.XFContainsIgnoreCase("yes") Then
						isVisible = True
					Else
						isVisible = False
					End If
				Case Else
					If indSourceValue.XFContainsIgnoreCase("yes") Then isVisible = False
				End Select
				Return isVisible
			End If
#End Region

#Region "Display Copy and Phase withholds button"
' Example: IsColumnVisible = XFBR(TGT_String_Helper,Displaycpywthbtn)

				If args.FunctionName.XFEqualsIgnoreCase("Displaycpywthbtn") Then
					Dim currentProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
					Dim sWFProfile As String = currentProfile.Name
					'Dim sIsCMD As String = args.NameValuePairs.XFGetValue("IsCMD")
				
						If (sWFProfile.Contains("CMD"))  Then
								Return "True"
							Else
								Return "False"
						End If
					End If
#End Region

#Region "DisplayWFComponentsCMDDST: Display Lock Unlock WF Components CMD/FC"
				If args.FunctionName.XFEqualsIgnoreCase("DisplayWFComponentsCMDDST") Then
					Return Me.DisplayWFComponentsCMDDST(si, globals, api, args)
				End If
#End Region

				
#Region "Get Validation Status"
			
			If args.FunctionName.XFEqualsIgnoreCase("GetValidationStatusMsg") Then
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "ValidationStatusMsg", "ValidationStatusMsg","")
			End If
			
#End Region

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "GetCascadingMbrFilter"
		Public Function GetCascadingMbrFilter() As String
			Try
				' Read incoming args
				Dim cmd As String = args.NameValuePairs.XFGetValue("cmd", "NA")
				Dim entity As String = args.NameValuePairs.XFGetValue("entity", "NA")
				Dim appn As String = args.NameValuePairs.XFGetValue("appn", String.Empty)
				Dim mdep As String = args.NameValuePairs.XFGetValue("mdep", "NA")
				Dim sag As String = args.NameValuePairs.XFGetValue("sag", "NA")
				Dim ape As String = args.NameValuePairs.XFGetValue("ape", "NA")
				Dim dollarType As String = args.NameValuePairs.XFGetValue("dollarType", "NA")
				Dim status As String = args.NameValuePairs.XFGetValue("status", "NA")
				Dim returnType As String = args.NameValuePairs.XFGetValue("returnType", "NA")
				Dim cvName As String = args.NameValuePairs.XFGetValue("cvName", "NA") '"CMD_PGM_cPROBE_FDX_CV"

				' Build a compact signature for Entity + Appn only
				Dim currRebuildparams As String = String.Concat(cmd, "|", entity, "|", appn, "|", status)
				
				' Use workspace session settings to persist last seen signatures per user/workspace
				Dim cacheCat As String = $"CMD_TGT_CascadingFilterCache_{cvName}"
				Dim filterDTparams As String = $"CMD_TGT_FilterDTparams_{cvName}"
				Dim rebuildparams As String = "CMD_TGT_rebuildparams_{cvName}"

				Dim prevRebuildParams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, "")
				Dim needsRebuild As Boolean = Not String.Equals(prevRebuildParams, currRebuildparams, StringComparison.Ordinal)

				Dim dt As New DataTable 
BRApi.Errorlog.LogMessage(si,$"Hit here - {entity} - {returnType} - {appn} - {Cmd} - {needsRebuild}")

				If needsRebuild Then
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, currRebuildparams)
					If cvName.Substring(0,7) = "CMD_TGT"
						BRApi.Errorlog.LogMessage(si,$"Hit This")
						dt = GetFDXCascadingMbrFilter(cvName,entity,appn)
					Else
						BRApi.Errorlog.LogMessage(si,$"Hit Else")
						dt = GetFDXCascadingMbrFilter(cvName,Cmd,appn)
					End If
				Else
					dt = BRApi.Utilities.GetSessionDataTable(si, si.UserName, "CMD_TGT_CascadingFilter")
				End If
				
				Dim returnTypeMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
					{"APPN", "UD1"},
					{"MDEP", "UD2"},
					{"SAG",  "UD3"},
					{"APE",  "UD3"},
					{"DollarType",  "UD4"}
				}

				' Determine which physical column (if any) corresponds to the requested returnType
				Dim selectedColumn = String.Empty
				selectedColumn = returnTypeMap.XFGetValue(returnType,"Not Found")
				If Not dt Is Nothing Then

					' Build a deterministic signature of the inputs (full)
					Dim currFilterDTparams As String = String.Concat(entity, "|", appn, "|", status, "|", returnType, "|", mdep, "|", sag, "|", ape, "|", dollarType)
	
					Dim prevFilterDTparams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, filterDTparams, "")
	
	
					Dim filterParts As New List(Of String)
	
					' Only add filters when parameter is provided and not "NA"
					If Not String.IsNullOrWhiteSpace(mdep) AndAlso Not mdep.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD2] = '" & mdep.Replace("'", "''") & "'")
					End If
	
					If Not String.IsNullOrWhiteSpace(ape) AndAlso Not ape.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD3] = '" & ape.Replace("'", "''") & "'")
					End If
	
					If Not String.IsNullOrWhiteSpace(dollarType) AndAlso Not dollarType.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD4] = '" & dollarType.Replace("'", "''") & "'")
					End If
	
	
					Dim filterExpr As String = If(filterParts.Count > 0, String.Join(" AND ", filterParts), String.Empty)
					' Filter dt into a DataTable so it can be converted to a DataView
					Dim filteredDt As DataTable
					If String.IsNullOrEmpty(filterExpr) Then
						filteredDt = dt.Copy()
					Else
						filteredDt = dt.Clone()
						Dim selectedRows() As DataRow = dt.Select(filterExpr)
						For Each row As DataRow In selectedRows
							filteredDt.ImportRow(row)
						Next
					End If
					BRApi.Utilities.SetSessionDataTable(si, si.UserName, "CMD_TGT_CascadingFilter",filteredDt)
	
					Dim dv As DataView = New DataView(filteredDt)
					' Map returnType values to column keys (case-insensitive)

					If selectedColumn <> "Not Found"
	
						dv.RowFilter = $"{selectedColumn} IS NOT NULL AND {selectedColumn} <> ''"
						dv.Sort = selectedColumn & " ASC"
						Dim mlDT = dv.ToTable(True, selectedColumn) ' Distinct values only
		
						Dim result As String = String.Empty
						For Each dr As DataRow In mlDT.Rows
							Dim val As String = dr(selectedColumn).ToString()
							If Not String.IsNullOrWhiteSpace(val) Then
								If String.IsNullOrEmpty(result) Then
									result = $"{selectedColumn}#{val}"
								Else
									result &= $",{selectedColumn}#{val}"
								End If
							End If
						Next

						Return result
					Else
						Return String.Empty
					End If
				Else
'BRapi.ErrorLog.LogMessage(si,"Hit Empty")
					Return $"{selectedColumn}#None"
				End If					

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Updated 10/16/2025
		
#Region "Helper Functions"
	
		Private Function GetFDXCascadingMbrFilter(ByVal cvName As String,ByVal entFilter As String,ByVal appn As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "00 GBL"
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim CprobeScen As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetSrccPROBEScen(si,wfInfoDetails("ScenarioName"))

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{CprobeScen}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
'			If appn = String.Empty
'				appn = "OMA"
'			End If
			NameValuePairs.Add("ML_GBL_APPN",appn)
			
			If cvName.Substring(0,7) = "CMD_TGT"
				wsName = "40 CMD TGT"
				scenFilter = $"S#{wfInfoDetails("ScenarioName")}"
				NameValuePairs.Remove("ML_GBL_APPN")
			End If
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 1, False)
If dt Is Nothing
	BRAPI.ErrorLog.LogMessage(si,$"Hit NOthing - {wfInfoDetails("ScenarioName")} - {CprobeScen}")
End If
			Return dt
		End Function
#End Region 'Updated 10/16/2025

#Region "Get Pay/Non-Pay"
		Public Function GetPayNonPay() As String
			Dim PayNonPayType As String = args.NameValuePairs.XFGetValue("PayNonPayType","NA")
			Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
			

			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			If Entity = wfInfoDetails("CMDName")
				Dim mbrInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimType.Entity.Id, Entity)
				Entity = BRApi.Finance.Entity.Text(si, mbrInfo.Member.MemberId, 1, DimConstants.Unknown, DimConstants.Unknown)
			End If
			Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,Entity,wfInfoDetails("TimeName"))
			If Val_Approach("CMD_Val_Pay_NonPay_Approach") = "Yes"
				If PayNonPayType = "cPROBEBO5"
					Return "A#BO5"
				ElseIf PayNonPayType = "TopLineAdj" Or PayNonPayType = "ModifyTGTDistWH" Or PayNonPayType = "NewTGTDist" Or PayNonPayType = "NewWH" Then
					Return "U6#CostCat,U6#Pay_Benefits,U6#Non_Pay"
				End If
			Else
				If PayNonPayType = "cPROBEBO5" 
					Return "A#NA"
				ElseIf (PayNonPayType = "TopLineAdj" Or PayNonPayType = "ModifyTGTDistWH" Or PayNonPayType = "NewTGTDist" Or PayNonPayType = "NewWH") Then
					Return "U6#CostCat_General"
				End If
			End If
			
			
		End Function


#End Region

#Region "Get Entity Flow"
		Public Function GetEntFlow() As String
			Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
			Dim newRowType As String = args.NameValuePairs.XFGetValue("newRowType","NA")
			Dim entityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Entity)
			If newRowType = "WH"
				Return $"{entityLevel}_Dist_Final"
			Else
				Return $"{entityLevel}_Ctrl_Intermediate"
			End If
			
			
		End Function


#End Region
#Region "TargetLockCheck"
		Private Function TargetLockCheck(ByVal si As SessionInfo, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			'Command: XFBR({TGT_String_Helper}{TargetLockCheck}{argsPath=CMDXYZ})
			'SubCommand: XFBR({TGT_String_Helper}{TargetLockCheck}{argsPath=subCMDXYZ, subCommandEntity=|!prompt_cbx_TGTDST_TGTADM_0CaAa_D_FundCenter__ManageAccessSubCMD!|})	
'brapi.ErrorLog.LogMessage(si,$"Inside TargetLock")				
			'Dim argsPath As String = args.NameValuePairs.XFGetValue("argsPath")
			Dim wfCube As String = args.SubstVarSourceInfo.WFCube
			Dim wfScenario As String = args.SubstVarSourceInfo.WFScenario.Name
			Dim wfTime As String = args.SubstVarSourceInfo.WFTime.Name
			Dim sWFProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCMDLevel As String = String.Empty
			If sWFProfileName.XFContainsIgnoreCase("CMD")
				wfCMDLevel = "CMD"
			Else
				wfCMDLevel = "SubCMD"
			End If
			Dim isSelf As Boolean = args.NameValuePairs.XFGetValue("Self",False)
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
			Dim commandEntity As String = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk , $"E#{wfCube}.Descendants.Where(HasChildren = True).Where(Text3 Contains EntityLevel=L2)", True,,)(0).Member.Name
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim commandBalanceScript As String = "E#" & commandEntity & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim commandBalanceStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, commandBalanceScript).DataCellEx.DataCellAnnotation
			Dim subCommandBalanceScript As String = "E#" & sEntity & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim subCommandBalanceStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, subCommandBalanceScript).DataCellEx.DataCellAnnotation

			
'brapi.ErrorLog.LogMessage(si,$"commandEntity {commandEntity}")		

			Select Case wfCMDLevel	
			Case "CMD"
			If isSelf 
				If commandBalanceStatus = "Y" Then					
					Return "E#" & commandEntity & "_General:Name( )"
				Else					
					Return "E#" & commandEntity & "_General:U8#ReadOnlyAnnotation:Name( )"
				End If
				
			Else
				Dim returnEntityList As String = "E#" & wfCube & ".Descendants.Where(HasChildren = True).Where(Text3 Contains EntityLevel=L3)"
				Dim entityList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, returnEntityList, True)
				Dim returnScript As String = ""
				For Each entityMember In entityList
'					Dim balanceScript As String = "E#" & entityMember.Member.Name & ":C#Local:S#" & wfScenario &":T#" & wfTime & "M12:V#YTD:A#Ctrl_Target_Transfer_Variance:F#Baseline:O#BeforeAdj:I#None:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
'					Dim balanceAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,balanceScript).DataCellEx.DataCell.CellAmount
					Dim balanceStatusScript As String = "E#" & entityMember.Member.Name & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim balanceStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, balanceStatusScript).DataCellEx.DataCellAnnotation
					Dim annotationScript As String = "E#" & entityMember.Member.name & "_General:C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim annotation As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "ARMY", annotationScript).DataCellEx.DataCellAnnotation
					
					If Not String.IsNullOrWhiteSpace(returnScript) Then returnScript &= ","
					
					If balanceStatus = "Y" Or annotation = "Yes" Then
						returnScript &= "E#" & entityMember.Member.Name & "_General"
					Else
						returnScript &= "E#" & entityMember.Member.Name & "_General" & ":U8#ReadOnlyAnnotation"
					End If
'BRApi.ErrorLog.LogMessage(si,$"entity = {entityMember.Member.Name} || balanceStatus = {balanceStatus} || returnScript = {returnScript}")					
				Next
				'Return nothing
				If returnScript = "" Then returnScript = "E#" & wfCube & ".Descendants.Where(Name Contains _General).Where(Text3 Contains EntityLevel=L10)"
				Return returnScript				
			End If
				
			Case "SubCMD"			
				If isSelf			
					If String.IsNullOrWhiteSpace(sEntity) Then Return "E#None"
					Dim lockScript As String = "E#" & sEntity & "_General:C#USD:S#" & wfScenario & ":T#" & wfTime & "M12:V#YTD:A#TGT_Target_Distribution_Complete_Ind:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim lockAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,lockScript).DataCellEx.DataCell.CellAmount
'BRApi.ErrorLog.LogMessage(si,$"sEntity = {sEntity} || subCommandBalanceStatus = {subCommandBalanceStatus}")				
					If subCommandBalanceStatus = "Y" And lockAmt <> 1 Then
						Return "E#" & sEntity & ":Name( )"
					Else
						Return "E#" & sEntity & ":U8#ReadOnlyAnnotation:Name( )"
					End If
					
				Else 					
					If String.IsNullOrWhiteSpace(sEntity) Then Return "E#None"
					Dim returnEntityList As String = "E#" & sEntity & ".Descendants.Where(HasChildren = True).Where(Text3 Contains EntityLevel=L4)"
					Dim entityList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, returnEntityList, True)
					Dim returnScript As String = ""
					For Each entityMember In entityList
	'					Dim balanceScript As String = "E#" & entityMember.Member.Name & ":C#Local:S#" & wfScenario &":T#" & wfTime & "M12:V#YTD:A#Ctrl_Target_Transfer_Variance:F#Baseline:O#BeforeAdj:I#None:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
	'					Dim balanceAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfCube,balanceScript).DataCellEx.DataCell.CellAmount
						Dim balanceStatusScript As String = "E#" & entityMember.Member.Name & ":C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Balanced_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim balanceStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, balanceStatusScript).DataCellEx.DataCellAnnotation
						
						Dim annotationScript As String = "E#" & entityMember.Member.name & ":C#Local:S#" & wfScenario & ":T#" & wfTime & "M12:V#Annotation:A#TGT_Target_Distribution_Complete_Ind:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim annotation As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "ARMY", annotationScript).DataCellEx.DataCellAnnotation
						
						If returnScript <> "" Then returnScript &= ","
						If balanceStatus = "Y" Or annotation = "Yes" Then
							returnScript &= "E#" & entityMember.Member.Name
						Else
							returnScript &= "E#" & entityMember.Member.Name & ":U8#ReadOnlyAnnotation"
						End If
					Next
					'Return nothing
					If returnScript = "" Then returnScript = "E#" & sEntity & ".Descendants.Where(Name Contains _General).Where(Text3 Contains EntityLevel=L10)"
					Return returnScript
				End If
				
			End Select
			Return Nothing
		End Function
#End Region

#Region "DisplayWFComponentsCMDDST: Display Complete or Revert WF Steps Components"
	Public Function DisplayWFComponentsCMDDST(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
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

		Public Function GetTGTReviewDist() As String
			
		End Function
		
		Public Function ReturnEntityList() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function
		
		
		Public Function ReturnModifyFlow() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function
		
		Public Function ReturnModifyOrigin() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function
		
		Public Function ReturnModifyRows() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function
		
		Public Function ReturnEntCtrlAcct() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function
		
		Public Function ReturnEntDistAcct() As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
			Dim sType As String = args.NameValuePairs.XFGetValue("Type","")
			Dim sReturnValue As String = String.Empty
				
			'Return nothing if no enity is selected from the Funds Center cbx
			If String.IsNullOrEmpty(sEntity)
				sReturnValue = $"E#None"
				Return sReturnValue
			Else	
				Dim mbr As Member = BRApi.Finance.Members.GetMember(si, dimType.Entity.Id, sEntity)
				Dim MbrName = BRApi.Finance.Entity.Text(si, Mbr.MemberId, 1, SharedConstants.Unknown, SharedConstants.Unknown)

				If sType.XFEqualsIgnoreCase("CMD_Member") Then
					Return $"E#{MbrName}:O#AdjInput"
				Else If sType.XFEqualsIgnoreCase("CMD_Member_Children") Then
					Return $"E#{MbrName}.Children:O#AdjInput"
				End If
					
					
'			End Select						
			
			Return sReturnValue				
			End If
			
		End Function

#Region "Constants"
	Public BRName = "TGT_String_Helper" 
#End Region
	End Class
End Namespace
