using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	public class FMM_DB_Svc : IWsasDashboardV800
	{
        public XFLoadDashboardTaskResult ProcessLoadDashboardTask(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
        {
            try
            {
                if ((brGlobals != null) && (workspace != null) && (args?.LoadDashboardTaskInfo != null))
                {
                    if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
                    {
                        // Implement Load Dashboard logic here.
                        if ((args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize) && (args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters))
                        {
                            var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
                            loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
                            loadDashboardTaskResult.ModifiedCustomSubstVars = null;
                            return loadDashboardTaskResult;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
		
		public XFSelectionChangedTaskResult ProcessSelectionChangedTask(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
		{
			try
			{
				if ((brGlobals != null) && (workspace != null) && (args?.SelectionChangedTaskInfo != null))
				{
					// Handle selection changes from table editors
					if (args.FunctionName.XFEqualsIgnoreCase("Add_Src_Cell_Row"))
					{
						// Handle Add button for Src Cell Dashboard Data Entry to dynamically generate new row
						return Add_SrcCellRow(si, brGlobals, workspace, args);
					}
					else if (args.SelectionChangedTaskInfo.ComponentName.XFContainsIgnoreCase("ted_FMM"))
					{
						// Handle table editor selection changes
						var taskResult = new XFSelectionChangedTaskResult();
						taskResult.ChangeCustomSubstVarsInDashboard = true;
						taskResult.ModifiedCustomSubstVars = new Dictionary<string, string>();
						
						// Get selected row data and update custom substitution variables
						if (args.SelectionChangedTaskInfo.SelectedDataRows != null && args.SelectionChangedTaskInfo.SelectedDataRows.Count > 0)
						{
							var selectedRow = args.SelectionChangedTaskInfo.SelectedDataRows[0];
							
							// Map selection to custom substitution variables based on component
							if (args.SelectionChangedTaskInfo.ComponentName.XFEqualsIgnoreCase("ted_FMM_Calc_Config"))
							{
								if (selectedRow.Table.Columns.Contains("Calc_ID"))
								{
									taskResult.ModifiedCustomSubstVars.Add("IV_FMM_Calc_ID", selectedRow["Calc_ID"].ToString());
								}
							}
							else if (args.SelectionChangedTaskInfo.ComponentName.XFEqualsIgnoreCase("ted_FMM_Model_Grp_Assign"))
							{
								if (selectedRow.Table.Columns.Contains("Model_ID"))
								{
									taskResult.ModifiedCustomSubstVars.Add("IV_FMM_Model_ID", selectedRow["Model_ID"].ToString());
								}
							}
						}
						
						return taskResult;
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
		
		/// <summary>
		/// Add a new row to the Src Cell table dynamically
		/// </summary>
		private XFSelectionChangedTaskResult Add_SrcCellRow(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardExtenderArgs args)
		{
			try
			{
				var taskResult = new XFSelectionChangedTaskResult();
				taskResult.ChangeCustomSubstVarsInDashboard = true;
				taskResult.ModifiedCustomSubstVars = new Dictionary<string, string>();
				
				// Get current context from custom substitution variables
				string calcId = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Calc_ID");
				string cubeId = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Cube_ID");
				string actId = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Act_ID");
				string modelId = args.SelectionChangedTaskInfo.CustomSubstVars.XFGetValue("IV_FMM_Model_ID");
				
				// Create new row in FMM_Src_Cell table
				using (DbConnInfo dbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si))
				{
					string insertSql = @"
						INSERT INTO FMM_Src_Cell (Calc_ID, Cube_ID, Act_ID, Model_ID, Src_Order, Create_Date, Create_User, Update_Date, Update_User)
						VALUES (@CalcId, @CubeId, @ActId, @ModelId, 
							(SELECT ISNULL(MAX(Src_Order), 0) + 1 FROM FMM_Src_Cell WHERE Calc_ID = @CalcId), 
							GETDATE(), @UserName, GETDATE(), @UserName)";
					
					using (DbCommand cmd = BRApi.Database.CreateCommand(si, dbConnInfo, insertSql))
					{
						BRApi.Database.AddCommandParameter(si, dbConnInfo, cmd, "@CalcId", DbType.String, calcId);
						BRApi.Database.AddCommandParameter(si, dbConnInfo, cmd, "@CubeId", DbType.String, cubeId);
						BRApi.Database.AddCommandParameter(si, dbConnInfo, cmd, "@ActId", DbType.String, actId);
						BRApi.Database.AddCommandParameter(si, dbConnInfo, cmd, "@ModelId", DbType.String, modelId);
						BRApi.Database.AddCommandParameter(si, dbConnInfo, cmd, "@UserName", DbType.String, si.UserName);
						
						BRApi.Database.ExecuteNonQuery(si, dbConnInfo, cmd);
					}
				}
				
				// Refresh the dashboard to show the new row
				taskResult.ModifiedCustomSubstVars.Add("IV_FMM_Refresh_SrcCell", DateTime.Now.Ticks.ToString());
				
				return taskResult;
			}
			catch (Exception ex)
			{
				throw new XFException(si, ex);
			}
		}
	}
}