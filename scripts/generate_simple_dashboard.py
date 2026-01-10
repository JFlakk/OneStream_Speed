#!/usr/bin/env python3
"""
OneStream Dashboard XML Generator
Generates basic Dashboard XML structures for OneStream XF

Usage:
    python generate_simple_dashboard.py

This script generates a simple dashboard XML based on user input.
For complex dashboards, use this as a starting point and modify as needed.
"""

import xml.etree.ElementTree as ET
from xml.dom import minidom

def prettify_xml(elem):
    """Return a pretty-printed XML string for the Element."""
    rough_string = ET.tostring(elem, encoding='utf-8')
    reparsed = minidom.parseString(rough_string)
    return reparsed.toprettyxml(indent="    ", encoding="utf-8").decode('utf-8')

def create_parameter(name, param_type, default_value="", display_items="", value_items=""):
    """Create a parameter element."""
    param = ET.Element("parameter")
    param.set("name", name)
    param.set("description", "")
    param.set("userPrompt", "")
    param.set("parameterType", param_type)
    param.set("sortOrder", "0")
    
    # Add child elements
    ET.SubElement(param, "defaultValue").text = default_value
    ET.SubElement(param, "resultFormatStringType").text = "Default"
    ET.SubElement(param, "resultCustomFormatString")
    ET.SubElement(param, "parameterCommandType").text = "Unknown"
    ET.SubElement(param, "sqlQuery")
    ET.SubElement(param, "dbLocation").text = "Application"
    ET.SubElement(param, "externalDBConnName")
    ET.SubElement(param, "methodType").text = "Unknown"
    ET.SubElement(param, "methodQuery")
    ET.SubElement(param, "resultsTableName")
    ET.SubElement(param, "displayMember")
    ET.SubElement(param, "valueMember")
    ET.SubElement(param, "displayItems").text = display_items
    ET.SubElement(param, "valueItems").text = value_items
    ET.SubElement(param, "cubeName")
    ET.SubElement(param, "dimType")
    ET.SubElement(param, "dimName")
    ET.SubElement(param, "memberFilter")
    
    return param

def create_component(name, comp_type, **kwargs):
    """Create a component element."""
    comp = ET.Element("component")
    comp.set("name", name)
    comp.set("description", kwargs.get("description", ""))
    comp.set("xfText", kwargs.get("xfText", ""))
    comp.set("toolTip", kwargs.get("toolTip", ""))
    comp.set("componentType", comp_type)
    comp.set("templateParameterValues", "")
    comp.set("brText1", "")
    comp.set("brText2", "")
    comp.set("boundParameterName", kwargs.get("boundParameter", ""))
    
    if comp_type != "EmbeddedDashboard":
        comp.set("paramValueForButtonClick", "")
        comp.set("applyParamValueToCurrentDbrd", "true")
        comp.set("selectionChangedSaveType", "NoAction")
        comp.set("selectionChangedPovActionType", "NoAction")
        comp.set("selectionChangedTaskType", kwargs.get("taskType", "NoTask"))
        comp.set("selectionChangedUIActionType", kwargs.get("uiActionType", "NoAction"))
        comp.set("selectionChangedNavigationType", "NoAction")
    
    # Add display format if provided
    if "displayFormat" in kwargs:
        ET.SubElement(comp, "displayFormat").text = kwargs["displayFormat"]
    
    # Add dashboards to redraw
    if "dashboardsToRedraw" in kwargs:
        ET.SubElement(comp, "dashboardsToRedraw").text = kwargs["dashboardsToRedraw"]
    
    # Component definition
    comp_def = ET.SubElement(comp, "componentDefinition")
    
    if comp_type == "EmbeddedDashboard":
        comp.set("embeddedDashboardName", kwargs.get("embeddedDashboardName", ""))
        comp.set("templateNameSuffix", "")
        comp.set("templateParameterValues", "")
        comp.set("brText1", "")
        comp.set("brText2", "")
    elif comp_type == "Button":
        button_def = ET.SubElement(comp_def, "XFButtonDefinition")
        button_def.set("ButtonType", kwargs.get("buttonType", "Standard"))
        ET.SubElement(button_def, "ImageFileSourceType").text = "Unknown"
        ET.SubElement(button_def, "ImageUrlOrFullFileName")
        ET.SubElement(button_def, "PageNumber")
        ET.SubElement(button_def, "ExcelSheet")
        ET.SubElement(button_def, "ExcelNamedRange")
    elif comp_type == "CubeView":
        cv_def = ET.SubElement(comp_def, "XFCubeViewDefinition")
        ET.SubElement(cv_def, "CubeViewName").text = kwargs.get("cubeViewName", "")
        ET.SubElement(cv_def, "ShowHeader").text = str(kwargs.get("showHeader", True)).lower()
        ET.SubElement(cv_def, "ShowToggleSizeButton").text = str(kwargs.get("showToggleSize", True)).lower()
    
    ET.SubElement(comp, "adapterMembers")
    
    return comp

def create_dashboard(name, description, dashboard_type, layout_type, is_visible=True):
    """Create a dashboard element."""
    dashboard = ET.Element("dashboard")
    dashboard.set("name", name)
    dashboard.set("description", description)
    dashboard.set("pageCaption", "")
    dashboard.set("dashboardType", dashboard_type)
    dashboard.set("layoutType", layout_type)
    dashboard.set("isInitiallyVisible", str(is_visible).lower())
    dashboard.set("loadDashboardTaskType", "NoTask")
    
    # Dashboard definition
    dash_def = ET.SubElement(dashboard, "DashboardDefinition")
    ET.SubElement(dash_def, "ShowTitle").text = "true"
    ET.SubElement(dash_def, "Notes")
    
    dyn_def = ET.SubElement(dash_def, "DynamicDashboardDefinition")
    ET.SubElement(dyn_def, "ComponentTemplateRepeatItems")
    
    cc_def = ET.SubElement(dash_def, "CustomControlDbrdDefinition")
    cc_def.set("RequiredInputParameters", "")
    
    # Grid layout definition
    if layout_type == "Grid":
        grid_def = ET.SubElement(dash_def, "GridLayoutDefinition")
        ET.SubElement(grid_def, "ColumnDefinitions")
        ET.SubElement(grid_def, "RowDefinitions")
    else:
        grid_def = ET.SubElement(dash_def, "GridLayoutDefinition")
        ET.SubElement(grid_def, "ColumnDefinitions")
        ET.SubElement(grid_def, "RowDefinitions")
    
    # Component members
    comp_members = ET.SubElement(dashboard, "componentMembers")
    
    return dashboard, comp_members

def add_component_member(parent, comp_name):
    """Add a component member to a dashboard."""
    member = ET.SubElement(parent, "componentMember")
    member.set("name", comp_name)
    member.set("left", "")
    member.set("top", "")
    member.set("width", "")
    member.set("height", "")
    member.set("dockPosition", "Left")
    return member

def generate_simple_dashboard():
    """Generate a simple dashboard XML."""
    print("=== OneStream Simple Dashboard Generator ===\n")
    
    # Get basic info
    workspace_name = input("Workspace Name (e.g., '00 GBL'): ") or "00 GBL"
    namespace_prefix = input("Namespace Prefix (e.g., 'GBL'): ") or "GBL"
    maint_unit_name = input("Maintenance Unit Name (e.g., 'Test Dashboard'): ") or "Test Dashboard"
    dashboard_group_name = input("Dashboard Group Name (e.g., 'Test Group'): ") or "Test Group"
    dashboard_name = input("Dashboard Name (e.g., 'Test_Main'): ") or "Test_Main"
    
    # Create root
    root = ET.Element("OneStreamXF")
    root.set("version", "9.0.1.17403")
    
    app_root = ET.SubElement(root, "applicationWorkspacesRoot")
    workspaces = ET.SubElement(app_root, "workspaces")
    
    # Create workspace
    workspace = ET.SubElement(workspaces, "workspace")
    workspace.set("name", workspace_name)
    workspace.set("description", "")
    workspace.set("notes", "")
    workspace.set("accessGroup", "Everyone")
    workspace.set("maintenanceGroup", "Everyone")
    workspace.set("isShareableWorkspace", "false")
    workspace.set("sharedWorkspaceNames", "")
    workspace.set("companyName", "")
    workspace.set("versionNum", "")
    workspace.set("author", "")
    workspace.set("namespacePrefix", namespace_prefix)
    for i in range(1, 9):
        workspace.set(f"importsNamespace{i}", "")
    workspace.set("wsAssemblyService", "")
    for i in range(1, 9):
        workspace.set(f"text{i}", "")
    
    ws_def = ET.SubElement(workspace, "WorkspaceDefinition")
    ET.SubElement(ws_def, "SubstitutionVariableItems")
    
    # Create maintenance units
    maint_units = ET.SubElement(workspace, "maintenanceUnits")
    maint_unit = ET.SubElement(maint_units, "maintenanceUnit")
    maint_unit.set("name", maint_unit_name)
    maint_unit.set("description", "")
    maint_unit.set("uiPlatformType", "WpfOrSilverlight")
    maint_unit.set("accessGroup", "Everyone")
    maint_unit.set("maintenanceGroup", "Everyone")
    maint_unit.set("wsAssemblyService", "")
    
    ET.SubElement(maint_unit, "fileResources")
    ET.SubElement(maint_unit, "stringResources")
    
    # Parameters
    parameters = ET.SubElement(maint_unit, "parameters")
    
    # Add a sample input value parameter
    param1 = create_parameter("IV_Sample_Input", "InputValue", "")
    parameters.append(param1)
    
    # Adapters
    ET.SubElement(maint_unit, "adapters")
    
    # Components
    components = ET.SubElement(maint_unit, "components")
    
    # Add a sample embedded dashboard component
    comp1 = create_component(
        f"Embedded_{dashboard_name}_Content",
        "EmbeddedDashboard",
        embeddedDashboardName=f"{dashboard_name}_Content"
    )
    components.append(comp1)
    
    # Dashboard groups
    dashboard_groups = ET.SubElement(maint_unit, "dashboardGroups")
    dash_group = ET.SubElement(dashboard_groups, "dashboardGroup")
    dash_group.set("name", dashboard_group_name)
    dash_group.set("description", "")
    dash_group.set("accessGroup", "Everyone")
    
    dashboards = ET.SubElement(dash_group, "dashboards")
    
    # Create main dashboard
    dashboard, comp_members = create_dashboard(
        dashboard_name,
        "Main Dashboard",
        "TopLevel",
        "Grid",
        True
    )
    dashboards.append(dashboard)
    
    # Add component to dashboard
    add_component_member(comp_members, f"Embedded_{dashboard_name}_Content")
    
    # Workspace assemblies
    ET.SubElement(maint_unit, "workspaceAssemblies")
    
    # Cube view groups
    ET.SubElement(maint_unit, "cubeViewGroups")
    
    # Data management groups
    ET.SubElement(maint_unit, "dataManagementGroups")
    
    # Generate and save XML
    xml_string = prettify_xml(root)
    
    output_file = f"GeneratedXML/{workspace_name}_{maint_unit_name}.xml"
    output_file = output_file.replace(" ", "_")
    
    print(f"\nGenerating XML file: {output_file}")
    
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(xml_string)
    
    print(f"✓ Dashboard XML generated successfully!")
    print(f"\nNext steps:")
    print(f"1. Review the generated XML at: {output_file}")
    print(f"2. Modify as needed for your specific requirements")
    print(f"3. Import into OneStream using the Code Utility extension")
    print(f"4. Test in OneStream platform")

if __name__ == "__main__":
    try:
        generate_simple_dashboard()
    except KeyboardInterrupt:
        print("\n\nOperation cancelled by user.")
    except Exception as e:
        print(f"\nError: {e}")
        import traceback
        traceback.print_exc()
