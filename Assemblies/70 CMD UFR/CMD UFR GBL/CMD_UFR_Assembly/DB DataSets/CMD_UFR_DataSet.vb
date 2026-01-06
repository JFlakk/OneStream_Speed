Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database
Imports Workspace.GBL.GBL_Assembly

'Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
'	Public Class CMD_UFR_DataSet
		
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_UFR_DataSet
	Public Class MainClass

		Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardDataSetArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args
				
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						Select Case args.DataSetName.ToLower()
							Case "reqtitlelist"
								Return Me.REQTitleList()
							Case "reqlistbyentityandstatus"
								Return Me.REQListByEntityAndStatus()
'							Case "getallusers"							
'								Return Me.GetAllUsers(si,globals,api,args)	
'							Case "get_copyreq_acctvalues"
'								Return Me.Get_CopyReq_AcctValues()	
'							Case "get_copyreq_list"
'								Return Me.Get_CopyReq_List()
'							Case "getuserfundscenterbywf"
'								Return Me.GetUserFundsCenterByWF(si, globals, api, args)
'							Case "getreqstatus"
'								Return Me.GetReqStatus()
'							Case "fullreqlist"
'								Return Me.FullREQList()
'							Case "getdemotestatuslist"
'								Return Me.GetDemoteStatusList()
'							Case "getpackagesforsubmission"
'								Return Me.GetPackagesForSubmission()
						End Select

#Region "GetFileNamesInFolder" 
'						'Return a list of files in a folder
'						If args.DataSetName.XFEqualsIgnoreCase("GetFileNamesInFolder") Then
'							Return Me.GetFileNamesInFolder(si,globals,api,args)	
'						End If						
#End Region 'Updated 09/23/2025

#Region "ManpowerPBList"
'						If args.DataSetName.XFEqualsIgnoreCase("ManpowerPBList") Then
'							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'					        'Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'							Dim sScenario As String = args.NameValuePairs.XFGetValue("scenario",si.WorkflowClusterPk.ScenarioKey)
'					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'							Dim sCurrYear As String = sREQTime.Substring(2,2)
'							Dim sPriorYear As String = sCurrYear - 1
'							Dim sPositions As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"S_RMW","S#HQDA_Programming.base.Where((Name Contains " & sCurrYear & ") or (Name Contains " & sPriorYear & "))" ,True)
''						
'							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "PositionList")
'							'Loop through the fetched datatable and write the result datatable to be returned
'							For Each sPosition As MemberInfo In sPositions 	
														
'									Me.WriteNameValuePairRow(si, dt, sPosition.Member.Name, sPosition.Member.Name)
								
'							Next
'							Return dt
'							End If
#End Region

#Region "Get Fund Centers By Command"
'						If args.DataSetName.XFEqualsIgnoreCase("GetFundCentersByCommand") Then
'							Return Me.GetFundCentersByCommand(si,globals,api,args)						
'						End If
#End Region

#Region "Get REQs to be Deleted"
'						If args.DataSetName.XFEqualsIgnoreCase("GetREQsToBeDeleted") Then
'							Return Me.GetREQsToBeDeleted(si,globals,api,args)						
'						End If
#End Region
				End Select

				Return Nothing
			Catch ex As Exception

				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "REQListByEntityAndStatus"
		Public Function REQListByEntityAndStatus() As Object
			Try		
				Dim dt As New Datatable()			
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
						
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				'--- Added ---
				Dim sDashboard As String = args.NameValuePairs.XFGetValue("Dashboard")
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Return dt
				End If
			
				Dim FCArray() As String = sFundCenter.Split(","c)
	
       		 ' Convert the array to a list
        		Dim FCList As List (Of String) = FCArray.ToList
			
				Dim FCMulti As New Text.StringBuilder()
				Dim uniqueStatuses As New HashSet(Of String)() ' Using a HashSet automatically handles duplicates

	        ' Loop through Each FC in multiselect
	       		For Each FC As String In FCList
					
					'If the fund center passed is not a descendent of the WF then stop
					FC = FC.Replace("_General","").Trim
					Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  FC & ")"
					Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
	'
					If Not cbMembers.Count > 0 Then
						Return dt
					End If
					
								
					'---Checking if Base------
	
					Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
						
	         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)
						
					Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FC.Replace("_General",""))	
					
					Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
					If Not isBase Then 
						'FCMulti.Append($"'{FC}'")
						FC = FC & ".DescendantsInclusive"
					End If
		
					Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#" & FC,True)
					For Each FundCenter As MemberInfo In LFundCenters
						If FCMulti.Length > 0 Then
							FCMulti.Append(",")
						End If
						FCMulti.Append($"'{FundCenter.Member.Name}'")
					Next
					'--------- get Entity Text3 --------- 
					Dim sFC As String = FC.Replace(".DescendantsInclusive","")
					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFC).Member
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sREQTime)
					Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
					entityText3 = entityText3.Substring(entityText3.Length -2, 2)
	
					'--------- get next workflow level --------- 
					Dim currText3Num As String = entityText3.Substring(1,1)
					Dim icurrText3Num As Integer = currText3Num
					'icurrText3Num = icurrText3Num - 1
					Dim newText3Num As String = icurrText3Num - 1
					Dim newWFLevel As String = "L" & newText3Num
					Dim prevWFLevel As String = "L" & (icurrText3Num + 1)
					Dim curWFLevel As String = entityText3
				
					'========================================================================== Define Targeted Status based on WF and Dashboard's FC selection ==========================================================================
					
					Select Case True
						Case sProfileName.XFContainsIgnoreCase("Manage UFR")		
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Staff_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_UFR'")
						
						Case sProfileName.XFContainsIgnoreCase("Manage CMD")		
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Staff_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Final_UFR'")
							
						Case sProfileName.XFContainsIgnoreCase("Formulate")
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_UFR'")
							
						Case sProfileName.XFContainsIgnoreCase("Rollover")
							sREQTime = (Convert.ToInt32(sREQTime) - 1).ToString
'							sScenario = $"CMD_UFR_C{sREQTime}"
							sScenario = $"CMD_SPLN_WORK"
							wfInfoDetails("ScenarioName") = sScenario
							wfInfoDetails("TimeName") = sREQTime
							uniqueStatuses.Add($"'L2_Final_UFR'")
						
						Case sProfileName.XFContainsIgnoreCase("Validate")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_UFR'")
						
						Case sProfileName.XFContainsIgnoreCase("Staff")
							uniqueStatuses.Add($"'{curWFLevel}_Staff_UFR'")
						
						Case sProfileName.XFContainsIgnoreCase("Prioritize")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_UFR'")
							
						Case sProfileName.XFContainsIgnoreCase("Approve") 
							uniqueStatuses.Add($"'{curWFLevel}_Approve_UFR'")
						' --- Added ---	
						Case sProfileName.XFContainsIgnoreCase("Review") 
							uniqueStatuses.Add($"'{curWFLevel}_Review_UFR'")
						
						Case wfProfileName.XFContainsIgnoreCase("(CMD UFR)")
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Staff_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_UFR'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_UFR'")	
							
							uniqueStatuses.Add($"'L2_Final_UFR'")
					
'					Case sProfileName.XFContainsIgnoreCase("CMD(PGM)")
'						uniqueStatuses.Add($"'L2_CMD_PGM_Final_Reqs'")
						
						
	
					End Select
				Next

				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allREQs As String = ""
				Dim allFCs As String = ""
				
					
				allFCs = FCMulti.ToString()
				Dim sTgtStatus As String = String.Join(",", uniqueStatuses)
'	Brapi.ErrorLog.LogMessage(si,"Status" & sTgtStatus)
'	Brapi.ErrorLog.LogMessage(si,"allFCs" & allFCs)
				'If after looping, we have no valid entities or statuses, return an empty table.
				If String.IsNullOrWhiteSpace(allFCs) Or String.IsNullOrWhiteSpace(sTgtStatus) Then
					Return dt
				End If
			
				
				
			
				Dim SQL As New Text.StringBuilder

'----------------------------------------
				Select Case sDashboard
					Case "UFRGridList"
						SQL.Append($"SELECT 
										Req.CMD_UFR_Tracking_No,
										Req.Title As [Title],
										Req.UFR_ID As [UFR ID],
										Req.ROC As [ROC],
										Req.Description As [Description],
										Req.Entity As [Funds Center],
										Req.Study_Category As [Study Category],
										Req.Command_UFR_Priority,
										Req.UFR_Driver,
										Req.PEG,
										Req.Review_Staff As [Review Staff],
										Req.MustFund As [Must Fund],
										Req.Solicited_NonSolicited As [Solicited or NonSolicited],
										Req.REQ_Link_ID As [REQ Link ID],
										Req.APPN,
										Req.SAG_SSN,
										Dtl.Flow,
										Dtl.Account,
										Dtl.UD1 as [Fund Code],
										Dtl.UD2 As [MDEP],
										Dtl.UD3 as [APE9],
										Dtl.UD4 as [DollarType],
										Dtl.UD6 As [Object Class],
										Dtl.UD5 As [cType],
										FORMAT(Dtl.FY, 'N0') As FY
									FROM XFC_CMD_UFR AS Req
									LEFT JOIN XFC_CMD_UFR_Details AS Dtl
										ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
										AND Req.WFScenario_Name = Dtl.WFScenario_Name
										AND Req.WFCMD_Name = Dtl.WFCMD_Name
										AND Req.WFTime_Name = Dtl.WFTime_Name
										AND Req.Entity = Dtl.Entity
									WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
										AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
										AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
										AND Dtl.Account = 'Req_Funding'
										AND Dtl.Unit_Of_Measure = 'Funding'
										AND Dtl.Flow IN ({sTgtStatus})
										AND Req.Entity IN ({allFCs})
									Order By Req.Entity,Req.CMD_UFR_Tracking_No
						")
						
					Case "SPLNUFRAmount"
						Brapi.ErrorLog.LogMessage(si, "inside case")
						SQL.Append($"SELECT 
										s.REQ_ID,
										s.Title As [SPLN Req ID Title],
										s.Description AS [SPLN Req ID Description],
										s.Entity,
										sd.UD1,
										sd.UD2,
										sd.UD3,
										sd.UD4,
										sd.UD5,
										sd.UD6,
										CASE
											WHEN sd.Account = 'UFR_Obligations' THEN COALESCE(sd.Month1,0)
															   + COALESCE(sd.Month2, 0)
															   + COALESCE(sd.Month3, 0)
															   + COALESCE(sd.Month4, 0) 
															   + COALESCE(sd.Month5, 0) 
															   + COALESCE(sd.Month6, 0) 
															   + COALESCE(sd.Month7, 0) 
															   + COALESCE(sd.Month8, 0)
															   + COALESCE(sd.Month9, 0)
															   + COALESCE(sd.Month10, 0)
															   + COALESCE(sd.Month11, 0)
															   + COALESCE(sd.Month12, 0)					   
										END AS [Unfunded Amount],
										CASE
											WHEN sd.Account = 'Obligations' THEN COALESCE(sd.Month1,0)
															   + COALESCE(sd.Month2, 0)
															   + COALESCE(sd.Month3, 0)
															   + COALESCE(sd.Month4, 0) 
															   + COALESCE(sd.Month5, 0) 
															   + COALESCE(sd.Month6, 0) 
															   + COALESCE(sd.Month7, 0) 
															   + COALESCE(sd.Month8, 0)
															   + COALESCE(sd.Month9, 0)
															   + COALESCE(sd.Month10, 0)
															   + COALESCE(sd.Month11, 0)
															   + COALESCE(sd.Month12, 0)					   
										END AS [Obligations],	
										CASE
											WHEN sd.Account = 'WH_Obligations' THEN COALESCE(sd.Month1,0)
															   + COALESCE(sd.Month2, 0)
															   + COALESCE(sd.Month3, 0)
															   + COALESCE(sd.Month4, 0) 
															   + COALESCE(sd.Month5, 0) 
															   + COALESCE(sd.Month6, 0) 
															   + COALESCE(sd.Month7, 0) 
															   + COALESCE(sd.Month8, 0)
															   + COALESCE(sd.Month9, 0)
															   + COALESCE(sd.Month10, 0)
															   + COALESCE(sd.Month11, 0)
															   + COALESCE(sd.Month12, 0)					   
										END AS [WH Obligations],						
						
										ud.FY AS [Requested Amount],
										u.UFR_ID,
										u.REQ_LINK_ID,
										u.Title As [UFR Title],
										u.MustFund
										
										FROM
										XFC_CMD_SPLN_REQ s
										INNER JOIN XFC_CMD_SPLN_REQ_Details sd
										ON s.CMD_SPLN_REQ_ID = sd.CMD_SPLN_REQ_ID
										LEFT JOIN XFC_CMD_UFR_Details ud
										ON sd.UD1 = ud.UD1
										AND sd.UD2 = ud.UD2
										AND sd.UD3 = ud.UD3
										AND sd.UD4 = ud.UD4
										AND sd.UD5 = ud.UD5
										AND sd.UD6 = ud.UD6
										AND sd.Entity = ud.Entity
										LEFT JOIN XFC_CMD_UFR u
										ON ud.CMD_UFR_Tracking_No = u.CMD_UFR_Tracking_No
										
										WHERE
											s.Entity IN ({allFCs})
													
										
						")
						
'					Case "StaffingNarrative"
'						SQL.Append($"SELECT 
'										Req.CMD_UFR_Tracking_No,
'										Req.Title As [Title],
'										Req.UFR_ID As [UFR ID],
'										Req.ROC As [ROC],
'										Req.PEG,
'										Req.Description As [Description],
'										Req.Entity As [Funds Center],
'										Req.Study_Category As [Study Category],
'										Req.Command_UFR_Priority As [Command UFR Priority],
'										Req.Review_Staff As [Review Staff],
'										Req.MustFund As [Must Fund],
'										Req.Solicited_NonSolicited As [Solicited or NonSolicited],
'										Req.REQ_Link_ID As [REQ Link ID],
'										Req.APPN,
'										Req.SAG_SSN,
'										Dtl.Flow,
'										Dtl.Account,
'										Dtl.UD1 as [Fund Code],
'										Dtl.UD2 As [MDEP],
'										Dtl.UD3 as [APE9],
'										Dtl.UD4 as [DollarType],
'										Dtl.UD6 As [Object Class],
'										Dtl.UD5 As [cType],
'										FORMAT(Dtl.FY, 'N0') As FY,
'										Staff.Level,
'										Staff.Review_Input,
'										Staff.Review_POC,
'										Staff.BRP_Candidate
'									FROM XFC_CMD_UFR AS Req
'									LEFT JOIN XFC_CMD_UFR_Details AS Dtl
'										ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
'										AND Req.WFScenario_Name = Dtl.WFScenario_Name
'										AND Req.WFCMD_Name = Dtl.WFCMD_Name
'										AND Req.WFTime_Name = Dtl.WFTime_Name
'										AND Req.Entity = Dtl.Entity
'									WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'										AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
'										AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
'										AND Dtl.Account = 'Req_Funding'
'										AND Dtl.Unit_Of_Measure = 'Funding'
'										AND Dtl.Flow IN ({sTgtStatus})
'										AND Req.Entity IN ({allFCs})
'									Order By Req.Entity,Req.CMD_UFR_Tracking_No
'						")
						
						
						
#Region "Possibly Delete"						
'					Case "UFRStatus"
'						SQL.Append($"CREATE VIEW vw_UFRStatus AS
'									SELECT 
'										Req.CMD_UFR_Tracking_No,
'										Req.Title As [Title],
'										Req.UFR_ID As [UFR ID],
'										Req.ROC As [ROC],
'										Req.Description As [Description],
'										Req.Entity As [Funds Center],
'										Req.Study_Category As [Study Category],
'										Req.Command_UFR_Priority As [Command UFR Priority],
'										Req.Review_Staff As [Review Staff],
'										Req.MustFund As [Must Fund],
'										Req.Solicited_NonSolicited As [Solicited or NonSolicited],
'										Req.REQ_Link_ID As [REQ Link ID],
'										Req.APPN,
'										--Req.SAG,
'										Dtl.Flow,
'										Dtl.Account,
'										Dtl.UD1 as [Fund Code],
'										Dtl.UD2 As [MDEP],
'										Dtl.UD3 as [APE9],
'										Dtl.UD4 as [DollarType],
'										Dtl.UD6 As [Object Class],
'										Dtl.UD5 As [cType],
'										FORMAT(Dtl.FY, 'N0') As FY
'									FROM XFC_CMD_UFR AS Req
'									LEFT JOIN XFC_CMD_UFR_Details AS Dtl
'										ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
'										AND Req.WFScenario_Name = Dtl.WFScenario_Name
'										AND Req.WFCMD_Name = Dtl.WFCMD_Name
'										AND Req.WFTime_Name = Dtl.WFTime_Name
'										AND Req.Entity = Dtl.Entity
'									WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
'										AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
'										AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
'										AND Dtl.Account = 'Req_Funding'
'										AND Dtl.Unit_Of_Measure = 'Funding'
'										AND Dtl.Flow IN ({sTgtStatus})
'										AND Req.Entity IN ({allFCs})
'									Order By Req.Entity,Req.CMD_UFR_Tracking_No
'						")
#End Region						
				End Select
				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntityAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)

					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						
				End Using
				Return dtAll 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
End Function
#End Region 


#Region "Helper Functions"
		Private Function CreateNameValuePairTable(ByVal si As SessionInfo, ByVal dataTableName As String) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				
				Dim objCol = New DataColumn
	            objCol.ColumnName = "Name"
	            objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)
				
				objCol = New DataColumn
				objCol.ColumnName = "Value"
				objCol.DataType = GetType(String)
	            objCol.DefaultValue = ""
	            objCol.AllowDBNull = False
	            dt.Columns.Add(objCol)
							
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub WriteNameValuePairRow(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal name As String, ByVal value As String)
			Try
	            'Create a new row and append it to the table
				Dim row As DataRow = dt.NewRow()

				row("Name") = name
				row("Value") = value

'Brapi.ErrorLog.LogMessage(si, $"Name = {Name} | Value = {Value}")				
                dt.Rows.Add(row)
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub		
#End Region

#Region "REQTitleList"
		Public Function REQTitleList() As DataTable
			Try	
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQTitleList")				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN","")
				'If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Return dt
				End If
				
				'If the fund center passed is not a descendent of the WF then stop
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter.Replace("_General","") & ")"
				Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
#Region "Delete"				
'				If Not cbMembers.Count > 0 Then
'					Return dt
'				End If

				'Remove _General to get the parent Entity
'				If Not sProfileName.XFContainsIgnoreCase("Formulate")
'					If sFundCenter.Contains("_General") Then 'added
'						sFundCenter = sFundCenter.Replace("_General",".Base") 'added 
'					End If 'added														
'				End If
#End Region 

				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundCenter,True)
		
				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				
				Dim allFCs As String = ""
				If LFundCenters.Count = 0 Then
					Return dt
				End If
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next

				allFCs = allFCs.Substring(0,allFCs.Length-1)
				
					     
				Dim SQL As String
				SQL = $"Select Req.Title,
						  	   Req.Entity,
							   Req.UFR_ID,
							   Req.UFR_ID_Type,
							   Req.Command_UFR_Status
						FROM XFC_CMD_UFR AS Req
						
						WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
						AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
						AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
						AND Req.Entity IN ({allFCs})
						Order By Req.Entity,Req.UFR_ID"

				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQTitleList")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
				
				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("Entity")
					Dim ExistingREQ As String =  dataRow.Item("UFR_ID")
					Dim TitleValue As String = ""
					If dbnull.Value.Equals(dataRow.Item("Title")) Then
						TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
					Else
						 TitleValue  =  dataRow.Item("Title")
					End If 
					
					Dim REQID As String = datarow.Item("UFR_ID")
					Dim REQStatus As String = dataRow.Item("Command_UFR_Status")
'					Dim REQType As String = dataRow.Item("UFR_ID_Type")
					Dim REQTitle As String = dataRow.Item("Title")
					
					'--------- get Entity Text3 --------- 
					Dim sFC As String = sFundCenter.Replace(".Base","")
					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFC).Member
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sREQTime)
					Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
					entityText3 = entityText3.Substring(entityText3.Length -2, 2)

					'--------- get next workflow level --------- 
					Dim currText3Num As String = entityText3.Substring(1,1)
					Dim icurrText3Num As Integer = currText3Num
					'icurrText3Num = icurrText3Num - 1
					Dim newText3Num As String = icurrText3Num - 1
					Dim newWFLevel As String = "L" & newText3Num
					
			
					'--------- get REQ workflow status level --------- 
					Dim reqWFStatusLevel As String = REQStatus.Substring(0,2)	
					If String.IsNullOrWhiteSpace(TitleValue) Then Continue For
						
					'================================Sub-CMD WF Statuses======================		
					If (sProfileName = "Manage UFR") Or (sProfileName = "Manage UFR" And REQStatus.XFEqualsIgnoreCase("BlankStatus")) Then	
							If Not REQStatus.XFEqualsIgnoreCase("L2_") Then
								If reqWFStatusLevel = entityText3 Then			
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
								End If
							End If
#Region "Delete"						
					'================================Command Manage WF======================	
					'*** not necessary?
					ElseIf ( sProfileName = "Manage CMD UFR") Or ( sProfileName = "Manage CMD UFR" And REQStatus.XFEqualsIgnoreCase("BlankStatus")) Then
							If REQStatus.XFContainsIgnoreCase("L2_") Then 			
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
#End Region											
					Else If sProfileName = "Formulate UFR" 
							If REQStatus.XFContainsIgnoreCase("Formulate") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
#Region "Delete"													
'							If (REQStatus.XFContainsIgnoreCase("Formulate") And Not REQStatus.XFContainsIgnoreCase("L2") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And Not REQType.XFEqualsIgnoreCase("CivPay") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And Not REQType.XFEqualsIgnoreCase("Withhold")) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'							End If
							
'							If (REQStatus.XFContainsIgnoreCase("Formulate") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And REQType.XFEqualsIgnoreCase("CivPay") And REQID.XFContainsIgnoreCase(sAPPN) Then 
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
							
'							If (REQStatus.XFContainsIgnoreCase("Formulate") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And REQType.XFEqualsIgnoreCase("Withhold") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
#End Region

					Else If sProfileName = "Staff UFR" 
							If REQStatus.XFContainsIgnoreCase("Staff") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
					
					Else If sProfileName = "Validate UFR" 
							If REQStatus.XFContainsIgnoreCase("Validate") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
					
					Else If sProfileName = "Prioritize UFR" 
							If REQStatus.XFContainsIgnoreCase("Prioritize") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
					
					Else If sProfileName = "Approve UFR" 
							If REQStatus.XFContainsIgnoreCase("Approve") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
					
					Else If sProfileName = "Review UFR" 
							If REQStatus.XFContainsIgnoreCase("Review") Then
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If

					'================================CMD WF Statuses======================	
					
					Else If sProfileName = "Manage CMD UFR"
							If (REQStatus = "L2_Manage_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
				
					Else If sProfileName = "Formulate CMD UFR"
							If (REQStatus = "L2_Formulate_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
					Else If sProfileName = "Staff CMD UFR"
							If (REQStatus = "L2_Staff_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
					Else If sProfileName = "Validate CMD UFR"
							If (REQStatus = "L2_Validate_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
					Else If sProfileName = "Prioritize CMD UFR"
							If (REQStatus = "L2_Prioritize_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
#Region "Delete"				
'							Else If sProfileName = "Formulate CMD UFR"
'							If (REQStatus = "L2_Formulate_SPLN") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And Not REQType.XFEqualsIgnoreCase("CivPay") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And Not REQType.XFEqualsIgnoreCase("Withhold") Then 
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'							End If
'							If (REQStatus = "L2_Formulate_UFR" Or REQStatus = "L2_Final_UFR") 
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And REQType.XFEqualsIgnoreCase("CivPay") And REQID.XFContainsIgnoreCase(sAPPN) Then
      								
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
'							If (REQStatus = "L2_Formulate_UFR" Or REQStatus = "L2_Final_UFR") 
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And REQType.XFEqualsIgnoreCase("Withhold") And REQID.XFContainsIgnoreCase(sAPPN) Then
      								
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
#End Region							

					Else If sProfileName = "Approve CMD UFR"
							If (REQStatus = "L2_Approve_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If

					Else If sProfileName = "Review CMD UFR"
							If (REQStatus = "L2_Review_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
							
					'================================HQ WF Statuses======================	

					Else If sProfileName = "Manage HQ UFR"
							If (REQStatus = "L1_Manage_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
								
					Else If sProfileName = "Staff HQ UFR"
							If (REQStatus = "L1_Staff_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
					Else If sProfileName = "Validate HQ UFR"
							If (REQStatus = "L1_Validate_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
				
					Else If sProfileName = "Prioritize HQ UFR"
							If (REQStatus = "L1_Prioritize_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
							
					Else If sProfileName = "Approve HQ UFR"
							If (REQStatus = "L1_Approve_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If

					Else If sProfileName = "Review HQ UFR"
							If (REQStatus = "L1_Review_UFR") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
							
							
							
#Region "Delete"
					'================================Validate REQs WF======================			
'					ElseIf sProfileName = "Validate UFR" Then
'							If reqWFStatusLevel = entityText3 And REQStatus.Contains("Validate") And Not REQStatus.Contains("L2")And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And Not REQType.XFEqualsIgnoreCase("CivPay") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And Not REQType.XFEqualsIgnoreCase("Withhold") Then							
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
'							End If
'							If (REQStatus.XFContainsIgnoreCase("Validate") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And REQType.XFEqualsIgnoreCase("CivPay") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
							
'							If (REQStatus.XFContainsIgnoreCase("Validate") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And REQType.XFEqualsIgnoreCase("Withhold") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If
							
							
'					'================================Validate CMD WF ======================		
'					ElseIf sProfileName = "Validate CMD UFR" Then
'						If REQStatus = ("L2_Validate_UFR")  Then 'updated
'							If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And REQType.XFEqualsIgnoreCase("CivPay") And REQID.XFContainsIgnoreCase(sAPPN) Then
'								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'							End If 
'							If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And REQType.XFEqualsIgnoreCase("Withhold") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'							End If 
'							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
'						End If 	
				
'					'================================Approve WF======================	
'					ElseIf wfProfileName.XFContainsIgnoreCase("Approve") Then
						
'						If REQStatus.XFContainsIgnoreCase("Approve") And reqWFStatusLevel = entityText3 Then 'updated
'							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
'						End If
'						If (REQStatus.XFContainsIgnoreCase("Approve") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Civ") And REQType.XFEqualsIgnoreCase("CivPay") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If	
							
'							If (REQStatus.XFContainsIgnoreCase("Approve") And Not REQStatus.XFContainsIgnoreCase("L2")) Then
'								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("WH") And REQType.XFEqualsIgnoreCase("Withhold") And REQID.XFContainsIgnoreCase(sAPPN) Then
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
'								End If 
'							End If
						
'							'================================Review Requirements WF======================			
'				ElseIf wfProfileName.XFContainsIgnoreCase("Review") Then
'						Me.WriteNameValuePairRow(si, dt, FundCenter & " - "  & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated 			
					End If 
#End Region					
					
					
					
'				
				Next
'BRApi.ErrorLog.LogMessage(si, "returned row count: " & dt.Rows.Count)

				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

	End Class
End Namespace