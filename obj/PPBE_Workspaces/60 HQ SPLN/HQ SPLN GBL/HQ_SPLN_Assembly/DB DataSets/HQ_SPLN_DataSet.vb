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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.HQ_SPLN_DataSet
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					Case Is = DashboardDataSetFunctionType.GetDataSet
						'If args.DataSetName.XFEqualsIgnoreCase("MyDataSet") Then
						'End If			
				End Select
				
#Region "Security Logic: Get Appropiation from User Security"						
						If args.DataSetName.XFEqualsIgnoreCase("GetUserAPPN") Then
							Return Me.GetUserAPPN(si,globals,api,args)	
						End If						
#End Region	

#Region "Get ACOM Who Submitted APPN SPL"
						If args.DataSetName.XFEqualsIgnoreCase("GetCommands") Then
							Return Me.GetCommands(si,globals,api,args)	
						End If	
#End Region

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Get ACOM Who Submitted APPN SPL"
		Public Function GetCommands(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try	
				
				Dim sAPPN As String = args.NameValuePairs.XFGetValue("APPN")
'brapi.ErrorLog.LogMessage(si,"appn=" & sAPPN)
				Dim lAPPN As List(Of String) = StringHelper.SplitString(sAPPN,"_")
				sAPPN = lAPPN(5).Replace("Review","")
'brapi.ErrorLog.LogMessage(si,"appn=" & sAPPN)
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "ValidCommands")
				'Get all L2 '_General' members
				Dim lEntityMembers As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#ARMY.Base.Where(Text3 Contains L2)", True)
'brapi.ErrorLog.LogMessage(si,"here 1 CM")				
				'Loop through entity member list and validate the value in account 'SPL_Validation_Ind' to verify whether a command SPL has been submitted or not
				For Each entity As MemberInfo In lEntityMembers
					Dim sEntityName As String = entity.Member.Name
'brapi.ErrorLog.LogMessage(si,"here 2 sEntityName = " & sEntityName)							
					Dim sDataBufferValidationIND As String =  $"Cb#ARMY:S#{wfScenarioName}:T#{wfTimeName}M12:C#Local:V#Annotation:E#{sEntityName}:A#SPL_Validation_Ind:I#None:F#None:O#Forms:U1#{sAPPN}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"				
					Dim sValidationINDValue As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "ARMY", sDataBufferValidationIND).DataCellEx.DataCellAnnotation
		
					'If a command SPL is submitted, return that command name to be populated in the cbx dropdown
					If sValidationINDValue.XFContainsIgnoreCase("yes") Then		
'brapi.ErrorLog.LogMessage(si,"here CM before substring")
						Dim lTopCMDEntity As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", $"E#ARMY.Descendants.Where(Text1 Contains {sEntityName.Substring(0,3).Trim})", True)
						For Each topCMDEntity As MemberInfo In lTopCMDEntity		
							'i.e., Name: AFC | Value: A97_General
							Me.WriteNameValuePairRow(si, dt, topCMDEntity.Member.Name, sEntityName)
						Next	
					End If
				
				Next
				Return dt
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Security Logic: Get Appropiation from User Security"
'Updated 06/18/2025 - Fronz
'Updated 5/29/2025 MF. New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserAPPN(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try		
'Brapi.ErrorLog.LogMessage(si,"In Fund Center Rule")
'				Dim fcDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "UserAppropiations")
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim sCube As String = wfProfile.CubeName
'				Dim sCube As String = args.NameValuePairs.XFGetValue("Cube").ToUpper
				Dim sCMDEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0)	
'brapi.ErrorLog.LogMessage(si,$"sCMDEntity: {sCMDEntity}")
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name
				Dim wfProfileAccessGroup2 As String = "CMD_RV_" & sCMDEntity
'brapi.ErrorLog.LogMessage(si, wfProfileAccessGroup)				'returns g_SPL_AFC_CMD_WF
				
				Dim WFManager As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"MG")

'For Each xxxx In accessGroupInfo_Split
'	brapi.ErrorLog.LogMessage(si, xxxx)
'Next
				
				Dim wfProfileStep As String = accessGroupInfo_Split(accessGroupInfo_Split.Count-1)
'brapi.ErrorLog.LogMessage(si, wfProfileStep) ' returns WF or last two letters of security group

				If wfProfileStep = "WF"
					wfProfileAccessGroup = StringHelper.ReplaceString(wfProfileAccessGroup,"CMD_WF",String.Empty,True) 'returns g_SPL_AFC_
				End If		
'brapi.ErrorLog.LogMessage(si, $"wfProfileAccessGroup {wfProfileAccessGroup}")		 'returns g_SPL_AFC_

'				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name		
				'Get command
'				Dim cmd As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName 
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
							Where AccessGroup Like '{wfProfileAccessGroup}%' AND AccessGroup LIKE '%{wfProfileAccessGroup2}'"
				Else
					SQL = $"
						SELECT SG.Name as AccessGroup
						FROM SecGroup as SG
						WHERE SG.Name like '{wfProfileAccessGroup}%' AND SG.Name LIKE '%{wfProfileAccessGroup2}'"
				End If
'brapi.ErrorLog.LogMessage(si, sql.ToString)
				Dim dtAll As New DataTable
'				Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "GroupSecList")
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
'brapi.ErrorLog.LogMessage(si,"GRP: " & Grp)
					Dim Grp_Split As List(Of String) = StringHelper.SplitString(Grp, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
					Dim fc_name As String = Grp_Split(Grp_Split.Count-4)				
					Me.WriteNameValuePairRow(si, dt, fc_name, fc_name)

'					Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, fc_name).Member
'					Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					Dim wfTimeId As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id,wfTime)
'					Dim entityText3 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 3, wfScenarioTypeID, wfTimeId)

'					If (Not entityText3.Contains("=L")) Then
'						Throw New Exception("Fund Center Dropdown:" & Environment.NewLine & "Member " & entityMem.Name & " is missing its text 3 property." & Environment.NewLine & "Contact an Administrator for help.")
'					End If
'					Dim MbrExpansion As String = String.Empty
''					If Not entityText3.Contains("=L2") Then
''						MbrExpansion = ".DescendantsInclusive"
''					End If						
					
'					Dim fcList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", $"E#{fc_name}{MbrExpansion}", True)
					
'					For Each fc In fcList						
'						If BRApi.Finance.Entity.HasChildren(si,fcDimPk,fc.Member.MemberId,objDimDisplayOptions) Then
'							Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name & "_General")
'						Else
'							Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
'						End If
'					Next			
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
		
		
		
	End Class
End Namespace
