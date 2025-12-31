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
	Public Class SQA_XFC_CMD_SPLN_REQ_Details_Audit
        Private ReadOnly _connection As SqlConnection

        Public Sub New(ByVal connection As SqlConnection)
            _connection = connection
        End Sub

        Public Sub Fill_XFC_CMD_SPLN_REQ_Details_Audit_DT(ByVal sqa As SqlDataAdapter, ByVal dt As DataTable, ByVal selectQuery As String, ByVal ParamArray sqlparams() As SqlParameter)
            Using command As New SqlCommand(selectQuery, _connection)
                command.CommandType = CommandType.Text
                If sqlparams IsNot Nothing AndAlso sqlparams.Length > 0 Then
                    command.Parameters.AddRange(sqlparams)
                End If

                sqa.SelectCommand = command
                sqa.Fill(dt)
                command.Parameters.Clear()
                sqa.SelectCommand = Nothing
            End Using
        End Sub

        Public Sub Update_XFC_CMD_SPLN_REQ_Details_Audit(ByVal dt As DataTable, ByVal sqa As SqlDataAdapter)
            Using transaction As SqlTransaction = _connection.BeginTransaction()
                ' INSERT
                Dim insertQuery As String = "
                    INSERT INTO XFC_CMD_SPLN_REQ_Details_Audit (
                        CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Entity, Account, Fiscal_Year,
                        Orig_IC, Updated_IC, Orig_Flow, Updated_Flow,
                        Orig_UD1, Updated_UD1, Orig_UD2, Updated_UD2, Orig_UD3, Updated_UD3, Orig_UD4, Updated_UD4,
                        Orig_UD5, Updated_UD5, Orig_UD6, Updated_UD6, Orig_UD7, Updated_UD7, Orig_UD8, Updated_UD8,
                        Orig_Month1, Updated_Month1, Orig_Month2, Updated_Month2, Orig_Month3, Updated_Month3, Orig_Month4, Updated_Month4,
                        Orig_Month5, Updated_Month5, Orig_Month6,Updated_Month6,Orig_Month7,Updated_Month7,Orig_Month8,Updated_Month8,Orig_Month9,
						Updated_Month9,Orig_Month10,Updated_Month10,Orig_Month11,Updated_Month11,Orig_Month12,Updated_Month12,
						Orig_Quarter1,Updated_Quarter1,Orig_Quarter2, Updated_Quarter2,Orig_Quarter3, Updated_Quarter3,Orig_Quarter4,Updated_Quarter4,
						Orig_Yearly, Updated_Yearly,Create_Date, Create_User
                    ) VALUES (
                        @CMD_SPLN_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Entity, @Account, @Fiscal_Year,
                        @Orig_IC, @Updated_IC, @Orig_Flow, @Updated_Flow,
                        @Orig_UD1, @Updated_UD1, @Orig_UD2, @Updated_UD2, @Orig_UD3, @Updated_UD3, @Orig_UD4, @Updated_UD4,
                        @Orig_UD5, @Updated_UD5, @Orig_UD6, @Updated_UD6, @Orig_UD7, @Updated_UD7, @Orig_UD8, @Updated_UD8,
                        @Orig_Month1, @Updated_Month1, @Orig_Month2, @Updated_Month2, @Orig_Month3, @Updated_Month3, @Orig_Month4, @Updated_Month4,
                        @Orig_Month5, @Updated_Month5, @Orig_Month6,@Updated_Month6,@Orig_Month7,@Updated_Month7,@Orig_Month8,@Updated_Month8,@Orig_Month9,
						@Updated_Month9,@Orig_Month10,@Updated_Month10,@Orig_Month11,@Updated_Month11,@Orig_Month12,@Updated_Month12,
						@Orig_Quarter1,@Updated_Quarter1,@Orig_Quarter2, @Updated_Quarter2,@Orig_Quarter3, @Updated_Quarter3,@Orig_Quarter4,@Updated_Quarter4,
						@Orig_Yearly, @Updated_Yearly,@Create_Date, @Create_User
                    );"

                sqa.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
                AddParameters(sqa.InsertCommand, isUpdate:=False)

                ' UPDATE
                Dim updateQuery As String = "
                    UPDATE XFC_CMD_SPLN_REQ_Details_Audit SET
                        WFScenario_Name = @WFScenario_Name,
                        WFCMD_Name = @WFCMD_Name,
                        WFTime_Name = @WFTime_Name,
                        Entity = @Entity,
                        Orig_IC = @Orig_IC,
                        Updated_IC = @Updated_IC,
                        Orig_Flow = @Orig_Flow,
                        Updated_Flow = @Updated_Flow,
                        Orig_UD1 = @Orig_UD1,
                        Updated_UD1 = @Updated_UD1,
                        Orig_UD2 = @Orig_UD2,
                        Updated_UD2 = @Updated_UD2,
                        Orig_UD3 = @Orig_UD3,
                        Updated_UD3 = @Updated_UD3,
                        Orig_UD4 = @Orig_UD4,
                        Updated_UD4 = @Updated_UD4,
                        Orig_UD5 = @Orig_UD5,
                        Updated_UD5 = @Updated_UD5,
                        Orig_UD6 = @Orig_UD6,
                        Updated_UD6 = @Updated_UD6,
                        Orig_UD7 = @Orig_UD7,
                        Updated_UD7 = @Updated_UD7,
                        Orig_UD8 = @Orig_UD8,
                        Updated_UD8 = @Updated_UD8,
						Orig_Month1 = @Orig_Month1, 
						Updated_Month1 = @Updated_Month1, 
						Orig_Month2 = @Orig_Month2, 
						Updated_Month2 = @Updated_Month2, 
						Orig_Month3 = @Orig_Month3,
						Updated_Month3 = @Updated_Month3, 
						Orig_Month4 = @Orig_Month4,
						Updated_Month4 = @Updated_Month4,
		              	Orig_Month5 = @Orig_Month5, 
						Updated_Month5 = @Updated_Month5, 
						Orig_Month6 = @Orig_Month6,
						Updated_Month6 = @Updated_Month6,
						Orig_Month7 = @Orig_Month7,
						Updated_Month7 = @Updated_Month7,
						Orig_Month8 = @Orig_Month8,
						Updated_Month8 = @Updated_Month8,
						Orig_Month9 = @Orig_Month9,
						Updated_Month9 = @Updated_Month9,
						Orig_Month10 = @Orig_Month10,
						Updated_Month10 = @Updated_Month10,
						Orig_Month11 = @Orig_Month11,
						Updated_Month11 = @Updated_Month11,
						Orig_Month12 = @Orig_Month12,
						Updated_Month12 = @Updated_Month12,
						Orig_Quarter1 = @Orig_Quarter1,
						Updated_Quarter1 = @Updated_Quarter1,
						Orig_Quarter2 = @Orig_Quarter2, 
						Updated_Quarter2 = @Updated_Quarter2,
						Orig_Quarter3 = @Orig_Quarter3, 
						Updated_Quarter3 = @Updated_Quarter3,
						Orig_Quarter4 = @Orig_Quarter4,
						Updated_Quarter4 = @Updated_Quarter4,
						Orig_Yearly = @Orig_Yearly, 
						Updated_Yearly = @Updated_Yearly,
						Create_Date = @Create_Date, 
						Create_User = @Create_User
                       
                    WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Fiscal_Year = @Fiscal_Year AND Account = @Account;"

                sqa.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
                AddParameters(sqa.UpdateCommand, isUpdate:=True)

                ' DELETE
                Dim deleteQuery As String = "
                    DELETE FROM XFC_CMD_SPLN_REQ_Details_Audit
                    WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Fiscal_Year = @Fiscal_Year AND Account = @Account;"

                sqa.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
                With sqa.DeleteCommand.Parameters
                    .Add(New SqlParameter("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier) With {.SourceColumn = "CMD_SPLN_REQ_ID", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Fiscal_Year", SqlDbType.NVarChar, 4) With {.SourceColumn = "Fiscal_Year", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Account", SqlDbType.NVarChar, 100) With {.SourceColumn = "Account", .SourceVersion = DataRowVersion.Original})
                End With

                Try
                    sqa.Update(dt)
                    transaction.Commit()
                    sqa.InsertCommand = Nothing
                    sqa.UpdateCommand = Nothing
                    sqa.DeleteCommand = Nothing
                Catch ex As Exception
                    transaction.Rollback()
                    Throw
                End Try
            End Using
        End Sub

        ' Consolidated AddParameters similar to Req_Details file
        Private Sub AddParameters(ByVal cmd As SqlCommand, Optional ByVal isUpdate As Boolean = False)
            cmd.Parameters.Add("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_SPLN_REQ_ID"
            cmd.Parameters.Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name"
            cmd.Parameters.Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name"
            cmd.Parameters.Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name"
            cmd.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
            cmd.Parameters.Add("@Account", SqlDbType.NVarChar, 100).SourceColumn = "Account"
			cmd.Parameters.Add("@Fiscal_Year", SqlDbType.NVarChar, 4).SourceColumn = "Fiscal_Year"
			cmd.Parameters.Add("@Orig_IC", SqlDbType.NVarChar, 100).SourceColumn = "Orig_IC"
			cmd.Parameters.Add("@Updated_IC", SqlDbType.NVarChar, 100).SourceColumn = "Updated_IC"
            cmd.Parameters.Add("@Orig_Flow", SqlDbType.NVarChar, 100).SourceColumn = "Orig_Flow"
			cmd.Parameters.Add("@Updated_Flow", SqlDbType.NVarChar, 100).SourceColumn = "Updated_Flow"
            cmd.Parameters.Add("@Orig_UD1", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD1"
			cmd.Parameters.Add("@Updated_UD1", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD1"
            cmd.Parameters.Add("@Orig_UD2", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD2"
			cmd.Parameters.Add("@Updated_UD2", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD2"
            cmd.Parameters.Add("@Orig_UD3", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD3"
			cmd.Parameters.Add("@Updated_UD3", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD3"
            cmd.Parameters.Add("@Orig_UD4", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD4"
			cmd.Parameters.Add("@Updated_UD4", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD4"
            cmd.Parameters.Add("@Orig_UD5", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD5"
			cmd.Parameters.Add("@Updated_UD5", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD5"
            cmd.Parameters.Add("@Orig_UD6", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD6"
			cmd.Parameters.Add("@Updated_UD6", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD6"
            cmd.Parameters.Add("@Orig_UD7", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD7"
			cmd.Parameters.Add("@Updated_UD7", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD7"
            cmd.Parameters.Add("@Orig_UD8", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD8"
			cmd.Parameters.Add("@Updated_UD8", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD8"
            cmd.Parameters.Add("@Orig_Month1", SqlDbType.Decimal).SourceColumn = "Orig_Month1"
			cmd.Parameters.Add("@Updated_Month1", SqlDbType.Decimal).SourceColumn = "Updated_Month1"
			cmd.Parameters.Add("@Orig_Month2", SqlDbType.Decimal).SourceColumn = "Orig_Month2"
			cmd.Parameters.Add("@Updated_Month2", SqlDbType.Decimal).SourceColumn = "Updated_Month2"
			cmd.Parameters.Add("@Orig_Month3", SqlDbType.Decimal).SourceColumn = "Orig_Month3"
			cmd.Parameters.Add("@Updated_Month3", SqlDbType.Decimal).SourceColumn = "Updated_Month3"
			cmd.Parameters.Add("@Orig_Month4", SqlDbType.Decimal).SourceColumn = "Orig_Month4"
			cmd.Parameters.Add("@Updated_Month4", SqlDbType.Decimal).SourceColumn = "Updated_Month4"
			cmd.Parameters.Add("@Orig_Month5", SqlDbType.Decimal).SourceColumn = "Orig_Month5"
			cmd.Parameters.Add("@Updated_Month5", SqlDbType.Decimal).SourceColumn = "Updated_Month5"
			cmd.Parameters.Add("@Orig_Month6", SqlDbType.Decimal).SourceColumn = "Orig_Month6"
			cmd.Parameters.Add("@Updated_Month6", SqlDbType.Decimal).SourceColumn = "Updated_Month6"
			cmd.Parameters.Add("@Orig_Month7", SqlDbType.Decimal).SourceColumn = "Orig_Month7"
			cmd.Parameters.Add("@Updated_Month7", SqlDbType.Decimal).SourceColumn = "Updated_Month7"
			cmd.Parameters.Add("@Orig_Month8", SqlDbType.Decimal).SourceColumn = "Orig_Month8"
			cmd.Parameters.Add("@Updated_Month8", SqlDbType.Decimal).SourceColumn = "Updated_Month8"
			cmd.Parameters.Add("@Orig_Month9", SqlDbType.Decimal).SourceColumn = "Orig_Month9"
			cmd.Parameters.Add("@Updated_Month9", SqlDbType.Decimal).SourceColumn = "Updated_Month9"
			cmd.Parameters.Add("@Orig_Month10", SqlDbType.Decimal).SourceColumn = "Orig_Month10"
			cmd.Parameters.Add("@Updated_Month10", SqlDbType.Decimal).SourceColumn = "Updated_Month10"
			cmd.Parameters.Add("@Orig_Month11", SqlDbType.Decimal).SourceColumn = "Orig_Month11"
			cmd.Parameters.Add("@Updated_Month11", SqlDbType.Decimal).SourceColumn = "Updated_Month11"
			cmd.Parameters.Add("@Orig_Month12", SqlDbType.Decimal).SourceColumn = "Orig_Month12"
			cmd.Parameters.Add("@Updated_Month12", SqlDbType.Decimal).SourceColumn = "Updated_Month12"
			cmd.Parameters.Add("@Orig_Quarter1", SqlDbType.Decimal).SourceColumn = "Orig_Quarter1"
			cmd.Parameters.Add("@Updated_Quarter1", SqlDbType.Decimal).SourceColumn = "Updated_Quarter1"
			cmd.Parameters.Add("@Orig_Quarter2", SqlDbType.Decimal).SourceColumn = "Orig_Quarter2"
			cmd.Parameters.Add("@Updated_Quarter2", SqlDbType.Decimal).SourceColumn = "Updated_Quarter2"
			cmd.Parameters.Add("@Orig_Quarter3", SqlDbType.Decimal).SourceColumn = "Orig_Quarter3"
			cmd.Parameters.Add("@Updated_Quarter3", SqlDbType.Decimal).SourceColumn = "Updated_Quarter3"
			cmd.Parameters.Add("@Orig_Quarter4", SqlDbType.Decimal).SourceColumn = "Orig_Quarter4"
			cmd.Parameters.Add("@Updated_quarter4", SqlDbType.Decimal).SourceColumn = "Updated_quarter4"
			cmd.Parameters.Add("@Orig_Yearly", SqlDbType.Decimal).SourceColumn = "Orig_Yearly"
			cmd.Parameters.Add("@Updated_Yearly", SqlDbType.Decimal).SourceColumn = "Updated_Yearly"
            cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
            cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"

            If isUpdate Then
                ' set key SourceVersion for update/delete operations
                cmd.Parameters("@CMD_SPLN_REQ_ID").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Fiscal_Year").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Account").SourceVersion = DataRowVersion.Original
            End If
        End Sub
	End Class
End Namespace