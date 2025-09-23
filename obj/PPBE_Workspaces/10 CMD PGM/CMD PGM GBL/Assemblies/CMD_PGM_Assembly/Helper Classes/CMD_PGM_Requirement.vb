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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_PGM_Requirement
		'Class to hold the requirement object
		'-----Mapping definitions----
		Public Dim Entity As String = ""
		Public Dim FundCode As String = ""
		Public Dim MDEP As String = ""
		Public Dim APE9 As String = ""
		Public Dim DollarType As String = ""
		Public Dim Cycle As String = ""
		Public Dim FY1 As String = ""
		Public Dim FY2 As String = ""
		Public Dim FY3 As String = ""
		Public Dim FY4 As String = ""
		Public Dim FY5 As String = ""
		Public Dim Title As String = ""
		Public Dim Description As String = ""
		Public Dim Justification  As String = ""
		Public Dim ImpactifnotFunded As String = ""
		Public Dim RiskifnotFunded As String = ""
		Public Dim CostMethodology As String = ""
		Public Dim CostGrowthJustification As String = ""
		Public Dim MustFund As String = ""
		Public Dim FundingSource  As String = ""
		Public Dim ArmyInitiative_Directive As String = ""
		Public Dim CommandInitiative_Directive As String = ""
		Public Dim Activity_Exercise As String = ""
		Public Dim IT_CyberRequirement  As String = ""
		Public Dim UIC As String = ""
		Public Dim FlexField1  As String = ""
		Public Dim FlexField2  As String = ""
		Public Dim FlexField3  As String = ""
		Public Dim FlexField4  As String = ""
		Public Dim FlexField5  As String = ""
		Public Dim EmergingRequirement As String = ""
		Public Dim CPATopic As String = ""
		Public Dim PBRSubmission As String = ""
		Public Dim UPLSubmission As String = ""
		Public Dim ContractNumber  As String = ""
		Public Dim TaskOrderNumber  As String = ""
		Public Dim AwardTargetDate  As String = ""
		Public Dim POPExpirationDate  As String = ""
		Public Dim ContractorManYearEquiv_CME  As String = ""
		Public Dim COREmail  As String = ""
		Public Dim POCEmail As String = ""
		Public Dim Directorate  As String = ""
		Public Dim Division  As String = ""
		Public Dim Branch As String = ""
		Public Dim ReviewingPOCEmail As String = ""
		Public Dim MDEPFunctionalEmail As String = ""
		Public Dim NotificationListEmails As String = ""
		Public Dim GeneralComments_Notes As String = ""
		Public Dim JUON As String = ""
		Public Dim ISR_Flag As String = ""
		Public Dim Cost_Model As String = ""
		Public Dim Combat_Loss As String = ""
		Public Dim Cost_Location As String = ""
		Public Dim Category_A_Code As String = ""
		Public Dim CBS_Code As String = ""
		Public Dim MIP_Proj_Code As String = ""
		Public Dim RequirementType As String = ""
		Public Dim DD_Priority As String = ""
		Public Dim Portfolio As String = ""
		Public Dim DD_Capability As String = ""
		Public Dim JNT_CAP_AREA As String = ""
		Public Dim TBM_COST_POOL As String = ""
		Public Dim TBM_TOWER As String = ""
		Public Dim APMSAITRNum As String = ""
		Public Dim ZERO_TRUST_CAPABILITY As String = ""
		Public Dim ASSOCIATED_DIRECTIVES As String = ""
		Public Dim CLOUD_INDICATOR As String = ""
		Public Dim STRAT_CYBERSEC_PGRM As String = ""
		Public Dim NOTES As String = ""
		Public Dim UNIT_OF_MEASURE As String = ""
		Public Dim FY1_ITEMS As String = ""
		Public Dim FY1_UNIT_COST As String = ""
		Public Dim FY2_ITEMS As String = ""
		Public Dim FY2_UNIT_COST As String = ""
		Public Dim FY3_ITEMS As String = ""
		Public Dim FY3_UNIT_COST As String = ""
		Public Dim FY4_ITEMS As String = ""
		Public Dim FY4_UNIT_COST As String = ""
		Public Dim FY5_ITEMS As String = ""
		Public Dim FY5_UNIT_COST As String = ""
		Public Dim SS_Priority As String = ""
		Public Dim Commitment_Group As String = ""
		Public Dim SS_Capability As String = ""
		Public Dim Strategic_BIN As String = ""
		Public Dim LIN As String = ""
		Public Dim FY1_QTY As String = ""
		Public Dim FY2_QTY As String = ""
		Public Dim FY3_QTY As String = ""
		Public Dim FY4_QTY As String = ""
		Public Dim FY5_QTY As String = ""
		Public Dim CommitmentItem As String = ""
		Public Dim sCType As String = ""
		Public Dim Status As String = ""
		Public Dim flow As String = ""				'Calculated
		Public Dim REQ_ID As String = ""			'Calculated
		Public Dim command As String = ""			'Calculated
		Public Dim scenario As String = ""			'Calculated
		
		Public Dim valid As Boolean = True
		Public Dim validationError As String = ""
		
'		Public Sub SetValidationError(ByVal valError As String)
'			If String.IsNullOrWhiteSpace(validationError) Then
'				validationError = "REQ " & title & " has errors. " &  valError
'			Else
'				validationError = validationError  & ", "  & valError
'			End If
'		End Sub
		
		Public Function StringOutput() As String
			Dim output As String = Me.command & "," & _	
			Me.Entity & "," & _
			Me.FundCode & "," & _
			Me.MDEP & "," & _
			Me.APE9 & "," & _
			Me.DollarType & "," & _
			Me.Cycle & "," & _
			Me.FY1 & "," & _
			Me.FY2 & "," & _
			Me.FY3 & "," & _
			Me.FY4 & "," & _
			Me.FY5 & "," & _
			Me.Title & "," & _
			Me.Description  & "," & _
			Me.Justification  & "," & _
			Me.ImpactifnotFunded & "," & _
			Me.RiskifnotFunded & "," & _
			Me.CostMethodology & "," & _
			Me.CostGrowthJustification & "," & _
			Me.MustFund & "," & _
			Me.FundingSource  & "," & _
			Me.ArmyInitiative_Directive & "," & _
			Me.CommandInitiative_Directive & "," & _
			Me.Activity_Exercise & "," & _
			Me.IT_CyberRequirement  & "," & _
			Me.UIC & "," & _
			Me.FlexField1  & "," & _
			Me.FlexField2  & "," & _
			Me.FlexField3  & "," & _
			Me.FlexField4  & "," & _
			Me.FlexField5  & "," & _
			Me.EmergingRequirement & "," & _
			Me.CPATopic & "," & _
			Me.PBRSubmission & "," & _
			Me.UPLSubmission & "," & _
			Me.ContractNumber  & "," & _
			Me.TaskOrderNumber  & "," & _
			Me.AwardTargetDate  & "," & _
			Me.POPExpirationDate  & "," & _
			Me.ContractorManYearEquiv_CME  & "," & _
			Me.COREmail  & "," & _
			Me.POCEmail & "," & _
			Me.Directorate  & "," & _
			Me.Division  & "," & _
			Me.Branch & "," & _
			Me.ReviewingPOCEmail & "," & _
			Me.MDEPFunctionalEmail & "," & _
			Me.NotificationListEmails & "," & _
			Me.GeneralComments_Notes & "," & _
			Me.JUON & "," & _
			Me.ISR_Flag & "," & _
			Me.Cost_Model & "," & _
			Me.Combat_Loss & "," & _
			Me.Cost_Location & "," & _
			Me.Category_A_Code & "," & _
			Me.CBS_Code & "," & _
			Me.MIP_Proj_Code & "," & _
			Me.RequirementType & "," & _
			Me.DD_Priority & "," & _
			Me.Portfolio & "," & _
			Me.DD_Capability & "," & _
			Me.JNT_CAP_AREA & "," & _
			Me.TBM_COST_POOL & "," & _
			Me.TBM_TOWER & "," & _
			Me.APMSAITRNum & "," & _
			Me.ZERO_TRUST_CAPABILITY & "," & _
			Me.ASSOCIATED_DIRECTIVES & "," & _
			Me.CLOUD_INDICATOR & "," & _
			Me.STRAT_CYBERSEC_PGRM & "," & _
			Me.NOTES & "," & _
			Me.UNIT_OF_MEASURE & "," & _
			Me.FY1_ITEMS & "," & _
			Me.FY1_UNIT_COST & "," & _
			Me.FY2_ITEMS & "," & _
			Me.FY2_UNIT_COST & "," & _
			Me.FY3_ITEMS & "," & _
			Me.FY3_UNIT_COST & "," & _
			Me.FY4_ITEMS & "," & _
			Me.FY4_UNIT_COST & "," & _
			Me.FY5_ITEMS & "," & _
			Me.FY5_UNIT_COST & "," & _
			Me.SS_Priority & "," & _
			Me.Commitment_Group & "," & _
			Me.SS_Capability & "," & _
			Me.Strategic_BIN & "," & _
			Me.LIN & "," & _
			Me.FY1_QTY & "," & _
			Me.FY2_QTY & "," & _
			Me.FY3_QTY & "," & _
			Me.FY4_QTY & "," & _
			Me.FY5_QTY & "," & _
			Me.flow & "," & _				
			Me.REQ_ID & "," & _			
			Me.scenario & "," & _	
			Me.validationError
			
			Return output
		End Function
        Public Function Test(ByVal si As SessionInfo) As Object
            Try

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
