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
    public class SQA_Reg_Plan_Details
    {
        private readonly SqlConnection _connection;

        public SQA_Reg_Plan_Details(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_Register_Plan_Details_DataTable(SessionInfo si, SqlDataAdapter adapter, DataTable dataTable, string selectQuery, params SqlParameter[] parameters)
        {
            using (SqlCommand command = new SqlCommand(selectQuery, _connection))
            {
                command.CommandType = CommandType.Text;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }
                adapter.SelectCommand = command;
                adapter.Fill(dataTable);
            }
        }

        public void Update_Register_Plan_Details(SessionInfo si, DataTable dataTable, SqlDataAdapter adapter)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                string insertQuery = @"
                    INSERT INTO Register_Plan_Details (
                        Register_Plan_ID, WF_Scenario_Name, WF_Profile_Name, Activity_ID, Model_ID, Entity, Approval_Level_ID, 
                        Plan_Units, Account, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Year, 
                        Month1, Month2, Month3, Month4, Month5, Month6, Month7, Month8, Month9, Month10, 
                        Month11, Month12, Quarter1, Quarter2, Quarter3, Quarter4, Yearly, AllowUpdate, Create_Date, Create_User, Update_Date, Update_User
                    ) VALUES (
                        @Register_Plan_ID, @WF_Scenario_Name, @WF_Profile_Name, @Activity_ID, @Model_ID, @Entity, @Approval_Level_ID, 
                        @Plan_Units, @Account, @Flow, @UD1, @UD2, @UD3, @UD4, @UD5, @UD6, @UD7, @UD8, @Year, 
                        @Month1, @Month2, @Month3, @Month4, @Month5, @Month6, @Month7, @Month8, @Month9, @Month10, 
                        @Month11, @Month12, @Quarter1, @Quarter2, @Quarter3, @Quarter4, @Yearly, @AllowUpdate, @Create_Date, @Create_User, @Update_Date, @Update_User
                    )";

                adapter.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                adapter.InsertCommand.Parameters.Add("@Register_Plan_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Register_Plan_ID";
                adapter.InsertCommand.Parameters.Add("@WF_Scenario_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Scenario_Name";
                adapter.InsertCommand.Parameters.Add("@WF_Profile_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Profile_Name";
                adapter.InsertCommand.Parameters.Add("@WF_Time_Name", SqlDbType.NVarChar, 100).SourceColumn = "WF_Time_Name";
                adapter.InsertCommand.Parameters.Add("@Activity_ID", SqlDbType.Int).SourceColumn = "Activity_ID";
                adapter.InsertCommand.Parameters.Add("@Model_ID", SqlDbType.Int).SourceColumn = "Model_ID";
                adapter.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                adapter.InsertCommand.Parameters.Add("@Approval_Level_ID", SqlDbType.UniqueIdentifier).SourceColumn = "Approval_Level_ID";
                adapter.InsertCommand.Parameters.Add("@Plan_Units", SqlDbType.NVarChar, 20).SourceColumn = "Plan_Units";
                adapter.InsertCommand.Parameters.Add("@Account", SqlDbType.NVarChar, 20).SourceColumn = "Account";
                adapter.InsertCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    adapter.InsertCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Year", SqlDbType.NVarChar, 4) { SourceColumn = "Year" });
                for (int i = 1; i <= 12; i++)
                {
                    adapter.InsertCommand.Parameters.Add(new SqlParameter($"@Month{i}", SqlDbType.Decimal) { SourceColumn = $"Month{i}" });
                }
                for (int i = 1; i <= 4; i++)
                {
                    adapter.InsertCommand.Parameters.Add(new SqlParameter($"@Quarter{i}", SqlDbType.Decimal) { SourceColumn = $"Quarter{i}" });
                }
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Yearly", SqlDbType.Decimal) { SourceColumn = "Yearly" });
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@AllowUpdate", SqlDbType.Bit) { SourceColumn = "AllowUpdate" });
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Create_User", SqlDbType.NVarChar, 50) { Value = si.UserName });
                adapter.InsertCommand.Parameters.Add(new SqlParameter("@Update_User", SqlDbType.NVarChar, 50) { Value = si.UserName });

                string updateQuery = @"
                    UPDATE Register_Plan_Details SET
                        WF_Scenario_Name = @WF_Scenario_Name, WF_Profile_Name = @WF_Profile_Name, Activity_ID = @Activity_ID, 
                        Model_ID = @Model_ID, Entity = @Entity, Approval_Level_ID = @Approval_Level_ID, Plan_Units = @Plan_Units, 
                        Account = @Account, Flow = @Flow, UD1 = @UD1, UD2 = @UD2, UD3 = @UD3, UD4 = @UD4, UD5 = @UD5, 
                        UD6 = @UD6, UD7 = @UD7, UD8 = @UD8, Year = @Year, Month1 = @Month1, Month2 = @Month2, 
                        Month3 = @Month3, Month4 = @Month4, Month5 = @Month5, Month6 = @Month6, Month7 = @Month7, Month8 = @Month8, 
                        Month9 = @Month9, Month10 = @Month10, Month11 = @Month11, Month12 = @Month12, Quarter1 = @Quarter1, 
                        Quarter2 = @Quarter2, Quarter3 = @Quarter3, Quarter4 = @Quarter4, Yearly = @Yearly, AllowUpdate = @AllowUpdate, 
                        Update_Date = @Update_Date, Update_User = @Update_User
                    WHERE Register_Plan_ID = @Register_Plan_ID AND Year = @Year AND Plan_Units = @Plan_Units AND Account = @Account";

                adapter.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                //AddUpdateParameters(adapter.UpdateCommand);

                string deleteQuery = "DELETE FROM Register_Plan_Details WHERE Register_Plan_ID = @Register_Plan_ID AND Year = @Year AND Plan_Units = @Plan_Units AND Account = @Account";
                adapter.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Register_Plan_ID", SqlDbType.UniqueIdentifier) { SourceColumn = "Register_Plan_ID", SourceVersion = DataRowVersion.Original });
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Year", SqlDbType.NVarChar, 4) { SourceColumn = "Year", SourceVersion = DataRowVersion.Original });
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Plan_Units", SqlDbType.NVarChar, 20) { SourceColumn = "Plan_Units", SourceVersion = DataRowVersion.Original });
                adapter.DeleteCommand.Parameters.Add(new SqlParameter("@Account", SqlDbType.NVarChar, 20) { SourceColumn = "Account", SourceVersion = DataRowVersion.Original });

                try
                {
                    adapter.Update(dataTable);
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