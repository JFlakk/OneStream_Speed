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
Imports Workspace.GBL.GBL_Assembly

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.cPROBE_String_Helper
	Public Class MainClass
		Public si As SessionInfo
        Public globals As BRGlobals
        Public api As Object
        Public args As DashboardStringFunctionArgs
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Me.si = si
				Me.globals = globals
				Me.api = api
				Me.args = args

				Select Case args.FunctionName.ToLower()
				Case "getcprobecascadingmbrfilter"	
					Return Me.GetcPROBECascadingMbrFilter()
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "GetcPROBECascadingMbrFilter"
		Public Function GetcPROBECascadingMbrFilter() As String
			Try
				' Read incoming args
				Dim cmd As String = args.NameValuePairs.XFGetValue("cmd", "NA")
				Dim entity As String = args.NameValuePairs.XFGetValue("entity", "NA")
				Dim appn As String = args.NameValuePairs.XFGetValue("appn", String.Empty)
				Dim mdep As String = args.NameValuePairs.XFGetValue("mdep", "NA")
				Dim sag As String = args.NameValuePairs.XFGetValue("sag", "NA")
				Dim ape As String = args.NameValuePairs.XFGetValue("ape", "NA")
				Dim dollarType As String = args.NameValuePairs.XFGetValue("dollarType", "NA")
				Dim status As String = args.NameValuePairs.XFGetValue("status", "NA")
				Dim returnType As String = args.NameValuePairs.XFGetValue("returnType", "NA")
				Dim cvName As String = args.NameValuePairs.XFGetValue("cvName", "NA") '"CMD_PGM_cPROBE_FDX_CV"

				' Build a compact signature for Entity + Appn only
				Dim currRebuildparams As String = String.Concat(entity, "|", appn, "|", status, "|", returnType)
				
				' Use workspace session settings to persist last seen signatures per user/workspace
				Dim cacheCat As String = "CascadingFilterCache"
				Dim filterDTparams As String = "FilterDTparams"
				Dim rebuildparams As String = "rebuildparams"

				Dim prevRebuildParams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, "")
				Dim needsRebuild As Boolean = Not String.Equals(prevRebuildParams, currRebuildparams, StringComparison.Ordinal)

				Dim dt As New DataTable 
'BRApi.Errorlog.LogMessage(si,$"Hit here - {entity} - {returnType} - {appn}")

				If needsRebuild Then
					BRApi.Utilities.SetWorkspaceSessionSetting(si, si.UserName, cacheCat, rebuildparams, currRebuildparams)
					dt = GetFDXCascadingMbrFilter(cvName,cmd,appn)
				Else
					dt = BRApi.Utilities.GetSessionDataTable(si, si.UserName, "CascadingFilter")
				End If
				If Not dt Is Nothing Then

					' Build a deterministic signature of the inputs (full)
					Dim currFilterDTparams As String = String.Concat(entity, "|", appn, "|", status, "|", returnType, "|", mdep, "|", sag, "|", ape, "|", dollarType)
	
					Dim prevFilterDTparams As String = BRApi.Utilities.GetWorkspaceSessionSetting(si, si.UserName, cacheCat, filterDTparams, "")
	
	
					Dim filterParts As New List(Of String)
	
					' Only add filters when parameter is provided and not "NA"
					If Not String.IsNullOrWhiteSpace(mdep) AndAlso Not mdep.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD2] = '" & mdep.Replace("'", "''") & "'")
					End If
	
					If Not String.IsNullOrWhiteSpace(ape) AndAlso Not ape.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD3] = '" & ape.Replace("'", "''") & "'")
					End If
	
					If Not String.IsNullOrWhiteSpace(dollarType) AndAlso Not dollarType.Equals("NA", StringComparison.OrdinalIgnoreCase) Then
						filterParts.Add("[UD4] = '" & dollarType.Replace("'", "''") & "'")
					End If
	
	
					Dim filterExpr As String = If(filterParts.Count > 0, String.Join(" AND ", filterParts), String.Empty)
					' Filter dt into a DataTable so it can be converted to a DataView
					Dim filteredDt As DataTable
					If String.IsNullOrEmpty(filterExpr) Then
						filteredDt = dt.Copy()
					Else
						filteredDt = dt.Clone()
						Dim selectedRows() As DataRow = dt.Select(filterExpr)
						For Each row As DataRow In selectedRows
							filteredDt.ImportRow(row)
						Next
					End If
					BRApi.Utilities.SetSessionDataTable(si, si.UserName, "CascadingFilter",filteredDt)
	
					Dim dv As DataView = New DataView(filteredDt)
					' Map returnType values to column keys (case-insensitive)
					Dim returnTypeMap As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
						{"APPN", "UD1"},
						{"MDEP", "UD2"},
						{"SAG",  "UD3"},
						{"APE",  "UD3"},
						{"DollarType",  "UD4"}
					}
	
					' Determine which physical column (if any) corresponds to the requested returnType
					Dim selectedColumn = String.Empty
					selectedColumn = returnTypeMap.XFGetValue(returnType,"Not Found")
					If selectedColumn <> "Not Found"
	
						dv.RowFilter = $"{selectedColumn} IS NOT NULL AND {selectedColumn} <> ''"
						dv.Sort = selectedColumn & " ASC"
						Dim mlDT = dv.ToTable(True, selectedColumn) ' Distinct values only
		
						Dim result As String = String.Empty
						For Each dr As DataRow In mlDT.Rows
							Dim val As String = dr(selectedColumn).ToString()
							If Not String.IsNullOrWhiteSpace(val) Then
								If String.IsNullOrEmpty(result) Then
									result = $"{selectedColumn}#{val}"
								Else
									result &= $",{selectedColumn}#{val}"
								End If
							End If
						Next
'BRapi.ErrorLog.LogMessage(si,$"Hit result: {selectedColumn} - {result}")
						Return result
					Else
						Return String.Empty
					End If
				Else
'BRapi.ErrorLog.LogMessage(si,"Hit Empty")
					Return String.Empty
				End If					

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "Helper Functions"
	
		Private Function GetFDXCascadingMbrFilter(ByVal cvName As String,ByVal entFilter As String,ByVal appn As String) As DataTable
			Dim dt As New DataTable()
			Dim wsName As String = "00 GBL"
			Dim wsID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si,False,wsName)
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim sScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			Dim CprobeScen As String = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetSrccPROBEScen(si,sScenario)

			Dim entDim = $"E_{wfInfoDetails("CMDName")}"
			Dim scenDim = "S_RMW"
			Dim scenFilter = $"S#{CprobeScen}"
			Dim timeFilter = String.Empty '$"T#{wfInfoDetails("TimeName")}"
			Dim NameValuePairs = New Dictionary(Of String,String)
'			If appn = String.Empty
'				appn = "OMA"
'			End If
			'NameValuePairs.Add("ML_CMD_PGM_FormulateAPPN",appn)
			
			Dim nvbParams As NameValueFormatBuilder = New NameValueFormatBuilder(String.Empty,NameValuePairs,False)

			'dt = BRApi.Import.Data.FdxExecuteCubeView(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 1, False)

			dt = BRApi.Import.Data.FdxExecuteCubeViewTimePivot(si, wsID, cvName, entDim, $"E#{entFilter}", scenDim, scenFilter, timeFilter, nvbParams, False, True, True, String.Empty, 1, False)
'If dt Is Nothing
'	BRAPI.ErrorLog.LogMessage(si,"Hit NOthing")
'End If
			Return dt
		End Function
		
	
	#End Region 'Update 10/16/2025
	End Class
End Namespace