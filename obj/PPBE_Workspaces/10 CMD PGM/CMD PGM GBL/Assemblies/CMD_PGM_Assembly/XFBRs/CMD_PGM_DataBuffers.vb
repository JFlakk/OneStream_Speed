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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_PGM_DataBuffers
	Public Class MainClass
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("Org_Totals_by_SAG") Then				
					Return Me.Org_Totals_by_SAG(si,globals,api,args)
				End If				
				If args.FunctionName.XFEqualsIgnoreCase("MDEP_Summary") Then				
					Return Me.MDEP_Summary(si,globals,api,args)
				End If				
				If args.FunctionName.XFEqualsIgnoreCase("Cert_Summary_Report") Then				
					Return Me.Cert_Summary_Report(si,globals,api,args)
				End if
				If args.FunctionName.XFEqualsIgnoreCase("EntityText1") Then				
					Return Me.EntityText1(si,globals,api,args)
				End If	
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Constants"
Public GBL_Helper As New Workspace.GBL.GBL_Assembly.BusinessRule.DashboardExtender.GBL_Helper.MainClass	
#End Region	
		
#Region "Return ORG Totals By SAG"
	Public Function Org_Totals_by_SAG(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
	Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
	Dim Entity As String = args.NameValuePairs("Entity")
	Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
	'BRAPI.ErrorLog.LogMessage(si, $"Hit {Entity} - {lEntity.Count}")
	Dim Scenario As String = args.NameValuePairs("Scenario")
	'Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
	Dim Time As String = args.NameValuePairs("Time")
	Dim lTime As List(Of String) = StringHelper.SplitString(Time,",")
	Dim Account As String = "Req_Funding"

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
			'brapi.ErrorLog.LogMessage(si,$"Time: {tm}")
			'FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Else
				FilterString = $"{FilterString} + Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
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

#Region "Return MDEP Summary"
	Public Function MDEP_Summary(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
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
			'FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Else
				FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
			End If
		Next
'brapi.ErrorLog.LogMessage(si,"filter=" & FilterString)
				globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing

		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

'brapi.ErrorLog.LogMessage(si,"results = " & results.Count)
	
		Dim objU2DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U2_MDEP")

		For Each msb In results.Keys
		   msb.Scenario = vbNullString
		   msb.Entity =  e		   
		   msb.Account = vbNullString
		   msb.Origin = vbNullString
		   msb.IC = vbNullString
		   msb.Flow = vbNullString
		   msb.UD1 = vbNullString
		  
		
		   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU2DimPK, "U2#" &  msb.UD2, True,,)
		   msb.UD2 = lsAncestorList(0).Member.Name

		   msb.UD3 = vbNullString 
		   msb.UD4 = vbNullString
		   msb.UD5 = vbNullString
		   msb.UD6 = vbNullString
		   msb.UD7 = vbNullString
		   msb.UD8 = vbNullString	   
			If Not toSort.ContainsKey(msb.GetMemberScript)
				toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U2#{msb.UD2}")
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

#Region "Return Cert Summary Report"
	Public Function Cert_Summary_Report(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
'brapi.ErrorLog.LogMessage(si,"KLCM in br rule")
	Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
	Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim
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
				FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#L2_Final_PGM:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Else
				FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#L2_Final_PGM:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
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

#Region "Utilities: Get DataBuffer"
	
	Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object,ByRef args As DashboardStringFunctionArgs)
		'Dim filter = globals.GetStringValue("Filter","NA")

		'Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")				
		Dim Dictionary As New Dictionary(Of String, String)
		
		'BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,workspaceID, "workspace.GBL.GBL Objects.WSMU","Global_Buffers",Dictionary,customcalculatetimetype.CurrentPeriod)
		BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Global_Buffers","GetCVDataBuffer",Dictionary,customcalculatetimetype.CurrentPeriod)

'brapi.ErrorLog.LogMessage(si,"HIt 2")
	End Sub

#End Region		
	
#Region "Entity Text 1"
	Public Function EntityText1(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object

	Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
	Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim
		Return Entity
End Function
#End Region
	End Class
End Namespace
