-- =============================================
-- FMM Validation Framework - Database Schema
-- Description: Validation framework for Finance Model Manager supporting both
--              table-level and cube-level data validations
-- 
-- Key Features:
--   - Bifurcates between TABLE and CUBE validation contexts
--   - Table validations: Simple column value checks against valid lists/ranges
--   - Cube validations: Complex multi-dimensional scenario comparisons
--   - Flexible JSON configuration per validation type
--   - Execution tracking with run metadata
--   - Detailed result storage for failed validations
-- =============================================

-- Drop tables in reverse dependency order if they exist
IF OBJECT_ID('dbo.FMM_Validation_Result', 'U') IS NOT NULL DROP TABLE dbo.FMM_Validation_Result;
IF OBJECT_ID('dbo.FMM_Validation_Run', 'U') IS NOT NULL DROP TABLE dbo.FMM_Validation_Run;
IF OBJECT_ID('dbo.FMM_Validation_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Validation_Config;
GO

-- =============================================
-- Table: FMM_Validation_Config
-- Description: Stores validation rule definitions for both table and cube validations
-- 
-- Validation_Context: 
--   - TABLE: Validations that check table data (column values, constraints, etc.)
--   - CUBE: Validations that check cube data (scenario comparisons, dimensional analysis)
-- 
-- Table Validation Types:
--   - ColumnValueList: Check column values against a predefined list
--   - ColumnValueRange: Check numeric column values within a range
--   - ColumnValuePattern: Check column values match a regex pattern
--   - RequiredColumns: Ensure required columns are populated (not NULL/empty)
--   - UniqueConstraint: Check for duplicate values in specified columns
--   - ReferentialIntegrity: Validate foreign key relationships
--   - CustomTableSQL: Execute custom SQL query against table data
-- 
-- Cube Validation Types:
--   - ScenarioComparison: Compare values between two scenarios
--   - DimensionalBalance: Check balances across dimensional slices
--   - CrossDimensionalRule: Validate business rules across multiple dimensions
--   - TemporalConsistency: Check data consistency across time periods
--   - AllocationValidation: Validate allocation totals and distributions
--   - VarianceThreshold: Check variances against thresholds
--   - CustomCubeSQL: Execute custom SQL against cube view data
-- =============================================

CREATE TABLE [dbo].[FMM_Validation_Config](
	[Validation_Config_ID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Validation_Context] [nvarchar](20) NOT NULL,               -- 'TABLE' or 'CUBE'
	[Validation_Type] [nvarchar](50) NOT NULL,
	[Target_Object] [nvarchar](255) NOT NULL,                   -- Table name for TABLE context, Cube/View name for CUBE context
	[Process_Type] [nvarchar](100) NULL,                        -- Links to FMM process (e.g., 'Register_Plan', 'Budget_Plan')
	[Is_Active] [bit] NOT NULL DEFAULT 1,
	[Severity] [nvarchar](20) NOT NULL,                         -- 'Error', 'Warning', 'Info'
	[Config_JSON] [nvarchar](max) NULL,                         -- JSON configuration specific to validation type
	[Create_Date] [datetime] NOT NULL DEFAULT GETDATE(),
	[Create_User] [nvarchar](50) NOT NULL,
	[Update_Date] [datetime] NOT NULL DEFAULT GETDATE(),
	[Update_User] [nvarchar](50) NOT NULL,
    
    CONSTRAINT PK_FMM_Validation_Config PRIMARY KEY CLUSTERED ([Validation_Config_ID] ASC),
	
	CONSTRAINT CK_FMM_Validation_Config_Context CHECK ([Validation_Context] IN ('TABLE', 'CUBE')),
	
	CONSTRAINT CK_FMM_Validation_Config_Table_Type CHECK (
		[Validation_Context] != 'TABLE' OR [Validation_Type] IN (
			'ColumnValueList',
			'ColumnValueRange',
			'ColumnValuePattern',
			'RequiredColumns',
			'UniqueConstraint',
			'ReferentialIntegrity',
			'CustomTableSQL'
		)
	),
	
	CONSTRAINT CK_FMM_Validation_Config_Cube_Type CHECK (
		[Validation_Context] != 'CUBE' OR [Validation_Type] IN (
			'ScenarioComparison',
			'DimensionalBalance',
			'CrossDimensionalRule',
			'TemporalConsistency',
			'AllocationValidation',
			'VarianceThreshold',
			'CustomCubeSQL'
		)
	),
	
	CONSTRAINT CK_FMM_Validation_Config_Severity CHECK ([Severity] IN ('Error', 'Warning', 'Info'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indexes for faster lookups
CREATE NONCLUSTERED INDEX IX_FMM_Validation_Config_Context 
ON FMM_Validation_Config (Validation_Context, Is_Active)
INCLUDE (Target_Object, Process_Type);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Config_Process 
ON FMM_Validation_Config (Process_Type, Is_Active);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Config_Target 
ON FMM_Validation_Config (Target_Object, Validation_Context);
GO

-- =============================================
-- Table: FMM_Validation_Run
-- Description: Tracks validation execution metadata
-- Stores summary information about each validation run including
-- timing, user, and aggregate statistics
-- =============================================

CREATE TABLE [dbo].[FMM_Validation_Run](
	[Run_ID] [int] NOT NULL,
	[Run_Date] [datetime] NOT NULL,
	[Run_User] [nvarchar](50) NOT NULL,
	[Validation_Context] [nvarchar](20) NULL,                   -- Filter: 'TABLE', 'CUBE', or NULL for all
	[Process_Type] [nvarchar](100) NULL,                        -- Filter: specific process or NULL for all
	[Target_Object] [nvarchar](255) NULL,                       -- Filter: specific table/cube or NULL for all
	[Total_Validations] [int] NOT NULL DEFAULT 0,
	[Total_Records_Checked] [int] NOT NULL DEFAULT 0,
	[Total_Failures] [int] NOT NULL DEFAULT 0,
	[Total_Warnings] [int] NOT NULL DEFAULT 0,
	[Total_Info] [int] NOT NULL DEFAULT 0,
	[Execution_Time_Ms] [int] NULL,
	[Status] [nvarchar](20) NOT NULL,                           -- 'Completed', 'Failed', 'InProgress', 'Cancelled'
	[Error_Message] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_FMM_Validation_Run PRIMARY KEY CLUSTERED ([Run_ID] ASC),
	CONSTRAINT CK_FMM_Validation_Run_Status CHECK ([Status] IN ('Completed', 'Failed', 'InProgress', 'Cancelled')),
	CONSTRAINT CK_FMM_Validation_Run_Context CHECK ([Validation_Context] IS NULL OR [Validation_Context] IN ('TABLE', 'CUBE'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indexes for run history lookups
CREATE NONCLUSTERED INDEX IX_FMM_Validation_Run_Date 
ON FMM_Validation_Run (Run_Date DESC)
INCLUDE (Status, Validation_Context, Process_Type);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Run_Context 
ON FMM_Validation_Run (Validation_Context, Process_Type, Run_Date DESC);
GO

-- =============================================
-- Table: FMM_Validation_Result
-- Description: Stores detailed validation execution results
-- Each row represents a failed/warned validation for a specific record or data point
-- For table validations: stores table row identifier
-- For cube validations: stores dimensional POV (Entity, Scenario, Time, etc.)
-- =============================================

CREATE TABLE [dbo].[FMM_Validation_Result](
	[Validation_Result_ID] [int] IDENTITY(1,1) NOT NULL,
	[Validation_Config_ID] [int] NOT NULL,
	[Run_ID] [int] NOT NULL,
	[Run_Date] [datetime] NOT NULL,
	[Run_User] [nvarchar](50) NOT NULL,
	[Validation_Context] [nvarchar](20) NOT NULL,               -- 'TABLE' or 'CUBE'
	
	-- Table context fields (populated when Validation_Context = 'TABLE')
	[Table_Name] [nvarchar](255) NULL,
	[Primary_Key_Value] [nvarchar](500) NULL,                   -- Primary key value(s) of the failed row
	[Column_Name] [nvarchar](255) NULL,                         -- Specific column that failed validation
	[Column_Value] [nvarchar](max) NULL,                        -- Actual value that failed
	
	-- Cube context fields (populated when Validation_Context = 'CUBE')
	[Cube_POV] [nvarchar](max) NULL,                            -- JSON: {"Scenario":"Actual","Entity":"E001","Account":"A100",...}
	[Dimension_Values] [nvarchar](max) NULL,                    -- Human-readable dimension member names
	[Cell_Value] [decimal](28, 8) NULL,                         -- Numeric value for cube validations
	[Comparison_Value] [decimal](28, 8) NULL,                   -- Comparison value (e.g., other scenario, threshold)
	
	-- Common result fields
	[Validation_Status] [nvarchar](20) NOT NULL,                -- 'Failed', 'Warning', 'Info'
	[Error_Message] [nvarchar](max) NULL,
	[Error_Details] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL DEFAULT GETDATE(),
    
    CONSTRAINT PK_FMM_Validation_Result PRIMARY KEY CLUSTERED ([Validation_Result_ID] ASC),
    
    CONSTRAINT FK_FMM_Validation_Result_Config FOREIGN KEY ([Validation_Config_ID]) 
		REFERENCES [dbo].[FMM_Validation_Config]([Validation_Config_ID]) ON DELETE CASCADE,
    
    CONSTRAINT FK_FMM_Validation_Result_Run FOREIGN KEY ([Run_ID]) 
		REFERENCES [dbo].[FMM_Validation_Run]([Run_ID]) ON DELETE CASCADE,
	
	CONSTRAINT CK_FMM_Validation_Result_Context CHECK ([Validation_Context] IN ('TABLE', 'CUBE')),
	CONSTRAINT CK_FMM_Validation_Result_Status CHECK ([Validation_Status] IN ('Failed', 'Warning', 'Info'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indexes for efficient result queries
CREATE NONCLUSTERED INDEX IX_FMM_Validation_Result_Config_ID 
ON FMM_Validation_Result (Validation_Config_ID, Validation_Status);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Result_Run_ID 
ON FMM_Validation_Result (Run_ID, Validation_Status) 
INCLUDE (Validation_Config_ID, Run_Date, Error_Message);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Result_Context 
ON FMM_Validation_Result (Validation_Context, Validation_Status, Run_Date DESC);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Validation_Result_Table 
ON FMM_Validation_Result (Table_Name, Primary_Key_Value) 
WHERE Table_Name IS NOT NULL;
GO

-- =============================================
-- Helper Views for Easier Querying
-- =============================================

-- View: Complete validation configuration with run statistics
CREATE VIEW dbo.vw_FMM_Validation_Config_Summary AS
SELECT 
    vc.Validation_Config_ID,
    vc.Name,
    vc.Description,
    vc.Validation_Context,
    vc.Validation_Type,
    vc.Target_Object,
    vc.Process_Type,
    vc.Is_Active,
    vc.Severity,
    vc.Create_Date,
    vc.Create_User,
    vc.Update_Date,
    vc.Update_User,
    COUNT(DISTINCT vr.Run_ID) AS Total_Runs,
    COUNT(CASE WHEN vr.Validation_Status = 'Failed' THEN 1 END) AS Total_Failures,
    COUNT(CASE WHEN vr.Validation_Status = 'Warning' THEN 1 END) AS Total_Warnings,
    MAX(vr.Run_Date) AS Last_Run_Date
FROM dbo.FMM_Validation_Config vc
LEFT JOIN dbo.FMM_Validation_Result vr ON vc.Validation_Config_ID = vr.Validation_Config_ID
GROUP BY 
    vc.Validation_Config_ID, vc.Name, vc.Description, vc.Validation_Context,
    vc.Validation_Type, vc.Target_Object, vc.Process_Type, vc.Is_Active,
    vc.Severity, vc.Create_Date, vc.Create_User, vc.Update_Date, vc.Update_User;
GO

-- View: Validation results with configuration details
CREATE VIEW dbo.vw_FMM_Validation_Results AS
SELECT 
    vr.Validation_Result_ID,
    vr.Run_ID,
    vr.Run_Date,
    vr.Run_User,
    vr.Validation_Context,
    vc.Name AS Validation_Name,
    vc.Validation_Type,
    vc.Target_Object,
    vc.Process_Type,
    vc.Severity,
    vr.Table_Name,
    vr.Primary_Key_Value,
    vr.Column_Name,
    vr.Column_Value,
    vr.Cube_POV,
    vr.Dimension_Values,
    vr.Cell_Value,
    vr.Comparison_Value,
    vr.Validation_Status,
    vr.Error_Message,
    vr.Error_Details,
    vr.Create_Date
FROM dbo.FMM_Validation_Result vr
INNER JOIN dbo.FMM_Validation_Config vc ON vr.Validation_Config_ID = vc.Validation_Config_ID;
GO

-- View: Validation run summary with statistics
CREATE VIEW dbo.vw_FMM_Validation_Run_Summary AS
SELECT 
    vrun.Run_ID,
    vrun.Run_Date,
    vrun.Run_User,
    vrun.Validation_Context,
    vrun.Process_Type,
    vrun.Target_Object,
    vrun.Total_Validations,
    vrun.Total_Records_Checked,
    vrun.Total_Failures,
    vrun.Total_Warnings,
    vrun.Total_Info,
    vrun.Execution_Time_Ms,
    vrun.Status,
    vrun.Error_Message,
    vrun.Notes,
    COUNT(DISTINCT vr.Validation_Config_ID) AS Validations_With_Results,
    COUNT(vr.Validation_Result_ID) AS Total_Result_Records
FROM dbo.FMM_Validation_Run vrun
LEFT JOIN dbo.FMM_Validation_Result vr ON vrun.Run_ID = vr.Run_ID
GROUP BY 
    vrun.Run_ID, vrun.Run_Date, vrun.Run_User, vrun.Validation_Context,
    vrun.Process_Type, vrun.Target_Object, vrun.Total_Validations,
    vrun.Total_Records_Checked, vrun.Total_Failures, vrun.Total_Warnings,
    vrun.Total_Info, vrun.Execution_Time_Ms, vrun.Status, vrun.Error_Message, vrun.Notes;
GO

PRINT 'FMM Validation Framework schema created successfully.';
GO

-- =============================================
-- Example Table Validation Configurations
-- =============================================

-- Example 1: Column Value List Validation
-- Validates that a column value is in a predefined list
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    1,
    'Register Plan Status Validation',
    'Validates that Status column contains only valid status values',
    'TABLE',
    'ColumnValueList',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"ColumnName": "Status", "ValidValues": ["Draft", "Submitted", "Approved", "Rejected", "Closed"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 2: Column Value Range Validation
-- Validates that a numeric column is within a specified range
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    2,
    'Register Plan Monthly Amount Range',
    'Validates that monthly amounts are within reasonable bounds',
    'TABLE',
    'ColumnValueRange',
    'Register_Plan_Details',
    'Register_Plan',
    1,
    'Warning',
    '{"ColumnName": "Month1", "MinValue": -1000000000, "MaxValue": 1000000000, "IncludeNull": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 3: Required Columns Validation
-- Validates that required columns are not NULL or empty
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    3,
    'Register Plan Required Fields',
    'Validates that required fields are populated',
    'TABLE',
    'RequiredColumns',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"RequiredColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 4: Unique Constraint Validation
-- Validates uniqueness of column combinations
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    4,
    'Register Plan Unique Key',
    'Validates that combination of workflow fields is unique',
    'TABLE',
    'UniqueConstraint',
    'Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"UniqueColumns": ["WF_Scenario_Name", "WF_Profile_Name", "Activity_ID", "Entity", "Account"]}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- =============================================
-- Example Cube Validation Configurations
-- =============================================

-- Example 5: Scenario Comparison Validation
-- Compares values between two scenarios across dimensions
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    5,
    'Actual vs Budget Variance Check',
    'Validates that Actual values do not exceed Budget by more than 10%',
    'CUBE',
    'ScenarioComparison',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Warning',
    '{"BaseScenario": "S#Budget", "CompareScenario": "S#Actual", "VarianceType": "Percentage", "Threshold": 10, "Dimensions": ["Entity", "Account", "Time"], "FilterPOV": {"Flow": "F#EndBalance", "View": "V#Periodic"}}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 6: Dimensional Balance Validation
-- Validates that data balances across dimensional hierarchies
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    6,
    'Entity Consolidation Balance',
    'Validates that parent entity totals equal sum of children',
    'CUBE',
    'DimensionalBalance',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"BalanceDimension": "Entity", "AggregationMethod": "Sum", "Tolerance": 0.01, "CheckAllParents": true, "FixedPOV": {"Scenario": "S#Actual", "Flow": "F#EndBalance"}}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 7: Temporal Consistency Validation
-- Validates data consistency across time periods
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    7,
    'Month to Quarter Rollup Check',
    'Validates that quarterly amounts equal sum of monthly amounts',
    'CUBE',
    'TemporalConsistency',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"TimeDimension": "Time", "BaseLevel": "Month", "RollupLevel": "Quarter", "Tolerance": 0.01, "CheckYearly": true}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 8: Variance Threshold Validation
-- Validates that variances are within acceptable thresholds
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    8,
    'Budget Variance Threshold',
    'Validates that budget variances are within acceptable limits',
    'CUBE',
    'VarianceThreshold',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Warning',
    '{"BaseScenario": "S#Budget", "CompareScenario": "S#Forecast", "ThresholdType": "Absolute", "MinThreshold": -100000, "MaxThreshold": 100000, "AccountFilter": "A#[Revenue].Base"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/

-- Example 9: Custom Cube SQL Validation
-- Executes custom SQL query for complex cube validations
/*
INSERT INTO FMM_Validation_Config (
    Validation_Config_ID, Name, Description, Validation_Context, Validation_Type,
    Target_Object, Process_Type, Is_Active, Severity, Config_JSON,
    Create_Date, Create_User, Update_Date, Update_User
)
VALUES (
    9,
    'Custom Allocation Check',
    'Custom SQL to validate allocation logic across entities',
    'CUBE',
    'CustomCubeSQL',
    'CubeView_Register_Plan',
    'Register_Plan',
    1,
    'Error',
    '{"SQLQuery": "SELECT EntityName, AccountName, SUM(Amount) as Total FROM vw_CubeData WHERE ScenarioName = ''Actual'' AND TimeName = ''2024M01'' GROUP BY EntityName, AccountName HAVING SUM(Amount) < 0 AND AccountName LIKE ''Revenue%''", "ErrorMessage": "Negative revenue found in allocation"}',
    GETDATE(), 'admin', GETDATE(), 'admin'
);
*/
