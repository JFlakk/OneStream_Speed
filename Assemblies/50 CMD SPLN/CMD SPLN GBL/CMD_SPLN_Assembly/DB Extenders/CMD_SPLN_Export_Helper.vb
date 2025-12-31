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
Imports Microsoft.Data.SqlClient
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports Workspace.GBL.GBL_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_SPLN_Export_Helper
Public Class MainClass
		Private si As SessionInfo
        Private globals As BRGlobals
        Private api As Object
        Private args As DashboardExtenderArgs
		Private sFilePath As String = String.Empty
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Me.si = si
            Me.globals = globals
            Me.api = api
            Me.args = args
			Try
				Select Case args.FunctionType

					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						Select Case args.FunctionName
							'Export PGM Requirements to Excel
							Case "ExportAllREQs"					
								Return Me.ExportAllREQs()
							Case "ExportforUpdate"						
								Return Me.ExportforUpdate()
						End Select

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function


#Region "Export Helper"
'		'----------------------------------------------------------------------------------
		'     Create data tables to be used for fetching and processing fetched data
		'----------------------------------------------------------------------------------
		Private Function GenerateReportFile(ByVal si As SessionInfo, ByVal processDT As DataTable, ByVal sFileHeader As String, ByVal sCube As String, ByVal iTime As Integer, ByVal sTemplate As String, ByVal sFvParam As String)
			Try
				'Initialize file 
				Dim file As New Text.StringBuilder
				file.Append(sFileHeader)	

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
			    Dim fileName As String = $"{sCube}_{iTime}_{sTemplate}_{sUser}_{sTimeStamp}.csv"
			
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
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "BuildFile"
		
		'----------------------------------------------------------------------------------
		'    Build export file
		'----------------------------------------------------------------------------------
		Private Sub BuildFile(ByVal si As SessionInfo, ByVal sFileContent As String, ByVal sFileName As String, ByVal sCommandName As String)
			Try
				'Pass text to bytes
				Dim fileBytes As Byte() = Encoding.UTF8.GetBytes(sFileContent)
				
				'Define folder to hold file
				Dim sFolderPath As String = "Documents/Users/" & si.UserName
				Dim objXFFolderEx As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, sFolderPath)
				
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
		
#Region "Set Parameter"
		'Set a parameter with passed in value using selectionChangedTaskResult
		Public Function SetParameter(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal dKeyVal As Dictionary(Of String, String), Optional ByVal selectionChangedTaskResult As XFSelectionChangedTaskResult = Nothing )As Object				
			If selectionChangedTaskResult Is Nothing Then
				selectionChangedTaskResult = New XFSelectionChangedTaskResult()
			End If
			
			selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True			
			selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True
			
			For Each KeyVal As KeyValuePair(Of String, String) In dKeyVal
				selectionChangedTaskResult.ModifiedCustomSubstVars.Remove(KeyVal.Key)
				selectionChangedTaskResult.ModifiedCustomSubstVars.Add(KeyVal.Key, KeyVal.Value)
			
				selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add(KeyVal.Key, KeyVal.Value)			
			Next
			
			Return selectionChangedTaskResult
		End Function
#End Region			
		
#End Region		
		
#Region "ExportAllREQs(Review Dashboard)"	
		'Export SPLN Requirement Data
		Public Function ExportAllREQs() As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sFundCenter)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sCarryoverYr As Integer = iTime + 1
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			Dim sReq_ID As String = args.NameValuePairs.XFGetValue("req_IDs","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			
		
'Brapi.ErrorLog.LogMessage(si,"IDs" & sReq_ID)
			If sFundCenter.XFContainsIgnoreCase("_General") Then
				sFundCenter = sFundCenter.Replace("_General","")
			Else 
				sFundCenter = sFundCenter
			End If
				
				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Throw New Exception("Please select a Command to export")
				End If
			
				Dim FCArray() As String = sFundCenter.Split(","c)
	
       		 ' Convert the array to a list
        		Dim FCList As List (Of String) = FCArray.ToList
			
				Dim FCMulti As New Text.StringBuilder()
				

	        ' Loop through Each FC in multiselect
	       		For Each FC As String In FCList
					
					'If the fund center passed is not a descendent of the WF then stop
					FC = FC.Trim()
					Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  FC & ")"
					Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
'Brapi.ErrorLog.LogMessage(si, "FC:" & FC)
	'
					If Not cbMembers.Count > 0 Then
						Return Nothing
					End If
					
							
					'---Checking if Base------
	
					Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
						
	         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)
						
					Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FC)	
					
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
						Next	
						
							Dim allFCs As String = ""
				
					
				allFCs = FCMulti.ToString()
	
			
			
			'Declare variables to fetch data
		
			Dim sFileHeader As String = ""
					
			
	sFileHeader = $"SCENARIO,FundsCenter,REQID,Status,FUND,MDEP,APE9,DollarType,ObjectClass,CType,Total Commitment,Total Obligation,Title,Description,RelatedRequest,Justification,CostMethodology,ImpactIfNotFunded,RiskIfNotFunded,CostGrowthJustification,MustFund,RequestedFundSource,ArmyInitiativeDirective,CommandInitiativeDirective,ActivityExercise,ITCyberRequirement,UIC,FlexField1,FlexField2,FlexField3,FlexField4,FlexField5,EmergingRequirement,CPACandidate,PBRSubmission,UPLSubmission,ContractNumber,TaskOrderNumber,AwardTargetDate,POPExpirationDate,CME,COREmail,POCEmail,Directorate,Division,Branch,ReviewingPOCEmail,MDEPFunctionalEmail,NotificationEmailList,CreatorName,CreationDateTime,LastUpdatedName,LastUpdatedDate"

	 '---------------------------------------------------
    Dim filterClause As String = ""
    If Not String.IsNullOrWhiteSpace(sReq_ID) Then
        ' If sReq_ID has values, format them for an SQL IN clause
        Dim quotedReqIDs As String = String.Join(",", sReq_ID.Split(","c).Select(Function(id) $"'{id.Trim()}'"))
        filterClause = $"AND Req.REQ_ID IN ({quotedReqIDs})"
    Else
        ' Otherwise, use the original filter for all Fund Centers
        filterClause = $"AND Req.Entity IN ({allFCs})"
    End If
	
'Fill the DataTable 
Dim sql As String = $"
With AggregatedData As (
Select
    Req.WFScenario_Name,Req.Entity,Req.REQ_ID,Req.Status,Req.APPN,Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,
 	SUM(CASE WHEN Dtl.Account = 'Commitments' and Dtl.Fiscal_Year = {iTime}
        THEN (
            Dtl.Month1 + Dtl.Month2 + Dtl.Month3 + Dtl.Month4 + 
            Dtl.Month5 + Dtl.Month6 + Dtl.Month7 + Dtl.Month8 + 
            Dtl.Month9 + Dtl.Month10 + Dtl.Month11 + Dtl.Month12
        )
		WHEN Dtl.Account = 'Commitments' AND dtl.Fiscal_Year = {iTime + 1}
        THEN Dtl.Yearly
        ELSE 0 
    END
) AS Total_Commitments,
	SUM(CASE WHEN Dtl.Account = 'Obligations' and Dtl.Fiscal_Year = {iTime}
        THEN (
            Dtl.Month1 + Dtl.Month2 + Dtl.Month3 + Dtl.Month4 + 
            Dtl.Month5 + Dtl.Month6 + Dtl.Month7 + Dtl.Month8 + 
            Dtl.Month9 + Dtl.Month10 + Dtl.Month11 + Dtl.Month12
        )
		 WHEN Dtl.Account = 'Obligations' AND dtl.Fiscal_Year = {iTime + 1}
        THEN Dtl.Yearly

        ELSE 0 
    END
) AS Total_Obligations,
	Req.Title,
    Req.Description,Req.Related_REQs, Req.Justification,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded, Req.Cost_Growth_Justification,
    Req.Must_Fund, Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise,
    Req.IT_Cyber_REQ, Req.UIC, Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Emerging_REQ, Req.CPA_Topic,
    Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num,
    Req.Award_Target_Date, Req.POP_Exp_Date, Req.CME, Req.COR_Email,
    Req.POC_Email,Req.Directorate, Req.Div, Req.Branch, Req.Review_POC_Email,Req.MDEP_Functional_Email, Req.Notification_List_Emails,
    Req.Create_User,Req.Create_Date,Req.Update_User,Req.Update_Date
   From
    XFC_CMD_SPLN_REQ As Req
LEFT Join
    XFC_CMD_SPLN_REQ_Details As Dtl
On
    Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
    And Req.WFScenario_Name = Dtl.WFScenario_Name
    And Req.WFCMD_Name = Dtl.WFCMD_Name
    And Req.WFTime_Name = Dtl.WFTime_Name
	
Where
    Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
    And Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
    And Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
	{filterClause} 

Group by  Req.WFScenario_Name,
Req.REQ_ID, Req.Entity,Req.Title,Req.Status,Req.APPN,Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,
    Req.Description,Req.Related_REQs, Req.Justification,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded, Req.Cost_Growth_Justification,
    Req.Must_Fund, Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise,
    Req.IT_Cyber_REQ, Req.UIC, Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Emerging_REQ, Req.CPA_Topic,
    Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num,
    Req.Award_Target_Date, Req.POP_Exp_Date, Req.CME, Req.COR_Email,
    Req.POC_Email,Req.Directorate, Req.Div, Req.Branch, Req.Review_POC_Email,Req.MDEP_Functional_Email, Req.Notification_List_Emails,
   	Req.Create_User,Req.Create_Date,Req.Update_User,Req.Update_Date
	
)
	SELECT
   WFScenario_Name,Entity,REQ_ID,Status,APPN,MDEP,APE9, Dollar_Type, Obj_Class, CType,Total_Commitments,Total_Obligations,
    
   	'""' + ISNULL(REPLACE(CAST(Title AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Title,
    '""' + ISNULL(REPLACE(CAST(Description AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Description,
	'""' + ISNULL(REPLACE(CAST(Related_REQs AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Related_REQs,
    '""' + ISNULL(REPLACE(CAST(Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Justification,
    '""' + ISNULL(REPLACE(CAST(Cost_Methodology AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Methodology,
    '""' + ISNULL(REPLACE(CAST(Impact_Not_Funded AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Impact_Not_Funded,
    '""' + ISNULL(REPLACE(CAST(Risk_Not_Funded AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Risk_Not_Funded,
    '""' + ISNULL(REPLACE(CAST(Cost_Growth_Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Growth_Justification,
    '""' + ISNULL(REPLACE(CAST(Must_Fund AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Must_Fund,
    '""' + ISNULL(REPLACE(CAST(Funding_Src AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Funding_Src,
    '""' + ISNULL(REPLACE(CAST(Army_Init_Dir AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Army_Init_Dir,
    '""' + ISNULL(REPLACE(CAST(CMD_Init_Dir AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CMD_Init_Dir,
    '""' + ISNULL(REPLACE(CAST(Activity_Exercise AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Activity_Exercise,
    '""' + ISNULL(REPLACE(CAST(IT_Cyber_REQ AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS IT_Cyber_REQ,
    '""' + ISNULL(REPLACE(CAST(UIC AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS UIC,
    '""' + ISNULL(REPLACE(CAST(FF_1 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_1,
    '""' + ISNULL(REPLACE(CAST(FF_2 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_2,
    '""' + ISNULL(REPLACE(CAST(FF_3 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_3,
    '""' + ISNULL(REPLACE(CAST(FF_4 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_4,
    '""' + ISNULL(REPLACE(CAST(FF_5 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_5,
    '""' + ISNULL(REPLACE(CAST(Emerging_REQ AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Emerging_REQ,
    '""' + ISNULL(REPLACE(CAST(CPA_Topic AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CPA_Topic,
    '""' + ISNULL(REPLACE(CAST(PBR_Submission AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS PBR_Submission,
    '""' + ISNULL(REPLACE(CAST(UPL_Submission AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS UPL_Submission,
    '""' + ISNULL(REPLACE(CAST(Contract_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Contract_Num,
    '""' + ISNULL(REPLACE(CAST(Task_Order_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Task_Order_Num,
    '""' + ISNULL(REPLACE(CAST(Award_Target_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Award_Target_Date,
    '""' + ISNULL(REPLACE(CAST(POP_Exp_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS POP_Exp_Date,
    '""' + ISNULL(REPLACE(CAST(CME AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CME,
    '""' + ISNULL(REPLACE(CAST(COR_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS COR_Email,
    '""' + ISNULL(REPLACE(CAST(POC_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS POC_Email,
    '""' + ISNULL(REPLACE(CAST(Directorate AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Directorate,
    '""' + ISNULL(REPLACE(CAST(Div AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Div,
    '""' + ISNULL(REPLACE(CAST(Branch AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Branch,
    '""' + ISNULL(REPLACE(CAST(Review_POC_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Review_POC_Email,
    '""' + ISNULL(REPLACE(CAST(MDEP_Functional_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS MDEP_Functional_Email,
    '""' + ISNULL(REPLACE(CAST(Notification_List_Emails AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Notification_List_Emails,
    '""' + ISNULL(REPLACE(CAST(Create_User AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Create_User,
	'""' + ISNULL(REPLACE(CAST(Create_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Create_Date,
	'""' + ISNULL(REPLACE(CAST(Update_User AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Update_User,
	'""' + ISNULL(REPLACE(CAST(Update_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Update_Date
	
    


FROM AggregatedData;"
			
		Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "ExportAllReqs")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
	    
	

		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region

#Region "ExportforUpdate(ImportDashboard)"	
		'Export PGM Requirement Data
		Public Function ExportforUpdate() As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sFundCenter)
			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

				If String.IsNullOrWhiteSpace(sFundCenter) Then 
					Throw New Exception("Please select a Command to export")
				End If
			
				Dim FCArray() As String = sFundCenter.Split(","c)
	
       		 ' Convert the array to a list
        		Dim FCList As List (Of String) = FCArray.ToList
			
				Dim FCMulti As New Text.StringBuilder()
				

	        ' Loop through Each FC in multiselect
	       		For Each FC As String In FCList
						
					'If the fund center passed is not a descendent of the WF then stop
					FC = FC.Trim()
					Dim mbrScrpt As String = "E#" & sCube & ".DescendantsInclusive.Where(Name Contains " &  FC & ")"
					Dim cbMembers As List (Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_" & sCube, mbrScrpt, True  )
'Brapi.ErrorLog.LogMessage(si, "FC:" & FC)
	'
					If Not cbMembers.Count > 0 Then
						Return Nothing
					End If
					
							
					'---Checking if Base------
	
'					Dim entityPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & sCube)
						
'	         		Dim nAncestorID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sCube)
						
'					Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, FC.Replace("_General",""))	
					
'					Dim isBase As Boolean = BRApi.Finance.Members.IsBase(si,entityPk, nAncestorID, nBaseID)
''Brapi.ErrorLog.LogMessage(si,"Here 2.4")	
'					If Not isBase Then FC = FC & ".Base"
	
					Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_Army", "E#" & FC,True)
'Brapi.ErrorLog.LogMessage(si, "FC count " & LFundCenters.Count)
						For Each FundCenter As MemberInfo In LFundCenters
							If FCMulti.Length > 0 Then
								FCMulti.Append(",")
							End If
							FCMulti.Append($"'{FundCenter.Member.Name}'")
						
						Next
				Next	
						
							Dim allFCs As String = ""
				
					
				allFCs = FCMulti.ToString()
				
'Brapi.ErrorLog.LogMessage(si, "FC" & allFCs)
				
		'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			
			
			'Declare variables to fetch data
		
			Dim sFileHeader As String = ""
					
			
	sFileHeader = $"FundsCenter,REQ_ID,APPN,MDEP,APE9,DOLLARTYPE,OBJECTCLASS,CTYPE,Cycle,FY1,FY2,FY3,FY4,FY5,Title,Description,Justification,CostMethodology,ImpactIfNotFunded,RiskIfNotFunded,CostGrowthJustification,MustFund,FundingSource,ArmyInitiative_Directive,CommandInitiative_Directive,Activity_Exercise,IT_CyberRequirement,UIC,Flex1,Flex2,Flex3,Flex4,Flex5,EmergingRequirement_ER,CPATopic,PBRSubmission,UPLSubmission,ContractNumber,TaskOrderNumber,AwardTargetDate,POPExpirationDate,ContractManYearEquiv_CME,COREmail,POCEmail,Directorate,Division,Branch,ReviewingPOCEmail,MDEPFunctionalEmail,NotificationEmailList,GeneralComments_Notes,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMSAITRNUM,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"	

	 '---------------------------------------------------
    Dim filterClause As String = ""
        
        filterClause = $"AND Req.Entity IN ({allFCs})"
'Fill the DataTable 
Dim sql As String = $"
WITH AggregatedData As (
Select
    Req.Entity,Req.REQ_ID,Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,Req.WFTime_Name,
 	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_1 ELSE NULL END) AS FY{iTime0},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_2 ELSE NULL END) AS FY{iTime1},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_3 ELSE NULL END) AS FY{iTime2},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_4 ELSE NULL END) AS FY{iTime3},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_5 ELSE NULL END) AS FY{iTime4},
	Req.Title,
    Req.Description, Req.Justification,Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded, Req.Cost_Growth_Justification,
    Req.Must_Fund, Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise,
    Req.IT_Cyber_REQ, Req.UIC, Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5,Req.Emerging_REQ, Req.CPA_Topic,
    Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num,
    Req.Award_Target_Date, Req.POP_Exp_Date, Req.Contractor_ManYear_Equiv_CME, Req.COR_Email,
    Req.POC_Email,Req.Directorate, Req.Div, Req.Branch, Req.Review_POC_Email,Req.MDEP_Functional_Email, Req.Notification_List_Emails,
    Req.Gen_Comments_Notes, Req.JUON, Req.ISR_Flag,
    Req.Cost_Model, Req.Combat_Loss, Req.Cost_Location, Req.Cat_A_Code, Req.CBS_Code, Req.MIP_Proj_Code,  Req.SS_Priority, Req.Commit_Group, Req.SS_Cap, Req.Strategic_BIN, Req.LIN,
   	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_1 ELSE NULL END) AS FY1_QTY,
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_2 ELSE NULL END) AS FY2_QTY,
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_3 ELSE NULL END) AS FY3_QTY,
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_4 ELSE NULL END) AS FY4_QTY,
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_5 ELSE NULL END) AS FY5_QTY,
	Req.REQ_Type, Req.DD_Priority, Req.Portfolio,
    Req.DD_Cap, Req.JNT_Cap_Area, Req.TBM_Cost_Pool, Req.TBM_Tower, Req.APMS_AITR_Num,
    Req.Zero_Trust_Cap, Req.Assoc_Directives, Req.Cloud_IND, Req.Strat_Cyber_Sec_PGM, Req.Notes,
	MAX(CASE WHEN Dtl.Unit_of_Measure != 'Funding' and Dtl.Account != 'Quantity' THEN Dtl.Unit_of_Measure ELSE Null END)as Unit_of_Measure,
	MAX(Case When Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_1 ELSE NULL END) AS Manpower_FY_1,
    MAX(Case When Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_1 ELSE NULL END) AS Manpower_FY_1_Cost,
    MAX(Case When Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_2 ELSE NULL END) AS Manpower_FY_2,
    MAX(Case When Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_2 ELSE NULL END) AS Manpower_FY_2_Cost,
    MAX(Case When Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_3 ELSE NULL END) AS Manpower_FY_3,
    MAX(Case When Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_3 ELSE NULL END) AS Manpower_FY_3_Cost,
    MAX(Case When Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_4 ELSE NULL END) AS Manpower_FY_4,
    MAX(Case When Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_4 ELSE NULL END) AS Manpower_FY_4_Cost,
    MAX(Case When Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_5 ELSE NULL END) AS Manpower_FY_5,
    MAX(Case When Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_5 ELSE NULL END) AS Manpower_FY_5_Cost
   From
    XFC_CMD_PGM_REQ As Req
LEFT Join
    XFC_CMD_PGM_REQ_Details As Dtl
On
    Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
    And Req.WFScenario_Name = Dtl.WFScenario_Name
    And Req.WFCMD_Name = Dtl.WFCMD_Name
    And Req.WFTime_Name = Dtl.WFTime_Name
	
Where
    Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
    And Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
    And Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
	And Req.REQ_ID_Type <> 'Manpower'
	{filterClause} 

Group by
Req.REQ_ID, Req.Title,Req.WFTime_Name,
    Req.Description, Req.Justification, Req.Entity, Req.REQ_Type, Req.DD_Priority, Req.Portfolio,
    Req.DD_Cap, Req.JNT_Cap_Area, Req.TBM_Cost_Pool, Req.TBM_Tower, Req.APMS_AITR_Num,
    Req.Zero_Trust_Cap, Req.Assoc_Directives, Req.Cloud_IND, Req.Strat_Cyber_Sec_PGM, Req.Notes,
    Req.SS_Priority, Req.Commit_Group, Req.SS_Cap, Req.Strategic_BIN, Req.LIN, Req.Status,
    Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType, Req.UIC,
    Req.Cost_Methodology, Req.Impact_Not_Funded, Req.Risk_Not_Funded, Req.Cost_Growth_Justification,
    Req.Must_Fund, Req.Funding_Src, Req.Army_Init_Dir, Req.CMD_Init_Dir, Req.Activity_Exercise,
    Req.Directorate, Req.Div, Req.Branch, Req.IT_Cyber_REQ, Req.Emerging_REQ, Req.CPA_Topic,
    Req.PBR_Submission, Req.UPL_Submission, Req.Contract_Num, Req.Task_Order_Num,
    Req.Award_Target_Date, Req.POP_Exp_Date, Req.Contractor_ManYear_Equiv_CME, Req.COR_Email,
    Req.POC_Email, Req.Review_POC_Email, Req.MDEP_Functional_Email, Req.Notification_List_Emails,
    Req.Gen_Comments_Notes, Req.FF_1, Req.FF_2, Req.FF_3, Req.FF_4, Req.FF_5, Req.Create_Date,
    Req.Create_User, Req.Update_Date, Req.Update_User, Req.Related_REQs, Req.JUON, Req.ISR_Flag,
    Req.Cost_Model, Req.Combat_Loss, Req.Cost_Location, Req.Cat_A_Code, Req.CBS_Code, Req.MIP_Proj_Code
	)		
SELECT
    Entity, REQ_ID, APPN, MDEP, APE9, Dollar_Type, Obj_Class, CType,WFTime_Name,
    FY{iTime0}, FY{iTime1}, FY{iTime2}, FY{iTime3}, FY{iTime4},
   '""' + ISNULL(REPLACE(CAST(Title AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Title,
    '""' + ISNULL(REPLACE(CAST(Description AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Description,
    '""' + ISNULL(REPLACE(CAST(Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Justification,
    '""' + ISNULL(REPLACE(CAST(Cost_Methodology AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Methodology,
    '""' + ISNULL(REPLACE(CAST(Impact_Not_Funded AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Impact_Not_Funded,
    '""' + ISNULL(REPLACE(CAST(Risk_Not_Funded AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Risk_Not_Funded,
    '""' + ISNULL(REPLACE(CAST(Cost_Growth_Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Growth_Justification,
    '""' + ISNULL(REPLACE(CAST(Must_Fund AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Must_Fund,
    '""' + ISNULL(REPLACE(CAST(Funding_Src AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Funding_Src,
    '""' + ISNULL(REPLACE(CAST(Army_Init_Dir AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Army_Init_Dir,
    '""' + ISNULL(REPLACE(CAST(CMD_Init_Dir AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CMD_Init_Dir,
    '""' + ISNULL(REPLACE(CAST(Activity_Exercise AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Activity_Exercise,
    '""' + ISNULL(REPLACE(CAST(IT_Cyber_REQ AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS IT_Cyber_REQ,
    '""' + ISNULL(REPLACE(CAST(UIC AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS UIC,
    '""' + ISNULL(REPLACE(CAST(FF_1 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_1,
    '""' + ISNULL(REPLACE(CAST(FF_2 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_2,
    '""' + ISNULL(REPLACE(CAST(FF_3 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_3,
    '""' + ISNULL(REPLACE(CAST(FF_4 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_4,
    '""' + ISNULL(REPLACE(CAST(FF_5 AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS FF_5,
    '""' + ISNULL(REPLACE(CAST(Emerging_REQ AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Emerging_REQ,
    '""' + ISNULL(REPLACE(CAST(CPA_Topic AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CPA_Topic,
    '""' + ISNULL(REPLACE(CAST(PBR_Submission AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS PBR_Submission,
    '""' + ISNULL(REPLACE(CAST(UPL_Submission AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS UPL_Submission,
    '""' + ISNULL(REPLACE(CAST(Contract_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Contract_Num,
    '""' + ISNULL(REPLACE(CAST(Task_Order_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Task_Order_Num,
    '""' + ISNULL(REPLACE(CAST(Award_Target_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Award_Target_Date,
    '""' + ISNULL(REPLACE(CAST(POP_Exp_Date AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS POP_Exp_Date,
    '""' + ISNULL(REPLACE(CAST(Contractor_ManYear_Equiv_CME AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Contractor_ManYear_Equiv_CME,
    '""' + ISNULL(REPLACE(CAST(COR_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS COR_Email,
    '""' + ISNULL(REPLACE(CAST(POC_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS POC_Email,
    '""' + ISNULL(REPLACE(CAST(Directorate AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Directorate,
    '""' + ISNULL(REPLACE(CAST(Div AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Div,
    '""' + ISNULL(REPLACE(CAST(Branch AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Branch,
    '""' + ISNULL(REPLACE(CAST(Review_POC_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Review_POC_Email,
    '""' + ISNULL(REPLACE(CAST(MDEP_Functional_Email AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS MDEP_Functional_Email,
    '""' + ISNULL(REPLACE(CAST(Notification_List_Emails AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Notification_List_Emails,
    '""' + ISNULL(REPLACE(CAST(Gen_Comments_Notes AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Gen_Comments_Notes,
    '""' + ISNULL(REPLACE(CAST(JUON AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS JUON,
    '""' + ISNULL(REPLACE(CAST(ISR_Flag AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS ISR_Flag,
    '""' + ISNULL(REPLACE(CAST(Cost_Model AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Model,
    '""' + ISNULL(REPLACE(CAST(Combat_Loss AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Combat_Loss,
    '""' + ISNULL(REPLACE(CAST(Cost_Location AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cost_Location,
    '""' + ISNULL(REPLACE(CAST(Cat_A_Code AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cat_A_Code,
    '""' + ISNULL(REPLACE(CAST(CBS_Code AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS CBS_Code,
    '""' + ISNULL(REPLACE(CAST(MIP_Proj_Code AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS MIP_Proj_Code,
    '""' + ISNULL(REPLACE(CAST(SS_Priority AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Priority,
    '""' + ISNULL(REPLACE(CAST(Commit_Group AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Commit_Group,
    '""' + ISNULL(REPLACE(CAST(SS_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Cap,
    '""' + ISNULL(REPLACE(CAST(Strategic_BIN AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Strategic_BIN,
    '""' + ISNULL(REPLACE(CAST(LIN AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS LIN,

    FY1_QTY, FY2_QTY, FY3_QTY, FY4_QTY, FY5_QTY,

    '""' + ISNULL(REPLACE(CAST(REQ_Type AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS REQ_Type,
    '""' + ISNULL(REPLACE(CAST(DD_Priority AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS DD_Priority,
    '""' + ISNULL(REPLACE(CAST(Portfolio AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Portfolio,
    '""' + ISNULL(REPLACE(CAST(DD_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS DD_Cap,
    '""' + ISNULL(REPLACE(CAST(JNT_Cap_Area AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS JNT_Cap_Area,
    '""' + ISNULL(REPLACE(CAST(TBM_Cost_Pool AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS TBM_Cost_Pool,
    '""' + ISNULL(REPLACE(CAST(TBM_Tower AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS TBM_Tower,
    '""' + ISNULL(REPLACE(CAST(APMS_AITR_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS APMS_AITR_Num,
    '""' + ISNULL(REPLACE(CAST(Zero_Trust_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Zero_Trust_Cap,
    '""' + ISNULL(REPLACE(CAST(Assoc_Directives AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Assoc_Directives,
    '""' + ISNULL(REPLACE(CAST(Cloud_IND AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cloud_IND,
    '""' + ISNULL(REPLACE(CAST(Strat_Cyber_Sec_PGM AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Strat_Cyber_Sec_PGM,
    '""' + ISNULL(REPLACE(CAST(Notes AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Notes,
    '""' + ISNULL(REPLACE(CAST(Unit_of_Measure AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Unit_of_Measure,

    Manpower_FY_1, Manpower_FY_1_Cost,
    Manpower_FY_2, Manpower_FY_2_Cost,
    Manpower_FY_3, Manpower_FY_3_Cost,
    Manpower_FY_4, Manpower_FY_4_Cost,
    Manpower_FY_5, Manpower_FY_5_Cost
FROM AggregatedData;"

		Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "ExportAllReqs")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
	    
	

		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region


	End Class
End Namespace