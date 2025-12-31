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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.GBL_Dashboard_Helper
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
							If (args.PrimaryDashboard.Name <> "GBL_Rpt_0_Main" And args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And (args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters or args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeSubsequentGetParameters)) Or
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
		
		
#Region "Load_GBL_DB()"	
		Public Function Load_GBL_DB() As XFLoadDashboardTaskResult
			Try
				Dim LoadDBTaskResult As New XFLoadDashboardTaskResult()
				LoadDBTaskResult.ChangeCustomSubstVarsInDashboard = True
		
				Dim Load_SPLN_DB_SubstVars As Dictionary(Of String, String) = Me.args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved() 'CustomSubstVarsFromPriorRun()

				' Loop through each key-value pair in the dictionary
				For Each kvp As KeyValuePair(Of String, String) In args.NameValuePairs
				    ' Access the Key and Value for each pair
				    Dim key As String = kvp.Key
				    Dim value As String = kvp.Value
				
				   ' brapi.ErrorLog.LogMessage(si,$"Here 1.5 {kvp.Key} - {kvp.Value}")
				Next

				Dim WFText2 As String = args.NameValuePairs.XFGetValue("WFText2","")
				Dim WFText3 As String = args.NameValuePairs.XFGetValue("WFText3","")
				Dim WFText4 As String = args.NameValuePairs.XFGetValue("WFText4","")
				Dim SelectedDB As String = args.NameValuePairs.XFGetValue("SelectedDB","")
				Dim DynMenuParam As String = args.NameValuePairs.XFGetValue("DynMenuParam","")
				If SelectedDB.Contains("|!") Then SelectedDB = ""
				Dim Dynamic_Content As String = args.NameValuePairs.XFGetValue(DynMenuParam,"")
				
				'brapi.ErrorLog.LogMessage(si,$"Here 1.5 {DynMenuParam} - {Dynamic_Content} - {selectedDB} - {WFText2} - {WFText3}")

				
				'If dropdown has a previously selected value evaluate according to the list stored in the selected wf step and the currently selected value in dropdown
				'If there was previously selected value but no currently selected value
				If Dynamic_Content <> String.Empty And String.IsNullOrEmpty(SelectedDB)						
					'if previously selected value exists in WFStep's list - use the previously selected value (as user is returning to the same step that they left off at or just refreshed)
					If WFText3 <> String.Empty And WFText3.XFContainsIgnoreCase(Dynamic_Content)
						If Dynamic_Content.Substring(0,3) = "DB_"
'BRApi.ErrorLog.LogMessage(si,$"Here1 - {Dynamic_Content}")	
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent",Dynamic_Content.Substring(3,Dynamic_Content.Length-3))
						Else
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent","GBL_0c1a_Dynamic_CV")
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicCV",Dynamic_Content.Substring(3,Dynamic_Content.Length-3))
						End If
												
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicHeader",GetDynamicHeader(Dynamic_Content,WFText2,WFText3))
						UpdateCustomSubstVar(LoadDBTaskResult,DynMenuParam,Dynamic_Content)	

					'if not - the user has selected a different wf step and therefore default to the first db
					ElseIf WFText3 <> String.Empty And Not WFText3.XFContainsIgnoreCase(Dynamic_Content) 
						If WFText3.XFContainsIgnoreCase(",") Then
							WFText3 = WFText3.Split(",")(0)
						End If
						'brapi.ErrorLog.LogMessage(si,$"Here 1.5 {WFText3}")
						If WFText3.Substring(0,3) = "DB_"					
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent",WFText3.Substring(3,WFText3.Length-3))	
						Else
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent","GBL_0c1a_Dynamic_CV")
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicCV",WFText3.Substring(3,WFText3.Length-3))
						End If
						If WFText2.XFContainsIgnoreCase(",") Then
							WFText2 = WFText2.Split(",")(0)
						End If
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicHeader",WFText2)
						UpdateCustomSubstVar(LoadDBTaskResult,DynMenuParam,WFText3)	
					End If
				'If there is currently selected value and is part of WFStep's list, use it	
				ElseIf Not String.IsNullOrEmpty(SelectedDB) And WFText3.XFContainsIgnoreCase(SelectedDB)
					
					If SelectedDB.Substring(0,3) = "DB_"					
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent",SelectedDB.Substring(3,SelectedDB.Length-3))	
					Else
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent","GBL_0c1a_Dynamic_CV")
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicCV",SelectedDB.Substring(3,SelectedDB.Length-3))
					End If
					
					UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicHeader",GetDynamicHeader(SelectedDB,WFText2,WFText3))
					UpdateCustomSubstVar(LoadDBTaskResult,DynMenuParam,SelectedDB)
			
				'if dropdown does not have a selected value (due to first time logging in or new session) then default to wfstep's first db	
				Else		
					'BRApi.ErrorLog.LogMessage(si,$"Here1 - {WFText3}")	
					If WFText3 <> String.Empty Then	
						'BRApi.ErrorLog.LogMessage(si,$"Here1 - {WFText3}")	
						If WFText3.XFContainsIgnoreCase(",") Then
							WFText3 = WFText3.Split(",")(0)
						End If
						'BRApi.ErrorLog.LogMessage(si,$"Here1 - {selectedDB} - {DynMenuParam}")	
						If WFText3.XFContainsIgnoreCase("|!")
							'Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, args.PrimaryDashboard.WorkspaceID)
							Dim DBParamInfo As DashboardParamDisplayInfo = BRApi.Dashboards.Parameters.GetParameterDisplayInfo(si, False,args.LoadDashboardTaskInfo.CustomSubstVarsAlreadyResolved, args.PrimaryDashboard.WorkspaceID, DynMenuParam)
							'args.PrimaryDashboard.
							WFText3 = DBParamInfo.Parameter.ValueItems
							'BRApi.ErrorLog.LogMessage(si,$"Default - {DBParamInfo.Parameter.ValueItems}")	
						End If	
						If WFText3.Substring(0,3) = "DB_"					
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent",WFText3.Substring(3,WFText3.Length-3))	
						Else
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicContent","GBL_0c1a_Dynamic_CV")
							UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicCV",WFText3.Substring(3,WFText3.Length-3))
						End If
						If WFText2.XFContainsIgnoreCase(",") Then
							WFText2 = WFText2.Split(",")(0)
						End If
						'BRApi.ErrorLog.LogMessage(si,$"Here1 - {WFText2}")
						UpdateCustomSubstVar(LoadDBTaskResult,"IV_GBL_DynamicHeader",WFText2)
						'LoadDBTaskResult.ModifiedCustomSubstVars.XFGetValue("IV_GBL_DynamicHeader","NA")
						'BRApi.ErrorLog.LogMessage(si,$"Here1 - {LoadDBTaskResult.ModifiedCustomSubstVars.XFGetValue("IV_GBL_DynamicHeader","NA")}")
						UpdateCustomSubstVar(LoadDBTaskResult,DynMenuParam,WFText3)	
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
'BRApi.ErrorLog.LogMessage(si,$"{key} | {value}")				
			If Result.ModifiedCustomSubstVars.ContainsKey(key)
				Result.ModifiedCustomSubstVars.XFSetValue(key,value)
			Else
				Result.ModifiedCustomSubstVars.Add(key,value)
			End If
			
		End Sub
#End Region		

#Region "GetDynamic DB Items"
Private Function GetDynamicHeader(Dynamic_Content As String,WFText2 As String,WFText3 As String) As String
	Dim WFText2List As List( Of String ) = WFText2.Split(",").ToList()
	Dim WFText3List As List( Of String ) = WFText3.Split(",").ToList()
	'BRApi.ErrorLog.LogMessage(si,$"Hit {Dynamic_Content}")
	If WFText2List.Count > 1
		Dim contentPos As Integer = WFText3List.FindIndex(Function(x) x.XFEqualsIgnoreCase(Dynamic_Content))
		'BRApi.ErrorLog.LogMessage(si,$"Hit {Dynamic_Content} - {WFText3List.Count} - {WFText3}")
		Return WFText2List(contentPos)
	Else
		Return WFText2
	End If

End Function

Private Function GetDynamicHelp(Dynamic_Content As String,WFText3 As String,WFText4 As String) As String
	Dim WFText4List As List( Of String ) = WFText4.Split(",").ToList()
	Dim WFText3List As List( Of String ) = WFText3.Split(",").ToList()
	If WFText4List.Count > 1
		Dim contentPos As Integer = WFText3List.FindIndex(Function(x) x.XFEqualsIgnoreCase(Dynamic_Content))
		Return WFText4List(contentPos)
	Else
		Return WFText4
	End If

End Function
	
#End Region		

End Class
End Namespace