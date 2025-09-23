Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.Data.SqlClient
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports Workspace.current.GBL_Assembly


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.CMD_PGM_REQ_ID_XFTV
    Public Class MainClass
        ' Global variables
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As SpreadsheetArgs

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
			
            Return xftv
        End Function

        Private Function Save_REQ_Base_Info() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)

                        Dim Sql As String = "SELECT * FROM XFC_CMD_PGM_REQ"
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
		                    Dim req_ID_Col = xftvRow.Item("CMD_PGM_REQ_ID")
                            Dim targetRow As New DataRow()
                            Dim isInsert As Boolean = False

                            If req_ID_Col Is Nothing Then
                                targetRow = dt.NewRow()
                                isInsert = True
                            Else
                                ' Find matching DataRow in dt by CMD_PGM_REQ_ID (if provided)
                                Dim cellValObj = req_ID_Col.OriginalValue
                                targetRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
                            End If

		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = GBL_Helper.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next

		                    ' If this is an insert, add the new row to the DataTable
		                    If isInsert Then
		                    	dt.Rows.Add(targetRow)
		                    End If
		                Next
		
		                ' Persist changes back to the DB using the configured adapter
		                sqa.Update(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function

    End Class
End Namespace