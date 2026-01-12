# Model Dimension Manager CDC Dashboard Enhancement

## Overview
This enhancement adds comprehensive file upload and advanced column mapping capabilities to the CDC (Change Data Capture) Dashboard in the Model Dimension Manager maintenance unit.

## Problem Statement
The original CDC Dashboard only supported external database connections for source data. This enhancement adds:
1. File upload support (CSV format)
2. Flexible column mapping for dimension member fields and text properties
3. Scenario/Time-based filtering for text property updates
4. Performance optimization for large-scale operations

## Solution Architecture

### Components

#### 1. Database Schema (`scripts/MDM_CDC_Config_Schema_Enhancement.sql`)
Enhanced MDM_CDC_Config table with:
- `Src_Type`: 'External' or 'File'
- `File_Path`: Path to uploaded CSV file
- `Column_Mappings`: JSON mapping for member fields
- `Text_Property_Mappings`: JSON mapping for text properties
- `ScenarioType_Field`: Source column for scenario filtering
- `Time_Field`: Source column for time filtering

New MDM_CDC_Log table for operation tracking and audit trail.

#### 2. CDC Processing Engine (`MDM_CDC_Process.cs`)
Core processing engine with:
- **Dual Source Support**: Read from external DB or CSV file
- **Batch Processing**: Process 100 records per batch for optimal performance
- **CSV Parser**: Handle quoted values and complex CSV structures
- **Bulk Operations**: Use staging tables and SqlBulkCopy
- **Transaction Safety**: Full ACID compliance with rollback

Key Methods:
- `Process_CDC_Data()`: Main processing orchestrator
- `Read_External_Source_Data()`: Fetch data from external database
- `Read_File_Source_Data()`: Parse CSV files
- `Bulk_Process_Member_Updates()`: Batch update dimension members

#### 3. Enhanced CDC Config Adapter (`MDM_CDC_Config.cs`)
Updated SQL adapter supporting new schema columns in INSERT/UPDATE operations.

#### 4. Dashboard String Functions (`MDM_CDC_StringFunctions.cs`)
Business rules exposing CDC functionality to dashboards:
- `ProcessCDCData`: Execute CDC process for a configuration
- `UploadFile`: Handle file uploads to OneStream file system
- `ValidateColumnMapping`: Validate JSON mapping syntax
- `GetSourceDataPreview`: Preview first N rows from source

### Data Flow

```
External DB or CSV File
        ↓
Read_Source_Data() - with 5min timeout, optimized queries
        ↓
Column Mapping - JSON-based flexible mapping
        ↓
Batch Processing - 100 records per batch
        ↓
Staging Tables - temporary bulk insert
        ↓
Dimension Updates - through OneStream APIs
        ↓
MDM_CDC_Log - operation tracking
```

## Configuration

### External Database Source
```
Name: My CDC Config
Src_Type: External
Src_Connection: Server=myserver;Database=mydb;Integrated Security=true;
Src_SQL_String: SELECT MemberID, MemberDesc, ParentID, Text1Val FROM DimMembers
Column_Mappings: {"MemberName":"MemberID","MemberDescription":"MemberDesc","Parent":"ParentID"}
Text_Property_Mappings: {"Text1":"Text1Val"}
```

### File Upload Source
```
Name: My File CDC Config
Src_Type: File
File_Path: CDC_Uploads/members.csv
Column_Mappings: {"MemberName":"Member_ID","MemberDescription":"Description"}
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

## Installation

### 1. Database Migration
Execute the SQL migration script:
```sql
-- Run in OneStream application database
scripts/MDM_CDC_Config_Schema_Enhancement.sql
```

### 2. Import Updated Assemblies
Using OneStream Code Utility for VS Code:
1. Import `MDM_Config_UI_Assembly` with updated files
2. Verify compilation in OneStream
3. Test with sample configuration

### 3. Configure CDC Settings
1. Navigate to Model Dimension Manager → CDC Setup
2. Create new configuration or edit existing
3. Select source type (External or File)
4. Configure column mappings in JSON format
5. Test with preview function

## Performance Optimization

### Batch Processing
- Default: 100 records per batch
- Adjust based on:
  - Available memory
  - Transaction log size
  - Network latency
  - Data volume

### Query Optimization
- 5-minute timeout for external queries
- Use indexed columns in SQL queries
- Minimize data transfer with SELECT specific columns
- Consider partitioning for very large datasets

### Staging Tables
- Temporary tables created per batch
- Auto-cleaned on transaction completion
- Uses GUID for unique naming to avoid conflicts

### Logging
- Indexed on (CDC_Config_ID, Processed_Date)
- Regular cleanup recommended for production
- Consider archiving old logs

## Error Handling

- **Transaction Rollback**: All errors trigger automatic rollback
- **Comprehensive Logging**: Every operation logged in MDM_CDC_Log
- **Detailed Messages**: Error messages returned to dashboard
- **Validation**: JSON mapping validated before processing

## Testing

### Unit Test Scenarios
1. External database connection
2. CSV file parsing (with quoted values)
3. Column mapping validation
4. Batch processing with various sizes
5. Transaction rollback on errors
6. Scenario/Time filtering

### Integration Test Scenarios
1. End-to-end CDC process
2. Large dataset processing (10,000+ records)
3. File upload through dashboard
4. Preview function
5. Log table population

## Security Considerations

- **Connection Strings**: Store encrypted in OneStream
- **File Uploads**: Validate file types and sizes
- **SQL Injection**: All queries parameterized
- **Access Control**: Use OneStream dashboard security
- **Audit Trail**: All operations logged with user info

## Troubleshooting

### Common Issues

**"File Not Found"**
- Verify file path in configuration
- Check OneStream file system permissions
- Ensure file was uploaded successfully

**"Column Not Found"**
- Validate column mappings JSON syntax
- Preview source data to verify column names
- Check for case sensitivity in mappings

**"External Connection Failed"**
- Test connection string independently
- Verify SQL Server access from OneStream server
- Check firewall rules and network connectivity

**"Performance Issues"**
- Reduce batch size
- Add indexes to source tables
- Optimize SQL queries
- Check network bandwidth

## Future Enhancements

1. **Additional File Formats**: Excel, XML, JSON
2. **Scheduling**: Automated CDC runs
3. **Delta Detection**: Incremental updates only
4. **Conflict Resolution**: Handle concurrent updates
5. **Real-time Validation**: Live preview as you type
6. **Export Templates**: Download mapping configuration templates
7. **Dry Run Mode**: Preview changes before committing

## Support

For questions, issues, or enhancement requests:
- Review documentation in `/docs/CDC_Dashboard_Enhancement_Guide.md`
- Check OneStream community forums
- Contact OneStream support
- Submit issues to repository

## License

Copyright © 2024. All rights reserved.

## Authors

- GitHub Copilot (AI Assistant)
- JFlakk (Repository Owner)

## Version History

- **v1.0.0** (2024-01-10): Initial release
  - Dual source support (External DB + File)
  - Column mapping with JSON
  - Scenario/Time filtering
  - Performance optimization with batch processing
  - Comprehensive error handling and logging
