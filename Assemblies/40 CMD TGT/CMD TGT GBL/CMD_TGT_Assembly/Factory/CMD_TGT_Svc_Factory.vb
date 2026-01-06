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
	Public Class CMD_TGT_Svc_Factory
		Implements IWsAssemblyServiceFactory

        Public Function CreateWsAssemblyServiceInstance(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal wsasType As WsAssemblyServiceType, ByVal itemName As String) As IWsAssemblyServiceBase Implements IWsAssemblyServiceFactory.CreateWsAssemblyServiceInstance
            Try				
                Select Case wsasType                   
					Case Is = WsAssemblyServiceType.FinanceCustomCalculate			
						Return New CMD_TGT_FinCustCalc()
						
                    Case Is = WsAssemblyServiceType.Component
'brapi.ErrorLog.LogMessage(si, "Component")						
                     '   Return New WsasComponent()

                    Case Is = WsAssemblyServiceType.Dashboard
'brapi.ErrorLog.LogMessage(si, "Dashboard")						
                        'Return New WsasDashboard()

                    Case Is = WsAssemblyServiceType.DataManagementStep
'brapi.ErrorLog.LogMessage(si, "DataManagementStep")						
                        'Return New CMD_TGT_DataManagementStep()

                    Case Is = WsAssemblyServiceType.DataSet
'brapi.ErrorLog.LogMessage(si, "DataSet")						
                        'Return New WsasDataSet()

                    Case Is = WsAssemblyServiceType.DynamicDashboards
'brapi.ErrorLog.LogMessage(si, "DynamicDashboards")						
                    	'Return New WsasDynamicDashboards()

                    Case Is = WsAssemblyServiceType.DynamicCubeView
'brapi.ErrorLog.LogMessage(si, "DynamicCubeView")						
                    	'Return New WsasDynamicCubeView()

                    Case Is = WsAssemblyServiceType.DynamicGrid						
                        Return New CMD_TGT_Dyn_Grid_Svc()

                    Case Is = WsAssemblyServiceType.FinanceCore
'brapi.ErrorLog.LogMessage(si, "DynamicGrid")						
                        'Return New WsasFinanceCore()

                    Case Is = WsAssemblyServiceType.FinanceGetDataCell
'brapi.ErrorLog.LogMessage(si, "FinanceGetDataCell")						
                        'Return New WsasFinanceGetDataCell()

                    Case Is = WsAssemblyServiceType.FinanceMemberLists
'brapi.ErrorLog.LogMessage(si, "FinanceMemberLists")						
                        'Return New WsasFinanceMemberLists()

                    Case Is = WsAssemblyServiceType.SqlTableEditor
'brapi.ErrorLog.LogMessage(si, "SqlTableEditor")						
                        'Return New WsasSqlTableEditor()

                    Case Is = WsAssemblyServiceType.TableView
'brapi.ErrorLog.LogMessage(si, "TableView")						
                        'Return New WsasTableView()

                    Case Is = WsAssemblyServiceType.XFBRString
'brapi.ErrorLog.LogMessage(si, "XFBRString")						
                    	'Return New WsasXFBRString()

                    Case Else
                        Return Nothing
                End Select

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

	End Class
End Namespace