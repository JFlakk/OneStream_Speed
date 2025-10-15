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
		    
		    Dim allowedChars As String = "..." ' The allowed set of characters
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
		
		Public Shared Function GetCsvDataReader(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal sr As IO.StreamReader, ByVal delimiter As Char, ByVal mappings As List(Of GBL_ColumnMap)) As DataTable
		    Dim dt As New DataTable()
		    
			Dim firstLine As String = sr.ReadLine()
			If firstLine Is Nothing Then Throw New InvalidDataException("File is empty.")
		
			' Use ParseCsvLine to respect quoted fields
			Dim csvHeaders As String() = ParseCsvLine(firstLine).Select(Function(h) h.Trim().Trim(""""c)).ToArray()
			Dim headerMapDict As New Dictionary(Of Integer, GBL_ColumnMap)()
		
			' 1. Set up the DataTable columns based on the MAPPING
			For Each mapping As GBL_ColumnMap In mappings
				dt.Columns.Add(mapping.DataTableColumnName, mapping.DataType)
			Next
		
			' 2. Create a fast lookup map for CSV column index to its mapping; also try to associate CMD_TGT_Dist if present on the mapping
			For i As Integer = 0 To csvHeaders.Length - 1
				Dim header As String = csvHeaders(i)
				Dim mapping As GBL_ColumnMap = mappings.FirstOrDefault(Function(m) m.CsvHeaderName.Equals(header, StringComparison.OrdinalIgnoreCase))
		
				If mapping IsNot Nothing Then
					headerMapDict.Add(i, mapping)
				Else
					' No direct mapping found - skip column by default
				End If
			Next
		
			' Optional: Add a column to track errors
			dt.Columns.Add("Invalid Errors", GetType(String))
		
			' --- Phase 2: Read Data and Perform Validation ---
			While Not sr.EndOfStream
				Dim line As String = sr.ReadLine()
				If line Is Nothing Then Continue While
				Dim values As String() = ParseCsvLine(line)
				Dim newRow As DataRow = dt.NewRow()
				Dim hasError As Boolean = False
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

							Select Case valTypeStr
								Case "WFCube"
									rawValue = rawValue.Trim()
								Case "WFTime"
									rawValue = rawValue.ToUpperInvariant()
								Case "Ent_Member"
									rawValue = CleanUpSpecialCharacters(rawValue)
									Dim entVal As String = ValidateEntMbr(si, globals, "Ent_Member", rawValue)
									If entVal.StartsWith("Invalid_") Then
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid entity member '{rawValue}'.")
										hasError = True
									Else
										rawValue = entVal
									End If
								Case "Member"
									rawValue = CleanUpSpecialCharacters(rawValue)
									Dim mbrVal As String = ValidateMbrExists(si, globals, mapping.ValDim, mapping.ValToken, rawValue)
									If mbrVal.StartsWith("Invalid_") Then
										errorMessages.Add($"{mapping.DataTableColumnName}: invalid entity member '{rawValue}'.")
										hasError = True
									Else
										rawValue = mbrVal
									End If
								Case "numeric"
									' Remove common thousands separators so numeric parsing is easier
									rawValue = rawValue.Replace(",", String.Empty).Replace(" ", String.Empty)
								Case "date"
									' leave as-is; parsing/validation happens later
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
		
		Public Shared Function ValidateEntMbr(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal command As String, ByVal entity As String) As String
			If globals.GetStringValue($"Valid_EntMbr_{entity}","NA") = "NA"	
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & command)
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & command & ".member.base", True)
	
				If Not membList.Exists(Function(x) x.Member.Name = entity) Then 
					globals.SetStringValue($"Valid_EntMbr_{entity}",$"Invalid_{entity}")
					Return $"Invalid_{entity}"
				Else
					globals.SetStringValue($"Valid_EntMbr_{entity}",entity)
					Return entity
				End If
			Else
				Return globals.GetStringValue($"Valid_EntMbr_{entity}","NA") 
			End If
			
		End Function
		
		Public Shared Function ValidateMbrExists(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal Dimension As String, ByVal Token As String, ByVal Value As String) As String
			If globals.GetStringValue($"Valid_Mbr_{Dimension}_{Token}_{Value}","NA") = "NA" Then
				Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, Dimension)
				Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
				membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, Token & "#" & Value & ".member.base", True)

				If (membList.Count <> 1 ) Then
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
				If (Not String.IsNullOrWhiteSpace(value)) And (IsNumeric(value)) Then
						Return True
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

        Public Shared Function ValidateDate(ByVal si As SessionInfo, ByVal value As String) As Boolean
            Try
				If (Not String.IsNullOrWhiteSpace(value)) And (IsDate(value)) Then
						Return True
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
#End Region

	End Class
End Namespace
