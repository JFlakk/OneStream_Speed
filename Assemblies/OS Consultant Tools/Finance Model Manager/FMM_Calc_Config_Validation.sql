-- =============================================
-- FMM Calculation Configuration System
-- Validation and Testing Script
-- =============================================
-- This script validates the schema deployment and tests basic functionality
-- Run this after deploying FMM_Calc_Config_Schema.sql
-- =============================================

SET NOCOUNT ON;
GO

PRINT '=======================================================';
PRINT 'FMM Calculation Configuration System Validation';
PRINT '=======================================================';
PRINT '';

-- =============================================
-- Test 1: Verify Tables Exist
-- =============================================
PRINT 'Test 1: Verifying table existence...';

DECLARE @TableCount INT = 0;

SELECT @TableCount = COUNT(*)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('FMM_Calc_Config', 'FMM_Dest_Cell', 'FMM_Src_Cell')
AND TABLE_TYPE = 'BASE TABLE';

IF @TableCount = 3
    PRINT '  ✓ All 3 tables exist';
ELSE
    PRINT '  ✗ ERROR: Expected 3 tables, found ' + CAST(@TableCount AS NVARCHAR(10));

PRINT '';

-- =============================================
-- Test 2: Verify Indexes
-- =============================================
PRINT 'Test 2: Verifying indexes...';

DECLARE @IndexCount INT = 0;

SELECT @IndexCount = COUNT(DISTINCT i.name)
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE t.name IN ('FMM_Calc_Config', 'FMM_Dest_Cell', 'FMM_Src_Cell')
AND i.name IS NOT NULL
AND i.name NOT LIKE 'PK_%';  -- Exclude primary keys

IF @IndexCount >= 6
    PRINT '  ✓ Found ' + CAST(@IndexCount AS NVARCHAR(10)) + ' indexes';
ELSE
    PRINT '  ✗ WARNING: Expected at least 6 indexes, found ' + CAST(@IndexCount AS NVARCHAR(10));

PRINT '';

-- =============================================
-- Test 3: Verify Foreign Keys
-- =============================================
PRINT 'Test 3: Verifying foreign keys...';

DECLARE @FKCount INT = 0;

SELECT @FKCount = COUNT(*)
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS
WHERE CONSTRAINT_NAME IN ('FK_Dest_Cell_Calc', 'FK_Src_Cell_Calc');

IF @FKCount = 2
    PRINT '  ✓ All 2 foreign keys exist';
ELSE
    PRINT '  ✗ ERROR: Expected 2 foreign keys, found ' + CAST(@FKCount AS NVARCHAR(10));

PRINT '';

-- =============================================
-- Test 4: Verify Views
-- =============================================
PRINT 'Test 4: Verifying views...';

DECLARE @ViewCount INT = 0;

SELECT @ViewCount = COUNT(*)
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_NAME IN (
    'vw_FMM_Calc_Complete',
    'vw_FMM_Src_Cell_Details',
    'vw_FMM_Dest_Cell_Details',
    'vw_FMM_Calc_Summary_By_Type'
);

IF @ViewCount = 4
    PRINT '  ✓ All 4 views exist';
ELSE
    PRINT '  ✗ ERROR: Expected 4 views, found ' + CAST(@ViewCount AS NVARCHAR(10));

PRINT '';

-- =============================================
-- Test 5: Insert and Query Test Data
-- =============================================
PRINT 'Test 5: Testing basic CRUD operations...';

BEGIN TRY
    BEGIN TRANSACTION;

    -- Insert test calculation
    INSERT INTO FMM_Calc_Config (
        Cube_ID, Act_ID, Model_ID, Name, Sequence, Calc_Type,
        Calc_Explanation, Status, Create_User, Update_User
    )
    VALUES (
        999, 999, 999, 'Test Calculation', 1, 2,
        'Test calculation for validation', 'Build', 'ValidationScript', 'ValidationScript'
    );

    DECLARE @TestCalcID INT = SCOPE_IDENTITY();

    -- Insert test destination
    INSERT INTO FMM_Dest_Cell (
        Cube_ID, Act_ID, Model_ID, Calc_ID,
        Acct, View, Flow,
        Create_User, Update_User
    )
    VALUES (
        999, 999, 999, @TestCalcID,
        'A#Test', 'V#Test', 'F#Test',
        'ValidationScript', 'ValidationScript'
    );

    -- Insert test source
    INSERT INTO FMM_Src_Cell (
        Cube_ID, Act_ID, Model_ID, Calc_ID, Src_Order,
        Src_Type, Src_Item,
        Create_User, Update_User
    )
    VALUES (
        999, 999, 999, @TestCalcID, 1,
        'Cube', 'Test Source',
        'ValidationScript', 'ValidationScript'
    );

    -- Query test data
    DECLARE @QueryCount INT;
    
    SELECT @QueryCount = COUNT(*)
    FROM FMM_Calc_Config
    WHERE Calc_ID = @TestCalcID;

    IF @QueryCount = 1
    BEGIN
        PRINT '  ✓ Insert successful';
        
        -- Test cascading delete
        DELETE FROM FMM_Calc_Config WHERE Calc_ID = @TestCalcID;
        
        SELECT @QueryCount = COUNT(*)
        FROM FMM_Dest_Cell
        WHERE Calc_ID = @TestCalcID;
        
        IF @QueryCount = 0
            PRINT '  ✓ Cascading delete successful';
        ELSE
            PRINT '  ✗ ERROR: Cascading delete failed';
    END
    ELSE
    BEGIN
        PRINT '  ✗ ERROR: Insert verification failed';
    END

    ROLLBACK TRANSACTION;
    PRINT '  ✓ Transaction rolled back (test data cleaned up)';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT '  ✗ ERROR: ' + ERROR_MESSAGE();
END CATCH

PRINT '';

-- =============================================
-- Test 6: Verify Check Constraints
-- =============================================
PRINT 'Test 6: Verifying check constraints...';

DECLARE @ConstraintCount INT = 0;

SELECT @ConstraintCount = COUNT(*)
FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS
WHERE CONSTRAINT_NAME IN ('CHK_Calc_Type', 'CHK_Status');

IF @ConstraintCount >= 2
    PRINT '  ✓ Check constraints exist';
ELSE
    PRINT '  ✗ WARNING: Expected at least 2 check constraints, found ' + CAST(@ConstraintCount AS NVARCHAR(10));

PRINT '';

-- =============================================
-- Test 7: Column Validation
-- =============================================
PRINT 'Test 7: Verifying key columns...';

DECLARE @ColumnIssues INT = 0;

-- Check FMM_Calc_Config columns
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'FMM_Calc_Config' 
    AND COLUMN_NAME = 'Calc_Type'
)
BEGIN
    PRINT '  ✗ Missing column: FMM_Calc_Config.Calc_Type';
    SET @ColumnIssues = @ColumnIssues + 1;
END

-- Check for dimension columns in FMM_Dest_Cell
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'FMM_Dest_Cell' 
    AND COLUMN_NAME IN ('Cons', 'View', 'Acct', 'Flow', 'IC', 'Origin')
    HAVING COUNT(*) = 6
)
BEGIN
    PRINT '  ✗ Missing dimension columns in FMM_Dest_Cell';
    SET @ColumnIssues = @ColumnIssues + 1;
END

-- Check for UD columns in FMM_Src_Cell
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'FMM_Src_Cell' 
    AND COLUMN_NAME IN ('UD1', 'UD2', 'UD3', 'UD4', 'UD5', 'UD6', 'UD7', 'UD8')
    HAVING COUNT(*) = 8
)
BEGIN
    PRINT '  ✗ Missing UD columns in FMM_Src_Cell';
    SET @ColumnIssues = @ColumnIssues + 1;
END

IF @ColumnIssues = 0
    PRINT '  ✓ All key columns exist';
ELSE
    PRINT '  ✗ Found ' + CAST(@ColumnIssues AS NVARCHAR(10)) + ' column issues';

PRINT '';

-- =============================================
-- Test 8: View Functionality
-- =============================================
PRINT 'Test 8: Testing view queries...';

BEGIN TRY
    -- Test each view
    DECLARE @ViewQueryCount INT;
    
    SELECT @ViewQueryCount = COUNT(*) FROM vw_FMM_Calc_Complete;
    SELECT @ViewQueryCount = COUNT(*) FROM vw_FMM_Src_Cell_Details;
    SELECT @ViewQueryCount = COUNT(*) FROM vw_FMM_Dest_Cell_Details;
    SELECT @ViewQueryCount = COUNT(*) FROM vw_FMM_Calc_Summary_By_Type;
    
    PRINT '  ✓ All views are queryable';
END TRY
BEGIN CATCH
    PRINT '  ✗ ERROR querying views: ' + ERROR_MESSAGE();
END CATCH

PRINT '';

-- =============================================
-- Summary
-- =============================================
PRINT '=======================================================';
PRINT 'Validation Complete';
PRINT '=======================================================';
PRINT '';
PRINT 'Schema validation completed. Review results above.';
PRINT '';
PRINT 'Next Steps:';
PRINT '  1. If all tests passed, deploy sample data (FMM_Calc_Config_Sample_Data.sql)';
PRINT '  2. Review documentation (FMM_CALC_CONFIG_README.md)';
PRINT '  3. Configure calculations based on your requirements';
PRINT '';

-- =============================================
-- Optional: Display Schema Summary
-- =============================================
PRINT 'Schema Summary:';
PRINT '===============';
PRINT '';

PRINT 'Tables:';
SELECT 
    '  - ' + TABLE_NAME + ' (' + CAST(
        (SELECT COUNT(*) 
         FROM INFORMATION_SCHEMA.COLUMNS c 
         WHERE c.TABLE_NAME = t.TABLE_NAME) AS NVARCHAR(10)
    ) + ' columns)' AS Summary
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_NAME IN ('FMM_Calc_Config', 'FMM_Dest_Cell', 'FMM_Src_Cell')
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Views:';
SELECT 
    '  - ' + TABLE_NAME AS Summary
FROM INFORMATION_SCHEMA.VIEWS
WHERE TABLE_NAME LIKE 'vw_FMM_%'
ORDER BY TABLE_NAME;

PRINT '';
PRINT 'Supported Calculation Types:';
PRINT '  1 = Table (table-based calculations)';
PRINT '  2 = Cube (cube-based calculations)';
PRINT '  3 = BRTabletoCube (Business Rule table to cube)';
PRINT '  4 = ImportTabletoCube (Import table to cube)';
PRINT '  5 = CubetoTable (Cube to table)';
PRINT '  6 = Consolidate (Consolidation calculations)';
PRINT '';

GO
