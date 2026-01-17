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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.GBL_DataBuffer_Helper
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
                    Case "GetDataBufferMembers"
                        Return Me.GetDataBufferMembers()
                End Select
                
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

#Region "GetDataBufferMembers - Parameterized DataBuffer Function"
        ''' <summary>
        ''' Parameterized function to retrieve data buffer members with flexible configuration.
        ''' 
        ''' Parameters (passed via args.NameValuePairs):
        ''' 
        ''' Core Filter Parameters:
        ''' - FilterExpression: The data buffer filter expression (required)
        ''' - Entity: Entity member(s), comma-separated for multiple (optional, can be part of FilterExpression)
        ''' - Scenario: Scenario member (optional, can be part of FilterExpression)
        ''' - Time: Time member(s), comma-separated for multiple (optional, can be part of FilterExpression)
        ''' - Account: Account member (optional, can be part of FilterExpression)
        ''' - Cube: Cube name (optional, defaults to workflow cube)
        ''' 
        ''' Dimension Clear Parameters (set to "True" to clear):
        ''' - ClearScenario: Clear scenario dimension from results
        ''' - ClearEntity: Clear entity dimension from results
        ''' - ClearAccount: Clear account dimension from results
        ''' - ClearFlow: Clear flow dimension from results
        ''' - ClearOrigin: Clear origin dimension from results
        ''' - ClearIC: Clear IC dimension from results
        ''' - ClearUD1: Clear UD1 dimension from results
        ''' - ClearUD2: Clear UD2 dimension from results
        ''' - ClearUD3: Clear UD3 dimension from results
        ''' - ClearUD4: Clear UD4 dimension from results
        ''' - ClearUD5: Clear UD5 dimension from results
        ''' - ClearUD6: Clear UD6 dimension from results
        ''' - ClearUD7: Clear UD7 dimension from results
        ''' - ClearUD8: Clear UD8 dimension from results
        ''' 
        ''' Dimension Set Parameters (set dimension to specific value):
        ''' - SetEntity: Set entity to specific value
        ''' - SetUD1: Set UD1 to specific value
        ''' - SetUD2: Set UD2 to specific value
        ''' - SetUD3: Set UD3 to specific value
        ''' - SetUD4: Set UD4 to specific value
        ''' - SetUD5: Set UD5 to specific value
        ''' - SetUD6: Set UD6 to specific value
        ''' - SetUD7: Set UD7 to specific value
        ''' - SetUD8: Set UD8 to specific value
        ''' 
        ''' Member Transformation Parameters:
        ''' - TransformUD1: Ancestor transformation for UD1 (e.g., "U1_APPN" to get APPN ancestor)
        ''' - TransformUD2: Ancestor transformation for UD2 (e.g., "U2_PEG" to get PEG ancestor)
        ''' - TransformUD3: Ancestor transformation for UD3 (e.g., "U3_SAG" to get SAG ancestor)
        ''' - TransformUD3Filter: Member filter for UD3 (e.g., ".Ancestors.Where(Text1 = SAG)")
        ''' - TransformUD1Filter: Member filter for UD1
        ''' - TransformUD2Filter: Member filter for UD2
        ''' 
        ''' Output Parameters:
        ''' - SortBy: Sort expression (e.g., "E#{entity},U3#{UD3},U1#{UD1}")
        ''' - SortOrder: "Ascending" or "Descending" (default: Descending)
        ''' - DefaultOutput: Default output when no results (default: "U5#One")
        ''' - OutputFormat: "MemberScript" (default) or "Custom"
        ''' - CustomOutputFormat: Custom format string (e.g., "U1#{UD1}:U3#{UD3}")
        ''' 
        ''' Returns: Comma-separated list of member scripts or custom formatted output
        ''' </summary>
        Public Function GetDataBufferMembers() As String
            Try
                ' Get filter expression (required)
                Dim filterExpr As String = args.NameValuePairs.XFGetValue("FilterExpression", "")
                If String.IsNullOrWhiteSpace(filterExpr) Then
                    Return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One")
                End If
                
                ' Optional: Build filter from components if not provided directly
                If filterExpr.XFEqualsIgnoreCase("BUILD") Then
                    filterExpr = BuildFilterFromComponents()
                End If
                
                ' Set filter and execute GetCVDataBuffer
                globals.SetStringValue("Filter", filterExpr)
                GetDataBuffer(si, globals, api, args)
                
                ' Check if results exist
                If globals.GetObject("Results") Is Nothing Then
                    Return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One")
                End If
                
                Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")
                If results.Count = 0 Then
                    Return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One")
                End If
                
                ' Process results
                Dim toSort As New Dictionary(Of String, String)
                
                For Each msb In results.Keys
                    ' Clear dimensions as specified
                    ApplyDimensionClearing(msb)
                    
                    ' Set dimensions to specific values
                    ApplyDimensionSetting(msb)
                    
                    ' Apply member transformations
                    ApplyMemberTransformations(msb)
                    
                    ' Build sort key
                    Dim sortKey As String = BuildSortKey(msb)
                    
                    ' Add to dictionary if not duplicate
                    If Not toSort.ContainsKey(msb.GetMemberScript) Then
                        toSort.Add(msb.GetMemberScript, sortKey)
                    End If
                Next
                
                ' Sort results
                Dim sorted As Dictionary(Of String, String)
                Dim sortOrder As String = args.NameValuePairs.XFGetValue("SortOrder", "Descending")
                
                If sortOrder.XFEqualsIgnoreCase("Ascending") Then
                    sorted = toSort.OrderBy(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
                Else
                    sorted = toSort.OrderByDescending(Function(x) x.Value).ToDictionary(Function(x) x.Key, Function(y) y.Value)
                End If
                
                ' Build output
                Dim output As String = ""
                Dim outputFormat As String = args.NameValuePairs.XFGetValue("OutputFormat", "MemberScript")
                
                For Each item In sorted
                    If outputFormat.XFEqualsIgnoreCase("MemberScript") Then
                        output &= item.Key & ","
                    Else
                        ' Custom format - will be implemented based on needs
                        output &= item.Key & ","
                    End If
                Next
                
                ' Return results or default
                If String.IsNullOrWhiteSpace(output) Then
                    Return args.NameValuePairs.XFGetValue("DefaultOutput", "U5#One")
                End If
                
                Return output
                
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        
        ''' <summary>
        ''' Builds a filter expression from component parameters
        ''' </summary>
        Private Function BuildFilterFromComponents() As String
            Dim sCube As String = args.NameValuePairs.XFGetValue("Cube", "")
            If String.IsNullOrWhiteSpace(sCube) Then
                sCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
            End If
            
            Dim entities As String = args.NameValuePairs.XFGetValue("Entity", "")
            Dim scenario As String = args.NameValuePairs.XFGetValue("Scenario", "")
            Dim times As String = args.NameValuePairs.XFGetValue("Time", "")
            Dim account As String = args.NameValuePairs.XFGetValue("Account", "")
            Dim baseFilter As String = args.NameValuePairs.XFGetValue("BaseFilter", "")
            
            Dim filterList As New List(Of String)
            Dim lEntity As List(Of String) = StringHelper.SplitString(entities, ",")
            Dim lTime As List(Of String) = StringHelper.SplitString(times, ",")
            
            ' Build filter for each entity/time combination
            For Each e As String In lEntity
                For Each tm As String In lTime
                    Dim filterPart As String = baseFilter
                    If String.IsNullOrWhiteSpace(filterPart) Then
                        filterPart = $"Cb#{sCube}:C#Aggregated:S#{scenario}:T#{tm}:E#[{e}]:A#{account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
                    Else
                        ' Replace placeholders
                        filterPart = filterPart.Replace("{Cube}", sCube)
                        filterPart = filterPart.Replace("{Entity}", e)
                        filterPart = filterPart.Replace("{Scenario}", scenario)
                        filterPart = filterPart.Replace("{Time}", tm)
                        filterPart = filterPart.Replace("{Account}", account)
                    End If
                    filterList.Add(filterPart)
                Next
            Next
            
            ' Combine with REMOVENODATA
            Dim combinedFilter As String = String.Join(" + ", filterList)
            Dim additionalFilters As String = args.NameValuePairs.XFGetValue("AdditionalFilters", "")
            
            If Not String.IsNullOrWhiteSpace(additionalFilters) Then
                Return $"FilterMembers(REMOVENODATA({combinedFilter}),{additionalFilters})"
            Else
                Return $"REMOVENODATA({combinedFilter})"
            End If
        End Function
        
        ''' <summary>
        ''' Clears dimensions based on parameters
        ''' </summary>
        Private Sub ApplyDimensionClearing(ByRef msb As MemberScriptBuilder)
            If args.NameValuePairs.XFGetValue("ClearScenario", "").XFEqualsIgnoreCase("True") Then
                msb.Scenario = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearEntity", "").XFEqualsIgnoreCase("True") Then
                msb.Entity = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearAccount", "").XFEqualsIgnoreCase("True") Then
                msb.Account = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearFlow", "").XFEqualsIgnoreCase("True") Then
                msb.Flow = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearOrigin", "").XFEqualsIgnoreCase("True") Then
                msb.Origin = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearIC", "").XFEqualsIgnoreCase("True") Then
                msb.IC = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD1", "").XFEqualsIgnoreCase("True") Then
                msb.UD1 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD2", "").XFEqualsIgnoreCase("True") Then
                msb.UD2 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD3", "").XFEqualsIgnoreCase("True") Then
                msb.UD3 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD4", "").XFEqualsIgnoreCase("True") Then
                msb.UD4 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD5", "").XFEqualsIgnoreCase("True") Then
                msb.UD5 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD6", "").XFEqualsIgnoreCase("True") Then
                msb.UD6 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD7", "").XFEqualsIgnoreCase("True") Then
                msb.UD7 = vbNullString
            End If
            If args.NameValuePairs.XFGetValue("ClearUD8", "").XFEqualsIgnoreCase("True") Then
                msb.UD8 = vbNullString
            End If
        End Sub
        
        ''' <summary>
        ''' Sets dimensions to specific values based on parameters
        ''' </summary>
        Private Sub ApplyDimensionSetting(ByRef msb As MemberScriptBuilder)
            Dim setValue As String
            
            setValue = args.NameValuePairs.XFGetValue("SetEntity", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.Entity = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD1", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD1 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD2", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD2 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD3", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD3 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD4", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD4 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD5", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD5 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD6", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD6 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD7", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD7 = setValue
            End If
            
            setValue = args.NameValuePairs.XFGetValue("SetUD8", "")
            If Not String.IsNullOrWhiteSpace(setValue) Then
                msb.UD8 = setValue
            End If
        End Sub
        
        ''' <summary>
        ''' Applies member transformations (ancestor lookups, etc.)
        ''' </summary>
        Private Sub ApplyMemberTransformations(ByRef msb As MemberScriptBuilder)
            ' Transform UD1
            Dim transformUD1 As String = args.NameValuePairs.XFGetValue("TransformUD1", "")
            If Not String.IsNullOrWhiteSpace(transformUD1) AndAlso Not String.IsNullOrWhiteSpace(msb.UD1) Then
                Dim ud1Filter As String = args.NameValuePairs.XFGetValue("TransformUD1Filter", "")
                If String.IsNullOrWhiteSpace(ud1Filter) Then
                    ud1Filter = $".Ancestors.Where(MemberDim = {transformUD1})"
                End If
                
                Dim objUD1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, transformUD1)
                Dim lsAncestorList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD1DimPK, $"U1#{msb.UD1}{ud1Filter}", True)
                If lsAncestorList IsNot Nothing AndAlso lsAncestorList.Count > 0 Then
                    msb.UD1 = lsAncestorList(0).Member.Name
                End If
            End If
            
            ' Transform UD2
            Dim transformUD2 As String = args.NameValuePairs.XFGetValue("TransformUD2", "")
            If Not String.IsNullOrWhiteSpace(transformUD2) AndAlso Not String.IsNullOrWhiteSpace(msb.UD2) Then
                Dim ud2Filter As String = args.NameValuePairs.XFGetValue("TransformUD2Filter", "")
                If String.IsNullOrWhiteSpace(ud2Filter) Then
                    ud2Filter = $".Ancestors.Where(MemberDim = {transformUD2})"
                End If
                
                Dim objUD2DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, transformUD2)
                Dim lsAncestorList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD2DimPK, $"U2#{msb.UD2}{ud2Filter}", True)
                If lsAncestorList IsNot Nothing AndAlso lsAncestorList.Count > 0 Then
                    msb.UD2 = lsAncestorList(0).Member.Name
                End If
            End If
            
            ' Transform UD3
            Dim transformUD3 As String = args.NameValuePairs.XFGetValue("TransformUD3", "")
            If Not String.IsNullOrWhiteSpace(transformUD3) AndAlso Not String.IsNullOrWhiteSpace(msb.UD3) Then
                Dim ud3Filter As String = args.NameValuePairs.XFGetValue("TransformUD3Filter", "")
                If String.IsNullOrWhiteSpace(ud3Filter) Then
                    ud3Filter = $".Ancestors.Where(MemberDim = {transformUD3})"
                End If
                
                Dim objUD3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, transformUD3)
                Dim lsAncestorList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objUD3DimPK, $"U3#{msb.UD3}{ud3Filter}", True)
                If lsAncestorList IsNot Nothing AndAlso lsAncestorList.Count > 0 Then
                    msb.UD3 = lsAncestorList(0).Member.Name
                End If
            End If
        End Sub
        
        ''' <summary>
        ''' Builds sort key for ordering results
        ''' </summary>
        Private Function BuildSortKey(msb As MemberScriptBuilder) As String
            Dim sortBy As String = args.NameValuePairs.XFGetValue("SortBy", "")
            
            If String.IsNullOrWhiteSpace(sortBy) Then
                ' Default sort key
                Return $"E#{msb.Entity},U1#{msb.UD1},U2#{msb.UD2},U3#{msb.UD3}"
            End If
            
            ' Replace placeholders in sort expression
            Dim sortKey As String = sortBy
            sortKey = sortKey.Replace("{entity}", msb.Entity)
            sortKey = sortKey.Replace("{scenario}", msb.Scenario)
            sortKey = sortKey.Replace("{account}", msb.Account)
            sortKey = sortKey.Replace("{flow}", msb.Flow)
            sortKey = sortKey.Replace("{origin}", msb.Origin)
            sortKey = sortKey.Replace("{ic}", msb.IC)
            sortKey = sortKey.Replace("{UD1}", msb.UD1)
            sortKey = sortKey.Replace("{UD2}", msb.UD2)
            sortKey = sortKey.Replace("{UD3}", msb.UD3)
            sortKey = sortKey.Replace("{UD4}", msb.UD4)
            sortKey = sortKey.Replace("{UD5}", msb.UD5)
            sortKey = sortKey.Replace("{UD6}", msb.UD6)
            sortKey = sortKey.Replace("{UD7}", msb.UD7)
            sortKey = sortKey.Replace("{UD8}", msb.UD8)
            
            Return sortKey
        End Function
        
#End Region

#Region "Utilities: Get DataBuffer"
        Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object, ByRef args As DashboardStringFunctionArgs)
            Dim Dictionary As New Dictionary(Of String, String)
            BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si, "Global_Buffers", "GetCVDataBuffer", Dictionary, customcalculatetimetype.CurrentPeriod)
        End Sub
#End Region
        
    End Class
End Namespace
