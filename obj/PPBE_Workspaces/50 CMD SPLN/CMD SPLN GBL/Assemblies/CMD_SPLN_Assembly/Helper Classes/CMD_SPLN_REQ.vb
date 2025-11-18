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
	Public Class CMD_SPLN_REQ
	
		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)

		Public Sub New()
		
			Dim FundCenter As New GBL_ColumnMap("FundCenter","FundCenter",GetType(String),True,"Ent_Member","WFCube","E#",100)
			ColumnMaps.Add(FundCenter)
			
			Dim APPN As New GBL_ColumnMap("Fundcode","Fundcode",GetType(String),True,"Member","U1_FundCode","U1#",100)
			ColumnMaps.Add(APPN)
			
			Dim MDEP As New GBL_ColumnMap("MDEP","MDEP",GetType(String),True,"Member","U2_MDEP","U2#",100)
			ColumnMaps.Add(MDEP)
			
			Dim APE9 As New GBL_ColumnMap("APE9","APE9",GetType(String),True,"U3_Member","U3_All_APE","U3#",100)
			ColumnMaps.Add(APE9)
			
			Dim DollarType As New GBL_ColumnMap("DollarType","DollarType",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(DollarType)
			
			Dim ObjectClass As New GBL_ColumnMap("ObjectClass","ObjectClass",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(ObjectClass)
			
			Dim Cycle As New GBL_ColumnMap("Cycle","Cycle",GetType(String),True,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(Cycle)
						
			Dim COM_M1 As New GBL_ColumnMap("COM_M1","COM_M1",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M1)
		
			Dim COM_M2 As New GBL_ColumnMap("COM_M2","COM_M2",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M2)
			
			Dim COM_M3 As New GBL_ColumnMap("COM_M3","COM_M3",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M3)
			
			Dim COM_M4 As New GBL_ColumnMap("COM_M4","COM_M4",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M4)
			
			Dim COM_M5 As New GBL_ColumnMap("COM_M5","COM_M5",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M5)
			
			Dim COM_M6 As New GBL_ColumnMap("COM_M6","COM_M6",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M6)
			
			Dim COM_M7 As New GBL_ColumnMap("COM_M7","COM_M7",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M7)
			
			Dim COM_M8 As New GBL_ColumnMap("COM_M8","COM_M8",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M8)
			
			Dim COM_M9 As New GBL_ColumnMap("COM_M9","COM_M9",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M9)
			
			Dim COM_M10 As New GBL_ColumnMap("COM_M10","COM_M10",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M10)
			
			Dim COM_M11 As New GBL_ColumnMap("COM_M11","COM_M11",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M11)
			
			Dim COM_M12 As New GBL_ColumnMap("COM_M12","COM_M12",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_M12)
			
			Dim COM_Carryover As New GBL_ColumnMap("COM_Carryover","COM_Carryover",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(COM_Carryover)
						
			Dim OBL_M1 As New GBL_ColumnMap("OBL_M1","OBL_M1",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M1)
			
			Dim OBL_M2 As New GBL_ColumnMap("OBL_M2","OBL_M2",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M2)
			
			Dim OBL_M3 As New GBL_ColumnMap("OBL_M3","OBL_M3",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M3)
			
			Dim OBL_M4 As New GBL_ColumnMap("OBL_M4","OBL_M4",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M4)
			
			Dim OBL_M5 As New GBL_ColumnMap("OBL_M5","OBL_M5",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M5)
			
			Dim OBL_M6 As New GBL_ColumnMap("OBL_M6","OBL_M6",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M6)
			
			Dim OBL_M7 As New GBL_ColumnMap("OBL_M7","OBL_M7",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M7)
			
			Dim OBL_M8 As New GBL_ColumnMap("OBL_M8","OBL_M8",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M8)
			
			Dim OBL_M9 As New GBL_ColumnMap("OBL_M9","OBL_M9",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M9)
			
			Dim OBL_M10 As New GBL_ColumnMap("OBL_M10","OBL_M10",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M10)
			
			Dim OBL_M11 As New GBL_ColumnMap("OBL_M11","OBL_M11",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M11)
			
			Dim OBL_M12 As New GBL_ColumnMap("OBL_M12","OBL_M12",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_M12)
			
			Dim OBL_Carryover As New GBL_ColumnMap("OBL_Carryover","OBL_Carryover",GetType(String),False,"Numeric",String.Empty,String.Empty,30)
			ColumnMaps.Add(OBL_Carryover)
			
			Dim Title As New GBL_ColumnMap("Title","Title",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Title)
			
			Dim Description As New GBL_ColumnMap("Description","Description",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(Description)
			
			Dim Justification As New GBL_ColumnMap("Justification","Justification",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(Justification)
			
			Dim ImpactIfNotFunded As New GBL_ColumnMap("ImpactIfNotFunded","ImpactIfNotFunded",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(ImpactIfNotFunded)
			
			Dim RiskIfNotFunded As New GBL_ColumnMap("RiskIfNotFunded","RiskIfNotFunded",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(RiskIfNotFunded)
						
			Dim CostMethodology As New GBL_ColumnMap("CostMethodology","CostMethodology",GetType(String),False,"String",String.Empty,String.Empty,100000)
			ColumnMaps.Add(CostMethodology)
			
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
			
			Dim AwardTargetDate As New GBL_ColumnMap("AwardTargetDate","AwardTargetDate",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(AwardTargetDate)
			
			Dim POPExpirationDate As New GBL_ColumnMap("POPExpirationDate","POPExpirationDate",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(POPExpirationDate)
			
			Dim CME As New GBL_ColumnMap("CME","CME",GetType(String),False,"String",String.Empty,String.Empty,100)
			ColumnMaps.Add(CME)
			
			Dim COREmail As New GBL_ColumnMap("COREmail","COREmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(COREmail)
			
			Dim POCEmail As New GBL_ColumnMap("POCEmail","POCEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(POCEmail)
			
			Dim Directorate As New GBL_ColumnMap("Directorate","Directorate",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Directorate)
			
			Dim Divisionision As New GBL_ColumnMap("Division","Division",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Divisionision)
			
			Dim Branch As New GBL_ColumnMap("Branch","Branch",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(Branch)
			
			Dim ReviewingPOCEmail As New GBL_ColumnMap("ReviewingPOCEmail","ReviewingPOCEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(ReviewingPOCEmail)
			
			Dim MDEPFunctionalEmail As New GBL_ColumnMap("MDEPFunctionalEmail","MDEPFunctionalEmail",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(MDEPFunctionalEmail)
			
			Dim NotificationEmailList As New GBL_ColumnMap("NotificationEmailList","NotificationEmailList",GetType(String),False,"String",String.Empty,String.Empty,1000)
			ColumnMaps.Add(NotificationEmailList)

'			Dim UNIT_OF_MEASURE As New GBL_ColumnMap("UNIT_OF_MEASURE","UNIT_OF_MEASURE",GetType(String),False,"String",String.Empty,String.Empty,1000)
'			ColumnMaps.Add(UNIT_OF_MEASURE)

			
			
		End Sub

	End Class
End Namespace
