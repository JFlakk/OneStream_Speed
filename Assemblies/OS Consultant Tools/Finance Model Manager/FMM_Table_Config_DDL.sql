-- =============================================
-- FMM Custom Table Configuration System
-- Database Schema Definition
-- =============================================

-- Drop tables in reverse dependency order if they exist
IF OBJECT_ID('dbo.FMM_Table_FK_Column_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_FK_Column_Config;
IF OBJECT_ID('dbo.FMM_Table_FK_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_FK_Config;
IF OBJECT_ID('dbo.FMM_Table_Index_Column_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_Index_Column_Config;
IF OBJECT_ID('dbo.FMM_Table_Index_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_Index_Config;
IF OBJECT_ID('dbo.FMM_Table_Audit_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_Audit_Config;
IF OBJECT_ID('dbo.FMM_Table_Column_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_Column_Config;
IF OBJECT_ID('dbo.FMM_Table_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Table_Config;
GO

-- =============================================
-- 1. FMM_Table_Config - Core Table Configuration
-- =============================================
CREATE TABLE dbo.FMM_Table_Config (
    Table_Config_ID INT IDENTITY(1,1) NOT NULL,
    Process_Type NVARCHAR(100) NOT NULL,
    Table_Name NVARCHAR(255) NOT NULL,
    Table_Type NVARCHAR(50) NOT NULL,                           -- 'Master', 'Detail', 'Audit', 'Extension'
    Parent_Table_Config_ID INT NULL,
    Description NVARCHAR(500) NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Enable_Audit BIT NOT NULL DEFAULT 1,
    Audit_Table_Config_ID INT NULL,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_Config PRIMARY KEY CLUSTERED (Table_Config_ID),
    CONSTRAINT FK_Table_Parent FOREIGN KEY (Parent_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_Table_Audit FOREIGN KEY (Audit_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_Table_Name UNIQUE (Table_Name),
    CONSTRAINT CHK_Table_Type CHECK (Table_Type IN ('Master', 'Detail', 'Audit', 'Extension'))
);
GO

-- Create index on Process_Type for common queries
CREATE NONCLUSTERED INDEX IX_FMM_Table_Config_Process 
ON dbo.FMM_Table_Config (Process_Type, Is_Active) 
INCLUDE (Table_Name, Table_Type);
GO

-- =============================================
-- 2. FMM_Table_Column_Config - Column Definitions
-- =============================================
CREATE TABLE dbo.FMM_Table_Column_Config (
    Column_Config_ID INT IDENTITY(1,1) NOT NULL,
    Table_Config_ID INT NOT NULL,
    Column_Name NVARCHAR(255) NOT NULL,
    Data_Type NVARCHAR(50) NOT NULL,
    Max_Length INT NULL,                                        -- For VARCHAR/NVARCHAR types
    Precision INT NULL,                                         -- For DECIMAL/NUMERIC types
    Scale INT NULL,                                             -- For DECIMAL/NUMERIC types
    Is_Nullable BIT NOT NULL DEFAULT 1,
    Default_Value NVARCHAR(500) NULL,
    Is_Identity BIT NOT NULL DEFAULT 0,
    Identity_Seed INT NULL,
    Identity_Increment INT NULL,
    Column_Order INT NOT NULL,
    Description NVARCHAR(500) NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_Column_Config PRIMARY KEY CLUSTERED (Column_Config_ID),
    CONSTRAINT FK_Column_Table FOREIGN KEY (Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID) ON DELETE CASCADE,
    CONSTRAINT UQ_Column_Name UNIQUE (Table_Config_ID, Column_Name),
    CONSTRAINT CHK_Data_Type CHECK (Data_Type IN (
        'INT', 'BIGINT', 'SMALLINT', 'TINYINT',
        'DECIMAL', 'NUMERIC', 'MONEY', 'SMALLMONEY',
        'FLOAT', 'REAL',
        'BIT',
        'CHAR', 'VARCHAR', 'NCHAR', 'NVARCHAR', 'TEXT', 'NTEXT',
        'DATETIME', 'DATETIME2', 'DATE', 'TIME', 'SMALLDATETIME',
        'UNIQUEIDENTIFIER',
        'BINARY', 'VARBINARY', 'IMAGE',
        'XML'
    )),
    CONSTRAINT CHK_Identity_Seed CHECK (Is_Identity = 0 OR (Identity_Seed IS NOT NULL AND Identity_Increment IS NOT NULL))
);
GO

-- Create index on Table_Config_ID for efficient column lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_Column_Config_Table 
ON dbo.FMM_Table_Column_Config (Table_Config_ID, Column_Order) 
INCLUDE (Column_Name, Data_Type, Is_Nullable);
GO

-- =============================================
-- 3. FMM_Table_Index_Config - Index Definitions
-- =============================================
CREATE TABLE dbo.FMM_Table_Index_Config (
    Index_Config_ID INT IDENTITY(1,1) NOT NULL,
    Table_Config_ID INT NOT NULL,
    Index_Name NVARCHAR(255) NOT NULL,
    Index_Type NVARCHAR(50) NOT NULL,                           -- 'PRIMARY_KEY', 'CLUSTERED', 'NONCLUSTERED', 'UNIQUE'
    Is_Clustered BIT NOT NULL DEFAULT 0,
    Is_Unique BIT NOT NULL DEFAULT 0,
    Fill_Factor INT NULL,                                       -- Index fill factor (0-100)
    Description NVARCHAR(500) NULL,
    Is_Active BIT NOT NULL DEFAULT 1,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_Index_Config PRIMARY KEY CLUSTERED (Index_Config_ID),
    CONSTRAINT FK_Index_Table FOREIGN KEY (Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID) ON DELETE CASCADE,
    CONSTRAINT UQ_Index_Name UNIQUE (Table_Config_ID, Index_Name),
    CONSTRAINT CHK_Index_Type CHECK (Index_Type IN ('PRIMARY_KEY', 'CLUSTERED', 'NONCLUSTERED', 'UNIQUE')),
    CONSTRAINT CHK_Fill_Factor CHECK (Fill_Factor IS NULL OR (Fill_Factor >= 0 AND Fill_Factor <= 100)),
    CONSTRAINT CHK_Clustered_Valid CHECK (
        -- Primary key can be clustered or non-clustered
        -- Only one clustered index per table enforced at application level
        Is_Clustered = 0 OR Index_Type IN ('PRIMARY_KEY', 'CLUSTERED')
    )
);
GO

-- Create index for table lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_Index_Config_Table 
ON dbo.FMM_Table_Index_Config (Table_Config_ID, Is_Active);
GO

-- =============================================
-- 4. FMM_Table_Index_Column_Config - Index Column Mappings
-- =============================================
CREATE TABLE dbo.FMM_Table_Index_Column_Config (
    Index_Column_Config_ID INT IDENTITY(1,1) NOT NULL,
    Index_Config_ID INT NOT NULL,
    Column_Config_ID INT NOT NULL,
    Key_Ordinal INT NOT NULL,                                   -- Order of column in index (1, 2, 3, ...)
    Sort_Direction NVARCHAR(10) NOT NULL DEFAULT 'ASC',         -- 'ASC' or 'DESC'
    Is_Included_Column BIT NOT NULL DEFAULT 0,                  -- For included columns in covering indexes
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_Index_Column_Config PRIMARY KEY CLUSTERED (Index_Column_Config_ID),
    CONSTRAINT FK_IndexCol_Index FOREIGN KEY (Index_Config_ID) 
        REFERENCES dbo.FMM_Table_Index_Config(Index_Config_ID) ON DELETE CASCADE,
    CONSTRAINT FK_IndexCol_Column FOREIGN KEY (Column_Config_ID) 
        REFERENCES dbo.FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT UQ_IndexCol_Ordinal UNIQUE (Index_Config_ID, Key_Ordinal),
    CONSTRAINT CHK_Sort_Direction CHECK (Sort_Direction IN ('ASC', 'DESC'))
);
GO

-- Create index for efficient index column lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_Index_Column_Config_Index 
ON dbo.FMM_Table_Index_Column_Config (Index_Config_ID, Key_Ordinal);
GO

-- =============================================
-- 5. FMM_Table_FK_Config - Foreign Key Constraint Definitions
-- =============================================
CREATE TABLE dbo.FMM_Table_FK_Config (
    FK_Config_ID INT IDENTITY(1,1) NOT NULL,
    FK_Name NVARCHAR(255) NOT NULL,
    Source_Table_Config_ID INT NOT NULL,                        -- Child table
    Target_Table_Config_ID INT NOT NULL,                        -- Parent table
    On_Delete_Action NVARCHAR(50) NOT NULL DEFAULT 'NO ACTION', -- 'CASCADE', 'SET NULL', 'SET DEFAULT', 'NO ACTION'
    On_Update_Action NVARCHAR(50) NOT NULL DEFAULT 'NO ACTION',
    Is_Active BIT NOT NULL DEFAULT 1,
    Description NVARCHAR(500) NULL,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_FK_Config PRIMARY KEY CLUSTERED (FK_Config_ID),
    CONSTRAINT FK_Source_Table FOREIGN KEY (Source_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_Target_Table FOREIGN KEY (Target_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_FK_Name UNIQUE (FK_Name),
    CONSTRAINT CHK_Delete_Action CHECK (On_Delete_Action IN ('CASCADE', 'SET NULL', 'SET DEFAULT', 'NO ACTION')),
    CONSTRAINT CHK_Update_Action CHECK (On_Update_Action IN ('CASCADE', 'SET NULL', 'SET DEFAULT', 'NO ACTION')),
    CONSTRAINT CHK_Different_Tables CHECK (Source_Table_Config_ID != Target_Table_Config_ID)
);
GO

-- Create indexes for FK lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_FK_Config_Source 
ON dbo.FMM_Table_FK_Config (Source_Table_Config_ID, Is_Active);

CREATE NONCLUSTERED INDEX IX_FMM_Table_FK_Config_Target 
ON dbo.FMM_Table_FK_Config (Target_Table_Config_ID, Is_Active);
GO

-- =============================================
-- 6. FMM_Table_FK_Column_Config - Foreign Key Column Mappings
-- =============================================
CREATE TABLE dbo.FMM_Table_FK_Column_Config (
    FK_Column_Config_ID INT IDENTITY(1,1) NOT NULL,
    FK_Config_ID INT NOT NULL,
    Source_Column_Config_ID INT NOT NULL,                       -- Column in child table
    Target_Column_Config_ID INT NOT NULL,                       -- Column in parent table
    Column_Ordinal INT NOT NULL,                                -- Order for composite foreign keys
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_FK_Column_Config PRIMARY KEY CLUSTERED (FK_Column_Config_ID),
    CONSTRAINT FK_FKCol_FK FOREIGN KEY (FK_Config_ID) 
        REFERENCES dbo.FMM_Table_FK_Config(FK_Config_ID) ON DELETE CASCADE,
    CONSTRAINT FK_FKCol_Source FOREIGN KEY (Source_Column_Config_ID) 
        REFERENCES dbo.FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT FK_FKCol_Target FOREIGN KEY (Target_Column_Config_ID) 
        REFERENCES dbo.FMM_Table_Column_Config(Column_Config_ID),
    CONSTRAINT UQ_FKCol_Ordinal UNIQUE (FK_Config_ID, Column_Ordinal)
);
GO

-- Create index for FK column lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_FK_Column_Config_FK 
ON dbo.FMM_Table_FK_Column_Config (FK_Config_ID, Column_Ordinal);
GO

-- =============================================
-- 7. FMM_Table_Audit_Config - Audit Configuration
-- =============================================
CREATE TABLE dbo.FMM_Table_Audit_Config (
    Audit_Config_ID INT IDENTITY(1,1) NOT NULL,
    Source_Table_Config_ID INT NOT NULL,
    Audit_Table_Config_ID INT NOT NULL,
    Track_Inserts BIT NOT NULL DEFAULT 1,
    Track_Updates BIT NOT NULL DEFAULT 1,
    Track_Deletes BIT NOT NULL DEFAULT 1,
    Audit_User_Column NVARCHAR(255) NOT NULL DEFAULT 'Audit_User',
    Audit_Date_Column NVARCHAR(255) NOT NULL DEFAULT 'Audit_Date',
    Audit_Action_Column NVARCHAR(255) NOT NULL DEFAULT 'Audit_Action',
    Is_Active BIT NOT NULL DEFAULT 1,
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Table_Audit_Config PRIMARY KEY CLUSTERED (Audit_Config_ID),
    CONSTRAINT FK_AuditCfg_Source FOREIGN KEY (Source_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT FK_AuditCfg_Audit FOREIGN KEY (Audit_Table_Config_ID) 
        REFERENCES dbo.FMM_Table_Config(Table_Config_ID),
    CONSTRAINT UQ_AuditCfg_Source UNIQUE (Source_Table_Config_ID)
);
GO

-- Create index for audit config lookups
CREATE NONCLUSTERED INDEX IX_FMM_Table_Audit_Config_Source 
ON dbo.FMM_Table_Audit_Config (Source_Table_Config_ID, Is_Active);
GO

-- =============================================
-- Create views for easier querying
-- =============================================

-- View to see complete table structure with columns
CREATE VIEW dbo.vw_FMM_Table_Structure AS
SELECT 
    tc.Table_Config_ID,
    tc.Process_Type,
    tc.Table_Name,
    tc.Table_Type,
    tc.Description AS Table_Description,
    tc.Is_Active AS Table_Is_Active,
    cc.Column_Config_ID,
    cc.Column_Name,
    cc.Data_Type,
    cc.Max_Length,
    cc.Precision,
    cc.Scale,
    cc.Is_Nullable,
    cc.Default_Value,
    cc.Is_Identity,
    cc.Column_Order,
    cc.Description AS Column_Description,
    cc.Is_Active AS Column_Is_Active
FROM dbo.FMM_Table_Config tc
INNER JOIN dbo.FMM_Table_Column_Config cc ON tc.Table_Config_ID = cc.Table_Config_ID
WHERE tc.Is_Active = 1 AND cc.Is_Active = 1;
GO

-- View to see all indexes with their columns
CREATE VIEW dbo.vw_FMM_Table_Indexes AS
SELECT 
    tc.Table_Name,
    ic.Index_Name,
    ic.Index_Type,
    ic.Is_Clustered,
    ic.Is_Unique,
    icc.Key_Ordinal,
    cc.Column_Name,
    icc.Sort_Direction,
    icc.Is_Included_Column
FROM dbo.FMM_Table_Config tc
INNER JOIN dbo.FMM_Table_Index_Config ic ON tc.Table_Config_ID = ic.Table_Config_ID
INNER JOIN dbo.FMM_Table_Index_Column_Config icc ON ic.Index_Config_ID = icc.Index_Config_ID
INNER JOIN dbo.FMM_Table_Column_Config cc ON icc.Column_Config_ID = cc.Column_Config_ID
WHERE tc.Is_Active = 1 AND ic.Is_Active = 1 AND cc.Is_Active = 1;
GO

-- View to see all foreign keys with their columns
CREATE VIEW dbo.vw_FMM_Table_ForeignKeys AS
SELECT 
    fk.FK_Name,
    st.Table_Name AS Source_Table,
    sc.Column_Name AS Source_Column,
    tt.Table_Name AS Target_Table,
    tc.Column_Name AS Target_Column,
    fk.On_Delete_Action,
    fk.On_Update_Action,
    fkc.Column_Ordinal
FROM dbo.FMM_Table_FK_Config fk
INNER JOIN dbo.FMM_Table_Config st ON fk.Source_Table_Config_ID = st.Table_Config_ID
INNER JOIN dbo.FMM_Table_Config tt ON fk.Target_Table_Config_ID = tt.Table_Config_ID
INNER JOIN dbo.FMM_Table_FK_Column_Config fkc ON fk.FK_Config_ID = fkc.FK_Config_ID
INNER JOIN dbo.FMM_Table_Column_Config sc ON fkc.Source_Column_Config_ID = sc.Column_Config_ID
INNER JOIN dbo.FMM_Table_Column_Config tc ON fkc.Target_Column_Config_ID = tc.Column_Config_ID
WHERE fk.Is_Active = 1;
GO

-- =============================================
-- Grant permissions (adjust as needed for your security model)
-- =============================================
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_Column_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_Index_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_Index_Column_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_FK_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_FK_Column_Config TO [YourRole];
-- GRANT SELECT, INSERT, UPDATE, DELETE ON dbo.FMM_Table_Audit_Config TO [YourRole];
-- GRANT SELECT ON dbo.vw_FMM_Table_Structure TO [YourRole];
-- GRANT SELECT ON dbo.vw_FMM_Table_Indexes TO [YourRole];
-- GRANT SELECT ON dbo.vw_FMM_Table_ForeignKeys TO [YourRole];

PRINT 'FMM Custom Table Configuration System schema created successfully.';
GO
