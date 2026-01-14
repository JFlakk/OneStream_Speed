# Model Dimension Manager - Validation Dashboard Enhancement

## Executive Summary
This document outlines the recommended table structure and UI design for enhancing the Model Dimension Manager's Validation Dashboard to support comprehensive dimension validation capabilities.

## Current State Analysis

### Existing Tables
1. **MDM_ValConfig_VAL** - Basic validation configuration
   - OS_Validation_ID (PK)
   - OS_ValidationName
   - OS_DimID
   - Basic configuration fields

2. **MDM_ValResults_VAL** - Validation execution results
   - OS_Validation_ID (FK)
   - OS_Member_ID
   - OS_HierNotIn
   - Result data

3. **MDM_ValConfigCriteriaDetail_VAL** - Validation criteria details
   - OS_Validation_ID (FK)
   - OS_Validation_Criteria_ID
   - Criteria configuration

### Current Limitations
- Limited validation types (primarily hierarchy comparison)
- Inflexible rule configuration
- Basic visualization
- No validation execution history
- Limited audit trail

## Recommended Table Structure

### 1. MDM_DimValidationTypes (Master Validation Types)
Master table defining all available validation types.

```sql
CREATE TABLE MDM_DimValidationTypes (
    ValidationTypeID INT PRIMARY KEY IDENTITY(1,1),
    ValidationTypeName NVARCHAR(100) NOT NULL UNIQUE,
    ValidationTypeCode NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(500),
    Category NVARCHAR(50), -- e.g., 'Hierarchy', 'Property', 'CrossReference', 'Completeness'
    IsActive BIT DEFAULT 1,
    RequiresComparison BIT DEFAULT 0,
    SupportsMultipleDimensions BIT DEFAULT 0,
    ConfigurationSchema NVARCHAR(MAX), -- JSON schema for type-specific config
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME
)
```

**Standard Validation Types:**
- HIER_COMPLETENESS - Check hierarchy coverage
- HIER_COMPARISON - Compare member presence across hierarchies
- PROP_POPULATION - Verify required properties are populated
- PROP_VALIDATION - Validate property values against rules
- CROSS_DIM_REF - Validate cross-dimension references
- MEMBER_UNIQUENESS - Check for duplicate members
- PARENT_CHILD - Validate parent-child relationships
- DATA_INTEGRITY - Check data integrity constraints

### 2. MDM_DimValidationConfig (Validation Configuration)
Main configuration table for validation rules.

```sql
CREATE TABLE MDM_DimValidationConfig (
    ValidationConfigID INT PRIMARY KEY IDENTITY(1,1),
    ValidationName NVARCHAR(200) NOT NULL,
    ValidationTypeID INT NOT NULL,
    Description NVARCHAR(1000),
    DimID INT NOT NULL,
    IsActive BIT DEFAULT 1,
    Severity NVARCHAR(20), -- 'Critical', 'Warning', 'Info'
    ExecutionSchedule NVARCHAR(50), -- 'OnDemand', 'Daily', 'Weekly', 'OnChange'
    NotifyOnFailure BIT DEFAULT 0,
    NotificationList NVARCHAR(500),
    LastExecutedDate DATETIME,
    LastExecutedBy NVARCHAR(100),
    LastExecutionStatus NVARCHAR(50),
    LastExecutionDuration INT, -- milliseconds
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    FOREIGN KEY (ValidationTypeID) REFERENCES MDM_DimValidationTypes(ValidationTypeID),
    FOREIGN KEY (DimID) REFERENCES Dim(DimID)
)
```

### 3. MDM_DimValidationRules (Detailed Rule Definitions)
Stores the specific rule criteria and parameters for each validation configuration.

```sql
CREATE TABLE MDM_DimValidationRules (
    ValidationRuleID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    RuleSequence INT DEFAULT 1,
    RuleType NVARCHAR(50), -- 'Include', 'Exclude', 'Condition', 'Comparison'
    TargetHierarchyID INT, -- For hierarchy-based validations
    ComparisonHierarchyID INT, -- For comparison validations
    TargetPropertyName NVARCHAR(100), -- For property validations
    ComparisonOperator NVARCHAR(20), -- 'Equals', 'NotEquals', 'Contains', 'IsNull', 'IsNotNull'
    ComparisonValue NVARCHAR(500),
    MemberFilterExpression NVARCHAR(MAX), -- Filter for which members to validate
    IsActive BIT DEFAULT 1,
    ConfigurationJSON NVARCHAR(MAX), -- Type-specific configuration in JSON
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID),
    FOREIGN KEY (TargetHierarchyID) REFERENCES Member(MemberID),
    FOREIGN KEY (ComparisonHierarchyID) REFERENCES Member(MemberID)
)
```

### 4. MDM_DimValidationExecution (Execution History)
Tracks each validation execution with summary metrics.

```sql
CREATE TABLE MDM_DimValidationExecution (
    ExecutionID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    ExecutionDate DATETIME DEFAULT GETDATE(),
    ExecutedBy NVARCHAR(100),
    ExecutionStatus NVARCHAR(50), -- 'Success', 'Warning', 'Error', 'Running'
    StartTime DATETIME,
    EndTime DATETIME,
    DurationMS INT,
    TotalMembersChecked INT,
    IssuesFound INT,
    CriticalIssues INT,
    WarningIssues INT,
    InfoIssues INT,
    ExecutionLog NVARCHAR(MAX),
    ErrorMessage NVARCHAR(MAX),
    FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID)
)
```

### 5. MDM_DimValidationResults (Detailed Results)
Stores individual validation issues found during execution.

```sql
CREATE TABLE MDM_DimValidationResults (
    ResultID INT PRIMARY KEY IDENTITY(1,1),
    ExecutionID INT NOT NULL,
    ValidationConfigID INT NOT NULL,
    ValidationRuleID INT,
    DimID INT NOT NULL,
    MemberID INT NOT NULL,
    HierarchyID INT,
    IssueType NVARCHAR(100), -- Specific issue type
    IssueSeverity NVARCHAR(20), -- 'Critical', 'Warning', 'Info'
    IssueDescription NVARCHAR(1000),
    ExpectedValue NVARCHAR(500),
    ActualValue NVARCHAR(500),
    PropertyName NVARCHAR(100),
    ComparisonMemberID INT,
    ResolutionStatus NVARCHAR(50), -- 'Open', 'Acknowledged', 'Resolved', 'Ignored'
    ResolvedBy NVARCHAR(100),
    ResolvedDate DATETIME,
    ResolutionNotes NVARCHAR(1000),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ExecutionID) REFERENCES MDM_DimValidationExecution(ExecutionID),
    FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID),
    FOREIGN KEY (ValidationRuleID) REFERENCES MDM_DimValidationRules(ValidationRuleID),
    FOREIGN KEY (DimID) REFERENCES Dim(DimID),
    FOREIGN KEY (MemberID) REFERENCES Member(MemberID),
    FOREIGN KEY (HierarchyID) REFERENCES Member(MemberID),
    FOREIGN KEY (ComparisonMemberID) REFERENCES Member(MemberID)
)
```

### 6. MDM_DimValidationSchedule (Optional: Automated Scheduling)
For future automated validation scheduling capability.

```sql
CREATE TABLE MDM_DimValidationSchedule (
    ScheduleID INT PRIMARY KEY IDENTITY(1,1),
    ValidationConfigID INT NOT NULL,
    ScheduleType NVARCHAR(50), -- 'Daily', 'Weekly', 'Monthly', 'OnChange'
    ScheduleFrequency INT, -- Numeric frequency
    ScheduleTimeOfDay TIME,
    DayOfWeek INT, -- 1-7 for weekly
    DayOfMonth INT, -- 1-31 for monthly
    IsActive BIT DEFAULT 1,
    LastExecutionDate DATETIME,
    NextExecutionDate DATETIME,
    CreatedBy NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(100),
    ModifiedDate DATETIME,
    FOREIGN KEY (ValidationConfigID) REFERENCES MDM_DimValidationConfig(ValidationConfigID)
)
```

### Supporting Indexes

```sql
-- Performance indexes
CREATE INDEX IX_ValidationConfig_DimID ON MDM_DimValidationConfig(DimID);
CREATE INDEX IX_ValidationConfig_TypeID ON MDM_DimValidationConfig(ValidationTypeID);
CREATE INDEX IX_ValidationConfig_Active ON MDM_DimValidationConfig(IsActive);
CREATE INDEX IX_ValidationRules_ConfigID ON MDM_DimValidationRules(ValidationConfigID);
CREATE INDEX IX_ValidationExecution_ConfigID ON MDM_DimValidationExecution(ValidationConfigID);
CREATE INDEX IX_ValidationExecution_Date ON MDM_DimValidationExecution(ExecutionDate);
CREATE INDEX IX_ValidationResults_ExecutionID ON MDM_DimValidationResults(ExecutionID);
CREATE INDEX IX_ValidationResults_MemberID ON MDM_DimValidationResults(MemberID);
CREATE INDEX IX_ValidationResults_Status ON MDM_DimValidationResults(ResolutionStatus);
CREATE INDEX IX_ValidationSchedule_ConfigID ON MDM_DimValidationSchedule(ValidationConfigID);
CREATE INDEX IX_ValidationSchedule_Next ON MDM_DimValidationSchedule(NextExecutionDate, IsActive);
```

## Recommended UI Design

### Dashboard Group Structure

#### Main Dashboard: "Validation Overview"
**Purpose:** Executive summary of all validation configurations and recent execution results.

**Components:**
1. **KPI Cards (Top Row)**
   - Total Active Validations
   - Total Issues (Last 7 Days)
   - Critical Issues (Open)
   - Last Execution Date

2. **Validation Status Chart**
   - Pie/Donut chart showing validation pass/fail/warning distribution
   - By dimension or validation type

3. **Recent Executions Grid**
   - Last 20 validation executions
   - Columns: Name, Dimension, Type, Execution Date, Status, Issues Found, Duration
   - Click to drill down to details

4. **Issues Trend Chart**
   - Line chart showing issues over time (last 30 days)
   - Multiple lines for Critical/Warning/Info

#### Configuration Dashboard: "Validation Setup"
**Purpose:** Create, edit, and manage validation configurations.

**Layout:** Left-right split panel design

**Left Panel: Configuration List**
- Listbox showing all validation configurations
- Grouped by dimension or validation type
- Status indicators (Active/Inactive, Last execution status)
- Search/filter capability
- "Add New Validation" button at top

**Right Panel: Configuration Detail (Tabbed)**

**Tab 1: General Settings**
- Validation Name (textbox)
- Description (multi-line textbox)
- Validation Type (dropdown from MDM_DimValidationTypes)
- Target Dimension (dropdown)
- Severity Level (dropdown: Critical/Warning/Info)
- Active Status (checkbox)

**Tab 2: Rules Configuration**
- Dynamic form based on ValidationTypeCode
- For HIER_COMPARISON:
  - Target Hierarchy (member selector)
  - Comparison Hierarchy (member selector)
  - Member Filter (U4 selector or expression builder)
- For PROP_POPULATION:
  - Property Name (dropdown from dim properties)
  - Required Properties (multi-select)
  - Validation Rules (grid)
- For PROP_VALIDATION:
  - Property Name (dropdown)
  - Validation Operator (dropdown)
  - Expected Value (textbox)
  - Member Filter (expression builder)

**Tab 3: Execution Settings**
- Schedule Type (dropdown: On Demand, Daily, Weekly, etc.)
- Execution Time (time picker for scheduled)
- Notify on Failure (checkbox)
- Notification Recipients (textbox)

**Tab 4: History**
- Read-only grid of past executions
- Columns: Date, Executed By, Status, Duration, Issues Found
- Link to view detailed results

**Action Buttons:**
- Save Configuration
- Execute Now
- Delete Configuration
- Duplicate Configuration

#### Execution Dashboard: "Run Validations"
**Purpose:** Execute validations and monitor progress.

**Components:**
1. **Execution Queue Panel**
   - Shows validations ready to execute
   - Multi-select capability
   - Execute Selected button
   - Execute All button

2. **Active Executions Panel**
   - Real-time status of running validations
   - Progress bars
   - Elapsed time
   - Cancel button

3. **Recent Completions**
   - Last completed executions (last 10)
   - Quick status view
   - Link to results

#### Results Dashboard: "Validation Results"
**Purpose:** View and manage validation issues.

**Layout:** Three-panel design

**Top Panel: Filters**
- Date Range selector
- Dimension filter (multi-select)
- Validation Type filter (multi-select)
- Severity filter (checkboxes: Critical/Warning/Info)
- Resolution Status filter (Open/Acknowledged/Resolved/Ignored)

**Middle Panel: Summary View**
- Grouped grid showing issues by validation configuration
- Columns: Validation Name, Dimension, Type, Total Issues, Critical, Warning, Info, Last Run
- Expandable rows to show issue details
- Charts showing distribution

**Bottom Panel: Issue Detail**
- Appears when row selected in summary
- Detailed grid of individual issues
- Columns: Member Name, Hierarchy, Issue Type, Description, Expected, Actual, Severity, Status, Resolution Notes
- Bulk actions: Mark as Acknowledged, Mark as Resolved, Ignore Selected
- Export to Excel button
- Individual actions: View Member, Edit Resolution, Add Note

**Advanced Features:**
- Drill-through to member maintenance
- Bulk resolution with notes
- Issue assignment to users
- Email report generation

#### Member Validation View (Embedded in Member Maintenance)
**Purpose:** Show validation issues for a specific member during maintenance.

**Components:**
1. **Validation Status Indicator**
   - Icon/badge showing if member has validation issues
   - Count of issues by severity

2. **Issue List**
   - Compact grid of issues for current member
   - Click to see full details
   - Quick resolution actions

### Navigation Flow

```
MDM Dashboard (DBRD) Landing Page
├── Member Audit (existing)
├── Validations (enhanced) ─┐
│                            │
└── New "Validation Overview" ←┘
    ├── Click on Chart/Grid → Validation Results Dashboard
    ├── Manage Button → Validation Setup Dashboard
    └── Execute Button → Run Validations Dashboard
```

### Component Naming Convention

Following MDM pattern:
- **VAL2_** prefix for new validation components
- **DBRD_VAL2_** for dashboard group components
- **Param_VAL2_** for parameters
- **DATA_VAL2_** for adapters

Example components:
- `lbx_VAL2_ConfigList` - Configuration listbox
- `cbx_VAL2_ValidationType` - Validation type dropdown
- `grd_VAL2_Results` - Results grid
- `btn_VAL2_Execute` - Execute button
- `DATA_VAL2_ConfigList` - Adapter for config list
- `DATA_VAL2_ExecutionHistory` - Adapter for execution history

## Implementation Recommendations

### Phase 1: Database Schema
1. Create all tables with proper foreign keys
2. Populate MDM_DimValidationTypes with standard types
3. Migrate existing data from old tables to new structure
4. Create stored procedures for common operations

### Phase 2: Core Dashboards
1. Implement Validation Overview dashboard
2. Implement Validation Setup dashboard (basic)
3. Update DATA_Validation_Dashboard adapter for new tables

### Phase 3: Execution & Results
1. Implement Run Validations dashboard
2. Implement Validation Results dashboard with drill-down
3. Add resolution workflow

### Phase 4: Advanced Features
1. Add scheduling capability
2. Implement notifications
3. Add trend analysis
4. Integrate with member maintenance

### Phase 5: Migration & Training
1. Data migration scripts
2. User documentation
3. Training materials
4. Rollout plan

## Benefits

1. **Flexibility**: Support any validation type with rule engine
2. **Scalability**: Structured schema supports growth
3. **Auditability**: Complete execution and resolution history
4. **Usability**: Intuitive UI with drill-down and filtering
5. **Performance**: Proper indexing for large datasets
6. **Maintainability**: Clear separation of configuration, execution, and results
7. **Extensibility**: JSON configuration allows type-specific extensions

## Migration Strategy

### From Current to New Structure

```sql
-- Migration script example
-- 1. Migrate validation types (manual seeding)
INSERT INTO MDM_DimValidationTypes (ValidationTypeName, ValidationTypeCode, Category)
VALUES ('Hierarchy Comparison', 'HIER_COMPARISON', 'Hierarchy');

-- 2. Migrate existing configs
INSERT INTO MDM_DimValidationConfig (ValidationName, ValidationTypeID, DimID, ...)
SELECT 
    OS_ValidationName,
    (SELECT ValidationTypeID FROM MDM_DimValidationTypes WHERE ValidationTypeCode = 'HIER_COMPARISON'),
    OS_DimID,
    ...
FROM MDM_VAL_Config;

-- 3. Migrate rules
INSERT INTO MDM_DimValidationRules (ValidationConfigID, TargetHierarchyID, ComparisonHierarchyID, ...)
SELECT ...
FROM MDM_ValConfigCriteriaDetail_VAL ...;

-- 4. Create execution records for historical results
-- 5. Migrate results
```

### Backward Compatibility
- Keep old tables temporarily with views
- Create backward-compatible adapters
- Gradual cutover of dashboards

## Appendix: Sample Data

### Sample Validation Types
```sql
INSERT INTO MDM_DimValidationTypes VALUES
('Hierarchy Completeness', 'HIER_COMPLETENESS', 'Hierarchy', 'Verify all members exist in specified hierarchies', 1, 0, 0, NULL),
('Hierarchy Comparison', 'HIER_COMPARISON', 'Hierarchy', 'Compare member presence across two hierarchies', 1, 1, 0, NULL),
('Property Population', 'PROP_POPULATION', 'Property', 'Check that required properties are populated', 1, 0, 0, NULL),
('Property Value Validation', 'PROP_VALIDATION', 'Property', 'Validate property values against rules', 1, 0, 0, NULL),
('Cross-Dimension Reference', 'CROSS_DIM_REF', 'CrossReference', 'Validate references to members in other dimensions', 1, 0, 1, NULL),
('Member Uniqueness', 'MEMBER_UNIQUENESS', 'Integrity', 'Check for duplicate members across hierarchies', 1, 0, 0, NULL),
('Parent-Child Integrity', 'PARENT_CHILD', 'Integrity', 'Validate parent-child relationships', 1, 0, 0, NULL),
('Data Completeness', 'DATA_COMPLETENESS', 'Completeness', 'Verify required data elements are present', 1, 0, 0, NULL);
```

### Sample Configuration
```sql
INSERT INTO MDM_DimValidationConfig VALUES
('Entity IC Relationships Check', 1, 'Verify all entities in default hierarchy exist in IC hierarchy', 5, 1, 'Critical', 'Daily', 1, 'admin@company.com', NULL, NULL, NULL, NULL, 'System', GETDATE(), NULL, NULL);
```

## Conclusion

This design provides a robust, flexible, and scalable foundation for dimension validation in the Model Dimension Manager. The table structure supports multiple validation types and complex rules, while the UI design ensures usability and efficient issue management. The phased implementation approach allows for incremental delivery of value while maintaining stability of the existing system.
