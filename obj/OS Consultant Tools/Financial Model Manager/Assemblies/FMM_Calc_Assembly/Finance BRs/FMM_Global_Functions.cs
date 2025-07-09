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
    public class Global_Functions : IWsasFinanceCustomCalculateV800
    {
        #region System

        public SessionInfo si;
        public BRGlobals globals;
        public FinanceRulesApi api;
        public object args;
        public Dictionary<string, Member> memberDict;

        private Dictionary<string, DataBuffer> unbal_srcdataBuffers = new Dictionary<string, DataBuffer>();

        #endregion
        public void CustomCalculate(SessionInfo si, BRGlobals brGlobals, FinanceRulesApi api, FinanceRulesArgs args)
        {
            try
            {
                this.si = si;
                this.globals = globals;
                this.api = api;
                this.args = args;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
        #region "Standard Buffer Calcs"
        #region UpdateCellDefinition
        /// <summary>
        /// Updates the definition of a data buffer cell based on various driver database dimensions.
        /// </summary>
        /// <param name="destcell">Reference to the cell to update.</param>
        /// <param name="DriverDB_Acct">Account dimension, optional.</param>
        /// <param name="DriverDB_Flow">Flow dimension, optional.</param>
        /// <param name="DriverDB_Origin">Origin dimension, optional.</param>
        /// <param name="DriverDB_IC">Intercompany dimension, optional.</param>
        /// <param name="DriverDB_UD1">User Defined dimension 1, optional.</param>
        /// <param name="DriverDB_UD2">User Defined dimension 2, optional.</param>
        /// <param name="DriverDB_UD3">User Defined dimension 3, optional.</param>
        /// <param name="DriverDB_UD4">User Defined dimension 4, optional.</param>
        /// <param name="DriverDB_UD5">User Defined dimension 5, optional.</param>
        /// <param name="DriverDB_UD6">User Defined dimension 6, optional.</param>
        /// <param name="DriverDB_UD7">User Defined dimension 7, optional.</param>
        /// <param name="DriverDB_UD8">User Defined dimension 8, optional.</param>
        /// <returns>Updated data buffer cell.</returns>
        public DataBufferCell UpdateCellDefinition(
            DataBufferCell destcell,
            string DriverDB_Acct = "NoPassedValue",
            string DriverDB_Flow = "NoPassedValue",
            string DriverDB_Origin = "NoPassedValue",
            string DriverDB_IC = "NoPassedValue",
            string DriverDB_UD1 = "NoPassedValue",
            string DriverDB_UD2 = "NoPassedValue",
            string DriverDB_UD3 = "NoPassedValue",
            string DriverDB_UD4 = "NoPassedValue",
            string DriverDB_UD5 = "NoPassedValue",
            string DriverDB_UD6 = "NoPassedValue",
            string DriverDB_UD7 = "NoPassedValue",
            string DriverDB_UD8 = "NoPassedValue")
        {
            var DriverDBCell = new DataBufferCell(destcell);

            if (!string.IsNullOrEmpty(DriverDB_Acct) && DriverDB_Acct != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.AccountId = UpdateDimension(DriverDBCell.DataBufferCellPk.AccountId, DriverDB_Acct, api.Pov.AccountDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_Flow) && DriverDB_Flow != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.FlowId = UpdateDimension(DriverDBCell.DataBufferCellPk.FlowId, DriverDB_Flow, api.Pov.FlowDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_Origin) && DriverDB_Origin != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.OriginId = UpdateDimension(DriverDBCell.DataBufferCellPk.OriginId, DriverDB_Origin, api.Pov.OriginDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_IC) && DriverDB_IC != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.ICId = UpdateDimension(DriverDBCell.DataBufferCellPk.ICId, DriverDB_IC, api.Pov.ICDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD1) && DriverDB_UD1 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD1Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD1Id, DriverDB_UD1, api.Pov.UD1Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD2) && DriverDB_UD2 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD2Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD2Id, DriverDB_UD2, api.Pov.UD2Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD3) && DriverDB_UD3 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD3Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD3Id, DriverDB_UD3, api.Pov.UD3Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD4) && DriverDB_UD4 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD4Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD4Id, DriverDB_UD4, api.Pov.UD4Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD5) && DriverDB_UD5 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD5Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD5Id, DriverDB_UD5, api.Pov.UD5Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD6) && DriverDB_UD6 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD6Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD6Id, DriverDB_UD6, api.Pov.UD6Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD7) && DriverDB_UD7 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD7Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD7Id, DriverDB_UD7, api.Pov.UD7Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD8) && DriverDB_UD8 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD8Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD8Id, DriverDB_UD8, api.Pov.UD8Dim, api);
            }

            return DriverDBCell;
        }

        #endregion

        #region GetBCValue
        /// <summary>
        /// Retrieves the buffer cell value based on specified dimensions.
        /// </summary>
        public decimal GetBCValue(
            ref DataBufferCell srccell,
            DataBuffer DriverDB,
            string DriverDB_Acct = "NoPassedValue",
            string DriverDB_Flow = "NoPassedValue",
            string DriverDB_Origin = "NoPassedValue",
            string DriverDB_IC = "NoPassedValue",
            string DriverDB_UD1 = "NoPassedValue",
            string DriverDB_UD2 = "NoPassedValue",
            string DriverDB_UD3 = "NoPassedValue",
            string DriverDB_UD4 = "NoPassedValue",
            string DriverDB_UD5 = "NoPassedValue",
            string DriverDB_UD6 = "NoPassedValue",
            string DriverDB_UD7 = "NoPassedValue",
            string DriverDB_UD8 = "NoPassedValue")
        {
            DataBufferCell DriverDBCell = new DataBufferCell(srccell);
            BRApi.ErrorLog.LogMessage(si, "db cell val: " + DriverDBCell.CellAmount);
            BRApi.ErrorLog.LogMessage(si, "vals: " + DriverDB_Acct + " " + DriverDB_Flow + " " + DriverDB_Origin + " " + DriverDB_UD1 + " " + DriverDB_UD2 + " " + DriverDB_UD3 + " " + DriverDB_UD4 + " " + DriverDB_UD5 + " " + DriverDB_UD6 + " " + DriverDB_UD7 + " " + DriverDB_UD8);

            if (!string.IsNullOrEmpty(DriverDB_Acct) && DriverDB_Acct != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.AccountId = UpdateDimension(DriverDBCell.DataBufferCellPk.AccountId, DriverDB_Acct, api.Pov.AccountDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_Flow) && DriverDB_Flow != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.FlowId = UpdateDimension(DriverDBCell.DataBufferCellPk.FlowId, DriverDB_Flow, api.Pov.FlowDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_Origin) && DriverDB_Origin != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.OriginId = UpdateDimension(DriverDBCell.DataBufferCellPk.OriginId, DriverDB_Origin, api.Pov.OriginDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_IC) && DriverDB_IC != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.ICId = UpdateDimension(DriverDBCell.DataBufferCellPk.ICId, DriverDB_IC, api.Pov.ICDim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD1) && DriverDB_UD1 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD1Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD1Id, DriverDB_UD1, api.Pov.UD1Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD2) && DriverDB_UD2 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD2Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD2Id, DriverDB_UD2, api.Pov.UD2Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD3) && DriverDB_UD3 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD3Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD3Id, DriverDB_UD3, api.Pov.UD3Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD4) && DriverDB_UD4 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD4Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD4Id, DriverDB_UD4, api.Pov.UD4Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD5) && DriverDB_UD5 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD5Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD5Id, DriverDB_UD5, api.Pov.UD5Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD6) && DriverDB_UD6 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD6Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD6Id, DriverDB_UD6, api.Pov.UD6Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD7) && DriverDB_UD7 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD7Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD7Id, DriverDB_UD7, api.Pov.UD7Dim, api);
            }
            if (!string.IsNullOrEmpty(DriverDB_UD8) && DriverDB_UD8 != "NoPassedValue")
            {
                DriverDBCell.DataBufferCellPk.UD8Id = UpdateDimension(DriverDBCell.DataBufferCellPk.UD8Id, DriverDB_UD8, api.Pov.UD8Dim, api);
            }

            var foundCell = DriverDB.GetCell(si, DriverDBCell.DataBufferCellPk);
            DriverDB.LogDataBuffer(api, "driverdb", 500);
            if (foundCell != null)
            {
                return foundCell.CellAmount;
            }
            else
            {
                BRApi.ErrorLog.LogMessage(si, "did not find a cell");
                return 0;
            }
        }

        private int UpdateDimension(int Member_Id, string dimensionValue, Dim dimension, FinanceRulesApi api)
        {
            if (!string.IsNullOrEmpty(dimensionValue) && dimensionValue != "NoPassedValue")
            {
                if (string.Equals(dimensionValue, "Common", StringComparison.OrdinalIgnoreCase))
                {
                    Member_Id = GetMember(ref dimension, "XFCommon", ref api).MemberId;
                }
                else
                {
                    Member_Id = GetMember(ref dimension, dimensionValue, ref api).MemberId;
                }
            }
            return Member_Id;
        }
        #endregion

        #region "Update Value"
        public void UpdateValue(ref DataBufferCell srcCell, ref DataBuffer currCellDB, ref DataBuffer destDB, decimal value)
        {
            if (value == 0)
            {
                if (currCellDB.GetCell(si, srcCell.DataBufferCellPk) == null)
                {
                    // srcCell.CellAmount = null;
                    // if (srcCell.CellStatus.IsDerivedData)
                    // {
                    //     srcCell.CellAmount = srcCell.CellAmount;
                    //     var status = new DataCellStatus(true);
                    //     var clearCell = new DataBufferCell(srcCell.DataBufferCellPk, 0, status);
                    //     destDB.SetCell(si, clearCell);
                    // }
                    // else if (srcCell.CellStatus.IsRealData)
                    // {
                    //     var status = new DataCellStatus(true);
                    //     var clearCell = new DataBufferCell(srcCell.DataBufferCellPk, 0, status);
                    //     srcCell.CellAmount = srcCell.CellAmount;
                    //     destDB.SetCell(si, clearCell);
                    // }
                    // else
                    {
                        var status = new DataCellStatus(false);
                        srcCell.CellAmount = 0.0M;
                        var clearCell = new DataBufferCell(srcCell.DataBufferCellPk, 0, status);
                        destDB.SetCell(si, clearCell);
                    }
                }
                else
                {
                    // if (!currCellDB.GetCell(si, srcCell.DataBufferCellPk).CellStatus.StorageType == DataCellStorageType.Input)
                    // {
                    //     if (srcCell.CellStatus.IsRealOrDerivedData)
                    //     {
                    //         var status = new DataCellStatus(true);
                    //         var clearCell = new DataBufferCell(srcCell.DataBufferCellPk, 0, status);
                    //         srcCell.CellAmount = srcCell.CellAmount;
                    //         destDB.SetCell(si, clearCell);
                    //     }
                    //     else
                    {
                        srcCell.CellAmount = 0.0M;
                        var status = new DataCellStatus(false);
                        var clearCell = new DataBufferCell(srcCell.DataBufferCellPk, 0, status);
                        destDB.SetCell(si, clearCell);
                    }
                    // }
                }
            }
            else
            {
                srcCell.CellAmount = value;

                if (currCellDB.GetCell(si, srcCell.DataBufferCellPk) == null)
                {
                    if (!srcCell.CellStatus.IsNoData)
                    {
                        destDB.SetCell(si, srcCell, true);
                    }
                }
                else
                {
                    var currCell = currCellDB.GetCell(si, srcCell.DataBufferCellPk);
                    if (!currCell.CellStatus.IsRealData || currCell.CellStatus.IsNoData || currCell.CellStatus.StorageType != DataCellStorageType.Input)
                    {
                        if (currCell.CellAmount != srcCell.CellAmount)
                        {
                            destDB.SetCell(si, srcCell, true);
                        }
                    }
                    else
                    {
                        if (!currCell.CellStatus.WriteAccess)
                        {
                            if (currCell.CellAmount != srcCell.CellAmount)
                            {
                                destDB.SetCell(si, srcCell, true);
                            }
                        }
                        else
                        {
                            if (currCell.CellAmount != srcCell.CellAmount)
                            {
                                destDB.SetCell(si, srcCell, true);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #endregion

        #region "MCM Buffer Calcs"
        #region "Balanced Buffer"
        public void Calc_Balanced_Buffer(
            int os_Calc_ID,
            string balanced_buffer_calc
            )
        {
            string bal_buffer_dest_cell_SQL = @"
												Select OS_Target_Acct, OS_Target_View,OS_Target_Origin,OS_Target_IC,OS_Target_Flow,OS_Target_UD1,
													OS_Target_UD2,OS_Target_UD3,OS_Target_UD4,OS_Target_UD5,OS_Target_UD6,OS_Target_UD7,OS_Target_U8,
													OS_Curr_Cube_Buffer_Filter,OS_Src_Buffer_Filter
													FROM MCM_Dest_Cell
													WHERE OS_Calc_ID = @OS_Calc_ID";

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();
                var command = new SqlCommand(bal_buffer_dest_cell_SQL, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dest_acct = reader["OS_Target_Acct"] != DBNull.Value ? reader["OS_Target_Acct"].ToString() : null;
                        var dest_view = reader["OS_Target_View"] != DBNull.Value ? reader["OS_Target_View"].ToString() : null;
                        var dest_origin = reader["OS_Target_Origin"] != DBNull.Value ? reader["OS_Target_Origin"].ToString() : null;
                        var dest_ic = reader["OS_Target_IC"] != DBNull.Value ? reader["OS_Target_IC"].ToString() : null;
                        var dest_flow = reader["OS_Target_Flow"] != DBNull.Value ? reader["OS_Target_Flow"].ToString() : null;
                        var dest_ud1 = reader["OS_Target_UD1"] != DBNull.Value ? reader["OS_Target_UD1"].ToString() : null;
                        var dest_ud2 = reader["OS_Target_UD2"] != DBNull.Value ? reader["OS_Target_UD2"].ToString() : null;
                        var dest_ud3 = reader["OS_Target_UD3"] != DBNull.Value ? reader["OS_Target_UD3"].ToString() : null;
                        var dest_ud4 = reader["OS_Target_UD4"] != DBNull.Value ? reader["OS_Target_UD4"].ToString() : null;
                        var dest_ud5 = reader["OS_Target_UD5"] != DBNull.Value ? reader["OS_Target_UD5"].ToString() : null;
                        var dest_ud6 = reader["OS_Target_UD6"] != DBNull.Value ? reader["OS_Target_UD6"].ToString() : null;
                        var dest_ud7 = reader["OS_Target_UD7"] != DBNull.Value ? reader["OS_Target_UD7"].ToString() : null;
                        var dest_ud8 = reader["OS_Target_UD8"] != DBNull.Value ? reader["OS_Target_UD8"].ToString() : null;

                        var os_Curr_Cube_Buffer = reader["OS_Curr_Cube_Buffer_Filter"].ToString();
                        var os_Src_Buffer = reader["OS_Src_Buffer_Filter"].ToString();
                        //Test RemoveNoData and RemoveZeroes
                        var currCubeBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({dest_view},{os_Curr_Cube_Buffer})");
                        var destBuffer = new DataBuffer(currCubeBuffer.CommonDataBufferCellPk);
                        var destInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");

                        var clearCubeData = new DataBuffer();

                        var balancedBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({balanced_buffer_calc},{os_Src_Buffer}])");

                        foreach (var balancedBuffer_Cell in balancedBuffer.DataBufferCells.Values)
                        {
                            //Need to set the Dest Cell correctly so that the clear routine works
                            var destCell = new DataBufferCell(UpdateCellDefinition(balancedBuffer_Cell, dest_acct, dest_flow, dest_origin,
                                dest_ic, dest_ud1, dest_ud2, dest_ud3, dest_ud4, dest_ud5, dest_ud6, dest_ud7, dest_ud8));

                            UpdateValue(ref destCell, ref currCubeBuffer, ref destBuffer, destCell.CellAmount);
                            currCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk);
                        }

                        // Write calculated cells from above back to final output data buffer
                        api.Data.SetDataBuffer(destBuffer, destInfo);
                        destBuffer.DataBufferCells.Clear();

                        foreach (var ClearCubeCell in currCubeBuffer.DataBufferCells.Values)
                        {
                            var status = new DataCellStatus(false);
                            var clearCell = new DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, status);
                            clearCubeData.SetCell(si, clearCell);
                        }

                        var clearInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");
                        api.Data.SetDataBuffer(clearCubeData, clearInfo);
                    }
                }
            }

        }

        #endregion

        #region "Unbalanced Buffer"
        public void Calc_Unbalanced_Buffer(
            int os_Calc_ID,
            string balanced_buffer_calc,
            string Unbalanced_Calc
            )
        {

            string unbal_buffer_dest_cell_SQL = @"
												Select OS_Target_Acct, OS_Target_View,OS_Target_Origin,OS_Target_IC,OS_Target_Flow,OS_Target_UD1,
													OS_Target_UD2,OS_Target_UD3,OS_Target_UD4,OS_Target_UD5,OS_Target_UD6,OS_Target_UD7,OS_Target_U8,
													OS_Curr_Cube_Buffer_Filter,OS_Src_Buffer_Filter
													FROM MCM_Dest_Cell
													WHERE OS_Calc_ID = @OS_Calc_ID";
            string src_cell_SQL = @"
						        SELECT OS_Calc_Src_ID_Order, OS_Calc_Open_Parens, OS_Calc_Math_Operator, OS_Src_Entity, OS_Src_Cons, OS_Src_Scenario,
						            OS_Src_Time, OS_Src_Origin, OS_Src_IC, OS_Src_View, OS_Src_Acct, OS_Src_Flow, OS_Src_UD1, OS_Src_UD2,
						            OS_Src_UD3, OS_Src_UD4, OS_Src_UD5, OS_Src_UD6, OS_Src_UD7, OS_Src_UD8, OS_Calc_Close_Parens,Unbalanced_Src_Cell_Buffer,
						            Unbalanced_Origin_Override,Unbalanced_IC_Override,Unbalanced_Acct_Override,Unbalanced_Flow_Override,Unbalanced_UD1_Override,
									Unbalanced_UD2_Override,Unbalanced_UD3_Override,Unbalanced_UD4_Override,Unbalanced_UD5_Override,Unbalanced_UD6_Override,
									Unbalanced_UD7_Override,Unbalanced_UD8_Override,Unbalanced_Src_Cell_Buffer_Filter,OS_Dynamic_Calc_Script,Override_Dest_Value,
									OS_Calc_Src_Type
						        FROM MCM_Src_Cell
						        WHERE OS_Calc_ID = @OS_Calc_ID
								AND OS_Calc_Src_ID_Order > 1
						        ORDER BY OS_Calc_Src_ID_Order";

            var srcCellTable = new DataTable();

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();
                var unbal_srcCommand = new SqlCommand(src_cell_SQL, connection);
                var src_buffers = 0;
                unbal_srcCommand.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });

                using (var unbal_srcreader = unbal_srcCommand.ExecuteReader())
                {
                    while (unbal_srcreader.Read())
                    {
                        string calc_srcType = unbal_srcreader["OS_Calc_Src_Type"].ToString();
                        var row = srcCellTable.NewRow();
                        row["OS_Calc_Src_ID_Order"] = unbal_srcreader["OS_Calc_Src_ID_Order"];
                        row["OS_Calc_Open_Parens"] = unbal_srcreader["OS_Calc_Open_Parens"];
                        row["OS_Calc_Math_Operator"] = unbal_srcreader["OS_Calc_Math_Operator"];
                        row["OS_Src_Entity"] = unbal_srcreader["OS_Src_Entity"];
                        row["OS_Src_Cons"] = unbal_srcreader["OS_Src_Cons"];
                        row["OS_Src_Scenario"] = unbal_srcreader["OS_Src_Scenario"];
                        row["OS_Src_Time"] = unbal_srcreader["OS_Src_Time"];
                        row["OS_Src_Origin"] = unbal_srcreader["OS_Src_Origin"];
                        row["OS_Src_IC"] = unbal_srcreader["OS_Src_IC"];
                        row["OS_Src_View"] = unbal_srcreader["OS_Src_View"];
                        row["OS_Src_Acct"] = unbal_srcreader["OS_Src_Acct"];
                        row["OS_Src_Flow"] = unbal_srcreader["OS_Src_Flow"];
                        row["OS_Src_UD1"] = unbal_srcreader["OS_Src_UD1"];
                        row["OS_Src_UD2"] = unbal_srcreader["OS_Src_UD2"];
                        row["OS_Src_UD3"] = unbal_srcreader["OS_Src_UD3"];
                        row["OS_Src_UD4"] = unbal_srcreader["OS_Src_UD4"];
                        row["OS_Src_UD5"] = unbal_srcreader["OS_Src_UD5"];
                        row["OS_Src_UD6"] = unbal_srcreader["OS_Src_UD6"];
                        row["OS_Src_UD7"] = unbal_srcreader["OS_Src_UD7"];
                        row["OS_Src_UD8"] = unbal_srcreader["OS_Src_UD8"];
                        row["OS_Calc_Close_Parens"] = unbal_srcreader["OS_Calc_Close_Parens"];
                        row["Unbalanced_Src_Cell_Buffer"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer"];
                        row["Unbalanced_Origin_Override"] = unbal_srcreader["Unbalanced_Origin_Override"];
                        row["Unbalanced_IC_Override"] = unbal_srcreader["Unbalanced_IC_Override"];
                        row["Unbalanced_Acct_Override"] = unbal_srcreader["Unbalanced_Acct_Override"];
                        row["Unbalanced_Flow_Override"] = unbal_srcreader["Unbalanced_Flow_Override"];
                        row["Unbalanced_UD1_Override"] = unbal_srcreader["Unbalanced_UD1_Override"];
                        row["Unbalanced_UD2_Override"] = unbal_srcreader["Unbalanced_UD2_Override"];
                        row["Unbalanced_UD3_Override"] = unbal_srcreader["Unbalanced_UD3_Override"];
                        row["Unbalanced_UD4_Override"] = unbal_srcreader["Unbalanced_UD4_Override"];
                        row["Unbalanced_UD5_Override"] = unbal_srcreader["Unbalanced_UD5_Override"];
                        row["Unbalanced_UD6_Override"] = unbal_srcreader["Unbalanced_UD6_Override"];
                        row["Unbalanced_UD7_Override"] = unbal_srcreader["Unbalanced_UD7_Override"];
                        row["Unbalanced_UD8_Override"] = unbal_srcreader["Unbalanced_UD8_Override"];
                        row["Unbalanced_Src_Cell_Buffer_Filter"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer_Filter"];
                        row["OS_Dynamic_Calc_Script"] = unbal_srcreader["OS_Dynamic_Calc_Script"];
                        row["Override_Dest_Value"] = unbal_srcreader["Override_Dest_Value"];
                        row["OS_Calc_Src_Type"] = unbal_srcreader["OS_Calc_Src_Type"];
                        srcCellTable.Rows.Add(row);

                        int index = (int)unbal_srcreader["OS_Calc_Src_ID_Order"];
                        src_buffers = index;

                        if (calc_srcType != "Dynamic Calc")
                        {

                            string unbal_srcBuffer = unbal_srcreader["Unbalanced_Src_Cell_Buffer"].ToString();
                            string unbal_srcBufferFilter = unbal_srcreader["OS_Src_Buffer_Filter"].ToString();
                            string bufferName = $"SrcBuffer{index}";

                            // Dynamically create and store the data buffer
                            var dataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({unbal_srcBuffer}, {unbal_srcBufferFilter})");
                            unbal_srcdataBuffers.Add(bufferName, dataBuffer);
                        }
                    }
                }
                var command = new SqlCommand(unbal_buffer_dest_cell_SQL, connection);
                command.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dest_acct = reader["OS_Target_Acct"] != DBNull.Value ? reader["OS_Target_Acct"].ToString() : null;
                        var dest_view = reader["OS_Target_View"] != DBNull.Value ? reader["OS_Target_View"].ToString() : null;
                        var dest_origin = reader["OS_Target_Origin"] != DBNull.Value ? reader["OS_Target_Origin"].ToString() : null;
                        var dest_ic = reader["OS_Target_IC"] != DBNull.Value ? reader["OS_Target_IC"].ToString() : null;
                        var dest_flow = reader["OS_Target_Flow"] != DBNull.Value ? reader["OS_Target_Flow"].ToString() : null;
                        var dest_ud1 = reader["OS_Target_UD1"] != DBNull.Value ? reader["OS_Target_UD1"].ToString() : null;
                        var dest_ud2 = reader["OS_Target_UD2"] != DBNull.Value ? reader["OS_Target_UD2"].ToString() : null;
                        var dest_ud3 = reader["OS_Target_UD3"] != DBNull.Value ? reader["OS_Target_UD3"].ToString() : null;
                        var dest_ud4 = reader["OS_Target_UD4"] != DBNull.Value ? reader["OS_Target_UD4"].ToString() : null;
                        var dest_ud5 = reader["OS_Target_UD5"] != DBNull.Value ? reader["OS_Target_UD5"].ToString() : null;
                        var dest_ud6 = reader["OS_Target_UD6"] != DBNull.Value ? reader["OS_Target_UD6"].ToString() : null;
                        var dest_ud7 = reader["OS_Target_UD7"] != DBNull.Value ? reader["OS_Target_UD7"].ToString() : null;
                        var dest_ud8 = reader["OS_Target_UD8"] != DBNull.Value ? reader["OS_Target_UD8"].ToString() : null;

                        var os_Curr_Cube_Buffer = reader["OS_Curr_Cube_Buffer_Filter"].ToString();
                        var os_Src_Buffer = reader["OS_Src_Buffer_Filter"].ToString();
                        //Test RemoveNoData and RemoveZeroes
                        var currCubeBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({dest_view},{os_Curr_Cube_Buffer})");
                        var destBuffer = new DataBuffer(currCubeBuffer.CommonDataBufferCellPk);
                        var destInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");

                        var clearCubeData = new DataBuffer();

                        var balancedBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({balanced_buffer_calc},{os_Src_Buffer}])");

                        foreach (var balancedBuffer_Cell in balancedBuffer.DataBufferCells.Values)
                        {
                            //Need to set the Dest Cell correctly so that the clear routine works
                            var destCell = new DataBufferCell(UpdateCellDefinition(balancedBuffer_Cell, dest_acct, dest_flow, dest_origin,
                                dest_ic, dest_ud1, dest_ud2, dest_ud3, dest_ud4, dest_ud5, dest_ud6, dest_ud7, dest_ud8));

                            //Call new function, pass in balancedbuffer_cell and first unbal src row...  Build up calc string.  Run math  
                            var table = new DataTable();
                            var expression = Unbalanced_Calc.Replace("BalancedBuffer", balancedBuffer_Cell.CellAmount.XFToString());
                            //If this is Dynamic Calc, then call Dyn Calc 
                            for (int i = 2; i <= src_buffers; i++)
                            {
                                var srcRow = srcCellTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["OS_Calc_Src_ID_Order"]) == i);
                                if (srcRow != null)
                                {
                                    if (srcRow["OS_Calc_Src_Type"].ToString() == "Dynamic Calc")
                                    {
                                        var dyn_Calc = getdynamic_calc_value(balancedBuffer_Cell, srcRow);
                                        expression = Unbalanced_Calc.Replace($"DynamicCalc{i}", dyn_Calc.XFToString());

                                    }
                                    else if (srcRow["OS_Calc_Src_Type"].ToString() == "Stored Cell")
                                    {
                                        string searchName = $"SrcBuffer{i}";
                                        var resultBuffer = new DataBuffer();

                                        unbal_srcdataBuffers.XFGetValue(searchName, resultBuffer);
                                        {
                                            // The key was found, and resultBuffer now holds the corresponding DataBuffer
                                            Console.WriteLine("Buffer found for the name: " + searchName);
                                        }
                                        var src_buffer_Value = get_src_unbal_buffer_value(balancedBuffer_Cell, resultBuffer, srcRow);
                                        expression = Unbalanced_Calc.Replace($"SrcBuffer{i}", src_buffer_Value.XFToString());

                                    }
                                }
                            }
                            // Use the Compute method to evaluate the expression
                            var result = table.Compute(expression, string.Empty);
                            UpdateValue(ref destCell, ref currCubeBuffer, ref destBuffer, Convert.ToDecimal(result));
                            currCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk);
                        }

                        // Write calculated cells from above back to final output data buffer
                        api.Data.SetDataBuffer(destBuffer, destInfo);
                        destBuffer.DataBufferCells.Clear();

                        foreach (var ClearCubeCell in currCubeBuffer.DataBufferCells.Values)
                        {
                            var status = new DataCellStatus(false);
                            var clearCell = new DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, status);
                            clearCubeData.SetCell(si, clearCell);
                        }

                        var clearInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");
                        api.Data.SetDataBuffer(clearCubeData, clearInfo);
                    }
                }
            }

        }

        #endregion

        #region "UnbalAlloc"
        public void Calc_Unbal_Alloc_Buffer(
            int os_Calc_ID,
            string balanced_buffer_calc,
            string Unbalanced_Calc
            )
        {
            var cell_Expressions = new Dictionary<DataBufferCell, string>();
            var src_buffers = 0;
            var unbal_srcdataBuffers = new Dictionary<string, DataBuffer>();
            string unbal_buffer_dest_cell_SQL = @"
												Select OS_Target_Acct, OS_Target_View,OS_Target_Origin,OS_Target_IC,OS_Target_Flow,OS_Target_UD1,
													OS_Target_UD2,OS_Target_UD3,OS_Target_UD4,OS_Target_UD5,OS_Target_UD6,OS_Target_UD7,OS_Target_U8,
													OS_Curr_Cube_Buffer_Filter,OS_Src_Buffer_Filter
													FROM MCM_Dest_Cell
													WHERE OS_Calc_ID = @OS_Calc_ID";
            string src_cell_SQL = @"
						        SELECT OS_Calc_Src_ID_Order, OS_Calc_Open_Parens, OS_Calc_Math_Operator, OS_Src_Entity, OS_Src_Cons, OS_Src_Scenario,
						            OS_Src_Time, OS_Src_Origin, OS_Src_IC, OS_Src_View, OS_Src_Acct, OS_Src_Flow, OS_Src_UD1, OS_Src_UD2,
						            OS_Src_UD3, OS_Src_UD4, OS_Src_UD5, OS_Src_UD6, OS_Src_UD7, OS_Src_UD8, OS_Calc_Close_Parens,Unbalanced_Src_Cell_Buffer,
						            Unbalanced_Origin_Override,Unbalanced_IC_Override,Unbalanced_Acct_Override,Unbalanced_Flow_Override,Unbalanced_UD1_Override,
									Unbalanced_UD2_Override,Unbalanced_UD3_Override,Unbalanced_UD4_Override,Unbalanced_UD5_Override,Unbalanced_UD6_Override,
									Unbalanced_UD7_Override,Unbalanced_UD8_Override,Unbalanced_Src_Cell_Buffer_Filter,OS_Dynamic_Calc_Script,Override_Dest_Value,
									OS_Calc_Src_Type
						        FROM MCM_Src_Cell
						        WHERE OS_Calc_ID = @OS_Calc_ID
								AND OS_Calc_Src_ID_Order > 1
						        ORDER BY OS_Calc_Src_ID_Order";

            var srcCellTable = new DataTable();

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();
                var unbal_srcCommand = new SqlCommand(src_cell_SQL, connection);
                unbal_srcCommand.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });

                using (var unbal_srcreader = unbal_srcCommand.ExecuteReader())
                {
                    while (unbal_srcreader.Read())
                    {
                        string calc_srcType = unbal_srcreader["OS_Calc_Src_Type"].ToString();
                        var row = srcCellTable.NewRow();
                        row["OS_Calc_Src_ID_Order"] = unbal_srcreader["OS_Calc_Src_ID_Order"];
                        row["OS_Calc_Open_Parens"] = unbal_srcreader["OS_Calc_Open_Parens"];
                        row["OS_Calc_Math_Operator"] = unbal_srcreader["OS_Calc_Math_Operator"];
                        row["OS_Src_Entity"] = unbal_srcreader["OS_Src_Entity"];
                        row["OS_Src_Cons"] = unbal_srcreader["OS_Src_Cons"];
                        row["OS_Src_Scenario"] = unbal_srcreader["OS_Src_Scenario"];
                        row["OS_Src_Time"] = unbal_srcreader["OS_Src_Time"];
                        row["OS_Src_Origin"] = unbal_srcreader["OS_Src_Origin"];
                        row["OS_Src_IC"] = unbal_srcreader["OS_Src_IC"];
                        row["OS_Src_View"] = unbal_srcreader["OS_Src_View"];
                        row["OS_Src_Acct"] = unbal_srcreader["OS_Src_Acct"];
                        row["OS_Src_Flow"] = unbal_srcreader["OS_Src_Flow"];
                        row["OS_Src_UD1"] = unbal_srcreader["OS_Src_UD1"];
                        row["OS_Src_UD2"] = unbal_srcreader["OS_Src_UD2"];
                        row["OS_Src_UD3"] = unbal_srcreader["OS_Src_UD3"];
                        row["OS_Src_UD4"] = unbal_srcreader["OS_Src_UD4"];
                        row["OS_Src_UD5"] = unbal_srcreader["OS_Src_UD5"];
                        row["OS_Src_UD6"] = unbal_srcreader["OS_Src_UD6"];
                        row["OS_Src_UD7"] = unbal_srcreader["OS_Src_UD7"];
                        row["OS_Src_UD8"] = unbal_srcreader["OS_Src_UD8"];
                        row["OS_Calc_Close_Parens"] = unbal_srcreader["OS_Calc_Close_Parens"];
                        row["Unbalanced_Src_Cell_Buffer"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer"];
                        row["Unbalanced_Origin_Override"] = unbal_srcreader["Unbalanced_Origin_Override"];
                        row["Unbalanced_IC_Override"] = unbal_srcreader["Unbalanced_IC_Override"];
                        row["Unbalanced_Acct_Override"] = unbal_srcreader["Unbalanced_Acct_Override"];
                        row["Unbalanced_Flow_Override"] = unbal_srcreader["Unbalanced_Flow_Override"];
                        row["Unbalanced_UD1_Override"] = unbal_srcreader["Unbalanced_UD1_Override"];
                        row["Unbalanced_UD2_Override"] = unbal_srcreader["Unbalanced_UD2_Override"];
                        row["Unbalanced_UD3_Override"] = unbal_srcreader["Unbalanced_UD3_Override"];
                        row["Unbalanced_UD4_Override"] = unbal_srcreader["Unbalanced_UD4_Override"];
                        row["Unbalanced_UD5_Override"] = unbal_srcreader["Unbalanced_UD5_Override"];
                        row["Unbalanced_UD6_Override"] = unbal_srcreader["Unbalanced_UD6_Override"];
                        row["Unbalanced_UD7_Override"] = unbal_srcreader["Unbalanced_UD7_Override"];
                        row["Unbalanced_UD8_Override"] = unbal_srcreader["Unbalanced_UD8_Override"];
                        row["Unbalanced_Src_Cell_Buffer_Filter"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer_Filter"];
                        row["OS_Dynamic_Calc_Script"] = unbal_srcreader["OS_Dynamic_Calc_Script"];
                        row["Override_Dest_Value"] = unbal_srcreader["Override_Dest_Value"];
                        row["OS_Calc_Src_Type"] = unbal_srcreader["OS_Calc_Src_Type"];
                        srcCellTable.Rows.Add(row);

                        int index = (int)unbal_srcreader["OS_Calc_Src_ID_Order"];
                        src_buffers = index;

                        if (calc_srcType != "Dynamic Calc")
                        {
                            string unbal_srcBuffer = unbal_srcreader["Unbalanced_Src_Cell_Buffer"].ToString();
                            string unbal_srcBufferFilter = unbal_srcreader["OS_Src_Buffer_Filter"].ToString();
                            string bufferName = $"SrcBuffer{index}";

                            // Dynamically create and store the data buffer
                            var dataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({unbal_srcBuffer}, {unbal_srcBufferFilter})");
                            unbal_srcdataBuffers.Add(bufferName, dataBuffer);
                        }
                    }
                }
                var command = new SqlCommand(unbal_buffer_dest_cell_SQL, connection);
                command.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var dest_acct = reader["OS_Target_Acct"] != DBNull.Value ? reader["OS_Target_Acct"].ToString() : null;
                        var dest_view = reader["OS_Target_View"] != DBNull.Value ? reader["OS_Target_View"].ToString() : null;
                        var dest_origin = reader["OS_Target_Origin"] != DBNull.Value ? reader["OS_Target_Origin"].ToString() : null;
                        var dest_ic = reader["OS_Target_IC"] != DBNull.Value ? reader["OS_Target_IC"].ToString() : null;
                        var dest_flow = reader["OS_Target_Flow"] != DBNull.Value ? reader["OS_Target_Flow"].ToString() : null;
                        var dest_ud1 = reader["OS_Target_UD1"] != DBNull.Value ? reader["OS_Target_UD1"].ToString() : null;
                        var dest_ud2 = reader["OS_Target_UD2"] != DBNull.Value ? reader["OS_Target_UD2"].ToString() : null;
                        var dest_ud3 = reader["OS_Target_UD3"] != DBNull.Value ? reader["OS_Target_UD3"].ToString() : null;
                        var dest_ud4 = reader["OS_Target_UD4"] != DBNull.Value ? reader["OS_Target_UD4"].ToString() : null;
                        var dest_ud5 = reader["OS_Target_UD5"] != DBNull.Value ? reader["OS_Target_UD5"].ToString() : null;
                        var dest_ud6 = reader["OS_Target_UD6"] != DBNull.Value ? reader["OS_Target_UD6"].ToString() : null;
                        var dest_ud7 = reader["OS_Target_UD7"] != DBNull.Value ? reader["OS_Target_UD7"].ToString() : null;
                        var dest_ud8 = reader["OS_Target_UD8"] != DBNull.Value ? reader["OS_Target_UD8"].ToString() : null;

                        var os_Curr_Cube_Buffer = reader["OS_Curr_Cube_Buffer_Filter"].ToString();
                        var os_Src_Buffer = reader["OS_Src_Buffer_Filter"].ToString();
                        //Test RemoveNoData and RemoveZeroes
                        var currCubeBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({dest_view},{os_Curr_Cube_Buffer})");
                        var destBuffer = new DataBuffer(currCubeBuffer.CommonDataBufferCellPk);
                        var destInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");

                        var clearCubeData = new DataBuffer();

                        var balancedBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({balanced_buffer_calc},{os_Src_Buffer}])");

                        foreach (var balancedBuffer_Cell in balancedBuffer.DataBufferCells.Values)
                        {
                            // Set up the destination cell
                            var destCell = new DataBufferCell(UpdateCellDefinition(balancedBuffer_Cell, dest_acct, dest_flow, dest_origin,
                                dest_ic, dest_ud1, dest_ud2, dest_ud3, dest_ud4, dest_ud5, dest_ud6, dest_ud7, dest_ud8));

                            // Initialize DataTable and expression
                            var table = new DataTable();
                            var expression = Unbalanced_Calc.Replace("BalancedBuffer", balancedBuffer_Cell.CellAmount.XFToString());
                            cell_Expressions.Add(destCell, expression);

                            // First Loop: Handle Dynamic Calcs and Stored Cells
                            for (int i = 2; i <= src_buffers; i++)
                            {
                                var srcRow = srcCellTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["OS_Calc_Src_ID_Order"]) == i);
                                if (srcRow != null)
                                {
                                    UpdateExpressionForRow(balancedBuffer_Cell, srcRow, unbal_srcdataBuffers, ref cell_Expressions, destCell, i);
                                }
                            }

                            foreach (var kvp in cell_Expressions)
                            {
                                var cell = kvp.Key;     // The DataBufferCell (key)
                                expression = kvp.Value;     // The associated expression (value)
                                                            // Use the Compute method to evaluate the expression after both loops
                                var result = table.Compute(expression, string.Empty);
                                UpdateValue(ref cell, ref currCubeBuffer, ref destBuffer, Convert.ToDecimal(result));
                                currCubeBuffer.DataBufferCells.Remove(cell.DataBufferCellPk);
                                cell_Expressions.Remove(cell);
                            }
                        }


                        // Write calculated cells from above back to final output data buffer
                        api.Data.SetDataBuffer(destBuffer, destInfo);
                        destBuffer.DataBufferCells.Clear();

                        foreach (var ClearCubeCell in currCubeBuffer.DataBufferCells.Values)
                        {
                            var status = new DataCellStatus(false);
                            var clearCell = new DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, status);
                            clearCubeData.SetCell(si, clearCell);
                        }

                        var clearInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");
                        api.Data.SetDataBuffer(clearCubeData, clearInfo);
                    }
                }
            }

        }

        #endregion

        #region "Ext_UnbalAlloc"
        public void Calc_Ext_Unbal_Alloc_Buffer(
            int os_Calc_ID,
            string balanced_buffer_calc,
            string Unbalanced_Calc,
            string memberList1Dim,
            string memberList1Filter,
            string memberList1DimType,
            string memberList1DestFilter,
            string memberList2Dim,
            string memberList2Filter,
            string memberList2DimType,
            string memberList2DestFilter,
            string memberList3Dim,
            string memberList3Filter,
            string memberList3DimType,
            string memberList3DestFilter,
            string memberList4Dim,
            string memberList4Filter,
            string memberList4DimType,
            string memberList4DestFilter
        )
        {
            // Process the parameters as needed in your method

            // Example usage of the parameters
            BRApi.ErrorLog.LogMessage(si, $"Processing OS_Calc_ID: {os_Calc_ID}");
            BRApi.ErrorLog.LogMessage(si, $"Balanced Buffer Calc: {balanced_buffer_calc}, Unbalanced Calc: {Unbalanced_Calc}");

            // Log MemberList 1 details
            BRApi.ErrorLog.LogMessage(si, $"MemberList 1: Dim: {memberList1Dim}, Filter: {memberList1Filter}, DimType: {memberList1DimType}, DestFilter: {memberList1DestFilter}");

            // Log MemberList 2 details
            if (memberList2Dim != null)
                BRApi.ErrorLog.LogMessage(si, $"MemberList 2: Dim: {memberList2Dim}, Filter: {memberList2Filter}, DimType: {memberList2DimType}, DestFilter: {memberList2DestFilter}");

            // Log MemberList 3 details
            if (memberList3Dim != null)
                BRApi.ErrorLog.LogMessage(si, $"MemberList 3: Dim: {memberList3Dim}, Filter: {memberList3Filter}, DimType: {memberList3DimType}, DestFilter: {memberList3DestFilter}");

            // Log MemberList 4 details
            if (memberList4Dim != null)
                BRApi.ErrorLog.LogMessage(si, $"MemberList 4: Dim: {memberList4Dim}, Filter: {memberList4Filter}, DimType: {memberList4DimType}, DestFilter: {memberList4DestFilter}");

            // Additional processing logic based on the new parameters can be added here
        }

        #endregion

        #region "Ext_Unbalanced"
        public void Calc_Ext_Unbalanced_Buffer(
            int os_Calc_ID,
            string balanced_buffer_calc,
            string Unbalanced_Calc,
            string memberList1Dim,
            string memberList1Filter,
            string memberList1DimType,
            string memberList1DestFilter,
            string memberList2Dim,
            string memberList2Filter,
            string memberList2DimType,
            string memberList2DestFilter,
            string memberList3Dim,
            string memberList3Filter,
            string memberList3DimType,
            string memberList3DestFilter,
            string memberList4Dim,
            string memberList4Filter,
            string memberList4DimType,
            string memberList4DestFilter
        )
        {
            //BRApi.ErrorLog.LogMessage(si, "Executing Calc_Ext_Unbalanced_Buffer");
            //BRApi.ErrorLog.LogMessage(si, "calc id: "  + os_Calc_ID.ToString());

            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();



                //Get SRC Balanced Buffer,
                //Get all SRC Unbal Buffers

                var unbal_srcdataBuffers = new Dictionary<string, DataBuffer>();
                string unbal_buffer_dest_cell_SQL = @"
													Select OS_Target_Acct, OS_Target_View,OS_Target_Origin,OS_Target_IC,OS_Target_Flow,OS_Target_UD1,
														OS_Target_UD2,OS_Target_UD3,OS_Target_UD4,OS_Target_UD5,OS_Target_UD6,OS_Target_UD7,OS_Target_UD8,
														OS_Curr_Cube_Buffer_Filter,OS_Src_Buffer_Filter
														FROM MCM_Dest_Cell
														WHERE OS_Calc_ID = @OS_Calc_ID";

                var command = new SqlCommand(unbal_buffer_dest_cell_SQL, connection);
                command.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });

                using (var reader = command.ExecuteReader())
                {


                    DataBuffer clearCubeData = null;

                    while (reader.Read())
                    {

                        var dest_view = reader["OS_Target_View"].ToString();
                        var os_Curr_Cube_Buffer = reader["OS_Curr_Cube_Buffer_Filter"].ToString();

                        //Test RemoveNoData and RemoveZeroes
                        var currCubeBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({dest_view},{os_Curr_Cube_Buffer})");
                        var destBuffer = new DataBuffer(currCubeBuffer.CommonDataBufferCellPk);
                        var destInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");

                        Dim mbrList1_Dim = null;
                        List<MemberInfo> mem1List = new List<MemberInfo>();

                        Dim mbrList2_Dim = null;
                        List<MemberInfo> mem2List = new List<MemberInfo>();

                        Dim mbrList3_Dim = null;
                        List<MemberInfo> mem3List = new List<MemberInfo>();

                        Dim mbrList4_Dim = null;
                        List<MemberInfo> mem4List = new List<MemberInfo>();



                        //build out memberlists
                        if (!string.IsNullOrEmpty(memberList1Dim) && !string.IsNullOrEmpty(memberList1Filter))
                        {
                            mbrList1_Dim = api.Dimensions.GetDim(memberList1Dim);
                            mem1List = api.Members.GetMembersUsingFilter(mbrList1_Dim.DimPk, memberList1Filter);
                        }
                        if (!string.IsNullOrEmpty(memberList2Dim) && !string.IsNullOrEmpty(memberList2Filter))
                        {
                            mbrList2_Dim = api.Dimensions.GetDim(memberList2Dim);
                            mem2List = api.Members.GetMembersUsingFilter(mbrList2_Dim.DimPk, memberList2Filter);
                        }
                        if (!string.IsNullOrEmpty(memberList3Dim) && !string.IsNullOrEmpty(memberList3Filter))
                        {
                            mbrList3_Dim = api.Dimensions.GetDim(memberList3Dim);
                            mem3List = api.Members.GetMembersUsingFilter(mbrList3_Dim.DimPk, memberList3Filter);
                        }
                        if (!string.IsNullOrEmpty(memberList4Dim) && !string.IsNullOrEmpty(memberList4Filter))
                        {
                            mbrList4_Dim = api.Dimensions.GetDim(memberList4Dim);
                            mem4List = api.Members.GetMembersUsingFilter(mbrList4_Dim.DimPk, memberList4Filter);
                        }

                        clearCubeData = new DataBuffer();


                        // loop through memberlist and execute databaffer code
                        if (mbrList1_Dim != null)
                        {
                            foreach (var member1 in mem1List)
                            {
                                if (mbrList2_Dim != null)
                                {

                                    foreach (var member2 in mem2List)
                                    {

                                        if (mbrList3_Dim != null)
                                        {

                                            foreach (var member3 in mem3List)
                                            {

                                                if (mbrList4_Dim != null)
                                                {

                                                    foreach (var member4 in mem4List)
                                                    {
                                                        ExecuteDataBufferCode(os_Calc_ID, ref currCubeBuffer, ref destBuffer, ref clearCubeData, destInfo, member1.Member.Name, member2.Member.Name, member3.Member.Name, member4.Member.Name, reader, balanced_buffer_calc, Unbalanced_Calc);
                                                    }

                                                }
                                                else
                                                {
                                                    ExecuteDataBufferCode(os_Calc_ID, ref currCubeBuffer, ref destBuffer, ref clearCubeData, destInfo, member1.Member.Name, member2.Member.Name, member3.Member.Name, "", reader, balanced_buffer_calc, Unbalanced_Calc);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ExecuteDataBufferCode(os_Calc_ID, ref currCubeBuffer, ref destBuffer, ref clearCubeData, destInfo, member1.Member.Name, member2.Member.Name, "", "", reader, balanced_buffer_calc, Unbalanced_Calc);
                                        }
                                    }
                                }
                                else
                                {
                                    ExecuteDataBufferCode(os_Calc_ID, ref currCubeBuffer, ref destBuffer, ref clearCubeData, destInfo, member1.Member.Name, "", "", "", reader, balanced_buffer_calc, Unbalanced_Calc);
                                }
                            }
                        }


                        // Write calculated cells from above back to final output data buffer
                        api.Data.SetDataBuffer(destBuffer, destInfo);
                        destBuffer.DataBufferCells.Clear();

                        foreach (var ClearCubeCell in currCubeBuffer.DataBufferCells.Values)
                        {
                            var status = new DataCellStatus(false);
                            var clearCell = new DataBufferCell(ClearCubeCell.DataBufferCellPk, 0, status);
                            clearCubeData.SetCell(si, clearCell);
                        }

                        var clearInfo = api.Data.GetExpressionDestinationInfo($"{dest_view}");
                        api.Data.SetDataBuffer(clearCubeData, clearInfo);
                    }

                }
            }
        }

        #endregion

        #region "Extensibility Unbal Helpers"

        private void ExecuteDataBufferCode(int os_Calc_ID, ref DataBuffer currCubeBuffer, ref DataBuffer destBuffer, ref DataBuffer clearCubeBuffer, ExpressionDestinationInfo destInfo, string dim1MemName, string dim2MemName, string dim3MemName, string dim4MemName,
            SqlDataReader reader, string balanced_buffer_calc, string Unbalanced_Calc)
        {
            unbal_srcdataBuffers.Clear();
            string memFilterStringPrefix = "|!MemberList_";
            string memFilterStringSuffix = "_Filter.Name!|";

            string src_cell_SQL = @"
							        SELECT OS_Calc_Src_ID_Order, OS_Calc_Open_Parens, OS_Calc_Math_Operator, OS_Src_Entity, OS_Src_Cons, OS_Src_Scenario,
							            OS_Src_Time, OS_Src_Origin, OS_Src_IC, OS_Src_View, OS_Src_Acct, OS_Src_Flow, OS_Src_UD1, OS_Src_UD2,
							            OS_Src_UD3, OS_Src_UD4, OS_Src_UD5, OS_Src_UD6, OS_Src_UD7, OS_Src_UD8, OS_Calc_Close_Parens,Unbalanced_Src_Cell_Buffer,
							            Unbalanced_Origin_Override,Unbalanced_IC_Override,Unbalanced_Acct_Override,Unbalanced_Flow_Override,Unbalanced_UD1_Override,
										Unbalanced_UD2_Override,Unbalanced_UD3_Override,Unbalanced_UD4_Override,Unbalanced_UD5_Override,Unbalanced_UD6_Override,
										Unbalanced_UD7_Override,Unbalanced_UD8_Override,Unbalanced_Src_Cell_Buffer_Filter,OS_Dynamic_Calc_Script,Override_Dest_Value,
										OS_Calc_Src_Type
							        FROM MCM_Src_Cell
							        WHERE OS_Calc_ID = @OS_Calc_ID
									AND OS_Calc_Src_ID_Order > 1
							        ORDER BY OS_Calc_Src_ID_Order";
            // Prepare and execute the source data query
            var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si);
            using (var connection = new SqlConnection(dbConnApp.ConnectionString))
            {
                connection.Open();

                var unbal_srcCommand = new SqlCommand(src_cell_SQL, connection);
                unbal_srcCommand.Parameters.Add(new SqlParameter("@OS_Calc_ID", SqlDbType.Int) { Value = os_Calc_ID });



                var src_buffers = 0;
                var srcCellTable = new DataTable("srcCellTable");

                using (var unbal_srcreader = unbal_srcCommand.ExecuteReader())
                {
                    unbal_srcreader.Read();
                    // setup table with all expected columns
                    for (int i = 0; i < unbal_srcreader.FieldCount; i++)
                    {
                        srcCellTable.Columns.Add(unbal_srcreader.GetName(i));
                    }

                    string member = "";
                    string calc_srcType = unbal_srcreader["OS_Calc_Src_Type"].ToString();
                    var row = srcCellTable.NewRow();
                    for (int i = 0; i < unbal_srcreader.FieldCount; i++)
                    {
                        string tempstr = unbal_srcreader[i].ToString();
                        //BRApi.ErrorLog.LogMessage(si, "field name: " + unbal_srcreader.GetName(i) + " field val: " + tempstr);
                        if (tempstr.XFContainsIgnoreCase(memFilterStringPrefix))
                        {
                            int listNum = Convert.ToInt16(tempstr.Substring(unbal_srcreader[i].ToString().IndexOf(memFilterStringPrefix) + memFilterStringPrefix.Length, 1));
                            switch (listNum)
                            {
                                case 1:
                                    member = dim1MemName; // passed in member name for dim1
                                    break;
                                case 2:
                                    member = dim2MemName; // passed in member name for dim2
                                    break;
                                case 3:
                                    member = dim3MemName; // passed in member name for dim3
                                    break;
                                case 4:
                                    member = dim4MemName; // passed in member name for dim4
                                    break;
                                _:
                                    break;
                            }
                            tempstr = tempstr.Replace(memFilterStringPrefix + listNum.ToString() + memFilterStringSuffix, member); // member is the member that is passed into the function based on switch statement above
                        }
                        row[unbal_srcreader.GetName(i)] = tempstr;
                    }

                    srcCellTable.Rows.Add(row);

                    int index = Convert.ToInt32(row["OS_Calc_Src_ID_Order"]);
                    src_buffers = index;

                    if (calc_srcType != "Dynamic Calc")
                    {
                        string unbal_srcBuffer = row["Unbalanced_Src_Cell_Buffer"].ToString();
                        string unbal_srcBufferFilter = row["Unbalanced_Src_Cell_Buffer_Filter"].ToString();
                        string bufferName = $"SrcBuffer{index}";

                        // Dynamically create and store the data buffer
                        var dataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({unbal_srcBuffer}, {unbal_srcBufferFilter})");
                        //dataBuffer.LogDataBuffer(api, "unbalsrcbuffer?", 500);
                        if (!unbal_srcdataBuffers.ContainsKey(bufferName))
                        {
                            unbal_srcdataBuffers.Add(bufferName, dataBuffer);
                        }
                    }

                    //		            while (unbal_srcreader.Read())
                    //		            {


                    //	                    row["OS_Calc_Src_ID_Order"] = unbal_srcreader["OS_Calc_Src_ID_Order"];
                    //	                    row["OS_Calc_Open_Parens"] = unbal_srcreader["OS_Calc_Open_Parens"];
                    //	                    row["OS_Calc_Math_Operator"] = unbal_srcreader["OS_Calc_Math_Operator"];
                    //	                    row["OS_Src_Entity"] = unbal_srcreader["OS_Src_Entity"];
                    //	                    row["OS_Src_Cons"] = unbal_srcreader["OS_Src_Cons"];
                    //	                    row["OS_Src_Scenario"] = unbal_srcreader["OS_Src_Scenario"];
                    //	                    row["OS_Src_Time"] = unbal_srcreader["OS_Src_Time"];
                    //	                    row["OS_Src_Origin"] = unbal_srcreader["OS_Src_Origin"];
                    //	                    row["OS_Src_IC"] = unbal_srcreader["OS_Src_IC"];
                    //	                    row["OS_Src_View"] = unbal_srcreader["OS_Src_View"];
                    //	                    row["OS_Src_Acct"] = unbal_srcreader["OS_Src_Acct"];
                    //	                    row["OS_Src_Flow"] = unbal_srcreader["OS_Src_Flow"];
                    //	                    row["OS_Src_UD1"] = unbal_srcreader["OS_Src_UD1"];
                    //	                    row["OS_Src_UD2"] = unbal_srcreader["OS_Src_UD2"];
                    //	                    row["OS_Src_UD3"] = unbal_srcreader["OS_Src_UD3"];
                    //	                    row["OS_Src_UD4"] = unbal_srcreader["OS_Src_UD4"];
                    //	                    row["OS_Src_UD5"] = unbal_srcreader["OS_Src_UD5"];
                    //	                    row["OS_Src_UD6"] = unbal_srcreader["OS_Src_UD6"];
                    //	                    row["OS_Src_UD7"] = unbal_srcreader["OS_Src_UD7"];
                    //	                    row["OS_Src_UD8"] = unbal_srcreader["OS_Src_UD8"];
                    //	                    row["OS_Calc_Close_Parens"] = unbal_srcreader["OS_Calc_Close_Parens"];
                    //	                    row["Unbalanced_Src_Cell_Buffer"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer"];
                    //	                    row["Unbalanced_Origin_Override"] = unbal_srcreader["Unbalanced_Origin_Override"];
                    //	                    row["Unbalanced_IC_Override"] = unbal_srcreader["Unbalanced_IC_Override"];
                    //	                    row["Unbalanced_Acct_Override"] = unbal_srcreader["Unbalanced_Acct_Override"];
                    //	                    row["Unbalanced_Flow_Override"] = unbal_srcreader["Unbalanced_Flow_Override"];
                    //	                    row["Unbalanced_UD1_Override"] = unbal_srcreader["Unbalanced_UD1_Override"];
                    //	                    row["Unbalanced_UD2_Override"] = unbal_srcreader["Unbalanced_UD2_Override"];
                    //	                    row["Unbalanced_UD3_Override"] = unbal_srcreader["Unbalanced_UD3_Override"];
                    //	                    row["Unbalanced_UD4_Override"] = unbal_srcreader["Unbalanced_UD4_Override"];
                    //	                    row["Unbalanced_UD5_Override"] = unbal_srcreader["Unbalanced_UD5_Override"];
                    //	                    row["Unbalanced_UD6_Override"] = unbal_srcreader["Unbalanced_UD6_Override"];
                    //	                    row["Unbalanced_UD7_Override"] = unbal_srcreader["Unbalanced_UD7_Override"];
                    //	                    row["Unbalanced_UD8_Override"] = unbal_srcreader["Unbalanced_UD8_Override"];
                    //	                    row["Unbalanced_Src_Cell_Buffer_Filter"] = unbal_srcreader["Unbalanced_Src_Cell_Buffer_Filter"];
                    //	                    row["OS_Dynamic_Calc_Script"] = unbal_srcreader["OS_Dynamic_Calc_Script"];
                    //	                    row["Override_Dest_Value"] = unbal_srcreader["Override_Dest_Value"];
                    //	                    row["OS_Calc_Src_Type"] = unbal_srcreader["OS_Calc_Src_Type"];

                    //						srcCellTable.Rows.Add(row);

                    //						int index = (int)unbal_srcreader["OS_Calc_Src_ID_Order"];
                    //						src_buffers = index;

                    //						if (calc_srcType != "Dynamic Calc")
                    //						{	
                    //							string unbal_srcBuffer = unbal_srcreader["Unbalanced_Src_Cell_Buffer"].ToString();
                    //			                string unbal_srcBufferFilter = unbal_srcreader["OS_Src_Buffer_Filter"].ToString();
                    //			                string bufferName = $"SrcBuffer{index}";

                    //			                // Dynamically create and store the data buffer
                    //			                var dataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({unbal_srcBuffer}, {unbal_srcBufferFilter})");
                    //			                unbal_srcdataBuffers.Add(bufferName, dataBuffer);
                    //						}
                    //					} 
                }

                var destCellTable = new DataTable("destCellTable");

                //setup table with expected columns
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    destCellTable.Columns.Add(reader.GetName(i));
                }


                var destrow = destCellTable.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    string member = "";
                    string tempstr = reader[i].ToString();
                    if (tempstr.XFContainsIgnoreCase(memFilterStringPrefix))
                    {
                        int listNum = Convert.ToInt16(tempstr.Substring(reader[i].ToString().IndexOf(memFilterStringPrefix) + memFilterStringPrefix.Length, 1));
                        switch (listNum)
                        {
                            case 1:
                                member = dim1MemName; // passed in member name for dim1
                                break;
                            case 2:
                                member = dim2MemName; // passed in member name for dim2
                                break;
                            case 3:
                                member = dim3MemName; // passed in member name for dim3
                                break;
                            case 4:
                                member = dim4MemName; // passed in member name for dim4
                                break;
                            _:
                                break;
                        }
                        tempstr = tempstr.Replace(memFilterStringPrefix + listNum.ToString() + memFilterStringSuffix, member); // member is the member that is passed into the function based on switch statement above
                    }
                    destrow[reader.GetName(i)] = tempstr;
                }
                string dest_acct = destrow["OS_Target_Acct"] != DBNull.Value ? destrow["OS_Target_Acct"].ToString() : null;
                string dest_view = destrow["OS_Target_View"] != DBNull.Value ? destrow["OS_Target_View"].ToString() : null;
                string dest_origin = destrow["OS_Target_Origin"] != DBNull.Value ? destrow["OS_Target_Origin"].ToString() : null;
                string dest_ic = destrow["OS_Target_IC"] != DBNull.Value ? destrow["OS_Target_IC"].ToString() : null;
                string dest_flow = destrow["OS_Target_Flow"] != DBNull.Value ? destrow["OS_Target_Flow"].ToString() : null;
                string dest_ud1 = destrow["OS_Target_UD1"] != DBNull.Value ? destrow["OS_Target_UD1"].ToString() : null;
                string dest_ud2 = destrow["OS_Target_UD2"] != DBNull.Value ? destrow["OS_Target_UD2"].ToString() : null;
                string dest_ud3 = destrow["OS_Target_UD3"] != DBNull.Value ? destrow["OS_Target_UD3"].ToString() : null;
                string dest_ud4 = destrow["OS_Target_UD4"] != DBNull.Value ? destrow["OS_Target_UD4"].ToString() : null;
                string dest_ud5 = destrow["OS_Target_UD5"] != DBNull.Value ? destrow["OS_Target_UD5"].ToString() : null;
                string dest_ud6 = destrow["OS_Target_UD6"] != DBNull.Value ? destrow["OS_Target_UD6"].ToString() : null;
                string dest_ud7 = destrow["OS_Target_UD7"] != DBNull.Value ? destrow["OS_Target_UD7"].ToString() : null;
                string dest_ud8 = destrow["OS_Target_UD8"] != DBNull.Value ? destrow["OS_Target_UD8"].ToString() : null;

                List<string> destInfos = new List<string> {
                        dest_acct, dest_view, dest_origin, dest_ic, dest_flow, dest_ud1, dest_ud2, dest_ud3, dest_ud4, dest_ud5, dest_ud6, dest_ud7, dest_ud8
                    };

                for (int i = 0; i < destInfos.Count; i++)
                {
                    string tempstr = destInfos[i];
                    if (tempstr != null && tempstr.Contains("#"))
                    {
                        destInfos[i] = tempstr.Remove(0, tempstr.IndexOf("#") + 1);
                    }
                }

                var os_Curr_Cube_Buffer = destrow["OS_Curr_Cube_Buffer_Filter"].ToString();
                var os_Src_Buffer = destrow["OS_Src_Buffer_Filter"].ToString();

                //BRApi.ErrorLog.LogMessage(si, "balBufferCalc: " + balanced_buffer_calc + " srcbuff: " + os_Src_Buffer);
                //BRApi.ErrorLog.LogMessage(si, "entity: " + api.Pov.Entity.Name + " time: " + api.Pov.Time.Name);

                var balancedBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers({balanced_buffer_calc},{os_Src_Buffer})");
                //balancedBuffer.LogDataBuffer(api, "databuffer:",500);
                if (balancedBuffer.DataBufferCells.Values.Count > 0)
                {
                    BRApi.ErrorLog.LogMessage(si, "balancedBuffer cell count: " + balancedBuffer.DataBufferCells.Values.Count);
                    balancedBuffer.LogDataBuffer(api, "balbuffer", 500);
                }

                foreach (var balancedBuffer_Cell in balancedBuffer.DataBufferCells.Values)
                {
                    //Need to set the Dest Cell correctly so that the clear routine works
                    //			            var destCell = new DataBufferCell(UpdateCellDefinition(balancedBuffer_Cell, dest_acct, dest_flow, dest_origin,
                    //							dest_ic,dest_ud1,dest_ud2,dest_ud3,dest_ud4,dest_ud5,dest_ud6,dest_ud7,dest_ud8));
                    var destCell = new DataBufferCell(UpdateCellDefinition(balancedBuffer_Cell, destInfos[0], destInfos[4], destInfos[2],
                        destInfos[3], destInfos[5], destInfos[6], destInfos[7], destInfos[8], destInfos[9], destInfos[10], destInfos[11], destInfos[12]));

                    //						BRApi.ErrorLog.LogMessage(si, "prechange acct: " + destCell.GetAccountName(api));
                    //						destCell.SetAccount(api, dest_acct);
                    //						BRApi.ErrorLog.LogMessage(si, "post acct: " + destCell.GetAccountName(api));

                    BRApi.ErrorLog.LogMessage(si, "dest cell info: " + destCell.DataBufferCellPk.GetMemberScript(api));
                    BRApi.ErrorLog.LogMessage(si, "balbuff cell val: " + balancedBuffer_Cell.CellAmount);


                    //Call new function, pass in balancedbuffer_cell and first unbal src row...  Build up calc string.  Run math  
                    var table = new DataTable();

                    var expression = Unbalanced_Calc.Replace("BalancedBuffer", balancedBuffer_Cell.CellAmount.XFToString());
                    //If this is Dynamic Calc, then call Dyn Calc
                    for (int i = 2; i <= src_buffers; i++)
                    {
                        var srcRow = srcCellTable.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["OS_Calc_Src_ID_Order"]) == i);
                        if (srcRow != null)
                        {
                            if (srcRow["OS_Calc_Src_Type"].ToString() == "Dynamic Calc")
                            {
                                var dyn_Calc = getdynamic_calc_value(balancedBuffer_Cell, srcRow);
                                expression = Unbalanced_Calc.Replace($"DynamicCalc{i}", dyn_Calc.XFToString());

                            }
                            else if (srcRow["OS_Calc_Src_Type"].ToString() == "Stored Cell")
                            {
                                string searchName = $"SrcBuffer{i}";
                                var resultBuffer = new DataBuffer();

                                BRApi.ErrorLog.LogMessage(si, "searching for " + searchName);
                                if (unbal_srcdataBuffers.ContainsKey(searchName))
                                {
                                    // The key was found, and resultBuffer now holds the corresponding DataBuffer
                                    BRApi.ErrorLog.LogMessage(si, "Buffer found for the name: " + searchName);
                                    resultBuffer = unbal_srcdataBuffers.XFGetValue(searchName, resultBuffer);
                                    resultBuffer.LogDataBuffer(api, "result buffer", 500);
                                }

                                var src_buffer_Value = get_src_unbal_buffer_value(balancedBuffer_Cell, resultBuffer, srcRow); // this value is currently coming back as 0
                                expression = expression.Replace($"SrcBufferValue{i}", src_buffer_Value.XFToString());

                            }
                        }
                        else
                        {
                            BRApi.ErrorLog.LogMessage(si, "Did not find src row");
                        }
                    }
                    BRApi.ErrorLog.LogMessage(si, "final express: " + expression);
                    // Use the Compute method to evaluate the expression
                    var result = table.Compute(expression, string.Empty);
                    UpdateValue(ref destCell, ref currCubeBuffer, ref destBuffer, Convert.ToDecimal(result));
                    currCubeBuffer.DataBufferCells.Remove(destCell.DataBufferCellPk);
                }
            }
        }
        #endregion

        #region "Unbalanced Buffer Helpers"

        private decimal getdynamic_calc_value(DataBufferCell balanced_Src_Cell, DataRow srcRow)
        {
            var dyncalc_mbrScriptBuilder = api.Data.CreateMemberScriptBuilder(srcRow["OS_Dynamic_Calc_Script"].ToString());
            api.Data.ApplyDataBufferCellPkToMemberScriptBuilder(dyncalc_mbrScriptBuilder, balanced_Src_Cell.DataBufferCellPk);
            // Apply overrides if they are set
            if (srcRow["Unbalanced_Origin_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Origin_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetOrigin(srcRow["OS_Src_Origin"].ToString().Replace("O#", string.Empty));
            }

            if (srcRow["Unbalanced_IC_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_IC_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetIC(srcRow["OS_Src_IC"].ToString().Replace("IC#", string.Empty));
            }

            if (srcRow["Unbalanced_Acct_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Acct_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetAccount(srcRow["OS_Src_Acct"].ToString().Replace("A#", string.Empty));
            }

            if (srcRow["Unbalanced_Flow_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Flow_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetFlow(srcRow["OS_Src_Flow"].ToString().Replace("F#", string.Empty));
            }

            if (srcRow["Unbalanced_UD1_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD1_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD1(srcRow["OS_Src_UD1"].ToString().Replace("UD1#", string.Empty).Replace("U1#", string.Empty));
            }

            if (srcRow["Unbalanced_UD2_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD2_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD2(srcRow["OS_Src_UD2"].ToString().Replace("UD2#", string.Empty).Replace("U2#", string.Empty));
            }

            if (srcRow["Unbalanced_UD3_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD3_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD3(srcRow["OS_Src_UD3"].ToString().Replace("UD3#", string.Empty).Replace("U3#", string.Empty));
            }

            if (srcRow["Unbalanced_UD4_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD4_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD4(srcRow["OS_Src_UD4"].ToString().Replace("UD4#", string.Empty).Replace("U4#", string.Empty));
            }

            if (srcRow["Unbalanced_UD5_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD5_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD5(srcRow["OS_Src_UD5"].ToString().Replace("UD5#", string.Empty).Replace("U5#", string.Empty));
            }

            if (srcRow["Unbalanced_UD6_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD6_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD6(srcRow["OS_Src_UD6"].ToString().Replace("UD6#", string.Empty).Replace("U6#", string.Empty));
            }

            if (srcRow["Unbalanced_UD7_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD7_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD7(srcRow["OS_Src_UD7"].ToString().Replace("UD7#", string.Empty).Replace("U7#", string.Empty));
            }

            if (srcRow["Unbalanced_UD8_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD8_Override"].ToString()))
            {
                dyncalc_mbrScriptBuilder.SetUD8(srcRow["OS_Src_UD8"].ToString().Replace("UD8#", string.Empty).Replace("U8#", string.Empty));
            }
            var dyncalc_DataCell = api.Data.GetDataCell(dyncalc_mbrScriptBuilder.GetMemberScript());
            return dyncalc_DataCell.CellAmount;
        }

        private decimal get_src_unbal_buffer_value(DataBufferCell balanced_Src_Cell, DataBuffer Unbal_DataBuffer, DataRow srcRow)
        {

            var DriverDB_Acct = string.Empty;
            if (srcRow["Unbalanced_Acct_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Acct_Override"].ToString()))
            {
                DriverDB_Acct = srcRow["OS_Src_Acct"].ToString().Replace("A#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_Flow = string.Empty;
            if (srcRow["Unbalanced_Flow_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Flow_Override"].ToString()))
            {
                DriverDB_Flow = srcRow["OS_Src_Flow"].ToString().Replace("F#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_Origin = string.Empty;
            if (srcRow["Unbalanced_Origin_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_Origin_Override"].ToString()))
            {
                DriverDB_Origin = srcRow["OS_Src_Origin"].ToString().Replace("O#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_IC = string.Empty;
            if (srcRow["Unbalanced_IC_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_IC_Override"].ToString()))
            {
                DriverDB_IC = srcRow["OS_Src_IC"].ToString().Replace("IC#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD1 = string.Empty;
            if (srcRow["Unbalanced_UD1_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD1_Override"].ToString()))
            {
                DriverDB_UD1 = srcRow["OS_Src_UD1"].ToString().Replace("UD1#", string.Empty).Replace("U1#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD2 = string.Empty;
            if (srcRow["Unbalanced_UD2_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD2_Override"].ToString()))
            {
                DriverDB_UD2 = srcRow["OS_Src_UD2"].ToString().Replace("UD2#", string.Empty).Replace("U2#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD3 = string.Empty;
            if (srcRow["Unbalanced_UD3_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD3_Override"].ToString()))
            {
                DriverDB_UD3 = srcRow["OS_Src_UD3"].ToString().Replace("UD3#", string.Empty).Replace("U3#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD4 = string.Empty;
            if (srcRow["Unbalanced_UD4_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD4_Override"].ToString()))
            {
                DriverDB_UD4 = srcRow["OS_Src_UD4"].ToString().Replace("UD4#", string.Empty).Replace("U4#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD5 = string.Empty;
            if (srcRow["Unbalanced_UD5_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD5_Override"].ToString()))
            {
                DriverDB_UD5 = srcRow["OS_Src_UD5"].ToString().Replace("UD5#", string.Empty).Replace("U5#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD6 = string.Empty;
            if (srcRow["Unbalanced_UD6_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD6_Override"].ToString()))
            {
                DriverDB_UD6 = srcRow["OS_Src_UD6"].ToString().Replace("UD6#", string.Empty).Replace("U6#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD7 = string.Empty;
            if (srcRow["Unbalanced_UD7_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD7_Override"].ToString()))
            {
                DriverDB_UD7 = srcRow["OS_Src_UD7"].ToString().Replace("UD7#", string.Empty).Replace("U7#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var DriverDB_UD8 = string.Empty;
            if (srcRow["Unbalanced_UD8_Override"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["Unbalanced_UD8_Override"].ToString()))
            {
                DriverDB_UD8 = srcRow["OS_Src_UD8"].ToString().Replace("UD8#", string.Empty).Replace("U8#", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
            }
            var bufferCellValue = GetBCValue(ref balanced_Src_Cell,
            Unbal_DataBuffer,
            DriverDB_Acct,
            DriverDB_Flow,
            DriverDB_Origin,
            DriverDB_IC,
            DriverDB_UD1,
            DriverDB_UD2,
            DriverDB_UD3,
            DriverDB_UD4,
            DriverDB_UD5,
            DriverDB_UD6,
            DriverDB_UD7,
            DriverDB_UD8);

            return bufferCellValue;
        }

        private List<Func<DataBufferCell, bool>> get_filter_Conditions(DataBufferCell balanced_Src_Cell, DataRow srcRow)
        {
            var filterConditions = new List<Func<DataBufferCell, bool>>();
            //Origin
            if (srcRow["OS_Src_Origin"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_Origin"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetOriginName(api) == balanced_Src_Cell.DataBufferCellPk.GetOriginName(api));
            }
            // Flow
            if (srcRow["OS_Src_Flow"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_Flow"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetFlowName(api) == balanced_Src_Cell.DataBufferCellPk.GetFlowName(api));
            }

            // Account
            if (srcRow["OS_Src_Acct"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_Acct"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetAccountName(api) == balanced_Src_Cell.DataBufferCellPk.GetAccountName(api));
            }

            // IC
            if (srcRow["OS_Src_IC"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_IC"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetICName(api) == balanced_Src_Cell.DataBufferCellPk.GetICName(api));
            }

            // UD1
            if (srcRow["OS_Src_UD1"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD1"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD1Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD1Name(api));
            }

            // UD2
            if (srcRow["OS_Src_UD2"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD2"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD2Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD2Name(api));
            }

            // UD3
            if (srcRow["OS_Src_UD3"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD3"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD3Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD3Name(api));
            }

            // UD4
            if (srcRow["OS_Src_UD4"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD4"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD4Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD4Name(api));
            }

            // UD5
            if (srcRow["OS_Src_UD5"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD5"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD5Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD5Name(api));
            }

            // UD6
            if (srcRow["OS_Src_UD6"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD6"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD6Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD6Name(api));
            }

            // UD7
            if (srcRow["OS_Src_UD7"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD7"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD7Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD7Name(api));
            }

            // UD8
            if (srcRow["OS_Src_UD8"] != DBNull.Value && !string.IsNullOrEmpty(srcRow["OS_Src_UD8"].ToString()))
            {
                filterConditions.Add(cell => cell.DataBufferCellPk.GetUD8Name(api) == balanced_Src_Cell.DataBufferCellPk.GetUD8Name(api));
            }

            return filterConditions;
        }

        private List<DataBufferCell> GetFilteredCells(DataBuffer unbal_alloc, params Func<DataBufferCell, bool>[] filterConditions)
        {
            if (unbal_alloc == null || unbal_alloc.DataBufferCells == null)
            {
                return new List<DataBufferCell>(); // Return an empty list if unbal_alloc or DataBufferCells is null
            }

            return unbal_alloc.DataBufferCells.Values
                .Where(cell => filterConditions.All(condition => condition(cell)))
                .ToList();
        }

        private void UpdateExpressionForRow(DataBufferCell balancedBuffer_Cell, DataRow srcRow, Dictionary<string, DataBuffer> unbal_srcdataBuffers, ref Dictionary<DataBufferCell, string> cell_Expressions, DataBufferCell dest_Cell, int i)
        {
            var dest_acct = string.Empty;
            var dest_flow = string.Empty;
            var dest_origin = string.Empty;
            var dest_ic = string.Empty;
            var dest_ud1 = string.Empty;
            var dest_ud2 = string.Empty;
            var dest_ud3 = string.Empty;
            var dest_ud4 = string.Empty;
            var dest_ud5 = string.Empty;
            var dest_ud6 = string.Empty;
            var dest_ud7 = string.Empty;
            var dest_ud8 = string.Empty;
            // Ensure that there is an entry for this cell in the dictionary
            if (!cell_Expressions.ContainsKey(dest_Cell))
            {
                cell_Expressions[dest_Cell] = string.Empty; // or some initial expression if needed
            }

            // Retrieve the current expression for this cell
            string currentExpression = cell_Expressions[dest_Cell];


            if (srcRow["OS_Calc_Src_Type"].ToString() == "Dynamic Calc")
            {
                var dyn_Calc = getdynamic_calc_value(balancedBuffer_Cell, srcRow);
                currentExpression = currentExpression.Replace($"DynamicCalc{i}", dyn_Calc.XFToString());
            }
            else if (srcRow["OS_Calc_Src_Type"].ToString() == "Stored Cell")
            {
                string searchName = $"SrcBuffer{i}";
                var resultBuffer = new DataBuffer();

                // Find the buffer corresponding to the search name
                unbal_srcdataBuffers.XFGetValue(searchName, resultBuffer);

                // Get filter conditions based on the source row
                var unbal_alloc_filter_conditions = get_filter_Conditions(balancedBuffer_Cell, srcRow);

                // Apply the filter conditions to get the relevant cells
                var filtered_Cells = GetFilteredCells(resultBuffer, unbal_alloc_filter_conditions.ToArray());

                // Process the filtered cells
                foreach (var unbal_Alloc_Buffer_Cell in filtered_Cells)
                {
                    if (srcRow["Override_Dest_Value"] != DBNull.Value)
                    {
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("A#"))
                        {
                            dest_acct = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetAccountName(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("O#"))
                        {
                            dest_origin = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetOriginName(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("F#"))
                        {
                            dest_flow = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetFlowName(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("IC#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("I#"))
                        {
                            dest_ic = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetICName(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U1#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD1#"))
                        {
                            dest_ud1 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD1Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U2#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD2#"))
                        {
                            dest_ud2 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD2Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U3#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD3#"))
                        {
                            dest_ud3 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD3Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U4#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD4#"))
                        {
                            dest_ud4 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD4Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U5#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD5#"))
                        {
                            dest_ud5 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD5Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U6#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD6#"))
                        {
                            dest_ud6 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD6Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U7#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD7#"))
                        {
                            dest_ud7 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD7Name(api);
                        }
                        if (srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("U8#") || srcRow["Override_Dest_Value"].ToString().XFContainsIgnoreCase("UD8#"))
                        {
                            dest_ud8 = unbal_Alloc_Buffer_Cell.DataBufferCellPk.GetUD8Name(api);
                        }
                        var unbal_alloc_DestCell = new DataBufferCell(UpdateCellDefinition(dest_Cell, dest_acct, dest_flow, dest_origin,
                                dest_ic, dest_ud1, dest_ud2, dest_ud3, dest_ud4, dest_ud5, dest_ud6, dest_ud7, dest_ud8));
                        currentExpression = cell_Expressions[dest_Cell];
                        currentExpression = currentExpression.Replace($"SrcBuffer{i}", unbal_Alloc_Buffer_Cell.CellAmount.XFToString());
                        cell_Expressions.Add(unbal_alloc_DestCell, currentExpression);


                    }
                }
            }
        }



        #endregion


        #endregion

        #region Utility Functions

        /// <summary>
        /// Retrieves a Member object from a dictionary cache or loads it if not present.
        /// </summary>
        /// <param name="dm">The dimension to query from.</param>
        /// <param name="mbrName">The name of the member to retrieve.</param>
        /// <param name="api">The API instance used to retrieve members if not cached.</param>
        /// <returns>The requested Member object.</returns>
        public Member GetMember(ref Dim dm, string mbrName, ref FinanceRulesApi api)
        {
            string key = dm.Name + ":" + mbrName;
            if (memberDict == null)
                memberDict = new Dictionary<string, Member>();

            if (!memberDict.ContainsKey(key))
                memberDict[key] = api.Members.GetMember(dm.DimPk.DimTypeId, mbrName);

            return memberDict[key];
        }

        #endregion

    }
}