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
    Public Class SQA_XFC_APPN_Mapping
        Private ReadOnly _connection As SqlConnection

        Public Sub New(ByVal connection As SqlConnection)
            _connection = connection
        End Sub

        ''' <summary>
        ''' Fills a DataTable using the provided select query and parameters.
        ''' </summary>
        Public Sub Fill_XFC_APPN_Mapping_DT(ByVal sqa As SqlDataAdapter, ByVal dt As DataTable, ByVal selectQuery As String, ByVal ParamArray sqlparams() As SqlParameter)
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

        ''' <summary>
        ''' Configures the SqlDataAdapter to perform INSERT, UPDATE, and DELETE operations
        ''' on the XFC_APPN_Mapping table within a transaction.
        ''' </summary>
        Public Sub Update_XFC_APPN_Mapping(ByVal dt As DataTable, ByVal sqa As SqlDataAdapter)
            'sqa.UpdateBatchSize = 500 ' Set batch size for performance
			Using transaction As SqlTransaction = _connection.BeginTransaction()
                ' INSERT
                Dim insertQuery As String = "
                    INSERT INTO XFC_APPN_Mapping (
                        Appropriation_CD, Treasury_CD, Years_of_Availability, Dollar_Type, 
                        Supp_ID, Seventh_Character, Partial_Fund_CD
                    ) VALUES (
                        @Appropriation_CD, @Treasury_CD, @Years_of_Availability, @Dollar_Type, 
                        @Supp_ID, @Seventh_Character, @Partial_Fund_CD
                    );"

                sqa.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
                AddParameters(sqa.InsertCommand, isUpdate:=False)

                ' UPDATE
                ' Only the non-primary key field(s) are updated.
                Dim updateQuery As String = "
                    UPDATE XFC_APPN_Mapping SET
                        Partial_Fund_CD = @Partial_Fund_CD
                    WHERE 
                        Appropriation_CD = @Appropriation_CD AND
                        Treasury_CD = @Treasury_CD AND
                        Years_of_Availability = @Years_of_Availability AND
                        Dollar_Type = @Dollar_Type AND
                        Supp_ID = @Supp_ID AND
                        Seventh_Character = @Seventh_Character;"

                sqa.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
                AddParameters(sqa.UpdateCommand, isUpdate:=True)

                ' DELETE
                Dim deleteQuery As String = "
                    DELETE FROM XFC_APPN_Mapping
                    WHERE 
                        Appropriation_CD = @Appropriation_CD AND
                        Treasury_CD = @Treasury_CD AND
                        Years_of_Availability = @Years_of_Availability AND
                        Dollar_Type = @Dollar_Type AND
                        Supp_ID = @Supp_ID AND
                        Seventh_Character = @Seventh_Character;"

                sqa.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
                ' Add only the primary key parameters for the delete command
                With sqa.DeleteCommand.Parameters
                    .Add(New SqlParameter("@Appropriation_CD", SqlDbType.NVarChar, 6) With {.SourceColumn = "Appropriation_CD", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Treasury_CD", SqlDbType.NVarChar, 4) With {.SourceColumn = "Treasury_CD", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Years_of_Availability", SqlDbType.NVarChar, 1) With {.SourceColumn = "Years_of_Availability", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Dollar_Type", SqlDbType.NVarChar, 10) With {.SourceColumn = "Dollar_Type", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Supp_ID", SqlDbType.NVarChar, 1) With {.SourceColumn = "Supp_ID", .SourceVersion = DataRowVersion.Original})
                    .Add(New SqlParameter("@Seventh_Character", SqlDbType.NVarChar, 1) With {.SourceColumn = "Seventh_Character", .SourceVersion = DataRowVersion.Original})
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

        ''' <summary>
        ''' Helper method to add all parameters to the InsertCommand and UpdateCommand.
        ''' </summary>
        Private Sub AddParameters(ByVal cmd As SqlCommand, Optional ByVal isUpdate As Boolean = False)
            cmd.Parameters.Add("@Appropriation_CD", SqlDbType.NVarChar, 6).SourceColumn = "Appropriation_CD"
            cmd.Parameters.Add("@Treasury_CD", SqlDbType.NVarChar, 4).SourceColumn = "Treasury_CD"
            cmd.Parameters.Add("@Years_of_Availability", SqlDbType.NVarChar, 1).SourceColumn = "Years_of_Availability"
            cmd.Parameters.Add("@Dollar_Type", SqlDbType.NVarChar, 10).SourceColumn = "Dollar_Type"
            cmd.Parameters.Add("@Supp_ID", SqlDbType.NVarChar, 1).SourceColumn = "Supp_ID"
            cmd.Parameters.Add("@Seventh_Character", SqlDbType.NVarChar, 1).SourceColumn = "Seventh_Character"
            cmd.Parameters.Add("@Partial_Fund_CD", SqlDbType.NVarChar, 10).SourceColumn = "Partial_Fund_CD"

            If isUpdate Then
                ' Set key SourceVersion for update/delete operations
                cmd.Parameters("@Appropriation_CD").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Treasury_CD").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Years_of_Availability").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Dollar_Type").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Supp_ID").SourceVersion = DataRowVersion.Original
                cmd.Parameters("@Seventh_Character").SourceVersion = DataRowVersion.Original
            End If
        End Sub
    End Class
End Namespace