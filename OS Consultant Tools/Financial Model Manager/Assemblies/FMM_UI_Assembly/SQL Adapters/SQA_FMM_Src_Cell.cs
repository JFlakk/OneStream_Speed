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
    public class SQA_FMM_Src_Cell
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Src_Cell(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Src_Cell_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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
            }
        }

        public void Update_FMM_Src_Cell(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                INSERT INTO [dbo].[FMM_Src_Cell]
                (
                    [Cube_ID], [Activity_ID], [Model_ID], [OS_Calc_ID], [OS_Src_Cell_ID],
                    [OS_Calc_Src_ID_Order], [OS_Calc_Src_Type], [OS_Calc_Src_Item], 
                    [OS_Calc_Open_Parens], [OS_Calc_Math_Operator], [OS_Src_Entity], 
                    [OS_Src_Cons], [OS_Src_Scenario], [OS_Src_Time], [OS_Src_Origin], 
                    [OS_Src_IC], [OS_Src_View], [Src_Plan_Units], [OS_Src_Acct], 
                    [OS_Src_Flow], [OS_Src_UD1], [OS_Src_UD2], [OS_Src_UD3], 
                    [OS_Src_UD4], [OS_Src_UD5], [OS_Src_UD6], [OS_Src_UD7], 
                    [OS_Src_UD8], [OS_Calc_Close_Parens], [Unbalanced_Src_Cell_Buffer], 
                    [Unbalanced_Origin_Override], [Unbalanced_IC_Override], 
                    [Unbalanced_Acct_Override], [Unbalanced_Flow_Override], 
                    [Unbalanced_UD1_Override], [Unbalanced_UD2_Override], 
                    [Unbalanced_UD3_Override], [Unbalanced_UD4_Override], 
                    [Unbalanced_UD5_Override], [Unbalanced_UD6_Override], 
                    [Unbalanced_UD7_Override], [Unbalanced_UD8_Override], 
                    [Unbalanced_Src_Cell_Buffer_Filter], [OS_Dynamic_Calc_Script], 
                    [Override_Dest_Value], [Table_Calc_Expression], [Table_Join_Expression], 
                    [Table_Filter_Expression], [Map_Type], [Map_Source], 
                    [Map_Logic], [Src_SQL_Stmt], [Use_Temp_Table], [Temp_Table_Name], 
                    [Create_Date], [Create_User], [Update_Date], [Update_User]
                )
                VALUES
                (
                    @Cube_ID, @Activity_ID, @Model_ID, @OS_Calc_ID, @OS_Src_Cell_ID,
                    @OS_Calc_Src_ID_Order, @OS_Calc_Src_Type, @OS_Calc_Src_Item, 
                    @OS_Calc_Open_Parens, @OS_Calc_Math_Operator, @OS_Src_Entity, 
                    @OS_Src_Cons, @OS_Src_Scenario, @OS_Src_Time, @OS_Src_Origin, 
                    @OS_Src_IC, @OS_Src_View, @Src_Plan_Units, @OS_Src_Acct, 
                    @OS_Src_Flow, @OS_Src_UD1, @OS_Src_UD2, @OS_Src_UD3, 
                    @OS_Src_UD4, @OS_Src_UD5, @OS_Src_UD6, @OS_Src_UD7, 
                    @OS_Src_UD8, @OS_Calc_Close_Parens, @Unbalanced_Src_Cell_Buffer, 
                    @Unbalanced_Origin_Override, @Unbalanced_IC_Override, 
                    @Unbalanced_Acct_Override, @Unbalanced_Flow_Override, 
                    @Unbalanced_UD1_Override, @Unbalanced_UD2_Override, 
                    @Unbalanced_UD3_Override, @Unbalanced_UD4_Override, 
                    @Unbalanced_UD5_Override, @Unbalanced_UD6_Override, 
                    @Unbalanced_UD7_Override, @Unbalanced_UD8_Override, 
                    @Unbalanced_Src_Cell_Buffer_Filter, @OS_Dynamic_Calc_Script, 
                    @Override_Dest_Value, @Table_Calc_Expression, @Table_Join_Expression, 
                    @Table_Filter_Expression, @Map_Type, @Map_Source, 
                    @Map_Logic, @Src_SQL_Stmt, @Use_Temp_Table, @Temp_Table_Name, 
                    @Create_Date, @Create_User, @Update_Date, @Update_User
                )";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

                // Add parameters to the insert command
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Activity_ID", SqlDbType.Int).SourceColumn = "Activity_ID";
                sqa.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_ID", SqlDbType.Int).SourceColumn = "OS_Calc_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Cell_ID", SqlDbType.Int).SourceColumn = "OS_Src_Cell_ID";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Src_ID_Order", SqlDbType.Int).SourceColumn = "OS_Calc_Src_ID_Order";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Src_Type", SqlDbType.NVarChar, 20).SourceColumn = "OS_Calc_Src_Type";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Src_Item", SqlDbType.NVarChar, 50).SourceColumn = "OS_Calc_Src_Item";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Open_Parens", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Open_Parens";
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Math_Operator", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Math_Operator";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Entity", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Entity";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Cons", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Cons";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Scenario", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Scenario";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Time", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Time";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Origin", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Origin";
                sqa.InsertCommand.Parameters.Add("@OS_Src_IC", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_IC";
                sqa.InsertCommand.Parameters.Add("@OS_Src_View", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_View";
                sqa.InsertCommand.Parameters.Add("@Src_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Src_Plan_Units";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Acct", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Acct";
                sqa.InsertCommand.Parameters.Add("@OS_Src_Flow", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@OS_Src_UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"OS_Src_UD{i}";
                }
                sqa.InsertCommand.Parameters.Add("@OS_Calc_Close_Parens", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Close_Parens";
                sqa.InsertCommand.Parameters.Add("@Unbalanced_Src_Cell_Buffer", SqlDbType.NVarChar, 500).SourceColumn = "Unbalanced_Src_Cell_Buffer";
                sqa.InsertCommand.Parameters.Add("@Unbalanced_Origin_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Origin_Override";
                sqa.InsertCommand.Parameters.Add("@Unbalanced_IC_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_IC_Override";
                sqa.InsertCommand.Parameters.Add("@Unbalanced_Acct_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Acct_Override";
                sqa.InsertCommand.Parameters.Add("@Unbalanced_Flow_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Flow_Override";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@Unbalanced_UD{i}_Override", SqlDbType.NVarChar, 200).SourceColumn = $"Unbalanced_UD{i}_Override";
                }
                sqa.InsertCommand.Parameters.Add("@Unbalanced_Src_Cell_Buffer_Filter", SqlDbType.NVarChar, 500).SourceColumn = "Unbalanced_Src_Cell_Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@OS_Dynamic_Calc_Script", SqlDbType.NVarChar, 500).SourceColumn = "OS_Dynamic_Calc_Script";
                sqa.InsertCommand.Parameters.Add("@Override_Dest_Value", SqlDbType.NVarChar, 200).SourceColumn = "Override_Dest_Value";
                sqa.InsertCommand.Parameters.Add("@Table_Calc_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Calc_Expression";
                sqa.InsertCommand.Parameters.Add("@Table_Join_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Join_Expression";
                sqa.InsertCommand.Parameters.Add("@Table_Filter_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Filter_Expression";
                sqa.InsertCommand.Parameters.Add("@Map_Type", SqlDbType.NVarChar, 20).SourceColumn = "Map_Type";
                sqa.InsertCommand.Parameters.Add("@Map_Source", SqlDbType.NVarChar, 50).SourceColumn = "Map_Source";
                sqa.InsertCommand.Parameters.Add("@Map_Logic", SqlDbType.NVarChar, 100).SourceColumn = "Map_Logic";
                sqa.InsertCommand.Parameters.Add("@Src_SQL_Stmt", SqlDbType.NVarChar, 2000).SourceColumn = "Src_SQL_Stmt";
                sqa.InsertCommand.Parameters.Add("@Use_Temp_Table", SqlDbType.Bit).SourceColumn = "Use_Temp_Table";
                sqa.InsertCommand.Parameters.Add("@Temp_Table_Name", SqlDbType.NVarChar, 50).SourceColumn = "Temp_Table_Name";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
                UPDATE [dbo].[FMM_Src_Cell]
                SET [Cube_ID] = @Cube_ID, [Activity_ID] = @Activity_ID, [Model_ID] = @Model_ID, 
                    [OS_Calc_Src_ID_Order] = @OS_Calc_Src_ID_Order, [OS_Calc_Src_Type] = @OS_Calc_Src_Type,
                    [OS_Calc_Src_Item] = @OS_Calc_Src_Item, [OS_Calc_Open_Parens] = @OS_Calc_Open_Parens, 
                    [OS_Calc_Math_Operator] = @OS_Calc_Math_Operator, [OS_Src_Entity] = @OS_Src_Entity, 
                    [OS_Src_Cons] = @OS_Src_Cons, [OS_Src_Scenario] = @OS_Src_Scenario, 
                    [OS_Src_Time] = @OS_Src_Time, [OS_Src_Origin] = @OS_Src_Origin, 
                    [OS_Src_IC] = @OS_Src_IC, [OS_Src_View] = @OS_Src_View, 
                    [Src_Plan_Units] = @Src_Plan_Units, [OS_Src_Acct] = @OS_Src_Acct, 
                    [OS_Src_Flow] = @OS_Src_Flow, [OS_Src_UD1] = @OS_Src_UD1, 
                    [OS_Src_UD2] = @OS_Src_UD2, [OS_Src_UD3] = @OS_Src_UD3, 
                    [OS_Src_UD4] = @OS_Src_UD4, [OS_Src_UD5] = @OS_Src_UD5, 
                    [OS_Src_UD6] = @OS_Src_UD6, [OS_Src_UD7] = @OS_Src_UD7, 
                    [OS_Src_UD8] = @OS_Src_UD8, [OS_Calc_Close_Parens] = @OS_Calc_Close_Parens,
                    [Unbalanced_Src_Cell_Buffer] = @Unbalanced_Src_Cell_Buffer, 
                    [Unbalanced_Origin_Override] = @Unbalanced_Origin_Override,
                    [Unbalanced_IC_Override] = @Unbalanced_IC_Override, 
                    [Unbalanced_Acct_Override] = @Unbalanced_Acct_Override,
                    [Unbalanced_Flow_Override] = @Unbalanced_Flow_Override, 
                    [Unbalanced_UD1_Override] = @Unbalanced_UD1_Override,
                    [Unbalanced_UD2_Override] = @Unbalanced_UD2_Override, 
                    [Unbalanced_UD3_Override] = @Unbalanced_UD3_Override,
                    [Unbalanced_UD4_Override] = @Unbalanced_UD4_Override, 
                    [Unbalanced_UD5_Override] = @Unbalanced_UD5_Override,
                    [Unbalanced_UD6_Override] = @Unbalanced_UD6_Override, 
                    [Unbalanced_UD7_Override] = @Unbalanced_UD7_Override,
                    [Unbalanced_UD8_Override] = @Unbalanced_UD8_Override, 
                    [Unbalanced_Src_Cell_Buffer_Filter] = @Unbalanced_Src_Cell_Buffer_Filter,
                    [OS_Dynamic_Calc_Script] = @OS_Dynamic_Calc_Script, 
                    [Override_Dest_Value] = @Override_Dest_Value, 
                    [Table_Calc_Expression] = @Table_Calc_Expression, 
                    [Table_Join_Expression] = @Table_Join_Expression, 
                    [Table_Filter_Expression] = @Table_Filter_Expression, 
                    [Map_Type] = @Map_Type, [Map_Source] = @Map_Source,
                    [Map_Logic] = @Map_Logic, [Src_SQL_Stmt] = @Src_SQL_Stmt, 
                    [Use_Temp_Table] = @Use_Temp_Table, [Temp_Table_Name] = @Temp_Table_Name, 
                    [Update_Date] = @Update_Date, [Update_User] = @Update_User
                WHERE OS_Src_Cell_ID = @OS_Src_Cell_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

                // Add parameters for the update command
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@OS_Src_Cell_ID", SqlDbType.Int) { SourceColumn = "OS_Src_Cell_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.UpdateCommand.Parameters.Add("@Activity_ID", SqlDbType.Int).SourceColumn = "Activity_ID";
                sqa.UpdateCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Src_ID_Order", SqlDbType.Int).SourceColumn = "OS_Calc_Src_ID_Order";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Src_Type", SqlDbType.NVarChar, 20).SourceColumn = "OS_Calc_Src_Type";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Src_Item", SqlDbType.NVarChar, 50).SourceColumn = "OS_Calc_Src_Item";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Open_Parens", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Open_Parens";
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Math_Operator", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Math_Operator";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Entity", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Entity";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Cons", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Cons";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Scenario", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Scenario";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Time", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Time";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Origin", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Origin";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_IC", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_IC";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_View", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_View";
                sqa.UpdateCommand.Parameters.Add("@Src_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Src_Plan_Units";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Acct", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Acct";
                sqa.UpdateCommand.Parameters.Add("@OS_Src_Flow", SqlDbType.NVarChar, 100).SourceColumn = "OS_Src_Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@OS_Src_UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"OS_Src_UD{i}";
                }
                sqa.UpdateCommand.Parameters.Add("@OS_Calc_Close_Parens", SqlDbType.NVarChar, 10).SourceColumn = "OS_Calc_Close_Parens";
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_Src_Cell_Buffer", SqlDbType.NVarChar, 500).SourceColumn = "Unbalanced_Src_Cell_Buffer";
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_Origin_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Origin_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_IC_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_IC_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_Acct_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Acct_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_Flow_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbalanced_Flow_Override";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@Unbalanced_UD{i}_Override", SqlDbType.NVarChar, 200).SourceColumn = $"Unbalanced_UD{i}_Override";
                }
                sqa.UpdateCommand.Parameters.Add("@Unbalanced_Src_Cell_Buffer_Filter", SqlDbType.NVarChar, 500).SourceColumn = "Unbalanced_Src_Cell_Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@OS_Dynamic_Calc_Script", SqlDbType.NVarChar, 500).SourceColumn = "OS_Dynamic_Calc_Script";
                sqa.UpdateCommand.Parameters.Add("@Override_Dest_Value", SqlDbType.NVarChar, 200).SourceColumn = "Override_Dest_Value";
                sqa.UpdateCommand.Parameters.Add("@Table_Calc_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Calc_Expression";
                sqa.UpdateCommand.Parameters.Add("@Table_Join_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Join_Expression";
                sqa.UpdateCommand.Parameters.Add("@Table_Filter_Expression", SqlDbType.NVarChar, 1000).SourceColumn = "Table_Filter_Expression";
                sqa.UpdateCommand.Parameters.Add("@Map_Type", SqlDbType.NVarChar, 20).SourceColumn = "Map_Type";
                sqa.UpdateCommand.Parameters.Add("@Map_Source", SqlDbType.NVarChar, 50).SourceColumn = "Map_Source";
                sqa.UpdateCommand.Parameters.Add("@Map_Logic", SqlDbType.NVarChar, 100).SourceColumn = "Map_Logic";
                sqa.UpdateCommand.Parameters.Add("@Src_SQL_Stmt", SqlDbType.NVarChar, 2000).SourceColumn = "Src_SQL_Stmt";
                sqa.UpdateCommand.Parameters.Add("@Use_Temp_Table", SqlDbType.Bit).SourceColumn = "Use_Temp_Table";
                sqa.UpdateCommand.Parameters.Add("@Temp_Table_Name", SqlDbType.NVarChar, 50).SourceColumn = "Temp_Table_Name";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
		            DELETE FROM [dbo].[FMM_Src_Cell] 
		            WHERE OS_Src_Cell_ID = @OS_Src_Cell_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@OS_Src_Cell_ID", SqlDbType.Int) { SourceColumn = "OS_Src_Cell_ID", SourceVersion = DataRowVersion.Original });

                try
                {
                    sqa.Update(dt);
                    transaction.Commit();
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