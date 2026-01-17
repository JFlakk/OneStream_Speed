-- =============================================
-- CMD SPLN Cube-to-Table Calculation Configuration System
-- Database Schema Definition
-- Based on CMD_SPLN_FinCustCalc CivPay and Withhold patterns
-- =============================================

-- Drop tables in reverse dependency order if they exist
IF OBJECT_ID('dbo.XFC_CMD_SPLN_Calc_Dest_Cell', 'U') IS NOT NULL DROP TABLE dbo.XFC_CMD_SPLN_Calc_Dest_Cell;
IF OBJECT_ID('dbo.XFC_CMD_SPLN_Calc_Src_Cell', 'U') IS NOT NULL DROP TABLE dbo.XFC_CMD_SPLN_Calc_Src_Cell;
IF OBJECT_ID('dbo.XFC_CMD_SPLN_Calc_Config', 'U') IS NOT NULL DROP TABLE dbo.XFC_CMD_SPLN_Calc_Config;
GO

-- =============================================
-- 1. XFC_CMD_SPLN_Calc_Config - Calculation Header Configuration
-- =============================================
-- Purpose: Defines a cube-to-table calculation configuration similar to CivPay/Withhold
-- Each record represents a calculation that reads from cube and writes to table(s)
CREATE TABLE dbo.XFC_CMD_SPLN_Calc_Config (
    Calc_Config_ID INT IDENTITY(1,1) NOT NULL,
    
    -- Identification
    Calc_Name NVARCHAR(100) NOT NULL,                              -- e.g., 'Copy_CivPay', 'Copy_Withhold'
    Calc_Type NVARCHAR(50) NOT NULL,                               -- e.g., 'CubeToTable', 'TableToCube', 'Transform'
    Description NVARCHAR(500) NULL,
    
    -- Workflow Context
    WF_CMD_Name NVARCHAR(100) NULL,                                -- Optional CMD filter
    WF_Scenario_Pattern NVARCHAR(255) NULL,                        -- e.g., 'CMD_SPLN_C%' for target scenario pattern
    
    -- Source Configuration (Cube)
    Source_Type NVARCHAR(50) NOT NULL DEFAULT 'Cube',              -- 'Cube', 'Table', 'FDX'
    Source_Cube_Name NVARCHAR(100) NULL,                           -- Cube name if source is cube
    Source_Formula_Template NVARCHAR(MAX) NULL,                    -- Formula template with placeholders, e.g., 
                                                                    -- 'FilterMembers(RemoveZeros(E#{Entity}:S#{Scenario}:T#{Year}:C#Aggregated:V#Periodic:F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])'
    
    -- Destination Configuration (Table)
    Dest_Type NVARCHAR(50) NOT NULL DEFAULT 'Table',               -- 'Table', 'Cube', 'Both'
    Dest_Table_Header NVARCHAR(255) NULL,                          -- e.g., 'XFC_CMD_SPLN_REQ' for header table
    Dest_Table_Detail NVARCHAR(255) NULL,                          -- e.g., 'XFC_CMD_SPLN_REQ_Details' for detail table
    
    -- Spread Rate Configuration (Optional)
    Use_Spread_Rates BIT NOT NULL DEFAULT 0,                       -- Whether to apply spread rates
    Spread_Rate_FDX_Name NVARCHAR(255) NULL,                       -- e.g., 'CMD_SPLN_APPN_SpreadPct_FDX_CV'
    Spread_Rate_Match_Columns NVARCHAR(500) NULL,                  -- Comma-separated list of columns to match, e.g., 'UD1,Account'
    
    -- REQ ID Configuration
    REQ_ID_Type NVARCHAR(50) NULL,                                 -- e.g., 'CivPay_Copied', 'Withhold'
    Generate_REQ_ID BIT NOT NULL DEFAULT 1,                        -- Whether to auto-generate REQ_ID
    
    -- Status/Flow Configuration
    Status_Level_Template NVARCHAR(255) NULL,                      -- e.g., '{EntityLevel}_Formulate_SPLN'
    
    -- Execution Order
    Execution_Order INT NOT NULL DEFAULT 100,
    
    -- Active Flag
    Is_Active BIT NOT NULL DEFAULT 1,
    
    -- Audit Columns
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_XFC_CMD_SPLN_Calc_Config PRIMARY KEY CLUSTERED (Calc_Config_ID),
    CONSTRAINT UQ_Calc_Name UNIQUE (Calc_Name)
);
GO

-- Create index on Calc_Name for quick lookups
CREATE NONCLUSTERED INDEX IX_XFC_CMD_SPLN_Calc_Config_Name 
ON dbo.XFC_CMD_SPLN_Calc_Config (Calc_Name, Is_Active);
GO

-- =============================================
-- 2. XFC_CMD_SPLN_Calc_Src_Cell - Source Cell Configuration
-- =============================================
-- Purpose: Defines how to map cube cells or table rows to the calculation
-- Inspired by FMM_Src_Cell structure
CREATE TABLE dbo.XFC_CMD_SPLN_Calc_Src_Cell (
    Src_Cell_ID INT IDENTITY(1,1) NOT NULL,
    Calc_Config_ID INT NOT NULL,
    
    -- Ordering
    Src_Order INT NOT NULL DEFAULT 1,
    
    -- Source Type and Item
    Src_Type NVARCHAR(50) NOT NULL,                                -- 'CubeCell', 'TableRow', 'Constant', 'Formula'
    Src_Item NVARCHAR(100) NULL,                                   -- Item name/identifier
    
    -- Dimension Filters (for CubeCell source type)
    -- These can be specific values or patterns
    Entity NVARCHAR(255) NULL,                                      -- e.g., '{POV}', 'FORSCOM', NULL (any)
    Scenario NVARCHAR(255) NULL,
    Time NVARCHAR(255) NULL,
    View NVARCHAR(255) NULL,
    Account NVARCHAR(255) NULL,                                     -- e.g., 'Target', 'Commitments,Obligations'
    IC NVARCHAR(255) NULL,
    Origin NVARCHAR(255) NULL,
    Flow NVARCHAR(255) NULL,
    UD1 NVARCHAR(255) NULL,
    UD2 NVARCHAR(255) NULL,
    UD3 NVARCHAR(255) NULL,
    UD4 NVARCHAR(255) NULL,
    UD5 NVARCHAR(255) NULL,
    UD6 NVARCHAR(255) NULL,                                         -- e.g., 'Pay_Benefits'
    UD7 NVARCHAR(255) NULL,
    UD8 NVARCHAR(255) NULL,
    
    -- Additional Filters (for advanced filtering)
    Additional_Filter NVARCHAR(MAX) NULL,                           -- e.g., 'RemoveZeros(...)'
    
    -- Math Operations
    Math_Operator NVARCHAR(10) NULL,                                -- '+', '-', '*', '/', NULL
    Multiplier DECIMAL(18, 6) NULL DEFAULT 1.0,                     -- Multiply source value by this
    
    -- Grouping/Aggregation
    Group_By_Dimensions NVARCHAR(500) NULL,                         -- Comma-separated list of dimensions to group by
    Aggregation_Method NVARCHAR(50) NULL DEFAULT 'SUM',             -- 'SUM', 'AVG', 'MIN', 'MAX', 'COUNT'
    
    -- Active Flag
    Is_Active BIT NOT NULL DEFAULT 1,
    
    -- Audit Columns
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_XFC_CMD_SPLN_Calc_Src_Cell PRIMARY KEY CLUSTERED (Src_Cell_ID),
    CONSTRAINT FK_SrcCell_CalcConfig FOREIGN KEY (Calc_Config_ID) 
        REFERENCES dbo.XFC_CMD_SPLN_Calc_Config(Calc_Config_ID) ON DELETE CASCADE
);
GO

-- Create index on Calc_Config_ID for efficient lookups
CREATE NONCLUSTERED INDEX IX_XFC_CMD_SPLN_Calc_Src_Cell_Config 
ON dbo.XFC_CMD_SPLN_Calc_Src_Cell (Calc_Config_ID, Src_Order, Is_Active);
GO

-- =============================================
-- 3. XFC_CMD_SPLN_Calc_Dest_Cell - Destination Cell/Table Configuration
-- =============================================
-- Purpose: Defines how to map calculation results to destination table columns
-- Maps source dimensions/values to destination table structure
CREATE TABLE dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Dest_Cell_ID INT IDENTITY(1,1) NOT NULL,
    Calc_Config_ID INT NOT NULL,
    
    -- Destination Table
    Dest_Table_Name NVARCHAR(255) NOT NULL,                        -- 'XFC_CMD_SPLN_REQ' or 'XFC_CMD_SPLN_REQ_Details'
    Dest_Table_Type NVARCHAR(50) NOT NULL,                         -- 'Header', 'Detail'
    
    -- Column Mapping
    Dest_Column_Name NVARCHAR(255) NOT NULL,                       -- Target column name
    
    -- Source Mapping
    Source_Type NVARCHAR(50) NOT NULL,                             -- 'Dimension', 'Constant', 'Formula', 'SpreadRate', 'Generated'
    Source_Value NVARCHAR(MAX) NULL,                               -- The value or expression to map
                                                                    -- Examples:
                                                                    -- Dimension: 'UD1', 'UD2', 'Account'
                                                                    -- Constant: 'CivPay_Copied', 'Withhold', 'None'
                                                                    -- Formula: '{CellAmount} * {SpreadRate} / 100'
                                                                    -- Generated: 'REQ_ID', 'GUID', 'DateTime'
    
    -- Transformation Rules
    Transform_Type NVARCHAR(50) NULL,                              -- 'None', 'Append', 'Replace', 'Lookup', 'Calculate'
    Transform_Rule NVARCHAR(MAX) NULL,                             -- Transformation expression
                                                                    -- Examples:
                                                                    -- Append: 'ments' (Commit → Commitments)
                                                                    -- Replace: 'Pay_Benefits,Pay_General→Pay_General'
                                                                    -- Lookup: 'FDX:{FDXName}:Column:{ColumnName}'
    
    -- Conditional Mapping (Optional)
    Condition_Expression NVARCHAR(MAX) NULL,                        -- e.g., 'Account = ''Commit'''
    Condition_True_Value NVARCHAR(MAX) NULL,                        -- Value if condition is true
    Condition_False_Value NVARCHAR(MAX) NULL,                       -- Value if condition is false
    
    -- Spread Rate Time Distribution (for Detail tables)
    Apply_Time_Spread BIT NOT NULL DEFAULT 0,                       -- Whether to apply monthly/quarterly spread
    Time_Column_Pattern NVARCHAR(100) NULL,                         -- e.g., 'Month{1-12}', 'Quarter{1-4}', 'Yearly'
    
    -- Data Type (for validation)
    Data_Type NVARCHAR(50) NULL,                                    -- 'NVARCHAR', 'INT', 'DECIMAL', 'DATETIME', 'GUID', 'BIT'
    
    -- Default Value (if source is NULL)
    Default_Value NVARCHAR(MAX) NULL,
    
    -- Required Field
    Is_Required BIT NOT NULL DEFAULT 0,
    
    -- Active Flag
    Is_Active BIT NOT NULL DEFAULT 1,
    
    -- Audit Columns
    Create_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Create_User NVARCHAR(100) NULL,
    Update_Date DATETIME NOT NULL DEFAULT GETDATE(),
    Update_User NVARCHAR(100) NULL,
    
    CONSTRAINT PK_XFC_CMD_SPLN_Calc_Dest_Cell PRIMARY KEY CLUSTERED (Dest_Cell_ID),
    CONSTRAINT FK_DestCell_CalcConfig FOREIGN KEY (Calc_Config_ID) 
        REFERENCES dbo.XFC_CMD_SPLN_Calc_Config(Calc_Config_ID) ON DELETE CASCADE
);
GO

-- Create index on Calc_Config_ID and Dest_Table_Name
CREATE NONCLUSTERED INDEX IX_XFC_CMD_SPLN_Calc_Dest_Cell_Config 
ON dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Is_Active);
GO

-- =============================================
-- Create views for easier querying
-- =============================================

-- View to see complete calculation configuration
CREATE VIEW dbo.vw_XFC_CMD_SPLN_Calc_Summary AS
SELECT 
    cc.Calc_Config_ID,
    cc.Calc_Name,
    cc.Calc_Type,
    cc.Description,
    cc.Source_Type,
    cc.Source_Cube_Name,
    cc.Dest_Type,
    cc.Dest_Table_Header,
    cc.Dest_Table_Detail,
    cc.REQ_ID_Type,
    cc.Is_Active,
    COUNT(DISTINCT sc.Src_Cell_ID) AS Source_Cell_Count,
    COUNT(DISTINCT dc.Dest_Cell_ID) AS Dest_Cell_Count
FROM dbo.XFC_CMD_SPLN_Calc_Config cc
LEFT JOIN dbo.XFC_CMD_SPLN_Calc_Src_Cell sc ON cc.Calc_Config_ID = sc.Calc_Config_ID AND sc.Is_Active = 1
LEFT JOIN dbo.XFC_CMD_SPLN_Calc_Dest_Cell dc ON cc.Calc_Config_ID = dc.Calc_Config_ID AND dc.Is_Active = 1
WHERE cc.Is_Active = 1
GROUP BY 
    cc.Calc_Config_ID, cc.Calc_Name, cc.Calc_Type, cc.Description,
    cc.Source_Type, cc.Source_Cube_Name, cc.Dest_Type,
    cc.Dest_Table_Header, cc.Dest_Table_Detail, cc.REQ_ID_Type, cc.Is_Active;
GO

-- View to see source cell mappings
CREATE VIEW dbo.vw_XFC_CMD_SPLN_Calc_Src_Cells AS
SELECT 
    cc.Calc_Name,
    sc.Src_Cell_ID,
    sc.Src_Order,
    sc.Src_Type,
    sc.Src_Item,
    sc.Entity,
    sc.Scenario,
    sc.Time,
    sc.Account,
    sc.UD1,
    sc.UD2,
    sc.UD3,
    sc.UD4,
    sc.UD5,
    sc.UD6,
    sc.Math_Operator,
    sc.Multiplier,
    sc.Aggregation_Method
FROM dbo.XFC_CMD_SPLN_Calc_Config cc
INNER JOIN dbo.XFC_CMD_SPLN_Calc_Src_Cell sc ON cc.Calc_Config_ID = sc.Calc_Config_ID
WHERE cc.Is_Active = 1 AND sc.Is_Active = 1;
GO

-- View to see destination cell mappings
CREATE VIEW dbo.vw_XFC_CMD_SPLN_Calc_Dest_Cells AS
SELECT 
    cc.Calc_Name,
    dc.Dest_Cell_ID,
    dc.Dest_Table_Name,
    dc.Dest_Table_Type,
    dc.Dest_Column_Name,
    dc.Source_Type,
    dc.Source_Value,
    dc.Transform_Type,
    dc.Transform_Rule,
    dc.Apply_Time_Spread,
    dc.Time_Column_Pattern,
    dc.Data_Type,
    dc.Is_Required
FROM dbo.XFC_CMD_SPLN_Calc_Config cc
INNER JOIN dbo.XFC_CMD_SPLN_Calc_Dest_Cell dc ON cc.Calc_Config_ID = dc.Calc_Config_ID
WHERE cc.Is_Active = 1 AND dc.Is_Active = 1;
GO

PRINT 'CMD SPLN Cube-to-Table Calculation Configuration System schema created successfully.';
GO
