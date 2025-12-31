Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Text
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
Imports Workspace.GBL.GBL_Assembly
Imports Microsoft.Data.SqlClient


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_PGM_Dyn_Grid_Svc
		Implements IWsasDynamicGridV800

		Private si As SessionInfo
        Public globals As BRGlobals
        Private workspace As DashboardWorkspace
        Private args As DashboardDynamicGridArgs
		Private api As Object
		
        Public Function GetDynamicGridData(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridGetDataResult Implements IWsasDynamicGridV800.GetDynamicGridData
			
            'Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.workspace = workspace
            Me.args = args	
			Me.api = api
			
        	Try
                If (Globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Imported_Req") Then
						Return dg_CMD_PGM_Imported_Req()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQList") Then
				    	Return dg_CMD_PGM_REQList()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Prioritize_REQ") Then
				    	Return dg_CMD_PGM_Prioritize_REQ()            
					End If
										
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_RolloverREQ") Then
				    	Return dg_CMD_PGM_Rollover_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQManpower") Then
				    	Return dg_CMD_PGM_REQManpower()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQManage") Then
				    	Return dg_CMD_PGM_REQManage()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQFundingLine") Then
				    	Return dg_CMD_PGM_REQFundingLine()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQAmtsReview") Then
				    	Return dg_CMD_PGM_REQAmtsReview()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQCertDetail") Then
				    	Return dg_CMD_PGM_REQCertDetail()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQCertSummary") Then
				    	Return dg_CMD_PGM_REQCertSummary()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Attachments") Then
				    	Return dg_CMD_PGM_Attachments()            
					End If
					
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Create") Then
				    	Return dg_CMD_PGM_Create()        
					End If
					
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		
		
#Region "SaveDynamicGridData"
        Public Function SaveDynamicGridData(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridSaveDataResult Implements IWsasDynamicGridV800.SaveDynamicGridData
            Me.si = si
            Me.globals = globals
            Me.workspace = workspace
            Me.args = args	
			Me.api = api
			
			Try
'Brapi.ErrorLog.LogMessage(si,"In Save")
                If (Globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
             
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Prioritize_REQ") Then
				    	Return Save_dg_CMD_PGM_Prioritize_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQFundingLine") Then
				    	Return Save_dg_CMD_PGM_REQFundingLine()            
					End If
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQManage") Then

				    	Return Save_dg_CMD_PGM_REQManage()           
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Create") Then
				    	Return Save_dg_CMD_PGM_Create()           
					End If
					
				End If
				
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
#End Region			

#Region "CMD PGM Import"
		Private Function dg_CMD_PGM_Imported_Req() As XFDynamicGridGetDataResult
		
			'Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim tableName As String = "CMD_PGM_Import_" & sScenario
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName,tableName)

			If dt Is Nothing Then Return Nothing
				
			Dim skp As New Dictionary(Of String, Object)
#Region "testing dynamic grid"			
''--------------------------------------------------------------------------------------------
'TEST 1 Example
'			Dim pageSize As Integer = 50
'			Dim currentPage As Integer = 2
'			Dim totalRows As Integer = dt.Rows.Count
'			Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(pageSize))))
			
			
'			Dim xfdt As XFDataTable = Nothing
'			xfdt = New XFDataTable(si,dt,Nothing,pageSize,currentPage)
'			BRApi.ErrorLog.LogMessage(si, "Totalpages: " & totalPages)
'			BRApi.ErrorLog.LogMessage(si, "CurrentPage: " & currentPage)
''----------------------------------------------------------------------------------------------
'TEST 2 Example

'			Dim pageSize As Integer = 50
'			Dim pageNumber As Integer = 1
'			Dim Skip As Integer = (pageNumber - 1) * pageSize
'			Dim rows = dt.AsEnumerable().Skip(Skip).Take(pageSize)
'			Dim pageDt As DataTable = If(rows.Any(), rows.CopyToDataTable(), dt.Clone())
'			Dim xfdt As New XFDataTable(si, pageDt, Nothing, pageDt.Rows.Count, 1)
'''----------------------------------------------------------------------------------------------
'TEST 3 Example
'			Dim pageSize As Integer = 50
'			Dim totalRows As Integer = dt.Rows.Count
'			Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(pageSize))))
'			Dim xfdt As XFDataTable = Nothing
'			Dim sourceDt As DataTable = dt
'			For pageNumber As Integer = 1 To totalPages
'				Dim Skip As Integer = (pageNumber - 1) * pageSize
'				Dim rows = sourceDt.AsEnumerable().Skip(Skip).Take(pageSize)
'				Dim pageDt As DataTable = If(rows.Any(), rows.CopyToDataTable(), sourceDt.Clone())
				
'				xfdt = New XFDataTable(si,dt,Nothing,pageDt.Rows.Count,1)
''				BRApi.ErrorLog.LogMessage(si, "CurrentPageNumber: " & pageNumber)

'			Next	
'''----------------------------------------------------------------------------------------------
'TEST 4 Example
'			Dim pageSize As Integer = 50
'			Dim totalRows As Integer = dt.Rows.Count
'			Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(pageSize))))
'			Dim xfdt As XFDataTable = Nothing
'			Dim pageNumber As Integer 
'			For pageNumber = 1 To totalPages
''				pageNumber+=1
'				xfdt = New XFDataTable(si,dt,Nothing,pagesize,pageNumber)
'				pageNumber+=1
'			Next
'				BRApi.ErrorLog.LogMessage(si, "pageNumber: " & pageNumber)
'				BRApi.ErrorLog.LogMessage(si, "totalPages: " & totalPages)
'				BRApi.ErrorLog.LogMessage(si, "totalRows: " & totalRows)



''			Dim xfdt As XFDataTable = New XFDataTable(si,dt,Nothing,pageSize,1)
''			Dim xfdt As XFDataTable = New XFDataTable("CMD_PGM_Import")
''			Dim xfdt As XFDataTable = New XFDataTable(si,dt,Nothing,20)
'''----------------------------------------------------------------------------------------------
'RETURNS FIRST 50 ROWS ON TWO DIFFERENT PAGES
'			Dim pageLimit As Integer = 50
'			Dim pageSize
'			Dim startPage
'			Dim pageNumber As Integer
'			Dim totalRows As Integer = dt.Rows.Count
'			Dim totalPages As Integer = Math.Max(1, CInt(Math.Ceiling(totalRows / CDbl(pageLimit))))
'			For startPage = 1 To totalPages
'				pageNumber+=1
'			Next
''			While startPage <= totalPages
''				pageNumber+=1
''			End While
'			Brapi.ErrorLog.LogMessage(si, "pageNumber: " & pageNumber)
'			Dim xfdt As XFDataTable = New XFDataTable(si,dt,Nothing,1000)
			
			
'''----------------------------------------------------------------------------------------------
'ORIGINAL CODE BELOW		
			
			
'			'Dim colColl As XFDataColumnCollection = New XFDataColumnCollection()
			
'			For Each c As DataColumn In dt.Columns
'				Dim newCol As  New XFDynamicGridColumnDefinition()
'				newCol.ColumnName = c.ColumnName
'				newCol.IsVisible = TriStateBool.TrueValue
'				coldefs.Add(newCol)
				
'				Dim xfCol As New XFDataColumn(c)
'				'xfdt.Columns.Add(xfCol)
'			Next
			
'	        For Each row As DataRow In dt.Rows
'	            Dim xfRow As New XFDataRow(si,xfdt.Columns,row,Nothing)
'	            xfdt.Rows.Add(xfRow)
'	        Next
#End Region

			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
#Region "columnDefinitions"			
			Dim ValidationError As New XFDynamicGridColumnDefinition()
			ValidationError.ColumnName = "Invalid Errors"
			ValidationError.IsFromTable = True
			ValidationError.IsVisible = True
			ValidationError.AllowUpdates = False
			columnDefinitions.Add(ValidationError)
			
						
			Dim REQ_ID As New XFDynamicGridColumnDefinition()
			REQ_ID.ColumnName = "REQ_ID"
			REQ_ID.IsFromTable = True
			REQ_ID.IsVisible = True
			REQ_ID.AllowUpdates = False
			columnDefinitions.Add(REQ_ID)
			
			Dim CMD_PGM_REQ_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_REQ_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_REQ_ID.IsFromTable = True
			CMD_PGM_REQ_ID.IsVisible = False
			CMD_PGM_REQ_ID.AllowUpdates = False
			columnDefinitions.Add(CMD_PGM_REQ_ID)
			
			Dim FundCenter As New XFDynamicGridColumnDefinition()
			FundCenter.ColumnName = "FundCenter"
			FundCenter.Description = "FundsCenter"
			FundCenter.IsFromTable = True
			FundCenter.IsVisible = True
			FundCenter.AllowUpdates = False
			columnDefinitions.Add(FundCenter)
			
			Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "APPN"
			APPN.IsFromTable = True
			APPN.IsVisible = True
			APPN.AllowUpdates = False
			columnDefinitions.Add(APPN)
			
			Dim MDEP As New XFDynamicGridColumnDefinition()
			MDEP.ColumnName = "MDEP"
			MDEP.IsFromTable = True
			MDEP.IsVisible = True
			MDEP.AllowUpdates = False
			columnDefinitions.Add(MDEP)
			
			Dim APE9 As New XFDynamicGridColumnDefinition()
			APE9.ColumnName = "APE9"
			APE9.IsFromTable = True
			APE9.IsVisible = True
			APE9.AllowUpdates = False
			columnDefinitions.Add(APE9)
			
			Dim DollarType As New XFDynamicGridColumnDefinition()
			DollarType.ColumnName = "DollarType"
			DollarType.IsFromTable = True
			DollarType.IsVisible = True
			DollarType.AllowUpdates = False
			columnDefinitions.Add(DollarType)
			
			Dim ObjectClass As New XFDynamicGridColumnDefinition()
			ObjectClass.ColumnName = "ObjectClass"
			ObjectClass.IsFromTable = True
			ObjectClass.IsVisible = True
			ObjectClass.AllowUpdates = False
			columnDefinitions.Add(ObjectClass)
			
			Dim C_Type As New XFDynamicGridColumnDefinition()
			C_Type.ColumnName = "Ctype"
			C_Type.IsFromTable = True
			C_Type.IsVisible = True
			C_Type.AllowUpdates = False
			columnDefinitions.Add(C_Type)
			
			Dim Cycle As New XFDynamicGridColumnDefinition()
			Cycle.ColumnName = "Cycle"
			Cycle.IsFromTable = True
			Cycle.IsVisible = True
			Cycle.AllowUpdates = False
			columnDefinitions.Add(Cycle)
			
			Dim FY1 As New XFDynamicGridColumnDefinition()
			FY1.ColumnName = "FY1"
			FY1.IsFromTable = True
			FY1.IsVisible = True
			FY1.AllowUpdates = False
			columnDefinitions.Add(FY1)
			
			Dim FY2 As New XFDynamicGridColumnDefinition()
			FY2.ColumnName = "FY2"
			FY2.IsFromTable = True
			FY2.IsVisible = True
			FY2.AllowUpdates = False
			columnDefinitions.Add(FY2)
			
			Dim FY3 As New XFDynamicGridColumnDefinition()
			FY3.ColumnName = "FY3"
			FY3.IsFromTable = True
			FY3.IsVisible = True
			FY3.AllowUpdates = False
			columnDefinitions.Add(FY3)
			
			Dim FY4 As New XFDynamicGridColumnDefinition()
			FY4.ColumnName = "FY4"
			FY4.IsFromTable = True
			FY4.IsVisible = True
			FY4.AllowUpdates = False
			columnDefinitions.Add(FY4)
			
			Dim FY5 As New XFDynamicGridColumnDefinition()
			FY5.ColumnName = "FY5"
			FY5.IsFromTable = True
			FY5.IsVisible = True
			FY5.AllowUpdates = False
			columnDefinitions.Add(FY5)
			
			Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
			Dim Description As New XFDynamicGridColumnDefinition()
			Description.ColumnName = "Description"
			Description.IsFromTable = True
			Description.IsVisible = True
			Description.AllowUpdates = False
			columnDefinitions.Add(Description)
			
			Dim Justification As New XFDynamicGridColumnDefinition()
			Justification.ColumnName = "Justification"
			Justification.IsFromTable = True
			Justification.IsVisible = True
			Justification.AllowUpdates = False
			columnDefinitions.Add(Justification)
			
			Dim CostMethodology As New XFDynamicGridColumnDefinition()
			CostMethodology.ColumnName = "CostMethodology"
			CostMethodology.IsFromTable = True
			CostMethodology.IsVisible = True
			CostMethodology.AllowUpdates = False
			columnDefinitions.Add(CostMethodology)
			
			Dim ImpactIfNotFunded As New XFDynamicGridColumnDefinition()
			ImpactIfNotFunded.ColumnName = "ImpactIfNotFunded"
			ImpactIfNotFunded.IsFromTable = True
			ImpactIfNotFunded.IsVisible = True
			ImpactIfNotFunded.AllowUpdates = False
			columnDefinitions.Add(ImpactIfNotFunded)
			
			Dim RiskIfNotFunded As New XFDynamicGridColumnDefinition()
			RiskIfNotFunded.ColumnName = "RiskIfNotFunded"
			RiskIfNotFunded.IsFromTable = True
			RiskIfNotFunded.IsVisible = True
			RiskIfNotFunded.AllowUpdates = False
			columnDefinitions.Add(RiskIfNotFunded)
			
			Dim CostGrowthJustification As New XFDynamicGridColumnDefinition()
			CostGrowthJustification.ColumnName = "CostGrowthJustification"
			CostGrowthJustification.IsFromTable = True
			CostGrowthJustification.IsVisible = True
			CostGrowthJustification.AllowUpdates = False
			columnDefinitions.Add(CostGrowthJustification)
			
			Dim MustFund As New XFDynamicGridColumnDefinition()
			MustFund.ColumnName = "MustFund"
			MustFund.IsFromTable = True
			MustFund.IsVisible = True
			MustFund.AllowUpdates = False
			columnDefinitions.Add(MustFund)
			
			Dim FundingSource As New XFDynamicGridColumnDefinition()
			FundingSource.ColumnName = "FundingSource"
			FundingSource.IsFromTable = True
			FundingSource.IsVisible = True
			FundingSource.AllowUpdates = False
			columnDefinitions.Add(FundingSource)
			
			Dim ArmyInitiative_Directive As New XFDynamicGridColumnDefinition()
			ArmyInitiative_Directive.ColumnName = "ArmyInitiative_Directive"
			ArmyInitiative_Directive.IsFromTable = True
			ArmyInitiative_Directive.IsVisible = True
			ArmyInitiative_Directive.AllowUpdates = False
			columnDefinitions.Add(ArmyInitiative_Directive)
			
			Dim CommandInitiative_Directive As New XFDynamicGridColumnDefinition()
			CommandInitiative_Directive.ColumnName = "CommandInitiative_Directive"
			CommandInitiative_Directive.IsFromTable = True
			CommandInitiative_Directive.IsVisible = True
			CommandInitiative_Directive.AllowUpdates = False
			columnDefinitions.Add(CommandInitiative_Directive)
			
			Dim Activity_Exercise As New XFDynamicGridColumnDefinition()
			Activity_Exercise.ColumnName = "Activity_Exercise"
			Activity_Exercise.IsFromTable = True
			Activity_Exercise.IsVisible = True
			Activity_Exercise.AllowUpdates = False
			columnDefinitions.Add(Activity_Exercise)
			
			Dim IT_CyberRequirement As New XFDynamicGridColumnDefinition()
			IT_CyberRequirement.ColumnName = "IT_CyberRequirement"
			IT_CyberRequirement.IsFromTable = True
			IT_CyberRequirement.IsVisible = True
			IT_CyberRequirement.AllowUpdates = False
			columnDefinitions.Add(IT_CyberRequirement)
			
			Dim UIC As New XFDynamicGridColumnDefinition()
			UIC.ColumnName = "UIC"
			UIC.IsFromTable = True
			UIC.IsVisible = True
			UIC.AllowUpdates = False
			columnDefinitions.Add(UIC)
			
			Dim FLEX1 As New XFDynamicGridColumnDefinition()
			FLEX1.ColumnName = "FLEX1"
			FLEX1.IsFromTable = True
			FLEX1.IsVisible = True
			FLEX1.AllowUpdates = False
			columnDefinitions.Add(FLEX1)
			
			Dim FLEX2 As New XFDynamicGridColumnDefinition()
			FLEX2.ColumnName = "FLEX2"
			FLEX2.IsFromTable = True
			FLEX2.IsVisible = True
			FLEX2.AllowUpdates = False
			columnDefinitions.Add(FLEX2)
			
			Dim FLEX3 As New XFDynamicGridColumnDefinition()
			FLEX3.ColumnName = "FLEX3"
			FLEX3.IsFromTable = True
			FLEX3.IsVisible = True
			FLEX3.AllowUpdates = False
			columnDefinitions.Add(FLEX3)
			
			Dim FLEX4 As New XFDynamicGridColumnDefinition()
			FLEX4.ColumnName = "FLEX4"
			FLEX4.IsFromTable = True
			FLEX4.IsVisible = True
			FLEX4.AllowUpdates = False
			columnDefinitions.Add(FLEX4)
			
			Dim FLEX5 As New XFDynamicGridColumnDefinition()
			FLEX5.ColumnName = "FLEX5"
			FLEX5.IsFromTable = True
			FLEX5.IsVisible = True
			FLEX5.AllowUpdates = False
			columnDefinitions.Add(FLEX5)
			
			Dim EmergingRequirement_ER As New XFDynamicGridColumnDefinition()
			EmergingRequirement_ER.ColumnName = "EmergingRequirement_ER"
			EmergingRequirement_ER.IsFromTable = True
			EmergingRequirement_ER.IsVisible = True
			EmergingRequirement_ER.AllowUpdates = False
			columnDefinitions.Add(EmergingRequirement_ER)
			
			Dim CPATopic As New XFDynamicGridColumnDefinition()
			CPATopic.ColumnName = "CPATopic"
			CPATopic.IsFromTable = True
			CPATopic.IsVisible = True
			CPATopic.AllowUpdates = False
			columnDefinitions.Add(CPATopic)
			
			Dim PBRSubmission As New XFDynamicGridColumnDefinition()
			PBRSubmission.ColumnName = "PBRSubmission"
			PBRSubmission.IsFromTable = True
			PBRSubmission.IsVisible = True
			PBRSubmission.AllowUpdates = False
			columnDefinitions.Add(PBRSubmission)
			
			Dim UPLSubmission As New XFDynamicGridColumnDefinition()
			UPLSubmission.ColumnName = "UPLSubmission"
			UPLSubmission.IsFromTable = True
			UPLSubmission.IsVisible = True
			UPLSubmission.AllowUpdates = False
			columnDefinitions.Add(UPLSubmission)
			
			Dim ContractNumber As New XFDynamicGridColumnDefinition()
			ContractNumber.ColumnName = "ContractNumber"
			ContractNumber.IsFromTable = True
			ContractNumber.IsVisible = True
			ContractNumber.AllowUpdates = False
			columnDefinitions.Add(ContractNumber)
			
			Dim TaskOrderNumber As New XFDynamicGridColumnDefinition()
			TaskOrderNumber.ColumnName = "TaskOrderNumber"
			TaskOrderNumber.IsFromTable = True
			TaskOrderNumber.IsVisible = True
			TaskOrderNumber.AllowUpdates = False
			columnDefinitions.Add(TaskOrderNumber)
			
			Dim AwardTargetDate As New XFDynamicGridColumnDefinition()
			AwardTargetDate.ColumnName = "AwardTargetDate"
			AwardTargetDate.IsFromTable = True
			AwardTargetDate.IsVisible = True
			AwardTargetDate.AllowUpdates = False
			columnDefinitions.Add(AwardTargetDate)
			
			Dim POPExpirationDate As New XFDynamicGridColumnDefinition()
			POPExpirationDate.ColumnName = "POPExpirationDate"
			POPExpirationDate.IsFromTable = True
			POPExpirationDate.IsVisible = True
			POPExpirationDate.AllowUpdates = False
			columnDefinitions.Add(POPExpirationDate)
			
			Dim ContractManYearEquiv_CME As New XFDynamicGridColumnDefinition()
			ContractManYearEquiv_CME.ColumnName = "ContractManYearEquiv_CME"
			ContractManYearEquiv_CME.IsFromTable = True
			ContractManYearEquiv_CME.IsVisible = True
			ContractManYearEquiv_CME.AllowUpdates = False
			columnDefinitions.Add(ContractManYearEquiv_CME)
			
			Dim COREmail As New XFDynamicGridColumnDefinition()
			COREmail.ColumnName = "COREmail"
			COREmail.IsFromTable = True
			COREmail.IsVisible = True
			COREmail.AllowUpdates = False
			columnDefinitions.Add(COREmail)
			
			Dim POCEmail As New XFDynamicGridColumnDefinition()
			POCEmail.ColumnName = "POCEmail"
			POCEmail.IsFromTable = True
			POCEmail.IsVisible = True
			POCEmail.AllowUpdates = False
			columnDefinitions.Add(POCEmail)
			
			Dim Directorate As New XFDynamicGridColumnDefinition()
			Directorate.ColumnName = "Directorate"
			Directorate.IsFromTable = True
			Directorate.IsVisible = True
			Directorate.AllowUpdates = False
			columnDefinitions.Add(Directorate)
			
			Dim Division As New XFDynamicGridColumnDefinition()
			Division.ColumnName = "Division"
			Division.IsFromTable = True
			Division.IsVisible = True
			Division.AllowUpdates = False
			columnDefinitions.Add(Division)
			
			Dim Branch As New XFDynamicGridColumnDefinition()
			Branch.ColumnName = "Branch"
			Branch.IsFromTable = True
			Branch.IsVisible = True
			Branch.AllowUpdates = False
			columnDefinitions.Add(Branch)
			
			Dim ReviewingPOCEmail As New XFDynamicGridColumnDefinition()
			ReviewingPOCEmail.ColumnName = "ReviewingPOCEmail"
			ReviewingPOCEmail.IsFromTable = True
			ReviewingPOCEmail.IsVisible = True
			ReviewingPOCEmail.AllowUpdates = False
			columnDefinitions.Add(ReviewingPOCEmail)
			
			Dim MDEPFunctionalEmail As New XFDynamicGridColumnDefinition()
			MDEPFunctionalEmail.ColumnName = "MDEPFunctionalEmail"
			MDEPFunctionalEmail.IsFromTable = True
			MDEPFunctionalEmail.IsVisible = True
			MDEPFunctionalEmail.AllowUpdates = False
			columnDefinitions.Add(MDEPFunctionalEmail)
			
			Dim NotificationEmailList As New XFDynamicGridColumnDefinition()
			NotificationEmailList.ColumnName = "NotificationEmailList"
			NotificationEmailList.IsFromTable = True
			NotificationEmailList.IsVisible = True
			NotificationEmailList.AllowUpdates = False
			columnDefinitions.Add(NotificationEmailList)
			
			Dim GeneralComments_Notes As New XFDynamicGridColumnDefinition()
			GeneralComments_Notes.ColumnName = "GeneralComments_Notes"
			GeneralComments_Notes.IsFromTable = True
			GeneralComments_Notes.IsVisible = True
			GeneralComments_Notes.AllowUpdates = False
			columnDefinitions.Add(GeneralComments_Notes)
			
			Dim JUON As New XFDynamicGridColumnDefinition()
			JUON.ColumnName = "JUON"
			JUON.IsFromTable = True
			JUON.IsVisible = True
			JUON.AllowUpdates = False
			columnDefinitions.Add(JUON)
			
			Dim ISR_Flag As New XFDynamicGridColumnDefinition()
			ISR_Flag.ColumnName = "ISR_Flag"
			ISR_Flag.IsFromTable = True
			ISR_Flag.IsVisible = True
			ISR_Flag.AllowUpdates = False
			columnDefinitions.Add(ISR_Flag)
			
			Dim Cost_Model As New XFDynamicGridColumnDefinition()
			Cost_Model.ColumnName = "Cost_Model"
			Cost_Model.IsFromTable = True
			Cost_Model.IsVisible = True
			Cost_Model.AllowUpdates = False
			columnDefinitions.Add(Cost_Model)
			
			Dim Combat_Lost As New XFDynamicGridColumnDefinition()
			Combat_Lost.ColumnName = "Combat_Lost"
			Combat_Lost.IsFromTable = True
			Combat_Lost.IsVisible = True
			Combat_Lost.AllowUpdates = False
			columnDefinitions.Add(Combat_Lost)
			
			Dim Cost_Location As New XFDynamicGridColumnDefinition()
			Cost_Location.ColumnName = "Cost_Location"
			Cost_Location.IsFromTable = True
			Cost_Location.IsVisible = True
			Cost_Location.AllowUpdates = False
			columnDefinitions.Add(Cost_Location)
			
			Dim Category_A_Code As New XFDynamicGridColumnDefinition()
			Category_A_Code.ColumnName = "Category_A_Code"
			Category_A_Code.IsFromTable = True
			Category_A_Code.IsVisible = True
			Category_A_Code.AllowUpdates = False
			columnDefinitions.Add(Category_A_Code)
			
			Dim CBS_Code As New XFDynamicGridColumnDefinition()
			CBS_Code.ColumnName = "CBS_Code"
			CBS_Code.IsFromTable = True
			CBS_Code.IsVisible = True
			CBS_Code.AllowUpdates = False
			columnDefinitions.Add(CBS_Code)
			
			Dim MIP_Proj_Code As New XFDynamicGridColumnDefinition()
			MIP_Proj_Code.ColumnName = "MIP_Proj_Code"
			MIP_Proj_Code.IsFromTable = True
			MIP_Proj_Code.IsVisible = True
			MIP_Proj_Code.AllowUpdates = False
			columnDefinitions.Add(MIP_Proj_Code)
			
			Dim SS_Priority As New XFDynamicGridColumnDefinition()
			SS_Priority.ColumnName = "SS_Priority"
			SS_Priority.IsFromTable = True
			SS_Priority.IsVisible = True
			SS_Priority.AllowUpdates = False
			columnDefinitions.Add(SS_Priority)
			
			Dim Commitment_Group As New XFDynamicGridColumnDefinition()
			Commitment_Group.ColumnName = "Commitment_Group"
			Commitment_Group.IsFromTable = True
			Commitment_Group.IsVisible = True
			Commitment_Group.AllowUpdates = False
			columnDefinitions.Add(Commitment_Group)
			
			Dim SS_Capability As New XFDynamicGridColumnDefinition()
			SS_Capability.ColumnName = "SS_Capability"
			SS_Capability.IsFromTable = True
			SS_Capability.IsVisible = True
			SS_Capability.AllowUpdates = False
			columnDefinitions.Add(SS_Capability)
			
			Dim Strategic_BIN As New XFDynamicGridColumnDefinition()
			Strategic_BIN.ColumnName = "Strategic_BIN"
			Strategic_BIN.IsFromTable = True
			Strategic_BIN.IsVisible = True
			Strategic_BIN.AllowUpdates = False
			columnDefinitions.Add(Strategic_BIN)
			
			Dim LIN As New XFDynamicGridColumnDefinition()
			LIN.ColumnName = "LIN"
			LIN.IsFromTable = True
			LIN.IsVisible = True
			LIN.AllowUpdates = False
			columnDefinitions.Add(LIN)
			
			Dim FY1_QTY As New XFDynamicGridColumnDefinition()
			FY1_QTY.ColumnName = "FY1_QTY"
			FY1_QTY.IsFromTable = True
			FY1_QTY.IsVisible = True
			FY1_QTY.AllowUpdates = False
			columnDefinitions.Add(FY1_QTY)
			
			Dim FY2_QTY As New XFDynamicGridColumnDefinition()
			FY2_QTY.ColumnName = "FY2_QTY"
			FY2_QTY.IsFromTable = True
			FY2_QTY.IsVisible = True
			FY2_QTY.AllowUpdates = False
			columnDefinitions.Add(FY2_QTY)
			
			Dim FY3_QTY As New XFDynamicGridColumnDefinition()
			FY3_QTY.ColumnName = "FY3_QTY"
			FY3_QTY.IsFromTable = True
			FY3_QTY.IsVisible = True
			FY3_QTY.AllowUpdates = False
			columnDefinitions.Add(FY3_QTY)
			
			Dim FY4_QTY As New XFDynamicGridColumnDefinition()
			FY4_QTY.ColumnName = "FY4_QTY"
			FY4_QTY.IsFromTable = True
			FY4_QTY.IsVisible = True
			FY4_QTY.AllowUpdates = False
			columnDefinitions.Add(FY4_QTY)
			
			Dim FY5_QTY As New XFDynamicGridColumnDefinition()
			FY5_QTY.ColumnName = "FY5_QTY"
			FY5_QTY.IsFromTable = True
			FY5_QTY.IsVisible = True
			FY5_QTY.AllowUpdates = False
			columnDefinitions.Add(FY5_QTY)
			
			Dim RequirementType As New XFDynamicGridColumnDefinition()
			RequirementType.ColumnName = "RequirementType"
			RequirementType.IsFromTable = True
			RequirementType.IsVisible = True
			RequirementType.AllowUpdates = False
			columnDefinitions.Add(RequirementType)
			
			Dim DD_Priority As New XFDynamicGridColumnDefinition()
			DD_Priority.ColumnName = "DD_Priority"
			DD_Priority.IsFromTable = True
			DD_Priority.IsVisible = True
			DD_Priority.AllowUpdates = False
			columnDefinitions.Add(DD_Priority)
			
			Dim Portfolio As New XFDynamicGridColumnDefinition()
			Portfolio.ColumnName = "Portfolio"
			Portfolio.IsFromTable = True
			Portfolio.IsVisible = True
			Portfolio.AllowUpdates = False
			columnDefinitions.Add(Portfolio)
			
			Dim DD_Capability As New XFDynamicGridColumnDefinition()
			DD_Capability.ColumnName = "DD_Capability"
			DD_Capability.IsFromTable = True
			DD_Capability.IsVisible = True
			DD_Capability.AllowUpdates = False
			columnDefinitions.Add(DD_Capability)
			
			Dim JNT_CAP_AREA As New XFDynamicGridColumnDefinition()
			JNT_CAP_AREA.ColumnName = "JNT_CAP_AREA"
			JNT_CAP_AREA.IsFromTable = True
			JNT_CAP_AREA.IsVisible = True
			JNT_CAP_AREA.AllowUpdates = False
			columnDefinitions.Add(JNT_CAP_AREA)
			
			Dim TBM_COST_POOL As New XFDynamicGridColumnDefinition()
			TBM_COST_POOL.ColumnName = "TBM_COST_POOL"
			TBM_COST_POOL.IsFromTable = True
			TBM_COST_POOL.IsVisible = True
			TBM_COST_POOL.AllowUpdates = False
			columnDefinitions.Add(TBM_COST_POOL)
			
			Dim TBM_TOWER As New XFDynamicGridColumnDefinition()
			TBM_TOWER.ColumnName = "TBM_TOWER"
			TBM_TOWER.IsFromTable = True
			TBM_TOWER.IsVisible = True
			TBM_TOWER.AllowUpdates = False
			columnDefinitions.Add(TBM_TOWER)
			
			Dim APMSAITRNum As New XFDynamicGridColumnDefinition()
			APMSAITRNum.ColumnName = "APMSAITRNum"
			APMSAITRNum.IsFromTable = True
			APMSAITRNum.IsVisible = True
			APMSAITRNum.AllowUpdates = False
			columnDefinitions.Add(APMSAITRNum)
			
			Dim ZERO_TRUST_CAPABILITY As New XFDynamicGridColumnDefinition()
			ZERO_TRUST_CAPABILITY.ColumnName = "ZERO_TRUST_CAPABILITY"
			ZERO_TRUST_CAPABILITY.IsFromTable = True
			ZERO_TRUST_CAPABILITY.IsVisible = True
			ZERO_TRUST_CAPABILITY.AllowUpdates = False
			columnDefinitions.Add(ZERO_TRUST_CAPABILITY)
			
			Dim ASSOCIATED_DIRECTIVES As New XFDynamicGridColumnDefinition()
			ASSOCIATED_DIRECTIVES.ColumnName = "ASSOCIATED_DIRECTIVES"
			ASSOCIATED_DIRECTIVES.IsFromTable = True
			ASSOCIATED_DIRECTIVES.IsVisible = True
			ASSOCIATED_DIRECTIVES.AllowUpdates = False
			columnDefinitions.Add(ASSOCIATED_DIRECTIVES)
			
			Dim CLOUD_INDICATOR As New XFDynamicGridColumnDefinition()
			CLOUD_INDICATOR.ColumnName = "CLOUD_INDICATOR"
			CLOUD_INDICATOR.IsFromTable = True
			CLOUD_INDICATOR.IsVisible = True
			CLOUD_INDICATOR.AllowUpdates = False
			columnDefinitions.Add(CLOUD_INDICATOR)
			
			Dim STRAT_CYBERSEC_PGRM As New XFDynamicGridColumnDefinition()
			STRAT_CYBERSEC_PGRM.ColumnName = "STRAT_CYBERSEC_PGRM"
			STRAT_CYBERSEC_PGRM.IsFromTable = True
			STRAT_CYBERSEC_PGRM.IsVisible = True
			STRAT_CYBERSEC_PGRM.AllowUpdates = False
			columnDefinitions.Add(STRAT_CYBERSEC_PGRM)
			
			Dim NOTES As New XFDynamicGridColumnDefinition()
			NOTES.ColumnName = "NOTES"
			NOTES.IsFromTable = True
			NOTES.IsVisible = True
			NOTES.AllowUpdates = False
			columnDefinitions.Add(NOTES)
			
			Dim UNIT_OF_MEASURE As New XFDynamicGridColumnDefinition()
			UNIT_OF_MEASURE.ColumnName = "UNIT_OF_MEASURE"
			UNIT_OF_MEASURE.IsFromTable = True
			UNIT_OF_MEASURE.IsVisible = True
			UNIT_OF_MEASURE.AllowUpdates = False
			columnDefinitions.Add(UNIT_OF_MEASURE)
			
			Dim FY1_Items As New XFDynamicGridColumnDefinition()
			FY1_Items.ColumnName = "FY1_Items"
			FY1_Items.IsFromTable = True
			FY1_Items.IsVisible = True
			FY1_Items.AllowUpdates = False
			columnDefinitions.Add(FY1_Items)
			
			Dim FY1_Unit_Cost As New XFDynamicGridColumnDefinition()
			FY1_Unit_Cost.ColumnName = "FY1_Unit_Cost"
			FY1_Unit_Cost.IsFromTable = True
			FY1_Unit_Cost.IsVisible = True
			FY1_Unit_Cost.AllowUpdates = False
			columnDefinitions.Add(FY1_Unit_Cost)
			
			Dim FY2_Items As New XFDynamicGridColumnDefinition()
			FY2_Items.ColumnName = "FY2_Items"
			FY2_Items.IsFromTable = True
			FY2_Items.IsVisible = True
			FY2_Items.AllowUpdates = False
			columnDefinitions.Add(FY2_Items)
			
			Dim FY2_Unit_Cost As New XFDynamicGridColumnDefinition()
			FY2_Unit_Cost.ColumnName = "FY2_Unit_Cost"
			FY2_Unit_Cost.IsFromTable = True
			FY2_Unit_Cost.IsVisible = True
			FY2_Unit_Cost.AllowUpdates = False
			columnDefinitions.Add(FY2_Unit_Cost)
			
			Dim FY3_Items As New XFDynamicGridColumnDefinition()
			FY3_Items.ColumnName = "FY3_Items"
			FY3_Items.IsFromTable = True
			FY3_Items.IsVisible = True
			FY3_Items.AllowUpdates = False
			columnDefinitions.Add(FY3_Items)
			
			Dim FY3_Unit_Cost As New XFDynamicGridColumnDefinition()
			FY3_Unit_Cost.ColumnName = "FY3_Unit_Cost"
			FY3_Unit_Cost.IsFromTable = True
			FY3_Unit_Cost.IsVisible = True
			FY3_Unit_Cost.AllowUpdates = False
			columnDefinitions.Add(FY3_Unit_Cost)
			
			Dim FY4_Items As New XFDynamicGridColumnDefinition()
			FY4_Items.ColumnName = "FY4_Items"
			FY4_Items.IsFromTable = True
			FY4_Items.IsVisible = True
			FY4_Items.AllowUpdates = False
			columnDefinitions.Add(FY4_Items)
			
			Dim FY4_Unit_Cost As New XFDynamicGridColumnDefinition()
			FY4_Unit_Cost.ColumnName = "FY4_Unit_Cost"
			FY4_Unit_Cost.IsFromTable = True
			FY4_Unit_Cost.IsVisible = True
			FY4_Unit_Cost.AllowUpdates = False
			columnDefinitions.Add(FY4_Unit_Cost)
			
			Dim FY5_Items As New XFDynamicGridColumnDefinition()
			FY5_Items.ColumnName = "FY5_Items"
			FY5_Items.IsFromTable = True
			FY5_Items.IsVisible = True
			FY5_Items.AllowUpdates = False
			columnDefinitions.Add(FY5_Items)
			
			Dim FY5_Unit_Cost As New XFDynamicGridColumnDefinition()
			FY5_Unit_Cost.ColumnName = "FY5_Unit_Cost"
			FY5_Unit_Cost.IsFromTable = True
			FY5_Unit_Cost.IsVisible = True
			FY5_Unit_Cost.AllowUpdates = False
			columnDefinitions.Add(FY5_Unit_Cost)
			
			Dim REQ_ID_Type As New XFDynamicGridColumnDefinition()
			REQ_ID_Type.ColumnName = "REQ_ID_Type"
			REQ_ID_Type.IsFromTable = True
			REQ_ID_Type.IsVisible = True
			REQ_ID_Type.AllowUpdates = False
			columnDefinitions.Add(REQ_ID_Type)
			
			Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Status"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.AllowUpdates = False
			columnDefinitions.Add(Status)
			
			Dim Attach_File_Name As New XFDynamicGridColumnDefinition()
			Attach_File_Name.ColumnName = "Attach_File_Name"
			Attach_File_Name.IsFromTable = True
			Attach_File_Name.IsVisible = True
			Attach_File_Name.AllowUpdates = False
			columnDefinitions.Add(Attach_File_Name)
			
			Dim Attach_File_Bytes As New XFDynamicGridColumnDefinition()
			Attach_File_Bytes.ColumnName = "Attach_File_Bytes"
			Attach_File_Bytes.IsFromTable = True
			Attach_File_Bytes.IsVisible = True
			Attach_File_Bytes.AllowUpdates = False
			columnDefinitions.Add(Attach_File_Bytes)
			
			Dim Invalid As New XFDynamicGridColumnDefinition()
			Invalid.ColumnName = "Invalid"
			Invalid.IsFromTable = True
			Invalid.IsVisible = True
			Invalid.AllowUpdates = False
			columnDefinitions.Add(Invalid)
			
			Dim Create_Date As New XFDynamicGridColumnDefinition()
			Create_Date.ColumnName = "Create_Date"
			Create_Date.IsFromTable = True
			Create_Date.IsVisible = True
			Create_Date.AllowUpdates = False
			columnDefinitions.Add(Create_Date)
			
			Dim Create_User As New XFDynamicGridColumnDefinition()
			Create_User.ColumnName = "Create_User"
			Create_User.IsFromTable = True
			Create_User.IsVisible = True
			Create_User.AllowUpdates = False
			columnDefinitions.Add(Create_User)
			
			Dim Update_Date As New XFDynamicGridColumnDefinition()
			Update_Date.ColumnName = "Update_Date"
			Update_Date.IsFromTable = True
			Update_Date.IsVisible = True
			Update_Date.AllowUpdates = False
			columnDefinitions.Add(Update_Date)
			
			Dim Update_User As New XFDynamicGridColumnDefinition()
			Update_User.ColumnName = "Update_User"
			Update_User.IsFromTable = True
			Update_User.IsVisible = True
			Update_User.AllowUpdates = False
			columnDefinitions.Add(Update_User)
			
			Dim WFScenario_Name As New XFDynamicGridColumnDefinition()
			WFScenario_Name.ColumnName = "WFScenario_Name"
			WFScenario_Name.IsFromTable = True
			WFScenario_Name.IsVisible = True
			WFScenario_Name.AllowUpdates = False
			columnDefinitions.Add(WFScenario_Name)
			
			Dim WFCMD_Name As New XFDynamicGridColumnDefinition()
			WFCMD_Name.ColumnName = "WFCMD_Name"
			WFCMD_Name.IsFromTable = True
			WFCMD_Name.IsVisible = True
			WFCMD_Name.AllowUpdates = False
			columnDefinitions.Add(WFCMD_Name)
			
			Dim WFTime_Name As New XFDynamicGridColumnDefinition()
			WFTime_Name.ColumnName = "WFTime_Name"
			WFTime_Name.IsFromTable = True
			WFTime_Name.IsVisible = True
			WFTime_Name.AllowUpdates = False
			columnDefinitions.Add(WFTime_Name)
			
			Dim IC As New XFDynamicGridColumnDefinition()
			IC.ColumnName = "IC"
			IC.IsFromTable = True
			IC.IsVisible = True
			IC.AllowUpdates = False
			columnDefinitions.Add(IC)
			
			Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = True
			Account.AllowUpdates = False
			columnDefinitions.Add(Account)
			
			Dim Flow As New XFDynamicGridColumnDefinition()
			Flow.ColumnName = "Flow"
			Flow.IsFromTable = True
			Flow.IsVisible = True
			Flow.AllowUpdates = False
			columnDefinitions.Add(Flow)
			
			Dim UD7 As New XFDynamicGridColumnDefinition()
			UD7.ColumnName = "UD7"
			UD7.IsFromTable = True
			UD7.IsVisible = True
			UD7.AllowUpdates = False
			columnDefinitions.Add(UD7)
			
			Dim UD8 As New XFDynamicGridColumnDefinition()
			UD8.ColumnName = "UD8"
			UD8.IsFromTable = True
			UD8.IsVisible = True
			UD8.AllowUpdates = False
			columnDefinitions.Add(UD8)
			
			Dim FY_Total As New XFDynamicGridColumnDefinition()
			FY_Total.ColumnName = "FY_Total"
			FY_Total.IsFromTable = True
			FY_Total.IsVisible = True
			FY_Total.AllowUpdates = False
			columnDefinitions.Add(FY_Total)
			
			Dim AllowUpdate As New XFDynamicGridColumnDefinition()
			AllowUpdate.ColumnName = "AllowUpdate"
			AllowUpdate.IsFromTable = True
			AllowUpdate.IsVisible = True
			AllowUpdate.AllowUpdates = False
			columnDefinitions.Add(AllowUpdate)


#End Region
'Pass table into Filter Function
			Dim Filteredtable As DataTable = Me.FilterTable(dt)
			
			Dim xfdt As New XFDataTable(si,Filteredtable,Nothing,10000)
			Dim rslt As New XFDynamicGridGetDataResult(xfdt,columnDefinitions,DataAccessLevel.AllAccess)
'BRApi.ErrorLog.LogMessage(si, "row count: " & rslt.DataTable.Rows.Count )
			Return rslt
			
		End Function
#End Region	
					
#Region "Constants"
Private BR_CMD_PGMDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_PGM_DataSet.MainClass
#End Region	

#Region "REQ List"
Private Function dg_CMD_PGM_REQList() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		


Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = False
			Account.AllowUpdates = False

			columnDefinitions.Add(Account)
Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_PGM_ID)
    
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			columnDefinitions.Add(FY_5)
			
	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Status"
			Status.FilterByColumnName = "Flow"
			columnDefinitions.Add(Status)
						
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars.XFGetValue("IV_FundsCenter","NA")
						Dim sAPPN As String  = args.CustomSubstVars.XFGetValue("IV_APPN","NA")
						Dim sSAG As String  = args.CustomSubstVars.XFGetValue("IV_SAG","NA")
						Dim sMDEP As String  = args.CustomSubstVars.XFGetValue("IV_MDEP","NA")
						Dim review_Type As String  = args.CustomSubstVars.XFGetValue("DL_CMD_PGM_Sum_Review_CV","NA")
					'Brapi.ErrorLog.LogMessage(si,"Hit: " & review_Type)
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
						dargs.NameValuePairs.XFSetValue("APPN", sAPPN)
						dargs.NameValuePairs.XFSetValue("SAG", sSAG)
						dargs.NameValuePairs.XFSetValue("MDEP", sMDEP)
						
						objdt = BR_CMD_PGMDataSet.Main(si, globals, api, dargs)
						
			'Pass table into Filter Function
						Dim Filteredtable As DataTable = Me.FilterTable(objdt)
										  				
					' Create the XFTable
					Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function
#End Region

#Region "PGM Cert Details"
Private Function dg_CMD_PGM_REQCertDetail() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		


    
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			columnDefinitions.Add(FY_5)
			
	Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Entity"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)			
			
	 Dim UD1 As New XFDynamicGridColumnDefinition()
			UD1.ColumnName = "UD1"
			UD1.IsFromTable = True
			UD1.IsVisible = True
			UD1.Description = "APPN"
			columnDefinitions.Add(UD1)
			
	Dim UD2 As New XFDynamicGridColumnDefinition()
			UD2.ColumnName = "UD2"
			UD2.IsFromTable = True
			UD2.IsVisible = True
			UD2.Description = "MDEP"
			columnDefinitions.Add(UD2)
			
	Dim UD3 As New XFDynamicGridColumnDefinition()
			UD3.ColumnName = "UD3"
			UD3.IsFromTable = True
			UD3.IsVisible = True
			UD3.Description = "APE9"
			columnDefinitions.Add(UD3)	
			
	Dim UD4 As New XFDynamicGridColumnDefinition()
			UD4.ColumnName = "UD4"
			UD4.IsFromTable = True
			UD4.IsVisible = True
			UD4.Description = "Dollar Type"
			columnDefinitions.Add(UD4)	
			
	Dim UD5 As New XFDynamicGridColumnDefinition()
			UD5.ColumnName = "UD5"
			UD5.IsFromTable = True
			UD5.IsVisible = True
			UD5.Description = "CType"
			columnDefinitions.Add(UD5)	
			
	Dim UD6 As New XFDynamicGridColumnDefinition()
			UD6.ColumnName = "UD6"
			UD6.IsFromTable = True
			UD6.IsVisible = True
			UD6.Description = "Object Class"
			columnDefinitions.Add(UD6)			
						
							
Dim dt As New DataTable()
			Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)		
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
			
            Dim sql As String =$"SELECT  
									Req.Entity,
									REQ_ID,
									Dtl.UD1,
									Dtl.UD2,
									Dtl.UD3,
									Dtl.UD4,
									Dtl.UD5,
									Dtl.UD6,
									FORMAT(Dtl.FY_1, 'N2') As FY_1,
							         FORMAT(Dtl.FY_2, 'N2') AS FY_2,
							         FORMAT(Dtl.FY_3, 'N2') AS FY_3,
							          FORMAT(Dtl.FY_4, 'N2') AS FY_4,
							          FORMAT(Dtl.FY_5, 'N2') AS FY_5,
									Req.Title,
									Req.Description,
									Req.Emerging_REQ, 
									Req. CPA_Topic, 
									Dtl.Flow  
								FROM XFC_CMD_PGM_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_PGM_REQ AS Req
									ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
									AND Req.WFScenario_Name = Dtl.WFScenario_Name
									AND Req.WFCMD_Name = Dtl.WFCMD_Name
									AND Req.WFTime_Name = Dtl.WFTime_Name
									AND Req.Entity = Dtl.Entity
								WHERE Flow = 'L2_Final_PGM'
								AND	Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			'Pass table into Filter Function
						Dim Filteredtable As DataTable = Me.FilterTable(dt)

'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function		

#End Region

#Region "PGM Cert Summary"
Private Function dg_CMD_PGM_REQCertSummary() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		


    
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			columnDefinitions.Add(FY_5)
			
	Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Entity"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)			
			
	 Dim UD1 As New XFDynamicGridColumnDefinition()
			UD1.ColumnName = "UD1"
			UD1.IsFromTable = True
			UD1.IsVisible = True
			UD1.Description = "APPN"
			columnDefinitions.Add(UD1)
			
	Dim UD2 As New XFDynamicGridColumnDefinition()
			UD2.ColumnName = "UD2"
			UD2.IsFromTable = True
			UD2.IsVisible = True
			UD2.Description = "MDEP"
			columnDefinitions.Add(UD2)
			
	Dim UD3 As New XFDynamicGridColumnDefinition()
			UD3.ColumnName = "UD3"
			UD3.IsFromTable = True
			UD3.IsVisible = True
			UD3.Description = "APE9"
			columnDefinitions.Add(UD3)	
			
	Dim UD4 As New XFDynamicGridColumnDefinition()
			UD4.ColumnName = "UD4"
			UD4.IsFromTable = True
			UD4.IsVisible = True
			UD4.Description = "Dollar Type"
			columnDefinitions.Add(UD4)	
			
	Dim UD5 As New XFDynamicGridColumnDefinition()
			UD5.ColumnName = "UD5"
			UD5.IsFromTable = True
			UD5.IsVisible = True
			UD5.Description = "CType"
			columnDefinitions.Add(UD5)	
			
	Dim UD6 As New XFDynamicGridColumnDefinition()
			UD6.ColumnName = "UD6"
			UD6.IsFromTable = True
			UD6.IsVisible = True
			UD6.Description = "Object Class"
			columnDefinitions.Add(UD6)			
						
							
Dim dt As New DataTable()
			Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)		
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
			
            Dim sql As String =$"SELECT  
									Req.Entity,
									REQ_ID,
									Dtl.UD1,
									Dtl.UD2,
									Dtl.UD3,
									Dtl.UD4,
									Dtl.UD5,
									Dtl.UD6,
									FORMAT(Dtl.FY_1, 'N2') As FY_1,
							         FORMAT(Dtl.FY_2, 'N2') AS FY_2,
							         FORMAT(Dtl.FY_3, 'N2') AS FY_3,
							          FORMAT(Dtl.FY_4, 'N2') AS FY_4,
							          FORMAT(Dtl.FY_5, 'N2') AS FY_5
									Req.Title,
									Req.Description,
									Req.Emerging_REQ, 
									Req. CPA_Topic, 
									Dtl.Flow  
								FROM XFC_CMD_PGM_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_PGM_REQ AS Req
									ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
									AND Req.WFScenario_Name = Dtl.WFScenario_Name
									AND Req.WFCMD_Name = Dtl.WFCMD_Name
									AND Req.WFTime_Name = Dtl.WFTime_Name
									AND Req.Entity = Dtl.Entity
								WHERE Flow = 'L2_Final_PGM'
								AND	Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								"
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			'Pass table into Filter Function
						Dim Filteredtable As DataTable = Me.FilterTable(dt)
						
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function		

#End Region

#Region "Manpower"
Private Function dg_CMD_PGM_REQManpower() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		


    
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			columnDefinitions.Add(FY_5)
			
	Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Entity"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)			
			
	 Dim UD1 As New XFDynamicGridColumnDefinition()
			UD1.ColumnName = "UD1"
			UD1.IsFromTable = True
			UD1.IsVisible = True
			UD1.Description = "APPN"
			columnDefinitions.Add(UD1)
			
	Dim UD2 As New XFDynamicGridColumnDefinition()
			UD2.ColumnName = "UD2"
			UD2.IsFromTable = True
			UD2.IsVisible = True
			UD2.Description = "MDEP"
			columnDefinitions.Add(UD2)
			
	Dim UD3 As New XFDynamicGridColumnDefinition()
			UD3.ColumnName = "UD3"
			UD3.IsFromTable = True
			UD3.IsVisible = True
			UD3.Description = "APE9"
			columnDefinitions.Add(UD3)	
			
	Dim UD4 As New XFDynamicGridColumnDefinition()
			UD4.ColumnName = "UD4"
			UD4.IsFromTable = True
			UD4.IsVisible = True
			UD4.Description = "Dollar Type"
			columnDefinitions.Add(UD4)	
			
	Dim UD5 As New XFDynamicGridColumnDefinition()
			UD5.ColumnName = "UD5"
			UD5.IsFromTable = True
			UD5.IsVisible = True
			UD5.Description = "CType"
			columnDefinitions.Add(UD5)	
			
	Dim UD6 As New XFDynamicGridColumnDefinition()
			UD6.ColumnName = "UD6"
			UD6.IsFromTable = True
			UD6.IsVisible = True
			UD6.Description = "Object Class"
			columnDefinitions.Add(UD6)			
						
							
Dim dt As New DataTable()
			Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
			
            Dim sql As String =$"SELECT 
										 Req.Entity,Dtl.UD1,Dtl.UD2,Dtl.UD3,Dtl.UD4,Dtl.UD5,Dtl.UD6,Dtl.Unit_Of_Measure,
										 FORMAT(Dtl.FY_1, 'N2') As FY_1,
								         FORMAT(Dtl.FY_2, 'N2') AS FY_2,
								         FORMAT(Dtl.FY_3, 'N2') AS FY_3,
								          FORMAT(Dtl.FY_4, 'N2') AS FY_4,
								          FORMAT(Dtl.FY_5, 'N2') AS FY_5			
								FROM XFC_CMD_PGM_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_PGM_REQ AS Req
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								AND Dtl.Unit_Of_Measure = 'CIV_COST'
								OR Dtl.Unit_Of_Measure = 'CIV_FTE'"
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using

				'Pass table into Filter Function
						Dim Filteredtable As DataTable = Me.FilterTable(dt)
								
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function		
#End Region

#Region "Prioritize"
Private Function dg_CMD_PGM_Prioritize_REQ() As Object

Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		


Dim Scenario As New XFDynamicGridColumnDefinition()
			Scenario.ColumnName = "WFScenario_Name"
			Scenario.IsFromTable = True
			Scenario.IsVisible = False
			Scenario.AllowUpdates = False
			columnDefinitions.Add(Scenario)
			
Dim Time As New XFDynamicGridColumnDefinition()
			Time.ColumnName = "WFTime_Name"
			Time.IsFromTable = True
			Time.IsVisible = False
			Time.AllowUpdates = False
			columnDefinitions.Add(Time)
			
Dim CMD As New XFDynamicGridColumnDefinition()
			CMD.ColumnName = "WFCMD_Name"
			CMD.IsFromTable = True
			CMD.IsVisible = False
			CMD.AllowUpdates = False
			columnDefinitions.Add(CMD)

Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			columnDefinitions.Add(CMD_PGM_ID)
			
Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Entity"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.AllowUpdates = False
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)
			
Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "UD1"
			APPN.IsFromTable = True
			APPN.IsVisible = True
			APPN.AllowUpdates = False
			APPN.Description = "APPN"
			columnDefinitions.Add(APPN)
			
   Dim MDEP As New XFDynamicGridColumnDefinition()
			MDEP.ColumnName = "UD2"
			MDEP.IsFromTable = True
			MDEP.IsVisible = True
			MDEP.AllowUpdates = False
			MDEP.Description = "MDEP"
			columnDefinitions.Add(MDEP)
	Dim APE As New XFDynamicGridColumnDefinition()
			APE.ColumnName = "UD3"
			APE.IsFromTable = True
			APE.IsVisible = True
			APE.AllowUpdates = False
			APE.Description = "APE9"
			columnDefinitions.Add(APE)
	Dim DollarType As New XFDynamicGridColumnDefinition()
			DollarType.ColumnName = "UD4"
			DollarType.IsFromTable = True
			DollarType.IsVisible = True
			DollarType.AllowUpdates = False
			DollarType.Description = "DollarType"
			columnDefinitions.Add(DollarType)
	Dim REQ_ID As New XFDynamicGridColumnDefinition()
			REQ_ID.ColumnName = "REQ_ID"
			REQ_ID.IsFromTable = True
			REQ_ID.IsVisible = True
			REQ_ID.AllowUpdates = False
			REQ_ID.Description = "REQ_ID"
			columnDefinitions.Add(REQ_ID)		
			
	Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)

    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			FY_1.AllowUpdates = False
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			FY_2.AllowUpdates = False
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			FY_3.AllowUpdates = False
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			FY_4.AllowUpdates = False
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			FY_5.AllowUpdates = False
			columnDefinitions.Add(FY_5)
			
	Dim Total_Score As New XFDynamicGridColumnDefinition()
			Total_Score.ColumnName = "Score"
			Total_Score.IsFromTable = True
			Total_Score.IsVisible = True
			Total_Score.Description = "Total Score"
			Total_Score.AllowUpdates = False
			columnDefinitions.Add(Total_Score)
			
	Dim Weighted_Score As New XFDynamicGridColumnDefinition()
			Weighted_Score.ColumnName = "Weighted_Score"
			Weighted_Score.IsFromTable = True
			Weighted_Score.IsVisible = True
			Weighted_Score.Description = "Weighted Score"
			Weighted_Score.AllowUpdates = False
			columnDefinitions.Add(Weighted_Score)
			
	Dim Auto_Rank As New XFDynamicGridColumnDefinition()
			Auto_Rank.ColumnName = "Auto_Rank"
			
			Auto_Rank.IsFromTable = True
			Auto_Rank.IsVisible = True
			Auto_Rank.Description = "Auto Rank"
			Auto_Rank.AllowUpdates = False
			columnDefinitions.Add(Auto_Rank)
			
	Dim Rank_Override As New XFDynamicGridColumnDefinition()
			Rank_Override.ColumnName = "Rank_Override"
			
			Rank_Override.IsFromTable = True
			Rank_Override.IsVisible = True
			Rank_Override.Description = "Rank Override"
			Rank_Override.AllowUpdates = True
			columnDefinitions.Add(Rank_Override)
			
	Dim Review_Entity As New XFDynamicGridColumnDefinition()
			Review_Entity.ColumnName = "Review_Entity"
			Review_Entity.IsFromTable = True
			Review_Entity.IsVisible = False
			columnDefinitions.Add(Review_Entity)
			
			
			
			

   		Dim CatName As  List(Of String) = Me.GetCategoryAndWeight(si)
		
		For i As Integer = 1 To 15
    Dim catDefinition As New XFDynamicGridColumnDefinition()
    
    catDefinition.ColumnName = $"Cat_{i}_Score"
    catDefinition.IsFromTable = True
    
    Dim cat1Name As String = CatName(i - 1)
    
    If String.IsNullOrWhiteSpace(cat1Name) Then
        catDefinition.IsVisible = False
    Else
        catDefinition.IsVisible = True
    End If
    
    catDefinition.Description = cat1Name
    catDefinition.AllowUpdates = True
    
    columnDefinitions.Add(catDefinition)
Next
				
							
						' Get the data you want To put In the grid
						
		Dim sEntity As String  = args.CustomSubstVars.XFGetValue("BL_CMD_PGM_FundsCenter","NA")
			If String.IsNullOrWhiteSpace(sEntity) Then
				Return Nothing
			Else
				Dim sFundcenter As String = ""
				'Remove _General to get the parent Entity
				Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
				Dim entityPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
         		Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sEntity)
				Dim Haschildren As Boolean = BRApi.Finance.Members.HasChildren(si,entityPk,nBaseID)
'Brapi.ErrorLog.LogMessage(si,"**sEntity: " & sEntity)				
'				If sEntity.Contains("_General") Then 'added
'					sFundcenter = sEntity.Replace("_General",".Base") 'added 
'				End If 'added														
				If Haschildren Then 'added
					sFundcenter = sEntity & ".DescendantsInclusive" 'added 
				End If 'added
				
				Dim LFundCenters As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "E_ARMY", "E#"& sFundcenter,True)
				Dim allFCs As String = ""
				If Haschildren Then allFCs = "'" & sEntity & "',"
				For Each FundCenter As MemberInfo In LFundCenters'LFundCenters 'added
					allFcs = allFcs  & "'" & FundCenter.Member.Name & "'," 
				Next
				allFCs = allFCs.Substring(0,allFCs.Length-1)
'Brapi.ErrorLog.LogMessage(si,"AllFC" & allFCs)
				
				Dim dtPri As New DataTable()
				Dim WFInfoDetails As New Dictionary(Of String, String)()
	
	            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
	            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
				Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
	            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
	            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
	            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
				WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
				
				
				Dim entityLevel As String = Me.GetEntityLevel(sEntity)
				Dim sREQWFStatus As String = entityLevel & "_Prioritize_PGM"
	            Dim sql As String =$"SELECT Req.CMD_PGM_REQ_ID, 
										 Req.WFScenario_Name, 
										 Req.WFCMD_Name, Req.WFTime_Name, Req.Entity,Dtl.UD1,Dtl.UD2,Dtl.UD3,Dtl.UD4,Req.REQ_ID,Req.Title,Pri.Cat_1_Score,
										Pri.Cat_2_Score,Pri.Cat_3_Score,Pri.Cat_4_Score,Pri.Cat_5_Score,Pri.Cat_6_Score,Pri.Cat_7_Score,
										Pri.Cat_8_Score,Pri.Cat_9_Score,Pri.Cat_10_Score,Pri.Cat_11_Score,Pri.Cat_12_Score,Pri.Cat_13_Score,			
										Pri.Cat_14_Score,Pri.Cat_15_Score,Pri.Score,Pri.Weighted_Score,Pri.Auto_Rank,Pri.Rank_Override, 
										Pri.Review_Entity,
										FORMAT(Dtl.FY_1, 'N1') As FY_1,
								        FORMAT(Dtl.FY_2, 'N1') AS FY_2,
								        FORMAT(Dtl.FY_3, 'N1') AS FY_3,
								        FORMAT(Dtl.FY_4, 'N1') AS FY_4,
								        FORMAT(Dtl.FY_5, 'N1') AS FY_5
								
								FROM XFC_CMD_PGM_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_PGM_REQ_Priority AS Pri
								ON Pri.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Pri.WFScenario_Name = Dtl.WFScenario_Name
								AND Pri.WFCMD_Name = Dtl.WFCMD_Name
								AND Pri.WFTime_Name = Dtl.WFTime_Name
								AND Pri.Entity = Dtl.Entity
								AND Pri.Review_Entity = '{sEntity}'
								LEFT JOIN XFC_CMD_PGM_REQ AS Req
								ON Req.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								AND Dtl.Account = 'Req_Funding'
								AND Dtl.flow = '{sREQWFStatus}'
								AND Req.Entity in ({allFCs})"
				Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
	                dtPri = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
	            End Using

 				'Pass table into Filter Function
				Dim Filteredtable As DataTable = Me.FilterTable(dtPri)
						
			     'Create the XFTable
			    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
			
			     'Send the result To the Interface component
			    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
			        
			    Return taskResult
			End If
		End Function
#End Region

#Region "Manage"
	Private Function dg_CMD_PGM_REQManage() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
	Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		
		Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Current Status"
			Status.AllowUpdates = False
			status.FilterByColumnName = "Flow"
			columnDefinitions.Add(Status)
			
'-------------------------------------------------------------------------------------------			
			Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
			globals.SetStringValue("Entity",sEntity)
			
	Dim NewStatus As New XFDynamicGridColumnDefinition()
			NewStatus.ColumnName = "New Status"
			NewStatus.IsFromTable = True
			NewStatus.IsVisible = True
			NewStatus.AllowUpdates = True
		'	NewStatus.CustomParameters = "StatusDT"
			'newstatus.DefaultValue = "L2_Formulate_PGM,L2_Validate_PGM"
			NewStatus.ParameterName = "BL_CMD_PGM_StatusChange"
			columnDefinitions.Add(NewStatus)
			
			
	Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
	Dim REQ_ID As New XFDynamicGridColumnDefinition()
			REQ_ID.ColumnName = "REQ_ID"
			REQ_ID.IsFromTable = True
			REQ_ID.IsVisible = True
			REQ_ID.AllowUpdates = False
			REQ_ID.Description = "REQ_ID"
			columnDefinitions.Add(REQ_ID)
			
	Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Funds Center"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.AllowUpdates = False
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)
	
Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "APPN"
			APPN.IsFromTable = True
			APPN.IsVisible = True
			APPN.AllowUpdates = False
			
			columnDefinitions.Add(APPN)
			
   Dim MDEP As New XFDynamicGridColumnDefinition()
			MDEP.ColumnName = "MDEP"
			MDEP.IsFromTable = True
			MDEP.IsVisible = True
			MDEP.AllowUpdates = False
			
			columnDefinitions.Add(MDEP)
	Dim APE As New XFDynamicGridColumnDefinition()
			APE.ColumnName = "APE9"
			APE.IsFromTable = True
			APE.IsVisible = True
			APE.AllowUpdates = False
			APE.Description = "APE9"
			columnDefinitions.Add(APE)
	Dim DollarType As New XFDynamicGridColumnDefinition()
			DollarType.ColumnName = "DollarType"
			DollarType.IsFromTable = True
			DollarType.IsVisible = True
			DollarType.AllowUpdates = False
			DollarType.Description = "DollarType"
			columnDefinitions.Add(DollarType)
			
	Dim ObjectClass As New XFDynamicGridColumnDefinition()
			ObjectClass.ColumnName = "Object Class"
			ObjectClass.IsFromTable = True
			ObjectClass.IsVisible = False
			ObjectClass.AllowUpdates = False
			
			columnDefinitions.Add(ObjectClass)	
			
	Dim scType As New XFDynamicGridColumnDefinition()
			scType.ColumnName = "cType"
			scType.IsFromTable = True
			scType.IsVisible = False
			scType.AllowUpdates = False
			
			columnDefinitions.Add(scType)	
	Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = False
			Account.AllowUpdates = False
			
			columnDefinitions.Add(Account)		
		
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			FY_1.AllowUpdates = False
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			FY_2.AllowUpdates = False
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			FY_3.AllowUpdates = False
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			FY_4.AllowUpdates = False
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			FY_5.AllowUpdates = False
			columnDefinitions.Add(FY_5)

Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_PGM_ID)
    
			
	
							
						' Get the data you want To put In the grid
						
					'	Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
					'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_PGMDataSet.Main(si, globals, api, dargs)
						objdt.Columns.Add("New Status",GetType(String))
				
					'Pass table into Filter Function
						Dim Filteredtable As DataTable = Me.FilterTable(objdt)

					' Create the XFTable
						Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)

					  
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function	
		
#End Region
		
#Region "Review Amounts" 'Flagged for removal
	Private Function dg_CMD_PGM_REQAmtsReview() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		

Dim Entity As New XFDynamicGridColumnDefinition()
			Entity.ColumnName = "Funds Center"
			Entity.IsFromTable = True
			Entity.IsVisible = True
			Entity.AllowUpdates = False
			Entity.Description = "Funds Center"
			columnDefinitions.Add(Entity)
			
			
Dim REQ_ID As New XFDynamicGridColumnDefinition()
			REQ_ID.ColumnName = "REQ_ID"
			REQ_ID.IsFromTable = True
			REQ_ID.IsVisible = True
			REQ_ID.AllowUpdates = False
			REQ_ID.Description = "REQ_ID"
			columnDefinitions.Add(REQ_ID)	
			
Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "APPN"
			APPN.IsFromTable = True
			APPN.IsVisible = True
			APPN.AllowUpdates = False
			
			columnDefinitions.Add(APPN)
			
   Dim MDEP As New XFDynamicGridColumnDefinition()
			MDEP.ColumnName = "MDEP"
			MDEP.IsFromTable = True
			MDEP.IsVisible = True
			MDEP.AllowUpdates = False
			
			columnDefinitions.Add(MDEP)
	Dim APE As New XFDynamicGridColumnDefinition()
			APE.ColumnName = "APE9"
			APE.IsFromTable = True
			APE.IsVisible = True
			APE.AllowUpdates = False
			APE.Description = "APE9"
			columnDefinitions.Add(APE)
	Dim DollarType As New XFDynamicGridColumnDefinition()
			DollarType.ColumnName = "DollarType"
			DollarType.IsFromTable = True
			DollarType.IsVisible = True
			DollarType.AllowUpdates = False
			DollarType.Description = "DollarType"
			columnDefinitions.Add(DollarType)
			
	Dim ObjectClass As New XFDynamicGridColumnDefinition()
			ObjectClass.ColumnName = "Object Class"
			ObjectClass.IsFromTable = True
			ObjectClass.IsVisible = False
			ObjectClass.AllowUpdates = False
			
			columnDefinitions.Add(ObjectClass)	
			
	Dim scType As New XFDynamicGridColumnDefinition()
			scType.ColumnName = "cType"
			scType.IsFromTable = True
			scType.IsVisible = False
			scType.AllowUpdates = False
			
			columnDefinitions.Add(scType)	
	Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = False
			Account.AllowUpdates = False
			
			columnDefinitions.Add(Account)		
		
			
	Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = False
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
	Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = False
			Status.Description = "Current Status"
			Status.AllowUpdates = False
			columnDefinitions.Add(Status)
			

    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			FY_1.AllowUpdates = False
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			FY_2.AllowUpdates = False
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			FY_3.AllowUpdates = False
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			FY_4.AllowUpdates = False
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			FY_5.AllowUpdates = False
			columnDefinitions.Add(FY_5)

Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_PGM_ID)
    
			
	
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
					'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_PGMDataSet.Main(si, globals, api, dargs)
						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function	
		
#End Region

#Region "Save Prioritize"		
Public Function Save_dg_CMD_PGM_Prioritize_REQ( ) As Object
Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
    If saveDataArgs Is Nothing Then
        Return Nothing
    End If
'Brapi.ErrorLog.LogMessage(si, "HERE 1")
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
	'Brapi.ErrorLog.LogMessage(si, "HERE 2")
		Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
		'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
		'Brapi.ErrorLog.LogMessage(si, "HERE 4")	

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

			' --- Priority Table (XFC_CMD_PGM_REQ_Priority) ---
            Dim dt_Priority As New DataTable()
            Dim sqa3 As New SqlDataAdapter()
            Dim sqaReaderPriority As New SQA_XFC_CMD_PGM_REQ_Priority(sqlConn)
            Dim sqlPriority As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Priority WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderPriority.Fill_XFC_CMD_PGM_REQ_Priority_DT(si, sqa3, dt_Priority, sqlPriority, sqlParams)

            ' ************************************
            ' ************************************
          
	 For Each editedDataRow As XFEditedDataRow In editedDataRows
	Dim sEntity As String  = args.CustomSubstVars.XFGetValue("BL_CMD_PGM_FundsCenter","NA")	
'Brapi.ErrorLog.LogMessage(si, "Pri Entity" & sEntity & " : " & editedDataRow.ModifiedDataRow.Item("REQ_ID"))
		
	
    Dim targetRow As DataRow 											
	Dim req_ID_Val As Guid
	req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_PGM_REQ_ID")
	Dim REQIDrow As DataRow =  dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
	Dim isInsert As Boolean = False		
	targetRow = dt_Priority.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' And Review_Entity = '{sEntity}'").FirstOrDefault()
	
If targetRow Is Nothing Then
	 isInsert = True
    targetRow = dt_Priority.NewRow() 
	Dim req_ID As Guid = editedDataRow.ModifiedDataRow.Item("CMD_PGM_REQ_ID")
    Dim mainRow As DataRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID}'").FirstOrDefault()

    If mainRow IsNot Nothing Then
        targetRow("CMD_PGM_REQ_ID") = mainRow("CMD_PGM_REQ_ID")
      
    End If
    
    
Else
   
    isInsert = False
	targetRow = dt_Priority.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'And Review_Entity = '{sEntity}'").FirstOrDefault()
    End If
		targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
		targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
		targetRow("WFTime_Name") = wfInfoDetails("TimeName")
		targetRow("Entity") = editedDataRow.ModifiedDataRow.Item("Entity").ToString()
		 targetRow("Review_Entity") = sEntity
		targetRow("REQ_ID") = REQIDrow.Item("REQ_ID")
			Dim Cat1 As Decimal =  editedDataRow.ModifiedDataRow.Item("Cat_1_Score")				 
			Dim Cat2 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_2_Score")
			Dim Cat3 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_3_Score")
			Dim Cat4 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_4_Score")
			Dim Cat5 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_5_Score")
			Dim Cat6 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_6_Score")
			Dim Cat7 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_7_Score")
			Dim Cat8 As Decimal= editedDataRow.ModifiedDataRow.Item("Cat_8_Score")
			Dim Cat9 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_9_Score")
			Dim Cat10 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_10_Score")
			Dim Cat11 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_11_Score")
			Dim Cat12 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_12_Score")
			Dim Cat13 As Decimal= editedDataRow.ModifiedDataRow.Item("Cat_13_Score")
			Dim Cat14 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_14_Score")
			Dim Cat15 As Decimal = editedDataRow.ModifiedDataRow.Item("Cat_15_Score")

		 
		 
		 
		 targetRow("Cat_1_Score") = Cat1
		 targetRow("Cat_2_Score") = Cat2
		 targetRow("Cat_3_Score") = Cat3
		 targetRow("Cat_4_Score") =	Cat4
		 targetRow("Cat_5_Score") = Cat5
		 targetRow("Cat_6_Score") = Cat6
		targetRow("Cat_7_Score") = Cat7
		targetRow("Cat_8_Score") = Cat8
		targetRow("Cat_9_Score") = Cat9
		targetRow("Cat_10_Score") = Cat10
		targetRow("Cat_11_Score") = Cat11
		targetRow("Cat_12_Score") = Cat12
		targetRow("Cat_13_Score") = Cat13
		targetRow("Cat_14_Score") = Cat14
		targetRow("Cat_15_Score") = Cat15
		targetRow("Score") = Cat1 + Cat2 + Cat3 + Cat4 + Cat5 + Cat6 + Cat7 + Cat8 + Cat9 + Cat10 + Cat11 + Cat12 + Cat13 + Cat14 + Cat15
		targetRow("Rank_Override") = editedDataRow.ModifiedDataRow.Item("Rank_Override").ToString()
		
		Dim editedOverride As String = editedDataRow.ModifiedDataRow.Item("Rank_Override").ToString()
		Dim targetOverride As String = targetRow("Rank_Override").ToString()
		If (editedOverride = "0") Or (targetOverride = "0") Then
    	targetRow("Auto_Rank") = editedDataRow.ModifiedDataRow.Item("Auto_Rank").ToString()

		Else
   		targetRow("Auto_Rank") = targetRow("Rank_Override").ToString()

		End If

		targetRow("Create_Date") = DateTime.Now
		targetRow("Create_User") = si.UserName
		targetRow("Update_Date") = DateTime.Now
		targetRow("Update_User") = si.UserName

'Brapi.ErrorLog.LogMessage(si, "HERE 8") 
	  ' Iterate each column/cell in the XFTV row and apply dirty changes to the DataRow
		                    ' Make a copy of the keys to avoid collection modification issues
'		                    Dim dirtyColList As List(Of String) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Dirty_Column_List(si,editedDataRow)

'                            For Each colName As String In dirtyColList   
'								targetRow(colName) = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.Convert_xftvCol_to_DbValue(si, editedDataRow.ModifiedDataRow.Item(colName))   
'                            Next

		                    ' If this is an insert, add the new row to the DataTable
		                    If isInsert Then
								dt_Priority.Rows.Add(targetRow)
		                    End If
		                Next
	
		                ' Persist changes back to the DB using the configured adapter
		               ' Brapi.ErrorLog.LogMessage(si, "HERE 10") 
		                sqaReaderPriority.Update_XFC_CMD_PGM_REQ_Priority(si,dt_Priority,sqa3)
						
						
						
		                End Using
		            End Using
'Brapi.ErrorLog.LogMessage(si, "HERE 11") 
            Return Nothing
		
End Function		
#End Region

#Region"Rollover"
		Public Function dg_CMD_PGM_Rollover_REQ() As Object
			Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 
			
			Dim CMD_PGM_REQ_ID As New XFDynamicGridColumnDefinition()
					CMD_PGM_REQ_ID.ColumnName = "CMD_PGM_REQ_ID"
					CMD_PGM_REQ_ID.IsFromTable = True
					CMD_PGM_REQ_ID.IsVisible = False
					columnDefinitions.Add(CMD_PGM_REQ_ID)
								
			Dim Entity As New XFDynamicGridColumnDefinition()
					Entity.ColumnName = "Funds Center"
					Entity.IsFromTable = True
					Entity.IsVisible = True
					columnDefinitions.Add(Entity)
					

			Dim REQ_ID As New XFDynamicGridColumnDefinition()
					REQ_ID.ColumnName = "REQ_ID"
					REQ_ID.IsFromTable = True
					REQ_ID.IsVisible = True
					columnDefinitions.Add(REQ_ID)					
					
			Dim APPN As New XFDynamicGridColumnDefinition()
					APPN.ColumnName = "APPN"
					APPN.IsFromTable = True
					APPN.IsVisible = True
					columnDefinitions.Add(APPN)
					
		   	Dim MDEP As New XFDynamicGridColumnDefinition()
					MDEP.ColumnName = "MDEP"
					MDEP.IsFromTable = True
					MDEP.IsVisible = True
					columnDefinitions.Add(MDEP)
					
			Dim APE As New XFDynamicGridColumnDefinition()
					APE.ColumnName = "APE"
					APE.IsFromTable = True
					APE.IsVisible = True
					columnDefinitions.Add(APE)					
					
			Dim Flow As New XFDynamicGridColumnDefinition()
					Flow.ColumnName = "Flow"
					Flow.IsFromTable = True
					Flow.IsVisible = False
					Flow.Description = "Status"
					columnDefinitions.Add(Flow)

			Dim Title As New XFDynamicGridColumnDefinition()
					Title.ColumnName = "Title"
					Title.IsFromTable = True
					Title.IsVisible = True
					'Title.Description = "Title"
					columnDefinitions.Add(Title)
					
			Dim DollarType As New XFDynamicGridColumnDefinition()
					DollarType.ColumnName = "DollarType"
					DollarType.IsFromTable = True
					DollarType.IsVisible = True
					columnDefinitions.Add(DollarType)
					
		   Dim ObjectClass As New XFDynamicGridColumnDefinition()
					ObjectClass.ColumnName = "Object Class"
					ObjectClass.IsFromTable = True
					ObjectClass.IsVisible = False
					columnDefinitions.Add(ObjectClass)
					
		    Dim ctypecol As New XFDynamicGridColumnDefinition()
					ctypecol.ColumnName = "cType"
					ctypecol.IsFromTable = True
					ctypecol.IsVisible = False
					columnDefinitions.Add(ctypecol)
					
		    Dim account As New XFDynamicGridColumnDefinition()
					account.ColumnName = "account"
					account.IsFromTable = True
					account.IsVisible = False
					columnDefinitions.Add(account)					
		    
		    Dim FY_1 As New XFDynamicGridColumnDefinition()
					FY_1.ColumnName = "FY_1"
					FY_1.IsFromTable = True
					FY_1.IsVisible = False
					FY_1.Description = WFYear
					columnDefinitions.Add(FY_1)
					
			Dim FY_2 As New XFDynamicGridColumnDefinition()
					FY_2.ColumnName = "FY_2"
					FY_2.IsFromTable = True
					FY_2.IsVisible = True
					FY_2.Description = WFYear 
					columnDefinitions.Add(FY_2)
					
		   Dim FY_3 As New XFDynamicGridColumnDefinition()
					FY_3.ColumnName = "FY_3"
					FY_3.IsFromTable = True
					FY_3.IsVisible = False
					FY_3.Description = WFYear + 1
					columnDefinitions.Add(FY_3)
					
		    Dim FY_4 As New XFDynamicGridColumnDefinition()
					FY_4.ColumnName = "FY_4"
					FY_4.IsFromTable = True
					FY_4.IsVisible = False
					FY_4.Description = WFYear + 2
					columnDefinitions.Add(FY_4)
					
		    Dim FY_5 As New XFDynamicGridColumnDefinition()
					FY_5.ColumnName = "FY_5"
					FY_5.IsFromTable = True
					FY_5.IsVisible = False
					FY_5.Description = WFYear + 3
					columnDefinitions.Add(FY_5)
								
							
			' Get the data you want To put In the grid
			
			Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
			Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim entityDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & WFCube)
			Dim baseMbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si,entityDimPk,"E#" & sEntity.Replace("_General","") & ".DescendantsInclusive",True)
'Brapi.ErrorLog.LogMessage(si,"basMbrs: " & baseMbrs.Count)	
			Dim FCList As String = String.Empty
			For Each mbr In baseMbrs
				FCList = FCList & mbr.Member.Name & ","
			Next
			
			Dim objdt As New DataTable 
			Dim dargs As New DashboardDataSetArgs
			dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
			dargs.DataSetName = "REQListByEntityAndStatus"
			dargs.NameValuePairs.XFSetValue("Entity", sEntity)
			objdt = BR_CMD_PGMDataSet.Main(si, globals, api, dargs)
	  		
			'Pass table into Filter Function
			Dim Filteredtable As DataTable = Me.FilterTable(objdt)
		     'Create the XFTable
		    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
		
		     'Send the result To the Interface component
		    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult		
		End Function
#End Region	

#Region "REQ Funding Line"
Private Function dg_CMD_PGM_REQFundingLine() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	 Dim dt As New DataTable()
	 Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
	 Dim EditValue As String = args.CustomSubstVars.XFGetValue("DL_CMD_PGM_AdjustFundingLine")
	  
	'If EditValue.XFEqualsIgnoreCase("lock") Then

	Dim Scenario As New XFDynamicGridColumnDefinition()
			Scenario.ColumnName = "WFScenario_Name"
			Scenario.IsFromTable = True
			Scenario.IsVisible = False
			Scenario.AllowUpdates = False
			columnDefinitions.Add(Scenario)
			
	Dim Time As New XFDynamicGridColumnDefinition()
			Time.ColumnName = "WFTime_Name"
			Time.IsFromTable = True
			Time.IsVisible = False
			Time.AllowUpdates = False
			columnDefinitions.Add(Time)
			
	Dim CMD As New XFDynamicGridColumnDefinition()
			CMD.ColumnName = "WFCMD_Name"
			CMD.IsFromTable = True
			CMD.IsVisible = False
			CMD.AllowUpdates = False
			columnDefinitions.Add(CMD)
			
	Dim sEntity As New XFDynamicGridColumnDefinition()
			sEntity.ColumnName = "Entity"
			sEntity.IsFromTable = True
			sEntity.IsVisible = True
			sEntity.AllowUpdates = False
			sEntity.Description = "Funds Center"
			columnDefinitions.Add(sEntity)		
	Dim REQ_ID As New XFDynamicGridColumnDefinition()
			REQ_ID.ColumnName = "REQ_ID"
			REQ_ID.IsFromTable = True
			REQ_ID.IsVisible = True
			REQ_ID.AllowUpdates = False
			REQ_ID.Description = "REQ_ID"
			columnDefinitions.Add(REQ_ID)		
				
		
				

   Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "UD1"
			APPN.IsFromTable = True
			APPN.IsVisible = True
			APPN.AllowUpdates = False
			APPN.Description = "APPN"
			columnDefinitions.Add(APPN)
   Dim MDEP As New XFDynamicGridColumnDefinition()
			MDEP.ColumnName = "UD2"
			MDEP.IsFromTable = True
			MDEP.IsVisible = True
			MDEP.AllowUpdates = False
			MDEP.Description = "MDEP"
			columnDefinitions.Add(MDEP)
			
	Dim APE As New XFDynamicGridColumnDefinition()
			APE.ColumnName = "UD3"
			APE.IsFromTable = True
			APE.IsVisible = True
			APE.AllowUpdates = False
			APE.Description = "APE9"
			columnDefinitions.Add(APE)
	Dim DollarType As New XFDynamicGridColumnDefinition()
			DollarType.ColumnName = "UD4"
			DollarType.IsFromTable = True
			DollarType.IsVisible = True
			DollarType.AllowUpdates = False
			DollarType.Description = "Dollar Type"
			columnDefinitions.Add(DollarType)
   Dim ObjectClass As New XFDynamicGridColumnDefinition()
			ObjectClass.ColumnName = "UD6"
			ObjectClass.IsFromTable = True
			ObjectClass.IsVisible = True
			ObjectClass.AllowUpdates = False
			ObjectClass.Description = "Object Class"
			columnDefinitions.Add(ObjectClass)
			
    Dim ctypecol As New XFDynamicGridColumnDefinition()
			ctypecol.ColumnName = "UD5"
			ctypecol.IsFromTable = True
			ctypecol.IsVisible = True
			ctypecol.AllowUpdates = False
			ctypecol.Description = "cType"
			columnDefinitions.Add(ctypecol)

Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_PGM_ID)
    
    Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			FY_1.DataFormatString = "N0"
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			FY_2.DataFormatString = "N0"
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			FY_3.DataFormatString = "N0"
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			FY_4.DataFormatString = "N0"
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			FY_5.DataFormatString = "N0"
			columnDefinitions.Add(FY_5)
			
	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = False
			Status.Description = "Status"
			columnDefinitions.Add(Status)

							
						' Get the data you want To put In the grid
						
		Dim ReqID As String = args.CustomSubstVars.XFGetValue("IV_CMD_TGT_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
					Return Nothing
				Else 
			
			
			Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
            Dim sql As String = $"SELECT Req.CMD_PGM_REQ_ID,
								  Req.WFScenario_Name,
								  Req.WFCMD_Name,
								  Req.WFTime_Name,
								  Req.Entity,
								  Dtl.UD1,
								  Dtl.UD2,
								  Dtl.UD3,
								  Dtl.UD4,
								  Dtl.UD5,
								  Dtl.UD6,
								  dtl.flow,
								Dtl.FY_1 As FY_1,
						         Dtl.FY_2 AS FY_2,
						         Dtl.FY_3 AS FY_3,
						         Dtl.FY_4 AS FY_4,
						         Dtl.FY_5 AS FY_5
					
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
								And Req.REQ_ID = '{ReqID}'
								AND  Dtl.Account = 'REQ_Funding'"
			
           Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
				     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
					End If 
		End Function

#End Region

#Region "Save REQ Funding Line "		
Public Function Save_dg_CMD_PGM_REQFundingLine() As Object
Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
    If saveDataArgs Is Nothing Then
        Return Nothing
    End If
'Brapi.ErrorLog.LogMessage(si, "HERE 1")
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
	
	'Brapi.ErrorLog.LogMessage(si, "HERE 2")
		Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
			Dim status As New list (Of String)
			Dim FCList As New list (Of String)
			Dim UD1 As New list (Of String)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
		'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID  = @CMD_PGM_REQ_ID"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
				New SqlParameter("@CMD_PGM_REQ_ID", SqlDbType.UniqueIdentifier) With {.Value = editedDataRows.Item(0).ModifiedDataRow("CMD_PGM_REQ_ID")}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
		'Brapi.ErrorLog.LogMessage(si, "HERE 4")	

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
			 Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_PGM_REQ_ID  = @CMD_PGM_REQ_ID"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

			Dim sqa_xfc_cmd_pgm_req_details_audit = New SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT(sqlConn)
			Dim SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT = New DataTable()

			Dim SQL_Audit As String = $"SELECT * 
								FROM XFC_CMD_PGM_REQ_Details_Audit
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_PGM_REQ_ID  = @CMD_PGM_REQ_ID"
		sqa_xfc_cmd_pgm_req_details_audit.Fill_XFC_CMD_PGM_REQ_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, SQL_Audit, sqlParams)
			
			
			
            ' ************************************
            ' ************************************
          
	
			
	 For Each editedDataRow As XFEditedDataRow In editedDataRows
		
		
    Dim targetRow As DataRow 											
	Dim req_ID_Val As Guid
	req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_PGM_REQ_ID")
	If Not status.Contains(editedDataRow.ModifiedDataRow.Item("Flow"))
		status.Add(editedDataRow.ModifiedDataRow.Item("Flow"))
	End If 
	If Not FCList.Contains(editedDataRow.ModifiedDataRow.Item("Entity"))
		FCList.Add(editedDataRow.ModifiedDataRow.Item("Entity"))
	End If 
	If Not UD1.Contains(editedDataRow.ModifiedDataRow.Item("UD1"))
		UD1.Add(editedDataRow.ModifiedDataRow.Item("UD1"))
	End If 
'brapi.ErrorLog.LogMessage(si,"save req funding = " & status & Entity & UD1)	
			
	targetRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
			
		
			
			
			
			
				targetRow("Update_Date") = DateTime.Now
				targetRow("Update_User") = si.UserName																																																																																				

		Dim targetRowFunding As DataRow
			targetRowFunding = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
			
			If targetRowFunding IsNot Nothing Then
			
			Dim FY1 As Decimal =  editedDataRow.ModifiedDataRow.Item("FY_1")		 
			Dim FY2 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_2")
			Dim FY3 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_3")
		    Dim FY4 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_4")
		 	Dim FY5 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_5")

			'Added Audit update before funding line updates are written to the table			
					If SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Rows.Count > 0 Then
								Dim drow As DataRow
									drow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
									drow("Orig_FY1") = targetRowFunding("FY_1")
									drow("Updated_FY1") = FY1
									drow("Orig_FY2") = targetRowFunding("FY_2")
									drow("Updated_FY2") = FY2
									drow("Orig_FY3") = targetRowFunding("FY_3")
									drow("Updated_FY3") = FY3
									drow("Orig_FY4") = targetRowFunding("FY_4")
									drow("Updated_FY4") = FY4
									drow("Orig_FY5") = targetRowFunding("FY_5")
									drow("Updated_FY5") =  FY5
									
						Else
										
								Dim newrow As datarow = SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.NewRow()
									newrow("CMD_PGM_REQ_ID") = targetRow("CMD_PGM_REQ_ID")
									newrow("WFScenario_Name") = targetRow("WFScenario_Name")
									newrow("WFCMD_Name") = targetRow("WFCMD_Name")
									newrow("WFTime_Name") = targetRow("WFTime_Name")
									newrow("Entity") = targetRow("Entity")
									newrow("Account") = "Req_Funding"
									newrow("Start_Year") = targetRow("WFTime_Name")
									newrow("Orig_IC") = "None"
									newrow("Updated_IC") = "None"
									newrow("Orig_Flow") =  targetRow("Status")
									newrow("Updated_Flow") = targetRow("Status")
									newrow("Orig_UD1") = targetRow("APPN")
									newrow("Updated_UD1") = targetRow("APPN")
									newrow("Orig_UD2") = targetRow("MDEP")
									newrow("Updated_UD2") = targetRow("MDEP")
									newrow("Orig_UD3") = targetRow("APE9")
									newrow("Updated_UD3") = targetRow("APE9")
									newrow("Orig_UD4") = targetRow("Dollar_Type")
									newrow("Updated_UD4") = targetRow("Dollar_Type")
									If String.IsNullOrWhiteSpace(targetRow("CType").ToString()) Then
										
									newrow("Orig_UD5") = "None"
									newrow("Updated_UD5") = "None"
									Else 
									newrow("Orig_UD5") = targetRow("CType")
									newrow("Updated_UD5") = targetRow("CType")
								End if
									newrow("Orig_UD6") = targetRow("Obj_Class")
									newrow("Updated_UD6") = targetRow("Obj_Class")
									newrow("Orig_UD7") = "None"
									newrow("Updated_UD7") = "None"
									newrow("Orig_UD8") = "None"
									newrow("Updated_UD8") = "None"
									newrow("Create_Date") = DateTime.Now
									newrow("Create_User") = si.UserName
									newrow("Orig_FY1") = targetRowFunding("FY_1")
									newrow("Updated_FY1") = FY1
									newrow("Orig_FY2") = targetRowFunding("FY_2")
									newrow("Updated_FY2") = FY2
									newrow("Orig_FY3") = targetRowFunding("FY_3")
									newrow("Updated_FY3") = FY3
									newrow("Orig_FY4") = targetRowFunding("FY_4")
									newrow("Updated_FY4") = FY4
									newrow("Orig_FY5") = targetRowFunding("FY_5")
									newrow("Updated_FY5") =  FY5
										
								SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT.rows.add(newrow)	
										
						End If		
							targetRowFunding("FY_1") = FY1
							targetRowFunding("FY_2") = FY2
							targetRowFunding("FY_3") = FY3
							targetRowFunding("FY_4") = FY4
							targetRowFunding("FY_5") = FY5
							targetRowFunding("Update_Date") = DateTime.Now
	                        targetRowFunding("Update_User") = si.UserName  
						
						End If      
					Next	   
		                
	
		                ' Persist changes back to the DB using the configured adapter
		               
		               	sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						sqa_xfc_cmd_pgm_req_details_audit.Update_XFC_CMD_PGM_REQ_DETAILS_AUDIT(SQA_XFC_CMD_PGM_REQ_DETAILS_AUDIT_DT, sqa)
						
						Dim customSubstVars As New Dictionary(Of String, String) 
					 	Dim workspaceid As guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")

								
						Dim EntityLists  = GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_PGM")
						Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
						For Each JoinedEntity As String In joinedentitylist
							globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", String.join("|",Status))	
							Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",String.join("|",UD1))	
						Next
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
						customSubstVars.Add("EntList",BaseEntityList)
						customSubstVars.Add("ParentEntList",ParentEntityList)
						customSubstVars.Add("WFScen",WFInfoDetails("ScenarioName"))
						Dim currentYear As Integer = Convert.ToInt32(WFInfoDetails("TimeName"))
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)	
						
						
		                End Using
		            End Using

            Return Nothing
		
End Function		
#End Region

#Region "Save Manage Status"		
Public Function Save_dg_CMD_PGM_REQManage() As Object
Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
    If saveDataArgs Is Nothing Then
        Return Nothing
    End If
'Brapi.ErrorLog.LogMessage(si, "HERE 1 CM")
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
'Brapi.ErrorLog.LogMessage(si, "HERE 2")
		Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
		    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
      		Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
'Brapi.ErrorLog.LogMessage(si, "HERE 4")	

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
			Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
'Brapi.ErrorLog.LogMessage(si, "dt_Details = " & dt_Details.Rows.Count)	
            ' ************************************
             ' --- Details Audit Table (XFC_CMD_PGM_REQ_Details_Audit) ---
            Dim dt_Details_Audit As New DataTable()
			Dim sqa3 As New SqlDataAdapter()
            Dim sqaReaderdetailAudit As New SQA_XFC_CMD_PGM_REQ_Details_Audit(sqlConn)
            Dim sqlAudit As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details_Audit WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetailAudit.Fill_XFC_CMD_PGM_REQ_Details_Audit_DT(sqa3, dt_Details_Audit, sqlAudit, sqlParams)
'Brapi.ErrorLog.LogMessage(si, "dt_Details = " & dt_Details.Rows.Count)	
			
			
			' ************************************
		     Dim sEntity As String = ""
			 Dim old_status As String = ""
			 Dim New_Status As String = ""
			 Dim tm As String = wfInfoDetails("TimeName")
			 Dim sScenario As String = wfInfoDetails("ScenarioName")
			 Dim workspaceid As guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")
			  Dim targetRow As DataRow 
			  Dim req_ID_Val As Guid
			  Dim StatustoPass As String = ""
			  Dim FCList As New List (Of String)
			  Dim APPNList As New List (Of String)			
'Brapi.ErrorLog.LogMessage(si, "editedDataRows = " & editedDataRows.Count)		
			 For Each editedDataRow As XFEditedDataRow In editedDataRows
		'Brapi.ErrorLog.LogMessage(si, "HERE 5")	
				
					Dim isInsert As Boolean = "false"
				   											
					
					req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_PGM_REQ_ID")
'Brapi.ErrorLog.LogMessage(si, "req_ID_Val CMKL = " & req_ID_Val.ToString)					
							
					targetRow = dt.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
		'Brapi.ErrorLog.LogMessage(si, "HERE 6")	
					old_Status = targetRow.Item("Status")
					
					New_Status = editedDataRow.ModifiedDataRow.Item("New Status")
					
					If StatustoPass.XFEqualsIgnoreCase("") Then 
						statustopass = old_Status & "|" & New_Status
					Else  
						statustopass += "|" & old_Status & "|" & New_Status 
					End If 
'brapi.ErrorLog.LogMessage(si,"CM new stat=" & New_Status & ": Old Stat=" & old_Status)
				  	sEntity = editedDataRow.ModifiedDataRow.Item("Funds Center")
					FCList.Add(sEntity)
					APPNList.Add(targetrow("APPN"))
					targetRow("Update_Date") = DateTime.Now	
					targetRow("Update_User") = si.UserName
					targetRow("Status") = New_Status
		
					Dim targetRowFunding As DataRow()
					targetRowFunding = dt_Details.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'")
					For Each dr As DataRow In targetRowFunding
						dr("Flow") = New_Status
						dr("Update_Date") = DateTime.Now
	                	dr("Update_User") = si.UserName
					Next
		'Brapi.ErrorLog.LogMessage(si, "Row Count " &  dt_Details.Rows.Count )
					Dim targetRowAudit As DataRow()
					targetRowAudit = dt_Details_Audit.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'")
												
						If targetRowAudit.Length > 0 Then
							For Each drow As DataRow In targetRowAudit
								Dim currentHistory As String = If(drow("Orig_Flow") Is DBNull.Value, _
												 String.Empty, _
												 drow("Orig_Flow").ToString())
								If String.IsNullOrEmpty(currentHistory) Then
									drow("Orig_Flow") = old_Status
								Else
									drow("Orig_Flow") = currentHistory + ", " + old_Status
								End If
									drow("Updated_Flow") = New_Status
							Next
						Else
											Dim newrow As datarow = dt_Details_Audit.NewRow()
											newrow("CMD_PGM_REQ_ID") = targetRow("CMD_PGM_REQ_ID")
											newrow("WFScenario_Name") = targetRow("WFScenario_Name")
											newrow("WFCMD_Name") = targetRow("WFCMD_Name")
											newrow("WFTime_Name") = targetRow("WFTime_Name")
											newrow("Entity") = targetRow("Entity")
											newrow("Account") = "Req_Funding"
											newrow("Start_Year") = targetRow("WFTime_Name")
											newrow("Orig_IC") = "None"
											newrow("Updated_IC") = "None"
											newrow("Orig_Flow") =  old_Status
											newrow("Updated_Flow") = New_Status
											newrow("Orig_UD1") = targetRow("APPN")
											newrow("Updated_UD1") = targetRow("APPN")
											newrow("Orig_UD2") = targetRow("MDEP")
											newrow("Updated_UD2") = targetRow("MDEP")
											newrow("Orig_UD3") = targetRow("APE9")
											newrow("Updated_UD3") = targetRow("APE9")
											newrow("Orig_UD4") = targetRow("Dollar_Type")
											newrow("Updated_UD4") = targetRow("Dollar_Type")
											newrow("Orig_UD5") = "None"
											newrow("Updated_UD5") = "None"
											newrow("Orig_UD6") = targetRow("Obj_Class")
											newrow("Updated_UD6") = targetRow("Obj_Class")
											newrow("Orig_UD7") = "None"
											newrow("Updated_UD7") = "None"
											newrow("Orig_UD8") = "None"
											newrow("Updated_UD8") = "None"
											newrow("Create_Date") = DateTime.Now
											newrow("Create_User") = si.UserName
										
										
										
											dt_Details_Audit.rows.add(newrow)
									
									
						End If

'Brapi.ErrorLog.LogMessage(si, "Row Count End" &  dt_Details.Rows.Count )
		    	Next

                ' Persist changes back to the DB using the configured adapter
               
               sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
               sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
			   sqaReaderdetailAudit.Update_XFC_CMD_PGM_REQ_Details_Audit(dt_Details_Audit,sqa3)
'brapi.ErrorLog.LogMessage(si,"Here CM 1 Status = " & New_Status)

			   'Writing to cube
						Dim customSubstVars As New Dictionary(Of String, String) 
					 
						Dim EntityLists  = GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_PGM")
						Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
						For Each JoinedEntity As String In joinedentitylist
							globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", statustopass)	
							Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",String.Join("|",APPNList))
						Next
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
						customSubstVars.Add("EntList",BaseEntityList)
						customSubstVars.Add("ParentEntList",ParentEntityList)
						customSubstVars.Add("WFScen",WFInfoDetails("ScenarioName"))
						Dim currentYear As Integer = Convert.ToInt32(WFInfoDetails("TimeName"))
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)	
						
		                End Using
		            End Using

            Return Nothing
		
End Function		
#End Region

#Region "Req Attachment"
Private Function dg_CMD_PGM_Attachments() As Object
Dim dt As New DataTable()
Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	

Dim CMD_PGM_ID As New XFDynamicGridColumnDefinition()
			CMD_PGM_ID.ColumnName = "CMD_PGM_REQ_ID"
			CMD_PGM_ID.IsFromTable = True
			CMD_PGM_ID.IsVisible = False
			CMD_PGM_ID.AllowUpdates = False
			columnDefinitions.Add(CMD_PGM_ID)
			
Dim FileName As New XFDynamicGridColumnDefinition()
			FileName.ColumnName = "Attach_File_Name"
			FileName.IsFromTable = True
			FileName.IsVisible = True
			FileName.AllowUpdates = False
			FileName.Description = "File Name"
			columnDefinitions.Add(FileName)
			
Dim FileBytes As New XFDynamicGridColumnDefinition()
			FileBytes.ColumnName = "Attach_File_Bytes"
			FileBytes.IsFromTable = True
			FileBytes.IsVisible = False
			FileBytes.AllowUpdates = False
			
			columnDefinitions.Add(FileBytes)
			
			
	Dim sREQ As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQPrompts","REQ","")		
	
	
	   Dim sql As String = $"SELECT Att.CMD_PGM_REQ_ID, Att.Attach_File_Name 
	   						From XFC_CMD_PGM_REQ_Attachment as Att
	   						LEFT JOIN XFC_CMD_PGM_REQ AS Req
							ON Req.CMD_PGM_REQ_ID = Att.CMD_PGM_REQ_ID
							WHERE 
	  					 Req.REQ_ID = '{sREQ}'"
	   
	   
	    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
			'Pass table into Filter Function
			Dim Filteredtable As DataTable = Me.FilterTable(dt)
				     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,Filteredtable,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
					
		End Function
	   
	   
#End Region	

#Region "Create New REQ"
Private Function dg_CMD_PGM_Create() As Object
Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
Dim dt As New DataTable()
Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
Dim createrow As DataRow = dt.NewRow()
dt.Columns.Add("Title")
dt.Columns.Add("FY_1")
dt.Columns.Add("FY_2")
dt.Columns.Add("FY_3")
dt.Columns.Add("FY_4")
dt.Columns.Add("FY_5")


			
Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = True
			Title.Width = "600"
			columnDefinitions.Add(Title)
			
 Dim FY_1 As New XFDynamicGridColumnDefinition()
			FY_1.ColumnName = "FY_1"
			FY_1.IsFromTable = True
			FY_1.IsVisible = True
			FY_1.Description = WFYear
			FY_1.DataFormatString = "N0"
			columnDefinitions.Add(FY_1)
			
	  Dim FY_2 As New XFDynamicGridColumnDefinition()
			FY_2.ColumnName = "FY_2"
			FY_2.IsFromTable = True
			FY_2.IsVisible = True
			FY_2.Description = WFYear + 1
			FY_2.DataFormatString = "N0"
			columnDefinitions.Add(FY_2)
			
   Dim FY_3 As New XFDynamicGridColumnDefinition()
			FY_3.ColumnName = "FY_3"
			FY_3.IsFromTable = True
			FY_3.IsVisible = True
			FY_3.Description = WFYear + 2
			FY_3.DataFormatString = "N0"
			columnDefinitions.Add(FY_3)
			
    Dim FY_4 As New XFDynamicGridColumnDefinition()
			FY_4.ColumnName = "FY_4"
			FY_4.IsFromTable = True
			FY_4.IsVisible = True
			FY_4.Description = WFYear + 3
			FY_4.DataFormatString = "N0"
			columnDefinitions.Add(FY_4)
			
    Dim FY_5 As New XFDynamicGridColumnDefinition()
			FY_5.ColumnName = "FY_5"
			FY_5.IsFromTable = True
			FY_5.IsVisible = True
			FY_5.Description = WFYear + 4
			FY_5.DataFormatString = "N0"
			columnDefinitions.Add(FY_5)
			
			
			dt.Rows.Add(createrow)

 'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
					
		End Function
#End Region

#Region "Save Create new req"
Public Function Save_dg_CMD_PGM_Create() As Object
	
#Region "Global Checks/Functions"
		'Clear the previous Guid
		GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
		'Get New/Requirement ID based on new GUID generated in Globals
		Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
#End Region	
	
Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
    If saveDataArgs Is Nothing Then
        Return Nothing
    End If
'Brapi.ErrorLog.LogMessage(si, "HERE 1")
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
	
	          ' Get Workflow context details
 Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
           
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)

    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
           sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_PGM_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_PGM_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("TimeName")}
						}
              sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
			'Brapi.ErrorLog.LogMessage(si,"In Save Create SQL")

            ' --- Details Table (XFC_CMD_PGM_REQ_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_PGM_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

'Brapi.ErrorLog.LogMessage(si,"In Save Create SQL 2")
            ' ************************************
            ' ************************************
				Dim sU1APPNInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateAPPN","")
							Dim sU2Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateMDEP","")
							Dim sU3Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateAPEPT","")
							Dim sU4Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateDollarType","")
							Dim sU5Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateCType","")
							Dim sU6Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_PGM_FormulateObjectClass","")

						Dim sEntity As String =  args.CustomSubstVars.XFGetValue("BL_CMD_PGM_FundsCenter","")
						Dim entityLevel As String = Me.GetEntityLevel(sEntity)
						Dim sREQWFStatus As String = entityLevel & "_Formulate_PGM"
					'	Dim sEntityList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EntDimPk, "E#" & sEntity & ".Parents", True)
						'm sReviewEntity As String = sEntityList(0).Member.Name
			
						'BRapi.ErrorLog.LogMessage(si,"Hit")
            		For Each editedDataRow As XFEditedDataRow In editedDataRows						
								
						
							Dim requiredString As String = ""
							If String.IsNullOrWhiteSpace(sU1APPNInput) Then
								requiredString += "Appropriation"
							End If	
							
							If String.IsNullOrWhiteSpace(sU2Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "MDEP"
							End If	
							
							If String.IsNullOrWhiteSpace(sU3Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "APE9"
							End If	
							If String.IsNullOrWhiteSpace(sU4Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "DollarType"
							End If	
							If String.IsNullOrWhiteSpace(sU6Input) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "Object_Class"
							End If	
							If String.IsNullOrWhiteSpace(editedDataRow.ModifiedDataRow.Item("Title").ToString()) Then
								If Not String.IsNullOrWhiteSpace(requiredString) Then
									requiredString += ", "
								End If
								requiredString += "Title"
							End If	
							
						
							If Not String.IsNullOrWhiteSpace(requiredString) Then
								Throw New Exception("The following fields must be populated when creating a requirement: " + requiredString + ".")
							End If	
						 
                            Dim targetRowDetails As DataRow = dt_Details.NewRow()
                         
					
                                
							targetRowDetails("CMD_PGM_REQ_ID") = NewReqID
							targetRowDetails("WFScenario_Name") = wfInfoDetails("ScenarioName")
							targetRowDetails("WFCMD_Name") = wfInfoDetails("CMDName")
							targetRowDetails("WFTime_Name") = wfInfoDetails("TimeName")
							targetRowDetails("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_PGM_FundsCenter","")
							targetRowDetails("Unit_of_Measure") = "Funding"
							targetRowDetails("IC") = "None"
							targetRowDetails("Account") = "Req_Funding"
							targetRowDetails("Flow") = sREQWFStatus
'BRApi.ErrorLog.LogMessage(si,"Hit2")
							targetRowDetails("UD1") = sU1APPNInput
							targetRowDetails("UD2") = sU2Input
							targetRowDetails("UD3") = sU3Input
							targetRowDetails("UD4") = sU4Input
						If String.IsNullOrWhiteSpace(sU5Input)
							targetRowDetails("UD5") = "None"
						Else 
							targetRowDetails("UD5") = sU5Input
						End If
							targetRowDetails("UD6") = sU6Input
'BRApi.ErrorLog.LogMessage(si,"Hit3")
							targetRowDetails("UD7") = "None"
							targetRowDetails("UD8") = "None"
							targetRowDetails("Start_Year") = wfInfoDetails("TimeName")
                            Dim fy1 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_1").ToString.XFConvertToDecimal
                            Dim fy2 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_2").ToString.XFConvertToDecimal
                            Dim fy3 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_3").ToString.XFConvertToDecimal
                            Dim fy4 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_4").ToString.XFConvertToDecimal
                            Dim fy5 As Decimal = editedDataRow.ModifiedDataRow.Item("FY_5").ToString.XFConvertToDecimal
							
						
'BRApi.ErrorLog.LogMessage(si,"Hit4")
                            targetRowDetails("FY_1") = fy1
                            targetRowDetails("FY_2") = fy2
                            targetRowDetails("FY_3") = fy3
                            targetRowDetails("FY_4") = fy4
                            targetRowDetails("FY_5") = fy5
                            targetRowDetails("FY_Total") = fy1 + fy2 + fy3 + fy4 + fy5
							targetRowDetails("AllowUpdate") = "True"
							targetRowDetails("Create_Date") = DateTime.Now
							targetRowDetails("Create_User") = si.UserName
							targetRowDetails("Update_Date") = DateTime.Now
							targetRowDetails("Update_User") = si.UserName
								
					Dim targetRow As DataRow = dt.NewRow()
                         
								targetRow("CMD_PGM_REQ_ID") = NewReqID
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_PGM_FundsCenter","")
								targetRow("Review_Entity") = sEntity
								targetRow("REQ_ID") = Me.Get_FC_REQ_ID(si,sEntity)
								targetRow("REQ_ID_Type") = "Requirement"
								targetRow("Title") = editedDataRow.ModifiedDataRow.Item("Title").ToString()
								targetRow("APPN") = sU1APPNInput
								
								targetRow("MDEP") = sU2Input
								targetRow("APE9") = sU3Input
								targetRow("Dollar_Type") = sU4Input
								targetRow("Obj_Class") = sU6Input
								If String.IsNullOrWhiteSpace(sU5Input)
								targetRow("CType") = "None"
							Else
								targetRow("CType") = sU5Input
							End If
								targetRow("Create_Date") = DateTime.Now
								targetRow("Create_User") = si.UserName
								targetRow("Update_Date") = DateTime.Now
								targetRow("Update_User") = si.UserName
								targetRow("Status") = sREQWFStatus
							
								
		                    	dt_Details.Rows.Add(targetRowDetails)
								dt.Rows.Add(targetRow)
		             
		                Next
	
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_PGM_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
						Dim customSubstVars As New Dictionary(Of String, String) 
					    Dim workspaceid As guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"10 CMD PGM")

						Dim FCList As New List (Of String)
						FCList.Add(sEntity)
								
						Dim EntityLists  = GBL.GBL_Assembly.GBL_Helpers.GetEntityLists(si,FCList,"CMD_PGM")
						Dim joinedentitylist = EntityLists.Item1.union(EntityLists.Item2).ToList()
						For Each JoinedEntity As String In joinedentitylist
							globals.SetStringValue($"FundsCenterStatusUpdates - {JoinedEntity}", sREQWFStatus)	
							Globals.setStringValue($"FundsCenterStatusappnUpdates - {JoinedEntity}",sU1APPNInput)
						Next
						Dim ParentEntityList As String = String.Join(", ", EntityLists.Item1.Select(Function(s) $"E#{s}"))
						Dim BaseEntityList As String = String.Join(", ", EntityLists.Item2.Select(Function(s) $"E#{s}"))
						customSubstVars.Add("EntList",BaseEntityList)
						customSubstVars.Add("ParentEntList",ParentEntityList)
						customSubstVars.Add("WFScen",WFInfoDetails("ScenarioName"))
						Dim currentYear As Integer = Convert.ToInt32(WFInfoDetails("TimeName"))
						customSubstVars.Add("WFTime",$"T#{currentYear.ToString()},T#{(currentYear+1).ToString()},T#{(currentYear+2).ToString()},T#{(currentYear+3).ToString()},T#{(currentYear+4).ToString()}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_PGM_Proc_Status_Updates", customSubstVars)	
						
		                End Using
		            End Using

            Return Nothing
        End Function	

#End Region


#Region "Save REQ Comments"		
'Public Function Save_dg_CMD_PGM_Comments()   As Object
'Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
'    If saveDataArgs Is Nothing Then
'        Return Nothing
'    End If
''Brapi.ErrorLog.LogMessage(si, "HERE 1")
'    ' Get the edited rows
'    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
'    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
'        Return Nothing
'    End If
	
'	'Brapi.ErrorLog.LogMessage(si, "HERE 2")
'		Dim WFInfoDetails As New Dictionary(Of String, String)()
'            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
'            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
'			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
'            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
'            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
'            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
'			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
'		'Brapi.ErrorLog.LogMessage(si, "HERE 3")	
'		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
'            sqlConn.Open()

'            ' ************************************
'            ' ************************************
'            ' --- Main Request Table (XFC_CMD_PGM_REQ_CMT) ---
'            Dim dt As New DataTable()
'            Dim sqa As New SqlDataAdapter()
'            Dim sqaReader As New SQA_XFC_CMD_PGM_REQ(sqlConn)		
'			Dim ReqID As String = args.CustomSubstVars.XFGetValue("IV_CMD_TGT_REQTitleList")
'				If String.IsNullOrWhiteSpace(ReqID)
'					Return Nothing
'				Else 
			
'				'Fill the DataTable 
'				Dim sql As String = $"SELECT * 
'									FROM XFC_CMD_PGM_REQ 
'									WHERE WFScenario_Name = @WFScenario_Name
'									AND WFCMD_Name = @WFCMD_Name
'									AND WFTime_Name = @WFTime_Name
'									AND REQ_ID  = {ReqID}"
				
		
'	    ' 2. Create a list to hold the parameters
'	   Dim sqlParams As SqlParameter() = New SqlParameter(){
'        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
'        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
'        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
'   		}
'			sqaReader.Fill_XFC_CMD_PGM_REQ_DT(sqa,dt,sql, sqlparams)
			
'			Dim sREQ_ID_Val As String = String.Empty
'			Dim REQ_ID_Val_guid As Guid = Guid.Empty
			
'		If dt.Rows.Count > 0 Then
'    		'sREQ_ID_Val = Convert.ToString(dt.Rows(0)("CMD_PGM_REQ_ID"))
'    		REQ_ID_Val_guid = dt.Rows(0)("CMD_PGM_REQ_ID")
'		Else
    
'   	 		Return Nothing 
'		End If			
				
					
'			Dim dt_cmt As New DataTable()
'			Dim sqa2 As New SqlDataAdapter()
'           Dim sqaReaderCMT As New SQA_XFC_CMD_PGM_REQ_Cmt(sqlConn)
'			Dim sql_Cmt As String = $"SELECT * 
'									FROM XFC_CMD_PGM_REQ_Cmt
'									WHERE
'									CMD_PGM_REQ_ID  = {REQ_ID_Val_guid}"
				
		
'	    ' 2. Create a list to hold the parameters
'	   Dim sqlParamsCMT As SqlParameter() = New SqlParameter(){
'   		}
'			sqaReaderCMT.Fill_XFC_CMD_PGM_REQ_CMT_DT(sqa2,dt_cmt,sql_Cmt,sqlParamsCMT)
			
			
			
			
'            ' ************************************
'            ' ************************************
'	For Each editedDataRow As XFEditedDataRow In editedDataRows
		
'		'Brapi.errorlog.LogMessage(si,"Loop")
		
'    Dim targetRow As DataRow 											
	
'	targetRow = dt_cmt.NewRow()
	
'		targetRow("CMD_PGM_REQ_ID") = REQ_ID_Val_guid
'		targetRow("General_Comment") = editedDataRow.ModifiedDataRow.Item("General_Comment").ToString()
'		targetRow("Update_Date") = DateTime.Now
'		targetRow("Update_User") = si.UserName

			
		
'			dt_cmt.Rows.Add(targetRow)
	
'					Next	   
		                
	
'		                ' Persist changes back to the DB using the configured adapter
		               
'		               sqaReaderCMT.Update_XFC_CMD_PGM_REQ_Cmt(dt_cmt,sqa2)
'		                'sqaReader.Update_XFC_CMD_PGM_REQ(dt,sqa)
						
'					End if		
'		                End Using
'		            End Using

'            Return Nothing
		
'End Function		
#End Region 'Flagged for removal 

#Region "Get Priority Categories"
 Private Function GetCategoryAndWeight(ByVal si As SessionInfo) As Object
            Try
                'Get the list of catagories
                Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
                Dim WFScenario As String = "RMW_Cycle_Config_Annual"
                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
                Dim REQTime As String = WFYear
            
                Dim sFundCenter As String =  args.CustomSubstVars("BL_CMD_PGM_FundsCenter")

                Dim priCatMembers As List(Of MemberInfo)
                Dim priFilter As String = "A#GBL_Priority_Cat_Weight.Children"
                Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
                 priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
                
                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#UUUU:U6#None:U7#None:U8#None"
                Dim priCatNameList As New List(Of String)
                Dim priCatName As String = ""
                'Dim priCatWeight As Decimal
                
                'Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
                For Each pricat As MemberInfo In priCatMembers
					
                    priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
			
				priCatNameList.Add(priCatName)
                  
                Next
                
                Return priCatNameList
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Function




#End Region
		
#Region "Get Entity Level"	

	Public Function GetEntityLevel(sEntity As String) As String
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
#End Region

#Region"Get REQID"
        Public Function Get_FC_REQ_ID(si As SessionInfo, fundCenter As String) As String
			Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
		  ' Query to get the highest REQ_ID from both tables
            Dim SQL As String = $"
                SELECT MAX(CAST(SUBSTRING(REQ_ID, CHARINDEX('_', REQ_ID) + 1, LEN(REQ_ID)) AS INT)) AS MaxID
                FROM (
                    SELECT REQ_ID FROM XFC_CMD_PGM_REQ  WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                    UNION ALL
                    SELECT REQ_ID FROM XFC_CMD_SPLN_REQ WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                ) AS Combined"
'BRApi.ErrorLog.LogMessage(si,"SQL: " & SQL)
			
			Dim dtREQID As DataTable = New DataTable()
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			 	dtREQID = BRApi.Database.ExecuteSql(dbConn,SQL,True)
			End Using
			
			Dim nextID As Integer = 1
			If (Not dtREQID Is Nothing) AndAlso (Not dtREQID.Rows.Count = 0) AndAlso (Not IsDBNull(dtREQID.Rows(0)("MaxID"))) Then
			    Dim maxID As Integer = Convert.ToInt32(dtREQID.Rows(0)("MaxID"))
                nextID = maxID + 1
			End If
			
			Dim modifiedFC As String = fundCenter
			modifiedFC = modifiedFC.Replace("_General", "")
			If modifiedFC.Length = 3 Then modifiedFC = modifiedFC & "xx"
			Dim nextREQ_ID As String = modifiedFC &"_" & nextID.ToString("D5")
'BRApi.ErrorLog.LogMessage(si,"nextREQ_ID: " &nextREQ_ID)				

			Return nextREQ_ID
        End Function
#End Region	

#Region "Filter Datatable for grideview"
Public Function FilterTable(ByVal dt As DataTable) As Object

Dim dataView As DataView = dt.DefaultView
Dim getDataArgs As DashboardDynamicGridGetDataArgs = args.GetDataArgs
Dim PagedGridColumnFilters As List(Of PagedGridColumnFilter) = getDataArgs.PagedGridColumnFilters
Dim pagedGridDbOrderBys As List(Of DbOrderBy) = getDataArgs.PagedGridDbOrderBys				
Dim dbWheresForFilters As List(Of DbWhere) = New List(Of DbWhere)()					
			 
		 If pagedGridColumnFilters IsNot Nothing AndAlso pagedGridColumnFilters.Count > 0 Then
             For Each pagedGridColumnFilter As PagedGridColumnFilter In pagedGridColumnFilters	
                Dim columnName As String = pagedGridColumnFilter.PropertyName   
                    If Not String.IsNullOrEmpty(columnName) Then
                     Dim dataType As Type = Nothing    
						If dt.Columns.Contains(columnName) Then
                			dataType = dt.Columns(columnName).DataType
            			End If	
                        If dataType IsNot Nothing Then   
						  Dim dbWheresTemp As List(Of DbWhere) = pagedGridColumnFilter.CreateDbWheres(columnName, dataType, False, Nothing)
								' BRApi.ErrorLog.LogMessage(si, $"
'                                    ColumnName = {columnName}
'                                    DbOperator1 = {pagedGridColumnFilter.FilterOperator1.ToString()} 
'                                    Value1 = {pagedGridColumnFilter.FilterValue1.ToString()}
'                                    MatchCase1 = {pagedGridColumnFilter.FilterMatchCase1.ToString()}
                                    
'                                    DbOperator2 = {pagedGridColumnFilter.FilterOperator2.ToString()}
'                                    Value2 = {pagedGridColumnFilter.FilterValue2.ToString()} 
'                                    MatchCase2 = {pagedGridColumnFilter.FilterMatchCase2.ToString()}
'                                    ")
		
            				If dbWheresTemp IsNot Nothing AndAlso dbWheresTemp.Count > 0 Then
								dbWheresForFilters.AddRange(dbWheresTemp)
                            End If
						End If     
                       End If
				Next 
		End If 
						 
Dim rowFilterBuilder As StringBuilder = New StringBuilder()
	If dbWheresForFilters.Count > 0 Then
	  Dim firstCondition As Boolean = True				    
		For Each sdbWhere As DbWhere In dbWheresForFilters			  
		 If Not String.IsNullOrEmpty(CType(sdbWhere.ColumnName, String)) AndAlso CType(sdbWhere.ParamValue, Object) IsNot Nothing Then
           If Not firstCondition Then
			Dim strUseORVersusAnd = String.Empty
              strUseORVersusAnd = sdbWhere.AppendWithORVersusAND.ToString()
				If strUseORVersusAnd.XFEqualsIgnoreCase("False") Then
                   rowFilterBuilder.Append(" OR ")	
                Else
                   rowFilterBuilder.Append(" AND ")	
                End If		  
                                    
             Else
               firstCondition = False
             End If
        
			 Dim brackets() As Char = {"["c, "]"c}
             Dim FormatString As String = Me.FormatDbWhereToRowFilter(si, sdbWhere,dt.Columns.Item(sdbWhere.ColumnName.Trim(brackets)).DataType)
                rowFilterBuilder.Append(FormatString)
					            
			End If
		Next
	End If
					
Dim filterexpersion As String = rowFilterBuilder.ToString()
dataView.RowFilter = filterexpersion
Dim entDT As DataTable = dataView.ToTable() ' Creates a new DataTable with filtered rows			

Return entDT								
					
End Function			
					
#End Region

#Region "Format Row for Filters"
Public Function FormatDbWhereToRowFilter(si As SessionInfo, dbwhere As Object, datatype As type) As Object
   
    Dim condition As String = String.Empty
    Dim columnName As String = CType(dbWhere.ColumnName, String)
    Dim valueString As String = CType(dbWhere.ParamValue, Object).ToString()
    Dim op As String = CType(dbWhere.DbOperator, Object).ToString().Trim().ToUpper() ' Normalize operator
   
	If valueString Is Nothing OrElse String.IsNullOrEmpty(valueString) Then Return String.Empty
   
    Dim escapedValue As String = valueString.Replace("'", "''")

    If dataType Is GetType(String) Then
        Select Case op
            Case "ISEQUALTO"
                condition = $"{dbWhere.ColumnName} = '{escapedValue}'"
            Case "ISNOTEQUALTO"
                condition = $"{dbWhere.ColumnName} <> '{escapedValue}'"
            Case "CONTAINS"
                condition = $"{dbWhere.ColumnName} LIKE '%{escapedValue}%'"
			Case "DOESNOTCONTAIN"
                condition = $"{dbWhere.ColumnName} NOT LIKE '%{escapedValue}%'"
			Case "STARTSWITH"
                condition = $"{dbWhere.ColumnName} LIKE '{escapedValue}%'"
			Case "ENDSWITH"
                condition = $"{dbWhere.ColumnName} LIKE '%{escapedValue}'"
            Case Else
                ' Default for strings
                condition = $"{dbWhere.ColumnName} {dbWhere.DbOperator.ToString()} '{escapedValue}'"
        End Select
    Else If dataType Is GetType(DateTime) Then
        ' Dates must be formatted as #MM/DD/YYYY# in RowFilter
        If DateTime.TryParse(valueString, New DateTime()) Then
            Dim dt As DateTime = CDate(valueString)
            condition = $"{dbWhere.ColumnName} {dbWhere.DbOperator.ToString()} #{dt.ToString("MM/dd/yyyy")}#"
        End If
    Else         
        If IsNumeric(valueString) Then
             condition = $"{dbWhere.ColumnName} {dbWhere.DbOperator.ToString()} {valueString}"
        End If
    End If
    
    Return condition
End Function
#End Region



End Class
End Namespace