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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_PGM_DataSet
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
						Select Case args.DataSetName.ToLowerInvariant()
							Case "reqtitlelist"
								Return Me.REQTitleList()
							Case "reqlistbyentityandstatus"
								Return Me.REQListByEntityAndStatus(si, globals, api, args)
						End Select



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


'#Region "REQ List by Cached Entity"

'						If args.DataSetName.XFEqualsIgnoreCase("REQListCachedEntity") Then
'							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
'					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
'							Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
'							Dim ExistingREQs As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"F_REQ_Main","F#Command_Requirements.Base",True).OrderBy(Function(x) x.Member.name).ToList()
'							Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQListCachedEntity")

'								For Each ExistingREQ As MemberInfo In ExistingREQs
'						            Dim REQEntityMemberScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sScenario & ":T#" & sREQTime & ":V#Annotation:A#REQ_Title:F#" & ExistingREQ.Member.Name & ":O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'						            Dim TitleValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, REQEntityMemberScript).DataCellEx.DataCellAnnotation
		               
'		                            If Not String.IsNullOrWhiteSpace(TitleValue) Then
'										Me.WriteNameValuePairRow(si, dt, sFundCenter & " - " & ExistingREQ.Member.Name & " - " & TitleValue, ExistingREQ.Member.Name)

'									End If 

'								Next

'							Return dt 	
							
'						End If


'#End Region  'Updated

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
	
#Region "REQTitleList"
		Public Function REQTitleList() As DataTable
			Try	
'Dim tStart As DateTime =  Date.Now()				
'brapi.ErrorLog.LogMessage(si,"START: "	& System.DateTime.Now)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQTitleList")				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.GetWFInfoDetails(si)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
'brapi.ErrorLog.LogMessage(si,"sProfileSubString: " & sProfileSubString)							
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")


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
							   Req.REQ_ID,
							   Req.Status
						FROM XFC_CMD_PGM_REQ AS Req
						
						WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
						AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
						AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'	
						Order By Req.Entity,Req.REQ_ID"

				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQTitleList")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using

				'Build dt to return 
				For Each dataRow As DataRow In dtAll.Rows					
					
					Dim FundCenter As String = dataRow.Item("Entity")
					Dim ExistingREQ As String =  dataRow.Item("REQ_ID")
					Dim TitleValue As String = ""
					If dbnull.Value.Equals(dataRow.Item("Title")) Then
						TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
					Else
						 TitleValue  =  dataRow.Item("Title")
					End If 

					Dim REQStatus As String = dataRow.Item("Status")
					
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
'BRApi.ErrorLog.LogMessage(si, "wfstatus " & reqWFStatusLevel)					
					If String.IsNullOrWhiteSpace(TitleValue) Then Continue For
'BRApi.ErrorLog.LogMessage(si, "Fund Center " & FundCenter)	
					'================================Non-CMD level Manage WF======================		
					If (sProfileName = "Manage Requirements") Or (sProfileName = "Manage Requirements" And REQStatus.XFEqualsIgnoreCase("BlankStatus")) Then	
						
							If REQStatus.XFEqualsIgnoreCase("Returned From " & newWFLevel) Or  REQStatus.XFEqualsIgnoreCase("Returned From " & reqWFStatusLevel) Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)	
							End If
						
							If Not REQStatus.XFEqualsIgnoreCase("Returned From CMD") And Not REQStatus.XFEqualsIgnoreCase("Ready for CMD Prioritization") And Not REQStatus.XFEqualsIgnoreCase("CMD Prioritized") And Not REQStatus.XFEqualsIgnoreCase("CMD Approved") And Not REQStatus.XFEqualsIgnoreCase("Returned From L2")  And Not REQStatus.XFEqualsIgnoreCase("L2 Working") And Not REQStatus.XFEqualsIgnoreCase("L2 Copied")Then
								If reqWFStatusLevel = entityText3 Or (REQStatus.XFEqualsIgnoreCase(newWFLevel & " Ready for Validation")) Then			
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
								End If
							End If
						
					'================================Command Manage WF======================	
					'*** not necessary?
					ElseIf ( sProfileName = "Manage Requirements CMD") Or ( sProfileName = "Manage Requirements CMD") And REQStatus.XFEqualsIgnoreCase("BlankStatus") Then
						
							If REQStatus.XFEqualsIgnoreCase("Returned From CMD") Or REQStatus.XFEqualsIgnoreCase("Returned From L2") Then 						
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)	
							End If
						
					'***
							If REQStatus.XFEqualsIgnoreCase("L2 Ready for Prioritization") Or REQStatus.XFEqualsIgnoreCase("L2 Prioritized") Or REQStatus.XFEqualsIgnoreCase("L2 Ready for Validation" ) Or REQStatus.XFEqualsIgnoreCase("L2 Ready for Approval" ) Or REQStatus.XFEqualsIgnoreCase("L2 Approved" ) Or REQStatus.XFEqualsIgnoreCase("L2 Working") And Not ExistingREQ.XFContainsIgnoreCase("REQ_00") Then 			
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
											
					'================================Formulate WF======================	
					Else If sProfileName = "Formulate Requirements" 
							If (REQStatus.XFContainsIgnoreCase("Working") Or REQStatus.XFContainsIgnoreCase("Copied") Or REQStatus.XFContainsIgnoreCase("Imported")) And Not REQStatus.XFContainsIgnoreCase("L2")  Then  'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If		

					'================================Formulate CMD WF======================	
					Else If sProfileName = "Formulate Requirements CMD" 
							If (REQStatus = "L2 Working" Or REQStatus = "L2 Copied" Or REQStatus = "L2 Imported")  And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Manpower") Then 
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
Public Function REQListByEntityAndStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try		
'Brapi.ErrorLog.LogMessage(si, "In DataSet 2")
				Dim dt As New Datatable()			

				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name

			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.GetWFInfoDetails(si)
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,8)
						
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
				
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
				'Brapi.ErrorLog.LogMessage(si, "FC:" & FC)
'
				If Not cbMembers.Count > 0 Then
					Return dt
				End If
				
							
				'---Checking if Base------

				Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
					
         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)
					
				Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FC.Replace("_General",""))	
				
				Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
				'Brapi.ErrorLog.LogMessage(si,"Here 2.4")	
				If Not isBase Then FC = FC & ".Base"
	
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#" & FC,True)
					For Each FundCenter As MemberInfo In LFundCenters
						If FCMulti.Length > 0 Then
							FCMulti.Append(",")
						End If
						FCMulti.Append($"'{FundCenter.Member.Name}'")
					Next
					'Brapi.ErrorLog.LogMessage(si,"Here 2.6")
				'--------- get Entity Text3 --------- 
				Dim sFC As String = FC.Replace(".Base","")
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
								
			'Brapi.ErrorLog.LogMessage(si,"Here 3")
			
				'========================================================================== Define Targeted Status based on WF and Dashboard's FC selection ==========================================================================
				
				Select Case True
					Case wfProfileName.XFContainsIgnoreCase("Manage")		

					uniqueStatuses.Add($"'% Working'")
					uniqueStatuses.Add($"'% Copied'")
					uniqueStatuses.Add($"'% Imported'")
					uniqueStatuses.Add($"'% Ready For Validation'")
					uniqueStatuses.Add($"'% Ready For Prioritization'")
					uniqueStatuses.Add($"'% Prioritized'")
					uniqueStatuses.Add($"'% Ready For Approval'")	
					
				Case sProfileName.XFContainsIgnoreCase("Formulate")
					uniqueStatuses.Add($"'{curWFLevel} Working'")
					uniqueStatuses.Add($"'{curWFLevel} Copied'")
					uniqueStatuses.Add($"'{curWFLevel} Imported'")
		
				
				Case wfProfileName.XFContainsIgnoreCase("Rollover")
					sREQTime = (Convert.ToInt32(sREQTime) - 1).ToString
					sScenario = $"CMD_PGM_C{sREQTime}"
				
			Case wfProfileName.XFContainsIgnoreCase("Validate")
					uniqueStatuses.Add($"'{curWFLevel} Ready For Validation'")
				
				Case wfProfileName.XFContainsIgnoreCase("Prioritize")
					uniqueStatuses.Add($"'{curWFLevel} Ready For Prioritization'")
					uniqueStatuses.Add($"'{curWFLevel} Prioritized'")
				
				Case wfProfileName.XFContainsIgnoreCase("Approve")
					uniqueStatuses.Add($"'{curWFLevel} Ready For Approval'")
				
				
			Case wfProfileName.XFContainsIgnoreCase("Review")
					uniqueStatuses.Add($"'% Working'")
					uniqueStatuses.Add($"'% Copied'")
					uniqueStatuses.Add($"'% Imported'")
					uniqueStatuses.Add($"'% Ready For Validation'")
					uniqueStatuses.Add($"'% Ready For Prioritization'")
					uniqueStatuses.Add($"'% Prioritized'")
					uniqueStatuses.Add($"'% Ready For Approval'")				
				
				Case wfProfileName.XFContainsIgnoreCase("CMD (PGM)")
					uniqueStatuses.Add($"'{curWFLevel} Approved'")
					

				End Select
Next

				'Set up where clauses for getting the annotation from the table
				'This approach was more performant that getDataCell method call
				Dim allREQs As String = ""
				Dim allFCs As String = ""
				
					
				allFCs = FCMulti.ToString()
				Dim sTgtStatus As String = String.Join(",", uniqueStatuses)

				'If after looping, we have no valid entities or statuses, return an empty table.
				If String.IsNullOrWhiteSpace(allFCs) Or String.IsNullOrWhiteSpace(sTgtStatus) Then
					Return dt
				End If
			
				
				
			
				Dim SQL As New Text.StringBuilder

'----------------------------------------

SQL.Append($"SELECT 
          Req.Title As [Title],
          Req.REQ_ID,
          Req.Entity As [Funds Center],
          Req.Status,
          Dtl.Account,
          Dtl.Flow,
          Dtl.UD1 as [APPN],
          Dtl.UD2 As [MDEP],
          Dtl.UD3 as [APE],
          Dtl.UD4 as [DollarType],
          Dtl.UD5 As [cType],
          Dtl.UD6 As [Object Class],
          Dtl.FY_1,
          Dtl.FY_2,
          Dtl.FY_3,
          Dtl.FY_4,
          Dtl.FY_5
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
								AND  Dtl.Account = 'REQ_Requested_Amt'
								AND Req.Status IN ({sTgtStatus})
								AND Req.Entity IN ({allFCs})
								Order By Req.Entity,Req.REQ_ID
")



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


