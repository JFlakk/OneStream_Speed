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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class GBL_Import_Helpers

		Public Shared Function CleanUpSpecialCharacters(fieldValue As String) As String
		    fieldValue = fieldValue.Replace("???", " ")
		    
		    Dim allowedChars As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789,""@!#$%'()+=_.:<>?~-[]*^/" & vbCrLf & vbLf ' The allowed set of characters
		    Dim result As New StringBuilder()
		    
		    For Each c As Char In fieldValue
		        If allowedChars.Contains(c) Then
		            result.Append(c)
		        Else
		            result.Append(" ") ' or just skip the character
		        End If
		    Next
		    
		    Return result.ToString().Trim() ' Add a final trim for cleanliness
		End Function	
		
	    Public Shared Function ParseCsvLine(line As String) As String()
	
			'Use reg expressions to split the csv.
			'The expression accounts for commas that are with in "" to treat them as data.
			Dim pattern As String = ",(?=(?:[^""]*""[^""]*"")*[^""]*$)"
			Dim fields As String () = Regex.Split(line, pattern)
			Return fields
			
	    End Function	
		
		Public Shared Function PrepImportFile(ByVal si As SessionInfo, ByVal filePath As String) As StringReader
		    Try
		        ' The XFFileInfo constructor will handle the file path.
		        ' If the file doesn't exist, GetFile will throw an exception.
		        Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, filePath)
		        
		        ' BRApi.FileSystem.GetFile retrieves the file content.
		        Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, objXFFileInfo.FileSystemLocation, objXFFileInfo.FullName, True, True)
		
		        ' Read the file bytes directly and create a StringReader.
		        ' This is a concise way to create the reader from the content.
		        Dim sFileText As String = System.Text.Encoding.UTF8.GetString(objXFFileEx.XFFile.ContentFileBytes)
		        Dim fullFile As New StringReader(sFileText)
		
		        Return fullFile
		
		    Catch ex As Exception
		        ' If the file does not exist or another error occurs,
		        ' a custom exception is thrown to provide a clear message.
		        Throw New XFUserMsgException(si, New Exception("File " & filePath & " does not exist or an error occurred during import.", ex))
		    End Try
		End Function
		
		Public Shared Function GetCsvDataReader(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal sr As IO.StreamReader, ByVal delimiter As Char, ByVal mappings As List(Of GBL_ColumnMap), Optional ByVal numOfColumns As Integer = 0) As DataTable
		    Dim command As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim hasError As Boolean = False
			Dim dt As New DataTable()
			Dim firstLine As String = sr.ReadLine()
			If firstLine Is Nothing Then Throw New InvalidDataException("File is empty.")
			' Use ParseCsvLine to respect quoted fields
			Dim csvHeaders As String() = ParseCsvLine(firstLine).Select(Function(h) h.Trim().Trim(""""c)).ToArray()
			Dim headerMapDict As New Dictionary(Of Integer, GBL_ColumnMap)()
			Dim structureErrMessage As String = ""
			
			If numOfColumns > 0 Then
				If csvHeaders.Length <> numOfColumns Then 
					hasError = True
					structureErrMessage = $"Invalid number of columns ({csvHeaders.Length}). Required number of columns is ({numOfColumns})."
				End If
			End If
			' 1. Set up the DataTable columns based on the MAPPING
'BRApi.ErrorLog.LogMessage(si, "start Header: " & firstLine)						
'			For Each mapping As GBL_ColumnMap In mappings
'				dt.Columns.Add(mapping.DataTableColumnName, mapping.DataType)
'			Next
'BRApi.ErrorLog.LogMessage(si, "End")			
			' 2. Create a fast lookup map for CSV column index to its mapping; also try to associate CMD_TGT_Dist if present on the mapping
			For i As Integer = 0 To csvHeaders.Length - 1
				Dim header As String = csvHeaders(i)
				Dim mapping As GBL_ColumnMap = mappings.FirstOrDefault(Function(m) m.CsvHeaderName.Equals(header, StringComparison.OrdinalIgnoreCase))

				If mapping IsNot Nothing Then
					headerMapDict.Add(i, mapping)
					dt.Columns.Add(mapping.DataTableColumnName, mapping.DataType)
				Else
					hasError = True
					structureErrMessage = $"Invalid column name '{header}'."
				End If
			Next
			' Optional: Add a column to track errors
			dt.Columns.Add("Invalid Errors", GetType(String))

			If hasError Then
				Dim errorRow As DataRow = dt.NewRow()
				errorRow("Invalid Errors") = structureErrMessage
				dt.Rows.Add(errorRow)
				Return dt
			End If
			' --- Phase 2: Read Data and Perform Validation ---
			While Not sr.EndOfStream
				Dim line As String = sr.ReadLine()
				If line Is Nothing Then Continue While
				Dim values As String() = ParseCsvLine(line)
				Dim newRow As DataRow = dt.NewRow()
				
				Dim errorMessages As New List(Of String)()
		
				' Ensure the row has at least as many fields as we found headers
				If values.Length < csvHeaders.Length Then
					errorMessages.Add($"Row has fewer fields ({values.Length}) than headers ({csvHeaders.Length}).")
					hasError = True
				Else
					For i As Integer = 0 To csvHeaders.Length - 1
						If headerMapDict.ContainsKey(i) Then
							
							Dim mapping As GBL_ColumnMap = headerMapDict(i)
							Dim rawValue As String = If(i < values.Length, values(i).Trim().Trim(""""c), String.Empty)

							newRow(i) = rawValue
'If i < 5 Then BRApi.ErrorLog.LogMessage(si,i & " @rawValue: " & rawValue & ", valTypeStr: " & mapping.ValType.ToString() & ", dependency: " & mapping.Dependency & " ;header nam:e " & headerMapDict(i).CsvHeaderName & "; mapping name: " & mapping.CsvHeaderName & " ;newrow(i): " & newRow(i) & " ; Item(i).ColumnName : " & newRow.Table.Columns.Item(i).ColumnName & " ; newrow(APPN) : " & newRow("APPN") & " ; newrow(REQ_ID) : " & newRow("REQ_ID"))
							Dim valTypeStr As String = String.Empty
							If mapping IsNot Nothing AndAlso mapping.ValType IsNot Nothing Then
								valTypeStr = mapping.ValType.ToString().Trim().ToLowerInvariant()
							End If
							
							' 1) Required check
							If mapping.ValRequired AndAlso String.IsNullOrWhiteSpace(rawValue) Then
								errorMessages.Add($"{mapping.DataTableColumnName}: value is required.")
								hasError = True
								Continue For
							End If
							
							If mapping.MaxLengthValue > 0 AndAlso rawValue.Length > mapping.MaxLengthValue Then
								errorMessages.Add($"{mapping.DataTableColumnName}: length {rawValue.Length} exceeds maximum {mapping.MaxLengthValue}.")
								hasError = True
							End If
							Select Case valTypeStr.ToLower
								Case "wfcube"
									rawValue = rawValue.Trim()
								Case "wftime"
									rawValue = rawValue.ToUpperInvariant()
								Case "ent_member"
									rawValue = CleanUpSpecialCharacters(rawValue)
'BRApi.ErrorLog.LogMessage(si, "Entity: " & mapping.DataTableColumnName & " = " & rawValue)									
									Dim entVal As String = ValidateEntMbr(si, globals, command, rawValue)
									If entVal.StartsWith("Invalid_") Then
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid entity member '{rawValue}'.")
										hasError = True
									Else
										rawValue = entVal
										'This is to attach _General for parent members
										newRow(i) = rawValue
									End If
								Case "member"
									rawValue = CleanUpSpecialCharacters(rawValue)
'BRApi.ErrorLog.LogMessage(si, "member Before mapping.ValDim: " & mapping.DataTableColumnName & " = " & rawValue)																											
									Dim mbrVal As String = ValidateMbrExists(si, globals, mapping.ValDim, mapping.ValToken, rawValue)
									If mbrVal.StartsWith("Invalid_") Then
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid member '{rawValue}'.")
										hasError = True
									Else
										rawValue = mbrVal
										newRow(i) = rawValue
									End If
'BRApi.ErrorLog.LogMessage(si, "After mapping.ValDim: " & mapping.DataTableColumnName & " = " & rawValue)	
								Case "u3_member"
'BRApi.ErrorLog.LogMessage(si, "U3 member start mapping.ValDim: " & mapping.DataTableColumnName & " = " & rawValue)	
									rawValue = CleanUpSpecialCharacters(rawValue)
									Dim depedentIndex As Integer = globals.GetInt32Value("U3_MemberDependentIndex",-1)
									Dim dependentMap As GBL_ColumnMap = globals.GetObject("U3_MemberDependentMap")
									
									'Get the dependent index and mapping
									If Not String.IsNullOrWhiteSpace(mapping.Dependency) And  depedentIndex = -1 Then 
										For Each kvp As KeyValuePair(Of Integer, GBL_ColumnMap) In headerMapDict
								            If kvp.Value.CsvHeaderName = mapping.Dependency Then
								                depedentIndex = kvp.Key ' Return the matching key
												dependentMap = kvp.Value
												globals.SetInt32Value("U3_MemberDependentIndex", depedentIndex)
												globals.SetObject("U3_MemberDependentMap", dependentMap)
								            End If
								        Next
									End If
									Dim fullU3Member As String = ""
									If depedentIndex > -1 Then
										Dim dependentValue As String = values(depedentIndex)
										
										If dependentMap.ValDim.Equals("U1_APPN", StringComparison.OrdinalIgnoreCase) Then 
											fullU3Member = $"{dependentValue}_{rawValue}"
										Else 
											'Get the APPN parent
											Dim UD1objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U1_FundCode")
											Dim lsAncestorListU1 As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1objDimPk, "U1#" & dependentValue & ".Ancestors.Where(MemberDim = 'U1_APPN')", True)
											If ((Not lsAncestorListU1 Is Nothing) And (lsAncestorListU1.Count > 1)) Then
						   						Dim dependentAPPN = lsAncestorListU1(0).Member.Name
												fullU3Member = $"{dependentAPPN}_{rawValue}"
											Else
												fullU3Member = rawValue
											End If
										End If
									Else
										fullU3Member = rawValue
									End If
									
'BRApi.ErrorLog.LogMessage(si, "U3_member Before mapping.ValDim: " & mapping.DataTableColumnName & ", depedentIndex = " & depedentIndex & ", Dim = " &  mapping.ValDim &" = " & fullU3Member)																											
									Dim mbrVal As String = ValidateMbrExists(si, globals, mapping.ValDim, mapping.ValToken, fullU3Member)
									If mbrVal.StartsWith("Invalid_") Then
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid member '{fullU3Member}'.")
										hasError = True
									End If	
								Case "numeric"
									' Remove common thousands separators so numeric parsing is easier
									rawValue = rawValue.Replace(",", String.Empty).Replace(" ", String.Empty)
									If Not ValidateNumeric(si, rawValue)
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid number '{rawValue}'.")
										hasError = True
									End If
								Case "date"
									' leave as-is; parsing/validation happens later
									If Not ValidateDate(si, rawValue)
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid date '{rawValue}'.")
										hasError = True
									End If
								Case Else
									' Unknown ValType - no special handling
							End Select

						End If
					Next
				End If
	
				' Add the row (either valid data or an error record)
				If hasError Then
					' If there's an error, log it in the ParseError column
					newRow("Invalid Errors") = String.Join("; ", errorMessages)
				End If
				dt.Rows.Add(newRow)
			End While
	    
		    Return dt
		End Function
		
#Region "Validation Functions"

'				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'				'BRApi.errorlog.logmessage(si,$"Hit {req_IDs}")
'				If String.IsNullOrWhiteSpace(new_Status) Then 

'					Return dbExt_ChangedResult
'				Else
'					Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'					Using connection As New SqlConnection(dbConnApp.ConnectionString)
'						connection.Open()
'						Dim sqa_xfc_cmd_pgm_req = New SQA_XFC_CMD_PGM_REQ(connection)
'						Dim SQA_XFC_CMD_PGM_REQ_DT = New DataTable()
'						Dim sqa_xfc_cmd_pgm_req_details = New SQA_XFC_CMD_PGM_REQ_DETAILS(connection)
'						Dim SQA_XFC_CMD_PGM_REQ_DETAILS_DT = New DataTable()
'						Dim sqa = New SqlDataAdapter()

'					'Fill the DataTable With the current data From FMM_Dest_Cell
'					Dim sql As String = $"SELECT * 
'										FROM XFC_CMD_PGM_REQ 
'										WHERE WFScenario_Name = @WFScenario_Name
'										AND WFCMD_Name = @WFCMD_Name
'										AND WFTime_Name = @WFTime_Name"
'					If num_Reqs > 1
'						sql &= " AND REQ_ID In (@REQ_ID)"
'					Else
'						sql &= " AND REQ_ID = @REQ_ID"
'					End If

'					Dim sqlparams As SqlParameter() = {
'						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
'						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
'						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
'						New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = req_IDs}
		
		Public Shared Function ValidateEntMbr(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal command As String, ByVal entity As String) As String
			If globals.GetStringValue($"Valid_EntMbr_{entity}","NA") = "NA"	
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & command)
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				'membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & command & ".member.base", True)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & entity & ".member.base", True)
	
				If Not membList.Exists(Function(x) x.Member.Name = entity) Then 
					globals.SetStringValue($"Valid_EntMbr_{entity}",$"Invalid_{entity}")
					Return $"Invalid_{entity}"
				Else
					If membList.Count > 1 Then 
						'entity = $"{entity}_General"
						entity = $"{entity}"
					End If	
					globals.SetStringValue($"Valid_EntMbr_{entity}",entity)
					Return entity
				End If
			Else
				Return globals.GetStringValue($"Valid_EntMbr_{entity}","NA") 
			End If
			
		End Function
		
		Public Shared Function ValidateMbrExists(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal Dimension As String, ByVal Token As String, ByVal Value As String) As String
'BRApi.ErrorLog.LogMessage(si, $"_{Dimension}_{Token}_{Value}_")			
			If String.IsNullOrWhiteSpace(Value) Then
				Return "None"
			End If
			
			If globals.GetObject($"AllMembers_{Dimension}_{Token}") Is Nothing Then
				Dim cube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim scenarioType As String = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Name

				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, Dimension)
				Dim allMembers As List(Of memberinfo) = New List(Of MemberInfo)
				allMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, $"{Token}Top.base.Options(Cube = {cube}, ScenarioType = {scenarioType})", True)
				globals.SetObject($"AllMembers_{Dimension}_{Token}",allMembers)
'BRApi.ErrorLog.LogMessage(si, $"Creating {Token}Top.base.Options(Cube = {cube}, scenario = {scenarioType}) , count : {allMembers.Count}")				
			End If
			If globals.GetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}","NA") = "NA" Then
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, Dimension)
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				'membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, Token & Value & ".member.base", True)
				membList = globals.GetObject($"AllMembers_{Dimension}_{Token}")
				If Not (membList.Any(Function(m) m.member.Name.ToLower = Value.ToLower))
				'If (membList.Count <> 1 ) Then
					globals.SetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}",$"Invalid_{Dimension}_{Token}_{Value}")
					Return $"Invalid_{Dimension}_{Token}_{Value}"
				Else
					globals.SetStringValue($"Valid_Mbr_{Dimension}_{Token}{Value}",Value)
					Return Value
				End If
			Else
				Return globals.GetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}","NA")
			End If

		End Function

		Public Shared Function ValidateU3MbrExists(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal Dimension As String, ByVal Token As String, ByVal Value As String) As String
			If globals.GetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}","NA") = "NA" Then
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, Dimension)
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)

				'membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, Token & Value & ".member.base", True)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, Token & "Top.Base.Where(Name Contains " & Value & ")", True)
				
'BRApi.ErrorLog.LogMessage(si, "ValidateU3MbrExists script: " &  Token & "Top.Base.Where(Name Contains " & Value & "), membList count: " & membList.Count)
				If (membList.Count <> 0 ) Then
					globals.SetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}",$"Invalid_{Dimension}_{Token}_{Value}")
					Return $"Invalid_{Dimension}_{Token}_{Value}"
				Else
					globals.SetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}",Value)
					Return Value
				End If
			Else
				Return globals.GetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}","NA")
		End If

		End Function
		
        Public Shared Function ValidateNumeric(ByVal si As SessionInfo, ByVal value As String) As Boolean
            Try
				If Not String.IsNullOrWhiteSpace(value)
					If IsNumeric(value) Then
						Return True
					Else
						Return False
					End If
				Else
					Return True
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

        Public Shared Function ValidateDate(ByVal si As SessionInfo, ByVal value As String) As Boolean
            Try
				If Not String.IsNullOrWhiteSpace(value) Then
					If IsDate(value)
						Return True
					Else
						Return False
					End If
				Else
					Return True
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
#End Region

	End Class
End Namespace