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


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_SPLN_Import_Helper
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
'brapi.ErrorLog.LogMessage(si,"Hit 1")
								'BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Dim runningImport As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import")
								dbExt_ChangedResult = Workspace.GBL.GBL_Assembly.GBL_Helpers.Check_WF_Complete_Lock(si, globals, api, args)
								If Not runningImport.XFEqualsIgnoreCase("running")
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","running")
'brapi.ErrorLog.LogMessage(si,"Hit 2")									
									Me.ImportREQ(si,globals,api,args)
								Else
									Throw New System.Exception("There is an import running currently." & vbCrLf & " Please try in a few minutes.")
								End If
								
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
							Catch ex As Exception
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "var_REQPRO_IMPORT_0CaAa_A_Requirement_Singular_Import","completed")
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
							
							
							
'						Else If args.FunctionName.XFEqualsIgnoreCase("ImportCivPayREQ") Then
'							Return Me.ImportCivPayREQ(si, globals,api,args)
							
							
							
						End If
#End Region 
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		

'#Region "REQ CivPay Mass Import"
'		Public Function	ImportCivPayREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							
'			Try
				
'				'Get the file and send to the proper location
'				Dim CivPayREQ As New CMD_SPLN_REQ()
'				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				
'				Dim timeStart As DateTime = System.DateTime.Now
'				Dim sScenario As String = "" 'Scenario will be determined from the Cycle.
	
'				Dim mbrComd = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Entity, wfInfoDetails("CMDName")).Member
'				Dim comd As String = BRApi.Finance.Entity.Text(si, mbrComd.MemberId, 1, 0, 0)
				
'				Dim fileName As String = args.NameValuePairs.XFGetValue("FileName") 
	
'				Dim FilePath As String = $"{BRApi.Utilities.GetFileShareFolder(si, FileshareFolderTypes.FileShareRoot,Nothing)}/{FileName}"
				
'				Dim CivPayImportreq_DT As New DataTable("CivPayImportreqs")
'					Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
'						CivPayImportreq_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", CivPayREQ.ColumnMaps)
'						CivPayImportreq_DT.TableName = "CivPayImportreqs"
						
'					End Using				
				
'				'Check for errors
'				Dim validFile As Boolean = True
'				Dim errRow As DataRow = CivPayImportreq_DT.AsEnumerable().
'											FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
											
'				If errRow IsNot Nothing Then validFile = False

'				Dim CivPayREQDataTable As New DataTable("XFC_CMD_SPLN_REQ")
'				Dim CivPayREQDetailDataTable As New DataTable("XFC_CMD_SPLN_REQ_Details")
'	            If validFile Then
'					'get req_id and guid
'					CivPayUpdateColsForDatabase(CivPayImportreq_DT)

'					'In the Post process New Req function, get the ancestors of each fund code to get the appropriation, for each new appropriation fund center combination create a new unique requirement id, requirement id's need to then have a type of CivPay
'					CivPayPostProcessNewREQ(CivPayImportreq_DT)

'					'Split fullDataTable and insert into the two tables
'					Me.CivPaySplitAndInsertIntoREQTables(CivPayImportreq_DT, CivPayREQDataTable,CivPayREQDetailDataTable)
					
'					Dim CivPayREQ_IDs As New List(Of String)
'					For Each r As DataRow In CivPayImportreq_DT.Rows
'						CivPayREQ_IDs.Add(r("REQ_ID").ToString)	
'					Next
'					Dim loader As New CMD_SPLN_Helper.MainClass
'					Args.NameValuePairs.Add("req_IDs", String.Join(",", CivPayREQ_IDs))
'					Args.NameValuePairs.Add("new_Status", "Formulate") 'It keeps the same status
'					loader.main(si, globals, api, args)
					
'					'File load complete. Write file to explorer
'					'Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
'					Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf 
'					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
'					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_SPLN_Import_" & wfInfoDetails("ScenarioName") ,  CivPayImportREQ_DT)
					
'					Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
'				Else 'If the validation failed, write the error out.
'					Dim sErrorLog As String = ""
									
'					Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
'					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
'					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_SPLN_Import_" & wfInfoDetails("ScenarioName"),  CivPayImportREQ_DT)
'					Return Nothing
'				End If
					
					
'				'Make and Get both Req and Req detail table
'				'Update the columns for the database based on the file, adding addtional fields 
'				'Split and instert the new requirements into to the proper tables
				
				
				
				
'				Return Nothing
'	        Catch ex As Exception
'	            Throw New Exception("An error occurred: " & ex.Message)
'	        End Try
'		End Function			
'#End Region		
		
'#Region "Import Civ Pay Helpers"

'#Region "Call Function CivPayUpdateColsForDatabase"
'	Public Function CivPayUpdateColsForDatabase(ByRef dt As DataTable) As StringReader
'		'Add columns in database but not in file records
'		dt.Columns.Add("ValidationError")
'		dt.Columns.Add("CMD_SPLN_REQ_ID",GetType(Guid))
'		dt.Columns.Add("REQ_ID_Type")
'		dt.Columns.Add("REQ_ID")
'		dt.Columns.Add("Status")
'        ' Assuming the first line contains headers

'		'Add audit columns for narrative
'		'table updates will delete once confirmed.
'		dt.Columns.Add("Invalid",GetType(String))
'		dt.Columns.Add("Create_Date", GetType(DateTime))
'		dt.Columns.Add("Create_User",GetType(String))
'		dt.Columns.Add("Update_Date", GetType(DateTime))
'		dt.Columns.Add("Update_User",GetType(String))
'		dt.Columns.Add("WFScenario_Name",GetType(String))
'		dt.Columns.Add("WFCMD_Name",GetType(String))
'		dt.Columns.Add("WFTime_Name",GetType(String))
		
'		'Add columns for detail
'		dt.Columns.Add("IC",GetType(String))
'		dt.Columns.Add("Account",GetType(String))
'		dt.Columns.Add("Flow",GetType(String))
'		dt.Columns.Add("UD5",GetType(String))
'		dt.Columns.Add("UD7",GetType(String))
'		dt.Columns.Add("UD8",GetType(String))
'		dt.Columns.Add("Quarter1",GetType(String))
'		dt.Columns.Add("Quarter2",GetType(String))
'		dt.Columns.Add("Quarter3",GetType(String))
'		dt.Columns.Add("Quarter4",GetType(String))
'		dt.Columns.Add("Yearly",GetType(String))
'		dt.Columns.Add("AllowUpdate",GetType(Boolean))
'        dt.Columns.Add("Unit_Of_Measure",GetType(String))
'        Return Nothing
'	End Function			


'#End Region

'#Region "Call Function CivPayPostProcessNewREQ"
'	Public Function CivPayPostProcessNewREQ(ByRef dt As DataTable) As StringReader
'		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'		For Each row As DataRow In dt.Rows
'			Dim objU1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U1_FundCode")
'			Dim lsAncestorListUD1 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU1DimPK, "U1#" &  row("FundCode") & ".Ancestors.Where(MemberDim = U1_APPN)", True,,)	
'			Dim fundCenter As String = row("FundCenter").ToString
'			Dim APPN As String = row("APE9") = lsAncestorListUD1(0).Member.Name & "_" & row("APE9") 
'			Dim CivPayREQIDKey As String = fundCenter & "_" & APPN
'			Dim modFC As String = fundCenter.Replace("_General","")
			
'			'If 
			
'			Dim REQ_ID As String = CivPayGetNextREQID(CivPayREQIDKey)
''			Dim REQ_ID As String = $"{fundCenter}_CP_{APPN}_0001"
'			row("REQ_ID") = REQ_ID			
'			row("CMD_SPLN_REQ_ID") = Guid.NewGuid()
'			row("WFScenario_Name") =  wfInfoDetails("ScenarioName")
'			row("WFTime_Name") =  wfInfoDetails("TimeName")
'			row("WFCMD_Name") =  wfInfoDetails("CMDName")
'			row("IC") = "None"
'			row("Account") = "Req_Funding"	
'			row("Flow") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_SPLN"
'			row("Status") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_SPLN"
'			row("UD5") = "None"
'			row("UD7") = "None"
'			row("UD8") = "None"			
'			row("FundCenter") = fundCenter		
'			row("APE9") = lsAncestorListUD1(0).Member.Name & "_" & row("APE9")         '"OMA_111011000"'row("FundCode") & "_" & row("APE9") 'CMD_SPLN_Utilities.GetUD3(si, row("APPN"), row("APE9")) '"OMA_122011000"
'			row("REQ_ID_Type") = "CivPay" 'Temp import for Civpay
'			row("Create_Date") = DateTime.Now
'			row("Create_User") = si.UserName
'			row("Update_Date") = DateTime.Now
'			row("Update_User") = si.UserName
'		Next
'        Return Nothing
'	End Function	
	
	
'#Region "Call Function CivPayGetNexREQID"
'	Dim CivPaystartingREQ_IDByFC As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)
'	Function CivPayGetNextREQID(CivPayREQIDKey As String) As String
'		Dim CivPaycurrentREQID As Integer
'		Dim CivPaynewREQ_ID As String
'		Dim fundCenter As String = String.Empty
'			'If the Key (FundsCenter & APPN combination) doesnt exist then split fund center to make the Requirement ID
'			If CivPaystartingREQ_IDByFC.TryGetValue(CivPayREQIDKey, CivPaycurrentREQID) = False Then
'				fundCenter = CivPayREQIDKey.Split("_"c)(0)
			
''			If CivPaystartingREQ_IDByFC.TryGetValue(fundCenter, CivPaycurrentREQID) Then
'				CivPaycurrentREQID = CivPaycurrentREQID + 1
'				Dim modifiedFC As String = fundCenter
'				modifiedFC = modifiedFC.Replace("_General", "")
'				If modifiedFC.Length = 3 Then 
'					modifiedFC = modifiedFC & "xx"
'					CivPaynewREQ_ID =  modifiedFC &"_" & CivPaycurrentREQID.ToString("D5")
'					CivPaystartingREQ_IDByFC(fundCenter) = CivPaycurrentREQID
'				Else
'					CivPaynewREQ_ID = GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,fundCenter)
'					CivPaystartingREQ_IDByFC.Add(fundCenter.Trim, CivPaynewREQ_ID.Split("_")(1))
'				End If 
				
'			Else If CivPaystartingREQ_IDByFC.TryGetValue(CivPayREQIDKey, CivPaycurrentREQID) = True Then
'					CivPaynewREQ_ID = CivPaycurrentREQID
'			End If
'		Return CivPaynewREQ_ID
'	End Function
'#End Region

'#End Region

'#Region "Call Function CivPaySplitAndInsertIntoREQTables to setup split into tables"
'	Function CivPaySplitAndInsertIntoREQTables(ByRef CivPayfullDT As DataTable, ByRef CivPayREQDT As DataTable, ByRef CivPayREQDTDetail As DataTable )

'		Dim sqa As New SqlDataAdapter()
'		Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'		Dim scName As String = wfInfoDetails("ScenarioName")
'		Dim cmd As String = wfInfoDetails("CMDName")
'		Dim tm As String = wfInfoDetails("TimeName")

'       Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
'                sqlConn.Open()
'                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
'                Dim SqlREQ As String = $"SELECT * 
'									FROM XFC_CMD_SPLN_REQ
'									WHERE WFScenario_Name = '{scName}'
'									And WFCMD_Name = '{cmd}'
'									AND WFTime_Name = '{tm}'"

'				Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
'                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, CivPayREQDT, SqlREQ, sqlparamsREQ)
'				CivPayREQDT.PrimaryKey = New DataColumn() {CivPayREQDT.Columns("CMD_SPLN_REQ_ID")}

'				'Prepare Detail	
'				 Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
'				 Dim SqlREQDetail As String = $"SELECT * 
'									FROM XFC_CMD_SPLN_REQ_Details
'									WHERE WFScenario_Name = '{scName}'
'									And WFCMD_Name = '{cmd}'
'									AND WFTime_Name = '{tm}'"

'				Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
'                sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, CivPayREQDTDetail, SqlREQDetail, sqlparamsREQDetails)
'				'Define PKs to match the table
''				REQDTDetail.PrimaryKey = New DataColumn() {
''					 REQDTDetail.Columns("CMD_SPLN_REQ_ID"),
''					 REQDTDetail.Columns("Start_Year"),
''			         REQDTDetail.Columns("Unit_of_Measure"),
''					 REQDTDetail.Columns("Account")
''					 }
'				'REQDTDetail.PrimaryKey = primaryKeyColumns
		 				
''				Me.CivPaySplitFullDataTable(CivPayfullDT, CivPayREQDT, CivPayREQDTDetail)

'				sqaREQReader.Update_XFC_CMD_SPLN_REQ(CivPayREQDT, sqa)
			 							
'				sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(CivPayREQDTDetail,sqa)
		 								

'			End Using
'		End Using
		
'''BRApi.ErrorLog.LogMessage(si, "setupXFC_CMD_SPLN_REQ_Details. 1" & detailDT.Columns(0).ColumnName)		
       
'	End Function

'	#Region"Call Function CivPaySplitFullDataTable to Split dataset"
'	Public Function CivPaySplitFullDataTable (ByRef CivPayfullDT As DataTable, ByRef CivPayREQDT As DataTable, ByRef CivPayREQDTDetail As DataTable) As Object
'		' Split and translate data per row
		
'		For Each row As DataRow In CivPayfullDT.Rows
	   
'			'Handle CivPayREQDT translation and insertion/update
'			Dim REQExists As Boolean = False
'			Dim existingREQ_ID As String
'			Dim existingCMD_SPLN_REQ_ID As String
'			Dim MappingForREQ As New Dictionary(Of String, String)
'			MappingForREQ = GetMappingForREQ()
''		    Dim existingRowREQDT As DataRow = REQDT.Rows.Find(row("CMD_SPLN_REQ_ID")) '***DEV NOTE: TO DO this will have to be modified to match on other fields
			    
''			If existingRowREQDT IsNot Nothing Then
''				REQExists = True
''				existingCMD_SPLN_REQ_ID = existingRowREQDT("CMD_SPLN_REQ_ID")
''				existingREQ_ID = existingRowREQDT("REQ_ID")
''			End If
		      
''		    ' Create a new row
''	        Dim newRowREQDT As DataRow = REQDT.NewRow()
			
''	        For Each fullCol As KeyValuePair (Of String, String) In MappingForREQ
''				If REQDT.Columns(fullCol.Value).DataType Is GetType(DateTime) Then
''                    Dim tempDate As DateTime
''                    If DateTime.TryParse(row(fullCol.Key), tempDate) Then
''                        newRowREQDT(MappingForREQ(fullCol.Key)) = row(fullCol.Key)
''                    Else
''                        newRowREQDT(MappingForREQ(fullCol.Key)) = DBNull.Value
''                    End If
''                Else
''					newRowREQDT(MappingForREQ(fullCol.Key)) = row(fullCol.Key)
''				End If 

''	        Next

''			'If it is an existing row we will keep the original REC_ID. All the data will be from the file
''			If REQExists Then
''				 Me.UpdateExistingREQIDs(newRowREQDT, existingCMD_SPLN_REQ_ID, existingREQ_ID)					 
''			End If
		    
''			REQDT.Rows.Add(newRowREQDT)

''	        ' Create a new row
''			Dim MappingForREQDetailsObligation As New Dictionary(Of String, String)
''			MappingForREQDetailsObligation = GetMappingForREQDetailsObligation()
''	        Dim newRowREQDTDetailObligation As DataRow = REQDTDetail.NewRow()
''	        For Each fullCol As String In GetMappingForREQDetailsObligation.Keys
''	            newRowREQDTDetailObligation(GetMappingForREQDetailsObligation(fullCol)) = row(fullCol)
''	        Next
''			'Map funding to the regular FYDEP
	
''			newRowREQDTDetailObligation("Account") = "Obligations"
''			newRowREQDTDetailObligation("UNIT_OF_MEASURE") = "Funding"
''			newRowREQDTDetailObligation("Quarter1") = convert.ToInt32(row("OBL_M1")) + convert.ToInt32(row("OBL_M2")) + convert.ToInt32(row("OBL_M3"))
''			newRowREQDTDetailObligation("Quarter2") = convert.ToInt32(row("OBL_M4")) + convert.ToInt32(row("OBL_M5")) + convert.ToInt32(row("OBL_M6"))
''			newRowREQDTDetailObligation("Quarter3") = convert.ToInt32(row("OBL_M7")) + convert.ToInt32(row("OBL_M8")) + convert.ToInt32(row("OBL_M9"))
''			newRowREQDTDetailObligation("Quarter4") = convert.ToInt32(row("OBL_M10")) + convert.ToInt32(row("OBL_M11")) + convert.ToInt32(row("OBL_M12"))
''			If REQExists Then
''				Me.UpdateExistingREQIDs(newRowREQDTDetailObligation, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 				 
''			End If
''			REQDTDetail.Rows.Add(newRowREQDTDetailObligation)
		   
			
''			' Handle REQDTDetail Items 

''			Dim MappingForREQDetailsCommitment As New Dictionary(Of String, String)
''			MappingForREQDetailsCommitment = GetMappingForREQDetailsCommitment()
''	        ' Create a new row
''	        Dim newRowREQDTDetailCommitment As DataRow = REQDTDetail.NewRow()
''	        For Each fullCol As String In MappingForREQDetailsCommitment.Keys
''	            newRowREQDTDetailCommitment(MappingForREQDetailsCommitment(fullCol)) = row(fullCol)
''	        Next
		
''			newRowREQDTDetailCommitment("Account") = "Commitments"
''			newRowREQDTDetailCommitment("UNIT_OF_MEASURE") = "Funding"
''			newRowREQDTDetailCommitment("Quarter1") = convert.ToInt32(row("COM_M1")) + convert.ToInt32(row("COM_M2")) + convert.ToInt32(row("COM_M3"))
''			newRowREQDTDetailCommitment("Quarter2") = convert.ToInt32(row("COM_M4")) + convert.ToInt32(row("COM_M5")) + convert.ToInt32(row("COM_M6"))
''			newRowREQDTDetailCommitment("Quarter3") = convert.ToInt32(row("COM_M7")) + convert.ToInt32(row("COM_M8")) + convert.ToInt32(row("COM_M9"))
''			newRowREQDTDetailCommitment("Quarter4") = convert.ToInt32(row("COM_M10")) + convert.ToInt32(row("COM_M11")) + convert.ToInt32(row("COM_M12"))
			
''		    If REQExists Then
''				Me.UpdateExistingREQIDs(newRowREQDTDetailCommitment, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
''			End If
''			REQDTDetail.Rows.Add(newRowREQDTDetailCommitment)
			
''			If CheckComCarryover(row) Then 

''				Dim MappingForREQDetailsCommitmentCarryover As New Dictionary(Of String, String)
''				MappingForREQDetailsCommitmentCarryover = GetMappingForREQDetailsCarryoverCommitment()
''		        ' Create a new row
''		        Dim newRowREQDTDetailCommitmentCarryover As DataRow = REQDTDetail.NewRow()
''		        For Each fullCol As String In MappingForREQDetailsCommitmentCarryover.Keys
''		            newRowREQDTDetailCommitmentCarryover(MappingForREQDetailsCommitmentCarryover(fullCol)) = row(fullCol)
''		        Next

''				newRowREQDTDetailCommitmentCarryover("Account") = "Commitments"
''				newRowREQDTDetailCommitmentCarryover("UNIT_OF_MEASURE") = "Funding"
''				newRowREQDTDetailCommitmentCarryover("Fiscal_Year") = row("Cycle") + 1
'''brapi.ErrorLog.LogMessage(si,"CM: " & row("Cycle") & " - " & row("Cycle") + 1)
''				newRowREQDTDetailCommitmentCarryover("Yearly") = row("COM_Carryover")
				
''			    If REQExists Then
''					Me.UpdateExistingREQIDs(newRowREQDTDetailCommitmentCarryover, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
''				End If
''				REQDTDetail.Rows.Add(newRowREQDTDetailCommitmentCarryover)
''			End If 
			
			
''			If CheckOblCarryover(row) Then
'''brapi.ErrorLog.LogMessage(si,"inside Oblig Carryover if")
''				Dim MappingForREQDetailsObligationCarryover As New Dictionary(Of String, String)
''				MappingForREQDetailsObligationCarryover = GetMappingForREQDetailsCarryoverObligation()
''		        ' Create a new row
''		        Dim newRowREQDTDetailObligationCarryover As DataRow = REQDTDetail.NewRow()
''		        For Each fullCol As String In MappingForREQDetailsObligationCarryover.Keys
''		            newRowREQDTDetailObligationCarryover(MappingForREQDetailsObligationCarryover(fullCol)) = row(fullCol)
''		        Next

''				newRowREQDTDetailObligationCarryover("Account") = "Obligations"
''				newRowREQDTDetailObligationCarryover("UNIT_OF_MEASURE") = "Funding"
''				newRowREQDTDetailObligationCarryover("Fiscal_Year") = row("Cycle") + 1
''				newRowREQDTDetailObligationCarryover("Yearly") = row("OBL_Carryover")
''			    If REQExists Then
''					Me.UpdateExistingREQIDs(newRowREQDTDetailObligationCarryover, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
''				End If
''				REQDTDetail.Rows.Add(newRowREQDTDetailObligationCarryover)
''			End If 

			

			
''			' Handle REQDTDetail Quantity
					
			
			
'		Next
'''BRApi.ErrorLog.LogMessage(si,"In Split* 7")

'	End Function
	
''	Public Sub UpdateExistingREQIDs(ByRef dr As DataRow, ByVal existingCMD_SPLN_REQ_ID As String, ByVal existingREQ_ID As String )
''		dr("CMD_SPLN_REQ_ID") = existingCMD_SPLN_REQ_ID
''		dr("REQ_ID") = existingREQ_ID
''	End Sub
'#End Region


'#End Region

'#End Region
		
		
		
#Region "REQ Mass Import"
		'This rule reads the imported file chcks if it is readable then parses into the REQ class
		'*****FILL OUT MORE

		Public Function	ImportREQ(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object							

			Try
'brapi.ErrorLog.LogMessage(si,"Hit 3")
				Dim REQ As New CMD_SPLN_REQ()
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
'brapi.ErrorLog.LogMessage(si,"Hit 4")		        
				Dim Importreq_DT As New DataTable("Importreqs")
				Using sr As New StreamReader(System.IO.File.OpenRead(filePath))
					ImportREQ_DT = Workspace.GBL.GBL_Assembly.GBL_Import_Helpers.GetCsvDataReader(si, globals, sr, ",", REQ.ColumnMaps)
					Importreq_DT.TableName = "Importreqs"
					
				End Using				
				
'brapi.ErrorLog.LogMessage(si,"Hit 5")
'BRApi.ErrorLog.LogMessage(si, "Returned to Import: " & ImportREQ_DT.Rows(0)("Invalid Errors").ToString)

				'Check for errors
				Dim validFile As Boolean = True
				Dim errRow As DataRow = Importreq_DT.AsEnumerable().
											FirstOrDefault(Function(r) Not String.IsNullOrEmpty(r.Field(Of String)("Invalid Errors")) )
											
				If errRow IsNot Nothing Then validFile = False

				Dim REQDataTable As New DataTable("XFC_CMD_SPLN_REQ")
				Dim REQDetailDataTable As New DataTable("XFC_CMD_SPLN_REQ_Details")
'brapi.ErrorLog.LogMessage(si,"Hit 6")				
	            If validFile Then
					'get req_id and guid
'brapi.ErrorLog.LogMessage(si,"Hit 7")	
					UpdateColsForDatabase(Importreq_DT)
'brapi.ErrorLog.LogMessage(si,"Hit 8")	
					PostProcessNewREQ(ImportREQ_DT)
'brapi.ErrorLog.LogMessage(si,"Hit 9")						
'BRApi.ErrorLog.LogMessage(si, "Post proc completed " & Importreq_DT.TableName)					
					'Split fullDataTable and insert into the two tables
					Me.SplitAndInsertIntoREQTables(Importreq_DT, REQDataTable,REQDetailDataTable)
'brapi.ErrorLog.LogMessage(si,"Hit 10")						
	            'End If
'BRApi.ErrorLog.LogMessage(si, "Post 1")				
					'write to cube
					Dim REQ_IDs As New List(Of String)
					For Each r As DataRow In ImportREQ_DT.Rows
						REQ_IDs.Add(r("REQ_ID").ToString)	
'brapi.ErrorLog.LogMessage(si,"row" & r.Item("POPExpirationDate").ToString)
					Next
'BRApi.ErrorLog.LogMessage(si, "Post 2")				
					Dim loader As New CMD_SPLN_Helper.MainClass
					Args.NameValuePairs.Add("req_IDs", String.Join(",", REQ_IDs))
					Args.NameValuePairs.Add("new_Status", "Formulate") 'It keeps the same status
					loader.main(si, globals, api, args)
					
					'File load complete. Write file to explorer
					'Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf & "Output file is located in the following folder for review:" & vbCrLf & "DOCUMENTS/USERS/" & si.UserName.ToUpper
					Dim uploadStatus As String = "IMPORT PASSED" & vbCrLf 
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", uploadStatus)
					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_SPLN_Import_" & wfInfoDetails("ScenarioName") ,  ImportREQ_DT)
					
					Dim sPasstimespent As System.TimeSpan = Now.Subtract(timestart)
				Else 'If the validation failed, write the error out.
					Dim sErrorLog As String = ""
									
					Dim stastusMsg As String = "LOAD FAILED" & vbCrLf & fileName & " has invalid data." & vbCrLf & vbCrLf & $"To view import error(s), please take look at the column titled ""ValidationError""."
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus", stastusMsg)
					Brapi.Utilities.SetSessionDataTable(si,si.UserName, "CMD_SPLN_Import_" & wfInfoDetails("ScenarioName"),  ImportREQ_DT)
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
'brapi.ErrorLog.LogMessage(si,"Hit Post Process 1")			
			Dim objU1DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U1_FundCode")
			Dim objU6DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U6_CommitItem")
brapi.ErrorLog.LogMessage(si,"ObjectClass = " & row("ObjectClass"))
			Dim lsAncestorListUD1 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU1DimPK, "U1#" &  row("FundCode") & ".Ancestors.Where(MemberDim = U1_APPN)", True,,)	
			Dim lsAncestorListUD6 As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objU6DimPK, "U6#" &  row("ObjectClass") & ".Ancestors.Where(MemberDim = U6_CostCat)", True,,)	
			Dim fundCenter As String = row("FundCenter").ToString
			'If 
			Dim REQ_D As String = GetNextREQID(fundCenter)
			row("REQ_ID") = REQ_D			
			row("CMD_SPLN_REQ_ID") = Guid.NewGuid()
			row("WFScenario_Name") =  wfInfoDetails("ScenarioName")
			row("WFTime_Name") =  wfInfoDetails("TimeName")
			row("WFCMD_Name") =  wfInfoDetails("CMDName")
			row("IC") = "None"
			row("Account") = "Req_Funding"	
			row("Flow") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_SPLN"
			row("Status") = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si,fundCenter) &"_Formulate_SPLN"
			row("UD5") = "None"
			row("UD7") = "None"
			row("UD8") = "None"			
			row("FundCenter") = fundCenter		
'brapi.ErrorLog.LogMessage(si,"lsAncestorListUD1=" & lsAncestorListUD1(0).Member.Name)
			row("APE9") =   lsAncestorListUD1(0).Member.Name & "_" & row("APE9") '"OMA_111011000"'row("FundCode") & "_" & row("APE9") 'CMD_SPLN_Utilities.GetUD3(si, row("APPN"), row("APE9")) '"OMA_122011000"
			
brapi.ErrorLog.LogMessage(si,"ancestors = " & lsAncestorListUD6(0).Member.Name)
			If lsAncestorListUD6(0).Member.Name = "Pay_Benefits" Then
				row("REQ_ID_Type") = "CivPay" 'Temp import for Civpay
			Else
				row("REQ_ID_Type") = "Requirement"
			End If
'row("REQ_ID_Type") = "CivPay" 'Temp import for Civpay
'row("REQ_ID_Type") = "Withhold" 'Temp import for Withhold
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
		dt.Columns.Add("CMD_SPLN_REQ_ID",GetType(Guid))
		'dt.PrimaryKey = New DataColumn() {dt.Columns("CMD_SPLN_REQ_ID")}
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
		dt.Columns.Add("UD5",GetType(String))
		dt.Columns.Add("UD7",GetType(String))
		dt.Columns.Add("UD8",GetType(String))
		dt.Columns.Add("Quarter1",GetType(String))
		dt.Columns.Add("Quarter2",GetType(String))
		dt.Columns.Add("Quarter3",GetType(String))
		dt.Columns.Add("Quarter4",GetType(String))
		dt.Columns.Add("Yearly",GetType(String))
		dt.Columns.Add("AllowUpdate",GetType(Boolean))
        dt.Columns.Add("Unit_Of_Measure",GetType(String))
        Return Nothing
	
	End Function			
	
	Public Function GetExistingRow(ByRef dt As DataTable, ByRef account As String) As DataRow
'		Dim existingRow As System.Data.DataRow = ( From r As System.Data.DataRow In dt.AsEnumerable()
'									   Where r.Field(Of String)("CMD_SPLN_REQ_ID").ToString().XFContainsIgnoreCase(row("CMD_SPLN_REQ_ID").ToString) AndAlso
'											 r.Field(Of String)("Cycle").ToString().XFContainsIgnoreCase(row("Cycle").ToString) AndAlso
'											 r.Field(Of String)("UNIT_OF_MEASURE").ToString().XFContainsIgnoreCase(row("UNIT_OF_MEASURE").ToString) AndAlso
'											 r.Field(Of String)("Account").ToString().XFContainsIgnoreCase(account) 
'										Select r
'										).FirstOrDefault()
		For Each row In dt.Rows 
			If (row("CMD_SPLN_REQ_ID").ToString().XFContainsIgnoreCase(row("CMD_SPLN_REQ_ID").ToString) AndAlso
				row("Start_Year").ToString().XFContainsIgnoreCase(row("Start_Year").ToString) AndAlso
				row("UNIT_OF_MEASURE").ToString().XFContainsIgnoreCase(row("UNIT_OF_MEASURE").ToString) AndAlso
				row("Account").ToString().XFContainsIgnoreCase(account))
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
        Dim allowedChars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,""@!#$%'()+=_.:><?~-[]*^/" & vbCrLf & vbLf
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
                Dim sqaREQReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
                Dim SqlREQ As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

				Dim sqlparamsREQ As SqlParameter() = New SqlParameter() {}
                sqaREQReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, REQDT, SqlREQ, sqlparamsREQ)
				REQDT.PrimaryKey = New DataColumn() {REQDT.Columns("CMD_SPLN_REQ_ID")}

				'Prepare Detail	
				 Dim sqaREQDetailReader As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
				 Dim SqlREQDetail As String = $"SELECT * 
									FROM XFC_CMD_SPLN_REQ_Details
									WHERE WFScenario_Name = '{scName}'
									And WFCMD_Name = '{cmd}'
									AND WFTime_Name = '{tm}'"

				Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
                sqaREQDetailReader.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa, REQDTDetail, SqlREQDetail, sqlparamsREQDetails)
				'Define PKs to match the table
'				REQDTDetail.PrimaryKey = New DataColumn() {
'					 REQDTDetail.Columns("CMD_SPLN_REQ_ID"),
'					 REQDTDetail.Columns("Start_Year"),
'			         REQDTDetail.Columns("Unit_of_Measure"),
'					 REQDTDetail.Columns("Account")
'					 }
				'REQDTDetail.PrimaryKey = primaryKeyColumns
		 				
				Me.SplitFullDataTable(fullDT, REQDT, REQDTDetail)

				sqaREQReader.Update_XFC_CMD_SPLN_REQ(REQDT, sqa)
			 							
				sqaREQDetailReader.Update_XFC_CMD_SPLN_REQ_Details(REQDTDetail,sqa)
		 								

			End Using
		End Using
		
'BRApi.ErrorLog.LogMessage(si, "setupXFC_CMD_SPLN_REQ_Details. 1" & detailDT.Columns(0).ColumnName)		
       
	End Function
#End Region

#Region"Split dataset"
	Public Function SplitFullDataTable (ByRef fullDT As DataTable, ByRef REQDT As DataTable, ByRef REQDTDetail As DataTable) As Object
		' Split and translate data per row
		
		For Each row As DataRow In fullDT.Rows
	   
			' Handle REQDT translation and insertion/update
			Dim REQExists As Boolean = False
			Dim existingREQ_ID As String
			Dim existingCMD_SPLN_REQ_ID As String
			Dim MappingForREQ As New Dictionary(Of String, String)
			MappingForREQ = GetMappingForREQ()
		    Dim existingRowREQDT As DataRow = REQDT.Rows.Find(row("CMD_SPLN_REQ_ID")) '***DEV NOTE: TO DO this will have to be modified to match on other fields
			    
			If existingRowREQDT IsNot Nothing Then
				REQExists = True
				existingCMD_SPLN_REQ_ID = existingRowREQDT("CMD_SPLN_REQ_ID")
				existingREQ_ID = existingRowREQDT("REQ_ID")
			End If
		      
		    ' Create a new row
	        Dim newRowREQDT As DataRow = REQDT.NewRow()
			
	        For Each fullCol As KeyValuePair (Of String, String) In MappingForREQ
				If REQDT.Columns(fullCol.Value).DataType Is GetType(DateTime) Then
                    Dim tempDate As DateTime
                    If DateTime.TryParse(row(fullCol.Key), tempDate) Then
                        newRowREQDT(MappingForREQ(fullCol.Key)) = row(fullCol.Key)
                    Else
                        newRowREQDT(MappingForREQ(fullCol.Key)) = DBNull.Value
                    End If
                Else
					newRowREQDT(MappingForREQ(fullCol.Key)) = row(fullCol.Key)
				End If 

	        Next

			'If it is an existing row we will keep the original REC_ID. All the data will be from the file
			If REQExists Then
				 Me.UpdateExistingREQIDs(newRowREQDT, existingCMD_SPLN_REQ_ID, existingREQ_ID)					 
			End If
		    
			REQDT.Rows.Add(newRowREQDT)

	        ' Create a new row
			Dim MappingForREQDetailsObligation As New Dictionary(Of String, String)
			MappingForREQDetailsObligation = GetMappingForREQDetailsObligation()
	        Dim newRowREQDTDetailObligation As DataRow = REQDTDetail.NewRow()
	        For Each fullCol As String In GetMappingForREQDetailsObligation.Keys
	            newRowREQDTDetailObligation(GetMappingForREQDetailsObligation(fullCol)) = row(fullCol)
	        Next
			'Map funding to the regular FYDEP
	
			newRowREQDTDetailObligation("Account") = "Obligations"
			newRowREQDTDetailObligation("UNIT_OF_MEASURE") = "Funding"
			newRowREQDTDetailObligation("Quarter1") = convert.ToInt32(row("OBL_M1")) + convert.ToInt32(row("OBL_M2")) + convert.ToInt32(row("OBL_M3"))
			newRowREQDTDetailObligation("Quarter2") = convert.ToInt32(row("OBL_M4")) + convert.ToInt32(row("OBL_M5")) + convert.ToInt32(row("OBL_M6"))
			newRowREQDTDetailObligation("Quarter3") = convert.ToInt32(row("OBL_M7")) + convert.ToInt32(row("OBL_M8")) + convert.ToInt32(row("OBL_M9"))
			newRowREQDTDetailObligation("Quarter4") = convert.ToInt32(row("OBL_M10")) + convert.ToInt32(row("OBL_M11")) + convert.ToInt32(row("OBL_M12"))
			If REQExists Then
				Me.UpdateExistingREQIDs(newRowREQDTDetailObligation, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 				 
			End If
			REQDTDetail.Rows.Add(newRowREQDTDetailObligation)
		   
			
			' Handle REQDTDetail Items 

			Dim MappingForREQDetailsCommitment As New Dictionary(Of String, String)
			MappingForREQDetailsCommitment = GetMappingForREQDetailsCommitment()
	        ' Create a new row
	        Dim newRowREQDTDetailCommitment As DataRow = REQDTDetail.NewRow()
	        For Each fullCol As String In MappingForREQDetailsCommitment.Keys
	            newRowREQDTDetailCommitment(MappingForREQDetailsCommitment(fullCol)) = row(fullCol)
	        Next
		
			newRowREQDTDetailCommitment("Account") = "Commitments"
			newRowREQDTDetailCommitment("UNIT_OF_MEASURE") = "Funding"
			newRowREQDTDetailCommitment("Quarter1") = convert.ToInt32(row("COM_M1")) + convert.ToInt32(row("COM_M2")) + convert.ToInt32(row("COM_M3"))
			newRowREQDTDetailCommitment("Quarter2") = convert.ToInt32(row("COM_M4")) + convert.ToInt32(row("COM_M5")) + convert.ToInt32(row("COM_M6"))
			newRowREQDTDetailCommitment("Quarter3") = convert.ToInt32(row("COM_M7")) + convert.ToInt32(row("COM_M8")) + convert.ToInt32(row("COM_M9"))
			newRowREQDTDetailCommitment("Quarter4") = convert.ToInt32(row("COM_M10")) + convert.ToInt32(row("COM_M11")) + convert.ToInt32(row("COM_M12"))
			
		    If REQExists Then
				Me.UpdateExistingREQIDs(newRowREQDTDetailCommitment, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
			End If
			REQDTDetail.Rows.Add(newRowREQDTDetailCommitment)
			
			If CheckComCarryover(row) Then 

				Dim MappingForREQDetailsCommitmentCarryover As New Dictionary(Of String, String)
				MappingForREQDetailsCommitmentCarryover = GetMappingForREQDetailsCarryoverCommitment()
		        ' Create a new row
		        Dim newRowREQDTDetailCommitmentCarryover As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetailsCommitmentCarryover.Keys
		            newRowREQDTDetailCommitmentCarryover(MappingForREQDetailsCommitmentCarryover(fullCol)) = row(fullCol)
		        Next

				newRowREQDTDetailCommitmentCarryover("Account") = "Commitments"
				newRowREQDTDetailCommitmentCarryover("UNIT_OF_MEASURE") = "Funding"
				newRowREQDTDetailCommitmentCarryover("Fiscal_Year") = row("Cycle") + 1
'brapi.ErrorLog.LogMessage(si,"CM: " & row("Cycle") & " - " & row("Cycle") + 1)
				newRowREQDTDetailCommitmentCarryover("Yearly") = row("COM_Carryover")
				
			    If REQExists Then
					Me.UpdateExistingREQIDs(newRowREQDTDetailCommitmentCarryover, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
				End If
				REQDTDetail.Rows.Add(newRowREQDTDetailCommitmentCarryover)
			End If 
			
			
			If CheckOblCarryover(row) Then
'brapi.ErrorLog.LogMessage(si,"inside Oblig Carryover if")
				Dim MappingForREQDetailsObligationCarryover As New Dictionary(Of String, String)
				MappingForREQDetailsObligationCarryover = GetMappingForREQDetailsCarryoverObligation()
		        ' Create a new row
		        Dim newRowREQDTDetailObligationCarryover As DataRow = REQDTDetail.NewRow()
		        For Each fullCol As String In MappingForREQDetailsObligationCarryover.Keys
		            newRowREQDTDetailObligationCarryover(MappingForREQDetailsObligationCarryover(fullCol)) = row(fullCol)
		        Next

				newRowREQDTDetailObligationCarryover("Account") = "Obligations"
				newRowREQDTDetailObligationCarryover("UNIT_OF_MEASURE") = "Funding"
				newRowREQDTDetailObligationCarryover("Fiscal_Year") = row("Cycle") + 1
				newRowREQDTDetailObligationCarryover("Yearly") = row("OBL_Carryover")
			    If REQExists Then
					Me.UpdateExistingREQIDs(newRowREQDTDetailObligationCarryover, existingCMD_SPLN_REQ_ID, existingREQ_ID)	 
				End If
				REQDTDetail.Rows.Add(newRowREQDTDetailObligationCarryover)
			End If 

			

			
			' Handle REQDTDetail Quantity
					
			
			
		Next
'BRApi.ErrorLog.LogMessage(si,"In Split* 7")

	End Function
	
	Public Sub UpdateExistingREQIDs(ByRef dr As DataRow, ByVal existingCMD_SPLN_REQ_ID As String, ByVal existingREQ_ID As String )
		dr("CMD_SPLN_REQ_ID") = existingCMD_SPLN_REQ_ID
		dr("REQ_ID") = existingREQ_ID
	End Sub
#End Region

#Region	"Get Mapping XFC_CMD_SPLN_REQ_Details"
	Public Function GetMappingForREQDetailsObligation() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_SPLN_REQ_ID", "CMD_SPLN_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Account","Account"},
				{"Flow","Flow"},
				{"FundCode","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"UD5","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Fiscal_Year"},
				{"OBL_M1","Month1"},
				{"OBL_M2","Month2"},
				{"OBL_M3","Month3"},
				{"OBL_M4","Month4"},
				{"OBL_M5","Month5"},
				{"OBL_M6","Month6"},
				{"OBL_M7","Month7"},
				{"OBL_M8","Month8"},
				{"OBL_M9","Month9"},
				{"OBL_M10","Month10"},
				{"OBL_M11","Month11"},
				{"OBL_M12","Month12"},
				{"Quarter1","Quarter1"},
				{"Quarter2","Quarter2"},
				{"Quarter3","Quarter3"},
				{"Quarter4","Quarter4"},
				{"Yearly","Yearly"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
	
	Public Function GetMappingForREQDetailsCommitment() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_SPLN_REQ_ID", "CMD_SPLN_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Account","Account"},
				{"Flow","Flow"},
				{"FundCode","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"UD5","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Fiscal_Year"},
				{"COM_M1","Month1"},
				{"COM_M2","Month2"},
				{"COM_M3","Month3"},
				{"COM_M4","Month4"},
				{"COM_M5","Month5"},
				{"COM_M6","Month6"},
				{"COM_M7","Month7"},
				{"COM_M8","Month8"},
				{"COM_M9","Month9"},
				{"COM_M10","Month10"},
				{"COM_M11","Month11"},
				{"COM_M12","Month12"},
				{"Quarter1","Quarter1"},
				{"Quarter2","Quarter2"},
				{"Quarter3","Quarter3"},
				{"Quarter4","Quarter4"},
				{"Yearly","Yearly"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
	
	Public Function GetMappingForREQDetailsCarryoverObligation() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_SPLN_REQ_ID", "CMD_SPLN_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Account","Account"},
				{"Flow","Flow"},
				{"FundCode","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"UD5","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Fiscal_Year"},
				{"OBL_M12","Yearly"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function
	
	Public Function GetMappingForREQDetailsCarryoverCommitment() As Object
		Dim REQ_DetailsColMapping As New Dictionary(Of String, String) From{
				{"CMD_SPLN_REQ_ID", "CMD_SPLN_REQ_ID"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},	
				{"UNIT_OF_MEASURE","Unit_of_Measure"},
				{"FundCenter","Entity"},
				{"IC","IC"},
				{"Account","Account"},
				{"Flow","Flow"},
				{"FundCode","UD1"},
				{"MDEP","UD2"},
				{"APE9","UD3"},
				{"DollarType","UD4"},
				{"UD5","UD5"},
				{"ObjectClass","UD6"},
				{"UD7","UD7"},
				{"UD8","UD8"},
				{"Cycle","Fiscal_Year"},
				{"COM_M12","Yearly"},
				{"AllowUpdate","AllowUpdate"},
				{"Create_Date","Create_Date"},
				{"Create_User","Create_User"},
				{"Update_Date","Update_Date"},
				{"Update_User","Update_User"}		
		}
		
		Return REQ_DetailsColMapping
	End Function


#Region	"Get Mapping XFC_CMD_SPLN_REQ"
	Public Function GetMappingForREQ() As Object
		Dim REQColMapping As New Dictionary(Of String, String) From{
				{"CMD_SPLN_REQ_ID", "CMD_SPLN_REQ_ID"},
				{"REQ_ID", "REQ_ID"},
				{"REQ_ID_Type", "REQ_ID_Type"},
				{"WFScenario_Name", "WFScenario_Name"},
				{"WFCMD_Name", "WFCMD_Name"},
				{"WFTime_Name", "WFTime_Name"},
				{"Title","Title"},
				{"Description","Description"},
				{"Justification","Justification"},
				{"FundCenter","Entity"},
				{"FundCode","APPN"},
				{"MDEP","MDEP"},
				{"APE9","APE9"},
				{"DollarType","Dollar_Type"},
				{"ObjectClass","Obj_Class"},
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
				{"CME","CME"},
				{"COREmail","COR_Email"},
				{"POCEmail","POC_Email"},
				{"ReviewingPOCEmail","Review_POC_Email"},
				{"MDEPFunctionalEmail","MDEP_Functional_Email"},
				{"NotificationEmailList","Notification_List_Emails"},
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
	Public Function CheckComCarryover(row As DataRow) As Boolean
		If ((Not IsDbNull(row("COM_Carryover")) AndAlso Not String.IsNullOrEmpty(row("COM_Carryover").ToString())))
'BRApi.ErrorLog.LogMessage(si,"True")			
			Return True
		Else
'BRApi.ErrorLog.LogMessage(si,"False")			
			Return False
		End If
	End Function
	
		Public Function CheckOblCarryover(row As DataRow) As Boolean
		If ((Not IsDbNull(row("OBL_Carryover")) AndAlso Not String.IsNullOrEmpty(row("OBL_Carryover").ToString())))
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
