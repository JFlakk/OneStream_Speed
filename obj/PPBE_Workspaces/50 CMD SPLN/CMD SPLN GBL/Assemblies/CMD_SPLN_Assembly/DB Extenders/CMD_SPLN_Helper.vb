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
Imports System.Text.RegularExpressions

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_SPLN_Helper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
						Select Case args.FunctionName
							Case args.FunctionName.XFEqualsIgnoreCase("ApprovalBUDsVal")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
								If Not Me.IsBUDValidationAllowed(si, args) Then
									dbExt_ChangedResult.ShowMessageBox = True
									dbExt_ChangedResult.Message = "Cannot approve Requirement at this time. Contact Budget manager."
									Return dbExt_ChangedResult
								End If
								Return Me.ApprovalBUDsVal()
							Case args.FunctionName.XFEqualsIgnoreCase("AttachDocument")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If dbExt_ChangedResult.ShowMessageBox = True Then
									Return dbExt_ChangedResult
								End If
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
							'Dim reqShared As String = "BUD_Shared"
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

						Case args.FunctionName.XFEqualsIgnoreCase("Check_WF_Complete_Lock")
							Dim dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
							If dbExt_ChangedResult.ShowMessageBox = True Then
								Return dbExt_ChangedResult
							End If
						Case args.FunctionName.XFEqualsIgnoreCase("CheckWFCompleteLockAndAllowBUDCreation")
							Me.CheckWFCompleteLockAndAllowBUDCreation(si, globals, api, args)
						Case args.FunctionName.XFEqualsIgnoreCase("CopyBUD") 
							Dim sEntity As String = GetEntity(si, args)
							If Not Me.IsBUDCreationAllowed(si, sEntity) Then
								Throw  New Exception("Can not create Requirement at this time. Contact BUD manager." & environment.NewLine)
							End If
							Dim BUDFlow = Me.CreateEmptyBUD(si, globals, api, args)
'							Me.CopyBUDDetails(si,globals,api,args, BUDFlow)
'BRApi.ErrorLog.LogMessage(si, "BUDFlow = " & BUDFlow)							
							'Me.SaveInitialAmount(si,globals,api,args, BUDFlow)
							Me.CopyAmount(si,globals,api,args, BUDFlow)
'BRApi.ErrorLog.LogMessage(si, "SAVE INITIAL AMOUNT ")							
							'Clear selections
							Dim paramsToClear As String = "prompt_tbx_CMD_SPLN_BUDTitle__Shared," & _
														"prompt_cbx_BUDPRO_AAAAAA_BUDListCachedEntity__Shared,"
							Return Me.ClearSelections(si, globals, api, args, paramsToClear)
						Case args.FunctionName.XFEqualsIgnoreCase("ClearPrompts")
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
						Case args.FunctionName.XFEqualsIgnoreCase("CreateBUD")
							Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
							'User's permissions check
							If Not Me.IsBUDCreationAllowed(si, sEntity) Then
								Throw  New Exception("Can not create Requirement at this time. Contact BUD manager." & environment.NewLine)
							End If
							
							'Validate Requested Amount Input
							Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")	
							If Not Integer.TryParse(sRequestedAmt, 0) Then	
								Throw New Exception("Requested Amount: " &  sRequestedAmt &  environment.NewLine & "Must enter a whole number.")					
							ElseIf sRequestedAmt <= 0
								Throw New Exception("Requested Amount: Must enter a value greater than zero.")
							End If	
							
							Dim sU1APPNInput As String = args.NameValuePairs.XFGetValue("APPN")
							Dim sU1Input As String = args.NameValuePairs.XFGetValue("APPNFund")
							Dim sU2Input As String = args.NameValuePairs.XFGetValue("MDEP")
							Dim sU3SAGInput As String = args.NameValuePairs.XFGetValue("SAG")
							Dim sU3Input As String = args.NameValuePairs.XFGetValue("APE")
							Dim sU4Input As String = args.NameValuePairs.XFGetValue("DollarType")
							Dim sU6Input As String = args.NameValuePairs.XFGetValue("CommitmentItem")
							
'						    Apply this change to PGM----------------------start
							Dim requiredString As String = ""
							If String.IsNullOrWhiteSpace(sU1APPNInput) Then
								requiredString += "Appropriation"
							End If	
							If String.IsNullOrWhiteSpace(sU1Input) Then
									If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "Fund Code"
							End If	
							If String.IsNullOrWhiteSpace(sU2Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "MDEP"
							End If	
'							If String.IsNullOrWhiteSpace(sU3SAGInput) Then
'								If Not String.IsNullOrWhiteSpace(requiredString) Then
'									requiredString += ", "
'								End If
'								requiredString += "SAG"
'							End If	
							If String.IsNullOrWhiteSpace(sU3Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "APE"
							End If	
'							If String.IsNullOrWhiteSpace(sU4Input) Then
'								If Not String.IsNullOrWhiteSpace(requiredString) Then
'									requiredString += ", "
'								End If
'								requiredString += "Dollar Type"
'							End If	
							If String.IsNullOrWhiteSpace(sU6Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "Cost Category"
							End If	
							If Not String.IsNullOrWhiteSpace(requiredString) Then
								Throw New Exception("The following fields must be populated: " + requiredString + ".")
							End If	

							Dim BUDFlow = Me.CreateEmptyBUD(si, globals, api, args, "Create",sEntity)
				
							Me.SaveInitialAmount(si, globals, api, args, BUDFlow)

							'Clear the selections
							Dim paramsToClear As String = "prompt_tbx_CMD_SPLN_BUDTitle__Shared," & _
														"prompt_tbx_CMD_SPLN_BUDAmt__Shared," & _
														"DL_CMD_SPLN_APPN__Shared," & _
														"DL_CMD_SPLN_APPNFund__Shared," & _
														"DL_CMD_SPLN_MDEP__Shared," & _
														"DL_CMD_SPLN_SAG__Shared," & _
														"DL_CMD_SPLN_SAGAPE__Shared," & _
														"DL_CMD_SPLN_DollarType__Shared," & _
														"DL_CMD_SPLN_CommitmentItem__Shared," & _
														"DL_CMD_SPLN_RecurringCost__Shared," & _
														"prompt_cbx_BUDPRO_AAAAAA_BUDListByEntity__Shared_UpdateReq,"
														
							Return Me.ClearSelections(si, globals, api, args, paramsToClear)
						Case args.FunctionName.XFEqualsIgnoreCase("CreateManpowerREQ")
							Me.CreateManpowerREQ(si, globals, api, args)
						Case args.FunctionName.XFEqualsIgnoreCase("CreateWithholdREQ")
							Try
								Me.CreateWithholdREQ(si, globals, api, args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("DeleteAllFundingRequestsByBUD")

							'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
							Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
							Dim sUFRToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")							
							
							'Add cached parameters to args
							args.NameValuePairs.Add("BUDEntity", sEntity)
							args.NameValuePairs.Add("BUD", sUFRToDelete)
							
							'Calls method to clear the rows
							Me.CallDataManagementDeleteBUD(si, globals, api, args)
							'Calls method to add a default funding request
'Brapi.ErrorLog.LogMessage(si,"made it out of delete")								
							Me.SaveInitialAmount(si, globals, api, args, sUFRToDelete)
'Brapi.ErrorLog.LogMessage(si,"made it out of save")								
							'Set updated name and date
							Me.LastUpdated(si, globals, api, args, sUFRToDelete, sEntity)
						Case args.FunctionName.XFEqualsIgnoreCase("DeleteBUD")
							 Me.DeleteBUD(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("DeleteBUDAcctValue")
							Try
								Me.DeleteBUDAcctValue(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("DeleteDataAttachement")
							Try
'								Me.Check_WF_Complete_Lock(si, globals, api, args)
								Me.DeleteDataAttachement(si,globals,api,args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("DemoteBUD")
							Me.DemoteBUD(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("ImportREQ")
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
									Me.ImportREQ(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try again in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("ImportManpowerREQ")
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_BUDPRO_BUDCRT_0CaAa_A_CivPayRequirement_Singular_Import")
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_BUDCRT_0CaAa_A_CivPayRequirement_Singular_Import","running")
									Me.ImportManpowerREQ(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try again in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_BUDCRT_0CaAa_A_CivPayRequirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_BUDPRO_BUDCRT_0CaAa_A_CivPayRequirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("LastUpdated")
							Return Me.LastUpdated(si,globals,api,args)
						End If
#End Region

#Region "RollFwdReq: Roll Fwd Req"
						If args.FunctionName.XFEqualsIgnoreCase("RollFwdReq") Then	

							'---------------------------------------------------------------------------------------------------
							' PURPOSE: invoke data management to copy Budgets from source S#T# to target S#T#
							'
							' LOGIC OVERVIEW:
							'		- if Roll Forward flag = No then exit
							'		- data management sequence = RollFwdReq 
							'
							' USAGE:
							'		- From button:
							'			Sever Task = Execute Dashbaord Extender Business Rule (General Server)
							'			Task Arguments = {BUD_SolutionHelper}{RollFwdReq}{}
							'			
							' MODIFIED: 
							' <date> 		<user id> 	<JIRA ticket> 	<change description>
							' 2024-04-02 	AK 			RMW-1171		created	
							' 2025-03-21	MH			958				updated to accomodate Oblig and Commit
							'---------------------------------------------------------------------------------------------------
							
'Brapi.ErrorLog.LogMessage(si,"RollFwdReq: Here22")


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
									selectionChangedTaskResult.Message = "Roll Forward has been disallowed by Budgets manager."
								Return selectionChangedTaskResult									
								
							End If	
								
							'---------- run data mgmt sequence ----------
							Dim dataMgmtSeq As String = "BUD_RollFwdReq"     
							Dim params As New Dictionary(Of String, String) 
							'params.Add("Scenario", sScenario)
							'params.Add("Time", "T#" & sTime & ".Base")
							'params.Add("Entity", "E#" & sEntity)
'brapi.ErrorLog.LogMessage(si,"here1")					
							BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
							
							
							
							'---------- run data mgmt sequence (spread) ----------
							Dim dataMgmtSeqSpread As String = "BUD_RollFwdReqSpread"     
							Dim paramsSpread As New Dictionary(Of String, String) 
							'params.Add("Scenario", sScenario)
							'params.Add("Time", "T#" & sTime & ".Base")
							'params.Add("Entity", "E#" & sEntity)
'brapi.ErrorLog.LogMessage(si,"here1")					
							BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeqSpread, paramsSpread)

							


							'---------- warn of any U1s that were not spread because of psread pct not being 100% or 0% ----------
							Dim iScenarioYear As Integer = Strings.Right(sCurrScenario,4).XFConvertToInt

'BRApi.ErrorLog.LogMessage(si, sDebugRuleName & "." & sDebugFuncName & ": sCube=" & sCube & "   sScenario=" & sScenario & "   iScenarioYear=" & iScenarioYear)
							
							Dim mbrU1List As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "U1_APPN_L2", "U1#Appropriation.Base", True)		
							Dim OBLIG_sU1ListOfNoSpread As String = ""
							Dim COMMIT_sU1ListOfNoSpread As String = ""
							
							For Each mbrInfo In mbrU1List
								
								'----- get rollfwd status OBLIG -----
								Dim OBLIG_sSrcMbrScript As String	= "Cb#" & sCube & ":S#" & sCurrScenario & ":T#" & iScenarioYear & "M1:C#Local:E#" & sCube & "_General"  
								OBLIG_sSrcMbrScript &= ":V#Periodic:A#REQ_Phased_Obligation_Spread_Prct:I#None:F#None:O#BeforeAdj:U1#" & mbrInfo.Member.Name & "_General:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

								Dim OBLIG_dTotalPctSpread_M1 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M2 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M2:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M3 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M3:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M4 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M4:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M5 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M5:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M6 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M6:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M7 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M7:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M8 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M8:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M9 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M9:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M10 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M10:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M11 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M11:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M12 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.Replace("M1:C#", "M12:C#")).DataCellEx.DataCell.CellAmount
								Dim OBLIG_dTotalPctSpread_M13 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, OBLIG_sSrcMbrScript.replace("T#" & iScenarioYear, "T#" & iScenarioYear+1).Replace("M1:C#", "M12:C#")).DataCellEx.DataCell.CellAmount
								
								Dim OBLIG_dTotalPctSpread = OBLIG_dTotalPctSpread_M1 + OBLIG_dTotalPctSpread_M2 + OBLIG_dTotalPctSpread_M3 + OBLIG_dTotalPctSpread_M4 + OBLIG_dTotalPctSpread_M5 + OBLIG_dTotalPctSpread_M6 + OBLIG_dTotalPctSpread_M7 + OBLIG_dTotalPctSpread_M8 + OBLIG_dTotalPctSpread_M9 + OBLIG_dTotalPctSpread_M10 + OBLIG_dTotalPctSpread_M11 + OBLIG_dTotalPctSpread_M12 + OBLIG_dTotalPctSpread_M13
								
								
								'----- get rollfwd status COMMIT -----
								Dim COMMIT_sSrcMbrScript As String	= "Cb#" & sCube & ":S#" & sCurrScenario & ":T#" & iScenarioYear & "M1:C#Local:E#" & sCube & "_General"  
								COMMIT_sSrcMbrScript &= ":V#Periodic:A#REQ_Phased_Commitment_Spread_Prct:I#None:F#None:O#BeforeAdj:U1#" & mbrInfo.Member.Name & "_General:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

								Dim COMMIT_dTotalPctSpread_M1 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M2 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M2:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M3 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M3:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M4 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M4:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M5 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M5:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M6 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M6:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M7 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M7:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M8 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M8:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M9 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M9:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M10 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M10:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M11 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M11:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M12 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.Replace("M1:C#", "M12:C#")).DataCellEx.DataCell.CellAmount
								Dim COMMIT_dTotalPctSpread_M13 As Decimal = brapi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, COMMIT_sSrcMbrScript.replace("T#" & iScenarioYear, "T#" & iScenarioYear+1).Replace("M1:C#", "M12:C#")).DataCellEx.DataCell.CellAmount
								
								Dim COMMIT_dTotalPctSpread = COMMIT_dTotalPctSpread_M1 + COMMIT_dTotalPctSpread_M2 + COMMIT_dTotalPctSpread_M3 + COMMIT_dTotalPctSpread_M4 + COMMIT_dTotalPctSpread_M5 + COMMIT_dTotalPctSpread_M6 + COMMIT_dTotalPctSpread_M7 + COMMIT_dTotalPctSpread_M8 + COMMIT_dTotalPctSpread_M9 + COMMIT_dTotalPctSpread_M10 + COMMIT_dTotalPctSpread_M11 + COMMIT_dTotalPctSpread_M12 + COMMIT_dTotalPctSpread_M13
								
'If mbrInfo.Member.Name = "BCA5" Then BRApi.ErrorLog.LogMessage(si, "COMMIT_sSrcMbrScript=" & COMMIT_sSrcMbrScript & "   COMMIT_dTotalPctSpread_M13= " & COMMIT_dTotalPctSpread_M13 & "   COMMIT_dTotalPctSpread= " & COMMIT_dTotalPctSpread)
'If mbrInfo.Member.Name = "BCA5" Then BRApi.ErrorLog.LogMessage(si, "OBLIG_sSrcMbrScript=" & OBLIG_sSrcMbrScript & "   OBLIG_dTotalPctSpread_M13= " & OBLIG_dTotalPctSpread_M13 & "   OBLIG_dTotalPctSpread= " & OBLIG_dTotalPctSpread)
								
								If OBLIG_dTotalPctSpread <> 100 And OBLIG_dTotalPctSpread <> 0 Then 
									If OBLIG_sU1ListOfNoSpread <> "" Then OBLIG_sU1ListOfNoSpread &= ", " 
									OBLIG_sU1ListOfNoSpread &= mbrInfo.Member.Name
								End If
								
								If COMMIT_dTotalPctSpread <> 100 And COMMIT_dTotalPctSpread <> 0 Then 
									If COMMIT_sU1ListOfNoSpread <> "" Then COMMIT_sU1ListOfNoSpread &= ", " 
									COMMIT_sU1ListOfNoSpread &= mbrInfo.Member.Name
								End If
								
							Next
							
'BRApi.ErrorLog.LogMessage(si, "OBLIG_sU1ListOfNoSpread=" & OBLIG_sU1ListOfNoSpread & "   COMMIT_sU1ListOfNoSpread=" & COMMIT_sU1ListOfNoSpread)

							Dim sMsg As String = "Check Task Activity to confirm when Roll Forward is done"
							
							If OBLIG_sU1ListOfNoSpread <> "" Then
								sMsg &= vbCrLf & vbCrLf & "WARNING: The following appropriations were not spread since their Obligation spread percent did not equal 100% or 0%:" & vbCrLf & vbCrLf &  OBLIG_sU1ListOfNoSpread
							End If
							
							If COMMIT_sU1ListOfNoSpread <> "" Then
								sMsg &= vbCrLf & vbCrLf & "WARNING: The following appropriations were not spread since their Commitment spread percent did not equal 100% or 0%:" & vbCrLf & vbCrLf &  COMMIT_sU1ListOfNoSpread
							End If
							
							BFRM_SolutionHelper.WorkflowCompleteOneOrAllMain(si, globals, api, args)
							
							'---------- display done message ----------
							Dim selectionChangedTaskResult2 As New XFSelectionChangedTaskResult()
								selectionChangedTaskResult2.IsOK = True
								selectionChangedTaskResult2.ShowMessageBox = True
								selectionChangedTaskResult2.Message = sMsg
							Return selectionChangedTaskResult2									
						Case args.FunctionName.XFEqualsIgnoreCase("SaveAllHelper")
							Me.Check_WF_Complete_Lock(si, globals, api, args)
							Return Me.SaveAllHelper(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("SaveGenAcctAnnotation")
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
						Case args.FunctionName.XFEqualsIgnoreCase("SelectAll")						
							Return Me.SelectAll(si,globals,api,args)					
						Case args.FunctionName.XFEqualsIgnoreCase("SendStkhldrEmail")
							 Me.SendStatusChangeEmail(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("SetNotificationList")
							 Me.SetNotificationList(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("SetRelatedBUDs")	
							 Me.SetRelatedBUDs(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("ShowAndHideDashboards")
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
							Dim ModifiedCustomSubstVars As XFSelectionChangedTaskResult = Nothing
'BRApi.ErrorLog.LogMessage(si, "sDb: ")
							If wfProfileName.XFContainsIgnoreCase("Approve") Then
								Dim paramsToClear As String = "prompt_cbx_UFRPRO_UFRAPR_0CaAa_ApproveUFRs__UpdateFundingStatus," & _
													"prompt_cbx_UFRPRO_AAAAAA_ApproverWFStatus__Shared,"
'brapi.ErrorLog.LogMessage(si, "401 Approve Condition")													
								ModifiedCustomSubstVars = Me.ShowAndHideDashboards(si,globals,api,args)
								Return Me.ReplaceSelections(si, globals, api, args, paramsToClear, ModifiedCustomSubstVars)
							Else
'brapi.ErrorLog.LogMessage(si, "405 Approve Else Condition")	
								Return Me.ShowAndHideDashboards(si,globals,api,args)
							End If 
						Case args.FunctionName.XFEqualsIgnoreCase("UpdatedStatus")
							Me.Check_WF_Complete_Lock(si, globals, api, args)
							Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
						
							If BUDFlow.xfContainsignorecase("REQ") And wfProfileName.XFContainsIgnoreCase("Formulate") Then
								Try
									Me.SubmitBUD(si, globals, api, args)
									Catch ex As Exception
								End Try
								
							Else If BUDFlow.xfContainsignorecase("REQ") And wfProfileName.XFContainsIgnoreCase("Validate") Then
								Try
									If Not Me.IsBUDValidationAllowed(si, args) Then
										Throw New Exception("Cannot validate Requirement at this time. Contact Budget manager.")
									End If
									Me.ValidateBudget(si, globals, api, args)
									Catch ex As Exception
										Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
								End Try			
								
							Else If (Not BUDFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Approve Budgets CMD") Then
								Me.ApproveBudget(si, globals, api, args)

							Else If (Not BUDFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Approve Budgets") Then				
								Me.SubmitToCommand(si, globals, api, args)
								
								
							Else If (Not BUDFlow.xfContainsignorecase("REQ")) And wfProfileName.XFContainsIgnoreCase("Manage") Then
		'						Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Budgets_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()									
		'							For Each ExistingUFR As MemberInfo In ExistingUFRs
		'								Dim UFR As String = ExistingUFR.Member.Name
		'								Try
											Me.ManageBUDStatusUpdated(si, globals, api, args, "")
		'									Catch ex As Exception
		'								End Try
		'							Next
							End If
							Case args.FunctionName.XFEqualsIgnoreCase("UpdateManpowerBUDStatus")
							Try
								Me.UpdateManpowerBUDStatus(si, globals, api, args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("ValidateBUDs")
							Try
								Me.Check_WF_Complete_Lock(si, globals, api, args)
								If Not Me.IsBUDValidationAllowed(si, args) Then
									Throw New Exception("Cannot validate Requirement at this time. Contact Budget manager.")
								End If
								Return Me.ValidateBUDs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("SubmitBUDs")
							
							Me.Check_WF_Complete_Lock(si, globals, api, args)
							Dim sEntityFLow As String = args.NameValuePairs.XFGetValue("EntityFlow","")
							If String.IsNullOrWhiteSpace(sEntityFlow) Then Return Nothing
							Try
								Dim lEntityFlow As List(Of String) = sEntityFLow.Split(",").ToList()
								For Each EntityFlow As String In lEntityFlow
									Dim BUDEntity As String = EntityFlow.Split(":")(0)
									Dim BUDFlow As String = EntityFlow.Split(":")(1)
									args.NameValuePairs.XFSetValue("BUDEntity", BUDEntity)
									args.NameValuePairs.XFSetValue("BUDFlow", BUDFlow)
									Me.SubmitBUD(si, globals, api, args)
								Next
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Caseargs.FunctionName.XFEqualsIgnoreCase("RollFwd_ExportAllREQs")
							Try	
								Return Me.RollFwd_ExportAllREQs(si,globals,api,args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("ExportAllREQs")
							Try	
								Return Me.ExportAllREQs(si,globals,api,args)
							Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						Case args.FunctionName.XFEqualsIgnoreCase("ExportApprovedREQs")
								Return Me.ExportApprovedREQs(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("FormulateSaveValidation")
							Dim sProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
								
							If sProfileName.Contains(".") Then
								sProfileName = sProfileName.Split(".")(1)
							Else
								Return Nothing
							End If
							
							Me.Check_WF_Complete_Lock(si, globals, api, args)	
					If sProfileName.XFContainsIgnoreCase("Formulate") Or sProfileName.XFContainsIgnoreCase("Validate") Or sProfileName.XFContainsIgnoreCase("Approve")  Then 
								Me.BlankTitleCheck(si,globals,api,args)
							End If 
							Return Me.FormulateSaveValidation(si,globals,api,args)
						Case args.FunctionName.XFEqualsIgnoreCase("SendPackageStatusChangeEmail")
							Me.SendPackageStatusChangeEmail(si, globals, api, args)
						Case args.FunctionName.XFEqualsIgnoreCase("ExportPackage")
							Return Me.ExportPackage(si, globals, api, args)



				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

'END MAIN =================================================================================================

#Region "===CONSTANTS==="
	Public BFRM_SolutionHelper As New OneStream.BusinessRule.DashboardExtender.GBL_BFRM_SolutionHelper.MainClass
	Public BUD_String_Helper As New OneStream.BusinessRule.DashboardStringFunction.CMD_String_Helper.MainClass	
	Private BR_BUDDataSet As New OneStream.BusinessRule.DashboardDataSet.BUD_DataSet.MainClass
	Public GEN_General_String_Helper As New OneStream.BusinessRule.DashboardStringFunction.GEN_General_String_Helper.MainClass	
#End Region

#Region "===PROPERTIES==="
	Private Property sFilePath As String = ""
#End Region

#Region "===HELPER METHODS==="

#Region "IsBUDCreationAllowed: Is BUD Creation Allowed"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for BUD_C20XX
		Public Function IsBUDCreationAllowed(ByVal si As SessionInfo, ByVal sEntity As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sBUDAllowCreationMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Allow_Creation:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sBUDAllowCreation As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sBUDAllowCreationMbrScript).DataCellEx.DataCellAnnotation	
			If sBUDAllowCreation.XFEqualsIgnoreCase("No") Then
				Return False
			Else
				Return True
			End If
		End Function
#End Region		

#Region"Check WF Complete Lock And Allow BUD Creation"
		Public Function CheckWFCompleteLockAndAllowBUDCreation(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Try
				Me.Check_WF_Complete_Lock(si, globals, api, args)
				Dim sEntity As String = args.NameValuePairs.XFGetValue("entity")
				If	Not Me.IsBUDCreationAllowed(si,sEntity) Then
					Throw  New Exception("Can not create Requirement at this time. Contact BUD manager." & environment.NewLine)
				End If
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region
	
#Region "IsBUDPrioritizationAllowed: Is BUD Prioritization Allowed" 
		'Updated: EH RMW-1564 9/3/24 Updated to annual for BUD_C20XX
		Public Function IsBUDPrioritizationAllowed(ByVal si As SessionInfo, ByVal sEntity As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)  & "M12"
			Dim sBUDPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Allow_Prioritization:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sBUDPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sBUDPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sBUDPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If
		End Function
#End Region

#Region "IsBUDValidationAllowed: Is BUD Validation Allowed"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for BUD_C20XX
		Public Function IsBUDValidationAllowed(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, Optional ByVal sEntity As String = "") As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter", sEntity)
			Dim sBUDPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Allow_Validation:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sBUDPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sBUDPermissionMbrScript).DataCellEx.DataCellAnnotation	
			If sBUDPermission.XFEqualsIgnoreCase("no") Then
				Return False
			Else
				Return True
			End If	
		
		End Function
#End Region	

#Region "BlankTitleCheck: Is BUD Title Blank"
		'Updated: EH 8/28/2024 - Ticket 1565 Title member script updated to BUD_Shared scenario
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function BlankTitleCheck(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sFlow As String = args.NameValuePairs.XFGetValue("BUDFlow").Trim
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity").Trim
			Dim sBUDTitleMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sBUDTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sBUDTitleMbrScript).DataCellEx.DataCellAnnotation	

			If sBUDTitle = "" Then

				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = sBUDTitleMbrScript
				objScriptValTitle.TextValue = "!!! REPLACE WITH BUDGET TITLE !!!"
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
				
				Throw  New Exception("Budget title cannot be blank." & environment.NewLine &  " Please enter a title and click save button.")
			End If	
		Return Nothing
		
		End Function
#End Region

#Region "GetEntity: Get Entity"
		'Entity can be passed in directly in the Args or through session when it comes through a pop up
		'This code handles that
		Public Function GetEntity(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
			If String.IsNullOrWhiteSpace(sEntity) Then
				sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
			End If
			Return sEntity
		End Function
#End Region	

#Region "SetParameter: Set Parameter"
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

#Region "ClearSelections: Clear Selections"
		Public Function ClearSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String)As Object
'BRApi.ErrorLog.LogMessage(si,$"ParamsToClear = {ParamsToClear}")
				Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
				For i As Integer = 0 To objDictionary.Count -1
					'Cleare only prompts that are passed in as parameters
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

#Region "ReplaceSelections: Replace Selections"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for BUD_C20XX
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
			Dim sTime As String = WFYear & "M12"
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
			Dim sFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
'brapi.ErrorLog.LogMessage(si, "4098 sEntity: " & sEntity)
'brapi.ErrorLog.LogMessage(si, "4099 sFlow: " & sFlow)	
'brapi.ErrorLog.LogMessage(si, "4099 ParamsToClear: " & ParamsToClear)	
			'member scripts
			Dim sWFMemberScript As String = "Cb#" & wfCube & ":E#" & sEntity & ":C#Local:S#" & wfScenario & ":T#" & sTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sFundingMemberScript As String = "Cb#" & wfCube & ":E#" & sEntity & ":C#Local:S#" & wfScenario & ":T#" & sTime & ":V#Annotation:A#REQ_Funding_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            'get wf and funding satus
			Dim currentWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sWFMemberScript).DataCellEx.DataCellAnnotation
			Dim currentFundingStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sFundingMemberScript).DataCellEx.DataCellAnnotation
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
#End Region

#Region "SelectAll: Select All"
		'Use to set Parameter with value for all selection options of a parameter - Select/Deselect All
		Public Function SelectAll(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			'Get targeted Param
			Dim sParam As String = args.NameValuePairs.XFGetValue("Param").Trim()
			If String.IsNullOrEmpty(sParam) Then Return Nothing

			'Get User's selection & val
			Dim sSelection As String = args.NameValuePairs.XFGetValue("Selection").Trim()
			If String.IsNullOrWhiteSpace(sSelection) Then sSelection = "Unselect"			
			Dim sSelectAllVal As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUDSelectAll","")
			
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

#Region "Get User Security"

		'This method takes in the role of a user and returns the list of FCs the user has access for the given role
		'If the user has an Admin role it would Return Administrator
		Public Function	GetUserSecurityFCs(ByVal si As SessionInfo, ByVal role As String) As String							

			'---get user fund centers for--	
			Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
			Dim sUserName As String = objSignedOnUser.User.Name	
			Dim sUserFundCenters As String = ""
			Dim SQLFC As New Text.StringBuilder
			SQLFC.Append($" 
			With AllGroups AS(
			   SELECT sgc.[GroupKey], sgc.[ChildKey]
			   FROM [SecGroupChild] sgc  
			          
			   UNION All

			   SELECT sgc.[GroupKey], AG.[ChildKey]
			   FROM [SecGroupChild] sgc            
			   JOIN AllGroups AG on sgc.childkey = AG.Groupkey
			)
			SELECT Distinct SG.[UniqueID] as GroupID, SG.Name as AccessGroup,U.[UniqueID] as UserID,U.Name as UserName, GU.GroupKey as [role]

			FROM  [SecUser] as u
			JOIN  [AllGroups ] AS GU on gu.ChildKey = u.uniqueid
			JOIN  [SecGroup] AS SG on sg.UniqueID = gu.GroupKey
			WHERE ((SG.Name Like '%BUD%' and  SG.Name Like '%{role}%' and  SG.Name Not Like '%{role}') OR (SG.Name = 'Administrators'))
			AND U.Name = '{sUserName}'
			ORDER by SG.Name
			")

'brapi.ErrorLog.Logmessage(si,"sql=" & SQLFC.ToString)
			Dim dtAll As New DataTable()
			Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
				 dtAll = BRApi.Database.ExecuteSql(dbConn,SQLFC.ToString(),True)
			End Using
			
			For Each dataRow As DataRow In dtAll.Rows					
				Dim sGroup As String = dataRow.Item("AccessGroup")
				'If the User is part of Admin, return only Administrator
				If sGroup.XFEqualsIgnoreCase("Administrators") Then
					sUserFundCenters = 	"Administrator"
					Exit For
				End If
				
				Dim sFC As String =""
				If sGroup.XFContainsIgnoreCase("USAREUR_AF") 
					sfc = sGroup.Split("_")(6)
				Else
					sfc = sGroup.Split("_")(5)
				End If
					
				sUserFundCenters += sFC  + ","
			Next

			If (String.IsNullOrWhiteSpace(sUserFundCenters)) Then
				sUserFundCenters = String.Empty
			Else 
				sUserFundCenters = sUserFundCenters.TrimEnd(","c)
			End If
			
BRApi.ErrorLog.LogMessage(si, "FC = " & sUserFundCenters)					
			Return sUserFundCenters
			
		End Function	
			
	
#End Region

#End Region

#Region "AddBUDAcctValue: Add BUD Account Value"
		
		'RMW-1224: KN: Add BUD Account Value for Dropdown Lists
		Public Function AddBUDAcctValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
		Try
				Dim sCmd As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
				Dim sValue As String = args.NameValuePairs.XFGetValue("Value")
				Dim SQL As New Text.StringBuilder
				SQL.Append($"INSERT INTO XFC_BUD_Name_Value_List VALUES ('{sCmd}', '{sAccount}', '{sValue}', '');") 
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "ApproveBudget: Approve Budget"
		'Updated: EH RMW-1564 9/3/24 Updated to annual for BUD_C20XX
		'Updated: KN RMW-1717 9/30/24 Updated to allow submission by filters
		Public Function ApproveBudget(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim BUDList As String = ""
			Dim sEntity As String = ""
			Dim sBUD As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
'			'Args To pass Into dataset BR
'			Dim dsArgs As New DashboardDataSetArgs 
'			dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
'			dsArgs.DataSetName = "BUDListByEntity"
'			dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
'			dsargs.NameValuePairs.XFSetValue("CubeView", "")

			
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")
			
			'Loops and update status
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sBUD = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
			
			    Dim BUDStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusMemberScript).DataCellEx.DataCellAnnotation

				If BUDStatus.XFEqualsIgnoreCase("L2 Ready for Approval") Then
					
					Dim NewStatus As String = "L2 Approved"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = BUDStatusMemberScript
					objScriptValWFStatus.TextValue = NewStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
						'update Status History
						Try
							Me.UpdateStatusHistory(si, globals, api, args, NewStatus, sBUD, sEntity, sFundCenter)
							Catch ex As Exception
						End Try
						
						'Send email
						Try
							Me.SendStatusChangeEmail(si, globals, api, args, sBUD)
							Catch ex As Exception
						End Try
						
						'Set Updated Date and Name
						Try
							Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
							Catch ex As Exception
						End Try
				
					End If 					
				Next
			
			Return Nothing
		End Function
	
#End Region

#Region "ApprovalBUDsVal: Approve BUDs from Validate"
		'Created: KN - 09/25/2024 - Ticket 1708	
		'Created to submit multiple BUDs for prioritization 
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
				Dim BUDTime As String = WFYear & "M12"
				'Dim BUDTime As String = WFYear
			
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")
				For Each row As DataRow In oReqList.Rows
					Dim BUDEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
					Dim BUDFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
'BRApi.ErrorLog.LogMessage(si, $"EntityFlow = {row("EntityFlow")} || BUDEntity = {BUDEntity} || BUDFlow = {BUDFlow}")					
				
					'------------Get current BUD workflow status-------------
					Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"BUDMemberScript = {BUDMemberScript}")
					Dim currBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
					If(String.IsNullOrWhiteSpace(currBUDStatus)) Then
						Return Nothing
					End If
					
					Dim BUDValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim BUDApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim newBUDStatus As String = ""
				
					If sProfileSubString = "Validate Budgets" Then
	'					If sValidatedAmount = 0 Then 
	'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
	'					End If
						currBUDStatus = currBUDStatus.Substring(0,2)
						newBUDStatus = currBUDStatus & " Ready for Approval"
						
					Else If sProfileSubString = "Validate Budgets CMD"
	'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
	'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
	'					End If
						currBUDStatus = currBUDStatus.Substring(0,2)
						newBUDStatus = currBUDStatus & " Ready for Approval"
					End If
					
                    'Update BUD Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = BUDMemberScript
					objScriptValStatus.TextValue = newBUDStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus, BUDFlow, BUDEntity)
						'Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, BUDFlow, BUDEntity)
					Catch ex As Exception
					End Try
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region

#Region "CachePrompts: Cache Prompts"
			Public Function CachePrompts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sBUD As String = args.NameValuePairs.XFGetValue("BUD")
			Dim sMode As String = args.NameValuePairs.XFGetValue("mode","")
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			
			If sMode.XFContainsIgnoreCase("copyBUD") And String.IsNullOrWhiteSpace(sEntity) Then
				Throw New Exception("Please select a Fund Center")
				Return Nothing
			End If
			
			If Not String.IsNullOrWhiteSpace(sEntity) Then
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity",sEntity)
			End If
			
'BRApi.ErrorLog.LogMessage(si, "RD/ BUD: " + sBUD)
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD",sBUD)
			
			If Not String.IsNullOrWhiteSpace(sDashboard) Then
'BRApi.ErrorLog.LogMessage(si, "RD/ Dashboard: " + sDashboard)
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Dashboard",sDashboard)
			End If

Return Nothing
End Function


#End Region

#Region "CallDataManagementDeleteBUD: CallDataManagementDeleteBUD"
		'The CallDataManagementDeleteBUD removes all funding request values for the entity/ufr combination
		Public Function CallDataManagementDeleteBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
		Try						
				
			'Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim iBUDAmount As Integer = Nothing
			Dim sEntity As String =  args.NameValuePairs.XFGetValue("BUDEntity")
			Dim sBUD As String =  args.NameValuePairs.XFGetValue("BUD")
			
					'invoke a DM task to clear added row data
					Dim sBUDTimeParam As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim dataMgmtSeq As String = "Delete_BUD"     
					Dim params As New Dictionary(Of String, String) 
					params.Add("Entity", "E#" & sEntity)
					params.Add("Scenario", sScenario)
					'params.Add("Time", "T#" & sBUDTimeParam & ".Base")
					'params.Add("Time", "T#" & sBUDTimeParam)
					params.Add("Flow", sBUD)
'Brapi.ErrorLog.LogMessage(si,"sBUDTimeParam = " & sBUDTimeParam)						
					'Get the FYDP from the cycle
					Dim intYear = 0
					Integer.TryParse(sBUDTimeParam, intYear)
					
					'DEV NOTE: It ignores the first year for some reason, so it is added at the end.
					Dim time As String = ""
					'time = "T#" & intYear & ",T#" & intYear - 1 &  ",T#" & intYear + 1 &  ",T#" & intYear + 2 &  ",T#" & intYear + 3 &	  ",T#" & intYear + 4 & ",T#" & intYear
					time = "T#" & intYear & "M1,T#" & intYear &  "M2,T#" & intYear &  "M3,T#" & intYear &  "M4,T#" & intYear & "M5,T#" & intYear & "M6,T#" & intYear & "M7,T#" & intYear & "M8,T#" & intYear & "M9,T#" & intYear & "M10,T#" & intYear & "M11,T#" & intYear & "M12,T#" & intYear + 1 & "M12,T#" & intYear & "M1"
									   									   				   									   
					params.Add("Time", "T#" & time)
'Brapi.ErrorLog.LogMessage(si,"time = " & time)					
					BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
'Brapi.ErrorLog.LogMessage(si,"Sequence should have finished")
			
			Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function
			  
#End Region

#Region "ConfirmReview: Confirm Review"
		
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
				Dim BUDTime As String = WFYear & "M12"

				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
				Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
		
				'Get current status of UFR
				Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

				Dim currentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
				If(String.IsNullOrWhiteSpace(currentStatus)) Then
					
					Return Nothing
				End If
				
				Dim newStatus As String = "Ready for Validation"
					
                    'Update UFR Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue	
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = BUDMemberScript
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
					Me.LastUpdated(si, globals, api, args, BUDFlow, BUDEntity)
				Catch ex As Exception
				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region

#Region "CopyAmount: Copy Amount"
		
		Public Function CopyAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal newBUDFlow As String) As Boolean
		Try
			Dim cube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			
			'Get source scenrio and time from the passed in value
			Dim srcEntity = args.NameValuePairs.XFGetValue("sourceEntity")
			'Dim TrgtEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
			Dim srcFlow As String = args.NameValuePairs.XFGetValue("SourceBUD")
			Dim srcSenario As String = args.NameValuePairs.XFGetValue("sourceScenario")
			Dim srcScenarioMbrId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, srcSenario)
			Dim srcTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si, srcScenarioMbrId)
			srcTime  =  srcTime.Substring(0,4)		
			
			'target BUD
					
			Dim trgtScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim trgtBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim trgtFlow = newBUDFlow
			Dim trgtEntity As String = GetEntity(si, args)
		
			Dim params As New Dictionary(Of String, String) 
			'params.Add("Scenario", sScenario)
			'params.Add("Time", "T#" & sTime & ".Base")
			params.Add("Entity", TrgtEntity)
			
			params.Add("srcEntity", srcEntity)
			params.Add("srcFlow", srcFlow)
			params.Add("srcSenario", srcSenario)
			params.Add("srcTime", srcTime)
			
			params.Add("trgtFlow", trgtFlow)
	
BRApi.ErrorLog.LogMessage(si, "srcEntity = " & srcEntity & ", srcFlowBUD = " & srcFlow & ", srcSenario = " & srcSenario & ", trgtFlow = " & trgtFlow & ", trgtEntity = " & trgtEntity)
			
			BRApi.Utilities.ExecuteDataMgmtSequence(si, "BUD_Copy_Amount", params)

			Return Nothing
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function
			  
#End Region 

#Region "CopyBUDDetails: Copy BUD Details"
		'Created: YH - 10/20/2023
		'Updated: Fronz - 03/19/2024 replaced hard coded accounts assigned to the CopyAccounts list with BUD_Copy_List.Children
		'Updated: Fronz - 04/17/2024 commented out the function to copy an existing attachment and an existing "General Comment" from a previously created Budget
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'How it works:
				'The copy function gets the source BUD passed into it through args and gets the destination from session variable.
				'The new BUD name is passed into it through args.	
				'Source entity can be passed in as well. If it is not the target entity will be the default.
		'USAGE: Called on_Click btn_CMD_SPLN_CopyREQ
		'Updated: EH - 08/26/24 - RMW 1565 Updated Time to yearly for BUD_C20XX
		'Updated: AK 09/10/2024 - RMW-1573 modified to BUD_Shared-->1999
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		
		Public Function CopyBUDDetails(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal newBUDFlow As String, Optional ByVal srcEntity As String = "") As Boolean
			Dim sWFProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			
			'obtains the Fund, Name, Entity, and IT selection from the Create BUD Dashboard
			Dim sSourceBUD As String = args.NameValuePairs.XFGetValue("SourceBUD")
'BRApi.ErrorLog.LogMessage(si, "Source: " & sSourceBUD) 
			
			'For Merge the source format is "FC - Flow : Title but for copy it is just "Flow". These accounts for both
			If (sSourceBUD.XFContainsIgnoreCase("-") And sSourceBUD.XFContainsIgnoreCase(":")) Then
				sSourceBUD = sSourceBUD.Split(":")(0).Split("-")(1).Trim
			End If
			
			Dim sBUDTitle As String = args.NameValuePairs.XFGetValue("BUDName")		
			
			'Get source scenrio and time from the passed in value
			Dim sourceSenario As String = args.NameValuePairs.XFGetValue("sourceScenario")
			Dim sourceScenarioMbrId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sourceSenario)
			Dim sourceTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si, sourceScenarioMbrId)
			sourceTime  =  sourceTime.Substring(0,4)
			
'BRApi.ErrorLog.LogMessage(si, "title: " & sBUDTitle)						
			Dim sEntity As String = GetEntity(si, args)
			
			If Not Me.IsBUDCreationAllowed(si, sEntity) Then			
				Throw  New Exception("Can not copy Requirement at this time. Contact Budget manager." & environment.NewLine)			
			End If
			'For parent level merges the source and target entries may be different. In that case srcEntity is passed in.
			
			If String.IsNullOrWhiteSpace(srcEntity) Then
				'srcEntity = sEntity
				srcEntity = args.NameValuePairs.XFGetValue("sourceEntity")
			End If
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sBUDUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim sBUDFundingStatus As String = "Unfunded"
			
			'-----------Grab text3 from base entity member-------------------
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			entityText3 = entityText3.Substring(entityText3.Length -2, 2)
			'-----------set new BUD workflow status--------------------
			Dim sBUDWFStatus As String = entityText3 & " Working"
			
			'Validate that BUD Title is not empty
			Dim BUDCreated As Boolean = False
'	        'Create UFR	
			'Does this varaible serve a purpose? does not seem to be writing to an intersection after being declared Fronz 03/19/2024
			'Dim BUDAmountMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#UFR_Requested_Amt:F#" & newBUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
			'Copy all children of A#REQ_Copy_Info members when creating a new Budget
			Dim CopyAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "A_REQ_Main"), "A#REQ_Copy_List.Children.Remove(REQ_Title,REQ_ID,REQ_Status_History,REQ_Rqmt_Status,REQ_Creator_Name,REQ_Creation_Date_Time)", True)

			For Each BUDAccount As MemberInfo In CopyAccounts
				Dim sourceMbrScript As String = "Cb#" & sCube & ":E#" & srcEntity & ":C#Local:S#" & sourceSenario & ":T#" & sourceTime & ":V#Annotation:A#" & BUDAccount.Member.Name & ":F#" & sSourceBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim targetMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & BUDAccount.Member.Name & ":F#" & newBUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
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
'			Dim sCopyAttachments As String = Me.CopyAttachments(si, sCube, srcEntity, sEntity, sScenario, sBUDTime, sSourceBUD, newBUDFlow)
			
'			'Copy General Comments
'			Me.CopyGeneralComments(si,globals,api,args, sSourceBUD, srcEntity, newBUDFlow, sEntity)
				
			BUDCreated = True

			Return BUDCreated
			 
		End Function
			  
#End Region

#Region "CopyCMDApprovedAmount: Copy CMD Approved Amount"	
		'Copy Amt from A#REQ_Requested_Amt to A#REQ_CMD_App_Req_Amt_ADM once the BUD is approved by CMD and status is set to L2 Approved
		Public Function CopyCMDApprovedAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal sEntity As String, ByVal sFlow As String)		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)		
			Dim sSrcAcct As String = "REQ_Requested_Amt"
			Dim sTgtAcct As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim iTime As Integer = Convert.ToInt32(sBUDTime)
			

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

#Region "CreateEmptyBUD: Create Empty BUD"
		'Created: YH - 10/25/2023
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'The Create function gets the Entity and Title passed into it.
		'It will get audit info from the logged in user and populates the audit information
		'The new BUD name is passed into it through args.
		'Updated: EH 8/22/2024 - Ticket 1565 Updated Title, ID, Creation Name/Date
		'Data saved at BUD_Shared and updated time to yearly
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function CreateEmptyBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal flag As String = "", Optional ByVal TargetEntity As String =  "") As Object

			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = TargetEntity
'BRApi.ErrorLog.LogMessage(si, "Entity (before): " & sEntity)			
			If String.IsNullOrWhiteSpace(sEntity) Then
				sEntity = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
			End If
'BRApi.ErrorLog.LogMessage(si, "Entity (after): " & sEntity)				
			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			'Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)

			
			Dim sRecurringCost As String = args.NameValuePairs.XFGetValue("RecurringCost")
			If (String.IsNullOrWhiteSpace(sRecurringCost)) Then sRecurringCost = "Yes"
			Dim sBUDTitle As String = args.NameValuePairs.XFGetValue("BUDName")	
			Dim BUDtitleLength As Integer = sBUDTitle.Length
			
			'throw error if BUD title Is empty
			If BUDtitleLength = 0 Then 
				Throw New Exception("Must enter a title for the Requirement." & environment.NewLine)
			End If
			
			Dim sRequestedAmt As String = args.NameValuePairs.XFGetValue("RequestedAmt").Replace(",","")
			
			'throw error if BUD Amount Is empty
			If flag.XFEqualsIgnoreCase("create") Then
				If sRequestedAmt.Length = 0 Then 
					Throw New Exception("Must enter an amount for the Requirement." & environment.NewLine)
				End If
			End If
			Dim sBUDUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim sBUDFundingStatus As String = "Unfunded"
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
			'-----------set new BUD workflow status--------------------------
			Dim sBUDWFStatus As String = entityText3 & " Working"
				
			Dim BUDCreated As Boolean = False
	        'Check if there is a BUD flow member available. If not create one	
			Dim BUDFlow As String = ""
			Dim ExistingBUDs As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).Select(Function(n) n.Member.Name).ToList()
			ExistingBUDs.OrderBy(Function(x) convert.ToInt32(x.split("_")(1))).ToList()
			
'Updated by JM 4/30/25 - Logic copied over from import method. Finds used flows via SQL		
				Dim UsedFlowsSQL As String ="Select [Entity], Flow From DataAttachment WHERE  [Entity] = '" & sEntity &"' And [Cube] = '" & sCube &"' And Scenario =  '" &  sScenario & "' Order by Flow"
			
				Dim dtUsedFlows As New DataTable()
				Using dbConnAppflow As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					dtUsedFlows =BRApi.Database.ExecuteSql(dbConnAppflow,UsedFlowsSQL,True)
				End Using
				
				Dim usedFlowsPerScenarioAndFC As List(Of String) = New List(Of String)
			
				For Each r As DataRow In dtUsedFlows.Rows
					usedFlowsPerScenarioAndFC.Add(r.Item("Entity") & ":" &  r.Item("Flow"))		
				Next		
			
				For Each fl As String In ExistingBUDs
					Dim key As String = sEntity & ":"  & fl
					
					If Not usedFlowsPerScenarioAndFC.Contains(key)
					'BRApi.ErrorLog.LogMessage(si, "Used REQ  = " & fl)
							BUDFlow = fl
						Exit For
					End If
				Next

				If String.IsNullOrWhiteSpace(BUDFlow) Then
						
				BUDFlow = Me.CreateFlow(si, globals, api, args)
			End If
	BRApi.ErrorLog.LogMessage(si, "From Create BUD : " &	BUDFlow & ", sEntity: " & sEntity & ", sBUDTitle: " & sBUDTitle)			

'			Dim ExistingBUDs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).OrderBy(Function(x) convert.ToInt32(x.Member.name.split("_")(1))).ToList()

'			Dim flowMemberExists As Boolean = False
'			For Each ExistingBUD As MemberInfo In ExistingBUDs
						
'				Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & ExistingBUD.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
''BRApi.ErrorLog.LogMessage(si, "BudName : " &	ExistingBUD.Member.Name)				
'			 	Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleScr).DataCellEx.DataCellAnnotation
'            	If String.IsNullOrWhiteSpace(TitleValue) Then				
'					flowMemberExists = True
'					BUDFlow = ExistingBUD.Member.Name
'					Exit For
'				End If
'			Next

'			If Not flowMemberExists Then	
'				BUDFlow = Me.CreateFlow(si, globals, api, args)
'			End If
''BRApi.ErrorLog.LogMessage(si, "From Create BUD : " &	BUDFlow & ", sEntity: " & sEntity & ", sBUDTitle: " & sBUDTitle)	

'Create  REQ_ID 
		Dim BUDIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_ID:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		'Dim BUDIDSharedMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#BUD_Shared:T#1999:V#Annotation:A#REQ_ID:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

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
		Dim sDTBUDIDs As New List(Of String)
		
		For Each dataRow As DataRow In dtID.Rows
			sDTBUDIDs.Add(dataRow.Item("ID"))
		Next		
			
		'	'For Each dataRow As DataRow In dtID.Rows		
		'	Dim record As String =  dataRow.ToString
		'	record = dataRow.Item("ID")
		'	brapi.ErrorLog.LogMessage(si, record)
		'	'Next

'BRApi.ErrorLog.LogMessage(si, "Before Rows = 0" & dtID.Rows.Count)								
		If dtID.Rows.Count = 0 Then
			Dim sEntityRemoveGen = sEntity.Replace("_General","")
			Dim sStratingIDNum As String = "00001"
			Dim sStrartingBUDID As String = sEntityRemoveGen & "_" & sStratingIDNum
'BRApi.ErrorLog.LogMessage(si, "After Rows = 0")
			
		'Update BUD ID for WF Scenario
			Dim objListofScriptsBUDID As New List(Of MemberScriptandValue)
		    Dim objScriptValBUDID As New MemberScriptAndValue
			objScriptValBUDID.CubeName = sCube
			objScriptValBUDID.Script = BUDIDMemberScript
			objScriptValBUDID.TextValue = sStrartingBUDID
			objScriptValBUDID.IsNoData = False
			objListofScriptsBUDID.Add(objScriptValBUDID)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDID)
		'Update BUD ID for Shared Scenario
'			Dim objListofScriptsBUDIDShared As New List(Of MemberScriptandValue)
'		    Dim objScriptValBUDIDShared As New MemberScriptAndValue
'			objScriptValBUDIDShared.CubeName = sCube
'			objScriptValBUDIDShared.Script = BUDIDSharedMemberScript
'			objScriptValBUDIDShared.TextValue = sStrartingBUDID
'			objScriptValBUDIDShared.IsNoData = False
'			objListofScriptsBUDIDShared.Add(objScriptValBUDIDShared)
'			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDIDShared)	
			

		Else
'BRApi.ErrorLog.LogMessage(si, "Inside else ")	
			Dim sBUDIDfromDT As String = String.Join(",", sDTBUDIDs.ToArray())
			Dim iBUDIDNum As Integer = Convert.ToInt32(sBUDIDfromDT.split("_")(1))
			Dim sNewIDNum As Integer = iBUDIDNum + 1
			Dim sNewfullIDNum = sNewIDNum.ToString("D5")
	 		Dim sEntityRemoveGeneral = sEntity.Replace("_General","")
			Dim sNewBUDID As String = sEntityRemoveGeneral & "_" & sNewfullIDNum
			'Update BUD ID for WF Scenario
			Dim objListofScriptsBUDID As New List(Of MemberScriptandValue)
		    Dim objScriptValBUDID As New MemberScriptAndValue
			objScriptValBUDID.CubeName = sCube
			objScriptValBUDID.Script = BUDIDMemberScript
			objScriptValBUDID.TextValue = sNewBUDID
			objScriptValBUDID.IsNoData = False
			objListofScriptsBUDID.Add(objScriptValBUDID)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDID)
			'Update BUD ID for WF Scenario
'			Dim objListofScriptsBUDIDShared As New List(Of MemberScriptandValue)
'		    Dim objScriptValBUDIDShared As New MemberScriptAndValue
'			objScriptValBUDIDShared.CubeName = sCube
'			objScriptValBUDIDShared.Script = BUDIDSharedMemberScript
'			objScriptValBUDIDShared.TextValue = sNewBUDID
'			objScriptValBUDIDShared.IsNoData = False
'			objListofScriptsBUDIDShared.Add(objScriptValBUDIDShared)
'			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDIDShared)
			
		End If
			
'			Old intersection for Title Account
			Dim BUDTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			'Dim BUDTitleSharedMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#BUD_Shared:T#1999:V#Annotation:A#REQ_Title:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			
			Dim BUDUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creator_Name:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim BUDDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim BUDFundingStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Funding_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim BUDWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim BUDRecurringCostMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Recurring_Cost_Ind:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"   				
			Dim BUDWFBUDStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Status_History:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			'Brapi.ErrorLog.LogMessage(si, "Title" & BUDTitleMemberScript)	
'            Update BUD Title For WF Scenario
			Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
		    Dim objScriptValTitle As New MemberScriptAndValue
			objScriptValTitle.CubeName = sCube
			objScriptValTitle.Script = BUDTitleMemberScript
			objScriptValTitle.TextValue = sBUDTitle
			objScriptValTitle.IsNoData = False
			objListofScriptsTitle.Add(objScriptValTitle)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
'	BRApi.ErrorLog.LogMessage(si, "Inside else ")		
			'Update BUD TitleShared for Shared Scenario
'			Dim objListofScriptsTitleShared As New List(Of MemberScriptandValue)
'		    Dim objScriptValTitleShared As New MemberScriptAndValue
'			objScriptValTitleShared.CubeName = sCube
'			objScriptValTitleShared.Script = BUDTitleSharedMemberScript
'			objScriptValTitleShared.TextValue = sBUDTitle
'			objScriptValTitleShared.IsNoData = False
'			objListofScriptsTitleShared.Add(objScriptValTitleShared)
			
'			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitleShared)
			
			'Update BUD Creator
			Dim objListofScriptsUser As New List(Of MemberScriptandValue)
		    Dim objScriptValUser As New MemberScriptAndValue
			objScriptValUser.CubeName = sCube
			objScriptValUser.Script = BUDUserMemberScript
			objScriptValUser.TextValue = sBUDUser
			objScriptValUser.IsNoData = False
			objListofScriptsUser.Add(objScriptValUser)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsUser)
			
			'Update BUD Created Date
			Dim objListofScriptsDate As New List(Of MemberScriptandValue)
		    Dim objScriptValDate As New MemberScriptAndValue
			objScriptValDate.CubeName = sCube
			objScriptValDate.Script = BUDDateMemberScript
			objScriptValDate.TextValue = CurDate
			objScriptValDate.IsNoData = False
			objListofScriptsDate.Add(objScriptValDate)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDate)
			
			'Update BUD Funding Status 
			Dim objListofScriptsFundingStatus As New List(Of MemberScriptandValue)
		    Dim objScriptValFundingStatus As New MemberScriptAndValue
			objScriptValFundingStatus.CubeName = sCube
			objScriptValFundingStatus.Script = BUDFundingStatusMemberScript
			objScriptValFundingStatus.TextValue = sBUDFundingStatus
			objScriptValFundingStatus.IsNoData = False
			objListofScriptsFundingStatus.Add(objScriptValFundingStatus)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsFundingStatus)
			
			'Update BUD Workflow Status
			Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
		    Dim objScriptValWFStatus As New MemberScriptAndValue
			objScriptValWFStatus.CubeName = sCube
			objScriptValWFStatus.Script = BUDWFStatusMemberScript
			objScriptValWFStatus.TextValue = sBUDWFStatus
			objScriptValWFStatus.IsNoData = False
			objListofScriptsWFStatus.Add(objScriptValWFStatus)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
			
			'Update BUD Recurring Cost
			Dim objListofScriptsRecurringCost As New List(Of MemberScriptandValue)
		    Dim objScriptValRecurringCost As New MemberScriptAndValue
			objScriptValRecurringCost.CubeName = sCube
			objScriptValRecurringCost.Script = BUDRecurringCostMemberScript
			objScriptValRecurringCost.TextValue = sRecurringCost
			objScriptValRecurringCost.IsNoData = False
			objListofScriptsRecurringCost.Add(objScriptValRecurringCost)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsRecurringCost)		
			
			'Update BUD Status History
			Dim sCompleteRQWFStatus As String = sBUDUser & " : " & CurDate & " : " & sEntity & " : " & sBUDWFStatus
			Dim objListofScriptsStatusHistory As New List(Of MemberScriptandValue)
		    Dim objScriptValStatusHistory As New MemberScriptAndValue
			objScriptValStatusHistory.CubeName = sCube
			objScriptValStatusHistory.Script = BUDWFBUDStatusHistoryMemberScript
			objScriptValStatusHistory.TextValue = sCompleteRQWFStatus
			objScriptValStatusHistory.IsNoData = False
			objListofScriptsStatusHistory.Add(objScriptValStatusHistory)		
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsStatusHistory)	
			
#Region "Old update status history code"			
'			'Update BUD Status History(need to revise) causes other status except for working to record
'				Try
'					Me.UpdateStatusHistory(si, globals, api, args, sBUDWFStatus, BUDFlow, sEntity )
'				Catch ex As Exception
'				End Try
#End Region

				'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, BUDFlow, sEntity)
				Catch ex As Exception
			End Try
			
			Return BUDFlow
		End Function
	
#End Region

#Region "CreateFlow: Create Flow"
		Public Function CreateFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			Dim ExistingBUDs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).OrderBy(Function(x) convert.ToInt32(x.Member.name.split("_")(1))).ToList()
			Dim mbrSourceBUD As MemberInfo = ExistingBUDS.Last()
'Brapi.ErrorLog.LogMessage(si,"Line 138 inside IF" & mbrSourceBUD.Member.Name)
            Dim iBUDNum As Integer = Convert.ToInt32(mbrSourceBUD.Member.Name.split("_")(1))
			Dim sParentMemberName As String = "Command_Requirements"
	        Dim sMemberName As String = "REQ_" & iBUDNum + 1
			Dim sMemberDesc As String = ""
'Brapi.ErrorLog.LogMessage(si,"Line 143 inside IF  " & sMemberName)								
			
	        'Dim mbrSourceBUD As String
			Dim sourceMbrProperties As VaryingMemberProperties = BRApi.Finance.Members.ReadMemberPropertiesNoCache(si, dimtype.Flow.Id, mbrSourceBUD.member.Name)
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
			
			'Create new Flow BUD 
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
#End Region

#Region "CreateManpowerREQ: CreateManpowerREQ"

		Public Function CreateManpowerREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
			Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			Dim return_message As String = ""
			If String.IsNullOrWhiteSpace(sEntity) OrElse String.IsNullOrWhiteSpace(sAPPN) Then
				return_message = "Please select a Funds Center and Appropriation to create a requirement"
				Throw New XFUserMsgException(si, New Exception(return_message))
			End If
			Dim sFlow As String = "REQ_00_CP_" & sAPPN
			Dim sBUDTitle As String = sAPPN & " Manpower Requirement"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sBUDUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim REQID As String = ""
'brapi.ErrorLog.LogMessage(si,"sBUDTime=" & sBUDTime)			
			'--------- get Entity Text3 --------- 							
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			entityText3 = entityText3.Substring(entityText3.Length -2, 2)
			
'brapi.ErrorLog.LogMessage(si,"entityText3=" & entityText3)				
				Dim BUDIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_ID:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim BUDWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Rqmt_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Creator_Name:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Creation_Date_Time:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

				If sEntity.XFContainsIgnoreCase("_General")
					REQID = sEntity.Replace("_General","_")  & "_00000_CP_" & sAPPN 
				Else 
					REQID = sEntity &  "_00000_CP_" & sAPPN 
				End If 
				Dim objListofScriptsBUDID As New List(Of MemberScriptandValue)
			    Dim objScriptValBUDID As New MemberScriptAndValue
				objScriptValBUDID.CubeName = sCube
				objScriptValBUDID.Script = BUDIDMemberScript
				objScriptValBUDID.TextValue = REQID
				objScriptValBUDID.IsNoData = False
				objListofScriptsBUDID.Add(objScriptValBUDID)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDID)
				
				' Update BUD Title For WF Scenario
				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = BUDTitleScr
				objScriptValTitle.TextValue = sBUDTitle
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)

				'Update BUD Creator
				Dim objListofScriptsUser As New List(Of MemberScriptandValue)
			    Dim objScriptValUser As New MemberScriptAndValue
				objScriptValUser.CubeName = sCube
				objScriptValUser.Script = BUDUserMemberScript
				objScriptValUser.TextValue = sBUDUser
				objScriptValUser.IsNoData = False
				objListofScriptsUser.Add(objScriptValUser)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsUser)
				
				'Update BUD Created Date
				Dim objListofScriptsDate As New List(Of MemberScriptandValue)
			    Dim objScriptValDate As New MemberScriptAndValue
				objScriptValDate.CubeName = sCube
				objScriptValDate.Script = BUDDateMemberScript
				objScriptValDate.TextValue = CurDate
				objScriptValDate.IsNoData = False
				objListofScriptsDate.Add(objScriptValDate)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDate)

				'Update BUD Workflow Status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = BUDWFStatusMemberScript
				objScriptValWFStatus.TextValue = entityText3 & " Working"
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				Try
					Me.UpdateStatusHistory(si, globals, api, args, entityText3 & " Working",sFlow)
				Catch ex As Exception
				End Try
				Try
					Me.LastUpdated(si, globals, api, args, sFlow, sEntity)
				Catch ex As Exception
				End Try
				
				Return Nothing
			End Function				
#End Region

#Region "CreateWithholdREQ: CreateWithholdREQ"

		Public Function CreateWithholdREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity")
			Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			Dim return_message As String = ""
			If String.IsNullOrWhiteSpace(sEntity) OrElse String.IsNullOrWhiteSpace(sAPPN) Then
				return_message = "Please select a Funds Center and Appropriation to create a requirement"
				Throw New XFUserMsgException(si, New Exception(return_message))
			End If
			Dim sFlow As String = "REQ_00_WH_" & sAPPN
			Dim sBUDTitle As String = sAPPN & " Withhold Requirement"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sBUDUser As String = si.UserName
			Dim CurDate As Date = Datetime.Now
			Dim REQID As String = ""
'brapi.ErrorLog.LogMessage(si,"sBUDTime=" & sBUDTime)			
			'--------- get Entity Text3 --------- 							
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			entityText3 = entityText3.Substring(entityText3.Length -2, 2)
			
'brapi.ErrorLog.LogMessage(si,"entityText3=" & entityText3)				
				Dim BUDIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_ID:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim BUDWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Rqmt_Status:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Creator_Name:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & "M12:V#Annotation:A#REQ_Creation_Date_Time:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

				If sEntity.XFContainsIgnoreCase("_General")
					REQID = sEntity.Replace("_General","_")  & "_00000_WH_" & sAPPN 
				Else 
					REQID = sEntity &  "_00000_WH_" & sAPPN 
				End If 
				Dim objListofScriptsBUDID As New List(Of MemberScriptandValue)
			    Dim objScriptValBUDID As New MemberScriptAndValue
				objScriptValBUDID.CubeName = sCube
				objScriptValBUDID.Script = BUDIDMemberScript
				objScriptValBUDID.TextValue = REQID
				objScriptValBUDID.IsNoData = False
				objListofScriptsBUDID.Add(objScriptValBUDID)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListofScriptsBUDID)
				
				' Update BUD Title For WF Scenario
				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = BUDTitleScr
				objScriptValTitle.TextValue = sBUDTitle
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)

				'Update BUD Creator
				Dim objListofScriptsUser As New List(Of MemberScriptandValue)
			    Dim objScriptValUser As New MemberScriptAndValue
				objScriptValUser.CubeName = sCube
				objScriptValUser.Script = BUDUserMemberScript
				objScriptValUser.TextValue = sBUDUser
				objScriptValUser.IsNoData = False
				objListofScriptsUser.Add(objScriptValUser)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsUser)
				
				'Update BUD Created Date
				Dim objListofScriptsDate As New List(Of MemberScriptandValue)
			    Dim objScriptValDate As New MemberScriptAndValue
				objScriptValDate.CubeName = sCube
				objScriptValDate.Script = BUDDateMemberScript
				objScriptValDate.TextValue = CurDate
				objScriptValDate.IsNoData = False
				objListofScriptsDate.Add(objScriptValDate)
				
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDate)

				'Update BUD Workflow Status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = BUDWFStatusMemberScript
				objScriptValWFStatus.TextValue = entityText3 & " Working"
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				Try
					Me.UpdateStatusHistory(si, globals, api, args, entityText3 & " Working",sFlow)
				Catch ex As Exception
				End Try
				Try
					Me.LastUpdated(si, globals, api, args, sFlow, sEntity)
				Catch ex As Exception
				End Try
				
				Return Nothing
			End Function				
#End Region

#Region "DeleteBUD: Delete BUD"
		'Updated EH 08/27/24 RMW-1565 to delete at BUD_Shared level
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function DeleteBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
									
			'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
			Dim sBUDToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")	
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			'Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim iBUDAmount As Integer = Nothing
			Dim sBUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow", sBUDToDelete)

			
			'Add entity and BUD flow to args
			args.NameValuePairs.Add("BUDEntity", sEntity)
			args.NameValuePairs.Add("BUD", sBUDToDelete)
			'Call method to clear Budget Funding Request Amount values
			Me.CallDataManagementDeleteBUD(si, globals, api, args)
				
			'Delete annotation from data Attachement table
			Dim deleteDataAttchSQL As String = "Delete from DataAttachment WHERE Cube = '" &  sCube & "' And Entity = '" & sEntity & "' And Scenario = '" & sScenario & "' And Flow = '" & sBUDToDelete & "'"
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
			For Each BUDAccount As MemberInfo In DeleteAnnotationAccounts
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & BUDAccount.Member.Name & ":F#" & sBUDToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						

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
			
			
			'Loops through accounts in an annotation member script at BUD_Shared scenario level
'			For Each BUDAccount As MemberInfo In DeleteAnnotationAccounts
'				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#BUD_Shared:T#1999:V#Annotation:A#" & BUDAccount.Member.Name & ":F#" & sBUDToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"						

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
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_POC_Name:F#" & sBUDToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & POCMbr.Member.Name & ":U6#None:U7#None:U8#None"										
				
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
			For Each BUDAccount As MemberInfo In DeletePeriodicAccounts
				Dim DeleteMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#" & BUDAccount.Member.Name & ":F#" & sBUDToDelete & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									
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
			  
#End Region

#Region "DeleteBUDAcctValue: Delete BUD Account Value"
		
		'RMW-1224: KN: Delete BUD Account Value for Dropdown Lists
		Public Function DeleteBUDAcctValue(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
		Try
				Dim sCmd As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account")
				Dim sValue As String = args.NameValuePairs.XFGetValue("Value")
				Dim SQL As New Text.StringBuilder
				SQL.Append($"DELETE FROM XFC_BUD_Name_Value_List WHERE BUD_ORGANIZATION = '{sCmd}' AND BUD_MEMBERNAME = '{sAccount}' AND BUD_VALUE = '{sValue}';") 
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "Delete Data Attachement"
		'This function runs a delete on the Data Attachement table fo the give entity and cycle
		Public Function	DeleteDataAttachement(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object	
		
			Dim scenario As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
			Dim cycle As String = scenario.Substring(5,4)
			Dim cube As String =BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).CubeName
			Dim entity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim REQIDsToDelete As String() = args.NameValuePairs.XFGetValue("REQIDsToDelete").Split(",")

			'Get the FYDP from the cycle
			Dim intYear = 0
			Integer.TryParse(cycle, intYear)
			
			Dim time As String = "T#" & cycle & ".Months,T#" & intYear + 1 & "M12" 
								  
			For Each DeleteREQ As String In REQIDsToDelete
				'delete cube data
				Dim params As New Dictionary(Of String, String) 
				params.Add("Cube", cube)
				params.Add("Entity", "E#" & entity)
				params.Add("Scenario", scenario)
				params.Add("Time", time )
				params.Add("Flow", DeleteREQ.Trim )
'brapi.ErrorLog.LogMessage(si, "Cube = " & Cube & ", entity = " & entity & ", scenario = " & scenario & ", time = " & time & ", Flow = " & DeleteREQ)				
				Dim result As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Delete_BUD", params)
				
				'Delete annotation from data Attachement table
				Dim deleteDataAttchSQL As String = "Delete from [dbo].[DataAttachment] WHERE Entity = '" &entity & "' And Scenario = '" & scenario & "' And Flow = '" & DeleteREQ.Trim & "'"
'brapi.ErrorLog.LogMessage(si, "deleteDataAttchSQL = " & deleteDataAttchSQL)				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn, deleteDataAttchSQL, True)
				End Using
				
				'Delete tem table
				Dim deleteTempTblSQL As String = "Delete From [dbo].[XFC_BUD_Import] Where Entity = '" & entity & "' And Fiscal_Year = '" & cycle & "' And Flow = '" & DeleteREQ.Trim & "'"
'brapi.ErrorLog.LogMessage(si, "deleteTempTblSQL = " & deleteTempTblSQL)				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 BRApi.Database.ExecuteSql(dbConn, deleteTempTblSQL, True)
				End Using
			Next

			Return Nothing
		End Function			
#End Region 

#Region "DemoteBUD: Demote BUD"
		'Updated 07252024 Mikayla RMW-1477
		'Updated 08292024 EH RMW-1565 sUFRTime updated to annual for BUD_C20XX scenario
		Public Function DemoteBUD(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
				Dim sComment As String = args.NameValuePairs.XFGetValue("demotionComment")
				'Demotion comment can't be blank
				If sComment = "" Then
					Throw New Exception ("You must enter a comment for demotion.")
				End If
				
				Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
				Dim sBUD As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")							
'brapi.ErrorLog.LogMessage(si,"sBUD = " & sBUD & ": sEntity = " & sEntity)
				'Set variables to be used in member script
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				
				
				'Create member script string to be used in the member script object
				Dim BUDWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"			
				Dim BUDdemoteMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Return_Cmt:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
				
				
				Dim currBUDWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDWFStatusMemberScript).DataCellEx.DataCellAnnotation				
				Dim currBUDWFStatusLvl As String = currBUDWFStatus.Substring(0,2)
				Dim sNewBUDWFStatus As String = ""
'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
'Set new workflow status variables for _General. _Gneral BUDs stay at same level when demoted
				If sEntity.XFContainsIgnoreCase("_General") And (currBUDWFStatusLvl = entityText3) Then
					sNewBUDWFStatus = $"Returned from {currBUDWFStatusLvl}"
				Else
				'Set new workflow status variables
					Dim nextwflevel As Integer = currBUDWFStatusLvl.substring(1,1) + 1
						sNewBUDWFStatus = $"L{nextwflevel} Returned from {currBUDWFStatusLvl}"
				End If			
				
'				'Set new workflow status variables
'				Dim currBUDWFStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDWFStatusMemberScript).DataCellEx.DataCellAnnotation				
'brapi.ErrorLog.LogMessage(si,"BUDWFStatusMemberScript: " & BUDWFStatusMemberScript &  ", currBUDWFStatus : " & currBUDWFStatus)			
'				Dim currBUDWFStatusLvl As String = currBUDWFStatus.Substring(0,2)
'				Dim nextwflevel As Integer = currBUDWFStatusLvl.substring(1,1) + 1
'				Dim sNewBUDWFStatus As String = $"L{nextwflevel} Returned from {currBUDWFStatusLvl}"
'brapi.ErrorLog.LogMessage(si,"sNewBUDWFStatus = " & sNewBUDWFStatus)
					'Validate that BUD Title is not empty
					If Not String.IsNullOrWhiteSpace(sBUD) Then
							
							'Update new BUD workflow status
							Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
						    Dim objScriptValWFStatus As New MemberScriptAndValue
							objScriptValWFStatus.CubeName = sCube
							objScriptValWFStatus.Script = BUDWFStatusMemberScript
							objScriptValWFStatus.TextValue = sNewBUDWFStatus
							objScriptValWFStatus.IsNoData = False
							objListofScriptsWFStatus.Add(objScriptValWFStatus)
							BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
							
							'Update the demotion comment
							Dim objListofScriptsDemotion As New List(Of MemberScriptandValue)
						    Dim objScriptValDemotion As New MemberScriptAndValue
							objScriptValDemotion.CubeName = sCube
							objScriptValDemotion.Script = BUDdemoteMemberScript
							objScriptValDemotion.TextValue = sComment
							objScriptValDemotion.IsNoData = False
							objListofScriptsDemotion.Add(objScriptValDemotion)					
							BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsDemotion)
						
					End If	
				
'						'Update BUD workflow status history
						Try						
							Me.UpdateStatusHistory(si, globals, api, args, sNewBUDWFStatus,sBUD,sEntity,sEntity)						
						Catch ex As Exception
						End Try
					
						'Send email
						Try
							Me.SendStatusChangeEmail(si, globals, api, args)
						Catch ex As Exception
						End Try
						
						'Update date and name
						Try
							Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
						Catch ex As Exception
						End Try
				
				Return Nothing
			End Function
#End Region

#Region "GetBUDID: Get REQ_ID"
		'This logic is the same as the logic in Create Req
		'***DEV TO DO the other function could be broken out to use it in both places
		Public Function GetBUDID(ByVal si As SessionInfo, ByVal sEntity As String,  ByVal sScenario As String,  ByVal sTitle As String) As Object
			Try
				'Check if the BUD exists per entity, scenario and title. If it does grab the flow.
				Dim sqlCheckExist As String = "Select distinct Flow,text  From DataAttachment 
									 WHERE Account = 'REQ_ID' and Entity =  '" & sEntity & "' And Scenario = '" & sScenario & "' and Text = '" & sTitle.Replace("'", "''") & "'"
'BRApi.ErrorLog.LogMessage(si,"REQ_ID sqlCheckExist = " & sqlCheckExist)

				Dim dtFetch As New DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtFetch = BRApi.Database.ExecuteSql(dbConn, sqlCheckExist, True)
				End Using

				If dtFetch.Rows.Count = 1  Then
					If  Not IsDBNull(dtFetch.Rows(0).Item("text")) Then
						Return dtFetch.Rows(0).Item("text")
					End If
				End If
				
				Dim SQL As String ="Select Max(Text) as ID From DataAttachment WHERE Account = 'REQ_ID' and Entity =  '" & sEntity & "'"
			
				Dim dtID As New DataTable()
				Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					dtID =BRApi.Database.ExecuteSql(dbConnApp,SQL,True)
				End Using
'BRApi.ErrorLog.LogMessage(si, "dtID count = " & dtID.Rows.Count)					
				Dim sBUDID As String = ""

				If dtID.Rows.Count = 0  Or IsDBNull( dtID.Rows(0).Item("ID")) Then
					Dim sEntityRemoveGen = sEntity.Replace("_General","")
					Dim sStratingIDNum As String = "00001"
					sBUDID = sEntityRemoveGen & "_" & sStratingIDNum
				Else

					Dim sDTBUDIDs As String = dtID.Rows(0).Item("ID")
					Dim sBUDIDfromDT As String = String.Join(",", sDTBUDIDs.ToArray())
					Dim iBUDIDNum As Integer = Convert.ToInt32(sBUDIDfromDT.split("_")(1))
					Dim sNewIDNum As Integer = iBUDIDNum + 1
					Dim sNewfullIDNum = sNewIDNum.ToString("D5")
			 		Dim sEntityRemoveGeneral = sEntity.Replace("_General","")
					sBUDID = sEntityRemoveGeneral & "_" & sNewfullIDNum
				End If
				
'BRApi.ErrorLog.LogMessage(si, "returned sBUDID = " & sBUDID)					
				Return sBUDID

				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try			 
			End Function
					
#End Region 

#Region "GetFlow: Get Flow"
		'For a given entity and scenario, it checks if the current title is in the data attachement table. If it is it is considered an update and the existing Flow ID is returned
		'If it is a new one, the logic will create one. A similar logic to the one in create reqquirement.
		'*** DEV TO DO The logic in create and here to generate the flow could be broken out to a function and be used from both places.
		Public Function	GetFlow(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal entity As String, ByVal title As String, ByVal usedFlows As List(Of String)) As String	
			Try
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = entity.Trim()
				
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				
				'Check if the BUD exists per entity, scenario and title. If it does grab the flow.
				Dim sqlCheckExist As String = "Select distinct Flow,text  From DataAttachment 
									 WHERE Account = 'REQ_Title' and Entity =  '" & entity & "' And Scenario = '" & sScenario & "' and Text = '" & title.Replace("'", "''") & "'"
'BRApi.ErrorLog.LogMessage(si,"Flow sqlCheckExist = " & sqlCheckExist)

				Dim dtFetch As New DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtFetch = BRApi.Database.ExecuteSql(dbConn, sqlCheckExist, True)
				End Using

				If dtFetch.Rows.Count = 1  Then
					If  Not IsDBNull(dtFetch.Rows(0).Item("Flow")) Then
						Return dtFetch.Rows(0).Item("Flow")
					End If
				End If

				Dim BUDCreated As Boolean = False
				
		        'Check if there is a BUD flow member available. If not create one	
				Dim BUDFlow As String = ""
				Dim ExistingBUDs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).OrderBy(Function(x) x.Member.name).ToList()
				
				Dim flowMemberExists As Boolean = False
				For Each ExistingBUD As MemberInfo In ExistingBUDs
					Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & ExistingBUD.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
			
				 	Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleScr).DataCellEx.DataCellAnnotation

	            	If String.IsNullOrWhiteSpace(TitleValue) And Not usedFlows.Contains(entity &":" & ExistingBUD.Member.Name) Then				
						flowMemberExists = True
						BUDFlow = ExistingBUD.Member.Name
						Exit For
					End If
				Next

				If Not flowMemberExists Then	
					BUDFlow = Me.CreateFlow(si, globals, api, New DashboardExtenderArgs())
				End If
				
'BRApi.ErrorLog.LogMessage(si, "BUDFlow : " &	BUDFlow)					
				Return BUDFlow
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 

#Region "GetSupportDocDataTableCV: Attach Doc"
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
'			SAG_APE
'			DollarType
'			CostCat
'			Cycle
'			COM_M1
'			COM_M2
'			COM_M3
'			COM_M4
'			COM_M5
'			COM_M6
'			COM_M7
'			COM_M8
'			COM_M9
'			COM_M10
'			COM_M12
'			COM_Carryover
'			OBL_M1
'			OBL_M2
'			OBL_M3
'			OBL_M4
'			OBL_M5
'			OBL_M6
'			OBL_M7
'			OBL_M8
'			OBL_M9
'			OBL_M10
'			OBL_M11
'			OBL_M12
'			OBL_Carryover
'			Title
'			Description
'			Justification
'			ImpactNotFunded
'			RiskNotFunded
'			CostMethodologyCmt
'			CostGrowthJustification
'			MustFund
'			ArmyInitiativeDirective
'			CommandInitiativeDirective
'			ActivityExercise
'			ITCyberReq
'			UIC
'			FlexField1
'			FlexField2
'			FlexField3
'			FlexField4
'			FlexField5
'			EmergingRequirement
'			CPACandidate
'			PBRSubmission
'			UPLSubmission
'			ContractNumber
'			TaskOrder
'			AwardTargetDate
'			POPExpirationDate
'			CME
'			COREmail
'			OwnerEmail
'			Directorate
'			Division
'			Branch
'			ReviewingPOCEmail
'			MDEPFuncEmail
'			NotificationEmailList
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
			
			Dim userFCs =  Me.GetUserSecurityFCs(si,"CR")
			If String.IsNullOrWhiteSpace(userFCs) Then
				Throw New XFUserMsgException(si, New Exception("User does not belong to a security group that is allowed to import"))
			End If
			
'BRApi.ErrorLog.LogMessage(si,userFCs)	
			Dim saUserFundCenters() As String = userFCs.Split(",")
		
			
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
			Dim REQs As List (Of BUD) = New List (Of BUD)
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
					'There needs to be 69 columns
					If fields.Length <> 69 Then
						Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 68, number columns in file header is "& fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 68, number columns in file header is "& fields.Length & vbCrLf & line ))
					End If
					
					firstLine = False
					Continue For
				End If
#End Region
		
#Region "Parse file"
				'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
'brapi.ErrorLog.LogMessage(si,"before parse req")
				Dim currREQ As BUD = Me.ParseREQ(si, fields)
'brapi.ErrorLog.LogMessage(si,"After parse req")
				'If EntityDescriptorType is a parent, add _General
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.Entity & ".base", True)
				If membList.Count > 1 Then
					currREQ.Entity = currREQ.Entity & "_General"
				End If
#End Region

#Region "Validate members"

				'validate req. If there is one error, then the file will be considered invalid and would not be loaded intot he cube
				'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				sScenario = "BUD_C" & currREQ.Cycle
				If Not sScenario.XFEqualsIgnoreCase(wfScenario) Then 
					Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is not targeting the current workflow year", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is not targeting the current workflow year"))
				End If
			
			
				Dim currREQVlidationRes As Boolean = False
				currREQVlidationRes = Me.ValidateMembers(si,currREQ,saUserFundCenters)
				If isFileValid Then 
					isFileValid = currREQVlidationRes
'BRApi.ErrorLog.LogMessage(si, "Error line : " & currREQ.StringOutput())					
				End If
								
				'Add REQ tot the REQ list
				REQs.Add(currREQ)
				
'BRApi.ErrorLog.LogMessage(si, "Added REQ =  " & currREQ.title & ", valid= " & currREQ.valid & ", msg= " & currREQ.validationError)				
			Next
#End Region

#Region "Populate table XFC_BUD_Import, Cube and Annotation"
			
			'Prior to starting to load data, clear pre-existing rows
			Dim SQLClearREQ As String = "Delete from XFC_BUD_Import  
											Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "' and UserName = '" & si.UserName & "'"

			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearREQ.ToString, False, True)
			End Using
		
			'Find the list of existing flows
			Dim ExistingREQs As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.Where(Name doesnotcontain REQ_00)",True).Select(Function(n) n.Member.Name).ToList()
			ExistingREQs.OrderBy(Function(x) convert.ToInt32(x.split("_")(1))).ToList()
			Dim currentUsedFlows As List(Of String) = Me.GetUsedFlows(si, wfCube, wfScenario)
			
			'loop through all parsed requirements list
			For Each currREQ As BUD In REQs
				Me.PopulateStageTable(si, globals, api, currREQ, isFileValid, ExistingREQs, currentUsedFlows)
				
				If isFileValid Then
					Me.PopulateAnnotation(si, currREQ)
				End If
			Next
			
			'Populate Cube
			If isFileValid Then
				Me.PopulateCube(si)
			End If 
			
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
				
				Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), review the column on the left titled ""ValidationError""."
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
			Dim currREQ As BUD = New BUD
			'Trim any unprintable character and surrounding quotes
			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
			Else 
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
			End If 
			currREQ.Entity = fields(0).Trim().Trim("""")
			currREQ.APPN = fields(1).Trim().Trim("""")
			currREQ.MDEP = fields(2).Trim().Trim("""")
			currREQ.SAG_APE = fields(3).Trim().Trim("""")
			currREQ.DollarType = fields(4).Trim().Trim("""")
			currREQ.CostCategory = fields(5).Trim().Trim("""")
			currREQ.Cycle = fields(6).Trim().Trim("""")
			currREQ.COM_M1 = Fields(7).Trim("""")
			currREQ.COM_M2 = Fields(8).Trim("""")
			currREQ.COM_M3 = Fields(9).Trim("""")
			currREQ.COM_M4 = Fields(10).Trim("""")
			currREQ.COM_M5 = Fields(11).Trim("""")
			currREQ.COM_M6 = Fields(12).Trim("""")
			currREQ.COM_M7 = Fields(13).Trim("""")
			currREQ.COM_M8 = Fields(14).Trim("""")
			currREQ.COM_M9 = Fields(15).Trim("""")
			currREQ.COM_M10 = Fields(16).Trim("""")
			currREQ.COM_M11 = Fields(17).Trim("""")
			currREQ.COM_M12 = Fields(18).Trim("""")
			currREQ.COM_Carryover = Fields(19).Trim("""")
			currREQ.OBL_M1 = Fields(20).Trim("""")
			currREQ.OBL_M2 = Fields(21).Trim("""")
			currREQ.OBL_M3 = Fields(22).Trim("""")
			currREQ.OBL_M4 = Fields(23).Trim("""")
			currREQ.OBL_M5 = Fields(24).Trim("""")
			currREQ.OBL_M6 = Fields(25).Trim("""")
			currREQ.OBL_M7 = Fields(26).Trim("""")
			currREQ.OBL_M8 = Fields(27).Trim("""")
			currREQ.OBL_M9 = Fields(28).Trim("""")
			currREQ.OBL_M10 = Fields(29).Trim("""")
			currREQ.OBL_M11 = Fields(30).Trim("""")
			currREQ.OBL_M12 = Fields(31).Trim("""")
			currREQ.OBL_Carryover = Fields(32).Trim("""")
			currREQ.Title = Fields(33).Trim().Trim("""")
			currREQ.Description = Fields(34).Trim().Trim("""")
			currREQ.Justification = Fields(35).Trim().Trim("""")
			currREQ.ImpactNotFunded = Fields(36).Trim().Trim("""")
			currREQ.RiskNotFunded = Fields(37).Trim().Trim("""")
			currREQ.CostMethodologyCmt = Fields(38).Trim().Trim("""")
			currREQ.CostGrowthJustification = Fields(39).Trim().Trim("""")
			currREQ.MustFund = Fields(40).Trim().Trim("""")
			currREQ.RequestedFundSource = Fields(41).Trim().Trim("""")
			currREQ.ArmyInitiativeDirective = Fields(42).Trim().Trim("""")
			currREQ.CommandInitiativeDirective = Fields(43).Trim().Trim("""")
			currREQ.ActivityExercise = Fields(44).Trim().Trim("""")
			currREQ.ITCyberReq = Fields(45).Trim().Trim("""")
			currREQ.UIC = Fields(46).Trim().Trim("""")
			currREQ.FlexField1 = Fields(47).Trim().Trim("""")
			currREQ.FlexField2 = Fields(48).Trim().Trim("""")
			currREQ.FlexField3 = Fields(49).Trim().Trim("""")
			currREQ.FlexField4 = Fields(50).Trim().Trim("""")
			currREQ.FlexField5 = Fields(51).Trim().Trim("""")
			currREQ.EmergingRequirement = Fields(52).Trim().Trim("""")
			currREQ.CPACandidate = Fields(53).Trim().Trim("""")
			currREQ.PBRSubmission = Fields(54).Trim().Trim("""")
			currREQ.UPLSubmission = Fields(55).Trim().Trim("""")
			currREQ.ContractNumber = Fields(56).Trim().Trim("""")
			currREQ.TaskOrder = Fields(57).Trim().Trim("""")
			currREQ.AwardTargetDate = Fields(58).Trim().Trim("""")
			currREQ.POPExpirationDate = Fields(59).Trim().Trim("""")
			currREQ.CME = Fields(60).Trim().Trim("""")
			currREQ.COREmail = Fields(61).Trim().Trim("""")
			currREQ.OwnerEmail = Fields(62).Trim().Trim("""")
			currREQ.Directorate = Fields(63).Trim().Trim("""")
			currREQ.Division = Fields(64).Trim().Trim("""")
			currREQ.Branch = Fields(65).Trim().Trim("""")
			currREQ.ReviewingPOCEmail = Fields(66).Trim().Trim("""")
			currREQ.MDEPFuncEmail = Fields(67).Trim().Trim("""")
			currREQ.NotificationEmailList = Fields(68).Trim().Trim("""")

			
			Return currREQ
			
		End Function
#End Region

#Region "Validate Members"		
		Public Function	ValidateMembers(ByVal si As SessionInfo, ByRef currREQ As BUD, ByVal saUserFundCenters As String()) As Object
			
			'validate fund code
			'This code leverages the way we validate in Data Source
			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
			
			Dim isFileValid As Boolean = True
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			Dim objDimPk As New DimPk
			
			If String.IsNullOrWhiteSpace(currREQ.title) Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Blank Title value in record"
			End If
			
			If wfProfileName.XFContainsIgnoreCase("CMD") Then
				'Validate that the Fund Center being loaded  s with in the command
				objDimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
				Dim currEntity As String = currREQ.Entity
				If Not membList.Exists(Function(x) x.Member.Name = currEntity) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
					'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
				End If
			Else
				
				Dim sFundCenter As String = currREQ.Entity
				If Not saUserFundCenters.Contains(sFundCenter.Replace("_General","")) Then 
					 If Not saUserFundCenters.Contains("Administrator") Then
						isFileValid = False
						currREQ.valid = False
						If currREQ.ValidationError <> "" Then currREQ.ValidationError &= vbCrlf 
						currREQ.ValidationError &= "Error: User does not have privileges to Funds Center: " & sFundCenter.Replace("_General","")
					End If
				End If 	
			End If 		
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.appn & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.appn
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			'Validation to check if Fund Code falls under a multi-year Appropiation
			Dim lParentMember As List(Of memberinfo) = New List(Of MemberInfo)
			lParentMember = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.appn & ".Parents", True)
			Dim u1ParentName As String = ""
			For Each u1Parent In  lParentMember
				u1ParentName = u1Parent.Member.Name
				Dim sU1ParentGen As String = u1ParentName & "_General"
				Dim sU1ParentGenID As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,sU1ParentGen)
				Dim sText2 = Brapi.Finance.UD.Text(si,dimtype.UD1.Id,sU1ParentGenID,2,Nothing, Nothing)
				If Not sText2.XFContainsIgnoreCase("Yes") And (Not String.IsNullOrEmpty(currREQ.COM_Carryover) Or Not String.IsNullOrEmpty(currREQ.COM_Carryover)) Then
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Fund Code is not part of a multi-year Appropiation: " & currREQ.appn
				End If 
			Next 
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & currREQ.MDEP & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
			Dim U3Name As String = currREQ.SAG_APE
'			Dim U3memSearch As String = "U3#BA.BASE.Where(Name Contains " & U3Name & " And  Name Contains " & U1ParentName & " )"
			Dim U3memSearch As String = $"U3#{U1ParentName}_{U3Name}"
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk ,U3memSearch ,True)
'			' check if 0 for  U3membList
'			If membList.Count = 0 Then
'				isFileValid = False
'				currREQ.valid = False
'				Throw New XFUserMsgException(si, New Exception("Error: Invalid APE value: " & currREQ.SAG_APE & " does Not exist"))
'			End If
'			Dim U3 As String = ""
''=============================================Hold off until BA decision===================================================						
'			'Check if there is multiple BA associated with APEPT
'			If membList.Count > 1 Then 'excluded BA 20 "Manpower"
'				For Each U3mbrInfo As MemberInfo In membList
'					U3 = U3mbrInfo.Member.Name
'					If U3.XFContainsIgnoreCase("_20_") Then
'						Continue For
'					Else
'						'Setting the non BA_20 U3 to CurrRowAPE_PT	
'						currREQ.SAG_APE = U3
'					End If	
'				Next
'			Else
'				U3 = membList.Item(0).Member.Name
'				'Setting CurrRowAPE_PT	
'				currREQ.SAG_APE = U3
'			End If	
''================================================================================================						
					
			If membList.Count <> 1 Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.SAG_APE & " does Not exist for APPN:" & U1ParentName
			Else
				currREQ.SAG_APE = membList.Item(0).Member.Name
			End If
			
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U4#" &currREQ. DollarType & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Dollar Type value: " & currREQ.DollarType
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CostCategory & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid ObjectClass value: " & currREQ.CostCategory
				
			End If
			
			
			'We determine the scenario from the cycle
			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sScenario = "BUD_C" & currREQ.Cycle
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
			If (membList.Count <> 1) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.Cycle

			Else
				currREQ.scenario = sScenario
			End If
			
			Return isFileValid
			
		End Function
#End Region

#Region "Populate Stage Table"		
		Public Function	PopulateStageTable(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByRef currREQ As BUD, ByVal isFileValid As Boolean,  ByRef ExistingREQs As List(Of String), ByRef currentUsedFlows As List(Of String)) As Object

			'get fcalcualted fields if file is valid. Else no need to create Flow as the records won't be loaded into the cube
			If(isFileValid) Then
				currREQ.flow = Me.GetFlow(si, globals, api, currREQ.command, currREQ.entity, currREQ.scenario, currREQ.Cycle, currREQ.title, currentUsedFlows, ExistingREQs)
				currREQ.REQ_ID = Me.GetREQID(si, globals, api, currREQ.entity, currREQ.scenario, currREQ.flow)
				'Add current flow to the list of used flows. This is used during the GetFlow process
				currentUsedFlows.Add(currREQ.entity & ":" & currREQ.flow)
			End If
			currREQ.UserName = 	si.UserName
			'insert into table
			Dim SQLInsert As String = "
			INSERT Into [dbo].[XFC_BUD_Import]
						([ValidationError]
						,[Command]
						,[Entity]
						,[APPN]
						,[MDEP]
						,[APE9]
						,[DollarType]
						,[ObjectClass]
						,[Fiscal_Year]
						,[COM_M1]
						,[COM_M2]
						,[COM_M3]
						,[COM_M4]
						,[COM_M5]
						,[COM_M6]
						,[COM_M7]
						,[COM_M8]
						,[COM_M9]
						,[COM_M10]
						,[COM_M11]
						,[COM_M12]
						,[COM_Carryover]
						,[OBL_M1]
						,[OBL_M2]
						,[OBL_M3]
						,[OBL_M4]
						,[OBL_M5]
						,[OBL_M6]
						,[OBL_M7]
						,[OBL_M8]
						,[OBL_M9]
						,[OBL_M10]
						,[OBL_M11]
						,[OBL_M12]
						,[OBL_Carryover]
						,[Title]
						,[Description]
						,[Justification]
						,[ImpactNotFunded]
						,[RiskNotFunded]
						,[CostMethodologyCmt]
						,[CostGrowthJustification]
						,[MustFund]
						,[RequestedFundSource]
						,[ArmyInitiativeDirective]
						,[CommandInitiativeDirective]
						,[ActivityExercise]
						,[ITCyberReq]
						,[UIC]
						,[FlexField1]
						,[FlexField2]
						,[FlexField3]
						,[FlexField4]
						,[FlexField5]
						,[EmergingRequirement]
						,[CPACandidate]
						,[PBRSubmission]
						,[UPLSubmission]
						,[ContractNumber]
						,[TaskOrder]
						,[AwardTargetDate]
						,[POPExpirationDate]
						,[CME]
						,[COREmail]
						,[OwnerEmail]
						,[Directorate]
						,[Division]
						,[Branch]
						,[ReviewingPOCEmail]
						,[MDEPFuncEmail]
						,[NotificationEmailList]
						,[REQ_ID]
						,[Flow]
						,[Scenario]
						,[UserName]
						)
		    VALUES
   				('" & currREQ.ValidationError.Replace("'", "''") & "','" &
					currREQ.Command.Replace("'", "''") & "','" &
					currREQ.Entity.Replace("'", "''") & "','" &
					currREQ.APPN.Replace("'", "''") & "','" &
					currREQ.MDEP.Replace("'", "''") & "','" &
					currREQ.SAG_APE.Replace("'", "''") & "','" &
					currREQ.DollarType.Replace("'", "''") & "','" &
					currREQ.CostCategory.Replace("'", "''") & "','" &
					currREQ.Cycle.Replace("'", "''") & "','" &
					currREQ.COM_M1.Replace("'", "''") & "','" &
					currREQ.COM_M2.Replace("'", "''") & "','" &
					currREQ.COM_M3.Replace("'", "''") & "','" &
					currREQ.COM_M4.Replace("'", "''") & "','" &
					currREQ.COM_M5.Replace("'", "''") & "','" &
					currREQ.COM_M6.Replace("'", "''") & "','" &
					currREQ.COM_M7.Replace("'", "''") & "','" &
					currREQ.COM_M8.Replace("'", "''") & "','" &
					currREQ.COM_M9.Replace("'", "''") & "','" &
					currREQ.COM_M10.Replace("'", "''") & "','" &
					currREQ.COM_M11.Replace("'", "''") & "','" &
					currREQ.COM_M12.Replace("'", "''") & "','" &
					currREQ.COM_Carryover.Replace("'", "''") & "','" &
					currREQ.OBL_M1.Replace("'", "''") & "','" &
					currREQ.OBL_M2.Replace("'", "''") & "','" &
					currREQ.OBL_M3.Replace("'", "''") & "','" &
					currREQ.OBL_M4.Replace("'", "''") & "','" &
					currREQ.OBL_M5.Replace("'", "''") & "','" &
					currREQ.OBL_M6.Replace("'", "''") & "','" &
					currREQ.OBL_M7.Replace("'", "''") & "','" &
					currREQ.OBL_M8.Replace("'", "''") & "','" &
					currREQ.OBL_M9.Replace("'", "''") & "','" &
					currREQ.OBL_M10.Replace("'", "''") & "','" &
					currREQ.OBL_M11.Replace("'", "''") & "','" &
					currREQ.OBL_M12.Replace("'", "''") & "','" &
					currREQ.OBL_Carryover.Replace("'", "''") & "','" &
					currREQ.Title.Replace("'", "''") & "','" &
					currREQ.Description.Replace("'", "''") & "','" &
					currREQ.Justification.Replace("'", "''") & "','" &
					currREQ.ImpactNotFunded.Replace("'", "''") & "','" &
					currREQ.RiskNotFunded.Replace("'", "''") & "','" &
					currREQ.CostMethodologyCmt.Replace("'", "''") & "','" &
					currREQ.CostGrowthJustification.Replace("'", "''") & "','" &
					currREQ.MustFund.Replace("'", "''") & "','" &
					currREQ.RequestedFundSource.Replace("'", "''") & "','" &
					currREQ.ArmyInitiativeDirective.Replace("'", "''") & "','" &
					currREQ.CommandInitiativeDirective.Replace("'", "''") & "','" &
					currREQ.ActivityExercise.Replace("'", "''") & "','" &
					currREQ.ITCyberReq.Replace("'", "''") & "','" &
					currREQ.UIC.Replace("'", "''") & "','" &
					currREQ.FlexField1.Replace("'", "''") & "','" &
					currREQ.FlexField2.Replace("'", "''") & "','" &
					currREQ.FlexField3.Replace("'", "''") & "','" &
					currREQ.FlexField4.Replace("'", "''") & "','" &
					currREQ.FlexField5.Replace("'", "''") & "','" &
					currREQ.EmergingRequirement.Replace("'", "''") & "','" &
					currREQ.CPACandidate.Replace("'", "''") & "','" &
					currREQ.PBRSubmission.Replace("'", "''") & "','" &
					currREQ.UPLSubmission.Replace("'", "''") & "','" &
					currREQ.ContractNumber.Replace("'", "''") & "','" &
					currREQ.TaskOrder.Replace("'", "''") & "','" &
					currREQ.AwardTargetDate.Replace("'", "''") & "','" &
					currREQ.POPExpirationDate.Replace("'", "''") & "','" &
					currREQ.CME.Replace("'", "''") & "','" &
					currREQ.COREmail.Replace("'", "''") & "','" &
					currREQ.OwnerEmail.Replace("'", "''") & "','" &
					currREQ.Directorate.Replace("'", "''") & "','" &
					currREQ.Division.Replace("'", "''") & "','" &
					currREQ.Branch.Replace("'", "''") & "','" &
					currREQ.ReviewingPOCEmail.Replace("'", "''") & "','" &
					currREQ.MDEPFuncEmail.Replace("'", "''") & "','" &
					currREQ.NotificationEmailList.Replace("'", "''") & "','" &
					currREQ.REQ_ID.Replace("'", "''") & "','" &
					currREQ.Flow.Replace("'", "''") & "','" &
					currREQ.Scenario.Replace("'", "''") & "','" &
					currREQ.UserName.Replace("'", "''") & "')"  
				
				
'BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
						
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
			End Using				
			Return Nothing
		End Function
#End Region

#Region "Populate Cube"		
		Public Function	PopulateCube(ByVal si As SessionInfo) As Object


					'---------- run data mgmt sequence ----------
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim sEntityList As String = ""
					Dim SQL_GetRows As String = "SELECT distinct entity FROM XFC_BUD_Import Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "' and UserName = '" & si.UserName & "'"
					Dim dict_Amt As New Dictionary(Of String, Long)
					Dim dtTableRows As DataTable
					
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 			dtTableRows = BRApi.Database.ExecuteSql(dbConn,SQL_GetRows,True)
					End Using
					
					For Each drEntity As DataRow In dtTableRows.Rows
						Dim sRow_Fundcenter As String = drEntity("Entity")
						If sEntityList = "" Then
							sEntityList = "E#" & sRow_Fundcenter 
						Else
							sEntityList &= ", E#" & sRow_Fundcenter
						End If
					Next

					Dim params As New Dictionary(Of String, String) 
					params.Add("EntityList", sEntityList)
					'params2.Add("EntityList", "E#A97AA_General, E#A97CC_General")

					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
					Dim objTAI2 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Import_Load_Cube", params)

					'system.Threading.Thread.Sleep(500)
					Dim objTaskActivityItem3 As TaskActivityItem = BRApi.TaskActivity.GetTaskActivityItem(si, objTAI2.UniqueID)
					If objTaskActivityItem3.HasError Then
						Throw New System.Exception("ERROR: Data Management Job = Load Cube failed.  Please check activity log for more details.")
					End If
					
					
					Return Nothing



		End Function
#End Region

#Region "Populate Annotations"		
		Public Function	PopulateAnnotation(ByVal si As SessionInfo, ByVal currREQ As BUD)As Object
			Dim sDataBufferPOVScript_POVLoop As String = "Cb#" & currREQ.command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":T#" & currREQ.Cycle & "M12:V#Annotation:I#None:F#"& currREQ.flow &":O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

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
											
				If Not String.IsNullOrWhiteSpace(currREQ.title) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Title"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.title
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.description) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Description"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.description
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If

				If Not String.IsNullOrWhiteSpace(currREQ.FlexField1) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_1"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField1
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If

				If Not String.IsNullOrWhiteSpace(currREQ.FlexField2) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_2"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField2
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If

				If Not String.IsNullOrWhiteSpace(currREQ.FlexField3) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_3"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField3
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If

				If Not String.IsNullOrWhiteSpace(currREQ.FlexField4) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_4"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField4
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.FlexField5) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Flex_Field_5"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.FlexField5
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.ITCyberReq) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_IT_Cyber_Rqmt_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ITCyberReq
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
				
				If Not String.IsNullOrWhiteSpace(currREQ.OwnerEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POC_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.OwnerEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
					
				If Not String.IsNullOrWhiteSpace(currREQ.ContractNumber) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Contract_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ContractNumber
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.POPExpirationDate) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_POP_Expiration_Date"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.POPExpirationDate
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
							
				If Not String.IsNullOrWhiteSpace(currREQ.CME) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FTE_CME"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CME
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If

				If Not String.IsNullOrWhiteSpace(currREQ.Justification) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Recurring_Justification"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Justification
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.CostMethodologyCmt) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Cost_Methodology_Cmt"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CostMethodologyCmt
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.ImpactNotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Impact_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ImpactNotFunded
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				If Not String.IsNullOrWhiteSpace(currREQ.RiskNotFunded) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Risk_If_Not_Funded"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RiskNotFunded
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
				
				If Not String.IsNullOrWhiteSpace(currREQ.RequestedFundSource) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Requested_Fund_Source"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.RequestedFundSource
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.ArmyInitiativeDirective) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Army_initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ArmyInitiativeDirective
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.CommandInitiativeDirective) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Command_Initiative_Directive"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CommandInitiativeDirective
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.ActivityExercise) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Activity_Exercise"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.ActivityExercise
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
				
				If Not String.IsNullOrWhiteSpace(currREQ.CPACandidate) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_CPA_Ind"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.CPACandidate
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
				
				If Not String.IsNullOrWhiteSpace(currREQ.TaskOrder) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Task_Order_Number"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.TaskOrder
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.AwardTargetDate) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Target_Date_Of_Award"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.AwardTargetDate
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.COREmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_COR_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.COREmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.Directorate) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Directorate"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.Directorate
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.Division) Then
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
				
				If Not String.IsNullOrWhiteSpace(currREQ.MDEPFuncEmail) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_MDEP_Func_Email"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.MDEPFuncEmail
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				End If
				
				If Not String.IsNullOrWhiteSpace(currREQ.NotificationEmailList) Then
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Notification_Email_List"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.NotificationEmailList
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
				
				'Set Last Updated by
				Dim sFilterScriptupdatedBy As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Last_Updated_Name"
			    Dim objScriptValUpdatedBy As New MemberScriptAndValue
				objScriptValUpdatedBy.CubeName = currREQ.command
				objScriptValUpdatedBy.Script = sFilterScriptupdatedBy
				objScriptValUpdatedBy.TextValue = sREQUser
				objScriptValUpdatedBy.IsNoData = False
				objListofScripts.Add(objScriptValUpdatedBy)	
				
				'Set Last updated date
				Dim sFilterScriptUpdatedTime As String = sDataBufferPOVScript_POVLoop & ":A#REQ_Last_Updated_Date"
			    Dim objScriptValUpdatedTime As New MemberScriptAndValue
				objScriptValUpdatedTime.CubeName = currREQ.command
				objScriptValUpdatedTime.Script = sFilterScriptUpdatedTime
				objScriptValUpdatedTime.TextValue = System.DateTime.Now.ToString()
				objScriptValUpdatedTime.IsNoData = False
				objListofScripts.Add(objScriptValUpdatedTime)	
				
				
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

'-----------MANPOWER
#Region "REQ Mass Import Manpower"

#Region "Import REQ Function Process Manpower"

		Public Function	ImportManpowerREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							

			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sFlow As String = args.NameValuePairs.XFGetValue("Flow")
			Dim sAPPN As String = sFlow.Replace("REQ_00_CP_","")
		
			'---get user fund centers for--				
			'---get user fund centers for--	
			Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
			Dim sUserName As String = objSignedOnUser.User.Name	
			Dim sUserFundCenters As String = ""
				
			Dim userFCs =  Me.GetUserSecurityFCs(si,"CR")
			If String.IsNullOrWhiteSpace(userFCs) Then
				Throw New XFUserMsgException(si, New Exception("User does not belong to a security group that is allowed to import"))
			End If

'BRApi.ErrorLog.LogMessage(si,userFCs)			
			Dim saUserFundCenters() As String = userFCs.Split(",")

					
#Region "Validate File structure Manpower"			
			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
'brapi.ErrorLog.LogMessage(si,"Filepath = " & filePath)
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
			Dim REQs As List (Of BUD) = New List (Of BUD)
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
					'There needs to be 22 columns
					If fields.Length <> 22 Then
						Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 22, number columns in file header is "& fields.Length & vbCrLf & line, "FAIL", objXFFileInfo.Name)
						Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " has invalid structure at line #" & iLineCount & ". Please check the file if its in the correct format. Expected number of columns is 22, number columns in file header is "& fields.Length & vbCrLf & line ))
					End If
					
					firstLine = False
					Continue For
				End If
#End Region
		
#Region "Parse file Manpower"
				'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
'brapi.ErrorLog.LogMessage(si,"before parse req")
				Dim currREQ As BUD = Me.ParseREQManpower(si, fields)
'brapi.ErrorLog.LogMessage(si,"After parse req")
				'If EntityDescriptorType is a parent, add _General
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.Entity & ".base", True)
				If membList.Count > 1 Then
					currREQ.Entity = currREQ.Entity & "_General"
				End If
#End Region


#Region "Validate members Manpower"

				'validate req. If there is one error, then the file will be considered invalid and would not be loaded intot he cube
				'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				sScenario = "BUD_C" & currREQ.Cycle
				If Not sScenario.XFEqualsIgnoreCase(wfScenario) Then 
					Me.REQMassImportFileHelper(si, globals, api, args, objXFFileInfo.Name & " is not targeting the current workflow year", "FAIL", objXFFileInfo.Name)
					Throw New XFUserMsgException(si, New Exception(objXFFileInfo.Name & " is not targeting the current workflow year"))
				End If
			
			
				Dim currREQVlidationRes As Boolean = False
				currREQVlidationRes = Me.ValidateMembersManpower(si,currREQ,saUserFundCenters,sAPPN)
				If isFileValid Then 
					isFileValid = currREQVlidationRes
'BRApi.ErrorLog.LogMessage(si, "Error line : " & currREQ.StringOutput())					
				End If
								
				'Add REQ tot the REQ list
				REQs.Add(currREQ)
				
'BRApi.ErrorLog.LogMessage(si, "Added REQ =  " & currREQ.title & ", valid= " & currREQ.valid & ", msg= " & currREQ.validationError)				
			Next
#End Region

#Region "Populate table XFC_BUD_Import, Cube and Annotation Manpower"
			
			'Prior to starting to load data, clear pre-existing rows
			Dim SQLClearREQ As String = "Delete from XFC_BUD_Manpower_Import  
											Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "' and UserName = '" & si.UserName & "'"
			Dim sBUDTimeParam As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim dataMgmtSeq As String = "Delete_BUD"     
			Dim params As New Dictionary(Of String, String) 
			params.Add("Entity", "E#" & args.NameValuePairs.XFGetValue("REQEntity"))
			params.Add("Scenario", wfScenario)
			params.Add("Flow", sFlow)
			params.Add("ClearText", "True")
			Dim intYear = 0
			Integer.TryParse(sBUDTimeParam, intYear)
			Dim time As String = ""
			time = "T#" & intYear & "M1,T#" & intYear &  "M2,T#" & intYear &  "M3,T#" & intYear &  "M4,T#" & intYear & "M5,T#" & intYear & "M6,T#" & intYear & "M7,T#" & intYear & "M8,T#" & intYear & "M9,T#" & intYear & "M10,T#" & intYear & "M11,T#" & intYear & "M12,T#" & intYear & "M1"
			params.Add("Time", "T#" & time)			
			BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
			
			
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLClearREQ.ToString, False, True)
			End Using
		
			'Find the list of existing flows
'			Dim ExistingREQs As List(Of String) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base.remove(REQ_00)",True).Select(Function(n) n.Member.Name).ToList()
'			ExistingREQs.OrderBy(Function(x) convert.ToInt32(x.split("_")(1))).ToList()
'			Dim currentUsedFlows As List(Of String) = Me.GetUsedFlows(si, wfCube, wfScenario)
Dim tStart1 As DateTime =  Date.Now()			
			'loop through all parsed requirements list
			For Each currREQ As BUD In REQs
				Me.PopulateStageTableManpower(si, globals, api, currREQ, isFileValid,sFlow)
				
				If isFileValid Then
					Me.PopulateAnnotationManpower(si, currREQ, sFlow)
				End If
			Next
'BRapi.ErrorLog.LogMessage(si, "populate stage/annotation took: " & Date.Now().Subtract(tStart1).TotalSeconds.ToString("0.0000"))			
			'Populate Cube
	
			If isFileValid Then
				Me.PopulateCubeManpower(si)
			End If 
		
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
				
				Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), review the column titled ""ValidationError""."
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

#Region "Import Helpers Manpower"

#Region "Parse REQ Manpower"
		'Parse a line into REQ object and return
		Public Function	ParseREQManpower(ByVal si As SessionInfo, ByVal fields As String())As Object
			'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
			Dim currREQ As BUD = New BUD
			'Trim any unprintable character and surrounding quotes
			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
			Else 
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
			End If 
			currREQ.Entity = fields(0).Trim().Trim("""")
			currREQ.APPN = fields(1).Trim().Trim("""")
			currREQ.MDEP = fields(2).Trim().Trim("""")
			currREQ.SAG_APE = fields(3).Trim().Trim("""")
			currREQ.DollarType = fields(4).Trim().Trim("""")
			currREQ.CostCategory = fields(5).Trim().Trim("""")
			currREQ.CivType = fields(6).Trim().Trim("""")
			currREQ.Cycle = fields(7).Trim().Trim("""")
			currREQ.OBL_M1 = Fields(8).Trim("""")
			currREQ.OBL_M2 = Fields(9).Trim("""")
			currREQ.OBL_M3 = Fields(10).Trim("""")
			currREQ.OBL_M4 = Fields(11).Trim("""")
			currREQ.OBL_M5 = Fields(12).Trim("""")
			currREQ.OBL_M6 = Fields(13).Trim("""")
			currREQ.OBL_M7 = Fields(14).Trim("""")
			currREQ.OBL_M8 = Fields(15).Trim("""")
			currREQ.OBL_M9 = Fields(16).Trim("""")
			currREQ.OBL_M10 = Fields(17).Trim("""")
			currREQ.OBL_M11 = Fields(18).Trim("""")
			currREQ.OBL_M12 = Fields(19).Trim("""")
			currREQ.FTE = fields(20).Trim("""")
			currREQ.Description = Fields(21).Trim().Trim("""")

			
			Return currREQ
			
		End Function
#End Region

#Region "Validate Members Manpower"		
		Public Function	ValidateMembersManpower(ByVal si As SessionInfo, ByRef currREQ As BUD, ByVal saUserFundCenters As String(),ByVal sAPPN As String) As Object
			
			'validate fund code
			'This code leverages the way we validate in Data Source
			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
			
			Dim isFileValid As Boolean = True
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			Dim objDimPk As New DimPk
			Dim APPNParent As String = ""
			Dim ParentmembList As List(Of memberinfo) = New List(Of MemberInfo)
			
			If wfProfileName.XFContainsIgnoreCase("CMD") Then
				'Validate that the Fund Center being loaded  s with in the command
				objDimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
				Dim currEntity As String = currREQ.Entity
				If Not membList.Exists(Function(x) x.Member.Name = currEntity) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
					'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
				End If
			Else
				Dim sFundCenter As String = currREQ.Entity
				If Not saUserFundCenters.Contains(sFundCenter.Replace("_General","")) Then 
					 If Not saUserFundCenters.Contains("Administrator") Then
						isFileValid = False
						currREQ.valid = False
						If currREQ.ValidationError <> "" Then currREQ.ValidationError &= vbCrlf 
						currREQ.ValidationError &= "Error: User does not have privileges to Funds Center: " & sFundCenter.Replace("_General","")
					End If
				End If 	
			End If 		
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.appn & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.appn
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			ParentmembList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.appn & ".Parents", True)
			For Each Parent In  ParentmembList
'BRapi.ErrorLog.LogMessage(si, $"sAPPN = {sAPPN} || Parent.Member.Name = {Parent.Member.Name}")
				If Not sAPPN.XFEqualsIgnoreCase(Parent.Member.Name) Then
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: FundCode in file does not match selected Appropriation: " & currREQ.appn
				End If 
			Next 
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & currREQ.MDEP & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
			Dim U3Name As String = currREQ.SAG_APE
			Dim U3memSearch As String = "U3#APPN.BASE.Where(Name Contains " & U3Name & " And  Name Contains " & sAPPN & " )"
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk ,U3memSearch ,True)
'BRApi.ErrorLog.LogMessage(si, $"U3memSearch = {U3memSearch} || membList.Count = {membList.Count}")			
			If membList.Count <> 1 Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.SAG_APE & " does Not exist for APPN:" & sAPPN
			Else
				currREQ.SAG_APE = membList.Item(0).Member.Name
			End If
'			Dim U3 As String = ""
''=============================================Hold off until BA decision===================================================						
'			'Check if there is multiple BA associated with APEPT
'			If membList.Count > 1 Then 'Use BA 20 "Manpower"
'				For Each U3mbrInfo As MemberInfo In membList
'					U3 = U3mbrInfo.Member.Name
'					If U3.XFContainsIgnoreCase("_20_") Then
'						currREQ.SAG_APE = U3					
'					Else
'						Continue For
'					End If	
'				Next
'			Else
'				U3 = membList.Item(0).Member.Name
'				If U3.XFContainsIgnoreCase("_20_") Then
'					'Setting CurrRowAPE_PT	
'					currREQ.SAG_APE = U3
'				End If
'			End If	
''================================================================================================						
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CostCategory & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid ObjectClass value: " & currREQ.CostCategory
				
			End If		
			
			'We determine the scenario from the cycle
			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sScenario = "BUD_C" & currREQ.Cycle
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
			If (membList.Count <> 1) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.Cycle

			Else
				currREQ.scenario = sScenario
			End If
			
			Return isFileValid
			
		End Function
#End Region

#Region "Populate Stage Table Manpower"		
		Public Function	PopulateStageTableManpower(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByRef currREQ As BUD, ByVal isFileValid As Boolean, ByVal sFlow As String) As Object

			'get calcualted fields if file is valid. Else no need to create Flow as the records won't be loaded into the cube
			If(isFileValid) Then
				currREQ.flow = sFlow
				currREQ.REQ_ID = currREQ.Entity & "_00000_CP_"

			End If
			currREQ.UserName = 	si.UserName
			'insert into table
			Dim SQLInsert As String = "
			INSERT Into [dbo].[XFC_BUD_Manpower_Import]
						([ValidationError]
						,[Command]
						,[Entity]
						,[APPN]
						,[MDEP]
						,[APE9]
						,[DollarType]
						,[ObjectClass]
						,[CType]
						,[Fiscal_Year]
						,[M1]
						,[M2]
						,[M3]
						,[M4]
						,[M5]
						,[M6]
						,[M7]
						,[M8]
						,[M9]
						,[M10]
						,[M11]
						,[M12]
						,[FTE]
						,[Remarks]
						,[REQ_ID]
						,[Flow]
						,[Scenario]
						,[UserName]
						)
		    VALUES
   				('" & currREQ.ValidationError.Replace("'", "''") & "','" &
					currREQ.Command.Replace("'", "''") & "','" &
					currREQ.Entity.Replace("'", "''") & "','" &
					currREQ.APPN.Replace("'", "''") & "','" &
					currREQ.MDEP.Replace("'", "''") & "','" &
					currREQ.SAG_APE.Replace("'", "''") & "','" &
					currREQ.DollarType.Replace("'", "''") & "','" &
					currREQ.CostCategory.Replace("'", "''") & "','" &
					currREQ.CivType.Replace("'", "''") & "','" &
					currREQ.Cycle.Replace("'", "''") & "','" &
					currREQ.OBL_M1.Replace("'", "''") & "','" &
					currREQ.OBL_M2.Replace("'", "''") & "','" &
					currREQ.OBL_M3.Replace("'", "''") & "','" &
					currREQ.OBL_M4.Replace("'", "''") & "','" &
					currREQ.OBL_M5.Replace("'", "''") & "','" &
					currREQ.OBL_M6.Replace("'", "''") & "','" &
					currREQ.OBL_M7.Replace("'", "''") & "','" &
					currREQ.OBL_M8.Replace("'", "''") & "','" &
					currREQ.OBL_M9.Replace("'", "''") & "','" &
					currREQ.OBL_M10.Replace("'", "''") & "','" &
					currREQ.OBL_M11.Replace("'", "''") & "','" &
					currREQ.OBL_M12.Replace("'", "''") & "','" &
					currREQ.FTE.Replace("'", "''") & "','" &
					currREQ.Description.Replace("'", "''") & "','" &
					currREQ.REQ_ID.Replace("'", "''") & "','" &
					currREQ.Flow.Replace("'", "''") & "','" &
					currREQ.Scenario.Replace("'", "''") & "','" &
					currREQ.UserName.Replace("'", "''") & "')"  
				
'BRApi.ErrorLog.LogMessage(si, "SQLInsert : " & SQLInsert)
						
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				BRAPi.Database.ExecuteActionQuery(dbConnApp, SQLInsert, False, True)
			End Using				
			Return Nothing
		End Function
#End Region

#Region "Populate Cube Manpower"		
		Public Function	PopulateCubeManpower(ByVal si As SessionInfo) As Object
		
					'---------- run data mgmt sequence ----------
					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim sEntityList As String = ""
					Dim SQL_GetRows As String = "SELECT distinct entity FROM XFC_BUD_Manpower_Import Where Command = '" & wfCube & "' and Scenario = '" & wfScenario & "' and UserName = '" & si.UserName & "'"
					Dim dict_Amt As New Dictionary(Of String, Long)
					Dim dtTableRows As DataTable
					
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 			dtTableRows = BRApi.Database.ExecuteSql(dbConn,SQL_GetRows,True)
					End Using
					
					For Each drEntity As DataRow In dtTableRows.Rows
						Dim sRow_Fundcenter As String = drEntity("Entity")
						If sEntityList = "" Then
							sEntityList = "E#" & sRow_Fundcenter 
						Else
							sEntityList &= ", E#" & sRow_Fundcenter
						End If
					Next

					Dim params As New Dictionary(Of String, String) 
					params.Add("EntityList", sEntityList)
					'params2.Add("EntityList", "E#A97AA_General, E#A97CC_General")

					'!!!!!DVLP NOTE: this is a synchronous call so it returns after job is done !!!!!
					Dim objTAI2 As TaskActivityItem =  BRApi.Utilities.ExecuteDataMgmtSequence(si, "Manpower_Import_Load_Cube", params)
					Return Nothing

		End Function
#End Region

#Region "Populate Annotations Manpower"		
		Public Function	PopulateAnnotationManpower(ByVal si As SessionInfo, ByVal currREQ As BUD, ByVal sFlow As String)As Object
				currREQ.flow = sFlow
				Dim sDataBufferPOVScript_POVLoop As String = "Cb#" & currREQ.command & ":E#" & currREQ.entity & ":C#Local:S#" & currREQ.scenario & ":T#" & currREQ.Cycle & "M12:V#Annotation:I#None:F#"& currREQ.flow &":O#BeforeAdj:U1#" & currREQ.APPN & ":U2#" & currREQ.MDEP & ":U3#" & currREQ.SAG_APE & ":U4#" & currREQ.DollarType & ":U5#" & currREQ.CivType & ":U6#" & currREQ.CostCategory & ":U7#None:U8#None"
				Dim txtValue As String = ""
				
				Dim objListofScripts As New List(Of MemberScriptandValue)
				
				Dim wfScenarioTypeID As Integer
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,currREQ.Cycle)
				wfScenarioTypeID = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si, dimtypeid.Scenario, currREQ.scenario)).Id
				
					Dim sFilterScript As String = sDataBufferPOVScript_POVLoop & ":A#REQ_FTE_Comment"
				    Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = currREQ.command
					objScriptVal.Script = sFilterScript
					objScriptVal.TextValue = currREQ.description
					objScriptVal.IsNoData = False
					objListofScripts.Add(objScriptVal)
				brapi.ErrorLog.LogMessage(si,"sFilterScript = " & sFilterScript & ": " & currREQ.description)
				
				
				'Set cell for all the fields
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				Return Nothing
		End Function
#End Region


#End Region
'-----------MANPOWER



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
		
#Region "LastUpdated: Last Updated"
		
		'Updates status history account with the passed in status in a comma delimited string
		'Updated: EH RMW-1565 9/4/24 Updated to annual for BUD_C20XX and BUD_Shared
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function LastUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "", Optional ByVal Entity As String =  "")
		Try
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim UFRTime As String = WFYear & "M12"
				Dim sUFRUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				Dim ufrEntity As String =  args.NameValuePairs.XFGetValue("updatedBUDEntity", Entity)
				Dim ufrFlow As String = args.NameValuePairs.XFGetValue("updatedBUDFlow", UFR)				

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
		
#End Region

#Region "ManageBUDStatusUpdated: Manage BUD Statuses Updated"		
		Public Function ManageBUDStatusUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal BUD As String =  "") As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
					
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim sBUD As String = args.NameValuePairs.XFGetValue("BUD", BUD)
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Args To pass Into dataset BR
			Dim dsArgs As New DashboardDataSetArgs 
			dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
			dsArgs.DataSetName = "BUDListByEntity"
			dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
			dsargs.NameValuePairs.XFSetValue("CubeView", "")
							
			'Call dataset BR to return a datatable that has been filtered by BUD status
			Dim dt As DataTable = BR_BUDDataset.Main(si, globals, api, dsArgs)
			
			For Each row As DataRow In dt.Rows
				Dim sEntity As String = row.Item("Value").Split(" ")(0)
				sBUD = row.Item("Value").Split(" ")(1)				
									
				Dim BUDStatusHistoryMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Status_History:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDCurrentStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			
				Dim StatusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusHistoryMemberScript).DataCellEx.DataCellAnnotation
				Dim CurrentStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDCurrentStatusMemberScript).DataCellEx.DataCellAnnotation
' BRApi.ErrorLog.LogMessage(si, $"BUDCurrentStatusMemberScript= {BUDCurrentStatusMemberScript} || CurrentStatus = {CurrentStatus}" )	
				'Dim LastHistoricalStatus As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
				Dim LastHistoricalEntry As String = StatusHistory.Substring(StatusHistory.LastIndexOf(",") + 1)
				Dim LastHistoricalStatus As String = LastHistoricalEntry.Substring(LastHistoricalEntry.LastIndexOf(":") + 1)
				'If Not String.compare(sFundCenter & " " & CurrentStatus, LastHistoricalStatus) = 0 Then
				If Not String.compare(CurrentStatus, LastHistoricalStatus) = 0 Then
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, CurrentStatus, sBUD, sEntity, sFundCenter)
				Catch ex As Exception
				End Try
				
				'Send email
				Try
'BRApi.ErrorLog.LogMessage(si,"Here Manage BUD Statuses Updated ")					
					Me.SendStatusChangeEmail(si, globals, api, args, sBUD, sFundCenter)
				Catch ex As Exception
				End Try			
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, sBUD, sFundCenter)
				Catch ex As Exception
				End Try
				
				End If 
			Next
			Return Nothing				
			
		End Function 
		
#End Region

#Region "SaveAllHelper: Save All Components Helper"
		
		'Triggered by Save All Components button to do various processes that need to also occur during the save
		'Updated EH 090424 RMW-1565 Updated to annual for BUD_C20XX
		Public Function SaveAllHelper(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "", Optional ByVal Entity As String =  "")
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name


			' Only do if workflow step name is Manage Budgets
			If wfProfileName.XFContainsIgnoreCase("Manage Budgets") Then
				Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
				Dim sDashboard As String = objDictionary("prompt_BUDPRO_BUDMGT_0CaAb_cbx_ContentList")
				
				' Only do if dashboard name is Administrative
				If sDashboard.Equals("_B__Administrative") Then
					Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
					Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim sEntity As String = GetEntity(si, args)
					Dim sBUDAdminPercentAmtMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime &":V#Periodic:A#REQ_Priority_Cat_Weight:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#UFR_Priority_Category:U6#None:U7#None:U8#None"
					Dim TotPct As Double = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sBUDAdminPercentAmtMemberScript).DataCellEx.DataCell.CellAmount
					If TotPct = 100 Then 
						Return Nothing
					Else
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = "Prioritization categories' total weight does not equal 100%."
						Return selectionChangedTaskResult

					End If
					Return Nothing
				End If
				Return Nothing
				
			End If
			Return Nothing
		End Function
		
#End Region

#Region "SaveGenAcctAnnotation: Save General Accounts Annotation Function"	
		'This function is used to save annotations for BUD inputs
		'RMW-1083 - updated to incorporate new structure for UD5 stakeholder members 
		'RMW-1565 EH 08/22/24 - Updated comment accounts to S#BUD_Shared, T#1999
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function SaveGenAcctAnnotation(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
					
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			'Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			
			'Passed In Parameters. Required to pass in null value if Account is not used.
			Dim sFlow As String = args.NameValuePairs.XFGetValue("BUDFlow").Trim
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity").Trim
			Dim sComment As String = args.NameValuePairs.XFGetValue("Comment")
			Dim sPOCComment As String = args.NameValuePairs.XFGetValue("POCComment")
			Dim sBUDAccount As String = args.NameValuePairs.XFGetValue("BUDAccount").Trim
			Dim sU5 As String = args.NameValuePairs.XFGetValue("U5").Trim
			Dim sPOCAcct As String = args.NameValuePairs.XFGetValue("POCAccount").Trim

		
			Dim sInfoMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & sBUDAccount & ":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sPOCMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & sPOCAcct & ":F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#None"
		
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
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, "Phased_Obligation_Base")
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
#End Region

#Region "SaveInitialAmount: Save Initial Amount"	
		'The Save initial amount function gets the sfunding line, amount and destination BUD Flow passed into it.
		'If there is no funding line and amount it will create a "none" funding line and amount 1.
		'Updated: EH RMW-1564 8/22/24 Updated to save amount at the year level for BUD_C20XX 
		'Updated:  by JM 3/27/25 to add new accounts for BUD
		
		Public Function SaveInitialAmount(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal BUDFlow As String) As Object
						
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sEntity As String = GetEntity(si, args)			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'BRApi.ErrorLog.LogMessage(si, "ScENARIO " & sScenario)			
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			'Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'BRApi.ErrorLog.LogMessage(si, "TIME " & sBUDTime)				
			  Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
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
				
			'On the initial creation of BUDs, the IC will be the creating entity
			Dim IC As String = sEntity

			
			' Write to Req_Requested amount for rounding used in Bud_Main_Calc 
			
			Dim BUDRequestedAmttMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#REQ_Requested_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommimentItem & ":U7#None:U8#None"
			Dim BUDRequestedAmttMemberScriptOblig As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#Phased_Obligation_Base:F#" & BUDFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommimentItem & ":U7#None:U8#None"
			Dim BUDRequestedAmttMemberScriptCommit As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#Phased_Commitment:F#" & BUDFlow & ":O#BeforeAdj:I#" & IC & ":U1#" & sAPPNFund & ":U2#" & sMDEP & ":U3#" & sAPE & ":U4#" & sDollarType & ":U5#None:U6#" & sCommimentItem & ":U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"sAPPNFund = {sAPPNFund} || sMDEP = {sMDEP} || sAPE = {sAPE}  || sDollarType {sDollarType}")
'BRApi.ErrorLog.LogMessage(si, "Amount = " & sRequestedAmt & ", Script = " & BUDRequestedAmttMemberScriptOblig)
'BRApi.ErrorLog.LogMessage(si, "Amount = " & sRequestedAmt & ", Script = " & BUDRequestedAmttMemberScriptCommit)
			'Update BUD Obligation Amount
			Dim objListofScriptsAmount As New List(Of MemberScriptandValue)
		    Dim objScriptValAmountOblig As New MemberScriptAndValue
			objScriptValAmountOblig.CubeName = sCube
			objScriptValAmountOblig.Script = BUDRequestedAmttMemberScriptOblig
			objScriptValAmountOblig.Amount = sRequestedAmt
			objScriptValAmountOblig.IsNoData = False
			objListofScriptsAmount.Add(objScriptValAmountOblig)

			'Update BUD Commitment Amount
		
		    Dim objScriptValAmountCommit As New MemberScriptAndValue
			objScriptValAmountCommit.CubeName = sCube
			objScriptValAmountCommit.Script = BUDRequestedAmttMemberScriptCommit
			objScriptValAmountCommit.Amount = sRequestedAmt
			objScriptValAmountCommit.IsNoData = False
			objListofScriptsAmount.Add(objScriptValAmountCommit)
			
			Dim objScriptValAmountREQAMT As New MemberScriptAndValue
			objScriptValAmountREQAMT.CubeName = sCube
			objScriptValAmountREQAMT.Script = BUDRequestedAmttMemberScript
			objScriptValAmountREQAMT.Amount = sRequestedAmt
			objScriptValAmountREQAMT.IsNoData = False
			objListofScriptsAmount.Add(objScriptValAmountREQAMT)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsAmount)	
			
			
			
			
			'Add entity and BUD flow to args
			
			Dim dataMgmtSeq As String = "BUD_FormulateREQ"     
			Dim params As New Dictionary(Of String, String) 
			params.Add("Entity", "E#" & sEntity)
			params.Add("Flow", BUDFlow)
			params.Add("IC", sEntity)
			params.Add("UD1", sAPPNFund)
			params.Add("UD2", sMDEP)
			params.Add("UD3", sAPE)
			params.Add("UD4", sDollarType)
			params.Add("UD6", sCommimentItem)
			
			BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
				
			

			Return Nothing
			
		End Function
	
#End Region

#Region "SendReviewRequestEmail: Send Review Request Email"
'------------------------------------------------------------------------------------------------------------
'Creator(04/08/2024): Kenny, Connor, Fronz
'
'Description: Sends a request-for-review email to the CMD role that is next in the Budgets life cycle
'
'Usage: SendReviewRequestEmail is called from the SendStatusChangeEmail function
	   'SendReviewRequestEmail uses BUDDataSet.GetAllUsers to return dt of user emails.			
Public Function SendReviewRequestEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal FundCenter As String, ByVal BUDid As String, ByVal BUDTitle As String, ByVal BUDCreatorName As String, ByVal BUDCreationDate As String ) As Object
		Try
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity",FundCenter)
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
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
			'Set args to return all user emails assigned to the resepective Fund Center & "Review Financials" security group 
			'Create email subject and body
			If wfProfileName = "Formulate Budget" Then
				DataSetArgs.NameValuePairs.XFSetValue("mode","FC_RF")
				'requestEmailSubject = "Request for Budget Financial Review"
				'requestEmailBody = "A Budget has been submitted for financial review for Fund Center: " & FundCenter & vbCrLf & " Budget: " & BUDid & " - " & BUDTitle & vbCrLf & "Created: " & BUDCreatorName & " - " & BUDCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
				requestEmailSubject = "Request for Budget Validation"
				requestEmailBody = "A Budget has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Budget: " & BUDid & " - " & BUDTitle & vbCrLf & "Created: " & BUDCreatorName & " - " & BUDCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
				
			End If
			'Set args to return all user emails assigned to the resepective Fund Center & "Validate Budgets" security group  (Commented out for now RMW-1283) 
			'Create email subject and body
			If wfProfileName = "Review Financials" Then
				validatorEmailsScript = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Validation_Email_List:F#" & BUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				validatorEmails  = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, validatorEmailsScript).DataCellEx.DataCellAnnotation
				requestEmailSubject = "Request for Budget Validation"
				requestEmailBody = "A Budget has been submitted to be reviewed and validated for prioritization for Fund Center: " & FundCenter & vbCrLf & " Budget: " & BUDid & " - " & BUDTitle & vbCrLf & "Created: " & BUDCreatorName & " - " & BUDCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & bodyDisclaimerBody
			End If
			
			'Call the BUDDataSet and return a datetable (dt) of users' emails
			Dim dtReviewUserEmails  As DataTable =  BR_BUDDataSet.GetAllUsers(si, globals ,api , DataSetArgs)
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
				For Each Email As String In vaReviewEmails			
			Next
			BRApi.Utilities.SendMail(si, EmailConnectorStr, vaReviewEmails, requestEmailSubject, requestEmailBody, Nothing)	
			End If
			
			Return Nothing	
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
End Function	
		
#End Region

#Region "SendStatusChangeEmail: Send Status Change Email "
	'Updated EH 08292024 RMW-1565 Updated sBUDTime to annual for BUD_C20XX and Centralized Text (BUD_Shared, 1999)
	'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
	Public Function SendStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal BUD As String =  "", Optional ByVal Entity As String =  "")
		'Usage: {UFR_SolutionHelper}{SendStkhldrEmail}{FundCenter=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_UserFundCenters__Shared!|],UFR=[|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|],StakeHolderEmails=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_StakeholderEmailList__Shared!|]}
		Try
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			'Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("BUDEntity", Entity)
			Dim sBUDid As String = args.NameValuePairs.XFGetValue("BUDFlow", BUD)	
			Dim userName As String = si.UserName

			'Title Member Script
			Dim BUDEntityTitleMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & sBUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sBUDTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEntityTitleMemberScript).DataCellEx.DataCellAnnotation
			
			'Status Member Script
			Dim BUDStatusMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusMemberScript).DataCellEx.DataCellAnnotation
			
			'Creator Name Member Script
			Dim BUDEntityCreatorNameMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creator_Name:F#" & sBUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sBUDCreatorName As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEntityCreatorNameMemberScript).DataCellEx.DataCellAnnotation
			
			'Creation Data Member Script
			Dim BUDEntityCreationDateMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & sBUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim sBUDCreationDate As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEntityCreationDateMemberScript).DataCellEx.DataCellAnnotation	

			'Creation Data Member Script
			Dim BUDEmailNotificationMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Notification_Email_List:F#" & sBUDid & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
            Dim BUDStatusEmailList As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEmailNotificationMemberScript).DataCellEx.DataCellAnnotation	
'Brapi.ErrorLog.LogMessage(si, "BUDEmailNotificationMemberScript: " & BUDEmailNotificationMemberScript)
'Brapi.ErrorLog.LogMessage(si, "BUDStatusEmailList: " & BUDStatusEmailList)

			'Variables to set up email functionality 
			Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
			Dim BodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
			Dim StatusChangeEmails  As New List(Of String) 
			StatusChangeEmails.AddRange(BUDStatusEmailList.Split(","))
			'//Status Change Email\\
			Dim statusChangeSubject As String = "Budget Status Change"
			'Build Body
			Dim statusChangeBody As String = "A Budget Request for Fund Center: " & sFundCenter & " with Budget Title: "  & sBUDid & " - " & sBUDTitle &  " has changed status to '" & sBUDStatus & "' " & vbCrLf & "Submitted by: " & userName & " - " & sBUDCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & BodyDisclaimerBody
			'Send email			
			If Not String.IsNullOrWhiteSpace(BUDStatusEmailList) Then
'Brapi.ErrorLog.LogMessage(si, "hits the send: " & EmailConnectorStr)
				BRApi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmails, statusChangeSubject, statusChangeBody, Nothing)	
			End If
			'Executes the "SendReviewRequestEmail" function and sends an email to the resepective CMD roles within it
			If sBUDStatus = "Ready for Financial Review" Or sBUDStatus = "Ready for Validation"
				Me.SendReviewRequestEmail(si, globals, api, sFundCenter, sBUDid, sBUDTitle, sBUDCreatorName, sBUDCreationDate)
			End If			
		Return Nothing
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
	End Function

#End Region

#Region "SetNotificationList: Set Notification List"

	Public Function SetNotificationList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
	'Added a section to show only validators on the list and seperated from all users - 5-30-24
	
		Try
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","") 'args.NameValuePairs.XFGetValue("UFREntity")
			Dim sBUD As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")			
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("BUDScenario")
			Dim sBUDTime As String = args.NameValuePairs.XFGetValue("BUDTime")
			Dim notificationEmails As String = args.NameValuePairs.XFGetValue("Emails")
			Dim vNotificationEmails As String = args.NameValuePairs.XFGetValue("vEmails")
				
			If notificationEmails.Length = 0 And vNotificationEmails.Length = 0 Then 
				Return Nothing
			End If
			
			Dim emailList As String() = notificationEmails.split(",")
			Dim vemailList As String() = vNotificationEmails.split(",")

			Dim notificationEmailsScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Notification_Email_List:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim notificationValidatorEmailsScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Validation_Email_List:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		
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
			
			'Update related BUD List
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
					Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
				Catch ex As Exception
				End Try
				
			Return Nothing

		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try 
	End Function

#End Region

#Region "SetRelatedBUDs: Set Related BUDs"
'Updated by Fronz 09/06/2024 - changed the S# to BUD_Shared and T# to 1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
	Public Function SetRelatedBUDs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","") 'args.NameValuePairs.XFGetValue("BUDEntity")
			Dim sBUD As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUD","")	
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("BUDScenario")
			Dim sBUDTime As String = args.NameValuePairs.XFGetValue("BUDTime")
'			Dim sScenario As String = "BUD_Shared"
'			Dim sBUDTime As String = "1999"
			Dim sRelatedBUDs As String = args.NameValuePairs.XFGetValue("RelatedBUDs")
			Dim RelatedBUDLength As Integer = sRelatedBUDs.Length
			Dim BUDList As String() = sRelatedBUDs.Split(","c).Select(Function(BUD) BUD.Trim()).ToArray()
'brapi.ErrorLog.LogMessage(si,"BUDList = " & RelatedBUDLength)
			If RelatedBUDLength = 0 Then 
				Return Nothing
			End If
'brapi.ErrorLog.LogMessage(si,"BUD = " & sBUD)			
			Dim BUDtitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDtitleMemberScript).DataCellEx.DataCellAnnotation
			Dim mainBUD As String = ""
			If sEntity.XFContainsIgnoreCase("_General") Then 
				mainBUD = $"{sEntity.Replace("_General","")} - {sBUD} - {TitleValue}"		
			Else
				mainBUD = $"{sEntity} - {sBUD} - {TitleValue}"
			End If
			
			Dim RelatedBUDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Related_Request:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'brapi.ErrorLog.LogMessage(si,"RelatedBUDMemberScript = " & RelatedBUDMemberScript)			
			Dim currentBUDs As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, RelatedBUDMemberScript).DataCellEx.DataCellAnnotation

			For Each BUD As String In BUDList
				If BUD.XFContainsIgnoreCase(mainBUD) Then Continue For
				Dim isGeneral As Boolean = BUD.Split("-")(0).Trim().XFContainsIgnoreCase("_General")
				If isGeneral Then BUD = BUD.Replace(BUD.Split("-")(0).Trim(),BUD.Split("-")(0).Trim().Replace("_General",""))
				If (Not currentBUDs.XFContainsIgnoreCase(BUD)) Then
					If (String.IsNullOrWhiteSpace(currentBUDs)) Then				
						currentBUDs = BUD
					Else
						currentBUDs = currentBUDs & ", " & BUD	
					End If
				End If
			
			Next
'brapi.ErrorLog.LogMessage(si,"currentBUDs = " & currentBUDs)						
           'Update related BUD List
			Dim objListofScripts As New List(Of MemberScriptandValue)
		    Dim objScriptVal As New MemberScriptAndValue
			
			objScriptVal.CubeName = sCube
			objScriptVal.Script = RelatedBUDMemberScript
			objScriptVal.TextValue = currentBUDs
			objScriptVal.IsNoData = False
			objListofScripts.Add(objScriptVal)
			
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
			
			'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
				Catch ex As Exception
				End Try
			
			'Update the other side - set the relate BUDs
			
			For Each relatedBUD As String In BUDList
				If relatedBUD.XFContainsIgnoreCase(mainBUD) Then Continue For
				Dim BUD = relatedBUD.Split("-")(1).Trim()
				sEntity = relatedBUD.Split("-")(0).Trim()
				RelatedBUDMemberScript = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Related_Request:F#" & BUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				currentBUDs = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, RelatedBUDMemberScript).DataCellEx.DataCellAnnotation
				If (Not currentBUDs.XFContainsIgnoreCase(mainBUD)) Then
					If (String.IsNullOrWhiteSpace(currentBUDs)) Then
						currentBUDs = mainBUD
					Else
						currentBUDs = currentBUDs & ", " & mainBUD	
					End If
				End If

				objScriptVal.CubeName = sCube
				objScriptVal.Script = RelatedBUDMemberScript
				objScriptVal.TextValue = currentBUDs
				objScriptVal.IsNoData = False
				objListofScripts.Add(objScriptVal)
			
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScripts)
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, BUD, sEntity)
				Catch ex As Exception
				End Try
			Next
		Return Nothing
	End Function

#End Region

#Region "ShowAndHideDashboards: Show and Hide"
		
		Public Function ShowAndHideDashboards(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Dim checkForBlank As String = args.NameValuePairs.XFGetValue("checkForBlank")
				'blank dashboard set to trueVale
				Dim trueValue As String = args.NameValuePairs.XFGetValue("trueValue")
				'dashboard content to show (eventually) set to falseValue
				Dim falseValue As String = args.NameValuePairs.XFGetValue("falseValue")
				Dim allTimeValue As String = args.NameValuePairs.XFGetValue("allTimeValue")
				'cache prompts for Linked Report
				Dim sDb As String = args.NameValuePairs.XFGetValue("Db","")
				If Not String.IsNullOrWhiteSpace(sDb) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"LR_BUD_Prompts","Db",sDb)
				Dim sFC As String = args.NameValuePairs.XFGetValue("FundCenter","")
				If Not String.IsNullOrWhiteSpace(sFC) Then BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"LR_BUD_Prompts","FC",sFC)
'BRApi.ErrorLog.LogMessage(si, "sDb: " & sDb)				
				Dim toShow As String = ""
				Dim toHide As String = ""
'BRApi.ErrorLog.LogMessage(si, "checkForBlank: " & checkForBlank)
'BRApi.ErrorLog.LogMessage(si, "truevale: " & trueValue)
'BRApi.ErrorLog.LogMessage(si, "falsevalue: " & falseValue)
'BRApi.ErrorLog.LogMessage(si, "allTimeValue: " & allTimeValue)
				If (String.IsNullOrWhiteSpace(allTimeValue)) Then
					If (String.IsNullOrWhiteSpace(checkForBlank)) Then
'BRApi.ErrorLog.LogMessage(si, "show blnkDB IF statement; no titlebox selection:")
						toShow =  trueValue
						toHide =  falseValue
					Else
'BRApi.ErrorLog.LogMessage(si, "show contentDB Else statement; does not contain IsNullOrWhiteSpace:")
						toShow =  falseValue
						toHide =  trueValue
'BRApi.ErrorLog.LogMessage(si,"toShow: " & toShow & ", toHide: " & toHide )
					End If
				Else ' If all time is set, the check is bypassed
'BRApi.ErrorLog.LogMessage(si, "Last Else statement - 1146")
					toShow =  trueValue
					toHide =  falseValue
				End If
'BRApi.ErrorLog.LogMessage(si, "allTimeValue: " & allTimeValue & " checkForBlank: " & checkForBlank & ", trueValue: " & trueVale & " , falseValue: " & falseValue)
				
'BRApi.ErrorLog.LogMessage(si,"Line 4811" )

				Dim objXFSelectionChangedUIActionInfo As XFSelectionChangedUIActionInfo = args.SelectionChangedTaskInfo.SelectionChangedUIActionInfo
				objXFSelectionChangedUIActionInfo.DashboardsToHide = toHide
				objXFSelectionChangedUIActionInfo.DashboardsToShow = toShow
				
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
'BRApi.ErrorLog.LogMessage(si,"toShow: " & toShow)			
				Return selectionChangedTaskResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "SubmitBUD: Submit BUD"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484	
'Updated: EH - 08/28/2024 - Ticket 1565. WFYear set to annual
		Public Function SubmitFCAppnREQs() As Object
			Try
			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim BUDTime As String = WFYear & "M12"
				'Dim BUDTime As String = WFYear
				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
				Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
			
				'-------Get current BUD workflow status-------
				Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim currBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
				Dim newBUDStatus As String =""
'Brapi.ErrorLog.LogMessage(si,"mem Script : " & BUDMemberScript)
				Dim isBlank As Boolean = String.IsNullOrWhiteSpace(currBUDStatus)
				Dim isStatusFormulate As Boolean = currBUDStatus.Contains("Working") Or currBUDStatus.Contains("Copied")

				If isBlank Or Not isStatusFormulate Then
					Return Nothing
				End If
			'Added If Statement To account for creating on the parent level
				If BUDEntity.XFContainsIgnoreCase("_General") Then 
					currBUDStatus = currBUDStatus.Substring(1,1)
					Dim icurrBUDStatus As Integer = currBUDStatus
					Dim valLevel As String = icurrBUDStatus
					newBUDStatus = "L" & valLevel & " Ready for Validation"
				Else
				'-------Set new BUD workflow status-------
					currBUDStatus = currBUDStatus.Substring(1,1)
					Dim icurrBUDStatus As Integer = currBUDStatus
					Dim inewBUDStatus = icurrBUDStatus - 1
					Dim valLevel As String = inewBUDStatus
					newBUDStatus  = "L" & valLevel & " Ready for Validation"
				End If
			
                'Update BUD workflow Status
				Dim objListofScriptStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValStatus As New MemberScriptAndValue
				objScriptValStatus.CubeName = wfCube
				objScriptValStatus.Script = BUDMemberScript
				objScriptValStatus.TextValue = newBUDStatus
				objScriptValStatus.IsNoData = False
				objListofScriptStatus.Add(objScriptValStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)					
			
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus)
				Catch ex As Exception
				End Try
		
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, BUDFlow, BUDEntity)
				Catch ex As Exception
				End Try
				
				'Hide db
'				Try
'					args.NameValuePairs.XFSetValue("allTimeValue","True")
'					args.NameValuePairs.XFSetValue("trueValue","BUDPRO_BLNKDB_BlankDashboard")
'					Me.ShowAndHideDashboards(si, globals, api, args)
'				Catch ex As Exception
'				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region

#Region "SubmitForApproval: Submit for Approval"	
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
'Updated: EH RMW-1564 9/3/24 Changed to annual for BUD_C20XX and Title member script to S#BUD_Shared:T#1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function SubmitForApproval(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim BUDList As String = ""
			Dim sEntity As String = ""
			Dim sBUD As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
	
			'Validate for Prioritization Allowance Status	
			If Not Me.IsBUDPrioritizationAllowed(si, sFundCenter) Then
				Throw  New Exception("Cannot prioritize BUD at this time. Contact BUD manager." & environment.NewLine)
			End If
	
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")
'BRApi.ErrorLog.LogMessage(si, $"dt.Rows.Count = {dt.Rows.Count}")			
			If dt.Rows.Count = 0 Then Return Nothing
			'Loops and update status
			For Each row As DataRow In dt.Rows
	
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sBUD = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")	
'BRApi.ErrorLog.LogMessage(si, $"sEntity = {sEntity} | sBUD = {sBUD}")			    
				Dim BUDRankOverrideMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Periodic:A#REQ_Priority_Override_Rank:F#" & sBUD & ":O#BeforeAdj:I#" & sFundCenter & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDTitleMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDTitleMemberScript).DataCellEx.DataCellAnnotation
				Dim RankOverride As DataCell = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDRankOverrideMemberScript).DataCellEx.DataCell
				Dim BUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusMemberScript).DataCellEx.DataCellAnnotation
					
'brapi.ErrorLog.LogMessage(si,"HERE CM")							
				If BUDStatus.XFcontainsIgnoreCase("Prioritized") Then
					'Or BUDStatus.XFEqualsIgnoreCase("CMD Prioritized")
					If RankOverride.Cellstatus.IsNoData Then
						'brapi.ErrorLog.LogMessage(si, RankOverride.Cellstatus)
						BUDList = sBUD & " - " & TitleValue & environment.NewLine & BUDList									
					End If 																		
				End If 					
			Next	
			
			If BUDList.xfcontainsignorecase("BUD") Then
				Throw New Exception("Must enter a Ranked Override Value for: " & environment.NewLine & BUDList)
				Return Nothing
			End If
			
			'Loops again and update status
			Dim newBUDStatus As String = ""
			For Each row As DataRow In dt.Rows

				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sBUD = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")		
			
			    Dim BUDStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusMemberScript).DataCellEx.DataCellAnnotation
				Dim currBUDLevel As String = BUDStatus.Substring(0,2)
				If BUDStatus.XFcontainsIgnoreCase("Prioritized") Then
						
					newBUDStatus = currBUDLevel & " Ready for Approval"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = BUDStatusMemberScript
					objScriptValWFStatus.TextValue = newBUDStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
						'update Status History
						Try
							Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus, sBUD, sEntity, sFundCenter)
							Catch ex As Exception
						End Try
						
						'Send email
						Try
							Me.SendStatusChangeEmail(si, globals, api, args, sBUD)
							Catch ex As Exception
						End Try
						
						'Set Updated Date and Name
						Try
							Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
							Catch ex As Exception
						End Try
				Else If BUDStatus.XFEqualsIgnoreCase("L2 Prioritized") Then	
					newBUDStatus = "L2 Ready for Approval"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = BUDStatusMemberScript
					objScriptValWFStatus.TextValue = newBUDStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
						'update Status History
						Try
							Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus, sBUD, sEntity, sFundCenter)
							Catch ex As Exception
						End Try
						
						'Send email
						Try
							Me.SendStatusChangeEmail(si, globals, api, args, sBUD)
							Catch ex As Exception
						End Try
						
						'Set Updated Date and Name
						Try
							Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
							Catch ex As Exception
						End Try
				
				End If 
					
					Next 					
					
			Return Nothing 
			
		End Function 
		
#End Region

#Region "SubmitToCommand: Submit to Command"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
'Updated: EH RMW-1564 9/3/24 Changed sBUDTime annual for BUD_C20XX and Title member script to S#BUD_Shared:T#1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
'Updated: KN RMW-1717 9/30/24 Updated to allow submission by filters
		Public Function SubmitToCommand(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Boolean
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			'Dim wfLevel As String = wfProfileName.Substring(0,2)
			Dim BUDList As String = ""
			Dim sEntity As String = ""
			Dim sBUD As String = ""
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
						
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")

			'Loops and update status
			For Each row As DataRow In dt.Rows
				sEntity = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
				sBUD = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")				
			    Dim BUDStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & sBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDStatusMemberScript).DataCellEx.DataCellAnnotation
			'----------set new BUD workflow status---------
				Dim currBUDLevel As String = BUDStatus.Substring(1,1)
				Dim icurrBUDLevel As Integer = currBUDLevel
				Dim snewBUDlevel As String = "L" & icurrBUDLevel - 1	
				If BUDStatus.XFContainsIgnoreCase("Ready for Approval") Then					
					Dim newBUDStatus As String = snewBUDlevel & " Ready for Validation"
					Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    	Dim objScriptValWFStatus As New MemberScriptAndValue
					objScriptValWFStatus.CubeName = sCube
					objScriptValWFStatus.Script = BUDStatusMemberScript
					objScriptValWFStatus.TextValue = newBUDStatus
					objScriptValWFStatus.IsNoData = False
					objListofScriptsWFStatus.Add(objScriptValWFStatus)
						
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
						
						'update Status History
						Try
							Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus, sBUD, sEntity, sFundCenter)
							Catch ex As Exception
						End Try
						
						'Send email
						Try
							Me.SendStatusChangeEmail(si, globals, api, args, sBUD)
							Catch ex As Exception
						End Try
						
						'Set Updated Date and Name
						Try
							Me.LastUpdated(si, globals, api, args, sBUD, sEntity)
							Catch ex As Exception
						End Try
				
					End If 
					
				Next
'BRApi.ErrorLog.LogMessage(si,"Debug C")					
			Return Nothing
		End Function
#End Region

#Region "UpdateManpowerBUDStatus: UpdateManpowerBUDStatus"

			Public Function UpdateManpowerBUDStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
				Dim sBUDTitle As String = "Manpower Budget"
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sBUDUser As String = si.UserName
				Dim CurDate As Date = Datetime.Now
				Dim sBUDStatus As String = ""
				
				Dim BUDIDMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_ID:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDTitleScr As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"							
				Dim BUDWFStatusMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDUserMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creator_Name:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim BUDDateMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Creation_Date_Time:F#REQ_00:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
				Dim sCurrTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDWFStatusMemberScript).DataCellEx.DataCellAnnotation
				If sCurrTitle.XFEqualsIgnoreCase("L2 Working") Then 
					sBUDStatus = "L2 Approved"
				End If 
				If sCurrTitle.XFEqualsIgnoreCase("L2 Approved") Then 
					sBUDStatus = "L2 Working"
				End If

				'Update BUD Workflow Status
				Dim objListofScriptsWFStatus As New List(Of MemberScriptandValue)
			    Dim objScriptValWFStatus As New MemberScriptAndValue
				objScriptValWFStatus.CubeName = sCube
				objScriptValWFStatus.Script = BUDWFStatusMemberScript
				objScriptValWFStatus.TextValue = sBUDStatus
				objScriptValWFStatus.IsNoData = False
				objListofScriptsWFStatus.Add(objScriptValWFStatus)
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsWFStatus)
				
				
				Try
					Me.UpdateStatusHistory(si, globals, api, args, sBUDStatus,"REQ_00")
				Catch ex As Exception
				End Try
				Try
					Me.LastUpdated(si, globals, api, args, "REQ_00", sEntity)
				Catch ex As Exception
				End Try
				
			Return Nothing
			End Function				
#End Region

#Region "UpdateStatusHistory: Update Status History"
		
		'Updates status history account with the passed in status in a comma delimited string
		'Updated EH 08292024 RMW-1565 Updated to annual for BUD_C20XX scenario
		'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
		Public Function UpdateStatusHistory(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal status As String =  "", Optional ByVal BUD As String =  "", Optional ByVal Entity As String =  "", Optional ByVal FundCenter As String =  "") As String
		Try			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim WFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)
				Dim BUDTime As String = WFYear & "M12"
				'Dim BUDTime As String = WFYear
		
				'Original code
				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity", Entity)
				Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow", BUD)
				Dim reqFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter", FundCenter)
'brapi.ErrorLog.LogMessage(si,"reqFundCenter " & reqFundCenter)
				Dim reqstatus As String = args.NameValuePairs.XFGetValue("status", status)			
				'reqstatus = reqFundCenter & " " & reqstatus
'brapi.ErrorLog.LogMessage(si,"reqstatus " & reqstatus)
				Dim updatedBy As String = si.AuthToken.UserName
				Dim UpdateDate As Date = DateTime.Now
				Dim completeReqStatus As String = updatedBy & " : " & UpdateDate & " : " & reqFundCenter & " : " & reqstatus
				
				If(Not String.IsNullOrWhiteSpace(reqstatus)) Then 				
					'Get current status of BUD
					Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Status_History:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim statusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation						

					If (String.IsNullOrWhiteSpace(statusHistory)) Then
						statusHistory = completeReqStatus
					
					Else
						statusHistory = statusHistory & ", " & completeReqStatus						
					End If
                    
					'Update BUD Status History
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = BUDMemberScript
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
		
#End Region

#Region "ValidateBudget: Validate BUD"
		'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484	
		'Updated 08292024 EH RMW-1565 BUDTime updated to annual for BUD_C20XX scenario, IC for A#REQ_Validated_Amt updated 
		Public Function ValidateBudget(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
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
				Dim BUDTime As String = WFYear & "M12"
				'Dim BUDTime As String = WFYear

				Dim BUDEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
				Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
					
				'------------Get current BUD workflow status-------------
				Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim currBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
				If(String.IsNullOrWhiteSpace(currBUDStatus)) Then
					Return Nothing
				End If
				
'				Dim BUDValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
'				Dim BUDApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'				Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
				Dim newBUDStatus As String = ""
			
				If sProfileSubString = "Validate Budgets" Then
'					If sValidatedAmount = 0 Then 
'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
'					End If
					currBUDStatus = currBUDStatus.Substring(0,2)
					newBUDStatus = currBUDStatus & " Ready for Prioritization"
					
				Else If sProfileSubString = "Validate Budgets CMD"
'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
'					End If
					currBUDStatus = currBUDStatus.Substring(0,2)
					newBUDStatus = currBUDStatus & " Ready for Prioritization"
				End If
				
                    'Update UFR Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = BUDMemberScript
					objScriptValStatus.TextValue = newBUDStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
				
				'update Status History
				Try
					Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus)
				Catch ex As Exception
				End Try
				
				'Send email
				Try
					Me.SendStatusChangeEmail(si, globals, api, args)
				Catch ex As Exception
				End Try
				
				'Set Updated Date and Name
				Try
					Me.LastUpdated(si, globals, api, args, BUDFlow, BUDEntity)
				Catch ex As Exception
				End Try
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region

#Region "ValidateBUDs: Validate BUDs"
		'Created: KN - 09/25/2024 - Ticket 1708	
		'Created to submit multiple BUDs for prioritization 
		Public Function ValidateBUDs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
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
				Dim BUDTime As String = WFYear & "M12"
				'Dim BUDTime As String = WFYear
			
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")
				For Each row As DataRow In oReqList.Rows
					Dim BUDEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","")
					Dim BUDFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","")
'BRApi.ErrorLog.LogMessage(si, $"EntityFlow = {row("EntityFlow")} || BUDEntity = {BUDEntity} || BUDFlow = {BUDFlow}")					
				
					'------------Get current BUD workflow status-------------
					Dim BUDMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRApi.ErrorLog.LogMessage(si, $"BUDMemberScript = {BUDMemberScript}")
					Dim currBUDStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDMemberScript).DataCellEx.DataCellAnnotation
					If(String.IsNullOrWhiteSpace(currBUDStatus)) Then
						Return Nothing
					End If
					
					Dim BUDValidatedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Validated_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sValidatedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDValidatedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim BUDApprovedAmtMemberScript As String = "Cb#" & wfCube & ":E#" & BUDEntity & ":C#Local:S#" & wfScenario & ":T#" & BUDTime & ":V#Periodic:A#REQ_Approved_Amt:F#" & BUDFlow & ":O#BeforeAdj:I#" & BUDEntity & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim sApprovedAmount As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, BUDApprovedAmtMemberScript).DataCellEx.DataCell.CellAmount
					Dim newBUDStatus As String = ""
				
					If sProfileSubString = "Validate Budgets" Then
	'					If sValidatedAmount = 0 Then 
	'						Throw New Exception("Validated Amount cannot be blank" & environment.NewLine)
	'					End If
						currBUDStatus = currBUDStatus.Substring(0,2)
						newBUDStatus = currBUDStatus & " Ready for Prioritization"
						
					Else If sProfileSubString = "Validate Budgets CMD"
	'					If sApprovedAmount = 0 Or sValidatedAmount = 0 Then 
	'						Throw New Exception("Approved and Validated amounts cannot be blank" & environment.NewLine)
	'					End If
						currBUDStatus = currBUDStatus.Substring(0,2)
						newBUDStatus = currBUDStatus & " Ready for Prioritization"
					End If
					
                    'Update BUD Status
					Dim objListofScriptStatus As New List(Of MemberScriptandValue)
				    Dim objScriptValStatus As New MemberScriptAndValue
					
					objScriptValStatus.CubeName = wfCube
					objScriptValStatus.Script = BUDMemberScript
					objScriptValStatus.TextValue = newBUDStatus
					objScriptValStatus.IsNoData = False
					objListofScriptStatus.Add(objScriptValStatus)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptStatus)
					
					'update Status History
					Try
						Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus, BUDFlow, BUDEntity)
						'Me.UpdateStatusHistory(si, globals, api, args, newBUDStatus)
					Catch ex As Exception
					End Try
					
					'Send email
					Try
						Me.SendStatusChangeEmail(si, globals, api, args)
					Catch ex As Exception
					End Try
					
					'Set Updated Date and Name
					Try
						Me.LastUpdated(si, globals, api, args, BUDFlow, BUDEntity)
					Catch ex As Exception
					End Try
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			Return Nothing
		End Function	

#End Region

#Region "ExportAllREQs RollFwd"	
		'Export PGM Requirement Data
		Public Function RollFwd_ExportAllREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			'========== debug vars ============================================================================================================ 
			Dim sDebugFuncName As String = args.FunctionName	

			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	 
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")	
			If sEntity.XFContainsIgnoreCase("_General") Then
				sEntity = sEntity.Replace("_General","")
			Else 
				sEntity = sEntity
			End If
			sEntity = sEntity.Replace("""","")		


			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command from dropdown to export requirements.")
			End If	

			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sEntity)
			Dim SAccount As String = "REQ_PGM_app_Req_Amt"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			Dim sFilePath As String = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1

		
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","REQUIREMENT STATUS","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	
			Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
			Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			
			For Each Entity As Member In lsEntity
'If si.UserName.XFEqualsIgnoreCase(DEBUGUSER) Then BRapi.ErrorLog.LogMessage(si,DEBUGRULE & "." & sDebugFuncName & ":   Entity=" & Entity.Name )						
				For i As Integer = 0 To 1 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString & "M12"))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
					Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'If si.UserName.XFEqualsIgnoreCase(DEBUGUSER) Then BRapi.ErrorLog.LogMessage(si,DEBUGRULE & "." & sDebugFuncName & ":   cell.count=" & myCells.Count )						
					If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
				Next
			Next

			
			'Process the fetched data into a format usable for report		
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			processColumns.AddRange(New String(){"SCENARIO","Entity","Title","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR_TYPE","OBJECTCLASS","CTYPE",$"FY{iTime0}",$"FY{iTime1}","ID","AttachmentsInd","RelatedRequest","Description ","RecurringJustification","CostMethodologyCmt","RequestedFundSource","ITCyberRqmtInd","UICAcct","FlexField1","FlexField2","FlexField3","FlexField4","FlexField5","NewRqmtInd","CPAInd","ContractNumber","Task Order Number ","TargetDateOfAward","POPExpirationDate","FTECME","Directorate","Division","Branch","NotificationEmailList","CreatorName","CreationDateTime","LastUpdatedName","LastUpdatedDate","StatusHistory"})
			sFileHeader = $"SCENARIO,Entity,Title,FLOW,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},ID,AttachmentsInd,RelatedRequest,Description ,RecurringJustification,CostMethodologyCmt,RequestedFundSource,ITCyberRqmtInd,UICAcct,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,NewRqmtInd,CPAInd,ContractNumber,Task Order Number ,TargetDateOfAward,POPExpirationDate,FTECME,Directorate,Division,Branch,NotificationEmailList,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate,StatusHistory"

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

#Region "ExportAllREQs Review"
'Returning all BUD requirements
		Public Function ExportAllREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try	
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,7)						
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sBUDTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sCarryoverYrM12 As String = BRApi.Finance.Time.AddYears(si,si.WorkflowClusterPk.TimeKey,1).ToString.Substring(0,4) & "M12"
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTimePeriod As String = args.NameValuePairs.XFGetValue("TimePeriod","")
				Dim sAccount As String = "Total_Phased_Commitment, Total_Phased_Obligation, Phased_Commitment_UFR, Phased_Obligation_UFR"
				Dim lAccount As List(Of String) = sAccount.Split(", ").Select(Function(item)item.Trim()).ToList()
				Dim sAccounSql As String = "REQ_ID,REQ_Rqmt_Status,REQ_Title,REQ_Description,REQ_Attachments_Ind,REQ_Related_Request,REQ_Recurring_Justification,REQ_Impact_If_Not_Funded,REQ_Risk_If_Not_Funded,REQ_Cost_Methodology_Cmt,REQ_Cost_Growth_Justification,REQ_Must_Fund,REQ_Requested_Fund_Source,REQ_Army_initiative_Directive,REQ_Command_Initiative_Directive,REQ_Activity_Exercise,REQ_IT_Cyber_Rqmt_Ind,REQ_UIC_Acct,REQ_Flex_Field_1,REQ_Flex_Field_2,REQ_Flex_Field_3,REQ_Flex_Field_4,REQ_Flex_Field_5,REQ_New_Rqmt_Ind,REQ_CPA_Ind,REQ_PBR_Submission,REQ_UPL_Submission,REQ_Contract_Number,REQ_Task_Order_Number,REQ_Target_Date_Of_Award,REQ_POP_Expiration_Date,REQ_FTE_CME,REQ_COR_Email,REQ_POC_Email,REQ_Directorate,REQ_Division,REQ_Branch,REQ_Rev_POC_Email,REQ_MDEP_Func_Email,REQ_Notification_Email_List,REQ_Comments,REQ_Creator_Name,REQ_Creation_Date_Time,REQ_Last_Updated_Name,REQ_Last_Updated_Date,REQ_Status_History"
				Dim lAccountSql As List(Of String) = sAccounSql.Split(", ").Select(Function(item)item.Trim()).ToList()
				Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
				Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
				Dim sFilePath As String = ""
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)	
				
				'Throw new exception if FC is not selected
				If String.IsNullOrWhiteSpace(sFundCenter) 
					Throw New Exception("Please select a funds center from dropdown to export all requirements.")
				End If	
				
				Dim dtAll As New DataTable 
				
				'If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Or String.IsNullOrWhiteSpace(sAccount) Then 
					Return dtAll
				End If
				
				'If the fund center passed is not a descendent of the WF then stop
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter.Replace("_General","") & ")"
				Dim lCubeMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
				
				If Not lCubeMembers.Count > 0 Then
					Return dtAll
				End If
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFundCenter).Member
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
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				Dim lEntity As New List(Of String)
				Dim entitySQL As String = ""				
				If isBase = True Then 
					entitySQL = $"('{sFundCenter}')"	
					lEntity.Add(sFundCenter)
				Else
					Dim lFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, "E#"& sFundCenter &".Base",True)
					Dim isFirst As Boolean = True
					For Each FC As MemberInfo In lFundCenters						
						If isFirst Then
							entitySQL &= $"('{FC.Member.Name}')"
						Else
							entitySQL &= $", ('{FC.Member.Name}')"
						End If
						isFirst = False
						lEntity.Add(FC.Member.Name)
					Next			
				End If	
				'--------- derive status ----------
				Dim statusSQL As String = $"'{currentWFLevel}%'"
				Dim SQL As New Text.StringBuilder
					'New
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")				
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					Dim i As Integer = 1				
					For Each Account As String In lAccountSql
						If i = lAccountSql.Count Then
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}' ")
						Else
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}', ")
						End If
						i+=1
					Next
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")
					SQL.AppendLine($" Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON DATAATTACHMENT.ENTITY = ENTITY_LIST.VALUE ")			
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{sCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{sScenario}' ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"  SCENARIO, ENTITY, FLOW")
'					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) Like {statusSQL} ")

					SQL.AppendLine($"; ")
					SQL.AppendLine($"Select ")
					SQL.AppendLine($"    SCENARIO, ")
					SQL.AppendLine($"    ENTITY AS 'Funds Ctr', ")
					SQL.AppendLine($"    FLOW AS 'REQ', ")
					SQL.AppendLine($"    '' AS FUND, ")
					SQL.AppendLine($"    ' ' AS MDEP, ")
					SQL.AppendLine($"    ' ' AS APEPT, ")
				    SQL.AppendLine($"    ' ' AS 'DollarType', ")
					SQL.AppendLine($"    ' ' AS CType, ")
					SQL.AppendLine($"    ' ' AS 'ObjectClass', ")
					SQL.AppendLine($"   '""' +  REQ_Title + '""', ")
					SQL.AppendLine($"    REQ_Rqmt_Status, ")
					For Each Acct As String In lAccount	
'						SQL.AppendLine($"    0 As '{Acct}', ") 'original
						SQL.AppendLine($"    CAST(0 As BIGINT) '{Acct}', ") 'used to handle large value greater then billions
'						SQL.AppendLine($"    0 As '{Acct} Carryover', ")
					Next
					lAccountSql.Remove("REQ_Title")
					lAccountSql.Remove("REQ_Rqmt_Status")
					i = 1
					For Each Account As String In lAccountSql
						If i = lAccountSql.Count Then 
							SQL.AppendLine($"   '""' + [{Account}] + '""'" )
						Else
							SQL.AppendLine($"    '""' + [{Account}] + '""'," )
						End If
						i+=1
					Next
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW ")

'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
				'Dim dtFetch As New DataTable

				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
'Return dtAll				
				 For Each column As DataColumn In dtAll.Columns
					column.ReadOnly = False
					If column.ColumnName = "FUND" Or column.ColumnName = "MDEP" Or column.ColumnName = "APEPT" Or column.ColumnName = "DollarType" Or column.ColumnName = "CType" Or column.ColumnName = "ObjectClass" Then column.MaxLength = 100
				 Next 
				'For loop used to retrieve the total amounts for Commitment, Obligation, CommitmentUFR , Obligation	UFR
				For Each FundCenter As String In lEntity
					For Each Acct As String In lAccount
					'DataUnitPK - GetDataBufferCell to grab total amount at the current FY
						Dim myDataUnitPkAccountTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sBUDTime))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkAccountTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkAccountTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
						myDbCellPkAccountTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkAccountTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)
						'myDbCellPkAccountTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkAccountTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkAccountTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalAccountAmounts As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkAccountTotal, dimConstants.Periodic, myDbCellPkAccountTotal, True, True)	

					'DataUnitPK - GetDataBufferCell to grab total carryover amount at (FY + 1)M12
						Dim myDataUnitPkCarryoverTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sCarryoverYrM12))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkCarryoverTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkCarryoverTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
						myDbCellPkCarryoverTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkCarryoverTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						'myDbCellPkCarryoverTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkCarryoverTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkCarryoverTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalCarryoverAmount As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCarryoverTotal, dimConstants.Periodic, myDbCellPkCarryoverTotal, True, True)					
		
						For Each amount As DataCell In lTotalAccountAmounts

							Dim sEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, amount.DataCellPk.EntityId)
							Dim sFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, amount.DataCellPk.FlowId)
							Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
							Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
							Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id)
							Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
							Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, amount.DataCellPk.UD5Id)
							Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
							Dim resultRow As DataRow = dtAll.AsEnumerable() _
									.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
															AndAlso row.Field(Of String)("REQ") = sFlow)
							If resultRow Is Nothing Then Continue For
							resultRow(Acct) = amount.CellAmount
							resultRow("FUND") = sUD1
							resultRow("MDEP") = sUD2
							resultRow("APEPT") = sUD3.Split("_")(1)
							resultRow("DollarType") = sUD4
							resultRow("CType") = sUD5
							resultRow("ObjectClass") = sUD6
							
						Next
						'Loop through each returned carryover amount
						For Each carryover As DataCell In lTotalCarryoverAmount
								Dim sCEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, carryover.DataCellPk.EntityId)
								Dim sCFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, carryover.DataCellPk.FlowId)
								Dim resultRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If resultRow Is Nothing Then Continue For	
								'if a carryover amount exists, add it to the existing Annual amount which is set on line 815 
								resultRow(Acct) = resultRow(Acct) + Math.Round(carryover.CellAmount,0)
						Next						
					Next
				Next
											
			'Process the fetched data into a format usable for report
			Dim sFileHeader As String = ""
			sFileHeader = $"SCENARIO,Funds Ctr,REQ,FUND,MDEP,APEPT,DollarType,CType,ObjectClass,Title,Status,Total Commitment,Total Obligation,Total Commitment UFR,Total Obligation UFR,REQID,Description,Attachment,RelatedRequest,Justification,ImpactIfNotFunded,RiskIfNotFunded,CostMethodology,CostGrowthJustification,MustFund,RequestedFundSource,ArmyInitiativeDirective,CommandInitiativeDirective,ActivityExercise,ITCyberRequirement,UIC,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,EmergingRequirement,CPACandidate,PBRSubmission,UPLSubmission,ContractNumber,TaskOrderNumber,AwardTargetDate,POPExpirationDate,CME,COREmail,POCEmail,Directorate,Division,Branch,ReviewingPOCEmail,MDEPFunctionalEmail,NotificationEmailList,Comments,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate,StatusHistory"
			
'			ID,AttachmentsInd,
'			RelatedRequest,Description,RecurringJustification,CostMethodologyCmt,RequestedFundSource,ITCyberRqmtInd,UICAcct,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,NewRqmtInd,CPAInd,ContractNumber,
'			Task Order Number,TargetDateOfAward,POPExpirationDate,FTECME,Directorate,Division,Branch,NotificationEmailList,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate,StatusHistory"
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, sBUDTime, sTemplate, sFvParam)				
'			Return dtAll 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Export Report Helper | Functions: CreateReportDataTable; WriteFetchTable; ProcessTableForReport; GenerateReportFile; BuildFile"

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

				#Region "Export All REQs"
				
				If FetchDt.TableName.XFContainsIgnoreCase("ExportAllREQs") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					
					Dim exportScenario As String = Scenario
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"ENTITY","FLOW","REQ_ID","REQ_Rqmt_Status","REQ_Attachments_Ind","REQ_Related_Request","REQ_Title","REQ_Description ","REQ_Recurring_Justification","REQ_Cost_Methodology_Cmt","REQ_Requested_Fund_Source","REQ_IT_Cyber_Rqmt_Ind","REQ_UIC_Acct","REQ_Flex_Field_1","REQ_Flex_Field_2","REQ_Flex_Field_3","REQ_Flex_Field_4","REQ_Flex_Field_5","REQ_New_Rqmt_Ind","REQ_CPA_Ind","REQ_Contract_Number","Task Order Number ","REQ_Target_Date_Of_Award","REQ_POP_Expiration_Date","REQ_FTE_CME","REQ_Directorate","REQ_Division","REQ_Branch","REQ_Notification_Email_List","REQ_Creator_Name","REQ_Creation_Date_Time","REQ_Last_Updated_Name","REQ_Last_Updated_Date","REQ_Status_History"})
					dt = Me.CreateReportDataTable(si,"ExportAllREQs",detailColumns,True)	
					
					Dim SQL As New Text.StringBuilder
					SQL.Append($"SELECT * FROM ") 
					
'					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
'								CASE
'									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
'									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
'									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
'									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
'									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
'									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
'									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
'									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'
'								ELSE
'									ACCOUNT
'								End as AccountCAT
'					FROM DataAttachment WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")

'					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
'								CASE
'									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
					
'									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'

'								ELSE
'									ACCOUNT
'								End as AccountCAT
'					FROM DataAttachment WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")

					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT, ACCOUNT as AccountCAT FROM DataAttachment WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")

					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_ID],[REQ_Rqmt_Status],[REQ_Attachments_Ind],[REQ_Related_Request],[REQ_Title],[REQ_Description ],[REQ_Recurring_Justification],[REQ_Cost_Methodology_Cmt],[REQ_Requested_Fund_Source],[REQ_IT_Cyber_Rqmt_Ind],[REQ_UIC_Acct],[REQ_Flex_Field_1],[REQ_Flex_Field_2],[REQ_Flex_Field_3],[REQ_Flex_Field_4],[REQ_Flex_Field_5],[REQ_New_Rqmt_Ind],[REQ_CPA_Ind],[REQ_Contract_Number],[Task Order Number ],[REQ_Target_Date_Of_Award],[REQ_POP_Expiration_Date],[REQ_FTE_CME],[REQ_Directorate],[REQ_Division],[REQ_Branch],[REQ_Notification_Email_List],[REQ_Creator_Name],[REQ_Creation_Date_Time],[REQ_Last_Updated_Name],[REQ_Last_Updated_Date],[REQ_Status_History])) AS PIVOTTABLE ") 

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
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR_TYPE")= U4	
						newRow("OBJECTCLASS")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						For i As Integer = 0 To 1 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next
						
						'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
						Dim resultRow As DataRow = dt.AsEnumerable() _
							.singleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
													AndAlso row.Field(Of String)("FLOW") = Flow)
													
						'Assign values
						If resultRow IsNot Nothing Then
							newRow("ID") = """" & resultRow("REQ_ID") & """"
							newRow("REQUIREMENT STATUS") = """" & resultRow("REQ_Rqmt_Status") & """"
							newRow("AttachmentsInd") = """" & resultRow("REQ_Attachments_Ind") & """"
							newRow("RelatedRequest") = """" & resultRow("REQ_Related_Request") & """"
							newRow("Title") = """" & resultRow("REQ_Title") & """"
							newRow("Description ") = """" & resultRow("REQ_Description ") & """"
							newRow("RecurringJustification") = """" & resultRow("REQ_Recurring_Justification") & """"
							newRow("CostMethodologyCmt") = """" & resultRow("REQ_Cost_Methodology_Cmt") & """"
							newRow("RequestedFundSource") = """" & resultRow("REQ_Requested_Fund_Source") & """"
							newRow("ITCyberRqmtInd") = """" & resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
							newRow("UICAcct") = """" & resultRow("REQ_UIC_Acct") & """"
							newRow("FlexField1") = """" & resultRow("REQ_Flex_Field_1") & """"
							newRow("FlexField2") = """" & resultRow("REQ_Flex_Field_2") & """"
							newRow("FlexField3") = """" & resultRow("REQ_Flex_Field_3") & """"
							newRow("FlexField4") = """" & resultRow("REQ_Flex_Field_4") & """"
							newRow("FlexField5") = """" & resultRow("REQ_Flex_Field_5") & """"
							newRow("NewRqmtInd") = """" & resultRow("REQ_New_Rqmt_Ind") & """"
							newRow("CPAInd") = """" & resultRow("REQ_CPA_Ind") & """"
							newRow("ContractNumber") = """" & resultRow("REQ_Contract_Number") & """"
							newRow("Task Order Number ") = """" & resultRow("Task Order Number ") & """"
							newRow("TargetDateOfAward") = """" & resultRow("REQ_Target_Date_Of_Award") & """"
							newRow("POPExpirationDate") = """" & resultRow("REQ_POP_Expiration_Date") & """"
							newRow("FTECME") = """" & resultRow("REQ_FTE_CME") & """"
							newRow("Directorate") = """" & resultRow("REQ_Directorate") & """"
							newRow("Division") = """" & resultRow("REQ_Division") & """"
							newRow("Branch") = """" & resultRow("REQ_Branch") & """"
							newRow("NotificationEmailList") = """" & resultRow("REQ_Notification_Email_List") & """"
							newRow("CreatorName") = """" & resultRow("REQ_Creator_Name") & """"
							newRow("CreationDateTime") = """" & resultRow("REQ_Creation_Date_Time") & """"
							newRow("LastUpdatedName") = """" & resultRow("REQ_Last_Updated_Name") & """"
							newRow("LastUpdatedDate") = """" & resultRow("REQ_Last_Updated_Date") & """"
							newRow("StatusHistory") = """" & resultRow("REQ_Status_History") & """"
							
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
		Private Function GenerateReportFile(ByVal si As SessionInfo, ByVal processDT As DataTable, ByVal sFileHeader As String, ByVal sCube As String, ByVal iTime As Integer, ByVal sTemplate As String, ByVal sFvParam As String)  As XFSelectionChangedTaskResult
			Try
				'Initialize file 
				Dim file As New Text.StringBuilder
				file.Append(sFileHeader)	

				'Populate file
				For Each row As DataRow In processDT.Rows
					Dim rowInfo As String = String.Join(",", row.ItemArray)
					'replaces "None" with an empty string
					rowInfo = rowInfo.Replace("None","")
'Return Nothing
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
				If Not String.IsNullOrWhiteSpace(sFvParam) Then BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"
				
				Return selectionChangedTaskResult
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
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
'BRapi.ErrorLog.LogMessage(si,$"sFilePath = {sFilePath}")							
				
				'Delete file
				'BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, String.Concat(sFolderPath, "/", sfileName))
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	

		
#End Region

#Region "ExportAllREQs L2 Approved - Certify"
'Export all L2 Approved BUD requirements
		Public Function ExportApprovedREQs() As Object
			Try	
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,7)						
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sBUDTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sCarryoverYrM12 As String = BRApi.Finance.Time.AddYears(si,si.WorkflowClusterPk.TimeKey,1).ToString.Substring(0,4) & "M12"
				Dim sAccount As String = "Total_Phased_Commitment, Total_Phased_Obligation, Phased_Commitment_UFR, Phased_Obligation_UFR"
				Dim lAccount As List(Of String) = sAccount.Split(", ").Select(Function(item)item.Trim()).ToList()
				Dim sAccounSql As String = "REQ_ID,REQ_Rqmt_Status,REQ_Title,REQ_Description,REQ_Attachments_Ind,REQ_Related_Request,REQ_Recurring_Justification,REQ_Impact_If_Not_Funded,REQ_Risk_If_Not_Funded,REQ_Cost_Methodology_Cmt,REQ_Cost_Growth_Justification,REQ_Must_Fund,REQ_Requested_Fund_Source,REQ_Army_initiative_Directive,REQ_Command_Initiative_Directive,REQ_Activity_Exercise,REQ_IT_Cyber_Rqmt_Ind,REQ_UIC_Acct,REQ_Flex_Field_1,REQ_Flex_Field_2,REQ_Flex_Field_3,REQ_Flex_Field_4,REQ_Flex_Field_5,REQ_New_Rqmt_Ind,REQ_CPA_Ind,REQ_PBR_Submission,REQ_UPL_Submission,REQ_Contract_Number,REQ_Task_Order_Number,REQ_Target_Date_Of_Award,REQ_POP_Expiration_Date,REQ_FTE_CME,REQ_COR_Email,REQ_POC_Email,REQ_Directorate,REQ_Division,REQ_Branch,REQ_Rev_POC_Email,REQ_MDEP_Func_Email,REQ_Notification_Email_List,REQ_Comments,REQ_Creator_Name,REQ_Creation_Date_Time,REQ_Last_Updated_Name,REQ_Last_Updated_Date,REQ_Status_History"
				Dim lAccountSql As List(Of String) = sAccounSql.Split(", ").Select(Function(item)item.Trim()).ToList()
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
				Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
				Dim sFilePath As String = ""
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)	
				
				'Throw new exception if FC is not selected
				If String.IsNullOrWhiteSpace(sFundCenter) 
					Throw New Exception("Please select a funds center from dropdown to export requirements.")
					Return Nothing
				End If	
				
				Dim dtAll As New DataTable 
				
				Dim lFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, $"E#{sFundCenter.ToLower.Replace("_general","")}.Base",True)
				
				'--------- derive status ----------
				Dim SQL As New Text.StringBuilder
					'New
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")				
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					Dim counter As Integer = 1				
					For Each Account As String In lAccountSql
						If counter = lAccountSql.Count Then
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}' ")
						Else
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}', ")
						End If
						counter+=1
					Next
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")			
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{sCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{sScenario}' ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"  SCENARIO, ENTITY, FLOW")
					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) = 'L2 Approved' ")

					SQL.AppendLine($"; ")
					SQL.AppendLine($"Select ")
					SQL.AppendLine($"    SCENARIO, ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW AS 'REQ', ")
					SQL.AppendLine($"    ' ' AS UD1, ")
					SQL.AppendLine($"    ' ' AS UD2, ")
					SQL.AppendLine($"    ' ' AS UD3, ")
				    SQL.AppendLine($"    ' ' AS UD4, ")
					SQL.AppendLine($"    ' ' AS UD5, ")
					SQL.AppendLine($"    ' ' AS UD6, ")
					SQL.AppendLine($"   '""' +  REQ_Title + '""', ")
					SQL.AppendLine($"    REQ_Rqmt_Status, ")
					For Each Acct As String In lAccount	
						SQL.AppendLine($"    CAST(0 As BIGINT) 'Total {Acct}', ") 'used to handle large value greater then billions
						For i As Integer = 1 To 12
							SQL.AppendLine($"    CAST(0 As BIGINT) '{Acct} M{i}', ")
						Next
						SQL.AppendLine($"    CAST(0 As BIGINT) '{Acct} Carryover', ")
					Next
					lAccountSql.Remove("REQ_Title")
					lAccountSql.Remove("REQ_Rqmt_Status")
					counter = 1
					For Each Account As String In lAccountSql
						If counter = lAccountSql.Count Then 
							SQL.AppendLine($"   '""' + [{Account}] + '""'" )
						Else
							SQL.AppendLine($"    '""' + [{Account}] + '""'," )
						End If
						counter+=1
					Next
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW ")

'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
				'Dim dtFetch As New DataTable

				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
			
				For Each column As DataColumn In dtAll.Columns
					column.ReadOnly = False
					If column.ColumnName = "UD1" Or column.ColumnName = "UD2" Or column.ColumnName = "UD3" Or column.ColumnName = "UD4" Or column.ColumnName = "UD5" Or column.ColumnName = "UD6" Then column.MaxLength = 100
				Next 
				'For loop used to retrieve the total amounts for Commitment, Obligation, CommitmentUFR , Obligation	UFR
				For Each FC As MemberInfo In lFundCenters
					Dim FundCenter As String = FC.Member.Name
					For Each Acct As String In lAccount
						For month As Integer = 1 To 12
							'used in databuffer
							Dim sMonth As String = sBUDTime & "M" & month
							
							'DataUnitPK - GetDataBufferCell to grab total amount at each month
							Dim myDataUnitPkAccountMonthly As New DataUnitPk( _
							BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
							DimConstants.Local, _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sMonth))
							'Buffer coordinates.
							'DataBufferCellPK
							Dim myDbCellPkAccountMonthly As New DataBufferCellPk( DimConstants.All )
							myDbCellPkAccountMonthly.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
							myDbCellPkAccountMonthly.OriginId = DimConstants.BeforeAdj
							myDbCellPkAccountMonthly.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)
							myDbCellPkAccountMonthly.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
							myDbCellPkAccountMonthly.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

							Dim lAllAccountMonthly As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkAccountMonthly, dimConstants.Periodic, myDbCellPkAccountMonthly, True, True)	

							For Each amount As DataCell In lAllAccountMonthly
								Dim sEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, amount.DataCellPk.EntityId)
								Dim sFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, amount.DataCellPk.FlowId)
								'Check if REQ_00 requirements and process
								If sFlow.XFContainsIgnoreCase("REQ_00") Then
									Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
									Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
									Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id).Split("_")(1)
									Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
									Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, amount.DataCellPk.UD5Id)
									Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
									Dim resultRow As DataRow = dtAll.AsEnumerable() _
											.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow _
																	AndAlso row.Field(Of String)("UD1") = sUD1 _
																	AndAlso row.Field(Of String)("UD2") = sUD2 _
																	AndAlso row.Field(Of String)("UD3") = sUD3 _
																	AndAlso row.Field(Of String)("UD4") = sUD4 _
																	AndAlso row.Field(Of String)("UD5") = sUD5 _
																	AndAlso row.Field(Of String)("UD6") = sUD6 )
'BRApi.ErrorLog.LogMessage(si, $"REQ: E#{sEntity}:F#{sFlow}:U1#{sUD1}:U2#{sUD2}:U3#{sUD3}:U4#{sUD4}:U5#{sUD5}:U6#{sUD6}	= {amount.CellAmount}")															
									If resultRow Is Nothing Then 
										'Check and see if that E#F# combination exists
										Dim resultRow2 As datarow = dtAll.AsEnumerable() _
											.FirstOrDefault(Function(row) row.Field(Of String)("ENTITY") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow)
										If resultRow2 Is Nothing Then 
											Continue For
										Else
'BRApi.ErrorLog.LogMessage(si, $"Creating new row for REQ: E#{sEntity}:F#{sFlow}:U1#{sUD1}:U2#{sUD2}:U3#{sUD3}:U4#{sUD4}:U5#{sUD5}:U6#{sUD6}")											
											'Clone the row and create a new row for the new combination of funding line
											Dim newRow As DataRow = dtAll.NewRow()
											newRow.ItemArray = resultRow2.ItemArray.Clone()
											newRow("UD1") = sUD1
											newRow("UD2") = sUD2
											newRow("UD3") = sUD3
											newRow("UD4") = sUD4
											newRow("UD5") = sUD5					
											newRow("UD6") = sUD6
											newRow($"{Acct} M{month}") =  Math.Round(amount.CellAmount,0)					
											newRow($"Total {Acct}") = Math.Round(amount.CellAmount,0)
											dtAll.Rows.Add(newRow)
										End If
									Else			
										'Write to the monthly account column for each indiviudal monthly amount 
										resultRow($"{Acct} M{month}") =  Math.Round(amount.CellAmount,0)
										'Write to the Total {Account} column for each month that passes through
										resultRow($"Total {Acct}") = resultRow($"Total {Acct}") + Math.Round(amount.CellAmount,0)
									End If
								Else
									Dim resultRow As DataRow = dtAll.AsEnumerable() _
											.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow)
								
									If resultRow Is Nothing Then Continue For
									'Write to the monthly account column for each indiviudal monthly amount 
									resultRow($"{Acct} M{month}") =  Math.Round(amount.CellAmount,0)
									'Write to the Total {Account} column for each month that passes through
									resultRow($"Total {Acct}") = resultRow($"Total {Acct}") + Math.Round(amount.CellAmount,0)
									If resultRow("UD1") = " " Then
										Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
										Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
										Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id).Split("_")(1)
										Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
										Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, amount.DataCellPk.UD5Id)
										Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
										resultRow("UD1") = sUD1
										resultRow("UD2") = sUD2
										resultRow("UD3") = sUD3
										resultRow("UD4") = sUD4
										resultRow("UD5") = sUD5
										resultRow("UD6") = sUD6
									End If 
								End If
							Next							
						Next

					'DataUnitPK - GetDataBufferCell to grab total carryover amount at (FY + 1)M12
					'This is outside of the monthly for loop because the carryover amount only exists at FY+1M12
					'After the monthly amounts are retrieved and added to the columns, loop through the carryover and add it to the respective column cells and add it to the Total {Account} column
						Dim myDataUnitPkCarryoverTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sCarryoverYrM12))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkCarryoverTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkCarryoverTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
						myDbCellPkCarryoverTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkCarryoverTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						
						myDbCellPkCarryoverTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkCarryoverTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkCarryoverTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lAllCarryoverAmount As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCarryoverTotal, dimConstants.Periodic, myDbCellPkCarryoverTotal, True, True)	
							
						For Each carryover As DataCell In lAllCarryoverAmount
							Dim sCEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, carryover.DataCellPk.EntityId)
							Dim sCFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, carryover.DataCellPk.FlowId)
						
							Dim resultRow As DataRow = dtAll.AsEnumerable() _
									.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = sCEntity _
															AndAlso row.Field(Of String)("REQ") = sCFlow)
							If resultRow Is Nothing Then Continue For	
							resultRow($"{Acct} Carryover") =  Math.Round(carryover.CellAmount,0)
							'Add carryover amount to Total {Account} column
							resultRow($"Total {Acct}") = resultRow($"Total {Acct}") + Math.Round(carryover.CellAmount,0)
							If String.IsNullOrWhiteSpace(resultRow("UD1").ToString) Then
								Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, carryover.DataCellPk.UD1Id)
								Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, carryover.DataCellPk.UD2Id)
								Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, carryover.DataCellPk.UD3Id).Split("_")(1)
								Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, carryover.DataCellPk.UD4Id)
								Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, carryover.DataCellPk.UD5Id)
								Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, carryover.DataCellPk.UD6Id)
								resultRow("UD1") = sUD1
								resultRow("UD2") = sUD2
								resultRow("UD3") = sUD3
								resultRow("UD4") = sUD4
								resultRow("UD5") = sUD5
								resultRow("UD6") = sUD6
							End If 
						Next
					Next
				Next
											
			'Create File Headers
			Dim lFileHeader As New List(Of String)
			lFileHeader.Add("Scenario")
			lFileHeader.Add("Funds Ctr")
			lFileHeader.Add("REQ")
			lFileHeader.Add("FUND")
			lFileHeader.Add("MDEP")
			lFileHeader.Add("APEPT")
			lFileHeader.Add("DollarType")
			lFileHeader.Add("CType")
			lFileHeader.Add("Cost Cat")
			lFileHeader.Add("Title")
			lFileHeader.Add("Status")
			lFileHeader.Add("Total Commitment")
			For i As Integer = 1 To 12
				lFileHeader.Add($"Commitment M{i}")
			Next
			lFileHeader.Add($"Commitment Carryover")
			lFileHeader.Add("Total Obligation")
			For i As Integer = 1 To 12
				lFileHeader.Add($"Obligation M{i}")
			Next
			lFileHeader.Add($"Obligation Carryover")
			lFileHeader.Add("Total Commitment UFR")
			For i As Integer = 1 To 12
				lFileHeader.Add($"Commitment UFR M{i}")
			Next
			lFileHeader.Add($"Commitment UFR Carryover")
			lFileHeader.Add("Total Obligation UFR")
			For i As Integer = 1 To 12
				lFileHeader.Add($"Obligation UFR M{i}")
			Next
			lFileHeader.Add($"Obligation UFR Carryover")
			lFileHeader.Add("REQID")
			lFileHeader.Add("Description")
			lFileHeader.Add("Attachment")
			lFileHeader.Add("RelatedRequest")
			lFileHeader.Add("Justification")
			lFileHeader.Add("ImpactIfNotFunded")
			lFileHeader.Add("RiskIfNotFunded")
			lFileHeader.Add("CostMethodology")
			lFileHeader.Add("CostGrowthJustification")
			lFileHeader.Add("MustFund")
			lFileHeader.Add("RequestedFundSource")
			lFileHeader.Add("ArmyInitiativeDirective")
			lFileHeader.Add("CommandInitiativeDirective")
			lFileHeader.Add("ActivityExercise")
			lFileHeader.Add("ITCyberRequirement")
			lFileHeader.Add("UIC")
			lFileHeader.Add("FlexField1")
			lFileHeader.Add("FlexField2")
			lFileHeader.Add("FlexField3")
			lFileHeader.Add("FlexField4")
			lFileHeader.Add("FlexField5")
			lFileHeader.Add("EmergingRequirement")
			lFileHeader.Add("CPACandidate")
			lFileHeader.Add("PBRSubmission")
			lFileHeader.Add("UPLSubmission")
			lFileHeader.Add("ContractNumber")
			lFileHeader.Add("TaskOrderNumber")
			lFileHeader.Add("AwardTargetDate")
			lFileHeader.Add("POPExpirationDate")
			lFileHeader.Add("CME")
			lFileHeader.Add("COREmail")
			lFileHeader.Add("POCEmail")
			lFileHeader.Add("Directorate")
			lFileHeader.Add("Division")
			lFileHeader.Add("Branch")
			lFileHeader.Add("ReviewingPOCEmail")
			lFileHeader.Add("MDEPFunctionalEmail")
			lFileHeader.Add("NotificationEmailList")
			lFileHeader.Add("Comments")
			lFileHeader.Add("CreatorName")
			lFileHeader.Add("CreationDateTime")
			lFileHeader.Add("LastUpdatedName")
			lFileHeader.Add("LastUpdatedDate")
			lFileHeader.Add("StatusHistory")
			Dim sFileHeader As String = String.Join(",", lFileHeader)

			'Generate & write File and update FvParam for filepath needed for file viewer
			Dim dv As New DataView(dtAll)
			dv.sort = "ENTITY ASC"
			Dim sortedTable As DataTable = dv.ToTable()
			Return Me.GenerateReportFile(si, sortedTable, sFileHeader, sCube, sBUDTime, sTemplate, sFvParam)				

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "FormulateSaveandValidation"
'Created 3/27/25 by JM - On save validation for comit and oblig data and save for annotation data 
Public Function FormulateSaveValidation() As Object
			Try
				
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sEntity As String = args.NameValuePairs.XFGetValue("BUDEntity")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim BUDFlow As String = args.NameValuePairs.XFGetValue("BUDFlow")
			
			Dim sComment As String = args.NameValuePairs.XFGetValue("Comment")
			'Dim sPOCComment As String = args.NameValuePairs.XFGetValue("POCComment")
			Dim sBUDAccount As String = args.NameValuePairs.XFGetValue("BUDAccount").Trim
			Dim sU5 As String = args.NameValuePairs.XFGetValue("U5").Trim
			'Dim sPOCAcct As String = args.NameValuePairs.XFGetValue("POCAccount").Trim

		
			Dim sInfoMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & sBUDAccount & ":F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#None"
			'Dim sPOCMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#" & sPOCAcct & ":F#" & BUDFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#" & sU5 & ":U6#None:U7#None:U8#None"
			
'Brapi.ErrorLog.LogMessage(si, "MemberScrpt" & sInfoMemberScript & "Comment" & sComment)				
			
			'set cell for general info comment
			Dim objListofScriptsComment As New List(Of MemberScriptandValue)
		    Dim objScriptValComment As New MemberScriptAndValue
			objScriptValComment.CubeName = sCube
			objScriptValComment.Script = sInfoMemberScript
			objScriptValComment.TextValue = sComment
			objScriptValComment.IsNoData = False
			objListofScriptsComment.Add(objScriptValComment)
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsComment)
			
			'Set Updated Date and Name
			Try
				Me.LastUpdated(si, globals, api, args, BUDFlow, sEntity)
			Catch ex As Exception
			End Try	
					
				

				
			Dim BUDOblig As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & WFYear.ToString & ":V#Periodic:A#Phased_Obligation_Base:F#" & BUDFlow & ":O#BeforeAdj:I#" & sEntity & ":U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Dim BUDCommit As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & WFYear.ToString & ":V#Periodic:A#Phased_Commitment:F#" & BUDFlow & ":O#BeforeAdj:I#" & sEntity & ":U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Dim BUDObligAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDOblig).DataCellEx.DataCell.CellAmount
			Dim BUDCommitAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDCommit).DataCellEx.DataCell.CellAmount
			
			Dim BUDObligWFNext As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & (WFYear + 1).ToString & "M12" & ":V#Periodic:A#Phased_Obligation_Base:F#" & BUDFlow & ":O#BeforeAdj:I#" & sEntity & ":U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Dim BUDCommitWFNext As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & (WFYear + 1).ToString & "M12" & ":V#Periodic:A#Phased_Commitment:F#" & BUDFlow & ":O#BeforeAdj:I#" & sEntity & ":U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Dim BUDObligAmtWFNext As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDObligWFNext).DataCellEx.DataCell.CellAmount
			Dim BUDCommitAmtWFNext As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDCommitWFNext).DataCellEx.DataCell.CellAmount

			Dim ObligTotal As Long = BUDObligAmt + BUDObligAmtWFNext
			Dim ComitTotal As Long = BUDCommitAmt + BUDCommitAmtWFNext

			If ObligTotal <> ComitTotal Then
				Dim sScriptAmtCacheName = args.NameValuePairs.XFGetValue("ScriptAmtCacheName")
				Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName, sScriptAmtCacheName)
				Dim sScriptAmtCacheNameUFR = args.NameValuePairs.XFGetValue("ScriptAmtUFRCacheName")
				Dim dtUFR As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName, sScriptAmtCacheNameUFR)
				Dim objListScriptVal As New List(Of MemberScriptAndValue)
				
				For Each row As DataRow In dt.Rows
					Dim objScriptVal As New MemberScriptAndValue
					objScriptVal.CubeName = sCube
					objScriptVal.Script = row("Script")
					objScriptVal.Amount = row("Amount")
					objScriptVal.IsNoData = row("IsNoData")
					objListScriptVal.Add(objScriptVal)
					BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListScriptVal)
				Next
				If dtUFR IsNot Nothing Then
					For Each row As DataRow In dtUFR.Rows
						Dim objScriptVal As New MemberScriptAndValue
						objScriptVal.CubeName = sCube
						objScriptVal.Script = row("Script")
						objScriptVal.Amount = row("Amount")
						objScriptVal.IsNoData = row("IsNoData")
						objListScriptVal.Add(objScriptVal)
						BRApi.Finance.Data.SetDataCellsUsingMemberScript(si,objListScriptVal)
					Next
				End If
				Throw New Exception("Phased Commitments do not equal Phased Obligations." & vbCrLf & "Please update and save again." & vbCrLf & $"Total Commitment Amount: {ComitTotal.ToString("N0")}" & vbCrLf & $"Total Obligation Amount: {ObligTotal.ToString("N0")}")
				Return Nothing
			Else If BUDObligAmt > BUDCommitAmt
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Warning: Year to Date Obligations exceeds Commitments"
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
						selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing 'objXFSelectionChangedUIActionInfo
						selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
						selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
						selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
						selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
						selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
						selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
						Return selectionChangedTaskResult
				
				Else
				
		
				Return Nothing	
				End If 
				
				
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		
			Return Nothing
				
		
				
			End Try
		End Function
#End Region

#Region "SendPackageStatusChangeEmail: Send Package Status Change Email"
	'Updated EH 08292024 RMW-1565 Updated sBUDTime to annual for BUD_C20XX and Centralized Text (BUD_Shared, 1999)
	'Updated: EH 9/18/2024 - RMW-1732 Reverting BUD_Shared changes
	Public Sub SendPackageStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
		'Usage: {UFR_SolutionHelper}{SendStkhldrEmail}{FundCenter=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_UserFundCenters__Shared!|],UFR=[|!prompt_cbx_UFRPRO_AAAAAA_UFRListByEntity__Shared!|],StakeHolderEmails=[|!prompt_cbx_UFRPRO_AAAAAA_0CaAa_StakeholderEmailList__Shared!|]}
		Try
			Dim cube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim scenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim time As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
			Dim requirement_numbers As String() = Strings.Split(args.NameValuePairs.XFGetValue("requirement_numbers"), ",")
			Dim funds_center As String = args.NameValuePairs.XFGetValue("funds_center")
			Dim appropriation As String = args.NameValuePairs.XFGetValue("appropriation")
			Dim status As String = args.NameValuePairs.XFGetValue("status")
			Dim reason As String = args.NameValuePairs.XFGetValue("reason", "")
'BRApi.ErrorLog.LogMessage(si, $"{args.NameValuePairs.XFGetValue("requirement_numbers")} {funds_center} {appropriation} {status}")
			
			Dim email_req_map As New Dictionary(Of String, List(Of String))
			
			For Each requirement_number As String In requirement_numbers
				' Get all email addresses added to the requirement
				Dim email_addresses_raw_script As String = "Cb#" & cube & ":E#" & funds_center & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#REQ_Notification_Email_List:F#" & requirement_number & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	            Dim email_addresses_raw As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cube, email_addresses_raw_script).DataCellEx.DataCellAnnotation	
'BRApi.ErrorLog.LogMessage(si, email_addresses_raw)
				' Get title for the requirement
				Dim title_script As String = "Cb#" & cube & ":E#" & funds_center & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#REQ_Title:F#" & requirement_number & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	            Dim title As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, cube, title_script).DataCellEx.DataCellAnnotation
				
				' Break out into list of email addresses
				Dim email_addresses As String() = Strings.Split(email_addresses_raw, ",")
				
				' Either add the requirement to existing address or else make new entry with this requirement
				For Each address As String In email_addresses
					If Not email_req_map.ContainsKey(address) Then
						email_req_map.Add(address, New List(Of String))
					End If
					email_req_map(address).Append($"{requirement_number} - {title}")
				Next
				
			Next

'			'Status Member Script
'			Dim status_script As String = "Cb#" & cube & ":E#" & funds_center & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#REQ_Rqmt_Status:F#" & requirement_number & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim status As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, status_script).DataCellEx.DataCellAnnotation
			
'			'Creator Name Member Script
'			Dim creator_name_script As String = "Cb#" & cube & ":E#" & funds_center & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#REQ_Creator_Name:F#" & requirement_number & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim creator_name As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, creator_name_script).DataCellEx.DataCellAnnotation
			
'			'Creation Data Member Script
'			Dim creation_date_script As String = "Cb#" & cube & ":E#" & funds_center & ":C#Local:S#" & scenario & ":T#" & time & ":V#Annotation:A#REQ_Creation_Date_Time:F#" & requirement_number & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'            Dim creation_date As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, creation_date_script).DataCellEx.DataCellAnnotation	
''Brapi.ErrorLog.LogMessage(si, "BUDEmailNotificationMemberScript: " & BUDEmailNotificationMemberScript)
''Brapi.ErrorLog.LogMessage(si, "BUDStatusEmailList: " & BUDStatusEmailList)

			' Variables to set up email functionality 
			Dim email_connector As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
			Dim disclaimer As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
			
			' Send emails to each address in the dictionary
			For Each email_req_map_kvp As KeyValuePair(Of String, List(Of String)) In email_req_map
				' Email address
				Dim to_email As New List(Of String)
				to_email.Append(email_req_map_kvp.Key)
				
				'Subject and body
				Dim subject As String = "RMW Budget - Requirements Package Status Has Changed"
				Dim body As String = ""
				If Not String.IsNullOrWhiteSpace(reason) Then
					body = $"A package has been returned to a prior step ({status}): {vbCrLf}{vbCrLf}Funds Center: {funds_center}{vbCrLf}Appropriation: {appropriation}{vbCrLf}Reason: {reason}"
				Else
					body = $"A package's status has changed to ""{status}"": {vbCrLf}{vbCrLf}Funds Center: {funds_center}{vbCrLf}Appropriation: {appropriation}"
				End If
'				Dim body As String = $"A package's status has changed to ""{status}"": {vbCrLf}{vbCrLf}Funds Center: {funds_center}{vbCrLf}Appropriation: {appropriation}{vbCrLf}{vbCrLf}From this package, you are a POC for the following requirements:{vbCrLf}"
'				For Each req As String In email_req_map_kvp.Value
'					body &= vbCrLf & req
'				Next
				body &= vbCrLf & vbCrLf & vbCrLf & disclaimer
				
				' Send email
				Try
					BRApi.Utilities.SendMail(si, email_connector, to_email, subject, body, Nothing)
				Catch ex As Exception
					BRApi.ErrorLog.LogMessage(si, "Email not sent." & vbcrlf & ex.Message)
				End Try
'BRApi.ErrorLog.LogMessage(si, body)
			Next
			'//Status Change Email\\
			'Build Body
			'Dim body As String = "A Budget Request for Fund Center: " & sFundCenter & " with Budget Title: "  & sBUDid & " - " & sBUDTitle &  " has changed status to '" & sBUDStatus & "' " & vbCrLf & "Submitted by: " & userName & " - " & sBUDCreationDate  & vbCrLf & vbCrLf & vbCrLf & vbCrLf & vbCrLf & BodyDisclaimerBody

			'Send email
'			If Not String.IsNullOrWhiteSpace(email_list_final) Then
''Brapi.ErrorLog.LogMessage(si, "hits the send: " & EmailConnectorStr)
'				'BRApi.Utilities.SendMail(si, email_connector, email_list, subject, body, Nothing)	
'			End If
			'Executes the "SendReviewRequestEmail" function and sends an email to the resepective CMD roles within it
			'Me.SendReviewRequestEmail(si, globals, api, funds_center, flow, title, creator_name, creation_date)
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try                       
	End Sub

#End Region
	
#Region "ExportPackage: Export Requirements by Package"
		Public Function ExportPackage(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try	
				' Get workflow profile full name and step name
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,7)		
				' Get workflow info
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sBUDTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sCarryoverYrM12 As String = BRApi.Finance.Time.AddYears(si,si.WorkflowClusterPk.TimeKey,1).ToString.Substring(0,4) & "M12"
				' Get passed args
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("entity")
				Dim u1 As String = args.NameValuePairs.XFGetValue("u1")
				Dim sTimePeriod As String = args.NameValuePairs.XFGetValue("TimePeriod","")
				' Accounts
				Dim sAccount As String = "Total_Phased_Commitment, Total_Phased_Obligation, Phased_Commitment_UFR, Phased_Obligation_UFR"
				Dim lAccount As List(Of String) = sAccount.Split(", ").Select(Function(item)item.Trim()).ToList()
				Dim sAccounSql As String = "REQ_ID,REQ_Rqmt_Status,REQ_Title,REQ_Description,REQ_Attachments_Ind,REQ_Related_Request,REQ_Recurring_Justification,REQ_Impact_If_Not_Funded,REQ_Risk_If_Not_Funded,REQ_Cost_Methodology_Cmt,REQ_Cost_Growth_Justification,REQ_Must_Fund,REQ_Requested_Fund_Source,REQ_Army_initiative_Directive,REQ_Command_Initiative_Directive,REQ_Activity_Exercise,REQ_IT_Cyber_Rqmt_Ind,REQ_UIC_Acct,REQ_Flex_Field_1,REQ_Flex_Field_2,REQ_Flex_Field_3,REQ_Flex_Field_4,REQ_Flex_Field_5,REQ_New_Rqmt_Ind,REQ_CPA_Ind,REQ_PBR_Submission,REQ_UPL_Submission,REQ_Contract_Number,REQ_Task_Order_Number,REQ_Target_Date_Of_Award,REQ_POP_Expiration_Date,REQ_FTE_CME,REQ_COR_Email,REQ_POC_Email,REQ_Directorate,REQ_Division,REQ_Branch,REQ_Rev_POC_Email,REQ_MDEP_Func_Email,REQ_Notification_Email_List,REQ_Comments,REQ_Creator_Name,REQ_Creation_Date_Time,REQ_Last_Updated_Name,REQ_Last_Updated_Date,REQ_Status_History"
				Dim lAccountSql As List(Of String) = sAccounSql.Split(", ").Select(Function(item)item.Trim()).ToList()
				' Get file name args
				Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
				Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
				Dim sFilePath As String = ""
				BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)	
				
				' Throw new exception if FC or APPN is not selected
				If String.IsNullOrWhiteSpace(sFundCenter) Or String.IsNullOrWhiteSpace(u1) Then
					Throw New Exception("Please select a funds center and appropriation to export package details.")
				End If	
				
				' Data table that stores information for export
				Dim dtAll As New DataTable 
				
				' If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Or String.IsNullOrWhiteSpace(sAccount) Then 
					Return dtAll
				End If
				
				' If the fund center passed is not a descendent of the WF then stop
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter.Replace("_General","") & ")"
				Dim lCubeMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
				
				If Not lCubeMembers.Count > 0 Then
					Return dtAll
				End If
				
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFundCenter).Member
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
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				Dim lEntity As New List(Of String)
				Dim entitySQL As String = ""				
				If isBase = True Then 
					entitySQL = $"('{sFundCenter}')"	
					lEntity.Add(sFundCenter)
				Else
					Dim lFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, "E#"& sFundCenter &".Base",True)
					Dim isFirst As Boolean = True
					For Each FC As MemberInfo In lFundCenters						
						If isFirst Then
							entitySQL &= $"('{FC.Member.Name}')"
						Else
							entitySQL &= $", ('{FC.Member.Name}')"
						End If
						isFirst = False
						lEntity.Add(FC.Member.Name)
					Next			
				End If	
				'--------- derive status ----------
				Dim statusSQL As String = $"'{currentWFLevel}%'"
				Dim SQL As New Text.StringBuilder
					'New
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")				
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					Dim i As Integer = 1				
					For Each Account As String In lAccountSql
						If i = lAccountSql.Count Then
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}' ")
						Else
							SQL.AppendLine($"   MAX(Case When ACCOUNT = '{Account}' THEN TEXT END) AS '{Account}', ")
						End If
						i+=1
					Next
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")
					SQL.AppendLine($" Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON DATAATTACHMENT.ENTITY = ENTITY_LIST.VALUE ")			
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{sCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{sScenario}' ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"  SCENARIO, ENTITY, FLOW")
'					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) Like {statusSQL} ")

					SQL.AppendLine($"; ")
					SQL.AppendLine($"Select ")
					SQL.AppendLine($"    SCENARIO, ")
					SQL.AppendLine($"    ENTITY AS 'Funds Ctr', ")
					SQL.AppendLine($"    FLOW AS 'REQ', ")
					SQL.AppendLine($"    '' AS FUND, ")
					SQL.AppendLine($"    ' ' AS MDEP, ")
					SQL.AppendLine($"    ' ' AS APEPT, ")
				    SQL.AppendLine($"    ' ' AS 'DollarType', ")
					SQL.AppendLine($"    ' ' AS 'Cost Cat', ")
					SQL.AppendLine($"   '""' +  REQ_Title + '""', ")
					SQL.AppendLine($"    REQ_Rqmt_Status, ")
					For Each Acct As String In lAccount	
'						SQL.AppendLine($"    0 As '{Acct}', ") 'original
						SQL.AppendLine($"    CAST(0 As BIGINT) '{Acct}', ") 'used to handle large value greater then billions
'						SQL.AppendLine($"    0 As '{Acct} Carryover', ")
					Next
					lAccountSql.Remove("REQ_Title")
					lAccountSql.Remove("REQ_Rqmt_Status")
					i = 1
					For Each Account As String In lAccountSql
						If i = lAccountSql.Count Then 
							SQL.AppendLine($"   '""' + [{Account}] + '""'" )
						Else
							SQL.AppendLine($"    '""' + [{Account}] + '""'," )
						End If
						i+=1
					Next
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW ")

'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
				'Dim dtFetch As New DataTable

				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
'Return dtAll				
				 For Each column As DataColumn In dtAll.Columns
					column.ReadOnly = False
					If column.ColumnName = "FUND" Or column.ColumnName = "MDEP" Or column.ColumnName = "APEPT" Or column.ColumnName = "DollarType" Or column.ColumnName = "Cost Cat" Then column.MaxLength = 100
				 Next 
				'For loop used to retrieve the total amounts for Commitment, Obligation, CommitmentUFR , Obligation	UFR
				For Each FundCenter As String In lEntity
					For Each Acct As String In lAccount
					'DataUnitPK - GetDataBufferCell to grab total amount at the current FY
						Dim myDataUnitPkAccountTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sBUDTime))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkAccountTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkAccountTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
						myDbCellPkAccountTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkAccountTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)
						myDbCellPkAccountTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkAccountTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkAccountTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalAccountAmounts As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkAccountTotal, dimConstants.Periodic, myDbCellPkAccountTotal, True, True)	

					'DataUnitPK - GetDataBufferCell to grab total carryover amount at (FY + 1)M12
						Dim myDataUnitPkCarryoverTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sCarryoverYrM12))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkCarryoverTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkCarryoverTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, Acct)
						myDbCellPkCarryoverTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkCarryoverTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						myDbCellPkCarryoverTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkCarryoverTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkCarryoverTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalCarryoverAmount As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCarryoverTotal, dimConstants.Periodic, myDbCellPkCarryoverTotal, True, True)					
		
						For Each amount As DataCell In lTotalAccountAmounts

							Dim sEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, amount.DataCellPk.EntityId)
							Dim sFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, amount.DataCellPk.FlowId)
							Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
							Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
							Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id).Split("_")(1)
							Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
							Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
							Dim resultRow As DataRow = dtAll.AsEnumerable() _
									.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
															AndAlso row.Field(Of String)("REQ") = sFlow)
							If resultRow Is Nothing Then Continue For
							If Not BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), amount.DataCellPk.UD1Id, True, )(0).Name.XFEqualsIgnoreCase(u1) Then
								dtAll.Rows.Remove(resultRow)
								Continue For
							End If
							resultRow(Acct) = amount.CellAmount
							resultRow("FUND") = sUD1
							resultRow("MDEP") = sUD2
							resultRow("APEPT") = sUD3
							resultRow("DollarType") = sUD4
							resultRow("Cost Cat") = sUD6
							
						Next
						'Loop through each returned carryover amount
						For Each carryover As DataCell In lTotalCarryoverAmount
								Dim sCEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, carryover.DataCellPk.EntityId)
								Dim sCFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, carryover.DataCellPk.FlowId)
								Dim resultRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If resultRow Is Nothing Then Continue For	
								'if a carryover amount exists, add it to the existing Annual amount which is set on line 815 
								resultRow(Acct) = resultRow(Acct) + Math.Round(carryover.CellAmount,0)
						Next						
					Next
				Next
											
			'Process the fetched data into a format usable for report
			Dim sFileHeader As String = ""
			sFileHeader = $"SCENARIO,Funds Ctr,REQ,FUND,MDEP,APEPT,DollarType,Cost Cat,Title,Status,Total Commitment,Total Obligation,Total Commitment UFR,Total Obligation UFR,REQID,Description,Attachment,RelatedRequest,Justification,ImpactIfNotFunded,RiskIfNotFunded,CostMethodology,CostGrowthJustification,MustFund,RequestedFundSource,ArmyInitiativeDirective,CommandInitiativeDirective,ActivityExercise,ITCyberRequirement,UIC,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,EmergingRequirement,CPACandidate,PBRSubmission,UPLSubmission,ContractNumber,TaskOrderNumber,AwardTargetDate,POPExpirationDate,CME,COREmail,POCEmail,Directorate,Division,Branch,ReviewingPOCEmail,MDEPFunctionalEmail,NotificationEmailList,Comments,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate,StatusHistory"
			
'			ID,AttachmentsInd,
'			RelatedRequest,Description,RecurringJustification,CostMethodologyCmt,RequestedFundSource,ITCyberRqmtInd,UICAcct,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,NewRqmtInd,CPAInd,ContractNumber,
'			Task Order Number,TargetDateOfAward,POPExpirationDate,FTECME,Directorate,Division,Branch,NotificationEmailList,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate,StatusHistory"
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, sBUDTime, sTemplate, sFvParam)				
'			Return dtAll 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

				

	End Class

#Region "BUD Class"
	Public Class BUD
		'Class to hold the requirement object
		'-----Mapping definitions----
		Public Dim Entity As String = ""
		Public Dim APPN As String = ""
		Public Dim MDEP As String = ""
		Public Dim SAG_APE As String = ""
		Public Dim DollarType As String = ""
		Public Dim CostCategory As String = ""
		Public Dim CivType As String = ""
		Public Dim Cycle As String = ""
		Public Dim COM_M1 As String = ""
		Public Dim COM_M2 As String = ""
		Public Dim COM_M3 As String = ""
		Public Dim COM_M4 As String = ""
		Public Dim COM_M5 As String = ""
		Public Dim COM_M6 As String = ""
		Public Dim COM_M7 As String = ""
		Public Dim COM_M8 As String = ""
		Public Dim COM_M9 As String = ""
		Public Dim COM_M10 As String = ""
		Public Dim COM_M11 As String = ""
		Public Dim COM_M12 As String = ""
		Public Dim COM_Carryover As String = ""
		Public Dim OBL_M1 As String = ""
		Public Dim OBL_M2 As String = ""
		Public Dim OBL_M3 As String = ""
		Public Dim OBL_M4 As String = ""
		Public Dim OBL_M5 As String = ""
		Public Dim OBL_M6 As String = ""
		Public Dim OBL_M7 As String = ""
		Public Dim OBL_M8 As String = ""
		Public Dim OBL_M9 As String = ""
		Public Dim OBL_M10 As String = ""
		Public Dim OBL_M11 As String = ""
		Public Dim OBL_M12 As String = ""
		Public Dim OBL_Carryover As String = ""
		Public Dim FTE As String = ""
		Public Dim Title As String = ""
		Public Dim Description As String = ""
		Public Dim Justification As String = ""
		Public Dim ImpactNotFunded As String = ""
		Public Dim RiskNotFunded As String = ""
		Public Dim CostMethodologyCmt As String = ""
		Public Dim CostGrowthJustification As String = ""
		Public Dim MustFund As String = ""
		Public Dim RequestedFundSource As String = ""
		Public Dim ArmyInitiativeDirective As String = ""
		Public Dim CommandInitiativeDirective As String = ""
		Public Dim ActivityExercise As String = ""
		Public Dim ITCyberReq As String = ""
		Public Dim UIC As String = ""
		Public Dim FlexField1 As String = ""
		Public Dim FlexField2 As String = ""
		Public Dim FlexField3 As String = ""
		Public Dim FlexField4 As String = ""
		Public Dim FlexField5 As String = ""
		Public Dim EmergingRequirement As String = ""
		Public Dim CPACandidate As String = ""
		Public Dim PBRSubmission As String = ""
		Public Dim UPLSubmission As String = ""
		Public Dim ContractNumber As String = ""
		Public Dim TaskOrder As String = ""
		Public Dim AwardTargetDate As String = ""
		Public Dim POPExpirationDate As String = ""
		Public Dim CME As String = ""
		Public Dim COREmail As String = ""
		Public Dim OwnerEmail As String = ""
		Public Dim Directorate As String = ""
		Public Dim Division As String = ""
		Public Dim Branch As String = ""
		Public Dim ReviewingPOCEmail As String = ""
		Public Dim MDEPFuncEmail As String = ""
		Public Dim NotificationEmailList As String = ""
		Public Dim flow As String = ""				'Calculated
		Public Dim REQ_ID As String = ""			'Calculated
		Public Dim command As String = ""			'Calculated
		Public Dim scenario As String = ""			'Calculated
		
		Public Dim valid As Boolean = True
		Public Dim validationError As String = ""
		Public Dim UserName As String = ""
		
'		Public Sub SetValidationError(ByVal valError As String)
'			If String.IsNullOrWhiteSpace(validationError) Then
'				validationError = "REQ " & title & " has errors. " &  valError
'			Else
'				validationError = validationError  & ", "  & valError
'			End If
'		End Sub
		
		Public Function StringOutput() As String
			Dim output As String = Me.validationError & "," & _
			Me.command & "," & _	
			Me.Entity & "," &  _
			Me.APPN & "," &  _
			Me.MDEP & "," &  _
			Me.SAG_APE & "," &  _
			Me.DollarType & "," &  _
			Me.CostCategory & "," &  _
			Me.CivType & "," &  _
			Me.Cycle & "," &  _
			Me.COM_M1 & "," &  _
			Me.COM_M2 & "," &  _
			Me.COM_M3 & "," &  _
			Me.COM_M4 & "," &  _
			Me.COM_M5 & "," &  _
			Me.COM_M6 & "," &  _
			Me.COM_M7 & "," &  _
			Me.COM_M8 & "," &  _
			Me.COM_M9 & "," &  _
			Me.COM_M10 & "," &  _
			Me.COM_M11 & "," &  _
			Me.COM_M12 & "," &  _
			Me.COM_Carryover & "," &  _
			Me.OBL_M1 & "," &  _
			Me.OBL_M2 & "," &  _
			Me.OBL_M3 & "," &  _
			Me.OBL_M4 & "," &  _
			Me.OBL_M5 & "," &  _
			Me.OBL_M6 & "," &  _
			Me.OBL_M7 & "," &  _
			Me.OBL_M8 & "," &  _
			Me.OBL_M9 & "," &  _
			Me.OBL_M10 & "," &  _
			Me.OBL_M11 & "," &  _
			Me.OBL_M12 & "," &  _
			Me.OBL_Carryover & "," &  _
			Me.FTE & "," &  _
			Me.Title & "," & _
			Me.Description & "," & _
			Me.Justification & "," & _
			Me.ImpactNotFunded & "," & _
			Me.RiskNotFunded & "," & _
			Me.CostMethodologyCmt & "," & _
			Me.CostGrowthJustification & "," & _
			Me.MustFund & "," & _
			Me.RequestedFundSource & "," & _
			Me.ArmyInitiativeDirective & "," & _
			Me.CommandInitiativeDirective & "," & _
			Me.ActivityExercise & "," & _
			Me.ITCyberReq & "," & _
			Me.UIC & "," & _
			Me.FlexField1 & "," & _
			Me.FlexField2 & "," & _
			Me.FlexField3 & "," & _
			Me.FlexField4 & "," & _
			Me.FlexField5 & "," & _
			Me.EmergingRequirement & "," & _
			Me.CPACandidate & "," & _
			Me.PBRSubmission & "," & _
			Me.UPLSubmission & "," & _
			Me.ContractNumber & "," & _
			Me.TaskOrder & "," & _
			Me.AwardTargetDate & "," & _
			Me.POPExpirationDate & "," & _
			Me.CME & "," & _
			Me.COREmail & "," & _
			Me.OwnerEmail & "," & _
			Me.Directorate & "," & _
			Me.Division & "," & _
			Me.Branch & "," & _
			Me.ReviewingPOCEmail & "," & _
			Me.MDEPFuncEmail & "," & _
			Me.NotificationEmailList & "," & _
			Me.flow & "," & _				
			Me.REQ_ID & "," & _			
			Me.scenario	 & "," & _		
			Me.UserName
			
			
			Return output
		End Function
	End Class
#End Region


End Namespace