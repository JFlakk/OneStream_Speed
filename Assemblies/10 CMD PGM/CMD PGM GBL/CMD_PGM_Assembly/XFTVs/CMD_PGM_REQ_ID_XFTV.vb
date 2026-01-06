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


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.CMD_PGM_REQ_ID_XFTV
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
						Dim custSubstVars As New List(Of String)
						Return custSubstVars
                    Case SpreadsheetFunctionType.GetTableView
						If args.CustSubstVarsAlreadyResolved IsNot Nothing Then
						    Dim json As String = JsonSerializer.Serialize(args.CustSubstVarsAlreadyResolved)
						    Dim bytes As Byte() = System.Text.Encoding.UTF8.GetBytes(json)
						    BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName, String.Empty, bytes)
						Else
						    BRApi.State.SetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName, String.Empty, Nothing)
						End If
						Dim SubstVars_List As New List(Of String)
	'BRApi.ErrorLog.LogMessage(si,$"Hit {args.CustSubstVarsAlreadyResolved.count()}")
						If args.TableViewName = "REQ_Base_Info"
							'SubstVars_List.Add("BL_CMD_PGM_REQTitleList")
                      		Return Get_REQ_Base_Info()
							
						Else If args.TableViewName = "Create_New_REQ_Base_Info"
                      		Return Get_Create_New_REQ_Base_Info()	
							
						Else If args.TableViewName = "REQ_Base_Info_Param_Activity_Exercise"
							Return Get_REQ_Base_Info_Params("Activity_Exercise")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_CMD_INIT"
							Return Get_REQ_Base_Info_Params("CMD_Init_Dir")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_ARMY_INIT"
							Return Get_REQ_Base_Info_Params("Army_Init_Dir")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_FF_4"
							Return Get_REQ_Base_Info_Params("FF_4")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_FF_5"
							Return Get_REQ_Base_Info_Params("FF_5")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_Funding_Source"
							Return Get_REQ_Base_Info_Params("Funding_Src")
							
						Else If args.TableViewName = "REQ_Base_Info_Param_Risk_Not_Funded"
							Return Get_REQ_Base_Info_Params("Risk_Not_Funded")
							
						Else If args.TableViewName = "cWork_OOC"
							Return Get_REQ_cWork_OOC()
						
						Else If args.TableViewName = "SS_PEG"
							Return Get_REQ_SS_PEG()
						
						Else If args.TableViewName = "DD_PEG"
							Return Get_REQ_DD_PEG()
						
						End If
                    Case SpreadsheetFunctionType.SaveTableView
						Dim uState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName)
						
						If uState IsNot Nothing Then
						    Dim jsonString As String = System.Text.Encoding.UTF8.GetString(uState.BinaryValue)
						    args.CustSubstVarsAlreadyResolved = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonString)
						End If
					
						If args.TableViewName = "REQ_Base_Info"
							'Brapi.ErrorLog.LogMessage(si,"Here Baseinfo")
                       	 	Return Save_REQ_Base_Info()
							
						Else If args.TableViewName = "Create_New_REQ_Base_Info"
							'Brapi.ErrorLog.LogMessage(si,"Here create Baseinfo")
                       	 	Return Save_Create_New_REQ_Base_Info()
							
						Else If args.TableViewName = "cWork_OOC"
							'Brapi.ErrorLog.LogMessage(si,"Here cWork_OOC")
							Return Save_cWork_OOC()
							
						Else If args.TableViewName = "SS_PEG"
							'Brapi.ErrorLog.LogMessage(si,"Here SS_PEG")
							Return Save_SS_PEG()
							
						Else If args.TableViewName = "DD_PEG"
							'Brapi.ErrorLog.LogMessage(si,"Here DD_PEG")
							Return Save_DD_PEG()
							
						End If
							
						
                    Case Else
                        Return Nothing
                End Select
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

#Region "Get XFTVs"
#Region "Get Create New REQ BaseInfo"
        Private Function Get_Create_New_REQ_Base_Info() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
'				If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
'Brapi.ErrorLog.LogMessage(si, "ID" & RequirementID)
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID,Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name,
										 Req.REQ_ID,Req.Description, Req.Justification, Req.Entity, Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, 
									     Req.Obj_Class,Req.CType,Req.UIC,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded,Req.Cost_Growth_Justification, Req.Must_Fund, 
										 Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise, Req.Directorate, Req.Div, Req.Branch, Req.IT_Cyber_REQ, Req.Emerging_REQ, 
										 Req.CPA_Topic, Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num, Req.Award_Target_Date, Req.POP_Exp_Date, 
									     Req.Contractor_ManYear_Equiv_CME, Req.COR_Email, Req.POC_Email, Req.Review_POC_Email, Req.MDEP_Functional_Email, Req.Notification_List_Emails, Req.Gen_Comments_Notes,
									     Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Status,Req.Create_Date,Req.Create_User,Req.Update_Date,Req.Update_User,Req.Related_REQs,Att.Attach_File_Name,Req.Demotion_Comment,Aud.Orig_Flow
                				FROM XFC_CMD_PGM_REQ As Req
								LEFT JOIN XFC_CMD_PGM_REQ_Attachment AS Att
								ON Req.CMD_PGM_REQ_ID = Att.CMD_PGM_REQ_ID
								LEFT JOIN XFC_CMD_PGM_REQ_Details_Audit as Aud
								ON Req.CMD_PGM_REQ_ID = Aud.CMD_PGM_REQ_ID
			 					WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								AND 1 <> 1
								
								"
			
'Brapi.ErrorLog.LogMessage(si,"SQL" & sql )
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		'End If
        End Function
#End Region
#Region "Get BaseInfo"
        Private Function Get_REQ_Base_Info() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList")
			'Brapi.ErrorLog.LogMessage(si,"REQID XFTV" & ReqID)
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			'Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			'Dim Entity As String  =  REQ_ID_Split(0)
			'Dim RequirementID As String  = REQ_ID_Split(1)
			
'Brapi.ErrorLog.LogMessage(si, "ID" & RequirementID)
            xftv.CanModifyData = True
			'xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID,Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name,
										 Req.REQ_ID, Req.Title, Req.Description, Req.Justification, Req.Entity, Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, 
									     Req.Obj_Class,Req.CType,Req.UIC,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded,Req.Cost_Growth_Justification, Req.Must_Fund, 
										 Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise, Req.Directorate, Req.Div, Req.Branch, Req.IT_Cyber_REQ, Req.Emerging_REQ, 
										 Req.CPA_Topic, Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num, Req.Award_Target_Date, Req.POP_Exp_Date, 
									     Req.Contractor_ManYear_Equiv_CME, Req.COR_Email, Req.POC_Email, Req.Review_POC_Email, Req.MDEP_Functional_Email, Req.Notification_List_Emails, Req.Gen_Comments_Notes,
									     Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Status,Req.Create_Date,Req.Create_User,Req.Update_Date,Req.Update_User,Req.Related_REQs,Att.Attach_File_Name,Req.Demotion_Comment,Aud.Orig_Flow
                				FROM XFC_CMD_PGM_REQ As Req
								LEFT JOIN XFC_CMD_PGM_REQ_Attachment AS Att
								ON Req.CMD_PGM_REQ_ID = Att.CMD_PGM_REQ_ID
								LEFT JOIN XFC_CMD_PGM_REQ_Details_Audit as Aud
								ON Req.CMD_PGM_REQ_ID = Aud.CMD_PGM_REQ_ID
			 					WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{ReqID}'
								
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
#Region "Get BaseInfo Params"
        Private Function Get_REQ_Base_Info_Params(ByVal ColumnName As String) As TableView
'			Dim pivot_Columns As New List(Of String)
'			pivot_Columns.Add("Activity_Exercise")
'			pivot_Columns.Add("Army_Init_Dir")
'			pivot_Columns.Add("CMD_Init_Dir")
'			pivot_Columns.Add("Funding_Src")
'			pivot_Columns.Add("Risk_Not_Funded")
'			pivot_Columns.Add("FF_4")
'			pivot_Columns.Add("FF_5")
            Dim dt As New DataTable()
            Dim xftv As New TableView()
'			Dim columnListString = $"[{String.Join("],[", pivot_Columns)}]"
'			Dim transformedColumns = pivot_Columns.Select(Function(colName) $"ISNULL([{colName}], ' ') AS [{colName}]")
'			Dim selectcolumnListString = String.Join(", ", transformedColumns)
			
'Brapi.ErrorLog.LogMessage(si, "in Param rule")
            xftv.CanModifyData = False
			'xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
	
			Dim sql = $"SELECT Command,Cycle,Column_Name,Value
						FROM XFC_CMD_Cycle_Param_Values
						    WHERE 
						        Command = '{wfInfoDetails("CMDName")}' 
						        AND Cycle = 'All' 
								AND Column_Name = '{ColumnName}'"
				
			
			
			
'			Dim sql = $"SELECT Command,
'						    Cycle,
'						    {columnListString}
'						FROM 
'						(
'						    -- Source data for the pivot
'						    SELECT 
'						        Command, 
'						        Cycle, 
'						        Column_Name, 
'						        NULLIF(Value, '''') AS Value,
'						        ROW_NUMBER() OVER (PARTITION BY Command, [Column_Name] ORDER BY (SELECT NULL)) AS rn
'						    FROM XFC_CMD_Cycle_Param_Values
'						    WHERE 
'						        Command = '{wfInfoDetails("CMDName")}' 
'						        AND Cycle = 'All' 
'						) AS SourceData
'						PIVOT
'						(
'						    -- The aggregation to perform and the column to pivot
'						    MAX(Value) 
'						    FOR Column_Name IN 
'						    (
'						        {columnListString}
'						    )
'						) AS PivotTable"
			
'Brapi.ErrorLog.LogMessage(si,"SQL" & sql )
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
        End Function
#End Region
#Region "Get cWork OOC"
        Private Function Get_REQ_cWork_OOC() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			'xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT CMD_PGM_REQ_ID, 
										 WFScenario_Name, 
										 WFCMD_Name, WFTime_Name, REQ_ID, Title, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code  
			                             
                				FROM XFC_CMD_PGM_REQ
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								And REQ_ID = '{ReqID}'"
'Brapi.ErrorLog.LogMessage(si,"SQL" & sql )
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		End If
        End Function
#End Region
#Region "Get SS PEG"
        Private Function Get_REQ_SS_PEG() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			'xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID, 
										 Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name, Req.REQ_ID, Req.Title,Req.SS_Priority, 
									     Req.Commit_Group, Req.SS_Cap, Req.Strategic_BIN,Req.LIN,
										 Dtl.Unit_of_Measure,
										 Dtl.FY_1,
										 Dtl.FY_2,
										 Dtl.FY_3,
										 Dtl.FY_4,
										 Dtl.FY_5,
										 Req.Status,Req.APPN,Req.MDEP,Req.APE9,Req.Dollar_Type,Req.Obj_Class
		                		FROM XFC_CMD_PGM_REQ Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Dtl.Account ='Quantity'
							    WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{ReqID}'"
'Brapi.ErrorLog.LogMessage(si,"SQL" & sql )
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		End If
        End Function
#End Region
#Region "Get DD PEG"
        Private Function Get_REQ_DD_PEG() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT 
									    Req.CMD_PGM_REQ_ID, 
									    Req.WFScenario_Name, 
									    Req.WFCMD_Name, 
									    Req.WFTime_Name, 
									    Req.REQ_ID, 
									    Req.Title, 
									    Req.REQ_Type, 
									    Req.DD_Priority, 
									    Req.Portfolio, 
									    Req.DD_Cap, 
									    Req.JNT_Cap_Area, 
									    Req.TBM_Cost_Pool, 
									    Req.TBM_Tower, 
									    Req.APMS_AITR_Num, 
									    Req.Zero_Trust_Cap, 
									    Req.Assoc_Directives, 
									    Req.Cloud_IND, 
									    Req.Strat_Cyber_Sec_PGM, 
									    Req.Notes, 
									   MAX(CASE WHEN Dtl.Unit_of_Measure != 'Funding' and Dtl.Account != 'Quantity' THEN Dtl.Unit_of_Measure ELSE Null END)as Unit_of_Measure,
									
									  
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_1 ELSE Null END) AS FY_1,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_1 ELSE Null END) AS FY_1_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_2 ELSE Null END) AS FY_2,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_2 ELSE Null END) AS FY_2_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_3 ELSE Null END) AS FY_3,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_3 ELSE Null END) AS FY_3_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_4 ELSE Null END) AS FY_4,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_4 ELSE Null END) AS FY_4_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_5 ELSE Null END) AS FY_5,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_5 ELSE Null END) AS FY_5_Cost
									
									FROM 
									    XFC_CMD_PGM_REQ AS Req
									LEFT JOIN 
									    XFC_CMD_PGM_REQ_Details AS Dtl
									    ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
									    AND Req.WFScenario_Name = Dtl.WFScenario_Name
									    AND Req.WFCMD_Name = Dtl.WFCMD_Name
									    AND Req.WFTime_Name = Dtl.WFTime_Name
									WHERE 
									    Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
									    AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
									    AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
									    AND Req.REQ_ID = '{ReqID}'
									    
									    
									   
									GROUP BY
									    Req.CMD_PGM_REQ_ID,
									    Req.WFScenario_Name,
									    Req.WFCMD_Name,
									    Req.WFTime_Name,
									    Req.REQ_ID,
									    Req.Title,
									    Req.REQ_Type,
									    Req.DD_Priority,
									    Req.Portfolio,
									    Req.DD_Cap,
									    Req.JNT_Cap_Area,
									    Req.TBM_Cost_Pool,
									    Req.TBM_Tower,
									    Req.APMS_AITR_Num,
									    Req.Zero_Trust_Cap,
									    Req.Assoc_Directives,
									    Req.Cloud_IND,
									    Req.Strat_Cyber_Sec_PGM,
									    Req.Notes
									    
        							
			"
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
#Region "Save Create New REQ BaseInfo"
Private Function Save_Create_New_REQ_Base_Info() As Object
'			Dim REQ_ID_List As New List(Of String) 
'			REQ_ID_List = StringHelper.SplitString(args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList","NA")," ")
			
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	
			Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
			'Brapi.ErrorLog.LogMessage(si, " XFTV NewReqID = " & NewReqID)		
				
				
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)


                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
					sqlConn.open()
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
											

						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}	
						}
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql, sqlparams)

            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
					
		                 
							
                            Dim targetRow As DataRow
                            Dim isInsert As Boolean = False
							targetRow = dt.Select($"CMD_PGM_REQ_ID = '{NewReqID.xfconverttoGUID}'").FirstOrDefault()
   							
								If Not targetRow Is Nothing
								
								
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Description") = xftvRow.Item("Description").Value
								targetRow("Justification") = xftvRow.Item("Justification").Value
								targetRow("UIC") = xftvRow.Item("UIC").Value
								targetRow("Cost_Methodology") = xftvRow.Item("Cost_Methodology").Value
								targetRow("Impact_Not_Funded") = xftvRow.Item("Impact_Not_Funded").Value
								targetRow("Risk_Not_Funded") = xftvRow.Item("Risk_Not_Funded").Value
								targetRow("Cost_Growth_Justification") = xftvRow.Item("Cost_Growth_Justification").Value
								targetRow("Must_Fund") = xftvRow.Item("Must_Fund").Value
								targetRow("Funding_Src") = xftvRow.Item("Funding_Src").Value
								targetRow("Army_Init_Dir") = xftvRow.Item("Army_Init_Dir").Value
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
								
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 6")
                             End If
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))
							Next

  					 		Catch ex As Exception
                      			 'BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
                    		End Try

		                Next
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#Region "Save BaseInfo"
        Private Function Save_REQ_Base_Info() As Object
			'Dim REQ_ID_List As New List(Of String) 
		Dim REQID As String  = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList","NA")
			
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
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											AND REQ_ID = @REQ_ID"
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 3")
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
							New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = REQID}
						}
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql, sqlparams)

            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
					
		                 
							Dim req_ID_Col = xftvRow.Item("CMD_PGM_REQ_ID")
                            Dim targetRow As DataRow
                            Dim isInsert As Boolean = False
							
   							Dim reg_ID_GUID As GUID = New GUID(req_ID_Col.OriginalValue)
                                targetRow = dt.Select($"CMD_PGM_REQ_ID = '{reg_ID_GUID}'").FirstOrDefault()
								If Not targetRow Is Nothing
								'BRApi.ErrorLog.LogMessage(si,$"CMD_PGM_REQ_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'")
							
'								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
'								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
'								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
'								targetRow("REQ_ID") = xftvRow.Item("REQ_ID").Value
'								targetRow("Title") = xftvRow.Item("Title").Value
'								targetRow("Description") = xftvRow.Item("Description").Value
'								targetRow("Justification") = xftvRow.Item("Justification").Value
'								targetRow("Entity") = xftvRow.Item("Entity").Value
'								targetRow("APPN") = xftvRow.Item("APPN").Value
'								targetRow("MDEP") = xftvRow.Item("MDEP").Value
'								targetRow("APE9") = xftvRow.Item("APE9").Value
'								targetRow("Dollar_Type")= xftvRow.Item("Dollar_Type").Value
'								targetRow("Obj_Class") = xftvRow.Item("Obj_Class").Value
'								targetRow("CType") = xftvRow.Item("CType").Value
'								targetRow("UIC") = xftvRow.Item("UIC").Value
'								targetRow("Cost_Methodology") = xftvRow.Item("Cost_Methodology").Value
'								targetRow("Impact_Not_Funded") = xftvRow.Item("Impact_Not_Funded").Value
'								targetRow("Risk_Not_Funded") = xftvRow.Item("Risk_Not_Funded").Value
'								targetRow("Cost_Growth_Justification") = xftvRow.Item("Cost_Growth_Justification").Value
'								targetRow("Must_Fund") = xftvRow.Item("Must_Fund").Value
'								targetRow("Funding_Src") = xftvRow.Item("Funding_Src").Value
'								If String.IsNullOrWhiteSpace(xftvRow.Item("Army_Init_Dir").Value) Then
									
'								targetRow("Army_Init_Dir") = DBNull.Value
'								Else
'									targetRow("Army_Init_Dir") = xftvRow.Item("Army_Init_Dir").Value
'								End If 







'								targetRow("Army_Init_Dir") = xftvRow.Item("Army_Init_Dir").Value

'								targetRow("CMD_Init_Dir") = xftvRow.Item("CMD_Init_Dir").Value
'								targetRow("Activity_Exercise") = xftvRow.Item("Activity_Exercise").Value
'								targetRow("Directorate") = xftvRow.Item("Directorate").Value
'								targetRow("Div") = xftvRow.Item("Div").Value
'								targetRow("Branch") = xftvRow.Item("Branch").Value
'								targetRow("IT_Cyber_REQ") = xftvRow.Item("IT_Cyber_REQ").Value
'								targetRow("Emerging_REQ") = xftvRow.Item("Emerging_REQ").Value
'								targetRow("CPA_Topic") = xftvRow.Item("CPA_Topic").Value
'								targetRow("PBR_Submission") = xftvRow.Item("PBR_Submission").Value
'								targetRow("UPL_Submission") = xftvRow.Item("UPL_Submission").Value
'								targetRow("Contract_Num") = xftvRow.Item("Contract_Num").Value
'								targetRow("Task_Order_Num") = xftvRow.Item("Task_Order_Num").Value
'								If String.IsNullOrWhiteSpace(xftvRow.Item("Award_Target_Date").Value) Then
									
'								targetRow("Award_Target_Date") = DBNull.Value
'								Else
'									targetRow("Award_Target_Date") = xftvRow.Item("Award_Target_Date").Value
'								End If 
								
'								If String.IsNullOrWhiteSpace(xftvRow.Item("POP_Exp_Date").Value) Then
								
'								targetRow("POP_Exp_Date") = DBNull.Value
'								Else
'										targetRow("POP_Exp_Date") = xftvRow.Item("POP_Exp_Date").Value
'								End If 						
'								targetRow("Contractor_ManYear_Equiv_CME") = xftvRow.Item("Contractor_ManYear_Equiv_CME").Value
'								targetRow("COR_Email") = xftvRow.Item("COR_Email").Value
'								targetRow("POC_Email") = xftvRow.Item("POC_Email").Value
'								targetRow("Review_POC_Email") = xftvRow.Item("Review_POC_Email").Value
'								targetRow("MDEP_Functional_Email") = xftvRow.Item("MDEP_Functional_Email").Value
'								targetRow("Notification_List_Emails") = xftvRow.Item("Notification_List_Emails").Value
'								targetRow("Gen_Comments_Notes") = xftvRow.Item("Gen_Comments_Notes").Value

'								targetRow("FF_1") = xftvRow.Item("FF_1").Value
'								targetRow("FF_2") = xftvRow.Item("FF_2").Value
'								targetRow("FF_3") = xftvRow.Item("FF_3").Value
'								targetRow("FF_4") = xftvRow.Item("FF_4").Value
'								targetRow("FF_5") = xftvRow.Item("FF_5").Value
							
								
								
'								targetRow("Status") = xftvRow.Item("Status").Value
'								targetRow("Update_Date") = DateTime.Now
'								targetRow("Update_User") = si.UserName
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 6")
                             End If
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
'BRApi.ErrorLog.LogMessage(si,$"Hit Save" & dirtyColList.Count)
                            For Each colName As String In dirtyColList
'BRApi.ErrorLog.LogMessage(si,$"Hit Save Column Name: " & colName)
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
'BRApi.ErrorLog.LogMessage(si,$"Hit Save Column - {targetRow(colName).ToString()}")
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
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#Region "Save cWork OOC"
  Private Function Save_cWork_OOC() As Object
                Dim xftv As New TableView
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
				
			
				Dim REQID As String  = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList","NA")
				If String.IsNullOrWhiteSpace(REQID)
					Return Nothing
						
				End If
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
'BRApi.ErrorLog.LogMessage(si,$"In cWork TV")
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'BRApi.ErrorLog.LogMessage(si,$"Hit Save {args.CustSubstVarsAlreadyResolved.XFGetValue("BL_REQPRO_FundCenter","")}")

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 2")
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											AND REQ_ID = @REQ_ID"
'BRApi.ErrorLog.LogMessage(si,$"Hit Save 3")
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
							New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = REQID}
						}
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql, sqlparams)
						
            			
						For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
							Dim req_ID_Col = xftvRow.Item("CMD_PGM_REQ_ID")
                            Dim targetRow As DataRow
                            Dim isInsert As Boolean = False
						 	
                                
								targetRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("REQ_ID") = xftvRow.Item("REQ_ID").Value
								
								targetRow("Title") = xftvRow.Item("Title").Value
								targetRow("JUON") = xftvRow.Item("JUON").Value
								targetRow("ISR_Flag") = xftvRow.Item("ISR_Flag").Value
								
								targetRow("Cost_Model") = xftvRow.Item("Cost_Model").Value
								targetRow("Combat_Loss") = xftvRow.Item("Combat_Loss").Value
								
								targetRow("Cost_Location") = xftvRow.Item("Cost_Location").Value
								
								targetRow("Cat_A_Code") = xftvRow.Item("Cat_A_Code").Value
								targetRow("CBS_Code") = xftvRow.Item("CBS_Code").Value
								targetRow("MIP_Proj_Code") = xftvRow.Item("MIP_Proj_Code").Value
								
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next

		                    ' If this is an insert, add the new row to the DataTable
		                    If isInsert Then
		                    	dt.Rows.Add(targetRow)
								
		                    End If
		                Catch ex As Exception
                       ' BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
                    End Try
 'BRApi.ErrorLog.LogMessage(si, $"Error loop")
							Next
	
		                ' Persist changes back to the DB using the configured adapter
		          
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
						
		                End Using
		            End Using
			End If
            Return Nothing
        End Function	
#End Region
#Region "Save SS PEG"
 Public Function Save_SS_PEG() As Object
            Dim xftv As New TableView
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			End If
				
				Dim REQID As String  = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList","NA")
				If String.IsNullOrWhiteSpace(REQID)
					Return Nothing
						
				End If
	          ' Get Workflow context details 
    Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
    Dim scenarioName As String = wfInfoDetails("ScenarioName")
    Dim cmdName As String = wfInfoDetails("CMDName")
    Dim timeName As String = wfInfoDetails("TimeName")

    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = scenarioName},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = cmdName},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = timeName}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)


            ' ************************************
            ' ************************************
					
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
					Try
                    Dim req_ID_Val As Guid = xftvRow.Item("CMD_PGM_REQ_ID").Value.xfconverttoGUID
                    Dim isInsert As Boolean = False

                    ' --- Find the DataRows ---
                    Dim targetRow As DataRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
								targetRow("SS_Priority") = xftvRow.Item("SS_Priority").Value
								targetRow("Commit_Group") = xftvRow.Item("Commit_Group").Value
								targetRow("SS_Cap") = xftvRow.Item("SS_Cap").Value
								targetRow("Strategic_BIN") = xftvRow.Item("Strategic_BIN").Value
								targetRow("LIN") = xftvRow.Item("LIN").Value
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Title") = xftvRow.Item("Title").Value
								
                        Dim targetRowQuantity As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Quantity'").FirstOrDefault()
								If targetRowQuantity Is Nothing Then
									
  								  isInsert = True
							targetRowQuantity = dt_Details.NewRow()
				  			targetRowQuantity("CMD_PGM_REQ_ID") = req_ID_Val
							targetRowQuantity("WFScenario_Name") = wfInfoDetails("ScenarioName")
							targetRowQuantity("WFCMD_Name") = wfInfoDetails("CMDName")
							targetRowQuantity("WFTime_Name") = wfInfoDetails("TimeName")
							targetRowQuantity("Entity") = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_FundsCenter","")
							targetRowQuantity("Unit_of_Measure") = "Quantity"
							targetRowQuantity("IC") = "None"
							targetRowQuantity("Account") = "Quantity"
							targetRowQuantity("Flow") = xftvRow.Item("Status").Value
						
							targetRowQuantity("UD1") = xftvRow.Item("APPN").Value
					
							targetRowQuantity("UD2") = xftvRow.Item("MDEP").Value
							targetRowQuantity("UD3") = xftvRow.Item("APE9").Value
							targetRowQuantity("UD4") = xftvRow.Item("Dollar_Type").Value
							targetRowQuantity("UD5") = "None"
							targetRowQuantity("UD6") = xftvRow.Item("Obj_Class").Value
							targetRowQuantity("UD7") = "None"
							targetRowQuantity("UD8") = "None"
							targetRowQuantity("Start_Year") = wfInfoDetails("TimeName")
                            Dim fy1 As Decimal = xftvRow.Item("FY_1").Value.XFConvertToDecimal
                            Dim fy2 As Decimal = xftvRow.Item("FY_2").Value.XFConvertToDecimal
                            Dim fy3 As Decimal = xftvRow.Item("FY_3").Value.XFConvertToDecimal
                            Dim fy4 As Decimal = xftvRow.Item("FY_4").Value.XFConvertToDecimal
                            Dim fy5 As Decimal = xftvRow.Item("FY_5").Value.XFConvertToDecimal
	
                            targetRowQuantity("FY_1") = fy1
                            targetRowQuantity("FY_2") = fy2
                            targetRowQuantity("FY_3") = fy3
                            targetRowQuantity("FY_4") = fy4
                            targetRowQuantity("FY_5") = fy5
							'Brapi.ErrorLog.LogMessage(si,"In Insert 2")
							targetRowQuantity("Update_Date") = DateTime.Now
							targetRowQuantity("Update_User") = si.UserName
							targetRowQuantity("Create_Date") = DateTime.Now
							targetRowQuantity("Create_User") = si.UserName
							dt_Details.Rows.Add(targetRowQuantity)
							Else 
								targetRowQuantity("Unit_of_Measure") = targetRowQuantity("Unit_of_Measure")
								targetRowQuantity("FY_1") = xftvRow.Item("FY_1").Value.XFConvertToDecimal
								targetRowQuantity("FY_2") = xftvRow.Item("FY_2").Value.XFConvertToDecimal
								targetRowQuantity("FY_3") = xftvRow.Item("FY_3").Value.XFConvertToDecimal
								targetRowQuantity("FY_4") = xftvRow.Item("FY_4").Value.XFConvertToDecimal
								targetRowQuantity("FY_5") = xftvRow.Item("FY_5").Value.XFConvertToDecimal
								targetRowQuantity("Update_Date") = DateTime.Now
								targetRowQuantity("Update_User") = si.UserName
						End If 		
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))  
								targetRowQuantity(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next
							
							  
							
							
							   Catch ex As Exception
                        'BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
                    End Try
		                Next
	
		                 ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
						
						
		                End Using
		            End Using
		
            Return Nothing
        End Function	
#End Region
#Region "Save DD PEG"
Public Function Save_DD_PEG() As Object
            Dim xftv As New TableView
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			End If
				Dim REQID As String  = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_PGM_REQTitleList","NA")
				If String.IsNullOrWhiteSpace(REQID)
					Return Nothing	
				End If
	
	          ' Get Workflow context details
    Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
    Dim scenarioName As String = wfInfoDetails("ScenarioName")
    Dim cmdName As String = wfInfoDetails("CMDName")
    Dim timeName As String = wfInfoDetails("TimeName")

    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = scenarioName},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = cmdName},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = timeName}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)


            ' ************************************
            ' ************************************
            	For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
					Try
                    Dim req_ID_Val As Guid = xftvRow.Item("CMD_PGM_REQ_ID").Value.xfconverttoGUID
                  	Dim isInsert As Boolean = False
                    ' --- Find the DataRows ---
                    Dim targetRow As DataRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
						

								targetRow("REQ_Type")= xftvRow.Item("REQ_Type").Value	
								targetRow("DD_Priority") = xftvRow.Item("DD_Priority").Value	
								targetRow("Portfolio") = xftvRow.Item("Portfolio").Value
								targetRow("DD_Cap") = xftvRow.Item("DD_Cap").Value
								targetRow("JNT_Cap_Area") = xftvRow.Item("JNT_Cap_Area").Value
								targetRow("TBM_Cost_Pool") = xftvRow.Item("TBM_Cost_Pool").Value
								targetRow("TBM_Tower") = xftvRow.Item("TBM_Tower").Value
								targetRow("APMS_AITR_Num") = xftvRow.Item("APMS_AITR_Num").Value
								targetRow("Zero_Trust_Cap") = xftvRow.Item("Zero_Trust_Cap").Value
								targetRow("Assoc_Directives") = xftvRow.Item("Assoc_Directives").Value
								targetRow("Cloud_IND") = xftvRow.Item("Cloud_IND").Value
								targetRow("Strat_Cyber_Sec_PGM") = xftvRow.Item("Strat_Cyber_Sec_PGM").Value
								targetRow("Notes") = xftvRow.Item("Notes").Value
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								
								
				Dim unitOfMeasure As String = xftvRow.Item("Unit_of_Measure").Value
						
						
					
					
						Dim targetRowManpowerCost As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'DD_PEG_Cost'").FirstOrDefault()
						
						If targetRowManpowerCost Is Nothing Then
							 isInsert = True
								Dim newrowCost As datarow = dt_Details.NewRow()
								newrowCost("CMD_PGM_REQ_ID") = req_ID_Val
								newrowCost("WFScenario_Name") = scenarioName
								newrowCost("WFCMD_Name") = cmdName
								newrowCost("WFTime_Name") = timeName
								newrowCost("Unit_of_Measure") = unitOfMeasure
								newrowCost("Entity") = targetRow("Entity")
								newrowCost("IC") = "None"
								newrowCost("Account") = "DD_PEG_Cost"
								newrowCost("Flow") = targetRow("Status")
								newrowCost("UD1") = targetRow("APPN")
								newrowCost("UD2") = targetRow("MDEP")
								newrowCost("UD3") = targetRow("APE9")
								newrowCost("UD4") = targetRow("Dollar_Type")
								If targetRow.IsNull("CType") Then
								newrowCost("UD5") = "None"
								Else
								newrowCost("UD5") = targetRow("CType")
								End If
								newrowCost("UD6") = targetRow("Obj_Class")
								newrowCost("UD7") = "None"
								newrowCost("UD8") = "None"
								newrowCost("Start_Year") = timeName
		                     newrowCost("FY_1") = xftvRow.Item("FY_1_Cost").Value.XFConvertToDecimal
		                     newrowCost("FY_2") = xftvRow.Item("FY_2_Cost").Value.XFConvertToDecimal
		                     newrowCost("FY_3") = xftvRow.Item("FY_3_Cost").Value.XFConvertToDecimal
		                     newrowCost("FY_4") = xftvRow.Item("FY_4_Cost").Value.XFConvertToDecimal
		                     newrowCost("FY_5") = xftvRow.Item("FY_5_Cost").Value.XFConvertToDecimal
							 newrowCost("FY_Total") = "0"
							 newrowCost("Create_Date") = targetRow("Create_Date")
		                     newrowCost("Create_User") = targetRow("Create_User")
		                     newrowCost("Update_Date") = DateTime.Now
		                     newrowCost("Update_User") = si.UserName
							 
							 dt_Details.rows.add(newrowCost)
							 
							 Else 
								 
							
		                     targetRowManpowerCost("Unit_of_Measure") = unitOfMeasure
		                     targetRowManpowerCost("WFScenario_Name") = scenarioName
		                     targetRowManpowerCost("WFCMD_Name") = cmdName
		                     targetRowManpowerCost("WFTime_Name") = timeName
		                     targetRowManpowerCost("FY_1") = xftvRow.Item("FY_1_Cost").Value.XFConvertToDecimal
		                     targetRowManpowerCost("FY_2") = xftvRow.Item("FY_2_Cost").Value.XFConvertToDecimal
		                     targetRowManpowerCost("FY_3") = xftvRow.Item("FY_3_Cost").Value.XFConvertToDecimal
		                     targetRowManpowerCost("FY_4") = xftvRow.Item("FY_4_Cost").Value.XFConvertToDecimal
		                     targetRowManpowerCost("FY_5") = xftvRow.Item("FY_5_Cost").Value.XFConvertToDecimal
		                     targetRowManpowerCost("Update_Date") = DateTime.Now
		                     targetRowManpowerCost("Update_User") = si.UserName
							 
							 End If
								 
								
								
						
						Dim targetRowManpower As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'DD_PEG_Item'").FirstOrDefault()
						If targetRowManpower Is Nothing  Then
							 isInsert = True
							
						Dim newrowItem As datarow = dt_Details.NewRow()
								newrowItem("CMD_PGM_REQ_ID") = req_ID_Val
								 newrowItem("WFScenario_Name") = scenarioName
								newrowItem("WFCMD_Name") = cmdName
								newrowItem("WFTime_Name") = timeName
								newrowItem("Unit_of_Measure") = unitOfMeasure
								newrowItem("Entity") = targetRow("Entity")
								newrowItem("IC") = "None"
								newrowItem("Account") = "DD_PEG_Item"
								newrowItem("Flow") = targetRow("Status")
								newrowItem("UD1") = targetRow("APPN")
								newrowItem("UD2") = targetRow("MDEP")
								newrowItem("UD3") = targetRow("APE9")
								newrowItem("UD4") = targetRow("Dollar_Type")
								If targetRow.IsNull("CType")  Then
									newrowItem("UD5") = "None"
								Else
								newrowItem("UD5") = targetRow("CType")
								End If
								newrowItem("UD6") = targetRow("Obj_Class")
								newrowItem("UD7") = "None"
								newrowItem("UD8") = "None"
								newrowItem("Start_Year") = timeName
		                     newrowItem("FY_1") = xftvRow.Item("FY_1").Value.XFConvertToDecimal
		                     newrowItem("FY_2") = xftvRow.Item("FY_2").Value.XFConvertToDecimal
		                     newrowItem("FY_3") = xftvRow.Item("FY_3").Value.XFConvertToDecimal
		                     newrowItem("FY_4") = xftvRow.Item("FY_4").Value.XFConvertToDecimal
		                     newrowItem("FY_5") = xftvRow.Item("FY_5").Value.XFConvertToDecimal
							 newrowItem("FY_Total") = "0"
							 newrowItem("Create_Date") = targetRow("Create_Date")
		                     newrowItem("Create_User") = targetRow("Create_User")
		                     newrowItem("Update_Date") = DateTime.Now
		                     newrowItem("Update_User") = si.UserName
							 
							 dt_Details.rows.add(newrowItem)
							 
							 Else
								 
							
							targetRowManpower("Unit_of_Measure") = unitOfMeasure
	                        targetRowManpower("WFScenario_Name") = scenarioName
	                        targetRowManpower("WFCMD_Name") = cmdName
	                        targetRowManpower("WFTime_Name") = timeName
	                        targetRowManpower("FY_1") = xftvRow.Item("FY_1").Value.XFConvertToDecimal
	                        targetRowManpower("FY_2") = xftvRow.Item("FY_2").Value.XFConvertToDecimal
	                        targetRowManpower("FY_3") = xftvRow.Item("FY_3").Value.XFConvertToDecimal
	                        targetRowManpower("FY_4") = xftvRow.Item("FY_4").Value.XFConvertToDecimal
	                        targetRowManpower("FY_5") = xftvRow.Item("FY_5").Value.XFConvertToDecimal
	                        targetRowManpower("Update_Date") = DateTime.Now
	                        targetRowManpower("Update_User") = si.UserName    
								    
							End If
							
							
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
							If isInsert = False
		                   Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								targetRowManpowerCost(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))
								targetRowManpower(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next
						End If 
					 Catch ex As Exception
                        'BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
                    End Try
		                Next
	
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
						'Brapi.ErrorLog.LogMessage(si, "HERE")
		                End Using
		            End Using

            Return Nothing
        End Function	
#End Region

#End Region

    End Class
End Namespace