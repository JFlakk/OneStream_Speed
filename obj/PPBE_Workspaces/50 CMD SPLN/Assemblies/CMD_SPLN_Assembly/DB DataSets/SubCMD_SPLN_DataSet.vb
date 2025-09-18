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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_SPLN_DataSet
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
						Select Case args.DataSetName
							Case args.DataSetName.XFEqualsIgnoreCase("BUDListByEntity")
								Return Me.BUDListByEntity(si,globals,api,args)			
							Case args.DataSetName.XFEqualsIgnoreCase("BUDListByEntityAndStatus")
								Return Me.BUDListByEntityAndStatus(si,globals,api,args)	
						End Select

#Region "BUDListCachedEntity: BUD List by Cached Entity"
'Updated to reference BUD Prefix - JM 11/18/24
						If args.DataSetName.XFEqualsIgnoreCase("BUDListCachedEntity") Then
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) &"M12"
							Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
							Dim ExistingBUDs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base",True).OrderBy(Function(x) x.Member.name).ToList()
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListCachedEntity")

								For Each ExistingBUD As MemberInfo In ExistingBUDs
						            Dim BUDEntityMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & ExistingBUD.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEntityMemberScript).DataCellEx.DataCellAnnotation
		               
		                            If Not String.IsNullOrWhiteSpace(TitleValue) Then
										Me.WriteNameValuePairRow(si, dt, sFundCenter & " - " & ExistingBUD.Member.Name & " - " & TitleValue, ExistingBUD.Member.Name)

									End If 

								Next

							Return dt 	
							
						End If


#End Region

#Region "BUDListForCopy: BUD List For Copy"
						'Return a datatable of Requirements to be used as copy source based on user's filter inputs
						'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
						'Updated to reference BUD Prefix - JM 11/18/24
						If args.DataSetName.XFEqualsIgnoreCase("BUDListForCopy") Then
							
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        'Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sScenario As String = args.NameValuePairs.XFGetValue("scenario",si.WorkflowClusterPk.ScenarioKey)
					        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
							Dim sFundCenter As String = args.NameValuePairs.XFGetValue("FC","")
							Dim	sAccount As String = args.NameValuePairs.XFGetValue("Account","")
							Dim	sAccountVal As String = args.NameValuePairs.XFGetValue("AccountVal","")
							Dim	sKeyword As String = args.NameValuePairs.XFGetValue("Keyword","")
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListForCopy")				
							
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
							SQL.Append($"WHERE ENTITY = '{sFundCenter}' AND REQ_TITLE IS NOT NULL")
							

						
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
								Dim ExistingBUD As String =  dataRow.Item("FLOW")	
								Dim TitleValue As String =  dataRow.Item("REQ_TITLE")
								If Not String.IsNullOrWhiteSpace(TitleValue) Then								
									Me.WriteNameValuePairRow(si, dt, sFundCenter & " - " & ExistingBUD & " - " & TitleValue, ExistingBUD)
								End If 
							Next
'BRApi.ErrorLog.LogMessage(si,"Debug F")			
							Return dt							
						End If


#End Region

#Region "BUDListToSubmit: Get BUDs for Submission"						
						If args.DataSetName.XFEqualsIgnoreCase("BUDListToSubmit") Then
'Brapi.ErrorLog.LogMessage(si,"public call")
							Return Me.BUDListToSubmit(si,globals,api,args)	
							
						End If						
#End Region

#Region "BUDWFStatusList: BUD WF Status List"
						If args.DataSetName.XFEqualsIgnoreCase("BUDWFStatusList") Then
							Return Me.BUDWFStatusList(si,globals,api,args)						
						End If
#End Region

#Region "Security Logic: GetUserFundCentersByWF: Get User Fund Centers By Workflow"						
						If args.DataSetName.XFEqualsIgnoreCase("GetUserFundCentersByWF") Then
							Return Me.GetUserFundCentersByWF(si,globals,api,args)	
							
						End If						
#End Region

#Region "FullBUDList: Full Related BUD List"
'Updated by Fronz 09/06/2024 - changed the S# to REQ_Shared and T# to 1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
'Updated to reference BUD Prefix - JM 11/18/24
						If args.DataSetName.XFEqualsIgnoreCase("FullBUDList") Then

							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
							Dim FundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
							Dim ExistingBUDs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base",True).OrderBy(Function(x) x.Member.name).ToList()
							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FullBUDList")

								'Checking and adding _General for upper levels'
								Dim pk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_" & sCube)

								Dim memberid As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FundCenter)					
								Dim parent As Boolean = BRApi.Finance.Members.HasChildren(si, pk, memberid)							
								If parent Then FundCenter = FundCenter & "_General"
								
								For Each ExistingBUD As MemberInfo In ExistingBUDs
						            Dim BUDEntityMemberScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Title:F#" & ExistingBUD.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, BUDEntityMemberScript).DataCellEx.DataCellAnnotation
		               
		                            If Not String.IsNullOrWhiteSpace(TitleValue) Then
										Me.WriteNameValuePairRow(si, dt, FundCenter.Replace("_General","") & " - " & ExistingBUD.Member.Name & " - " & TitleValue, FundCenter & " - " & ExistingBUD.Member.Name & " - " & TitleValue)

									End If 

								Next
							
							Return dt 	
							
						End If


#End Region

#Region "GetAllUsers: Get All Users"						
						If args.DataSetName.XFEqualsIgnoreCase("GetAllUsers") Then
'Brapi.ErrorLog.LogMessage(si,"public call")
							Return Me.GetAllUsers(si,globals,api,args)	
							
						End If						
#End Region 		

#Region "GetBUDStatus: Get BUD Status"						
						If args.DataSetName.XFEqualsIgnoreCase("GetBUDStatus") Then

							Return Me.GetBUDStatus(si,globals,api,args)	
							
						End If						
#End Region

#Region "GetFileNamesInFolder: Get List of File Names in Folder"
						'Return a list of files in a folder
						If args.DataSetName.XFEqualsIgnoreCase("GetFileNamesInFolder") Then

							Return Me.GetFileNamesInFolder(si,globals,api,args)	
							
						End If						
#End Region

#Region "ManpowerPBList: ManpowerPBList"
						If args.DataSetName.XFEqualsIgnoreCase("ManpowerPBList") Then
'BRApi.ErrorLog.LogMessage(si,"Debug F")								
							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        'Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sScenario As String = args.NameValuePairs.XFGetValue("scenario",si.WorkflowClusterPk.ScenarioKey)
					        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
							Dim sCurrYear As String = sBUDTime.Substring(2,2)
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

#Region "RePrioritizeBUDListByEntity: BUD List to Recall for Re-Prioritization by Entity"
						'RMW-1012 Return list of UFRs for Re-Prioritization
						If args.DataSetName.XFEqualsIgnoreCase("RePrioritizeBUDListByEntity") Then						
							Return Me.GetBUDListForRePrioritization(si,globals,api,args)
						End If
#End Region

#Region "StatusList: StatusList"
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
#End Region

#Region "Get Formulate Review Requirements"
						If args.DataSetName.XFEqualsIgnoreCase("GetFormulateReviewReqs") Then
							Return Me.GetFormulateReviewReqs(si,globals,api,args)						
						End If
#End Region

#Region "Get REQs to be Deleted"
						If args.DataSetName.XFEqualsIgnoreCase("GetREQsToBeDeleted") Then
							Return Me.GetREQsToBeDeleted(si,globals,api,args)						
						End If
#End Region

#Region "BUDListByEntityUpdateRequirement: Related BUDs By Selected Entity for Formulate Requirements And Validate Update Reqirements dashboard"
						If args.DataSetName.XFEqualsIgnoreCase("BUDListByEntityUpdateRequirement") Then
							Return Me.BUDListByEntityUpdateRequirement(si,globals,api,args)						
						End If
#End Region

#Region "Bud Review All REQs"
						If args.DataSetName.XFEqualsIgnoreCase("BUDReviewAllReqs") Then
							Return Me.BUDReviewAllReqs(si,globals,api,args)						
						End If
#End Region

#Region "Revise REQs By Target - Get Dimensional Members Data Set"
'=========================================================================================================================================================================================================================================
'==  WARNING: This method is used specifically for the Revise REQs By Target Linked Dashboard              																												     
'==           This method is used to return row headers for the Formulator to revise REQs' amount to match Funding Target   
'==              - Note: This function searches for specific inputted single member for each of the following dim: Entity, UD1, UD2, UD3 and return only Requirements with funding lines of this combination with the cuorrect Formulation statuses
'==           This method uses SQL to get the relevant information of requirements in correct status and return a datatable to be processed by BUD_Member_Lists > GetDimMbrsListReviseREQsByTarget
'=========================================================================================================================================================================================================================================
						If args.DataSetName.XFEqualsIgnoreCase("GetDimMbrsDataSet") Then
							Return Me.GetDimMbrsDataSet(si,globals,api,args)						
						End If
#End Region

			End Select

				Return Nothing
			Catch ex As Exception
'brapi.errorlog.LogMessage(si,"DataSetName = " & args.DataSetName)
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "===HELPER FUNCTIONS==="

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

#Region "BUDReviewAllReqs"
'Returning all BUD requirements
		Public Function BUDReviewAllReqs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try	
'Dim st As DateTime = DateTime.Now
'BRApi.ErrorLog.LogMessage(si, "In Grid view rule")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDReviewAllReqs")				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,7)						
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
		        Dim sBUDTimeM12 As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sCarryoverYrM12 As String = BRApi.Finance.Time.AddYears(si,si.WorkflowClusterPk.TimeKey,1).ToString.Substring(0,4) & "M12"
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTimePeriod As String = args.NameValuePairs.XFGetValue("TimePeriod","")
				Dim sAccount As String = args.NameValuePairs.XFGetValue("Account","")
				Dim lAccount As List(Of String) = sAccount.Split(", ").Select(Function(item)item.Trim()).ToList()
				lAccount.Sort()

				Dim dtAll As New DataTable 

				'If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Or String.IsNullOrWhiteSpace(sAccount) Then 
					Return dtAll
				End If
				
				'If the fund center passed is not a descendent of the WF then stop
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter.Replace("_General","") & ")"
				Dim lCubeMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
				
				If Not lCubeMembers.Count > 0 Then
					Return dt
				End If
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFundCenter).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = entityText3.Substring(1,1)
				Dim iCurrentLevel As Integer = CInt(currentStatus)
				Dim iNextLevel As Integer = iCurrentLevel - 1
				Dim currentWFLevel As String = "L" & iCurrentLevel
				Dim newWFLevel As String = "L" & iNextLevel				
				'--------- derive FC List ----------
				sFundCenter = sFundCenter.ToLower.Replace("_general","")
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				Dim lEntity As New List(Of String)
				Dim entitySQL As String = ""				
				If isBase = True Then 
					entitySQL = $"('{sFundCenter}')"	
					lEntity.Add(sFundCenter)
				Else
					Dim lFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, "E#"& sFundCenter &".Base",True)
					Dim isFirst As Boolean = True
					For Each FC As MemberInfo In lFundCenters						
						If isFirst Then
							entitySQL &= $"('{FC.Member.Name}')"
						Else
							entitySQL &= $", ('{FC.Member.Name}')"
						End If
						isFirst = False
						lEntity.Add(FC.Member.Name)
					Next			
				End If	

				'--------- derive status ----------
				Dim statusSQL As String = $"'{currentWFLevel}%'"
				'Return all reuirements regardless of status
				Dim SQL As New Text.StringBuilder
					'New
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   MAX(Case When ACCOUNT = 'REQ_TITLE' THEN TEXT END) AS 'Requirement Title', ")
					SQL.AppendLine($"   MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) AS 'Status' ")
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")
					SQL.AppendLine($"Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON DATAATTACHMENT.ENTITY = ENTITY_LIST.VALUE ")
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{sCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{sScenario}' ")
					SQL.AppendLine($"   And FLOW <> 'None' ")
'					SQL.AppendLine($"   And Exists (Select 1 from (VALUES {entitySQL}) AS ENTITY_LIST(ENTITY) where DATAATTACHMENT.ENTITY = ENTITY_LIST.ENTITY) ") 
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"   ENTITY, FLOW, SCENARIO ")
'					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) Like {statusSQL} ")

					SQL.AppendLine($"; ")
					SQL.AppendLine($"Select ")
					SQL.AppendLine($"    ENTITY AS 'Funds Ctr', ")
					SQL.AppendLine($"    FLOW AS 'REQ', ")
					SQL.AppendLine($"   '' AS FUND, ")
					SQL.AppendLine($"    ' ' AS MDEP, ")
					SQL.AppendLine($"    ' ' AS APEPT, ")
					SQL.AppendLine($"    ' ' AS DollarType, ")
				    SQL.AppendLine($"    ' ' AS CType, ")
					SQL.AppendLine($"    ' ' AS 'ObjectClass', ")
					SQL.AppendLine($"    [Requirement Title], ")
					SQL.AppendLine($"    Status ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails")
					SQL.AppendLine($" ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW, ")
					SQL.AppendLine($"    [Requirement Title], ")
					SQL.AppendLine($"    Status  ")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    ENTITY, ")
					SQL.AppendLine($"    FLOW ")
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)

				'Dim dtFetch As New DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
				' dtAll.Columns.IsReadOnly = False
				 For Each column As DataColumn In dtAll.Columns
					column.ReadOnly = False
					If column.ColumnName = "FUND" Or column.ColumnName = "MDEP" Or column.ColumnName = "APEPT" Or  column.ColumnName = "DollarType" Or column.ColumnName = "CType" Or column.ColumnName = "ObjectClass" Then column.MaxLength = 100
				 Next 
				  'Create another data table object for CivPay
				 Dim dtCP As New DataTable
				
'BRApi.ErrorLog.LogMessage(si, "dt all rows " & dtAll.Rows.Count)
'Return dtAll

'|***************************|
'|         Annual            |
'|***************************|
brapi.ErrorLog.LogMessage(si,"after sql statement")
			'Returns amounts for the following accounts at the annual level
			If sTimePeriod = "Annual"
					'First for loop creates a coloumn for each account selected in the data set cbx and adds it to the dtAll datatable
					For Each Acct As String In lAccount
						Dim sAcct As String = ""
						If Acct.XFEqualsIgnoreCase("Commitment") Then 
							sAcct = "Total Commitment"
						ElseIf Acct.XFEqualsIgnoreCase("Obligation") Then 
							sAcct = "Total Obligation"
						ElseIf Acct.XFEqualsIgnoreCase("UFRCommitment") Then 
							sAcct = "Total Commitment UFR"
						ElseIf Acct.XFEqualsIgnoreCase("UFRObligation") Then 
							sAcct = "Total Obligation UFR"
						End If
						
						Dim newColumn As New DataColumn(sAcct,GetType(Long)) With {.DefaultValue = 0}					
						dtAll.Columns.Add(newColumn)
'						dtAll.Columns.Add(sAcct & " Carryover")
					Next
					dtCP = dtAll.Clone()
				'For loop goes through all base members of the funds center passed through	
				For Each FundCenter As String In lEntity
					'For loop loops through each account selected to retrieve the data amounts at that account
					'sAcct is passed into the DatBuffer
					'sColumnName is defined to be referenced later to add the retrieved amounts to the column
					For Each Acct As String In lAccount
						Dim sAcct As String = ""
						Dim sColumnName As String = ""
						If Acct.XFEqualsIgnoreCase("Commitment") Then 
							sAcct = "Total_Phased_Commitment"
							sColumnName = "Total Commitment"
						ElseIf Acct.XFEqualsIgnoreCase("Obligation") Then 
							sAcct = "Total_Phased_Obligation"
							sColumnName = "Total Obligation"
						ElseIf Acct.XFEqualsIgnoreCase("UFRCommitment") Then 
							sAcct = "Phased_Commitment_UFR"
							sColumnName = "Total Commitment UFR"
						ElseIf Acct.XFEqualsIgnoreCase("UFRObligation") Then 
							sAcct = "Phased_Obligation_UFR"
							sColumnName = "Total Obligation UFR"
						End If
					
					'DataUnitPK - GetDataBufferCell to grab total amount at the current FY
						Dim myDataUnitPkAccountTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sBUDTime))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkAccountTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkAccountTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAcct)
						myDbCellPkAccountTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkAccountTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						myDbCellPkAccountTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkAccountTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalAccountAmounts As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkAccountTotal, dimConstants.Periodic, myDbCellPkAccountTotal, True, True)	
	
						
					'DataUnitPK - GetDataBufferCell to grab total carryover amount at (FY + 1)M12
						Dim myDataUnitPkCarryoverTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sCarryoverYrM12))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkCarryoverTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkCarryoverTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAcct)
						myDbCellPkCarryoverTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkCarryoverTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						
'						myDbCellPkCarryoverTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkCarryoverTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkCarryoverTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lTotalCarryoverAmount As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCarryoverTotal, dimConstants.Periodic, myDbCellPkCarryoverTotal, True, True)	
						
						'Loop through each returned amount for the current FY
						For Each amount As DataCell In lTotalAccountAmounts
							Dim sEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, amount.DataCellPk.EntityId)
							Dim sFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, amount.DataCellPk.FlowId)
							Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
							Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
							Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id)
							Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
							Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, amount.DataCellPk.UD5Id)
							Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
							
							If sFlow.XFContainsIgnoreCase("_00") Then 
								Dim checkRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																AndAlso row.Field(Of String)("REQ") = sFlow)
								If checkRow Is Nothing Then Continue For								
										
								Dim resultRow As DataRow = dtCP.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																AndAlso row.Field(Of String)("REQ") = sFlow _
																AndAlso row.Field(Of String)("FUND") = sUD1 _
																AndAlso row.Field(Of String)("MDEP") = sUD2 _
																AndAlso row.Field(Of String)("APEPT") = sUD3 _
																AndAlso row.Field(Of String)("DollarType") = sUD4 _
																AndAlso row.Field(Of String)("CType") = sUD5 _
																AndAlso row.Field(Of String)("ObjectClass") = sUD6)

								If resultRow Is Nothing Then
									Dim newRow As DataRow = dtCP.NewRow()
									newRow(sColumnName) = Math.Round(amount.CellAmount,0)
									newRow("Funds Ctr") = checkRow("Funds Ctr")
									newRow("REQ") = checkRow("REQ")
									newRow("FUND") = sUD1
									newRow("MDEP") = sUD2
									newRow("APEPT") = sUD3
									newRow("DollarType") = sUD4
									newRow("CType") = sUD5
									newRow("ObjectClass") = sUD6
									newRow("Requirement Title") = checkRow("Requirement Title")
									newRow("Status") = checkRow("Status")
									dtCP.Rows.Add(newRow)
'brapi.ErrorLog.LogMessage(si,"test 2")
								Else	
									resultRow(sColumnName) = Math.Round(amount.CellAmount,0)
								End If
							Else
							'compare the entity And flow in a returned data row from dtAll against the entity And flow from a returned databuffer cell
								Dim resultRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																AndAlso row.Field(Of String)("REQ") = sFlow)
								If resultRow Is Nothing Then Continue For						
									resultRow(sColumnName) = Math.Round(amount.CellAmount,0)
									resultRow("FUND") = sUD1
									resultRow("MDEP") = sUD2
									resultRow("APEPT") = sUD3
									resultRow("DollarType") = sUD4
									resultRow("CType") = sUD5
									resultRow("ObjectClass") = sUD6
'brapi.ErrorLog.LogMessage(si,"test 3")
							End If
						Next
						
						'Loop through each returned carryover amount
						For Each carryover As DataCell In lTotalCarryoverAmount
							Dim sCEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, carryover.DataCellPk.EntityId)
							Dim sCFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, carryover.DataCellPk.FlowId)
							Dim sCUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, carryover.DataCellPk.UD1Id)
							Dim sCUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, carryover.DataCellPk.UD2Id)
							Dim sCUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, carryover.DataCellPk.UD3Id)
							Dim sCUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, carryover.DataCellPk.UD4Id)
							Dim sCUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, carryover.DataCellPk.UD5Id)
							Dim sCUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, carryover.DataCellPk.UD6Id)
								
							If sCFlow.XFContainsIgnoreCase("_00") Then 
'								BRApi.ErrorLog.LogMessage(si, $"E#{sEntity}:F#{sFlow}:U1#{sUD1}:U2#{sUD2}:U3#{sUD3}:U4#{sUD4}:U5#{sUD5}:U6#{sUD6}")
								Dim checkRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If checkRow Is Nothing Then Continue For								
										
								Dim resultRow As DataRow = dtCP.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow _
																AndAlso row.Field(Of String)("FUND") = sCUD1 _
																AndAlso row.Field(Of String)("MDEP") = sCUD2 _
																AndAlso row.Field(Of String)("APEPT") = sCUD3 _
																AndAlso row.Field(Of String)("DollarType") = sCUD4 _
																AndAlso row.Field(Of String)("CType") = sCUD5 _
																AndAlso row.Field(Of String)("ObjectClass") = sCUD6)
								If resultRow Is Nothing Then
									Dim newRow As DataRow = dtCP.NewRow()
									newRow(sColumnName) = Math.Round(carryover.CellAmount,0)
									newRow("Funds Ctr") = checkRow("Funds Ctr")
									newRow("REQ") = checkRow("REQ")
									newRow("FUND") = sCUD1
									newRow("MDEP") = sCUD2
									newRow("APEPT") = sCUD3
									newRow("DollarType") = sCUD4
									newRow("CType") = sCUD5
									newRow("ObjectClass") = sCUD6
									newRow("Requirement Title") = checkRow("Requirement Title")
									newRow("Status") = checkRow("Status")
									dtCP.Rows.Add(newRow)
								Else	
									resultRow(sColumnName) = resultRow(sColumnName) + Math.Round(carryover.CellAmount,0)
								End If
							Else
								Dim resultRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If resultRow Is Nothing Then Continue For
								'if a carryover amount exists, add it to the existing Annual amount 
								resultRow(sColumnName) = resultRow(sColumnName) + Math.Round(carryover.CellAmount,0)
								resultRow("FUND") = sCUD1
								resultRow("MDEP") = sCUD2
								resultRow("APEPT") = sCUD3
								resultRow("DollarType") = sCUD4
								resultRow("CType") = sCUD5
								resultRow("ObjectClass") = sCUD6
							End If
						Next						
					Next
				Next
'|***************************|
'|         Monthly           |
'|***************************|	
			'Returns amounts for the following accounts at the monthly level
			ElseIf sTimePeriod = "Monthly"	
				'Create columns for the accounts selected in the dataset
				For Each Acct As String In lAccount			
					Dim sAcct As String = ""
					If Acct.XFEqualsIgnoreCase("Commitment") Then 
						sAcct = "Commitment"
					ElseIf Acct.XFEqualsIgnoreCase("Obligation") Then 
						sAcct = "Obligation"
					ElseIf Acct.XFEqualsIgnoreCase("UFRCommitment") Then 
						sAcct = "Commitment UFR"
					ElseIf Acct.XFEqualsIgnoreCase("UFRObligation") Then 
						sAcct = "Obligation UFR"
					End If
					'Create Total {Account} Columns
					Dim newColumn As New DataColumn("Total " & sAcct,GetType(Long)) With {.DefaultValue = 0}
					dtAll.Columns.Add(newColumn)
					
					'Create columns for full FY (M1-M12)
					For month As Integer = 1 To 12
						Dim sMonth As String = sBUDTime & "M" & month.ToString
						Dim mbrMonthDesc As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Time, sMonth)
						Dim iFYTime As Integer = 0	
						'Need to subtract 1 from current FY to accomondate oct, nov, and dec
						If mbrMonthDesc.Description.XFContainsIgnoreCase("oct")
							iFYTime= sBUDTime - 1
							dtAll.Columns.Add(sAcct & " Oct " & iFYTime)
						Else If mbrMonthDesc.Description.XFContainsIgnoreCase("nov")
							iFYTime= sBUDTime - 1
							dtAll.Columns.Add(sAcct & " Nov " & iFYTime)			
						Else If mbrMonthDesc.Description.XFContainsIgnoreCase("dec")
							iFYTime= sBUDTime - 1
							dtAll.Columns.Add(sAcct & " Dec " & iFYTime)
						Else If mbrMonthDesc.Description.XFContainsIgnoreCase("sep")
							dtAll.Columns.Add(sAcct & " Sep " & sBUDTime)
							'add carryover column after Sep CurrentFY
							dtAll.Columns.Add(sAcct & " Carryover")
						Else 
						dtAll.Columns.Add(sAcct & " " & mbrMonthDesc.Description)
						End If
					Next
				Next
				dtCP = dtAll.Clone()
				'Loop through each base member of funds center passed through
				For Each FundCenter As String In lEntity
					'Loop through each account to retrieved amounts 
					'sAcct is referenced in the GetDataBufferCell call
					'sColumnAccount is used in the following for loop to defined each monthly column
					'sTotalColAcct defined the Total {Account} column
					For Each Acct As String In lAccount
						Dim sAcct As String = ""
						Dim sColumnAccount As String = ""
						Dim sTotalColAcct As String = ""
						If Acct.XFEqualsIgnoreCase("Commitment") Then 
							sAcct = "Phased_Commitment"
							sColumnAccount = "Commitment"
							sTotalColAcct = "Total Commitment"
						ElseIf Acct.XFEqualsIgnoreCase("Obligation") Then 
							sAcct = "Phased_Obligation_Base"
							sColumnAccount = "Obligation" 
							sTotalColAcct = "Total Obligation"
						ElseIf Acct.XFEqualsIgnoreCase("UFRCommitment") Then 
							sAcct = "Phased_Commitment_UFR"
							sColumnAccount = "Commitment UFR"
							sTotalColAcct = "Total Commitment UFR"
						ElseIf Acct.XFEqualsIgnoreCase("UFRObligation") Then 
							sAcct = "Phased_Obligation_UFR"
							sColumnAccount = "Obligation UFR" 
							sTotalColAcct = "Total Obligation UFR"
						End If																
						'For each month, retrieve all data for the acccount passed through
						'Create the coloumn for that account + month to be referenced to write the data to 
						For month As Integer = 1 To 12
							'used in databuffer
							Dim sMonth As String = sBUDTime & "M" & month.ToString
							'used for getting column names
							Dim sColumnName As String = ""
							Dim mbrMonthDesc As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimTypeId.Time, sMonth)	
							Dim iFYTime As Integer = 0							
							If mbrMonthDesc.Description.XFContainsIgnoreCase("oct")
								iFYTime= sBUDTime - 1
								sColumnName = sColumnAccount & " Oct " & iFYTime
							Else If mbrMonthDesc.Description.XFContainsIgnoreCase("nov")
								iFYTime= sBUDTime - 1			
								sColumnName = sColumnAccount & " Nov " & iFYTime
							Else If mbrMonthDesc.Description.XFContainsIgnoreCase("dec")
								iFYTime= sBUDTime - 1
								sColumnName = sColumnAccount & " Dec " & iFYTime
							Else
								sColumnName = sColumnAccount &  " " & mbrMonthDesc.Description
							End If		
							
						'DataUnitPK - GetDataBufferCell to grab total amount at each month
							Dim myDataUnitPkAccountMonthly As New DataUnitPk( _
							BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
							DimConstants.Local, _
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
							BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sMonth))
						'Buffer coordinates.
						'DataBufferCellPK
							Dim myDbCellPkAccountMonthly As New DataBufferCellPk( DimConstants.All )
							myDbCellPkAccountMonthly.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAcct)
							myDbCellPkAccountMonthly.OriginId = DimConstants.BeforeAdj
							myDbCellPkAccountMonthly.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)
							myDbCellPkAccountMonthly.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
							myDbCellPkAccountMonthly.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

							Dim lAllAccountMonthly As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkAccountMonthly, dimConstants.Periodic, myDbCellPkAccountMonthly, True, True)	

							For Each amount As DataCell In lAllAccountMonthly
								Dim sEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, amount.DataCellPk.EntityId)
								Dim sFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, amount.DataCellPk.FlowId)
								Dim sUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, amount.DataCellPk.UD1Id)
								Dim sUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, amount.DataCellPk.UD2Id)
								Dim sUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, amount.DataCellPk.UD3Id)
								Dim sUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, amount.DataCellPk.UD4Id)
								Dim sUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, amount.DataCellPk.UD5Id)
								Dim sUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, amount.DataCellPk.UD6Id)
								
								If sFlow.XFContainsIgnoreCase("_00") Then 
									Dim checkRow As DataRow = dtAll.AsEnumerable() _
											.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow)
									If checkRow Is Nothing Then Continue For								
											
									Dim resultRow As DataRow = dtCP.AsEnumerable() _
											.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow _
																	AndAlso row.Field(Of String)("FUND") = sUD1 _
																	AndAlso row.Field(Of String)("MDEP") = sUD2 _
																	AndAlso row.Field(Of String)("APEPT") = sUD3 _
																	AndAlso row.Field(Of String)("DollarType") = sUD4 _
																	AndAlso row.Field(Of String)("CType") = sUD5 _
																	AndAlso row.Field(Of String)("ObjectClass") = sUD6)
									If resultRow Is Nothing Then
										'Create new row
										Dim newRow As DataRow = dtCP.NewRow()
										newRow(sColumnName) = Math.Round(amount.CellAmount,0)
										newRow(sTotalColAcct) = Math.Round(amount.CellAmount,0)
										newRow("Funds Ctr") = checkRow("Funds Ctr")
										newRow("REQ") = checkRow("REQ")
										newRow("FUND") = sUD1
										newRow("MDEP") = sUD2
										newRow("APEPT") = sUD3
										newRow("DollarType") = sUD4
										newRow("CType") = sUD5
										newRow("ObjectClass") = sUD6
										newRow("Requirement Title") = checkRow("Requirement Title")
										newRow("Status") = checkRow("Status")
										dtCP.Rows.Add(newRow)
									Else
										'Write to the monthly account column for each indiviudal monthly amount 
										resultRow(sColumnName) = Math.Round(amount.CellAmount,0)
										'Write to the Total {Account} column for each month that passes through
										resultRow(sTotalColAcct) = resultRow(sTotalColAcct) + Math.Round(amount.CellAmount,0)
									End If
								Else
									Dim resultRow As DataRow = dtAll.AsEnumerable() _
											.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sEntity _
																	AndAlso row.Field(Of String)("REQ") = sFlow)
									If resultRow Is Nothing Then Continue For
									'Write to the monthly account column for each indiviudal monthly amount 
									resultRow(sColumnName) =  Math.Round(amount.CellAmount,0)
									'Write to the Total {Account} column for each month that passes through
									resultRow(sTotalColAcct) = resultRow(sTotalColAcct) + Math.Round(amount.CellAmount,0)
									resultRow("FUND") = sUD1
									resultRow("MDEP") = sUD2
									resultRow("APEPT") = sUD3
									resultRow("DollarType") = sUD4
									resultRow("CType") = sUD5
									resultRow("ObjectClass") = sUD6
								End If
							Next							
						Next

					'DataUnitPK - GetDataBufferCell to grab total carryover amount at (FY + 1)M12
					'This is outside of the monthly for loop because the carryover amount only exists at FY+1M12
					'After the monthly amounts are retrieved and added to the columns, loop through the carryover and add it to the respective column cells and add it to the Total {Account} column
						Dim myDataUnitPkCarryoverTotal As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, sCarryoverYrM12))
					'Buffer coordinates.
					'DataBufferCellPK
						Dim myDbCellPkCarryoverTotal As New DataBufferCellPk( DimConstants.All )
						myDbCellPkCarryoverTotal.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAcct)
						myDbCellPkCarryoverTotal.OriginId = DimConstants.BeforeAdj
						myDbCellPkCarryoverTotal.ICId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, FundCenter)	
						
						'myDbCellPkCarryoverTotal.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "None")
						myDbCellPkCarryoverTotal.UD7Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD7, "None")
						myDbCellPkCarryoverTotal.UD8Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD8, "None")

						Dim lAllCarryoverAmount As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPkCarryoverTotal, dimConstants.Periodic, myDbCellPkCarryoverTotal, True, True)	
							
						For Each carryover As DataCell In lAllCarryoverAmount
							Dim sCEntity As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Entity, carryover.DataCellPk.EntityId)
							Dim sCFlow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, carryover.DataCellPk.FlowId)
							Dim sCUD1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, carryover.DataCellPk.UD1Id)
							Dim sCUD2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, carryover.DataCellPk.UD2Id)
							Dim sCUD3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, carryover.DataCellPk.UD3Id)
							Dim sCUD4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, carryover.DataCellPk.UD4Id)
							Dim sCUD5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, carryover.DataCellPk.UD5Id)
							Dim sCUD6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, carryover.DataCellPk.UD6Id)
							If sCFlow.XFContainsIgnoreCase("_00") Then 
'								BRApi.ErrorLog.LogMessage(si, $"E#{sEntity}:F#{sFlow}:U1#{sUD1}:U2#{sUD2}:U3#{sUD3}:U4#{sUD4}:U5#{sUD5}:U6#{sUD6}")
								Dim checkRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If checkRow Is Nothing Then Continue For								
										
								Dim resultRow As DataRow = dtCP.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow _
																AndAlso row.Field(Of String)("FUND") = sCUD1 _
																AndAlso row.Field(Of String)("MDEP") = sCUD2 _
																AndAlso row.Field(Of String)("APEPT") = sCUD3 _
																AndAlso row.Field(Of String)("DollarType") = sCUD4 _
																AndAlso row.Field(Of String)("CType") = sCUD5 _
																AndAlso row.Field(Of String)("ObjectClass") = sCUD6)
								If resultRow Is Nothing Then
									Dim newRow As DataRow = dtCP.NewRow()
									newRow(sColumnAccount & " Carryover") = Math.Round(carryover.CellAmount,0)
									newRow(sTotalColAcct) = Math.Round(carryover.CellAmount,0)
									newRow("Funds Ctr") = checkRow("Funds Ctr")
									newRow("REQ") = checkRow("REQ")
									newRow("FUND") = sCUD1
									newRow("MDEP") = sCUD2
									newRow("APEPT") = sCUD3
									newRow("DollarType") = sCUD4
									newRow("CType") = sCUD5
									newRow("ObjectClass") = sCUD6
									newRow("Requirement Title") = checkRow("Requirement Title")
									newRow("Status") = checkRow("Status")
									dtCP.Rows.Add(newRow)
								Else	
									resultRow(sColumnAccount & " Carryover") = Math.Round(carryover.CellAmount,0)
									resultRow(sTotalColAcct) = resultRow(sTotalColAcct) + Math.Round(carryover.CellAmount,0)
								End If
							Else
								Dim resultRow As DataRow = dtAll.AsEnumerable() _
										.SingleOrDefault(Function(row) row.Field(Of String)("Funds Ctr") = sCEntity _
																AndAlso row.Field(Of String)("REQ") = sCFlow)
								If resultRow Is Nothing Then Continue For	
								resultRow(sColumnAccount & " Carryover") = Math.Round(carryover.CellAmount,0)
								'Add carryover amount to Total {Account} column
								resultRow(sTotalColAcct) = resultRow(sTotalColAcct) + Math.Round(carryover.CellAmount,0)
								resultRow("FUND") = sCUD1
								resultRow("MDEP") = sCUD2
								resultRow("APEPT") = sCUD3
								resultRow("DollarType") = sCUD4
								resultRow("CType") = sCUD5
								resultRow("ObjectClass") = sCUD6
							End If
						Next	
					Next		
				Next
			End If	
			'Remove _00 Requirements from dtAll since dtCP will have all of them with the correct funding lines
			Dim rowsToDelete = dtAll.Select("REQ LIKE '%_00%'")
			For Each row As DataRow In rowsToDelete
				row.Delete()
			Next
			dtAll.AcceptChanges()
			'Merge dtAll into dtCP so that all CP (REQ_00) Requirements appear on top then return the final dtCP table obj
			dtCP.Merge(dtAll)
			Return dtCP
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "BUDListByEntityUpdateRequirement: BUDListByEntityUpdateRequirement"
'Updated:JM 3/27/25
'Updated: JM & MH 3/31/2025
'Updated fronz 04/17/2025 - added review dashboard
		Public Function BUDListByEntityUpdateRequirement(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try				
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntityUpdateRequirement")				
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
'brapi.ErrorLog.LogMessage(si,"sProfileName: " & sProfileName)	
'brapi.ErrorLog.LogMessage(si,"sProfileSubString: " & sProfileSubString)
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'brapi.ErrorLog.LogMessage(si,"sCube: " & sCube)				
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim	sKeyword As String = args.NameValuePairs.XFGetValue("Keyword", "")
'				Dim sFundCode As String = args.NameValuePairs.XFGetValue("FundCode")
'				Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEP")
'				Dim sAPE As String = args.NameValuePairs.XFGetValue("APE")
'brapi.ErrorLog.LogMessage(si, sBUDTime)

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
				
				'Remove _General to get the parent Entity for Validate
				If sProfileName.XFContainsIgnoreCase("Validate") Or sProfileName.XFContainsIgnoreCase("Approve") Or sProfileName.XFContainsIgnoreCase("CMD (BUD)") Or sProfileName.XFContainsIgnoreCase("Formulate") Or sProfileName.XFContainsIgnoreCase("Review") Then  ' Or sProfileName.XFContainsIgnoreCase("Formulate")
					If sFundCenter.Contains("_General") Then 'added
						sFundCenter = sFundCenter.Replace("_General",".Base") 'added 
					End If 'added	
				End If														
		
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundCenter,True)
'brapi.ErrorLog.LogMessage(si, "FCs Count line = " & LFundCenters.Count)				
				Dim sCubeView As String = args.NameValuePairs.XFGetValue("CubeView")
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allBUDs As String = ""
				Dim allFCs As String = ""
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
'							 AND A.TIME = '{sBUDTime}'				 
'							 AND B.ACCOUNT = 'REQ_TITLE'")
			
SQL.Append($"
With B as (
	SELECT B.cube as CUBE, B.scenario as SCENARIO, B.time as TIME, B.ENTITY AS ENTITY, B.FLOW AS FLOW,  B.TEXT AS TITLE 
	FROM DATAATTACHMENT B 
	Where B.ACCOUNT = 'REQ_TITLE'
	 AND B.CUBE = '{sCube}'
	 AND B.ENTITY IN ({allFCs}) 
	 AND B.SCENARIO = '{sScenario}'
	 AND B.TIME = '{sBUDTime}'	
	
),
A as (
	SELECT  A.cube as CUBE, A.scenario as SCENARIO, A.time as TIME, A.ENTITY AS ENTITY, A.FLOW AS FLOW,  A.TEXT AS TEXT 
	from DATAATTACHMENT A
	WHERE		
	A.ACCOUNT = 'REQ_RQMT_STATUS'
 	 AND A.CUBE = '{sCube}'
	 AND A.ENTITY IN ({allFCs}) 
	 AND A.SCENARIO = '{sScenario}'
	 AND A.TIME = '{sBUDTime}'	

 )
 SELECT B.ENTITY AS ENTITY, B.FLOW AS FLOW, A.TEXT AS STATUS, B.Title AS TITLE 
 from B 
 Left outer join A  ON B.CUBE = A.CUBE 
					 AND B.ENTITY = A.ENTITY 
					 AND B.SCENARIO = A.SCENARIO
					 AND B.TIME = A.TIME
					 AND B.FLOW = A.FLOW  
where 1=1
")	

				If Not String.IsNullOrWhiteSpace(sKeyword) Then SQL.Append($" AND B.Title LIKE '%{sKeyword}%'")			

SQL.Append($"
							 ORDER BY A.ENTITY, A.FLOW") 'ORDER BY A.ENTITY")
				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntityUpdateRequirement")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using				
'BRApi.ErrorLog.LogMessage(si, "dt all rows " & dtAll.Rows.Count)
				
				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows
					Dim FundCenter As String = dataRow.Item("ENTITY")
					Dim ExistingBUD As String =  dataRow.Item("FLOW")
					Dim TitleValue As String = ""'dataRow.Item("TITLE")
					If dbnull.Value.Equals(dataRow.Item("TITLE")) Then
						TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
					Else
						 TitleValue  =  dataRow.Item("TITLE")
					End If 
					
					Dim BUDStatus As String =  ""'dataRow.Item("STATUS")
					
					'--------- get Entity Text3 --------- 
					Dim sFC As String = sFundCenter.Replace(".Base","")
					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFC).Member
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
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
						Dim BUDStatusHistoryScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						'Parses REQ history to get the previous FC level
						Dim lastFcLevel As String = GetLastFundCenterLevel(si, sCube, BUDStatusHistoryScript)
						'REQStatus will be what is set in the db and also how the WF filter below will determine which CV's can see the REQ
						BUDStatus = lastFcLevel & " NoStatus"
						'Since the current status in the db is null, set it to the desired text
						SetBUDStatusFromNull(si, sCube, BUDStatusScript, BUDStatus)							
					Else
						BUDStatus = dataRow.Item("STATUS")									
					End If
					
					'--------- get REQ workflow status level --------- 
					Dim BUDWFStatusLevel As String = BUDStatus.Substring(0,2)	
'BRApi.ErrorLog.LogMessage(si, "wfstatus " & BUDWFStatusLevel)					
					If String.IsNullOrWhiteSpace(TitleValue) Then Continue For
'BRApi.ErrorLog.LogMessage(si, "Fund Center " & FundCenter)	
				
					'================================Formulate WF======================	
				 If sProfileName = "Formulate Requirements" And Not sCubeView.XFContainsIgnoreCase("BUD") Then
						If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And (BUDStatus.XFContainsIgnoreCase("Working") Or BUDStatus.XFContainsIgnoreCase("Copied") Or BUDStatus.XFContainsIgnoreCase("Imported") Or BUDStatus.XFContainsIgnoreCase("Returned to Formulation") Or BUDStatus.XFContainsIgnoreCase("Demoted from")) And Not BUDStatus.XFContainsIgnoreCase("L2")) And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then  'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
								'BRApi.ErrorLog.LogMessage(si, "Regular call ")	
						End If	
						If BUDStatus.XFContainsIgnoreCase("Working") 
							If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("CP_" & sAPPN) Then
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
								'BRApi.ErrorLog.LogMessage(si, "Manpwoer call ")	
							End If
							If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
								'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
							End If 
						End If	
						If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And (BUDStatus.XFContainsIgnoreCase("Working") Or BUDStatus.XFContainsIgnoreCase("Copied") Or BUDStatus.XFContainsIgnoreCase("Imported") Or BUDStatus.XFContainsIgnoreCase("Returned to Formulation") Or BUDStatus.XFContainsIgnoreCase("Demoted from")) And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit")) Then
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						'BRApi.ErrorLog.LogMessage(si, "verifyandsubmit call ")
					End If
					'================================Formulate CMD WF======================	
				Else If sProfileName = "Formulate Requirements CMD" And Not sCubeView.XFContainsIgnoreCase("BUD") Then
					If (BUDStatus = "L2 Working" Or BUDStatus = "L2 Copied" Or BUDStatus = "L2 Imported" Or  BUDStatus = "L2 Returned to Formulation" ) And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then 
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "regular call ")
					End If
					If BUDStatus = "L2 Working"
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("CP_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "manpower call ")
						End If 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
						End If 
'						
					End If		
					If (BUDStatus = "L2 Working" Or BUDStatus = "L2 Copied" Or BUDStatus = "L2 Imported" Or  BUDStatus = "L2 Returned to Formulation" ) And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						'BRApi.ErrorLog.LogMessage(si, "verifyandsubmit call ")
					End If
				'================================Validate BUDs WF======================			
				ElseIf sProfileName = "Validate Requirements" Then
					If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And BUDStatus.XFContainsIgnoreCase("Ready for Validation") Or BUDStatus.XFContainsIgnoreCase("Returned to Validation")) And Not BUDStatus.XFContainsIgnoreCase("L2") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
					If BUDStatus.XFContainsIgnoreCase("Validation") 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase(sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						End If 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
						End If 
					End If	
					If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And BUDStatus.XFContainsIgnoreCase("Ready for Validation") Or BUDStatus.XFContainsIgnoreCase("Returned to Validation")) And Not BUDStatus.XFContainsIgnoreCase("L2") And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
				'================================Revalidate BUD WF======================		
				ElseIf sProfileName = "Validate Requirements CMD" Then
					If (BUDStatus.XFContainsIgnoreCase("L2 Ready for Validation") Or BUDStatus.XFContainsIgnoreCase("L2 Returned to Validation")) And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then 'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If 	
					If BUDStatus.XFContainsIgnoreCase("L2 Ready for Validation") 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("CP_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						End If
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
						End If 
					End If	
					If (BUDStatus.XFContainsIgnoreCase("L2 Ready for Validation") Or BUDStatus.XFContainsIgnoreCase("L2 Returned to Validation")) And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then 'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
				'================================Approve BUDs WF======================			
				ElseIf sProfileName = "Approve Requirements" Then
'BRApi.ErrorLog.LogMessage(si, $"FundCenter = {FundCenter } || Flow = {ExistingBUD} || TitleValue = {TitleValue} || BUDStatus = {BUDStatus}")	
					If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And BUDStatus.XFContainsIgnoreCase("Ready For Approval")) And Not BUDStatus.XFContainsIgnoreCase("L2") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
					If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And BUDStatus.XFContainsIgnoreCase("Ready For Approval")) Then
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("CP_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						End If 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
						End If 
					End If	
					If (BUDWFStatusLevel.XFEqualsIgnoreCase(entityText3) And BUDStatus.XFContainsIgnoreCase("Ready For Approval")) And Not BUDStatus.XFContainsIgnoreCase("L2") And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
				ElseIf sProfileName = "Approve Requirements CMD" Then
					If BUDStatus.XFEqualsIgnoreCase("L2 Ready For Approval") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
					If BUDStatus.XFEqualsIgnoreCase("L2 Ready For Approval") Then
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("CP_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
						End If 
						If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Withhold") And ExistingBUD.XFContainsIgnoreCase("WH_" & sAPPN) Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							'BRApi.ErrorLog.LogMessage(si, "Withholds call ")	
						End If 
					End If
					If BUDStatus.XFEqualsIgnoreCase("L2 Ready For Approval") And args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("VerifyandSubmit") Then								
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
					End If
				'================================Review BUDs WF======================			
				ElseIf sProfileName.XFContainsIgnoreCase("Review Requirements") Then
					Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
				
					'================================Certify (Year) BUDs WF======================			
				ElseIf sProfileName.XFContainsIgnoreCase("CMD (BUD)") Then
						If BUDStatus.XFEqualsIgnoreCase("L2 Approved") Then
'BRApi.ErrorLog.LogMessage(si, $"FundCenter = {FundCenter } || Flow = {ExistingBUD} || TitleValue = {TitleValue} || BUDStatus = {BUDStatus}")							
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
						End If
				End If
'				For Each row As DataRow In dt.Rows
'BRApi.ErrorLog.LogMessage(si, row.Item("Name").ToString)
'				Next
			Next

				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Currently being used ***********

#Region "BUDListByEntity: BUDListByEntity"
'Updated: KL, MF, CM - 07/19/2024 - Ticket 1484
		'Updated: AK RMW-1565 8/29/24 Updated SQL to get Title from REQ_Shared, 1999 for PGM_C20XX
		'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function BUDListByEntity(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try				
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntity")				

				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
'brapi.ErrorLog.LogMessage(si,"sProfileSubString: " & sProfileSubString)							
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
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
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allBUDs As String = ""
				Dim allFCs As String = ""
				If LFundCenters.Count = 0 Then
					Return dt
				End If
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next

				allFCs = allFCs.Substring(0,allFCs.Length-1)
				
				Dim SQL As New Text.StringBuilder
'				SQL.Append("SELECT A.ENTITY AS ENTITY, A.FLOW AS FLOW, A.TEXT AS STATUS,
'								(SELECT B.TEXT FROM DATAATTACHMENT B 
'								WHERE A.CUBE = B.CUBE 
'								 AND A.ENTITY = B.ENTITY 
'								 AND A.SCENARIO = B.SCENARIO 
'								 AND A.TIME = B.TIME 
'								 AND A.FLOW = B.FLOW 
'								 AND B.ACCOUNT = 'REQ_TITLE') AS TITLE ") 
'				SQL.Append("FROM DATAATTACHMENT A ")
'				SQL.Append($"
'							 WHERE A.ACCOUNT = 'REQ_RQMT_STATUS'
'							 AND A.CUBE = '{sCube}'
'							 AND A.ENTITY IN ({allFCs}) 
'							 --AND A.FLOW IN ({allBUDs}) 
'							 AND A.SCENARIO = '{sScenario}'
'							 ORDER BY A.ENTITY")
				
SQL.Append($"
With B as (
	SELECT B.cube as CUBE, B.scenario as SCENARIO, B.time as TIME, B.ENTITY AS ENTITY, B.FLOW AS FLOW,  B.TEXT AS TITLE 
	FROM DATAATTACHMENT B 
	Where B.ACCOUNT = 'REQ_TITLE'
	 AND B.CUBE = '{sCube}'
	 AND B.ENTITY IN ({allFCs}) 
	 AND B.SCENARIO = '{sScenario}'
	 AND B.TIME = '{sBUDTime}'	
	
),
A as (
	SELECT  A.cube as CUBE, A.scenario as SCENARIO, A.time as TIME, A.ENTITY AS ENTITY, A.FLOW AS FLOW,  A.TEXT AS TEXT 
	from DATAATTACHMENT A
	WHERE		
	A.ACCOUNT = 'REQ_RQMT_STATUS'
 	 AND A.CUBE = '{sCube}'
	 AND A.ENTITY IN ({allFCs}) 
	 AND A.SCENARIO = '{sScenario}'
	 AND A.TIME = '{sBUDTime}'	

 )
 SELECT B.ENTITY AS ENTITY, B.FLOW AS FLOW, A.TEXT AS STATUS, B.Title AS TITLE 
 from B 
 Left outer join A  ON B.CUBE = A.CUBE 
					 AND B.ENTITY = A.ENTITY 
					 AND B.SCENARIO = A.SCENARIO
					 AND B.TIME = A.TIME
					 AND B.FLOW = A.FLOW  
where 1=1
")					
				
'				SQL.Append("SELECT A.ENTITY AS ENTITY, A.FLOW AS FLOW, A.TEXT AS STATUS,
'								(SELECT B.TEXT FROM DATAATTACHMENT B 
'								WHERE B.CUBE = A.CUBE 
'								 AND B.ENTITY = A.ENTITY 
'								 AND B.SCENARIO = 'REQ_Shared' 
'								 AND B.TIME = '1999' 
'								 AND B.FLOW = A.FLOW 
'								 AND B.ACCOUNT = 'REQ_Title') AS TITLE ") 
'				SQL.Append("FROM DATAATTACHMENT A ")
'				SQL.Append($"
'							 WHERE A.ACCOUNT = 'REQ_RQMT_STATUS'
'							 AND A.CUBE = '{sCube}'
'							 AND A.ENTITY IN ({allFCs}) 
'							 AND A.SCENARIO = '{sScenario}'
'							 AND A.TIME = '{sREQTime}'
'							 Order By A.ENTITY")
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)

				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntity")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				 
				
'BRApi.ErrorLog.LogMessage(si, "dt all rows " & dtAll.Rows.Count)
				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("ENTITY")
					Dim ExistingBUD As String =  dataRow.Item("FLOW")
					Dim TitleValue As String = ""'dataRow.Item("TITLE")
					If dbnull.Value.Equals(dataRow.Item("TITLE")) Then
						TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
					Else
						 TitleValue  =  dataRow.Item("TITLE")
					End If 

					Dim BUDStatus As String =  ""'dataRow.Item("STATUS")
					
					'--------- get Entity Text3 --------- 
					Dim sFC As String = sFundCenter.Replace(".Base","")
					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFC).Member
					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
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
						Dim BUDStatusHistoryScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						'Parses REQ history to get the previous FC level
						Dim lastFcLevel As String = GetLastFundCenterLevel(si, sCube, BUDStatusHistoryScript)
						'REQStatus will be what is set in the db and also how the WF filter below will determine which CV's can see the REQ
						BUDStatus = lastFcLevel & " NoStatus"
						'Since the current status in the db is null, set it to the desired text
						SetBUDStatusFromNull(si, sCube, BUDStatusScript, BUDStatus)							
					Else
						BUDStatus = dataRow.Item("STATUS")									
					End If
					'--------- get REQ workflow status level --------- 
					Dim BUDWFStatusLevel As String = BUDStatus.Substring(0,2)	
'BRApi.ErrorLog.LogMessage(si, "wfstatus " & reqWFStatusLevel)					
					If String.IsNullOrWhiteSpace(TitleValue) Then Continue For
'BRApi.ErrorLog.LogMessage(si, "Fund Center " & FundCenter)	
					'================================Non-CMD level Manage WF======================		
					If (sProfileName = "Manage Requirements") Or (sProfileName = "Manage Requirements" And BUDStatus.XFEqualsIgnoreCase("BlankStatus")) Then	
						If sCubeView.XFEqualsIgnoreCase("BUD_Manage_Demote") Then
							If BUDStatus.XFEqualsIgnoreCase("Returned From " & newWFLevel) Or  BUDStatus.XFEqualsIgnoreCase("Returned From " & BUDWFStatusLevel) Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)	
							End If
						Else 
							If Not BUDStatus.XFEqualsIgnoreCase("Returned From CMD") And Not BUDStatus.XFEqualsIgnoreCase("Ready for CMD Prioritization") And Not BUDStatus.XFEqualsIgnoreCase("CMD Prioritized") And Not BUDStatus.XFEqualsIgnoreCase("CMD Approved") And Not BUDStatus.XFEqualsIgnoreCase("Returned From L2")  And Not BUDStatus.XFEqualsIgnoreCase("L2 Working") And Not BUDStatus.XFEqualsIgnoreCase("L2 Copied")Then
								If BUDWFStatusLevel = entityText3 Or (BUDStatus.XFEqualsIgnoreCase(newWFLevel & " Ready for Validation")) Then			
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
								End If
							End If
						End If
					'================================Command Manage WF======================	
					'*** not necessary?
					ElseIf ( sProfileName = "Manage Requirements CMD") Or ( sProfileName = "Manage Requirements CMD") And BUDStatus.XFEqualsIgnoreCase("BlankStatus") Then
						If sCubeView.XFEqualsIgnoreCase("BUD_Manage_Demote") Then
							If BUDStatus.XFEqualsIgnoreCase("Returned From CMD") Or BUDStatus.XFEqualsIgnoreCase("Returned From L2") Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)	
							End If
						Else 
					'***
							If BUDStatus.XFEqualsIgnoreCase("L2 Ready for Prioritization") Or BUDStatus.XFEqualsIgnoreCase("L2 Prioritized") Or BUDStatus.XFEqualsIgnoreCase("L2 Ready for Validation" ) Or BUDStatus.XFEqualsIgnoreCase("L2 Ready for Approval" ) Or BUDStatus.XFEqualsIgnoreCase("L2 Approved" ) Or BUDStatus.XFEqualsIgnoreCase("L2 Working") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") Then 			
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							End If
						End If							
					'================================Formulate WF======================	
					Else If sProfileName = "Formulate Requirements" And Not sCubeView.XFContainsIgnoreCase("BUD") Then
							If (BUDStatus.XFContainsIgnoreCase("Working") Or BUDStatus.XFContainsIgnoreCase("Copied") Or BUDStatus.XFContainsIgnoreCase("Imported")) And Not BUDStatus.XFContainsIgnoreCase("L2") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") Then  'updated
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							End If
							If (BUDStatus.XFContainsIgnoreCase("Working") Or BUDStatus.XFContainsIgnoreCase("Approved")) 
								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase(sAPPN) Then
       								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue,ExistingBUD)
								End If 
							End If	

					'================================Formulate CMD WF======================	
					Else If sProfileName = "Formulate Requirements CMD" And Not sCubeView.XFContainsIgnoreCase("BUD") Then
							If (BUDStatus = "L2 Working" Or BUDStatus = "L2 Copied" Or BUDStatus = "L2 Imported") And Not ExistingBUD.XFContainsIgnoreCase("REQ_00") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)
							End If
							If (BUDStatus = "L2 Working" Or BUDStatus = "L2 Approved") 
								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") And ExistingBUD.XFContainsIgnoreCase("REQ_00") Then
       								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue,ExistingBUD)
								End If 
							End If			
					'================================Validate BUDs WF======================			
					ElseIf sProfileName = "Validate Requirements" Then
							If BUDWFStatusLevel = entityText3 And BUDStatus.Contains("Ready for Validation") Then								
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
							End If
					'================================Revalidate BUD WF======================		
					ElseIf sProfileName = "Validate Requirements CMD" Then
						If BUDStatus = ("L2 Ready for Validation")  Then 'updated
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
						End If 	
					'================================Review BUDs WF======================			
					ElseIf wfProfileName.XFContainsIgnoreCase("Review Requirements") Then
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - "  & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated 		
					'================================Prioritize WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Prioritize") Then
						If BUDStatus.XFContainsIgnoreCase("Prioritized")  Then 'updated
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
						End If
					'================================Approve WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Approve") Then
						If BUDStatus.XFContainsIgnoreCase("Approval") And BUDWFStatusLevel = entityText3 Then 'updated
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)'updated
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

				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Not used anymore

#Region "BUDListByEntityAndStatus: BUDListByEntityAndStatus"
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
		Public Function BUDListByEntityAndStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try				
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntityAndStatus")				
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
		        Dim sBUDTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				Dim sTitleSearch As String = args.NameValuePairs.XFGetValue("TitleSearch", "")
				Dim sCPA As String = args.NameValuePairs.XFGetValue("CPAFilter", "")
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode")

				'If no fund center is passed then stop
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Return dt
				End If
				
				'If the fund center passed is not a descendent of the WF CMD then stop
				sFundCenter = sFundCenter.Replace("_General","")
				Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  sFundCenter & ")"
				Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
				
				If Not cbMembers.Count > 0 Then
					Return dt
				End If
								
				'--------- Checking if Base --------- 
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter.Replace("_General",""))						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				If Not isBase Then sFundCenter = sFundCenter & ".Base"
	
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundCenter,True)
				
				'--------- Get Entity Text3 --------- 
				Dim sFC As String = sFundCenter.Replace(".Base","")
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sFC).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,sBUDTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)

				'--------- Get next workflow level --------- 
				Dim currText3Num As String = entityText3.Substring(1,1)
				Dim icurrText3Num As Integer = currText3Num
				'icurrText3Num = icurrText3Num - 1
				Dim newText3Num As String = icurrText3Num - 1
				Dim newWFLevel As String = "L" & newText3Num
				Dim prevWFLevel As String = "L" & (icurrText3Num + 1)
				Dim curWFLevel As String = entityText3
				Dim sCubeView As String = args.NameValuePairs.XFGetValue("CubeView")				
				
				'========= Define Targeted Status based on WF and Dashboard's FC selection ==========================================================================
				Dim sTgtStatus As String = ""
				Select Case True
				Case sProfileName = "Manage Requirements"				
					If sCubeView.XFEqualsIgnoreCase("BUD_Manage_Demote") Then					
						sTgtStatus = $"'{curWFLevel} Returned From {newWFLevel}','Returned From {curWFLevel}'"
					Else 
						sTgtStatus = $"'{curWFLevel} Working','{curWFLevel} Ready For Validation','{curWFLevel} Prioritized','{curWFLevel} Ready For Approval','{newWFLevel} Ready For Validation','{curWFLevel} Copied','{curWFLevel} Imported'"
						'If isBase = True Then sTgtStatus = $"{sTgtStatus},'{curWFLevel} Working'"
					End If
					
				Case sProfileName = "Manage Requirements CMD"
					If sCubeView.XFEqualsIgnoreCase("BUD_Manage_Demote") Then					
						sTgtStatus = $"'Returned From CMD','Returned From L2'" 
						Else
						sTgtStatus = $"'{curWFLevel} Prioritized','{curWFLevel} Ready For Approval','{curWFLevel} Approved','{curWFLevel} Ready For Validation','{curWFLevel} Copied','{curWFLevel} Working'"
					End If
					
				Case (wfProfileName.XFContainsIgnoreCase("Formulate") And Not sCubeView.XFContainsIgnoreCase("BUD"))
					If isBase = True Or  isBase = False Then
						sTgtStatus = $"'{curWFLevel} Working','{curWFLevel} Copied','{curWFLevel} Imported'"
					Else 
						Return Nothing
					End If
					
				Case wfProfileName.XFContainsIgnoreCase("Rollover")
					sBUDTime = (Convert.ToInt32(sBUDTime) - 1).ToString
					sScenario = $"BUD_C{sBUDTime}"
'BRApi.ErrorLog.LogMessage(si, "scenario from dataset" & sScenario)						
				Case sProfileName = "Validate Requirements"
					sTgtStatus = $"'{curWFLevel} Ready For Validation'"
				
				Case sProfileName = "Validate Requirements CMD"
					sTgtStatus = $"'{curWFLevel} Ready For Validation'"
				
'				Case wfProfileName.XFContainsIgnoreCase("Prioritize")
'					sTgtStatus = $"'{curWFLevel} Ready For Prioritization','{curWFLevel} Prioritized'"
				
				Case wfProfileName.XFContainsIgnoreCase("Approve")
					sTgtStatus = $"'{curWFLevel} Ready For Approval'"
				
				'Case wfProfileName.XFContainsIgnoreCase("CMD (PGM)")
					'sTgtStatus = $"'{curWFLevel} Approved'"	
				
				Case wfProfileName.XFContainsIgnoreCase("Review")
					sTgtStatus = $"'% Ready For Prioritization','% Prioritized','% Ready For Approval','% Approved','% Ready For Validation','% Working','% Returned From %','%Copied','Returned From %'"
				
				Case wfProfileName.XFContainsIgnoreCase("CMD (BUD)")
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
				Dim allBUDs As String = ""
				Dim allFCs As String = ""
				If LFundCenters.Count = 0 Then
					Return dt
				End If
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next

				allFCs = allFCs.Substring(0,allFCs.Length-1)
'BRApi.ErrorLog.LogMessage(si, "allFCs: " & allFCs & "     sCube: " & sCube & "" & "    sBUDTime " & sBUDTime & " sScenario: " & sScenario)				
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
'							 AND A.TIME = '{sBUDTime}'				 
'							 AND B.ACCOUNT = 'REQ_TITLE'")
				
				SQL.Append($"
With B as (
	SELECT B.cube as CUBE, B.scenario as SCENARIO, B.time as TIME, B.ENTITY AS ENTITY, B.FLOW AS FLOW,  B.TEXT AS TITLE 
	FROM DATAATTACHMENT B 
	Where B.ACCOUNT = 'REQ_TITLE'
	 AND B.CUBE = '{sCube}'
	 AND B.ENTITY IN ({allFCs}) 
	 AND B.SCENARIO = '{sScenario}'
	 AND B.TIME = '{sBUDTime}'	
	
),
A as (
	SELECT  A.cube as CUBE, A.scenario as SCENARIO, A.time as TIME, A.ENTITY AS ENTITY, A.FLOW AS FLOW,  A.TEXT AS TEXT 
	from DATAATTACHMENT A
	WHERE		
	A.ACCOUNT = 'REQ_RQMT_STATUS'
 	 AND A.CUBE = '{sCube}'
	 AND A.ENTITY IN ({allFCs}) 
	 AND A.SCENARIO = '{sScenario}'
	 AND A.TIME = '{sBUDTime}'	

 )
 SELECT B.ENTITY AS ENTITY, B.FLOW AS FLOW, A.TEXT AS STATUS, B.Title AS TITLE 
 from B 
 Left outer join A  ON B.CUBE = A.CUBE 
					 AND B.ENTITY = A.ENTITY 
					 AND B.SCENARIO = A.SCENARIO
					 AND B.TIME = A.TIME
					 AND B.FLOW = A.FLOW  
where 1=1
")		
				
				
				
				
				
				
				
				'--------- Workflow profile conditions --------- 
				If sProfileName.XFContainsIgnoreCase("Manage") Then
					SQL.Append($"
								 AND (A.TEXT IN ({sTgtStatus}) OR A.TEXT IS NULL)")
				
				Else If sProfileName.XFContainsIgnoreCase("Review") Then
					Dim oStatus As List(Of String) = sTgtStatus.Split(",").ToList()
					SQL.Append($"
								AND (A.TEXT IS NULL ")
						For Each status As String In oStatus 
							SQL.Append($"OR A.TEXT LIKE {status} ")
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
				
			    '--------- CPA Yes/No filter conditions --------- 
				If Not String.IsNullOrWhiteSpace(sCPA)
					If (Not sCPA.XFContainsIgnoreCase(",")) Then 
						'Filter = Yes
						If sCPA.XFEqualsIgnoreCase("Yes") Then
						SQL.Append($"
									AND CPA LIKE '%{sCPA}%'")	
						'Filter = No
						Else
							SQL.Append($"
									 AND (CPA LIKE '%{sCPA}%' or CPA is Null)")
						End If
	'					ElseIf sCPA.XFContainsIgnoreCase("%Yes, No%")						
	'						SQL.Append($"
	'								 AND CPA in('Yes','No') or CPA is Null")
					End If
				End If	
				SQL.Append($"
							 ORDER BY A.ENTITY")
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
				
				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "BUDListByEntityAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using			
'BRApi.ErrorLog.LogMessage(si, "dt all rows " & dtAll.Rows.Count)

				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("ENTITY")
					Dim ExistingBUD As String =  dataRow.Item("FLOW")
					Dim TitleValue As String = dataRow.Item("TITLE")
'BRApi.ErrorLog.LogMessage(si, $"E = {FundCenter} || F = {ExistingREQ} ")
					Dim BUDStatus As String =  ""'dataRow.Item("STATUS")
		
					'This is to account for instances where Status is set to blank by the manager.
					'Not a valid business process. But OS exposes 'No Data' option				
					If dataRow.IsNull(("STATUS")) Then	
						'RMW-1028 - To address the blank status issue, REQ history will be parsed to find the previous FC level the REQ was on. A new status will be set to that FC level and an additional error status will be appended after.
						'Member scripts may also be encapsulated in the method calls, but is currently done here to reduce duplicate code.
						Dim BUDStatusHistoryScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Status_History:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim BUDStatusScript As String = "Cb#" & sCube & ":E#" & FundCenter & ":C#Local:S#" & sScenario & ":T#" & sBUDTime & ":V#Annotation:A#REQ_Rqmt_Status:F#" & ExistingBUD & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						'Parses REQ history to get the previous FC level
						Dim lastFcLevel As String = GetLastFundCenterLevel(si, sCube, BUDStatusHistoryScript)
						'REQStatus will be what is set in the db and also how the WF filter below will determine which CV's can see the REQ
						BUDStatus = lastFcLevel & " NoStatus"
						'Since the current status in the db is null, set it to the desired text
						SetBUDStatusFromNull(si, sCube, BUDStatusScript, BUDStatus)							
					Else
						BUDStatus = dataRow.Item("STATUS")									
					End If	
					
'					oEFList.Add($"E#{FundCenter}:F#{ExistingREQ}")
					Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingBUD & " - " & TitleValue, FundCenter & " " & ExistingBUD)						
				Next
'BRApi.ErrorLog.LogMessage(si, $"dt Count = {dt.Rows.Count}")
				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Not used anymore

#Region "BUDListToSubmit: Get List of REQs for Submission"	
		'Get List of File Names in Folder
		Public Function BUDListToSubmit(ByVal si As SessionInfo, ByVal globals As BRGlobals, Optional ByVal api As Object = Nothing, Optional ByVal args As DashboardDataSetArgs = Nothing)
			Try		
				Dim oBUDList As DataTable = BRApi.Utilities.GetSessionDataTable(si,si.UserName,"BUDListCVResult")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "BUDListToSubmit")
				For Each row As DataRow In oBUDList.Rows
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

#Region "BUDWFStatusList: BUDWFStatusList"

		Public Function BUDWFStatusList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
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
				
				Dim BUDCountSQL As New Text.StringBuilder
				BUDCountSQL.Append($"
					select
						Entity,
						text,
						Count(*) as BUDCount
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

				Dim dtAll As New DataTable("BUDWFStatusListAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					If sReportType.XFEqualsIgnoreCase("BUD_List") Then
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				 	End If
				End Using
					 Dim dtList As New DataTable("BUDWFStatusListReduced")
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
						Dim dtBUD As String =  dataRow.Item("flow")
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
							dtList.Rows.Add(dtScenario, dtSourceFC, dtSourceFC,dtBUD,dtAccount,dtText,dtCurrLevel)	
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
										dtList.Rows.Add(dtScenario, dtSourceFC, sParent.Name,dtBUD,dtAccount,dtText,dtCurrLevel)	
										Exit For	
									End If 
								Next
						End If 
					Next
				 Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				 If sReportType.XFEqualsIgnoreCase("REQ_Status") Then
					 dtAll = BRApi.Database.ExecuteSql(dbConn,BUDCountSQL.ToString(),True)
					 Return dtAll
				 End If 
					End Using
				
				Return dtList 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "zGetUserFundCentersByWF: Get User Fund Centers By Workflow"
'Updated 07/19 ticket 1484 by KL, MF, CM
'Updated 5/1/25 by JM. New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
'		Public Function zGetUserFundCentersByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
'			Try
''Dim st As DateTime = DateTime.Now				
''Brapi.ErrorLog.LogMessage(si,"In Fund Center Rule")
'				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FundCentersByWF")
'				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name		
'				'Get command
'				Dim cmd As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName 
'					'Brapi.ErrorLog.LogMessage(si,"WF" & cmd)	
'				Dim sFCgroups As String = ""
'				'Get fund centers from the user profile. Get groups user belongs to and substring FC from them
'				Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
'				'Dim sUserName As String = objSignedOnUser.User.Name
'				Dim sUserName As String = si.AuthToken.UserName
				
			
'		Dim SQL As New Text.StringBuilder
'SQL.Append($" SELECT Distinct SG.[UniqueID] as GroupID, SG.Name as AccessGroup,U.[UniqueID] as UserID,U.Name as UserName, GU.GroupKey as [role]

' FROM  [SecUser] as u
'join  [SecGroupChild] AS GU on gu.ChildKey = u.uniqueid
'join  [SecGroup] AS SG on sg.UniqueID = gu.GroupKey
'  Where SG.Name like  '%_BUD_%' 
'and U.Name = '{sUserName}'
'Order by SG.Name

'")

'	Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "GroupSecList")
'				Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
'					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'				End Using
''Brapi.ErrorLog.LogMessage(si,"dtAll rows: " & dtAll.Rows.Count)				 
'For Each dataRow As DataRow In dtAll.Rows					
					
'					Dim sGroup As String = dataRow.Item("AccessGroup")

'sFCgroups+=sGroup + ","
'Next
			
'				If (String.IsNullOrWhiteSpace(sFCgroups)) Then
'					Return Nothing
'				Else 
'				sFCgroups = sFCgroups.TrimEnd(","c)
'				End If
					
'	'Brapi.ErrorLog.LogMessage(si,"FC" & sFCgroups)			

'				Dim sGroupList As String() = sFCgroups.Split(",")
'				'filter fundcenters in the command only
'				Dim cmdChildrenScript As String = "E#" & cmd & ".DescendantsInclusive"
'				Dim cmdDimpk As DimPk = BRApi.Finance.Dim.GetDimPk(si, cmd)
'				Dim objList As List(Of MemberInfo)
'				objList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", cmdChildrenScript, True)
''Brapi.ErrorLog.LogMessage(si, "objList count = " & objList.Count)	
				
'				Dim wfProfileCertify As String ="" 
'				If cmd.XFEqualsIgnoreCase("USAREUR_AF")
'					wfProfileCertify = "USAREUR AF CMD (BUD)"
'				Else
'					wfProfileCertify = cmd & " CMD (BUD)"
'				End If 
				
				
'				For Each Grp In sGroupList
					
'					Dim fc As String =""
'					If Grp.XFContainsIgnoreCase("USAREUR_AF") 
'						fc = Grp.Split("_")(5)
'					Else
'						fc = Grp.Split("_")(4)
'					End If
'					If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc))) Then

'						Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, fc).Member
'						Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'						Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'						Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'						Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
'						Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
''Brapi.ErrorLog.LogMessage(si, "WFLevel = " & WFLevel & ", entityText3 = " & entityText3)

'						'Filter fundcenter combobox for command and non cmd leve level
'						If wfProfileName.XFContainsIgnoreCase("CMD") And fc.length > 3 Then
'							Continue For
'						Else If Not wfProfileName.XFContainsIgnoreCase("CMD") And fc.length = 3 Then
'							Continue For						
'						End If
'						'verify text 3 is populated
'						If (Not entityText3.Contains("=L")) Then
'							Throw New Exception("Fund Center Dropdown:" & Environment.NewLine & "Member " & entityMem.Name & " is missing its text 3 property." & Environment.NewLine & "Contact an Administrator for help.")
'						End If
						
'						Dim FormulateFC As String = "CR_" & fc
'						Dim ValidateFC As String = "VA_" & fc
'						Dim ApproveFC As String = "AP_" & fc
'						Dim ManageFC As String = "MG_" & fc
						
						
''							
'						Select Case True
'						Case (wfProfileName.XFContainsIgnoreCase("Formulate") Or wfProfileName.XFContainsIgnoreCase("Import")) And Grp.XFContainsIgnoreCase(FormulateFC)
						
'							Dim fcChildrenScript As String = "E#" & fc & ".TreeDescendantsInclusive"
'							Dim fcList As List(Of MemberInfo)
'							fcList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", fcChildrenScript, True)
'							If fcList.Count > 1 Then
''									If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc & "_General"))) Then
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc & "_General")
'								Else
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc)
''									End If
'							End If
''					
'					Case wfProfileName.XFContainsIgnoreCase("Validate") And Grp.XFContainsIgnoreCase(ValidateFC)
'							Dim fcChildrenScript As String = "E#" & fc & ".TreeDescendantsInclusive"
'							Dim fcList As List(Of MemberInfo)
'							fcList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", fcChildrenScript, True)
'							If fcList.Count > 1 Then
''									If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc & "_General"))) Then
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc & "_General")
'								Else
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc)
''									End If
'							End If


'				Case wfProfileName.XFContainsIgnoreCase("Approve") And Grp.XFContainsIgnoreCase(ApproveFC)
'						Dim fcChildrenScript As String = "E#" & fc & ".TreeDescendantsInclusive"
'							Dim fcList As List(Of MemberInfo)
'							fcList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", fcChildrenScript, True)
'								If fcList.Count > 1 Then
''									If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc & "_General"))) Then
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc & "_General")
'								Else
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc)
''									End If
'							End If


'				Case wfProfileName.XFContainsIgnoreCase("Manage")   And Grp.XFContainsIgnoreCase(ManageFC)
'							Dim fcChildrenScript As String = "E#" & fc & ".TreeDescendantsInclusive"
'							Dim fcList As List(Of MemberInfo)
'							fcList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", fcChildrenScript, True)
'							If fcList.Count > 1 Then
''									If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc & "_General"))) Then
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc & "_General")
'								Else
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc)
''									End If
'							End If


'			Case wfProfileName.XFContainsIgnoreCase("Review") Or wfProfileName.XFEqualsIgnoreCase(wfProfileCertify)
'			Dim fcChildrenScript As String = "E#" & fc & ".TreeDescendantsInclusive"
'							Dim fcList As List(Of MemberInfo)
'							fcList = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", fcChildrenScript, True)
'							If fcList.Count > 1 Then
''									If (objList.Any(Function(x) x.Member.Name.XFEqualsIgnoreCase(fc & "_General"))) Then
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc & "_General")
'								Else
'									Me.WriteNameValuePairRow(si, dt, fcList(0).NameAndDescription, fc)
''									End If
'							End If
							
						
''Brapi.ErrorLog.LogMessage(si, "Member Name = " & fc)
'	End Select 
'End If 
			
'				Next
			
				
							
'Dim DTRemoveDups As DataTable

'Dim sColumnlist As New List(Of String)
'Dim sColumn As DataColumn

'For Each sColumn In dt.Columns
'	sColumnlist.Add(sColumn.ColumnName)

'Next

'DTRemoveDups = dt.defaultView.ToTable(True, sColumnlist.ToArray())


				
				
''Dim stp As DateTime = DateTime.Now	
''Dim sp As TimeSpan = stp - st
''Brapi.ErrorLog.LogMessage(si, "Fund Center Combobox took = " & sp.Milliseconds)				
'				Return DTRemoveDups

'			Catch ex As Exception
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'		End Function
		
#End Region

#Region "Security Logic: Get User Fund Centers By Workflow"
'Updated 07/19 ticket 1484 by KL, MF, CM
'Updated 5/29/2025 MF. New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundCentersByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
'Dim st As DateTime = DateTime.Now				
'Brapi.ErrorLog.LogMessage(si,"In Fund Center Rule")
				Dim fcDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FundCentersByWF")
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name

'brapi.ErrorLog.LogMessage(si, wfProfileAccessGroup)				
				
				Dim WFManager As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"MG")

'For Each xxxx In accessGroupInfo_Split
'	brapi.ErrorLog.LogMessage(si, xxxx)
'Next
				
				Dim wfProfileStep As String = accessGroupInfo_Split(accessGroupInfo_Split.Count-1)
'brapi.ErrorLog.LogMessage(si, wfProfileStep)

				If wfProfileStep = "WF"
					wfProfileAccessGroup = StringHelper.ReplaceString(wfProfileAccessGroup,"_WF",String.Empty,True)
				End If					
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name		
				'Get command
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

'					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, fc_name).Member
'					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
'					Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)

'					If (Not entityText3.Contains("=L")) Then
'						Throw New Exception("Fund Center Dropdown:" & Environment.NewLine & "Member " & entityMem.Name & " is missing its text 3 property." & Environment.NewLine & "Contact an Administrator for help.")
'					End If
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

#Region "zGetAllUsers: Get All Users"
'Updated to add BUD prefix - JM 11/18/24
'Updated 5/2/25 - JM - Removed command groups and updated to use new BUD sec groups with entity appended to the end. 
		Public Function zGetAllUsers(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs, Optional ByVal mode As String = "AllFC_CMD") As Object
			Try
'Brapi.ErrorLog.LogMessage(si,"method initial call")
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim fullBUDEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
				Dim BUDEntity As String = fullBUDEntity.Replace("_General","")
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode", mode)
				
				'Set variables to security BUD security group names based on user's session credentials
				Dim userSecGroupNameFC_CR As String= "g_BUD_" & wfCube & "_FC_CR_" & BUDEntity
				Dim userSecGroupNameFC_VA As String= "g_BUD_" & wfCube & "_FC_VA_"  & BUDEntity
				'Dim userSecGroupNameFC_AP As String= "g_BUD_" & wfCube & "_FC_AP_" & BUDEntity
				Dim userSecGroupNameFC_MG As String= "g_BUD_" & wfCube & "_FC_MG_" & BUDEntity

				
				'Get the BUD parent security group containing list of assigned child security groups (may contain both users and groups)
				'BUD FC groups (with children)
				Dim userSecGroupFC_CR As GroupInfo = BRApi.Security.Admin.GetGroup(si, userSecGroupNameFC_CR)
				Dim userSecGroupFC_VA As GroupInfo = BRApi.Security.Admin.GetGroup(si, userSecGroupNameFC_VA)
'				Dim userSecGroupFC_AP As GroupInfo = BRApi.Security.Admin.GetGroup(si, userSecGroupNameFC_AP)
				Dim userSecGroupFC_MG As GroupInfo = BRApi.Security.Admin.GetGroup(si, userSecGroupNameFC_MG)

				
				'Check whether a security group exists or has been modified
				If(userSecGroupFC_CR Is Nothing) Then
					Throw New System.Exception($"ERROR: RMW secruity group {userSecGroupNameFC_CR} does not exist or has been modified. Please contact system admin to update the following business rule: BUD_DataSet > GetAllUsers")
				Else If(userSecGroupFC_VA Is Nothing) Then
					Throw New System.Exception($"ERROR: RMW secruity group {userSecGroupNameFC_VA} does not exist or has been modified. Please contact system admin to update the following business rule: BUD_DataSet > GetAllUsers")
'					Else If(userSecGroupFC_AP Is Nothing) Then
'						Throw New System.Exception($"ERROR: RMW secruity group {userSecGroupNameFC_AP} does not exist or has been modified. Please contact system admin to update the following business rule: BUD_DataSet > GetAllUsers")
				Else If(userSecGroupFC_MG Is Nothing) Then
					Throw New System.Exception($"ERROR: RMW secruity group {userSecGroupNameFC_MG} does not exist or has been modified. Please contact system admin to update the following business rule: BUD_DataSet > GetAllUsers")
			
				End If 
				'Set variables to a list of child security groups declared above
				'BUD FC children security group list
				Dim usersFC_CR As List(Of Principal) = userSecGroupFC_CR.ChildPrincipals
				Dim usersFC_VA As List(Of Principal) = userSecGroupFC_VA.ChildPrincipals
'				Dim usersFC_AP As List(Of Principal) = userSecGroupFC_AP.ChildPrincipals
				Dim usersFC_MG As List(Of Principal) = userSecGroupFC_MG.ChildPrincipals


				'Set users list for the specific required type
				' added table name to distiguish between validator and all users
				Dim tableName As String = "AllPossibleStakeholders"
				Dim users As New List(Of Principal)
				Select Case sMode
				'BUD FC users
				Case "AllFC_CMD"
					Dim AllFC_CMD As New HashSet(Of Principal)(usersFC_CR.Concat(usersFC_VA).Concat(usersFC_MG)) '.Concat(usersFC_AP))
					users = AllFC_CMD.ToList()		
				Case "FC_CR"
					users = usersFC_CR
				Case "FC_VA"
					users = usersFC_VA
					tableName = "FCValidator"
				
				Case "FC_MG"
					users = usersFC_MG
				
				End Select
				'**Note** AllPossibleStakeholders may be called to return a dt that contains users from only one BUD security role i.e., only Manager (MG)
				
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, tableName)
				For Each user As Principal In users
					If (user.PrincipalType = PrincipalType.User) Then
						Dim objUserInfo As UserInfo = BRApi.Security.Authorization.GetUser(si, user.Name)
						Dim sFundCenters As String = ""
			
						Dim SQL As New Text.StringBuilder
						SQL.Append($" SELECT Distinct SG.[UniqueID] as GroupID, SG.Name as AccessGroup,U.[UniqueID] as UserID,U.Name as UserName, GU.GroupKey as [role]

						 FROM  [SecUser] as u
						join  [SecGroupChild] AS GU on gu.ChildKey = u.uniqueid
						join  [SecGroup] AS SG on sg.UniqueID = gu.GroupKey
						  Where SG.Name like  '%_BUD_%' 
						and U.Name = '{user}'
						Order by SG.Name

						")

						Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "GroupSecList")
						Using dbConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
							 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
'Brapi.ErrorLog.LogMessage(si,"dtAll rows: " & dtAll.Rows.Count)				 
						For Each dataRow As DataRow In dtAll.Rows		
							Dim sGroup As String = dataRow.Item("AccessGroup")
							Dim sFC As String = ""
							If sGroup.XFContainsIgnoreCase("USAREUR_AF") 
								sfc = sGroup.Split("_")(5)
							Else
								sfc = sGroup.Split("_")(4)
							End If
								
							sFundCenters+=sFC  + ","
						Next
						If (String.IsNullOrWhiteSpace(sFundCenters))Then
							Return Nothing
						Else 
							sFundCenters = sFundCenters.TrimEnd(","c)
						End If
'Brapi.ErrorLog.LogMessage(si, "FC" & sFundCenters)
						'filter users that contain the FC  in the assigned roles
						If (sFundCenters.XFContainsIgnoreCase(BUDEntity)) Then
							Me.WriteNameValuePairRow(si, dt, user.Name.Trim, objUserInfo.User.Email.Trim)
						End If
'Brapi.ErrorLog.LogMessage(si, "user.Name): " & user.Name)
					End If
				Next
				'Me.WriteNameValuePairRow(si, dt, "Test Name", "Test Email")
				Return dt
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try	
		End Function
#End Region

#Region "Notify Users Logic: Get All Users"

		Public Function GetAllUsers(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs, Optional ByVal mode As String = "AllFC_CMD") As Object
			Try
		
'Brapi.ErrorLog.LogMessage(si,"method initial call")
			
				'Dim wfLevel As String = wfProfileName.Substring(0,2)
				Dim fullPGMEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
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
				'Case used for Notify All Users that contain a BUD security group with the same Entity 
				'A user with security group g_BUD_AFC_FC_CR_A97EJ with see all users assigned to a BUD security group with _A97EJ appened to the end i.e., g_BUD_AFC_FC_MG_A97EJ
				'Applies to both FC and CMD level
				Case "AllFC_CMD"	
						SQL= $"Select Distinct U.Name As UserName, U.Email As UserEmail
						From  [SecUser] As u
						Join  [SecGroupChild] As GU On gu.ChildKey = u.uniqueid
						Join  [SecGroup] As SG On sg.UniqueID = gu.GroupKey
						Where (SG.Name Like  '%{wfProfileAccessGroupmodule}%' AND SG.Name like '%{PGMEntity}')"
				' Case used for Notify all Validators for the entity the creator creates requirements for
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
'Brapi.ErrorLog.LogMessage(si,"dtAll rows: " & dtAll.Rows.Count)	
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

#Region "GetBUDListForRePrioritization: Get BUD for Re-Prioritization"
		'RMW-1012 Return list of UFRs for Re-Prioritization
		Public Function GetBUDListForRePrioritization(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object	
		
			'get FundCenter from cached prompt from Prioritize dashboard
			Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","Entity","")
			If String.IsNullOrWhiteSpace(sFundCenter) Then Return Nothing																								
			'Call dataset BR to return a datatable that has been filtered by ufr status
			args.NameValuePairs.XFSetValue("Entity", sFundCenter)
			args.NameValuePairs.XFSetValue("Mode", "Re-Prioritize") 
			Dim dt = BUDListByEntity(si, globals, api, args)
			Dim sSelectAllVal As String = ""
			For Each row As DataRow In dt.Rows
				sSelectAllVal = $"{sSelectAllVal},{CStr(row("Value"))}"
			Next
			If sSelectAllVal.length > 0 Then sSelectAllVal = sSelectAllVal.Substring(1)
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si,si.UserName,"BUDPrompts","BUDSelectAll",sSelectAllVal)
			

'			Dim selectAllRow As DataRow = dt.NewRow()
'			selectAllRow("Name") = "All"
'			selectAllRow("Value") = sSelectAllVal
'			dt.Rows.InsertAt(selectAllRow,0)
			Return dt
		End Function
			  
#End Region

#Region "GetBUDStatus: Get BUD Status"

		Public Function GetBUDStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
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
							End If	
							If Not entityText3 = "L2" And bHasChildren = True
								statusValues =  entityText3 & " Working," & entityText3 & " Ready for Validation," & entityText3 & " Ready for Approval," & newWFLevel & " Ready for Validation"
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

#Region "GetFileNamesInFolder: Get List of File Names in Folder"	
		'Get List of File Names in Folder
		Public Function GetFileNamesInFolder(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs)
			Try		
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sFolder As String = $"Documents/Public/CMD_Budget/{wfcube}"
				Dim objList As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sFolder, XFFileType.All, False, False, False)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ListOfFiles")
				For Each item As NameAndAccessLevel In objList
					Dim sFileName As String = item.Name.Substring(item.Name.LastIndexOf("/") + 1)
					Dim sFileNameLower As String = sFileName.ToLower
					If sFileNameLower.StartsWith(wfCube.ToLower) And Not sFileNameLower.EndsWith("pdf") Then
						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
					End If
'					If Not wfCube.XFContainsIgnoreCase("Army") And sFileNameLower.StartsWith("Army".ToLower) Then
'						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
'					End If
				Next
				sFolder = $"Documents/Public/CMD_Budget/ARMY"
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
		
#End Region

#Region "GetLastFundCenterLevel: GetLastFundCenterLevel"
		' Created: 2024-Feb-05 - CS
		' Purpose: Parse UFR status history to return the last FC level the UFR was on
		' Usage: UFRListByEntity() calls in order to overwrite a null status to the proper FC level
		Public Function GetLastFundCenterLevel(ByVal si As SessionInfo, ByVal sCube As String, ByVal memberScript As String) As String	

			Dim fcLevel As String = ""
			'Get the status history
			Dim statusHistory As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, memberScript).DataCellEx.DataCellAnnotation
			'Split the status history into an array
			Dim statusArray() As String = statusHistory.Split(",")
			'Loop through the array, starting at the last place in the history
			For i As Integer = statusArray.Length - 1 To 0 Step -1
				'Split the individual status into an array for additional parsing. Expected output is {FC, FC Level, Status}
				Dim arrayStatusArray() As String = statusArray(i).Split(" ")
				'Sometimes only a FC is listed so an array size check is required. Additionally checking the FC Level is in the expected format of beginning with a L
				If arrayStatusArray.Length > 1 And arrayStatusArray(1).StartsWith("L") Then
					'Expected location of the FC Level after condition checks is then assigned to the fcLevel variable
					fcLevel = arrayStatusArray(1)
					Exit For
				End If
			Next	
'Brapi.ErrorLog.LogMessage(si,"fclevel: " & fcLevel)			
			Return fcLevel
		End Function
			  
#End Region			
				
#Region "SetBUDStatusFromNull: SetBUDStatusFromNull"
		' Created: 2024-Feb-05 - CS
		' Purpose: Sets the REQ status to statusMessage. Meant to be encapsulate logic to fix null statuses. Currenly just doing a SetDataCell.
		' Usage: BUDListByEntity() calls in order to overwrite a null status to an expected text
		Public Function SetBUDStatusFromNull(ByVal si As SessionInfo, ByVal sCube As String, ByVal memberScript As String, ByVal statusMessage As String)
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
			  
#End Region

#Region "Get Formulate Review Requirements"

		Public Function GetFormulateReviewReqs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Dim wfName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
   				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")
				Dim sFundCenter As String = sEntity
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim Year1 As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim Year2 As Integer = Year1 + 1
				Dim account1 As String = "Phased_Obligation_Base"
				Dim account2 As String = "Phased_Commitment"
				Dim account3 As String = "Phased_Obligation_UFR"
				Dim account4 As String = "Phased_Commitment_UFR"
				Dim accountId1 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account1)
				Dim accountId2 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account2)
				Dim accountId3 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account3)
				Dim accountId4 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account4)

				If String.IsNullOrWhiteSpace(sEntity) Then Return " "
					
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = entityText3.Substring(1,1)
				Dim iCurrentLevel As Integer = CInt(currentStatus)
				Dim iNextLevel As Integer = iCurrentLevel - 1
				Dim currentWFLevel As String = "L" & iCurrentLevel
				Dim newWFLevel As String = "L" & iNextLevel
				
				'--------- derive FC List ----------
				sFundCenter = sFundCenter.ToLower.Replace("_general","")
				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, wfCube)			
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)						
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				Dim entitySQL As String = ""				
				If isBase = True Then 
'					entitySQL = $"And ENTITY = '{sFundCenter}' "
					entitySQL = $"('{sFundCenter}')"				
				Else
					Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & wfCube, "E#"& sFundCenter &".Base",True)
					Dim isFirst As Boolean = True
'					entitySQL = $"And ENTITY IN ("
					For Each FC As MemberInfo In LFundCenters						
						If isFirst Then
'							entitySQL &= $"'{FC.Member.Name}'"
							entitySQL &= $"('{FC.Member.Name}')"
						Else
'							entitySQL &= $",'{FC.Member.Name}'"
							entitySQL &= $", ('{FC.Member.Name}')"
						End If
						isFirst = False
					Next			
				End If
'				entitySQL = "('a76c1'),('A76_General'),('A76CC_General')"
				
				'--------- derive status ----------
				Dim statusSQL As String = ""

				
				Select Case True
				Case wfName.XFContainsIgnoreCase("Formulate")
'					statusSQL = $"Where pd.Status IN ('{currentWFLevel} Imported','{currentWFLevel} Working','{currentWFLevel} Copied','{newWFLevel} Return to Formulator')"
'					statusSQL = $"('{currentWFLevel} Imported'), ('{currentWFLevel} Working'), ('{currentWFLevel} Copied'), ('{newWFLevel} Return to Formulator')"
					statusSQL = $"'{currentWFLevel} Imported','{currentWFLevel} Working','{currentWFLevel} Copied','{currentWFLevel} Returned to Formulation'"
					If iCurrentLevel > 2 Then statusSQL = $"{statusSQL},'{currentWFLevel} Demoted from {newWFLevel}'"
				Case wfName.XFContainsIgnoreCase("Validate")
					statusSQL = $"'{currentWFLevel} Ready for Validation','{currentWFLevel} Returned to Validation'"
				Case wfName.XFContainsIgnoreCase("Approve")
					statusSQL = $"'{currentWFLevel} Ready for Approval','{currentWFLevel} Approved'"
				Case wfName.XFContainsIgnoreCase("Review")
				End Select
				
				Dim SQL As New Text.StringBuilder
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   MAX(Case When ACCOUNT = 'REQ_TITLE' THEN TEXT END) AS 'Requirement Title', ")
					SQL.AppendLine($"   MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) AS 'Status' ")
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")
					SQL.AppendLine($"Join (VALUES {entitySQL}) AS ENTITY_LIST(VALUE) ON DATAATTACHMENT.ENTITY = ENTITY_LIST.VALUE ")
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{wfCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{wfScenario}' ")
'					SQL.AppendLine($"   And Exists (Select 1 from (VALUES {entitySQL}) AS ENTITY_LIST(ENTITY) where DATAATTACHMENT.ENTITY = ENTITY_LIST.ENTITY) ") 
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"   ENTITY, FLOW, SCENARIO ")
					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) IN ({statusSQL}) ")

					SQL.AppendLine($"; ")
					SQL.AppendLine($"  ")
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"A.ENTITYID, ") 
					SQL.AppendLine($"A.FLOWID, ")
					SQL.AppendLine($"  A.SCENARIOID, ") 
					SQL.AppendLine($"A.UD1ID, ")
					SQL.AppendLine($"A.UD2ID, ")
					SQL.AppendLine($"A.UD3ID, ")
					SQL.AppendLine($"A.UD4ID, ")
					SQL.AppendLine($"A.UD5ID, ")
					SQL.AppendLine($"A.UD6ID, ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId1} Then A.M12VALUE End) As 'Obligation {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId2} Then A.M12VALUE End) As 'Commitment {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId3} Then A.M12VALUE End) As 'Obligation UFR {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId4} Then A.M12VALUE End) As 'Commitment UFR {Year1}', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId1} Then B.M12VALUE End) As 'Obligation Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId2} Then B.M12VALUE End) As 'Commitment Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId3} Then B.M12VALUE End) As 'Obligation UFR Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId4} Then B.M12VALUE End) As 'Commitment UFR Carryover' ")
					SQL.AppendLine($"Into #FilteredDataRecords ")
					SQL.AppendLine($"    From DATARECORD{Year1} A ")
					SQL.AppendLine($"    FULL JOIN DATARECORD{Year2} B ")
					SQL.AppendLine($"On A.ENTITYID = B.ENTITYID And A.FLOWID = B.FLOWID And A.SCENARIOID = B.SCENARIOID And A.UD1ID = B.UD1ID And A.UD2ID = B.UD2ID And A.UD3ID = B.UD3ID And A.UD4ID = B.UD4ID And A.UD5ID = B.UD5ID And A.UD6ID = B.UD6ID")
					SQL.AppendLine($"    Where A.SCENARIOID = (Select MEMBERID From  MEMBER  Where NAME = '{wfScenario}' AND DIMTYPEID = 2 ) ")
					SQL.AppendLine($"    Group By ")
					SQL.AppendLine($"A.ENTITYID, ")
					SQL.AppendLine($"A.FLOWID, ")
					SQL.AppendLine($"A.SCENARIOID, ")
					SQL.AppendLine($"A.UD1ID, ")
					SQL.AppendLine($"A.UD2ID, ")
					SQL.AppendLine($"A.UD3ID, ")
					SQL.AppendLine($"A.UD4ID, ")
					SQL.AppendLine($"A.UD5ID, ")
					SQL.AppendLine($"A.UD6ID ")
					SQL.AppendLine($"; ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"Select ")
'					SQL.AppendLine($"    CONCAT(pd.ENTITY, ':', pd.FLOW) AS ENTITYFLOW, ")
					SQL.AppendLine($"    pd.ENTITY AS 'Funds Ctr', ")
					SQL.AppendLine($"    pd.FLOW AS 'REQ', ")
					SQL.AppendLine($"    m_ud1.NAME AS FUND, ")
					SQL.AppendLine($"    m_ud2.NAME AS MDEP, ")
					SQL.AppendLine($"    m_ud3.NAME AS APEPT, ")
					SQL.AppendLine($"    m_ud4.NAME AS DollarType, ")
					SQL.AppendLine($"    m_ud5.NAME AS CType, ")
					SQL.AppendLine($"    m_ud6.NAME AS 'ObjectClass', ")
					SQL.AppendLine($"    pd.[Requirement Title], ")
					SQL.AppendLine($"    pd.Status, ")
					SQL.AppendLine($"    Cast(A.[Obligation {Year1}] As BIGINT) AS 'Obligation {Year1}', ")
					SQL.AppendLine($"    Cast(A.[Obligation Carryover] As BIGINT) AS 'Obligation Carryover', ")
					SQL.AppendLine($"    Cast(A.[Commitment {Year1}] As BIGINT) AS 'Commitment {Year1}', ")
					SQL.AppendLine($"    Cast(A.[Commitment Carryover] As BIGINT) AS 'Commitment Carryover', ")
					SQL.AppendLine($"    Cast(A.[Obligation UFR {Year1}] As BIGINT) AS 'Obligation UFR {Year1}', ")
					SQL.AppendLine($"    Cast(A.[Obligation UFR Carryover] As BIGINT) AS 'Obligation UFR Carryover', ")
					SQL.AppendLine($"    Cast(A.[Commitment UFR {Year1}] As BIGINT) AS 'Commitment UFR {Year1}', ")	
					SQL.AppendLine($"    Cast(A.[Commitment UFR Carryover] As BIGINT) AS 'Commitment UFR Carryover' ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails pd ")
					SQL.AppendLine($"LEFT Join MEMBER m_Entity On pd.ENTITY= m_ENTITY.NAME ")
					SQL.AppendLine($"LEFT Join MEMBER m_Scenario On pd.SCENARIO = m_Scenario.NAME ")
					SQL.AppendLine($"LEFT Join MEMBER m_Flow On pd.FLOW= m_FLOW.NAME ")
					SQL.AppendLine($"LEFT Join #FilteredDataRecords A On m_ENTITY.memberID= A.ENTITYID And m_FLOW.memberID= A.FLOWID And A.SCENARIOID = m_Scenario.memberID ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud1 On A.UD1ID = m_ud1.memberId ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud2 On A.UD2ID = m_ud2.memberId ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud3 On A.UD3ID = m_ud3.memberId ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud4 On A.UD4ID = m_ud4.memberId ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud5 On A.UD5ID = m_ud5.memberId ")
					SQL.AppendLine($"LEFT Join MEMBER m_ud6 On A.UD6ID = m_ud6.memberId ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"WHERE A.[Obligation {Year1}] > 0 OR A.[Obligation Carryover] > 0 OR A.[Commitment {Year1}] > 0 OR A.[Commitment Carryover] > 0 OR A.[Obligation UFR {Year1}] > 0 OR A.[Obligation UFR Carryover] > 0 OR A.[Commitment UFR {Year1}] > 0 OR [Commitment UFR Carryover] > 0 ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"    pd.ENTITY, ")
					SQL.AppendLine($"    pd.FLOW, ")
					SQL.AppendLine($"    m_Flow.memberId, ")
					SQL.AppendLine($"    m_ud1.NAME, ")
					SQL.AppendLine($"    m_ud2.NAME, ")
					SQL.AppendLine($"    m_ud3.NAME, ")
					SQL.AppendLine($"    m_ud4.NAME, ")
					SQL.AppendLine($"    m_ud5.NAME, ")
					SQL.AppendLine($"    m_ud6.NAME, ")
					SQL.AppendLine($"    pd.[Requirement Title], ")
					SQL.AppendLine($"    pd.Status,  ")
					SQL.AppendLine($"    A.[Obligation {Year1}], ")
					SQL.AppendLine($"    A.[Commitment {Year1}], ")
					SQL.AppendLine($"    A.[Obligation UFR {Year1}], ")
					SQL.AppendLine($"    A.[Commitment UFR {Year1}], ")
					SQL.AppendLine($"    A.[Obligation Carryover], ")
					SQL.AppendLine($"    A.[Commitment Carryover], ")
					SQL.AppendLine($"    A.[Obligation UFR Carryover], ")
					SQL.AppendLine($"    A.[Commitment UFR Carryover] ")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    m_Flow.memberId ")

				
BRApi.ErrorLog.LogMessage(si,$"SQL: {sql}")	
				Dim dtAll As New DataTable
''Dim tStart As DateTime =  Date.Now()

				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
'Dim tStop As DateTime =  Date.Now()
'Dim tDuration As TimeSpan = tStop.Subtract(tStart)

'BRapi.ErrorLog.LogMessage(si, "KN SQL took " & tDuration.TotalSeconds.ToString("0.0000"))
'				Dim statusFilter As String() = {$"{currentWFLevel} Imported", $"{currentWFLevel} Working", $"{currentWFLevel} Copied", $"{newWFLevel} Return to Formulator"}
'				Dim filteredRows = From row In dtAll.AsEnumerable()Where statusFilter.Contains(row.Field(Of String)("Status"))Select row
'				Dim dtFiltered As DataTable = filteredRows.CopyToDataTable()
				'Me.WriteNameValuePairRow(si, dt, "Test Name", "Test Email")
				Return dtAll
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

#Region "Revise REQs By Target - Get Dimensional Members Data Set"
'=========================================================================================================================================================================================================================================
'==  WARNING: This method is used specifically for the Revise REQs By Target Linked Dashboard              																												     
'==           This method is used to return row headers for the Formulator to revise REQs' amount to match Funding Target   
'==              - Note: This function searches for specific inputted single member for each of the following dim: Entity, UD1, UD2, UD3 and return only Requirements with funding lines of this combination with the cuorrect Formulation statuses
'==           This method uses SQL to get the relevant information of requirements in correct status and return a datatable to be processed by BUD_Member_Lists > GetDimMbrsListReviseREQsByTarget
'=========================================================================================================================================================================================================================================
		Public Function GetDimMbrsDataSet(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try			
				Dim wfName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
   				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")			
				Dim cbxEntity As String = args.NameValuePairs.XFGetValue("cbxEntity","")
				Dim wfScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim Year1 As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				Dim Year2 As Integer = Year1 + 1
				Dim account1 As String = "Phased_Obligation_Base"
				Dim account2 As String = "Phased_Commitment"
				Dim account3 As String = "Phased_Obligation_UFR"
				Dim account4 As String = "Phased_Commitment_UFR"
				Dim iEntityID As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Entity.Id,sEntity)
				Dim iScenarioID As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Scenario.Id,wfScenario)
				Dim accountId1 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account1)
				Dim accountId2 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account2)
				Dim accountId3 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account3)
				Dim accountId4 As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Account.Id,account4)				
				
				Dim sDimDef As String = args.NameValuePairs.XFGetValue("DimDef","")
				Dim sU1Filter As String = args.NameValuePairs.XFGetValue("U1Filter","")
				Dim sU2Filter As String = args.NameValuePairs.XFGetValue("U2Filter","")
				Dim sU3Filter As String = args.NameValuePairs.XFGetValue("U3Filter","")
				Dim iUd1ID As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.UD1.Id ,sU1Filter)
				Dim iUd2ID As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.UD2.Id ,sU2Filter)
				Dim iUd3ID As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.UD3.Id ,sU3Filter)
				If String.IsNullOrWhiteSpace(sEntity) Then Return " "
					
				'--------- get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, cbxEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				entityText3 = entityText3.Substring(entityText3.Length -2, 2)
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = entityText3.Substring(1,1)
				Dim iCurrentLevel As Integer = CInt(currentStatus)
				Dim iNextLevel As Integer = iCurrentLevel - 1
				Dim currentWFLevel As String = "L" & iCurrentLevel
				Dim newWFLevel As String = "L" & iNextLevel
				
				
				'--------- derive status ----------
				Dim statusSQL As String = ""

				
				Select Case True
				Case wfName.XFContainsIgnoreCase("Formulate")
'					statusSQL = $"Where pd.Status IN ('{currentWFLevel} Imported','{currentWFLevel} Working','{currentWFLevel} Copied','{newWFLevel} Return to Formulator')"
'					statusSQL = $"('{currentWFLevel} Imported'), ('{currentWFLevel} Working'), ('{currentWFLevel} Copied'), ('{newWFLevel} Return to Formulator')"
					statusSQL = $"'{currentWFLevel} Imported','{currentWFLevel} Working','{currentWFLevel} Copied','{currentWFLevel} Returned to Formulation'"
					If iCurrentLevel > 2 Then statusSQL = $"{statusSQL},'{currentWFLevel} Demoted from {newWFLevel}'"
				Case wfName.XFContainsIgnoreCase("Validate")
					statusSQL = $"'{currentWFLevel} Ready for Validation','{currentWFLevel} Returned to Validation'"
				Case wfName.XFContainsIgnoreCase("Approve")
'brapi.ErrorLog.LogMessage(si,"Hit list")
					statusSQL = $"'{currentWFLevel} Ready for Approval'"
'				Case wfName.XFContainsIgnoreCase("CMD (BUD)")
'					statusSQL = $"'{currentWFLevel} Approved'"
'				Case wfName.XFContainsIgnoreCase("Review")
					
				End Select
				
				Dim SQL As New Text.StringBuilder
					SQL.AppendLine($"If object_id('tempdb..#PivotedDetails') is not null drop Table #PivotedDetails; ")
					SQL.AppendLine($"If object_id('tempdb..#FilteredDataRecords') is not null drop Table #FilteredDataRecords; ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"   ENTITY, ")
					SQL.AppendLine($"   FLOW, ")
					SQL.AppendLine($"   SCENARIO, ")
					SQL.AppendLine($"   MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) AS 'Status' ")
					SQL.AppendLine($"   Into #PivotedDetails ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"   DATAATTACHMENT ")
					SQL.AppendLine($"Where ")
					SQL.AppendLine($"   CUBE = '{wfCube}' ") 
					SQL.AppendLine($"   And SCENARIO = '{wfScenario}' ")
					SQL.AppendLine($"   And ENTITY = '{sEntity}' ") 
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"   ENTITY, FLOW, SCENARIO ")
					SQL.AppendLine($"   HAVING  MAX(Case When ACCOUNT = 'REQ_RQMT_STATUS' THEN TEXT END) IN ({statusSQL}) ")
					SQL.AppendLine($"; ")
					SQL.AppendLine($"  ")
					
					SQL.AppendLine($"Select ") 
					SQL.AppendLine($"A.ENTITYID, ") 
					SQL.AppendLine($"A.FLOWID, ")
					SQL.AppendLine($"A.SCENARIOID, ") 
					SQL.AppendLine($"A.UD1ID, ")
					SQL.AppendLine($"A.UD2ID, ")
					SQL.AppendLine($"A.UD3ID, ")
					SQL.AppendLine($"A.UD4ID, ")
					SQL.AppendLine($"A.UD5ID, ")
					SQL.AppendLine($"A.UD6ID, ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId1} Then A.M12VALUE End) As 'Obligation {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId2} Then A.M12VALUE End) As 'Commitment {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId3} Then A.M12VALUE End) As 'Obligation UFR {Year1}', ")
					SQL.AppendLine($"MAX(Case When A.ACCOUNTID = {accountId4} Then A.M12VALUE End) As 'Commitment UFR {Year1}', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId1} Then B.M12VALUE End) As 'Obligation Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId2} Then B.M12VALUE End) As 'Commitment Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId3} Then B.M12VALUE End) As 'Obligation UFR Carryover', ")
					SQL.AppendLine($"MAX(Case When B.ACCOUNTID = {accountId4} Then B.M12VALUE End) As 'Commitment UFR Carryover' ")
					SQL.AppendLine($"Into #FilteredDataRecords ")
					SQL.AppendLine($"    From DATARECORD{Year1} A ")
					SQL.AppendLine($"    FULL JOIN DATARECORD{Year2} B ")
					SQL.AppendLine($"On A.ENTITYID = B.ENTITYID And A.FLOWID = B.FLOWID And A.SCENARIOID = B.SCENARIOID And A.UD1ID = B.UD1ID And A.UD2ID = B.UD2ID And A.UD3ID = B.UD3ID And A.UD4ID = B.UD4ID And A.UD5ID = B.UD5ID And A.UD6ID = B.UD6ID")
					SQL.AppendLine($"    Where A.SCENARIOID = {iScenarioID} ")
					SQL.AppendLine($"	 AND A.ENTITYID = {iEntityID} ")
					SQL.AppendLine($"	 AND A.UD1ID = {iUd1ID} ")
					SQL.AppendLine($"	 AND A.UD2ID = {iUd2ID} ")
					SQL.AppendLine($"	 AND A.UD3ID = {iUd3ID} ")
'					SQL.AppendLine($"	 AND A.M12VALUE > 0 ")
					SQL.AppendLine($"    Group By ")
					SQL.AppendLine($"A.ENTITYID, ")
					SQL.AppendLine($"A.FLOWID, ")
					SQL.AppendLine($"A.SCENARIOID, ")
					SQL.AppendLine($"A.UD1ID, ")
					SQL.AppendLine($"A.UD2ID, ")
					SQL.AppendLine($"A.UD3ID, ")
					SQL.AppendLine($"A.UD4ID, ")
					SQL.AppendLine($"A.UD5ID, ")
					SQL.AppendLine($"A.UD6ID ")
					SQL.AppendLine($"; ")
					SQL.AppendLine($" ")

					' -------- Version 1 ----------
'					SQL.AppendLine($"Select ")
''					SQL.AppendLine($"    CONCAT(pd.ENTITY, ':', pd.FLOW) AS ENTITYFLOW, ")
'					SQL.AppendLine($"    pd.ENTITY AS Entity, ")
'					SQL.AppendLine($"    pd.FLOW AS Flow, ")
''					SQL.AppendLine($"    m_ud1.NAME AS U1, ")
''					SQL.AppendLine($"    m_ud2.NAME AS U2, ")
''					SQL.AppendLine($"    m_ud3.NAME AS U3, ")
'					SQL.AppendLine($"    m_ud4.NAME AS U4, ")
'					SQL.AppendLine($"    m_ud5.NAME AS U5, ")
'					SQL.AppendLine($"    m_ud6.NAME AS U6 ")
'					SQL.AppendLine($"From ")
'					SQL.AppendLine($"    #PivotedDetails pd ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_Entity On pd.ENTITY= m_ENTITY.NAME ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_Scenario On pd.SCENARIO = m_Scenario.NAME ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_Flow On pd.FLOW= m_FLOW.NAME ")
'					SQL.AppendLine($"LEFT JOIN #FilteredDataRecords A On m_ENTITY.memberID= A.ENTITYID And m_FLOW.memberID= A.FLOWID And A.SCENARIOID = m_Scenario.memberID ")
''					SQL.AppendLine($"LEFT JOIN MEMBER m_ud1 On A.UD1ID = m_ud1.memberId ")
''					SQL.AppendLine($"LEFT JOIN MEMBER m_ud2 On A.UD2ID = m_ud2.memberId ")
''					SQL.AppendLine($"LEFT JOIN MEMBER m_ud3 On A.UD3ID = m_ud3.memberId ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_ud4 On A.UD4ID = m_ud4.memberId ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_ud5 On A.UD5ID = m_ud5.memberId ")
'					SQL.AppendLine($"LEFT JOIN MEMBER m_ud6 On A.UD6ID = m_ud6.memberId ")
'					SQL.AppendLine($" ")
'					SQL.AppendLine($"WHERE (A.[Obligation {Year1}] > 0 OR A.[Obligation Carryover] > 0 OR A.[Commitment {Year1}] > 0 OR A.[Commitment Carryover] > 0 OR A.[Obligation UFR {Year1}] > 0 OR A.[Obligation UFR Carryover] > 0 OR A.[Commitment UFR {Year1}] > 0 OR [Commitment UFR Carryover] > 0) ")
'					SQL.AppendLine($"Group By ")
'					SQL.AppendLine($"    pd.ENTITY, ")
'					SQL.AppendLine($"    pd.FLOW, ")
'					SQL.AppendLine($"    m_Flow.memberId, ")
''					SQL.AppendLine($"    m_ud1.NAME, ")
''					SQL.AppendLine($"    m_ud2.NAME, ")
''					SQL.AppendLine($"    m_ud3.NAME, ")
'					SQL.AppendLine($"    m_ud4.NAME, ")
'					SQL.AppendLine($"    m_ud5.NAME, ")
'					SQL.AppendLine($"    m_ud6.NAME ")
'					SQL.AppendLine($"Order By ")
'					SQL.AppendLine($"    m_Flow.memberId ")

					' -------- Version 2 ----------
					SQL.AppendLine($"Select ")
					SQL.AppendLine($"    pd.ENTITY AS Entity, ")
					SQL.AppendLine($"    pd.FLOW AS Flow, ")
					SQL.AppendLine($"    A.UD4ID AS U4, ")
					SQL.AppendLine($"    A.UD5ID AS U5, ")
					SQL.AppendLine($"    A.UD6ID AS U6 ")
					SQL.AppendLine($"From ")
					SQL.AppendLine($"    #PivotedDetails pd ")
					SQL.AppendLine($"LEFT JOIN MEMBER m_Entity On pd.ENTITY= m_ENTITY.NAME ")
					SQL.AppendLine($"LEFT JOIN MEMBER m_Scenario On pd.SCENARIO = m_Scenario.NAME ")
					SQL.AppendLine($"LEFT JOIN MEMBER m_Flow On pd.FLOW= m_FLOW.NAME ")
					SQL.AppendLine($"LEFT JOIN #FilteredDataRecords A On m_ENTITY.memberID= A.ENTITYID And m_FLOW.memberID= A.FLOWID And A.SCENARIOID = m_Scenario.memberID ")
					SQL.AppendLine($" ")
					SQL.AppendLine($"WHERE (A.[Obligation {Year1}] > 0 OR A.[Obligation Carryover] > 0 OR A.[Commitment {Year1}] > 0 OR A.[Commitment Carryover] > 0 OR A.[Obligation UFR {Year1}] > 0 OR A.[Obligation UFR Carryover] > 0 OR A.[Commitment UFR {Year1}] > 0 OR [Commitment UFR Carryover] > 0) ")
					SQL.AppendLine($"Group By ")
					SQL.AppendLine($"    pd.ENTITY, ")
					SQL.AppendLine($"    pd.FLOW, ")
					SQL.AppendLine($"    m_Flow.memberId, ")
					SQL.AppendLine($"    A.UD4ID, ")
					SQL.AppendLine($"    A.UD5ID, ")
					SQL.AppendLine($"    A.UD6ID ")
					SQL.AppendLine($"Order By ")
					SQL.AppendLine($"    m_Flow.memberId ")
				
'BRApi.ErrorLog.LogMessage(si,$"SQL: {sql}")	
				Dim dtAll As New DataTable
''Dim tStart As DateTime =  Date.Now()

				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
'BRApi.ErrorLog.LogMessage(si,$"DataSet dtAll: {dtAll.Rows.Count}")			
				Return dtAll
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try	
		End Function
#End Region

	End Class
End Namespace
