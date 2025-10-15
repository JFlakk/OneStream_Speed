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
	Public Class CMD_PGM_Dyn_Grid_Svc
		Implements IWsasDynamicGridV800

		Private si As SessionInfo
        Private globals As BRGlobals
        Private workspace As DashboardWorkspace
        Private args As DashboardDynamicGridArgs
		Private api As Object
		
        Public Function GetDynamicGridData(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridGetDataResult Implements IWsasDynamicGridV800.GetDynamicGridData
			
            'Assign to global variables
            Me.si = si
            Me.globals = globals
            Me.workspace = workspace
            Me.args = args	
			Me.api = api
			
        	Try
                If (brGlobals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Imported_Req") Then
						Return dg_CMD_PGM_Imported_Req()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQList") Then
				    	Return dg_CMD_PGM_REQList()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Prioritize_REQ") Then
				    	Return dg_CMD_PGM_Prioritize_REQ()            
					End If
										
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Rollover_REQ") Then
				    	Return dg_CMD_PGM_Rollover_REQ()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQManpower") Then
				    	Return dg_CMD_PGM_REQManpower()            
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQManage") Then
				    	Return dg_CMD_PGM_REQManage()            
					End If
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

        Public Function SaveDynamicGridData(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridSaveDataResult Implements IWsasDynamicGridV800.SaveDynamicGridData
            Me.si = si
            Me.globals = globals
            Me.workspace = workspace
            Me.args = args	
			Me.api = api
			
			
			Try

                If (brGlobals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
             
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Prioritize_REQ") Then
				    	Return Save_dg_CMD_PGM_Prioritize_REQ()            
					End If
					
				
				
				End If
				
                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		


		Private Function dg_CMD_PGM_Imported_Req() As XFDynamicGridGetDataResult
		
			
			Dim tableName As String = "CMD_PGM_Import"
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

			Dim coldefs As New List(Of XFDynamicGridColumnDefinition)
			Dim xfdt As New XFDataTable(si,dt,Nothing,10000)
			Dim rslt As New XFDynamicGridGetDataResult(xfdt,coldefs,DataAccessLevel.AllAccess)
'BRApi.ErrorLog.LogMessage(si, "row count: " & rslt.DataTable.Rows.Count )
			Return rslt
			
		End Function
#Region "Constants"
Private BR_CMD_PGMDataSet As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CMD_PGM_DataSet.MainClass
#End Region	

#Region "REQ List"
Private Function dg_CMD_PGM_REQList() As Object
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
			columnDefinitions.Add(Status)
						
							
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
										 Req.Entity,Dtl.UD1,Dtl.UD2,Dtl.UD3,Dtl.UD4,Dtl.UD5,Dtl.UD6,
										 Dtl.FY_1,
										 Dtl.FY_2,
										 Dtl.FY_3,
										 Dtl.FY_4,
										 Dtl.FY_5				
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

						
					  				
'					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dt,Nothing,1000)
						
					
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
			APE.Description = "APE"
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
						'BRAPi.ErrorLog.LogMessage(si,$"Hit {sEntity}")
					
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
								Dtl.FY_1,
								  Dtl.FY_2,
								  Dtl.FY_3,
								  Dtl.FY_4,
								  Dtl.FY_5
								
								FROM XFC_CMD_PGM_REQ_Details AS Dtl
								LEFT JOIN XFC_CMD_PGM_REQ_Priority AS Pri
								ON Pri.CMD_PGM_REQ_ID = Dtl.CMD_PGM_REQ_ID
								AND Pri.WFScenario_Name = Dtl.WFScenario_Name
								AND Pri.WFCMD_Name = Dtl.WFCMD_Name
								AND Pri.WFTime_Name = Dtl.WFTime_Name
								AND Pri.Entity = Dtl.Entity
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
								AND Dtl.flow = '{sREQWFStatus}'"
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
                dtPri = BRApi.Database.ExecuteSql(dbConnApp,sql,False)
            End Using

					'Brapi.ErrorLog.LogMessage(si, "SQL" & sql )  				
					     'Create the XFTable
					    Dim xfTable As New xfDataTable(si,dtPri,Nothing,1000)
					
					     'Send the result To the Interface component
					    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
					        
					    Return taskResult
		End Function
#End Region

#Region "Manage"
	Private Function dg_CMD_PGM_REQManage() As Object
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
			Title.IsVisible = True
			Title.AllowUpdates = False
			columnDefinitions.Add(Title)
			
	Dim Status As New XFDynamicGridColumnDefinition()
			Status.ColumnName = "Flow"
			Status.IsFromTable = True
			Status.IsVisible = True
			Status.Description = "Current Status"
			Status.AllowUpdates = False
			columnDefinitions.Add(Status)
			
	Dim NewStatus As New XFDynamicGridColumnDefinition()
			NewStatus.ColumnName = "New Status"
			NewStatus.IsFromTable = True
			NewStatus.IsVisible = True
			NewStatus.AllowUpdates = True
			NewStatus.ParameterName = "BL_CMD_PGM_StatusChange"
			'.ParameterName = 
			columnDefinitions.Add(NewStatus)
								
			

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
						objdt.Columns.Add("New Status",GetType(String))
						
					  				
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
Brapi.ErrorLog.LogMessage(si, "HERE 1")
    ' Get the edited rows
    Dim editedDataRows As List(Of XFEditedDataRow) = saveDataArgs.EditedDataRows
    If editedDataRows Is Nothing OrElse editedDataRows.Count = 0 Then
        Return Nothing
    End If
	Brapi.ErrorLog.LogMessage(si, "HERE 2")
		Dim WFInfoDetails As New Dictionary(Of String, String)()
            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
			Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetProfile(si,wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
			WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeName)
			
		Brapi.ErrorLog.LogMessage(si, "HERE 3")	
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
			  
		Brapi.ErrorLog.LogMessage(si, "HERE 4")	

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
		
		
    Dim targetRow As DataRow 											
	Dim req_ID_Val As Guid
	req_ID_Val = editedDataRow.ModifiedDataRow.Item("CMD_PGM_REQ_ID")
	Dim isInsert As Boolean = False		
	targetRow = dt_Priority.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
	
If targetRow Is Nothing Then
	 isInsert = True
    targetRow = dt_Priority.NewRow() 
	Dim req_ID As String = editedDataRow.ModifiedDataRow.Item("REQ_ID").ToString()
    Dim mainRow As DataRow = dt.Select($"REQ_ID = '{req_ID}'").FirstOrDefault()

    If mainRow IsNot Nothing Then
        targetRow("CMD_PGM_REQ_ID") = mainRow("CMD_PGM_REQ_ID")
      
    End If
    
    
Else
   
    isInsert = False
	targetRow = dt_Priority.Select($"CMD_PGM_REQ_ID = '{req_ID_Val}'").FirstOrDefault()
    End If
		targetRow("WFScenario_Name") = wfInfoDetails("ScenarioName")
		targetRow("WFCMD_Name") = wfInfoDetails("CMDName")
		targetRow("WFTime_Name") = wfInfoDetails("TimeName")
		targetRow("Entity") = editedDataRow.ModifiedDataRow.Item("Entity").ToString()
		 targetRow("Review_Entity") = editedDataRow.ModifiedDataRow.Item("Entity").ToString()
		 
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
		targetRow("Auto_Rank") = editedDataRow.ModifiedDataRow.Item("Auto_Rank").ToString()
		targetRow("Rank_Override") = editedDataRow.ModifiedDataRow.Item("Rank_Override").ToString()
		
		targetRow("Create_Date") = DateTime.Now
		targetRow("Create_User") = si.UserName
		targetRow("Update_Date") = DateTime.Now
		targetRow("Update_User") = si.UserName

Brapi.ErrorLog.LogMessage(si, "HERE 8") 
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
		                Brapi.ErrorLog.LogMessage(si, "HERE 10") 
		                sqaReaderPriority.Update_XFC_CMD_PGM_REQ_Priority(si,dt_Priority,sqa3)
						
						
						
		                End Using
		            End Using
Brapi.ErrorLog.LogMessage(si, "HERE 11") 
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
					FY_2.Description = WFYear + 1
					columnDefinitions.Add(FY_2)
					
		   Dim FY_3 As New XFDynamicGridColumnDefinition()
					FY_3.ColumnName = "FY_3"
					FY_3.IsFromTable = True
					FY_3.IsVisible = False
					FY_3.Description = WFYear + 2
					columnDefinitions.Add(FY_3)
					
		    Dim FY_4 As New XFDynamicGridColumnDefinition()
					FY_4.ColumnName = "FY_4"
					FY_4.IsFromTable = True
					FY_4.IsVisible = False
					FY_4.Description = WFYear + 3
					columnDefinitions.Add(FY_4)
					
		    Dim FY_5 As New XFDynamicGridColumnDefinition()
					FY_5.ColumnName = "FY_5"
					FY_5.IsFromTable = True
					FY_5.IsVisible = False
					FY_5.Description = WFYear + 4
					columnDefinitions.Add(FY_5)
								
							
			' Get the data you want To put In the grid
			
			Dim sEntity As String  = args.CustomSubstVars("BL_CMD_PGM_FundsCenter")
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
'Brapi.ErrorLog.LogMessage(si,"Entity: " & FCList)
			objdt = BR_CMD_PGMDataSet.Main(si, globals, api, dargs)

			
		  				
'					     'Create the XFTable
		    Dim xfTable As New xfDataTable(si,objdt,Nothing,1000)
			
		
		     'Send the result To the Interface component
		    Dim taskResult As New XFDynamicGridGetDataResult(xfTable,columnDefinitions,DataAccessLevel.AllAccess)
		        
		    Return taskResult		
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
            
                Dim sFundCenter As String =  args.CustomSubstVars("BL_CMD_PGM_FundsCenter")

                Dim priCatMembers As List(Of MemberInfo)
                Dim priFilter As String = "A#GBL_Priority_Cat_Weight.Children"
                Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
                 priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
                
                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#UUUU:U6#None:U7#None:U8#None"
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

	End Class
End Namespace
