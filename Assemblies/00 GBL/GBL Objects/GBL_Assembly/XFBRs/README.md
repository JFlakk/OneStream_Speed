# GBL DataBuffer Helper - Quick Start Guide

## What is This?

A parameterized global XFBR function that consolidates and simplifies databuffer operations across the OneStream application. Instead of writing 100+ lines of code for each databuffer query, you can now configure the same functionality with ~25 lines of parameters.

## Files in This Folder

### VB.NET Implementation
- **GBL_DataBuffer_Helper.vb** - Core VB.NET implementation (424 lines)
- **GBL_DataBuffer_Examples.vb** - 4 working VB.NET examples (236 lines)
- **GBL_DataBuffer_Helper_Documentation.md** - Complete VB.NET reference guide

### C# Implementation (New!)
- **GBL_DataBuffer_Helper.cs** - Core C# implementation (511 lines)
- **GBL_DataBuffer_Helper_CS_Documentation.md** - Complete C# reference guide

### Other Files
- **GBL_String_Helper.vb** - String utility functions
- **README.md** - This file

## Quick Example

### VB.NET Example

#### Before (Old Pattern)
```vb
' 125 lines of code with nested loops, manual filter building, 
' dimension manipulation, member transformations, etc.
Public Function Org_Totals_by_SAG() As Object
    ' ... 125 lines of repetitive code ...
End Function
```

#### After (New Pattern)
```vb
' 25 lines of declarative configuration
Public Function Org_Totals_by_SAG() As Object
    Dim paramDict As New Dictionary(Of String, String)
    paramDict.Add("FilterExpression", "BUILD")
    paramDict.Add("Entity", args.NameValuePairs("Entity"))
    paramDict.Add("Scenario", args.NameValuePairs("Scenario"))
    paramDict.Add("Time", args.NameValuePairs("Time"))
    paramDict.Add("Account", "Req_Funding")
    paramDict.Add("ClearScenario", "True")
    paramDict.Add("ClearAccount", "True")
    paramDict.Add("TransformUD3", "U3_All_APE")
    paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)")
    paramDict.Add("SortBy", "E#{entity},U3#{UD3}")
    
    Return BRApi.Dashboards.StringFunctions.GetValue(si, 
        "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
End Function
```

### C# Example

```csharp
// Same pattern in C# - 25 lines of declarative configuration
public string GetOrgTotalsBySAG()
{
    var paramDict = new Dictionary<string, string>
    {
        { "FilterExpression", "BUILD" },
        { "Entity", args.NameValuePairs.XFGetValue("Entity", "") },
        { "Scenario", args.NameValuePairs.XFGetValue("Scenario", "") },
        { "Time", args.NameValuePairs.XFGetValue("Time", "") },
        { "Account", "Req_Funding" },
        { "ClearScenario", "True" },
        { "ClearAccount", "True" },
        { "TransformUD3", "U3_All_APE" },
        { "TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)" },
        { "SortBy", "E#{entity},U3#{UD3}" }
    };
    
    return BRApi.Dashboards.StringFunctions.GetValue(
        si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict);
}
```

## Key Benefits

✅ **71% less code** - Reduces ~3,830 lines to ~1,100 lines
✅ **Single point of maintenance** - Update one place instead of 26+
✅ **No breaking changes** - Can wrap existing functions
✅ **Easier to configure** - Parameters instead of procedural code
✅ **Self-documenting** - Parameter names describe intent
✅ **Tested patterns** - Built on proven OneStream APIs

## Common Use Cases

### 1. Simple Databuffer Query
```vb
Dim paramDict As New Dictionary(Of String, String)
paramDict.Add("FilterExpression", "REMOVENODATA(Cb#Army:C#Aggregated:S#Baseline:T#2026:E#[A97AA]:A#REQ_Funding:V#Periodic:O#Top:I#Top:F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None)")
paramDict.Add("ClearScenario", "True")
paramDict.Add("ClearEntity", "True")
Return BRApi.Dashboards.StringFunctions.GetValue(si, "GBL_DataBuffer_Helper", "GetDataBufferMembers", paramDict)
```

### 2. With Member Transformation (Get SAG Ancestors)
```vb
paramDict.Add("TransformUD3", "U3_All_APE")
paramDict.Add("TransformUD3Filter", ".Ancestors.Where(MemberDim = U3_SAG)")
```

### 3. Auto-Build Filter from Components
```vb
paramDict.Add("FilterExpression", "BUILD")
paramDict.Add("Entity", "A97AA,A97AB,A97AC")
paramDict.Add("Scenario", "Baseline")
paramDict.Add("Time", "2026,2027,2028")
paramDict.Add("Account", "REQ_Funding")
```

### 4. Custom Sorting
```vb
paramDict.Add("SortBy", "U1#{UD1},U3#{UD3},U2#{UD2}")
paramDict.Add("SortOrder", "Ascending")
```

## Parameter Categories

### Core Parameters
- `FilterExpression` - The databuffer filter (required)
- `Entity`, `Scenario`, `Time`, `Account` - For BUILD mode
- `Cube` - Defaults to workflow cube

### Dimension Operations
- `Clear[Dimension]` - Set to "True" to clear (e.g., `ClearScenario`)
- `Set[Dimension]` - Set to specific value (e.g., `SetEntity`)

### Member Transformations
- `TransformUD1/UD2/UD3` - Dimension for ancestor lookup
- `TransformUD1/UD2/UD3Filter` - Custom member filter

### Output Control
- `SortBy` - Sort expression with placeholders
- `SortOrder` - "Ascending" or "Descending"
- `DefaultOutput` - Return value when no results

## Getting Started

1. **Choose Your Language**: Use either VB.NET or C# version (both are functionally equivalent)
2. **Review Examples**: Start with `GBL_DataBuffer_Examples.vb` for working VB.NET code
3. **Check Documentation**: 
   - VB.NET: `GBL_DataBuffer_Helper_Documentation.md`
   - C#: `GBL_DataBuffer_Helper_CS_Documentation.md`
4. **Migrate Gradually**: Replace function internals while keeping signatures
5. **Test Thoroughly**: Compare outputs with existing functions

## Language Comparison

Both VB.NET and C# versions provide identical functionality:

| Feature | VB.NET | C# |
|---------|--------|-----|
| Lines of Code | 424 | 511 |
| Functionality | ✅ Complete | ✅ Complete |
| Documentation | ✅ Yes | ✅ Yes |
| Examples | ✅ Yes (VB.NET) | ✅ Yes (in docs) |
| Performance | Same | Same |

**Choose based on**:
- Team language preference
- Existing codebase language
- Developer expertise

## Documentation

For complete parameter reference, usage patterns, and migration guide:
- **VB.NET Documentation**: `GBL_DataBuffer_Helper_Documentation.md` (in this folder)
- **C# Documentation**: `GBL_DataBuffer_Helper_CS_Documentation.md` (in this folder)
- **Implementation Summary**: `DATABUFFER_CS_IMPLEMENTATION_SUMMARY.md` (in repo root)
- **VB.NET Examples**: `GBL_DataBuffer_Examples.vb` (in this folder)

## Support

Questions? Issues? Enhancements?
- Review the full documentation
- Check the examples
- Consult the OneStream development team
- Update docs as the function evolves

## Version

- **Created**: January 2026
- **Location**: `00 GBL/GBL Objects/GBL_Assembly/XFBRs/`
- **Function**: `GBL_DataBuffer_Helper.GetDataBufferMembers`

---

**Remember**: This function doesn't replace existing code automatically. It provides a better way to write new code and a path to refactor existing code gradually.
