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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.GBL_DB_DataSet
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
#Region "Security Logic: Get User FundsCenter By Workflow"						
						If args.DataSetName.XFEqualsIgnoreCase("GetUserFundsCenterByWF") Then
'BRApi.ErrorLog.LogMessage(si, "HERE")
							Return Me.GetUserFundsCenterByWF(si,globals,api,args)		
						End If						
#End Region	
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Security Logic: Get User Funds Center By Workflow"
'New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundsCenterByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
'Dim st As DateTime = DateTime.Now				
'Brapi.ErrorLog.LogMessage(si,"In Fund Center Rulexx")
				Dim fcDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
				Dim dt As DataTable = Me.CreateNameValuePairTable(si, "FundsCenterByWF")
				Dim wfProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
				Dim accessGroupInfo As GroupInfoEx = BRApi.Security.Admin.GetGroupInfoEx(si,wfProfile.AccessGroupUniqueID)
				Dim accessGroupInfo_Split As List(Of String) = StringHelper.SplitString(accessGroupInfo.GroupInfo.Group.Name, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
				Dim wfProfileAccessGroup As String = accessGroupInfo.GroupInfo.Group.Name
'				Dim WFManager As String  = wfProfileAccessGroup.Replace(right(wfProfileAccessGroup,2),"MG")
				Dim wfProfileStep As String = accessGroupInfo_Split(accessGroupInfo_Split.Count-1)
'brapi.ErrorLog.LogMessage(si, "WF Step" & wfProfileStep)		
'09/22/2025
'If Everyone
'Then look at wf cube
'Substring user security permission i.e., A97AA from g_PGM_AFC_FC_AP_A97AA
'Return A97AA in dropdown
'If Admin, return all FCs
'brapi.ErrorLog.LogMessage(si, "Here 1")
				Dim mode As String = String.Empty'args.NameValuePairs.XFGetValue("mode","")
				If wfProfileStep = "WF"
					wfProfileAccessGroup = StringHelper.ReplaceString(wfProfileAccessGroup,"_WF",String.Empty,True)
				End If						
				'Get WF profile cube
				Dim cmd As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				'Get WF profile name
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name	
				
				Dim x As Integer = InStr(wfProfileName, ".")
				Dim sProfileName As String = wfProfileName.Substring(x + 0)
				'Brapi.ErrorLog.LogMessage(si, "WFProfile" & sProfileName)
				If sProfileName.XFContainsIgnoreCase("CMD Requirements") 'Or sProfileName.XFContainsIgnoreCase("CMD Dist") Then
					mode = "ParentOnly"
				ElseIf wfProfileName.XFContainsIgnoreCase("CMD TGT")
					mode = "TGT"
				Else
					mode = "AllChildren"
				End If
'brapi.ErrorLog.LogMessage(si, $"{sProfileName}")				
'Brapi.ErrorLog.LogMessage(si,"WFCube" & cmd)
'Brapi.ErrorLog.LogMessage(si,"WFProfileName" & wfProfileName)
'Brapi.ErrorLog.LogMessage(si,"Mode" & mode)

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
'If param = demote
'Return sFCgroups
'End If				
				Dim objDimDisplayOptions As New DimDisplayOptions()
				'filter fundcenters in the command only
				For Each Grp In sGroupList

					Dim Grp_Split As List(Of String) = StringHelper.SplitString(Grp, "_", StageConstants.ParserDefaults.DefaultQuoteCharacter)
					Dim fc_name As String = Grp_Split(Grp_Split.Count-1)

					Dim MbrExpansion As String = String.Empty
					
					
					Dim fcList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", $"E#{fc_name}{MbrExpansion}", True)
'BRApi.ErrorLog.LogMessage(si, $"fc_name = {fc_name}")					
					For Each fc In fcList
'BRApi.ErrorLog.LogMessage(si, $"fc = {fc.Member.Name}")	
						Dim bHasChildren = BRApi.Finance.Entity.HasChildren(si,fcDimPk,fc.Member.MemberId,objDimDisplayOptions)
						Select Case mode
						Case "ParentOnly"
							If bHasChildren Then
								Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
							End If
						Case "AllChildren"
							If bHasChildren Then
								Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
							Else
								Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)							
							End If
						Case "TGT"
							Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
						Case Else
							If bHasChildren Then
								Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)
							End If
						End Select
					Next			
				Next	
'For Each row As DataRow In dt.Rows
'	BRApi.ErrorLog.LogMessage(si, $"Name = {row("Name")} || Value = {row("Value")}")
'Next	
				Dim sColumnlist As New List(Of String)

				For Each sColumn In dt.Columns
					sColumnlist.Add(sColumn.ColumnName)
				Next
'brapi.ErrorLog.LogMessage(si, $"HERE186")
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