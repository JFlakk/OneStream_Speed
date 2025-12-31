# OneStream Business Rules Development Guide

## Project Overview

This is a **OneStream XF business rules codebase** managed via the "Code Utility for OneStream" VS Code extension. OneStream is a corporate performance management (CPM) platform. This repository contains custom C# business rules that extend OneStream's financial planning, reporting, and dashboard capabilities.

**Key Concept:** Code files here are local copies synced with OneStream's internal database. Files are imported/exported via XML packages, not deployed as traditional .NET applications.

## Architecture & Organization

### Workspace Assembly Model
- **Workspaces** are top-level logical containers (e.g., "OS Consultant Tools", "PPBE Planning Process")
- **Maintenance Units** are functional modules within workspaces (e.g., "Dynamic Dashboard Manager", "Finance Model Manager")
- **Assemblies** are compiled C# code groups with dependencies (e.g., `DDM_UI_Assembly`, `FMM_Calc_Assembly`)
- Dependencies: Many assemblies depend on `GBL_UI_Assembly` (global shared functions)

### Folder Structure by Business Rule Type
```
Assemblies/          - Workspace assemblies (.NET code organized by workspace/maintenance unit)
DashboardExtender/   - Dashboard UI and data manipulation logic
DashboardDataSet/    - Data source providers for dashboards
DashboardStringFunction/ - Custom string/utility functions for dashboards
Finance/             - Financial calculation rules
Connector/           - Data source connectors
Parser/              - Data parsers
ConditionalRule/     - Conditional logic for transformations
TransformationRule/  - Data transformation rules
Spreadsheet/         - Custom spreadsheet (XFTV) behaviors
CubeViewExtender/    - Cube view customizations
*EventHandler/       - Event handlers (DataQuality, Transformation, Forms, WCF, etc.)
```

**Critical:** Each folder maps to an XML export type (e.g., `Assemblies/` → `ApplicationWorkspaces.xml`, `Finance/` → `FinanceRules.xml`).

### Namespace Conventions
All business rules use placeholder namespaces replaced during OneStream import:
```csharp
namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.RuleName
```
- `__WsNamespacePrefix`: Replaced with workspace prefix (e.g., "OSConsTools")
- `__WsAssemblyName`: Replaced with assembly name (e.g., "DDM_UI_Assembly")
- **Never hardcode these values** - they're template placeholders

## Standard Code Patterns

### Business Rule Entry Points
Every business rule type has a standard `Main` method signature:

**DashboardExtender:**
```csharp
public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
```

**DashboardDataSet:**
```csharp
public object Main(SessionInfo si, BRGlobals globals, object api, DashboardDataSetArgs args)
{
    switch (args.FunctionType)
    {
        case DashboardDataSetFunctionType.GetDataSet:
            if (args.DataSetName.XFEqualsIgnoreCase("DataSetName")) { /*...*/ }
            break;
    }
}
```

**DashboardStringFunction:**
```csharp
public object Main(SessionInfo si, BRGlobals globals, object api, DashboardStringFunctionArgs args)
{
    if (args.FunctionName.XFEqualsIgnoreCase("FunctionName")) { /*...*/ }
}
```

**Spreadsheet (XFTV):**
```csharp
public object Main(SessionInfo si, BRGlobals globals, object api, SpreadsheetArgs args)
{
    switch (args.FunctionType)
    {
        case SpreadsheetFunctionType.GetTableView: /*...*/ break;
        case SpreadsheetFunctionType.SaveTableView: /*...*/ break;
    }
}
```

### Core OneStream API Usage (BRApi)
```csharp
// Database access
DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si);
DataTable dt = BRApi.Database.ExecuteSql(dbConn, "SELECT * FROM TableName", false);
BRApi.Database.ExecuteNonQuery(conn, trans, CommandType.Text, sql, parameters);

// File system
XFFile file = BRApi.FileSystem.GetFile(si, fileGuid);
byte[] fileBytes = file.FileBytes;

// Session state
BRApi.State.SetSessionState(si, false, ClientModuleType.Unknown, typeName, "", key, si.UserName, "", bytes);
UserState state = BRApi.State.GetSessionState(si, false, ClientModuleType.Unknown, typeName, "", key, si.UserName);

// Logging
BRApi.ErrorLog.LogMessage(si, "Message");
```

### Error Handling Pattern
All business rules wrap logic in try-catch with standardized error handling:
```csharp
try
{
    // Business logic
}
catch (Exception ex)
{
    throw ErrorHandler.LogWrite(si, new XFException(si, ex));
    // OR
    throw new XFException(si, ex);
}
```

### Service Factory Pattern
Assemblies can expose services via `IWsAssemblyServiceFactory`:
```csharp
public class ServiceFactory : IWsAssemblyServiceFactory
{
    public IWsAssemblyServiceBase CreateWsAssemblyServiceInstance(
        SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, 
        WsAssemblyServiceType wsAssemblyServiceType, string itemName)
    {
        return wsAssemblyServiceType switch
        {
            WsAssemblyServiceType.Dashboard => new DB_Svc(),
            WsAssemblyServiceType.DynamicDashboards => new DynDB_Svc(),
            _ => throw new NotImplementedException()
        };
    }
}
```
Services referenced in metadata: `"wsAssemblyService": "AssemblyName.ServiceFactory"`

## Development Workflow

### Prerequisites
- **Code Utility for OneStream** VS Code extension installed
- OneStream DLLs referenced in `obj/Debug/net8.0/ref/dlls/` (configured via `Directory.Build.props`)
- Connection to OneStream environment for import/export

### Typical Workflow
1. **Import from OneStream:** Use extension to import business rules → generates folder structure + metadata
2. **Edit locally:** Modify `.cs` files using VS Code
3. **Test syntax:** Project targets .NET 8.0 for IntelliSense (OneStream may use different runtime)
4. **Export to OneStream:** Use extension to generate XML packages → import into OneStream environment
5. **Test in OneStream:** Changes take effect after OneStream import

### Build System
- **OmniSharp config:** [omnisharp.json](omnisharp.json) configures C# language server with DLL path
- **No traditional build:** This is not a deployable .NET app - IntelliSense only
- Metadata stored in `.onestream-metadata/metadata.json`

## Key Files & Components

### Configuration
- [Directory.Build.props](Directory.Build.props) - OneStream DLL path configuration
- [omnisharp.json](omnisharp.json) - C# tooling configuration  
- [OneStream_Speed.csproj](OneStream_Speed.csproj) - Project references (for IntelliSense only)
- `.onestream-metadata/metadata.json` - OneStream metadata (workspaces, assemblies, dependencies)

### Major Components
- **Dynamic Dashboard Manager (DDM):** Custom dashboard framework with configuration tables
- **Finance Model Manager (FMM):** Financial modeling and calculation engine
- **Model Dimension Manager (MDM):** Dimension maintenance tools
- **SQL Query Manager (SQM):** Dynamic SQL query execution

### Scripts
- [scripts/rename_workspace_components.py](scripts/rename_workspace_components.py) - Bulk rename dashboard components in XML

## Common Patterns

### Database Tables (Custom Schema)
Custom configuration tables follow naming pattern: `<Prefix>_<Entity>` (e.g., `DDM_Config`, `FMM_Models`)
- Access via `BRApi.Database.CreateApplicationDbConnInfo(si)` for OneStream app database
- Use SQL adapters in `SQL Adapters/` folders for complex CRUD operations

### String Comparisons
Use OneStream extension methods: `.XFEqualsIgnoreCase()` instead of `.ToUpper() ==`

### Common Abbreviations
- **SI:** SessionInfo
- **CSV:** CubeSelectionHelper (selection validation)
- **XFTV:** XF Table View (spreadsheet component)
- **SQA:** SQL Adapter
- **WS:** Workspace
- **BR:** Business Rule

## Important Constraints

1. **No direct file writes** - use `BRApi.FileSystem` for file operations
2. **No hardcoded connection strings** - always use `BRApi.Database.CreateDbConnInfo()`
3. **Session-scoped state only** - use `BRApi.State` for temporary data
4. **Placeholder namespaces** - never replace `__WsNamespacePrefix` or `__WsAssemblyName` manually
5. **Type aliases:** Use OneStream type abbreviations (e.g., `SI` for `SessionInfo`) where established

## Testing & Debugging

- **No local execution:** Code runs only within OneStream platform
- **Logging:** Use `BRApi.ErrorLog.LogMessage(si, message)` for diagnostic output
- **View logs:** Check OneStream System Log in application UI
- **Performance:** Use `System.Diagnostics.Stopwatch` for timing, log elapsed milliseconds

## Dependencies

- OneStream XF Platform APIs (not publicly available - DLLs provided by OneStream installation)
- .NET Standard/Framework types
- `Microsoft.Data.SqlClient` (SQL Server connectivity)
- `OneStreamWorkspacesApi` (workspace service interfaces)

---
*This repository structure is generated and maintained by the Code Utility for OneStream VS Code extension.*
