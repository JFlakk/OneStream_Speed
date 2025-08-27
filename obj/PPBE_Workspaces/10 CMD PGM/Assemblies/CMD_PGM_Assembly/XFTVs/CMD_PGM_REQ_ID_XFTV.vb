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
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As SpreadsheetArgs

'' Global dictionary mapping Parameter to ColumnName
'        Private ParamToColDict As New Dictionary(Of String, String) From {
'            {"@Entity", "Entity"},
'            {"@APPN", "APPN"},
'            {"@MDEP", "MDEP"},
'            {"@APE9", "APE9"},
'            {"@Dollar_Type", "Dollar_Type"},
'            {"@Obj_Class", "Obj_Class"},
'            {"@CType", "CType"},
'            {"@UIC", "UIC"},
'            {"@Cost_Methodology", "Cost_Methodology"},
'            {"@Impact_Not_Funded", "Impact_Not_Funded"},
'            {"@Risk_Not_Funded", "Risk_Not_Funded"},
'            {"@Cost_Growth_Justification", "Cost_Growth_Justification"},
'            {"@Must_Fund", "DL_REQPRO_YesNo"},
'            {"@Funding_Src", "Funding_Src"},
'            {"@Army_Init_Dir", "Army_Init_Dir"},
'            {"@CMD_Init_Dir", "CMD_Init_Dir"},
'            {"@Activity_Exercise", "Activity_Exercise"},
'            {"@Directorate", "Directorate"},
'            {"@Div", "Div"},
'            {"@Branch", "Branch"},
'            {"@IT_Cyber_REQ", "IT_Cyber_REQ"},
'            {"@Emerging_REQ", "Emerging_REQ"},
'            {"@CPA_Topic", "CPA_Topic"},
'            {"@PBR_Submission", "PBR_Submission"},
'            {"@UPL_Submission", "UPL_Submission"},
'            {"@Contract_Num", "Contract_Num"},
'            {"@Task_Order_Num", "Task_Order_Num"},
'            {"@Award_Target_Date", "Award_Target_Date"},
'            {"@POP_Exp_Date", "POP_Exp_Date"},
'            {"@Contractor_ManYear_Equiv_CME", "Contractor_ManYear_Equiv_CME"},
'            {"@COR_Email", "COR_Email"},
'            {"@POC_Email", "POC_Email"},
'            {"@Review_POC_Email", "Review_POC_Email"},
'            {"@MDEP_Functional_Email", "MDEP_Functional_Email"},
'            {"@Notification_List_Emails", "Notification_List_Emails"},
'            {"@Gen_Comments_Notes", "Gen_Comments_Notes"},
'            {"@JUON", "JUON"},
'            {"@ISR_Flag", "ISR_Flag"},
'            {"@Cost_Model", "Cost_Model"},
'            {"@Combat_Loss", "Combat_Loss"},
'            {"@Cost_Location", "Cost_Location"},
'            {"@Cat_A_Code", "Cat_A_Code"},
'            {"@CBS_Code", "CBS_Code"},
'            {"@MIP_Proj_Code", "MIP_Proj_Code"},
'            {"@SS_Priority", "SS_Priority"},
'            {"@Commit_Group", "Commit_Group"},
'            {"@SS_Cap", "SS_Cap"},
'            {"@Strategic_BIN", "Strategic_BIN"},
'            {"@LIN", "LIN"},
'            {"@REQ_Type", "REQ_Type"},
'            {"@DD_Priority", "DD_Priority"},
'            {"@Portfolio", "Portfolio"},
'            {"@DD_Cap", "DD_Cap"},
'            {"@JNT_Cap_Area", "JNT_Cap_Area"},
'            {"@TBM_Cost_Pool", "TBM_Cost_Pool"},
'            {"@TBM_Tower", "TBM_Tower"},
'            {"@APMS_AITR_Num", "APMS_AITR_Num"},
'            {"@Zero_Trust_Cap", "Zero_Trust_Cap"},
'            {"@Assoc_Directives", "Assoc_Directives"},
'            {"@Cloud_IND", "Cloud_IND"},
'            {"@Strat_Cyber_Sec_PGM", "Strat_Cyber_Sec_PGM"},
'            {"@Notes", "Notes"},
'            {"@FF_1", "FF_1"},
'            {"@FF_2", "FF_2"},
'            {"@FF_3", "FF_3"},
'            {"@FF_4", "FF_4"},
'            {"@FF_5", "FF_5"},
'            {"@Attach_File_Name", "Attach_File_Name"},
'            {"@Attach_File_Bytes", "Attach_File_Bytes"},
'            {"@Status", "Status"}
'        }

Private ParamToColDict As New Dictionary(Of String, String) From {
            {"Must_Fund", "DL_REQPRO_YesNo"},
			{"Funding_Src", "DL_REQPRO_YesNo"}
        }
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
            ' Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args

            Try
				BRApi.ErrorLog.LogMessage(si,$"Hit {args.TableViewName}")
                Select Case args.FunctionType
                    Case SpreadsheetFunctionType.GetCustomSubstVarsInUse
                        Return New List(Of String)()
                    Case SpreadsheetFunctionType.GetTableView
						If args.TableViewName = "REQ_Base_Info"
                        	Return Get_REQ_Base_Info()
						End If
                    Case SpreadsheetFunctionType.SaveTableView
						If args.TableViewName = "REQ_Base_Info"
                       	 	Return Save_REQ_Base_Info()
						End If
                    Case Else
                        Return Nothing
                End Select
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function Get_REQ_Base_Info() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
            Dim sql As String = "SELECT CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, Gen_Comments_Notes, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code, SS_Priority, Commit_Group, SS_Cap, Strategic_BIN, LIN, REQ_Type, DD_Priority, Portfolio, DD_Cap, JNT_Cap_Area, TBM_Cost_Pool, TBM_Tower, APMS_AITR_Num, Zero_Trust_Cap, Assoc_Directives, Cloud_IND, Strat_Cyber_Sec_PGM, Notes, FF_1, FF_2, FF_3, FF_4, FF_5, Attach_File_Name, Attach_File_Bytes, Status, Invalid, Val_Error, Create_Date, Create_User, Update_Date, Update_User
                				FROM XFC_CMD_PGM_REQ"
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            ' Update header columns with parameter info and sync header row to xftv.Columns
            Dim headerRow As TableViewRow = xftv.Rows.FirstOrDefault(Function(r) r.IsHeader)
            
            ' Update the TableView's Columns first
            For Each xftvCol As TableViewColumn In xftv.Columns
                Dim paramName As String = GetParameterFromColumnName(xftvCol.Name)
                If Not String.IsNullOrEmpty(paramName) Then
                    BRApi.ErrorLog.LogMessage(si, $"Hit - {xftvCol.Name} : {paramName}")
                    xftvCol.ParameterName = paramName
                    xftvCol.WorkspaceName = "CMD_PGM"
                    'xftvCol.ShowCellComment = True
					'xftvCol.AllowCellDataType = True
					xftvCol.UseDisplay = True
                End If
            Next
            
            ' If a header row exists, ensure its items reference the updated columns
            If headerRow IsNot Nothing Then
                ' Make a copy of the keys to avoid modifying collection while iterating
                Dim keys As List(Of String) = headerRow.Items.Keys.ToList()
                For Each key As String In keys
                    Dim colName As String = key
                    Dim updatedCol As TableViewColumn = xftv.Columns.FirstOrDefault(Function(c) String.Equals(c.Name, colName, StringComparison.OrdinalIgnoreCase))
                    If updatedCol IsNot Nothing Then
                        headerRow.Item(colName) = updatedCol
                    End If
                Next
            End If
			
            Return xftv
        End Function

        Private Function Save_REQ_Base_Info() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then Return Nothing

            	Using dbConn As DbConnection = BRApi.Database.CreateDbConnection(si)
'                dbConn.Open()
'                Using tran As DbTransaction = dbConn.BeginTransaction()
'                    Try
''                        For Each row As DataRow In dt.Rows
''                            Dim cmd As DbCommand = dbConn.CreateCommand()
''                            cmd.Transaction = tran

''                            Select Case row.RowState
''                                Case DataRowState.Added
''                                    cmd.CommandText = "
''                                        INSERT INTO XFC_CMD_PGM_REQ (
''                                            CMD_PGM_REQ_ID, WFScenario_Name, WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, Gen_Comments_Notes, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code, SS_Priority, Commit_Group, SS_Cap, Strategic_BIN, LIN, REQ_Type, DD_Priority, Portfolio, DD_Cap, JNT_Cap_Area, TBM_Cost_Pool, TBM_Tower, APMS_AITR_Num, Zero_Trust_Cap, Assoc_Directives, Cloud_IND, Strat_Cyber_Sec_PGM, Notes, FF_1, FF_2, FF_3, FF_4, FF_5, Attach_File_Name, Attach_File_Bytes, Status, Invalid, Val_Error, Create_Date, Create_User, Update_Date, Update_User
''                                        ) VALUES (
''                                            @CMD_PGM_REQ_ID, @WFScenario_Name, @WFCMD_Name, @WFTime_Name, @REQ_ID, @Title, @Description, @Justification, @Entity, @APPN, @MDEP, @APE9, @Dollar_Type, @Obj_Class, @CType, @UIC, @Cost_Methodology, @Impact_Not_Funded, @Risk_Not_Funded, @Cost_Growth_Justification, @Must_Fund, @Funding_Src, @Army_Init_Dir, @CMD_Init_Dir, @Activity_Exercise, @Directorate, @Div, @Branch, @IT_Cyber_REQ, @Emerging_REQ, @CPA_Topic, @PBR_Submission, @UPL_Submission, @Contract_Num, @Task_Order_Num, @Award_Target_Date, @POP_Exp_Date, @Contractor_ManYear_Equiv_CME, @COR_Email, @POC_Email, @Review_POC_Email, @MDEP_Functional_Email, @Notification_List_Emails, @Gen_Comments_Notes, @JUON, @ISR_Flag, @Cost_Model, @Combat_Loss, @Cost_Location, @Cat_A_Code, @CBS_Code, @MIP_Proj_Code, @SS_Priority, @Commit_Group, @SS_Cap, @Strategic_BIN, @LIN, @REQ_Type, @DD_Priority, @Portfolio, @DD_Cap, @JNT_Cap_Area, @TBM_Cost_Pool, @TBM_Tower, @APMS_AITR_Num, @Zero_Trust_Cap, @Assoc_Directives, @Cloud_IND, @Strat_Cyber_Sec_PGM, @Notes, @FF_1, @FF_2, @FF_3, @FF_4, @FF_5, @Attach_File_Name, @Attach_File_Bytes, @Status, @Invalid, @Val_Error, @Create_Date, @Create_User, @Update_Date, @Update_User
''                                        )"
''                                    AddParameters(cmd, row)
''                                    cmd.ExecuteNonQuery()
''                                Case DataRowState.Modified
''                                    cmd.CommandText = "
''                                        UPDATE XFC_CMD_PGM_REQ SET"
''                        tran.Rollback()
'                        Throw
'                    End Try
'                End Using
            End Using
			End If
            Return Nothing
        End Function

        ' Function to get parameter name by column name
        Public Function GetParameterFromColumnName(colName As String) As String
            If String.IsNullOrEmpty(colName) Then Return String.Empty

            ' One-off linear search (O(n))
            Dim found = ParamToColDict _
                .FirstOrDefault(Function(kvp) String.Equals(kvp.Key, colName, StringComparison.OrdinalIgnoreCase))
            Return If(found.Value, String.Empty)        
        End Function
    End Class
End Namespace