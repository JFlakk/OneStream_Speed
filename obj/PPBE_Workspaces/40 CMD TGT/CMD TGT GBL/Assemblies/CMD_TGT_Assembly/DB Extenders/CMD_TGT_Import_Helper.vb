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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_TGT_Import_Helper
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
#Region "Import TGT"
						'This makes sure there is only one import running at a time to make sure data is not overidden.
						'DEV NOTE: This may not be necessary with the new approach of adding the user into the loading tables
						If args.FunctionName.XFEqualsIgnoreCase("ImportREQ") Then
							Try
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
									
									Me.ImportTGT()
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
		
#Region "TGT Mass Import"
		Public Function	ImportTGT() As Object							
			Dim Dist As New CMD_TGT_Dist()
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.

			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)

			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath") 
			Dim fullFile = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.PrepImportFile(si,filePath)

	        Dim validFile As Boolean = True

			'Create DataTables to be used for importing and writing to the two relational tables
	        Dim tgtDataTable As New DataTable("CMD_TGT_Import")							
			Try
'				Using sr As New IO.StreamReader(csvFilePath)
'    dt = GetCsvDataReader(sr, delimiter)
'End Using
'	            Using reader As New StreamReader(fullFile)
'	                reader.TextFieldType = FieldType.Delimited
'	                reader.SetDelimiters(",")
	
'	                ' Add a column for validation errors and REQ_ID
'	                tgtDataTable.Columns.Add("ValidationError",GetType(String))
'					tgtDataTable.Columns.Add("ACOM",GetType(String))
'					tgtDataTable.Columns.Add("FiscalYear",GetType(String))
'					tgtDataTable.Columns.Add("FundsCenter",GetType(String))
'					tgtDataTable.Columns.Add("FundCode",GetType(String))
'					tgtDataTable.Columns.Add("MDEP",GetType(String))
'					tgtDataTable.Columns.Add("APE9",GetType(String))
'					tgtDataTable.Columns.Add("Amount",GetType(Decimal))
'	                ' Assuming the first line contains headers
'	                If Not reader.EndOfData Then
'	                    Dim headers = reader.ReadFields()
'	                    If headers.Length <> 7 Then
'	                       ' Throw New InvalidDataException(objXFFileInfo.Name & " has invalid structure. Please check the file if its in the correct format. Expected number of columns is 7, number columns in file header is "& headers.Length & vbCrLf & headers.ToString )
'	                    End If
'	                Else
'	                    Throw New InvalidDataException("The CSV file is empty.")
'	                End If
					
												
'	                Dim validLine As New StringBuilder()
'	                ' Read the file line by line
'	                While Not reader.EndOfData
'	                    Dim fields = reader.ReadFields()
'	                    Dim line As String = String.Join(",", fields)
'	                    If line.StartsWith(comd) Then
'	                        ' Process the previous valid line if it exists
'	                        If validLine.Length > 0 Then
'	                            ProcessLine(validLine.ToString(), fullDataTable, validFile)
'	                            validLine.Clear()
'	                        End If
'	                        ' Start a new valid line
'	                        validLine.Append(line)
'	                    Else
'	                        ' Concatenate with the previous line
'	                        validLine.Append(" " & line)
'	                    End If
'	                End While
								
'	                ' Process the last valid line
'	                If validLine.Length > 0 Then
'	                    ProcessLine(validLine.ToString(), fullDataTable, validFile)
'	                End If
'				End Using
											
'				'Write to session
'				BRApi.Utilities.SetSessionDataTable(si,si.UserName,"CMD_PGM_Import",fullDataTable)
'                ' Write the fullDataTable to the session if there are validation errors
'                If validFile Then
'					'Split fullDataTable and insert into the two tables
												
'					Me.SplitAndInsertIntoREQTables(fullDataTable, REQDataTable,REQDetailDataTable)
					
'                End If
			
	        Catch ex As Exception
	            Throw New Exception("An error occurred: " & ex.Message)
	        End Try
			'If the validation failed, write the error out.
			'If there are more than ten, we show only the first ten messages for the sake of redablity
'			Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
'			If Not validFile Then
'				Dim sErrorLog As String = ""
								
'				Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & objXFFileInfo.Name & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
'				BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
'				Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_IPGM_mport",  fullDataTable)
'				Return Nothing
'			End If
			
'			'File load complete. Write file to explorer
'			'Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
'			Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf 
'			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
'			Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_IPGM_mport",  fullDataTable)

		Return Nothing
		End Function
#End Region

#Region "Helper Functions"
'    Sub ProcessLine(line As String, ByRef dataTable As DataTable, ByRef validFile As Boolean)
'BRApi.ErrorLog.LogMessage(si, "Process Line 1")
'        Dim cleanedLine As String = CleanUpSpecialCharacters(line)
'		Dim values = ParseCsvLine(cleanedLine)
'		Dim currREQ As New CMD_PGM_Requirement
'BRApi.ErrorLog.LogMessage(si, "Process Line 2")		
'		currREQ = Me.ParseREQ(si,values, dataTable)
'		'If the FC is a parent, add _General
'		currREQ.Entity = CMD_PGM_Utilities.CheckFor_General(si, currREQ.Entity)' & "_General"
'BRApi.ErrorLog.LogMessage(si, "Process Line 3")
'		' Get APPN_FUND And PARENT APPN_L2 
'		currREQ.APE9 = CMD_PGM_Utilities.GetUD3(si, currREQ.FundCode, currREQ.APE9)
'BRApi.ErrorLog.LogMessage(si, "Process Line 4")		
''BRApi.ErrorLog.LogMessage(si, "entity: " & currREQ.Entity & ", APE: " & currREQ.APE9 & ", full line: " & line)		
'        Dim newRow = dataTable.NewRow()

'        ' Run validation (you can customize this part)
'        If Me.ValidateMembers(si, currREQ) Then
'BRApi.ErrorLog.LogMessage(si, "Process Line 5")			
'            newRow("ValidationError") = ""
'			Dim REQ_ID As String = Me.GetNextREQID(currREQ.Entity)' GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,currREQ.Entity)
'BRApi.ErrorLog.LogMessage(si, "Process Line 6")			
'			If Not String.IsNullOrWhiteSpace(REQ_ID) Then
'				newRow("REQ_ID") = REQ_ID
				
'				'Update Status
'				'Dim entityLevel As String = Me.GetEntityLevel(currREQ.Entity)
'				Dim sREQWFStatus As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,currREQ.Entity) & "_Formulate_PGM"
'				newRow("Status") = sREQWFStatus
'			Else 'Failed to get REQ_ID. File will be marked failed
'				validFile = False
'			End If
'        Else
'            validFile = False
'            newRow("ValidationError") = "Validation error: " & currREQ.validationError
'        End If
'BRApi.ErrorLog.LogMessage(si, "Process Line 7")	
'		'The offset is to account for the number of rows added to the dataTable before the file data
'		'The reason is so they can be seen easily at the begining of the table
'        For i As Integer = 0 To values.Length - 1
'            newRow(i + 5) = values(i)
'        Next
'		newRow("FundCenter") = currREQ.Entity
'		newRow("APE9") = currREQ.APE9
'		newRow("REQ_ID_Type") = "Requirement"
'		'Update Audit fields
'		newRow("Create_Date") = DateTime.Now
'		newRow("Create_User") = si.UserName
'		newRow("Update_Date") = DateTime.Now
'		newRow("Update_User") = si.UserName
'		newRow("WFScenario_Name") =  ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'		newRow("WFTime_Name") = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'		newRow("WFCMD_Name") =  BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
'		newRow("CMD_PGM_REQ_ID") = Guid.NewGuid()
'		newRow("IC") = "None"
'		newRow("Account") = "REQ_Requested_Amt"
'		newRow("Flow") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,currREQ.Entity) &"_Formulate_PGM"
'		newRow("UD7") = "None"
'		newRow("UD8") = "None"
'		'newRow("FY_Total") = "None"
'		'newRow("AlloUpdate") = "None"
		
'        dataTable.Rows.Add(newRow)
'    End Sub
#End Region	


	End Class
End Namespace
