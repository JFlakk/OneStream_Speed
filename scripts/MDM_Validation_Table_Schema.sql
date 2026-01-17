-- =============================================
-- Table: MDM_Validation_Config
-- Description: Configuration table for dimension validations in Model Dimension Manager
-- This table stores the configuration for various types of dimension member validations
-- 
-- NOTE: Validation_Config_ID is NOT an IDENTITY column - IDs must be manually assigned or managed
--       through application logic. This allows for controlled ID assignment and potential
--       synchronization across environments.
-- 
-- Dependencies: This table requires the following OneStream core tables to exist:
--   - Dim: Standard OneStream dimension table
-- These tables are part of the OneStream core database schema and should exist in all environments.
-- If the foreign key constraints fail, verify these tables exist before creating this table.
-- =============================================

CREATE TABLE [dbo].[MDM_Validation_Config](
	[Validation_Config_ID] [int] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Dim_Type] [nvarchar](50) NOT NULL,
	[Dim_ID] [int] NOT NULL,
	[Validation_Type] [nvarchar](50) NOT NULL,
	[Is_Active] [bit] NOT NULL,
	[Severity] [nvarchar](20) NOT NULL,
	[Config_JSON] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL,
	[Create_User] [nvarchar](50) NOT NULL,
	[Update_Date] [datetime] NOT NULL,
	[Update_User] [nvarchar](50) NOT NULL,
    CONSTRAINT PK_MDM_Validation_Config PRIMARY KEY CLUSTERED ([Validation_Config_ID] ASC),
    CONSTRAINT FK_MDM_Validation_Config_Dim FOREIGN KEY ([Dim_ID]) REFERENCES [dbo].[Dim]([DimID]),
	CONSTRAINT CK_MDM_Validation_Config_Type CHECK ([Validation_Type] IN (
		'MissingInHierarchy',
		'MissingTextProperty',
		'InvalidTextPropertyValue',
		'DuplicateMembers',
		'OrphanedMembers',
		'CircularReferences',
		'InvalidParentChild',
		'MissingRequiredProperty',
		'CustomSQL'
	)),
	CONSTRAINT CK_MDM_Validation_Config_Severity CHECK ([Severity] IN ('Error', 'Warning', 'Info'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indexes for faster lookups
CREATE NONCLUSTERED INDEX IX_MDM_Validation_Config_Dim_Type 
ON MDM_Validation_Config (Dim_Type);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Config_Dim_ID 
ON MDM_Validation_Config (Dim_ID);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Config_Active 
ON MDM_Validation_Config (Is_Active) 
INCLUDE (Dim_Type, Dim_ID);
GO

-- =============================================
-- Table: MDM_Validation_Result
-- Description: Stores validation execution results
-- Each row represents a member that failed a specific validation
-- This table is designed for regular cleanup/archival of historical results
-- =============================================

CREATE TABLE [dbo].[MDM_Validation_Result](
	[Validation_Result_ID] [int] IDENTITY(1,1) NOT NULL,
	[Validation_Config_ID] [int] NOT NULL,
	[Run_ID] [int] NOT NULL,
	[Run_Date] [datetime] NOT NULL,
	[Run_User] [nvarchar](50) NOT NULL,
	[Member_Name] [nvarchar](255) NOT NULL,
	[Member_ID] [int] NULL,
	[Validation_Status] [nvarchar](20) NOT NULL,
	[Error_Message] [nvarchar](max) NULL,
	[Error_Details] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL,
    CONSTRAINT PK_MDM_Validation_Result PRIMARY KEY CLUSTERED ([Validation_Result_ID] ASC),
    CONSTRAINT FK_MDM_Validation_Result_Config FOREIGN KEY ([Validation_Config_ID]) 
		REFERENCES [dbo].[MDM_Validation_Config]([Validation_Config_ID]) ON DELETE CASCADE,
	CONSTRAINT CK_MDM_Validation_Result_Status CHECK ([Validation_Status] IN ('Failed', 'Passed', 'Warning', 'Error'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Indexes for faster lookups and reporting
CREATE NONCLUSTERED INDEX IX_MDM_Validation_Result_Config_ID 
ON MDM_Validation_Result (Validation_Config_ID);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Result_Run_ID 
ON MDM_Validation_Result (Run_ID) 
INCLUDE (Validation_Config_ID, Run_Date, Validation_Status);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Result_Run_Date 
ON MDM_Validation_Result (Run_Date DESC);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Result_Member 
ON MDM_Validation_Result (Member_Name, Validation_Status);
GO

-- =============================================
-- Table: MDM_Validation_Run
-- Description: Stores metadata about validation runs
-- Tracks when validations were executed and summary statistics
-- =============================================

CREATE TABLE [dbo].[MDM_Validation_Run](
	[Run_ID] [int] NOT NULL,
	[Run_Date] [datetime] NOT NULL,
	[Run_User] [nvarchar](50) NOT NULL,
	[Dim_Type] [nvarchar](50) NULL,
	[Dim_ID] [int] NULL,
	[Total_Validations] [int] NOT NULL,
	[Total_Members_Checked] [int] NOT NULL,
	[Total_Failures] [int] NOT NULL,
	[Total_Warnings] [int] NOT NULL,
	[Execution_Time_Ms] [int] NULL,
	[Status] [nvarchar](20) NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL,
    CONSTRAINT PK_MDM_Validation_Run PRIMARY KEY CLUSTERED ([Run_ID] ASC),
	CONSTRAINT CK_MDM_Validation_Run_Status CHECK ([Status] IN ('Completed', 'Failed', 'InProgress', 'Cancelled'))
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Index for faster run history lookups
CREATE NONCLUSTERED INDEX IX_MDM_Validation_Run_Date 
ON MDM_Validation_Run (Run_Date DESC);
GO

CREATE NONCLUSTERED INDEX IX_MDM_Validation_Run_Dim 
ON MDM_Validation_Run (Dim_Type, Dim_ID);
GO

-- =============================================
-- Example Usage and Validation Types
-- =============================================

-- =============================================
-- 1. MISSING IN HIERARCHY VALIDATION
-- Description: Ensures all members appear in specified hierarchies
-- Config_JSON Example:
-- {
--   "RequiredHierarchies": ["Total Company", "Legal Entity"],
--   "CheckAllParents": true
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     1,
--     'Account Hierarchy Coverage',
--     'Ensures all accounts are represented in both Total Company and Legal Entity hierarchies',
--     'Account',
--     1,
--     'MissingInHierarchy',
--     1,
--     'Error',
--     '{"RequiredHierarchies": ["Total Company", "Legal Entity"], "CheckAllParents": true}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 2. MISSING TEXT PROPERTY VALIDATION
-- Description: Checks if required text properties are populated
-- Config_JSON Example:
-- {
--   "RequiredProperties": ["Text1", "Text2"],
--   "AllowEmpty": false
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     2,
--     'Account Category Required',
--     'Validates that all accounts have Text1 (Category) and Text2 (SubCategory) populated',
--     'Account',
--     1,
--     'MissingTextProperty',
--     1,
--     'Warning',
--     '{"RequiredProperties": ["Text1", "Text2"], "AllowEmpty": false}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 3. INVALID TEXT PROPERTY VALUE VALIDATION
-- Description: Validates text property values against a data source or list
-- Config_JSON Example:
-- {
--   "PropertyName": "Text1",
--   "ValidationType": "List",
--   "ValidValues": ["Asset", "Liability", "Equity", "Revenue", "Expense"],
--   "SourceTable": null,
--   "SourceQuery": null
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     3,
--     'Account Type Validation',
--     'Ensures account type (Text1) contains only valid values',
--     'Account',
--     1,
--     'InvalidTextPropertyValue',
--     1,
--     'Error',
--     '{"PropertyName": "Text1", "ValidationType": "List", "ValidValues": ["Asset", "Liability", "Equity", "Revenue", "Expense"]}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 4. INVALID TEXT PROPERTY VALUE - DATABASE LOOKUP
-- Config_JSON Example:
-- {
--   "PropertyName": "Text3",
--   "ValidationType": "Database",
--   "SourceConnection": "FinanceDB",
--   "SourceQuery": "SELECT DISTINCT Category FROM ValidAccountCategories WHERE Active = 1"
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     4,
--     'Account Category DB Validation',
--     'Validates account categories against source system master list',
--     'Account',
--     1,
--     'InvalidTextPropertyValue',
--     1,
--     'Error',
--     '{"PropertyName": "Text3", "ValidationType": "Database", "SourceConnection": "FinanceDB", "SourceQuery": "SELECT DISTINCT Category FROM ValidAccountCategories WHERE Active = 1"}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 5. DUPLICATE MEMBERS VALIDATION
-- Description: Identifies duplicate member names or descriptions
-- Config_JSON Example:
-- {
--   "CheckFields": ["Name", "Description"],
--   "CaseSensitive": false
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     5,
--     'Duplicate Entity Check',
--     'Identifies duplicate entity names',
--     'Entity',
--     2,
--     'DuplicateMembers',
--     1,
--     'Error',
--     '{"CheckFields": ["Name"], "CaseSensitive": false}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 6. ORPHANED MEMBERS VALIDATION
-- Description: Identifies members with no valid parent relationships
-- Config_JSON Example:
-- {
--   "ExcludeBaseMembers": true,
--   "RequireAtLeastOneParent": true
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     6,
--     'Orphaned Account Check',
--     'Identifies accounts with no parent relationships',
--     'Account',
--     1,
--     'OrphanedMembers',
--     1,
--     'Warning',
--     '{"ExcludeBaseMembers": true, "RequireAtLeastOneParent": true}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 7. CIRCULAR REFERENCES VALIDATION
-- Description: Detects circular parent-child relationships
-- Config_JSON Example:
-- {
--   "MaxDepth": 50,
--   "CheckAllHierarchies": true
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     7,
--     'Circular Reference Detection',
--     'Detects circular parent-child relationships in hierarchies',
--     'CostCenter',
--     3,
--     'CircularReferences',
--     1,
--     'Error',
--     '{"MaxDepth": 50, "CheckAllHierarchies": true}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 8. INVALID PARENT-CHILD VALIDATION
-- Description: Validates parent-child relationships against business rules
-- Config_JSON Example:
-- {
--   "Rules": [
--     {"ParentType": "Summary", "ChildType": "Base", "AllowedTypes": ["Base", "Summary"]},
--     {"ParentType": "Base", "ChildType": "Any", "Allowed": false}
--   ]
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     8,
--     'Parent-Child Type Validation',
--     'Ensures base members cannot have children',
--     'Product',
--     4,
--     'InvalidParentChild',
--     1,
--     'Error',
--     '{"Rules": [{"ParentType": "Base", "ChildType": "Any", "Allowed": false}]}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- 9. CUSTOM SQL VALIDATION
-- Description: Executes custom SQL to validate members
-- Config_JSON Example:
-- {
--   "SQLQuery": "SELECT m.Name FROM Member m WHERE m.DimID = @DimID AND LEN(m.Name) > 50",
--   "ErrorMessage": "Member name exceeds maximum length of 50 characters"
-- }
-- =============================================
-- INSERT INTO MDM_Validation_Config (
--     Validation_Config_ID, Name, Description, Dim_Type, Dim_ID, 
--     Validation_Type, Is_Active, Severity, Config_JSON,
--     Create_Date, Create_User, Update_Date, Update_User
-- )
-- VALUES (
--     9,
--     'Custom Name Length Check',
--     'Validates member names do not exceed 50 characters',
--     'Account',
--     1,
--     'CustomSQL',
--     1,
--     'Warning',
--     '{"SQLQuery": "SELECT m.Name FROM Member m WHERE m.DimID = @DimID AND LEN(m.Name) > 50", "ErrorMessage": "Member name exceeds maximum length"}',
--     GETDATE(), 'admin', GETDATE(), 'admin'
-- );

-- =============================================
-- Example: Insert a validation run
-- =============================================
-- INSERT INTO MDM_Validation_Run (
--     Run_ID, Run_Date, Run_User, Dim_Type, Dim_ID,
--     Total_Validations, Total_Members_Checked, Total_Failures, Total_Warnings,
--     Execution_Time_Ms, Status, Notes, Create_Date
-- )
-- VALUES (
--     1, GETDATE(), 'admin', 'Account', 1,
--     5, 1000, 15, 8, 
--     2500, 'Completed', 'Scheduled validation run', GETDATE()
-- );

-- =============================================
-- Example: Insert validation results
-- =============================================
-- INSERT INTO MDM_Validation_Result (
--     Validation_Config_ID, Run_ID, Run_Date, Run_User,
--     Member_Name, Member_ID, Validation_Status, 
--     Error_Message, Error_Details, Create_Date
-- )
-- VALUES (
--     1, 1, GETDATE(), 'admin',
--     'A1000', 12345, 'Failed',
--     'Member not found in required hierarchy: Total Company',
--     'Member A1000 exists in Legal Entity hierarchy but is missing from Total Company hierarchy',
--     GETDATE()
-- );
