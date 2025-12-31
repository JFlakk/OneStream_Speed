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
			If api.Pov.Entity.Name.XFContainsIgnoreCase("None") Then Return
			'Load_Type Param - Status Updates, New Req Creation, Rollover, Mpr Req Creation, Copy
			Dim wfStartTimeID As Integer = api.Scenario.GetWorkflowStartTime(api.Pov.Scenario.MemberId)
			Dim wfStartYear As String = api.Time.GetNameFromId(wfStartTimeID)
		    Dim table_FY = api.Time.GetYearFromId(api.Time.GetIdFromName(api.Pov.Time.Name)) - wfStartYear.XFConvertToInt + 1
			Dim inClause As String
			Dim parentFCList As New List(Of String)
			Dim baseFCList As New List(Of String)
			
			Dim tgt_Origin As String = "Import"
			Dim origin_Filter As String = "O#Import"
			Dim entDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "E_Army")
			Dim entityID As Integer = api.Members.GetMemberId(dimType.Entity.Id,api.Pov.Entity.Name)
			
			Dim Ent_Base = Not BRApi.Finance.Members.HasChildren(si, entDimPk,entityID)
			
			If Ent_Base = False Then
				tgt_Origin = "AdjInput"
				origin_Filter = "O#AdjInput,O#AdjConsolidated,O#Import"
				If (globals.GetObject($"{api.pov.entity.name}_baseFCList") Is Nothing) Then
					Dim entList = BRApi.Finance.Members.GetMembersUsingFilter(si, entDimPk, $"E#{api.Pov.Entity.Name}.Descendants", True)
					For Each ent As MemberInfo In entList
						If Not BRApi.Finance.Members.HasChildren(si, entDimPk,ent.Member.MemberId)
							baseFCList.Add(ent.Member.Name)
						Else
							parentFCList.Add(ent.Member.Name)
						End If
					Next
					If parentFCList.Count > 0
						globals.SetObject($"{api.pov.entity.name}_parentFCList",parentFCList)
					End If
					If baseFCList.Count > 0
						globals.SetObject($"{api.pov.entity.name}_baseFCList",baseFCList)
					End If
				Else
					baseFCList = globals.GetObject($"{api.pov.entity.name}_baseFCList")
					If Not (globals.GetObject($"{api.pov.entity.name}_parentFCList") Is Nothing) Then
						parentFCList = globals.GetObject($"{api.pov.entity.name}_parentFCList")
					End If
				End If
			End If
			
			
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
				inClause = $"'{StatusbyFundsCenter}'"
				StatusbyFundsCenter = $",[{StatusbyFundsCenter}]"
			End If
			Dim finalUD1WhereClause As String
			Dim appnList As New List(Of String) 
			If appnbyFundsCenter.xfcontainsIgnoreCase("|")
				
				appnList = StringHelper.SplitString(appnbyFundsCenter,"|")
				Dim prefixedAppn As List(Of String) = appnList.Select(Function(appn) $"U1#{appn}").ToList()
				Dim appnUD1whereList As New List(Of String)
				appnUD1whereList = appnList.Select(Function(appn) $"UD1 like '{appn}%'").ToList()

				finalUD1WhereClause = $"({String.Join(" OR ", appnUD1whereList)})"
	
				If appnbyFundsCenter <> String.Empty
					appnbyFundsCenter = $",[{String.Join(", ", prefixedAppn)}]"
				End If
			Else
				finalUD1WhereClause = $"UD1 like '{appnbyFundsCenter}%'"
				appnbyFundsCenter = $",[U1#{appnbyFundsCenter}]"
			End If
			Dim CurrCubeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(REMOVEZeros(V#Periodic),[origin_Filter]{StatusbyFundsCenter}{appnbyFundsCenter})")
			Dim destBuffer As DataBuffer = New DataBuffer()
			Dim ClearCubeData As DataBuffer = New DataBuffer()
'BRApi.ErrorLog.LogMessage(si, $"CurrCubeBuffer script: FilterMembers(REMOVEZeros(V#Periodic),[origin_Filter]{StatusbyFundsCenter})")
			'Define the SQL Statement 
			Dim sql = $"SELECT Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, '{tgt_origin}' as Origin, SUM(FY_{table_FY}) AS Tot_Annual
						FROM XFC_CMD_PGM_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						AND Entity = '{api.Pov.Entity.Name}'
						AND Account In ('Req_Funding')
						AND Flow In ({inClause})
						AND ({finalUD1WhereClause})
						Group By Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			
			If parentFCList.Count > 0 Then
				Dim parentFCFilter = $"('{String.Join("','", parentFCList)}')"
				sql = $"{sql} UNION ALL
						SELECT '{api.Pov.Entity.Name}' as Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, 'AdjConsolidated' as Origin, SUM(FY_{table_FY}) AS Tot_Annual
						FROM XFC_CMD_PGM_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						AND Entity In {parentFCFilter}
						AND Account In ('Req_Funding')
						AND Flow In ({inClause})
						AND ({finalUD1WhereClause})
						Group By Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			End If
			If baseFCList.Count > 0 Then
				Dim baseFCFilter = $"('{String.Join("','", baseFCList)}')"
				sql = $"{sql} UNION ALL
						SELECT '{api.Pov.Entity.Name}' as Entity, Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, 'Import' as Origin, SUM(FY_{table_FY}) AS Tot_Annual
						FROM XFC_CMD_PGM_REQ_Details
						WHERE WFScenario_Name = '{api.Pov.Scenario.Name}'
						AND Entity In {baseFCFilter}
						AND Account In ('Req_Funding')
						AND Flow In ({inClause})
						AND ({finalUD1WhereClause})
						Group By Account, IC, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8"
			End If

'If table_FY = 1 Then
'brapi.ErrorLog.LogMessage(si,"hit SQL PGM: " & sql)
'End If 
			
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
							ReqDataCell.DataBufferCellPk.SetOrigin(api, reader("Origin").ToString())
							ReqDataCell.DataBufferCellPk.SetUD1(api, reader("UD1").ToString())
							ReqDataCell.DataBufferCellPk.SetUD2(api, reader("UD2").ToString())
							ReqDataCell.DataBufferCellPk.SetUD3(api, reader("UD3").ToString())
							ReqDataCell.DataBufferCellPk.SetUD4(api, reader("UD4").ToString())
							ReqDataCell.DataBufferCellPk.SetUD5(api, reader("UD5").ToString())
							ReqDataCell.DataBufferCellPk.SetUD6(api, reader("UD6").ToString())
							ReqDataCell.DataBufferCellPk.SetUD7(api, reader("UD7").ToString())
							ReqDataCell.DataBufferCellPk.SetUD8(api, reader("UD8").ToString())
	
							If Not IsDBNull(reader("Tot_Annual")) AndAlso reader("Tot_Annual") <> 0 Then
								UpdateValue(ReqDataCell,CurrCubeBuffer,destbuffer,Convert.ToDecimal(reader("Tot_Annual")))
								CurrCubeBuffer.DataBufferCells.Remove(ReqDataCell.DataBufferCellPk)
							End If
						End While	
					End If
				End Using
			End Using

			Dim destInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("V#Periodic")
'brapi.ErrorLog.LogMessage(si,"**Hit count: " & destBuffer.DataBufferCells.Count)
			If destBuffer.DataBufferCells.Count > 0 Then
'CurrCubeBuffer.LogDataBuffer(api,"@ CurrCubeBuffer: " & api.Pov.Entity.Name & " : "  & api.Pov.Time.Name ,1000)	
'destBuffer.LogDataBuffer(api,"@ destBuffer: " & api.Pov.Entity.Name & " : "  & api.Pov.Time.Name ,1000)
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

				Dim sSrcMbrScript = $"FilterMembers(RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+1}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+2}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+3}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top +
												  E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear+4}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC])"

				Dim dbFundingLines = api.Data.GetDataBufferUsingFormula(sSrcMbrScript)
				
                Dim mpr_buffers As New Dictionary(Of String,DataBuffer)

                Dim BufferFY1Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{sTargetYear}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY1 = api.Data.GetDataBufferUsingFormula(BufferFY1Script)
                Dim BufferFY2Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+1)}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY2 = api.Data.GetDataBufferUsingFormula(BufferFY2Script)
                Dim BufferFY3Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+2)}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY3 = api.Data.GetDataBufferUsingFormula(BufferFY3Script)
                Dim BufferFY4Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+3)}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
                Dim BufferFY4 = api.Data.GetDataBufferUsingFormula(BufferFY4Script)
                Dim BufferFY5Script As String = $"RemoveZeros(E#{wfInfoDetails("CMDName")}:S#{sSourcePosition}:T#{(sTargetYear+4)}:C#Aggregated:V#Periodic:F#Top:O#Top:I#Top:U6#Top:U7#Top:U8#Top),[A#BO6,A#BOC]"
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
'Brapi.ErrorLog.LogMessage(si,"SQL" & SqlREQDetail)
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
'dbfundinglines.LogDataBuffer(api,"manpower test",1000)
	'Brapi.ErrorLog.LogMessage(si,"HERE 0")			
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
'Brapi.ErrorLog.LogMessage(si,"HERE1")
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
																	AND UD5 = '{FundingLineCell.DataBufferCellPk.GetUD5Name(api)}'"
																	).FirstOrDefault()
				
						If IsNothing(row) Then
							isinsert = True
							row = REQDetailDT.NewRow()
						End If 
	
						row("WFScenario_Name") = api.pov.Scenario.Name
						row("WFTime_Name") = api.Pov.Time.Name
						row("CMD_PGM_REQ_ID") = req_id_val
						row("WFCMD_Name") = api.Pov.Cube.Name
						row("Unit_of_Measure") = UoM 
						row("Entity") = api.Pov.Entity.Name
						row("IC") = "None"
						row("Account") = Acct
						row("Flow") = "L2_Formulate_PGM"
						row("Start_Year") = api.Pov.Time.Name
						row("FY_1") = GetBCValue(FundingLineCell,BufferFY1,cPROBE_Acct)
						row("FY_2") = GetBCValue(FundingLineCell,BufferFY2,cPROBE_Acct)
						row("FY_3") = GetBCValue(FundingLineCell,BufferFY3,cPROBE_Acct)	
						row("FY_4") = GetBCValue(FundingLineCell,BufferFY4,cPROBE_Acct)	
						row("FY_5") = GetBCValue(FundingLineCell,BufferFY5,cPROBE_Acct)
						row("FY_Total") = row("FY_1") + row("FY_2") + row("FY_3") + row("FY_4") + row("FY_5")
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


'@Devlin - Let's look at this as you proceed with UFR...  We don't want this to be part of the Finance Calcs and have to use a DM job to run, needs to be part of a DB Extender, less system OH.

#Region "CalculateWeightedScoreAndRank"
	Private Function CalculateWeightedScoreAndRank() As Object
        Try
			Dim wfInfoDetails = Workspace.GBL.GBL_Assembly.GBL_Helpers.GetWFInfoDetails(si)
			Dim wfProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
            Dim WFCube = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).CubeName   
            Dim WFScenario As String = ScenarioDimHelper.GetNameFromId(si, si.WorkflowClusterPk.ScenarioKey)
            Dim WFYear As Integer = TimeDimHelper.GetYearFromId(si.WorkflowClusterPk.TimeKey)   
            Dim REQTime As String = WFYear
            Dim sFundCenter As String =  args.CustomCalculateArgs.NameValuePairs.XFGetValue("FundCenter")
   
            'Get cat and weight
            Dim priCatNameAndWeight As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
            priCatNameAndWeight = Me.GetCategoryAndWeight()
				
			Dim categoryToColumnMap As Dictionary(Of String, String) = Me.GetCategoryColumnMappings()
			
			Dim req_IDs As Object = args.CustomCalculateArgs.NameValuePairs.XFGetValue("req_IDs")
			
			Dim Req_ID_List As New List(Of String)()				
				
		    Dim ID As String = req_IDs.ToString()
		    If Not String.IsNullOrEmpty(ID) Then		       
		        Req_ID_List = ID.Split(","c).ToList()
		    End If
				
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
										AND Review_Entity = @Review_Entity
					 					"
					

		Dim paramList As New List(Of SqlParameter) From {
        New SqlParameter("@WFScenario_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("ScenarioName")},
        New SqlParameter("@WFCMD_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("CMDName")},
        New SqlParameter("@WFTime_Name", SqlDbType.NVarChar) With {.Value = wfInfoDetails("TimeName")},
		New SqlParameter("@Review_Entity", SqlDbType.NVarChar) With {.Value = sFundCenter}
   		}

					'Brapi.ErrorLog.LogMessage(si, "HERE Count" & Req_ID_List.Count)
					If Req_ID_List.Count > 1 Then
				        Dim paramNames As New List(Of String)()
				        For i As Integer = 0 To Req_ID_List.Count - 1
				            Dim paramName As String = "@REQ_ID" & i
				            paramNames.Add(paramName)
				            paramList.Add(New SqlParameter(paramName, SqlDbType.NVarChar) With {.Value = Req_ID_List(i).Trim()})
							'Brapi.ErrorLog.LogMessage(si,"REQIDLIST" & Req_ID_List(i) )
				        Next
				        sql &= $" AND REQ_ID IN ({String.Join(",", paramNames)})"
				    ElseIf Req_ID_List.Count = 1 Then
				        sql &= " AND REQ_ID = @REQ_ID"
				        paramList.Add(New SqlParameter("@REQ_ID", SqlDbType.NVarChar) With {.Value = Req_ID_List(0).Trim()})
				    End If
				'Brapi.ErrorLog.LogMessage(si,"SQL: " & sql)
				    ' 4. Convert the list to the array your method expects
				    Dim sqlparams As SqlParameter() = paramList.ToArray()
					
					
					sqaReaderPriority.Fill_XFC_CMD_PGM_REQ_Priority_DT(si, sqa3, dt_Priority, sql, sqlparams)
'Brapi.ErrorLog.LogMessage(si, "REQID" &  dt_Priority.Rows.Count )
                For Each row As DataRow In dt_Priority.Rows
               ' Brapi.ErrorLog.LogMessage(si, "REQID" )
               
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
     rowView("Rank_Override") = 0
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
                
                Dim catNameMemScript As String   = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Annotation:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
                Dim catWeightMemScript As String = "Cb#" & WFCube & ":E#" & sFundCenter & ":C#Local:S#" & WFScenario & ":T#" & REQTime & ":V#Periodic:A#UUUU:F#None:O#AdjInput:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
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
	''---------------------------------------------------------------------------------------------------
		 Private Function GetBufferValue(ByVal db As DataBuffer, ByVal targetPk As DataBufferCellPk) As Decimal
        
        For Each cell As DataBufferCell In db.DataBufferCells.Values
            If cell.DataBufferCellPk.AccountId = targetPk.AccountId AndAlso
               cell.DataBufferCellPk.UD1Id = targetPk.UD1Id AndAlso
               cell.DataBufferCellPk.UD2Id = targetPk.UD2Id AndAlso
               cell.DataBufferCellPk.UD3Id = targetPk.UD3Id AndAlso
               cell.DataBufferCellPk.UD4Id = targetPk.UD4Id AndAlso
               cell.DataBufferCellPk.UD5Id = targetPk.UD5Id Then
               
               Return cell.CellAmount
            End If
        Next
        
        
        Return 0
    End Function
#End Region
			
				
	End Class

	
	
	
	
End Namespace