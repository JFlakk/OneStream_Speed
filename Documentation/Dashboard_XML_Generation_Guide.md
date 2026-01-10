# Dashboard XML Generation Guide for OneStream

## Overview

Yes, I am capable of generating Dashboard objects in XML files for OneStream. This guide explains the structure of Dashboard XML and what information is needed to generate them.

## What is a Dashboard in OneStream?

Dashboards in OneStream are UI components that provide interactive interfaces for users. They are defined in XML format within Workspace Maintenance Units and can include:
- Parameters (input values, delimited lists, literal values)
- Components (buttons, combo boxes, text boxes, embedded dashboards, cube views, etc.)
- Dashboard Groups (collections of related dashboards)
- Dashboard definitions with layout and component members

## Dashboard XML Structure

Dashboard objects are stored in the XML files under the `obj/` folder, specifically in workspace extracts like:
- `obj/PPBE_Workspaces/00 GBL/GBL Dashboard/WS Extracts/GBL Dashboard.xml`
- `obj/PPBE_Workspaces/00 GBL/GBL Admin Dashboards/WS Extracts/GBL Admin Dashboards.xml`

### High-Level Structure

```xml
<OneStreamXF version="9.0.1.17403">
    <applicationWorkspacesRoot>
        <workspaces>
            <workspace name="WorkspaceName" ...>
                <maintenanceUnits>
                    <maintenanceUnit name="MaintenanceUnitName" ...>
                        <!-- Parameters -->
                        <parameters>...</parameters>
                        
                        <!-- Adapters -->
                        <adapters />
                        
                        <!-- Components -->
                        <components>...</components>
                        
                        <!-- Dashboard Groups -->
                        <dashboardGroups>...</dashboardGroups>
                        
                        <!-- Workspace Assemblies -->
                        <workspaceAssemblies>...</workspaceAssemblies>
                        
                        <!-- Cube View Groups -->
                        <cubeViewGroups>...</cubeViewGroups>
                    </maintenanceUnit>
                </maintenanceUnits>
            </workspace>
        </workspaces>
    </applicationWorkspacesRoot>
</OneStreamXF>
```

## Required Information for Dashboard Generation

To generate a Dashboard object, I need the following information:

### 1. Workspace Information
- **Workspace Name**: e.g., "00 GBL"
- **Namespace Prefix**: e.g., "GBL"
- **Access Group**: e.g., "Everyone" or "Administrators"
- **Maintenance Group**: e.g., "Everyone" or "Administrators"

### 2. Maintenance Unit Information
- **Maintenance Unit Name**: e.g., "GBL Dashboard"
- **Description**: Brief description of the maintenance unit
- **UI Platform Type**: Usually "WpfOrSilverlight"

### 3. Dashboard Group Information
- **Dashboard Group Name**: e.g., "GBL Dashboard Rpt"
- **Description**: Brief description
- **Access Group**: Security group that can access this dashboard group

### 4. Dashboard Information

For each dashboard you want to create, provide:

#### Basic Dashboard Properties
- **Dashboard Name**: Unique identifier (e.g., "GBL_Rpt_0_Main")
- **Description**: User-friendly description
- **Page Caption**: Optional title shown in the UI
- **Dashboard Type**: Options include:
  - `TopLevel` - Top-level dashboard
  - `EmbeddedTopLevelWithoutParameterPrompts` - Embedded top-level without prompts
  - `Embedded` - Embedded dashboard
  - `Unknown` - Default type
- **Layout Type**: Options include:
  - `Grid` - Grid-based layout
  - `Uniform` - Uniform layout
  - `HorizontalStackPanel` - Horizontal stack
  - `VerticalStackPanel` - Vertical stack
- **Is Initially Visible**: true/false
- **Load Dashboard Task Type**: Options include:
  - `NoTask`
  - `ExecuteDashboardExtenderBRAllActions`
  - `ExecuteDashboardExtenderBROnce`

#### Dashboard Layout (for Grid layout)
- **Column Definitions**: Array of column specs
  - Column Type: `Component`, `MoveableSplitter`, etc.
  - Width: `*`, `Auto`, or specific pixel value
- **Row Definitions**: Array of row specs
  - Row Type: `Component`, etc.
  - Height: `*`, `Auto`, or specific pixel value

#### Component Members
List of components to include in the dashboard with their positioning:
- **Component Name**: Reference to a component defined in the `<components>` section
- **Dock Position**: Usually "Left"
- **Grid Position** (for Grid layouts): Row/column indices
- **Size**: Width and height if applicable

### 5. Parameter Definitions

For each parameter needed by the dashboard:

- **Parameter Name**: e.g., "IV_GBL_DynamicContent"
- **Parameter Type**: Options include:
  - `InputValue` - Single text input
  - `DelimitedList` - List of values
  - `LiteralValue` - Fixed value
- **Description**: Brief description
- **Default Value**: Initial value
- **Display Items**: For delimited lists, the display text
- **Value Items**: For delimited lists, the actual values
- **User Prompt**: Prompt text for user input

### 6. Component Definitions

For each component used in dashboards:

#### Common Component Properties
- **Component Name**: Unique identifier
- **Component Type**: Options include:
  - `Button` - Action button
  - `ComboBox` - Dropdown selection
  - `TextBox` - Text input
  - `Logo` - Logo/image display
  - `CubeView` - Cube view component
  - `EmbeddedDashboard` - Nested dashboard
  - `SuppliedParameter` - Parameter supplier
  - `SqlTableEditor` - Table editor
- **Description**: Component description
- **XF Text**: Display text/label
- **Tool Tip**: Hover text
- **Bound Parameter Name**: If bound to a parameter

#### Button-Specific Properties
- **Button Type**: `Standard`, `FileUpload`, etc.
- **Selection Changed Task Type**: Action on click
- **Selection Changed Task Args**: Arguments for the task
- **Dashboards To Redraw**: List of dashboards to refresh
- **Dashboards To Show**: List of dashboards to show
- **Dashboards To Hide**: List of dashboards to hide

#### ComboBox-Specific Properties
- **Bound Parameter Name**: Parameter containing list items
- **Selection Changed UI Action Type**: Usually `Refresh`

#### EmbeddedDashboard-Specific Properties
- **Embedded Dashboard Name**: Name of the dashboard to embed

#### CubeView-Specific Properties
- **Cube View Name**: Name of the cube view to display
- **Show Header**: true/false
- **Show Toggle Size Button**: true/false

### 7. Display Formats (Optional)
- **Display Format References**: e.g., "|!LV_GBL_Banner_Format!|"
- These reference literal value parameters for styling

### 8. Business Rule References (if applicable)
- **Dashboard Extender BR**: For load tasks
  - Business Rule Name
  - Function Name
  - Arguments
- **Dashboard String Function**: For dynamic text
- **Dashboard Data Set**: For data retrieval

## Example: Minimal Dashboard

Here's what you'd need to specify for a simple dashboard:

```
Workspace: "00 GBL"
Maintenance Unit: "GBL Dashboard"
Dashboard Group: "Simple Group"
  Access Group: "Everyone"
  
Dashboard: "Simple_Dashboard"
  Type: TopLevel
  Layout: Grid
  Description: "A simple dashboard"
  Is Initially Visible: true
  Load Task: NoTask
  
  Grid Layout:
    Columns: [{ Type: Component, Width: "*" }]
    Rows: [{ Type: Component, Height: "Auto" }]
  
  Components:
    - Name: "Embedded Child Dashboard"
      Reference: "Child_Dashboard"
      
Component: "Child_Dashboard"
  Type: EmbeddedDashboard
  Embedded Dashboard Name: "Actual_Child_Dashboard_Name"
```

## Example: Dashboard with Parameters and Components

```
Parameters:
  - Name: "DL_Menu_Options"
    Type: DelimitedList
    Display Items: "Option 1,Option 2,Option 3"
    Value Items: "opt1,opt2,opt3"
    
  - Name: "IV_Selected_Option"
    Type: InputValue
    Default Value: ""

Components:
  - Name: "cbx_Menu"
    Type: ComboBox
    XF Text: "Select an option:"
    Bound Parameter: "DL_Menu_Options"
    Dashboards To Redraw: "Main_Dashboard"
    
  - Name: "SP_Selected"
    Type: SuppliedParameter
    Bound Parameter: "IV_Selected_Option"

Dashboard: "Main_Dashboard"
  Type: EmbeddedTopLevelWithoutParameterPrompts
  Layout: Grid
  Load Task: ExecuteDashboardExtenderBRAllActions
  Load Task Args: "{Workspace.Current.Assembly.Helper}{Function}{Param=Value}"
  
  Grid Layout:
    Columns: [{ Type: Component, Width: "*" }]
    Rows: 
      - { Type: Component, Height: "Auto" }
      - { Type: Component, Height: "*" }
  
  Component Members:
    - Component: "cbx_Menu" (at row 0)
    - Component: "SP_Selected" (hidden/no position)
```

## Generation Process

When you provide the above information, I can:

1. **Create the XML structure** following the OneStream schema
2. **Generate parameter definitions** with proper types and formats
3. **Define components** with all necessary properties
4. **Build dashboard groups** and dashboard definitions
5. **Set up grid layouts** with rows and columns
6. **Link components** to dashboards with proper positioning
7. **Include business rule references** for extenders and functions
8. **Apply display formats** and styling references

## Important Notes

### Placeholder Values
The generated XML will contain placeholders like:
- `__WsNamespacePrefix` - Replaced by OneStream on import
- `__WsAssemblyName` - Replaced by OneStream on import

These should **NOT** be manually replaced in the XML.

### Version Compatibility
The XML examples are based on OneStream version 9.0.1.17403. The structure is consistent across recent versions but may have minor differences in older versions.

### Import Process
The generated XML file should be:
1. Placed in the appropriate location (e.g., `obj/` folder)
2. Imported using the Code Utility for OneStream VS Code extension
3. Tested within OneStream platform

### Complexity
Dashboards can be very complex with:
- Nested embedded dashboards (3-4 levels deep)
- Multiple parameters with interdependencies
- Dynamic behavior controlled by business rules
- Complex grid layouts with splitters

For complex dashboards, it's recommended to:
1. Start with a simple structure
2. Test in OneStream
3. Iteratively add complexity
4. Export from OneStream to see actual XML

## How to Request Dashboard Generation

When requesting dashboard generation, provide information in this format:

```
I need a dashboard with the following specifications:

Workspace: [name]
Maintenance Unit: [name]
Dashboard Group: [name]

Dashboard "[name]":
  - Type: [type]
  - Layout: [type]
  - Description: [text]
  
Parameters:
  - [name]: [type] = [values]
  
Components:
  - [name]: [type] with [properties]
  
Layout:
  - Columns: [specifications]
  - Rows: [specifications]
  - Component positioning: [details]
```

The more detailed your specification, the more accurate the generated XML will be.

## Resources

- Example Dashboards: See `obj/PPBE_Workspaces/00 GBL/GBL Dashboard/`
- Complex Examples: See `obj/PPBE_Workspaces/00 GBL/GBL Admin Dashboards/`
- OneStream Documentation: Refer to OneStream official documentation for component types and properties

## Conclusion

I can generate Dashboard XML objects for OneStream given proper specifications. The key is providing complete information about:
- Workspace and maintenance unit context
- Dashboard structure and layout
- Parameters and components needed
- Business logic integration

Start simple, test frequently, and build up complexity as needed.
