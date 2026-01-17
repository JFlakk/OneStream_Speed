-- =============================================
-- FMM Custom Table Configuration System
-- Sample Data: Register_Plan Configuration
-- =============================================
-- This script demonstrates how to configure the existing
-- Register_Plan and Register_Plan_Details tables using
-- the new custom table configuration system.
-- =============================================

USE [YourDatabase]; -- Replace with your database name
GO

-- =============================================
-- STEP 1: Create Master Table Configuration (Register_Plan)
-- =============================================

DECLARE @MasterTableID INT;
DECLARE @DetailTableID INT;
DECLARE @AuditTableID INT;

-- Insert master table configuration
INSERT INTO dbo.FMM_Table_Config (
    Process_Type, 
    Table_Name, 
    Table_Type, 
    Description,
    Enable_Audit,
    Is_Active,
    Create_User,
    Update_User
)
VALUES (
    'Register_Plan',
    'Register_Plan',
    'Master',
    'Main register plan table storing header information',
    1, -- Enable audit
    1, -- Is Active
    'SYSTEM',
    'SYSTEM'
);

SET @MasterTableID = SCOPE_IDENTITY();
PRINT 'Created Master Table Config ID: ' + CAST(@MasterTableID AS NVARCHAR(10));

-- =============================================
-- STEP 2: Define Columns for Register_Plan
-- =============================================

-- Primary Key Column
INSERT INTO dbo.FMM_Table_Column_Config (
    Table_Config_ID, Column_Name, Data_Type, Max_Length, Precision, Scale,
    Is_Nullable, Is_Identity, Column_Order, Description, Create_User
)
VALUES
    (@MasterTableID, 'Register_Plan_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 0, 1, 'Primary key identifier', 'SYSTEM'),
    
-- Workflow Columns
    (@MasterTableID, 'WF_Scenario_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 2, 'Workflow scenario name', 'SYSTEM'),
    (@MasterTableID, 'WF_Profile_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 3, 'Workflow profile name', 'SYSTEM'),
    (@MasterTableID, 'WF_Time_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 4, 'Workflow time name', 'SYSTEM'),
    (@MasterTableID, 'Activity_ID', 'INT', NULL, NULL, NULL, 0, 0, 5, 'Activity identifier', 'SYSTEM'),
    
-- Approval Columns
    (@MasterTableID, 'Approval_Level_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 1, 0, 6, 'Approval level identifier', 'SYSTEM'),
    
-- Entity Column
    (@MasterTableID, 'Entity', 'NVARCHAR', 255, NULL, NULL, 1, 0, 7, 'Entity name', 'SYSTEM'),
    
-- Attribute Columns (flexible metadata)
    (@MasterTableID, 'Attribute_1', 'NVARCHAR', 255, NULL, NULL, 1, 0, 8, 'Attribute 1', 'SYSTEM'),
    (@MasterTableID, 'Attribute_2', 'NVARCHAR', 255, NULL, NULL, 1, 0, 9, 'Attribute 2', 'SYSTEM'),
    (@MasterTableID, 'Attribute_3', 'NVARCHAR', 255, NULL, NULL, 1, 0, 10, 'Attribute 3', 'SYSTEM'),
    (@MasterTableID, 'Attribute_4', 'NVARCHAR', 255, NULL, NULL, 1, 0, 11, 'Attribute 4', 'SYSTEM'),
    (@MasterTableID, 'Attribute_5', 'NVARCHAR', 255, NULL, NULL, 1, 0, 12, 'Attribute 5', 'SYSTEM'),
    (@MasterTableID, 'Attribute_6', 'NVARCHAR', 255, NULL, NULL, 1, 0, 13, 'Attribute 6', 'SYSTEM'),
    (@MasterTableID, 'Attribute_7', 'NVARCHAR', 255, NULL, NULL, 1, 0, 14, 'Attribute 7', 'SYSTEM'),
    (@MasterTableID, 'Attribute_8', 'NVARCHAR', 255, NULL, NULL, 1, 0, 15, 'Attribute 8', 'SYSTEM'),
    (@MasterTableID, 'Attribute_9', 'NVARCHAR', 255, NULL, NULL, 1, 0, 16, 'Attribute 9', 'SYSTEM'),
    (@MasterTableID, 'Attribute_10', 'NVARCHAR', 255, NULL, NULL, 1, 0, 17, 'Attribute 10', 'SYSTEM'),
    (@MasterTableID, 'Attribute_11', 'NVARCHAR', 255, NULL, NULL, 1, 0, 18, 'Attribute 11', 'SYSTEM'),
    (@MasterTableID, 'Attribute_12', 'NVARCHAR', 255, NULL, NULL, 1, 0, 19, 'Attribute 12', 'SYSTEM'),
    (@MasterTableID, 'Attribute_13', 'NVARCHAR', 255, NULL, NULL, 1, 0, 20, 'Attribute 13', 'SYSTEM'),
    (@MasterTableID, 'Attribute_14', 'NVARCHAR', 255, NULL, NULL, 1, 0, 21, 'Attribute 14', 'SYSTEM'),
    (@MasterTableID, 'Attribute_15', 'NVARCHAR', 255, NULL, NULL, 1, 0, 22, 'Attribute 15', 'SYSTEM'),
    (@MasterTableID, 'Attribute_16', 'NVARCHAR', 255, NULL, NULL, 1, 0, 23, 'Attribute 16', 'SYSTEM'),
    (@MasterTableID, 'Attribute_17', 'NVARCHAR', 255, NULL, NULL, 1, 0, 24, 'Attribute 17', 'SYSTEM'),
    (@MasterTableID, 'Attribute_18', 'NVARCHAR', 255, NULL, NULL, 1, 0, 25, 'Attribute 18', 'SYSTEM'),
    (@MasterTableID, 'Attribute_19', 'NVARCHAR', 255, NULL, NULL, 1, 0, 26, 'Attribute 19', 'SYSTEM'),
    (@MasterTableID, 'Attribute_20', 'NVARCHAR', 255, NULL, NULL, 1, 0, 27, 'Attribute 20', 'SYSTEM'),
    
-- Attribute Value Columns (numeric)
    (@MasterTableID, 'Attribute_Value_1', 'DECIMAL', NULL, 18, 2, 1, 0, 28, 'Attribute value 1', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_2', 'DECIMAL', NULL, 18, 2, 1, 0, 29, 'Attribute value 2', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_3', 'DECIMAL', NULL, 18, 2, 1, 0, 30, 'Attribute value 3', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_4', 'DECIMAL', NULL, 18, 2, 1, 0, 31, 'Attribute value 4', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_5', 'DECIMAL', NULL, 18, 2, 1, 0, 32, 'Attribute value 5', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_6', 'DECIMAL', NULL, 18, 2, 1, 0, 33, 'Attribute value 6', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_7', 'DECIMAL', NULL, 18, 2, 1, 0, 34, 'Attribute value 7', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_8', 'DECIMAL', NULL, 18, 2, 1, 0, 35, 'Attribute value 8', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_9', 'DECIMAL', NULL, 18, 2, 1, 0, 36, 'Attribute value 9', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_10', 'DECIMAL', NULL, 18, 2, 1, 0, 37, 'Attribute value 10', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_11', 'DECIMAL', NULL, 18, 2, 1, 0, 38, 'Attribute value 11', 'SYSTEM'),
    (@MasterTableID, 'Attribute_Value_12', 'DECIMAL', NULL, 18, 2, 1, 0, 39, 'Attribute value 12', 'SYSTEM'),
    
-- Date Value Columns
    (@MasterTableID, 'Date_Value_1', 'DATETIME', NULL, NULL, NULL, 1, 0, 40, 'Date value 1', 'SYSTEM'),
    (@MasterTableID, 'Date_Value_2', 'DATETIME', NULL, NULL, NULL, 1, 0, 41, 'Date value 2', 'SYSTEM'),
    (@MasterTableID, 'Date_Value_3', 'DATETIME', NULL, NULL, NULL, 1, 0, 42, 'Date value 3', 'SYSTEM'),
    (@MasterTableID, 'Date_Value_4', 'DATETIME', NULL, NULL, NULL, 1, 0, 43, 'Date value 4', 'SYSTEM'),
    (@MasterTableID, 'Date_Value_5', 'DATETIME', NULL, NULL, NULL, 1, 0, 44, 'Date value 5', 'SYSTEM'),
    
-- Register ID Columns
    (@MasterTableID, 'Register_ID', 'NVARCHAR', 255, NULL, NULL, 1, 0, 45, 'Register ID', 'SYSTEM'),
    (@MasterTableID, 'Register_ID_1', 'INT', NULL, NULL, NULL, 1, 0, 46, 'Register ID 1', 'SYSTEM'),
    (@MasterTableID, 'Register_ID_2', 'INT', NULL, NULL, NULL, 1, 0, 47, 'Register ID 2', 'SYSTEM'),
    
-- Spread Columns
    (@MasterTableID, 'Spread_Amount', 'DECIMAL', NULL, 18, 2, 1, 0, 48, 'Spread amount', 'SYSTEM'),
    (@MasterTableID, 'Spread_Curve', 'NVARCHAR', 255, NULL, NULL, 1, 0, 49, 'Spread curve', 'SYSTEM'),
    
-- Status Column
    (@MasterTableID, 'Status', 'NVARCHAR', 50, NULL, NULL, 1, 0, 50, 'Status', 'SYSTEM'),
    
-- Invalid Flag
    (@MasterTableID, 'Invalid', 'BIT', NULL, NULL, NULL, 0, 0, 51, 'Invalid flag', 'SYSTEM'),
    
-- Audit Columns
    (@MasterTableID, 'Create_Date', 'DATETIME', NULL, NULL, NULL, 0, 0, 52, 'Record creation date', 'SYSTEM'),
    (@MasterTableID, 'Create_User', 'NVARCHAR', 100, NULL, NULL, 0, 0, 53, 'Record creation user', 'SYSTEM'),
    (@MasterTableID, 'Update_Date', 'DATETIME', NULL, NULL, NULL, 0, 0, 54, 'Last update date', 'SYSTEM'),
    (@MasterTableID, 'Update_User', 'NVARCHAR', 100, NULL, NULL, 0, 0, 55, 'Last update user', 'SYSTEM');

PRINT 'Created ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' columns for Register_Plan';

-- =============================================
-- STEP 3: Define Primary Key Index (Clustered)
-- =============================================

DECLARE @PKIndexID INT;
DECLARE @PKColumnID INT;

-- Create primary key index
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, 
    Index_Name, 
    Index_Type, 
    Is_Clustered, 
    Is_Unique,
    Description,
    Create_User
)
VALUES (
    @MasterTableID,
    'PK_Register_Plan',
    'PRIMARY_KEY',
    1, -- Clustered
    1, -- Unique
    'Primary key on Register_Plan_ID',
    'SYSTEM'
);

SET @PKIndexID = SCOPE_IDENTITY();

-- Get Column_Config_ID for Register_Plan_ID
SELECT @PKColumnID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'Register_Plan_ID';

-- Map primary key column
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID,
    Column_Config_ID,
    Key_Ordinal,
    Sort_Direction,
    Create_User
)
VALUES (
    @PKIndexID,
    @PKColumnID,
    1,
    'ASC',
    'SYSTEM'
);

PRINT 'Created Primary Key Index for Register_Plan';

-- =============================================
-- STEP 4: Define Non-Clustered Index on Workflow Columns
-- =============================================

DECLARE @WFIndexID INT;
DECLARE @ScenarioColID INT, @ProfileColID INT, @TimeColID INT, @ActivityColID INT;

-- Create workflow index
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, 
    Index_Name, 
    Index_Type, 
    Is_Clustered, 
    Is_Unique,
    Description,
    Create_User
)
VALUES (
    @MasterTableID,
    'IX_Register_Plan_WF',
    'NONCLUSTERED',
    0, -- Not Clustered
    0, -- Not Unique
    'Index on workflow columns for efficient querying',
    'SYSTEM'
);

SET @WFIndexID = SCOPE_IDENTITY();

-- Get column IDs for workflow columns
SELECT @ScenarioColID = Column_Config_ID FROM dbo.FMM_Table_Column_Config WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'WF_Scenario_Name';
SELECT @ProfileColID = Column_Config_ID FROM dbo.FMM_Table_Column_Config WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'WF_Profile_Name';
SELECT @TimeColID = Column_Config_ID FROM dbo.FMM_Table_Column_Config WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'WF_Time_Name';
SELECT @ActivityColID = Column_Config_ID FROM dbo.FMM_Table_Column_Config WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'Activity_ID';

-- Map index columns
INSERT INTO dbo.FMM_Table_Index_Column_Config (Index_Config_ID, Column_Config_ID, Key_Ordinal, Sort_Direction, Create_User)
VALUES 
    (@WFIndexID, @ScenarioColID, 1, 'ASC', 'SYSTEM'),
    (@WFIndexID, @ProfileColID, 2, 'ASC', 'SYSTEM'),
    (@WFIndexID, @TimeColID, 3, 'ASC', 'SYSTEM'),
    (@WFIndexID, @ActivityColID, 4, 'ASC', 'SYSTEM');

PRINT 'Created Workflow Index for Register_Plan';

-- =============================================
-- STEP 5: Create Detail Table Configuration (Register_Plan_Details)
-- =============================================

INSERT INTO dbo.FMM_Table_Config (
    Process_Type, 
    Table_Name, 
    Table_Type,
    Parent_Table_Config_ID,
    Description,
    Enable_Audit,
    Is_Active,
    Create_User,
    Update_User
)
VALUES (
    'Register_Plan',
    'Register_Plan_Details',
    'Detail',
    @MasterTableID, -- Link to master table
    'Detail table storing time-phased values for register plans',
    1, -- Enable audit
    1, -- Is Active
    'SYSTEM',
    'SYSTEM'
);

SET @DetailTableID = SCOPE_IDENTITY();
PRINT 'Created Detail Table Config ID: ' + CAST(@DetailTableID AS NVARCHAR(10));

-- =============================================
-- STEP 6: Define Columns for Register_Plan_Details
-- =============================================

INSERT INTO dbo.FMM_Table_Column_Config (
    Table_Config_ID, Column_Name, Data_Type, Max_Length, Precision, Scale,
    Is_Nullable, Is_Identity, Column_Order, Description, Create_User
)
VALUES
    -- Primary Key
    (@DetailTableID, 'Register_Plan_Detail_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 0, 1, 'Primary key identifier', 'SYSTEM'),
    
    -- Foreign Key to Master
    (@DetailTableID, 'Register_Plan_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 0, 0, 2, 'Foreign key to Register_Plan', 'SYSTEM'),
    
    -- Workflow Columns
    (@DetailTableID, 'WF_Scenario_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 3, 'Workflow scenario name', 'SYSTEM'),
    (@DetailTableID, 'WF_Profile_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 4, 'Workflow profile name', 'SYSTEM'),
    (@DetailTableID, 'WF_Time_Name', 'NVARCHAR', 255, NULL, NULL, 0, 0, 5, 'Workflow time name', 'SYSTEM'),
    (@DetailTableID, 'Activity_ID', 'INT', NULL, NULL, NULL, 0, 0, 6, 'Activity identifier', 'SYSTEM'),
    (@DetailTableID, 'Model_ID', 'INT', NULL, NULL, NULL, 1, 0, 7, 'Model identifier', 'SYSTEM'),
    
    -- Entity and Approval
    (@DetailTableID, 'Entity', 'NVARCHAR', 255, NULL, NULL, 1, 0, 8, 'Entity name', 'SYSTEM'),
    (@DetailTableID, 'Approval_Level_ID', 'UNIQUEIDENTIFIER', NULL, NULL, NULL, 1, 0, 9, 'Approval level identifier', 'SYSTEM'),
    
    -- Plan Units
    (@DetailTableID, 'Plan_Units', 'NVARCHAR', 255, NULL, NULL, 1, 0, 10, 'Plan units', 'SYSTEM'),
    
    -- Year
    (@DetailTableID, 'Year', 'NVARCHAR', 10, NULL, NULL, 1, 0, 11, 'Year', 'SYSTEM'),
    
    -- Monthly Values
    (@DetailTableID, 'Month1', 'DECIMAL', NULL, 18, 2, 1, 0, 12, 'January value', 'SYSTEM'),
    (@DetailTableID, 'Month2', 'DECIMAL', NULL, 18, 2, 1, 0, 13, 'February value', 'SYSTEM'),
    (@DetailTableID, 'Month3', 'DECIMAL', NULL, 18, 2, 1, 0, 14, 'March value', 'SYSTEM'),
    (@DetailTableID, 'Month4', 'DECIMAL', NULL, 18, 2, 1, 0, 15, 'April value', 'SYSTEM'),
    (@DetailTableID, 'Month5', 'DECIMAL', NULL, 18, 2, 1, 0, 16, 'May value', 'SYSTEM'),
    (@DetailTableID, 'Month6', 'DECIMAL', NULL, 18, 2, 1, 0, 17, 'June value', 'SYSTEM'),
    (@DetailTableID, 'Month7', 'DECIMAL', NULL, 18, 2, 1, 0, 18, 'July value', 'SYSTEM'),
    (@DetailTableID, 'Month8', 'DECIMAL', NULL, 18, 2, 1, 0, 19, 'August value', 'SYSTEM'),
    (@DetailTableID, 'Month9', 'DECIMAL', NULL, 18, 2, 1, 0, 20, 'September value', 'SYSTEM'),
    (@DetailTableID, 'Month10', 'DECIMAL', NULL, 18, 2, 1, 0, 21, 'October value', 'SYSTEM'),
    (@DetailTableID, 'Month11', 'DECIMAL', NULL, 18, 2, 1, 0, 22, 'November value', 'SYSTEM'),
    (@DetailTableID, 'Month12', 'DECIMAL', NULL, 18, 2, 1, 0, 23, 'December value', 'SYSTEM'),
    
    -- Quarterly Values
    (@DetailTableID, 'Quarter1', 'DECIMAL', NULL, 18, 2, 1, 0, 24, 'Q1 value', 'SYSTEM'),
    (@DetailTableID, 'Quarter2', 'DECIMAL', NULL, 18, 2, 1, 0, 25, 'Q2 value', 'SYSTEM'),
    (@DetailTableID, 'Quarter3', 'DECIMAL', NULL, 18, 2, 1, 0, 26, 'Q3 value', 'SYSTEM'),
    (@DetailTableID, 'Quarter4', 'DECIMAL', NULL, 18, 2, 1, 0, 27, 'Q4 value', 'SYSTEM'),
    
    -- Yearly Value
    (@DetailTableID, 'Yearly', 'DECIMAL', NULL, 18, 2, 1, 0, 28, 'Yearly total value', 'SYSTEM'),
    
    -- Audit Columns
    (@DetailTableID, 'Create_Date', 'DATETIME', NULL, NULL, NULL, 0, 0, 29, 'Record creation date', 'SYSTEM'),
    (@DetailTableID, 'Create_User', 'NVARCHAR', 100, NULL, NULL, 0, 0, 30, 'Record creation user', 'SYSTEM'),
    (@DetailTableID, 'Update_Date', 'DATETIME', NULL, NULL, NULL, 0, 0, 31, 'Last update date', 'SYSTEM'),
    (@DetailTableID, 'Update_User', 'NVARCHAR', 100, NULL, NULL, 0, 0, 32, 'Last update user', 'SYSTEM');

PRINT 'Created ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' columns for Register_Plan_Details';

-- =============================================
-- STEP 7: Define Primary Key Index for Detail Table
-- =============================================

DECLARE @DetailPKIndexID INT;
DECLARE @DetailPKColumnID INT;

-- Create primary key index
INSERT INTO dbo.FMM_Table_Index_Config (
    Table_Config_ID, 
    Index_Name, 
    Index_Type, 
    Is_Clustered, 
    Is_Unique,
    Description,
    Create_User
)
VALUES (
    @DetailTableID,
    'PK_Register_Plan_Details',
    'PRIMARY_KEY',
    1, -- Clustered
    1, -- Unique
    'Primary key on Register_Plan_Detail_ID',
    'SYSTEM'
);

SET @DetailPKIndexID = SCOPE_IDENTITY();

-- Get Column_Config_ID for Register_Plan_Detail_ID
SELECT @DetailPKColumnID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @DetailTableID AND Column_Name = 'Register_Plan_Detail_ID';

-- Map primary key column
INSERT INTO dbo.FMM_Table_Index_Column_Config (
    Index_Config_ID,
    Column_Config_ID,
    Key_Ordinal,
    Sort_Direction,
    Create_User
)
VALUES (
    @DetailPKIndexID,
    @DetailPKColumnID,
    1,
    'ASC',
    'SYSTEM'
);

PRINT 'Created Primary Key Index for Register_Plan_Details';

-- =============================================
-- STEP 8: Define Foreign Key from Detail to Master
-- =============================================

DECLARE @FKID INT;
DECLARE @SourceFKColID INT;
DECLARE @TargetFKColID INT;

-- Create foreign key configuration
INSERT INTO dbo.FMM_Table_FK_Config (
    FK_Name,
    Source_Table_Config_ID,
    Target_Table_Config_ID,
    On_Delete_Action,
    On_Update_Action,
    Description,
    Create_User
)
VALUES (
    'FK_RegPlanDetails_RegPlan',
    @DetailTableID,
    @MasterTableID,
    'CASCADE', -- Delete details when master is deleted
    'CASCADE', -- Update details when master is updated
    'Foreign key linking details to master register plan',
    'SYSTEM'
);

SET @FKID = SCOPE_IDENTITY();

-- Get column IDs for FK
SELECT @SourceFKColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @DetailTableID AND Column_Name = 'Register_Plan_ID';

SELECT @TargetFKColID = Column_Config_ID 
FROM dbo.FMM_Table_Column_Config 
WHERE Table_Config_ID = @MasterTableID AND Column_Name = 'Register_Plan_ID';

-- Map FK columns
INSERT INTO dbo.FMM_Table_FK_Column_Config (
    FK_Config_ID,
    Source_Column_Config_ID,
    Target_Column_Config_ID,
    Column_Ordinal,
    Create_User
)
VALUES (
    @FKID,
    @SourceFKColID,
    @TargetFKColID,
    1,
    'SYSTEM'
);

PRINT 'Created Foreign Key from Register_Plan_Details to Register_Plan';

-- =============================================
-- STEP 9: Create Audit Table Configuration
-- =============================================

INSERT INTO dbo.FMM_Table_Config (
    Process_Type, 
    Table_Name, 
    Table_Type,
    Parent_Table_Config_ID,
    Description,
    Enable_Audit,
    Is_Active,
    Create_User,
    Update_User
)
VALUES (
    'Register_Plan',
    'Register_Plan_Audit',
    'Audit',
    @MasterTableID,
    'Audit table for tracking changes to Register_Plan',
    0, -- No audit for audit table itself
    1, -- Is Active
    'SYSTEM',
    'SYSTEM'
);

SET @AuditTableID = SCOPE_IDENTITY();
PRINT 'Created Audit Table Config ID: ' + CAST(@AuditTableID AS NVARCHAR(10));

-- =============================================
-- STEP 10: Link Audit Table to Source Table
-- =============================================

-- Update master table to reference audit table
UPDATE dbo.FMM_Table_Config 
SET Audit_Table_Config_ID = @AuditTableID 
WHERE Table_Config_ID = @MasterTableID;

-- Create audit configuration
INSERT INTO dbo.FMM_Table_Audit_Config (
    Source_Table_Config_ID,
    Audit_Table_Config_ID,
    Track_Inserts,
    Track_Updates,
    Track_Deletes,
    Audit_User_Column,
    Audit_Date_Column,
    Audit_Action_Column,
    Create_User
)
VALUES (
    @MasterTableID,
    @AuditTableID,
    1, -- Track inserts
    1, -- Track updates
    1, -- Track deletes
    'Audit_User',
    'Audit_Date',
    'Audit_Action',
    'SYSTEM'
);

PRINT 'Created Audit Configuration for Register_Plan';

-- =============================================
-- STEP 11: Verification Queries
-- =============================================

PRINT '';
PRINT '===== CONFIGURATION SUMMARY =====';
PRINT '';

-- Summary of tables configured
SELECT 
    Table_Config_ID,
    Process_Type,
    Table_Name,
    Table_Type,
    Parent_Table_Config_ID,
    Enable_Audit,
    Audit_Table_Config_ID
FROM dbo.FMM_Table_Config
WHERE Process_Type = 'Register_Plan'
ORDER BY Table_Type, Table_Config_ID;

PRINT '';
PRINT 'Total columns configured:';
SELECT 
    tc.Table_Name,
    COUNT(*) AS Column_Count
FROM dbo.FMM_Table_Config tc
INNER JOIN dbo.FMM_Table_Column_Config cc ON tc.Table_Config_ID = cc.Table_Config_ID
WHERE tc.Process_Type = 'Register_Plan'
GROUP BY tc.Table_Name;

PRINT '';
PRINT 'Indexes configured:';
SELECT 
    tc.Table_Name,
    ic.Index_Name,
    ic.Index_Type,
    ic.Is_Clustered,
    ic.Is_Unique
FROM dbo.FMM_Table_Config tc
INNER JOIN dbo.FMM_Table_Index_Config ic ON tc.Table_Config_ID = ic.Table_Config_ID
WHERE tc.Process_Type = 'Register_Plan'
ORDER BY tc.Table_Name, ic.Index_Config_ID;

PRINT '';
PRINT 'Foreign keys configured:';
SELECT * FROM dbo.vw_FMM_Table_ForeignKeys;

PRINT '';
PRINT 'Sample configuration complete!';
GO
