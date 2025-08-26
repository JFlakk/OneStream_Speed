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

Namespace OneStream.BusinessRule.DashboardDataSet.REQ_DataSet
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
'If si.UserName.XFEqualsIgnoreCase("rdavies") Then BRapi.ErrorLog.LogMessage(si,"START Dataset: "& args.DataSetName & " " & Date.Now().ToString("hh:mm:ss:fff") )						
				
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("StatusList")
						Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						If args.DataSetName.XFEqualsIgnoreCase("StatusList") Then
							
							'Check the filter type in order to retrieve the proper stored list
							Dim listType As String = args.NameValuePairs.XFGetValue("Type","All")
							'Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
							Dim statusValues As String = "Test1, Test2" 							
							
							'Get the stored status list and turn it into a data table (Removes Square Brackets)
							Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")
						
							'Create and fill it from the list of status Values
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "StatusList")
							For Each statusItem As String In statusItems
								Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
							Next				
							Return dt 	
							
						End If
#Region "Related REQs By Selected Entity"
						If args.DataSetName.XFEqualsIgnoreCase("REQListByEntity") Then
							Return Me.REQListByEntity(si,globals,api,args)						
						End If
#End Region  'Updated

#Region "REQs By Entity an Status"
						If args.DataSetName.XFEqualsIgnoreCase("REQListByEntityAndStatus") Then
							Return Me.REQListByEntityAndStatus(si,globals,api,args)						
						End If
#End Region  'Updated

#Region "Demotion of UFR"
						If args.DataSetName.XFEqualsIgnoreCase("GetUFRFundCenterForDemote") Then	
							'Return Me.GetUFRFundCenterForDemote(si,globals,api,args)

						End If

#End Region 'commneted out

#Region "Full Related REQ List"
'Updated by Fronz 09/06/2024 - changed the S# to REQ_Shared and T# to 1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
						If args.DataSetName.XFEqualsIgnoreCase("FullREQList") Then

							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'							Dim sScenario As String = "REQ_Shared"
'							Dim sREQTime As String = "1999"
							Dim FundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
							Dim ExistingREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base",True).OrderBy(Function(x) x.Member.name).ToList()
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FullREQList")

								'Checking and adding _General for upper levels'
								Dim pk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_" & sCube)

								Dim memberid As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FundCenter)					
								Dim parent As Boolean = BRApi.Finance.Members.HasChildren(si, pk, memberid)							
								If parent Then FundCenter = FundCenter & "_General"
								
								For Each ExistingREQ As MemberInfo In ExistingREQs
						            Dim REQEntityMemberScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & ExistingREQ.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityMemberScript).DataCellEx.DataCellAnnotation
		               
		                            If Not String.IsNullOrWhiteSpace(TitleValue) Then
										Me.WriteNameValuePairRow(si, dt, FundCenter.Replace("_General","") & " - " & ExistingREQ.Member.Name & " - " & TitleValue, FundCenter & " - " & ExistingREQ.Member.Name & " - " & TitleValue)

									End If 

								Next
							
							Return dt 	
							
						End If


#End Region   'Updated

#Region "Get All Users"						
						If args.DataSetName.XFEqualsIgnoreCase("GetAllUsers") Then
'Brapi.ErrorLog.LogMessage(si,"public call")
							Return Me.GetAllUsers(si,globals,api,args)	
							
						End If						
#End Region 		

#Region "Get REQ Status"						
						If args.DataSetName.XFEqualsIgnoreCase("GetREQStatus") Then

							Return Me.GetREQStatus(si,globals,api,args)	
							
						End If						
#End Region

#Region "REQ List by Cached Entity"

						If args.DataSetName.XFEqualsIgnoreCase("REQListCachedEntity") Then
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
							Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
							Dim ExistingREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base",True).OrderBy(Function(x) x.Member.name).ToList()
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListCachedEntity")

								For Each ExistingREQ As MemberInfo In ExistingREQs
						            Dim REQEntityMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & ExistingREQ.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityMemberScript).DataCellEx.DataCellAnnotation
		               
		                            If Not String.IsNullOrWhiteSpace(TitleValue) Then
										Me.WriteNameValuePairRow(si, dt, sFundCenter & " - " & ExistingREQ.Member.Name & " - " & TitleValue, ExistingREQ.Member.Name)

									End If 

								Next

							Return dt 	
							
						End If


#End Region  'Updated

#Region "REQ List For Copy"
						'Return a datatable of Requirements to be used as copy source based on user's filter inputs
						'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
						If args.DataSetName.XFEqualsIgnoreCase("REQListForCopy") Then
							
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        'Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sScenario As String = args.NameValuePairs.XFGetValue("scenario",si.WorkflowClusterPk.ScenarioKey)
					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FC","")
							Dim	sAccount As String = args.NameValuePairs.XFGetValue("Account","")
							Dim	sAccountVal As String = args.NameValuePairs.XFGetValue("AccountVal","")
							Dim	sKeyword As String = args.NameValuePairs.XFGetValue("Keyword","")
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListForCopy")				
							
							'Return an empty datatable if no fund center was selected
							'Also to address a behavior where on initial load, |!param!| was passed in as a text instead of the selected value (which ought to be empty), an if statement is used here to address that
							If String.IsNullOrWhiteSpace(sFundCenter) Or sFundCenter.Contains("|!") Then Return dt
							If sAccount.Contains("|!") Then sAccount = ""
							If sAccountVal.Contains("|!") Then sAccountVal = ""
							
							'Grab the list of accounts that are available for the users to use as filter 	
							Dim lAccounts As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"A_REQ_Main","A#REQ_Requirement_Details_ComboBox_Selections.Children",True)
							Dim sAccountSQL As String = "[REQ_Title],[REQ_Description]"
							For Each Account As MemberInfo In lAccounts
								sAccountSQL = $"{sAccountSQL},[{Account.Member.Name}]"
							Next
							
							'Build SQL to return list of requirements that fit the user's criterias
							Dim SQL As New Text.StringBuilder
							SQL.Append($"SELECT * FROM 
											(SELECT ENTITY, FLOW, ACCOUNT, TEXT
    										 FROM DATAATTACHMENT
    										 WHERE SCENARIO = '{sScenario}'
											) As SOURCETABLE
										PIVOT (
 										   MAX(TEXT)
    									   FOR ACCOUNT IN ({sAccountSQL})
										) AS PIVOTTABLE ")
							SQL.Append($"WHERE ENTITY = '{sFundCenter}' AND REQ_TITLE IS NOT NULL AND FLOW not like 'REQ_00%'")
							
							BRApi.ErrorLog.LogMessage(si,"SQL query ran 190")
						
							If Not String.IsNullOrWhiteSpace(sAccount) And Not String.IsNullOrWhiteSpace(sAccountVal) Then SQL.Append($" AND {sAccount} = '{sAccountVal}'")
							If Not String.IsNullOrWhiteSpace(sKeyword) Then SQL.Append($" AND (REQ_TITLE LIKE '%{sKeyword}%' OR REQ_DESCRIPTION LIKE '%{sKeyword}%')")
'BRApi.ErrorLog.LogMessage(si,$"SQL: {SQL.ToString}")			
							
							'Fetch datatable 
							Dim dtFetch As New DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								 dtFetch = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
							End Using
							
							'Loop through the fetched datatable and write the result datatable to be returned
							For Each dataRow As DataRow In dtFetch.Rows		
								Dim ExistingREQ As String =  dataRow.Item("FLOW")	
								Dim TitleValue As String =  dataRow.Item("REQ_TITLE")
								If Not String.IsNullOrWhiteSpace(TitleValue) Then								
									Me.WriteNameValuePairRow(si, dt, sFundCenter & " - " & ExistingREQ & " - " & TitleValue, ExistingREQ)
								End If 
							Next
'BRApi.ErrorLog.LogMessage(si,"Debug F")			
							Return dt							
						End If


#End Region 'NEW

#Region "UFRs Available for Merging By Selected Entity"
'						If args.DataSetName.XFEqualsIgnoreCase("MergeUFRListByEntity") Then
'							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'							'Dim wfLevel As String = wfProfileName.Substring(0,2)			
'							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					        Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'							Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter","")
'							Dim sDashboard As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Dashboard","")

'							If String.IsNullOrEmpty(sFundCenter) Then
'								Return Nothing
'							End If
'							Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
'         					Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)
'							Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
'							Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
'							If Not isBase Then
'								Return Nothing
'							End If
							
'							Dim sStatus As String = ""
'							'Remove _General to get the parent Entity
'							sFundCenter = sFundCenter.Replace("_General","")
							
'							'Get List of FCs
'							'Dim FundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", $"E#{sFundCenter}.ChildrenInclusive",True).OrderBy(Function(x) x.Member.name).ToList()
'							Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'							Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)	
'							Dim sDimDef As String = "E#"
'							Dim sDataBufferPOVScript="Cb#" & sCube & ":S#" & sScenario & ":T#" & iWFYear & "M12:C#Local:V#Periodic:A#UFR_Requested_Amt:I#Top:O#BeforeAdj:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								
'							Dim sFlow As String = "Unfunded_Requirements_Flows.Base"
'							Dim FundCenters As List(Of memberinfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=General_Member_Lists, MemberListName=GetFilteredDataRows, DimDef=" & sDimDef & ", DataBufferPOVScript=" & sDataBufferPOVScript  & ", Entity="  & sFundCenter & ".member.base" & ", Flow=" & sFlow & ")", True)
'							If FundCenters.count = 0 Then Return Nothing
							
'							'Get List of UFRs
'							Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Requirements_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()
'							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "MergeUFRsList")
								
'							If wfProfileName.XFContainsIgnoreCase("Approve") Then
'								wfProfileName = "Approve"
'								sStatus = wfLevel & " Ready For Disposition"
'							Else If wfProfileName.XFContainsIgnoreCase("Create") Then
'								wfProfileName = "Create"
'								If sDashboard.XFContainsIgnoreCase("Promoted") Then
'									sDashboard = "Promoted"
'									sStatus = "% Release Draft to " & wfLevel
'								Else
'									sDashboard = "Create"
'									sStatus = wfLevel & " Working"
'								End If
'							End If
															
'							Dim allUFRs As String = ""
'							For Each UFR As MemberInfo In ExistingUFRs
'								allUFRs = $"{allUFRs}'{UFR.Member.Name}',"
'							Next
'							allUFRs = allUFRs.Substring(0,allUFRs.Length-1)
							
'							Dim allFCs As String = ""
'							For Each FundCenter As MemberInfo In FundCenters
'								allFCs = $"{allFCs}'{FundCenter.Member.Name}',"
'							Next							
'							allFCs = allFCs.Substring(0,allFCs.Length-1)
'							'SQL statement to concat "Entity - UFR: title" 
'							Dim SQL As New Text.StringBuilder
'							SQL.Append("SELECT CONCAT(A.ENTITY, ' - ', A.FLOW) AS EntityUFRs, CONCAT(A.ENTITY, ' - ', A.FLOW, ' : ', B.TEXT) AS UFRs ")
'							SQL.Append("FROM DATAATTACHMENT A, DATAATTACHMENT B ")
'							SQL.Append($"WHERE A.CUBE = B.CUBE 
'										 AND A.ENTITY = B.ENTITY 
'										 AND A.SCENARIO = B.SCENARIO 
'										 AND A.TIME = B.TIME 
'										 AND A.FLOW = B.FLOW 
'										 AND A.ACCOUNT = 'REQ_WORKFLOW_STATUS' 
'										 AND B.ACCOUNT = 'REQ_TITLE'
'										 AND A.TEXT LIKE '{sStatus}' 
'										 AND A.CUBE = '{sCube}'
'										 AND A.ENTITY IN ({allFCs}) 
'										 AND A.FLOW IN ({allUFRs}) 
'										 AND A.SCENARIO = '{sScenario}'
'										 ORDER BY A.ENTITY")
''BRApi.ErrorLog.LogMessage(si,SQL.ToString)
'							Dim dtFetch As New DataTable
'							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'								 dtFetch = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'							End Using

'							For intRows = 0 To dtFetch.Rows.Count - 1
'								Me.WriteNameValuePairRow(si, dt, dtFetch(intRows)("UFRs"),dtFetch(intRows)("EntityUFRs"))
'							Next

'							Return dt
'		#Region "Old code: To be deleted if new code is approved"					
''							Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
''							Dim wfLevel As String = wfProfileName.Substring(0,2)			
''							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
''					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
''					        Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
''							Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Entity","")
''							Dim sDashboard As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Dashboard","")
''							'Remove _General to get the parent Entity
''							sFundCenter = sFundCenter.Replace("_General","")
''							'Get List of FCs
''							Dim FundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", $"E#{sFundCenter}.ChildrenInclusive",True).OrderBy(Function(x) x.Member.name).ToList()
''							'Get List of UFRs
''							Dim ExistingUFRs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_UFR_Main","F#Unfunded_Requirements_Flows.Base",True).OrderBy(Function(x) x.Member.name).ToList()
''							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "MergeUFRsList")
							
''							If wfProfileName.XFContainsIgnoreCase("Approve") Then
''								wfProfileName = "Approve"
''							Else If wfProfileName.XFContainsIgnoreCase("Create") Then
''								wfProfileName = "Create"
''							End If
							
''							If sDashboard.XFContainsIgnoreCase("Promoted") Then
''								sDashboard = "Promoted"
''							Else
''								sDashboard = "Create"
''							End If
''							'Loop through FCs and UFRs to build data table
''							For Each FundCenter As MemberInfo In FundCenters
''								sFundCenter = FundCenter.Member.Name
''								For Each ExistingUFR As MemberInfo In ExistingUFRs
''						            Dim UFRtitleMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Title:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
										
''									Dim UFRStatusMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sUFRTime & ":V#Annotation:A#REQ_Workflow_Status:F#" & ExistingUFR.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
''									Dim UFRStatus As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRStatusMemberScript).DataCellEx.DataCellAnnotation
''									If String.IsNullOrWhiteSpace(UFRStatus) Then Continue For
										
''									Select Case wfProfileName
''									Case "Create"
''										If (sDashboard = "Create" And UFRStatus.XFContainsIgnoreCase(wfLevel & " Working")) Then
''											Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRtitleMemberScript).DataCellEx.DataCellAnnotation
''											Me.WriteNameValuePairRow(si, dt, $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}", $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}")
''										End If
										
''										If (sDashboard = "Promoted" And UFRStatus.XFContainsIgnoreCase("Draft to " & wfLevel)) Then		
''											Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRtitleMemberScript).DataCellEx.DataCellAnnotation
''											Me.WriteNameValuePairRow(si, dt, $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}", $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}")
''										End If
''									Case "Approve"
''										If UFRStatus.XFContainsIgnoreCase(wfLevel & " Ready For Disposition")  Then					
''											Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, UFRtitleMemberScript).DataCellEx.DataCellAnnotation
''											Me.WriteNameValuePairRow(si, dt, $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}", $"{sFundCenter} - {ExistingUFR.Member.Name} : {TitleValue}")
''										End If
''									End Select

''								Next
''							Next

''Dim tStop As DateTime =  Date.Now()
''Dim tDuration As TimeSpan = tStop.Subtract(tStart)
''BRAPI.ErrorLog.LogMessage(si,"MergeUFRListByEntity - Duration=" & tDuration.TotalSeconds.ToString("0.0000"))

''							Return dt 	

'		#End Region
'						End If

#End Region 'Commented out

#Region "UFR List to Recall for Re-Prioritization by Entity"
						'RMW-1012 Return list of UFRs for Re-Prioritization
						If args.DataSetName.XFEqualsIgnoreCase("RePrioritizeUFRListByEntity") Then						
							Return Me.GetUFRListForRePrioritization(si,globals,api,args)
						End If
#End Region

#Region "Get User Fund Centers By Workflow"						
						If args.DataSetName.XFEqualsIgnoreCase("GetUserFundCentersByWF") Then

							Return Me.GetUserFundCentersByWF(si,globals,api,args)	
							
						End If						
#End Region	

#Region "Get UFR Stakeholder POCs"	
						'RMW-1083 - This function returns appropriate U5 members for Stakeholders depending on whther workflow is Lv1 or not
						If args.DataSetName.XFEqualsIgnoreCase("GetUFRStkhldrPOCs") Then

							'Return Me.GetUFRStkhldrPOCs(si,globals,api,args)	
							
						End If						
#End Region 'commented out

#Region "Get List of File Names in Folder" 'New
						'Return a list of files in a folder
						If args.DataSetName.XFEqualsIgnoreCase("GetFileNamesInFolder") Then

							Return Me.GetFileNamesInFolder(si,globals,api,args)	
							
						End If						
#End Region

#Region "Get REQs for Submission"						
						If args.DataSetName.XFEqualsIgnoreCase("REQListToSubmit") Then
'Brapi.ErrorLog.LogMessage(si,"public call")
							Return Me.REQListToSubmit(si,globals,api,args)	
							
						End If						
#End Region

#Region "REQ WF Status List"
						If args.DataSetName.XFEqualsIgnoreCase("REQWFStatusList") Then
							Return Me.REQWFStatusList(si,globals,api,args)						
						End If
#End Region

#Region "ManpowerPBList"
						If args.DataSetName.XFEqualsIgnoreCase("ManpowerPBList") Then
'BRApi.ErrorLog.LogMessage(si,"Debug F")								
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        'Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sScenario As String = args.NameValuePairs.XFGetValue("scenario",si.WorkflowClusterPk.ScenarioKey)
					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
							Dim sCurrYear As String = sREQTime.Substring(2,2)
							Dim sPriorYear As String = sCurrYear - 1
'BRApi.ErrorLog.LogMessage(si,"sCurrYear = " & sCurrYear & ":  sPriorYear = " & sPriorYear)	
							Dim sPositions As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"S_Main","S#HQDA_Programming.base.Where((Name Contains " & sCurrYear & ") or (Name Contains " & sPriorYear & "))" ,True)
'						
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "PositionList")
							'Loop through the fetched datatable and write the result datatable to be returned
							For Each sPosition As MemberInfo In sPositions 	
														
									Me.WriteNameValuePairRow(si, dt, sPosition.Member.Name, sPosition.Member.Name)
								
							Next
'BRApi.ErrorLog.LogMessage(si,"Debug F")			
							Return dt
							End If
#End Region

#Region "Get Fund Centers By Command"
						If args.DataSetName.XFEqualsIgnoreCase("GetFundCentersByCommand") Then
							Return Me.GetFundCentersByCommand(si,globals,api,args)						
						End If
#End Region

#Region "Get REQs to be Deleted"
						If args.DataSetName.XFEqualsIgnoreCase("GetREQsToBeDeleted") Then
							Return Me.GetREQsToBeDeleted(si,globals,api,args)						
						End If
#End Region
				End Select

				Return Nothing
			Catch ex As Exception

				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
'#Region "Constants"
'	Private BR_GeneralMemberLists As New OneStream.BusinessRule.Finance.General_Member_Lists.MainClass
'#End Region
		
#Region "Get All Users"

		Public Function GetAllUsers(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs, Optional ByVal mode As String = "AllFC_CMD") As Object
			Try
		
'Brapi.ErrorLog.LogMessage(si,"method initial call")
			
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim fullPGMEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim PGMEntity As String = fullPGMEntity.Replace("_General","")
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode", mode)
				
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name
				Dim wfProfileAccessGroupmodule As String = wfProfileAccessGroup.split("_")(1)
				Dim WFValidator As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"VA")
				
				Dim SQL As String = String.Empty
				
				Dim tableName As String = "AllPossibleStakeholders"
				
			Select Case sMode
				Case "AllFC_CMD"				
						SQL= $"Select Distinct U.Name As UserName, U.Email As UserEmail
						From  [SecUser] As u
						Join  [SecGroupChild] As GU On gu.ChildKey = u.uniqueid
						Join  [SecGroup] As SG On sg.UniqueID = gu.GroupKey
						Where (SG.Name Like  '%{wfProfileAccessGroupmodule}%' AND SG.Name like '%{PGMEntity}')"
				
				Case "FC_VA"
						SQL = $" SELECT Distinct U.Name as UserName, U.Email as UserEmail
						FROM  [SecUser] as u
						join  [SecGroupChild] AS GU on gu.ChildKey = u.uniqueid
						join  [SecGroup] AS SG on sg.UniqueID = gu.GroupKey
						Where (SG.Name like  '{WFValidator}%' AND SG.Name like '%{PGMEntity}')"
						
						tableName = "FCValidator"
				End Select
					Dim dt As DataTable = Me.CreateNameValuePairTable(si, tableName)
						Dim dtAll As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
						 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
						End Using

				For Each row As DataRow In dtAll.Rows		
						Dim sUserName As String = String.Empty
						Dim sUserEmail As String = String.Empty
						If Row.Table.Columns.Contains("UserName") AndAlso Not IsDBNull(Row("UserName"))  Then
							sUserName = CStr(Row("UserName"))
							
						End If 
						If Row.Table.Columns.Contains("UserEmail") AndAlso Not IsDBNull(Row("UserEmail"))Then
							sUserEmail = CStr(Row("UserEmail"))
							
						End If 
						
						Me.WriteNameValuePairRow(si, dt, sUserName.Trim, sUserEmail.Trim)
						Next
				
				Return dt
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try	
		End Function

#End Region	'update here

#Region "GetLastFundCenterLevel"
		' Created: 2024-Feb-05 - CS
		' Purpose: Parse UFR status history to return the last FC level the UFR was on
		' Usage: UFRListByEntity() calls in order to overwrite a null status to the proper FC level
		Public Function GetLastFundCenterLevel(ByVal si As SessionInfo, ByVal sCube As String, ByVal memberScript As String) As String	

			Dim fcLevel As String = ""
			'Get the status history
			Dim statusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, memberScript).DataCellEx.DataCellAnnotation.Trim		
			'test if history is empty
			If statusHistory = "" Then
				Throw New exception ("Status History is Empty for: " & vbCrLf & memberScript.Replace(":F#","~").Split("~")(1).Split(":")(0))
			End If 
			'Split the status history into an array
			Dim statusArray() As String = statusHistory.Split(",")

				'Split the individual status into an array for additional parsing. Expected output is {FC, FC Level, Status}
				Dim arrayStatusArray() As String = statusArray(statusArray.Length - 1).trim.Split(" ")
				'Sometimes only a FC is listed so an array size check is required. Additionally checking the FC Level is in the expected format of beginning with a L
				If arrayStatusArray.Length > 8 And arrayStatusArray(8).StartsWith("L") Then
					'Expected location of the FC Level after condition checks is then assigned to the fcLevel variable
					fcLevel = arrayStatusArray(8)

				End If
			
			Return fcLevel
		End Function
			  
#End Region

#Region "Get UFR by Fund Center for demotion"
'		Public Function GetUFRFundCenterForDemote(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object	
		
'			'obtains the Fund, Name and Entityfrom the Create UFR Dashboard
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'			Dim wfLevel As String = wfProfileName.Substring(0,2)			
'			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'			Dim sUFRTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'			Dim sFundCenterOriginal As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","FundCenter","")
'			Dim sDashboard As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Dashboard","")
						
'							If String.IsNullOrWhiteSpace(sFundCenterOriginal) Then
'								Throw  New Exception("Please Select a Fund Center")
'							End If
							
'							Dim sStatus As String = ""
'							'Remove _General to get the parent Entity
'							Dim sFundCenter As String = sFundCenterOriginal.Replace("_General","")
							
'							'Get List of FCs
'							'Dim FundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", $"E#{sFundCenter}.ChildrenInclusive",True).OrderBy(Function(x) x.Member.name).ToList()
'							Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'							Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)	
'							Dim sDimDef As String = "E#"
'							Dim sDataBufferPOVScript="Cb#" & sCube & ":S#" & sScenario & ":T#" & iWFYear & "M12:C#Local:V#Periodic:A#UFR_Requested_Amt:I#Top:O#BeforeAdj:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
							
'							Dim sFlow As String = "Unfunded_Requirements_Flows.Base"
'							Dim FundCenters As List(Of memberinfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"E_ARMY", "E#Root.CustomMemberList(BRName=General_Member_Lists, MemberListName=GetFilteredDataRows, DimDef=" & sDimDef & ", DataBufferPOVScript=" & sDataBufferPOVScript  & ", Entity="  & sFundCenter & ".member.base" & ", Flow=" & sFlow & ")", True)
'							If FundCenters.count = 0 Then Return Nothing
							
									
									
									
'			'Call dataset BR to return a datatable that has been filtered by ufr status
'			args.NameValuePairs.XFSetValue("Entity", sFundCenterOriginal)
'			args.NameValuePairs.XFSetValue("Mode", "Demote")
'			Dim objDT As DataTable = REQListByEntity(si, globals, api, args)
			
'							Return objDT

			
		
			 
'		End Function
			  
#End Region 'Commnted out

#Region "Get UFRs for Re-Prioritization"
		'RMW-1012 Return list of UFRs for Re-Prioritization
		Public Function GetUFRListForRePrioritization(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object	
		
			'get FundCenter from cached prompt from Prioritize dashboard
			Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","Entity","")
			If String.IsNullOrWhiteSpace(sFundCenter) Then Return Nothing																								
			'Call dataset BR to return a datatable that has been filtered by ufr status
			args.NameValuePairs.XFSetValue("Entity", sFundCenter)
			args.NameValuePairs.XFSetValue("Mode", "Re-Prioritize") 
			Dim dt = REQListByEntity(si, globals, api, args)
			Dim sSelectAllVal As String = ""
			For Each row As DataRow In dt.Rows
				sSelectAllVal = $"{sSelectAllVal},{CStr(row("Value"))}"
			Next
			If sSelectAllVal.length > 0 Then sSelectAllVal = sSelectAllVal.Substring(1)
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"UFRPrompts","UFRSelectAll",sSelectAllVal)
			

'			Dim selectAllRow As DataRow = dt.NewRow()
'			selectAllRow("Name") = "All"
'			selectAllRow("Value") = sSelectAllVal
'			dt.Rows.InsertAt(selectAllRow,0)
			Return dt
		End Function
			  
#End Region

#Region "Get REQ Status"

		Public Function GetREQStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				'Get WF Profile and Status type
				Dim sWfProfile As String = args.NameValuePairs.XFGetValue("wfProfile")			
				Dim sStatusType As String = args.NameValuePairs.XFGetValue("StatusType")
				Dim sEntityGeneral As String = args.NameValuePairs.XFGetValue("Entity")
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sEntity As String = sEntityGeneral.Replace("_General","")
				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube) 
				Dim bHasChildren As Boolean = Nothing
				If  Not String.IsNullOrWhiteSpace(sEntity) Then
					bHasChildren = brapi.Finance.Members.HasChildren(si,DimPk,brapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id,sEntity))
				End If
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)	
				
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = entityText3.Substring(1,1)
				Dim icurrentStatus As Integer = currentStatus
				icurrentStatus = icurrentStatus - 1
				Dim currentWFLevel As String = icurrentStatus
				Dim newWFLevel As String = "L" & currentWFLevel
				
				'--------------------------------
				'---Get WF Status for Manager----
				'--------------------------------
				'If sWfProfile = "Manager" Then
				If sProfileName = "Manage Requirements CMD" Then
					If sStatusType = "Workflow" Then
						Dim statusValues As String = entityText3 & " Working," & entityText3 & " Ready for Validation," &  entityText3 & " Ready for Prioritization," &  entityText3 & " Prioritized," &  entityText3 & " Ready for Approval," &  entityText3 & " Approved"
				
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")
						
						'Create and fill it from the list of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "GetValidStatusListForManager")

						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next		

						Return dt
					Else
						Return Nothing
					End If
				End If

				If sProfileName = "Manage Requirements" Then	
					If sStatusType = "Workflow" Then
						Dim statusValues As String = ""
							If entityText3 = "L5" Or (entityText3 = "L4" And bHasChildren = False)
								statusValues = entityText3 & " Working," & newWFLevel & " Ready for Validation"

							'------ KN 2025/04/16 - Updated to If-Else If statement to accomodate for L3 base FCs which will need to go through the entire process like a normal L3 FC with children
							' Original code:
							'							End If	
							'							If Not entityText3 = "L2" And bHasChildren = True
							ElseIf Not entityText3 = "L2"
								statusValues =  entityText3 & " Working," & entityText3 & " Ready for Validation," & entityText3 & " Ready for Prioritization," & entityText3 & " Prioritized,"  & entityText3 & " Ready for Approval," & newWFLevel & " Ready for Validation"
							End If

						'Add L2 or CMD dropdown values?
						
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")

						'Create and fill it from the list of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "GetValidStatusListForManager")
						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next
				
						Return dt
					Else
						Return Nothing
					End If

						
					'Gettting the L# Release to Draft L# at a higher level
				ElseIf (wfProfileName.XFContainsIgnoreCase("Review") And bHasChildren) Then
					Dim statusValues As String = " Release Draft to " 'PrevWFLevel & " Release Draft to " & wfLevel
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")

						'Create And fill it From the list Of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ParentCreateStatus")
						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next		
			
						Return dt
				'--------------------------------
				'--Get Status List for Approver--
				'--------------------------------
				ElseIf sWfProfile = "Approver" Then
					
					'Get WF Status for Approver
					If sStatusType = "Workflow" Then
						Dim statusValues As String = " Ready For Disposition,Closed,On Hold"  
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")

						'Create and fill it from the list of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ApproverWfStatus")
						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next		
				
						Return dt
					
					
					'Get Funding Status for Approver
					ElseIf sStatusType = "Funding" Then
										
						Dim statusValues As String = "Unfunded,Internally Funded,Partially Funded,Non-Concur/Obsolete,Other"
				
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")

						'Create and fill it from the list of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "GetValidFundingStatusListForApprover")
						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next		
				
						Return dt
					Else
						Return Nothing
					End If
				Else
					Return Nothing
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Security Logic: Get User Fund Centers By Workflow"
'Updated 07/19 ticket 1484 by KL, MF, CM
'Updated 5/29/2025 MF. New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundCentersByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Dim fcDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FundCentersByWF")
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name

'For Each xxxx In accessGroupInfo_Split
'	brapi.ErrorLog.LogMessage(si, xxxx)
'Next

'brapi.ErrorLog.LogMessage(si, wfProfileAccessGroup)				
				
				Dim WFManager As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"MG")				
				Dim wfProfileStep As String = accessGroupInfo_Split(accessGroupInfo_Split.Count-1)
'brapi.ErrorLog.LogMessage(si, wfProfileStep)

				If wfProfileStep = "WF"
					wfProfileAccessGroup = StringHelper.ReplaceString(wfProfileAccessGroup,"_WF",String.Empty,True)
				End If					
				'i.e., g_PGM_AFC_FC_CR_WF --> g_PGM_AFC_FC_CR
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name		
				'Get WF profile command
				Dim cmd As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName 
'Brapi.ErrorLog.LogMessage(si,"WF" & cmd)	
				Dim sFCgroups As String = ""
				'Get fund centers from the user profile. Get groups user belongs to and substring FC from them
				'Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
				'Dim sUserName As String = objSignedOnUser.User.Name
				Dim sUserName As String = si.AuthToken.UserName
				Dim techSupport As Boolean = BRApi.Security.Authorization.IsUserInGroup(si,sUserName,"g_Technical_Support",False)
				Dim helpSupport As Boolean = BRApi.Security.Authorization.IsUserInGroup(si,sUserName,"g_Helpdesk_Support",False)
				Dim funcSupport As Boolean = BRApi.Security.Authorization.IsUserInGroup(si,sUserName,"g_Functional_Support",False)
				Dim admin As Boolean = BRApi.Security.Authorization.IsUserInGroup(si,sUserName,"Administrators",False)
				Dim SQL As String
'BRAPI.ErrorLog.LogMessage(si,$"Support Roles: {sUserName} - {techSupport} - {funcSupport}")
				If (techSupport = False And funcSupport = False And helpSupport = False And admin = False)
					SQL = $"
						WITH RecursiveCTE AS (
							SELECT SG.UniqueID as GroupID, 
								SG.Name as AccessGroup,
								GRP2.GroupKey as ChildGroupID
							FROM SecUser as u
							JOIN SecGroupChild AS GRP1 on GRP1.ChildKey = u.uniqueid
							JOIN SecGroup AS SG on sg.UniqueID = GRP1.GroupKey
							JOIN SecGroupChild AS GRP2 on GRP2.ChildKey = GRP1.GroupKey
							WHERE U.Name = '{sUserName}'
							UNION ALL
							SELECT GRP.UniqueID as GroupID, 
								GRP.Name as AccessGroup,
								GRP2.GroupKey as ChildGroupID
							FROM SecGroup as GRP
							JOIN SecGroupChild AS GRP2 on GRP2.ChildKey = GRP.uniqueid
							INNER JOIN RecursiveCTE rcte ON rcte.ChildGroupID = GRP.UniqueID)
							SELECT Distinct AccessGroup
							FROM RecursiveCTE
							Where AccessGroup Like '{wfProfileAccessGroup}%'"
				Else
					SQL = $"
						SELECT SG.Name as AccessGroup
						FROM SecGroup as SG
						WHERE SG.Name like '{wfProfileAccessGroup}%'"
				End If
'brapi.ErrorLog.LogMessage(si, sql.ToString)
				Dim dtAll As New DataTable
				'Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "GroupSecList")
				Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL,True)
				End Using
			 
				For Each dataRow As DataRow In dtAll.Rows					
					Dim sGroup As String = dataRow.Item("AccessGroup")
'brapi.ErrorLog.LogMessage(si,$"dtrows: {sFCgroups}")	
'Contains the access group and the role security group with entity appended at the end
					sFCgroups+=sGroup + ","
				Next			
				If (String.IsNullOrWhiteSpace(sFCgroups)) Then
					Return Nothing
				Else 
				sFCgroups = sFCgroups.TrimEnd(","c)
				End If		

				Dim sGroupList As String() = sFCgroups.Split(",")
				Dim objDimDisplayOptions As New DimDisplayOptions()
				'filter fundcenters in the command only

				For Each Grp In sGroupList
					Dim Grp_Split As List(Of String) = StringHelper.SplitString(Grp, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
					Dim fc_name As String = Grp_Split(Grp_Split.Count-1)

					Dim MbrExpansion As String = String.Empty
'					If Not entityText3.Contains("=L2") Then
'						MbrExpansion = ".DescendantsInclusive"
'					End If						
					
					Dim fcList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", $"E#{fc_name}{MbrExpansion}", True)
					
					For Each fc In fcList
						If BRApi.Finance.Entity.HasChildren(si,fcDimPk,fc.Member.MemberId,objDimDisplayOptions) Then
							Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name & "_General")
						Else
							Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
						End If
					Next			
				Next	

				Dim sColumnlist As New List(Of String)

				For Each sColumn In dt.Columns
					sColumnlist.Add(sColumn.ColumnName)
				Next

				Return dt.defaultView.ToTable(True, sColumnlist.ToArray())

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "SetREQStatusFromNull"
		' Created: 2024-Feb-05 - CS
		' Purpose: Sets the REQ status to statusMessage. Meant to be encapsulate logic to fix null statuses. Currenly just doing a SetDataCell.
		' Usage: REQListByEntity() calls in order to overwrite a null status to an expected text
		Public Function SetREQStatusFromNull(ByVal si As SessionInfo, ByVal sCube As String, ByVal memberScript As String, ByVal statusMessage As String)
			Try			
				'MemberScriptAndValue template
				Dim objListofScriptsTitle As New List(Of MemberScriptandValue)
			    Dim objScriptValTitle As New MemberScriptAndValue
				objScriptValTitle.CubeName = sCube
				objScriptValTitle.Script = memberScript
				'Status value
				objScriptValTitle.TextValue = statusMessage
				objScriptValTitle.IsNoData = False
				objListofScriptsTitle.Add(objScriptValTitle)
				
				'Sets the new status value
				BRApi.Finance.Data.SetDataCellsUsingMemberScript(si, objListofScriptsTitle)
			
				Return Nothing

			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			 
		End Function
			  
#End Region  'Updated
	
#Region "REQListByEntity"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'Updated: AK RMW-1565 8/29/24 Updated SQL to get Title from REQ_Shared, 1999 for PGM_C20XX
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function REQListByEntity(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try	
'Dim tStart As DateTime =  Date.Now()				
'brapi.ErrorLog.LogMessage(si,"START: "	& System.DateTime.Now)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntity")				

				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
'brapi.ErrorLog.LogMessage(si,"sProfileSubString: " & sProfileSubString)							
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
'brapi.ErrorLog.LogMessage(si, sFundCenter)

				'If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Return dt
				End If
				
				'If the fund center passed is not a descendent of the WF then stop
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter.Replace("_General","") & ")"
				Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
				
				If Not cbMembers.Count > 0 Then
					Return dt
				End If
				
				'Remove _General to get the parent Entity
				If sFundCenter.Contains("_General") Then 'added
					sFundCenter = sFundCenter.Replace("_General",".Base") 'added 
				End If 'added														
		
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundCenter,True)
'brapi.ErrorLog.LogMessage(si, "FCs Count line = " & LFundCenters.Count)				
			
				Dim sCubeView As String = args.NameValuePairs.XFGetValue("CubeView")

				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allREQs As String = ""
				Dim allFCs As String = ""
				If LFundCenters.Count = 0 Then
					Return dt
				End If
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next

				allFCs = allFCs.Substring(0,allFCs.Length-1)
				
				Dim SQL As New Text.StringBuilder
'				SQL.Append($"WITH Status AS (
'						SELECT A.CUBE AS CUBE, A.ENTITY AS ENTITY, A.SCENARIO AS SCENARIO, A.TIME AS TIME, A.FLOW AS FLOW, A.TEXT AS STATUS
'						       FROM DATAATTACHMENT A 
'						        WHERE A.ACCOUNT = 'REQ_RQMT_STATUS'
'						        AND A.CUBE = '{sCube}'
'						        AND A.ENTITY IN ({allFCs})
'						        AND A.SCENARIO = '{sScenario}'
'						)
'						SELECT Status.ENTITY, Status.FLOW, Status.STATUS AS STATUS, B.Text AS TITLE FROM Status
'						JOIN DATAATTACHMENT B ON 
'						         Status.CUBE = B.CUBE 
'						         AND Status.ENTITY = B.ENTITY 
'						         AND Status.SCENARIO = B.SCENARIO 
'						         AND Status.TIME = B.TIME 
'						         AND Status.FLOW = B.FLOW 
'						WHERE B.ACCOUNT = 'REQ_TITLE'")
				
				SQL.Append($"WITH Status AS (
						SELECT A.CUBE AS CUBE, A.ENTITY AS ENTITY, A.SCENARIO AS SCENARIO, A.TIME AS TIME, A.FLOW AS FLOW, A.TEXT AS STATUS
						       FROM DATAATTACHMENT A 
						        WHERE A.ACCOUNT = 'REQ_RQMT_STATUS'
						        AND A.CUBE = '{sCube}'
						        AND A.ENTITY IN ({allFCs})
						        AND A.SCENARIO = '{sScenario}'
						),
						Title AS (
						SELECT A.CUBE AS CUBE, A.ENTITY AS ENTITY, A.SCENARIO AS SCENARIO, A.TIME AS TIME, A.FLOW AS FLOW, A.TEXT AS Text
						       FROM DATAATTACHMENT A 
						        WHERE A.ACCOUNT = 'REQ_TITLE'
						        AND A.CUBE = '{sCube}'
						        AND A.ENTITY IN ({allFCs})
						        AND A.SCENARIO = '{sScenario}'
						)
						SELECT Status.ENTITY, Status.FLOW, Status.STATUS AS STATUS, B.Text AS TITLE 
						FROM Status  
						join Title B ON 
						         Status.CUBE = B.CUBE 
						         AND Status.ENTITY = B.ENTITY 
						         AND Status.SCENARIO = B.SCENARIO 
						         AND Status.TIME = B.TIME 
						         AND Status.FLOW = B.FLOW ")
				
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
'Return Nothing

				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntity")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
'BRApi.ErrorLog.LogMessage(si, "dt all rows " & dtAll.Rows.Count)
				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("ENTITY")
					Dim ExistingREQ As String =  dataRow.Item("FLOW")
					Dim TitleValue As String = ""'dataRow.Item("TITLE")
					If dbnull.Value.Equals(dataRow.Item("TITLE")) Then
						TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
					Else
						 TitleValue  =  dataRow.Item("TITLE")
					End If 

					Dim REQStatus As String =  ""'dataRow.Item("STATUS")
					
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
					
					'This is to account for instances where Status is set to blank by the manager.
					'Not a valid business process. But OS exposes 'No Data' option				
					If dataRow.IsNull(("STATUS")) Then	
						'RMW-1028 - To address the blank status issue, REQ history will be parsed to find the previous FC level the REQ was on. A new status will be set to that FC level and an additional error status will be appended after.
						'Member scripts may also be encapsulated in the method calls, but is currently done here to reduce duplicate code.
						Dim REQStatusHistoryScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim REQStatusScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & ExistingREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						'Parses REQ history to get the previous FC level
						Dim lastFcLevel As String = GetLastFundCenterLevel(si, sCube, REQStatusHistoryScript)
						'REQStatus will be what is set in the db and also how the WF filter below will determine which CV's can see the REQ
						REQStatus = lastFcLevel & " NoStatus"
						'Since the current status in the db is null, set it to the desired text
						SetREQStatusFromNull(si, sCube, REQStatusScript, REQStatus)							
					Else
						REQStatus = dataRow.Item("STATUS")									
					End If
					'--------- get REQ workflow status level --------- 
					Dim reqWFStatusLevel As String = REQStatus.Substring(0,2)	
'BRApi.ErrorLog.LogMessage(si, "wfstatus " & reqWFStatusLevel)					
					If String.IsNullOrWhiteSpace(TitleValue) Then Continue For
'BRApi.ErrorLog.LogMessage(si, "Fund Center " & FundCenter)	
					'================================Non-CMD level Manage WF======================		
					If (sProfileName = "Manage Requirements") Or (sProfileName = "Manage Requirements" And REQStatus.XFEqualsIgnoreCase("BlankStatus")) Then	
						If sCubeView.XFEqualsIgnoreCase("REQ_Manage_Demote") Then
							If REQStatus.XFEqualsIgnoreCase("Returned From " & newWFLevel) Or  REQStatus.XFEqualsIgnoreCase("Returned From " & reqWFStatusLevel) Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)	
							End If
						Else 
							If Not REQStatus.XFEqualsIgnoreCase("Returned From CMD") And Not REQStatus.XFEqualsIgnoreCase("Ready for CMD Prioritization") And Not REQStatus.XFEqualsIgnoreCase("CMD Prioritized") And Not REQStatus.XFEqualsIgnoreCase("CMD Approved") And Not REQStatus.XFEqualsIgnoreCase("Returned From L2")  And Not REQStatus.XFEqualsIgnoreCase("L2 Working") And Not REQStatus.XFEqualsIgnoreCase("L2 Copied")Then
								If reqWFStatusLevel = entityText3 Or (REQStatus.XFEqualsIgnoreCase(newWFLevel & " Ready for Validation")) Then			
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
								End If
							End If
						End If
					'================================Command Manage WF======================	
					'*** not necessary?
					ElseIf ( sProfileName = "Manage Requirements CMD") Or ( sProfileName = "Manage Requirements CMD") And REQStatus.XFEqualsIgnoreCase("BlankStatus") Then
						If sCubeView.XFEqualsIgnoreCase("REQ_Manage_Demote") Then
							If REQStatus.XFEqualsIgnoreCase("Returned From CMD") Or REQStatus.XFEqualsIgnoreCase("Returned From L2") Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)	
							End If
						Else 
					'***
							If REQStatus.XFEqualsIgnoreCase("L2 Ready for Prioritization") Or REQStatus.XFEqualsIgnoreCase("L2 Prioritized") Or REQStatus.XFEqualsIgnoreCase("L2 Ready for Validation" ) Or REQStatus.XFEqualsIgnoreCase("L2 Ready for Approval" ) Or REQStatus.XFEqualsIgnoreCase("L2 Approved" ) Or REQStatus.XFEqualsIgnoreCase("L2 Working") And Not ExistingREQ.XFContainsIgnoreCase("REQ_00") Then 			
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
						End If							
					'================================Formulate WF======================	
					Else If sProfileName = "Formulate Requirements" And Not sCubeView.XFContainsIgnoreCase("REQ") Then
							If (REQStatus.XFContainsIgnoreCase("Working") Or REQStatus.XFContainsIgnoreCase("Copied") Or REQStatus.XFContainsIgnoreCase("Imported")) And Not REQStatus.XFContainsIgnoreCase("L2")  Then  'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If		

					'================================Formulate CMD WF======================	
					Else If sProfileName = "Formulate Requirements CMD" And Not sCubeView.XFContainsIgnoreCase("REQ") Then
							If (REQStatus = "L2 Working" Or REQStatus = "L2 Copied" Or REQStatus = "L2 Imported") And Not ExistingREQ.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
							If (REQStatus = "L2 Working" Or REQStatus = "L2 Approved") 
								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingREQ.XFContainsIgnoreCase("REQ_00") Then
       								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue,ExistingREQ)
								End If 
							End If			
					'================================Validate REQs WF======================			
					ElseIf sProfileName = "Validate Requirements" Then
							If reqWFStatusLevel = entityText3 And REQStatus.Contains("Ready for Validation") Then								
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
							End If
					'================================Revalidate REQ WF======================		
					ElseIf sProfileName = "Validate Requirements CMD" Then
						If REQStatus = ("L2 Ready for Validation")  Then 'updated
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If 	
					'================================Review Requirements WF======================			
					ElseIf wfProfileName.XFContainsIgnoreCase("Review Requirements") Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - "  & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated 		
					'================================Prioritize WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Prioritize") Then
						If REQStatus.XFContainsIgnoreCase("Prioritized")  Then 'updated
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If
					'================================Approve WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Approve") Then
						If REQStatus.XFContainsIgnoreCase("Approval") And reqWFStatusLevel = entityText3 Then 'updated
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If
					End If 
'					'================================Review WF======================	
'					If  (wfProfileName.XFContainsIgnoreCase("Review") And REQStatus.XFContainsIgnoreCase(wflevel) ) Or (wfProfileName.XFContainsIgnoreCase("Review") And FundCenter = sCurrentFc ) Then 
				
'							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ) 'updated

					'================================Review Financials WF======================	
'					ElseIf sProfileName = "Review Financials" Then
'							If REQStatus = "Ready for Financial Review"  Then 'updated
'									Me.WriteNameValuePairRow(si, dt, FundCenter & " - "  & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
'							End If 
				Next
'brapi.ErrorLog.LogMessage(si,"END: "	& System.DateTime.Now)
'If si.UserName.XFEqualsIgnoreCase("yhussien") Then BRapi.ErrorLog.LogMessage(si, Date.Now().ToString("hh:mm:ss:fff") &  " - REQListByEntity took: " & Date.Now().Subtract(tStart).TotalSeconds.ToString("0.0000"))

				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region  'Updated

#Region "REQListByEntityAndStatus"
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function REQListByEntityAndStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try		
Dim sCaller As String = args.NameValuePairs.XFGetValue("Caller")
				
'BRApi.ErrorLog.LogMessage(si, "Debug REQListByEntityAndStatus")	
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntityAndStatus")				
				'Dim sTitleScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim sTitleYear As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name

'Brapi.ErrorLog.LogMessage(si,wfProfileName & "Debug WFProfile DS")				
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
'brapi.ErrorLog.LogMessage(si,"sProfileSubString: " & sProfileSubString)							
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTitleSearch As String = args.NameValuePairs.XFGetValue("TitleSearch", "")
				Dim sCPA As String = args.NameValuePairs.XFGetValue("CPAFilter", "")
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode")
'				Dim oEFList As New List(Of String)
'brapi.ErrorLog.LogMessage(si, sREQTime)
				
'BRApi.ErrorLog.LogMessage(si, "Debug A1")
				'If no fund center is passed then stop

'If (si.UserName.XFEqualsIgnoreCase("yhussien") Or si.UserName.XFEqualsIgnoreCase("akalwa")) Then BRApi.ErrorLog.LogMessage(si, "REQ_DataSet.REQListByEntityAndStatus - " & sCaller & ":   sFundCenter=" & sFundCenter)				
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Return dt
				End If
				
				'If the fund center passed is not a descendent of the WF then stop
				sFundCenter = sFundCenter.Replace("_General","")
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter & ")"
				Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )

'If (si.UserName.XFEqualsIgnoreCase("yhussien") Or si.UserName.XFEqualsIgnoreCase("akalwa")) Then BRApi.ErrorLog.LogMessage(si, "REQ_DataSet.REQListByEntityAndStatus - " & sCaller & ":   sCube=" & sCube & "    mbrScrpt=" & mbrScrpt)				
				If Not cbMembers.Count > 0 Then
					Return dt
				End If
				
								
				'---Checking if Base------

				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter.Replace("_General",""))						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				If Not isBase Then sFundCenter = sFundCenter & ".Base"
	
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundCenter,True)
			
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
				Dim prevWFLevel As String = "L" & (icurrText3Num + 1)
				Dim curWFLevel As String = entityText3
				Dim sCubeView As String = args.NameValuePairs.XFGetValue("CubeView")				
'			Brapi.ErrorLog.LogMessage(si,"NextWFL" & newWFLevel)
'			Brapi.ErrorLog.LogMessage(si,"CurrWFL" & curWFLevel)
'			Brapi.ErrorLog.LogMessage(si,"PrevWFL" & prevWFLevel)
				'========================================================================== Define Targeted Status based on WF and Dashboard's FC selection ==========================================================================
				Dim sTgtStatus As String = ""
				Select Case True
				Case sProfileName = "Manage Requirements"				
					If sCubeView.XFEqualsIgnoreCase("REQ_Manage_Demote") Then					
						sTgtStatus = $"'{curWFLevel} Returned From {newWFLevel}','Returned From {curWFLevel}'"
					Else 
						sTgtStatus = $"'{curWFLevel} Working','{curWFLevel} NoStatus','{curWFLevel} Ready For Validation','{curWFLevel} Ready For Prioritization','{curWFLevel} Prioritized','{curWFLevel} Ready For Approval','{newWFLevel} Ready For Validation','{curWFLevel} Copied','{curWFLevel} Imported'"
						'If isBase = True Then sTgtStatus = $"{sTgtStatus},'{curWFLevel} Working'"
					End If
					
				Case sProfileName = "Manage Requirements CMD"
					If sCubeView.XFEqualsIgnoreCase("REQ_Manage_Demote") Then					
						sTgtStatus = $"'Returned From CMD','Returned From L2'" 
						Else
						sTgtStatus = $"'{curWFLevel} Ready For Prioritization','{curWFLevel} NoStatus','{curWFLevel} Prioritized','{curWFLevel} Ready For Approval','{curWFLevel} Approved','{curWFLevel} Ready For Validation','{curWFLevel} Copied','{curWFLevel} Imported','{curWFLevel} Working'"
					End If
					
				Case sProfileName.XFContainsIgnoreCase("Formulate")
'					If sCubeView.XFContainsIgnoreCase("REQ") And Not sCubeView.XFContainsIgnoreCase("REQ_List_Import") Then
					If Not sCubeView.XFContainsIgnoreCase("REQ")
						sTgtStatus = $"'{curWFLevel} Working','{curWFLevel} Copied','{curWFLevel} Imported'"
					Else If sCubeView.XFContainsIgnoreCase("REQ_List_Import") Then 
						sTgtStatus = $"'{curWFLevel} Imported'"						
					Else 
						Return Nothing
					End If
				
				Case wfProfileName.XFContainsIgnoreCase("Rollover")
					sREQTime = (Convert.ToInt32(sREQTime) - 1).ToString
					sScenario = $"PGM_C{sREQTime}"
'BRApi.ErrorLog.LogMessage(si, "scenario from dataset" & sScenario)						
				Case sProfileName = "Validate Requirements"
					sTgtStatus = $"'{curWFLevel} Ready For Validation'"
				
				Case sProfileName = "Validate Requirements CMD"
					sTgtStatus = $"'{curWFLevel} Ready For Validation'"
				
				Case wfProfileName.XFContainsIgnoreCase("Prioritize")
					sTgtStatus = $"'{curWFLevel} Ready For Prioritization','{curWFLevel} Prioritized'"
				
				Case wfProfileName.XFContainsIgnoreCase("Approve")
					sTgtStatus = $"'{curWFLevel} Ready For Approval'"
				
				'Case wfProfileName.XFContainsIgnoreCase("CMD (PGM)")
					'sTgtStatus = $"'{curWFLevel} Approved'"	
				
				Case wfProfileName.XFContainsIgnoreCase("Review")
					sTgtStatus = $"'% Ready For Prioritization','% Prioritized','% Ready For Approval','% Approved','% Ready For Validation','% Working','% Returned From %','%Copied','%Imported','Returned From %'"
				
				Case wfProfileName.XFContainsIgnoreCase("CMD (PGM)")
					sTgtStatus = $"'{curWFLevel} Approved'"	
					
'BRApi.ErrorLog.LogMessage(si, "stat declared for review")	
				End Select
'BRApi.ErrorLog.LogMessage(si, "statuses: " & sTgtStatus)	
'BRApi.ErrorLog.LogMessage(si, "profile name: " & wfProfileName)
				
'				========= Manage
'-{curWFLevel} Ready For Validation,{curWFLevel} Ready For Prioritization,{curWFLevel} Prioritized,{curWFLevel} Ready For Approval,{newWFLevel} Ready For Validation
'- need a check For non-parent To include  {curWFLevel} Working

'========= CMD Manage
'- "Return From CMD" IIf cv = Demote
'-{curWFLevel} Ready For Prioritization,{curWFLevel} Prioritized,{curWFLevel} Ready For Approval,{curWFLevel} Approved,{curWFLevel} Ready For Validation

'========= Formulate
'- need a check For non-parent {curWFLevel} working

'========= Validate
'-{curWFLevel} Ready For Validation


'========= CMD Validate
'-{curWFLevel} Ready For Validation


'========= Prioritize
'-{curWFLevel} Ready For Prioritization,{curWFLevel} Prioritized


'========= Approve
'-{curWFLevel} Ready For Approval

				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allREQs As String = ""
				Dim allFCs As String = ""
				
'If (si.UserName.XFEqualsIgnoreCase("yhussien") Or si.UserName.XFEqualsIgnoreCase("akalwa")) Then BRApi.ErrorLog.LogMessage(si, "REQ_DataSet.REQListByEntityAndStatus - " & sCaller & ":   LFundCenters.Count=" & LFundCenters.Count)								
				If LFundCenters.Count = 0 Then
					Return dt
				End If
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next

				allFCs = allFCs.Substring(0,allFCs.Length-1)
			
				Dim SQL As New Text.StringBuilder
'				SQL.Append($"SELECT A.ENTITY AS ENTITY, A.FLOW AS FLOW, A.TEXT AS STATUS, B.TEXT AS TITLE ") 
'				SQL.Append("FROM DATAATTACHMENT A LEFT JOIN DATAATTACHMENT B ")
'				SQL.Append($" ON B.CUBE = A.CUBE 
'							 AND B.ENTITY = A.ENTITY 
'							 AND B.SCENARIO = A.SCENARIO
'							 AND B.TIME = A.TIME
'							 AND B.FLOW = A.FLOW ") 
'				SQL.Append($" LEFT JOIN (SELECT ENTITY, SCENARIO, TIME, FLOW, TEXT AS CPA FROM DATAATTACHMENT WHERE ACCOUNT = 'REQ_CPA_IND' ) AS C
'							 ON C.ENTITY = A.ENTITY
'							 AND C.SCENARIO = A.SCENARIO
'							 AND C.TIME = A.TIME
'							 AND C.FLOW = A.FLOW
'							 WHERE A.ACCOUNT = 'REQ_RQMT_STATUS'
'							 AND A.CUBE = '{sCube}'
'							 AND A.ENTITY IN ({allFCs}) 
'							 AND A.SCENARIO = '{sScenario}'
'							 AND A.TIME = '{sREQTime}'				 
'							 AND B.ACCOUNT = 'REQ_TITLE'")
'----------------------------------------

SQL.Append($"
With B as (
	SELECT B.cube as CUBE, B.scenario as SCENARIO, B.time as TIME, B.ENTITY AS ENTITY, B.FLOW AS FLOW,  B.TEXT AS TITLE 
	FROM DATAATTACHMENT B 
	Where B.ACCOUNT = 'REQ_TITLE'
	 AND B.CUBE = '{sCube}'
	 AND B.ENTITY IN ({allFCs}) 
	 AND B.SCENARIO = '{sScenario}'
	 AND B.TIME = '{sREQTime}'	
	
),
A as (
	SELECT  A.cube as CUBE, A.scenario as SCENARIO, A.time as TIME, A.ENTITY AS ENTITY, A.FLOW AS FLOW,  A.TEXT AS TEXT 
	from DATAATTACHMENT A
	WHERE		
	A.ACCOUNT = 'REQ_RQMT_STATUS'
 	 AND A.CUBE = '{sCube}'
	 AND A.ENTITY IN ({allFCs}) 
	 AND A.SCENARIO = '{sScenario}'
	 AND A.TIME = '{sREQTime}'	

 ),
 C AS(
	SELECT  CPA.cube as CUBE, CPA.scenario as SCENARIO, CPA.time as TIME, CPA.ENTITY AS ENTITY, CPA.FLOW AS FLOW,  CPA.TEXT AS CPA 
	from DATAATTACHMENT CPA
	WHERE		
	 CPA.ACCOUNT = 'REQ_CPA_INDa'
	 AND CPA.CUBE = '{sCube}'
	 AND CPA.ENTITY IN ({allFCs}) 
	 AND CPA.SCENARIO = '{sScenario}'
	 AND CPA.TIME = '{sREQTime}'	
 )
 SELECT B.ENTITY AS ENTITY, B.FLOW AS FLOW, A.TEXT AS STATUS, B.Title AS TITLE 
 from B 
 Left outer join A  ON B.CUBE = A.CUBE 
					 AND B.ENTITY = A.ENTITY 
					 AND B.SCENARIO = A.SCENARIO
					 AND B.TIME = A.TIME
					 AND B.FLOW = A.FLOW  
 Left outer join C  ON B.CUBE = A.CUBE 
							 AND B.ENTITY = C.ENTITY 
							 AND B.SCENARIO = C.SCENARIO
							 AND B.TIME = C.TIME
							 AND B.FLOW = C.FLOW  
where 1=1
")


'--------------------------------------------
'BRApi.ErrorLog.LogMessage(si, "CPA " & sCPA)
				If sProfileName.XFContainsIgnoreCase("Manage") Then
					SQL.Append($"
								 AND (A.TEXT IN ({sTgtStatus}) OR A.TEXT IS NULL)")
				Else If sProfileName.XFContainsIgnoreCase("Review") Then
'BRApi.ErrorLog.LogMessage(si, "CPA" & sCPA)
					Dim oStatus As List(Of String) = sTgtStatus.Split(",").ToList()
					SQL.Append($"
								AND (A.TEXT IS NULL ")
						For Each status As String In oStatus 
							SQL.Append($"OR A.TEXT LIKE {status} ")
'BRApi.ErrorLog.LogMessage(si, "SQL CPA: " & sCPA)
						Next
							SQL.Append(")")
				Else If sProfileName.XFContainsIgnoreCase("Rollover") Then
				Else
					SQL.Append($"
								 AND A.TEXT IN ({sTgtStatus})")
				End If
				If Not String.IsNullOrWhiteSpace(sTitleSearch) Then
					SQL.Append($"
								 AND B.Title LIKE '%{sTitleSearch}%'")
				End If
'Brapi.ErrorLog.LogMessage(si, "CPA - " & sCPA)			
				If Not String.IsNullOrWhiteSpace(sCPA)
					If (Not sCPA.XFContainsIgnoreCase(",")) Then 
					
						'Filter Yes
						If sCPA.XFEqualsIgnoreCase("Yes") Then
						
						SQL.Append($"
									AND CPA LIKE '%{sCPA}%'")	
						
	'					ElseIf sCPA.XFContainsIgnoreCase("%Yes, No%")						
	'						SQL.Append($"
	'								 AND CPA in('Yes','No') or CPA is Null")
							
						'Filters No
						Else
							SQL.Append($"
									 AND (CPA LIKE '%{sCPA}%' or CPA is Null)")
						End If
					End If
				End If

				SQL.Append($"
							 ORDER BY A.ENTITY")
'If (si.UserName.XFEqualsIgnoreCase("yhussien") Or si.UserName.XFEqualsIgnoreCase("akalwa")) Then BRApi.ErrorLog.LogMessage(si, "REQ_DataSet.REQListByEntityAndStatus - " & sCaller & ":   SQL: " & SQL.ToString)
'If si.UserName.XFEqualsIgnoreCase("rdavies") Then BRapi.ErrorLog.LogMessage(si,"START SQL: " & " " & Date.Now().ToString("hh:mm:ss:fff") )						

				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntityAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'If si.UserName.XFEqualsIgnoreCase("rdavies") Then BRapi.ErrorLog.LogMessage(si,"START dbConn: " & " " & Date.Now().ToString("hh:mm:ss:fff") )						
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'If si.UserName.XFEqualsIgnoreCase("rdavies") Then BRapi.ErrorLog.LogMessage(si,"END dbConn: " & " " & Date.Now().ToString("hh:mm:ss:fff") )						
				End Using
				 
'If si.UserName.XFEqualsIgnoreCase("rdavies") Then BRapi.ErrorLog.LogMessage(si,"END SQL: " & " " & Date.Now().ToString("hh:mm:ss:fff") )						

'If (si.UserName.XFEqualsIgnoreCase("yhussien") Or si.UserName.XFEqualsIgnoreCase("akalwa")) Then BRApi.ErrorLog.LogMessage(si, "REQ_DataSet.REQListByEntityAndStatus - " & sCaller & ":   dt all rows " & dtAll.Rows.Count)
				'Build dt to return 

				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("ENTITY")
					Dim ExistingREQ As String =  dataRow.Item("FLOW")
					Dim TitleValue As String = dataRow.Item("TITLE")
					

'BRApi.ErrorLog.LogMessage(si, $"E = {FundCenter} || F = {ExistingREQ} ")
					Dim REQStatus As String =  ""'dataRow.Item("STATUS")
		
					'This is to account for instances where Status is set to blank by the manager.
					'Not a valid business process. But OS exposes 'No Data' option				
					If dataRow.IsNull(("STATUS")) Then	
						'RMW-1028 - To address the blank status issue, REQ history will be parsed to find the previous FC level the REQ was on. A new status will be set to that FC level and an additional error status will be appended after.
						'Member scripts may also be encapsulated in the method calls, but is currently done here to reduce duplicate code.
						Dim REQStatusHistoryScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim REQStatusScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & ExistingREQ & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						'Parses REQ history to get the previous FC level
						Dim lastFcLevel As String = GetLastFundCenterLevel(si, sCube, REQStatusHistoryScript)
						'REQStatus will be what is set in the db and also how the WF filter below will determine which CV's can see the REQ
						REQStatus = lastFcLevel & " NoStatus"
						'Since the current status in the db is null, set it to the desired text
						SetREQStatusFromNull(si, sCube, REQStatusScript, REQStatus)							
					Else
						REQStatus = dataRow.Item("STATUS")									
					End If	
					
'					oEFList.Add($"E#{FundCenter}:F#{ExistingREQ}")
					Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)

				Next

'BRApi.ErrorLog.LogMessage(si, $"dt Count = {dt.Rows.Count}")
				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region  'New

#Region "Get UFR Stakeholder POCs"	
		'RMW-1083 - This function returns appropriate U5 members for Stakeholders depending on whther workflow is Lv1 or not
'		Public Function GetUFRStkhldrPOCs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
'			Try
'				'Get WF Profile Level
'				Dim sWfProfile As String = args.NameValuePairs.XFGetValue("wfProfile")
'				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'				Dim wfLevel As String = wfProfileName.Substring(0,2)

'				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "U5_Main") 
'				Dim sParentMbr As String = ""
'				'Determine Parent-level member
'				If wfLevel = "L1" Then
'					sParentMbr = "HQDA_UFR_Stkhldr_POCs"
'				Else
'					sParentMbr = "NonHQDA_UFR_Stkhldr_POCs"
'				End If
'				'Get Children members
'				Dim iMbrId As Integer =  BRApi.Finance.Members.GetMemberId(si, DimType.UD5.Id, sParentMbr)
'				Dim oChildrenMbrs As List(Of Member) = BRApi.Finance.Members.GetChildren(si, DimPK, iMbrId)
'				Dim sMbrDesc As String = ""
'				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "UFR_Stkhldr_POCs")
				
'				For Each child As Member In oChildrenMbrs
'					Me.WriteNameValuePairRow(si, dt, child.Description, child.Name)
'				Next
				
'				Return dt
'			Catch ex As Exception
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'		End Function

#End Region 'Commneted out

#Region "Get List of File Names in Folder"	
		'Get List of File Names in Folder
		Public Function GetFileNamesInFolder(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs)
			Try		
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sFolder As String = $"Documents/Public/CMD_Programming/{wfcube}"
				Dim objList As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sFolder, XFFileType.All, False, False, False)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ListOfFiles")
				For Each item As NameAndAccessLevel In objList
					Dim sFileName As String = item.Name.Substring(item.Name.LastIndexOf("/") + 1)
					Dim sFileNameLower As String = sFileName.ToLower
					If sFileNameLower.StartsWith(wfCube.ToLower)  And Not sFileNameLower.EndsWith("pdf") Then
						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
					End If
'					If Not wfCube.XFContainsIgnoreCase("Army") And sFileNameLower.StartsWith("Army".ToLower) Then
'						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
'					End If
				Next
				sFolder = $"Documents/Public/CMD_Programming/ARMY"
				Dim objListArmy As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sFolder, XFFileType.All, False, False, False)
				For Each item As NameAndAccessLevel In objListArmy
					Dim sFileName As String = item.Name.Substring(item.Name.LastIndexOf("/") + 1)
					Dim sFileNameLower As String = sFileName.ToLower
					If Not wfCube.XFContainsIgnoreCase("Army") And sFileNameLower.StartsWith("Army".ToLower) And Not sFileNameLower.EndsWith("pdf") Then
						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
					End If
				Next
				Return dt

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region  'New

#Region "Get List of REQs for Submission"	
		'Get List of File Names in Folder
		Public Function REQListToSubmit(ByVal si As SessionInfo, ByVal globals As BRGlobals, Optional ByVal api As Object = Nothing, Optional ByVal args As DashboardDataSetArgs = Nothing)
			Try		
				Dim oReqList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"REQListCVResult")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListToSubmit")
				For Each row As DataRow In oReqList.Rows
					Dim sEntity As String = row("EntityFlow").Split(":")(0).Replace("e#[","").Replace("]","").ToUpper()
					Dim sFlow As String = row("EntityFlow").Split(":")(1).Replace("f#[","").Replace("]","").ToUpper()
					Me.WriteNameValuePairRow(si, dt, sEntity & " - " & sFlow, sEntity & " - " & sFlow)
				Next
				
				Return dt

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "REQWFStatusList"

		Public Function REQWFStatusList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				
		        Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario")
				Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType")
				Dim sCommand As String = args.NameValuePairs.XFGetValue("Cube")
				Dim sOrigFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sCube As String = ""
				Dim CommandParents As list(Of member) = brapi.Finance.Members.GetParents(si,brapi.finance.Dim.GetDimPk(si,"E_ARMY"),brapi.Finance.Members.GetMemberId(si,DimType.Entity.Id,sCommand),False)
				For Each CommandParent As Member In CommandParents	
					If Not String.IsNullOrEmpty(CommandParent.Name)
						sCube = CommandParent.Name
						Exit For
					End If 
				Next
				
				Dim SQL As New Text.StringBuilder
				SQL.Append($"
					WITH sCurrLevel AS
						(
							SELECT DISTINCT [Cube]
						      ,[Entity] as SourceFundCenter
						      ,[Scenario]
						      ,[Time]
						      ,[Flow]
						      ,[Text]
							  ,case when [text] like 'L%' then SUBSTRING([Text],1,CHARINDEX(' ',[Text],1)-1) else null end as CurrLevel
						  FROM DATAATTACHMENT 
						  Where Scenario = '{sScenario}'
						  and Account in ('REQ_Rqmt_Status')
						)
						SELECT sMain.[Cube]
						      ,sMain.[Entity] as SourceFundCenter
						      ,sMain.[Scenario]
						      ,sMain.[Time]
						      ,sMain.[Flow]
						      ,sMain.[Account]
							  ,(CASE 
								WHEN sMain.[Account] = 'REQ_Title' THEN 1
								WHEN sMain.[Account] = 'REQ_Rqmt_Status' THEN 2
								WHEN sMain.[Account] in ('REQ_Creation_Date_Time','REQ_Creator_Name') THEN 3
								WHEN sMain.[Account] in ('REQ_Last_Updated_Name','REQ_Last_Updated_Name') THEN 4
								ELSE 5 END) as AccountSort
						      ,sMain.[Text]
							  ,sCurrLevel.[CurrLevel]
						  FROM DATAATTACHMENT  as sMain
						  LEFT OUTER JOIN sCurrLevel on sMain.Entity = sCurrLevel.SourceFundCenter and sMain.Scenario = sCurrLevel.Scenario and sMain.Time = sCurrLevel.Time and sMain.Flow = sCurrLevel.Flow
						  Where sMain.Scenario = '{sScenario}'
						  and sMain.cube = '{sCube}'
						  And sMain.Account In ('REQ_Title','REQ_Last_Updated_Date','REQ_Last_Updated_Name','REQ_Creation_Date_Time','REQ_Creator_Name','REQ_Rqmt_Status')
						ORDER BY 1,2,3,4,5,6
						;
						  ")
				
				Dim REQCountSQL As New Text.StringBuilder
				REQCountSQL.Append($"
					select
						Entity,
						text,
						Count(*) as ReqCount
					FROM 
						DATAATTACHMENT 
					where
						account = 'REQ_Rqmt_Status'
					and
						scenario = '{sScenario}'
					and 
							cube = '{sCube}'
					Group By entity, text ;
					")

				Dim dtAll As New DataTable("REQWFStatusListAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					If sReportType.XFEqualsIgnoreCase("REQ_List") Then
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				 	End If
				End Using
					 Dim dtList As New DataTable("REQWFStatusListReduced")
					dtList.Columns.Add("Scenario", GetType(String))
					dtList.Columns.Add("SourceFundCenter", GetType(String))
					dtList.Columns.Add("CurrFundCener", GetType(String))
					dtList.Columns.Add("Flow", GetType(String))
					dtList.Columns.Add("Account", GetType(String))
					dtList.Columns.Add("Text", GetType(String))
					dtList.Columns.Add("CurrLevel", GetType(String))
					 For Each dataRow As DataRow In dtAll.Rows	
						
						Dim dtCurrLevel As String = ""
						If dbnull.Value.Equals(dataRow.Item("CurrLevel")) Then
						dtCurrLevel = "Bad Data"
						Else
						 dtCurrLevel  =  dataRow.Item("CurrLevel")
						End If
						Dim dtScenario As String = dataRow.Item("scenario")
						Dim dtREQ As String =  dataRow.Item("flow")
						Dim dtText As String =  dataRow.Item("Text")
						Dim dtSourceFC As String =  dataRow.Item("sourcefundcenter")
						Dim dtAccount As String =  dataRow.Item("account")
						
						
						Dim wfScenario As String = args.NameValuePairs.XFGetValue("Scenario")
						Dim wfScenarioTypeID As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.Scenario.Id,wfScenario)
						Dim wfTime As Integer = BRApi.Finance.Scenario.GetWorkflowTime(si,wfScenarioTypeID)   
						Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
						Dim entityText3 As String = BRApi.Finance.Entity.Text(si, brapi.Finance.Members.GetMemberId(si,DimType.Entity.Id,dtSourceFC),3, wfScenarioTypeID, wfTimeId)
						Dim entityLevel As String = entityText3.Substring(entitytext3.IndexOf("=")+1,2)
						
						If entityLevel.XFEqualsIgnoreCase(dtCurrLevel) Then						
							dtList.Rows.Add(dtScenario, dtSourceFC, dtSourceFC,dtREQ,dtAccount,dtText,dtCurrLevel)	
						Else
							Dim EntityParents As list(Of member) = brapi.Finance.Members.GetAncestors(si,brapi.finance.Dim.GetDimPk(si,"E_ARMY"),brapi.Finance.Members.GetMemberId(si,DimType.Entity.Id,dtSourceFC),False)			
							For Each sParent As Member In EntityParents
								Dim ParentText3 As String = BRApi.Finance.Entity.Text(si, brapi.Finance.Members.GetMemberId(si,DimType.Entity.Id,sParent.Name),3, wfScenarioTypeID, wfTimeId)
								'Check to ensure text 3 is populated before substring to avoid error
								If String.IsNullOrEmpty(ParentText3) Then
										Continue For 
								End If
								Dim ParentLevel As String = ParentText3.Substring(ParentText3.IndexOf("=")+1,2)
									If ParentLevel.XFEqualsIgnoreCase(dtCurrLevel) Then
										dtList.Rows.Add(dtScenario, dtSourceFC, sParent.Name,dtREQ,dtAccount,dtText,dtCurrLevel)	
										Exit For	
									End If 
								Next
						End If 
					Next
				 Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 If sReportType.XFEqualsIgnoreCase("REQ_Status") Then
					 dtAll = BRApi.Database.ExecuteSql(dbConn,REQCountSQL.ToString(),True)
					 Return dtAll
				 End If 
					End Using
				
				Return dtList 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Get Fund Centers By Command"
		'This function all fundcenters in the data attachment table that have a title account populated.
		'It used from the batch delete 
		Public Function GetFundCentersByCommand(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				
		        Dim sCommand As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				
				Dim SQL As New Text.StringBuilder
				SQL.Append($"
						SELECT DISTINCT [Entity] as FundCenter
						   FROM DATAATTACHMENT 
						   Where Scenario = '{sScenario}'
						   and Cube = '{sCommand}'
						   and Account = 'REQ_Title'
						;
						  ")

				Dim dtAll As New DataTable("FundCenters")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using

				Dim FundCenterByCommand As DataTable = Me.CreateNameValuePairTable(si, "FundCenterByCommand") 
				For Each r As DataRow In dtAll.Rows
					Me.WriteNameValuePairRow(si, FundCenterByCommand, r.item(0), r.item("FundCenter"))
				Next
				Return FundCenterByCommand 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Get REQs to be Deleted"
		'This function all fundcenters in the data attachment table that have a title account populated.
		'It used from the batch delete 
		Public Function GetREQsToBeDeleted(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				
		        Dim sCommand As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
				Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim FC = args.NameValuePairs.XFGetValue("FundCenter")
				Dim SQL As New Text.StringBuilder
				SQL.Append($"
					WITH REQTitles AS
					  (
					   SELECT DISTINCT [Cube]
							,[Entity] as SourceFundCenter
							,[Scenario]
							,[Time]
							,[Flow]
							,[Text] as REQTitle
						FROM DATAATTACHMENT 
						Where Scenario = '{sScenario}'
							  and Cube = '{sCommand}'
							  and Account in ('REQ_Title')
							  and [Entity] = '{FC}'
					  )
					  SELECT REQTitles.[REQTitle]
						,REQTitles.[Flow] as FlowId
						,REQIDs.[Text] as REQID
						,(Select REQStatus.[Text] from DATAATTACHMENT REQStatus where REQStatus.Entity = REQTitles.SourceFundCenter and REQStatus.Scenario = REQTitles.Scenario and REQStatus.Time = REQTitles.Time and REQStatus.Flow = REQTitles.Flow and REQStatus.Account = 'REQ_Rqmt_Status' ) as STATUS 	
						FROM DATAATTACHMENT  as REQIDs
						JOIN REQTitles on REQIDs.Entity = REQTitles.SourceFundCenter and REQIDs.Scenario = REQTitles.Scenario and REQIDs.Time = REQTitles.Time and REQIDs.Flow = REQTitles.Flow
				 Where REQIDs.Account = 'REQ_ID' 
						;
						  ")

				Dim dtAll As New DataTable()
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using

				Dim sSelectAllVal As String = ""
				Dim REQsToBeDeleted As DataTable = Me.CreateNameValuePairTable(si, "REQsToBeDeleted") 
				For Each r As DataRow In dtAll.Rows
					Me.WriteNameValuePairRow(si, REQsToBeDeleted, r.item("REQTitle") & " - " & r.item("REQID") & " - " & r.item("FlowId") & " - " & r.item("STATUS"), r.item("FlowId")  )
					sSelectAllVal = $"{sSelectAllVal},{r.item("FlowId")}"
				Next

				If sSelectAllVal.length > 0 Then sSelectAllVal = sSelectAllVal.Substring(1)
				BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BatchDelete","DeleteAll",sSelectAllVal)
					
'BRApi.ErrorLog.LogMessage(si, "sSelectAllVal " & sSelectAllVal)					
				Return REQsToBeDeleted 	
				
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


	End Class
End Namespace


