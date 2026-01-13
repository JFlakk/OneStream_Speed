Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.Data.SqlClient
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
	''' <summary>
	''' Generic SQL Command Builder that dynamically generates INSERT/UPDATE/DELETE commands
	''' based on DataTable schema. This eliminates hardcoded SQL statements and provides
	''' automatic synchronization when schema changes occur.
	''' 
	''' This class works with any SqlConnection, including those created from:
	''' - BRApi.Database.CreateApplicationDbConnInfo(si)
	''' - BRApi.Database.CreateMergeDbConnInfo(si)
	''' - Any other database connection type
	''' </summary>
	Public Class GBL_SQL_Command_Builder
		Private ReadOnly _connection As SqlConnection
		Private ReadOnly _tableName As String
		Private ReadOnly _dataTable As DataTable
		Private ReadOnly _primaryKeyColumns As List(Of String)
		Private ReadOnly _excludeFromUpdate As List(Of String)
		Private ReadOnly _excludeFromInsert As List(Of String)

		''' <summary>
		''' Initialize the SQL Command Builder
		''' </summary>
		''' <param name="connection">SQL connection to use (can be from App DB or Merge DB)</param>
		''' <param name="tableName">Name of the table to generate commands for</param>
		''' <param name="dataTable">DataTable with schema information</param>
		Public Sub New(connection As SqlConnection, tableName As String, dataTable As DataTable)
			_connection = connection
			_tableName = tableName
			_dataTable = dataTable
			_primaryKeyColumns = New List(Of String)()
			_excludeFromUpdate = New List(Of String)()
			_excludeFromInsert = New List(Of String)()
		End Sub

		''' <summary>
		''' Set primary key column(s) for the table
		''' </summary>
		Public Sub SetPrimaryKey(ParamArray columnNames() As String)
			_primaryKeyColumns.Clear()
			_primaryKeyColumns.AddRange(columnNames)
		End Sub

		''' <summary>
		''' Set columns to exclude from UPDATE operations (e.g., primary keys, create date/user)
		''' </summary>
		Public Sub ExcludeFromUpdate(ParamArray columnNames() As String)
			_excludeFromUpdate.Clear()
			_excludeFromUpdate.AddRange(columnNames)
		End Sub

		''' <summary>
		''' Set columns to exclude from INSERT operations (e.g., identity columns)
		''' </summary>
		Public Sub ExcludeFromInsert(ParamArray columnNames() As String)
			_excludeFromInsert.Clear()
			_excludeFromInsert.AddRange(columnNames)
		End Sub

		''' <summary>
		''' Build INSERT command based on DataTable schema
		''' </summary>
		Public Function BuildInsertCommand(Optional transaction As SqlTransaction = Nothing) As SqlCommand
			Dim columns As New List(Of String)()
			Dim parameters As New List(Of String)()

			For Each col As DataColumn In _dataTable.Columns
				If _excludeFromInsert.Contains(col.ColumnName) Then
					Continue For
				End If

				columns.Add(col.ColumnName)
				parameters.Add("@" & col.ColumnName)
			Next

			Dim sql As String = _
				"INSERT INTO " & _tableName & " (" & Environment.NewLine & _
				"    " & String.Join(", ", columns) & Environment.NewLine & _
				") VALUES (" & Environment.NewLine & _
				"    " & String.Join(", ", parameters) & Environment.NewLine & _
				")"

			Dim command As SqlCommand
			If transaction IsNot Nothing Then
				command = New SqlCommand(sql, _connection, transaction)
			Else
				command = New SqlCommand(sql, _connection)
			End If

			command.UpdatedRowSource = UpdateRowSource.None

			For Each col As DataColumn In _dataTable.Columns
				If _excludeFromInsert.Contains(col.ColumnName) Then
					Continue For
				End If

				Dim param = command.Parameters.Add("@" & col.ColumnName, GetSqlDbType(col.DataType))
				param.SourceColumn = col.ColumnName

				If col.MaxLength > 0 AndAlso col.DataType Is GetType(String) Then
					param.Size = col.MaxLength
				End If
			Next

			Return command
		End Function

		''' <summary>
		''' Build UPDATE command based on DataTable schema
		''' </summary>
		Public Function BuildUpdateCommand(Optional transaction As SqlTransaction = Nothing) As SqlCommand
			If _primaryKeyColumns.Count = 0 Then
				Throw New InvalidOperationException("Primary key columns must be set before building UPDATE command")
			End If

			Dim setClauses As New List(Of String)()
			Dim whereClauses As New List(Of String)()

			For Each col As DataColumn In _dataTable.Columns
				If _primaryKeyColumns.Contains(col.ColumnName) Then
					whereClauses.Add(col.ColumnName & " = @" & col.ColumnName)
				ElseIf Not _excludeFromUpdate.Contains(col.ColumnName) Then
					setClauses.Add(col.ColumnName & " = @" & col.ColumnName)
				End If
			Next

			Dim sql As String = _
				"UPDATE " & _tableName & " SET" & Environment.NewLine & _
				"    " & String.Join("," & Environment.NewLine & "    ", setClauses) & Environment.NewLine & _
				"WHERE " & String.Join(" AND ", whereClauses)

			Dim command As SqlCommand
			If transaction IsNot Nothing Then
				command = New SqlCommand(sql, _connection, transaction)
			Else
				command = New SqlCommand(sql, _connection)
			End If

			command.UpdatedRowSource = UpdateRowSource.None

			' Add parameters for SET clause
			For Each col As DataColumn In _dataTable.Columns
				If _primaryKeyColumns.Contains(col.ColumnName) OrElse _excludeFromUpdate.Contains(col.ColumnName) Then
					Continue For
				End If

				Dim param = command.Parameters.Add("@" & col.ColumnName, GetSqlDbType(col.DataType))
				param.SourceColumn = col.ColumnName

				If col.MaxLength > 0 AndAlso col.DataType Is GetType(String) Then
					param.Size = col.MaxLength
				End If
			Next

			' Add parameters for WHERE clause (primary keys)
			For Each pkColumn As String In _primaryKeyColumns
				Dim col As DataColumn = _dataTable.Columns(pkColumn)
				Dim param = command.Parameters.Add("@" & pkColumn, GetSqlDbType(col.DataType))
				param.SourceColumn = pkColumn
				param.SourceVersion = DataRowVersion.Original

				If col.MaxLength > 0 AndAlso col.DataType Is GetType(String) Then
					param.Size = col.MaxLength
				End If
			Next

			Return command
		End Function

		''' <summary>
		''' Build DELETE command based on primary key columns
		''' </summary>
		Public Function BuildDeleteCommand(Optional transaction As SqlTransaction = Nothing) As SqlCommand
			If _primaryKeyColumns.Count = 0 Then
				Throw New InvalidOperationException("Primary key columns must be set before building DELETE command")
			End If

			Dim whereClauses As New List(Of String)()

			For Each pkColumn As String In _primaryKeyColumns
				whereClauses.Add(pkColumn & " = @" & pkColumn)
			Next

			Dim sql As String = _
				"DELETE FROM " & _tableName & Environment.NewLine & _
				"WHERE " & String.Join(" AND ", whereClauses)

			Dim command As SqlCommand
			If transaction IsNot Nothing Then
				command = New SqlCommand(sql, _connection, transaction)
			Else
				command = New SqlCommand(sql, _connection)
			End If

			command.UpdatedRowSource = UpdateRowSource.None

			For Each pkColumn As String In _primaryKeyColumns
				Dim col As DataColumn = _dataTable.Columns(pkColumn)
				Dim param = command.Parameters.Add("@" & pkColumn, GetSqlDbType(col.DataType))
				param.SourceColumn = pkColumn
				param.SourceVersion = DataRowVersion.Original

				If col.MaxLength > 0 AndAlso col.DataType Is GetType(String) Then
					param.Size = col.MaxLength
				End If
			Next

			Return command
		End Function

		''' <summary>
		''' Configure SqlDataAdapter with dynamically generated commands
		''' </summary>
		Public Sub ConfigureAdapter(adapter As SqlDataAdapter, Optional transaction As SqlTransaction = Nothing)
			adapter.InsertCommand = BuildInsertCommand(transaction)
			adapter.UpdateCommand = BuildUpdateCommand(transaction)
			adapter.DeleteCommand = BuildDeleteCommand(transaction)
			adapter.UpdateBatchSize = 0 ' Set batch size for performance
		End Sub

		''' <summary>
		''' Map .NET types to SQL Server types
		''' </summary>
		Private Function GetSqlDbType(dataType As Type) As SqlDbType
			If dataType Is GetType(Integer) Then
				Return SqlDbType.Int
			ElseIf dataType Is GetType(Long) Then
				Return SqlDbType.BigInt
			ElseIf dataType Is GetType(Short) Then
				Return SqlDbType.SmallInt
			ElseIf dataType Is GetType(Byte) Then
				Return SqlDbType.TinyInt
			ElseIf dataType Is GetType(Boolean) Then
				Return SqlDbType.Bit
			ElseIf dataType Is GetType(DateTime) Then
				Return SqlDbType.DateTime
			ElseIf dataType Is GetType(Decimal) Then
				Return SqlDbType.Decimal
			ElseIf dataType Is GetType(Double) Then
				Return SqlDbType.Float
			ElseIf dataType Is GetType(Single) Then
				Return SqlDbType.Real
			ElseIf dataType Is GetType(Guid) Then
				Return SqlDbType.UniqueIdentifier
			ElseIf dataType Is GetType(Byte()) Then
				Return SqlDbType.VarBinary
			ElseIf dataType Is GetType(String) Then
				Return SqlDbType.NVarChar
			Else
				Return SqlDbType.NVarChar ' Default fallback
			End If
		End Function
	End Class
End Namespace
