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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_PGM_Utilities
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		
		Public Shared Function	GetUD3(ByVal si As SessionInfo, UD1 As String, UD3 As String) As Object
			'====== Get APPN_FUND And PARENT APPN_L2 ======
'Brapi.ErrorLog.LogMessage(si, "UD1: " & UD1 & ", UD3: " & UD3)			
			Dim U1Name As String = UD1				
			Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_FundCode")
'Brapi.ErrorLog.LogMessage(si, "U1DimPk: " & U1DimPk.ToString)			
			Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
'BRApi.ErrorLog.LogMessage(si, "U1MemberID: " & U1MemberID.ToString)				
			Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 	
'BRApi.ErrorLog.LogMessage(si, "U1ParentName: " & U1ParentName)				
			Dim U3Concat As String = U1ParentName & "_" & UD3
'BRApi.ErrorLog.LogMessage(si, "U3Concat: " & U3Concat)					
			Return U3Concat
		End Function
		
		Public Shared Function GetCopiedRow(ByVal si As SessionInfo, ByRef originalRow As DataRow) As Object
			' Create a new row with the same schema
			Dim newRow As DataRow = originalRow.Table.NewRow()
			newRow.ItemArray = originalRow.ItemArray
			Return newRow
		End Function
			
		Public Shared Sub UpdateAuditColumns(ByVal si As SessionInfo, ByRef newRow As DataRow) 
			newRow("Create_Date") = DateTime.Now
			newRow("Create_User") = si.UserName
			newRow("Update_Date") = DateTime.Now
			newRow("Update_User") = si.UserName
		End Sub	

		Public Shared Function GetInflationRate(ByVal si As SessionInfo, ByVal sCube As String, ByVal sEntity As String, ByVal sScenario As String,ByVal sREQTime As String, ByVal sAPPN As String) As Object 
			Dim sInfRateScript As String = "Cb#" & sCube & ":S#" & sScenario & ":T#" & sREQTime & ":C#USD:E#"& sCube & "_General:V#Periodic:A#PGM_Inflation_Rate_Amt:O#BeforeAdj:I#None:F#None" 
			sInfRateScript &= ":U1#" & sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
			Dim dInflationRate As Decimal = 0
			dInflationRate = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sInfRateScript).DataCellEx.DataCell.CellAmount
			
			Return dInflationRate
		End Function
		
		Public Shared Function GetProcCtrlVal(ByVal si As SessionInfo, ByVal sEntity As String) As Object 
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)

			Dim WF_ProcCtrl_Account As New Dictionary(Of String, String) From {
			{"Formulate","CMD_PGM_Allow_Req_Creation"},
			{"Validate","CMD_PGM_Allow_Req_Validation"},
			{"Prioritize","CMD_PGM_Allow_Req_Priority"},
			{"Rollover","CMD_PGM_Allow_Req_Rollover"}
			}
			If WF_ProcCtrl_Account.ContainsKey(wfProfileName) Then
		    	Dim sAllowAccount As String = WF_ProcCtrl_Account(wfProfileName)
				Dim sAllowAccountMbrScript As String = $"Cb#{sCube}:E#{sEntity}:C#Local:S#RMW_Cycle_Config_Annual:T#{sREQTime}:V#Annotation:A#{sAllowAccount}:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Dim sAllowAccountValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAllowAccountMbrScript).DataCellEx.DataCellAnnotation			
'brapi.ErrorLog.Logmessage(si, $"sAllowAccountMbrScript: {sAllowAccountMbrScript} | sAllowAccountValue: {sAllowAccountValue}")	
				If sAllowAccountValue.XFContainsIgnoreCase("no") Then			
					Return "no"
				Else
					Return "yes"
				End If

			'Review Step will be blank --> do not want to show functional components on the review steps i.e., formulate requirement component
			Else If String.IsNullOrEmpty(wfProfileName)
				Return "no"
			End If	
		End Function
		
		Public Shared Function GetProcCtrlValMulti(ByVal si As SessionInfo) As Object 
			
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfProfileName As String = If(wfProfileFullName.Contains("."), wfProfileFullName.Substring(wfProfileFullName.IndexOf(".") + 1).Trim().Split(" "c)(0), String.Empty)
			Dim counter As Integer = 0
			Dim WF_ProcCtrl_Account As New Dictionary(Of String, String) From {
			{"Formulate","CMD_PGM_Allow_Req_Creation"},
			{"Validate","CMD_PGM_Allow_Req_Validation"},
			{"Import","CMD_PGM_Allow_Req_Creation"}
			}
			If WF_ProcCtrl_Account.ContainsKey(wfProfileName) Then		

				Dim userSecGroup As DataSet = brapi.Dashboards.Process.GetAdoDataSetForAdapter(si,False,"da_CMD_PGM_GetUserSec","FundsCenterByWF",Nothing)	
				Dim userSecGroup_dt As DataTable = userSecGroup.Tables(0)
	'brapi.ErrorLog.LogMessage(si, userSecGroup_dt.Rows.Count)
				'If userSecGroup_dt.Rows.Count > 1 Then
					
					For Each row As DataRow In userSecGroup_dt.Rows
						Dim sUserSecEntity = row("Value").ToString
						Dim sAllowAccount As String = WF_ProcCtrl_Account(wfProfileName)
					  	Dim sAllowAccountMbrScript As String = $"Cb#{sCube}:E#{sUserSecEntity}:C#Local:S#RMW_Cycle_Config_Annual:T#{sREQTime}:V#Annotation:A#{sAllowAccount}:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	 					Dim sAllowAccountValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sAllowAccountMbrScript).DataCellEx.DataCellAnnotation			
'brapi.ErrorLog.LogMessage(si,  $"|| {sAllowAccountMbrScript}")								
						If sAllowAccountValue.XFContainsIgnoreCase("no") Then			
							counter += 1
						End If
'brapi.ErrorLog.LogMessage(si, userSecGroup_dt.Rows.Count & $"|| {counter}")							
					Next	
					If counter = userSecGroup_dt.Rows.Count Then
						Return "no"
					Else 
						Return "yes"
					End If
				'End If
			End If	
		End Function

	End Class

End Namespace