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
	Public Class CMD_UFR_FinCustCalc
		Implements IWsasFinanceCustomCalculateV800
		
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As FinanceRulesApi
		Public args As FinanceRulesArgs
		Public Global_Functions As OneStream.BusinessRule.Finance.Global_Functions.MainClass
        
        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Me.Global_Functions = New OneStream.BusinessRule.Finance.Global_Functions.MainClass(si,globals,api,args)
				
'				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Load_Reqs_to_Cube") Then				
'					Me.Load_Reqs_to_Cube()
'				End If
							
'				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateWeightedScoreAndRank") Then
'					Me.CalculateWeightedScoreAndRank()
'					Brapi.ErrorLog.LogMessage(si, "Called Calc")
'				End If
				
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Sub

#Region "Helper Functions"		
		''-------------------------------------------------------------------------		
'		Public Function UpdateCellDefinition(ByRef destcell As DataBufferCell,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As DatabufferCell
'			Dim BufferCell = Global_Functions.UpdateCellDefinition(destcell,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
'			Return BufferCell
'	    End Function
				
'		''-------------------------------------------------------------------------
'		Public Function GetBCValue(ByRef srccell As DataBufferCell,ByRef DriverDB As DataBuffer,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As Decimal
'			Return Global_Functions.GetBCValue(srccell,DriverDB,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
'		End Function
		
'		''-------------------------------------------------------------------------
'		Public Sub UpdateValue(ByRef srcCell As DataBufferCell, ByRef currCellDB As DataBuffer, ByRef destDB As DataBuffer, ByVal value As Decimal)
'			Global_Functions.UpdateValue(srccell,currCellDB,destDB,value)
'		End Sub
'	''---------------------------------------------------------------------------------------------------
'		Private Function GetBufferValue(ByVal db As DataBuffer, ByVal targetPk As DataBufferCellPk) As Decimal
'	        For Each cell As DataBufferCell In db.DataBufferCells.Values
'	            If cell.DataBufferCellPk.AccountId = targetPk.AccountId AndAlso
'	               cell.DataBufferCellPk.UD1Id = targetPk.UD1Id AndAlso
'	               cell.DataBufferCellPk.UD2Id = targetPk.UD2Id AndAlso
'	               cell.DataBufferCellPk.UD3Id = targetPk.UD3Id AndAlso
'	               cell.DataBufferCellPk.UD4Id = targetPk.UD4Id AndAlso
'	               cell.DataBufferCellPk.UD5Id = targetPk.UD5Id Then
'	               Return cell.CellAmount
'	            End If
'	        Next
'        Return 0
'    End Function
#End Region

#Region "CalculateWeightedScoreAndRank"
'	Private Function CalculateWeightedScoreAndRank() As Object
'        Try
'			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
'	        Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
'	        Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
'	        Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
'	        Dim REQTime As String = WFYear
'	        'Dim sInternalRank As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("InternalRank")
'	        Dim sFundCenter As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("FundCenter")
	
'			'Get cat and weight
'			Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
'			priCatNameAndWeight = Me.GetCategoryAndWeight()
			
'			Dim categoryToColumnMap As Dictionary(Of String, String) = Me.GetCategoryColumnMappings()
			
'			Dim req_IDs As Object = args.CustomCalculateArgs.NameValuePairs.XFGetValue("req_IDs")
'			'Brapi.ErrorLog.LogMessage(si, "REQ_ID" & req_IDs )
'			Dim Req_ID_List As New List(Of String)()				
'			Dim ID As String = req_IDs.ToString()
			
'			If Not String.IsNullOrEmpty(ID) Then
'				Req_ID_List = ID.Split(","c).ToList()
'			End If
			
'			Brapi.ErrorLog.LogMessage(si, "Req_ID_List = " & Req_ID_List.ToString())
'			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
'			Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
'			Using connection As New SqlConnection(dbConnApp.ConnectionString)
'				connection.Open()
'				' --- Priority Table (XFC_CMD_UFR_Priority) ---
'				Dim dt_Priority As New DataTable()
'				Dim sqa3 As New SqlDataAdapter()
'				Dim sqaReaderPriority As New SQA_XFC_CMD_UFR_Priority(connection)
				
'				'Fill the DataTable With the current data From FMM_Dest_Cell
'				Dim sql As String = $"SELECT * 
'									FROM XFC_CMD_UFR_Priority
'									WHERE WFScenario_Name = @WFScenario_Name
'									AND WFCMD_Name = @WFCMD_Name
'									AND WFTime_Name = @WFTime_Name
'									AND Review_Entity = @Review_Entity
'					 					"
'				Dim paramList As New List(Of SqlParameter) From {
'			    New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
'			    New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
'			    New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
'				New SqlParameter("@Review_Entity", SqlDbType.NVarChar) With {.Value = sFundCenter}
'				}
	
'				'Brapi.ErrorLog.LogMessage(si, "HERE Count" & Req_ID_List.Count)
'				If Req_ID_List.Count > 1 Then
'			        Dim paramNames As New List(Of String)()
'			        For i As Integer = 0 To Req_ID_List.Count - 1
'			            Dim paramName As String = "@UFR_ID" & i
'			            paramNames.Add(paramName)
'			            paramList.Add(New SqlParameter(paramName, SqlDbType.NVarChar) With {.Value = Req_ID_List(i).Trim()})
'						'Brapi.ErrorLog.LogMessage(si,"REQIDLIST" & Req_ID_List(i) )
'			        Next
'			        sql &= $" AND UFR_ID IN ({String.Join(",", paramNames)})"
'			    ElseIf Req_ID_List.Count = 1 Then
'			        sql &= " AND UFR_ID = @UFR_ID"
'			        paramList.Add(New SqlParameter("@UFR_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0).Trim()})
'			    End If
'			    ' 4. Convert the list to the array your method expects
'			    Dim sqlparams As SqlParameter() = paramList.ToArray()
					
'				sqaReaderPriority.Fill_XFC_CMD_UFR_Priority_DT(si, sqa3, dt_Priority, sql, sqlparams)
'	            For Each row As DataRow In dt_Priority.Rows
'					Dim Totalweightedscore As Decimal = 0
'					' Loop through the categories from the configuration
'					For Each catAndWeight As KeyValuePair(Of String, Decimal) In priCatNameAndWeight
'		                Dim categoryMemberName As String = catAndWeight.Key 
'		                Dim categoryWeight As Decimal = catAndWeight.Value
	
'						' Use the mapping to find the correct column name for this category
'						If categoryToColumnMap.ContainsKey(categoryMemberName) Then
'							Dim scoreColumnName As String = categoryToColumnMap(categoryMemberName) ' e.g., "Cat_1_Score"
'						    Dim score As Decimal = 0D
'	                        Decimal.TryParse(row(scoreColumnName).ToString(), score)
	
'	                        If score > 0 Then
'	                          Totalweightedscore  += score * (categoryWeight / 100D)
'	                    	End If
'						End If
'		            Next
'					row("Weighted_Score") =  Totalweightedscore
'		        Next
	
'				Dim dv As DataView = dt_Priority.DefaultView
'				dv.Sort = "Weighted_Score DESC" 
				
'				Dim rank As Integer = 0
'				Dim previousScore As Decimal = Decimal.MinValue 
'				Dim rowCounter As Integer = 0
	
'				For Each rowView As DataRowView In dv
'					rowCounter += 1
'					Dim currentScore As Decimal = CDec(rowView("Weighted_Score"))
	
'					If currentScore <> previousScore Then
'					    rank = rowCounter
'					End If
	
'					' Assign the final rank to the 'Rank' column.
'					rowView("Auto_Rank") = rank
'					 rowView("Rank_Override") = 0
'					' Update the previous score for the next row's comparison.
'					previousScore = currentScore
'				Next			
'				sqaReaderPriority.Update_XFC_CMD_UFR_Priority(si,dt_Priority,sqa3)
'			End Using	
'			Return Nothing
'		Catch ex As Exception
'			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'		End Try
'	End Function
				
'	Private Function GetCategoryAndWeight() As Object
          
'        'Get the list of catagories
'        Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
'        Dim WFScenario As String = "RMW_Cycle_Config_Annual"
'        Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
'        Dim REQTime As String = WFYear
    
'        Dim sFundCenter As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("FundCenter")

'        Dim priCatMembers As List(Of MemberInfo)
'        Dim priFilter As String = "A#UFR_Priority_Cat_Weight.Children"
'        Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
'         priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
        
'        Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'        Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
'        Dim priCatNameList As New List(Of String)
'        Dim priCatName As String = ""
'        Dim priCatWeight As Decimal
'        Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
		
'        For Each pricat As MemberInfo In priCatMembers
'            priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
			
'			If (Not String.IsNullOrWhiteSpace(priCatName)) Then
'                priCatWeight = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catWeightMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCell.CellAmount    
'                priCatNameAndWeight.Add(priCat.Member.Name, priCatWeight)
'            End If
'        Next
        
'        Return priCatNameAndWeight
'		priCatNameList.Add(priCatName)
          
'	End Function	
		
'	Private Function GetCategoryColumnMappings() As Dictionary(Of String, String)
   
'		Return New Dictionary(Of String, String) From {
'		    {"UFR_Priority_Cat_1_Weight", "Cat_1_Score"},
'		    {"UFR_Priority_Cat_2_Weight", "Cat_2_Score"},
'		    {"UFR_Priority_Cat_3_Weight", "Cat_3_Score"},
'		    {"UFR_Priority_Cat_4_Weight", "Cat_4_Score"},
'		    {"UFR_Priority_Cat_5_Weight", "Cat_5_Score"},
'		    {"UFR_Priority_Cat_6_Weight", "Cat_6_Score"},
'		    {"UFR_Priority_Cat_7_Weight", "Cat_7_Score"},
'		    {"UFR_Priority_Cat_8_Weight", "Cat_8_Score"},
'		    {"UFR_Priority_Cat_9_Weight", "Cat_9_Score"},
'		    {"UFR_Priority_Cat_10_Weight", "Cat_10_Score"},
'		    {"UFR_Priority_Cat_11_Weight", "Cat_11_Score"},
'		    {"UFR_Priority_Cat_12_Weight", "Cat_12_Score"},
'		    {"UFR_Priority_Cat_13_Weight", "Cat_13_Score"},
'		    {"UFR_Priority_Cat_14_Weight", "Cat_14_Score"},
'		    {"UFR_Priority_Cat_15_Weight", "Cat_15_Score"}
'		}
'	End Function
		 
			 
			 
			 
			 
			 
				
#End Region		

		
	End Class
End Namespace