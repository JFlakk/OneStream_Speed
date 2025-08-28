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
	Public Class GBL_XFTV_Helpers

        Public Shared Function Dirty_Column_List(si As SessionInfo, xftvRow As TableViewRow) As List(Of String)
            Dim dirtyColList As List(Of String) = xftvRow.Items.Keys.Cast(Of String)().Where(Function(k)
                Dim xftvCol = xftvRow.Item(k)
                If xftvCol Is Nothing Then 
					Return False
				Else
                    Return CBool(xftvCol.IsDirty())
				End If
			End Function).ToList()
			Return dirtyColList
        End Function
		
        Public Shared Function Convert_xftvCol_to_DbValue(si As SessionInfo, xftvCol As TableViewColumn) As Object
            Try
				If xftvCol Is Nothing Then
	                Return DBNull.Value
                Else
                    Select Case xftvCol.DataType()
                        Case XfDataType.Text
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value
                        Case XfDataType.Int16, XfDataType.Int32, XfDataType.Int64
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToInt()
                        Case XfDataType.Decimal, XfDataType.Double
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToDecimal()
                        Case XfDataType.Boolean
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToBool()
                        Case XfDataType.DateTime
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
							Return DateTime.ParseExact(xftvCol.Value,"MM/dd/yyyy",CultureInfo.InvariantCulture)
                        Case XfDataType.Guid
                            If xftvCol.Value Is Nothing Then
                                Return DBNull.Value
                            End If
                            Return xftvCol.Value.XFConvertToGuid()
                        Case Else
                            Return If(xftvCol.Value Is Nothing, DBNull.Value, xftvCol.Value)
                    End Select
                End If
            ' Obtain Value and DataType properties by reflection (works for the XFTV cell)
            Catch ex As Exception
	            BRApi.ErrorLog.LogMessage(si, $"ConvertxftvColtoDbValue: unexpected error: {ex.Message}")
	            Return DBNull.Value
            End Try
        End Function

        Public Shared Function GetWFInfoDetails(si As SessionInfo) As Dictionary(Of String, String)
            Dim WFInfoDetails As New Dictionary(Of String, String)()

            Dim wfInitInfo = BRApi.Workflow.General.GetUserWorkflowInitInfo(si)
            Dim wfUnitInfo = wfInitInfo.GetSelectedWorkflowUnitInfo()
            Dim wfCubeRootInfo = BRApi.Workflow.Metadata.GetCubeRootInfo(si, wfUnitInfo.wfunitPk.ProfileKey,True)


            WFInfoDetails.Add("ProfileName", wfUnitInfo.ProfileName)
            WFInfoDetails.Add("ScenarioName", wfUnitInfo.ScenarioName)
            WFInfoDetails.Add("TimeName", wfUnitInfo.TimeName)
            WFInfoDetails.Add("CMDName", wfCubeRootInfo.CubeRootProfile.CubeName)

            Return WFInfoDetails
        End Function

	End Class
End Namespace

