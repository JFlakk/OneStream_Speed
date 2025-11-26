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
		
#Region "TGT Dist Import"
		Public Function	ImportTGT() As Object							
			Dim Dist As New CMD_TGT_Dist()
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = wfInfoDetails("ScenarioName")

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
'BRApi.ErrorLog.LogMessage(si, "Blank")
			Else 
'BRApi.ErrorLog.LogMessage(si, "ImportTGT_DT count = " & ImportTGT_DT.Rows.count)
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

			Brapi.Utilities.SetSessionDataTable(si,si.UserName, $"CMD_TGT_Import_TGT_Dist_{sScenario}", ImportTGT_DT)	

			'Load to cube
			If validFile Then
				
				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"40 CMD TGT")
				Dim rawEntList As List(Of String) = ImportTGT_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("FundsCenter")).ToList()
				Dim EntityList As String = "E#" & String.Join(",E#", ImportTGT_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("FundsCenter"))) & ""
				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,rawEntList)
				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
				Dim consolEntityList As String = String.Join(", ", EntityLists.Item3.Select(Function(s) $"E#{s}.DescendantsInclusive"))
				Dim DistOutconsolEntityList As String = String.Join(", ", EntityLists.Item3.Select(Function(s) $"E#{s}"))
				Dim DistOutEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("BaseEntityList",BaseEntityList)
				customSubstVars.Add("ParentEntityList",ParentEntityList)
				customSubstVars.Add("consolEntityList",consolEntityList)
				customSubstVars.Add("DistOutEntityList",$"{DistOutEntityList},{DistOutconsolEntityList}")

				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_TGT_Load_TGT_Dist_to_Cube", customSubstVars)
			End If
			
'Brapi.ErrorLog.LogMessage(si,"Done")

		Return Nothing
		End Function
#End Region

#Region "TGT WH Import"
		Public Function	ImportWH() As Object							
			Dim WH As New CMD_TGT_WH()
'BRApi.ErrorLog.LogMessage(si, "in TGT Import")			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
			Dim timeStart As DateTime = System.DateTime.Now
			Dim sScenario As String = wfInfoDetails("ScenarioName")

			Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
			Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
			
			Dim fileName As String = args.NameValuePairs.XFGetValue("FileName") 

			'Confirm source file exists
			Dim filePath As String = $"{BRApi.Utilities.GetFileShareFolder(si, FileshareFolderTypes.FileShareRoot,Nothing)}/{FileName}"
'			Dim fullFile = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.PrepImportFile(si,filePath)

	        Dim validFile As Boolean = True
			Dim ImportWH_DT As New DataTable()
			Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
				ImportWH_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", WH.ColumnMaps)
			End Using
			If ImportWH_DT Is Nothing	Then
'BRApi.ErrorLog.LogMessage(si, "Blank")
			Else 
'BRApi.ErrorLog.LogMessage(si, "ImportTGT_DT count = " & ImportTGT_DT.Rows.count)
			End If
			ImportWH_DT.TableName = "ImportWH_DT"
			
			'Check for errors
			Dim errRow As DataRow = ImportWH_DT.AsEnumerable().
										FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
										
			If errRow IsNot Nothing Then validFile = False
'BRApi.ErrorLog.LogMessage(si, "errRow: " & sScenario & " " & ImportTGT_DT.Rows.Count & ", passed: " & validFile)	
				
			'Write to the cube
			

			Dim stastusMsg As String = ""
			If Not validFile Then
				stastusMsg = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
			Else 
				Me.UpdateUD3(ImportWH_DT)
				stastusMsg = "IMPORT PASSED" & vbCrLf 
			End If

			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)

			Brapi.Utilities.SetSessionDataTable(si,si.UserName, $"CMD_TGT_Import_TGT_WH_{sScenario}", ImportWH_DT)	
			


			'Load to cube
			If validFile Then
				
				Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"40 CMD TGT")
				Dim rawEntList As List(Of String) = ImportWH_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("FundsCenter")).ToList()
				Dim EntityList As String = "E#" & String.Join(",E#", ImportWH_DT.AsEnumerable().Select(Function(row) row.Field(Of String)("FundsCenter"))) & ""
				Dim EntityLists  = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,rawEntList)
				Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
				Dim consolEntityList As String = String.Join(", ", EntityLists.Item3.Select(Function(s) $"E#{s}.DescendantsInclusive"))
				Dim customSubstVars As New Dictionary(Of String, String) 
				customSubstVars.Add("ParentEntityList",ParentEntityList)
				customSubstVars.Add("consolEntityList",consolEntityList)
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_TGT_Load_WH_to_Cube", customSubstVars)
			End If

		Return Nothing
		End Function
#End Region

#Region "Helper Functions"
		Public  Function UpdateUD3(ByRef importTGT_DT As DataTable) As Object
			Dim objU1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U1_FundCode")
			For Each r As DataRow In importTGT_DT.Rows
				Dim mbrScr As String = "U1#" &  r("FundsCode") & ".Ancestors.Where(MemberDim = U1_APPN)"
				
				'Dim mbrScr As String = "U1#" &  r("FundsCode") & ".Ancestors"
				
				Dim lsAncestorListUD1 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU1DimPK, mbrScr, True,,)
				If lsAncestorListUD1 Is Nothing Then
					Throw New Exception("No Valid APE")
				Else

				r("APE9") = lsAncestorListUD1(0).Member.Name & "_" & r("APE9").ToString().Trim
				End If		
'BRApi.ErrorLog.LogMessage(si, "APE9= " & r("APE9"))				
			Next
			Return Nothing
	End Function
	

#End Region
	End Class
End Namespace
