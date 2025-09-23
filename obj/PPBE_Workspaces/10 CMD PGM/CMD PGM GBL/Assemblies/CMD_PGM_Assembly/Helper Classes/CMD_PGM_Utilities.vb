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
	Public Class CMD_PGM_Utilities

        Public Function Test(ByVal si As SessionInfo) As Object
            Try

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

		Public shared Function	CheckFor_General(ByVal si As SessionInfo, entity As String) As Object
			'If the FC is a parent, add _General
			Dim fundCenter As String = entity
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_XFTV_Helpers.GetWFInfoDetails(si)
			Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & wfInfoDetails("CMDName"))
			membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, "E#" & fundCenter & ".base", True)
			If membList.Count > 1 Then
				fundCenter = fundCenter & "_General"
			End If
			Return fundCenter
		End Function
		
		Public Shared Function	GetUD3(ByVal si As SessionInfo, UD1 As String, UD3 As String) As Object
			'====== Get APPN_FUND And PARENT APPN_L2 ======
Brapi.ErrorLog.LogMessage(si, "UD1: " & UD1 & ", UD3: " & UD3)			
			Dim U1Name As String = UD1				
			Dim U1DimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "U1_FundCode")
Brapi.ErrorLog.LogMessage(si, "U1DimPk: " & U1DimPk.ToString)			
			Dim U1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimType.UD1.Id, U1Name)
BRApi.ErrorLog.LogMessage(si, "U1MemberID: " & U1MemberID.ToString)				
			Dim U1ParentName As String = BRApi.Finance.Members.GetParents(si,U1DimPk, U1MemberId, False, )(0).Name 	
BRApi.ErrorLog.LogMessage(si, "U1ParentName: " & U1ParentName)				
			Dim U3Concat As String = U1ParentName & "_" & UD3
BRApi.ErrorLog.LogMessage(si, "U3Concat: " & U3Concat)					
			Return U3Concat
			
		End Function
	
#Region "Get Inflation Rate"	
Public Shared Function GetInflationRate(ByVal si As SessionInfo, ByVal sCube As String, ByVal sEntity As String, ByVal sScenario As String,ByVal sREQTime As String, ByVal sAPPN As String) As Object 

				Dim sInfRateScript As String = "Cb#" & sCube & ":S#" & sScenario & ":T#" & sREQTime & ":C#USD:E#"& sCube & "_General:V#Periodic:A#PGM_Inflation_Rate_Amt:O#BeforeAdj:I#None:F#None" 
					sInfRateScript &= ":U1#" & sAPPN & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"		
				Dim dInflationRate As Decimal = 0
					dInflationRate = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, sCube, sInfRateScript).DataCellEx.DataCell.CellAmount
			Return dInflationRate

End Function
#End Region
			
		
		
		
	End Class
End Namespace
