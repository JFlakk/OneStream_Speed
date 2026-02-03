-- =============================================
-- Configurable Workflow System
-- Database Schema Definition
-- Supports both Cube and Table-based workflows
-- with configurable approval levels
-- =============================================

-- Drop tables in reverse dependency order if they exist
IF OBJECT_ID('dbo.WF_Approval_Level_Dimension', 'U') IS NOT NULL DROP TABLE dbo.WF_Approval_Level_Dimension;
IF OBJECT_ID('dbo.WF_Approval_Level_Column', 'U') IS NOT NULL DROP TABLE dbo.WF_Approval_Level_Column;
IF OBJECT_ID('dbo.WF_Step_Config', 'U') IS NOT NULL DROP TABLE dbo.WF_Step_Config;
IF OBJECT_ID('dbo.WF_Data_Source_Config', 'U') IS NOT NULL DROP TABLE dbo.WF_Data_Source_Config;
IF OBJECT_ID('dbo.WF_Config', 'U') IS NOT NULL DROP TABLE dbo.WF_Config;
GO

-- =============================================
-- 1. WF_Config - Master Workflow Configuration
-- =============================================
CREATE TABLE dbo.WF_Config (
    WF_Config_ID INT IDENTITY(1,1) NOT NULL,
    Workflow_Name NVARCHAR(255) NOT NULL,
    Workflow_Type NVARCHAR(50) NOT NULL,                    -- 'Approval', 'Review', 'Certification', etc.
    Process_Type NVARCHAR(100) NOT NULL,                    -- Links to specific business process
    Description NVARCHAR(500) NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Require_Comments BIT NOT NULL DEFAULT 0,
    Enable_Delegation BIT NOT NULL DEFAULT 0,
    Enable_Notifications BIT NOT NULL DEFAULT 1,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_WF_Config PRIMARY KEY CLUSTERED (WF_Config_ID),
    CONSTRAINT UQ_Workflow_Name UNIQUE (Workflow_Name),
    CONSTRAINT CHK_Workflow_Type CHECK (Workflow_Type IN ('Approval', 'Review', 'Certification', 'Validation', 'Custom'))
);
GO

-- Create index on Process_Type for common queries
CREATE NONCLUSTERED INDEX IX_WF_Config_Process 
ON dbo.WF_Config (Process_Type, Is_Active) 
INCLUDE (Workflow_Name, Workflow_Type);
GO

-- =============================================
-- 2. WF_Data_Source_Config - Data Source Configuration
-- Supports both Cube and Table data sources
-- =============================================
CREATE TABLE dbo.WF_Data_Source_Config (
    Data_Source_Config_ID INT IDENTITY(1,1) NOT NULL,
    WF_Config_ID INT NOT NULL,
    Data_Source_Type NVARCHAR(50) NOT NULL,                 -- 'Cube', 'Table', 'RelationalView', 'CustomQuery'
    
    -- For Cube data sources
    Cube_Name NVARCHAR(255) NULL,
    
    -- For Table data sources
    Table_Name NVARCHAR(255) NULL,
    Schema_Name NVARCHAR(50) NULL DEFAULT 'dbo',
    
    -- For Custom Query data sources
    Custom_Query NVARCHAR(MAX) NULL,
    
    -- Common properties
    Description NVARCHAR(500) NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_WF_Data_Source_Config PRIMARY KEY CLUSTERED (Data_Source_Config_ID),
    CONSTRAINT FK_DataSource_Workflow FOREIGN KEY (WF_Config_ID) 
        REFERENCES dbo.WF_Config(WF_Config_ID) ON DELETE CASCADE,
    CONSTRAINT CHK_Data_Source_Type CHECK (Data_Source_Type IN ('Cube', 'Table', 'RelationalView', 'CustomQuery')),
    CONSTRAINT CHK_Cube_Name CHECK (
        Data_Source_Type != 'Cube' OR Cube_Name IS NOT NULL
    ),
    CONSTRAINT CHK_Table_Name CHECK (
        Data_Source_Type NOT IN ('Table', 'RelationalView') OR Table_Name IS NOT NULL
    ),
    CONSTRAINT CHK_Custom_Query CHECK (
        Data_Source_Type != 'CustomQuery' OR Custom_Query IS NOT NULL
    )
);
GO

-- Create indexes for data source lookups
CREATE NONCLUSTERED INDEX IX_WF_Data_Source_Config_Workflow 
ON dbo.WF_Data_Source_Config (WF_Config_ID, Is_Active);

CREATE NONCLUSTERED INDEX IX_WF_Data_Source_Config_Type 
ON dbo.WF_Data_Source_Config (Data_Source_Type, Is_Active);
GO

-- =============================================
-- 3. WF_Step_Config - Workflow Step Configuration
-- =============================================
CREATE TABLE dbo.WF_Step_Config (
    Step_Config_ID INT IDENTITY(1,1) NOT NULL,
    WF_Config_ID INT NOT NULL,
    Step_Name NVARCHAR(255) NOT NULL,
    Step_Order INT NOT NULL,
    Step_Type NVARCHAR(50) NOT NULL,                        -- 'Approval', 'Review', 'Notification', 'Validation'
    
    -- Approval configuration
    Approval_Type NVARCHAR(50) NULL,                        -- 'Single', 'All', 'Majority', 'Custom'
    Min_Approvals INT NULL,                                 -- For custom approval types
    
    -- Timeout and escalation
    Timeout_Hours INT NULL,
    Escalation_Step_Config_ID INT NULL,                     -- Step to escalate to on timeout
    
    -- User/role assignment
    Assigned_Role NVARCHAR(255) NULL,
    Assigned_User NVARCHAR(255) NULL,
    Assignment_Rule NVARCHAR(MAX) NULL,                     -- Custom assignment logic
    
    -- Navigation
    Next_Step_Config_ID INT NULL,                           -- Next step on approval
    Rejection_Step_Config_ID INT NULL,                      -- Step to go to on rejection
    
    Is_Active BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500) NULL,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_WF_Step_Config PRIMARY KEY CLUSTERED (Step_Config_ID),
    CONSTRAINT FK_Step_Workflow FOREIGN KEY (WF_Config_ID) 
        REFERENCES dbo.WF_Config(WF_Config_ID) ON DELETE CASCADE,
    CONSTRAINT FK_Step_Escalation FOREIGN KEY (Escalation_Step_Config_ID) 
        REFERENCES dbo.WF_Step_Config(Step_Config_ID),
    CONSTRAINT FK_Step_Next FOREIGN KEY (Next_Step_Config_ID) 
        REFERENCES dbo.WF_Step_Config(Step_Config_ID),
    CONSTRAINT FK_Step_Rejection FOREIGN KEY (Rejection_Step_Config_ID) 
        REFERENCES dbo.WF_Step_Config(Step_Config_ID),
    CONSTRAINT UQ_Step_Order UNIQUE (WF_Config_ID, Step_Order),
    CONSTRAINT CHK_Step_Type CHECK (Step_Type IN ('Approval', 'Review', 'Notification', 'Validation', 'Custom')),
    CONSTRAINT CHK_Approval_Type CHECK (Approval_Type IS NULL OR Approval_Type IN ('Single', 'All', 'Majority', 'Custom')),
    CONSTRAINT CHK_Min_Approvals CHECK (Approval_Type != 'Custom' OR Min_Approvals IS NOT NULL)
);
GO

-- Create indexes for step lookups
CREATE NONCLUSTERED INDEX IX_WF_Step_Config_Workflow 
ON dbo.WF_Step_Config (WF_Config_ID, Step_Order, Is_Active);
GO

-- =============================================
-- 4. WF_Approval_Level_Column - Column-based Approval Level Configuration
-- For Table data sources - maps columns to approval levels
-- =============================================
CREATE TABLE dbo.WF_Approval_Level_Column (
    Approval_Level_Column_ID INT IDENTITY(1,1) NOT NULL,
    Step_Config_ID INT NOT NULL,
    Data_Source_Config_ID INT NOT NULL,
    
    -- Column configuration
    Column_Name NVARCHAR(255) NOT NULL,
    Column_Type NVARCHAR(50) NOT NULL,                      -- 'Direct', 'Derived', 'Lookup'
    
    -- For derived columns
    Derivation_Logic NVARCHAR(MAX) NULL,                    -- SQL expression or rule name
    
    -- For lookup columns
    Lookup_Table NVARCHAR(255) NULL,
    Lookup_Column NVARCHAR(255) NULL,
    Lookup_Key_Column NVARCHAR(255) NULL,
    
    -- Approval level mapping
    Level_Order INT NOT NULL,                               -- Order in hierarchy (1=highest)
    Require_All_Values BIT NOT NULL DEFAULT 0,              -- Approve all distinct values vs aggregate
    
    Is_Active BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500) NULL,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_WF_Approval_Level_Column PRIMARY KEY CLUSTERED (Approval_Level_Column_ID),
    CONSTRAINT FK_ApprovalCol_Step FOREIGN KEY (Step_Config_ID) 
        REFERENCES dbo.WF_Step_Config(Step_Config_ID) ON DELETE CASCADE,
    CONSTRAINT FK_ApprovalCol_DataSource FOREIGN KEY (Data_Source_Config_ID) 
        REFERENCES dbo.WF_Data_Source_Config(Data_Source_Config_ID),
    CONSTRAINT CHK_Column_Type CHECK (Column_Type IN ('Direct', 'Derived', 'Lookup')),
    CONSTRAINT CHK_Derived_Logic CHECK (Column_Type != 'Derived' OR Derivation_Logic IS NOT NULL),
    CONSTRAINT CHK_Lookup_Config CHECK (
        Column_Type != 'Lookup' OR (
            Lookup_Table IS NOT NULL AND 
            Lookup_Column IS NOT NULL AND 
            Lookup_Key_Column IS NOT NULL
        )
    )
);
GO

-- Create indexes for approval level column lookups
CREATE NONCLUSTERED INDEX IX_WF_Approval_Level_Column_Step 
ON dbo.WF_Approval_Level_Column (Step_Config_ID, Level_Order, Is_Active);

CREATE NONCLUSTERED INDEX IX_WF_Approval_Level_Column_DataSource 
ON dbo.WF_Approval_Level_Column (Data_Source_Config_ID, Is_Active);
GO

-- =============================================
-- 5. WF_Approval_Level_Dimension - Dimension-based Approval Level Configuration
-- For Cube data sources - maps dimensions to approval levels
-- =============================================
CREATE TABLE dbo.WF_Approval_Level_Dimension (
    Approval_Level_Dimension_ID INT IDENTITY(1,1) NOT NULL,
    Step_Config_ID INT NOT NULL,
    Data_Source_Config_ID INT NOT NULL,
    
    -- Dimension configuration
    Dimension_Name NVARCHAR(255) NOT NULL,
    Hierarchy_Name NVARCHAR(255) NULL,                      -- Optional specific hierarchy
    Level_Name NVARCHAR(255) NULL,                          -- Optional specific level in hierarchy
    
    -- Member filtering
    Member_Filter NVARCHAR(MAX) NULL,                       -- MDX filter expression
    Property_Name NVARCHAR(255) NULL,                       -- Member property to use for approval
    
    -- Approval level mapping
    Level_Order INT NOT NULL,                               -- Order in hierarchy (1=highest)
    Aggregate_To_Level NVARCHAR(255) NULL,                  -- Aggregate to specific level
    Require_All_Descendants BIT NOT NULL DEFAULT 0,         -- Approve all descendants vs aggregate
    
    Is_Active BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500) NULL,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_WF_Approval_Level_Dimension PRIMARY KEY CLUSTERED (Approval_Level_Dimension_ID),
    CONSTRAINT FK_ApprovalDim_Step FOREIGN KEY (Step_Config_ID) 
        REFERENCES dbo.WF_Step_Config(Step_Config_ID) ON DELETE CASCADE,
    CONSTRAINT FK_ApprovalDim_DataSource FOREIGN KEY (Data_Source_Config_ID) 
        REFERENCES dbo.WF_Data_Source_Config(Data_Source_Config_ID)
);
GO

-- Create indexes for approval level dimension lookups
CREATE NONCLUSTERED INDEX IX_WF_Approval_Level_Dimension_Step 
ON dbo.WF_Approval_Level_Dimension (Step_Config_ID, Level_Order, Is_Active);

CREATE NONCLUSTERED INDEX IX_WF_Approval_Level_Dimension_DataSource 
ON dbo.WF_Approval_Level_Dimension (Data_Source_Config_ID, Is_Active);
GO

-- =============================================
-- Create views for easier querying
-- =============================================

-- View to see complete workflow configuration
CREATE VIEW dbo.vw_WF_Complete_Config AS
SELECT 
    wf.WF_Config_ID,
    wf.Workflow_Name,
    wf.Workflow_Type,
    wf.Process_Type,
    wf.Description AS Workflow_Description,
    wf.Is_Active AS Workflow_Is_Active,
    ds.Data_Source_Config_ID,
    ds.Data_Source_Type,
    ds.Cube_Name,
    ds.Table_Name,
    ds.Schema_Name,
    step.Step_Config_ID,
    step.Step_Name,
    step.Step_Order,
    step.Step_Type,
    step.Approval_Type
FROM dbo.WF_Config wf
LEFT JOIN dbo.WF_Data_Source_Config ds ON wf.WF_Config_ID = ds.WF_Config_ID AND ds.Is_Active = 1
LEFT JOIN dbo.WF_Step_Config step ON wf.WF_Config_ID = step.WF_Config_ID AND step.Is_Active = 1
WHERE wf.Is_Active = 1;
GO

-- View to see table-based approval levels
CREATE VIEW dbo.vw_WF_Table_Approval_Levels AS
SELECT 
    wf.Workflow_Name,
    ds.Table_Name,
    ds.Schema_Name,
    step.Step_Name,
    step.Step_Order,
    col.Column_Name,
    col.Column_Type,
    col.Level_Order,
    col.Require_All_Values,
    col.Description AS Column_Description
FROM dbo.WF_Config wf
INNER JOIN dbo.WF_Data_Source_Config ds ON wf.WF_Config_ID = ds.WF_Config_ID
INNER JOIN dbo.WF_Step_Config step ON wf.WF_Config_ID = step.WF_Config_ID
INNER JOIN dbo.WF_Approval_Level_Column col ON step.Step_Config_ID = col.Step_Config_ID 
    AND ds.Data_Source_Config_ID = col.Data_Source_Config_ID
WHERE wf.Is_Active = 1 
    AND ds.Is_Active = 1 
    AND step.Is_Active = 1 
    AND col.Is_Active = 1
    AND ds.Data_Source_Type = 'Table';
GO

-- View to see cube-based approval levels
CREATE VIEW dbo.vw_WF_Cube_Approval_Levels AS
SELECT 
    wf.Workflow_Name,
    ds.Cube_Name,
    step.Step_Name,
    step.Step_Order,
    dim.Dimension_Name,
    dim.Hierarchy_Name,
    dim.Level_Name,
    dim.Level_Order,
    dim.Aggregate_To_Level,
    dim.Require_All_Descendants,
    dim.Description AS Dimension_Description
FROM dbo.WF_Config wf
INNER JOIN dbo.WF_Data_Source_Config ds ON wf.WF_Config_ID = ds.WF_Config_ID
INNER JOIN dbo.WF_Step_Config step ON wf.WF_Config_ID = step.WF_Config_ID
INNER JOIN dbo.WF_Approval_Level_Dimension dim ON step.Step_Config_ID = dim.Step_Config_ID 
    AND ds.Data_Source_Config_ID = dim.Data_Source_Config_ID
WHERE wf.Is_Active = 1 
    AND ds.Is_Active = 1 
    AND step.Is_Active = 1 
    AND dim.Is_Active = 1
    AND ds.Data_Source_Type = 'Cube';
GO

-- =============================================
-- Sample data for demonstration
-- =============================================

-- Sample workflow for table-based approval
INSERT INTO dbo.WF_Config (Workflow_Name, Workflow_Type, Process_Type, Description, Create_User)
VALUES ('Budget Approval - Table', 'Approval', 'Budget', 'Multi-level budget approval workflow using tables', 'System');

DECLARE @TableWFID INT = SCOPE_IDENTITY();

-- Table data source
INSERT INTO dbo.WF_Data_Source_Config (WF_Config_ID, Data_Source_Type, Table_Name, Schema_Name, Description, Create_User)
VALUES (@TableWFID, 'Table', 'Budget_Transactions', 'dbo', 'Budget transaction table', 'System');

DECLARE @TableDSID INT = SCOPE_IDENTITY();

-- Approval steps
INSERT INTO dbo.WF_Step_Config (WF_Config_ID, Step_Name, Step_Order, Step_Type, Approval_Type, Description, Create_User)
VALUES (@TableWFID, 'Department Manager Approval', 1, 'Approval', 'Single', 'Department level approval', 'System');

DECLARE @TableStepID INT = SCOPE_IDENTITY();

-- Column-based approval level
INSERT INTO dbo.WF_Approval_Level_Column (Step_Config_ID, Data_Source_Config_ID, Column_Name, Column_Type, Level_Order, Description, Create_User)
VALUES (@TableStepID, @TableDSID, 'Department', 'Direct', 1, 'Approval at department level', 'System');

-- Sample workflow for cube-based approval
INSERT INTO dbo.WF_Config (Workflow_Name, Workflow_Type, Process_Type, Description, Create_User)
VALUES ('Forecast Approval - Cube', 'Approval', 'Forecast', 'Multi-level forecast approval workflow using cube dimensions', 'System');

DECLARE @CubeWFID INT = SCOPE_IDENTITY();

-- Cube data source
INSERT INTO dbo.WF_Data_Source_Config (WF_Config_ID, Data_Source_Type, Cube_Name, Description, Create_User)
VALUES (@CubeWFID, 'Cube', 'Forecast', 'Forecast cube', 'System');

DECLARE @CubeDSID INT = SCOPE_IDENTITY();

-- Approval steps
INSERT INTO dbo.WF_Step_Config (WF_Config_ID, Step_Name, Step_Order, Step_Type, Approval_Type, Description, Create_User)
VALUES (@CubeWFID, 'Entity Hierarchy Approval', 1, 'Approval', 'Single', 'Approval based on entity hierarchy', 'System');

DECLARE @CubeStepID INT = SCOPE_IDENTITY();

-- Dimension-based approval level
INSERT INTO dbo.WF_Approval_Level_Dimension (Step_Config_ID, Data_Source_Config_ID, Dimension_Name, Hierarchy_Name, Level_Order, Description, Create_User)
VALUES (@CubeStepID, @CubeDSID, 'Entity', 'Default', 1, 'Approval at entity dimension level', 'System');

GO

PRINT 'Configurable Workflow System schema created successfully.';
PRINT 'Sample data inserted for table-based and cube-based workflows.';
GO
