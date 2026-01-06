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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class CMD_UFR_Dyn_Grid_Svc
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
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_REQManage") Then
						Return dg_CMD_UFR_REQManage()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_Create") Then
Brapi.ErrorLog.LogMessage(si, "Calling dg_CMD_UFR_Create")
				    	Return dg_CMD_UFR_Create()        
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_UFRList") Then
				    	Return dg_CMD_UFR_UFRList()        
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_Prioritize_REQ") Then
				    	Return dg_CMD_UFR_Prioritize_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_REQAmtsReview") Then
				    	Return dg_CMD_UFR_REQAmtsReview()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_CopySPLNREQ") Then
				    	Return dg_CMD_UFR_CopySPLNREQ()            
					End If
					
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

		
        Public Function SaveDynamicGridData(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridSaveDataResult Implements IWsasDynamicGridV800.SaveDynamicGridData
            Me.si = si
            Me.globals = globals
            Me.workspace = workspace
            Me.args = args	
			Me.api = api
			
			Try
                If (Globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
             
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_REQManage") Then
					    	Return Save_dg_CMD_UFR_REQManage()           
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_Create") Then
Brapi.ErrorLog.LogMessage(si, "Calling Save_dg_CMD_UFR_Create")
				    	Return Save_dg_CMD_UFR_Create()       
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_Prioritize_REQ") Then
				    	Return Save_dg_CMD_UFR_Prioritize_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_REQAmtsReview") Then
				    	Return Save_dg_CMD_UFR_REQAmtsReview()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_UFR_CopySPLNREQ") Then
				    	Return Save_dg_CMD_UFR_CopySPLNREQ()            
					End If
					
				End If
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		
#Region "Constants"
'	Private BR_CMD_UFRDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.CMD_UFR_DataSet
	Private BR_CMD_UFRDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_UFR_DataSet.MainClass
#End Region	

#Region "Dynamic Grid Services Helpers"

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
			
		  ' Query to get the highest UFR_ID from both tables
            Dim SQL As String = $"
                SELECT MAX(CAST(SUBSTRING(UFR_ID, CHARINDEX('_', UFR_ID) + 1, LEN(UFR_ID)) AS INT)) AS MaxID
                FROM (
                    SELECT UFR_ID FROM XFC_CMD_UFR  WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                    UNION ALL
                    SELECT UFR_ID FROM XFC_CMD_UFR WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                ) AS Combined"
			
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

#Region "Get Priority Categories"
 Private Function GetCategoryAndWeight(ByVal si As SessionInfo) As Object
            Try
                'Get the list of catagories
                Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
                Dim WFScenario As String = "RMW_Cycle_Config_Annual"
                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
                Dim REQTime As String = WFYear
            
                Dim sFundCenter As String =  args.CustomSubstVars("BL_CMD_UFR_FundsCenter")

                Dim priCatMembers As List(Of MemberInfo)
                Dim priFilter As String = "A#UFR_Priority_Cat_Weight.Children"
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

#End Region


' --- Call Functions ---

#Region "Function: dg_CMD_UFR_Create - New UFR"
#Region "Function: dg_CMD_UFR_Create - Create New UFR"
	Private Function dg_CMD_UFR_Create() As Object
Brapi.ErrorLog.LogMessage(si, "Inside Get Create New UFR")		
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
		Dim dt As New DataTable()
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
		Dim createrow As DataRow = dt.NewRow()
'		dt.Columns.Add("Title")
		dt.Columns.Add("Title", GetType(String))
'		dt.Columns.Add("FY")
		dt.Columns.Add("FY", GetType(Decimal))

		Dim Title As New XFDynamicGridColumnDefinition()
					Title.ColumnName = "Title"
					Title.IsFromTable = True
					Title.IsVisible = True
					Title.AllowUpdates = True
					Title.Width = "600"
					columnDefinitions.Add(Title)
					
		 Dim FY As New XFDynamicGridColumnDefinition()
					FY.ColumnName = "FY"
					FY.IsFromTable = True
					FY.IsVisible = True
					FY.Description = WFYear
					FY.DataFormatString = "N0"
					columnDefinitions.Add(FY)					
					
					dt.Rows.Add(createrow)
		
		 'Create the XFTable
		    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
			
		
		     'Send the result To the Interface component
		    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult
							
	End Function
#End Region

#Region "Function: Save_dg_CMD_UFR_Create - Save Create new UFR"
	Public Function Save_dg_CMD_UFR_Create() As Object	
Brapi.ErrorLog.LogMessage(si, "Inside Dyn Grid Save Line 263")
		Dim sEntity As String =  args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","")
		Dim UFR_ID As String = Me.Get_FC_REQ_ID(si,sEntity) 
		BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, "FormulateUFR", "UFR_ID", UFR_ID)
		Dim sU1APPNInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPPN","")
		Dim sU2Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateMDEP","")
		Dim sU3Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPEPT","")
		Dim sU4Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateDollarType","")
		Dim sU5Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateCType","")
		Dim sU6Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateObjectClass","")
		Dim sSAGInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateSAG","")
		
Brapi.ErrorLog.LogMessage(si, "Inside Dyn Grid Save Line 275")
#Region "Global Checks/Functions"
		'Clear the previous Guid
'		GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
		GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuidCopy(Me.si, UFR_ID, sEntity, sU1APPNInput, sU2Input, sU3Input, sU4Input, sU5Input, sU6Input, sSAGInput)

		
		'Reset the UFR Check Flag
'		GBL.GBL_Assembly.GBL_Helpers.ResetUFRState(si)
		
'		'Check Globals to verify xftv fields are input
'		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
'			Throw New XFException("UFR Is Invalid")
'		End If
		
		'Get New UFR/Requirement ID based on new GUID generated in Globals
'		Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
		
		Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuidCopy(Me.si, UFR_ID, sEntity, sU1APPNInput, sU2Input, sU3Input, sU4Input, sU5Input, sU6Input, sSAGInput)
		Brapi.ErrorLog.LogMessage(si, " Dyn Grid NewReqID = " & NewReqID)
'		Dim isValid As Boolean = GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si)
'		BRApi.ErrorLog.LogMessage(Me.si, "DynGrid: IsUFRValid = " & isValid.ToString())
		
		'Check Globals to verify xftv fields are input
'		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
'			Throw New XFException("UFR Spreadsheet input is invalid or incomplete. Please fix required fields and save again.")
'		End If
		
#End Region
		
		Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
	    If saveDataArgs Is Nothing Then
	        Return Nothing
	    End If

		'--- Get the edited rows --- 
	    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
	    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
	        Return Nothing
	    End If

#Region "Delete"
'		Dim xftv As TableView
'		Dim requiredColumns As List(Of String)
'		'Call globals to validate required Columns
'		GBL.GBL_Assembly.GBL_XFTV_Helpers.ValidateXFTVRequiredFields(Me.si, xftv, requiredColumns)

#End Region
	    '--- Get Workflow context details --- 
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
            ' --- Main Request Table (XFC_CMD_UFR) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
            Dim sqlMain As String = $"SELECT * FROM XFC_CMD_UFR WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            Dim sqlParams As SqlParameter() = New SqlParameter() {
                New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("ScenarioName")},
                New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("CMDName")},
                New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("TimeName")}
						}
              sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, sqlMain, sqlParams)
			  
            ' --- Details Table (XFC_CMD_UFR_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_UFR_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_UFR_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderdetail.Fill_XFC_CMD_UFR_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
			
			
            ' --- Staffing Input Table (XFC_CMD_Staffing Input) ---
            Dim dt_Staffing As New DataTable()
            Dim sqa3 As New SqlDataAdapter()
'            Dim sqaReaderStaffing As New GBL.GBL_Assembly.SQA_XFC_CMD_Staffing_Input(sqlConn)
            Dim sqaReaderStaffing As New SQA_XFC_CMD_Staffing_Input(sqlConn)
            Dim sqlStaffing As String = $"SELECT * FROM XFC_CMD_Staffing_Input WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
            sqaReaderStaffing.Fill_XFC_CMD_Staffing_Input_DT(sqa3, dt_Staffing, sqlStaffing, sqlParams)
			

            ' ************************************
            ' ************************************
			Dim EntDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
			Dim sEntityList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EntDimPk, "E#" & sEntity & ".Parents", True)
			Dim sReviewEntity As String = sEntityList(0).Member.Name
			Brapi.ErrorLog.LogMessage(si, "Review Entity = " & sReviewEntity)
			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
			Dim sREQWFStatus As String = entityLevel & "_Formulate_UFR"
#Region "Delete"			
			
			Brapi.ErrorLog.LogMessage(si, " Dyn Grid Svc NewReqID = " & NewReqID)
			Brapi.ErrorLog.LogMessage(si, "sU2Input = " & sU2Input)
#End Region			
			Dim sPEGInput As String = String.Empty
			Dim U2DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U2_MDEP")
			Dim sPEGList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, U2DimPk, "U2#" & sU2Input & ".Ancestors.Where(MemberDim = U2_PEG)", True)
			sPEGInput = sPEGList(0).Member.Name
			
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
					
					
					'--- Add UFR Details Table ---
                    Dim targetRowDetails As DataRow = dt_Details.NewRow()
					Dim APPN As String = sU1APPNInput
					Dim Dollar_Type As String = sU4Input
					Dim sFundCode As String = GBL.GBL_Assembly.GBL_Helpers.GetFundCode(si, APPN, Dollar_Type)
					targetRowDetails("CMD_UFR_Tracking_No") = NewReqID.XFConvertToGuid
					targetRowDetails("WFScenario_Name") = wfInfoDetails("ScenarioName")
					targetRowDetails("WFCMD_Name") = wfInfoDetails("CMDName")
					targetRowDetails("WFTime_Name") = wfInfoDetails("TimeName")
					targetRowDetails("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","")
'					Brapi.ErrorLog.LogMessage(si, "Entity = " & sEntity)
					targetRowDetails("Entity") = sEntity
					targetRowDetails("Unit_of_Measure") = "Funding"
					targetRowDetails("IC") = "None"
					targetRowDetails("Account") = "Req_Funding"
					targetRowDetails("Flow") = sREQWFStatus
					targetRowDetails("UD1") = sFundCode
					targetRowDetails("UD2") = sU2Input
					targetRowDetails("UD3") = sU3Input
					targetRowDetails("UD4") = sU4Input
					If String.IsNullOrWhiteSpace(sU5Input)
						targetRowDetails("UD5") = "None"
					Else 
						targetRowDetails("UD5") = sU5Input
					End If
					targetRowDetails("UD6") = sU6Input
					targetRowDetails("UD7") = "None"
					targetRowDetails("UD8") = "None"
                    Dim FY As Decimal = editedDataRow.ModifiedDataRow.Item("FY").ToString.XFConvertToDecimal
                    targetRowDetails("FY") = FY
					targetRowDetails("AllowUpdate") = "True"
					targetRowDetails("Create_Date") = DateTime.Now
					targetRowDetails("Create_User") = si.UserName
					targetRowDetails("Update_Date") = DateTime.Now
					targetRowDetails("Update_User") = si.UserName
					
'					--- Add UFR Header Table ---
					Dim targetRow As DataRow = dt.NewRow()
					targetRow("CMD_UFR_Tracking_No") = NewReqID
					targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
					targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
					targetRow("WFTime_Name") = wfInfoDetails("TimeName")
					targetRow("Entity") = args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","")
					targetRow("Review_Entity") = sReviewEntity
'					targetRowDetails("Entity") = sEntity
'					targetRow("UFR_ID") = Me.Get_FC_REQ_ID(si,sEntity)
					targetRow("UFR_ID") = UFR_ID
					targetRow("UFR_ID_Type") = "OMA"								
					targetRow("PEG") = sPEGInput
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
					targetRow("Command_UFR_Status") = sREQWFStatus
					
'					--- Add Staffing Input Table ---
					Dim targetRowStaffing As DataRow = dt_Staffing.NewRow()
'					targetRowStaffing("Tracking_No_ID") = NewReqID
					targetRowStaffing("Tracking_No_ID") = System.Guid.NewGuid().ToString("D")
					targetRowStaffing("WFScenario_Name") = wfInfoDetails("ScenarioName")
					targetRowStaffing("WFCMD_Name") = wfInfoDetails("CMDName")
					targetRowStaffing("WFTime_Name") = wfInfoDetails("TimeName")
					targetRowStaffing("Module") = "UFR"
					targetRowStaffing("Level") = entityLevel
					targetRowStaffing("UFR_ID") = UFR_ID
					
					'Add Rows to tables
					dt_Staffing.Rows.Add(targetRowStaffing)
                	dt_Details.Rows.Add(targetRowDetails)
					dt.Rows.Add(targetRow)
             
                Next
				
'				Throw New XFException("TEST: Dyn_Grid_Svc Update table was reached")

		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_UFR_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
		                sqaReaderStaffing.Update_XFC_CMD_Staffing_Input(dt_Staffing,sqa3)
						
		                End Using
		            End Using
					
            Return Nothing
        End Function	

#End Region
#End Region

#Region "Function: dg_CMD_UFR_REQManage - Manage"
	Private Function dg_CMD_UFR_REQManage() As Object
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
	
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 		
		Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Current Status"
			Status.AllowUpdates = False
			columnDefinitions.Add(Status)
			
'-------------------------------------------------------------------------------------------			
			Dim sEntity As String  = args.CustomSubstVars("BL_CMD_UFR_FundsCenter")
			globals.SetStringValue("Entity",sEntity)
			
		Dim NewStatus As New XFDynamicGridColumnDefinition()
			NewStatus.ColumnName = "New Status"
			NewStatus.IsFromTable = True
			NewStatus.IsVisible = True
			NewStatus.AllowUpdates = True
		'	NewStatus.CustomParameters = "StatusDT"
			'newstatus.DefaultValue = "L2_Formulate_PGM,L2_Validate_PGM"
			NewStatus.ParameterName = "BL_CMD_UFR_StatusChange"
			columnDefinitions.Add(NewStatus)
			
			
		Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
		Dim UFR_ID As New XFDynamicGridColumnDefinition()
			UFR_ID.ColumnName = "UFR_ID"
			UFR_ID.IsFromTable = True
			UFR_ID.IsVisible = True
			UFR_ID.AllowUpdates = False
			UFR_ID.Description = "UFR ID"
			columnDefinitions.Add(UFR_ID)
			
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

	    Dim FY As New XFDynamicGridColumnDefinition()
			FY.ColumnName = "FY"
			FY.IsFromTable = True
			FY.IsVisible = True
			FY.Description = WFYear
			FY.AllowUpdates = False
			columnDefinitions.Add(FY)
										
		' Get the data you want To put In the grid
		
	'	Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
		
		Dim objdt As New DataTable 
		Dim dargs As New DashboardDataSetArgs
		dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
		dargs.DataSetName = "REQListByEntityAndStatus"
		dargs.NameValuePairs.XFSetValue("Entity", sEntity)
		dargs.NameValuePairs.XFSetValue("Dashboard", "UFRGridList")
		
	'Update si, globals, api, dargs in the Data Set Rule to compile
		objdt = BR_CMD_UFRDataSet.Main(si, globals, api, dargs)
		objdt.Columns.Add("New Status",GetType(String))
						
					  				
'					     'Create the XFTable
	    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
		
	
	     'Send the result To the Interface component
	    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
	        
	    Return taskResult
	End Function	
		
#End Region

#Region "Function: Save_dg_CMD_UFR_REQManage - Save Manage Status"		
Public Function Save_dg_CMD_UFR_REQManage() As Object
	Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
	
    If saveDataArgs Is Nothing Then
        Return Nothing
    End If
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
	Dim WFInfoDetails As New Dictionary(Of String, String)()
		Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
		Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
		Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
		WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
		WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
		WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
		WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
					
	    Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		    sqlConn.Open()

    ' ************************************
    ' *** Fetch Data for BOTH tables *****
    ' ************************************
    ' --- Main Request Table (XFC_CMD_UFR) ---
    Dim dt As New DataTable()
    Dim sqa As New SqlDataAdapter()
    Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
    Dim sqlMain As String = $"SELECT * FROM XFC_CMD_UFR WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
    Dim sqlParams As SqlParameter() = New SqlParameter() {
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
							}
		sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, sqlMain, sqlParams)
				  
	
    ' --- Details Table (XFC_CMD_UFR_Details) ---
    Dim dt_Details As New DataTable()
	Dim sqa2 As New SqlDataAdapter()
    Dim sqaReaderdetail As New SQA_XFC_CMD_UFR_Details(sqlConn)
    Dim sqlDetail As String = $"SELECT * FROM XFC_CMD_UFR_Details WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
    sqaReaderdetail.Fill_XFC_CMD_UFR_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
	
    ' ************************************
     ' --- Details Audit Table (XFC_CMD_UFR_Details_Audit) ---
    Dim dt_Details_Audit As New DataTable()
	Dim sqa3 As New SqlDataAdapter()
    Dim sqaReaderdetailAudit As New SQA_XFC_CMD_UFR_Details_Audit(sqlConn)
    Dim sqlAudit As String = $"SELECT * FROM XFC_CMD_UFR_Details_Audit WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
    sqaReaderdetailAudit.Fill_XFC_CMD_UFR_Details_Audit_DT(sqa3, dt_Details_Audit, sqlAudit, sqlParams)
				
	
	' ************************************
	Dim sEntity As String = ""
	Dim old_status As String = ""
	Dim New_Status As String = ""
	Dim tm As String = wfInfoDetails("TimeName")
	Dim sScenario As String = wfInfoDetails("ScenarioName")
	Dim workspaceid As guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False,"70 CMD UFR")
	Dim targetRow As DataRow 
	Dim req_ID_Val As Guid
	Dim StatustoPass As String = ""
	'Brapi.ErrorLog.LogMessage(si, "editedDataRows = " & editedDataRows.Count)		
	For Each editedDataRow As XFEditedDataRow In editedDataRows
					
		Dim isInsert As Boolean = "false"		
		req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_UFR_Tracking_No")
			
		targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'").FirstOrDefault()
		old_Status = targetRow.Item("Status")
		
		New_Status = editedDataRow.ModifiedDataRow.Item("New Status")
		
		If StatustoPass.XFEqualsIgnoreCase("") Then 
		statustopass = old_Status & "|" & New_Status
		Else  
		statustopass += "|" & old_Status & "|" & New_Status 
		End If 

		sEntity = editedDataRow.ModifiedDataRow.Item("Funds Center")
		targetRow("Update_Date") = DateTime.Now	
		targetRow("Update_User") = si.UserName
		targetRow("Status") = New_Status
		
		Dim targetRowFunding As DataRow()
		targetRowFunding = dt_Details.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'")
					For Each dr As DataRow In targetRowFunding
							dr("Flow") = New_Status
							dr("Update_Date") = DateTime.Now
		                	dr("Update_User") = si.UserName
						Next
			'Brapi.ErrorLog.LogMessage(si, "Row Count " &  dt_Details.Rows.Count )
						Dim targetRowAudit As DataRow()
						targetRowAudit = dt_Details_Audit.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'")
													
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
												newrow("CMD_UFR_Tracking_No") = targetRow("CMD_UFR_Tracking_No")
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
	
			    	Next
	
	                ' Persist changes back to the DB using the configured adapter
	               
	               sqaReaderdetail.Update_XFC_CMD_UFR_Details(dt_Details,sqa2)
	               sqaReader.Update_XFC_CMD_UFR(dt,sqa)
				   sqaReaderdetailAudit.Update_XFC_CMD_UFR_Details_Audit(dt_Details_Audit,sqa3)
	
				   'Writing to cube
	
							Dim customSubstVars As New Dictionary(Of String, String) 
							globals.SetStringValue("FundsCenterStatusUpdates - " & sEntity, statustopass)
							customSubstVars.Add("EntList","E#" & sEntity)
							customSubstVars.Add("WFScen",sScenario)
							Dim currentYear As Integer = Convert.ToInt32(tm)
							customSubstVars.Add("WFTime",$"T#{currentYear.ToString()}")
							BRApi.Utilities.ExecuteDataMgmtSequence(si, workspaceID, "CMD_UFR_Proc_Status_Updates", customSubstVars)
					'Next	
			                End Using
			            End Using
	
	            Return Nothing
		
End Function		
#End Region

#Region "Function: dg_CMD_UFR_UFRList - UFR List Based on Status"
	Private Function dg_CMD_UFR_UFRList() As Object
		
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
		
		
		Dim UFR_ID As New XFDynamicGridColumnDefinition()
				UFR_ID.ColumnName = "UFR_ID"
				UFR_ID.IsFromTable = True
				UFR_ID.IsVisible = True
				UFR_ID.AllowUpdates = False
				UFR_ID.Description = "UFR ID"
				columnDefinitions.Add(UFR_ID)	
		
		Dim Entity As New XFDynamicGridColumnDefinition()
				Entity.ColumnName = "Funds Center"
				Entity.IsFromTable = True
				Entity.IsVisible = True
				Entity.AllowUpdates = False
				Entity.Description = "Funds Center"
				columnDefinitions.Add(Entity)
		
		Dim Title As New XFDynamicGridColumnDefinition()
				Title.ColumnName = "Title"
				Title.IsFromTable = True
				Title.IsVisible = True
				Title.AllowUpdates = False
				columnDefinitions.Add(Title)
		
		Dim CMD_UFR_Tracking_No As New XFDynamicGridColumnDefinition()
				CMD_UFR_Tracking_No.ColumnName = "CMD_UFR_Tracking_No"
				CMD_UFR_Tracking_No.IsFromTable = True
				CMD_UFR_Tracking_No.IsVisible = False
				CMD_UFR_Tracking_No.AllowUpdates = False
				CMD_UFR_Tracking_No.Description = "Tracking No"
				columnDefinitions.Add(CMD_UFR_Tracking_No)
				
		Dim Study_Category As New XFDynamicGridColumnDefinition()
				Study_Category.ColumnName = "Study_Category"
				Study_Category.IsFromTable = True
				Study_Category.IsVisible = True
				Study_Category.AllowUpdates = False
				Study_Category.Description = "Study Category"
				columnDefinitions.Add(Study_Category)	
				
		Dim UFR_Driver As New XFDynamicGridColumnDefinition()
				UFR_Driver.ColumnName = "UFR_Driver"
				UFR_Driver.IsFromTable = True
				UFR_Driver.IsVisible = True
				UFR_Driver.AllowUpdates = False
				UFR_Driver.Description = "UFR Driver"
				columnDefinitions.Add(UFR_Driver)	
				
		Dim PEG As New XFDynamicGridColumnDefinition()
				PEG.ColumnName = "PEG"
				PEG.IsFromTable = True
				PEG.IsVisible = True
				PEG.AllowUpdates = False
				PEG.Description = "PEG"
				columnDefinitions.Add(PEG)	
				
				
			Dim Command_UFR_Priority As New XFDynamicGridColumnDefinition()
				Command_UFR_Priority.ColumnName = "Command_UFR_Priority"
				Command_UFR_Priority.IsFromTable = True
				Command_UFR_Priority.IsVisible = False
				Command_UFR_Priority.AllowUpdates = False
				Command_UFR_Priority.Description = "Command UFR Priority"
				columnDefinitions.Add(Command_UFR_Priority)	

		Dim Date_Decision_Needed_By As New XFDynamicGridColumnDefinition()
				Date_Decision_Needed_By.ColumnName = "Date_Decision_Needed_By"
				Date_Decision_Needed_By.IsFromTable = True
				Date_Decision_Needed_By.IsVisible = True
				Date_Decision_Needed_By.AllowUpdates = False
				Date_Decision_Needed_By.Description = "Date Decision Needed By"
				columnDefinitions.Add(Date_Decision_Needed_By)	
				
		Dim Review_Staff As New XFDynamicGridColumnDefinition()
				Review_Staff.ColumnName = "Review_Staff"
				Review_Staff.IsFromTable = True
				Review_Staff.IsVisible = True
				Review_Staff.AllowUpdates = False
				Review_Staff.Description = "Review Staff"
				columnDefinitions.Add(Review_Staff)	
				
		Dim ROC As New XFDynamicGridColumnDefinition()
				ROC.ColumnName = "ROC"
				ROC.IsFromTable = True
				ROC.IsVisible = True
				ROC.AllowUpdates = False
				ROC.Description = "ROC"
				columnDefinitions.Add(ROC)	
				
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
				DollarType.Description = "DollarType"
				DollarType.AllowUpdates = False
				columnDefinitions.Add(DollarType)
				
		Dim ObjectClass As New XFDynamicGridColumnDefinition()
				ObjectClass.ColumnName = "Object Class"
				ObjectClass.IsFromTable = True
				ObjectClass.IsVisible = True
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
								
		Dim Status As New XFDynamicGridColumnDefinition()
				Status.ColumnName = "Flow"
				Status.IsFromTable = True
				Status.IsVisible = True
				Status.Description = "Current Status"
				Status.AllowUpdates = False
				columnDefinitions.Add(Status)
				
	    Dim FY As New XFDynamicGridColumnDefinition()
				FY.ColumnName = "FY"
				FY.IsFromTable = True
				FY.IsVisible = True
				FY.Description = "Request Amount"
				FY.AllowUpdates = False
				columnDefinitions.Add(FY)    
			
	
							
		' Get the data you want To put In the grid
						
		Dim sEntity As String  = args.CustomSubstVars("BL_CMD_UFR_FundsCenter")
		
		Dim objdt As New DataTable 
		Dim dargs As New DashboardDataSetArgs
		dargs.FunctionType = DashboardDataSetFunctionType.GetDataSet
		dargs.DataSetName = "REQListByEntityAndStatus"
		dargs.NameValuePairs.XFSetValue("Entity", sEntity)
		dargs.NameValuePairs.XFSetValue("Dashboard", "UFRGridList")
		objdt = BR_CMD_UFRDataSet.Main(si, globals, api, dargs)
	  				
'					     'Create the XFTable
	    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
		
	
	     'Send the result To the Interface component
	    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
	        
	    Return taskResult
	End Function	
		
#End Region

#Region "Function: dg_CMD_UFR_Prioritize_REQ - Prioritize UFR Dyn Grid"
#Region "Function: dg_CMD_UFR_Prioritize_REQ - Get Prioritize"
	Private Function dg_CMD_UFR_Prioritize_REQ() As Object
	
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
	
		Dim CMD_UFR_Tracking_No As New XFDynamicGridColumnDefinition()
			CMD_UFR_Tracking_No.ColumnName = "CMD_UFR_Tracking_No"
			CMD_UFR_Tracking_No.IsFromTable = True
			CMD_UFR_Tracking_No.IsVisible = False
			CMD_UFR_Tracking_No.AllowUpdates = False
			columnDefinitions.Add(CMD_UFR_Tracking_No)
				
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
					
		Dim Title As New XFDynamicGridColumnDefinition()
			Title.ColumnName = "Title"
			Title.IsFromTable = True
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
		Dim UFR_ID As New XFDynamicGridColumnDefinition()
			UFR_ID.ColumnName = "UFR_ID"
			UFR_ID.IsFromTable = True
			UFR_ID.IsVisible = True
			UFR_ID.AllowUpdates = False
			columnDefinitions.Add(UFR_ID)
			
			
		Dim Description As New XFDynamicGridColumnDefinition()
			Description.ColumnName = "Description"
			Description.IsFromTable = True
			Description.IsVisible = True
			Description.AllowUpdates = False
			Description.Description = "Description"
			columnDefinitions.Add(Description)
			
		Dim MustFund As New XFDynamicGridColumnDefinition()
			MustFund.ColumnName = "MustFund"
			MustFund.IsFromTable = True
			MustFund.IsVisible = True
			MustFund.AllowUpdates = False
			MustFund.Description = "Must Fund"
			columnDefinitions.Add(MustFund)
				
		Dim FY As New XFDynamicGridColumnDefinition()
			FY.ColumnName = "FY"
			FY.IsFromTable = True
			FY.IsVisible = True
			FY.Description = "Requested Amount"
			FY.AllowUpdates = False
			columnDefinitions.Add(FY)
							
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
						
		Dim sEntity As String  = args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","NA")
		If String.IsNullOrWhiteSpace(sEntity) Then
			Return Nothing
		Else
			Dim sFundcenter As String = ""
			'Remove _General to get the parent Entity
			Dim wfCube As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName
			Dim entityPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfCube)
     		Dim nBaseID As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Entity.Id, sEntity)
			Dim Haschildren As Boolean = BRApi.Finance.Members.HasChildren(si,entityPk,nBaseID)
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
			Dim sREQWFStatus As String = entityLevel & "_Prioritize_UFR"
            Dim sql As String =$"SELECT 
									Req.CMD_UFR_Tracking_No, 
									Req.WFScenario_Name, Req.Title, Req.UFR_ID, Req.Description, Req.MustFund,
									Req.WFCMD_Name, Req.WFTime_Name, Req.Entity,Dtl.UD1,Dtl.UD2,Dtl.UD3,Dtl.UD4,Pri.Cat_1_Score,
									Pri.Cat_2_Score,Pri.Cat_3_Score,Pri.Cat_4_Score,Pri.Cat_5_Score,Pri.Cat_6_Score,Pri.Cat_7_Score,
									Pri.Cat_8_Score,Pri.Cat_9_Score,Pri.Cat_10_Score,Pri.Cat_11_Score,Pri.Cat_12_Score,Pri.Cat_13_Score,			
									Pri.Cat_14_Score,Pri.Cat_15_Score,Pri.Score,Pri.Weighted_Score,Pri.Auto_Rank,Pri.Rank_Override, 
									Pri.Review_Entity,
									FORMAT(Dtl.FY, 'N1') As FY
								FROM XFC_CMD_UFR_Details AS Dtl
								LEFT JOIN XFC_CMD_UFR_Priority AS Pri
									ON Pri.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
									AND Pri.WFScenario_Name = Dtl.WFScenario_Name
									AND Pri.WFCMD_Name = Dtl.WFCMD_Name
									AND Pri.WFTime_Name = Dtl.WFTime_Name
									AND Pri.Entity = Dtl.Entity
									AND Pri.Review_Entity = '{sEntity}'
								LEFT JOIN XFC_CMD_UFR AS Req
									ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
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

'Brapi.ErrorLog.LogMessage(si, "SQL" & sql )  				
			     'Create the XFTable
			    Dim xfTable As New xfDataTable(si,dtPri,Nothing,1000)
			
			     'Send the result To the Interface component
			    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
			        
			    Return taskResult
			End If
		End Function
#End Region

#Region "Function: Save_dg_CMD_UFR_Prioritize_REQ - Save Prioritize"		
	Public Function Save_dg_CMD_UFR_Prioritize_REQ() As Object
		
		Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
	    If saveDataArgs Is Nothing Then
	        Return Nothing
	    End If
		
	    ' Get the edited rows
	    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
	    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
	        Return Nothing
	    End If
		
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
			        ' --- Main Request Table (XFC_CMD_UFR_REQ) ---
			        Dim dt As New DataTable()
			        Dim sqa As New SqlDataAdapter()
			        Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
			        Dim sqlMain As String = $"SELECT * FROM XFC_CMD_UFR WHERE WFScenario_Name = @WFScenario_Name AND WFCMD_Name = @WFCMD_Name AND WFTime_Name = @WFTime_Name"
			        Dim sqlParams As SqlParameter() = New SqlParameter() {
			            New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
			            New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
			            New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}
								}
			          sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, sqlMain, sqlParams)
						  
			        ' --- Details Table (XFC_CMD_UFR_Details) ---
			        Dim dt_Details As New DataTable()
			        Dim sqa2 As New SqlDataAdapter()
			        Dim sqaReaderdetail As New SQA_XFC_CMD_UFR_Details(sqlConn)
			        Dim sqlDetail As String = $"SELECT * 
												FROM XFC_CMD_UFR_Details 
												WHERE WFScenario_Name = @WFScenario_Name 
												AND WFCMD_Name = @WFCMD_Name 
												AND WFTime_Name = @WFTime_Name"
			        sqaReaderdetail.Fill_XFC_CMD_UFR_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
			
					' --- Priority Table (XFC_CMD_UFR_Priority) ---
			        Dim dt_Priority As New DataTable()
			        Dim sqa3 As New SqlDataAdapter()
			        Dim sqaReaderPriority As New SQA_XFC_CMD_UFR_Priority(sqlConn)
			        Dim sqlPriority As String = $"SELECT * 
												FROM XFC_CMD_UFR_Priority 
												WHERE WFScenario_Name = @WFScenario_Name 
												AND WFCMD_Name = @WFCMD_Name 
												AND WFTime_Name = @WFTime_Name"
			        sqaReaderPriority.Fill_XFC_CMD_UFR_Priority_DT(si, sqa3, dt_Priority, sqlPriority, sqlParams)
			          
					For Each editedDataRow As XFEditedDataRow In editedDataRows
						Dim sEntity As String  = args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","NA")	
					    Dim targetRow As DataRow 											
						Dim req_ID_Val As Guid
						req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_UFR_Tracking_No")
						Dim REQIDrow As DataRow =  dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'").FirstOrDefault()
						Dim isInsert As Boolean = False		
						targetRow = dt_Priority.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}' And Review_Entity = '{sEntity}'").FirstOrDefault()
						
						If targetRow Is Nothing Then
							isInsert = True
							targetRow = dt_Priority.NewRow() 
							
							Dim req_ID As Guid = editedDataRow.ModifiedDataRow.Item("CMD_UFR_Tracking_No")
							Dim mainRow As DataRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID}'").FirstOrDefault()
						
							If mainRow IsNot Nothing Then
								targetRow("CMD_UFR_Tracking_No") = mainRow("CMD_UFR_Tracking_No")
							End If
			    
			    
						Else
			   
						    isInsert = False
							targetRow = dt_Priority.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'And Review_Entity = '{sEntity}'").FirstOrDefault()
					    End If
						BRAPi.ErrorLog.LogMessage(si, "Line 1302")
						targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
						targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
						targetRow("WFTime_Name") = wfInfoDetails("TimeName")
						targetRow("Entity") = editedDataRow.ModifiedDataRow.Item("Entity").ToString()
						targetRow("Review_Entity") = sEntity
						targetRow("UFR_ID") = REQIDrow.Item("UFR_ID")
						
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
			        sqaReaderPriority.Update_XFC_CMD_UFR_Priority(si,dt_Priority,sqa3)
	                End Using
	            End Using
	    Return Nothing
	End Function		
#End Region
#End Region

#Region "Function: dg_CMD_UFR_REQAmtsReview - Review/Adjust UFR Detail"
#Region "Function: dg_CMD_UFR_REQAmtsReview - Get Review/Adjust UFR Detail"
	Private Function dg_CMD_UFR_REQAmtsReview() As Object
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
		
		Dim Title As New XFDynamicGridColumnDefinition()
					Title.ColumnName = "Title"
					Title.IsFromTable = True
					Title.IsVisible = True
					Title.AllowUpdates = False
					columnDefinitions.Add(Title)
					
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
					
		Dim Dollar_Type As New XFDynamicGridColumnDefinition()
					Dollar_Type.ColumnName = "Dollar_Type"
					Dollar_Type.IsFromTable = True
					Dollar_Type.IsVisible = True
					Dollar_Type.AllowUpdates = False
					columnDefinitions.Add(Dollar_Type)
					
		Dim Obj_Class As New XFDynamicGridColumnDefinition()
					Obj_Class.ColumnName = "Obj_Class"
					Obj_Class.IsFromTable = True
					Obj_Class.IsVisible = True
					Obj_Class.AllowUpdates = False
					columnDefinitions.Add(Obj_Class)

		Dim sCType As New XFDynamicGridColumnDefinition()
					sCType.ColumnName = "CType"
					sCType.IsFromTable = True
					sCType.IsVisible = True
					sCType.AllowUpdates = False
					columnDefinitions.Add(sCType)
					
		 Dim FY As New XFDynamicGridColumnDefinition()
					FY.ColumnName = "FY"
					FY.IsFromTable = True
					FY.IsVisible = True
					FY.AllowUpdates = True
					FY.Description = "Request Amount"
					FY.DataFormatString = "N0"
					columnDefinitions.Add(FY)	
					
					
		'---IsVisible on Approve Dashboard only	---
		 Dim wfProfileFullName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
		 Dim bResult As Boolean
		 If wfProfileFullName.XFContainsIgnoreCase("Approve")
			 bResult = True
		 Else
			 bResult = False
		 End If
		 Dim Fund_Amount As New XFDynamicGridColumnDefinition()
					Fund_Amount.ColumnName = "Fund_Amount"
					Fund_Amount.IsFromTable = True
					Fund_Amount.IsVisible = bResult
					Fund_Amount.AllowUpdates = True
					Fund_Amount.Description = "Fund Amount"
					Fund_Amount.DataFormatString = "N0"
					columnDefinitions.Add(Fund_Amount)	
					
		 Dim Fund_Status As New XFDynamicGridColumnDefinition()
					Fund_Status.ColumnName = "Fund_Status"
					Fund_Status.IsFromTable = True
					Fund_Status.IsVisible = bResult
					Fund_Status.AllowUpdates = True
					Fund_Status.Description = "Fund Status"
					Fund_Status.DataFormatString = "N0"
					Fund_Status.ParameterName = "DL_CMD_UFR_FundStatus"
					columnDefinitions.Add(Fund_Status)	
					
		 Dim Fund_Source As New XFDynamicGridColumnDefinition()
					Fund_Source.ColumnName = "Fund_Source"
					Fund_Source.IsFromTable = True
					Fund_Source.IsVisible = bResult
					Fund_Source.AllowUpdates = True
					Fund_Source.Description = "Fund Source"
					Fund_Source.DataFormatString = "N0"
					columnDefinitions.Add(Fund_Source)	
					
					
					
					
					
					
		 '--- Hidden Columns ---		
		 Dim UFR_Tracking_No As New XFDynamicGridColumnDefinition()
					UFR_Tracking_No.ColumnName = "CMD_UFR_Tracking_No"
					UFR_Tracking_No.IsFromTable = True
					UFR_Tracking_No.IsVisible = False
					columnDefinitions.Add(UFR_Tracking_No)	
					
		 Dim UFR_ID As New XFDynamicGridColumnDefinition()
					UFR_ID.ColumnName = "UFR_ID"
					UFR_ID.IsFromTable = True
					UFR_ID.IsVisible = False
					columnDefinitions.Add(UFR_ID)					
							
'		Dim UFRID As String = args.CustomSubstVars("IV_CMD_UFR_REQ_IDs")
		Dim UFRID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
		Brapi.ErrorLog.LogMessage(si, "Dyn Grid Svc REQ ID = " & UFRID)
		Dim dtAdj As New DataTable()
		Dim sql As String = $"SELECT 
								Req.CMD_UFR_Tracking_No,
								Req.Title As [Title],
								Req.UFR_ID,
								Req.APPN,
								Req.MDEP,
								Req.APE9,
								Req.Dollar_Type,
								Req.Obj_Class,
								Req.CType,
								FORMAT(Dtl.FY, 'N0') As FY,
								Req.Fund_Amount,
								Req.Fund_Status,
								--FORMAT(Dtl.FY, 'N0') As FY
								Req.Fund_Source
							FROM XFC_CMD_UFR AS Req
							LEFT JOIN XFC_CMD_UFR_Details AS Dtl
								ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
							WHERE Req.UFR_ID = '{UFRID}'"
				
		Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            dtAdj = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
        End Using
	  				
'					     'Create the XFTable
	    Dim xfTable As New xfDataTable(si,dtAdj,Nothing,1000)
		
	
	     'Send the result To the Interface component
	    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult
							
	End Function
#End Region

#Region "Function: Save_dg_CMD_UFR_REQAmtsReview - Save Review/Adjust UFR Detail"
	Public Function Save_dg_CMD_UFR_REQAmtsReview() As Object	

#Region "Global Checks/Functions"
		'Clear the previous Guid
'		GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
		
		'Reset the UFR Check Flag
'		GBL.GBL_Assembly.GBL_Helpers.ResetUFRState(si)
		
'		'Check Globals to verify xftv fields are input
'		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
'			Throw New XFException("UFR Is Invalid")
'		End If
		
		'Get New UFR/Requirement ID based on new GUID generated in Globals
'		Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
		
'		Dim isValid As Boolean = GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si)
'		BRApi.ErrorLog.LogMessage(Me.si, "DynGrid: IsUFRValid = " & isValid.ToString())
		
'		'Check Globals to verify xftv fields are input
'		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
'			Throw New XFException("UFR Spreadsheet input is invalid or incomplete. Please fix required fields and save again.")
'		End If
		
#End Region
		
		'Get Selected ReqID
'		Dim NewReqID As String = args.NameValuePairs("UFR")
		Dim UFRID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
'		Dim Tracking_No As String = args.CustomSubstVars("IV_CMD_UFR_REQ_IDs")
'		Brapi.ErrorLog.LogMessage(si, "Tracking No = " & Tracking_No)
		Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
	    If saveDataArgs Is Nothing Then
	        Return Nothing
	    End If
		
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
            ' --- Main Request Table (XFC_CMD_UFR) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
            Dim sqlMain As String = $"SELECT * 
										FROM XFC_CMD_UFR 
										WHERE WFScenario_Name = @WFScenario_Name 
										AND WFCMD_Name = @WFCMD_Name 
										AND WFTime_Name = @WFTime_Name
										AND CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No"
			
            Dim sqlParams As SqlParameter() = New SqlParameter() {
            New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("ScenarioName")},
            New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("CMDName")},
            New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("TimeName")},
            New SqlParameter("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier) With {.Value = editedDataRows.Item(0).ModifiedDataRow("CMD_UFR_Tracking_No")}
						}
						
             sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, sqlMain, sqlParams)
			 
			  
            ' --- Details Table (XFC_CMD_UFR_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_UFR_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * 
										FROM XFC_CMD_UFR_Details 
										WHERE WFScenario_Name = @WFScenario_Name 
										AND WFCMD_Name = @WFCMD_Name 
										AND WFTime_Name = @WFTime_Name
										AND CMD_UFR_Tracking_No = @CMD_UFR_Tracking_No"
			
            sqaReaderdetail.Fill_XFC_CMD_UFR_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
			
			
'            ' --- Details Table (XFC_CMD_UFR_Details_Audit) ---
			Dim sqa_xfc_cmd_ufr_details_audit = New SQA_XFC_CMD_UFR_DETAILS_AUDIT(sqlConn)
			Dim SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT = New DataTable()

			Dim SQL_Audit As String = $"SELECT * 
								FROM XFC_CMD_UFR_Details_Audit
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_UFR_Tracking_No  = @CMD_UFR_Tracking_No"
			
			sqa_xfc_cmd_ufr_details_audit.Fill_XFC_CMD_UFR_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT, SQL_Audit, sqlParams)

            ' ************************************
            ' ************************************
#Region "Delete"			
'			Dim sU1APPNInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPPN","")
'			Dim sU2Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateMDEP","")
'			Dim sU3Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPEPT","")
'			Dim sU4Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateDollarType","")
'			Dim sU5Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateCType","")
'			Dim sU6Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateObjectClass","")

'			Dim sEntity As String =  args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","")
'			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
'			Dim sREQWFStatus As String = entityLevel & "_Formulate_UFR"
			
'			Brapi.ErrorLog.LogMessage(si, " Dyn Grid Svc NewReqID = " & NewReqID)
#End Region	

			For Each editedDataRow As XFEditedDataRow In editedDataRows						
						
			    Dim targetRow As DataRow
				Dim req_ID_Val As Guid
				Dim Fund_Amount As Decimal =  editedDataRow.ModifiedDataRow.Item("Fund_Amount")		 
				Dim Fund_Status As String = editedDataRow.ModifiedDataRow.Item("Fund_Status")
				Dim Fund_Source As String = editedDataRow.ModifiedDataRow.Item("Fund_Source")
				
				req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_UFR_Tracking_No")
				
				'--- Update Header Table ---
				targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'").FirstOrDefault()
				targetRow("Fund_Amount") = Fund_Amount
				targetRow("Fund_Status") = Fund_Status
				targetRow("Fund_Source") = Fund_Source
				targetRow("Update_Date") = DateTime.Now
				targetRow("Update_User") = si.UserName																																																																																				

				Dim targetRowFunding As DataRow
				targetRowFunding = dt_Details.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
				
				If targetRowFunding IsNot Nothing Then
				
					Dim FY As Decimal = editedDataRow.ModifiedDataRow.Item("FY")

					'Added Audit update before funding line updates are written to the table			
					If SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.Rows.Count > 0 Then
						Dim drow As DataRow
						drow = SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
						drow("Orig_FY") = targetRowFunding("FY")
						drow("Updated_FY") = FY
					Else
						Dim newrow As datarow = SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.NewRow()
						newrow("CMD_UFR_Tracking_No") = targetRow("CMD_UFR_Tracking_No")
						newrow("WFScenario_Name") = targetRow("WFScenario_Name")
						newrow("WFCMD_Name") = targetRow("WFCMD_Name")
						newrow("WFTime_Name") = targetRow("WFTime_Name")
						newrow("Entity") = targetRow("Entity")
						newrow("Account") = "Req_Funding"
						newrow("Orig_IC") = "None"
						newrow("Updated_IC") = "None"
						newrow("Orig_Flow") =  targetRow("Command_UFR_Status")
						newrow("Updated_Flow") = targetRow("Command_UFR_Status")
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
						End If
						
						newrow("Orig_UD6") = targetRow("Obj_Class")
						newrow("Updated_UD6") = targetRow("Obj_Class")
						newrow("Orig_UD7") = "None"
						newrow("Updated_UD7") = "None"
						newrow("Orig_UD8") = "None"
						newrow("Updated_UD8") = "None"
						newrow("Create_Date") = DateTime.Now
						newrow("Create_User") = si.UserName
						newrow("Orig_FY") = targetRowFunding("FY")
						newrow("Updated_FY") = FY
								
						SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.rows.add(newrow)	
										
					End If		
						targetRowFunding("FY") = FY
						targetRowFunding("Update_Date") = DateTime.Now
                        targetRowFunding("Update_User") = si.UserName  
						
				End If      
			Next
				
'				Throw New XFException("TEST: Dyn_Grid_Svc Update table was reached")
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_UFR_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
						sqa_xfc_cmd_ufr_details_audit.Update_XFC_CMD_UFR_DETAILS_AUDIT(SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT, sqa)
						
		                End Using
		            End Using
					
            Return Nothing
        End Function	

#End Region
#End Region

#Region "Function: dg_CMD_UFR_CopySPLNREQ - Review/Copy SPLN REQs"
#Region "Function: dg_CMD_UFR_CopySPLNREQ - Get Review/Copy SPLN REQs"
	Private Function dg_CMD_UFR_CopySPLNREQ() As Object
		Dim WfYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)
		Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition) 	
'		Dim REQID As String =  args.CustomSubstVars.XFGetValue("BL_CMD_UFR_REQIDTitleList","")
		Dim REQID As String =  args.CustomSubstVars.XFGetValue("CMD_UFR_bi_SelectedREQID","")
		Brapi.ErrorLog.LogMessage(si, $"REQID = " & REQID)
		
		Dim Title As New XFDynamicGridColumnDefinition()
					Title.ColumnName = "Title"
					Title.IsFromTable = True
					Title.IsVisible = True
					Title.AllowUpdates = False
					columnDefinitions.Add(Title)
					
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
					
		Dim Dollar_Type As New XFDynamicGridColumnDefinition()
					Dollar_Type.ColumnName = "Dollar_Type"
					Dollar_Type.IsFromTable = True
					Dollar_Type.IsVisible = True
					Dollar_Type.AllowUpdates = False
					columnDefinitions.Add(Dollar_Type)
					
		Dim Obj_Class As New XFDynamicGridColumnDefinition()
					Obj_Class.ColumnName = "Obj_Class"
					Obj_Class.IsFromTable = True
					Obj_Class.IsVisible = True
					Obj_Class.AllowUpdates = False
					columnDefinitions.Add(Obj_Class)

		Dim sCType As New XFDynamicGridColumnDefinition()
					sCType.ColumnName = "CType"
					sCType.IsFromTable = True
					sCType.IsVisible = True
					sCType.AllowUpdates = False
					columnDefinitions.Add(sCType)
					
		 Dim FY As New XFDynamicGridColumnDefinition()
					FY.ColumnName = "FY"
					FY.IsFromTable = True
					FY.IsVisible = True
					FY.AllowUpdates = True
					FY.Description = "Request Amount"
					FY.DataFormatString = "N0"
					columnDefinitions.Add(FY)	
					
					
		 '--- Hidden Columns ---		
		 Dim CMD_UFR_Tracking_No As New XFDynamicGridColumnDefinition()
					CMD_UFR_Tracking_No.ColumnName = "CMD_UFR_Tracking_No"
					CMD_UFR_Tracking_No.IsFromTable = True
					CMD_UFR_Tracking_No.IsVisible = False
					columnDefinitions.Add(CMD_UFR_Tracking_No)	
					
		 Dim UFR_ID As New XFDynamicGridColumnDefinition()
					UFR_ID.ColumnName = "UFR_ID"
					UFR_ID.IsFromTable = True
					UFR_ID.IsVisible = True
					columnDefinitions.Add(UFR_ID)					
							
'		Dim UFRID As String = args.CustomSubstVars("IV_CMD_UFR_REQ_IDs")
'		Dim UFRID As String = BRApi.Utilities.GetWorkspaceSessionSetting(si,si.UserName,"REQRetrieve","ReqIDs","")
'		Brapi.ErrorLog.LogMessage(si, "Dyn Grid Svc REQ ID = " & UFRID)
		Dim dtAdj As New DataTable()
		Dim sql As String = $"SELECT 
								Req.CMD_UFR_Tracking_No,
								Req.UFR_ID,
								Req.Title As [Title],
								Req.APPN,
								Req.MDEP,
								Req.APE9,
								Req.Dollar_Type,
								Req.Obj_Class,
								Req.CType,
								Req.REQ_Link_ID,
								Dtl.FY 
							FROM XFC_CMD_UFR AS Req
							LEFT JOIN XFC_CMD_UFR_Details AS Dtl
							ON Req.CMD_UFR_Tracking_No = Dtl.CMD_UFR_Tracking_No
							WHERE Req.REQ_Link_ID = '{REQID}'
							"
				
		Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
            dtAdj = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
        End Using
	  				
'					     'Create the XFTable
	    Dim xfTable As New xfDataTable(si,dtAdj,Nothing,1000)
		
	
	     'Send the result To the Interface component
	    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult
							
	End Function
#End Region

#Region "Function: Save_dg_CMD_UFR_CopySPLNREQ - Save Copy of SPLN REQ"
	Public Function Save_dg_CMD_UFR_CopySPLNREQ() As Object	

#Region "Global Checks/Functions"
'		'Clear the previous Guid
''		GBL.GBL_Assembly.GBL_Helpers.ClearUFRGuid(Me.si)
		
'		'Reset the UFR Check Flag
''		GBL.GBL_Assembly.GBL_Helpers.ResetUFRState(si)
		
''		'Check Globals to verify xftv fields are input
''		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
''			Throw New XFException("UFR Is Invalid")
''		End If
		
'		'Get New UFR/Requirement ID based on new GUID generated in Globals
''		Dim NewReqID As String = GBL.GBL_Assembly.GBL_Helpers.GetNewUFRGuid(Me.si)
		
''		Dim isValid As Boolean = GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si)
''		BRApi.ErrorLog.LogMessage(Me.si, "DynGrid: IsUFRValid = " & isValid.ToString())
		
''		'Check Globals to verify xftv fields are input
''		If Not GBL.GBL_Assembly.GBL_Helpers.IsUFRValid(Me.si) Then
''			Throw New XFException("UFR Spreadsheet input is invalid or incomplete. Please fix required fields and save again.")
''		End If
		
#End Region
		
		'Get Selected ReqID
		Dim saveDataArgs As DashboardDynamicGridSaveDataArgs = args.SaveDataArgs
	    If saveDataArgs Is Nothing Then
	        Return Nothing
	    End If
		
		' Get the edited rows
	    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
	    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
	        Return Nothing
	    End If
		Dim CMD_UFR_Tracking_No As String = editedDataRows.Item(0).ModifiedDataRow("CMD_UFR_Tracking_No").ToString
		Brapi.ErrorLog.LogMessage(si, "CMD_UFR_Tracking_No = " & CMD_UFR_Tracking_No)
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
            ' --- Main Request Table (XFC_CMD_UFR) ---
            Dim dt As New DataTable()
            Dim sqa As New SqlDataAdapter()
            Dim sqaReader As New SQA_XFC_CMD_UFR(sqlConn)
            Dim sqlMain As String = $"SELECT * 
										FROM XFC_CMD_UFR 
										WHERE WFScenario_Name = @WFScenario_Name 
										AND WFCMD_Name = @WFCMD_Name 
										AND WFTime_Name = @WFTime_Name
										AND CMD_UFR_Tracking_No = '{CMD_UFR_Tracking_No}'"
			
            Dim sqlParams As SqlParameter() = New SqlParameter() {
            New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("ScenarioName")},
            New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("CMDName")},
            New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = WFInfoDetails("TimeName")} }
'            New SqlParameter("@CMD_UFR_Tracking_No", SqlDbType.UniqueIdentifier) With {.Value = editedDataRows.Item(0).ModifiedDataRow("CMD_UFR_Tracking_No")}
'						}
						
             sqaReader.Fill_XFC_CMD_UFR_DT(sqa, dt, sqlMain, sqlParams)
			 
			  
            ' --- Details Table (XFC_CMD_UFR_Details) ---
            Dim dt_Details As New DataTable()
            Dim sqa2 As New SqlDataAdapter()
            Dim sqaReaderdetail As New SQA_XFC_CMD_UFR_Details(sqlConn)
            Dim sqlDetail As String = $"SELECT * 
										FROM XFC_CMD_UFR_Details 
										WHERE WFScenario_Name = @WFScenario_Name 
										AND WFCMD_Name = @WFCMD_Name 
										AND WFTime_Name = @WFTime_Name
										AND CMD_UFR_Tracking_No = '{CMD_UFR_Tracking_No}'"
			
            sqaReaderdetail.Fill_XFC_CMD_UFR_Details_DT(sqa2, dt_Details, sqlDetail, sqlParams)
			
			
'            ' --- Details Table (XFC_CMD_UFR_Details_Audit) ---
			Dim sqa_xfc_cmd_ufr_details_audit = New SQA_XFC_CMD_UFR_DETAILS_AUDIT(sqlConn)
			Dim SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT = New DataTable()

			Dim SQL_Audit As String = $"SELECT * 
								FROM XFC_CMD_UFR_Details_Audit
								WHERE WFScenario_Name = @WFScenario_Name
								AND WFCMD_Name = @WFCMD_Name
								AND WFTime_Name = @WFTime_Name
								AND CMD_UFR_Tracking_No = '{CMD_UFR_Tracking_No}'"
			
			sqa_xfc_cmd_ufr_details_audit.Fill_XFC_CMD_UFR_DETAILS_Audit_DT(sqa, SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT, SQL_Audit, sqlParams)

            ' ************************************
            ' ************************************
#Region "Delete"			
'			Dim sU1APPNInput As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPPN","")
'			Dim sU2Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateMDEP","")
'			Dim sU3Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateAPEPT","")
'			Dim sU4Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateDollarType","")
'			Dim sU5Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateCType","")
'			Dim sU6Input As String = args.CustomSubstVars.XFGetValue("ML_CMD_UFR_FormulateObjectClass","")

'			Dim sEntity As String =  args.CustomSubstVars.XFGetValue("BL_CMD_UFR_FundsCenter","")
'			Dim entityLevel As String = Me.GetEntityLevel(sEntity)
'			Dim sREQWFStatus As String = entityLevel & "_Formulate_UFR"
			
'			Brapi.ErrorLog.LogMessage(si, " Dyn Grid Svc NewReqID = " & NewReqID)
#End Region	

			For Each editedDataRow As XFEditedDataRow In editedDataRows						
						
			    Dim targetRow As DataRow
				Dim req_ID_Val As Guid
				Dim Fund_Amount As Decimal =  editedDataRow.ModifiedDataRow.Item("Fund_Amount")		 
				Dim Fund_Status As String = editedDataRow.ModifiedDataRow.Item("Fund_Status")
				Dim Fund_Source As String = editedDataRow.ModifiedDataRow.Item("Fund_Source")
				
				req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_UFR_Tracking_No")
				
				'--- Update Header Table ---
				targetRow = dt.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'").FirstOrDefault()
				targetRow("Fund_Amount") = Fund_Amount
				targetRow("Fund_Status") = Fund_Status
				targetRow("Fund_Source") = Fund_Source
				targetRow("Update_Date") = DateTime.Now
				targetRow("Update_User") = si.UserName																																																																																				

				Dim targetRowFunding As DataRow
				targetRowFunding = dt_Details.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}'").FirstOrDefault()
				
				If targetRowFunding IsNot Nothing Then
				
					Dim FY As Decimal = editedDataRow.ModifiedDataRow.Item("FY")

					'Added Audit update before funding line updates are written to the table			
					If SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.Rows.Count > 0 Then
						Dim drow As DataRow
						drow = SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.Select($"CMD_UFR_Tracking_No = '{req_ID_Val}' AND Account = 'Req_Funding'").FirstOrDefault()
						drow("Orig_FY") = targetRowFunding("FY")
						drow("Updated_FY") = FY
					Else
						Dim newrow As datarow = SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.NewRow()
						newrow("CMD_UFR_Tracking_No") = targetRow("CMD_UFR_Tracking_No")
						newrow("WFScenario_Name") = targetRow("WFScenario_Name")
						newrow("WFCMD_Name") = targetRow("WFCMD_Name")
						newrow("WFTime_Name") = targetRow("WFTime_Name")
						newrow("Entity") = targetRow("Entity")
						newrow("Account") = "Req_Funding"
						newrow("Orig_IC") = "None"
						newrow("Updated_IC") = "None"
						newrow("Orig_Flow") =  targetRow("Command_UFR_Status")
						newrow("Updated_Flow") = targetRow("Command_UFR_Status")
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
						End If
						
						newrow("Orig_UD6") = targetRow("Obj_Class")
						newrow("Updated_UD6") = targetRow("Obj_Class")
						newrow("Orig_UD7") = "None"
						newrow("Updated_UD7") = "None"
						newrow("Orig_UD8") = "None"
						newrow("Updated_UD8") = "None"
						newrow("Create_Date") = DateTime.Now
						newrow("Create_User") = si.UserName
						newrow("Orig_FY") = targetRowFunding("FY")
						newrow("Updated_FY") = FY
								
						SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT.rows.add(newrow)	
										
					End If		
						targetRowFunding("FY") = FY
						targetRowFunding("Update_Date") = DateTime.Now
                        targetRowFunding("Update_User") = si.UserName  
						
				End If      
			Next
				
'				Throw New XFException("TEST: Dyn_Grid_Svc Update table was reached")
		                ' Persist changes back to the DB using the configured adapter
		                sqaReaderdetail.Update_XFC_CMD_UFR_Details(dt_Details,sqa2)
		                sqaReader.Update_XFC_CMD_UFR(dt,sqa)
						sqa_xfc_cmd_ufr_details_audit.Update_XFC_CMD_UFR_DETAILS_AUDIT(SQA_XFC_CMD_UFR_DETAILS_AUDIT_DT, sqa)
						
		                End Using
		            End Using
					
'            Return Nothing
        End Function	

#End Region
#End Region

	End Class
End Namespace