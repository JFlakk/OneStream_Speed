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
	Public Class SQA_XFC_CMD_UFR_Priority
        Private ReadOnly _connection As SqlConnection

        Public Sub New(connection As SqlConnection)
            _connection = connection
        End Sub

        Public Sub Fill_XFC_CMD_UFR_Priority_DT(ByVal si As SessionInfo, ByVal sqa As SqlDataAdapter, ByVal dt As DataTable, ByVal selectQuery As String, ByVal ParamArray sqlparams() As SqlParameter)
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

        Public Sub Update_XFC_CMD_UFR_Priority(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal sqa As SqlDataAdapter)
            Using transaction As SqlTransaction = _connection.BeginTransaction()
                ' INSERT
                Dim insertQuery As String = "
                    INSERT INTO XFC_CMD_UFR_Priority (
                        CMD_UFR_Tracking_No, WFScenario_Name, WFCMD_Name, WFTime_Name, Entity, Review_Entity,
                        Cat_1_Score, Cat_2_Score, Cat_3_Score, Cat_4_Score, Cat_5_Score, Cat_6_Score, Cat_7_Score,
                        Cat_8_Score, Cat_9_Score, Cat_10_Score, Cat_11_Score, Cat_12_Score, Cat_13_Score,
                        Cat_14_Score, Cat_15_Score, Score,
                        Cat_1_Weighted_Score, Cat_2_Weighted_Score, Cat_3_Weighted_Score, Cat_4_Weighted_Score,
                        Cat_5_Weighted_Score, Cat_6_Weighted_Score, Cat_7_Weighted_Score, Cat_8_Weighted_Score,
                        Cat_9_Weighted_Score, Cat_10_Weighted_Score, Cat_11_Weighted_Score, Cat_12_Weighted_Score,
                        Cat_13_Weighted_Score, Cat_14_Weighted_Score, Cat_15_Weighted_Score, Weighted_Score,
                        Auto_Rank, Rank_Override, Create_Date, Create_User, Update_Date, Update_User, UFR_ID
                    ) VALUES (
                        @CMD_UFR_Tracking_No, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Entity, @Review_Entity,
                        @Cat_1_Score, @Cat_2_Score, @Cat_3_Score, @Cat_4_Score, @Cat_5_Score, @Cat_6_Score, @Cat_7_Score,
                        @Cat_8_Score, @Cat_9_Score, @Cat_10_Score, @Cat_11_Score, @Cat_12_Score, @Cat_13_Score,
                        @Cat_14_Score, @Cat_15_Score, @Score,
                        @Cat_1_Weighted_Score, @Cat_2_Weighted_Score, @Cat_3_Weighted_Score, @Cat_4_Weighted_Score,
                        @Cat_5_Weighted_Score, @Cat_6_Weighted_Score, @Cat_7_Weighted_Score, @Cat_8_Weighted_Score,
                        @Cat_9_Weighted_Score, @Cat_10_Weighted_Score, @Cat_11_Weighted_Score, @Cat_12_Weighted_Score,
                        @Cat_13_Weighted_Score, @Cat_14_Weighted_Score, @Cat_15_Weighted_Score, @Weighted_Score,
                        @Auto_Rank, @Rank_Override, @Create_Date, @Create_User, @Update_Date, @Update_User, @UFR_ID
                    );"

                sqa.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
                AddParameters(sqa.InsertCommand, isUpdate:=False)

                ' UPDATE
                Dim updateQuery As String = "
                    UPDATE XFC_CMD_UFR_Priority SET
                        WFScenario_Name = @WFScenario_Name,
                        WFCMD_Name = @WFCMD_Name,
                        WFTime_Name = @WFTime_Name,
                        Cat_1_Score = @Cat_1_Score,
                        Cat_2_Score = @Cat_2_Score,
                        Cat_3_Score = @Cat_3_Score,
                        Cat_4_Score = @Cat_4_Score,
                        Cat_5_Score = @Cat_5_Score,
                        Cat_6_Score = @Cat_6_Score,
                        Cat_7_Score = @Cat_7_Score,
                        Cat_8_Score = @Cat_8_Score,
                        Cat_9_Score = @Cat_9_Score,
                        Cat_10_Score = @Cat_10_Score,
                        Cat_11_Score = @Cat_11_Score,
                        Cat_12_Score = @Cat_12_Score,
                        Cat_13_Score = @Cat_13_Score,
                        Cat_14_Score = @Cat_14_Score,
                        Cat_15_Score = @Cat_15_Score,
                        Score = @Score,
                        Cat_1_Weighted_Score = @Cat_1_Weighted_Score,
                        Cat_2_Weighted_Score = @Cat_2_Weighted_Score,
                        Cat_3_Weighted_Score = @Cat_3_Weighted_Score,
                        Cat_4_Weighted_Score = @Cat_4_Weighted_Score,
                        Cat_5_Weighted_Score = @Cat_5_Weighted_Score,
                        Cat_6_Weighted_Score = @Cat_6_Weighted_Score,
                        Cat_7_Weighted_Score = @Cat_7_Weighted_Score,
                        Cat_8_Weighted_Score = @Cat_8_Weighted_Score,
                        Cat_9_Weighted_Score = @Cat_9_Weighted_Score,
                        Cat_10_Weighted_Score = @Cat_10_Weighted_Score,
                        Cat_11_Weighted_Score = @Cat_11_Weighted_Score,
                        Cat_12_Weighted_Score = @Cat_12_Weighted_Score,
                        Cat_13_Weighted_Score = @Cat_13_Weighted_Score,
                        Cat_14_Weighted_Score = @Cat_14_Weighted_Score,
                        Cat_15_Weighted_Score = @Cat_15_Weighted_Score,
                        Weighted_Score = @Weighted_Score,
                        Auto_Rank = @Auto_Rank,
                        Rank_Override = @Rank_Override,
                        Update_Date = @Update_Date,
                        Update_User = @Update_User,
						UFR_ID = @UFR_ID
                        
                    WHERE CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No AND Entity = @Entity AND Review_Entity = @Review_Entity;"

                sqa.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
                AddParameters(sqa.UpdateCommand, isUpdate:=True)

                ' DELETE
                Dim deleteQuery As String = "
                    DELETE FROM XFC_CMD_UFR_Priority
                    WHERE CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No AND Entity = @Entity AND Review_Entity = @Review_Entity;"

                sqa.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
                With sqa.DeleteCommand.Parameters
                    .Add(New SqlParameter("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier) With {.SourceColumn = "CMD_UFR_Tracking_No", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Entity", SqlDbType.NVarChar, 100) With {.SourceColumn = "Entity", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Review_Entity", SqlDbType.NVarChar, 100) With {.SourceColumn = "Review_Entity", .SourceVersion = DataRowVersion.Original})
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

        ' Helper to create decimal parameter with precision/scale
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

        ' Consolidated parameter builder
        Private Sub AddParameters(ByVal cmd As SqlCommand, Optional ByVal isUpdate As Boolean = False)
            With cmd.Parameters
                .Add("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_UFR_Tracking_No"
                .Add("@WFScenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFScenario_Name"
                .Add("@WFCMD_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFCMD_Name"
                .Add("@WFTime_Name", SqlDbType.NVarChar, 100).SourceColumn = "WFTime_Name"
                .Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
                .Add("@Review_Entity", SqlDbType.NVarChar, 100).SourceColumn = "Review_Entity"

                ' score decimals (6,2)
                .Add(CreateDecimalParameter("@Cat_1_Score", 6, 2, "Cat_1_Score"))
                .Add(CreateDecimalParameter("@Cat_2_Score", 6, 2, "Cat_2_Score"))
                .Add(CreateDecimalParameter("@Cat_3_Score", 6, 2, "Cat_3_Score"))
                .Add(CreateDecimalParameter("@Cat_4_Score", 6, 2, "Cat_4_Score"))
                .Add(CreateDecimalParameter("@Cat_5_Score", 6, 2, "Cat_5_Score"))
                .Add(CreateDecimalParameter("@Cat_6_Score", 6, 2, "Cat_6_Score"))
                .Add(CreateDecimalParameter("@Cat_7_Score", 6, 2, "Cat_7_Score"))
                .Add(CreateDecimalParameter("@Cat_8_Score", 6, 2, "Cat_8_Score"))
                .Add(CreateDecimalParameter("@Cat_9_Score", 6, 2, "Cat_9_Score"))
                .Add(CreateDecimalParameter("@Cat_10_Score", 6, 2, "Cat_10_Score"))
                .Add(CreateDecimalParameter("@Cat_11_Score", 6, 2, "Cat_11_Score"))
                .Add(CreateDecimalParameter("@Cat_12_Score", 6, 2, "Cat_12_Score"))
                .Add(CreateDecimalParameter("@Cat_13_Score", 6, 2, "Cat_13_Score"))
                .Add(CreateDecimalParameter("@Cat_14_Score", 6, 2, "Cat_14_Score"))
                .Add(CreateDecimalParameter("@Cat_15_Score", 6, 2, "Cat_15_Score"))
                .Add(CreateDecimalParameter("@Score", 6, 2, "Score"))

                ' weighted decimals
                .Add(CreateDecimalParameter("@Cat_1_Weighted_Score", 6, 2, "Cat_1_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_2_Weighted_Score", 6, 2, "Cat_2_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_3_Weighted_Score", 6, 2, "Cat_3_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_4_Weighted_Score", 6, 2, "Cat_4_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_5_Weighted_Score", 6, 2, "Cat_5_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_6_Weighted_Score", 6, 2, "Cat_6_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_7_Weighted_Score", 6, 2, "Cat_7_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_8_Weighted_Score", 6, 2, "Cat_8_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_9_Weighted_Score", 6, 2, "Cat_9_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_10_Weighted_Score", 6, 2, "Cat_10_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_11_Weighted_Score", 6, 2, "Cat_11_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_12_Weighted_Score", 6, 2, "Cat_12_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_13_Weighted_Score", 6, 2, "Cat_13_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_14_Weighted_Score", 6, 2, "Cat_14_Weighted_Score"))
                .Add(CreateDecimalParameter("@Cat_15_Weighted_Score", 6, 2, "Cat_15_Weighted_Score"))
                .Add(CreateDecimalParameter("@Weighted_Score", 6, 2, "Weighted_Score"))

                .Add("@Auto_Rank", SqlDbType.Int).SourceColumn = "Auto_Rank"
                .Add("@Rank_Override", SqlDbType.Int).SourceColumn = "Rank_Override"
                .Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
                .Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"
                .Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date"
                .Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User"
                .Add("@UFR_ID", SqlDbType.NVarChar, 50).SourceColumn = "UFR_ID"
                
            End With

            If isUpdate Then
                cmd.Parameters("@CMD_UFR_Tracking_No").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Entity").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Review_Entity").SourceVersion = DataRowVersion.Original
            End If
        End Sub
	End Class
End Namespace