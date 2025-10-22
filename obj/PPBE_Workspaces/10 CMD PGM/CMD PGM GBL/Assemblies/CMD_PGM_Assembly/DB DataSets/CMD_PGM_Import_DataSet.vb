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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_PGM_Import_DataSet
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						If args.DataSetName.XFEqualsIgnoreCase("MyDataSet") Then
				
							Dim tableName As String = "CMD_PGM_Import"
							Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName,tableName)
'							dt.Clear()
'							If dt Is Nothing Or dt.Rows.Count = 0
'								dt = New DataTable(tableName)
'								dt.Columns.Add("REQ_Title")
'								dt.Columns.Add("REQ_ID")
'								dt.Columns.Add("Error Message")
'								Dim r1 As DataRow = dt.NewRow()
'								r1("REQ_Title") = "Title 2"
'								r1("REQ_ID") = "ID 2"
'								dt.Rows.Add(r1)
								
'								BRApi.Utilities.SetSessionDataTable(si,si.UserName,tableName,dt)
'							End If
							Return dt
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
