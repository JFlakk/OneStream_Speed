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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class GBL_XFTV_Helpers

        Public Shared Function Dirty_Column_List(si As SessionInfo, xftvRow As TableViewRow) As List(Of String)
            Dim dirtyColList As List(Of String) = xftvRow.Items.Keys.Cast(Of String)().Where(Function(k)
                Dim xftvCol = xftvRow.Item(k)
                If xftvCol Is Nothing Then 
					
            
					Return False
				Else
					'Brapi.ErrorLog.LogMessage(si,$"DEBUG: Column '{k}' exists. IsDirty() = { CBool(xftvCol.IsDirty())}.")
					Return CBool(xftvCol.IsDirty())
				End If
			End Function).ToList()
			Return dirtyColList
        End Function
		
        Public Shared Function Convert_xftvCol_to_DbValue(si As SessionInfo, xftvCol As TableViewColumn) As Object
            Try
'BRApi.ErrorLog.LogMessage(si, $"Hit: {xftvCol.DataType.ToString()} - {xftvCol.Value}")
				If xftvCol Is Nothing Then
	                Return DBNull.Value
                Else
                    Select Case xftvCol.DataType.ToString()
                        Case "Text"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value
                        Case "Int16", "Int32", "Int64"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToInt()
                        Case "Decimal", "Double"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToDecimal()
                        Case "Boolean"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToBool()
                        Case "DateTime"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
							Return DateTime.ParseExact(xftvCol.Value,"MM/dd/yyyy",CultureInfo.InvariantCulture)
                        Case "Guid"
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToGuid()
                        Case Else
                            Return If(xftvCol.Value Is Nothing, DBNull.Value, xftvCol.Value)
                    End Select
                End If
            ' Obtain Value and DataType properties by reflection (works for the XFTV cell)
            Catch ex As Exception
	           ' BRApi.ErrorLog.LogMessage(si, $"ConvertxftvColtoDbValue: unexpected error: {ex.Message}")
	            Return DBNull.Value
            End Try
        End Function
		
		
		Public Shared Sub ValidateXFTVRequiredFields(si As SessionInfo, xftv As TableView, requiredColumns As IEnumerable(Of String))
			If xftv Is Nothing Then
				Throw New XFException("Validation error: TableView is missing.")
			End If
			
			If xftv Is Nothing Then
				GBL_Helpers.SetUFRValid(si, False)
				Throw New XFException("Validation Error: TableView is Missing.")
			End If
			
			Dim rowIndex As Integer = 0
			Dim allErrors As New List(Of String)()
			
			
			For Each xftvRow As TableViewRow In xftv.Rows
				Dim rowErrors As New List(Of String)()
				
				For Each colName As String In requiredColumns 
					Dim val As Object = xftvRow.Item(colName).Value
										
					If val Is Nothing OrElse val Is DBNull.Value OrElse String.IsNullOrWhiteSpace(val.ToString()) Then
						rowErrors.Add(colName)
					End If
					Brapi.ErrorLog.LogMessage(si, "colName = " & colName)
					
					
				Next
				
				Brapi.ErrorLog.LogMessage(si, "Row" & (rowIndex + 1).ToString() & " - Row Errors Count = " & rowErrors.Count.ToString())
				
				If rowErrors.Count > 0 Then
					Dim msg As String = String.Format($"Row {0}: Missing required fields: {1}", (rowIndex +1).ToString(), String.Join(", ", rowErrors))
					allErrors.Add(msg)
				End If
				
				rowIndex +=1
			Next
			
'			BRApi.ErrorLog.LogMessage(si, "All Errors Count: " & String.Join(" || ", allErrors.Count.ToString()))
			
			If allErrors.Count > 0 Then
				Dim fullMsg As String = String.Join(" | ", allErrors)
				BRApi.ErrorLog.LogMessage(si, "XFTV Validation: FAILED - UFR_VALID set to 0")
				Throw New XFException(fullMsg)
			End If
			
			GBL_Helpers.SetUFRValid(si, True)
				BRApi.ErrorLog.LogMessage(si, "XFTV Validation: PASSED - UFR_VALID set to 1")
		End Sub

	End Class
End Namespace