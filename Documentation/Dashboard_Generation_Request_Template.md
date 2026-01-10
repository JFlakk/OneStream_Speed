# Dashboard Generation Request Template

Use this template when requesting a new Dashboard XML to be generated.

## Basic Information

**Workspace Name:** ___________________  
**Namespace Prefix:** ___________________  
**Maintenance Unit Name:** ___________________  
**Dashboard Group Name:** ___________________  
**Access Group:** ☐ Everyone ☐ Administrators ☐ Other: ___________________

## Dashboard Specifications

### Dashboard 1: [Name]

**Dashboard Name:** ___________________  
**Description:** ___________________  
**Dashboard Type:**  
☐ TopLevel  
☐ EmbeddedTopLevelWithoutParameterPrompts  
☐ Embedded  
☐ Unknown  

**Layout Type:**  
☐ Grid  
☐ Uniform  
☐ HorizontalStackPanel  
☐ VerticalStackPanel  

**Is Initially Visible:** ☐ Yes ☐ No  

**Load Dashboard Task:**  
☐ NoTask  
☐ ExecuteDashboardExtenderBRAllActions  
☐ ExecuteDashboardExtenderBROnce  

**Load Task Args (if applicable):**
```
___________________
```

**Display Format (optional):**
```
___________________
```

### Layout Configuration

**For Grid Layout:**

**Columns:**
| Column # | Type | Width |
|----------|------|-------|
| 1 | Component / MoveableSplitter | * / Auto / [pixels] |
| 2 | | |
| 3 | | |

**Rows:**
| Row # | Type | Height |
|-------|------|--------|
| 1 | Component | * / Auto / [pixels] |
| 2 | | |
| 3 | | |

## Parameters

### Parameter 1: [Name]

**Parameter Name:** ___________________  
**Parameter Type:**  
☐ InputValue  
☐ DelimitedList  
☐ LiteralValue  

**Description:** ___________________  
**User Prompt:** ___________________  
**Default Value:** ___________________  

**For DelimitedList only:**  
**Display Items:** ___________________  
**Value Items:** ___________________  

---

### Parameter 2: [Name]

*Copy the above section for each parameter needed*

---

## Components

### Component 1: [Name]

**Component Name:** ___________________  
**Component Type:**  
☐ Button  
☐ ComboBox  
☐ TextBox  
☐ Logo  
☐ CubeView  
☐ EmbeddedDashboard  
☐ SuppliedParameter  
☐ SqlTableEditor  
☐ Other: ___________________  

**Description:** ___________________  
**XF Text (Label):** ___________________  
**Tool Tip:** ___________________  
**Bound Parameter Name (if applicable):** ___________________  

**Component-Specific Properties:**

*For Button:*
- Button Type: ☐ Standard ☐ FileUpload
- Selection Changed Task Type: ___________________
- Selection Changed Task Args: ___________________
- Dashboards To Redraw: ___________________
- Dashboards To Show: ___________________
- Dashboards To Hide: ___________________

*For ComboBox:*
- Bound Parameter Name: ___________________
- Selection Changed UI Action: ☐ Refresh ☐ NoAction

*For EmbeddedDashboard:*
- Embedded Dashboard Name: ___________________

*For CubeView:*
- Cube View Name: ___________________
- Show Header: ☐ Yes ☐ No
- Show Toggle Size Button: ☐ Yes ☐ No

**Display Format (optional):** ___________________

---

### Component 2: [Name]

*Copy the above section for each component needed*

---

## Component Positioning in Dashboard

List where each component should appear in the dashboard:

| Component Name | Dashboard | Row | Column | Dock Position | Width | Height |
|----------------|-----------|-----|--------|---------------|-------|--------|
| | | | | Left | | |
| | | | | Left | | |
| | | | | Left | | |

## Business Rule Integration (Optional)

**Dashboard Extender BR:**
- Assembly Name: ___________________
- Business Rule Name: ___________________
- Function Name: ___________________

**Dashboard String Function:**
- Assembly Name: ___________________
- Business Rule Name: ___________________
- Function Name: ___________________

**Dashboard Data Set:**
- Assembly Name: ___________________
- Business Rule Name: ___________________
- Function Name: ___________________

## Additional Notes

Provide any additional context, requirements, or special considerations:

```
___________________
___________________
___________________
```

---

## Simple Example (Filled Out)

Here's a completed example for reference:

### Basic Information
- **Workspace Name:** 00 GBL
- **Namespace Prefix:** GBL
- **Maintenance Unit Name:** Test Dashboard
- **Dashboard Group Name:** Test Group
- **Access Group:** Everyone

### Dashboard: Test_Main_Dashboard
- **Type:** TopLevel
- **Layout:** Grid
- **Description:** A test dashboard for demonstration
- **Is Initially Visible:** Yes
- **Load Task:** NoTask

**Grid Layout:**
- Columns: 1 column, Type=Component, Width=*
- Rows: 2 rows
  - Row 1: Type=Component, Height=Auto
  - Row 2: Type=Component, Height=*

### Parameter: DL_Test_Options
- **Type:** DelimitedList
- **Display Items:** Option A,Option B,Option C
- **Value Items:** optA,optB,optC

### Component: cbx_Test_Selection
- **Type:** ComboBox
- **XF Text:** "Select an option:"
- **Bound Parameter:** DL_Test_Options
- **Dashboards To Redraw:** Test_Main_Dashboard

### Component: Embedded Test_Content
- **Type:** EmbeddedDashboard
- **Embedded Dashboard Name:** Test_Content_Dashboard

### Component Positioning:
- cbx_Test_Selection → Test_Main_Dashboard, Row 0
- Embedded Test_Content → Test_Main_Dashboard, Row 1

---

## Submission

Once completed, provide this information to generate your Dashboard XML file.
