﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using Microsoft.Data.SqlClient;
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
    public class SQA_FMM_Dest_Cell
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Dest_Cell(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Dest_Cell_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
        {
            using (SqlCommand command = new SqlCommand(sql, _connection))
            {
                command.CommandType = CommandType.Text;
                if (sqlparams != null)
                {
                    command.Parameters.AddRange(sqlparams);
                }

                sqa.SelectCommand = command;
                sqa.Fill(dt);
				command.Parameters.Clear();
				sqa.SelectCommand = null;
            }
        }

        public void Update_FMM_Dest_Cell(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
		            INSERT INTO FMM_Dest_Cell (
		                Cube_ID, Act_ID, Model_ID, OS_Calc_ID, OS_Dest_Cell_ID, 
		                OS_Target_Location, Calc_Plan_Units, OS_Target_Acct, OS_Target_View, 
		                OS_Target_Origin, OS_Target_IC, OS_Target_Flow, OS_Target_UD1, 
		                OS_Target_UD2, OS_Target_UD3, OS_Target_UD4, OS_Target_UD5, 
		                OS_Target_UD6, OS_Target_UD7, OS_Target_UD8, OS_Time_Filter, 
		                OS_Acct_Filter, OS_Origin_Filter, OS_IC_Filter, OS_Flow_Filter, 
		                OS_UD1_Filter, OS_UD2_Filter, OS_UD3_Filter, OS_UD4_Filter, 
		                OS_UD5_Filter, OS_UD6_Filter, OS_UD7_Filter, OS_UD8_Filter, 
		                OS_Conditional_Filter, OS_Curr_Cube_Buffer_Filter, OS_Src_Buffer_Filter, 
		                OS_Dest_Cell_Logic, OS_Dest_SQL_Logic, Create_Date, Create_User, Update_Date, Update_User
		            ) VALUES (
		                @Cube_ID, @Act_ID, @Model_ID, @OS_Calc_ID, @OS_Dest_Cell_ID, 
		                @OS_Target_Location, @Calc_Plan_Units, @OS_Target_Acct, @OS_Target_View, 
		                @OS_Target_Origin, @OS_Target_IC, @OS_Target_Flow, @OS_Target_UD1, 
		                @OS_Target_UD2, @OS_Target_UD3, @OS_Target_UD4, @OS_Target_UD5, 
		                @OS_Target_UD6, @OS_Target_UD7, @OS_Target_UD8, @OS_Time_Filter, 
		                @OS_Acct_Filter, @OS_Origin_Filter, @OS_IC_Filter, @OS_Flow_Filter, 
		                @OS_UD1_Filter, @OS_UD2_Filter, @OS_UD3_Filter, @OS_UD4_Filter, 
		                @OS_UD5_Filter, @OS_UD6_Filter, @OS_UD7_Filter, @OS_UD8_Filter, 
		                @OS_Conditional_Filter, @OS_Curr_Cube_Buffer_Filter, @OS_Src_Buffer_Filter, 
		                @OS_Dest_Cell_Logic, @OS_Dest_SQL_Logic, GETDATE(), @Create_User, GETDATE(), @Update_User
		            )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

                // Add parameters to the command
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_ID", SqlDbType.Int).SourceColumn = "OS_Calc_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Dest_Cell_ID", SqlDbType.Int).SourceColumn = "OS_Dest_Cell_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Target_Location", SqlDbType.NVarChar, 50).SourceColumn = "OS_Target_Location";
                sqa.InsertCommand.Parameters.Add("@Calc_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Calc_Plan_Units";
                sqa.InsertCommand.Parameters.Add("@OS_Target_Acct", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Acct";
                sqa.InsertCommand.Parameters.Add("@OS_Target_View", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_View";
                sqa.InsertCommand.Parameters.Add("@OS_Target_Origin", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Origin";
                sqa.InsertCommand.Parameters.Add("@OS_Target_IC", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_IC";
                sqa.InsertCommand.Parameters.Add("@OS_Target_Flow", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@OS_Target_UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"OS_Target_UD{i}";
                }
                sqa.InsertCommand.Parameters.Add("@OS_Time_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Time_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Acct_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Acct_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Origin_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Origin_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_IC_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_IC_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Flow_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Flow_Filter";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@OS_UD{i}_Filter", SqlDbType.NVarChar, 200).SourceColumn = $"OS_UD{i}_Filter";
                }
                sqa.InsertCommand.Parameters.Add("@OS_Conditional_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "OS_Conditional_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Curr_Cube_Buffer_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "OS_Curr_Cube_Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Buffer_Filter", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Src_Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Dest_Cell_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Dest_Cell_Logic";
                sqa.InsertCommand.Parameters.Add("@OS_Dest_SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Dest_SQL_Logic";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).Value = si.UserName;
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).Value = si.UserName;

                // Define the update query and parameters
                string updateQuery = @"
		            UPDATE FMM_Dest_Cell SET
		                Cube_ID = @Cube_ID, Act_ID = @Act_ID, Model_ID = @Model_ID,
		                OS_Calc_ID = @OS_Calc_ID, OS_Target_Location = @OS_Target_Location, 
		                Calc_Plan_Units = @Calc_Plan_Units, OS_Target_Acct = @OS_Target_Acct, 
		                OS_Target_View = @OS_Target_View, OS_Target_Origin = @OS_Target_Origin, 
		                OS_Target_IC = @OS_Target_IC, OS_Target_Flow = @OS_Target_Flow, 
		                OS_Target_UD1 = @OS_Target_UD1, OS_Target_UD2 = @OS_Target_UD2, 
		                OS_Target_UD3 = @OS_Target_UD3, OS_Target_UD4 = @OS_Target_UD4, 
		                OS_Target_UD5 = @OS_Target_UD5, OS_Target_UD6 = @OS_Target_UD6, 
		                OS_Target_UD7 = @OS_Target_UD7, OS_Target_UD8 = @OS_Target_UD8, 
		                OS_Time_Filter = @OS_Time_Filter, OS_Acct_Filter = @OS_Acct_Filter, 
		                OS_Origin_Filter = @OS_Origin_Filter, OS_IC_Filter = @OS_IC_Filter, 
		                OS_Flow_Filter = @OS_Flow_Filter, OS_UD1_Filter = @OS_UD1_Filter, 
		                OS_UD2_Filter = @OS_UD2_Filter, OS_UD3_Filter = @OS_UD3_Filter, 
		                OS_UD4_Filter = @OS_UD4_Filter, OS_UD5_Filter = @OS_UD5_Filter, 
		                OS_UD6_Filter = @OS_UD6_Filter, OS_UD7_Filter = @OS_UD7_Filter, 
		                OS_UD8_Filter = @OS_UD8_Filter, OS_Conditional_Filter = @OS_Conditional_Filter, 
		                OS_Curr_Cube_Buffer_Filter = @OS_Curr_Cube_Buffer_Filter, 
		                OS_Src_Buffer_Filter = @OS_Src_Buffer_Filter, OS_Dest_Cell_Logic = @OS_Dest_Cell_Logic, 
		                OS_Dest_SQL_Logic = @OS_Dest_SQL_Logic, Update_Date = GETDATE(), Update_User = @Update_User
		            WHERE OS_Dest_Cell_ID = @OS_Dest_Cell_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

                // Add parameters for the update command
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@OS_Dest_Cell_ID", SqlDbType.Int) { SourceColumn = "OS_Dest_Cell_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.UpdateCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.UpdateCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_ID", SqlDbType.Int).SourceColumn = "OS_Calc_ID";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_Location", SqlDbType.NVarChar, 50).SourceColumn = "OS_Target_Location";
                sqa.UpdateCommand.Parameters.Add("@Calc_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Calc_Plan_Units";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_Acct", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Acct";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_View", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_View";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_Origin", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Origin";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_IC", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_IC";
                sqa.UpdateCommand.Parameters.Add("@OS_Target_Flow", SqlDbType.NVarChar, 100).SourceColumn = "OS_Target_Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@OS_Target_UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"OS_Target_UD{i}";
                }
                sqa.UpdateCommand.Parameters.Add("@OS_Time_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Time_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Acct_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Acct_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Origin_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Origin_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_IC_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_IC_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Flow_Filter", SqlDbType.NVarChar, 200).SourceColumn = "OS_Flow_Filter";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@OS_UD{i}_Filter", SqlDbType.NVarChar, 200).SourceColumn = $"OS_UD{i}_Filter";
                }
                sqa.UpdateCommand.Parameters.Add("@OS_Conditional_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "OS_Conditional_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Curr_Cube_Buffer_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "OS_Curr_Cube_Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Buffer_Filter", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Src_Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Dest_Cell_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Dest_Cell_Logic";
                sqa.UpdateCommand.Parameters.Add("@OS_Dest_SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "OS_Dest_SQL_Logic";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).Value = si.UserName;

                // Define the delete query and parameters
                string deleteQuery = @"
		            DELETE FROM FMM_Dest_Cell 
		            WHERE OS_Dest_Cell_ID = @OS_Dest_Cell_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@OS_Dest_Cell_ID", SqlDbType.Int) { SourceColumn = "OS_Dest_Cell_ID", SourceVersion = DataRowVersion.Original });

                try
                {
                    sqa.Update(dt);
                    transaction.Commit();
					sqa.InsertCommand = null;
					sqa.UpdateCommand = null;
					sqa.DeleteCommand = null;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}