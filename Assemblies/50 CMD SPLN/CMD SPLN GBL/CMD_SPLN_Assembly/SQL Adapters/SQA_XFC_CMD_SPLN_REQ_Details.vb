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
	Public Class SQA_XFC_CMD_SPLN_REQ_Details
	    Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
	
	    Public Sub Fill_XFC_CMD_SPLN_REQ_Details_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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
	
	    Public Sub Update_XFC_CMD_SPLN_REQ_Details(dt As DataTable, adapter As SqlDataAdapter)
	        Using transaction As SqlTransaction = _connection.BeginTransaction()
	            ' Insert command
	            Dim insertQuery As String = "
	                INSERT INTO XFC_CMD_SPLN_REQ_Details (
	                    CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Unit_of_Measure, Entity, IC, Account, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Fiscal_Year, Month1, Month2, Month3, Month4, Month5, Month6, Month7, Month8, Month9, Month10, Month11, Month12, Quarter1, Quarter2, Quarter3, Quarter4, Yearly, AllowUpdate, Create_Date, Create_User, Update_Date, Update_User
	                ) VALUES (
	                    @CMD_SPLN_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Unit_of_Measure, @Entity, @IC, @Account, @Flow, @UD1, @UD2, @UD3, @UD4, @UD5, @UD6, @UD7, @UD8, @Fiscal_Year, @Month1, @Month2, @Month3, @Month4, @Month5, @Month6, @Month7, @Month8, @Month9, @Month10, @Month11, @Month12, @Quarter1, @Quarter2, @Quarter3, @Quarter4, @Yearly, @AllowUpdate, @Create_Date, @Create_User, @Update_Date, @Update_User
	                );"
	            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
	            AddParameters(adapter.InsertCommand)
	
	            ' Update command
	            Dim updateQuery As String = "
	                UPDATE XFC_CMD_SPLN_REQ_Details SET
	                    WFScenario_Name = @WFScenario_Name,
	                    WFCMD_Name = @WFCMD_Name,
	                    WFTime_Name = @WFTime_Name,
	                    Unit_of_Measure = @Unit_of_Measure,
	                    Entity = @Entity,
	                    IC = @IC,
	                    Account = @Account,
	                    Flow = @Flow,
	                    UD1 = @UD1,
	                    UD2 = @UD2,
	                    UD3 = @UD3,
	                    UD4 = @UD4,
	                    UD5 = @UD5,
	                    UD6 = @UD6,
	                    UD7 = @UD7,
	                    UD8 = @UD8,
	                    Fiscal_Year = @Fiscal_Year,
	                    Month1 = @Month1,
						Month2 = @Month2,
						Month3 = @Month3,
						Month4 = @Month4,
						Month5 = @Month5,
						Month6 = @Month6,
						Month7 = @Month7,
						Month8 = @Month8,
						Month9 = @Month9,
						Month10 = @Month10,
						Month11 = @Month11,
						Month12 = @Month12,
						Quarter1 = @Quarter1,
						Quarter2 = @Quarter2,
						Quarter3 = @Quarter3,
						Quarter4 = @Quarter4,
	                    Yearly = @Yearly,
	                    AllowUpdate = @AllowUpdate,
	                    Create_Date = @Create_Date,
	                    Create_User = @Create_User,
	                    Update_Date = @Update_Date,
	                    Update_User = @Update_User
	                WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Account = @Account AND Entity = @Entity AND IC = @IC AND  UD1 = @UD1 AND UD2 = @UD2 AND UD3 = @UD3 AND UD4 = @UD4 AND UD5 = @UD5 AND UD6 = @UD6 AND UD7 = @UD7 AND UD8 = @UD8 AND Fiscal_Year = @Fiscal_Year;"
	            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
	            AddParameters(adapter.UpdateCommand, True)
	
	            ' Delete command
	            Dim deleteQuery As String = "DELETE FROM XFC_CMD_SPLN_REQ_Details WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Account = @Account AND Entity = @Entity AND IC = @IC AND  UD1 = @UD1 AND UD2 = @UD2 AND UD3 = @UD3 AND UD4 = @UD4 AND UD5 = @UD5 AND UD6 = @UD6 AND UD7 = @UD7 AND UD8 = @UD8 AND Fiscal_Year = @Fiscal_Year;"
	            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
	            adapter.DeleteCommand.Parameters.Add("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_SPLN_REQ_ID"
	            adapter.DeleteCommand.Parameters.Add("@Account", SqlDbType.NVarChar, 100).SourceColumn = "Account"
				adapter.DeleteCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
	        	adapter.DeleteCommand.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC"
	        	adapter.DeleteCommand.Parameters.Add("@UD1", SqlDbType.NVarChar, 100).SourceColumn = "UD1"
	        	adapter.DeleteCommand.Parameters.Add("@UD2", SqlDbType.NVarChar, 100).SourceColumn = "UD2"
	        	adapter.DeleteCommand.Parameters.Add("@UD3", SqlDbType.NVarChar, 100).SourceColumn = "UD3"
	        	adapter.DeleteCommand.Parameters.Add("@UD4", SqlDbType.NVarChar, 100).SourceColumn = "UD4"
	        	adapter.DeleteCommand.Parameters.Add("@UD5", SqlDbType.NVarChar, 100).SourceColumn = "UD5"
	        	adapter.DeleteCommand.Parameters.Add("@UD6", SqlDbType.NVarChar, 100).SourceColumn = "UD6"
	        	adapter.DeleteCommand.Parameters.Add("@UD7", SqlDbType.NVarChar, 100).SourceColumn = "UD7"
	        	adapter.DeleteCommand.Parameters.Add("@UD8", SqlDbType.NVarChar, 100).SourceColumn = "UD8"
				adapter.DeleteCommand.Parameters.Add("@Fiscal_Year", SqlDbType.NVarChar, 4).SourceColumn = "Fiscal_Year"
				adapter.DeleteCommand.Parameters("@CMD_SPLN_REQ_ID").SourceVersion = DataRowVersion.Original
	            'cmd.Parameters("@Start_Year").SourceVersion = DataRowVersion.Original
	            'cmd.Parameters("@Unit_of_Measure").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@Entity").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@IC").SourceVersion = DataRowVersion.Original
	            adapter.DeleteCommand.Parameters("@Account").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD1").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD2").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD3").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD4").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD5").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD6").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD7").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@UD8").SourceVersion = DataRowVersion.Original
				adapter.DeleteCommand.Parameters("@Fiscal_Year").SourceVersion = DataRowVersion.Original
	            Try
					adapter.UpdateBatchSize = 1
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
		
			Public Sub MergeandSync_XFC_CMD_SPLN_REQ_Details_toDB(ByVal si As SessionInfo, ByVal srcTable As DataTable, ByVal tgtTable As DataTable)
			Dim compositeKeyColumns As String() = {
			        "CMD_SPLN_REQ_ID", 
			        "Account", 
			        "Entity", 
					"Fiscal_Year",
					"IC",
					"UD1",
					"UD2",
					"UD3",
					"UD4",
					"UD5",
					"UD6",
					"UD7",
					"UD8"
			    }
'		    Dim compositeKeyColumns As String() = {
'			        "CMD_SPLN_REQ_ID"
'			    }
					
		    Dim keys As DataColumn() = compositeKeyColumns.Select(Function(col) tgtTable.Columns(col)).ToArray()
		    tgtTable.PrimaryKey = keys
		
		    Dim sourceKeys As DataColumn() = compositeKeyColumns.Select(Function(col) srcTable.Columns(col)).ToArray()
		    srcTable.PrimaryKey = sourceKeys
			'tgtTable.PrimaryKey.Item(0).
		
			Dim handler As DataTableNewRowEventHandler = Sub(sender, e)
		        If e.Row("CMD_SPLN_REQ_ID") Is DBNull.Value OrElse DirectCast(e.Row("CMD_SPLN_REQ_ID"), Guid) = Guid.Empty Then
		            e.Row("CMD_SPLN_REQ_ID") = Guid.NewGuid()
		        End If
		    End Sub
	    
		    AddHandler tgtTable.TableNewRow, handler
		
		    Try
		        tgtTable.Merge(srcTable, False, MissingSchemaAction.Add)
		    Finally
		        RemoveHandler tgtTable.TableNewRow, handler
		    End Try
'brapi.ErrorLog.LogMessage(si,"count tgtTable =" & tgtTable.Rows.Count & ": " & tgtTable.Rows.Item(0).RowState.ToString)			
		    Dim changes As DataTable = tgtTable.GetChanges(DataRowState.Added Or DataRowState.Modified)
'brapi.ErrorLog.LogMessage(si,"count changes =" & changes.Rows.Count)			
		    If changes IsNot Nothing AndAlso changes.Rows.Count > 0 Then
		        ' Execute the bulk and SQL merge process
		        XFC_CMD_SPLN_REQ_Details_BulkSync(si, changes)

		        tgtTable.AcceptChanges()
		    End If
		End Sub
	
		Private Sub XFC_CMD_SPLN_REQ_Details_BulkSync(ByVal si As SessionInfo, ByVal changes As DataTable)
		    ' Start a transaction to ensure either everything saves or nothing saves
		    Using transaction As SqlTransaction = _connection.BeginTransaction()
		        Try
		            Dim createTempSql As String = $"SELECT TOP 0 CMD_SPLN_REQ_ID,WFScenario_Name,WFCMD_Name,WFTime_Name,Unit_of_Measure,
												Entity,IC,Account,Flow,UD1,UD2,UD3,UD4,UD5,UD6,UD7,UD8,
												Fiscal_Year,Month1,Month2,Month3,Month4,Month5,Month6,
												Month7,Month8,Month9,Month10,Month11,Month12,
												Quarter1,Quarter2,Quarter3,Quarter4,Yearly,
												AllowUpdate,Create_Date,Create_User,Update_Date,Update_User INTO #Temp_XFC_CMD_SPLN_REQ_Details_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)} FROM XFC_CMD_SPLN_REQ_Details"
					Dim createTempIndexSql = $"ALTER TABLE #Temp_XFC_CMD_SPLN_REQ_Details_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)} ADD PRIMARY KEY CLUSTERED (Account, CMD_SPLN_REQ_ID, Entity, Fiscal_Year, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8)"
		            Using cmd As New SqlCommand(createTempSql, _connection, transaction)
		                cmd.ExecuteNonQuery()
						cmd.CommandText = createTempIndexSql
						cmd.ExecuteNonQuery()
		            End Using

		            Using bulkCopy As New SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, transaction)
		                bulkCopy.DestinationTableName = $"#Temp_XFC_CMD_SPLN_REQ_Details_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)}"
		                bulkCopy.WriteToServer(changes)
		            End Using	
					
					 Dim mergeSql As String = $"MERGE INTO XFC_CMD_SPLN_REQ_Details AS Target
												USING #Temp_XFC_CMD_SPLN_REQ_Details_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)} AS Source
												ON 
												    Target.CMD_SPLN_REQ_ID = Source.CMD_SPLN_REQ_ID AND
												    Target.Account = Source.Account AND
												    Target.Entity = Source.Entity AND
												    Target.Fiscal_Year = Source.Fiscal_Year AND
												    Target.IC = Source.IC AND
												    Target.UD1 = Source.UD1 AND
												    Target.UD2 = Source.UD2 AND
												    Target.UD3 = Source.UD3 AND
												    Target.UD4 = Source.UD4 AND
												    Target.UD5 = Source.UD5 AND
												    Target.UD6 = Source.UD6 AND
												    Target.UD7 = Source.UD7 AND
												    Target.UD8 = Source.UD8
												
												WHEN MATCHED THEN
												    UPDATE SET 
												        Target.WFScenario_Name = Source.WFScenario_Name,
												        Target.WFCMD_Name = Source.WFCMD_Name,
												        Target.WFTime_Name = Source.WFTime_Name,
												        Target.Unit_of_Measure = Source.Unit_of_Measure,
												        Target.Flow = Source.Flow,
												        Target.Month1 = Source.Month1,
												        Target.Month2 = Source.Month2,
												        Target.Month3 = Source.Month3,
												        Target.Month4 = Source.Month4,
												        Target.Month5 = Source.Month5,
												        Target.Month6 = Source.Month6,
												        Target.Month7 = Source.Month7,
												        Target.Month8 = Source.Month8,
												        Target.Month9 = Source.Month9,
												        Target.Month10 = Source.Month10,
												        Target.Month11 = Source.Month11,
												        Target.Month12 = Source.Month12,
												        Target.Quarter1 = Source.Quarter1,
												        Target.Quarter2 = Source.Quarter2,
												        Target.Quarter3 = Source.Quarter3,
												        Target.Quarter4 = Source.Quarter4,
												        Target.Yearly = Source.Yearly,
												        Target.AllowUpdate = Source.AllowUpdate,
												        Target.Update_Date = GETDATE(),
												        Target.Update_User = @UserName

												WHEN NOT MATCHED BY TARGET THEN
												    INSERT (
												        CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Unit_of_Measure, 
												        Entity, IC, Account, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, 
												        Fiscal_Year, Month1, Month2, Month3, Month4, Month5, Month6, Month7, 
												        Month8, Month9, Month10, Month11, Month12, Quarter1, Quarter2, 
												        Quarter3, Quarter4, Yearly, AllowUpdate, Create_Date, Create_User, 
												        Update_Date, Update_User
												    )
												    VALUES (
												        Source.CMD_SPLN_REQ_ID, Source.WFScenario_Name, Source.WFCMD_Name, Source.WFTime_Name, Source.Unit_of_Measure, 
												        Source.Entity, Source.IC, Source.Account, Source.Flow, Source.UD1, Source.UD2, Source.UD3, Source.UD4, Source.UD5, Source.UD6, Source.UD7, Source.UD8, 
												        Source.Fiscal_Year, Source.Month1, Source.Month2, Source.Month3, Source.Month4, Source.Month5, Source.Month6, Source.Month7, 
												        Source.Month8, Source.Month9, Source.Month10, Source.Month11, Source.Month12, Source.Quarter1, Source.Quarter2, 
												        Source.Quarter3, Source.Quarter4, Source.Yearly, Source.AllowUpdate, GETDATE(), @UserName, 
												        GETDATE(), @UserName
												    );"					
		            Using mergeCmd As New SqlCommand(mergeSql, _connection, transaction)
		                mergeCmd.Parameters.AddWithValue("@UserName", si.UserName)
		                mergeCmd.ExecuteNonQuery()
		            End Using
		
		            transaction.Commit()
		        Catch ex As Exception
		            transaction.Rollback()
		            Throw New XFException($"Bulk Sync Failed: {ex.Message}", ex)
		        End Try
		    End Using
		End Sub	
	    Private Sub AddParameters(cmd As SqlCommand, Optional isUpdate As Boolean = False)
	        cmd.Parameters.Add("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_SPLN_REQ_ID"
	        cmd.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name"
	        cmd.Parameters.Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name"
	        cmd.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name"
	        cmd.Parameters.Add("@Unit_of_Measure", SqlDbType.NVarChar, 100).SourceColumn = "Unit_of_Measure"
	        cmd.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
	        cmd.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC"
	        cmd.Parameters.Add("@Account", SqlDbType.NVarChar, 100).SourceColumn = "Account"
	        cmd.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow"
	        cmd.Parameters.Add("@UD1", SqlDbType.NVarChar, 100).SourceColumn = "UD1"
	        cmd.Parameters.Add("@UD2", SqlDbType.NVarChar, 100).SourceColumn = "UD2"
	        cmd.Parameters.Add("@UD3", SqlDbType.NVarChar, 100).SourceColumn = "UD3"
	        cmd.Parameters.Add("@UD4", SqlDbType.NVarChar, 100).SourceColumn = "UD4"
	        cmd.Parameters.Add("@UD5", SqlDbType.NVarChar, 100).SourceColumn = "UD5"
	        cmd.Parameters.Add("@UD6", SqlDbType.NVarChar, 100).SourceColumn = "UD6"
	        cmd.Parameters.Add("@UD7", SqlDbType.NVarChar, 100).SourceColumn = "UD7"
	        cmd.Parameters.Add("@UD8", SqlDbType.NVarChar, 100).SourceColumn = "UD8"
	        cmd.Parameters.Add("@Fiscal_Year", SqlDbType.NVarChar, 4).SourceColumn = "Fiscal_Year"
	        cmd.Parameters.Add("@Month1", SqlDbType.Decimal).SourceColumn = "Month1"
	        cmd.Parameters.Add("@Month2", SqlDbType.Decimal).SourceColumn = "Month2"
	        cmd.Parameters.Add("@Month3", SqlDbType.Decimal).SourceColumn = "Month3"
	        cmd.Parameters.Add("@Month4", SqlDbType.Decimal).SourceColumn = "Month4"
	        cmd.Parameters.Add("@Month5", SqlDbType.Decimal).SourceColumn = "Month5"
			cmd.Parameters.Add("@Month6", SqlDbType.Decimal).SourceColumn = "Month6"
	        cmd.Parameters.Add("@Month7", SqlDbType.Decimal).SourceColumn = "Month7"
	        cmd.Parameters.Add("@Month8", SqlDbType.Decimal).SourceColumn = "Month8"
	        cmd.Parameters.Add("@Month9", SqlDbType.Decimal).SourceColumn = "Month9"
	        cmd.Parameters.Add("@Month10", SqlDbType.Decimal).SourceColumn = "Month10"
			cmd.Parameters.Add("@Month11", SqlDbType.Decimal).SourceColumn = "Month11"
	        cmd.Parameters.Add("@Month12", SqlDbType.Decimal).SourceColumn = "Month12"
	        cmd.Parameters.Add("@Quarter1", SqlDbType.Decimal).SourceColumn = "Quarter1"
	        cmd.Parameters.Add("@Quarter2", SqlDbType.Decimal).SourceColumn = "Quarter2"
	        cmd.Parameters.Add("@Quarter3", SqlDbType.Decimal).SourceColumn = "Quarter3"
			cmd.Parameters.Add("@Quarter4", SqlDbType.Decimal).SourceColumn = "Quarter4"
	        cmd.Parameters.Add("@Yearly", SqlDbType.Decimal).SourceColumn = "Yearly"
	        cmd.Parameters.Add("@AllowUpdate", SqlDbType.Bit).SourceColumn = "AllowUpdate"
	        cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
	        cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"
	        cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date"
	        cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User"
	        If isUpdate Then
	            cmd.Parameters("@CMD_SPLN_REQ_ID").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@Entity").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@IC").SourceVersion = DataRowVersion.Original
	            cmd.Parameters("@Account").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD1").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD2").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD3").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD4").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD5").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD6").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD7").SourceVersion = DataRowVersion.Original
				cmd.Parameters("@UD8").SourceVersion = DataRowVersion.Original
	        End If
	    End Sub
	End Class
End Namespace