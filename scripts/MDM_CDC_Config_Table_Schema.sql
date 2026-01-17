-- =============================================
-- Table: MDM_CDC_Config
-- Description: Configuration table for Change Data Capture (CDC) in Model Dimension Manager
-- This table stores the configuration for importing dimension members from various sources
-- 
-- Dependencies: This table requires the following OneStream core tables to exist:
--   - Dim: Standard OneStream dimension table
-- These tables are part of the OneStream core database schema and should exist in all environments.
-- If the foreign key constraints fail, verify these tables exist before creating this table.
-- =============================================

CREATE TABLE [dbo].[MDM_CDC_Config](
	[CDC_Config_ID] [int] NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Dim_Type] [nvarchar](50) NOT NULL,
	[Dim_ID] [int] NOT NULL,
	[Src_Connection] [nvarchar](200) NULL,
	[Src_SQL_String] [nvarchar](max) NULL,
	[Dim_Mgmt_Process] [nvarchar](max) NULL,
	[Trx_Rule] [nvarchar](max) NULL,
	[Appr_ID] [int] NULL,
	[Mbr_PrefSuff] [nvarchar](20) NULL,
	[Mbr_PrefSuff_Txt] [nvarchar](max) NULL,
	[Create_Date] [datetime] NOT NULL,
	[Create_User] [nvarchar](50) NOT NULL,
	[Update_Date] [datetime] NOT NULL,
	[Update_User] [nvarchar](50) NOT NULL,
    CONSTRAINT PK_MDM_CDC_Config PRIMARY KEY CLUSTERED ([CDC_Config_ID] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

-- Index for faster lookups
CREATE NONCLUSTERED INDEX IX_MDM_CDC_Config_Dim_Type 
ON MDM_CDC_Config (Dim_Type);
GO

CREATE NONCLUSTERED INDEX IX_MDM_CDC_Config_Dim_ID 
ON MDM_CDC_Config (Dim_ID);
GO

-- =============================================
-- Table: MDM_CDC_Config_Detail
-- Description: Detail table for CDC column mappings
-- Stores mapping between source columns and OneStream member properties
-- Supports varying by scenario and time
-- =============================================

CREATE TABLE [dbo].[MDM_CDC_Config_Detail](
	[CDC_Config_ID] [int] NOT NULL,
	[CDC_Config_Detail_ID] [int] NOT NULL,
	[OS_Mbr_Column] [nvarchar](100) NULL,
	[OS_Mbr_Vary_Scen_Column] [nvarchar](100) NULL,
	[OS_Mbr_Vary_Time_Column] [nvarchar](100) NULL,
	[Src_Mbr_Column] [nvarchar](100) NULL,
	[Src_Vary_Scen_Column] [nvarchar](100) NULL,
	[Src_Vary_Time_Column] [nvarchar](100) NULL,
	[Create_Date] [datetime] NOT NULL,
	[Create_User] [nvarchar](50) NOT NULL,
	[Update_Date] [datetime] NOT NULL,
	[Update_User] [nvarchar](50) NOT NULL,
    CONSTRAINT PK_MDM_CDC_Config_Detail PRIMARY KEY CLUSTERED ([CDC_Config_ID] ASC, [CDC_Config_Detail_ID] ASC),
    CONSTRAINT FK_MDM_CDC_Config_Detail_Config FOREIGN KEY ([CDC_Config_ID]) REFERENCES [dbo].[MDM_CDC_Config]([CDC_Config_ID])
) ON [PRIMARY]
GO

-- Index for faster lookups on detail table
CREATE NONCLUSTERED INDEX IX_MDM_CDC_Config_Detail_Config_ID 
ON MDM_CDC_Config_Detail (CDC_Config_ID);
GO

-- =============================================
-- Example Usage:
-- =============================================
-- Insert a CDC configuration
-- INSERT INTO MDM_CDC_Config (CDC_Config_ID, Name, Dim_Type, Dim_ID, Src_Connection, Src_SQL_String, Dim_Mgmt_Process, Trx_Rule, Appr_ID, Create_Date, Create_User, Update_Date, Update_User)
-- VALUES (1, 'Account CDC Config', 'Account', 1, 'MyConnectionName', 'SELECT * FROM Source_Accounts', NULL, NULL, NULL, GETDATE(), 'admin', GETDATE(), 'admin');

-- Insert detail mapping
-- INSERT INTO MDM_CDC_Config_Detail (CDC_Config_ID, CDC_Config_Detail_ID, OS_Mbr_Column, Src_Mbr_Column, Create_Date, Create_User, Update_Date, Update_User)
-- VALUES (1, 1, 'Name', 'Account_Name', GETDATE(), 'admin', GETDATE(), 'admin');
