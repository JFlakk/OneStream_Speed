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

        Public Sub New(ByVal si As SessionInfo, ByVal connection As SqlConnection)
            _connection = connection
        End Sub

        Public Sub Fill_XFC_CMD_SPLN_REQ_Details_Audit_DT(ByVal si As SessionInfo, ByVal sqa As SqlDataAdapter, ByVal dt As DataTable, ByVal selectQuery As String, ByVal ParamArray sqlparams() As SqlParameter)
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

        Public Sub Update_XFC_CMD_SPLN_REQ_Details_Audit(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal sqa As SqlDataAdapter)
            Using transaction As SqlTransaction = _connection.BeginTransaction()
                ' INSERT
                Dim insertQuery As String = "
                    INSERT INTO XFC_CMD_SPLN_REQ_Details_Audit (
                        CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Entity, Account, Fiscal_Year,
                        Orig_IC, Updated_IC, Orig_Flow, Updated_Flow,
                        Orig_UD1, Updated_UD1, Orig_UD2, Updated_UD2, Orig_UD3, Updated_UD3, Orig_UD4, Updated_UD4,
                        Orig_UD5, Updated_UD5, Orig_UD6, Updated_UD6, Orig_UD7, Updated_UD7, Orig_UD8, Updated_UD8,
                        Orig_Month1, Updated_Month1, Orig_Month2, Updated_Month2, Orig_Month3, Updated_Month3,
                        Orig_Month4, Updated_Month4, Orig_Month5, Updated_Month5, Orig_Month6, Updated_Month6,
                        Orig_Month7, Updated_Month7, Orig_Month8, Updated_Month8, Orig_Month9, Updated_Month9,
                        Orig_Month10, Updated_Month10, Orig_Month11, Updated_Month11, Orig_Month12, Updated_Month12,
                        Orig_Quarter1, Updated_Quarter1, Orig_Quarter2, Updated_Quarter2, Orig_Quarter3, Updated_Quarter3,
                        Orig_Quarter4, Updated_Quarter4, Orig_Yearly, Updated_Yearly, Create_Date, Create_User
                    ) VALUES (
                        @CMD_SPLN_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Entity, @Account, @Fiscal_Year,
                        @Orig_IC, @Updated_IC, @Orig_Flow, @Updated_Flow,
                        @Orig_UD1, @Updated_UD1, @Orig_UD2, @Updated_UD2, @Orig_UD3, @Updated_UD3, @Orig_UD4, @Updated_UD4,
                        @Orig_UD5, @Updated_UD5, @Orig_UD6, @Updated_UD6, @Orig_UD7, @Updated_UD7, @Orig_UD8, @Updated_UD8,
                        @Orig_Month1, @Updated_Month1, @Orig_Month2, @Updated_Month2, @Orig_Month3, @Updated_Month3,
                        @Orig_Month4, @Updated_Month4, @Orig_Month5, @Updated_Month5, @Orig_Month6, @Updated_Month6,
                        @Orig_Month7, @Updated_Month7, @Orig_Month8, @Updated_Month8, @Orig_Month9, @Updated_Month9,
                        @Orig_Month10, @Updated_Month10, @Orig_Month11, @Updated_Month11, @Orig_Month12, @Updated_Month12,
                        @Orig_Quarter1, @Updated_Quarter1, @Orig_Quarter2, @Updated_Quarter2, @Orig_Quarter3, @Updated_Quarter3,
                        @Orig_Quarter4, @Updated_Quarter4, @Orig_Yearly, @Updated_Yearly, @Create_Date, @Create_User
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
                        Updated_Yearly = @Updated_Yearly
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

        ' Helper to create a configured decimal parameter
        Private Function CreateDecimalParameter(paramName As String, precision As Byte, scale As Byte, sourceColumn As String, Optional sourceVersion As DataRowVersion? = Nothing) As SqlParameter
            Dim p As New SqlParameter(paramName, SqlDbType.Decimal)
            p.Precision = precision
            p.Scale = scale
            p.SourceColumn = sourceColumn
            If sourceVersion.HasValue Then
                p.SourceVersion = sourceVersion.Value
            End If
            Return p
        End Function

        ' Consolidated AddParameters (matches DDL)
        Private Sub AddParameters(ByVal cmd As SqlCommand, Optional ByVal isUpdate As Boolean = False)
            With cmd.Parameters
                .Add("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_SPLN_REQ_ID"
                .Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name"
                .Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name"
                .Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name"
                .Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
                .Add("@Account", SqlDbType.NVarChar, 100).SourceColumn = "Account"
                .Add("@Fiscal_Year", SqlDbType.NVarChar, 4).SourceColumn = "Fiscal_Year"

                .Add("@Orig_IC", SqlDbType.NVarChar, 100).SourceColumn = "Orig_IC"
                .Add("@Updated_IC", SqlDbType.NVarChar, 100).SourceColumn = "Updated_IC"
                .Add("@Orig_Flow", SqlDbType.NVarChar, 100).SourceColumn = "Orig_Flow"
                .Add("@Updated_Flow", SqlDbType.NVarChar, 100).SourceColumn = "Updated_Flow"

                .Add("@Orig_UD1", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD1"
                .Add("@Updated_UD1", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD1"
                .Add("@Orig_UD2", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD2"
                .Add("@Updated_UD2", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD2"
                .Add("@Orig_UD3", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD3"
                .Add("@Updated_UD3", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD3"
                .Add("@Orig_UD4", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD4"
                .Add("@Updated_UD4", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD4"
                .Add("@Orig_UD5", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD5"
                .Add("@Updated_UD5", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD5"
                .Add("@Orig_UD6", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD6"
                .Add("@Updated_UD6", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD6"
                .Add("@Orig_UD7", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD7"
                .Add("@Updated_UD7", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD7"
                .Add("@Orig_UD8", SqlDbType.NVarChar, 100).SourceColumn = "Orig_UD8"
                .Add("@Updated_UD8", SqlDbType.NVarChar, 100).SourceColumn = "Updated_UD8"

                ' months (decimal 28,4)
                .Add(CreateDecimalParameter("@Orig_Month1", 28, 4, "Orig_Month1"))
                .Add(CreateDecimalParameter("@Updated_Month1", 28, 4, "Updated_Month1"))
                .Add(CreateDecimalParameter("@Orig_Month2", 28, 4, "Orig_Month2"))
                .Add(CreateDecimalParameter("@Updated_Month2", 28, 4, "Updated_Month2"))
                .Add(CreateDecimalParameter("@Orig_Month3", 28, 4, "Orig_Month3"))
                .Add(CreateDecimalParameter("@Updated_Month3", 28, 4, "Updated_Month3"))
                .Add(CreateDecimalParameter("@Orig_Month4", 28, 4, "Orig_Month4"))
                .Add(CreateDecimalParameter("@Updated_Month4", 28, 4, "Updated_Month4"))
                .Add(CreateDecimalParameter("@Orig_Month5", 28, 4, "Orig_Month5"))
                .Add(CreateDecimalParameter("@Updated_Month5", 28, 4, "Updated_Month5"))
                .Add(CreateDecimalParameter("@Orig_Month6", 28, 4, "Orig_Month6"))
                .Add(CreateDecimalParameter("@Updated_Month6", 28, 4, "Updated_Month6"))
                .Add(CreateDecimalParameter("@Orig_Month7", 28, 4, "Orig_Month7"))
                .Add(CreateDecimalParameter("@Updated_Month7", 28, 4, "Updated_Month7"))
                .Add(CreateDecimalParameter("@Orig_Month8", 28, 4, "Orig_Month8"))
                .Add(CreateDecimalParameter("@Updated_Month8", 28, 4, "Updated_Month8"))
                .Add(CreateDecimalParameter("@Orig_Month9", 28, 4, "Orig_Month9"))
                .Add(CreateDecimalParameter("@Updated_Month9", 28, 4, "Updated_Month9"))
                .Add(CreateDecimalParameter("@Orig_Month10", 28, 4, "Orig_Month10"))
                .Add(CreateDecimalParameter("@Updated_Month10", 28, 4, "Updated_Month10"))
                .Add(CreateDecimalParameter("@Orig_Month11", 28, 4, "Orig_Month11"))
                .Add(CreateDecimalParameter("@Updated_Month11", 28, 4, "Updated_Month11"))
                .Add(CreateDecimalParameter("@Orig_Month12", 28, 4, "Orig_Month12"))
                .Add(CreateDecimalParameter("@Updated_Month12", 28, 4, "Updated_Month12"))

                ' quarters (decimal 28,4)
                .Add(CreateDecimalParameter("@Orig_Quarter1", 28, 4, "Orig_Quarter1"))
                .Add(CreateDecimalParameter("@Updated_Quarter1", 28, 4, "Updated_Quarter1"))
                .Add(CreateDecimalParameter("@Orig_Quarter2", 28, 4, "Orig_Quarter2"))
                .Add(CreateDecimalParameter("@Updated_Quarter2", 28, 4, "Updated_Quarter2"))
                .Add(CreateDecimalParameter("@Orig_Quarter3", 28, 4, "Orig_Quarter3"))
                .Add(CreateDecimalParameter("@Updated_Quarter3", 28, 4, "Updated_Quarter3"))
                .Add(CreateDecimalParameter("@Orig_Quarter4", 28, 4, "Orig_Quarter4"))
                .Add(CreateDecimalParameter("@Updated_Quarter4", 28, 4, "Updated_Quarter4"))

                .Add(CreateDecimalParameter("@Orig_Yearly", 28, 4, "Orig_Yearly"))
                .Add(CreateDecimalParameter("@Updated_Yearly", 28, 4, "Updated_Yearly"))

                .Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
                .Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"
            End With

            If isUpdate Then
                ' set key SourceVersion for update/delete operations
                cmd.Parameters("@CMD_SPLN_REQ_ID").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Fiscal_Year").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Account").SourceVersion = DataRowVersion.Original
            End If
        End Sub
    End Class
End Namespace