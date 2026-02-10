-- =============================================
-- Finance Model Manager (FMM) Database Tables
-- DDL Script for SQL Server
-- =============================================

-- =============================================
-- Table: RegPlan
-- Description: Master table for Regulatory Plans
-- =============================================
CREATE TABLE [dbo].[RegPlan] (
    [RegPlanID] UNIQUEIDENTIFIER NOT NULL,
    [WFScenarioName] NVARCHAR(100) NULL,
    [WFProfileName] NVARCHAR(100) NULL,
    [WFTimeName] NVARCHAR(100) NULL,
    [ActID] INT NULL,
    [Entity] NVARCHAR(100) NULL,
    [ApprLevelID] UNIQUEIDENTIFIER NULL,
    [RegID1] INT NULL,
    [RegID2] INT NULL,
    [RegID] NVARCHAR(100) NULL,
    
    -- Dynamic Text Attributes (1-20)
    [Attr1] NVARCHAR(100) NULL,
    [Attr2] NVARCHAR(100) NULL,
    [Attr3] NVARCHAR(100) NULL,
    [Attr4] NVARCHAR(100) NULL,
    [Attr5] NVARCHAR(100) NULL,
    [Attr6] NVARCHAR(100) NULL,
    [Attr7] NVARCHAR(100) NULL,
    [Attr8] NVARCHAR(100) NULL,
    [Attr9] NVARCHAR(100) NULL,
    [Attr10] NVARCHAR(100) NULL,
    [Attr11] NVARCHAR(100) NULL,
    [Attr12] NVARCHAR(100) NULL,
    [Attr13] NVARCHAR(100) NULL,
    [Attr14] NVARCHAR(100) NULL,
    [Attr15] NVARCHAR(100) NULL,
    [Attr16] NVARCHAR(100) NULL,
    [Attr17] NVARCHAR(100) NULL,
    [Attr18] NVARCHAR(100) NULL,
    [Attr19] NVARCHAR(100) NULL,
    [Attr20] NVARCHAR(100) NULL,
    
    -- Dynamic Decimal Attributes (1-12)
    [AttrVal1] DECIMAL(18, 2) NULL,
    [AttrVal2] DECIMAL(18, 2) NULL,
    [AttrVal3] DECIMAL(18, 2) NULL,
    [AttrVal4] DECIMAL(18, 2) NULL,
    [AttrVal5] DECIMAL(18, 2) NULL,
    [AttrVal6] DECIMAL(18, 2) NULL,
    [AttrVal7] DECIMAL(18, 2) NULL,
    [AttrVal8] DECIMAL(18, 2) NULL,
    [AttrVal9] DECIMAL(18, 2) NULL,
    [AttrVal10] DECIMAL(18, 2) NULL,
    [AttrVal11] DECIMAL(18, 2) NULL,
    [AttrVal12] DECIMAL(18, 2) NULL,
    
    -- Dynamic Date Attributes (1-5)
    [DateVal1] DATETIME NULL,
    [DateVal2] DATETIME NULL,
    [DateVal3] DATETIME NULL,
    [DateVal4] DATETIME NULL,
    [DateVal5] DATETIME NULL,
    
    -- Spread Configuration
    [SpreadAmount] DECIMAL(18, 2) NULL,
    [SpreadCurve] NVARCHAR(20) NULL,
    
    -- Status and Control
    [Status] NVARCHAR(100) NULL,
    [Invalid] BIT NULL,
    
    -- Audit Fields
    [CreateDate] DATETIME NULL,
    [CreateUser] NVARCHAR(50) NULL,
    [UpdateDate] DATETIME NULL,
    [UpdateUser] NVARCHAR(50) NULL,
    
    CONSTRAINT [PK_RegPlan] PRIMARY KEY CLUSTERED ([RegPlanID] ASC)
);
GO

-- =============================================
-- Table: RegPlan_Details
-- Description: Detail/Line Items for Regulatory Plans
-- =============================================
CREATE TABLE [dbo].[RegPlan_Details] (
    [RegPlanID] UNIQUEIDENTIFIER NOT NULL,
    [WFScenarioName] NVARCHAR(100) NULL,
    [WFProfileName] NVARCHAR(100) NULL,
    [WFTimeName] NVARCHAR(100) NULL,
    [ActID] INT NULL,
    [ModelID] INT NULL,
    [Entity] NVARCHAR(100) NULL,
    [ApprLevelID] UNIQUEIDENTIFIER NULL,
    
    -- Planning Data
    [PlanUnits] NVARCHAR(20) NOT NULL,
    [Account] NVARCHAR(20) NOT NULL,
    [Flow] NVARCHAR(100) NULL,
    
    -- User-Defined Fields (1-8)
    [UD1] NVARCHAR(100) NULL,
    [UD2] NVARCHAR(100) NULL,
    [UD3] NVARCHAR(100) NULL,
    [UD4] NVARCHAR(100) NULL,
    [UD5] NVARCHAR(100) NULL,
    [UD6] NVARCHAR(100) NULL,
    [UD7] NVARCHAR(100) NULL,
    [UD8] NVARCHAR(100) NULL,
    
    -- Time Dimension
    [Year] NVARCHAR(4) NOT NULL,
    
    -- Monthly Data (1-12)
    [Month1] DECIMAL(18, 2) NULL,
    [Month2] DECIMAL(18, 2) NULL,
    [Month3] DECIMAL(18, 2) NULL,
    [Month4] DECIMAL(18, 2) NULL,
    [Month5] DECIMAL(18, 2) NULL,
    [Month6] DECIMAL(18, 2) NULL,
    [Month7] DECIMAL(18, 2) NULL,
    [Month8] DECIMAL(18, 2) NULL,
    [Month9] DECIMAL(18, 2) NULL,
    [Month10] DECIMAL(18, 2) NULL,
    [Month11] DECIMAL(18, 2) NULL,
    [Month12] DECIMAL(18, 2) NULL,
    
    -- Quarterly Data (1-4)
    [Quarter1] DECIMAL(18, 2) NULL,
    [Quarter2] DECIMAL(18, 2) NULL,
    [Quarter3] DECIMAL(18, 2) NULL,
    [Quarter4] DECIMAL(18, 2) NULL,
    
    -- Yearly Data
    [Yearly] DECIMAL(18, 2) NULL,
    
    -- Control
    [AllowUpdate] BIT NULL,
    
    -- Audit Fields
    [CreateDate] DATETIME NULL,
    [CreateUser] NVARCHAR(50) NULL,
    [UpdateDate] DATETIME NULL,
    [UpdateUser] NVARCHAR(50) NULL,
    
    CONSTRAINT [PK_RegPlan_Details] PRIMARY KEY CLUSTERED (
        [RegPlanID] ASC,
        [Year] ASC,
        [PlanUnits] ASC,
        [Account] ASC
    )
);
GO

-- =============================================
-- Table: RegPlan_Audit
-- Description: Audit Trail for Regulatory Plans
-- =============================================
CREATE TABLE [dbo].[RegPlan_Audit] (
    [AuditID] INT NOT NULL IDENTITY(1,1),
    [RegisterPlanID] UNIQUEIDENTIFIER NOT NULL,
    [AuditDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [WFScenarioName] NVARCHAR(255) NULL,
    [ProjectID] VARCHAR(255) NULL,
    [Entity] NVARCHAR(255) NULL,
    [ResourceID] NVARCHAR(255) NULL,
    
    -- Spread Configuration
    [SpreadAmount] DECIMAL(18, 2) NULL,
    [SpreadCurve] NVARCHAR(255) NULL,
    
    -- Dynamic Text Attributes (1-20)
    [Attribute1] NVARCHAR(255) NULL,
    [Attribute2] NVARCHAR(255) NULL,
    [Attribute3] NVARCHAR(255) NULL,
    [Attribute4] NVARCHAR(255) NULL,
    [Attribute5] NVARCHAR(255) NULL,
    [Attribute6] NVARCHAR(255) NULL,
    [Attribute7] NVARCHAR(255) NULL,
    [Attribute8] NVARCHAR(255) NULL,
    [Attribute9] NVARCHAR(255) NULL,
    [Attribute10] NVARCHAR(255) NULL,
    [Attribute11] NVARCHAR(255) NULL,
    [Attribute12] NVARCHAR(255) NULL,
    [Attribute13] NVARCHAR(255) NULL,
    [Attribute14] NVARCHAR(255) NULL,
    [Attribute15] NVARCHAR(255) NULL,
    [Attribute16] NVARCHAR(255) NULL,
    [Attribute17] NVARCHAR(255) NULL,
    [Attribute18] NVARCHAR(255) NULL,
    [Attribute19] NVARCHAR(255) NULL,
    [Attribute20] NVARCHAR(255) NULL,
    
    -- Dynamic Decimal Attributes (1-12)
    [AttributeValue1] DECIMAL(18, 2) NULL,
    [AttributeValue2] DECIMAL(18, 2) NULL,
    [AttributeValue3] DECIMAL(18, 2) NULL,
    [AttributeValue4] DECIMAL(18, 2) NULL,
    [AttributeValue5] DECIMAL(18, 2) NULL,
    [AttributeValue6] DECIMAL(18, 2) NULL,
    [AttributeValue7] DECIMAL(18, 2) NULL,
    [AttributeValue8] DECIMAL(18, 2) NULL,
    [AttributeValue9] DECIMAL(18, 2) NULL,
    [AttributeValue10] DECIMAL(18, 2) NULL,
    [AttributeValue11] DECIMAL(18, 2) NULL,
    [AttributeValue12] DECIMAL(18, 2) NULL,
    
    -- Dynamic Date Attributes (1-5)
    [DateValue1] DATETIME NULL,
    [DateValue2] DATETIME NULL,
    [DateValue3] DATETIME NULL,
    [DateValue4] DATETIME NULL,
    [DateValue5] DATETIME NULL,
    
    CONSTRAINT [PK_RegPlan_Audit] PRIMARY KEY CLUSTERED ([AuditID] ASC)
);
GO

-- =============================================
-- Foreign Key Relationships
-- =============================================
ALTER TABLE [dbo].[RegPlan_Details]
ADD CONSTRAINT [FK_RegPlan_Details_RegPlan]
FOREIGN KEY ([RegPlanID])
REFERENCES [dbo].[RegPlan] ([RegPlanID])
ON DELETE CASCADE;
GO

-- =============================================
-- Indexes for Performance
-- =============================================
CREATE NONCLUSTERED INDEX [IX_RegPlan_WFScenario]
ON [dbo].[RegPlan] ([WFScenarioName], [WFProfileName], [WFTimeName]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Entity]
ON [dbo].[RegPlan] ([Entity]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Status]
ON [dbo].[RegPlan] ([Status]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Details_RegPlanID]
ON [dbo].[RegPlan_Details] ([RegPlanID]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Details_Year]
ON [dbo].[RegPlan_Details] ([Year]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Audit_RegisterPlanID]
ON [dbo].[RegPlan_Audit] ([RegisterPlanID]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Audit_AuditDate]
ON [dbo].[RegPlan_Audit] ([AuditDate]);
GO

CREATE NONCLUSTERED INDEX [IX_RegPlan_Audit_WFScenario]
ON [dbo].[RegPlan_Audit] ([WFScenarioName], [ProjectID]);
GO
