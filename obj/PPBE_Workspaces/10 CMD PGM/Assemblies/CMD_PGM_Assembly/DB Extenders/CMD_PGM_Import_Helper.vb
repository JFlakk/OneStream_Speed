Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.Data.SqlClient


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_PGM_Import_Helper
	Public Class MainClass
		
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
            ' Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args
			
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Load Dashboard logic here.
							
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
								End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						#Region                 "Import REQ"
						'This makes sure there is only one import running at a time to make sure data is not overidden.
						'DEV NOTE: This may not be necessary with the new approach of adding the user into the loading tables
						If args.FunctionName.XFEqualsIgnoreCase("ImportREQ") Then
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								Me.Check_WF_Complete_Lock(si)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
									Me.ImportREQ(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region 

						
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Dashboard Component Selection Changed logic here.
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							selectionChangedTaskResult.Message = ""
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							Return selectionChangedTaskResult
						End If
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement SQL Table Editor Save Data logic here.
							'Save the data rows.
							'Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							'Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName)
								'dbConn.BeginTrans()
								'BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, True, False, False)
								'dbConn.CommitTrans()
							'End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False 'Note: Use True if we already saved the data rows in this Business Rule.
							Return saveDataTaskResult
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		

#Region "REQ Mass Import"
		'This rule reads the imported file chcks if it is readable then parses into the REQ class
		'*****FILL OUT MORE

		

		Public Function	ImportREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							
			Dim REQ As New CMD_PGM_Requirement()
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.GetWFInfoDetails(si)
			
			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName

			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfCube).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)

			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath")
			Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
			If objXFFileInfo Is Nothing
				'Me.REQMassImportFileHelper(si, globals, api, args, "File " & objXFFileInfo.Name & " does NOT exist", "FAIL", objXFFileInfo.Name)
				Throw New XFUserMsgException(si, New Exception(String.Concat("File " & objXFFileInfo.Name & " does NOT exist")))
			End If
			Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)			
			Dim sFileText As String = system.Text.Encoding.ASCII.GetString(objXFFileEX.XFFile.ContentFileBytes)   
			Dim fullFile As New StringReader(sFileText)
			
	        Dim validFile As Boolean = True

			'Create DataTables to be used for importing and writing to the two relational tables
	        Dim fullDataTable As New DataTable("CMD_PGM_Import")
			Dim REQDataTable As New DataTable("XFC_CMD_PGM_REQ")
			Dim REQDetailDataTable As New DataTable("XFC_CMD_PGM_REQ_Details")
						
			Try
	            Using reader As New TextFieldParser(fullFile)
	                reader.TextFieldType = FieldType.Delimited
	                reader.SetDelimiters(",")
	
	                ' Add a column for validation errors and REQ_ID
	                fullDataTable.Columns.Add("ValidationError")
					fullDataTable.Columns.Add("CMD_PGM_REQ_ID",GetType(Guid))
					fullDataTable.PrimaryKey = New DataColumn() {fullDataTable.Columns("CMD_PGM_REQ_ID")}
					fullDataTable.Columns.Add("REQ_ID")
					fullDataTable.Columns.Add("Status")
	                ' Assuming the first line contains headers
	                If Not reader.EndOfData Then
	                    Dim headers = reader.ReadFields()
	                    If headers.Length <> 92 Then
	                        Throw New InvalidDataException(objXFFileInfo.Name & " has invalid structure. Please check the file if its in the correct format. Expected number of columns is 92, number columns in file header is "& headers.Length & vbCrLf & headers.ToString )
	                    End If
						
						'Create the columns of the Full data table
	                    For Each header In headers
	                        fullDataTable.Columns.Add(header.Trim(),GetType(String))
	                    Next
						'Add audit columns for narrative
						fullDataTable.Columns.Add("Attach_File_Name",GetType(String))
						fullDataTable.Columns.Add("Attach_File_Bytes",GetType(String))
						fullDataTable.Columns.Add("Invalid",GetType(String))
						fullDataTable.Columns.Add("Create_Date", GetType(DateTime))
						fullDataTable.Columns.Add("Create_User",GetType(String))
						fullDataTable.Columns.Add("Update_Date", GetType(DateTime))
						fullDataTable.Columns.Add("Update_User",GetType(String))
						fullDataTable.Columns.Add("WFScenario_Name",GetType(String))
						fullDataTable.Columns.Add("WFCMD_Name",GetType(String))
						fullDataTable.Columns.Add("WFTime_Name",GetType(String))
						
						'Add columns for detail
						fullDataTable.Columns.Add("IC",GetType(String))
						fullDataTable.Columns.Add("Account",GetType(String))
						fullDataTable.Columns.Add("Flow",GetType(String))
						fullDataTable.Columns.Add("UD7",GetType(String))
						fullDataTable.Columns.Add("UD8",GetType(String))
						fullDataTable.Columns.Add("FY_Total",GetType(String))
						fullDataTable.Columns.Add("AllowUpdate",GetType(Boolean))
						
						fullDataTable.PrimaryKey = New DataColumn() {fullDataTable.Columns("CMD_PGM_REQ_ID")}
	                Else
	                    Throw New InvalidDataException("The CSV file is empty.")
	                End If
	                Dim validLine As New StringBuilder()
	                ' Read the file line by line
	                While Not reader.EndOfData
	                    Dim fields = reader.ReadFields()
	                    Dim line As String = String.Join(",", fields)
	                    If line.StartsWith(comd) Then
	                        ' Process the previous valid line if it exists
	                        If validLine.Length > 0 Then
	                            ProcessLine(validLine.ToString(), fullDataTable, validFile)
	                            validLine.Clear()
	                        End If
	                        ' Start a new valid line
	                        validLine.Append(line)
	                    Else
	                        ' Concatenate with the previous line
	                        validLine.Append(" " & line)
	                    End If
	                End While
	
	                ' Process the last valid line
	                If validLine.Length > 0 Then
	                    ProcessLine(validLine.ToString(), fullDataTable, validFile)
	                End If
				End Using
				'Write to session
				BRApi.Utilities.SetSessionDataTable(si,si.UserName,"CMD_PGM_Import",fullDataTable)
                ' Write the fullDataTable to the session if there are validation errors
                If validFile Then
					'Split fullDataTable and insert into the two tables
					Me.SplitAndInsertIntoREQTables(fullDataTable, REQDataTable,REQDetailDataTable)
					
                End If
	        Catch ex As Exception
	            Throw New Exception("An error occurred: " & ex.Message)
	        End Try
			'If the validation failed, write the error out.
			'If there are more than ten, we show only the first ten messages for the sake of redablity
			Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
			If Not validFile Then
				Dim sErrorLog As String = ""
								
				Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
				BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
				Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_IPGM_mport",  fullDataTable)
				Return Nothing
			End If
			
			'File load complete. Write file to explorer
			'Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
			Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf 
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
			Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_IPGM_mport",  fullDataTable)

		Return Nothing
		End Function			


#End Region

#Region "Import Helpers"

#Region "GetNextREQID"
	Dim startingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
	Function GetNextREQID (fundCenter As String) As String
		Dim currentREQID As Integer
		Dim newREQ_ID As String
'BRApi.ErrorLog.LogMessage(si,"Fund Center: " & fundCenter)		
		If startingREQ_IDByFC.TryGetValue(fundCenter, currentREQID) Then
			currentREQID = currentREQID + 1
			newREQ_ID =  "REQ_" & currentREQID.ToString("D5")	
			startingREQ_IDByFC(fundCenter) = currentREQID
		Else	
			newREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,fundCenter)
'BRApi.ErrorLog.LogMessage(si,"newREQ_ID: " & newREQ_ID)			
			startingREQ_IDByFC.Add(fundCenter, newREQ_ID.Split("_")(1))
		End If 
			
		Return newREQ_ID
	End Function
#End Region	

#Region "Process Line"
    Sub ProcessLine(line As String, ByRef dataTable As DataTable, ByRef validFile As Boolean)

        Dim cleanedLine As String = CleanUpSpecialCharacters(line)
		Dim values = ParseCsvLine(cleanedLine)
		Dim currREQ As New CMD_PGM_Requirement
		currREQ = Me.ParseREQ(si,values, dataTable)
		'If the FC is a parent, add _General
		currREQ.Entity = CMD_PGM_Utilities.CheckFor_General(si, currREQ.Entity)' & "_General"

		' Get APPN_FUND And PARENT APPN_L2 
		currREQ.APE9 = CMD_PGM_Utilities.GetUD3(si, currREQ.FundCode, currREQ.APE9)
'BRApi.ErrorLog.LogMessage(si, "entity: " & currREQ.Entity & ", APE: " & currREQ.APE9 & ", full line: " & line)		
        Dim newRow = dataTable.NewRow()

        ' Run validation (you can customize this part)
        If Me.ValidateMembers(si, currREQ) Then
            newRow("ValidationError") = ""
			Dim REQ_ID As String = Me.GetNextREQID(currREQ.Entity)' GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,currREQ.Entity)
			If Not String.IsNullOrWhiteSpace(REQ_ID) Then
				newRow("REQ_ID") = REQ_ID
				'Update Status
				Dim entityLevel As String = Me.GetEntityLevel(currREQ.Entity)
				Dim sREQWFStatus As String = entityLevel & " Working"
				newRow("Status") = sREQWFStatus
			Else 'Failed to get REQ_ID. File will be marked failed
				validFile = False
			End If
        Else
            validFile = False
            newRow("ValidationError") = "Validation error: " & currREQ.validationError
        End If
        For i As Integer = 0 To values.Length - 1
            newRow(i + 4) = values(i)
        Next
		newRow("FundCenter") = currREQ.Entity
		newRow("APE9") = currREQ.APE9
		'Update Audit fields
		newRow("Create_Date") = DateTime.Now
		newRow("Create_User") = si.UserName
		newRow("Update_Date") = DateTime.Now
		newRow("Update_User") = "SS"'si.UserName
		newRow("WFScenario_Name") =  ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		newRow("WFTime_Name") = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		newRow("WFCMD_Name") =  BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
		newRow("IC") = "None"
		newRow("Account") = "REQ_Requested_Amt"
		newRow("Flow") = "None"
		newRow("UD7") = "None"
		newRow("UD8") = "None"
		'newRow("FY_Total") = "None"
		'newRow("AlloUpdate") = "None"
		
        dataTable.Rows.Add(newRow)
    End Sub
#End Region	

#Region "Get Entity Level"	

	Public Function GetEntityLevel(sEntity As String) As String
		Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
		Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

		Dim level As String  = String.Empty
		Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
		If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
			level = entityText3.Substring(entityText3.Length -2, 2)
		End If
		
		Return level
		
	End Function
#End Region	

#Region "CleanUp Special Characters"	
	
    Function CleanUpSpecialCharacters(value As String) As String
        Dim allowedChars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,""@!#$%'()+=_.:<>?~-[]*^/" & vbCrLf & vbLf
        Dim result As New StringBuilder()

		'If there are back to back (escaped) double quotes, they will be replaced with single quotes.
		'This is done becasue "s are used as column separator in csv files and "s inside would be represented as escaped quotes ("")		
		value = value.Replace("""""", "'")

		'this handles ZWSP's that get brought in as "???" and ignored by the second pass as question marks are allowed
		value = value.Replace("???", " ")		
		
        For Each c As Char In value
            If allowedChars.Contains(c) Then
                result.Append(c)
			Else
				result.Append(" ")
            End If
        Next

        Return result.ToString()
    End Function
#End Region	

#Region "Parse Csv Line Format"	
	
    Function ParseCsvLine(line As String) As String()

		'Use reg expressions to split the csv.
		'The expression accounts for commas that are with in "" to treat them as data.
		Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
		Dim fields As String () = Regex.Split(line, pattern)
		Return fields
		
    End Function	
#End Region	

#Region "setup Split And Insert Into REQ Tables"	

	Function SplitAndInsertIntoREQTables(ByRef fullDT As DataTable, ByRef REQDT As DataTable, ByRef REQDTDetail As DataTable )

		Dim sqa As New SqlDataAdapter()
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.GetWFInfoDetails(si)
		Dim scName As String = wfInfoDetails("ScenarioName")
		Dim cmd As String = wfInfoDetails("CMDName")
		Dim tm As String = wfInfoDetails("TimeName")

       Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                sqlConn.Open()
                Dim sqaREQReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
                Dim SqlREQ As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

				Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                sqaREQReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
				REQDT.PrimaryKey = New DataColumn() {REQDT.Columns("CMD_PGM_REQ_ID")}

			'Prepare Detail	
			 Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
			 Dim SqlREQDetail As String = $"SELECT * 
									FROM XFC_CMD_PGM_REQ_Details
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

			 Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
                sqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, REQDTDetail, SqlREQDetail, sqlparamsREQDetails)
				'Define PKs to match the table
				REQDTDetail.PrimaryKey = New DataColumn() {
					 REQDTDetail.Columns("CMD_PGM_REQ_ID"),
					 REQDTDetail.Columns("Start_Year"),
			         REQDTDetail.Columns("Unit_of_Measure"),
					 REQDTDetail.Columns("Account")
					 }
				'REQDTDetail.PrimaryKey = primaryKeyColumns
				
'Brapi.ErrorLog.LogMessage(si,"here * 1")			 				
			Me.SplitFullDataTable(fullDT, REQDT, REQDTDetail)
'Brapi.ErrorLog.LogMessage(si,"here * 2")			 							
			sqaREQReader.Update_XFC_CMD_PGM_REQ(REQDT, sqa)
'Brapi.ErrorLog.LogMessage(si,"here * 3")			 							
			sqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(REQDTDetail,sqa)
'Brapi.ErrorLog.LogMessage(si,"here * 4")			 								

			End Using
		End Using
		
'BRApi.ErrorLog.LogMessage(si, "setupXFC_CMD_PGM_REQ_Details. 1" & detailDT.Columns(0).ColumnName)		
       
	End Function
#End Region

#Region"Split dataset"
	Public Function SplitFullDataTable (ByRef fullDT As DataTable, ByRef REQDT As DataTable, ByRef REQDTDetail As DataTable) As Object
		' Split and translate data per row
'BRApi.ErrorLog.LogMessage(si,"In Split* 1")		
		For Each row As DataRow In fullDT.Rows
			
'BRApi.ErrorLog.LogMessage(si,"In Spli`t 2* GUID: " & row("CMD_PGM_REQ_ID").ToString & ", start_year: " & row("Cycle").ToString &  " ,Unit Of measure: " & row("UNIT_OF_MEASURE").ToString & " ,Acc: " & row("Account").ToString )
		   
			' Handle REQDT translation and insertion/update
			Dim MappingForREQ As New Dictionary(Of String, String)
			MappingForREQ = GetMappingForREQ()
		    Dim existingRowREQDT As DataRow = REQDT.Rows.Find(row("CMD_PGM_REQ_ID"))
		    If existingRowREQDT IsNot Nothing Then
		        ' Update the existing row
		        For Each fullCol As String In MappingForREQ.Keys
		            existingRowREQDT(MappingForREQ(fullCol)) = row(fullCol)
		        Next
		    Else
		        ' Create a new row
		        Dim newRowREQDT As DataRow = REQDT.NewRow()
		        For Each fullCol As String In MappingForREQ.Keys
		            newRowREQDT(MappingForREQ(fullCol)) = row(fullCol)
			
		        Next
		        REQDT.Rows.Add(newRowREQDT)
'BRApi.ErrorLog.LogMessage(si,"In REQ GUID       : " & newRowREQDT("CMD_PGM_REQ_ID").ToString )						
		    End If
			
			
		    ' Handle REQDTDetail Base translation and insertion/update
			Dim keyValues As Object() = {row("CMD_PGM_REQ_ID"), row("Cycle"), row("UNIT_OF_MEASURE"), row("Account")}
			Dim MappingForREQDetails As New Dictionary(Of String, String)
			MappingForREQDetails = GetMappingForREQDetailsBase()
		    Dim existingRowREQDTDetail As DataRow = REQDTDetail.Rows.Find(keyValues)
		    If existingRowREQDTDetail IsNot Nothing Then
		        ' Update the existing row
		        For Each fullCol As String In MappingForREQDetails.Keys
		            existingRowREQDTDetail(MappingForREQDetails(fullCol)) = row(fullCol)
		        Next
		    Else
		        ' Create a new row
		        Dim newRowREQDTDetail As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetails.Keys
		            newRowREQDTDetail(MappingForREQDetails(fullCol)) = row(fullCol)
		        Next
				'Map funding to the regular FYDEP
'BRApi.ErrorLog.LogMessage(si,"In REQ Detail Main GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & row("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
				
				newRowREQDTDetail("UNIT_OF_MEASURE") = "Funding"
				REQDTDetail.Rows.Add(newRowREQDTDetail)
		    End If
			
			' Handle REQDTDetail Items translation and insertion/update
			Dim MappingForREQDetailsItems As New Dictionary(Of String, String)
			MappingForREQDetailsItems = GetMappingForREQDetailsItems()
			If  CheckItems(row) Then
			    Dim existingRowREQDTDetailItem As DataRow = REQDTDetail.Rows.Find(keyValues)
			    If existingRowREQDTDetailItem IsNot Nothing Then
			        ' Update the existing row
			        For Each fullCol As String In MappingForREQDetailsItems.Keys
			            existingRowREQDTDetailItem(MappingForREQDetailsItems(fullCol)) = row(fullCol)
			        Next
			    Else
			        ' Create a new row
			        Dim newRowREQDTDetail As DataRow = REQDTDetail.NewRow()
			        For Each fullCol As String In MappingForREQDetailsItems.Keys
			            newRowREQDTDetail(MappingForREQDetailsItems(fullCol)) = row(fullCol)
			        Next
'BRApi.ErrorLog.LogMessage(si,"In REQ Detail item GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & newRowREQDTDetail("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
					
					newRowREQDTDetail("Account") = row("UNIT_OF_MEASURE")
				    REQDTDetail.Rows.Add(newRowREQDTDetail)
			    End If
				
			End If
			
			' Handle REQDTDetail Cosy translation and insertion/update
			Dim MappingForREQDetailsCost As New Dictionary(Of String, String)
			MappingForREQDetailsCost = GetMappingForREQDetailsCost()
			If  CheckItems(row) Then
			    Dim existingRowREQDTDetailCost As DataRow = REQDTDetail.Rows.Find(keyValues)
			    If existingRowREQDTDetailCost IsNot Nothing Then
			        ' Update the existing row
			        For Each fullCol As String In MappingForREQDetailsCost.Keys
			            existingRowREQDTDetailCost(MappingForREQDetailsCost(fullCol)) = row(fullCol)
			        Next
			    Else
			        ' Create a new row
			        Dim newRowREQDTDetail As DataRow = REQDTDetail.NewRow()
			        For Each fullCol As String In MappingForREQDetailsCost.Keys
			            newRowREQDTDetail(MappingForREQDetailsCost(fullCol)) = row(fullCol)
			        Next
'BRApi.ErrorLog.LogMessage(si,"In REQ Detail item GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & newRowREQDTDetail("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
					
					newRowREQDTDetail("Account") = row("UNIT_OF_MEASURE") & "_Cost"
				    REQDTDetail.Rows.Add(newRowREQDTDetail)
			    End If
				
			End If			
			
			
		Next
'BRApi.ErrorLog.LogMessage(si,"In Split* 7")

	End Function
#End Region

#Region	"Get Mapping XFC_CMD_PGM_REQ_Details Base"
	Public Function GetMappingForREQDetailsBase() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},		
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Account","Account"},
				{"Flow","Flow"},
				{"APPN","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"Ctype","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Start_Year"},
				{"FY1","FY_1"},
				{"FY2","FY_2"},
				{"FY3","FY_3"},
				{"FY4","FY_4"},
				{"FY5","FY_5"},
				{"FY_Total","FY_Total"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
#End Region	

#Region	"Get Mapping XFC_CMD_PGM_REQ_Details Items"
	Public Function GetMappingForREQDetailsItems() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},		
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Flow","Flow"},
				{"APPN","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"Ctype","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Start_Year"},
				{"FY1_Items","FY_1"},
				{"FY2_Items","FY_2"},
				{"FY3_Items","FY_3"},
				{"FY4_Items","FY_4"},
				{"FY5_Items","FY_5"},
				{"FY_Total","FY_Total"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
#End Region	

#Region	"Get Mapping XFC_CMD_PGM_REQ_Details Cost"
	Public Function GetMappingForREQDetailsCost() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},		
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Flow","Flow"},
				{"APPN","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"Ctype","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Start_Year"},
				{"FY1_Unit_Cost","FY_1"},
				{"FY2_Unit_Cost","FY_2"},
				{"FY3_Unit_Cost","FY_3"},
				{"FY4_Unit_Cost","FY_4"},
				{"FY5_Unit_Cost","FY_5"},
				{"FY_Total","FY_Total"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
#End Region	

#Region	"Get Mapping XFC_CMD_PGM_REQ"
	Public Function GetMappingForREQ() As Object
		Dim REQColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"REQ_ID", "REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},
				{"Title","Title"},
				{"Description","Description"},
				{"Justification","Justification"},
				{"FundCenter","Entity"},
				{"APPN","APPN"},
				{"MDEP","MDEP"},
				{"APE9","APE9"},
				{"DollarType","Dollar_Type"},
				{"ObjectClass","Obj_Class"},
				{"Ctype","CType"},
				{"UIC","UIC"},
				{"CostMethodology","Cost_Methodology"},
				{"ImpactIfNotFunded","Impact_Not_Funded"},
				{"RiskIfNotFunded","Risk_Not_Funded"},
				{"CostGrowthJustification","Cost_Growth_Justification"},
				{"MustFund","Must_Fund"},
				{"FundingSource","Funding_Src"},
				{"ArmyInitiative_Directive","Army_Init_Dir"},
				{"CommandInitiative_Directive","CMD_Init_Dir"},
				{"Activity_Exercise","Activity_Exercise"},
				{"Directorate","Directorate"},
				{"Division","Div"},
				{"Branch","Branch"},
				{"IT_CyberRequirement","IT_Cyber_REQ"},
				{"EmergingRequirement_ER","Emerging_REQ"},
				{"CPATopic","CPA_Topic"},
				{"PBRSubmission","PBR_Submission"},
				{"UPLSubmission","UPL_Submission"},
				{"ContractNumber","Contract_Num"},
				{"TaskOrderNumber","Task_Order_Num"},
				{"AwardTargetDate","Award_Target_Date"},
				{"POPExpirationDate","POP_Exp_Date"},
				{"ContractManYearEquiv_CME","Contractor_ManYear_Equiv_CME"},
				{"COREmail","COR_Email"},
				{"POCEmail","POC_Email"},
				{"ReviewingPOCEmail","Review_POC_Email"},
				{"MDEPFunctionalEmail","MDEP_Functional_Email"},
				{"NotificationEmailList","Notification_List_Emails"},
				{"GeneralComments_Notes","Gen_Comments_Notes"},
				{"JUON","JUON"},
				{"ISR_Flag","ISR_Flag"},
				{"Cost_Model","Cost_Model"},
				{"Combat_Lost","Combat_Loss"},
				{"Cost_Location","Cost_Location"},
				{"Category_A_Code","Cat_A_Code"},
				{"CBS_Code","CBS_Code"},
				{"MIP_Proj_Code","MIP_Proj_Code"},
				{"SS_Priority","SS_Priority"},
				{"Commitment_Group","Commit_Group"},
				{"SS_Capability","SS_Cap"},
				{"Strategic_BIN","Strategic_BIN"},
				{"LIN","LIN"},
				{"RequirementType","REQ_Type"},
				{"DD_Priority","DD_Priority"},
				{"Portfolio","Portfolio"},
				{"DD_Capability","DD_Cap"},
				{"JNT_CAP_AREA","JNT_Cap_Area"},
				{"TBM_COST_POOL","TBM_Cost_Pool"},
				{"TBM_TOWER","TBM_Tower"},
				{"APMSAITRNum","APMS_AITR_Num"},
				{"ZERO_TRUST_CAPABILITY","Zero_Trust_Cap"},
				{"ASSOCIATED_DIRECTIVES","Assoc_Directives"},
				{"CLOUD_INDICATOR","Cloud_IND"},
				{"STRAT_CYBERSEC_PGRM","Strat_Cyber_Sec_PGM"},
				{"NOTES","Notes"},
				{"FLEX1","FF_1"},
				{"FLEX2","FF_2"},
				{"FLEX3","FF_3"},
				{"FLEX4","FF_4"},
				{"FLEX5","FF_5"},
				{"Attach_File_Name","Attach_File_Name"},
				{"Attach_File_Bytes","Attach_File_Bytes"},
				{"Status","Status"},
				{"Invalid","Invalid"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		Return REQColMapping
	End Function

#End Region

#Region "Check Items"
	Public Function CheckItems(row As DataRow) As Boolean
		If ((Not IsDbNull(row("FY1_Items")) AndAlso Not String.IsNullOrEmpty(row("FY1_Items").ToString())) Or
			(Not IsDbNull(row("FY2_Items")) AndAlso Not String.IsNullOrEmpty(row("FY2_Items").ToString())) Or
			(Not IsDbNull(row("FY3_Items")) AndAlso Not String.IsNullOrEmpty(row("FY3_Items").ToString())) Or
			(Not IsDbNull(row("FY4_Items")) AndAlso Not String.IsNullOrEmpty(row("FY4_Items").ToString())) Or
			(Not IsDbNull(row("FY5_Items")) AndAlso Not String.IsNullOrEmpty(row("FY5_Items").ToString())))
'BRApi.ErrorLog.LogMessage(si,"True")			
			Return True
		Else
'BRApi.ErrorLog.LogMessage(si,"False")			
			Return False
		End If
	End Function
#End Region

#Region "Parse REQ"
		'Parse a line into REQ object and return
		Public Function	ParseREQ(ByVal si As SessionInfo, ByVal fields As String(), ByRef dt As DataTable)As Object
			'The parsed fileds are stored in the class. If new column is introduced, it needs to be added to the REQ class object as well
			Dim currREQ As CMD_PGM_Requirement = New CMD_PGM_Requirement
			'Trim any unprintable character and surrounding quotes
			If BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0).XFEqualsIgnoreCase("USAREUR") Then
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0) & "_AF"
			Else 
				currREQ.command = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name.Split(" ")(0)
			End If 
			currREQ.Entity = fields(dt.Columns("FundCenter").Ordinal - 4).Trim().Trim("""")
			currREQ.FundCode = fields(dt.Columns("APPN").Ordinal - 4).Trim().Trim("""") & "_General"
			currREQ.MDEP = fields(dt.Columns("MDEP").Ordinal - 4).Trim().Trim("""")

			currREQ.APE9 = fields(dt.Columns("APE9").Ordinal - 4).Trim().Trim("""")
			currREQ.DollarType = fields(dt.Columns("DollarType").Ordinal - 4).Trim().Trim("""")
			currREQ.CommitmentItem = fields(dt.Columns("ObjectClass").Ordinal - 4).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.CommitmentItem)
				currREQ.CommitmentItem = "None"
			End If 
			currREQ.sCtype = fields(dt.Columns("Ctype").Ordinal - 4).Trim().Trim("""")
			If String.IsNullOrWhiteSpace(currREQ.sCType)
				currREQ.sCType = "None"
			End If 
			currREQ.Cycle = fields(dt.Columns("Cycle").Ordinal - 4).Trim().Trim("""")
			currREQ.FY1 = fields(dt.Columns("FY1").Ordinal - 4).Trim().Trim("""")
			currREQ.FY2 = fields(dt.Columns("FY2").Ordinal - 4).Trim().Trim("""")
			currREQ.FY3 = fields(dt.Columns("FY3").Ordinal - 4).Trim().Trim("""")
			currREQ.FY4 = fields(dt.Columns("FY4").Ordinal - 4).Trim().Trim("""")
			currREQ.FY5 = fields(dt.Columns("FY5").Ordinal - 4).Trim().Trim("""")
			currREQ.Title = fields(dt.Columns("Title").Ordinal - 4).Trim().Trim("""")
			currREQ.Description = fields(dt.Columns("Description").Ordinal - 4).Trim().Trim("""")
			currREQ.Justification = fields(dt.Columns("Justification").Ordinal - 4).Trim().Trim("""")
			currREQ.CostMethodology = fields(dt.Columns("CostMethodology").Ordinal - 4).Trim().Trim("""")
			currREQ.ImpactifnotFunded = fields(dt.Columns("ImpactIfNotFunded").Ordinal - 4).Trim().Trim("""")
			currREQ.RiskifnotFunded = fields(dt.Columns("RiskIfNotFunded").Ordinal - 4).Trim().Trim("""")
			currREQ.CostGrowthJustification = fields(dt.Columns("CostGrowthJustification").Ordinal - 4).Trim().Trim("""")
			currREQ.MustFund = fields(dt.Columns("MustFund").Ordinal - 4).Trim().Trim("""")
			currREQ.FundingSource = fields(dt.Columns("FundingSource").Ordinal - 4).Trim().Trim("""")
			currREQ.ArmyInitiative_Directive = fields(dt.Columns("ArmyInitiative_Directive").Ordinal - 4).Trim().Trim("""")
			currREQ.CommandInitiative_Directive = fields(dt.Columns("CommandInitiative_Directive").Ordinal - 4).Trim().Trim("""")
			currREQ.Activity_Exercise = fields(dt.Columns("Activity_Exercise").Ordinal - 4).Trim().Trim("""")
			currREQ.IT_CyberRequirement = fields(dt.Columns("IT_CyberRequirement").Ordinal - 4).Trim().Trim("""")
			currREQ.UIC = fields(dt.Columns("UIC").Ordinal - 4).Trim().Trim("""")
			currREQ.FlexField1  = fields(dt.Columns("FLEX1").Ordinal - 4).Trim().Trim("""")
			currREQ.FlexField2  = fields(dt.Columns("FLEX2").Ordinal - 4).Trim().Trim("""")
			currREQ.FlexField3  = fields(dt.Columns("FLEX3").Ordinal - 4).Trim().Trim("""")
			currREQ.FlexField4  = fields(dt.Columns("FLEX4").Ordinal - 4).Trim().Trim("""")
			currREQ.FlexField5  = fields(dt.Columns("FLEX5").Ordinal - 4).Trim().Trim("""")
			currREQ.EmergingRequirement = fields(dt.Columns("EmergingRequirement_ER").Ordinal - 4).Trim().Trim("""")
			currREQ.CPATopic = fields(dt.Columns("CPATopic").Ordinal - 4).Trim().Trim("""")
			currREQ.PBRSubmission = fields(dt.Columns("PBRSubmission").Ordinal - 4).Trim().Trim("""")
			currREQ.UPLSubmission = fields(dt.Columns("UPLSubmission").Ordinal - 4).Trim().Trim("""")
			currREQ.ContractNumber = fields(dt.Columns("ContractNumber").Ordinal - 4).Trim().Trim("""")
			currREQ.TaskOrderNumber = fields(dt.Columns("TaskOrderNumber").Ordinal - 4).Trim().Trim("""")
			currREQ.AwardTargetDate = fields(dt.Columns("AwardTargetDate").Ordinal - 4).Trim().Trim("""")
			currREQ.POPExpirationDate = fields(dt.Columns("POPExpirationDate").Ordinal - 4).Trim().Trim("""")
			currREQ.ContractorManYearEquiv_CME = fields(dt.Columns("ContractManYearEquiv_CME").Ordinal - 4).Trim().Trim("""")
			currREQ.COREmail = fields(dt.Columns("COREmail").Ordinal - 4).Trim().Trim("""")
			currREQ.POCEmail = fields(dt.Columns("POCEmail").Ordinal - 4).Trim().Trim("""")
			currREQ.Directorate = fields(dt.Columns("Directorate").Ordinal - 4).Trim().Trim("""")
			currREQ.Division = fields(dt.Columns("Division").Ordinal - 4).Trim().Trim("""")
			currREQ.Branch = fields(dt.Columns("Branch").Ordinal - 4).Trim().Trim("""")
			currREQ.ReviewingPOCEmail = fields(dt.Columns("ReviewingPOCEmail").Ordinal - 4).Trim().Trim("""")
			currREQ.MDEPFunctionalEmail = fields(dt.Columns("MDEPFunctionalEmail").Ordinal - 4).Trim().Trim("""")
			currREQ.NotificationListEmails = fields(dt.Columns("NotificationEmailList").Ordinal - 4).Trim().Trim("""")
			currREQ.GeneralComments_Notes = fields(dt.Columns("GeneralComments_Notes").Ordinal - 4).Trim().Trim("""")
			currREQ.JUON = fields(dt.Columns("JUON").Ordinal - 4).Trim().Trim("""")
			currREQ.ISR_Flag = fields(dt.Columns("ISR_Flag").Ordinal - 4).Trim().Trim("""")
			currREQ.Cost_Model = fields(dt.Columns("Cost_Model").Ordinal - 4).Trim().Trim("""")
			currREQ.Combat_Loss = fields(dt.Columns("Combat_Lost").Ordinal - 4).Trim().Trim("""")
			currREQ.Cost_Location = fields(dt.Columns("Cost_Location").Ordinal - 4).Trim().Trim("""")
			currREQ.Category_A_Code = fields(dt.Columns("Category_A_Code").Ordinal - 4).Trim().Trim("""")
			currREQ.CBS_Code = fields(dt.Columns("CBS_Code").Ordinal - 4).Trim().Trim("""")
			currREQ.MIP_Proj_Code = fields(dt.Columns("MIP_Proj_Code").Ordinal - 4).Trim().Trim("""")
			currREQ.SS_Priority = fields(dt.Columns("SS_Priority").Ordinal - 4).Trim().Trim("""")
			currREQ.Commitment_Group = fields(dt.Columns("Commitment_Group").Ordinal - 4).Trim().Trim("""")
			currREQ.SS_Capability = fields(dt.Columns("SS_Capability").Ordinal - 4).Trim().Trim("""")
			currREQ.Strategic_BIN = fields(dt.Columns("Strategic_BIN").Ordinal - 4).Trim().Trim("""")
			currREQ.LIN = fields(dt.Columns("LIN").Ordinal - 4).Trim().Trim("""")
			currREQ.FY1_QTY = fields(dt.Columns("FY1_QTY").Ordinal - 4).Trim().Trim("""")
			currREQ.FY2_QTY = fields(dt.Columns("FY2_QTY").Ordinal - 4).Trim().Trim("""")
			currREQ.FY3_QTY = fields(dt.Columns("FY3_QTY").Ordinal - 4).Trim().Trim("""")
			currREQ.FY4_QTY = fields(dt.Columns("FY4_QTY").Ordinal - 4).Trim().Trim("""")
			currREQ.FY5_QTY = fields(dt.Columns("FY5_QTY").Ordinal - 4).Trim().Trim("""")
			currREQ.RequirementType = fields(dt.Columns("RequirementType").Ordinal - 4).Trim().Trim("""")
			currREQ.DD_Priority = fields(dt.Columns("DD_Priority").Ordinal - 4).Trim().Trim("""")
			currREQ.Portfolio = fields(dt.Columns("Portfolio").Ordinal - 4).Trim().Trim("""")
			currREQ.DD_Capability = fields(dt.Columns("DD_Capability").Ordinal - 4).Trim().Trim("""")
			currREQ.JNT_CAP_AREA = fields(dt.Columns("JNT_CAP_AREA").Ordinal - 4).Trim().Trim("""")
			currREQ.TBM_COST_POOL = fields(dt.Columns("TBM_COST_POOL").Ordinal - 4).Trim().Trim("""")
			currREQ.TBM_TOWER = fields(dt.Columns("TBM_TOWER").Ordinal - 4).Trim().Trim("""")
			currREQ.APMSAITRNum = fields(dt.Columns("APMSAITRNum").Ordinal - 4).Trim().Trim("""")
			currREQ.ZERO_TRUST_CAPABILITY = fields(dt.Columns("ZERO_TRUST_CAPABILITY").Ordinal - 4).Trim().Trim("""")
			currREQ.ASSOCIATED_DIRECTIVES = fields(dt.Columns("ASSOCIATED_DIRECTIVES").Ordinal - 4).Trim().Trim("""")
			currREQ.CLOUD_INDICATOR = fields(dt.Columns("CLOUD_INDICATOR").Ordinal - 4).Trim().Trim("""")
			currREQ.STRAT_CYBERSEC_PGRM = fields(dt.Columns("STRAT_CYBERSEC_PGRM").Ordinal - 4).Trim().Trim("""")
			currREQ.NOTES = fields(dt.Columns("NOTES").Ordinal - 4).Trim().Trim("""")
			currREQ.UNIT_OF_MEASURE = fields(dt.Columns("UNIT_OF_MEASURE").Ordinal - 4).Trim().Trim("""")
			currREQ.FY1_ITEMS = fields(dt.Columns("FY1_Items").Ordinal - 4).Trim().Trim("""")
			currREQ.FY1_UNIT_COST = fields(dt.Columns("FY1_Unit_Cost").Ordinal - 4).Trim().Trim("""")
			currREQ.FY2_ITEMS = fields(dt.Columns("FY2_Items").Ordinal - 4).Trim().Trim("""")
			currREQ.FY2_UNIT_COST = fields(dt.Columns("FY2_Unit_Cost").Ordinal - 4).Trim().Trim("""")
			currREQ.FY3_ITEMS = fields(dt.Columns("FY3_Items").Ordinal - 4).Trim().Trim("""")
			currREQ.FY3_UNIT_COST = fields(dt.Columns("FY3_Unit_Cost").Ordinal - 4).Trim().Trim("""")
			currREQ.FY4_ITEMS = fields(dt.Columns("FY4_Items").Ordinal - 4).Trim().Trim("""")
			currREQ.FY4_UNIT_COST = fields(dt.Columns("FY4_Unit_Cost").Ordinal - 4).Trim().Trim("""")
			currREQ.FY5_ITEMS = fields(dt.Columns("FY5_Items").Ordinal - 4).Trim().Trim("""")
			currREQ.FY5_UNIT_COST = fields(dt.Columns("FY5_Unit_Cost").Ordinal - 4).Trim().Trim("""")
			
			
			Return currREQ
			
		End Function
#End Region

#Region "Validate Members"		
		Public Function	ValidateMembers(ByVal si As SessionInfo, ByRef currREQ As CMD_PGM_Requirement) As Object
			
			'validate fund code
			'This code leverages the way we validate in Data Source
			'BRApi.Finance.Metadata.GetMember(si, dimTypeId.UD1, fundCode, includeProperties, dimDisplayOptions, memberDisplayOptions)
			
			Dim isFileValid As Boolean = True
			
			If String.IsNullOrWhiteSpace(currREQ.title) Then
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Blank Title value in record"
			End If
			
			'Validate that the Fund Center being loaded  s with in the command
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & currREQ.command)
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & currREQ.command & ".member.base", True)
			Dim currEntity As String = currREQ.Entity
			If Not membList.Exists(Function(x) x.Member.Name = currEntity) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U1#" & currREQ.fundCode & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Fund Code value: " & currREQ.fundCode
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Fund Code value: " & fundCode))
			End If
			
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U2#" & currREQ.MDEP & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid MDEPP value: " & currREQ.MDEP
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid MDEP value: " & MDEP))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U3#" & currREQ.APE9.Trim & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid APE value: " & currREQ.APE9
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid APE value: " & SAG_APE))
			End If
			
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U4#" &currREQ. DollarType & ".member.base", True)
			If (membList.Count <> 1 ) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: Invalid Dollar Type value: " & currREQ.DollarType
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			End If
			
			If Not String.IsNullOrWhiteSpace(currREQ.sCType) Or currREQ.sCType.XFEqualsIgnoreCase("None") Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U5_CTYPE")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U5#" &currREQ. sCType & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid CType value: " & currREQ.sCType
					
				End If
	
			End If 
			If Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem) Or currREQ.CommitmentItem.XFEqualsIgnoreCase("None")Then
				objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
				If (membList.Count <> 1 ) Then 
					isFileValid = False
					currREQ.valid = False
					currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
					
				End If
			End If 
			
			'Validate Numeric
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1)) And (Not IsNumeric(currREQ.FY1))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1 should be Numeric: " & currREQ.FY1
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2)) And (Not IsNumeric(currREQ.FY2))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2 should be Numeric: " & currREQ.FY2
				
			End If
			
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3)) And ( Not IsNumeric(currREQ.FY3))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3 should be Numeric: " & currREQ.FY3
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4)) And ( Not IsNumeric(currREQ.FY4))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4 should be Numeric: " & currREQ.FY4
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5)) And ( Not IsNumeric(currREQ.FY5))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5 should be Numeric: " & currREQ.FY5
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1_QTY)) And ( Not IsNumeric(currREQ.FY1_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1_QTY should be Numeric: " & currREQ.FY1_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2_QTY)) And ( Not IsNumeric(currREQ.FY2_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2_QTY should be Numeric: " & currREQ.FY2_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3_QTY)) And ( Not IsNumeric(currREQ.FY3_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3_QTY should be Numeric: " & currREQ.FY3_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4_QTY)) And ( Not IsNumeric(currREQ.FY4_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4_QTY should be Numeric: " & currREQ.FY4_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5_QTY)) And ( Not IsNumeric(currREQ.FY5_QTY))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5_QTY should be Numeric: " & currREQ.FY5_QTY
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1_Items)) And ( Not IsNumeric(currREQ.FY1_Items))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1_Items should be Numeric: " & currREQ.FY1_Items
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY1_Unit_Cost)) And ( Not IsNumeric(currREQ.FY1_Unit_Cost))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY1_Unit_Cost should be Numeric: " & currREQ.FY1_Unit_Cost
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2_Items)) And ( Not IsNumeric(currREQ.FY2_Items))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2_Items should be Numeric: " & currREQ.FY2_Items
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY2_Unit_Cost)) And ( Not IsNumeric(currREQ.FY2_Unit_Cost))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY2_Unit_Cost should be Numeric: " & currREQ.FY2_Unit_Cost
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3_Items)) And ( Not IsNumeric(currREQ.FY3_Items))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3_Items should be Numeric: " & currREQ.FY3_Items
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY3_Unit_Cost)) And ( Not IsNumeric(currREQ.FY3_Unit_Cost))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY3_Unit_Cost should be Numeric: " & currREQ.FY3_Unit_Cost
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4_Items)) And ( Not IsNumeric(currREQ.FY4_Items))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4_Items should be Numeric: " & currREQ.FY4_Items
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY4_Unit_Cost)) And ( Not IsNumeric(currREQ.FY4_Unit_Cost))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY4_Unit_Cost should be Numeric: " & currREQ.FY4_Unit_Cost
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5_Items)) And ( Not IsNumeric(currREQ.FY5_Items))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5_Items should be Numeric: " & currREQ.FY5_Items
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.FY5_Unit_Cost)) And ( Not IsNumeric(currREQ.FY5_Unit_Cost))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: FY5_Unit_Cost should be Numeric: " & currREQ.FY5_Unit_Cost
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.ContractorManYearEquiv_CME)) And ( Not IsNumeric(currREQ.ContractorManYearEquiv_CME))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: ContractorManYearEquiv_CME should be Numeric: " & currREQ.ContractorManYearEquiv_CME
				
			End If
			Dim validDate As DateTime
			If((Not String.IsNullOrWhiteSpace(currREQ.AwardTargetDate)) And ( Not DateTime.TryParse(currREQ.AwardTargetDate, validDate))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: AwardTargetDate is not a validate date: " & currREQ.AwardTargetDate
				
			End If
			If((Not String.IsNullOrWhiteSpace(currREQ.POPExpirationDate)) And ( Not DateTime.TryParse(currREQ.POPExpirationDate, validDate))) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: AwardTargetDate is not a validate date: " & currREQ.POPExpirationDate
				
			End If
			
			'We determine the scenario from the cycle
			'Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sScenario = "PGM_C" & currREQ.Cycle
			objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "S#" & sScenario & ".member.base", True)
			If (membList.Count <> 1) Then 
				isFileValid = False
				currREQ.valid = False
				currREQ.ValidationError = "Error: No valid Scenario for Cycle: " & currREQ.Cycle
				'Throw New XFUserMsgException(si, New Exception(filePath & " has invalid Dollar Type value: " & DollarType))
			Else
				currREQ.scenario = sScenario
			End If
			
			Return isFileValid
			
		End Function
#End Region

#Region "Check_WF_Complete_Lock"
		Public Function	Check_WF_Complete_Lock(ByVal si As SessionInfo) As Object							
			'--------------------------------------------------------------- 
			'Verify Workflow is NOT Complete Or Locked
			'---------------------------------------------------------------
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
				Throw New Exception("Notice: No updates are allowed. Workflow was marked ""Complete"" by Command HQ.")
			End If
			Return Nothing
		End Function
		
#End Region		
		
#End Region		

	End Class
End Namespace
