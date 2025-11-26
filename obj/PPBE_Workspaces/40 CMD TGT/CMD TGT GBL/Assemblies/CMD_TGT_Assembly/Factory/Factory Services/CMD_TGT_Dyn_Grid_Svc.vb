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
	Public Class CMD_TGT_Dyn_Grid_Svc
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
                	If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_TGT_ImportDist") Then
						Return dg_CMD_TGT_ImportDist()
					End If
					
					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_TGT_ImportWH") Then
						Return dg_CMD_TGT_ImportWH()
					End If
					
'					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_REQList") Then
'				    	Return dg_CMD_PGM_REQList()            
'					End If
					
'					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Prioritize_REQ") Then
'				    	Return dg_CMD_PGM_Prioritize_REQ()            
'					End If
										
'					If args.Component.Name.XFEqualsIgnoreCase("dg_CMD_PGM_Rollover_REQ") Then
'				    	Return dg_CMD_PGM_Rollover_REQ()            
'					End If
				
				End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

        Public Function SaveDynamicGridData(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardDynamicGridArgs) As XFDynamicGridSaveDataResult Implements IWsasDynamicGridV800.SaveDynamicGridData
            Try

                If (brGlobals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function
		
		Private Function dg_CMD_TGT_ImportDist() As XFDynamicGridGetDataResult
			'Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			'Dim sScenario As String = wfInfoDetails("ScenarioName")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
			Dim tableName As String = "CMD_TGT_Import_TGT_Dist_" & sScenario
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName,tableName)
			If dt Is Nothing Then Return Nothing
			Dim skp As New Dictionary(Of String, Object)
			
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
			Dim ValidationError As New XFDynamicGridColumnDefinition()
			ValidationError.ColumnName = "ValidationError"
			ValidationError.IsFromTable = True
			ValidationError.IsVisible = True
			ValidationError.AllowUpdates = False
			columnDefinitions.Add(ValidationError)
			
						
			Dim FiscalYear As New XFDynamicGridColumnDefinition()
			FiscalYear.ColumnName = "FiscalYear"
			FiscalYear.IsFromTable = True
			FiscalYear.IsVisible = True
			FiscalYear.AllowUpdates = False
			columnDefinitions.Add(FiscalYear)
			
			
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
			Dim xfdt As New XFDataTable(si,dt,Nothing,10000)
			Dim rslt As New XFDynamicGridGetDataResult(xfdt,columnDefinitions,DataAccessLevel.AllAccess)
'BRApi.ErrorLog.LogMessage(si, "row count: " & rslt.DataTable.Rows.Count )
			Return rslt

		End Function

			
		Private Function dg_CMD_TGT_ImportWH() As XFDynamicGridGetDataResult
			'Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			'Dim sScenario As String = wfInfoDetails("ScenarioName")
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
			Dim tableName As String = "CMD_TGT_Import_TGT_WH_" & sScenario
			Dim dt As DataTable = BRApi.Utilities.GetSessionDataTable(si, si.UserName,tableName)
			If dt Is Nothing Then Return Nothing
			Dim skp As New Dictionary(Of String, Object)
			
			Dim columnDefinitions As New List(Of XFDynamicGridColumnDefinition)
			Dim ValidationError As New XFDynamicGridColumnDefinition()
			ValidationError.ColumnName = "ValidationError"
			ValidationError.IsFromTable = True
			ValidationError.IsVisible = True
			ValidationError.AllowUpdates = False
			columnDefinitions.Add(ValidationError)
			
						
			Dim FiscalYear As New XFDynamicGridColumnDefinition()
			FiscalYear.ColumnName = "FiscalYear"
			FiscalYear.IsFromTable = True
			FiscalYear.IsVisible = True
			FiscalYear.AllowUpdates = False
			columnDefinitions.Add(FiscalYear)
			
			
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
			Dim xfdt As New XFDataTable(si,dt,Nothing,10000)
			Dim rslt As New XFDynamicGridGetDataResult(xfdt,columnDefinitions,DataAccessLevel.AllAccess)
'BRApi.ErrorLog.LogMessage(si, "row count: " & rslt.DataTable.Rows.Count )
			Return rslt

		End Function

	End Class
End Namespace
