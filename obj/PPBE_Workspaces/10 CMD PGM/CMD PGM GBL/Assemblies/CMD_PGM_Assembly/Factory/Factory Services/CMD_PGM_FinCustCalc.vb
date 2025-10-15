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
	Public Class CMD_PGM_FinCustCalc
		Implements IWsasFinanceCustomCalculateV800
		
		Public si As SessionInfo
		Public globals As BRGlobals
		Public api As FinanceRulesApi
		Public args As FinanceRulesArgs
		Public Global_Functions As OneStream.BusinessRule.Finance.Global_Functions.MainClass

        Public Sub CustomCalculate(ByVal si As SessionInfo, ByVal Globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) Implements IWsasFinanceCustomCalculateV800.CustomCalculate
            Try
#Region "General Initialization"
				Me.si = si
				Me.api = api
				Me.args = args
				Me.globals = globals
				Me.Global_Functions = New OneStream.BusinessRule.Finance.Global_Functions.MainClass(si,globals,api,args)
#End Region

				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Load_Reqs_to_Cube") Then				
					Me.Load_Reqs_to_Cube()
				End If
				
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Copy_Manpower") Then
					'brapi.ErrorLog.LogMessage(si,"hit CM and KL")
					Me.Copy_Manpower(globals)
				End If
			
				If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateWeightedScoreAndRank") Then
					
					Me.CalculateWeightedScoreAndRank()
				End If

            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Sub

#Region "Load_Reqs_to_Cube"
		Public Sub Load_Reqs_to_Cube()
Brapi.ErrorLog.LogMessage(si,"@HERE1 Load_Reqs_to_Cube")
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			Dim POVPeriodNum As Integer = api.Time.GetPeriodNumFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim POVYear As Integer = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name))
			Dim wfStartTimeID As Integer = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId)
			Dim wfStartYear As String = api.Time.GetNameFromId(wfStartTimeID)
		    Dim table_FY = POVYear - wfStartYear.XFConvertToInt + 1
			Dim inClause As String
				
			Dim StatusbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusUpdates - {api.pov.entity.name}",String.Empty)
			Dim appnbyFundsCenter As String = Globals.GetStringValue($"FundsCenterStatusappnUpdates - {api.pov.entity.name}",String.Empty)
			Dim StatusList As New List(Of String) 
			If StatusbyFundsCenter.xfcontainsIgnoreCase("|")
				
				StatusList = StringHelper.SplitString(StatusbyFundsCenter,"|")
				Dim prefixedStatuses As List(Of String) = StatusList.Select(Function(status) $"F#{status}").ToList()
				inClause = String.Join(",", StatusList.Select(Function(status) $"'{status}'"))
	
				
				If StatusbyFundsCenter <> String.Empty
					StatusbyFundsCenter = $",[{String.Join(", ", prefixedStatuses)}]"
				End If
			Else
				
			End If
			
			brapi.ErrorLog.LogMessage(si,$"zHit: {StatusbyFundsCenter}")
	
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[O#Import]{StatusbyFundsCenter})")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
	
			'Define the SQL Statement 
			Dim sql = $"SELECT Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, SUM(FY_{table_FY}) AS Tot_Annual
						FROM XFC_CMD_PGM_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						AND Entity = '{api.Pov.Entity.Name}'
						AND Account In ('Req_Funding')
						AND Flow In ({inClause})
						Group By Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
	'brapi.ErrorLog.LogMessage(si,"Hit SQL: ")
	brapi.ErrorLog.LogMessage(si,"Hit SQL: " & sql.ToString)
	
			Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using reader As DataTableReader = BRApi.Database.ExecuteSqlUsingReader(dbConnApp, sql, False).CreateDataReader
					If reader.HasRows Then
						While reader.Read()
							Dim ReqDataCellpk As New DataBufferCellPk()
							Dim Status As New DataCellStatus(True)
							Dim ReqDataCell As New DataBufferCell(ReqDataCellpk, 0, Status)
	
							ReqDataCell.DataBufferCellPk.SetAccount(api, reader("Account").ToString())
							ReqDataCell.DataBufferCellPk.SetFlow(api, reader("Flow").ToString())
							ReqDataCell.DataBufferCellPk.SetIC(api, reader("IC").ToString())
							ReqDataCell.DataBufferCellPk.SetOrigin(api, "Import")
							ReqDataCell.DataBufferCellPk.SetUD1(api, reader("UD1").ToString())
							ReqDataCell.DataBufferCellPk.SetUD2(api, reader("UD2").ToString())
							ReqDataCell.DataBufferCellPk.SetUD3(api, reader("UD3").ToString())
							ReqDataCell.DataBufferCellPk.SetUD4(api, reader("UD4").ToString())
							ReqDataCell.DataBufferCellPk.SetUD5(api, reader("UD5").ToString())
							ReqDataCell.DataBufferCellPk.SetUD6(api, reader("UD6").ToString())
							ReqDataCell.DataBufferCellPk.SetUD7(api, reader("UD7").ToString())
							ReqDataCell.DataBufferCellPk.SetUD8(api, reader("UD8").ToString())
	
							If Not IsDBNull(reader("Tot_Annual")) AndAlso reader("Tot_Annual") <> 0 Then
								ReqDataCell.CellAmount = Convert.ToDecimal(reader("Tot_Annual"))
								destBuffer.DataBufferCells.Add(ReqDataCell.DataBufferCellPk, ReqDataCell)
								CurrCubeBuffer.DataBufferCells.Remove(ReqDataCell.DataBufferCellPk)
							End If
						End While	
					End If
				End Using
			End Using

			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
'brapi.ErrorLog.LogMessage(si,"**Hit count: " & destBuffer.DataBufferCells.Count)
			If destBuffer.DataBufferCells.Count > 0 Then
	
destBuffer.LogDataBuffer(api,"@ DataBuffer: " & api.Pov.Entity.Name & " : "  & api.Pov.Time.Name ,1000)
				api.Data.SetDataBuffer(destBuffer, destInfo,,,,,,,,,,,,, True)
				destBuffer.DataBufferCells.Clear()
			End If

			For Each ClearCubeCell As DataBufferCell In CurrCubeBuffer.DataBufferCells.Values
				Dim Status As New DataCellStatus(False)
				Dim ClearCell As New DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, Status)
				ClearCubeData.SetCell(si, ClearCell)
			Next
	
			Dim ClearInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
			If ClearCubeData.DataBufferCells.Count > 0 Then
				api.Data.SetDataBuffer(ClearCubeData, ClearInfo)
			End If
		
		End Sub
#End Region

#Region "Copy_Manpower"
'Updated 12/03/2024 by Fronz - changed BO5 to BOC & added the 'Clear Existing Data' lines of code
		Public Sub Copy_Manpower(ByVal globals As BRGlobals)
			Try		
				Dim sSourcePosition As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourcePB")
				Dim req_id_val As guid = args.CustomCalculateArgs.NameValuePairs.XFGetValue("REQ_ID_VAL").XFConvertToGuid
				Dim sTargetFirstYear As String = api.Time.GetNameFromId(si.WorkflowClusterPk.TimeKey)
				Dim sTargetYear As Integer = api.Pov.Time.Name.XFConvertToInt
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)

				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+1}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+2}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+3}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+4}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC])"

				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

                Dim BufferFY1Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY1 = api.Data.GetDataBufferUsingFormula(BufferFY1Script)
                Dim BufferFY2Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+1)}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY2 = api.Data.GetDataBufferUsingFormula(BufferFY2Script)
                Dim BufferFY3Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+2)}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY3 = api.Data.GetDataBufferUsingFormula(BufferFY3Script)
                Dim BufferFY4Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+3)}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY4 = api.Data.GetDataBufferUsingFormula(BufferFY4Script)
                Dim BufferFY5Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+4)}:C#Local:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY5 = api.Data.GetDataBufferUsingFormula(BufferFY5Script)
			
						
				Dim REQDetailDT As DataTable = New DataTable("REQDetailDT")
				 
				Dim sqa As New SqlDataAdapter()
		        Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
		        	Using sqlConn As New SqlConnection(dbConnApp.ConnectionString)
		            	sqlConn.Open()
					    'Details
					    Dim sqaREQDetailReader As New SQA_XFC_CMD_PGM_REQ_Details(sqlConn)
						Dim SqlREQDetail As String = $"SELECT * 
								     				   FROM XFC_CMD_PGM_REQ_Details
											           WHERE WFCMD_Name = '{api.pov.Cube.Name}'
											           And WFTime_Name = '{api.pov.time.Name}'
											           AND WFScenario_Name = '{api.pov.Scenario.Name}'
											           AND ENTITY = '{api.pov.Entity.Name}'
						 					           and CMD_PGM_REQ_ID = '{req_id_val}'"
						Dim sqlparamsREQDetails As SqlParameter() = New SqlParameter() {}
						sqaREQDetailReader.Fill_XFC_CMD_PGM_REQ_Details_DT(sqa, REQDetailDT, SqlREQDetail, sqlparamsREQDetails)
						REQDetailDT.PrimaryKey = New DataColumn() {
												 REQDetailDT.Columns("CMD_PGM_REQ_ID"),
												 REQDetailDT.Columns("Start_Year"),
										         REQDetailDT.Columns("Unit_of_Measure"),
												 REQDetailDT.Columns("UD1"),
												 REQDetailDT.Columns("UD2"),
												 REQDetailDT.Columns("UD3"),
												 REQDetailDT.Columns("UD4"),
												 REQDetailDT.Columns("UD5")}

'Dim logBuilder As New StringBuilder()
'For Each drow As DataRow In REQDetailDT.Rows
'            For Each column As DataColumn In REQDetailDT.Columns
'                logBuilder.Append(drow(column).ToString() & vbTab)
'            Next
'            logBuilder.AppendLine()
'Next
'brapi.ErrorLog.LogMessage(si,"after copy CM Rows = " & logBuilder.tostring())			 
												 
												 
												 
												 'sSrcMbrScript log													 
'Brapi.ErrorLog.LogMessage(si,"Mem script:" + sSrcMbrScript)				
'buffer log								 
dbfundinglines.LogDataBuffer(api,"manpower test",1000)
				
				For Each FundingLineCell As DataBufferCell In dbFundingLines.DataBufferCells.values
					Dim Acct = "CIV_FTE"
					Dim UoM = "CIV_FTE"
					Dim cPROBE_Acct = "BO6"

					If 	FundingLineCell.DataBufferCellPk.GetAccountName(api) = "BOC" Then
							Acct = "Req_Funding"
							UoM = "CIV_COST"
							cPROBE_Acct = "BOC"
					End If 
						Dim isinsert As Boolean = False

						Dim row As DataRow = REQDetailDT.Select($"WFScenario_Name = '{api.pov.Scenario.Name}' 
																	AND Account = '{Acct}'
																	AND CMD_PGM_REQ_ID = '{req_id_val}' 
																	AND WFCMD_Name = '{api.pov.cube.name}' 
																	AND Unit_of_Measure = '{UoM}' 
																	AND Entity = '{api.pov.entity.name}'
																	AND UD1 = '{FundingLineCell.DataBufferCellPk.GetUD1Name(api)}'
																	AND UD2 = '{FundingLineCell.DataBufferCellPk.GetUD2Name(api)}'
																	AND UD3 = '{FundingLineCell.DataBufferCellPk.GetUD3Name(api)}'
																	AND UD4 = '{FundingLineCell.DataBufferCellPk.GetUD4Name(api)}'
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'").FirstOrDefault()
				
						If IsNothing(row) Then
							isinsert = True
							row = REQDetailDT.NewRow()
						End If 
	
						row("WFScenario_Name") = api.pov.Scenario.Name
						row("WFTime_Name") = api.Pov.Time.Name
						row("CMD_PGM_REQ_ID") = req_id_val
						row("WFCMD_Name") = api.Pov.Cube.Name
						row("Unit_of_Measure") = UoM 
						Row("Entity") = api.Pov.Entity.Name
						Row("IC") = "None"
						Row("Account") = Acct
						Row("Flow") = "L2_Formulate_PGM"
						Row("Start_Year") = api.Pov.Time.Name
						Row("FY_1") = GetBCValue(FundingLineCell,BufferFY1,cPROBE_Acct)
						Row("FY_2") = GetBCValue(FundingLineCell,BufferFY2,cPROBE_Acct)
						Row("FY_3") = GetBCValue(FundingLineCell,BufferFY3,cPROBE_Acct)	
						Row("FY_4") = GetBCValue(FundingLineCell,BufferFY4,cPROBE_Acct)	
						Row("FY_5") = GetBCValue(FundingLineCell,BufferFY5,cPROBE_Acct)
						Row("FY_Total") = Row("FY_1") + Row("FY_2") + Row("FY_3") + Row("FY_4") + Row("FY_5")
						row("UD1") = FundingLineCell.DataBufferCellPk.GetUD1Name(api)
						row("UD2") = FundingLineCell.DataBufferCellPk.GetUD2Name(api)
						row("UD3") = FundingLineCell.DataBufferCellPk.GetUD3Name(api)
						row("UD4") = FundingLineCell.DataBufferCellPk.GetUD4Name(api)
						row("UD5") = FundingLineCell.DataBufferCellPk.GetUD5Name(api)
						row("UD6") = "Pay_General"
						row("UD7") = "None"
						row("UD8") = "None"
						row("Create_Date") = DateTime.Now
						row("Create_User") = si.UserName
						row("Update_Date") = DateTime.Now
						row("Update_User") = si.UserName
					
						If isinsert Then
							REQDetailDT.Rows.Add(row)								
						End If 
'					Next
				Next
	
					'create a new Data row for req and detail
					sqaREQDetailReader.Update_XFC_CMD_PGM_REQ_Details(REQDetailDT, sqa)	
				 	End Using
				 End Using
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try			 
		End Sub
					
#End Region

#Region "CalculateWeightedScoreAndRank"
Private Function CalculateWeightedScoreAndRank() As Object
            Try
				Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
                Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
                Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
                Dim REQTime As String = WFYear
                'Dim sInternalRank As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("InternalRank")
                Dim sFundCenter As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("FundCenter")
   
                'Get cat and weight
                Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
                priCatNameAndWeight = Me.GetCategoryAndWeight()
				
				 Dim categoryToColumnMap As Dictionary(Of String, String) = Me.GetCategoryColumnMappings()
			
				Dim req_IDs As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("req_IDs")
				Brapi.ErrorLog.LogMessage(si, "REQId" & req_IDs)
				Dim ReqID_Guid As Guid = req_IDs.XFConvertToGuid()
      			  Dim RedID_Str As String = ReqID_Guid.ToString()
				Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
				
	
					Dim dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si)
					Using connection As New SqlConnection(dbConnApp.ConnectionString)
						connection.Open()
						' --- Priority Table (XFC_CMD_PGM_REQ_Priority) ---
           			 Dim dt_Priority As New DataTable()
           			 Dim sqa3 As New SqlDataAdapter()
            		Dim sqaReaderPriority As New SQA_XFC_CMD_PGM_REQ_Priority(connection)
					
            
					'Fill the DataTable With the current data From FMM_Dest_Cell
					Dim sql As String = $"SELECT * 
										FROM XFC_CMD_PGM_REQ_Priority
										WHERE WFScenario_Name = @WFScenario_Name
										AND WFCMD_Name = @WFCMD_Name
										AND WFTime_Name = @WFTime_Name
					 					"
					

					Dim sqlparams As SqlParameter() = {
						New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
						New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
						New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")}			
					}
					sqaReaderPriority.Fill_XFC_CMD_PGM_REQ_Priority_DT(si, sqa3, dt_Priority, sql, sqlParams)

                For Each row As DataRow In dt_Priority.Rows
                
               
				Dim Totalweightedscore As Decimal = 0
				 ' Loop through the categories from the configuration
                For Each catAndWeight As KeyValuePair(Of String, Decimal) In priCatNameAndWeight
                    Dim categoryMemberName As String = catAndWeight.Key 
                    Dim categoryWeight As Decimal = catAndWeight.Value

                    ' Use the mapping to find the correct column name for this category
                    If categoryToColumnMap.ContainsKey(categoryMemberName) Then
                        Dim scoreColumnName As String = categoryToColumnMap(categoryMemberName) ' e.g., "Cat_1_Score"
                            Dim score As Decimal = 0D
                            
                            Decimal.TryParse(row(scoreColumnName).ToString(), score)

                            If score > 0 Then
                              Totalweightedscore  += score * (categoryWeight / 100D)
							   
                        	End If
						End If
					
					
                Next
	
	row("Weighted_Score") =  Totalweightedscore
               
            Next

Dim dv As DataView = dt_Priority.DefaultView
dv.Sort = "Weighted_Score DESC" 

Dim rank As Integer = 0
Dim previousScore As Decimal = Decimal.MinValue 
Dim rowCounter As Integer = 0

For Each rowView As DataRowView In dv
    rowCounter += 1
    Dim currentScore As Decimal = CDec(rowView("Weighted_Score"))

    If currentScore <> previousScore Then
        rank = rowCounter
    End If

    ' Assign the final rank to the 'Rank' column.
    rowView("Auto_Rank") = rank
    
    ' Update the previous score for the next row's comparison.
    previousScore = currentScore
Next			
		
sqaReaderPriority.Update_XFC_CMD_PGM_REQ_Priority(si,dt_Priority,sqa3)
           
		   End Using	
            Return Nothing
     
    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
End Function
				
	Private Function GetCategoryAndWeight() As Object
          
                'Get the list of catagories
                Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
                Dim WFScenario As String = "RMW_Cycle_Config_Annual"
                Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
                Dim REQTime As String = WFYear
            
                Dim sFundCenter As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("FundCenter")

                Dim priCatMembers As List(Of MemberInfo)
                Dim priFilter As String = "A#GBL_Priority_Cat_Weight.Children"
                Dim priCatDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "A_Admin")
                 priCatMembers = BRApi.Finance.Members.GetMembersUsingFilter(si, priCatDimPk, priFilter, True)
                
                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim priCatNameList As New List(Of String)
                Dim priCatName As String = ""
                Dim priCatWeight As Decimal
                
                Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
                For Each pricat As MemberInfo In priCatMembers
                    priCatName = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catNameMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCellAnnotation 
				If (Not String.IsNullOrWhiteSpace(priCatName)) Then
                        priCatWeight = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, WFCube, catWeightMemScript.Replace("UUUU",priCat.Member.Name)).DataCellEx.DataCell.CellAmount    

                        priCatNameAndWeight.Add(priCat.Member.Name, priCatWeight)
                    End If
                Next
                
                Return priCatNameAndWeight
				priCatNameList.Add(priCatName)
                  
             End Function	
		
	Private Function GetCategoryColumnMappings() As Dictionary(Of String, String)
   
    Return New Dictionary(Of String, String) From {
        {"GBL_Priority_Cat_1_Weight", "Cat_1_Score"},
        {"GBL_Priority_Cat_2_Weight", "Cat_2_Score"},
        {"GBL_Priority_Cat_3_Weight", "Cat_3_Score"},
        {"GBL_Priority_Cat_4_Weight", "Cat_4_Score"},
        {"GBL_Priority_Cat_5_Weight", "Cat_5_Score"},
        {"GBL_Priority_Cat_6_Weight", "Cat_6_Score"},
        {"GBL_Priority_Cat_7_Weight", "Cat_7_Score"},
        {"GBL_Priority_Cat_8_Weight", "Cat_8_Score"},
        {"GBL_Priority_Cat_9_Weight", "Cat_9_Score"},
        {"GBL_Priority_Cat_10_Weight", "Cat_10_Score"},
        {"GBL_Priority_Cat_11_Weight", "Cat_11_Score"},
        {"GBL_Priority_Cat_12_Weight", "Cat_12_Score"},
        {"GBL_Priority_Cat_13_Weight", "Cat_13_Score"},
        {"GBL_Priority_Cat_14_Weight", "Cat_14_Score"},
        {"GBL_Priority_Cat_15_Weight", "Cat_15_Score"}
    }
End Function
		 
			 
			 
			 
			 
			 
				
#End Region		

#Region "Helper Functions"		
		''-------------------------------------------------------------------------		
		Public Function UpdateCellDefinition(ByRef destcell As DataBufferCell,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As DatabufferCell
		
			Dim BufferCell = Global_Functions.UpdateCellDefinition(destcell,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
			Return BufferCell
			
	    End Function
				
		''-------------------------------------------------------------------------
		Public Function GetBCValue(ByRef srccell As DataBufferCell,ByRef DriverDB As DataBuffer,Optional ByRef DriverDB_Acct As String = "NoPassedValue",Optional ByRef DriverDB_Flow As String = "NoPassedValue",Optional ByRef DriverDB_Origin As String = "NoPassedValue",Optional ByRef DriverDB_IC As String = "NoPassedValue",Optional ByRef DriverDB_UD1 As String = "NoPassedValue",Optional ByRef DriverDB_UD2 As String = "NoPassedValue",Optional ByRef DriverDB_UD3 As String = "NoPassedValue",Optional ByRef DriverDB_UD4 As String = "NoPassedValue",Optional ByRef DriverDB_UD5 As String = "NoPassedValue",Optional ByRef DriverDB_UD6 As String = "NoPassedValue",Optional ByRef DriverDB_UD7 As String = "NoPassedValue",Optional ByRef DriverDB_UD8 As String = "NoPassedValue") As Decimal
			
			Return Global_Functions.GetBCValue(srccell,DriverDB,DriverDB_Acct,DriverDB_Flow,DriverDB_Origin,DriverDB_IC,DriverDB_UD1,DriverDB_UD2,DriverDB_UD3,DriverDB_UD4,DriverDB_UD5,DriverDB_UD6,DriverDB_UD7,DriverDB_UD8)
			
		End Function
		
		''-------------------------------------------------------------------------
		Public Sub UpdateValue(ByRef srcCell As DataBufferCell, ByRef currCellDB As DataBuffer, ByRef destDB As DataBuffer, ByVal value As Decimal)
			
			Global_Functions.UpdateValue(srccell,currCellDB,destDB,value)

		End Sub
		
#End Region
			
				
	End Class

	
	
	
	
End Namespace
