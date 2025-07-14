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
    public class SQA_FMM_Calc_Config
    {
        private readonly SqlConnection _connection;

        public SQA_FMM_Calc_Config(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_FMM_Calc_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_FMM_Calc_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                    INSERT INTO FMM_Calc_Config (
                        Cube_ID, Activity_ID, Model_ID, Calc_ID, Name, Sequence, 
                        Calc_Condition, Calc_Explanation, Time_Phasing, Input_Frequency, 
                        MultiDim_Alloc, MbrList_Calc, MbrList_1_Dim, MbrList_1_Filter, 
                        MbrList_1_DimType, MbrList_1_Filter, MbrList_2_Dim, 
                        MbrList_2_Filter, MbrList_2_DimType, MbrList_2_Filter, 
                        MbrList_3_Dim, MbrList_3_Filter, MbrList_3_DimType, 
                        MbrList_3_Filter, MbrList_4_Dim, MbrList_4_Filter, 
                        MbrList_4_DimType, MbrList_4_Filter, BR_Calc, 
                        BR_Calc_Name, Balanced_Buffer, bal_buffer_calc, 
                        Unbal_Calc, Table_Calc_SQL_Logic, Status, Create_Date, Create_User, 
                        Update_Date, Update_User
                    ) VALUES (
                        @Cube_ID, @Activity_ID, @Model_ID, @Calc_ID, @Name, @Sequence, 
                        @Calc_Condition, @Calc_Explanation, @Time_Phasing, @Input_Frequency, 
                        @MultiDim_Alloc, @MbrList_Calc, @MbrList_1_Dim, @MbrList_1_Filter, 
                        @MbrList_1_DimType, @MbrList_1_Filter, @MbrList_2_Dim, 
                        @MbrList_2_Filter, @MbrList_2_DimType, @MbrList_2_Filter, 
                        @MbrList_3_Dim, @MbrList_3_Filter, @MbrList_3_DimType, 
                        @MbrList_3_Filter, @MbrList_4_Dim, @MbrList_4_Filter, 
                        @MbrList_4_DimType, @MbrList_4_Filter, @BR_Calc, 
                        @BR_Calc_Name, @Balanced_Buffer, @bal_buffer_calc, 
                        @Unbal_Calc, @Table_Calc_SQL_Logic, @Status, @Create_Date, 
                        @Create_User, @Update_Date, @Update_User
                    )";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);

                // Add parameters for the insert command
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Activity_ID", SqlDbType.Int).SourceColumn = "Activity_ID";
                sqa.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                sqa.InsertCommand.Parameters.Add("@Calc_ID", SqlDbType.Int).SourceColumn = "Calc_ID";
                sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.InsertCommand.Parameters.Add("@Sequence", SqlDbType.Int).SourceColumn = "Sequence";
                sqa.InsertCommand.Parameters.Add("@Calc_Condition", SqlDbType.NVarChar, 100).SourceColumn = "Calc_Condition";
                sqa.InsertCommand.Parameters.Add("@Calc_Explanation", SqlDbType.NVarChar, 2000).SourceColumn = "Calc_Explanation";
                sqa.InsertCommand.Parameters.Add("@Time_Phasing", SqlDbType.NVarChar, 20).SourceColumn = "Time_Phasing";
                sqa.InsertCommand.Parameters.Add("@Input_Frequency", SqlDbType.NVarChar, 20).SourceColumn = "Input_Frequency";
                sqa.InsertCommand.Parameters.Add("@MultiDim_Alloc", SqlDbType.Bit).SourceColumn = "MultiDim_Alloc";
                sqa.InsertCommand.Parameters.Add("@MbrList_Calc", SqlDbType.Bit).SourceColumn = "MbrList_Calc";
                sqa.InsertCommand.Parameters.Add("@MbrList_1_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_1_Dim";
                sqa.InsertCommand.Parameters.Add("@MbrList_1_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_1_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_1_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_1_DimType";
                sqa.InsertCommand.Parameters.Add("@MbrList_1_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_1_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_2_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_2_Dim";
                sqa.InsertCommand.Parameters.Add("@MbrList_2_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_2_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_2_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_2_DimType";
                sqa.InsertCommand.Parameters.Add("@MbrList_2_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_2_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_3_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_3_Dim";
                sqa.InsertCommand.Parameters.Add("@MbrList_3_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_3_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_3_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_3_DimType";
                sqa.InsertCommand.Parameters.Add("@MbrList_3_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_3_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_4_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_4_Dim";
                sqa.InsertCommand.Parameters.Add("@MbrList_4_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_4_Filter";
                sqa.InsertCommand.Parameters.Add("@MbrList_4_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_4_DimType";
                sqa.InsertCommand.Parameters.Add("@MbrList_4_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_4_Filter";
                sqa.InsertCommand.Parameters.Add("@BR_Calc", SqlDbType.Bit).SourceColumn = "BR_Calc";
                sqa.InsertCommand.Parameters.Add("@BR_Calc_Name", SqlDbType.NVarChar, 100).SourceColumn = "BR_Calc_Name";
                sqa.InsertCommand.Parameters.Add("@Balanced_Buffer", SqlDbType.NVarChar, 20).SourceColumn = "Balanced_Buffer";
                sqa.InsertCommand.Parameters.Add("@bal_buffer_calc", SqlDbType.NVarChar, 2000).SourceColumn = "bal_buffer_calc";
                sqa.InsertCommand.Parameters.Add("@Unbal_Calc", SqlDbType.NVarChar, 2000).SourceColumn = "Unbal_Calc";
                sqa.InsertCommand.Parameters.Add("@Table_Calc_SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "Table_Calc_SQL_Logic";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the update query and parameters
                string updateQuery = @"
                    UPDATE FMM_Calc_Config SET
                        Name = @Name, Sequence = @Sequence, 
                        Calc_Condition = @Calc_Condition, Calc_Explanation = @Calc_Explanation, 
                        Time_Phasing = @Time_Phasing, Input_Frequency = @Input_Frequency, 
                        MultiDim_Alloc = @MultiDim_Alloc, MbrList_Calc = @MbrList_Calc, 
                        MbrList_1_Dim = @MbrList_1_Dim, MbrList_1_Filter = @MbrList_1_Filter, 
                        MbrList_1_DimType = @MbrList_1_DimType, MbrList_1_Filter = @MbrList_1_Filter,
                        MbrList_2_Dim = @MbrList_2_Dim, MbrList_2_Filter = @MbrList_2_Filter, 
                        MbrList_2_DimType = @MbrList_2_DimType, MbrList_2_Filter = @MbrList_2_Filter,
                        MbrList_3_Dim = @MbrList_3_Dim, MbrList_3_Filter = @MbrList_3_Filter, 
                        MbrList_3_DimType = @MbrList_3_DimType, MbrList_3_Filter = @MbrList_3_Filter,
                        MbrList_4_Dim = @MbrList_4_Dim, MbrList_4_Filter = @MbrList_4_Filter, 
                        MbrList_4_DimType = @MbrList_4_DimType, MbrList_4_Filter = @MbrList_4_Filter,
                        BR_Calc = @BR_Calc, BR_Calc_Name = @BR_Calc_Name, 
                        Balanced_Buffer = @Balanced_Buffer, bal_buffer_calc = @bal_buffer_calc, 
                        Unbal_Calc = @Unbal_Calc, Table_Calc_SQL_Logic = @Table_Calc_SQL_Logic, 
                        Status = @Status, Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE Calc_ID = @Calc_ID";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);

                // Add parameters for the update command
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Calc_ID", SqlDbType.Int) { SourceColumn = "Calc_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.UpdateCommand.Parameters.Add("@Sequence", SqlDbType.Int).SourceColumn = "Sequence";
                sqa.UpdateCommand.Parameters.Add("@Calc_Condition", SqlDbType.NVarChar, 100).SourceColumn = "Calc_Condition";
                sqa.UpdateCommand.Parameters.Add("@Calc_Explanation", SqlDbType.NVarChar, 2000).SourceColumn = "Calc_Explanation";
                sqa.UpdateCommand.Parameters.Add("@Time_Phasing", SqlDbType.NVarChar, 20).SourceColumn = "Time_Phasing";
                sqa.UpdateCommand.Parameters.Add("@Input_Frequency", SqlDbType.NVarChar, 20).SourceColumn = "Input_Frequency";
                sqa.UpdateCommand.Parameters.Add("@MultiDim_Alloc", SqlDbType.Bit).SourceColumn = "MultiDim_Alloc";
                sqa.UpdateCommand.Parameters.Add("@MbrList_Calc", SqlDbType.Bit).SourceColumn = "MbrList_Calc";
                sqa.UpdateCommand.Parameters.Add("@MbrList_1_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_1_Dim";
                sqa.UpdateCommand.Parameters.Add("@MbrList_1_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_1_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_1_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_1_DimType";
                sqa.UpdateCommand.Parameters.Add("@MbrList_1_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_1_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_2_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_2_Dim";
                sqa.UpdateCommand.Parameters.Add("@MbrList_2_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_2_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_2_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_2_DimType";
                sqa.UpdateCommand.Parameters.Add("@MbrList_2_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_2_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_3_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_3_Dim";
                sqa.UpdateCommand.Parameters.Add("@MbrList_3_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_3_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_3_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_3_DimType";
                sqa.UpdateCommand.Parameters.Add("@MbrList_3_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_3_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_4_Dim", SqlDbType.NVarChar, 100).SourceColumn = "MbrList_4_Dim";
                sqa.UpdateCommand.Parameters.Add("@MbrList_4_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_4_Filter";
                sqa.UpdateCommand.Parameters.Add("@MbrList_4_DimType", SqlDbType.NVarChar, 20).SourceColumn = "MbrList_4_DimType";
                sqa.UpdateCommand.Parameters.Add("@MbrList_4_Filter", SqlDbType.NVarChar, 200).SourceColumn = "MbrList_4_Filter";
                sqa.UpdateCommand.Parameters.Add("@BR_Calc", SqlDbType.Bit).SourceColumn = "BR_Calc";
                sqa.UpdateCommand.Parameters.Add("@BR_Calc_Name", SqlDbType.NVarChar, 100).SourceColumn = "BR_Calc_Name";
                sqa.UpdateCommand.Parameters.Add("@Balanced_Buffer", SqlDbType.NVarChar, 20).SourceColumn = "Balanced_Buffer";
                sqa.UpdateCommand.Parameters.Add("@bal_buffer_calc", SqlDbType.NVarChar, 2000).SourceColumn = "bal_buffer_calc";
                sqa.UpdateCommand.Parameters.Add("@Unbal_Calc", SqlDbType.NVarChar, 2000).SourceColumn = "Unbal_Calc";
                sqa.UpdateCommand.Parameters.Add("@Table_Calc_SQL_Logic", SqlDbType.NVarChar, 2000).SourceColumn = "Table_Calc_SQL_Logic";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 20).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
                    DELETE FROM FMM_Calc_Config 
                    WHERE Calc_ID = @Calc_ID";

                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Calc_ID", SqlDbType.Int) { SourceColumn = "Calc_ID", SourceVersion = DataRowVersion.Original });

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