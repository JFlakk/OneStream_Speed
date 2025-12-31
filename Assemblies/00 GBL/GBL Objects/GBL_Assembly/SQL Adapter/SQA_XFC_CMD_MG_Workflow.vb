Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
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
	Public Class SQA_XFC_CMD_MG_Workflow
		Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
		
		
	    Public Sub Fill_XFC_CMD_MG_Workflow_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
	        Using command As New SqlCommand(selectQuery, _connection)
	            command.CommandType = CommandType.Text
	            If sqlparams IsNot Nothing Then
	                command.Parameters.AddRange(sqlparams)
	            End If
	            adapter.SelectCommand = command
	            adapter.Fill(dt)
	            command.Parameters.Clear()
	            adapter.SelectCommand = Nothing
	        End Using
	    End Sub
		
	    Private Sub AddParameters(cmd As SqlCommand, Optional isUpdate As Boolean = False)
	        cmd.Parameters.Add("@Command", SqlDbType.NVarChar, 100).SourceColumn = "Command" 
	        cmd.Parameters.Add("@Module", SqlDbType.NVarChar, 100).SourceColumn = "Module" 
	        cmd.Parameters.Add("@Level", SqlDbType.NVarChar, 100).SourceColumn = "Level" 
	        cmd.Parameters.Add("@Current_Status", SqlDbType.NVarChar, 100).SourceColumn = "Current_Status" 
	        cmd.Parameters.Add("@Action", SqlDbType.NVarChar, 100).SourceColumn = "Action" 
	        cmd.Parameters.Add("@New_Status", SqlDbType.NVarChar, 100).SourceColumn = "New_Status" 
	        cmd.Parameters.Add("@Order", SqlDbType.NVarChar, 100).SourceColumn = "Order" 
	    End Sub
	End Class
End Namespace