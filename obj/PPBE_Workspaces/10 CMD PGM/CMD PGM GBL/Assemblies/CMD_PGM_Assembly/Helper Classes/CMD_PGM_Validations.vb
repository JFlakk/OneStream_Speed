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
	Public Class CMD_PGM_Validations

		'Function takes in dimension and member script and returns true for valid member and false for not valid
		'Sample call ValidateMemberExists(si, "U3_APE_PT", "U3#.<APE9 value>.base")
        Public Function ValidateMemberExists(ByVal si As SessionInfo, ByVal dimension As String, ByVal memberScript As String) As Boolean
            Try
				If (Not String.IsNullOrWhiteSpace(dimension)) And (Not String.IsNullOrWhiteSpace(memberScript)) Then
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, dimension)
					If objDimPk Is Nothing Then
						Return False
					End If
					Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk,memberScript, True)
					If (membList.Count <> 1 ) Then 
						Return False
					Else
						Return True
					End If					
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
	
		
		'Function takes in a string value and returns true if numeric and false for not valid
		'Sample call ValidateMemberExists(si, "value")
        Public Function ValidateNumeric(ByVal si As SessionInfo, ByVal value As String) As Boolean
            Try
				If (Not String.IsNullOrWhiteSpace(value)) And (IsNumeric(value)) Then
						Return True
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

		
		'Function takes in a string value and returns true if date and false for not valid
		'Sample call ValidateMemberExists(si, "value")
        Public Function ValidateDate(ByVal si As SessionInfo, ByVal value As String) As Boolean
            Try
				If (Not String.IsNullOrWhiteSpace(value)) And (IsDate(value)) Then
						Return True
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
	
		
		'Function takes in a command anf fundcenter value and returns true if the fundcenter belongs to the command and false if not
		'Sample call ValidateMemberExists(si, "AFC", "A97AC")
        Public Function ValidateFundCenterInCommand(ByVal si As SessionInfo, ByVal command As String, ByVal fundCenter As String) As Boolean
            Try
				If (Not String.IsNullOrWhiteSpace(command)) And (Not String.IsNullOrWhiteSpace(fundCenter)) Then
					Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_" & command)
					If objDimPk Is Nothing Then
						Return False
					End If
					Dim membList As List(Of memberinfo) = New List(Of MemberInfo)
					membList = BRApi.Finance.Members.GetMembersUsingFilter(si, objDimPk, $"E#{command}.Base.Where(Name Contains {fundCenter})", True)
					If (membList.Count <> 1 ) Then 
						Return False
					Else
						Return True
					End If					
				Else
					Return False
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
	End Class
End Namespace