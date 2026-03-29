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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.HQ_SPLN_DataBuffers
	Public Class MainClass
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As Object
		Public args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Select Case args.FunctionName
					Case "HQ_SPLN_Adj_CMD_Summary"
						Return Me.HQ_SPLN_Adj_CMD_Summary()
					Case "HQ_SPLN_Adj_CMD_Details"
						Return Me.HQ_SPLN_Adj_CMD_Details()
					Case "HQ_SPLN_HQDA_Summary_Review"
						Return Me.HQ_SPLN_HQDA_Summary_Review()
					Case "HQ_SPLN_HQDA_Detail_Review"
						Return Me.HQ_SPLN_HQDA_Detail_Review()
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		

#Region "Return HQ SPLN Adj CMD Summary"
	Public Function HQ_SPLN_Adj_CMD_Summary() As String
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim sEntity As String = args.NameValuePairs("Entity")
		Dim View As String = args.NameValuePairs("View")
		Dim Account As String = args.NameValuePairs("Account")
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = String.empty
		Dim commDims = String.empty
		Dim filters = String.Empty
'brapi.ErrorLog.LogMessage(si, $"Entity: {sEntity} | View: {View} | Account: {Account} | APPN: {APPN}")		
		If String.IsNullOrWhiteSpace(sEntity) Then Return "E#None:U1#None:U3#None"
		Dim cmd_SPLN_Scenario = stringhelper.ReplaceString(wfInfoDetails("ScenarioName"),"HQ_","CMD_",True)
'brapi.ErrorLog.LogMessage(si, $"Scenario:-{cmd_SPLN_Scenario}-")
		commDims = $"cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
					 cb#ARMY:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		filters = $"[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}),{filters})")
'brapi.ErrorLog.LogMessage(si, $"FilterMembers(REMOVENODATA({commDims}),{filters})")

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
				
			    msb.UD2 = vbNullString   	
				
		   		msb.UD4 = vbNullString
		   		msb.UD5 = vbNullString
		   		msb.UD6 = vbNullString
		   		msb.UD7 = vbNullString
		   		msb.UD8 = vbNullString	   
				If Not toSort.ContainsKey(msb.GetMemberScript)
					toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1}:U3#{msb.UD3}")
				End If
			Next
		End If

		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

		For Each item In sorted
			output &= item.key & ","
		Next
	
		If output = "" Then
			output = "U1#One"
		End If
'brapi.ErrorLog.LogMessage(si, $"output:{output}")
		Return output

	End Function
#End Region
		
#Region "Return HQ SPLN Adj CMD Details"
	Public Function HQ_SPLN_Adj_CMD_Details() As String
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim sEntity As String = args.NameValuePairs("Entity")
		Dim View As String = args.NameValuePairs("View")
		Dim Account As String = args.NameValuePairs("Account")
		Dim U1 As String = args.NameValuePairs("U1")
		Dim U3 As String = args.NameValuePairs("U3")
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = ""
	
		Dim commDims = String.empty
		Dim filters = String.Empty
		
		Select Case APPN		
		   	Case "OMA" 
				commDims = $"Cb#ARMY:C#Local:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#AdjInput:I#None:F#Copy_Adj:U5#None:U7#None"
				filters = $"[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U2#None],
							  [U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U4#None],
							  [U6#CostCat.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])"
			Case "OPA","RDTE"
				commDims = $"Cb#ARMY:C#Local:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#AdjInput:I#None:F#Copy_Adj:U5#None:U7#None"
				filters = $"[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U2#PEG.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U4#DollarType.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U6#CostCat.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])"
		End Select

		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}),{filters})")
		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

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
			   		toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1}:U3#{msb.UD3}:U2#{msb.UD2}:U4#{msb.UD4}:U6#{msb.UD6}")
			   End If
				
			Next
		End If	

		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

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

#Region "Return HQ SPLN HQDA Summary Review"
	Public Function HQ_SPLN_HQDA_Summary_Review() As String
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim Entity As String = args.NameValuePairs("Entity")
		Dim lEntity As List(Of String) = StringHelper.SplitString(Entity,",")
		Dim View As String = args.NameValuePairs("View")
		Dim Account As String = args.NameValuePairs("Account")
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim toSort As New Dictionary(Of String, String)
		Dim output = String.Empty
	
		Dim commDims = String.empty
		Dim filters = String.Empty
		If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"	
			
		Dim cmd_SPLN_Scenario = stringhelper.ReplaceString(wfInfoDetails("ScenarioName"),"HQ_","CMD_",True)

		For Each e As String In lEntity
			commDims = $"cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
						 cb#ARMY:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{wfInfoDetails("TimeName")}:E#[{e}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			filters = $"[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}),{filters})")
	
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
				
				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  e	   
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString
				   msb.UD2 = vbNullString   
				   msb.UD4 = vbNullString
				   msb.UD5 = vbNullString
				   msb.UD6 = vbNullString
				   msb.UD7 = vbNullString
				   msb.UD8 = vbNullString	   
				   If Not toSort.ContainsKey(msb.GetMemberScript)
				   		toSort.Add(msb.GetMemberScript, $"E#{msb.entity}:U1#{msb.UD1}:U3#{msb.UD3}")
				   End If
				 Next
			End If
		Next

		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

		For Each item In sorted
			output &= item.key & ","
		Next
	
		If output = "" Then
			output = "U3#One"
		End If
	
		Return output

	End Function
#End Region

#Region "Return HQ SPLN HQDA Detail Review"
	Public Function HQ_SPLN_HQDA_Detail_Review() As String
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		Dim sEntity As String = args.NameValuePairs("Entity")
		Dim View As String = args.NameValuePairs("View")
		Dim Account As String = args.NameValuePairs("Account")
		Dim U1 As String = args.NameValuePairs("U1")
		Dim U3 As String = args.NameValuePairs("U3")
		Dim APPN As String = args.NameValuePairs("APPN")

		Dim toSort As New Dictionary(Of String, String)
		If String.IsNullOrWhiteSpace(sEntity) Or sEntity.XFContainsIgnoreCase("Command") Then Return "E#None"		
		Dim output = String.Empty
	
		Dim commDims = String.empty
		Dim filters = String.Empty
		
		
		Select Case APPN		
		   	Case "OMA" 
				commDims = $"Cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#None:F#Copy_Adj:U5#None:U7#None"
				filters = $"[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U2#None],
							  [U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U4#None],
							  [U6#CostCat.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])"
			Case "OPA","RDTE"
				commDims = $"Cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#None:F#Copy_Adj:U5#None:U7#None"
				filters = $"[U1#{U1}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U2#PEG.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U3#{U3}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U4#DollarType.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],
							  [U6#CostCat.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])"
		End Select
			
		globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}),{filters})")

		GetDataBuffer(si,globals,api,args)

		If Not globals.GetObject("Results") Is Nothing
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

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
					toSort.Add(msb.GetMemberScript, $"E#{msb.Entity}:U1#{msb.UD1}:U3#{msb.UD3}:U2#{msb.UD2}:U4#{msb.UD4}:U6#{msb.UD6}")
				End If
			Next
		End If	

		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

		For Each item In sorted
			output &= item.key & ","
		Next
'brapi.ErrorLog.LogMessage(si,"output:" & output)
		
		If output = "" Then
			output = "U3#One"
		End If

		Return output

	End Function
#End Region

#Region "Utilities: Get DataBuffer"
	
	Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object,ByRef args As DashboardStringFunctionArgs)

		' Call the shared parameterized XFBR in GBL_String_Helper
		Dim result As Object = Workspace.GBL.GBL_Assembly.GBL_String_Helper.GetDataBuffer(si, globals, api, args)

	End Sub

#End Region


	End Class
End Namespace