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

using Workspace.OSConsTools.GBL_UI_Assembly;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class SQA_FMM_Reg_Config
    {
        private readonly SQA_GBL_Command_Builder _cmdBuilder;

        public SQA_FMM_Reg_Config(SessionInfo si, SqlConnection connection)
        {
            _cmdBuilder = new SQA_GBL_Command_Builder(si, connection);
        }

        public void Fill_FMM_Reg_Config_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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

        public void Update_FMM_Reg_Config(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            sqa.UpdateBatchSize = 0; // Set batch size for performance
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                // Define the insert query and parameters
                string insertQuery = @"
                    INSERT INTO FMM_Reg_Config (
                        Cube_ID, Act_ID, Reg_Config_ID, Name, Time_Phase, 
						Time_Phase_Driver,Manual_Input_Plan_Units,Start_End_Dt_Src_Obj,
                        Start_Dt_Src, End_Dt_Src, Appr_Config, Status, Create_Date, 
                        Create_User, Update_Date, Update_User
                    ) VALUES (
                        @Cube_ID, @Act_ID, @Reg_Config_ID, @Name, @Time_Phase, 
						@Time_Phase_Driver,@Manual_Input_Plan_Units,@Start_End_Dt_Src_Obj,
                        @Start_Dt_Src, @End_Dt_Src, @Appr_Config, @Status, @Create_Date, 
                        @Create_User, @Update_Date, @Update_User
                    )";
                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
				sqa.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                sqa.InsertCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.InsertCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.InsertCommand.Parameters.Add("@Reg_Config_ID", SqlDbType.Int).SourceColumn = "Reg_Config_ID";
                sqa.InsertCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.InsertCommand.Parameters.Add("@Time_Phase", SqlDbType.NVarChar, 20).SourceColumn = "Time_Phase";
                sqa.InsertCommand.Parameters.Add("@Time_Phase_Driver", SqlDbType.NVarChar, 50).SourceColumn = "Time_Phase_Driver";
                sqa.InsertCommand.Parameters.Add("@Manual_Input_Plan_Units", SqlDbType.NVarChar, 1000).SourceColumn = "Manual_Input_Plan_Units";
                sqa.InsertCommand.Parameters.Add("@Start_End_Dt_Src_Obj", SqlDbType.NVarChar, 250).SourceColumn = "Start_End_Dt_Src_Obj";
                sqa.InsertCommand.Parameters.Add("@Start_Dt_Src", SqlDbType.NVarChar, 100).SourceColumn = "Start_Dt_Src";
                sqa.InsertCommand.Parameters.Add("@End_Dt_Src", SqlDbType.NVarChar, 100).SourceColumn = "End_Dt_Src";
                sqa.InsertCommand.Parameters.Add("@Appr_Config", SqlDbType.Int).SourceColumn = "Appr_Config";
                sqa.InsertCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.InsertCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.InsertCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.InsertCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.InsertCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";
				
                // Define the update query and parameters
                string updateQuery = @"
                    UPDATE FMM_Reg_Config SET
                        Cube_ID = @Cube_ID, Act_ID = @Act_ID, Name = @Name, 
                        Time_Phase = @Time_Phase, Time_Phase_Driver = @Time_Phase_Driver,
                        Manual_Input_Plan_Units = @Manual_Input_Plan_Units,
						Start_End_Dt_Src_Obj = @Start_End_Dt_Src_Obj,Start_Dt_Src = @Start_Dt_Src, 
                        End_Dt_Src = @End_Dt_Src, Config = @Appr_Config, 
                        Status = @Status, Create_Date = @Create_Date, 
                        Create_User = @Create_User, Update_Date = @Update_Date, 
                        Update_User = @Update_User
                    WHERE Reg_Config_ID = @Reg_Config_ID";
                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
				sqa.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                sqa.UpdateCommand.Parameters.Add(new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { SourceColumn = "Reg_Config_ID", SourceVersion = DataRowVersion.Original });
                sqa.UpdateCommand.Parameters.Add("@Cube_ID", SqlDbType.Int).SourceColumn = "Cube_ID";
                sqa.UpdateCommand.Parameters.Add("@Act_ID", SqlDbType.Int).SourceColumn = "Act_ID";
                sqa.UpdateCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 100).SourceColumn = "Name";
                sqa.UpdateCommand.Parameters.Add("@Time_Phase", SqlDbType.NVarChar, 20).SourceColumn = "Time_Phase";
                sqa.UpdateCommand.Parameters.Add("@Time_Phase_Driver", SqlDbType.NVarChar, 50).SourceColumn = "Time_Phase_Driver";
                sqa.UpdateCommand.Parameters.Add("@Manual_Input_Plan_Units", SqlDbType.NVarChar, 1000).SourceColumn = "Manual_Input_Plan_Units";
                sqa.UpdateCommand.Parameters.Add("@Start_End_Dt_Src_Obj", SqlDbType.NVarChar, 250).SourceColumn = "Start_End_Dt_Src_Obj";
                sqa.UpdateCommand.Parameters.Add("@Start_Dt_Src", SqlDbType.NVarChar, 100).SourceColumn = "Start_Dt_Src";
                sqa.UpdateCommand.Parameters.Add("@End_Dt_Src", SqlDbType.NVarChar, 100).SourceColumn = "End_Dt_Src";
                sqa.UpdateCommand.Parameters.Add("@Appr_Config", SqlDbType.Int).SourceColumn = "Appr_Config";
                sqa.UpdateCommand.Parameters.Add("@Status", SqlDbType.NVarChar, 10).SourceColumn = "Status";
                sqa.UpdateCommand.Parameters.Add("@Create_Date", SqlDbType.DateTime).SourceColumn = "Create_Date";
                sqa.UpdateCommand.Parameters.Add("@Create_User", SqlDbType.NVarChar, 50).SourceColumn = "Create_User";
                sqa.UpdateCommand.Parameters.Add("@Update_Date", SqlDbType.DateTime).SourceColumn = "Update_Date";
                sqa.UpdateCommand.Parameters.Add("@Update_User", SqlDbType.NVarChar, 50).SourceColumn = "Update_User";

                // Define the delete query and parameters
                string deleteQuery = @"
                    DELETE FROM FMM_Reg_Config 
                    WHERE Reg_Config_ID = @Reg_Config_ID";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
				sqa.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Reg_Config_ID", SqlDbType.Int) { SourceColumn = "Reg_Config_ID", SourceVersion = DataRowVersion.Original });

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