-- =============================================
-- FMM Calculation Configuration System
-- Sample Data for Testing and Demonstration
-- =============================================
-- This script provides sample data for the FMM calculation tables
-- demonstrating all supported calculation types
-- =============================================

USE [YourDatabaseName]
GO

-- =============================================
-- Example 1: Cube Calculation
-- Purpose: Calculate revenue allocation across entities
-- Type: Cube (CalcType = 2)
-- =============================================

-- Insert calculation configuration
INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Condition, Calc_Explanation, Status,
    MultiDim_Alloc, BR_Calc, Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Revenue Allocation', 10, 2,
    'Entity <> None', 'Allocates revenue across entities based on headcount',
    'Active', 0, 0, 'admin', 'admin'
);

DECLARE @CubeCalcID INT = SCOPE_IDENTITY();

-- Insert destination cell
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Cons, View, Acct, Flow, IC, Origin,
    UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,
    Time_Filter, Acct_Filter, Create_User, Update_User
)
VALUES (
    1, 1, 1, @CubeCalcID,
    'C#Consol', 'V#Periodic', 'A#Revenue', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'U3#None', 'U4#None', 'U5#None', 'U6#None', 'U7#None', 'U8#None',
    'T#2024M01:T#2024M12', 'A#Revenue.Base', 'admin', 'admin'
);

-- Insert source cells (multiple sources for allocation calculation)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, Math_Operator,
    Entity, Cons, Scenario, Time, View, Acct, Flow, IC, Origin,
    UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8,
    Create_User, Update_User
)
VALUES 
-- Source 1: Total revenue at parent level
(
    1, 1, 1, @CubeCalcID, 1,
    'Cube', 'Total Revenue', '',
    'E#Corp', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Revenue', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'U3#None', 'U4#None', 'U5#None', 'U6#None', 'U7#None', 'U8#None',
    'admin', 'admin'
),
-- Source 2: Allocation driver (headcount)
(
    1, 1, 1, @CubeCalcID, 2,
    'Cube', 'Headcount', '*',
    'E#POV', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Headcount', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'U3#None', 'U4#None', 'U5#None', 'U6#None', 'U7#None', 'U8#None',
    'admin', 'admin'
),
-- Source 3: Total headcount for percentage calculation
(
    1, 1, 1, @CubeCalcID, 3,
    'Cube', 'Total Headcount', '/',
    'E#Corp', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Headcount', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'U3#None', 'U4#None', 'U5#None', 'U6#None', 'U7#None', 'U8#None',
    'admin', 'admin'
);

-- =============================================
-- Example 2: Table Calculation
-- Purpose: Calculate derived columns in planning table
-- Type: Table (CalcType = 1)
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Table_Calc_SQL_Logic, Status,
    MbrList_Calc, MbrList_1_Calc, MbrList_1_DimType, MbrList_1_Dim, MbrList_1_Filter,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Budget Total Calculation', 20, 1,
    'Calculates total budget from monthly columns',
    'UPDATE Budget_Plan SET Total = Month1 + Month2 + Month3 + Month4 + Month5 + Month6 + Month7 + Month8 + Month9 + Month10 + Month11 + Month12',
    'Active',
    1, 1, 'Entity', 'Entity', 'E#Corp.Base',
    'admin', 'admin'
);

DECLARE @TableCalcID INT = SCOPE_IDENTITY();

-- Insert destination (table-based destination)
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Location, SQL_Logic,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @TableCalcID,
    'Budget_Plan.Total', 'SUM(Month1, Month2, Month3, Month4, Month5, Month6, Month7, Month8, Month9, Month10, Month11, Month12)',
    'admin', 'admin'
);

-- Insert source cells (table columns)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, Math_Operator,
    Table_Calc_Expression,
    Create_User, Update_User
)
VALUES 
(
    1, 1, 1, @TableCalcID, 1,
    'Table', 'Budget_Plan.Month1', '+',
    'SUM(Month1)',
    'admin', 'admin'
),
(
    1, 1, 1, @TableCalcID, 2,
    'Table', 'Budget_Plan.Month2', '+',
    'SUM(Month2)',
    'admin', 'admin'
);

-- =============================================
-- Example 3: Table to Cube Calculation
-- Purpose: Import budget data from table to cube
-- Type: BRTabletoCube (CalcType = 3)
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Status,
    Time_Phasing, Input_Frequency,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Budget Import to Cube', 30, 3,
    'Imports budget data from Budget_Plan table to cube',
    'Active',
    'Monthly', 'Monthly',
    'admin', 'admin'
);

DECLARE @TableToCubeID INT = SCOPE_IDENTITY();

-- Insert destination cell (cube)
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Cons, View, Acct, Flow, IC, Origin,
    UD1, UD2, Time_Filter,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @TableToCubeID,
    'C#Consol', 'V#Periodic', 'A#POV', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'T#2024M01:T#2024M12',
    'admin', 'admin'
);

-- Insert source cell (table)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, DB_Name,
    Table_Calc_Expression, Table_Filter_Expression,
    Map_Type, Map_Source, Map_Logic,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @TableToCubeID, 1,
    'Table', 'Budget_Plan', 'AppDB',
    'SELECT Entity, Account, Period, Amount FROM Budget_Plan WHERE Status = ''Approved''',
    'Status = ''Approved'' AND Period BETWEEN ''2024-01'' AND ''2024-12''',
    'Direct', 'Budget_Plan', 'Entity -> Entity, Account -> Acct, Period -> Time, Amount -> Value',
    'admin', 'admin'
);

-- =============================================
-- Example 4: Cube to Table Calculation
-- Purpose: Export actual data from cube to reporting table
-- Type: CubetoTable (CalcType = 5)
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Status,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Actuals Export to Table', 40, 5,
    'Exports actual data from cube to reporting table',
    'Active',
    'admin', 'admin'
);

DECLARE @CubeToTableID INT = SCOPE_IDENTITY();

-- Insert destination cell (table)
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Location, SQL_Logic,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @CubeToTableID,
    'Actuals_Report', 'INSERT INTO Actuals_Report (Entity, Account, Period, Amount)',
    'admin', 'admin'
);

-- Insert source cell (cube)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item,
    Entity, Cons, Scenario, Time, View, Acct, Flow, IC, Origin,
    UD1, UD2, UD3, UD4,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @CubeToTableID, 1,
    'Cube', 'Actuals',
    'E#[All]', 'C#Consol', 'S#Actual', 'T#2024M01:T#2024M12', 'V#Periodic', 'A#Revenue.Base', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'U3#None', 'U4#None',
    'admin', 'admin'
);

-- =============================================
-- Example 5: Consolidation Calculation
-- Purpose: Consolidate child entities to parent
-- Type: Consolidate (CalcType = 6)
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Balanced_Buffer, Status,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Entity Consolidation', 50, 6,
    'Consolidates child entities to parent entity',
    'Buffer for balanced consolidation',
    'Active',
    'admin', 'admin'
);

DECLARE @ConsolidateID INT = SCOPE_IDENTITY();

-- Insert destination cell (parent entity)
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Cons, View, Acct, Flow, IC, Origin,
    UD1, UD2, Time_Filter, Acct_Filter,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @ConsolidateID,
    'C#Consol', 'V#Periodic', 'A#[All]', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None', 'T#2024M01:T#2024M12', 'A#Revenue.Base',
    'admin', 'admin'
);

-- Insert source cell (child entities)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, Math_Operator,
    Entity, Cons, Scenario, Time, View, Acct, Flow, IC, Origin,
    UD1, UD2,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @ConsolidateID, 1,
    'Cube', 'Child Entities', '',
    'E#[ParentOf(E#POV)]', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#POV', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#None',
    'admin', 'admin'
);

-- =============================================
-- Example 6: Import Table to Cube with Mapping
-- Purpose: Import external data with dimension mapping
-- Type: ImportTabletoCube (CalcType = 4)
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Status,
    Time_Phasing, Input_Frequency,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'External Data Import', 60, 4,
    'Imports external forecast data with mapping',
    'Active',
    'Quarterly', 'Quarterly',
    'admin', 'admin'
);

DECLARE @ImportTableToCubeID INT = SCOPE_IDENTITY();

-- Insert destination cell (cube)
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Cons, View, Acct, Flow, IC, Origin,
    UD1, UD2, Time_Filter,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @ImportTableToCubeID,
    'C#Consol', 'V#Periodic', 'A#POV', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#None', 'U2#Forecast', 'T#2024Q1:T#2024Q4',
    'admin', 'admin'
);

-- Insert source cell (external table with mapping)
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, DB_Name,
    Table_Calc_Expression, Table_Filter_Expression,
    Map_Type, Map_Source, Map_Logic,
    Use_Temp_Table, Temp_Table_Name,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @ImportTableToCubeID, 1,
    'Table', 'External_Forecast', 'ExternalDB',
    'SELECT DISTINCT CompanyCode, GLAccount, FiscalPeriod, Amount FROM External_Forecast',
    'FiscalYear = 2024 AND DataSource = ''Planning''',
    'Transform', 'External_Forecast', 'CompanyCode -> Entity (via FMM_Entity_Map), GLAccount -> Acct (via FMM_Acct_Map), FiscalPeriod -> Time',
    1, '#TempForecast',
    'admin', 'admin'
);

-- =============================================
-- Example 7: Multi-Dimensional Allocation
-- Purpose: Complex allocation across multiple dimensions
-- Type: Cube (CalcType = 2) with MultiDim_Alloc = 1
-- =============================================

INSERT INTO dbo.FMM_Calc_Config (
    Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
    Calc_Explanation, Status,
    MultiDim_Alloc, Time_Phasing,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, 'Multi-Dim Overhead Allocation', 70, 2,
    'Allocates overhead costs across Entity and Department dimensions',
    'Active',
    1, 'Monthly',
    'admin', 'admin'
);

DECLARE @MultiDimAllocID INT = SCOPE_IDENTITY();

-- Insert destination cell
INSERT INTO dbo.FMM_Dest_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID,
    Cons, View, Acct, Flow, IC, Origin,
    UD1, UD2, Time_Filter, Acct_Filter,
    Create_User, Update_User
)
VALUES (
    1, 1, 1, @MultiDimAllocID,
    'C#Consol', 'V#Periodic', 'A#Overhead_Allocated', 'F#EndBalance', 'I#None', 'O#Calc',
    'U1#POV', 'U2#None', 'T#2024M01:T#2024M12', 'A#Overhead_Allocated',
    'admin', 'admin'
);

-- Insert source cells for multi-dimensional allocation
INSERT INTO dbo.FMM_Src_Cell (
    Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
    Src_Type, Src_Item, Math_Operator,
    Entity, Cons, Scenario, Time, View, Acct, Flow, IC, Origin,
    UD1, UD2,
    Create_User, Update_User
)
VALUES 
-- Total overhead to allocate
(
    1, 1, 1, @MultiDimAllocID, 1,
    'Cube', 'Total Overhead', '',
    'E#Corp', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Overhead_Total', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#Corporate', 'U2#None',
    'admin', 'admin'
),
-- Driver 1: Department headcount
(
    1, 1, 1, @MultiDimAllocID, 2,
    'Cube', 'Dept Headcount', '*',
    'E#POV', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Headcount', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#POV', 'U2#None',
    'admin', 'admin'
),
-- Driver 2: Total headcount
(
    1, 1, 1, @MultiDimAllocID, 3,
    'Cube', 'Total Headcount', '/',
    'E#Corp', 'C#Consol', 'S#Actual', 'T#POV', 'V#Periodic', 'A#Headcount', 'F#EndBalance', 'I#None', 'O#Import',
    'U1#[All]', 'U2#None',
    'admin', 'admin'
);

PRINT 'Sample data inserted successfully for all calculation types:';
PRINT '  1. Cube Calculation (Revenue Allocation)';
PRINT '  2. Table Calculation (Budget Total)';
PRINT '  3. Table to Cube (Budget Import)';
PRINT '  4. Cube to Table (Actuals Export)';
PRINT '  5. Consolidation (Entity Consolidation)';
PRINT '  6. Import Table to Cube (External Data Import)';
PRINT '  7. Multi-Dimensional Allocation (Overhead Allocation)';
GO

-- =============================================
-- Query Examples to Verify Data
-- =============================================

-- View all calculations
SELECT * FROM dbo.vw_FMM_Calc_Complete ORDER BY Sequence;

-- View calculations by type
SELECT * FROM dbo.vw_FMM_Calc_Summary_By_Type;

-- View source cells for a specific calculation
SELECT * FROM dbo.vw_FMM_Src_Cell_Details WHERE Calc_Name = 'Revenue Allocation' ORDER BY Src_Order;

-- View destination cells
SELECT * FROM dbo.vw_FMM_Dest_Cell_Details;
