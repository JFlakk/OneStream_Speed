# Excel Template for Dashboard Wireframing

This guide explains how to use the Excel template approach to design OneStream dashboards and convert them to XML format.

## Overview

The Excel/CSV template approach allows you to design dashboard wireframes in a spreadsheet format, which is then converted to the OneStream Dashboard XML format. This provides a more visual and structured way to plan your dashboards before importing them into OneStream.

## Quick Start

### 1. Generate the Template

```bash
cd /home/runner/work/OneStream_Speed/OneStream_Speed
python scripts/create_dashboard_template.py
```

This creates a template directory: `templates/Dashboard_Wireframe_Template/`

### 2. Edit the Template

The template consists of 7 CSV files (designed to be used as sheets in Excel):

- **1_Instructions.csv** - Usage instructions
- **2_Basic_Info.csv** - Workspace and maintenance unit settings
- **3_Parameters.csv** - Dashboard parameter definitions
- **4_Components.csv** - Component definitions
- **5_Dashboard_Layout.csv** - Dashboard structure
- **6_Component_Positioning.csv** - Component placement
- **7_Grid_Visual_Guide.csv** - Visual layout reference

#### Option A: Edit in Excel
1. Open Excel
2. Create a new workbook
3. For each CSV file, import it as a new sheet (Data > From Text/CSV)
4. Name each sheet to match the CSV filename (without the .csv extension)
5. Edit the sheets with your dashboard design
6. Save the workbook as .xlsx

#### Option B: Edit as CSV files
1. Open each CSV file in Excel, your favorite spreadsheet app, or a text editor
2. Edit the files directly
3. Save them back as CSV

### 3. Fill Out the Template

#### Sheet: 2_Basic_Info
Configure your workspace and maintenance unit:
```csv
Setting,Value
Workspace_Name,00 GBL
Namespace_Prefix,GBL
Maintenance_Unit_Name,My Dashboard
Dashboard_Group_Name,My Group
Access_Group,Everyone
```

#### Sheet: 3_Parameters
Define dashboard parameters:
```csv
Parameter_Name,Parameter_Type,Description,User_Prompt,Default_Value,Display_Items,Value_Items
DL_Options,DelimitedList,Option list,Select option,,Option 1|Option 2,opt1|opt2
IV_Input,InputValue,Text input,Enter value,,,
```

**Parameter Types:**
- `InputValue` - Single text input
- `DelimitedList` - Dropdown list (use comma-separated values)
- `LiteralValue` - Fixed value/constant

#### Sheet: 4_Components
Define dashboard components:
```csv
Component_Name,Component_Type,Description,XF_Text,Tool_Tip,Bound_Parameter,Extra_Property_1,Extra_Value_1,Extra_Property_2,Extra_Value_2
cbx_Menu,ComboBox,Menu selector,Select:,Choose option,DL_Options,dashboardsToRedraw,Main_Dashboard,uiActionType,Refresh
btn_Submit,Button,Submit button,Submit,Click to submit,,taskType,ExecuteDashboardExtenderBRAllActions,buttonType,Standard
txt_Input,TextBox,Text field,Value:,Enter text,IV_Input,,,,
```

**Component Types:**
- `Button` - Action button
- `ComboBox` - Dropdown selector
- `TextBox` - Text input field
- `EmbeddedDashboard` - Embedded dashboard container
- `CubeView` - Data grid/cube view
- `Logo` - Logo/image
- `SuppliedParameter` - Parameter supplier

**Extra Properties** (use Extra_Property_1/2 and Extra_Value_1/2 columns):
- `dashboardsToRedraw` - Dashboards to refresh on change
- `uiActionType` - UI action (Refresh, NoAction)
- `taskType` - Task type (NoTask, ExecuteDashboardExtenderBRAllActions)
- `buttonType` - Button type (Standard, FileUpload)
- `embeddedDashboardName` - Name of embedded dashboard
- `cubeViewName` - Name of cube view

#### Sheet: 5_Dashboard_Layout
Define dashboard structure:
```csv
Dashboard_Name,Dashboard_Type,Layout_Type,Description,Is_Initially_Visible,Load_Task_Type,Load_Task_Args,Grid_Columns,Grid_Rows
Main_Dashboard,TopLevel,Grid,Main dashboard,Yes,NoTask,,1:Component:*,3:Component:Auto;Component:*;Component:Auto
Content_Dashboard,Embedded,Grid,Content area,Yes,NoTask,,1:Component:*,1:Component:*
```

**Dashboard Types:**
- `TopLevel` - Top-level dashboard (standalone)
- `Embedded` - Embedded dashboard (shown inside another)
- `EmbeddedTopLevelWithoutParameterPrompts` - Embedded without parameter prompts

**Layout Types:**
- `Grid` - Grid-based layout (rows and columns)
- `Uniform` - Uniform grid
- `HorizontalStackPanel` - Horizontal stack
- `VerticalStackPanel` - Vertical stack

**Grid Definition Format:**
- Format: `count:Type:Size;Type:Size;...`
- Example: `3:Component:Auto;Component:*;Component:100`
  - 3 rows/columns total
  - Row/Column 1: Auto size
  - Row/Column 2: Fill remaining space (*)
  - Row/Column 3: 100 pixels
- Simple format: `1:Component:*` (single row/column, fill space)

**Size Values:**
- `*` - Fill remaining space (proportional)
- `Auto` - Size to content
- `100` - Fixed pixel size

#### Sheet: 6_Component_Positioning
Place components in dashboards:
```csv
Dashboard_Name,Component_Name,Row,Column,Dock_Position,Width,Height
Main_Dashboard,cbx_Menu,0,0,Left,,
Main_Dashboard,Embedded_Content,1,0,Left,,
Content_Dashboard,txt_Input,0,0,Left,,
```

**Position Notes:**
- Row and Column are 0-indexed (first row/column is 0)
- Width and Height are usually left empty (auto-sized)
- Dock_Position is typically "Left"

#### Sheet: 7_Grid_Visual_Guide
This sheet is for sketching your layout visually:
```
Info,Col_0,Col_1,Col_2
Row_0,Menu ComboBox,,
Row_1,Embedded Content (spans all columns),,
Row_2,Submit Button,,
```

Use this to visualize your grid layout before filling out the Component_Positioning sheet.

### 4. Convert to XML

Once you've filled out the template:

```bash
# If using CSV files directly
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/

# If using Excel file (future support)
python scripts/convert_excel_to_dashboard.py my_dashboard.xlsx
```

The script will generate: `GeneratedXML/[Workspace]_[MaintenanceUnit].xml`

### 5. Import into OneStream

1. Open the generated XML file to verify it looks correct
2. Use the Code Utility for OneStream VS Code extension to import the XML
3. Test the dashboard in OneStream

## Examples

### Example 1: Simple Data Entry Dashboard

**2_Basic_Info.csv:**
```csv
Setting,Value
Workspace_Name,00 APP
Namespace_Prefix,APP
Maintenance_Unit_Name,Data Entry
Dashboard_Group_Name,Data Entry Forms
```

**3_Parameters.csv:**
```csv
Parameter_Name,Parameter_Type,Description,User_Prompt,Default_Value,Display_Items,Value_Items
DL_Entity,DelimitedList,Entity selector,Select Entity:,,Entity1|Entity2|Entity3,E1|E2|E3
IV_Amount,InputValue,Amount input,Enter Amount:,0,,
```

**4_Components.csv:**
```csv
Component_Name,Component_Type,Description,XF_Text,Tool_Tip,Bound_Parameter,Extra_Property_1,Extra_Value_1
cbx_Entity,ComboBox,Entity dropdown,Entity:,Select entity,DL_Entity,uiActionType,Refresh
txt_Amount,TextBox,Amount field,Amount:,Enter amount,IV_Amount,,
btn_Save,Button,Save button,Save,Save data,,taskType,ExecuteDashboardExtenderBRAllActions
```

**5_Dashboard_Layout.csv:**
```csv
Dashboard_Name,Dashboard_Type,Layout_Type,Description,Is_Initially_Visible,Load_Task_Type,Load_Task_Args,Grid_Columns,Grid_Rows
DataEntry_Main,TopLevel,Grid,Data entry form,Yes,NoTask,,1:Component:*,3:Component:Auto;Component:Auto;Component:Auto
```

**6_Component_Positioning.csv:**
```csv
Dashboard_Name,Component_Name,Row,Column,Dock_Position,Width,Height
DataEntry_Main,cbx_Entity,0,0,Left,,
DataEntry_Main,txt_Amount,1,0,Left,,
DataEntry_Main,btn_Save,2,0,Left,,
```

### Example 2: Tabbed Dashboard with Embedded Content

**5_Dashboard_Layout.csv:**
```csv
Dashboard_Name,Dashboard_Type,Layout_Type,Description,Is_Initially_Visible,Load_Task_Type,Load_Task_Args,Grid_Columns,Grid_Rows
Main_Dashboard,TopLevel,Grid,Main container,Yes,NoTask,,1:Component:*,2:Component:Auto;Component:*
Tab1_Content,Embedded,Grid,Tab 1,Yes,NoTask,,1:Component:*,1:Component:*
Tab2_Content,Embedded,Grid,Tab 2,Yes,NoTask,,1:Component:*,1:Component:*
```

**4_Components.csv:**
```csv
Component_Name,Component_Type,Description,XF_Text,Tool_Tip,Bound_Parameter,Extra_Property_1,Extra_Value_1
cbx_TabSelector,ComboBox,Tab selector,View:,Select tab,DL_Tabs,dashboardsToRedraw,Main_Dashboard
Embedded_Tab1,EmbeddedDashboard,Tab 1 content,,,,embeddedDashboardName,Tab1_Content
Embedded_Tab2,EmbeddedDashboard,Tab 2 content,,,,embeddedDashboardName,Tab2_Content
```

**3_Parameters.csv:**
```csv
Parameter_Name,Parameter_Type,Description,User_Prompt,Default_Value,Display_Items,Value_Items
DL_Tabs,DelimitedList,Tab list,Select Tab:,Tab1,Tab 1|Tab 2,Tab1|Tab2
```

## Tips and Best Practices

### Design Tips
1. **Start Simple** - Begin with a basic layout and add complexity gradually
2. **Sketch First** - Use the Grid_Visual_Guide sheet to sketch your layout
3. **Plan Parameters** - Define all parameters before creating components
4. **Naming Convention** - Use consistent naming (e.g., cbx_ for ComboBox, btn_ for Button, txt_ for TextBox)
5. **Test Incrementally** - Generate and import XML frequently to test

### Grid Layout Tips
1. **Use Auto for Controls** - Set control rows to Auto height
2. **Use * for Content** - Set content area rows to * (fill remaining space)
3. **Keep It Simple** - Start with single column layouts
4. **Plan Proportions** - Think about how space should be divided

### Common Patterns

**Header + Content:**
```csv
Grid_Rows,2:Component:Auto;Component:*
```

**Header + Content + Footer:**
```csv
Grid_Rows,3:Component:Auto;Component:*;Component:Auto
```

**Two Columns (Equal):**
```csv
Grid_Columns,2:Component:*;Component:*
```

**Sidebar + Content:**
```csv
Grid_Columns,2:Component:250;Component:*
```

### Troubleshooting

**Issue: XML not generating**
- Check that all required CSV files exist
- Verify CSV files have proper headers
- Remove comment rows (starting with #) except where intended

**Issue: Components not appearing**
- Check Component_Positioning sheet has entries for all components
- Verify Dashboard_Name matches exactly between sheets
- Ensure Row/Column indices are within grid definition

**Issue: Parameters not working**
- Verify Parameter_Name in Parameters sheet matches Bound_Parameter in Components sheet
- Check Parameter_Type is valid (InputValue, DelimitedList, LiteralValue)
- For DelimitedList, ensure Display_Items and Value_Items have same number of entries

**Issue: Import fails in OneStream**
- Validate XML structure (should be well-formed)
- Check that Workspace and Maintenance Unit names don't conflict
- Verify all referenced dashboards exist (for EmbeddedDashboard components)

## Advanced Features

### Using Business Rules

To add business rule integration, add to **5_Dashboard_Layout.csv**:
```csv
Load_Task_Type,ExecuteDashboardExtenderBRAllActions
Load_Task_Args,{Workspace.Current.Assembly.MyAssembly}{Function=LoadData}{Param1=Value1}
```

### Custom Formatting

Add display formats in **4_Components.csv** Extra_Property columns:
```csv
Extra_Property_1,displayFormat
Extra_Value_1,|!LV_Banner_Format!|
```

### Multi-Row/Column Spans

For components spanning multiple rows/columns, set grid positions and use Width/Height:
```csv
Dashboard_Name,Component_Name,Row,Column,Dock_Position,Width,Height
Main,Banner,0,0,Left,*,100
```

## Workflow Summary

1. **Plan** - Sketch your dashboard design on paper or in the Grid_Visual_Guide
2. **Configure** - Fill out Basic_Info with workspace details
3. **Define** - Create Parameters and Components
4. **Layout** - Design Dashboard_Layout with grid structure
5. **Position** - Map Components to positions in Component_Positioning
6. **Generate** - Run convert_excel_to_dashboard.py
7. **Import** - Use OneStream Code Utility to import XML
8. **Test** - Test in OneStream and iterate

## Files Reference

### Created by create_dashboard_template.py
- `templates/Dashboard_Wireframe_Template/` - Template directory
  - `1_Instructions.csv` - Instructions
  - `2_Basic_Info.csv` - Configuration
  - `3_Parameters.csv` - Parameters
  - `4_Components.csv` - Components
  - `5_Dashboard_Layout.csv` - Layouts
  - `6_Component_Positioning.csv` - Positioning
  - `7_Grid_Visual_Guide.csv` - Visual guide
  - `README.txt` - Template readme

### Generated by convert_excel_to_dashboard.py
- `GeneratedXML/[Workspace]_[MaintenanceUnit].xml` - Dashboard XML

## Related Documentation

- [Dashboard XML Generation Guide](Dashboard_XML_Generation_Guide.md) - Complete Dashboard XML reference
- [Dashboard Generation Request Template](Dashboard_Generation_Request_Template.md) - Manual request template
- [Dashboard Generation Summary](Dashboard_Generation_Summary.md) - Overview of generation capabilities

## Script Reference

### create_dashboard_template.py
Generates the Excel/CSV template structure.

**Usage:**
```bash
python scripts/create_dashboard_template.py
```

**Output:** `templates/Dashboard_Wireframe_Template/`

### convert_excel_to_dashboard.py
Converts filled template to Dashboard XML.

**Usage:**
```bash
python scripts/convert_excel_to_dashboard.py <template_path>
```

**Input:** Template directory or Excel file  
**Output:** `GeneratedXML/[Workspace]_[MaintenanceUnit].xml`

## Support

For issues or questions:
1. Check this documentation
2. Review example CSV files in the template
3. Compare generated XML with existing dashboard XML files in `obj/` directory
4. Refer to OneStream documentation for component and parameter types

## Future Enhancements

Planned improvements:
- Direct Excel file (.xlsx) support without CSV conversion
- Visual grid designer with drag-and-drop
- Template validation before XML generation
- Import existing Dashboard XML into template format
- Pre-built templates for common dashboard patterns
