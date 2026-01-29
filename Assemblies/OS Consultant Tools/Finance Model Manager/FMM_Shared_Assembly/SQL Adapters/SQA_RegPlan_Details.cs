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
    public class SQA_RegPlan_Details
    {
        private readonly SqlConnection _connection;

        public SQA_RegPlan_Details(SessionInfo si, SqlConnection connection)
        {
            _connection = connection;
        }

        public void Fill_RegPlan_Details_DT(SessionInfo si, SqlDataAdapter sqa, DataTable dt, string sql, params SqlParameter[] sqlparams)
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
            }
        }

        public void Update_RegPlan_Details(SessionInfo si, DataTable dt, SqlDataAdapter sqa)
        {
            using (SqlTransaction transaction = _connection.BeginTransaction())
            {
                var insertQuery = @"INSERT INTO RegPlan_Details (
                        RegPlanID, WFScenarioName, WFProfileName, WFTimeName, ActID, ModelID, Entity, ApprLevelID, 
                        PlanUnits, Account, Flow, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8, Year, 
                        Month1, Month2, Month3, Month4, Month5, Month6, Month7, Month8, Month9, Month10, 
                        Month11, Month12, Quarter1, Quarter2, Quarter3, Quarter4, Yearly, AllowUpdate, CreateDate, CreateUser, UpdateDate, UpdateUser
                    ) VALUES (
                        @RegPlanID, @WFScenarioName, @WFProfileName, @WFTimeName, @ActID, @ModelID, @Entity, @ApprLevelID, 
                        @PlanUnits, @Account, @Flow, @UD1, @UD2, @UD3, @UD4, @UD5, @UD6, @UD7, @UD8, @Year, 
                        @Month1, @Month2, @Month3, @Month4, @Month5, @Month6, @Month7, @Month8, @Month9, @Month10, 
                        @Month11, @Month12, @Quarter1, @Quarter2, @Quarter3, @Quarter4, @Yearly, @AllowUpdate, @CreateDate, @CreateUser, @UpdateDate, @UpdateUser)";

                sqa.InsertCommand = new SqlCommand(insertQuery, _connection, transaction);
                sqa.InsertCommand.Parameters.Add("@RegPlanID", SqlDbType.UniqueIdentifier).SourceColumn = "RegPlanID";
                sqa.InsertCommand.Parameters.Add("@WFScenarioName", SqlDbType.NVarChar, 100).SourceColumn = "WFScenarioName";
                sqa.InsertCommand.Parameters.Add("@WFProfileName", SqlDbType.NVarChar, 100).SourceColumn = "WFProfileName";
                sqa.InsertCommand.Parameters.Add("@WFTimeName", SqlDbType.NVarChar, 100).SourceColumn = "WFTimeName";
                sqa.InsertCommand.Parameters.Add("@ActID", SqlDbType.Int).SourceColumn = "ActID";
                sqa.InsertCommand.Parameters.Add("@ModelID", SqlDbType.Int).SourceColumn = "ModelID";
                sqa.InsertCommand.Parameters.Add("@Entity", SqlDbType.NVarChar, 100).SourceColumn = "Entity";
                sqa.InsertCommand.Parameters.Add("@ApprLevelID", SqlDbType.UniqueIdentifier).SourceColumn = "ApprLevelID";
                sqa.InsertCommand.Parameters.Add("@PlanUnits", SqlDbType.NVarChar, 20).SourceColumn = "PlanUnits";
                sqa.InsertCommand.Parameters.Add("@Account", SqlDbType.NVarChar, 20).SourceColumn = "Account";
                sqa.InsertCommand.Parameters.Add("@Flow", SqlDbType.NVarChar, 100).SourceColumn = "Flow";
                for (int i = 1; i <= 8; i++)
                {
                    sqa.InsertCommand.Parameters.Add($"@UD{i}", SqlDbType.NVarChar, 100).SourceColumn = $"UD{i}";
                }
                sqa.InsertCommand.Parameters.Add(new SqlParameter("@Year", SqlDbType.NVarChar, 4) { SourceColumn = "Year" });
                for (int i = 1; i <= 12; i++)
                {
                    sqa.InsertCommand.Parameters.Add(new SqlParameter($"@Month{i}", SqlDbType.Decimal) { SourceColumn = $"Month{i}" });
                }
                for (int i = 1; i <= 4; i++)
                {
                    sqa.InsertCommand.Parameters.Add(new SqlParameter($"@Quarter{i}", SqlDbType.Decimal) { SourceColumn = $"Quarter{i}" });
                }
                sqa.InsertCommand.Parameters.Add(new SqlParameter("@Yearly", SqlDbType.Decimal) { SourceColumn = "Yearly" });
                sqa.InsertCommand.Parameters.Add(new SqlParameter("@AllowUpdate", SqlDbType.Bit) { SourceColumn = "AllowUpdate" });
                sqa.InsertCommand.Parameters.Add(new SqlParameter("@CreateUser", SqlDbType.NVarChar, 50) { Value = si.UserName });
                sqa.InsertCommand.Parameters.Add(new SqlParameter("@UpdateUser", SqlDbType.NVarChar, 50) { Value = si.UserName });

                var updateQuery = @"UPDATE RegPlan_Details SET
                        WFScenarioName = @WFScenarioName, WFProfileName = @WFProfileName, WFTimeName = @WFTimeName, ActID = @ActID, 
                        ModelID = @ModelID, Entity = @Entity, ApprLevelID = @ApprLevelID, PlanUnits = @PlanUnits, 
                        Account = @Account, Flow = @Flow, UD1 = @UD1, UD2 = @UD2, UD3 = @UD3, UD4 = @UD4, UD5 = @UD5, 
                        UD6 = @UD6, UD7 = @UD7, UD8 = @UD8, Year = @Year, Month1 = @Month1, Month2 = @Month2, 
                        Month3 = @Month3, Month4 = @Month4, Month5 = @Month5, Month6 = @Month6, Month7 = @Month7, Month8 = @Month8, 
                        Month9 = @Month9, Month10 = @Month10, Month11 = @Month11, Month12 = @Month12, Quarter1 = @Quarter1, 
                        Quarter2 = @Quarter2, Quarter3 = @Quarter3, Quarter4 = @Quarter4, Yearly = @Yearly, AllowUpdate = @AllowUpdate, 
                        UpdateDate = @UpdateDate, UpdateUser = @UpdateUser
                    WHERE RegPlanID = @RegPlanID AND Year = @Year AND PlanUnits = @PlanUnits AND Account = @Account";

                sqa.UpdateCommand = new SqlCommand(updateQuery, _connection, transaction);
                //AddUpdateParameters(sqa.UpdateCommand);

                var deleteQuery = "DELETE FROM RegPlan_Details WHERE RegPlanID = @RegPlanID AND Year = @Year AND PlanUnits = @PlanUnits AND Account = @Account";
                sqa.DeleteCommand = new SqlCommand(deleteQuery, _connection, transaction);
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@RegPlanID", SqlDbType.UniqueIdentifier) { SourceColumn = "RegPlanID", SourceVersion = DataRowVersion.Original });
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Year", SqlDbType.NVarChar, 4) { SourceColumn = "Year", SourceVersion = DataRowVersion.Original });
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@PlanUnits", SqlDbType.NVarChar, 20) { SourceColumn = "PlanUnits", SourceVersion = DataRowVersion.Original });
                sqa.DeleteCommand.Parameters.Add(new SqlParameter("@Account", SqlDbType.NVarChar, 20) { SourceColumn = "Account", SourceVersion = DataRowVersion.Original });

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