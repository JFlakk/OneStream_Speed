/*
 * SQL Script to extend MDM_CDC_Config table for file upload and enhanced column mapping support
 * Execute this script in the OneStream application database
 */

-- Add new columns to MDM_CDC_Config table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'Src_Type')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD Src_Type NVARCHAR(50) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'File_Path')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD File_Path NVARCHAR(500) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'Column_Mappings')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD Column_Mappings NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'Text_Property_Mappings')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD Text_Property_Mappings NVARCHAR(MAX) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'ScenarioType_Field')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD ScenarioType_Field NVARCHAR(100) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('MDM_CDC_Config') AND name = 'Time_Field')
BEGIN
    ALTER TABLE MDM_CDC_Config
    ADD Time_Field NVARCHAR(100) NULL;
END
GO

-- Create CDC Log table for tracking operations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'MDM_CDC_Log')
BEGIN
    CREATE TABLE MDM_CDC_Log (
        Log_ID INT IDENTITY(1,1) PRIMARY KEY,
        CDC_Config_ID INT NOT NULL,
        MemberName NVARCHAR(255),
        Operation NVARCHAR(50),
        Processed_Date DATETIME,
        Status NVARCHAR(50),
        Error_Message NVARCHAR(MAX),
        CONSTRAINT FK_MDM_CDC_Log_Config FOREIGN KEY (CDC_Config_ID) 
            REFERENCES MDM_CDC_Config(CDC_Config_ID)
    );
END
GO

-- Create index for performance optimization
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_MDM_CDC_Log_ConfigID_Date')
BEGIN
    CREATE NONCLUSTERED INDEX IX_MDM_CDC_Log_ConfigID_Date
    ON MDM_CDC_Log (CDC_Config_ID, Processed_Date DESC);
END
GO

-- Add default values for existing records
UPDATE MDM_CDC_Config
SET Src_Type = 'External'
WHERE Src_Type IS NULL AND Src_Connection IS NOT NULL;
GO

-- Add column descriptions
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Source type: External (database) or File (CSV upload)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'Src_Type';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'File path for uploaded CSV files (OneStream file system path)', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'File_Path';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'JSON mapping of source columns to dimension member fields: {"MemberName":"Col1","MemberDescription":"Col2","Parent":"Col3"}', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'Column_Mappings';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'JSON mapping of source columns to text properties: {"Text1":"Col4","Text2":"Col5","Text3":"Col6"}', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'Text_Property_Mappings';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Source column name containing scenario type for text property filtering', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'ScenarioType_Field';
GO

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Source column name containing time period for text property filtering', 
    @level0type = N'SCHEMA', @level0name = N'dbo',
    @level1type = N'TABLE', @level1name = N'MDM_CDC_Config',
    @level2type = N'COLUMN', @level2name = N'Time_Field';
GO

PRINT 'MDM_CDC_Config table schema successfully extended for file upload and enhanced mapping support.';
GO
