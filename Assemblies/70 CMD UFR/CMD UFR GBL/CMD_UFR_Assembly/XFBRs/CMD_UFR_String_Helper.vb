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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_UFR_String_Helper
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
'					Case "getsortedappnlist"
'						Return Me.GetSortedAppnList()
'					Case "displaydashboard"
'						Return Me.displaydashboard()
'					Case "createlbltext"
'						Return Me.createlbltext()
					Case "showhide"
						Return Me.ShowHide()
'					Case "showhidemulti"
'						Return Me.ShowHideMulti()
'					Case "showhidelabel"
'						Return Me.ShowHideLabel()
'					Case "showhidelabelmulti"
'						Return Me.ShowHideLabelMulti()	
'					Case "procctrllblmsg"
'						Return Me.ProcCtrlLblMsg()
'					Case "procctrllblmsgmulti"
'						Return Me.ProcCtrlLblMsgMulti()	
					Case "reqretrievecache"
						Return Me.REQRetrieveCache()
					Case "showhidenarrative"
						Return Me.ShowHideNarrative()
					Case "dynnarrativedashboard"
						Return Me.DynNarrartiveDashboard()
					Case "dynsharedheaderdashboard"
						Return Me.DynSharedHeaderDashboard()
					Case "dynsharedheaderdashboardformulate"
						Return Me.DynSharedHeaderDashboardFormulate()
					Case "dynsessionretrieve"
						Return Me.DynSessionRetrieve()
						
						
				End Select
								
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
#Region "Constants"
Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass
Public GBL_DataSet As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardDataSet.GBL_DB_DataSet.MainClass()
#End Region	

#Region "ShowHide"
'Purpose: Any component, cv, object etc., add a case statment 
'brapi.ErrorLog.LogMessage(si, $"sComponentName {sComponentName}")		
		Public Function ShowHide() As String	
			Dim sEntity = args.NameValuePairs.XFGetValue("entity")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sComponentName As String = args.NameValuePairs.XFGetValue("ComponentName")
			'Manpower Specific
			Dim sREQ_ID As String = args.NameValuePairs.XFGetValue("UFR_ID")
			#Region "Workflow complete and revert buttons"		
			Select Case sComponentName				
				Case "WorkflowAccess"
					If wfProfileFullName.XFContainsIgnoreCase("Manage CMD UFR") Then 
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
			Dim sProcCtrlVal As String = CMD_UFR_Utilities.GetProcCtrlVal(si,sEntity)
			
			If sProcCtrlVal.XFContainsIgnoreCase("no") Or sWFStatus.XFEqualsIgnoreCase("locked") Then
				Return "False"
			End If
			
			Select Case sComponentName
				Case Else				
					Return "True"
			End Select		
		End Function
#End Region

#Region "Function: REQRetrieveCache"
		Public Function REQRetrieveCache()
			Try
				Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim sREQToDelete As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")	
				Dim cachedREQ As String = sREQToDelete
				Return cachedREQ
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region 

#Region "Function: DynSessionRetrieve"
		'XFBR(Workspace.Current.CMD_UFR_Assembly.CMD_UFR_String_Helper, DynSessionRetrieve, RetrievalType=[])
		Public Function DynSessionRetrieve()
			Try
				Dim sRetrievalType As String = args.NameValuePairs.XFGetValue("RetrievalType")
				Dim result As String = String.Empty
				
				Dim sFormulateDashboard As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"GenericRetrieve","FormulateDashboard","")
				Dim sUFRList As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
				
'				BRAPI.ErrorLog.LogMessage(si, "Dynamic Session Get REQ ID = " & sUFRList)
				
				Select Case sRetrievalType
				Case "FormulateDashboard"
					result = sFormulateDashboard
					
				Case "UFRSelection"
'				BRAPI.ErrorLog.LogMessage(si, "Dynamic Session Get REQ ID = " & sUFRList)
					result = sUFRList
				End Select
				
				Return result
				Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))

			End Try

		End Function
		
#End Region 

#Region "Function: ShowHideNarrative - Tabs/Buttons"
		'XFBR(Workspace.Current.CMD_UFR_Assembly.CMD_UFR_String_Helper, ShowHideNarrative, WFLevel=[])
		Public Function ShowHideNarrative() As String	
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim WorkflowLevel As String = args.NameValuePairs("WFLevel")
			Dim showButton As String = "IsVisible = True"
			Dim hideButton As String = "IsVisible = False"
			Dim result As String
			
			If wfProfileFullName.XFContainsIgnoreCase("CMD") Then
				If WorkflowLevel = "CMD" Then
					result = showButton
				Else
					result = hideButton
				End If
			Else If wfProfileFullName.XFContainsIgnoreCase("HQ") Then
				If WorkflowLevel = "HQ" Then
					result = showButton
				Else
					result = hideButton
				End If
			End If
							
			Return result
			
			'Grab workflow status (locked or unlocked)
'			Dim deArgs As New DashboardExtenderArgs
'			deArgs.FunctionName = "Check_WF_Complete_Lock"
'			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			
		End Function
#End Region

#Region "Function: DynNarrativeDashboard - Dynamic Narrative Dashboard"
		'XFBR(Workspace.Current.CMD_UFR_Assembly.CMD_UFR_String_Helper, DynNarrativeDashboard)
		Public Function DynNarrartiveDashboard() As String	
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim result As String = String.Empty
			
			If wfProfileFullName.XFContainsIgnoreCase("Formulate UFR")
				result = "CMD_UFR_1c_Body_UFRDynNoStaffing"
			Else
				result = "CMD_UFR_1c_Body_UFRDynStaffing"
			End If
			
			Return result
			
			'Grab workflow status (locked or unlocked)
'			Dim deArgs As New DashboardExtenderArgs
'			deArgs.FunctionName = "Check_WF_Complete_Lock"
'			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			
		End Function
#End Region

#Region "Function: DynSharedHeaderDashboard - Dynamic Formulate Dashboard Header"
		'XFBR(Workspace.Current.CMD_UFR_Assembly.CMD_UFR_String_Helper, DynSharedHeaderDashboard)
		Public Function DynSharedHeaderDashboard() As String	
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim result As String = String.Empty
			
			If wfProfileFullName.XFContainsIgnoreCase("Formulate")
				result = "CMD_UFR_2a_Header_Dyn_Formulate"
			Else
				result = "CMD_UFR_2b_Header_Dyn_Other"
			End If
			
			Return result
			
			'Grab workflow status (locked or unlocked)
'			Dim deArgs As New DashboardExtenderArgs
'			deArgs.FunctionName = "Check_WF_Complete_Lock"
'			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			
		End Function
#End Region

#Region "Function: DynSharedHeaderDashboardFormulate - Dynamic Formulate Dashboard Header Formulate"
		'XFBR(Workspace.Current.CMD_UFR_Assembly.CMD_UFR_String_Helper, DynSharedHeaderDashboardFormulate, Dashboard = [])
		Public Function DynSharedHeaderDashboardFormulate() As String	
			Dim result As String = String.Empty
			Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
			
			If sDashboard = "CMD_UFR_2a1_Header_Dyn_Formulate"
				result = "CMD_UFR_2a1_Header_Dyn_Formulate"
			Else
				result = "CMD_UFR_2a2_Header_Dyn_Formulate"
			End If
			
			Return result
			
			'Grab workflow status (locked or unlocked)
'			Dim deArgs As New DashboardExtenderArgs
'			deArgs.FunctionName = "Check_WF_Complete_Lock"
'			Dim sWFStatus As String = GBL_Helper.Main(si, globals, api, deArgs)
			
			
		End Function
#End Region


	End Class
End Namespace