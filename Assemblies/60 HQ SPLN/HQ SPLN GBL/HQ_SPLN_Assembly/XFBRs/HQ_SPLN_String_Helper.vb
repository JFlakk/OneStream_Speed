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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.HQ_SPLN_String_Helper
	Public Class MainClass
		
		
	'Public Global_Functions As OneStream.BusinessRule.Finance.Global_Functions.MainClass
		
#Region "Main"		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Select Case args.FunctionName
					Case "ParseCommand"
						Return Me.ParseCommand(si, args)
					End Select

				
				If args.FunctionName.XFEqualsIgnoreCase("GetSpendPlanAdjUD3") Then
					Return Me.GetSpendPlanAdjUD3(si, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetSpendPlanAdj_TotalScript") Then
					Return Me.GetSpendPlanAdj_TotalScript(si, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetAccount2_OMA") Then
					Return Me.GetAccount2_OMA(si, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("ShowHideCommitObligRadBtn") Then
					Return Me.ShowHideCommitObligRadBtn(si, args)
				End If		
				
				If args.FunctionName.XFEqualsIgnoreCase("ReadWriteSPLAdjustmentCV") Then
					Return Me.ReadWriteSPLAdjustmentCV(si, globals, api,args)
				End If				

				If args.FunctionName.XFEqualsIgnoreCase("TopOrNone") Then
					Return Me.TopOrNone(si, args)
				End If

				If args.FunctionName.XFEqualsIgnoreCase("Command_SPLN_Intersection") Then
					Return Me.Command_SPLN_Intersection(si, globals, api, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("CMD_Details_Intersection") Then
					Return Me.CMD_Details_Intersection(si, globals, api, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("HQ_SPLN_Summary_Intersection") Then
'brapi.ErrorLog.LogMessage(si,"CM Hit 1")
					Return Me.HQ_SPLN_Summary_Intersection(si, globals, api, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("ShowHide_SPLN_Total") Then
					Return Me.ShowHide_SPLN_Total(si, args)
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("DisplayReturnComment") Then			
					Return Me.DisplayReturnComment(si, globals, api, args)
				End If
				If args.FunctionName.XFEqualsIgnoreCase("GetDODRate") Then
						Return Me.GetDODRate(si, globals, api, args)
				End If

				If args.FunctionName.XFEqualsIgnoreCase("getSPLNReturncmt") Then
						Return Me.getSPLNReturncmt(si, globals, api, args)
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region	

#Region "Parse Command"

		Private Function ParseCommand(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Example: XFBR(GEN_General_String_Helper, GetPrimaryFundCenter, Cube=|WFCube|)
				Dim sCommand As String = args.NameValuePairs.XFGetValue("Command")
				
				'Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0)				
'BRApi.ErrorLog.LogMessage(si,"GetPrimaryFundCenter:   Cube= " & sCommand)
				Return sCommand
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "GetSpendPlanAdjUD3"
'Return APEPT UD3 for adjustment cube view
		Private Function GetSpendPlanAdjUD3(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim U3 As String = args.NameValuePairs.XFGetValue("U3","")
				Dim U3DimPK As DimPk = BRApi.Finance.Dim.GetDimPK(si,"U3_APE_PT")
				Dim lU3 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3DimPK, $"U3#{U3}.Base.Where(Text8 Contains SpendPlan)", True,,)
	
				If lU3.Count = 0 Then Return Nothing
				
				Return "U3#" & lU3(0).Member.Name			
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetSpendPlanAdj_TotalScript"
'Return Total Row Script for Adjustment CV
		Private Function GetSpendPlanAdj_TotalScript(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim U1 As String = args.NameValuePairs.XFGetValue("U1","")
				Dim U3 As String = args.NameValuePairs.XFGetValue("U3","")
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				If U3.Length < 7 Then	
					Select Case True
					Case wfProfileName.XFContainsIgnoreCase("OMA")
			 			Return "U3#None"
					Case wfProfileName.XFContainsIgnoreCase("OPA")
			 			Return "U8#None"
					Case wfProfileName.XFContainsIgnoreCase("RDTE")
			 			Return "U8#None"
					End Select
				Else
					Select Case True
					Case wfProfileName.XFContainsIgnoreCase("OMA")
			 			Return $"U1#{U1}:U3#{U3}:U6#Top:Name()"
					Case wfProfileName.XFContainsIgnoreCase("OPA")
						Return $"U1#{U1}:U3#{U3}:U2#Top:U4#Top:U6#Top:U8#Top:Name()"
					Case wfProfileName.XFContainsIgnoreCase("RDTE")
						Return $"U1#{U1}:U3#{U3}:U2#Top:U4#Top:U6#Top:U8#Top:Name()"
					End Select
'					Return "GetDataCell(CVR(Key)):Name(Total)"
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetAccount2_OMA"
'Obligation account or Commitment account for variance column
		Private Function GetAccount2_OMA(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim Account As String = args.NameValuePairs.XFGetValue("Account","")
				If Account.XFContainsIgnoreCase("Commitment") Then	
			 		Return "Obligations"
				ElseIf Account.XFContainsIgnoreCase("Obligation")
					Return "Commitments"
				End If
				
				Return "None"
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "ShowHideCommitObligRadBtn"
		Public Function ShowHideCommitObligRadBtn(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCbxAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			Dim sIsVisible As String = "IsVisible = False"
			
			If sCbxAPPN.XFContainsIgnoreCase("OMA") Then
					sIsVisible = "IsVisible = True"
			End If
			
			Return sIsVisible
		End Function
		
#End Region

#Region "ReadWriteSPLAdjustmentCV"
		Public Function ReadWriteSPLAdjustmentCV(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfProfile As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfprofilesubstring As String = wfProfile.Split(".")(1)
			'Brapi.ErrorLog.LogMessage(si, "wfpro" & wfprofilesubstring)
			Dim sAppn As String = wfprofilesubstring.Replace(" Validate","")
			'Brapi.ErrorLog.LogMessage(si, "sAppn" & sAppn)
			Dim sDataBufferValidationFlag As String =  "Cb#" & sCube & ":S#" & sScenario & ":T#" & wfTimeName & "M12:C#Local:V#Annotation:E#" & sACOM & "_General:A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#" & sAppn & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sSPLockedval As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sDataBufferValidationFlag).DataCellEx.DataCellAnnotation
			Dim sIsVisible As String = ""
				
			If sSPLockedval.XFContainsIgnoreCase("Yes") Then
					sIsVisible = "U8#ReadOnlyData"
				Else
					sIsVisible = "U8#None"
			End If
			
			
			Return sIsVisible
		End Function
		
#End Region

#Region "Top or None"
'Return Top of None - used for Periodic vs Cummulative
		Private Function TopOrNone(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim View As String = args.NameValuePairs.XFGetValue("View","")
'BRApi.ErrorLog.LogMessage(si, "Variance" )			
				If View.XFContainsIgnoreCase("YTD") Then 
'BRApi.ErrorLog.LogMessage(si, "Top" )	
					Return "Top"
				Else
'BRApi.ErrorLog.LogMessage(si, "None" )	
					Return "None"
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Return HQDA Summary Review Intersections"
	Public Function Command_SPLN_Intersection(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
	Dim Entity As String = args.NameValuePairs("Entity")
	Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
	Dim Scenario As String = args.NameValuePairs("Scenario")
	Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
	Dim Time As String = args.NameValuePairs("Time")
	Dim View As String = args.NameValuePairs("View")
	Dim Account As String = args.NameValuePairs("Account")
	Dim APPN As String = args.NameValuePairs("APPN")
	Dim lAPPN As List(Of String) = StringHelper.SplitString(APPN,"_")
	APPN = lAPPN(4)
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
		For Each s As String In lScenario
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#ARMY:C#Local:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			Else
				FilterString = $"{FilterString} + Cb#ARMY:C#Aggregated:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"		
			End If
		Next

'brapi.ErrorLog.LogMessage(si,$"Hit 2nd: {FilterString}")
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing
		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

'brapi.ErrorLog.LogMessage(si,$"Hit - E#{e}:U1#Top:U3#{APPN}")
	
		Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
		Dim objCommandDimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "E_ARMY")
		Dim Command As String = e.Split("_")(0) 
		Dim lCommand As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objCommandDimPK, $"E#{Command}.Ancestors.Where(Text1 = {Command})", True,,)
		Dim CommandName As String = lCommand(0).Member.Name
		
		'Add Total row
		If Not toSort.ContainsKey($"E#{CommandName}:U1#Top:U3#{APPN}")
			toSort.Add($"E#{CommandName}:U1#Top:U3#{APPN}:Name(Total)",$"E#{CommandName},U1#Top")
		End If
		For Each msb In results.Keys
		   msb.Scenario = vbNullString
		   msb.Entity =  CommandName		   
		   msb.Account = vbNullString
		   msb.Origin = vbNullString
		   msb.IC = vbNullString
		   msb.Flow = vbNullString
		   msb.UD2 = vbNullString   
	'brapi.ErrorLog.LogMessage(si,"320" & msb.UD3.Split("_")(0))
	  	   Select Case msb.UD3.Split("_")(0)
		   Case "OMA" 
	'brapi.ErrorLog.LogMessage(si,$"Hit")		
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Ancestors.Where(Text1 = SAG)", True,,)
			   msb.UD3 = lsAncestorList(0).Member.Name
		   Case "OPA" 	 
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Parents", True,,)
			   msb.UD3 = lsAncestorList(0).Member.Name
	       End Select 
		   
		   msb.UD4 = vbNullString
		   msb.UD5 = vbNullString
		   msb.UD6 = vbNullString
		   msb.UD7 = vbNullString
		   msb.UD8 = vbNullString	   
			If Not toSort.ContainsKey(msb.GetMemberScript)
				toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1}")
			End If
		Next
		End If
	Next

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

#Region "Return HQDA Details Review Intersections"
	Public Function CMD_Details_Intersection(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
		Dim sCommand As String = args.NameValuePairs("Entity")
		Dim Scenario As String = args.NameValuePairs("Scenario")
		Dim Time As String = args.NameValuePairs("Time")
		Dim View As String = args.NameValuePairs("View")
		Dim Account As String = args.NameValuePairs("Account")
		Dim U1 As String = args.NameValuePairs("U1")
		Dim U3 As String = args.NameValuePairs("U3")
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim lAPPN As List(Of String) = StringHelper.SplitString(APPN,"_")
		APPN = lAPPN(4)
		Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCommand), 1, 0, 0)
		sEntity += "_General"
'brapi.ErrorLog.LogMessage(si,$"U1: {U1} | U3: {U3} | Entity: {sEntity}")	
'brapi.ErrorLog.LogMessage(si,$"Account: {Account}")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = ""
		If String.IsNullOrWhiteSpace(sCommand) Or sCommand.XFContainsIgnoreCase("Command") Then Return "E#None"		
		Dim FilterString As String = String.Empty
		
		Select Case APPN
#Region "OMA"			
	   	Case "OMA" 
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U2#None:U4#None:U5#None:U7#None"
			Else
				FilterString = $"{FilterString} + Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U2#None:U4#None:U5#None:U7#None"		
			End If

'brapi.ErrorLog.LogMessage(si,$"Hit 2nd: {FilterString}")
			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U6#None,U6#Pay_Benefits.Children,U6#Non_Pay.Children,U6#Obj_Class.Children])")

			GetDataBuffer(si,globals,api,args)

			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

				'Add Total row
				toSort.Add($"E#{sEntity}:U1#{U1}:U3#{U3}:U6#Top:Name(Total)",$"E#{sEntity}:U6#Top")

				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  sEntity
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString		
				   msb.UD2 = vbNullString
				   msb.UD4 = vbNullString
				   msb.UD5 = vbNullString
				   msb.UD7 = vbNullString   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}")
					End If
				Next
			End If	
#End Region
#Region "OPA"			
	   	Case "OPA" 
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U5#None:U7#None"
			Else
				FilterString = $"{FilterString} + Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U5#None:U7#None"		
			End If

'brapi.ErrorLog.LogMessage(si,$"Hit 2nd: {FilterString}")
			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U6#None,U6#Pay_Benefits.Children,U6#Non_Pay.Children,U6#Obj_Class.Children])")

			GetDataBuffer(si,globals,api,args)

			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

				'Add Total row
				toSort.Add($"E#{sEntity}:U1#{U1}:U2#Top:U3#{U3}:U4#Top:U6#Top:U8#Top:Name(Total)",$"E#{sEntity}:U1#{U1}:U2#Top:U3#{U3}")

				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  sEntity
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString
				   msb.UD3 = U3				   
				   msb.UD5 = vbNullString
				   msb.UD7 = vbNullString   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}")
					End If
				Next
			End If	
#End Region
#Region	"RDTE"  
		Case "RDTE" 
			If String.IsNullOrWhiteSpace(FilterString) Then
				FilterString = $"Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U5#None:U7#None"
			Else
				FilterString = $"{FilterString} + Cb#ARMY:C#Local:S#{Scenario}:T#{Time}:E#[{sEntity}]:A#{Account}:V#{View}:O#BeforeAdj:I#None:F#Baseline:U5#None:U7#None"		
			End If

'brapi.ErrorLog.LogMessage(si,$"Hit 2nd: {FilterString}")
			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U6#None,U6#Pay_Benefits.Children,U6#Non_Pay.Children,U6#Obj_Class.Children])")

			GetDataBuffer(si,globals,api,args)

			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

				'Add Total row
				toSort.Add($"E#{sEntity}:U1#{U1}:U2#Top:U3#{U3}:U4#Top:U6#Top:U8#Top:Name(Total)",$"E#{sEntity}:U1#{U1}:U2#Top:U3#{U3}")

				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  sEntity
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString			   
				   msb.UD5 = vbNullString
				   msb.UD7 = vbNullString   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}")
					End If
				Next
			End If
#End Region 
		End Select
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


#Region "Return CMD HQ Summary Intersections"
	Public Function HQ_SPLN_Summary_Intersection(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
	Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
	Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim & "_General"
	Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
	Dim Scenario As String = args.NameValuePairs("Scenario")
	Dim lScenario As List(Of String) = StringHelper.SplitString(Scenario,",")
	Dim Time As String = args.NameValuePairs("Time")
	Dim View As String = args.NameValuePairs("View")
	Dim Account As String = args.NameValuePairs("Account")
	Dim APPN As String = args.NameValuePairs("APPN")
	Dim lAPPN As List(Of String) = StringHelper.SplitString(APPN,"_")
	APPN = lAPPN(5).Replace("Adjustments","").Trim
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
		For Each s As String In lScenario
			FilterString = $"Cb#ARMY:C#USD:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
'			If String.IsNullOrWhiteSpace(FilterString) Then
'				FilterString = $"Cb#ARMY:C#Aggregated:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
'			Else
'				FilterString = $"{FilterString} + Cb#ARMY:C#Local:S#{s}:T#{Time}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"		
'			End If
		Next

		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}),[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])")
'brapi.ErrorLog.LogMessage(si,"Hit 1 SPLN")
		GetDataBuffer(si,globals,api,args)
'brapi.ErrorLog.LogMessage(si,"Hit 2 SPLN")
'If results Is Nothing Then
'	brapi.ErrorLog.LogMessage(si,"CM FAIL Return")
'Else
		If Not globals.GetObject("Results") Is Nothing
		Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

'brapi.ErrorLog.LogMessage(si,$"Hit 3 - E#{e}:U1#Top:U3#{APPN}")
	
		Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
		Dim objCommandDimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "E_ARMY")

		For Each msb In results.Keys
		   msb.Scenario = vbNullString
		   msb.Entity =  e		   
		   msb.Account = vbNullString
		   msb.Origin = vbNullString
		   msb.IC = vbNullString
		   msb.Flow = vbNullString
		   msb.UD2 = vbNullString   
	'brapi.ErrorLog.LogMessage(si,"570" & msb.UD3.Split("_")(0))
	  	   Select Case msb.UD3.Split("_")(0)
		   Case "OMA" 
	'brapi.ErrorLog.LogMessage(si,$"Hit CM")		
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Ancestors.Where(Text1 = SAG)", True,,)
			   msb.UD3 = lsAncestorList(0).Member.Name
		   Case "OPA" 	 
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Parents", True,,)
			   msb.UD3 = lsAncestorList(0).Member.Name
	       End Select 
		   
		   msb.UD4 = vbNullString
		   msb.UD5 = vbNullString
		   msb.UD6 = vbNullString
		   msb.UD7 = vbNullString
		   msb.UD8 = vbNullString	   
			If Not toSort.ContainsKey(msb.GetMemberScript)
				toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1}")
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

#Region "ShowHide_SPLN_Total Column"

		Private Function ShowHide_SPLN_Total(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim View As String = args.NameValuePairs.XFGetValue("View")
				
				If View.XFContainsIgnoreCase("YTD")	
					Return "IsColumnVisible = CVMathOnly"
				Else 
					Return "IsColumnVisible = True"
				End If 
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "DisplayReturnComment: Display Return Comment"
	Public Function DisplayReturnComment(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Dim wfProfile As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfprofilesubstring As String = wfProfile.Split(".")(1)
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("SPLNScenario")
			Dim sTime As String = args.NameValuePairs.XFGetValue("SPLNTime")
			
			
			Dim sAppn As String = wfprofilesubstring.Replace(" Validate","")
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0) & "_General"
			Dim sMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & "M12:V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#" & sAppn & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
		
			Dim sReturncmt As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sMemberScript).DataCellEx.DataCellAnnotation
'Brapi.ErrorLog.LogMessage(si, "Return cmt" & sReturncmt)
		If String.IsNullOrWhiteSpace(sReturncmt) Then 
			Return "False"
		Else  
			Return "True"
		End If
	End Function
#End Region

#Region "GetDODRate:"
	'Get SPLN Return Comment
	Public Function GetDODRate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
			Dim wfProfile As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfprofilesubstring As String = wfProfile.Split(".")(1)
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sTime As String = args.NameValuePairs.XFGetValue("SPLNTime")
			Dim sScenario As String = "BUD_C" & sTime
			Dim sAppn As String = wfprofilesubstring.Replace(" Validate","")
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0) & "_General"
			Dim sMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#RMW_Cycle_Config_Monthly" & ":T#" & sTime & "M10:V#Periodic:A#DOD_Rate:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"	                                        
brapi.ErrorLog.LogMessage(si, $"mbrcript: {sMemberScript}")			
			Dim SPLNAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sMemberScript)
			Dim SPLNReturnCmt As Integer = SPLNAnnotationCell.DataCellEx.DataCell.CellAmount
			Dim FullreturnComment As String = "July DOD Rate: " & math.Round(SPLNReturnCmt) & "%"
			Return FullreturnComment
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
#End Region


Public Function GetScenarios(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
			Dim Output As String = String.empty
			Dim lsFilteredDataRowList As List( Of MemberInfo) = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"S_RMW", "S#Scenarios.Base.where(name contains PGM or name contains TGT or name contains SPLN)",False)
			For Each scenario As MemberInfo In lsFilteredDataRowList
				Dim ScenStartTime As Integer = BRApi.Finance.Scenario.GetWorkflowStartTime(si, scenario.Member.MemberId)
				If String.IsNullOrEmpty(output)
					output = "S#" & Scenario.Member.name & ":T#" & ScenStartTime
				Else 
					output = output & ",S#" & Scenario.Member.Name & ":T#" & ScenStartTime
				End If 
			Next
			Return output
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
	
#Region "getSPLNReturncmt:"
	'Get SPLN Return Comment
	Public Function getSPLNReturncmt(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
			Dim wfProfile As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfprofilesubstring As String = wfProfile.Split(".")(1)
			Dim sCube As String = args.NameValuePairs.XFGetValue("Cube")		
			Dim sScenario As String = args.NameValuePairs.XFGetValue("SPLNScenario")
			Dim sTime As String = args.NameValuePairs.XFGetValue("SPLNTime")
			Dim sAppn As String = wfprofilesubstring.Replace(" Validate","")
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0) & "_General"
			Dim sMemberScript As String = "Cb#" & sCube & ":E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sTime & ":V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#" & sAppn & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
	
			Dim SPLNAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sMemberScript)
			Dim SPLNReturnCmt As String = SPLNAnnotationCell.DataCellEx.DataCellAnnotation
			Dim FullreturnComment As String = "HQDA Return Comment: " & SPLNReturnCmt
			Return FullreturnComment
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
#End Region

#Region "Utilities: Get DataBuffer"
	
	Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object,ByRef args As DashboardStringFunctionArgs)
		
		'Dim workspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "00 GBL")				
		Dim Dictionary As Dictionary(Of String, String)
		
		'BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,workspaceID, "workspace.GBL.GBL Objects.WSMU","Global_Buffers",Dictionary,customcalculatetimetype.CurrentPeriod)
		BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Global_Buffers","GetCVDataBuffer",Dictionary,customcalculatetimetype.CurrentPeriod)


	End Sub

#End Region
	End Class
End Namespace