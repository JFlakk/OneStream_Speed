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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_UFR_Helper
	Public Class MainClass
		Private si As SessionInfo
		Public globals As BRGlobals
		Private api As Object
		Private args As DashboardExtenderArgs
		Private ParamsToClear As String
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args	
				Me.ParamsToClear = ParamsToClear
				
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						Dim dbExt_LoadResult As New XFLoadDashboardTaskResult()
						Select Case args.FunctionName.ToLower()
							Case "load_req_detailsdashboard"
								dbExt_LoadResult = Me.load_req_detailsdashboard()
								Return dbExt_LoadResult	
'								Case "load_cmd_SPLN_header"
'									dbExt_LoadResult = Me.load_cmd_SPLN_header()
'									Return dbExt_LoadResult
						End Select		
						
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
						Dim dargs As New DashboardExtenderArgs
						dargs.FunctionName = "Check_WF_Complete_Lock"
						Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, dargs)
						If Not sWFStatus.XFContainsIgnoreCase("unlock")
							dbExt_ChangedResult.IsOK = False
							dbExt_ChangedResult.ShowMessageBox = True
							dbExt_ChangedResult.Message = vbCRLF & "Current workflow step is locked. Please contact your requriements manager to open access." & vbCRLF
							Return dbExt_ChangedResult
						End If	

						Select Case args.FunctionName.ToLower()
							Case "clearkey5param"
								dbExt_ChangedResult = Me.ClearKey5param()
								Return dbExt_ChangedResult
							
							Case "clearkey5params_nofcappn"
								dbExt_ChangedResult = Me.ClearKey5params_NoFCAPPN()
								Return dbExt_ChangedResult
								
							Case "deleterequirementids"		
								dbExt_ChangedResult = Me.DeleteRequirementID()
								Return dbExt_ChangedResult	
						
							Case "cacheprompts"
								'--- Update to check the workflow ---
'								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
'								If dbExt_ChangedResult.ShowMessageBox = True Then
									dbExt_ChangedResult = Me.CachePrompts()
'								End If
								Return dbExt_ChangedResult
'								Me.CachePrompts(si,globals,api,args)

#Region "Delete"
'							Case "submit_reqs", "importreq", "manage_req_status"
'								'--- Update to check the workflow ---
''								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
''								If dbExt_ChangedResult.ShowMessageBox = True Then
''									Return dbExt_ChangedResult
''								End If
'								dbExt_ChangedResult = Me.Update_Status()
'								Return dbExt_ChangedResult
#End Region								
							Case "updateufrstatus"
								dbExt_ChangedResult = Me.UpdateUFRStatus()
								Return dbExt_ChangedResult
								
							Case "dynsessionset"
								dbExt_ChangedResult = Me.DynSessionSet()
								Return dbExt_ChangedResult
								
							Case "calculateweightedscoreandrank"
								dbExt_ChangedResult = Me.CalculateWeightedScoreAndRank()
								Return dbExt_ChangedResult
								
								
							Case "copysplnreq"
								dbExt_ChangedResult = Me.CopySPLNREQ()
								Return dbExt_ChangedResult
								
								
								
							End Select
							
						
				End Select
				

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Constants"
		'Private BR_REQDataSet As New Workspace.CMD_SPLN.CMD_SPLN_Assembly.BusinessRule.DashboardDataSet.CMD_SPLN_DataSet.MainClass()
		Public GEN_General_String_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardStringFunction.GBL_String_Helper.MainClass
		Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
#End Region

#Region "Helper Methods"	

#Region "Set Default APPN Parameter"
Public Function SetDeafultAPPNParam() As Object

	Dim dKeyVal As New Dictionary(Of String, String)
							
	dKeyVal.Add("ML_CMD_UFR_FormulateAPPN","OMA")
	
	If args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_UFR_RelatedREQList","NA") <> "NA" Then
		dKeyVal.Add("BL_CMD_UFR_RelatedREQList",args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues.XFGetValue("BL_CMD_UFR_RelatedREQList","NA"))
	End If	
'Added 2 line to clear user cache before launching getcascadingfilters
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_UFR_CascadingFilterCache",$"CMD_UFR_rebuildparams_APPN","")
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName,$"CMD_UFR_CascadingFilterCache",$"CMD_UFR_rebuildparams_Other","")	
	Return Me.SetParameter(si, globals, api, dKeyVal)
		
End Function
#End Region

#Region "Is REQ Title Blank"
'		'Updated: EH 8/28/2024 - Ticket 1565 Title member script updated to REQ_Shared scenario
'		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
'		Public Function BlankTitleCheck(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'			Dim sFlow As String = args.NameValuePairs.XFGetValue("REQFlow").Trim
'			Dim sEntity As String = args.NameValuePairs.XFGetValue("REQEntity").Trim
'			Dim sREQTitleMbrScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & sFlow & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'			Dim sREQTitle As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sREQTitleMbrScript).DataCellEx.DataCellAnnotation	

'			If sREQTitle = "" Then

'				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
'			    Dim objScriptValTitle As New MemberScriptAndValue
'				objScriptValTitle.CubeName = sCube
'				objScriptValTitle.Script = sREQTitleMbrScript
'				objScriptValTitle.TextValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
'				objScriptValTitle.IsNoData = False
'				objListofScriptsTitle.Add(objScriptValTitle)
				
'				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
				
'				Throw New Exception("Requirement title cannot be blank." & environment.NewLine &  " Please enter a title and click save button.")
'			End If	
'			Return Nothing
		
'		End Function
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

#Region"Get REQID"
        Public Function Get_FC_REQ_ID(si As SessionInfo, fundCenter As String) As String
			Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
		  ' Query to get the highest UFR_ID from both tables
            Dim SQL As String = $"
                SELECT MAX(CAST(SUBSTRING(UFR_ID, CHARINDEX('_', UFR_ID) + 1, LEN(UFR_ID)) AS INT)) AS MaxID
                FROM (
                    SELECT UFR_ID FROM XFC_CMD_UFR  WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                    UNION ALL
                    SELECT UFR_ID FROM XFC_CMD_UFR WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                ) AS Combined"
			
			Dim dtREQID As DataTable = New DataTable()
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 	dtREQID = BRApi.Database.ExecuteSql(dbConn,SQL,True)
			End Using
			
			Dim nextID As Integer = 1
			If (Not dtREQID Is Nothing) AndAlso (Not dtREQID.Rows.Count = 0) AndAlso (Not IsDBNull(dtREQID.Rows(0)("MaxID"))) Then
			    Dim maxID As Integer = Convert.ToInt32(dtREQID.Rows(0)("MaxID"))
                nextID = maxID + 1
			End If
			
			Dim modifiedFC As String = fundCenter
			modifiedFC = modifiedFC.Replace("_General", "")
			If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
			Dim nextREQ_ID As String = modifiedFC &"_" & nextID.ToString("D5")
'BRApi.ErrorLog.LogMessage(si,"nextREQ_ID: " &nextREQ_ID)				

			Return nextREQ_ID
        End Function
#End Region	

#End Region

#Region "Clear Key5 params"
		Public Function ClearKey5param()
			Try
				Dim paramsToClear As String = "ML_CMD_UFR_FormulateAPEPT," & _
												"ML_CMD_UFR_FormulateFundCode," & _
												"ML_CMD_UFR_FormulateAPPN," & _
												"ML_CMD_UFR_FormulateCType," & _
												"ML_CMD_UFR_FormulateMDEP," & _
												"ML_CMD_UFR_FormulateDollarType," & _
												"ML_CMD_UFR_FormulateCommitItem," & _
												"ML_CMD_UFR_FormulateObjectClass," & _
												"ML_CMD_UFR_FormulateSAG" 
									
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
				Dim paramsToClear As String = "ML_CMD_UFR_FormulateAPEPT," & _
												"ML_CMD_UFR_FormulateFundCode," & _
												"ML_CMD_UFR_FormulateCType," & _
												"ML_CMD_UFR_FormulateMDEP," & _
												"ML_CMD_UFR_FormulateDollarType," & _
												"ML_CMD_UFR_FormulateCommitItem," & _
												"ML_CMD_UFR_FormulateObjectClass," & _
												"ML_CMD_UFR_FormulateSAG" 
			
				Dim selectionChangedTaskResult As XFSelectionChangedTaskResult = Me.ClearSelections(si, globals, api, args, paramsToClear)	
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True	
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
				Return selectionChangedTaskResult
		
		    Catch ex As Exception
		        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		    End Try
		End Function
#End Region	
	
#Region "Cache Prompts"
		Public Function CachePrompts()
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sREQ As String = args.NameValuePairs.XFGetValue("REQ")
'			Dim sSAG As String = args.NameValuePairs.XFGetValue("SAG")
'			Dim sMode As String = args.NameValuePairs.XFGetValue("mode","")
			Dim sMode As String = args.NameValuePairs.XFGetValue("Mode")
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
'			BRapi.ErrorLog.LogMessage(si, "Entity = " & sEntity)
'			BRapi.ErrorLog.LogMessage(si, "REQ = " & sREQ)
'			BRapi.ErrorLog.LogMessage(si, "Mode = " & sMode)
'			BRapi.ErrorLog.LogMessage(si, "sDashboard = " & sDashboard)
			
			'Clear Cache
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Entity", "")
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "Dashboard", "")
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQPrompts", "REQ", "")
			
			If sMode.XFContainsIgnoreCase("DeleteREQUFR") And String.IsNullOrWhiteSpace(sEntity) Then
				Throw New Exception("Please select a funds center.")
				Return Nothing
			End If
			
			If sMode.XFContainsIgnoreCase("DeleteREQUFR") And String.IsNullOrEmpty(sREQ) Then
				Throw New Exception("Please select a UFR.")
				Return Nothing
			End If 
			
			If Not String.IsNullOrWhiteSpace(sEntity) Then
					BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity",sEntity)
			End If
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ",sREQ)
			Brapi.ErrorLog.LogMessage(si, "REQ PROMPTs")
			
'			If sMode = "Formulate" Then
'				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ",sSAG)
'			End If
			
			If Not String.IsNullOrWhiteSpace(sDashboard) Then
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Dashboard",sDashboard)
			End If

			Return Nothing
		End Function
#End Region	

#Region "Dynamic Session Set"
		'{Workspace.CMD_UFR.CMD_UFR_Assembly.CMD_UFR_Helper}{DynSessionSet}{Dashboard = [], SelectedFormulateDashboard = [], Entity = []}
		Public Function DynSessionSet()
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			Dim sSelectedFormulateDashboard As String = args.NameValuePairs.XFGetValue("SelectedFormulateDashboard")
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity")
			Dim sGridREQIds As String = args.NameValuePairs.XFGetValue("GridREQIDs")
			Dim sCbxREQIds As String = args.NameValuePairs.XFGetValue("CbxREQIds")
			Dim sCbxREQIdsClean As String
			
			If Not sCbxREQIds = ""
				Brapi.ErrorLog.LogMessage(si, "sCbxREQIdsClean = " & sCbxREQIdsClean)
				sCbxREQIdsClean  = sCbxREQIds.Split(" "c)(1)
			End If
			
#Region "Delete"
'			BRapi.ErrorLog.LogMessage(si, "Entity = " & sEntity)
'			BRapi.ErrorLog.LogMessage(si, "REQ = " & sREQ)
'			BRapi.ErrorLog.LogMessage(si, "Mode = " & sMode)
'			BRapi.ErrorLog.LogMessage(si, "sDashboard = " & sDashboard)
#End Region			
			'Clear Session
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"GenericRetrieve","FormulateDashboard","")
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
			
			If sDashboard = "Formulate" Then
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"GenericRetrieve","FormulateDashboard",sSelectedFormulateDashboard)
			End If
			
			If sDashboard = "UFRList_Grid" Then
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs",sGridREQIds)
			Else If sDashboard = "UFRReport" Then
				Brapi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "REQRetrieve", "ReqIDs", sCbxREQIdsClean)
			End If
			
			Return Nothing
		End Function
#End Region	

#Region "Delete Requirement ID"
		Public Function DeleteRequirementID() As XFSelectionChangedTaskResult
			Try
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				' Get list of REQ IDs from args and split into list
				Dim req_IDsRaw As String = args.NameValuePairs.XFGetValue("req_IDs", "")
				Brapi.ErrorLog.LogMessage(si, "req_IDsRaw = " & req_IDsRaw.ToString())
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
					Dim sqa_xfc_cmd_UFR = New SQA_XFC_CMD_UFR(connection)
					Dim sql As String = $"SELECT * 
										FROM XFC_CMD_UFR 
										WHERE WFScenario_Name = @WFScenario_Name
										AND WFCMD_Name = @WFCMD_Name
										AND WFTime_Name = @WFTime_Name"

					Dim paramList As New List(Of SqlParameter) From {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
					}
'brapi.ErrorLog.LogMessage(si,"here2")
					Dim sqa_xfc_cmd_UFR_details = New SQA_XFC_CMD_UFR_Details(connection)

					' Build details SQL using CMD_SPLN_REQ_ID IN (...) so both queries filter by the same parent identifiers
					Dim detailSql As String = $"SELECT * 
												FROM XFC_CMD_UFR_Details AS Req
       											LEFT JOIN XFC_CMD_UFR AS Dtl
       											ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
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
							sql &= " AND UFR_ID = @UFR_ID"
							detailSql &= " AND Dtl.UFR_ID = @UFR_ID"
							paramList.Add(New SqlParameter("@UFR_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
							detailParamList.Add(New SqlParameter("@UFR_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0)})
						Else
							Dim paramNames As New List(Of String)
							For i As Integer = 0 To Req_ID_List.Count - 1
								Dim pname As String = "@UFR_ID" & i
								paramNames.Add(pname)
								paramList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
								detailParamList.Add(New SqlParameter(pname, SqlDbType.NVarChar) With {.Value = Req_ID_List(i)})
							Next
							sql &= $" AND UFR_ID IN ({String.Join(",", paramNames)})"
							detailSql &= $" AND Dtl.UFR_ID IN ({String.Join(",", paramNames)})"
						End If
					End If
'brapi.ErrorLog.LogMessage(si,"here4")
					sqa_xfc_cmd_UFR.Fill_XFC_CMD_UFR_DT(sqa, REQDT, sql, paramList.ToArray())
					sqa_xfc_cmd_UFR_details.Fill_XFC_CMD_UFR_DETAILS_DT(sqa, REQDetailDT, detailSql, detailParamList.ToArray())

					' Mark rows for deletion in both tables
					Dim entitiesFromReqList As New List(Of String)
					For Each reqId As String In Req_ID_List
						Dim rows As DataRow() = REQDT.Select($"UFR_ID = '{reqId}'")
						Dim GUIDROW As String = rows.FirstOrDefault().Item("CMD_UFR_Tracking_No").tostring
						Dim ent As String = rows.FirstOrDefault().Item("Entity").ToString
						entitiesFromReqList.Add(ent)
						Status  = rows.FirstOrDefault().Item("Command_UFR_Status").tostring
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
						Dim detailRows As DataRow() = REQDetailDT.Select($"CMD_UFR_Tracking_No = '{GUIDROW}'")
						For Each drow As DataRow In detailRows
							drow.Delete()
						Next
					Next
					' Persist changes back to DB
					sqa_xfc_cmd_UFR_details.Update_XFC_CMD_UFR_Details(REQDetailDT, sqa)
					sqa_xfc_cmd_UFR.Update_XFC_CMD_UFR(REQDT, sqa)
					
					'clear cube
'brapi.ErrorLog.LogMessage(si,"here6 Delete")					
					Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "70 CMD UFR")	
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
'					customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
'					BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
					

				End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

#End Region

#Region"load_req_detailsdashboard"
Public Function load_req_detailsdashboard() As XFLoadDashboardTaskResult
	Dim LoadDBTaskResult As New XFLoadDashboardTaskResult()
	LoadDBTaskResult.ChangeCustomSubstVarsInDashboard = True
	
	Dim reqTitle = args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
	
	If reqTitle <> String.Empty Or Not String.IsNullOrEmpty(reqTitle)
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_UFR_REQDetailsShowHide","CMD_UFR_0_Body_UFRList")
	Else	
		UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMD_UFR_REQDetailsShowHide","CMD_UFR_0_Body_UFRList")
	End If
	Return LoadDBTaskResult
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

#Region "Status Updates"
'	Public Function Update_Status() As xfselectionchangedTaskResult
'		Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
'		Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs","NA")
'		Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
'		Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
'		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'		Dim wfStepAllowed As Boolean = True
		
'		Try
'			If req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate") And Not Dashboard.XFContainsIgnoreCase("Mpr") Then
'				Me.Update_REQ_Status("Formulate")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Validate") Then
'				Me.Update_REQ_Status("Validate")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Staff") Then
'				Me.Update_REQ_Status("Staff")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Prioritize") Then
'				Me.Update_REQ_Status("Prioritize")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve") Then
'				Me.Update_REQ_Status("Approve")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Import") Then
'				Me.Update_REQ_Status("Formulate")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Rollover") Then
'				Me.Update_REQ_Status("Formulate")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve CMD UFR") Then
'				Me.Update_REQ_Status("Approve CMD")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Formulate CMD UFR") And Dashboard.XFContainsIgnoreCase("Mpr") Then
'				Me.Update_REQ_Status("Formulate CMD")
''			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Approve") Then
''				Me.Update_REQ_Status("Approve")
'			ElseIf req_IDs <> "NA" And wfProfileName.XFContainsIgnoreCase("Manage") Then
'				Me.ManageREQStatusUpdated(si, globals, api, args, "")
'			End If
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try
		
'		Return dbExt_ChangedResult
'	End Function		''' <summary>
	''' Centralized helper to set a REQ workflow status, update history, send emails, and set last updated.
	''' </summary>
'	Private Function Update_REQ_Status(ByVal curr_Status As String) As xfselectionchangedTaskResult
'		Try
'			Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
'			Dim demote_comment As String = args.NameValuePairs.XFGetValue("demotecomment")
'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
'			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
'			Dim FCList As New List(Of String)
'			Dim ChildFCList As New List(Of String)
'			Dim ParentFCList As New List(Of String)
'			Dim AggFCList As New List(Of String)
'			Dim Status_manager As New Dictionary(Of String,String)
			
'			'Formulate to Next Level Staff
'			Status_manager.Add("L5_Formulate_UFR|Staff","L4_Staff_UFR")
'			Status_manager.Add("L4_Formulate_UFR|Staff","L3_Staff_UFR")
'			Status_manager.Add("L3_Formulate_UFR|Staff","L3_Staff_UFR")
'			Status_manager.Add("L2_Formulate_UFR|Staff","L2_Staff_UFR")
			
'			'Staff to Same Level Validate
'			Status_manager.Add("L4_Staff_UFR|Validate","L4_Validate_UFR")
'			Status_manager.Add("L3_Staff_UFR|Validate","L3_Validate_UFR")
'			Status_manager.Add("L2_Staff_UFR|Validate","L2_Validate_UFR")
'			Status_manager.Add("L1_Staff_UFR|Validate","L1_Validate_UFR")
			
'			'Validate to Same Level Prioritize
'			Status_manager.Add("L4_Validate_UFR|Prioritize","L4_Prioritize_UFR")
'			Status_manager.Add("L3_Validate_UFR|Prioritize","L3_Prioritize_UFR")
'			Status_manager.Add("L2_Validate_UFR|Prioritize","L2_Prioritize_UFR")
			
'			'Prioritize to Same Level Approve
'			Status_manager.Add("L4_Prioritize_UFR|Approve","L4_Approve_UFR")
'			Status_manager.Add("L3_Prioritize_UFR|Approve","L3_Approve_UFR")
'			Status_manager.Add("L2_Prioritize_UFR|Approve","L2_Approve_UFR")
'			Status_manager.Add("L1_Prioritize_UFR|Approve","L1_Approve_UFR")
			
'			'Approve to Next Level Staff
'			Status_manager.Add("L4_Approve_UFR|Staff","L3_Staff_UFR")
'			Status_manager.Add("L3_Approve_UFR|Staff","L2_Staff_UFR")
'			Status_manager.Add("L2_Approve_UFR|Staff","L1_Staff_UFR")
			
			
'			Status_manager.Add("L5_Validate_UFR|Approve","L4_Validate_UFR")
'			Status_manager.Add("L4_Validate_UFR|Approve","L3_Validate_UFR")
'			Status_manager.Add("L3_Validate_UFR|Approve","L3_Approve_UFR")
'			Status_manager.Add("L2_Validate_UFR|Approve","L2_Approve_UFR")
'			Status_manager.Add("L2_Approve_UFR|Final","L2_Final_UFR")
'			Status_manager.Add("L3_Approve_UFR|Validate","L2_Validate_UFR")
'			Status_manager.Add("L2_Formulate_UFR|Final","L2_Final_UFR")
'			Status_manager.Add("L2_Final_UFR|Formulate","L2_Formulate_UFR")
			
'			If args.NameValuePairs.XFGetValue("Demote") = "True" Then
'				#Region "Demote Statuses"
'				'--------------------------Demote Statuses----------------------------------
'				'Approve
'				Status_manager.Add("L2_Approve_UFR|L2_Validate_UFR","L2_Validate_UFR")
'				Status_manager.Add("L2_Approve_UFR|L3_Approve_UFR","L3_Approve_UFR")
'				Status_manager.Add("L2_Approve_UFR|L3_Validate_UFR","L3_Validate_UFR")
'				Status_manager.Add("L2_Approve_UFR|L3_Formulate_UFR","L3_Formulate_UFR")
'				Status_manager.Add("L2_Approve_UFR|L4_Approve_UFR","L4_Approve_UFR")
'				Status_manager.Add("L2_Approve_UFR|L4_Validate_UFR","L4_Validate_UFR")
'				Status_manager.Add("L2_Approve_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L2_Approve_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
				
'				Status_manager.Add("L3_Approve_UFR|L3_Validate_UFR","L3_Validate_UFR")
'				Status_manager.Add("L3_Approve_UFR|L3_Formulate_UFR","L3_Formulate_UFR")
'				Status_manager.Add("L3_Approve_UFR|L4_Approve_UFR","L4_Approve_UFR")
				
'				Status_manager.Add("L3_Approve_UFR|L4_Validate_UFR","L4_Validate_UFR")
'				Status_manager.Add("L3_Approve_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L3_Approve_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
				
'				Status_manager.Add("L4_Approve_UFR|L4_Validate_UFR","L4_Validate_UFR")
'				Status_manager.Add("L4_Approve_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L4_Approve_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
				
'				'Validate
'				Status_manager.Add("L2_Validate_UFR|L3_Approve_UFR","L3_Approve_UFR")
'				Status_manager.Add("L2_Validate_UFR|L3_Validate_UFR","L3_Validate_UFR")
'				Status_manager.Add("L2_Validate_UFR|L3_Formulate_UFR","L3_Formulate_UFR")
'				Status_manager.Add("L2_Validate_UFR|L4_Approve_UFR","L4_Approve_UFR")
'				Status_manager.Add("L2_Validate_UFR|L4_Validate_UFR","L4_Validate_UFR")
'				Status_manager.Add("L2_Validate_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L2_Validate_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
				
'				Status_manager.Add("L3_Validate_UFR|L3_Formulate_UFR","L3_Formulate_UFR")
'				Status_manager.Add("L3_Validate_UFR|L4_Approve_UFR","L4_Approve_UFR")
'				Status_manager.Add("L3_Validate_UFR|L4_Validate_UFR","L4_Validate_UFR")
'				Status_manager.Add("L3_Validate_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L3_Validate_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
				
'				Status_manager.Add("L4_Validate_UFR|L4_Formulate_UFR","L4_Formulate_UFR")
'				Status_manager.Add("L4_Validate_UFR|L5_Formulate_UFR","L5_Formulate_UFR")
'				'--------------------------Demote Statuses----------------------------------
'#End Region
'			End If 
			
'			Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
'			Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs")			
'			Dim Mode As String = args.NameValuePairs.XFGetValue("Mode")
'			If Mode.XFEqualsIgnoreCase("Single") Then 
'				req_IDs = req_IDs.Split(" ").Last()
'			Else 
'				req_IDs = req_IDs
'			End If 		
'			Dim Req_ID_List As List (Of String) =  StringHelper.SplitString(req_IDs, ",")
			
'			Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
'			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
'			If String.IsNullOrWhiteSpace(new_Status) Then 

'				Return dbExt_ChangedResult
'			Else
'				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'				Using connection As New SqlConnection(dbConnApp.ConnectionString)
'					connection.Open()
'					Dim sqa_xfc_cmd_UFR = New SQA_XFC_CMD_UFR(connection)
'					Dim SQA_XFC_CMD_UFR_DT = New DataTable()
'					Dim sqa_xfc_cmd_UFR_details = New SQA_XFC_CMD_UFR_DETAILS(connection)
'					Dim SQA_XFC_CMD_UFR_DETAILS_DT = New DataTable()
'					Dim sqa = New SqlDataAdapter()
'					Dim dtfilter As String = String.Empty
'					Dim paramNames As New List(Of String)
					
'				    If Req_ID_List.Count > 1 Then
'				        For i As Integer = 0 To Req_ID_List.Count - 1
'				            Dim paramName As String = "'" & Req_ID_List(i) & "'"
'				            paramNames.Add(paramName)
'				        Next
'						dtfilter = String.Join(",", paramNames)
'				    ElseIf Req_ID_List.Count = 1 Then
'						Dim paramName As String = "'" & Req_ID_List(0) & "'"
'			            paramNames.Add(paramName)
'						dtfilter = String.Join(",", paramNames)
'				    End If
'					'Fill the DataTable With the current data From FMM_Dest_Cell
'					Dim sql As String = $"SELECT * 
'										FROM XFC_CMD_UFR
'										WHERE WFScenario_Name = @WFScenario_Name
'										AND WFCMD_Name = @WFCMD_Name
'										AND WFTime_Name = @WFTime_Name
'										AND UFR_ID in ({DTFilter})"
			
					
'				    ' 2. Create a list to hold the parameters
'				    Dim paramList As New List(Of SqlParameter) From {
'			        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
'			        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
'			        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
'			   		}
					
'				    ' 3. Dynamically build the rest of the query and parameters

'				    ' 4. Convert the list to the array your method expects
'				    Dim sqlparams As SqlParameter() = paramList.ToArray()

'					'IMPROVEMENT - Filter by fund center 
'						sqa_xfc_cmd_UFR.Fill_XFC_CMD_UFR_DT(sqa, SQA_XFC_CMD_UFR_DT, sql, sqlparams)
'					' --- get list of parent IDs and select all detail rows in one query ---
'					Dim parentIds As New List(Of String)()
'					For Each parentRow As DataRow In SQA_XFC_CMD_UFR_DT.Rows   '.Select(dtfilter)
'						If Not IsDBNull(parentRow("CMD_UFR_Tracking_No")) Then
'							Dim columnValue As Object = parentRow("CMD_UFR_Tracking_No")
'            				Dim reqIdAsGuid As Guid = Guid.Parse(columnValue.ToString())
'							parentIds.Add(reqIdAsGuid.ToString())
'						End If
'					Next
					
'					Dim DetaildtFilter As String 
'					If parentIds.Count > 0 Then
'						' Build a comma separated list of ints and query details table with IN (...)
'						' NOTE: parentIds are integers sourced from DB so this string concatenation is safe in this context.
'						Dim DetailparamNames As New List(Of String)
'						For i As Integer = 0 To parentIds.Count - 1
'	                        'detailsParams.Add(New SqlParameter($"@ID{i}", SqlDbType.NVarChar) With {.Value = parentIds(i)})
'							'Dim DetailparamName As String = "'{" & parentIds(i) & "}'"
'							'Dim Guiddetailparamname As String = $"CONVERT({DetailparamName},'System.Guid')"
'							Dim DetailparamName As String = "'" & parentIds(i) & "'"
'	            			'DetailparamNames.Add(Guiddetailparamname)
'							DetailparamNames.Add(DetailparamName)
'                    	Next
						
'						DetaildtFilter = String.Join(",", DetailparamNames)
						
'						Dim idsCsv As String = String.Join(",", parentIds)
'						sql = $"SELECT * 
'								FROM XFC_CMD_UFR_Details 
'								WHERE WFScenario_Name = @WFScenario_Name
'								AND WFCMD_Name = @WFCMD_Name
'								AND WFTime_Name = @WFTime_Name
'								AND CMD_UFR_Tracking_No in ({DetaildtFilter})"

'						  Dim detailsParams As New List(Of SqlParameter) From {
'								New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
'								New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
'								New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
'						   }
					

'							sqa_xfc_cmd_UFR_details.Fill_XFC_CMD_UFR_DETAILS_DT(sqa, SQA_XFC_CMD_UFR_DETAILS_DT, sql, detailsParams.ToArray())
'					End If
					
'					' At this point detailsAllDT contains all matching XFC_CMD_SPLN_REQ_Details rows (if any).
'					' Update all returned parent rows

'					For Each row As DataRow In SQA_XFC_CMD_UFR_DT.Rows
'						'Dim wfStepAllowed = Workspace.GBL.GBL_Assembly.GBL_Helpers.Is_Step_Allowed(si, args, curr_Status, row("Entity"))
'						Dim wfStepAllowed As String = Workspace.CMD_UFR.CMD_UFR_Assembly.CMD_UFR_Utilities.GetProcCtrlVal(si, row("Entity"))
'						'GetProcCtrlVal
'						If wfStepAllowed = "No" Then
'							BRAPI.ErrorLog.LogMessage(si,"Hit3")
'							dbExt_ChangedResult.ShowMessageBox = True
'							If Not String.IsNullOrWhiteSpace(dbExt_ChangedResult.Message) Then
'								dbExt_ChangedResult.Message &= Environment.NewLine
'							End If
'							dbExt_ChangedResult.Message &= $"Cannot change status of UFR_ID '{row("UFR_ID")}' at this time. Contact requirements manager."
'						Else
'							Dim existingStatus As String = ""
'							If Not IsDBNull(row("Command_UFR_Status")) Then existingStatus = row("Command_UFR_Status").ToString().Trim()

'							Dim lookupKey As String = existingStatus & "|" & new_Status
'							Dim resolvedStatus As String
'							If Status_manager.ContainsKey(lookupKey) Then
'								resolvedStatus = Status_manager(lookupKey)
'							Else
'								resolvedStatus = existingStatus
'								dbExt_ChangedResult.ShowMessageBox = True
'								dbExt_ChangedResult.Message &= $"UFR_ID '{row("UFR_ID")}' has an incorrect status, can't be updated."
'							End If
'							If Not String.IsNullOrEmpty(demote_comment) Then
'								row("Demotion_Comment") = demote_comment
'							End If 
'							row("Command_UFR_Status") = resolvedStatus
'							row("Update_User") = si.UserName
'							row("Update_Date") = DateTime.Now

'							' If we have details loaded, update the detail rows that belong to this parent now
'							If SQA_XFC_CMD_UFR_DETAILS_DT IsNot Nothing AndAlso SQA_XFC_CMD_UFR_DETAILS_DT.Rows.Count > 0 AndAlso Not IsDBNull(row("CMD_UFR_Tracking_No")) Then
							
'								Dim pid As String = ""
'								pid = row("CMD_UFR_Tracking_No").ToString()
'								Dim filterExpr As String = String.Format("CMD_UFR_Tracking_No = '{0}'", pid)
'								Dim matchingDetails() As DataRow = SQA_XFC_CMD_UFR_DETAILS_DT.Select(filterExpr)
																	
'								For Each drow As DataRow In matchingDetails
'									If Not FCList.contains($"{drow("Entity")}")
'											FCList.Add($"{drow("Entity")}")
'									End If 

'									globals.SetStringValue($"FundsCenterStatusUpdates - {drow("Entity")}", $"{existingStatus}|{resolvedStatus}")
'									drow("Flow") = resolvedStatus
'									drow("Update_User") = si.UserName
'									drow("Update_Date") = DateTime.Now
'								Next
'							End If
'							Dim EmailwfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'							If  Not EmailwfProfileName.XFContainsIgnoreCase("Import")
'								Workspace.GBL.GBL_Assembly.GBL_Helpers.SendStatusChangeEmail(si, globals,api,args,resolvedStatus, row("UFR_ID"))
'							End If 
'						End If
'					Next
'					' Persist all changes back to the database
'					sqa_xfc_cmd_UFR.Update_XFC_CMD_UFR(SQA_XFC_CMD_UFR_DT, sqa)
'					sqa_xfc_cmd_UFR_details.Update_XFC_CMD_UFR_DETAILS(SQA_XFC_CMD_UFR_DETAILS_DT, sqa)
'				End Using
'			End If
			
'			Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList)
'			Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
'			Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
'			Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"70 CMD UFR")
'			Dim customSubstVars As New Dictionary(Of String, String) 
'			customSubstVars.Add("EntList",String.Join(",",BaseEntityList))
'			customSubstVars.Add("ParentEntList",String.Join(",",ParentEntityList))
'			customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
'			Dim currentYear As String = wfInfoDetails("TimeName")
'			Dim nextyear As String = currentYear + 1
''				customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")

''				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
			
'			'Run consolidation for entities in the correct order
''				Dim consolEntityList As list (Of String) = EntityLists.Item3
''				Dim L4Entity,L3Entity,L2Entity,L1Entity As String 
''				For Each consolentity As String In consolEntityList
''					Dim Entitylevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, consolentity)
''					If consolentity = sCube Then EntityLevel = "L1"
''					Select Case Entitylevel
''					Case "L4"
''						L4Entity = $"{L4Entity},E#{ConsolEntity}"
''					Case "L3"
''						L3Entity = $"{L3Entity},E#{ConsolEntity}"
''					Case "L2"
''						L2Entity = $"{L2Entity},E#{ConsolEntity}"
''					Case "L1"
''						L1Entity = $"{L1Entity},E#{ConsolEntity}"
''					End Select
''				Next
''				customSubstVars.Add("consolEntityList","Default")
''				If Not String.IsNullOrEmpty(L4Entity) Then
''					customSubstVars("consolEntityList") = L4Entity 
''					BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
''				End If 
''				If Not String.IsNullOrEmpty(L3Entity) Then
''					customSubstVars("consolEntityList") = L3Entity 
''					BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
''				End If 
''				If Not String.IsNullOrEmpty(L2Entity) Then
''					customSubstVars("consolEntityList") = L2Entity 
''					BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
''				End If 
''				If Not String.IsNullOrEmpty(L1Entity) Then
''					customSubstVars("consolEntityList") = L1Entity 
''					BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Consol_Load_to_Cube", customSubstVars)
''				End If 
			
'			Return dbExt_ChangedResult
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try
'	End Function


'	Public Function ManageREQStatusUpdated(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, Optional ByVal UFR As String =  "") As Boolean
'		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
'		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'		'Dim wfLevel As String = wfProfileName.Substring(0,2)
'		Dim sUFR As String = args.NameValuePairs.XFGetValue("UFR", UFR)
'		Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundCenter")
'		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'		Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'		Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		
'		'Args To pass Into dataset BR
'		Dim dsArgs As New DashboardDataSetArgs 
'			dsargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
'			dsArgs.DataSetName = "REQListByEntity"
'			dsArgs.NameValuePairs.XFSetValue("Entity", sFundCenter)
'			dsargs.NameValuePairs.XFSetValue("CubeView", "")
						

'		Return Nothing				
		
'	End Function 

	
#End Region 'DELETE
	
#Region "UpdateUFRStatus"
'	{Workspace.CMD_UFR.CMD_UFR_Assembly.CMD_UFR_Helper}{UpdateUFRStatus}{req_IDs = [|!IV_CMD_UFR_REQ_IDS!|], Action = [], SelectionType = []}
	Public Function UpdateUFRStatus() As xfselectionchangedTaskResult
		Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
		Dim sSelectionType As String = args.NameValuePairs.XFGetValue("SelectionType")
		Dim sAction As String = args.NameValuePairs.XFGetValue("Action")
'		Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs")
'		Dim Req_ID_List As List (Of String) =  StringHelper.SplitString(req_IDs, ",")
		Dim req_IDsRaw As String = args.NameValuePairs.XFGetValue("req_IDs")
		Dim req_IDs As String
		
		If sSelectionType = "cbxList" Then
			req_IDs = req_IDsRaw.Split(" "c)(1)
			Brapi.ErrorLog.LogMessage(si, "cbxList Hit: req_IDs = " & req_IDs)
		Else
			req_IDs = req_IDsRaw
		End If
		
		
		Dim Req_ID_List As List (Of String) =  StringHelper.SplitString(req_IDs, ",")
		Dim FCList As New List(Of String)
		Dim ChildFCList As New List(Of String)
		Dim ParentFCList As New List(Of String)
		Dim AggFCList As New List(Of String)

		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim dtWorkflowManageStatus As DataTable = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWorkflowManageStatus(si, globals, api, args, ParamsToClear)

#Region "Troubleshooting - Pull count of rows from table"
'		Brapi.ErrorLog.LogMessage(si, "Rows Count = " & dtWorkflowManageStatus.Rows.Count)
#End Region

		Dim dictStatus As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
		
		For Each row As DataRow In dtWorkflowManageStatus.Rows
			
			Dim currentStatusRaw As String = row("Current_Status").ToString()
			Dim ActionRaw As String = row("Action").ToString()
			
			Dim currentStatus As String = currentStatusRaw.Trim()
			Dim Action As String = ActionRaw.Trim()


			Dim newStatus As String = row("New_Status").ToString()
			Dim key As String = $"{currentStatus}|{Action}"
			
			dictStatus(key) = newStatus
			
		Next
		
#Region "Troubleshooting - to pull dictionary keys and values"		
'		For Each kvp As KeyValuePair(Of String, String) In dictStatus
'			Brapi.ErrorLog.LogMessage(si, $"Key = '{kvp.Key}', Value(New_Status) = '{kvp.Value}'")
'		Next
#End Region
		
		Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		Using connection As New SqlConnection(dbConnApp.ConnectionString)
			connection.Open()
			Dim sqa_xfc_cmd_UFR = New SQA_XFC_CMD_UFR(connection)
			Dim SQA_XFC_CMD_UFR_DT = New DataTable()
			Dim sqa_xfc_cmd_UFR_details = New SQA_XFC_CMD_UFR_DETAILS(connection)
			Dim SQA_XFC_CMD_UFR_DETAILS_DT = New DataTable()
			Dim sqa = New SqlDataAdapter()
			Dim dtfilter As String = String.Empty
			Dim paramNames As New List(Of String)
			
		    If Req_ID_List.Count > 1 Then
		        For i As Integer = 0 To Req_ID_List.Count - 1
		            Dim paramName As String = "'" & Req_ID_List(i) & "'"
		            paramNames.Add(paramName)
		        Next
				dtfilter = String.Join(",", paramNames)
		    ElseIf Req_ID_List.Count = 1 Then
				Dim paramName As String = "'" & Req_ID_List(0) & "'"
	            paramNames.Add(paramName)
				dtfilter = String.Join(",", paramNames)
		    End If
			
			Dim sql As String = $"SELECT * 
								FROM XFC_CMD_UFR
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND UFR_ID in ({DTFilter})"
	
			
		    ' 2. Create a list to hold the parameters
		    Dim paramList As New List(Of SqlParameter) From {
	        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
	        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
	        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
	   		}
		
		    Dim sqlparams As SqlParameter() = paramList.ToArray()

			sqa_xfc_cmd_UFR.Fill_XFC_CMD_UFR_DT(sqa, SQA_XFC_CMD_UFR_DT, sql, sqlparams)
			
			' --- get list of parent IDs and select all detail rows in one query ---
			Dim parentIds As New List(Of String)()
			For Each parentRow As DataRow In SQA_XFC_CMD_UFR_DT.Rows   '.Select(dtfilter)
				If Not IsDBNull(parentRow("CMD_UFR_Tracking_No")) Then
					Dim columnValue As Object = parentRow("CMD_UFR_Tracking_No")
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
					Dim DetailparamName As String = "'" & parentIds(i) & "'"
					DetailparamNames.Add(DetailparamName)
            	Next
				
				DetaildtFilter = String.Join(",", DetailparamNames)
				
				Dim idsCsv As String = String.Join(",", parentIds)
				sql = $"SELECT * 
						FROM XFC_CMD_UFR_Details 
						WHERE WFScenario_Name = @WFScenario_Name
						AND WFCMD_Name = @WFCMD_Name
						AND WFTime_Name = @WFTime_Name
						AND CMD_UFR_Tracking_No in ({DetaildtFilter})"

				Dim detailsParams As New List(Of SqlParameter) From {
					New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
					New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
					New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
				}
			

				sqa_xfc_cmd_UFR_details.Fill_XFC_CMD_UFR_DETAILS_DT(sqa, SQA_XFC_CMD_UFR_DETAILS_DT, sql, detailsParams.ToArray())
			End If
					
				' At this point detailsAllDT contains all matching XFC_CMD_SPLN_REQ_Details rows (if any).
				' Update all returned parent rows

				For Each row As DataRow In SQA_XFC_CMD_UFR_DT.Rows
					' --- Check if in Staffing Process and User is correct staffing element to promote ---
					
#Region "WF and Security Group access for UFR Promotion"					
'					Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'					Brapi.ErrorLog.LogMessage(si, "wfProfileFullName = " & wfProfileFullName)
'					Dim sStudyCategory As String = row("Study_Category").ToString()
'					Brapi.ErrorLog.LogMessage(si, "sStudyCategory = " & sStudyCategory)
'					Dim targetGroup As String 
'					Dim isAuthorized As Boolean 
					
'					' --- CMD / SUB CDM Promotion Security Logic
'					If wfProfileFullName.XFContainsIgnoreCase("Staff") AndAlso wfProfileFullName.XFContainsIgnoreCase("CMD") Then
						
'						If sStudyCategory.XFContainsIgnoreCase("MILPERS") Then
'							targetGroup = "g_UFR_G1"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("INTEL") Then
'							targetGroup = "g_UFR_G2"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
						
'						Else If sStudyCategory.XFContainsIgnoreCase("OPS") Then
'							targetGroup = "g_UFR_G3"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("LOG") Then
'							targetGroup = "g_UFR_G4"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("PLANS") Then
'							targetGroup = "g_UFR_G5"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("IT") Then
'							targetGroup = "g_UFR_G6"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("TRAIN") Then
'							targetGroup = "g_UFR_G7"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("ACQ") Then
'							targetGroup = "g_UFR_G8"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("MILCON") Then
'							targetGroup = "g_UFR_G9"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("LEGAL") Then
'							targetGroup = "g_UFR_JAG"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
'						End If
						
'					' --- HQ Promotion Security Logic ---
'					Else If wfProfileFullName.XFContainsIgnoreCase("Staff") AndAlso wfProfileFullName.XFContainsIgnoreCase("HQ") Then
						
'						If sStudyCategory.XFContainsIgnoreCase("MILPERS") Or sStudyCategory.XFContainsIgnoreCase("TRAIN") Or sStudyCategory.XFContainsIgnoreCase("LOG") Or sStudyCategory.XFContainsIgnoreCase("IT")Then
'							targetGroup = "g_UFR_PEG"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("OPS") Or sStudyCategory.XFContainsIgnoreCase("PLANS") Or sStudyCategory.XFContainsIgnoreCase("INTEL")Then
'							targetGroup = "g_UFR_Integrator"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						Else If sStudyCategory.XFContainsIgnoreCase("ACQ") Or sStudyCategory.XFContainsIgnoreCase("MILCON") Then
'							targetGroup = "g_UFR_G8PAE"
'							isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'							If isAuthorized = False Then
'								Throw New Exception($"User: {si.UserName} is does not have access to promote UFR")
'							End If
							
'						End If	
							
'					End If
#End Region					
					
					'--- Check if step is allowed --- 
					Dim wfStepAllowed As String = Workspace.CMD_UFR.CMD_UFR_Assembly.CMD_UFR_Utilities.GetProcCtrlVal(si, row("Entity"))
					'GetProcCtrlVal
					If wfStepAllowed = "No" Then
						dbExt_ChangedResult.ShowMessageBox = True
						If Not String.IsNullOrWhiteSpace(dbExt_ChangedResult.Message) Then
							dbExt_ChangedResult.Message &= Environment.NewLine
						End If
						dbExt_ChangedResult.Message &= $"Cannot change status of UFR_ID '{row("UFR_ID")}' at this time. Contact requirements manager."
						
					'--- If Step is Allowed Continue ---
					Else
						Dim existingStatus As String = ""
						If Not IsDBNull(row("Command_UFR_Status")) Then existingStatus = row("Command_UFR_Status").ToString().Trim()
							
						Dim lookupKeyRaw As String = existingStatus.Trim() & "|" & sAction.Trim()
						Dim lookupKey As String = lookupKeyRaw.Trim()
						Dim resolvedStatus As String
						
						If dictStatus.ContainsKey(lookupKey) Then
							resolvedStatus = dictStatus(lookupKey)
						Else
							resolvedStatus = existingStatus
							dbExt_ChangedResult.ShowMessageBox = True
							dbExt_ChangedResult.Message &= $"UFR_ID '{row("UFR_ID")}' has an incorrect status, can't be updated."
						End If
						
						row("Command_UFR_Status") = resolvedStatus
						row("Update_User") = si.UserName
						row("Update_Date") = DateTime.Now
	
						' If we have details loaded, update the detail rows that belong to this parent now
						If SQA_XFC_CMD_UFR_DETAILS_DT IsNot Nothing AndAlso SQA_XFC_CMD_UFR_DETAILS_DT.Rows.Count > 0 AndAlso Not IsDBNull(row("CMD_UFR_Tracking_No")) Then
							Dim pid As String = ""
							pid = row("CMD_UFR_Tracking_No").ToString()
							Dim filterExpr As String = String.Format("CMD_UFR_Tracking_No = '{0}'", pid)
							Dim matchingDetails() As DataRow = SQA_XFC_CMD_UFR_DETAILS_DT.Select(filterExpr)
							For Each drow As DataRow In matchingDetails
								If Not FCList.contains($"{drow("Entity")}")
										FCList.Add($"{drow("Entity")}")
								End If 
								globals.SetStringValue($"FundsCenterStatusUpdates - {drow("Entity")}", $"{existingStatus}|{resolvedStatus}")
								drow("Flow") = resolvedStatus
								drow("Update_User") = si.UserName
								drow("Update_Date") = DateTime.Now
							Next
						End If
						Dim EmailwfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
						If  Not EmailwfProfileName.XFContainsIgnoreCase("Import")
							Workspace.GBL.GBL_Assembly.GBL_Helpers.SendStatusChangeEmail(si, globals,api,args,resolvedStatus, row("UFR_ID"))
						End If 
					End If
				Next
				
				' Persist all changes back to the database
				sqa_xfc_cmd_UFR.Update_XFC_CMD_UFR(SQA_XFC_CMD_UFR_DT, sqa)
				sqa_xfc_cmd_UFR_details.Update_XFC_CMD_UFR_DETAILS(SQA_XFC_CMD_UFR_DETAILS_DT, sqa)
				
			End Using			
	End Function



#End Region

#Region "CalculateWeightedScoreAndRank"
	Private Function CalculateWeightedScoreAndRank() As Object
        Try
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
	        Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
	        Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
	        Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
	        Dim REQTime As String = WFYear
	        Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FundsCenter")
			
			'Get cat and weight
			Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
			priCatNameAndWeight = Me.GetCategoryAndWeight()
			Dim categoryToColumnMap As Dictionary(Of String, String) = Me.GetCategoryColumnMappings()
			
			Dim req_IDs As Object = args.NameValuePairs.XFGetValue("req_IDs")
			
									
			Dim Req_ID_List As New List(Of String)()	
			
			Dim ID As String = req_IDs.ToString()
			If Not String.IsNullOrEmpty(ID) Then
				Req_ID_List = ID.Split(","c).ToList()
			End If
			
			Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
			Using connection As New SqlConnection(dbConnApp.ConnectionString)
				connection.Open()
				' --- Priority Table (XFC_CMD_UFR_Priority) ---
				Dim dt_Priority As New DataTable()
				Dim sqa3 As New SqlDataAdapter()
				Dim sqaReaderPriority As New SQA_XFC_CMD_UFR_Priority(connection)
				
				'Fill the DataTable With the current data From FMM_Dest_Cell
				Dim sql As String = $"SELECT * 
									FROM XFC_CMD_UFR_Priority
									WHERE WFScenario_Name = @WFScenario_Name
									AND WFCMD_Name = @WFCMD_Name
									AND WFTime_Name = @WFTime_Name
									AND Review_Entity = @Review_Entity
					 					"
				Dim paramList As New List(Of SqlParameter) From {
			    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
			    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
			    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
				New SqlParameter("@Review_Entity", SqlDbType.NVarChar) With {.Value = sFundCenter}
				}
	
				If Req_ID_List.Count > 1 Then
			        Dim paramNames As New List(Of String)()
			        For i As Integer = 0 To Req_ID_List.Count - 1
			            Dim paramName As String = "@UFR_ID" & i
			            paramNames.Add(paramName)
			            paramList.Add(New SqlParameter(paramName, SqlDbType.NVarChar) With {.Value = Req_ID_List(i).Trim()})
			        Next
			        sql &= $" AND UFR_ID IN ({String.Join(",", paramNames)})"
			    ElseIf Req_ID_List.Count = 1 Then
			        sql &= " AND UFR_ID = @UFR_ID"
			        paramList.Add(New SqlParameter("@UFR_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0).Trim()})
			    End If

				
			    ' 4. Convert the list to the array your method expects
			    Dim sqlparams As SqlParameter() = paramList.ToArray()
					
				sqaReaderPriority.Fill_XFC_CMD_UFR_Priority_DT(si, sqa3, dt_Priority, sql, sqlparams)
				
				For Each row As DataRow In dt_Priority.Rows
					Dim Totalweightedscore As Decimal = 0
					' Loop through the categories from the configuration
					For Each catAndWeight As KeyValuePair(Of String, Decimal) In priCatNameAndWeight
		                Dim categoryMemberName As String = catAndWeight.Key 
		                Dim categoryWeight As Decimal = catAndWeight.Value
						' Use the mapping to find the correct column name for this category
						If categoryToColumnMap.ContainsKey(categoryMemberName) Then
							Dim scoreColumnName As String = categoryToColumnMap(categoryMemberName) ' e.g., "Cat_1_Score"
						    Dim score As Decimal = 0D
	                        Decimal.TryParse(row(scoreColumnName).ToString(), score)
	                        If score > 0 Then
	                          Totalweightedscore  += score * (categoryWeight / 100D)
	                    	End If
						End If
		            Next
					row("Weighted_Score") =  Totalweightedscore
		        Next
				Dim dv As DataView = dt_Priority.DefaultView
				dv.Sort = "Weighted_Score DESC" 
				
				Dim rank As Integer = 0
				Dim previousScore As Decimal = Decimal.MinValue 
				Dim rowCounter As Integer = 0
	
				For Each rowView As DataRowView In dv
					rowCounter += 1
					Dim currentScore As Decimal = CDec(rowView("Weighted_Score"))
	
					If currentScore <> previousScore Then
					    rank = rowCounter
					End If
	
					' Assign the final rank to the 'Rank' column.
					rowView("Auto_Rank") = rank
					 rowView("Rank_Override") = 0
					' Update the previous score for the next row's comparison.
					previousScore = currentScore
				Next			
				sqaReaderPriority.Update_XFC_CMD_UFR_Priority(si,dt_Priority,sqa3)
			End Using	
			Return Nothing
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
				
	Private Function GetCategoryAndWeight() As Object
          
        'Get the list of catagories
        Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
        Dim WFScenario As String = "RMW_Cycle_Config_Annual"
        Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
        Dim REQTime As String = WFYear
    
        Dim sFundCenter As String =  args.NameValuePairs.XFGetValue("FundsCenter")
		
        Dim priCatMembers As List(Of MemberInfo)
        Dim priFilter As String = "A#UFR_Priority_Cat_Weight.Children"
        Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
         priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
        
        Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
        Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
        Dim priCatNameList As New List(Of String)
        Dim priCatName As String = ""
        Dim priCatWeight As Decimal
        Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
		
        For Each pricat As MemberInfo In priCatMembers
            priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
			If (Not String.IsNullOrWhiteSpace(priCatName)) Then
                priCatWeight = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catWeightMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCell.CellAmount    
                priCatNameAndWeight.Add(priCat.Member.Name, priCatWeight)
				
            End If
        Next
        
        Return priCatNameAndWeight
		priCatNameList.Add(priCatName)
          
	End Function	
		
	Private Function GetCategoryColumnMappings() As Dictionary(Of String, String)
   
		Return New Dictionary(Of String, String) From {
		    {"UFR_Priority_Cat_1_Weight", "Cat_1_Score"},
		    {"UFR_Priority_Cat_2_Weight", "Cat_2_Score"},
		    {"UFR_Priority_Cat_3_Weight", "Cat_3_Score"},
		    {"UFR_Priority_Cat_4_Weight", "Cat_4_Score"},
		    {"UFR_Priority_Cat_5_Weight", "Cat_5_Score"},
		    {"UFR_Priority_Cat_6_Weight", "Cat_6_Score"},
		    {"UFR_Priority_Cat_7_Weight", "Cat_7_Score"},
		    {"UFR_Priority_Cat_8_Weight", "Cat_8_Score"},
		    {"UFR_Priority_Cat_9_Weight", "Cat_9_Score"},
		    {"UFR_Priority_Cat_10_Weight", "Cat_10_Score"},
		    {"UFR_Priority_Cat_11_Weight", "Cat_11_Score"},
		    {"UFR_Priority_Cat_12_Weight", "Cat_12_Score"},
		    {"UFR_Priority_Cat_13_Weight", "Cat_13_Score"},
		    {"UFR_Priority_Cat_14_Weight", "Cat_14_Score"},
		    {"UFR_Priority_Cat_15_Weight", "Cat_15_Score"}
		}
	End Function
		 
			 
			 
			 
			 
			 
				
#End Region		
		
'		{Workspace.CMD_UFR.CMD_UFR_Assembly.CMD_UFR_Helper}{CopySPLNREQ}{REQ_LINK_ID=[|!BL_CMD_UFR_REQIDTitleList!|],SelectedREQID=[|!CMD_UFR_bi_SelectedREQID!|]}
		Public Function CopySPLNREQ() As xfselectionchangedTaskResult
			Try		
				Dim tStart1 As DateTime =  Date.Now()
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim wfYear = wfInfoDetails("TimeName")

'				Dim selectedREQID As String = args.NameValuePairs.XFGetValue("SelectedREQID")
				Dim REQ_Link_ID As String = args.NameValuePairs.XFGetValue("REQ_Link_ID")
				Dim sEntity As String = REQ_Link_ID.Split("_"c)(0)

				Dim UFR_ID As String = Me.Get_FC_REQ_ID(si,sEntity) 
				Dim sLevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, sEntity)
				Dim FCList As New List(Of String)
#Region "Delete"
'				Dim Dashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
'				Dim demote_comment As String = args.NameValuePairs.XFGetValue("demotecomment")
'				Dim new_Status As String = args.NameValuePairs.XFGetValue("new_Status")
'				Dim PrevStatus As String = args.NameValuePairs.XFGetValue("PrevStatus")
'				Dim req_IDs As String = args.NameValuePairs.XFGetValue("req_IDs","NA")
'				Dim Entity As String = args.NameValuePairs.XFGetValue("Entity")
'				Dim flow As String = args.NameValuePairs.XFGetValue("Flow","NA")
'				Dim Appn As String = args.NameValuePairs.XFGetValue("appn")
'				Dim Entity_APPN_Flow = args.NameValuePairs.XFGetValue("Entity_APPN_Flow","NA")
'				Dim lEntityAppnFlow As New List(Of String)
'				Dim Mode As String = args.NameValuePairs.XFGetValue("Mode")
'				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
''				Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{wfInfoDetails("CMDName")}")
'				Dim FCList As New List(Of String)
'				Dim Status_manager As New Dictionary(Of String,String)
'				Dim PackageList As New List(Of String)		
'				Dim packageFilter As String = String.Empty
'				Dim packageDetailFilter As String = String.empty
'				Dim packageAuditFilter As String = String.empty
'				Dim statusList As New List(Of String)
'				Dim APPNList As New List(Of String)
#End Region				
				
				Dim sql As String = String.empty
				Dim currDBsql As String = String.empty
				Dim Detailsql As String = String.empty
				Dim currDBDetailsql As String = String.empty

'				Dim REQ_Link_ID As String = args.NameValuePairs.XFGetValue("REQ_Link_ID")
#Region "Delete"
'					If Entity_APPN_Flow = "NA" Then 
'						Entity_APPN_Flow = $"{Entity}:{APPN}:{PrevStatus}"		
'					End If			
'					lEntityAppnFlow =  StringHelper.SplitString(Entity_APPN_Flow, ",")
#Region "Delete"					
					#Region "Promote Statuses"
'					Status_manager.Add("L5_Formulate_SPLN|Base","L5_Validate_SPLN")
'					Status_manager.Add("L4_Formulate_SPLN|Parent","L4_Validate_SPLN")
'					Status_manager.Add("L4_Formulate_SPLN|Base","L4_Validate_SPLN")
'					Status_manager.Add("L3_Formulate_SPLN|Parent","L3_Validate_SPLN")
'					Status_manager.Add("L3_Formulate_SPLN|Base","L3_Validate_SPLN")
'					Status_manager.Add("L2_Formulate_SPLN|Parent","L2_Validate_SPLN")
'					Status_manager.Add("L2_Formulate_SPLN|Base","L2_Validate_SPLN")
'					Status_manager.Add("L5_Validate_SPLN|Base","L4_Validate_SPLN")
'					Status_manager.Add("L4_Validate_SPLN|Base","L3_Validate_SPLN")
'					Status_manager.Add("L4_Validate_SPLN|Parent","L4_Approve_SPLN")
'					Status_manager.Add("L3_Validate_SPLN|Parent","L3_Approve_SPLN")
'					Status_manager.Add("L3_Validate_SPLN|Base","L3_Approve_SPLN")
'					Status_manager.Add("L2_Validate_SPLN|Parent","L2_Approve_SPLN")
'					Status_manager.Add("L2_Validate_SPLN|Base","L2_Approve_SPLN")
'					Status_manager.Add("L2_Approve_SPLN|Parent","L2_Final_SPLN")
'					Status_manager.Add("L2_Approve_SPLN|Base","L2_Final_SPLN")
'					Status_manager.Add("L3_Approve_SPLN|Parent","L2_Validate_SPLN")
'					Status_manager.Add("L3_Approve_SPLN|Base","L2_Validate_SPLN")
					#End Region
#End Region					

'					For Each EAF As String In lEntityAppnFlow
'						Dim E As String = EAF.Split(":")(0)
''						Dim A As String = EAF.Split(":")(1)
'						Dim F As String = EAF.Split(":")(2)	
'						If Not statusList.Contains(F) Then
'							statusList.Add(F)
'						End If
''						If Not APPNList.Contains(A) Then
''							APPNList.Add(A)
''						End If
'						Dim EobjDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_Army")
'						Dim U1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
'						Dim EDesc As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EobjDimPk, "E#" & E & ".descendantsinclusive", True)
''						Dim U1Base As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U1objDimPk, "U1#" & A & ".Base", True)
'						Dim newReviewEntity As String
'						For Each mementity As MemberInfo In EDesc.orderby(Function(m) m.Member.Name).ToList()
''							PackageList.Clear()
'							If mementity.Member.Name = E
'								Dim ReviewEntList = BRApi.Finance.Members.GetParents(si,entDimPk,mementity.member.MemberId,False)
'								newReviewEntity = ReviewEntList(0).Name
'							End If
#Region "Delete"							
'							PackageList.Add($"'{mementity.member.name}|{A}|{F}'")	
'							For Each memU1 As MemberInfo In U1Base
'								PackageList.Add($"'{mementity.member.name}|{memU1.member.name}|{F}'")
'							Next
'						packageFilter = "AND CONCAT(Entity,'|',APPN,'|',Status) IN (" & String.Join(",", PackageList) & ")"
'						packageDetailFilter = "AND CONCAT(Entity,'|',UD1,'|',Flow) IN (" & String.Join(",", PackageList) & ")"
#End Region
'							Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,mementity.member.MemberId)
'							Dim entBaseParent As String

'							If globals.GetStringValue($"{mementity.member.name}_Base","NA") = "NA"
'								Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, mementity.Member.MemberId)
'								If Ent_Base = True
'									entBaseParent = "Base" 
'								Else
'									entBaseParent = "Parent"
'								End If
'								globals.SetStringValue($"{mementity.member.name}_Base",entBaseParent)
'							Else
'								entBaseParent = globals.GetStringValue($"{mementity.member.name}_Base","NA")
'							End If
#Region "Delete"	
'							If Not String.IsNullOrEmpty(new_status) Then 
'								If Not statusList.Contains(new_status) Then
'									statusList.Add(new_status)
'								End If
'							Else If	Status_manager.ContainsKey($"{F}|{entBaseParent}") Then
'								new_status = Status_manager.XFGetValue($"{F}|{entBaseParent}")
'								If Not statusList.Contains(new_status) Then
'									statusList.Add(new_status)
'								End If
'							End If
#End Region
							Dim reviewEntityString As String = "Review_Entity"
#Region "Delete"
'							If F.Substring(0,2) <> new_status.Substring(0,2)
'								If Not demote_comment = "" Then 
									
'								Else
'									reviewEntityString = $"'{newReviewEntity}' as Review_Entity"
'								End If
'								Brapi.ErrorLog.LogMessage(si, "reviewEntityString = "	& reviewEntityString)
'							End If --{reviewEntityString}
#End Region			

#End Region
'								Target header sql
								currDBsql = $"SELECT
												CMD_UFR_Tracking_No, 
												WFScenario_Name, 
												WFCMD_Name, 
												WFTime_Name,
											    Title, 
												UFR_ID,
												REQ_Link_ID,
												Description, 
												Entity, 
												Review_Entity,
												APPN, 
												MDEP, 
												APE9,
											    Dollar_Type, 
												Obj_Class, 
												CType,
												Command_UFR_Status,
												MustFund,
											    Create_Date, Create_User,
											    Update_Date, Update_User 
										FROM dbo.XFC_CMD_UFR
										WHERE WFScenario_Name = @WFScenario_Name
										AND WFCMD_Name = @WFCMD_Name
										AND WFTime_Name = @WFTime_Name
										AND REQ_Link_ID = '{REQ_Link_ID}'"

								'Source header sql
								sql = $"SELECT 
												CMD_SPLN_REQ_ID As 'CMD_UFR_Tracking_No', 
												'CMD_SPLN_WORK' AS WFScenario_Name,
												WFCMD_Name, 
												WFTime_Name, 
											    Title, 
												'{UFR_ID}' As 'UFR_ID',
												REQ_ID As 'REQ_Link_ID',
												Description,		
												Entity, 
												Review_Entity,
												APPN, 
												MDEP, 
												APE9,
											    Dollar_Type, 
												Obj_Class, 
												CType,
												'{sLevel}_Formulate_UFR' As Command_UFR_Status,
												Must_Fund As 'MustFund', 
												Create_Date, Create_User,
											    Update_Date, Update_User 
											  FROM dbo.XFC_CMD_SPLN_REQ
											  WHERE WFScenario_Name = 'CMD_SPLN_C{wfYear}'
											  AND WFCMD_Name = @WFCMD_Name
											  AND WFTime_Name = @WFTime_Name
											  AND REQ_ID = '{REQ_LINK_ID}'"
												
								Brapi.ErrorLog.LogMessage(si, $"REQ_LINK_ID = {REQ_LINK_ID}")
								'Target detail sql
								currDBDetailSQL = $"SELECT
												Dtl.CMD_UFR_Tracking_No,
												Dtl.WFScenario_Name,
												Dtl.WFCMD_Name,
												Dtl.WFTime_Name,
												Dtl.Unit_of_Measure,
												Dtl.Entity,
												Dtl.IC,
												Dtl.Account,
												Dtl.Flow,
												Dtl.UD1,
												Dtl.UD2,
												Dtl.UD3,
												Dtl.UD4,
												Dtl.UD5,
												Dtl.UD6,
												Dtl.UD7,
												Dtl.UD8,
												Dtl.AllowUpdate,
												Dtl.Create_Date,
												Dtl.Create_User,
												Dtl.Update_Date,
												Dtl.Update_User
											FROM dbo.XFC_CMD_UFR_Details AS Dtl
											LEFT JOIN dbo.XFC_CMD_UFR AS Req
											ON Dtl.CMD_UFR_Tracking_No = Req.CMD_UFR_Tracking_No
											WHERE Dtl.WFScenario_Name = @WFScenario_Name
											AND Dtl.WFCMD_Name = @WFCMD_Name
											AND Dtl.WFTime_Name = @WFTime_Name
											AND Req.REQ_Link_ID = '{REQ_LINK_ID}'"
								
								'Source detail sql
								Detailsql = $"SELECT 
												Dtl.CMD_SPLN_REQ_ID As 'CMD_UFR_Tracking_No',
												CAST('CMD_SPLN_WORK' AS NVARCHAR(100)) AS [WFScenario_Name],
												Dtl.WFCMD_Name,
												Dtl.WFTime_Name,
												Dtl.Unit_of_Measure,
												Dtl.Entity,
												Dtl.IC,
												CAST('Req_Funding' AS NVARCHAR(100)) AS [Account],
												CAST('{sLevel}_Formulate_UFR' AS NVARCHAR(100)) AS [Flow],
												Dtl.UD1,
												Dtl.UD2,
												Dtl.UD3,
												Dtl.UD4,
												Dtl.UD5,
												Dtl.UD6,
												Dtl.UD7,
												Dtl.UD8,   
												Dtl.AllowUpdate,
												Dtl.Create_Date,
												Dtl.Create_User,
												Dtl.Update_Date,
												Dtl.Update_User
											FROM dbo.XFC_CMD_SPLN_REQ_Details AS Dtl
											LEFT JOIN dbo.XFC_CMD_SPLN_REQ AS Req
											ON Dtl.CMD_SPLN_REQ_ID = Req.CMD_SPLN_REQ_ID
											WHERE Dtl.WFScenario_Name = 'CMD_SPLN_C{wfYear}' 
											AND Dtl.WFTime_Name = @WFTime_Name
											AND Req.REQ_ID = '{REQ_LINK_ID}'
											AND Dtl.Account = 'UFR_Obligations'
											AND Dtl.Fiscal_Year = '{wfYear}'"
'						Next
'					Next

				Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()

					Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
					Using connection As New SqlConnection(dbConnApp.ConnectionString)
						connection.Open()
						Dim sqa_xfc_cmd_ufr = New SQA_XFC_CMD_UFR(connection)
						Dim SQA_XFC_CMD_UFR_DT = New DataTable()
						Dim src_XFC_CMD_UFR_DT = New DataTable()
						
						Dim sqa_xfc_cmd_ufr_details = New SQA_XFC_CMD_UFR_DETAILS(connection)
						Dim SQA_XFC_CMD_UFR_DETAILS_DT = New DataTable()
						Dim src_XFC_CMD_UFR_DETAILS_DT = New DataTable()
						
						Dim sqa_xfc_cmd_ufr_details_audit = New SQA_XFC_CMD_UFR_DETAILS_AUDIT(connection)
						Dim SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT = New DataTable()
						
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
						sqa_xfc_cmd_ufr.Fill_XFC_CMD_UFR_DT(sqa, SQA_XFC_CMD_UFR_DT, currDBsql, sqlparams)
						'details
						sqa_xfc_cmd_ufr_Details.Fill_XFC_CMD_UFR_DETAILS_DT(sqa, SQA_XFC_CMD_UFR_DETAILS_DT, currDBDetailSQL, sqlparams)
						SQA_XFC_CMD_UFR_DT.AcceptChanges()
						SQA_XFC_CMD_UFR_DETAILS_DT.AcceptChanges()
						sqa.AcceptChangesDuringFill = False
						sqa_xfc_cmd_ufr.Fill_XFC_CMD_UFR_DT(sqa, src_XFC_CMD_UFR_DT, sql, sqlparams)
						sqa_xfc_cmd_ufr_Details.Fill_XFC_CMD_UFR_DETAILS_DT(sqa, src_XFC_CMD_UFR_DETAILS_DT, Detailsql, sqlparams)
BRApi.ErrorLog.LogMessage(si, "Line 1772")
						fclist = src_XFC_CMD_UFR_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("Entity")).Distinct().ToList()
						
						'merge and sync
						sqa_xfc_cmd_ufr.MergeandSync_XFC_CMD_UFR_toDB(si,src_XFC_CMD_UFR_DT,SQA_XFC_CMD_UFR_DT)
BRApi.ErrorLog.LogMessage(si, "Line 1785")
						sqa_xfc_cmd_ufr_Details.MergeandSync_XFC_CMD_UFR_Details_toDB(si,src_XFC_CMD_UFR_DETAILS_DT,SQA_XFC_CMD_UFR_DETAILS_DT)

BRApi.ErrorLog.LogMessage(si, "Line 1788")
					End Using
		
				'End If	
'brapi.ErrorLog.LogMessage(si,$"DB Processes Complete {String.Join(",",FCList)}")
'				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_SPLN")
'				Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
'				For Each JoinedEntity As String In joinedentitylist
'					Dim GlobalAPPNs As String = String.Join("|",APPNList)
'					Dim GlobalFlows As String = String.Join("|",StatusList)
'					globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", GlobalFlows)	
'					Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",GlobalAPPNs)
'				Next
'				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
'				Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))			
'				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
'				Dim customSubstVars As New Dictionary(Of String, String) 
'				customSubstVars.Add("EntList",String.Join(",",BaseEntityList))
'				customSubstVars.Add("ParentEntList",String.Join(",",ParentEntityList))
'				customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
'				Dim currentYear As String = wfInfoDetails("TimeName")
'				Dim nextyear As String = currentYear + 1
'				customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")

'				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)				
				
				Return dbExt_ChangedResult
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
	End Class
End Namespace