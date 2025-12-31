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
                Me.si = si
                Me.globals = globals
                Me.api = api
                Me.args = args

                Select Case args.FunctionName
                    Case "Org_Totals_by_SAG"
                        Return Me.Org_Totals_by_SAG()
                    Case "MDEP_Summary"
                        Return Me.MDEP_Summary(si, globals, api, args)
                    Case "Cert_Summary_Report"
                        Return Me.Cert_Summary_Report(si, globals, api, args)
                    Case "Sum_Review_By_APPN_SAG"
                        Return Me.Sum_Review("APPN_SAG")
                    Case "Sum_Review_By_MDEP"
                        Return Me.Sum_Review("MDEP")
                End Select
                If args.FunctionName.XFEqualsIgnoreCase("EntityText1") Then
                    Return Me.EntityText1(si, globals, api, args)
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
        Public Function Org_Totals_by_SAG() As Object
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim Entity As String = args.NameValuePairs("Entity")
            Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
            Dim Scenario As String = args.NameValuePairs("Scenario")
            Dim Time As String = args.NameValuePairs("Time")
            Dim lTime As List(Of String) = StringHelper.SplitString(Time, ",")
            Dim Account As String = "Req_Funding"

            Dim toSort As New Dictionary(Of String, String)
            Dim output = ""
            Dim FilterString As String
            If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
            For Each e As String In lEntity
                FilterString = String.Empty
                For Each tm As String In lTime
                    If String.IsNullOrWhiteSpace(FilterString) Then
                        FilterString = $"Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    Else
                        FilterString = $"{FilterString} + Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    End If
                Next
                globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")

                GetDataBuffer(si, globals, api, args)

                If Not globals.GetObject("Results") Is Nothing

                    Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

                    Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")

                    For Each msb In results.Keys
                        msb.Scenario = vbNullString
                        msb.Entity = e
                        msb.Account = vbNullString
                        msb.Origin = vbNullString
                        msb.IC = vbNullString
                        msb.Flow = vbNullString
                        msb.UD1 = vbNullString
                        msb.UD2 = vbNullString

                        Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True, , )
                        msb.UD3 = lsAncestorList(0).Member.Name
                        msb.UD4 = vbNullString
                        msb.UD5 = vbNullString
                        msb.UD6 = vbNullString
                        msb.UD7 = vbNullString
                        msb.UD8 = vbNullString
                        If Not toSort.ContainsKey(msb.GetMemberScript) Then
                            toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U3#{msb.UD3}")
                        End If
                    Next
                End If
            Next

            Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

            For Each item In sorted
                output &= item.key & ","
            Next

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
            Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
            Dim Scenario As String = args.NameValuePairs("Scenario")
            Dim Time As String = args.NameValuePairs("Time")
            Dim lTime As List(Of String) = StringHelper.SplitString(Time, ",")
            Dim Account As String = "REQ_Funding"
            Dim toSort As New Dictionary(Of String, String)
            Dim output = ""
            Dim FilterString As String
            If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
            For Each e As String In lEntity
                FilterString = String.Empty
                For Each tm As String In lTime
                    If String.IsNullOrWhiteSpace(FilterString) Then
                        FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    Else
                        FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    End If
                Next
                globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")

                GetDataBuffer(si, globals, api, args)

                If Not globals.GetObject("Results") Is Nothing

                    Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

                    Dim objU2DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U2_MDEP")

                    For Each msb In results.Keys
                        msb.Scenario = vbNullString
                        msb.Entity = e
                        msb.Account = vbNullString
                        msb.Origin = vbNullString
                        msb.IC = vbNullString
                        msb.Flow = vbNullString
                        msb.UD1 = vbNullString


                        Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU2DimPK, "U2#" & msb.UD2, True, , )
                        msb.UD2 = lsAncestorList(0).Member.Name

                        msb.UD3 = vbNullString
                        msb.UD4 = vbNullString
                        msb.UD5 = vbNullString
                        msb.UD6 = vbNullString
                        msb.UD7 = vbNullString
                        msb.UD8 = vbNullString
                        If Not toSort.ContainsKey(msb.GetMemberScript) Then
                            toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U2#{msb.UD2}")
                        End If
                    Next
                End If
            Next

            Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

            For Each item In sorted
                output &= item.key & ","
            Next

            If output = "" Then
                output = "U5#One"
            End If

            Return output

        End Function
#End Region

#Region "Return Cert Summary Report"
        Public Function Cert_Summary_Report(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sCube), 1, 0, 0).Trim
            Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
            Dim Scenario As String = args.NameValuePairs("Scenario")
            Dim Time As String = args.NameValuePairs("Time")
            Dim lTime As List(Of String) = StringHelper.SplitString(Time, ",")
            Dim Account As String = "REQ_Funding"
            Dim toSort As New Dictionary(Of String, String)
            Dim output = ""
            Dim FilterString As String
            If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U2#None:U3#None"
            For Each e As String In lEntity
                FilterString = String.Empty
                For Each tm As String In lTime
                    If String.IsNullOrWhiteSpace(FilterString) Then
                        FilterString = $"Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#L2_Final_PGM:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    Else
                        FilterString = $"{FilterString} + Cb#{sCube}:C#USD:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#L2_Final_PGM:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    End If
                Next
                globals.SetStringValue("Filter", $"REMOVENODATA({FilterString})")

                GetDataBuffer(si, globals, api, args)

                If Not globals.GetObject("Results") Is Nothing

                    Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

                    Dim objU2DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U2_MDEP")

                    Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")

                    For Each msb In results.Keys
                        msb.Scenario = vbNullString
                        msb.Entity = e
                        msb.Account = vbNullString
                        msb.Origin = vbNullString
                        msb.IC = vbNullString
                        msb.Flow = vbNullString



                        Dim lsAncestorListUD2 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU2DimPK, "U2#" & msb.UD2 & ".Ancestors.Where(MemberDim = U2_PEG)", True, , )
                        msb.UD2 = lsAncestorListUD2(0).Member.Name

                        Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True, , )
                        msb.UD3 = lsAncestorList(0).Member.Name
                        msb.UD4 = vbNullString
                        msb.UD5 = vbNullString
                        msb.UD6 = vbNullString
                        msb.UD7 = vbNullString
                        msb.UD8 = vbNullString
                        If Not toSort.ContainsKey(msb.GetMemberScript) Then
                            toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3}")
                        End If
                    Next
                End If
            Next

            Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

            For Each item In sorted
                output &= item.key & ","
            Next

            If output = "" Then
                output = "U5#One"

            End If

            Return output

        End Function
#End Region

#Region "Review Requirements"
        Public Function Sum_Review(ByVal review_Type As String) As String
            Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim entList As String = args.NameValuePairs("BL_CMD_PGM_FundsCenter")
            Dim lEntity As List(Of String) = StringHelper.SplitString(entList, ",")
			Dim tm As String = args.NameValuePairs("Time")
			Dim cmd_SPLN_Scenario As String = $"CMD_SPLN_C{tm.XFConvertToInt-1}"
			Dim prior_CMD_PGM_Scenario As String = $"CMD_PGM_C{tm.XFConvertToInt-1}"
			Dim curr_CMD_PGM_Scenario As String = $"CMD_PGM_C{tm}"
			If tm.XFConvertToInt <= 2026
				cmd_SPLN_Scenario = "Execution"
				prior_CMD_PGM_Scenario = $"CMD_PGM_C{tm}"
			End If

            Dim toSort As New Dictionary(Of String, String)
            Dim output = ""
            Dim FilterString As String
			Dim commDims As String
            If String.IsNullOrWhiteSpace(entList) Then Return "E#None:U1#None:U2#None:U3#None"
            For Each e As String In lEntity
                FilterString = String.Empty
				If review_Type.XFEqualsIgnoreCase("MDEP")
	                commDims = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#Execution:T#{tm.XFConvertToInt-2}:E#[{e}]:A#Obligations:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{tm.XFConvertToInt-1}:E#[{e}]:A#Funded_Obligations:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+1}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+1}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+2}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+2}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+3}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+3}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+4}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					FilterString = $",[U2#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=LongTerm,MergeMembersFromReferencedCubes=False)]"
				Else
	                commDims = $"Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#Execution:T#{tm.XFConvertToInt-2}:E#[{e}]:A#Obligations:V#Periodic:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{tm.XFConvertToInt-1}:E#[{e}]:A#Funded_Obligations:V#Periodic:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+1}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+1}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+2}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+2}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
								 Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{prior_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+3}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+3}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None +
					             Cb#{wfInfoDetails("CMDName")}:C#Aggregated:S#{curr_CMD_PGM_Scenario}:T#{tm.XFConvertToInt+4}:E#[{e}]:A#Req_Funding:V#Periodic:O#Top:I#Top:F#CMD_Requirements:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
					
					FilterString = $",[U1#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=LongTerm,MergeMembersFromReferencedCubes=False)]
								     ,[U3#Top.Base.Options(Cube={wfInfoDetails("CMDName")},ScenarioType=LongTerm,MergeMembersFromReferencedCubes=False)]"
					
				End If
					
                globals.SetStringValue("Filter", $"FilterMembers(REMOVENODATA({commDims}){FilterString})")

                GetDataBuffer(si, globals, api, args)

                If Not globals.GetObject("Results") Is Nothing

                    Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

                    Dim objU1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U1_APPN")
					Dim objU3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_All_APE")

                    For Each msb In results.Keys
                        msb.Scenario = vbNullString
                        msb.Entity = e
                        msb.Account = vbNullString
                        msb.Origin = vbNullString
                        msb.IC = vbNullString
                        msb.Flow = vbNullString
						If review_Type.XFEqualsIgnoreCase("MDEP")
							msb.UD1 = vbNullString
							msb.UD3 = vbNullString
						Else
	                        Dim lsAPPNList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU1DimPK, "U1#" & msb.UD1 & ".Where(MemberDim = U1_APPN)", True, , )
'	                        If lsAPPNList.Count > 0
'								msb.UD1 = lsAPPNList(0).Member.Name
'							End If
	                        Dim lsSAGList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU3DimPK, "U3#" & msb.UD3 & ".Ancestors.Where(MemberDim = U3_SAG)", True, , )
	                        msb.UD3 = lsSAGList(0).Member.Name
							msb.UD2 = vbNullString
						End If
                        msb.UD4 = vbNullString
                        msb.UD5 = vbNullString
                        msb.UD6 = vbNullString
                        msb.UD7 = vbNullString
                        msb.UD8 = vbNullString
                        If Not toSort.ContainsKey(msb.GetMemberScript) Then
                            toSort.Add(msb.GetMemberScript, $"E#{msb.entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3}")
                        End If
                    Next
                End If
            Next

            Dim sorted As Dictionary(Of String, String) = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)

            For Each item In sorted
                output &= item.key & ","
            Next

            If output = "" Then
                output = "U5#One"

            End If

            Return output

        End Function
#End Region

#Region "Utilities: Get DataBuffer"

        Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object, ByRef args As DashboardStringFunctionArgs)

            Dim Dictionary As New Dictionary(Of String, String)

            BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "Global_Buffers", "GetCVDataBuffer", Dictionary, customcalculatetimetype.CurrentPeriod)

        End Sub

#End Region

#Region "Entity Text 1"
        Public Function EntityText1(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object

            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim Entity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sCube), 1, 0, 0).Trim
            Return Entity
        End Function
#End Region
    End Class
End Namespace