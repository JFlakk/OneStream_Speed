# Quick Start: Excel Template for Dashboard Wireframing

Get started with the Excel template approach in 5 minutes!

## Step 1: Generate Template (30 seconds)

```bash
python scripts/create_dashboard_template.py
```

This creates: `templates/Dashboard_Wireframe_Template/` with 7 CSV files.

## Step 2: Open in Excel (1 minute)

### Option A: Import CSVs into Excel
1. Open Microsoft Excel
2. Create new workbook
3. Import each CSV file as a new sheet:
   - Data → From Text/CSV
   - Select file, click Load
   - Rename sheet (remove number prefix)

### Option B: Edit CSVs Directly
1. Open each CSV file in Excel
2. Edit and save

## Step 3: Design Your Dashboard (3 minutes)

### A. Basic Info (2_Basic_Info.csv)
Replace the sample values:
```
Workspace_Name: Your_Workspace
Maintenance_Unit_Name: Your_Dashboard
Dashboard_Group_Name: Your_Group
```

### B. Parameters (3_Parameters.csv)
Delete example rows (marked with #), add yours:
```csv
DL_MyOptions,DelimitedList,Description,Prompt,,Opt1,Opt2,val1,val2
```
(Use commas to separate values in CSV format)

### C. Components (4_Components.csv)
Delete examples, add your components:
```csv
cbx_Selector,ComboBox,Description,Label:,Tooltip,DL_MyOptions,...
```
(CSV format with comma-separated values)

### D. Dashboard Layout (5_Dashboard_Layout.csv)
Define your dashboard:
```csv
MyDash,TopLevel,Grid,Description,Yes,NoTask,,1:Component:*,2:Component:Auto;Component:*
```
(CSV format - note the grid definition in the last column)

### E. Component Positioning (6_Component_Positioning.csv)
Place components:
```csv
MyDash,cbx_Selector,0,0,Left,,
MyDash,Content_Area,1,0,Left,,
```
(CSV format showing row, column, and dock position)

## Step 4: Convert to XML (30 seconds)

```bash
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/
```

Output: `GeneratedXML/Your_Workspace_Your_Dashboard.xml`

## Step 5: Import to OneStream

1. Open VS Code with OneStream extension
2. Use Code Utility to import the XML
3. Test in OneStream!

---

## Minimal Example

Here's the absolute minimum for a working dashboard:

### 2_Basic_Info.csv
```csv
Setting,Value
Workspace_Name,00 TEST
Namespace_Prefix,TEST
Maintenance_Unit_Name,Simple Dash
Dashboard_Group_Name,Test Group
Access_Group,Everyone
```

### 3_Parameters.csv
```csv
Parameter_Name,Parameter_Type,Description,User_Prompt,Default_Value,Display_Items,Value_Items
# Leave empty for now
```

### 4_Components.csv
```csv
Component_Name,Component_Type,Description,XF_Text,Tool_Tip,Bound_Parameter,Extra_Property_1,Extra_Value_1,Extra_Property_2,Extra_Value_2
lbl_Hello,TextBox,Welcome label,Hello World!,,,,,,
```

### 5_Dashboard_Layout.csv
```csv
Dashboard_Name,Dashboard_Type,Layout_Type,Description,Is_Initially_Visible,Load_Task_Type,Load_Task_Args,Grid_Columns,Grid_Rows
Simple_Main,TopLevel,Grid,Simple dashboard,Yes,NoTask,,1:Component:*,1:Component:*
```

### 6_Component_Positioning.csv
```csv
Dashboard_Name,Component_Name,Row,Column,Dock_Position,Width,Height
Simple_Main,lbl_Hello,0,0,Left,,
```

Convert and import - you'll have a working dashboard with a "Hello World!" label!

---

## Common Patterns

### Pattern: Dropdown + Content Area
```csv
# Components
cbx_Menu,ComboBox,...
Embedded_Content,EmbeddedDashboard,...

# Layout
Grid_Rows: 2:Component:Auto;Component:*

# Positioning
Dashboard,cbx_Menu,0,0,Left,,
Dashboard,Embedded_Content,1,0,Left,,
```

### Pattern: Form with Multiple Fields
```csv
# Layout
Grid_Rows: 5:Component:Auto;Component:Auto;Component:Auto;Component:Auto;Component:Auto

# Positioning (stack vertically)
Dashboard,Field1,0,0,Left,,
Dashboard,Field2,1,0,Left,,
Dashboard,Field3,2,0,Left,,
Dashboard,btn_Save,3,0,Left,,
Dashboard,btn_Cancel,4,0,Left,,
```

### Pattern: Sidebar + Content
```csv
# Layout
Grid_Columns: 2:Component:250;Component:*
Grid_Rows: 1:Component:*

# Positioning
Dashboard,Sidebar,0,0,Left,,
Dashboard,Content,0,1,Left,,
```

---

## Troubleshooting

**Error: Could not read basic info file**
→ Check that `2_Basic_Info.csv` exists and has headers

**Components not appearing**
→ Verify Component_Name matches between Components and Positioning sheets

**Parameters not working**
→ Bound_Parameter must match Parameter_Name exactly

**Import fails**
→ Check XML is well-formed (open in browser/XML editor)

---

## Next Steps

- See [Excel Template Examples](Excel_Template_Examples.md) for more examples
- Read [Excel Template Guide](Excel_Template_Guide.md) for complete documentation
- Check [Dashboard XML Generation Guide](Dashboard_XML_Generation_Guide.md) for XML details

---

## Tips

1. **Start small** - Get a simple dashboard working first
2. **Test often** - Generate XML frequently to catch issues early
3. **Use Grid_Visual_Guide** - Sketch your layout visually
4. **Name consistently** - Use prefixes like cbx_, txt_, btn_
5. **Delete examples** - Remove sample rows before adding your data

Happy dashboard building! 🎉
