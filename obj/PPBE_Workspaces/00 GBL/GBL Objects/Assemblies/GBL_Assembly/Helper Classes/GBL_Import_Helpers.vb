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
		
		Public Shared Function ValidateEntMbr(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByVal command As String, ByVal entity As String) As String
			
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & command)
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & command & ".member.base", True)

			If Not membList.Exists(Function(x) x.Member.Name = entity) Then 
'				isFileValid = False
'				currREQ.valid = False
'				currREQ.ValidationError = "Error: Invalid Entity: " & currREQ.Entity & " does not exist in command " & currREQ.command
			End If
			
		End Function
		
'		Public Shared Function ValidateMbr(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByRef currREQ As CMD_PGM_Requirement) As String
			
'				If (Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem)) And (Not currREQ.CommitmentItem.XFEqualsIgnoreCase("None")) Then
'					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
'					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
'					If (membList.Count <> 1 ) Then 
'						isFileValid = False
'						currREQ.valid = False
'						currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
						
'					End If
'				End If 
			
'		End Function
		
'		Public Shared Function ValidateNumeric(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByRef currREQ As CMD_PGM_Requirement) As String
			
'				If (Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem)) And (Not currREQ.CommitmentItem.XFEqualsIgnoreCase("None")) Then
'					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
'					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
'					If (membList.Count <> 1 ) Then 
'						isFileValid = False
'						currREQ.valid = False
'						currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
						
'					End If
'				End If 
			
'		End Function
		
'		Public Shared Function ValidateDate(ByVal si As SessionInfo,  ByVal globals As BRGlobals, ByRef currREQ As CMD_PGM_Requirement) As String
			
'				If (Not String.IsNullOrWhiteSpace(currREQ.CommitmentItem)) And (Not currREQ.CommitmentItem.XFEqualsIgnoreCase("None")) Then
'					objDimPk  = BRApi.Finance.Dim.GetDimPk(si, "U6_CommitmentItem")
'					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "U6#" &currREQ. CommitmentItem & ".member.base", True)
'					If (membList.Count <> 1 ) Then 
'						isFileValid = False
'						currREQ.valid = False
'						currREQ.ValidationError = "Error: Invalid Cost Category value: " & currREQ.CommitmentItem
						
'					End If
'				End If 
			
'		End Function
		
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
		
		Public Shared Function GetCsvDataReader(ByVal sr As IO.StreamReader, ByVal delimiter As Char, ByVal mappings As List(Of GBL_ColumnMap)) As DataTable
		    Dim dt As New DataTable()
		    
		    Dim firstLine As String = sr.ReadLine()
		    If firstLine Is Nothing Then Throw New InvalidDataException("File is empty.")
		    
		    Dim csvHeaders As String() = firstLine.Split(delimiter).Select(Function(h) h.Trim()).ToArray()
		    Dim headerMapDict As New Dictionary(Of Integer, GBL_ColumnMap)()
		    
		    ' 1. Set up the DataTable columns based on the MAPPING
		    For Each mapping As GBL_ColumnMap In mappings
		        dt.Columns.Add(mapping.DataTableColumnName, mapping.DataType)
		    Next
		    
		    ' 2. Create a fast lookup map for CSV column index to its mapping
		    For i As Integer = 0 To csvHeaders.Length - 1
		        Dim header As String = csvHeaders(i)
		        Dim mapping As GBL_ColumnMap = mappings.FirstOrDefault(Function(m) m.CsvHeaderName.Equals(header, StringComparison.OrdinalIgnoreCase))
		        
		        If mapping IsNot Nothing Then
		            headerMapDict.Add(i, mapping)
		        Else
		            ' Optional: Add a column for unmapped data, or skip (here we skip)
		        End If
		    Next
		
		    ' Optional: Add a column to track errors
		    dt.Columns.Add("ParseError", GetType(String))
		
		    ' --- Phase 2: Read Data and Perform Validation ---
		
		    While Not sr.EndOfStream
		        Dim line As String = sr.ReadLine()
		        Dim values As String() = line.Split(delimiter)
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
		                    Dim rawValue As String = values(i).Trim()
		                    
'		                    ' Validate and convert the value using the helper function
'		                    Dim resultTuple = ValidateAndConvert(rawValue, mapping)
		                    
'		                    If resultTuple.Item2.IsValid Then
'		                        newRow(mapping.DataTableColumnName) = resultTuple.Item1
'		                    Else
'		                        ' Validation failed
'		                        errorMessages.Add($"{mapping.DataTableColumnName}: {resultTuple.Item2.ErrorMessage}")
'		                        hasError = True
'		                    End If
		                End If
		            Next
		        End If
		
		        ' Add the row (either valid data or an error record)
		        If hasError Then
		            ' If there's an error, log it in the ParseError column
		            newRow("ParseError") = String.Join("; ", errorMessages)
		        End If
		        
		        dt.Rows.Add(newRow)
		    End While
		    
		    Return dt
		End Function

	End Class
End Namespace
