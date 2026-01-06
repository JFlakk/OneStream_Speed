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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.CMD_UFR_REQ_ID_XFTV
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
							
						If args.TableViewName = "Create_New_UFR"
                      		Return Get_Create_New_UFR()							
						End If
						
						If args.TableViewName = "UFR_Base_Info"
                      		Return Get_UFR_Base_Info()							
						End If
						
						If args.TableViewName.XFContainsIgnoreCase("Staffing_Narrative")
'							BRapi.ErrorLog.LogMessage(si, "TableViewName = " & args.TableViewName)
							Return Get_Staffing_Narrative()
						End If
						


                    Case SpreadsheetFunctionType.SaveTableView
						Dim uState = BRApi.State.GetSessionState(si, False, ClientModuleType.Unknown, GetType(Dictionary(Of String, String)).Name, String.Empty, "SubstVars", si.UserName)
						If uState IsNot Nothing Then
						    Dim jsonString As String = System.Text.Encoding.UTF8.GetString(uState.BinaryValue)
						    args.CustSubstVarsAlreadyResolved = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(jsonString)
						End If
						
						If args.TableViewName = "Create_New_UFR"
                       	 	Return Save_Create_New_UFR()	
						End If
						
						If args.TableViewName = "UFR_Base_Info"
                       	 	Return Save_UFR_Base_Info()	
						End If

						If args.TableViewName.XFContainsIgnoreCase("Staffing_Narrative")
'							BRapi.ErrorLog.LogMessage(si, "TableViewName = " & args.TableViewName)
							Return Save_Staffing_Narrative()
						End If

#Region "Delete"
'						If args.TableViewName = "Staffing_Narrative_G8"
'                       	 	Return Save_Staffing_Narrative_G8()	
'						End If

'						If args.TableViewName = "Staffing_Narrative_JAG"
'                       	 	Return Save_Staffing_Narrative_JAG()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_ABO"
'                       	 	Return Save_Staffing_Narrative_ABO()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_HQ_G8"
'                       	 	Return Save_Staffing_Narrative_HQ_G8()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_G_3_5_7"
'                       	 	Return Save_Staffing_Narrative_G_3_5_7()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_G8_PAE"
'                       	 	Return Save_Staffing_Narrative_G8_PAE()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_Integrator"
'                       	 	Return Save_Staffing_Narrative_Integrator()	
'						End If
						
'						If args.TableViewName = "Staffing_Narrative_PEG"
'                       	 	Return Save_Staffing_Narrative_PEG()	
'						End If

'						If args.TableViewName = "Staffing_Narrative_BRP"
'                       	 	Return Save_Staffing_Narrative_BRP()	
'						End If
#End Region						
                    Case Else
                        Return Nothing
					End Select
   			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
' --- XFTV Helper Functions ---	
#Region "XFTV Helpers"	
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


#End Region
		
' --- Call Functions ---
	
#Region "Create_New_UFR"		
#Region "Function: Get_Create_New_UFR - Create New UFR"
        Private Function Get_Create_New_UFR() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
		
            Dim sql As String = $"SELECT
								    Req.[Army_Campaign_Objectives],
								    Req.[Initial_Review_Type],
								    Req.[CMD_UFR_Tracking_No], Req.[Command_UFR_Priority], Req.[Command_UFR_Status],
								    Req.[Create_Date], Req.[Create_User], Req.[Date_Decision_Needed_By], Req.[Description],
								    Req.[Fund_Type], Req.[Funds_Required_By],
								    Req.[MustFund], Req.[PEG],
									Req.[Study_Category],
								    Req.[RDA_UFR_Executable_Fund_Year], Req.[RDA_UFR_FY_New_Start], Req.[Requirement_Background],
								    Req.[Review_Entity], Req.[Review_Staff], Req.[REQ_Link_ID], 
									Req.[ROC], Req.[Rollover_To_Next_UFR_Cycle], Req.[SAG_SSN], Req.[Solicited_NonSolicited],
								    Req.[UFR_Capability_GAP], Req.[UFR_Capability_GAP_If_CMD_Fund], Req.[UFR_Driver],
								    Req.[UFR_Driver_Explanation], Req.[UFR_ID], Req.[UFR_ID_Type],
								    Req.[Update_Date], Req.[Update_User],
								    Req.[WFCMD_Name], Req.[WFScenario_Name], Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								AND 1 <> 1
								
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Create_New_UFR - Save Create New UFR"
		Public Function Save_Create_New_UFR() As Object
			
			Dim xftv As TableView = args.TableView
			Dim sEntity As String = args.CustSubstVarsAlreadyResolved("BL_CMD_UFR_FundsCenter")
			Dim sU1APPNInput As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateAPPN")
			Dim sU2Input As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateMDEP")
			Dim sU3Input As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateAPEPT")
			Dim sU4Input As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateDollarType")
			Dim sU5Input As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateCType")
			Dim sU6Input As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateObjectClass")
			Dim sSAGInput As String = args.CustSubstVarsAlreadyResolved("ML_CMD_UFR_FormulateSAG")
			Dim UFR_ID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "FormulateUFR", "UFR_ID", String.Empty)
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim U3DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U3_SAG")
			Dim U3List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3DimPk, "U3#" & sSAGInput & ".Ancestors.Where(MemberDim = U3_BA)", True)
			Dim sBAInput As String = U3List(0).Member.Name
			'Clean the values
			Dim sSAG As String = sSAGInput.Split("_"c)(1)
			Dim sBA As String = sBAInput.Split("_"c)(1)
			Dim sTime As String = sWFYear.Substring(2)
			
			Brapi.ErrorLog.LogMessage(si, $"XFTV - SettingNameString = {UFR_ID} - {sEntity} - {sU1APPNInput} - {sU2Input} - {sU3Input} - {sU4Input} - {sU5Input} - {sU6Input}")
			
#Region "Global Checks/Functions"
			'Clear the previous Guid
'			GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
			
			'Reset the UFR Check Flag
'			GBL.GBL_Assembly.GBL_Helpers.ResetUFRState(Me.si)
		
'			Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
			Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuidCopy(Me.si, UFR_ID, sEntity, sU1APPNInput, sU2Input, sU3Input, sU4Input, sU5Input, sU6Input, sSAGInput)
			
			Brapi.ErrorLog.LogMessage(si, " XFTV NewReqID = " & NewReqID)
			
			'Declare required Columns to validate they were filled out
'			Dim requiredColumns As String() = {"ROC", "Description"}
'			Brapi.ErrorLog.LogMessage(si, "RequiredColumns = " & requiredColumns.ToString)
			
			'Call globals to validate required Columns
'			Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.ValidateXFTVRequiredFields(Me.si, xftv, requiredColumns)
			
			'If Valid - mark as valid
'			Workspace.GBL.GBL_Assembly.GBL_Helpers.SetUFRValid(Me.si, True)
			
#End Region			
						
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)	
				
								
                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
#Region "Debugging"
'								Dim rowIndex As Integer
'								If rowIndex = 0 Then
'									BRapi.ErrorLog.LogMessage(Me.si, "---- ROW" & rowIndex.ToString() & "----")
'									For Each key As Object In xftvRow.Items.Keys
'										Brapi.ErrorLog.LogMessage(Me.si, "XFTV Column Key:  " & key.ToString)
'										Dim val As Object = xftvRow.Item(key).Value
'										Dim printable As String = If(val Is Nothing, "<Null", val.ToString())
'										Brapi.ErrorLog.LogMessage(Me.si, key.ToString() & " = " & printable)
'									Next
'								End If
#End Region
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
'								Try
			'								' --- Fill Narrative information ---
									targetRow = dt.Select($"CMD_UFR_Tracking_No = '{NewReqID.xfconverttoGUID}'").FirstOrDefault()
									targetRow("Description") = xftvRow.Item("Description").Value
									targetRow("Requirement_Background") = xftvRow.Item("Requirement_Background").Value
									targetRow("ROC") = xftvRow.Item("ROC").Value
									targetRow("MustFund") = xftvRow.Item("MustFund").Value 
									targetRow("Review_Staff") = xftvRow.Item("Review_Staff").Value 
									targetRow("Fund_Type") = xftvRow.Item("Fund_Type").Value 
									targetRow("Study_Category") = xftvRow.Item("Study_Category").Value 
									If sU1APPNInput.XFContainsIgnoreCase("OMA") Then
										targetRow("SAG_SSN") = sSAG
										targetRow("UFR_ID_Type") = "OMA"
									Else If sU1APPNInput.XFContainsIgnoreCase("RDTE") Or sU1APPNInput.XFContainsIgnoreCase("OPA") Then
										targetRow("SAG_SSN") = sCube & "_" & xftvRow.Item("Study_Category").Value & "_" & sBA & "_" & sTime
										targetRow("UFR_ID_Type") = "RDA"
									End If
									targetRow("UFR_Driver") = xftvRow.Item("UFR_Driver").Value 
									targetRow("UFR_Driver_Explanation") = xftvRow.Item("UFR_Driver_Explanation").Value 
									targetRow("UFR_Capability_GAP") = xftvRow.Item("UFR_Capability_GAP").Value 
									targetRow("UFR_Capability_GAP_If_CMD_Fund") = xftvRow.Item("UFR_Capability_GAP_If_CMD_Fund").Value 
									targetRow("Solicited_NonSolicited") = xftvRow.Item("Solicited_NonSolicited").Value 
									targetRow("Date_Decision_Needed_By") = xftvRow.Item("Date_Decision_Needed_By").Value 
									targetRow("Funds_Required_By") = xftvRow.Item("Funds_Required_By").Value 
									targetRow("Rollover_To_Next_UFR_Cycle") = xftvRow.Item("Rollover_To_Next_UFR_Cycle").Value 
									targetRow("Army_Campaign_Objectives") = xftvRow.Item("Army_Campaign_Objectives").Value
									targetRow("Initial_Review_Type") = xftvRow.Item("Initial_Review_Type").Value

		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName)) 
							Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
'						Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
                    End Try
		                Next
'				Throw New XFException("TEST: REQ_ID_XFTV Update table was reached")
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
						
		                End Using
		            End Using
				
					BRapi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "FormulateUFR", "UFR_ID", "")
				
            Return Nothing
        End Function
#End Region 
#End Region		

#Region "UFR_Base_Info"
#Region "Function: Get_UFR_Base_Info - Get UFR Base Info"
        Private Function Get_UFR_Base_Info() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
'			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("IV_CMD_UFR_REQ_IDs")
			Dim ReqID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
'			Brapi.ErrorLog.LogMessage(si, "ReqID = " & ReqID)
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

            Dim sql As String = $"SELECT
								    Req.[Army_Campaign_Objectives],
									Req.[Title],
								    Req.[Initial_Review_Type],
								    Req.[CMD_UFR_Tracking_No], Req.[Command_UFR_Priority], Req.[Command_UFR_Status],
								    Req.[Create_Date], Req.[Create_User], Req.[Date_Decision_Needed_By], Req.[Description],
								    Req.[Fund_Type], Req.[Funds_Required_By],
								    Req.[MustFund], Req.[PEG], Req.[Study_Category],
								    Req.[RDA_UFR_Executable_Fund_Year], Req.[RDA_UFR_FY_New_Start], Req.[Requirement_Background],
								    Req.[Review_Entity], Req.[Review_Staff], Req.[REQ_Link_ID], 
									Req.[ROC], Req.[Rollover_To_Next_UFR_Cycle], Req.[SAG_SSN], Req.[Solicited_NonSolicited],
								    Req.[UFR_Capability_GAP], Req.[UFR_Capability_GAP_If_CMD_Fund], Req.[UFR_Driver],
								    Req.[UFR_Driver_Explanation], Req.[UFR_ID], Req.[UFR_ID_Type],
								    Req.[Update_Date], Req.[Update_User],
								    Req.[WFCMD_Name], Req.[WFScenario_Name], Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								AND Req.UFR_ID = '{ReqID}'
								--AND 1 <> 1
								
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region

#Region "Function: Save_UFR_Base_Info - Save Create New UFR"
		Public Function Save_UFR_Base_Info() As Object
			
			Dim xftv As TableView = args.TableView
'			Dim sSAGInput As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "FormulateUFR", "SAG", String.Empty)
'			Dim sAPPNInput As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "FormulateUFR", "APPN", String.Empty)
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			
#Region "Global Checks/Functions"
'			'Clear the previous Guid
''			GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
			
'			'Reset the UFR Check Flag
'			GBL.GBL_Assembly.GBL_Helpers.ResetUFRState(Me.si)
		
'			Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
'			Brapi.ErrorLog.LogMessage(si, " XFTV NewReqID = " & NewReqID)
			
'			'Declare required Columns to validate they were filled out
'			Dim requiredColumns As String() = {"ROC", "Description"}
''			Brapi.ErrorLog.LogMessage(si, "RequiredColumns = " & requiredColumns.ToString)
			
'			'Call globals to validate required Columns
'			Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.ValidateXFTVRequiredFields(Me.si, xftv, requiredColumns)
			
'			'If Valid - mark as valid
'			Workspace.GBL.GBL_Assembly.GBL_Helpers.SetUFRValid(Me.si, True)
			
#End Region			
						
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)	
				
								
                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								Dim ufr_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
'								Try
			'								' --- Fill Narrative information ---
									targetRow = dt.Select($"CMD_UFR_Tracking_No = '{ufr_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
									targetRow("Description") = xftvRow.Item("Description").Value
									targetRow("Requirement_Background") = xftvRow.Item("Requirement_Background").Value
									targetRow("REQ_Link_ID") = xftvRow.Item("REQ_Link_ID").Value
									targetRow("ROC") = xftvRow.Item("ROC").Value
									targetRow("MustFund") = xftvRow.Item("MustFund").Value 
									targetRow("Review_Staff") = xftvRow.Item("Review_Staff").Value 
									targetRow("Fund_Type") = xftvRow.Item("Fund_Type").Value 
									targetRow("Study_Category") = xftvRow.Item("Study_Category").Value 
									targetRow("UFR_Driver") = xftvRow.Item("UFR_Driver").Value 
									targetRow("UFR_Driver_Explanation") = xftvRow.Item("UFR_Driver_Explanation").Value 
									targetRow("UFR_Capability_GAP") = xftvRow.Item("UFR_Capability_GAP").Value 
									targetRow("UFR_Capability_GAP_If_CMD_Fund") = xftvRow.Item("UFR_Capability_GAP_If_CMD_Fund").Value 
									targetRow("Solicited_NonSolicited") = xftvRow.Item("Solicited_NonSolicited").Value 
									targetRow("Date_Decision_Needed_By") = xftvRow.Item("Date_Decision_Needed_By").Value 
									targetRow("Funds_Required_By") = xftvRow.Item("Funds_Required_By").Value 
									targetRow("Rollover_To_Next_UFR_Cycle") = xftvRow.Item("Rollover_To_Next_UFR_Cycle").Value 
									targetRow("Army_Campaign_Objectives") = xftvRow.Item("Army_Campaign_Objectives").Value
									targetRow("Initial_Review_Type") = xftvRow.Item("Initial_Review_Type").Value

		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName)) 
							Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
'						Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
                    End Try
		                Next
						
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
						
		                End Using
		            End Using
				
				
            Return Nothing
        End Function
#End Region 

#End Region

#Region "Dynamic Get and Save Staffing Narrative TableViews"		
#Region "Dynamic Get Staffing Narrative TableView"
        Private Function Get_Staffing_Narrative() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim sEntity As String =  args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_FundsCenter","")
			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
			Brapi.ErrorLog.LogMessage(si, "ReqID = " & ReqID)
#Region "Delete"
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
#End Region
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String 
'	Brapi.ErrorLog.LogMessage(si, "Inside Dyn Narrative TableViewName = " & args.TableViewName.ToString())
			Dim targetGroup As String 
			Dim isAuthorized As Boolean 
			Dim sStaffElement As String = args.TableViewName.Split("_"c)(2)
			
'Brapi.ErrorLog.LogMessage(si, $"sStaffElement - {sStaffElement}")
			Select Case args.TableViewName
#Region "Delete"				
'				Case "Staffing_Narrative_G1"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G1_Input], Req.[G1_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'						"
#End Region					
				Case $"Staffing_Narrative_{sStaffElement}" 
					sql = $"SELECT
							    Req.[CMD_UFR_Tracking_No], 
								Staff.[Tracking_No_ID],
								Staff.[BRP_Candidate],
								Staff.[BRP_Topic],
								Staff.[Pre_BRP_Date],
								Staff.[COL_BRP_Date],
								Staff.[Two_Star_Date],
								Staff.[Three_Star_Date],
								Staff.[BRP_Notes],
								Staff.[ABO_Decision],
								Staff.[Review_Input],
								Staff.[Review_POC],
							    Req.[WFCMD_Name], 
								Req.[WFScenario_Name], 
								Req.[WFTime_Name],
							    Att.Attach_File_Name
							FROM XFC_CMD_UFR AS Req
							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
							RIGHT JOIN XFC_CMD_Staffing_Input AS Staff
							ON Req.UFR_ID = Staff.UFR_ID
							WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
							AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
							AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
							AND Req.UFR_ID = '{ReqID}'
							AND Staff.Level = '{entityLevel}'
							AND Staff.Staffing_Element = '{sStaffElement}'
							--AND 1 <> 1
						"
#Region "Delete"					
'				Case "Staffing_Narrative_G2"
''					targetGroup = "g_UFR_AFC_G2"
''					isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''					BRapi.ErrorLog.LogMessage(si, $"User={si.UserName} InGroup({targetGroup})={isAuthorized}")
			
''					If isAuthorized = True Then
''			            xftv.CanModifyData = True
''					Else
''						xftv.CanModifyData = False
''					End If
					
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G2_Input], Req.[G2_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
'				Case "Staffing_Narrative_G3"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G3_Input], Req.[G3_POC], Req.BRP_Topic,
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G4"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G4_Input], Req.[G4_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G5"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G5_Input], Req.[G5_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G6"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G6_Input], Req.[G6_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G7"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G7_Input], Req.[G7_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G8"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G8_Input], Req.[G8_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_JAG"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[JAG_Input], Req.[JAG_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G357"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G_3_5_7_Review_Input], Req.[G_3_5_7_Review_POC],
'								Req.[BRP_Topic],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_G8PAE"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[G8_PAE_Review_Input], Req.[G8_PAE_Review_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							--AND 1 <> 1
'							"
					
'				Case "Staffing_Narrative_PEG"
'					sql = $"SELECT
'							    Req.[CMD_UFR_Tracking_No], 
'							    Req.[PEG_Review_Input], Req.[PEG_Review_POC],
'							    Req.[WFCMD_Name], 
'								Req.[WFScenario_Name], 
'								Req.[WFTime_Name],
'							    Att.Attach_File_Name
'							FROM XFC_CMD_UFR AS Req
'							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'							AND Req.UFR_ID = '{ReqID}'
'							AND 1 <> 1
'							"
					
''				Case "Staffing_Narrative_Integrator"
''					sql = $"SELECT
''							    Req.[CMD_UFR_Tracking_No], 
''							    Req.[Integrator_Input], Req.[Integrator_POC],
''							    Req.[WFCMD_Name], 
''								Req.[WFScenario_Name], 
''								Req.[WFTime_Name],
''							    Att.Attach_File_Name
''							FROM XFC_CMD_UFR AS Req
''							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
''							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
''							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
''							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
''							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
''							AND Req.UFR_ID = '{ReqID}'
''							AND 1 <> 1
''							"
					
''				Case "Staffing_Narrative_ABO"
''					sql = $"SELECT
''							    Req.[ABO_Decision], 
''								Req.[ABO_Funded_Amount], 
''								Req.[ABO_Review_Input], 
''								Req.[ABO_Review_POC], 
''								Req.[ABO_UFR_Status],
''							    Att.Attach_File_Name
''							FROM XFC_CMD_UFR AS Req
''							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
''							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
''							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
''							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
''							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
''							AND Req.UFR_ID = '{ReqID}'
''							--AND 1 <> 1
''							"
					
''				Case "Staffing_Narrative_BRP"
''					sql = $"SELECT
''							    Req.[CMD_UFR_Tracking_No], 
''							    Req.[Pre_BRP_Date], 
''							    Req.[COL_BRP_Date], 
''							    Req.[Two_Star_BRP_Date], 
''							    Req.[Three_Star_BRP_Date], 
''							    Req.[BRP_Notes], 
''							    Req.[WFCMD_Name], 
''								Req.[WFScenario_Name], 
''								Req.[WFTime_Name],
''							    Att.Attach_File_Name
''							FROM XFC_CMD_UFR AS Req
''							LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
''							ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
''							WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
''							AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
''							AND WFTime_Name = '{wfInfoDetails("TimeName")}'
''							AND Req.UFR_ID = '{ReqID}'
''							--AND 1 <> 1
''							"					
#End Region					

			End Select
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function


#End Region

#Region "Dynamic Save Staffing Narrative TableView"
        Private Function Save_Staffing_Narrative() As Object
			
			Dim sEntity As String =  args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_FundsCenter","")
			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
			Dim ReqID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")

            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
'                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
                        Dim sqaReader As New SQA_XFC_CMD_Staffing_Input(sqlConn)
#Region "Delete"
'                        Dim Sql As String = $"SELECT * 
'											FROM XFC_CMD_UFR
'											WHERE WFScenario_Name = @WFScenario_Name
'											AND WFCMD_Name = @WFCMD_Name
'											AND WFTime_Name = @WFTime_Name"
#End Region

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_Staffing_Input 
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name
											AND UFR_ID = UFR_ID"
#Region "Delete"						
'                        Dim Sql As String = $"SELECT 
'											Req.[UFR_ID],
'											Req.Update_Date,
'											Req.Update_User,
'											Staff.[Tracking_No_ID],
'											Staff.[WFScenario_Name],
'											Staff.[WFCMD_Name],
'											Staff.[WFTime_Name],
'											Staff.Level,
'											Staff.Staffing_Element,
'											Staff.Review_Input,
'											Staff.Review_POC,
'											Staff.[Module],
'											Staff.[BRP_Candidate],
'											Staff.[BRP_Topic],
'											Staff.[Pre_BRP_Date],
'											Staff.[COL_BRP_Date],
'											Staff.[Two_Star_Date],
'											Staff.[Three_Star_Date],
'											Staff.[BRP_Notes],
'											Staff.[ABO_Decision],
'											Staff.[UFR_ID]
'											FROM XFC_CMD_Staffing_Input AS Staff
'											RIGHT JOIN XFC_CMD_UFR AS Req
'											ON Staff.UFR_ID = Req.UFR_ID
'											WHERE Staff.WFScenario_Name = @WFScenario_Name
'											AND Staff.WFCMD_Name = @WFCMD_Name
'											AND Staff.WFTime_Name = @WFTime_Name
'											AND Staff.UFR_ID = Req.UFR_ID
'											"
#End Region						
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
'                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
                        sqaReader.Fill_XFC_CMD_Staffing_Input_DT(sqa, dt, Sql, sqlparams)
						brapi.ErrorLog.LogMessage(si, $"Staffing DT rows={dt.Rows.Count}, cols={dt.Columns.Count}")
						
						Dim targetGroup As String 
						Dim isAuthorized As Boolean 
						Dim sStaffElement As String = args.TableViewName.Split("_"c)(2)
						
						For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
'								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
								Dim req_ID_Col = xftvRow.Item("Tracking_No_ID")
	                            Dim targetRow As DataRow = dt.NewRow()
	                            Dim isInsert As Boolean = True
								Dim cellValObj = req_ID_Col.OriginalValue
'								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
'								targetRow = dt.Select($"Tracking_No_ID = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
'								targetRow = dt.Select($"UFR_ID = '{ReqID}'").FirstOrDefault()
'								Brapi.ErrorLog.LogMessage(si, "targetRow = " & targetRow.ToString())

								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Tracking_No_ID") = System.Guid.NewGuid().ToString("D")
								targetRow("UFR_ID") = ReqID
								targetRow("Module") = "UFR"

								Select Case args.TableViewName
#Region "Delete"									
'									' --- G1 Staffing Narrative ---
'									Case "Staffing_Narrative_G1"
''										targetGroup = "g_UFR_G1"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G1_Input") = xftvRow.Item("G1_Input").Value
'											targetRow("G1_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G1")
''										End If
#End Region
									' --- All Staffing Narrative ---
									Case $"Staffing_Narrative_{sStaffElement}"
	Brapi.ErrorLog.LogMessage(si, $"Inside Staffing {sStaffElement} and {entityLevel}")
'										targetGroup = "g_UFR_G1"
'										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
'										If isAuthorized = False Then
'											Throw New Exception($"User: {si.UserName} is does not have access to edit")
'										Else

										If sStaffElement.XFContainsIgnoreCase("G3") And entityLevel = "L2" Then
											targetRow("Staffing_Element") = sStaffElement
											targetRow("BRP_Candidate") = xftvRow.Item("BRP_Candidate").Value
											targetRow("Review_Input") = xftvRow.Item("Review_Input").Value
											targetRow("Review_POC") = si.UserName
											targetRow("Level") = entityLevel
											
										Else If sStaffElement.XFContainsIgnoreCase("G3") And entityLevel = "L1" Then
											targetRow("Staffing_Element") = sStaffElement
											targetRow("BRP_Topic") = xftvRow.Item("BRP_Topic").Value
											targetRow("Review_Input") = xftvRow.Item("Review_Input").Value
											targetRow("Review_POC") = si.UserName
											targetRow("Level") = entityLevel
											
										Else If sStaffElement.XFContainsIgnoreCase("BRP") And entityLevel = "L1" Then
											targetRow("Staffing_Element") = sStaffElement
											targetRow("Pre_BRP_Date") = xftvRow.Item("Pre_BRP_Date").Value
											targetRow("COL_BRP_Date") = xftvRow.Item("COL_BRP_Date").Value
											targetRow("Two_Star_Date") = xftvRow.Item("Two_Star_Date").Value
											targetRow("Three_Star_Date") = xftvRow.Item("Three_Star_Date").Value
											targetRow("BRP_Notes") = xftvRow.Item("BRP_Notes").Value
											targetRow("Review_Input") = xftvRow.Item("Review_Input").Value
											targetRow("Review_POC") = si.UserName
											targetRow("Level") = entityLevel
											
										Else If sStaffElement.XFContainsIgnoreCase("ABO") And entityLevel = "L1" Then
											targetRow("ABO_Decision") = xftvRow.Item("ABO_Decision").Value
											targetRow("Staffing_Element") = sStaffElement
											targetRow("Review_Input") = xftvRow.Item("Review_Input").Value
											targetRow("Review_POC") = si.UserName
											targetRow("Level") = entityLevel
											
										Else 
											targetRow("Staffing_Element") = sStaffElement
											targetRow("Review_Input") = xftvRow.Item("Review_Input").Value
											targetRow("Review_POC") = si.UserName
											targetRow("Level") = entityLevel
											Brapi.ErrorLog.LogMessage(si, "After Target Row inserts")
											
										End If


#Region "Delete"										
									' --- G2 Staffing Narrative ---
'									Case "Staffing_Narrative_G2"
''										targetGroup = "g_UFR_G2"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										BRapi.ErrorLog.LogMessage(si, $"User={si.UserName} InGroup({targetGroup})={isAuthorized}")
										
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G2_Input") = xftvRow.Item("G2_Input").Value
'											targetRow("G2_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G2")
''										End If
										
'									' --- G3 Staffing Narrative ---
'									Case "Staffing_Narrative_G3"
''										targetGroup = "g_UFR_G3"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
										
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G3_Input") = xftvRow.Item("G3_Input").Value
'											targetRow("BRP_Topic") = xftvRow.Item("BRP_Topic").Value
'											targetRow("G3_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G3")
''										End If
										
'									' --- G4 Staffing Narrative ---
'									Case "Staffing_Narrative_G4"
''										targetGroup = "g_UFR_G4"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G4_Input") = xftvRow.Item("G4_Input").Value
'											targetRow("G4_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G4")
''										End If
										
'									' --- G5 Staffing Narrative ---
'									Case "Staffing_Narrative_G5"
''										targetGroup = "g_UFR_G5"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G5_Input") = xftvRow.Item("G5_Input").Value
'											targetRow("G5_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G5")
''										End If
										
'									' --- G6 Staffing Narrative ---
'									Case "Staffing_Narrative_G6"
''										targetGroup = "g_UFR_G6"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G6_Input") = xftvRow.Item("G6_Input").Value
'											targetRow("G6_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G6")
''										End If
										
'									' --- G7 Staffing Narrative ---
'									Case "Staffing_Narrative_G7"
''										targetGroup = "g_UFR_G7"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G7_Input") = xftvRow.Item("G7_Input").Value
'											targetRow("G7_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G7")
''										End If
										
'									' --- G8 Staffing Narrative ---
'									Case "Staffing_Narrative_G8"
''										targetGroup = "g_UFR_G8"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("G8_Input") = xftvRow.Item("G8_Input").Value
'											targetRow("G8_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing G8")
''										End If
										
'									' --- JAG Staffing Narrative ---
'									Case "Staffing_Narrative_JAG"
''										targetGroup = "g_UFR_JAG"
''										isAuthorized = BRApi.Security.Authorization.IsUserInGroup(si, targetGroup)
''										If isAuthorized = False Then
''											Throw New Exception($"User: {si.UserName} is does not have access to edit")
''										Else
'											targetRow("JAG_Input") = xftvRow.Item("JAG_Input").Value
'											targetRow("JAG_POC") = si.UserName
'											Brapi.ErrorLog.LogMessage(si, "Inside Staffing JAG")
''										End If
										
'									' --- PEG Staffing Narrative ---
'									Case "Staffing_Narrative_PEG"
'										targetRow("PEG_Review_Input") = xftvRow.Item("PEG_Review_Input").Value
'										targetRow("PEG_Review_POC") = si.UserName
										
'									' --- Integrator Staffing Narrative ---
'									Case "Staffing_Narrative_Integrator"
'										targetRow("Integrator_Input") = xftvRow.Item("Integrator_Input").Value
'										targetRow("Integrator_POC") = si.UserName
										
'									' --- G3/5/7 Staffing Narrative ---
'									Case "Staffing_Narrative_G357"
'										targetRow("G_3_5_7_Review_Input") = xftvRow.Item("G_3_5_7_Review_Input").Value
'										targetRow("BRP_Topic") = xftvRow.Item("BRP_Topic").Value
'										targetRow("G_3_5_7_Review_POC") = si.UserName
										
'									' --- G8PAE Staffing Narrative ---
'									Case "Staffing_Narrative_G8PAE"
'										targetRow("G8_PAE_Review_Input") = xftvRow.Item("G8_PAE_Review_Input").Value
'										targetRow("G8_PAE_Review_POC") = si.UserName
										
'									' --- ABO Staffing Narrative ---
'									Case "Staffing_Narrative_ABO"
'										targetRow("ABO_Decision") = xftvRow.Item("ABO_Decision").Value
'										targetRow("ABO_Funded_Amount") = xftvRow.Item("ABO_Funded_Amount").Value
'										targetRow("ABO_Review_Input") = xftvRow.Item("ABO_Review_Input").Value
'										targetRow("ABO_Review_POC") = si.UserName
'										targetRow("ABO_UFR_Status") = xftvRow.Item("ABO_UFR_Status").Value
										
'									' --- BRP Staffing Narrative ---
'									Case "Staffing_Narrative_BRP"
'										targetRow("Pre_BRP_Date") = xftvRow.Item("Pre_BRP_Date").Value
'										targetRow("COL_BRP_Date") = xftvRow.Item("COL_BRP_Date").Value
'										targetRow("Two_Star_BRP_Date") = xftvRow.Item("Two_Star_BRP_Date").Value
'										targetRow("Three_Star_BRP_Date") = xftvRow.Item("Three_Star_BRP_Date").Value
'										targetRow("BRP_Notes") = xftvRow.Item("BRP_Notes").Value
#End Region
									End Select
								
								' --- Status and Audit Fields ---
'								targetRow("Update_Date") = DateTime.Now
'								targetRow("Update_User") = si.UserName
                             
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
						 Throw
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
'		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                sqaReader.Update_XFC_CMD_Staffing_Input(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 



#End Region		

#Region "Delete"

#Region "Staffing_Narrative_G8"		
#Region "Function: Get_Staffing_Narrative_G8 - Get Staffing Narrative For G8"
        Private Function Get_Staffing_Narrative_G8() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
								    Req.[CMD_UFR_Tracking_No], 
								    Req.[G8_Input], Req.[G8_POC],
								    Req.[WFCMD_Name], 
									Req.[WFScenario_Name], 
									Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_G8 - Save Staffing Narrative for G8"
        Private Function Save_Staffing_Narrative_G8() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- G8 Fields ---
								targetRow("G8_Input") = xftvRow.Item("G8_Input").Value
								targetRow("G8_POC") = xftvRow.Item("G8_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region		

#Region "Staffing_Narrative_JAG"		
#Region "Function: Get_Staffing_Narrative_JAG - Get Staffing Narrative For JAG"
        Private Function Get_Staffing_Narrative_JAG() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
								    Req.[CMD_UFR_Tracking_No], 
								    Req.[JAG_Input], Req.[JAG_POC],
								    Req.[WFCMD_Name], 
									Req.[WFScenario_Name], 
									Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_JAG - Save Staffing Narrative for JAG"
        Private Function Save_Staffing_Narrative_JAG() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- JAG Fields ---
								targetRow("JAG_Input") = xftvRow.Item("JAG_Input").Value
								targetRow("JAG_POC") = xftvRow.Item("JAG_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region		

#End Region

#Region "Delete"
		
#Region "Staffing_Narrative_ABO"		
#Region "Function: Get_Staffing_Narrative_ABO - Staffing Narrative From ABO"
        Private Function Get_Staffing_Narrative_ABO() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
								    Req.ABO_Decision, 
									Req.[ABO_Funded_Amount], 
									Req.[ABO_Review_Input], 
									Req.[ABO_Review_POC], 
									Req.[ABO_UFR_Status],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_ABO - Save Staffing Narrative From ABO"
        Private Function Save_Staffing_Narrative_ABO() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- ABO Specific Fields ---
								targetRow("ABO_Decision") = xftvRow.Item("ABO_Decision").Value ' Assuming this is a new input field
								targetRow("ABO_Funded_Amount") = xftvRow.Item("ABO_Funded_Amount").Value ' Assuming this is a new input field
								targetRow("ABO_Review_Input") = xftvRow.Item("ABO_Review_Input").Value
								targetRow("ABO_Review_POC") = xftvRow.Item("ABO_Review_POC").Value
								targetRow("ABO_UFR_Status") = xftvRow.Item("ABO_UFR_Status").Value
								
								' --- Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region	

#Region "Staffing Narrative HQ G8"		
#Region "Function: Get_Staffing_Narrative_HQ_G8 - Get Staffing Narrative For HQ G8"
        Private Function Get_Staffing_Narrative_HQ_G8() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
									Req.CMD_UFR_Tracking_No,
								    Req.[CMD_HQ_G8_Submission_Approver], 
									Req.[CMD_HQ_G8_Submission_POC], 
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_HQ_G8 - Save Staffing Narrative For HQ G8"
        Private Function Save_Staffing_Narrative_HQ_G8() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- HQ G8 Fields ---
								targetRow("CMD_HQ_G8_Submission_Approver") = xftvRow.Item("CMD_HQ_G8_Submission_Approver").Value
								targetRow("CMD_HQ_G8_Submission_POC") = xftvRow.Item("CMD_HQ_G8_Submission_POC").Value
								
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region		

#Region "Staffing_Narrative_G_3_5_7"		
#Region "Function: Get_Staffing_Narrative_G_3_5_7 - Get Staffing Narrative For G 3/5/7"
        Private Function Get_Staffing_Narrative_G_3_5_7() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
								    Req.[CMD_UFR_Tracking_No], 
								    Req.[G_3_5_7_Review_Input], 
									Req.[G_3_5_7_Review_POC],
								    Req.[WFCMD_Name], 
									Req.[WFScenario_Name], 
									Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_G_3_5_7 - Save Staffing Narrative For G 3/5/7"
        Private Function Save_Staffing_Narrative_G_3_5_7() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- Review/Approval Fields (New in XFC_CMD_UFR, assumed to be blank on update unless new input fields exist) ---
								targetRow("G_3_5_7_Review_Input") = xftvRow.Item("G_3_5_7_Review_Input").Value
								targetRow("G_3_5_7_Review_POC") = xftvRow.Item("G_3_5_7_Review_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region	

#Region "Staffing_Narrative_G8_PAE"		
#Region "Function: Get_Staffing_Narrative_G8_PAE - Get Staffing Narrative For G8 PAE"
'        Private Function Get_Staffing_Narrative_G8_PAE() As TableView
'            Dim dt As New DataTable()
'            Dim xftv As New TableView()
'			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
'			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
''			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
''			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
''			End If
''			If String.IsNullOrWhiteSpace(ReqID)
''				Return Nothing
''			Else
''			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

''			Dim Entity As String  =  REQ_ID_Split(0)
''			Dim RequirementID As String  = REQ_ID_Split(1)
			
'            xftv.CanModifyData = True
'			xftv.NumberOfEmptyRowsToAdd = 1
'			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
'			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'            Dim sql As String = $"SELECT
'								    Req.[CMD_UFR_Tracking_No], 
'								    Req.[G8_PAE_Review_Input], 
'									Req.[G8_PAE_Review_POC],
'								    Req.[WFCMD_Name], 
'									Req.[WFScenario_Name], 
'									Req.[WFTime_Name],
'								    Att.Attach_File_Name
'								FROM XFC_CMD_UFR AS Req
'								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'								"

'            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
'            End Using
			
'			xftv.PopulateFromDataTable(dt,True,True)
			
'            Return xftv
''		End If
'        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_G8_PAE - Save Staffing Narrative for G8 PAE"
        Private Function Save_Staffing_Narrative_G8_PAE() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- G8 PAE Fields ---
								targetRow("G8_PAE_Review_Input") = xftvRow.Item("G8_PAE_Review_Input").Value
								targetRow("G8_PAE_Review_POC") = xftvRow.Item("G8_PAE_Review_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region	

#Region "Staffing_Narrative_Integrator"		
#Region "Function: Get_Staffing_Narrative_Integrator - Get Staffing Narrative For Integrator"
'        Private Function Get_Staffing_Narrative_Integrator() As TableView
'            Dim dt As New DataTable()
'            Dim xftv As New TableView()
'			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
'			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
''			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
''			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
''			End If
''			If String.IsNullOrWhiteSpace(ReqID)
''				Return Nothing
''			Else
''			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

''			Dim Entity As String  =  REQ_ID_Split(0)
''			Dim RequirementID As String  = REQ_ID_Split(1)
			
'            xftv.CanModifyData = True
'			xftv.NumberOfEmptyRowsToAdd = 1
'			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
'			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'            Dim sql As String = $"SELECT
'								    Req.[CMD_UFR_Tracking_No], 
'								    Req.[Integrator_Input], Req.[Integrator_POC],
'								    Req.[WFCMD_Name], 
'									Req.[WFScenario_Name], 
'									Req.[WFTime_Name],
'								    Att.Attach_File_Name
'								FROM XFC_CMD_UFR AS Req
'								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
'								"
			
'            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
'            End Using
			
'			xftv.PopulateFromDataTable(dt,True,True)
			
'            Return xftv
''		End If
'        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_Integrator - Save Staffing Narrative for Integrator"
        Private Function Save_Staffing_Narrative_Integrator() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- Integrator Fields ---
								targetRow("Integrator_Input") = xftvRow.Item("Integrator_Input").Value
								targetRow("Integrator_POC") = xftvRow.Item("Integrator_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region		

#Region "Staffing_Narrative_PEG"		
#Region "Function: Get_Staffing_Narrative_PEG - Get Staffing Narrative For PEG"
'        Private Function Get_Staffing_Narrative_PEG() As TableView
'            Dim dt As New DataTable()
'            Dim xftv As New TableView()
'			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
'			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
''			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
''			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
''				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
''			End If
''			If String.IsNullOrWhiteSpace(ReqID)
''				Return Nothing
''			Else
''			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

''			Dim Entity As String  =  REQ_ID_Split(0)
''			Dim RequirementID As String  = REQ_ID_Split(1)
			
'            xftv.CanModifyData = True
'			xftv.NumberOfEmptyRowsToAdd = 1
'			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
'			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'            Dim sql As String = $"SELECT
'								    Req.[CMD_UFR_Tracking_No], 
'								    Req.[PEG_Review_Input], 
'									Req.[PEG_Review_POC],
'								    Req.[WFCMD_Name], 
'									Req.[WFScenario_Name], 
'									Req.[WFTime_Name],
'								    Att.Attach_File_Name
'								FROM XFC_CMD_UFR AS Req
'								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
'								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
'								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
'								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								
'								"

'            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
'            End Using
			
'			xftv.PopulateFromDataTable(dt,True,True)
			
'            Return xftv
''		End If
'        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_PEG - Save Staffing Narrative for PEG"
        Private Function Save_Staffing_Narrative_PEG() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- PEG Fields ---
								targetRow("PEG_Review_Input") = xftvRow.Item("PEG_Review_Input").Value
								targetRow("PEG_Review_POC") = xftvRow.Item("PEG_Review_POC").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region		

#Region "Staffing_Narrative_BRP"		
#Region "Function: Get_Staffing_Narrative_BRP - Get Staffing Narrative for BRP"
        Private Function Get_Staffing_Narrative_BRP() As TableView
            Dim dt As New DataTable()
            Dim xftv As New TableView()
			Dim menuOption As String = args.CustSubstVarsAlreadyResolved.XFGetValue("DL_GBL_WF_MenuOptions")
			Dim ReqID As String = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_UFR_REQTitleList")
'			If menuOption.XFContainsIgnoreCase("REQWHDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListWH")
'			ElseIf menuOption.XFContainsIgnoreCase("REQCivDetails") Then
'				ReqID = args.CustSubstVarsAlreadyResolved.XFGetValue("BL_CMD_SPLN_REQTitleListCiv")
'			End If
'			If String.IsNullOrWhiteSpace(ReqID)
'				Return Nothing
'			Else
'			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")

'			Dim Entity As String  =  REQ_ID_Split(0)
'			Dim RequirementID As String  = REQ_ID_Split(1)
			
            xftv.CanModifyData = True
			xftv.NumberOfEmptyRowsToAdd = 1
			xftv.EmptyRowsBackgroundColor = XFColors.AliceBlue
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
            Dim sql As String = $"SELECT
								    Req.[CMD_UFR_Tracking_No], 
								    Req.[Pre_BRP_Date], 
								    Req.[COL_BRP_Date], 
								    Req.[Two_Star_BRP_Date], 
								    Req.[Three_Star_BRP_Date], 
								    Req.[BRP_Notes], 
								    Req.[WFCMD_Name], 
									Req.[WFScenario_Name], 
									Req.[WFTime_Name],
								    Att.Attach_File_Name
								FROM XFC_CMD_UFR AS Req
								LEFT JOIN XFC_CMD_UFR_Attachment AS Att -- Assuming the attachment table name changes to match the new UFR table
								ON Req.CMD_UFR_Tracking_No = Att.CMD_UFR_Tracking_No -- Joining on the new PK
								WHERE WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND WFTime_Name = '{wfInfoDetails("TimeName")}'
								
								"

		
            Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			xftv.PopulateFromDataTable(dt,True,True)
			
            Return xftv
'		End If
        End Function
#End Region
		
#Region "Function: Save_Staffing_Narrative_BRP - Save Staffing Narrative For BRP"
        Private Function Save_Staffing_Narrative_BRP() As Object
            Dim xftv As New TableView()
			xftv = args.TableView
            If xftv Is Nothing OrElse xftv.Rows.Count = 0 Then 
				Return Nothing
			Else
	            Dim dt As New DataTable()
	            Dim sqa As New SqlDataAdapter()
				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

                Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                        sqlConn.Open()
                        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)

                        Dim Sql As String = $"SELECT * 
											FROM XFC_CMD_UFR
											WHERE WFScenario_Name = @WFScenario_Name
											AND WFCMD_Name = @WFCMD_Name
											AND WFTime_Name = @WFTime_Name"
						
						Dim sqlparams As SqlParameter() = New SqlParameter() {
						    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
                        sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, Sql, sqlparams)
						
            			For Each xftvRow As TableViewRow In xftv.Rows.Where(Function(r) Not r.IsHeader)
							Try
								
								Dim req_ID_Col = xftvRow.Item("CMD_UFR_Tracking_No")
	                            Dim targetRow As DataRow
	                            Dim isInsert As Boolean = False
			                 
								Dim cellValObj = req_ID_Col.OriginalValue
								' **Update for new Primary Key:** CMD_UFR_Tracking_No (GUID)
								targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Col.OriginalValue.xfconverttoGUID}'").FirstOrDefault()
								
								' --- Workflow and Core Identifiers ---
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								
								' --- BRP Fields ---
								targetRow("Pre_BRP_Date") = xftvRow.Item("Pre_BRP_Date").Value
								targetRow("COL_BRP_Date") = xftvRow.Item("COL_BRP_Date").Value
								targetRow("Two_Star_BRP_Date") = xftvRow.Item("Two_Star_BRP_Date").Value
								targetRow("Three_Star_BRP_Date") = xftvRow.Item("Three_Star_BRP_Date").Value
								targetRow("BRP_Notes") = xftvRow.Item("BRP_Notes").Value
								
								' --- Status and Audit Fields ---
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
                             
		                    ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,xftvRow)
                            For Each colName As String In dirtyColList
                                targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, xftvRow.Item(colName))   
								Next
		                    ' If this is an insert, add the new row to the DataTable
  					 Catch ex As Exception
                    End Try

		                Next
					
		                ' Persist changes back to the DB using the configured adapter
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                End Using
		            End Using
			End If
            Return Nothing
        End Function
#End Region 
#End Region

#End Region

	End Class
End Namespace