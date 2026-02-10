-- =============================================
-- Dynamic Dashboard Manager (DDM) Database Tables
-- DDL Script for SQL Server
-- =============================================

-- =============================================
-- Table: DDM_Config
-- Description: Main configuration table for Dynamic Dashboard Manager
-- Stores dashboard configuration metadata for both WFProfile and StandAlone types
-- =============================================
CREATE TABLE [dbo].[DDM_Config] (
    [DDM_Config_ID] INT NOT NULL IDENTITY(1,1),
    [DDM_Type] INT NULL,              -- 0=None, 1=WFProfile, 2=StandAlone
    [Scen_Type] INT NULL,             -- Scenario Type for WFProfile configurations
    [Profile_Key] UNIQUEIDENTIFIER NULL,  -- Workflow Profile Key for WFProfile configurations
    
    -- Audit Fields
    [Create_Date] DATETIME NULL,
    [Create_User] NVARCHAR(50) NULL,
    [Update_Date] DATETIME NULL,
    [Update_User] NVARCHAR(50) NULL,
    
    CONSTRAINT [PK_DDM_Config] PRIMARY KEY CLUSTERED ([DDM_Config_ID] ASC)
);
GO

-- =============================================
-- Table: DDM_Config_Menu_Layout
-- Description: Menu structure and layout definitions for Dynamic Dashboard Manager
-- Defines the menu hierarchy, content types, and layout configurations
-- =============================================
CREATE TABLE [dbo].[DDM_Config_Menu_Layout] (
    [DDM_Menu_ID] INT NOT NULL IDENTITY(1,1),
    [DDM_Config_ID] INT NOT NULL,
    [DDM_Type] INT NULL,              -- 0=None, 1=WFProfile, 2=StandAlone
    [Scen_Type] INT NULL,             -- Scenario Type
    [Profile_Key] UNIQUEIDENTIFIER NULL,  -- Workflow Profile Key
    
    -- Menu Configuration
    [Name] NVARCHAR(255) NULL,        -- Menu option name
    [Sort_Order] INT NULL,            -- Display order of menu items
    [Layout_Type] INT NULL,           -- 0=None, 1=Dashboard, 2=CubeView, 3=Dashboard_TopBottom, 
                                      -- 4=Dashboard_LeftRight, 5=Dashboard_2Top1Bottom, 
                                      -- 6=Dashboard_1Top2Bottom, 7=Dashboard_2Left1Right,
                                      -- 8=Dashboard_1Left2Right, 9=Dashboard_2x2, 10=Custom_Dashboard
    
    -- Workspace/Maintenance Unit/Dashboard References
    [Workspace_ID] NVARCHAR(255) NULL,
    [MaintUnit_ID] NVARCHAR(255) NULL,
    [Dashboard_ID] NVARCHAR(255) NULL,
    
    -- Layout Configuration Properties (stored as text for flexibility)
    -- These columns store layout-specific configuration values
    -- Examples: DB_Name, CV_Name, Top_Height, Left_Width, etc.
    [DB_Name] NVARCHAR(255) NULL,
    [CV_Name] NVARCHAR(255) NULL,
    [Top_Height] NVARCHAR(50) NULL,
    [Left_Width] NVARCHAR(50) NULL,
    [Top_Content_Type] NVARCHAR(50) NULL,
    [Bottom_Content_Type] NVARCHAR(50) NULL,
    [Left_Content_Type] NVARCHAR(50) NULL,
    [Right_Content_Type] NVARCHAR(50) NULL,
    [DB_Name_Top] NVARCHAR(255) NULL,
    [DB_Name_Bottom] NVARCHAR(255) NULL,
    [DB_Name_Left] NVARCHAR(255) NULL,
    [DB_Name_Right] NVARCHAR(255) NULL,
    [CV_Name_Top] NVARCHAR(255) NULL,
    [CV_Name_Bottom] NVARCHAR(255) NULL,
    [CV_Name_Left] NVARCHAR(255) NULL,
    [CV_Name_Right] NVARCHAR(255) NULL,
    
    -- Status and Control
    [Status] NVARCHAR(50) NULL,       -- e.g., "In Process", "Active", "Inactive"
    
    -- Audit Fields
    [Create_Date] DATETIME NULL,
    [Create_User] NVARCHAR(50) NULL,
    [Update_Date] DATETIME NULL,
    [Update_User] NVARCHAR(50) NULL,
    
    CONSTRAINT [PK_DDM_Config_Menu_Layout] PRIMARY KEY CLUSTERED ([DDM_Menu_ID] ASC)
);
GO

-- =============================================
-- Table: DDM_Config_Hdr_Ctrls
-- Description: Header controls configuration for Dynamic Dashboard Manager
-- Defines controls (filters, buttons) displayed in dashboard headers
-- =============================================
CREATE TABLE [dbo].[DDM_Config_Hdr_Ctrls] (
    [DDM_Hdr_Ctrl_ID] INT NOT NULL IDENTITY(1,1),
    [DDM_Menu_ID] INT NOT NULL,
    [DDM_Config_ID] INT NOT NULL,
    [DDM_Type] INT NULL,              -- 0=None, 1=WFProfile, 2=StandAlone
    [Scen_Type] INT NULL,             -- Scenario Type
    [Profile_Key] UNIQUEIDENTIFIER NULL,  -- Workflow Profile Key
    
    -- Header Control Configuration
    [Name] NVARCHAR(255) NULL,        -- Control name
    [Sort_Order] INT NULL,            -- Display order of controls
    [Option_Type] INT NULL,           -- 0=None, 1=Filter, 2=Button
    
    -- Status and Control
    [Status] NVARCHAR(50) NULL,       -- e.g., "In Process", "Active", "Inactive"
    
    -- Audit Fields
    [Create_Date] DATETIME NULL,
    [Create_User] NVARCHAR(50) NULL,
    [Update_Date] DATETIME NULL,
    [Update_User] NVARCHAR(50) NULL,
    
    CONSTRAINT [PK_DDM_Config_Hdr_Ctrls] PRIMARY KEY CLUSTERED ([DDM_Hdr_Ctrl_ID] ASC)
);
GO

-- =============================================
-- Foreign Key Relationships
-- =============================================
ALTER TABLE [dbo].[DDM_Config_Menu_Layout]
ADD CONSTRAINT [FK_DDM_Config_Menu_Layout_DDM_Config]
FOREIGN KEY ([DDM_Config_ID])
REFERENCES [dbo].[DDM_Config] ([DDM_Config_ID])
ON DELETE CASCADE;
GO

ALTER TABLE [dbo].[DDM_Config_Hdr_Ctrls]
ADD CONSTRAINT [FK_DDM_Config_Hdr_Ctrls_DDM_Config]
FOREIGN KEY ([DDM_Config_ID])
REFERENCES [dbo].[DDM_Config] ([DDM_Config_ID])
ON DELETE NO ACTION;
GO

ALTER TABLE [dbo].[DDM_Config_Hdr_Ctrls]
ADD CONSTRAINT [FK_DDM_Config_Hdr_Ctrls_DDM_Config_Menu_Layout]
FOREIGN KEY ([DDM_Menu_ID])
REFERENCES [dbo].[DDM_Config_Menu_Layout] ([DDM_Menu_ID])
ON DELETE CASCADE;
GO

-- =============================================
-- Indexes for Performance
-- =============================================
CREATE NONCLUSTERED INDEX [IX_DDM_Config_ProfileKey]
ON [dbo].[DDM_Config] ([Profile_Key]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_DDMType]
ON [dbo].[DDM_Config] ([DDM_Type]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Menu_Layout_ConfigID]
ON [dbo].[DDM_Config_Menu_Layout] ([DDM_Config_ID]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Menu_Layout_SortOrder]
ON [dbo].[DDM_Config_Menu_Layout] ([Sort_Order]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Menu_Layout_ProfileKey]
ON [dbo].[DDM_Config_Menu_Layout] ([Profile_Key]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Hdr_Ctrls_MenuID]
ON [dbo].[DDM_Config_Hdr_Ctrls] ([DDM_Menu_ID]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Hdr_Ctrls_ConfigID]
ON [dbo].[DDM_Config_Hdr_Ctrls] ([DDM_Config_ID]);
GO

CREATE NONCLUSTERED INDEX [IX_DDM_Config_Hdr_Ctrls_SortOrder]
ON [dbo].[DDM_Config_Hdr_Ctrls] ([Sort_Order]);
GO

-- =============================================
-- Comments and Documentation
-- =============================================

-- DDM_Config stores the main configuration for dashboards
-- It links to WorkflowProfileHierarchy via Profile_Key for WFProfile type configurations
-- The Profile_Key can be used to join with the OneStream WorkflowProfileHierarchy table

-- DDM_Config_Menu_Layout stores the menu structure and layout configurations
-- Each menu item is associated with a DDM_Config record
-- Layout_Type determines which dashboard layout to use and which layout-specific columns are relevant
-- The layout-specific columns (DB_Name, CV_Name, Top_Height, etc.) store configuration values

-- DDM_Config_Hdr_Ctrls stores the header controls (filters, buttons) for each menu
-- Each control is associated with both a menu (DDM_Menu_ID) and configuration (DDM_Config_ID)
-- Option_Type determines the type of control (Filter or Button)
