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
	Public Class GBL_ColumnMap
	    Public Property CsvHeaderName As String
	    Public Property DataTableColumnName As String
	    Public Property DataType As Type ' e.g., GetType(String), GetType(Decimal)
	    Public Property ValType As String
		Public Property ValDim As String
		Public Property ValToken As String
	    Public Property MaxLengthValue As Integer ' Used if ValidationRule.MaxLength is used
	    
	    ' Constructor
	    Public Sub New(csvName As String, dbName As String, colType As Type, Validation As String, Optional Dimension As String = "", Optional Token As String = "", Optional maxLength As Integer = 0)
	        Me.CsvHeaderName = csvName
	        Me.DataTableColumnName = dbName
	        Me.DataType = colType
	        Me.ValType = Validation
			Me.ValDim = Dimension
			Me.ValToken = Token
	        Me.MaxLengthValue = maxLength
	    End Sub
	End Class
End Namespace
