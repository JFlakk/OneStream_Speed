# Model Dimension Manager - Validation Dashboard Implementation Guide

## Quick Start

This guide provides step-by-step instructions for implementing the enhanced Validation Dashboard in Model Dimension Manager.

## Prerequisites

- OneStream XF Platform version 9.2 or higher
- Model Dimension Manager assembly installed
- SQL Server access with create table permissions
- Administrative access to the application

## Implementation Steps

### Step 1: Database Schema Creation

Execute the following SQL scripts in order on your OneStream application database:

#### 1.1 Create Validation Types Table

```sql
-- Create the master validation types table
CREATE TABLE MDM_DimValidationTypes (
    ValidationTypeID INT PRIMARY KEY IDENTITY(1,1),
    ValidationTypeName NVARCHAR(100) NOT NULL UNIQUE,
    ValidationTypeCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Category NVARCHAR(50),
    IsActive BIT DEFAULT 1,
    RequiresComparison BIT DEFAULT 0,
    SupportsMultipleDimensions BIT DEFAULT 0,
    ConfigurationSchema NVARCHAR(MAX),
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
);

-- Insert standard validation types
INSERT INTO MDM_DimValidationTypes (ValidationTypeName, ValidationTypeCode, Category, Description, IsActive, RequiresComparison, SupportsMultipleDimensions) VALUES
('Hierarchy Completeness', 'HIER_COMPLETENESS', 'Hierarchy', 'Verify all members exist in specified hierarchies', 1, 0, 0),
('Hierarchy Comparison', 'HIER_COMPARISON', 'Hierarchy', 'Compare member presence across two hierarchies', 1, 1, 0),
('Property Population', 'PROP_POPULATION', 'Property', 'Check that required properties are populated', 1, 0, 0),
('Property Value Validation', 'PROP_VALIDATION', 'Property', 'Validate property values against rules', 1, 0, 0),
('Cross-Dimension Reference', 'CROSS_DIM_REF', 'CrossReference', 'Validate references to members in other dimensions', 1, 0, 1),
('Member Uniqueness', 'MEMBER_UNIQUENESS', 'Integrity', 'Check for duplicate members across hierarchies', 1, 0, 0),
('Parent-Child Integrity', 'PARENT_CHILD', 'Integrity', 'Validate parent-child relationships', 1, 0, 0),
('Data Completeness', 'DATA_COMPLETENESS', 'Completeness', 'Verify required data elements are present', 1, 0, 0);
```

#### 1.2 Create Configuration Tables

```sql
-- Create validation configuration table
CREATE TABLE MDM_DimValidationConfig (
    ValidationConfigID INT PRIMARY KEY IDENTITY(1,1),
    ValidationName NVARCHAR(200) NOT NULL,
    ValidationTypeID INT NOT NULL,
    Description NVARCHAR(1000),
    DimID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    Severity NVARCHAR(20) DEFAULT 'Warning',
    ExecutionSchedule NVARCHAR(50) DEFAULT 'OnDemand',
    NotifyOnFailure BIT DEFAULT 0,
    NotificationList NVARCHAR(500),
    LastExecutedDate DATETIME,
    LastExecutedBy NVARCHAR(100),
    LastExecutionStatus NVARCHAR(50),
    LastExecutionDuration INT,
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    CONSTRAINT FK_ValConfig_Type FOREIGN KEY (ValidationTypeID) REFERENCES MDM_DimValidationTypes(ValidationTypeID),
    CONSTRAINT FK_ValConfig_Dim FOREIGN KEY (DimID) REFERENCES Dim(DimID)
);

-- Create validation rules table
CREATE TABLE MDM_DimValidationRules (
    ValidationRuleID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    RuleSequence INT DEFAULT 1,
    RuleType NVARCHAR(50),
    TargetHierarchyID INT,
    ComparisonHierarchyID INT,
    TargetPropertyName NVARCHAR(100),
    ComparisonOperator NVARCHAR(20),
    ComparisonValue NVARCHAR(500),
    MemberFilterExpression NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    ConfigurationJSON NVARCHAR(MAX),
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    CONSTRAINT FK_ValRules_Config FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID) ON DELETE CASCADE,
    CONSTRAINT FK_ValRules_TargetHier FOREIGN KEY (TargetHierarchyID) REFERENCES Member(MemberID),
    CONSTRAINT FK_ValRules_CompHier FOREIGN KEY (ComparisonHierarchyID) REFERENCES Member(MemberID)
);
```

#### 1.3 Create Execution and Results Tables

```sql
-- Create execution history table
CREATE TABLE MDM_DimValidationExecution (
    ExecutionID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    ExecutionDate DATETIME DEFAULT GETDATE(),
    ExecutedBy NVARCHAR(100),
    ExecutionStatus NVARCHAR(50) DEFAULT 'Running',
    StartTime DATETIME,
    EndTime DATETIME,
    DurationMS INT,
    TotalMembersChecked INT DEFAULT 0,
    IssuesFound INT DEFAULT 0,
    CriticalIssues INT DEFAULT 0,
    WarningIssues INT DEFAULT 0,
    InfoIssues INT DEFAULT 0,
    ExecutionLog NVARCHAR(MAX),
    ErrorMessage NVARCHAR(MAX),
    CONSTRAINT FK_ValExec_Config FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID)
);

-- Create results table
CREATE TABLE MDM_DimValidationResults (
    ResultID INT PRIMARY KEY IDENTITY(1,1),
    ExecutionID INT NOT NULL,
    ValidationConfigID INT NOT NULL,
    ValidationRuleID INT,
    DimID INT NOT NULL,
    MemberID INT NOT NULL,
    HierarchyID INT,
    IssueType NVARCHAR(100),
    IssueSeverity NVARCHAR(20) DEFAULT 'Warning',
    IssueDescription NVARCHAR(1000),
    ExpectedValue NVARCHAR(500),
    ActualValue NVARCHAR(500),
    PropertyName NVARCHAR(100),
    ComparisonMemberID INT,
    ResolutionStatus NVARCHAR(50) DEFAULT 'Open',
    ResolvedBy NVARCHAR(100),
    ResolvedDate DATETIME,
    ResolutionNotes NVARCHAR(1000),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ValResults_Exec FOREIGN KEY (ExecutionID) REFERENCES MDM_DimValidationExecution(ExecutionID),
    CONSTRAINT FK_ValResults_Config FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID),
    CONSTRAINT FK_ValResults_Rule FOREIGN KEY (ValidationRuleID) REFERENCES MDM_DimValidationRules(ValidationRuleID),
    CONSTRAINT FK_ValResults_Dim FOREIGN KEY (DimID) REFERENCES Dim(DimID),
    CONSTRAINT FK_ValResults_Member FOREIGN KEY (MemberID) REFERENCES Member(MemberID),
    CONSTRAINT FK_ValResults_Hier FOREIGN KEY (HierarchyID) REFERENCES Member(MemberID),
    CONSTRAINT FK_ValResults_CompMember FOREIGN KEY (ComparisonMemberID) REFERENCES Member(MemberID)
);

-- Create scheduling table (optional)
CREATE TABLE MDM_DimValidationSchedule (
    ScheduleID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    ScheduleType NVARCHAR(50) DEFAULT 'Daily',
    ScheduleFrequency INT DEFAULT 1,
    ScheduleTimeOfDay TIME,
    DayOfWeek INT,
    DayOfMonth INT,
    IsActive BIT DEFAULT 1,
    LastExecutionDate DATETIME,
    NextExecutionDate DATETIME,
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    CONSTRAINT FK_ValSched_Config FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID) ON DELETE CASCADE
);
```

#### 1.4 Create Performance Indexes

```sql
-- Create indexes for optimal query performance
CREATE INDEX IX_ValConfig_DimID ON MDM_DimValidationConfig(DimID);
CREATE INDEX IX_ValConfig_TypeID ON MDM_DimValidationConfig(ValidationTypeID);
CREATE INDEX IX_ValConfig_Active ON MDM_DimValidationConfig(IsActive);
CREATE INDEX IX_ValConfig_LastExec ON MDM_DimValidationConfig(LastExecutedDate);

CREATE INDEX IX_ValRules_ConfigID ON MDM_DimValidationRules(ValidationConfigID);
CREATE INDEX IX_ValRules_Active ON MDM_DimValidationRules(IsActive);

CREATE INDEX IX_ValExec_ConfigID ON MDM_DimValidationExecution(ValidationConfigID);
CREATE INDEX IX_ValExec_Date ON MDM_DimValidationExecution(ExecutionDate);
CREATE INDEX IX_ValExec_Status ON MDM_DimValidationExecution(ExecutionStatus);

CREATE INDEX IX_ValResults_ExecutionID ON MDM_DimValidationResults(ExecutionID);
CREATE INDEX IX_ValResults_ConfigID ON MDM_DimValidationResults(ValidationConfigID);
CREATE INDEX IX_ValResults_MemberID ON MDM_DimValidationResults(MemberID);
CREATE INDEX IX_ValResults_Status ON MDM_DimValidationResults(ResolutionStatus);
CREATE INDEX IX_ValResults_Severity ON MDM_DimValidationResults(IssueSeverity);
CREATE INDEX IX_ValResults_Created ON MDM_DimValidationResults(CreatedDate);

CREATE INDEX IX_ValSched_ConfigID ON MDM_DimValidationSchedule(ValidationConfigID);
CREATE INDEX IX_ValSched_Next ON MDM_DimValidationSchedule(NextExecutionDate, IsActive);
```

### Step 2: Data Migration (If Upgrading from Existing System)

#### 2.1 Backup Existing Data

```sql
-- Backup existing validation tables
SELECT * INTO MDM_ValConfig_VAL_BACKUP FROM MDM_ValConfig_VAL;
SELECT * INTO MDM_ValResults_VAL_BACKUP FROM MDM_ValResults_VAL;
SELECT * INTO MDM_ValConfigCriteriaDetail_VAL_BACKUP FROM MDM_ValConfigCriteriaDetail_VAL;
```

#### 2.2 Migrate Data to New Structure

```sql
-- Get the ValidationTypeID once
DECLARE @HierCompTypeID INT;
SELECT @HierCompTypeID = ValidationTypeID 
FROM MDM_DimValidationTypes 
WHERE ValidationTypeCode = 'HIER_COMPARISON';

-- Migrate existing configurations
INSERT INTO MDM_DimValidationConfig (
    ValidationName, ValidationTypeID, Description, DimID, 
    IsActive, CreatedBy, CreatedDate
)
SELECT 
    OS_ValidationName,
    @HierCompTypeID,
    'Migrated from legacy system',
    OS_DimID,
    1,
    'System',
    GETDATE()
FROM MDM_ValConfig_VAL;

-- Migrate existing rules (customize based on your old schema)
INSERT INTO MDM_DimValidationRules (
    ValidationConfigID, RuleSequence, TargetHierarchyID, 
    ComparisonHierarchyID, IsActive, CreatedBy, CreatedDate
)
SELECT 
    nc.ValidationConfigID,
    1,
    oc.OS_HierMember1,
    oc.OS_HierMember2,
    1,
    'System',
    GETDATE()
FROM MDM_ValConfig_VAL oc
INNER JOIN MDM_DimValidationConfig nc ON oc.OS_ValidationName = nc.ValidationName;
```

### Step 3: XML Configuration Update

The enhanced dashboard XML has been updated in:
`obj/OS Consultant Tools/Model Dimension Manager/DB Extracts/Model Dimension Manager.xml`

Key changes made:
1. Enhanced `DBRD_Validations` dashboard with KPI overview panel
2. Added new components: `lbl_VAL2_TotalValidations`, `lbl_VAL2_TotalIssues`, `lbl_VAL2_CriticalIssues`
3. Added `btn_VAL2_ManageConfig` button for navigation
4. Added parameters backed by SQL queries

### Step 4: Import XML into OneStream

1. Open OneStream Designer
2. Navigate to Application → Workspace → OS Consultant Tools → Model Dimension Manager
3. Use Code Utility for OneStream VS Code extension to import the XML package
4. Or manually import via OneStream Application Manager

### Step 5: Create Supporting SQL Adapters

Add these SQL adapters to the MDM assembly for the new dashboard components:

#### 5.1 Validation Configuration List Adapter

```csharp
// File: SQL_VAL2_Get_ConfigList.cs
// Location: Assemblies/OS Consultant Tools/Model Dimension Manager/MDM_UI_Assembly/SQL Adapters/

using System;
using System.Data;
using OneStream.Shared.Common;
using OneStream.Shared.Wcf;
using OneStream.Shared.Engine;
using OneStream.Shared.Database;

namespace __WsNamespacePrefix.MDM_UI_Assembly
{
    public class SQL_VAL2_Get_ConfigList
    {
        public static DataTable Execute(SessionInfo si, string dimFilter = "")
        {
            try
            {
                string sql = @"
                    SELECT 
                        vc.ValidationConfigID,
                        vc.ValidationName,
                        vt.ValidationTypeName,
                        d.Name as DimensionName,
                        vc.IsActive,
                        vc.Severity,
                        vc.LastExecutedDate,
                        vc.LastExecutionStatus,
                        ISNULL(lastExec.IssuesFound, 0) as IssuesFound
                    FROM MDM_DimValidationConfig vc
                    INNER JOIN MDM_DimValidationTypes vt ON vc.ValidationTypeID = vt.ValidationTypeID
                    INNER JOIN Dim d ON vc.DimID = d.DimID
                    OUTER APPLY (
                        SELECT TOP 1 IssuesFound 
                        FROM MDM_DimValidationExecution 
                        WHERE ValidationConfigID = vc.ValidationConfigID 
                        ORDER BY ExecutionDate DESC
                    ) lastExec
                    WHERE 1=1";
                
                if (!string.IsNullOrEmpty(dimFilter) && dimFilter != "All")
                {
                    sql += " AND d.Name = @DimName";
                }
                
                sql += " ORDER BY d.Name, vc.ValidationName";
                
                using (DbConnInfo dbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnInfo, sql, true);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}
```

#### 5.2 Validation Results Adapter

```csharp
// File: SQL_VAL2_Get_Results.cs

using System;
using System.Data;
using OneStream.Shared.Common;
using OneStream.Shared.Wcf;
using OneStream.Shared.Engine;
using OneStream.Shared.Database;

namespace __WsNamespacePrefix.MDM_UI_Assembly
{
    public class SQL_VAL2_Get_Results
    {
        public static DataTable Execute(SessionInfo si, int configId = 0, string statusFilter = "Open")
        {
            try
            {
                string sql = @"
                    SELECT 
                        vr.ResultID,
                        m.Name as MemberName,
                        h.Name as HierarchyName,
                        vr.IssueType,
                        vr.IssueSeverity,
                        vr.IssueDescription,
                        vr.ExpectedValue,
                        vr.ActualValue,
                        vr.ResolutionStatus,
                        vr.ResolutionNotes,
                        vr.CreatedDate
                    FROM MDM_DimValidationResults vr
                    INNER JOIN Member m ON vr.MemberID = m.MemberID
                    LEFT JOIN Member h ON vr.HierarchyID = h.MemberID
                    WHERE vr.ResolutionStatus = @Status";
                
                if (configId > 0)
                {
                    sql += " AND vr.ValidationConfigID = @ConfigId";
                }
                
                sql += " ORDER BY vr.IssueSeverity DESC, vr.CreatedDate DESC";
                
                using (DbConnInfo dbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
                {
                    // Add parameters
                    var parameters = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "@Status", statusFilter },
                        { "@ConfigId", configId }
                    };
                    
                    DataTable dt = BRApi.Database.ExecuteSql(dbConnInfo, sql, true);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                throw ErrorHandler.LogWrite(si, new XFException(si, ex));
            }
        }
    }
}
```

### Step 6: Update Dashboard Extender Business Rules

Update the MDM Dashboard Extender to handle new button clicks:

```csharp
// File: MDM_DB_DataSets.cs (add to existing Main method switch statement)

case "VAL2_ManageConfig":
    // Navigate to configuration dashboard
    return "NavigateToDashboard|VAL_LandingPage";

case "VAL2_RefreshKPIs":
    // Refresh KPI values
    BRApi.State.RefreshParameter(si, "Param_VAL2_TotalValidations");
    BRApi.State.RefreshParameter(si, "Param_VAL2_TotalIssues");
    BRApi.State.RefreshParameter(si, "Param_VAL2_CriticalIssues");
    return "Success";
```

### Step 7: Testing

#### 7.1 Verify Database Setup

```sql
-- Check tables were created
SELECT name FROM sys.tables WHERE name LIKE 'MDM_DimValidation%';

-- Verify validation types were inserted
SELECT * FROM MDM_DimValidationTypes;

-- Check indexes
SELECT name, object_id FROM sys.indexes WHERE name LIKE 'IX_Val%';
```

#### 7.2 Test Dashboard Loading

1. Login to OneStream web interface
2. Navigate to: Workspace → OS Consultant Tools → Model Dimension Manager
3. Click on "MDM Dashboard (DBRD)" group
4. Click on "Validations" dashboard
5. Verify KPI cards display (may show zeros initially)
6. Verify chart loads with existing validation data
7. Click "Manage Validations" button
8. Verify navigation to configuration dashboard

#### 7.3 Create Test Validation

1. In VAL_LandingPage dashboard, click "Add New Validation"
2. Enter test validation details:
   - Name: "Test Entity Hierarchy Check"
   - Type: "Hierarchy Comparison"
   - Dimension: Entity
   - Target Hierarchy: Default
   - Comparison Hierarchy: IC
3. Save configuration
4. Click "Execute Now"
5. Verify execution completes
6. Check results appear in grid

### Step 8: User Training

Provide training to users on:

1. **Viewing Validation Status**
   - Understanding KPI cards
   - Reading validation charts
   - Interpreting severity levels

2. **Configuring Validations**
   - Creating new validations
   - Editing existing configurations
   - Setting up rules and criteria

3. **Running Validations**
   - On-demand execution
   - Understanding execution status
   - Viewing execution history

4. **Managing Results**
   - Filtering and sorting issues
   - Acknowledging and resolving issues
   - Adding resolution notes
   - Exporting results

### Step 9: Performance Tuning

Monitor and optimize:

```sql
-- Monitor execution times
SELECT 
    vc.ValidationName,
    AVG(ve.DurationMS) as AvgDurationMS,
    MAX(ve.DurationMS) as MaxDurationMS,
    COUNT(*) as ExecutionCount
FROM MDM_DimValidationExecution ve
INNER JOIN MDM_DimValidationConfig vc ON ve.ValidationConfigID = vc.ValidationConfigID
WHERE ve.ExecutionDate > DATEADD(day, -7, GETDATE())
GROUP BY vc.ValidationName
ORDER BY AvgDurationMS DESC;

-- Check for missing indexes
SELECT 
    OBJECT_NAME(s.object_id) AS TableName,
    i.name AS IndexName,
    s.user_seeks,
    s.user_scans,
    s.user_lookups,
    s.user_updates
FROM sys.dm_db_index_usage_stats s
INNER JOIN sys.indexes i ON s.object_id = i.object_id AND s.index_id = i.index_id
WHERE OBJECT_NAME(s.object_id) LIKE 'MDM_DimValidation%'
ORDER BY s.user_seeks + s.user_scans + s.user_lookups DESC;
```

## Troubleshooting

### Common Issues and Solutions

#### Issue: KPI Cards Show Zero

**Solution:**
- Verify tables are populated with data
- Check parameter SQL queries are executing correctly
- Verify database connection string

#### Issue: Chart Not Loading

**Solution:**
- Check DATA_Validation_Dashboard adapter SQL
- Verify BIViewer component configuration
- Check browser console for errors

#### Issue: Navigation Not Working

**Solution:**
- Verify dashboard names match exactly
- Check navigation arguments in button component
- Verify user has access to target dashboard

#### Issue: Slow Performance

**Solution:**
- Review execution plan of SQL queries
- Add additional indexes as needed
- Consider archiving old validation results
- Implement result pagination

## Maintenance

### Regular Maintenance Tasks

1. **Archive Old Results** (Monthly)
```sql
-- Archive results older than 90 days
INSERT INTO MDM_DimValidationResults_Archive
SELECT * FROM MDM_DimValidationResults
WHERE CreatedDate < DATEADD(day, -90, GETDATE());

DELETE FROM MDM_DimValidationResults
WHERE CreatedDate < DATEADD(day, -90, GETDATE());
```

2. **Update Statistics** (Weekly)
```sql
UPDATE STATISTICS MDM_DimValidationConfig;
UPDATE STATISTICS MDM_DimValidationRules;
UPDATE STATISTICS MDM_DimValidationExecution;
UPDATE STATISTICS MDM_DimValidationResults;
```

3. **Monitor Disk Usage**
```sql
SELECT 
    t.name AS TableName,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB,
    (SUM(a.total_pages) - SUM(a.used_pages)) * 8 AS UnusedSpaceKB
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
INNER JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.name LIKE 'MDM_DimValidation%'
GROUP BY t.name
ORDER BY UsedSpaceKB DESC;
```

## Support

For issues or questions:
- Review documentation in `/Documentation/` folder
- Check OneStream community forums
- Contact your OneStream administrator
- Refer to OneStream Developer Reference Guide

## Appendix A: Complete SQL Script

See separate file: `Database_Schema_Complete.sql`

## Appendix B: XML Changes Summary

See: `obj/OS Consultant Tools/Model Dimension Manager/DB Extracts/Model Dimension Manager.xml`

Changes include:
- Enhanced DBRD_Validations dashboard (lines 8706-8780)
- New components: lbl_VAL2_* and btn_VAL2_* (lines 8084-8110)
- New parameters: Param_VAL2_* (lines 3797-3870)

## Appendix C: Component Reference

| Component Name | Type | Purpose |
|---|---|---|
| DBRD_Validations | Dashboard | Main enhanced validation dashboard |
| DBRD_VAL2_Overview | Dashboard | KPI panel with metrics |
| lbl_VAL2_TotalValidations | Label | Shows count of active validations |
| lbl_VAL2_TotalIssues | Label | Shows count of issues in last 7 days |
| lbl_VAL2_CriticalIssues | Label | Shows count of critical open issues |
| btn_VAL2_ManageConfig | Button | Navigate to configuration dashboard |
| BIView_Validations | BIViewer | Chart showing validation issues |

## Version History

- **v1.0** - Initial implementation with enhanced dashboard and 6-table schema
- Documentation created: January 2026

## License and Attribution

Part of OneStream Model Dimension Manager
Copyright © 2026
