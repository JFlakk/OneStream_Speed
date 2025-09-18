Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
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

Public Class SQA_XFC_CMD_PGM_REQ
    Private ReadOnly _connection As SqlConnection

    Public Sub New(connection As SqlConnection)
        _connection = connection
    End Sub

    Public Sub Fill_XFC_CMD_PGM_REQ_DT(adapter As SqlDataAdapter, dt As DataTable, selectQuery As String, ParamArray sqlparams() As SqlParameter)
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

    Public Sub Update_XFC_CMD_PGM_REQ(dt As DataTable, adapter As SqlDataAdapter)
        Using transaction As SqlTransaction = _connection.BeginTransaction()
            ' Insert command
            Dim insertQuery As String = """
                INSERT INTO XFC_CMD_PGM_REQ (
                    CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, Gen_Comments_Notes, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code, SS_Priority, Commit_Group, SS_Cap, Strategic_BIN, LIN, REQ_Type, DD_Priority, Portfolio, DD_Cap, JNT_Cap_Area, TBM_Cost_Pool, TBM_Tower, APMS_AITR_Num, Zero_Trust_Cap, Assoc_Directives, Cloud_IND, Strat_Cyber_Sec_PGM, Notes, FF_1, FF_2, FF_3, FF_4, FF_5, Attach_File_Name, Attach_File_Bytes, Status, Invalid, Val_Error, Create_Date, Create_User, Update_Date, Update_User
                ) VALUES (
                    @CMD_PGM_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @REQ_ID, @Title, @Description, @Justification, @Entity, @APPN, @MDEP, @APE9, @Dollar_Type, @Obj_Class, @CType, @UIC, @Cost_Methodology, @Impact_Not_Funded, @Risk_Not_Funded, @Cost_Growth_Justification, @Must_Fund, @Funding_Src, @Army_Init_Dir, @CMD_Init_Dir, @Activity_Exercise, @Directorate, @Div, @Branch, @IT_Cyber_REQ, @Emerging_REQ, @CPA_Topic, @PBR_Submission, @UPL_Submission, @Contract_Num, @Task_Order_Num, @Award_Target_Date, @POP_Exp_Date, @Contractor_ManYear_Equiv_CME, @COR_Email, @POC_Email, @Review_POC_Email, @MDEP_Functional_Email, @Notification_List_Emails, @Gen_Comments_Notes, @JUON, @ISR_Flag, @Cost_Model, @Combat_Loss, @Cost_Location, @Cat_A_Code, @CBS_Code, @MIP_Proj_Code, @SS_Priority, @Commit_Group, @SS_Cap, @Strategic_BIN, @LIN, @REQ_Type, @DD_Priority, @Portfolio, @DD_Cap, @JNT_Cap_Area, @TBM_Cost_Pool, @TBM_Tower, @APMS_AITR_Num, @Zero_Trust_Cap, @Assoc_Directives, @Cloud_IND, @Strat_Cyber_Sec_PGM, @Notes, @FF_1, @FF_2, @FF_3, @FF_4, @FF_5, @Attach_File_Name, @Attach_File_Bytes, @Status, @Invalid, @Val_Error, @Create_Date, @Create_User, @Update_Date, @Update_User
                );"""
            adapter.InsertCommand = New SqlCommand(insertQuery, _connection, transaction)
            AddParameters(adapter.InsertCommand)

            ' Update command
            Dim updateQuery As String = """
                UPDATE XFC_CMD_PGM_REQ SET
                    WFScenario_Name = @WFScenario_Name,
                    WFCMD_Name = @WFCMD_Name,
                    WFTime_Name = @WFTime_Name,
                    REQ_ID = @REQ_ID,
                    Title = @Title,
                    Description = @Description,
                    Justification = @Justification,
                    Entity = @Entity,
                    APPN = @APPN,
                    MDEP = @MDEP,
                    APE9 = @APE9,
                    Dollar_Type = @Dollar_Type,
                    Obj_Class = @Obj_Class,
                    CType = @CType,
                    UIC = @UIC,
                    Cost_Methodology = @Cost_Methodology,
                    Impact_Not_Funded = @Impact_Not_Funded,
                    Risk_Not_Funded = @Risk_Not_Funded,
                    Cost_Growth_Justification = @Cost_Growth_Justification,
                    Must_Fund = @Must_Fund,
                    Funding_Src = @Funding_Src,
                    Army_Init_Dir = @Army_Init_Dir,
                    CMD_Init_Dir = @CMD_Init_Dir,
                    Activity_Exercise = @Activity_Exercise,
                    Directorate = @Directorate,
                    Div = @Div,
                    Branch = @Branch,
                    IT_Cyber_REQ = @IT_Cyber_REQ,
                    Emerging_REQ = @Emerging_REQ,
                    CPA_Topic = @CPA_Topic,
                    PBR_Submission = @PBR_Submission,
                    UPL_Submission = @UPL_Submission,
                    Contract_Num = @Contract_Num,
                    Task_Order_Num = @Task_Order_Num,
                    Award_Target_Date = @Award_Target_Date,
                    POP_Exp_Date = @POP_Exp_Date,
                    Contractor_ManYear_Equiv_CME = @Contractor_ManYear_Equiv_CME,
                    COR_Email = @COR_Email,
                    POC_Email = @POC_Email,
                    Review_POC_Email = @Review_POC_Email,
                    MDEP_Functional_Email = @MDEP_Functional_Email,
                    Notification_List_Emails = @Notification_List_Emails,
                    Gen_Comments_Notes = @Gen_Comments_Notes,
                    JUON = @JUON,
                    ISR_Flag = @ISR_Flag,
                    Cost_Model = @Cost_Model,
                    Combat_Loss = @Combat_Loss,
                    Cost_Location = @Cost_Location,
                    Cat_A_Code = @Cat_A_Code,
                    CBS_Code = @CBS_Code,
                    MIP_Proj_Code = @MIP_Proj_Code,
                    SS_Priority = @SS_Priority,
                    Commit_Group = @Commit_Group,
                    SS_Cap = @SS_Cap,
                    Strategic_BIN = @Strategic_BIN,
                    LIN = @LIN,
                    REQ_Type = @REQ_Type,
                    DD_Priority = @DD_Priority,
                    Portfolio = @Portfolio,
                    DD_Cap = @DD_Cap,
                    JNT_Cap_Area = @JNT_Cap_Area,
                    TBM_Cost_Pool = @TBM_Cost_Pool,
                    TBM_Tower = @TBM_Tower,
                    APMS_AITR_Num = @APMS_AITR_Num,
                    Zero_Trust_Cap = @Zero_Trust_Cap,
                    Assoc_Directives = @Assoc_Directives,
                    Cloud_IND = @Cloud_IND,
                    Strat_Cyber_Sec_PGM = @Strat_Cyber_Sec_PGM,
                    Notes = @Notes,
                    FF_1 = @FF_1,
                    FF_2 = @FF_2,
                    FF_3 = @FF_3,
                    FF_4 = @FF_4,
                    FF_5 = @FF_5,
                    Attach_File_Name = @Attach_File_Name,
                    Attach_File_Bytes = @Attach_File_Bytes,
                    Status = @Status,
                    Invalid = @Invalid,
                    Val_Error = @Val_Error,
                    Create_Date = @Create_Date,
                    Create_User = @Create_User,
                    Update_Date = @Update_Date,
                    Update_User = @Update_User
                WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID;"""
            adapter.UpdateCommand = New SqlCommand(updateQuery, _connection, transaction)
            AddParameters(adapter.UpdateCommand, True)

            ' Delete command
            Dim deleteQuery As String = "DELETE FROM XFC_CMD_PGM_REQ WHERE CMD_PGM_REQ_ID = @CMD_PGM_REQ_ID;"
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
        cmd.Parameters.Add("@REQ_ID", SqlDbType.NVarChar, 100).SourceColumn = "REQ_ID"
        cmd.Parameters.Add("@Title", SqlDbType.NVarChar, 1000).SourceColumn = "Title"
        cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).SourceColumn = "Description"
        cmd.Parameters.Add("@Justification", SqlDbType.NVarChar, -1).SourceColumn = "Justification"
        cmd.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity"
        cmd.Parameters.Add("@APPN", SqlDbType.NVarChar, 100).SourceColumn = "APPN"
        cmd.Parameters.Add("@MDEP", SqlDbType.NVarChar, 100).SourceColumn = "MDEP"
        cmd.Parameters.Add("@APE9", SqlDbType.NVarChar, 100).SourceColumn = "APE9"
        cmd.Parameters.Add("@Dollar_Type", SqlDbType.NVarChar, 100).SourceColumn = "Dollar_Type"
        cmd.Parameters.Add("@Obj_Class", SqlDbType.NVarChar, 100).SourceColumn = "Obj_Class"
        cmd.Parameters.Add("@CType", SqlDbType.NVarChar, 100).SourceColumn = "CType"
        cmd.Parameters.Add("@UIC", SqlDbType.NVarChar, 100).SourceColumn = "UIC"
        cmd.Parameters.Add("@Cost_Methodology", SqlDbType.NVarChar, -1).SourceColumn = "Cost_Methodology"
        cmd.Parameters.Add("@Impact_Not_Funded", SqlDbType.NVarChar, -1).SourceColumn = "Impact_Not_Funded"
        cmd.Parameters.Add("@Risk_Not_Funded", SqlDbType.NVarChar, -1).SourceColumn = "Risk_Not_Funded"
        cmd.Parameters.Add("@Cost_Growth_Justification", SqlDbType.NVarChar, 1000).SourceColumn = "Cost_Growth_Justification"
        cmd.Parameters.Add("@Must_Fund", SqlDbType.NVarChar, 1000).SourceColumn = "Must_Fund"
        cmd.Parameters.Add("@Funding_Src", SqlDbType.NVarChar, 1000).SourceColumn = "Funding_Src"
        cmd.Parameters.Add("@Army_Init_Dir", SqlDbType.NVarChar, 1000).SourceColumn = "Army_Init_Dir"
        cmd.Parameters.Add("@CMD_Init_Dir", SqlDbType.NVarChar, 1000).SourceColumn = "CMD_Init_Dir"
        cmd.Parameters.Add("@Activity_Exercise", SqlDbType.NVarChar, 1000).SourceColumn = "Activity_Exercise"
        cmd.Parameters.Add("@Directorate", SqlDbType.NVarChar, 1000).SourceColumn = "Directorate"
        cmd.Parameters.Add("@Div", SqlDbType.NVarChar, 1000).SourceColumn = "Div"
        cmd.Parameters.Add("@Branch", SqlDbType.NVarChar, 1000).SourceColumn = "Branch"
        cmd.Parameters.Add("@IT_Cyber_REQ", SqlDbType.NVarChar, 1000).SourceColumn = "IT_Cyber_REQ"
        cmd.Parameters.Add("@Emerging_REQ", SqlDbType.NVarChar, 1000).SourceColumn = "Emerging_REQ"
        cmd.Parameters.Add("@CPA_Topic", SqlDbType.NVarChar, 1000).SourceColumn = "CPA_Topic"
        cmd.Parameters.Add("@PBR_Submission", SqlDbType.NVarChar, 1000).SourceColumn = "PBR_Submission"
        cmd.Parameters.Add("@UPL_Submission", SqlDbType.NVarChar, 1000).SourceColumn = "UPL_Submission"
        cmd.Parameters.Add("@Contract_Num", SqlDbType.NVarChar, 1000).SourceColumn = "Contract_Num"
        cmd.Parameters.Add("@Task_Order_Num", SqlDbType.NVarChar, 1000).SourceColumn = "Task_Order_Num"
        cmd.Parameters.Add("@Award_Target_Date", SqlDbType.DateTime).SourceColumn = "Award_Target_Date"
        cmd.Parameters.Add("@POP_Exp_Date", SqlDbType.DateTime).SourceColumn = "POP_Exp_Date"
        cmd.Parameters.Add("@Contractor_ManYear_Equiv_CME", SqlDbType.NVarChar, 100).SourceColumn = "Contractor_ManYear_Equiv_CME"
        cmd.Parameters.Add("@COR_Email", SqlDbType.NVarChar, 1000).SourceColumn = "COR_Email"
        cmd.Parameters.Add("@POC_Email", SqlDbType.NVarChar, 1000).SourceColumn = "POC_Email"
        cmd.Parameters.Add("@Review_POC_Email", SqlDbType.NVarChar, 1000).SourceColumn = "Review_POC_Email"
        cmd.Parameters.Add("@MDEP_Functional_Email", SqlDbType.NVarChar, 1000).SourceColumn = "MDEP_Functional_Email"
        cmd.Parameters.Add("@Notification_List_Emails", SqlDbType.NVarChar, 1000).SourceColumn = "Notification_List_Emails"
        cmd.Parameters.Add("@Gen_Comments_Notes", SqlDbType.NVarChar, 1000).SourceColumn = "Gen_Comments_Notes"
        cmd.Parameters.Add("@JUON", SqlDbType.NVarChar, 1000).SourceColumn = "JUON"
        cmd.Parameters.Add("@ISR_Flag", SqlDbType.NVarChar, 1000).SourceColumn = "ISR_Flag"
        cmd.Parameters.Add("@Cost_Model", SqlDbType.NVarChar, 1000).SourceColumn = "Cost_Model"
        cmd.Parameters.Add("@Combat_Loss", SqlDbType.NVarChar, 1000).SourceColumn = "Combat_Loss"
        cmd.Parameters.Add("@Cost_Location", SqlDbType.NVarChar, 1000).SourceColumn = "Cost_Location"
        cmd.Parameters.Add("@Cat_A_Code", SqlDbType.NVarChar, 1000).SourceColumn = "Cat_A_Code"
        cmd.Parameters.Add("@CBS_Code", SqlDbType.NVarChar, 1000).SourceColumn = "CBS_Code"
        cmd.Parameters.Add("@MIP_Proj_Code", SqlDbType.NVarChar, 1000).SourceColumn = "MIP_Proj_Code"
        cmd.Parameters.Add("@SS_Priority", SqlDbType.NVarChar, 1000).SourceColumn = "SS_Priority"
        cmd.Parameters.Add("@Commit_Group", SqlDbType.NVarChar, 1000).SourceColumn = "Commit_Group"
        cmd.Parameters.Add("@SS_Cap", SqlDbType.NVarChar, 1000).SourceColumn = "SS_Cap"
        cmd.Parameters.Add("@Strategic_BIN", SqlDbType.NVarChar, 1000).SourceColumn = "Strategic_BIN"
        cmd.Parameters.Add("@LIN", SqlDbType.NVarChar, 1000).SourceColumn = "LIN"
        cmd.Parameters.Add("@REQ_Type", SqlDbType.NVarChar, 1000).SourceColumn = "REQ_Type"
        cmd.Parameters.Add("@DD_Priority", SqlDbType.NVarChar, 1000).SourceColumn = "DD_Priority"
        cmd.Parameters.Add("@Portfolio", SqlDbType.NVarChar, 1000).SourceColumn = "Portfolio"
        cmd.Parameters.Add("@DD_Cap", SqlDbType.NVarChar, 1000).SourceColumn = "DD_Cap"
        cmd.Parameters.Add("@JNT_Cap_Area", SqlDbType.NVarChar, 1000).SourceColumn = "JNT_Cap_Area"
        cmd.Parameters.Add("@TBM_Cost_Pool", SqlDbType.NVarChar, 1000).SourceColumn = "TBM_Cost_Pool"
        cmd.Parameters.Add("@TBM_Tower", SqlDbType.NVarChar, 1000).SourceColumn = "TBM_Tower"
        cmd.Parameters.Add("@APMS_AITR_Num", SqlDbType.NVarChar, 1000).SourceColumn = "APMS_AITR_Num"
        cmd.Parameters.Add("@Zero_Trust_Cap", SqlDbType.NVarChar, 1000).SourceColumn = "Zero_Trust_Cap"
        cmd.Parameters.Add("@Assoc_Directives", SqlDbType.NVarChar, 1000).SourceColumn = "Assoc_Directives"
        cmd.Parameters.Add("@Cloud_IND", SqlDbType.NVarChar, 1000).SourceColumn = "Cloud_IND"
        cmd.Parameters.Add("@Strat_Cyber_Sec_PGM", SqlDbType.NVarChar, 1000).SourceColumn = "Strat_Cyber_Sec_PGM"
        cmd.Parameters.Add("@Notes", SqlDbType.VarChar, 1000).SourceColumn = "Notes"
        cmd.Parameters.Add("@FF_1", SqlDbType.NVarChar, 1000).SourceColumn = "FF_1"
        cmd.Parameters.Add("@FF_2", SqlDbType.NVarChar, 1000).SourceColumn = "FF_2"
        cmd.Parameters.Add("@FF_3", SqlDbType.NVarChar, 1000).SourceColumn = "FF_3"
        cmd.Parameters.Add("@FF_4", SqlDbType.NVarChar, 1000).SourceColumn = "FF_4"
        cmd.Parameters.Add("@FF_5", SqlDbType.NVarChar, 1000).SourceColumn = "FF_5"
        cmd.Parameters.Add("@Attach_File_Name", SqlDbType.NVarChar, 250).SourceColumn = "Attach_File_Name"
        cmd.Parameters.Add("@Attach_File_Bytes", SqlDbType.VarBinary, -1).SourceColumn = "Attach_File_Bytes"
        cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).SourceColumn = "Status"
        cmd.Parameters.Add("@Invalid", SqlDbType.Bit).SourceColumn = "Invalid"
        cmd.Parameters.Add("@Val_Error", SqlDbType.NVarChar, -1).SourceColumn = "Val_Error"
        cmd.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date"
        cmd.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User"
        cmd.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date"
        cmd.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User"
        If isUpdate Then
            cmd.Parameters("@CMD_PGM_REQ_ID").SourceVersion = DataRowVersion.Original
        End If
    End Sub
End Class
