# DDL Files for FMM and DDM Tables

This repository now contains DDL (Data Definition Language) scripts for creating database tables used by the Finance Model Manager (FMM) and Dynamic Dashboard Manager (DDM) modules.

## Files Created

### 1. DDL_FMM_Tables.sql
Finance Model Manager database tables including:

#### RegPlan Table
- **Purpose**: Master table for regulatory planning data
- **Primary Key**: RegPlanID (UNIQUEIDENTIFIER)
- **Key Features**:
  - 20 dynamic text attributes (Attr1-Attr20)
  - 12 dynamic decimal attributes (AttrVal1-AttrVal12)
  - 5 dynamic date attributes (DateVal1-DateVal5)
  - Workflow integration (WFScenarioName, WFProfileName, WFTimeName)
  - Spread configuration (SpreadAmount, SpreadCurve)
  - Status tracking and audit fields

#### RegPlan_Details Table
- **Purpose**: Detail/line items for regulatory plans with time-series data
- **Primary Key**: Composite (RegPlanID, Year, PlanUnits, Account)
- **Key Features**:
  - Monthly data columns (Month1-Month12)
  - Quarterly data columns (Quarter1-Quarter4)
  - Yearly data column
  - 8 user-defined fields (UD1-UD8)
  - Planning dimensions (Entity, Account, Flow)
  - Foreign key to RegPlan with CASCADE delete

#### RegPlan_Audit Table
- **Purpose**: Audit trail for tracking changes to regulatory plans
- **Primary Key**: AuditID (IDENTITY)
- **Key Features**:
  - Automatic audit timestamp (AuditDate with DEFAULT GETDATE())
  - Same attribute structure as RegPlan for change tracking
  - Optimized for audit queries with indexes on RegisterPlanID and AuditDate

### 2. DDL_DDM_Tables.sql
Dynamic Dashboard Manager configuration tables including:

#### DDM_Config Table
- **Purpose**: Main configuration metadata for dynamic dashboards
- **Primary Key**: DDM_Config_ID (IDENTITY)
- **Key Features**:
  - DDM_Type: 0=None, 1=WFProfile, 2=StandAlone
  - Profile_Key: Link to OneStream WorkflowProfileHierarchy
  - Scenario Type configuration
  - Audit fields for tracking changes

#### DDM_Config_Menu_Layout Table
- **Purpose**: Menu structure and layout configurations
- **Primary Key**: DDM_Menu_ID (IDENTITY)
- **Key Features**:
  - Layout_Type enumeration (Dashboard, CubeView, various split layouts)
  - Workspace/MaintUnit/Dashboard references
  - Flexible layout configuration columns (DB_Name, CV_Name, sizing, content types)
  - Foreign key to DDM_Config with CASCADE delete

#### DDM_Config_Hdr_Ctrls Table
- **Purpose**: Header controls (filters, buttons) configuration
- **Primary Key**: DDM_Hdr_Ctrl_ID (IDENTITY)
- **Key Features**:
  - Option_Type: 0=None, 1=Filter, 2=Button
  - Sort order for control display
  - Foreign keys to both DDM_Config and DDM_Config_Menu_Layout with CASCADE delete

## Database Design Decisions

### Denormalization in RegPlan_Details
The RegPlan_Details table includes dimensional columns (WFScenarioName, WFProfileName, etc.) that are also in RegPlan. This intentional denormalization:
- Optimizes query performance for reporting scenarios
- Follows OneStream patterns for detail tables
- Reduces JOIN operations in common query patterns

### Cascade Delete Behavior
The DDM tables use CASCADE delete to maintain referential integrity:
- Deleting DDM_Config removes all child Menu_Layout and Hdr_Ctrls records
- Deleting Menu_Layout removes all child Hdr_Ctrls records
- This ensures complete cleanup of configuration hierarchies

## Performance Optimizations

### Indexes Created
Both DDL files include strategic indexes for common query patterns:

**FMM Indexes:**
- RegPlan workflow lookups (WFScenarioName, WFProfileName, WFTimeName)
- RegPlan Entity and Status filters
- RegPlan_Details by RegPlanID and Year
- RegPlan_Audit by RegisterPlanID, AuditDate, and WFScenario

**DDM Indexes:**
- DDM_Config by Profile_Key and DDM_Type
- DDM_Config_Menu_Layout by DDM_Config_ID, Sort_Order, and Profile_Key
- DDM_Config_Hdr_Ctrls by DDM_Menu_ID, DDM_Config_ID, and Sort_Order

## Usage Instructions

1. **Review the scripts**: Examine both DDL files to ensure they match your environment requirements
2. **Modify if needed**: Adjust data types, sizes, or add environment-specific customizations
3. **Execute in order**: 
   - Run DDL_FMM_Tables.sql first if using FMM
   - Run DDL_DDM_Tables.sql first if using DDM
   - Each file is independent and can be run separately
4. **Verify creation**: Check that tables, foreign keys, and indexes were created successfully
5. **Test permissions**: Ensure application users have appropriate permissions on the new tables

## Compatibility

- **Database**: Microsoft SQL Server
- **OneStream Integration**: Tables are designed to work with OneStream XF platform
- **SQL Adapters**: Table structures match the SQL adapters in the codebase:
  - FMM: SQA_RegPlan.cs, SQA_RegPlan_Details.cs, SQA_RegPlan_Audit.cs
  - DDM: DDM_Config_Data.cs, DDM_Config_Migration.cs

## Support

For questions or issues with these DDL scripts:
1. Review the corresponding SQL adapter code in the Assemblies directory
2. Check OneStream XF documentation for database integration patterns
3. Verify that column names and data types match your OneStream configuration
