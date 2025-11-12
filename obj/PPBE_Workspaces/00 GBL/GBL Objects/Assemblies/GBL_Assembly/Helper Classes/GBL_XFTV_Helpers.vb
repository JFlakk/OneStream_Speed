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
                    Return CBool(xftvCol.IsDirty())
				End If
			End Function).ToList()
			Return dirtyColList
        End Function
		
        Public Shared Function Convert_xftvCol_to_DbValue(si As SessionInfo, xftvCol As TableViewColumn) As Object
            Try
				BRApi.ErrorLog.LogMessage(si, $"Hit: {xftvCol.DataType.ToString()} - {xftvCol.Value}")
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

	End Class
End Namespace
