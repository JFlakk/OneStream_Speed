# CDC Dashboard Enhancement - Implementation Guide

## Overview
The Model Dimension Manager CDC (Change Data Capture) Dashboard has been enhanced to support both external database sources and file uploads for dimension member updates, with advanced column mapping and scenario/time-based filtering capabilities.

## New Features

### 1. Dual Source Support
- **External Database**: Connect to any SQL Server database with custom queries
- **File Upload**: Upload CSV files directly through the OneStream dashboard

### 2. Column Mapping
- Flexible JSON-based mapping of source columns to dimension member fields
- Supported mappings:
  - `MemberName` (required): Source column for member name
  - `MemberDescription`: Source column for member description
  - `Parent`: Source column for parent member
  - Text properties: `Text1`, `Text2`, `Text3`, etc.

### 3. Scenario and Time Filtering
- Map source columns containing ScenarioType
- Map source columns containing Time Period
- Text properties can be filtered based on these values

### 4. Performance Optimization
- Batch processing with configurable batch size (default: 100 records)
- Bulk insert operations using staging tables
- Transaction-based operations with rollback support
- Optimized for large datasets (5-minute timeout for queries)

## Configuration Example

### External Database Source
```
Src_Type: External
Src_Connection: Server=myserver;Database=mydb;Integrated Security=true;
Src_SQL_String: SELECT MemberID, MemberDesc, ParentID, Text1Val, ScenType, Period FROM DimMembers
Column_Mappings: {"MemberName":"MemberID","MemberDescription":"MemberDesc","Parent":"ParentID"}
Text_Property_Mappings: {"Text1":"Text1Val"}
ScenarioType_Field: ScenType
Time_Field: Period
```

### File Upload Source
```
Src_Type: File
File_Path: CDC_Uploads/members_2024.csv
Column_Mappings: {"MemberName":"Member_ID","MemberDescription":"Description","Parent":"Parent_Member"}
Text_Property_Mappings: {"Text1":"Custom_Field1","Text2":"Custom_Field2"}
ScenarioType_Field: Scenario
Time_Field: Time_Period
```

### CSV File Format
```csv
Member_ID,Description,Parent_Member,Custom_Field1,Custom_Field2,Scenario,Time_Period
M001,Member One,ROOT,Value1,Value2,Actual,2024M01
M002,Member Two,M001,Value3,Value4,Actual,2024M01
M003,Member Three,M001,Value5,Value6,Budget,2024M02
```

## Support

For questions or issues, contact the OneStream development team or refer to the OneStream documentation at https://onestream.com.
