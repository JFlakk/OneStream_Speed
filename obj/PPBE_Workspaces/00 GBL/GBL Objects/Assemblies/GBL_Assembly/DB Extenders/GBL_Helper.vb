Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports OneStreamWorkspacesApi.V820

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.GBL_Helper
	Public Class MainClass
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As Object
		Public args As DashboardExtenderArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("Load_GBL_DB") Then
							
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							
							If (args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters) Or
							   (args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.ComponentSelectionChanged And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeSubsequentGetParameters) Then
								loadDashboardTaskResult = Me.Load_GBL_DB()
							End If
							
							Return loadDashboardTaskResult
						End If		
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
#Region "	Load_SPLN_DB()"	
		Public Function Load_GBL_DB() As XFLoadDashboardTaskResult
			Try
				Dim LoadDBTaskResult As New XFLoadDashboardTaskResult()
				LoadDBTaskResult.ChangeCustomSubstVarsInDashboard = True
				Dim Load_SPLN_DB_SubstVars As Dictionary(Of String,String) = Me.args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun()
				Dim WFText1 As String = args.NameValuePairs.XFGetValue("WFText1","")
				Dim SelectedDB As String = args.NameValuePairs.XFGetValue("SelectedDB","")
				If SelectedDB.Contains("|!") Then SelectedDB = ""
				Dim Dynamic_Content As String = Load_SPLN_DB_SubstVars.XFGetValue("DL_CMDHQ_SPLN_MenuOptions",String.Empty)
'BRApi.ErrorLog.LogMessage(si,$"WFText1 = {WFText1} || SelectedDB = {SelectedDB} || Dynamic_Content = {Dynamic_Content}")				
				
				'If dropdown has a previously selected value evaluate according to the list stored in the selected wf step and the currently selected value in dropdown
				'If there was previously selected value but no currently selected value
				If Dynamic_Content <> String.Empty And String.IsNullOrEmpty(SelectedDB)						
					'if previously selected value exists in WFStep's list - use the previously selected value (as user is returning to the same step that they left off at or just refreshed)
					If WFText1 <> String.Empty And WFText1.XFContainsIgnoreCase(Dynamic_Content)
						If Dynamic_Content.Substring(0,3) = "DB_"					
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicContent",Dynamic_Content.Substring(3,Dynamic_Content.Length-3))
						Else
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicContent","0b2b1_CMDHQ_SPLN_Dynamic_CV")
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicCV",Dynamic_Content.Substring(3,Dynamic_Content.Length-3))
						End If
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicMenu",Dynamic_Content.Substring(3,Dynamic_Content.Length-3) & "_Menu")
						UpdateCustomSubstVar(LoadDBTaskResult,"DL_CMDHQ_SPLN_MenuOptions",Dynamic_Content)	
					'if not - the user has selected a different wf step and therefore default to the first db
					ElseIf WFText1 <> String.Empty And Not WFText1.XFContainsIgnoreCase(Dynamic_Content) 
						WFText1 = WFText1.Split(",")(0)
						If WFText1.Substring(0,3) = "DB_"					
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicContent",WFText1.Substring(3,WFText1.Length-3))				
						End If
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicMenu",WFText1.Substring(3,WFText1.Length-3) & "_Menu")
						UpdateCustomSubstVar(LoadDBTaskResult,"DL_CMDHQ_SPLN_MenuOptions",WFText1)	
					End If
				'If there is currently selected value and is part of WFStep's list, use it	
				ElseIf Not String.IsNullOrEmpty(SelectedDB) And WFText1.XFContainsIgnoreCase(SelectedDB)
					
					If SelectedDB.Substring(0,3) = "DB_"					
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicContent",SelectedDB.Substring(3,SelectedDB.Length-3))				
					End If
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicMenu",SelectedDB.Substring(3,SelectedDB.Length-3) & "_Menu")
						UpdateCustomSubstVar(LoadDBTaskResult,"DL_CMDHQ_SPLN_MenuOptions",SelectedDB)
			
				'if dropdown does not have a selected value (due to first time logging in or new session) then default to wfstep's first db	
				Else					
					If WFText1 <> String.Empty Then
						WFText1 = WFText1.Split(",")(0)
						If WFText1.Substring(0,3) = "DB_"					
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicContent",WFText1.Substring(3,WFText1.Length-3))				
						End If
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_CMDHQ_SPLN_DynamicMenu",WFText1.Substring(3,WFText1.Length-3) & "_Menu")
						UpdateCustomSubstVar(LoadDBTaskResult,"DL_CMDHQ_SPLN_MenuOptions",WFText1)	
					End If
				End If
				Return LoadDBTaskResult
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

End Class
End Namespace
