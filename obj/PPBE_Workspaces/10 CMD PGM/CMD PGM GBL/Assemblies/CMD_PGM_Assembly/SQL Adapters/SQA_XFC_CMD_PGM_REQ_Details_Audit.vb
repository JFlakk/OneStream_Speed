' filepath: /workspaces/OneStream_Speed/obj/PPBE_Workspaces/10 CMD PGM/CMD PGM GBL/Assemblies/CMD_PGM_Assembly/SQL Adapters/SQA_XFC_CMD_PGM_REQ_Details_Audit.vb
' ...existing code...
Imports System
Imports System.Data
Imports Microsoft.Data.SqlClient
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
    Public Class SQA_XFC_CMD_PGM_REQ_Details_Audit
        Private ReadOnly _connection As SqlConnection

        Public Sub New(ByVal si As SessionInfo, ByVal connection As SqlConnection)
            _connection = connection
        End Sub

        Public Sub Fill_XFC_CMD_PGM_REQ_Details_Audit_DT(ByVal si As SessionInfo, ByVal sqa As SqlDataAdapter, ByVal dt As DataTable, ByVal selectQuery As String, ByVal ParamArray sqlparams() As SqlParameter)
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

        Public Sub Update_XFC_CMD_PGM_REQ_Details_Audit(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal sqa As SqlDataAdapter)
            Using transaction As SqlTransaction = _connection.BeginTransaction()
                ' INSERT
                Dim insertQuery As String = "
                    INSERT INTO XFC_CMD_PGM_REQ_Details_Audit (
                        CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Entity, Account, Start_Year,
                        Orig_IC, Updated_IC, Orig_Flow, Updated_Flow,
                        Orig_UD1, Updated_UD1, Orig_UD2, Updated_UD2, Orig_UD3, Updated_UD3, Orig_UD4, Updated_UD4,
                        Orig_UD5, Updated_UD5, Orig_UD6, Updated_UD6, Orig_UD7, Updated_UD7, Orig_UD8, Updated_UD8,
                        Orig_FY1, Updated_FY1, Orig_FY2, Updated_FY2, Orig_FY3, Updated_FY3, Orig_FY4, Updated_FY4,
                        Orig_FY5, Updated_FY5, Orig_FY_Total, Updated_FY_Total, Create_Date, Create_User
                    ) VALUES (
                        @CMD_PGM_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Entity, @Account, @Start_Year,
                        @Orig_IC, @Updated_IC, @Orig_Flow, @Updated_Flow,
                        @Orig_UD1, @Updated_UD1, @Orig_UD2, @Updated_UD2, @Orig_UD3, @Updated_UD3, @Orig_UD4, @Updated_UD4,
                        @Orig_UD5, @Updated_UD5, @Orig_UD6, @Updated_UD6, @Orig_UD7, @Updated_UD7, @Orig_UD8, @Updated_UD8,
                        @Orig_FY1, @Updated_FY1, @Orig_FY2, @Updated_FY2, @Orig_FY3, @Updated_FY3, @Orig_FY4, @Updated_FY4,
                        @Orig_FY5, @Updated_FY5, @Orig_FY_Total, @Updated_FY_Total, @Create_Date, @Create_User
                    );"

                sqa.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
                AddParameters(sqa.InsertCommand, isUpdate:=False)

                ' UPDATE
                Dim updateQuery As String = "
                    UPDATE XFC_CMD_PGM_REQ_Details_Audit SET
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
                        Orig_FY1 = @Orig_FY1,
                        Updated_FY1 = @Updated_FY1,
                        Orig_FY2 = @Orig_FY2,
                        Updated_FY2 = @Updated_FY2,
                        Orig_FY3 = @Orig_FY3,
                        Updated_FY3 = @Updated_FY3,
                        Orig_FY4 = @Orig_FY4,
                        Updated_FY4 = @Updated_FY4,
                        Orig_FY5 = @Orig_FY5,
                        Updated_FY5 = @Updated_FY5,
                        Orig_FY_Total = @Orig_FY_Total,
                        Updated_FY_Total = @Updated_FY_Total
                    WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID AND Start_Year = @Start_Year AND Account = @Account;"

                sqa.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
                AddParameters(sqa.UpdateCommand, isUpdate:=True)

                ' DELETE
                Dim deleteQuery As String = "
                    DELETE FROM XFC_CMD_PGM_REQ_Details_Audit
                    WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID AND Start_Year = @Start_Year AND Account = @Account;"

                sqa.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
                With sqa.DeleteCommand.Parameters
                    .Add(New SqlParameter("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier) With {.SourceColumn = "CMD_PGM_REQ_ID", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Start_Year", SqlDbType.NVarChar, 4) With {.SourceColumn = "Start_Year", .SourceVersion = DataRowVersion.Original})
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
            cmd.Parameters.Add("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_PGM_REQ_ID"
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
            cmd.Parameters.Add("@Start_Year", SqlDbType.NVarChar, 4).SourceColumn = "Start_Year"
            cmd.Parameters.Add("@FY_1", SqlDbType.Decimal).SourceColumn = "FY_1"
            cmd.Parameters.Add("@FY_2", SqlDbType.Decimal).SourceColumn = "FY_2"
            cmd.Parameters.Add("@FY_3", SqlDbType.Decimal).SourceColumn = "FY_3"
            cmd.Parameters.Add("@FY_4", SqlDbType.Decimal).SourceColumn = "FY_4"
            cmd.Parameters.Add("@FY_5", SqlDbType.Decimal).SourceColumn = "FY_5"
            cmd.Parameters.Add("@FY_Total", SqlDbType.Decimal).SourceColumn = "FY_Total"
            cmd.Parameters.Add("@AllowUpdate", SqlDbType.Bit).SourceColumn = "AllowUpdate"
            cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
            cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"
            cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date"
            cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User"

            If isUpdate Then
                ' set key SourceVersion for update/delete operations
                cmd.Parameters("@CMD_PGM_REQ_ID").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Start_Year").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Account").SourceVersion = DataRowVersion.Original
            End If
        End Sub
    End Class
End Namespace