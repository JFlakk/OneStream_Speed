# FMM Validation Framework - Implementation Summary

## Overview

This document summarizes the implementation of the **Data Validation Framework** for the Finance Model Manager (FMM) module. The framework addresses the requirement to perform data validations that bifurcate between **table-level** and **cube-level** validations, with support for both simple checks and complex multi-dimensional comparisons.

## Problem Statement

The user requested:
> "For the FMM module, I need to recommend a table structure for performing data validations. It will need to bifurcate between table and cube validations. Some of the validations may be simple such as checking a column value against a list of valid values, others may need to be at the cube level and running comparisons between separate scenarios for 1 to all of the OS dimensions."

## Solution Architecture

### Validation Context Bifurcation

The framework implements a clean bifurcation between two validation contexts:

1. **TABLE Context** - Validations on relational table data
   - Simple column value checks
   - Data type and range validations
   - Referential integrity checks
   - Pattern matching and constraints

2. **CUBE Context** - Validations on OneStream cube data
   - Scenario-to-scenario comparisons
   - Multi-dimensional balance checks
   - Temporal consistency validations
   - Cross-dimensional business rules

### Core Components

#### 1. Database Schema
**File:** `FMM_Validation_Table_Schema.sql`

Three tables form the foundation:

**FMM_Validation_Config** (Configuration)
- Stores validation rule definitions
- Bifurcates by `Validation_Context` (TABLE or CUBE)
- Supports 14 validation types total (7 for each context)
- Flexible JSON configuration per validation type
- Active/inactive flag for enabling/disabling rules
- Full audit trail

**FMM_Validation_Run** (Execution Tracking)
- Tracks validation execution metadata
- Summary statistics per run
- Execution time tracking
- Run status tracking (Completed, Failed, InProgress, Cancelled)
- Context and process type filtering

**FMM_Validation_Result** (Results Storage)
- Detailed results for each failed/warned validation
- Context-specific fields:
  - TABLE: Table_Name, Primary_Key_Value, Column_Name, Column_Value
  - CUBE: Cube_POV (JSON), Dimension_Values, Cell_Value, Comparison_Value
- Validation status (Failed, Warning, Info)
- Error messages and details

#### 2. SQL Adapter Class
**File:** `SQA_FMM_Validation_Config.cs`

Complete CRUD operations with:
- Configuration management (Insert, Update, Delete, Query)
- Run tracking and management
- Result storage with bulk insert optimization (SqlBulkCopy)
- Flexible filtering methods
- Helper methods for ID generation
- OneStream error handling patterns

Key Methods:
- `Update_FMM_Validation_Config()` - CRUD for configurations
- `Update_FMM_Validation_Run()` - CRUD for runs
- `Insert_Validation_Results()` - Bulk insert optimized for performance
- `Get_Validation_Config_By_Context()` - Filter by TABLE or CUBE
- `Get_Validation_Config_By_Process()` - Filter by process type
- `Delete_Old_Validation_Results()` - Archive maintenance

#### 3. Dashboard Datasets
**File:** `FMM_DB_DataSets.cs` (Modified)

Seven new datasets added:
- `get_FMM_Validation_Config` - All validation configurations
- `get_FMM_Validation_Contexts` - TABLE/CUBE context options
- `get_FMM_Table_Validation_Types` - All table validation types
- `get_FMM_Cube_Validation_Types` - All cube validation types
- `get_FMM_Validation_Severities` - Error/Warning/Info levels
- `get_FMM_Validation_Results` - Results with filtering
- `get_FMM_Validation_Runs` - Run history with filtering

#### 4. Comprehensive Documentation
**File:** `FMM_VALIDATION_README.md`

28KB+ documentation covering:
- Complete guide for all 14 validation types
- JSON configuration examples
- SQL configuration examples
- C# usage examples
- Querying and reporting
- Best practices and troubleshooting

## Validation Types

### Table Validations (7 Types)

1. **ColumnValueList** - Check column values against predefined list
   - Example: Status must be Draft, Submitted, Approved, Rejected, or Closed

2. **ColumnValueRange** - Check numeric values within range
   - Example: Monthly amounts between -1B and +1B

3. **ColumnValuePattern** - Check values match regex pattern
   - Example: Account codes must be 2 letters + 4 digits

4. **RequiredColumns** - Ensure required columns are populated
   - Example: WF_Scenario_Name, WF_Profile_Name must not be NULL

5. **UniqueConstraint** - Check for duplicate values
   - Example: Combination of workflow fields must be unique

6. **ReferentialIntegrity** - Validate foreign key relationships
   - Example: Detail records must reference valid parent records

7. **CustomTableSQL** - Execute custom SQL for complex rules
   - Example: Approved plans must have approval dates

### Cube Validations (7 Types)

1. **ScenarioComparison** - Compare values between scenarios
   - Example: Actual should not exceed Budget by more than 10%

2. **DimensionalBalance** - Check balances across hierarchies
   - Example: Parent entity totals must equal sum of children

3. **CrossDimensionalRule** - Validate rules across dimensions
   - Example: Revenue accounts must have positive values

4. **TemporalConsistency** - Check consistency across time
   - Example: Quarterly amounts must equal sum of monthly amounts

5. **AllocationValidation** - Validate allocation totals
   - Example: Allocated expenses must equal source total

6. **VarianceThreshold** - Check variances within thresholds
   - Example: Budget variance must be within acceptable limits

7. **CustomCubeSQL** - Execute custom SQL for cube data
   - Example: Complex multi-dimensional business rule checks

## Key Design Decisions

### 1. Bifurcation Strategy
The `Validation_Context` column in FMM_Validation_Config cleanly separates TABLE and CUBE validations, with CHECK constraints ensuring only appropriate validation types are used for each context.

### 2. Flexible JSON Configuration
Each validation type stores its specific configuration in a `Config_JSON` field, allowing:
- Different parameters per validation type
- Easy extension without schema changes
- Clear documentation of validation requirements

### 3. Context-Specific Result Fields
FMM_Validation_Result includes both TABLE-specific fields (Table_Name, Column_Name, etc.) and CUBE-specific fields (Cube_POV, Dimension_Values, etc.), with proper NULL handling based on context.

### 4. Performance Optimization
- SqlBulkCopy for bulk result insertion (1000 row batches)
- Strategic indexes on all three tables
- Optimized queries with proper filtering
- CASCADE DELETE from config to results

### 5. Integration with Existing Patterns
Follows established patterns from:
- MDM validation framework (similar structure)
- FMM table configuration system (naming conventions)
- GBL_SQL_Command_Builder for CRUD operations
- OneStream error handling (ErrorHandler.LogWrite)

## Usage Workflow

### 1. Configure Validations
```sql
-- Create a table validation
INSERT INTO FMM_Validation_Config (...)
VALUES (...);

-- Create a cube validation
INSERT INTO FMM_Validation_Config (...)
VALUES (...);
```

### 2. Execute Validations
```csharp
// In OneStream business rule
var adapter = new SQA_FMM_Validation_Config(si, connection);

// Get active validations
var configs = adapter.Get_Active_Validation_Configs(si);

// Create run record
int runID = adapter.Get_Next_Run_ID(si);
// ... populate run data

// Execute validation logic
// ... collect results

// Bulk insert results
adapter.Insert_Validation_Results(si, dtResults);

// Update run summary
// ... update with statistics
```

### 3. Review Results
```sql
-- View recent failures
SELECT * FROM vw_FMM_Validation_Results
WHERE Validation_Status = 'Failed'
  AND Run_Date >= DATEADD(day, -7, GETDATE());
```

### 4. Maintain Data
```csharp
// Archive old results (90+ days)
adapter.Delete_Old_Validation_Results(si, 90);
```

## Example Scenarios

### Simple Table Validation
**Requirement:** Ensure Status column only contains valid values

**Configuration:**
```json
{
  "ColumnName": "Status",
  "ValidValues": ["Draft", "Submitted", "Approved", "Rejected", "Closed"]
}
```

**Result Example:**
- Table_Name: "Register_Plan"
- Primary_Key_Value: "ABC123"
- Column_Name: "Status"
- Column_Value: "InvalidStatus"
- Error_Message: "Invalid status value: InvalidStatus"

### Complex Cube Validation
**Requirement:** Compare Actual vs Budget across all dimensions

**Configuration:**
```json
{
  "BaseScenario": "S#Budget",
  "CompareScenario": "S#Actual",
  "VarianceType": "Percentage",
  "Threshold": 10,
  "Dimensions": ["Entity", "Account", "Time"],
  "FilterPOV": {"Flow": "F#EndBalance", "View": "V#Periodic"}
}
```

**Result Example:**
- Cube_POV: `{"Scenario":"S#Actual","Entity":"E001","Account":"A100","Time":"2024M01"}`
- Dimension_Values: "Scenario=Actual, Entity=East Region, Account=Revenue, Time=Jan 2024"
- Cell_Value: 1250000.00
- Comparison_Value: 1000000.00
- Error_Message: "Actual exceeds Budget by 25%"

## Database Schema Summary

### Tables Created
1. `FMM_Validation_Config` (14 columns, 3 indexes)
2. `FMM_Validation_Run` (16 columns, 2 indexes)
3. `FMM_Validation_Result` (18 columns, 4 indexes)

### Views Created
1. `vw_FMM_Validation_Config_Summary` - Configs with statistics
2. `vw_FMM_Validation_Results` - Results with config details
3. `vw_FMM_Validation_Run_Summary` - Runs with summary stats

### Indexes Created
- 9 strategic indexes across all tables for optimal query performance
- Covering indexes for common filter patterns
- Filtered indexes for context-specific queries

## Code Metrics

### Files Created/Modified
1. `FMM_Validation_Table_Schema.sql` - 585 lines (schema + examples)
2. `SQA_FMM_Validation_Config.cs` - 642 lines (SQL adapter)
3. `FMM_DB_DataSets.cs` - 223 lines added (datasets)
4. `FMM_VALIDATION_README.md` - 1,130 lines (documentation)

**Total:** ~2,580 lines of code and documentation

### Key Features
- 3 database tables with full CRUD support
- 14 validation types (7 table, 7 cube)
- 3 helper views for reporting
- 7 dashboard datasets for UI integration
- Comprehensive documentation with 30+ examples

## Next Steps for Implementation

To complete the validation feature, implement:

1. **Validation Execution Engine**
   - Business rule to execute validations
   - Logic for each validation type
   - Result aggregation and insertion

2. **Dashboard UI Components**
   - Configuration form for creating/editing validations
   - Validation execution interface
   - Results viewer with filtering
   - Run history viewer

3. **Integration with FMM Processes**
   - Hook validations into Register Plan workflow
   - Schedule regular validation runs
   - Email notifications for failures

4. **Testing**
   - Unit tests for SQL adapter methods
   - Integration tests for validation execution
   - Performance tests for large datasets

## Benefits

### Flexibility
- 14 validation types cover wide range of scenarios
- JSON configuration allows customization
- Easy to add new validation types

### Performance
- Bulk operations optimized for large datasets
- Strategic indexes for fast queries
- Efficient result storage

### Maintainability
- Clear separation of concerns
- Comprehensive documentation
- Follows OneStream patterns

### Auditability
- Complete tracking of all validation runs
- Detailed error messages and context
- Full audit trail for configurations

## Comparison with MDM Validation Framework

The FMM validation framework builds upon the successful MDM validation implementation with key enhancements:

### Similarities
- Three-table structure (Config, Run, Result)
- JSON-based configuration
- Severity levels (Error, Warning, Info)
- Bulk insert optimization
- Helper views for querying

### Enhancements for FMM
- **Bifurcated Contexts** - Clear separation between TABLE and CUBE validations
- **Dual Result Storage** - Separate fields for table vs cube validation results
- **Cube-Specific Features** - POV handling, dimensional values, numeric comparisons
- **Process Type Linking** - Explicit linking to FMM processes
- **Extended Validation Types** - 7 cube-specific validation types

## Conclusion

The FMM Validation Framework provides a robust, flexible, and scalable solution for data quality validation in the Finance Model Manager module. The clean bifurcation between table and cube validations addresses the core requirement while providing extensive flexibility through JSON configuration and custom SQL options.

The implementation follows OneStream and repository conventions, integrates seamlessly with existing FMM functionality, and provides comprehensive documentation for users to configure validations for their specific needs.

### Key Achievements
✅ Bifurcated validation contexts (TABLE/CUBE)
✅ 14 validation types covering simple to complex scenarios
✅ Complete CRUD operations via SQL adapter
✅ Dashboard datasets for UI integration
✅ Helper views for reporting and querying
✅ Comprehensive documentation with 30+ examples
✅ Performance-optimized with bulk operations and indexes
✅ Full audit trail and error tracking

The framework is ready for integration into the FMM module and can be extended with additional validation types as new requirements emerge.
