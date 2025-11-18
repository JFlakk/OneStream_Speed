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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_TGT_DataBuffers
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
				Case "getreviewsumtgtdist"	
					Return Me.GetReviewSumTgtDist()
				Case "getreviewtgtdist"	
					Return Me.getreviewtgtdist()
				Case "getreviewrectgt"
					Return Me.GetReviewRecTGT()
				Case "getmodify_distwh"
					Return Me.GetModify_DistWH()
				Case "getreviewtoplinesummary"
					Return Me.GetReviewTopLineSummary()
				Case "gettopline_ctrls"
					Return Me.getTopLine_Ctrls()
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

	#Region "Get Review Sum TGT Dist"
		Public Function GetReviewSumTgtDist() As String
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
			Dim cmd_L2FC As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetCMD_L2FC(si,wfInfoDetails("CMDName"))
			Dim RptType As String = args.NameValuePairs("RptType")  'Dist/Dist Variance
			Dim RowType As String = args.NameValuePairs("RowType")
			Dim Scenario As String = args.NameValuePairs("Scenario")
			Dim Time As String = args.NameValuePairs("Time")
			Dim toSort As New Dictionary(Of String, String)
			Dim u1commDims As String = String.Empty
			Dim u2commDims As String = String.Empty
			Dim u4commDims As String = String.Empty
			Dim u5commDims As String = String.Empty
			Dim u6commDims As String = String.Empty
			Dim output = ""
			Dim commDims As String
			Dim u1Mbr As String = "OM"
			If RowType = "PROC"
				u1Mbr = "PROC"
			ElseIf RowType = "RDTE"
				u1Mbr = "RDA"
			ElseIf RowType = "OTH_APPN"
				u1Mbr ="OTH_APPN"
			End If
			Dim u1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN")	
			Dim appnList As New List(Of MemberInfo)
			
			Dim FilterString As String = $",[U3#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
BRAPI.ErrorLog.LogMessage(si,$"Hit1: {Entity}")
			If Entity = String.Empty Or Entity = "NA" Or Entity = vbNullString Then
				BRAPI.ErrorLog.LogMessage(si,$"Hit1: {Entity}")
				Return "U1#One:U2#Top:U3#Top:U4#Top:U6#Top"
			End If
			Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Entity)
			Dim Acct As String = "Target"
			Dim Flow As String = $"{EntityLevel}_Dist_Balance"
			Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,cmd_L2FC,Time)

			For Each pair In Val_Approach
				If pair.Key.xfcontainsignorecase("APPN")
					If pair.Value <> "APPN"
						FilterString &= $",[U1#{u1Mbr}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
						appnList = BRApi.Finance.Members.GetMembersUsingFilter(si, u1DimPk, $"U1#{u1Mbr}", True)
					Else
						appnList = BRApi.Finance.Members.GetMembersUsingFilter(si, u1DimPk, $"U1#{u1Mbr}.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)", True)
					End If
				ElseIf pair.Key.xfcontainsignorecase("MDEP")
					If pair.Value = "Yes"
						FilterString &= $",[U2#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u2commDims = "U2#Top:"
					End If
				ElseIf pair.Key.xfcontainsignorecase("DollarType")
					If pair.Value = "Yes"
						FilterString &= $",[U4#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u4commDims = "U4#Top:"
					End If
				ElseIf pair.Key.xfcontainsignorecase("Pay_NonPay")
					If pair.Value = "Yes"
						FilterString &= $",[U6#Pay_Benefits,U6#Non_Pay]"
					Else
						u6commDims = "U6#CostCat:"
					End If
				End If
			Next
			Dim dict_FundCodeAPPN1Map As New Dictionary(Of String, String)
			If Val_Approach("CMD_Val_APPN_Approach") <> "APPN"		
				'Build SQL to return list of requirements that fit the user's criterias
				Dim SQL As String
				SQL = $"SELECT Partial_Fund_CD,Appropriation_CD  
						FROM XFC_APPN_Mapping"
	
				'Fetch mapping table 
				Dim dtU1Mappings As New DataTable()
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtU1Mappings = BRApi.Database.ExecuteSql(dbConn,SQL,True)
				End Using
				
				'Loop through the fetched datatable and load to dictionary
				For Each u1MapRow As DataRow In dtU1Mappings.Rows		
					If Not dict_FundCodeAPPN1Map.ContainsKey(u1MapRow.Item("Partial_Fund_CD")) Then 
						dict_FundCodeAPPN1Map.Add(u1MapRow.Item("Partial_Fund_CD"),u1MapRow.Item("Appropriation_CD"))
					End If
				Next
			End If

			For Each appn As MemberInfo In appnList
				
				If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
				If appn.Member.Name = "APPN"
					commDims =  $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Acct}:V#Periodic:O#Top:I#Top:F#{Flow}:{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#Top"
				Else
					commDims =  $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Acct}:V#Periodic:O#Top:I#Top:F#{Flow}:{u2commDims}{u4commDims}U5#Top:{u6commDims}U7#Top:U8#Top"
				End If
				
				globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}){FilterString})")
		
				GetDataBuffer(si,globals,api,args)
		
				If Not globals.GetObject("Results") Is Nothing
		
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
				For Each msb In results.Keys
				   msb.Scenario = vbNullString
				   msb.Entity =  Entity		   
				   msb.Account = vbNullString
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = vbNullString
				   If Val_Approach("CMD_Val_APPN_Approach") = "APPN" Then
					   msb.UD1 = appn.Member.Name
				   Else
					   msb.UD1 = dict_FundCodeAPPN1Map.XFGetValue(msb.UD1.Substring(0,7),"NA")
				   End If
				   If u2commDims = "U2#Top:"
				   		msb.UD2 = "Top"
				   End If
				
				   If Val_Approach("CMD_Val_APE_Approach") = "ABO Level"
						If RowType = "RDTE" Then
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
						Else If RowType <> "PROC"
							Dim U3mbr_Expansion As String = ".Ancestors.Where(Text1 = SAG)"
							Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
							Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, $"U3#{msb.UD3}{U3mbr_Expansion}",True)
							msb.UD3 = lsAncestorList(0).Member.Name	
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
						Else
							msb.UD3 = msb.UD3.Substring(0,msb.UD3.Length-3)
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3.Substring(0,msb.UD3.Length-3))
						End If
				   End If
				   If u4commDims = "U4#Top:"
				   		msb.UD4 = "Top"
				   End If
				   msb.UD5 = vbNullString
				   If u6commDims = "U6#CostCat:"
				   		msb.UD6 = "CostCat"
				   End If
				   msb.UD7 = vbNullString
				   msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1},U3#{msb.UD3},U2#{msb.UD2},U4#{msb.UD4},U6#{msb.UD6}")
					End If
				Next
			End If
		Next
	
		Dim sorted As Dictionary(Of String, String) = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
	
		For Each item In sorted
			output &= item.key & ","
		Next
		
		If output = "" Then
		output = "U5#One"
		End If
		
		Return output
	
		End Function
	#End Region

	#Region "Get Review TGT Dist"
		Public Function getreviewtgtdist() As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim RowType As String = args.NameValuePairs("RowType") 'OMA/OPA/RDTE/Other
			Dim RptType As String = args.NameValuePairs("RptType")  'Dist/Dist Variance
			Dim Scenario As String = args.NameValuePairs("Scenario")
			Dim Time As String = args.NameValuePairs("Time")
			Dim toSort As New Dictionary(Of String, String)
			Dim u1commDims As String = String.Empty
			Dim u2commDims As String = String.Empty
			Dim u4commDims As String = String.Empty
			Dim u5commDims As String = String.Empty
			Dim u6commDims As String = String.Empty
			Dim output = ""
			Dim commDims As String
			Dim FilterString As String
			
			Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Entity)
			Dim Acct As String = "Target"
			Dim Flow As String = $"{EntityLevel}_Dist_Balance"
			Dim Val_Approach = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetValidationApproach(si,wfInfoDetails("CMDName"),Time)
			For Each pair In Val_Approach
				If pair.Key.xfcontainsignorecase("APPN")
					If pair.Value = "Yes"
						FilterString = $",[U1#APPN.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u1commDims = "U1#Top:"
					End If
				Else If pair.Key.xfcontainsignorecase("MDEP")
					If pair.Value = "Yes"
						FilterString = $",[U2#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u2commDims = "U2#Top:"
					End If
				Else If pair.Key.xfcontainsignorecase("DollarType")
					If pair.Value = "Yes"
						FilterString = $",[U4#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u4commDims = "U4#Top:"
					End If
				Else If pair.Key.xfcontainsignorecase("cType")
					If pair.Value = "Yes"
						FilterString = $",[U5#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
					Else
						u5commDims = "U5#Top:"
					End If
				Else If pair.Key.xfcontainsignorecase("Pay_NonPay")
					If pair.Value = "Yes"
						FilterString = $",[U6#Pay_Benefits,U6#Non_Pay]"
					Else
						u6commDims = "U6#Top:"
					End If
				End If
			Next
						

			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
			Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{Entity}", True)
			

			If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
			commDims =  $"Cb#{wfInfoDetails("CMDName")}:C#USD:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Acct}:V#Periodic:O#Top:I#Top:F#{Flow}:{u1commDims}{u2commDims}{u4commDims}{u5commDims}{u6commDims}U7#Top:U8#Top"

			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}){FilterString})")
	
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
	
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

			Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
	
			For Each msb In results.Keys
			   msb.Scenario = vbNullString
			   msb.Entity =  Entity		   
			   msb.Account = vbNullString
			   msb.Origin = vbNullString
			   msb.IC = vbNullString
			   msb.Flow = vbNullString
			   msb.UD1 = vbNullString
			   msb.UD2 = vbNullString   
			
			   Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" &  msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True)
			   msb.UD3 = lsAncestorList(0).Member.Name	   
			   msb.UD4 = vbNullString
			   msb.UD5 = vbNullString
			   msb.UD6 = vbNullString
			   msb.UD7 = vbNullString
			   msb.UD8 = vbNullString	   
				If Not toSort.ContainsKey(msb.GetMemberScript)
					toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3},U4#{msb.UD4},U5#{msb.UD5},U6#{msb.UD6}")
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
	
	#Region "Get Review TopLine Summary"
		Public Function GetReviewTopLineSummary() As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim RowType As String = args.NameValuePairs.XFGetValue("RowType","NA") 
			Dim Scenario As String = args.NameValuePairs("Scenario")
			Dim PBScenario As String = args.NameValuePairs.XFGetValue("PBScenario","NA")
			Dim Time As String = args.NameValuePairs("Time")
			Dim Dist_Acct As String = "Target"
			Dim Dist_Flow As String = "L2_Ctrl_Intermediate"
			Dim mbr_Expansion As String = String.Empty
			Dim u1Mbr As String = "OM"
			If RowType = "PROC"
				u1Mbr = "PROC"
			ElseIf RowType = "RDTE"
				u1Mbr = "RDA"
			ElseIf RowType = "OTH_APPN"
				u1Mbr ="OTH_APPN"
			End If
			Dim u1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN")		
			Dim appnList = BRApi.Finance.Members.GetMembersUsingFilter(si, u1DimPk, $"U1#{u1Mbr}.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)", True)

			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim FilterString As String
		    For Each appn As MemberInfo In appnList

				If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
				FilterString = String.Empty
				If PBScenario = "NA" or PBScenario = String.Empty
					FilterString = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#Target:V#Periodic:O#Top:I#Top:F#L2_Ctrl_Intermediate:U1#{appn.Member.Name}:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"	
				Else
					FilterString = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#Target:V#Periodic:O#Top:I#Top:F#L2_Ctrl_Intermediate:U1#{appn.Member.Name}:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								     Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{PBScenario}:T#{Time}:E#[{wfInfoDetails("CMDName")}]:A#BO1:V#Periodic:O#Top:I#Top:F#Tot_Position:U1#{appn.Member.Name}:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
					                 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{PBScenario}:T#{Time}:E#[{wfInfoDetails("CMDName")}]:A#BO5:V#Periodic:O#Top:I#Top:F#Tot_Position:U1#{appn.Member.Name}:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
				End If
				globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")
		
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
					msb.UD1 = appn.Member.Name
					msb.UD2 = vbNullString   
						If RowType = "RDTE" Then
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
						Else If RowType <> "PROC"
							Dim U3mbr_Expansion As String = ".Ancestors.Where(Text1 = SAG)"
							Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
							Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, $"U3#{msb.UD3}{U3mbr_Expansion}",True)
							msb.UD3 = lsAncestorList(0).Member.Name	
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
						Else
							msb.UD3 = msb.UD3.Substring(0,msb.UD3.Length-3)
							globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3.Substring(0,msb.UD3.Length-3))
						End If
					msb.UD4 = vbNullString
					msb.UD5 = vbNullString
					msb.UD6 = vbNullString
					msb.UD7 = vbNullString
					msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U3#{msb.UD3}")
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
	
	#Region "Get Review Received TGT"
		Public Function GetReviewRecTGT() As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim RptType As String = args.NameValuePairs("RptType")  'TGT Received/TGT Received by Appn
			Dim RowType As String = args.NameValuePairs.XFGetValue("RowType","NA") 
			Dim Scenario As String = args.NameValuePairs("Scenario")
			Dim Time As String = args.NameValuePairs("Time")
			Dim Dist_Acct As String = "Target"
			Dim Dist_Flow As String = "L2_Ctrl_Intermediate"
			Dim mbr_Expansion As String = String.Empty
			Dim u3Mbrs As String = "U3#Top:"
			If RptType = "TGT_Rec" Then
				mbr_Expansion = ".Children"
				u3Mbrs = String.Empty
			End If
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
			Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{Entity}{mbr_Expansion}", True)

			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim FilterString As String
			
			For Each Ent As MemberInfo In entList
				Dim EntBase = Not BRApi.Finance.Members.HasChildren(si, entDimPk, Ent.Member.MemberId)
				Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Ent.Member.Name)
	
				Select Case EntityLevel
				    Case "L3"
						If EntBase = True
				        	Dist_Flow = "L3_Dist_Final"
						Else
							Dist_Flow = "L3_Ctrl_Intermediate"
						End If
				    Case "L4"
						If EntBase = True
				        	Dist_Flow = "L4_Dist_Final"
						Else
							Dist_Flow = "L4_Ctrl_Intermediate"
						End If

						If EntBase = True
				        	Dist_Flow = "L5_Dist_Final"
						Else
							Dist_Flow = "L5_Ctrl_Intermediate"
						End If
						
				End Select
			

				If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
				FilterString = String.Empty
	
				FilterString = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Dist_Acct}:V#Periodic:O#Top:I#Top:F#{Dist_Flow}:U2#Top:{u3Mbrs}U4#Top:U5#Top:U6#Top:U7#None:U8#None"
	
				globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")
		
				GetDataBuffer(si,globals,api,args)
		
				If Not globals.GetObject("Results") Is Nothing
		
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
				For Each msb In results.Keys
					msb.Scenario = vbNullString
					msb.Entity =  Entity		   
					msb.Account = vbNullString
					msb.Origin = vbNullString
					msb.IC = vbNullString
					msb.Flow = vbNullString
					msb.UD2 = vbNullString   
					If RptType = "TGT_Rec_Appn" Then
						msb.UD3 = vbNullString	
					Else
						If globals.GetStringValue($"RptU3_{msb.UD3}","NA")
							If RowType = "RDTE" Then
								globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
							Else If RowType <> "PROC"
								Dim U3mbr_Expansion As String = ".Ancestors.Where(MemberDim = U3_SAG)"
								Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")
								Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, $"U3#{msb.UD3}{U3mbr_Expansion}",True)
								msb.UD3 = lsAncestorList(0).Member.Name	
								globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3)
							Else
								msb.UD3 = msb.UD3.Substring(0,msb.UD3.Length-3)
								globals.SetStringValue($"RptU3_{msb.UD3}",msb.UD3.Substring(0,msb.UD3.Length-3))
							End If
						Else
							msb.UD3 = globals.GetStringValue($"RptU3_{msb.UD3}","NA")
						End If
					End If
					msb.UD4 = vbNullString
					msb.UD5 = vbNullString
					msb.UD6 = vbNullString
					msb.UD7 = vbNullString
					msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						If RptType = "TGT_Rec_Appn" Then
							toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1}")
						Else
							toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U3#{msb.UD3}")
						End If
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

	#Region "Get Modify TGT DIST/WH"
		Public Function GetModify_DistWH() As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs.XFGetValue("Entity","NA")
			If Entity = "NA" Or Entity = String.Empty Then
				BRAPI.ErrorLog.LogMessage(si,$"Hit: {Entity}")
				Return "U6#One"
			End If
			Dim Appn As String = args.NameValuePairs.XFGetValue("APPN","NA")
			Dim Mdep As String = args.NameValuePairs.XFGetValue("MDEP","NA")
			Dim SagApe As String = args.NameValuePairs.XFGetValue("SAGAPE","NA")
			Dim DollarType As String = args.NameValuePairs.XFGetValue("DollarType","NA")
			Dim Scenario As String = args.NameValuePairs.XFGetValue("Scenario","NA")
			Dim Time As String = args.NameValuePairs.XFGetValue("Time","NA")
			Dim Acct As String = "TGT_WH"
			Dim Flow As String = "L2_Control"
			Dim mbr_Expansion As String = ".ChildrenInclusive"

			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")		
			Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{Entity}{mbr_Expansion}", True)

			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim commDims As String
			Dim FilterString As String = $",[U1#{Appn}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
										   ,[U2#{Mdep}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
										   ,[U3#{SagApe}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
										   ,[U4#{DollarType}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"

			For Each Ent As MemberInfo In entList
				Dim EntBase = Not BRApi.Finance.Members.HasChildren(si, entDimPk, Ent.Member.MemberId)
				Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,Ent.Member.Name)
				If Entity <> Ent.Member.Name Then
					Acct = "Target"
				End If
	
				Select Case EntityLevel
				    Case "L2"
						If Acct = "TGT_WH"
				        	Flow = "L2_Dist_Final"
						End If
				    Case "L3"
						If Acct = "TGT_WH"
				        	Flow = "L3_Dist_Final"
						ElseIf EntBase = True
				        	Flow = "L3_Dist_Final"
						Else
							Flow = "L3_Ctrl_Intermediate"
						End If
				    Case "L4"
						If Acct = "TGT_WH"
				        	Flow = "L4_Dist_Final"
						ElseIf EntBase = True
				        	Flow = "L4_Dist_Final"
						Else
							Flow = "L4_Ctrl_Intermediate"
						End If
				    Case "L5"
				        Flow = "L5_Dist_Final"
						
				End Select
				
	
				If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
				commDims = String.Empty
	
				commDims = $"Cb#{wfInfoDetails("CMDName")}:C#Local:S#{Scenario}:T#{Time}:E#[{Ent.Member.Name}]:A#{Acct}:V#Periodic:O#Top:I#Top:F#{Flow}:U5#Top:U6#CostCat:U7#None:U8#None"
				
				globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}){FilterString})")
		brapi.ErrorLog.LogMessage(si,$"FilterMembers(REMOVENODATA({commDims}){FilterString})")
				GetDataBuffer(si,globals,api,args)
		
				If Not globals.GetObject("Results") Is Nothing
		
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
		
				For Each msb In results.Keys
					brapi.ErrorLog.LogMessage(si,"Hit:" & msb.ud3)
				   msb.Scenario = vbNullString
				   msb.Entity =  Ent.Member.Name	   
				   msb.Account = Acct
				   msb.Origin = vbNullString
				   msb.IC = vbNullString
				   msb.Flow = Flow
				   msb.UD5 = vbNullString
				   msb.UD6 = vbNullString
				   msb.UD7 = vbNullString
				   msb.UD8 = vbNullString	   
					If Not toSort.ContainsKey(msb.GetMemberScript)
						toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3},U4#{msb.UD4},A#{msb.Account},E#{msb.entity}")
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
	
	#Region "Get TopLine Ctrls"
		Public Function getTopLine_Ctrls() As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim Entity As String = args.NameValuePairs("Entity")
			Dim APPN As String = args.NameValuePairs.XFGetValue("APPN","NA")
			Dim SAGAPE As String = args.NameValuePairs.XFGetValue("SAGAPE","NA") 
			Dim Scenario As String = args.NameValuePairs("Scenario")
			Dim Time As String = args.NameValuePairs("Time")

			Dim toSort As New Dictionary(Of String, String)
			Dim output = ""
			Dim FilterString As String = String.Empty
			Dim Filters As String = String.Empty
			If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"

		    FilterString = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#Target:V#Periodic:O#Top:I#Top:F#L2_Ctrl_Intermediate:U5#Top:U6#CostCat:U7#Top:U8#Top"
			Filters = $",[U1#{APPN}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
					    ,[U2#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
			            ,[U3#{SAGAPE}.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]
			            ,[U4#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=Target,MergeMembersFromReferencedCubes=False)]"
			globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({FilterString}){Filters})")
	BRAPi.ErrorLog.LogMessage(si,$"FilterMembers(REMOVENODATA({FilterString}){Filters})")
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
	
			Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
	
			For Each msb In results.Keys
				BRAPi.errorlog.LogMessage(si,"Hit:" & msb.UD1)
				msb.Scenario = vbNullString
				msb.Entity =  vbNullString		   
				msb.Account = vbNullString
				msb.Origin = vbNullString
				msb.IC = vbNullString
				msb.Flow = vbNullString   
				msb.UD5 = vbNullString
				msb.UD6 = vbNullString
				msb.UD7 = vbNullString
				msb.UD8 = vbNullString	   
				If Not toSort.ContainsKey(msb.GetMemberScript)
					toSort.Add(msb.GetMemberScript, $"U1#{msb.UD1},U3#{msb.UD3}")
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
