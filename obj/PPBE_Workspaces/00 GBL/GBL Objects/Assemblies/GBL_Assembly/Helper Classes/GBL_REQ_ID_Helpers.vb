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
	Public Class GBL_REQ_ID_Helpers

		'This method returns the MAX REQ ID from the PGM and SPL REQ tables
        Public Shared Function Get_FC_REQ_ID(si As SessionInfo, fundCenter as String) As String
			Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
			
		  ' Query to get the highest REQ_ID from both tables
            Dim SQL As String = $"
                SELECT MAX(CAST(SUBSTRING(REQ_ID, CHARINDEX('_', REQ_ID) + 1, LEN(REQ_ID)) AS INT)) AS MaxID
                FROM (
                    SELECT REQ_ID FROM XFC_CMD_PGM_REQ  WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                    UNION ALL
                    SELECT REQ_ID FROM XFC_CMD_SPLN_REQ WHERE ENTITY = '{fundcenter}' AND WFScenario_Name = '{WFScenario}'
                ) AS Combined"
'BRApi.ErrorLog.LogMessage(si,"SQL: " & SQL)
			
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
		
	End Class
End Namespace
