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
				command.Parameters.Clear();
				sqa.SelectCommand = null;
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
                    [Cube_ID], [Act_ID], [Model_ID], [Calc_ID], [Cell_ID],
                    [Src_Order], [Src_Type], [Src_Item], 
                    [Open_Parens], [Math_Operator], [Entity], 
                    [Cons], [Scenario], [Time], [Origin], 
                    [IC], [View], [Src_Plan_Units], [Acct], 
                    [Flow], [UD1], [UD2], [UD3], 
                    [UD4], [UD5], [UD6], [UD7], 
                    [UD8], [Close_Parens], [Unbal_Src_Cell_Buffer], 
                    [Unbal_Origin_Override], [Unbal_IC_Override], 
                    [Unbal_Acct_Override], [Unbal_Flow_Override], 
                    [Unbal_UD1_Override], [Unbal_UD2_Override], 
                    [Unbal_UD3_Override], [Unbal_UD4_Override], 
                    [Unbal_UD5_Override], [Unbal_UD6_Override], 
                    [Unbal_UD7_Override], [Unbal_UD8_Override], 
                    [Unbal_Src_Cell_Buffer_Filter], [Dyn_Calc_Script], 
                    [Override_Value], [Table_Calc_Expression], [Table_Join_Expression], 
                    [Table_Filter_Expression], [Map_Type], [Map_Source], 
                    [Map_Logic], [Src_SQL_Stmt], [Use_Temp_Table], [Temp_Table_Name], 
                    [Create_Date], [Create_User], [Update_Date], [Update_User]
                )
                VALUES
                (
                    @Cube_ID, @Act_ID, @Model_ID, @Calc_ID, @Cell_ID,
                    @Src_Order, @Src_Type, @Src_Item, 
                    @Open_Parens, @Math_Operator, @Entity, 
                    @Cons, @Scenario, @Time, @Origin, 
                    @IC, @View, @Src_Plan_Units, @Acct, 
                    @Flow, @UD1, @UD2, @UD3, 
                    @UD4, @UD5, @UD6, @UD7, 
                    @UD8, @Close_Parens, @Unbal_Src_Cell_Buffer, 
                    @Unbal_Origin_Override, @Unbal_IC_Override, 
                    @Unbal_Acct_Override, @Unbal_Flow_Override, 
                    @Unbal_UD1_Override, @Unbal_UD2_Override, 
                    @Unbal_UD3_Override, @Unbal_UD4_Override, 
                    @Unbal_UD5_Override, @Unbal_UD6_Override, 
                    @Unbal_UD7_Override, @Unbal_UD8_Override, 
                    @Unbal_Src_Cell_Buffer_Filter, @Dyn_Calc_Script, 
                    @Override_Value, @Table_Calc_Expression, @Table_Join_Expression, 
                    @Table_Filter_Expression, @Map_Type, @Map_Source, 
                    @Map_Logic, @Src_SQL_Stmt, @Use_Temp_Table, @Temp_Table_Name, 
                    @Create_Date, @Create_User, @Update_Date, @Update_User
                )";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

                // Add parameters to the insert command
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.InsertCommand.Parameters.Add("@Calc_ID", SqlDbType.Int).SourceColumn = "Calc_ID";
                sqa.InsertCommand.Parameters.Add("@Cell_ID", SqlDbType.Int).SourceColumn = "Cell_ID";
                sqa.InsertCommand.Parameters.Add("@Src_Order", SqlDbType.Int).SourceColumn = "Src_Order";
                sqa.InsertCommand.Parameters.Add("@Src_Type", SqlDbType.NVarChar, 20).SourceColumn = "Src_Type";
                sqa.InsertCommand.Parameters.Add("@Src_Item", SqlDbType.NVarChar, 50).SourceColumn = "Src_Item";
                sqa.InsertCommand.Parameters.Add("@Open_Parens", SqlDbType.NVarChar, 10).SourceColumn = "Open_Parens";
                sqa.InsertCommand.Parameters.Add("@Math_Operator", SqlDbType.NVarChar, 10).SourceColumn = "Math_Operator";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@Cons", SqlDbType.NVarChar, 100).SourceColumn = "Cons";
                sqa.InsertCommand.Parameters.Add("@Scenario", SqlDbType.NVarChar, 100).SourceColumn = "Scenario";
                sqa.InsertCommand.Parameters.Add("@Time", SqlDbType.NVarChar, 100).SourceColumn = "Time";
                sqa.InsertCommand.Parameters.Add("@Origin", SqlDbType.NVarChar, 100).SourceColumn = "Origin";
                sqa.InsertCommand.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC";
                sqa.InsertCommand.Parameters.Add("@View", SqlDbType.NVarChar, 100).SourceColumn = "View";
                sqa.InsertCommand.Parameters.Add("@Src_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Src_Plan_Units";
                sqa.InsertCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.InsertCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                sqa.InsertCommand.Parameters.Add("@Close_Parens", SqlDbType.NVarChar, 10).SourceColumn = "Close_Parens";
                sqa.InsertCommand.Parameters.Add("@Unbal_Src_Cell_Buffer", SqlDbType.NVarChar, 500).SourceColumn = "Unbal_Src_Cell_Buffer";
                sqa.InsertCommand.Parameters.Add("@Unbal_Origin_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Origin_Override";
                sqa.InsertCommand.Parameters.Add("@Unbal_IC_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_IC_Override";
                sqa.InsertCommand.Parameters.Add("@Unbal_Acct_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Acct_Override";
                sqa.InsertCommand.Parameters.Add("@Unbal_Flow_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Flow_Override";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@Unbal_UD{i}_Override", SqlDbType.NVarChar, 200).SourceColumn = $"Unbal_UD{i}_Override";
                }
                sqa.InsertCommand.Parameters.Add("@Unbal_Src_Cell_Buffer_Filter", SqlDbType.NVarChar, 500).SourceColumn = "Unbal_Src_Cell_Buffer_Filter";
                sqa.InsertCommand.Parameters.Add("@Dyn_Calc_Script", SqlDbType.NVarChar, 500).SourceColumn = "Dyn_Calc_Script";
                sqa.InsertCommand.Parameters.Add("@Override_Value", SqlDbType.NVarChar, 200).SourceColumn = "Override_Value";
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
                SET [Cube_ID] = @Cube_ID, [Act_ID] = @Act_ID, [Model_ID] = @Model_ID, 
                    [Src_Order] = @Src_Order, [Src_Type] = @Src_Type,
                    [Src_Item] = @Src_Item, [Open_Parens] = @Open_Parens, 
                    [Math_Operator] = @Math_Operator, [Entity] = @Entity, 
                    [Cons] = @Cons, [Scenario] = @Scenario, 
                    [Time] = @Time, [Origin] = @Origin, 
                    [IC] = @IC, [View] = @View, 
                    [Src_Plan_Units] = @Src_Plan_Units, [Acct] = @Acct, 
                    [Flow] = @Flow, [UD1] = @UD1, 
                    [UD2] = @UD2, [UD3] = @UD3, 
                    [UD4] = @UD4, [UD5] = @UD5, 
                    [UD6] = @UD6, [UD7] = @UD7, 
                    [UD8] = @UD8, [Close_Parens] = @Close_Parens,
                    [Unbal_Src_Cell_Buffer] = @Unbal_Src_Cell_Buffer, 
                    [Unbal_Origin_Override] = @Unbal_Origin_Override,
                    [Unbal_IC_Override] = @Unbal_IC_Override, 
                    [Unbal_Acct_Override] = @Unbal_Acct_Override,
                    [Unbal_Flow_Override] = @Unbal_Flow_Override, 
                    [Unbal_UD1_Override] = @Unbal_UD1_Override,
                    [Unbal_UD2_Override] = @Unbal_UD2_Override, 
                    [Unbal_UD3_Override] = @Unbal_UD3_Override,
                    [Unbal_UD4_Override] = @Unbal_UD4_Override, 
                    [Unbal_UD5_Override] = @Unbal_UD5_Override,
                    [Unbal_UD6_Override] = @Unbal_UD6_Override, 
                    [Unbal_UD7_Override] = @Unbal_UD7_Override,
                    [Unbal_UD8_Override] = @Unbal_UD8_Override, 
                    [Unbal_Src_Cell_Buffer_Filter] = @Unbal_Src_Cell_Buffer_Filter,
                    [Dyn_Calc_Script] = @Dyn_Calc_Script, 
                    [Override_Value] = @Override_Value, 
                    [Table_Calc_Expression] = @Table_Calc_Expression, 
                    [Table_Join_Expression] = @Table_Join_Expression, 
                    [Table_Filter_Expression] = @Table_Filter_Expression, 
                    [Map_Type] = @Map_Type, [Map_Source] = @Map_Source,
                    [Map_Logic] = @Map_Logic, [Src_SQL_Stmt] = @Src_SQL_Stmt, 
                    [Use_Temp_Table] = @Use_Temp_Table, [Temp_Table_Name] = @Temp_Table_Name, 
                    [Update_Date] = @Update_Date, [Update_User] = @Update_User
                WHERE Cell_ID = @Cell_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

                // Add parameters for the update command
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Cell_ID", SqlDbType.Int) { SourceColumn = "Cell_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.UpdateCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.UpdateCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.UpdateCommand.Parameters.Add("@Src_Order", SqlDbType.Int).SourceColumn = "Src_Order";
                sqa.UpdateCommand.Parameters.Add("@Src_Type", SqlDbType.NVarChar, 20).SourceColumn = "Src_Type";
                sqa.UpdateCommand.Parameters.Add("@Src_Item", SqlDbType.NVarChar, 50).SourceColumn = "Src_Item";
                sqa.UpdateCommand.Parameters.Add("@Open_Parens", SqlDbType.NVarChar, 10).SourceColumn = "Open_Parens";
                sqa.UpdateCommand.Parameters.Add("@Math_Operator", SqlDbType.NVarChar, 10).SourceColumn = "Math_Operator";
                sqa.UpdateCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.UpdateCommand.Parameters.Add("@Cons", SqlDbType.NVarChar, 100).SourceColumn = "Cons";
                sqa.UpdateCommand.Parameters.Add("@Scenario", SqlDbType.NVarChar, 100).SourceColumn = "Scenario";
                sqa.UpdateCommand.Parameters.Add("@Time", SqlDbType.NVarChar, 100).SourceColumn = "Time";
                sqa.UpdateCommand.Parameters.Add("@Origin", SqlDbType.NVarChar, 100).SourceColumn = "Origin";
                sqa.UpdateCommand.Parameters.Add("@IC", SqlDbType.NVarChar, 100).SourceColumn = "IC";
                sqa.UpdateCommand.Parameters.Add("@View", SqlDbType.NVarChar, 100).SourceColumn = "View";
                sqa.UpdateCommand.Parameters.Add("@Src_Plan_Units", SqlDbType.NVarChar, 250).SourceColumn = "Src_Plan_Units";
                sqa.UpdateCommand.Parameters.Add("@Acct", SqlDbType.NVarChar, 100).SourceColumn = "Acct";
                sqa.UpdateCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                sqa.UpdateCommand.Parameters.Add("@Close_Parens", SqlDbType.NVarChar, 10).SourceColumn = "Close_Parens";
                sqa.UpdateCommand.Parameters.Add("@Unbal_Src_Cell_Buffer", SqlDbType.NVarChar, 500).SourceColumn = "Unbal_Src_Cell_Buffer";
                sqa.UpdateCommand.Parameters.Add("@Unbal_Origin_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Origin_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbal_IC_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_IC_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbal_Acct_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Acct_Override";
                sqa.UpdateCommand.Parameters.Add("@Unbal_Flow_Override", SqlDbType.NVarChar, 200).SourceColumn = "Unbal_Flow_Override";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.UpdateCommand.Parameters.Add($"@Unbal_UD{i}_Override", SqlDbType.NVarChar, 200).SourceColumn = $"Unbal_UD{i}_Override";
                }
                sqa.UpdateCommand.Parameters.Add("@Unbal_Src_Cell_Buffer_Filter", SqlDbType.NVarChar, 500).SourceColumn = "Unbal_Src_Cell_Buffer_Filter";
                sqa.UpdateCommand.Parameters.Add("@Dyn_Calc_Script", SqlDbType.NVarChar, 500).SourceColumn = "Dyn_Calc_Script";
                sqa.UpdateCommand.Parameters.Add("@Override_Value", SqlDbType.NVarChar, 200).SourceColumn = "Override_Value";
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
		            WHERE Cell_ID = @Cell_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Cell_ID", SqlDbType.Int) { SourceColumn = "Cell_ID", SourceVersion = DataRowVersion.Original });

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