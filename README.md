# OneStream Code Repository

## Folder Structure

This repository follows the OneStream rule type organization used in XML exports:

### Business Rules by Type
- **Assemblies/** - .NET assemblies and DLL files (ApplicationWorkspaces.xml)
- **Extender/** - Extender business rules (ExtensibilityRules.xml)
- **Finance/** - Finance business rules (FinanceRules.xml)
- **Connector/** - Data source connectors (DataSources.xml)
- **Parser/** - Data parsers (DataSources.xml)

### Dashboard Components
- **DashboardExtender/** - Dashboard extender rules (ApplicationWorkspaces.xml)
- **DashboardDataSet/** - Dashboard data set rules (ApplicationWorkspaces.xml)
- **DashboardStringFunction/** - Dashboard string functions (ApplicationWorkspaces.xml)
- **Spreadsheet/** - Spreadsheet rules (ApplicationWorkspaces.xml)

### Event Handlers
- **DataQualityEventHandler/** - Data quality event handlers (ExtensibilityRules.xml)
- **TransformationEventHandler/** - Transformation event handlers (ExtensibilityRules.xml)
- **DataManagementEventHandler/** - Data management event handlers (ExtensibilityRules.xml)
- **FormsEventHandler/** - Forms event handlers (ExtensibilityRules.xml)
- **WcfEventHandler/** - WCF event handlers (ExtensibilityRules.xml)

### Other Rule Types
- **SmartIntegrationFunction/** - Smart integration functions (SmartIntegration.xml)
- **CubeViewExtender/** - Cube view extenders (CubeViews.xml)
- **ConditionalRule/** - Conditional rules (TransformationRules.xml)
- **TransformationRule/** - Transformation rules (TransformationRules.xml)

### Utility Folders
- **GeneratedXML/** - XML files generated for export
- **Documentation/** - Project documentation
  - Dashboard XML Generation Guide - Comprehensive guide for Dashboard XML generation
  - Dashboard Generation Request Template - Template for requesting new dashboards
- **Resources/** - Additional resources
- **Scripts/** - Utility scripts
  - **scripts/generate_simple_dashboard.py** - Python script to generate basic dashboard XML
  - **Scripts/SQL/** - SQL scripts
  - **Scripts/PowerShell/** - PowerShell scripts
- **Tests/** - Test files and scripts

## Usage

1. Place your OneStream code files in the appropriate folders
2. Use the Code Utility for OneStream VS Code extension to manage exports/imports
3. Maintain version control with Git

## Dashboard XML Generation

This repository includes tools and documentation for generating Dashboard objects in XML format.

### Quick Start Options

#### Option 1: Excel Template Approach (Recommended for Complex Dashboards)

Design your dashboard in a spreadsheet format and convert to XML:

```bash
# 1. Generate the Excel/CSV template
python scripts/create_dashboard_template.py

# 2. Edit the template files in: templates/Dashboard_Wireframe_Template/
#    (Open CSV files in Excel or import them as sheets in a workbook)

# 3. Convert to XML
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/
```

The template provides a structured way to:
- Design dashboard layouts visually
- Define parameters and components in a table format
- Map component positioning with clear row/column indices
- See your dashboard structure at a glance

See **[Excel Template Guide](Documentation/Excel_Template_Guide.md)** for detailed instructions.

#### Option 2: Interactive Script (Quick and Simple)

Generate a basic dashboard using the interactive Python script:

```bash
python scripts/generate_simple_dashboard.py
```

### Documentation

- **[Excel Template Guide](Documentation/Excel_Template_Guide.md)** - NEW! Complete guide for the spreadsheet-based wireframe approach
- **[Dashboard XML Generation Guide](Documentation/Dashboard_XML_Generation_Guide.md)** - Complete guide on Dashboard XML structure and generation
- **[Dashboard Generation Request Template](Documentation/Dashboard_Generation_Request_Template.md)** - Template for specifying dashboard requirements

### What Information is Needed?

To generate a Dashboard XML file, you need:

1. **Workspace Information**: Name, namespace prefix, access groups
2. **Maintenance Unit Details**: Name and configuration
3. **Dashboard Specifications**: Type, layout, components
4. **Parameters**: Input values, delimited lists, literal values
5. **Components**: Buttons, combo boxes, embedded dashboards, cube views, etc.
6. **Layout Configuration**: Grid definitions with rows and columns
7. **Business Rules** (optional): Dashboard extenders, string functions, data sets

### Example Dashboard Structures

Reference examples can be found in:
- `obj/PPBE_Workspaces/00 GBL/GBL Dashboard/` - Main dashboard examples
- `obj/PPBE_Workspaces/00 GBL/GBL Admin Dashboards/` - Administrative dashboard examples

See the [Dashboard XML Generation Guide](Documentation/Dashboard_XML_Generation_Guide.md) for detailed information.

## Code Utility for OneStream Extension

This project is designed to work with the Code Utility for OneStream VS Code extension.

### Features:
- Multi-select rule export
- XML generation
- Smart ZIP packaging
- Import and organization
- Metadata management

Generated by Code Utility for OneStream