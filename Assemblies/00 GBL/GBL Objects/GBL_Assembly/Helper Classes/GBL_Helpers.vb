Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports Microsoft.Data.SqlClient
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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class GBL_Helpers
		
        Public Shared Function GetSrccPROBEScen(ByVal si As SessionInfo,ByVal Scenario As String) As String
            Try
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim sCube = "Army"
' Updated 11/3/25  - Updated to allow CMD_SPLN to look at Montlhy Scenario on cprobe admin dashboard 
				Dim memberScript As String = ""
				If Scenario.XFContainsIgnoreCase("CMD_SPLN") Then
					memberScript = $"Cb#ARMY:E#ARMY:C#Local:S#{Scenario}:T#{wfInfoDetails("TimeName")}M1:V#Annotation:A#Var_Selected_Position:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				Else 
					memberScript = $"Cb#ARMY:E#ARMY:C#Local:S#{Scenario}:T#{wfInfoDetails("TimeName")}:V#Annotation:A#Var_Selected_Position:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
				End If 
				Dim sCprobeScen As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, memberScript).DataCellEx.DataCellAnnotation
			
                Return sCprobeScen
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		Public Shared Function GetValidationApproach(si As SessionInfo,ByVal CMD As String,ByVal Year As String) As Dictionary(Of String, String)
            Try
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				Dim cmd_ValidationApproaches As New Dictionary(Of String, String)
				#Region "acctList"
				Dim acctList As New List(Of String)
				acctList.Add("CMD_Val_APPN_Approach")
				acctList.Add("CMD_Val_APE_Approach")
				acctList.Add("CMD_Val_MDEP_Approach")
				acctList.Add("CMD_Val_DollarType_Approach")
				acctList.Add("CMD_Val_Pay_NonPay_Approach")
				#End Region
				#Region "CMD_Val_Config"
				Dim cmd_Validations As New Dictionary(Of String, String)
				cmd_Validations.Add("CMD_Val_APPN_Approach|1","APPN")
				cmd_Validations.Add("CMD_Val_APPN_Approach|2","Fund Code")
				cmd_Validations.Add("CMD_Val_APE_Approach|1","ABO Level")
				cmd_Validations.Add("CMD_Val_APE_Approach|2","APE9")
				cmd_Validations.Add("CMD_Val_MDEP_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_MDEP_Approach|0","No")
				cmd_Validations.Add("CMD_Val_DollarType_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_DollarType_Approach|0","No")
				cmd_Validations.Add("CMD_Val_Pay_NonPay_Approach|1","Yes")
				cmd_Validations.Add("CMD_Val_Pay_NonPay_Approach|0","No")
				#End Region
				
				For Each acct As String In acctList
					Dim memberScript As String = $"cb#{wfInfoDetails("CMDName")}:E#{CMD}:C#Local:S#RMW_Cycle_Config_Annual:T#{Year}:V#Periodic:A#{acct}:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
					Dim Val_Approach As Integer = BRApi.Finance.Data.GetDataCellUsingMemberScript(si,wfInfoDetails("CMDName"), memberScript).DataCellEx.DataCell.CellAmount
					If cmd_Validations.XFGetValue($"{acct}|{Val_Approach.XFToString}","NA") <> "NA"
						cmd_ValidationApproaches.Add(acct,cmd_Validations.XFGetValue($"{acct}|{Val_Approach.XFToString()}","NA"))
					End If
				Next
				
                Return cmd_ValidationApproaches
			
			Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
		End Function
		
        Public Shared Function GetWFInfoDetails(si As SessionInfo) As Dictionary(Of String, String)
            Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
 

            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
            Return WFInfoDetails
        End Function
		
		Public Shared Function Is_Step_Allowed(ByRef si As SessionInfo, ByRef args As DashboardExtenderArgs, ByVal wfStep As String, ByVal sFundCenter As String) As Boolean
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName		
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim sTime As String = BRApi.Finance.Time.GetNameFromId(si,si.WorkflowClusterPk.TimeKey)
			Dim sConfigCycle As String = ""
			Dim WF_ProcCtrl_Account As New Dictionary(Of String, String) From {
			{"Formulate","CMD_PGM_Allow_Req_Creation"},
			{"Validate","CMD_PGM_Allow_Req_Validation"},
			{"Prioritize","CMD_PGM_Allow_Req_Priority"},
			{"Approve","CMD_PGM_Allow_Req_Approval"},
			{"Rollover","CMD_PGM_Allow_Req_Rollover"}
			}
'			{"Formulate_SPLN","CMD_SPLN_Allow_Req_Creation"},
'			{"Validation","CMD_SPLN_Allow_Req_Validation"},
'			{"Rollover","CMD_SPLN_Allow_Req_Rollover"}
			
			
			If WF_ProcCtrl_Account.ContainsKey(wfStep) Then
				
				If sScenario.XFContainsIgnoreCase("CMD_PGM") Then 	
					sConfigCycle = "RMW_Cycle_Config_Annual"
				Else If sScenario.XFContainsIgnoreCase("CMD_SPLN") Then 
					sConfigCycle = "RMW_Cycle_Config_Monthly"
				End If
			Dim sAllowAccount As String = WF_ProcCtrl_Account(wfStep)
			Dim sPermissionMbrScript As String = "Cb#" & sCube & ":E#" & sFundCenter & ":C#Local:S#" & sConfigCycle & ":T#" & sTime & ":V#Annotation:A#" & sAllowAccount & ":F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
			Dim sPermission As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sPermissionMbrScript).DataCellEx.DataCellAnnotation	
				If sPermission.XFEqualsIgnoreCase("no") Then
					Return False
				Else
					Return True
				End If
			Else 
				Return Nothing 
			End If
			
			End Function
		
		Public Shared Function Check_WF_Complete_Lock(ByRef si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult						
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = False
			selectionChangedTaskResult.Message = ""
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
			selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
			selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
			Return selectionChangedTaskResult
			'--------------------------------------------------------------- 
			'Verify Workflow is NOT Complete Or Locked
			'---------------------------------------------------------------
			If (BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Completed") And Not BRApi.Workflow.General.GetUserWorkflowInitInfo(si).GetSelectedWorkflowInfo.GetOverallStatusText(False).Contains("Load Completed")) Or BRApi.Workflow.Status.GetWorkflowStatus(si, si.WorkflowClusterPk, True).Locked Then
				selectionChangedTaskResult.ShowMessageBox = True
				selectionChangedTaskResult.Message = "Notice: No updates are allowed. Workflow was marked ""Complete"" by Command HQ."
			End If
			Return selectionChangedTaskResult
		Return Nothing
		End Function			
		
		Public Shared Function GetEntityLevel(ByRef si As SessionInfo, sEntity As String) As String
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
		
		Public Shared Function GetCMD_L2FC(ByRef si As SessionInfo, sEntity As String) As String
			Dim entityMem As Member = BRApi.Finance.Metadata.GetMember(si, DimType.Entity.Id, sEntity).Member

			Dim entityText1 As String = BRApi.Finance.Entity.Text(si, entityMem.MemberId, 1, Dimconstants.Unknown,DimConstants.Unknown)
			If Not String.IsNullOrWhiteSpace(entityText1) AndAlso entityText1.StartsWith("A") Then
				Return entityText1
			Else
				Return "None"
			End If
			
		End Function	
	
		Public Shared Function GetDistinctValues(ByVal sourceTable As DataTable, ByVal columnName As String) As List(Of Object)
		    
		    If Not sourceTable.Columns.Contains(columnName) Then
		        Throw New ArgumentException($"Column '{columnName}' not found in the DataTable.")
		    End If
		
		    ' Use LINQ to select the column values and then filter for distinct results.
		    Dim distinctValues As List(Of Object) = sourceTable.AsEnumerable() _
		        .Select(Function(row) row.Field(Of Object)(columnName)) _
		        .Distinct() _
		        .ToList()
		        
		    Return distinctValues
		    
		End Function
		
		Public Shared Function GetEntityLists(ByRef si As SessionInfo, rawEntList As List(Of String), Optional ByVal sMode As String = "NA") As Tuple(Of List(Of String), List(Of String), List(Of String))
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	
			Dim ParentEntityList As New List(Of String)
			'Dim sMode As String = args.NameValuePairs.XFGetValue("Mode","NA")
			Dim BaseEntityList As New List(Of String)
			Dim targetLevelForEntity As New Dictionary(Of String, Integer)
			Dim ConsolEntityList As New List(Of String)
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
		
			For Each entityName As String In rawEntList
				
				Dim EntityLevel As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, entityName)
				Dim EntityID As Integer = BRApi.Finance.Members.GetMemberId(si, dimType.Entity.ID, entityName)

				Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk, EntityID)
				If Not Ent_Base Then
					If Not ParentEntityList.Contains(EntityName)
						ParentEntityList.Add(entityName)
					End If
				Else
					If Not BaseEntityList.Contains(EntityName)
						BaseEntityList.Add(entityName)
					End If
				End If
				Dim lsAncestorList As List(Of memberinfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{entityName}" & ".Ancestors.Where(MemberDim = 'E_" & sCube &  "')", True,,)
				For Each Parent As Memberinfo In lsAncestorList
					If Not ConsolEntityList.Contains($"{Parent.Member.Name}")
						ConsolEntityList.Add($"{Parent.Member.Name}")
					End If
				Next
			Next
			
			If ParentEntityList.Count = 0
				ParentEntityList.Add("None")
			End If
			
			If BaseEntityList.Count = 0
				BaseEntityList.Add("None")
			End If
			
			If ConsolEntityList.Count = 0
				ConsolEntityList.Add("None")
			End If
			
			If sMode = "CMD_SPLN" Or sMode = "CMD_PGM" Then 
				parententitylist = ParentEntityList.union(consolentitylist).ToList()
			End If 
	
			Return Tuple.Create(ParentEntityList, BaseEntityList,ConsolEntityList)
		End Function
		
		Public Shared Sub ProcConsol(ByRef si As SessionInfo,ByRef consolEntityList As List(Of String),ByRef cmdName As String,ByRef wsID As GUID,ByRef consolSEQ As String,ByRef customSubstVars As Dictionary(Of String, String))
			'Run consolidation for entities in the correct order
			Dim L4Entity,L3Entity,L2Entity,L1Entity As String 
			For Each consolentity As String In consolEntityList
				Dim Entitylevel As String = GBL.GBL_Assembly.GBL_Helpers.GetEntityLevel(si, consolentity)
				If consolentity = cmdName Then EntityLevel = "L1"
				Select Case Entitylevel
				Case "L4"
					L4Entity = $"{L4Entity},E#{ConsolEntity}"
				Case "L3"
					L3Entity = $"{L3Entity},E#{ConsolEntity}"
				Case "L2"
					L2Entity = $"{L2Entity},E#{ConsolEntity}"
				Case "L1"
					L1Entity = $"{L1Entity},E#{ConsolEntity}"
				End Select
			Next
			customSubstVars.Add("consolEntityList","Default")
			If Not String.IsNullOrEmpty(L4Entity) Then
				customSubstVars("consolEntityList") = L4Entity 
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, consolSEQ, customSubstVars)
			End If 
			If Not String.IsNullOrEmpty(L3Entity) Then
				customSubstVars("consolEntityList") = L3Entity 
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, consolSEQ, customSubstVars)
			End If 
			If Not String.IsNullOrEmpty(L2Entity) Then
				customSubstVars("consolEntityList") = L2Entity 
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, consolSEQ, customSubstVars)
			End If 
			If Not String.IsNullOrEmpty(L1Entity) Then
				customSubstVars("consolEntityList") = L1Entity 
				BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, consolSEQ, customSubstVars)
			End If 
		End Sub
#Region "Set Parameter"
		'Set a parameter with passed in value using selectionChangedTaskResult
		Public Shared Function SetParameter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal dKeyVal As Dictionary(Of String, String), Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing )As Object				
			If selectionChangedTaskResult Is Nothing Then
				selectionChangedTaskResult = New XFSelectionChangedTaskResult()
			End If
			
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True			
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			
			For Each KeyVal As KeyValuePair(Of String, String) In dKeyVal
				If selectionChangedTaskResult.ModifiedCustomSubstVars.ContainsKey(KeyVal.Key)
					selectionChangedTaskResult.ModifiedCustomSubstVars.XFSetValue(KeyVal.Key, KeyVal.Value)
				Else
					selectionChangedTaskResult.ModifiedCustomSubstVars.Add(KeyVal.Key, KeyVal.Value)
				End If
			
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add(KeyVal.Key, KeyVal.Value)			
			Next
			
			Return selectionChangedTaskResult
		End Function
#End Region	
		
		Public Shared Function ClearSelections(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String)As Object
			Dim objDictionary = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues
			For i As Integer = 0 To objDictionary.Count -1
				'Clear only prompts that are passed in as parameters
				Dim thisKey = objDictionary.ElementAt(i).Key
				Dim thisValue = objDictionary.ElementAt(i).Value
				If(ParamsToClear.XFContainsIgnoreCase(thisKey)) Then
					objDictionary.Remove(thisKey)
					objDictionary.Add(thisKey,"")
					
				End If 
			Next
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = False
			selectionChangedTaskResult.Message = ""
			selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
			selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing 'objXFSelectionChangedUIActionInfo
			selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
			selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
			selectionChangedTaskResult.ModifiedCustomSubstVars = objDictionary
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = objDictionary
			
			Return selectionChangedTaskResult
		End Function

#Region "Send Email"
Public Shared Function SendStatusChangeEmail(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs,  Optional ByVal Status As String = "",Optional ByVal ReqID As String = "")
    Try
        ' Get Workflow Details for the SQL Query
        Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
        Dim UserName As String = si.UserName

        ' Initialize DataTable to hold results
        Dim DataEmail As New DataTable()
        
        'Brapi.ErrorLog.LogMessage(si,"Status" & Status)
		' Brapi.ErrorLog.LogMessage(si,"ID" & ReqID)
        ' --- 2. DATABASE CONNECTION & QUERY EXECUTION ---
        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
                sqlConn.Open()
            	Dim TableName As String =  ""
				Dim wfScenarioName As String = wfInfoDetails("ScenarioName").ToString()
				Dim WF As String() = wfScenarioName.Split("_")
				Dim sScerario As String = WF(1)
				'Brapi.ErrorLog.LogMessage(si,"HERE")
       			 Select Case sScerario
            		Case "PGM"
               		 	TableName = "XFC_CMD_PGM_REQ"
						'Brapi.ErrorLog.LogMessage(si,"HERE" & TableName)
           			 Case "SPLN"
                		TableName = "XFC_CMD_SPLN_REQ"
            		Case Else
                		Return Nothing
       			End Select
				
                Dim SqlString As String = "Select Title, Status, Create_User, Create_Date, Notification_List_Emails,Entity, Validation_List_Emails
                                          From " & TableName & "
                                          Where WFScenario_Name = @WFScenario_Name 
                                          And WFCMD_Name = @WFCMD_Name 
                                          And WFTime_Name = @WFTime_Name
										  And REQ_ID = @REQ_ID"
                'Brapi.ErrorLog.LogMessage(si,"HERE" & SqlString)
               Dim paramlist As New List(Of SqlParameter)()
                
                paramlist.Add(New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")})
                paramlist.Add(New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")})
                paramlist.Add(New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")})
				paramlist.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = ReqID})
                
               
                
                ' Execute the Query and fill the DataTable
                Using sqlCommand As New SqlCommand(SqlString, sqlConn)
                    sqlCommand.Parameters.AddRange(paramlist.ToArray())
                    
                    Using sqa As New SqlDataAdapter(sqlCommand)
                        sqa.Fill(DataEmail)
                    End Using
                End Using
            End Using 
        End Using 
        
        
        ' --- 3. PROCESS DATA & SEND EMAIL ---
        
        ' Get Email Parameters
        Dim EmailConnectorStr As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Var_Email_Connector_String")
        Dim BodyDisclaimerBody As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "varEmailDisclaimer")
        
        For Each drow As DataRow In DataEmail.Rows 
            
            Dim ReqTitle As String = drow("Title").ToString()
            Dim ReqStatus As String = Status
            Dim CreateDate As String = drow("Create_Date").ToString()
            Dim FundCenter As String = drow("Entity").ToString()
            Dim StatusChangeEmailIs As New List(Of String)
			Dim ValidationEmailIs As New List(Of String)
            Dim EmailIs As String = drow("Notification_List_Emails").ToString()
			Dim VEmailIs As String = drow("Validation_List_Emails").ToString()
			    If Not String.IsNullOrWhiteSpace(EmailIs) Then

			        ' Define the potential delimiters to check 
			        Dim potentialDelimiters As Char() = {","c, ";"c, "|"c, " "c}
			       
			        Dim emailDelimiter As Char = DetectDelimiter(EmailIs, potentialDelimiters)
			        
			        ' Split the string using the detected delimiter
			        Dim emailArray As String() = EmailIs.Split(emailDelimiter, StringSplitOptions.RemoveEmptyEntries)
			        
					' Trim whitespace from each email address and add it to the List(Of String)
			        For Each singleEmail As String In emailArray
			            Dim trimmedEmail As String = singleEmail.Trim()
			            If Not String.IsNullOrWhiteSpace(trimmedEmail) Then
			                StatusChangeEmailIs.Add(trimmedEmail)
			            End If
			        Next
			    End If
            
            Dim StatusChangeSubject As String = "Requirement Status Change"
            
            ' Build Body
            Dim StatusChangeBody As String = "A Requirement Request for Funds Center: " & FundCenter & _
                                             " with Requirement Title: " & ReqTitle & _
                                             " has changed status to **" & ReqStatus & "**." & _
                                             " Submitted by: " & UserName & " - " & CreateDate & vbCrLf & BodyDisclaimerBody
            
            ' Send email
            If StatusChangeEmailIs.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(EmailConnectorStr) Then
              ' Brapi.Errorlog.LogMessage(si, "Attempting to send status change email to: " & EmailIs)
                
                ' Call the utility function to send the email
               ' Brapi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmailIs, StatusChangeSubject, StatusChangeBody, Nothing)
                
              ' Brapi.Errorlog.LogMessage(si, "Successfully triggered email for Request Title: " & ReqTitle)
            End If
			
        'Validation Email List    
			If Not String.IsNullOrWhiteSpace(VEmailIs) And ReqStatus.XFContainsIgnoreCase("Validate") Then
				
			        ' Split the string using the detected delimiter
			        Dim VemailArray As String() = VEmailIs.Split(",")
			        
					' Trim whitespace from each email address and add it to the List(Of String)
			        For Each vsingleEmail As String In vemailArray
			            Dim vtrimmedEmail As String = vsingleEmail.Trim()
			            If Not String.IsNullOrWhiteSpace(vtrimmedEmail) Then
			                ValidationEmailIs.Add(vtrimmedEmail)
			            End If
			        Next
			    End If
            
            Dim ValidationSubject As String = "Request for Requirement Validation"
            
            ' Build Body
            Dim ValidationBody As String = "A requirement has been submitted to be reviewed and validated for prioritization or approval for Fund Center:" & FundCenter & _
                                             " with Requirement Title: " & ReqTitle & _
                                             " Submitted by: " & UserName & " - " & CreateDate & vbCrLf & BodyDisclaimerBody
            
            ' Send email
            If ValidationEmailIs.Count > 0 AndAlso Not String.IsNullOrWhiteSpace(EmailConnectorStr) Then
               ' Brapi.Errorlog.LogMessage(si, "Attempting to send Validate change email to: " & VEmailIs)
                
                ' Call the utility function to send the email
               ' Brapi.Utilities.SendMail(si, EmailConnectorStr, StatusChangeEmailIs, StatusChangeSubject, StatusChangeBody, Nothing)
                
              ' Brapi.Errorlog.LogMessage(si, "Successfully triggered validate email for Request Title: " & ReqTitle)
            End If
			
        Next
        
        Return Nothing 
        
   Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try            
    
End Function
#End Region

#Region "Helper Fnctions"
' Helper function to find the delimiter used in a string
Public Shared Function DetectDelimiter(ByVal inputString As String, ByVal potentialDelimiters As Char()) As Char
    ' Default to the first potential delimiter if none are found, or a comma if the list is empty
    Dim defaultDelimiter As Char = If(potentialDelimiters IsNot Nothing AndAlso potentialDelimiters.Length > 0, potentialDelimiters(0), ","c)

    If String.IsNullOrWhiteSpace(inputString) Then
        Return defaultDelimiter
    End If

    For Each delimiter As Char In potentialDelimiters
        ' Check if the string contains the delimiter
        If inputString.Contains(delimiter) Then
            ' Check if splitting yields more than one non-empty entry
            If inputString.Split(delimiter).Length > 1 Then
                Return delimiter
            End If
        End If
    Next

    Return defaultDelimiter
End Function
#End Region	


		Public Shared Function GetNewUFRGuid(si As SessionInfo) As String
'		Public Shared Function GetNewUFRGuid(si As SessionInfo, UFR_ID As String, sEntity As String, sAPPNInput As String, sU2Input As String, sU3Input As String, sU4Input As String, sU5Input As String, sU6Input As String, sSAGInput As String) As String
			If si Is Nothing Then
				Throw New Exception("GetNewUFRGuid: SessionInfo is Nothing.")
			End If
			
			Dim settingGroup As String = "UFR"
			Dim settingName As String = "UFR_GUID"
'			Dim settingName As String = $"UFR_GUID{UFR_ID}{sEntity}{sAPPNInput}{sU2Input}{sU3Input}{sU4Input}{sU5Input}{sU6Input}"
			
			Dim existing As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName,"")
			
			If Not String.IsNullOrWhiteSpace(existing) Then
				Return existing
			End If
			
			Dim newGuid As String = System.Guid.NewGuid().ToString("D")
			
			Brapi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName, newGuid)
			
			Return newGuid
			
'			Return System.Guid.NewGuid().ToString("D")
		End Function
		
		Public Shared Sub ClearUFRGuid(si As SessionInfo)
			
			If si Is Nothing Then Exit Sub
				
			Const settingGroup As String = "UFR"
			Const settingName As String = "UFR_GUID"
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName, "")
			
		End Sub
		
#Region "UFR/REQ Valid Flag"	

		Private Const settingGroup As String = "UFR"
		Private Const settingNameValid As String = "UFR_Valid"
		
		Public Shared Sub ResetUFRState(si As SessionInfo)
			If si Is Nothing Then Exit Sub
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingNameValid, "1")
		End Sub
		
		Public Shared Sub SetUFRValid(si As SessionInfo, isValid As Boolean)
			If si Is Nothing Then Exit Sub
			Dim value As String = If(isValid, "1", "0")
			Brapi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingNameValid, "")
		End Sub
		
		Public Shared Function IsUFRValid(si As SessionInfo) As Boolean
			If si Is Nothing Then Return False
				
				Dim value As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingNameValid, "")
				
				Return (value = "1")
		End Function


#End Region

#Region "Get Manage Workflow Table"
		Public Shared Function GetWorkflowManageStatus(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs, ByVal ParamsToClear As String) As Object
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sModule As String = String.Empty
			
			If wfProfileName.XFContainsIgnoreCase("PGM") Then
				sModule = "PGM"
			Else If wfProfileName.XFContainsIgnoreCase("SPLN") Then
				sModule = "SPLN"
			Else If wfProfileName.XFContainsIgnoreCase("UFR") Then
				sModule = "UFR"
			End If
			
				Dim SQA_XFC_CMD_MG_Workflow_DT = New DataTable()
				
				Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using connection As New SqlConnection(dbConnApp.ConnectionString)
					connection.Open()
					Dim sqa_xfc_cmd_mg_workflow = New SQA_XFC_CMD_MG_Workflow(connection)
					Dim sqa = New SqlDataAdapter()
					Dim sql As String = $"SELECT * 
										FROM XFC_CMD_MG_Workflow
										WHERE Command = @Command
										AND Module = @Module
										"				
				    Dim paramList As New List(Of SqlParameter) From {
			        New SqlParameter("@Command", SqlDbType.NVarChar) With {.Value = sCube},
			        New SqlParameter("@Module", SqlDbType.NVarChar) With {.Value = sModule}
			   		}
					
				    Dim sqlparams As SqlParameter() = paramList.ToArray()
					sqa_xfc_cmd_mg_workflow.Fill_XFC_CMD_MG_Workflow_DT(sqa, SQA_XFC_CMD_MG_Workflow_DT, sql, sqlparams)
				End Using
				
				Return SQA_XFC_CMD_MG_Workflow_DT
				
		End Function
#End Region


#Region "***COPY*** of the GetNewUFRGuid"
'		Public Shared Function GetNewUFRGuid(si As SessionInfo) As String
		Public Shared Function GetNewUFRGuidCopy(si As SessionInfo, UFR_ID As String, sEntity As String, sAPPNInput As String, sU2Input As String, sU3Input As String, sU4Input As String, sU5Input As String, sU6Input As String, sSAGInput As String) As String
			If si Is Nothing Then
				Throw New Exception("GetNewUFRGuid: SessionInfo is Nothing.")
			End If
			
			Dim settingGroup As String = "UFR"
'			Dim settingName As String = "UFR_GUID"
			Dim settingName As String = $"UFR_GUID{UFR_ID}{sEntity}{sAPPNInput}{sU2Input}{sU3Input}{sU4Input}{sU5Input}{sU6Input}{sSAGInput}"
			
			Dim existing As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName,"")
			
			If Not String.IsNullOrWhiteSpace(existing) Then
				Return existing
			End If
			
			Dim newGuid As String = System.Guid.NewGuid().ToString("D")
			
			Brapi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName, newGuid)
			
			Return newGuid
			
'			Return System.Guid.NewGuid().ToString("D")
		End Function
		
		Public Shared Sub ClearUFRGuidCopy(si As SessionInfo, UFR_ID As String, sEntity As String, sAPPNInput As String, sU2Input As String, sU3Input As String, sU4Input As String, sU5Input As String, sU6Input As String, sSAGInput As String)
			
			If si Is Nothing Then Exit Sub
				
			Dim settingGroup As String = "UFR"
			Dim settingName As String = $"UFR_GUID{UFR_ID}{sEntity}{sAPPNInput}{sU2Input}{sU3Input}{sU4Input}{sU5Input}{sU6Input}{sSAGInput}"
			
			BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, settingGroup, settingName, "")
			
		End Sub
#End Region

	End Class
End Namespace