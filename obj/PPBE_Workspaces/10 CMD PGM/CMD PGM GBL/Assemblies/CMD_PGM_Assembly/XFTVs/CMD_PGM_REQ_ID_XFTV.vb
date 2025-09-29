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
				BRApi.ErrorLog.LogMessage(si,$"Hit {args.TableViewName}")
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
				
							
						If args.TableViewName = "REQ_Base_Info"
                      		Return Get_REQ_Base_Info()
						
						Else If args.TableViewName = "cWork_OOC"
							Return Get_REQ_cWork_OOC()
							
						Else If args.TableViewName = "SS_PEG"
							Return Get_REQ_SS_PEG()
							
						Else If args.TableViewName = "DD_PEG"
							Return Get_REQ_DD_PEG()
							
						Else If args.TableViewName = "Create_New_Req"
							
							Return Get_Create_New_Req()
							
						Else If args.TableViewName = "REQ_Funding_Line"
							Return Get_REQ_Funding_Line()
							
						Else If args.TableViewName = "CMD_PGM_Prioritize"
							Return Get_CMD_PGM_Prioritize()
							
						End If
                    Case SpreadsheetFunctionType.SaveTableView
						Dim uState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName)
						
						If uState IsNot Nothing Then
						    Dim jsonString As String = System.Text.Encoding.UTF8.GetString(uState.BinaryValue)
						    args.CustSubstVarsAlreadyResolved = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonString)
						End If
						If args.TableViewName = "REQ_Base_Info"
                       	 	Return Save_REQ_Base_Info()
							
						Else If args.TableViewName = "Create_New_Req"
							Brapi.ErrorLog.LogMessage(si,"In Save")
							Return Save_Create_New_Req()
							
						Else If args.TableViewName = "cWork_OOC"
							Return Save_cWork_OOC()
							
						Else If args.TableViewName = "SS_PEG"
							Return Save_SS_PEG()
							
						Else If args.TableViewName = "DD_PEG"
							Return Save_DD_PEG()
							
						Else If args.TableViewName = "REQ_Funding_Line"
							Return Save_REQ_Funding_Line()
							
						Else If args.TableViewName = "CMD_PGM_Prioritize"
							Return Save_CMD_PGM_Prioritize()
							
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
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
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
            Dim sql As String = $"SELECT CMD_PGM_REQ_ID, 
										 WFScenario_Name, 
										 WFCMD_Name, WFTime_Name, REQ_ID, Title, Description, Justification, Entity, APPN, MDEP, APE9, Dollar_Type, 
									     Obj_Class, CType, UIC, Cost_Methodology, Impact_Not_Funded, Risk_Not_Funded, Cost_Growth_Justification, Must_Fund, 
										 Funding_Src, Army_Init_Dir, CMD_Init_Dir, Activity_Exercise, Directorate, Div, Branch, IT_Cyber_REQ, Emerging_REQ, 
										 CPA_Topic, PBR_Submission, UPL_Submission, Contract_Num, Task_Order_Num, Award_Target_Date, POP_Exp_Date, 
									     Contractor_ManYear_Equiv_CME, COR_Email, POC_Email, Review_POC_Email, MDEP_Functional_Email, Notification_List_Emails, 
									     Gen_Comments_Notes, FF_1, FF_2, FF_3, FF_4, FF_5, 
			                             Attach_File_Name, Attach_File_Bytes, Status,Create_Date,Create_User,Update_Date,Update_User
                				FROM XFC_CMD_PGM_REQ
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
#Region "Get cWork OOC"
        Private Function Get_REQ_cWork_OOC() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT CMD_PGM_REQ_ID, 
										 WFScenario_Name, 
										 WFCMD_Name, WFTime_Name, REQ_ID, Title, JUON, ISR_Flag, Cost_Model, Combat_Loss, Cost_Location, Cat_A_Code, CBS_Code, MIP_Proj_Code  
			                             
                				FROM XFC_CMD_PGM_REQ
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								And REQ_ID = '{RequirementID}'
								And Entity  = '{Entity}'"
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
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
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
								Dtl.FY_5
                				FROM XFC_CMD_PGM_REQ Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{RequirementID}'
								And Req.Entity  = '{Entity}'
								AND Dtl.Unit_of_Measure ='Funding' "
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
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID, 
										 Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name, Req.REQ_ID, Req.Title, Req.REQ_Type, Req.DD_Priority, Req.Portfolio, Req.DD_Cap, Req.JNT_Cap_Area, Req.TBM_Cost_Pool, Req.TBM_Tower, 
			                             Req.APMS_AITR_Num, Req.Zero_Trust_Cap, Req.Assoc_Directives, Req.Cloud_IND, Req.Strat_Cyber_Sec_PGM,Req.Notes,Dtl.Unit_of_Measure,
										
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower' Then Dtl.FY_1
									Else Null
								End) As FY_1,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower_Cost' Then Dtl.FY_1
									Else Null
								End) As FY_1_Cost,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower' Then Dtl.FY_2
									Else Null
								End) As FY_2,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower_Cost' Then Dtl.FY_2
									Else Null
								End) As FY_2_Cost,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower'  Then Dtl.FY_3
									Else Null
								End) As FY_3,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower_Cost' Then Dtl.FY_3
									Else Null
								End) As FY_3_Cost,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower' Then Dtl.FY_4
									Else Null
								End) As FY_4,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower_Cost' Then Dtl.FY_4
									Else Null
								End) As FY_4_Cost,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower' Then Dtl.FY_5
									Else Null
								End) As FY_5,
								MAX(CASE WHEN
									Dtl.Unit_of_Measure = 'Manpower' And Dtl.Account = 'Manpower_Cost' Then Dtl.FY_5
									Else Null
								End) As FY_5_Cost
                			FROM 
								XFC_CMD_PGM_REQ Req
							LEFT JOIN 
								XFC_CMD_PGM_REQ_Details AS Dtl
							ON 
								Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
							WHERE 
									Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{RequirementID}'
								And Req.Entity  = '{Entity}'
								AND 
       								 Dtl.Unit_of_Measure = 'Manpower'
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
							    Req.Notes,
								Dtl.Unit_of_Measure
							
        							
			"
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		End If
        End Function
	#End Region	
#Region "Get Create New REQ"
        Private Function Get_Create_New_Req() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID,
          Req.WFScenario_Name,
          Req.WFCMD_Name,
          Req.WFTime_Name,
          Req.REQ_ID,
          Req.Title,
          Req.Entity,
          Req.APPN,
          Req.MDEP,
          Req.APE9,
          Req.Dollar_Type,
          Req.Obj_Class,
          Req.CType,
          Req.UIC,
          Req.Invalid,
          Req.Val_Error,
          Dtl.Unit_of_Measure,
          Dtl.IC,
          Dtl.Account,
          Dtl.Flow,
          Dtl.UD1,
          Dtl.UD2,
          Dtl.UD3,
          Dtl.UD4,
          Dtl.UD5,
          Dtl.UD6,
          Dtl.UD7,
          Dtl.UD8,
          Dtl.Start_Year,
          Dtl.FY_1,
          Dtl.FY_2,
          Dtl.FY_3,
          Dtl.FY_4,
          Dtl.FY_5,
          Dtl.FY_Total,
          Dtl.AllowUpdate
        FROM XFC_CMD_PGM_REQ AS Req
        LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
ON 
    Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								"
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			'BRApi.ErrorLog.LogMessage(si,$"Hit Get Function")
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
        End Function
#End Region
#Region "Get REQ Funding Line"
 Private Function Get_REQ_Funding_Line() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
				Return Nothing
			Else
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID,
								  Req.WFScenario_Name,
								  Req.WFCMD_Name,
								  Req.WFTime_Name,
								  Req.Title,
								  Req.Entity,
								  Dtl.Unit_of_Measure,
								  Dtl.Account,
								  Dtl.Flow,
								  Dtl.UD1,
								  Dtl.UD2,
								  Dtl.UD3,
								  Dtl.UD4,
								  Dtl.UD5,
								  Dtl.UD6,
								  Dtl.UD7,
								  Dtl.UD8,
								  Dtl.Start_Year,
								  Dtl.FY_1,
								  Dtl.FY_2,
								  Dtl.FY_3,
								  Dtl.FY_4,
								  Dtl.FY_5,
								  Dtl.AllowUpdate
								FROM XFC_CMD_PGM_REQ AS Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{RequirementID}'
								And Req.Entity  = '{Entity}'
								AND  Dtl.Account = 'REQ_Requested_Amt'	"
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			'BRApi.ErrorLog.LogMessage(si,$"Hit Get Function")
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
		End If
        End Function
#End Region
#Region "Get CMD PGM Prioritize"
 Private Function Get_CMD_PGM_Prioritize() As TableView
	     Dim dt As New DataTable()
            Dim xftv As New TableView()
			xftv.CanModifyData = True
			
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			BRApi.ErrorLog.LogMessage(si,$"Hit Get Function 1")
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'			Dim sEntity As String =  args.CustSubstVarsAlreadyResolved.XFGetValue("BL_REQPRO_FundCenter","")
'			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
'			Dim sREQWFStatus As String = entityLevel & "_Prioritize_PGM"
            Dim sql As String =$"SELECT Req.CMD_PGM_REQ_ID, 
										 Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name, Req.Entity,Req.Title,Req.Cat_1_Score,
										Req.Cat_2_Score,Req.Cat_3_Score,Req.Cat_4_Score,Req.Cat_5_Score,Req.Cat_6_Score,Req.Cat_7_Score,
										Req.Cat_8_Score,Req.Cat_9_Score,Req.Cat_10_Score,Req.Cat_11_Score,Req.Cat_12_Score,Req.Cat_13_Score,			
										Req.Cat_14_Score,Req.Cat_15_Score,Req.Score,Req.Weighted_Score,Req.Auto_Rank,Req.Rank_Override,
									Dtl.UD1,
									Dtl.UD2,
									Dtl.UD3,
									Dtl.UD4,
									Dtl.FY_1,
								  Dtl.FY_2,
								  Dtl.FY_3,
								  Dtl.FY_4,
								  Dtl.FY_5
								FROM XFC_CMD_PGM_REQ_Priority AS Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								
								AND Dtl.Account = 'REQ_Requested_Amt'"
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			BRApi.ErrorLog.LogMessage(si,$"Hit Get Function 2")
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
	
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
                        Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_PGM_REQ
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						'BRApi.ErrorLog.LogMessage(si,$"Hit Save 3")
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql, sqlparams)
						'BRApi.ErrorLog.LogMessage(si,$"Hit Save 4")
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
							
		                    Dim req_ID_Col = xftvRow.Item("CMD_PGM_REQ_ID")
                            Dim targetRow As DataRow
                            Dim isInsert As Boolean = False
							'BRApi.ErrorLog.LogMessage(si,$"Hit Save 5")
   							Dim cellValObj = req_ID_Col.OriginalValue
                                targetRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
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
								targetRow("Award_Target_Date") = xftvRow.Item("Award_Target_Date").Value
								targetRow("POP_Exp_Date") = xftvRow.Item("POP_Exp_Date").Value
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
                        BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
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
											AND WFTime_Name = @WFTime_Name"
						'BRApi.ErrorLog.LogMessage(si,$"Hit Save 3")
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, Sql, sqlparams)
						BRApi.ErrorLog.LogMessage(si,$"Hit Save 4")
            			
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
								targetRow("Cost_Location") = xftvRow.Item("Cost_location").Value
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
                        BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
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
#Region "Save SS PEG"
 Public Function Save_SS_PEG() As Object
            Dim xftv As New TableView
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
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
								
								
								targetRowQuantity("Unit_of_Measure") = xftvRow.Item("Unit_of_Measure").Value
								targetRowQuantity("FY_1") = xftvRow.Item("FY_1").Value.XFConvertToDecimal
								targetRowQuantity("FY_2") = xftvRow.Item("FY_2").Value.XFConvertToDecimal
								targetRowQuantity("FY_3") = xftvRow.Item("FY_3").Value.XFConvertToDecimal
								targetRowQuantity("FY_4") = xftvRow.Item("FY_4").Value.XFConvertToDecimal
								targetRowQuantity("FY_5") = xftvRow.Item("FY_5").Value.XFConvertToDecimal
								targetRowQuantity("Update_Date") = DateTime.Now
								targetRowQuantity("Update_User") = si.UserName
							
								
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))  
								targetRowQuantity(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next
							   Catch ex As Exception
                        BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
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
							
								
					 			targetRow("Title") = xftvRow.Item("Title").Value
								targetRow("REQ_Type") = xftvRow.Item("REQ_Type").Value
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
						Dim targetRowManpowerCost As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Manpower_Cost'").FirstOrDefault()
								
							 targetRowManpowerCost("Unit_of_Measure") = unitOfMeasure
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
								
						Dim targetRowManpower As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Manpower'").FirstOrDefault()
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
								    
							
							
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								targetRowManpowerCost(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))
								targetRowManpower(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next

					 Catch ex As Exception
                        BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
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
#Region"Save Create New Req"
   Public Function Save_Create_New_REQ() As Object
         Dim xftv As New TableView
			xftv = args.TableView
			Brapi.ErrorLog.LogMessage(si,"In Save Create")
'            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
'				Return Nothing
'			End If
	          ' Get Workflow context details
    Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
    Dim scenarioName As String = wfInfoDetails("ScenarioName")
    Dim cmdName As String = wfInfoDetails("CMDName")
    Dim timeName As String = wfInfoDetails("TimeName")

Brapi.ErrorLog.LogMessage(si,$"In Save Create - {args.CustSubstVarsAlreadyResolved.Count}")
For Each item As KeyValuePair(Of String, String) In args.CustSubstVarsAlreadyResolved
	Brapi.ErrorLog.LogMessage(si,$"Key: {item.Key}, Value: {item.Value}")
Next
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
            
						Dim sEntity As String =  args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_FundsCenter","")
						Dim entityLevel As String = Me.GetEntityLevel(sEntity)
						Dim sREQWFStatus As String = entityLevel & "_Formulate_PGM"
						Dim NewReqID As Guid = Guid.NewGuid()
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							
						 
                            Dim targetRowDetails As DataRow = dt_Details.NewRow()
                            Dim isInsert As Boolean = True
							
                                
							targetRowDetails("CMD_PGM_REQ_ID") = NewReqID
							targetRowDetails("WFScenario_Name") = wfInfoDetails("ScenarioName")
							targetRowDetails("WFCMD_Name") = wfInfoDetails("CMDName")
							targetRowDetails("WFTime_Name") = wfInfoDetails("TimeName")
							targetRowDetails("Entity") = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_FundsCenter","")
							targetRowDetails("Unit_of_Measure") = "Funding"
							targetRowDetails("IC") = "None"
							targetRowDetails("Account") = "REQ_Requested_Amt"
							targetRowDetails("Flow") = sREQWFStatus
							targetRowDetails("UD1") = xftvRow.Item("APPN").Value
							targetRowDetails("UD2") = xftvRow.Item("MDEP").Value
							targetRowDetails("UD3") = xftvRow.Item("APE9").Value
							targetRowDetails("UD4") = xftvRow.Item("Dollar_Type").Value
							targetRowDetails("UD5") = "None"
							targetRowDetails("UD6") = xftvRow.Item("Obj_Class").Value
							targetRowDetails("UD7") = "None"
							targetRowDetails("UD8") = "None"
							targetRowDetails("Start_Year") = wfInfoDetails("TimeName")
                            Dim fy1 As Decimal = xftvRow.Item("FY_1").Value.XFConvertToDecimal
                            Dim fy2 As Decimal = xftvRow.Item("FY_2").Value.XFConvertToDecimal
                            Dim fy3 As Decimal = xftvRow.Item("FY_3").Value.XFConvertToDecimal
                            Dim fy4 As Decimal = xftvRow.Item("FY_4").Value.XFConvertToDecimal
                            Dim fy5 As Decimal = xftvRow.Item("FY_5").Value.XFConvertToDecimal

                            targetRowDetails("FY_1") = fy1
                            targetRowDetails("FY_2") = fy2
                            targetRowDetails("FY_3") = fy3
                            targetRowDetails("FY_4") = fy4
                            targetRowDetails("FY_5") = fy5
                            targetRowDetails("FY_Total") = fy1 + fy2 + fy3 + fy4 + fy5
							targetRowDetails("AllowUpdate") = "True"
							targetRowDetails("Create_Date") = DateTime.Now
							targetRowDetails("Create_User") = si.UserName
							targetRowDetails("Update_Date") = DateTime.Now
							targetRowDetails("Update_User") = si.UserName
								
								Dim targetRow As DataRow = dt.NewRow()
                         
								targetRow("CMD_PGM_REQ_ID") = NewReqID
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Entity") = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_PGM_FundsCenter","")
								targetRow("REQ_ID") = Workspace.GBL.GBL_Assembly.GBL_REQ_ID_Helpers.Get_FC_REQ_ID(si,"A76_General")
								targetRow("Title") = xftvRow.Item("Title").Value
								targetRow("APPN") = xftvRow.Item("APPN").Value
								targetRow("MDEP") = xftvRow.Item("MDEP").Value
								targetRow("APE9") = xftvRow.Item("APE9").Value
								targetRow("Dollar_Type") = xftvRow.Item("Dollar_Type").Value
								targetRow("Obj_Class") = xftvRow.Item("Obj_Class").Value
								targetRow("Create_Date") = DateTime.Now
								targetRow("Create_User") = si.UserName
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
								targetRow("Status") = sREQWFStatus
							
								
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRowDetails(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next

		                    ' If this is an insert, add the new row to the DataTable
		                    If isInsert Then
		                    	dt_Details.Rows.Add(targetRowDetails)
								dt.Rows.Add(targetRow)
		                    End If
		                Next
	
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
						
						
		                End Using
		            End Using

            Return Nothing
        End Function	
#End Region	
#Region "Save REQ_Funding_Line"
Public Function Save_REQ_Funding_Line() As Object
            Dim xftv As New TableView
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
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
								targetRow("APPN") = xftvRow.Item("UD1").Value
								targetRow("MDEP") = xftvRow.Item("UD2").Value
								targetRow("APE9") = xftvRow.Item("UD3").Value
								targetRow("Dollar_Type") = xftvRow.Item("UD4").Value
								targetRow("Obj_Class") = xftvRow.Item("UD6").Value
								targetRow("CType") = xftvRow.Item("UD5").Value
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
						
						Dim targetRowFunding As DataRow = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'REQ_Requested_Amt'").FirstOrDefault()
						 	
							targetRowFunding("UD1") = xftvRow.Item("UD1").Value
							targetRowFunding("UD2") = xftvRow.Item("UD2").Value
							targetRowFunding("UD3") = xftvRow.Item("UD3").Value
							targetRowFunding("UD4") = xftvRow.Item("UD4").Value
							targetRowFunding("UD6") = xftvRow.Item("UD6").Value
							targetRowFunding("UD5") = xftvRow.Item("UD5").Value
							targetRowFunding("WFScenario_Name") = scenarioName
	                        targetRowFunding("WFCMD_Name") = cmdName
	                        targetRowFunding("WFTime_Name") = timeName
	                        targetRowFunding("FY_1") = xftvRow.Item("FY_1").Value.XFConvertToDecimal
	                        targetRowFunding("FY_2") = xftvRow.Item("FY_2").Value.XFConvertToDecimal
	                        targetRowFunding("FY_3") = xftvRow.Item("FY_3").Value.XFConvertToDecimal
	                        targetRowFunding("FY_4") = xftvRow.Item("FY_4").Value.XFConvertToDecimal
	                        targetRowFunding("FY_5") = xftvRow.Item("FY_5").Value.XFConvertToDecimal
	                        targetRowFunding("Update_Date") = DateTime.Now
	                        targetRowFunding("Update_User") = si.UserName  
							
							
							
							
								
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)

                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								
								targetRowFunding(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
                            Next

					 Catch ex As Exception
                        BRApi.ErrorLog.LogMessage(si, $"Error processing row: {ex.Message} - {ex.StackTrace}")
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
#Region "Save CMD PGM Prioritize"
 Private Function Save_CMD_PGM_Prioritize() As TableView
	 
 End Function
#End Region
#End Region

#Region "Get Entity Level"	

	Public Function GetEntityLevel(sEntity As String) As String
		Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
		Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
		Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)

		Dim level As String  = String.Empty
		Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
		If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
			level = entityText3.Substring(entityText3.Length -2, 2)
		End If
		
		Return level
		
	End Function
#End Region

    End Class
End Namespace