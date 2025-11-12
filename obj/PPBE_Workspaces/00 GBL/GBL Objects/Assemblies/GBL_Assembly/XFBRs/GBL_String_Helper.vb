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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.GBL_String_Helper
	Public Class MainClass
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		
'---------------------------------------------------------------
'Description: 
'             
'---------------------------------------------------------------
		
#Region "Main"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args
				
				Select Case args.FunctionName.ToLower()
					Case "getcascadingmbrfilter"
						Return Me.GetCascadingMbrFilter()
					Case "getcprobefdxfilteredrows"
						Return Me.GetcPROBEFDXFilteredRows()
					Case "getcprobefdxappnrows"
						Return Me.GetcPROBEFDXAPPNRows()
					Case "getpomyears"
						Return Me.GetPOMYears()
				End Select
				If args.FunctionName.XFEqualsIgnoreCase("Remove_General") Then
#Region "Remove_General"
					Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity").ToLower
				
					Return sEntity.Replace("_general","")
#End Region
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetPrimaryFundCenter") Then
					Return Me.GetPrimaryFundCenter(si, args)
#Region "GetPrimaryFundCenter (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'					'Example: XFBR(GEN_General_String_Helper, GetPrimaryFundCenter, Cube=|WFCube|)
'					Dim sCube As String = args.NameValuePairs.XFGetValue("Cube").ToUpper
'					Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0)				
''BRApi.ErrorLog.LogMessage(si,"GetPrimaryFundCenter:   Cube=" & sCube & "   ACOM=" & sACOM)
'					Return sACOM
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetDefaultBUOScenario") Then
					Return Me.GetDefaultBUOScenario(si)
#Region "GetDefaultBUOScenario (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'					Dim scenarioPk = BRApi.Finance.Dim.GetDimPk(si,"S_MAIN")
'					Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
'					Dim twoDigitYr As String  =sWFYear.Substring(2)
					
'					Dim scenarioList As List (Of Member)  = BRApi.Finance.Members.GetAllMembers(si,scenarioPk,True)

'					scenarioList.Sort(Function(x, y)
'									     Return y.Name.CompareTo(x.Name)
'									  End Function)	
				
'					For Each mem As Member In scenarioList
''BRApi.ErrorLog.LogMessage(si,"Two digit year: " & twoDigitYr & ", Name: "  & mem.Name)	
'						If mem.Name.Contains("_BUO") And mem.Name.Contains(twoDigitYr) Then
''BRApi.ErrorLog.LogMessage(si,"ZZTwo digit year: " & twoDigitYr & ", Name: "  & mem.Name)						
'							Return mem.Name
'						End If
'					Next
						
'					Return Nothing
					
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetSignedOnUserFundCenters") Then
					Return Me.GetSignedOnUserFundCenters(si)
#Region "GetSignedOnUserFundCenters (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'					'RH 8/11/2023 RMW-714 Function to Retrieve Fund Centers of signed on user
'					'Purpose: Looks up the signed on user's security user name to get it's Text2 string, and extracts values within brackets of FundCenters=[   ]. The values within the brackets is a comma delimited list of Fund Centers.
'					'Returns: Comma delimited list of Fund Centers assigned to the signed on user within the "Manage Security (ADMTLS_MNGSEC)" dashboard
'					'Example: BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
'					'Usage:
'						'cbx_UFRPRO_AAAAAA_0Cb_UserFundCenters__Shared
'							'prompt_cbx_UFRPRO_AAAAAA_0Cb_UserFundCenters__Shared  
'								'Parameter Type = Delimited List 
'								'Display Item = BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
'								'Value Item = BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
'					Dim sFundCenters As String = "Contact Support. Missing Fund Centers in User Text2."
'					Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
'					Dim sUserName As String = objSignedOnUser.User.Name
'					Dim sUserDesc As String = objSignedOnUser.User.Description
'					Dim sText2 As String = objSignedOnUser.User.Text2
'					If ((Not String.IsNullOrWhiteSpace(sText2)) And sText2.XFContainsIgnoreCase("[") ) Then 
'						'Get string between brackets "[   ]". Assumes there are no other brackets in Text2 except for FundCenters=[   ]
'						Dim i As Integer = sText2.IndexOf("[")
'						sFundCenters = sText2.Substring(i + 1, sText2.IndexOf("]", i + 1) - i - 1)
''BRApi.ErrorLog.LogMessage(si, "sText2 " & sFundCenters )
''BRApi.ErrorLog.LogMessage(si, "FunctionName: GetSignedOnUserFundCenters, sUserName : " & sUserName & ", sUserDesc : " & sUserDesc & ", sText2 : " & sText2 & ", sFundCenters : " & sFundCenters )
'					End If
'					Return sFundCenters
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetTwoDigitYr") Then
					Return Me.GetTwoDigitYr(si)
#Region "GetTwoDigitYr (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"		

'					Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
'					Return sWFYear.Substring(2)
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetYearFromScenario") Then
					Return Me.GetYearFromScenario(si, args)
#Region "GetYearFromScenario (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"		

'					Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")
'					Dim sScenarioMbfInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id,sScenario)
'					'Dim sYear As String = sScenarioMbfInfo.GetScenarioProperties.WorkflowTime
'					Dim sYear As String = sScenario.Trim.Substring(sScenario.Length-4)
''BRapi.ErrorLog.LogMessage(si,"sScenario=" & sScenario & "   sYear=" & sYear ) 					
'					Return sYear
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetScenarioTimePairsFromScenario") Then
					Return Me.GetScenarioTimePairsFromScenario(si, args)
#Region "GetScenarioTimePairsFromScenario (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"		
	
'					Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")	
'					Dim sScenarioType As String = args.NameValuePairs.XFGetValue("SrcScenarioType")
'					Dim mode As String = args.NameValuePairs.XFGetValue("mode")
'				    Dim sResult As String = ""
'					Dim yearPrefix As String = "FY"		
'					Dim DefaultScenario As String = Me.GetFirstMemberFromDimName(si,"S_Main","S#"& sScenarioType & ".Base")
'					Dim DefaultTime As String = Me.GetYearFromScenarioName(si,DefaultScenario)
'					sScenario = sScenario.Replace(" ","")
'				If sScenario = ""	
'					Select Case mode
'						Case "T"
'							Return DefaultTime
'						Case "S"
'							Return DefaultScenario
'						Case "S#T#"
'							Return $"S#{DefaultScenario}:T#{DefaultTime}"
'					End Select
'				Else
'					Select Case mode	
'						Case "T"
'							Dim sSubstrings As String() = sScenario.Split(",")						
'							For Each sSubstring As String In sSubstrings
'								Dim Time As String = Me.GetYearFromScenarioName(si,sSubstring)
'								sResult = $"{sResult}{Time},"
'							Next			
'						Case "S"
'							Dim sSubstrings As String() = sScenario.Split(",")				
'							For Each sSubstring As String In sSubstrings
'								sResult = $"{sResult}{sSubstring},"
'							Next					
'						Case "S#T#"				
'				   			Dim sSubstrings As String() = sScenario.Split(",")
'							For Each sSubstring As String In sSubstrings
'								Dim Time As String = Me.GetYearFromScenarioName(si,sSubstring)
'								sResult = $"{sResult}S#{sSubstring}:T#{Time},"
'							Next
'					End Select
'					Return sResult.Substring(0,sResult.Length-1)
'				End If
#End Region
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetYearRangeFromScenario") Then
#Region "GetYearRangeFromScenario"		

					Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")
					Dim sMode As String = args.NameValuePairs.XFGetValue("Mode", "")
				
					Return Me.GetYearRangeFromScenario(si, sScenario, sMode)
					
				End If
				
#End Region

				If args.FunctionName.XFEqualsIgnoreCase("GetFilterHelper") Then
					Return Me.GetFilterHelper(si, args)
#Region "GetFilterHelper (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"		
		
'					Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")	
'					Dim sScenarioType As String = args.NameValuePairs.XFGetValue("SrcScenarioType")
'					Dim sCV As String = args.NameValuePairs.XFGetValue("CV")
'					If sCV = "DST_SPI_VersionsCompare_Main" Or sCV = "DST_SPI_VersionsCompare_Main"
'						Select Case sScenarioType
'							Case "Initial_Distribution"
'								Return "Total_Appr_Distr_Budget"
'							Case "Spend_Plan"
'								Return "Total_Phased_Obligation_wAdj,Total_Phased_Commitment_wAdj"						
'						End Select
'					End If
#End Region
				End If


				If args.FunctionName.XFEqualsIgnoreCase("GetControlBudgetSourceScenario") Then
					Return Me.GetControlBudgetSourceScenario(si)
#Region "GetControlBudgetSourceScenario (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'                    'Example: S#XFBR(GEN_General_String_Helper, GetControlBudgetSourceScenario)
						
'					'========== workflow vars ============================================================================================================ 
'					'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'					Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'					'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							

'					Dim sMbrScript As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & wfCube & "_General" & ":C#Local:V#Annotation"
'					sMbrScript &= ":A#ControlBudgetSourcePosition:I#None:F#None:O#Forms:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

'					Return BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMbrScript).DataCellEx.DataCellAnnotation

#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetEntityGeneralfromParam") Then
					Return Me.GetEntityGeneralfromParam(si, args)
#Region "GetEntityGeneralfromParam (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'					'Example: Passing param SourceEntity=A57, return A57_General. Passing SourceEntity=A57_FC, return A57_FC
					
'					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
'					Dim sEntity As String = args.NameValuePairs.XFGetValue("SourceEntity")

'					If sEntity.trim = "" Then sEntity="None"
		
'					Dim sEntityNew As String = sEntity
'					Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
					
'					Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
'					Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

'					If bValue Then
'						sEntityNew = sEntity & "_General"
'					End If
					
''BRapi.ErrorLog.LogMessage(si,"ME: GetEntityGeneralfromParam : " & sEntity & " : " & sEntityNew)								
'					Return "E#" & sEntityNew	
#End Region		
				End If

				If args.FunctionName.XFEqualsIgnoreCase("GetEntityGeneralfromCube") Then
					Return Me.GetEntityGeneralfromCube(si, args)
#Region "GetEntityGeneralfromCube (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
					
'					Dim wfCube = args.NameValuePairs.XFGetValue("Entity","Army")
					
'					Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0)
'					Dim sEntityNew As String = sEntity
'					Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
''BRApi.ErrorLog.LogMessage(si, "sEntity= " & sEntity &" ||  sEntityNew= " & sEntityNew)
'					Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
'					Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

'					If bValue Then
'						sEntityNew = sEntity & "_General"
'					End If
							
'					Return sEntityNew
					
#End Region
				End If
	
				If args.FunctionName.XFEqualsIgnoreCase("GetCopiedCEACVersion") Then
					Return Me.GetCopiedCEACVersion(si)
#Region "GetCopiedCEACVersion (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'                    'Example: S#XFBR(GEN_General_String_Helper, GetCopiedCEACVersion)
						
'					'========== workflow vars ============================================================================================================ 
'					'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'					Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'					Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'					'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							

'					Dim sMbrScript As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & wfCube & "_General" & ":C#Local:V#Annotation"
'					sMbrScript &= ":A#CurrentCEACRateVersion:I#None:F#None:O#Forms:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'					Return BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMbrScript).DataCellEx.DataCellAnnotation

#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetFirstMemberfromScenarioList") Then
					Return Me.GetFirstMemberfromScenarioList(si, args)
#Region "GetFirstMemberfromScenarioList (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'                    'Example: S#XFBR(GEN_General_String_Helper, GetFirstMemberfromScenarioList)
						
'					'========== workflow vars ============================================================================================================ 
'					'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'					'Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'					'Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					'Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'					'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							
					
'					Dim scenarioPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
'					Dim scenarioMember As String = args.NameValuePairs.XFGetValue("Scenario","")
'					Dim MbrFilter As String = "S#" & scenarioMember & ".base"
'					Dim scenarioList As List (Of MemberInfo)  = BRApi.Finance.Members.GetMembersUsingFilter(si,scenarioPk,MbrFilter,True)
					
''BRAPI.ErrorLog.LogMessage(si, "Scenario=" & scenarioList(0).Member.Name)					
'					Return scenarioList(0).Member.Name

#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetEntityNameDesc") Then
					Return Me.GetEntityNameDesc(si, args)
#Region "GetEntityNameDesc (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'                    'Example: :NAME(XFBR(GEN_General_String_Helper, GetEntityNameDesc))
						
'					'========== workflow vars ============================================================================================================ 
'					'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'					'Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
'					'Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'					'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
'					'Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
'					'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
'					'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							
''BRAPI.ErrorLog.LogMessage(si,"Here1")
'					Dim sMbr As String = args.NameValuePairs.XFGetValue("Member")
''BRAPI.ErrorLog.LogMessage(si,"Here2")
'					Dim iDimTypeId As Integer = brapi.Finance.Dim.GetDimPk(si,"E_ARMY").DimTypeId
''BRAPI.ErrorLog.LogMessage(si,"Here3")
'					Dim sMbr_ID As String = BRApi.Finance.Members.GetMemberId(si, iDimTypeId, sMbr )  
					
''BRAPI.ErrorLog.LogMessage(si,"Here4")
'					Dim sEntity_Mbr_Name As String = BRApi.Finance.Members.GetMember(si, iDimTypeId, sMbr).Name
''BRAPI.ErrorLog.LogMessage(si,"Here5")
'					Dim sEntity_Mbr_Desc As String = BRApi.Finance.Members.GetMember(si, iDimTypeId, sMbr).Description
'					Return sEntity_Mbr_Desc

#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetFirstValueFromParam") Then
					Return Me.GetFirstValueFromParam(si, args)
#Region "GetFirstValueFromParam (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'        Dim sParam As String = args.NameValuePairs.XFGetValue("Param")
'		Dim sMbrFilter As String = args.NameValuePairs.XFGetValue("MbrFilter","Default")
'		Dim sDimName As String = args.NameValuePairs.XFGetValue("DimName","Default")
'		Dim sMode As String = args.NameValuePairs.XFGetValue("mode","Default")
'		Dim sMbr As String = ""
'		If sParam = "" Then
'			If sMbrFilter.XFContainsIgnoreCase("E#") Then
'				If sMbrFilter.Contains(".") Then sMbrFilter &= ".Where(UserInAnyDataSecurityGroup = True)"
'				If Not sMbrFilter.Contains(".") Then sMbrFilter &= ".member.Where(UserInAnyDataSecurityGroup = True)"	
'			End If	
'			sMbr = Me.GetFirstMemberFromDimName(si,sDimName,sMbrFilter)
'		Else
'			sMbr = sParam
'		End If
		
'		Select Case sMode
'		Case "Entity"
'			Return sMbr
'		Case "EntityGeneral"
'			Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sMbr), 1, 0, 0)
'			Dim sEntityNew As String = sMbr
'			Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & sMbr)
''BRApi.ErrorLog.LogMessage(si, "sMbr= " & sMbr & " sEntity= " & sEntity &" ||  sEntityNew= " & sEntityNew)
'			Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
'			Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

'			If bValue Then
'				sEntityNew = sEntity & "_General"
'			End If
''BRapi.ErrorLog.LogMessage(si,$"sEntityNew = {sEntityNew}")
'			Return sEntityNew
'		End Select	
'Return sMbr
#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetYearFromFirstScenario") Then
					Return Me.GetYearFromFirstScenario(si, args)
#Region "GetYearFromFirstScenario (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'                    'Example: :NAME(XFBR(GEN_General_String_Helper, GetYearFromFirstScenario, SrcScenario=[]))	
'					'Return only the workflow year from the first scenario
'		Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")
'		Dim sSubstrings As String() = sScenario.Split(",")
'  		Dim MbrTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si,BRApi.Finance.Members.GetMemberId(si,dimTypeId.Scenario,sSubstrings(0))).ToString
	
'		Return MbrTime.Substring(0,4)
	
#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetYearsConsecutive") Then
					Return Me.GetYearsConsecutive(si, args)
#Region "GetYearsConsecutive (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'		Dim StartYr As String = args.NameValuePairs.XFGetValue("StartYr")
'		Dim NumYr As String = args.NameValuePairs.XFGetValue("NumYr","1")
'		Dim SrcScenario As String = args.NameValuePairs.XFGetValue("SrcScenario","NoneInputted")
	  
'		If SrcScenario <> "NoneInputted"
'		  StartYr = Me.GetYearFromScenarioName(si,SrcScenario)
'		End If
'		Dim iStartYr As Integer = Integer.Parse(StartYr)

'		Dim i As Integer = 1
'		Dim sResult As String = $"T#{iStartYr}"
		
'		While i < Integer.Parse(NumYr)
'			sResult = $"{sResult},T#{iStartYr+i}"
'			i = i+1
'		End While
		
'		Return sResult
#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("ReplaceString") Then
					Return Me.ReplaceString(si, args)
#Region "ReplaceString (RDD 2024.04.24: MOVED CODE, REGION FLAGGED FOR DELETION)"
'					Dim sSrcString As String = args.NameValuePairs.XFGetValue("Text")
'					Dim sRtnString As String = sSrcString
					
'					sRtnString = sRtnString.Replace("\n", vbCRLF)
					
'					Return sRtnString
#End Region
	            End If

				If args.FunctionName.XFEqualsIgnoreCase("GetConsolidationStatus") Then
					Return Me.GetConsolidationStatus(si, args)
	            End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetShowHideSubCon") Then
					Return Me.GetShowHideSubCon(si, args)
	            End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetEntityTextProperty") Then
					Return Me.GetEntityTextProperty(si, args)
	            End If
				
				If args.FunctionName.XFEqualsIgnoreCase("ShowHideComponent") Then
					Return Me.ShowHideComponent(si, args)
	            End If		
				
				If args.FunctionName.XFEqualsIgnoreCase("GetL2MbrUsingText1") Then
					Return Me.GetL2MbrUsingText1(si, args)
	            End If
				
#Region "Get Mass Upload Status"
			
			If args.FunctionName.XFEqualsIgnoreCase("GetImportStatus") Then
   				Return BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, "UploadStatus", "UploadStatus","")
			End If
			
#End Region
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "GetCascadingMbrFilter"
		Public Function GetCascadingMbrFilter() As String
			Try
				#Region "Args"
				' Read incoming args
				Dim scenList As String = args.NameValuePairs.XFGetValue("scenList", "NA")
				Dim cmd As String = args.NameValuePairs.XFGetValue("cmd", "NA")
				Dim entity As String = args.NameValuePairs.XFGetValue("entity", "NA")
				Dim appn As String = args.NameValuePairs.XFGetValue("appn","OMA")
				If appn = String.Empty
					appn = "OMA"
				End If
				Dim mdep As String = args.NameValuePairs.XFGetValue("mdep", "NA")
				If mdep = String.Empty
					mdep = "NA"
				End If
				Dim sag As String = args.NameValuePairs.XFGetValue("sag", "NA")
				If sag = String.Empty
					sag = "NA"
				End If
				Dim ape As String = args.NameValuePairs.XFGetValue("ape", "NA")
				If ape = String.Empty
					ape = "NA"
				End If
				Dim dollarType As String = args.NameValuePairs.XFGetValue("dollarType", "NA")
				If dollarType = String.Empty
					dollarType = "NA"
				End If
				Dim status As String = args.NameValuePairs.XFGetValue("status", "NA")
				Dim app_module As String = args.NameValuePairs.XFGetValue("app_module", "NA")
				Dim returnType As String = args.NameValuePairs.XFGetValue("returnType", "NA")
				Dim cvName As String = args.NameValuePairs.XFGetValue("cvName", "NA") '$"{app_module}_cPROBE_FDX_CV"
				#End Region
				Dim resultList As New List(Of String)
				' Build a compact signature for Entity + Appn only
				Dim currRebuildparams As String = $"{cmd}|{entity}|{scenList}|{appn}|{status}"
				Dim U3DimPK As DimPK = BRapi.Finance.Dim.GetDimPk(si, "U3_ALL_APE")
				
				' Use workspace session settings to persist last seen signatures per user/workspace
				Dim cacheCat As String = $"{app_module}_CascadingFilterCache"
				Dim filterDTparams As String = $"{app_module}_FilterDTparams"
				Dim rebuildparams As String = $"{app_module}_rebuildparams_APPN"
				If Not returnType.XFEqualsIgnoreCase("APPN")
					rebuildparams = $"{app_module}_rebuildparams_Other"
				End If

				Dim prevRebuildParams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, "")
				Dim needsRebuild As Boolean = Not String.Equals(prevRebuildParams, currRebuildparams, StringComparison.Ordinal)

				Dim dt As New DataTable 
			'If String.IsNullOrEmpty(prevRebuildParams)
				If needsRebuild Then
					Brapi.ErrorLog.LogMessage(si,"In needs rebuild")
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, currRebuildparams)
					If cvName.XFContainsIgnoreCase("cPROBE") And Not app_module.XFContainsIgnoreCase("cPROBE")
						dt = GetFDXcPROBESrcData(cvName,cmd,appn)
					Else If cvName.XFContainsIgnoreCase("CMD_TGT")
						dt = GetFDXCMD_TGTDISTData(cvName,cmd,appn)
					Else If cvName.XFContainsIgnoreCase("cPROBE") And app_module.XFContainsIgnoreCase("cPROBE")
						dt = GetFDXcPROBEData(cvName,cmd,appn,scenList)
					End If
					If returnType.XFEqualsIgnoreCase("APPN")
						BRApi.Utilities.SetSessionDataTable(si, si.UserName, $"{app_module}_CascadingFilter_APPN",dt)
					Else
						BRApi.Utilities.SetSessionDataTable(si, si.UserName, $"{app_module}_CascadingFilter",dt)
					End If
				Else
					If returnType.XFEqualsIgnoreCase("APPN")
						dt = BRApi.Utilities.GetSessionDataTable(si, si.UserName, $"{app_module}_CascadingFilter_APPN")
					Else
						dt = BRApi.Utilities.GetSessionDataTable(si, si.UserName, $"{app_module}_CascadingFilter")
					End If
				End If
				If Not dt Is Nothing Then

					' Build a deterministic signature of the inputs (full)
					Dim currFilterDTparams As String = String.Concat(scenList, "|", entity, "|", appn, "|", status, "|", returnType, "|", mdep, "|", sag, "|", ape, "|", dollarType)
	
					Dim returnTypeMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
							{"APPN", "UD1"},
							{"MDEP", "UD2"},
							{"SAG",  "UD3"},
							{"APE",  "UD3"},
							{"DollarType",  "UD4"}
						}
					'Dim prevFilterDTparams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, filterDTparams, "")
					If returnType = "APPN"
						Dim dv As DataView = New DataView(dt)
						' Map returnType values to column keys (case-insensitive)

						' Determine which physical column (if any) corresponds to the requested returnType
						Dim selectedColumn = String.Empty
						selectedColumn = returnTypeMap.XFGetValue(returnType,"Not Found")
						If selectedColumn <> "Not Found"
		
							dv.RowFilter = $"{selectedColumn} IS NOT NULL AND {selectedColumn} <> ''"
							dv.Sort = selectedColumn & " ASC"
							Dim mlDT = dv.ToTable(True, selectedColumn) ' Distinct values only

							For Each dr As DataRow In mlDT.Rows
								Dim val As String = dr(selectedColumn).ToString()
								If Not String.IsNullOrWhiteSpace(val) Then
									If Not resultList.Contains($"{selectedColumn}#{val}")
										resultList.Add($"{selectedColumn}#{val}")
									End If
								End If
							Next
							Return String.Join(",",resultList)
						Else
							Return String.Empty
						End If
						
					Else
	
						Dim filterParts As New List(Of String)
		
						' Only add filters when parameter is provided and not "NA"
						If Not String.IsNullOrWhiteSpace(appn) AndAlso Not appn.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
							filterParts.Add("[UD1] = '" & appn.Replace("'", "''") & "'")
						End If
						
						If Not String.IsNullOrWhiteSpace(mdep) AndAlso Not mdep.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
							filterParts.Add("[UD2] = '" & mdep.Replace("'", "''") & "'")
						End If
						
						If Not String.IsNullOrWhiteSpace(sag) AndAlso Not sag.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
							If String.IsNullOrWhiteSpace(ape) OrElse ape.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
								Dim apeFilters As New List(Of String)
								Dim ape9List As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3DimPK, $"U3#{sag}.Base", True)
								If Not ape9List Is Nothing
									If ape9List.Count > 0
										For Each ape9 As MemberInfo In ape9List
											apeFilters.Add($"'{ape9.Member.Name}'")
										Next
										filterParts.Add($"[UD3] IN ({String.Join(",",apeFilters)})")
									End If	
								End If
							End If
						End If
						
						If Not String.IsNullOrWhiteSpace(ape) AndAlso Not ape.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
							filterParts.Add("[UD3] = '" & ape.Replace("'", "''") & "'")
						End If
						
						If Not String.IsNullOrWhiteSpace(dollarType) AndAlso Not dollarType.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
							filterParts.Add("[UD4] = '" & dollarType.Replace("'", "''") & "'")
						End If
		
		
						Dim filterExpr As String = If(filterParts.Count > 0, String.Join(" AND ", filterParts), String.Empty)

						' Filter dt into a DataTable so it can be converted to a DataView
						Dim filteredDt As DataTable
						If String.IsNullOrEmpty(filterExpr) Then
							filteredDt = dt.Copy()
						Else
							filteredDt = dt.Clone()
							Dim selectedRows() As DataRow = dt.Select(filterExpr)
							For Each row As DataRow In selectedRows
								filteredDt.ImportRow(row)
							Next
						End If
						'BRApi.Utilities.SetSessionDataTable(si, si.UserName, $"{app_module}_CascadingFilter",filteredDt)
		
						Dim dv As DataView = New DataView(filteredDt)
						' Map returnType values to column keys (case-insensitive)

						' Determine which physical column (if any) corresponds to the requested returnType
						Dim selectedColumn = String.Empty
						selectedColumn = returnTypeMap.XFGetValue(returnType,"Not Found")
						If selectedColumn <> "Not Found"
		
							dv.RowFilter = $"{selectedColumn} IS NOT NULL AND {selectedColumn} <> ''"
							dv.Sort = selectedColumn & " ASC"
							Dim mlDT = dv.ToTable(True, selectedColumn) ' Distinct values only
			
							Dim result As String = String.Empty
							For Each dr As DataRow In mlDT.Rows
								Dim val As String = dr(selectedColumn).ToString()
								If Not String.IsNullOrWhiteSpace(val) Then
									If returnType.XFEqualsIgnoreCase("SAG")
										Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U3DimPK, $"U3#{val}.Ancestors.Where(Text1 = SAG)", True,,)
   		   								If Not lsAncestorList Is Nothing
											If lsAncestorList.Count > 0
												val = lsAncestorList(0).Member.Name
											End If	
										End If
									End If
									If Not resultList.Contains($"{selectedColumn}#{val}")
										resultList.Add($"{selectedColumn}#{val}")
									End If
								End If
							Next
							Return String.Join(",",resultList)
						Else
							Return String.Empty
						End If
					End If
				Else
'BRapi.ErrorLog.LogMessage(si,"Hit Empty")
					Return String.Empty
				End If					

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region 'Updated 10/16/2025

#Region "Helper Functions"
	
		Private Function GetFDXcPROBESrcData(ByVal cvName As String,ByVal entFilter As String,ByVal appn As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "00 GBL"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim CprobeScen As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetSrccPROBEScen(si,sScenario)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{CprobeScen}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			If cvName.XFEqualsIgnoreCase("GBL_cPROBE_FDX_CV")
				NameValuePairs.Add("ML_GBL_APPN",appn)
			End If
			Brapi.ErrorLog.LogMessage(si,"Scenario" & scenFilter)
			Brapi.ErrorLog.LogMessage(si,"timeFilter" & timeFilter)
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 8, False)
If dt Is Nothing
			BRAPI.ErrorLog.LogMessage(si,$"Hit FDX {EntFilter} - {cvName}")
		End If
			Return dt
		End Function
		
		Private Function GetFDXcPROBEData(ByVal cvName As String,ByVal entFilter As String,ByVal appn As String,ByVal scenFilter As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "00 GBL"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)

			Dim entDim = $"E_{entFilter}"
			Dim scenDim = "S_RMW"
			Dim timeFilter = String.Empty '"T#2026" '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			If cvName.XFEqualsIgnoreCase("GBL_cPROBE_FDX_by_Position_CV")
				NameValuePairs.Add("ML_GBL_APPN",appn)
			End If
			NameValuePairs.Add("ML_GBL_Scenario",scenFilter)
			NameValuePairs.Add("ML_GBL_AllYears","2026")
			BRAPI.ErrorLog.LogMessage(si,$"Hit Scen: {scenFilter}")
			
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}",String.Empty,String.Empty, timeFilter, nvbParams, False, True, True, String.Empty, 8, False)
If dt Is Nothing
			BRAPI.ErrorLog.LogMessage(si,$"Hit FDX {EntFilter} - {cvName}")
		End If
			Return dt
		End Function
		
		Private Function GetFDXCMD_TGTDISTData(ByVal cvName As String,ByVal entFilter As String,ByVal appn As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "00 GBL"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim CprobeScen As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetSrccPROBEScen(si,sScenario)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{CprobeScen}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			If cvName.XFEqualsIgnoreCase("GBL_cPROBE_FDX_CV")
				NameValuePairs.Add("ML_GBL_APPN",appn)
			End If
			
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 1, False)

			Return dt
		End Function
#End Region

#Region "GetcPROBEFDXFilteredRows"
Public Function GetcPROBEFDXFilteredRows() As String
			Dim cmd As String = args.NameValuePairs.XFGetValue("cmd", "NA")
			Dim scen As String = args.NameValuePairs.XFGetValue("scen", "NA")
			Dim time As String = args.NameValuePairs.XFGetValue("time", "NA")
			Dim appn As String = args.NameValuePairs.XFGetValue("appn",String.Empty)
			Dim commDims As String
			Dim FilterString = String.Empty
			Dim toSort As New Dictionary(Of String,String)
			Dim output = vbNullString
			Dim scenParts As New List(Of String)	

			' Handle single or multiple scenarios in scen (comma-delimited)
			scenParts = stringhelper.SplitString(scen,",")

			If scenParts.Count > 1
				' Build one commDims segment per scenario then join with " + "
				Dim segments As New List(Of String)
				For Each sc As String In scenParts
					If appn = String.Empty
						segments.Add($"Cb#{cmd}:C#Aggregated:S#{sc}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U5#Top:U6#Top:U7#Top:U8#Top")
					Else
						segments.Add($"Cb#{cmd}:C#Aggregated:S#{sc}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U1#[{appn}]:U5#Top:U6#Top:U7#Top:U8#Top")
					End If
				Next
				commDims = String.Join(" + ", segments)

				' Use same FilterString rules as single-scenario branch (only set once)
				If appn = String.Empty
					FilterString = $",[U1#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U2#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U3#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U4#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
				Else
					FilterString = $",[U2#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U3#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U4#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
				End If

			Else
				' Single scenario - preserve original behavior
				If appn = String.Empty
					commDims = $"Cb#{cmd}:C#Aggregated:S#{scen}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U5#Top:U6#Top:U7#Top:U8#Top"
					FilterString = $",[U1#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U2#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U3#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U4#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
				Else
					commDims = $"Cb#{cmd}:C#Aggregated:S#{scen}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U1#[{appn}]:U5#Top:U6#Top:U7#Top:U8#Top"
					FilterString = $",[U2#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U3#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)],
									 [U4#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
				End If
			End If
			
			Dim bufferFilter = $"FilterMembers(REMOVENODATA({commDims}){FilterString})"
			

			globals.SetStringValue("Filter", bufferFilter)
			
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

				' Normalize each MemberScriptBuilder and project to (script, sortKey)
				Dim projected = results.Keys.Select(Function(msb)
													  msb.Entity = vbNullString
													  msb.Scenario = vbNullString
													  msb.Origin = vbNullString
													  msb.Account = vbNullString
													  msb.IC = vbNullString
													  msb.Flow = vbNullString
													  msb.UD1 = If(appn = String.Empty, msb.UD1,appn)
													  msb.UD5 = vbNullString
													  msb.UD6 = vbNullString
													  msb.UD7 = vbNullString
													  msb.UD8 = vbNullString
													  Return New With {
														  .Script = msb.GetMemberScript,
														  .SortKey = $"U1#{msb.UD1}:U3#{msb.UD3}:U2#{msb.UD2}:U4#{msb.UD4}"
													  }
												  End Function).ToList()

				' Order and build output
				For Each item In projected.OrderByDescending(Function(x) x.SortKey)
					output &= item.Script & ","
				Next
			End If
			'BRAPI.ErrorLog.LogMessage(si,$"Output: {output}")
			Return output
			
		End Function
#End Region

#Region "GetcPROBEFDXAPPNRows"
Public Function GetcPROBEFDXAPPNRows() As String
			Dim cmd As String = args.NameValuePairs.XFGetValue("cmd", "NA")
			Dim scen As String = args.NameValuePairs.XFGetValue("scen", "NA")
			Dim time As String = args.NameValuePairs.XFGetValue("time", "NA")
			Dim commDims As String
			Dim FilterString = String.Empty
			Dim toSort As New Dictionary(Of String,String)
			Dim output = vbNullString
			Dim scenParts As New List(Of String)	

			' Handle single or multiple scenarios in scen (comma-delimited)
			scenParts = stringhelper.SplitString(scen,",")
			If scenParts.Count > 1
				' Build one commDims segment per scenario then join with " + "
				Dim segments As New List(Of String)
				For Each sc As String In scenParts
					segments.Add($"Cb#{cmd}:C#Aggregated:S#{sc}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top")
				Next
				commDims = String.Join(" + ", segments)
			Else
				commDims = $"Cb#{cmd}:C#Aggregated:S#{scen}:T#{Time}:E#[{cmd}]:A#BO1:V#Periodic:O#Top:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
			End If
			FilterString = $",[U1#Top.Base.Options(Cube={cmd},ScenarioType=ScenarioType1,MergeMembersFromReferencedCubes=False)]"
			
			Dim bufferFilter = $"FilterMembers(REMOVENODATA({commDims}){FilterString})"
			BRAPI.ErrorLog.LogMessage(si,bufferFilter)
			globals.SetStringValue("Filter", bufferFilter)
			
			GetDataBuffer(si,globals,api,args)
	
			If Not globals.GetObject("Results") Is Nothing
				Dim results As Dictionary(Of MemberScriptBuilder, DataBufferCell) = globals.GetObject("Results")

				' Normalize each MemberScriptBuilder and project to (script, sortKey)
				Dim projected = results.Keys.Select(Function(msb)
													  msb.Entity = vbNullString
													  msb.Scenario = vbNullString
													  msb.Origin = vbNullString
													  msb.Account = vbNullString
													  msb.IC = vbNullString
													  msb.Flow = vbNullString
													  msb.UD2 = vbNullString
													  msb.UD3 = vbNullString
													  msb.UD4 = vbNullString
													  msb.UD5 = vbNullString
													  msb.UD6 = vbNullString
													  msb.UD7 = vbNullString
													  msb.UD8 = vbNullString
													  Return New With {
														  .Script = msb.GetMemberScript,
														  .SortKey = $"U1#{msb.UD1}"
													  }
												  End Function).ToList()

				' Order and build output
				For Each item In projected.OrderByDescending(Function(x) x.SortKey)
					output &= item.Script & ","
				Next
			End If
			BRAPI.ErrorLog.LogMessage(si,$"Output: {output}")
			Return output
			
		End Function
#End Region
#Region "GetPrimaryFundCenter"

		Private Function GetPrimaryFundCenter(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Example: XFBR(GEN_General_String_Helper, GetPrimaryFundCenter, Cube=|WFCube|)
				Dim sCube As String = args.NameValuePairs.XFGetValue("Cube").ToUpper
				Dim sACOM As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sCube), 1, 0, 0)				
'BRApi.ErrorLog.LogMessage(si,"GetPrimaryFundCenter:   Cube=" & sCube & "   ACOM=" & sACOM)
				Return sACOM
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "GetDefaultBUOScenario"

		'Referenced By: Global_SolutionHelper.HQAREV_ContentLoad
		Private Function GetDefaultBUOScenario(ByVal si As SessionInfo) As String
			Try
				Dim scenarioPk = BRApi.Finance.Dim.GetDimPk(si,"S_MAIN")
				Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
				Dim twoDigitYr As String  =sWFYear.Substring(2)
				
				Dim scenarioList As List (Of Member)  = BRApi.Finance.Members.GetAllMembers(si,scenarioPk,True)

				scenarioList.Sort(Function(x, y)
								     Return y.Name.CompareTo(x.Name)
								  End Function)	
			
				For Each mem As Member In scenarioList
'BRApi.ErrorLog.LogMessage(si,"Two digit year: " & twoDigitYr & ", Name: "  & mem.Name)	
					If mem.Name.Contains("_BUO") And mem.Name.Contains(twoDigitYr) Then
'BRApi.ErrorLog.LogMessage(si,"ZZTwo digit year: " & twoDigitYr & ", Name: "  & mem.Name)						
						Return mem.Name
					End If
				Next
					
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetSignedOnUserFundCenters"

		Private Function GetSignedOnUserFundCenters(ByVal si As SessionInfo) As String
			Try
				'RH 8/11/2023 RMW-714 Function to Retrieve Fund Centers of signed on user
				'Purpose: Looks up the signed on user's security user name to get it's Text2 string, and extracts values within brackets of FundCenters=[   ]. The values within the brackets is a comma delimited list of Fund Centers.
				'Returns: Comma delimited list of Fund Centers assigned to the signed on user within the "Manage Security (ADMTLS_MNGSEC)" dashboard
				'Example: BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
				'Usage:
					'cbx_UFRPRO_AAAAAA_0Cb_UserFundCenters__Shared
						'prompt_cbx_UFRPRO_AAAAAA_0Cb_UserFundCenters__Shared  
							'Parameter Type = Delimited List 
							'Display Item = BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
							'Value Item = BRString(GEN_General_String_Helper, GetSignedOnUserFundCenters)
				Dim sFundCenters As String = "Contact Support. Missing Fund Centers in User Text2."
				Dim objSignedOnUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
				Dim sUserName As String = objSignedOnUser.User.Name
				Dim sUserDesc As String = objSignedOnUser.User.Description
				Dim sText2 As String = objSignedOnUser.User.Text2
				If ((Not String.IsNullOrWhiteSpace(sText2)) And sText2.XFContainsIgnoreCase("[") ) Then 
					'Get string between brackets "[   ]". Assumes there are no other brackets in Text2 except for FundCenters=[   ]
					Dim i As Integer = sText2.IndexOf("[")
					sFundCenters = sText2.Substring(i + 1, sText2.IndexOf("]", i + 1) - i - 1)
'BRApi.ErrorLog.LogMessage(si, "sText2 " & sFundCenters )
'BRApi.ErrorLog.LogMessage(si, "FunctionName: GetSignedOnUserFundCenters, sUserName : " & sUserName & ", sUserDesc : " & sUserDesc & ", sText2 : " & sText2 & ", sFundCenters : " & sFundCenters )
				End If
				Return sFundCenters

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetTwoDigitYr"

		Private Function GetTwoDigitYr(ByVal si As SessionInfo) As String
			Try
				Dim sWFYear As String = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
				Return sWFYear.Substring(2)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetYearFromScenario"

		Private Function GetYearFromScenario(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")
				Dim sScenarioMbfInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id,sScenario)
				'Dim sYear As String = sScenarioMbfInfo.GetScenarioProperties.WorkflowTime
				Dim sYear As String = sScenario.Trim.Substring(sScenario.Length-4)
'BRapi.ErrorLog.LogMessage(si,"sScenario=" & sScenario & "   sYear=" & sYear ) 					
				Return sYear

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetScenarioTimePairsFromScenario"

		Private Function GetScenarioTimePairsFromScenario(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try	
				Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")	
				Dim sScenarioType As String = args.NameValuePairs.XFGetValue("SrcScenarioType")
				Dim mode As String = args.NameValuePairs.XFGetValue("mode")
			    Dim sResult As String = ""
				Dim yearPrefix As String = "FY"		
				Dim DefaultScenario As String = Me.GetFirstMemberFromDimName(si,"S_Main","S#"& sScenarioType & ".Base")
				Dim DefaultTime As String = Me.GetYearFromScenarioName(si,DefaultScenario)
				sScenario = sScenario.Replace(" ","")
			If sScenario = ""	
				Select Case mode
					Case "T"
						Return DefaultTime
					Case "S"
						Return DefaultScenario
					Case "S#T#"
						Return $"S#{DefaultScenario}:T#{DefaultTime}"
				End Select
			Else
				Select Case mode	
					Case "T"
						Dim sSubstrings As String() = sScenario.Split(",")						
						For Each sSubstring As String In sSubstrings
							Dim Time As String = Me.GetYearFromScenarioName(si,sSubstring)
							sResult = $"{sResult}{Time},"
						Next			
					Case "S"
						Dim sSubstrings As String() = sScenario.Split(",")				
						For Each sSubstring As String In sSubstrings
							sResult = $"{sResult}{sSubstring},"
						Next					
					Case "S#T#"				
			   			Dim sSubstrings As String() = sScenario.Split(",")
						For Each sSubstring As String In sSubstrings
							Dim Time As String = Me.GetYearFromScenarioName(si,sSubstring)
							sResult = $"{sResult}S#{sSubstring}:T#{Time},"
						Next
				End Select
				Return sResult.Substring(0,sResult.Length-1)
			End If
			
			Return Nothing

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetFilterHelper"

		Private Function GetFilterHelper(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")	
				Dim sScenarioType As String = args.NameValuePairs.XFGetValue("SrcScenarioType")
				Dim sCV As String = args.NameValuePairs.XFGetValue("CV")
				If sCV = "DST_SPI_VersionsCompare_Main" Or sCV = "DST_SPI_VersionsCompare_Main"
					Select Case sScenarioType
						Case "Initial_Distribution"
							Return "Total_Appr_Distr_Budget"
						Case "Spend_Plan"
							Return "Total_Phased_Obligation_wAdj,Total_Phased_Commitment_wAdj"						
					End Select
				End If
				
				Return Nothing

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetBUOSourceScenario"

		Private Function GetBUOSourceScenario(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
                'Example: S#XFBR(GEN_General_String_Helper, GetBUOSourceScenario, BUOScenario=|WFScenario|)
                Dim sBUOScenario As String = args.NameValuePairs.XFGetValue("BUOScenario")            
                If Not sBUOScenario.Contains("_BUO") Then Return Nothing
                Dim sBUOSourceScenario As String = sBUOScenario.Split("_")(0).Trim
'BRapi.ErrorLog.LogMessage(si,"GetBUOSourceScenario: sBUOScenario=" & sBUOScenario & "  sBUOSourceScenario=" & sBUOSourceScenario)
                Return sBUOSourceScenario
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetControlBudgetSourceScenario"

		Private Function GetControlBudgetSourceScenario(ByVal si As SessionInfo) As String
			Try
                'Example: S#XFBR(GEN_General_String_Helper, GetControlBudgetSourceScenario)
					
				'========== workflow vars ============================================================================================================ 
				'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey) & "M12"
			
				'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							

				Dim sMbrScript As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & wfCube & "_General" & ":C#Local:V#Annotation"
				sMbrScript &= ":A#ControlBudgetSourcePosition:I#None:F#None:O#Forms:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"

				Return BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMbrScript).DataCellEx.DataCellAnnotation

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetEntityGeneralfromParam"

		Private Function GetEntityGeneralfromParam(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Example: Passing param SourceEntity=A57, return A57_General. Passing SourceEntity=A57_FC, return A57_FC
				
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sEntity As String = args.NameValuePairs.XFGetValue("SourceEntity")

				If sEntity.trim = "" Then sEntity="None"
	
				Dim sEntityNew As String = sEntity
				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
				
				Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
				Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

				If bValue Then
					sEntityNew = sEntity & "_General"
				End If
				
'BRapi.ErrorLog.LogMessage(si,"ME: GetEntityGeneralfromParam : " & sEntity & " : " & sEntityNew)								
				Return "E#" & sEntityNew	

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetEntityGeneralfromCube"

		Private Function GetEntityGeneralfromCube(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim wfCube = args.NameValuePairs.XFGetValue("Entity","Army")
				
				Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, wfCube), 1, 0, 0)
				Dim sEntityNew As String = sEntity
				Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
'BRApi.ErrorLog.LogMessage(si, "sEntity= " & sEntity &" ||  sEntityNew= " & sEntityNew)
				Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
				Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

				If bValue Then
					sEntityNew = sEntity & "_General"
				End If
						
				Return sEntityNew
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetCopiedCEACVersion"

		Private Function GetCopiedCEACVersion(ByVal si As SessionInfo) As String
			Try
                'Example: S#XFBR(GEN_General_String_Helper, GetCopiedCEACVersion)
					
				'========== workflow vars ============================================================================================================ 
				'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							

				Dim sMbrScript As String = "Cb#" & wfCube & ":S#" & wfScenarioName & ":T#" & wfTimeName & ":E#" & wfCube & "_General" & ":C#Local:V#Annotation"
				sMbrScript &= ":A#CurrentCEACRateVersion:I#None:F#None:O#Forms:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Return BRApi.Finance.Data.GetDataCellUsingMemberScript(si, wfCube, sMbrScript).DataCellEx.DataCellAnnotation

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetFirstMemberfromScenarioList"

		Private Function GetFirstMemberfromScenarioList(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
                'Example: S#XFBR(GEN_General_String_Helper, GetFirstMemberfromScenarioList)
					
				'========== workflow vars ============================================================================================================ 
				'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				'Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				'Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							
				
				Dim scenarioPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "S_Main")
				Dim scenarioMember As String = args.NameValuePairs.XFGetValue("Scenario","")
				Dim MbrFilter As String = "S#" & scenarioMember & ".base"
				Dim scenarioList As List (Of MemberInfo)  = BRApi.Finance.Members.GetMembersUsingFilter(si,scenarioPk,MbrFilter,True)
				
				If scenarioList.Count < 1 Then 
					Return Nothing
				End If
				
'BRAPI.ErrorLog.LogMessage(si, "Scenario=" & scenarioList(0).Member.Name)					
				Return scenarioList(0).Member.Name

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetEntityNameDesc"

		Private Function GetEntityNameDesc(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
                'Example: :NAME(XFBR(GEN_General_String_Helper, GetEntityNameDesc))
					
				'========== workflow vars ============================================================================================================ 
				'Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
				'Dim wfCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
				'Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
				'Dim wfScenarioTypeID As Integer = BRApi.Finance.Scenario.GetScenarioType(si, si.WorkflowClusterPk.ScenarioKey).Id
				'Dim wfTimeName As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
				'Dim iWFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)	
				'Dim iWFMonth As Integer = TimeDimHelper.GetMonthIdFromId(si.WorkflowClusterPk.TimeKey)							
'BRAPI.ErrorLog.LogMessage(si,"Here1")
				Dim sMbr As String = args.NameValuePairs.XFGetValue("Member")
'BRAPI.ErrorLog.LogMessage(si,"Here2")
				Dim iDimTypeId As Integer = brapi.Finance.Dim.GetDimPk(si,"E_ARMY").DimTypeId
'BRAPI.ErrorLog.LogMessage(si,"Here3")
				Dim sMbr_ID As String = BRApi.Finance.Members.GetMemberId(si, iDimTypeId, sMbr )  
				
'BRAPI.ErrorLog.LogMessage(si,"Here4")
				Dim sEntity_Mbr_Name As String = BRApi.Finance.Members.GetMember(si, iDimTypeId, sMbr).Name
'BRAPI.ErrorLog.LogMessage(si,"Here5")
				Dim sEntity_Mbr_Desc As String = BRApi.Finance.Members.GetMember(si, iDimTypeId, sMbr).Description
				Return sEntity_Mbr_Desc

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetFirstValueFromParam"

		Private Function GetFirstValueFromParam(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
		        Dim sParam As String = args.NameValuePairs.XFGetValue("Param")
				Dim sMbrFilter As String = args.NameValuePairs.XFGetValue("MbrFilter","Default")
				Dim sDimName As String = args.NameValuePairs.XFGetValue("DimName","Default")
				Dim sMode As String = args.NameValuePairs.XFGetValue("mode","Default")
				Dim sMbr As String = ""
				If sParam = "" Then
					If sMbrFilter.XFContainsIgnoreCase("E#") Then
						If sMbrFilter.Contains(".") Then sMbrFilter &= ".Where(UserInAnyDataSecurityGroup = True)"
						If Not sMbrFilter.Contains(".") Then sMbrFilter &= ".member.Where(UserInAnyDataSecurityGroup = True)"	
					End If	
					sMbr = Me.GetFirstMemberFromDimName(si,sDimName,sMbrFilter)
				Else
					sMbr = sParam
				End If
				
				Select Case sMode
				Case "Entity"
					Return sMbr
				Case "EntityGeneral"
					Dim sEntity As String = BRapi.Finance.Entity.Text(si, BRapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, sMbr), 1, 0, 0)
					Dim sEntityNew As String = sMbr
					Dim DimPK As DimPk = brapi.Finance.Dim.GetDimPk(si, "E_" & sMbr)
'BRApi.ErrorLog.LogMessage(si, "sMbr= " & sMbr & " sEntity= " & sEntity &" ||  sEntityNew= " & sEntityNew)
					Dim nEntityId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Entity.Id, sEntity)
					Dim bValue As Boolean = BRApi.Finance.Members.HasChildren(si, DimPK, nEntityId)

					If bValue Then
						sEntityNew = sEntity & "_General"
					End If
'BRapi.ErrorLog.LogMessage(si,$"sEntityNew = {sEntityNew}")
					Return sEntityNew
				End Select	
				Return sMbr

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetYearFromFirstScenario"

		Private Function GetYearFromFirstScenario(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Example: :NAME(XFBR(GEN_General_String_Helper, GetYearFromFirstScenario, SrcScenario=[]))	
				'Return only the workflow year from the first scenario
				Dim sScenario As String = args.NameValuePairs.XFGetValue("SrcScenario")
				Dim sSubstrings As String() = sScenario.Split(",")
		  		Dim MbrTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si,BRApi.Finance.Members.GetMemberId(si,dimTypeId.Scenario,sSubstrings(0))).ToString
			
				Select Case True
					Case sSubstrings(0).StartsWith("PB")
						Return "20" & MbrTime.Substring(2,2)
					Case Else
						Return MbrTime.Substring(0,4)					
				End Select 
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetYearsConsecutive"

		Private Function GetYearsConsecutive(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim StartYr As String = args.NameValuePairs.XFGetValue("StartYr")
				Dim NumYr As String = args.NameValuePairs.XFGetValue("NumYr","1")
				Dim SrcScenario As String = args.NameValuePairs.XFGetValue("SrcScenario","NoneInputted")
			  
				If SrcScenario <> "NoneInputted"
				  StartYr = Me.GetYearFromScenarioName(si,SrcScenario)
				End If
				Dim iStartYr As Integer = Integer.Parse(StartYr)

				Dim i As Integer = 1
				Dim sResult As String = $"T#{iStartYr}"
				
				While i < Integer.Parse(NumYr)
					sResult = $"{sResult},T#{iStartYr+i}"
					i = i+1
				End While
				
				Return sResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function GetPOMYears() As String
			Try
				Dim srcScenario As String = args.NameValuePairs.XFGetValue("SrcScenario","NA")
				Dim srcScenarioMbfInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id,srcScenario)
				Dim StartYr_Key As Integer = BRApi.Finance.Scenario.GetWorkflowTime(si, srcScenarioMbfInfo.Member.MemberId)
				Dim StartYr As String = BRApi.Finance.Time.GetNameFromId(si,StartYr_Key)
			  
				Dim iStartYr As Integer = Integer.Parse(StartYr)

				Dim i As Integer = 1
				Dim sResult As String = $"T#{iStartYr}"
				
				While i < Integer.Parse(5)
					sResult = $"{sResult},T#{iStartYr+i}"
					i = i+1
				End While
				
				Return sResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "ReplaceString"

		Private Function ReplaceString(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sSrcString As String = args.NameValuePairs.XFGetValue("Text")
				Dim sRtnString As String = sSrcString
				
				sRtnString = sRtnString.Replace("\n", vbCRLF)
				
				Return sRtnString

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetConsolidationStatus"

		Private Function GetConsolidationStatus(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "None").Replace("_General","")
				Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","None")
				Dim sPeriod As String = args.NameValuePairs.XFGetValue("Period","1999")
				
				
				Dim sMbrScript As String = "Cb#ARMY:E#" & sEntity & ":C#Local:S#" & sScenario & ":T#" & sPeriod & ":V#CalcStatus:A#None:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				
				Dim sResult As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "ARMY", sMbrScript).DataCellEx.DataCell.CellAmountAsText
				
'BRAPI.ErrorLog.LogMessage(si, "GEN_General_String_Helper.GetConsolidationStatus: sScenario=" & sScenario & "   sPeriod=" & sPeriod & "   sEntity=" & sEntity & "   sResult=" & sResult)				
				Return sResult

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetShowHideSubCon"

		Private Function GetShowHideSubCon(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				
Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity", "None")
Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","None")
Dim sPeriod As String = args.NameValuePairs.XFGetValue("Period","1999")
'BRAPI.ErrorLog.LogMessage(si, "GEN_General_String_Helper.GetShowHideSubCon: sScenario=" & sScenario & "   sPeriod=" & sPeriod & "   sEntity=" & sEntity)				
				
				
				Dim sSubConStatus As String = Me.GetConsolidationStatus(si, args)			  
'BRAPI.ErrorLog.LogMessage(si, "GEN_General_String_Helper.GetShowHideSubCon: sSubConStatus=" & sSubConStatus)				


				Select Case sSubConStatus.ToLower
				Case "1","0"
'BRAPI.ErrorLog.LogMessage(si, "GEN_General_String_Helper.GetShowHideSubCon: FALSE")				
					Return "False"
				Case Else
'BRAPI.ErrorLog.LogMessage(si, "GEN_General_String_Helper.GetShowHideSubCon: TRUE")				
					Return "True"
				End Select 		

				
				'0 - OK,NA no data
				'1 - OK
				'2 - ok,MC metadata changed
				'3 - CA - need ot calc
				'4 - TR - need to translate
				'5 - CN - nedd to consolidate
				'6 - CA,CN - need to calc and consolidate
				'7 - CA,TR - need to calc and translate
				'8 - TR,CN - need to translate and calculate
				'9 - CA,TR,CN - need to calc, translate, and consolidate
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "GetYearRangeFromScenario"	
		'Created 6/13/2024 by KN
		'Return range of time in years for scenario member
		Private Function GetYearRangeFromScenario(ByVal si As SessionInfo, sScenario As String, sMode As String) As String
			Try				
					Dim sScenarioMbfInfo As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id,sScenario)
					Dim iStartYear As String = Convert.ToInt32(BRApi.Finance.Scenario.GetWorkflowStartTime(si,sScenarioMbfInfo.Member.MemberId).ToString.Trim.Substring(0,4))
					Dim iEndYear As String = Convert.ToInt32(BRApi.Finance.Scenario.GetWorkflowEndTime(si,sScenarioMbfInfo.Member.MemberId).ToString.Trim.Substring(0,4))
					Dim sYears As String = ""
'BRapi.ErrorLog.LogMessage(si, $"iStartYear = {iStartYear} || iEndYear = {iEndYear}") 
					If sMode.XFContainsIgnoreCase("MbrFilter") Then
						For yr As Integer = iStartYear To iEndYear
							sYears = sYears & ",T#" & yr
						Next
					Else
						For yr As Integer = iStartYear To iEndYear
							sYears = sYears & "," & yr
						Next
					End If
					sYears = sYears.Substring(1,sYears.Length-1)
					Return sYears	
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "GetEntityTextProperty"	
		'Created 10/28/2024 by KN
		'Return Text Property of a given Entity member
		Public Function GetEntityTextProperty(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try				
					Dim sMbr As String = args.NameValuePairs.XFGetValue("Mbr")
					If String.IsNullOrWhiteSpace(sMbr) Then Return Nothing
					Dim sTextNum As String = args.NameValuePairs.XFGetValue("TextNum")
					Dim memberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sMbr)
					Dim sValue As String = BRApi.Finance.Entity.Text(si, memberId, Convert.ToInt32(sTextNum), -1, -1)
'BRapi.ErrorLog.LogMessage(si, $"sValue = {sValue}") 
					Return sValue
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "GetL2MbrUsingText1"	
		'Created 5/27/2025 by KN
		'Return Text1 Property of root Entity member which contains the L2 member
		Private Function GetL2MbrUsingText1(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try				
					Dim sText1 As String = Me.GetEntityTextProperty(si,args)
					If String.IsNullOrWhiteSpace(sText1) Then Return Nothing
					Dim sMode As String = args.NameValuePairs.XFGetValue("Mode")					
					Dim sValue As String = "E#" & sText1
					If sMode.XFContainsIgnoreCase("AppendGeneral") Then
'BRapi.ErrorLog.LogMessage(si, $"sValue = {sValue}" & "_General") 
						Return sValue & "_General"
					Else
						Return sValue
					End If
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "ShowHideComponent"
		Private Function ShowHideComponent(ByVal si As SessionInfo, ByVal args As DashboardStringFunctionArgs) As String
			Try
				Dim sTempType As String = args.NameValuePairs.XFGetValue("tempType").ToUpper
				Dim sVisible As String = "False"
				
				Select Case sTempType 
				Case "CDIGITAL","CSUSTAIN NO LINS"
					sVisible = "True" 
					Return sVisible
				End Select 
				
				Return sVisible
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

'BELOW NOT REFERENCED IN MAIN

'#Region "Constants"
'	Private BR_GeneralMemberList As New OneStream.BusinessRule.Finance.General_Member_Lists.MainClass		
'#End Region

#Region "GetFirstMemberFromDimName"		
	Public Function GetFirstMemberFromDimName(ByVal si As SessionInfo, DimName As String, MbrFilter As String) As Object		
				Dim dimPk = BRApi.Finance.Dim.GetDimPk(si, DimName)				
				Dim mbrList As List (Of MemberInfo)  = BRApi.Finance.Members.GetMembersUsingFilter(si,dimPk,MbrFilter,True)	
				Return mbrList(0).Member.Name
	End Function		
#End Region

#Region "GetYearFromScenarioName"
	Public Function GetYearFromScenarioName(ByVal si As SessionInfo, sScenario As String) As Object	
		Dim MbrTime As String = BRApi.Finance.Scenario.GetWorkflowTime(si,BRApi.Finance.Members.GetMemberId(si,dimTypeId.Scenario,sScenario)).ToString
'BRapi.ErrorLog.LogMessage(si,$"Scenario = {sScenario} | MbrTime = {MbrTime}") 
	
			Return MbrTime.Substring(0,4)
	
	End Function
#End Region

#Region "Security Logic: Get User Funds Center By Workflow"
'New logic to pull user groups then pass group and FC into case statment for FC dropdown. Added logic to remove duplicate FC for the Review step and CMD certify step
		Public Function GetUserFundsCenterByWF(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
'Dim st As DateTime = DateTime.Now				
'Brapi.ErrorLog.LogMessage(si,"In Fund Center Rule")
				Dim fcDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
				Dim dt As New DataTable() ' = Me.CreateNameValuePairTable(si, "FundsCenterByWF")
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
				Else
					mode = "AllChildren"
				End If
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
								'Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name & "_General")
							End If
						Case "AllChildren"
							If bHasChildren Then
								'Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name & "_General")
							Else
								'Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name)							
							End If
						Case Else
							If bHasChildren Then
								'Me.WriteNameValuePairRow(si, dt, fc.NameAndDescription, fc.Member.Name & "_General")
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

			Return dt.defaultView.ToTable(True, sColumnlist.ToArray())
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region
#Region "Utilities: Get DataBuffer"
	
	Public Sub GetDataBuffer(ByRef si As SessionInfo, ByRef globals As BRGlobals, ByRef api As Object,ByRef args As DashboardStringFunctionArgs)
		Dim filter = globals.GetStringValue("Filter","NA")
			
		Dim Dictionary As New Dictionary(Of String, String)
		
		BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule(si,"Global_Buffers","GetCVDataBuffer",Dictionary,customcalculatetimetype.CurrentPeriod)

	End Sub

#End Region

	End Class
End Namespace
