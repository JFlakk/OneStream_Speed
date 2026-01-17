-- =============================================
-- CMD SPLN Cube-to-Table Calculation Configuration System
-- Sample Configuration Data - CivPay Example
-- Based on CMD_SPLN_FinCustCalc.Copy_CivPay (lines 253-461)
-- =============================================

-- =============================================
-- EXAMPLE 1: Copy_CivPay Configuration
-- =============================================
-- This configuration replicates the CivPay calculation logic:
-- 1. Reads cube data with Account#Target, U6#Pay_Benefits filters
-- 2. Maps to REQ header (by APPN, MDEP, APE9, Dollar_Type, Ctype)
-- 3. Creates details for Commitments and Obligations
-- 4. Applies spread rates from CMD_SPLN_APPN_SpreadPct_FDX_CV
-- 5. Distributes across months (and carryover year if Time13 <> 0)

DECLARE @CivPayCalcID INT;

-- =============================================
-- Step 1: Create Calc Configuration Header
-- =============================================
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Config (
    Calc_Name,
    Calc_Type,
    Description,
    WF_CMD_Name,
    WF_Scenario_Pattern,
    Source_Type,
    Source_Cube_Name,
    Source_Formula_Template,
    Dest_Type,
    Dest_Table_Header,
    Dest_Table_Detail,
    Use_Spread_Rates,
    Spread_Rate_FDX_Name,
    Spread_Rate_Match_Columns,
    REQ_ID_Type,
    Generate_REQ_ID,
    Status_Level_Template,
    Execution_Order,
    Is_Active,
    Create_User,
    Update_User
)
VALUES (
    'Copy_CivPay',                                                  -- Calc_Name
    'CubeToTable',                                                  -- Calc_Type
    'Copy CivPay funding lines from cube to REQ/REQ_Details tables with APPN spread rates', -- Description
    NULL,                                                           -- WF_CMD_Name (any CMD)
    'CMD_SPLN_C%',                                                  -- WF_Scenario_Pattern
    'Cube',                                                         -- Source_Type
    NULL,                                                           -- Source_Cube_Name (uses current POV cube)
    'FilterMembers(RemoveZeros(E#{Entity}:S#{SourcePosition}:T#{TargetYear}:C#Aggregated:V#Periodic:F#Tot_Dist_Final:O#Top:I#Top:U6#Pay_Benefits:U7#Top:U8#Top),[A#Target])', -- Source_Formula_Template
    'Table',                                                        -- Dest_Type
    'XFC_CMD_SPLN_REQ',                                             -- Dest_Table_Header
    'XFC_CMD_SPLN_REQ_Details',                                     -- Dest_Table_Detail
    1,                                                              -- Use_Spread_Rates
    'CMD_SPLN_APPN_SpreadPct_FDX_CV',                              -- Spread_Rate_FDX_Name
    'UD1,Account',                                                  -- Spread_Rate_Match_Columns (APPN ancestor from UD1, Account prefix match)
    'CivPay_Copied',                                                -- REQ_ID_Type
    1,                                                              -- Generate_REQ_ID
    '{EntityLevel}_Formulate_SPLN',                                -- Status_Level_Template
    10,                                                             -- Execution_Order
    1,                                                              -- Is_Active
    'SYSTEM',                                                       -- Create_User
    'SYSTEM'                                                        -- Update_User
);

SET @CivPayCalcID = SCOPE_IDENTITY();
PRINT 'Created Copy_CivPay Calc Config ID: ' + CAST(@CivPayCalcID AS NVARCHAR(10));

-- =============================================
-- Step 2: Define Source Cell Configuration
-- =============================================
-- For CivPay, we read from cube with specific filters
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Src_Cell (
    Calc_Config_ID,
    Src_Order,
    Src_Type,
    Src_Item,
    Entity,
    Scenario,
    Time,
    View,
    Account,
    IC,
    Origin,
    Flow,
    UD1,
    UD2,
    UD3,
    UD4,
    UD5,
    UD6,
    UD7,
    UD8,
    Additional_Filter,
    Math_Operator,
    Multiplier,
    Group_By_Dimensions,
    Aggregation_Method,
    Is_Active,
    Create_User,
    Update_User
)
VALUES (
    @CivPayCalcID,                                                  -- Calc_Config_ID
    1,                                                              -- Src_Order
    'CubeCell',                                                     -- Src_Type
    'CivPay_FundingLines',                                          -- Src_Item
    '{POV}',                                                        -- Entity (use POV entity)
    '{SourcePosition}',                                             -- Scenario (runtime parameter)
    '{TargetYear}',                                                 -- Time (runtime parameter)
    'Periodic',                                                     -- View
    'Target',                                                       -- Account (filter to Target account)
    'Top',                                                          -- IC
    'Top',                                                          -- Origin
    'Tot_Dist_Final',                                               -- Flow
    NULL,                                                           -- UD1 (any - will iterate)
    NULL,                                                           -- UD2 (any - will iterate)
    NULL,                                                           -- UD3 (any - will iterate)
    NULL,                                                           -- UD4 (any - will iterate)
    NULL,                                                           -- UD5 (any - will iterate)
    'Pay_Benefits',                                                 -- UD6 (filter to Pay_Benefits)
    'Top',                                                          -- UD7
    'Top',                                                          -- UD8
    'RemoveZeros',                                                  -- Additional_Filter
    NULL,                                                           -- Math_Operator
    1.0,                                                            -- Multiplier
    'UD1,UD2,UD3,UD4,UD5',                                          -- Group_By_Dimensions
    'SUM',                                                          -- Aggregation_Method
    1,                                                              -- Is_Active
    'SYSTEM',                                                       -- Create_User
    'SYSTEM'                                                        -- Update_User
);

PRINT 'Created Copy_CivPay Source Cell Configuration';

-- =============================================
-- Step 3: Define Destination Cell Mappings - HEADER TABLE
-- =============================================
-- Map to XFC_CMD_SPLN_REQ (header table)
-- These mappings create one REQ header per unique combination of APPN/MDEP/APE9/Dollar_Type/Ctype

-- WFScenario_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFScenario_Name', 'Constant', '{TargetScenario}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- WFTime_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFTime_Name', 'Constant', '{TimeName}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- WFCMD_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFCMD_Name', 'Constant', '{CMDName}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Entity
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Entity', 'Dimension', 'Entity', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- APPN (from UD1)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'APPN', 'Dimension', 'UD1', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- MDEP (from UD2)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'MDEP', 'Dimension', 'UD2', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- APE9 (from UD3)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'APE9', 'Dimension', 'UD3', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Dollar_Type (from UD4)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Dollar_Type', 'Dimension', 'UD4', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Ctype (from UD5)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Ctype', 'Dimension', 'UD5', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Obj_Class (constant for CivPay)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Obj_Class', 'Constant', 'Pay_General', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- UIC (constant)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'UIC', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Status
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Status', 'Generated', '{EntityLevel}_Formulate_SPLN', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Title (formula-based)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Title', 'Formula', 'CivPay - {Entity} - {UD1}-{UD2}-{UD3}-{UD4}-{UD5}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- REQ_ID_Type
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'REQ_ID_Type', 'Constant', 'CivPay_Copied', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- REQ_ID (generated)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'REQ_ID', 'Generated', 'GetNextREQID', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- CMD_SPLN_REQ_ID (GUID)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'CMD_SPLN_REQ_ID', 'Generated', 'GUID', 'None', NULL, 'UNIQUEIDENTIFIER', 1, 1, 'SYSTEM', 'SYSTEM');

-- Audit columns for header
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Create_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Create_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Update_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Update_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

PRINT 'Created Copy_CivPay Header Destination Cell Mappings';

-- =============================================
-- Step 4: Define Destination Cell Mappings - DETAIL TABLE
-- =============================================
-- Map to XFC_CMD_SPLN_REQ_Details (detail table)
-- Creates 2 detail records per funding line (Commitments and Obligations) with time spread

-- Common detail fields
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'CMD_SPLN_REQ_ID', 'Generated', 'ParentGUID', 'UNIQUEIDENTIFIER', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFScenario_Name', 'Constant', '{TargetScenario}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFTime_Name', 'Constant', '{TimeName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFCMD_Name', 'Constant', '{CMDName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Unit_of_Measure', 'Constant', 'Funding', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Entity', 'Dimension', 'Entity', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'IC', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Flow', 'Generated', '{EntityLevel}_Formulate_SPLN', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD1', 'Dimension', 'UD1', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD2', 'Dimension', 'UD2', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD3', 'Dimension', 'UD3', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD4', 'Dimension', 'UD4', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD5', 'Dimension', 'UD5', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD6', 'Constant', 'Pay_General', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD7', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD8', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Fiscal_Year', 'Constant', '{TimeName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Account field - conditional based on iteration (Commitments vs Obligations)
-- This uses condition expression to handle Commit vs Obligation
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, 
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Condition_Expression, Condition_True_Value, Condition_False_Value,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Account',
    'Formula', '{AccountType}', 'Append', 'ments|s',               -- Appends 'ments' for Commit, 's' for Obligation
    'AccountType = ''Commit''', 'Commitments', 'Obligations',
    'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'
);

-- Monthly spread values (Month1-Month12) - applies spread rate formula
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Apply_Time_Spread, Time_Column_Pattern,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Month{1-12}',
    'SpreadRate', '{CellAmount} * {SpreadRate_Time{N}} / 100', 'Calculate', NULL,
    1, 'Month{1-12}',
    'DECIMAL', 0, 1, 'SYSTEM', 'SYSTEM'
);

-- Carryover to next year (Yearly column for next fiscal year)
-- Only created if Time13 spread rate <> 0
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Condition_Expression, Condition_True_Value, Condition_False_Value,
    Apply_Time_Spread, Time_Column_Pattern,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Yearly',
    'SpreadRate', '{CellAmount} * {SpreadRate_Time13} / 100', 'Calculate', NULL,
    'SpreadRate_Time13 <> 0', '{Formula}', '0',
    1, 'Yearly',
    'DECIMAL', 0, 1, 'SYSTEM', 'SYSTEM'
);

-- Audit columns for detail
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Create_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Create_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Update_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@CivPayCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Update_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

PRINT 'Created Copy_CivPay Detail Destination Cell Mappings';

PRINT '';
PRINT '===== Copy_CivPay Configuration Complete =====';
PRINT 'Calc_Config_ID: ' + CAST(@CivPayCalcID AS NVARCHAR(10));
PRINT 'Total Source Cells: 1';
PRINT 'Total Dest Cells (Header): ~16';
PRINT 'Total Dest Cells (Detail): ~21';
PRINT '';
GO
