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
#Region "Export All Updated Requirements"
						'Export PGM Requirements to Excel
						If args.FunctionName.XFEqualsIgnoreCase("ExportAllUpdatedREQs") Then
							Try								
								Return Me.ExportAllUpdatedREQs(si,globals,api,args)
								Catch ex As Exception
								Throw ErrorHandler.LogWrite(si, New XFException(si,ex))
							End Try
						End If
#End Region

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
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			If String.IsNullOrWhiteSpace(sEntity) 
				Throw New Exception("Please select a Command to export")
			End If	
			
			Dim iTime0 As Integer = iScenariofstyr + 0
			Dim iTime1 As Integer = iScenariofstyr + 1
			Dim iTime2 As Integer = iScenariofstyr + 2
			Dim iTime3 As Integer = iScenariofstyr + 3
			Dim iTime4 As Integer = iScenariofstyr + 4
			Dim iTime_1 As Integer = iScenariofstyr + 5
			Dim iTime_2 As Integer = iScenariofstyr + 6
			Dim iTime_3 As Integer = iScenariofstyr + 7 
			Dim iTime_4 As Integer = iScenariofstyr + 8 
			sEntity = sEntity.Replace("""","")		

			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			processColumns.AddRange(New String(){"SCENARIO","NAME","REMARKS","JUSTIFICATION","ISSUE","MDEP","APPN","APE","ROC","DOLLAR_TYPE","BO","RC","CTYPE","UIC","REIMS","REIMC","FSC",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",$"FY{iTime_1}",$"FY{iTime_2}",$"FY{iTime_3}",$"FY{iTime_4}"})
			sFileHeader = $"SCENARIO,NAME,REMARKS,JUSTIFICATION,ISSUE,MDEP,APPN,APE,ROC,DOLLAR_TYPE,BO,RC,CTYPE,UIC,REIMS,REIMC,FSC,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},FY{iTime_1},FY{iTime_2},FY{iTime_3},FY{iTime_4}"
				
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	For i As Integer = 0 To 8
				Dim myDataUnitPk As New DataUnitPk( _
				BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
				DimConstants.Local, _
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
				BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iScenariofstyr + i).ToString))

				' Buffer coordinates.
				' Default to #All for everything, then set IDs where we need it.
				Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
				myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
				myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
				myDbCellPk.OriginId = DimConstants.BeforeAdj
				myDbCellPk.ICId = DimConstants.Top
				myDbCellPk.UD6Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD6, "Top")		
				myDbCellPk.UD7Id = DimConstants.None
				myDbCellPk.UD8Id = DimConstants.None					
				' Get & Loop through different U1 APPN members and write
				'No Selected PEG & MDEP = Get all MDEPs
				If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
					Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
					For Each oU1 As MemberInfo In oU1List
						myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
						' parameters: si, DataUnitPK, viewID, CommonMembersCellPk, includeUDAttributes, suppressNoData
						Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
						If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
					Next			
				
				'No Selected PEG & Select MDEP = Get selected MDEPs
				'Selected PEG & Selected MDEP = Get selected MDEPs
				Else If Not String.IsNullOrWhiteSpace(sMDEP) Then			
					Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
					For Each oU2 As MemberInfo In oU2List
						myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
						Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
						For Each oU1 As MemberInfo In oU1List
							myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
						Next						
					Next				
				
				'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
				Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then	
					Dim arrPEG As String() = sPEG.Split(",")
					For Each PEG As String In arrPEG
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim oU1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_L2"), "U1#Appropriation.Base", True,,)
							For Each oU1 As MemberInfo In oU1List
								myDbCellPk.UD1Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD1, oU1.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iScenariofstyr + i).ToString,myCells)
							Next	
						Next	
					Next
				End If
			Next	
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("firstYr",iScenariofstyr.ToString)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("Scenario",sScenario)
			dArgs.Add("Cube",sCube)
			
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

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
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
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
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	
				processColumns.AddRange(New String(){"SCENARIO","ISSUECODE","BO","RQMT TITLE","RQMT DESCRIPTION","REMARKS","MDEP","APPN","APE","ROC","DOLLAR TYPE","JUON","ISR FLAG","COST MODEL","COMBAT LOSS","COST LOCATION","CATEGORY A CODE","CBS CODE","MIP PROJ CODE",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,ISSUECODE,BO,REQUIREMENT_TITLE,REQUIREMENT_DESCRIPTION,REMARKS,MDEP,APPN,APE,ROC,DOLLAR_TYPE,JUON,ISR_FLAG,COST_MODEL,COMBAT_LOSS,COST_LOCATION,CATEGORY_A_CODE,CBS_CODE,MIP_PROJ_CODE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
							
				For Each Entity As Member In lsEntity
					'For i As Integer = 0 To 4 Step 1 
					'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
					For i As Integer =  0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						Dim oU4List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U4_DollarType"), $"U4#Dollar_Type.Base.Where(Name DoesNotContain 'BASE')", True,,)		
								
						For Each oU4 As MemberInfo In oU4List
									myDbCellPk.UD4Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD4, oU4.Member.Name)
						
						
						'No Selected PEG & MDEP = Get all MDEPs
						If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)

						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						Else If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String() = sPEG.Split(",")
							For Each PEG As String In arrPEG
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							Next
						
						End If	
						Next
					Next
				Next
			
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

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
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
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
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

			If sReportType.XFContainsIgnoreCase("Summary") Then	
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
				For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
					
					'No Selected PEG & Select MDEP = Get selected MDEPs
					'Selected PEG & Selected MDEP = Get selected MDEPs
					 If Not String.IsNullOrWhiteSpace(sMDEP) Then
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
						Next				
					
					'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
					Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
						Dim arrPEG As String = "SS"
						
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
							Next	
						
					End If			
				Next
				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			Else If sReportType.XFContainsIgnoreCase("Detail") Then
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4}"
				
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						
						'
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							
						End If			
					Next
				Next
			End If
			

			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

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
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
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
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","BO","NAME","REMARKS","JUSTIFICATION","FUNCTIONAL PRIORITY","MDEP","APPN","APE","ROC","DOLLAR TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN","LIN",$"FY{iTime0}",$"FY1_QTY",$"FY{iTime1}",$"FY2_QTY",$"FY{iTime2}",$"FY3_QTY",$"FY{iTime3}",$"FY4_QTY",$"FY{iTime4}",$"FY5_QTY"})
				sFileHeader = $"SCENARIO,BO,NAME,REMARKS,JUSTIFICATION,FUNCTIONAL PRIORITY,MDEP,APPN,APE,ROC,DOLLAR TYPE,COMMITMENT GROUP,CAPABILITY,STRATEGIC BIN,LIN,FY{iTime0}(D),FY{iTime0}(Q),FY{iTime1}(D),FY{iTime1}(Q),FY{iTime2}(D),FY{iTime2}(Q),FY{iTime3}(D),FY{iTime3}(Q),FY{iTime4}(D),FY{iTime4}(Q)"
				
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String = "SS"
							
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							
						End If			
					Next
				Next
			
			

			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

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
			Dim sReportType As String = args.NameValuePairs.XFGetValue("ReportType","")			
			Dim sPEG As String = args.NameValuePairs.XFGetValue("PEGFilter","")
			Dim sMDEP As String = args.NameValuePairs.XFGetValue("MDEPFilter","")	
			Dim sAccount As String = "REQ_CMD_App_Req_Amt_ADM"
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
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
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
#Region "CDIGITAL Summary"
			If sReportType.XFContainsIgnoreCase("Summary") Then	
				processColumns.AddRange(New String(){"SCENARIO","RQMT TYPE","RQMT SHORT TITLE","RQMT DESCRIPTION","BO","MDEP","APPN","APE","ROC","SUBCMD","DOLLAR TYPE","CTYPE","EMERGING RQMT?","APMS AITR #","PRIORITY","PORTFOLIO","CAPABILITY","JNT CAP AREA,","TBM COST POOL","TBM TOWER","ZERO TRUST CAPABILITY","ASSOCIATED DIRECTIVES","CLOUD INDICATOR","STRAT CYBERSEC PGRM","NOTES","BOR ID","BO7 ID","BO1 ID","UNIT OF MEASURE",$"FY{iTime0} # ITEMS",$"FY{iTime0} # UNIT COST",$"FY{iTime1} # ITEMS",$"FY{iTime1} # UNIT COST",$"FY{iTime2} # ITEMS",$"FY{iTime2} # UNIT COST",$"FY{iTime3} # ITEMS",$"FY{iTime3} # UNIT COST",$"FY{iTime4} # ITEMS",$"FY{iTime4} # UNIT COST"})
				sFileHeader = $"SCENARIO,RQMT TYPE,RQMT SHORT TITLE,RQMT DESCRIPTION,BO,MDEP,APPN,APE,ROC,SUBCMD,DOLLAR TYPE,CTYPE,EMERGING RQMT?,APMS AITR #,PRIORITY,PORTFOLIO,CAPABILITY,JNT CAP AREA,,TBM COST POOL,TBM TOWER,ZERO TRUST CAPABILITY,ASSOCIATED DIRECTIVES,CLOUD INDICATOR,STRAT CYBERSEC PGRM,NOTES,BOR ID,BO7 ID,BO1 ID,UNIT OF MEASURE,FY{iTime0} # ITEMS,FY{iTime0} # UNIT COST,FY{iTime1} # ITEMS,FY{iTime1} # UNIT COST,FY{iTime2} # ITEMS,FY{iTime2} # UNIT COST,FY{iTime3} # ITEMS,FY{iTime3} # UNIT COST,FY{iTime4} # ITEMS,FY{iTime4} # UNIT COST"
				'For i As Integer = 0 To 4 Step 1 
				'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
				For  i As Integer = 0 To 0 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, sEntity ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.FlowId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Flow, "Top")
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.ICId = DimConstants.Top
					'myDbCellPk.UD5Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD5, "Top")		
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
				
					'No selected PEG & selected MDEP = Get selected MDEPs
					'Selected PEG & selected MDEP = Get selected MDEPs
					 If Not String.IsNullOrWhiteSpace(sMDEP) Then
						Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
						For Each oU2 As MemberInfo In oU2List
							myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
						Next				
					
					'Selected PEG & No selected MDEP = Get all MDEPs under selected PEG
					Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
						Dim arrPEG As String = "DD"
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,sEntity,sScenario,(iTime + i).ToString,myCells)
							Next	
						
					End If			
				Next
#End Region				
			'For Detail Report, need to loop through all Base Entities and 5 years per Entity
			Else If sReportType.XFContainsIgnoreCase("Detail") Then
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)	
				processColumns.AddRange(New String(){"SCENARIO","RQMT TYPE","RQMT SHORT TITLE","RQMT DESCRIPTION","BO","MDEP","APPN","APE","ROC","SUBCMD","DOLLAR TYPE","CTYPE","EMERGING RQMT?","APMS AITR #","PRIORITY","PORTFOLIO","CAPABILITY","JNT CAP AREA","TBM COST POOL","TBM TOWER","ZERO TRUST CAPABILITY","ASSOCIATED DIRECTIVES","CLOUD INDICATOR","STRAT CYBERSEC PGRM","NOTES","UNIT OF MEASURE",$"FY1 # ITEMS",$"FY1 # UNIT COST",$"FY2 # ITEMS",$"FY2 # UNIT COST",$"FY3 # ITEMS",$"FY3 # UNIT COST",$"FY4 # ITEMS",$"FY4 # UNIT COST",$"FY5 # ITEMS",$"FY5 # UNIT COST"})
				sFileHeader = $"SCENARIO,RQMT TYPE,RQMT SHORT TITLE,RQMT DESCRIPTION,BO,MDEP,APPN,APE,ROC,SUBCMD,DOLLAR TYPE,CTYPE,EMERGING RQMT?,APMS AITR #,PRIORITY,PORTFOLIO,CAPABILITY,JNT CAP AREA,TBM COST POOL,TBM TOWER,ZERO TRUST CAPABILITY,ASSOCIATED DIRECTIVES,CLOUD INDICATOR,STRAT CYBERSEC PGRM,NOTES,UNIT OF MEASURE,FY{iTime0} # ITEMS,FY{iTime0} # UNIT COST,FY{iTime1} # ITEMS,FY{iTime1} # UNIT COST,FY{iTime2} # ITEMS,FY{iTime2} # UNIT COST,FY{iTime3} # ITEMS,FY{iTime3} # UNIT COST,FY{iTime4} # ITEMS,FY{iTime4} # UNIT COST"
							
				For Each Entity As Member In lsEntity
					'For i As Integer = 0 To 4 Step 1 
					'DD PEG Template does not need to iterate through 4 years (11/06/2025 - Fronz)
					For i As Integer = 0 To 0 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						
						'Selected PEG & Selected MDEP = Get selected MDEPs
						 If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String ="DD"
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{arrPEG}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
						
						End If			
					Next
				Next
			End If
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("ROC",sROC)
			dArgs.Add("ReportType",sReportType)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
'			Return Nothing
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

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
			Dim sAccount As String = "REQ_Requested_Amt"

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
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)
'Dim tStart2 As DateTime =  Date.Now()
			
			
				Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
				Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
				processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR TYPE","COST CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"REQ_Return_Cmt",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST"})


			
sFileHeader = $"SCENARIO,Entity,FLOW,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,Requirement_Return_Comment,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST"
			
			
				For Each Entity As Member In lsEntity
					For i As Integer = 0 To 4 Step 1 
						Dim myDataUnitPk As New DataUnitPk( _
						BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
						DimConstants.Local, _
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
						BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

						' Buffer coordinates.
						' Default to #All for everything, then set IDs where we need it.
						Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
						myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
						myDbCellPk.OriginId = DimConstants.BeforeAdj
						myDbCellPk.UD7Id = DimConstants.None
						myDbCellPk.UD8Id = DimConstants.None
						
						'No Selected PEG & MDEP = Get all MDEPs
						If String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then
							Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
							If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
'BRApi.ErrorLog.LogMessage(si, "myCells.Count: " & myCells.Count)
						'No Selected PEG & Select MDEP = Get selected MDEPs
						'Selected PEG & Selected MDEP = Get selected MDEPs
						Else If Not String.IsNullOrWhiteSpace(sMDEP) Then
							Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#PEG.Base.Keep({sMDEP})", True,,)
							For Each oU2 As MemberInfo In oU2List
								myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
								Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
								If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
							Next				
						
						'Selected PEG & No Selected MDEP = Get all MDEPs under selected PEG
						Else If Not String.IsNullOrWhiteSpace(sPEG) And String.IsNullOrWhiteSpace(sMDEP) Then					
							Dim arrPEG As String() = sPEG.Split(",")
							For Each PEG As String In arrPEG
								Dim oU2List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP"), $"U2#{PEG.Trim}.Base", True,,)		
								For Each oU2 As MemberInfo In oU2List
									myDbCellPk.UD2Id = BRApi.Finance.Members.GetMemberId(si, dimTypeId.UD2, oU2.Member.Name)
									Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
									If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
								Next	
							Next
						End If			
					Next
				Next
			
'Dim tStop2 As DateTime =  Date.Now()
'Dim tDuration2 As TimeSpan = tStop2.Subtract(tStart2)
'BRapi.ErrorLog.LogMessage(si, "Time to read & write DataBuffer" & tDuration2.TotalSeconds.ToString("0.0000"))
				
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
			
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate, sFvParam)

		End Function
	
#End Region

#Region "ExportReport Helper"

		'----------------------------------------------------------------------------------
		'     Create data tables to be used for fetching and processing fetched data
		'----------------------------------------------------------------------------------
		Private Function CreateReportDataTable(ByVal si As SessionInfo, ByVal dataTableName As String, ByVal columns As List(Of String), Optional ByVal bAllowDBNull As Boolean = False) As DataTable
			Try
				'Create the data table to return
				Dim dt As New DataTable(dataTableName)
				For Each column As String In columns	
					Dim objCol = New DataColumn
		            objCol.ColumnName = column
					If column.XFContainsIgnoreCase("amount") Then
						objCol.DataType = GetType(Long)
						objCol.DefaultValue = 0
					Else
		           		objCol.DataType = GetType(String)
						objCol.DefaultValue = ""
					End If
		            
		            objCol.AllowDBNull = bAllowDBNull
		            dt.Columns.Add(objCol)
				Next
								
				Return dt
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		'----------------------------------------------------------------------------------
		'     Write data from databuffer's datacells into datatable
		'----------------------------------------------------------------------------------
		Private Sub WriteFetchTable(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal Entity As String, ByVal Scenario As String, ByVal Time As String, ByVal oDataCells As List(Of Datacell))
			Try
				For Each oDataCell As DataCell In oDataCells
	            'Create a new row and append it to the table
					Dim row As DataRow = dt.NewRow()
					Dim account As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Account, oDataCell.DataCellPk.AccountId)
					Dim flow As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.Flow, oDataCell.DataCellPk.FlowId)
					Dim u1 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD1, oDataCell.DataCellPk.UD1Id)
					Dim u2 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD2, oDataCell.DataCellPk.UD2Id)
					Dim u3 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD3, oDataCell.DataCellPk.UD3Id)
					Dim u4 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD4, oDataCell.DataCellPk.UD4Id)
					Dim u5 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD5, oDataCell.DataCellPk.UD5Id)
					Dim u6 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD6, oDataCell.DataCellPk.UD6Id)
					Dim u7 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD7, oDataCell.DataCellPk.UD7Id)
					Dim u8 As String = BRApi.Finance.Members.GetMemberName(si, dimTypeId.UD7, oDataCell.DataCellPk.UD8Id)
					
					'Get List of Column Names
					Dim columnNames As List(Of String) = dt.Columns.Cast(Of DataColumn)().Select(Function(col) col.ColumnName).ToList()
					'Assign values to row by column names
					If columnNames.Contains("ENTITY") Then row("ENTITY") = Entity
					If columnNames.Contains("SCENARIO") Then row("SCENARIO") = Scenario
					If columnNames.Contains("TIME") Then row("TIME") = Time
					If columnNames.Contains("FLOW") Then row("FLOW") = flow
					If columnNames.Contains("ACCOUNT") Then row("ACCOUNT") = account
					If columnNames.Contains("U1") Then row("U1") = u1
					If columnNames.Contains("U2") Then row("U2") = u2
					If columnNames.Contains("U3") Then row("U3") = u3.Split("_")(1)
					If columnNames.Contains("U4") Then row("U4") = u4
					If columnNames.Contains("U5") Then row("U5") = u5
					If columnNames.Contains("U6") Then row("U6") = u6
					If columnNames.Contains("U7") Then row("U7") = u7
					If columnNames.Contains("U8") Then row("U8") = u8
					If columnNames.Contains("AMOUNT") Then row("AMOUNT") = oDataCell.CellAmount
						
'					row("ENTITY") = Entity
'					row("U1") = u1
'					row("U2") = u2
'					row("U3") = u3
'					row("U4") = u4				
'					row("TIME") = Time
'					row("AMOUNT") = oDataCell.CellAmount
				
'					Select Case True
'					Case dt.TableName.XFContainsIgnoreCase("cWork") 					
'						row("U5") = u5
'						row("SCENARIO") = Scenario
'					Case dt.TableName.XFContainsIgnoreCase("General") Or dt.TableName.XFContainsIgnoreCase("SS PEG")
'						row("FLOW") = flow
'						row("U6") = u6
'					Case dt.TableName.XFContainsIgnoreCase("DD PEG")
'						row("SCENARIO") = Scenario
'						row("FLOW") = flow
'						row("U5") = u5
						
'					End Select
'Brapi.ErrorLog.LogMessage(si, $"Name = {Name} | Value = {Value}")	

                	dt.Rows.Add(row)
				Next
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	
		
		'--------------------------------------------------------------------------------------------------------------------------
		'     Process data from fetch datatable into another datatable with amount combined according to keys (E#F#U1#.....)
		'--------------------------------------------------------------------------------------------------------------------------
		
		Private Sub ProcessTableForReport(ByVal si As SessionInfo, ByVal FetchDt As DataTable, processDT As DataTable, Optional ByVal dArgs As Dictionary(Of String, String) = Nothing) 
			Try				
				#Region "cWork Key5"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cWork Key5") Then
					Dim Cube As String = dArgs("Cube")
					Dim firstYr As Integer = COnvert.ToInt32(dArgs("firstYr"))
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim ROC As String = dArgs("ROC")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
				
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						'Dim Entity As String = Row("ENTITY")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")							
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$" | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | ")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String) = Tuple.Create(U1,U2,U3,U4)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(8){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - firstYr	
						Brapi.ErrorLog.LogMessage(si, "iPOS" & iPos )
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
					
					
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						'Dim Entity As String = kvp.Key.Item1
						Dim U1 As String = kvp.Key.Item1
						Dim U2 As String = kvp.Key.Item2
						Dim U3 As String = kvp.Key.Item3
						Dim U4 As String = kvp.Key.Item4
					
					
						Dim RC As String = ""

						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						'Get Issuecode
						Dim Issuecode As String = ROC.Substring(0,2) & "RS1000"
						'Write to processed DataTable
						
						Dim newRow As DataRow = processDT.Rows.Add()
						'"BUDGET CYCLE","BUDGET VERSION","MAIN ACCOUNT","SPENDING PLAN VERSION","FUNDS CENTER","FUND","FUNDED PROGRAM","COMMITMENT ITEM GROUP","COST COLLECTOR TYPE","COST COLLECTOR","FUNCTIONAL AREA","SOURCE SYSTEM","BUDGET CYCLE YEAR","AREA OF RESP","ATTRIBUTE 1","ATTRIBUTE 2","ATTRIBUTE 3","ATTRIBUTE 4","Period 1-AMT","Period 2-AMT","Period 3-AMT","Period 4-AMT","Period 5-AMT","Period 6-AMT","Period 7-AMT","Period 8-AMT","Period 9-AMT","Period 10-AMT","Period 11-AMT","Period 12-AMT","Period 1-QTY","Period 2-QTY","Period 3-QTY","Period 4-QTY","Period 5-QTY","Period 6-QTY","Period 7-QTY","Period 8-QTY","Period 9-QTY","Period 10-QTY","Period 11-QTY","Period 12-QTY"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("ROC")= ROC
						newRow("DOLLAR_TYPE")= U4
						newRow("BO")= "R"
						newRow("RC")= U1
						newRow("Name")= "RMW_" & Issuecode
						newRow("Justification")= "RMW snchronization"
						newRow("Issue")= Issuecode
						newRow("Remarks")= "Command input for BO R data call"
						'Write 8-Up amounts
'					Dim lowerbound As Integer = -4
'					Dim arraylength As Integer = 9 
'					Dim length As Integer() = {arraylength}
'					Dim newlowerbound As Integer() = {lowerbound}
				
'				Dim newarray As System.Array = Array.CreateInstance(GetType(Double), length,newlowerbound)
'						For i As Integer =	newarray.GetLowerBound(0) To newarray.GetUpperBound(0)

				For i As Integer = 0 To 8
					'Get cPROBE Data
						Dim cProbePos As String =  "Cb#ARMY:E#GlobalVar:C#Local:S#" & Scenario & ":T#" & startYr & ":V#Annotation:A#Var_Selected_Position:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
						Dim cProbeScenario As String = 	 BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, cProbePos).DataCellEx.DataCellAnnotation
						Dim U3Concat As String = U1 & "_" & U3
					Dim sSrcMbrScript As String = "Cb#ARMY:S#" & cProbeScenario & ":T#" & firstYr + i & ":C#Local:E#" & ROC & ":V#YTD:A#BOR:F#Baseline:I#Top:O#BeforeAdj:U1#" & U1 & ":U2#" & U2 & ":U3#" & U3Concat & ":U4#" & U4 & ":U5#Top:U6#None:U7#Top:U8#Top"
					Dim cProbeAMT As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, Cube, sSrcMbrScript).DataCellEx.DataCell.CellAmount
						'Brapi.ErrorLog.LogMessage(si, "cPROBE script" & sSrcMbrScript)
					'Brapi.ErrorLog.LogMessage(si, "REQAMT" & Amount(i))
					Dim cWorkDelta As Decimal =Math.Round(((Amount(i) - cProbeAMT)/1000),0)
					'Dim cWorkDeltaRound As Decimal  =  Math.Round((cWorkDelta/1000),0)
							newRow($"FY{firstYr + i}")= cWorkDelta
						Next
						
					Next			
				End If
				#End Region
				
				#Region "cWork Key15"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cWork Key15") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","ISSUECODE","BO","RQMT TITLE","RQMT DESCRIPTION","REMARKS","MDEP","APPN","APE","ROC","DOLLAR TYPE","JUON","ISR FLAG","COST MODEL","COMBAT LOSS","COST LOCATION","CATEGORY A CODE","CBS CODE","MIP PROJ CODE"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get Text accounts From DataAttachment Using SQL - Do it For the entire cube

						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_DESCRIPTION],
											[REQ_JUON],
											[REQ_ISR_Flag],
											[REQ_Cost_Model],
											[REQ_Combat_Loss],
											[REQ_Cost_Location],
											[REQ_Category_A_Code],
											[REQ_CBS_Code],
											[REQ_MIP_Proj_Code])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'BRApi.ErrorLog.LogMessage(si, $"dt num rows: {dt.Rows.Count}")
						End Using
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
					    	
						'Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | Flow = {Flow} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(4){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						'Get Issuecode
						Dim Issuecode As String = ROC.Substring(0,2) & "RS1000"
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4 
						newRow("BO")= "R"
						newRow("ROC")= ROC
						newRow("ISSUECODE")= Issuecode
						newRow("Remarks")= "Command input for OOC key15 data call"
						
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
						Next

							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								
								newRow("RQMT TITLE")= """"&resultRow("REQ_TITLE") & """"
								newRow("RQMT DESCRIPTION")= """"&resultRow("REQ_Description") & """"
								newRow("JUON")= """"&resultRow("REQ_Juon") & """"
								newRow("ISR FLAG")= """"&resultRow("REQ_ISR_Flag") & """"
								newRow("COST MODEL")= """"&resultRow("REQ_Cost_Model") & """"
								newRow("COMBAT LOSS")= """"&resultRow("REQ_Combat_Loss") & """"
								newRow("COST LOCATION")= """"&resultRow("REQ_Cost_Location") & """"
								newRow("CATEGORY A CODE")= """"&resultRow("REQ_Category_A_Code") & """"
								newRow("CBS CODE")= """"&resultRow("REQ_CBS_Code") & """"
								newRow("MIP PROJ CODE")= """"&resultRow("REQ_MIP_Proj_Code") & """"
								
								
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "cSustain"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cSustain No LINS") Then
					
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","BO","REQ_RQMT_STATUS","REQ_TITLE","REQ_COMMENTS","REQ_RECURRING_JUSTIFICATION","REQ_SS_PRIORITY","REQ_COMMITMENT_GROUP","REQ_CAPABILITY_SS","REQ_STRATEGIC_BIN"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					If ReportType.XFContainsIgnoreCase("Detail") Then
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_SS_PRIORITY],[REQ_COMMITMENT_GROUP],[REQ_CAPABILITY_SS],[REQ_STRATEGIC_BIN])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
					End If	
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("ROC")= ROC	
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
						Next

							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
														
								newRow("NAME")= """"&resultRow("REQ_TITLE") & """"
								newRow("REMARKS")= """"&resultRow("REQ_COMMENTS") & """"
								newRow("JUSTIFICATION")= """"&resultRow("REQ_RECURRING_JUSTIFICATION") & """"
								newRow("FUNCTIONAL PRIORITY")= """"&resultRow("REQ_SS_PRIORITY") & """"
								newRow("CAPABILITY")= """"&resultRow("REQ_CAPABILITY_SS") & """"
								newRow("STRATEGIC BIN")= """"&resultRow("REQ_STRATEGIC_BIN") & """"
								newRow("COMMITMENT GROUP")= """"&resultRow("REQ_COMMITMENT_GROUP") & """"
							End If
						
					Next			
				End If
				#End Region
				
				#Region "cDIGITAL"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cDigital") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"REQ_RQMT_STATUS","REQ_Type","REQ_TITLE","REQ_DESCRIPTION","REQ_New_Rqmt_Ind","REQ_APMS_AITR_Num","REQ_DD_Priority","REQ_Portfolio","REQ_Capability_DD","REQ_JNT_CAP_AREA","REQ_TBM_COST_POOL","REQ_TBM_TOWER","REQ_ZERO_TRUST_CAPABILITY","REQ_Assoc_Directorate","REQ_CLOUD_INDICATOR","REQ_STRAT_CYBERSEC_PGRM","REQ_Notes","REQ_UNIT_OF_MEASURE","REQ_FY1_ITEMS","REQ_FY1_UNIT_COST","REQ_FY2_ITEMS","REQ_FY2_UNIT_COST","REQ_FY3_ITEMS","REQ_FY3_UNIT_COST","REQ_FY4_ITEMS","REQ_FY4_UNIT_COST","REQ_FY5_ITEMS","REQ_FY5_UNIT_COST"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					If ReportType.XFContainsIgnoreCase("Detail") Then
						
						'Get Text accounts From DataAttachment Using SQL - Do it For the entire cube

						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_Type],[REQ_TITLE],[REQ_DESCRIPTION],
											[REQ_New_Rqmt_Ind],
											[REQ_APMS_AITR_Num],
											[REQ_DD_Priority],
											[REQ_Portfolio],
											[REQ_Capability_DD],
											[REQ_JNT_CAP_AREA],
											[REQ_TBM_COST_POOL],
											[REQ_TBM_TOWER],
											[REQ_ZERO_TRUST_CAPABILITY],
											[REQ_Assoc_Directorate],
											[REQ_CLOUD_INDICATOR],
											[REQ_STRAT_CYBERSEC_PGRM],
											[REQ_Notes],
											[REQ_UNIT_OF_MEASURE],
											[REQ_FY1_ITEMS],
											[REQ_FY1_UNIT_COST],
											[REQ_FY2_ITEMS],
											[REQ_FY2_UNIT_COST],
											[REQ_FY3_ITEMS],
											[REQ_FY3_UNIT_COST],
											[REQ_FY4_ITEMS],
											[REQ_FY4_UNIT_COST],
											[REQ_FY5_ITEMS],
											[REQ_FY5_UNIT_COST])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
'BRApi.ErrorLog.LogMessage(si, $"dt num rows: {dt.Rows.Count}")
						End Using
					End If	
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
					    Dim U5 As String = Row("U5")	
						'Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | Flow = {Flow} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U5)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U5 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("CTYPE")= U5
						newRow("ROC")= ROC
						NewRow("SUBCMD")= Entity
						'Write 5-Up amounts
						'For i As Integer = 0 To 4 Step 1
'						For i As Integer = 0 To 0 
'							newRow($"FY{startYr + i}")= Amount(i)
'						Next

							
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								newRow("RQMT TYPE")= """"&resultRow("REQ_TYPE") & """"
								newRow("RQMT SHORT TITLE")= """"&resultRow("REQ_TITLE") & """"
								newRow("RQMT DESCRIPTION")= """"&resultRow("REQ_Description") & """"
								newRow("EMERGING RQMT?")= """"&resultRow("REQ_NEW_RQMT_IND") & """"
								newRow("APMS AITR #")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
								newRow("Priority")=  """"&resultRow("REQ_DD_Priority") & """"
								newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
								newRow("Capability")=  """"&resultRow("REQ_Capability_DD") & """"
								newRow("JNT CAP AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
								newRow("TBM COST POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
								newRow("TBM TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
								newRow("ZERO TRUST CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
								newRow("Associated Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
								newRow("CLOUD INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
								newRow("STRAT CYBERSEC PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
								newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
								newRow("UNIT OF MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
								newRow("FY1 # ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
								newRow("FY1 # UNIT COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
								newRow("FY2 # ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
								newRow("FY2 # UNIT COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
								newRow("FY3 # ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
								newRow("FY3 # UNIT COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
								newRow("FY4 # ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
								newRow("FY4 # UNIT COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
								newRow("FY5 # ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
								newRow("FY5 # UNIT COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "General(All Reqs)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("All Requirements") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					
					Dim Scenario As String = dArgs("Scenario")
					
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & " REQ"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){
					"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Recurring_Justification",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_Return_Cmt",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($" (Select ENTITY, FLOW, TEXT,
								Case
									When ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									When ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									When ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									When ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									When ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									When ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									When ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									When ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								Else
									ACCOUNT
								End As AccountCAT
					From DataAttachment Where  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
					[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_Return_Cmt],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]
					)) AS PIVOTTABLE ") 
						 
						
						
	'	BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
				
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U5 As String = Row("U5")
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
					Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COST CATEGORY","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						
						newRow("COST CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next

						
							'Get "REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT"
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
								'NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN
								'[REQ_Title],[REQ_ID],[REQ_DESCRIPTION],[REQ_APPROVAL_COMMENT],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_COMMITMENT_GROUP]
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("REQ_Return_Cmt")=  """"&resultRow("REQ_Return_Cmt") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"

							End If 
						
					Next			
				End If
				#End Region	
				
				#Region "DMOPS"
				
				If FetchDt.TableName.XFContainsIgnoreCase("cSustain DMOPS") Then
					
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim ROC As String = dArgs("ROC")
					Dim Scenario As String = dArgs("Scenario")
					Dim ReportType As String = dArgs("ReportType")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String),Long())
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & "-BASE"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCENARIO","BO","REQ_RQMT_STATUS","REQ_TITLE","REQ_COMMENTS","REQ_RECURRING_JUSTIFICATION","REQ_SS_PRIORITY","REQ_COMMITMENT_GROUP","REQ_CAPABILITY_SS","REQ_STRATEGIC_BIN"})
					dt = Me.CreateReportDataTable(si,"CMDApprovedREQList",detailColumns,True)	
					
						
						'Get NAME,REQ_ID,REQ_DESCRIPTION,REQ_APPROVAL_COMMENT,REMARKS,JUSTIFICATION,STRATEGIC BIN from DataAttachment using SQL - do it for the entire cube
						Dim SQL As New Text.StringBuilder
						SQL.Append($"SELECT * FROM ") 
						SQL.Append($"	(SELECT ENTITY, FLOW, ACCOUNT, TEXT FROM DATAATTACHMENT WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS SOURCETABLE ")
						SQL.Append($"	PIVOT (") 
						SQL.Append($"	MAX(TEXT) FOR ACCOUNT IN ([REQ_RQMT_STATUS],[REQ_TITLE],[REQ_COMMENTS],[REQ_RECURRING_JUSTIFICATION],[REQ_SS_PRIORITY],[REQ_COMMITMENT_GROUP],[REQ_CAPABILITY_SS],[REQ_STRATEGIC_BIN],[REQ_LIN],[REQ_FY1_QTY],[REQ_FY2_QTY],[REQ_FY3_QTY],[REQ_FY4_QTY],[REQ_FY5_QTY])) AS PIVOTTABLE ") 
						SQL.Append($"	WHERE [REQ_RQMT_STATUS] = 'L2 Approved'")
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
						'Dim dtFetch As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
						End Using
					
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")					
						Dim U6 As String = Row("U6")				
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key As Tuple(Of String, String, String, String, String, String, String) = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6)
					
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String, String),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
						Dim U6 As String = kvp.Key.Item7
					
						Dim RC As String = ""
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Get Parent APPN
							
							Dim iU1MbrID As Integer = BRapi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,U1)
							Dim sParentAppn As String = BRApi.Finance.Members.GetParents(si, BRApi.Finance.Dim.GetDimPk(si, "U1_APPN_FUND"), iU1MbrID, False)(0).Name
							
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
						'"SCENARIO","BO","NAME","REQ_ID","REQ_DESCRIPTION","REQ_APPROVAL_COMMENT","REMARKS","JUSTIFICATION","MDEP","APPN","APE","ROC","DOLLAR_TYPE","COMMITMENT GROUP","CAPABILITY","STRATEGIC BIN",$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}"
						newRow("SCENARIO")= exportScenario
						newRow("APPN")= sParentAppn
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR TYPE")= U4
						newRow("BO")= "R"
						newRow("ROC")= ROC	
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							Dim updatedValue As Double = math.Round((Amount(i)/1000),0)
							newRow($"FY{startYr + i}")= updatedValue
							
						Next

							
							'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
							Dim resultRow As DataRow = dt.AsEnumerable() _
								.SingleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
														AndAlso row.Field(Of String)("FLOW") = Flow)
							'Assign values
							If resultRow IsNot Nothing Then
														
								newRow("NAME")= """"&resultRow("REQ_TITLE") & """"
								newRow("REMARKS")= """"&resultRow("REQ_COMMENTS") & """"
								newRow("JUSTIFICATION")= """"&resultRow("REQ_RECURRING_JUSTIFICATION") & """"
								newRow("FUNCTIONAL PRIORITY")= """"&resultRow("REQ_SS_PRIORITY") & """"
								newRow("CAPABILITY")= """"&resultRow("REQ_CAPABILITY_SS") & """"
								newRow("STRATEGIC BIN")= """"&resultRow("REQ_STRATEGIC_BIN") & """"
								newRow("COMMITMENT GROUP")= """"&resultRow("REQ_COMMITMENT_GROUP") & """"
								newRow("LIN")= """"&resultRow("REQ_LIN") & """"
								
								If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY1_QTY").ToString)
									
								newRow("FY1_QTY") = "0"
							Else 
								newRow("FY1_QTY") = """"&resultRow("REQ_FY1_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY2_QTY").ToString)
									
								newRow("FY2_QTY") = "0"
							Else 
								newRow("FY2_QTY") = """"&resultRow("REQ_FY2_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY3_QTY").ToString)
									
								newRow("FY3_QTY") = "0"
							Else 
								newRow("FY3_QTY") = """"&resultRow("REQ_FY3_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY4_QTY").ToString)
									
								newRow("FY4_QTY") = "0"
							Else 
								newRow("FY4_QTY") = """"&resultRow("REQ_FY4_QTY") & """"
							End If
							If 	String.IsNullOrWhiteSpace(resultRow("REQ_FY5_QTY").ToString)
									
								newRow("FY5_QTY") = "0"
							Else 
								newRow("FY5_QTY") = """"&resultRow("REQ_FY5_QTY") & """"
							End If
								
							End If
						
					Next			
				End If
				#End Region
				
				#Region "Export All REQs(Review dashboard)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("ExportAllREQs") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = COnvert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					
					Dim exportScenario As String = "POM" & startYr.ToString.Substring(startYr.ToString.Length - 2) & " REQ"
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)({
					"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Recurring_Justification",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_Return_Cmt",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY", 
																"Command"})
					dt = Me.CreateReportDataTable(si,"ExportAllREQs",detailColumns,True)	
				
						
						
					Dim SQL As New Text.StringBuilder
					SQL.Append($"SELECT * FROM ") 
					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
								CASE
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								ELSE
									ACCOUNT
								End as AccountCAT
					FROM DataAttachment WHERE  CUBE = '{Cube}' AND SCENARIO = '{Scenario}') AS src ")
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
					[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_Return_Cmt],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]
					)) AS PIVOTTABLE ") 
						
						
					
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
					'Dim dtFetch As New DataTable
						
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
					End Using
'BRApi.ErrorLog.LogMessage(si, "SQL is done")
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U6 As String = Row("U6")
						Dim U5 As String = Row("U5")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
					
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
					
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
					
						newRow("SCENARIO")= exportScenario
						newRow("Command")= Cube
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR_TYPE")= U4	
						newRow("COST_CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next
						
						
							
						'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
						Dim resultRow As DataRow = dt.AsEnumerable() _
							.singleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
													AndAlso row.Field(Of String)("FLOW") = Flow)
													
						'Assign values
						'Assign values
						If resultRow IsNot Nothing Then
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("REQ_Return_Cmt")=  """"&resultRow("REQ_Return_Cmt") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"



						End If
					
					Next			
				End If
				#End Region	
				
				#Region "Export All Updated REQs(Import Dashboard)"
				
				If FetchDt.TableName.XFContainsIgnoreCase("ExportAllUpdatedREQs") Then
					Dim Cube As String = dArgs("Cube")
					Dim startYr As Integer = Convert.ToInt32(dArgs("startYr"))
					Dim Scenario As String = dArgs("Scenario")
					Dim groupedData As New Dictionary(Of Tuple(Of String, String, String, String, String, String, String, Tuple(Of String)),Long())
					Dim StatusAccount As String = "REQ_Rqmt_Status"
					
					Dim exportScenario As String = "PGM_C" & startYr.ToString.Substring(startYr.ToString.Length - 4) 
					
					Dim dt As New DataTable
					Dim detailColumns As New list(Of String)
					detailColumns.AddRange(New String(){"SCERARIO","ENTITY","FLOW","REQ_RQMT_STATUS","REQ_Title",
																"REQ_Description",
																"REQ_Recurring_Justification",
																"REQ_Cost_Methodology_Cmt",
																"REQ_Impact_If_Not_Funded",
																"REQ_Risk_If_Not_Funded",
																"REQ_Cost_Growth_Justification",
																"REQ_Must_Fund",
																"REQ_Requested_Fund_Source",
																"REQ_Army_initiative_Directive",
																"REQ_Command_Initiative_Directive",
																"REQ_Activity_Exercise",
																"REQ_IT_Cyber_Rqmt_Ind",
																"REQ_UIC_Acct",
																"REQ_Flex_Field_1",
																"REQ_Flex_Field_2",
																"REQ_Flex_Field_3",
																"REQ_Flex_Field_4",
																"REQ_Flex_Field_5",
																"REQ_New_Rqmt_Ind",
																"REQ_CPA_Topic",
																"REQ_PBR_Submission",
																"REQ_UPL_Submission",
																"REQ_Contract_Number",
																"REQ_Task_Order_Number",
																"REQ_Target_Date_Of_Award",
																"REQ_POP_Expiration_Date",
																"REQ_FTE_CME",
																"REQ_COR_Email",
																"REQ_POC_Email",
																"REQ_Directorate",
																"REQ_Division",
																"REQ_Branch",
																"REQ_Rev_POC_Email",
																"REQ_MDEP_Func_Email",
																"REQ_Notification_Email_List",
																"REQ_Comments",
																"REQ_JUON",
																"REQ_ISR_Flag",
																"REQ_Cost_Model",
																"REQ_Combat_Loss",
																"REQ_Cost_Location",
																"REQ_Category_A_Code",
																"REQ_CBS_Code",
																"REQ_MIP_Proj_Code",
																"REQ_Type",
																"REQ_DD_Priority",
																"REQ_Portfolio",
																"REQ_Capability_DD",
																"REQ_JNT_CAP_AREA",
																"REQ_TBM_COST_POOL",
																"REQ_TBM_TOWER",
																"REQ_APMS_AITR_Num",
																"REQ_ZERO_TRUST_CAPABILITY",
																"REQ_Assoc_Directorate",
																"REQ_CLOUD_INDICATOR",
																"REQ_STRAT_CYBERSEC_PGRM",
																"REQ_Notes",
																"REQ_UNIT_OF_MEASURE",
																"REQ_FY1_ITEMS",
																"REQ_FY1_UNIT_COST",
																"REQ_FY2_ITEMS",
																"REQ_FY2_UNIT_COST",
																"REQ_FY3_ITEMS",
																"REQ_FY3_UNIT_COST",
																"REQ_FY4_ITEMS",
																"REQ_FY4_UNIT_COST",
																"REQ_FY5_ITEMS",
																"REQ_FY5_UNIT_COST",
																"REQ_SS_Priority",
																"REQ_Commitment_Group",
																"REQ_Capability_SS",
																"REQ_Strategic_BIN",
																"REQ_LIN",
																"REQ_FY1_QTY",
																"REQ_FY2_QTY",
																"REQ_FY3_QTY",
																"REQ_FY4_QTY",
																"REQ_FY5_QTY", 
																"Command"})
					dt = Me.CreateReportDataTable(si,"ExportAllUpdatedREQs",detailColumns,True)	
				
						
						
					Dim SQL As New Text.StringBuilder
					SQL.Append($"SELECT * FROM ") 
					SQL.Append($"	(SELECT ENTITY, FLOW, TEXT,
								CASE
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Owner'  then 'OwnerName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Owner' then 'OwnerEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Owner' then 'OwnerPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Owner' then 'OwnerCmt'
					
									WHEN ACCOUNT = 'REQ_POC_Name' AND UD5 = 'REQ_Func_POC'  then 'MDEPFuncName'
									WHEN ACCOUNT = 'REQ_POC_Email' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncEmail'
									WHEN ACCOUNT = 'REQ_POC_Phone' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncPhone'
									WHEN ACCOUNT = 'REQ_POC_Cmt' AND UD5 = 'REQ_Func_POC' then 'MDEPFuncCmt'

								ELSE
									ACCOUNT
								End as AccountCAT
					FROM DataAttachment WHERE CUBE = '{Cube}' AND SCENARIO = '{Scenario}' ) as src ")
					'WHERE (ACCOUNT = '{StatusAccount}' and Text Not like '{ApprovedStatus}')
					SQL.Append($"	PIVOT (") 
					SQL.Append($"	MAX(TEXT) FOR AccountCAT IN ([REQ_RQMT_STATUS],[REQ_Title],
[REQ_Description],
[REQ_Recurring_Justification],
[REQ_Cost_Methodology_Cmt],
[REQ_Impact_If_Not_Funded],
[REQ_Risk_If_Not_Funded],
[REQ_Cost_Growth_Justification],
[REQ_Must_Fund],
[REQ_Requested_Fund_Source],
[REQ_Army_initiative_Directive],
[REQ_Command_Initiative_Directive],
[REQ_Activity_Exercise],
[REQ_IT_Cyber_Rqmt_Ind],
[REQ_UIC_Acct],
[REQ_Flex_Field_1],
[REQ_Flex_Field_2],
[REQ_Flex_Field_3],
[REQ_Flex_Field_4],
[REQ_Flex_Field_5],
[REQ_New_Rqmt_Ind],
[REQ_CPA_Topic],
[REQ_PBR_Submission],
[REQ_UPL_Submission],
[REQ_Contract_Number],
[REQ_Task_Order_Number],
[REQ_Target_Date_Of_Award],
[REQ_POP_Expiration_Date],
[REQ_FTE_CME],
[REQ_COR_Email],
[REQ_POC_Email],
[REQ_Directorate],
[REQ_Division],
[REQ_Branch],
[REQ_Rev_POC_Email],
[REQ_MDEP_Func_Email],
[REQ_Notification_Email_List],
[REQ_Comments],
[REQ_JUON],
[REQ_ISR_Flag],
[REQ_Cost_Model],
[REQ_Combat_Loss],
[REQ_Cost_Location],
[REQ_Category_A_Code],
[REQ_CBS_Code],
[REQ_MIP_Proj_Code],
[REQ_Type],
[REQ_DD_Priority],
[REQ_Portfolio],
[REQ_Capability_DD],
[REQ_JNT_CAP_AREA],
[REQ_TBM_COST_POOL],
[REQ_TBM_TOWER],
[REQ_APMS_AITR_Num],
[REQ_ZERO_TRUST_CAPABILITY],
[REQ_Assoc_Directorate],
[REQ_CLOUD_INDICATOR],
[REQ_STRAT_CYBERSEC_PGRM],
[REQ_Notes],
[REQ_UNIT_OF_MEASURE],
[REQ_FY1_ITEMS],
[REQ_FY1_UNIT_COST],
[REQ_FY2_ITEMS],
[REQ_FY2_UNIT_COST],
[REQ_FY3_ITEMS],
[REQ_FY3_UNIT_COST],
[REQ_FY4_ITEMS],
[REQ_FY4_UNIT_COST],
[REQ_FY5_ITEMS],
[REQ_FY5_UNIT_COST],
[REQ_SS_Priority],
[REQ_Commitment_Group],
[REQ_Capability_SS],
[REQ_Strategic_BIN],
[REQ_LIN],
[REQ_FY1_QTY],
[REQ_FY2_QTY],
[REQ_FY3_QTY],
[REQ_FY4_QTY],
[REQ_FY5_QTY]

					)) AS PIVOTTABLE ") 
						
						
					
						
'BRApi.ErrorLog.LogMessage(si, "SQL: " & SQL.ToString)
					'Dim dtFetch As New DataTable
						
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						 dt = BRApi.Database.ExecuteSql(dbConn,SQL.ToString(),True)
					End Using
'BRApi.ErrorLog.LogMessage(si, "SQL is done")
					'Loop through the fetched datatable and group the monthly amounts into an array (value) of the same dim combination (key), write this into a dictionary						
					For Each Row As DataRow In FetchDt.Rows
						Dim Entity As String = Row("ENTITY")
						Dim Flow As String = Row("Flow")
						Dim U1 As String = Row("U1")
						Dim U2 As String = Row("U2")
						Dim U3 As String = Row("U3")
						Dim U4 As String = Row("U4")
						Dim U6 As String = Row("U6")
						Dim U5 As String = Row("U5")
						Dim Time As String = Row("TIME")
						Dim Amount As Long = Row("AMOUNT")
'BRapi.ErrorLog.LogMessage(si,$"Entity = {Entity} | U1 = {U1} | U2 = {U2} | U3 = {U3} | U4 = {U4} | U5 = {U5}")							
						'use Entity,U1,U2,U3,U4,U5 combination as Key
						Dim key = Tuple.Create(Entity,Flow,U1,U2,U3,U4,U6,U5)
						'Dim Key2 As Tuple(Of Key, String, String) = Tuple.Create(Key,U5,U6)
						
					'As Tuple(Of String, String, String, String, String, String, String, String)
						If Not groupedData.ContainsKey(key) Then
							groupedData(key) = New Long(5){}
						End If
						'group the amounts into an array of Long where Year 1 = array[0] and so on. The array is then used as the value of the tuple Key
						Dim iPos As Integer = Convert.ToInt32(Time) - startYr				
						groupedData(key)(iPos) = groupedData(key)(iPos) + Amount						
					Next
						
					'Iterate through the dictionary and write to processed datatable
					For Each kvp As KeyValuePair(Of Tuple(Of String, String, String, String, String, String,String, Tuple(Of String)),Long()) In groupedData
'					For Each row As DataRow In oSortedDt.Rows
						Dim Entity As String = kvp.Key.Item1
						Dim Flow As String = kvp.Key.Item2
						Dim U1 As String = kvp.Key.Item3
						Dim U2 As String = kvp.Key.Item4
						Dim U3 As String = kvp.Key.Item5
						Dim U4 As String = kvp.Key.Item6
					
						Dim U6 As String = kvp.Key.Item7
						Dim U5 As String = kvp.Key.Rest.Item1
					
'						Dim sFund As String = $"{U4} / {U1}"
						
						'Get amount-by-year array'
						Dim Amount As Long() = kvp.Value
						
						'Write to processed DataTable
						Dim newRow As DataRow = processDT.Rows.Add()
					
						newRow("SCENARIO")= exportScenario
						newRow("Command")= Cube
						newRow("ENTITY")= Entity
						newRow("FLOW")= Flow
						newRow("APPN")= U1
						newRow("MDEP")= U2
						newRow("APE")= U3
						newRow("DOLLAR_TYPE")= U4	
						newRow("COST_CATEGORY")= U6
						newRow("CTYPE")= U5
						'Write 5-Up amounts
						
						For i As Integer = 0 To 4 Step 1
							newRow($"FY{startYr + i}")= Amount(i)
						Next
						
							
						'Using LINQ to get row with Entity and Flow as key from the DataTable fetched from DataAttachment above
						Dim resultRow As DataRow = dt.AsEnumerable() _
							.singleOrDefault(Function(row) row.Field(Of String)("ENTITY") = Entity _
													AndAlso row.Field(Of String)("FLOW") = Flow)
													
						'Assign values
						If resultRow IsNot Nothing Then
							newRow("REQUIREMENT STATUS")= """" & resultRow("REQ_RQMT_STATUS") & """"
							newRow("TITLE")=  """"&resultRow("REQ_TITLE") & """"
							
			
newRow("Description")=  """"&resultRow("REQ_Description") & """"
newRow("Justification")=  """"&resultRow("REQ_Recurring_Justification") & """"
newRow("Cost_Methodology")=  """"&resultRow("REQ_Cost_Methodology_Cmt") & """"
newRow("Impact_If_Not_Funded")=  """"&resultRow("REQ_Impact_If_Not_Funded") & """"
newRow("Risk_If_Not_Funded")=  """"&resultRow("REQ_Risk_If_Not_Funded") & """"
newRow("Cost_Growth_Justification")=  """"&resultRow("REQ_Cost_Growth_Justification") & """"
newRow("Must_Fund")=  """"&resultRow("REQ_Must_Fund") & """"
newRow("Funding_Source")=  """"&resultRow("REQ_Requested_Fund_Source") & """"
newRow("Army_Initiative_Directive")=  """"&resultRow("REQ_Army_initiative_Directive") & """"
newRow("Command_Initiative_Directive")=  """"&resultRow("REQ_Command_Initiative_Directive") & """"
newRow("Activity_Exercise")=  """"&resultRow("REQ_Activity_Exercise") & """"
newRow("IT_Cyber_Requirement")=  """"&resultRow("REQ_IT_Cyber_Rqmt_Ind") & """"
newRow("UIC")=  """"&resultRow("REQ_UIC_Acct") & """"
newRow("Flex_Field_1")=  """"&resultRow("REQ_Flex_Field_1") & """"
newRow("Flex_Field_2")=  """"&resultRow("REQ_Flex_Field_2") & """"
newRow("Flex_Field_3")=  """"&resultRow("REQ_Flex_Field_3") & """"
newRow("Flex_Field_4")=  """"&resultRow("REQ_Flex_Field_4") & """"
newRow("Flex_Field_5")=  """"&resultRow("REQ_Flex_Field_5") & """"
newRow("Emerging_Requirement")=  """"&resultRow("REQ_New_Rqmt_Ind") & """"
newRow("CPA_Topic")=  """"&resultRow("REQ_CPA_Topic") & """"
newRow("PBR_Submission")=  """"&resultRow("REQ_PBR_Submission") & """"
newRow("UPL_Submission")=  """"&resultRow("REQ_UPL_Submission") & """"
newRow("Contract_Number")=  """"&resultRow("REQ_Contract_Number") & """"
newRow("Task_Order_Number")=  """"&resultRow("REQ_Task_Order_Number") & """"
newRow("Target_Date_Of_Award")=  """"&resultRow("REQ_Target_Date_Of_Award") & """"
newRow("POP_Expiration_Date")=  """"&resultRow("REQ_POP_Expiration_Date") & """"
newRow("ContractorManYearEquiv_CME")=  """"&resultRow("REQ_FTE_CME") & """"
newRow("COR_Email")=  """"&resultRow("REQ_COR_Email") & """"
newRow("POC_Email")=  """"&resultRow("REQ_POC_Email") & """"
newRow("Directorate")=  """"&resultRow("REQ_Directorate") & """"
newRow("Division")=  """"&resultRow("REQ_Division") & """"
newRow("Branch")=  """"&resultRow("REQ_Branch") & """"
newRow("Rev_POC_Email")=  """"&resultRow("REQ_Rev_POC_Email") & """"
newRow("MDEP_Functional_Email")=  """"&resultRow("REQ_MDEP_Func_Email") & """"
newRow("Notification_Email_List")=  """"&resultRow("REQ_Notification_Email_List") & """"
newRow("Comments")=  """"&resultRow("REQ_Comments") & """"
newRow("JUON")=  """"&resultRow("REQ_JUON") & """"
newRow("ISR_Flag")=  """"&resultRow("REQ_ISR_Flag") & """"
newRow("Cost_Model")=  """"&resultRow("REQ_Cost_Model") & """"
newRow("Combat_Loss")=  """"&resultRow("REQ_Combat_Loss") & """"
newRow("Cost_Location")=  """"&resultRow("REQ_Cost_Location") & """"
newRow("Category_A_Code")=  """"&resultRow("REQ_Category_A_Code") & """"
newRow("CBS_Code")=  """"&resultRow("REQ_CBS_Code") & """"
newRow("MIP_Proj_Code")=  """"&resultRow("REQ_MIP_Proj_Code") & """"
newRow("RequirementType")=  """"&resultRow("REQ_Type") & """"
newRow("DD_Priority")=  """"&resultRow("REQ_DD_Priority") & """"
newRow("Portfolio")=  """"&resultRow("REQ_Portfolio") & """"
newRow("DD_Capability")=  """"&resultRow("REQ_Capability_DD") & """"
newRow("JNT_CAP_AREA")=  """"&resultRow("REQ_JNT_CAP_AREA") & """"
newRow("TBM_COST_POOL")=  """"&resultRow("REQ_TBM_COST_POOL") & """"
newRow("TBM_TOWER")=  """"&resultRow("REQ_TBM_TOWER") & """"
newRow("APMS_AITR_Num")=  """"&resultRow("REQ_APMS_AITR_Num") & """"
newRow("ZERO_TRUST_CAPABILITY")=  """"&resultRow("REQ_ZERO_TRUST_CAPABILITY") & """"
newRow("Associated_Directives")=  """"&resultRow("REQ_Assoc_Directorate") & """"
newRow("CLOUD_INDICATOR")=  """"&resultRow("REQ_CLOUD_INDICATOR") & """"
newRow("STRAT_CYBERSEC_PGRM")=  """"&resultRow("REQ_STRAT_CYBERSEC_PGRM") & """"
newRow("Notes")=  """"&resultRow("REQ_Notes") & """"
newRow("UNIT_OF_MEASURE")=  """"&resultRow("REQ_UNIT_OF_MEASURE") & """"
newRow("FY1_ITEMS")=  """"&resultRow("REQ_FY1_ITEMS") & """"
newRow("FY1_UNIT_COST")=  """"&resultRow("REQ_FY1_UNIT_COST") & """"
newRow("FY2_ITEMS")=  """"&resultRow("REQ_FY2_ITEMS") & """"
newRow("FY2_UNIT_COST")=  """"&resultRow("REQ_FY2_UNIT_COST") & """"
newRow("FY3_ITEMS")=  """"&resultRow("REQ_FY3_ITEMS") & """"
newRow("FY3_UNIT_COST")=  """"&resultRow("REQ_FY3_UNIT_COST") & """"
newRow("FY4_ITEMS")=  """"&resultRow("REQ_FY4_ITEMS") & """"
newRow("FY4_UNIT_COST")=  """"&resultRow("REQ_FY4_UNIT_COST") & """"
newRow("FY5_ITEMS")=  """"&resultRow("REQ_FY5_ITEMS") & """"
newRow("FY5_UNIT_COST")=  """"&resultRow("REQ_FY5_UNIT_COST") & """"
newRow("SS_Priority")=  """"&resultRow("REQ_SS_Priority") & """"
newRow("Commitment_Group")=  """"&resultRow("REQ_Commitment_Group") & """"
newRow("SS_Capability")=  """"&resultRow("REQ_Capability_SS") & """"
newRow("Strategic_BIN")=  """"&resultRow("REQ_Strategic_BIN") & """"
newRow("LIN")=  """"&resultRow("REQ_LIN") & """"
newRow("FY1_QTY")=  """"&resultRow("REQ_FY1_QTY") & """"
newRow("FY2_QTY")=  """"&resultRow("REQ_FY2_QTY") & """"
newRow("FY3_QTY")=  """"&resultRow("REQ_FY3_QTY") & """"
newRow("FY4_QTY")=  """"&resultRow("REQ_FY4_QTY") & """"
newRow("FY5_QTY")=  """"&resultRow("REQ_FY5_QTY") & """"



						End If
					
					Next			
				End If
				#End Region				
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub	

		'----------------------------------------------------------------------------------
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

#Region "ExportAllUpdatedREQs(Import Dashboard)"	
		'Export PGM Requirement Data
		Public Function ExportAllUpdatedREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sCube)
			Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
			Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			
			
			Dim SAccount As String = "REQ_Requested_Amt"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
			Dim sFvParam As String = args.NameValuePairs.XFGetValue("FvParam","")
			sFilePath = ""
			BRApi.Dashboards.Parameters.SetLiteralParameterValue(si,False,sFvParam,sFilePath)		
			
			
			
			'Declare all Time values
			Dim iTime0 As Integer = iTime + 0
			Dim iTime1 As Integer = iTime + 1
			Dim iTime2 As Integer = iTime + 2
			Dim iTime3 As Integer = iTime + 3
			Dim iTime4 As Integer = iTime + 4
		
			
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
			columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","REQUIREMENT STATUS","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	
			
			processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR_TYPE","COST_CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST",
 "Command"})


			
sFileHeader = $"SCENARIO:Do Not Update,Entity:Do Not Update,FLOW:Do Not Update,REQUIREMENT STATUS:Do Not Update,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST,Command:Do Not Update"
			
'Get variable Ids and flow list to remove Manpower reqs
			Dim cubeid As Integer = BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId
			Dim scenarioid As Integer =BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario)
			Dim flowlist As List(Of MemberInfo) =  BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "F_REQ_Main"), $"F#Command_Requirements.Base.Where(Name DoesNotContain 'REQ_00')", True,,)
		
			
			For Each Entity As Member In lsEntity
				Dim entityid As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name)
				
					For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					 cubeid, _
					entityid, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					scenarioid, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
		'Get full data set for all flows then filter down to flow Ids we want			
					Dim allcellsubset As List(Of DataCell) = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
	
					 If allcellsubset.Count  > 0 Then
						 'Get flow Ids we need
						 Dim DesiredFlowIds As New HashSet(Of Integer)(flowlist.Select(Function(f) f.Member.MemberId))
						' Filter our subset cells vs the flows we want 
						 Dim Filteredcells As List(Of DataCell) = allcellsubset.Where(Function(cell) DesiredFlowIds.Contains(cell.DataCellPk.FlowId)).ToList()
					If Filteredcells.Count > 0 Then
						'write filter data to table 
						Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,Filteredcells)
					End If 
				End If 

					
				Next
			Next
		
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTableUpdate",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sCube)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region


#Region "ExportAllREQs(Review Daashboard)"	
		'Export PGM Requirement Data
		Public Function ExportAllREQs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As XFSelectionChangedTaskResult		
'BRapi.ErrorLog.LogMessage(si,$"Debug A")
			Dim sCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName	 
			Dim sEntity As String = args.NameValuePairs.XFGetValue("Entity","")	
			If sEntity.XFContainsIgnoreCase("_General") Then
				sEntity = sEntity.Replace("_General","")
			Else 
				sEntity = sEntity
			End If
			Dim iMemberId As Integer = BRApi.Finance.Members.GetMemberId(si,dimtypeid.Entity,sEntity)
			Dim SAccount As String = "REQ_Requested_Amt"
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim iScenarioID As Integer = brapi.Finance.Members.GetMemberId(si, DimType.Scenario.Id, sScenario)
			Dim iTime As Integer = BRApi.Finance.Time.GetYearFromId(si,BRApi.Finance.Scenario.GetWorkflowTime(si, iScenarioID))
			Dim sTemplate As String = args.NameValuePairs.XFGetValue("Template","")
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
			
			'Declare variables to fetch data
			Dim columns As New List(Of String)
			Dim processColumns As New List(Of String)
			Dim sFileHeader As String = ""
			
				columns.AddRange(New String(){"SCENARIO","ENTITY","FLOW","REQUIREMENT STATUS","U1","U2","U3","U4","U5","U6","TIME","AMOUNT"})
			Dim FetchDt As DataTable = Me.CreateReportDataTable(si,sTemplate,columns)

	
			Dim dimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"E_{sCube}")
			Dim lsEntity As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si, dimPK, iMemberId,)
			processColumns.AddRange(New String(){"SCENARIO","Entity","FLOW","REQUIREMENT STATUS","APPN","MDEP","APE","DOLLAR_TYPE","COST_CATEGORY","CTYPE",
			$"FY{iTime0}",$"FY{iTime1}",$"FY{iTime2}",$"FY{iTime3}",$"FY{iTime4}",
			"Title",
"Description",
"Justification",
"Cost_Methodology",
"Impact_If_Not_Funded",
"Risk_If_Not_Funded",
"Cost_Growth_Justification",
"Must_Fund",
"Funding_Source",
"Army_Initiative_Directive",
"Command_Initiative_Directive",
"Activity_Exercise",
"IT_Cyber_Requirement",
"UIC",
"Flex_Field_1",
"Flex_Field_2",
"Flex_Field_3",
"Flex_Field_4",
"Flex_Field_5",
"Emerging_Requirement",
"CPA_Topic",
"PBR_Submission",
"UPL_Submission",
"Contract_Number",
"Task_Order_Number",
"Target_Date_Of_Award",
"POP_Expiration_Date",
"ContractorManYearEquiv_CME",
"COR_Email",
"POC_Email",
"Directorate",
"Division",
"Branch",
"Rev_POC_Email",
"MDEP_Functional_Email",
"Notification_Email_List",
"Comments",
"REQ_Return_Cmt",
"JUON",
"ISR_Flag",
"Cost_Model",
"Combat_Loss",
"Cost_Location",
"Category_A_Code",
"CBS_Code",
"MIP_Proj_Code",
"SS_Priority",
"Commitment_Group",
"SS_Capability",
"Strategic_BIN",
"LIN",
"FY1_QTY",
"FY2_QTY",
"FY3_QTY",
"FY4_QTY",
"FY5_QTY",
"RequirementType",
"DD_Priority",
"Portfolio",
"DD_Capability",
"JNT_CAP_AREA",
"TBM_COST_POOL",
"TBM_TOWER",
"APMS_AITR_Num",
"ZERO_TRUST_CAPABILITY",
"Associated_Directives",
"CLOUD_INDICATOR",
"STRAT_CYBERSEC_PGRM",
"Notes",
"UNIT_OF_MEASURE",
"FY1_ITEMS",
"FY1_UNIT_COST",
"FY2_ITEMS",
"FY2_UNIT_COST",
"FY3_ITEMS",
"FY3_UNIT_COST",
"FY4_ITEMS",
"FY4_UNIT_COST",
"FY5_ITEMS",
"FY5_UNIT_COST",
 "Command"})


			
sFileHeader = $"SCENARIO,Entity,FLOW,REQUIREMENT STATUS,APPN,MDEP,APE,DOLLAR_TYPE,OBJECTCLASS,CTYPE,FY{iTime0},FY{iTime1},FY{iTime2},FY{iTime3},FY{iTime4},Title,Description,Justification,Cost_Methodology,Impact_If_Not_Funded,Risk_If_Not_Funded,Cost_Growth_Justification,Must_Fund,Funding_Source,Army_Initiative_Directive,Command_Initiative_Directive,Activity_Exercise,IT_Cyber_Requirement,UIC,Flex_Field_1,Flex_Field_2,Flex_Field_3,Flex_Field_4,Flex_Field_5,Emerging_Requirement,CPA_Topic,PBR_Submission,UPL_Submission,Contract_Number,Task_Order_Number,Target_Date_Of_Award,POP_Expiration_Date,ContractorManYearEquiv_CME,COR_Email,POC_Email,Directorate,Division,Branch,Rev_POC_Email,MDEP_Functional_Email,Notification_Email_List,Comments,Requirement_Return_Comment,JUON,ISR_Flag,Cost_Model,Combat_Loss,Cost_Location,Category_A_Code,CBS_Code,MIP_Proj_Code,SS_Priority,Commitment_Group,SS_Capability,Strategic_BIN,LIN,FY1_QTY,FY2_QTY,FY3_QTY,FY4_QTY,FY5_QTY,RequirementType,DD_Priority,Portfolio,DD_Capability,JNT_CAP_AREA,TBM_COST_POOL,TBM_TOWER,APMS_AITR_Num,ZERO_TRUST_CAPABILITY,Associated_Directives,CLOUD_INDICATOR,STRAT_CYBERSEC_PGRM,Notes,UNIT_OF_MEASURE,FY1_ITEMS,FY1_UNIT_COST,FY2_ITEMS,FY2_UNIT_COST,FY3_ITEMS,FY3_UNIT_COST,FY4_ITEMS,FY4_UNIT_COST,FY5_ITEMS,FY5_UNIT_COST,Command"
			
			
			For Each Entity As Member In lsEntity
				For i As Integer = 0 To 4 Step 1 
					Dim myDataUnitPk As New DataUnitPk( _
					BRApi.Finance.Cubes.GetCubeInfo(si, sCube).Cube.CubeId, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, Entity.Name ), _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Entity, ""), _
					DimConstants.Local, _
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, sScenario),
					BRApi.Finance.Members.GetMemberId(si, dimTypeId.Time, (iTime + i).ToString))

					' Buffer coordinates.
					' Default to #All for everything, then set IDs where we need it.
					Dim myDbCellPk As New DataBufferCellPk( DimConstants.All )
					myDbCellPk.AccountId = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, sAccount)
					myDbCellPk.OriginId = DimConstants.BeforeAdj
					myDbCellPk.UD7Id = DimConstants.None
					myDbCellPk.UD8Id = DimConstants.None
					
					Dim myCells As List(Of DataCell)  = BRApi.Finance.Data.GetDataBufferDataCells(si, myDataUnitPk, dimConstants.Periodic, myDbCellPk, True, True)
					If myCells.Count > 0 Then Me.WriteFetchTable(si,FetchDt,Entity.Name,sScenario,(iTime + i).ToString,myCells)
				Next
			Next
			
			'Process the fetched data into a format usable for report		
			Dim processDT As DataTable = Me.CreateReportDataTable(si,"processTable",processColumns,True)	
			Dim dArgs As New Dictionary(Of String, String)
			dArgs.Add("startYr",iTime.ToString)
			dArgs.Add("Cube",sCube)
			dArgs.Add("Entity",sEntity)
			dArgs.Add("Scenario",sScenario)
			Me.ProcessTableForReport(si, FetchDt, processDT, dArgs)
		
			'Generate & write File and update FvParam for filepath needed for file viewer
			Return Me.GenerateReportFile(si, processDT, sFileHeader, sCube, iTime, sTemplate,sFvParam)

		End Function
#End Region


	End Class
End Namespace
