-- ============================================================
-- Data Validation Manager (DVM) Database Schema
-- ============================================================
-- Supports validations within either a Cube or Table context.
--
-- Table context: Compare values between rows/columns of a SQL
--   table or view, e.g.
--     row y column x  =  row a column y
--     row y column x  <  row a column y  +  5%
--
-- Cube context: Retrieve cell values via FDX queries and apply
--   the same comparison operators.
-- ============================================================

-- ============================================================
-- DVM_Config  –  Master configuration for a validation set
-- ============================================================
IF OBJECT_ID('DVM_Config', 'U') IS NULL
BEGIN
    CREATE TABLE DVM_Config
    (
        DVM_Config_ID   INT            IDENTITY(1,1) NOT NULL,
        Name            NVARCHAR(100)  NOT NULL,
        Description     NVARCHAR(500)  NULL,

        -- 'Table' or 'Cube'
        Context_Type    NVARCHAR(10)   NOT NULL
            CONSTRAINT CK_DVM_Config_Context_Type
            CHECK (Context_Type IN ('Table', 'Cube')),

        -- Table context
        Table_Schema    NVARCHAR(50)   NULL,        -- defaults to dbo when NULL
        Table_Name      NVARCHAR(200)  NULL,        -- SQL table or view to query
        Table_Filter    NVARCHAR(1000) NULL,        -- optional WHERE clause fragment applied to base query

        -- Cube context
        Cube_View_Name  NVARCHAR(200)  NULL,        -- OneStream cube view name
        FDX_Base_Query  NVARCHAR(MAX)  NULL,        -- base FDX query / POV for the validation

        Is_Active       BIT            NOT NULL DEFAULT 1,

        Create_Date     DATETIME       NOT NULL DEFAULT GETDATE(),
        Create_User     NVARCHAR(50)   NOT NULL DEFAULT SYSTEM_USER,
        Update_Date     DATETIME       NULL,
        Update_User     NVARCHAR(50)   NULL,

        CONSTRAINT PK_DVM_Config PRIMARY KEY CLUSTERED (DVM_Config_ID)
    );
END;
GO

-- Index: common lookups by name and context type
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Config_Name' AND object_id = OBJECT_ID('DVM_Config'))
    CREATE NONCLUSTERED INDEX IX_DVM_Config_Name
        ON DVM_Config (Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Config_Context_Type' AND object_id = OBJECT_ID('DVM_Config'))
    CREATE NONCLUSTERED INDEX IX_DVM_Config_Context_Type
        ON DVM_Config (Context_Type, Is_Active);
GO

-- ============================================================
-- DVM_Rule  –  Individual validation rule within a config
-- ============================================================
IF OBJECT_ID('DVM_Rule', 'U') IS NULL
BEGIN
    CREATE TABLE DVM_Rule
    (
        DVM_Rule_ID     INT            IDENTITY(1,1) NOT NULL,
        DVM_Config_ID   INT            NOT NULL,
        Rule_Name       NVARCHAR(100)  NOT NULL,
        Description     NVARCHAR(500)  NULL,

        -- Comparison operator applied between source and target values:
        --   Equality | LessThan | LessThanOrEqual | GreaterThan |
        --   GreaterThanOrEqual | NotEqual | PercentVariance | LessThanWithPct | GreaterThanWithPct
        Rule_Type       NVARCHAR(30)   NOT NULL,

        -- 'Error', 'Warning', 'Info'
        Severity        NVARCHAR(20)   NOT NULL DEFAULT 'Error',

        -- --------------------------------------------------------
        -- Table-context columns
        --   Source = left-hand operand  (row y column x)
        --   Target = right-hand operand (row a column y)
        -- --------------------------------------------------------
        Src_Row_Filter  NVARCHAR(500)  NULL,   -- WHERE predicate to identify the source row(s)
        Src_Column      NVARCHAR(100)  NULL,   -- numeric column whose value is the source
        Tgt_Row_Filter  NVARCHAR(500)  NULL,   -- WHERE predicate to identify the target row(s)
        Tgt_Column      NVARCHAR(100)  NULL,   -- numeric column whose value is the target

        -- --------------------------------------------------------
        -- Cube-context columns
        -- --------------------------------------------------------
        Src_FDX         NVARCHAR(MAX)  NULL,   -- FDX expression for the source cell
        Tgt_FDX         NVARCHAR(MAX)  NULL,   -- FDX expression for the target cell

        -- --------------------------------------------------------
        -- Tolerance – used by PercentVariance, LessThanWithPct,
        --   and GreaterThanWithPct rule types.
        --   Represents a percentage value, e.g. 5 = 5%.
        --   Tolerance_Pct = 0 degrades to the non-tolerance variant (e.g., GreaterThanWithPct
        --     with 0% is equivalent to a plain GreaterThan check).
        --   For GreaterThanWithPct the value must be in [0, 100).  A value >= 100 makes the
        --     right-hand side 0 or negative and is rejected by the engine at runtime.
        --   For PercentVariance, when tgt = 0 the rule passes only if src = 0 as well
        --     (avoids division-by-zero; a non-zero src against a zero tgt always fails).
        -- --------------------------------------------------------
        Tolerance_Pct   DECIMAL(18,4)  NULL
            CONSTRAINT CK_DVM_Rule_Tolerance_Pct CHECK (Tolerance_Pct IS NULL OR (Tolerance_Pct >= 0 AND Tolerance_Pct < 100)),

        Sort_Order      INT            NOT NULL DEFAULT 1,
        Is_Active       BIT            NOT NULL DEFAULT 1,

        Create_Date     DATETIME       NOT NULL DEFAULT GETDATE(),
        Create_User     NVARCHAR(50)   NOT NULL DEFAULT SYSTEM_USER,
        Update_Date     DATETIME       NULL,
        Update_User     NVARCHAR(50)   NULL,

        CONSTRAINT PK_DVM_Rule PRIMARY KEY CLUSTERED (DVM_Rule_ID),
        CONSTRAINT FK_DVM_Rule_DVM_Config
            FOREIGN KEY (DVM_Config_ID) REFERENCES DVM_Config (DVM_Config_ID)
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Rule_Config' AND object_id = OBJECT_ID('DVM_Rule'))
    CREATE NONCLUSTERED INDEX IX_DVM_Rule_Config
        ON DVM_Rule (DVM_Config_ID, Sort_Order);
GO

-- ============================================================
-- DVM_Run  –  Metadata about a single validation execution
-- ============================================================
IF OBJECT_ID('DVM_Run', 'U') IS NULL
BEGIN
    CREATE TABLE DVM_Run
    (
        DVM_Run_ID          INT            IDENTITY(1,1) NOT NULL,
        DVM_Config_ID       INT            NOT NULL,

        Run_Date            DATETIME       NOT NULL DEFAULT GETDATE(),
        Run_User            NVARCHAR(50)   NOT NULL DEFAULT SYSTEM_USER,

        -- 'InProgress', 'Completed', 'Failed', 'Cancelled'
        Status              NVARCHAR(20)   NOT NULL DEFAULT 'InProgress',

        Total_Rules         INT            NULL,
        Total_Pass          INT            NULL,
        Total_Fail          INT            NULL,
        Total_Warning       INT            NULL,
        Execution_Time_Ms   INT            NULL,
        Error_Message       NVARCHAR(MAX)  NULL,

        CONSTRAINT PK_DVM_Run PRIMARY KEY CLUSTERED (DVM_Run_ID),
        CONSTRAINT FK_DVM_Run_DVM_Config
            FOREIGN KEY (DVM_Config_ID) REFERENCES DVM_Config (DVM_Config_ID)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Run_Config_Date' AND object_id = OBJECT_ID('DVM_Run'))
    CREATE NONCLUSTERED INDEX IX_DVM_Run_Config_Date
        ON DVM_Run (DVM_Config_ID, Run_Date DESC);
GO

-- ============================================================
-- DVM_Result  –  Per-rule result for a validation run
-- ============================================================
IF OBJECT_ID('DVM_Result', 'U') IS NULL
BEGIN
    CREATE TABLE DVM_Result
    (
        DVM_Result_ID       INT            IDENTITY(1,1) NOT NULL,
        DVM_Run_ID          INT            NOT NULL,
        DVM_Rule_ID         INT            NOT NULL,
        Rule_Name           NVARCHAR(100)  NULL,

        -- 'Pass', 'Fail', 'Warning', 'Error'
        Status              NVARCHAR(20)   NOT NULL,

        -- Actual values retrieved during the run (stored as NVARCHAR
        --   to accommodate both numeric and text cells)
        Src_Value           NVARCHAR(200)  NULL,
        Tgt_Value           NVARCHAR(200)  NULL,

        Expected_Operator   NVARCHAR(10)   NULL,   -- =, <, <=, >, >=, !=, <%
        Tolerance_Pct       DECIMAL(18,4)  NULL,

        -- Human-readable description of why the rule passed or failed
        Message             NVARCHAR(1000) NULL,

        -- For Table context: identifies the row combination evaluated
        Row_Context         NVARCHAR(500)  NULL,

        CONSTRAINT PK_DVM_Result PRIMARY KEY CLUSTERED (DVM_Result_ID),
        CONSTRAINT FK_DVM_Result_DVM_Run
            FOREIGN KEY (DVM_Run_ID) REFERENCES DVM_Run (DVM_Run_ID)
            ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Result_Run' AND object_id = OBJECT_ID('DVM_Result'))
    CREATE NONCLUSTERED INDEX IX_DVM_Result_Run
        ON DVM_Result (DVM_Run_ID, Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DVM_Result_Rule' AND object_id = OBJECT_ID('DVM_Result'))
    CREATE NONCLUSTERED INDEX IX_DVM_Result_Rule
        ON DVM_Result (DVM_Rule_ID);
GO

-- ============================================================
-- Reference data – Rule Type descriptions
-- ============================================================
/*
Rule_Type values and their meaning:

  Equality          src  =   tgt
  NotEqual          src  !=  tgt
  LessThan          src  <   tgt
  LessThanOrEqual   src  <=  tgt
  GreaterThan       src  >   tgt
  GreaterThanOrEqual src >= tgt
  PercentVariance   |src - tgt| / tgt  <=  Tolerance_Pct / 100
                    Note: when tgt = 0 the division is undefined.  The engine treats
                    this as pass only if src = 0 as well.  A non-zero src against a
                    zero tgt always fails.
  LessThanWithPct   src  <   tgt * (1 + Tolerance_Pct / 100)
  GreaterThanWithPct src >   tgt * (1 - Tolerance_Pct / 100)

Examples
--------
-- Table: row y column x  =  row a column y
INSERT INTO DVM_Rule (DVM_Config_ID, Rule_Name, Rule_Type,
                      Src_Row_Filter, Src_Column,
                      Tgt_Row_Filter, Tgt_Column)
VALUES (1, 'Revenue check', 'Equality',
        'Account = ''Revenue'' AND Period = ''Jan''', 'Amount',
        'Account = ''NetRevenue'' AND Period = ''Jan''', 'Amount');

-- Table: row y column x  <  row a column y  +  5%
INSERT INTO DVM_Rule (DVM_Config_ID, Rule_Name, Rule_Type,
                      Src_Row_Filter, Src_Column,
                      Tgt_Row_Filter, Tgt_Column,
                      Tolerance_Pct)
VALUES (1, 'COGS ceiling check', 'LessThanWithPct',
        'Account = ''COGS'' AND Period = ''Jan''', 'Amount',
        'Account = ''Revenue'' AND Period = ''Jan''', 'Amount',
        5.00);

-- Cube: compare two FDX cell values
INSERT INTO DVM_Rule (DVM_Config_ID, Rule_Name, Rule_Type,
                      Src_FDX, Tgt_FDX)
VALUES (2, 'Budget vs Actual', 'LessThan',
        'S#Actual:T#2024M1:E#Total:A#NetIncome',
        'S#Budget:T#2024M1:E#Total:A#NetIncome');
*/
