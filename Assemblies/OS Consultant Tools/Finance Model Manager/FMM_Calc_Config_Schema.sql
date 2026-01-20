-- =============================================
-- FMM Calculation Configuration System
-- Database Schema Definition
-- =============================================
-- This schema defines the calculation engine tables for Finance Model Manager
-- supporting Cube calcs, Table calcs, Table to Cube, Cube to Table, and Consolidations
-- =============================================

-- Drop tables in reverse dependency order if they exist
IF OBJECT_ID('dbo.FMM_Src_Cell', 'U') IS NOT NULL DROP TABLE dbo.FMM_Src_Cell;
IF OBJECT_ID('dbo.FMM_Dest_Cell', 'U') IS NOT NULL DROP TABLE dbo.FMM_Dest_Cell;
IF OBJECT_ID('dbo.FMM_Calc_Config', 'U') IS NOT NULL DROP TABLE dbo.FMM_Calc_Config;
GO

-- =============================================
-- 1. FMM_Calc_Config - Core Calculation Configuration
-- =============================================
-- Stores calculation metadata and configuration for all calculation types
-- Supports: Cube, Table, BRTabletoCube, ImportTabletoCube, CubetoTable, Consolidate
-- =============================================

CREATE TABLE dbo.FMM_Calc_Config (
    Calc_ID INT IDENTITY(1,1) NOT NULL,
    Cube_ID INT NOT NULL,
    Act_ID INT NOT NULL,
    Model_ID INT NOT NULL,
    
    -- Calculation Metadata
    Name NVARCHAR(255) NOT NULL,
    Sequence INT NULL,
    Calc_Type INT NOT NULL,                                     -- 1=Table, 2=Cube, 3=BRTabletoCube, 4=ImportTabletoCube, 5=CubetoTable, 6=Consolidate
    Calc_Condition NVARCHAR(MAX) NULL,                          -- Condition/filter for when calc runs
    Calc_Explanation NVARCHAR(MAX) NULL,                        -- Description/documentation
    
    -- Calculation Logic Fields (used by different calc types)
    Balanced_Buffer NVARCHAR(MAX) NULL,                         -- Buffer for balanced calculations
    bal_buffer_calc NVARCHAR(MAX) NULL,                         -- Balanced buffer calculation logic
    Unbal_Calc NVARCHAR(MAX) NULL,                              -- Unbalanced calculation logic
    Table_Calc_SQL_Logic NVARCHAR(MAX) NULL,                    -- SQL logic for table calculations
    
    -- Table Calculation Specific Fields
    MbrList_Calc BIT NULL DEFAULT 0,                            -- Enable member list calculations
    MbrList_1_Calc BIT NULL DEFAULT 0,                          -- Enable member list 1
    MbrList_1_DimType NVARCHAR(50) NULL,                        -- Dimension type for member list 1
    MbrList_1_Dim NVARCHAR(100) NULL,                           -- Dimension for member list 1
    MbrList_1_Filter NVARCHAR(MAX) NULL,                        -- Filter for member list 1
    MbrList_2_Calc BIT NULL DEFAULT 0,                          -- Enable member list 2
    MbrList_2_DimType NVARCHAR(50) NULL,                        -- Dimension type for member list 2
    MbrList_2_Dim NVARCHAR(100) NULL,                           -- Dimension for member list 2
    MbrList_2_Filter NVARCHAR(MAX) NULL,                        -- Filter for member list 2
    MbrList_3_Calc BIT NULL DEFAULT 0,                          -- Enable member list 3
    MbrList_3_DimType NVARCHAR(50) NULL,                        -- Dimension type for member list 3
    MbrList_3_Dim NVARCHAR(100) NULL,                           -- Dimension for member list 3
    MbrList_3_Filter NVARCHAR(MAX) NULL,                        -- Filter for member list 3
    MbrList_4_Calc BIT NULL DEFAULT 0,                          -- Enable member list 4
    MbrList_4_DimType NVARCHAR(50) NULL,                        -- Dimension type for member list 4
    MbrList_4_Dim NVARCHAR(100) NULL,                           -- Dimension for member list 4
    MbrList_4_Filter NVARCHAR(MAX) NULL,                        -- Filter for member list 4
    
    -- Time Management
    Time_Phasing NVARCHAR(50) NULL,                             -- Time phasing method
    Input_Frequency NVARCHAR(50) NULL,                          -- Input frequency (Monthly, Quarterly, etc.)
    
    -- Advanced Options
    MultiDim_Alloc BIT NULL DEFAULT 0,                          -- Enable multi-dimensional allocation
    BR_Calc BIT NULL DEFAULT 0,                                 -- Enable Business Rule calculation
    BR_Calc_Name NVARCHAR(255) NULL,                            -- Business Rule name
    
    -- Status and Audit
    Status NVARCHAR(50) NOT NULL DEFAULT 'Build',               -- Build, Active, Inactive, Archived
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Calc_Config PRIMARY KEY CLUSTERED (Calc_ID),
    CONSTRAINT CHK_Calc_Type CHECK (Calc_Type IN (1, 2, 3, 4, 5, 6)),
    CONSTRAINT CHK_Status CHECK (Status IN ('Build', 'Active', 'Inactive', 'Archived'))
);
GO

-- Create indexes for common query patterns
CREATE NONCLUSTERED INDEX IX_FMM_Calc_Config_Cube_Act_Model 
ON dbo.FMM_Calc_Config (Cube_ID, Act_ID, Model_ID, Sequence) 
INCLUDE (Calc_ID, Name, Calc_Type, Status);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Calc_Config_Type 
ON dbo.FMM_Calc_Config (Calc_Type, Status) 
INCLUDE (Calc_ID, Name, Cube_ID, Act_ID, Model_ID);
GO

-- =============================================
-- 2. FMM_Dest_Cell - Destination Cell Configuration
-- =============================================
-- Stores destination cell configuration for calculations
-- Defines where calculation results are written (cube dimensions or table columns)
-- =============================================

CREATE TABLE dbo.FMM_Dest_Cell (
    Dest_Cell_ID INT IDENTITY(1,1) NOT NULL,
    Cube_ID INT NOT NULL,
    Act_ID INT NOT NULL,
    Model_ID INT NOT NULL,
    Calc_ID INT NOT NULL,
    
    -- Destination Location
    Location NVARCHAR(255) NULL,                                -- Location identifier
    Calc_Plan_Units NVARCHAR(MAX) NULL,                         -- Planning units for calculation
    
    -- Cube Dimensions (for Cube-based calculations)
    Cons NVARCHAR(500) NULL,                                    -- Consolidation dimension
    View NVARCHAR(500) NULL,                                    -- View dimension
    Acct NVARCHAR(500) NULL,                                    -- Account dimension
    Flow NVARCHAR(500) NULL,                                    -- Flow dimension
    IC NVARCHAR(500) NULL,                                      -- Intercompany dimension
    Origin NVARCHAR(500) NULL,                                  -- Origin dimension
    UD1 NVARCHAR(500) NULL,                                     -- User-defined dimension 1
    UD2 NVARCHAR(500) NULL,                                     -- User-defined dimension 2
    UD3 NVARCHAR(500) NULL,                                     -- User-defined dimension 3
    UD4 NVARCHAR(500) NULL,                                     -- User-defined dimension 4
    UD5 NVARCHAR(500) NULL,                                     -- User-defined dimension 5
    UD6 NVARCHAR(500) NULL,                                     -- User-defined dimension 6
    UD7 NVARCHAR(500) NULL,                                     -- User-defined dimension 7
    UD8 NVARCHAR(500) NULL,                                     -- User-defined dimension 8
    
    -- Dimension Filters
    Time_Filter NVARCHAR(MAX) NULL,                             -- Time dimension filter
    Acct_Filter NVARCHAR(MAX) NULL,                             -- Account filter
    Origin_Filter NVARCHAR(MAX) NULL,                           -- Origin filter
    IC_Filter NVARCHAR(MAX) NULL,                               -- IC filter
    Flow_Filter NVARCHAR(MAX) NULL,                             -- Flow filter
    UD1_Filter NVARCHAR(MAX) NULL,                              -- UD1 filter
    UD2_Filter NVARCHAR(MAX) NULL,                              -- UD2 filter
    UD3_Filter NVARCHAR(MAX) NULL,                              -- UD3 filter
    UD4_Filter NVARCHAR(MAX) NULL,                              -- UD4 filter
    UD5_Filter NVARCHAR(MAX) NULL,                              -- UD5 filter
    UD6_Filter NVARCHAR(MAX) NULL,                              -- UD6 filter
    UD7_Filter NVARCHAR(MAX) NULL,                              -- UD7 filter
    UD8_Filter NVARCHAR(MAX) NULL,                              -- UD8 filter
    
    -- Additional Filters and Logic
    Conditional_Filter NVARCHAR(MAX) NULL,                      -- Conditional filter expression
    Curr_Cube_Buffer_Filter NVARCHAR(MAX) NULL,                 -- Current cube buffer filter
    Buffer_Filter NVARCHAR(MAX) NULL,                           -- General buffer filter
    Dest_Cell_Logic NVARCHAR(MAX) NULL,                         -- Custom destination logic
    SQL_Logic NVARCHAR(MAX) NULL,                               -- SQL logic for table destinations
    
    -- Audit
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Dest_Cell PRIMARY KEY CLUSTERED (Dest_Cell_ID),
    CONSTRAINT FK_Dest_Cell_Calc FOREIGN KEY (Calc_ID) 
        REFERENCES dbo.FMM_Calc_Config(Calc_ID) ON DELETE CASCADE
);
GO

-- Create indexes for efficient lookups
CREATE NONCLUSTERED INDEX IX_FMM_Dest_Cell_Calc 
ON dbo.FMM_Dest_Cell (Calc_ID) 
INCLUDE (Dest_Cell_ID, Cube_ID, Act_ID, Model_ID);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Dest_Cell_Cube_Act_Model 
ON dbo.FMM_Dest_Cell (Cube_ID, Act_ID, Model_ID);
GO

-- =============================================
-- 3. FMM_Src_Cell - Source Cell Configuration
-- =============================================
-- Stores source cell configuration for calculations
-- Supports multiple source cells per calculation with dimensional mapping
-- Handles cube-to-cube, table-to-cube, cube-to-table, and consolidation sources
-- =============================================

CREATE TABLE dbo.FMM_Src_Cell (
    Cell_ID INT IDENTITY(1,1) NOT NULL,
    Cube_ID INT NOT NULL,
    Act_ID INT NOT NULL,
    Model_ID INT NOT NULL,
    Calc_ID INT NOT NULL,
    
    -- Source Cell Ordering and Type
    Src_Order INT NULL,                                         -- Order of source cell in calculation
    Src_Type NVARCHAR(50) NULL,                                 -- Type of source (Cube, Table, Constant, etc.)
    Src_Item NVARCHAR(255) NULL,                                -- Source item identifier
    
    -- Mathematical Expression
    Open_Parens NVARCHAR(50) NULL,                              -- Opening parentheses for grouping
    Math_Operator NVARCHAR(10) NULL,                            -- Math operator (+, -, *, /)
    Close_Parens NVARCHAR(50) NULL,                             -- Closing parentheses for grouping
    
    -- Source Cube Dimensions
    Entity NVARCHAR(500) NULL,                                  -- Entity dimension
    Cons NVARCHAR(500) NULL,                                    -- Consolidation dimension
    Scenario NVARCHAR(500) NULL,                                -- Scenario dimension
    Time NVARCHAR(500) NULL,                                    -- Time dimension
    View NVARCHAR(500) NULL,                                    -- View dimension
    Acct NVARCHAR(500) NULL,                                    -- Account dimension
    Flow NVARCHAR(500) NULL,                                    -- Flow dimension
    IC NVARCHAR(500) NULL,                                      -- Intercompany dimension
    Origin NVARCHAR(500) NULL,                                  -- Origin dimension
    UD1 NVARCHAR(500) NULL,                                     -- User-defined dimension 1
    UD2 NVARCHAR(500) NULL,                                     -- User-defined dimension 2
    UD3 NVARCHAR(500) NULL,                                     -- User-defined dimension 3
    UD4 NVARCHAR(500) NULL,                                     -- User-defined dimension 4
    UD5 NVARCHAR(500) NULL,                                     -- User-defined dimension 5
    UD6 NVARCHAR(500) NULL,                                     -- User-defined dimension 6
    UD7 NVARCHAR(500) NULL,                                     -- User-defined dimension 7
    UD8 NVARCHAR(500) NULL,                                     -- User-defined dimension 8
    
    -- Planning Units
    Src_Plan_Units NVARCHAR(MAX) NULL,                          -- Source planning units
    
    -- Unbalanced Calculation Overrides
    Unbal_Src_Cell_Buffer NVARCHAR(MAX) NULL,                   -- Unbalanced source cell buffer
    Unbal_Origin_Override NVARCHAR(500) NULL,                   -- Origin override for unbalanced calc
    Unbal_IC_Override NVARCHAR(500) NULL,                       -- IC override for unbalanced calc
    Unbal_Acct_Override NVARCHAR(500) NULL,                     -- Account override for unbalanced calc
    Unbal_Flow_Override NVARCHAR(500) NULL,                     -- Flow override for unbalanced calc
    Unbal_UD1_Override NVARCHAR(500) NULL,                      -- UD1 override for unbalanced calc
    Unbal_UD2_Override NVARCHAR(500) NULL,                      -- UD2 override for unbalanced calc
    Unbal_UD3_Override NVARCHAR(500) NULL,                      -- UD3 override for unbalanced calc
    Unbal_UD4_Override NVARCHAR(500) NULL,                      -- UD4 override for unbalanced calc
    Unbal_UD5_Override NVARCHAR(500) NULL,                      -- UD5 override for unbalanced calc
    Unbal_UD6_Override NVARCHAR(500) NULL,                      -- UD6 override for unbalanced calc
    Unbal_UD7_Override NVARCHAR(500) NULL,                      -- UD7 override for unbalanced calc
    Unbal_UD8_Override NVARCHAR(500) NULL,                      -- UD8 override for unbalanced calc
    Unbal_Src_Cell_Buffer_Filter NVARCHAR(MAX) NULL,            -- Filter for unbalanced buffer
    
    -- Dynamic Calculation
    Dyn_Calc_Script NVARCHAR(MAX) NULL,                         -- Dynamic calculation script
    Override_Value NVARCHAR(MAX) NULL,                          -- Override value
    
    -- Table Calculation Fields (for Table-based sources)
    Table_Calc_Expression NVARCHAR(MAX) NULL,                   -- Table calculation expression
    Table_Join_Expression NVARCHAR(MAX) NULL,                   -- Table join expression
    Table_Filter_Expression NVARCHAR(MAX) NULL,                 -- Table filter expression
    
    -- Mapping Configuration (for cross-system mappings)
    Map_Type NVARCHAR(50) NULL,                                 -- Mapping type
    Map_Source NVARCHAR(255) NULL,                              -- Mapping source
    Map_Logic NVARCHAR(MAX) NULL,                               -- Mapping logic
    
    -- SQL and Temporary Table Support
    Src_SQL_Stmt NVARCHAR(MAX) NULL,                            -- Source SQL statement
    Use_Temp_Table BIT NULL DEFAULT 0,                          -- Use temporary table
    Temp_Table_Name NVARCHAR(255) NULL,                         -- Temporary table name
    DB_Name NVARCHAR(255) NULL,                                 -- Database name for cross-DB queries
    
    -- Audit
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_FMM_Src_Cell PRIMARY KEY CLUSTERED (Cell_ID),
    CONSTRAINT FK_Src_Cell_Calc FOREIGN KEY (Calc_ID) 
        REFERENCES dbo.FMM_Calc_Config(Calc_ID) ON DELETE CASCADE
);
GO

-- Create indexes for efficient lookups
CREATE NONCLUSTERED INDEX IX_FMM_Src_Cell_Calc 
ON dbo.FMM_Src_Cell (Calc_ID, Src_Order) 
INCLUDE (Cell_ID, Cube_ID, Act_ID, Model_ID, Src_Type);
GO

CREATE NONCLUSTERED INDEX IX_FMM_Src_Cell_Cube_Act_Model 
ON dbo.FMM_Src_Cell (Cube_ID, Act_ID, Model_ID);
GO

-- =============================================
-- Helper Views for Easier Querying
-- =============================================

-- View: Complete calculation configuration with destination and source details
CREATE VIEW dbo.vw_FMM_Calc_Complete AS
SELECT 
    cc.Calc_ID,
    cc.Cube_ID,
    cc.Act_ID,
    cc.Model_ID,
    cc.Name AS Calc_Name,
    cc.Sequence,
    cc.Calc_Type,
    CASE cc.Calc_Type
        WHEN 1 THEN 'Table'
        WHEN 2 THEN 'Cube'
        WHEN 3 THEN 'BRTabletoCube'
        WHEN 4 THEN 'ImportTabletoCube'
        WHEN 5 THEN 'CubetoTable'
        WHEN 6 THEN 'Consolidate'
        ELSE 'Unknown'
    END AS Calc_Type_Name,
    cc.Calc_Condition,
    cc.Calc_Explanation,
    cc.Status,
    cc.MultiDim_Alloc,
    cc.BR_Calc,
    cc.BR_Calc_Name,
    dc.Dest_Cell_ID,
    dc.Acct AS Dest_Acct,
    dc.View AS Dest_View,
    dc.Flow AS Dest_Flow,
    COUNT(DISTINCT sc.Cell_ID) AS Source_Cell_Count,
    cc.Create_Date,
    cc.Create_User,
    cc.Update_Date,
    cc.Update_User
FROM dbo.FMM_Calc_Config cc
LEFT JOIN dbo.FMM_Dest_Cell dc ON cc.Calc_ID = dc.Calc_ID
LEFT JOIN dbo.FMM_Src_Cell sc ON cc.Calc_ID = sc.Calc_ID
GROUP BY 
    cc.Calc_ID, cc.Cube_ID, cc.Act_ID, cc.Model_ID, cc.Name, cc.Sequence,
    cc.Calc_Type, cc.Calc_Condition, cc.Calc_Explanation, cc.Status,
    cc.MultiDim_Alloc, cc.BR_Calc, cc.BR_Calc_Name,
    dc.Dest_Cell_ID, dc.Acct, dc.View, dc.Flow,
    cc.Create_Date, cc.Create_User, cc.Update_Date, cc.Update_User;
GO

-- View: Source cells with calculation context
CREATE VIEW dbo.vw_FMM_Src_Cell_Details AS
SELECT 
    sc.Cell_ID,
    sc.Calc_ID,
    cc.Name AS Calc_Name,
    cc.Calc_Type,
    sc.Src_Order,
    sc.Src_Type,
    sc.Src_Item,
    sc.Math_Operator,
    sc.Entity,
    sc.Scenario,
    sc.Time,
    sc.Acct,
    sc.View,
    sc.Flow,
    sc.Origin,
    sc.IC,
    sc.UD1,
    sc.UD2,
    sc.UD3,
    sc.UD4,
    sc.DB_Name,
    sc.Table_Calc_Expression,
    sc.Dyn_Calc_Script
FROM dbo.FMM_Src_Cell sc
INNER JOIN dbo.FMM_Calc_Config cc ON sc.Calc_ID = cc.Calc_ID;
GO

-- View: Destination cells with calculation context
CREATE VIEW dbo.vw_FMM_Dest_Cell_Details AS
SELECT 
    dc.Dest_Cell_ID,
    dc.Calc_ID,
    cc.Name AS Calc_Name,
    cc.Calc_Type,
    dc.Location,
    dc.Cons,
    dc.View,
    dc.Acct,
    dc.Flow,
    dc.IC,
    dc.Origin,
    dc.UD1,
    dc.UD2,
    dc.UD3,
    dc.UD4,
    dc.Time_Filter,
    dc.Acct_Filter,
    dc.SQL_Logic
FROM dbo.FMM_Dest_Cell dc
INNER JOIN dbo.FMM_Calc_Config cc ON dc.Calc_ID = cc.Calc_ID;
GO

-- View: Calculation summary by type
CREATE VIEW dbo.vw_FMM_Calc_Summary_By_Type AS
SELECT 
    Calc_Type,
    CASE Calc_Type
        WHEN 1 THEN 'Table'
        WHEN 2 THEN 'Cube'
        WHEN 3 THEN 'BRTabletoCube'
        WHEN 4 THEN 'ImportTabletoCube'
        WHEN 5 THEN 'CubetoTable'
        WHEN 6 THEN 'Consolidate'
        ELSE 'Unknown'
    END AS Calc_Type_Name,
    Status,
    COUNT(*) AS Calc_Count,
    SUM(CASE WHEN MultiDim_Alloc = 1 THEN 1 ELSE 0 END) AS MultiDim_Alloc_Count,
    SUM(CASE WHEN BR_Calc = 1 THEN 1 ELSE 0 END) AS BR_Calc_Count
FROM dbo.FMM_Calc_Config
GROUP BY Calc_Type, Status;
GO

PRINT 'FMM Calculation Configuration System schema created successfully.';
PRINT '';
PRINT 'Tables created:';
PRINT '  - FMM_Calc_Config (Core calculation configuration)';
PRINT '  - FMM_Dest_Cell (Destination cell configuration)';
PRINT '  - FMM_Src_Cell (Source cell configuration)';
PRINT '';
PRINT 'Supported calculation types:';
PRINT '  1 = Table (table-based calculations)';
PRINT '  2 = Cube (cube-based calculations)';
PRINT '  3 = BRTabletoCube (Business Rule table to cube)';
PRINT '  4 = ImportTabletoCube (Import table to cube)';
PRINT '  5 = CubetoTable (Cube to table)';
PRINT '  6 = Consolidate (Consolidation calculations)';
PRINT '';
PRINT 'Views created for easier querying:';
PRINT '  - vw_FMM_Calc_Complete';
PRINT '  - vw_FMM_Src_Cell_Details';
PRINT '  - vw_FMM_Dest_Cell_Details';
PRINT '  - vw_FMM_Calc_Summary_By_Type';
GO
