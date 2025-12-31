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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.GBL_ADM_String_Helper
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
				Select Case args.FunctionName
					Case Is = "cPRPOBE_posAssignment"
						Return cPRPOBE_posAssignment()
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function


 		Private Function cPRPOBE_posAssignment() As String
                  Dim scenList As New List(Of String)
                  Dim scenDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "S_RMW")
                  Dim scenmbrList As New List(Of MemberInfo)
                  scenmbrList = BRApi.Finance.Members.GetMembersUsingFilter(si, scenDimPk, $"S#CMD_PGM.Base, S#CMD_TGT.Base, S#CMD_SPLN.Base, S#HQ_SPLN.Base", True)
                  For Each scenmbr As memberInfo In scenmbrList
                        Dim timeID As Integer = BRApi.Finance.Scenario.GetWorkflowTime(si, scenmbr.Member.MemberID)                       
                        Dim WFTime As String = BRApi.Finance.Time.GetNameFromId(si, timeId)
                        Dim monthly As String = String.Empty
                        If scenmbr.Member.Name.xfcontainsignorecase("SPLN")
                              monthly = "M1"
                        End If
                        scenList.Add($"S#{scenmbr.Member.Name}:T#{WFTime}{monthly}")            
                  Next
                  Return String.Join(",",scenList)
                  
		End Function
		
	End Class
End Namespace