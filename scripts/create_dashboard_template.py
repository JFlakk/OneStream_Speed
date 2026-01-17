#!/usr/bin/env python3
"""
OneStream Dashboard Excel Template Generator
Creates an Excel template for designing dashboard wireframes

Usage:
    python create_dashboard_template.py

This script generates an Excel template that can be filled out to design
OneStream dashboards. The completed template can then be converted to XML
using the convert_excel_to_dashboard.py script.
"""

import csv
import os
from datetime import datetime

def create_csv_template(base_name, sheets_data):
    """Create CSV files for each sheet in the template.
    
    Args:
        base_name: Base name for the template directory
        sheets_data: List of tuples, each containing:
            - sheet_name (str): Name of the CSV file (without .csv extension)
            - headers (list): List of column header names
            - sample_data (list of lists): Sample rows of data
            
    Returns:
        str: Path to the created template directory
    """
    output_dir = "templates"
    os.makedirs(output_dir, exist_ok=True)
    
    template_dir = os.path.join(output_dir, base_name)
    os.makedirs(template_dir, exist_ok=True)
    
    for sheet_name, headers, sample_data in sheets_data:
        filename = os.path.join(template_dir, f"{sheet_name}.csv")
        with open(filename, 'w', newline='', encoding='utf-8') as f:
            writer = csv.writer(f)
            writer.writerow(headers)
            for row in sample_data:
                writer.writerow(row)
    
    return template_dir

def main():
    """Generate the dashboard template."""
    print("=== OneStream Dashboard Excel Template Generator ===\n")
    
    template_name = "Dashboard_Wireframe_Template"
    
    # Define all sheets and their structure
    sheets_data = [
        # Sheet 1: Instructions
        ("1_Instructions", 
         ["Section", "Content"],
         [
             ["Overview", "This template allows you to design OneStream dashboards in Excel and convert them to XML format."],
             ["", ""],
             ["How to Use", "1. Fill out the Basic_Info sheet with workspace and maintenance unit details"],
             ["", "2. Define parameters in the Parameters sheet (if needed)"],
             ["", "3. Define components in the Components sheet"],
             ["", "4. Design your dashboard layout in the Dashboard_Layout sheet"],
             ["", "5. Map components to positions in the Component_Positioning sheet"],
             ["", "6. Save the file and run: python scripts/convert_excel_to_dashboard.py your_file.xlsx"],
             ["", ""],
             ["Tips", "- Start simple with a basic layout"],
             ["", "- Use the sample data as a reference"],
             ["", "- You can have multiple dashboards in one template"],
             ["", "- Delete sample rows before adding your own data"],
             ["", ""],
             ["Component Types", "Button, ComboBox, TextBox, Logo, CubeView, EmbeddedDashboard, SuppliedParameter, SqlTableEditor"],
             ["Parameter Types", "InputValue, DelimitedList, LiteralValue"],
             ["Dashboard Types", "TopLevel, EmbeddedTopLevelWithoutParameterPrompts, Embedded, Unknown"],
             ["Layout Types", "Grid, Uniform, HorizontalStackPanel, VerticalStackPanel"],
         ]),
        
        # Sheet 2: Basic Info
        ("2_Basic_Info",
         ["Setting", "Value"],
         [
             ["Workspace_Name", "00 GBL"],
             ["Namespace_Prefix", "GBL"],
             ["Maintenance_Unit_Name", "Test Dashboard"],
             ["Dashboard_Group_Name", "Test Group"],
             ["Access_Group", "Everyone"],
             ["Maintenance_Group", "Everyone"],
             ["UI_Platform_Type", "WpfOrSilverlight"],
         ]),
        
        # Sheet 3: Parameters
        ("3_Parameters",
         ["Parameter_Name", "Parameter_Type", "Description", "User_Prompt", "Default_Value", "Display_Items", "Value_Items"],
         [
             ["# Example parameters - delete these rows and add your own", "", "", "", "", "", ""],
             ["DL_Test_Options", "DelimitedList", "Test options list", "Select an option", "", "Option A,Option B,Option C", "optA,optB,optC"],
             ["IV_User_Input", "InputValue", "User text input", "Enter value", "", "", ""],
             ["LV_Static_Value", "LiteralValue", "Static display value", "", "Welcome Message", "", ""],
         ]),
        
        # Sheet 4: Components
        ("4_Components",
         ["Component_Name", "Component_Type", "Description", "XF_Text", "Tool_Tip", "Bound_Parameter", "Extra_Property_1", "Extra_Value_1", "Extra_Property_2", "Extra_Value_2"],
         [
             ["# Example components - delete these rows and add your own", "", "", "", "", "", "", "", "", ""],
             ["cbx_Test_Selection", "ComboBox", "Option selector", "Select an option:", "Choose from available options", "DL_Test_Options", "dashboardsToRedraw", "Test_Main_Dashboard", "uiActionType", "Refresh"],
             ["btn_Submit", "Button", "Submit button", "Submit", "Click to submit", "", "buttonType", "Standard", "taskType", "ExecuteDashboardExtenderBRAllActions"],
             ["txt_User_Input", "TextBox", "Text input field", "Enter value:", "Type here", "IV_User_Input", "", "", "", ""],
             ["Embedded_Content", "EmbeddedDashboard", "Content area", "", "", "", "embeddedDashboardName", "Test_Content_Dashboard", "", ""],
         ]),
        
        # Sheet 5: Dashboard Layout
        ("5_Dashboard_Layout",
         ["Dashboard_Name", "Dashboard_Type", "Layout_Type", "Description", "Is_Initially_Visible", "Load_Task_Type", "Load_Task_Args", "Grid_Columns", "Grid_Rows"],
         [
             ["# Example dashboard - delete this row and add your own", "", "", "", "", "", "", "", ""],
             ["Test_Main_Dashboard", "TopLevel", "Grid", "Main test dashboard", "Yes", "NoTask", "", "1:Component:*", "2:Component:Auto;Component:*"],
             ["Test_Content_Dashboard", "Embedded", "Grid", "Content dashboard", "Yes", "NoTask", "", "1:Component:*", "1:Component:*"],
         ]),
        
        # Sheet 6: Component Positioning
        ("6_Component_Positioning",
         ["Dashboard_Name", "Component_Name", "Row", "Column", "Dock_Position", "Width", "Height"],
         [
             ["# Example positioning - delete these rows and add your own", "", "", "", "", "", ""],
             ["Test_Main_Dashboard", "cbx_Test_Selection", "0", "0", "Left", "", ""],
             ["Test_Main_Dashboard", "Embedded_Content", "1", "0", "Left", "", ""],
             ["Test_Content_Dashboard", "txt_User_Input", "0", "0", "Left", "", ""],
         ]),
        
        # Sheet 7: Grid Layout Visual (for reference)
        ("7_Grid_Visual_Guide",
         ["Info", "Col_0", "Col_1", "Col_2", "Col_3"],
         [
             ["This sheet is for visual reference only - sketch your layout here", "", "", "", ""],
             ["", "", "", "", ""],
             ["Row_0", "Component Name", "", "", ""],
             ["Row_1", "Component Name", "Component Name", "", ""],
             ["Row_2", "Component Name", "", "", ""],
             ["", "", "", "", ""],
             ["Instructions:", "", "", "", ""],
             ["1. Use this grid to visualize your dashboard layout", "", "", "", ""],
             ["2. Write component names in cells where they should appear", "", "", "", ""],
             ["3. Merge cells visually to show component span", "", "", "", ""],
             ["4. Use this as reference when filling Component_Positioning sheet", "", "", "", ""],
         ]),
    ]
    
    # Create the template
    print(f"Creating template: {template_name}")
    template_dir = create_csv_template(template_name, sheets_data)
    
    print(f"\n✓ Template created successfully in: {template_dir}/")
    print(f"\nThe template consists of CSV files (one per sheet):")
    print("  1. Instructions - How to use the template")
    print("  2. Basic_Info - Workspace and maintenance unit settings")
    print("  3. Parameters - Dashboard parameters definition")
    print("  4. Components - Dashboard components definition")
    print("  5. Dashboard_Layout - Dashboard structure and grid layout")
    print("  6. Component_Positioning - Component placement in dashboards")
    print("  7. Grid_Visual_Guide - Visual reference for layout design")
    
    print(f"\nNext steps:")
    print(f"1. Open the CSV files in Excel or your preferred spreadsheet application")
    print(f"2. You can import all CSV files into a single Excel workbook (one per sheet)")
    print(f"3. Fill out the template with your dashboard design")
    print(f"4. Save as Excel (.xlsx) or keep as CSV files")
    print(f"5. Run: python scripts/convert_excel_to_dashboard.py <your_template>")
    
    # Create a readme for the template
    readme_path = os.path.join(template_dir, "README.txt")
    with open(readme_path, 'w', encoding='utf-8') as f:
        f.write("OneStream Dashboard Wireframe Template\n")
        f.write("=" * 50 + "\n\n")
        f.write("This template helps you design OneStream dashboards.\n\n")
        f.write("Files:\n")
        f.write("  1_Instructions.csv - Usage instructions\n")
        f.write("  2_Basic_Info.csv - Workspace settings\n")
        f.write("  3_Parameters.csv - Parameter definitions\n")
        f.write("  4_Components.csv - Component definitions\n")
        f.write("  5_Dashboard_Layout.csv - Dashboard layout structure\n")
        f.write("  6_Component_Positioning.csv - Component placement\n")
        f.write("  7_Grid_Visual_Guide.csv - Layout visualization\n\n")
        f.write("To use:\n")
        f.write("1. Open these CSV files in Excel (or import into one workbook)\n")
        f.write("2. Fill in your dashboard design\n")
        f.write("3. Save the workbook as .xlsx\n")
        f.write("4. Run: python scripts/convert_excel_to_dashboard.py your_file.xlsx\n\n")
        f.write(f"Created: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
    
    print(f"\n✓ README created: {readme_path}")

if __name__ == "__main__":
    try:
        main()
    except KeyboardInterrupt:
        print("\n\nOperation cancelled by user.")
    except Exception as e:
        print(f"\nError: {e}")
        import traceback
        traceback.print_exc()
