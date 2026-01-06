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
	Public Class SQA_XFC_CMD_UFR
		Private ReadOnly _connection As SqlConnection
	
	    Public Sub New(connection As SqlConnection)
	        _connection = connection
	    End Sub
	
	    Public Sub Fill_XFC_CMD_UFR_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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
	
	    Public Sub Update_XFC_CMD_UFR(dt As DataTable, adapter As SqlDataAdapter)
	        Using transaction As SqlTransaction = _connection.BeginTransaction()
	            ' Insert command
				Dim insertQuery As String = "
	                INSERT INTO XFC_CMD_UFR (
	                    APE9, APPN, CMD_UFR_Tracking_No, Command_UFR_Priority, Command_UFR_Status, Date_Decision_Needed_By, Description, Dollar_Type, Entity, Fund_Type, Funds_Required_By, MDEP, Obj_Class, PEG, Requirement_Background, ROC, SAG_SSN, Title, UFR_Driver, UFR_ID, UFR_ID_Type, WFCMD_Name, WFScenario_Name, WFTime_Name, ABO_Decision, ABO_Funded_Amount, ABO_Review_Input, ABO_Review_POC, ABO_UFR_Status, Army_Campaign_Objectives, CMD_HQ_G8_Submission_Approver, CMD_HQ_G8_Submission_POC, Create_Date, Create_User, G_3_5_7_Review_Input, G_3_5_7_Review_POC, G8_PAE_Review_Input, G8_PAE_Review_POC, Integrator_Input, Integrator_POC, PEG_Review_Input, PEG_Review_POC, RDA_UFR_Executable_Fund_Year, RDA_UFR_FY_New_Start, Rollover_To_Next_UFR_Cycle, UFR_Capability_GAP, UFR_Capability_GAP_If_CMD_Fund, UFR_Driver_Explanation, Update_Date, Update_User, Review_Entity, Review_Staff, BRP_Topic, Solicited_NonSolicited, MustFund, REQ_Link_ID, CType, Initial_Review_Type, Pre_BRP_Date, COL_BRP_Date, Two_Star_BRP_Date, Three_Star_BRP_Date, BRP_Notes, G1_Input, G1_POC, G2_Input, G2_POC, G3_Input, G3_POC, G4_Input, G4_POC, G5_Input, G5_POC, G6_Input, G6_POC, G7_Input, G7_POC, G8_Input, G8_POC, JAG_Input, JAG_POC, HQ_G3_Input, HQ_G3_POC, HQ_G8_Input, HQ_G8_POC, Functional_Area, Study_Category, Fund_Amount, Fund_Status, Fund_Source
	                ) VALUES (
	                    @APE9, @APPN, @CMD_UFR_Tracking_No, @Command_UFR_Priority, @Command_UFR_Status, @Date_Decision_Needed_By, @Description, @Dollar_Type, @Entity, @Fund_Type, @Funds_Required_By, @MDEP, @Obj_Class, @PEG, @Requirement_Background, @ROC, @SAG_SSN, @Title, @UFR_Driver, @UFR_ID, @UFR_ID_Type, @WFCMD_Name, @WFScenario_Name, @WFTime_Name, @ABO_Decision, @ABO_Funded_Amount, @ABO_Review_Input, @ABO_Review_POC, @ABO_UFR_Status, @Army_Campaign_Objectives, @CMD_HQ_G8_Submission_Approver, @CMD_HQ_G8_Submission_POC, @Create_Date, @Create_User, @G_3_5_7_Review_Input, @G_3_5_7_Review_POC, @G8_PAE_Review_Input, @G8_PAE_Review_POC, @Integrator_Input, @Integrator_POC, @PEG_Review_Input, @PEG_Review_POC, @RDA_UFR_Executable_Fund_Year, @RDA_UFR_FY_New_Start, @Rollover_To_Next_UFR_Cycle, @UFR_Capability_GAP, @UFR_Capability_GAP_If_CMD_Fund, @UFR_Driver_Explanation, @Update_Date, @Update_User, @Review_Entity, @Review_Staff, @BRP_Topic, @Solicited_NonSolicited, @MustFund, @REQ_Link_ID, @CType, @Initial_Review_Type, @Pre_BRP_Date, @COL_BRP_Date, @Two_Star_BRP_Date, @Three_Star_BRP_Date, @BRP_Notes, @G1_Input, @G1_POC, @G2_Input, @G2_POC, @G3_Input, @G3_POC, @G4_Input, @G4_POC, @G5_Input, @G5_POC, @G6_Input, @G6_POC, @G7_Input, @G7_POC, @G8_Input, @G8_POC, @JAG_Input, @JAG_POC, @HQ_G3_Input, @HQ_G3_POC, @HQ_G8_Input, @HQ_G8_POC, @Functional_Area, @Study_Category, @Fund_Amount, @Fund_Status, @Fund_Source
	                );"

	    
	            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
	            AddParameters(adapter.InsertCommand)
	
	            ' Update command
	            
	            Dim updateQuery As String = "
	                UPDATE XFC_CMD_UFR SET
	                    APE9 = @APE9,
	                        APPN = @APPN,
	                        Command_UFR_Priority = @Command_UFR_Priority,
	                        Command_UFR_Status = @Command_UFR_Status,
	                        Date_Decision_Needed_By = @Date_Decision_Needed_By,
	                        Description = @Description,
	                        Dollar_Type = @Dollar_Type,
	                        Entity = @Entity,
	                        Fund_Type = @Fund_Type,
	                        Funds_Required_By = @Funds_Required_By,
	                        MDEP = @MDEP,
	                        Obj_Class = @Obj_Class,
	                        PEG = @PEG,
	                        Requirement_Background = @Requirement_Background,
	                        ROC = @ROC,
	                        SAG_SSN = @SAG_SSN,
	                        Title = @Title,
	                        --UFR_Amount = @UFR_Amount,
	                        UFR_Driver = @UFR_Driver,
	                        UFR_ID = @UFR_ID,
	                        UFR_ID_Type = @UFR_ID_Type,
	                        WFCMD_Name = @WFCMD_Name,
	                        WFScenario_Name = @WFScenario_Name,
	                        WFTime_Name = @WFTime_Name,
	                        ABO_Decision = @ABO_Decision,
	                        ABO_Funded_Amount = @ABO_Funded_Amount,
	                        ABO_Review_Input = @ABO_Review_Input,
	                        ABO_Review_POC = @ABO_Review_POC,
	                        ABO_UFR_Status = @ABO_UFR_Status,
	                        Army_Campaign_Objectives = @Army_Campaign_Objectives,
	                        CMD_HQ_G8_Submission_Approver = @CMD_HQ_G8_Submission_Approver,
	                        CMD_HQ_G8_Submission_POC = @CMD_HQ_G8_Submission_POC,
	                        Create_Date = @Create_Date,
	                        Create_User = @Create_User,
	                        G_3_5_7_Review_Input = @G_3_5_7_Review_Input,
	                        G_3_5_7_Review_POC = @G_3_5_7_Review_POC,
	                        G8_PAE_Review_Input = @G8_PAE_Review_Input,
	                        G8_PAE_Review_POC = @G8_PAE_Review_POC,
	                        Integrator_Input = @Integrator_Input,
	                        Integrator_POC = @Integrator_POC,
	                        PEG_Review_Input = @PEG_Review_Input,
	                        PEG_Review_POC = @PEG_Review_POC,
	                        RDA_UFR_Executable_Fund_Year = @RDA_UFR_Executable_Fund_Year,
	                        RDA_UFR_FY_New_Start = @RDA_UFR_FY_New_Start,
	                        Rollover_To_Next_UFR_Cycle = @Rollover_To_Next_UFR_Cycle,
	                        UFR_Capability_GAP = @UFR_Capability_GAP,
	                        UFR_Capability_GAP_If_CMD_Fund = @UFR_Capability_GAP_If_CMD_Fund,
	                        UFR_Driver_Explanation = @UFR_Driver_Explanation,
	                        Update_Date = @Update_Date,
	                        Update_User = @Update_User,
	                        Review_Entity = @Review_Entity,
							Review_Staff = @Review_Staff,
							MustFund = @MustFund,
							BRP_Topic = @BRP_Topic,
							Solicited_NonSolicited = @Solicited_NonSolicited,
							REQ_Link_ID = @REQ_Link_ID,
							CType = @CType,
							Initial_Review_Type = @Initial_Review_Type,
							Pre_BRP_Date = @Pre_BRP_Date,
							COL_BRP_Date = @COL_BRP_Date,
							Two_Star_BRP_Date = @Two_Star_BRP_Date,
							Three_Star_BRP_Date = @Three_Star_BRP_Date,
							BRP_Notes = @BRP_Notes,
							G1_Input = @G1_Input,
							G1_POC = @G1_POC,
							G2_Input = @G2_Input,
							G2_POC = @G2_POC,
							G3_Input = @G3_Input,
							G3_POC = @G3_POC,
							G4_Input = @G4_Input,
							G4_POC = @G4_POC,
							G5_Input = @G5_Input,
							G5_POC = @G5_POC,
							G6_Input = @G6_Input,
							G6_POC = @G6_POC,
							G7_Input = @G7_Input,
							G7_POC = @G7_POC,
							G8_Input = @G8_Input,
							G8_POC = @G8_POC,
							JAG_Input = @JAG_Input,
							JAG_POC = @JAG_POC,
							HQ_G3_Input = @HQ_G3_Input,
							HQ_G3_POC = @HQ_G3_POC,
							HQ_G8_Input = @HQ_G8_Input,
							HQ_G8_POC = @HQ_G8_POC, 
							Functional_Area = @Functional_Area,
							Study_Category = @Study_Category,
							Fund_Amount = @Fund_Amount,
							Fund_Status = @Fund_Status,
							Fund_Source = @Fund_Source
	                WHERE CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No;"
	    
	            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
	            AddParameters(adapter.UpdateCommand, True)
	
	            
	            ' Delete command
	            Dim deleteQuery As String = "DELETE FROM XFC_CMD_UFR WHERE CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No;"
	            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
	            adapter.DeleteCommand.Parameters.Add("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_UFR_Tracking_No"
	    
	
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
		
		Public Sub MergeandSync_XFC_CMD_UFR_toDB(ByVal si As SessionInfo, ByVal srcTable As DataTable, ByVal tgtTable As DataTable)
			Dim compositeKeyColumns As String() = {
			        "WFScenario_Name", 
			        "WFCMD_Name", 
			        "WFTime_Name", 
			        "UFR_ID"
			    }
'			Dim compositeKeyColumns As String() = {
'			        "WFCMD_Name", 
'			        "WFTime_Name", 
'			        "UFR_ID"
'			    }
				
				
'			Dim compositeKeyColumns As String() = {
'			        "CMD_UFR_Tracking_No"
'			    }
								
				
		    Dim keys As DataColumn() = compositeKeyColumns.Select(Function(col) tgtTable.Columns(col)).ToArray()
		    tgtTable.PrimaryKey = keys

		    Dim sourceKeys As DataColumn() = compositeKeyColumns.Select(Function(col) srcTable.Columns(col)).ToArray()
		    srcTable.PrimaryKey = sourceKeys
			'tgtTable.PrimaryKey.Item(0).

			Dim handler As DataTableNewRowEventHandler = Sub(sender, e)
		        If e.Row("CMD_UFR_Tracking_No") Is DBNull.Value OrElse DirectCast(e.Row("CMD_UFR_Tracking_No"), Guid) = Guid.Empty Then
		            e.Row("CMD_UFR_Tracking_No") = Guid.NewGuid()
		        End If
		    End Sub

		    AddHandler tgtTable.TableNewRow, handler

		    Try
		        tgtTable.Merge(srcTable, False, MissingSchemaAction.Add)
		    Finally
		        RemoveHandler tgtTable.TableNewRow, handler
		    End Try
		    Dim changes As DataTable = tgtTable.GetChanges(DataRowState.Added Or DataRowState.Modified)

		    If changes IsNot Nothing AndAlso changes.Rows.Count > 0 Then
		        ' Execute the bulk and SQL merge process
		        XFC_CMD_UFR_BulkSync(si, changes)
Brapi.ErrorLog.LogMessage(si,"Line 216")
		        tgtTable.AcceptChanges()
		    End If
Brapi.ErrorLog.LogMessage(si,"Line 217")
		End Sub
	
		Private Sub XFC_CMD_UFR_BulkSync(ByVal si As SessionInfo, ByVal changes As DataTable)
		    ' Start a transaction to ensure either everything saves or nothing saves
		    Using transaction As SqlTransaction = _connection.BeginTransaction()
		        Try
					Brapi.ErrorLog.LogMessage(si,$"Hit Sync:")
		            Dim createTempSql As String = $"SELECT TOP 0 CMD_UFR_Tracking_No, 
															WFScenario_Name, 
															WFCMD_Name, 
															WFTime_Name, 
														    Title, 
															UFR_ID,
															REQ_Link_ID,
															Description, 
															Entity, 
															Review_Entity,
															APPN, 
															MDEP, 
															APE9,
														    Dollar_Type, 
															Obj_Class, 
															CType, 
															Command_UFR_Status,
															MustFund,
														    Create_Date, Create_User,
														    Update_Date, Update_User 
														    INTO #Temp_XFC_CMD_UFR_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)} 
															FROM XFC_CMD_UFR"
		            Using cmd As New SqlCommand(createTempSql, _connection, transaction)
		                cmd.ExecuteNonQuery()
		            End Using
	
					Brapi.ErrorLog.LogMessage(si,$"Hit Sync req: {changes.Rows.Count}")
		            ' 2. Use SqlBulkCopy to stream the changes to the #Temp table
		            Using bulkCopy As New SqlBulkCopy(_connection, SqlBulkCopyOptions.Default, transaction)
		                bulkCopy.DestinationTableName = $"#Temp_XFC_CMD_UFR_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)}"
		                bulkCopy.WriteToServer(changes)
		            End Using
					Brapi.ErrorLog.LogMessage(si,$"Line 252")
		            ' We use the GUID column as the unique identifier for the join
		            Dim mergeSql As String = $"MERGE INTO XFC_CMD_UFR AS Target
								                USING #Temp_XFC_CMD_UFR_{StringHelper.ReplaceString(si.AuthToken.AuthSessionID.ToString(),"-","_",True)} AS Source
								                ON Target.CMD_UFR_Tracking_No = Source.CMD_UFR_Tracking_No
								                WHEN MATCHED THEN
								                    UPDATE SET 
								                        Target.APE9 = Source.APE9, 
														Target.APPN = Source.APPN, 
														Target.Title = Source.Title,
														Target.UFR_ID = Source.UFR_ID,
														Target.REQ_Link_ID = Source.UFR_ID,
														Target.Description = Source.Description, 
														Target.Dollar_Type = Source.Dollar_Type,
								                        Target.Entity = Source.Entity, 
														Target.Review_Entity = Source.Entity,
								                        Target.MDEP = Source.MDEP, 
														Target.Obj_Class = Source.Obj_Class, 
														Target.Command_UFR_Status = Source.Command_UFR_Status,
														Target.MustFund = Source.MustFund,
								                        Target.WFCMD_Name = Source.WFCMD_Name, 
														Target.WFScenario_Name = Source.WFScenario_Name, 
														Target.WFTime_Name = Source.WFTime_Name,
								                        Target.Update_Date = GETDATE(), Target.Update_User = @UserName
								
								                WHEN NOT MATCHED BY TARGET THEN
								                    INSERT (CMD_UFR_Tracking_No, WFScenario_Name, WFCMD_Name, WFTime_Name, Title, UFR_ID, REQ_Link_ID, Description, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, Command_UFR_Status, MustFund, Create_User, Create_Date, Update_User, Update_Date, Review_Entity)
								                    VALUES (Source.CMD_UFR_Tracking_No, Source.WFScenario_Name, Source.WFCMD_Name, Source.WFTime_Name, Source.Title, Source.UFR_ID, Source.REQ_Link_ID, Source.Description, Source.Entity, Source.APPN, Source.MDEP, Source.APE9, Source.Dollar_Type, Source.Obj_Class, Source.CType, Source.Command_UFR_Status, Source.MustFund, @UserName, GETDATE(), @UserName, GETDATE(), Source.Review_Entity);"
										
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
	        cmd.Parameters.Add("@APE9", SqlDbType.NVarChar, 100).SourceColumn = "APE9" 
	        cmd.Parameters.Add("@APPN", SqlDbType.NVarChar, 100).SourceColumn = "APPN" 
	        cmd.Parameters.Add("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_UFR_Tracking_No" 
	        cmd.Parameters.Add("@Command_UFR_Priority", SqlDbType.NVarChar, 100).SourceColumn = "Command_UFR_Priority" 
	        cmd.Parameters.Add("@Command_UFR_Status", SqlDbType.NVarChar, 100).SourceColumn = "Command_UFR_Status" 
	        cmd.Parameters.Add("@Date_Decision_Needed_By", SqlDbType.DateTime).SourceColumn = "Date_Decision_Needed_By" 
	        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).SourceColumn = "Description" 
	        cmd.Parameters.Add("@Dollar_Type", SqlDbType.NVarChar, 100).SourceColumn = "Dollar_Type" 
	        cmd.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity" 
	        cmd.Parameters.Add("@Fund_Type", SqlDbType.NVarChar, 100).SourceColumn = "Fund_Type" 
	        cmd.Parameters.Add("@Funds_Required_By", SqlDbType.DateTime).SourceColumn = "Funds_Required_By" 
	        cmd.Parameters.Add("@MDEP", SqlDbType.NVarChar, 100).SourceColumn = "MDEP" 
	        cmd.Parameters.Add("@Obj_Class", SqlDbType.NVarChar, 100).SourceColumn = "Obj_Class" 
	        cmd.Parameters.Add("@PEG", SqlDbType.NVarChar, 100).SourceColumn = "PEG" 
	        cmd.Parameters.Add("@Requirement_Background", SqlDbType.NVarChar, -1).SourceColumn = "Requirement_Background" 
	        cmd.Parameters.Add("@ROC", SqlDbType.NVarChar, 100).SourceColumn = "ROC" 
	        cmd.Parameters.Add("@SAG_SSN", SqlDbType.NVarChar, 100).SourceColumn = "SAG_SSN" 
	        cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 1000).SourceColumn = "Title" 
'	        cmd.Parameters.Add("@UFR_Amount", SqlDbType.NVarChar, 1000).SourceColumn = "UFR_Amount" 
	        cmd.Parameters.Add("@UFR_Driver", SqlDbType.NVarChar, 100).SourceColumn = "UFR_Driver" 
	        cmd.Parameters.Add("@UFR_ID", SqlDbType.NVarChar, 100).SourceColumn = "UFR_ID" 
	        cmd.Parameters.Add("@UFR_ID_Type", SqlDbType.NVarChar, 20).SourceColumn = "UFR_ID_Type" 
	        cmd.Parameters.Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name" 
	        cmd.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name" 
	        cmd.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name" 
	        cmd.Parameters.Add("@ABO_Decision", SqlDbType.NVarChar, 100).SourceColumn = "ABO_Decision" 
	        cmd.Parameters.Add("@ABO_Funded_Amount", SqlDbType.NVarChar, 100).SourceColumn = "ABO_Funded_Amount" 
	        cmd.Parameters.Add("@ABO_Review_Input", SqlDbType.NVarChar, -1).SourceColumn = "ABO_Review_Input" 
	        cmd.Parameters.Add("@ABO_Review_POC", SqlDbType.NVarChar, 100).SourceColumn = "ABO_Review_POC" 
	        cmd.Parameters.Add("@ABO_UFR_Status", SqlDbType.NVarChar, 100).SourceColumn = "ABO_UFR_Status" 
	        cmd.Parameters.Add("@Army_Campaign_Objectives", SqlDbType.NVarChar, 100).SourceColumn = "Army_Campaign_Objectives" 
	        cmd.Parameters.Add("@CMD_HQ_G8_Submission_Approver", SqlDbType.NVarChar, 100).SourceColumn = "CMD_HQ_G8_Submission_Approver" 
	        cmd.Parameters.Add("@CMD_HQ_G8_Submission_POC", SqlDbType.NVarChar, 100).SourceColumn = "CMD_HQ_G8_Submission_POC" 
	        cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date" 
	        cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User" 
	        cmd.Parameters.Add("@G_3_5_7_Review_Input", SqlDbType.NVarChar, -1).SourceColumn = "G_3_5_7_Review_Input" 
	        cmd.Parameters.Add("@G_3_5_7_Review_POC", SqlDbType.NVarChar, 100).SourceColumn = "G_3_5_7_Review_POC" 
	        cmd.Parameters.Add("@G8_PAE_Review_Input", SqlDbType.NVarChar, -1).SourceColumn = "G8_PAE_Review_Input" 
	        cmd.Parameters.Add("@G8_PAE_Review_POC", SqlDbType.NVarChar, 100).SourceColumn = "G8_PAE_Review_POC" 
	        cmd.Parameters.Add("@Integrator_Input", SqlDbType.NVarChar, -1).SourceColumn = "Integrator_Input" 
	        cmd.Parameters.Add("@Integrator_POC", SqlDbType.NVarChar, 100).SourceColumn = "Integrator_POC" 
	        cmd.Parameters.Add("@PEG_Review_Input", SqlDbType.NVarChar, -1).SourceColumn = "PEG_Review_Input" 
	        cmd.Parameters.Add("@PEG_Review_POC", SqlDbType.NVarChar, 100).SourceColumn = "PEG_Review_POC" 
	        cmd.Parameters.Add("@RDA_UFR_Executable_Fund_Year", SqlDbType.NVarChar, 100).SourceColumn = "RDA_UFR_Executable_Fund_Year" 
	        cmd.Parameters.Add("@RDA_UFR_FY_New_Start", SqlDbType.NVarChar, 100).SourceColumn = "RDA_UFR_FY_New_Start" 
	        cmd.Parameters.Add("@Rollover_To_Next_UFR_Cycle", SqlDbType.NVarChar, 100).SourceColumn = "Rollover_To_Next_UFR_Cycle" 
	        cmd.Parameters.Add("@UFR_Capability_GAP", SqlDbType.NVarChar, 100).SourceColumn = "UFR_Capability_GAP" 
	        cmd.Parameters.Add("@UFR_Capability_GAP_If_CMD_Fund", SqlDbType.NVarChar, 100).SourceColumn = "UFR_Capability_GAP_If_CMD_Fund" 
	        cmd.Parameters.Add("@UFR_Driver_Explanation", SqlDbType.NVarChar, -1).SourceColumn = "UFR_Driver_Explanation" 
	        cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date" 
	        cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 100).SourceColumn = "Update_User" 
	        cmd.Parameters.Add("@Review_Entity", SqlDbType.NVarChar, 100).SourceColumn = "Review_Entity" 
			
			' Added
	        cmd.Parameters.Add("@Review_Staff", SqlDbType.NVarChar, 100).SourceColumn = "Review_Staff" 
	        cmd.Parameters.Add("@MustFund", SqlDbType.NVarChar, 100).SourceColumn = "MustFund" 
	        cmd.Parameters.Add("@BRP_Topic", SqlDbType.NVarChar, 100).SourceColumn = "BRP_Topic" 
	        cmd.Parameters.Add("@Solicited_NonSolicited", SqlDbType.NVarChar, 100).SourceColumn = "Solicited_NonSolicited" 
	        cmd.Parameters.Add("@REQ_Link_ID", SqlDbType.NVarChar, 100).SourceColumn = "REQ_Link_ID" 
	        cmd.Parameters.Add("@CType", SqlDbType.NVarChar, 100).SourceColumn = "CType" 
	        cmd.Parameters.Add("@Initial_Review_Type", SqlDbType.NVarChar, 100).SourceColumn = "Initial_Review_Type" 
	        cmd.Parameters.Add("@Pre_BRP_Date", SqlDbType.DateTime).SourceColumn = "Pre_BRP_Date" 
	        cmd.Parameters.Add("@COL_BRP_Date", SqlDbType.DateTime).SourceColumn = "COL_BRP_Date" 
	        cmd.Parameters.Add("@Two_Star_BRP_Date", SqlDbType.DateTime).SourceColumn = "Two_Star_BRP_Date" 
	        cmd.Parameters.Add("@Three_Star_BRP_Date", SqlDbType.DateTime).SourceColumn = "Three_Star_BRP_Date" 
	        cmd.Parameters.Add("@BRP_Notes", SqlDbType.NVarChar, 100).SourceColumn = "BRP_Notes" 
			
	        cmd.Parameters.Add("@G1_Input", SqlDbType.NVarChar, -1).SourceColumn = "G1_Input" 
	        cmd.Parameters.Add("@G1_POC", SqlDbType.NVarChar, 100).SourceColumn = "G1_POC" 
	        cmd.Parameters.Add("@G2_Input", SqlDbType.NVarChar, -1).SourceColumn = "G2_Input" 
	        cmd.Parameters.Add("@G2_POC", SqlDbType.NVarChar, 100).SourceColumn = "G2_POC" 
	        cmd.Parameters.Add("@G3_Input", SqlDbType.NVarChar, -1).SourceColumn = "G3_Input" 
	        cmd.Parameters.Add("@G3_POC", SqlDbType.NVarChar, 100).SourceColumn = "G3_POC" 
	        cmd.Parameters.Add("@G4_Input", SqlDbType.NVarChar, -1).SourceColumn = "G4_Input" 
	        cmd.Parameters.Add("@G4_POC", SqlDbType.NVarChar, 100).SourceColumn = "G4_POC" 
	        cmd.Parameters.Add("@G5_Input", SqlDbType.NVarChar, -1).SourceColumn = "G5_Input" 
	        cmd.Parameters.Add("@G5_POC", SqlDbType.NVarChar, 100).SourceColumn = "G5_POC" 
	        cmd.Parameters.Add("@G6_Input", SqlDbType.NVarChar, -1).SourceColumn = "G6_Input" 
	        cmd.Parameters.Add("@G6_POC", SqlDbType.NVarChar, 100).SourceColumn = "G6_POC" 
	        cmd.Parameters.Add("@G7_Input", SqlDbType.NVarChar, -1).SourceColumn = "G7_Input" 
	        cmd.Parameters.Add("@G7_POC", SqlDbType.NVarChar, 100).SourceColumn = "G7_POC" 
	        cmd.Parameters.Add("@G8_Input", SqlDbType.NVarChar, -1).SourceColumn = "G8_Input" 
	        cmd.Parameters.Add("@G8_POC", SqlDbType.NVarChar, 100).SourceColumn = "G8_POC" 
	        cmd.Parameters.Add("@JAG_Input", SqlDbType.NVarChar, -1).SourceColumn = "JAG_Input" 
	        cmd.Parameters.Add("@JAG_POC", SqlDbType.NVarChar, 100).SourceColumn = "JAG_POC" 
	        cmd.Parameters.Add("@HQ_G3_Input", SqlDbType.NVarChar, -1).SourceColumn = "HQ_G3_Input" 
	        cmd.Parameters.Add("@HQ_G3_POC", SqlDbType.NVarChar, 100).SourceColumn = "HQ_G3_POC" 
	        cmd.Parameters.Add("@HQ_G8_Input", SqlDbType.NVarChar, -1).SourceColumn = "HQ_G8_Input" 
	        cmd.Parameters.Add("@HQ_G8_POC", SqlDbType.NVarChar, 100).SourceColumn = "HQ_G8_POC" 
	        cmd.Parameters.Add("@Functional_Area", SqlDbType.NVarChar, 100).SourceColumn = "Functional_Area" 
	        cmd.Parameters.Add("@Study_Category", SqlDbType.NVarChar, 100).SourceColumn = "Study_Category" 
	        cmd.Parameters.Add("@Fund_Amount", SqlDbType.NVarChar, 100).SourceColumn = "Fund_Amount" 
	        cmd.Parameters.Add("@Fund_Status", SqlDbType.NVarChar, 100).SourceColumn = "Fund_Status" 
	        cmd.Parameters.Add("@Fund_Source", SqlDbType.NVarChar, 100).SourceColumn = "Fund_Source" 
	        If isUpdate Then
	            cmd.Parameters("@CMD_UFR_Tracking_No").SourceVersion = DataRowVersion.Original
	        End If
	    End Sub
	End Class
End Namespace