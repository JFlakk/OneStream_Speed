# GBL_DataBuffer_Helper.cs - C# Parameterized DataBuffer XFBR

## Overview
The `GBL_DataBuffer_Helper.cs` is a C# implementation of a global XFBR function that provides a highly parameterized way to work with databuffers in OneStream. This C# version is equivalent to the VB.NET `GBL_DataBuffer_Helper.vb` and consolidates common patterns found across multiple assembly-specific `_DataBuffers` XFBRs (CMD_PGM, cPROBE, CMD_TGT, CMD_SPLN, HQ_SPLN) into a single, reusable, and easily configurable function.

## Why C# Version?

While the VB.NET version (`GBL_DataBuffer_Helper.vb`) works perfectly fine, the C# version offers:

1. **Modern Language Features**: Leverage C# language features like LINQ, string interpolation, and more concise syntax
2. **Team Preference**: Some development teams prefer working in C# over VB.NET
3. **Integration**: Better integration with C#-based assemblies and custom code
4. **Consistency**: Maintains consistency if your organization standardizes on C#
5. **Performance**: Potentially better performance characteristics in certain scenarios
6. **Tooling**: Better IDE support and tooling in many development environments

Both versions are functionally equivalent and can be used interchangeably.

## Problem Statement
Previously, each assembly had its own `_DataBuffers.vb` file with multiple similar functions that:
- Built filter strings with repetitive code
- Called `GetCVDataBuffer` through the Global_Buffers business rule
- Manipulated MemberScriptBuilder objects in similar ways
- Had similar patterns for clearing dimensions, transforming members, and sorting results

This led to:
- Code duplication across assemblies (3000+ lines total across 5 assemblies)
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

### Basic Usage (C#)

```csharp
// Call from another XFBR or Dashboard String Function
var paramDict = new Dictionary<string, string>();
paramDict.Add("FilterExpression", "REMOVENODATA(Cb#Army:C#Aggregated:S#Baseline:T#2026:E#[A97AA]:A#REQ_Funding:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None)");
paramDict.Add("ClearScenario", "True");
paramDict.Add("ClearEntity", "True");
paramDict.Add("ClearAccount", "True");
paramDict.Add("TransformUD3", "U3_All_APE");
paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)");
paramDict.Add("SortBy", "U3#{UD3},U1#{UD1}");
paramDict.Add("SortOrder", "Descending");

string result = BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict);
```

### Basic Usage (VB.NET) - For Comparison

```vb
' Call from VB.NET code
Dim paramDict As New Dictionary(Of String, String)
paramDict.Add("FilterExpression", "REMOVENODATA(...)")
paramDict.Add("ClearScenario", "True")
' ... additional parameters ...

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

## C# Code Examples

### Example 1: Simple Databuffer Query (C#)

```csharp
public string GetSimpleDataBuffer()
{
    string sCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName;
    string entity = args.NameValuePairs.XFGetValue("Entity", "A97AA");
    string scenario = args.NameValuePairs.XFGetValue("Scenario", "Baseline");
    string time = args.NameValuePairs.XFGetValue("Time", "2026");
    
    // Build filter directly
    string filterExpr = $"REMOVENODATA(Cb#{sCube}:C#Aggregated:S#{scenario}:T#{time}:E#[{entity}]:A#REQ_Funding:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None)";
    
    var paramDict = new Dictionary<string, string>
    {
        { "FilterExpression", filterExpr },
        { "ClearScenario", "True" },
        { "ClearEntity", "True" },
        { "ClearAccount", "True" },
        { "SortBy", "U1#{UD1},U3#{UD3}" }
    };
    
    return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict);
}
```

### Example 2: Using BUILD Mode (C#)

```csharp
public string GetOrgTotalsBySAG()
{
    string entity = args.NameValuePairs.XFGetValue("Entity", "");
    string scenario = args.NameValuePairs.XFGetValue("Scenario", "");
    string time = args.NameValuePairs.XFGetValue("Time", "");
    
    if (string.IsNullOrWhiteSpace(entity)) return "E#None:U1#None:U3#None";
    
    // Build parameter dictionary
    var paramDict = new Dictionary<string, string>
    {
        // Use BUILD mode to construct filter from components
        { "FilterExpression", "BUILD" },
        { "Entity", entity },
        { "Scenario", scenario },
        { "Time", time },
        { "Account", "Req_Funding" },
        { "BaseFilter", "Cb#{Cube}:C#Aggregated:S#{Scenario}:T#{Time}:E#[{Entity}]:A#{Account}:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None" },
        
        // Clear dimensions we don't need
        { "ClearScenario", "True" },
        { "ClearAccount", "True" },
        { "ClearOrigin", "True" },
        { "ClearIC", "True" },
        { "ClearFlow", "True" },
        { "ClearUD1", "True" },
        { "ClearUD2", "True" },
        { "ClearUD4", "True" },
        { "ClearUD5", "True" },
        { "ClearUD6", "True" },
        { "ClearUD7", "True" },
        { "ClearUD8", "True" },
        
        // Set entity to first entity in list
        { "SetEntity", entity.Split(',')[0] },
        
        // Transform UD3 to get SAG ancestor
        { "TransformUD3", "U3_All_APE" },
        { "TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)" },
        
        // Configure sorting
        { "SortBy", "E#{entity},U3#{UD3}" },
        { "SortOrder", "Descending" },
        { "DefaultOutput", "U5#One" }
    };
    
    return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict);
}
```

### Example 3: Complex Filter with Transformations (C#)

```csharp
public string GetComplexDataBuffer()
{
    string entity = args.NameValuePairs.XFGetValue("Entity", "");
    string appn = args.NameValuePairs.XFGetValue("APPN", "OMA");
    
    if (string.IsNullOrWhiteSpace(entity)) return "E#None:U1#None:U3#None";
    
    // Build complex filter with FilterMembers
    string filterExpr = $"FilterMembers(REMOVENODATA(Cb#Army:C#Aggregated:S#Baseline:T#2026:E#[{entity}]:A#Obligations:V#Periodic:O#Top:I#Top:F#Baseline:U4#Top:U6#Top),[U1#{appn}.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],[U2#PEG.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],[U3#APPN.Base.Options(Cube=Army,ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)])";
    
    var paramDict = new Dictionary<string, string>
    {
        { "FilterExpression", filterExpr },
        
        // Clear most dimensions
        { "ClearScenario", "True" },
        { "ClearEntity", "True" },
        { "ClearAccount", "True" },
        { "ClearOrigin", "True" },
        { "ClearIC", "True" },
        { "ClearFlow", "True" },
        { "ClearUD4", "True" },
        { "ClearUD6", "True" },
        
        // Transform UD1 to APPN ancestor
        { "TransformUD1", "U1_APPN" },
        { "TransformUD1Filter", ".Ancestors.Where(MemberDim = 'U1_APPN')" },
        
        // Transform UD3 using custom filter (Text1 property)
        { "TransformUD3", "U3_All_APE" },
        { "TransformUD3Filter", ".Ancestors.Where(Text1 = SAG)" },
        
        { "SortBy", "U1#{UD1},U2#{UD2},U3#{UD3}" },
        { "SortOrder", "Ascending" },
        { "DefaultOutput", "U5#One" }
    };
    
    return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict);
}
```

## Benefits

1. **Code Reuse**: Single implementation replaces dozens of similar functions across multiple assemblies
2. **Easier Configuration**: New databuffer queries can be created by passing parameters instead of writing new functions
3. **Maintainability**: Updates to common logic only need to be made in one place
4. **Consistency**: All databuffer operations follow the same patterns and conventions
5. **Flexibility**: Supports a wide range of use cases through comprehensive parameterization
6. **Reduced Errors**: Less code duplication means fewer opportunities for copy-paste errors
7. **Language Choice**: C# version allows teams to work in their preferred language

## C# vs VB.NET Comparison

### Code Size Reduction Example

**Original VB.NET Code** (CMD_PGM_DataBuffers.Org_Totals_by_SAG): ~125 lines
**New C# Parameterized Call**: ~30 lines
**Reduction**: ~75% fewer lines of code

### Syntax Differences

| Feature | VB.NET | C# |
|---------|--------|-----|
| Dictionary Creation | `New Dictionary(Of String, String)` | `new Dictionary<string, string>` |
| String Interpolation | `$"Value: {variable}"` | `$"Value: {variable}"` |
| Null Check | `String.IsNullOrWhiteSpace(value)` | `string.IsNullOrWhiteSpace(value)` |
| Case Sensitivity | Case-insensitive by default | Case-sensitive |
| Line Continuation | `_` (underscore) | Not needed (implicit) |
| Property Access | `msb.UD1` | `msb.UD1` |
| Array Indexing | `array(0)` | `array[0]` |

## Migration Path

Existing `_DataBuffers` functions can be:
1. **Kept as-is**: No breaking changes to existing code
2. **Refactored gradually**: Replace function internals with calls to `GBL_DataBuffer_Helper` (C# or VB) while maintaining the same interface
3. **Simplified**: Complex functions can often be reduced to a single parameterized call
4. **Language Transition**: Use C# version to transition from VB.NET to C# gradually

## Technical Notes

- The C# version uses the existing `Global_Buffers.GetCVDataBuffer` business rule
- All member transformations use standard OneStream BRApi methods
- The function follows OneStream namespace conventions with placeholder `__WsNamespacePrefix` and `__WsAssemblyName`
- Error handling follows the pattern established in other OneStream business rules
- Both C# and VB.NET versions can be used simultaneously in the same environment
- The function is thread-safe and can be called concurrently

## Performance Considerations

- Filter expressions are executed once per call
- Member transformations involve database lookups and should be used judiciously
- Consider caching results if the same databuffer query is called multiple times
- Use `FilterMembers` in the filter expression for better performance when filtering large result sets

## Common Patterns Consolidated

The C# helper consolidates these patterns from existing databuffer files:

1. **CMD_PGM_DataBuffers.vb** (408 lines):
   - Org_Totals_by_SAG
   - MDEP_Summary
   - Cert_Summary_Report
   
2. **cPROBE_DataBuffers.vb** (281 lines):
   - Get_cPROBE_CivAuth_by_DataElements
   - Get_cPROBE_by_SAG
   
3. **CMD_TGT_DataBuffers.vb** (1077 lines):
   - Multiple target-specific databuffer functions
   
4. **CMD_SPLN_DataBuffers.vb** (1111 lines):
   - SPLN-specific databuffer operations
   
5. **HQ_SPLN_DataBuffers.vb** (332 lines):
   - HQ SPLN summary functions

Total lines consolidated: **~3200 lines** → Single reusable function

## Support and Extensibility

The C# function can be extended to support:
- Additional transformation types
- Custom output formatting
- Advanced filtering capabilities
- Performance optimizations for specific use cases
- Integration with custom C# assemblies

## File Location

**C# Version**: `Assemblies/00 GBL/GBL Objects/GBL_Assembly/XFBRs/GBL_DataBuffer_Helper.cs`
**VB.NET Version**: `Assemblies/00 GBL/GBL Objects/GBL_Assembly/XFBRs/GBL_DataBuffer_Helper.vb`
**Examples (VB)**: `Assemblies/00 GBL/GBL Objects/GBL_Assembly/XFBRs/GBL_DataBuffer_Examples.vb`

Both versions are functionally equivalent and can be used based on your team's preference.

## Questions or Enhancements

For questions or enhancement requests, consult the OneStream development team or refer to the existing VB.NET implementation for additional examples and patterns.
