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
	Public Class CMD_UFR_REQ

		Public ReadOnly ColumnMaps As New List(Of GBL_ColumnMap)
		
		Public Sub New()
		
		Dim ABO_Decision As New GBL_ColumnMap("ABO_Decision","ABO_Decision",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(ABO_Decision)
		
		Dim ABO_Funded_Amount As New GBL_ColumnMap("ABO_Funded_Amount","ABO_Funded_Amount",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(ABO_Funded_Amount)
		
		Dim ABO_Review_Input As New GBL_ColumnMap("ABO_Review_Input","ABO_Review_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(ABO_Review_Input)
		
		Dim ABO_Review_POC As New GBL_ColumnMap("ABO_Review_POC","ABO_Review_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(ABO_Review_POC)
		
		Dim ABO_UFR_Status As New GBL_ColumnMap("ABO_UFR_Status","ABO_UFR_Status",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(ABO_UFR_Status)
		
		'--- G1 through G8 And JAG Staffing Elements at the Command and Sub Command Level ---
		
		'- G1 -
		Dim G1_Input As New GBL_ColumnMap("G1_Input","G1_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G1_Input)
		
		Dim G1_POC As New GBL_ColumnMap("G1_POC","G1_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G1_POC)
		
		'- G2 -
		Dim G2_Input As New GBL_ColumnMap("G2_Input","G2_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G2_Input)
		
		Dim G2_POC As New GBL_ColumnMap("G2_POC","G2_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G2_POC)
		
		'- G3 -
		Dim G3_Input As New GBL_ColumnMap("G3_Input","G3_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G3_Input)
		
		Dim G3_POC As New GBL_ColumnMap("G3_POC","G3_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G3_POC)
		
		'- G4 -
		Dim G4_Input As New GBL_ColumnMap("G4_Input","G4_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G4_Input)
		
		Dim G4_POC As New GBL_ColumnMap("G4_POC","G4_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G4_POC)
		
		'- G5 -
		Dim G5_Input As New GBL_ColumnMap("G5_Input","G5_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G5_Input)
		
		Dim G5_POC As New GBL_ColumnMap("G5_POC","G5_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G5_POC)
		
		'- G6 -
		Dim G6_Input As New GBL_ColumnMap("G6_Input","G6_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G6_Input)
		
		Dim G6_POC As New GBL_ColumnMap("G6_POC","G6_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G6_POC)
		
		'- G7 -
		Dim G7_Input As New GBL_ColumnMap("G7_Input","G7_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G7_Input)
		
		Dim G7_POC As New GBL_ColumnMap("G7_POC","G7_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G7_POC)
		
		'- G8 -
		Dim G8_Input As New GBL_ColumnMap("G8_Input","G8_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(G8_Input)
		
		Dim G8_POC As New GBL_ColumnMap("G8_POC","G8_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G8_POC)
		
		'- JAG -
		Dim JAG_Input As New GBL_ColumnMap("JAG_Input","JAG_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(JAG_Input)
		
		Dim JAG_POC As New GBL_ColumnMap("JAG_POC","JAG_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(JAG_POC)
		
		'--- G3 and G8 at HQ Level ---
		'- HQ G3 -
		Dim HQ_G3_Input As New GBL_ColumnMap("HQ_G3_Input","HQ_G3_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(HQ_G3_Input)
		
		Dim HQ_G3_POC As New GBL_ColumnMap("HQ_G3_POC","HQ_G3_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(HQ_G3_POC)
		
		'- HQ G8 -
		Dim HQ_G8_Input As New GBL_ColumnMap("HQ_G8_Input","HQ_G8_Input",GetType(String),False,"String",String.Empty,String.Empty,100000) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(HQ_G8_Input)
		
		Dim HQ_G8_POC As New GBL_ColumnMap("HQ_G8_POC","HQ_G8_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(HQ_G8_POC)
		
		'--- Approval / Funding Fields CMD ---
		Dim Fund_Amount As New GBL_ColumnMap("Fund_Amount","Fund_Amount",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Fund_Amount)
		
		Dim Fund_Status As New GBL_ColumnMap("Fund_Status","Fund_Status",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Fund_Status)
		
		Dim Fund_Source As New GBL_ColumnMap("Fund_Source","Fund_Source",GetType(String),False,"String",String.Empty,String.Empty,100) ' Max is large, using 100000 (typical Max length in string maps)
		ColumnMaps.Add(Fund_Source)
	
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim APE9 As New GBL_ColumnMap("APE9","APE9",GetType(String),True,"U3_Member","U3_All_APE","U3#",100)
		ColumnMaps.Add(APE9)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim APPN As New GBL_ColumnMap("APPN","APPN",GetType(String),True,"Member","U1_FundCode","U1#",100)
		ColumnMaps.Add(APPN)
		
		Dim Army_Campaign_Objectives As New GBL_ColumnMap("Army_Campaign_Objectives","Army_Campaign_Objectives",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Army_Campaign_Objectives)
		
		Dim BRP_Topic As New GBL_ColumnMap("BRP_Topic","BRP_Topic",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(BRP_Topic)
		
		Dim CMD_HQ_G8_Submission_Approver As New GBL_ColumnMap("CMD_HQ_G8_Submission_Approver","CMD_HQ_G8_Submission_Approver",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(CMD_HQ_G8_Submission_Approver)
		
		Dim CMD_HQ_G8_Submission_POC As New GBL_ColumnMap("CMD_HQ_G8_Submission_POC","CMD_HQ_G8_Submission_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(CMD_HQ_G8_Submission_POC)
		
		' uniqueidentifier is often mapped as String or GUID (if available). Using String for consistency with other maps. NOT NULL means Required=True.
		Dim CMD_UFR_Tracking_No As New GBL_ColumnMap("CMD_UFR_Tracking_No","CMD_UFR_Tracking_No",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(CMD_UFR_Tracking_No)
		
		Dim Command_UFR_Priority As New GBL_ColumnMap("Command_UFR_Priority","Command_UFR_Priority",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Command_UFR_Priority)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Command_UFR_Status As New GBL_ColumnMap("Command_UFR_Status","Command_UFR_Status",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Command_UFR_Status)
		
		' datetime is mapped to GetType(DateTime). NOT NULL means Required=True.
		Dim Create_Date As New GBL_ColumnMap("Create_Date","Create_Date",GetType(DateTime),True,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Create_Date)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Create_User As New GBL_ColumnMap("Create_User","Create_User",GetType(String),True,"String",String.Empty,String.Empty,50)
		ColumnMaps.Add(Create_User)
		
		' datetime is mapped to GetType(DateTime)
		Dim Date_Decision_Needed_By As New GBL_ColumnMap("Date_Decision_Needed_By","Date_Decision_Needed_By",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Date_Decision_Needed_By)
		
		' nvarchar(MAX) is large, using 100000 (typical Max length in string maps). NOT NULL means Required=True.
		Dim Description As New GBL_ColumnMap("Description","Description",GetType(String),True,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(Description)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Dollar_Type As New GBL_ColumnMap("Dollar_Type","Dollar_Type",GetType(String),True,"Member","U4_DollarType","U4#",100)
		ColumnMaps.Add(Dollar_Type)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Entity As New GBL_ColumnMap("Entity","Entity",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Entity)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Fund_Type As New GBL_ColumnMap("Fund_Type","Fund_Type",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Fund_Type)
		
		' datetime is mapped to GetType(DateTime)
		Dim Funds_Required_By As New GBL_ColumnMap("Funds_Required_By","Funds_Required_By",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Funds_Required_By)
		
		' nvarchar(MAX) is large, using 100000
		Dim G_3_5_7_Review_Input As New GBL_ColumnMap("G_3_5_7_Review_Input","G_3_5_7_Review_Input",GetType(String),False,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(G_3_5_7_Review_Input)
		
		Dim G_3_5_7_Review_POC As New GBL_ColumnMap("G_3_5_7_Review_POC","G_3_5_7_Review_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G_3_5_7_Review_POC)
		
		' nvarchar(MAX) is large, using 100000
		Dim G8_PAE_Review_Input As New GBL_ColumnMap("G8_PAE_Review_Input","G8_PAE_Review_Input",GetType(String),False,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(G8_PAE_Review_Input)
		
		Dim G8_PAE_Review_POC As New GBL_ColumnMap("G8_PAE_Review_POC","G8_PAE_Review_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(G8_PAE_Review_POC)
		
		' nvarchar(MAX) is large, using 100000
		Dim Integrator_Input As New GBL_ColumnMap("Integrator_Input","Integrator_Input",GetType(String),False,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(Integrator_Input)
		
		Dim Integrator_POC As New GBL_ColumnMap("Integrator_POC","Integrator_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Integrator_POC)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim MDEP As New GBL_ColumnMap("MDEP","MDEP",GetType(String),True,"Member","U2_MDEP","U2#",100)
		ColumnMaps.Add(MDEP)
						
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Obj_Class As New GBL_ColumnMap("Obj_Class","Obj_Class",GetType(String),True,"Member","U6_ObjClass","U6#",100) ' Assuming a cube mapping based on original logic
		ColumnMaps.Add(Obj_Class)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim PEG As New GBL_ColumnMap("PEG","PEG",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(PEG)
		
		' nvarchar(MAX) is large, using 100000
		Dim PEG_Review_Input As New GBL_ColumnMap("PEG_Review_Input","PEG_Review_Input",GetType(String),False,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(PEG_Review_Input)
		
		Dim PEG_Review_POC As New GBL_ColumnMap("PEG_Review_POC","PEG_Review_POC",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(PEG_Review_POC)
		
		Dim RDA_UFR_Executable_Fund_Year As New GBL_ColumnMap("RDA_UFR_Executable_Fund_Year","RDA_UFR_Executable_Fund_Year",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(RDA_UFR_Executable_Fund_Year)
		
		Dim RDA_UFR_FY_New_Start As New GBL_ColumnMap("RDA_UFR_FY_New_Start","RDA_UFR_FY_New_Start",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(RDA_UFR_FY_New_Start)
		
		Dim REQ_Link_ID As New GBL_ColumnMap("REQ_Link_ID","REQ_Link_ID",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(REQ_Link_ID)
		
		' nvarchar(MAX) is large, using 100000. NOT NULL means Required=True.
		Dim Requirement_Background As New GBL_ColumnMap("Requirement_Background","Requirement_Background",GetType(String),True,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(Requirement_Background)
		
		Dim Review_Entity As New GBL_ColumnMap("Review_Entity","Review_Entity",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Review_Entity)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim ROC As New GBL_ColumnMap("ROC","ROC",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(ROC)
		
		Dim Review_Staff As New GBL_ColumnMap("Review_Staff","Review_Staff",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Review_Staff)
		
		Dim Functional_Area As New GBL_ColumnMap("Functional_Area","Functional_Area",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Functional_Area)
		
		Dim Study_Category As New GBL_ColumnMap("Study_Category","Study_Category",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Study_Category)
		
		Dim MustFund As New GBL_ColumnMap("MustFund","MustFund",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(MustFund)
		
		Dim Rollover_To_Next_UFR_Cycle As New GBL_ColumnMap("Rollover_To_Next_UFR_Cycle","Rollover_To_Next_UFR_Cycle",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Rollover_To_Next_UFR_Cycle)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim SAG_SSN As New GBL_ColumnMap("SAG_SSN","SAG_SSN",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(SAG_SSN)
		
		Dim Solicited_NonSolicited As New GBL_ColumnMap("Solicited_NonSolicited","Solicited_NonSolicited",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Solicited_NonSolicited)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Title As New GBL_ColumnMap("Title","Title",GetType(String),True,"String",String.Empty,String.Empty,1000)
		ColumnMaps.Add(Title)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim UFR_Amount As New GBL_ColumnMap("UFR_Amount","UFR_Amount",GetType(String),True,"String",String.Empty,String.Empty,1000)
		ColumnMaps.Add(UFR_Amount)
		
		Dim UFR_Capability_GAP As New GBL_ColumnMap("UFR_Capability_GAP","UFR_Capability_GAP",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(UFR_Capability_GAP)
		
		Dim UFR_Capability_GAP_If_CMD_Fund As New GBL_ColumnMap("UFR_Capability_GAP_If_CMD_Fund","UFR_Capability_GAP_If_CMD_Fund",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(UFR_Capability_GAP_If_CMD_Fund)
		
		Dim UFR_Driver As New GBL_ColumnMap("UFR_Driver","UFR_Driver",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(UFR_Driver)
		
		' nvarchar(MAX) is large, using 100000
		Dim UFR_Driver_Explanation As New GBL_ColumnMap("UFR_Driver_Explanation","UFR_Driver_Explanation",GetType(String),False,"String",String.Empty,String.Empty,100000)
		ColumnMaps.Add(UFR_Driver_Explanation)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim UFR_ID As New GBL_ColumnMap("UFR_ID","UFR_ID",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(UFR_ID)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim UFR_ID_Type As New GBL_ColumnMap("UFR_ID_Type","UFR_ID_Type",GetType(String),True,"String",String.Empty,String.Empty,20)
		ColumnMaps.Add(UFR_ID_Type)
		
		' datetime is mapped to GetType(DateTime). NOT NULL means Required=True.
		Dim Update_Date As New GBL_ColumnMap("Update_Date","Update_Date",GetType(DateTime),True,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Update_Date)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim Update_User As New GBL_ColumnMap("Update_User","Update_User",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Update_User)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim WFCMD_Name As New GBL_ColumnMap("WFCMD_Name","WFCMD_Name",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(WFCMD_Name)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim WFScenario_Name As New GBL_ColumnMap("WFScenario_Name","WFScenario_Name",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(WFScenario_Name)
		
		' This column is NOT NULL in the SQL DDL, so setting 'Required' to True
		Dim WFTime_Name As New GBL_ColumnMap("WFTime_Name","WFTime_Name",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(WFTime_Name)
		
		Dim Initial_Review_Type As New GBL_ColumnMap("Initial_Review_Type","Initial_Review_Type",GetType(String),True,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(Initial_Review_Type)
		
		Dim Pre_BRP_Date As New GBL_ColumnMap("Pre_BRP_Date","Pre_BRP_Date",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Pre_BRP_Date)
		
		Dim COL_BRP_Date As New GBL_ColumnMap("COL_BRP_Date","COL_BRP_Date",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(COL_BRP_Date)
		
		Dim Two_Star_BRP_Date As New GBL_ColumnMap("Two_Star_BRP_Date","Two_Star_BRP_Date",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Two_Star_BRP_Date)
		
		Dim Three_Star_BRP_Date As New GBL_ColumnMap("Three_Star_BRP_Date","Three_Star_BRP_Date",GetType(DateTime),False,"DateTime",String.Empty,String.Empty,0)
		ColumnMaps.Add(Three_Star_BRP_Date)
		
		Dim BRP_Notes As New GBL_ColumnMap("BRP_Notes","BRP_Notes",GetType(String),False,"String",String.Empty,String.Empty,100)
		ColumnMaps.Add(BRP_Notes)
		
		End Sub

	End Class
End Namespace