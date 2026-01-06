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
	Public Class SQA_XFC_CMD_Staffing_Input

	    Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
	
	    Public Sub Fill_XFC_CMD_Staffing_Input_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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
	
	    Public Sub Update_XFC_CMD_Staffing_Input(dt As DataTable, adapter As SqlDataAdapter)
	        Using transaction As SqlTransaction = _connection.BeginTransaction()
	            ' Insert command
	            Dim insertQuery As String = "
	                INSERT INTO XFC_CMD_Staffing_Input (
	                    Tracking_No_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Module, Level, Staffing_Element, Review_Input, Review_POC, BRP_Candidate, BRP_Topic, Pre_BRP_Date, COL_BRP_Date, Two_Star_Date, Three_Star_Date, BRP_Notes, ABO_Decision, UFR_ID
	                ) VALUES (
	                    @Tracking_No_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Module, @Level, @Staffing_Element, @Review_Input, @Review_POC, @BRP_Candidate, @BRP_Topic, @Pre_BRP_Date, @COL_BRP_Date, @Two_Star_Date, @Three_Star_Date, @BRP_Notes, @ABO_Decision, @UFR_ID
	                );"
	            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
	            AddParameters(adapter.InsertCommand)
	
	            ' Update command
	            Dim updateQuery As String = "
	                UPDATE XFC_CMD_Staffing_Input SET
						--Tracking_No_ID = @Tracking_No_ID,
	                    WFScenario_Name = @WFScenario_Name,
	                    WFCMD_Name = @WFCMD_Name,
	                    WFTime_Name = @WFTime_Name,
	                    Module = @Module,
	                    Level = @Level,
	                    Staffing_Element = @Staffing_Element,
	                    Review_Input = @Review_Input,
	                    Review_POC = @Review_POC,
	                    BRP_Candidate = @BRP_Candidate,
	                    BRP_Topic = @BRP_Topic,
	                    Pre_BRP_Date = @Pre_BRP_Date,
	                    COL_BRP_Date = @COL_BRP_Date,
	                    Two_Star_Date = @Two_Star_Date,
	                    Three_Star_Date = @Three_Star_Date,
	                    BRP_Notes = @BRP_Notes,
	                    ABO_Decision = @ABO_Decision,
						UFR_ID = @UFR_ID
	                WHERE Tracking_No_ID = @Tracking_No_ID;"
	            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
	            AddParameters(adapter.UpdateCommand, True)
	
	            ' Delete command
	            Dim deleteQuery As String = "DELETE FROM XFC_CMD_Staffing_Input WHERE Tracking_No_ID = @Tracking_No_ID;"
	            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
	            adapter.DeleteCommand.Parameters.Add("@Tracking_No_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Tracking_No_ID"
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
	        cmd.Parameters.Add("@Tracking_No_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Tracking_No_ID" 
	        cmd.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name" 
	        cmd.Parameters.Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name" 
	        cmd.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name" 
	        cmd.Parameters.Add("@Module", SqlDbType.NVarChar, 100).SourceColumn = "Module" 
	        cmd.Parameters.Add("@Level", SqlDbType.NVarChar, 100).SourceColumn = "Level" 
	        cmd.Parameters.Add("@Staffing_Element", SqlDbType.NVarChar, 100).SourceColumn = "Staffing_Element" 
	        cmd.Parameters.Add("@Review_Input", SqlDbType.NVarChar, 10000).SourceColumn = "Review_Input" 
	        cmd.Parameters.Add("@Review_POC", SqlDbType.NVarChar, 100).SourceColumn = "Review_POC" 
	        cmd.Parameters.Add("@BRP_Candidate", SqlDbType.NVarChar, 100).SourceColumn = "BRP_Candidate" 
	        cmd.Parameters.Add("@BRP_Topic", SqlDbType.NVarChar, 100).SourceColumn = "BRP_Topic" 
	        cmd.Parameters.Add("@Pre_BRP_Date", SqlDbType.DateTime).SourceColumn = "Pre_BRP_Date" 
	        cmd.Parameters.Add("@COL_BRP_Date", SqlDbType.DateTime).SourceColumn = "COL_BRP_Date" 
	        cmd.Parameters.Add("@Two_Star_Date", SqlDbType.DateTime).SourceColumn = "Two_Star_Date" 
	        cmd.Parameters.Add("@Three_Star_Date", SqlDbType.DateTime).SourceColumn = "Three_Star_Date" 
	        cmd.Parameters.Add("@BRP_Notes", SqlDbType.NVarChar, 10000).SourceColumn = "BRP_Notes" 
	        cmd.Parameters.Add("@ABO_Decision", SqlDbType.NVarChar, 100).SourceColumn = "ABO_Decision" 	
	        cmd.Parameters.Add("@UFR_ID", SqlDbType.NVarChar, 100).SourceColumn = "UFR_ID" 			
	        If isUpdate Then
	            cmd.Parameters("@Tracking_No_ID").SourceVersion = DataRowVersion.Original
	        End If
	    End Sub
	End Class
End Namespace