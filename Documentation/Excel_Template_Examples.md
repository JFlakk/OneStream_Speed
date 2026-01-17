# Excel Template Visual Examples

This document shows visual examples of what the Excel template sheets look like when filled out.

## Example 1: Simple Dashboard

This example creates a simple dashboard with a dropdown selector and an embedded content area.

### Sheet: Basic_Info

| Setting | Value |
|---------|-------|
| Workspace_Name | 00 GBL |
| Namespace_Prefix | GBL |
| Maintenance_Unit_Name | Test Dashboard |
| Dashboard_Group_Name | Test Group |
| Access_Group | Everyone |
| Maintenance_Group | Everyone |
| UI_Platform_Type | WpfOrSilverlight |

### Sheet: Parameters

| Parameter_Name | Parameter_Type | Description | User_Prompt | Default_Value | Display_Items | Value_Items |
|----------------|----------------|-------------|-------------|---------------|---------------|-------------|
| DL_Test_Options | DelimitedList | Test options list | Select an option | | Option A,Option B,Option C | optA,optB,optC |
| IV_User_Input | InputValue | User text input | Enter value | | | |

### Sheet: Components

| Component_Name | Component_Type | Description | XF_Text | Tool_Tip | Bound_Parameter | Extra_Property_1 | Extra_Value_1 | Extra_Property_2 | Extra_Value_2 |
|----------------|----------------|-------------|---------|----------|-----------------|------------------|---------------|------------------|---------------|
| cbx_Test_Selection | ComboBox | Option selector | Select an option: | Choose from available options | DL_Test_Options | dashboardsToRedraw | Test_Main_Dashboard | uiActionType | Refresh |
| Embedded_Content | EmbeddedDashboard | Content area | | | | embeddedDashboardName | Test_Content_Dashboard | | |

### Sheet: Dashboard_Layout

| Dashboard_Name | Dashboard_Type | Layout_Type | Description | Is_Initially_Visible | Load_Task_Type | Load_Task_Args | Grid_Columns | Grid_Rows |
|----------------|----------------|-------------|-------------|---------------------|----------------|----------------|--------------|-----------|
| Test_Main_Dashboard | TopLevel | Grid | Main test dashboard | Yes | NoTask | | 1:Component:* | 2:Component:Auto;Component:* |
| Test_Content_Dashboard | Embedded | Grid | Content dashboard | Yes | NoTask | | 1:Component:* | 1:Component:* |

### Sheet: Component_Positioning

| Dashboard_Name | Component_Name | Row | Column | Dock_Position | Width | Height |
|----------------|----------------|-----|--------|---------------|-------|--------|
| Test_Main_Dashboard | cbx_Test_Selection | 0 | 0 | Left | | |
| Test_Main_Dashboard | Embedded_Content | 1 | 0 | Left | | |

### Sheet: Grid_Visual_Guide

Visual representation of Test_Main_Dashboard:

```
┌─────────────────────────────────┐
│ Row 0 (Auto)                    │
│ cbx_Test_Selection              │
├─────────────────────────────────┤
│ Row 1 (*)                       │
│ Embedded_Content                │
│ (fills remaining space)         │
│                                 │
└─────────────────────────────────┘
```

---

## Example 2: Data Entry Form

A more complex example with multiple input fields and a submit button.

### Sheet: Basic_Info

| Setting | Value |
|---------|-------|
| Workspace_Name | 00 APP |
| Namespace_Prefix | APP |
| Maintenance_Unit_Name | Data Entry |
| Dashboard_Group_Name | Data Entry Forms |
| Access_Group | Everyone |

### Sheet: Parameters

| Parameter_Name | Parameter_Type | Description | User_Prompt | Default_Value | Display_Items | Value_Items |
|----------------|----------------|-------------|-------------|---------------|---------------|-------------|
| DL_Entity | DelimitedList | Entity selector | Select Entity: | | Entity1,Entity2,Entity3 | E1,E2,E3 |
| IV_Account | InputValue | Account input | Account: | | | |
| IV_Amount | InputValue | Amount input | Enter Amount: | 0 | | |
| IV_Description | InputValue | Description | Description: | | | |

### Sheet: Components

| Component_Name | Component_Type | Description | XF_Text | Tool_Tip | Bound_Parameter | Extra_Property_1 | Extra_Value_1 | Extra_Property_2 | Extra_Value_2 |
|----------------|----------------|-------------|---------|----------|-----------------|------------------|---------------|------------------|---------------|
| cbx_Entity | ComboBox | Entity dropdown | Entity: | Select entity | DL_Entity | uiActionType | Refresh | dashboardsToRedraw | DataEntry_Main |
| txt_Account | TextBox | Account field | Account: | Enter account | IV_Account | | | | |
| txt_Amount | TextBox | Amount field | Amount: | Enter amount | IV_Amount | | | | |
| txt_Description | TextBox | Description field | Description: | Enter description | IV_Description | | | | |
| btn_Save | Button | Save button | Save | Save data | | taskType | ExecuteDashboardExtenderBRAllActions | buttonType | Standard |
| btn_Clear | Button | Clear button | Clear | Clear form | | taskType | ExecuteDashboardExtenderBRAllActions | buttonType | Standard |

### Sheet: Dashboard_Layout

| Dashboard_Name | Dashboard_Type | Layout_Type | Description | Is_Initially_Visible | Load_Task_Type | Load_Task_Args | Grid_Columns | Grid_Rows |
|----------------|----------------|-------------|-------------|---------------------|----------------|----------------|--------------|-----------|
| DataEntry_Main | TopLevel | Grid | Data entry form | Yes | ExecuteDashboardExtenderBRAllActions | {Function=OnLoad} | 1:Component:* | 6:Component:Auto;Component:Auto;Component:Auto;Component:Auto;Component:Auto;Component:Auto |

### Sheet: Component_Positioning

| Dashboard_Name | Component_Name | Row | Column | Dock_Position | Width | Height |
|----------------|----------------|-----|--------|---------------|-------|--------|
| DataEntry_Main | cbx_Entity | 0 | 0 | Left | | |
| DataEntry_Main | txt_Account | 1 | 0 | Left | | |
| DataEntry_Main | txt_Amount | 2 | 0 | Left | | |
| DataEntry_Main | txt_Description | 3 | 0 | Left | | |
| DataEntry_Main | btn_Save | 4 | 0 | Left | | |
| DataEntry_Main | btn_Clear | 5 | 0 | Left | | |

### Sheet: Grid_Visual_Guide

Visual representation of DataEntry_Main:

```
┌─────────────────────────────────┐
│ Row 0: Entity ComboBox          │
├─────────────────────────────────┤
│ Row 1: Account TextBox          │
├─────────────────────────────────┤
│ Row 2: Amount TextBox           │
├─────────────────────────────────┤
│ Row 3: Description TextBox      │
├─────────────────────────────────┤
│ Row 4: Save Button              │
├─────────────────────────────────┤
│ Row 5: Clear Button             │
└─────────────────────────────────┘
```

---

## Example 3: Tabbed Dashboard with Sidebar

Complex layout with multiple columns and embedded dashboards.

### Sheet: Dashboard_Layout

| Dashboard_Name | Dashboard_Type | Layout_Type | Description | Is_Initially_Visible | Load_Task_Type | Grid_Columns | Grid_Rows |
|----------------|----------------|-------------|-------------|---------------------|----------------|--------------|-----------|
| Main_Dashboard | TopLevel | Grid | Main container | Yes | NoTask | 2:Component:250;Component:* | 2:Component:Auto;Component:* |
| Sidebar_Content | Embedded | Grid | Sidebar | Yes | NoTask | 1:Component:* | 3:Component:Auto;Component:Auto;Component:Auto |
| Tab1_Content | Embedded | Grid | Tab 1 | Yes | NoTask | 1:Component:* | 1:Component:* |
| Tab2_Content | Embedded | Grid | Tab 2 | Yes | NoTask | 1:Component:* | 1:Component:* |

### Sheet: Component_Positioning

| Dashboard_Name | Component_Name | Row | Column | Dock_Position | Width | Height |
|----------------|----------------|-----|--------|---------------|-------|--------|
| Main_Dashboard | Embedded_Sidebar | 0 | 0 | Left | | |
| Main_Dashboard | cbx_TabSelector | 0 | 1 | Left | | |
| Main_Dashboard | Embedded_TabContent | 1 | 1 | Left | | |
| Sidebar_Content | btn_Option1 | 0 | 0 | Left | | |
| Sidebar_Content | btn_Option2 | 1 | 0 | Left | | |
| Sidebar_Content | btn_Option3 | 2 | 0 | Left | | |

### Sheet: Grid_Visual_Guide

Visual representation of Main_Dashboard:

```
┌──────────────┬────────────────────────────┐
│ Sidebar      │ Row 0: Tab Selector        │
│ (250px)      │                            │
│              ├────────────────────────────┤
│ Embedded     │ Row 1: Tab Content (*)     │
│ Sidebar      │                            │
│              │ Embedded_TabContent        │
│              │                            │
│              │ (fills remaining space)    │
└──────────────┴────────────────────────────┘
    Col 0              Col 1 (*)
```

---

## Grid Layout Patterns

### Pattern 1: Header + Content
```
Grid_Rows: 2:Component:Auto;Component:*
```
```
┌────────────┐
│ Header     │ Auto height (fits content)
├────────────┤
│ Content    │ * (fills remaining space)
│            │
└────────────┘
```

### Pattern 2: Header + Content + Footer
```
Grid_Rows: 3:Component:Auto;Component:*;Component:Auto
```
```
┌────────────┐
│ Header     │ Auto
├────────────┤
│ Content    │ * (fills)
│            │
├────────────┤
│ Footer     │ Auto
└────────────┘
```

### Pattern 3: Two Column Equal
```
Grid_Columns: 2:Component:*;Component:*
```
```
┌──────┬──────┐
│      │      │
│ Col1 │ Col2 │
│ (*)  │ (*)  │
│      │      │
└──────┴──────┘
```

### Pattern 4: Sidebar + Content
```
Grid_Columns: 2:Component:250;Component:*
```
```
┌──────┬───────────┐
│ Side │ Content   │
│ 250px│ (*)       │
│      │           │
└──────┴───────────┘
```

### Pattern 5: Three Column with Center Larger
```
Grid_Columns: 3:Component:200;Component:*;Component:200
```
```
┌──────┬──────────┬──────┐
│ Left │ Center   │Right │
│ 200px│ (*)      │200px │
│      │          │      │
└──────┴──────────┴──────┘
```

---

## Tips for Visualization

When filling out the Grid_Visual_Guide sheet in Excel:

1. **Merge cells** to show component spans
2. **Use borders** to show grid lines
3. **Add colors** to differentiate component types
4. **Write component names** in the cells
5. **Add row/column labels** in the first row/column

Example in Excel:
```
     | A              | B              | C
-----+----------------+----------------+------------
  1  | Info           | Col_0          | Col_1
-----+----------------+----------------+------------
  2  | Row_0          | [Merge: Header spanning both columns]
-----+----------------+----------------+------------
  3  | Row_1          | Sidebar        | Content
-----+----------------+----------------+------------
```

Where [Merge: ...] indicates merged cells.

---

## Converting to XML

After filling out all sheets, save and convert:

```bash
# If using CSV files in a directory
python scripts/convert_excel_to_dashboard.py templates/my_dashboard/

# Output
GeneratedXML/00_APP_Data_Entry.xml
```

The generated XML will be ready to import into OneStream!

---

## Next Steps

1. **Review Generated XML** - Open and verify structure
2. **Import to OneStream** - Use Code Utility extension
3. **Test** - Verify dashboard appears and functions correctly
4. **Iterate** - Make changes and regenerate as needed

For more details, see [Excel Template Guide](../Documentation/Excel_Template_Guide.md)
