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
Imports Workspace.GBL.GBL_Assembly
Imports Microsoft.Data.SqlClient
Imports Workspace.GBL

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_SPLN_Dyn_Grid_Svc
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
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_Imported_Req") Then
						Return dg_CMD_SPLN_Imported_Req()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_ReviewREQs") Then
				    	Return dg_CMD_SPLN_ReviewREQs()            
					End If
					
										
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_RolloverREQ") Then
				    	Return dg_CMD_SPLN_Rollover_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_REQCivFundingLine") Then
				    	Return dg_CMD_SPLN_REQCivFundingLine()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_REQWHFundingLine") Then
				    	Return dg_CMD_SPLN_REQWHFundingLine()            
					End If
					
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_REQFundingLine") Then
				    	Return dg_CMD_SPLN_REQFundingLine()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_REQAmtsReview") Then
				    	Return dg_CMD_SPLN_REQAmtsReview()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_Create") Then
						Return dg_CMD_SPLN_Create()
					End If
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_ReviseSPLNRequirements") Then
						Return dg_CMD_SPLN_ReviseSPLNRequirements()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_PackageSubmission") Then
						Return dg_CMD_SPLN_PackageSubmission()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_Attachments") Then
						Return dg_CMD_SPLN_Attachments()
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
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_REQFundingLine") Then
				    	Return Save_dg_CMD_SPLN_REQFundingLine()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_Create") Then
						Return Save_dg_CMD_SPLN_Create()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_SPLN_ReviseSPLNRequirements") Then
						Return Save_dg_CMD_SPLN_REQRevisions()
					End If
				End If
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
#End Region			

#Region "CMD PGM Import"
		Private Function dg_CMD_SPLN_Imported_Req() As XFDynamicGridGetDataResult
		
			'Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim tableName As String = "CMD_SPLN_Import_" & sScenario
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName,tableName)

			If dt Is Nothing Then Return Nothing
				
			Dim skp As New Dictionary(Of String, Object)

			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
#Region "columnDefinitions"			
			Dim ValidationError As New XFDynamicGridColumnDefinition()
			ValidationError.ColumnName = "ValidationError"
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
			
			Dim CMD_SPLN_REQ_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_REQ_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_REQ_ID.IsFromTable = True
			CMD_SPLN_REQ_ID.IsVisible = False
			CMD_SPLN_REQ_ID.AllowUpdates = False
			columnDefinitions.Add(CMD_SPLN_REQ_ID)
			
			Dim FundCenter As New XFDynamicGridColumnDefinition()
			FundCenter.ColumnName = "FundCenter"
			FundCenter.IsFromTable = True
			FundCenter.IsVisible = True
			FundCenter.AllowUpdates = False
			columnDefinitions.Add(FundCenter)
			
			Dim APPN As New XFDynamicGridColumnDefinition()
			APPN.ColumnName = "FundCode"
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
			
			Dim C_Type As New XFDynamicGridColumnDefinition() 'Ctype
			C_Type.ColumnName = "UD5"
			C_Type.IsFromTable = True
			C_Type.IsVisible = False 'set to true if used
			C_Type.AllowUpdates = False
			columnDefinitions.Add(C_Type)
			
			Dim Cycle As New XFDynamicGridColumnDefinition()
			Cycle.ColumnName = "Cycle"
			Cycle.IsFromTable = True
			Cycle.IsVisible = True
			Cycle.AllowUpdates = False
			columnDefinitions.Add(Cycle)
			
			Dim COMM1 As New XFDynamicGridColumnDefinition()
			COMM1.ColumnName = "COM_M1"
			COMM1.IsFromTable = True
			COMM1.IsVisible = True
			COMM1.AllowUpdates = False
			columnDefinitions.Add(COMM1)
			
			Dim COMM2 As New XFDynamicGridColumnDefinition()
			COMM2.ColumnName = "COM_M2"
			COMM2.IsFromTable = True
			COMM2.IsVisible = True
			COMM2.AllowUpdates = False
			columnDefinitions.Add(COMM2)
			
			Dim COMM3 As New XFDynamicGridColumnDefinition()
			COMM3.ColumnName = "COM_M3"
			COMM3.IsFromTable = True
			COMM3.IsVisible = True
			COMM3.AllowUpdates = False
			columnDefinitions.Add(COMM3)
			
			Dim COMM4 As New XFDynamicGridColumnDefinition()
			COMM4.ColumnName = "COM_M4"
			COMM4.IsFromTable = True
			COMM4.IsVisible = True
			COMM4.AllowUpdates = False
			columnDefinitions.Add(COMM4)
			
			Dim COMM5 As New XFDynamicGridColumnDefinition()
			COMM5.ColumnName = "COM_M5"
			COMM5.IsFromTable = True
			COMM5.IsVisible = True
			COMM5.AllowUpdates = False
			columnDefinitions.Add(COMM5)
			
			Dim COMM6 As New XFDynamicGridColumnDefinition()
			COMM6.ColumnName = "COM_M6"
			COMM6.IsFromTable = True
			COMM6.IsVisible = True
			COMM6.AllowUpdates = False
			columnDefinitions.Add(COMM6)
			
			Dim COMM7 As New XFDynamicGridColumnDefinition()
			COMM7.ColumnName = "COM_M7"
			COMM7.IsFromTable = True
			COMM7.IsVisible = True
			COMM7.AllowUpdates = False
			columnDefinitions.Add(COMM7)
			
			Dim COMM8 As New XFDynamicGridColumnDefinition()
			COMM8.ColumnName = "COM_M8"
			COMM8.IsFromTable = True
			COMM8.IsVisible = True
			COMM8.AllowUpdates = False
			columnDefinitions.Add(COMM8)
			
			Dim COMM9 As New XFDynamicGridColumnDefinition()
			COMM9.ColumnName = "COM_M9"
			COMM9.IsFromTable = True
			COMM9.IsVisible = True
			COMM9.AllowUpdates = False
			columnDefinitions.Add(COMM9)
			
			Dim COMM10 As New XFDynamicGridColumnDefinition()
			COMM10.ColumnName = "COM_M10"
			COMM10.IsFromTable = True
			COMM10.IsVisible = True
			COMM10.AllowUpdates = False
			columnDefinitions.Add(COMM10)
			
			Dim COMM11 As New XFDynamicGridColumnDefinition()
			COMM11.ColumnName = "COM_M11"
			COMM11.IsFromTable = True
			COMM11.IsVisible = True
			COMM11.AllowUpdates = False
			columnDefinitions.Add(COMM11)
			
			Dim COMM12 As New XFDynamicGridColumnDefinition()
			COMM12.ColumnName = "COM_M12"
			COMM12.IsFromTable = True
			COMM12.IsVisible = True
			COMM12.AllowUpdates = False
			columnDefinitions.Add(COMM12)
			
			Dim COMCarryOver As New XFDynamicGridColumnDefinition()
			COMCarryOver.ColumnName = "COM_Carryover"
			COMCarryOver.IsFromTable = True
			COMCarryOver.IsVisible = True
			COMCarryOver.AllowUpdates = False
			columnDefinitions.Add(COMCarryOver)
			
			Dim OBLM1 As New XFDynamicGridColumnDefinition()
			OBLM1.ColumnName = "OBL_M1"
			OBLM1.IsFromTable = True
			OBLM1.IsVisible = True
			OBLM1.AllowUpdates = False
			columnDefinitions.Add(OBLM1)
			
			Dim OBLM2 As New XFDynamicGridColumnDefinition()
			OBLM2.ColumnName = "OBL_M2"
			OBLM2.IsFromTable = True
			OBLM2.IsVisible = True
			OBLM2.AllowUpdates = False
			columnDefinitions.Add(OBLM2)
			
			Dim OBLM3 As New XFDynamicGridColumnDefinition()
			OBLM3.ColumnName = "OBL_M3"
			OBLM3.IsFromTable = True
			OBLM3.IsVisible = True
			OBLM3.AllowUpdates = False
			columnDefinitions.Add(OBLM3)
			
			Dim OBLM4 As New XFDynamicGridColumnDefinition()
			OBLM4.ColumnName = "OBL_M4"
			OBLM4.IsFromTable = True
			OBLM4.IsVisible = True
			OBLM4.AllowUpdates = False
			columnDefinitions.Add(OBLM4)
			
			Dim OBLM5 As New XFDynamicGridColumnDefinition()
			OBLM5.ColumnName = "OBL_M5"
			OBLM5.IsFromTable = True
			OBLM5.IsVisible = True
			OBLM5.AllowUpdates = False
			columnDefinitions.Add(OBLM5)
			
			Dim OBLM6 As New XFDynamicGridColumnDefinition()
			OBLM6.ColumnName = "OBL_M6"
			OBLM6.IsFromTable = True
			OBLM6.IsVisible = True
			OBLM6.AllowUpdates = False
			columnDefinitions.Add(OBLM6)
			
			Dim OBLM7 As New XFDynamicGridColumnDefinition()
			OBLM7.ColumnName = "OBL_M7"
			OBLM7.IsFromTable = True
			OBLM7.IsVisible = True
			OBLM7.AllowUpdates = False
			columnDefinitions.Add(OBLM7)
			
			Dim OBLM8 As New XFDynamicGridColumnDefinition()
			OBLM8.ColumnName = "OBL_M8"
			OBLM8.IsFromTable = True
			OBLM8.IsVisible = True
			OBLM8.AllowUpdates = False
			columnDefinitions.Add(OBLM8)
			
			Dim OBLM9 As New XFDynamicGridColumnDefinition()
			OBLM9.ColumnName = "OBL_M9"
			OBLM9.IsFromTable = True
			OBLM9.IsVisible = True
			OBLM9.AllowUpdates = False
			columnDefinitions.Add(OBLM9)
			
			Dim OBLM10 As New XFDynamicGridColumnDefinition()
			OBLM10.ColumnName = "OBL_M10"
			OBLM10.IsFromTable = True
			OBLM10.IsVisible = True
			OBLM10.AllowUpdates = False
			columnDefinitions.Add(OBLM10)
			
			Dim OBLM11 As New XFDynamicGridColumnDefinition()
			OBLM11.ColumnName = "OBL_M11"
			OBLM11.IsFromTable = True
			OBLM11.IsVisible = True
			OBLM11.AllowUpdates = False
			columnDefinitions.Add(OBLM11)
			
			Dim OBLM12 As New XFDynamicGridColumnDefinition()
			OBLM12.ColumnName = "OBL_M12"
			OBLM12.IsFromTable = True
			OBLM12.IsVisible = True
			OBLM12.AllowUpdates = False
			columnDefinitions.Add(OBLM12)
			
			Dim OBLCarryOver As New XFDynamicGridColumnDefinition()
			OBLCarryOver.ColumnName = "OBL_Carryover"
			OBLCarryOver.IsFromTable = True
			OBLCarryOver.IsVisible = True
			OBLCarryOver.AllowUpdates = False
			columnDefinitions.Add(OBLCarryOver)
			
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
			ContractManYearEquiv_CME.ColumnName = "CME"
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
			
			Dim REQ_Type As New XFDynamicGridColumnDefinition()
			REQ_Type.ColumnName = "RequirementType"
			REQ_Type.IsFromTable = True
			REQ_Type.IsVisible = False
			REQ_Type.AllowUpdates = False
			columnDefinitions.Add(REQ_Type)
			
			Dim UNIT_OF_MEASURE As New XFDynamicGridColumnDefinition()
			UNIT_OF_MEASURE.ColumnName = "UNIT_OF_MEASURE"
			UNIT_OF_MEASURE.IsFromTable = True
			UNIT_OF_MEASURE.IsVisible = False
			UNIT_OF_MEASURE.AllowUpdates = False
			columnDefinitions.Add(UNIT_OF_MEASURE)
		
			Dim REQ_ID_Type As New XFDynamicGridColumnDefinition()
			REQ_ID_Type.ColumnName = "REQ_ID_Type"
			REQ_ID_Type.IsFromTable = True
			REQ_ID_Type.IsVisible = False
			REQ_ID_Type.AllowUpdates = False
			columnDefinitions.Add(REQ_ID_Type)
			
			Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Status"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.AllowUpdates = False
			columnDefinitions.Add(Status)
			
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
			UD7.IsVisible = False
			UD7.AllowUpdates = False
			columnDefinitions.Add(UD7)
			
			Dim UD8 As New XFDynamicGridColumnDefinition()
			UD8.ColumnName = "UD8"
			UD8.IsFromTable = True
			UD8.IsVisible = False
			UD8.AllowUpdates = False
			columnDefinitions.Add(UD8)
			
			Dim FY_Total As New XFDynamicGridColumnDefinition()
			FY_Total.ColumnName = "Yearly"
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
			
			Dim quater1 As New XFDynamicGridColumnDefinition()
			quater1.ColumnName = "Quarter1"
			quater1.IsFromTable = True
			quater1.IsVisible = False
			quater1.AllowUpdates = False
			columnDefinitions.Add(quater1)
			
			Dim quater2 As New XFDynamicGridColumnDefinition()
			quater2.ColumnName = "Quarter2"
			quater2.IsFromTable = True
			quater2.IsVisible = False
			quater2.AllowUpdates = False
			columnDefinitions.Add(quater2)
			
			Dim quater3 As New XFDynamicGridColumnDefinition()
			quater3.ColumnName = "Quarter3"
			quater3.IsFromTable = True
			quater3.IsVisible = False
			quater3.AllowUpdates = False
			columnDefinitions.Add(quater3)
			
			Dim quater4 As New XFDynamicGridColumnDefinition()
			quater4.ColumnName = "Quarter4"
			quater4.IsFromTable = True
			quater4.IsVisible = False
			quater4.AllowUpdates = False
			columnDefinitions.Add(quater4)


#End Region

			Dim xfdt As New XFDataTable(si,dt,Nothing,10000)
			Dim rslt As New XFDynamicGridGetDataResult(xfdt,columnDefinitions,DataAccessLevel.AllAccess)
'BRApi.ErrorLog.LogMessage(si, "row count: " & rslt.DataTable.Rows.Count )
			Return rslt
			
		End Function
#End Region	
					
#Region "Constants"
Private BR_CMD_SPLNDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_SPLN_DataSet.MainClass
Private BR_CMD_SPLN_String_Helper As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.CMD_SPLN_String_Helper.MainClass
#End Region	

#Region "REQ List"
Private Function dg_CMD_SPLN_ReviewREQs() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		

'   Dim APPN As New XFDynamicGridColumnDefinition()
'			APPN.ColumnName = "Funds Center"
'			APPN.IsFromTable = True
'			APPN.IsVisible = True
			
'   Dim MDEP As New XFDynamicGridColumnDefinition()
'			MDEP.ColumnName = "Funds Center"
'			MDEP.IsFromTable = True
'			MDEP.IsVisible = True
			
			
'	Dim APE As New XFDynamicGridColumnDefinition()
'			APE.ColumnName = "Funds Center"
'			APE.IsFromTable = True
'			APE.IsVisible = True
			
'	Dim DollarType As New XFDynamicGridColumnDefinition()
'			DollarType.ColumnName = "Funds Center"
'			DollarType.IsFromTable = True
'			DollarType.IsVisible = True
			
'   Dim ObjectClass As New XFDynamicGridColumnDefinition()
'			ObjectClass.ColumnName = "Funds Center"
'			ObjectClass.IsFromTable = True
'			ObjectClass.IsVisible = True
			
'    Dim ctypecol As New XFDynamicGridColumnDefinition()
'			ctypecol.ColumnName = "Funds Center"
'			ctypecol.IsFromTable = True
'			ctypecol.IsVisible = True

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
    
    Dim COMM1 As New XFDynamicGridColumnDefinition()
			COMM1.ColumnName = "COM_M1"
			COMM1.IsFromTable = True
			COMM1.IsVisible = True
			COMM1.AllowUpdates = False
			columnDefinitions.Add(COMM1)
			
			Dim COMM2 As New XFDynamicGridColumnDefinition()
			COMM2.ColumnName = "COM_M2"
			COMM2.IsFromTable = True
			COMM2.IsVisible = True
			COMM2.AllowUpdates = False
			columnDefinitions.Add(COMM2)
			
			Dim COMM3 As New XFDynamicGridColumnDefinition()
			COMM3.ColumnName = "COM_M3"
			COMM3.IsFromTable = True
			COMM3.IsVisible = True
			COMM3.AllowUpdates = False
			columnDefinitions.Add(COMM3)
			
			Dim COMM4 As New XFDynamicGridColumnDefinition()
			COMM4.ColumnName = "COM_M4"
			COMM4.IsFromTable = True
			COMM4.IsVisible = True
			COMM4.AllowUpdates = False
			columnDefinitions.Add(COMM4)
			
			Dim COMM5 As New XFDynamicGridColumnDefinition()
			COMM5.ColumnName = "COM_M5"
			COMM5.IsFromTable = True
			COMM5.IsVisible = True
			COMM5.AllowUpdates = False
			columnDefinitions.Add(COMM5)
			
			Dim COMM6 As New XFDynamicGridColumnDefinition()
			COMM6.ColumnName = "COM_M6"
			COMM6.IsFromTable = True
			COMM6.IsVisible = True
			COMM6.AllowUpdates = False
			columnDefinitions.Add(COMM6)
			
			Dim COMM7 As New XFDynamicGridColumnDefinition()
			COMM7.ColumnName = "COM_M7"
			COMM7.IsFromTable = True
			COMM7.IsVisible = True
			COMM7.AllowUpdates = False
			columnDefinitions.Add(COMM7)
			
			Dim COMM8 As New XFDynamicGridColumnDefinition()
			COMM8.ColumnName = "COM_M8"
			COMM8.IsFromTable = True
			COMM8.IsVisible = True
			COMM8.AllowUpdates = False
			columnDefinitions.Add(COMM8)
			
			Dim COMM9 As New XFDynamicGridColumnDefinition()
			COMM9.ColumnName = "COM_M9"
			COMM9.IsFromTable = True
			COMM9.IsVisible = True
			COMM9.AllowUpdates = False
			columnDefinitions.Add(COMM9)
			
			Dim COMM10 As New XFDynamicGridColumnDefinition()
			COMM10.ColumnName = "COM_M10"
			COMM10.IsFromTable = True
			COMM10.IsVisible = True
			COMM10.AllowUpdates = False
			columnDefinitions.Add(COMM10)
			
			Dim COMM11 As New XFDynamicGridColumnDefinition()
			COMM11.ColumnName = "COM_M11"
			COMM11.IsFromTable = True
			COMM11.IsVisible = True
			COMM11.AllowUpdates = False
			columnDefinitions.Add(COMM11)
			
			Dim COMM12 As New XFDynamicGridColumnDefinition()
			COMM12.ColumnName = "COM_M12"
			COMM12.IsFromTable = True
			COMM12.IsVisible = True
			COMM12.AllowUpdates = False
			columnDefinitions.Add(COMM12)
			
			Dim COMCarryOver As New XFDynamicGridColumnDefinition()
			COMCarryOver.ColumnName = "COM_Carryover"
			COMCarryOver.IsFromTable = True
			COMCarryOver.IsVisible = True
			COMCarryOver.AllowUpdates = False
			columnDefinitions.Add(COMCarryOver)
			
			Dim OBLM1 As New XFDynamicGridColumnDefinition()
			OBLM1.ColumnName = "OBL_M1"
			OBLM1.IsFromTable = True
			OBLM1.IsVisible = True
			OBLM1.AllowUpdates = False
			columnDefinitions.Add(OBLM1)
			
			Dim OBLM2 As New XFDynamicGridColumnDefinition()
			OBLM2.ColumnName = "OBL_M2"
			OBLM2.IsFromTable = True
			OBLM2.IsVisible = True
			OBLM2.AllowUpdates = False
			columnDefinitions.Add(OBLM2)
			
			Dim OBLM3 As New XFDynamicGridColumnDefinition()
			OBLM3.ColumnName = "OBL_M3"
			OBLM3.IsFromTable = True
			OBLM3.IsVisible = True
			OBLM3.AllowUpdates = False
			columnDefinitions.Add(OBLM3)
			
			Dim OBLM4 As New XFDynamicGridColumnDefinition()
			OBLM4.ColumnName = "OBL_M4"
			OBLM4.IsFromTable = True
			OBLM4.IsVisible = True
			OBLM4.AllowUpdates = False
			columnDefinitions.Add(OBLM4)
			
			Dim OBLM5 As New XFDynamicGridColumnDefinition()
			OBLM5.ColumnName = "OBL_M5"
			OBLM5.IsFromTable = True
			OBLM5.IsVisible = True
			OBLM5.AllowUpdates = False
			columnDefinitions.Add(OBLM5)
			
			Dim OBLM6 As New XFDynamicGridColumnDefinition()
			OBLM6.ColumnName = "OBL_M6"
			OBLM6.IsFromTable = True
			OBLM6.IsVisible = True
			OBLM6.AllowUpdates = False
			columnDefinitions.Add(OBLM6)
			
			Dim OBLM7 As New XFDynamicGridColumnDefinition()
			OBLM7.ColumnName = "OBL_M7"
			OBLM7.IsFromTable = True
			OBLM7.IsVisible = True
			OBLM7.AllowUpdates = False
			columnDefinitions.Add(OBLM7)
			
			Dim OBLM8 As New XFDynamicGridColumnDefinition()
			OBLM8.ColumnName = "OBL_M8"
			OBLM8.IsFromTable = True
			OBLM8.IsVisible = True
			OBLM8.AllowUpdates = False
			columnDefinitions.Add(OBLM8)
			
			Dim OBLM9 As New XFDynamicGridColumnDefinition()
			OBLM9.ColumnName = "OBL_M9"
			OBLM9.IsFromTable = True
			OBLM9.IsVisible = True
			OBLM9.AllowUpdates = False
			columnDefinitions.Add(OBLM9)
			
			Dim OBLM10 As New XFDynamicGridColumnDefinition()
			OBLM10.ColumnName = "OBL_M10"
			OBLM10.IsFromTable = True
			OBLM10.IsVisible = True
			OBLM10.AllowUpdates = False
			columnDefinitions.Add(OBLM10)
			
			Dim OBLM11 As New XFDynamicGridColumnDefinition()
			OBLM11.ColumnName = "OBL_M11"
			OBLM11.IsFromTable = True
			OBLM11.IsVisible = True
			OBLM11.AllowUpdates = False
			columnDefinitions.Add(OBLM11)
			
			Dim OBLM12 As New XFDynamicGridColumnDefinition()
			OBLM12.ColumnName = "OBL_M12"
			OBLM12.IsFromTable = True
			OBLM12.IsVisible = True
			OBLM12.AllowUpdates = False
			columnDefinitions.Add(OBLM12)
			
			Dim OBLCarryOver As New XFDynamicGridColumnDefinition()
			OBLCarryOver.ColumnName = "OBL_Carryover"
			OBLCarryOver.IsFromTable = True
			OBLCarryOver.IsVisible = True
			OBLCarryOver.AllowUpdates = False
			columnDefinitions.Add(OBLCarryOver)
			
	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Status"
			columnDefinitions.Add(Status)
						
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars("BL_CMD_SPLN_FundsCenter")
					
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
					'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_SPLNDataSet.Main(si, globals, api, dargs)

						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function
#End Region

#Region "CivPay Funding Line Details"
Private Function dg_CMD_SPLN_REQCivFundingLine() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		

'   Dim APPN As New XFDynamicGridColumnDefinition()
'			APPN.ColumnName = "Funds Center"
'			APPN.IsFromTable = True
'			APPN.IsVisible = True
			
'   Dim MDEP As New XFDynamicGridColumnDefinition()
'			MDEP.ColumnName = "Funds Center"
'			MDEP.IsFromTable = True
'			MDEP.IsVisible = True
			
			
'	Dim APE As New XFDynamicGridColumnDefinition()
'			APE.ColumnName = "Funds Center"
'			APE.IsFromTable = True
'			APE.IsVisible = True
			
'	Dim DollarType As New XFDynamicGridColumnDefinition()
'			DollarType.ColumnName = "Funds Center"
'			DollarType.IsFromTable = True
'			DollarType.IsVisible = True
			
'   Dim ObjectClass As New XFDynamicGridColumnDefinition()
'			ObjectClass.ColumnName = "Funds Center"
'			ObjectClass.IsFromTable = True
'			ObjectClass.IsVisible = True
			
'    Dim ctypecol As New XFDynamicGridColumnDefinition()
'			ctypecol.ColumnName = "Funds Center"
'			ctypecol.IsFromTable = True
'			ctypecol.IsVisible = True

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
    
    Dim COMM1 As New XFDynamicGridColumnDefinition()
			COMM1.ColumnName = "COM_M1"
			COMM1.IsFromTable = True
			COMM1.IsVisible = True
			COMM1.AllowUpdates = False
			columnDefinitions.Add(COMM1)
			
			Dim COMM2 As New XFDynamicGridColumnDefinition()
			COMM2.ColumnName = "COM_M2"
			COMM2.IsFromTable = True
			COMM2.IsVisible = True
			COMM2.AllowUpdates = False
			columnDefinitions.Add(COMM2)
			
			Dim COMM3 As New XFDynamicGridColumnDefinition()
			COMM3.ColumnName = "COM_M3"
			COMM3.IsFromTable = True
			COMM3.IsVisible = True
			COMM3.AllowUpdates = False
			columnDefinitions.Add(COMM3)
			
			Dim COMM4 As New XFDynamicGridColumnDefinition()
			COMM4.ColumnName = "COM_M4"
			COMM4.IsFromTable = True
			COMM4.IsVisible = True
			COMM4.AllowUpdates = False
			columnDefinitions.Add(COMM4)
			
			Dim COMM5 As New XFDynamicGridColumnDefinition()
			COMM5.ColumnName = "COM_M5"
			COMM5.IsFromTable = True
			COMM5.IsVisible = True
			COMM5.AllowUpdates = False
			columnDefinitions.Add(COMM5)
			
			Dim COMM6 As New XFDynamicGridColumnDefinition()
			COMM6.ColumnName = "COM_M6"
			COMM6.IsFromTable = True
			COMM6.IsVisible = True
			COMM6.AllowUpdates = False
			columnDefinitions.Add(COMM6)
			
			Dim COMM7 As New XFDynamicGridColumnDefinition()
			COMM7.ColumnName = "COM_M7"
			COMM7.IsFromTable = True
			COMM7.IsVisible = True
			COMM7.AllowUpdates = False
			columnDefinitions.Add(COMM7)
			
			Dim COMM8 As New XFDynamicGridColumnDefinition()
			COMM8.ColumnName = "COM_M8"
			COMM8.IsFromTable = True
			COMM8.IsVisible = True
			COMM8.AllowUpdates = False
			columnDefinitions.Add(COMM8)
			
			Dim COMM9 As New XFDynamicGridColumnDefinition()
			COMM9.ColumnName = "COM_M9"
			COMM9.IsFromTable = True
			COMM9.IsVisible = True
			COMM9.AllowUpdates = False
			columnDefinitions.Add(COMM9)
			
			Dim COMM10 As New XFDynamicGridColumnDefinition()
			COMM10.ColumnName = "COM_M10"
			COMM10.IsFromTable = True
			COMM10.IsVisible = True
			COMM10.AllowUpdates = False
			columnDefinitions.Add(COMM10)
			
			Dim COMM11 As New XFDynamicGridColumnDefinition()
			COMM11.ColumnName = "COM_M11"
			COMM11.IsFromTable = True
			COMM11.IsVisible = True
			COMM11.AllowUpdates = False
			columnDefinitions.Add(COMM11)
			
			Dim COMM12 As New XFDynamicGridColumnDefinition()
			COMM12.ColumnName = "COM_M12"
			COMM12.IsFromTable = True
			COMM12.IsVisible = True
			COMM12.AllowUpdates = False
			columnDefinitions.Add(COMM12)
			
			Dim COMCarryOver As New XFDynamicGridColumnDefinition()
			COMCarryOver.ColumnName = "COM_Carryover"
			COMCarryOver.IsFromTable = True
			COMCarryOver.IsVisible = True
			COMCarryOver.AllowUpdates = False
			columnDefinitions.Add(COMCarryOver)
			
			Dim OBLM1 As New XFDynamicGridColumnDefinition()
			OBLM1.ColumnName = "OBL_M1"
			OBLM1.IsFromTable = True
			OBLM1.IsVisible = True
			OBLM1.AllowUpdates = False
			columnDefinitions.Add(OBLM1)
			
			Dim OBLM2 As New XFDynamicGridColumnDefinition()
			OBLM2.ColumnName = "OBL_M2"
			OBLM2.IsFromTable = True
			OBLM2.IsVisible = True
			OBLM2.AllowUpdates = False
			columnDefinitions.Add(OBLM2)
			
			Dim OBLM3 As New XFDynamicGridColumnDefinition()
			OBLM3.ColumnName = "OBL_M3"
			OBLM3.IsFromTable = True
			OBLM3.IsVisible = True
			OBLM3.AllowUpdates = False
			columnDefinitions.Add(OBLM3)
			
			Dim OBLM4 As New XFDynamicGridColumnDefinition()
			OBLM4.ColumnName = "OBL_M4"
			OBLM4.IsFromTable = True
			OBLM4.IsVisible = True
			OBLM4.AllowUpdates = False
			columnDefinitions.Add(OBLM4)
			
			Dim OBLM5 As New XFDynamicGridColumnDefinition()
			OBLM5.ColumnName = "OBL_M5"
			OBLM5.IsFromTable = True
			OBLM5.IsVisible = True
			OBLM5.AllowUpdates = False
			columnDefinitions.Add(OBLM5)
			
			Dim OBLM6 As New XFDynamicGridColumnDefinition()
			OBLM6.ColumnName = "OBL_M6"
			OBLM6.IsFromTable = True
			OBLM6.IsVisible = True
			OBLM6.AllowUpdates = False
			columnDefinitions.Add(OBLM6)
			
			Dim OBLM7 As New XFDynamicGridColumnDefinition()
			OBLM7.ColumnName = "OBL_M7"
			OBLM7.IsFromTable = True
			OBLM7.IsVisible = True
			OBLM7.AllowUpdates = False
			columnDefinitions.Add(OBLM7)
			
			Dim OBLM8 As New XFDynamicGridColumnDefinition()
			OBLM8.ColumnName = "OBL_M8"
			OBLM8.IsFromTable = True
			OBLM8.IsVisible = True
			OBLM8.AllowUpdates = False
			columnDefinitions.Add(OBLM8)
			
			Dim OBLM9 As New XFDynamicGridColumnDefinition()
			OBLM9.ColumnName = "OBL_M9"
			OBLM9.IsFromTable = True
			OBLM9.IsVisible = True
			OBLM9.AllowUpdates = False
			columnDefinitions.Add(OBLM9)
			
			Dim OBLM10 As New XFDynamicGridColumnDefinition()
			OBLM10.ColumnName = "OBL_M10"
			OBLM10.IsFromTable = True
			OBLM10.IsVisible = True
			OBLM10.AllowUpdates = False
			columnDefinitions.Add(OBLM10)
			
			Dim OBLM11 As New XFDynamicGridColumnDefinition()
			OBLM11.ColumnName = "OBL_M11"
			OBLM11.IsFromTable = True
			OBLM11.IsVisible = True
			OBLM11.AllowUpdates = False
			columnDefinitions.Add(OBLM11)
			
			Dim OBLM12 As New XFDynamicGridColumnDefinition()
			OBLM12.ColumnName = "OBL_M12"
			OBLM12.IsFromTable = True
			OBLM12.IsVisible = True
			OBLM12.AllowUpdates = False
			columnDefinitions.Add(OBLM12)
			
			Dim OBLCarryOver As New XFDynamicGridColumnDefinition()
			OBLCarryOver.ColumnName = "OBL_Carryover"
			OBLCarryOver.IsFromTable = True
			OBLCarryOver.IsVisible = True
			OBLCarryOver.AllowUpdates = False
			columnDefinitions.Add(OBLCarryOver)
			
	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Status"
			columnDefinitions.Add(Status)
						
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars("BL_CMD_SPLN_FundsCenter")
					
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_SPLNDataSet.Main(si, globals, api, dargs)

						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function
#End Region

#Region "Withhold Funding Line Details"
Private Function dg_CMD_SPLN_REQWHFundingLine() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		

'   Dim APPN As New XFDynamicGridColumnDefinition()
'			APPN.ColumnName = "Funds Center"
'			APPN.IsFromTable = True
'			APPN.IsVisible = True
			
'   Dim MDEP As New XFDynamicGridColumnDefinition()
'			MDEP.ColumnName = "Funds Center"
'			MDEP.IsFromTable = True
'			MDEP.IsVisible = True
			
			
'	Dim APE As New XFDynamicGridColumnDefinition()
'			APE.ColumnName = "Funds Center"
'			APE.IsFromTable = True
'			APE.IsVisible = True
			
'	Dim DollarType As New XFDynamicGridColumnDefinition()
'			DollarType.ColumnName = "Funds Center"
'			DollarType.IsFromTable = True
'			DollarType.IsVisible = True
			
'   Dim ObjectClass As New XFDynamicGridColumnDefinition()
'			ObjectClass.ColumnName = "Funds Center"
'			ObjectClass.IsFromTable = True
'			ObjectClass.IsVisible = True
			
'    Dim ctypecol As New XFDynamicGridColumnDefinition()
'			ctypecol.ColumnName = "Funds Center"
'			ctypecol.IsFromTable = True
'			ctypecol.IsVisible = True

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
    
    Dim COMM1 As New XFDynamicGridColumnDefinition()
			COMM1.ColumnName = "COM_M1"
			COMM1.IsFromTable = True
			COMM1.IsVisible = True
			COMM1.AllowUpdates = False
			columnDefinitions.Add(COMM1)
			
			Dim COMM2 As New XFDynamicGridColumnDefinition()
			COMM2.ColumnName = "COM_M2"
			COMM2.IsFromTable = True
			COMM2.IsVisible = True
			COMM2.AllowUpdates = False
			columnDefinitions.Add(COMM2)
			
			Dim COMM3 As New XFDynamicGridColumnDefinition()
			COMM3.ColumnName = "COM_M3"
			COMM3.IsFromTable = True
			COMM3.IsVisible = True
			COMM3.AllowUpdates = False
			columnDefinitions.Add(COMM3)
			
			Dim COMM4 As New XFDynamicGridColumnDefinition()
			COMM4.ColumnName = "COM_M4"
			COMM4.IsFromTable = True
			COMM4.IsVisible = True
			COMM4.AllowUpdates = False
			columnDefinitions.Add(COMM4)
			
			Dim COMM5 As New XFDynamicGridColumnDefinition()
			COMM5.ColumnName = "COM_M5"
			COMM5.IsFromTable = True
			COMM5.IsVisible = True
			COMM5.AllowUpdates = False
			columnDefinitions.Add(COMM5)
			
			Dim COMM6 As New XFDynamicGridColumnDefinition()
			COMM6.ColumnName = "COM_M6"
			COMM6.IsFromTable = True
			COMM6.IsVisible = True
			COMM6.AllowUpdates = False
			columnDefinitions.Add(COMM6)
			
			Dim COMM7 As New XFDynamicGridColumnDefinition()
			COMM7.ColumnName = "COM_M7"
			COMM7.IsFromTable = True
			COMM7.IsVisible = True
			COMM7.AllowUpdates = False
			columnDefinitions.Add(COMM7)
			
			Dim COMM8 As New XFDynamicGridColumnDefinition()
			COMM8.ColumnName = "COM_M8"
			COMM8.IsFromTable = True
			COMM8.IsVisible = True
			COMM8.AllowUpdates = False
			columnDefinitions.Add(COMM8)
			
			Dim COMM9 As New XFDynamicGridColumnDefinition()
			COMM9.ColumnName = "COM_M9"
			COMM9.IsFromTable = True
			COMM9.IsVisible = True
			COMM9.AllowUpdates = False
			columnDefinitions.Add(COMM9)
			
			Dim COMM10 As New XFDynamicGridColumnDefinition()
			COMM10.ColumnName = "COM_M10"
			COMM10.IsFromTable = True
			COMM10.IsVisible = True
			COMM10.AllowUpdates = False
			columnDefinitions.Add(COMM10)
			
			Dim COMM11 As New XFDynamicGridColumnDefinition()
			COMM11.ColumnName = "COM_M11"
			COMM11.IsFromTable = True
			COMM11.IsVisible = True
			COMM11.AllowUpdates = False
			columnDefinitions.Add(COMM11)
			
			Dim COMM12 As New XFDynamicGridColumnDefinition()
			COMM12.ColumnName = "COM_M12"
			COMM12.IsFromTable = True
			COMM12.IsVisible = True
			COMM12.AllowUpdates = False
			columnDefinitions.Add(COMM12)
			
			Dim COMCarryOver As New XFDynamicGridColumnDefinition()
			COMCarryOver.ColumnName = "COM_Carryover"
			COMCarryOver.IsFromTable = True
			COMCarryOver.IsVisible = True
			COMCarryOver.AllowUpdates = False
			columnDefinitions.Add(COMCarryOver)
			
			Dim OBLM1 As New XFDynamicGridColumnDefinition()
			OBLM1.ColumnName = "OBL_M1"
			OBLM1.IsFromTable = True
			OBLM1.IsVisible = True
			OBLM1.AllowUpdates = False
			columnDefinitions.Add(OBLM1)
			
			Dim OBLM2 As New XFDynamicGridColumnDefinition()
			OBLM2.ColumnName = "OBL_M2"
			OBLM2.IsFromTable = True
			OBLM2.IsVisible = True
			OBLM2.AllowUpdates = False
			columnDefinitions.Add(OBLM2)
			
			Dim OBLM3 As New XFDynamicGridColumnDefinition()
			OBLM3.ColumnName = "OBL_M3"
			OBLM3.IsFromTable = True
			OBLM3.IsVisible = True
			OBLM3.AllowUpdates = False
			columnDefinitions.Add(OBLM3)
			
			Dim OBLM4 As New XFDynamicGridColumnDefinition()
			OBLM4.ColumnName = "OBL_M4"
			OBLM4.IsFromTable = True
			OBLM4.IsVisible = True
			OBLM4.AllowUpdates = False
			columnDefinitions.Add(OBLM4)
			
			Dim OBLM5 As New XFDynamicGridColumnDefinition()
			OBLM5.ColumnName = "OBL_M5"
			OBLM5.IsFromTable = True
			OBLM5.IsVisible = True
			OBLM5.AllowUpdates = False
			columnDefinitions.Add(OBLM5)
			
			Dim OBLM6 As New XFDynamicGridColumnDefinition()
			OBLM6.ColumnName = "OBL_M6"
			OBLM6.IsFromTable = True
			OBLM6.IsVisible = True
			OBLM6.AllowUpdates = False
			columnDefinitions.Add(OBLM6)
			
			Dim OBLM7 As New XFDynamicGridColumnDefinition()
			OBLM7.ColumnName = "OBL_M7"
			OBLM7.IsFromTable = True
			OBLM7.IsVisible = True
			OBLM7.AllowUpdates = False
			columnDefinitions.Add(OBLM7)
			
			Dim OBLM8 As New XFDynamicGridColumnDefinition()
			OBLM8.ColumnName = "OBL_M8"
			OBLM8.IsFromTable = True
			OBLM8.IsVisible = True
			OBLM8.AllowUpdates = False
			columnDefinitions.Add(OBLM8)
			
			Dim OBLM9 As New XFDynamicGridColumnDefinition()
			OBLM9.ColumnName = "OBL_M9"
			OBLM9.IsFromTable = True
			OBLM9.IsVisible = True
			OBLM9.AllowUpdates = False
			columnDefinitions.Add(OBLM9)
			
			Dim OBLM10 As New XFDynamicGridColumnDefinition()
			OBLM10.ColumnName = "OBL_M10"
			OBLM10.IsFromTable = True
			OBLM10.IsVisible = True
			OBLM10.AllowUpdates = False
			columnDefinitions.Add(OBLM10)
			
			Dim OBLM11 As New XFDynamicGridColumnDefinition()
			OBLM11.ColumnName = "OBL_M11"
			OBLM11.IsFromTable = True
			OBLM11.IsVisible = True
			OBLM11.AllowUpdates = False
			columnDefinitions.Add(OBLM11)
			
			Dim OBLM12 As New XFDynamicGridColumnDefinition()
			OBLM12.ColumnName = "OBL_M12"
			OBLM12.IsFromTable = True
			OBLM12.IsVisible = True
			OBLM12.AllowUpdates = False
			columnDefinitions.Add(OBLM12)
			
			Dim OBLCarryOver As New XFDynamicGridColumnDefinition()
			OBLCarryOver.ColumnName = "OBL_Carryover"
			OBLCarryOver.IsFromTable = True
			OBLCarryOver.IsVisible = True
			OBLCarryOver.AllowUpdates = False
			columnDefinitions.Add(OBLCarryOver)
			
	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Status"
			columnDefinitions.Add(Status)
						
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars("BL_CMD_SPLN_FundsCenter")
					
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_SPLNDataSet.Main(si, globals, api, dargs)

						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function
#End Region

#Region "PGM Cert Details"
Private Function dg_CMD_SPLN_REQCertDetail() As Object
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
								FROM XFC_CMD_SPLN_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_SPLN_REQ AS Req
									ON Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
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
			
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function		

#End Region

#Region "Manpower"
Private Function dg_CMD_SPLN_REQManpower() As Object
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
								FROM XFC_CMD_SPLN_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_SPLN_REQ AS Req
								ON Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
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

						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function		
#End Region
		
#Region "Review Amounts"
	Private Function dg_CMD_SPLN_REQAmtsReview() As Object
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
			APE.ColumnName = "APE"
			APE.IsFromTable = True
			APE.IsVisible = True
			APE.AllowUpdates = False
			APE.Description = "APE"
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

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
    
			
	
							
						' Get the data you want To put In the grid
						
						Dim sEntity As String  = args.CustomSubstVars("BL_CMD_SPLN_FundsCenter")
						
						Dim objdt As New DataTable 
						Dim dargs As New DashboardDataSetArgs
						dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
						dargs.DataSetName = "REQListByEntityAndStatus"
						dargs.NameValuePairs.XFSetValue("Entity", sEntity)
'Brapi.ErrorLog.LogMessage(si,"Entity" & sEntity)
						objdt = BR_CMD_SPLNDataSet.Main(si, globals, api, dargs)
						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function	
		
#End Region

		
#Region "Revise requirements"
	Private Function dg_CMD_SPLN_ReviseSPLNRequirements() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	 Dim dt As New DataTable()
	 Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
	 Dim EditValue As String = args.CustomSubstVars.XFGetValue("DL_CMD_SPLN_AdjustFundingLine")
	  
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

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
			
			Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = True
			Account.Description = "Account"
			Account.AllowUpdates = False
			
			columnDefinitions.Add(Account)
    
    Dim Month1 As New XFDynamicGridColumnDefinition()
			Month1.ColumnName = "Month1"
			Month1.IsFromTable = True
			Month1.IsVisible = True
			Month1.Description = "October"
			Month1.DataFormatString = "N0"
			columnDefinitions.Add(Month1)
			
    Dim Month2 As New XFDynamicGridColumnDefinition()
			Month2.ColumnName = "Month2"
			Month2.IsFromTable = True
			Month2.IsVisible = True
			Month2.Description = "November"
			Month2.DataFormatString = "N0"
			columnDefinitions.Add(Month2)
			
   Dim Month3 As New XFDynamicGridColumnDefinition()
			Month3.ColumnName = "Month3"
			Month3.IsFromTable = True
			Month3.IsVisible = True
			Month3.Description = "December"
			Month3.DataFormatString = "N0"
			columnDefinitions.Add(Month3)
			
    Dim Month4 As New XFDynamicGridColumnDefinition()
			Month4.ColumnName = "Month4"
			Month4.IsFromTable = True
			Month4.IsVisible = True
			Month4.Description = "January"
			Month4.DataFormatString = "N0"
			columnDefinitions.Add(Month4)
			
    Dim Month5 As New XFDynamicGridColumnDefinition()
			Month5.ColumnName = "Month5"
			Month5.IsFromTable = True
			Month5.IsVisible = True
			Month5.Description = "Febuary"
			Month5.DataFormatString = "N0"
			columnDefinitions.Add(Month5)
			
	Dim Month6 As New XFDynamicGridColumnDefinition()
			Month6.ColumnName = "Month6"
			Month6.IsFromTable = True
			Month6.IsVisible = True
			Month6.Description = "March"
			Month6.DataFormatString = "N0"
			columnDefinitions.Add(Month6)
    Dim Month7 As New XFDynamicGridColumnDefinition()
			Month7.ColumnName = "Month7"
			Month7.IsFromTable = True
			Month7.IsVisible = True
			Month7.Description = "April"
			Month7.DataFormatString = "N0"
			columnDefinitions.Add(Month7)
			
   Dim Month8 As New XFDynamicGridColumnDefinition()
			Month8.ColumnName = "Month8"
			Month8.IsFromTable = True
			Month8.IsVisible = True
			Month8.Description = "May"
			Month8.DataFormatString = "N0"
			columnDefinitions.Add(Month8)
			
    Dim Month9 As New XFDynamicGridColumnDefinition()
			Month9.ColumnName = "Month9"
			Month9.IsFromTable = True
			Month9.IsVisible = True
			Month9.Description = "June"
			Month9.DataFormatString = "N0"
			columnDefinitions.Add(Month9)
			
    Dim Month10 As New XFDynamicGridColumnDefinition()
			Month10.ColumnName = "Month10"
			Month10.IsFromTable = True
			Month10.IsVisible = True
			Month10.Description = "July"
			Month10.DataFormatString = "N0"
			columnDefinitions.Add(Month10)	
	Dim Month11 As New XFDynamicGridColumnDefinition()
			Month11.ColumnName = "Month11"
			Month11.IsFromTable = True
			Month11.IsVisible = True
			Month11.Description = "August"
			Month11.DataFormatString = "N0"
			columnDefinitions.Add(Month11)
	Dim Month12 As New XFDynamicGridColumnDefinition()
			Month12.ColumnName = "Month12"
			Month12.IsFromTable = True
			Month12.IsVisible = True
			Month12.Description = "September"
			Month12.DataFormatString = "N0"
			columnDefinitions.Add(Month12)
			
		Dim Carryover As New XFDynamicGridColumnDefinition()
			Carryover.ColumnName = "Yearly"
			Carryover.IsFromTable = True
			Carryover.IsVisible = True
			Carryover.Description = "Carryover"
			Carryover.DataFormatString = "N0"
			columnDefinitions.Add(Carryover)

	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = False
			Status.Description = "Status"
			columnDefinitions.Add(Status)
	Dim REQ_ID_Type As New XFDynamicGridColumnDefinition()
			REQ_ID_Type.ColumnName = "REQ_ID_Type"
			REQ_ID_Type.IsFromTable = True
			REQ_ID_Type.IsVisible = False
			REQ_ID_Type.Description = "REQ_ID_Type"
			columnDefinitions.Add(REQ_ID_Type)
							
			'Get the data you want To put In the grid
			Dim Entity As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_Entity")
			Dim U1 As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_UD1")
			Dim U1Filter As String = String.Empty
			Dim U2 As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_UD2")
			Dim U2Filter As String = String.Empty
			Dim U3 As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_UD3")
			Dim U3Filter As String = String.Empty
			Dim U4 As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_UD4")
			Dim U4Filter As String = String.Empty
			Dim U6 As String  =  args.CustomSubstVars.XFGetValue("LR_CMD_SPLN_UD6")
			Dim U6Filter As String = String.Empty
			
			If Not (String.IsNullOrEmpty(U1) Or U1.XFContainsIgnoreCase("Top")) Then U1Filter = $"AND dtl.UD1  = '{U1}'"			
			If Not (String.IsNullOrEmpty(U2) Or U2.XFContainsIgnoreCase("Top")) Then U2Filter = $"AND dtl.UD2  = '{U2}'"		
			If Not (String.IsNullOrEmpty(U3) Or U3.XFContainsIgnoreCase("Top")) Then U3Filter = $"AND dtl.UD3  = '{U3}'"		
			If Not (String.IsNullOrEmpty(U4) Or U4.XFContainsIgnoreCase("Top")) Then U4Filter = $"AND dtl.UD4  = '{U4}'"
'brapi.ErrorLog.LogMessage(si,"U6 " & U6)
			If U6.XFContainsIgnoreCase("Pay_Benefits") Then U6Filter = $"AND req.req_id_type  = 'CivPay'"
			If U6.XFContainsIgnoreCase("Non_Pay") Then U6Filter = $"AND req.req_id_type  = 'Requirement'"
			If U6.XFContainsIgnoreCase("Top") Then U6Filter = ""
			
			brapi.ErrorLog.LogMessage(si,"U1: " & U1Filter & vbCrLf & "U2: " & U2Filter  & vbCrLf & "U3: " & U3Filter  & vbCrLf & "U4: " & U4Filter & vbCrLf & "U6: " & U6Filter )
'brapi.ErrorLog.LogMessage(si,"Entity from LR = " & Entity)
			If String.IsNullOrWhiteSpace(Entity)
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
			Dim nextyear As Integer = wfInfoDetails("TimeName") + 1
			
            Dim sql As String = $"SELECT Req.CMD_SPLN_REQ_ID,
								  req.req_id as REQ_ID,
									req.req_id_type,
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
								  Dtl.Account,
								dtl.Flow,
								Dtl.Month1 As Month1,
						         Dtl.Month2 AS Month2,
						         Dtl.Month3 AS Month3,
						         Dtl.Month4 AS Month4,
						         Dtl.Month5 AS Month5,
								 Dtl.Month6 AS Month6,
						         Dtl.Month7 AS Month7,
						         Dtl.Month8 AS Month8,
						         Dtl.Month9 AS Month9,
					 			 Dtl.Month10 AS Month10,
						         Dtl.Month11 AS Month11,
						         Dtl.Month12 AS Month12,
				               	dtl1.Yearly
							
					
					
								FROM XFC_CMD_SPLN_REQ AS Req
								LEFT JOIN XFC_CMD_SPLN_REQ_Details AS Dtl
								ON Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								Left Outer Join XFC_CMD_SPLN_REQ_Details as dtl1 ON  
				          	 	Req.CMD_SPLN_REQ_ID = Dtl1.CMD_SPLN_REQ_ID
				                AND req.WFScenario_Name = Dtl1.WFScenario_Name
				                AND Req.WFCMD_Name = Dtl1.WFCMD_Name
				                AND Req.WFTime_Name = Dtl1.WFTime_Name
				              	 AND dtl1.fiscal_year = '{nextyear}'
				                AND dtl1.account = dtl.account
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								and dtl.fiscal_year = '{wfInfoDetails("TimeName")}'
								And Req.Entity  = '{Entity}'
								{U1Filter}
								{U2Filter}
								{U3Filter}
								{U4Filter}
								{U6Filter}
								AND  Dtl.Account in ('Commitments','Obligations')"
'brapi.ErrorLog.LogMessage(si,"SQL = " & sql)			
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


#Region"Rollover"
		Public Function dg_CMD_SPLN_Rollover_REQ() As Object
			Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 
			
			Dim CMD_SPLN_REQ_ID As New XFDynamicGridColumnDefinition()
					CMD_SPLN_REQ_ID.ColumnName = "CMD_SPLN_REQ_ID"
					CMD_SPLN_REQ_ID.IsFromTable = True
					CMD_SPLN_REQ_ID.IsVisible = False
					columnDefinitions.Add(CMD_SPLN_REQ_ID)
								
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
			
			Dim sEntity As String  = args.CustomSubstVars("BL_CMD_SPLN_FundsCenter")
			Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim entityDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & WFCube)
			Dim baseMbrs As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si,entityDimPk,"E#" & sEntity.Replace("_General","") & ".Base",True)
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
			objdt = BR_CMD_SPLNDataSet.Main(si, globals, api, dargs)
	  				
		     'Create the XFTable
		    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
		
		     'Send the result To the Interface component
		    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult		
		End Function
#End Region	

#Region "REQ Funding Line"
Private Function dg_CMD_SPLN_REQFundingLine() As Object
	Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	 Dim dt As New DataTable()
	 Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
	 Dim EditValue As String = args.CustomSubstVars.XFGetValue("DL_CMD_SPLN_AdjustFundingLine")
	  
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

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			
			columnDefinitions.Add(CMD_SPLN_ID)
			
			Dim Account As New XFDynamicGridColumnDefinition()
			Account.ColumnName = "Account"
			Account.IsFromTable = True
			Account.IsVisible = True
			Account.Description = "Account"
			Account.AllowUpdates = False
			
			columnDefinitions.Add(Account)
    
    Dim Month1 As New XFDynamicGridColumnDefinition()
			Month1.ColumnName = "Month1"
			Month1.IsFromTable = True
			Month1.IsVisible = True
			Month1.Description = "October"
			Month1.DataFormatString = "N0"
			columnDefinitions.Add(Month1)
			
    Dim Month2 As New XFDynamicGridColumnDefinition()
			Month2.ColumnName = "Month2"
			Month2.IsFromTable = True
			Month2.IsVisible = True
			Month2.Description = "November"
			Month2.DataFormatString = "N0"
			columnDefinitions.Add(Month2)
			
   Dim Month3 As New XFDynamicGridColumnDefinition()
			Month3.ColumnName = "Month3"
			Month3.IsFromTable = True
			Month3.IsVisible = True
			Month3.Description = "December"
			Month3.DataFormatString = "N0"
			columnDefinitions.Add(Month3)
			
    Dim Month4 As New XFDynamicGridColumnDefinition()
			Month4.ColumnName = "Month4"
			Month4.IsFromTable = True
			Month4.IsVisible = True
			Month4.Description = "January"
			Month4.DataFormatString = "N0"
			columnDefinitions.Add(Month4)
			
    Dim Month5 As New XFDynamicGridColumnDefinition()
			Month5.ColumnName = "Month5"
			Month5.IsFromTable = True
			Month5.IsVisible = True
			Month5.Description = "Febuary"
			Month5.DataFormatString = "N0"
			columnDefinitions.Add(Month5)
			
	Dim Month6 As New XFDynamicGridColumnDefinition()
			Month6.ColumnName = "Month6"
			Month6.IsFromTable = True
			Month6.IsVisible = True
			Month6.Description = "March"
			Month6.DataFormatString = "N0"
			columnDefinitions.Add(Month6)
    Dim Month7 As New XFDynamicGridColumnDefinition()
			Month7.ColumnName = "Month7"
			Month7.IsFromTable = True
			Month7.IsVisible = True
			Month7.Description = "April"
			Month7.DataFormatString = "N0"
			columnDefinitions.Add(Month7)
			
   Dim Month8 As New XFDynamicGridColumnDefinition()
			Month8.ColumnName = "Month8"
			Month8.IsFromTable = True
			Month8.IsVisible = True
			Month8.Description = "May"
			Month8.DataFormatString = "N0"
			columnDefinitions.Add(Month8)
			
    Dim Month9 As New XFDynamicGridColumnDefinition()
			Month9.ColumnName = "Month9"
			Month9.IsFromTable = True
			Month9.IsVisible = True
			Month9.Description = "June"
			Month9.DataFormatString = "N0"
			columnDefinitions.Add(Month9)
			
    Dim Month10 As New XFDynamicGridColumnDefinition()
			Month10.ColumnName = "Month10"
			Month10.IsFromTable = True
			Month10.IsVisible = True
			Month10.Description = "July"
			Month10.DataFormatString = "N0"
			columnDefinitions.Add(Month10)	
	Dim Month11 As New XFDynamicGridColumnDefinition()
			Month11.ColumnName = "Month11"
			Month11.IsFromTable = True
			Month11.IsVisible = True
			Month11.Description = "August"
			Month11.DataFormatString = "N0"
			columnDefinitions.Add(Month11)
	Dim Month12 As New XFDynamicGridColumnDefinition()
			Month12.ColumnName = "Month12"
			Month12.IsFromTable = True
			Month12.IsVisible = True
			Month12.Description = "September"
			Month12.DataFormatString = "N0"
			columnDefinitions.Add(Month12)
			
		Dim Carryover As New XFDynamicGridColumnDefinition()
			Carryover.ColumnName = "Yearly"
			Carryover.IsFromTable = True
			Carryover.IsVisible = True
			Carryover.Description = "Carryover"
			Carryover.DataFormatString = "N0"
			columnDefinitions.Add(Carryover)

	 Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = False
			Status.Description = "Status"
			columnDefinitions.Add(Status)
							
						' Get the data you want To put In the grid
						
		Dim ReqID As String = args.CustomSubstVars.XFGetValue("BL_CMD_SPLN_REQTitleList")
				If String.IsNullOrWhiteSpace(ReqID)
					Return Nothing
				Else 
			Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(ReqID, " ")
			Dim Entity As String  =  REQ_ID_Split(0)
			Dim RequirementID As String  = REQ_ID_Split(1)
			
			Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			Dim nextyear As Integer = wfInfoDetails("TimeName") + 1
			
            Dim sql As String = $"SELECT Req.CMD_SPLN_REQ_ID,
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
								  Dtl.Account,
								Dtl.Month1 As Month1,
						         Dtl.Month2 AS Month2,
						         Dtl.Month3 AS Month3,
						         Dtl.Month4 AS Month4,
						         Dtl.Month5 AS Month5,
								 Dtl.Month6 AS Month6,
						         Dtl.Month7 AS Month7,
						         Dtl.Month8 AS Month8,
						         Dtl.Month9 AS Month9,
					 			 Dtl.Month10 AS Month10,
						         Dtl.Month11 AS Month11,
						         Dtl.Month12 AS Month12,
				               		dtl1.Yearly
					
					
								FROM XFC_CMD_SPLN_REQ AS Req
								LEFT JOIN XFC_CMD_SPLN_REQ_Details AS Dtl
								ON Req.CMD_SPLN_REQ_ID = Dtl.CMD_SPLN_REQ_ID
								AND Req.WFScenario_Name = Dtl.WFScenario_Name
								AND Req.WFCMD_Name = Dtl.WFCMD_Name
								AND Req.WFTime_Name = Dtl.WFTime_Name
								AND Req.Entity = Dtl.Entity
								Left Outer Join XFC_CMD_SPLN_REQ_Details as dtl1 ON  
				          	 	Req.CMD_SPLN_REQ_ID = Dtl1.CMD_SPLN_REQ_ID
				                AND req.WFScenario_Name = Dtl1.WFScenario_Name
				                AND Req.WFCMD_Name = Dtl1.WFCMD_Name
				                AND Req.WFTime_Name = Dtl1.WFTime_Name
				              	 AND dtl1.fiscal_year = '{nextyear}'
				                AND dtl1.account = dtl.account
								WHERE Req.WFScenario_Name = '{wfInfoDetails("ScenarioName")}'
								AND Req.WFCMD_Name = '{wfInfoDetails("CMDName")}'
								AND Req.WFTime_Name = '{wfInfoDetails("TimeName")}'
								and dtl.fiscal_year = '{wfInfoDetails("TimeName")}'
								And Req.REQ_ID = '{RequirementID}'
								And Req.Entity  = '{Entity}'
								AND  Dtl.Account in ('Commitments','Obligations')"
'brapi.ErrorLog.LogMessage(si,"SQL = " & sql)			
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


#Region"PackageSubmission"
		Public Function dg_CMD_SPLN_PackageSubmission() As Object
			Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 
		
			Dim PovCubeName As New XFDynamicGridColumnDefinition()
					PovCubeName.ColumnName = "PovCubeName"
					PovCubeName.IsFromTable = True
					PovCubeName.IsVisible = False
					columnDefinitions.Add(PovCubeName)
			Dim Pov00EntityName As New XFDynamicGridColumnDefinition()
					Pov00EntityName.ColumnName = "Pov00EntityName"
					Pov00EntityName.IsFromTable = True
					Pov00EntityName.IsVisible = False
					columnDefinitions.Add(Pov00EntityName)
			Dim Pov01ConsolidationName As New XFDynamicGridColumnDefinition()
					Pov01ConsolidationName.ColumnName = "Pov01ConsolidationName"
					Pov01ConsolidationName.IsFromTable = True
					Pov01ConsolidationName.IsVisible = False
					columnDefinitions.Add(Pov01ConsolidationName)
			Dim Pov02ScenarioName As New XFDynamicGridColumnDefinition()
					Pov02ScenarioName.ColumnName = "Pov02ScenarioName"
					Pov02ScenarioName.IsFromTable = True
					Pov02ScenarioName.IsVisible = False
					columnDefinitions.Add(Pov02ScenarioName)
			Dim Pov03TimeName As New XFDynamicGridColumnDefinition()
					Pov03TimeName.ColumnName = "Pov03TimeName"
					Pov03TimeName.IsFromTable = True
					Pov03TimeName.IsVisible = False
					columnDefinitions.Add(Pov03TimeName)
			Dim Pov04ViewName As New XFDynamicGridColumnDefinition()
					Pov04ViewName.ColumnName = "Pov04ViewName"
					Pov04ViewName.IsFromTable = True
					Pov04ViewName.IsVisible = False
					columnDefinitions.Add(Pov04ViewName)
					
			Dim Pov05AccountName As New XFDynamicGridColumnDefinition()
					Pov05AccountName.ColumnName = "Pov05AccountName"
					Pov05AccountName.IsFromTable = True
					Pov05AccountName.IsVisible = False
					columnDefinitions.Add(Pov05AccountName)
			Dim Pov06FlowName As New XFDynamicGridColumnDefinition()
					Pov06FlowName.ColumnName = "Pov06FlowName"
					Pov06FlowName.IsFromTable = True
					Pov06FlowName.IsVisible = False
					columnDefinitions.Add(Pov06FlowName)
			Dim Pov07OriginName As New XFDynamicGridColumnDefinition()
					Pov07OriginName.ColumnName = "Pov07OriginName"
					Pov07OriginName.IsFromTable = True
					Pov07OriginName.IsVisible = False
					columnDefinitions.Add(Pov07OriginName)
			Dim Pov08ICName As New XFDynamicGridColumnDefinition()
					Pov08ICName.ColumnName = "Pov08ICName"
					Pov08ICName.IsFromTable = True
					Pov08ICName.IsVisible = False
					columnDefinitions.Add(Pov08ICName)
			Dim Pov09UD1Name As New XFDynamicGridColumnDefinition()
					Pov09UD1Name.ColumnName = "Pov09UD1Name"
					Pov09UD1Name.IsFromTable = True
					Pov09UD1Name.IsVisible = False
					columnDefinitions.Add(Pov09UD1Name)
					
			Dim Pov10UD2Name As New XFDynamicGridColumnDefinition()
					Pov10UD2Name.ColumnName = "Pov10UD2Name"
					Pov10UD2Name.IsFromTable = True
					Pov10UD2Name.IsVisible = False
					columnDefinitions.Add(Pov10UD2Name)
			Dim Pov11UD3Name As New XFDynamicGridColumnDefinition()
					Pov11UD3Name.ColumnName = "Pov11UD3Name"
					Pov11UD3Name.IsFromTable = True
					Pov11UD3Name.IsVisible = False
					columnDefinitions.Add(Pov11UD3Name)
			Dim Pov12UD4Name As New XFDynamicGridColumnDefinition()
					Pov12UD4Name.ColumnName = "Pov12UD4Name"
					Pov12UD4Name.IsFromTable = True
					Pov12UD4Name.IsVisible = False
					columnDefinitions.Add(Pov12UD4Name)
			Dim Pov13UD5Name As New XFDynamicGridColumnDefinition()
					Pov13UD5Name.ColumnName = "Pov13UD5Name"
					Pov13UD5Name.IsFromTable = True
					Pov13UD5Name.IsVisible = False
					columnDefinitions.Add(Pov13UD5Name)
			Dim Pov14UD6Name As New XFDynamicGridColumnDefinition()
					Pov14UD6Name.ColumnName = "Pov14UD6Name"
					Pov14UD6Name.IsFromTable = True
					Pov14UD6Name.IsVisible = False
					columnDefinitions.Add(Pov14UD6Name)
					
			Dim Pov15UD7Name As New XFDynamicGridColumnDefinition()
					Pov15UD7Name.ColumnName = "Pov15UD7Name"
					Pov15UD7Name.IsFromTable = True
					Pov15UD7Name.IsVisible = False
					columnDefinitions.Add(Pov15UD7Name)
			Dim Pov16UD8Name As New XFDynamicGridColumnDefinition()
					Pov16UD8Name.ColumnName = "Pov16UD8Name"
					Pov16UD8Name.IsFromTable = True
					Pov16UD8Name.IsVisible = False
					columnDefinitions.Add(Pov16UD8Name)
			
			
			Dim Entity As New XFDynamicGridColumnDefinition()
					Entity.ColumnName = "RowHdr0_Entity"
					Entity.IsFromTable = True
					Entity.IsVisible = True
					Entity.description = "Funds Center"
					columnDefinitions.Add(Entity)
								
			Dim RowHdr0ParentName As New XFDynamicGridColumnDefinition()
					RowHdr0ParentName.ColumnName = "RowHdr0ParentName"
					RowHdr0ParentName.IsFromTable = True
					RowHdr0ParentName.IsVisible = False
					columnDefinitions.Add(RowHdr0ParentName)
			
'			Dim APPN As New XFDynamicGridColumnDefinition()
'					APPN.ColumnName = "RowHdr1_UD1"
'					APPN.IsFromTable = True
'					APPN.IsVisible = True
'					APPN.Description = "APPN"
'					columnDefinitions.Add(APPN)
					
			Dim Flow As New XFDynamicGridColumnDefinition()
					Flow.ColumnName = "RowHdr1_Flow"
					Flow.IsFromTable = True
					Flow.IsVisible = True
					Flow.Description = "Flow"
					columnDefinitions.Add(Flow)
					
			Dim APPN As New XFDynamicGridColumnDefinition()
					APPN.ColumnName = "RowHdr2_UD1"
					APPN.IsFromTable = True
					APPN.IsVisible = True
					APPN.Description = "APPN"
					columnDefinitions.Add(APPN)
					
		   	Dim DODRATE As New XFDynamicGridColumnDefinition()
					DODRATE.ColumnName = "ColVal0_JulyDODRate"
					DODRATE.IsFromTable = True
					DODRATE.IsVisible = False
					DODRATE.Description = "DOD_Rate"
					DODRATE.DataFormatString = "N0"
					columnDefinitions.Add(DODRATE)
					
			Dim Total As New XFDynamicGridColumnDefinition()
					Total.ColumnName = "ColVal1_Total"
					Total.IsFromTable = True
					Total.IsVisible = True
					Total.Description = "Targets"
					total.DataFormatString = "N0"
					columnDefinitions.Add(Total)					
					
			Dim Commitments As New XFDynamicGridColumnDefinition()
					Commitments.ColumnName = "ColVal2_TotalCommitments"
					Commitments.IsFromTable = True
					Commitments.IsVisible = True
					Commitments.Description = "Commitments"
					commitments.DataFormatString = "N0"
					columnDefinitions.Add(Commitments)

			Dim CommitVariance As New XFDynamicGridColumnDefinition()
					CommitVariance.ColumnName = $"ColVal3_CommitmentVar{WfYear}"
					CommitVariance.IsFromTable = True
					CommitVariance.IsVisible = True
					CommitVariance.Description = "Commitment_Variance"
					CommitVariance.DataFormatString = "N0"
					columnDefinitions.Add(CommitVariance)
			Dim Obligations As New XFDynamicGridColumnDefinition()
					Obligations.ColumnName = "ColVal4_TotalObligations"
					Obligations.IsFromTable = True
					Obligations.IsVisible = True
					Obligations.Description = "Obligations"
					Obligations.DataFormatString = "N0"
					columnDefinitions.Add(Obligations)
			Dim ObligVariance As New XFDynamicGridColumnDefinition()
					ObligVariance.ColumnName = $"ColVal5_ObligationVar{WfYear}"
					ObligVariance.IsFromTable = True
					ObligVariance.IsVisible = True
					ObligVariance.Description = "Obligation_Variance"
					ObligVariance.DataFormatString = "N0"
					columnDefinitions.Add(ObligVariance)
			Dim Entity_APPN As New XFDynamicGridColumnDefinition()
					Entity_APPN.ColumnName = "Entity_APPN"
					Entity_APPN.IsFromTable = True
					Entity_APPN.IsVisible = False
					Entity_APPN.Description = "Entity_APPN"
					columnDefinitions.Add(Entity_APPN)
			
	
			Dim dt As New DataTable()
			dt.TableName = "PackageForSubmission"
			Dim scenFilter As String = String.empty
			Dim cvname As String = "CMD_SPLN_Package_Summary_APPN_FDX"
			Dim wsName As String = "50 CMD SPLN"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim timeFilter = "T#" & WfYear '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)
			Dim DC_Entity_APPN As New DataColumn("Entity_APPN",GetType(String))
			dt = BRApi.Import.Data.FdxExecuteCubeView(si, wsID, cvName, String.Empty, String.Empty,String.Empty,String.Empty, timeFilter, nvbParams, False, False, String.Empty, 8, False)
			
			If dt Is Nothing Then
				Return Nothing
			End If 
			
			DC_Entity_APPN.Expression = "ISNULL(RowHdr0_Entity, '') + ':' + ISNULL(RowHdr2_UD1, '')"

			dt.Columns.Add(DC_Entity_APPN)

			For i As Integer = dt.Rows.Count -1 To 0 Step -1
				Dim rEntity_APPN As String = dt.Rows(i).Item("Entity_APPN")
				Dim dargs As New DashboardStringFunctionArgs
				dargs.FunctionName = "PackageValidations"
				dargs.NameValuePairs.XFSetValue("Entity_APPN", rEntity_APPN)
				Dim IsValid As Boolean = BR_CMD_SPLN_String_Helper.Main(si, globals, api, dargs)
				
				If IsValid = False Then 
					dt.Rows.Remove(dt.Rows(i))
				End If
			Next
			
			
			
		     'Create the XFTable
		    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
		
		     'Send the result To the Interface component
		    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		       
			
			
		    Return taskResult		
		End Function
#End Region	


#Region "Save REQ Funding Line "		
Public Function Save_dg_CMD_SPLN_REQFundingLine() As Object
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
			Dim WFYear As Integer = wfInfoDetails("TimeName")
			Dim nextyear As Integer = wfInfoDetails("TimeName") + 1
'Brapi.ErrorLog.LogMessage(si, "Hit 1")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_SPLN_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_SPLN_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_SPLN_REQ_ID  = @CMD_SPLN_REQ_ID"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
				New SqlParameter("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier) With {.Value = editedDataRows.Item(0).ModifiedDataRow("CMD_SPLN_REQ_ID")}
						}
              sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
'Brapi.ErrorLog.LogMessage(si, "HERE 4")	

            ' --- Details Table (XFC_CMD_SPLN_REQ_Details) ---
            Dim dt_Details As New DataTable()
			 Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_SPLN_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_SPLN_REQ_ID  = @CMD_SPLN_REQ_ID"
            sqaReaderdetail.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

            ' ************************************
            ' ************************************
          		
	 		For Each editedDataRow As XFEditedDataRow In editedDataRows
					Dim EditAccount As String = editedDataRow.ModifiedDataRow.Item("Account")
'brapi.ErrorLog.LogMessage(si,"Edit ACcount = " & EditAccount)
				    Dim targetRow As DataRow 											
					Dim req_ID_Val As Guid
					req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_SPLN_REQ_ID")
					
					targetRow = dt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()

					targetRow("Update_Date") = DateTime.Now
					targetRow("Update_User") = si.UserName																																																																																				

					Dim targetRowFundingCarryover As DataRow

					targetRowFundingCarryover = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = '{EditAccount}' and Fiscal_Year = '{nextyear}'").FirstOrDefault()		
					If targetRowFundingCarryover IsNot Nothing Then
						
						Dim Yearly As Decimal =  editedDataRow.ModifiedDataRow.Item("Yearly")	
'Brapi.ErrorLog.LogMessage(si,"Carryover" & Yearly)
						targetRowFundingCarryover("Yearly") = Yearly
						targetRowFundingCarryover("Update_Date") = DateTime.Now
                   		targetRowFundingCarryover("Update_User") = si.UserName  		
					End If 
					
				Dim targetRowFunding As DataRow
					targetRowFunding = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = '{EditAccount}' and Fiscal_Year = '{WFYear}'").FirstOrDefault()
		
					If targetRowFunding IsNot Nothing Then
			
						Dim Month1 As Decimal = editedDataRow.ModifiedDataRow.Item("Month1")		 
						Dim Month2 As Decimal = editedDataRow.ModifiedDataRow.Item("Month2")
						Dim Month3 As Decimal = editedDataRow.ModifiedDataRow.Item("Month3")
					    Dim Month4 As Decimal = editedDataRow.ModifiedDataRow.Item("Month4")
					 	Dim Month5 As Decimal = editedDataRow.ModifiedDataRow.Item("Month5")
						Dim Month6 As Decimal = editedDataRow.ModifiedDataRow.Item("Month6")		 
						Dim Month7 As Decimal = editedDataRow.ModifiedDataRow.Item("Month7")
						Dim Month8 As Decimal = editedDataRow.ModifiedDataRow.Item("Month8")
					    Dim Month9 As Decimal = editedDataRow.ModifiedDataRow.Item("Month9")
					 	Dim Month10 As Decimal = editedDataRow.ModifiedDataRow.Item("Month10")
						Dim Month11 As Decimal = editedDataRow.ModifiedDataRow.Item("Month11")
					 	Dim Month12 As Decimal = editedDataRow.ModifiedDataRow.Item("Month12")	
						
						targetRowFunding("Month1") = Month1
						targetRowFunding("Month2") = Month2
						targetRowFunding("Month3") = Month3
						targetRowFunding("Month4") = Month4
						targetRowFunding("Month5") = Month5
						targetRowFunding("Month6") = Month6
						targetRowFunding("Month7") = Month7
						targetRowFunding("Month8") = Month8
						targetRowFunding("Month9") = Month9
						targetRowFunding("Month10") = Month10
						targetRowFunding("Month11") = Month11
						targetRowFunding("Month12") = Month12
					
						
						targetRowFunding("Update_Date") = DateTime.Now
	                    targetRowFunding("Update_User") = si.UserName 
					
					End If 
				Next	   
		                
	
		                ' Persist changes back to the DB using the configured adapter
		               
		               sqaReaderdetail.Update_XFC_CMD_SPLN_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
						
						
		                End Using
		            End Using

            Return Nothing
		
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

#Region "Create New REQ"
	Private Function dg_CMD_SPLN_Create() As Object
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
		Dim dt As New DataTable()
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
		Dim createrow As DataRow = dt.NewRow()
		dt.Columns.Add("Title")
		dt.Columns.Add("Month1")
		dt.Columns.Add("Month2")
		dt.Columns.Add("Month3")
		dt.Columns.Add("Month4")
		dt.Columns.Add("Month5")
		dt.Columns.Add("Month6")
		dt.Columns.Add("Month7")
		dt.Columns.Add("Month8")
		dt.Columns.Add("Month9")
		dt.Columns.Add("Month10")
		dt.Columns.Add("Month11")
		dt.Columns.Add("Month12")
		
				
		Dim Title As New XFDynamicGridColumnDefinition()
				Title.ColumnName = "Title"
				Title.IsFromTable = True
				Title.IsVisible = True
				Title.AllowUpdates = True
				Title.Width = "600"
				columnDefinitions.Add(Title)
				
		Dim Month1 As New XFDynamicGridColumnDefinition()
				Month1.ColumnName = "Month1"
				Month1.IsFromTable = True
				Month1.IsVisible = True
				Month1.Description = "October"
				Month1.DataFormatString = "N0"
				columnDefinitions.Add(Month1)
				
		Dim Month2 As New XFDynamicGridColumnDefinition()
				Month2.ColumnName = "Month2"
				Month2.IsFromTable = True
				Month2.IsVisible = True
				Month2.Description = "November"
				Month2.DataFormatString = "N0"
				columnDefinitions.Add(Month2)
				
		Dim Month3 As New XFDynamicGridColumnDefinition()
				Month3.ColumnName = "Month3"
				Month3.IsFromTable = True
				Month3.IsVisible = True
				Month3.Description = "December"
				Month3.DataFormatString = "N0"
				columnDefinitions.Add(Month3)
				
		Dim Month4 As New XFDynamicGridColumnDefinition()
				Month4.ColumnName = "Month4"
				Month4.IsFromTable = True
				Month4.IsVisible = True
				Month4.Description = "January"
				Month4.DataFormatString = "N0"
				columnDefinitions.Add(Month4)
				
		Dim Month5 As New XFDynamicGridColumnDefinition()
				Month5.ColumnName = "Month5"
				Month5.IsFromTable = True
				Month5.IsVisible = True
				Month5.Description = "February"
				Month5.DataFormatString = "N0"
				columnDefinitions.Add(Month5)
				
		Dim Month6 As New XFDynamicGridColumnDefinition()
				Month6.ColumnName = "Month6"
				Month6.IsFromTable = True
				Month6.IsVisible = True
				Month6.Description = "March"
				Month6.DataFormatString = "N0"
				columnDefinitions.Add(Month6)

		Dim Month7 As New XFDynamicGridColumnDefinition()
				Month7.ColumnName = "Month7"
				Month7.IsFromTable = True
				Month7.IsVisible = True
				Month7.Description = "April"
				Month7.DataFormatString = "N0"
				columnDefinitions.Add(Month7)
				
		Dim Month8 As New XFDynamicGridColumnDefinition()
				Month8.ColumnName = "Month8"
				Month8.IsFromTable = True
				Month8.IsVisible = True
				Month8.Description = "May"
				Month8.DataFormatString = "N0"
				columnDefinitions.Add(Month8)
				
		Dim Month9 As New XFDynamicGridColumnDefinition()
				Month9.ColumnName = "Month9"
				Month9.IsFromTable = True
				Month9.IsVisible = True
				Month9.Description = "June"
				Month9.DataFormatString = "N0"
				columnDefinitions.Add(Month9)
				
		Dim Month10 As New XFDynamicGridColumnDefinition()
				Month10.ColumnName = "Month10"
				Month10.IsFromTable = True
				Month10.IsVisible = True
				Month10.Description = "July"
				Month10.DataFormatString = "N0"
				columnDefinitions.Add(Month10)
				
		Dim Month11 As New XFDynamicGridColumnDefinition()
				Month11.ColumnName = "Month11"
				Month11.IsFromTable = True
				Month11.IsVisible = True
				Month11.Description = "August"
				Month11.DataFormatString = "N0"
				columnDefinitions.Add(Month11)
				
		Dim Month12 As New XFDynamicGridColumnDefinition()
				Month12.ColumnName = "Month12"
				Month12.IsFromTable = True
				Month12.IsVisible = True
				Month12.Description = "September"
				Month12.DataFormatString = "N0"
				columnDefinitions.Add(Month12)
				
				dt.Rows.Add(createrow)
		
		'Create the XFTable
		Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
		
		 'Send the result To the Interface component
		Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		    
		Return taskResult
	End Function
#End Region

#Region "Save Create new req"
Public Function Save_dg_CMD_SPLN_Create() As Object
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
            ' --- Main Request Table (XFC_CMD_SPLN_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_SPLN_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("TimeName")}
						}
              sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
			'Brapi.ErrorLog.LogMessage(si,"In Save Create SQL")

            ' --- Details Table (XFC_CMD_SPLN_REQ_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_SPLN_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

'Brapi.ErrorLog.LogMessage(si,"In Save Create SQL 2")
            ' ************************************
            ' ************************************
						Dim sU1APPNInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateAPPN","")
						Dim sU1FundCodeInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateFundCode","")
						Dim sU2Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateMDEP","")
						Dim sU3Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateAPEPT","")
						Dim sU4Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateDollarType","")
						Dim sU5Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateCType","")
						Dim sU6Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_SPLN_FormulateCommitItem","")

'							Dim sU1APPNInput As String = args.NameValuePairs.XFGetValue("APPN")
'							Dim sU2Input As String = args.NameValuePairs.XFGetValue("MDEP")
'							Dim sU3Input As String = args.NameValuePairs.XFGetValue("APE")
'							Dim sU4Input As String = args.NameValuePairs.XFGetValue("DollarType")
'							Dim sU5Input As String = args.NameValuePairs.XFGetValue("CType")
'							Dim sU6Input As String = args.NameValuePairs.XFGetValue("ObjClass")
							
'Brapi.ErrorLog.LogMessage(si,"Create Comboboxes: " & sU1APPNInput & "," & sU2Input & "," & sU3Input & "," & sU4Input & "," & sU6Input)
            
						Dim sEntity As String =  args.CustomSubstVars.XFGetValue("BL_CMD_SPLN_FundsCenter","")
						Dim entityLevel As String = Me.GetEntityLevel(sEntity)
						Dim sREQWFStatus As String = entityLevel & "_Formulate_SPLN"
						Dim NewReqID As Guid = Guid.NewGuid()
						'BRapi.ErrorLog.LogMessage(si,"Hit")
            		For Each editedDataRow As XFEditedDataRow In editedDataRows						
								
						
							Dim requiredString As String = ""
							If String.IsNullOrWhiteSpace(sU1APPNInput) Then
								requiredString += "Appropriation"
							End If	
							
							If String.IsNullOrWhiteSpace(sU1FundCodeInput) Then
								requiredString += "Fund Code"
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
								requiredString += "Obj_Class"
							End If	
							If Not String.IsNullOrWhiteSpace(requiredString) Then
								Throw New Exception("The following fields must be populated when creating a requirement: " + requiredString + ".")
							End If	
							
							Dim accountList As New List(Of String) From {"Obligations", "Commitments"}

						For Each accountName As String In accountList
						 
                            Dim targetRowDetails As DataRow = dt_Details.NewRow()
                    
							targetRowDetails("CMD_SPLN_REQ_ID") = NewReqID
							targetRowDetails("WFScenario_Name") = wfInfoDetails("ScenarioName")
							targetRowDetails("WFCMD_Name") = wfInfoDetails("CMDName")
							targetRowDetails("WFTime_Name") = wfInfoDetails("TimeName")
							targetRowDetails("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_SPLN_FundsCenter","")
							targetRowDetails("Unit_of_Measure") = "Funding"
							targetRowDetails("IC") = "None"
							targetRowDetails("Account") = accountName
							targetRowDetails("Flow") = sREQWFStatus
'BRApi.ErrorLog.LogMessage(si,"Hit2")
							targetRowDetails("UD1") = sU1FundCodeInput
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
							targetRowDetails("Fiscal_Year") = wfInfoDetails("TimeName")
                            Dim Month1 As Decimal = editedDataRow.ModifiedDataRow.Item("Month1").ToString.XFConvertToDecimal
                            Dim Month2 As Decimal = editedDataRow.ModifiedDataRow.Item("Month2").ToString.XFConvertToDecimal
                            Dim Month3 As Decimal = editedDataRow.ModifiedDataRow.Item("Month3").ToString.XFConvertToDecimal
                            Dim Month4 As Decimal = editedDataRow.ModifiedDataRow.Item("Month4").ToString.XFConvertToDecimal
                            Dim Month5 As Decimal = editedDataRow.ModifiedDataRow.Item("Month5").ToString.XFConvertToDecimal
                            Dim Month6 As Decimal = editedDataRow.ModifiedDataRow.Item("Month6").ToString.XFConvertToDecimal
                            Dim Month7 As Decimal = editedDataRow.ModifiedDataRow.Item("Month7").ToString.XFConvertToDecimal
                            Dim Month8 As Decimal = editedDataRow.ModifiedDataRow.Item("Month8").ToString.XFConvertToDecimal
                            Dim Month9 As Decimal = editedDataRow.ModifiedDataRow.Item("Month9").ToString.XFConvertToDecimal
                            Dim Month10 As Decimal = editedDataRow.ModifiedDataRow.Item("Month10").ToString.XFConvertToDecimal
                            Dim Month11 As Decimal = editedDataRow.ModifiedDataRow.Item("Month11").ToString.XFConvertToDecimal
                            Dim Month12 As Decimal = editedDataRow.ModifiedDataRow.Item("Month12").ToString.XFConvertToDecimal
							
						
'BRApi.ErrorLog.LogMessage(si,"Hit4")
                            targetRowDetails("Month1") = Month1
                            targetRowDetails("Month2") = Month2
                            targetRowDetails("Month3") = Month3
                            targetRowDetails("Month4") = Month4
                            targetRowDetails("Month5") = Month5
                            targetRowDetails("Month6") = Month6
                            targetRowDetails("Month7") = Month7
                            targetRowDetails("Month8") = Month8
                            targetRowDetails("Month9") = Month9
                            targetRowDetails("Month10") = Month10
                            targetRowDetails("Month11") = Month11
                            targetRowDetails("Month12") = Month12
							targetRowDetails("Fiscal_Year") = WFInfoDetails("TimeName")
							targetRowDetails("AllowUpdate") = "True"
							targetRowDetails("Create_Date") = DateTime.Now
							targetRowDetails("Create_User") = si.UserName
							targetRowDetails("Update_Date") = DateTime.Now
							targetRowDetails("Update_User") = si.UserName

							dt_Details.Rows.Add(targetRowDetails)
						Next	
							Dim Req_Type As String
							Dim UD6objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si,"U6_CommitItem")
							Dim lsAncestorListU6 = BRApi.Finance.Members.GetMembersUsingFilter(si, UD6objDimPk, "U6#" & sU6Input & ".Ancestors.Where(MemberDim = 'U6_CostCat')", True)
							If lsAncestorListU6(0).Member.Name = "Pay_Benefits" Then
								Req_Type = "CivPay"
							Else
								Req_Type = "Requirement"
							End If 
							Dim targetRow As DataRow = dt.NewRow()
                         
								targetRow("CMD_SPLN_REQ_ID") = NewReqID
								targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
								targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
								targetRow("WFTime_Name") = wfInfoDetails("TimeName")
								targetRow("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_SPLN_FundsCenter","")
								targetRow("REQ_ID") = Me.Get_FC_REQ_ID(si,sEntity)
								
								targetRow("REQ_ID_Type") = Req_Type
								targetRow("Title") = editedDataRow.ModifiedDataRow.Item("Title").ToString()
'								targetRow("APPN") = sU1APPNInput
								targetRow("APPN") = sU1FundCodeInput
'BRApi.ErrorLog.LogMessage(si,"Hit6")								
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
							
								
		                    	
								dt.Rows.Add(targetRow)
		             
		                Next
'BRApi.ErrorLog.LogMessage(si,"Hit7")
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_SPLN_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
'BRApi.ErrorLog.LogMessage(si,"Hit8")
						globals.SetStringValue($"FundsCenterStatusUpdates - {sEntity}", sREQWFStatus)
						Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
'Brapi.ErrorLog.LogMessage(si,"@HERE1" &String.Join(",",FCList))
						Dim customSubstVars As New Dictionary(Of String, String) 
						customSubstVars.Add("EntList","E#" & sEntity)
						customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
						Dim currentYear As String = wfInfoDetails("TimeName")
						Dim nextyear as Integer = Convert.ToInt32(currentYear) + 1
						customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
		                End Using
		            End Using
'BRApi.ErrorLog.LogMessage(si,"Hit9")

            Return Nothing
        End Function	

#End Region


#Region "Save REQ Revisions "		
Public Function Save_dg_CMD_SPLN_REQRevisions() As Object
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
		Dim editentity As String = String.empty
		Dim status As String = String.empty
		Dim statustopass As String = String.empty
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
			
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			Dim WFYear As Integer = wfInfoDetails("TimeName")
			Dim nextyear As Integer = wfInfoDetails("TimeName") + 1
'Brapi.ErrorLog.LogMessage(si, "Hit 1")	
		 Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
        Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
            sqlConn.Open()

            ' ************************************
            ' *** Fetch Data for BOTH tables *****
            ' ************************************
            ' --- Main Request Table (XFC_CMD_SPLN_REQ) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_SPLN_REQ(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_SPLN_REQ WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_SPLN_REQ_ID  = @CMD_SPLN_REQ_ID"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
				New SqlParameter("@CMD_SPLN_REQ_ID", SqlDbType.UniqueIdentifier) With {.Value = editedDataRows.Item(0).ModifiedDataRow("CMD_SPLN_REQ_ID")}
						}
              sqaReader.Fill_XFC_CMD_SPLN_REQ_DT(sqa, dt, sqlMain, sqlParams)
			  
'Brapi.ErrorLog.LogMessage(si, "HERE 4")	

            ' --- Details Table (XFC_CMD_SPLN_REQ_Details) ---
            Dim dt_Details As New DataTable()
			 Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_SPLN_REQ_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_SPLN_REQ_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name AND CMD_SPLN_REQ_ID  = @CMD_SPLN_REQ_ID"
            sqaReaderdetail.Fill_XFC_CMD_SPLN_REQ_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)

            ' ************************************
            ' ************************************
          		
	 		For Each editedDataRow As XFEditedDataRow In editedDataRows
					Dim EditAccount As String = editedDataRow.ModifiedDataRow.Item("Account")
					status = editedDataRow.ModifiedDataRow.Item("Flow")

brapi.ErrorLog.LogMessage(si,"CM - hit 1 Status = " & status)					
					If StatustoPass.XFEqualsIgnoreCase("") Then 
						statustopass = status & "|" & status
					Else  
						statustopass += "|" & status & "|" & status 
					End If 
					EditEntity = editedDataRow.ModifiedDataRow.Item("Entity")
brapi.ErrorLog.LogMessage(si,"CM - hit 1 entity = " & EditEntity)						
					
'brapi.ErrorLog.LogMessage(si,"Edit ACcount = " & EditAccount)
				    Dim targetRow As DataRow 											
					Dim req_ID_Val As Guid
					req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_SPLN_REQ_ID")
					
					targetRow = dt.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}'").FirstOrDefault()

					targetRow("Update_Date") = DateTime.Now
					targetRow("Update_User") = si.UserName																																																																																				

					Dim targetRowFundingCarryover As DataRow

					targetRowFundingCarryover = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = '{EditAccount}' and Fiscal_Year = '{nextyear}'").FirstOrDefault()		
					If targetRowFundingCarryover IsNot Nothing Then
						
						Dim Yearly As Decimal =  editedDataRow.ModifiedDataRow.Item("Yearly")	
'Brapi.ErrorLog.LogMessage(si,"Carryover" & Yearly)
						targetRowFundingCarryover("Yearly") = Yearly
						targetRowFundingCarryover("Update_Date") = DateTime.Now
                   		targetRowFundingCarryover("Update_User") = si.UserName  		
					End If 
					
				Dim targetRowFunding As DataRow
					targetRowFunding = dt_Details.Select($"CMD_SPLN_REQ_ID = '{req_ID_Val}' AND Account = '{EditAccount}' and Fiscal_Year = '{WFYear}'").FirstOrDefault()
		
					If targetRowFunding IsNot Nothing Then
			
						Dim Month1 As Decimal = editedDataRow.ModifiedDataRow.Item("Month1")		 
						Dim Month2 As Decimal = editedDataRow.ModifiedDataRow.Item("Month2")
						Dim Month3 As Decimal = editedDataRow.ModifiedDataRow.Item("Month3")
					    Dim Month4 As Decimal = editedDataRow.ModifiedDataRow.Item("Month4")
					 	Dim Month5 As Decimal = editedDataRow.ModifiedDataRow.Item("Month5")
						Dim Month6 As Decimal = editedDataRow.ModifiedDataRow.Item("Month6")		 
						Dim Month7 As Decimal = editedDataRow.ModifiedDataRow.Item("Month7")
						Dim Month8 As Decimal = editedDataRow.ModifiedDataRow.Item("Month8")
					    Dim Month9 As Decimal = editedDataRow.ModifiedDataRow.Item("Month9")
					 	Dim Month10 As Decimal = editedDataRow.ModifiedDataRow.Item("Month10")
						Dim Month11 As Decimal = editedDataRow.ModifiedDataRow.Item("Month11")
					 	Dim Month12 As Decimal = editedDataRow.ModifiedDataRow.Item("Month12")	
						
						targetRowFunding("Month1") = Month1
						targetRowFunding("Month2") = Month2
						targetRowFunding("Month3") = Month3
						targetRowFunding("Month4") = Month4
						targetRowFunding("Month5") = Month5
						targetRowFunding("Month6") = Month6
						targetRowFunding("Month7") = Month7
						targetRowFunding("Month8") = Month8
						targetRowFunding("Month9") = Month9
						targetRowFunding("Month10") = Month10
						targetRowFunding("Month11") = Month11
						targetRowFunding("Month12") = Month12
					
						
						targetRowFunding("Update_Date") = DateTime.Now
	                    targetRowFunding("Update_User") = si.UserName 
					
					End If 
				Next	   
		                
	
		                ' Persist changes back to the DB using the configured adapter
		               
		               sqaReaderdetail.Update_XFC_CMD_SPLN_REQ_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_SPLN_REQ(dt,sqa)
						globals.SetStringValue($"FundsCenterStatusUpdates - {EditEntity}", Statustopass)
						Dim wsID  = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"50 CMD SPLN")
'Brapi.ErrorLog.LogMessage(si,"@HERE1" &String.Join(",",FCList))
						Dim customSubstVars As New Dictionary(Of String, String) 
						customSubstVars.Add("EntList","E#" & Editentity)
						customSubstVars.Add("WFScen",wfInfoDetails("ScenarioName"))
						Dim currentYear As String = wfInfoDetails("TimeName")
						customSubstVars.Add("WFTime",$"T#{currentYear}M1,T#{currentYear}M2,T#{currentYear}M3,T#{currentYear}M4,T#{currentYear}M5,T#{currentYear}M6,T#{currentYear}M7,T#{currentYear}M8,T#{currentYear}M9,T#{currentYear}M10,T#{currentYear}M11,T#{currentYear}M12,T#{nextyear}")
						BRApi.Utilities.ExecuteDataMgmtSequence(si, wsID, "CMD_SPLN_Proc_Status_Updates", customSubstVars)
						
						
						
		                End Using
		            End Using

            Return Nothing
		
End Function		
#End Region
	

#Region "Req Attachment"
Private Function dg_CMD_SPLN_Attachments() As Object
Dim dt As New DataTable()
Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	

Dim CMD_SPLN_ID As New XFDynamicGridColumnDefinition()
			CMD_SPLN_ID.ColumnName = "CMD_SPLN_REQ_ID"
			CMD_SPLN_ID.IsFromTable = True
			CMD_SPLN_ID.IsVisible = False
			CMD_SPLN_ID.AllowUpdates = False
			columnDefinitions.Add(CMD_SPLN_ID)
			
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
	Dim REQ_ID_Split As List(Of String) = StringHelper.SplitString(sREQ, " ")	
	Dim RequirementID As String  = REQ_ID_Split(1)
	
	   Dim sql As String = $"SELECT Att.CMD_SPLN_REQ_ID, Att.Attach_File_Name 
	   						From XFC_CMD_SPLN_REQ_Attachment as Att
	   						LEFT JOIN XFC_CMD_SPLN_REQ AS Req
							ON Req.CMD_SPLN_REQ_ID = Att.CMD_SPLN_REQ_ID
							WHERE 
	  					 Req.REQ_ID = '{RequirementID}'"
	   
	   
	    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dt = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using
			
				     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
					
		End Function
	   
	   
#End Region	



#Region"Get REQID"
        Public Function Get_FC_REQ_ID(si As SessionInfo, fundCenter As String) As String
			Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
		  ' Query to get the highest REQ_ID from both tables
            Dim SQL As String = $"
                SELECT MAX(CAST(SUBSTRING(REQ_ID, CHARINDEX('_', REQ_ID) + 1, LEN(REQ_ID)) AS INT)) AS MaxID
                FROM (
                    SELECT REQ_ID FROM XFC_CMD_SPLN_REQ  WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
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



	End Class
End Namespace
