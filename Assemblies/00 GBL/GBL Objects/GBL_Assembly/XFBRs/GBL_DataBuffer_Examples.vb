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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.GBL_DataBuffer_Examples
    ''' <summary>
    ''' Example implementations showing how to use GBL_DataBuffer_Helper to replace
    ''' existing databuffer code patterns with parameterized calls.
    ''' 
    ''' These examples demonstrate migration from the old pattern to the new pattern.
    ''' </summary>
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
                    Case "Example_OrgTotalsBySAG"
                        Return Me.Example_OrgTotalsBySAG()
                    Case "Example_MDEPSummary"
                        Return Me.Example_MDEPSummary()
                    Case "Example_SimpleFilter"
                        Return Me.Example_SimpleFilter()
                    Case "Example_WithTransformations"
                        Return Me.Example_WithTransformations()
                End Select
                
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

#Region "Example 1: Org Totals By SAG (CMD_PGM Pattern)"
        ''' <summary>
        ''' Example showing how to replace CMD_PGM_DataBuffers.Org_Totals_by_SAG
        ''' 
        ''' Original function: ~125 lines of code
        ''' New function: ~25 lines of code (80% reduction)
        ''' </summary>
        Public Function Example_OrgTotalsBySAG() As String
            ' Get input parameters (same as original)
            Dim Entity As String = args.NameValuePairs.XFGetValue("Entity", "")
            Dim Scenario As String = args.NameValuePairs.XFGetValue("Scenario", "")
            Dim Time As String = args.NameValuePairs.XFGetValue("Time", "")
            Dim Account As String = "Req_Funding"
            
            If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
            
            ' Build parameter dictionary
            Dim paramDict As New Dictionary(Of String, String)
            
            ' Use BUILD mode to construct filter from components
            paramDict.Add("FilterExpression", "BUILD")
            paramDict.Add("Entity", Entity)
            paramDict.Add("Scenario", Scenario)
            paramDict.Add("Time", Time)
            paramDict.Add("Account", Account)
            paramDict.Add("BaseFilter", "Cb#{Cube}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None")
            
            ' Clear dimensions we don't need
            paramDict.Add("ClearScenario", "True")
            paramDict.Add("ClearAccount", "True")
            paramDict.Add("ClearOrigin", "True")
            paramDict.Add("ClearIC", "True")
            paramDict.Add("ClearFlow", "True")
            paramDict.Add("ClearUD1", "True")
            paramDict.Add("ClearUD2", "True")
            paramDict.Add("ClearUD4", "True")
            paramDict.Add("ClearUD5", "True")
            paramDict.Add("ClearUD6", "True")
            paramDict.Add("ClearUD7", "True")
            paramDict.Add("ClearUD8", "True")
            
            ' Set entity to first entity in list
            Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
            paramDict.Add("SetEntity", lEntity(0))
            
            ' Transform UD3 to get SAG ancestor
            paramDict.Add("TransformUD3", "U3_All_APE")
            paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)")
            
            ' Configure sorting
            paramDict.Add("SortBy", "E#{entity},U3#{UD3}")
            paramDict.Add("SortOrder", "Descending")
            paramDict.Add("DefaultOutput", "U5#One")
            
            ' Call the parameterized function
            Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
        End Function
#End Region

#Region "Example 2: MDEP Summary (CMD_PGM Pattern)"
        ''' <summary>
        ''' Example showing how to replace CMD_PGM_DataBuffers.MDEP_Summary
        ''' 
        ''' Demonstrates using multiple entities and transforming UD2 instead of UD3
        ''' </summary>
        Public Function Example_MDEPSummary() As String
            Dim Entity As String = args.NameValuePairs.XFGetValue("Entity", "")
            Dim Scenario As String = args.NameValuePairs.XFGetValue("Scenario", "")
            Dim Time As String = args.NameValuePairs.XFGetValue("Time", "")
            Dim Account As String = "REQ_Funding"
            
            If String.IsNullOrWhiteSpace(Entity) Then Return "E#None:U1#None:U3#None"
            
            Dim paramDict As New Dictionary(Of String, String)
            paramDict.Add("FilterExpression", "BUILD")
            paramDict.Add("Entity", Entity)
            paramDict.Add("Scenario", Scenario)
            paramDict.Add("Time", Time)
            paramDict.Add("Account", Account)
            paramDict.Add("BaseFilter", "Cb#{Cube}:C#USD:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None")
            
            ' Clear most dimensions
            paramDict.Add("ClearScenario", "True")
            paramDict.Add("ClearAccount", "True")
            paramDict.Add("ClearOrigin", "True")
            paramDict.Add("ClearIC", "True")
            paramDict.Add("ClearFlow", "True")
            paramDict.Add("ClearUD1", "True")
            paramDict.Add("ClearUD3", "True")
            paramDict.Add("ClearUD4", "True")
            paramDict.Add("ClearUD5", "True")
            paramDict.Add("ClearUD6", "True")
            paramDict.Add("ClearUD7", "True")
            paramDict.Add("ClearUD8", "True")
            
            ' Keep entity from first in list
            Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
            paramDict.Add("SetEntity", lEntity(0))
            
            ' Transform UD2 to get MDEP ancestor
            paramDict.Add("TransformUD2", "U2_MDEP")
            ' Default filter will be used: ".Ancestors.Where(MemberDim = U2_MDEP)"
            
            paramDict.Add("SortBy", "E#{entity},U2#{UD2}")
            paramDict.Add("SortOrder", "Descending")
            paramDict.Add("DefaultOutput", "U5#One")
            
            Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
        End Function
#End Region

#Region "Example 3: Simple Pre-built Filter"
        ''' <summary>
        ''' Example using a pre-built filter expression (no BUILD mode)
        ''' Useful when you have a complex filter already constructed
        ''' </summary>
        Public Function Example_SimpleFilter() As String
            Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            Dim Entity As String = args.NameValuePairs.XFGetValue("Entity", "A97AA")
            Dim Scenario As String = args.NameValuePairs.XFGetValue("Scenario", "Baseline")
            Dim Time As String = args.NameValuePairs.XFGetValue("Time", "2026")
            
            ' Build filter directly
            Dim filterExpr As String = $"REMOVENODATA(Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#REQ_Funding:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None)"
            
            Dim paramDict As New Dictionary(Of String, String)
            paramDict.Add("FilterExpression", filterExpr)
            paramDict.Add("ClearScenario", "True")
            paramDict.Add("ClearEntity", "True")
            paramDict.Add("ClearAccount", "True")
            paramDict.Add("SortBy", "U1#{UD1},U3#{UD3}")
            
            Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
        End Function
#End Region

#Region "Example 4: Multiple Transformations"
        ''' <summary>
        ''' Example showing multiple dimension transformations and FilterMembers usage
        ''' Similar to HQ_SPLN or cPROBE patterns
        ''' </summary>
        Public Function Example_WithTransformations() As String
            Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "")
            Dim APPN As String = args.NameValuePairs.XFGetValue("APPN", "OMA")
            
            If String.IsNullOrWhiteSpace(sEntity) Then Return "E#None:U1#None:U3#None"
            
            ' Build complex filter with FilterMembers
            Dim filterExpr As String = $"FilterMembers(REMOVENODATA(Cb#Army:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#Obligations:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top),[U1#{APPN}.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],[U2#PEG.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],[U3#APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)])"
            
            Dim paramDict As New Dictionary(Of String, String)
            paramDict.Add("FilterExpression", filterExpr)
            
            ' Clear most dimensions
            paramDict.Add("ClearScenario", "True")
            paramDict.Add("ClearEntity", "True")
            paramDict.Add("ClearAccount", "True")
            paramDict.Add("ClearOrigin", "True")
            paramDict.Add("ClearIC", "True")
            paramDict.Add("ClearFlow", "True")
            paramDict.Add("ClearUD4", "True")
            paramDict.Add("ClearUD6", "True")
            
            ' Transform UD1 to APPN ancestor
            paramDict.Add("TransformUD1", "U1_APPN")
            paramDict.Add("TransformUD1Filter", ".Ancestors.Where(MemberDim = 'U1_APPN')")
            
            ' Transform UD3 using custom filter (Text1 property)
            paramDict.Add("TransformUD3", "U3_All_APE")
            paramDict.Add("TransformUD3Filter", ".Ancestors.Where(Text1 = SAG)")
            
            paramDict.Add("SortBy", "U1#{UD1},U2#{UD2},U3#{UD3}")
            paramDict.Add("SortOrder", "Ascending")
            paramDict.Add("DefaultOutput", "U5#One")
            
            Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
        End Function
#End Region

    End Class
End Namespace
