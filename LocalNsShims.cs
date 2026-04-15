// LocalNsShims.cs
// Empty namespace declarations for cross-assembly using statements.
// In OneStream, the platform replaces __WsNamespacePrefix/__WsAssemblyName
// with the actual workspace/assembly names (e.g. OSConsTools/GBL_UI_Assembly).
// These shims allow those using directives to resolve during local IntelliSense checks.
// This file is NOT deployed to OneStream.

namespace Workspace.OSConsTools.GBL_UI_Assembly { }
namespace Workspace.OSConsTools.DDM_ConfigUI_Assembly { }
namespace Workspace.OSConsultantTools.FMM_UI_Assembly { }
