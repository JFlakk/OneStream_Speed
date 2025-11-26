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
Imports Workspace.GBL.GBL_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.cPROBE_DataBuffers
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
				
				Select Case args.FunctionName
				Case "Get_cPROBE_CivAuth_by_DataElements"	
					Return Me.Get_cPROBE_CivAuth_by_DataElements()
				Case "Get_cPROBE_by_SAG"	
					Return Me.Get_cPROBE_by_SAG()
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Return Get_cPROBE_CivAuth_by_DataElements"
	Public Function Get_cPROBE_CivAuth_by_DataElements() As String
	Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
	Dim srcScenario As String = args.NameValuePairs.XFGetValue("SrcScenario","NA")
	Dim srcScenarioMbfInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id,srcScenario)
	Dim StartYr_Key As Integer = BRApi.Finance.Scenario.GetWorkflowTime(si, srcScenarioMbfInfo.Member.MemberId)
	Dim StartYr_String As String = BRApi.Finance.Time.GetNameFromId(si,StartYr_Key)
	Dim StartYr_Int As Integer
brapi.ErrorLog.LogMessage(si,"results = " & Entity)
	Dim toSort As New Dictionary(Of String, String)
	Dim output = ""
	Dim FilterString As String
	If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
	FilterString = String.Empty
brapi.ErrorLog.LogMessage(si,"results = " & StartyR_sTRING)
	If Integer.TryParse(StartYr_String,StartYr_Int) Then 
		For Year As Integer = StartYr_Int To StartYr_Int + 4
			'FilterString = $"Cb#{sCube}:C#Aggregated:S#{srcScenario}:T#{tm}:E#{e}:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO4:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top +
								Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO6:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top +
								Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO9:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top"
			Else
				FilterString = $"{FilterString} + Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO4:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top +
								Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO6:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top +
								Cb#Army:C#Aggregated:S#{srcScenario}:T#{Year}:E#[{Entity}]:A#BO9:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top"
			End If
		Next
		Dim Filters As String = $",[U1#APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]
								  ,[U2#PEG.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]
								  ,[U3#APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]
								  ,[U5#Total_APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]
								  ,[U7#UIC.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]
								  ,[U8#REIMS.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
		
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}){Filters})")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing

		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
		For Each msb In results.Keys
		   msb.Scenario = vbNullString
		   msb.Entity =  vbNullString	   
		   msb.Account = vbNullString
		   msb.Origin = vbNullString
		   msb.IC = vbNullString
		   msb.Flow = vbNullString  
		   msb.UD4 = vbNullString
		   msb.UD6 = vbNullString 
			If Not toSort.ContainsKey(msb.GetMemberScript)
				toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3},U5#{msb.UD5},U7#{msb.UD7},U8#{msb.UD8}")
			End If
		Next
		End If
	End If

	Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

	For Each item In sorted
		output &= item.key & ","
	Next
brapi.ErrorLog.LogMessage(si,"output:" & output)
	
	If output = "" Then
	output = "U5#One"
	End If
	
	Return output

	End Function
#End Region

#Region "Return Get_cPROBE_by_SAG"
	Public Function Get_cPROBE_by_SAG() As String
	Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
	Dim srcScenario As String = args.NameValuePairs.XFGetValue("SrcScenario","NA")
	Dim srcYear As String = args.NameValuePairs.XFGetValue("SrcYear","NA")
	Dim srcSPLNScenario As String = args.NameValuePairs.XFGetValue("SrcSPLNScenario","NA")
	If srcSPLNScenario.XFEqualsIgnoreCase("NA") Or srcSPLNScenario = String.Empty
		srcSPLNScenario = $"CMD_SPLN_C{srcYear}"
	End If
	Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
	
	Dim toSort As New Dictionary(Of String, String)
	Dim output = ""
	Dim FilterString As String
	If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
	FilterString = String.Empty

	FilterString = $"Cb#Army:C#Aggregated:S#{srcScenario}:T#{srcYear}:E#[{Entity}]:A#BO1:V#Periodic:O#Top:I#Top:F#Baseline:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
					Cb#Army:C#Aggregated:S#{srcScenario}:T#{srcYear}:E#[{Entity}]:A#BO7:V#Periodic:O#Top:I#Top:F#Baseline:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
					Cb#Army:C#Aggregated:S#{srcScenario}:T#{srcYear}:E#[{Entity}]:A#BOR:V#Periodic:O#Top:I#Top:F#Baseline:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top + 
					Cb#Army:C#Aggregated:S#CMD_TGT_C{srcYear}:T#{srcYear}:E#[{Entity}]:A#Target:V#Periodic:O#Top:I#Top:F#L2_Dist_Out:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None +
	                Cb#Army:C#Aggregated:S#{srcSPLNScenario}:T#{srcYear}:E#[{Entity}]:A#Obligations:V#Periodic:O#Top:I#Top:F#L2_Finalized_SPLN:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"

		Dim Filters As String = $",[U3#APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
		BRAPI.ErrorLog.LogMessage(si,$"FilterMembers(REMOVENODATA({FilterString}){Filters})")
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}){Filters})")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing

		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
		For Each msb In results.Keys
		   msb.Scenario = vbNullString
		   msb.Entity =  vbNullString	   
		   msb.Account = vbNullString
		   msb.Origin = vbNullString
		   msb.IC = vbNullString
		   msb.Flow = vbNullString  
		   msb.UD1 = vbNullString
		   msb.UD2 = vbNullString
		   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, $"U3#{msb.UD3}.Ancestors.Where(Text1 = SAG)", True,,)
   		   If Not lsAncestorList Is Nothing 
		   		msb.UD3 = lsAncestorList(0).Member.Name
		   End If
		   msb.UD4 = vbNullString
		   msb.UD5 = vbNullString
		   msb.UD6 = vbNullString
		   msb.UD7 = vbNullString
		   msb.UD8 = vbNullString
			If Not toSort.ContainsKey(msb.GetMemberScript)
				toSort.Add(msb.GetMemberScript, $"U3#{msb.UD3}")
			End If
		Next
		End If

	Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

	For Each item In sorted
		output &= item.key & ","
	Next
brapi.ErrorLog.LogMessage(si,"output:" & output)
	
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
	End Class
End Namespace
