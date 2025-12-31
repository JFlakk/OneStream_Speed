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
	Public Class SQA_XFC_CMD_PGM_REQ_Cmt
	    Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
		
	    Public Sub Fill_XFC_CMD_PGM_REQ_CMT_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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
	
	    Public Sub Update_XFC_CMD_PGM_REQ_Cmt(dt As DataTable, adapter As SqlDataAdapter)
	        Using transaction As SqlTransaction = _connection.BeginTransaction()
	            ' Insert command
	            Dim insertQuery As String = "
	                INSERT INTO XFC_CMD_PGM_REQ_Cmt (
	                    CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name,General_Comment, Update_Date, Update_User
	                ) VALUES (
	                    @CMD_PGM_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @General_Comment, @Update_Date, @Update_User
	                );"
	            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
	            AddParameters(adapter.InsertCommand)
	
	            ' Update command
	            Dim updateQuery As String = "
	                UPDATE XFC_CMD_PGM_REQ_Cmt SET
						CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID,
	                	WFScenario_Name = @WFScenario_Name,
                        WFCMD_Name = @WFCMD_Name,
                        WFTime_Name = @WFTime_Name,
	                    General_Comment = @General_Comment,
	                    Update_Date = @Update_Date,
	                    Update_User = @Update_User"
	                    
						
	              
	            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
	            AddParameters(adapter.UpdateCommand, True)
	
	            ' Delete command
	            Dim deleteQuery As String = "DELETE FROM XFC_CMD_PGM_REQ_Cmt WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID;"
	            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
	            adapter.DeleteCommand.Parameters.Add("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_PGM_REQ_ID"
	
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
			cmd.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name"
            cmd.Parameters.Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name"
            cmd.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name"
	        cmd.Parameters.Add("@General_Comment", SqlDbType.NVarChar, 1000).SourceColumn = "General_Comment"
	        cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date"
	        cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User"
	        
			
	        If isUpdate Then
	            cmd.Parameters("@CMD_PGM_REQ_ID").SourceVersion = DataRowVersion.Original
	        End If
	    End Sub
	End Class

End Namespace