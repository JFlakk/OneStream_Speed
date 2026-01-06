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
	Public Class HQ_SPLN_Svc_Factory
		Implements IWsAssemblyServiceFactory

        Public Function CreateWsAssemblyServiceInstance(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal wsasType As WsAssemblyServiceType, ByVal itemName As String) As IWsAssemblyServiceBase Implements IWsAssemblyServiceFactory.CreateWsAssemblyServiceInstance
            Try
                Select Case wsasType

                    Case Is = WsAssemblyServiceType.FinanceCustomCalculate				
                  		Return New HQ_SPLN_FinCustCalc()

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