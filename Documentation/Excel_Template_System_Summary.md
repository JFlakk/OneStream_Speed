# Excel Template System - Project Summary

## Overview

This project adds a comprehensive Excel/CSV template system for designing OneStream dashboard wireframes that can be easily converted to XML format. This was built in response to PR #17's dashboard XML generation capabilities, providing a more visual and user-friendly approach to dashboard design.

## What Problem Does This Solve?

Previously, creating OneStream dashboards required either:
1. Writing XML by hand (error-prone, tedious)
2. Using an interactive script with limited features
3. Filling out a text-based request template

This solution provides:
- **Visual design** in familiar spreadsheet format
- **Structured approach** with clear sections for each dashboard aspect
- **Easy editing** in Excel or any spreadsheet application
- **Quick iteration** - edit template, regenerate XML, reimport
- **No XML knowledge required** - all complexity handled by converter

## Components

### 1. Scripts

#### `create_dashboard_template.py`
- Generates CSV template structure with 7 sheets
- Creates sample data for reference
- No dependencies (standard library only)
- Output: `templates/Dashboard_Wireframe_Template/`

**Usage:**
```bash
python scripts/create_dashboard_template.py
```

#### `convert_excel_to_dashboard.py`
- Converts filled CSV template to OneStream XML
- Validates structure and data
- Sanitizes filenames for cross-platform compatibility
- Generates well-formed XML ready for import
- No dependencies (standard library only)
- Output: `GeneratedXML/[Workspace]_[MaintenanceUnit].xml`

**Usage:**
```bash
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/
```

### 2. Template Structure

The template consists of 7 CSV files designed to be imported as sheets in Excel:

1. **Instructions** - Usage guide and reference
2. **Basic_Info** - Workspace and maintenance unit settings
3. **Parameters** - Dashboard parameter definitions (InputValue, DelimitedList, LiteralValue)
4. **Components** - Component definitions (Button, ComboBox, TextBox, EmbeddedDashboard, etc.)
5. **Dashboard_Layout** - Dashboard structure with grid layout definitions
6. **Component_Positioning** - Component placement with row/column indices
7. **Grid_Visual_Guide** - Visual reference for sketching layouts

### 3. Documentation

#### Quick Start Guide (5 minutes)
- `Documentation/Excel_Template_Quick_Start.md`
- Minimal example for getting started fast
- Step-by-step instructions with common patterns

#### Complete Guide
- `Documentation/Excel_Template_Guide.md`
- Comprehensive reference for all features
- Detailed explanation of each sheet
- Tips and best practices
- Troubleshooting section

#### Visual Examples
- `Documentation/Excel_Template_Examples.md`
- Filled-out example templates
- Common dashboard patterns
- Grid layout visualizations
- Real-world use cases

## Workflow

```
1. Generate Template
   ↓
2. Import CSVs to Excel
   ↓
3. Design Dashboard (fill sheets)
   ↓
4. Save/Export
   ↓
5. Convert to XML
   ↓
6. Import to OneStream
   ↓
7. Test & Iterate
```

## Key Features

### Template Features
- ✅ Pre-structured with all required sheets
- ✅ Sample data for reference
- ✅ Clear headers and labels
- ✅ Comment rows to guide users
- ✅ CSV format (compatible with all spreadsheet apps)

### Converter Features
- ✅ Robust parsing of CSV data
- ✅ Grid layout definition support (multiple formats)
- ✅ Component property mapping
- ✅ Parameter type handling
- ✅ Filename sanitization
- ✅ Clear error messages
- ✅ Well-formed XML output

### Documentation Features
- ✅ Quick start guide (5 min)
- ✅ Complete reference guide
- ✅ Visual examples
- ✅ Common patterns
- ✅ Troubleshooting tips
- ✅ Excel import instructions

## Technical Details

### Language
- Python 3.6+ (tested with 3.12)

### Dependencies
- None (uses standard library only)
- No pip install required
- Works out of the box

### File Formats
- Input: CSV files (can be created/edited in Excel)
- Output: XML (OneStream Dashboard format)

### Compatibility
- Cross-platform (Windows, Mac, Linux)
- Works with Excel, Google Sheets, LibreOffice Calc
- OneStream version 9.0.1+ (XML format)

## Usage Examples

### Example 1: Simple Dashboard
```bash
# Generate template
python scripts/create_dashboard_template.py

# Edit templates/Dashboard_Wireframe_Template/*.csv
# Fill in: workspace name, dashboard layout, components

# Convert
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/

# Result: GeneratedXML/Your_Workspace_Your_Dashboard.xml
```

### Example 2: Complex Multi-Dashboard Layout
```bash
# Same process, but:
# - Add multiple rows in Dashboard_Layout.csv for multiple dashboards
# - Define embedded dashboards
# - Map components to correct dashboard/row/column in Component_Positioning.csv
```

## Benefits Over Manual XML Editing

1. **Easier to understand** - Tabular format is more intuitive
2. **Less error-prone** - Structured input reduces mistakes
3. **Faster iteration** - Edit spreadsheet, regenerate, reimport
4. **Better visualization** - See layout structure at a glance
5. **Team collaboration** - Excel format familiar to all team members
6. **Version control friendly** - CSV files work well with Git

## Benefits Over Interactive Script

1. **More control** - Full access to all properties
2. **Better for complex layouts** - Multiple dashboards, nested components
3. **Reusable** - Save templates for similar dashboards
4. **Visual design** - Sketch in Grid_Visual_Guide sheet
5. **Documentation** - Template serves as documentation

## Future Enhancements

Potential improvements (not implemented yet):

- Direct .xlsx file support (currently requires CSV export)
- Visual grid designer with drag-and-drop UI
- Template validation before XML generation
- Import existing XML back to template format
- Pre-built templates for common patterns
- Excel macros for in-sheet validation

## Integration with Existing Tools

This complements the existing dashboard generation capabilities from PR #17:

- **Interactive Script** (`generate_simple_dashboard.py`) - For quick, simple dashboards
- **Request Template** (`Dashboard_Generation_Request_Template.md`) - For manual requests
- **This Excel System** - For complex, visual dashboard design

Users can choose the approach that best fits their needs.

## Testing

### Test Coverage
- ✅ Template generation
- ✅ CSV parsing
- ✅ XML generation
- ✅ Filename sanitization
- ✅ Grid layout parsing
- ✅ Component properties
- ✅ Parameter definitions
- ✅ Error handling

### Security
- ✅ No vulnerabilities detected (CodeQL scan)
- ✅ Filename sanitization prevents path traversal
- ✅ No code injection risks
- ✅ Safe XML generation

### Quality
- ✅ Code reviewed
- ✅ Documentation complete
- ✅ Examples tested
- ✅ Cross-platform compatibility verified

## Files Added

```
Documentation/
  Excel_Template_Quick_Start.md (NEW)
  Excel_Template_Guide.md (NEW)
  Excel_Template_Examples.md (NEW)

scripts/
  create_dashboard_template.py (NEW)
  convert_excel_to_dashboard.py (NEW)

templates/
  Dashboard_Wireframe_Template/ (NEW)
    1_Instructions.csv
    2_Basic_Info.csv
    3_Parameters.csv
    4_Components.csv
    5_Dashboard_Layout.csv
    6_Component_Positioning.csv
    7_Grid_Visual_Guide.csv
    README.txt

README.md (UPDATED)
scripts/README.md (UPDATED)
```

## Summary

This Excel template system provides a powerful, user-friendly way to design OneStream dashboards visually in a spreadsheet format and convert them to XML. It addresses the request in PR #17 for an easier dashboard creation workflow, making dashboard development more accessible to users who prefer visual design over manual XML editing.

The system is:
- **Complete** - All features implemented and tested
- **Documented** - Comprehensive guides with examples
- **Robust** - Error handling and security validated
- **Easy to use** - 5-minute quick start available
- **Zero dependencies** - Works out of the box with Python

Users now have three options for dashboard generation:
1. Interactive script (quick & simple)
2. Request template (manual/custom)
3. **Excel template (visual & structured)** ← NEW!

This provides flexibility for different use cases and user preferences.
