#!/usr/bin/env python3
"""
OneStream Dashboard Excel to XML Converter
Converts Excel template to Dashboard XML format

Usage:
    python convert_excel_to_dashboard.py <excel_file_or_directory>
    python convert_excel_to_dashboard.py template.xlsx
    python convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/

This script reads a filled-out dashboard template (Excel or CSV files) and
generates the corresponding OneStream Dashboard XML file.
"""

import csv
import os
import re
import sys
import xml.etree.ElementTree as ET
from xml.dom import minidom

def sanitize_filename_component(name):
    """Sanitize a string to be safe for use in filenames.
    
    Replaces spaces and path separators with underscores,
    and removes characters that are problematic in filenames.
    
    Args:
        name: String to sanitize
        
    Returns:
        Sanitized string safe for filenames
    """
    safe_name = name.replace(" ", "_").replace("/", "_").replace("\\", "_")
    # Remove other problematic characters: < > : " | ? *
    safe_name = re.sub(r'[<>:"|?*]', '_', safe_name)
    return safe_name

def prettify_xml(elem):
    """Return a pretty-printed XML string for the Element."""
    rough_string = ET.tostring(elem, encoding='utf-8')
    reparsed = minidom.parseString(rough_string)
    return reparsed.toprettyxml(indent="    ")

def read_csv_file(filepath):
    """Read a CSV file and return rows as list of dictionaries."""
    if not os.path.exists(filepath):
        return []
    
    with open(filepath, 'r', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        rows = []
        for row in reader:
            # Skip comment rows (starting with #)
            if any(str(v).startswith('#') for v in row.values()):
                continue
            # Skip empty rows
            if not any(str(v).strip() for v in row.values()):
                continue
            rows.append(row)
        return rows

def read_settings_csv(filepath):
    """Read a settings CSV file (key-value pairs)."""
    if not os.path.exists(filepath):
        return {}
    
    settings = {}
    with open(filepath, 'r', encoding='utf-8') as f:
        reader = csv.reader(f)
        next(reader, None)  # Skip header
        for row in reader:
            if len(row) >= 2 and row[0] and not row[0].startswith('#'):
                settings[row[0]] = row[1]
    return settings

def parse_grid_definition(grid_def):
    """Parse grid definition string.
    
    Supports two formats:
    1. With count prefix: "count:Type:Size;Type:Size;..."
       Example: "3:Component:Auto;Component:*;Component:100"
       - First element indicates total count (3 rows/columns)
       - Following elements define Type and Size for each
    
    2. Simple format: "Type:Size;Type:Size;..."
       Example: "Component:*;Component:Auto"
       - Each element directly defines Type and Size
    
    Size values:
    - "*" = Fill remaining space (proportional)
    - "Auto" = Size to content
    - "100" = Fixed pixel size
    
    Returns: List of dicts with 'type' and 'size' keys
    """
    if not grid_def or str(grid_def).strip() == "":
        return []
    
    parts = str(grid_def).split(';')
    result = []
    
    # Check if first part is a count
    first_part = parts[0].split(':')
    if len(first_part) == 3 and first_part[0].isdigit():
        # Format: "count:Type:Size;Type:Size"
        for part in parts:
            type_size = part.split(':')
            if len(type_size) >= 2:
                # Use last two elements as type and size
                result.append({
                    'type': type_size[-2],
                    'size': type_size[-1]
                })
    else:
        # Simple format, each entry is Type:Size
        for part in parts:
            type_size = part.split(':')
            if len(type_size) >= 2:
                result.append({
                    'type': type_size[0],
                    'size': type_size[1]
                })
    
    return result

def create_parameter_element(param_data):
    """Create a parameter XML element."""
    param = ET.Element("parameter")
    param.set("name", param_data.get('Parameter_Name', ''))
    param.set("description", param_data.get('Description', ''))
    param.set("userPrompt", param_data.get('User_Prompt', ''))
    param.set("parameterType", param_data.get('Parameter_Type', 'InputValue'))
    param.set("sortOrder", "0")
    
    ET.SubElement(param, "defaultValue").text = param_data.get('Default_Value', '')
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
    ET.SubElement(param, "displayItems").text = param_data.get('Display_Items', '')
    ET.SubElement(param, "valueItems").text = param_data.get('Value_Items', '')
    ET.SubElement(param, "cubeName")
    ET.SubElement(param, "dimType")
    ET.SubElement(param, "dimName")
    ET.SubElement(param, "memberFilter")
    
    return param

def create_component_element(comp_data):
    """Create a component XML element."""
    comp = ET.Element("component")
    comp_type = comp_data.get('Component_Type', 'Button')
    
    comp.set("name", comp_data.get('Component_Name', ''))
    comp.set("description", comp_data.get('Description', ''))
    comp.set("xfText", comp_data.get('XF_Text', ''))
    comp.set("toolTip", comp_data.get('Tool_Tip', ''))
    comp.set("componentType", comp_type)
    comp.set("templateParameterValues", "")
    comp.set("brText1", "")
    comp.set("brText2", "")
    comp.set("boundParameterName", comp_data.get('Bound_Parameter', ''))
    
    # Set default attributes for non-embedded dashboards
    if comp_type != "EmbeddedDashboard":
        comp.set("paramValueForButtonClick", "")
        comp.set("applyParamValueToCurrentDbrd", "true")
        comp.set("selectionChangedSaveType", "NoAction")
        comp.set("selectionChangedPovActionType", "NoAction")
        
        # Get task type from extra properties or use default
        task_type = "NoTask"
        ui_action_type = "NoAction"
        
        # Check extra properties
        if comp_data.get('Extra_Property_1') == 'taskType':
            task_type = comp_data.get('Extra_Value_1', 'NoTask')
        elif comp_data.get('Extra_Property_2') == 'taskType':
            task_type = comp_data.get('Extra_Value_2', 'NoTask')
            
        if comp_data.get('Extra_Property_1') == 'uiActionType':
            ui_action_type = comp_data.get('Extra_Value_1', 'NoAction')
        elif comp_data.get('Extra_Property_2') == 'uiActionType':
            ui_action_type = comp_data.get('Extra_Value_2', 'NoAction')
        
        comp.set("selectionChangedTaskType", task_type)
        comp.set("selectionChangedUIActionType", ui_action_type)
        comp.set("selectionChangedNavigationType", "NoAction")
    
    # Add dashboardsToRedraw if specified
    if comp_data.get('Extra_Property_1') == 'dashboardsToRedraw':
        ET.SubElement(comp, "dashboardsToRedraw").text = comp_data.get('Extra_Value_1', '')
    elif comp_data.get('Extra_Property_2') == 'dashboardsToRedraw':
        ET.SubElement(comp, "dashboardsToRedraw").text = comp_data.get('Extra_Value_2', '')
    
    # Component definition
    comp_def = ET.SubElement(comp, "componentDefinition")
    
    if comp_type == "EmbeddedDashboard":
        embedded_name = ""
        if comp_data.get('Extra_Property_1') == 'embeddedDashboardName':
            embedded_name = comp_data.get('Extra_Value_1', '')
        elif comp_data.get('Extra_Property_2') == 'embeddedDashboardName':
            embedded_name = comp_data.get('Extra_Value_2', '')
        
        comp.set("embeddedDashboardName", embedded_name)
        comp.set("templateNameSuffix", "")
        comp.set("templateParameterValues", "")
        comp.set("brText1", "")
        comp.set("brText2", "")
    elif comp_type == "Button":
        button_type = "Standard"
        if comp_data.get('Extra_Property_1') == 'buttonType':
            button_type = comp_data.get('Extra_Value_1', 'Standard')
        elif comp_data.get('Extra_Property_2') == 'buttonType':
            button_type = comp_data.get('Extra_Value_2', 'Standard')
        
        button_def = ET.SubElement(comp_def, "XFButtonDefinition")
        button_def.set("ButtonType", button_type)
        ET.SubElement(button_def, "ImageFileSourceType").text = "Unknown"
        ET.SubElement(button_def, "ImageUrlOrFullFileName")
        ET.SubElement(button_def, "PageNumber")
        ET.SubElement(button_def, "ExcelSheet")
        ET.SubElement(button_def, "ExcelNamedRange")
    elif comp_type == "CubeView":
        cv_def = ET.SubElement(comp_def, "XFCubeViewDefinition")
        
        cube_view_name = ""
        if comp_data.get('Extra_Property_1') == 'cubeViewName':
            cube_view_name = comp_data.get('Extra_Value_1', '')
        elif comp_data.get('Extra_Property_2') == 'cubeViewName':
            cube_view_name = comp_data.get('Extra_Value_2', '')
        
        ET.SubElement(cv_def, "CubeViewName").text = cube_view_name
        ET.SubElement(cv_def, "ShowHeader").text = "true"
        ET.SubElement(cv_def, "ShowToggleSizeButton").text = "true"
    
    ET.SubElement(comp, "adapterMembers")
    
    return comp

def create_dashboard_element(dash_data, components_positioning):
    """Create a dashboard XML element."""
    dashboard = ET.Element("dashboard")
    
    dash_name = dash_data.get('Dashboard_Name', '')
    dashboard.set("name", dash_name)
    dashboard.set("description", dash_data.get('Description', ''))
    dashboard.set("pageCaption", "")
    dashboard.set("dashboardType", dash_data.get('Dashboard_Type', 'TopLevel'))
    dashboard.set("layoutType", dash_data.get('Layout_Type', 'Grid'))
    
    is_visible = str(dash_data.get('Is_Initially_Visible', 'Yes')).lower()
    dashboard.set("isInitiallyVisible", "true" if is_visible in ['yes', 'true', '1'] else "false")
    dashboard.set("loadDashboardTaskType", dash_data.get('Load_Task_Type', 'NoTask'))
    
    # Dashboard definition
    dash_def = ET.SubElement(dashboard, "DashboardDefinition")
    ET.SubElement(dash_def, "ShowTitle").text = "true"
    ET.SubElement(dash_def, "Notes")
    
    dyn_def = ET.SubElement(dash_def, "DynamicDashboardDefinition")
    ET.SubElement(dyn_def, "ComponentTemplateRepeatItems")
    
    cc_def = ET.SubElement(dash_def, "CustomControlDbrdDefinition")
    cc_def.set("RequiredInputParameters", "")
    
    # Grid layout
    grid_def = ET.SubElement(dash_def, "GridLayoutDefinition")
    
    # Parse column definitions
    col_defs_str = dash_data.get('Grid_Columns', '')
    col_defs_elem = ET.SubElement(grid_def, "ColumnDefinitions")
    col_defs = parse_grid_definition(col_defs_str)
    for col_def in col_defs:
        col_elem = ET.SubElement(col_defs_elem, "ColumnDefinition")
        col_elem.set("Type", col_def['type'])
        col_elem.set("Width", col_def['size'])
    
    # Parse row definitions
    row_defs_str = dash_data.get('Grid_Rows', '')
    row_defs_elem = ET.SubElement(grid_def, "RowDefinitions")
    row_defs = parse_grid_definition(row_defs_str)
    for row_def in row_defs:
        row_elem = ET.SubElement(row_defs_elem, "RowDefinition")
        row_elem.set("Type", row_def['type'])
        row_elem.set("Height", row_def['size'])
    
    # Component members
    comp_members = ET.SubElement(dashboard, "componentMembers")
    
    # Add positioned components
    for pos in components_positioning:
        if pos.get('Dashboard_Name') == dash_name:
            member = ET.SubElement(comp_members, "componentMember")
            member.set("name", pos.get('Component_Name', ''))
            member.set("left", "")
            member.set("top", "")
            member.set("width", pos.get('Width', ''))
            member.set("height", pos.get('Height', ''))
            member.set("dockPosition", pos.get('Dock_Position', 'Left'))
            member.set("grid_row", pos.get('Row', ''))
            member.set("grid_column", pos.get('Column', ''))
    
    return dashboard

def convert_template_to_xml(template_path):
    """Convert Excel/CSV template to Dashboard XML."""
    print(f"\n=== Converting Template to Dashboard XML ===\n")
    print(f"Template: {template_path}\n")
    
    # Determine if it's a directory of CSVs or an Excel file
    if os.path.isdir(template_path):
        # Directory of CSV files
        base_dir = template_path
        basic_info_file = os.path.join(base_dir, "2_Basic_Info.csv")
        parameters_file = os.path.join(base_dir, "3_Parameters.csv")
        components_file = os.path.join(base_dir, "4_Components.csv")
        dashboards_file = os.path.join(base_dir, "5_Dashboard_Layout.csv")
        positioning_file = os.path.join(base_dir, "6_Component_Positioning.csv")
    elif template_path.endswith('.xlsx') or template_path.endswith('.xls'):
        # Excel file - future enhancement
        print("Error: Excel file format (.xlsx/.xls) is not yet supported.")
        print("Please use the CSV template directory approach:")
        print("  1. Run: python scripts/create_dashboard_template.py")
        print("  2. Edit the CSV files in templates/Dashboard_Wireframe_Template/")
        print("  3. Run: python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/")
        print("\nAlternatively, export your Excel sheets to CSV files in a directory.")
        return None
    else:
        print(f"Error: Unrecognized template format: {template_path}")
        print("Expected: directory containing CSV files or .xlsx file (future support)")
        return None
    
    # Read basic info
    print("Reading basic information...")
    basic_info = read_settings_csv(basic_info_file)
    if not basic_info:
        print("Error: Could not read basic info file")
        return None
    
    workspace_name = basic_info.get('Workspace_Name', '00 GBL')
    namespace_prefix = basic_info.get('Namespace_Prefix', 'GBL')
    maint_unit_name = basic_info.get('Maintenance_Unit_Name', 'Dashboard')
    dashboard_group_name = basic_info.get('Dashboard_Group_Name', 'Dashboard Group')
    access_group = basic_info.get('Access_Group', 'Everyone')
    maint_group = basic_info.get('Maintenance_Group', 'Everyone')
    
    print(f"  Workspace: {workspace_name}")
    print(f"  Maintenance Unit: {maint_unit_name}")
    print(f"  Dashboard Group: {dashboard_group_name}")
    
    # Read parameters
    print("\nReading parameters...")
    parameters = read_csv_file(parameters_file)
    print(f"  Found {len(parameters)} parameter(s)")
    
    # Read components
    print("\nReading components...")
    components = read_csv_file(components_file)
    print(f"  Found {len(components)} component(s)")
    
    # Read dashboards
    print("\nReading dashboard layouts...")
    dashboards = read_csv_file(dashboards_file)
    print(f"  Found {len(dashboards)} dashboard(s)")
    
    # Read component positioning
    print("\nReading component positioning...")
    positioning = read_csv_file(positioning_file)
    print(f"  Found {len(positioning)} position(s)")
    
    # Build XML
    print("\nGenerating XML structure...")
    
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
    workspace.set("accessGroup", access_group)
    workspace.set("maintenanceGroup", maint_group)
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
    maint_unit.set("uiPlatformType", basic_info.get('UI_Platform_Type', 'WpfOrSilverlight'))
    maint_unit.set("accessGroup", access_group)
    maint_unit.set("maintenanceGroup", maint_group)
    maint_unit.set("wsAssemblyService", "")
    
    ET.SubElement(maint_unit, "fileResources")
    ET.SubElement(maint_unit, "stringResources")
    
    # Add parameters
    params_elem = ET.SubElement(maint_unit, "parameters")
    for param in parameters:
        params_elem.append(create_parameter_element(param))
    
    # Add adapters
    ET.SubElement(maint_unit, "adapters")
    
    # Add components
    comps_elem = ET.SubElement(maint_unit, "components")
    for comp in components:
        comps_elem.append(create_component_element(comp))
    
    # Dashboard groups
    dashboard_groups = ET.SubElement(maint_unit, "dashboardGroups")
    dash_group = ET.SubElement(dashboard_groups, "dashboardGroup")
    dash_group.set("name", dashboard_group_name)
    dash_group.set("description", "")
    dash_group.set("accessGroup", access_group)
    
    dashboards_elem = ET.SubElement(dash_group, "dashboards")
    
    # Add dashboards
    for dash in dashboards:
        dashboards_elem.append(create_dashboard_element(dash, positioning))
    
    # Workspace assemblies
    ET.SubElement(maint_unit, "workspaceAssemblies")
    
    # Cube view groups
    ET.SubElement(maint_unit, "cubeViewGroups")
    
    # Data management groups
    ET.SubElement(maint_unit, "dataManagementGroups")
    
    # Generate XML string
    xml_string = prettify_xml(root)
    
    # Save to file
    # Sanitize workspace and maintenance unit names for filename
    safe_workspace = sanitize_filename_component(workspace_name)
    safe_maint_unit = sanitize_filename_component(maint_unit_name)
    
    output_file = f"GeneratedXML/{safe_workspace}_{safe_maint_unit}.xml"
    
    os.makedirs("GeneratedXML", exist_ok=True)
    
    print(f"\nWriting XML to: {output_file}")
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(xml_string)
    
    print(f"\n✓ Dashboard XML generated successfully!")
    print(f"\nNext steps:")
    print(f"1. Review the generated XML: {output_file}")
    print(f"2. Import into OneStream using the Code Utility extension")
    print(f"3. Test in OneStream platform")
    
    return output_file

def main():
    """Main entry point."""
    if len(sys.argv) < 2:
        print("Usage: python convert_excel_to_dashboard.py <template_path>")
        print("\nExamples:")
        print("  python convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/")
        print("  python convert_excel_to_dashboard.py my_dashboard.xlsx (not yet supported)")
        sys.exit(1)
    
    template_path = sys.argv[1]
    
    if not os.path.exists(template_path):
        print(f"Error: Template not found: {template_path}")
        sys.exit(1)
    
    output_file = convert_template_to_xml(template_path)
    
    if output_file:
        print("\n" + "=" * 60)
        print("Conversion complete!")
        print("=" * 60)

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\nOperation cancelled by user.")
    except Exception as e:
        print(f"\nError: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
