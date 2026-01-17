# How to Create an Excel Workbook from the Template

The Dashboard Wireframe Template is provided as CSV files for maximum compatibility. Here's how to create an Excel workbook from these files:

## Method 1: Using Excel (Recommended)

### Step 1: Create New Workbook
1. Open Microsoft Excel
2. Create a new blank workbook

### Step 2: Import Each CSV File as a Sheet
For each CSV file in the template directory:

1. Go to **Data** tab → **Get Data** → **From File** → **From Text/CSV**
   - Or: **Data** tab → **From Text/CSV** (depending on Excel version)

2. Navigate to `templates/Dashboard_Wireframe_Template/`

3. Select the first CSV file: `1_Instructions.csv`

4. In the import preview:
   - File Origin: **Unicode (UTF-8)**
   - Delimiter: **Comma**
   - Click **Load**

5. Rename the imported table/query to match the sheet name
   - Right-click the sheet tab → **Rename**
   - Remove the "1_" prefix (e.g., "Instructions" instead of "1_Instructions")

6. Repeat steps 3-5 for all CSV files:
   - `1_Instructions.csv` → Sheet name: "Instructions"
   - `2_Basic_Info.csv` → Sheet name: "Basic_Info"
   - `3_Parameters.csv` → Sheet name: "Parameters"
   - `4_Components.csv` → Sheet name: "Components"
   - `5_Dashboard_Layout.csv` → Sheet name: "Dashboard_Layout"
   - `6_Component_Positioning.csv` → Sheet name: "Component_Positioning"
   - `7_Grid_Visual_Guide.csv` → Sheet name: "Grid_Visual_Guide"

### Step 3: Format and Edit
1. Adjust column widths for readability
2. Apply cell formatting (bold headers, colors, etc.) as desired
3. Delete the sample/example rows (marked with #)
4. Fill in your dashboard design

### Step 4: Save
1. **File** → **Save As**
2. Choose location
3. File name: `My_Dashboard_Design.xlsx`
4. Save as type: **Excel Workbook (*.xlsx)**

### Step 5: Export Back to CSV (if needed)
If the converter doesn't support .xlsx yet:
1. Create a folder: `my_dashboard_template/`
2. For each sheet:
   - Click the sheet tab
   - **File** → **Save As**
   - Navigate to your folder
   - Save as type: **CSV UTF-8 (Comma delimited) (*.csv)**
   - Name it with the number prefix: `1_Instructions.csv`, `2_Basic_Info.csv`, etc.

## Method 2: Using Google Sheets

### Step 1: Upload CSV Files
1. Go to Google Drive
2. Create a new Google Sheet
3. **File** → **Import**
4. Upload each CSV file
5. Import each as a separate sheet

### Step 2: Edit and Save
1. Edit your dashboard design in the sheets
2. **File** → **Download** → **Microsoft Excel (.xlsx)**

### Step 3: Convert
If the converter needs CSV format:
- **File** → **Download** → **Comma Separated Values (.csv)**
- Do this for each sheet, saving to a folder

## Method 3: Using LibreOffice Calc

### Step 1: Open All CSVs
1. Open LibreOffice Calc
2. **File** → **Open**
3. Select all CSV files (hold Ctrl to multi-select)
4. Each opens in a separate window/tab

### Step 2: Combine into One Workbook
1. Create a new blank spreadsheet
2. Copy each CSV content into a new sheet
3. Name each sheet appropriately

### Step 3: Save as Excel
1. **File** → **Save As**
2. File type: **Microsoft Excel 2007-365 (.xlsx)**
3. Save your file

## Method 4: Python Script (Advanced)

If you have openpyxl installed, you can convert CSV to Excel automatically:

```python
import csv
import os
from openpyxl import Workbook

def csv_to_excel(csv_dir, output_file):
    wb = Workbook()
    wb.remove(wb.active)  # Remove default sheet
    
    csv_files = sorted([f for f in os.listdir(csv_dir) if f.endswith('.csv')])
    
    for csv_file in csv_files:
        sheet_name = csv_file.replace('.csv', '').split('_', 1)[1]  # Remove number prefix
        ws = wb.create_sheet(sheet_name)
        
        with open(os.path.join(csv_dir, csv_file), 'r', encoding='utf-8') as f:
            reader = csv.reader(f)
            for row in reader:
                ws.append(row)
    
    wb.save(output_file)
    print(f"Created: {output_file}")

# Usage
csv_to_excel('templates/Dashboard_Wireframe_Template', 'Dashboard_Template.xlsx')
```

## Tips for Excel Editing

### Formatting Recommendations
1. **Headers**: Bold, filled background color
2. **Instructions Sheet**: Merge cells for better readability
3. **Grid Visual Guide**: Merge cells to show component spans visually
4. **Freeze Panes**: Freeze the header row for easier scrolling
   - Select row 2 → **View** → **Freeze Panes** → **Freeze Top Row**

### Data Validation (Optional)
Add dropdown lists for common fields:

1. **Parameter_Type** column:
   - Select cells in the column
   - **Data** → **Data Validation**
   - Allow: **List**
   - Source: `InputValue,DelimitedList,LiteralValue`

2. **Component_Type** column:
   - Source: `Button,ComboBox,TextBox,EmbeddedDashboard,CubeView,Logo,SuppliedParameter`

3. **Dashboard_Type** column:
   - Source: `TopLevel,Embedded,EmbeddedTopLevelWithoutParameterPrompts`

4. **Layout_Type** column:
   - Source: `Grid,Uniform,HorizontalStackPanel,VerticalStackPanel`

### Conditional Formatting (Optional)
Highlight completed sections:
1. Select a column (e.g., Component_Name)
2. **Home** → **Conditional Formatting** → **Highlight Cell Rules** → **Text that Contains**
3. Format cells that contain text (non-empty) with a color

### Table Format (Recommended)
Convert each sheet to an Excel Table:
1. Select any cell in the data range
2. **Insert** → **Table** (or Ctrl+T)
3. Check "My table has headers"
4. Click OK

Benefits:
- Auto-filters on headers
- Easier to add rows
- Better visual structure

## Verification

Before converting to XML:
1. Check all sheets have proper headers
2. Remove example rows (those starting with #)
3. Verify no empty rows in the middle of data
4. Ensure Parameter_Name references match between sheets
5. Confirm Dashboard_Name references match between layouts and positioning

## Converting Your Excel File

Once you have your Excel workbook ready:

### If Using CSV Files:
```bash
python scripts/convert_excel_to_dashboard.py templates/Dashboard_Wireframe_Template/
```

### If You Have .xlsx (Future Support):
```bash
python scripts/convert_excel_to_dashboard.py My_Dashboard_Design.xlsx
```

Currently, the converter works with CSV files. If you've created an Excel file:
1. Export each sheet back to CSV (see Step 5 in Method 1)
2. Place them in a folder
3. Run the converter on that folder

## Example Workflow

1. Run: `python scripts/create_dashboard_template.py`
2. Open Excel
3. Import all CSV files as sheets in one workbook
4. Format and edit to design your dashboard
5. Save as `My_Dashboard.xlsx`
6. Export sheets back to CSV in a new folder (e.g., `my_dashboard/`)
7. Run: `python scripts/convert_excel_to_dashboard.py my_dashboard/`
8. Result: `GeneratedXML/[YourWorkspace]_[YourMaintenanceUnit].xml`

## Troubleshooting

**Issue: Import shows garbled characters**
- Solution: Ensure UTF-8 encoding is selected during import

**Issue: Delimiter not recognized**
- Solution: Verify comma is selected as delimiter

**Issue: Can't convert Excel file**
- Current limitation: Use CSV export method
- Future: Direct .xlsx support planned

**Issue: Data not aligned after import**
- Solution: Check that delimiter is comma, not semicolon or tab

## Next Steps

After creating your Excel workbook and converting to XML:
1. Review the generated XML file
2. Import into OneStream using Code Utility extension
3. Test the dashboard in OneStream
4. Iterate as needed (edit Excel, regenerate XML, reimport)
