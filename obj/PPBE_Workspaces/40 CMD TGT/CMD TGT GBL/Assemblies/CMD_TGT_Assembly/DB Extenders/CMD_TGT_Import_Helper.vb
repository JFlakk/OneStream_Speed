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
						If args.FunctionName.XFEqualsIgnoreCase("ImportTGT") Then
							dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
							Me.ImportTGT()
						Else If args.FunctionName.XFEqualsIgnoreCase("ImportWH") Then
							dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
							Me.ImportWH()
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
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)

			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
			
			Dim fileName As String = args.NameValuePairs.XFGetValue("FileName") 

			Dim FilePath As String = $"{BRApi.Utilities.GetFileShareFolder(si, FileshareFolderTypes.FileShareRoot,Nothing)}/{FileName}"
			'Confirm source file exists
			'Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath") 
'			Dim fullFile = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.PrepImportFile(si,filePath)

	        Dim validFile As Boolean = True
			Dim ImportTGT_DT As New DataTable()
			Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
				ImportTGT_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", Dist.ColumnMaps)
			End Using
			If ImportTGT_DT Is Nothing	Then
				BRApi.ErrorLog.LogMessage(si, "Blank")
			Else 
				BRApi.ErrorLog.LogMessage(si, "ImportTGT_DT count = " & ImportTGT_DT.Rows.count)
			End If
			ImportTGT_DT.TableName = "ImportTGT_DT"
			
			'Check for errors
			Dim errRow As DataRow = ImportTGT_DT.AsEnumerable().
										FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
										
			If errRow IsNot Nothing Then validFile = False
'BRApi.ErrorLog.LogMessage(si, "errRow: " & sScenario & " " & ImportTGT_DT.Rows.Count & ", passed: " & validFile)	
				
			'Write to the cube
			

			Dim stastusMsg As String = ""
			If Not validFile Then
				stastusMsg = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
			Else 
				Me.UpdateUD3(ImportTGT_DT)
				stastusMsg = "IMPORT PASSED" & vbCrLf 
			End If

			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
			Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_TGT_Import_" & sScenario,  ImportTGT_DT)
			
			Brapi.Utilities.SetSessionDataTable(si,si.UserName, $"CMD_TGT_Import_TGT_Dist_{sScenario}", ImportTGT_DT)	

			'Load to cube
			If validFile Then
				
				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"40 CMD TGT")
				Dim EntityList As String = "E#" & String.Join(",E#", ImportTGT_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("FundsCenter"))) & ""
	Brapi.ErrorLog.LogMessage(si,"@HERE1" &String.Join(",",EntityList))
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("EntityList",EntityList)
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_TGT_Load_TGT_Dist_to_Cube", customSubstVars)
			End If
			
Brapi.ErrorLog.LogMessage(si,"Done")

		Return Nothing
		End Function
#End Region

#Region "TGT Mass Import"
		Public Function	ImportWH() As Object							
			Dim Dist As New CMD_TGT_Dist()
BRApi.ErrorLog.LogMessage(si, "in TGT Import")			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = "" 'Scenario will be determined from the Cycle.

			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)

			'Confirm source file exists
			Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath") 
'			Dim fullFile = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.PrepImportFile(si,filePath)

	        Dim validFile As Boolean = True
			Dim ImportTGT_DT As New DataTable()
			Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
				ImportTGT_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", Dist.ColumnMaps)
			End Using
			
			'Check for errors
			Dim errRow As DataRow = ImportTGT_DT.AsEnumerable().
										FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
										
			If errRow IsNot Nothing Then validFile = False

'			Dim REQDataTable As New DataTable("XFC_CMD_PGM_REQ")
'			Dim REQDetailDataTable As New DataTable("XFC_CMD_PGM_REQ_Details")
			
'            If validFile Then
'				'get req_id and guid
'				UpdateColsForDatabase(Importreq_DT)
'				PostProcessNewREQ(ImportREQ_DT)
				
''BRApi.ErrorLog.LogMessage(si, "Post proc completed " & Importreq_DT.TableName)					
'				'Split fullDataTable and insert into the two tables
'				Me.SplitAndInsertIntoREQTables(Importreq_DT, REQDataTable,REQDetailDataTable)
				
'            End If
			
			'write to cube
'			Dim REQ_IDs As New List(Of String)
'			For Each r As DataRow In ImportTGT_DT.Rows
'				REQ_IDs.Add(r("REQ_ID").ToString)	
'			Next
			
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

'Get Entity List
'Run DM Sequence to load data into Cube
		Return Nothing
		End Function
#End Region

#Region "Helper Functions"
		Public  Function UpdateUD3(ByRef importTGT_DT As DataTable) As Object
			
			For Each r As DataRow In importTGT_DT.Rows
				r("APE9") = r("FundsCode").ToString().Trim & "_" & r("APE9").ToString().Trim
Brapi.ErrorLog.LogMessage(si,"APE: " & r("APE9"))
				
			Next
			Return Nothing
	End Function

#End Region
	End Class
End Namespace
