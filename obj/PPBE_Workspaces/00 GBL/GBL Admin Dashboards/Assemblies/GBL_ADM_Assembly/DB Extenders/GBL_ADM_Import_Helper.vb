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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.GBL_ADM_Import_Helper
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
						If args.FunctionName.XFEqualsIgnoreCase("ImportAppnMap") Then
							Me.ImportAPPNMap()
						End If
#End Region 
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Import Appn Map"
		Public Function	ImportAPPNMap() As Object							
			Dim APPNMap As New GBL_ADM_Appn_Map()
			
			Dim timeStart As DateTime = System.DateTime.Now
			
			Dim fileName As String = args.NameValuePairs.XFGetValue("FileName") 

			Dim FilePath As String = $"{BRApi.Utilities.GetFileShareFolder(si, FileshareFolderTypes.FileShareRoot,Nothing)}/{FileName}"
	        Dim validFile As Boolean = True
			Dim ImportAppnMap_DT As New DataTable()
			Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
				ImportAppnMap_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", APPNMap.ColumnMaps)
			End Using
			ImportAppnMap_DT.TableName = "ImportAppnMap_DT"
			
			'Check for errors
			Dim errRow As DataRow = ImportAppnMap_DT.AsEnumerable().
										FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
										
			If errRow IsNot Nothing Then validFile = False
'BRApi.ErrorLog.LogMessage(si, "errRow: " & sScenario & " " & ImportTGT_DT.Rows.Count & ", passed: " & validFile)	
				
			'Write to the cube
			

			Dim stastusMsg As String = ""
			If Not validFile Then
				stastusMsg = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
			Else 
				stastusMsg = "IMPORT PASSED" & vbCrLf 
			End If

			'Load to cube
			If validFile Then
				Dim sqa As New SqlDataAdapter()
				Dim xfcAppnMap_DT As DataTable = New DataTable()
		       	Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		                sqlConn.Open()
		                Dim sqaAppnMapReader As New SQA_XFC_APPN_Mapping(sqlConn)
		                Dim SqlAppnMap As String = $"SELECT * 
													FROM XFC_APPN_Mapping"	
						Dim sqlparamsAppnMap As SqlParameter() = New SqlParameter() {}
		                sqaAppnMapReader.Fill_XFC_APPN_Mapping_DT(sqa, xfcAppnMap_DT, SqlAppnMap, sqlparamsAppnMap)
						For Each importrow As DataRow In ImportAppnMap_DT.Rows
							' Get the primary key values from the import row
							Dim appnCd = importrow.Field(Of String)("Appropriation_CD")
							Dim dollarType = importrow.Field(Of String)("Dollar_Type")
							Dim seventhChar = importrow.Field(Of String)("Seventh_Character")
							Dim suppId = importrow.Field(Of String)("Supp_ID")
							Dim treasuryCd = importrow.Field(Of String)("Treasury_CD")
							Dim yearsAvail = importrow.Field(Of String)("Years_of_Availability")

							' Find the matching row in the DataTable from the database (xfcAppnMap_DT)
							Dim existingRow = xfcAppnMap_DT.AsEnumerable().FirstOrDefault(
								Function(dbRow)
									Return dbRow.Field(Of String)("Appropriation_CD") = appnCd AndAlso
										   dbRow.Field(Of String)("Dollar_Type") = dollarType AndAlso
										   dbRow.Field(Of String)("Seventh_Character") = seventhChar AndAlso
										   dbRow.Field(Of String)("Supp_ID") = suppId AndAlso
										   dbRow.Field(Of String)("Treasury_CD") = treasuryCd AndAlso
										   dbRow.Field(Of String)("Years_of_Availability") = yearsAvail
								End Function
							)
							
							If existingRow IsNot Nothing Then
								' --- UPDATE ---
								' The row exists, so update the non-key column(s)
								existingRow("Partial_Fund_CD") = importrow("Partial_Fund_CD")
							Else
								' --- INSERT ---
								' The row does not exist, so create a new row
								Dim newRow = xfcAppnMap_DT.NewRow()
								
								' Set all the key columns
								newRow("Appropriation_CD") = appnCd
								newRow("Dollar_Type") = dollarType
								newRow("Seventh_Character") = seventhChar
								newRow("Supp_ID") = suppId
								newRow("Treasury_CD") = treasuryCd
								newRow("Years_of_Availability") = yearsAvail
								
								' Set the non-key data column
								newRow("Partial_Fund_CD") = importrow("Partial_Fund_CD")
								
								' Add the new row to the database's DataTable
								xfcAppnMap_DT.Rows.Add(newRow)
							End If
						Next
						sqaAppnMapReader.Update_XFC_APPN_Mapping(xfcAppnMap_DT,sqa)
					End Using
				End Using
			End If

		Return Nothing
		End Function
#End Region

	End Class
End Namespace
