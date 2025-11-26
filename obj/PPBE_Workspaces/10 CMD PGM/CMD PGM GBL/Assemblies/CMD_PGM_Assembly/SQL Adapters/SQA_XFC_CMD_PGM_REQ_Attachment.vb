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
	Public Class SQA_XFC_CMD_PGM_REQ_Attachment
	    Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
	
	    Public Sub Fill_XFC_CMD_PGM_REQ_Attachment_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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
	
	    Public Sub Update_XFC_CMD_PGM_REQ_Attachment(dt As DataTable, adapter As SqlDataAdapter)
	        Using transaction As SqlTransaction = _connection.BeginTransaction()
	            ' Insert command
	            Dim insertQuery As String = "
	                INSERT INTO XFC_CMD_PGM_REQ_Attachment (
	                    CMD_PGM_REQ_ID,Attach_File_Name,Attach_File_Bytes
	                ) VALUES (
	                    @CMD_PGM_REQ_ID,@Attach_File_Name,@Attach_File_Bytes
	                );"
	            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
	            AddParameters(adapter.InsertCommand)
	
	            ' Update command
	            Dim updateQuery As String = "
	                UPDATE XFC_CMD_PGM_REQ_Attachment SET
	                    Attach_File_Name = @Attach_File_Name,
	          			Attach_File_Bytes = @Attach_File_Bytes
	                WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID AND Attach_File_Name = @Attach_File_Name;"
	            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
	            AddParameters(adapter.UpdateCommand, True)
	
	            ' Delete command
	            Dim deleteQuery As String = "DELETE FROM XFC_CMD_PGM_REQ_Attachment WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID AND AND Attach_File_Name = @Attach_File_Name;"
	            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
	            adapter.DeleteCommand.Parameters.Add("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_PGM_REQ_ID"
	  			adapter.DeleteCommand.Parameters.Add("@Attach_File_Name", SqlDbType.NVarChar, 1000).SourceColumn = "Attach_File_Name"
	            Try
	                adapter.Update(dt)
	                transaction.Commit()
	                adapter.InsertCommand = Nothing
	                adapter.UpdateCommand = Nothing
	                adapter.DeleteCommand = Nothing
	            Catch ex As Exception
	                transaction.Rollback()
	                Throw
	            End Try
	        End Using
	    End Sub
	
	    Private Sub AddParameters(cmd As SqlCommand, Optional isUpdate As Boolean = False)
	        cmd.Parameters.Add("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_PGM_REQ_ID"
	        cmd.Parameters.Add("@Attach_File_Name", SqlDbType.NVarChar, 1000).SourceColumn = "Attach_File_Name"
	        cmd.Parameters.Add("@Attach_File_Bytes", SqlDbType.VarBinary, -1).SourceColumn = "Attach_File_Bytes"
	        If isUpdate Then
	            cmd.Parameters("@CMD_PGM_REQ_ID").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@Attach_File_Name").SourceVersion = DataRowVersion.Original
	        End If
	    End Sub
	End Class

End Namespace
