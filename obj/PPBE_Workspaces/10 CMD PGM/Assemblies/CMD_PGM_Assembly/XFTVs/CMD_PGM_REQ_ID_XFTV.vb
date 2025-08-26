' filepath: /workspaces/OneStream_Speed/obj/PPBE_Workspaces/10 CMD PGM/Assemblies/CMD_PGM_Assembly/XFTVs/CMD_PGM_REQ_ID_XFTV.vb
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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.CMD_PGM_REQ_ID_XFTV
    Public Class MainClass
        ' Global variables
        Private Shared si As SessionInfo
        Private Shared globals As BRGlobals
        Private Shared api As Object
        Private Shared args As SpreadsheetArgs

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
            ' Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args

            Try
                Select Case args.FunctionType
                    Case SpreadsheetFunctionType.GetCustomSubstVarsInUse
                        Return New List(Of String)()
                    Case SpreadsheetFunctionType.GetTableView
                        Return GetTableView(si, args)
                    Case SpreadsheetFunctionType.SaveTableView
                        Return SaveTableView(si, args)
                    Case Else
                        Return Nothing
                End Select
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function GetTableView() As DataTable
            Dim dt As New DataTable()
            Dim sql As String = "
                SELECT 
                    CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, Gen_Comments_Notes, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code, SS_Priority, Commit_Group, SS_Cap, Strategic_BIN, LIN, REQ_Type, DD_Priority, Portfolio, DD_Cap, JNT_Cap_Area, TBM_Cost_Pool, TBM_Tower, APMS_AITR_Num, Zero_Trust_Cap, Assoc_Directives, Cloud_IND, Strat_Cyber_Sec_PGM, Notes, FF_1, FF_2, FF_3, FF_4, FF_5, Attach_File_Name, Attach_File_Bytes, Status, Invalid, Val_Error, Create_Date, Create_User, Update_Date, Update_User
                FROM XFC_CMD_PGM_REQ"
            Using dbConn As DbConnection = BRApi.Database.CreateDbConnection(si)
                dbConn.Open()
                Using cmd As DbCommand = dbConn.CreateCommand()
                    cmd.CommandText = sql
                    Using da As DbDataAdapter = BRApi.Database.CreateDataAdapter(cmd)
                        da.Fill(dt)
                    End Using
                End Using
            End Using
            Return dt
        End Function

        Private Function SaveTableView() As Object
            Dim dt As DataTable = args.TableView
            If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return Nothing

            Using dbConn As DbConnection = BRApi.Database.CreateDbConnection(si)
                dbConn.Open()
                Using tran As DbTransaction = dbConn.BeginTransaction()
                    Try
                        For Each row As DataRow In dt.Rows
                            Dim cmd As DbCommand = dbConn.CreateCommand()
                            cmd.Transaction = tran

                            Select Case row.RowState
                                Case DataRowState.Added
                                    cmd.CommandText = "
                                        INSERT INTO XFC_CMD_PGM_REQ (
                                            CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, Gen_Comments_Notes, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code, SS_Priority, Commit_Group, SS_Cap, Strategic_BIN, LIN, REQ_Type, DD_Priority, Portfolio, DD_Cap, JNT_Cap_Area, TBM_Cost_Pool, TBM_Tower, APMS_AITR_Num, Zero_Trust_Cap, Assoc_Directives, Cloud_IND, Strat_Cyber_Sec_PGM, Notes, FF_1, FF_2, FF_3, FF_4, FF_5, Attach_File_Name, Attach_File_Bytes, Status, Invalid, Val_Error, Create_Date, Create_User, Update_Date, Update_User
                                        ) VALUES (
                                            @CMD_PGM_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @REQ_ID, @Title, @Description, @Justification, @Entity, @APPN, @MDEP, @APE9, @Dollar_Type, @Obj_Class, @CType, @UIC, @Cost_Methodology, @Impact_Not_Funded, @Risk_Not_Funded, @Cost_Growth_Justification, @Must_Fund, @Funding_Src, @Army_Init_Dir, @CMD_Init_Dir, @Activity_Exercise, @Directorate, @Div, @Branch, @IT_Cyber_REQ, @Emerging_REQ, @CPA_Topic, @PBR_Submission, @UPL_Submission, @Contract_Num, @Task_Order_Num, @Award_Target_Date, @POP_Exp_Date, @Contractor_ManYear_Equiv_CME, @COR_Email, @POC_Email, @Review_POC_Email, @MDEP_Functional_Email, @Notification_List_Emails, @Gen_Comments_Notes, @JUON, @ISR_Flag, @Cost_Model, @Combat_Loss, @Cost_Location, @Cat_A_Code, @CBS_Code, @MIP_Proj_Code, @SS_Priority, @Commit_Group, @SS_Cap, @Strategic_BIN, @LIN, @REQ_Type, @DD_Priority, @Portfolio, @DD_Cap, @JNT_Cap_Area, @TBM_Cost_Pool, @TBM_Tower, @APMS_AITR_Num, @Zero_Trust_Cap, @Assoc_Directives, @Cloud_IND, @Strat_Cyber_Sec_PGM, @Notes, @FF_1, @FF_2, @FF_3, @FF_4, @FF_5, @Attach_File_Name, @Attach_File_Bytes, @Status, @Invalid, @Val_Error, @Create_Date, @Create_User, @Update_Date, @Update_User
                                        )"
                                    AddParameters(cmd, row)
                                    cmd.ExecuteNonQuery()
                                Case DataRowState.Modified
                                    cmd.CommandText = "
                                        UPDATE XFC_CMD_PGM_REQ SET"
                        tran.Rollback()
                        Throw
                    End Try
                End Using
            End Using
            Return Nothing
        End Function

        Private Sub AddParameters(cmd As DbCommand, row As DataRow)
            ' Add all parameters for insert/update
            Dim colNames = New String() {
                "CMD_PGM_REQ_ID", "WFScenario_Name", "WFCMD_Name", "WFTime_Name", "REQ_ID", "Title", "Description", "Justification", "Entity", "APPN", "MDEP", "APE9", "Dollar_Type", "Obj_Class", "CType", "UIC", "Cost_Methodology", "Impact_Not_Funded", "Risk_Not_Funded", "Cost_Growth_Justification", "Must_Fund", "Funding_Src", "Army_Init_Dir", "CMD_Init_Dir", "Activity_Exercise", "Directorate", "Div", "Branch", "IT_Cyber_REQ", "Emerging_REQ", "CPA_Topic", "PBR_Submission", "UPL_Submission", "Contract_Num", "Task_Order_Num", "Award_Target_Date", "POP_Exp_Date", "Contractor_ManYear_Equiv_CME", "COR_Email", "POC_Email", "Review_POC_Email", "MDEP_Functional_Email", "Notification_List_Emails", "Gen_Comments_Notes", "JUON", "ISR_Flag", "Cost_Model", "Combat_Loss", "Cost_Location", "Cat_A_Code", "CBS_Code", "MIP_Proj_Code", "SS_Priority", "Commit_Group", "SS_Cap", "Strategic_BIN", "LIN", "REQ_Type", "DD_Priority", "Portfolio", "DD_Cap", "JNT_Cap_Area", "TBM_Cost_Pool", "TBM_Tower", "APMS_AITR_Num", "Zero_Trust_Cap", "Assoc_Directives", "Cloud_IND", "Strat_Cyber_Sec_PGM", "Notes", "FF_1", "FF_2", "FF_3", "FF_4", "FF_5", "Attach_File_Name", "Attach_File_Bytes", "Status", "Invalid", "Val_Error", "Create_Date", "Create_User", "Update_Date", "Update_User"
            }
            For Each col In colNames
                Dim value As Object = If(row.Table.Columns.Contains(col), row(col), DBNull.Value)
                cmd.Parameters.Add(BRApi.Database.CreateDbParameter(cmd, "@" & col, value))
            Next
        End Sub
    End Class
End Namespace