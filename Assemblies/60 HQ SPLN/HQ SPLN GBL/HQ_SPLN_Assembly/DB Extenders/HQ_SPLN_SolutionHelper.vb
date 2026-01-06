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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.HQ_SPLN_SolutionHelper
	Public Class MainClass
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As Object
		Public args As DashboardExtenderArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
'						
						
						Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
							
						If args.FunctionName.XFEqualsIgnoreCase("ExportSpendPlanReport")
							Return Me.ExportSpendPlanReport()
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("CachePrompts")
							Return Me.CachePrompts(si, globals, api, args)
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("ReturnSPLN")
							Return Me.ReturnSPLN(si, globals, api, args)
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("VerifyandSubmitOMA")
							Return Me.VerifyandSubmitOMA(si, globals, api, args)
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("VerifyandSubmitOPA")
							Return Me.VerifyandSubmitOPA(si, globals, api, args)
							
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("VerifyandSubmitRDTE")
							Return Me.VerifyandSubmitRDTE(si, globals, api, args)
							
						End If
						
						
						If args.FunctionName.XFEqualsIgnoreCase("SpendObligWarning")
							Return Me.SpendObligWarning(si, globals, api, args)
							
						End If
						
						
		
#Region "EmailSubmitandVerifySpendPlan"
'						If args.FunctionName.XFEqualsIgnoreCase("EmailSubmitandVerifySpendPlan") Then
'							Return Me.EmailSubmitandVerifySpendPlan()
'						End If
#End Region
					
						
						
						
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
'--------------------Methods----------------------------
#Region "Constants"
'	Private BR_BFRM_SolutionHelper As New OneStream.BusinessRule.DashboardExtender.BFRM_SolutionHelper.MainClass
'	Private SPLN_Member_Lists As New OneStream.BusinessRule.Finance.SPLN_Member_Lists.MainClass
#End Region
		
#Region "UpdateCustomSubstVar"
		Private Sub UpdateCustomSubstVar(ByRef Result As XFLoadDashboardTaskResult,ByVal key As String,ByVal value As String)
			If Result.ModifiedCustomSubstVars.ContainsKey(key)
				Result.ModifiedCustomSubstVars.XFSetValue(key,value)
			Else
				Result.ModifiedCustomSubstVars.Add(key,value)
			End If
			
		End Sub
#End Region		

#Region "EmailSubmitandVerifySpendPlan"
		Private Function EmailSubmitandVerifySpendPlan() As XFSelectionChangedTaskResult
			Try
			' Initialize variables
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			
	
		
			' Connection to the mail server
			Dim emailConnectionName As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
			
			' Email content (Subject & Body)
			Dim subject As String = "RMW -- Spend Plan has been verified and submitted"
			Dim messageBody As String = " (" & wfCube & ") has verifed and submitted their Spend Plan "
			   
			' Get list of users
			Dim toEmail As New List(Of String)
			
			
			Dim SQL As String = String.Empty
			Dim tableName As String = "ABOUsers"
			Dim sUserEmail As String =String.Empty
			SQL= $"Select Distinct U.Name As UserName, U.Email As UserEmail
									From  [SecUser] As u
									Join  [SecGroupChild] As GU On gu.ChildKey = u.uniqueid
									Join  [SecGroup] As SG On sg.UniqueID = gu.GroupKey
									Where (SG.Name Like  '%ABO%')"

			
									Dim dtAll As New DataTable
									Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
									 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
									End Using


			For Each row As DataRow In dtAll.Rows
			sUserEmail = row.Item("UserEmail")
			toEmail.Add(sUserEmail)
			Next
			
'			For Each item In toEmail
'				BRAPI.ErrorLog.LogMessage(si, item)
'			Next
			' Send the message
			BRApi.Utilities.SendMail(si, emailConnectionName, toEmail, subject, messageBody, Nothing)
		
			' Return message to user
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				selectionChangedTaskResult.IsOK = True
				selectionChangedTaskResult.ShowMessageBox = False
				selectionChangedTaskResult.Message = "Email Sent."
				selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
				selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
				selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
			Return selectionChangedTaskResult
		Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try			
		End Function
				
#End Region

#Region "Export Spend Plan"
Public Function ExportSpendPlanReport()
	Dim sTemplate As String = args.NameValuePairs.XFGetValue("APPN")
	
	Select Case sTemplate
		Case "OMA"			
			Return Me.ExportOMAReport(si,globals,api,args)
		Case "OPA"	
			Return Me.ExportOPAReport(si,globals,api,args)
		Case "RDTE"	
			Return Me.ExportRDTEReport(si,globals,api,args)
	End Select
	
	Return Nothing
	
End Function

#End Region

#Region "Property"
	Private Property sFilePath As String = ""
#End Region

#Region "Validate Target"	
	Public Function Validate_HQ_SPLN() As XFSelectionChangedTaskResult
		Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
		Dim Entity As String = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("BL_CMD_TGT_FundsCenter","NA")	
		Dim cvName = "CMD_TGT_FDX_ValidateDist_CV"
		Dim dt As New DataTable 
		dt = GetFDXvalidateTGTData(cvName,Entity)
		
		
		Dim filteredRows = dt.AsEnumerable().Where(Function(row) row.Field(Of Decimal)("Time1") <> 0)
		If filteredRows.Any() Then
			selectionChangedTaskResult.IsOK = False
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = $"{Entity} did not Validate Successfully, please update your Distributions."
		Else
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = $"{Entity} Validated Successfully."
		End If
		Return selectionChangedTaskResult
	End Function
		#End Region
		
		Private Function GetFDXvalidateTGTData(ByVal cvName As String,ByVal entFilter As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "60 HQ SPLN"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{wfInfoDetails("ScenarioName")}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)

			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 8, False)

			Return dt
		End Function

#Region "Export  OMA Report"

'Export Obligation or Commitment data to be used to import back into ARMY's system
		Public Function ExportOMAReport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)		
			' for grabbing current year and past 2 years for template headers
		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey) ' for grabbing current year for template headers
			Dim year As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim FY1 As Integer = Convert.ToInt32(year)
			Dim FY2 As Integer = FY1 - 1 
			Dim FY3 = year - 2	
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("APPN")
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType")
			Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			Dim sCube As String = wfProfile.CubeName
			
			'CMD Entity
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim
			Dim sEntityGeneral As String = sEntity & "_General"
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim sTime As String = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sAmountType As String = args.NameValuePairs.XFGetValue("AmountType","")
			Dim sAccount As String = ""
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""

			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
			If sReportType.XFContainsIgnoreCase("oblig") Then sAccount = "Obligations"
			If sReportType.XFContainsIgnoreCase("commit") Then sAccount = "Commitments"

				
			If String.IsNullOrWhiteSpace(sTemplate) 
				Throw New Exception("Please select an appropriation.")
			End If	

			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""

			'Export selection Process
			columns.AddRange(New String(){"FY","ENTITY","U1","U2","U3","U4","U6","TIME","AMOUNT"})
			processColumns.AddRange(New String(){"Command","Fund","SAG","Commodity","October","November","December","January","February","March","April","May","June","July","August","September","Workflow Action"})
			sFileHeader = "Command,Fund,SAG,Commodity,October,November,December,January,February,March,April,May,June,July,August,September,Workflow Action"

				
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
			'Loop through list of base entity members, and loop through 12 months per entity member to get the databuffer data cells then write to fetchDataTable

			' DataUnit coordinates: cubeId, entityId, parentId (use empty string if not needed), consId, scenarioId, timeId
				For i As Integer = 1 To 12 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntityGeneral ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTime & "M" & i))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, "Top")	
					myDbCellPk.UD4Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, "Top")	
					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")	
					myDbCellPk.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "Top")	
					myDbCellPk.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "Top")	
					myDbCellPk.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "Top")
										
					' Get & Loop through different U6#ABO_XX members and write
					Dim oU6List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U6_CIG"), "U6#Obj_Class.Base", True,,)
					Dim oU3List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U3_SAG"), "U3#APPN.Base", True,,)
	
					For Each oU3 As MemberInfo In oU3List
						myDbCellPk.UD3Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD3, oU3.Member.Name)
						For Each oU6 As MemberInfo In oU6List
							myDbCellPk.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, oU6.Member.Name)
							' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntityGeneral,sScenario,sTime & "M" & i,myCells)
						Next
					Next					
				Next	
				
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns)	
			Me.ProcessTableForReport(si, FetchDt, processDT, sAmountType)
			
			processDT.DefaultView.Sort = "Command,Fund,SAG,Commodity"
			processDT = processDT.DefaultView.ToTable()
				
			'Initialize file 
			
#Region "StringBuilders"
			Dim file As New Text.StringBuilder
			file.Append(sFileHeader)	
#End Region
			
			'Populate file
			For Each row As DataRow In processDT.Rows
				Dim rowInfo As String = ""
				For Each column As DataColumn In processDT.Columns
					rowInfo = rowInfo & "," & row(Column)				
				Next
				rowInfo = rowInfo.Remove(0,1)
				rowInfo = rowInfo.Replace("None","")
				file.Append(vbCrLf + rowInfo)
			Next
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","")
			If datetime.Now.Month < 10 Then sTimeStamp = "0" & sTimeStamp			
		    Dim fileName As String = ""
			
			If sTemplate.XFContainsIgnoreCase("OMA") And sAmountType.XFContainsIgnoreCase("YTD") Then
				fileName = $"{sCube}_{sReportType}_{sTime}_{sTemplate}_Cumulative_Export_{sUser}_{sTimeStamp}.csv"
			Else
				fileName = $"{sCube}_{sReportType}_{sTime}_{sTemplate}_Export_{sUser}_{sTimeStamp}.csv"
			End If	
			Me.BuildFile(si, file.ToString, fileName, sCube)
			
			Dim dKeyVal As New Dictionary(Of String,String) From {{sFvParam,sFilePath}}
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"
			
			Return selectionChangedTaskResult

		End Function
	
#End Region

#Region "Export  OPA Report"	
'Export Obligation or Commitment data to be used to import back into ARMY's system

		Public Function ExportOPAReport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)		
			' for grabbing current year and past 2 years for template headers
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey) ' for grabbing current year for template headers
			Dim year As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim FY1 As Integer = Convert.ToInt32(year)
			Dim FY2 As Integer = FY1 - 1 
			Dim FY3 = year - 2	
			Dim sFY1 = FY1.ToString()
			sFY1 = sFY1.Substring(sFY1.Length - 2)
			Dim sFY2 = FY2.ToString()
			sFY2 = sFY2.Substring(sFY2.Length - 2)
			Dim sFY3 = FY3.ToString()
			sFY3 = sFY3.Substring(sFY3.Length - 2)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("APPN")		
			Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			Dim sCube As String = wfProfile.CubeName
			
			'CMD Entity
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim
			Dim sEntityGeneral As String = sEntity & "_General"
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim sTime As String = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sAmountType As String = args.NameValuePairs.XFGetValue("AmountType","")
			Dim sAccount As String = "Obligations"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
	
			If String.IsNullOrWhiteSpace(sTemplate) 
				Throw New Exception("Please select an appropriation.")
			End If	

			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			'Export selection Process
			columns.AddRange(New String(){"ENTITY","U1","U2","U3","U4","U6","U8","TIME","AMOUNT"})
			processColumns.AddRange(New String(){
			"Tracking Number:Tracking Number", 
			"APPN",
			"BA",
			"ROLL_SSN",
			"ROLL_SSN_DESC",
			"SSN",
			"SSN_DESC",
			"CMD",
			"MDEP",
			"DOLLAR_TYPE",
			"PEG",
			"FY " & FY3,
			"FY " & FY2,
			"FY " & FY1,
			"New Start Y/N",
			"HASC Marks",
			"HASC Adds",
			"HASC Cong Lang",
			"SASC Marks",
			"SASC Adds",
			"SASC Cong Lang",
			"FY " & sFY1 & " NDAA",
			"NDAA Cong Lang",
			"HAC-D Adds",
			"HAC Cong Lang",
			"SAC-D Marks",
			"SAC-D Adds",
			"SAC Cong Lang",
			"Approp Conf Adds (+)",
			"Approp Conf Marks (-)",
			"Approp Cong Lang",
			"FY" & sFY1 & "APPROP",
			"MAX CR AUTH",
			":CRA #1 Distribution",
			"CRA #2 Distribution",
			"CRA #3 Distribution",
			"Total CRA Distribution",
			"CRA Balance Remaining",
			"CR SPEND PLAN REQUIRED? Y/N",
			"ABO Comments",
			"FUNDS ISSUED IN (GFEBS or PBAS)",
			"HQDA Comments",
			"Field Comments",
			"Field POC",
			"OCT Reimb DA CIV PAY ($K)",
			"OCT Direct DA CIV PAY ($K)",
			"OCT Contracts ($K)",
			"OCT AllObj ($K)",
			"NOV Reimb DA CIV PAY ($K)",
			"NOV Direct DA CIV PAY ($K)",
			"NOV Contracts ($K)",
			"NOV AllObj ($K)",
			"DEC Reimb DA CIV PAY ($K)",
			"DEC Direct DA CIV PAY ($K)",
			"DEC Contracts ($K)",
			"DEC AllObj ($K)",
			"JAN Reimb DA CIV PAY ($K)",
			"JAN Direct DA CIV PAY ($K)",
			"JAN Contracts ($K)",
			"JAN AllObj ($K)",
			"FEB Reimb DA CIV PAY ($K)",
			"FEB Direct DA CIV PAY ($K)",
			"FEB Contracts ($K)",
			"FEB AllObj ($K)",
			"MAR Reimb DA CIV PAY ($K)",
			"MAR Direct DA CIV PAY ($K)",
			"MAR Contracts ($K)",
			"MAR AllObj ($K)",
			"APR Reimb DA CIV PAY ($K)",
			"APR Direct DA CIV PAY ($K)",
			"APR Contracts ($K)",
			"APR AllObj ($K)",
			"MAY Reimb DA CIV PAY ($K)",
			"MAY Direct DA CIV PAY ($K)",
			"MAY Contracts ($K)",
			"MAY AllObj ($K)",
			"JUN Reimb DA CIV PAY ($K)",
			"JUN Direct DA CIV PAY ($K)",
			"JUN Contracts ($K)",
			"JUN AllObj ($K)",
			"JUL Reimb DA CIV PAY ($K)",
			"JUL Direct DA CIV PAY ($K)",
			"JUL Contracts ($K)",
			"JUL AllObj ($K)",
			"AUG Reimb DA CIV PAY ($K)",
			"AUG Direct DA CIV PAY ($K)",
			"AUG Contracts ($K)",
			"AUG AllObj ($K)",
			"SEP Reimb DA CIV PAY ($K)",
			"SEP Direct DA CIV PAY ($K)",
			"SEP Contracts ($K)",
			"SEP AllObj ($K)",
			"ITEM_TYPE",
			"PATH"
			})
			
			sFileHeader = 
			"Tracking Number:Tracking Number,APPN,BA,ROLL_SSN,ROLL_SSN_DESC,SSN,SSN_DESC,CMD,MDEP,DOLLAR_TYPE,PEG,FY " & FY3 &",FY "& FY2 &",FY "& FY1 &",New Start Y/N,HASC Marks,HASC Adds,HASC Cong Lang,SASC Marks,SASC Adds,SASC Cong Lang,FY "& sFY1 &" NDAA,NDAA Cong Lang,HAC-D Adds,HAC-D Adds,HAC Cong Lang,SAC-D Marks,SAC-D Adds,SAC Cong Lang,Approp Conf Adds (+),Approp Conf Marks (-),Approp Cong Lang,FY"& sFY1 &" APPROP,MAX CR AUTH,:CRA #1 Distribution,CRA #2 Distribution,CRA #3 Distribution,Total CRA Distribution,CRA Balance Remaining,CR SPEND PLAN REQUIRED? Y/N,ABO Comments,FUNDS ISSUED IN (GFEBS or PBAS),HQDA Comments,Field Comments,Field POC,OCT Reimb DA CIV PAY ($K),OCT Direct DA CIV PAY ($K),OCT Contracts ($K),OCT AllObj ($K),NOV Reimb DA CIV PAY ($K),NOV Direct DA CIV PAY ($K),NOV Contracts ($K),NOV AllObj ($K),DEC Reimb DA CIV PAY ($K),DEC Direct DA CIV PAY ($K),DEC Contracts ($K),DEC AllObj ($K),JAN Reimb DA CIV PAY ($K),JAN Direct DA CIV PAY ($K),JAN Contracts ($K),JAN AllObj ($K),FEB Reimb DA CIV PAY ($K),FEB Direct DA CIV PAY ($K),FEB Contracts ($K),FEB AllObj ($K),MAR Reimb DA CIV PAY ($K),MAR Direct DA CIV PAY ($K),MAR Contracts ($K),MAR AllObj ($K),APR Reimb DA CIV PAY ($K),APR Direct DA CIV PAY ($K),APR Contracts ($K),APR AllObj ($K),MAY Reimb DA CIV PAY ($K),MAY Direct DA CIV PAY ($K),MAY Contracts ($K),MAY AllObj ($K),JUN Reimb DA CIV PAY ($K),JUN Direct DA CIV PAY ($K),JUN Contracts ($K),JUN AllObj ($K),JUL Reimb DA CIV PAY ($K),JUL Direct DA CIV PAY ($K),JUL Contracts ($K),JUL AllObj ($K),AUG Reimb DA CIV PAY ($K),AUG Direct DA CIV PAY ($K),AUG Contracts ($K),AUG AllObj ($K),SEP Reimb DA CIV PAY ($K),SEP Direct DA CIV PAY ($K),SEP Contracts ($K),SEP AllObj ($K),ITEM_TYPE,PATH"

			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
			'Loop through list of base entity members, and loop through 12 months per entity member to get the databuffer data cells then write to fetchDataTable

			' DataUnit coordinates: cubeId, entityId, parentId (use empty string if not needed), consId, scenarioId, timeId
				For i As Integer = 1 To 12 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntityGeneral ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTime & "M" & i))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "Top")	

					' Get & Loop through different U6#ABO_XX members and write
'					Dim oU1 As MemberInfo = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN"), "U1#OPA", True,,)(0)
'					Dim oU3List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U3_SAG"), "U3#APPN.Base", True,,)

'					For Each oU3 As MemberInfo In oU3List
'						myDbCellPk.UD3Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD3, oU3.Member.Name)
'						For Each oU6 As MemberInfo In oU6List
							myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, sTemplate)
'							' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntityGeneral,sScenario,sTime & "M" & i,myCells)
'						Next
'					Next					
				Next	
					
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns)	
		
			Me.ProcessTableForReport(si, FetchDt, processDT, sAmountType)
			
			processDT.DefaultView.Sort = "APPN,SSN,MDEP"
			processDT = processDT.DefaultView.ToTable()
				
			'Initialize file 
			
#Region "StringBuilders"
			Dim file As New Text.StringBuilder
			file.Append(sFileHeader)	
#End Region
			
			'Populate file
			For Each row As DataRow In processDT.Rows
				Dim rowInfo As String = ""
				For Each column As DataColumn In processDT.Columns
					rowInfo = rowInfo & "," & row(Column)				
				Next
				rowInfo = rowInfo.Remove(0,1)
				rowInfo = rowInfo.Replace("None","")
				file.Append(vbCrLf + rowInfo)
			Next
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","")
			If datetime.Now.Month < 10 Then sTimeStamp = "0" & sTimeStamp			
		    Dim fileName As String = ""

			fileName = $"{sCube}_{sTime}_OPA_Export_{sUser}_{sTimeStamp}.csv"
			
			Me.BuildFile(si, file.ToString, fileName, sCube)
			
			Dim dKeyVal As New Dictionary(Of String,String) From {{sFvParam,sFilePath}}
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"
			
			Return selectionChangedTaskResult

		End Function
	
#End Region

#Region "Export  RDTE Report"	
'Export Obligation or Commitment data to be used to import back into ARMY's system

		Public Function ExportRDTEReport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)		
'' for grabbing current year and past 2 years for template headers
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey) ' for grabbing current year for template headers
			Dim year As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim FY1 As Integer = Convert.ToInt32(year)
			Dim FY2 As Integer = FY1 - 1 
			Dim FY3 = year - 2	
			Dim sFY1 = FY1.ToString()
			sFY1 = sFY1.Substring(sFY1.Length - 2)
			Dim sFY2 = FY2.ToString()
			sFY2 = sFY2.Substring(sFY2.Length - 2)
			Dim sFY3 = FY3.ToString()
			sFY3 = sFY3.Substring(sFY3.Length - 2)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("APPN")		
			Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			Dim sCube As String = wfProfile.CubeName
			
			'CMD Entity
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0).Trim
			Dim sEntityGeneral As String = sEntity & "_General"
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim sTime As String = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sAmountType As String = args.NameValuePairs.XFGetValue("AmountType","")
			Dim sAccount As String = "Obligations"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
	
			If String.IsNullOrWhiteSpace(sTemplate) 
				Throw New Exception("Please select an appropriation.")
			End If	

			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			'Export selection Process
			columns.AddRange(New String(){"ENTITY","U1","U2","U3","U4","U6","U8","TIME","AMOUNT"})
			processColumns.AddRange(New String(){
			"Tracking Number:Tracking Number",
			"APPN",
			"PB LINE #",
			"Dollar_Type",
			"BA",
			"OSD PE",
			"Parent SSN/APE",
			"Baby SSN/RDTE Project",
			"Parent SSN/APE Nomen",
			"Baby SSN/Project Nomen",
			"CMD",
			"Funds Center",
			"MDEP",
			"PEG",
			"FY" & sFY2 & " ENACTED",
			"FY" & sFY3 & " RECISSIONS",
			"PENDING FY" & sFY2 & " RECISSIONS",
			"FY" & sFY1 & " PB REQUEST",
			"New Start Y/N",
			"HASC MARKS",
			"HASC ADDS",
			"HASC Adj",
			"SASC MARKS",
			"SASC ADDS",
			"SASC Adj",
			"FY" & sFY3 & " NDAA",
			"FY" & sFY3 & " NDAA Adj",
			"HAC-D MARKS",
			"HAC-D ADDS",
			"HAC-D Adj",
			"SAC-D MARKS",
			"SAC-D ADDS",
			"SAC-D Adj",
			"FY" & sFY3 & " FY25 APPROP CONF ADDS (+)",
			"FY" & sFY3 & " FY25 APPROP CONF MARKS (-)",
			"CONG LANG",
			"FY" & sFY1 & " APPROP",
			"MAX CR AUTH",
			"CRA #1 Distribution",
			"CRA #2 Distribution",
			"CRA #3 Distribution",
			"Total CRA Distribution",
			"SBIR",
			"STTR",
			"SUM SBIR_STTR",
			"30-Day APPN#1 (APR)",
			"30-Day APPN#2 (MAY)",
			"Balance Remaining",
			"CR SPEND PLAN REQUIRED? Y/N " ,
			"ABO COMMENTS",
			"FUNDS ISSUED IN (GFEBS or PBAS)",
			"HQDA Comments",
			"FIELD COMMENTS",
			"Field POC",
			"OCT Direct DA CIV PAY ($K)",
			"OCT Reimb DA CIV PAY ($K)",
			"OCT Contracts ($K)",
			"OCT AllObj ($K)",
			"NOV Direct DA CIV PAY ($K)",
			"NOV Reimb DA CIV PAY ($K)",
			"NOV Contracts ($K)",
			"NOV AllObj ($K)",
			"DEC Direct DA CIV PAY ($K)",
			"DEC Reimb DA CIV PAY ($K)",
			"DEC Contracts ($K)",
			"DEC AllObj ($K)",
			"JAN Direct DA CIV PAY ($K)",
			"JAN Reimb DA CIV PAY ($K)",
			"JAN Contracts ($K)",
			"JAN AllObj ($K)",
			"FEB Direct DA CIV PAY ($K)",
			"FEB Reimb DA CIV PAY ($K)",
			"FEB Contracts ($K)",
			"FEB AllObj ($K)",
			"MAR Direct DA CIV PAY ($K)",
			"MAR Reimb DA CIV PAY ($K)",
			"MAR Contracts ($K)",
			"MAR AllObj ($K)",
			"APR Reimb DA CIV PAY ($K)",
			"APR Direct DA CIV PAY ($K)",
			"APR Contracts ($K)",
			"APR AllObj ($K)",
			"MAY Reimb DA CIV PAY ($K)",
			"MAY Direct DA CIV PAY ($K)",
			"MAY Contracts ($K)",
			"MAY AllObj ($K)",
			"JUN Reimb DA CIV PAY ($K)",
			"JUN Direct DA CIV PAY ($K)",
			"JUN Contracts ($K)",
			"JUN AllObj ($K)",
			"JUL Reimb DA CIV PAY ($K)",
			"JUL Direct DA CIV PAY ($K)",
			"JUL Contracts ($K)",
			"JUL AllObj ($K)",
			"AUG Reimb DA CIV PAY ($K)",
			"AUG Direct DA CIV PAY ($K)",
			"AUG Contracts ($K)",
			"AUG AllObj ($K)",
			"SEP Reimb DA CIV PAY ($K)",
			"SEP Direct DA CIV PAY ($K)",
			"SEP Contracts ($K)",
			"SEP AllObj ($K)",
			"Created",
			"Created By",
			"Modified By",
			"Modified",
			"Item Type",
			"Path"
			})
			
			sFileHeader = 
			"Tracking Number:Tracking Number,APPN,PB LINE #,Dollar_Type,BA,OSD PE,Parent SSN/APE,Baby SSN/RDTE Project,Parent SSN/APE Nomen,Baby SSN/Project Nomen,CMD,Funds Center,MDEP,PEG,FY" & sFY2 & " ENACTED,FY" & sFY3 & " RECISSIONS,PENDING FY" & sFY2 & " RECISSIONS,FY" & sFY1 & " PB REQUEST,New Start Y/N,HASC MARKS,HASC ADDS,HASC Adj,SASC MARKS,SASC ADDS,SASC Adj,FY" & sFY3 & " NDAA,FY" & sFY3 & " NDAA Adj,HAC-D MARKS,HAC-D ADDS,HAC-D Adj,SAC-D MARKS,SAC-D ADDS,SAC-D Adj,FY" & sFY3 & " FY25 APPROP CONF ADDS (+),FY" & sFY3 & " FY25 APPROP CONF MARKS (-),CONG LANG,FY" & sFY1 & " APPROP,MAX CR AUTH,CRA #1 Distribution,CRA #2 Distribution,CRA #3 Distribution,Total CRA Distribution,SBIR,STTR,SUM SBIR_STTR,330-Day APPN#1 (APR),30-Day APPN#2 (MAY),Balance Remaining,CR SPEND PLAN REQUIRED? Y/N  ,ABO COMMENTS,FUNDS ISSUED IN (GFEBS or PBAS),HQDA Comments,FIELD COMMENTS,Field POC,OCT Direct DA CIV PAY ($K),OCT Reimb DA CIV PAY ($K),OCT Contracts ($K),OCT AllObj ($K),NOV Direct DA CIV PAY ($K),NOV Reimb DA CIV PAY ($K),NOV Contracts ($K),NOV AllObj ($K),DEC Direct DA CIV PAY ($K),DEC Reimb DA CIV PAY ($K),DEC Contracts ($K),DEC AllObj ($K),JAN Direct DA CIV PAY ($K),JAN Reimb DA CIV PAY ($K),JAN Contracts ($K),JAN AllObj ($K),FEB Direct DA CIV PAY ($K),FEB Reimb DA CIV PAY ($K),FEB Contracts ($K),FEB AllObj ($K),MAR Direct DA CIV PAY ($K),MAR Reimb DA CIV PAY ($K),MAR Contracts ($K),MAR AllObj ($K),APR Reimb DA CIV PAY ($K),APR Direct DA CIV PAY ($K),APR Contracts ($K),APR AllObj ($K),MAY Reimb DA CIV PAY ($K),MAY Direct DA CIV PAY ($K),MAY Contracts ($K),MAY AllObj ($K),JUN Reimb DA CIV PAY ($K),JUN Direct DA CIV PAY ($K),JUN Contracts ($K),JUN AllObj ($K),JUL Reimb DA CIV PAY ($K),JUL Direct DA CIV PAY ($K),JUL Contracts ($K),JUL AllObj ($K),AUG Reimb DA CIV PAY ($K),AUG Direct DA CIV PAY ($K),AUG Contracts ($K),AUG AllObj ($K),SEP Reimb DA CIV PAY ($K),SEP Direct DA CIV PAY ($K),SEP Contracts ($K),SEP AllObj ($K),Created,Created By,Modified By,Modified,Item Type,Path"
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
			'Loop through list of base entity members, and loop through 12 months per entity member to get the databuffer data cells then write to fetchDataTable

			' DataUnit coordinates: cubeId, entityId, parentId (use empty string if not needed), consId, scenarioId, timeId
				For i As Integer = 1 To 12 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntityGeneral ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sTime & "M" & i))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "Top")	

					' Get & Loop through different U6#ABO_XX members and write
'					Dim oU1 As MemberInfo = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN"), "U1#OPA", True,,)(0)
'					Dim oU3List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U3_SAG"), "U3#APPN.Base", True,,)

'					For Each oU3 As MemberInfo In oU3List
'						myDbCellPk.UD3Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD3, oU3.Member.Name)
'						For Each oU6 As MemberInfo In oU6List
							myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, sTemplate)
'							' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntityGeneral,sScenario,sTime & "M" & i,myCells)
'						Next
'					Next					
				Next	
					
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns)	
		
			Me.ProcessTableForReport(si, FetchDt, processDT, sAmountType)
			
			processDT.DefaultView.Sort = "APPN,Parent SSN/APE,MDEP"
			processDT = processDT.DefaultView.ToTable()
				
			'Initialize file 
			
#Region "StringBuilders"
			Dim file As New Text.StringBuilder
			file.Append(sFileHeader)	
#End Region
			
			'Populate file
			For Each row As DataRow In processDT.Rows
				Dim rowInfo As String = ""
				For Each column As DataColumn In processDT.Columns
					rowInfo = rowInfo & "," & row(Column)				
				Next
				rowInfo = rowInfo.Remove(0,1)
				rowInfo = rowInfo.Replace("None","")
				file.Append(vbCrLf + rowInfo)
			Next
			Dim sUser As String = si.UserName
			Dim sTimeStamp As String = datetime.Now.ToString.Replace("/","").Replace(":","")
			If datetime.Now.Month < 10 Then sTimeStamp = "0" & sTimeStamp			
		    Dim fileName As String = ""

			fileName = $"{sCube}_{sTime}_RDTE_Export_{sUser}_{sTimeStamp}.csv"
			
			Me.BuildFile(si, file.ToString, fileName, sCube)
			
			Dim dKeyVal As New Dictionary(Of String,String) From {{sFvParam,sFilePath}}
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationType = XFSelectionChangedNavigationType.OpenFile
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo.SelectionChangedNavigationArgs = $"FileSourceType=Application, UrlOrFullFileName=[{sFilePath}], OpenInXFPageIfPossible=False, PinNavPane=True, PinPOVPane=False"
			
			Return selectionChangedTaskResult

		End Function
	
#End Region

#Region "ExportReport Helper"
		Private Function CreateReportDataTable(ByVal si As SessionInfo, ByVal dataTableName As String, ByVal columns As List(Of String)) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				For Each column As String In columns	
					Dim objCol = New DataColumn
		            objCol.ColumnName = column
					If column.XFContainsIgnoreCase("amount") Then
						objCol.DataType = GetType(Long)
						objCol.DefaultValue = 0
					Else
		           		objCol.DataType = GetType(String)
						objCol.DefaultValue = ""
					End If
		            
		            objCol.AllowDBNull = False
		            dt.Columns.Add(objCol)
				Next
								
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub WriteFetchTable(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String, ByVal oDataCells As List(Of Datacell))
			Try
				For Each oDataCell As DataCell In oDataCells
	            'Create a new row and append it to the table
					Dim row As DataRow = dt.NewRow()
					Dim account As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Account, oDataCell.DataCellPk.AccountId)
					Dim flow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, oDataCell.DataCellPk.FlowId)
					Dim u1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, oDataCell.DataCellPk.UD1Id)
					Dim u2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, oDataCell.DataCellPk.UD2Id)
					Dim u3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, oDataCell.DataCellPk.UD3Id)
					Dim u4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, oDataCell.DataCellPk.UD4Id)
					Dim u5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, oDataCell.DataCellPk.UD5Id)
					Dim u6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, oDataCell.DataCellPk.UD6Id)
					Dim u7 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD7, oDataCell.DataCellPk.UD7Id)
					Dim u8 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD8, oDataCell.DataCellPk.UD8Id)
					
					'Get List of Column Names
					Dim columnNames As List(Of String) = dt.Columns.Cast(Of DataColumn)().Select(Function(col) col.ColumnName).ToList()
					'Assign values to row by column names
					If columnNames.Contains("ENTITY") Then row("ENTITY") = Entity
					If columnNames.Contains("SCENARIO") Then row("SCENARIO") = Scenario
					If columnNames.Contains("TIME") Then row("TIME") = Time
					If columnNames.Contains("FLOW") Then row("FLOW") = flow
					If columnNames.Contains("ACCOUNT") Then row("ACCOUNT") = account
					If columnNames.Contains("U1") Then row("U1") = u1
					If columnNames.Contains("U2") Then row("U2") = u2
					If columnNames.Contains("U3") Then row("U3") = u3
					If columnNames.Contains("U4") Then row("U4") = u4
					If columnNames.Contains("U5") Then row("U5") = u5
					If columnNames.Contains("U6") Then row("U6") = u6
					If columnNames.Contains("U7") Then row("U7") = u7
					If columnNames.Contains("U8") Then row("U8") = u8
					If columnNames.Contains("AMOUNT") Then row("AMOUNT") = oDataCell.CellAmount
                	dt.Rows.Add(row)
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub		
		
		Private Sub ProcessTableForReport(ByVal si As SessionInfo, ByVal FetchDt As DataTable, processDT As DataTable, Optional ByVal amountType As String = "") 
			Try		
				#Region "OMA"
				If FetchDt.TableName.XFContainsIgnoreCase("OMA") Then
					Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
					Dim sCube As String = wfProfile.CubeName
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String),Long())
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim U1 As String = Row("U1")
						Dim U1ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id, U1)
						Dim U1Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), U1ID, False)(0).Name
						
						If U1Parent <> "OMA" Then
							Continue For
						End If

						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3").split("_")(2)
						'Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
						
						'use Entity,U1,U3,U6 combination as Key
						Dim key As Tuple(Of String, String, String, String) = Tuple.Create(Entity,U1,U3,U6)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(11){}
						End If
						
						'group the amounts into an array of Long where M1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim Month As String = Time.Substring(5)						
						groupedData(key)(Month.XFConvertToInt -1) = groupedData(key)(Month.XFConvertToInt -1) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String),Long()) In groupedData
						Dim Entity As String = kvp.Key.Item1
						Dim U1 As String = kvp.Key.Item2
						Dim U3 As String = kvp.Key.Item3
						Dim U6 As String = kvp.Key.Item4
						Dim U6Desc As String = BRApi.Finance.Members.GetMember(si, DimType.UD6.Id, U6).Description
							
						'Get amount-by-month array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						newRow("Command")= sCube
						newRow("Fund")= U1
						newRow("SAG")= U3
						newRow("Commodity")= U6Desc	
						newRow("October")= Amount(0)
						newRow("November")= newRow("October") + Amount(1)
						newRow("December")= newRow("November") + Amount(2)
						newRow("January")= newRow("December") + Amount(3)
						newRow("February")= newRow("January") + Amount(4)
						newRow("March")= newRow("February") + Amount(5)
						newRow("April")= newRow("March") + Amount(6)
						newRow("May")= newRow("April") + Amount(7)
						newRow("June")= newRow("May") + Amount(8)
						newRow("July")= newRow("June") + Amount(9)
						newRow("August")= newRow("July") + Amount(10)
						newRow("September")= newRow("August") + Amount(11)
					Next					
				End If
				#End Region

				#Region "OPA"
				If FetchDt.TableName.XFContainsIgnoreCase("OPA") Then
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String,String,String,String),Long())						
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U3ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD3.Id, U3)
						Dim U3Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT"), U3ID, False)(0).Name		
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")
						Dim U8 As String = Row("U8")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
						
						'use Entity,U1,U2,U3,U4,U6,U8 combination as Key
						Dim key As Tuple(Of String, String, String, String,String,String,String) = Tuple.Create(Entity,U1,U2,U3Parent,U4,U6,U8)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(11){}
						End If
						
						'group the amounts into an array of Long where M1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim Month As String = Time.Substring(5)						
						groupedData(key)(Month.XFConvertToInt -1) = groupedData(key)(Month.XFConvertToInt -1) + Amount		
					Next
					
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String,String,String,String),Long()) In groupedData
						Dim Entity As String = kvp.Key.Item1
						Dim U1 As String = kvp.Key.Item2
						Dim U2 As String = kvp.Key.Item3
						Dim U3 As String = kvp.Key.Item4
						Dim APE As String = U3.Split("_")(1)
						Dim U4 As String = kvp.Key.Item5
						Dim U6 As String = kvp.Key.Item6
						Dim U8 As String = kvp.Key.Item7
						Dim U6Desc As String = BRApi.Finance.Members.GetMember(si, DimType.UD6.Id, U6).Description
						
						'Get amount-by-month array'
						Dim Amount As Long() = kvp.Value
							'Using LINQ to get row with APPN, MDEP, SSN, and DOLLAR_TYPE as key from the DataTable
							Dim resultRow As DataRow = processDT.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("APPN") = U1 _
														AndAlso row.Field(Of String)("MDEP") = U2 _
														AndAlso row.Field(Of String)("SSN") = APE _
														AndAlso row.Field(Of String)("DOLLAR_TYPE") = U4)
							'Assign values
							If resultRow Is Nothing Then 'Add new row						
								Dim U3ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD3.Id, U3)
								Dim U3Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U3_APE"), U3ID, False)(0).Name
								Dim U3Desc As String = BRApi.Finance.Members.GetMember(si, DimType.UD3.Id, U3).Description
								Dim U3BA = U3Parent.Split("_")(1)							
								Dim U2ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD2.Id, U2)
								Dim U2Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), U2ID, False)(0).Name
								
								
							
								Dim sRollSSN As String = BRapi.Finance.UD.Text(si, DimTypeID.UD3,U3ID, 2, Nothing, Nothing).Replace("ROLL_SSN=","")
	

 
								
								'Write to processed DataTable
								Dim newRow As DataRow = processDT.Rows.Add()
									newRow("APPN")= U1
									newRow("BA")= U3Ba
									newRow("SSN")= APE
									newRow("SSN_DESC")= """" & U3Desc & """"
									newRow("MDEP")= U2
									newRow("DOLLAR_TYPE")= U4
									newRow("PEG")= U2Parent
									NewRow("ROLL_SSN")= sRollSSN
							
								Select Case True
									Case U6 = "ABO_01" And U8 = "None"
										newRow("OCT Direct DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Direct DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Direct DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Direct DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Direct DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Direct DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Direct DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Direct DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Direct DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Direct DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Direct DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Direct DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_01" And U8 = "REIMS"
										newRow("OCT Reimb DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Reimb DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Reimb DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Reimb DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Reimb DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Reimb DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Reimb DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Reimb DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Reimb DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Reimb DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Reimb DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Reimb DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_04"
										newRow("OCT Contracts ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Contracts ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Contracts ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Contracts ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Contracts ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Contracts ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Contracts ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Contracts ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Contracts ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Contracts ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Contracts ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Contracts ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ALLOBJ"
										newRow("OCT AllObj ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV AllObj ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC AllObj ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN AllObj ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB AllObj ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR AllObj ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR AllObj ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY AllObj ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN AllObj ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL AllObj ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG AllObj ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP AllObj ($K)")= Math.Round(Amount(11)/1000)
								End Select
								
							Else 'update row								
								Select Case True
									Case U6 = "ABO_01" And U8 = "None"
										resultRow("OCT Direct DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Direct DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Direct DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Direct DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Direct DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Direct DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Direct DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Direct DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Direct DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Direct DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Direct DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Direct DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_01" And U8 = "REIMS"
										resultRow("OCT Reimb DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Reimb DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Reimb DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Reimb DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Reimb DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Reimb DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Reimb DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Reimb DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Reimb DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Reimb DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Reimb DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Reimb DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_04"
										resultRow("OCT Contracts ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Contracts ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Contracts ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Contracts ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Contracts ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Contracts ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Contracts ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Contracts ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Contracts ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Contracts ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Contracts ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Contracts ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ALLOBJ"
										resultRow("OCT AllObj ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV AllObj ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC AllObj ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN AllObj ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB AllObj ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR AllObj ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR AllObj ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY AllObj ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN AllObj ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL AllObj ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG AllObj ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP AllObj ($K)")= Math.Round(Amount(11)/1000)
								End Select
					
							End If
					Next					
				End If
#End Region
				
				#Region "RDTE"
				If FetchDt.TableName.XFContainsIgnoreCase("RDTE") Then
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String,String,String,String),Long())						
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						'Dim U3ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD3.Id, U3)
						'Dim U3Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT"), U3ID, False)(0).Name		
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")
						Dim U8 As String = Row("U8")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
						
						'use Entity,U1,U2,U3,U4,U6,U8 combination as Key
						Dim key As Tuple(Of String, String, String, String,String,String,String) = Tuple.Create(Entity,U1,U2,U3,U4,U6,U8)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(11){}
						End If
						
						'group the amounts into an array of Long where M1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim Month As String = Time.Substring(5)						
						groupedData(key)(Month.XFConvertToInt -1) = groupedData(key)(Month.XFConvertToInt -1) + Amount		
					Next
					
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String,String,String,String),Long()) In groupedData
						Dim Entity As String = kvp.Key.Item1
						Dim U1 As String = kvp.Key.Item2
						Dim U2 As String = kvp.Key.Item3
						Dim U3 As String = kvp.Key.Item4
						Dim U3ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD3.Id, U3)
						Dim U3Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U3_APE_PT"), U3ID, False)(0).Name	
						Dim APE As String = U3Parent.Split("_")(1)
						Dim U4 As String = kvp.Key.Item5
						Dim U6 As String = kvp.Key.Item6
						Dim U8 As String = kvp.Key.Item7
						Dim U6Desc As String = BRApi.Finance.Members.GetMember(si, DimType.UD6.Id, U6).Description
						
						'Get amount-by-month array'
						Dim Amount As Long() = kvp.Value
							'Using LINQ to get row with APPN, MDEP, SSN, and DOLLAR_TYPE as key from the DataTable
							Dim resultRow As DataRow = processDT.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("APPN") = U1 _
														AndAlso row.Field(Of String)("MDEP") = U2 _
														AndAlso row.Field(Of String)("Parent SSN/APE") = APE _
														AndAlso row.Field(Of String)("Baby SSN/RDTE Project") = U3.Substring(U3.Length - 3) _
														AndAlso row.Field(Of String)("Dollar_Type") = U4)
							'Assign values
							If resultRow Is Nothing Then 'Add new row						
								Dim APEID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD3.Id, U3Parent)
								Dim U3SAG = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U3_APE"), APEID, False)(0).Name
								Dim APEDesc As String = BRApi.Finance.Members.GetMember(si, DimType.UD3.Id, U3Parent).Description
								Dim U3BA = U3SAG.Split("_")(1)						
								Dim APEPTDESC As String = BRApi.Finance.Members.GetMember(si, DimType.UD3.Id, U3).Description
								Dim U2ID As String = BRapi.Finance.Members.GetMemberId(si,dimtype.UD2.Id, U2)
								Dim U2Parent = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), U2ID, False)(0).Name
								
								'Write to processed DataTable
								Dim newRow As DataRow = processDT.Rows.Add()
									newRow("APPN")= U1
									newRow("BA")= U3Ba
									newRow("Parent SSN/APE")= APE
									newRow("Parent SSN/APE NOMEN")= """" & APEDesc &""""
									newRow("Baby SSN/RDTE Project")= U3.Substring(U3.Length - 3)
									newRow("Baby SSN/Project Nomen")= """" & APEPTDESC &""""
									newRow("MDEP")= U2
									newRow("DOLLAR_TYPE")= U4
									newRow("PEG")= U2Parent
	'Math.Round(Amount(1)/1000)						
								Select Case True
									Case U6 = "ABO_01" And U8 = "None"
										newRow("OCT Direct DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Direct DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Direct DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Direct DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Direct DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Direct DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Direct DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Direct DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Direct DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Direct DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Direct DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Direct DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_01" And U8 = "REIMS"
										newRow("OCT Reimb DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Reimb DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Reimb DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Reimb DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Reimb DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Reimb DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Reimb DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Reimb DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Reimb DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Reimb DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Reimb DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Reimb DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_04"
										newRow("OCT Contracts ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV Contracts ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC Contracts ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN Contracts ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB Contracts ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR Contracts ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR Contracts ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY Contracts ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN Contracts ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL Contracts ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG Contracts ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP Contracts ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ALLOBJ"
										newRow("OCT AllObj ($K)")= Math.Round(Amount(0)/1000)
										newRow("NOV AllObj ($K)")= Math.Round(Amount(1)/1000)
										newRow("DEC AllObj ($K)")= Math.Round(Amount(2)/1000)
										newRow("JAN AllObj ($K)")= Math.Round(Amount(3)/1000)
										newRow("FEB AllObj ($K)")= Math.Round(Amount(4)/1000)
										newRow("MAR AllObj ($K)")= Math.Round(Amount(5)/1000)
										newRow("APR AllObj ($K)")= Math.Round(Amount(6)/1000)
										newRow("MAY AllObj ($K)")= Math.Round(Amount(7)/1000)
										newRow("JUN AllObj ($K)")= Math.Round(Amount(8)/1000)
										newRow("JUL AllObj ($K)")= Math.Round(Amount(9)/1000)
										newRow("AUG AllObj ($K)")= Math.Round(Amount(10)/1000)
										newRow("SEP AllObj ($K)")= Math.Round(Amount(11)/1000)
								End Select
								
							Else 'update row								
								Select Case True
									Case U6 = "ABO_01" And U8 = "None"
										resultRow("OCT Direct DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Direct DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Direct DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Direct DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Direct DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Direct DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Direct DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Direct DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Direct DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Direct DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Direct DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Direct DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_01" And U8 = "REIMS"
										resultRow("OCT Reimb DA CIV PAY ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Reimb DA CIV PAY ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Reimb DA CIV PAY ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Reimb DA CIV PAY ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Reimb DA CIV PAY ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Reimb DA CIV PAY ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Reimb DA CIV PAY ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Reimb DA CIV PAY ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Reimb DA CIV PAY ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Reimb DA CIV PAY ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Reimb DA CIV PAY ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Reimb DA CIV PAY ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ABO_04"
										resultRow("OCT Contracts ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV Contracts ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC Contracts ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN Contracts ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB Contracts ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR Contracts ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR Contracts ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY Contracts ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN Contracts ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL Contracts ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG Contracts ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP Contracts ($K)")= Math.Round(Amount(11)/1000)
									Case U6 = "ALLOBJ"
										resultRow("OCT AllObj ($K)")= Math.Round(Amount(0)/1000)
										resultRow("NOV AllObj ($K)")= Math.Round(Amount(1)/1000)
										resultRow("DEC AllObj ($K)")= Math.Round(Amount(2)/1000)
										resultRow("JAN AllObj ($K)")= Math.Round(Amount(3)/1000)
										resultRow("FEB AllObj ($K)")= Math.Round(Amount(4)/1000)
										resultRow("MAR AllObj ($K)")= Math.Round(Amount(5)/1000)
										resultRow("APR AllObj ($K)")= Math.Round(Amount(6)/1000)
										resultRow("MAY AllObj ($K)")= Math.Round(Amount(7)/1000)
										resultRow("JUN AllObj ($K)")= Math.Round(Amount(8)/1000)
										resultRow("JUL AllObj ($K)")= Math.Round(Amount(9)/1000)
										resultRow("AUG AllObj ($K)")= Math.Round(Amount(10)/1000)
										resultRow("SEP AllObj ($K)")= Math.Round(Amount(11)/1000)
								End Select
					
							End If
					Next					
				End If
#End Region
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	

#Region "BuildFile"
		Private Sub BuildFile(ByVal si As SessionInfo, ByVal sFileContent As String, ByVal sFileName As String, ByVal sCommandName As String)
			Try
				'Pass text to bytes
				Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(sFileContent)
				
				'Define folder to hold file
				Dim sFolderPath As String = "Documents/Users/" & si.UserName
				Dim objXFFolderEx As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sFolderPath)

				'Check if folder doesn't exist
				'This should never happen because we created the folder manually
				'If objXFFolderEx Is Nothing Then
				'	Throw New XFUserMsgException(si, New Exception("Users/" & si.UserName.Replace(" ",String.Empty) & " folder does NOT exist"))
				'End If
				
				Dim objXFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, String.Concat(sFolderPath, "/", sFileName))
				Dim objXFFile As New XFFile(objXFFileInfo,String.Empty,fileBytes)

				'Load file
				BRApi.FileSystem.InsertOrUpdateFile(si, objXFFile)
				sFilePath = $"{sFolderPath}/{sFileName}"
				
				'Delete file
				'BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, String.Concat(sFolderPath, "/", sfileName))
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
#End Region
		
#End Region

#Region "CachePrompts: Cache Prompts"
			Public Function CachePrompts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
			Dim sCommand As String = args.NameValuePairs.XFGetValue("Command")
			Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
			
			If String.IsNullOrWhiteSpace(sCommand) Then
				Throw New Exception("Please select a Command")
				Return Nothing
			End If
			
			Dim lCommand As List(Of String) = StringHelper.SplitString(sCommand,",")

			If lCommand.Count > 1 Then
				Throw New Exception ("Please select one Command at a time.")
			End If
			
			If Not String.IsNullOrWhiteSpace(sCommand) Then
'BRAPI.ErrorLog.LogMessage(si, "Cached sCommand= " & sCommand)
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"SPNHQDAPrompts","Command",sCommand)
			End If
			
			If Not String.IsNullOrWhiteSpace(sAPPN) Then
'BRAPI.ErrorLog.LogMessage(si, "Cached sAPPN= " & sAPPN)
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"SPNHQDAPrompts","APPN",sAPPN)
			End If

Return Nothing
End Function


#End Region

#Region "Return Spend Plan"
Public Function ReturnSPLN(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
	Try
		'========== workflow vars ============================================================================================================ 
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
			Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)	
			Dim return_message As String = ""
			
			Dim sComment As String = args.NameValuePairs.XFGetValue("returnComment")
			
			'Return comment can't be blank
			If sComment = "" Then
				Throw New Exception ("You must enter a comment for Spend Plan returns.")
			End If
			
			Dim sEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"SPNHQDAPrompts","Command","")
'BRAPI.ErrorLog.LogMessage(si, "------!------- Current entity: " & sEntity)			
			Dim sAPPN As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"SPNHQDAPrompts","APPN","")
			Dim lAPPN As List(Of String) = StringHelper.SplitString(sAPPN,"_")
			sAPPN = lAPPN(4)
'BRAPI.ErrorLog.LogMessage(si, "Pased ReturnSPLN sAPPN = " & sAPPN)

			Dim objCommandDimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "E_ARMY")
			Dim Command As String = sEntity.Split("_")(0) 
			Dim lCommand As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, objCommandDimPK, $"E#{Command}.Ancestors.Where(Text1 = {Command})", True,,)
			Dim CommandName As String = lCommand(0).Member.Name

	
			Dim returnCommentMemberScript As String = "Cb#" & wfCube & ":E#" & sEntity & ":C#Local:S#" & wfScenarioName & ":T#" & wfTimeName & "M12:V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#" &sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'BRAPI.ErrorLog.LogMessage(si, "returnCommentMemberScript = " & returnCommentMemberScript)	
			Dim sReturnCommentVal As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, returnCommentMemberScript).DataCellEx.DataCellAnnotation
'BRAPI.ErrorLog.LogMessage(si, "sComment = " & sComment)
'BRAPI.ErrorLog.LogMessage(si, "sReturnCommentVal = " & sReturnCommentVal)	
	
				
				
			'Update the return comment
			Dim objListofScriptsReturn As New List(Of MemberScriptandValue)
			Dim objScriptSplnReturn As New MemberScriptAndValue
			objScriptSplnReturn.CubeName = wfCube
			objScriptSplnReturn.Script = returnCommentMemberScript
			objScriptSplnReturn.TextValue = sComment
			objScriptSplnReturn.IsNoData = False
			objListofScriptsReturn.Add(objScriptSplnReturn)					
			BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsReturn)
				
'BRAPI.ErrorLog.LogMessage(si, "sReturnCommentVal AFTER UPDATE= " & sReturnCommentVal)
				
			
			' Check if Spend Plan is completed/locked
			Dim sDataBufferValidationFlag As String =  "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & "M12:C#Local:V#Annotation:E#" & sEntity & ":A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#" & sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sSPLockedval As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sDataBufferValidationFlag).DataCellEx.DataCellAnnotation
'BRAPI.ErrorLog.LogMessage(si, "------!------- sSPLockedval= " & sSPLockedval)
'BRAPI.ErrorLog.LogMessage(si, "------!------- sDataBufferValidationFlag = " & sDataBufferValidationFlag)
			If sSPLockedval = "Yes" Then
			'Reset Completion flag to false
			
				Dim objListofScriptsvalidation As New List(Of MemberScriptandValue)
				Dim objScriptValvalidation As New MemberScriptAndValue
				objScriptValvalidation.CubeName = wfCube
				objScriptValvalidation.Script = sDataBufferValidationFlag
				objScriptValvalidation.TextValue = "No"
				objScriptValvalidation.IsNoData = False
				objListofScriptsvalidation.Add(objScriptValvalidation)
				
				Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsvalidation)
				
				args.NameValuePairs.XFSetValue("Entity",CommandName)
				args.NameValuePairs.XFSetValue("APPN",sAPPN)
				
			'	BR_BFRM_SolutionHelper.WorkflowRevertSPLN(si, globals, api, args)
				
				Dim Statusmsg As String = "The Spend Plan for " & CommandName & " - " & sAPPN & " has been returned. You can now close the return window."
	
		
				Throw New XFUserMsgException(si, New Exception(Statusmsg))
				
				End If 
'BRAPI.ErrorLog.LogMessage(si, "------!------- sSPLockedval after flag change = " & sSPLockedval)	
	
			
		Return Nothing
	
	Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
	
End Function

#End Region

#Region "Verify And Submit OMA"
Public Function VerifyandSubmitOMA(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
	Try
			
			'========== workflow vars ============================================================================================================ 
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeMonth As String = wfTimeName & "M12"
			Dim return_message As String = ""
			Dim return_message_warning As String = ""
			Dim dashAction As String ="Refresh"
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0).Trim
			'Dim sEntityGeneral As String = sEntity & "_General"
			Dim sACOM As String =  sEntity & "_General"
			' Check if workflow is completed/locked
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And _
				Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or _
				(BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked)
				return_message = "Notice: No updates are allowed. Workflow was marked ""Complete"""
				Throw New XFUserMsgException(si, New Exception(return_message))
			End If
		
			' Check if Spend Plan is completed/locked
			Dim sDataBufferValidationFlag As String =  "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeMonth & ":C#Local:V#Annotation:E#" & sACOM & ":A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#OMA:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sSPLockedval As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sDataBufferValidationFlag).DataCellEx.DataCellAnnotation
			If sSPLockedval = "Yes" Then
				return_message = "Notice: No updates are allowed. OMA Spend Plan has been validated and submitted already."
				Throw New XFUserMsgException(si, New Exception(return_message))
			
			End If 
			
			'========== get rows that have data ============================================================================================================ 
			Dim sDataBufferScript  As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & sACOM & ":C#Local:V#Periodic:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
			
			Dim lsFilteredDataRowList As List( Of MemberInfo) = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_SAG", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, DataBufferPOVScript=" & sDataBufferScript & ",U3=OMA.Base,U3Level=SAG)",False)
			
			'========== loop thru rows and validate ============================================================================================================ 
		Dim lsU1List As New List(Of String)
		Dim lsU3List As New List(Of String)
				
			For Each sMbrScript As Memberinfo In lsFilteredDataRowList
				'BRApi.ErrorLog.LogMessage(si, "MemberList " & sMbrScript.Member.Name)	
				Dim sU1 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
				Dim sU3 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
					
					
				If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
				If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
			
				'Get values needed for the validation
				
				'SP Values
				Dim oblScript As String ="Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim oblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,oblScript).DataCellEx.DataCell.CellAmount
				Dim cmtScript As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Commitments:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim cmtAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,cmtScript).DataCellEx.DataCell.CellAmount
				
				'BUD Values
				Dim BUDoblScript As String = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & wfCube & ":A#Obligations:C#Aggregated:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim BUDoblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,BUDoblScript).DataCellEx.DataCell.CellAmount
				Dim BUDcmtScript As String = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & wfCube & ":A#Commitments:C#Aggregated:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim BUDcmtAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,BUDcmtScript).DataCellEx.DataCell.CellAmount
				
				'DOD Plan Rate Validation Values
				Dim dod_jul_script, obl_mar_script, obl_jul_script As String
				Dim dod_mar_amt, dod_jul_amt, obl_mar_amt, obl_jul_amt As Int64    	          
				
				obl_mar_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M6:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_mar_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_mar_script).DataCellEx.DataCell.CellAmount
				
				obl_jul_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M10:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_jul_script).DataCellEx.DataCell.CellAmount
					
				'Always 50% in March
				dod_mar_amt = 50
				
				dod_jul_script = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName  & "M10:E#" & sACOM & ":A#DOD_Rate_Calc:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				dod_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,dod_jul_script).DataCellEx.DataCell.CellAmount
				
				'brapi.ErrorLog.LogMessage(si, "July rate" &  dod_jul_amt)
'BRAPI.ErrorLog.LogMessage(si, sU1 & " and " & sU3 & " submit check: obl_mar_amt: " & obl_mar_amt & "     |     " & "oblAmt: " & oblAmt)
			
				If  oblAmt <> cmtAmt Then
						If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: Spend Plan Obligation amount does not equal Splend Plan Commitment amount for the following funding line:"& vbCrLf  & vbCrLf & "FundCode: " & sU1 & "	SAG: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				End If
				If oblAmt <> BUDoblAmt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: Spend Plan Obligation amount does not equal Budget Obligation amount for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SAG: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				End If
				If cmtAmt <> BUDcmtAmt
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: Spend Plan Commitment amount does not equal Budget Commitment amount for the following funding line:"& vbCrLf  & vbCrLf & "FundCode: " & sU1 & "	SAG: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				End If
				If Int(100 * (obl_mar_amt / oblAmt)) < dod_mar_amt Then
					If Not String.IsNullOrWhiteSpace(return_message_warning) Then
									return_message_warning += vbCrLf
					End If
					return_message_warning += vbCrLf & "WARNING: March DOD spending rate of 50% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SAG: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				End If
				If Int(100 * (obl_jul_amt / oblAmt)) < dod_jul_amt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: July DOD spending rate of " & dod_jul_amt & "% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SAG: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				Else
				Continue For
				End If
				
			Next
			If Not String.IsNullOrWhiteSpace(return_message) Then
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = return_message & vbCrLf & return_message_warning
		 
				
				Return selectionChangedTaskResult
			End If	
				
		'Email users 
		
'		Try
'			SPLN_SolutionHelper.EmailSubmitandVerifySpendPlan(si,globals,api,args)		
'				Catch ex As Exception
'				End Try
			
			
		'Set Completion flag	
				
					Dim objListofScriptsvalidation As New List(Of MemberScriptandValue)
					Dim objScriptValvalidation As New MemberScriptAndValue
					objScriptValvalidation.CubeName = wfCube
					objScriptValvalidation.Script = sDataBufferValidationFlag
					objScriptValvalidation.TextValue = "Yes"
					objScriptValvalidation.IsNoData = False
					objListofScriptsvalidation.Add(objScriptValvalidation)
					
					
					Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsvalidation)
	' Reset return cmt if not blank						
			Dim sMemberScriptReturncmt As String = "Cb#" & wfCube & ":E#" & sACOM & ":C#Local:S#" & wfScenarioName & ":T#" & wfTimeMonth & ":V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#OMA:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim SPLNAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMemberScriptReturncmt)
			Dim SPLNReturnCmt As String = SPLNAnnotationCell.DataCellEx.DataCellAnnotation	
			Dim resetvalue As String = ""
			
			If Not String.IsNullOrWhiteSpace(SPLNReturnCmt) Then 
				Dim objListofScriptresetcmt As New List(Of MemberScriptandValue)
					Dim objScriptresetcmt As New MemberScriptAndValue
					objScriptresetcmt.CubeName = wfCube
					objScriptresetcmt.Script = sMemberScriptReturncmt
					objScriptresetcmt.TextValue = resetvalue
					objScriptresetcmt.IsNoData = False
					objListofScriptresetcmt.Add(objScriptresetcmt)	
					
					
				Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptresetcmt)
				
			End If 

			Dim dataMgmtSeq As String = "Consolidate_SpendPlan"     
			Dim params As New Dictionary(Of String, String) 
											
			BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
			
				Dim CompleteselectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim Currprofilename As String = curProfile.Name
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				
				Dim CompleteobjXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim CompleteobjXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				CompleteobjXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				CompleteobjXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = CompleteobjXFSelectionChangedUIActionType
				
				'BRApi.ErrorLog.LogMessage(si, "Profile" & Currprofilename)
					
					'Lock WF
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, Currprofilename, wfScenarioName,wfTimeName )
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
										
					
					CompleteselectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					CompleteselectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					CompleteselectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					CompleteselectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = CompleteobjXFSelectionChangedUIActionInfo
					CompleteselectionChangedTaskResult.ShowMessageBox = True	
					CompleteselectionChangedTaskResult.Message = "Spend Plan has been validated and submitted."	& vbCrLf & return_message_warning						
		 
				
				Return CompleteselectionChangedTaskResult
			
			
			
Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
	
End Function

#End Region

#Region "Verify and Submit OPA"
Public Function VerifyandSubmitOPA(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
	Try
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeMonth As String = wfTimeName & "M12"
			Dim return_message As String = ""
			Dim return_message_warning As String = ""
			Dim dashAction As String ="Refresh"
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0).Trim
			'Dim sEntityGeneral As String = sEntity & "_General"
			Dim sACOM As String =  sEntity & "_General"
			
			 'Check If workflow Is completed/locked
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And _
				Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or _
				(BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked)
				return_message = "Notice: No updates are allowed. Workflow was marked ""Complete"""
				Throw New XFUserMsgException(si, New Exception(return_message))
			End If
		
			' Check if Spend Plan is completed/locked
			Dim sDataBufferValidationFlag As String =  "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeMonth & ":C#Local:V#Annotation:E#" & sACOM & ":A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#OPA:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sSPLockedval As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sDataBufferValidationFlag).DataCellEx.DataCellAnnotation
			If sSPLockedval = "Yes" Then
				return_message = vbCrLf & "Notice: No updates are allowed. OPA Spend Plan has been validated and submitted already."
				Throw New XFUserMsgException(si, New Exception(return_message))
			
			End If 
		
			
			'========== get rows that have data ============================================================================================================ 
			Dim sDataBufferScript  As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & sACOM & ":C#Local:V#Periodic:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
			
			Dim lsFilteredDataRowList As List( Of MemberInfo) = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_APE", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, Account=Obligations,DataBufferPOVScript=" & sDataBufferScript & ",U1=OPA.Base,U3=OPA.Base,U3Level=APE)",False)
			
			
			'========== loop thru rows and validate ============================================================================================================ 
		Dim lsU1List As New List(Of String)
		Dim lsU3List As New List(Of String)
				
			For Each sMbrScript As Memberinfo In lsFilteredDataRowList
				'BRApi.ErrorLog.LogMessage(si, "MemberList " & sMbrScript.Member.Name)	
				Dim sU1 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
				Dim sU3 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
					
					
				If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
				If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
			
				'Get values needed for the validation
				
				'SP Values
				Dim oblScript As String ="Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim oblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,oblScript).DataCellEx.DataCell.CellAmount
				
				
				'BUD Values
				Dim BUDoblScript As String = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & wfCube & ":A#Obligations:C#Aggregated:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim BUDoblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,BUDoblScript).DataCellEx.DataCell.CellAmount
			
				'DOD Plan Rate Validation Values
				Dim dod_jul_script, obl_mar_script, obl_jul_script As String
				Dim dod_mar_amt, dod_jul_amt, obl_mar_amt, obl_jul_amt As Int64    	          
				
				obl_mar_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M6:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_mar_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_mar_script).DataCellEx.DataCell.CellAmount
				
				obl_jul_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M10:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_jul_script).DataCellEx.DataCell.CellAmount
					
				'Always 50% in March
				dod_mar_amt = 50
				
				dod_jul_script = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName  & "M10:E#" & sACOM & ":A#DOD_Rate_Calc:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				dod_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,dod_jul_script).DataCellEx.DataCell.CellAmount
				
				'brapi.ErrorLog.LogMessage(si, "July rate" &  dod_jul_amt)		
				
				If oblAmt <> BUDoblAmt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: Spend Plan Obligation amount does not equal Budget Obligation amount for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SSN: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				End If

				 If Int(100 * (obl_mar_amt / oblAmt)) < dod_mar_amt Then
					If Not String.IsNullOrWhiteSpace(return_message_warning) Then
									return_message_warning += vbCrLf
					End If
					return_message_warning += vbCrLf & "WARNING: March DOD spending rate of 50% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SSN: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				End If
			If Int(100 * (obl_jul_amt / oblAmt)) < dod_jul_amt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: July DOD spending rate of " & dod_jul_amt & "% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	SSN: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				
				End If
				
			Next
		If Not String.IsNullOrWhiteSpace(return_message) Then
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = return_message & vbCrLf & return_message_warning					
		 
				
				Return selectionChangedTaskResult
			End If	
				
			
		'Email users 
		
'		Try
'			SPLN_SolutionHelper.EmailSubmitandVerifySpendPlan(si,globals,api,args)		
'				Catch ex As Exception
'				End Try
		


	
		'Set Completion flag	
				
					Dim objListofScriptsvalidation As New List(Of MemberScriptandValue)
					Dim objScriptValvalidation As New MemberScriptAndValue
					objScriptValvalidation.CubeName = wfCube
					objScriptValvalidation.Script = sDataBufferValidationFlag
					objScriptValvalidation.TextValue = "Yes"
					objScriptValvalidation.IsNoData = False
					objListofScriptsvalidation.Add(objScriptValvalidation)
					
					
					Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsvalidation)
			
		' Reset return cmt if not blank						
			Dim sMemberScriptReturncmt As String = "Cb#" & wfCube & ":E#" & sACOM & ":C#Local:S#" & wfScenarioName & ":T#" & wfTimeMonth & ":V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#OPA:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim SPLNAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMemberScriptReturncmt)
			Dim SPLNReturnCmt As String = SPLNAnnotationCell.DataCellEx.DataCellAnnotation	
			Dim resetvalue As String = ""
			
			If Not String.IsNullOrWhiteSpace(SPLNReturnCmt) Then 
				Dim objListofScriptresetcmt As New List(Of MemberScriptandValue)
					Dim objScriptresetcmt As New MemberScriptAndValue
					objScriptresetcmt.CubeName = wfCube
					objScriptresetcmt.Script = sMemberScriptReturncmt
					objScriptresetcmt.TextValue = resetvalue
					objScriptresetcmt.IsNoData = False
					objListofScriptresetcmt.Add(objScriptresetcmt)	
					
					
				Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptresetcmt)
				
			End If 
					
							
		Dim dataMgmtSeq As String = "Consolidate_SpendPlan"     
			Dim params As New Dictionary(Of String, String) 
											
			BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
			
			
			Dim CompleteselectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim Currprofilename As String = curProfile.Name
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				
				Dim CompleteobjXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim CompleteobjXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				CompleteobjXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				CompleteobjXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = CompleteobjXFSelectionChangedUIActionType
				
				'BRApi.ErrorLog.LogMessage(si, "Profile" & Currprofilename)
					
					'Lock WF
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, Currprofilename, wfScenarioName,wfTimeName )
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
										
					
					CompleteselectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					CompleteselectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					CompleteselectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					CompleteselectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = CompleteobjXFSelectionChangedUIActionInfo
					CompleteselectionChangedTaskResult.ShowMessageBox = True	
					CompleteselectionChangedTaskResult.Message = "Spend Plan has been validated and submitted." & vbCrLf & return_message_warning							
		 
				
				Return CompleteselectionChangedTaskResult
	
				
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
#End Region	

#Region "Verify and Submit RDTE"
Public Function VerifyandSubmitRDTE(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
	Try
			'========== workflow vars ============================================================================================================ 
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim wfTimeMonth As String = wfTimeName & "M12"
			Dim return_message As String = ""
			Dim return_message_warning As String = ""
			Dim dashAction As String ="Refresh"
			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0).Trim
			'Dim sEntityGeneral As String = sEntity & "_General"
			Dim sACOM As String =  sEntity & "_General"
			
			' Check if workflow is completed/locked
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And _
				Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or _
				(BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked)
				return_message = "Notice: No updates are allowed. Workflow was marked ""Complete"""
				Throw New XFUserMsgException(si, New Exception(return_message))
			End If
		
			' Check if Spend Plan is completed/locked
			Dim sDataBufferValidationFlag As String =  "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeMonth & ":C#Local:V#Annotation:E#" & sACOM & ":A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#RDTE:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sSPLockedval As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sDataBufferValidationFlag).DataCellEx.DataCellAnnotation
			If sSPLockedval = "Yes" Then
				return_message = vbCrLf & "Notice: No updates are allowed. RDTE Spend Plan has been validated and submitted already."
				Throw New XFUserMsgException(si, New Exception(return_message))
			
			End If 
		
			
				'========== get rows that have data ============================================================================================================ 
			Dim sDataBufferScript  As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & sACOM & ":C#Local:V#Periodic:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
			
			Dim lsFilteredDataRowList As List( Of MemberInfo) = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_APE_PT", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, Account=Obligations,DataBufferPOVScript=" & sDataBufferScript & ",U1=RDTE.Base,U3=RDTE.Base,U3Level=APEPT)",False)
			
			'========== loop thru rows and validate ============================================================================================================ 
		Dim lsU1List As New List(Of String)
		Dim lsU3List As New List(Of String)
				
			For Each sMbrScript As Memberinfo In lsFilteredDataRowList
				'BRApi.ErrorLog.LogMessage(si, "MemberList " & sMbrScript.Member.Name)	
				Dim sU1 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
				Dim sU3 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
					
					
				If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
				If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
			
				'Get values needed for the validation
				
				'SP Values
				Dim oblScript As String ="Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim oblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,oblScript).DataCellEx.DataCell.CellAmount
				
				'BUD Values
				Dim BUDoblScript As String = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & wfCube & ":A#Obligations:C#Aggregated:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim BUDoblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,BUDoblScript).DataCellEx.DataCell.CellAmount
				
				'DOD Plan Rate Validation Values
				Dim dod_jul_script, obl_mar_script, obl_jul_script As String
				Dim dod_mar_amt, dod_jul_amt, obl_mar_amt, obl_jul_amt As Int64    	          
				
				obl_mar_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M6:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_mar_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_mar_script).DataCellEx.DataCell.CellAmount
				
				obl_jul_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M10:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_jul_script).DataCellEx.DataCell.CellAmount
					
				'Always 50% in March
				dod_mar_amt = 50
				
				dod_jul_script = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName  & "M10:E#" & sACOM & ":A#DOD_Rate_Calc:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				dod_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,dod_jul_script).DataCellEx.DataCell.CellAmount
				
				'brapi.ErrorLog.LogMessage(si, "July rate" &  dod_jul_amt)
			
			
				 If oblAmt <> BUDoblAmt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: Spend Plan Obligation amount does not equal Budget Obligation amount for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	APE: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				End If
			
				If Int(100 * (obl_mar_amt / oblAmt)) < dod_mar_amt Then
					If Not String.IsNullOrWhiteSpace(return_message_warning) Then
									return_message_warning += vbCrLf
					End If
					return_message_warning += vbCrLf & "WARNING: March DOD spending rate of 50% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	APE: " & sU3
					
				End If
				 If Int(100 * (obl_jul_amt / oblAmt)) < dod_jul_amt Then
					If Not String.IsNullOrWhiteSpace(return_message) Then
									return_message += vbCrLf
					End If
					return_message += vbCrLf & "Error: July DOD spending rate of " & dod_jul_amt & "% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	APE: " & sU3
					'Throw New XFUserMsgException(si, New Exception(return_message))
				
				
				End If
				
			Next
			
			If Not String.IsNullOrWhiteSpace(return_message) Then
				
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim objXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim objXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				
					selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = False
					selectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = objXFSelectionChangedUIActionInfo
					selectionChangedTaskResult.ShowMessageBox = True	
					selectionChangedTaskResult.Message = return_message	& vbCrLf & return_message_warning				
		 
				
				Return selectionChangedTaskResult
			End If	
		'Email users 
		
'		Try
'			SPLN_SolutionHelper.EmailSubmitandVerifySpendPlan(si,globals,api,args)		
'				Catch ex As Exception
'				End Try
		


	
		'Set Completion flag	
				
					Dim objListofScriptsvalidation As New List(Of MemberScriptandValue)
					Dim objScriptValvalidation As New MemberScriptAndValue
					objScriptValvalidation.CubeName = wfCube
					objScriptValvalidation.Script = sDataBufferValidationFlag
					objScriptValvalidation.TextValue = "Yes"
					objScriptValvalidation.IsNoData = False
					objListofScriptsvalidation.Add(objScriptValvalidation)
					
					
					Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsvalidation)
					
					
		' Reset return cmt if not blank						
			Dim sMemberScriptReturncmt As String = "Cb#" & wfCube & ":E#" & sACOM & ":C#Local:S#" & wfScenarioName & ":T#" & wfTimeMonth & ":V#Annotation:A#SPL_HQDA_Return_Cmt:F#None:O#BeforeAdj:I#None:U1#RDTE:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim SPLNAnnotationCell As DataCellInfoUsingMemberScript = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMemberScriptReturncmt)
			Dim SPLNReturnCmt As String = SPLNAnnotationCell.DataCellEx.DataCellAnnotation	
			Dim resetvalue As String = ""
			
			If Not String.IsNullOrWhiteSpace(SPLNReturnCmt) Then 
				Dim objListofScriptresetcmt As New List(Of MemberScriptandValue)
					Dim objScriptresetcmt As New MemberScriptAndValue
					objScriptresetcmt.CubeName = wfCube
					objScriptresetcmt.Script = sMemberScriptReturncmt
					objScriptresetcmt.TextValue = resetvalue
					objScriptresetcmt.IsNoData = False
					objListofScriptresetcmt.Add(objScriptresetcmt)	
					
					
				Brapi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptresetcmt)
				
			End If 
		
					
		Dim dataMgmtSeq As String = "Consolidate_SpendPlan"     
			Dim params As New Dictionary(Of String, String) 
											
			BRApi.Utilities.ExecuteDataMgmtSequence(si, dataMgmtSeq, params)
			
			
			
			
		Dim CompleteselectionChangedTaskResult As New XFSelectionChangedTaskResult()
				
				Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim Currprofilename As String = curProfile.Name
				Dim wfClusterPK As New WorkflowUnitClusterPk()
				
				'Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				'Added by EBurke 3-19 Get Primary Dashboard for refresh
				Dim currDashboard As Dashboard = args.PrimaryDashboard
				
				Dim CompleteobjXFSelectionChangedUIActionType As XFSelectionChangedUIActionType = [Enum].Parse(GetType(XFSelectionChangedUIActionType),dashAction) 
				Dim CompleteobjXFSelectionChangedUIActionInfo As New XFSelectionChangedUIActionInfo()
				CompleteobjXFSelectionChangedUIActionInfo.DashboardsToRedraw = currDashboard.Name
				CompleteobjXFSelectionChangedUIActionInfo.SelectionChangedUIActionType = CompleteobjXFSelectionChangedUIActionType
				
				'BRApi.ErrorLog.LogMessage(si, "Profile" & Currprofilename)
					
					'Lock WF
					wfClusterPK = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, Currprofilename, wfScenarioName,wfTimeName )
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPK)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage("", wfClusterDesc), "", "", Guid.Empty)				
					
										
					
					CompleteselectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
					CompleteselectionChangedTaskResult.IsOK = True		
					'Added/Updated by Eburke to show message box and refresh dashboard 
					CompleteselectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
					CompleteselectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = CompleteobjXFSelectionChangedUIActionInfo
					CompleteselectionChangedTaskResult.ShowMessageBox = True	
					CompleteselectionChangedTaskResult.Message = "Spend Plan has been validated and submitted."	& vbCrLf & return_message_warning						
		 
				
				Return CompleteselectionChangedTaskResult
		
				
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
#End Region

#Region "Spend Rate Obligation Warning"
Public Function SpendObligWarning(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
	Try
'BRAPI.ErrorLog.LogMessage(si, "marchObligWarning opens")
		'========== workflow vars ============================================================================================================ 
		Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
		Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		Dim wfTimeMonth As String = wfTimeName & "M12"
		Dim return_message_warning As String = ""
		Dim dashAction As String ="Refresh"
		Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0).Trim
		'Dim sEntityGeneral As String = sEntity & "_General"
		Dim sACOM As String =  sEntity & "_General"
		
		Dim APPN As String = args.NameValuePairs("APPN")
		Dim lAPPN As List(Of String) = StringHelper.SplitString(APPN,"_")
		APPN = lAPPN(4)
'BRAPI.ErrorLog.LogMessage(si, "APPN: " & APPN)
		
		'========== get rows that have data ============================================================================================================ 
		Dim sDataBufferScript  As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & sACOM & ":C#Local:V#Periodic:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
		
		Dim lsFilteredDataRowList As New List(Of MemberInfo)
		
		Select Case APPN
#Region	"OMA"  
		Case "OMA" 
'BRAPI.ErrorLog.LogMessage(si, "OMA Case opened")
			lsFilteredDataRowList = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_SAG", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, DataBufferPOVScript=" & sDataBufferScript & ",U3=OMA.Base,U3Level=SAG)",False)
#End Region	
#Region	"OPA"  
		Case "OPA" 
'BRAPI.ErrorLog.LogMessage(si, "OPA Case opened")
			lsFilteredDataRowList = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_APE", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, Account=Obligations,DataBufferPOVScript=" & sDataBufferScript & ",U1=OPA.Base,U3=OPA.Base,U3Level=APE)",False)
#End Region	
#Region	"RDTE"  
		Case "RDTE" 
'BRAPI.ErrorLog.LogMessage(si, "RDTE Case opened")
			lsFilteredDataRowList = Brapi.Finance.Metadata.GetMembersUsingFilter(si,"U3_APE_PT", "U3#Root.CustomMemberList(BRName=SPLN_Member_Lists, MemberListName=GetSPLAdjustments,CacheName=,Mode=Summary,DimDef=U1#U3#, Account=Obligations,DataBufferPOVScript=" & sDataBufferScript & ",U1=RDTE.Base,U3=RDTE.Base,U3Level=APEPT)",False)
#End Region			
		End Select
		'========== loop thru rows and validate ============================================================================================================ 
		Dim lsU1List As New List(Of String)
		Dim lsU3List As New List(Of String)
				
			For Each sMbrScript As Memberinfo In lsFilteredDataRowList
'BRApi.ErrorLog.LogMessage(si, "MemberList " & sMbrScript.Member.Name)	
				Dim sU1 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD1.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD1Id).Member.Name
				Dim sU3 As String = BRApi.Finance.Metadata.GetMember(si, dimType.UD3.Id, sMbrScript.RowOrColDataCellPkAndCalcScript.DataCellPk.UD3Id).Member.Name
					
					
				If Not lsU1List.Contains(sU1) Then lsU1List.Add(sU1)
				If Not lsU3List.Contains(sU3) Then lsU3List.Add(sU3)
			
				'Get values needed for the validation
				
				'DOD Plan Rate Validation Values
				'DOD Plan Rate Validation Values
				Dim obl_mar_script, dod_jul_script, obl_july_script As String
				Dim dod_mar_amt, obl_mar_amt, dod_jul_amt, obl_july_amt As Int64   
				'Always 50% in March
				dod_mar_amt = 50
				
				dod_jul_script = "Cb#" & wfCube & ":S#BUD_C" & wfTimeName & ":T#" & wfTimeName  & "M10:E#" & sACOM & ":A#DOD_Rate_Calc:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				dod_jul_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,dod_jul_script).DataCellEx.DataCell.CellAmount
				
						
				obl_mar_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M6:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_mar_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_mar_script).DataCellEx.DataCell.CellAmount
				
				obl_july_script = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName  & "M10:U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				obl_july_amt = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,obl_july_script).DataCellEx.DataCell.CellAmount
				
				
				
				'SP Values
				Dim oblScript As String ="Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":U1#" & sU1 & ":U3#" & sU3 & ":E#" & sACOM & ":A#Obligations:C#Local:V#YTD:I#Top:F#Top:O#BeforeAdj:U2#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#None"
				Dim oblAmt As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube,oblScript).DataCellEx.DataCell.CellAmount
				
'BRAPI.ErrorLog.LogMessage(si, sU1 & " and " & sU3 & " save check: obl_mar_amt: " & obl_mar_amt & "     |     " & "oblAmt: " & oblAmt)	

				If Int(100 * (obl_mar_amt / oblAmt)) < dod_mar_amt Then
					If Not String.IsNullOrWhiteSpace(return_message_warning) Then
									return_message_warning += vbCrLf
					End If
					return_message_warning += vbCrLf & "WARNING: March DOD spending rate of 50% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	APE: " & sU3
					
				End If
'brapi.ErrorLog.LogMessage(si,"obl_july_amt=" & obl_july_amt & "; oblAmt=" & oblAmt & ";  dod_jul_amt=" & dod_jul_amt)
				If Int(100 * (obl_july_amt / oblAmt)) < dod_jul_amt Then
					If Not String.IsNullOrWhiteSpace(return_message_warning) Then
									return_message_warning += vbCrLf
					End If
					return_message_warning += vbCrLf & "WARNING: July DOD spending rate of " & math.Round(dod_jul_amt) &  "% has not been met for the following funding line:" & vbCrLf & vbCrLf & "FundCode: " & sU1 & "	APE: " & sU3
					
				End If
			Next	
'BRApi.ErrorLog.LogMessage(si, "return_message_warning: " & return_message_warning)
				Dim selectionChangedTaskResult2 As New XFSelectionChangedTaskResult()
					selectionChangedTaskResult2.IsOK = True
					selectionChangedTaskResult2.ShowMessageBox = True
					selectionChangedTaskResult2.Message = return_message_warning
				Return selectionChangedTaskResult2
			
		Return Nothing
	
	Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
	
End Function

#End Region

#Region "Set Parameter"
		'Set a parameter with passed in value using selectionChangedTaskResult
		Public Function SetParameter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal dKeyVal As Dictionary(Of String, String), Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing)As Object			
				If selectionChangedTaskResult Is Nothing Then
					selectionChangedTaskResult = New XFSelectionChangedTaskResult()
				End If

				selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True			
				selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
				
				For Each KeyVal As KeyValuePair(Of String, String) In dKeyVal
					selectionChangedTaskResult.ModifiedCustomSubstVars.Add(KeyVal.Key, KeyVal.Value)
					selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add(KeyVal.Key, KeyVal.Value)
'BRApi.ErrorLog.LogMessage(si, $"KeyVal.Key = {KeyVal.Key} || KeyVal.Value = {KeyVal.Value} ")	
				Next
		
				Return selectionChangedTaskResult
		End Function
#End Region



	End Class
End Namespace