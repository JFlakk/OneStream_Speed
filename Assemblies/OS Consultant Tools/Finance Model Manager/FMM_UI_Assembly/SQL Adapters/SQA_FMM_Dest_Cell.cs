using System;
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
		                Cube_ID, Act_ID, Model_ID, Calc_ID, Dest_Cell_ID, 
		                Location, Calc_Plan_Units, Acct, View, 
		                Origin, IC, Flow, UD1, 
		                UD2, UD3, UD4, UD5, 
		                UD6, UD7, UD8, Time_Filter, 
		                Acct_Filter, Origin_Filter, IC_Filter, Flow_Filter, 
		                UD1_Filter, UD2_Filter, UD3_Filter, UD4_Filter, 
		                UD5_Filter, UD6_Filter, UD7_Filter, UD8_Filter, 
		                Conditional_Filter, Curr_Cube_Buffer_Filter, Buffer_Filter, 
		                Dest_Cell_Logic, SQL_Logic, Create_Date, Create_User, Update_Date, Update_User
		            ) VALUES (
		                @Cube_ID, @Act_ID, @Model_ID, @Calc_ID, @Dest_Cell_ID, 
		                @Location, @Calc_Plan_Units, @Acct, @View, 
		                @Origin, @IC, @Flow, @UD1, 
		                @UD2, @UD3, @UD4, @UD5, 
		                @UD6, @UD7, @UD8, @Time_Filter, 
		                @Acct_Filter, @Origin_Filter, @IC_Filter, @Flow_Filter, 
		                @UD1_Filter, @UD2_Filter, @UD3_Filter, @UD4_Filter, 
		                @UD5_Filter, @UD6_Filter, @UD7_Filter, @UD8_Filter, 
		                @Conditional_Filter, @Curr_Cube_Buffer_Filter, @Buffer_Filter, 
		                @Dest_Cell_Logic, @SQL_Logic, GETDATE(), @Create_User, GETDATE(), @Update_User
		            )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

                // Add parameters to the command
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.InsertCommand.Parameters.Add("@Calc_ID", SqlDbType.Int).SourceColumn = "Calc_ID";
                sqa.InsertCommand.Parameters.Add("@Dest_Cell_ID", SqlDbType.Int).SourceColumn = "Dest_Cell_ID";
                sqa.InsertCommand.Parameters.Add("@Location", SqlDbType.NVarChar, 50).SourceColumn = "Location";
                sqa.InsertCommand.Parameters.Add("@Calc_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Calc_Plan_Units";
                sqa.InsertCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.InsertCommand.Parameters.Add("@View", SqlDbType.NVarChar, 100).SourceColumn = "View";
                sqa.InsertCommand.Parameters.Add("@Origin", SqlDbType.NVarChar, 100).SourceColumn = "Origin";
                sqa.InsertCommand.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC";
                sqa.InsertCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                sqa.InsertCommand.Parameters.Add("@Time_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Time_Filter";
                sqa.InsertCommand.Parameters.Add("@Acct_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Acct_Filter";
                sqa.InsertCommand.Parameters.Add("@Origin_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Origin_Filter";
                sqa.InsertCommand.Parameters.Add("@IC_Filter", SqlDbType.NVarChar, 200).SourceColumn = "IC_Filter";
                sqa.InsertCommand.Parameters.Add("@Flow_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Flow_Filter";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@OS_UD{i}_Filter", SqlDbType.NVarChar, 200).SourceColumn = $"OS_UD{i}_Filter";
                }
                sqa.InsertCommand.Parameters.Add("@Conditional_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "Conditional_Filter";
                sqa.InsertCommand.Parameters.Add("@Curr_Cube_Buffer_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "Curr_Cube_Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@Buffer_Filter", SqlDbType.NVarChar, 2000).SourceColumn = "Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@Dest_Cell_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "Dest_Cell_Logic";
                sqa.InsertCommand.Parameters.Add("@SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "SQL_Logic";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).Value = si.UserName;
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).Value = si.UserName;

                // Define the update query and parameters
                string updateQuery = @"
		            UPDATE FMM_Dest_Cell SET
		                Cube_ID = @Cube_ID, Act_ID = @Act_ID, Model_ID = @Model_ID,
		                Calc_ID = @Calc_ID, Location = @Location, 
		                Calc_Plan_Units = @Calc_Plan_Units, Acct = @Acct, 
		                View = @View, Origin = @Origin, 
		                IC = @IC, Flow = @Flow, 
		                UD1 = @UD1, UD2 = @UD2, 
		                UD3 = @UD3, UD4 = @UD4, 
		                UD5 = @UD5, UD6 = @UD6, 
		                UD7 = @UD7, UD8 = @UD8, 
		                Time_Filter = @Time_Filter, Acct_Filter = @Acct_Filter, 
		                Origin_Filter = @Origin_Filter, IC_Filter = @IC_Filter, 
		                Flow_Filter = @Flow_Filter, UD1_Filter = @UD1_Filter, 
		                UD2_Filter = @UD2_Filter, UD3_Filter = @UD3_Filter, 
		                UD4_Filter = @UD4_Filter, UD5_Filter = @UD5_Filter, 
		                UD6_Filter = @UD6_Filter, UD7_Filter = @UD7_Filter, 
		                UD8_Filter = @UD8_Filter, Conditional_Filter = @Conditional_Filter, 
		                Curr_Cube_Buffer_Filter = @Curr_Cube_Buffer_Filter, 
		                Buffer_Filter = @Buffer_Filter, Dest_Cell_Logic = @Dest_Cell_Logic, 
		                SQL_Logic = @SQL_Logic, Update_Date = GETDATE(), Update_User = @Update_User
		            WHERE Dest_Cell_ID = @Dest_Cell_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

                // Add parameters for the update command
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Dest_Cell_ID", SqlDbType.Int) { SourceColumn = "Dest_Cell_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.UpdateCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.UpdateCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.UpdateCommand.Parameters.Add("@Calc_ID", SqlDbType.Int).SourceColumn = "Calc_ID";
                sqa.UpdateCommand.Parameters.Add("@Location", SqlDbType.NVarChar, 50).SourceColumn = "Location";
                sqa.UpdateCommand.Parameters.Add("@Calc_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Calc_Plan_Units";
                sqa.UpdateCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.UpdateCommand.Parameters.Add("@View", SqlDbType.NVarChar, 100).SourceColumn = "View";
                sqa.UpdateCommand.Parameters.Add("@Origin", SqlDbType.NVarChar, 100).SourceColumn = "Origin";
                sqa.UpdateCommand.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC";
                sqa.UpdateCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                sqa.UpdateCommand.Parameters.Add("@Time_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Time_Filter";
                sqa.UpdateCommand.Parameters.Add("@Acct_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Acct_Filter";
                sqa.UpdateCommand.Parameters.Add("@Origin_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Origin_Filter";
                sqa.UpdateCommand.Parameters.Add("@IC_Filter", SqlDbType.NVarChar, 200).SourceColumn = "IC_Filter";
                sqa.UpdateCommand.Parameters.Add("@Flow_Filter", SqlDbType.NVarChar, 200).SourceColumn = "Flow_Filter";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@OS_UD{i}_Filter", SqlDbType.NVarChar, 200).SourceColumn = $"OS_UD{i}_Filter";
                }
                sqa.UpdateCommand.Parameters.Add("@Conditional_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "Conditional_Filter";
                sqa.UpdateCommand.Parameters.Add("@Curr_Cube_Buffer_Filter", SqlDbType.NVarChar, 1000).SourceColumn = "Curr_Cube_Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@Buffer_Filter", SqlDbType.NVarChar, 2000).SourceColumn = "Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@Dest_Cell_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "Dest_Cell_Logic";
                sqa.UpdateCommand.Parameters.Add("@SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "SQL_Logic";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).Value = si.UserName;

                // Define the delete query and parameters
                string deleteQuery = @"
		            DELETE FROM FMM_Dest_Cell 
		            WHERE Dest_Cell_ID = @Dest_Cell_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Dest_Cell_ID", SqlDbType.Int) { SourceColumn = "Dest_Cell_ID", SourceVersion = DataRowVersion.Original });

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