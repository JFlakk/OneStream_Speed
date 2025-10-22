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
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						Dim dbExt_ChangedResult As New XFSelectionChangedTaskResult()
						#Region                 "Import REQ"
						'This makes sure there is only one import running at a time to make sure data is not overidden.
						'DEV NOTE: This may not be necessary with the new approach of adding the user into the loading tables
						If args.FunctionName.XFEqualsIgnoreCase("ImportREQ") Then
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
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

			Try
				Dim REQ As New CMD_PGM_REQ()
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				
				Dim timeStart As DateTime = System.DateTime.Now
				Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
	
				Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
				Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
				
				Dim fileName As String = args.NameValuePairs.XFGetValue("FileName") 
	
				Dim FilePath As String = $"{BRApi.Utilities.GetFileShareFolder(si, FileshareFolderTypes.FileShareRoot,Nothing)}/{FileName}"
				'Confirm source file exists
				'Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath") 
	'			Dim fullFile = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.PrepImportFile(si,filePath)
		        
				Dim Importreq_DT As New DataTable("Importreqs")
				Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
					ImportREQ_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", REQ.ColumnMaps)
					Importreq_DT.TableName = "Importreqs"
					
				End Using
'BRApi.ErrorLog.LogMessage(si, "Returned to Import: " & ImportREQ_DT.Rows(0)("Invalid Errors").ToString)

				'Check for errors
				Dim validFile As Boolean = True
				Dim errRow As DataRow = Importreq_DT.AsEnumerable().
											FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
											
				If errRow IsNot Nothing Then validFile = False

				Dim REQDataTable As New DataTable("XFC_CMD_PGM_REQ")
				Dim REQDetailDataTable As New DataTable("XFC_CMD_PGM_REQ_Details")
				
	            If validFile Then
					'get req_id and guid
					UpdateColsForDatabase(Importreq_DT)
					PostProcessNewREQ(ImportREQ_DT)
					
'BRApi.ErrorLog.LogMessage(si, "Post proc completed " & Importreq_DT.TableName)					
					'Split fullDataTable and insert into the two tables
					Me.SplitAndInsertIntoREQTables(Importreq_DT, REQDataTable,REQDetailDataTable)
					
	            'End If
'BRApi.ErrorLog.LogMessage(si, "Post 1")				
					'write to cube
					Dim REQ_IDs As New List(Of String)
					For Each r As DataRow In ImportREQ_DT.Rows
						REQ_IDs.Add(r("REQ_ID").ToString)	
					Next
'BRApi.ErrorLog.LogMessage(si, "Post 2")				
					Dim loader As New CMD_PGM_Helper.MainClass
					Args.NameValuePairs.Add("req_IDs", String.Join(",", REQ_IDs))
					Args.NameValuePairs.Add("new_Status", "Formulate") 'It keeps the same status
					loader.main(si, globals, api, args)
					
					'File load complete. Write file to explorer
					'Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
					Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf 
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_PGM_Import_" & wfInfoDetails("ScenarioName") ,  ImportREQ_DT)
					
					Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
				Else 'If the validation failed, write the error out.
					Dim sErrorLog As String = ""
									
					Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_PGM_Import_" & wfInfoDetails("ScenarioName"),  ImportREQ_DT)
					Return Nothing
				End If
				

'BRApi.ErrorLog.LogMessage(si, "Import completed")
				Return Nothing
	        Catch ex As Exception
	            Throw New Exception("An error occurred: " & ex.Message)
	        End Try
		End Function			
#End Region

#Region "Import Helpers"

#Region "GetNextREQID"
	Dim startingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
	Function GetNextREQID (fundCenter As String) As String
		Dim currentREQID As Integer
		Dim newREQ_ID As String
		If startingREQ_IDByFC.TryGetValue(fundCenter, currentREQID) Then
'BRApi.ErrorLog.LogMessage(si,"IF Fund Center: " & fundCenter & ", currentREQID: " & currentREQID )			
			currentREQID = currentREQID + 1
			Dim modifiedFC As String = fundCenter
			modifiedFC = modifiedFC.Replace("_General", "")
			If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
			newREQ_ID =  modifiedFC &"_" & currentREQID.ToString("D5")
			startingREQ_IDByFC(fundCenter) = currentREQID
		Else	
			newREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,fundCenter)
'BRApi.ErrorLog.LogMessage(si,"ELSE Fund Center: " & fundCenter & ", newREQ_ID: " & newREQ_ID.Split("_")(1) )				
			startingREQ_IDByFC.Add(fundCenter.Trim, newREQ_ID.Split("_")(1))
		End If 
			
		Return newREQ_ID
	End Function
#End Region	

	Public Function PostProcessNewREQ(ByRef dt As DataTable) As StringReader
	    
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		For Each row As DataRow In dt.Rows
			
			Dim fundCenter As String = row("FundCenter").ToString
			'If 
			Dim REQ_D As String = GetNextREQID(fundCenter)
			row("REQ_ID") = REQ_D
			
			row("CMD_PGM_REQ_ID") = Guid.NewGuid()
			row("WFScenario_Name") =  wfInfoDetails("ScenarioName")
			row("WFTime_Name") =  wfInfoDetails("TimeName")
			row("WFCMD_Name") =  wfInfoDetails("CMDName")
			row("IC") = "None"
			row("Account") = "Req_Funding"
			row("Flow") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_PGM"
			row("Status") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_PGM"
			row("UD7") = "None"
			row("UD8") = "None"			
			row("FundCenter") = fundCenter
			row("APE9") =  row("APPN") & "_" & row("APE9") 'CMD_PGM_Utilities.GetUD3(si, row("APPN"), row("APE9")) '"OMA_122011000"
			row("REQ_ID_Type") = "Requirement"
			row("Create_Date") = DateTime.Now
			row("Create_User") = si.UserName
			row("Update_Date") = DateTime.Now
			row("Update_User") = si.UserName

		Next
       
        Return Nothing
	
	End Function	

	Public Function UpdateColsForDatabase(ByRef dt As DataTable) As StringReader
	    
		
		'Add columns in database but not in file records
		dt.Columns.Add("ValidationError")
		dt.Columns.Add("CMD_PGM_REQ_ID",GetType(Guid))
		'dt.PrimaryKey = New DataColumn() {dt.Columns("CMD_PGM_REQ_ID")}
		dt.Columns.Add("REQ_ID_Type")
		dt.Columns.Add("REQ_ID")
		dt.Columns.Add("Status")
        ' Assuming the first line contains headers

		'Add audit columns for narrative
		'table updates will delete once confirmed.
		'dt.Columns.Add("Attach_File_Name",GetType(String))
		'dt.Columns.Add("Attach_File_Bytes",GetType(String))
		dt.Columns.Add("Invalid",GetType(String))
		dt.Columns.Add("Create_Date", GetType(DateTime))
		dt.Columns.Add("Create_User",GetType(String))
		dt.Columns.Add("Update_Date", GetType(DateTime))
		dt.Columns.Add("Update_User",GetType(String))
		dt.Columns.Add("WFScenario_Name",GetType(String))
		dt.Columns.Add("WFCMD_Name",GetType(String))
		dt.Columns.Add("WFTime_Name",GetType(String))
		
		'Add columns for detail
		dt.Columns.Add("IC",GetType(String))
		dt.Columns.Add("Account",GetType(String))
		dt.Columns.Add("Flow",GetType(String))
		dt.Columns.Add("UD7",GetType(String))
		dt.Columns.Add("UD8",GetType(String))
		dt.Columns.Add("FY_Total",GetType(String))
		dt.Columns.Add("AllowUpdate",GetType(Boolean))
       
        Return Nothing
	
	End Function			
	
	Public Function GetExistingRow(ByRef dt As DataTable, ByRef account As String) As DataRow
'		Dim existingRow As System.Data.DataRow = ( From r As System.Data.DataRow In dt.AsEnumerable()
'									   Where r.Field(Of String)("CMD_PGM_REQ_ID").ToString().XFContainsIgnoreCase(row("CMD_PGM_REQ_ID").ToString) AndAlso
'											 r.Field(Of String)("Cycle").ToString().XFContainsIgnoreCase(row("Cycle").ToString) AndAlso
'											 r.Field(Of String)("UNIT_OF_MEASURE").ToString().XFContainsIgnoreCase(row("UNIT_OF_MEASURE").ToString) AndAlso
'											 r.Field(Of String)("Account").ToString().XFContainsIgnoreCase(account) 
'										Select r
'										).FirstOrDefault()
		For Each row In dt.Rows 
			If (row("CMD_PGM_REQ_ID").ToString().XFContainsIgnoreCase(row("CMD_PGM_REQ_ID").ToString) AndAlso
				row("Start_Year").ToString().XFContainsIgnoreCase(row("Start_Year").ToString) AndAlso
				row("UNIT_OF_MEASURE").ToString().XFContainsIgnoreCase(row("UNIT_OF_MEASURE").ToString) AndAlso
				row("Account").ToString().XFContainsIgnoreCase(account) )
				Return row
			End If
		Next
		Return Nothing
	End Function
		

#Region "Get Entity Level"	

'	Public Function GetEntityLevel(sEntity As String) As String
'		Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
'		Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'		Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

'		Dim level As String  = String.Empty
'		Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
'		If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
'			level = entityText3.Substring(entityText3.Length -2, 2)
'		End If
		
'		Return level
		
'	End Function
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
		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
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
'				REQDTDetail.PrimaryKey = New DataColumn() {
'					 REQDTDetail.Columns("CMD_PGM_REQ_ID"),
'					 REQDTDetail.Columns("Start_Year"),
'			         REQDTDetail.Columns("Unit_of_Measure"),
'					 REQDTDetail.Columns("Account")
'					 }
				'REQDTDetail.PrimaryKey = primaryKeyColumns
				
'Brapi.ErrorLog.LogMessage(si,"here * 1")			 				
				Me.SplitFullDataTable(fullDT, REQDT, REQDTDetail)
			
'Brapi.ErrorLog.LogMessage(si,"here * 2 ")	

'BRApi.ErrorLog.LogMessage(si, l)
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
			Dim REQExists As Boolean = False
			Dim existingREQ_ID As String
			Dim existingCMD_PGM_REQ_ID As String
			Dim MappingForREQ As New Dictionary(Of String, String)
			MappingForREQ = GetMappingForREQ()
		    Dim existingRowREQDT As DataRow = REQDT.Rows.Find(row("CMD_PGM_REQ_ID")) '***DEV NOTE: TO DO this will have to be modified to match on other fields
		    
			If existingRowREQDT IsNot Nothing Then
				REQExists = True
				existingCMD_PGM_REQ_ID = existingRowREQDT("CMD_PGM_REQ_ID")
				existingREQ_ID = existingRowREQDT("REQ_ID")
			End If
		      
		    ' Create a new row
	        Dim newRowREQDT As DataRow = REQDT.NewRow()
	        For Each fullCol As String In MappingForREQ.Keys
	            newRowREQDT(MappingForREQ(fullCol)) = row(fullCol)
		
	        Next
			'If it is an existing row we will keep the original REC_ID. All the data will be from the file
			If REQExists Then
				 Me.UpdateExistingREQIDs(newRowREQDT, existingCMD_PGM_REQ_ID, existingREQ_ID)					 
			End If
		    
			REQDT.Rows.Add(newRowREQDT)
'BRApi.ErrorLog.LogMessage(si,"In REQ GUID       : " & newRowREQDT("CMD_PGM_REQ_ID").ToString )						
			
			
		    ' Handle REQDTDetail Base FYDP
	        ' Create a new row
			Dim MappingForREQDetails As New Dictionary(Of String, String)
			MappingForREQDetails = GetMappingForREQDetailsBase()
	        Dim newRowREQDTDetail As DataRow = REQDTDetail.NewRow()
	        For Each fullCol As String In MappingForREQDetails.Keys
	            newRowREQDTDetail(MappingForREQDetails(fullCol)) = row(fullCol)
	        Next
			'Map funding to the regular FYDEP
'BRApi.ErrorLog.LogMessage(si,"In REQ Detail Main GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & row("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
			
			newRowREQDTDetail("UNIT_OF_MEASURE") = "Funding"
			If REQExists Then
				Me.UpdateExistingREQIDs(newRowREQDTDetail, existingCMD_PGM_REQ_ID, existingREQ_ID)	 				 
			End If
			REQDTDetail.Rows.Add(newRowREQDTDetail)
		   
			
			' Handle REQDTDetail Items 
			If  CheckItems(row) Then
				Dim MappingForREQDetailsItems As New Dictionary(Of String, String)
				MappingForREQDetailsItems = GetMappingForREQDetailsItem()
		        ' Create a new row
		        Dim newRowREQDTDetailItem As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetailsItems.Keys
		            newRowREQDTDetailItem(MappingForREQDetailsItems(fullCol)) = row(fullCol)
		        Next
	'BRApi.ErrorLog.LogMessage(si,"In REQ Detail item GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & newRowREQDTDetail("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
				
				newRowREQDTDetailItem("Account") = row("UNIT_OF_MEASURE")
			    If REQExists Then
					Me.UpdateExistingREQIDs(newRowREQDTDetailItem, existingCMD_PGM_REQ_ID, existingREQ_ID)	 
				End If
				REQDTDetail.Rows.Add(newRowREQDTDetailItem)
			End If
			
			' Handle REQDTDetail Cost 
			If  CheckUnitCost(row) Then
				Dim MappingForREQDetailsCost As New Dictionary(Of String, String)
				MappingForREQDetailsCost = GetMappingForREQDetailsCost() 
		        ' Create a new row
		        Dim newRowREQDTDetailCost As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetailsCost.Keys
		            newRowREQDTDetailCost(MappingForREQDetailsCost(fullCol)) = row(fullCol)
		        Next
	'BRApi.ErrorLog.LogMessage(si,"In REQ Detail item GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & newRowREQDTDetail("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
				
				newRowREQDTDetailCost("Account") = row("UNIT_OF_MEASURE") & "_Cost"
				
				 If REQExists Then
					Me.UpdateExistingREQIDs(newRowREQDTDetailCost, existingCMD_PGM_REQ_ID, existingREQ_ID)	 
				End If		
				REQDTDetail.Rows.Add(newRowREQDTDetailCost)
			End If
			
			' Handle REQDTDetail Quantity
			
			If  CheckQTY(row) Then
				Dim MappingForREQDetailsQTY As New Dictionary(Of String, String)
				MappingForREQDetailsQTY = GetMappingForREQDetailsQTY()
		        ' Create a new row
		        Dim newRowREQDTDetailQTY As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetailsQTY.Keys
		            newRowREQDTDetailQTY(MappingForREQDetailsQTY(fullCol)) = row(fullCol)
		        Next
'BRApi.ErrorLog.LogMessage(si,"In REQ Detail item GUID: " & newRowREQDTDetail("CMD_PGM_REQ_ID").ToString & ", start_year: " & newRowREQDTDetail("Start_Year").ToString &  " , Unit Of measure: " & newRowREQDTDetail("UNIT_OF_MEASURE").ToString & " , Acc: " & newRowREQDTDetail("Account").ToString )
					
				newRowREQDTDetailQTY("Account") = "Quantity"
				
				 If REQExists Then
					 Me.UpdateExistingREQIDs(newRowREQDTDetailQTY, existingCMD_PGM_REQ_ID, existingREQ_ID)
				End If			
				REQDTDetail.Rows.Add(newRowREQDTDetailQTY)
				
			End If						
			
			
		Next
'BRApi.ErrorLog.LogMessage(si,"In Split* 7")

	End Function
	
	Public Sub UpdateExistingREQIDs(ByRef dr As DataRow, ByVal existingCMD_PGM_REQ_ID As String, ByVal existingREQ_ID As String )
		dr("CMD_PGM_REQ_ID") = existingCMD_PGM_REQ_ID
		dr("REQ_ID") = existingREQ_ID
	End Sub
#End Region

#Region	"Get Mapping XFC_CMD_PGM_REQ_Details"
	Public Function GetMappingForREQDetailsBase() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
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
	
	Public Function GetMappingForREQDetailsItem() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
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

	Public Function GetMappingForREQDetailsCost() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
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
	
	Public Function GetMappingForREQDetailsQTY() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
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


#Region	"Get Mapping XFC_CMD_PGM_REQ"
	Public Function GetMappingForREQ() As Object
		Dim REQColMapping As New Dictionary(Of String, String) From{
				{"CMD_PGM_REQ_ID", "CMD_PGM_REQ_ID"},
				{"REQ_ID", "REQ_ID"},
				{"REQ_ID_Type", "REQ_ID_Type"},
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

#Region "Check Amount"
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
	
		Public Function CheckUnitCost(row As DataRow) As Boolean
		If ((Not IsDbNull(row("FY1_Unit_Cost")) AndAlso Not String.IsNullOrEmpty(row("FY1_Items").ToString())) Or
			(Not IsDbNull(row("FY2_Unit_Cost")) AndAlso Not String.IsNullOrEmpty(row("FY2_Items").ToString())) Or
			(Not IsDbNull(row("FY3_Unit_Cost")) AndAlso Not String.IsNullOrEmpty(row("FY3_Items").ToString())) Or
			(Not IsDbNull(row("FY4_Unit_Cost")) AndAlso Not String.IsNullOrEmpty(row("FY4_Items").ToString())) Or
			(Not IsDbNull(row("FY5_Unit_Cost")) AndAlso Not String.IsNullOrEmpty(row("FY5_Items").ToString())))
'BRApi.ErrorLog.LogMessage(si,"True")			
			Return True
		Else
'BRApi.ErrorLog.LogMessage(si,"False")			
			Return False
		End If
	End Function
	
		Public Function CheckQTY(row As DataRow) As Boolean
		If ((Not IsDbNull(row("FY1_QTY")) AndAlso Not String.IsNullOrEmpty(row("FY1_Items").ToString())) Or
			(Not IsDbNull(row("FY2_QTY")) AndAlso Not String.IsNullOrEmpty(row("FY2_Items").ToString())) Or
			(Not IsDbNull(row("FY3_QTY")) AndAlso Not String.IsNullOrEmpty(row("FY3_Items").ToString())) Or
			(Not IsDbNull(row("FY4_QTY")) AndAlso Not String.IsNullOrEmpty(row("FY4_Items").ToString())) Or
			(Not IsDbNull(row("FY5_QTY")) AndAlso Not String.IsNullOrEmpty(row("FY5_Items").ToString())))
'BRApi.ErrorLog.LogMessage(si,"True")			
			Return True
		Else
'BRApi.ErrorLog.LogMessage(si,"False")			
			Return False
		End If
	End Function
#End Region

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
