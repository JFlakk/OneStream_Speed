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
						Select Case args.DataSetName.ToLower()
							Case "reqtitlelist"
								Return Me.REQTitleList()
							Case "reqlistbyentityandstatus"
								Return Me.REQListByEntityAndStatus()
							Case "getallusers"							
								Return Me.GetAllUsers(si,globals,api,args)	
							Case "get_copyreq_acctvalues"
								Return Me.Get_CopyReq_AcctValues()	
							Case "get_copyreq_list"
								Return Me.Get_CopyReq_List()
							Case "getuserfundscenterbywf"
								Return Me.GetUserFundsCenterByWF(si, globals, api, args)
							Case "getreqstatus"
								Return Me.GetReqStatus()
							Case "fullreqlist"
								Return Me.FullREQList()
							Case "getdemotestatuslist"
								Return Me.GetDemoteStatusList()
						End Select

#Region "GetFileNamesInFolder" 
						'Return a list of files in a folder
						If args.DataSetName.XFEqualsIgnoreCase("GetFileNamesInFolder") Then
							Return Me.GetFileNamesInFolder(si,globals,api,args)	
						End If						
#End Region 'Updated 09/23/2025

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
							Dim sPositions As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si,"S_RMW","S#HQDA_Programming.base.Where((Name Contains " & sCurrYear & ") or (Name Contains " & sPriorYear & "))" ,True)
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
		
#Region "Email Stakeholder or Validators"

		Public Function GetAllUsers(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs, Optional ByVal mode As String = "AllFC_CMD") As Object
			Try	
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode", mode)
				Dim sEntity As String = args.NameValuePairs.XFGetValue("entity", "")
				sEntity = sEntity.Replace("_General","")
				
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name
				Dim wfProfileAccessGroupmodule As String = wfProfileAccessGroup.split("_")(1)
				Dim WFValidator As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"VA")
				Dim SQL As String = String.Empty
				Dim tableName As String = String.Empty
				
			Select Case sMode
				Case "AllStakeholders"				
						SQL= $"Select Distinct U.Name As UserName, U.Email As UserEmail
						From  [SecUser] As u
						Join  [SecGroupChild] As GU On gu.ChildKey = u.uniqueid
						Join  [SecGroup] As SG On sg.UniqueID = gu.GroupKey
						Where (SG.Name Like  '%{wfProfileAccessGroupmodule}%' AND SG.Name like '%{sEntity}')"
						tableName = "AllStakeholders"
						
				Case "Validators"
						SQL = $" SELECT Distinct U.Name as UserName, U.Email as UserEmail
						FROM  [SecUser] as u
						join  [SecGroupChild] AS GU on gu.ChildKey = u.uniqueid
						join  [SecGroup] AS SG on sg.UniqueID = gu.GroupKey
						Where (SG.Name like  '{WFValidator}%' AND SG.Name like '%{sEntity}')"
						tableName = "Validators"
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

#End Region	'Updated 10/15

#Region "Get Copy REQ DataSets"
		Public Function Get_CopyReq_List() As DataTable
			Try
				Dim sFundsCenter As String = args.NameValuePairs.XFGetValue("FC","NA")
				Dim copyREQAcct As String = args.NameValuePairs.XFGetValue("copyREQAcct","NA")
				Dim	copyREQAcctVal As String = args.NameValuePairs.XFGetValue("copyREQAcctVal","NA")
				Dim	sKeyword As String = args.NameValuePairs.XFGetValue("Keyword","NA")
				Dim	srcScenario As String = args.NameValuePairs.XFGetValue("scenario","NA")
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim dt As New DataTable
				'dt.TableName = "REQListForCopy"
'BRApi.ErrorLog.LogMessage(si, "srcScenario: " & srcScenario & ", sFundsCenter: " & sFundsCenter)				
				'Return an empty datatable if no fund center was selected
				'Also to address a behavior where on initial load, |!param!| was passed in as a text instead of the selected value (which ought to be empty), an if statement is used here to address that
				If sFundsCenter.XFEqualsIgnoreCase("NA") Or sFundsCenter.Contains("|!") Then 
					dt.Columns.Add("Name")
					dt.Columns.Add("Value")
					Return dt
				End If
				If copyREQAcct.Contains("|!") Or String.IsNullOrWhiteSpace(copyREQAcct) Then copyREQAcct = "NA"
				If copyREQAcctVal.Contains("|!") Or String.IsNullOrWhiteSpace(copyREQAcctVal) Then copyREQAcctVal = "NA"
				If sKeyword.Contains("|!") Or String.IsNullOrWhiteSpace(sKeyword) Then sKeyword  = "NA"
				If srcScenario.Contains("|!") Or String.IsNullOrWhiteSpace(srcScenario) Then srcScenario  = "NA"
				
				
				'Build SQL to return list of requirements that fit the user's criterias
							'Build SQL to return list of requirements that fit the user's criterias
				Dim SQL As String = $"Select CONCAT(REQ_ID,' - ',Title) as Name, REQ_ID as Value
									 From XFC_CMD_PGM_REQ 
									 Where Entity = '{sfundsCenter}' 
									 AND WFScenario_Name = '{srcScenario}'
									 AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
									 AND WFTime_Name = '{wfInfoDetails("TimeName")}'
				                     And Title Is Not NULL"
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)				
			
				If Not copyREQAcct.XFEqualsIgnoreCase("NA") And Not copyREQAcctVal.XFEqualsIgnoreCase("NA") Then 
					SQL &= $" AND {copyREQAcct} = '{copyREQAcctVal}'"
				End If
				If Not sKeyword.XFEqualsIgnoreCase("NA") Then 
					SQL &= $" AND (Title LIKE '%{sKeyword}%' OR Description LIKE '%{sKeyword}%')"
				End If	
				
				'Fetch datatable 
	
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dt = BRApi.Database.ExecuteSql(dbConn,SQL,True)
				End Using
'BRApi.ErrorLog.LogMessage(si, "return rec count: " & dt.TableName & " : " & dt.Rows.Count)				
				dt.TableName = "REQListForCopy"
				Return dt							
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try	
		End Function

		Public Function Get_CopyReq_AcctValues() As DataTable
			Try	
				Dim fundsCenter As String = args.NameValuePairs.XFGetValue("fundsCenter","NA")
				Dim copyREQAcct As String = args.NameValuePairs.XFGetValue("copyREQAcct","NA")
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim dt As New DataTable
				dt.TableName = "AccountVals"
				If copyREQAcct = "NA" Or copyREQAcct.XFContainsIgnoreCase("|") Or String.IsNullOrWhiteSpace(copyREQAcct)
					dt.Columns.Add("Text")
					Return dt
				End If
				Dim SQL As String = $"Select DISTINCT {copyREQAcct} as Name
									 From XFC_CMD_PGM_REQ 
									 Where Entity = '{fundsCenter}' 
									 AND WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
									 AND WFCMD_Name = '{wfInfoDetails("CMDName")}'
									 AND WFTime_Name = '{wfInfoDetails("TimeName")}'"
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.ExecuteSql(dbConn,SQL,True)
				End Using
				
				Return dt
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try	
		End Function

#End Region
	
#Region "REQTitleList"
		Public Function REQTitleList() As DataTable
			Try	
'Dim tStart As DateTime =  Date.Now()				
'brapi.ErrorLog.LogMessage(si,"START: "	& System.DateTime.Now)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "REQTitleList")				
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				'Dim sProfileSubString As String = wfProfileName.Substring(x + ".".Length-1,13)
'brapi.ErrorLog.LogMessage(si,"sProfile: " & sProfileName)							
				Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
		        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
		        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
'				Dim sREQType As String = args.NameValuePairs.XFGetValue("REQType")

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
							   Req.REQ_ID_Type,
							   Req.Status
						FROM XFC_CMD_PGM_REQ AS Req
						
						WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
						AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
						AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
						And Req.Entity IN ({allFcs})
						Order By Req.Entity,Req.REQ_ID"

'BRApi.ErrorLog.LogMessage(si, "SQL " & SQL.ToString)					
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
					Dim REQType As String = dataRow.Item("REQ_ID_Type")
					
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
							If Not REQStatus.XFEqualsIgnoreCase("L2_") Then
								If reqWFStatusLevel = entityText3 Then			
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
								End If
							End If
						
					'================================Command Manage WF======================	
					'*** not necessary?
					ElseIf ( sProfileName = "Manage CMD Requirements") Or ( sProfileName = "Manage CMD Requirements" And REQStatus.XFEqualsIgnoreCase("BlankStatus")) Then
							If REQStatus.XFContainsIgnoreCase("L2_") Then 			
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
											
					'================================Formulate WF======================	
					Else If sProfileName = "Formulate Requirements" 
							If (REQStatus.XFContainsIgnoreCase("Formulate") And Not REQStatus.XFContainsIgnoreCase("L2"))  Then  'updated
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If		

					'================================Formulate CMD WF======================	
					Else If sProfileName = "Formulate CMD Requirements"
							If (REQStatus = "L2_Formulate_PGM") And Not args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Mpr") and not REQType.XFEqualsIgnoreCase("Manpower") Then 
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
							End If
							If (REQStatus = "L2_Formulate_PGM" Or REQStatus = "L2_Final_PGM") 
								If args.NameValuePairs.XFGetValue("Dashboard").XFContainsIgnoreCase("Mpr") And REQType.XFEqualsIgnoreCase("Manpower") Then
       								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
								End If 
							End If			
					'================================Validate REQs WF======================			
					ElseIf sProfileName = "Validate Requirements" Then
							If reqWFStatusLevel = entityText3 And REQStatus.Contains("Validate") And Not REQStatus.Contains("L2")Then								
								Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
							End If
					'================================Validate CMD WF ======================		
					ElseIf sProfileName = "Validate CMD Requirements" Then
						If REQStatus = ("L2_Validate_PGM")  Then 'updated
									Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If 	
				
					'================================Prioritize WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Prioritize") Then
						If REQStatus.XFContainsIgnoreCase("Prioritize")  Then 'updated
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If
					'================================Approve WF======================	
					ElseIf wfProfileName.XFContainsIgnoreCase("Approve") Then
						
						If REQStatus.XFContainsIgnoreCase("Approve") And reqWFStatusLevel = entityText3 Then 'updated
							
							Me.WriteNameValuePairRow(si, dt, FundCenter & " - " & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated
						End If
							'================================Review Requirements WF======================			
				ElseIf wfProfileName.XFContainsIgnoreCase("Review") Then
						Me.WriteNameValuePairRow(si, dt, FundCenter & " - "  & ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)'updated 			
					End If 
'				
				Next
'BRApi.ErrorLog.LogMessage(si, "returned row count: " & dt.Rows.Count)

				Return dt 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
Public Function REQListByEntityAndStatus() As Object
			Try		
'Brapi.ErrorLog.LogMessage(si, "In DataSet 2")
				Dim dt As New Datatable()			

				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'Brapi.ErrorLog.LogMessage(si,"WFPRfile" & wfProfileName)
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
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
								'Brapi.ErrorLog.LogMessage(si,"Text3" & curWFLevel)	
				'Brapi.ErrorLog.LogMessage(si,"Here 3")
				
					'========================================================================== Define Targeted Status based on WF and Dashboard's FC selection ==========================================================================
					
					Select Case True
						Case sProfileName.XFContainsIgnoreCase("Manage Requirements")		
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_PGM'")
						
						Case sProfileName.XFContainsIgnoreCase("Manage CMD")		
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Final_PGM'")
							
						Case sProfileName.XFContainsIgnoreCase("Formulate")
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_PGM'")
							
				
						
						Case sProfileName.XFContainsIgnoreCase("Rollover")
							sREQTime = (Convert.ToInt32(sREQTime) - 1).ToString
							sScenario = $"CMD_PGM_C{sREQTime}"
							wfInfoDetails("ScenarioName") = sScenario
							wfInfoDetails("TimeName") = sREQTime
							'uniqueStatuses.Add($"'L2_CMD_PGM_Final_Reqs'")
							uniqueStatuses.Add($"'L2_Final_PGM'")
						Case sProfileName.XFContainsIgnoreCase("Validate")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_PGM'")
						
						Case sProfileName.XFContainsIgnoreCase("Prioritize")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_PGM'")
							
						Case sProfileName.XFContainsIgnoreCase("Approve") 
							uniqueStatuses.Add($"'{curWFLevel}_Approve_PGM'")
							
						
						
						Case wfProfileName.XFContainsIgnoreCase("(CMD PGM)")
						'	Brapi.ErrorLog.LogMessage(si,"HERE 4")
							uniqueStatuses.Add($"'{curWFLevel}_Formulate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Validate_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Prioritize_PGM'")
							uniqueStatuses.Add($"'{curWFLevel}_Approve_PGM'")	
							
							uniqueStatuses.Add($"'L2_Final_PGM'")
					
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

SQL.Append($"SELECT 
		  Req.CMD_PGM_REQ_ID,
          Req.Title As [Title],
          Req.REQ_ID,
          Req.Entity As [Funds Center],
          Dtl.Flow,
          Dtl.Account,
          Dtl.UD1 as [APPN],
          Dtl.UD2 As [MDEP],
          Dtl.UD3 as [APE],
          Dtl.UD4 as [DollarType],
          Dtl.UD5 As [cType],
          Dtl.UD6 As [Object Class],
          FORMAT(Dtl.FY_1, 'N0') As FY_1,
         FORMAT(Dtl.FY_2, 'N0') AS FY_2,
         FORMAT(Dtl.FY_3, 'N0') AS FY_3,
          FORMAT(Dtl.FY_4, 'N0') AS FY_4,
          FORMAT(Dtl.FY_5, 'N0') AS FY_5
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
								AND Dtl.Account = 'Req_Funding'
								AND Dtl.Unit_Of_Measure = 'Funding'
								AND Dtl.Flow IN ({sTgtStatus})
								AND Req.Entity IN ({allFCs})
								Order By Req.Entity,Req.REQ_ID
")


'brapi.ErrorLog.LogMessage(si,"@SQL" & SQL.ToString)
				'Dim dtFetch As New DataTable
				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "REQListByEntityAll")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)

					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						
				End Using
'brapi.ErrorLog.LogMessage(si,"dtAll returned: " & dtAll.Rows.Count)
				Return dtAll 	
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
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
#End Region  'New

#Region "GetFileNamesInFolder"	'Updated 09/23/2025
		'Get List of File Names in Folder
		Public Function GetFileNamesInFolder(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs)
			Try		
'Brapi.ErrorLog.LogMessage(si, "Dataset file")				
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sFolder As String = $"Documents/Public/CMD_Programming/{wfcube}"
'Brapi.ErrorLog.LogMessage(si, "fodler:"  + sFolder)				
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ListOfFiles")							
				Dim objList As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sFolder, XFFileType.All, False, False, False)

For Each item As NameAndAccessLevel In objList				
'Brapi.ErrorLog.LogMessage(si, "itme name: " & item.AccessLevel.ToString )	
Next 


				For Each item As NameAndAccessLevel In objList
'Brapi.ErrorLog.LogMessage(si, "hit1" )					
					Dim sFileName As String = item.Name.Substring(item.Name.LastIndexOf("/") + 1)
'Brapi.ErrorLog.LogMessage(si, "sFileName: " & sFileName)						
					Dim sFileNameLower As String = sFileName.ToLower
					If sFileNameLower.StartsWith(wfCube.ToLower)  And Not sFileNameLower.EndsWith("pdf") Then
						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
					End If
'					If Not wfCube.XFContainsIgnoreCase("Army") And sFileNameLower.StartsWith("Army".ToLower) Then
'						Me.WriteNameValuePairRow(si, dt, sFileName, sFileName)
'					End If
				Next
'Brapi.ErrorLog.LogMessage(si, "hit2" )				
				sFolder = $"Documents/Public/CMD_Programming/ARMY"
				Dim objListArmy As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, FileSystemLocation.ApplicationDatabase, sFolder, XFFileType.All, False, False, False)
				For Each item As NameAndAccessLevel In objListArmy
'Brapi.ErrorLog.LogMessage(si, "hit3" )					
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

#Region "Security Logic: Get User Fund Centers By Workflow"
'Updated 07/19 ticket 1484 by KL, MF, CM
'Updated 5/29/2025 MF. New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
Public Function GetUserFundsCenterByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
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
						'Brapi.ErrorLog.LogMessage(si, "FC" &  fc.Member.Name)
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

#Region "GetReqStatus"
Public Function GetReqStatus() As Object
			Try
					'Get WF Profile and Status type
				
				
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim sEntityGeneral As String = globals.GetStringValue("Entity") 'args.NameValuePairs.XFGetValue("Entity")
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sEntity As String = ""
				If sEntityGeneral.XFContainsIgnoreCase("General")
					sEntity = sEntityGeneral.Replace("_General","")
				Else 
					sEntity = sEntityGeneral
				End If 
				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube) 
				Dim bHasChildren As Boolean = Nothing
				'Brapi.ErrorLog.LogMessage(si, "Entity CMJM = " & sEntity)
				
				
				
				If  Not String.IsNullOrWhiteSpace(sEntity) Then
					bHasChildren = brapi.Finance.Members.HasChildren(si,DimPk,brapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id,sEntity))
				End If
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)	
				'Brapi.ErrorLog.LogMessage(si, "Entity" & sEntityGeneral)
						'--------- Get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
		
				Dim level As String  = String.Empty
				Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				If Not String.IsNullOrWhiteSpace(entityText3) AndAlso entityText3.StartsWith("EntityLevel") Then
					level = entityText3.Substring(entityText3.Length -2, 2)
				End If
		
		
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = level.Substring(1,1)
				Dim icurrentStatus As Integer = currentStatus
				icurrentStatus = icurrentStatus - 1
				Dim currentWFLevel As String = icurrentStatus
				Dim newWFLevel As String = "L" & currentWFLevel
				
				'--------------------------------
				'---Get WF Status for Manager----
				'--------------------------------
				'
				If sProfileName = "Manage CMD Requirements" Then
				
						Dim statusValues As String = level & "_Formulate_PGM," & level & "_Validate_PGM," &  level & "_Prioritize_PGM," &  level & "_Approve_PGM," &  level & "_Final_PGM"
				
						'Get the stored status list and turn it into a data table (Removes Square Brackets)
						Dim statusItems As List(Of String) = StringHelper.SplitString(statusValues,",")
						
						'Create and fill it from the list of status Values
						Dim dt As DataTable = Me.CreateNameValuePairTable(si, "GetValidStatusListForManager")

						For Each statusItem As String In statusItems
							Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
						Next		

						Return dt
				ElseIf sProfileName = "Manage Requirements" Then	
				
						Dim statusValues As String = ""
						If entityText3 = "L5" Or (entityText3 = "L4" And bHasChildren = False)
							statusValues = level & "_Formulate_PGM," 

						'------ KN 2025/04/16 - Updated to If-Else If statement to accomodate for L3 base FCs which will need to go through the entire process like a normal L3 FC with children
						' Original code:
						'							End If	
						'							If Not entityText3 = "L2" And bHasChildren = True
						ElseIf Not entityText3 = "L2"
							Dim nextLevel As String = "L" & level.Substring(1,1) -1 
							
							statusValues = level & "_Formulate_PGM," & level & "_Validate_PGM," &  level & "_Prioritize_PGM," &  level & "_Approve_PGM," & nextLevel & "_Validate_PGM"
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
		
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region


#Region "GetDemoteStatusList"
Public Function GetDemoteStatusList() As Object
			Try
					'Get WF Profile and Status type
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sEntityGeneral As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")
				Dim REQIDs As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","").Split(" ")(1)
				Dim sOrigEntity As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","").Split(" ")(0)
				'Dim sEntityGeneral As String = args.NameValuePairs.XFGetValue("Entity")
				'Dim REQIDs As String = args.NameValuePairs.XFGetValue("req_IDs")
'brapi.errorlog.LogMessage(si,"entREQCM=" & sEntityGeneral & ": " & REQIDs & ": " & sOrigEntity)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim sEntity As String = ""
				If sEntityGeneral.XFContainsIgnoreCase("General")
					sEntity = sEntityGeneral.Replace("_General","")
				Else 
					sEntity = sEntityGeneral
				End If 
				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube) 
				Dim bHasChildren As Boolean = Nothing
				'Brapi.ErrorLog.LogMessage(si, "Entity CMJM = " & sEntity)
				
				If  Not String.IsNullOrWhiteSpace(sEntity) Then
					bHasChildren = brapi.Finance.Members.HasChildren(si,DimPk,brapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id,sEntity))
				End If
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0).split(" ")(0)
'Brapi.ErrorLog.LogMessage(si, "sProfileName= " & sProfileName)
						'--------- Get Entity Text3 --------- 							
				Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sOrigEntity).Member
				Dim CurrEntityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member
				Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
		
				Dim OrigLevel As String = String.Empty
				Dim CurrLevel As String = String.Empty
				Dim OrigentityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
				Dim CurrentityText3 As String = BRApi.Finance.Entity.Text(si, CurrEntityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)
'brapi.ErrorLog.LogMessage(Si,"CurrentityText3=" & CurrentityText3 & sEntity)					
				If Not String.IsNullOrWhiteSpace(OrigentityText3) AndAlso OrigentityText3.StartsWith("EntityLevel") Then
					OrigLevel = OrigentityText3.Substring(OrigentityText3.Length -2, 2)
				End If
				If Not String.IsNullOrWhiteSpace(CurrentityText3) AndAlso CurrentityText3.StartsWith("EntityLevel") Then
					CurrLevel = CurrentityText3.Substring(CurrentityText3.Length -2, 2)
				End If
				
				'--------- get next workflow level --------- 
				Dim currentStatus As String = OrigLevel.Substring(1,1)
				Dim icurrentStatus As Integer = currentStatus
				icurrentStatus = icurrentStatus - 1
				Dim currentWFLevel As String = icurrentStatus
				Dim newWFLevel As String = "L" & currentWFLevel
				
				'---Get WF Status for demotion----

				Dim statusValues As String = "" 'level & "_Formulate_PGM," & level & "_Validate_PGM," &  level & "_Prioritize_PGM," &  level & "_Approve_PGM," &  level & "_Final_PGM"
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "GetValidDemoteStatus")
				Dim statusItems As List(Of String)
				'List of Statuses
#Region "Status List"
				Select Case sProfileName
					Case "Validate"
						'current level L2
						If CurrLevel = "L2" And OrigLevel = "L3" Then 
							statusValues = "L3_Approve_PGM,L3_Prioritize_PGM,L3_Validate_PGM,L3_Formulate_PGM"
						Else If CurrLevel = "L2" And OrigLevel = "L4" Then 
							statusValues = "L3_Approve_PGM,L3_Prioritize_PGM,L3_Validate_PGM,L3_Formulate_PGM,L4_Formulate_PGM"
						Else If CurrLevel = "L2" And OrigLevel = "L5" Then 
							statusValues = "L3_Approve_PGM,L3_Validate_PGM,L3_Formulate_PGM,L4_Approve_PGM,L4_Prioritize_PGM,L5_Formulate_PGM"
						'current level L3	
						Else If CurrLevel = "L3" And OrigLevel = "L3" Then 
							statusValues = "L3_Formulate_PGM"
						Else If CurrLevel = "L3" And OrigLevel = "L4" Then 
							statusValues = "L4_Formulate_PGM"	
						Else If CurrLevel = "L3" And OrigLevel = "L5" Then 
							statusValues = "L4_Approve_PGM,L4_Prioritize_PGM,L4_Validate_PGM,L5_Formulate_PGM"	
						'current level L4	
						Else If CurrLevel = "L4" And OrigLevel = "L4" Then 
							statusValues = "L4_Formulate_PGM"
						Else If CurrLevel = "L4" And OrigLevel = "L5" Then 
							statusValues = "L5_Formulate_PGM"
						End If 
						
					Case "Approve"
						'current level L2
						If CurrLevel = "L2" And OrigLevel = "L3" Then 
							statusValues = "L2_Prioritize_PGM,L2_Validate_PGM,L3_Approve_PGM,L3_Prioritize_PGM,L3_Validate_PGM,L3_Formulate_PGM"
						Else If CurrLevel = "L2" And OrigLevel = "L4" Then 
							statusValues = "L2_Prioritize_PGM,L2_Validate_PGM,L3_Approve_PGM,L3_Prioritize_PGM,L3_Validate_PGM,L4_Formulate_PGM"
						Else If CurrLevel = "L2" And OrigLevel = "L5" Then 
							statusValues = "L2_Prioritize_PGM,L2_Validate_PGM,L3_Approve_PGM,L3_Validate_PGM,L4_Approve_PGM,L4_Prioritize_PGM,L4_Validate_PGM,L5_Formulate_PGM"
						'current level L3	
						Else If CurrLevel = "L3" And OrigLevel = "L3" Then 
							statusValues = "L3_Prioritize_PGM,L3_Validate_PGM,L3_Formulate_PGM"
						Else If CurrLevel = "L3" And OrigLevel = "L4" Then 
							statusValues = "L3_Prioritize_PGM,L3_Validate_PGM,L4_Formulate_PGM"	
						Else If CurrLevel = "L3" And OrigLevel = "L5" Then 
							statusValues = "L3_Prioritize_PGM,L3_Validate_PGM,L4_Approve_PGM,L4_Prioritize_PGM,L4_Validate_PGM,L5_Formulate_PGM"	
						'current level L4	
						Else If CurrLevel = "L4" And OrigLevel = "L4" Then 
							statusValues = "L4_Formulate_PGM"
						Else If CurrLevel = "L4" And OrigLevel = "L5" Then 
							statusValues = "L4_Prioritize_PGM,L4_Validate_PGM,L5_Formulate_PGM"
						End If 
				End Select
#End Region
				statusItems = StringHelper.SplitString(statusValues,",")

				'Create and fill it from the list of status Values
				For Each statusItem As String In statusItems
					Me.WriteNameValuePairRow(si, dt, statusItem.Trim, statusItem.Trim)
				Next
		
				Return dt
		
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region


#Region "Full Related REQ List"
'Updated by Fronz 09/06/2024 - changed the S# to REQ_Shared and T# to 1999
'Updated: EH 9/18/2024 - RMW-1732 Reverting REQ_Shared changes
Public Function FullREQList() As DataTable
			Try	

							Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
					        Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
					        Dim sREQTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
							'Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
						Dim sFundCenter As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","Entity","")						
							Dim dtall As DataTable = Me.CreateNameValuePairTable(si, "FullREQList")
							Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
								'Checking and adding _General for upper levels'
								Dim pk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"E_" & sCube)

								Dim memberid As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sFundCenter)					
								Dim parent As Boolean = BRApi.Finance.Members.HasChildren(si, pk, memberid)	
								'Brapi.ErrorLog.LogMessage(si, "parent" & parent.ToString())	
								If parent Then sFundCenter = sFundCenter & "_General"
								
								Dim SQL As String
								SQL = $"Select Req.Title,
						  	   Req.Entity,
							   Req.REQ_ID
						FROM XFC_CMD_PGM_REQ AS Req
						WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
						AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
						AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
						AND Req.Entity = '{sFundCenter}'
						Order By Req.Entity,Req.REQ_ID"


			Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FullREQList")
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using	
									
							'Brapi.ErrorLog.LogMessage(si, "SQL" & SQL)		
									
									
									
								For Each dataRow As DataRow In dt.Rows	
									Dim FundCenter As String = dataRow.Item("Entity")
									Dim ExistingREQ As String =  dataRow.Item("REQ_ID")
									Dim TitleValue As String = ""
								If dbnull.Value.Equals(dataRow.Item("Title")) Then
									TitleValue = "!!! REPLACE WITH REQUIREMENT TITLE !!!"
								Else
									TitleValue  =  dataRow.Item("Title")
								End If 

		                         
										Me.WriteNameValuePairRow(si, dtall, FundCenter & " - " &  ExistingREQ & " - " & TitleValue, FundCenter & " " & ExistingREQ)
			

									Next
							
							Return dtall 	
							
						
					Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		
					End Try
	End Function


#End Region   'Updated

	End Class
End Namespace


