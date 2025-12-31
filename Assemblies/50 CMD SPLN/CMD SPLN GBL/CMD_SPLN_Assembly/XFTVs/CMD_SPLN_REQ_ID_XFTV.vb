Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.CMD_SPLN_REQ_ID_XFTV
    Public Class MainClass
        ' Global variables
        Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As SpreadsheetArgs
				
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
            ' Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args

            Try
'BRApi.ErrorLog.LogMessage(si,$"Hit {args.TableViewName}")
                Select Case args.FunctionType
                    Case SpreadsheetFunctionType.GetCustomSubstVarsInUse
                        Return New List(Of String)()
                    Case SpreadsheetFunctionType.GetTableView
						If args.CustSubstVarsAlreadyResolved IsNot Nothing Then
						    Dim json As String = JsonSerializer.Serialize(args.CustSubstVarsAlreadyResolved)
						    Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(json)
						    BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName, String.Empty, bytes)
						Else
						    BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName, String.Empty, Nothing)
						End If
'BRApi.ErrorLog.LogMessage(si,$"Hit {args.CustSubstVarsAlreadyResolved.count()}")
							
						If args.TableViewName = "REQ_Base_Info"
                      		Return Get_REQ_Base_Info()							
						End If
                    Case SpreadsheetFunctionType.SaveTableView
						Dim uState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName)
						
						If uState IsNot Nothing Then
						    Dim jsonString As String = System.Text.Encoding.UTF8.GetString(uState.BinaryValue)
						    args.CustSubstVarsAlreadyResolved = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonString)
						End If
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

#Region "Get XFTVs"
#Region "Get BaseInfo"
        Private Function Get_REQ_Base_Info() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleList")
			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
			End If
			If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
'Brapi.ErrorLog.LogMessage(si, "ID" & RequirementID)
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

            Dim sql As String = $"SELECT Req.CMD_SPLN_REQ_ID,Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name,
										 Req.REQ_ID, Req.Title, Req.Description, Req.Justification, Req.Entity, Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, 
									     Req.Obj_Class,Req.CType,Req.UIC,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded,Req.Cost_Growth_Justification, Req.Must_Fund, 
										 Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise, Req.Directorate, Req.Div, Req.Branch, Req.IT_Cyber_REQ, Req.Emerging_REQ, 
										 Req.CPA_Topic, Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num, Req.Award_Target_Date, Req.POP_Exp_Date, 
									     Req.CME, Req.COR_Email, Req.POC_Email, Req.Review_POC_Email, Req.MDEP_Functional_Email, Req.Notification_List_Emails,
									     Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Status,Req.Create_Date,Req.Create_User,Req.Update_Date,Req.Update_User,Req.Related_REQs,Req.Demotion_Comment,Att.Attach_File_Name
                				FROM XFC_CMD_SPLN_REQ As Req
								LEFT JOIN XFC_CMD_SPLN_REQ_Attachment AS Att
								ON Req.CMD_SPLN_REQ_ID = Att.CMD_SPLN_REQ_ID
			 					WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								And REQ_ID = '{RequirementID}'
								And Entity  = '{Entity}'
								"
			
'Brapi.ErrorLog.LogMessage(si,"SQL" & sql )
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		End If
        End Function
#End Region

#End Region

#Region "Save XFTVs"
#Region "Save BaseInfo"
        Private Function Save_REQ_Base_Info() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'BRApi.ErrorLog.LogMessage(si,$"Hit Save {args.CustSubstVarsAlreadyResolved.XFGetValue("BL_REQPRO_FundCenter","NA")}")

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 2")
                        Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_SPLN_REQ
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 3")
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, dt, Sql, sqlparams)
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 4")
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
'brapi.ErrorLog.LogMessage(si,"title = " & xftvRow.Item("CMD_SPLN_REQ_ID").Value)						
		                 
							Dim req_ID_Col = xftvRow.Item("CMD_SPLN_REQ_ID")
                            Dim targetRow As DataRow
                            Dim isInsert As Boolean = False
							'BRApi.ErrorLog.LogMessage(si,$"Hit Save 5")
   							Dim cellValObj = req_ID_Col.OriginalValue
                                targetRow = dt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("REQ_ID") = xftvRow.Item("REQ_ID").Value
								targetRow("Title") = xftvRow.Item("Title").Value
								targetRow("Description") = xftvRow.Item("Description").Value
								targetRow("Justification") = xftvRow.Item("Justification").Value
								targetRow("Entity") = xftvRow.Item("Entity").Value
								targetRow("APPN") = xftvRow.Item("APPN").Value
								targetRow("MDEP") = xftvRow.Item("MDEP").Value
								targetRow("APE9") = xftvRow.Item("APE9").Value
								targetRow("Dollar_Type")= xftvRow.Item("Dollar_Type").Value
								targetRow("Obj_Class") = xftvRow.Item("Obj_Class").Value
								targetRow("CType") = xftvRow.Item("CType").Value
								targetRow("UIC") = xftvRow.Item("UIC").Value
								targetRow("Cost_Methodology") = xftvRow.Item("Cost_Methodology").Value
								targetRow("Impact_Not_Funded") = xftvRow.Item("Impact_Not_Funded").Value
								targetRow("Risk_Not_Funded") = xftvRow.Item("Risk_Not_Funded").Value
								targetRow("Cost_Growth_Justification") = xftvRow.Item("Cost_Growth_Justification").Value
								targetRow("Must_Fund") = xftvRow.Item("Must_Fund").Value
								targetRow("Funding_Src") = xftvRow.Item("Funding_Src").Value
								targetRow("Army_Init_Dir") = xftvRow.Item("Army_Init_Dir").Value
								targetRow("CMD_Init_Dir") = xftvRow.Item("CMD_Init_Dir").Value
								targetRow("Activity_Exercise") = xftvRow.Item("Activity_Exercise").Value
								targetRow("Directorate") = xftvRow.Item("Directorate").Value
								targetRow("Div") = xftvRow.Item("Div").Value
								targetRow("Branch") = xftvRow.Item("Branch").Value
								targetRow("IT_Cyber_REQ") = xftvRow.Item("IT_Cyber_REQ").Value
								targetRow("Emerging_REQ") = xftvRow.Item("Emerging_REQ").Value
								targetRow("CPA_Topic") = xftvRow.Item("CPA_Topic").Value
								targetRow("PBR_Submission") = xftvRow.Item("PBR_Submission").Value
								targetRow("UPL_Submission") = xftvRow.Item("UPL_Submission").Value
								targetRow("Contract_Num") = xftvRow.Item("Contract_Num").Value
								targetRow("Task_Order_Num") = xftvRow.Item("Task_Order_Num").Value
								If String.IsNullOrWhiteSpace(xftvRow.Item("Award_Target_Date").Value) Then
									
								targetRow("Award_Target_Date") = DBNull.Value
								Else
									targetRow("Award_Target_Date") = xftvRow.Item("Award_Target_Date").Value
								End If 
								
								If String.IsNullOrWhiteSpace(xftvRow.Item("POP_Exp_Date").Value) Then
								
								targetRow("POP_Exp_Date") = DBNull.Value
								Else
										targetRow("POP_Exp_Date") = xftvRow.Item("POP_Exp_Date").Value
								End If 						
								targetRow("Contractor_ManYear_Equiv_CME") = xftvRow.Item("Contractor_ManYear_Equiv_CME").Value
								targetRow("COR_Email") = xftvRow.Item("COR_Email").Value
								targetRow("POC_Email") = xftvRow.Item("POC_Email").Value
								targetRow("Review_POC_Email") = xftvRow.Item("Review_POC_Email").Value
								targetRow("MDEP_Functional_Email") = xftvRow.Item("MDEP_Functional_Email").Value
								targetRow("Notification_List_Emails") = xftvRow.Item("Notification_List_Emails").Value
								targetRow("Gen_Comments_Notes") = xftvRow.Item("Gen_Comments_Notes").Value

								targetRow("FF_1") = xftvRow.Item("FF_1").Value
								targetRow("FF_2") = xftvRow.Item("FF_2").Value
								targetRow("FF_3") = xftvRow.Item("FF_3").Value
								targetRow("FF_4") = xftvRow.Item("FF_4").Value
								targetRow("FF_5") = xftvRow.Item("FF_5").Value
							
								
								
								targetRow("Status") = xftvRow.Item("Status").Value
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 6")
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
'BRApi.ErrorLog.LogMessage(si,$"Hit Save" & dirtyColList.Count)
                            For Each colName As String In dirtyColList
								'BRApi.ErrorLog.LogMessage(si,$"Hit Save Column Name: " & colName)
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                           	'BRApi.ErrorLog.LogMessage(si,$"Hit Save Column")
								Next
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 7")
		                    ' If this is an insert, add the new row to the DataTable
'		                    If isInsert Then
'		                    	dt.Rows.Add(targetRow)
'		                    End If
  					 Catch ex As Exception
                       'BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
                    End Try

		                Next
					
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 8")
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 

#End Region

    End Class
End Namespace