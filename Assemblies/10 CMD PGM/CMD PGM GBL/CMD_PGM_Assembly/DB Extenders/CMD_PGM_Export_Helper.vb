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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.CMD_PGM_Export_Helper
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
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Dashboard Component Selection Changed logic here.
							
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
						End If



#Region "Export All Requirements"
						'Export PGM Requirements to Excel
						If args.FunctionName.XFEqualsIgnoreCase("ExportAllREQs") Then
							Try								
								Return Me.ExportAllREQs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region
#Region "Export"
						'Export PGM Requirement Data to be used as import
						If args.FunctionName.XFEqualsIgnoreCase("ExportReport") Then
							Try								
								Return Me.ExportReport(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region
#Region "Export For Update"
						'Export PGM Requirements to Excel
						If args.FunctionName.XFEqualsIgnoreCase("ExportforUpdate") Then
							Try								
								Return Me.ExportforUpdate(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

#Region "Set Default PEG"
						'Set Default PEG for Requirements Export
						If args.FunctionName.XFEqualsIgnoreCase("SetDefaultPEG") Then	
							 Return Me.SetDefaultPEG(si,globals,api,args)
						End If
#End Region



				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "ExportReport"	
		'Export PGM Requirement Data to be used as import 
		Public Function ExportReport(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)		
			
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Select Case sTemplate.ToUpper
			Case "CWORK KEY5"
				Return Me.ExportReport_cWorkKey5(si,globals,api,args)
			Case "CWORK KEY15 OOC"
				Return Me.ExportReport_cWorkKey15(si,globals,api,args)
			Case "CSUSTAIN NO LINS"
				Return Me.ExportReport_cSustain(si,globals,api,args)
			Case "CDIGITAL"
				Return Me.ExportReport_cDIGITAL(si,globals,api,args)
			Case "CSUSTAIN DMOPS"
				Return Me.ExportReport_DMOPS(si,globals,api,args)
			Case "ALL REQUIREMENTS"
				Return Me.ExportReport_General(si,globals,api,args)
			End Select
			Return Nothing
	
		End Function
	
#End Region

#Region "ExportReport_cWork Key5"	
		'Export PGM Requirement Data to be used as import using cWork template 
	Public Function ExportReport_cWorkKey5(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))	
			Dim iScenariofstyr As Integer = iTime -4
			Dim sAccount As String = "Req_Funding"
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim exportScenario As String = "POM" & iTime.ToString.Substring(iTime.ToString.Length - 2) & "-BASE"
			Dim Issuecode As String = sROC.Substring(0,2) & "RS1000"
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			Dim yearCols(8) As Integer
    			For i = 0 To 8
        		yearCols(i) = iScenariofstyr + i
    			Next

			

			'Declare variables to fetch data
			Dim filterClause As String = ""
	
				' Get & Loop through different U1 APPN members and write
				'No Selected PEG & MDEP = Get all MDEPs
'				If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
'					Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
'					For Each oU1 As MemberInfo In oU1List
						
					'Next			
				
				'No Selected PEG & Select MDEP = Get selected MDEPs
				'Selected PEG & Selected MDEP = Get selected MDEPs
				 If Not String.IsNullOrWhiteSpace(sMDEP) Then			
					Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
					
						Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
'						Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
'						For Each oU1 As MemberInfo In oU1List
							
							
'						Next						
								
				
				'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
				Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then	
					Dim allMDEPs As New List(Of String)()
					Dim arrPEG As String() = sPEG.Split(",")
					For Each PEG As String In arrPEG
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
						
								For Each oU2 As MemberInfo In oU2List
   							 allMDEPs.Add($"'{oU2.Member.Name.ToString()}'")
								Next
							Next 
						filterClause = $"AND Req.MDEP IN ({String.Join(", ", allMDEPs)})"
						
'							Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
'							For Each oU1 As MemberInfo In oU1List
							
'							Next	
					
					'Next
				End If
	Dim sql As String =$"
        SELECT 
            Req.MDEP,
            Req.APPN,
            Req.APE9,
            Req.Dollar_Type,
            Req.CType,
            Req.UIC,
            MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_1 ELSE 0 END) AS FY_Data_0,
            MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_2 ELSE 0 END) AS FY_Data_1,
            MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_3 ELSE 0 END) AS FY_Data_2,
            MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_4 ELSE 0 END) AS FY_Data_3,
            MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_5 ELSE 0 END) AS FY_Data_4
        FROM XFC_CMD_PGM_REQ Req
        LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
            ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
            AND Req.WFScenario_Name = Dtl.WFScenario_Name
            AND Req.WFCMD_Name = Dtl.WFCMD_Name
            AND Req.WFTime_Name = Dtl.WFTime_Name
        WHERE Req.WFScenario_Name = '{sScenario}'
    		And Req.WFCMD_Name = '{sCube}'
    		And Req.WFTime_Name = {iTime}
			AND Req.Status = 'L2_Final_PGM'
            {filterClause}
        GROUP BY
            Req.MDEP, Req.APPN, Req.APE9, Req.Dollar_Type, Req.CType, Req.UIC"
	
	
	
			Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "CWorkKey15OOC")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
Dim sFileHeader As String = $"SCENARIO,NAME,REMARKS,JUSTIFICATION,ISSUE,MDEP,APPN,APE,ROC,DOLLAR_TYPE,BO,RC,CTYPE,UIC,REIMS,REIMC,FSC,FY{yearCols(0)},FY{yearCols(1)},FY{yearCols(2)},FY{yearCols(3)},FY{yearCols(4)},FY{yearCols(5)},FY{yearCols(6)},FY{yearCols(7)},FY{yearCols(8)}"
    Dim dtFinal As New DataTable()
    For Each colName As String In sFileHeader.Split(","c)
        dtFinal.Columns.Add(colName)
    Next
    
    '#--- 6. Process Data and Calculate Deltas ---'
    'Get the cPROBE scenario name once before the loop
    Dim cProbePos As String = $"Cb#ARMY:E#Army:C#Local:S#{sScenario}:T#{iScenariofstyr}:V#Annotation:A#Var_Selected_Position:#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
    Dim cProbeScenario As String = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, cProbePos).DataCellEx.DataCellAnnotation

    For Each row As DataRow In dtAll.Rows
        Dim newRow As DataRow = dtFinal.NewRow()
        
        '-- Get UDim values from the current row
        Dim U1_Appn As String = row("APPN").ToString()
        Dim U2_Mdep As String = row("MDEP").ToString()
        Dim U3_Ape As String = row("APE9").ToString()
        Dim U4_DollarType As String = row("Dollar_Type").ToString()

        '-- Populate fixed columns
        newRow("SCENARIO") = exportScenario
        newRow("APPN") = U1_Appn
        newRow("MDEP") = U2_Mdep
        newRow("APE") = U3_Ape
        newRow("ROC") = sROC
        newRow("DOLLAR_TYPE") = U4_DollarType
        newRow("BO") = "R"
        newRow("RC") = U1_Appn
        newRow("CTYPE") = row("CType").ToString()
        newRow("UIC") = row("UIC").ToString()
        newRow("Name") = "RMW_" & issueCode
        newRow("Justification") = "RMW snchronization"
        newRow("Issue") = issueCode
        newRow("Remarks") = "Command input for BO R data call"

        '-- Get SQL data for the first 5 years
        Dim sqlAmounts(4) As Decimal
        For i = 0 To 4
            sqlAmounts(i) = Convert.ToDecimal(row($"FY_Data_{i}"))
        Next

        '-- Loop through all 9 years for the output file
        For i As Integer = 0 To 8
            Dim cProbeAMT As Decimal = 0
            Dim reqAmount As Decimal = 0
            
            '-- Get cPROBE Data for the current year/intersection
            Dim sSrcMbrScript As String = $"Cb#ARMY:S#{cProbeScenario}:T#{yearCols(i)}:C#Local:E#{sROC}:V#Periodic:A#BOR:F#Baseline:I#Top:O#BeforeAdj:U1#{U1_Appn}:U2#{U2_Mdep}:U3#{U3_Ape}:U4#{U4_DollarType}:U5#Top:U6#None:U7#Top:U8#Top"
            cProbeAMT = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sSrcMbrScript).DataCellEx.DataCell.CellAmount
            
            '-- Use SQL data for the first 5 years; otherwise, it's 0
            If i < sqlAmounts.Length Then
                reqAmount = sqlAmounts(i)
            End If
            
            '-- Calculate and round the delta
            Dim cWorkDelta As Decimal = Math.Round((reqAmount - cProbeAMT) / 1000, 0)
            newRow($"FY{yearCols(i)}") = cWorkDelta
        Next
        
        dtFinal.Rows.Add(newRow)
    Next

    '#--- 7. Generate File ---'
    Return Me.GenerateReportFile(si, dtFinal, sFileHeader, sCube, iTime, sTemplate, sFvParam)
End Function
#End Region

#Region "ExportReport_cWorkKey15/OOC"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cWorkKey15(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			'Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_Funding"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim exportScenario As String = "POM" & iTime.ToString.Substring(iTime.ToString.Length - 2) & "-BASE"
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
		
			Dim sFileHeader As String = ""
			Dim filterClause As String = ""
			Dim Issuecode As String = sROC.Substring(0,2) & "RS1000"
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	

				sFileHeader = $"SCENARIO,ISSUECODE,BO,REQUIREMENT_TITLE,REQUIREMENT_DESCRIPTION,REMARKS,MDEP,APPN,APE,ROC,DOLLAR_TYPE,JUON,ISR_FLAG,COST_MODEL,COMBAT_LOSS,COST_LOCATION,CATEGORY_A_CODE,CBS_CODE,MIP_PROJ_CODE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
							
			
						
							 

						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
						
								
									
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then	
					Dim allMDEPs As New List(Of String)()
					Dim arrPEG As String() = sPEG.Split(",")
					For Each PEG As String In arrPEG
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
						
								For Each oU2 As MemberInfo In oU2List
   							 allMDEPs.Add($"'{oU2.Member.Name.ToString()}'")
								Next
							Next 
						filterClause = $"AND Req.MDEP IN ({String.Join(", ", allMDEPs)})"
						
						
						End If	
Dim sql As String = $"SELECT '{exportScenario}' AS Scenario,'{Issuecode}' AS Issuecode,('R') AS BO,Req.Title, Req.Description,('Command input for OOC key15 data call') As Remarks,Req.MDEP,Req.APPN,Req.APE9,'{sROC}' As ROC,Req.Dollar_Type,Req.JUON,Req.ISR_Flag, Req.Cost_Model, Req.Combat_Loss, Req.Cost_Location, Req.Cat_A_Code, Req.CBS_Code, Req.MIP_Proj_Code,  
			                             
                			
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_1 ELSE 0 END) AS FY{iTime0},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_2 ELSE 0 END) AS FY{iTime1},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_3 ELSE 0 END) AS FY{iTime2},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_4 ELSE 0 END) AS FY{iTime3},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_5 ELSE 0 END) AS FY{iTime4}
	
								
                				FROM XFC_CMD_PGM_REQ Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								
							WHERE Req.WFScenario_Name = '{sScenario}'
    								And Req.WFCMD_Name = '{sCube}'
    								And Req.WFTime_Name = {iTime}
								AND Req.Status = 'L2_Final_PGM'
								And Req.Dollar_Type != 'Base'
								 {filterClause}
								GROUP BY
								    Req.Title, 
								    Req.Description,
								    Req.MDEP,
								    Req.APPN,
								    Req.APE9,
								    Req.Dollar_Type,
								    Req.JUON,
								    Req.ISR_Flag, 
								    Req.Cost_Model, 
								    Req.Combat_Loss, 
								    Req.Cost_Location, 
								    Req.Cat_A_Code, 
								    Req.CBS_Code, 
								    Req.MIP_Proj_Code
								"
											
			Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "CWorkKey15OOC")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				

			
			
		
					
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

			
			

		End Function
	
#End Region

#Region "ExportReport_cSustain NO LINS"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cSustain(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			'Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_Funding"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim exportScenario As String = "POM" & iTime.ToString.Substring(iTime.ToString.Length - 2) & "-BASE"
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			If Not sPEG.XFEqualsIgnoreCase("SS")
				Throw New Exception("Please select SS PEG for cSustain No LINS Export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			'Declare variables to fetch data
			
			Dim sFileHeader As String = ""
			Dim filterClause As String = ""
		

				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
							
						End If			
				
				
			'End If
			
Dim sql As String = $"
WITH AggregatedData AS ( 
SELECT '{exportScenario}' AS Scenario,('R') AS BO,Req.Title,Req.Gen_Comments_Notes, Req.Justification,
Req.SS_Priority,Req.MDEP,Req.APPN,Req.APE9,({sROC}) as ROC,Req.Dollar_Type,Req.Commit_Group, Req.SS_Cap, Req.Strategic_BIN,

	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_1 ELSE 0 END) AS FY{iTime0},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_2 ELSE 0 END) AS FY{iTime1},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_3 ELSE 0 END) AS FY{iTime2},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_4 ELSE 0 END) AS FY{iTime3},
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_5 ELSE 0 END) AS FY{iTime4}
	
								
                				FROM XFC_CMD_PGM_REQ Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								
							WHERE Req.WFScenario_Name = '{sScenario}'
    								And Req.WFCMD_Name = '{sCube}'
    								And Req.WFTime_Name = {iTime}
								AND Req.Status = 'L2_Final_PGM'
								 {filterClause}
							GROUP BY
							    Req.Title,
							    Req.Gen_Comments_Notes,
							    Req.Justification,
							    Req.SS_Priority,
							    Req.MDEP,
							    Req.APPN,
							    Req.APE9,
							    Req.Dollar_Type,
							    Req.Commit_Group,
							    Req.SS_Cap,
							    Req.Strategic_BIN
)
SELECT
   Scenario,BO
    '""' + ISNULL(REPLACE(CAST(Title AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Title,
    '""' + ISNULL(REPLACE(CAST(Gen_Comments_Notes AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Gen_Comments_Notes,
    '""' + ISNULL(REPLACE(CAST(Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Justification,
    '""' + ISNULL(REPLACE(CAST(SS_Priority AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Priority,
   	MDEP,APPN,APE9,ROC,Dollar_Type,
    '""' + ISNULL(REPLACE(CAST(Commit_Group AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Commit_Group,
    '""' + ISNULL(REPLACE(CAST(SS_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Cap,
    '""' + ISNULL(REPLACE(CAST(Strategic_BIN AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Strategic_BIN,
    FY{iTime0},
    FY{iTime1},
    FY{iTime2},
    FY{iTime3},
    FY{iTime4}
FROM AggregatedData;
"
			
			
			Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "CSustain")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				

			
			
		
					
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
	
#End Region

#Region "ExportReport_DMOPS"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_DMOPS(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			'Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")	
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_Funding"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim exportScenario As String = "POM" & iTime.ToString.Substring(iTime.ToString.Length - 2) & "-BASE"
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
				If Not sPEG.XFEqualsIgnoreCase("SS")
				Throw New Exception("Please select SS PEG for cSustain DMOPS Export")
			End If	
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			
		
			Dim sFileHeader As String = ""
			
		Dim filterClause As String = ""
				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,LIN,FY{iTime0}(D),FY{iTime0}(Q),FY{iTime1}(D),FY{iTime1}(Q),FY{iTime2}(D),FY{iTime2}(Q),FY{iTime3}(D),FY{iTime3}(Q),FY{iTime4}(D),FY{iTime4}(Q)"
			
						
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"

						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"

							
						End If			
Dim sql As String = $"WITH AggregatedData AS (

SELECT '{exportScenario}' AS Scenario,('R') AS BO,Req.Title,Req.Gen_Comments_Notes, Req.Justification,
Req.SS_Priority,Req.MDEP,Req.APPN,Req.APE9,({sROC}) as ROC,Req.Dollar_Type,Req.Commit_Group, Req.SS_Cap, Req.Strategic_BIN,Req.LIN,

	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_1 ELSE 0 END) AS FY{iTime0},
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_1 ELSE 0 END) AS FY1_QTY,
	
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_2 ELSE 0 END) AS FY{iTime1},
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_2 ELSE 0 END) AS FY2_QTY,
	
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_3 ELSE 0 END) AS FY{iTime2},
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_3 ELSE 0 END) AS FY3_QTY,
	
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_4 ELSE 0 END) AS FY{iTime3},
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_4 ELSE 0 END) AS FY4_QTY,
		
	MAX(Case When Dtl.Account = 'Req_Funding' THEN Dtl.FY_5 ELSE 0 END) AS FY{iTime4},
	MAX(Case When Dtl.Account = 'Quantity' THEN Dtl.FY_5 ELSE 0 END) AS FY5_QTY
								
                				FROM XFC_CMD_PGM_REQ Req
								LEFT JOIN XFC_CMD_PGM_REQ_Details AS Dtl
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								
							WHERE Req.WFScenario_Name = '{sScenario}'
    								And Req.WFCMD_Name = '{sCube}'
    								And Req.WFTime_Name = {iTime}
									AND Req.Status = 'L2_Final_PGM'
								 {filterClause}
							GROUP BY
							    Req.Title,
							    Req.Gen_Comments_Notes,
							    Req.Justification,
							    Req.SS_Priority,
							    Req.MDEP,
							    Req.APPN,
							    Req.APE9,
							    Req.Dollar_Type,
							    Req.Commit_Group,
							    Req.SS_Cap,
							    Req.Strategic_BIN,
							    Req.LIN
)
SELECT
   Scenario,BO,
    '""' + ISNULL(REPLACE(CAST(Title AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Title,
    '""' + ISNULL(REPLACE(CAST(Gen_Comments_Notes AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Gen_Comments_Notes,
    '""' + ISNULL(REPLACE(CAST(Justification AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Justification,
    '""' + ISNULL(REPLACE(CAST(SS_Priority AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Priority,
   MDEP,
    APPN,
    APE9,
    ROC,
    Dollar_Type,
    '""' + ISNULL(REPLACE(CAST(Commit_Group AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Commit_Group,
    '""' + ISNULL(REPLACE(CAST(SS_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS SS_Cap,
    '""' + ISNULL(REPLACE(CAST(Strategic_BIN AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Strategic_BIN,
    '""' + ISNULL(REPLACE(CAST(LIN AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS LIN,

   
    FY{iTime0},
    FY1_QTY,
    FY{iTime1},
    FY2_QTY,
    FY{iTime2},
    FY3_QTY,
    FY{iTime3},
    FY4_QTY,
    FY{iTime4},
    FY5_QTY
FROM AggregatedData;
"
			
			Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "DMOPS")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				

			
			
			'Generate & write File and update FvParam for filepath needed for file viewer
					
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

	


		End Function
	
#End Region

#Region "ExportReport_cDIGITAL"	
		'Export PGM Requirement Data to be used as import using General template 
		Public Function ExportReport_cDIGITAL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sROC As String = BRApi.Finance.Entity.Text(si, iMemberId, 2, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			'Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_Funding"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim exportScenario As String = "POM" & iTime.ToString.Substring(iTime.ToString.Length - 2) & "-BASE"
			
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			If Not sPEG.XFEqualsIgnoreCase("DD")
				Throw New Exception("Please select DD PEG for cDigital Export")
			End If	
			
			
'BRapi.ErrorLog.LogMessage(si,$"sTemplate: {sTemplate} || sReportType = {sReportType} || sPEG = {sPEG} || sMDEP = {sMDEP}")			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
			
			Dim sFileHeader As String = ""
			
			 Dim filterClause As String = ""

				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	
				
				sFileHeader = $"SCENARIO,RQMT TYPE,RQMT SHORT TITLE,RQMT DESCRIPTION,BO,MDEP,APPN,APE,ROC,SUBCMD,DOLLAR TYPE,CTYPE,EMERGING RQMT?,APMS AITR #,PRIORITY,PORTFOLIO,CAPABILITY,JNT CAP AREA,TBM COST POOL,TBM TOWER,ZERO TRUST CAPABILITY,ASSOCIATED DIRECTIVES,CLOUD INDICATOR,STRAT CYBERSEC PGRM,NOTES,UNIT OF MEASURE,FY{iTime0} # ITEMS,FY{iTime0} # UNIT COST,FY{iTime1} # ITEMS,FY{iTime1} # UNIT COST,FY{iTime2} # ITEMS,FY{iTime2} # UNIT COST,FY{iTime3} # ITEMS,FY{iTime3} # UNIT COST,FY{iTime4} # ITEMS,FY{iTime4} # UNIT COST"
							
				
						
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"

						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String ="DD"
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
						End If			
				
			
			'End If
		
'Fill the DataTable 
Dim sql As String =$"WITH AggregatedData AS (
Select '{exportScenario}' AS Scenario, Req.REQ_Type, Req.Title, Req.Description,('R') as BO, Req.MDEP,Req.APPN,Req.APE9,({sROC}) as ROC,
Req.Entity, Req.Dollar_Type, Req.CType, Req.Emerging_REQ, Req.APMS_AITR_Num,Req.DD_Priority, Req.Portfolio, Req.DD_Cap, Req.JNT_Cap_Area, Req.TBM_Cost_Pool, Req.TBM_Tower, 
Req.Zero_Trust_Cap, Req.Assoc_Directives, Req.Cloud_IND, Req.Strat_Cyber_Sec_PGM,Req.Notes,
										
								  MAX(CASE WHEN Dtl.Unit_of_Measure != 'Funding' and Dtl.Account != 'Quantity' THEN Dtl.Unit_of_Measure ELSE Null END)as Unit_of_Measure,
									
									  
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_1 ELSE Null END) AS FY_1,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_1 ELSE Null END) AS FY_1_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_2 ELSE Null END) AS FY_2,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_2 ELSE Null END) AS FY_2_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_3 ELSE Null END) AS FY_3,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_3 ELSE Null END) AS FY_3_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_4 ELSE Null END) AS FY_4,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_4 ELSE Null END) AS FY_4_Cost,
									    
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Item' THEN Dtl.FY_5 ELSE Null END) AS FY_5,
									    MAX(CASE WHEN Dtl.Account = 'DD_PEG_Cost' THEN Dtl.FY_5 ELSE Null END) AS FY_5_Cost
									
							FROM 
								XFC_CMD_PGM_REQ Req
							LEFT JOIN 
								XFC_CMD_PGM_REQ_Details AS Dtl
							ON 
								Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
							WHERE 
									Req.WFScenario_Name = '{sScenario}'
    								And Req.WFCMD_Name = '{sCube}'
    								And Req.WFTime_Name = {iTime}
								 {filterClause}
								
								AND  Dtl.Unit_of_Measure != 'Funding' 
								AND Dtl.Account != 'Quantity'
								AND Req.Status = 'L2_Final_PGM'
							GROUP BY
								Req.REQ_Type,
								Req.Title,
								Req.Description,
								Req.MDEP,
								Req.APPN,
								Req.APE9,
								Req.Entity,
								Req.Dollar_Type,
								Req.CType,
								Req.Emerging_REQ,
								Req.APMS_AITR_Num,
								Req.DD_Priority,
								Req.Portfolio,
								Req.DD_Cap,
								Req.JNT_Cap_Area,
								Req.TBM_Cost_Pool,
								Req.TBM_Tower,
								Req.Zero_Trust_Cap,
								Req.Assoc_Directives,
								Req.Cloud_IND,
								Req.Strat_Cyber_Sec_PGM,
								Req.Notes,
								Dtl.Unit_of_Measure
)
		SELECT Scenario,REQ_Type,
    '""' + ISNULL(REPLACE(CAST(Title AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Title,
    '""' + ISNULL(REPLACE(CAST(Description AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Description,
    BO,MDEP,APPN,APE9,ROC,Entity,Dollar_Type,CType,
    '""' + ISNULL(REPLACE(CAST(Emerging_REQ AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Emerging_REQ,
    '""' + ISNULL(REPLACE(CAST(APMS_AITR_Num AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS APMS_AITR_Num,
    '""' + ISNULL(REPLACE(CAST(DD_Priority AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS DD_Priority,
    '""' + ISNULL(REPLACE(CAST(Portfolio AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Portfolio,
    '""' + ISNULL(REPLACE(CAST(DD_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS DD_Cap,
    '""' + ISNULL(REPLACE(CAST(JNT_Cap_Area AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS JNT_Cap_Area,
    '""' + ISNULL(REPLACE(CAST(TBM_Cost_Pool AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS TBM_Cost_Pool,
    '""' + ISNULL(REPLACE(CAST(TBM_Tower AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS TBM_Tower,
    '""' + ISNULL(REPLACE(CAST(Zero_Trust_Cap AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Zero_Trust_Cap,
    '""' + ISNULL(REPLACE(CAST(Assoc_Directives AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Assoc_Directives,
    '""' + ISNULL(REPLACE(CAST(Cloud_IND AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Cloud_IND,
    '""' + ISNULL(REPLACE(CAST(Strat_Cyber_Sec_PGM AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Strat_Cyber_Sec_PGM,
    '""' + ISNULL(REPLACE(CAST(Notes AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Notes,
    '""' + ISNULL(REPLACE(CAST(Unit_of_Measure AS NVARCHAR(MAX)), '""', '""""'), '') + '""' AS Unit_of_Measure,
    FY_1,
    FY_1_Cost,
    FY_2,
    FY_2_Cost,
    FY_3,
    FY_3_Cost,
    FY_4,
    FY_4_Cost,
    FY_5,
    FY_5_Cost
FROM AggregatedData;
"						
				
		Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "CDigital")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function


#End Region

#Region "ExportReport_General(All Requirements PEG export dashboard) "	
		'Export PGM Requirement Data for all fields 
		Public Function ExportReport_General(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")		
			Dim sCube As String = args.NameValuePairs.XFGetValue("CMD","")			
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim sEntity As String = BRApi.Finance.Entity.Text(si, iMemberId, 1, -1, -1)
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sScenario As String = args.NameValuePairs.XFGetValue("Scenario","")	
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))	
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "Req_Funding"
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			sEntity = sEntity.Replace("""","")		
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			
			Dim sFileHeader As String = ""
					
			
	sFileHeader = $"Entity,REQ_ID,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"	

	 '---------------------------------------------------
    Dim filterClause As String = ""
			
						
						'No Selected PEG & MDEP = Get all MDEPs
						
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							
							Dim MDEPList As New List(Of String)()
								For Each oU2 As MemberInfo In oU2List
   							 MDEPList.Add($"'{oU2.Member.Name.ToString()}'")
								Next

						filterClause  = $"AND Req.MDEP IN ({String.Join(", ", MDEPList)})"
										
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
								Dim allMDEPs As New List(Of String)()
					Dim arrPEG As String() = sPEG.Split(",")
					For Each PEG As String In arrPEG
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
						
								For Each oU2 As MemberInfo In oU2List
   							 allMDEPs.Add($"'{oU2.Member.Name.ToString()}'")
								Next
							Next 
						filterClause = $"AND Req.MDEP IN ({String.Join(", ", allMDEPs)})"
						End If			
					
				Dim sql As String = $"
				With AggregatedData As (
Select
    Req.Entity,Req.REQ_ID,Req.Status,Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,
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
	MAX(Case When Dtl.Unit_of_Measure != 'Funding' and Dtl.Account != 'Quantity' THEN Dtl.Unit_of_Measure ELSE Null END)as Unit_of_Measure,
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
    Req.WFScenario_Name = '{sScenario}'
    And Req.WFCMD_Name = '{sCube}'
    And Req.WFTime_Name = {iTime}
	{filterClause} 

Group By
Req.REQ_ID, Req.Title,
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
	Select
    Entity, REQ_ID,Status,APPN,MDEP,APE9, Dollar_Type, Obj_Class, CType,
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
From AggregatedData;"
				
		Dim dtAll As DataTable = Me.CreateNameValuePairTable(si, "ExportAllReqs")
		
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dtAll = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
				End Using
				
	    
	

		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, dtAll, sFileHeader, sCube, iTime, sTemplate,sFvParam)

			

				
			
		End Function
	
#End Region


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

				'Check if folder doesn't exist
				'This should never happen because we created the folder manually
				'If objXFFolderEx Is Nothing Then
				'	Throw New XFUserMsgException(si, New Exception("Users/" & si.UserName.Replace(" ",String.Empty) & " folder does NOT exist"))
				'End If
				
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
		
		


#Region "ExportReport - Set Default PEG"
			'Set Default PEG for Requirements Export
			Public Function	SetDefaultPEG(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs)
				Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template")
				Dim sParam As String = args.NameValuePairs.XFGetValue("Param")
				Dim sPEG As String = ""
				If sTemplate.XFContainsIgnoreCase("cSustain") Or sTemplate.XFContainsIgnoreCase("DMOPS")
					sPEG = "SS"
				Else If sTemplate.XFContainsIgnoreCase("cDigital")
					sPEG = "DD"
				End If
				Dim dKeyVal As New Dictionary(Of String, String)				
				dKeyVal.Add(sParam,sPEG)		
				Return Me.SetParameter(si, globals, api, dKeyVal)				
		
			End Function
#End Region



#Region "ExportAllREQs(Review Dashboard)"	
		'Export PGM Requirement Data
		Public Function ExportAllREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim sFundCenter As String = args.NameValuePairs.XFGetValue("Entity")
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sFundCenter)
			
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
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
					FC = FC.Replace("_General","").Trim
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
						Next	
						
							Dim allFCs As String = ""
				
					
				allFCs = FCMulti.ToString()
		'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			
			
			'Declare variables to fetch data
		
			Dim sFileHeader As String = ""
					
			
	sFileHeader = $"Entity,REQ_ID,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"	

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
    Req.Entity,Req.REQ_ID,Req.Status,Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,
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
	{filterClause} 

Group by
Req.REQ_ID, Req.Title,
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
    Entity, REQ_ID,Status,APPN,MDEP,APE9, Dollar_Type, Obj_Class, CType,
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

#Region "ExportforUpdate(ImportDashboard)"	
		'Export PGM Requirement Data
		Public Function ExportforUpdate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
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
'					'Brapi.ErrorLog.LogMessage(si,"Here 2.4")	
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
					
			
	sFileHeader = $"FundsCenter,REQ_ID,Status,APPN,MDEP,APE9,DOLLARTYPE,OBJECTCLASS,CTYPE,Cycle,FY1,FY2,FY3,FY4,FY5,Title,Description,Justification,CostMethodology,ImpactIfNotFunded,RiskIfNotFunded,CostGrowthJustification,MustFund,FundingSource,ArmyInitiative_Directive,CommandInitiative_Directive,Activity_Exercise,IT_CyberRequirement,UIC,Flex1,Flex2,Flex3,Flex4,Flex5,EmergingRequirement_ER,CPATopic,PBRSubmission,UPLSubmission,ContractNumber,TaskOrderNumber,AwardTargetDate,POPExpirationDate,ContractManYearEquiv_CME,COREmail,POCEmail,Directorate,Division,Branch,ReviewingPOCEmail,MDEPFunctionalEmail,NotificationEmailList,GeneralComments_Notes,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMSAITRNUM,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"	

	 '---------------------------------------------------
    Dim filterClause As String = ""
        
        filterClause = $"AND Req.Entity IN ({allFCs})"
'Fill the DataTable 
Dim sql As String = $"
WITH AggregatedData As (
Select
    Req.Entity,Req.REQ_ID,Status,Req.APPN, Req.MDEP, Req.APE9, Req.Dollar_Type, Req.Obj_Class, Req.CType,Req.WFTime_Name,
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
Req.REQ_ID,Status, Req.Title,Req.WFTime_Name,
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
    Entity, REQ_ID,Status, APPN, MDEP, SUBSTRING(APE9, LEN(APE9) - 8, 9), Dollar_Type, Obj_Class, CType,WFTime_Name,
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