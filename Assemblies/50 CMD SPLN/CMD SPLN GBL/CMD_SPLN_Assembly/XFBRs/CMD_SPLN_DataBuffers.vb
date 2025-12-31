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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_SPLN_DataBuffers
	Public Class MainClass
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
'brapi.ErrorLog.LogMessage(si,"here CM")
#Region "Return ORG Totals by SAG"
				If args.FunctionName.XFEqualsIgnoreCase("Org_Totals_by_SAG") Then				
					Return Me.Org_Totals_by_SAG(si,globals,api,args)
				End If				
#End Region


#Region "Return MDEP Summary"
				If args.FunctionName.XFEqualsIgnoreCase("MDEP_Summary") Then				
					Return Me.MDEP_Summary(si,globals,api,args)
				End If				
#End Region

#Region "Entities For Rollover"
				If args.FunctionName.XFEqualsIgnoreCase("EntitiesForRollover") Then				
					Return Me.EntitiesForRollover(si,globals,api,args)
				End If				
#End Region


#Region "Return Cert Summary Report"
				If args.FunctionName.XFEqualsIgnoreCase("Cert_Summary_Report") Then				
					Return Me.Cert_Summary_Report(si,globals,api,args)
				End If				
#End Region

'#Region "Package Summary"
'				If args.FunctionName.XFEqualsIgnoreCase("PackageSummary") Then				
'					Return Me.PackageSummary(si,globals,api,args)
'				End If				
'#End Region

#Region "Package Detail"
				If args.FunctionName.XFEqualsIgnoreCase("PackageDetail") Then				
					Return Me.PackageDetail(si,globals,api,args)
				End If				
#End Region

#Region "PackageEntityFlow"
				If args.FunctionName.XFEqualsIgnoreCase("PackageEntityFlow") Then				
					Return Me.PackageEntityFlow(si,globals,api,args)
				End If				
#End Region

#Region "Package Submit"
				If args.FunctionName.XFEqualsIgnoreCase("PackageSubmit") Then				
					Return Me.PackageSubmit(si,globals,api,args)
				End If				
#End Region

#Region "GetUserFundsCenterByWF"
				If args.FunctionName.XFEqualsIgnoreCase("GetUserFundsCenterByWF") Then				
					Return Me.GetUserFundsCenterByWF(si,globals,api,args)
				End If				
#End Region

#Region "Appn List Manage"
				If args.FunctionName.XFEqualsIgnoreCase("GetAPPNPropertyList") Then	
		
					Return Me.GetAPPNPropertyList(si,globals,api,args)
				End If	
#End Region 'Updated 10/21/2025

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
			'FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			Else
				FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
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

#Region "Package Entity Flow"
	Public Function PackageEntityFlow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim wfProfileNameAdj As String
		If wfProfileName.XFContainsIgnoreCase($".") Then
			wfProfileNameAdj = wfProfileName.Split("."c)(1).Split(" "c)(0)	
		Else
			wfProfileNameAdj = "Final"
		End If
	
		Dim lEntity As List(Of String) = stringhelper.splitstring(Me.GetUserFundsCenterByWF(si,globals,api,args),",")

		Dim Time As String = args.NameValuePairs("Time")
		Dim SubmissionFlag As String = args.NameValuePairs.XFGetValue("SubmissionFlag")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = ""
		
		Dim FilterString As String = String.Empty
		Dim Filters As String = String.Empty
				
		If lEntity.Count = 0 Then Return "E#None"
	'	BRapi.ErrorLog.LogMessage(si,"Hit 1")
		'Grab each entity based on WF and User security
		For Each e As String In lEntity		
			FilterString = String.Empty
			Dim entityMbrName As String = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, e).Member.Name
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, e).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,Time)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)		
			'Get each Entity user has security to and grab text 3
			'Subtring based on what level the entity is and get the children to pass into a list
			Dim objEntityDimPk As DimPk = BRapi.Finance.Dim.GetDimPk(si, "E_Army")
			Dim lsEntityChildren As List(Of MemberInfo)
			Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,e)
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
			Dim entityID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.Entity.Id,e)
			Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)

			Dim expansion As String = String.Empty
			If SubmissionFlag = "True" Then 
				If EntityLevel = "L2" Then		
					lsEntityChildren= BRApi.Finance.Members.GetMembersUsingFilter(si, objEntityDimPk, "E#" & e.Substring(0,3) ,True)'& ".ChildrenInclusive", True)
				Else					
					If Ent_Base = True Then
						lsEntityChildren= BRApi.Finance.Members.GetMembersUsingFilter(si, objEntityDimPk, "E#" & e.Substring(0,5), True)
					Else
						lsEntityChildren= BRApi.Finance.Members.GetMembersUsingFilter(si, objEntityDimPk, "E#" & e.Substring(0,5) ,True)'& ".ChildrenInclusive", True)
					End If 
				End If
			Else
				If EntityLevel = "L2" Then		
					lsEntityChildren= BRApi.Finance.Members.GetMembersUsingFilter(si, objEntityDimPk, "E#" & e.Substring(0,3) ,True)'& ".ChildrenInclusive", True)
				Else 
					lsEntityChildren= BRApi.Finance.Members.GetMembersUsingFilter(si, objEntityDimPk, "E#" & e.Substring(0,5) ,True)'& ".ChildrenInclusive", True)
				End If
			End If 
'			Next
	'		BRapi.ErrorLog.LogMessage(si,"Hit 2")
			Dim SPLNTGTresults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
			Dim TGTResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
			Dim SPLNStatusResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
			Dim SPLNTotResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell)
			
			'Get each entity and child entity through filter string
			For Each ChildMbr As MemberInfo In lsEntityChildren
'brapi.ErrorLog.LogMessage(si,"hit entity flow 2.5: " & ChildMbr.Member.Name)
				Dim FlowCommDims As String = $"F#{EntityLevel}"
				'Data Buffer 1 - SPLN + TGT: Get all of Spend Plan and Target, for each entity grab the entity level in the correct workflow
				FilterString = $"Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:{FlowCommDims}_{wfProfileNameAdj}_SPLN:V#Periodic:O#Top:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None" & _
								$"+ Cb#{sCube}:C#Aggregated:S#CMD_TGT_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:F#Tot_Dist_Final:V#Periodic:O#Top:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None" & _
								$"+ Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:{FlowCommDims}_{wfProfileNameAdj}_SPLN:V#Periodic:O#AdjInput:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({FilterString}))")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNTGTresults = globals.GetObject("Results")
				End If
				
				'Data Buffer 2 - TGT: Get all Target members under Tot Dist Final for each entity
				FilterString = $"Cb#{sCube}:C#Aggregated:S#CMD_TGT_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:F#Tot_Dist_Final:V#Periodic:O#Top:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({FilterString}))")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				TGTResults = globals.GetObject("Results")
				End If
				
				'Data Buffer 3 - SPLN: Get Each entity with level as status associated to the entity in Spend Plan
				FilterString = $"Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:{FlowCommDims}_{wfProfileNameAdj}_SPLN:V#Periodic:O#Top:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({FilterString}))")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNStatusResults = globals.GetObject("Results")
				End If
				
				'Data Buffer 4 - Get all of Spend Plan Members
				FilterString = $"Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{ChildMbr.Member.Name}]:F#CMD_Requirements:V#Periodic:O#Top:I#None:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({FilterString}))")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNTotResults = globals.GetObject("Results")
				End If
'brapi.ErrorLog.LogMessage(si,"entity = " & ChildMbr.Member.Name)					
				'Loop through dictionary / buffer 1 - SPLN + TGT: Get all of Spend Plan and Target, for each entity grab the entity level in the correct workflow
				For Each kvp As KeyValuePair(Of MemberScriptBuilder, DataBufferCell) In SPLNTGTresults
					Dim EntBase = Not BRApi.Finance.Members.HasChildren(si, entDimPk, ChildMbr.Member.MemberId)
					
					Dim msb As MemberScriptBuilder = kvp.Key
					
					'Brapi.ErrorLog.LogMessage(si, "Inside Loop 1")
					
					Dim cell As DataBufferCell = kvp.Value
					Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
					Dim lsAncestorListU1 As List(Of MemberInfo)
					'Check if exists in dictionary / buffer 3 - SPLN: Get Each entity with level as status associated to the entity in Spend Plan
					If SPLNStatusResults.Count > 0  AndAlso SPLNStatusResults.ContainsKey(msb) Then		
'					Brapi.ErrorLog.LogMessage(si, "Inside Loop 3")
						   msb.Scenario = vbNullString
						   msb.Entity = ChildMbr.Member.name
						   msb.Flow = $"{EntityLevel}_{wfProfileNameAdj}_SPLN"
						   msb.Account = vbNullString
						   'msb.Origin = vbNullString
						   If Not EntBase And wfProfileNameAdj.XFContainsIgnoreCase("Formulate") Then msb.Origin = "AdjInput" Else msb.Origin = "Top"
						   msb.IC = vbNullString
						   lsAncestorListU1 = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & msb.UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
						   msb.UD1 = lsAncestorListU1(0).Member.Name
						   msb.UD2 = vbNullString   
						   msb.UD3 = vbNullString
						   msb.UD4 = vbNullString
						   msb.UD5 = vbNullString
						   msb.UD6 = vbNullString
						   msb.UD7 = vbNullString
						   msb.UD8 = vbNullString	
						If Not toSort.ContainsKey(msb.GetMemberScript)
							toSort.Add(msb.GetMemberScript, $"F#{msb.Flow},E#{msb.Entity}")
						End If
						Continue For
					
					'Check if exists in dictionary / buffer 2 - TGT: Get all Target members under Tot Dist Final for each entity
					ElseIf TGTResults.ContainsKey(msb) Then
						'Check if also exists in dictionary / buffer 4 - Get all of Spend Plan Members
						'If entity is base, switch from Child to entity level
						'Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
						If EntBase = True Then
'							If SPLNTotResults IsNot Nothing AndAlso SPLNTotResults.ContainsKey(msb) Then 'Fixed for submitting the package to the next status
							If SPLNTotResults.Count > 0 Then 'IsNot Nothing Then
								Continue For

							ElseIf wfProfileNameAdj = "Formulate"
								   msb.Scenario = vbNullString
								   msb.Entity = e
								   msb.Flow = $"{EntityLevel}_{wfProfileNameAdj}_SPLN"
								   msb.Account = vbNullString
								   'msb.Origin = vbNullString
								   If Not EntBase And wfProfileNameAdj.XFContainsIgnoreCase("Formulate") Then msb.Origin = "AdjInput" Else msb.Origin = "Top"
								   msb.IC = vbNullString
								   lsAncestorListU1 = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & msb.UD1 & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
								   msb.UD1 = lsAncestorListU1(0).Member.Name
								   msb.UD2 = vbNullString   
								   msb.UD3 = vbNullString
								   msb.UD4 = vbNullString
								   msb.UD5 = vbNullString
								   msb.UD6 = vbNullString
								   msb.UD7 = vbNullString
								   msb.UD8 = vbNullString	
								If Not toSort.ContainsKey(msb.GetMemberScript)
									toSort.Add(msb.GetMemberScript, $"F#{msb.Flow},E#{msb.Entity}")
								End If
							End If
							Continue For
						End If
					End If
				Next
						
			Next
		Next
	
		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
		For Each item In sorted
			output = output & item.key & ","
		Next
		If output = "" Then
			output = "U8#None"
		End If
'Brapi.ErrorLog.LogMessage(si, "output = " & output)
		Return output
	
	End Function
#End Region

#Region "Package Detail"
	Public Function PackageDetail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
		Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim wfProfileNameAdj As String
		If wfProfileName.XFContainsIgnoreCase($".") Then
			wfProfileNameAdj = wfProfileName.Split("."c)(1).Split(" "c)(0)	
		Else
			wfProfileNameAdj = "Final"
		End If
		Dim ValidationSelected As String = args.NameValuePairs("ValidationSelected")	
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim SPLNTGTresults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
		Dim TGTResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
		Dim SPLNStatusResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
		Dim SPLNTotResults As Dictionary(Of MemberScriptBuilder, DataBufferCell)
		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell)
		
'brapi.ErrorLog.LogMessage(si,"Entity: " & Entity)
'brapi.ErrorLog.LogMessage(si,"APPN: " & APPN)
		Dim Scenario As String = args.NameValuePairs("Scenario")
		Dim Time As String = args.NameValuePairs("Time")
		Dim toSort As New Dictionary(Of String, String)
		Dim u1commDims As String = String.Empty
		Dim u2commDims As String = String.Empty
		Dim u3commDims As String = String.Empty
		Dim u4commDims As String = String.Empty
		Dim u5commDims As String = String.Empty
		Dim u6commDims As String = String.Empty
		Dim commDims As String
		Dim output = ""
		Dim FilterString As String = String.Empty
		Dim Filters As String = String.Empty
		If String.IsNullOrWhiteSpace(Entity) Or Entity = sCube Then Return "E#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None"
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
		Dim entityID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.Entity.Id,Entity)
		Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)
		Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Entity)

		Dim Val_Entity
		If Entity = sCube Then
			Val_Entity = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetCMD_L2FC(si,Entity)
		Else
			Val_Entity = Entity.Substring(0,3)
		End If
		
		Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,Val_Entity,Time)
		For Each pair In Val_Approach
			If pair.Key.xfcontainsignorecase("APPN") 
				If pair.Value = "Fund Code"
					FilterString &= $",[U1#{APPN}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False)]"
				Else
					u1commDims = $"U1#{APPN}:"
				End If
			Else If pair.Key.xfcontainsignorecase("MDEP")
				If pair.Value = "Yes"
					FilterString &= $",[U2#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False)]"
				Else
					u2commDims = "U2#Top:"
				End If
			Else If pair.Key.xfcontainsignorecase("DollarType")
				If pair.Value = "Yes"
					FilterString &= $",[U4#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False)]"
				Else
					u4commDims = "U4#Top:"
				End If
			Else If pair.Key.xfcontainsignorecase("cType")
				If pair.Value = "Yes"
					FilterString &= $",[U5#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False)]"
				Else
					u5commDims = "U5#Top:"
				End If
			Else If pair.Key.xfcontainsignorecase("Pay_NonPay")
				If pair.Value = "Yes"
					FilterString &= $",[U6#Pay_Benefits, U6#Non_Pay, U6#Pay_Benefits.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False), U6#Non_Pay.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Plan,MergeMembersFromReferencedCubes=False)]"
				Else
					u6commDims = "U6#CostCat:"
				End If
			End If
		Next
		Dim CurrStatus As String = $"{EntityLevel}_{wfProfileNameAdj}_SPLN"
		Dim FlowCommDims As String = $"F#{EntityLevel}"
				'Data Buffer 1 - SPLN + TGT: Get all of Spend Plan and Target, for each entity grab the entity level in the correct workflow
				commDims = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{Entity}]:F#{CurrStatus}:V#Periodic:O#Top:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None" & _
					$" + Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#CMD_TGT_C{Time}:T#{Time}:E#[{Entity}]:F#Tot_Dist_Final:V#Periodic:O#Top:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None" & _
					$" + Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{Entity}]:F#{CurrStatus}:V#Periodic:O#AdjInput:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({commDims}){FilterString})")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNTGTresults = globals.GetObject("Results")
				End If
				
				'Data Buffer 2 - TGT: Get all Target members under Tot Dist Final for each entity
				commDims = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#CMD_TGT_C{Time}:T#{Time}:E#[{Entity}]:F#Tot_Dist_Final:V#Periodic:O#Top:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({commDims}){FilterString})")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				TGTResults = globals.GetObject("Results")
				End If
				
				'Data Buffer 3 - SPLN: Get Each entity with level as status associated to the entity in Spend Plan
				commDims =  $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{Entity}]:F#{CurrStatus}:V#Periodic:O#Top:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({commDims}){FilterString})")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNStatusResults = globals.GetObject("Results")
				End If
				
				'Data Buffer 4 - Get all of Spend Plan Members
				commDims = $"Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{Entity}]:F#CMD_Requirements:V#Periodic:O#Top:I#None:{u1commDims}{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#None"
				globals.SetStringValue("Filter", $"FilterMembers(REMOVEZEROS({commDims}){FilterString})")
				If Not globals.GetObject("Filter") Is Nothing
					GetDataBuffer(si,globals,api,args)
				End If
				If Not globals.GetObject("Results") Is Nothing
				SPLNTotResults = globals.GetObject("Results")
				End If
		'Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
'brapi.ErrorLog.LogMessage(si,"results - " & results.Count)
		Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
		Dim objU6DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U6_CostCat")
		
			For Each kvp As KeyValuePair(Of MemberScriptBuilder, DataBufferCell) In SPLNTGTresults
					
				Dim msb As MemberScriptBuilder = kvp.Key
				
				
				
				If SPLNStatusResults.Count > 0  AndAlso SPLNStatusResults.ContainsKey(msb) Then	
					msb.Scenario = vbNullString
					msb.Entity =  Entity		   
					msb.Account = vbNullString
					msb.Flow = CurrStatus
					If Not Ent_Base And wfProfileNameAdj.XFContainsIgnoreCase("Formulate") Then msb.Origin = "AdjInput" Else msb.Origin = "Top"
					msb.IC = vbNullString
					'Get Member base on validation selection for UD1
					If Val_Approach("CMD_Val_APPN_Approach") = "Fund Code" Then
						msb.UD1 = msb.UD1
					Else
						msb.UD1 = $"{APPN}"
					End If
					'Get Member base on validation selection for UD2
					If Val_Approach("CMD_Val_MDEP_Approach") = "Yes" Then
						msb.UD2 = msb.UD2
					Else
						msb.UD2 = "Top"
					End If
					'Get Member base on validation selection for UD3
					If Val_Approach("CMD_Val_APE_Approach") = "APE9" Then
						msb.UD3 = msb.UD3
					Else
						If msb.UD3.XFContainsIgnoreCase("RDTE") Then
							msb.UD3 = msb.UD3
						Else If msb.UD3.XFContainsIgnoreCase("OPA") Then
							Dim lAncestorListU3OPA As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Parents.Where(MemberDim = U3_HQ_SPLN)", True)
							msb.UD3 = lAncestorListU3OPA(0).Member.Name
						Else
							Dim lAncestorListU3OMA As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True)
							msb.UD3 = lAncestorListU3OMA(0).Member.Name
						End If
					End If
					'Get Member base on validation selection for UD4
					If Val_Approach("CMD_Val_DollarType_Approach") = "Yes" Then
						msb.UD4 = msb.UD4
					Else
						msb.UD4 = "Top"
					End If
					msb.UD5 = vbNullString
					msb.UD6 = vbNullString
					msb.UD7 = vbNullString
					msb.UD8 = vbNullString	 
			
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.entity},F#{msb.flow},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3},U4#{msb.UD4},U6#{msb.UD6}")
					End If
					Continue For
					
					ElseIf TGTResults.ContainsKey(msb) Then
						If Ent_Base = True Then
							If SPLNTotResults.Count > 0 Then 
								Continue For

							ElseIf wfProfileNameAdj = "Formulate"
								msb.Scenario = vbNullString
								msb.Entity =  Entity		   
								msb.Account = vbNullString
								msb.Flow = CurrStatus
								If Not Ent_Base And wfProfileNameAdj.XFContainsIgnoreCase("Formulate") Then msb.Origin = "AdjInput" Else msb.Origin = "Top"
								msb.IC = vbNullString
								'Get Member base on validation selection for UD1
								If Val_Approach("CMD_Val_APPN_Approach") = "Fund Code" Then
									msb.UD1 = msb.UD1
								Else
									msb.UD1 = $"{APPN}"
								End If
								'Get Member base on validation selection for UD2
								If Val_Approach("CMD_Val_MDEP_Approach") = "Yes" Then
									msb.UD2 = msb.UD2
								Else
									msb.UD2 = "Top"
								End If
								'Get Member base on validation selection for UD3
								If Val_Approach("CMD_Val_APE_Approach") = "APE9" Then
									msb.UD3 = msb.UD3
								Else
									If msb.UD3.XFContainsIgnoreCase("RDTE") Then
										msb.UD3 = msb.UD3
									Else If msb.UD3.XFContainsIgnoreCase("OPA") Then
										Dim lAncestorListU3OPA As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Parents.Where(MemberDim = U3_HQ_SPLN)", True)
										msb.UD3 = lAncestorListU3OPA(0).Member.Name
									Else
										Dim lAncestorListU3OMA As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True)
										msb.UD3 = lAncestorListU3OMA(0).Member.Name
									End If
								End If
								'Get Member base on validation selection for UD4
								If Val_Approach("CMD_Val_DollarType_Approach") = "Yes" Then
									msb.UD4 = msb.UD4
								Else
									msb.UD4 = "Top"
								End If
								msb.UD5 = vbNullString
								msb.UD6 = vbNullString
								msb.UD7 = vbNullString
								msb.UD8 = vbNullString	 
						
								If Not toSort.ContainsKey(msb.GetMemberScript)
									toSort.Add(msb.GetMemberScript, $"E#{msb.entity},F#{msb.flow},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3},U4#{msb.UD4},U6#{msb.UD6}")
								End If
							End If 
						End If 
					End If 

				Next
'			Next
		
		
	Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

	For Each item In sorted
		output &= item.key & ","
	Next
'brapi.ErrorLog.LogMessage(si,"output:" & output)
	
	If output = "" Then
	output = "E#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None"
	End If
	
	Return output

	End Function
#End Region

#Region "Package Submit"
	Public Function PackageSubmit(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
'brapi.errorlog.Logmessage(si,"inside Submit CM DB")
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
		Dim slevelval As String = args.NameValuePairs("Level")
		For Each e As String In lEntity			
			FilterString = String.Empty
	
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, e).Member
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,Time)
			Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
			If Not sLevel =  entityText3 Then Continue For
			FilterString = $"Cb#{sCube}:C#Aggregated:S#CMD_SPLN_C{Time}:T#{Time}:E#[{e}]:V#Periodic:O#Top:I#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"					

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
			   
						
						Dim CommitSPLNMbrScrpt As String = $"Cb#{sCube}:E#[{e}]:S#CMD_SPLN_C{Time}:T#{Time}:V#Periodic:A#Commitments"
							Dim CommitSPLNAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, CommitSPLNMbrScrpt).DataCellEx.DataCell.CellAmount
							Dim ObligSPLNMbrScrpt As String = $"Cb#{sCube}:E#[{e}]:S#CMD_SPLN_C{Time}:T#{Time}:V#Periodic:A#Obligations"
							Dim ObligSPLNAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, ObligSPLNMbrScrpt).DataCellEx.DataCell.CellAmount
						   	Dim WHCommitSPLNMbrScrpt As String = $"Cb#{sCube}:E#[{e}]:S#CMD_SPLN_C{Time}:T#{Time}:V#Periodic:A#WH_Commitments"
							Dim WHCommitSPLNAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, WHCommitSPLNMbrScrpt).DataCellEx.DataCell.CellAmount
							Dim WHObligSPLNMbrScrpt As String = $"Cb#{sCube}:E#[{e}]:S#CMD_SPLN_C{Time}:T#{Time}:V#Periodic:A#WH_Obligations"
							Dim WHObligSPLNAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, WHObligSPLNMbrScrpt).DataCellEx.DataCell.CellAmount
							Dim Target As String = $"Cb#{sCube}:E#[{e}]:S#CMD_TGT_C{Time}:T#{Time}:V#Periodic:A#Target"
							Dim TargetAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, Target).DataCellEx.DataCell.CellAmount
							Dim WHTarget As String = $"Cb#{sCube}:E#[{e}]:S#CMD_TGT_C{Time}:T#{Time}:V#Periodic:A#TGT_WH"
							Dim WHTargetAmount As Long = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, WHTarget).DataCellEx.DataCell.CellAmount	
		
						
								If msb.Flow.XFContainsIgnoreCase($"{slevelval}_{wfProfileNameAdj}_SPLN") 
									If TargetAmount + WHTargetAmount <> 0 And ObligSPLNAmount + CommitSPLNAmount + WHObligSPLNAmount + WHCommitSPLNAmount = 0 And Not toSort.ContainsKey(msb.GetMemberScript) Then
										toSort.Add(msb.GetMemberScript, $"E#{msb.entity}F#{msb.flow}")
									Else If TargetAmount + WHTargetAmount = 0 And ObligSPLNAmount + CommitSPLNAmount + WHObligSPLNAmount + WHCommitSPLNAmount <> 0 And Not toSort.ContainsKey(msb.GetMemberScript) Then
										toSort.Add(msb.GetMemberScript, $"E#{msb.entity}F#{msb.flow}")
									Else If TargetAmount + WHTargetAmount <> 0 And ObligSPLNAmount + CommitSPLNAmount + WHObligSPLNAmount + WHCommitSPLNAmount <> 0 And Not toSort.ContainsKey(msb.GetMemberScript) Then
										toSort.Add(msb.GetMemberScript, $"E#{msb.entity}F#{msb.flow}")
									End If
								Else If msb.Flow.XFContainsIgnoreCase("Dist_Final")
									If TargetAmount + WHTargetAmount <> 0 And ObligSPLNAmount + CommitSPLNAmount + WHObligSPLNAmount + WHCommitSPLNAmount = 0 And Not toSort.ContainsKey(msb.GetMemberScript) Then
										toSort.Add(msb.GetMemberScript, $"E#{msb.entity}F#{msb.flow}")
									End If
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

#Region "Get User Funds Center By Workflow"
'New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundsCenterByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Dim output As String = String.Empty
				Dim userSecGroup As DataSet = brapi.Dashboards.Process.GetAdoDataSetForAdapter(si,False,"da_CMD_SPLN_GetUserSec","FundsCenterByWF",Nothing)	
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
		
#Region "Lockdown Data Cell U1 U8 GetMbrList (APPN List Manage)"
		
			Public Function GetAPPNPropertyList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
				
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN")
			Dim lAPPNMembList As List(Of memberinfo) = New List(Of MemberInfo)
			lAPPNMembList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#APPN.Base", True)
			Dim lsDimensions As New List (Of String)
			Dim output As String
		
			'Check loop
			For Each appn As MemberInfo In lAPPNMembList
				
				'--------- get APPN Text2 --------- 							
				'Dim sText2 As String = BRApi.Finance.Account.Text(si, appn, 2, wfScenarioTypeID,wfTimeId)
				Dim MemberAppn As Member = brapi.Finance.Members.GetMember(si,dimtype.UD1.Id, appn.Member.Name)
				Dim sText2 As String = brapi.Finance.UD.Text(si,dimtype.UD1.Id,MemberAppn.MemberId,2,0,0)

'Brapi.ErrorLog.LogMessage(si, appn.Member.Name & ": " & sText2)
				
			
				If Not sText2.XFContainsIgnoreCase("Yes") Then
					
					lsDimensions.Add("U1#" & appn.Member.Name & ":U8#ReadOnlyAnnotation")
				Else
						
					lsDimensions.Add(", U1#" & appn.Member.Name & ":U8#None")				
				End If

			Next
			
				For Each item As String In lsDimensions
					output &= item & ","
				Next
			
			Return output			
			
			End Function 	
#End Region 'Updated 10/21/25
		
#Region "Return Entities for Roll Over"
		Public Function EntitiesForRollover(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
							
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sCube As String = wfInfoDetails("CMDName")
	        Dim tm As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sScenario As String = "CMD_PGM_C" & tm
			Dim finalStatus As String = "L2_Final_PGM"
			Dim Account As String = "REQ_Funding"
			
			Dim sEntity As String = args.NameValuePairs.XFGetValue("entity", "")
			sEntity = sEntity.Replace("_General","")
			Dim tableName As String ="GetEntityListByStatus"
			
			Dim dt As New DataTable ("GetEntityListByStatus")
			
			Dim SQL As String
			SQL = $"Select DISTINCT Req.Entity
						   
					FROM XFC_CMD_PGM_REQ AS Req
					
					WHERE Req.WFScenario_Name = '{sScenario}'
					AND Req.WFCMD_Name = '{sCube}'
					AND Req.WFTime_Name = '{tm}'
					AND Req.Status = '{finalStatus}'
					Order By Req.Entity"
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
			End Using
			
			Dim FilterString As String = ""
			For Each r In dt.Rows
				Dim ent As String = r("Entity")
	
				If Not String.IsNullOrWhiteSpace(ent) Then
					If String.IsNullOrWhiteSpace(FilterString) Then
						FilterString = $"e#{ent}"
						'FilterString = $"Cb#{sCube}:C#USD:S#{sScenario}:T#{tm}:E#[{ent}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					Else
						FilterString = FilterString & $",e#{ent}"
						'FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{sScenario}:T#{tm}:E#[{ent}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"		
					End If	
				End If
			Next
'BRApi.ErrorLog.LogMessage(si, "FilterString: " & FilterString)	
Return FilterString' "E#A97AA_General"
			globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")

			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
				
			End If

			Return "E#A97AA_General"
		End Function

#End Region

	End Class
End Namespace