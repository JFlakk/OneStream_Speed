# Model Dimension Manager - Validation Dashboard UI Layout

## Enhanced DBRD_Validations Dashboard Layout

This document provides visual representations of the recommended UI layout for the enhanced Validation Dashboard.

## Main Validation Dashboard View

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ DIMENSION VALIDATIONS                                                         │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────┐  ┌────────────┐ │
│  │ Active         │  │ Total Issues   │  │ Critical       │  │   Manage   │ │
│  │ Validations    │  │ (7 days)       │  │ Issues         │  │ Validations│ │
│  │                │  │                │  │                │  │            │ │
│  │      12        │  │      45        │  │       3        │  │   [Button] │ │
│  │                │  │                │  │                │  │            │ │
│  └────────────────┘  └────────────────┘  └────────────────┘  └────────────┘ │
│                                                                               │
├───────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│  VALIDATION ISSUES OVERVIEW                                                   │
│                                                                               │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │                                                                          │ │
│  │   Chart: Validation Issues by Dimension                                 │ │
│  │                                                                          │ │
│  │   ┌─────────────┐                                                       │ │
│  │   │   Entity    │ ████████████████ 25 Issues                           │ │
│  │   │   Account   │ ██████████ 15 Issues                                 │ │
│  │   │   Scenario  │ ███ 5 Issues                                         │ │
│  │   └─────────────┘                                                       │ │
│  │                                                                          │ │
│  │   Legend: [■] Critical  [■] Warning  [■] Info                          │ │
│  │                                                                          │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│                                                                               │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Validation Configuration Dashboard (VAL_LandingPage Enhanced)

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ VALIDATION CONFIGURATION                                                      │
├─────────────────────────┬────────────────────────────────────────────────────┤
│ CONFIGURATION LIST      │ CONFIGURATION DETAILS                              │
│                         │                                                    │
│ ┌─────────────────────┐ │ ┌────────────────────────────────────────────────┐ │
│ │ [+ Add New]         │ │ │ Tab: General | Rules | Execution | History     │ │
│ └─────────────────────┘ │ ├────────────────────────────────────────────────┤ │
│                         │ │ General Settings                               │ │
│ Entity Validations      │ │                                                │ │
│ ├─ ✓ IC Hierarchy Chk  │ │ Validation Name: [Entity IC Check            ] │ │
│ ├─ ✓ Property Populate │ │                                                │ │
│ └─ ⚠ Parent-Child Val  │ │ Description: [Verify all entities in default │ │
│                         │ │              hierarchy exist in IC hierarchy] │ │
│ Account Validations     │ │                                                │ │
│ ├─ ✓ Account Type Chk  │ │ Validation Type: [Hierarchy Comparison   ▼]   │ │
│ └─ ⚠ Base Member Chk   │ │                                                │ │
│                         │ │ Target Dimension: [Entity                ▼]   │ │
│ Scenario Validations    │ │                                                │ │
│ └─ ✓ Scenario Setup    │ │ Severity: [Critical ▼]   Active: [✓]         │ │
│                         │ │                                                │ │
│ [Filter: All ▼]         │ │ ───────────────────────────────────────────── │ │
│ [Search...]             │ │                                                │ │
│                         │ │ [Save Config] [Execute Now] [Delete] [Duplicate]│ │
│                         │ │                                                │ │
└─────────────────────────┴────────────────────────────────────────────────────┘
```

## Validation Results Dashboard

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ VALIDATION RESULTS                                                            │
├──────────────────────────────────────────────────────────────────────────────┤
│ Filters: Date Range: [Last 7 Days ▼]  Dimension: [All ▼]  Severity: [☑All]  │
│          Status: [☑Open] [☐Acknowledged] [☐Resolved]                         │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│ SUMMARY VIEW                                                                  │
│ ┌───────────────────────────────────────────────────────────────────────────┐│
│ │Validation Name       │Dim    │Type      │Total│Crit│Warn│Info│Last Run   ││
│ ├─────────────────────┼───────┼──────────┼─────┼────┼────┼────┼───────────┤│
│ │►Entity IC Check     │Entity │Hier Comp │  25 │  3 │ 15 │  7 │2026-01-10 ││
│ │►Property Population │Entity │Prop Pop  │  15 │  0 │ 10 │  5 │2026-01-10 ││
│ │►Parent-Child Val    │Account│Parent-Ch │   5 │  2 │  3 │  0 │2026-01-09 ││
│ └───────────────────────────────────────────────────────────────────────────┘│
│                                                                               │
├───────────────────────────────────────────────────────────────────────────────┤
│ ISSUE DETAILS (Entity IC Check selected)                                     │
│ ┌───────────────────────────────────────────────────────────────────────────┐│
│ │Member     │Hierarchy│Issue Type        │Severity │Status│Resolution Note ││
│ ├───────────┼─────────┼──────────────────┼─────────┼──────┼────────────────┤│
│ │E100       │IC_Hier  │Missing in Hier   │Critical │Open  │                ││
│ │E125       │IC_Hier  │Missing in Hier   │Critical │Open  │                ││
│ │E150       │IC_Hier  │Missing in Hier   │Critical │Open  │                ││
│ │E200       │IC_Hier  │Missing in Hier   │Warning  │Open  │                ││
│ └───────────────────────────────────────────────────────────────────────────┘│
│                                                                               │
│ Actions: [Mark as Acknowledged] [Resolve Selected] [Ignore] [Export Excel]   │
│                                                                               │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Validation Execution Dashboard

```
┌──────────────────────────────────────────────────────────────────────────────┐
│ RUN VALIDATIONS                                                               │
├──────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│ EXECUTION QUEUE                                                               │
│ ┌────────────────────────────────────────────────────────────────────────┐   │
│ │ [☑] Entity IC Hierarchy Check                    [Status: Ready]      │   │
│ │ [☑] Entity Property Population                   [Status: Ready]      │   │
│ │ [☐] Account Type Validation                      [Status: Ready]      │   │
│ │ [☑] Scenario Setup Check                         [Status: Ready]      │   │
│ │                                                                        │   │
│ │ [Execute Selected] [Execute All] [Schedule]                           │   │
│ └────────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│ ACTIVE EXECUTIONS                                                             │
│ ┌────────────────────────────────────────────────────────────────────────┐   │
│ │ Entity IC Check         ███████████░░░░░░░░░ 65%    00:02:15  [Cancel]│   │
│ │ Property Population     ██████░░░░░░░░░░░░░░░ 35%    00:01:10  [Cancel]│   │
│ └────────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
│ RECENT COMPLETIONS                                                            │
│ ┌────────────────────────────────────────────────────────────────────────┐   │
│ │ Validation Name          │ Status  │ Duration │ Issues │ Completed      │   │
│ ├──────────────────────────┼─────────┼──────────┼────────┼───────────────┤   │
│ │ Account Type Validation  │ Success │ 00:01:45 │     12 │ 10:30:15      │   │
│ │ Scenario Setup Check     │ Success │ 00:00:30 │      3 │ 10:28:30      │   │
│ │ Parent-Child Integrity   │ Warning │ 00:02:15 │      8 │ 10:25:00      │   │
│ └────────────────────────────────────────────────────────────────────────┘   │
│                                                                               │
└──────────────────────────────────────────────────────────────────────────────┘
```

## Component Hierarchy

```
MDM Dashboard (DBRD)
└── DBRD_Validations (Main Enhanced Dashboard)
    ├── DBRD_VAL2_Overview (KPI Panel)
    │   ├── lbl_VAL2_TotalValidations
    │   ├── lbl_VAL2_TotalIssues
    │   ├── lbl_VAL2_CriticalIssues
    │   └── btn_VAL2_ManageConfig
    │
    └── BIView_Validations (Chart Component)
        └── DATA_Validation_Dashboard (SQL Adapter)

VAL_LandingPage (Configuration Dashboard) - Existing Enhanced
├── VAL_LeftMenu
│   ├── btn_VAL_AddConfig
│   └── lbx_VAL_SavedConfig
│
└── VAL_Content
    ├── VAL_HierCompConfigSetup
    │   ├── txt_VAL_Name
    │   ├── cbx_VAL_ValidationMethod
    │   ├── cbx_GBL_Dimension
    │   └── VAL_HierCompProcess
    │       ├── btn_VAL_SaveConfig
    │       ├── btn_VAL_Process
    │       └── btn_VAL_DeleteConfig
    │
    └── VAL_Results
        └── Grd_ValidationResults_VAL
```

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         DATABASE TABLES                                  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
                    ▼               ▼               ▼
        ┌──────────────────┐ ┌─────────────┐ ┌────────────────┐
        │ MDM_Dim         │ │ MDM_Dim     │ │ MDM_Dim        │
        │ ValidationTypes │ │ Validation  │ │ Validation     │
        │                 │ │ Config      │ │ Rules          │
        └──────────────────┘ └─────────────┘ └────────────────┘
                    │               │               │
                    └───────────────┼───────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
        ┌──────────────────┐ ┌─────────────┐ ┌────────────────┐
        │ SQL ADAPTERS     │ │ PARAMETERS  │ │ BUSINESS       │
        │                  │ │             │ │ RULES          │
        │ DATA_VAL2_       │ │ Param_VAL2_ │ │ MDM_           │
        │ ConfigList       │ │ TotalValid  │ │ Validation     │
        │ ExecutionHist    │ │ TotalIssues │ │ Executor       │
        │ Results          │ │ Critical    │ │                │
        └──────────────────┘ └─────────────┘ └────────────────┘
                    │               │               │
                    └───────────────┼───────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
        ┌──────────────────┐ ┌─────────────┐ ┌────────────────┐
        │ DASHBOARD        │ │ COMPONENTS  │ │ VISUALIZATIONS │
        │ DBRD_Validations │ │ Labels      │ │ BIView Charts  │
        │ VAL_LandingPage  │ │ Buttons     │ │ Grids          │
        │ VAL_Results      │ │ Grids       │ │                │
        └──────────────────┘ └─────────────┘ └────────────────┘
                                    │
                                    ▼
                    ┌───────────────────────────────┐
                    │        USER INTERFACE         │
                    │                               │
                    │  View → Configure → Execute  │
                    │    → Review Results → Resolve │
                    └───────────────────────────────┘
```

## Color Scheme and Icons

### Status Indicators
- ✓ Green - Validation passed / Active
- ⚠ Yellow - Warning / Issues found
- ✗ Red - Critical issues / Failed
- ○ Gray - Inactive / Not run
- ▶ Blue - In progress

### Severity Colors
- **Critical**: Red background (#FFE5E5), Dark red text (#8B0000)
- **Warning**: Orange background (#FFF5E5), Dark orange text (#FF8C00)
- **Info**: Blue background (#E5F3FF), Dark blue text (#00008B)

### KPI Card Colors
- **Active Validations**: Light blue (#F0F8FF), Dark blue border/text
- **Total Issues**: Light orange (#FFF5E6), Dark orange border/text
- **Critical Issues**: Light red (#FFF0F0), Dark red border/text

## Responsive Behavior

### Desktop View (>1200px)
- 4 KPI cards in row at top
- Full-width chart below
- Configuration: 30% left panel, 70% right panel
- Results: 3-panel stacked layout

### Tablet View (768-1200px)
- 2 KPI cards per row
- Condensed chart
- Configuration: 40% left panel, 60% right panel
- Results: 2-panel layout (filters collapse)

### Mobile View (<768px)
- 1 KPI card per row
- Simplified chart
- Configuration: Single column, tabs instead of panels
- Results: Single column with expandable sections

## Accessibility Features

1. **Keyboard Navigation**: Full keyboard support for all interactive elements
2. **Screen Reader**: Proper ARIA labels on all components
3. **Color Contrast**: WCAG AA compliant contrast ratios
4. **Focus Indicators**: Clear visual indication of focused elements
5. **Error Messages**: Clear, actionable error messages with context

## Interactive Elements

### Drill-Down Paths
1. **From KPI Cards**: Click → Navigate to detailed results view filtered by that metric
2. **From Chart**: Click segment → Navigate to results filtered by that dimension/type
3. **From Results Grid**: Click row → Expand to show issue details
4. **From Issue Details**: Click member → Navigate to member maintenance
5. **From Manage Button**: Navigate to configuration dashboard

### Tooltips
- All KPI cards show detailed descriptions on hover
- Chart segments show count breakdown on hover
- Grid cells show full content on hover for truncated text
- Buttons show action descriptions on hover

## Implementation Notes

### OneStream-Specific Considerations
1. Components use standard OneStream grid layout system
2. All formatting uses OneStream display format syntax
3. Parameters use SQL adapter pattern for dynamic data
4. Navigation uses standard OneStream navigation methods
5. Business rules follow OneStream BR pattern

### Performance Optimization
1. SQL queries include appropriate indexes
2. Large result sets use pagination
3. Charts limited to reasonable data points
4. Lazy loading for configuration details
5. Cached parameter values where appropriate

## Future Enhancements

### Phase 2 Features
- Real-time validation status updates
- Email notifications for failures
- Automated scheduling interface
- Export to multiple formats (PDF, CSV, Excel)
- Custom report templates

### Phase 3 Features
- Machine learning for issue prediction
- Automated resolution suggestions
- Integration with workflow approval
- Mobile app support
- Advanced analytics dashboard

## Summary

This UI layout provides:
- **Clear visual hierarchy** with KPIs at top
- **Intuitive navigation** between view, configure, and results
- **Comprehensive details** available through drill-down
- **Action-oriented design** with clear next steps
- **Scalable structure** supporting future enhancements
- **Consistent styling** following OneStream patterns
- **Accessible design** meeting WCAG standards
