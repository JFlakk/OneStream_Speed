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
            Dim insertQuery As String = """
                INSERT INTO XFC_CMD_SPLN_REQ_Details (
                    CMD_SPLN_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, Unit_of_Measure, Entity, IC, Account, Flow,
                    UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Fiscal_Year,
                    Month1, Month2, Month3, Month4, Month5, Month6, Month7, Month8, Month9, Month10, Month11, Month12,
                    Quarter1, Quarter2, Quarter3, Quarter4, Yearly, AllowUpdate, Create_Date, Create_User, Update_Date, Update_User
                ) VALUES (
                    @CMD_SPLN_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @Unit_of_Measure, @Entity, @IC, @Account, @Flow,
                    @UD1, @UD2, @UD3, @UD4, @UD5, @UD6, @UD7, @UD8, @Fiscal_Year,
                    @Month1, @Month2, @Month3, @Month4, @Month5, @Month6, @Month7, @Month8, @Month9, @Month10, @Month11, @Month12,
                    @Quarter1, @Quarter2, @Quarter3, @Quarter4, @Yearly, @AllowUpdate, @Create_Date, @Create_User, @Update_Date, @Update_User
                );"""
            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
            AddParameters(adapter.InsertCommand)

            ' Update command
            Dim updateQuery As String = """
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
                WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Fiscal_Year = @Fiscal_Year AND Unit_of_Measure = @Unit_of_Measure AND Account = @Account;"""
            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
            AddParameters(adapter.UpdateCommand, True)

            ' Delete command
            Dim deleteQuery As String = "DELETE FROM XFC_CMD_SPLN_REQ_Details WHERE CMD_SPLN_REQ_ID = @CMD_SPLN_REQ_ID AND Fiscal_Year = @Fiscal_Year AND Unit_of_Measure = @Unit_of_Measure AND Account = @Account;"
            adapter.DeleteCommand = New SqlCommand(deleteQuery, _connection, transaction)
            adapter.DeleteCommand.Parameters.Add("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier).SourceColumn = "CMD_SPLN_REQ_ID"
            adapter.DeleteCommand.Parameters.Add("@Fiscal_Year", SqlDbType.NVarChar, 4).SourceColumn = "Fiscal_Year"
            adapter.DeleteCommand.Parameters.Add("@Unit_of_Measure", SqlDbType.NVarChar, 100).SourceColumn = "Unit_of_Measure"
            adapter.DeleteCommand.Parameters.Add("@Account", SqlDbType.NVarChar, 100).SourceColumn = "Account"

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
            cmd.Parameters("@Fiscal_Year").SourceVersion = DataRowVersion.Original
            cmd.Parameters("@Unit_of_Measure").SourceVersion = DataRowVersion.Original
            cmd.Parameters("@Account").SourceVersion = DataRowVersion.Original
        End If
    End Sub
End Class
