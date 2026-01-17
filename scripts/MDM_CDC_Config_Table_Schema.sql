-- =============================================
-- Table: MDM_CDC_Config
-- Description: Configuration table for Change Data Capture (CDC) in Model Dimension Manager
-- This table stores the configuration for importing dimension members from various sources
-- 
-- Dependencies: This table requires the following OneStream core tables to exist:
--   - DimType: Standard OneStream dimension type table
--   - Dim: Standard OneStream dimension table
-- These tables are part of the OneStream core database schema and should exist in all environments.
-- If the foreign key constraints fail, verify these tables exist before creating this table.
-- =============================================

CREATE TABLE MDM_CDC_Config (
    -- Primary Key
    CDC_Config_ID INT IDENTITY(1,1) PRIMARY KEY,
    
    -- Dimension Configuration
    DimTypeID INT NOT NULL,
    DimID INT NOT NULL,
    
    -- Source Configuration
    SourceType NVARCHAR(50) NOT NULL,  -- Values: 'SQL', 'API', 'Flat File'
    SourceConfig NVARCHAR(MAX) NULL,    -- JSON or XML configuration for the source (connection string, API endpoint, file path, etc.)
                                        -- SECURITY NOTE: This field may contain sensitive data (passwords, API keys).
                                        -- Consider implementing column-level encryption using Always Encrypted or application-level encryption.
                                        -- See README_CDC_Config.md for detailed security guidance.
    
    -- Column Mapping Configuration
    -- Maps source data columns to dimension member properties
    Map_Name NVARCHAR(255) NULL,        -- Source column name for Member Name
    Map_Description NVARCHAR(255) NULL, -- Source column name for Member Description
    Map_Text1 NVARCHAR(255) NULL,       -- Source column name for Text1 property
    Map_Text2 NVARCHAR(255) NULL,       -- Source column name for Text2 property
    Map_Text3 NVARCHAR(255) NULL,       -- Source column name for Text3 property
    Map_Text4 NVARCHAR(255) NULL,       -- Source column name for Text4 property
    Map_Text5 NVARCHAR(255) NULL,       -- Source column name for Text5 property
    Map_Text6 NVARCHAR(255) NULL,       -- Source column name for Text6 property
    Map_Text7 NVARCHAR(255) NULL,       -- Source column name for Text7 property
    Map_Text8 NVARCHAR(255) NULL,       -- Source column name for Text8 property
    
    -- Audit Fields
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(255) NOT NULL,
    Modify_Date DATETIME NULL,
    Modify_User NVARCHAR(255) NULL,
    
    -- Constraints
    -- Note: If these foreign keys fail, verify DimType and Dim tables exist in your environment
    CONSTRAINT FK_MDM_CDC_Config_DimType FOREIGN KEY (DimTypeID) REFERENCES DimType(DimTypeID),
    CONSTRAINT FK_MDM_CDC_Config_Dim FOREIGN KEY (DimID) REFERENCES Dim(DimID),
    CONSTRAINT UQ_MDM_CDC_Config_Dim UNIQUE (DimTypeID, DimID)
);

-- Index for faster lookups
CREATE NONCLUSTERED INDEX IX_MDM_CDC_Config_DimType 
ON MDM_CDC_Config (DimTypeID);

CREATE NONCLUSTERED INDEX IX_MDM_CDC_Config_Dim 
ON MDM_CDC_Config (DimID);

-- =============================================
-- Example Usage:
-- =============================================
-- Insert a SQL-based CDC configuration
-- INSERT INTO MDM_CDC_Config (DimTypeID, DimID, SourceType, SourceConfig, Map_Name, Map_Description, Map_Text1, Create_User)
-- VALUES (1, 1, 'SQL', 'SELECT Code, Name, Description, Category FROM SourceTable', 'Code', 'Description', 'Category', 'admin');

-- Insert an API-based CDC configuration
-- INSERT INTO MDM_CDC_Config (DimTypeID, DimID, SourceType, SourceConfig, Map_Name, Map_Description, Create_User)
-- VALUES (2, 2, 'API', '{"endpoint": "https://api.example.com/dimensions", "method": "GET"}', 'id', 'name', 'admin');

-- Insert a Flat File-based CDC configuration
-- INSERT INTO MDM_CDC_Config (DimTypeID, DimID, SourceType, SourceConfig, Map_Name, Map_Description, Map_Text1, Map_Text2, Create_User)
-- VALUES (3, 3, 'Flat File', '{"filePath": "/data/dimensions.csv", "delimiter": ","}', 'MemberCode', 'MemberName', 'Category', 'Type', 'admin');
