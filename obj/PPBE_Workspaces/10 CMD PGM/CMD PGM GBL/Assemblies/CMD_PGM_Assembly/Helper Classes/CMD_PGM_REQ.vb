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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_PGM_REQ
	
		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)

		Public Sub New()
		
			Dim FundCenter As New GBL_ColumnMap("FundsCenter","FundsCenter",GetType(String),True,"Ent_Member","WFCube","E#",100)
			ColumnMaps.Add(FundCenter)
			
			Dim APPN As New GBL_ColumnMap("APPN","APPN",GetType(String),True,"Member","U1_FundCode","U1#",100)
			ColumnMaps.Add(APPN)

			Dim REQ_ID As New GBL_ColumnMap("REQ_ID","REQ_ID",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(REQ_ID)
			
			Dim MDEP As New GBL_ColumnMap("MDEP","MDEP",GetType(String),True,"Member","U2_MDEP","U2#",100)
			ColumnMaps.Add(MDEP)
			
			Dim APE9 As New GBL_ColumnMap("APE9","APE9",GetType(String),True,"U3_Member","U3_All_APE","U3#",100)
			ColumnMaps.Add(APE9)
			
			Dim DollarType As New GBL_ColumnMap("DollarType","DollarType",GetType(String),True,"Member","U4_DollarType","U4#",100)
			ColumnMaps.Add(DollarType)
			
			Dim ObjectClass As New GBL_ColumnMap("ObjectClass","ObjectClass",GetType(String),False,"Member","U6_ObjClass","U6#",100)
			ColumnMaps.Add(ObjectClass)
			
			Dim C_Type As New GBL_ColumnMap("Ctype","Ctype",GetType(String),False,"Member","U5_CType","U5#",100)
			ColumnMaps.Add(C_Type)
			
			Dim Cycle As New GBL_ColumnMap("Cycle","Cycle",GetType(String),True,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(Cycle)
			
			Dim FY1 As New GBL_ColumnMap("FY1","FY1",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY1)
			
			Dim FY2 As New GBL_ColumnMap("FY2","FY2",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY2)
			
			Dim FY3 As New GBL_ColumnMap("FY3","FY3",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY3)
			
			Dim FY4 As New GBL_ColumnMap("FY4","FY4",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY4)
			
			Dim FY5 As New GBL_ColumnMap("FY5","FY5",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY5)
			
			Dim Title As New GBL_ColumnMap("Title","Title",GetType(String),True,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Title)
			
			Dim Description As New GBL_ColumnMap("Description","Description",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(Description)
			
			Dim Justification As New GBL_ColumnMap("Justification","Justification",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(Justification)
			
			Dim CostMethodology As New GBL_ColumnMap("CostMethodology","CostMethodology",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(CostMethodology)
			
			Dim ImpactIfNotFunded As New GBL_ColumnMap("ImpactIfNotFunded","ImpactIfNotFunded",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(ImpactIfNotFunded)
			
			Dim RiskIfNotFunded As New GBL_ColumnMap("RiskIfNotFunded","RiskIfNotFunded",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(RiskIfNotFunded)
			
			Dim CostGrowthJustification As New GBL_ColumnMap("CostGrowthJustification","CostGrowthJustification",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(CostGrowthJustification)
			
			Dim MustFund As New GBL_ColumnMap("MustFund","MustFund",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(MustFund)
			
			Dim FundingSource As New GBL_ColumnMap("FundingSource","FundingSource",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FundingSource)
			
			Dim ArmyInitiative_Directive As New GBL_ColumnMap("ArmyInitiative_Directive","ArmyInitiative_Directive",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ArmyInitiative_Directive)
			
			Dim CommandInitiative_Directive As New GBL_ColumnMap("CommandInitiative_Directive","CommandInitiative_Directive",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(CommandInitiative_Directive)
			
			Dim Activity_Exercise As New GBL_ColumnMap("Activity_Exercise","Activity_Exercise",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Activity_Exercise)
			
			Dim IT_CyberRequirement As New GBL_ColumnMap("IT_CyberRequirement","IT_CyberRequirement",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(IT_CyberRequirement)
			
			Dim UIC As New GBL_ColumnMap("UIC","UIC",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(UIC)
			
			Dim FLEX1 As New GBL_ColumnMap("FLEX1","FLEX1",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FLEX1)
			
			Dim FLEX2 As New GBL_ColumnMap("FLEX2","FLEX2",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FLEX2)
			
			Dim FLEX3 As New GBL_ColumnMap("FLEX3","FLEX3",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FLEX3)
			
			Dim FLEX4 As New GBL_ColumnMap("FLEX4","FLEX4",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FLEX4)
			
			Dim FLEX5 As New GBL_ColumnMap("FLEX5","FLEX5",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(FLEX5)
			
			Dim EmergingRequirement_ER As New GBL_ColumnMap("EmergingRequirement_ER","EmergingRequirement_ER",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(EmergingRequirement_ER)
			
			Dim CPATopic As New GBL_ColumnMap("CPATopic","CPATopic",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(CPATopic)
			
			Dim PBRSubmission As New GBL_ColumnMap("PBRSubmission","PBRSubmission",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(PBRSubmission)
			
			Dim UPLSubmission As New GBL_ColumnMap("UPLSubmission","UPLSubmission",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(UPLSubmission)
			
			Dim ContractNumber As New GBL_ColumnMap("ContractNumber","ContractNumber",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ContractNumber)
			
			Dim TaskOrderNumber As New GBL_ColumnMap("TaskOrderNumber","TaskOrderNumber",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(TaskOrderNumber)
			
			Dim AwardTargetDate As New GBL_ColumnMap("AwardTargetDate","AwardTargetDate",GetType(String),False,"date",String.Empty,String.Empty,1000)
			ColumnMaps.Add(AwardTargetDate)
			
			Dim POPExpirationDate As New GBL_ColumnMap("POPExpirationDate","POPExpirationDate",GetType(String),False,"date",String.Empty,String.Empty,1000)
			ColumnMaps.Add(POPExpirationDate)
			
			Dim ContractManYearEquiv_CME As New GBL_ColumnMap("ContractManYearEquiv_CME","ContractManYearEquiv_CME",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(ContractManYearEquiv_CME)
			
			Dim COREmail As New GBL_ColumnMap("COREmail","COREmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(COREmail)
			
			Dim POCEmail As New GBL_ColumnMap("POCEmail","POCEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(POCEmail)
			
			Dim Directorate As New GBL_ColumnMap("Directorate","Directorate",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Directorate)
			
			Dim Division As New GBL_ColumnMap("Division","Division",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Division)
			
			Dim Branch As New GBL_ColumnMap("Branch","Branch",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Branch)
			
			Dim ReviewingPOCEmail As New GBL_ColumnMap("ReviewingPOCEmail","ReviewingPOCEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ReviewingPOCEmail)
			
			Dim MDEPFunctionalEmail As New GBL_ColumnMap("MDEPFunctionalEmail","MDEPFunctionalEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(MDEPFunctionalEmail)
			
			Dim NotificationEmailList As New GBL_ColumnMap("NotificationEmailList","NotificationEmailList",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(NotificationEmailList)
			
			Dim GeneralComments_Notes As New GBL_ColumnMap("GeneralComments_Notes","GeneralComments_Notes",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(GeneralComments_Notes)
			
			Dim JUON As New GBL_ColumnMap("JUON","JUON",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(JUON)
			
			Dim ISR_Flag As New GBL_ColumnMap("ISR_Flag","ISR_Flag",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ISR_Flag)
			
			Dim Cost_Model As New GBL_ColumnMap("Cost_Model","Cost_Model",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Cost_Model)
			
			Dim Combat_Loss As New GBL_ColumnMap("Combat_Loss","Combat_Loss",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Combat_Loss)
			
			Dim Cost_Location As New GBL_ColumnMap("Cost_Location","Cost_Location",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Cost_Location)
			
			Dim Category_A_Code As New GBL_ColumnMap("Category_A_Code","Category_A_Code",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Category_A_Code)
			
			Dim CBS_Code As New GBL_ColumnMap("CBS_Code","CBS_Code",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(CBS_Code)
			
			Dim MIP_Proj_Code As New GBL_ColumnMap("MIP_Proj_Code","MIP_Proj_Code",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(MIP_Proj_Code)
			
			Dim SS_Priority As New GBL_ColumnMap("SS_Priority","SS_Priority",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(SS_Priority)
			
			Dim Commitment_Group As New GBL_ColumnMap("Commitment_Group","Commitment_Group",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Commitment_Group)
			
			Dim SS_Capability As New GBL_ColumnMap("SS_Capability","SS_Capability",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(SS_Capability)
			
			Dim Strategic_BIN As New GBL_ColumnMap("Strategic_BIN","Strategic_BIN",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Strategic_BIN)
			
			Dim LIN As New GBL_ColumnMap("LIN","LIN",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(LIN)
			
			Dim FY1_QTY As New GBL_ColumnMap("FY1_QTY","FY1_QTY",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY1_QTY)
			
			Dim FY2_QTY As New GBL_ColumnMap("FY2_QTY","FY2_QTY",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY2_QTY)
			
			Dim FY3_QTY As New GBL_ColumnMap("FY3_QTY","FY3_QTY",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY3_QTY)
			
			Dim FY4_QTY As New GBL_ColumnMap("FY4_QTY","FY4_QTY",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY4_QTY)
			
			Dim FY5_QTY As New GBL_ColumnMap("FY5_QTY","FY5_QTY",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY5_QTY)
			
			Dim RequirementType As New GBL_ColumnMap("RequirementType","RequirementType",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(RequirementType)
			
			Dim DD_Priority As New GBL_ColumnMap("DD_Priority","DD_Priority",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(DD_Priority)
			
			Dim Portfolio As New GBL_ColumnMap("Portfolio","Portfolio",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Portfolio)
			
			Dim DD_Capability As New GBL_ColumnMap("DD_Capability","DD_Capability",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(DD_Capability)
			
			Dim JNT_CAP_AREA As New GBL_ColumnMap("JNT_CAP_AREA","JNT_CAP_AREA",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(JNT_CAP_AREA)
			
			Dim TBM_COST_POOL As New GBL_ColumnMap("TBM_COST_POOL","TBM_COST_POOL",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(TBM_COST_POOL)
			
			Dim TBM_TOWER As New GBL_ColumnMap("TBM_TOWER","TBM_TOWER",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(TBM_TOWER)
			
			Dim APMSAITRNum As New GBL_ColumnMap("APMSAITRNum","APMSAITRNum",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(APMSAITRNum)
			
			Dim ZERO_TRUST_CAPABILITY As New GBL_ColumnMap("ZERO_TRUST_CAPABILITY","ZERO_TRUST_CAPABILITY",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ZERO_TRUST_CAPABILITY)
			
			Dim ASSOCIATED_DIRECTIVES As New GBL_ColumnMap("ASSOCIATED_DIRECTIVES","ASSOCIATED_DIRECTIVES",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ASSOCIATED_DIRECTIVES)
			
			Dim CLOUD_INDICATOR As New GBL_ColumnMap("CLOUD_INDICATOR","CLOUD_INDICATOR",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(CLOUD_INDICATOR)
			
			Dim STRAT_CYBERSEC_PGRM As New GBL_ColumnMap("STRAT_CYBERSEC_PGRM","STRAT_CYBERSEC_PGRM",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(STRAT_CYBERSEC_PGRM)
			
			Dim NOTES As New GBL_ColumnMap("NOTES","NOTES",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(NOTES)
			
			Dim UNIT_OF_MEASURE As New GBL_ColumnMap("UNIT_OF_MEASURE","UNIT_OF_MEASURE",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(UNIT_OF_MEASURE)
			
			Dim FY1_Items As New GBL_ColumnMap("FY1_Items","FY1_Items",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY1_Items)
			
			Dim FY1_Unit_Cost As New GBL_ColumnMap("FY1_Unit_Cost","FY1_Unit_Cost",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY1_Unit_Cost)
			
			Dim FY2_Items As New GBL_ColumnMap("FY2_Items","FY2_Items",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY2_Items)
			
			Dim FY2_Unit_Cost As New GBL_ColumnMap("FY2_Unit_Cost","FY2_Unit_Cost",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY2_Unit_Cost)
			
			Dim FY3_Items As New GBL_ColumnMap("FY3_Items","FY3_Items",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY3_Items)
			
			Dim FY3_Unit_Cost As New GBL_ColumnMap("FY3_Unit_Cost","FY3_Unit_Cost",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY3_Unit_Cost)
			
			Dim FY4_Items As New GBL_ColumnMap("FY4_Items","FY4_Items",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY4_Items)
			
			Dim FY4_Unit_Cost As New GBL_ColumnMap("FY4_Unit_Cost","FY4_Unit_Cost",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY4_Unit_Cost)
			
			Dim FY5_Items As New GBL_ColumnMap("FY5_Items","FY5_Items",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY5_Items)
			
			Dim FY5_Unit_Cost As New GBL_ColumnMap("FY5_Unit_Cost","FY5_Unit_Cost",GetType(String),False,"numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(FY5_Unit_Cost)
			
'---------------- MM PEG Fields on Hold - JM 11/12/15 - PER Dave Bellows we might not need MM PEG. On hold for now
'			Dim Group_Name As New GBL_ColumnMap("Group_Name","Group_Name",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Group_Name)
			
'			Dim Cat1 As New GBL_ColumnMap("Cat1","Cat1",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Cat1)
			
'			Dim Cat2 As New GBL_ColumnMap("Cat2","Cat2",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Cat2)
			
'			Dim Cat3 As New GBL_ColumnMap("Cat3","Cat3",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Cat3)
			
'			Dim Cat4 As New GBL_ColumnMap("Cat4","Cat4",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Cat4)
			
'			Dim RQMT_Driver As New GBL_ColumnMap("RQMT_Driver","RQMT_Driver",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(RQMT_Driver)
			
'			Dim Cost_Factor As New GBL_ColumnMap("Cost_Factor","Cost_Factor",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Cost_Factor)
			
'			Dim Factor_Type As New GBL_ColumnMap("Factor_Type","Factor_Type",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Factor_Type)
			
'			Dim Field_Description As New GBL_ColumnMap("Field_Description","Field_Description",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Field_Description)
			
'			Dim MMPEG_Issue As New GBL_ColumnMap("MMPEG_Issue","MMPEG_Issue",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(MMPEG_Issue)
			
'			Dim Post_COEX_Issue As New GBL_ColumnMap("Post_COEX_Issue","Post_COEX_Issue",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Post_COEX_Issue)
			
'			Dim Inflation_Type As New GBL_ColumnMap("Inflation_Type","Inflation_Type",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(Inflation_Type)

			
			
		End Sub

	End Class
End Namespace
