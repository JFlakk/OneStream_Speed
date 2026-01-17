-- =============================================
-- CMD SPLN Cube-to-Table Calculation Configuration System
-- Sample Configuration Data - Withhold Example
-- Based on CMD_SPLN_FinCustCalc.Copy_Withhold (lines 465-670)
-- =============================================

-- =============================================
-- EXAMPLE 2: Copy_Withhold Configuration
-- =============================================
-- This configuration replicates the Withhold calculation logic:
-- 1. Reads cube data with Account#TGT_WH filter (withhold accounts)
-- 2. Maps to REQ header (by APPN only - simplified structure)
-- 3. Creates details for WH_Commitments and WH_Obligations
-- 4. Applies spread rates from CMD_SPLN_WH_SpreadPct_FDX_CV
-- 5. Distributes across months (no carryover year for withhold)
-- 6. UD6 is transformed: Pay_Benefits/Pay_General → Pay_General, others → NonPay_General

DECLARE @WithholdCalcID INT;

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
    'Copy_Withhold',                                                -- Calc_Name
    'CubeToTable',                                                  -- Calc_Type
    'Copy Withhold funding lines from cube to REQ/REQ_Details tables with withhold spread rates', -- Description
    '{CMDName}',                                                    -- WF_CMD_Name (uses cube name filter)
    'CMD_SPLN_C%',                                                  -- WF_Scenario_Pattern
    'Cube',                                                         -- Source_Type
    '{CMDName}',                                                    -- Source_Cube_Name (from runtime)
    'FilterMembers(RemoveZeros(Cb#{CMDName}:E#{Entity}:S#{SourcePosition}:T#{TargetYear}:C#Aggregated:V#Periodic:F#Tot_Dist_Final:O#Top:I#None:U7#Top:U8#None),[A#TGT_WH])', -- Source_Formula_Template
    'Table',                                                        -- Dest_Type
    'XFC_CMD_SPLN_REQ',                                             -- Dest_Table_Header
    'XFC_CMD_SPLN_REQ_Details',                                     -- Dest_Table_Detail
    1,                                                              -- Use_Spread_Rates
    'CMD_SPLN_WH_SpreadPct_FDX_CV',                                 -- Spread_Rate_FDX_Name
    'Account',                                                      -- Spread_Rate_Match_Columns (Account prefix match only)
    'Withhold',                                                     -- REQ_ID_Type
    1,                                                              -- Generate_REQ_ID
    '{EntityLevel}_Formulate_SPLN',                                -- Status_Level_Template
    20,                                                             -- Execution_Order
    1,                                                              -- Is_Active
    'SYSTEM',                                                       -- Create_User
    'SYSTEM'                                                        -- Update_User
);

SET @WithholdCalcID = SCOPE_IDENTITY();
PRINT 'Created Copy_Withhold Calc Config ID: ' + CAST(@WithholdCalcID AS NVARCHAR(10));

-- =============================================
-- Step 2: Define Source Cell Configuration
-- =============================================
-- For Withhold, we read from cube with TGT_WH account filter
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
    @WithholdCalcID,                                                -- Calc_Config_ID
    1,                                                              -- Src_Order
    'CubeCell',                                                     -- Src_Type
    'Withhold_FundingLines',                                        -- Src_Item
    '{POV}',                                                        -- Entity (use POV entity)
    '{SourcePosition}',                                             -- Scenario (runtime parameter)
    '{TargetYear}',                                                 -- Time (runtime parameter)
    'Periodic',                                                     -- View
    'TGT_WH',                                                       -- Account (filter to TGT_WH accounts)
    'None',                                                         -- IC (different from CivPay!)
    'Top',                                                          -- Origin
    'Tot_Dist_Final',                                               -- Flow
    NULL,                                                           -- UD1 (any - will iterate)
    NULL,                                                           -- UD2 (any - will iterate)
    NULL,                                                           -- UD3 (any - will iterate)
    NULL,                                                           -- UD4 (any - will iterate)
    NULL,                                                           -- UD5 (any - will iterate)
    NULL,                                                           -- UD6 (any - will iterate, then transformed)
    'Top',                                                          -- UD7
    'None',                                                         -- UD8 (different from CivPay!)
    'RemoveZeros',                                                  -- Additional_Filter
    NULL,                                                           -- Math_Operator
    1.0,                                                            -- Multiplier
    'UD1,UD2,UD3,UD4,UD5,UD6',                                      -- Group_By_Dimensions (includes UD6)
    'SUM',                                                          -- Aggregation_Method
    1,                                                              -- Is_Active
    'SYSTEM',                                                       -- Create_User
    'SYSTEM'                                                        -- Update_User
);

PRINT 'Created Copy_Withhold Source Cell Configuration';

-- =============================================
-- Step 3: Define Destination Cell Mappings - HEADER TABLE
-- =============================================
-- Map to XFC_CMD_SPLN_REQ (header table)
-- Withhold creates one REQ header per APPN (simplified vs CivPay)

-- WFScenario_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFScenario_Name', 'Constant', '{TargetScenario}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- WFTime_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFTime_Name', 'Constant', '{TimeName}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- WFCMD_Name
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'WFCMD_Name', 'Constant', '{CMDName}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Entity
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Entity', 'Dimension', 'Entity', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- APPN (extracted from UD3 - first part before underscore)
-- Code does: APPN = UD3.Split("_")(0)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'APPN', 'Formula', '{UD3}.Split(''_'')[0]', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- MDEP (constant None for Withhold)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'MDEP', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- APE9 (constant None for Withhold)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'APE9', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Dollar_Type (constant None for Withhold)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Dollar_Type', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Ctype (constant None for Withhold)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Ctype', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Obj_Class (constant None for Withhold)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Obj_Class', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- UIC (constant None)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'UIC', 'Constant', 'None', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Status
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Status', 'Generated', '{EntityLevel}_Formulate_SPLN', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- Title (formula-based - simpler than CivPay)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Title', 'Formula', 'Withhold - {Entity} - {APPN}', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- REQ_ID_Type
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'REQ_ID_Type', 'Constant', 'Withhold', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- REQ_ID (generated)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'REQ_ID', 'Generated', 'GetNextREQID', 'None', NULL, 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- CMD_SPLN_REQ_ID (GUID)
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Transform_Type, Transform_Rule, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'CMD_SPLN_REQ_ID', 'Generated', 'GUID', 'None', NULL, 'UNIQUEIDENTIFIER', 1, 1, 'SYSTEM', 'SYSTEM');

-- Audit columns for header
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Create_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Create_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Update_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ', 'Header', 'Update_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

PRINT 'Created Copy_Withhold Header Destination Cell Mappings';

-- =============================================
-- Step 4: Define Destination Cell Mappings - DETAIL TABLE
-- =============================================
-- Map to XFC_CMD_SPLN_REQ_Details (detail table)
-- Creates 2 detail records per funding line (WH_Commitments and WH_Obligations) with time spread

-- Common detail fields
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'CMD_SPLN_REQ_ID', 'Generated', 'ParentGUID', 'UNIQUEIDENTIFIER', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFScenario_Name', 'Constant', '{TargetScenario}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFTime_Name', 'Constant', '{TimeName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'WFCMD_Name', 'Constant', '{CMDName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Unit_of_Measure', 'Constant', 'Funding', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Entity', 'Dimension', 'Entity', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'IC', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Flow', 'Generated', '{EntityLevel}_Formulate_SPLN', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD1', 'Dimension', 'UD1', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD2', 'Dimension', 'UD2', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD3', 'Dimension', 'UD3', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD4', 'Dimension', 'UD4', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD5', 'Dimension', 'UD5', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD7', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD8', 'Constant', 'None', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Fiscal_Year', 'Constant', '{TimeName}', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

-- UD6 - special transformation logic for Withhold
-- If (UD6.XFEqualsIgnoreCase("Pay_Benefits") Or UD6.XFEqualsIgnoreCase("Pay_General")) Then UD6= "Pay_General" Else UD6 = "NonPay_General"
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Condition_Expression, Condition_True_Value, Condition_False_Value,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'UD6',
    'Dimension', 'UD6', 'Replace', 'Pay_Benefits,Pay_General→Pay_General',
    'UD6 IN (''Pay_Benefits'', ''Pay_General'')', 'Pay_General', 'NonPay_General',
    'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'
);

-- Account field - conditional with WH_ prefix
-- For Withhold: WH_Commitments or WH_Obligations
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, 
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Condition_Expression, Condition_True_Value, Condition_False_Value,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Account',
    'Formula', 'WH_{AccountType}', 'Append', 'ments|s',            -- Prepends WH_ and appends 'ments' for Commit, 's' for Obligation
    'AccountType = ''Commit''', 'WH_Commitments', 'WH_Obligations',
    'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'
);

-- Monthly spread values (Month1-Month12) - applies spread rate formula
-- Withhold spread rates do NOT have Time13 (no carryover), so only Month1-12
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (
    Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name,
    Source_Type, Source_Value, Transform_Type, Transform_Rule,
    Apply_Time_Spread, Time_Column_Pattern,
    Data_Type, Is_Required, Is_Active, Create_User, Update_User
)
VALUES (
    @WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Month{1-12}',
    'SpreadRate', '{CellAmount} * {SpreadRate_Time{N}} / 100', 'Calculate', NULL,
    1, 'Month{1-12}',
    'DECIMAL', 0, 1, 'SYSTEM', 'SYSTEM'
);

-- Audit columns for detail
INSERT INTO dbo.XFC_CMD_SPLN_Calc_Dest_Cell (Calc_Config_ID, Dest_Table_Name, Dest_Table_Type, Dest_Column_Name, Source_Type, Source_Value, Data_Type, Is_Required, Is_Active, Create_User, Update_User)
VALUES 
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Create_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Create_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Update_Date', 'Generated', 'DateTime', 'DATETIME', 1, 1, 'SYSTEM', 'SYSTEM'),
    (@WithholdCalcID, 'XFC_CMD_SPLN_REQ_Details', 'Detail', 'Update_User', 'Generated', 'UserName', 'NVARCHAR', 1, 1, 'SYSTEM', 'SYSTEM');

PRINT 'Created Copy_Withhold Detail Destination Cell Mappings';

PRINT '';
PRINT '===== Copy_Withhold Configuration Complete =====';
PRINT 'Calc_Config_ID: ' + CAST(@WithholdCalcID AS NVARCHAR(10));
PRINT 'Total Source Cells: 1';
PRINT 'Total Dest Cells (Header): ~16';
PRINT 'Total Dest Cells (Detail): ~20';
PRINT '';
GO
