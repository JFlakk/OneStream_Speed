# Dashboard XML Generation Scripts

This directory contains utilities for generating OneStream Dashboard XML files.

## Available Scripts

### generate_simple_dashboard.py

A Python script that generates a basic Dashboard XML structure through an interactive prompt.

**Prerequisites:**
- Python 3.6 or higher
- No additional packages required (uses standard library)

**Usage:**

```bash
# From the repository root directory
python scripts/generate_simple_dashboard.py
```

The script will prompt you for:
- Workspace Name
- Namespace Prefix
- Maintenance Unit Name
- Dashboard Group Name
- Dashboard Name

It will then generate a basic Dashboard XML file in the `GeneratedXML/` directory.

**Output:**

The generated XML file will include:
- A workspace structure
- A maintenance unit
- Sample parameter definitions
- Sample component definitions
- A dashboard group
- A basic dashboard with grid layout
- Proper XML formatting for OneStream import

**Customization:**

The generated file is a starting point. You can:
1. Open the generated XML file in a text editor
2. Add additional parameters, components, or dashboards
3. Modify layout configurations
4. Add business rule references
5. Configure component properties

**Example:**

```bash
$ python scripts/generate_simple_dashboard.py
=== OneStream Simple Dashboard Generator ===

Workspace Name (e.g., '00 GBL'): 00 TEST
Namespace Prefix (e.g., 'GBL'): TEST
Maintenance Unit Name (e.g., 'Test Dashboard'): My Dashboard
Dashboard Group Name (e.g., 'Test Group'): My Group
Dashboard Name (e.g., 'Test_Main'): MyDash_Main

Generating XML file: GeneratedXML/00_TEST_My_Dashboard.xml
✓ Dashboard XML generated successfully!

Next steps:
1. Review the generated XML at: GeneratedXML/00_TEST_My_Dashboard.xml
2. Modify as needed for your specific requirements
3. Import into OneStream using the Code Utility extension
4. Test in OneStream platform
```

## Manual Dashboard Generation

If you prefer to request custom Dashboard XML generation:

1. Review the documentation: `Documentation/Dashboard_XML_Generation_Guide.md`
2. Fill out the template: `Documentation/Dashboard_Generation_Request_Template.md`
3. Provide the specifications to have the XML generated for you

## Dashboard XML Structure Reference

For understanding the XML structure, refer to existing dashboards in:
- `obj/PPBE_Workspaces/00 GBL/GBL Dashboard/WS Extracts/GBL Dashboard.xml`
- `obj/PPBE_Workspaces/00 GBL/GBL Admin Dashboards/WS Extracts/GBL Admin Dashboards.xml`

These contain complex, real-world examples of dashboard configurations.

## Tips

1. **Start Simple**: Begin with a basic dashboard structure and add complexity incrementally
2. **Test Frequently**: Import and test in OneStream after each modification
3. **Use Examples**: Reference existing dashboard XML files for patterns and structures
4. **Version Control**: Keep your generated XML files in version control
5. **Documentation**: Document your dashboard configurations for future reference

## Troubleshooting

**Import Errors:**
- Ensure XML is well-formed (use an XML validator)
- Check that all referenced components exist
- Verify workspace and maintenance unit names match

**Dashboard Not Appearing:**
- Check access group permissions
- Verify dashboard is marked as initially visible
- Ensure it's in the correct dashboard group

**Components Not Working:**
- Verify bound parameter names match parameter definitions
- Check that embedded dashboard names reference existing dashboards
- Ensure component types and properties are valid

## Additional Resources

- OneStream Documentation: See official OneStream documentation for component types
- Code Utility Extension: Use the VS Code extension for import/export
- Repository Examples: Study existing dashboard XML files in the `obj/` directory
