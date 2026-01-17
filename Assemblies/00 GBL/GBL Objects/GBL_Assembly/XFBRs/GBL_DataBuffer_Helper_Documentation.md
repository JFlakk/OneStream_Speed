# GBL_DataBuffer_Helper - Parameterized DataBuffer XFBR

## Overview
The `GBL_DataBuffer_Helper` is a new global XFBR function that provides a highly parameterized way to work with databuffers in OneStream. It consolidates the common patterns found across multiple assembly-specific `_DataBuffers` XFBRs (CMD_PGM, cPROBE, CMD_TGT, CMD_SPLN, HQ_SPLN) into a single, reusable, and easily configurable function.

## Problem Statement
Previously, each assembly had its own `_DataBuffers.vb` file with multiple similar functions that:
- Built filter strings with repetitive code
- Called `GetCVDataBuffer` through the Global_Buffers business rule
- Manipulated MemberScriptBuilder objects in similar ways
- Had similar patterns for clearing dimensions, transforming members, and sorting results

This led to:
- Code duplication across assemblies
- Maintenance challenges when updating common logic
- Difficulty in configuring new databuffer queries
- Repetitive boilerplate code

## Solution
The `GBL_DataBuffer_Helper.GetDataBufferMembers` function provides a single, parameterized interface that:
- Accepts flexible configuration via named parameters
- Handles filter expression building or accepts pre-built filters
- Supports dynamic dimension clearing and setting
- Provides member transformation capabilities (ancestor lookups, etc.)
- Includes flexible sorting and output formatting
- Reduces code duplication and improves maintainability

## Function: GetDataBufferMembers

### Basic Usage

```vb
' Call from another XFBR or Dashboard String Function
Dim paramDict As New Dictionary(Of String, String)
paramDict.Add("FilterExpression", "REMOVENODATA(Cb#Army:C#Aggregated:S#Baseline:T#2026:E#[A97AA]:A#REQ_Funding:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None)")
paramDict.Add("ClearScenario", "True")
paramDict.Add("ClearEntity", "True")
paramDict.Add("ClearAccount", "True")
paramDict.Add("TransformUD3", "U3_All_APE")
paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)")
paramDict.Add("SortBy", "U3#{UD3},U1#{UD1}")
paramDict.Add("SortOrder", "Descending")

Dim result As String = BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
```

### Parameters

#### Core Filter Parameters

| Parameter | Type | Description | Required |
|-----------|------|-------------|----------|
| `FilterExpression` | String | The databuffer filter expression. Can be "BUILD" to construct from components, or a complete filter string | Yes |
| `Entity` | String | Entity member(s), comma-separated for multiple. Used when FilterExpression="BUILD" | No |
| `Scenario` | String | Scenario member. Used when FilterExpression="BUILD" | No |
| `Time` | String | Time member(s), comma-separated for multiple. Used when FilterExpression="BUILD" | No |
| `Account` | String | Account member. Used when FilterExpression="BUILD" | No |
| `Cube` | String | Cube name. Defaults to workflow cube if not specified | No |
| `BaseFilter` | String | Base filter template with placeholders like {Entity}, {Scenario}, etc. | No |
| `AdditionalFilters` | String | Additional filters to append (e.g., dimension base filters) | No |

#### Dimension Clear Parameters
Set any of these to "True" to clear the dimension from results:

| Parameter | Description |
|-----------|-------------|
| `ClearScenario` | Clear scenario dimension |
| `ClearEntity` | Clear entity dimension |
| `ClearAccount` | Clear account dimension |
| `ClearFlow` | Clear flow dimension |
| `ClearOrigin` | Clear origin dimension |
| `ClearIC` | Clear IC dimension |
| `ClearUD1` through `ClearUD8` | Clear UD1-UD8 dimensions |

#### Dimension Set Parameters
Set dimensions to specific values:

| Parameter | Description |
|-----------|-------------|
| `SetEntity` | Set entity to specific value |
| `SetUD1` through `SetUD8` | Set UD1-UD8 to specific values |

#### Member Transformation Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `TransformUD1` | Dimension name for UD1 ancestor lookup | "U1_APPN" |
| `TransformUD1Filter` | Custom filter for UD1 transformation | ".Ancestors.Where(MemberDim = U1_APPN)" |
| `TransformUD2` | Dimension name for UD2 ancestor lookup | "U2_MDEP" |
| `TransformUD2Filter` | Custom filter for UD2 transformation | ".Ancestors.Where(MemberDim = U2_PEG)" |
| `TransformUD3` | Dimension name for UD3 ancestor lookup | "U3_All_APE" |
| `TransformUD3Filter` | Custom filter for UD3 transformation | ".Ancestors.Where(Text1 = SAG)" |

#### Output Parameters

| Parameter | Description | Default |
|-----------|-------------|---------|
| `SortBy` | Sort expression with placeholders like {entity}, {UD1}, etc. | "E#{entity},U1#{UD1},U2#{UD2},U3#{UD3}" |
| `SortOrder` | "Ascending" or "Descending" | "Descending" |
| `DefaultOutput` | Output when no results found | "U5#One" |
| `OutputFormat` | "MemberScript" or "Custom" | "MemberScript" |

### Examples

#### Example 1: Org Totals by SAG (from CMD_PGM_DataBuffers)

**Original Code:**
```vb
Public Function Org_Totals_by_SAG() As Object
    Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
    Dim Entity As String = args.NameValuePairs("Entity")
    Dim lEntity As List(Of String) = StringHelper.SplitString(Entity, ",")
    Dim Scenario As String = args.NameValuePairs("Scenario")
    Dim Time As String = args.NameValuePairs("Time")
    Dim lTime As List(Of String) = StringHelper.SplitString(Time, ",")
    Dim Account As String = "Req_Funding"
    
    ' ... lots of repetitive code ...
    
    For Each e As String In lEntity
        FilterString = String.Empty
        For Each tm As String In lTime
            If String.IsNullOrWhiteSpace(FilterString) Then
                FilterString = $"Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
            Else
                FilterString = $"{FilterString} + Cb#{sCube}:C#Aggregated:S#{Scenario}:T#{tm}:E#[{e}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None"
            End If
        Next
        ' ... more code ...
    Next
End Function
```

**New Parameterized Call:**
```vb
Public Function Org_Totals_by_SAG() As Object
    Dim Entity As String = args.NameValuePairs("Entity")
    Dim Scenario As String = args.NameValuePairs("Scenario")
    Dim Time As String = args.NameValuePairs("Time")
    
    Dim paramDict As New Dictionary(Of String, String)
    paramDict.Add("FilterExpression", "BUILD")
    paramDict.Add("Entity", Entity)
    paramDict.Add("Scenario", Scenario)
    paramDict.Add("Time", Time)
    paramDict.Add("Account", "Req_Funding")
    paramDict.Add("BaseFilter", "Cb#{Cube}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None")
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
    paramDict.Add("SetEntity", Entity.Split(","c)(0))  ' Use first entity
    paramDict.Add("TransformUD3", "U3_All_APE")
    paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)")
    paramDict.Add("SortBy", "E#{entity},U3#{UD3}")
    paramDict.Add("SortOrder", "Descending")
    
    Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
End Function
```

#### Example 2: HQ SPLN Adj CMD Summary (from HQ_SPLN_DataBuffers)

**Original Code:**
```vb
Public Function HQ_SPLN_Adj_CMD_Summary() As String
    Dim sEntity As String = args.NameValuePairs("Entity")
    Dim View As String = args.NameValuePairs("View")
    Dim Account As String = args.NameValuePairs("Account")
    Dim APPN As String = args.NameValuePairs("APPN")
    
    commDims = $"cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top +
                 cb#ARMY:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
    filters = $"[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)]"
    ' ... more processing ...
End Function
```

**New Parameterized Call:**
```vb
Public Function HQ_SPLN_Adj_CMD_Summary() As String
    Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
    Dim sEntity As String = args.NameValuePairs("Entity")
    Dim View As String = args.NameValuePairs("View")
    Dim Account As String = args.NameValuePairs("Account")
    Dim APPN As String = args.NameValuePairs("APPN")
    Dim cmd_SPLN_Scenario = stringhelper.ReplaceString(wfInfoDetails("ScenarioName"),"HQ_","CMD_",True)
    
    Dim filterExpr As String = $"FilterMembers(REMOVENODATA(cb#ARMY:C#Aggregated:S#{wfInfoDetails("ScenarioName")}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top + cb#ARMY:C#Aggregated:S#{cmd_SPLN_Scenario}:T#{wfInfoDetails("TimeName")}:E#[{sEntity}]:A#{Account}:V#{View}:O#Top:I#Top:F#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top),[U1#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)],[U3#{APPN}.Base.Options(Cube=ARMY,ScenarioType=Forecast,MergeMembersFromReferencedCubes=False)])"
    
    Dim paramDict As New Dictionary(Of String, String)
    paramDict.Add("FilterExpression", filterExpr)
    paramDict.Add("ClearScenario", "True")
    paramDict.Add("ClearEntity", "True")
    paramDict.Add("ClearAccount", "True")
    paramDict.Add("ClearOrigin", "True")
    paramDict.Add("ClearIC", "True")
    paramDict.Add("ClearFlow", "True")
    paramDict.Add("ClearUD2", "True")
    paramDict.Add("ClearUD4", "True")
    paramDict.Add("ClearUD5", "True")
    paramDict.Add("ClearUD6", "True")
    paramDict.Add("ClearUD7", "True")
    paramDict.Add("ClearUD8", "True")
    paramDict.Add("SortBy", "U1#{UD1}:U3#{UD3}")
    paramDict.Add("SortOrder", "Ascending")
    paramDict.Add("DefaultOutput", "U1#One")
    
    Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
End Function
```

## Benefits

1. **Code Reuse**: Single implementation replaces dozens of similar functions across multiple assemblies
2. **Easier Configuration**: New databuffer queries can be created by passing parameters instead of writing new functions
3. **Maintainability**: Updates to common logic only need to be made in one place
4. **Consistency**: All databuffer operations follow the same patterns and conventions
5. **Flexibility**: Supports a wide range of use cases through comprehensive parameterization
6. **Reduced Errors**: Less code duplication means fewer opportunities for copy-paste errors

## Migration Path

Existing `_DataBuffers` functions can be:
1. **Kept as-is**: No breaking changes to existing code
2. **Refactored gradually**: Replace function internals with calls to `GBL_DataBuffer_Helper` while maintaining the same interface
3. **Simplified**: Complex functions can often be reduced to a single parameterized call

## Technical Notes

- The function uses the existing `Global_Buffers.GetCVDataBuffer` business rule (FMM_Global_Buffers.cs)
- All member transformations use standard OneStream BRApi methods
- The function follows OneStream namespace conventions with placeholder `__WsNamespacePrefix` and `__WsAssemblyName`
- Error handling follows the pattern established in other OneStream business rules

## Support and Extensibility

The function can be extended to support:
- Additional transformation types
- Custom output formatting
- Advanced filtering capabilities
- Performance optimizations for specific use cases

For questions or enhancement requests, consult the OneStream development team.
